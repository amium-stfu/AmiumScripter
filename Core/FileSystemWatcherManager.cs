using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AmiumScripter.Core
{
    public interface IWatcher : IDisposable
    {
        string InstanceName { get; }
        bool IsActive { get; }
        void Start();
        void Stop();
    }


    public class AFileSystemWatcher : IWatcher
    {
        public string InstanceName { get; }
        private readonly FileSystemWatcher _watcher;
        public bool IsActive { get; private set; }

        public AFileSystemWatcher(string name, string path, string filter = "*.*")
        {
            InstanceName = name;
            _watcher = new FileSystemWatcher(path, filter)
            {
                EnableRaisingEvents = false,
                IncludeSubdirectories = true
            };
            FileSystemWatcherManager.Register(this);
        }

        public void Start()
        {
            if (!IsActive)
            {
                _watcher.EnableRaisingEvents = true;
                IsActive = true;
            }
        }

        public void Stop()
        {
            if (IsActive)
            {
                _watcher.EnableRaisingEvents = false;
                IsActive = false;
            }
        }

        public FileSystemWatcher Inner => _watcher;

        public void Dispose()
        {
            Stop();
            _watcher.Dispose();
            FileSystemWatcherManager.Deregister(this);
        }
    }

    public static class FileSystemWatcherManager
    {
        private static readonly List<IWatcher> _watchers = new();

        public static void Register(IWatcher watcher)
        {
            lock (_watchers)
            {
                _watchers.Add(watcher);
            }
        }

        public static void Deregister(IWatcher watcher)
        {
            lock (_watchers)
            {
                _watchers.Remove(watcher);
            }
        }

        public static void StopAll()
        {
            lock (_watchers)
            {
                foreach (var watcher in _watchers.ToList())
                {
                    watcher.Stop();
                }
            }
        }

        public static void DisposeAll()
        {
            lock (_watchers)
            {
                foreach (var watcher in _watchers.ToList())
                {
                    watcher.Dispose();
                }
                _watchers.Clear();
            }
        }

        public static IEnumerable<IWatcher> All => _watchers.ToList();
    }

}
