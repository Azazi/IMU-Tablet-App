using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

using IntAirAct;

namespace TestIMU
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IAIntAirAct intAirAct;
        IARoute sendRoute = IARoute.Post("/location");
        int sendCounter = 0;
        object counterLock = new object();

        IMU imu;

        const double SCALE_FACTOR = 15.75467;
        const double RAW_FACTOR = 2.972420635;
        const double TO_DEGREES = 0.0000875;
        const double TO_DPS = 0.00875;
        const double MINIMUM_GYRO_RATE_OF_CHANGE = 0.5;
        const int MINIMUM_ACCEL_RATE_OF_CHANGE = 139;
        const int TO_360 = 360;

        double rawTiltAngle = 0;
        double unscaledRawTiltAngle = 0;

        object positionLock = new object();
        object tiltLock = new object();
        object orientationLock = new object();

        Point currentPosition = new Point(0, 0);
        double tiltAngle = 0;
        double orientaion = 0;

        double previousAcceleration = 0;
        double previousVelocity = 0;
        double unscaledOrientation = 0;

        string output = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            intAirAct = IAIntAirAct.New();

            try
            {
                intAirAct.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            imu = new IMU();
            imu.OnDataRecieved += new IMU.DataRecieved(imu_OnDataRecieved);
            imu.Start();
        }

        void imu_OnDataRecieved(int accelX, int accelY, int accelZ, int gyroX, int gyroY, int gyroZ)
        {
            /// Computes the position of the device in the 2D space using the accelerometer. I account
            /// here for the noise of the signal, and accumelatively compute the displacement using the
            /// accelY measurements.
            if (accelY > MINIMUM_ACCEL_RATE_OF_CHANGE)
            {
                lock (positionLock)
                {
                    double currentAcceleration = accelY;
                    double currentVelocity = (previousAcceleration + currentAcceleration) * (0.05);
                    double distance = (previousVelocity + currentVelocity) * (0.05);

                    currentPosition.X = currentPosition.X + (distance * Math.Cos(orientaion * Math.PI / 180));
                    currentPosition.Y = currentPosition.Y + (distance * Math.Sin(orientaion * Math.PI / 180));
                }
            }

            /// Compute the acceleration vector and the angle it makes with the X-axis to be used 
            /// later to find the tilt angle.
            double R = Math.Sqrt(Math.Pow(accelX, 2) + Math.Pow(accelY, 2) + Math.Pow(accelZ, 2));
            double XR = Math.Acos(accelX / R) * 180 / Math.PI;

            /// Calculates the horizontal orientation of the device by using
            /// the gyroscope readings of the Z-axis. I account here for the
            /// Random Angle Walk factor -refer to paper details.
            if (Math.Abs(gyroZ * TO_DPS) >= MINIMUM_GYRO_RATE_OF_CHANGE)
            {
                lock (orientationLock)
                {
                    double newGyroZ = gyroZ + RAW_FACTOR;
                    unscaledOrientation = (newGyroZ * TO_DEGREES + unscaledOrientation);
                    if (unscaledOrientation < 0) { orientaion = (SCALE_FACTOR * unscaledOrientation) + TO_360; }
                    else { orientaion = (SCALE_FACTOR * unscaledOrientation); }
                    orientaion = orientaion % TO_360;
                }
            }

            /// I am just including this for comparison purposes, the acutal 
            /// yOrientaion aka the tilt angle will be computed using the Kalman
            /// filter as in below. Could be used to conduct comparison of filtered
            /// vs. raw tilt angle.
            if (Math.Abs(gyroY * TO_DPS) >= MINIMUM_GYRO_RATE_OF_CHANGE)
            {
                double newGyroY = gyroY + RAW_FACTOR;
                unscaledRawTiltAngle = (newGyroY * TO_DEGREES + unscaledRawTiltAngle);
                if (unscaledRawTiltAngle < 0) { rawTiltAngle = (SCALE_FACTOR * unscaledRawTiltAngle) + TO_360; }
                else { rawTiltAngle = (SCALE_FACTOR * unscaledRawTiltAngle); }
                rawTiltAngle = rawTiltAngle % TO_360;
            }

            /// This line calculates the tilt angle by passing the gyro rate of
            /// change and the angle returned by the accelerometer to the Kalman
            /// filter, producing a refined tilt (yOrientaion) angle.
            lock (tiltLock)
            {
                tiltAngle = 90 + KalmanFilter.getAngle(XR - 90, gyroY, 0.01);                
            }

            /// Logging
            this.Dispatcher.Invoke(new Action(delegate()
            {
                statusLabel.Content = "Location: (" + Math.Round(currentPosition.X, 2) + ", " + Math.Round(currentPosition.Y, 2) + ")\tTilt: " + tiltAngle + "\tOrientation: " + orientaion;
            }));

            lock (counterLock)
            {
                sendCounter++;

                if (sendCounter > 10)
                {
                    Point bla = Util.getIntersection(new Point(0, 0), tiltAngle, orientaion);
                    Console.WriteLine("(" + bla.X + "," + bla.Y + ")");
                    IEnumerable<IADevice> devices = this.intAirAct.Devices;

                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("x", bla.X.ToString());
                    dictionary.Add("y", bla.Y.ToString());

                    IARequest request = new IARequest(sendRoute);
                    request.SetBodyWith(dictionary);
                    request.Origin = this.intAirAct.OwnDevice;

                    foreach (IADevice device in devices)
                    {
                        this.intAirAct.SendRequest(request, device);
                    }
                    sendCounter = 0;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imu.Stop();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            rawTiltAngle = 0;
            unscaledRawTiltAngle = 0;

            currentPosition = new Point(0, 0);
            tiltAngle = 0;
            orientaion = 0;

            previousAcceleration = 0;
            previousVelocity = 0;
            unscaledOrientation = 0;          
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                imu.Stop();
                Application.Current.Shutdown();
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(nameTextBox.Text + "_results.txt");
            file.WriteLine(output);
            file.Close();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            Point intersection = Util.getIntersection(new Point(0, 0), tiltAngle, orientaion);
            output += "Location: (" + Math.Round(currentPosition.X, 2) + ", " + Math.Round(currentPosition.Y, 2) + ")\tTilt: " + Math.Round(tiltAngle,2) + "\tOrientation: " + Math.Round(orientaion,2) + "\tIntersection: (" + intersection.X + ", "+ intersection.Y + ")\r\n";
        }
    }
}
