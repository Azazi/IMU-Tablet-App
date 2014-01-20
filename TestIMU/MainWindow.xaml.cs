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
        EMU emu;

        double previousXOrientation = 0;
        double previousYOrientation = 0;
        double previousZOrientation = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            emu = new EMU();
            emu.OnDataRecieved += new EMU.DataRecieved(emu_OnDataRecieved);
            emu.Start();
        }

        void emu_OnDataRecieved(int accelX, int accelY, int accelZ, int gyroX, int gyroY, int gyroZ)
        {
            double scaleFactor = 15.75467;
            double sensitivity = 0.0000875;

            //double xrAngle;
            //double yrAngle;
            //double zrAngle;

            //double r = Math.Sqrt(accelX * accelX + accelY * accelY + accelZ * accelZ);

            double xOrientation = (gyroX * sensitivity + previousXOrientation);
            previousXOrientation = xOrientation;

            double yOrientation = (gyroY * sensitivity + previousYOrientation);
            previousYOrientation = yOrientation;

            double zOrientation = (gyroZ * sensitivity + previousZOrientation);
            previousZOrientation = zOrientation;

            double scaledXOrientation = scaleFactor * xOrientation;
            double scaledYOrientation = scaleFactor * yOrientation;
            double scaledZOrientation = scaleFactor * zOrientation;

            Console.WriteLine(scaledZOrientation + "");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            emu.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            previousXOrientation = 0;
            previousYOrientation = 0;
            previousZOrientation = 0;
        }
    }
}
