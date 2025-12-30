using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaritorCodeFoundry.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class PreviewDialog : Form
    {
        private TabControl tabControl;
        private TabPage tabViewSql;
        private TabPage tabViewModel;

        private TextBox txtViewSql;
        private TextBox txtViewModel;
        private TabPage tabMetaData;
        private TextBox txtMetaData;


        private Button btnCopy;
        private Button btnClose;

        public PreviewDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Preview";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 950;
            this.Height = 650;
            this.MinimizeBox = false;
            this.MaximizeBox = true;

            // -----------------------------
            // TabControl
            // -----------------------------
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            tabViewSql = new TabPage("View SQL");
            tabViewModel = new TabPage("ViewModel");
            tabMetaData = new TabPage("MetaData");

            // -----------------------------
            // View SQL TextBox
            // -----------------------------
            txtViewSql = CreateCodeTextBox();
            tabViewSql.Controls.Add(txtViewSql);

            // -----------------------------
            // ViewModel TextBox
            // -----------------------------
            txtViewModel = CreateCodeTextBox();
            tabViewModel.Controls.Add(txtViewModel);
            txtMetaData = CreateCodeTextBox();
            tabMetaData.Controls.Add(txtMetaData);
            tabControl.TabPages.Add(tabViewSql);
            tabControl.TabPages.Add(tabViewModel);
            tabControl.TabPages.Add(tabMetaData);

            // -----------------------------
            // Bottom Panel
            // -----------------------------
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45
            };

            btnCopy = new Button
            {
                Text = "Copy",
                Width = 90,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };

            btnClose = new Button
            {
                Text = "Close",
                Width = 90,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };

            btnClose.Click += (s, e) => this.Close();
            btnCopy.Click += BtnCopy_Click;

            // Position buttons
            btnClose.Location = new Point(this.ClientSize.Width - 110, 8);
            btnCopy.Location = new Point(this.ClientSize.Width - 210, 8);

            bottomPanel.Controls.Add(btnClose);
            bottomPanel.Controls.Add(btnCopy);
            bottomPanel.BringToFront();
            tabControl.Dock = DockStyle.Fill;
            bottomPanel.Dock = DockStyle.Bottom;

            // Keep buttons aligned on resize
            this.Resize += (s, e) =>
            {
                btnClose.Location = new Point(this.ClientSize.Width - 110, 8);
                btnCopy.Location = new Point(this.ClientSize.Width - 210, 8);
            };

            // -----------------------------
            // Add Controls
            // -----------------------------
            this.Controls.Add(bottomPanel);
            this.Controls.Add(tabControl);
        }

        private TextBox CreateCodeTextBox()
        {
            return new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 10),
                BackColor = Color.White
            };
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabViewSql)
            {
                Clipboard.SetText(txtViewSql.Text);
            }
            else if (tabControl.SelectedTab == tabViewModel)
            {
                Clipboard.SetText(txtViewModel.Text);
            }
            else if (tabControl.SelectedTab == tabMetaData)
            {
                Clipboard.SetText(txtMetaData.Text);
            }

        }

        // ---------------------------------
        // TEMP setters (for next step)
        // ---------------------------------
        public void SetViewSql(string sql)
        {
            txtViewSql.Text = sql ?? string.Empty;
        }

        public void SetViewModel(string code)
        {
            txtViewModel.Text = code ?? string.Empty;
        }
        public void SetMetaData(string code)
        {
            txtMetaData.Text = code ?? string.Empty;
        }

    }


}
