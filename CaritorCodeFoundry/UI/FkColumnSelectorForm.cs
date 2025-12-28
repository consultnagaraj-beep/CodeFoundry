using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeFoundry.Generator.Models;

namespace CodeFoundry.Generator.UI
{
    public class FkColumnSelectorForm : Form
    {
        private CheckedListBox lst;
        private Button btnOk;
        private Button btnCancel;

        public List<string> SelectedColumns { get; private set; } =
            new List<string>();

        public FkColumnSelectorForm(
            TableSchemaDto fkSchema,
            string referencedTable,
            string existingSelectionCsv)
        {
            Text = $"Select columns from {referencedTable}";
            Width = 400;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;

            lst = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true   // 🔑 Mouse click fix
            };

            btnOk = new Button
            {
                Text = "OK",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnOk.Click += (_, __) =>
            {
                SelectedColumns = lst.CheckedItems
                    .Cast<string>()
                    .ToList();
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnCancel.Click += (_, __) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(lst);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            var preSelected = (existingSelectionCsv ?? "")
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var c in fkSchema.Columns.Where(x => !x.IsPrimaryKey))
            {
                lst.Items.Add(c.ColumnName, preSelected.Contains(c.ColumnName));
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FkColumnSelectorForm
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "FkColumnSelectorForm";
            this.Load += new System.EventHandler(this.FkColumnSelectorForm_Load);
            this.ResumeLayout(false);

        }

        private void FkColumnSelectorForm_Load(object sender, EventArgs e)
        {

        }
    }
}
