using AmiumScripter.Core;
using AmiumScripter.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Controls
{
    public static class ControlManager
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<BaseControl>> _controls = new();

        public static void Register(string signalName, BaseControl control)
        {
            var bag = _controls.GetOrAdd(signalName, _ => new ConcurrentBag<BaseControl>());
            bag.Add(control);
        }

        public static void SignalUpdated(string name)
        {
            if (_controls.TryGetValue(name, out var bag))
            {
                foreach (var control in bag)
                    control.Update();
            }
        }
    }
}
