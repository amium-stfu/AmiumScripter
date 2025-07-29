using AmiumScripter.Core;
using AmiumScripter.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AmiumScripter
{
    public partial class Form1 : Form
    {
        public Form1()
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
            string vscodePath = @"C:\Users\StefanFunk\AppData\Local\Programs\Microsoft VS Code\Code.exe";
            string projectPath = ProjectManager.GetProjectPath("MeinModulProjekt");
            Debug.WriteLine("project : '" + projectPath + "'");

            Process.Start(vscodePath, $"\"{projectPath}\"");

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
    }

}
