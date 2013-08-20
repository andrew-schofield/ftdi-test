using System;
using System.Windows.Forms;
namespace FTDI_Test
{
    public partial class FTDI_Test : Form
    {
        public FTDI_Test()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FTDIComms transponders = new FTDIComms();

            if (transponders.DeviceCount > 0)
            {
                transponders.GetDeviceList();
            }
            else
            {
                MessageBox.Show("No FTDI devices found");
            }
        }
    }
}
