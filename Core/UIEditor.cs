using AmiumScripter.Modules;
using Microsoft.CodeAnalysis;
using AmiumScripter.Controls;
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
        static Panel EditorPanel = new();

        public static string CurrentPageName { get; set; } = "TestPage";

        public static Dictionary<string, BaseView> PageView = new();
        //BuildView

        public static (int X, int Y, int H, int W) NewControl;

        public static void CreateAllViews()
        {
            Debug.WriteLine("[UIEditor] Start creative pageviews");
            Debug.WriteLine("[UIEditor] qty of pages " + PageView.Count);
            AttachPagesViewToUI();
        }

        public static bool EditMode { get; set; } = false;
        public static void Reset()
        {
            foreach (BaseView view in PageView.Values)
                view.Destroy();
            PageView.Clear();
           
        }

        public static void AttachPagesViewToUI()
        {
            CurrentPageName = null;
            Debug.WriteLine("[UIEditor] Try to Add PageViews");

            AmiumScripter.Root.Main.DeletePages();
            int c = ProjectManager.Project.Pages.Count + 1;
            foreach(string page in ProjectManager.Project.Pages.AsEnumerable().Reverse())
            {
                c--;
                var view = PageView[page];
                view.Name = page;
                view.Invalidate();
                view.Dock = DockStyle.Fill;
                view.BackColor = Color.White;
                string text = view.PageText;
                view.BorderStyle = BorderStyle.FixedSingle;
                AmiumScripter.Root.Main.AddPageToUI(text, view, c);
            }
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
        public static void AddSignalControl(string name, string page, string source = "", int x = 0, int y = 0, int h = 100, int w = 200)
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
         
            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            PageView[page].Controls.Add(
                new SignalView()
                {
                    Name = name,
                    Location = new Point(x, y),
                    Size = new Size(w, h),
                    BorderColor = Color.Black,
                    SignalText = name,
                    SignalUnit = "udef",
                    SignalValue = "udef",
                    SourceName = source
                }
                );

            var controlLines = new List<string>
    {

        $"    Name = \"{name}\",",
        $"    Location = new Point({x}, {y}),",
        $"    Size = new Size({w}, {h}),",
        $"    BorderColor = Color.Black,",
        $"    SignalText = \"{name}\",",
        $"    SignalUnit = \"udef\",",
        $"    SignalValue = \"udef\",",
        $"    SourceName = \"{source}\"",

    };
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "SignalView");
        }
        public static void AddModuleControl(string name, string page, string source = "", int x = 0, int y = 0, int h = 100, int w = 200)
        {
         
            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            PageView[page].Controls.Add(
              new ModuleView()
              {
                  Name = name,
                  Location = new Point(x, y),
                  Size = new Size(w, h),
                  BorderColor = Color.Black,
                  SignalText = name,
                  SignalUnit = "udef",
                  SignalValue = "udef",
                  SourceName = source
              }
              );


            var controlLines = new List<string>
    {

        $"    Name = \"{name}\",",
        $"    Location = new Point({x}, {y}),",
        $"    Size = new Size({w}, {h}),",
        $"    BorderColor = Color.Black,",
        $"    SignalText = \"{name}\",",
        $"    SignalUnit = \"udef\",",
        $"    SignalValue = \"udef\",",
        $"    SourceName = \"{source}\"",

    };
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "ModuleView");
        }
        public static void AddStringSignalControl(string name, string page, string source = "", int x = 0, int y = 0, int h = 100, int w = 200)
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            PageView[page].Controls.Add(
              new StringSignalView()
              {
                  Name = name,
                  Location = new Point(x, y),
                  Size = new Size(w, h),
                  BorderColor = Color.Black,
                  SignalText = name,
                  SignalValue = "udef",
                  SourceName = source
              }
              );

            var controlLines = new List<string>
    {
        $"    Name = \"{name}\",",
        $"    Location = new Point({x}, {y}),",
        $"    Size = new Size({w}, {h}),",
        $"    BorderColor = Color.Black,",
        $"    SignalText = \"{name.Replace("Ctr","")}\",",
        $"    SignalValue = \"udef\",",
        $"    SourceName = \"{source}\"",

    };
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "StringSignalView");
        }
        public static void AddSimpleButtonControl(string name, string text, string page, int x, int y, int h, int w)
        {

            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            PageView[page].Controls.Add(
              new SimpleButton()
              {
                  Name = name,
                  Location = new Point(x, y),
                  Size = new Size(w, h),
                  BorderColor = Color.Black,
                  SignalText = name,
                  SignalValue = "udef"
              }
              );

            var controlLines = new List<string>
    {
        $"    Name = \"{name}\",",
        $"    Location = new Point({x}, {y}),",
        $"    Size = new Size({w}, {h}),",
        $"    BorderColor = Color.Black,",
        $"    SignalText = \"{name.Replace("Ctr","")}\",",
        $"    SignalValue = \"{text}\",",
    };
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "SimpleButton");
        }
        public static void AddIconButtonControl(string name, string text, string page, int x, int y, int h, int w)
        {
            name = name.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
        

            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            PageView[page].Controls.Add(
              new IconButton()
              {
                  Name = name,
                  Location = new Point(x, y),
                  Size = new Size(w, h),
                  BorderColor = Color.Black,
              }
              );

            var controlLines = new List<string>
    {
        $"    Name = \"{name}\",",
        $"    Location = new Point({x}, {y}),",
        $"    Size = new Size({w}, {h}),",
        $"    BorderColor = Color.Black,",
        $"    ButtonText = \"{text}\",",
    };
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "IconButton");
        }


        public static void AddChart(string name, string text, string page, int x, int y, int h, int w)
        {

            if (ControlExists(name, page))
            {
                MessageBox.Show($"❌ A control with the name '{name}' already exists in the file {page}/Controls.cs");
                return;
            }

            PageView[page].Controls.Add(
              new Chart()
              {
                  Name = name,
                  Location = new Point(x, y),
                  Size = new Size(w, h),
                  BorderColor = Color.Black,
              }
              );

            var controlLines = new List<string>
    {
        $"    Name = \"{name}\",",
        $"    Location = new Point({x}, {y}),",
        $"    Size = new Size({w}, {h}),",
        $"    BorderColor = Color.Black,",
    };
            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            UpdateControlsCs(filePath, controlLines, name, "Chart");
        }

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

        public static void UpdateControlSize(string page, string controlName, string controlType, int width, int height)
        {
            Debug.WriteLine("Update Control Size " + controlName);
            Debug.WriteLine("Type " + controlType);
            Debug.WriteLine("W " + width);
            Debug.WriteLine("H " + height);

            var filePath = Path.Combine(ProjectManager.Project.Workspace, "Pages", page, "Controls.cs");
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("File not found");
                return;
            }

            var lines = File.ReadAllLines(filePath).ToList();
            var output = new List<string>();

            bool insideTargetBlock = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (!insideTargetBlock && trimmed.StartsWith($"{controlName} = new {controlType}"))
                {
                    insideTargetBlock = true;
                }

                if (insideTargetBlock)
                {
                    string property = line.Replace(" ", "");
                    if (property.StartsWith("Size"))
                    {
                        output.Add($"                    Size = new Size({width}, {height}),");
                    }
                    else
                    {
                        output.Add(line);
                    }

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
