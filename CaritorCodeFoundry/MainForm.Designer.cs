namespace CaritorCodeFoundry
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.txtConnection = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();

            this.grpTables = new System.Windows.Forms.GroupBox();
            this.lblPascalName = new System.Windows.Forms.Label();
            this.txtPascalName = new System.Windows.Forms.TextBox();
            this.lstTables = new System.Windows.Forms.ListBox();

            this.grpActions = new System.Windows.Forms.GroupBox();
            this.btnConfigureFields = new System.Windows.Forms.Button();
            this.btnGenerateCode = new System.Windows.Forms.Button();
            this.btnGenerateViewModel = new System.Windows.Forms.Button();
            this.btnGenerateSP = new System.Windows.Forms.Button();
            this.btnGeneratePackage = new System.Windows.Forms.Button();
            this.btnGenerateAll = new System.Windows.Forms.Button();

            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.txtPreview = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();

            // ================= LEFT PANEL =================
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(8);
            this.pnlLeft.Width = 420;
            this.pnlLeft.Controls.Add(this.grpTables);
            this.pnlLeft.Controls.Add(this.grpConnection);
            this.pnlLeft.Controls.Add(this.grpActions);

            // ================= CONNECTION =================
            this.grpConnection.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpConnection.Height = 130;
            this.grpConnection.Text = "Database Connection";

            this.txtConnection.Multiline = true;
            this.txtConnection.SetBounds(12, 28, 324, 86);

            this.btnConnect.Text = "Connect";
            this.btnConnect.SetBounds(342, 28, 52, 86);
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

            this.grpConnection.Controls.Add(this.txtConnection);
            this.grpConnection.Controls.Add(this.btnConnect);

            // ================= TABLES =================
            this.grpTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTables.Text = "Tables";

            this.lblPascalName.Text = "Pascal Name";
            this.lblPascalName.SetBounds(12, 28, 100, 20);

            this.txtPascalName.SetBounds(12, 52, 380, 26);

            this.lstTables.SetBounds(12, 92, 380, 300);
            this.lstTables.Anchor = (
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right);
            this.lstTables.SelectedIndexChanged +=
                new System.EventHandler(this.lstTables_SelectedIndexChanged);

            this.grpTables.Controls.Add(this.lblPascalName);
            this.grpTables.Controls.Add(this.txtPascalName);
            this.grpTables.Controls.Add(this.lstTables);

            // ================= ACTIONS =================
            this.grpActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpActions.Height = 270;
            this.grpActions.Text = "Actions";

            int y = 25;
            int h = 38;
            int w = 380;

            this.btnConfigureFields.SetBounds(12, y, w, h);
            this.btnConfigureFields.Text = "Configure Fields";
            this.btnConfigureFields.Click +=
                new System.EventHandler(this.btnConfigureFields_Click);

            y += 42;
            this.btnGenerateCode.SetBounds(12, y, w, h);
            this.btnGenerateCode.Text = "Generate Code (legacy)";

            y += 42;
            this.btnGenerateViewModel.SetBounds(12, y, w, h);
            this.btnGenerateViewModel.Text = "Generate ViewModel";
            this.btnGenerateViewModel.Click +=
                new System.EventHandler(this.btnGenerateViewModel_Click);

            y += 42;
            this.btnGenerateSP.SetBounds(12, y, w, h);
            this.btnGenerateSP.Text = "Generate SPs";
            this.btnGenerateSP.Click +=
                new System.EventHandler(this.btnGenerateSP_Click);

            y += 42;
            this.btnGeneratePackage.SetBounds(12, y, w, h);
            this.btnGeneratePackage.Text = "Generate Package";
            this.btnGeneratePackage.Click +=
                new System.EventHandler(this.btnGeneratePackage_Click);

            y += 42;
            this.btnGenerateAll.SetBounds(12, y, w, h);
            this.btnGenerateAll.Text = "Generate All";

            this.grpActions.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.btnConfigureFields,
                this.btnGenerateCode,
                this.btnGenerateViewModel,
                this.btnGenerateSP,
                this.btnGeneratePackage,
                this.btnGenerateAll
            });

            // ================= PREVIEW =================
            this.grpPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPreview.Text = "Preview / Output";

            this.txtPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPreview.Multiline = true;
            this.txtPreview.ReadOnly = true;
            this.txtPreview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtPreview.WordWrap = false;

            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Height = 28;
            this.lblStatus.Text = "Status: Ready";

            this.grpPreview.Controls.Add(this.txtPreview);
            this.grpPreview.Controls.Add(this.lblStatus);

            // ================= FORM =================
            this.ClientSize = new System.Drawing.Size(1580, 821);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.pnlLeft);
            this.Text = "CodeFoundry - Generator";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
        }

        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.TextBox txtConnection;
        private System.Windows.Forms.Button btnConnect;

        private System.Windows.Forms.GroupBox grpTables;
        private System.Windows.Forms.Label lblPascalName;
        private System.Windows.Forms.TextBox txtPascalName;
        private System.Windows.Forms.ListBox lstTables;

        private System.Windows.Forms.GroupBox grpActions;
        private System.Windows.Forms.Button btnConfigureFields;
        private System.Windows.Forms.Button btnGenerateCode;
        private System.Windows.Forms.Button btnGenerateViewModel;
        private System.Windows.Forms.Button btnGenerateSP;
        private System.Windows.Forms.Button btnGeneratePackage;
        private System.Windows.Forms.Button btnGenerateAll;

        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.TextBox txtPreview;
        private System.Windows.Forms.Label lblStatus;
    }
}
