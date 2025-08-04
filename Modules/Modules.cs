using AmiumScripter.Core;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static AmiumScripter.Core.StorageItem;
using static System.Windows.Forms.DataFormats;

namespace AmiumScripter.Modules
{
    /// <summary>
    /// Abstract base class for all signal types.
    /// Provides common properties, metadata storage, and update mechanisms.
    /// </summary>

    public class PropertyItem
    {
        [XmlAttribute("Key")]
        public string Key { get; set; }

        [XmlAttribute("Value")]
        public string Value { get; set; }
    }

    public abstract class BaseSignal
    {
        [XmlIgnore]
        public string Name { get; protected set; }

        [XmlElement("Name")]
        public string XmlName
        {
            get => Name;
            set => Name = value;
        }

        [XmlIgnore]
        public ulong LastUpdate { get; protected set; }

        [XmlElement("LastUpdate")]
        public ulong XmlLastUpdate
        {
            get => LastUpdate;
            set => LastUpdate = value;
        }

        [XmlIgnore]
        public string LastSender { get; protected set; }

        [XmlElement("LastSender")]
        public string XmlLastSender
        {
            get => LastSender;
            set => LastSender = value;
        }

        [XmlIgnore]
        public abstract object? Value { get; set; }

        [XmlIgnore]
        public BaseSignal WriteSet
        {
            get => _writeSet;
            protected set
            {
                _writeSet = value;
                _writeSet?.UpdateStorage(); // ensure it's also stored
            }
        }
        private BaseSignal _writeSet;

        [XmlIgnore]
        public virtual string Type => "BaseSignal";

        [XmlIgnore]
        public Dictionary<string, string> Properties { get; set; } = new();

        [XmlElement("Property")]
        public List<PropertyItem> XmlProperties
        {
            get => Properties.Select(p => new PropertyItem { Key = p.Key, Value = p.Value }).ToList();
            set => Properties = value?.ToDictionary(p => p.Key, p => p.Value) ?? new();
        }

        protected void UpdateStorage(string sender = null)
        {
            LastUpdate = (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            SignalPool.Set(Name, this);
            if (sender != null)
                LastSender = sender;
        }

        public IEnumerable<string> PropertyKeys => Properties.Keys;

        public string GetProperty(string key, string fallback = "")
        {
            return Properties.TryGetValue(key, out var val) ? val ?? fallback : fallback;
        }

        public void SetProperty(string key, string? value)
        {
            if (value == null)
                Properties.Remove(key);
            else
                Properties[key] = value;

            UpdateStorage(sender: "Code");
        }

        public bool MatchesWriteSet()
        {
            return WriteSet != null && Equals(Value, WriteSet.Value);
        }
        public void SetLastSender(string sender) => LastSender = sender;
    }

    public class Signal : BaseSignal
    {
        public override object? Value
        {
            get
            {
                if (SignalPool.TryGet(Name, out var obj) && obj is Signal sig)
                    return sig._internalValue;
                return double.NaN;
            }
            set
            {
             _internalValue = Convert.ToDouble(value);
              UpdateStorage(sender:"Code");  
            }
        }

        private double _internalValue;

        public Signal() { }
        public Signal(string name, string text = null, string unit = "", string format = "0.000", double value = double.NaN)
        {
            Name = name;
            Value = value;

            SetProperty("unit", unit);
            SetProperty("text", text ?? name);
            SetProperty("format", format);

            if (!name.EndsWith(".WriteSet"))
                WriteSet = new Signal(name + ".WriteSet", this.Text + " WriteSet", unit, format);
            
            UpdateStorage();
        }

        public string Unit
        {
            get => GetProperty("unit", "");
            set => SetProperty("unit", value);
        }
        public string Text
        {
            get => GetProperty("text", "#" + Name);
            set => SetProperty("text", value);
        }
        public string Format
        {
            get => GetProperty("format", "0.000");
            set => SetProperty("format", value);
        }
        public override string Type => "Signal";
    }
    public class StringSignal : BaseSignal
    {
        private string _value;

        public override object Value
        {
            get 
            {
                if (SignalPool.TryGet(Name, out var obj) && obj is StringSignal sig)
                    return sig._value;
                return double.NaN;
            }
            set
            {
                _value = value?.ToString() ?? "NA";
                UpdateStorage(sender: "Code");
            }
        }

        public StringSignal() { }
        public StringSignal(string name, string text = null)
        {
            Name = name;

            SetProperty("text", text ?? "#" + name);


            if (!name.EndsWith(".WriteSet"))
                WriteSet = new StringSignal(name + ".WriteSet", this.Text + " WriteSet");

            UpdateStorage();
        }

        public string Text
        {
            get => GetProperty("text", "#" + Name);
            set => SetProperty("text", value);
        }

        public override string Type => "StringSignal";
    }
    public class BoolSignal : BaseSignal
    {
        private bool _value;
        public override object Value
        {
            get 
            {
                if (SignalPool.TryGet(Name, out var obj) && obj is BoolSignal sig)
                    return sig._value;
                return double.NaN;
            }
            set
            {
                _value = Convert.ToBoolean(value);
                UpdateStorage(sender: "Code");
            }
        }

        public BoolSignal() { }
        public BoolSignal(string name, string text = null)
        {
            Name = name;
            Text = text ?? "#" + name;

            if (!name.EndsWith(".WriteSet"))
                WriteSet = new BoolSignal(name + ".WriteSet", this.Text + " WriteSet");

            UpdateStorage(sender: "Code");
        }
        public string Text
        {
            get => Properties.TryGetValue("text", out var t) ? t?.ToString() ?? "#" + Name : "#" + Name;
            set => Properties["text"] = value;
        }
        public override string Type => "BoolSignal";

    

    }
    public class Module : Signal
    {
        public Signal Set { get; set; } = null;
        public Signal Out { get; set; } = null;

        public Module() { }
        public Module(string name, string text = null, string unit = "", string format = "0.000") : base(name, text, unit, format)
        {
            Name = name;
            Properties["unit"] = unit;
            Properties["text"] = text ?? "#" + name;
            Properties["format"] = format;

            Set = new Signal(name + ".set", text + ".Set", unit, format);
            Out = new Signal(name + ".out", text + ".Out", "%", "0.00");

            UpdateStorage();
        }

        public override string Type => "Module";

    }





    }
