namespace AmiumScripter.UI
{
    partial class FormAddControl
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            btnAddIconButton = new AmiumScripter.Controls.IconButton();
            btnAddSimpleButton = new AmiumScripter.Controls.IconButton();
            btnAddStringSignal = new AmiumScripter.Controls.IconButton();
            btnAddModule = new AmiumScripter.Controls.IconButton();
            btnAddSignal = new AmiumScripter.Controls.IconButton();
            btnAddChart = new AmiumScripter.Controls.IconButton();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnAddChart);
            panel1.Controls.Add(btnAddIconButton);
            panel1.Controls.Add(btnAddSimpleButton);
            panel1.Controls.Add(btnAddStringSignal);
            panel1.Controls.Add(btnAddModule);
            panel1.Controls.Add(btnAddSignal);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(10);
            panel1.Size = new Size(284, 341);
            panel1.TabIndex = 0;
            // 
            // btnAddIconButton
            // 
            btnAddIconButton.BorderColor = Color.Transparent;
            btnAddIconButton.ButtonIcon = "fa:plus:black";
            btnAddIconButton.ButtonText = "IconButton";
            btnAddIconButton.Dock = DockStyle.Top;
            btnAddIconButton.GridScale = 5;
            btnAddIconButton.HoverColor = Color.DarkOrange;
            btnAddIconButton.Location = new Point(10, 190);
            btnAddIconButton.Name = "btnAddIconButton";
            btnAddIconButton.ShortcutText = "";
            btnAddIconButton.SignalValue = "Unknown";
            btnAddIconButton.Size = new Size(264, 45);
            btnAddIconButton.TabIndex = 0;
            btnAddIconButton.Text = "iconButton1";
            btnAddIconButton.Click += btnAddIconButton_Click;
            btnAddIconButton.KeyDown += FormAddControl_KeyDown;
            // 
            // btnAddSimpleButton
            // 
            btnAddSimpleButton.BorderColor = Color.Transparent;
            btnAddSimpleButton.ButtonIcon = "fa:plus:black";
            btnAddSimpleButton.ButtonText = "SimpleButton";
            btnAddSimpleButton.Dock = DockStyle.Top;
            btnAddSimpleButton.GridScale = 5;
            btnAddSimpleButton.HoverColor = Color.DarkOrange;
            btnAddSimpleButton.Location = new Point(10, 145);
            btnAddSimpleButton.Name = "btnAddSimpleButton";
            btnAddSimpleButton.ShortcutText = "";
            btnAddSimpleButton.SignalValue = "Unknown";
            btnAddSimpleButton.Size = new Size(264, 45);
            btnAddSimpleButton.TabIndex = 0;
            btnAddSimpleButton.Text = "iconButton1";
            btnAddSimpleButton.Click += btnAddSimpleButton_Click;
            btnAddSimpleButton.KeyDown += FormAddControl_KeyDown;
            // 
            // btnAddStringSignal
            // 
            btnAddStringSignal.BorderColor = Color.Transparent;
            btnAddStringSignal.ButtonIcon = "fa:plus:black";
            btnAddStringSignal.ButtonText = "StringSignal";
            btnAddStringSignal.Dock = DockStyle.Top;
            btnAddStringSignal.GridScale = 5;
            btnAddStringSignal.HoverColor = Color.DarkOrange;
            btnAddStringSignal.Location = new Point(10, 100);
            btnAddStringSignal.Name = "btnAddStringSignal";
            btnAddStringSignal.ShortcutText = "";
            btnAddStringSignal.SignalValue = "Unknown";
            btnAddStringSignal.Size = new Size(264, 45);
            btnAddStringSignal.TabIndex = 0;
            btnAddStringSignal.Text = "iconButton1";
            btnAddStringSignal.Click += btnAddStringSignal_Click;
            btnAddStringSignal.KeyDown += FormAddControl_KeyDown;
            // 
            // btnAddModule
            // 
            btnAddModule.BorderColor = Color.Transparent;
            btnAddModule.ButtonIcon = "fa:plus:black";
            btnAddModule.ButtonText = "Module";
            btnAddModule.Dock = DockStyle.Top;
            btnAddModule.GridScale = 5;
            btnAddModule.HoverColor = Color.DarkOrange;
            btnAddModule.Location = new Point(10, 55);
            btnAddModule.Name = "btnAddModule";
            btnAddModule.ShortcutText = "";
            btnAddModule.SignalValue = "Unknown";
            btnAddModule.Size = new Size(264, 45);
            btnAddModule.TabIndex = 0;
            btnAddModule.Text = "iconButton1";
            btnAddModule.Click += btnAddModule_Click;
            btnAddModule.KeyDown += FormAddControl_KeyDown;
            // 
            // btnAddSignal
            // 
            btnAddSignal.BorderColor = Color.Transparent;
            btnAddSignal.ButtonIcon = "fa:plus:black";
            btnAddSignal.ButtonText = "Signal";
            btnAddSignal.Dock = DockStyle.Top;
            btnAddSignal.GridScale = 5;
            btnAddSignal.HoverColor = Color.DarkOrange;
            btnAddSignal.Location = new Point(10, 10);
            btnAddSignal.Name = "btnAddSignal";
            btnAddSignal.ShortcutText = "";
            btnAddSignal.SignalValue = "Unknown";
            btnAddSignal.Size = new Size(264, 45);
            btnAddSignal.TabIndex = 0;
            btnAddSignal.Text = "iconButton1";
            btnAddSignal.Click += iconButton1_Click;
            btnAddSignal.KeyDown += FormAddControl_KeyDown;
            // 
            // btnAddChart
            // 
            btnAddChart.BorderColor = Color.Transparent;
            btnAddChart.ButtonIcon = "fa:plus:black";
            btnAddChart.ButtonText = "Chart";
            btnAddChart.Dock = DockStyle.Top;
            btnAddChart.GridScale = 5;
            btnAddChart.HoverColor = Color.DarkOrange;
            btnAddChart.Location = new Point(10, 235);
            btnAddChart.Name = "btnAddChart";
            btnAddChart.ShortcutText = "";
            btnAddChart.SignalValue = "Unknown";
            btnAddChart.Size = new Size(264, 45);
            btnAddChart.TabIndex = 0;
            btnAddChart.Text = "Chart";
            btnAddChart.Click += btnAddChart_Click;
            btnAddChart.KeyDown += FormAddControl_KeyDown;
            // 
            // FormAddControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 341);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormAddControl";
            StartPosition = FormStartPosition.Manual;
            Text = "FormAddControl";
            KeyDown += FormAddControl_KeyDown;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Controls.IconButton btnAddSignal;
        private Controls.IconButton btnAddIconButton;
        private Controls.IconButton btnAddSimpleButton;
        private Controls.IconButton btnAddStringSignal;
        private Controls.IconButton btnAddModule;
        private Controls.IconButton btnAddChart;
    }
}