using AmiumScripter.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{

    public interface IClient
    {
        string Name { get; }
        void Initialize();
        void Run();
        void Destroy();
    }

    public abstract class AClient : IClient
    {
        public string Name { get; protected set; }

        protected AClient(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ClientManager.Register(this);
        }

        public abstract void Initialize();
        public abstract void Run();
        public abstract void Destroy();

        public void PushSignal(object value, bool direct = false)
        {
            if (value is not BaseSignalCommon signal)
                throw new Exception($"AClient {Name} tried to push an invalid signal type: {value?.GetType().Name}");

            if (!ClientManager.IsRegistered(this))
                throw new InvalidOperationException("Unauthorized SignalClient!");

            var fullName = $"{Name}_{signal.Name}";

            if (direct)
                SignalManager.SetImmediate(fullName, signal);
            else
                SignalManager.QueueSet(fullName, signal);
        }


        public void SetProperty(string name, string key, string? value)
        {
            var fullName = $"{Name}_{name}";
            SignalManager.SetProperty(fullName, key, value);
        }

        public void RemoveSignal(string name)
        {
            var fullName = $"{Name}_{name}";
            SignalManager.RemoveSignal(fullName);
        }
    }



    public static class ClientManager
    {
        private static readonly List<IClient> _clients = new();

        internal static void Register(IClient client)
        {
            if (!_clients.Contains(client))
                _clients.Add(client);
        }

        public static void Deregister(IClient client)
        {
            _clients.Remove(client);
        }

        public static void DestroyAll()
        {
            foreach (var c in _clients.ToList())
                c.Destroy();
            _clients.Clear();
        }
        public static bool IsRegistered(IClient client) => _clients.Contains(client);
    }


}
