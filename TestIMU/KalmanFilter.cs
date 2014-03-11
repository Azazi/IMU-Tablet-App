using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIMU
{
    public static class KalmanFilter
    {
        static double Q_angle = 0.001;                                    // Process noise variance for the accelerometer
         static double Q_bias = 0.003;                                     // Process noise variance for the gyro bias
         static double R_measure = 0.03;                                   // Measurement noise variance - this is actually the variance of the measurement noise

         static double angle = 0;                                          // The angle calculated by the Kalman filter - part of the 2x1 state vector
         static double bias = 0;                                           // The gyro bias calculated by the Kalman filter - part of the 2x1 state vector
         static double rate;                                               // Unbiased rate calculated from the rate and the calculated bias - you have to call getAngle to update the rate

         static double[,] P = new double[2,2] { { 0, 0 }, { 0, 0 }};       // Error covariance matrix - This is a 2x2 matrix
         static double[] K = new double[2];                                // Kalman gain - This is a 2x1 vector

         static double y;                                                  // Angle difference
         static double S;                                                  // Estimate error

        //public void setQangle(double newQ_angle) { Q_angle = newQ_angle; }
        //public void setQbias(double newQ_bias) { Q_bias = newQ_bias; }
        //public void setRmeasure(double newR_measure) { R_measure = newR_measure; }

        //public double getQangle() { return Q_angle; }
        //public double getQbias() { return Q_bias; }
        //public double getRmeasure() { return R_measure; }

        //public void setAngle(double newAngle) { angle = newAngle; }
        //public double getRate() { return rate; }

        /// <summary>
        /// Returns the current angle based on previous state and current reading
        /// </summary>
        /// <param name="newAngle"> Angle in degrees</param>
        /// <param name="newRate"> Rate in degrees per second</param>
        /// <param name="dt">Delta time in seconds</param>
        /// <returns>Angle</returns>
        public static double getAngle(double newAngle, double newRate, double dt)
        {
            // KasBot V2  -  Kalman filter module - http://www.x-firm.com/?page_id=145
            // Modified by Kristian Lauszus
            // See my blog post for more information: http://blog.tkjelectronics.dk/2012/09/a-practical-approach-to-kalman-filter-and-how-to-implement-it

            // Discrete Kalman filter time update equations - Time Update ("Predict")
            // Update xhat - Project the state ahead
            /* Step 1 */

            rate = newRate - bias;
            angle += dt * rate;

            // Update estimation error covariance - Project the error covariance ahead
            /* Step 2 */
            P[0,0] += dt * (dt * P[1,1] - P[0,1] - P[1,0] + Q_angle);
            P[0,1] -= dt * P[1,1];
            P[1,0] -= dt * P[1,1];
            P[1,1] += Q_bias * dt;

            // Discrete Kalman filter measurement update equations - Measurement Update ("Correct")
            // Calculate Kalman gain - Compute the Kalman gain
            /* Step 4 */
            S = P[0,0] + R_measure;
            /* Step 5 */
            K[0] = P[0,0] / S;
            K[1] = P[1,0] / S;

            // Calculate angle and bias - Update estimate with measurement zk (newAngle)
            /* Step 3 */
            y = newAngle - angle;
            /* Step 6 */
            angle += K[0] * y;
            bias += K[1] * y;

            // Calculate estimation error covariance - Update the error covariance
            /* Step 7 */
            P[0,0] -= K[0] * P[0,0];
            P[0,1] -= K[0] * P[0,1];
            P[1,0] -= K[1] * P[0,0];
            P[1,1] -= K[1] * P[0,1];

            return angle;
        }
    }
}
