using AmiumScripter.Core;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;



namespace AmiumScripter
{
    public static class Root
    {
        public static FormMain Main { get; set; }
    }


    internal static class Program
    {


        [STAThread]
        public static void Main()
        {

            // >>> Start der eigentlichen App
            ApplicationConfiguration.Initialize();
            Application.Run(Root.Main = new FormMain());
        }

        }
}