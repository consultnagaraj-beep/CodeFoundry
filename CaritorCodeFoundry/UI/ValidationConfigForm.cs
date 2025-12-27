using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeFoundry.Generator.Models;

namespace CodeFoundry.Generator.UI
{
    public class ValidationConfigForm : Form
    {
        private readonly SelectionDto _selection;

        private ListBox lstFields;

        // ---------- UI ----------
        private CheckBox chkUiRequired;
        private TextBox txtUiRequiredMsg;

        private CheckBox chkUiMinLen;
        private NumericUpDown numUiMinLen;
        private TextBox txtUiMinLenMsg;

        private CheckBox chkUiMaxLen;
        private NumericUpDown numUiMaxLen;
        private TextBox txtUiMaxLenMsg;

        // ---------- DB ----------
        private CheckBox chkDbRequired;
        private TextBox txtDbRequiredMsg;

        private CheckBox chkDbMinLen;
        private NumericUpDown numDbMinLen;
        private TextBox txtDbMinLenMsg;

        private CheckBox chkDbMaxLen;
        private NumericUpDown numDbMaxLen;
        private TextBox txtDbMaxLenMsg;

        private CheckBox chkDbUnique;
        private TextBox txtDbUniqueCols;
        private TextBox txtDbUniqueMsg;

        private Button btnClose;

        public ValidationConfigForm(SelectionDto selection)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            BuildUI();
            LoadFields();
        }

        // =====================================================
        // UI
        // =====================================================
        private void BuildUI()
        {
            Text = "Configure Validations (Normal)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1150, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            lstFields = new ListBox
            {
                Left = 10,
                Top = 10,
                Width = 220,
                Height = 440
            };
            lstFields.SelectedIndexChanged += (_, __) => LoadFieldValidation();

            // ---------- UI group ----------
            var grpUi = new GroupBox
            {
                Text = "UI Validation",
                Left = 240,
                Top = 10,
                Width = 420,
                Height = 440
            };

            int y = 30;

            chkUiRequired = new CheckBox { Text = "Required", Left = 10, Top = y };
            txtUiRequiredMsg = new TextBox { Left = 160, Top = y, Width = 240 };
            y += 35;

            chkUiMinLen = new CheckBox { Text = "Min Length", Left = 10, Top = y };
            numUiMinLen = new NumericUpDown { Left = 160, Top = y, Width = 60, Minimum = 1, Maximum = 9999 };
            txtUiMinLenMsg = new TextBox { Left = 230, Top = y, Width = 170 };
            y += 35;

            chkUiMaxLen = new CheckBox { Text = "Max Length", Left = 10, Top = y };
            numUiMaxLen = new NumericUpDown { Left = 160, Top = y, Width = 60, Minimum = 0, Maximum = 9999 };
            txtUiMaxLenMsg = new TextBox { Left = 230, Top = y, Width = 170 };

            grpUi.Controls.AddRange(new Control[]
            {
                chkUiRequired, txtUiRequiredMsg,
                chkUiMinLen, numUiMinLen, txtUiMinLenMsg,
                chkUiMaxLen, numUiMaxLen, txtUiMaxLenMsg
            });

            // ---------- DB group ----------
            var grpDb = new GroupBox
            {
                Text = "DB Validation",
                Left = 680,
                Top = 10,
                Width = 420,
                Height = 440
            };

            y = 30;

            chkDbRequired = new CheckBox { Text = "Required", Left = 10, Top = y };
            txtDbRequiredMsg = new TextBox { Left = 160, Top = y, Width = 240 };
            y += 35;

            chkDbMinLen = new CheckBox { Text = "Min Length", Left = 10, Top = y };
            numDbMinLen = new NumericUpDown { Left = 160, Top = y, Width = 60, Minimum = 1, Maximum = 9999 };
            txtDbMinLenMsg = new TextBox { Left = 230, Top = y, Width = 170 };
            y += 35;

            chkDbMaxLen = new CheckBox { Text = "Max Length", Left = 10, Top = y };
            numDbMaxLen = new NumericUpDown { Left = 160, Top = y, Width = 60, Minimum = 0, Maximum = 9999 };
            txtDbMaxLenMsg = new TextBox { Left = 230, Top = y, Width = 170 };
            y += 35;

            chkDbUnique = new CheckBox { Text = "Unique (Duplicate Check)", Left = 10, Top = y };
            txtDbUniqueCols = new TextBox { Left = 160, Top = y, Width = 120 };
            txtDbUniqueMsg = new TextBox { Left = 290, Top = y, Width = 110 };

            grpDb.Controls.AddRange(new Control[]
            {
                chkDbRequired, txtDbRequiredMsg,
                chkDbMinLen, numDbMinLen, txtDbMinLenMsg,
                chkDbMaxLen, numDbMaxLen, txtDbMaxLenMsg,
                chkDbUnique, txtDbUniqueCols, txtDbUniqueMsg
            });

            btnClose = new Button
            {
                Text = "Close",
                Width = 120,
                Left = 980,
                Top = 460
            };
            btnClose.Click += (_, __) => Close();

            Controls.AddRange(new Control[]
            {
                lstFields, grpUi, grpDb, btnClose
            });

            WireSaveHandlers();
        }

        private void WireSaveHandlers()
        {
            foreach (var chk in Controls.OfType<GroupBox>()
                     .SelectMany(g => g.Controls.OfType<CheckBox>()))
                chk.CheckedChanged += (_, __) => SaveCurrent();

            foreach (var num in Controls.OfType<GroupBox>()
                     .SelectMany(g => g.Controls.OfType<NumericUpDown>()))
                num.ValueChanged += (_, __) => SaveCurrent();

            foreach (var txt in Controls.OfType<GroupBox>()
                     .SelectMany(g => g.Controls.OfType<TextBox>()))
                txt.TextChanged += (_, __) => SaveCurrent();
        }


        // =====================================================
        // DATA
        // =====================================================
        private void LoadFields()
        {
            lstFields.Items.Clear();

            foreach (var f in _selection.Validation.Fields.Keys.OrderBy(x => x))
                lstFields.Items.Add(f);

            if (lstFields.Items.Count > 0)
                lstFields.SelectedIndex = 0;
        }

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

            fv.Ui.Required = chkUiRequired.Checked
                ? new RequiredRule { Enabled = true, Message = txtUiRequiredMsg.Text }
                : null;

            fv.Ui.MinLength = chkUiMinLen.Checked
                ? new LengthRule { Enabled = true, Value = (int)numUiMinLen.Value, Message = txtUiMinLenMsg.Text }
                : null;

            fv.Ui.MaxLength = chkUiMaxLen.Checked
                ? new LengthRule { Enabled = true, Value = (int)numUiMaxLen.Value, Message = txtUiMaxLenMsg.Text }
                : null;

            fv.Db.Required = chkDbRequired.Checked
                ? new RequiredRule { Enabled = true, Message = txtDbRequiredMsg.Text }
                : null;

            fv.Db.MinLength = chkDbMinLen.Checked
                ? new LengthRule { Enabled = true, Value = (int)numDbMinLen.Value, Message = txtDbMinLenMsg.Text }
                : null;

            fv.Db.MaxLength = chkDbMaxLen.Checked
                ? new LengthRule { Enabled = true, Value = (int)numDbMaxLen.Value, Message = txtDbMaxLenMsg.Text }
                : null;

            fv.Db.Unique = chkDbUnique.Checked
                ? new UniqueRule
                {
                    Enabled = true,
                    ColumnsCsv = txtDbUniqueCols.Text,
                    Message = txtDbUniqueMsg.Text
                }
                : null;
        }

        private void AutoFillDefaults(string field, FieldValidation fv)
        {
            if (fv.Ui.Required != null) return;

            fv.Ui.Required = new RequiredRule { Enabled = false, Message = $"{field} is required" };
            fv.Db.Required = new RequiredRule { Enabled = false, Message = $"{field} is required" };

            fv.Ui.MinLength = new LengthRule { Enabled = false, Value = 1, Message = $"{field} must have at least 1 character" };
            fv.Db.MinLength = new LengthRule { Enabled = false, Value = 1, Message = $"{field} must have at least 1 character" };
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
