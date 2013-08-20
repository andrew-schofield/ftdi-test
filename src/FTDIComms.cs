// -----------------------------------------------------------------------
// <copyright file="FTDI.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace FTDI_Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using FTD2XX_NET;
    
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class FTDIComms
    {
        private UInt32 ftdiDeviceCount = 0;
        private FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

        private FTDI myFtdiDevice;

        public int DeviceCount
        {
            get { return Convert.ToInt32(ftdiDeviceCount); }
        }

        public FTDIComms()
        {
            // Create new instance of the FTDI device class
            FTDI myFtdiDevice = new FTDI();

            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);

            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Number of FTDI devices: " + ftdiDeviceCount.ToString());
                Console.WriteLine("");
            }
            else
            {
                // Wait for a key press
                Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                return;
            }

            // If no devices available, return
            if (ftdiDeviceCount == 0)
            {
                // Wait for a key press
                Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                return;
            }
        }

        public List<FTDI.FT_DEVICE_INFO_NODE> GetDeviceList()
        {
            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

            // Populate our device list
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                for (UInt32 i = 0; i < ftdiDeviceCount; i++)
                {
                    Console.WriteLine("Device Index: " + i.ToString());
                    Console.WriteLine("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                    Console.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                    Console.WriteLine("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                    Console.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                    Console.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                    Console.WriteLine("Description: " + ftdiDeviceList[i].Description.ToString());
                    Console.WriteLine("");
                }
            }
            return ftdiDeviceList.ToList();
        }

        public void OpenDevice(string serialNumber)
        {
            // Open first device in our list by serial number
            ftStatus = myFtdiDevice.OpenBySerialNumber(serialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to open device (error " + ftStatus.ToString() + ")");
                return;
            }
        }

        public void SetupDevice()
        {
            // Set up device data parameters
            // Set Baud rate to 9600
            ftStatus = myFtdiDevice.SetBaudRate(9600);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set Baud rate (error " + ftStatus.ToString() + ")");
                return;
            }

            // Set data characteristics - Data bits, Stop bits, Parity
            ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set data characteristics (error " + ftStatus.ToString() + ")");
                return;
            }

            // Set flow control - set RTS/CTS flow control
            ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x11, 0x13);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set flow control (error " + ftStatus.ToString() + ")");
                return;
            }

            // Set read timeout to 5 seconds, write timeout to infinite
            ftStatus = myFtdiDevice.SetTimeouts(5000, 0);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set timeouts (error " + ftStatus.ToString() + ")");
                return;
            }
        }

        public void WriteData(string data)
        {
            // Perform loop back - make sure loop back connector is fitted to the device
            // Write string data to the device
            UInt32 numBytesWritten = 0;
            // Note that the Write method is overloaded, so can write string or byte array data
            ftStatus = myFtdiDevice.Write(data, data.Length, ref numBytesWritten);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to write to device (error " + ftStatus.ToString() + ")");
                return;
            }
        }

        public string ReadData()
        {
            // Check the amount of data available to read
            // In this case we know how much data we are expecting, 
            // so wait until we have all of the bytes we have sent.
            UInt32 numBytesAvailable = 0;
            do
            {
                ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    // Wait for a key press
                    Console.WriteLine("Failed to get number of bytes available to read (error " + ftStatus.ToString() + ")");
                    return string.Empty;
                }
                Thread.Sleep(10);
            } while (numBytesAvailable < 32); //32byte data packet?

            // Now that we have the amount of data we want available, read it
            string readData;
            UInt32 numBytesRead = 0;
            // Note that the Read method is overloaded, so can read string or byte array data
            ftStatus = myFtdiDevice.Read(out readData, numBytesAvailable, ref numBytesRead);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to read data (error " + ftStatus.ToString() + ")");
                return string.Empty;
            }
            Console.WriteLine(readData);
            return readData;
        }

        public void Close()
        {
            // Close our device
            ftStatus = myFtdiDevice.Close();
        }
    }
}
