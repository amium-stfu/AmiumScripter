
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace AmiumScripter.UI
{
    partial class Keyboard
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
            panelQuertz = new FlowLayoutPanel();
            tbResult = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            Numblock = new FlowLayoutPanel();
            btnClear = new Button();
            btnRemove = new Button();
            panel1 = new Panel();
            btnPaste = new Button();
            btnCopy = new Button();
            btnAbort = new Button();
            btnCheck = new Button();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panelQuertz
            // 
            panelQuertz.Dock = DockStyle.Fill;
            panelQuertz.Location = new Point(0, 42);
            panelQuertz.Margin = new Padding(0);
            panelQuertz.Name = "panelQuertz";
            panelQuertz.Size = new Size(410, 165);
            panelQuertz.TabIndex = 0;
            // 
            // tbResult
            // 
            tbResult.BackColor = Color.LightGray;
            tbResult.BorderStyle = BorderStyle.None;
            tableLayoutPanel1.SetColumnSpan(tbResult, 2);
            tbResult.Dock = DockStyle.Fill;
            tbResult.Font = new Font("Calibri", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tbResult.Location = new Point(4, 3);
            tbResult.Margin = new Padding(4, 3, 4, 3);
            tbResult.Name = "tbResult";
            tbResult.Size = new Size(465, 36);
            tbResult.TabIndex = 16;
            tbResult.KeyDown += tbResult_KeyDown;
            tbResult.KeyPress += tbResult_KeyPress;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 410F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 63F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8F));
            tableLayoutPanel1.Controls.Add(tbResult, 0, 0);
            tableLayoutPanel1.Controls.Add(panelQuertz, 0, 1);
            tableLayoutPanel1.Controls.Add(Numblock, 1, 1);
            tableLayoutPanel1.Controls.Add(btnClear, 3, 0);
            tableLayoutPanel1.Controls.Add(btnRemove, 2, 0);
            tableLayoutPanel1.Controls.Add(panel1, 3, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 165F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(606, 214);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // Numblock
            // 
            tableLayoutPanel1.SetColumnSpan(Numblock, 2);
            Numblock.Dock = DockStyle.Fill;
            Numblock.Location = new Point(414, 45);
            Numblock.Margin = new Padding(4, 3, 4, 3);
            Numblock.Name = "Numblock";
            Numblock.Size = new Size(120, 159);
            Numblock.TabIndex = 18;
            // 
            // btnClear
            // 
            btnClear.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClear.Location = new Point(542, 3);
            btnClear.Margin = new Padding(4, 3, 4, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(54, 36);
            btnClear.TabIndex = 19;
            btnClear.Text = "DEL";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnRemove
            // 
            btnRemove.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRemove.Location = new Point(477, 3);
            btnRemove.Margin = new Padding(4, 3, 4, 3);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(54, 36);
            btnRemove.TabIndex = 19;
            btnRemove.Text = "<";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnPaste);
            panel1.Controls.Add(btnCopy);
            panel1.Controls.Add(btnAbort);
            panel1.Controls.Add(btnCheck);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(541, 45);
            panel1.Name = "panel1";
            panel1.Size = new Size(62, 159);
            panel1.TabIndex = 20;
            // 
            // btnPaste
            // 
            btnPaste.Dock = DockStyle.Top;
            btnPaste.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPaste.Location = new Point(0, 39);
            btnPaste.Margin = new Padding(4, 3, 4, 3);
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new Size(62, 39);
            btnPaste.TabIndex = 19;
            btnPaste.Text = "Paste";
            btnPaste.UseVisualStyleBackColor = true;
            btnPaste.Click += btnPaste_Click;
            // 
            // btnCopy
            // 
            btnCopy.Dock = DockStyle.Top;
            btnCopy.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCopy.Location = new Point(0, 0);
            btnCopy.Margin = new Padding(4, 3, 4, 3);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(62, 39);
            btnCopy.TabIndex = 19;
            btnCopy.Text = "Copy";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            // 
            // btnAbort
            // 
            btnAbort.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnAbort.Location = new Point(0, 81);
            btnAbort.Margin = new Padding(4, 3, 4, 3);
            btnAbort.Name = "btnAbort";
            btnAbort.Size = new Size(56, 39);
            btnAbort.TabIndex = 19;
            btnAbort.UseVisualStyleBackColor = true;
            btnAbort.Click += btnAbort_Click;
            // 
            // btnCheck
            // 
            btnCheck.Dock = DockStyle.Bottom;
            btnCheck.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnCheck.Location = new Point(0, 120);
            btnCheck.Margin = new Padding(4, 3, 4, 3);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(62, 39);
            btnCheck.TabIndex = 19;
            btnCheck.UseVisualStyleBackColor = true;
            btnCheck.Click += btnCheck_Click;
            // 
            // Keyboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(606, 214);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 3, 4, 3);
            Name = "Keyboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Keyboard";
            FormClosing += Keyboard_FormClosing;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.FlowLayoutPanel panelQuertz;
        private System.Windows.Forms.TextBox tbResult;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel Numblock;
 
        private Button btnCheck;
        private Button btnAbort;
        private Button btnClear;
        private Button btnRemove;
        private Panel panel1;
        private Button btnPaste;
        private Button btnCopy;
    }
}