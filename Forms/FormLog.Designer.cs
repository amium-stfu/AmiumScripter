namespace AmiumScripter.Forms
{
    partial class FormLog
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
            tableLayoutPanel1 = new TableLayoutPanel();
            dataGridView1 = new DataGridView();
            panel1 = new Panel();
            btnDebug = new Button();
            btnInfo = new Button();
            btnWarning = new Button();
            btnFatal = new Button();
            btnClipboard = new Button();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(dataGridView1, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1137, 640);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // dataGridView1
            // 
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 33);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(1131, 584);
            dataGridView1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnDebug);
            panel1.Controls.Add(btnInfo);
            panel1.Controls.Add(btnWarning);
            panel1.Controls.Add(btnFatal);
            panel1.Controls.Add(btnClipboard);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1131, 24);
            panel1.TabIndex = 1;
            // 
            // btnDebug
            // 
            btnDebug.Dock = DockStyle.Right;
            btnDebug.Location = new Point(756, 0);
            btnDebug.Name = "btnDebug";
            btnDebug.Size = new Size(75, 24);
            btnDebug.TabIndex = 1;
            btnDebug.Text = "Debug";
            btnDebug.UseVisualStyleBackColor = true;
            btnDebug.Click += btnDebug_Click;
            // 
            // btnInfo
            // 
            btnInfo.Dock = DockStyle.Right;
            btnInfo.Location = new Point(831, 0);
            btnInfo.Name = "btnInfo";
            btnInfo.Size = new Size(75, 24);
            btnInfo.TabIndex = 1;
            btnInfo.Text = "Info";
            btnInfo.UseVisualStyleBackColor = true;
            btnInfo.Click += btnInfo_Click;
            // 
            // btnWarning
            // 
            btnWarning.Dock = DockStyle.Right;
            btnWarning.Location = new Point(906, 0);
            btnWarning.Name = "btnWarning";
            btnWarning.Size = new Size(75, 24);
            btnWarning.TabIndex = 1;
            btnWarning.Text = "Warning";
            btnWarning.UseVisualStyleBackColor = true;
            btnWarning.Click += btnWarning_Click;
            // 
            // btnFatal
            // 
            btnFatal.Dock = DockStyle.Right;
            btnFatal.Location = new Point(981, 0);
            btnFatal.Name = "btnFatal";
            btnFatal.Size = new Size(75, 24);
            btnFatal.TabIndex = 1;
            btnFatal.Text = "Fatal";
            btnFatal.UseVisualStyleBackColor = true;
            btnFatal.Click += btnFatal_Click;
            // 
            // btnClipboard
            // 
            btnClipboard.Dock = DockStyle.Right;
            btnClipboard.Location = new Point(1056, 0);
            btnClipboard.Name = "btnClipboard";
            btnClipboard.Size = new Size(75, 24);
            btnClipboard.TabIndex = 0;
            btnClipboard.Text = "Clip";
            btnClipboard.UseVisualStyleBackColor = true;
            // 
            // FormLog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1137, 640);
            Controls.Add(tableLayoutPanel1);
            Name = "FormLog";
            Text = "FormLog";
            FormClosing += FormLog_FormClosing;
            Shown += FormLog_Shown;
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView dataGridView1;
        private Panel panel1;
        private Button btnClipboard;
        private Button btnDebug;
        private Button btnInfo;
        private Button btnWarning;
        private Button btnFatal;
    }
}