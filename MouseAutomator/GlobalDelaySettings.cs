using System;
using System.Windows.Forms;

namespace MouseAutomator
{
    public partial class GlobalDelaySettings : Form
    {
        private int globalDelay;
        public int GlobalDelay
        {
            get
            {
                return globalDelay;
            }
        }
        public GlobalDelaySettings(int currentGlobalDelay)
        {
            InitializeComponent();
            globalDelay = currentGlobalDelay;
            txtGlobalDelay.Text = currentGlobalDelay.ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtGlobalDelay.Text, out globalDelay))
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid delay time. Make sure time is a numerical value.", "Error", MessageBoxButtons.OKCancel);
            }
        }
    }
}
