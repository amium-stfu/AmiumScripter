using AmiumScripter.Core;
using AmiumScripter.Forms;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using static AmiumScripter.Logger;
using Microsoft.Build.Locator;



namespace AmiumScripter
{
    public static class Root
    {
        public static FormMain Main { get; set; }

        public static FormLog LogForm = new FormLog();
    }
    public enum LogLevelEnum { Debug, Information, Warning, Error, Fatal }

    public class LogEntry
    {
        public DateTime Time { get; set; }
        public LogLevelEnum LevelEnum { get; set; } = LogLevelEnum.Information;
        public string Level => LevelEnum.ToString(); // nie null
        public string Message { get; set; } = "";
    }

    public static class Logger
    {


        public class WinFormsSink : ILogEventSink
        {
            private readonly SynchronizationContext _ui = new WindowsFormsSynchronizationContext();
            public BindingList<LogEntry> Entries { get; } = new();

            public void Emit(LogEvent e)
            {
                var entry = new LogEntry
                {
                    Time = e.Timestamp.LocalDateTime,
                    LevelEnum = (LogLevelEnum)Enum.Parse(typeof(LogLevelEnum), e.Level.ToString(), true),
                    Message = e.RenderMessage() ?? string.Empty
                };

                _ui.Post(_ =>
                {
                    if (Entries.Count > 2000) Entries.RemoveAt(0);
                    Entries.Add(entry);
                }, null);
            }

        }


        public static WinFormsSink winFormsSink = new WinFormsSink();

        static ILogger Log = new LoggerConfiguration()
       .MinimumLevel.Debug()
       .WriteTo.File(
           path: EnsureLogDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "log-.txt")),
           fileSizeLimitBytes: 10 * 1024 * 1024,
           rollOnFileSizeLimit: true,
           rollingInterval: RollingInterval.Day,
           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:l}{NewLine}{Exception}")
       .WriteTo.Sink(winFormsSink) // <-- Wichtig!
       .CreateLogger();

        private static string EnsureLogDirectory(string logPath)
        {
            var dir = System.IO.Path.GetDirectoryName(logPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            return logPath;
        }


        public static void DebugMsg(string msg)
        {
            Debug.WriteLine($"[Debug] {msg}");
            Log.Debug(msg);
        }

        public static void InfoMsg(string msg)
        {
            Debug.WriteLine($"[LOG] {msg}");
            Log.Information(msg);
        }

        public static void WarningMsg(string msg)
        {
            Debug.WriteLine($"[Warning] {msg}");
            Log.Warning(msg);
        }

        public static void FatalMsg(string msg, Exception ex = null)
        {
            Debug.WriteLine($"[FATAL] {msg}");
            if(ex != null) Debug.WriteLine(ex.Message);
            Log.Fatal(ex, msg);
        }
    }


    internal static class Program
    {


        [STAThread]
        public static void Main()
        {
            if (!Microsoft.Build.Locator.MSBuildLocator.IsRegistered)
                Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

            Debug.WriteLine("Cleaning up temp folders...");
            Cleanup.DeleteProjectTempFolders();
            Debug.WriteLine("Cleanup complete.");


            ApplicationConfiguration.Initialize();
            Application.Run(Root.Main = new FormMain());
        }

        }
}