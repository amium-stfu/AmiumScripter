using AmiumScripter.Core;
using AmiumScripter.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
            string dummy = DateTime.Now.ToString("yyyyMMddHHmmss");
            ProjectManager.CreateProject("MeinModulProjekt_" + dummy);
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
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
            UIEditor.AddSignalControl(
            name: $"Dummy{dummySignalCounter}",
            page: UIEditor.CurrentPageName,
            source: "Motor.Temperature" // SourceName im SignalView
);
            dummySignalCounter++;
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
    }

}
