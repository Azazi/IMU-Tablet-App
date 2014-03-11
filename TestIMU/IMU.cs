using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace TestIMU
{
    public class IMU
    {
        SerialPort port;

        int count = 0;

        string bufferString = "";

        public delegate void DataRecieved(int accelX, int accelY, int accelZ, int gyroX, int gyroY, int gyroZ);
        public event DataRecieved OnDataRecieved;

        public IMU()
        {
            port = new SerialPort("COM5", 115200, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        System.Timers.Timer timer = new System.Timers.Timer(1000);
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public void Start()
        {
            port.Open();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
            watch.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine(count / (watch.ElapsedMilliseconds / 1000) + " fps");
            watch.Restart();
            count = 0;
        }

        public void Stop()
        {
            port.Close();
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Read data from serial port
            string data = port.ReadExisting();

            // Append data to anything that may not have been completely read last reading
            bufferString += data;

            // Split the string on the newline
            string[] split = bufferString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // If the last character is not a newline, then the last line has not been read complete so we ignore it this round
            // and try again next reading.
            if (bufferString.ToCharArray().Last() != '\n')
            {
                bufferString = split.Last();
            }
            else
            {
                bufferString = split.Last() + "\n";
            }

            if (split.Length > 1)
            {
                // Process all of the lines we have, ignoring the last line because it might be incomplete, we'll process it next round.
                for (int i = 0; i < split.Length - 1; i++)
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
                                count++;
                            }
                            catch (Exception exception)
                            {
                                //One of the values of 6 could not be formatted into an int
                                Console.WriteLine("Error Parsing line: " + line);
                                Console.WriteLine("Error Message: " + exception.Message);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Format Error: " + line);
                    }
                }
            }
        }
    }
}
