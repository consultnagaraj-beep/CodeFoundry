using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.UI
{
    public class ConfigureFieldsForm : Form
    {
        private Label lblTitle;
        private ComboBox cmbGridType;
        private CheckedListBox clbColumns;
        private Button btnFkEditor;
        private Button btnMarkHidden;
        private Button btnMarkUnhidable;
        private Button btnOk;
        private Button btnCancel;

        private TableSchemaDto _schema;
        private SelectionDto _selection;
        private string _currentGridType = "EditGrid";

        private readonly string[] DefaultGridTypes =
        {
            "EditGrid",
            "InfoGrid",
            "DropDownGrid"
        };

        public ConfigureFieldsForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Configure Fields";
            Width = 720;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            lblTitle = new Label
            {
                Left = 12,
                Top = 9,
                Width = 680,
                Text = "Configure columns",
                AutoSize = false
            };

            cmbGridType = new ComboBox
            {
                Left = 12,
                Top = 36,
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbGridType.SelectedIndexChanged += (_, __) =>
            {
                _currentGridType = cmbGridType.SelectedItem?.ToString();
                LoadColumnsForGrid(_currentGridType);
            };

            clbColumns = new CheckedListBox
            {
                Left = 12,
                Top = 72,
                Width = 680,
                Height = 360,
                CheckOnClick = true
            };

            btnFkEditor = new Button { Text = "Edit FK Selections", Left = 12, Width = 140, Top = 440 };
            btnFkEditor.Click += BtnFkEditor_Click;

            btnMarkHidden = new Button { Text = "Mark Selected Hidden", Left = 168, Width = 160, Top = 440 };
            btnMarkHidden.Click += (_, __) => ToggleMarkSelected(hidden: true, unhidable: false);

            btnMarkUnhidable = new Button { Text = "Mark Selected Unhidable", Left = 336, Width = 180, Top = 440 };
            btnMarkUnhidable.Click += (_, __) => ToggleMarkSelected(hidden: true, unhidable: true);

            btnOk = new Button { Text = "OK", Left = 520, Width = 80, Top = 480 };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button { Text = "Cancel", Left = 608, Width = 80, Top = 480 };
            btnCancel.Click += (_, __) => Close();

            Controls.AddRange(new Control[]
            {
                lblTitle, cmbGridType, clbColumns,
                btnFkEditor, btnMarkHidden, btnMarkUnhidable,
                btnOk, btnCancel
            });
        }

        public void Initialize(TableSchemaDto schema, SelectionDto existingSelection = null)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _selection = existingSelection ?? new SelectionDto();

            lblTitle.Text = $"Configure columns for table: {_schema.TableName}";

            cmbGridType.Items.Clear();
            foreach (var g in DefaultGridTypes)
                cmbGridType.Items.Add(g);

            foreach (var g in _selection.GridTypes)
                if (!cmbGridType.Items.Contains(g))
                    cmbGridType.Items.Add(g);

            cmbGridType.SelectedItem = cmbGridType.Items.Contains("EditGrid")
                ? "EditGrid"
                : cmbGridType.Items[0];

            _currentGridType = cmbGridType.SelectedItem.ToString();
            LoadColumnsForGrid(_currentGridType);
        }

        private void LoadColumnsForGrid(string gridType)
        {
            clbColumns.Items.Clear();

            var selected = _selection.GetSelectedColumns(gridType);
            var hidden = _selection.GetHiddenColumns(gridType);
            var unhidable = _selection.GetUnhidableColumns(gridType);

            foreach (var col in _schema.Columns)
            {
                var tags = new List<string>();
                if (hidden.Contains(col.ColumnName)) tags.Add("hidden");
                if (unhidable.Contains(col.ColumnName)) tags.Add("unhidable");

                var suffix = tags.Count > 0 ? $" — {string.Join(", ", tags)}" : "";
                var text = $"{col.ColumnName} [{col.DataType}]{suffix}";

                clbColumns.Items.Add(text, selected.Contains(col.ColumnName));
            }
        }

        private void ToggleMarkSelected(bool hidden, bool unhidable)
        {
            foreach (int i in clbColumns.CheckedIndices)
            {
                var col = ExtractColumnName(clbColumns.Items[i].ToString());

                if (hidden)
                    AddTo(_selection.PerGridHidden, _currentGridType, col);

                if (unhidable)
                    AddTo(_selection.PerGridUnhidable, _currentGridType, col);
            }

            LoadColumnsForGrid(_currentGridType);
        }

        private void BtnFkEditor_Click(object sender, EventArgs e)
        {
            var map = GetOrCreateMap(_selection.PerGridFkSelections, _currentGridType);

            using (var dlg = new FkEditorForm(map))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _selection.PerGridFkSelections[_currentGridType] = dlg.GetFkSelections();
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var selected = new List<string>();

            for (int i = 0; i < clbColumns.Items.Count; i++)
                if (clbColumns.GetItemChecked(i))
                    selected.Add(ExtractColumnName(clbColumns.Items[i].ToString()));

            _selection.PerGridColumns[_currentGridType] = selected;

            // ✅ SAFE: mutate only, never assign
            if (!_selection.GridTypes.Contains(_currentGridType, StringComparer.OrdinalIgnoreCase))
                _selection.GridTypes.Add(_currentGridType);

            DialogResult = DialogResult.OK;
            Close();
        }

        public SelectionDto GetSelection() => _selection;

        // ---------- helpers ----------

        private static void AddTo(Dictionary<string, List<string>> map, string key, string value)
        {
            if (!map.TryGetValue(key, out var list))
                map[key] = list = new List<string>();

            if (!list.Contains(value))
                list.Add(value);
        }

        private static Dictionary<string, List<string>> GetOrCreateMap(
            Dictionary<string, Dictionary<string, List<string>>> dict,
            string key)
        {
            if (!dict.TryGetValue(key, out var map))
                dict[key] = map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            return map;
        }

        private static string ExtractColumnName(string text)
        {
            var idx = text.IndexOf('[');
            return idx > 0 ? text.Substring(0, idx).Trim() : text;
        }
    }
}
