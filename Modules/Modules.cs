using AmiumScripter.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Text.Json.Serialization;
using static AmiumScripter.Core.StorageItem;
using static System.Windows.Forms.DataFormats;
using AmiumScripter.Simulation;

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

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Signal), "signal")]
    [JsonDerivedType(typeof(Module), "module")]
    [JsonDerivedType(typeof(BoolSignal), "boolsignal")]
    [JsonDerivedType(typeof(StringSignal), "stringsignal")]
    [JsonDerivedType(typeof(DemoSignal), "demosignal")]
    public abstract class BaseSignalCommon
    {
  
        public string Name { get; protected set; }
        public ulong LastUpdate { get; protected set; }
        public bool register;
        public string LastSender { get; protected set; }
        public BaseSignalCommon WriteSet
        {
            get => _writeSet;
            protected set
            {
                _writeSet = value;
                _writeSet?.UpdateStorage();
            }
        }
        private BaseSignalCommon _writeSet;
        public virtual string Type => "BaseSignal";
        public Dictionary<string, string> Properties { get; set; } = new();
        protected void UpdateStorage(string sender = null)
        {
            if (!register) return;
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
            return WriteSet != null && Equals(ValueAsObject, WriteSet.ValueAsObject);
        }
        public void SetLastSender(string sender) => LastSender = sender;

        // Für Polymorphie im Pool
        public abstract object ValueAsObject { get; set; }
    }

    public abstract class BaseSignalDouble : BaseSignalCommon
    {
        public abstract double Value { get; set; }
        public override object ValueAsObject
        {
            get => Value;
            set => Value = value is double d ? d : Convert.ToDouble(value);
        }
    }

    public abstract class BaseSignalBool : BaseSignalCommon
    {
        public abstract bool? Value { get; set; }
        public override object ValueAsObject
        {
            get => Value;
            set => Value = value is bool b ? b : Convert.ToBoolean(value);
        }
    }

    public abstract class BaseSignalString : BaseSignalCommon
    {
        public abstract string Value { get; set; }
        public override object ValueAsObject
        {
            get => Value;
            set => Value = value?.ToString() ?? "";
        }
    }

    public class Signal : BaseSignalDouble
    {
        private double _internalValue;
        public override double Value
        {
            get
            {
                if (SignalPool.TryGet(Name, out var obj) && obj is Signal sig)
                    return sig._internalValue;
                return double.NaN;
            }
            set
            {
                _internalValue = value;
                UpdateStorage(sender: "Code");
            }
        }
        public double DoubleValue
        {
            get => _internalValue;
            set
            {
                _internalValue = value;
                UpdateStorage("Code");
            }
        }
        public Signal() { }
        public Signal(string name, string text = null, string unit = "", string format = "0.000", double value = double.NaN, bool register = true)
        {
            this.register = register;
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

    public class StringSignal : BaseSignalString
    {
        public Action OnUpdate = null;
        
        private string _value;
        public override string Value
        {
            get
            {
                if (SignalPool.TryGet(Name, out var obj) && obj is StringSignal sig)
                    return sig._value;
                return "NA";
            }
            set
            {
                _value = value ?? "NA";
                UpdateStorage(sender: "Code");
                if (OnUpdate != null) OnUpdate();
                
            }
        }
        public StringSignal() { }
        public StringSignal(string name, string text = null,string value = null, bool register = true)
        {
            this.register = register;
            Name = name;
            Value = value ?? "NA";
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

    public class BoolSignal : BaseSignalBool
    {
        private bool _value;
        public override bool? Value
        {
            get
            {
                if (SignalPool.TryGet(Name, out var obj) && obj is BoolSignal sig)
                    return sig._value;
                return null;
            }
            set
            {
                _value = value ?? false;
                UpdateStorage(sender: "Code");
            }
        }
        public BoolSignal() { }
        public BoolSignal(string name, string text = null, bool register = true)
        {
            this.register = register;
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
        public Module(string name, string text = null, string unit = "", string format = "0.000", bool register = true) : base(name, text, unit, format, register: register)
        {
            Name = name;
            Properties["unit"] = unit;
            Properties["text"] = text ?? "#" + name;
            Properties["format"] = format;
            Set = new Signal(name + ".set", text + ".Set", unit, format, register: register);
            Out = new Signal(name + ".out", text + ".Out", "%", "0.00", register: register);
            UpdateStorage();
        }
        public override string Type => "Module";
    }



}
