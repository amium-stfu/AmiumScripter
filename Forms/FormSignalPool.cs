using AmiumScripter.Core;
using AmiumScripter.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmiumScripter.Forms
{
    public partial class FormSignalPool : Form
    {
        private System.Windows.Forms.Timer _timer;

        private DataTable _table;

        public FormSignalPool()
        {
            InitializeComponent();

            _table = SignalPool.SignalsToDataTable();
            dataGridView1.DataSource = _table;

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 250; // alle 250ms aktualisieren
            _timer.Tick += (s, e) => UpdateTableLive();
            _timer.Start();
        }

        private void UpdateTableLive()
        {
            // Aktuelle Scrollposition und ausgewählte Zeile merken
            int firstDisplayedRow = dataGridView1.FirstDisplayedScrollingRowIndex;
            int selectedRowIndex = dataGridView1.CurrentRow?.Index ?? -1;

            // ... Update-Logik wie gehabt (siehe vorige Antwort) ...
            // Beispiel:
            var snapshot = SignalPool.Snapshot();

            foreach (DataRow row in _table.Rows)
            {
                string name = (string)row["Name"];
                if (snapshot.TryGetValue(name, out var obj) && obj is BaseSignalCommon signal)
                {
                    row["Text"] = signal.GetProperty("Text", "");
                    row["Value"] = signal.ValueAsObject?.ToString() ?? "";
                }
                else
                {
                    row.Delete();
                }
            }
            _table.AcceptChanges();

            foreach (var kvp in snapshot)
            {
                string name = kvp.Key;
                var found = _table.Rows.Find(name);
                if (found == null && kvp.Value is BaseSignalCommon signal)
                {
                    var newRow = _table.NewRow();
                    newRow["Name"] = signal.Name;
                    newRow["Text"] = signal.GetProperty("Text", "");
                    newRow["Value"] = signal.ValueAsObject?.ToString() ?? "";
                    _table.Rows.Add(newRow);
                }
            }

            // --- Nach dem Update: Scrollposition wiederherstellen ---
            if (firstDisplayedRow >= 0 && firstDisplayedRow < dataGridView1.RowCount)
                dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedRow;

            // Optional: Selektion zurücksetzen
            if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView1.RowCount)
                dataGridView1.CurrentCell = dataGridView1.Rows[selectedRowIndex].Cells[0];
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _timer?.Stop();
            _timer?.Dispose();
        }
    }

}
