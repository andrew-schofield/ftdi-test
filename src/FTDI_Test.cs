using System;
using System.Windows.Forms;
namespace FTDI_Test
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    using FTD2XX_NET;

    public partial class FTDI_Test : Form
    {
        private List<FTDI.FT_DEVICE_INFO_NODE> ftdiDeviceList;
        FTDIComms transponders;
        protected Thread pThreadRead;

        private delegate void SetTextCallback(string SetText);
     
        public FTDI_Test()
        {
            InitializeComponent();
        }

        private void connectButtonClick(object sender, EventArgs e)
        {
            transponders = new FTDIComms();
            if (transponders.DeviceCount > 0)
            {
                StartButton.Enabled = true;
                ftdiDeviceList = transponders.GetDeviceList();
                ConnectButton.Enabled = false;
            }
            else
            {
                MessageBox.Show("No FTDI devices found");
            }
        }

        private void readButtonClick(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            StringBuilder serialNumbers = new StringBuilder();
            serialNumbers.AppendLine("Device serial numbers");
            foreach (var device in ftdiDeviceList)
            {
                serialNumbers.AppendLine(device.SerialNumber);
            }

            MessageBox.Show(serialNumbers.ToString(), "FTDI Devices found");

            transponders.OpenDeviceBySerialNumber(ftdiDeviceList[0].SerialNumber);

            pThreadRead = new Thread(new ThreadStart(ReadThread));
            pThreadRead.Start();
        }

        private void ReadThread()
        {
            while (true)
            {
                this.SetText(transponders.ReadData());
            }
        }

        private void StopButtonClick(object sender, EventArgs e)
        {
            pThreadRead.Abort();
            StartButton.Enabled = true;
            StopButton.Enabled = false;
            transponders.Close();
        }

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.DebugOutput.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.DebugOutput.AppendText(text);
                //this.DebugOutput.ScrollToCaret();
            }
        }
    }
}
