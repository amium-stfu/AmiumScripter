using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AmiumScripter.Net
{
    public class UdpReceiverBinary
    {
        public string Name { get; }
        public int Port { get; set; }

        public bool IsRunning { get; private set; } = false;
        private Thread idleThread;
        public string HostIp = string.Empty;

        // Event for incoming raw byte messages
        public event EventHandler<byte[]> OnReceivedMessage;

        public UdpReceiverBinary(string name,string hostIp, int port)
        {
            Name = name;
            Port = port;
            HostIp = hostIp;
            idleThread = new Thread(Idle) { IsBackground = true };
        }

        public void Start()
        {
            //   Debug.WriteLine($"[{Name}] Starting UDP receiver on port {Port}...");
            if (!IsRunning)
            {
                Debug.WriteLine($"[{Name}] Starting UDP receiver on port {Port}...");
                IsRunning = true;
                idleThread.Start();
            }
        }

        public void Stop()
        {
            Debug.WriteLine($"[{Name}] Stopping UDP receiver...");
            IsRunning = false;
        }

        private void Idle()
        {

           // HostIp = "192.176.10.1";
            IPAddress allowedIp = IPAddress.Parse(HostIp);
            using (UdpClient udpClient = new UdpClient(Port))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
                while (IsRunning)
                {
                    try
                    {
                        byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);

                      //  if (remoteEndPoint.Address.Equals(allowedIp))
                            OnReceivedMessage?.Invoke(this, receivedBytes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{Name}] Error receiving UDP message: {ex.Message}");
                    }
                    Thread.Sleep(10);
                }
            }
            Console.WriteLine($"[{Name}] UDP receiver stopped.");
        }
    }
}
