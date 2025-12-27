using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using CodeFoundry.Generator.Tools;
using CodeFoundry.Generator.Generators;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.UI;

namespace CaritorCodeFoundry
{
    public partial class MainForm : Form
    {
        private string _connectionString;

        // Selection per physical table
        private readonly Dictionary<string, SelectionDto> _selections =
            new Dictionary<string, SelectionDto>(StringComparer.OrdinalIgnoreCase);

        private TableSchemaDto _currentSchema;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Status: Ready";
            txtPreview.Clear();

            var cs = ConfigurationManager.ConnectionStrings["CodeFoundryDb"];
            if (cs != null)
                txtConnection.Text = cs.ConnectionString;

            // Wire PascalName change → SelectionDto
            txtPascalName.TextChanged += TxtPascalName_TextChanged;
        }

        // ---------------- CONNECT ----------------

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _connectionString = txtConnection.Text?.Trim();
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    MessageBox.Show("Enter a connection string.");
                    return;
                }

                lblStatus.Text = "Status: Reading tables...";
                Application.DoEvents();

                var tables = SchemaReader.GetTables(_connectionString);
                lstTables.Items.Clear();
                tables.ForEach(t => lstTables.Items.Add(t));

                lblStatus.Text = $"Status: Connected ({tables.Count} tables)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connect failed");
                lblStatus.Text = "Status: Error";
            }
        }

        // ---------------- TABLE SELECT ----------------

        private void lstTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstTables.SelectedItem == null) return;

            try
            {
                var table = lstTables.SelectedItem.ToString();
                lblStatus.Text = $"Status: Loading schema for {table}";
                Application.DoEvents();

                _currentSchema = SchemaReader.GetTableSchema(_connectionString, table);

                // Ensure SelectionDto exists EARLY
                if (!_selections.TryGetValue(table, out var selection))
                {
                    selection = new SelectionDto();
                    _selections[table] = selection;
                }

                // Resolve PascalName
                if (string.IsNullOrWhiteSpace(selection.LogicalTableName))
                {
                    selection.LogicalTableName = ToPascalCase(table);
                }

                // Populate UI
                txtPascalName.Text = selection.LogicalTableName;

                var sb = new StringBuilder();
                sb.AppendLine($"Table: {_currentSchema.TableName}");
                sb.AppendLine($"Logical Name: {selection.LogicalTableName}");
                sb.AppendLine(new string('-', 60));

                foreach (var c in _currentSchema.Columns)
                {
                    var fk = _currentSchema.ForeignKeys?
                        .FirstOrDefault(f =>
                            string.Equals(f.ColumnName, c.ColumnName, StringComparison.OrdinalIgnoreCase));

                    var fkText = fk != null
                        ? $"  FK→ {fk.ReferencedTable}({fk.ReferencedColumn})"
                        : string.Empty;

                    sb.AppendLine(
                        $"{c.ColumnName,-30} {c.DataType}" +
                        $"{(c.MaxLength.HasValue ? $" ({c.MaxLength})" : "")} " +
                        $"PK:{c.IsPrimaryKey}{fkText}"
                    );
                }

                txtPreview.Text = sb.ToString();
                lblStatus.Text = $"Status: Schema loaded ({_currentSchema.Columns.Count} columns)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Schema error");
                lblStatus.Text = "Status: Error";
            }
        }

        // ---------------- CONFIGURE FIELDS ----------------

        private void btnConfigureFields_Click(object sender, EventArgs e)
        {
            if (_currentSchema == null)
            {
                MessageBox.Show("Select a table first.");
                return;
            }

            var selection = _selections[_currentSchema.TableName];

            using (var dlg = new ConfigureFieldsFormAdvanced())
            {
                dlg.Initialize(_currentSchema, selection, _connectionString);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _selections[_currentSchema.TableName] = dlg.GetSelection();
                    lblStatus.Text = $"Status: Fields configured for {_currentSchema.TableName}";
                }
            }
        }

        // ---------------- VIEWMODEL PREVIEW ----------------

        private void btnGenerateViewModel_Click(object sender, EventArgs e)
        {
            if (!TryGetSelection(out var sel)) return;

            const string gridType = "EditGrid";

            var fkMap = sel.GetFkSelections(gridType)
                           .ToDictionary(
                               k => k.Key,
                               v => (IEnumerable<string>)v.Value,
                               StringComparer.OrdinalIgnoreCase
                           );

            var files = ViewModelGenerator.GenerateViewModelFiles(
                _currentSchema,
                gridType,
                sel.GetSelectedColumns(gridType),
                fkMap
            );

            txtPreview.Text = files.Values.FirstOrDefault() ?? "";
            lblStatus.Text = "Status: ViewModel generated (preview)";
        }

        // ---------------- SP PREVIEW ----------------

        private void btnGenerateSP_Click(object sender, EventArgs e)
        {
            if (_currentSchema == null)
            {
                MessageBox.Show("Select a table first.");
                return;
            }

            var files = StoredProcGenerator.GenerateSpFiles(_currentSchema);
            txtPreview.Text = files.Values.FirstOrDefault() ?? "";
            lblStatus.Text = "Status: Stored Procedures generated (preview)";
        }

        // ---------------- FULL PACKAGE ----------------

        private void btnGeneratePackage_Click(object sender, EventArgs e)
        {
            if (!TryGetSelection(out var sel)) return;

            using (var dlg = new FolderBrowserDialog
            {
                Description = "Select base output folder"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var output = GeneratorOrchestrator.GenerateFullPackage(
                    _connectionString,
                    _currentSchema.TableName,
                    dlg.SelectedPath,
                    sel,
                    includeEntity: false
                );

                txtPreview.Text = $"Package created:\r\n{output}";
                lblStatus.Text = "Status: Package generated";
            }
        }

        // ---------------- HELPERS ----------------

        private bool TryGetSelection(out SelectionDto selection)
        {
            selection = null;

            if (_currentSchema == null)
            {
                MessageBox.Show("Select a table first.");
                return false;
            }

            if (!_selections.TryGetValue(_currentSchema.TableName, out selection))
            {
                MessageBox.Show("Configure fields first.");
                return false;
            }

            return true;
        }

        private void TxtPascalName_TextChanged(object sender, EventArgs e)
        {
            if (_currentSchema == null) return;

            if (_selections.TryGetValue(_currentSchema.TableName, out var sel))
            {
                sel.LogicalTableName = txtPascalName.Text?.Trim();
            }
        }

        private static string ToPascalCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            var parts = name
                .Replace("-", "_")
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();
            foreach (var p in parts)
            {
                var part = p.ToLowerInvariant();
                sb.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                    sb.Append(part.Substring(1));
            }
            return sb.ToString();
        }

        private void txtPreview_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
