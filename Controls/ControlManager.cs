using AmiumScripter.Core;
using AmiumScripter.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmiumScripter.Controls
{
 
    public class ControlItem
    {
        public string Name { get; set; }
        public string SignalName { get; set; }
        public BaseControl Control { get; set; }

        public ControlItem(string name, string signalName, BaseControl control)
        {
            Name = name;
            SignalName = signalName;
            Control = control;
        }
    }


    public static class ControlManager
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<ControlItem>> _controls = new();

        public static void Register(string name, string sourceName, BaseControl control)
        {
            var controlItem = new ControlItem(name, sourceName, control);
            var bag = _controls.GetOrAdd(sourceName, _ => new ConcurrentBag<ControlItem>());
            bag.Add(controlItem);
        }

        public static void SignalUpdated(string signalName)
        {
            if (_controls.TryGetValue(signalName, out var bag))
                foreach (var item in bag)
                    item.Control.Update();
        }
    }
}
