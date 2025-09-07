using AmiumScripter.Core;
using AmiumScripter.Forms;
using AmiumScripter.Helpers;
using AmiumScripter.UI;
using FontAwesome.Sharp;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AmiumScripter
{
    public partial class FormMain : Form
    {
        ContextMenuStrip popupMenu = new ContextMenuStrip();
        private ToolStrip toolStrip;

        public FormMain()
        {
            InitializeComponent();
            this.KeyPreview = true;

            //ProjectManager.OnBuildProject = () => UpdateProjectButtons();
            //ProjectManager.OnRunProject = () => UpdateProjectButtons();
            //ProjectManager.OnStopProject = () => UpdateProjectButtons();

        }

        int dummyPageCounter = 0;

        void UpdateProjectButtons()
        {
            Debug.WriteLine("..");
            btnBuildProject.IconColor = ProjectManager.BuildSuccess ? Color.Orange : Color.Black;
            btnRunProject.IconColor = Color.Orange;
            btnStopProject.IconColor = Color.Orange;
        }

        public void DeletePages() => panelPages.Controls.Clear();

        public void AddPageToUI(string pageName, BaseView view, int count)
        {
            var Page = new UserControlPage();
            Page.PageText = pageName;
            Page.PageNumber = count.ToString();
            Page.View = view;

            Page.Dock = DockStyle.Top;
            panelPages.Controls.Add(Page);
        }

        public void OnOpenProject()
        {
            UserControlPage page = panelPages.Controls[panelPages.Controls.Count - 1] as UserControlPage;

            if (ProjectManager.BuildSuccess)
            {
                page.SelectView();
            }
        }

        public void ShowPageView(BaseView view)
        {
            foreach (UserControlPage page in panelPages.Controls)
            {
                if (page.View != view)
                {
                    page.Deselect();
                }
            }

            Control oldControl = PanelRoot.GetControlFromPosition(3, 3);

            if (oldControl == view)
            {
                return; // Nichts zu tun, wenn die Ansicht bereits angezeigt wird
            }
            if (oldControl != null)
            {
                PanelRoot.Controls.Remove(oldControl);
                // oldControl.Dispose(); // Optional: Speicher freigeben
            }

            view.Dock = DockStyle.Fill;
            PanelRoot.Controls.Add(view, 3, 3);
            PanelRoot.SetRowSpan(view, 2);
            lblPage.Text = view.PageText;

        }


        private void btnAddClass_Click(object sender, EventArgs e)
        {
            ClassManager.AddClass(ProjectManager.Project.Name, "TestModule");
            dummyPageCounter = 0;
        }

        private void btnAddPage_Click(object sender, EventArgs e)
        {
            int c = ProjectManager.Project.Pages.Count + 1;
            string name = "Page" + c;

            if (EditValue.WithKeyboardDialog(ref name, "Enter Project Name"))
            {
                name = Functions.RemoveInvalidChars(name);
                Logger.DebugMsg("[FormMain] Button AddPage confirmed");
                PageManager.AddPage(ProjectManager.Project.Name, $"{name}");
                ProjectManager.BuildProject();
            }

        }

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            ProjectManager.OpenEditor();
            this.ActiveControl = null;
        }



        private void btnAddProject_Click(object sender, EventArgs e)
        {

            string name = "MyProject_v." + DateTime.Now.ToString("yyyyMMddHHmmss");

            if (EditValue.WithKeyboardDialog(ref name, "Enter Project Name"))
            {
                Logger.DebugMsg("[FormMain] Button AddProjectDialog confirmed");
                ProjectManager.CreateProject(name);
            }
            else
            {
                Logger.DebugMsg("[FormMain] Button AddProjectDialog canceled");
            }
            this.ActiveControl = null;
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (ProjectManager.Project == null)
            {
                MessageBox.Show("No project loaded. Please create or load a project first.");
                return;
            }
            btnBuildProject.IconColor = Color.Black;
            ProjectManager.BuildProject();
            if (ProjectManager.BuildSuccess)
            {

                OnOpenProject();
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

            panelPages.Controls.Clear();
            this.ActiveControl = null;
        }

      

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // ProjectManager.ProjectBrowser();

            ProjectManager.LoadFromAScript();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //ProjectManager.SaveProject(ProjectManager.Project);
            ProjectManager.Save();
        }

        int dummySignalCounter = 0;
        private void btnAddSignal_Click(object sender, EventArgs e)
        {
            string name = "";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                UIEditor.AddSignalControl(
                name: name,
                page: UIEditor.CurrentPageName,
                source: "" // SourceName im SignalView
                );
            }
        }



        private void Book_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl?.SelectedTab != null)
            {
                UIEditor.CurrentPageName = tabControl.SelectedTab.Name;
                //   MessageBox.Show($"Aktuelle Seite: {UIEditor.CurrentPageName}");
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            ProjectManager.SaveAs();
        }

        private void btnAddDll_Click(object sender, EventArgs e)
        {
            ProjectManager.AddDllFile();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            Root.LogForm.Show();
        }

        private void btnSignalPool_Click(object sender, EventArgs e)
        {
            FormSignalPool signalPoolForm = new FormSignalPool();
            signalPoolForm.Show();

        }

        private void btnAddStringSignal_Click(object sender, EventArgs e)
        {
            string name = "";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                UIEditor.AddStringSignalControl(
                name: name,
                page: UIEditor.CurrentPageName,
                source: "" // SourceName im SignalView
                );
            }
        }



        private void iconButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void btnEditView_Click(object sender, EventArgs e)
        {
            UIEditor.EditMode = !UIEditor.EditMode;

            btnEditView.BackColor = UIEditor.EditMode ? Color.Orange : Color.White;
            this.ActiveControl = null;


        }

        private void btnMenuBar_Click(object sender, EventArgs e)
        {
            PanelRoot.ColumnStyles[1].Width = PanelRoot.ColumnStyles[1].Width == 0 ? 40 : 0;
            btnMenuBar.IconChar = PanelRoot.ColumnStyles[1].Width == 0 ? FontAwesome.Sharp.IconChar.CaretRight : FontAwesome.Sharp.IconChar.CaretLeft;
            this.ActiveControl = null;

        }

        private void PanelRoot_Paint(object sender, PaintEventArgs e)
        {

        }

        private void iconButton1_Click_1(object sender, EventArgs e)
        {
            FormFile rootFile = new FormFile();
            //  Point buttonScreenPos = iconButton1.PointToScreen(new Point(iconButton1.Left, iconButton1.Top));
            Point buttonScreenPos = iconButton1.PointToScreen(new Point(0, 0));
            rootFile.StartPosition = FormStartPosition.Manual;
            rootFile.Location = buttonScreenPos;
            rootFile.ShowDialog();
            this.ActiveControl = null;
        }

        private void btnPageUp_Click(object sender, EventArgs e)
        {
            string page = UIEditor.CurrentPageName;
            ProjectManager.MovePage(page, -1);
            ProjectManager.BuildProject();

            foreach (UserControlPage c in panelPages.Controls)
            {
                if (c.View.Name == page)
                {
                    c.SelectView();
                }

            }
        }

        private void btnPageDown_Click(object sender, EventArgs e)
        {
            string page = UIEditor.CurrentPageName;
            ProjectManager.MovePage(page, 1);
            ProjectManager.BuildProject();

            foreach (UserControlPage c in panelPages.Controls)
            {
                if (c.View.Name == page)
                {
                    c.SelectView();
                }

            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Control)
            {
                ProjectManager.Save();
            }

            if (e.KeyCode == Keys.O && e.Control)
            {
                ProjectManager.LoadFromAScript();
                ProjectManager.BuildProject();

                AmiumScripter.Root.Main.OnOpenProject();
                ProjectManager.RunProject();
            }

            if (e.KeyCode == Keys.S && e.Control && e.Shift)
            {
                ProjectManager.SaveAs();
            }

            if (e.KeyCode == Keys.B && e.Control)
            {
                ProjectManager.BuildProject();
            }

            if (e.KeyCode == Keys.R && e.Control)
            {
                ProjectManager.RunProject();
            }

            if (e.KeyCode == Keys.X && e.Control)
            {
                ProjectManager.StopProject();
            }

            if (e.KeyCode == Keys.Oemplus && e.Control)
            {
                this.Close();
                string name = "MyProject_v." + DateTime.Now.ToString("yyyyMMddHHmmss");

                if (EditValue.WithKeyboardDialog(ref name, "Enter Project Name"))
                {
                    ProjectManager.SaveAs(name);
                }
            }

            if (e.KeyCode == Keys.Add && e.Control)
            {
                this.Close();
                string name = "MyProject_v." + DateTime.Now.ToString("yyyyMMddHHmmss");

                if (EditValue.WithKeyboardDialog(ref name, "Enter Project Name"))
                {
                    ProjectManager.SaveAs(name);
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            CodeEditorForm Code = new CodeEditorForm();
            Code.Show();

        }
    }

}
