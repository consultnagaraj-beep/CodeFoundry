// ⚠ FULL FILE – DO NOT MERGE – REPLACE COMPLETELY
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CaritorCodeFoundry.UI;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.UI
{
    public class ConfigureFieldsFormAdvanced : Form
    {
        private Button btnPreview;

        private ComboBox cmbGridType;
        private CheckBox chkShowSelectedOnly;
        private CheckBox chkShowUnselectedOnly;
        private Button btnSelectAll;
        private Button btnSort;
        private Button btnValidation;
        private DataGridView dgv;
        private Button btnApply;
        private Button btnCancel;

        private TableSchemaDto _schema;
        private SelectionDto _selection;
        private string _gridType = "EditGrid";
        private string _connectionString;

        private readonly string[] GridTypes = { "EditGrid", "InfoGrid", "DropDownGrid", "Normal" };
        private readonly string[] ControlTypes = { "TextBox", "Number", "Date", "DropDown", "Label" };

        public ConfigureFieldsFormAdvanced()
        {
            BuildUI();
        }

        public void Initialize(TableSchemaDto schema, SelectionDto selection, string connectionString)
        {
            _schema = schema;
            _selection = selection;
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
            WindowState = FormWindowState.Maximized;

            cmbGridType = new ComboBox { Left = 12, Top = 12, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGridType.SelectedIndexChanged += (_, __) =>
            {
                _gridType = cmbGridType.SelectedItem.ToString();
                UpdateValidationButtonState();
                LoadGrid();
            };

            chkShowSelectedOnly = new CheckBox { Left = 230, Top = 15, Width = 220, Text = "Show only selected fields" };
            chkShowSelectedOnly.CheckedChanged += ChkShowSelectedOnly_CheckedChanged;


            chkShowUnselectedOnly = new CheckBox
            {
                Left = chkShowSelectedOnly.Right + 50,
                Top = chkShowSelectedOnly.Top,
                Width = 240,
                Text = "Show only unselected fields"
                
            };

            chkShowUnselectedOnly.CheckedChanged += ChkShowUnselectedOnly_CheckedChanged;
            int actionLeft = chkShowUnselectedOnly.Right + 15;
            btnSelectAll = new Button { Text = "Select All", Left = actionLeft, Top = 10, Width = 100 };
            btnSelectAll.Click += (_, __) =>
            {
                foreach (DataGridViewRow r in dgv.Rows)
                    r.Cells["Use"].Value = true;
            };

            btnSort = new Button { Text = "Sort (Group → Order)", Left = btnSelectAll.Right + 10, Top = 10, Width = 160 };
            btnSort.Click += (_, __) => SortGrid();

            btnValidation = new Button { Text = "Validations...", Left = btnSort.Right + 10, Top = 10, Width = 140 };
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgv.CellContentClick += Dgv_CellContentClick;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            btnPreview = new Button { Text = "Preview", Width = 80, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnPreview.Location = new Point(ClientSize.Width - 360, ClientSize.Height - 60);
            btnPreview.Click += BtnPreview_Click;

            //btnPreview = new Button(Text = "Preview", Width = 120, Anchor = AnchorStyles.Bottom | AnchorStyles.Right);
            //this.btnPreview.Location = new System.Drawing.Point(Width - 260, Height - 45);
            //this.btnPreview.Name = "btnPreview";
            //this.btnPreview.Size = new System.Drawing.Size(90, 28);
            //this.btnPreview.Text = "Preview";
            //this.btnPreview.UseVisualStyleBackColor = true;
            //this.btnPreview.Click += new System.EventHandler(this.BtnPreview_Click);

            this.Controls.Add(this.btnPreview);

            btnApply = new Button { Text = "Apply", Width = 120, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnApply.Location = new Point(ClientSize.Width - 260, ClientSize.Height - 60);
            btnApply.Click += ApplyChanges;

            btnCancel = new Button { Text = "Cancel", Width = 120, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnCancel.Location = new Point(ClientSize.Width - 130, ClientSize.Height - 60);
            btnCancel.Click += (_, __) => Close();

            Controls.AddRange(new Control[]
            {
                cmbGridType, chkShowSelectedOnly,chkShowUnselectedOnly, btnSelectAll,
                btnSort, btnValidation, dgv, btnApply, btnCancel
            });
        }
        private void BtnPreview_Click(object sender, EventArgs e)
        {
            // STEP-2 will go here
            using (var dlg = new PreviewDialog())
            {
                dlg.ShowDialog(this);
            }
        }

        private void SeedColumnDataTypes()
        {
            if (_selection.ColumnDataTypes.Count > 0)
                return;

            foreach (var col in _schema.Columns)
            {
                // Use DB-native datatype, normalized
                _selection.ColumnDataTypes[col.ColumnName] =
                    col.DataType.ToLowerInvariant();
            }
        }

        // =====================================================
        // GRID LOAD
        // =====================================================
        //private void LoadGrid()
        //{
        //    BuildColumns();
        //    dgv.Rows.Clear();

        //    var selected = _selection.GetSelectedColumns(_gridType);
        //    var hidden = _selection.GetHiddenColumns(_gridType);
        //    var unhidable = _selection.GetUnhidableColumns(_gridType);
        //    var validation = _selection.GetFieldValidation(_gridType);
        //    var fkSelections = _selection.GetFkSelections(_gridType);

        //    int orderNo = 1;

        //    foreach (var col in _schema.Columns)
        //    {
        //        int r = dgv.Rows.Add();
        //        var row = dgv.Rows[r];

        //        row.Cells["FieldName"].Value = col.ColumnName;
        //        row.Cells["Use"].Value = selected.Contains(col.ColumnName);
        //        row.Cells["GroupNo"].Value = 1;
        //        row.Cells["Order"].Value = orderNo++;
        //        row.Cells["GroupTitle"].Value = "";
        //        row.Cells["Hidden"].Value = hidden.Contains(col.ColumnName);
        //        row.Cells["Unhidable"].Value = unhidable.Contains(col.ColumnName);
        //        row.Cells["ControlType"].Value = "TextBox";
        //        row.Cells["DropDown"].Value = false;
        //        row.Cells["ReturnColumns"].Value = "";

        //        if (fkSelections.TryGetValue(col.ColumnName, out var fkCols))
        //        {
        //            row.Cells["ControlType"].Value = "DropDown";
        //            row.Cells["DropDown"].Value = true;
        //            row.Cells["ReturnColumns"].Value = string.Join(",", fkCols);
        //            InsertFkChildRows(r, col.ColumnName, fkCols);
        //        }
        //    }
        //}
        private void ChkShowSelectedOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowSelectedOnly.Checked)
                chkShowUnselectedOnly.Checked = false;

            ApplyRowFilter();
        }
        private void ChkShowUnselectedOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowUnselectedOnly.Checked)
                chkShowSelectedOnly.Checked = false;

            ApplyRowFilter();
        }
        private void ApplyRowFilter()
        {
            bool showSelected = chkShowSelectedOnly.Checked;
            bool showUnselected = chkShowUnselectedOnly.Checked;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow)
                    continue;

                var fieldName = row.Cells["FieldName"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(fieldName))
                    continue;

                bool isSelected =
                    Convert.ToBoolean(row.Cells["Use"].Value);

                bool isFkChild =
                    fieldName.Contains(".");

                // Default: show everything
                bool visible = true;

                if (showSelected)
                {
                    // Selected + FK rows
                    visible = isSelected || isFkChild;
                }
                else if (showUnselected)
                {
                    // Unselected rows ONLY
                    // FK rows hidden to avoid confusion
                    visible = !isSelected && !isFkChild;
                }

                row.Visible = visible;
            }
        }

        private void ChkShowSelectedOnly_CheckedChangedOld(object sender, EventArgs e)
        {
            bool showOnlySelected = chkShowSelectedOnly.Checked;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow)
                    continue;

                var fieldName = row.Cells["FieldName"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(fieldName))
                    continue;

                bool isSelected =
                    Convert.ToBoolean(row.Cells["Use"].Value);

                bool isFkChild =
                    fieldName.Contains(".");

                if (showOnlySelected)
                {
                    // View filter ONLY — no state change
                    row.Visible = isSelected || isFkChild;
                }
                else
                {
                    row.Visible = true;
                }
            }
        }

        private static string MakeFriendly(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return string.Empty;

            return string.Join(" ",
                columnName
                    .Replace("_", " ")
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
        }

        private void LoadGrid()
        {
            BuildColumns();
            dgv.Rows.Clear();

            var selected = _selection.GetSelectedColumns(_gridType);
            var hidden = _selection.GetHiddenColumns(_gridType);
            var unhidable = _selection.GetUnhidableColumns(_gridType);
            var validation = _selection.GetFieldValidation(_gridType);
            var fkSelections = _selection.GetFkSelections(_gridType);

            // NEW: display name metadata
            var displayNames = _selection.GetDisplayNames(_gridType);

            int orderNo = 1;

            foreach (var col in _schema.Columns)
            {
                int r = dgv.Rows.Add();
                var row = dgv.Rows[r];

                row.Cells["FieldName"].Value = col.ColumnName;
                row.Cells["Use"].Value = selected.Contains(col.ColumnName);
                row.Cells["GroupNo"].Value = 1;
                row.Cells["Order"].Value = orderNo++;
                row.Cells["GroupTitle"].Value = "";
                row.Cells["Hidden"].Value = hidden.Contains(col.ColumnName);
                row.Cells["Unhidable"].Value = unhidable.Contains(col.ColumnName);
                row.Cells["ControlType"].Value = "TextBox";
                row.Cells["DropDown"].Value = false;
                row.Cells["ReturnColumns"].Value = "";

                // ============================
                // DISPLAY NAME HANDLING (NEW)
                // ============================
                bool isEdited = false;

                string displayName;

                SelectionDto.DisplayNameMeta dn;
                if (displayNames.TryGetValue(col.ColumnName, out dn) &&
                    !string.IsNullOrWhiteSpace(dn.DisplayName))
                {
                    displayName = dn.DisplayName;
                }
                else
                {
                    displayName = NamingHelper.ToDisplayName(col.ColumnName);
                }

                row.Cells["DisplayName"].Value = displayName;
                row.Tag = new
                {
                    IsDisplayNameEdited = isEdited
                };

                // ============================
                // FK HANDLING (UNCHANGED)
                // ============================
                if (fkSelections.TryGetValue(col.ColumnName, out var fkCols))
                {
                    row.Cells["ControlType"].Value = "DropDown";
                    row.Cells["DropDown"].Value = true;
                    row.Cells["ReturnColumns"].Value = string.Join(",", fkCols);

                    InsertFkChildRows(r, col.ColumnName, fkCols);
                }
            }
        }

        // =====================================================
        // FK CHILD ROWS
        // =====================================================
        private void InsertFkChildRows(int parentRowIndex, string fkField, List<string> fkCols)
        {
            int insertAt = parentRowIndex + 1;

            // Compute parent friendly name once
            string parentDisplay =
                NamingHelper.ToPascalCase(fkField);

            foreach (var col in fkCols)
            {
                dgv.Rows.Insert(insertAt, 1);
                var row = dgv.Rows[insertAt];

                string childFriendly = MakeFriendly(col);

                row.Cells["FieldName"].Value = fkField + "." + col;
                row.Cells["Use"].Value = true;
                row.Cells["GroupNo"].Value = 0;
                row.Cells["Order"].Value = 0;
                row.Cells["GroupTitle"].Value = "";
                row.Cells["Hidden"].Value = true;
                row.Cells["Unhidable"].Value = true;
                row.Cells["ControlType"].Value = "Label";
                row.Cells["DropDown"].Value = false;
                row.Cells["ReturnColumns"].Value = "";

                // =========================
                // DISPLAY NAME (NEW)
                // =========================
                row.Cells["DisplayName"].Value =
                    parentDisplay + " " + childFriendly;

                // FK child rows are system-derived
                row.ReadOnly = true;

                insertAt++;
            }
        }

        //private void InsertFkChildRows(int parentRowIndex, string fkField, List<string> fkCols)
        //{
        //    int insertAt = parentRowIndex + 1;

        //    foreach (var col in fkCols)
        //    {
        //        dgv.Rows.Insert(insertAt, 1);
        //        var row = dgv.Rows[insertAt];

        //        row.Cells["FieldName"].Value = $"{fkField}.{col}";
        //        row.Cells["Use"].Value = true;
        //        row.Cells["GroupNo"].Value = 0;

        //        row.Cells["Order"].Value = 0;
        //        row.Cells["GroupTitle"].Value = "";
        //        row.Cells["Hidden"].Value = true;
        //        row.Cells["Unhidable"].Value = true;
        //        row.Cells["ControlType"].Value = "Label";
        //        row.Cells["DropDown"].Value = false;
        //        row.Cells["ReturnColumns"].Value = "";
        //        row.ReadOnly = true;

        //        insertAt++;
        //    }
        //}

        // =====================================================
        // FK PICK
        // =====================================================
        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "FkPick")
                return;

            var row = dgv.Rows[e.RowIndex];
            var field = row.Cells["FieldName"].Value?.ToString();
            if (string.IsNullOrEmpty(field)) return;

            var fk = _schema.ForeignKeys.FirstOrDefault(x =>
                string.Equals(x.ColumnName, field, StringComparison.OrdinalIgnoreCase));

            if (fk == null) return;

            var fkSchema =
                SchemaReader.GetTableSchema(_connectionString, fk.ReferencedTable);

            // Capture parent context BEFORE dialog
            int parentRowIndex = e.RowIndex;

            int parentGroupNo =
                Convert.ToInt32(row.Cells["GroupNo"].Value);

            int parentOrderNo =
                Convert.ToInt32(row.Cells["Order"].Value);

            // Get existing FK selections (IMPORTANT)
            var fkMap = _selection.GetFkSelections(_gridType);
            List<string> existingFkCols;
            fkMap.TryGetValue(field, out existingFkCols);
            string existingFkColsCsv =
    existingFkCols != null
        ? string.Join(",", existingFkCols)
        : null;
            using (var dlg = new FkColumnSelectorForm(
                       fkSchema,
                       fk.ReferencedTable,
                       existingFkColsCsv))
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                // -------------------------------
                // Persist FK selection
                // -------------------------------
                fkMap[field] = dlg.SelectedColumns;
                _selection.SetFkSelections(_gridType, fkMap);

                // -------------------------------
                // Update parent row UI
                // -------------------------------
                row.Cells["ControlType"].Value = "DropDown";
                row.Cells["DropDown"].Value = true;
                row.Cells["ReturnColumns"].Value =
                    string.Join(",", dlg.SelectedColumns);

                // -------------------------------
                // Apply FK rows with ordering
                // -------------------------------
                var ctx = new FkConfigContext
                {
                    GridType = _gridType,
                    ParentFieldName = field,
                    ParentRowIndex = parentRowIndex,
                    ReferencedTableName=fk.ReferencedTable,
                    ParentGroupNo = parentGroupNo,
                    ParentOrderNo = parentOrderNo
                };

                ApplyFkSelection(ctx, dlg.SelectedColumns);
            }
        }
        private static bool IsFkChildRow(string fieldName, string parentField)
        {
            return fieldName != null &&
                   fieldName.StartsWith(parentField + ".", System.StringComparison.OrdinalIgnoreCase);
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
                var field = row.Cells["FieldName"].Value?.ToString();
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
        private void ApplyFkSelection(
    FkConfigContext ctx,
    List<string> selectedFkColumns)
        {
            // -----------------------------------------
            // 1. Remove existing FK child rows
            // -----------------------------------------
            for (int i = dgv.Rows.Count - 1; i >= 0; i--)
            {
                var row = dgv.Rows[i];
                if (row.IsNewRow)
                    continue;

                var fieldName = row.Cells["FieldName"].Value?.ToString();
                if (IsFkChildRow(fieldName, ctx.ParentFieldName))
                {
                    dgv.Rows.RemoveAt(i);
                }
            }

            // -----------------------------------------
            // 2. If no FK columns selected → done
            // -----------------------------------------
            if (selectedFkColumns == null || selectedFkColumns.Count == 0)
                return;

            // -----------------------------------------
            // 3. Insert FK rows
            // -----------------------------------------
            int insertAt = ctx.ParentRowIndex + 1;
            int currentOrder = ctx.ParentOrderNo;

            bool isSystemGroup = ctx.ParentGroupNo == 0;

            foreach (var col in selectedFkColumns)
            {
                dgv.Rows.Insert(insertAt, 1);
                var row = dgv.Rows[insertAt];

                row.Cells["FieldName"].Value =
                    ctx.ParentFieldName + "." + col;

                row.Cells["Use"].Value = true;
                row.Cells["GroupNo"].Value = isSystemGroup ? 0 : ctx.ParentGroupNo;
                row.Cells["Order"].Value = isSystemGroup ? 0 : ++currentOrder;
                row.Cells["GroupTitle"].Value = "";
                row.Cells["Hidden"].Value = false;
                row.Cells["Unhidable"].Value = false;
                row.Cells["ControlType"].Value = "TextBox";
                row.Cells["DropDown"].Value = false;
                row.Cells["ReturnColumns"].Value = "";

                // ----------------------------
                // DisplayName (FIXED)
                // ----------------------------
                row.Cells["DisplayName"].Value =
    NamingHelper.ToDisplayName(
        NamingHelper.ToPascalCase(ctx.ReferencedTableName)) +
    " " +
    NamingHelper.ToDisplayName(col);


                row.ReadOnly = false;

                insertAt++;
            }

            // -----------------------------------------
            // 4. Recalculate Order for remaining rows
            //    (ONLY same group, ONLY if GroupNo > 0)
            // -----------------------------------------
            if (!isSystemGroup)
            {
                for (int i = insertAt; i < dgv.Rows.Count; i++)
                {
                    var row = dgv.Rows[i];
                    if (row.IsNewRow)
                        continue;

                    int groupNo =
                        System.Convert.ToInt32(row.Cells["GroupNo"].Value);

                    if (groupNo != ctx.ParentGroupNo)
                        continue;

                    row.Cells["Order"].Value = ++currentOrder;
                }
            }
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private void BuildColumns()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add(TextCol("FieldName", "Field", true));
            dgv.Columns.Add(CheckCol("Use", "Use"));
            dgv.Columns.Add(TextCol("DisplayName", "DisplayName"));
            dgv.Columns.Add(TextCol("GroupNo", "Group"));
            dgv.Columns.Add(TextCol("Order", "Order"));
            dgv.Columns.Add(TextCol("GroupTitle", "Group Title"));
            dgv.Columns.Add(CheckCol("Hidden", "Hidden"));
            dgv.Columns.Add(CheckCol("Unhidable", "Unhidable"));
            dgv.Columns.Add(new DataGridViewComboBoxColumn { Name = "ControlType", HeaderText = "Control", DataSource = ControlTypes });
            dgv.Columns.Add(CheckCol("DropDown", "DropDown"));
            dgv.Columns.Add(TextCol("ReturnColumns", "Return Columns"));
            dgv.Columns.Add(new DataGridViewButtonColumn { Name = "FkPick", Text = "...", UseColumnTextForButtonValue = true, Width = 40 });
        }

        private void SortGrid()
        {
            var rows = dgv.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .ToList();

            var active = rows
                .Where(r => !(IntVal(r, "GroupNo") == 0 && IntVal(r, "Order") == 0))
                .OrderBy(r => IntVal(r, "GroupNo"))
                .ThenBy(r => IntVal(r, "Order"))
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
            var clone = (DataGridViewRow)src.Clone();

            for (int i = 0; i < src.Cells.Count; i++)
                clone.Cells[i].Value = src.Cells[i].Value;

            return clone;
        }

        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dgv.Columns[e.ColumnIndex].Name == "Order")
            {
                var row = dgv.Rows[e.RowIndex];
                int order = IntVal(row, "Order");

                if (order == 0)
                    row.Cells["GroupNo"].Value = 0;
            }
        }

        private void UpdateValidationButtonState()
        {
            btnValidation.Enabled =
                string.Equals(_gridType, "Normal", StringComparison.OrdinalIgnoreCase);
        }
        private void OpenValidationPopup()
        {
            // -------------------------------------------------
            // Build LIVE field snapshot from grid (NO Apply dependency)
            // -------------------------------------------------
            string fieldsCsv = string.Join(
                ",",
                dgv.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Where(r => Convert.ToBoolean(r.Cells["Use"].Value))
                    .Where(r => !Convert.ToBoolean(r.Cells["Hidden"].Value))
                    .Where(r => Convert.ToInt32(r.Cells["Order"].Value) > 0)
                    .Where(r => !r.Cells["FieldName"].Value.ToString().Contains(".")) // exclude FK child rows
                    .Select(r => r.Cells["FieldName"].Value.ToString())
            );

            // -------------------------------------------------
            // DO NOT seed from schema here
            // ValidationConfig will create entries lazily
            // -------------------------------------------------

            using (var dlg = new ValidationConfigForm(
                _selection,
                _gridType,
                string.IsNullOrWhiteSpace(fieldsCsv) ? null : fieldsCsv))
            {
                SeedColumnDataTypes();

                dlg.ShowDialog(this);
            }
        }

        //private void OpenValidationPopup()
        //{
        //    // Seed validation dictionary once
        //    if (_selection.Validation.Fields.Count == 0)
        //    {
        //        foreach (var col in _schema.Columns)
        //        {
        //            if (!_selection.Validation.Fields.ContainsKey(col.ColumnName))
        //                _selection.Validation.Fields[col.ColumnName] = new FieldValidation();
        //        }
        //    }

        //    using (var dlg = new ValidationConfigForm(_selection,_gridType))
        //    {
        //        dlg.ShowDialog(this);
        //    }
        //}


        private static bool Bool(DataGridViewRow r, string c) => r.Cells[c].Value is bool b && b;
        private static string Str(DataGridViewRow r, string c) => r.Cells[c].Value?.ToString();
        private static int IntVal(DataGridViewRow r, string c) => int.TryParse(Str(r, c), out var v) ? v : 0;

        private static DataGridViewTextBoxColumn TextCol(string name, string header, bool ro = false) =>
            new DataGridViewTextBoxColumn { Name = name, HeaderText = header, ReadOnly = ro };

        private static DataGridViewCheckBoxColumn CheckCol(string name, string header) =>
            new DataGridViewCheckBoxColumn { Name = name, HeaderText = header };
    }
    public class FkConfigContext
    {
        public string GridType { get; set; }

        public string ParentFieldName { get; set; }
        // FK referenced table(e.g., Department)
    public string ReferencedTableName { get; set; }

        public int ParentRowIndex { get; set; }

        public int ParentGroupNo { get; set; }

        public int ParentOrderNo { get; set; }

        public List<string> ExistingFkColumns { get; set; }
    }

}
