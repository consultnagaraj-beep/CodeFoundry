using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeFoundry.Generator.UI
{
    public class FkEditorForm : Form
    {
        private TextBox txtMappings;
        private Button btnOk;
        private Button btnCancel;

        private Dictionary<string, List<string>> _fkSelections;

        public FkEditorForm(Dictionary<string, List<string>> existing)
        {
            _fkSelections = existing != null
                ? new Dictionary<string, List<string>>(existing, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            InitializeComponents();
            LoadExisting();
        }

        private void InitializeComponents()
        {
            this.Text = "FK Selections Editor";
            this.Width = 640;
            this.Height = 420;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            txtMappings = new TextBox { Left = 12, Top = 12, Width = 600, Height = 320, Multiline = true, ScrollBars = ScrollBars.Vertical };
            btnOk = new Button { Text = "OK", Left = 452, Width = 80, Top = 344, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Left = 540, Width = 72, Top = 344, DialogResult = DialogResult.Cancel };

            btnOk.Click += BtnOk_Click;

            this.Controls.AddRange(new Control[] { txtMappings, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadExisting()
        {
            var sb = new StringBuilder();
            foreach (var kv in _fkSelections)
            {
                var vals = string.Join(",", kv.Value ?? new List<string>());
                sb.AppendLine($"{kv.Key} -> {vals}");
            }
            txtMappings.Text = sb.ToString();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var lines = txtMappings.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var ln in lines)
            {
                var parts = ln.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
                var key = parts[0].Trim();
                var vals = new List<string>();
                if (parts.Length > 1)
                {
                    vals = parts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => x.Trim())
                                   .Where(x => !string.IsNullOrEmpty(x)).ToList();
                }
                if (!string.IsNullOrEmpty(key)) dict[key] = vals;
            }
            _fkSelections = dict;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public Dictionary<string, List<string>> GetFkSelections()
        {
            return _fkSelections;
        }
    }
}
