using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AmiumScripter.Forms
{
    public partial class FormLog : Form
    {
        bool all = false;
        bool debug = true;
        bool info = true;
        bool warning = true;
        bool fatal = true;

        public FormLog()
        {
            InitializeComponent();
            dataGridView1.DataSource = Logger.winFormsSink.Entries;
            dataGridView1.Columns["Time"].Width = 150;
            dataGridView1.Columns["Time"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss.fff";
            dataGridView1.Columns["Level"].Width = 50;
            dataGridView1.Columns["Message"].Width = 1900; // oder was du brauchst
            Logger.winFormsSink.Entries.ListChanged += (s, e) => ApplyFilter();
            UpdateButtons();
            ApplyFilter();
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            debug = !debug;
            UpdateButtons();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            info = !info;
            UpdateButtons();
        }

        private void btnWarning_Click(object sender, EventArgs e)
        {
            warning = !warning;
            UpdateButtons();
        }

        private void btnFatal_Click(object sender, EventArgs e)
        {
            fatal = !fatal;
            UpdateButtons();
        }

        void UpdateButtons()
        {
            btnDebug.BackColor = debug ? Color.LightGreen : Color.LightGray;
            btnInfo.BackColor = info ? Color.LightGreen : Color.LightGray;
            btnWarning.BackColor = warning ? Color.LightGreen : Color.LightGray;
            btnFatal.BackColor = fatal ? Color.LightGreen : Color.LightGray;
            if (all)
            {
                btnDebug.Enabled = false;
                btnInfo.Enabled = false;
                btnWarning.Enabled = false;
                btnFatal.Enabled = false;
            }
            else
            {
                btnDebug.Enabled = true;
                btnInfo.Enabled = true;
                btnWarning.Enabled = true;
                btnFatal.Enabled = true;
            }
            ApplyFilter();
        }

        void ApplyFilter()
        {
            // Läuft im UI-Thread!
            var snap = Logger.winFormsSink.Entries.ToList(); // snapshot

            if (all)
            {
                // Nur einmal binden, nicht bei jedem Tick neu setzen
                if (!ReferenceEquals(dataGridView1.DataSource, Logger.winFormsSink.Entries))
                    dataGridView1.DataSource = Logger.winFormsSink.Entries;
                return;
            }

            var filtered = snap.Where(x =>
                (debug && x.Level.Equals("Debug", StringComparison.OrdinalIgnoreCase)) ||
                (info && x.Level.Equals("Information", StringComparison.OrdinalIgnoreCase)) ||
                (warning && x.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase)) ||
                (fatal && (x.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase) || x.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            SetDataSourceSafe(filtered); // dein Invoke-Safe Setter
        }


        private void SetDataSourceSafe(IList<LogEntry> filtered)
        {
            void SetIt()
            {
                if (dataGridView1.IsDisposed) return;
                dataGridView1.SuspendLayout();

                // (optional) Scroll-Pos merken
                int first = -1;
                try { first = dataGridView1.FirstDisplayedScrollingRowIndex; } catch { }

                dataGridView1.DataSource = new BindingList<LogEntry>(filtered);

                // (optional) Scroll-Pos zurück
                if (first >= 0 && first < dataGridView1.RowCount)
                    dataGridView1.FirstDisplayedScrollingRowIndex = first;

                dataGridView1.ResumeLayout();
            }

            if (!dataGridView1.IsHandleCreated || dataGridView1.Disposing || dataGridView1.IsDisposed) return;

            if (dataGridView1.InvokeRequired)
                dataGridView1.BeginInvoke((Action)SetIt);
            else
                SetIt();
        }


        private void FormLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void FormLog_Shown(object sender, EventArgs e)
        {
            UpdateButtons();
            ApplyFilter();
        }
    }
}
