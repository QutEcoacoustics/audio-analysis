using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;

namespace QutBioacosutics.Xie
{
    class ExtractTracks
    {
        public double[,] GetTracks(double[,] matrix)
        {

            var row = matrix.GetLength(1);
            var column = matrix.GetLength(0);

            // save local peaks to an array of list

            var Points = new List<Peak>();

            

            //for (int i = 0; i < rowMaximum; i++)
            //{
            //    for (int j = 0; j < colMaximum; i++)
            //    {
            //        var point = new Peak();
            //        point.X = i;
            //        point.Y = j;
            //        point.Amplitude = matrix[i, j];
            //        Points.Add(point);
            //    }

            //}
            return null;
                
        
        
        }

    }
}
