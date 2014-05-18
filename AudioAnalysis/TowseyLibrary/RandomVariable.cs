using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLibrary
{



 /**
 * @author towsey
 *
 * generates numbers according to a gaussian distribution 
 */

    
    
    public class RandomVariable
    {
        private double mean = 0.0;
        private double SD = 1.0;
        private RandomNumber R;


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public RandomVariable()
        {
            R = new RandomNumber();
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="SD"></param>
        public RandomVariable(double mean, double SD)
        {
            this.mean = mean;
            this.SD = SD;
            R = new RandomNumber();
        }

        public RandomVariable(double mean, double SD, int seed)
        {
            this.mean = mean;
            this.SD = SD;
            R = new RandomNumber(seed);
        }

        //public double getVar()
        //{
        //    double n = R.getGaussian();
        //    bool positive = R.getBoolean();
        //    if (positive) return mean + (n * SD);
        //    else return mean - (n * SD);
        //}


        /**
         * @return Returns the mean.
         */
        public double getMean()
        {
            return mean;
        }
        /**
         * @param mean The mean to set.
         */
        public void setMean(double mean)
        {
            this.mean = mean;
        }
        /**
         * @return Returns the sD.
         */
        public double getSD()
        {
            return SD;
        }
        /**
         * @param sd The sD to set.
         */
        public void setSD(double sd)
        {
            SD = sd;
        }
    }

}
