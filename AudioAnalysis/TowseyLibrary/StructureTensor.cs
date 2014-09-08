using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{
    public static class StructureTensor
    {
        //private double[,] StructureTensorMatrix = new double[2,2];

        public class StructureTensorResult
        {
            public double Magnitude {get; set;}
            public double Radians { get; set; }
            // easier to debug with degrees
            public double Degrees { get; set; }
            public double Derivative { get; set; }
            public double EigenValue1 { get; set; }
            public double EigenValue2 { get; set; }
            public double Dominance { get; set; }
        }
        public class RidgeTensorResult
        {
            /// <summary>
            /// average of gradient magnitudes on left and right side of ridge.
            /// </summary>
            public double AvMagnitude { get; set; }
            /// <summary>
            /// average of gradient directions on left and right side of ridge.
            /// </summary>
            public double AvRadians { get; set; }
            /// <summary>
            /// ridge direction (in radians) derived fomr average of left and right gradient directions.
            /// </summary>
            public double RidgeDirection { get; set; }
            /// <summary>
            /// difference in gradient directions (in radians) on left and right side of ridge
            /// </summary>
            public double DirectionDifference { get; set; }
            public double DirectionIndecision { get; set; }
            // easier to debug with degrees
            public double Degrees { get; set; }
            /// <summary>
            /// ridge direction category i.e. vertical, horozontal, pos and neg 45 degrees.
            /// 0=not a ridge; 1=horizontal; 2=+45 degrees; 3= -45 degrees; 4=vertical.
            /// </summary>
            public byte RidgeDirectionCategory { get; set; }
            /// <summary>
            /// average of the eigen vector dominances
            /// </summary>
            public double AvDominance { get; set; }
            public bool IsRidge { get; set; }
        }


        /// <summary>
        /// CONSTRUCTOR
        /// Pass both the original image and structure tensor because need to calculate derivatives as well as eigenvalues.
        /// Cannot easily derive the partial derivatives from the structure tensor because have lost the sign info.
        /// </summary>
        /// <param name="avImage"></param>
        /// <param name="avStructureTensor"></param>
        public static StructureTensorResult GetStructureTensorInfo(double[,] avImage, double[,] avStructureTensor)
        {
            int rowCount = avStructureTensor.GetLength(0);
            int colCount = avStructureTensor.GetLength(1);
            if ((rowCount != 2) && (colCount != 2)) throw new Exception("Structure tensor must be 2x2 matrix");

            double dx = avImage[1, 1] - avImage[1, 0];
            double dy = avImage[0, 0] - avImage[1, 0];

            var result = new StructureTensorResult();
            result.Derivative = dy / dx;
            result.Magnitude = Math.Sqrt((dy * dy) + (dx * dx));
            double radians = Math.Atan2(dy, dx);
            result.Radians = radians;
            result.Degrees = radians * (180 / Math.PI);

            double[] eigenValues = CalculateEigenValues(avStructureTensor);
            result.EigenValue1 = Math.Abs(eigenValues[0]);
            result.EigenValue2 = Math.Abs(eigenValues[1]);
            result.Dominance = Math.Abs((result.EigenValue1 - result.EigenValue2) / (result.EigenValue1 + result.EigenValue2));

            //Console.WriteLine("Slope magnitude={0}     dy/dx={1}", result.Magnitude, result.Derivative);
            //Console.WriteLine("Radians={0:f2}   Angle={1:f1}", radians, result.Degrees);
            //Console.WriteLine("Eigenvalues = {0:f6} and {1:f6}.    Dominance = {2:f2}", eigenValues[0], eigenValues[1], result.Dominance);
            return result;
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


        public static StructureTensorResult EdgeDetection_VerticalDirection(double[,] M)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            int halfwidth = colCount / 2;

            //accumulate info in the positive direction
            var avMatrixValues = new double[2, 2];
            var avStructTensor = new double[2, 2];
            var window = new double[2, 2];

            //search for ridge in central vertical cells
            int N = 0;
            //avoid the edge rows
            for (int r = 1; r < rowCount-1; r++)
            {
                // calculate average matrix in positive direction
                window[0, 0] = M[r - 1, halfwidth];
                window[0, 1] = M[r - 1, halfwidth + 1];
                window[1, 0] = M[r, halfwidth];
                window[1, 1] = M[r, halfwidth + 1];
                avMatrixValues = MatrixTools.AddMatrices(avMatrixValues, window);

                // calculate structure tensor in positive direction
                var st = StructureTensor.CalculateStructureTensor(window);
                avStructTensor = MatrixTools.AddMatrices(avStructTensor, st);


                // count number of contributing cells so can get average 
                N++;
            }
            // get average
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 2; c++)
                {
                    avMatrixValues[r, c] /= (double)N;
                    avStructTensor[r, c] /= (double)N;
                }
            }

            TowseyLibrary.StructureTensor.StructureTensorResult result = StructureTensor.GetStructureTensorInfo(avMatrixValues, avStructTensor);
            return result;
        }


        public static RidgeTensorResult RidgeDetection_VerticalDirection(double[,] M, double magnitudeThreshold, double dominanceThreshold)
        {
            TowseyLibrary.StructureTensor.StructureTensorResult result1 = StructureTensor.EdgeDetection_VerticalDirection(M);
            M = MatrixTools.MatrixRotate180(M);
            TowseyLibrary.StructureTensor.StructureTensorResult result2 = StructureTensor.EdgeDetection_VerticalDirection(M);

            RidgeTensorResult result = new RidgeTensorResult();
            result.AvMagnitude = (result1.Magnitude + result2.Magnitude) / (double)2;
            result.AvRadians   = (result1.Radians + result2.Radians) / (double)2;
            result.DirectionDifference = (result1.Radians - result2.Radians);
            result.DirectionIndecision = result.DirectionDifference / (result1.Radians + result2.Radians);
            result.AvDominance = (result1.Dominance + result2.Dominance) / (double)2;
            // ridge direction is at right angles to the slope direction. Add 90 degrees.
            double piDiv2 = Math.PI / (double)2;
            result.RidgeDirection = result.AvRadians + piDiv2;
            // correct if slope > 90 degrees or < 90 degrees.
            if (result.RidgeDirection < -piDiv2) result.RidgeDirection += Math.PI;
            if (result.RidgeDirection >  piDiv2) result.RidgeDirection -= Math.PI;

            //calculate boundaries in degrees = easier!!
            double category1Boundary = 22.5;
            double category2Boundary = category1Boundary * 3;
            //double category3Boundary = category1Boundary * 5;
            //double category4Boundary = category1Boundary * 7;

            double ridgeAngle = result.RidgeDirection * (180 / Math.PI);
            //double angle = (Math.PI / (double)8) * (180 / Math.PI);
            result.RidgeDirectionCategory = 0;
            if ((ridgeAngle >= -category1Boundary) && (ridgeAngle <= category1Boundary))
                result.RidgeDirectionCategory = 1;
            else
                if ((ridgeAngle <= category2Boundary) && (ridgeAngle > category1Boundary))
                    result.RidgeDirectionCategory = 2;
                else
                    if ((ridgeAngle < -category1Boundary) && (ridgeAngle > -category2Boundary))
                        result.RidgeDirectionCategory = 3;
                    else
                        if ((ridgeAngle > category2Boundary) && (ridgeAngle <  piDiv2))
                            result.RidgeDirectionCategory = 4;
                        else
                        if ((ridgeAngle <= -category2Boundary) && (ridgeAngle > -piDiv2))
                            result.RidgeDirectionCategory = 4;

            return result;
        }


    }
}
