using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TestIMU
{
    public static class Util
    {
        private const double HEIGHT = 66;
        private const double DISTANCE_FROM_DISPLAY = 150;
        public static Point getIntersection(Point location, double tiltAngle, double orientation)
        {
            Point returnPoint = new Point();

            //Get X-Y Line
            double m_1 = Math.Tan(orientation*Math.PI/180);
            double n_1 = location.Y - (m_1 * location.X);

            // Get X-Z Line
            double m_2 = Math.Tan(tiltAngle * Math.PI / 180);
            double n_2 = HEIGHT - (m_2 * location.X);

            // Get intersection point
            double x = (DISTANCE_FROM_DISPLAY - n_1) / m_1;
            double z = (m_2 * x) + n_2;

            if(x<=71.585 && x>=-71.585 && z>=39.95 && z<=119.85)
            {
                returnPoint.Y = z * (-0.0125156) + (1.5);
                returnPoint.X = x * (0.0069847) + (0.5);
            }
            else
            {
                returnPoint.X = -1;
                returnPoint.Y = -1;
            }
            return returnPoint;
        }
    }
}
