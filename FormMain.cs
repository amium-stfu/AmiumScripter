using AmiumScripter.Core;
using AmiumScripter.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AmiumScripter.Helpers;

namespace AmiumScripter
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        int dummyPageCounter = 0;
        private void btnAddClass_Click(object sender, EventArgs e)
        {
            ClassManager.AddClass(ProjectManager.Project.Name, "TestModule");
            dummyPageCounter = 0;
        }

        private void btnAddPage_Click(object sender, EventArgs e)
        {
            dummyPageCounter++;
            PageManager.AddPage(ProjectManager.Project.Name, $"Page{dummyPageCounter}");
        }

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            ProjectManager.OpenEditor();
        }



        private void btnAddProject_Click(object sender, EventArgs e)
        {
            dummyPageCounter = 0;
            dummySignalCounter = 0;
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
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (ProjectManager.Project == null)
            {
                MessageBox.Show("No project loaded. Please create or load a project first.");
                return;
            }
            ProjectManager.BuildProject();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            ProjectManager.RunProject();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            ProjectManager.StopProject();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            FormCodeEditor editor = new FormCodeEditor();
            editor.Show();
            string uri = ProjectManager.GetProjectPath(ProjectManager.Project.Name);
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
                source: "Motor.Temperature" // SourceName im SignalView
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
                source: "Motor.Temperature" // SourceName im SignalView
                );
            }
        }
    }

}
