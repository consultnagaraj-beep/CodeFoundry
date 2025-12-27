using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CodeFoundry.Generator.UI
{
    /// <summary>
    /// Simple dialog to get a valid project name and preview project folder.
    /// Validation happens only when the user clicks Create.
    /// </summary>
    public class ProjectNameDialog : Form
    {
        private Label lblPrompt;
        private TextBox txtName;
        private Label lblPreview;
        private Button btnCreate;
        private Button btnCancel;
        private Label lblError;

        public string ProjectName => txtName.Text?.Trim();

        public ProjectNameDialog()
        {
            InitializeComponents();
            // Do NOT validate here. Validation runs only on Create click.
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Show initial folder preview but do not validate.
            //UpdatePreview();
            lblError.Text = "";
            // Enable Create only if there is some non-empty text to start with.
            btnCreate.Enabled = !string.IsNullOrWhiteSpace(txtName.Text);
        }

        private void InitializeComponents()
        {
            this.Text = "Create New Project";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(520, 160);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            lblPrompt = new Label { Left = 12, Top = 12, AutoSize = true, Text = "Project name:" };
            txtName = new TextBox { Left = 12, Top = 34, Width = 496 };
            // Only update preview & enable Create on text change — do NOT validate here.
            txtName.TextChanged += (s, e) =>
            {
                UpdatePreview();
                btnCreate.Enabled = !string.IsNullOrWhiteSpace(txtName.Text);
                lblError.Text = "";
            };

            lblPreview = new Label { Left = 12, Top = 68, Width = 496, AutoSize = false };
            lblPreview.Height = 28;

            lblError = new Label { Left = 12, Top = 100, Width = 496, ForeColor = System.Drawing.Color.DarkRed, AutoSize = false };
            lblError.Height = 20;

            // Do NOT set DialogResult on Create button. We handle DialogResult in the click handler after validation.
            btnCreate = new Button { Text = "Create", Left = 338, Width = 80, Top = 120 };
            btnCancel = new Button { Text = "Cancel", Left = 426, Width = 80, Top = 120, DialogResult = DialogResult.Cancel };

            btnCreate.Click += BtnCreate_Click;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblPrompt, txtName, lblPreview, lblError, btnCreate, btnCancel });

            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            // Update preview (keeps UI consistent)
            UpdatePreview();

            // Perform validation now — only on Create click
            var err = ValidateName(txtName.Text);
            if (!string.IsNullOrEmpty(err))
            {
                lblError.Text = err;
                // keep dialog open so user can fix
                return;
            }

            // valid — set DialogResult and close
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void UpdatePreview()
        {
            var name = txtName.Text?.Trim();

            // Always use a safe base folder — fallback to application folder if MyDocuments is empty.
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrWhiteSpace(docs))
                docs = AppDomain.CurrentDomain.BaseDirectory;

            var baseDir = Path.Combine(docs, "CodeFoundryProjects");

            string previewFolder =
                string.IsNullOrWhiteSpace(name)
                ? Path.Combine(baseDir, "<ProjectName>")
                : Path.Combine(baseDir, name);

            lblPreview.Text = "Folder: " + previewFolder;
        }


        private string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Project name cannot be empty.";

            // disallow invalid filename chars
            var invalid = Path.GetInvalidFileNameChars();
            if (name.IndexOfAny(invalid) >= 0) return "Project name contains invalid characters.";

            // disallow names made only of dots or spaces
            if (Regex.IsMatch(name.Trim(), @"^[\. ]+$")) return "Project name is not valid.";

            // simple length guard
            if (name.Length > 100) return "Project name is too long.";

            // reserved names check (windows)
            var reserved = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "LPT1" };
            if (reserved.Contains(name.ToUpperInvariant())) return "Project name is reserved. Choose another.";

            return null;
        }
    }
}
