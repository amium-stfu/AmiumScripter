namespace AmiumScripter
{
    partial class Form1
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
            btnStop = new Button();
            btnRun = new Button();
            btnBuild = new Button();
            Book = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            btnEdit = new Button();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            Book.SuspendLayout();
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
            button4.Dock = DockStyle.Right;
            button4.Location = new Point(1011, 0);
            button4.Name = "button4";
            button4.Size = new Size(75, 24);
            button4.TabIndex = 4;
            button4.Text = "OpenCode";
            button4.UseVisualStyleBackColor = true;
            button4.Click += btnOpenEditor_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(Book, 0, 1);
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
            panel1.Controls.Add(btnEdit);
            panel1.Controls.Add(btnStop);
            panel1.Controls.Add(btnRun);
            panel1.Controls.Add(btnBuild);
            panel1.Controls.Add(btnAddProject);
            panel1.Controls.Add(button4);
            panel1.Controls.Add(btnAddPage);
            panel1.Controls.Add(btnAddClass);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1086, 24);
            panel1.TabIndex = 0;
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
            btnBuild.Text = "Build";
            btnBuild.UseVisualStyleBackColor = true;
            btnBuild.Click += btnBuild_Click;
            // 
            // Book
            // 
            Book.Controls.Add(tabPage1);
            Book.Controls.Add(tabPage2);
            Book.Dock = DockStyle.Fill;
            Book.Location = new Point(3, 33);
            Book.Multiline = true;
            Book.Name = "Book";
            Book.SelectedIndex = 0;
            Book.Size = new Size(1086, 550);
            Book.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1078, 522);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1078, 522);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Dock = DockStyle.Right;
            btnEdit.Location = new Point(936, 0);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(75, 24);
            btnEdit.TabIndex = 9;
            btnEdit.Text = "btnEdit";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1092, 606);
            Controls.Add(tableLayoutPanel1);
            Name = "Form1";
            Text = "Form1";
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            Book.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnAddClass;
        private Button btnAddPage;
        private Button btnAddProject;
        private Button button4;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private TabControl Book;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button btnBuild;
        private Button btnRun;
        private Button btnStop;
        private Button btnEdit;
    }
}
