using AmiumScripter.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AmiumScripter.NET
{
    public class AKServer : AClient
    {
        private Thread IdleThread;
        private TcpListener TcpListener;
        private Socket TcpSocket;
        private System.Timers.Timer Timer;

        private readonly int Port;
        private readonly int Timeout;
        private volatile bool _shouldStop = false;

        public string Address { get; private set; }
        public List<byte> resp = new();
        public bool Connected => TcpSocket != null && TcpSocket.Connected;

        public event Action<AKServer, string> MessageReceived;          // raw
        public event Action<AKServer, AkMessage> MessageParsed;         // structured

        private long _rxCount = 0;
        private long _txCount = 0;

        public AKServer(string name, int port, int timeoutMs)
            : base(name)
        {
            Port = port;
            Timeout = timeoutMs;
            Address = "";
            Initialize();
            Logger.DebugMsg($"[AKHost] {Name} initialized on port {Port} timeout {Timeout}ms");
        }

        public class AkMessage
        {
            public string Command { get; set; }
            public string Channel { get; set; }
            public string[] Parameters { get; set; }
            public string Raw { get; set; }
        }

        public override void Initialize()
        {
            Timer = new System.Timers.Timer(Timeout);
            Timer.Elapsed += (s, e) => OnTimeout();
        }

        public override void Run()
        {
            try
            {
                if (TcpListener == null)
                    TcpListener = new TcpListener(IPAddress.Any, Port);
                TcpListener.Start();
                Logger.DebugMsg($"[AKHost] {Name} listening on {Port}");
                _shouldStop = false;
                IdleThread = new Thread(Idle) { IsBackground = true, Name = "AKServer.Idle" };
                IdleThread.Start();
                Timer.Start();
            }
            catch (Exception ex)
            {
                Logger.FatalMsg($"[AKHost] {Name} start failed: {ex.Message}");
                Disconnect();
            }
        }

        public override void Destroy()
        {
            _shouldStop = true;
            try { IdleThread?.Join(500); } catch { }
            try { TcpListener?.Stop(); } catch { }
            try { TcpSocket?.Close(); } catch { }
            try { Timer?.Stop(); Timer?.Dispose(); } catch { }
            Timer = null;
            TcpSocket = null;
            TcpListener = null;
            Address = "";
        }

        private void OnTimeout()
        {
            Logger.WarningMsg($"[AKHost] {Name} inactivity timeout");
            Disconnect();
        }

        public void Disconnect()
        {
            try { TcpSocket?.Close(); } catch { }
            TcpSocket = null;
            Address = "";
            try { Timer?.Stop(); } catch { }
            Logger.DebugMsg($"[AKHost] {Name} disconnected");
        }

        private void Idle()
        {
            while (!_shouldStop)
            {
                if (!Connected)
                {
                    try
                    {
                        Logger.DebugMsg($"[AKHost] {Name} waiting for client...");
                        var socket = TcpListener.AcceptSocket();
                        try { socket.NoDelay = true; } catch { }
                        try { socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); } catch { }
                        TcpSocket = socket;
                        Address = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                        resp.Clear();
                        Timer.Stop(); Timer.Start();
                        Logger.DebugMsg($"[AKHost] {Name} client connected {Address}");
                    }
                    catch (Exception ex)
                    {
                        if (_shouldStop) break;
                        Logger.WarningMsg($"[AKHost] {Name} accept error: {ex.Message}");
                        Thread.Sleep(500);
                    }
                    continue;
                }

                try
                {
                    if (TcpSocket.Available > 0)
                    {
                        byte[] buf = new byte[Math.Min(4096, TcpSocket.Available)];
                        int read = TcpSocket.Receive(buf);
                        if (read <= 0)
                        {
                            Disconnect();
                        }
                        else
                        {
                            for (int i = 0; i < read; i++)
                            {
                                byte b = buf[i];
                                if (b == 0x02)
                                {
                                    resp.Clear();
                                }
                                else if (b == 0x03)
                                {
                                    var raw = Encoding.ASCII.GetString(resp.ToArray());
                                    resp.Clear();
                                    Timer.Stop(); Timer.Start();
                                    _rxCount++;
                                 //   Logger.DebugMsg($"[AKHost] {Name} RX#{_rxCount} raw='{raw}'");
                                    RaiseMessage(raw);
                                }
                                else
                                {
                                    resp.Add(b);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WarningMsg($"[AKHost] {Name} receive error: {ex.Message}");
                    Disconnect();
                }

                Thread.Sleep(2);
            }
        }

        public bool Transmit(string text)
        {
            try
            {
                if (!Connected || string.IsNullOrEmpty(text)) return false;
                var payload = Encoding.ASCII.GetBytes(text);
                var framed = new byte[payload.Length + 2];
                framed[0] = 0x02;
                Buffer.BlockCopy(payload, 0, framed, 1, payload.Length);
                framed[^1] = 0x03;
                int sent = TcpSocket.Send(framed, 0, framed.Length, SocketFlags.None);
                _txCount++;
             //   Logger.DebugMsg($"[AKHost] {Name} TX#{_txCount} '{text}' bytes={sent}/{framed.Length}");
                return sent == framed.Length;
            }
            catch (Exception ex)
            {
                Logger.WarningMsg($"[AKHost] {Name} TX error: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        // Server-Antworten laut deiner Spezifikation:
        // Request: " ASTZ K0"  -> Response: " ASTZ 0 SREM ASTZ" (immer ohne Channel, Status=0, Parameter SREM <Command>)
        // Wichtig: führendes Leerzeichen beibehalten falls angekommen.
        protected virtual void OnMessageReceived(AkMessage msg)
        {
            string firstParam = msg.Parameters.Length > 0 ? msg.Parameters[0] : "";
            Logger.DebugMsg($"[AKHost] {Name} parsed: '{msg.Raw.Replace("\r\n","")}' -> Cmd='{msg.Command}' Ch='{msg.Channel}' P0='{firstParam}'");

            bool hadLeadingSpace = msg.Raw.Length > 0 && msg.Raw[0] == ' ';
            string prefix = hadLeadingSpace ? " " : "";

            if (msg.Command == "ASTZ")
            {
                // Antwort ohne Channel, mit Status 0, Parameter SREM + Original-Command
                Transmit($"{prefix}{msg.Command} 0 SREM {msg.Command}");
            }
            else if (msg.Command == "PING")
            {
                // Falls PING ähnlich (ohne Channel) beantwortet werden soll:
                Transmit($"{prefix}{msg.Command} 0 TRUE");
            }
        }

        private void RaiseMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;
            try { MessageReceived?.Invoke(this, raw); } catch { }
            var parsed = ParseAkMessage(raw);
            try { MessageParsed?.Invoke(this, parsed); } catch { }
            try { OnMessageReceived(parsed); } catch { }
        }

        private static AkMessage ParseAkMessage(string raw)
        {
            var msg = new AkMessage
            {
                Raw = raw,
                Command = null,
                Channel = null,
                Parameters = Array.Empty<string>()
            };
            var s = raw; // nicht trimmen, führendes Space relevant
            if (string.IsNullOrEmpty(s)) return msg;

            // Für Parsing ohne führendes Space:
            var trimmed = s.TrimStart();
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return msg;

            int idx = 0;
            // Optional numerisches Byte ignorieren (wie Client tolerant)
            if (IsByteToken(parts[0]) && parts.Length > 1 && !IsByteToken(parts[1]))
                idx = 1;

            if (idx >= parts.Length) return msg;
            msg.Command = parts[idx++];

            // Falls nach Command etwas wie Kanal aussieht (K0 / K1 ...), nur dann als Channel setzen.
            if (idx < parts.Length && IsChannelToken(parts[idx]))
            {
                msg.Channel = parts[idx++];
            }

            if (idx < parts.Length)
            {
                var rest = new string[parts.Length - idx];
                Array.Copy(parts, idx, rest, 0, rest.Length);
                msg.Parameters = rest;
            }
            return msg;

            static bool IsByteToken(string t)
            {
                if (byte.TryParse(t, out _)) return true;
                if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    return byte.TryParse(t.Substring(2),
                        System.Globalization.NumberStyles.HexNumber,
                        System.Globalization.CultureInfo.InvariantCulture, out _);
                return false;
            }

            static bool IsChannelToken(string t) =>
                t.Length >= 2 && (t[0] == 'K' || t[0] == 'k');
        }
    }
}