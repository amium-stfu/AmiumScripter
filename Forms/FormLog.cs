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
            var entries = Logger.winFormsSink.Entries;

            // Wenn "all" aktiviert ist, zeige alle Einträge
            if (all)
            {
                dataGridView1.DataSource = entries;
                return;
            }

            // Ansonsten filtern wir explizit nach den Flags
            var filtered = entries.Where(x =>
                (debug && x.Level.Equals("Debug", StringComparison.OrdinalIgnoreCase)) ||
                (info && x.Level.Equals("Information", StringComparison.OrdinalIgnoreCase)) ||
                (warning && x.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase)) ||
                (fatal && (x.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase) || x.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            dataGridView1.DataSource = new BindingList<LogEntry>(filtered);
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
