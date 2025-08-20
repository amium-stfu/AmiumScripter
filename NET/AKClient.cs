using AmiumScripter.Core;
using AmiumScripter.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AmiumScripter.NET
{
    /// <summary>
    /// Sequenzieller AK-Client (eine Klasse, AClient-Ableitung)
    /// - Senden über Raw-Socket (TcpClient.Client.Send)
    /// - STX/ETX-Framing optional (default: an)
    /// - Asynchroner Receive mit OnReceived/MessageReceived
    /// - Sequencer-Queue: immer nur 1 Request outstanding (Verb-basiertes Matching)
    /// - Optional: synchrones Transceive für Anfrage/Antwort
    /// - Watchdog-Timeout wird bei jedem gültigen Frame zurückgesetzt
    /// </summary>
    public class AkClient : AClient
    {

        private sealed class ReaderGroup
        {
            public readonly List<AkReader> Readers = new();
            public System.Threading.Timer Timer;
            public int Interval;
        }

        private  class AkReader
        {
            public string Command { get; set; }
            public int Interval { get; set; }
            public List<string> Response = new();
            public DateTime LastResponse { get; set; } = DateTime.MinValue;
            public long NextDueTick { get; set; }
            public bool InFlight { get; set; }
            public AkReader(string command, int interval)
            {
                Command = command;
                Interval = interval;
            }
            public string GetString(int index) => (index < 0 || index >= Response.Count) ? "#" : Response[index];
            public double GetDouble(int index) => (index < 0 || index >= Response.Count) ? double.NaN : Response[index].ToDouble();
            public string GetStringList(char separator) => string.Join(separator, Response);
            public List<string> GetList => Response;
         
        }

        public string GetReaderResponse(string akRead, int index)
        {
            lock (_readersLock)
            {
                if (Readers.TryGetValue(akRead, out var reader))
                {
                    return reader.GetString(index);
                }
            }
            return "#";
        }

        public double GetReaderResponseDouble(string akRead, int index)
        {
            lock (_readersLock)
            {
                if (Readers.TryGetValue(akRead, out var reader))
                {
                    Debug.WriteLine("Read found: " + akRead + " index: " + index);
                    return reader.GetDouble(index);

                }
            }
            return double.NaN;
        }

        public string GetReaderResponseList(string akRead, char separator = ',')
        {
            lock (_readersLock)
            {
                if (Readers.TryGetValue(akRead, out var reader))
                {
                    return reader.GetStringList(separator);
                }
            }
            return string.Empty;
        }

        private readonly object _readerGroupsLock = new();
        private readonly Dictionary<int, ReaderGroup> _readerGroups = new(); // key = Interval (ms)


        // Threads
        private Thread _workerThread;
        private Thread _sequencerThread;

        // IO
        private TcpClient _tcpClient;
        private readonly object _connLock = new object();
        private System.Timers.Timer _timer; // Watchdog, wird bei RX-Frame resettet

        // Config
        private readonly string _host;
        private readonly int _port;
        private readonly int _timeoutMs;

        // State
        private volatile bool _shouldStop = false;
        private volatile bool _exclusiveIo = false; // blockiert WorkLoop während Transceive
        private readonly object _ioLock = new object();

        public bool Connected => _tcpClient != null && _tcpClient.Connected;

        // RX-Framebuffer (asynchroner WorkLoop)
        private readonly List<byte> _resp = new List<byte>();

        // Sequencer
        private class CmdItem
        {
            public string Text;
            public int TimeoutMs;
            public bool WithStxEtx;
            public bool AppendCrlf;
            public string ExpectedVerb;     // z. B. "AKON"
            public string ExpectedChannel;  // z. B. "K0"
            public Action<string> Callback; // optional
        }

        private readonly Queue<CmdItem> _queue = new Queue<CmdItem>();
        private readonly AutoResetEvent _queueEvt = new AutoResetEvent(false);
        private bool _inFlight = false;
        private string _expectedVerb = null;
        private string _expectedChannel = null;
        private long _inFlightDeadline = 0; // NICHT volatile (C#-Regel)
        private Action<string> _currentCallback = null;

        // Events / API
        public event Action<AkClient, string> MessageReceived;
        public event Action<string> OnReceived;
        public event Action<string, string> OnMatchedResponse; // (expectedVerb, response)
        public event Action<AkClient, AkMessage> MessageParsed; // geparste Variante

        private Dictionary<string,AkReader> Readers { get; } = new Dictionary<string, AkReader>();

        public string Received { get; private set; } = string.Empty;

        public AkClient(string name, string host, int port, int timeoutMs)
            : base(name)
        {
            _host = host;
            _port = port;
            _timeoutMs = timeoutMs;
            Initialize();
        }

        private readonly object _readersLock = new();
        public void AddReader(string command, int interval)
        {
            if (string.IsNullOrWhiteSpace(command)) throw new ArgumentNullException(nameof(command));
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));

            var reader = new AkReader(command, interval)
            {
                NextDueTick = Environment.TickCount64, // initial sofort
                InFlight = false
            };

            lock (_readerGroupsLock)
            {
                if (!_readerGroups.TryGetValue(interval, out var group))
                {
                    group = new ReaderGroup { Interval = interval };
                    group.Timer = new System.Threading.Timer(GroupTimerCallback, group, 0, interval);
                    _readerGroups[interval] = group;
                }
                group.Readers.Add(reader);
                lock (_readersLock)
                {
                    Readers[command] = reader; // weiter zentrale Map lassen (für Response-Zuordnung)
                }
            }
        }
        public bool RemoveReader(string command)
        {
            AkReader r;
            lock (_readersLock)
            {
                if (!Readers.TryGetValue(command, out r)) return false;
                Readers.Remove(command);
            }

            bool removed = false;
            lock (_readerGroupsLock)
            {
                if (_readerGroups.TryGetValue(r.Interval, out var group))
                {
                    group.Readers.Remove(r);
                    removed = true;
                    if (group.Readers.Count == 0)
                    {
                        group.Timer.Dispose();
                        _readerGroups.Remove(r.Interval);
                    }
                }
            }
            return removed;
        }
        public void ClearReaders()
        {
            lock (_readerGroupsLock)
            {
                foreach (var g in _readerGroups.Values)
                    g.Timer.Dispose();
                _readerGroups.Clear();
            }
            lock (_readersLock)
                Readers.Clear();
        }
        private void GroupTimerCallback(object state)
        {
            if (_shouldStop) return;
            var group = (ReaderGroup)state;

            List<AkReader> snapshot;
            lock (_readerGroupsLock)
            {
                // Gruppe könnte schon disposed sein
                if (!_readerGroups.ContainsKey(group.Interval)) return;
                snapshot = group.Readers.ToList();
            }

            long now = Environment.TickCount64;

            foreach (var r in snapshot)
            {
                // Reentrancy / InFlight respektieren
                if (r.InFlight) continue;

                // optional: einfache Drift-Korrektur
                if (now < r.NextDueTick - 5) continue;

                r.InFlight = true;
                r.NextDueTick = now + r.Interval;
                // Sequencer kümmert sich um Serialisierung
                Transmit(r.Command, true, true);
            }
        }
        public void WriteReaderResponse(string akRead, List<string> response)
        {
            //Debug.WriteLine("write '" + akRead + "'");
            //Debug.WriteLine("Reader registed " + Readers.Count);

          //  foreach (string r in Readers.Keys) Debug.WriteLine(r);

            lock (_readersLock)
            {
                if (Readers.TryGetValue(akRead, out var reader))
                {
                   // Debug.WriteLine("reader found");
                    reader.Response = response;
                    reader.LastResponse = DateTime.Now;
                    reader.InFlight = false; // freigeben für nächste Abfrage
                }
                else
                {
                  //  Debug.WriteLine("reader not found");
                }
            }
        }

        public override void Initialize()
        {
            _timer = new System.Timers.Timer(_timeoutMs)
            {
                AutoReset = false, // One-shot; wird bei RX neu gestartet
                Enabled = false
            };
            _timer.Elapsed += (s, e) => OnTimeout();
        }

        public override void Run()
        {
            _shouldStop = false;
            _workerThread = new Thread(WorkLoop) { IsBackground = true };
            _workerThread.Start();
            _sequencerThread = new Thread(SequencerLoop) { IsBackground = true };
            _sequencerThread.Start();
         //   _readerTimer = new System.Threading.Timer(_readerTimer_Elapsed, null, 0, 10);
        }

        public override void Destroy()
        {
            _shouldStop = true;
            _queueEvt.Set();
            try { _workerThread?.Join(500); } catch { }
            try { _sequencerThread?.Join(500); } catch { }
            Disconnect();
            try { _timer?.Stop(); _timer?.Dispose(); } catch { }

            // Neu: Gruppen-Timer freigeben
            lock (_readerGroupsLock)
            {
                foreach (var g in _readerGroups.Values)
                    g.Timer.Dispose();
                _readerGroups.Clear();
            }
        }

        private void OnTimeout()
        {
            Logger.WarningMsg($"[AKClient] {Name} timeout – disconnect");
            Disconnect();
        }

        public void Disconnect()
        {
            lock (_connLock)
            {
                try
                {
                    var tc = _tcpClient; // lokale Kopie gegen Race
                    _tcpClient = null;   // erst Feld kappen, dann schließen
                    try { _timer?.Stop(); } catch { }
                    if (tc != null)
                    {
                        try { tc.Close(); } catch { }
                    }
                    Logger.DebugMsg($"[AKClient] {Name} disconnected");
                }
                catch (Exception ex)
                {
                    Logger.WarningMsg($"[AKClient] {Name} disconnect error: {ex.Message}");
                }
            }
        }

        private bool EnsureConnected()
        {
            if (Connected) return true;
            lock (_connLock)
            {
                if (Connected) return true;
                try
                {
                    Logger.DebugMsg($"[AKClient] {Name} connecting to {_host}:{_port} ...");
                    var tc = new TcpClient();
                    tc.NoDelay = true;
                    tc.Connect(_host, _port);
                    try { tc.Client.NoDelay = true; } catch { }
                    try { tc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); } catch { }
                    _tcpClient = tc; // jetzt erst publizieren

                    // Watchdog erst NACH erfolgreichem Connect starten
                    try { _timer?.Stop(); _timer?.Start(); } catch { }

                    Logger.DebugMsg($"[AKClient] {Name} connected");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.FatalMsg($"[AKClient] {Name} connect error: {ex.Message}");
                    try { _tcpClient?.Close(); } catch { }
                    _tcpClient = null;
                    return false;
                }
            }
        }

        // -------------------- Sequencer --------------------
        private void SequencerLoop()
        {
            while (!_shouldStop)
            {
                _queueEvt.WaitOne(10);
                if (_shouldStop) break;

                if (!_inFlight)
                {
                    CmdItem item = null;
                    lock (_queue)
                    {
                        if (_queue.Count > 0) item = _queue.Dequeue();
                    }

                    if (item != null)
                    {
                        _expectedVerb = item.ExpectedVerb;
                        _expectedChannel = item.ExpectedChannel;
                        _currentCallback = item.Callback;
                        _inFlight = true;
                        _inFlightDeadline = Environment.TickCount64 + (item.TimeoutMs > 0 ? item.TimeoutMs : _timeoutMs);

                        if (!TransmitImmediate(item.Text, item.WithStxEtx, item.AppendCrlf))
                        {
                            // Senden fehlgeschlagen → sofort freigeben
                            _expectedVerb = null;
                            _expectedChannel = null;
                            _currentCallback = null;
                            _inFlight = false;
                            _queueEvt.Set();
                        }
                    }
                }
                else
                {
                    // Timeout prüfen
                    if (Environment.TickCount64 > _inFlightDeadline)
                    {
                        Logger.WarningMsg($"[AKClient] {Name} sequencer timeout waiting for '{_expectedVerb}'");
                        _expectedVerb = null;
                        _expectedChannel = null;
                        _currentCallback = null;
                        _inFlight = false;
                        _queueEvt.Set();
                    }
                }
            }
        }

        // -------------------- Receiver --------------------
        protected void WorkLoop()
        {
            while (!_shouldStop)
            {
                if (!EnsureConnected()) { Thread.Sleep(200); continue; }
                if (_exclusiveIo) { Thread.Sleep(1); continue; }

                try
                {
                    var tcLocal = _tcpClient;
                    if (tcLocal == null) { Thread.Sleep(5); continue; }
                    var sock = tcLocal.Client;

                    while (sock.Available > 0)
                    {
                        int toRead = Math.Min(4096, sock.Available);
                        if (toRead <= 0) break;
                        byte[] buf = new byte[toRead];
                        int n = sock.Receive(buf, 0, toRead, SocketFlags.None);
                        if (n <= 0) break;

                        for (int i = 0; i < n; i++)
                        {
                            byte b = buf[i];
                            if (b == 0x02)
                            {
                                _resp.Clear();
                            }
                            else if (b == 0x03)
                            {
                                // kompletter Frame
                                var msg = Encoding.ASCII.GetString(_resp.ToArray());
                                _resp.Clear();

                                if (!string.IsNullOrEmpty(msg))
                                {
                                    // Watchdog reset bei jedem gültigen Frame
                                    try { _timer?.Stop(); _timer?.Start(); } catch { }

                                    Received = msg;
                                    RaiseMessage(msg);
                                    try { OnReceived?.Invoke(msg); } catch { }

                                    if (_inFlight && VerbMatches(msg, _expectedVerb))
                                    {
                                        try { OnMatchedResponse?.Invoke(_expectedVerb, msg); } catch { }
                                        try { _currentCallback?.Invoke(msg); } catch { }

                                        // Parsed Message erzeugen und feuern
                                        var parsed = ParseAkResponse(msg, _expectedVerb, _expectedChannel);
                                        try { MessageParsed?.Invoke(this, parsed); } catch { }
                                        try { OnMessageReceived(parsed); } catch { }

                                        // Reader freigeben (wenn es einer ist)
                                        try
                                        {
                                            if (!string.IsNullOrEmpty(parsed.Command))
                                                WriteReaderResponse($" {parsed.Command}{(parsed.Channel != null ? " " + parsed.Channel : "")}",
                                                    parsed.Parameters?.ToList() ?? new List<string>());
                                        }
                                        catch { }

                                        _expectedVerb = null;
                                        _expectedChannel = null;
                                        _currentCallback = null;
                                        _inFlight = false;
                                        _queueEvt.Set();
                                    }
                                }
                            }
                            else
                            {
                                _resp.Add(b);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.FatalMsg($"[AKClient] {Name} receive error: {ex.Message}");
                    Disconnect();
                    Thread.Sleep(250);
                }

                Thread.Sleep(3);
            }
        }

        private static bool VerbMatches(string msg, string expectedVerb)
        {
            if (string.IsNullOrEmpty(expectedVerb)) return false;
            if (string.IsNullOrWhiteSpace(msg)) return false;

            var s = msg.TrimStart();
            var parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return false;

            int idx = 0;
            // Wenn erstes Token ein Zahl-Byte ist, steht das Verb vermutlich an Position 1
            if (TryParseByteFlexible(parts[0], out _)) idx = 1;
            if (parts.Length <= idx) return false;

            var verb = parts[idx];
            return verb.Equals(expectedVerb, StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractVerb(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var s = text.TrimStart();
            int sp = s.IndexOf(' ');
            return sp >= 0 ? s.Substring(0, sp) : s;
        }

        private static string ExtractChannel(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var s = text.TrimStart();
            int sp1 = s.IndexOf(' ');
            if (sp1 < 0) return null;
            int sp2 = s.IndexOf(' ', sp1 + 1);
            if (sp2 < 0) return s.Substring(sp1 + 1); // bis zum Ende
            return s.Substring(sp1 + 1, sp2 - sp1 - 1); // zweites Token
        }

        // -------------------- Öffentliche API --------------------
        public void Transmit(string text, bool withStxEtx = true, bool appendCrlf = false)
        {
            if (string.IsNullOrEmpty(text)) return;
            var expectedVerb = ExtractVerb(text);
            var expectedChannel = ExtractChannel(text);
            Enqueue(text, _timeoutMs, withStxEtx, appendCrlf, expectedVerb, expectedChannel, null);
        }

        public void Transmit(string text, int timeoutMs, bool withStxEtx, bool appendCrlf, Action<string> onResponse)
        {
            if (string.IsNullOrEmpty(text)) return;
            var expectedVerb = ExtractVerb(text);
            var expectedChannel = ExtractChannel(text);
            Enqueue(text, timeoutMs, withStxEtx, appendCrlf, expectedVerb, expectedChannel, onResponse);
        }

        public string Transceive(string message, int timeoutMs, bool withStxEtx = true, bool appendCrlf = false)
        {
            return Transceive(message, timeoutMs, withStxEtx, appendCrlf, (string _) => true);
        }

        public string Transceive(string message, int timeoutMs, bool withStxEtx, bool appendCrlf, Func<string, bool> validator)
        {
            if (message == null) return null;
            if (!EnsureConnected()) return null;
            if (validator == null) validator = _ => true;

            lock (_ioLock)
            {
                _exclusiveIo = true;
                try
                {
                    FlushReceive();
                    if (appendCrlf) message += "\r\n";

                    byte[] payload = Encoding.ASCII.GetBytes(message);
                    byte[] framed = withStxEtx ? FrameWithStxEtx(payload) : payload;

                    var tcLocal = _tcpClient;
                    if (tcLocal == null) return null;
                    var sock = tcLocal.Client;
                    sock.NoDelay = true;
                    int sent = sock.Send(framed, 0, framed.Length, SocketFlags.None);
                 //   Logger.DebugMsg($"[AKClient] {Name} TX bytes({sent}): {ToHex(framed)}");
                    if (sent != framed.Length) return null;

                    byte[] receiveBytes = new byte[4096];
                    byte[] receiveBuffer = new byte[8192];
                    int receiveIndex = 0;

                    long deadline = Environment.TickCount64 + timeoutMs;
                    while (Environment.TickCount64 < deadline)
                    {
                        if (sock.Available > 0)
                        {
                            int read = sock.Receive(receiveBytes, 0, Math.Min(receiveBytes.Length, sock.Available), SocketFlags.None);
                            if (read > 0)
                            {
                                for (int i = 0; i < read; i++)
                                {
                                    byte b = receiveBytes[i];
                                    if (withStxEtx)
                                    {
                                        if (b == 0x02)
                                        {
                                            receiveIndex = 0;
                                        }
                                        else if (b == 0x03)
                                        {
                                            string raw = Encoding.ASCII.GetString(receiveBuffer, 0, receiveIndex);
                                            if (validator(raw))
                                            {
                                                try { _timer?.Stop(); _timer?.Start(); } catch { }
                                                Received = raw;
                                                RaiseMessage(raw);
                                                try { OnReceived?.Invoke(raw); } catch { }

                                                var parsed = ParseAkResponse(raw, ExtractVerb(message), ExtractChannel(message));
                                                try { MessageParsed?.Invoke(this, parsed); } catch { }
                                                try { 
                                                    OnMessageReceived(parsed);
                                                    //WriteReaderResponse(" " + parsed.Command + " " + parsed.Channel, parsed.Parameters.ToList());

                                                } catch { }
                                                return raw;
                                            }
                                            else
                                            {
                                                RaiseMessage(raw);
                                                try { OnReceived?.Invoke(raw); } catch { }
                                                receiveIndex = 0;
                                            }
                                        }
                                        else
                                        {
                                            receiveBuffer[receiveIndex] = b;
                                            receiveIndex = (receiveIndex + 1) % receiveBuffer.Length;
                                        }
                                    }
                                    else
                                    {
                                        if (b == (byte)'\n' || b == (byte)'\r')
                                        {
                                            string raw = Encoding.ASCII.GetString(receiveBuffer, 0, receiveIndex);
                                            if (!string.IsNullOrEmpty(raw))
                                            {
                                                if (validator(raw))
                                                {
                                                    try { _timer?.Stop(); _timer?.Start(); } catch { }
                                                    Received = raw;
                                                    RaiseMessage(raw);
                                                    try { OnReceived?.Invoke(raw); } catch { }
                                                    var parsed = ParseAkResponse(raw, ExtractVerb(message), ExtractChannel(message));
                                                    try { MessageParsed?.Invoke(this, parsed); } catch { }
                                                    try { 
                                                        OnMessageReceived(parsed);
                                                        //WriteReaderResponse(" " + parsed.Command + " " + parsed.Channel, parsed.Parameters.ToList());
                                                        } 
                                                    catch { }
                                                    return raw;
                                                }
                                                else
                                                {
                                                    RaiseMessage(raw);
                                                    try { OnReceived?.Invoke(raw); } catch { }
                                                    receiveIndex = 0;
                                                    continue;
                                                }
                                            }
                                            receiveIndex = 0;
                                        }
                                        else
                                        {
                                            receiveBuffer[receiveIndex] = b;
                                            receiveIndex = (receiveIndex + 1) % receiveBuffer.Length;
                                        }
                                    }
                                }
                            }
                        }
                        Thread.Sleep(7);
                    }

                    Logger.WarningMsg($"[AKClient] {Name} transceive timeout after {timeoutMs} ms");
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.FatalMsg($"[AKClient] {Name} transceive error: {ex.Message}");
                    Disconnect();
                    return null;
                }
                finally
                {
                    _exclusiveIo = false;
                }
            }
        }

        private void Enqueue(string text, int timeoutMs, bool withStxEtx, bool appendCrlf, string expectedVerb, string expectedChannel, Action<string> cb)
        {
            lock (_queue)
            {
                _queue.Enqueue(new CmdItem
                {
                    Text = text,
                    TimeoutMs = timeoutMs,
                    WithStxEtx = withStxEtx,
                    AppendCrlf = appendCrlf,
                    ExpectedVerb = expectedVerb,
                    ExpectedChannel = expectedChannel,
                    Callback = cb
                });
            }
            _queueEvt.Set();
        }
        private bool TransmitImmediate(string text, bool withStxEtx, bool appendCrlf)
        {
            try
            {
                if (!EnsureConnected()) return false;
                if (appendCrlf) text += "\r\n";

                byte[] payload = Encoding.ASCII.GetBytes(text);
                byte[] framed = withStxEtx ? FrameWithStxEtx(payload) : payload;

                var tcLocal = _tcpClient;
                if (tcLocal == null) return false;
                var sock = tcLocal.Client;
                sock.NoDelay = true;
                int sent = sock.Send(framed, 0, framed.Length, SocketFlags.None);
          //      Logger.DebugMsg($"[AKClient] {Name} TX bytes({sent}): {ToHex(framed)}");
                return sent == framed.Length;
            }
            catch (Exception ex)
            {
                Logger.FatalMsg($"[AKClient] {Name} transmit error: {ex.Message}");
                return false;
            }
        }
        private static byte[] FrameWithStxEtx(byte[] payload)
        {
            var framed = new byte[payload.Length + 2];
            framed[0] = 0x02;
            Buffer.BlockCopy(payload, 0, framed, 1, payload.Length);
            framed[framed.Length - 1] = 0x03;
            return framed;
        }
        private void FlushReceive()
        {
            try
            {
                var tcLocal = _tcpClient;
                if (tcLocal == null) return;
                var sock = tcLocal.Client;
                byte[] sink = new byte[4096];
                while (sock.Available > 0)
                {
                    int n = sock.Receive(sink, 0, Math.Min(sink.Length, sock.Available), SocketFlags.None);
                    if (n <= 0) break;
                }
                _resp.Clear();
            }
            catch
            {
                // ignore
            }
        }
        private void RaiseMessage(string msg)
        {
            try { MessageReceived?.Invoke(this, msg); } catch { }
        }

        // -------- Parsing --------
        public class AkMessage
        {
            public string Command { get; set; }
            public string Channel { get; set; }
            public string[] Parameters { get; set; }
            public byte Error { get; set; }     // optional Fehlerbyte am Anfang
            public byte? Status { get; set; }   // optional Byte direkt nach dem Befehl
            public string Raw { get; set; }
        }

        private static AkMessage ParseAkResponse(string raw, string expectedVerb, string expectedChannel)
        {
            // Unterstützt beide Formate:
            // 1) " {FehlerByte} {Befehl} {Antwort...}"
            // 2) " {Befehl} {StatusByte?} {Antwort...}"
            var result = new AkMessage
            {
                Raw = raw,
                Channel = expectedChannel,
                Command = expectedVerb,
                Parameters = Array.Empty<string>(),
                Error = 0,
                Status = null
            };

            if (string.IsNullOrWhiteSpace(raw)) return result;

            var s = raw.TrimStart();
            var parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return result;

            int idx = 0;

            // Optionales Fehlerbyte am Anfang (dezimal oder hex)
            if (TryParseByteFlexible(parts[0], out var err))
            {
                // Nur als Fehler interpretieren, wenn danach kein weiteres Byte folgt,
                // sondern ein Verb (also ein nicht-numerisches Token)
                if (parts.Length > 1 && !TryParseByteFlexible(parts[1], out _))
                {
                    result.Error = err;
                    idx = 1;
                }
            }

            // Befehl übernehmen
            if (parts.Length > idx)
            {
                result.Command = parts[idx];
                idx++;
            }

            // Optionales Status-Byte direkt nach dem Befehl überspringen/speichern
            if (parts.Length > idx && TryParseByteFlexible(parts[idx], out var status))
            {
                result.Status = status;
                idx++;
            }

            // Restliche Parameter
            if (parts.Length > idx)
            {
                var paramList = new List<string>(parts.Length - idx);
                for (int i = idx; i < parts.Length; i++) paramList.Add(parts[i]);
                result.Parameters = paramList.ToArray();
            }

            return result;
        }
        private static bool TryParseByteFlexible(string token, out byte value)
        {
            // Erst Dezimal, dann Hex (ohne 0x, mit 0x tolerieren)
            if (byte.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return true;

            string t = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? token.Substring(2) : token;
            return byte.TryParse(t, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        // virtuelle Overload, damit der Nutzer strukturierte Messages verarbeiten kann
        protected virtual void OnMessageReceived(AkMessage msg) 
        {
              string v = msg.Parameters[0];
               Logger.DebugMsg($"[AKClient] {DateTime.Now.ToString("HH:mm:ss.fff")} {Name} received message: {msg.Command} {msg.Channel} {v}");

        }
        protected virtual void OnRawMessage(string raw)
        {
            try { MessageReceived?.Invoke(this, raw); } catch { }
}
    }

    
}
