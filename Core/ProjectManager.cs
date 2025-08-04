using AmiumScripter.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{
    public class ProjectLoadContext : AssemblyLoadContext
    {
        public ProjectLoadContext() : base(isCollectible: true) { }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // Optional: eigene Logik für Abhängigkeiten
            return null;
        }
    }


    public class ModuleData
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
    public class ProjectData
    {
        public string Name { get; set; }
        public string Autor { get; set; }
        public DateTime Erstellt { get; set; }

        public List<ModuleData> Modules { get; set; } = new();
        public List<string> Pages { get; set; } = new();  // ✅ hier

        public ProjectData(string name)
        {
            Name = name;
            Erstellt = DateTime.Now;
        }

        public ProjectData() { }
    }
    public static class ProjectManager
    {
        public static ProjectData Project;
     //   public static Assembly? LoadedAssembly { get; set; }

        private static ProjectLoadContext? _loadedContext;
        public static Assembly? LoadedAssembly { get; private set; }
        public static string? LastProjectName { get; private set; }

        public static Dictionary<string, IPage> Pages { get; set; } = new();
        public static Dictionary<string, BaseView> Views = new();
        public static string GetProjectPath(string projectName)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basePath, "Projects", projectName);
        }
        public static void CreateProject(string projectName)
        {
            string vsSetting = @"{
  ""files.exclude"": {
    ""**/*.sln"": true,
    ""**/*.user"": true,
    ""**/bin"": true,
    ""**/obj"": true,
    ""**/.vs"": true,
    ""**/.vscode"": true,
    ""**/project.json"": true
  },
  ""search.exclude"": {
    ""**/bin"": true,
    ""**/obj"": true,
    ""**/.vs"": true
  },
  ""files.watcherExclude"": {
    ""**/bin/**"": true,
    ""**/obj/**"": true,
    ""**/.vs/**"": true
  },
  ""editor.tabSize"": 4,
  ""editor.insertSpaces"": true,
  ""editor.detectIndentation"": false,
  ""editor.wordWrap"": ""on""
}";

            string path = GetProjectPath(projectName);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path,".vscode"));
            Directory.CreateDirectory(Path.Combine(path, "dlls"));

            Directory.CreateDirectory(Path.Combine(path, "Shared"));
            Directory.CreateDirectory(Path.Combine(path, "Shared","Classes"));
            Directory.CreateDirectory(Path.Combine(path, "Shared", "Ressources"));

            SignalPoolCsGenerator.ScheduleUpdate();

            File.WriteAllText(Path.Combine(path, ".vscode","settings.json"), vsSetting);

            string dummyClass = $@"
namespace AmiumScripter.Shared.Classes
{{
    // Placeholder to prevent CS0234
    internal static class NamespaceStub {{ }}
}}";

            string dummy = Path.Combine(path, "Shared", "Classes", "dummy.cs");

            File.WriteAllText(dummy, dummyClass);

            Project = new ProjectData(projectName)
            {
                Autor = Environment.UserName
            };

            SaveProject(Project);
            ProjectBuilder.GenerateDynamicProjectFile(path, projectName);
            MessageBox.Show($"📁 Projekt '{projectName}' mit Referenzen angelegt: {path}");
        }
        public static void ProjectBrowser()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string projectsRoot = Path.Combine(basePath, "Projects");

            using var dialog = new FolderBrowserDialog
            {
                Description = "Projektverzeichnis auswählen",
                SelectedPath = projectsRoot,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;
                string projectName = Path.GetFileName(selectedPath);
                var project = LoadProject(projectName);

                if (project != null)
                {
                    Project = project;
                    MessageBox.Show($"📂 Projekt '{projectName}' wurde geladen.");
                }
                else
                {
                    MessageBox.Show($"❌ Projektdatei konnte nicht geladen werden: {selectedPath}");
                }
            }
        }
        public static ProjectData? LoadProject(string projectName)
        {
            string path = Path.Combine(GetProjectPath(projectName), "project.json");
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
           
            return JsonSerializer.Deserialize<ProjectData>(json);
            
        }
        public static void SaveProject(ProjectData project)
        {
            string path = GetProjectPath(project.Name);
            Directory.CreateDirectory(path);

            string jsonPath = Path.Combine(path, "project.json");
            string json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonPath, json);
            SignalStorageSerializer.SaveToXml();
        }
        public static void BuildProject()
        {
            UIEditor.Reset();
            SignalStorageSerializer.LoadFromXml();
            ClassRuntimeManager.ClearAll();
            ThreadsManager.StopAll();

            if(!ProjectBuilder.BuildAssembly(GetProjectPath(Project.Name)))
            {
                MessageBox.Show("❌ Build failed");
                return;
            }
            var assembly = LoadedAssembly;
            var projectType = assembly.GetType("AmiumScripter.Project");

            if (assembly == null)
            {
                MessageBox.Show("❌ Assembly ist null");
                return;
            }
            if (projectType == null)
            {
                MessageBox.Show("❌ Project-Typ nicht gefunden");
                return;
            }
            GetProjectType().GetMethod("AddPagesViews", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            GetPagesFormAssembly();
            GetViewsFormAssembly();
            projectType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);

            UIEditor.PageView = Views;
            UIEditor.AttachPagesViewToUI();

            Logger.Log("[ProjectManager] BuildProject()");

        }
        public static void RunProject()
        {
            var assembly = LoadedAssembly;
            var projectType = assembly.GetType("AmiumScripter.Project");
            projectType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            Logger.Log("[ProjectManager] RunProject()");
        }
        public static void StopProject()
        {
            Logger.Log("[ProjectManager] StopProject()");
            ThreadsManager.StopAll();
            TokenManager.CancelAll();
            TasksManager.StopAll();
            ClassRuntimeManager.ClearAll();
            UIEditor.Reset();
            PageManager.Pages?.Clear();
            UIEditor.PageView?.Clear();

            UnloadProject();
            Logger.Log("[ProjectManager] Project stopped");
        }
        public static void OpenEditor()
        {
            if (ProjectManager.Project == null)
            {
                MessageBox.Show("Bitte erst ein Projekt erstellen oder laden.");
                return;
            }
            string projectPath = ProjectManager.GetProjectPath(ProjectManager.Project.Name);

            string codeExe = Environment.ExpandEnvironmentVariables(
    @"%LocalAppData%\Programs\Microsoft VS Code\Code.exe");

            Process.Start(codeExe, $"\"{projectPath}\"");
        }
        static void GetPagesFormAssembly()
        {
            Pages.Clear();
            Logger.Log("[ProjectManager] Get Pages");
            var assembly = LoadedAssembly;


            var projectType = assembly.GetType("AmiumScripter.Project");
            var pagesDict = projectType.GetField("Pages", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as IDictionary;
            Logger.Log("[ProjectManager] qty Pages " + pagesDict.Count);


            foreach (DictionaryEntry entry in pagesDict)
                if (entry.Key is string key && entry.Value is IPage page)
                    Pages[key] = page;
        }
        static void GetViewsFormAssembly()
        {
            Pages.Clear();
            var assembly = LoadedAssembly;
            var projectType = assembly.GetType("AmiumScripter.Project");
            var viewsDict = projectType.GetField("Views", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as IDictionary;
       
            foreach (DictionaryEntry entry in viewsDict)
                if (entry.Key is string key && entry.Value is BaseView page)
                    Views[key] = page;
        }
        public static bool BuildAndLoadProject(byte[] assemblyBytes, string projectName = null)
        {
            UnloadProject();

            _loadedContext = new ProjectLoadContext();
            try
            {
                using var ms = new MemoryStream(assemblyBytes);
                LoadedAssembly = _loadedContext.LoadFromStream(ms);
                LastProjectName = projectName;
                return true;
            }
            catch (Exception ex)
            {
                LoadedAssembly = null;
                _loadedContext = null;
                Console.WriteLine($"[ProjectManager] BuildAndLoadProject failed: {ex.Message}");
                return false;
            }
        }
        public static void UnloadProject()
        {
            GetProjectType()?.GetMethod("Destroy", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
 
            if (_loadedContext != null)
            {
                _loadedContext.Unload();
                _loadedContext = null;
                LoadedAssembly = null;
                LastProjectName = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        public static Type? GetProjectType()
        {
            return LoadedAssembly?.GetType("AmiumScripter.Project");
        }


    }

    public static class ProjectBuilder
    {
        public static void GenerateDynamicProjectFile(string projectPath, string projectName)
        {
            string csprojContent = GenerateCsprojWithAbsolutePaths();

            // Speichern
            string csprojPath = Path.Combine(projectPath, $"{projectName}.csproj");
            File.WriteAllText(csprojPath, csprojContent);
      
        }
        public static string GenerateCsprojWithAbsolutePaths()
        {
            string projectDlls = Path.Combine(ProjectManager.GetProjectPath(ProjectManager.Project.Name), "dlls");
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Alle DLL-Dateien aus beiden Verzeichnissen holen
            var dllFiles = Directory.GetFiles(baseDir, "*.dll")
                .Concat(Directory.Exists(projectDlls) ? Directory.GetFiles(projectDlls, "*.dll") : Array.Empty<string>())
                .ToArray();

            string[] exclude = { "Microsoft.CodeAnalysis" };

            var referenceItems = dllFiles
                .Where(path =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    return !exclude.Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                })
                .Select(path =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    return $@"
    <Reference Include=""{fileName}"">
      <HintPath>{path}</HintPath>
    </Reference>";
                });

            return $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  <ItemGroup>
{string.Join("\n", referenceItems)}
  </ItemGroup>
</Project>";
        }

        private static bool _assemblyResolveRegistered = false;
        public static bool BuildAssembly(string projectPath)
        {
            var syntaxTrees = new List<SyntaxTree>();
            try
            {
                Logger.Log("[ProjectBuilder] BuildAssembly Try to Build: " + projectPath);

                string projectRoot = projectPath;
                string sharedClassesRoot = Path.Combine(projectRoot, "Shared", "Classes");
 
                string customDllsRoot = Path.Combine(projectRoot, "dlls");

                Logger.Log("[ProjectBuilder] projectRoot:          " + projectRoot);
                Logger.Log("[ProjectBuilder] sharedClassesRoot:    " + sharedClassesRoot);
                Logger.Log("[ProjectBuilder] customDllsRoot:       " + customDllsRoot);

                string pagesFolder = Path.Combine(projectPath, "Pages");

                List<string> pages = Directory.GetDirectories(pagesFolder, "*", SearchOption.TopDirectoryOnly).ToList();

                Logger.Log("[ProjectBuilder] Qty pages:       " + pages.Count);

                foreach (string page in pages)
                {
                  syntaxTrees.AddRange(TryParsePageFiles(page, debug: false));
                }

                // Shared Classes
                if (Directory.Exists(sharedClassesRoot))
                    syntaxTrees.AddRange(TryParseValidCSharpFiles(sharedClassesRoot));

                // Project
                string projectCsPath = Path.Combine(projectRoot, "project.cs");
                if (File.Exists(projectCsPath))
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(CheckCode(projectCsPath), path: projectCsPath));
                else
                    Logger.Fatal("[PageRuntime] project.cs not found: " + projectCsPath);

                //SignalPool
                string sharedSignalPool = Path.Combine(projectRoot, "Shared", "SignalPool.cs");
                if (File.Exists(sharedSignalPool))
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(CheckCode(sharedSignalPool), path: sharedSignalPool));
                else
                    Logger.Fatal("[PageRuntime] SignalPool.cs not found: " + sharedSignalPool);


                foreach (var tree in syntaxTrees)
                    Logger.Log("[Build] SyntaxTree: " + tree.FilePath);


                // Custom DLLs - Assembly Resolve einmalig registrieren
                if (!_assemblyResolveRegistered)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                        string dllPath = Path.Combine(customDllsRoot, assemblyName);
                        if (File.Exists(dllPath))
                        {
                            return Assembly.LoadFrom(dllPath);
                        }
                        return null;
                    };

                    _assemblyResolveRegistered = true;
                }

                // Referenzen aus .NET-Runtime
                var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ?? Array.Empty<string>();
                var neededRefs = new[]
                {
            "System.Private.CoreLib",
            "System.Runtime",
            "System.Console",
            "System.Linq",
            "System.Drawing",
            "System.Drawing.Primitives",
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
                    .Where(path => neededRefs.Any(name => Path.GetFileNameWithoutExtension(path).Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .Select(path => MetadataReference.CreateFromFile(path))
                    .ToList();

                // Eigene Referenz hinzufügen (z. B. IPage, eigene Basisklassen etc.)
                references.Add(MetadataReference.CreateFromFile(typeof(IPage).Assembly.Location));

                // Benutzerdefinierte DLLs aus dem "dlls"-Ordner einbinden (immer aktuell)
                if (Directory.Exists(customDllsRoot))
                {
                    foreach (var dll in Directory.GetFiles(customDllsRoot, "*.dll"))
                    {
                        // Alte Referenzen mit identischem Pfad entfernen
                        references.RemoveAll(r =>
                            string.Equals(r.Display, dll, StringComparison.OrdinalIgnoreCase));

                        // Neue Referenz hinzufügen
                        references.Add(MetadataReference.CreateFromFile(dll));
                    }
                }

                // Kompilieren
                var compilation = CSharpCompilation.Create(
                    "DynamicPageAssembly",
                    syntaxTrees,
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    Logger.Fatal("❌ Build-Fehler:");
                    foreach (var d in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                        Logger.Fatal($"  ➤ {d}");
                    return false;
                }

                ms.Seek(0, SeekOrigin.Begin);

                byte[] asmBytes = ms.ToArray(); // aus MemoryStream nach Roslyn-Emit
                ProjectManager.BuildAndLoadProject(asmBytes, ProjectManager.Project.Name);

                Logger.Log("[PageRuntime] BuildAssembly successful.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal("[PageRuntime] BuildAssembly Failed " + ex.Message);
                return false;
            }
        }
        private static List<SyntaxTree> TryParseValidCSharpFiles(string directoryPath, bool debug = true)
        {
            var syntaxTrees = new List<SyntaxTree>();

            if (!Directory.Exists(directoryPath))
                return syntaxTrees;

            foreach (var file in Directory.GetFiles(directoryPath, "*.cs"))
            {
                try
                {
                    var code = CheckCode(file);

                    // Einfache Plausibilitätsprüfung – kann erweitert werden
                    if (!string.IsNullOrWhiteSpace(code) &&
                        (code.Contains("namespace") || code.Contains("class") || code.Contains("interface")))
                    {
                        var tree = CSharpSyntaxTree.ParseText(code, path: file);
                        syntaxTrees.Add(tree);

                        if (debug)
                            Debug.WriteLine($"[✔] Added valid file: {file}");
                    }
                    else if (debug)
                    {
                        Debug.WriteLine($"[⚠️] Skipped non-C# or empty file: {file}");
                    }
                }
                catch (Exception ex)
                {
                    if (debug)
                        Debug.WriteLine($"[❌] Failed to parse file: {file}\n{ex.Message}");
                }
            }

            return syntaxTrees;
        }
        private static List<SyntaxTree> TryParsePageFiles(string pageRoot, bool debug = true)
        {
            var syntaxTrees = new List<SyntaxTree>();
            if (!Directory.Exists(pageRoot))
                return syntaxTrees;

            // Page (code)
            string pageCode = Path.Combine(pageRoot, "page.cs");
            if (File.Exists(pageCode))
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(CheckCode(pageCode), path: pageCode));

            // View
            string viewPath = Path.Combine(pageRoot, "view.cs");
            if (File.Exists(viewPath))
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(CheckCode(viewPath), path: viewPath));

            // Controls
            string controlsPath = Path.Combine(pageRoot, "controls.cs");
            if (File.Exists(controlsPath))
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(CheckCode(controlsPath), path: controlsPath));

            // Page Classes
            string pageClassesRoot = Path.Combine(pageRoot, "Classes");
            if (Directory.Exists(pageClassesRoot))
                syntaxTrees.AddRange(TryParseValidCSharpFiles(pageClassesRoot));

            return syntaxTrees;
        }

        static string CheckCode(string file)
        {
            string code = File.ReadAllText(file);


            if(code.Contains("CancellationToken"))
                Logger.Log($"[WARN] {file} contains direct unmanaged CancellationToken usage! Please use 'AToken()' instead.");
            // Thread
            var reThread = new Regex(@"new\s+Thread\b|System\.Threading\.Thread\b", RegexOptions.IgnoreCase);
            if (reThread.IsMatch(code))
                Logger.Log($"[WARN] {file} contains direct unmanaged Thread usage! Please use 'AThread' instead.");

            // Task
            var reTask = new Regex(@"new\s+Task\b|System\.Threading\.Tasks\.Task\b", RegexOptions.IgnoreCase);
            if (reTask.IsMatch(code))
                Logger.Log($"[WARN] {file} contains direct unmanaged Task usage! Please use 'ATask' or your own task registry instead.");

            // Sleep
            //var reSleep = new Regex(@"\bSleep\s*\(|System\.Threading\.Thread\.Sleep\b", RegexOptions.IgnoreCase);
            //if (reSleep.IsMatch(code))
            //    Logger.Log($"[WARN] {file} contains direct unmanaged Sleep usage! Please use 'ASleep' instead.");

            // while(true)
            var reWhile = new Regex(@"while\s*\(\s*true\s*\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (reWhile.IsMatch(code))
                Logger.Log($"[WARN] {file} contains 'while(true)'! Please use a cancellation token or IsRunning flag for exit condition.");

            // for(;;)
            var reFor = new Regex(@"for\s*\(\s*;\s*;\s*\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (reFor.IsMatch(code))
                Logger.Log($"[WARN] {file} contains 'for(;;)'! Please use a cancellation token or IsRunning flag for exit condition.");

            return code;
        }






    }











}
