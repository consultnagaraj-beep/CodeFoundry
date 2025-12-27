using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.UI
{
    public class ConfigureFieldsFormAdvanced : Form
    {
        private ComboBox cmbGridType;
        private CheckBox chkShowSelectedOnly;
        private Button btnSelectAll;
        private Button btnSort;
        private Button btnValidation; // 🔴 NEW
        private DataGridView dgv;
        private Button btnApply;
        private Button btnCancel;

        private TableSchemaDto _schema;
        private SelectionDto _selection;
        private string _gridType = "EditGrid";
        private string _connectionString;

        private readonly string[] GridTypes = { "EditGrid", "InfoGrid", "DropDownGrid", "Normal" };
        private readonly string[] ControlTypes = { "TextBox", "Number", "Date", "DropDown" };

        public ConfigureFieldsFormAdvanced()
        {
            BuildUI();
        }

        public void Initialize(TableSchemaDto schema, SelectionDto selection, string connectionString)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _selection = selection ?? new SelectionDto();
            _connectionString = connectionString;

            cmbGridType.Items.Clear();
            foreach (var g in GridTypes) cmbGridType.Items.Add(g);
            cmbGridType.SelectedItem = "EditGrid";

            UpdateValidationButtonState();
            LoadGrid();
        }

        public SelectionDto GetSelection() => _selection;

        // =====================================================
        // UI
        // =====================================================
        private void BuildUI()
        {
            Text = "Configure Grid Fields";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;

            cmbGridType = new ComboBox { Left = 12, Top = 12, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGridType.SelectedIndexChanged += (_, __) =>
            {
                _gridType = cmbGridType.SelectedItem.ToString();
                UpdateValidationButtonState();
                LoadGrid();
            };

            chkShowSelectedOnly = new CheckBox { Left = 230, Top = 15, Width = 220, Text = "Show only selected fields" };
            chkShowSelectedOnly.CheckedChanged += (_, __) => LoadGrid();

            btnSelectAll = new Button { Text = "Select All", Left = 470, Top = 10, Width = 100 };
            btnSelectAll.Click += (_, __) =>
            {
                foreach (DataGridViewRow r in dgv.Rows)
                    r.Cells["Use"].Value = true;
            };

            btnSort = new Button { Text = "Sort (Group → Order)", Left = 580, Top = 10, Width = 160 };
            btnSort.Click += (_, __) => SortGrid();

            btnValidation = new Button // 🔴 NEW
            {
                Text = "Validations...",
                Left = 750,
                Top = 10,
                Width = 140,
                Enabled = false
            };
            btnValidation.Click += (_, __) => OpenValidationPopup();

            dgv = new DataGridView
            {
                Left = 12,
                Top = 44,
                Width = ClientSize.Width - 24,
                Height = ClientSize.Height - 120,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgv.CellEndEdit += Dgv_CellEndEdit;

            btnApply = new Button { Text = "Apply", Width = 120, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnApply.Location = new Point(ClientSize.Width - 260, ClientSize.Height - 60);
            btnApply.Click += ApplyChanges;

            btnCancel = new Button { Text = "Cancel", Width = 120, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnCancel.Location = new Point(ClientSize.Width - 130, ClientSize.Height - 60);
            btnCancel.Click += (_, __) => Close();

            Controls.AddRange(new Control[]
            {
                cmbGridType,
                chkShowSelectedOnly,
                btnSelectAll,
                btnSort,
                btnValidation, // 🔴 NEW
                dgv,
                btnApply,
                btnCancel
            });
        }

        // =====================================================
        // VALIDATION BUTTON LOGIC
        // =====================================================
        private void UpdateValidationButtonState()
        {
            btnValidation.Enabled = string.Equals(_gridType, "Normal", StringComparison.OrdinalIgnoreCase);
        }

        private void OpenValidationPopup()
        {
            // 🔴 SEED validation fields ONCE
            if (_selection.Validation.Fields.Count == 0)
            {
                foreach (var col in _schema.Columns)
                {
                    if (!_selection.Validation.Fields.ContainsKey(col.ColumnName))
                    {
                        _selection.Validation.Fields[col.ColumnName] = new FieldValidation();
                    }
                }
            }

            using (var dlg = new ValidationConfigForm(_selection))
            {
                dlg.ShowDialog(this);
            }
        }



        // =====================================================
        // GRID LOAD
        // =====================================================
        private void LoadGrid()
        {
            BuildColumns();
            dgv.Rows.Clear();

            var selected = _selection.GetSelectedColumns(_gridType);
            var hidden = _selection.GetHiddenColumns(_gridType);
            var unhidable = _selection.GetUnhidableColumns(_gridType);
            var validation = _selection.GetFieldValidation(_gridType);

            int rowNo = 1;

            foreach (var col in _schema.Columns)
            {
                if (chkShowSelectedOnly.Checked && !selected.Contains(col.ColumnName)) continue;

                int r = dgv.Rows.Add();
                var row = dgv.Rows[r];

                row.Cells["FieldName"].Value = col.ColumnName;
                row.Cells["Use"].Value = selected.Contains(col.ColumnName);
                row.Cells["DisplayName"].Value = col.ColumnName;

                if (validation.TryGetValue(col.ColumnName, out var meta))
                {
                    row.Cells["GroupNo"].Value = meta["GroupNo"];
                    row.Cells["Order"].Value = meta["Order"];
                    row.Cells["GroupTitle"].Value = meta["GroupTitle"];
                }
                else
                {
                    row.Cells["GroupNo"].Value = 1;
                    row.Cells["Order"].Value = rowNo;
                    row.Cells["GroupTitle"].Value = "";
                }

                row.Cells["Hidden"].Value = hidden.Contains(col.ColumnName);
                row.Cells["Unhidable"].Value = unhidable.Contains(col.ColumnName);
                row.Cells["ControlType"].Value = "TextBox";
                row.Cells["DropDown"].Value = false;
                row.Cells["ReturnColumns"].Value = "";
                row.Cells["FkPick"].Value = "";

                rowNo++;
            }

            SortGrid();
        }

        private void BuildColumns()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add(TextCol("FieldName", "Field", true));
            dgv.Columns.Add(CheckCol("Use", "Use"));
            dgv.Columns.Add(TextCol("DisplayName", "Display Name"));
            dgv.Columns.Add(TextCol("GroupNo", "Group No"));
            dgv.Columns.Add(TextCol("GroupTitle", "Group Title"));
            dgv.Columns.Add(TextCol("Order", "Order"));
            dgv.Columns.Add(CheckCol("Hidden", "Hidden"));
            dgv.Columns.Add(CheckCol("Unhidable", "Unhidable"));

            dgv.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "ControlType",
                HeaderText = "Control",
                DataSource = ControlTypes
            });

            dgv.Columns.Add(CheckCol("DropDown", "DropDown"));
            dgv.Columns.Add(TextCol("ReturnColumns", "Return Columns"));

            dgv.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "FkPick",
                HeaderText = "",
                Text = "...",
                UseColumnTextForButtonValue = true,
                Width = 40
            });
        }

        // =====================================================
        // ORDER / GROUP RULE ENFORCEMENT
        // =====================================================
        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgv.Rows[e.RowIndex];
            if (dgv.Columns[e.ColumnIndex].Name == "Order")
            {
                int order = IntVal(row, "Order");
                if (order == 0)
                    row.Cells["GroupNo"].Value = 0;
            }

            SortGrid();
        }

        // =====================================================
        // SORTING
        // =====================================================
        private void SortGrid()
        {
            var rows = dgv.Rows.Cast<DataGridViewRow>().ToList();

            var active = rows
                .Where(r => !(IntVal(r, "GroupNo") == 0 && IntVal(r, "Order") == 0))
                .OrderBy(r => IntVal(r, "GroupNo") * 10000 + IntVal(r, "Order"))
                .ToList();

            var parked = rows
                .Where(r => IntVal(r, "GroupNo") == 0 && IntVal(r, "Order") == 0)
                .ToList();

            dgv.Rows.Clear();
            foreach (var r in active.Concat(parked))
                dgv.Rows.Add(CloneRow(r));
        }

        private DataGridViewRow CloneRow(DataGridViewRow src)
        {
            var r = (DataGridViewRow)src.Clone();
            for (int i = 0; i < src.Cells.Count; i++)
                r.Cells[i].Value = src.Cells[i].Value;
            return r;
        }

        // =====================================================
        // APPLY
        // =====================================================
        private void ApplyChanges(object sender, EventArgs e)
        {
            var selected = new List<string>();
            var hidden = new List<string>();
            var unhidable = new List<string>();
            var validation = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                var field = Str(row, "FieldName");
                if (string.IsNullOrEmpty(field)) continue;

                int order = IntVal(row, "Order");
                int group = IntVal(row, "GroupNo");

                if (Bool(row, "Use")) selected.Add(field);
                if (order == 0) hidden.Add(field);
                if (Bool(row, "Unhidable")) unhidable.Add(field);

                validation[field] = new Dictionary<string, string>
                {
                    ["GroupNo"] = group.ToString(),
                    ["Order"] = order.ToString(),
                    ["GroupTitle"] = Str(row, "GroupTitle") ?? ""
                };
            }

            _selection.SetSelectedColumns(_gridType, selected);
            _selection.SetHiddenColumns(_gridType, hidden);
            _selection.SetUnhidableColumns(_gridType, unhidable);
            _selection.SetFieldValidation(_gridType, validation);

            DialogResult = DialogResult.OK;
            Close();
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private static bool Bool(DataGridViewRow r, string c) => r.Cells[c].Value is bool b && b;
        private static string Str(DataGridViewRow r, string c) => r.Cells[c].Value?.ToString();
        private static int IntVal(DataGridViewRow r, string c) => int.TryParse(Str(r, c), out var v) ? v : 0;

        private static DataGridViewTextBoxColumn TextCol(string name, string header, bool ro = false) =>
            new DataGridViewTextBoxColumn { Name = name, HeaderText = header, ReadOnly = ro };

        private static DataGridViewCheckBoxColumn CheckCol(string name, string header) =>
            new DataGridViewCheckBoxColumn { Name = name, HeaderText = header };
    }
}
