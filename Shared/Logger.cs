using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Shared
{
    public static class Logger
    {
        public static void Log(string msg) => Debug.WriteLine($"[LOG] {msg}");
        public static void Fatal(string msg) => Debug.WriteLine($"[FTL] {msg}");
    }
}
