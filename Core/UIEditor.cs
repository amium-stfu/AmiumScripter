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
            CurrentPageName = null;
            Debug.WriteLine("[UIEditor] Try to Add PageViews");
            AmiumScripter.Root.Main.Book.TabPages.Clear();

            foreach (var view in PageView)
            {
                Debug.WriteLine("[UIEditor] Add PageView " + view.Key);
                view.Value.Invalidate();
                view.Value.Dock = DockStyle.Fill;
                view.Value.BackColor = Color.White;

                var tabPage = new TabPage(view.Key);
                tabPage.Name = view.Key;
                CurrentPageName??= tabPage.Name;
                tabPage.Controls.Add(view.Value);
                AmiumScripter.Root.Main.Book.TabPages.Add(tabPage);

            }

           // MessageBox.Show($"Aktuelle Seite: {UIEditor.CurrentPageName}");
        }

        //BuildView End


        // EditControls.cs-----
        public static void UpdateControlsCs(string filePath, List<string> controlInitLines, string controlName, string controlType)
        {
            var beginDeclTag = "//#BEGIN-AUTO-ADD-CONTROLS";
            var endDeclTag = "//#END-AUTO-ADD-CONTROLS";
            var beginInitTag = "//#BEGIN-AUTO-GENERATED";
            var endInitTag = "//#END-AUTO-GENERATED";

            var originalLines = File.ReadAllLines(filePath).ToList();
            var output = new List<string>();

            bool insideDeclBlock = false;
            bool insideInitBlock = false;
            bool controlAddedToDecl = false;
            bool controlAddedToInit = false;

            foreach (var line in originalLines)
            {
                if (line.Contains(beginDeclTag))
                {
                    insideDeclBlock = true;
                    output.Add(line);
                    continue;
                }
                if (line.Contains(endDeclTag))
                {
                    // Add new control declaration before closing tag, if not already present
                    if (!originalLines.Any(l => l.Contains($"public static {controlType} {controlName};")))
                    {
                        output.Add($"        public static {controlType} {controlName};");
                    }
                    insideDeclBlock = false;
                    output.Add(line);
                    continue;
                }

                if (insideDeclBlock)
                {
                    // Keep existing declarations
                    output.Add(line);
                    continue;
                }

                if (line.Contains(beginInitTag))
                {
                    insideInitBlock = true;
                    output.Add(line);
                    continue;
                }

                if (line.Contains(endInitTag))
                {
                    // Add new control initialization before closing tag, if not already present
                    if (!originalLines.Any(l => l.Contains($"{controlName} = new {controlType}")))
                    {
                        output.Add($"            {controlName} = new {controlType}");
                        output.Add("            {");
                        foreach (var ctrlLine in controlInitLines)
                            output.Add("                " + ctrlLine);
                        output.Add("            };");
                        output.Add($"            parent.Controls.Add({controlName});");
                        output.Add($"            Logger.DebugMsg(\"[PageControls] Creating instance of SignalView  {controlName}\");");
                        output.Add("            // ----");
                    }
                    insideInitBlock = false;
                    output.Add(line);
                    continue;
                }

                if (insideInitBlock)
                {
                    // Keep existing initializations
                    output.Add(line);
                    continue;
                }

                output.Add(line);
            }

            File.WriteAllLines(filePath, output);
        }
        public static bool ControlExists(string name, string page)
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
            if (!name.StartsWith("Ctr"))
                name = "Ctr" + name;

            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");

            if (!File.Exists(filePath))
                return false;

            // Suche nach Feld-Deklaration mit beliebigem Typ, aber passendem Namen
            return File.ReadAllLines(filePath)
                       .Any(line => line.Contains($" {name};"));
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

            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            var controlLines = AddSignalControlLines(name, source);
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "SignalView");
        }
        static List<string> AddSignalControlLines(string name, string source)
        {
            return new List<string>
    {

        $"    Name = \"{name}\",",
        $"    Location = new Point(10, 10),",
        $"    Size = new Size(200, 100),",
        $"    BorderColor = Color.Black,",
        $"    SignalText = \"{name.Replace("Ctr","")}\",",
        $"    SignalUnit = \"udef\",",
        $"    SignalValue = \"udef\",",
        $"    SourceName = \"{source}\"",

    };
        }

        // SignalControl End-----

        // StringSignalControl-----
        public static void AddStringSignalControl(string name, string page, string source = "")
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
            if (!name.StartsWith("Ctr"))
                name = "Ctr" + name;

            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            var controlLines = AddStringSignalControlLines(name, source);
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "StringSignalView");
        }
        static List<string> AddStringSignalControlLines(string name, string source)
        {
            return new List<string>
    {
        $"    Name = \"{name}\",",
        $"    Location = new Point(10, 10),",
        $"    Size = new Size(200, 100),",
        $"    BorderColor = Color.Black,",
        $"    SignalText = \"{name.Replace("Ctr","")}\",",
        $"    SignalValue = \"udef\",",
        $"    SourceName = \"{source}\"",

    };
        }

        // SignalControl End-----

        public static void UpdateControlPosition(string page, string controlName, string controlType, int x, int y)
        {
            Debug.WriteLine("Update Control " + controlName);
            Debug.WriteLine("Type " + controlType);
            Debug.WriteLine("X " + x);            
            Debug.WriteLine("Y " + y);
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
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

                if (!insideTargetBlock && trimmed.StartsWith($"{controlName} = new {controlType}"))
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
