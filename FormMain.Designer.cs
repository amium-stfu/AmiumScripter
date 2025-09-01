namespace AmiumScripter
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            PanelRoot = new TableLayoutPanel();
            panel1 = new Panel();
            lblPage = new Label();
            pictureBox1 = new PictureBox();
            panelPages = new Panel();
            panel4 = new Panel();
            btnAddClass = new FontAwesome.Sharp.IconButton();
            btnAddDll = new FontAwesome.Sharp.IconButton();
            label2 = new Label();
            btnStopProject = new FontAwesome.Sharp.IconButton();
            btnRunProject = new FontAwesome.Sharp.IconButton();
            btnBuildProject = new FontAwesome.Sharp.IconButton();
            label1 = new Label();
            btnSignals = new FontAwesome.Sharp.IconButton();
            btnShowLog = new FontAwesome.Sharp.IconButton();
            btnOpenVS = new FontAwesome.Sharp.IconButton();
            btnEditView = new FontAwesome.Sharp.IconButton();
            label4 = new Label();
            iconButton1 = new FontAwesome.Sharp.IconButton();
            panel2 = new Panel();
            btnMenuBar = new FontAwesome.Sharp.IconButton();
            label3 = new Label();
            panel5 = new Panel();
            btnPageDown = new FontAwesome.Sharp.IconButton();
            btnPageUp = new FontAwesome.Sharp.IconButton();
            binRemovePage = new FontAwesome.Sharp.IconButton();
            btnAddPage = new FontAwesome.Sharp.IconButton();
            toolTip1 = new ToolTip(components);
            iconToolStripButton1 = new FontAwesome.Sharp.IconToolStripButton();
            PanelRoot.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel4.SuspendLayout();
            panel2.SuspendLayout();
            panel5.SuspendLayout();
            SuspendLayout();
            // 
            // PanelRoot
            // 
            PanelRoot.BackColor = Color.White;
            PanelRoot.ColumnCount = 5;
            PanelRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
            PanelRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45F));
            PanelRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2F));
            PanelRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            PanelRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 368F));
            PanelRoot.Controls.Add(panel1, 3, 1);
            PanelRoot.Controls.Add(panelPages, 4, 3);
            PanelRoot.Controls.Add(panel4, 1, 0);
            PanelRoot.Controls.Add(panel2, 0, 5);
            PanelRoot.Controls.Add(label3, 3, 2);
            PanelRoot.Controls.Add(panel5, 4, 4);
            PanelRoot.Dock = DockStyle.Fill;
            PanelRoot.Location = new Point(0, 0);
            PanelRoot.Name = "PanelRoot";
            PanelRoot.RowCount = 6;
            PanelRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            PanelRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            PanelRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            PanelRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            PanelRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            PanelRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            PanelRoot.Size = new Size(1299, 669);
            PanelRoot.TabIndex = 5;
            PanelRoot.Paint += PanelRoot_Paint;
            // 
            // panel1
            // 
            PanelRoot.SetColumnSpan(panel1, 2);
            panel1.Controls.Add(lblPage);
            panel1.Controls.Add(pictureBox1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(60, 30);
            panel1.Name = "panel1";
            panel1.Size = new Size(1236, 49);
            panel1.TabIndex = 0;
            // 
            // lblPage
            // 
            lblPage.Dock = DockStyle.Left;
            lblPage.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblPage.ImageAlign = ContentAlignment.BottomLeft;
            lblPage.Location = new Point(0, 0);
            lblPage.Name = "lblPage";
            lblPage.Size = new Size(867, 49);
            lblPage.TabIndex = 1;
            lblPage.TextAlign = ContentAlignment.BottomLeft;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Right;
            pictureBox1.Image = Properties.Resources.amium;
            pictureBox1.Location = new Point(1097, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(139, 49);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // panelPages
            // 
            panelPages.Dock = DockStyle.Fill;
            panelPages.Location = new Point(934, 86);
            panelPages.Name = "panelPages";
            panelPages.Size = new Size(362, 509);
            panelPages.TabIndex = 2;
            // 
            // panel4
            // 
            panel4.Controls.Add(btnAddClass);
            panel4.Controls.Add(btnAddDll);
            panel4.Controls.Add(label2);
            panel4.Controls.Add(btnStopProject);
            panel4.Controls.Add(btnRunProject);
            panel4.Controls.Add(btnBuildProject);
            panel4.Controls.Add(label1);
            panel4.Controls.Add(btnSignals);
            panel4.Controls.Add(btnShowLog);
            panel4.Controls.Add(btnOpenVS);
            panel4.Controls.Add(btnEditView);
            panel4.Controls.Add(label4);
            panel4.Controls.Add(iconButton1);
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(13, 3);
            panel4.Name = "panel4";
            PanelRoot.SetRowSpan(panel4, 4);
            panel4.Size = new Size(39, 592);
            panel4.TabIndex = 4;
            // 
            // btnAddClass
            // 
            btnAddClass.CausesValidation = false;
            btnAddClass.Dock = DockStyle.Top;
            btnAddClass.FlatAppearance.BorderColor = Color.White;
            btnAddClass.FlatStyle = FlatStyle.Flat;
            btnAddClass.IconChar = FontAwesome.Sharp.IconChar.PuzzlePiece;
            btnAddClass.IconColor = Color.Black;
            btnAddClass.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnAddClass.IconSize = 32;
            btnAddClass.Location = new Point(0, 476);
            btnAddClass.Name = "btnAddClass";
            btnAddClass.Size = new Size(39, 37);
            btnAddClass.TabIndex = 3;
            btnAddClass.TabStop = false;
            toolTip1.SetToolTip(btnAddClass, "Add global\r\nClasses, Tempates, ...");
            btnAddClass.UseVisualStyleBackColor = false;
            btnAddClass.Click += btnSave_Click;
            // 
            // btnAddDll
            // 
            btnAddDll.CausesValidation = false;
            btnAddDll.Dock = DockStyle.Top;
            btnAddDll.FlatAppearance.BorderColor = Color.White;
            btnAddDll.FlatStyle = FlatStyle.Flat;
            btnAddDll.IconChar = FontAwesome.Sharp.IconChar.FileCode;
            btnAddDll.IconColor = Color.Black;
            btnAddDll.IconFont = FontAwesome.Sharp.IconFont.Solid;
            btnAddDll.IconSize = 32;
            btnAddDll.Location = new Point(0, 439);
            btnAddDll.Name = "btnAddDll";
            btnAddDll.Size = new Size(39, 37);
            btnAddDll.TabIndex = 3;
            btnAddDll.TabStop = false;
            toolTip1.SetToolTip(btnAddDll, "Add DLL to Project");
            btnAddDll.UseVisualStyleBackColor = false;
            btnAddDll.Click += btnAddDll_Click;
            // 
            // label2
            // 
            label2.Dock = DockStyle.Top;
            label2.Location = new Point(0, 387);
            label2.Name = "label2";
            label2.Size = new Size(39, 52);
            label2.TabIndex = 5;
            label2.Text = "..";
            // 
            // btnStopProject
            // 
            btnStopProject.CausesValidation = false;
            btnStopProject.Dock = DockStyle.Top;
            btnStopProject.FlatAppearance.BorderColor = Color.White;
            btnStopProject.FlatStyle = FlatStyle.Flat;
            btnStopProject.IconChar = FontAwesome.Sharp.IconChar.CircleStop;
            btnStopProject.IconColor = Color.Black;
            btnStopProject.IconFont = FontAwesome.Sharp.IconFont.Solid;
            btnStopProject.IconSize = 32;
            btnStopProject.Location = new Point(0, 350);
            btnStopProject.Name = "btnStopProject";
            btnStopProject.Size = new Size(39, 37);
            btnStopProject.TabIndex = 3;
            btnStopProject.TabStop = false;
            toolTip1.SetToolTip(btnStopProject, "Stop Project\r\nCtrl + X");
            btnStopProject.UseVisualStyleBackColor = false;
            btnStopProject.Click += btnStop_Click;
            // 
            // btnRunProject
            // 
            btnRunProject.CausesValidation = false;
            btnRunProject.Dock = DockStyle.Top;
            btnRunProject.FlatAppearance.BorderColor = Color.White;
            btnRunProject.FlatStyle = FlatStyle.Flat;
            btnRunProject.IconChar = FontAwesome.Sharp.IconChar.PlayCircle;
            btnRunProject.IconColor = Color.Black;
            btnRunProject.IconFont = FontAwesome.Sharp.IconFont.Solid;
            btnRunProject.IconSize = 32;
            btnRunProject.Location = new Point(0, 313);
            btnRunProject.Name = "btnRunProject";
            btnRunProject.Size = new Size(39, 37);
            btnRunProject.TabIndex = 3;
            btnRunProject.TabStop = false;
            toolTip1.SetToolTip(btnRunProject, "Run Project\r\nCtrl + R");
            btnRunProject.UseVisualStyleBackColor = false;
            btnRunProject.Click += btnRun_Click;
            // 
            // btnBuildProject
            // 
            btnBuildProject.CausesValidation = false;
            btnBuildProject.Dock = DockStyle.Top;
            btnBuildProject.FlatAppearance.BorderColor = Color.White;
            btnBuildProject.FlatStyle = FlatStyle.Flat;
            btnBuildProject.IconChar = FontAwesome.Sharp.IconChar.CheckCircle;
            btnBuildProject.IconColor = Color.Black;
            btnBuildProject.IconFont = FontAwesome.Sharp.IconFont.Solid;
            btnBuildProject.IconSize = 32;
            btnBuildProject.Location = new Point(0, 276);
            btnBuildProject.Name = "btnBuildProject";
            btnBuildProject.Size = new Size(39, 37);
            btnBuildProject.TabIndex = 3;
            btnBuildProject.TabStop = false;
            toolTip1.SetToolTip(btnBuildProject, "Build Project\r\nCtrl + B");
            btnBuildProject.UseVisualStyleBackColor = false;
            btnBuildProject.Click += btnBuild_Click;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(0, 224);
            label1.Name = "label1";
            label1.Size = new Size(39, 52);
            label1.TabIndex = 4;
            label1.Text = "..";
            // 
            // btnSignals
            // 
            btnSignals.CausesValidation = false;
            btnSignals.Dock = DockStyle.Top;
            btnSignals.FlatAppearance.BorderColor = Color.White;
            btnSignals.FlatStyle = FlatStyle.Flat;
            btnSignals.IconChar = FontAwesome.Sharp.IconChar.TableList;
            btnSignals.IconColor = Color.Black;
            btnSignals.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSignals.IconSize = 32;
            btnSignals.Location = new Point(0, 187);
            btnSignals.Name = "btnSignals";
            btnSignals.Size = new Size(39, 37);
            btnSignals.TabIndex = 3;
            btnSignals.TabStop = false;
            toolTip1.SetToolTip(btnSignals, "Show registed Signals");
            btnSignals.UseVisualStyleBackColor = false;
            btnSignals.Click += btnSignalPool_Click;
            // 
            // btnShowLog
            // 
            btnShowLog.CausesValidation = false;
            btnShowLog.Dock = DockStyle.Top;
            btnShowLog.FlatAppearance.BorderColor = Color.White;
            btnShowLog.FlatStyle = FlatStyle.Flat;
            btnShowLog.IconChar = FontAwesome.Sharp.IconChar.Info;
            btnShowLog.IconColor = Color.Black;
            btnShowLog.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnShowLog.IconSize = 32;
            btnShowLog.Location = new Point(0, 150);
            btnShowLog.Name = "btnShowLog";
            btnShowLog.Size = new Size(39, 37);
            btnShowLog.TabIndex = 2;
            btnShowLog.TabStop = false;
            btnShowLog.UseVisualStyleBackColor = false;
            btnShowLog.Click += btnLog_Click;
            // 
            // btnOpenVS
            // 
            btnOpenVS.CausesValidation = false;
            btnOpenVS.Dock = DockStyle.Top;
            btnOpenVS.FlatAppearance.BorderColor = Color.White;
            btnOpenVS.FlatStyle = FlatStyle.Flat;
            btnOpenVS.IconChar = FontAwesome.Sharp.IconChar.Code;
            btnOpenVS.IconColor = Color.Black;
            btnOpenVS.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnOpenVS.IconSize = 32;
            btnOpenVS.Location = new Point(0, 113);
            btnOpenVS.Name = "btnOpenVS";
            btnOpenVS.Size = new Size(39, 37);
            btnOpenVS.TabIndex = 1;
            btnOpenVS.TabStop = false;
            btnOpenVS.UseVisualStyleBackColor = false;
            btnOpenVS.Click += btnOpenEditor_Click;
            // 
            // btnEditView
            // 
            btnEditView.CausesValidation = false;
            btnEditView.Dock = DockStyle.Top;
            btnEditView.FlatAppearance.BorderColor = Color.White;
            btnEditView.FlatStyle = FlatStyle.Flat;
            btnEditView.IconChar = FontAwesome.Sharp.IconChar.Pencil;
            btnEditView.IconColor = Color.Black;
            btnEditView.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnEditView.IconSize = 32;
            btnEditView.Location = new Point(0, 76);
            btnEditView.Name = "btnEditView";
            btnEditView.Size = new Size(39, 37);
            btnEditView.TabIndex = 0;
            btnEditView.TabStop = false;
            btnEditView.UseVisualStyleBackColor = false;
            btnEditView.Click += btnEditView_Click;
            // 
            // label4
            // 
            label4.Dock = DockStyle.Top;
            label4.Location = new Point(0, 36);
            label4.Name = "label4";
            label4.Size = new Size(39, 40);
            label4.TabIndex = 8;
            // 
            // iconButton1
            // 
            iconButton1.CausesValidation = false;
            iconButton1.Dock = DockStyle.Top;
            iconButton1.FlatAppearance.BorderColor = Color.White;
            iconButton1.FlatStyle = FlatStyle.Flat;
            iconButton1.IconChar = FontAwesome.Sharp.IconChar.EllipsisH;
            iconButton1.IconColor = Color.Black;
            iconButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconButton1.IconSize = 36;
            iconButton1.Location = new Point(0, 0);
            iconButton1.Name = "iconButton1";
            iconButton1.Size = new Size(39, 36);
            iconButton1.TabIndex = 7;
            toolTip1.SetToolTip(iconButton1, "FIle Dialog\r\nOpen / Save / New / Exit");
            iconButton1.UseVisualStyleBackColor = true;
            iconButton1.Click += iconButton1_Click_1;
            // 
            // panel2
            // 
            PanelRoot.SetColumnSpan(panel2, 4);
            panel2.Controls.Add(btnMenuBar);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(3, 641);
            panel2.Name = "panel2";
            panel2.Size = new Size(925, 25);
            panel2.TabIndex = 5;
            // 
            // btnMenuBar
            // 
            btnMenuBar.Dock = DockStyle.Left;
            btnMenuBar.FlatAppearance.BorderColor = Color.White;
            btnMenuBar.FlatStyle = FlatStyle.Flat;
            btnMenuBar.IconChar = FontAwesome.Sharp.IconChar.CaretLeft;
            btnMenuBar.IconColor = Color.Black;
            btnMenuBar.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnMenuBar.IconSize = 26;
            btnMenuBar.Location = new Point(0, 0);
            btnMenuBar.Name = "btnMenuBar";
            btnMenuBar.Size = new Size(19, 25);
            btnMenuBar.TabIndex = 0;
            btnMenuBar.UseVisualStyleBackColor = true;
            btnMenuBar.Click += btnMenuBar_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Black;
            PanelRoot.SetColumnSpan(label3, 2);
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(60, 82);
            label3.Name = "label3";
            label3.Size = new Size(1236, 1);
            label3.TabIndex = 6;
            label3.Text = "label3";
            // 
            // panel5
            // 
            panel5.Controls.Add(btnPageDown);
            panel5.Controls.Add(btnPageUp);
            panel5.Controls.Add(binRemovePage);
            panel5.Controls.Add(btnAddPage);
            panel5.Dock = DockStyle.Fill;
            panel5.Location = new Point(934, 601);
            panel5.Name = "panel5";
            panel5.Size = new Size(362, 34);
            panel5.TabIndex = 7;
            // 
            // btnPageDown
            // 
            btnPageDown.CausesValidation = false;
            btnPageDown.Dock = DockStyle.Left;
            btnPageDown.FlatAppearance.BorderColor = Color.White;
            btnPageDown.FlatStyle = FlatStyle.Flat;
            btnPageDown.IconChar = FontAwesome.Sharp.IconChar.ArrowDown;
            btnPageDown.IconColor = Color.Black;
            btnPageDown.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnPageDown.IconSize = 32;
            btnPageDown.Location = new Point(39, 0);
            btnPageDown.Name = "btnPageDown";
            btnPageDown.Size = new Size(39, 34);
            btnPageDown.TabIndex = 3;
            btnPageDown.UseVisualStyleBackColor = false;
            btnPageDown.Click += btnPageDown_Click;
            // 
            // btnPageUp
            // 
            btnPageUp.CausesValidation = false;
            btnPageUp.Dock = DockStyle.Left;
            btnPageUp.FlatAppearance.BorderColor = Color.White;
            btnPageUp.FlatStyle = FlatStyle.Flat;
            btnPageUp.IconChar = FontAwesome.Sharp.IconChar.ArrowUp;
            btnPageUp.IconColor = Color.Black;
            btnPageUp.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnPageUp.IconSize = 32;
            btnPageUp.Location = new Point(0, 0);
            btnPageUp.Name = "btnPageUp";
            btnPageUp.Size = new Size(39, 34);
            btnPageUp.TabIndex = 3;
            btnPageUp.UseVisualStyleBackColor = false;
            btnPageUp.Click += btnPageUp_Click;
            // 
            // binRemovePage
            // 
            binRemovePage.CausesValidation = false;
            binRemovePage.FlatAppearance.BorderColor = Color.White;
            binRemovePage.FlatStyle = FlatStyle.Flat;
            binRemovePage.IconChar = FontAwesome.Sharp.IconChar.FileCircleMinus;
            binRemovePage.IconColor = Color.Black;
            binRemovePage.IconFont = FontAwesome.Sharp.IconFont.Auto;
            binRemovePage.IconSize = 32;
            binRemovePage.Location = new Point(284, 0);
            binRemovePage.Name = "binRemovePage";
            binRemovePage.Size = new Size(39, 34);
            binRemovePage.TabIndex = 3;
            toolTip1.SetToolTip(binRemovePage, "Remove Page");
            binRemovePage.UseVisualStyleBackColor = false;
            binRemovePage.Click += btnAddProject_Click;
            // 
            // btnAddPage
            // 
            btnAddPage.CausesValidation = false;
            btnAddPage.Dock = DockStyle.Right;
            btnAddPage.FlatAppearance.BorderColor = Color.White;
            btnAddPage.FlatStyle = FlatStyle.Flat;
            btnAddPage.IconChar = FontAwesome.Sharp.IconChar.FileCirclePlus;
            btnAddPage.IconColor = Color.Black;
            btnAddPage.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnAddPage.IconSize = 32;
            btnAddPage.Location = new Point(323, 0);
            btnAddPage.Name = "btnAddPage";
            btnAddPage.Size = new Size(39, 34);
            btnAddPage.TabIndex = 3;
            toolTip1.SetToolTip(btnAddPage, "Add Page");
            btnAddPage.UseVisualStyleBackColor = false;
            btnAddPage.Click += btnAddPage_Click;
            // 
            // toolTip1
            // 
            toolTip1.AutoPopDelay = 5000;
            toolTip1.BackColor = Color.FromArgb(255, 255, 192);
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 100;
            toolTip1.Popup += toolTip1_Popup;
            // 
            // iconToolStripButton1
            // 
            iconToolStripButton1.IconChar = FontAwesome.Sharp.IconChar.None;
            iconToolStripButton1.IconColor = Color.Black;
            iconToolStripButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButton1.Name = "iconToolStripButton1";
            iconToolStripButton1.Size = new Size(23, 23);
            iconToolStripButton1.Text = "iconToolStripButton1";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1299, 669);
            Controls.Add(PanelRoot);
            Name = "FormMain";
            Text = "Form1";
            KeyDown += FormMain_KeyDown;
            PanelRoot.ResumeLayout(false);
            PanelRoot.PerformLayout();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel4.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel5.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button button4;
        private TableLayoutPanel PanelRoot;
        private Panel panel1;
        private Panel panelPages;
        private Button btnLog;
        private Button btnSignalPool;
        private Panel panel3;
        private Panel panel4;
        private FontAwesome.Sharp.IconButton btnSignals;
        private FontAwesome.Sharp.IconButton btnOpenVS;
        private FontAwesome.Sharp.IconButton btnEditView;
        private FontAwesome.Sharp.IconButton btnShowLog;
        private ToolTip toolTip1;
        private FontAwesome.Sharp.IconButton btnAddClass;
        private FontAwesome.Sharp.IconButton btnAddDll;
        private FontAwesome.Sharp.IconButton btnStopProject;
        private FontAwesome.Sharp.IconButton btnRunProject;
        private FontAwesome.Sharp.IconButton btnBuildProject;
        private Label label1;
        private Label label2;
        private Panel panel2;
        private FontAwesome.Sharp.IconButton btnMenuBar;
        private Label label3;
        private PictureBox pictureBox1;
        private FontAwesome.Sharp.IconButton iconButton1;
        private Label label4;
        private Label lblPage;
        private Panel panel5;
        private FontAwesome.Sharp.IconButton binRemovePage;
        private FontAwesome.Sharp.IconButton btnAddPage;
        private FontAwesome.Sharp.IconButton btnPageDown;
        private FontAwesome.Sharp.IconButton btnPageUp;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton1;
    }
}
