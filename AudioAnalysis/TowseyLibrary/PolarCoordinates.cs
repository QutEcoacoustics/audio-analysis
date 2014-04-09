using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{
    public static class PolarCoordinates
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ox">X coordinate of origin</param>
        /// <param name="Oy">Y coordinate of origin</param>
        /// <param name="theta">the angle in radians</param>
        /// <param name="distance">the distance as integer</param>
        /// <returns></returns>
        public static double[] GetPolarCoordinates(int Ox, int Oy, double theta, int distance)
        {
            double[] p = new double[2];
            p[0] = Ox + (distance * Math.Cos(Math.PI * theta));
            p[1] = Oy + (distance * Math.Sin(Math.PI * theta));
            return p;
        }
    }
}
