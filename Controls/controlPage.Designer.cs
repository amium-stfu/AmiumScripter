namespace AmiumScripter.Controls
{
    partial class controlPage
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
            PageContent = new GroupBox();
            SuspendLayout();
            // 
            // PageContent
            // 
            PageContent.Dock = DockStyle.Fill;
            PageContent.Location = new Point(0, 0);
            PageContent.Name = "PageContent";
            PageContent.Size = new Size(618, 483);
            PageContent.TabIndex = 0;
            PageContent.TabStop = false;
            PageContent.Text = "groupBox1";
            // 
            // controlPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(PageContent);
            Name = "controlPage";
            Size = new Size(618, 483);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox PageContent;
    }
}
