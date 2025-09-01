using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
using System.Linq;
using System.Drawing;

namespace AmiumScripter.Net
{
    using System;
    using System.Collections.Generic;




    public class ValueDescriptor
    {
        public UInt16 Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Unit { get; set; }
        public UInt16 Interval { get; set; }

        public bool StepMode { get; set; }

        public ValueDescriptor()
        {

        }

        public ValueDescriptor(UInt16 id, string name, string text, string unit, UInt16 interval, bool stepMode)
        {
            Id = id;
            Name = name;
            Text = text;
            Unit = unit;
            Interval = interval;
            StepMode = stepMode;
        }
    }
    public class ValueDescriptorList
    {
        public UInt16 idCount = 0;

        public Dictionary<string,UInt16> ValueTypes = new Dictionary<string,UInt16>();
        public ValueDescriptorList()
        {
            ValueType = new List<ValueDescriptor>();
            CommandSpecifier = 1;
            DataCount = 1;
        }
        public UInt16 RollingCounter;
        public UInt16 CommandSpecifier;
        public UInt16 DataCount;
        public List<ValueDescriptor> ValueType { get; set; }

        public UInt16 Add(string name, string text,UInt16 interval,bool stepMode, string unit = "") 
        {
            ValueTypes.Add(name, idCount);
            ValueType.Add(new ValueDescriptor(id:idCount, name:name,text:text,unit:unit,interval:interval,stepMode:stepMode));
            UInt16 id = idCount;
            idCount++;
            return id;
        }
    }


    public struct EpochValue
    {
        public UInt64 Epoch;
        public float Value;
        public UInt16 Id;
    }
    public struct DataPacket
    {
        public UInt16 RollingCounter;
        public UInt16 CommandSpecifier;
        public UInt16 DataCount;
        public EpochValue[] Values;
    }
    public struct MessageCounter
    {
        public UInt16 RollingCounter;
        public UInt16 CommandSpecifier;
        public UInt16 DataCount;
        public UInt16 Messages;
    }
}


