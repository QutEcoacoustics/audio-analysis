using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TowseyLib;
using System.Drawing.Imaging;



namespace QutBioacosutics.Xie
{
    public static class XieFunction
    {
        /// <summary>
        /// Adding Multiple Elements to a List on One Line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        public static void AddMany<T>(this List<T> list, params T[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                list.Add(elements[i]);
            }
        }


        public static int ArrayCount(double[] Array)
        {
            int number = 0;
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i] > 0) number++;          
            }
            return number;
        }



        //ArrayIndex function in R
        public static int[] ArrayIndex(double[] Array)
        {
            var index = new List<int>();
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i] > 0)
                {
                    index.Add(i);               
                }                           
            }
      
            var result = index.ToArray();
            return result;
        }


        public static double Sum(params double[] customerssalary)
        {
            double result = 0;

            for (int i = 0; i < customerssalary.Length; i++)
            {
                result += customerssalary[i];
            }

            return result;
        }
        // Draw tracks on the spectrogram
        //public static void DrawSpectrogram(double[,] matrix)
        //{

        //}
    }
}
