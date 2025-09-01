using AmiumScripter.Core;
using AmiumScripter.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmiumScripter.UI
{
    public partial class FormFile : Form
    {
        private Action _onCloseCallback;


        public FormFile()
        {
            InitializeComponent();
            this.FormClosed += (s, e) => _onCloseCallback?.Invoke();

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FormFile_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            this.Close();

            ProjectManager.LoadFromAScript();

            if(!ProjectManager.BuildSuccess) return;

            ProjectManager.BuildProject();
            ProjectManager.RunProject();
            AmiumScripter.Root.Main.OnOpenProject();
        }

        private void iconButton4_Click(object sender, EventArgs e)
        {
            this.Close();
            string name = "MyProject_v." + DateTime.Now.ToString("yyyyMMddHHmmss");

            if (EditValue.WithKeyboardDialog(ref name, "Enter Project Name"))
            {
                ProjectManager.SaveAs(name);
            }

            this.ActiveControl = null;
        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            this.Close();
            ProjectManager.SaveAs();
            this.ActiveControl = null;
        }

        private void iconButton5_Click(object sender, EventArgs e)
        {
            ProjectManager.StopProject();
            Application.Exit();

        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            ProjectManager.Save();
            this.ActiveControl = null;
        }

        private void FormFile_Leave(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormFile_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
