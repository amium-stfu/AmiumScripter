using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
        public string Uri { get; set; }
        public string Workspace { get; set; }
        public DateTime Erstellt { get; set; }
        [JsonIgnore]
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

        private static ProjectLoadContext? _loadedContext;
        public static Assembly? LoadedAssembly { get; private set; }
        public static byte[]? LoadedAssemblyBytes { get; private set; }
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

            Logger.InfoMsg($"[ProjectManager] Creating project {projectName}");
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
  ""editor.wordWrap"": ""on"",

  ""window.restoreWindows"": ""none"",
  ""window.openFoldersInNewWindow"": ""on""

}";
            string path = Path.Combine(Path.GetTempPath(), "Project_" + projectName + "_" + Guid.NewGuid());
          //  Logger.InfoMsg($"[ProjectManager] Workspace '{path}'");
            Directory.CreateDirectory(path);

          //  string path = GetProjectPath(projectName);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path,".vscode"));
            Directory.CreateDirectory(Path.Combine(path, "dlls"));

            Directory.CreateDirectory(Path.Combine(path, "Shared"));
            Directory.CreateDirectory(Path.Combine(path, "Shared","Classes"));
            Directory.CreateDirectory(Path.Combine(path, "Shared", "Ressources"));

            SignalPoolCsGenerator.ScheduleUpdate();

            File.WriteAllText(Path.Combine(path, ".vscode","settings.json"), vsSetting);

            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "csharp.code-snippets");
            string targetPath = Path.Combine(Path.Combine(path, ".vscode"), "csharp.code-snippets");
          
            
            File.Copy(sourcePath, targetPath, overwrite: true);



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
                Autor = Environment.UserName,
                Workspace = path,

            };

            SaveProject(Project);
            ProjectBuilder.GenerateDynamicProjectFile(path, projectName);
            Logger.InfoMsg($"[ProjectManager]  Project '{projectName}' created: {path}");
        }
        public static ProjectData? LoadProject(string projectName)
        {
            string path = Path.Combine(GetProjectPath(projectName), "project.json");
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
           
            return JsonSerializer.Deserialize<ProjectData>(json);
            
        }

        public static void AddDllFile() { 
            using var dialog = new OpenFileDialog
            {
                Filter = "DLL-File (*.dll)|*.dll",
                Title = "Select dll-file"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string dllPath = dialog.FileName;
                string targetDir = Path.Combine(Project.Workspace, "dlls");
                Directory.CreateDirectory(targetDir);
                string targetPath = Path.Combine(targetDir, Path.GetFileName(dllPath));
                try
                {
                    File.Copy(dllPath, targetPath, overwrite: true);
                   // MessageBox.Show($"DLL erfolgreich hinzugefügt: {targetPath}");
                }
                catch (Exception ex)
                {
                    Logger.FatalMsg($"[ProjectManager] Failed to add dlls: {ex.Message}");
                }
            }
        }
        public static void LoadFromAScript()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Amium Script (*.AScript)|*.AScript",
                Title = "Open AmiumScript File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string name = Path.GetFileNameWithoutExtension(dialog.FileName);

                string tempDir = Path.Combine(Path.GetTempPath(), "Project_" + name + "_" + Guid.NewGuid());
              //  string tempDir = Path.Combine(Path.GetTempPath(), "Project_" + name);
                Directory.CreateDirectory(tempDir);


                try
                {
                    ZipFile.ExtractToDirectory(dialog.FileName, tempDir);
                    string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "csharp.code-snippets");
                    string targetPath = Path.Combine(Path.Combine(tempDir, ".vscode"), "csharp.code-snippets");
                    File.Copy(sourcePath, targetPath, overwrite: true);

                    //Project.Uri = dialog.FileName;
                    //Project.Workspace = tempDir;

                    string projectJson = Path.Combine(tempDir, "project.json");
                    if (File.Exists(projectJson))
                    {
                        var project = JsonSerializer.Deserialize<ProjectData>(File.ReadAllText(projectJson));
                        Project = project;
                        Project.Workspace = tempDir; // Setze Workspace auf tempDir
                    //    MessageBox.Show($"Projekt geladen: {Project.Name}");
                    }
                    else
                    {
                        MessageBox.Show("project.json nicht gefunden!");
                    }
                    // Editor kann jetzt auf tempDir zugreifen
                    // z.B.: OpenEditor(tempDir);
                }
                catch (Exception ex)
                {
                    Logger.FatalMsg($"[ProjectManager] ZIP failed : {ex.Message}");
                }
            }
        }
        public static void SaveProject(ProjectData project)
        {
            string path = project.Workspace;
            Directory.CreateDirectory(path);
            string jsonPath = Path.Combine(path, "project.json");
            string json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonPath, json);
            SignalStorageSerializer.SaveToJson();
        }
        public static void SaveAs(string newName = null)
        {
            if(newName != null)
                CreateProject(newName);

            if (Project == null)
            {
                MessageBox.Show("No active project.");
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Amium Script (*.AScript)|*.AScript",
                DefaultExt = "AScript",
                FileName = Project.Name + ".AScript",
                Title = "Projekt save as"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveProject(Project);
                string projectPath = Project.Workspace;
                string targetFile = dialog.FileName;
                Project.Uri = targetFile;

                try
                {
                    if (File.Exists(targetFile))
                        File.Delete(targetFile);

                    ZipFile.CreateFromDirectory(projectPath, targetFile, CompressionLevel.Optimal, includeBaseDirectory: false);

                  //  MessageBox.Show($"Projekt erfolgreich gespeichert: {targetFile}");
                }
                catch (Exception ex)
                {
                    Logger.FatalMsg($"[ProjectManager] Save failed\r\n {ex.Message}");
                }
            }
        }
        public static void Save()
        {
            if (Project == null || string.IsNullOrWhiteSpace(Project.Workspace) || string.IsNullOrWhiteSpace(Project.Uri))
            {
                Logger.FatalMsg("[ProjectManager] Workspace or path not found");
                return;
            }

            try
            {
                // Projektdatei aktualisieren
                SaveProject(Project);

                // Vorherige Datei löschen, falls vorhanden
                if (File.Exists(Project.Uri))
                    File.Delete(Project.Uri);

                // Workspace als ZIP speichern
                ZipFile.CreateFromDirectory(Project.Workspace, Project.Uri, CompressionLevel.Optimal, includeBaseDirectory: false);

                Logger.InfoMsg($"[ProjectManager] Projekt saved: {Project.Uri}");
            }
            catch (Exception ex)
            {
                Logger.FatalMsg($"[ProjectManager] Fehler beim Speichern: {ex.Message}");
            }
        }

        public static bool BuildSuccess = true;

        public static void BuildProject()
        {
            StopProject();
            BuildSuccess = true;

            ProjectBuilder.GenerateDynamicProjectFile(Project.Workspace, Project.Name);

            if (!ProjectBuilder.BuildAssembly(ProjectManager.Project.Workspace))
            {
                MessageBox.Show("❌ Build failed");
                BuildSuccess = false;
                return;
            }
            var assembly = LoadedAssembly;
            var projectType = assembly.GetType("AmiumScripter.Project");

            if (assembly == null)
            {
                MessageBox.Show("❌ Assembly ist null");
                BuildSuccess = false;
                return;
            }
            if (projectType == null)
            {
                MessageBox.Show("❌ Project-Typ nicht gefunden");
                BuildSuccess = false;
                return;
            }
            GetProjectType().GetMethod("AddPagesViews", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            GetPagesFormAssembly();
            GetViewsFormAssembly();
            projectType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);

            UIEditor.PageView = Views;
            UIEditor.AttachPagesViewToUI();
            Logger.DebugMsg("[ProjectManager] BuildProject()");
            IsRunning = false;

        }

        public static bool IsRunning = false;
        public static void RunProject()
        {
            if (IsRunning) return;

            var assembly = LoadedAssembly;
            var projectType = assembly.GetType("AmiumScripter.Project");

            projectType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            Logger.DebugMsg("[ProjectManager] RunProject()");
            IsRunning = true;
        }

      
        public static void StopProject()
        {
            Logger.DebugMsg("[ProjectManager] StopProject()");
            ThreadsManager.StopAll();
            TokenManager.CancelAll();
            TasksManager.StopAll();
            TimerManager.StopAll();
            SocketManager.CloseAll();
            FileSystemWatcherManager.StopAll();
            ClassRuntimeManager.ClearAll();
            ClientManager.DestroyAll();
            UIEditor.Reset();
            //PageManager.Pages?.Clear();
            UIEditor.PageView?.Clear();

            UnloadProject();
            Logger.DebugMsg("[ProjectManager] Project stopped");
          
        }
        public static void OpenEditor()
        {
            if (ProjectManager.Project == null)
            {
                MessageBox.Show("Bitte erst ein Projekt erstellen oder laden.");
                return;
            }

            // Nutze Workspace, falls vorhanden, sonst Standardpfad
            string projectPath = !string.IsNullOrWhiteSpace(Project.Workspace)
                ? Project.Workspace
                : GetProjectPath(Project.Name);

            string workspacePath = Path.Combine(projectPath, "launch.code-workspace");

            // Baue launch.code-workspace mit absolutem Pfad
            string json = $$"""
    {
      "folders": [
        {
          "path": "{{projectPath.Replace("\\", "\\\\")}}"
        }
      ],
      "settings": {
        "window.restoreWindows": "none",
        "window.restoreFiles": "none",
        "workbench.startupEditor": "none"
      }
    }
    """;

            File.WriteAllText(workspacePath, json);

            string codeExe = Environment.ExpandEnvironmentVariables(
                @"%LocalAppData%\Programs\Microsoft VS Code\Code.exe");

            // Workspace ohne Trust-Abfrage öffnen
            string args = $"--disable-workspace-trust --new-window \"{workspacePath}\"";

         //   Process.Start(codeExe, $"--new-window \"{workspacePath}\"");
            Process.Start(codeExe, args);
        }
        static void GetPagesFormAssembly()
        {
            Pages.Clear();
        //    Logger.DebugMsg("[ProjectManager] Get Pages");
            var assembly = LoadedAssembly;


            var projectType = assembly.GetType("AmiumScripter.Project");
            var pagesDict = projectType.GetField("Pages", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as IDictionary;
          //  Logger.DebugMsg("[ProjectManager] qty Pages " + pagesDict.Count);


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
                LoadedAssemblyBytes = assemblyBytes;
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
                LoadedAssemblyBytes = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        public static Type? GetProjectType()
        {
            return LoadedAssembly?.GetType("AmiumScripter.Project");
        }
        public static void MovePage(string Page, int shift)
        {
        //    Debug.WriteLine("Move page " + Page);
            int PageIndex = Project.Pages.IndexOf(Page);
            int NewIndex = PageIndex + shift;

            if (PageIndex < 0 || PageIndex >= Project.Pages.Count || NewIndex < 0 || NewIndex >= Project.Pages.Count)
            {
                Logger.WarningMsg("[ProjectManager] MovePage Failed");
                return;
            }

            string element = Project.Pages[PageIndex];
            Project.Pages.RemoveAt(PageIndex);
            Project.Pages.Insert(NewIndex, element);

            //foreach (string p in Project.Pages)
            //    Debug.WriteLine(p);
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
            string projectRoot = ProjectManager.Project.Workspace;
            string projectDlls = Path.Combine(projectRoot, "dlls");
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

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

            // WICHTIG: Alle C#-Dateien wie in deinem Build einbinden
            // (wildcards decken Unterordner ab)
            var compileItems = $@"
    <Compile Include=""project.cs"" />
    <Compile Include=""Shared\SignalPool.cs"" />
    <Compile Include=""Shared\Classes\**\*.cs"" />
    <Compile Include=""Pages\**\*.cs"" />";

            return $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <EnableDefaultItems>false</EnableDefaultItems>
  </PropertyGroup>

  <ItemGroup>
{compileItems}
  </ItemGroup>

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
                Logger.DebugMsg("[ProjectBuilder] BuildAssembly Try to Build: " + projectPath);

                string projectRoot = projectPath;
                string sharedClassesRoot = Path.Combine(projectRoot, "Shared", "Classes");
 
                string customDllsRoot = Path.Combine(projectRoot, "dlls");

                Logger.DebugMsg("[ProjectBuilder] projectRoot:          " + projectRoot);
                Logger.DebugMsg("[ProjectBuilder] sharedClassesRoot:    " + sharedClassesRoot);
                Logger.DebugMsg("[ProjectBuilder] customDllsRoot:       " + customDllsRoot);

                string pagesFolder = Path.Combine(projectPath, "Pages");

                List<string> pages = Directory.GetDirectories(pagesFolder, "*", SearchOption.TopDirectoryOnly).ToList();

                Logger.DebugMsg("[ProjectBuilder] Qty pages:       " + pages.Count);

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
                    Logger.FatalMsg("[PageRuntime] project.cs not found: " + projectCsPath);

                //SignalPool
                string sharedSignalPool = Path.Combine(projectRoot, "Shared", "SignalPool.cs");
                if (File.Exists(sharedSignalPool))
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(CheckCode(sharedSignalPool), path: sharedSignalPool));
                else
                    Logger.FatalMsg("[PageRuntime] SignalPool.cs not found: " + sharedSignalPool);


                foreach (var tree in syntaxTrees)
                    Logger.DebugMsg("[Build] SyntaxTree: " + tree.FilePath);


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

                Logger.DebugMsg("[ProjectBuilder] reading custom dll folder: " + customDllsRoot);
                // Benutzerdefinierte DLLs aus dem "dlls"-Ordner einbinden (immer aktuell)
                if (Directory.Exists(customDllsRoot))
                {
                    foreach (var dll in Directory.GetFiles(customDllsRoot, "*.dll"))
                    {
                        Logger.DebugMsg("[ProjectBuilder] Add custom dll:" +dll);// Alte Referenzen mit identischem Pfad entfernen
                        references.RemoveAll(r =>
                            string.Equals(r.Display, dll, StringComparison.OrdinalIgnoreCase));

                        // Neue Referenz hinzufügen
                        references.Add(MetadataReference.CreateFromFile(dll));
                    }
                }
                else
                {
                    Logger.FatalMsg("Dll Folder not found: " + customDllsRoot);
                }

                foreach (var r in references)
                    Logger.DebugMsg("[ProjectBuilder] Reference: " + r.Display);


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
                    Logger.FatalMsg("Build-Fehler:");
                    foreach (var d in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                        Logger.FatalMsg($"  ➤ {d}");
                    return false;
                }

                ms.Seek(0, SeekOrigin.Begin);

                byte[] asmBytes = ms.ToArray(); // aus MemoryStream nach Roslyn-Emit
                ProjectManager.BuildAndLoadProject(asmBytes, ProjectManager.Project.Name);

                Logger.DebugMsg("[PageRuntime] BuildAssembly successful.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.DebugMsg("[PageRuntime] BuildAssembly Failed " + ex.Message);
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
                Logger.DebugMsg($"[WARN] {file} contains direct unmanaged CancellationToken usage! Please use 'AToken()' instead.");
            // Thread
            var reThread = new Regex(@"new\s+Thread\b|System\.Threading\.Thread\b", RegexOptions.IgnoreCase);
            if (reThread.IsMatch(code))
                Logger.DebugMsg($"[WARN] {file} contains direct unmanaged Thread usage! Please use 'AThread' instead.");

            // Task
            var reTask = new Regex(@"new\s+Task\b|System\.Threading\.Tasks\.Task\b", RegexOptions.IgnoreCase);
            if (reTask.IsMatch(code))
                Logger.DebugMsg($"[WARN] {file} contains direct unmanaged Task usage! Please use 'ATask' or your own task registry instead.");

            // Sleep
            //var reSleep = new Regex(@"\bSleep\s*\(|System\.Threading\.Thread\.Sleep\b", RegexOptions.IgnoreCase);
            //if (reSleep.IsMatch(code))
            //    Logger.Log($"[WARN] {file} contains direct unmanaged Sleep usage! Please use 'ASleep' instead.");

            // while(true)
            var reWhile = new Regex(@"while\s*\(\s*true\s*\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (reWhile.IsMatch(code))
                Logger.DebugMsg($"[WARN] {file} contains 'while(true)'! Please use a cancellation token or IsRunning flag for exit condition.");

            // for(;;)
            var reFor = new Regex(@"for\s*\(\s*;\s*;\s*\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (reFor.IsMatch(code))
                Logger.DebugMsg($"[WARN] {file} contains 'for(;;)'! Please use a cancellation token or IsRunning flag for exit condition.");

            return code;
        }




    }











}
