using AmiumScripter.Shared;
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
        void Initialize();
        void Run();
        void Destroy();
    }

    public static class PageManager
    {
        private static readonly List<IPage> LoadedPages = new();
        public static void AddPage(string projectName, string pageName)
        {

            if (!pageName.StartsWith("Page_"))
                pageName = "Page_" + pageName;

            // Struktur: Pages/PageName/PageName.cs, Pages/PageName/Classes/, Pages/PageName/UI/
            string pageRootPath = Path.Combine(ProjectManager.GetProjectPath(projectName), "Pages", pageName);
            string uiPath = Path.Combine(pageRootPath, "UI");
            string classesPath = Path.Combine(pageRootPath, "Classes");

            Directory.CreateDirectory(pageRootPath);
            Directory.CreateDirectory(uiPath);
            Directory.CreateDirectory(classesPath);

            string pageCode = $@"
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

namespace AmiumScripter.Pages
{{
    public class {pageName} : IPage
    {{
        public void Initialize()
            {{ 
                Console.WriteLine(""{pageName} init"");
            }}

        public void Run() 
            {{
                Console.WriteLine(""{pageName} run"");
            }}
        public void Destroy()
            {{
                Console.WriteLine(""{pageName} destroy"");
            }}
    }}
}}";

            string filePath = Path.Combine(pageRootPath, pageName + ".cs");
            File.WriteAllText(filePath, pageCode);

            string projectPath = Path.Combine(ProjectManager.GetProjectPath(projectName), "project.json");

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

            MessageBox.Show($"📄 Seite '{pageName}' wurde im Projekt '{projectName}' erstellt.");
            ProjectManager.Project = project;
        }
    }

    public static class PageRuntimeManager
    {
        private static readonly List<PageThreadWrapper> _pageThreads = new();

        public static bool BuildPageThreaded(string sourcePath)
        {
            try
            {
                Debug.WriteLine("try to Build: " + sourcePath);

                var syntaxTrees = new List<SyntaxTree>();

                // Hauptseite einlesen
                string sourceCode = File.ReadAllText(sourcePath);
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceCode));

                // Klassen aus Pages/XXX/Classes
                string pageRoot = Path.GetDirectoryName(sourcePath)!;
                string projectRoot = Directory.GetParent(pageRoot)!.Parent!.FullName;
                string sharedPath = Path.Combine(projectRoot, "Shared", "Classes");
                string classesPath = Path.Combine(pageRoot, "Classes");

                if (Directory.Exists(classesPath))
                {
                    foreach (var file in Directory.GetFiles(classesPath, "*.cs"))
                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(file)));
                }

                if (Directory.Exists(sharedPath))
                {
                    foreach (var file in Directory.GetFiles(sharedPath, "*.cs"))
                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(file)));
                }

                // Referenzen sammeln
                var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ?? Array.Empty<string>();
                var neededRefs = new[]
                {
                "System.Private.CoreLib", "System.Runtime", "System.Console", "System.Linq",
                "System.Drawing", "System.Text.Json", "System.Text.RegularExpressions", "System.Private.Uri",
                "netstandard", "System.Collections", "System.IO", "System.Private.Xml", "System.Private.Xml.Linq",
                "System.ComponentModel.Primitives", "System.ComponentModel.TypeConverter", "System.Runtime.Extensions",
                "System.ObjectModel", "System.Linq.Expressions", "System.Memory"
            };

                var references = trustedAssemblies
                    .Where(path => neededRefs.Any(name => Path.GetFileNameWithoutExtension(path).Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .Select(path => MetadataReference.CreateFromFile(path))
                    .ToList();

                references.Add(MetadataReference.CreateFromFile(typeof(IPage).Assembly.Location));

                var compilation = CSharpCompilation.Create(
                    "DynamicPageAssembly",
                    syntaxTrees,
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    Debug.WriteLine("❌ Build-Fehler:");
                    foreach (var d in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                        Debug.WriteLine($"  ➤ {d}");
                    return false;
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());

                var pageType = assembly.GetTypes().FirstOrDefault(t => typeof(IPage).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (pageType == null)
                {
                    Debug.WriteLine("⚠️ Kein gültiger Page-Typ gefunden.");
                    return false;
                }

                var pageInstance = Activator.CreateInstance(pageType) as IPage;
                if (pageInstance == null)
                {
                    Debug.WriteLine("⚠️ Instanz konnte nicht erzeugt werden.");
                    return false;
                }

                var wrapper = new PageThreadWrapper(pageInstance);
                _pageThreads.Add(wrapper);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Exception beim Build:\n{ex}");
                return false;
            }
        }

        public static void BuildAllPages()
        {
            foreach (var wrapper in _pageThreads)
            {
                wrapper.Start(PageThreadWrapper.Phase.Initialize);
            }
        }

        public static void RunAllPages()
        {
            foreach (var wrapper in _pageThreads)
            {
                wrapper.Start(PageThreadWrapper.Phase.Run);
            }
        }

        public static void StopAllPages()
        {
            foreach (var wrapper in _pageThreads)
            {
                wrapper.Stop();
            }
            _pageThreads.Clear();
        }
    }



    public class PageThreadWrapper
    {
        private readonly IPage _page;
        private Thread? _thread;

        private static PageThreadWrapper? _activeInstance;
        public static PageThreadWrapper? Active => _activeInstance;

        public enum Phase { Initialize, Run }

        public PageThreadWrapper(IPage page)
        {
            _page = page;
            _activeInstance = this;
        }

        public void Start(Phase phase)
        {
            _thread = new Thread(() =>
            {
                try
                {
                    switch (phase)
                    {
                        case Phase.Initialize:
                            _page.Initialize();
                            break;
                        case Phase.Run:
                            _page.Run();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"[PageThreadWrapper] Fehler in {phase}: {ex.Message}");
                }
            })
            {
                IsBackground = true,
                Name = $"PageThread_{_page.GetType().Name}_{phase}"
            };

            _thread.Start();
        }

        public void Stop()
        {
            _page.Destroy();
        }

        public static void StopActive() => _activeInstance?.Stop();
        public static void ClearActive() => _activeInstance = null;
    }






}
