using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Runtime.Loader;



namespace AmiumScripter
{
    public static class Root
    {
        public static Form1 Main { get; set; }
    }


    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 



        [STAThread]
        public static void Main()
        {
            // >>> Manuelle Assembly-Resolution für DLLs in /lib
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                string libPath = Path.Combine(AppContext.BaseDirectory, "lib", assemblyName);

                if (File.Exists(libPath))
                {
                    return Assembly.LoadFrom(libPath);
                }

                return null;
            };

            // Optional: auch Satellite Assemblies (Sprachverzeichnisse in \lang) nachladen
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (args.Name.Contains(".resources"))
                {
                    string culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
                    string baseName = new AssemblyName(args.Name).Name;
                    string satellitePath = Path.Combine(AppContext.BaseDirectory, "lang", culture, baseName + ".dll");

                    if (File.Exists(satellitePath))
                    {
                        return Assembly.LoadFrom(satellitePath);
                    }
                }
                return null;
            };

            // >>> Start der eigentlichen App
            ApplicationConfiguration.Initialize();
            Application.Run(Root.Main = new Form1());
        }

    }
}