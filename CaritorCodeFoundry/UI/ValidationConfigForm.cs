using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.UI
{
    public class ValidationConfigForm : Form
    {
        private readonly SelectionDto _selection;

        private ListBox lstFields;

        // ================= UI =================
        private CheckBox chkUiRequired;
        private TextBox txtUiRequiredMsg;

        private CheckBox chkUiMinLen;
        private NumericUpDown numUiMinLen;
        private TextBox txtUiMinLenMsg;

        private CheckBox chkUiMaxLen;
        private NumericUpDown numUiMaxLen;
        private TextBox txtUiMaxLenMsg;

        private CheckBox chkUiPattern;
        private ComboBox cmbUiPattern;
        private TextBox txtUiPatternMsg;

        // ================= DB =================
        private CheckBox chkDbRequired;
        private TextBox txtDbRequiredMsg;

        private CheckBox chkDbMinLen;
        private NumericUpDown numDbMinLen;
        private TextBox txtDbMinLenMsg;

        private CheckBox chkDbMaxLen;
        private NumericUpDown numDbMaxLen;
        private TextBox txtDbMaxLenMsg;

        private CheckBox chkDbMinValue;
        private NumericUpDown numDbMinValue;
        private TextBox txtDbMinValueMsg;

        private CheckBox chkDbMaxValue;
        private NumericUpDown numDbMaxValue;
        private TextBox txtDbMaxValueMsg;

        private CheckBox chkDbUnique;
        private TextBox txtDbUniqueCols;
        private TextBox txtDbUniqueMsg;

        private Button btnClose;

        private readonly string _gridType;
        private readonly string[] _currentFields;

        public ValidationConfigForm(
            SelectionDto selection,
            string gridType,
            string fieldsCsv)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _gridType = gridType;

            _currentFields = string.IsNullOrWhiteSpace(fieldsCsv)
                ? null
                : fieldsCsv.Split(
                    new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);

            BuildUI();
            LoadFields();
        }
        private string ResolveDisplayName(string field)
        {
            // Prefer DisplayName from SelectionDto for the current grid type
            var map = _selection.GetDisplayNames(_gridType);
            if (map != null &&
                map.TryGetValue(field, out var meta) &&
                !string.IsNullOrWhiteSpace(meta.DisplayName))
            {
                return meta.DisplayName;
            }

            // Fallback: make it human-readable
            return NamingHelper.ToDisplayName(field);
        }
       


        //public ValidationConfigForm(SelectionDto selection, string gridType,string fieldsCsv)
        //{
        //    _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        //    _gridType = gridType;
        //    BuildUI();
        //    LoadFields();
        //}
        private string GetDisplayName(string field)
        {
            var map = _selection.GetDisplayNames(_gridType);

            if (map != null &&
                map.TryGetValue(field, out var meta) &&
                !string.IsNullOrWhiteSpace(meta.DisplayName))
            {
                return meta.DisplayName;
            }

            return field;
        }

        // =====================================================
        // UI
        // =====================================================
        private void BuildUI()
        {
            Text = "Configure Validations (Normal)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1250, 540);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            lstFields = new ListBox
            {
                Left = 10,
                Top = 10,
                Width = 220,
                Height = 460
            };
            lstFields.SelectedIndexChanged += (_, __) => LoadFieldValidation();

            // ================= UI GROUP =================
            var grpUi = new GroupBox
            {
                Text = "UI Validation",
                Left = 240,
                Top = 10,
                Width = 480,
                Height = 460
            };

            int y = 30;

            chkUiRequired = NewCheck("Required", y);
            txtUiRequiredMsg = NewText(160, y);
            y += 32;

            chkUiMinLen = NewCheck("Min Length", y);
            numUiMinLen = NewNum(160, y, 1);
            txtUiMinLenMsg = NewText(230, y);
            y += 32;

            chkUiMaxLen = NewCheck("Max Length", y);
            numUiMaxLen = NewNum(160, y, 0);
            txtUiMaxLenMsg = NewText(230, y);
            y += 32;

            chkUiPattern = NewCheck("Pattern Type", y);
            cmbUiPattern = new ComboBox
            {
                Left = 160,
                Top = y,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbUiPattern.Items.AddRange(new object[]
            {
                "Email", "Mobile", "PAN", "Aadhaar", "IFSC"
            });
            txtUiPatternMsg = NewText(290, y);

            grpUi.Controls.AddRange(new Control[]
            {
                chkUiRequired, txtUiRequiredMsg,
                chkUiMinLen, numUiMinLen, txtUiMinLenMsg,
                chkUiMaxLen, numUiMaxLen, txtUiMaxLenMsg,
                chkUiPattern, cmbUiPattern, txtUiPatternMsg
            });

            // ================= DB GROUP =================
            var grpDb = new GroupBox
            {
                Text = "DB Validation",
                Left = 740,
                Top = 10,
                Width = 480,
                Height = 460
            };

            y = 30;

            chkDbRequired = NewCheck("Required", y);
            txtDbRequiredMsg = NewText(160, y);
            y += 32;

            chkDbMinLen = NewCheck("Min Length", y);
            numDbMinLen = NewNum(160, y, 1);
            txtDbMinLenMsg = NewText(230, y);
            y += 32;

            chkDbMaxLen = NewCheck("Max Length", y);
            numDbMaxLen = NewNum(160, y, 0);
            txtDbMaxLenMsg = NewText(230, y);
            y += 32;

            chkDbMinValue = NewCheck("Min Value", y);
            numDbMinValue = NewNum(160, y, 0);
            txtDbMinValueMsg = NewText(230, y);
            y += 32;

            chkDbMaxValue = NewCheck("Max Value", y);
            numDbMaxValue = NewNum(160, y, 0);
            txtDbMaxValueMsg = NewText(230, y);
            y += 32;

            chkDbUnique = NewCheck("Unique (Duplicate)", y);
            txtDbUniqueCols = new TextBox { Left = 160, Top = y, Width = 120 };
            txtDbUniqueMsg = new TextBox { Left = 290, Top = y, Width = 160 };

            grpDb.Controls.AddRange(new Control[]
            {
                chkDbRequired, txtDbRequiredMsg,
                chkDbMinLen, numDbMinLen, txtDbMinLenMsg,
                chkDbMaxLen, numDbMaxLen, txtDbMaxLenMsg,
                chkDbMinValue, numDbMinValue, txtDbMinValueMsg,
                chkDbMaxValue, numDbMaxValue, txtDbMaxValueMsg,
                chkDbUnique, txtDbUniqueCols, txtDbUniqueMsg
            });

            btnClose = new Button
            {
                Text = "Close",
                Width = 120,
                Left = 1080,
                Top = 480
            };
            btnClose.Click += (_, __) => Close();

            Controls.AddRange(new Control[]
            {
                lstFields, grpUi, grpDb, btnClose
            });

            WireSaveHandlers();
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private CheckBox NewCheck(string text, int y) =>
            new CheckBox { Text = text, Left = 10, Top = y };

        private TextBox NewText(int x, int y) =>
            new TextBox { Left = x, Top = y, Width = 200 };

        private NumericUpDown NewNum(int x, int y, int min) =>
            new NumericUpDown { Left = x, Top = y, Width = 60, Minimum = min, Maximum = 999999 };

        private void WireSaveHandlers()
        {
            foreach (var c in Controls.OfType<GroupBox>()
                .SelectMany(g => g.Controls.Cast<Control>()))
            {
                if (c is CheckBox cb) cb.CheckedChanged += (_, __) => SaveCurrent();
                if (c is TextBox tb) tb.TextChanged += (_, __) => SaveCurrent();
                if (c is NumericUpDown n) n.ValueChanged += (_, __) => SaveCurrent();
                if (c is ComboBox b) b.SelectedIndexChanged += (_, __) => SaveCurrent();
            }
        }
        private void LoadFields()
        {
            lstFields.Items.Clear();

            // ValidationConfig field list MUST come from live grid snapshot
            // Hidden fields and FK child rows are already excluded upstream
            if (_currentFields != null)
            {
                foreach (var f in _currentFields)
                    lstFields.Items.Add(f);
            }

            if (lstFields.Items.Count > 0)
                lstFields.SelectedIndex = 0;
        }

        //private void LoadFields()
        //{
        //    lstFields.Items.Clear();

        //    // Use grid order, not alphabetical
        //    var orderedFields =
        //        _selection.GetValidationFieldOrder(_gridType); // see helper below

        //    foreach (var f in orderedFields)
        //        lstFields.Items.Add(f);

        //    if (lstFields.Items.Count > 0)
        //        lstFields.SelectedIndex = 0;
        //}

        // =====================================================
        // DATA
        // =====================================================
        //private void LoadFields()
        //{
        //    lstFields.Items.Clear();
        //    foreach (var f in _selection.Validation.Fields.Keys.OrderBy(x => x))
        //        lstFields.Items.Add(f);

        //    if (lstFields.Items.Count > 0)
        //        lstFields.SelectedIndex = 0;
        //}

        private void LoadFieldValidation()
        {
            if (lstFields.SelectedItem == null) return;

            var field = lstFields.SelectedItem.ToString();
            var fv = GetOrCreate(field);

            AutoFillDefaults(field, fv);

            chkUiRequired.Checked = fv.Ui.Required?.Enabled == true;
            txtUiRequiredMsg.Text = fv.Ui.Required?.Message ?? "";

            chkUiMinLen.Checked = fv.Ui.MinLength?.Enabled == true;
            numUiMinLen.Value = fv.Ui.MinLength?.Value ?? 1;
            txtUiMinLenMsg.Text = fv.Ui.MinLength?.Message ?? "";

            chkUiMaxLen.Checked = fv.Ui.MaxLength?.Enabled == true;
            numUiMaxLen.Value = fv.Ui.MaxLength?.Value ?? 0;
            txtUiMaxLenMsg.Text = fv.Ui.MaxLength?.Message ?? "";

            chkUiPattern.Checked = fv.Ui.PatternType != null;
            cmbUiPattern.SelectedItem = fv.Ui.PatternType;
            txtUiPatternMsg.Text = fv.Ui.PatternMessage ?? "";

            chkDbRequired.Checked = fv.Db.Required?.Enabled == true;
            txtDbRequiredMsg.Text = fv.Db.Required?.Message ?? "";

            chkDbMinLen.Checked = fv.Db.MinLength?.Enabled == true;
            numDbMinLen.Value = fv.Db.MinLength?.Value ?? 1;
            txtDbMinLenMsg.Text = fv.Db.MinLength?.Message ?? "";

            chkDbMaxLen.Checked = fv.Db.MaxLength?.Enabled == true;
            numDbMaxLen.Value = fv.Db.MaxLength?.Value ?? 0;
            txtDbMaxLenMsg.Text = fv.Db.MaxLength?.Message ?? "";

            chkDbUnique.Checked = fv.Db.Unique?.Enabled == true;
            txtDbUniqueCols.Text = fv.Db.Unique?.ColumnsCsv ?? field;
            txtDbUniqueMsg.Text = fv.Db.Unique?.Message ?? $"{field} already exists";
        }

        private void SaveCurrent()
        {
            if (lstFields.SelectedItem == null) return;
            var field = lstFields.SelectedItem.ToString();
            var fv = GetOrCreate(field);

            fv.Ui.Required = chkUiRequired.Checked ? new RequiredRule { Enabled = true, Message = txtUiRequiredMsg.Text } : null;
            fv.Ui.MinLength = chkUiMinLen.Checked ? new LengthRule { Enabled = true, Value = (int)numUiMinLen.Value, Message = txtUiMinLenMsg.Text } : null;
            fv.Ui.MaxLength = chkUiMaxLen.Checked ? new LengthRule { Enabled = true, Value = (int)numUiMaxLen.Value, Message = txtUiMaxLenMsg.Text } : null;

            fv.Ui.PatternType = chkUiPattern.Checked ? cmbUiPattern.Text : null;
            fv.Ui.PatternMessage = chkUiPattern.Checked ? txtUiPatternMsg.Text : null;

            fv.Db.Required = chkDbRequired.Checked ? new RequiredRule { Enabled = true, Message = txtDbRequiredMsg.Text } : null;
            fv.Db.MinLength = chkDbMinLen.Checked ? new LengthRule { Enabled = true, Value = (int)numDbMinLen.Value, Message = txtDbMinLenMsg.Text } : null;
            fv.Db.MaxLength = chkDbMaxLen.Checked ? new LengthRule { Enabled = true, Value = (int)numDbMaxLen.Value, Message = txtDbMaxLenMsg.Text } : null;

            fv.Db.Unique = chkDbUnique.Checked
                ? new UniqueRule { Enabled = true, ColumnsCsv = txtDbUniqueCols.Text, Message = txtDbUniqueMsg.Text }
                : null;
        }

        private void AutoFillDefaults(string field, FieldValidation fv)
        {
            var displayName = ResolveDisplayName(field);

            // UI - Required
            if (fv.Ui.Required == null)
            {
                fv.Ui.Required = new RequiredRule
                {
                    Enabled = false,
                    Message = $"{displayName} is required"
                };
            }
            else if (string.IsNullOrWhiteSpace(fv.Ui.Required.Message))
            {
                fv.Ui.Required.Message = $"{displayName} is required";
            }

            // UI - Min Length
            if (fv.Ui.MinLength == null)
            {
                fv.Ui.MinLength = new LengthRule
                {
                    Enabled = false,
                    Value = 1,
                    Message = $"{displayName} must have at least 1 character"
                };
            }
            else if (string.IsNullOrWhiteSpace(fv.Ui.MinLength.Message))
            {
                fv.Ui.MinLength.Message = $"{displayName} must have at least 1 character";
            }

            // DB - Unique (default only if empty)
            if (fv.Db.Unique != null &&
                string.IsNullOrWhiteSpace(fv.Db.Unique.Message))
            {
                fv.Db.Unique.Message = $"{displayName} already exists";
            }
        }



        private FieldValidation GetOrCreate(string field)
        {
            if (!_selection.Validation.Fields.TryGetValue(field, out var fv))
            {
                fv = new FieldValidation();
                _selection.Validation.Fields[field] = fv;
            }
            return fv;
        }
    }
}
