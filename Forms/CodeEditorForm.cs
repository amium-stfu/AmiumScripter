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
                    MessageBox.Show("Workspace init failed: " + ex.Message);
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



        private void InitEditor()
        {
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

            // Indicator 2 = Methoden-Markierung
            Editor.Indicators[2].Style = IndicatorStyle.PointCharacter;  // oder TextFore wenn du reine Farbe willst
            Editor.Indicators[2].Under = true;
            Editor.Indicators[2].ForeColor = Color.FromArgb(0x56, 0x9C, 0xD6); // passend zum Keyword-Blau


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

            // Squiggle Indicator
            Editor.Indicators[0].Style = IndicatorStyle.Squiggle;
            Editor.Indicators[0].ForeColor = Color.Red;
            Editor.Indicators[0].Under = true;

            // Beispieltext beim ersten Start (optional)
            if (string.IsNullOrWhiteSpace(Editor.Text))
            {
                Editor.Text = "using System;\nusing System.Drawing;\n\nclass Demo\n{\n    static void Main()\n    {\n        Color.\n        System.\n    }\n}\n";
                Editor.SetSel(Editor.TextLength, Editor.TextLength);
            }
        }





        private void InitEvents()
        {
            // Projekt-Baum
            btnRefresh.Click += (s, e) => LoadProjectTree();
            btnSave.Click += (s, e) => SaveCurrentFile();
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
            string root = ProjectManager.Project.Workspace;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                treeProject.Nodes.Add("(Kein Projektordner gefunden)");
                treeProject.EndUpdate();
                return;
            }

            var rootNode = new TreeNode(Path.GetFileName(root)) { Tag = root };
            treeProject.Nodes.Add(rootNode);
            AddDirectoryNodes(rootNode, root);
            rootNode.Expand();
            treeProject.EndUpdate();
        }

        private string FindProjectRoot()
        {
            // Simple Heuristik: gehe vom aktuellen .exe-Verzeichnis nach oben, bis eine *.csproj zu finden ist.
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 5; i++)
            {
                if (Directory.EnumerateFiles(dir, "*.csproj", SearchOption.TopDirectoryOnly).Any())
                    return dir;
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private void AddDirectoryNodes(TreeNode parent, string path)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    var name = Path.GetFileName(dir);
                    if (name.StartsWith(".")) continue; // .git etc.
                    var n = new TreeNode(name) { Tag = dir };
                    parent.Nodes.Add(n);
                    AddDirectoryNodes(n, dir);
                }
                foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext is ".cs" or ".txt" or ".json" or ".editorconfig")
                        parent.Nodes.Add(new TreeNode(Path.GetFileName(file)) { Tag = file });
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
            // Text aus dem Editor
            string buffer = Editor.Text;

            List<Microsoft.CodeAnalysis.Diagnostic> diags = new();

            if (WorkspaceService.IsReady)
            {
                // ECHT: Dokument aus dem geladenen Projekt (inkl. Refs/NuGets)
                var doc = WorkspaceService.GetDocumentForBuffer(_currentFile, buffer);
                if (doc != null)
                {
                    // Syntax + Semantik nur für DIESES Dokument
                    var tree = await doc.GetSyntaxTreeAsync().ConfigureAwait(false);
                    if (tree != null)
                    {
                        diags.AddRange(tree.GetDiagnostics()); // Syntax
                    }

                    var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
                    if (model != null)
                    {
                        diags.AddRange(model.GetDiagnostics()); // Semantik nur auf diesem Tree
                    }

                    // Optional: projektweite Fehler, aber auf dieses Dokument gefiltert
                    var comp = await doc.Project.GetCompilationAsync().ConfigureAwait(false);
                    if (comp != null && tree != null)
                    {
                        var compDiags = comp.GetDiagnostics()
                            .Where(d => d.Location.IsInSource && d.Location.SourceTree == tree);
                        diags.AddRange(compDiags);
                    }
                }
            }
            else
            {
                // FALLBACK: alte Minimal-Compilation (nur wenn MSBuild nicht bereit ist)
                var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(buffer);
                var refs = IntelliSenseService.GetBasicReferences();
                var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
                    "Live",
                    new[] { tree },
                    refs,
                    new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                diags.AddRange(compilation.GetDiagnostics());
            }

            // Filtern + bereinigen
            var final = diags
                .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error && !d.IsSuppressed)
                .Where(d => d.Location.IsInSource)
                .GroupBy(d => (d.Id, d.Location.SourceSpan.Start, d.Location.SourceSpan.Length))
                .Select(g => g.First())
                .ToList();

            // Squiggles rendern
            OnUi(() =>
            {
                // ALLE Editor-Zugriffe hier drin!
                //Editor.IndicatorCurrent = 0;
                //Editor.IndicatorClearRange(0, Editor.TextLength);

                Editor.IndicatorCurrent = 2;                    // unser Methoden-Indicator
                Editor.IndicatorClearRange(0, Editor.TextLength); // nur diesen Indicator leeren


                foreach (var d in final)
                {
                    if (!d.Location.IsInSource) continue;
                    var span = d.Location.SourceSpan;
                    int start = Math.Max(0, Math.Min(span.Start, Editor.TextLength));
                    int len = Math.Max(1, Math.Min(span.Length, Editor.TextLength - start));
                    Editor.IndicatorFillRange(start, len);
                }

                _currentDiagnostics.Clear();
                _currentDiagnostics.AddRange(final);
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
            // --- VS-Chord: Start mit Ctrl+M ---
            if (e.Control && e.KeyCode == Keys.M && !_vsChordPending)
            {
                _vsChordPending = true;
                _vsChordSince = DateTime.UtcNow;
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            // --- Zweite Taste des Chords innerhalb Timeout ---
            if (_vsChordPending)
            {
                if ((DateTime.UtcNow - _vsChordSince).TotalMilliseconds > VsChordTimeoutMs)
                {
                    _vsChordPending = false; // Timeout abgelaufen
                }
                else if (e.Control && e.KeyCode == Keys.O)
                {
                    // Ctrl+M, Ctrl+O: Collapse to Definitions
                    SendMessage(Editor.Handle, SCI_FOLDALL, (IntPtr)SC_FOLDACTION_CONTRACT, IntPtr.Zero);
                    _vsChordPending = false;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
                else if (e.Control && e.KeyCode == Keys.L)
                {
                    // Ctrl+M, Ctrl+L: Expand All
                    SendMessage(Editor.Handle, SCI_FOLDALL, (IntPtr)SC_FOLDACTION_EXPAND, IntPtr.Zero);
                    _vsChordPending = false;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
                else if (e.Control && e.KeyCode == Keys.M)
                {
                    // Ctrl+M, Ctrl+M: Toggle aktueller Block
                    int line = Editor.LineFromPosition(Editor.CurrentPosition);
                    SendMessage(Editor.Handle, SCI_TOGGLEFOLD, (IntPtr)line, IntPtr.Zero);
                    _vsChordPending = false;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
            }

            // --- deine bestehenden Shortcuts (z. B. Escape etc.) ---
            if (e.KeyCode == Keys.Escape && Editor.AutoCActive) { Editor.AutoCCancel(); e.Handled = true; }
            if (e.KeyCode == Keys.Escape && Editor.CallTipActive) { Editor.CallTipCancel(); e.Handled = true; }
        }


        private async void Editor_CharAdded(object? sender, CharAddedEventArgs e)
        {
            char c = (char)e.Char;

            if (c == '(' || c == ',')
            {
                // Signaturhilfe (Konstruktoren/Methoden)
                await TrySignatureHelpAsync();
                return;
            }
            if (c == ')')
            {
                Editor.CallTipCancel();
                // optional: danach Completion weitermachen
            }

            if (c == '.')
            {
                if (await TryRoslynCompletionAsync(triggerOnDot: true)) return;
                TryHandleDotCompletion();
            }
            else if (char.IsLetterOrDigit(c) || c == '_')
            {
                await TryRoslynCompletionAsync(triggerOnDot: false);
            }
        }

        // Zeigt/aktualisiert die Parameterhilfe. updateOnly=true => nur Markierung neu setzen.
        private async Task<bool> TrySignatureHelpAsync(bool updateOnly = false)
        {
            // Wenn der Workspace bereit ist, arbeiten wir stets auf dem echten Document.
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

            // Lauf nach oben bis Aufruf oder Objekt-Erzeugung gefunden
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
            // Index = Anzahl der Argumente, deren Span komplett vor dem Caret endet
            int idx = 0;
            foreach (var a in args.Arguments)
            {
                if (caretPos > a.Span.End) idx++;
                else break;
            }
            // Begrenzen
            if (idx < 0) idx = 0;
            if (idx >= args.Arguments.Count) idx = Math.Max(0, args.Arguments.Count - 1);
            return idx;
        }

        private async Task<bool> ShowSignatureForCreationAsync(ObjectCreationExpressionSyntax obj, RoslynDocument doc, int caretPos, bool updateOnly)
        {
            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            // Zieltyp bestimmen
            var type = model.GetTypeInfo(obj).Type as INamedTypeSymbol
                       ?? model.GetSymbolInfo(obj.Type).Symbol as INamedTypeSymbol;
            if (type == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var ctors = type.InstanceConstructors
                            .Where(c => !c.IsStatic)
                            .OrderBy(c => c.Parameters.Length)
                            .ToArray();
            if (ctors.Length == 0) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            int argIndex = GetArgumentIndex(obj.ArgumentList!, caretPos);
            return ShowSignatureCallTip(type, ctors, argIndex, updateOnly);
        }

        private async Task<bool> ShowSignatureForInvocationAsync(InvocationExpressionSyntax inv, RoslynDocument doc, int caretPos, bool updateOnly)
        {
            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var info = model.GetSymbolInfo(inv);

            // FIX: beide Seiten auf denselben Typ bringen (Array)
            var methodGroup = info.Symbol != null
                ? new[] { info.Symbol }
                : (info.CandidateSymbols.IsDefaultOrEmpty ? Array.Empty<ISymbol>() : info.CandidateSymbols.ToArray());

            var methods = methodGroup.OfType<IMethodSymbol>().ToArray();

            if (methods.Length == 0)
            {
                // Fallback: Typ der linken Seite + Methodenname aus Syntax ermitteln
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
            // Text für Overloads bauen
            var formatType = SymbolDisplayFormat.MinimallyQualifiedFormat;
            var formatParam = SymbolDisplayFormat.MinimallyQualifiedFormat;

            var lines = new List<string>();
            var spanForHighlight = (start: -1, length: 0);

            // Nimm das "beste" Overload als erstes: jenes mit >= argIndex+1 Parametern, sonst erstes
            var ordered = overloads.OrderBy(m => m.Parameters.Length).ToList();
            var best = ordered.FirstOrDefault(m => m.Parameters.Length > argIndex) ?? ordered.First();

            foreach (var m in ordered.Take(12)) // Limit auf 12 Overloads
            {
                var typeName = containerType.ToDisplayString(formatType);
                var namePart = m.MethodKind == MethodKind.Constructor ? typeName : $"{typeName}.{m.Name}";

                var paramPieces = new List<string>();
                int highlightStartInLine = -1, highlightLen = 0;

                for (int i = 0; i < m.Parameters.Length; i++)
                {
                    var p = m.Parameters[i];
                    string piece = p.ToDisplayString(formatParam); // "int count" / "string? name" etc.

                    if (m.Equals(best) && i == argIndex)
                    {
                        highlightStartInLine = string.Join(", ", paramPieces).Length + (paramPieces.Count > 0 ? 2 : 0);
                        highlightLen = piece.Length;
                    }

                    paramPieces.Add(piece);
                }

                string paramList = string.Join(", ", paramPieces);
                string line = $"{namePart}({paramList})";
                if (m.Equals(best)) spanForHighlight = (start: lines.Sum(l => l.Length + 1) + namePart.Length + 1 + (paramPieces.Count > 0 ? 0 : 0) + (paramPieces.Count > 0 ? 0 : 0), length: 0); // dummy, wir highlighten gleich separat
                lines.Add(line);

                // Wir merken uns Highlight relativ zur gesamten CallTip-Zeichenkette später
                if (m.Equals(best) && highlightStartInLine >= 0)
                {
                    // globaler Offset = Länge aller Zeilen vor dieser + Newlines
                    int globalOffset = lines.Take(lines.Count - 1).Sum(x => x.Length + 1) + namePart.Length + 1; // + '('
                    spanForHighlight = (globalOffset + highlightStartInLine, highlightLen);
                }
            }

            string text = string.Join("\n", lines);
            if (!Editor.CallTipActive)
            {
                Editor.CallTipShow(Editor.CurrentPosition, text);
            }
            else
            {
                // CallTip neu zeigen, um Inhalt zu wechseln (Scintilla erlaubt kein „Editieren“ des CallTips)
                Editor.CallTipCancel();
                Editor.CallTipShow(Editor.CurrentPosition, text);
            }

            // Parameter hervorheben, wenn API vorhanden
            try
            {
                // ScintillaNET 6 hat CallTipSetHlt
                if (spanForHighlight.start >= 0 && spanForHighlight.length > 0)
                    Editor.CallTipSetHlt(spanForHighlight.start, spanForHighlight.start + spanForHighlight.length);
            }
            catch
            {
                // Fallback: kein Highlight – trotzdem nützlich
            }

            return true;
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
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '.') start--;
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

       

        private async Task<bool> TryRoslynCompletionAsync(bool triggerOnDot)
        {
            int pos = Editor.CurrentPosition;
            // Roslyn expects position in absolute buffer index
            var items = await IntelliSenseService.GetCompletionsAsync(Editor.Text, pos, _currentFile);
            if (items == null || items.Count == 0) return false;

            // Map items to display/insertion
            var texts = new List<string>();
            foreach (var it in items)
            {
                // Prefer display text; insertion text will be handled by Scintilla's insertion of the selected token
                var display = it.DisplayText;
                if (string.IsNullOrWhiteSpace(display)) continue;
                // Filter out weird generic arity artifacts
                int tick = display.IndexOf('`');
                if (tick >= 0) display = display.Substring(0, tick);
                texts.Add(display);
                if (texts.Count >= 200) break;
            }
            if (texts.Count == 0) return false;

            // Determine the start length for replacement
            int wordStart = Editor.WordStartPosition(pos, true);
            int len = Math.Max(0, pos - wordStart);
            Editor.AutoCShow(len, string.Join(" ", texts.Distinct().OrderBy(x => x)));
            return true;
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

                // Minimale + häufig genutzte libs (gern erweitern, wenn euch etwas fehlt)
                string[] need = {
                    "System.Private.CoreLib",
                    "System.Runtime",
                    "System.Console",
                    "System.Linq",
                    "System.Private.Uri",
                    "System.Collections",
                    "System.IO",
                    "System.Private.Xml",
                    "System.Private.Xml.Linq",
                    "System.ObjectModel",
                    "System.Runtime.Extensions",
                    "netstandard",
                    "System.Drawing",
                    "System.Drawing.Primitives",
                    "System.Windows.Forms",
                    "System.Memory",
                    "System.Text.RegularExpressions",
                    "System.Text.Json",
                    "System.Linq.Expressions",
                    "System.ComponentModel.Primitives",
                    "System.ComponentModel.TypeConverter"
                };

                foreach (var p in tpa)
                {
                    var name = Path.GetFileNameWithoutExtension(p);
                    if (need.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        try { refs.Add(MetadataReference.CreateFromFile(p)); } catch { /* ignore */ }
                    }
                }
            }
            catch { /* ignore */ }

            // + vom Host registrierte Zusatz-Refs (dll-Ordner usw.)
            refs.AddRange(_extraRefs.Values);
            return refs;
        }

        private static (RoslynDocument? doc, RoslynWorkspace ws) EnsureAdhocDocument(string buffer, string? filePath)
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
