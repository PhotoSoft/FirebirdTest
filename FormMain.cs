using System;
using System.Windows.Forms;

namespace FirebirdTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                Enabled = false;

                new FirebirdClient(checkDelete.Checked).UpdateTest();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Enabled = true;
                checkDelete.Checked = false;
            }
        }
    }
}