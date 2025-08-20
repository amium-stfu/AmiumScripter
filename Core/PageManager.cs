
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace AmiumScripter.Core
{
    public interface IPage
    {
        string Name { get; }
        void Initialize();
        void Run();
        void Destroy();
    }




    public static class PageManager
    {
        private static readonly List<IPage> LoadedPages = new();

        public static Dictionary<string, IPage> Pages { get; set; } = new();
        public static void AddPage(string projectName, string pageName)
        {
            string pageRootPath = Path.Combine(ProjectManager.Project.Workspace, "Pages", pageName);
 
            string classesPath = Path.Combine(pageRootPath, "Classes");
            string controlsPath = Path.Combine(pageRootPath, "Classes");

            string dummyClass = $@"
namespace AmiumScripter.Pages.{pageName}.Classes
{{
    // Placeholder to prevent CS0234
    internal static class NamespaceStub {{ }}
}}";

            Directory.CreateDirectory(pageRootPath);
            Directory.CreateDirectory(classesPath);

            File.WriteAllText(Path.Combine(classesPath, "dummy.cs"), dummyClass);

            AddView(projectName, pageName);
            AddControls(projectName, pageName);
  
            string pageCode = $@"
using AmiumScripter;
using AmiumScripter.Modules;
using AmiumScripter.Core;
using AmiumScripter.Shared;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Text.Json;
using AmiumScripter.Pages.{pageName}.Classes;

namespace AmiumScripter.Pages.{pageName}
{{
    public class Page_{pageName} : IPage
    {{
        public string Name {{get;}}= ""{pageName}""; 

        public void Initialize()
            {{ 
               
                Logger.DebugMsg(""[PageCode] TestPage Initialize"");
            }}

        public void Run() 
            {{
                Logger.DebugMsg(""[PageCode] TestPage Run"");
            }}
        public void Destroy()
            {{
                Logger.DebugMsg(""[PageCode] TestPage Destroy"");
            }}
    }}
}}";

            string filePath = Path.Combine(pageRootPath, "page.cs");
            File.WriteAllText(filePath, pageCode);

            string projectPath = Path.Combine(ProjectManager.Project.Workspace, "project.json");

            if (!File.Exists(projectPath))
            {
                MessageBox.Show($"❌ Projektdatei '{projectPath}' existiert nicht.");
                return;
            }
            else
            {
                MessageBox.Show($"❌ Projektdatei '{projectPath}' existiert");
            }

            string json = File.ReadAllText(projectPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                MessageBox.Show($"❌ Projektdatei '{projectPath}' ist leer oder ungültig.");
                return;
            }

            ProjectData? project;
            try
            {
                project = JsonSerializer.Deserialize<ProjectData>(json);
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"❌ Fehler beim Laden der Projektdatei:\n{ex.Message}");
                return;
            }

            if (project == null)
            {
                MessageBox.Show("❌ Projekt konnte nicht geladen werden.");
                return;
            }

            // Page-URI ermitteln
            string pageCodeFile = Path.Combine(pageRootPath, pageName + ".cs");

            // Wenn noch nicht eingetragen: hinzufügen
            if (!project.Pages.Contains(pageCodeFile))
                project.Pages.Add(pageCodeFile);

            json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(projectPath, json);

         //   MessageBox.Show($"📄 Seite '{pageName}' wurde im Projekt '{projectName}' erstellt.");
            ProjectManager.Project = project;

            WriteProjectCs(ProjectManager.Project.Name);
        }
        internal static void AddView(string projectName, string pageName)
        {
            string viewPath = Path.Combine(ProjectManager.Project.Workspace, "Pages", pageName, "view.cs");
            string viewCode = $@"
using AmiumScripter;
using AmiumScripter.Core;
using AmiumScripter.Shared;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Windows.Forms;
using AmiumScripter.Pages.{pageName}.Classes;

namespace AmiumScripter.Pages.{pageName}
{{
 public class View_{pageName}: BaseView
    {{
        Page_{pageName} Page = Project.{pageName};
        public override void Initialize()
        {{ 
            View.AddControls(this); 
            Logger.DebugMsg(""[PageView] {pageName} Initialize"");
        }}

        
        public override void Run()
        {{ 
            StartIdleLoop(interval: 1000); // Start Idle Loop with 1000ms interval
            Logger.DebugMsg(""[PageView] {pageName} Run"");

        }}
       
        public override void ViewIdleLoop()
        {{
            // View Idle-Code here
            // Example: SaveInvoke(() => Control.BackColor = Control.Value > 100 ? Color.Red : Color.Green);
        }}
    }}
}}
";
            File.WriteAllText(viewPath, viewCode);
        }
        internal static void AddControls(string projectName, string pageName)
        {
            string controlsPath = Path.Combine(ProjectManager.Project.Workspace, "Pages", pageName, "controls.cs");
            string controlsCode = $@"
using AmiumScripter.Controls;
using AmiumScripter.Core;
using AmiumScripter.Shared;
using System.Drawing;
using System.Windows.Forms;
using AmiumScripter.Pages.{pageName}.Classes;

namespace AmiumScripter.Pages.{pageName}
{{
 public static class View
    {{
         //#BEGIN-AUTO-ADD-CONTROLS

         //#END-AUTO-ADD-CONTROLS

        public static void AddControls(Control parent)
        {{
            //#BEGIN-AUTO-GENERATED

            //#END-AUTO-GENERATED
        }}
    }}
}}
";

            File.WriteAllText(controlsPath, controlsCode);
        }
        internal static void WriteProjectCs(string projectName)
        {
            Pages.Clear();
            string ProjectPath = Path.Combine(ProjectManager.Project.Workspace, "Project.cs");

            var cPage = new List<string>();
            var uPage = new List<string>();
            var aPage = new List<string>();
            var vPage = new List<string>();

            // Für jeden Page-Dateinamen im Projekt
            foreach (string pageFile in ProjectManager.Project.Pages)
            {
                string pageName = Path.GetFileNameWithoutExtension(pageFile); // "TestPage"
                string pageClass = $"Page_{pageName}";
                string viewClass = $"View_{pageName}";

                // Property für direkten Zugriff (z.B. Project.TestPage)
                cPage.Add($"public static {pageClass} {pageName} = new {pageClass}();");

                // Usings (für jede Page, für Typsicherheit)
                uPage.Add($"using AmiumScripter.Pages.{pageName};");

                // Dictionaries für dynamischen Zugriff
                aPage.Add($"Pages[\"{pageName}\"] = {pageName};");
                vPage.Add($"Views[\"{pageName}\"] = new {viewClass}();");
            }

            var code = new List<string>();

            // Usings
            code.Add("using AmiumScripter.Core;");
            code.Add("using System.Collections.Generic;");
            foreach (string line in uPage)
                code.Add(line);

            code.Add("");
            code.Add("namespace AmiumScripter");
            code.Add("{");
            code.Add("    public static class Project");
            code.Add("    {");
            code.Add("        public static Dictionary<string, BaseView> Views = new();");
            code.Add("        public static Dictionary<string, IPage> Pages = new();");

            // Properties (typsicherer Zugriff)
            foreach (string line in cPage)
                code.Add("        " + line);

            code.Add("");
            code.Add("        public static void AddPagesViews()");
            code.Add("        {");
            foreach (string line in aPage)
                code.Add("            " + line);
            foreach (string line in vPage)
                code.Add("            " + line);
            code.Add("        }");
            code.Add("");
            code.Add("        public static void Initialize()");
            code.Add("        {");
            code.Add("            foreach (var page in Pages.Values) page.Initialize();");
            code.Add("            foreach (var view in Views.Values) view.Initialize();");
            code.Add("        }");
            code.Add("");
            code.Add("        public static void Run()");
            code.Add("        {");
            code.Add("            foreach (var page in Pages.Values) page.Run();");
            code.Add("            foreach (var view in Views.Values) view.Run();");
            code.Add("        }");
            code.Add("        public static void Destroy()");
            code.Add("        {");
            code.Add("            foreach (var view in Views.Values) view.Destroy();");
            code.Add("            foreach (var page in Pages.Values) page.Destroy();");
            code.Add("        }");
            code.Add("    }");
            code.Add("}");

            File.WriteAllLines(ProjectPath, code);
        }

    }
}
