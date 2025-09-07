namespace AmiumScripter.Forms
{
    partial class CodeEditorForm :Form
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
            Editor = new ScintillaNET.Scintilla();
            tableLayoutPanel1 = new TableLayoutPanel();
            treeProject = new TreeView();
            panelTop = new FlowLayoutPanel();
            btnRefresh = new Button();
            btnSave = new Button();
            tableLayoutPanel1.SuspendLayout();
            panelTop.SuspendLayout();
            SuspendLayout();
            // 
            // Editor
            // 
            Editor.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            Editor.Dock = DockStyle.Fill;
            Editor.LexerName = null;
            Editor.Location = new Point(203, 42);
            Editor.Name = "Editor";
            Editor.ScrollWidth = 49;
            Editor.Size = new Size(595, 396);
            Editor.TabIndex = 0;
            Editor.Text = "scintilla1";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(treeProject, 0, 1);
            tableLayoutPanel1.Controls.Add(Editor, 1, 1);
            tableLayoutPanel1.Controls.Add(panelTop, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(801, 438);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // treeProject
            // 
            treeProject.Dock = DockStyle.Fill;
            treeProject.Location = new Point(3, 43);
            treeProject.Name = "treeProject";
            treeProject.Size = new Size(194, 392);
            treeProject.TabIndex = 2;
            // 
            // panelTop
            // 
            panelTop.AutoSize = true;
            panelTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(panelTop, 2);
            panelTop.Controls.Add(btnRefresh);
            panelTop.Controls.Add(btnSave);
            panelTop.Dock = DockStyle.Fill;
            panelTop.Location = new Point(3, 3);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(795, 34);
            panelTop.TabIndex = 3;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(3, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "Reload";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(84, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // CodeEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(801, 438);
            Controls.Add(tableLayoutPanel1);
            Name = "CodeEditorForm";
            Text = "Code Editor";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panelTop.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ScintillaNET.Scintilla Editor;
        private TableLayoutPanel tableLayoutPanel1;
        private TreeView treeProject;
        private FlowLayoutPanel panelTop;
        private Button btnRefresh;
        private Button btnSave;
    }
}