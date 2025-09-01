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
    public partial class FormAddControl : Form
    {
        public FormAddControl()
        {
            InitializeComponent();
            panel1.AutoScroll = false;
            panel1.HorizontalScroll.Enabled = false;
            panel1.HorizontalScroll.Visible = false;
            panel1.HorizontalScroll.Maximum = 0;
            panel1.AutoScroll = true;

        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            this.Close();
            string name = "";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                UIEditor.AddSignalControl(
                name: name,
                page: UIEditor.CurrentPageName,
                source: "",
                x: UIEditor.NewControl.X,
                y: UIEditor.NewControl.Y,
                h: UIEditor.NewControl.H,
                w: UIEditor.NewControl.W
                );
            }
        }

        private void btnAddStringSignal_Click(object sender, EventArgs e)
        {
            this.Close();
            string name = "";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                UIEditor.AddStringSignalControl(
                name: name,
                page: UIEditor.CurrentPageName,
                source: "",
                x: UIEditor.NewControl.X,
                y: UIEditor.NewControl.Y,
                h: UIEditor.NewControl.H,
                w: UIEditor.NewControl.W
                );
            }
        }

        private void btnAddSimpleButton_Click(object sender, EventArgs e)
        {
            this.Close();
            string name = "";
            string text = "Button";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                EditValue.WithKeyboardDialog(ref text, "Enter Button Text");
                UIEditor.AddSimpleButtonControl(
                name: name,
                text: text,
                page: UIEditor.CurrentPageName,
                x: UIEditor.NewControl.X,
                y: UIEditor.NewControl.Y,
                h: UIEditor.NewControl.H,
                w: UIEditor.NewControl.W
                );
            }
        }

        private void FormAddControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void btnAddModule_Click(object sender, EventArgs e)
        {
            this.Close();
            string name = "";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                UIEditor.AddModuleControl(
                name: name,
                page: UIEditor.CurrentPageName,
                source: "",
                x: UIEditor.NewControl.X,
                y: UIEditor.NewControl.Y,
                h: UIEditor.NewControl.H,
                w: UIEditor.NewControl.W
                );
            }
        }

        private void btnAddIconButton_Click(object sender, EventArgs e)
        {
            this.Close();
            string name = "";
            string text = "Button";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                EditValue.WithKeyboardDialog(ref text, "Enter Button Text");
                UIEditor.AddIconButtonControl(
                name: name,
                text: text,
                page: UIEditor.CurrentPageName,
                x: UIEditor.NewControl.X,
                y: UIEditor.NewControl.Y,
                h: UIEditor.NewControl.H,
                w: UIEditor.NewControl.W
                );
            }
        }

        private void btnAddChart_Click(object sender, EventArgs e)
        {
            this.Close();
            string name = "";

            if (EditValue.WithKeyboardDialog(ref name, "Enter Control Name"))
            {
                UIEditor.AddChart(
                name: name,
                text: name,
                page: UIEditor.CurrentPageName,
                x: UIEditor.NewControl.X,
                y: UIEditor.NewControl.Y,
                h: UIEditor.NewControl.H,
                w: UIEditor.NewControl.W
                );
            }
        }
    }
}
