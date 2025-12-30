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
        private ComboBox cmbUiDateRule;
        private TextBox txtUiDateRuleMsg;
        private Label lblUiDateRule;


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

            // 🔒 HARD GUARD – ZERO FIELDS = DO NOT OPEN FORM
            if (_currentFields == null || _currentFields.Length == 0)
            {
                MessageBox.Show(
                    "Please select at least one field before configuring validations.",
                    "Validation Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
                return;
            }

            LoadFields();
            // Pattern checkbox is ALWAYS ON and DISABLED (frozen rule)
            //chkUiPattern.Checked = true;
            //chkUiPattern.Enabled = false;
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
        private string GetFieldDataType(string field)
        {
            // Try SelectionDto first (if already stored)
            if (_selection.ColumnDataTypes != null &&
                _selection.ColumnDataTypes.TryGetValue(field, out var dt))
            {
                return dt.ToLower();
            }

            // Fallback: infer from schema naming (safe default)
            return "string";
        }
        private void ApplyDataTypeVisibility(string dataType)
        {
            bool isString = dataType.Contains("char") || dataType.Contains("text") || dataType == "string";
            bool isNumeric = dataType.Contains("int") || dataType.Contains("decimal") || dataType.Contains("float");
            bool isDate = dataType.Contains("date");
            bool isBool = dataType.Contains("bool");
            // -------- UI --------
            chkUiRequired.Visible = true;
            txtUiRequiredMsg.Visible = true;
            cmbUiDateRule.Visible = isDate;
            txtUiDateRuleMsg.Visible = isDate;
            lblUiDateRule.Visible = isDate;

            if (isDate)
            {
                chkUiMinLen.Visible = false;
                numUiMinLen.Visible = false;
                txtUiMinLenMsg.Visible = false;

                chkUiMaxLen.Visible = false;
                numUiMaxLen.Visible = false;
                txtUiMaxLenMsg.Visible = false;

                chkUiPattern.Visible = false;
                cmbUiPattern.Visible = false;
                txtUiPatternMsg.Visible = false;
                // 🔴 Date Rule visible ONLY for Date fields
                //chkUiDateRule.Visible = true;

                cmbUiDateRule.Visible = true;
                txtUiDateRuleMsg.Visible = true;
                lblUiDateRule.Visible = true;


            }
            else
            {
                chkUiMinLen.Visible = isString;
                numUiMinLen.Visible = isString;
                txtUiMinLenMsg.Visible = isString;

                chkUiMaxLen.Visible = isString;
                numUiMaxLen.Visible = isString;
                txtUiMaxLenMsg.Visible = isString;

                chkUiPattern.Visible = isString;
                cmbUiPattern.Visible = isString;
                txtUiPatternMsg.Visible = isString;
                //chkUiDateRule.Visible = false;
                cmbUiDateRule.Visible = false;
                txtUiDateRuleMsg.Visible = false;
                lblUiDateRule.Visible = false;

            }

            //if (isDate)
            //{
            //    chkUiMinLen.Visible = false;
            //    numUiMinLen.Visible = false;
            //    txtUiMinLenMsg.Visible = false;

            //    chkUiMaxLen.Visible = false;
            //    numUiMaxLen.Visible = false;
            //    txtUiMaxLenMsg.Visible = false;

            //    chkUiPattern.Visible = false;
            //    cmbUiPattern.Visible = false;
            //    txtUiPatternMsg.Visible = false;
            //}

            //// -------- UI --------
            //chkUiRequired.Visible = true;
            //txtUiRequiredMsg.Visible = true;

            //chkUiMinLen.Visible = isString;
            //numUiMinLen.Visible = isString;
            //txtUiMinLenMsg.Visible = isString;

            //chkUiMaxLen.Visible = isString;
            //numUiMaxLen.Visible = isString;
            //txtUiMaxLenMsg.Visible = isString;

            //chkUiPattern.Visible = isString;
            //cmbUiPattern.Visible = isString;
            //txtUiPatternMsg.Visible = isString;

            // -------- DB --------
            chkDbRequired.Visible = true;
            txtDbRequiredMsg.Visible = true;

            chkDbMinLen.Visible = false;
            numDbMinLen.Visible = false;
            txtDbMinLenMsg.Visible = false;

            chkDbMaxLen.Visible = false;
            numDbMaxLen.Visible = false;
            txtDbMaxLenMsg.Visible = false;

            chkDbMinValue.Visible = isNumeric;
            numDbMinValue.Visible = isNumeric;
            txtDbMinValueMsg.Visible = isNumeric;

            chkDbMaxValue.Visible = isNumeric;
            numDbMaxValue.Visible = isNumeric;
            txtDbMaxValueMsg.Visible = isNumeric;

            chkDbUnique.Visible = !isBool;
            txtDbUniqueCols.Visible = !isBool;
            txtDbUniqueMsg.Visible = !isBool;
        }

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
               "None", "Email", "Mobile", "PAN", "Aadhaar", "IFSC"
            });
            txtUiPatternMsg = NewText(290, y);
            // ================= DATE RULE =================
            y += 32;

             lblUiDateRule = new Label
            {
                Text = "Date Rule",
                Left = 10,
                Top = y + 4,
                Width = 120
            };

            cmbUiDateRule = new ComboBox
            {
                Left = 160,
                Top = y,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbUiDateRule.Items.AddRange(new object[]
            {"None",
    "Only Past",
    "Only Future",
    "Past + Today",
    "Today Only",
    "Today + Future"
            });

            txtUiDateRuleMsg = NewText(290, y);

            //grpUi.Controls.Add(lblUiDateRule);
            //grpUi.Controls.Add(cmbUiDateRule);
            //grpUi.Controls.Add(txtUiDateRuleMsg);


            grpUi.Controls.AddRange(new Control[]
            {
                chkUiRequired, txtUiRequiredMsg,
                chkUiMinLen, numUiMinLen, txtUiMinLenMsg,
                chkUiMaxLen, numUiMaxLen, txtUiMaxLenMsg,
                chkUiPattern, cmbUiPattern, txtUiPatternMsg,lblUiDateRule ,cmbUiDateRule,txtUiDateRuleMsg
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
        private bool _isLoading;
        private void LoadFieldValidation()
        {
            if (lstFields.SelectedItem == null) return;

            _isLoading = true;
            try
            {
                var field = lstFields.SelectedItem.ToString();
                var fv = GetOrCreate(field);

                AutoFillDefaults(field, fv);

                chkUiRequired.Checked = fv.Ui.Required?.Enabled == true;
                SafeSetText(txtUiRequiredMsg,fv.Ui.Required?.Message ?? "");

                chkUiMinLen.Checked = fv.Ui.MinLength?.Enabled == true;
                numUiMinLen.Value = fv.Ui.MinLength?.Value ?? 1;
                SafeSetText(txtUiMinLenMsg,fv.Ui.MinLength?.Message ?? "");

                chkUiMaxLen.Checked = fv.Ui.MaxLength?.Enabled == true;
                numUiMaxLen.Value = fv.Ui.MaxLength?.Value ?? 0;
                SafeSetText(txtUiMaxLenMsg,fv.Ui.MaxLength?.Message ?? "");

                // ===============================
                // UI – Pattern (with NONE support)
                // ===============================
                cmbUiPattern.SelectedItem = fv.Ui.PatternType ?? "None";

                // 🔒 ISSUE–1 FIX: reset stale pattern message on load
                if (!string.IsNullOrWhiteSpace(fv.Ui.PatternType) &&
                    !string.IsNullOrWhiteSpace(fv.Ui.PatternMessage))
                {
                    if (
                                                //(fv.Ui.PatternType == "None" && !fv.Ui.PatternMessage.Contains("None")) ||

                        (fv.Ui.PatternType == "Email" && !fv.Ui.PatternMessage.Contains("Email")) ||
                        (fv.Ui.PatternType == "Mobile" && !fv.Ui.PatternMessage.Contains("Mobile")) ||
                        (fv.Ui.PatternType == "PAN" && !fv.Ui.PatternMessage.Contains("PAN")) ||
                        (fv.Ui.PatternType == "Aadhaar" && !fv.Ui.PatternMessage.Contains("Aadhaar")) ||
                        (fv.Ui.PatternType == "IFSC" && !fv.Ui.PatternMessage.Contains("IFSC"))
                    )
                    {
                        fv.Ui.PatternMessage = null;
                    }
                }

                EnsureDefaultMessages(field, fv);

                // ✅ UI sync happens ONLY ONCE, at the end
                SafeSetText(txtUiPatternMsg,fv.Ui.PatternMessage ?? "");

// ===============================
// UI – Date Rule (Pattern clone)
// ===============================
cmbUiDateRule.SelectedItem = fv.Ui.DateRuleType ?? "None";

// 🔒 ISSUE–2 FIX: reset stale DateRule message on load
if (!string.IsNullOrWhiteSpace(fv.Ui.DateRuleType) &&
    !string.IsNullOrWhiteSpace(fv.Ui.DateRuleMessage))
{
    if (
        //(fv.Ui.DateRuleType == "None" && !fv.Ui.DateRuleMessage.Contains("None")) ||
        (fv.Ui.DateRuleType == "Only Past" && !fv.Ui.DateRuleMessage.Contains("past")) ||
        (fv.Ui.DateRuleType == "Only Future" && !fv.Ui.DateRuleMessage.Contains("future")) ||
        (fv.Ui.DateRuleType == "Past + Today" && !fv.Ui.DateRuleMessage.Contains("today")) ||
        (fv.Ui.DateRuleType == "Today Only" && !fv.Ui.DateRuleMessage.Contains("today")) ||
        (fv.Ui.DateRuleType == "Today + Future" && !fv.Ui.DateRuleMessage.Contains("future"))
    )
    {
        fv.Ui.DateRuleMessage = null;
    }
}

EnsureDefaultMessages(field, fv);
                //txtUiDateRuleMsg.Text= txtUiDateRuleMsg

                SafeSetText(txtUiDateRuleMsg,fv.Ui.DateRuleMessage ?? "");
                // ✅ UI sync ONLY ONCE

                //cmbUiDateRule.SelectedItem = fv.Ui.DateRuleType;
                //txtUiDateRuleMsg,fv.Ui.DateRuleMessage ?? "");

                chkDbRequired.Checked = fv.Db.Required?.Enabled == true;
                SafeSetText(txtDbRequiredMsg,fv.Db.Required?.Message ?? "");

                //chkDbMinLen.Checked = fv.Db.MinLength?.Enabled == true;
                //numDbMinLen.Value = fv.Db.MinLength?.Value ?? 1;
                //txtDbMinLenMsg,fv.Db.MinLength?.Message ?? "");

                //chkDbMaxLen.Checked = fv.Db.MaxLength?.Enabled == true;
                //numDbMaxLen.Value = fv.Db.MaxLength?.Value ?? 0;
                //txtDbMaxLenMsg,fv.Db.MaxLength?.Message ?? "");

                chkDbUnique.Checked = fv.Db.Unique?.Enabled == true;
                SafeSetText(txtDbUniqueCols,fv.Db.Unique?.ColumnsCsv ?? field);
                SafeSetText(txtDbUniqueMsg,fv.Db.Unique?.Message ?? $"{field} already exists");

                var dataType = GetFieldDataType(field);
                if (GetFieldDataType(field).Contains("char"))
                {
                    chkUiMaxLen.Checked = true;
                    chkUiMaxLen.Enabled = false;
                }
                ApplyDataTypeVisibility(dataType);
                EnsurePatternConsistency(fv);

            }
            finally
            {
                _isLoading = false;
            }
        }

        //private void LoadFieldValidation()
        //{
        //    if (lstFields.SelectedItem == null) return;

        //    var field = lstFields.SelectedItem.ToString();
        //    var fv = GetOrCreate(field);

        //    AutoFillDefaults(field, fv);

        //    chkUiRequired.Checked = fv.Ui.Required?.Enabled == true;
        //    txtUiRequiredMsg.Text = fv.Ui.Required?.Message ?? "";

        //    chkUiMinLen.Checked = fv.Ui.MinLength?.Enabled == true;
        //    numUiMinLen.Value = fv.Ui.MinLength?.Value ?? 1;
        //    txtUiMinLenMsg.Text = fv.Ui.MinLength?.Message ?? "";

        //    chkUiMaxLen.Checked = fv.Ui.MaxLength?.Enabled == true;
        //    numUiMaxLen.Value = fv.Ui.MaxLength?.Value ?? 0;
        //    txtUiMaxLenMsg.Text = fv.Ui.MaxLength?.Message ?? "";

        //    chkUiPattern.Checked = fv.Ui.PatternType != null;
        //    cmbUiPattern.SelectedItem = fv.Ui.PatternType;
        //    txtUiPatternMsg.Text = fv.Ui.PatternMessage ?? "";

        //    chkDbRequired.Checked = fv.Db.Required?.Enabled == true;
        //    txtDbRequiredMsg.Text = fv.Db.Required?.Message ?? "";

        //    chkDbMinLen.Checked = fv.Db.MinLength?.Enabled == true;
        //    numDbMinLen.Value = fv.Db.MinLength?.Value ?? 1;
        //    txtDbMinLenMsg.Text = fv.Db.MinLength?.Message ?? "";

        //    chkDbMaxLen.Checked = fv.Db.MaxLength?.Enabled == true;
        //    numDbMaxLen.Value = fv.Db.MaxLength?.Value ?? 0;
        //    txtDbMaxLenMsg.Text = fv.Db.MaxLength?.Message ?? "";

        //    chkDbUnique.Checked = fv.Db.Unique?.Enabled == true;
        //    txtDbUniqueCols.Text = fv.Db.Unique?.ColumnsCsv ?? field;
        //    txtDbUniqueMsg.Text = fv.Db.Unique?.Message ?? $"{field} already exists";
        //    var dataType = GetFieldDataType(field);
        //    ApplyDataTypeVisibility(dataType);

        //}
        private void EnsurePatternConsistency(FieldValidation fv)
        {
            if (fv.Ui.PatternType == null)
                return;

            // If message is empty OR mismatched, force regeneration
            if (string.IsNullOrWhiteSpace(fv.Ui.PatternMessage))
            {
                fv.Ui.PatternMessage = null;
            }
        }

        private void EnsureDefaultMessages(string field, FieldValidation fv)
        {
            var displayName = ResolveDisplayName(field);

            // UI - Required
            if (fv.Ui.Required != null &&
                string.IsNullOrWhiteSpace(fv.Ui.Required.Message))
            {
                fv.Ui.Required.Message = $"{displayName} is required";
            }

            // UI - Min Length
            if (fv.Ui.MinLength != null &&
                string.IsNullOrWhiteSpace(fv.Ui.MinLength.Message))
            {
                fv.Ui.MinLength.Message =
                    $"{displayName} must have at least {fv.Ui.MinLength.Value} characters";
            }

            // UI - Max Length
            if (fv.Ui.MaxLength != null &&
                string.IsNullOrWhiteSpace(fv.Ui.MaxLength.Message))
            {
                fv.Ui.MaxLength.Message =
                    $"{displayName} must not exceed {fv.Ui.MaxLength.Value} characters";
            }

            // UI - Pattern
            if (fv.Ui.PatternType != null &&
                string.IsNullOrWhiteSpace(fv.Ui.PatternMessage))
            {
                if (fv.Ui.PatternType == "Email")
                    fv.Ui.PatternMessage = "Email address is invalid";
                else if (fv.Ui.PatternType == "Mobile")
                    fv.Ui.PatternMessage = "Mobile number is invalid";
                else if (fv.Ui.PatternType == "PAN")
                    fv.Ui.PatternMessage = "PAN is invalid";
                else if (fv.Ui.PatternType == "Aadhaar")
                    fv.Ui.PatternMessage = "Aadhaar number is invalid";
                else if (fv.Ui.PatternType == "IFSC")
                    fv.Ui.PatternMessage = "IFSC code is invalid";
                else
                    fv.Ui.PatternMessage = $"{displayName} format is invalid";
            }
            // ===============================
            // UI - Date Rule (Pattern-style)
            // ===============================
            if (fv.Ui.DateRuleType != null &&
                string.IsNullOrWhiteSpace(fv.Ui.DateRuleMessage))
            {
                if (fv.Ui.DateRuleType == "Only Past")
                    fv.Ui.DateRuleMessage = $"{displayName} must be a past date";
                else if (fv.Ui.DateRuleType == "Only Future")
                    fv.Ui.DateRuleMessage = $"{displayName} must be a future date";
                else if (fv.Ui.DateRuleType == "Past + Today")
                    fv.Ui.DateRuleMessage = $"{displayName} must be today or a past date";
                else if (fv.Ui.DateRuleType == "Today Only")
                    fv.Ui.DateRuleMessage = $"{displayName} must be today";
                else if (fv.Ui.DateRuleType == "Today + Future")
                    fv.Ui.DateRuleMessage = $"{displayName} must be today or a future date";
                else
                    fv.Ui.DateRuleMessage = $"{displayName} date is invalid";
            }

            // DB - Required
            if (fv.Db.Required != null &&
                string.IsNullOrWhiteSpace(fv.Db.Required.Message))
            {
                fv.Db.Required.Message = $"{displayName} is required";
            }

            // DB - Unique
            if (fv.Db.Unique != null &&
                string.IsNullOrWhiteSpace(fv.Db.Unique.Message))
            {
                fv.Db.Unique.Message = $"{displayName} already exists";
            }
        }
        private void SafeSetText(TextBox tb, string value)
        {
            if (tb.Text != value)
            {
                _isLoading = true;
                tb.Text = value;
                _isLoading = false;
            }
        }

        private void SaveCurrent()
        {
            if (_isLoading) return;   // 🔴 THIS WAS MISSING
            if (lstFields.SelectedItem == null) return;

            var field = lstFields.SelectedItem.ToString();
            var fv = GetOrCreate(field);

            fv.Ui.Required = chkUiRequired.Checked
                ? new RequiredRule { Enabled = true, Message = txtUiRequiredMsg.Text }
                : null;

            fv.Ui.MinLength = chkUiMinLen.Checked
                ? new LengthRule { Enabled = true, Value = (int)numUiMinLen.Value, Message = txtUiMinLenMsg.Text }
                : null;

            if (chkUiMaxLen.Checked)
            {
                var newValue = (int)numUiMaxLen.Value;

                if (fv.Ui.MaxLength == null || fv.Ui.MaxLength.Value != newValue)
                {
                    fv.Ui.MaxLength = new LengthRule
                    {
                        Enabled = true,
                        Value = newValue,
                        Message = null   // 🔒 clear stale message
                    };

                    // 🔒 CRITICAL: sync UI immediately
                    //txtUiMaxLenMsg.Text = "";
                    SafeSetText(txtUiMaxLenMsg, "");
                }
            }
            else
            {
                fv.Ui.MaxLength = null;
            }


            var newPattern = cmbUiPattern.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(newPattern) || newPattern == "None")
            {
                fv.Ui.PatternType = null;
                fv.Ui.PatternMessage = null;
                //txtUiPatternMsg.Text = "";
                SafeSetText(txtUiPatternMsg, "");
            }
            else if (fv.Ui.PatternType != newPattern)
            {
                fv.Ui.PatternType = newPattern;
                fv.Ui.PatternMessage = null;
                SafeSetText(txtUiPatternMsg , "");
            }

            var newDateRule = cmbUiDateRule.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(newDateRule) || newDateRule == "None")
            {
                fv.Ui.DateRuleType = null;
                fv.Ui.DateRuleMessage = null;
                SafeSetText(txtUiDateRuleMsg, "");

            }
            else if (fv.Ui.DateRuleType != newDateRule)
            {
                fv.Ui.DateRuleType = newDateRule;
                fv.Ui.DateRuleMessage = null;
                SafeSetText(txtUiDateRuleMsg,"");
            }

            //fv.Ui.PatternType = chkUiPattern.Checked ? cmbUiPattern.Text : null;
            //fv.Ui.PatternMessage = chkUiPattern.Checked ? txtUiPatternMsg.Text : null;

            fv.Db.Required = chkDbRequired.Checked
                ? new RequiredRule { Enabled = true, Message = txtDbRequiredMsg.Text }
                : null;

            //fv.Db.MinLength = chkDbMinLen.Checked
            //    ? new LengthRule { Enabled = true, Value = (int)numDbMinLen.Value, Message = txtDbMinLenMsg.Text }
            //    : null;

            //fv.Db.MaxLength = chkDbMaxLen.Checked
            //    ? new LengthRule { Enabled = true, Value = (int)numDbMaxLen.Value, Message = txtDbMaxLenMsg.Text }
            //    : null;

            fv.Db.Unique = chkDbUnique.Checked
                ? new UniqueRule { Enabled = true, ColumnsCsv = txtDbUniqueCols.Text, Message = txtDbUniqueMsg.Text }
                : null;
            EnsureDefaultMessages(field, fv);
            // ✅ UI SYNC — Date Rule message
            if (string.IsNullOrWhiteSpace(txtUiDateRuleMsg.Text))
            {
                SafeSetText(txtUiDateRuleMsg,fv.Ui.DateRuleMessage ?? "");
            }

            // Sync UI with defaults that may have been generated
            if (fv.Ui.MinLength != null && string.IsNullOrWhiteSpace(txtUiMinLenMsg.Text))
                SafeSetText(txtUiMinLenMsg,fv.Ui.MinLength.Message ?? "");

            if (fv.Ui.MaxLength != null && string.IsNullOrWhiteSpace(txtUiMaxLenMsg.Text))
                SafeSetText(txtUiMaxLenMsg,fv.Ui.MaxLength.Message ?? "");

            if (!string.IsNullOrWhiteSpace(fv.Ui.PatternMessage) &&
                string.IsNullOrWhiteSpace(txtUiPatternMsg.Text))
            {
                SafeSetText(txtUiPatternMsg,fv.Ui.PatternMessage);
            }

            if (fv.Ui.Required != null && string.IsNullOrWhiteSpace(txtUiRequiredMsg.Text))
                SafeSetText(txtUiRequiredMsg, fv.Ui.Required.Message ?? "");

            if (fv.Db.Required != null && string.IsNullOrWhiteSpace(txtDbRequiredMsg.Text))
                SafeSetText(txtDbRequiredMsg,fv.Db.Required.Message ?? "");

            if (fv.Db.Unique != null && string.IsNullOrWhiteSpace(txtDbUniqueMsg.Text))
                SafeSetText(txtDbUniqueMsg,fv.Db.Unique.Message ?? "");

        }

        //private void SaveCurrent()
        //{
        //    if (lstFields.SelectedItem == null) return;
        //    var field = lstFields.SelectedItem.ToString();
        //    var fv = GetOrCreate(field);

        //    fv.Ui.Required = chkUiRequired.Checked ? new RequiredRule { Enabled = true, Message = txtUiRequiredMsg.Text } : null;
        //    fv.Ui.MinLength = chkUiMinLen.Checked ? new LengthRule { Enabled = true, Value = (int)numUiMinLen.Value, Message = txtUiMinLenMsg.Text } : null;
        //    fv.Ui.MaxLength = chkUiMaxLen.Checked ? new LengthRule { Enabled = true, Value = (int)numUiMaxLen.Value, Message = txtUiMaxLenMsg.Text } : null;

        //    fv.Ui.PatternType = chkUiPattern.Checked ? cmbUiPattern.Text : null;
        //    fv.Ui.PatternMessage = chkUiPattern.Checked ? txtUiPatternMsg.Text : null;

        //    fv.Db.Required = chkDbRequired.Checked ? new RequiredRule { Enabled = true, Message = txtDbRequiredMsg.Text } : null;
        //    fv.Db.MinLength = chkDbMinLen.Checked ? new LengthRule { Enabled = true, Value = (int)numDbMinLen.Value, Message = txtDbMinLenMsg.Text } : null;
        //    fv.Db.MaxLength = chkDbMaxLen.Checked ? new LengthRule { Enabled = true, Value = (int)numDbMaxLen.Value, Message = txtDbMaxLenMsg.Text } : null;

        //    fv.Db.Unique = chkDbUnique.Checked
        //        ? new UniqueRule { Enabled = true, ColumnsCsv = txtDbUniqueCols.Text, Message = txtDbUniqueMsg.Text }
        //        : null;
        //}
        private void AutoFillDefaults(string field, FieldValidation fv)
        {
            var displayName = ResolveDisplayName(field);

            // ONLY handle DB Unique default message when enabled
            if (fv.Db.Unique != null &&
                string.IsNullOrWhiteSpace(fv.Db.Unique.Message))
            {
                fv.Db.Unique.Message = $"{displayName} already exists";
            }
        }


        //private void AutoFillDefaults(string field, FieldValidation fv)
        //{
        //    var displayName = ResolveDisplayName(field);

        //    // UI - Required
        //    if (fv.Ui.Required == null)
        //    {
        //        fv.Ui.Required = new RequiredRule
        //        {
        //            Enabled = false,
        //            Message = $"{displayName} is required"
        //        };
        //    }
        //    else if (string.IsNullOrWhiteSpace(fv.Ui.Required.Message))
        //    {
        //        fv.Ui.Required.Message = $"{displayName} is required";
        //    }

        //    // UI - Min Length
        //    if (fv.Ui.MinLength == null)
        //    {
        //        fv.Ui.MinLength = new LengthRule
        //        {
        //            Enabled = false,
        //            Value = 1,
        //            Message = $"{displayName} must have at least 1 character"
        //        };
        //    }
        //    else if (string.IsNullOrWhiteSpace(fv.Ui.MinLength.Message))
        //    {
        //        fv.Ui.MinLength.Message = $"{displayName} must have at least 1 character";
        //    }

        //    // DB - Unique (default only if empty)
        //    if (fv.Db.Unique != null &&
        //        string.IsNullOrWhiteSpace(fv.Db.Unique.Message))
        //    {
        //        fv.Db.Unique.Message = $"{displayName} already exists";
        //    }
        //    // DB - Required
        //    if (fv.Db.Required == null)
        //    {
        //        fv.Db.Required = new RequiredRule
        //        {
        //            Enabled = false,
        //            Message = $"{displayName} is required"
        //        };
        //    }
        //    else if (string.IsNullOrWhiteSpace(fv.Db.Required.Message))
        //    {
        //        fv.Db.Required.Message = $"{displayName} is required";
        //    }

        //}




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
    public class UiValidation
    {
        public RequiredRule Required { get; set; }
        public LengthRule MinLength { get; set; }
        public LengthRule MaxLength { get; set; }
        public AllowedValuesRule AllowedValues { get; set; }

        public string PatternType { get; set; }
        public string PatternMessage { get; set; }

        // ✅ ADD THIS
        public string LastPatternType { get; set; }
    }
}
