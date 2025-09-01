
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace AmiumScripter.Net
{
   
    internal class UdpSenderBinary
    {

        public string Name { get; }
        public int Port { get; }

        bool Send = false;

        bool descriptors = false;
        bool values = false;

        ValueDescriptorList Descriptors = new ValueDescriptorList();

        DataPacket valuePacket = new DataPacket() { CommandSpecifier = 2 };
        List<EpochValue> Values = new List<EpochValue>();

        UInt16 RollingCounter;

        public bool IsRunning { get; private set; } = false;
        private Thread idleThread;
        public UdpSenderBinary(string name, int port)
        {
            Name = name;
            Port = port;
            idleThread = new Thread(Idle) { IsBackground = true };

            Descriptors.Add(name: "vdl", text: "ValueDescriptorList", unit: "",interval:0,stepMode:false);
            Descriptors.Add(name: "dps", text: "DataPackets", unit: "", interval: 0, stepMode: false);
            Descriptors.idCount = 100; //CommandSpecifier 0..100 only for System
        }

        public UInt16 AddDescriptor(string name, string text, UInt16 interval, string unit = "",bool stepMode=false)
        {
            return Descriptors.Add(name: name, text: text, unit: unit, interval: interval, stepMode: stepMode);
        }

        public void AddValueToPacket(UInt16 id, string name, UInt64 epoch, float value)
        {
          //  Debug.WriteLine($"{id} {name} {epoch} {value}");
            Values.Add(new EpochValue() { Id = id, Epoch = epoch, Value = value });  
        }

        public UInt16 Type(string name)
        {
            return Descriptors.ValueTypes[name];
        }

        public void Start()
        {
            if (!IsRunning)
            {
                Debug.WriteLine($"[{Name}] Starting UDP sending on port {Port}...");
                IsRunning = true;
                idleThread.Start();
            }
        }

        public void Stop()
        {
            Debug.WriteLine($"[{Name}] Stopping UDP sender...");
            IsRunning = false;
        }

    

        public void SendDiscriptors()
        {
            descriptors = true;
            Send = true;
            
        }

        public void SendValues()
        {
            //descriptors = false;
            values = true;
            Send = true;
        }

        void Idle()
        {
         
            using (UdpClient udpClient = new UdpClient(Port))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
                try
                {
                    while (IsRunning)
                    {
                        Thread.Sleep(1);
                        if (Send)
                        {
                            byte[] data = null;
                            RollingCounter = 0;

                            if (descriptors)
                            {
                                descriptors = false;
                                RollingCounter++;
                                Descriptors.RollingCounter = RollingCounter;
                                data = Descriptors.ToBytes();

                             //   Debug.WriteLine($"--------------------------------------------");
                             //   Debug.WriteLine($"[Sender] Sending descriptors {data.Length} bytes {DateTime.Now.ToString("HH.mm:ss.fff")}");
                                udpClient.Send(data, data.Length);
                            }

                            if (values)
                            {
                                values = false;
                                valuePacket.Values = Values.ToArray();
                                valuePacket.DataCount = (UInt16)Values.Count;
                                data = valuePacket.ToBytes();
                                //Debug.WriteLine($"--------------------------------------------");
                               // Debug.WriteLine($"[Sender] Sending values {data.Length} bytes {DateTime.Now.ToString("HH.mm:ss.fff")} Values: {Values.Count} -> ");
                                udpClient.Send(data, data.Length);
                                Values.Clear();
                            }

                            Send = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{Name}] Error sending UDP message: {ex.Message}");
                }
            }
        }

    }
}
