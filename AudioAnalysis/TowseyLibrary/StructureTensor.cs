using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{
    public class StructureTensor
    {
        private double[,] StructureTensorMatrix = new double[2,2];

        public double magnitude;
        public double radians;
        public double derivative;
        public double category;
        public double eigenValue1, eigenValue2;

        public StructureTensor(double[,] matrix)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            if ((rowCount != 2) && (colCount != 2)) throw new Exception("Structure tensor must be 2x2 matrix");

            double dx = matrix[1,1] - matrix[1,0];
            double dy = matrix[0,0] - matrix[1,0];
            double derivative = dy / dx;
            this.StructureTensorMatrix = CalculateStructureTensor(matrix);
            this.magnitude = Math.Sqrt((dy * dy) + (dx * dx));
            this.radians = Math.Atan2(dy, dx);


            double category0Boundary = Math.PI /(double)8;
            double category1Boundary = category0Boundary * 3;
            double category2Boundary = category1Boundary * 5;
            double category3Boundary = category2Boundary * 7;
            //double angle = radians * (180 / Math.PI);
            //double angle = (Math.PI / (double)8) * (180 / Math.PI);
            if ((this.derivative <= category0Boundary) && (this.derivative >= -category0Boundary)) 
                category = 0;
            else
                if ((this.derivative <= category1Boundary) && (this.derivative > category0Boundary))
                    category = 1;
                else
                    if ((this.derivative <= category2Boundary) && (this.derivative > category1Boundary))
                        category = 2;
                    else
                        if ((this.derivative <= category3Boundary) && (this.derivative > category2Boundary))
                            category = 3;


            double[] eigenValues = CalculateEigenValues(this.StructureTensorMatrix);
            Console.WriteLine("eigenValues = {0} and {1}", eigenValues[0], eigenValues[1]);

        }


        public static double[,] CalculateStructureTensor(double[,] matrix)
        {
            double dx = matrix[1, 1] - matrix[1, 0];
            double dy = matrix[0, 0] - matrix[1, 0];
            double[,] structureTensorMatrix = new double[2,2];
            structureTensorMatrix[0, 0] = dx * dx;
            structureTensorMatrix[1, 1] = dy * dy;
            structureTensorMatrix[0, 1] = dy * dx;
            structureTensorMatrix[1, 0] = dy * dx;

            return structureTensorMatrix;
        }

        public static double[] CalculateEigenValues(double[,] M)
        {
            double traceSTM = M[0, 0] + M[1, 1];
            double determinantSTM = (M[0, 0] * M[1, 1]) - (M[0, 1] * M[1, 0]);
            double discriminantSTM = (traceSTM * traceSTM) - (4 * determinantSTM);

            double[] eigenValues = new double[2];

            eigenValues[0] = (traceSTM / (double)2) + (Math.Sqrt(discriminantSTM)) / (double)2;
            eigenValues[1] = (traceSTM / (double)2) - (Math.Sqrt(discriminantSTM)) / (double)2;
            return eigenValues;
        }

    }
}
