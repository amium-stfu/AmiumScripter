using AmiumScripter.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AmiumScripter.Core
{
    internal class StorageItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public ulong LastUpdate { get; set; }
        public object Value { get; set; }
        public Dictionary<string, object> Attribute { get; set; }
        public T? GetValue<T>()
        {
            if (Value is T t)
                return t;
            return default;
        }
    }

    internal static class SignalPool
    {
        private static readonly ConcurrentDictionary<string, object> _values = new();
        internal static void Reset()
        {
            foreach (string key in _values.Keys)
                Remove(key);
        }
        internal static void Set(string key, object value)
        {
            
            
            bool isNew = !_values.ContainsKey(key);

            if (isNew)
            {
                _values[key] = value;
                Debug.WriteLine("Add Signal " + key);
                SignalPoolCsGenerator.ScheduleUpdate();
              
            }
            else
            {
                Debug.WriteLine("Update Signal " + key);
                _values[key] = value;
            }
        }
        public static T? Get<T>(string key)
        {
            if (_values.TryGetValue(key, out var value) && value is T t)
                return t;
            return default;
        }
        public static bool TryGet(string key, out object value)
        {
            return _values.TryGetValue(key, out value);
        }
        public static IReadOnlyDictionary<string, object> Snapshot()
        {
            return new Dictionary<string, object>(_values);
        }
        internal static void Remove(string key)
        {
            _values.TryRemove(key, out _);
        }
        public static IEnumerable<string> Keys => _values.Keys;

    }
    internal static class SignalPoolCsGenerator
    {
        private static System.Threading.Timer? _delayTimer;
        private static readonly object _lock = new();
        private static bool _pending;

        public static void ScheduleUpdate()
        {
            lock (_lock)
            {
                _pending = true;
                _delayTimer?.Dispose();
                _delayTimer = new System.Threading.Timer(_ => Generate(), null, 3000, Timeout.Infinite);
            }
        }

        private static void Generate()
        {
            Debug.WriteLine("SignalPoolGenerator.Generate() called");


            lock (_lock)
            {
                if (!_pending) return;
                _pending = false;
            }

            var sb = new StringBuilder();
            sb.AppendLine("using AmiumScripter.Modules;");
            sb.AppendLine();
            sb.AppendLine("public static class SignalPool");
            sb.AppendLine("{");

            foreach (var key in SignalPool.Keys)
            {
                Debug.WriteLine($"Found {SignalPool.Keys.Count()} keys");
                if (SignalPool.TryGet(key, out var obj) && obj is BaseSignal signal)
                {
                    if (signal.Name.EndsWith(".WriteSet") || signal.Name.EndsWith(".set") || signal.Name.EndsWith(".out"))
                        continue; // optional: keine abgeleiteten Signale einfügen

                    string fieldName = NormalizeName(key);
                    string typeName = signal.GetType().Name;

                    var text = signal.GetProperty("text", $"#{signal.Name}");
                    var unit = signal.GetProperty("unit", "");
                    var format = signal.GetProperty("format", "0.000");

                    // Konstruktor je nach Typ erzeugen
                    string ctorCode = signal switch
                    {
                        Module => $"new Module(\"{signal.Name}\", \"{text}\", \"{unit}\", \"{format}\")",
                        BoolSignal => $"new BoolSignal(\"{signal.Name}\", \"{text}\")",
                        StringSignal => $"new StringSignal(\"{signal.Name}\", \"{text}\")",
                        Signal => $"new Signal(\"{signal.Name}\", \"{text}\", \"{unit}\", \"{format}\")",
                        _ => $"/* Unsupported signal type: {typeName} */ null"
                    };

                    sb.AppendLine($"    public static readonly {typeName} {fieldName} = {ctorCode};");
                }
            }


            sb.AppendLine("}");

            string path = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name),"Shared", "SignalPool.cs");
            Debug.WriteLine($"Writing SignalPool.cs to: {path}");
            try
            {
                File.WriteAllText(path, sb.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SignalPool write failed: " + ex.Message);
            }

        }

        private static string NormalizeName(string raw)
        {
            var parts = raw.Split('.', '_');
            return string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
        }
    }
    public static class SignalStorageSerializer
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(List<BaseSignal>), new[]
        {
            typeof(Signal), typeof(BoolSignal), typeof(StringSignal), typeof(Module)
        });

        public static void SaveToXml()
        {
            string filePath = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name), "DataStorage.xml");

            var allSignals = new List<BaseSignal>();

            foreach (var key in SignalPool.Keys)
            {
                if (SignalPool.TryGet(key, out var obj) && obj is BaseSignal signal)
                {
                    allSignals.Add(signal);
                }
            }

            using var writer = new StreamWriter(filePath);
            _serializer.Serialize(writer, allSignals);
        }

        public static void LoadFromXml()
        {
            string filePath = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name),"DataStorage.xml");
            
            SignalPool.Reset();

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("DataStorage.xml not found");
                return;
            }

            try
            {
                using var reader = new StreamReader(filePath);
                if (_serializer.Deserialize(reader) is List<BaseSignal> signals)
                {
                    foreach (var signal in signals)
                    {
                        SignalPool.Set(signal.Name, signal);
                    }
                }
            }
            catch (Exception ex)
            {
                // Optional Logging
                Debug.WriteLine("Failed to load signals: " + ex.Message);
            }
        }
    }

}
