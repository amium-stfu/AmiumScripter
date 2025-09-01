using AmiumScripter.Core;
using AmiumScripter.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmiumScripter.UI
{
    public partial class UserControlPage : UserControl
    {
        public BaseView View;

       
        public UserControlPage()
        {
            InitializeComponent();
      
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PageText
        {
            get => lblPageText.Text;
            set => lblPageText.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PageNumber
        {
            get => lblPageNumber.Text;
            set => lblPageNumber.Text = value;
        }

        private void lblPageNumber_Click(object sender, EventArgs e)
        {
            SelectView();


            //foreach (Control ctrl in Parent.Controls)
            //{
            //    if (ctrl is UserControlPage page)
            //    {
            //        page.Deselect();
            //    }
            //}

            //lblPageNumber.BackColor = Color.Orange;
            //AmiumScripter.Root.Main.ShowPageView(View);
            //Debug.WriteLine("Selected Page " + View.Name);
            //UIEditor.CurrentPageName = View.Name;
        }

        public void Deselect()
        {
            lblPageNumber.BackColor = Color.LightGray;
        }

        public void SelectView()
        {
            foreach (Control ctrl in Parent.Controls)
            {
                if (ctrl is UserControlPage page)
                {
                    page.Deselect();
                }
            }

            lblPageNumber.BackColor = Color.Orange;
            AmiumScripter.Root.Main.ShowPageView(View);
            Debug.WriteLine("Selected Page " + View.Name);
            UIEditor.CurrentPageName = View.Name;
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
          
        }
    }
}
