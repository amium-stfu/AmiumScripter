using AmiumScripter.Controls;
using AmiumScripter.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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


        public static object GetSignal(string name)
        {
            if (_values.ContainsKey(name))
                return _values[name];
            else
                return null;
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
                //  Debug.WriteLine("Update Signal " + key);
                _values[key] = value;
            }
            ControlManager.SignalUpdated(key);
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

        public static DataTable SignalsToDataTable()
        {
            var snapshot = Snapshot(); // Macht ein thread-sicheres Abbild

            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Text", typeof(string));
            table.Columns.Add("Value", typeof(string));
            table.PrimaryKey = new DataColumn[] { table.Columns["Name"] };

            foreach (var kvp in snapshot)
            {
                if (kvp.Value is BaseSignalCommon signal)
                {
                    string name = signal.Name;
                    string text = signal.GetProperty("Text", "");
                    string valueStr = signal.ValueAsObject?.ToString() ?? "";

                    table.Rows.Add(name, text, valueStr);
                }
            }
            return table;
        }



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
                if (SignalPool.TryGet(key, out var obj) && obj is BaseSignalCommon signal)
                {
                    string fieldName = NormalizeName(key);
                    string signalType = signal.Type; // Verwende das SignalType-Property

                    var text = signal.GetProperty("text", $"#{signal.Name}");
                    var unit = signal.GetProperty("unit", "");
                    var format = signal.GetProperty("format", "0.000");

                    // Konstruktor je nach Typ erzeugen
                    string ctorCode = signal switch
                    {

                        Module => $"new Module(name:\"{signal.Name}\", text:\"{text}\", unit:\"{unit}\", format:\"{format}\", register:false)",
                        BoolSignal => $"new BoolSignal(name:\"{signal.Name}\",  text:\"{text}\", register:false)",
                        StringSignal => $"new StringSignal(name:\"{signal.Name}\", text:\"{text}\", register:false)",
                        Signal => $"new Signal(name:\"{signal.Name}\",  text:\"{text}\", unit:\"{unit}\",  format:\"{format}\", register:false)",
                        _ => $"/* Unsupported signal type: {signalType} */ null"
                    };

                    sb.AppendLine($"    public static readonly {signalType} {fieldName} = {ctorCode};");
                }
            }


            sb.AppendLine("}");

            string path = Path.Combine(ProjectManager.Project.Workspace, "Shared", "SignalPool.cs");
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
        public static void SaveToJson()
        {
            string filePath = Path.Combine(ProjectManager.Project.Workspace, "DataStorage.json");
            var allSignals = new List<BaseSignalCommon>();
            foreach (var key in SignalPool.Keys)
            {
                if (SignalPool.TryGet(key, out var obj) && obj is BaseSignalCommon signal)
                {
                    allSignals.Add(signal);
                }
            }
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true,
                    Converters = { new JsonStringEnumConverter() },
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
                string json = JsonSerializer.Serialize(allSignals, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("JSON Serialization error: " + ex.Message);
                MessageBox.Show("JSON Serialization error: " + ex.Message);
            }
        }

        public static void LoadFromJson()
        {
            string filePath = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name), "DataStorage.json");
            SignalPool.Reset();
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("DataStorage.json not found");
                return;
            }
            try
            {
                var options = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
                string json = File.ReadAllText(filePath);
                var signals = JsonSerializer.Deserialize<List<BaseSignalCommon>>(json, options);
                if (signals != null)
                {
                    foreach (var signal in signals)
                    {
                        SignalPool.Set(signal.Name, signal);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load signals: " + ex.Message);
            }
        }
    }

}
