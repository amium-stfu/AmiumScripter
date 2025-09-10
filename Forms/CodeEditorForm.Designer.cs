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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeEditorForm));
            tableLayoutPanel1 = new TableLayoutPanel();
            treeProject = new TreeView();
            Editor = new ScintillaNET.Scintilla();
            panelTop = new FlowLayoutPanel();
            btnRefresh = new Button();
            btnRebuild = new FontAwesome.Sharp.IconButton();
            btnRun = new FontAwesome.Sharp.IconButton();
            btnStop = new FontAwesome.Sharp.IconButton();
            btnSave = new FontAwesome.Sharp.IconButton();
            imageList1 = new ImageList(components);
            tableLayoutPanel1.SuspendLayout();
            panelTop.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 322F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(treeProject, 0, 1);
            tableLayoutPanel1.Controls.Add(Editor, 1, 1);
            tableLayoutPanel1.Controls.Add(panelTop, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 49F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 144F));
            tableLayoutPanel1.Size = new Size(987, 559);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // treeProject
            // 
            treeProject.Dock = DockStyle.Fill;
            treeProject.Location = new Point(3, 52);
            treeProject.Name = "treeProject";
            treeProject.Size = new Size(316, 360);
            treeProject.TabIndex = 2;
            // 
            // Editor
            // 
            Editor.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            Editor.Dock = DockStyle.Fill;
            Editor.LexerName = null;
            Editor.Location = new Point(325, 52);
            Editor.Name = "Editor";
            Editor.ScrollWidth = 49;
            Editor.Size = new Size(659, 360);
            Editor.TabIndex = 0;
            Editor.Text = "Editor";
            // 
            // panelTop
            // 
            panelTop.AutoSize = true;
            panelTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelTop.BackColor = Color.White;
            tableLayoutPanel1.SetColumnSpan(panelTop, 2);
            panelTop.Controls.Add(btnRefresh);
            panelTop.Controls.Add(btnRebuild);
            panelTop.Controls.Add(btnRun);
            panelTop.Controls.Add(btnStop);
            panelTop.Controls.Add(btnSave);
            panelTop.Dock = DockStyle.Fill;
            panelTop.Location = new Point(3, 3);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(981, 43);
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
            btnRefresh.Visible = false;
            // 
            // btnRebuild
            // 
            btnRebuild.Dock = DockStyle.Left;
            btnRebuild.FlatAppearance.BorderColor = Color.White;
            btnRebuild.FlatStyle = FlatStyle.Flat;
            btnRebuild.IconChar = FontAwesome.Sharp.IconChar.Cog;
            btnRebuild.IconColor = Color.Black;
            btnRebuild.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnRebuild.IconSize = 30;
            btnRebuild.ImageAlign = ContentAlignment.MiddleLeft;
            btnRebuild.Location = new Point(84, 3);
            btnRebuild.Name = "btnRebuild";
            btnRebuild.Size = new Size(88, 38);
            btnRebuild.TabIndex = 2;
            btnRebuild.Text = "Rebuild";
            btnRebuild.TextAlign = ContentAlignment.MiddleRight;
            btnRebuild.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            btnRun.AutoSize = true;
            btnRun.Dock = DockStyle.Left;
            btnRun.FlatAppearance.BorderColor = Color.White;
            btnRun.FlatStyle = FlatStyle.Flat;
            btnRun.IconChar = FontAwesome.Sharp.IconChar.PlayCircle;
            btnRun.IconColor = Color.Black;
            btnRun.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnRun.IconSize = 30;
            btnRun.ImageAlign = ContentAlignment.MiddleLeft;
            btnRun.Location = new Point(178, 3);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(88, 38);
            btnRun.TabIndex = 2;
            btnRun.Text = "Run";
            btnRun.TextAlign = ContentAlignment.MiddleRight;
            btnRun.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            btnStop.AutoSize = true;
            btnStop.Dock = DockStyle.Left;
            btnStop.FlatAppearance.BorderColor = Color.White;
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.IconChar = FontAwesome.Sharp.IconChar.CircleStop;
            btnStop.IconColor = Color.Black;
            btnStop.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnStop.IconSize = 30;
            btnStop.ImageAlign = ContentAlignment.MiddleLeft;
            btnStop.Location = new Point(272, 3);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(88, 38);
            btnStop.TabIndex = 2;
            btnStop.Text = "Stop";
            btnStop.TextAlign = ContentAlignment.MiddleRight;
            btnStop.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.AutoSize = true;
            btnSave.Dock = DockStyle.Left;
            btnSave.FlatAppearance.BorderColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.IconChar = FontAwesome.Sharp.IconChar.Save;
            btnSave.IconColor = Color.Black;
            btnSave.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSave.IconSize = 30;
            btnSave.ImageAlign = ContentAlignment.MiddleLeft;
            btnSave.Location = new Point(366, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 38);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.TextAlign = ContentAlignment.MiddleRight;
            btnSave.Click += btnSave_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "folderorange.png");
            imageList1.Images.SetKeyName(1, "c#.png");
            imageList1.Images.SetKeyName(2, "foldergrey.png");
            imageList1.Images.SetKeyName(3, "sharp_c_icon_211998.png");
            imageList1.Images.SetKeyName(4, "");
            // 
            // CodeEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(987, 559);
            Controls.Add(tableLayoutPanel1);
            Name = "CodeEditorForm";
            Text = "Code Editor";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel panelTop;
        private Button btnRefresh;
    
        private FontAwesome.Sharp.IconButton btnRebuild;
        private FontAwesome.Sharp.IconButton btnRun;
        private FontAwesome.Sharp.IconButton btnStop;
        private FontAwesome.Sharp.IconButton btnSave;
        private TreeView treeProject;
        private ScintillaNET.Scintilla Editor;
        private ImageList imageList1;
    }
}