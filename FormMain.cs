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

        private void btnAddClass_Click(object sender, EventArgs e)
        {
            ClassManager.AddClass("MeinModulProjekt", "TestModule");

        }

        private void btnAddPage_Click(object sender, EventArgs e)
        {
            PageManager.AddPage("MeinModulProjekt", "TestPage");
        }

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            ProjectManager.OpenEditor();
        }



        private void btnAddProject_Click(object sender, EventArgs e)
        {
            ProjectManager.CreateProject("MeinModulProjekt");
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
            ProjectManager.ProjectBrowser();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ProjectManager.SaveProject(ProjectManager.Project);
        }

        private void btnAddSignal_Click(object sender, EventArgs e)
        {
            UIEditor.AddSignalControl(
            name: "MotorTemp",
            page: "TestPage",
            source: "Motor.Temperature" // SourceName im SignalView
);
        }

        private void btnBuildUi_Click(object sender, EventArgs e)
        {
            UIEditor.CreateAllViews();
        }
    }

}
