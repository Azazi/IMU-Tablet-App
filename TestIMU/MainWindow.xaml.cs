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
            double scaleFactor = 15.75467;
            double sensitivity = 0.0000875;

            double toDPS = 0.00875;
            double minimumRateOfChange = 0.5;

            double zOrientation = 0;

            if (Math.Abs(gyroZ * toDPS) >= minimumRateOfChange)
            {
                double newGyroZ = gyroZ + 2.972420635;

                zOrientation = (newGyroZ * sensitivity + previousZOrientation);
                previousZOrientation = zOrientation;

                double scaledZOrientation = scaleFactor * zOrientation;
                Console.WriteLine(scaledZOrientation + "");
            }

            double xOrientation = (gyroX * sensitivity + previousXOrientation);
            previousXOrientation = xOrientation;

            double yOrientation = (gyroY * sensitivity + previousYOrientation);
            previousYOrientation = yOrientation;

            double scaledXOrientation = scaleFactor * xOrientation;
            double scaledYOrientation = scaleFactor * yOrientation;
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
