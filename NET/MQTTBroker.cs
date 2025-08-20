using AmiumScripter.Core;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AmiumScripter.NET
{
    public class MqttBroker : AClient
    {
        private MqttServer mqttServer;
        private int Port;

        public MqttBroker(string name, int port = 1883) : base(name)
        {
            Port = port;
        }

        public override void Initialize()
        {
            // Nichts nötig, alles in Run
        }

        public override void Run()
        {
            StartAsync().GetAwaiter().GetResult();
        }

        public override void Destroy()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        public async Task StartAsync()
        {
            if (mqttServer != null)
            {
                Logger.WarningMsg($"[MQTT-Broker] {Name} already running!");
                return;
            }

            var options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(Port)
                .Build();

            mqttServer = new MqttFactory().CreateMqttServer(options);

            mqttServer.ClientConnectedAsync += e =>
            {
                Logger.DebugMsg($"[MQTT-Broker] {Name}: Client connected: {e.ClientId}");
                return Task.CompletedTask;
            };
            mqttServer.ClientDisconnectedAsync += e =>
            {
                Logger.DebugMsg($"[MQTT-Broker] {Name}: Client disconnected: {e.ClientId}");
                return Task.CompletedTask;
            };
            mqttServer.InterceptingPublishAsync += e =>
            {
                Logger.DebugMsg($"[MQTT-Broker] {Name}: Topic: {e.ApplicationMessage.Topic}, Payload: {e.ApplicationMessage.ConvertPayloadToString()}");
                return Task.CompletedTask;
            };

            await mqttServer.StartAsync();
            Logger.DebugMsg($"[MQTT-Broker] {Name} läuft auf Port {Port}");
        }

        public async Task StopAsync()
        {
            if (mqttServer != null)
            {
                await mqttServer.StopAsync();
                Logger.DebugMsg($"[MQTT-Broker] {Name} gestoppt.");
                mqttServer = null;
            }
        }
    }
}
