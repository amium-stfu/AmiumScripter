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
            btnAddClass = new Button();
            btnAddPage = new Button();
            btnAddProject = new Button();
            button4 = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            btnSave = new Button();
            btnLoad = new Button();
            btnStop = new Button();
            btnRun = new Button();
            btnBuild = new Button();
            Book = new TabControl();
            panel2 = new Panel();
            btnAddSignal = new Button();
            btnBuildUi = new Button();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // btnAddClass
            // 
            btnAddClass.Dock = DockStyle.Left;
            btnAddClass.Location = new Point(0, 0);
            btnAddClass.Name = "btnAddClass";
            btnAddClass.Size = new Size(75, 24);
            btnAddClass.TabIndex = 1;
            btnAddClass.Text = "AddClass";
            btnAddClass.UseVisualStyleBackColor = true;
            btnAddClass.Click += btnAddClass_Click;
            // 
            // btnAddPage
            // 
            btnAddPage.Dock = DockStyle.Left;
            btnAddPage.Location = new Point(75, 0);
            btnAddPage.Name = "btnAddPage";
            btnAddPage.Size = new Size(75, 24);
            btnAddPage.TabIndex = 2;
            btnAddPage.Text = "AddPage";
            btnAddPage.UseVisualStyleBackColor = true;
            btnAddPage.Click += btnAddPage_Click;
            // 
            // btnAddProject
            // 
            btnAddProject.Dock = DockStyle.Left;
            btnAddProject.Location = new Point(150, 0);
            btnAddProject.Name = "btnAddProject";
            btnAddProject.Size = new Size(75, 24);
            btnAddProject.TabIndex = 0;
            btnAddProject.Text = "AddProject";
            btnAddProject.UseVisualStyleBackColor = true;
            btnAddProject.Click += btnAddProject_Click;
            // 
            // button4
            // 
            button4.Dock = DockStyle.Bottom;
            button4.Location = new Point(0, 526);
            button4.Name = "button4";
            button4.Size = new Size(207, 24);
            button4.TabIndex = 4;
            button4.Text = "OpenCode";
            button4.UseVisualStyleBackColor = true;
            button4.Click += btnOpenEditor_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 213F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(Book, 0, 1);
            tableLayoutPanel1.Controls.Add(panel2, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1092, 606);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnBuildUi);
            panel1.Controls.Add(btnSave);
            panel1.Controls.Add(btnLoad);
            panel1.Controls.Add(btnStop);
            panel1.Controls.Add(btnRun);
            panel1.Controls.Add(btnBuild);
            panel1.Controls.Add(btnAddProject);
            panel1.Controls.Add(btnAddPage);
            panel1.Controls.Add(btnAddClass);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(873, 24);
            panel1.TabIndex = 0;
            // 
            // btnSave
            // 
            btnSave.Dock = DockStyle.Left;
            btnSave.Location = new Point(525, 0);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 24);
            btnSave.TabIndex = 11;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnLoad
            // 
            btnLoad.Dock = DockStyle.Left;
            btnLoad.Location = new Point(450, 0);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(75, 24);
            btnLoad.TabIndex = 10;
            btnLoad.Text = "Load";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnStop
            // 
            btnStop.Dock = DockStyle.Left;
            btnStop.Location = new Point(375, 0);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(75, 24);
            btnStop.TabIndex = 8;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnRun
            // 
            btnRun.Dock = DockStyle.Left;
            btnRun.Location = new Point(300, 0);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(75, 24);
            btnRun.TabIndex = 7;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // btnBuild
            // 
            btnBuild.Dock = DockStyle.Left;
            btnBuild.Location = new Point(225, 0);
            btnBuild.Name = "btnBuild";
            btnBuild.Size = new Size(75, 24);
            btnBuild.TabIndex = 6;
            btnBuild.Text = "Build Code";
            btnBuild.UseVisualStyleBackColor = true;
            btnBuild.Click += btnBuild_Click;
            // 
            // Book
            // 
            Book.Dock = DockStyle.Fill;
            Book.Location = new Point(3, 33);
            Book.Multiline = true;
            Book.Name = "Book";
            Book.SelectedIndex = 0;
            Book.Size = new Size(873, 550);
            Book.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnAddSignal);
            panel2.Controls.Add(button4);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(882, 33);
            panel2.Name = "panel2";
            panel2.Size = new Size(207, 550);
            panel2.TabIndex = 2;
            // 
            // btnAddSignal
            // 
            btnAddSignal.Dock = DockStyle.Top;
            btnAddSignal.Location = new Point(0, 0);
            btnAddSignal.Name = "btnAddSignal";
            btnAddSignal.Size = new Size(207, 23);
            btnAddSignal.TabIndex = 5;
            btnAddSignal.Text = "Add Signal";
            btnAddSignal.UseVisualStyleBackColor = true;
            btnAddSignal.Click += btnAddSignal_Click;
            // 
            // btnBuildUi
            // 
            btnBuildUi.Dock = DockStyle.Left;
            btnBuildUi.Location = new Point(600, 0);
            btnBuildUi.Name = "btnBuildUi";
            btnBuildUi.Size = new Size(75, 24);
            btnBuildUi.TabIndex = 12;
            btnBuildUi.Text = "Build UI";
            btnBuildUi.UseVisualStyleBackColor = true;
            btnBuildUi.Click += btnBuildUi_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1092, 606);
            Controls.Add(tableLayoutPanel1);
            Name = "FormMain";
            Text = "Form1";
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnAddClass;
        private Button btnAddPage;
        private Button btnAddProject;
        private Button button4;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Button btnBuild;
        private Button btnRun;
        private Button btnStop;
        private Button btnLoad;
        private Button btnSave;
        private Panel panel2;
        private Button btnAddSignal;
        public TabControl Book;
        private Button btnBuildUi;
    }
}
