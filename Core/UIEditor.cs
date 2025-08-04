using AmiumScripter.Modules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AmiumScripter.Core
{
    public static class UIEditor
    {

        public static string CurrentPageName { get; set; } = "TestPage";

        public static Dictionary<string, BaseView> PageView = new();
        //BuildView

        public static void CreateAllViews()
        {
            Debug.WriteLine("[UIEditor] Start creative pageviews");
            Debug.WriteLine("[UIEditor] qty of pages " + PageView.Count);
      
            AttachPagesViewToUI();
        }


        public static void BuildPageViewWithCompilation(string pageName)
        {
            var projectPath = ProjectManager.GetProjectPath(ProjectManager.Project.Name);
            var pagePath = Path.Combine(projectPath, "Pages", pageName, pageName + ".cs");
            var viewPath = Path.Combine(projectPath, "Pages", pageName, "View.cs");
            var controlsPath = Path.Combine(projectPath, "Pages", pageName, "Controls.cs");
            string classesPath = Path.Combine(projectPath, "Pages", pageName, "Classes");

            if (!File.Exists(viewPath))
            {
                Debug.WriteLine($"❌ View.cs nicht gefunden: {viewPath}");
                return;
            }

            var syntaxTrees = new List<SyntaxTree>
    {
        CSharpSyntaxTree.ParseText(File.ReadAllText(viewPath))
    };

            string projectCs = Path.Combine(projectPath, "project.cs");

            if (File.Exists(pagePath))
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(pagePath), path: pagePath));
                Debug.WriteLine("[UIExploroer] Add Buildfile: " + pagePath);
            }
            else
            {
                Debug.WriteLine("Project.cs not found:" + projectCs);
            }



            if (File.Exists(projectCs))
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(projectCs), path: projectCs));
                Debug.WriteLine("Add file: " + projectCs);
            }
            else
            {
                Debug.WriteLine("Project.cs not found:" + projectCs);
            }


            if (Directory.Exists(classesPath))
            {
                foreach (var file in Directory.GetFiles(classesPath, "*.cs"))
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(file)));
            }


            if (File.Exists(controlsPath))
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(controlsPath)));

            // Referenzen: Framework + dein eigenes Projekt
            var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ?? Array.Empty<string>();
            var refNames = new[]
{
    "System.Private.CoreLib",
    "System.Runtime",
    "System.Console",
    "System.Linq",
    "System.Drawing",
    "System.Drawing.Primitives",  // ✅ HINZUGEFÜGT
    "System.Windows.Forms",
    "System.Text.Json",
    "System.Text.RegularExpressions",
    "System.Private.Uri",
    "netstandard",
    "System.Collections",
    "System.IO",
    "System.Private.Xml",
    "System.Private.Xml.Linq",
    "System.ComponentModel.Primitives",
    "System.ComponentModel.TypeConverter",
    "System.Runtime.Extensions",
    "System.ObjectModel",
    "System.Linq.Expressions",
    "System.Memory"
};

            var references = trustedAssemblies
                .Where(p => refNames.Any(r => Path.GetFileNameWithoutExtension(p).Equals(r, StringComparison.OrdinalIgnoreCase)))
                .Select(p => MetadataReference.CreateFromFile(p))
                .ToList();

            references.Add(MetadataReference.CreateFromFile(typeof(BaseView).Assembly.Location)); // dein Projektcode (z. B. IView, BaseView)

            var compilation = CSharpCompilation.Create(
                $"ViewAssembly_{pageName}",
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                Debug.WriteLine("❌ Fehler beim Kompilieren von View.cs:");
                foreach (var diag in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                    Debug.WriteLine("  ➤ " + diag);
                return;
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            // View-Typ laden
            var fullTypeName = $"AmiumScripter.Pages.{pageName}.View";
            var viewType = assembly.GetType(fullTypeName);
            if (viewType == null || !typeof(BaseView).IsAssignableFrom(viewType))
            {
                Debug.WriteLine($"❌ View-Klasse '{fullTypeName}' nicht gefunden oder ungültig.");
                return;
            }
            PageView[pageName] = Activator.CreateInstance(viewType) as BaseView;
        }

        public static TabControl TabControlHost { get; set; } = AmiumScripter.Root.Main.Book;

        public static void Reset()
        {
            foreach (BaseView view in PageView.Values)
                view.Destroy();
            PageView.Clear();
            AmiumScripter.Root.Main.Book.TabPages.Clear();
        }

        public static void AttachPagesViewToUI()
        {

            Debug.WriteLine("[UIEditor] Try to Add PageViews");
            AmiumScripter.Root.Main.Book.TabPages.Clear();

            foreach (var view in PageView)
            {
                Debug.WriteLine("[UIEditor] Add PageView " + view.Key);
                view.Value.Invalidate();
                view.Value.Dock = DockStyle.Fill;
                view.Value.BackColor = Color.White;

                var tabPage = new TabPage(view.Key);
                tabPage.Controls.Add(view.Value);
                AmiumScripter.Root.Main.Book.TabPages.Add(tabPage);

            }
        }

        //BuildView End


        // EditControls.cs-----
        public static void UpdateControlsCs(string filePath, List<string> controlInitLines, string controlName)
        {
            var beginDeclTag = "//#BEGIN-AUTO-ADD-CONTROLS";
            var endDeclTag = "//#END-AUTO-ADD-CONTROLS";
            var beginInitTag = "//#BEGIN-AUTO-GENERATED";
            var endInitTag = "//#END-AUTO-GENERATED";

            var originalLines = File.ReadAllLines(filePath).ToList();
            var output = new List<string>();

            bool insideDeclBlock = false;
            bool insideInitBlock = false;

            foreach (var line in originalLines)
            {
                if (line.Contains(beginDeclTag))
                {
                    insideDeclBlock = true;
                    output.Add(line);
                    output.Add($"        public static SignalView {controlName};");
                    continue;
                }
                if (line.Contains(endDeclTag))
                {
                    insideDeclBlock = false;
                    output.Add(line);
                    continue;
                }

                if (line.Contains(beginInitTag))
                {
                    insideInitBlock = true;
                    output.Add(line);

                    // Init block
                    output.Add($"            {controlName} = new SignalView");
                    output.Add("            {");
                    foreach (var ctrlLine in controlInitLines)
                        output.Add("                " + ctrlLine);
                    output.Add("            };");
                    output.Add($"            parent.Controls.Add({controlName});");
                    output.Add($"            Logger.Log(\"[PageControls] Creating instance of SignalView  {controlName}\");");
                    output.Add("            // ----");
                    continue;
                }

                if (line.Contains(endInitTag))
                {
                    insideInitBlock = false;
                    output.Add(line);
                    continue;
                }

                if (!insideDeclBlock && !insideInitBlock)
                    output.Add(line);
            }

            File.WriteAllLines(filePath, output);
        }
        public static bool ControlExists(string name, string page)
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
            if (!name.StartsWith("Ctr"))
                name = "Ctr" + name;

            var filePath = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name), "Pages", page, "Controls.cs");

            if (!File.Exists(filePath))
                return false;

            return File.ReadAllLines(filePath)
                       .Any(line => line.Contains($"{name} {{ get; set; }}"));
        }
        public static void RemoveSignalControl(string name, string target, string page)
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
            if (!name.StartsWith("Ctr"))
                name = "Ctr" + name;

            var filePath = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name), "Pages", page, "Controls.cs");

            if (!File.Exists(filePath))
            {
                MessageBox.Show($"⚠️ Datei nicht gefunden: {filePath}");
                return;
            }

            var beginTag = "//#BEGIN-AUTO-GENERATED";
            var endTag = "//#END-AUTO-GENERATED";

            var lines = File.ReadAllLines(filePath).ToList();
            var output = new List<string>();
            bool insideGenerated = false;
            bool skipBlock = false;

            foreach (var line in lines)
            {
                if (line.Contains(beginTag))
                {
                    output.Add(line);
                    insideGenerated = true;
                    continue;
                }

                if (line.Contains(endTag))
                {
                    insideGenerated = false;
                    output.Add(line);
                    continue;
                }

                if (insideGenerated)
                {
                    // Prüfen, ob ein Block zu entfernen beginnt
                    if (line.Contains($"// {target}.{name} ----"))
                    {
                        skipBlock = true;
                        continue;
                    }

                    // Blockende erkannt
                    if (line.Contains($"{target}.Controls.Add({name});") && skipBlock)
                    {
                        skipBlock = false;
                        continue;
                    }

                    if (skipBlock)
                        continue;
                }

                output.Add(line);
            }

            File.WriteAllLines(filePath, output);
        }
        // EditControls.cs End-----


        // SignalControl-----
        public static void AddSignalControl(string name, string page, string source = "")
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
            if (!name.StartsWith("Ctr"))
                name = "Ctr" + name;

            //if (!ControlExists(name, page))
            //{
            //    MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
            //    return;
            //}

            var controlLines = AddSignalControlLines(name, source);
            var filePath = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name), "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name);
        }

        static List<string> AddSignalControlLines(string name, string source)
        {
            return new List<string>
    {

        $"    Name = \"{name}\",",
        $"    Location = new Point(10, 10),",
        $"    Size = new Size(200, 100),",
        $"    BorderColor = Color.Black,",
        $"    SignalText = \"Temperatur\",",
        $"    SignalUnit = \"°C\",",
        $"    SignalValue = \"23.5\",",
        $"    SourceName = \"{source}\"",

    };
        }

        // SignalControl End-----

        public static void UpdateControlPosition(string page, string controlName, int x, int y)
        {
            Debug.WriteLine("Update Control " + controlName);
            Debug.WriteLine("X " + x);            
            Debug.WriteLine("Y " + y);
            var filePath = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name), "Pages", page, "Controls.cs");
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("File not found");
                return;
            }
            else
            {
                Debug.WriteLine("File found");
            }

                var lines = File.ReadAllLines(filePath).ToList();
            var output = new List<string>();

            bool insideTargetBlock = false;
            int openBraces = 0;

            foreach (string line in lines)
            {
                var trimmed = line.Trim();

                if (!insideTargetBlock && trimmed.StartsWith($"{controlName} = new SignalView"))
                {
                    Debug.WriteLine("Control found");
                    insideTargetBlock = true;
                }

                if (insideTargetBlock)
                {
                    string property = line.Replace(" ", "");
                    Debug.WriteLine("Property '" + property + "'");
                    if (property.StartsWith("Location"))
                    {
                        Debug.WriteLine("Property Location found");
                        output.Add($"                    Location = new Point({x}, {y}),");
                    }
                    else
                    {
                        output.Add(line);
                    }

                    //// Blockende erreicht
                    if (property.Contains("}"))
                    {
                        insideTargetBlock = false;
                        continue;
                    }
                }
                else
                {
                    output.Add(line);
                }
            }

                                 File.WriteAllLines(filePath, output);
        }



    }
}
