namespace AmiumScripter.UI
{
    partial class FormFile
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            iconButton5 = new AmiumScripter.Controls.IconButton();
            iconButton4 = new AmiumScripter.Controls.IconButton();
            iconButton3 = new AmiumScripter.Controls.IconButton();
            iconButton2 = new AmiumScripter.Controls.IconButton();
            iconButton1 = new AmiumScripter.Controls.IconButton();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(iconButton5);
            panel1.Controls.Add(iconButton4);
            panel1.Controls.Add(iconButton3);
            panel1.Controls.Add(iconButton2);
            panel1.Controls.Add(iconButton1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(10);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(10);
            panel1.Size = new Size(221, 250);
            panel1.TabIndex = 0;
            panel1.Paint += panel1_Paint;
            // 
            // iconButton5
            // 
            iconButton5.BorderColor = Color.Transparent;
            iconButton5.ButtonIcon = "fa:person-walking:Black";
            iconButton5.ButtonText = "Exit";
            iconButton5.Dock = DockStyle.Top;
            iconButton5.HoverColor = Color.DarkOrange;
            iconButton5.Location = new Point(10, 190);
            iconButton5.Name = "iconButton5";
            iconButton5.ShortcutText = "";
            iconButton5.SignalValue = "Unknown";
            iconButton5.Size = new Size(201, 45);
            iconButton5.TabIndex = 2;
            iconButton5.Text = "iconButton5";
            iconButton5.Click += iconButton5_Click;
            // 
            // iconButton4
            // 
            iconButton4.BorderColor = Color.Transparent;
            iconButton4.ButtonIcon = "fa:plus:Black";
            iconButton4.ButtonText = "New";
            iconButton4.Dock = DockStyle.Top;
            iconButton4.HoverColor = Color.DarkOrange;
            iconButton4.Location = new Point(10, 145);
            iconButton4.Name = "iconButton4";
            iconButton4.ShortcutText = "Ctrl + +";
            iconButton4.SignalValue = "Unknown";
            iconButton4.Size = new Size(201, 45);
            iconButton4.TabIndex = 1;
            iconButton4.Text = "iconButton4";
            iconButton4.Click += iconButton4_Click;
            // 
            // iconButton3
            // 
            iconButton3.BorderColor = Color.Transparent;
            iconButton3.ButtonIcon = "fa:floppy-disk:Blue";
            iconButton3.ButtonText = "Save as";
            iconButton3.Dock = DockStyle.Top;
            iconButton3.HoverColor = Color.DarkOrange;
            iconButton3.Location = new Point(10, 100);
            iconButton3.Name = "iconButton3";
            iconButton3.Padding = new Padding(10);
            iconButton3.ShortcutText = "Ctrl + Shift + S";
            iconButton3.SignalValue = "Unknown";
            iconButton3.Size = new Size(201, 45);
            iconButton3.TabIndex = 0;
            iconButton3.Text = "iconButton1";
            iconButton3.Click += iconButton3_Click;
            iconButton3.KeyDown += FormFile_KeyDown;
            // 
            // iconButton2
            // 
            iconButton2.BorderColor = Color.Transparent;
            iconButton2.ButtonIcon = "fa:floppy-disk:Black";
            iconButton2.ButtonText = "Save";
            iconButton2.Dock = DockStyle.Top;
            iconButton2.HoverColor = Color.DarkOrange;
            iconButton2.Location = new Point(10, 55);
            iconButton2.Name = "iconButton2";
            iconButton2.Padding = new Padding(10);
            iconButton2.ShortcutText = "Ctrl + S";
            iconButton2.SignalValue = "Unknown";
            iconButton2.Size = new Size(201, 45);
            iconButton2.TabIndex = 0;
            iconButton2.Text = "iconButton1";
            iconButton2.Click += iconButton2_Click;
            iconButton2.KeyDown += FormFile_KeyDown;
            // 
            // iconButton1
            // 
            iconButton1.BorderColor = Color.Transparent;
            iconButton1.ButtonIcon = "fa:file:Black";
            iconButton1.ButtonText = "Open";
            iconButton1.Dock = DockStyle.Top;
            iconButton1.HoverColor = Color.DarkOrange;
            iconButton1.Location = new Point(10, 10);
            iconButton1.Name = "iconButton1";
            iconButton1.Padding = new Padding(10);
            iconButton1.ShortcutText = "Ctrl + O";
            iconButton1.SignalValue = "Unknown";
            iconButton1.Size = new Size(201, 45);
            iconButton1.TabIndex = 0;
            iconButton1.Text = "iconButton1";
            iconButton1.Click += iconButton1_Click;
            iconButton1.KeyDown += FormFile_KeyDown;
            // 
            // FormFile
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(221, 250);
            ControlBox = false;
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            Name = "FormFile";
            Click += FormFile_Click;
            KeyDown += FormFile_KeyDown;
            Leave += FormFile_Leave;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Controls.IconButton iconButton3;
        private Controls.IconButton iconButton2;
        private Controls.IconButton iconButton1;
        private Controls.IconButton iconButton5;
        private Controls.IconButton iconButton4;
    }
}
