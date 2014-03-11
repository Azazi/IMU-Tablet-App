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

namespace TestIMU
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IMU imu;

        double previousXOrientation = 0;
        double previousYOrientation = 0;
        double previousZOrientation = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imu = new IMU();
            imu.OnDataRecieved += new IMU.DataRecieved(imu_OnDataRecieved);
            imu.Start();
        }

        void imu_OnDataRecieved(int accelX, int accelY, int accelZ, int gyroX, int gyroY, int gyroZ)
        {
            double R = Math.Sqrt(Math.Pow(accelX, 2) + Math.Pow(accelY, 2) + Math.Pow(accelZ, 2));
            double XR = Math.Acos(accelX / R) * 180 / Math.PI;
            double YR = Math.Acos(accelY / R) * 180 / Math.PI;
            double ZR = Math.Acos(accelZ / R) * 180 / Math.PI;

            double scaleFactor = 15.75467;
            double sensitivity = 0.0000875;

            double toDPS = 0.00875;
            double minimumRateOfChange = 0.5;

            double zOrientation = 0;
            double yOrientation = 0;

            if (Math.Abs(gyroZ * toDPS) >= minimumRateOfChange)
            {
                double newGyroZ = gyroZ + 2.972420635;

                zOrientation = (newGyroZ * sensitivity + previousZOrientation);
                previousZOrientation = zOrientation;

                double scaledZOrientation = scaleFactor * zOrientation;
                Console.WriteLine(scaledZOrientation + "");
            }


            /// I am just including this for comparison purposes, the acutal 
            /// yOrientaion aka the tilt angle will be computed using the Kalman
            /// filter as in below.
            if (Math.Abs(gyroY * toDPS) >= minimumRateOfChange)
            {
                double newGyroY = gyroY + 2.972420635;

                yOrientation = (newGyroY * sensitivity + previousZOrientation);
                previousYOrientation = yOrientation;

                double scaledYOrientation = scaleFactor * yOrientation;
                Console.WriteLine(scaledYOrientation + "");
            }

            /// This line calculates the tilt angle by passing the gyro rate of
            /// change and the angle returned by the accelerometer to the Kalman
            /// filter, producing a refined tilt (yOrientaion) angle
            double tiltAngle = KalmanFilter.getAngle(XR - 90, gyroY, 0.01);

            /// Displaying a rounded tilt angle as the content of the reset button
            /// as it provides a more readable measure than monitoring long lines of
            /// digits on the console
            this.Dispatcher.Invoke(new Action(delegate()
            {
                resetButton.Content = Math.Round(tiltAngle, 2);
            }));

            /// The next three lines are not needed as we are not considering
            /// the xOrientation for our case study
            double xOrientation = (gyroX * sensitivity + previousXOrientation);
            previousXOrientation = xOrientation;
            double scaledXOrientation = scaleFactor * xOrientation;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imu.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            previousXOrientation = 0;
            previousYOrientation = 0;
            previousZOrientation = 0;
        }
    }
}
