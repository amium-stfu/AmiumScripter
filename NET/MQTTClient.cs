using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using AmiumScripter.Core;

namespace AmiumScripter.NET
{


    namespace AmiumScripter.NET
    {
        public class MqttClient : AClient
        {
            private IManagedMqttClient client;
            private ManagedMqttClientOptions options;

            public string Broker { get; }
            public int Port { get; }
            public string Username { get; }
            public string Password { get; }
            public string ClientId { get; }
            public bool Connected { get; private set; }
            public bool Error { get; private set; }

            public MqttClient(
                string name,
                string broker,
                int port,
                string username,
                string password,
                string clientId
            ) : base(name)
            {
                Broker = broker;
                Port = port;
                Username = username;
                Password = password;
                ClientId = clientId;
            }

            public override void Initialize()
            {
                var builder = new MqttClientOptionsBuilder()
                    .WithTcpServer(Broker, Port)
                    .WithCredentials(Username, Password)
                    .WithClientId(ClientId)
                    .WithCleanSession();

                options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(10))
                    .WithClientOptions(builder.Build())
                    .Build();

                client = new MqttFactory().CreateManagedMqttClient();

                client.ConnectedAsync += e =>
                {
                    Connected = true;
                    Error = false;
                    Debug.WriteLine($"[MQTT] Connected: {Name}");
                    return Task.CompletedTask;
                };
                client.DisconnectedAsync += e =>
                {
                    Connected = false;
                    Debug.WriteLine($"[MQTT] Disconnected: {Name}");
                    return Task.CompletedTask;
                };
                client.ConnectingFailedAsync += e =>
                {
                    Connected = false;
                    Error = true;
                    Debug.WriteLine($"[MQTT] Connect FAILED: {Name}");
                    return Task.CompletedTask;
                };
                client.ApplicationMessageReceivedAsync += e =>
                {
                    // Hier kannst du deine Signale pushen
                    OnMessageReceived(e);
                    return Task.CompletedTask;
                };
            }

            public override void Run()
            {
                client.StartAsync(options).GetAwaiter().GetResult();
            }

            public override void Destroy()
            {
                try { client?.StopAsync().GetAwaiter().GetResult(); } catch { }
                ClientManager.Deregister(this);
            }

            // Convenience-Methoden:
            public void Publish(string topic, string payload, bool retain = false)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithRetainFlag(retain)
                    .Build();
                client.EnqueueAsync(msg);
            }

            public void Subscribe(string topic)
            {
                client.SubscribeAsync(topic);
            }

            public void Unsubscribe(string topic)
            {
                client.UnsubscribeAsync(topic);
            }

            // Kannst du virtuell lassen oder als Event rausreichen
            protected virtual void OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
            {
                // z.B.: PushSignal(...) nutzen!
            }
        }
    }

}
