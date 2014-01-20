using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace TestIMU
{
    public class EMU
    {
        SerialPort port;

        int count = 0;

        string bufferString = "";

        public delegate void DataRecieved(int accelX, int accelY, int accelZ, int gyroX, int gyroY, int gyroZ);
        public event DataRecieved OnDataRecieved;

        public EMU()
        {
            port = new SerialPort("COM7", 115200, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        public void Start()
        {
            port.Open();
        }

        public void Stop()
        {
            port.Close();
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = port.ReadExisting();
            bufferString += data;

            //Console.WriteLine(bufferString);

            string[] split = bufferString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            bufferString = "";

            for (int i = 0; i < split.Length; i++)
            {
                string line = split[i];
                string[] split2 = line.Split(',');

                if (split2.Length == 6)
                {
                    if (OnDataRecieved != null)
                    {
                        try
                        {
                            OnDataRecieved(Convert.ToInt32(split2[0]), Convert.ToInt32(split2[1]), Convert.ToInt32(split2[2]), Convert.ToInt32(split2[3]), Convert.ToInt32(split2[4]), Convert.ToInt32(split2[5]));
                        }
                        catch (Exception exception)
                        {
                            //One of the values of 6 could not be formatted into an int
                            int a = 0;
                            //Console.WriteLine(count++ + "\t" + line);
                        }
                    }
                }
                else
                {
                    //This line did not contain 6 entries
                    if (i == (split.Length - 1))
                    {
                        // Adds the last line to the next buffer, since next data collection time will contain the rest of the data.
                        // Does not take into consideration if the last entry is only '-'
                        bufferString = line;
                    }
                }
            }
        }
    }
}
