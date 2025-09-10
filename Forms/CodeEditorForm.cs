using AmiumScripter.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax; // added for class extraction
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.Build.Locator;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoslynDocument = Microsoft.CodeAnalysis.Document;
using RoslynWorkspace = Microsoft.CodeAnalysis.Workspace;
using System.Runtime.InteropServices;
using AmiumScripter.Helpers;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Rename; // hinzugefügt für Rename


namespace AmiumScripter.Forms
{
    public partial class CodeEditorForm : Form
    {
        private readonly System.Windows.Forms.Timer _diagnosticTimer = new() { Interval = 500 };
        private readonly List<Diagnostic> _currentDiagnostics = new();
        private readonly ToolTip _tooltip = new();
        private int _docVersion = 0;
        private string? _currentFile;

        private const int SCI_TOGGLEFOLD = 2231;
        private const int SCI_FOLDALL = 2662;
        private const int SC_FOLDACTION_CONTRACT = 0;
        private const int SC_FOLDACTION_EXPAND = 1;
        private const int SC_FOLDACTION_TOGGLE = 2;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);


        #region C# Highlighting (ScintillaNET 6, Lexer=cpp)

        private enum EditorTheme { Dark, Light }

        private void ApplyCSharpHighlighting(EditorTheme theme = EditorTheme.Dark)
        {
            // 1) Lexer
            Editor.LexerName = "cpp"; // C#, C, C++ teilen sich den CPP-Lexer

            // 2) Keywords
            // Word list 0 = Hauptkeywords
            Editor.SetKeywords(0, string.Join(" ",
                "abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while var dynamic record nint nuint"));

            // Word list 1 = Kontext-Keywords
            Editor.SetKeywords(1, string.Join(" ",
                "add alias ascending async await by descending from get global group init into join let nameof not notnull on orderby partial remove select set unmanaged value when where with yield"));

            // Word list 3 = Präprozessor (für #region/#if/…)
            Editor.SetKeywords(3, string.Join(" ",
                "if elif else endif define undef warning error line region endregion pragma nullable"));

            // 3) Grundstyling
            Editor.StyleResetDefault();
            Editor.Styles[Style.Default].Font = "Consolas";
            Editor.Styles[Style.Default].Size = 11;
            Editor.StyleClearAll();

            // Paletten
            Color back, fore, accent, comment, number, stringCol, preproc, typeCol, opCol, classes;
            if (theme == EditorTheme.Dark)
            {
                back = Color.FromArgb(0x1E, 0x1E, 0x1E);
                fore = Color.FromArgb(0xD4, 0xD4, 0xD4);
                accent = Color.FromArgb(0x56, 0x9C, 0xD6); // keywords
                comment = Color.FromArgb(0x6A, 0x99, 0x4E);
                number = Color.FromArgb(0xB5, 0xCE, 0xA8);
                stringCol = Color.FromArgb(0xCE, 0x91, 0x78);
                preproc = Color.FromArgb(0xC5, 0x86, 0xC0);
                typeCol = Color.FromArgb(0x4E, 0xC9, 0xB0);
                opCol = Color.FromArgb(0xD4, 0xD4, 0xD4);
                classes = Color.Orange;
            }
            else // Light
            {
                back = Color.White;
                fore = Color.Black;
                accent = Color.FromArgb(0x00, 0x37, 0xDA);
                comment = Color.FromArgb(0x00, 0x80, 0x00);
                number = Color.FromArgb(0x09, 0x56, 0x00);
                stringCol = Color.FromArgb(0xA3, 0x15, 0x15);
                preproc = Color.FromArgb(0x7A, 0x3E, 0x9D);
                typeCol = Color.FromArgb(0x2B, 0x91, 0xAF);
                opCol = Color.FromArgb(0x00, 0x00, 0x00);
                classes = Color.Orange;
            }

            Editor.Styles[Style.Default].BackColor = back;
            Editor.Styles[Style.Default].ForeColor = fore;
            Editor.CaretForeColor = fore;
            Editor.CaretLineVisible = true;
            Editor.CaretLineBackColor = theme == EditorTheme.Dark
                ? Color.FromArgb(0x22, 0x22, 0x22) : Color.FromArgb(0xF3, 0xF3, 0xF3);

            // 4) CPP-Styles mappen (funktioniert auch für C#)
            Editor.Styles[Style.Cpp.Default].ForeColor = fore;
            Editor.Styles[Style.Cpp.Comment].ForeColor = comment;
            Editor.Styles[Style.Cpp.CommentLine].ForeColor = comment;
            Editor.Styles[Style.Cpp.CommentDoc].ForeColor = comment;
            Editor.Styles[Style.Cpp.Number].ForeColor = number;
            Editor.Styles[Style.Cpp.Word].ForeColor = accent;   // keywords (SetKeywords 0)
            Editor.Styles[Style.Cpp.Word2].ForeColor = typeCol;  // kontext keywords (SetKeywords 1)
            Editor.Styles[Style.Cpp.String].ForeColor = stringCol;
            Editor.Styles[Style.Cpp.Character].ForeColor = stringCol;
            Editor.Styles[Style.Cpp.Preprocessor].ForeColor = preproc;  // #region etc.
            Editor.Styles[Style.Cpp.Operator].ForeColor = opCol;
            Editor.Styles[Style.Cpp.Identifier].ForeColor = fore;
            Editor.Styles[Style.Cpp.GlobalClass].ForeColor = fore;


            // 5) Sichtbare Hilfen
            Editor.WhitespaceSize = 2;
            Editor.ViewWhitespace = WhitespaceMode.Invisible;
            Editor.IndentationGuides = IndentView.LookBoth;

            // 6) Folding einschalten (für { } / #region)
            Editor.SetProperty("fold", "1");
            Editor.SetProperty("fold.compact", "1");
            Editor.SetProperty("fold.preprocessor", "1");

            // Margin für Folding (links)
            const int MARGIN_FOLD = 2;
            Editor.Margins[MARGIN_FOLD].Type = MarginType.Symbol;
            Editor.Margins[MARGIN_FOLD].Mask = Marker.MaskFolders;
            Editor.Margins[MARGIN_FOLD].Sensitive = true;
            Editor.Margins[MARGIN_FOLD].Width = 16;

            // Folder-Marker stylen
            Editor.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            Editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            Editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            Editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            Editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            Editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            Editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            var foldFore = theme == EditorTheme.Dark ? Color.FromArgb(0x80, 0x80, 0x80) : Color.FromArgb(0x60, 0x60, 0x60);
            var foldBack = back;
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpenMid; i++)
            {
                Editor.Markers[i].SetForeColor(foldFore);
                Editor.Markers[i].SetBackColor(foldBack);
            }

            // 7) Klammern-Paar-Highlight (optional)

            Editor.Indicators[1].Style = IndicatorStyle.StraightBox;
            Editor.Indicators[1].Under = true;
            Editor.Indicators[1].ForeColor = theme == EditorTheme.Dark ? Color.FromArgb(0x66, 0xCC, 0xFF) : Color.FromArgb(0x00, 0x60, 0xB0);
        }
        
        #endregion


        private readonly HashSet<string> _keywords = new(StringComparer.Ordinal)
        {
            "abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue",
            "decimal","default","delegate","do","double","else","enum","event","explicit","extern","false","finally",
            "fixed","float","for","foreach","goto","if","implicit","in","int","interface","internal","is","lock",
            "long","namespace","new","null","object","operator","out","override","params","private","protected",
            "public","readonly","ref","return","sbyte","sealed","short","sizeof","stackalloc","static","string",
            "struct","switch","this","throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort",
            "using","virtual","void","volatile","while","var","record","nint","nuint","when","with","init","global","AmiumScripter","AmiumScripter.Core"
        };

        // Namespace-Index (für Vorschläge bei System.Drawing. usw.)
        private readonly Dictionary<string, HashSet<string>> _nsChildren = new(StringComparer.Ordinal);
        private bool _nsIndexBuilt = false;

        public CodeEditorForm()
        {
            InitializeComponent();
            InitEditor();
            InitEvents();
            InitEditorContextMenu(); // neues Kontextmenü
            Task.Run(BuildNamespaceIndex);

            _tooltip.InitialDelay = 0;
            _tooltip.ReshowDelay = 0;
            _tooltip.AutoPopDelay = 10000;
            _tooltip.ShowAlways = true;
            // Optional hübscher:
            //  _tooltip.IsBalloon = true;

            Editor.MouseDwellTime = 400;

            Editor.DwellStart += (s, e) =>
            {
                if (e.Position < 0) return;

                // Diagnostic finden, dessen Span die Position enthält
                var hit = _currentDiagnostics.FirstOrDefault(d =>
                {
                    if (!d.Location.IsInSource) return false;
                    var sp = d.Location.SourceSpan;
                    return e.Position >= sp.Start && e.Position < sp.Start + Math.Max(1, sp.Length);
                });

                if (hit == null) return;

                ShowTooltipAbovePosition(e.Position, hit.GetMessage());
            };

            Editor.DwellEnd += (s, e) => _tooltip.Hide(Editor);

            this.Shown += async (_, __) =>
            {
                try
                {
                    // a) Pfade + Name
                    string root = ProjectManager.Project.Workspace;
                    string name = ProjectManager.Project.Name ?? "DynamicProject";
                    string csprojPath = Path.Combine(root, $"{name}.csproj");

                    // b) csproj NEU erzeugen (du hast GenerateCsprojWithAbsolutePaths bereits angepasst!)
                    ProjectBuilder.GenerateDynamicProjectFile(root, name);

                    // c) MSBuildWorkspace auf das erzeugte csproj setzen
                    await WorkspaceService.InitializeAsync(csprojPath);

                    // d) (Optionaler Fallback) Adhoc-Workspace mit denselben DLLs füttern
                    string dllDir = Path.Combine(root, "dlls");
                    IntelliSenseService.RegisterReferenceDirectory(dllDir, recursive: true);
                    IntelliSenseService.RegisterReference(typeof(IPage).Assembly.Location);

                    // e) kleine Sichtkontrolle
                    this.Text = $"Editor – {(WorkspaceService.IsReady ? "MSBuild" : "Adhoc")} Workspace";
                }
                catch (Exception ex)
                {
                    // MessageBox.Show("Workspace init failed: " + ex.Message);
                }
            };

            LoadProjectTree();
        }

        private void ShowTooltipAbovePosition(int pos, string text)
        {
            // Pixelkoordinaten der Textposition
            int x = Editor.PointXFromPosition(pos);
            int y = Editor.PointYFromPosition(pos);

            // DPI-skalierter, fixer Offset nach oben (ca. 24 px @96dpi)
            int offset = DpiScaleY(24);

            // Grenzen einhalten
            int clampedX = Math.Max(0, Math.Min(x, Editor.ClientSize.Width - DpiScaleX(20)));
            int clampedY = Math.Max(0, y - offset);

            // Vorherigen Tooltip schließen, damit er sauber „umzieht“
            _tooltip.Hide(Editor);
            _tooltip.Show(text, Editor, new Point(clampedX, clampedY), 8000);
        }

        private int DpiScaleY(int px)
        {
            using var g = Editor.CreateGraphics();
            return (int)Math.Round(px * (g.DpiY / 96f));
        }
        private int DpiScaleX(int px)
        {
            using var g = Editor.CreateGraphics();
            return (int)Math.Round(px * (g.DpiX / 96f));
        }

        private void Editor_UpdateUI_BraceMatch(object? sender, UpdateUIEventArgs e)
        {
            // Nur reagieren, wenn sich die Auswahl/Position geändert hat
            if ((e.Change & UpdateChange.Selection) == 0)
                return;

            int caret = Editor.CurrentPosition;

            // Kandidat links vom Caret
            int bracePos1 = -1;
            if (caret > 0 && IsBrace(Editor.GetCharAt(caret - 1)))
                bracePos1 = caret - 1;
            // sonst Kandidat unter dem Caret
            else if (IsBrace(Editor.GetCharAt(caret)))
                bracePos1 = caret;

            if (bracePos1 >= 0)
            {
                int bracePos2 = Editor.BraceMatch(bracePos1);
                if (bracePos2 != -1)
                {
                    // gültiges Paar
                    Editor.BraceHighlight(bracePos1, bracePos2);
                }
                else
                {
                    // ungültige Klammer
                    Editor.BraceBadLight(bracePos1);
                }
            }
            else
            {
                // alles löschen
                Editor.BraceHighlight(-1, -1);
            }
        }

        private static bool IsBrace(int ch)
        {
            char c = (char)ch;
            // Für C# reichen i. d. R. diese drei Paare
            return c == '(' || c == ')' ||
                   c == '[' || c == ']' ||
                   c == '{' || c == '}';
        }

        private const int SCI_AUTOCSETIGNORECASE = 2114;
        private const int SCI_AUTOCSETAUTOHIDE = 2111;
        private void InitEditor()
        {
            Editor.Margins[0].Width = 40; // Breite in Pixeln

            // Optional: Stil für die Zeilennummern anpassen
            Editor.Styles[Style.LineNumber].ForeColor = Color.Gray;
            Editor.Styles[Style.LineNumber].BackColor = Color.White;
            Editor.Styles[Style.LineNumber].Font = "Consolas";

            // ScintillaNET 6.x: kein Lexer-Enum mehr
            Editor.LexerName = "cpp"; // C#-ähnlich
            Editor.WrapMode = WrapMode.None;
            Editor.IndentWidth = 4;
            Editor.TabWidth = 4;
            Editor.UseTabs = false;

            // Folding aktivieren (für { } und #region)
            Editor.SetProperty("fold", "1");
            Editor.SetProperty("fold.compact", "1");
            Editor.SetProperty("fold.preprocessor", "1");

            // Automatik: Marker anzeigen, per Klick falten, bei Textänderung aktualisieren
            Editor.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;

            // Fold-Margin links (ID muss zu deinem bestehenden MARGIN_FOLD passen)
            const int MARGIN_FOLD = 2;
            Editor.Margins[MARGIN_FOLD].Type = MarginType.Symbol;
            Editor.Margins[MARGIN_FOLD].Mask = Marker.MaskFolders;
            Editor.Margins[MARGIN_FOLD].Sensitive = true;
            Editor.Margins[MARGIN_FOLD].Width = 16;

            // Folder-Marker (falls noch nicht gesetzt)
            Editor.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            Editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            Editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            Editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            Editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            Editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            Editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;


            // 0 = Errors (hast du)
            Editor.Indicators[0].Style = IndicatorStyle.Squiggle;
            Editor.Indicators[0].ForeColor = Color.Red;
            Editor.Indicators[0].Under = true;

            // 2..9 = Semantik-Overlay (Textfarbe oder Unterstreichung)
            Editor.Indicators[2].Style = IndicatorStyle.TextFore;   // Methoden
            Editor.Indicators[2].Under = true;
            Editor.Indicators[2].ForeColor = Color.DarkOrange.ChangeBrightness(-20);

            Editor.Indicators[3].Style = IndicatorStyle.TextFore;    // Klassen
            Editor.Indicators[3].ForeColor = Color.DarkViolet;

            Editor.Indicators[4].Style = IndicatorStyle.TextFore;    // Interfaces
            Editor.Indicators[4].ForeColor = Color.Red;

            Editor.Indicators[5].Style = IndicatorStyle.TextFore;    // Structs
            Editor.Indicators[5].ForeColor = Color.Firebrick;

            Editor.Indicators[6].Style = IndicatorStyle.TextFore;    // Enums
            Editor.Indicators[6].ForeColor = Color.FromArgb(0xCE, 0x91, 0x78);

            Editor.Indicators[7].Style = IndicatorStyle.TextFore;    // Delegates
            Editor.Indicators[7].ForeColor = Color.FromArgb(0xD4, 0xD4, 0xD4);


            Editor.Indicators[8].Style = IndicatorStyle.TextFore;    // Properties
            Editor.Indicators[8].ForeColor = Color.Black;

            Editor.Indicators[9].Style = IndicatorStyle.TextFore;    // Fields
            Editor.Indicators[9].ForeColor = Color.Black;

            //Autovervollständigung
            try
            {
                Editor.AutoCIgnoreCase = true;
                Editor.AutoCAutoHide = false;
                Editor.AutoCSeparator = ' ';
                // Wenn verfügbar: sortiere intern
                Editor.AutoCOrder = Order.PerformSort;
            }
            catch
            {
                // Falls deine Wrapper-Version die Properties nicht hat, ist das okay.
                // (Optional: per SendMessage – kann ich dir geben, wenn du magst)
            }

            // sinnvolle Stop-/Fillup-Zeichen (Geschmackssache)
            Editor.AutoCStops(" \t\r\n()[]{};:,.+-=*/%!&|^~<>?");

            try { Editor.AutoCIgnoreCase = true; Editor.AutoCAutoHide = false; }
            catch
            {
                SendMessage(Editor.Handle, SCI_AUTOCSETIGNORECASE, (IntPtr)1, IntPtr.Zero);
                SendMessage(Editor.Handle, SCI_AUTOCSETAUTOHIDE, (IntPtr)0, IntPtr.Zero);
            }



            Editor.MarginClick += (s, e) =>
            {
                const int MARGIN_FOLD = 2;
                if (e.Margin != MARGIN_FOLD) return;

                int line = Editor.LineFromPosition(e.Position);
                SendMessage(Editor.Handle, SCI_TOGGLEFOLD, (IntPtr)line, IntPtr.Zero);
            };


            // Basis-Styling
            Editor.StyleResetDefault();
            Editor.Styles[Style.Default].Font = "Consolas";
            Editor.Styles[Style.Default].Size = 11;
            Editor.StyleClearAll();

            Editor.Styles[Style.BraceLight].ForeColor = Color.FromArgb(0x66, 0xCC, 0xFF);
            Editor.Styles[Style.BraceLight].BackColor = Color.Transparent;

            Editor.Styles[Style.BraceBad].ForeColor = Color.White;
            Editor.Styles[Style.BraceBad].BackColor = Color.FromArgb(0xE5, 0x51, 0x51);

            Editor.UpdateUI += Editor_UpdateUI_BraceMatch;
            Editor.UpdateUI += async (s, e) =>
            {
                if ((e.Change & UpdateChange.Selection) != 0 && Editor.CallTipActive)
                    await TrySignatureHelpAsync(updateOnly: true);
            };

            ApplyCSharpHighlighting(EditorTheme.Light);
            Editor.CaretLineVisible = true;

            // Autocomplete
            Editor.AutoCSeparator = ' ';
            Editor.AutoCMaxHeight = 16;



            // Beispieltext beim ersten Start (optional)
            if (string.IsNullOrWhiteSpace(Editor.Text))
            {
                Editor.Text = "using System;\nusing System.Drawing;\n\nclass Demo\n{\n    static void Main()\n    {\n        Color.\n        System.\n    }\n}\n";
                Editor.SetSel(Editor.TextLength, Editor.TextLength);
            }
        }

        // Ganze Wort-Spanne für einen Diagnostic ermitteln (inkl. Spezialfall CS1002)
        private (int start, int len) GetNiceErrorRange(Diagnostic d)
        {
            var span = d.Location.SourceSpan;
            int docLen = Editor.TextLength;

            // Spezial: fehlendes Semikolon -> markiere das vorherige Token „voll“
            if (d.Id == "CS1002")
            {
                if (TryGetMissingSemicolonRange(span.Start, out int s, out int l))
                    return (s, l);
            }

            // Normale Fehler: Wort an der Stelle komplett markieren
            int s1 = Editor.WordStartPosition(Math.Max(0, span.Start), onlyWordCharacters: true);
            int e1 = Editor.WordEndPosition(Math.Min(docLen, span.Start + Math.Max(1, span.Length)), onlyWordCharacters: true);

            if (e1 > s1)
                return (s1, Math.Min(docLen - s1, e1 - s1));

            // Fallback: nach links zum nächsten Nicht-Whitespace, dann Wort nehmen
            int p = Math.Max(0, Math.Min(docLen - 1, span.Start));
            string txt = Editor.Text;
            while (p > 0 && char.IsWhiteSpace(txt[p])) p--;

            s1 = Editor.WordStartPosition(p, true);
            e1 = Editor.WordEndPosition(p, true);
            if (e1 > s1) return (s1, e1 - s1);

            // Letzter Fallback: 1 Zeichen
            int start = Math.Max(0, Math.Min(span.Start, docLen - 1));
            return (start, 1);
        }

        // Range bestimmen, wenn ein Semikolon fehlt („; expected“ / CS1002)
        private bool TryGetMissingSemicolonRange(int pos, out int start, out int len)
        {
            int docLen = Editor.TextLength;
            string txt = Editor.Text;

            // Gehe ein Zeichen links vom gemeldeten Punkt und überspringe Whitespaces
            int p = Math.Max(0, Math.Min(docLen - 1, pos > 0 ? pos - 1 : 0));
            while (p > 0 && char.IsWhiteSpace(txt[p])) p--;

            // Versuche das komplette Wort/Token zu erwischen
            int s = Editor.WordStartPosition(p, true);
            int e = Editor.WordEndPosition(p, true);
            if (e > s)
            {
                start = s; len = e - s;
                return true;
            }

            // Klammern o.ä.: markiere dieses Zeichen
            start = p; len = 1;
            return true;
        }


        private void InitEvents()
        {
            // Projekt-Baum
            btnRefresh.Click += (s, e) => LoadProjectTree();

            treeProject.NodeMouseDoubleClick += TreeProject_NodeMouseDoubleClick;

            // Editor
            Editor.CharAdded += Editor_CharAdded;
            Editor.KeyDown += Editor_KeyDown;

            // Diagnostics
            Editor.TextChanged += (s, e) =>
            {
                _docVersion++;
                _diagnosticTimer.Stop();
                _diagnosticTimer.Start();
            };
            _diagnosticTimer.Tick += async (s, e) =>
            {
                _diagnosticTimer.Stop();
                await RunDiagnosticsAsync();
            };

            // Tooltip über Fehler
            Editor.MouseDwellTime = 400;
            Editor.DwellStart += (s, e) =>
            {
                if (e.Position < 0) return;
                var diag = _currentDiagnostics.FirstOrDefault(d =>
                {
                    var span = d.Location.GetLineSpan();
                    var pos = e.Position;
                    // grobe Zuordnung: wir nehmen hier die absolute Position (Simplifizierung)
                    return true;
                });
            };
            Editor.DwellEnd += (s, e) => _tooltip.Hide(Editor);
        }

        // ---------------- Projektbaum ----------------
        private void LoadProjectTree()
        {
            treeProject.BeginUpdate();
            treeProject.Nodes.Clear();
            treeProject.Font = new Font("Calibri", 12);
            string root = ProjectManager.Project.Workspace;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                treeProject.Nodes.Add("(Kein Projektordner gefunden)");
                treeProject.EndUpdate();
                return;
            }

            treeProject.ImageList = imageList1;
            var rootNode = new TreeNode(Path.GetFileName(root)) { Tag = root ,ImageIndex = 1};
            treeProject.Nodes.Add(rootNode);

            // Nur die gewünschten Verzeichnisse anzeigen
            string[] wantedDirs = { "Pages", "Shared", "dlls" };
            foreach (var dirName in wantedDirs)
            {
                string dirPath = Path.Combine(root, dirName);
                if (Directory.Exists(dirPath))
                {
                    var node = new TreeNode(dirName) { Tag = dirPath };
                    node.ImageIndex = 2;
                    rootNode.Nodes.Add(node);
                    AddDirectoryNodes(node, dirPath);
                }
            }
            rootNode.ExpandAll();
            rootNode.Text = ProjectManager.Project.Name;
            treeProject.EndUpdate();
        }

        private void AddDirectoryNodes(TreeNode parent, string path)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    var name = Path.GetFileName(dir);
                    if (name.StartsWith(".")) continue; // .git etc.
                    var n = new TreeNode(name) { Tag = dir, ImageIndex = 2 };
                    parent.Nodes.Add(n);
                    AddDirectoryNodes(n, dir);
                }
                foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext is ".cs" or ".txt" or ".json" or ".editorconfig")
                    {
                        var fname = Path.GetFileName(file);
                        if (fname.Equals("dummy.cs", StringComparison.OrdinalIgnoreCase) || fname.Equals("SignalPool.cs", StringComparison.OrdinalIgnoreCase))
                            continue; // ausgeblendete Dateien
                        parent.Nodes.Add(new TreeNode(fname) { Tag = file, ImageIndex = 3 });
                    }
                }
            }
            catch { }
        }

        private void TreeProject_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is string p && File.Exists(p) && Path.GetExtension(p).Equals(".cs", StringComparison.OrdinalIgnoreCase))
                OpenFile(p);
        }

        private void OpenFile(string path)
        {
            try
            {
                _currentFile = path;
                Editor.Text = File.ReadAllText(path, Encoding.UTF8);
                Text = "Editor – " + Path.GetFileName(path);
                Editor.SetSel(Editor.TextLength, Editor.TextLength);
            }
            catch (Exception ex) { MessageBox.Show("Öffnen fehlgeschlagen: " + ex.Message); }
        }

        private void SaveCurrentFile()
        {
            if (_currentFile == null) return;
            try { File.WriteAllText(_currentFile, Editor.Text, Encoding.UTF8); }
            catch (Exception ex) { MessageBox.Show("Speichern fehlgeschlagen: " + ex.Message); }
        }

        // ---------------- Diagnostics ----------------
        private async Task RunDiagnosticsAsync()
        {
            string buffer = Editor.Text;
            var diags = new List<Microsoft.CodeAnalysis.Diagnostic>();

            if (WorkspaceService.IsReady)
            {
                var doc = WorkspaceService.GetDocumentForBuffer(_currentFile, buffer);
                if (doc != null)
                {
                    var tree = await doc.GetSyntaxTreeAsync();
                    if (tree != null) diags.AddRange(tree.GetDiagnostics());

                    var model = await doc.GetSemanticModelAsync();
                    if (model != null) diags.AddRange(model.GetDiagnostics());

                    var comp = await doc.Project.GetCompilationAsync();
                    if (comp != null && tree != null)
                        diags.AddRange(comp.GetDiagnostics()
                            .Where(d => d.Location.IsInSource && d.Location.SourceTree == tree));
                }
            }
            else
            {
                var tree = CSharpSyntaxTree.ParseText(buffer);
                var refs = IntelliSenseService.GetBasicReferences();
                var compilation = CSharpCompilation.Create("Live", new[] { tree }, refs,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                diags.AddRange(compilation.GetDiagnostics());
            }

            var final = diags
                .Where(d => d.Severity == DiagnosticSeverity.Error && !d.IsSuppressed)
                .Where(d => d.Location.IsInSource)
                .GroupBy(d => (d.Id, d.Location.SourceSpan.Start, d.Location.SourceSpan.Length))
                .Select(g => g.First())
                .ToList();

            OnUi(() =>
            {
                // ✅ Fehler = Indicator 0 (rot, Squiggle)
                Editor.IndicatorCurrent = 0;
                Editor.IndicatorClearRange(0, Editor.TextLength);

                foreach (var d in final)
                {
                    var (s, l) = GetNiceErrorRange(d);
                    if (l <= 0) l = 1;
                    // Sicherheitsbegrenzung
                    s = Math.Max(0, Math.Min(s, Editor.TextLength));
                    l = Math.Max(1, Math.Min(l, Editor.TextLength - s));

                    Editor.IndicatorFillRange(s, l);
                }

                _currentDiagnostics.Clear();
                _currentDiagnostics.AddRange(final);
            });


            //  await RefreshSemanticColoringAsync();
            // await RefreshClassColoringAsync();
            await RefreshSemanticOverlaysAsync();
        }

        private async Task RefreshSemanticOverlaysAsync()
        {
            // MSBuild-Workspace nötig, sonst überspringen
            if (!WorkspaceService.IsReady) return;

            // Schutz: extrem große Dateien überspringen
            if (Editor.TextLength > 500_000) return;

            var doc = WorkspaceService.GetDocumentForBuffer(_currentFile, Editor.Text);
            if (doc is null) return;

            var text = await doc.GetTextAsync();
            var span = new TextSpan(0, text.Length);

            var classified = await Classifier.GetClassifiedSpansAsync(doc, span);

            // Indicator -> Liste von (start, len)
            var buckets = new Dictionary<int, List<(int s, int l)>>()
            {
                [2] = new(), // Methoden
                [3] = new(), // Klassen
                [4] = new(), // Interfaces
                [5] = new(), // Structs
                [6] = new(), // Enums
                [7] = new(), // Delegates
                [8] = new(), // Properties (optional)
                [9] = new(), // Fields    (optional)
            };

            // Roslyn-Klassifikationen -> Indicator
            var map = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [ClassificationTypeNames.MethodName] = 2,
                [ClassificationTypeNames.ExtensionMethodName] = 2,

                [ClassificationTypeNames.ClassName] = 3,
                // Falls deine Roslyn-Version "RecordClassName" hat, nimm:
                // [ClassificationTypeNames.RecordClassName]     = 3,

                [ClassificationTypeNames.InterfaceName] = 4,
                [ClassificationTypeNames.StructName] = 5,
                [ClassificationTypeNames.EnumName] = 6,
                [ClassificationTypeNames.DelegateName] = 7,

                // Optional:
                [ClassificationTypeNames.PropertyName] = 8,
                [ClassificationTypeNames.FieldName] = 9,
            };

            int docLen = Editor.TextLength;
            const int maxPerBucket = 2000; // Cap für sehr große Dateien

            foreach (var c in classified)
            {
                if (!map.TryGetValue(c.ClassificationType, out int ind)) continue;

                int start = Math.Max(0, Math.Min(c.TextSpan.Start, docLen));
                int len = Math.Max(0, Math.Min(c.TextSpan.Length, docLen - start));
                if (len == 0) continue;

                var list = buckets[ind];
                if (list.Count < maxPerBucket)
                    list.Add((start, len));
            }

            OnUi(() =>
            {
                // erst alle semantischen Indikatoren löschen …
                foreach (var ind in buckets.Keys)
                {
                    Editor.IndicatorCurrent = ind;
                    Editor.IndicatorClearRange(0, docLen);
                }

                // … dann effizient füllen
                foreach (var (ind, list) in buckets)
                {
                    if (list.Count == 0) continue;
                    Editor.IndicatorCurrent = ind;
                    foreach (var (s, l) in list)
                        Editor.IndicatorFillRange(s, l);
                }
            });
        }

        private void OnUi(Action action)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }



        // ---------------- Autocomplete ----------------

        // Zustand für VS-Chord
        private bool _vsChordPending = false;
        private DateTime _vsChordSince;
        private const int VsChordTimeoutMs = 1000; // 1s Zeitfenster für die zweite Taste



        private void Editor_KeyDown(object? sender, KeyEventArgs e)
        {
            // Go To Definition (F12)
            if (!e.Control && !e.Alt && e.KeyCode == Keys.F12)
            {
                _ = GoToDefinitionAtCaretAsync();
                e.Handled = true; e.SuppressKeyPress = true;
                return;
            }

            // Force: Ctrl+Space => explizite Completion
            if (e.Control && e.KeyCode == Keys.Space)
            {
                BeginInvoke(new Action(async () =>
                {
                    await TryRoslynCompletionDirectAsync(null, forceInvoke: true);
                }));
                e.Handled = true; e.SuppressKeyPress = true;
                return;
            }

            // Punkt -> Member-Completion (nur hier noch automatische Completion)
            if (!e.Control && !e.Alt && (e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.Decimal))
            {
                BeginInvoke(new Action(async () =>
                {
                    if (!await TryRoslynCompletionDirectAsync('.', forceInvoke: false))
                        TryHandleDotCompletion();
                }));
                return;
            }
        }

        private async void Editor_CharAdded(object? sender, CharAddedEventArgs e)
        {
            char c = (char)e.Char;

            if (c == '(' || c == ',')
            {
                await TrySignatureHelpAsync();
                return;
            }
            if (c == ')')
            {
                Editor.CallTipCancel();
            }

            if (c == '.')
            {
                if (!await TryRoslynCompletionDirectAsync(c, forceInvoke: false))
                    TryHandleDotCompletion();
            }
            // Entfernt: automatischer Aufruf für Buchstaben/Ziffern (früher: else if (char.IsLetterOrDigit ...))
        }

        private async Task<bool> TryRoslynCompletionDirectAsync(char? typedChar, bool forceInvoke)
        {
            int pos = Editor.CurrentPosition;
            // Roslyn erwartet die Position im absoluten Puffer-Index
            var items = await IntelliSenseService.GetCompletionsAsync(Editor.Text, pos, _currentFile);
            if (items == null || items.Count == 0) return false;

            // Map items to display/insertion
            var texts = new List<string>();
            foreach (var it in items)
            {
                var display = it.DisplayText;
                if (string.IsNullOrWhiteSpace(display)) continue;
                int tick = display.IndexOf('`');
                if (tick >= 0) display = display.Substring(0, tick);
                texts.Add(display);
                if (texts.Count >= 200) break;
            }
            if (texts.Count == 0) return false;

            // Startposition des aktuellen Wortes bestimmen und Prefix extrahieren
            int wordStart = Editor.WordStartPosition(pos, true);
            int len = Math.Max(0, pos - wordStart);
            string prefix = len > 0 ? Editor.GetTextRange(wordStart, len) : string.Empty;

            Editor.AutoCShow(len, string.Join(" ", texts.Distinct().OrderBy(x => x)));
            try { if (!string.IsNullOrEmpty(prefix)) Editor.AutoCSelect(prefix); } catch { }

            return true;
        }

        private static bool IsDotCompletionTriggerChar(char c)
        {
            // Die Triggermengen können je nach Bedarf angepasst werden
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private bool TryHandleDotCompletion()
        {
            int pos = Editor.CurrentPosition - 2;
            if (pos < 0) return false;
            int start = pos;
            string text = Editor.Text;
            while (start >= 0)
            {
                char ch = text[start];
                if (IsDotCompletionTriggerChar(ch)) start--;
                else break;
            }
            start++;
            if (start < 0 || start > pos) return false;
            string chain = text.Substring(start, pos - start + 1).Trim('.');
            if (string.IsNullOrEmpty(chain)) return false;

            // 1) TYPE-Resolution über aktuelle 'using' (damit Color. -> System.Drawing.Color)
            var type = IntelliSenseService.ResolveTypeByUsings(Editor.Text, chain);
            if (type != null)
            {
                var members = IntelliSenseService.GetTypeMemberNames(type).Take(200).ToList();
                if (members.Count > 0) { Editor.AutoCShow(0, string.Join(" ", members)); return true; }
            }

            // 2) Namespace-Kette (System., System.Drawing. ...) – nur Namespaces in 'using'-Zeilen
            EnsureNamespaceIndex();
            string parentKey = "";
            string accum = "";
            var parts = chain.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string seg = parts[i];
                string key = accum;
                if (_nsChildren.TryGetValue(key, out var set) && set.Contains(seg))
                {
                    accum = string.IsNullOrEmpty(accum) ? seg : (accum + "." + seg);
                    parentKey = accum;
                }
                else break;
            }

            if (_nsChildren.TryGetValue(parentKey, out var direct))
            {
                bool inUsing = IsInUsingDirective();
                IEnumerable<string> items = direct;
                if (inUsing)
                {
                    // In 'using'-Zeile NUR Namespaces vorschlagen (Kinder, die selbst Eltern sind)
                    items = items.Where(child =>
                    {
                        var full = string.IsNullOrEmpty(parentKey) ? child : (parentKey + "." + child);
                        return _nsChildren.ContainsKey(full);
                    });
                }
                var list = items.OrderBy(x => x).Take(200).ToList();
                if (list.Count > 0) { Editor.AutoCShow(0, string.Join(" ", list)); return true; }
            }

            // 3) Fallback
            string[] fallback = { "ToString", "Equals", "GetHashCode", "GetType", "Length", "Count", "Add", "Remove", "Clear" };
            Editor.AutoCShow(0, string.Join(" ", fallback));
            return true;
        }
        private bool IsInUsingDirective()
        {
            try
            {
                int line = Editor.LineFromPosition(Editor.CurrentPosition);
                int lineStart = Editor.Lines[line].Position;
                int len = Editor.CurrentPosition - lineStart;
                if (len < 0) return false;
                string lineText = Editor.GetTextRange(lineStart, len);
                return lineText.TrimStart().StartsWith("using ", StringComparison.Ordinal);
            }
            catch { return false; }
        }
        private void EnsureNamespaceIndex()
        {
            if (_nsIndexBuilt) return;
            BuildNamespaceIndex();
        }
        private void BuildNamespaceIndex()
        {
            try
            {
                _nsChildren.Clear();
                void Add(string parent, string child)
                {
                    if (string.IsNullOrWhiteSpace(child)) return;
                    if (!_nsChildren.TryGetValue(parent, out var set))
                    {
                        set = new HashSet<string>(StringComparer.Ordinal);
                        _nsChildren[parent] = set;
                    }
                    set.Add(child);
                }

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = asm.GetTypes(); } catch { continue; }
                    foreach (var t in types)
                    {
                        if (!IsBrowsableType(t)) continue;
                        string ns = t.Namespace ?? "";
                        if (!string.IsNullOrEmpty(ns))
                        {
                            var segs = ns.Split('.');
                            string parent = "";
                            for (int i = 0; i < segs.Length; i++)
                            {
                                string curr = segs[i];
                                Add(parent, curr);
                                parent = string.IsNullOrEmpty(parent) ? curr : parent + "." + curr;
                            }
                        }
                        string displayName = t.Name;
                        int tick = displayName.IndexOf('`');
                        if (tick >= 0) displayName = displayName.Substring(0, tick);
                        Add(ns, displayName);
                    }
                }
                _nsIndexBuilt = true;
            }
            catch { }
        }

        private bool IsBrowsableType(Type t)
        {
            if (t == null) return false;
            if (t.IsNested) return false;
            if (!(t.IsPublic || t.IsNestedPublic)) return false;
            string name = t.Name;
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name.IndexOf('<') >= 0) return false;
            if (name.IndexOf('`') >= 0) return false; // Action`1, Func`16 etc. ausblenden
            if (name.StartsWith("_", StringComparison.Ordinal)) return false;
            if (name.EndsWith("e__FixedBuffer", StringComparison.Ordinal)) return false;
            return true;
        }



        private void btnRebuild_Click(object sender, EventArgs e)
        {
            if (ProjectManager.Project == null)
            {
                MessageBox.Show("No project loaded. Please create or load a project first.");
                return;
            }

            ProjectManager.BuildProject();
            if (ProjectManager.BuildSuccess)
            {

                AmiumScripter.Root.Main.OnOpenProject();
                this.ActiveControl = null;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            ProjectManager.RunProject();
            this.ActiveControl = null;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            ProjectManager.StopProject();
            AmiumScripter.Root.Main.ClearPagesFromUI();

            this.ActiveControl = null;
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            SaveCurrentFile();
        }

        private async Task<bool> TrySignatureHelpAsync(bool updateOnly = false)
        {
            if (!WorkspaceService.IsReady)
            {
                if (!updateOnly) Editor.CallTipCancel();
                return false;
            }

            int pos = Math.Max(0, Editor.CurrentPosition);
            var doc = WorkspaceService.GetDocumentForBuffer(_currentFile, Editor.Text);
            if (doc == null)
            {
                if (!updateOnly) Editor.CallTipCancel();
                return false;
            }

            var tree = await doc.GetSyntaxTreeAsync().ConfigureAwait(false);
            if (tree == null)
            {
                if (!updateOnly) Editor.CallTipCancel();
                return false;
            }

            var root = await tree.GetRootAsync().ConfigureAwait(false);
            var token = root.FindToken(Math.Max(0, pos - 1));
            var node = token.Parent;

            while (node != null)
            {
                if (node is InvocationExpressionSyntax inv && inv.ArgumentList != null)
                    return await ShowSignatureForInvocationAsync(inv, doc, pos, updateOnly);
                if (node is ObjectCreationExpressionSyntax obj && obj.ArgumentList != null)
                    return await ShowSignatureForCreationAsync(obj, doc, pos, updateOnly);
                node = node.Parent;
            }

            if (!updateOnly) Editor.CallTipCancel();
            return false;
        }

        private static int GetArgumentIndex(ArgumentListSyntax args, int caretPos)
        {
            int idx = 0;
            foreach (var a in args.Arguments)
            {
                if (caretPos > a.Span.End) idx++;
                else break;
            }
            if (idx < 0) idx = 0;
            if (idx >= args.Arguments.Count) idx = Math.Max(0, args.Arguments.Count - 1);
            return idx;
        }

        private async Task<bool> ShowSignatureForCreationAsync(ObjectCreationExpressionSyntax obj, RoslynDocument doc, int caretPos, bool updateOnly)
        {
            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var type = model.GetTypeInfo(obj).Type as INamedTypeSymbol
                       ?? model.GetSymbolInfo(obj.Type).Symbol as INamedTypeSymbol;
            if (type == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var ctors = type.InstanceConstructors.Where(c => !c.IsStatic).OrderBy(c => c.Parameters.Length).ToArray();
            if (ctors.Length == 0) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            int argIndex = GetArgumentIndex(obj.ArgumentList!, caretPos);
            return ShowSignatureCallTip(type, ctors, argIndex, updateOnly);
        }

        private async Task<bool> ShowSignatureForInvocationAsync(InvocationExpressionSyntax inv, RoslynDocument doc, int caretPos, bool updateOnly)
        {
            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var info = model.GetSymbolInfo(inv);
            var methodGroup = info.Symbol != null ? new[] { info.Symbol } : (info.CandidateSymbols.IsDefaultOrEmpty ? Array.Empty<ISymbol>() : info.CandidateSymbols.ToArray());
            var methods = methodGroup.OfType<IMethodSymbol>().ToArray();

            if (methods.Length == 0)
            {
                var exprType = model.GetTypeInfo(inv.Expression).Type as INamedTypeSymbol;
                if (exprType != null)
                {
                    var name = inv.Expression is MemberAccessExpressionSyntax ma ? ma.Name.Identifier.ValueText
                             : inv.Expression is IdentifierNameSyntax id ? id.Identifier.ValueText
                             : null;
                    if (!string.IsNullOrEmpty(name))
                        methods = exprType.GetMembers(name).OfType<IMethodSymbol>().ToArray();
                }
            }

            if (methods.Length == 0) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            int argIndex = inv.ArgumentList != null ? GetArgumentIndex(inv.ArgumentList, caretPos) : 0;
            return ShowSignatureCallTip(methods[0].ContainingType, methods, argIndex, updateOnly);
        }

        private bool ShowSignatureCallTip(INamedTypeSymbol containerType, IEnumerable<IMethodSymbol> overloads, int argIndex, bool updateOnly)
        {
            var formatType = SymbolDisplayFormat.MinimallyQualifiedFormat;
            var formatParam = SymbolDisplayFormat.MinimallyQualifiedFormat;
            var lines = new List<string>();
            var spanForHighlight = (start: -1, length: 0);

            var ordered = overloads.OrderBy(m => m.Parameters.Length).ToList();
            var best = ordered.FirstOrDefault(m => m.Parameters.Length > argIndex) ?? ordered.First();

            foreach (var m in ordered.Take(12))
            {
                var typeName = containerType.ToDisplayString(formatType);
                var namePart = m.MethodKind == MethodKind.Constructor ? typeName : $"{typeName}.{m.Name}";
                var paramPieces = new List<string>();
                int highlightStartInLine = -1, highlightLen = 0;

                for (int i = 0; i < m.Parameters.Length; i++)
                {
                    var p = m.Parameters[i];
                    string piece = p.ToDisplayString(formatParam);
                    if (m.Equals(best) && i == argIndex)
                    {
                        highlightStartInLine = string.Join(", ", paramPieces).Length + (paramPieces.Count > 0 ? 2 : 0);
                        highlightLen = piece.Length;
                    }
                    paramPieces.Add(piece);
                }

                string paramList = string.Join(", ", paramPieces);
                string line = $"{namePart}({paramList})";
                lines.Add(line);

                if (m.Equals(best) && highlightStartInLine >= 0)
                {
                    int globalOffset = lines.Take(lines.Count - 1).Sum(x => x.Length + 1) + namePart.Length + 1; // '('
                    spanForHighlight = (globalOffset + highlightStartInLine, highlightLen);
                }
            }

            string text = string.Join("\n", lines);
            if (!Editor.CallTipActive)
                Editor.CallTipShow(Editor.CurrentPosition, text);
            else
            {
                Editor.CallTipCancel();
                Editor.CallTipShow(Editor.CurrentPosition, text);
            }

            try
            {
                if (spanForHighlight.start >= 0 && spanForHighlight.length > 0)
                    Editor.CallTipSetHlt(spanForHighlight.start, spanForHighlight.start + spanForHighlight.length);
            }
            catch { }

            return true;
        }

        private async Task GoToDefinitionAtCaretAsync()
        {
            try
            {
                if (!WorkspaceService.IsReady) return;
                int caret = Math.Max(0, Editor.CurrentPosition);

                // Aktuelles Dokument aktualisieren/holen
                var doc = WorkspaceService.GetDocumentForBuffer(_currentFile, Editor.Text);
                if (doc == null) return;

                var semanticModel = await doc.GetSemanticModelAsync().ConfigureAwait(false);
                if (semanticModel == null) return;

                var root = await semanticModel.SyntaxTree.GetRootAsync().ConfigureAwait(false);
                var token = root.FindToken(Math.Max(0, caret - 1));
                if (token == default || string.IsNullOrWhiteSpace(token.ValueText)) return;

                SyntaxNode? node = token.Parent;
                ISymbol? symbol = null;

                // Versuche zuerst deklarierte Symbole (Klassen, Methoden, Variablen, etc.)
                if (node != null)
                {
                    symbol = semanticModel.GetDeclaredSymbol(node);
                    if (symbol == null)
                        symbol = semanticModel.GetSymbolInfo(node).Symbol;
                }

                // Fallback: Lookup nach Name an der Position
                if (symbol == null && !string.IsNullOrEmpty(token.ValueText))
                {
                    var lookup = semanticModel.LookupSymbols(caret, name: token.ValueText);
                    symbol = lookup.FirstOrDefault();
                }

                if (symbol == null) { ShowDefinitionNotFoundTooltip(caret, token.ValueText); return; }

                var sourceLoc = symbol.Locations.FirstOrDefault(l => l.IsInSource);
                if (sourceLoc == null) { ShowDefinitionNotFoundTooltip(caret, token.ValueText); return; }

                string targetFile = sourceLoc.SourceTree?.FilePath ?? string.Empty;
                int targetPos = sourceLoc.SourceSpan.Start;

                if (!string.IsNullOrEmpty(targetFile) && File.Exists(targetFile))
                {
                    // Wenn anderes File, öffnen
                    if (!string.Equals(_currentFile, targetFile, StringComparison.OrdinalIgnoreCase))
                    {
                        OnUi(() => OpenFile(targetFile));
                    }

                    // Caret setzen nach dem Laden (kleine Verzögerung bei Dateiwechsel)
                    OnUi(async () =>
                    {
                        if (!string.Equals(_currentFile, targetFile, StringComparison.OrdinalIgnoreCase))
                            await Task.Delay(50); // minimal warten bis Text gesetzt

                        targetPos = Math.Max(0, Math.Min(targetPos, Editor.TextLength));
                        Editor.SetSel(targetPos, targetPos);
                        Editor.ScrollCaret();
                    });
                }
                else
                {
                    ShowDefinitionNotFoundTooltip(caret, token.ValueText);
                }
            }
            catch
            {
                // Ignorieren oder optional Logging
            }
        }

        private void ShowDefinitionNotFoundTooltip(int caret, string identifier)
        {
            try
            {
                int x = Editor.PointXFromPosition(caret);
                int y = Editor.PointYFromPosition(caret);
                _tooltip.Show($"Definition für '{identifier}' nicht gefunden", Editor, new Point(x, y + 16), 2500);
            }
            catch { }
        }

        private void InitEditorContextMenu()
        {
            var cms = new ContextMenuStrip();

            var miGoto = new ToolStripMenuItem("Go To Definition (F12)");
            miGoto.Click += async (_, __) => await GoToDefinitionAtCaretAsync();
            cms.Items.Add(miGoto);

            var miRename = new ToolStripMenuItem("Rename...");
            miRename.Click += async (_, __) => await RenameSymbolAtCaretAsync();
            cms.Items.Add(miRename);

            cms.Items.Add(new ToolStripSeparator());

            var miCollapse = new ToolStripMenuItem("Collapse to Definition");
            miCollapse.Click += (_, __) => CollapseToDefinitions();
            cms.Items.Add(miCollapse);

            var miExpand = new ToolStripMenuItem("Expand All");
            miExpand.Click += (_, __) => ExpandAllFolds();
            cms.Items.Add(miExpand);

            Editor.ContextMenuStrip = cms;
        }

        private void CollapseToDefinitions()
        {
            // Nutzt vorhandene Folding-API: Alles kontrahieren
            SendMessage(Editor.Handle, SCI_FOLDALL, (IntPtr)SC_FOLDACTION_CONTRACT, IntPtr.Zero);
        }
        private void ExpandAllFolds()
        {
            SendMessage(Editor.Handle, SCI_FOLDALL, (IntPtr)SC_FOLDACTION_EXPAND, IntPtr.Zero);
        }

        private async Task RenameSymbolAtCaretAsync()
        {
            try
            {
                if (!WorkspaceService.IsReady) return;
                int caret = Math.Max(0, Editor.CurrentPosition);
                var doc = WorkspaceService.GetDocumentForBuffer(_currentFile, Editor.Text);
                if (doc == null) return;

                var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
                if (model == null) return;

                var root = await model.SyntaxTree.GetRootAsync().ConfigureAwait(false);
                var token = root.FindToken(Math.Max(0, caret - 1));
                if (token == default || string.IsNullOrWhiteSpace(token.ValueText)) return;

                SyntaxNode? node = token.Parent;
                ISymbol? symbol = null;
                if (node != null)
                {
                    symbol = model.GetDeclaredSymbol(node) ?? model.GetSymbolInfo(node).Symbol;
                }
                if (symbol == null)
                {
                    var lookup = model.LookupSymbols(caret, name: token.ValueText);
                    symbol = lookup.FirstOrDefault();
                }
                if (symbol == null) return;

                // Ursprüngliche Definition merken (für Caret-Repositionierung)
                var defLoc = symbol.Locations.FirstOrDefault(l => l.IsInSource);
                var defFile = defLoc?.SourceTree?.FilePath;
                var defStart = defLoc?.SourceSpan.Start ?? -1;
                var oldName = symbol.Name;

                string? newName = PromptForInput("Rename", $"Rename '{symbol.Name}' to:", symbol.Name);
                if (string.IsNullOrWhiteSpace(newName) || newName == symbol.Name) return;
                if (!SyntaxFacts.IsValidIdentifier(newName))
                {
                    MessageBox.Show("Ungültiger Bezeichner.");
                    return;
                }

                var solution = WorkspaceService.CurrentSolution;
                if (solution == null) return;

                Solution newSolution;
                try
                {
                    newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, solution.Workspace.Options, default);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Rename failed: " + ex.Message);
                    return;
                }

                var ws = WorkspaceService.Workspace;
                if (ws != null && ws.TryApplyChanges(newSolution))
                {
                    // Aktuelle Datei neu laden
                    if (!string.IsNullOrEmpty(_currentFile))
                    {
                        var updatedDoc = ws.CurrentSolution.Projects.SelectMany(p => p.Documents)
                            .FirstOrDefault(d => string.Equals(d.FilePath, _currentFile, StringComparison.OrdinalIgnoreCase));
                        if (updatedDoc != null)
                        {
                            var text = await updatedDoc.GetTextAsync();
                            string newContent = text.ToString();

                            // Caret-Ziel bestimmen
                            int targetCaret = caret; // Fallback
                            if (defFile != null && string.Equals(defFile, _currentFile, StringComparison.OrdinalIgnoreCase) && defStart >= 0)
                            {
                                // Suche neuen Namen nahe der alten Startposition (Fenster ± 64 Zeichen)
                                int searchFrom = Math.Max(0, defStart - 64);
                                int searchTo = Math.Min(newContent.Length, defStart + oldName.Length + 64);
                                string window = newContent.Substring(searchFrom, searchTo - searchFrom);
                                int relIndex = window.IndexOf(newName, StringComparison.Ordinal);
                                if (relIndex >= 0)
                                {
                                    targetCaret = searchFrom + relIndex + newName.Length; // hinter den neuen Namen setzen
                                }
                            }

                            OnUi(async () =>
                            {
                                // Indikatoren vorher löschen, damit keine verschobenen Highlights bleiben
                                for (int ind = 0; ind <= 9; ind++)
                                {
                                    Editor.IndicatorCurrent = ind;
                                    Editor.IndicatorClearRange(0, Editor.TextLength);
                                }

                                Editor.Text = newContent;
                                targetCaret = Math.Max(0, Math.Min(targetCaret, Editor.TextLength));
                                Editor.SetSel(targetCaret, targetCaret);
                                Editor.ScrollCaret();

                                // Force immediate refresh (Diagnostics + Semantic Overlays)
                                _ = RunDiagnosticsAsync();
                                await Task.Delay(50);
                                await RefreshSemanticOverlaysAsync();
                            });
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Rename failed (apply changes)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rename failed: " + ex.Message);
            }
        }

        private string? PromptForInput(string title, string label, string defaultValue)
        {
            using var f = new Form()
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(360, 140)
            };
            var lbl = new Label { Text = label, Left = 10, Top = 10, Width = 340 };
            var tb = new TextBox { Left = 10, Top = 40, Width = 340, Text = defaultValue };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 190, Width = 75, Top = 80 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 275, Width = 75, Top = 80 };
            f.Controls.AddRange(new Control[] { lbl, tb, ok, cancel });
            f.AcceptButton = ok; f.CancelButton = cancel;
            return f.ShowDialog(this) == DialogResult.OK ? tb.Text : null;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveCurrentFile();
        }
    }
    /// <summary>
    /// Roslyn-gestütztes IntelliSense mit
    /// - MSBuildWorkspace (echtes Projekt, alle Refs/NuGets) als Primary
    /// - AdhocWorkspace (aktuelle Datei + Basis-Refs + extra Refs) als Fallback
    /// - Zusatz-APIs zum Nachrüsten von Referenzen (dll-Ordner etc.)
    /// - Reflection-Helfer für Fallback-Completion (Dot-Completion)
    /// </summary>
    public static class IntelliSenseService
    {
        // ---------- Extra-Referenzen, die ihr zur Laufzeit "füttern" könnt ----------
        private static readonly Dictionary<string, MetadataReference> _extraRefs =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Eine spezifische DLL als MetadataReference registrieren.</summary>
        public static void RegisterReference(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    _extraRefs[path] = MetadataReference.CreateFromFile(path);
            }
            catch { /* ignore */ }
        }

        /// <summary>Alle *.dll in einem Ordner (rekursiv optional) registrieren.</summary>
        public static void RegisterReferenceDirectory(string dir, bool recursive = true)
        {
            try
            {
                if (!Directory.Exists(dir)) return;
                var files = Directory.GetFiles(dir, "*.dll",
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (var f in files) RegisterReference(f);
            }
            catch { /* ignore */ }
        }

        // ---------- PUBLIC API: Completions über Roslyn ----------
        public static async Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(
            string buffer, int position, string? currentFilePath, CancellationToken ct = default)
        {
            // 1) Bevorzugt: echtes Projekt laden (kennt NuGets/ProjectRefs/Analyzer etc.)
            if (WorkspaceService.IsReady)
            {
                var doc = WorkspaceService.GetDocumentForBuffer(currentFilePath, buffer);
                if (doc != null)
                {
                    var svc = CompletionService.GetService(doc);
                    if (svc != null)
                    {
                        var list = await svc.GetCompletionsAsync(doc, position, cancellationToken: ct)
                                            .ConfigureAwait(false);
                        if (list != null)
                        {
                            // ImmutableArray<CompletionItem> -> Array
                            return list.Items.Length == 0 ? Array.Empty<CompletionItem>() : list.Items.ToArray();
                        }
                    }
                }
            }

            // 2) Fallback: AdhocWorkspace (aktuelle Datei + Basis-Refs (+ extra Refs))
            var (adhocDoc, _) = EnsureAdhocDocument(buffer, currentFilePath);
            if (adhocDoc == null) return Array.Empty<CompletionItem>();

            var service = CompletionService.GetService(adhocDoc);
            if (service == null) return Array.Empty<CompletionItem>();

            var res = await service.GetCompletionsAsync(adhocDoc, position, cancellationToken: ct)
                                   .ConfigureAwait(false);
            if (res == null) return Array.Empty<CompletionItem>();
            return res.Items.Length == 0 ? Array.Empty<CompletionItem>() : res.Items.ToArray();
        }

        // ---------- Reflection-Helfer für Fallbacks (z. B. Dot-Completion) ----------
        /// <summary>Löst einen Typ bevorzugt vollqualifiziert, sonst über 'using'-Direktiven im Buffer.</summary>
        public static Type? ResolveTypeByUsings(string buffer, string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return null;

            if (typeName.Contains("."))
            {
                var t = ResolveType(typeName);
                if (t != null) return t;
            }

            try
            {
                var tree = CSharpSyntaxTree.ParseText(buffer);
                var root = tree.GetCompilationUnitRoot();
                var usings = root.Usings
                                 .Select(u => u.Name?.ToString())
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Distinct()
                                 .ToList();
                foreach (var ns in usings)
                {
                    var t = ResolveType(ns + "." + typeName);
                    if (t != null) return t;
                }
            }
            catch { /* ignore */ }

            return ResolveType(typeName);
        }

        /// <summary>Versucht Type.GetType + alle geladenen Assemblies.</summary>
        public static Type? ResolveType(string full)
        {
            try
            {
                var t = Type.GetType(full, throwOnError: false, ignoreCase: false);
                if (t != null) return t;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        t = asm.GetType(full, throwOnError: false, ignoreCase: false);
                        if (t != null) return t;
                    }
                    catch { /* ignore */ }
                }
                return null;
            }
            catch { return null; }
        }

        /// <summary>Gefilterte Member-Namen (ohne Backticks/SpecialNames), statisch + instanziert.</summary>
        public static IEnumerable<string> GetTypeMemberNames(Type t)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);

            static bool IsGood(string n)
            {
                if (string.IsNullOrWhiteSpace(n)) return false;
                if (n[0] == '_' || n.StartsWith("get_") || n.StartsWith("set_") || n.StartsWith("add_") || n.StartsWith("remove_")) return false;
                if (n.IndexOf('<') >= 0) return false;
                if (n.IndexOf('`') >= 0) return false;
                return true;
            }

            try
            {
                const BindingFlags S = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                const BindingFlags I = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

                foreach (var f in t.GetFields(S)) if (IsGood(f.Name)) set.Add(f.Name);
                foreach (var p in t.GetProperties(S)) if (IsGood(p.Name)) set.Add(p.Name);
                foreach (var m in t.GetMethods(S)) if (!m.IsSpecialName && IsGood(m.Name)) set.Add(m.Name);

                foreach (var f in t.GetFields(I)) if (IsGood(f.Name)) set.Add(f.Name);
                foreach (var p in t.GetProperties(I)) if (IsGood(p.Name)) set.Add(p.Name);
                foreach (var m in t.GetMethods(I)) if (!m.IsSpecialName && IsGood(m.Name)) set.Add(m.Name);
            }
            catch { /* ignore */ }

            return set.OrderBy(x => x);
        }

        // ---------- Adhoc-Workspace (Fallback) ----------
        private static AdhocWorkspace? _adhoc;
        private static ProjectId? _adhocProjectId;
        private static readonly object _adhocLock = new();

        /// <summary>Basis-Referenzen aus der TPA-Liste + optional registrierte Extra-Referenzen.</summary>
        public static IReadOnlyList<MetadataReference> GetBasicReferences()
        {
            var refs = new List<MetadataReference>();
            try
            {
                var tpa = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)?.Split(Path.PathSeparator)
                          ?? Array.Empty<string>();

                // Minimale + häufig genutzte libs
                string[] need = {
                    "System.Private.CoreLib", "System.Runtime", "System.Console", "System.Linq", "System.Private.Uri",
                    "System.Collections", "System.IO", "System.Private.Xml", "System.Private.Xml.Linq", "System.ObjectModel",
                    "System.Runtime.Extensions", "netstandard", "System.Drawing", "System.Drawing.Primitives", "System.Windows.Forms",
                    "System.Memory", "System.Text.RegularExpressions", "System.Text.Json", "System.Linq.Expressions",
                    "System.ComponentModel.Primitives", "System.ComponentModel.TypeConverter"
                };

                foreach (var p in tpa)
                {
                    var name = Path.GetFileNameWithoutExtension(p);
                    if (((IEnumerable<string>)need).Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        try { refs.Add(MetadataReference.CreateFromFile(p)); } catch { /* ignore */ }
                    }
                }
            }
            catch { /* ignore */ }

            // Eigene Assembly (falls geladen)
            if (ProjectManager.LoadedAssemblyBytes != null)
            {
                refs.Add(MetadataReference.CreateFromImage(ProjectManager.LoadedAssemblyBytes));
            }

            // + vom Host registrierte Zusatz-Refs (dll-Ordner usw.)
            refs.AddRange(_extraRefs.Values);
            return refs;
        }

        public static (RoslynDocument? doc, RoslynWorkspace ws) EnsureAdhocDocument(string buffer, string? filePath)
        {
            lock (_adhocLock)
            {
                if (_adhoc == null)
                {
                    var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
                    _adhoc = new AdhocWorkspace(host);

                    var projInfo = ProjectInfo.Create(
                        ProjectId.CreateNewId(),
                        VersionStamp.Create(),
                        name: "AdhocProject",
                        assemblyName: "AdhocProject",
                        language: LanguageNames.CSharp,
                        parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
                        compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                        metadataReferences: GetBasicReferences()
                    );

                    _adhocProjectId = projInfo.Id;
                    _adhoc.AddProject(projInfo);
                }

                if (_adhocProjectId == null)
                    throw new InvalidOperationException("No project id");

                var existing = _adhoc.CurrentSolution.Projects
                                  .First(p => p.Id == _adhocProjectId)
                                  .Documents.FirstOrDefault();

                var name = string.IsNullOrWhiteSpace(filePath) ? "Current.cs" : Path.GetFileName(filePath);
                var text = SourceText.From(buffer);

                if (existing == null)
                {
                    var docId = DocumentId.CreateNewId(_adhocProjectId);
                    // TextLoader.From erwartet TextAndVersion
                    var loader = TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create(), filePath));
                    _adhoc.AddDocument(DocumentInfo.Create(docId, name, loader: loader, filePath: filePath));
                    var doc = _adhoc.CurrentSolution.GetDocument(docId);
                    return (doc, _adhoc);
                }
                else
                {
                    var newSolution = existing.WithText(text).Project.Solution;
                    _adhoc.TryApplyChanges(newSolution);
                    existing = _adhoc.CurrentSolution.GetDocument(existing.Id);
                    return (existing, _adhoc);
                }
            }
        }
    }


    public static class WorkspaceService
    {
        private static readonly object _lock = new();
        private static bool _registered;
        private static bool _initialized;
        private static MSBuildWorkspace? _msbuild;
        private static Solution? _solution;
        private static readonly Dictionary<string, DocumentId> _transientDocs = new(StringComparer.OrdinalIgnoreCase);
        private static ProjectId? _defaultProjectId; // Hinweis: ProjectId ist eine Klasse, kein Nullable-struct
        public static bool IsReady => _initialized && _msbuild != null && _solution != null;
        public static MSBuildWorkspace? Workspace => _msbuild;
        public static Solution? CurrentSolution => _solution;

        /// <summary>
        /// Übergib Pfad zu .sln oder .csproj.
        /// </summary>
        public static async Task InitializeAsync(string projectOrSolutionPath)
        {
            lock (_lock)
            {
                if (!_registered)
                {
                    try { MSBuildLocator.RegisterDefaults(); }
                    catch (InvalidOperationException) { /* already registered */ }
                    _registered = true;
                }
            }

            projectOrSolutionPath = Path.GetFullPath(projectOrSolutionPath);
            if (!File.Exists(projectOrSolutionPath))
                throw new FileNotFoundException("Projekt/Solution nicht gefunden", projectOrSolutionPath);

            _msbuild ??= MSBuildWorkspace.Create();

            if (projectOrSolutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                _solution = await _msbuild.OpenSolutionAsync(projectOrSolutionPath).ConfigureAwait(false);
            else
            {
                var project = await _msbuild.OpenProjectAsync(projectOrSolutionPath).ConfigureAwait(false);
                _solution = project.Solution;
            }

            // Erstes Projekt als Default für transiente Dokumente
            var firstProject = _solution!.Projects.FirstOrDefault();
            _defaultProjectId = firstProject?.Id;

            _initialized = true;
        }

        /// <summary>
        /// Gibt ein Roslyn-Dokument für den aktuellen Buffer zurück.
        /// - Falls filePath Teil des Projekts ist: echtes Dokument aktualisieren (WithText).
        /// - Sonst: transienten Doc-Knoten im Default-Projekt anlegen/aktualisieren.
        /// </summary>
        public static RoslynDocument? GetDocumentForBuffer(string? filePath, string buffer)
        {
            if (!IsReady) return null;
            if (_msbuild == null || _solution == null) return null;

            var text = SourceText.From(buffer);

            // 1) Datei im Projekt suchen
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var doc = _solution.Projects.SelectMany(p => p.Documents)
                    .FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

                if (doc != null)
                {
                    var newSolution = doc.WithText(text).Project.Solution;
                    if (_msbuild.TryApplyChanges(newSolution))
                    {
                        _solution = _msbuild.CurrentSolution;
                        return _solution.GetDocument(doc.Id);
                    }
                    return doc;
                }
            }

            // 2) Transient im Default-Projekt
            if (_defaultProjectId == null) return null;

            var key = string.IsNullOrWhiteSpace(filePath) ? "__CURRENT__" : Path.GetFullPath(filePath);
            if (_transientDocs.TryGetValue(key, out var existingId))
            {
                var doc = _solution.GetDocument(existingId);
                if (doc != null)
                {
                    var newSolution = doc.WithText(text).Project.Solution;
                    if (_msbuild.TryApplyChanges(newSolution))
                    {
                        _solution = _msbuild.CurrentSolution;
                        return _solution.GetDocument(existingId);
                    }
                    return doc;
                }
            }

            // Neu anlegen (WICHTIG: bei MSBuildWorkspace niemals AddDocument(...) am Workspace selber aufrufen)
            var name = string.IsNullOrWhiteSpace(filePath) ? "Current.cs" : Path.GetFileName(filePath);
            var newDocId = DocumentId.CreateNewId(_defaultProjectId); // ProjectId ist eine Klasse; kein .Value verwenden
            var loader = TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create(), filePath));
            var docInfo = DocumentInfo.Create(newDocId, name, filePath: filePath, loader: loader);

            var newSolution2 = _msbuild.CurrentSolution.AddDocument(docInfo);
            if (_msbuild.TryApplyChanges(newSolution2))
            {
                _solution = _msbuild.CurrentSolution;
                _transientDocs[key] = newDocId;
                return _solution.GetDocument(newDocId);
            }

            // Falls TryApplyChanges fehlschlägt, letzten Stand zurückgeben (ohne Garantie)
            return _solution.GetDocument(newDocId);
        }
    }
}
