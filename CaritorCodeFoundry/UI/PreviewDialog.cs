using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaritorCodeFoundry.UI
{
    public partial class PreviewDialog : Form
    {
        public PreviewDialog()
        {
            InitializeComponent();
            this.Text = "Preview";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 900;
            this.Height = 600;
        }

        private void InitializeComponent()
        {
            //throw new NotImplementedException();
        }
    }

}
