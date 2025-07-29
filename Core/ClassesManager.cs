using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{
    public static class ClassManager
    {
        public static void AddClass(string projectName, string className, string page = null)
        {
            if (!className.StartsWith("Class_"))
                className = "Class_" + className;

            if (!Regex.IsMatch(className, @"^[A-Z][A-Za-z0-9_]*$"))
            {
                MessageBox.Show("❌ Ungültiger Klassenname. Muss mit Großbuchstaben beginnen und darf nur Buchstaben/Zahlen/_ enthalten.");
                return;
            }

            string classPath = string.Empty;
            string nameSpace = page == null
                ? "AmiumScripter.Shared"
                : $"AmiumScripter.Pages.{page}";

            if (page == null)
            {
                classPath = Path.Combine(ProjectManager.GetProjectPath(projectName), "Shared", "Classes");
            }
            else
            {
                classPath = Path.Combine(ProjectManager.GetProjectPath(projectName), page, "Classes");
            }
            Directory.CreateDirectory(classPath); // Falls Projekt noch nicht existiert


            string classCode = $@"
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

namespace {nameSpace}
{{
    public class {className} : ClassBase
    {{
        public {className}(string instanceName) : base(instanceName)
        {{
            // Optional: Initialisierungscode
            Console.WriteLine(""[{className}] Initialized with name: "" + InstanceName);
        }}

        public override void Destroy()
        {{
            Console.WriteLine(""[{className}] Destroyed: "" + InstanceName);
        }}
    }}
}}";


            string filePath = Path.Combine(classPath, className + ".cs");
            File.WriteAllText(filePath, classCode);

            ProjectManager.Project.Modules.Add(new ModuleData
            {
                Name = className,
                Enabled = true
            });
            ProjectManager.SaveProject(ProjectManager.Project);


            MessageBox.Show($"✅ Modul '{className}' wurde im Projekt '{projectName}' erstellt.");
        }

        public static string[] ListClasses(string projectName)
        {
            string path = ProjectManager.GetProjectPath(projectName);
            return Directory.Exists(path)
                ? Directory.GetFiles(path, "*.cs")
                : Array.Empty<string>();
        }

        public static IClass? LoadClass(string projectName, string className)
        {
            string modulesPath = ProjectManager.GetProjectPath(projectName);
            string filePath = Directory.GetFiles(modulesPath, className + ".cs", SearchOption.AllDirectories)
                                       .FirstOrDefault();

            if (filePath == null || !File.Exists(filePath))
            {
                MessageBox.Show($"❌ Modul '{className}' nicht gefunden.");
                return null;
            }

            string sourceCode = File.ReadAllText(filePath);

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var references = new[]
            {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IClass).Assembly.Location)
        };

            var compilation = CSharpCompilation.Create(
                "DynamicModule",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var errors = string.Join("\n", result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString()));
                MessageBox.Show($"❌ Kompilierungsfehler im Modul:\n{errors}");
                return null;
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            var moduleType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IClass).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (moduleType == null)
            {
                MessageBox.Show("⚠️ Kein passender `IClass`-Typ gefunden.");
                return null;
            }

            var instance = Activator.CreateInstance(moduleType) as IClass;


            if (instance != null)
            {
                ClassRuntimeManager.Register(instance); // 🔄 Zentrale Zerstörungslogik registrieren
                                                        // ✅ Initialisieren wie bisher
                MessageBox.Show($"📦 Modul '{className}' geladen");
            }
            return instance;
        }

    }
    public static class ClassRuntimeManager
    {
        private static readonly List<Action> _onDestroyCallbacks = new();

        public static void Register(IClass instance)
        {
            _onDestroyCallbacks.Add(() =>
            {
                instance.Destroy();
                Debug.WriteLine($"Destroyed: {instance.InstanceName}");
            });
        }

        public static void ClearAll()
        {
            foreach (var destroy in _onDestroyCallbacks)
                destroy();

            _onDestroyCallbacks.Clear();
        }
    }
}
