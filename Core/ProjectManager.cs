using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{

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

            File.WriteAllText(Path.Combine(path, ".vscode","settings.json"), vsSetting);



            Project = new ProjectData(projectName)
            {
                Autor = Environment.UserName
            };

            SaveProject(Project);
            ProjectBuilder.GenerateDynamicProjectFile(path, projectName);
            MessageBox.Show($"📁 Projekt '{projectName}' mit Referenzen angelegt: {path}");
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
        }

        public static void BuildProject()
        {
            ClassRuntimeManager.ClearAll();
            ThreadsManager.StopAll();
            PageRuntimeManager.StopAllPages();

            foreach (string pagePath in Project.Pages)
            {
                Debug.WriteLine($"🛠️ Build Page: {pagePath}");
                PageRuntimeManager.BuildPageThreaded(pagePath);
            }
            PageRuntimeManager.BuildAllPages();
        }

        public static void RunProject()
        {
            Debug.WriteLine("▶️ RunProject(): Alle Pages starten.");
            PageRuntimeManager.RunAllPages();
        }

        public static void StopProject()
        {
            Debug.WriteLine("⏹️ StopProject(): Alle Pages stoppen.");
            PageRuntimeManager.StopAllPages();
            ThreadsManager.StopAll();
            ClassRuntimeManager.ClearAll();
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
  </PropertyGroup>
  <ItemGroup>
{string.Join("\n", referenceItems)}
  </ItemGroup>
</Project>";
        }




    }











}
