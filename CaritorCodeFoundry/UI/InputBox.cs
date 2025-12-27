using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodeFoundry.Generator.UI
{
    public static class InputBox
    {
        public static string Show(string prompt, string title = "Input", string defaultValue = "")
        {
            using (var form = new Form())
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;
                form.Text = title;
                form.ClientSize = new Size(420, 140);

                var lbl = new Label { Left = 12, Top = 12, AutoSize = true, Text = prompt };
                var txt = new TextBox { Left = 12, Top = 38, Width = 395, Text = defaultValue };

                var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 245, Width = 75, Top = 80 };
                var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 330, Width = 75, Top = 80 };

                form.Controls.Add(lbl);
                form.Controls.Add(txt);
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);

                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                return form.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }
    }
}
