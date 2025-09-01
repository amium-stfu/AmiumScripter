namespace AmiumScripter.UI
{
    partial class UserControlPage
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
            tableLayoutPanel1 = new TableLayoutPanel();
            lblPageNumber = new Label();
            btnAdd = new FontAwesome.Sharp.IconButton();
            lblPageText = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.WhiteSmoke;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.Controls.Add(lblPageNumber, 0, 0);
            tableLayoutPanel1.Controls.Add(btnAdd, 2, 0);
            tableLayoutPanel1.Controls.Add(lblPageText, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 43.9393921F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 56.0606079F));
            tableLayoutPanel1.Size = new Size(332, 42);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // lblPageNumber
            // 
            lblPageNumber.AutoSize = true;
            lblPageNumber.BackColor = Color.DarkGray;
            lblPageNumber.Dock = DockStyle.Fill;
            lblPageNumber.Font = new Font("Arial Rounded MT Bold", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPageNumber.ForeColor = Color.DimGray;
            lblPageNumber.Location = new Point(3, 0);
            lblPageNumber.Name = "lblPageNumber";
            tableLayoutPanel1.SetRowSpan(lblPageNumber, 2);
            lblPageNumber.Size = new Size(44, 42);
            lblPageNumber.TabIndex = 0;
            lblPageNumber.Text = "1";
            lblPageNumber.TextAlign = ContentAlignment.MiddleCenter;
            lblPageNumber.Click += lblPageNumber_Click;
            // 
            // btnAdd
            // 
            btnAdd.Dock = DockStyle.Fill;
            btnAdd.FlatAppearance.BorderColor = Color.WhiteSmoke;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.IconChar = FontAwesome.Sharp.IconChar.EllipsisH;
            btnAdd.IconColor = Color.DimGray;
            btnAdd.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnAdd.IconSize = 36;
            btnAdd.Location = new Point(295, 3);
            btnAdd.Name = "btnAdd";
            tableLayoutPanel1.SetRowSpan(btnAdd, 2);
            btnAdd.Size = new Size(34, 36);
            btnAdd.TabIndex = 1;
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // lblPageText
            // 
            lblPageText.AutoSize = true;
            lblPageText.Dock = DockStyle.Fill;
            lblPageText.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPageText.ForeColor = Color.DimGray;
            lblPageText.Location = new Point(53, 0);
            lblPageText.Name = "lblPageText";
            tableLayoutPanel1.SetRowSpan(lblPageText, 2);
            lblPageText.Size = new Size(236, 42);
            lblPageText.TabIndex = 2;
            lblPageText.Text = "label2";
            lblPageText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // UserControlPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "UserControlPage";
            Padding = new Padding(0, 2, 0, 2);
            Size = new Size(332, 46);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label lblPageNumber;
        private FontAwesome.Sharp.IconButton btnAdd;
        private Label lblPageText;
    }
}
