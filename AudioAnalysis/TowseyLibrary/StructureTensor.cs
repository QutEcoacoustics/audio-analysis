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


        public static RidgeTensorResult RidgeDetection_VerticalDirection(double[,] M)
        {
            // 0,1,2,3 indicate directions East, North, West, South respectively of the position (x,y).
            //The following variables are required to obtain the local gradient.
            double dx0, dy1, dx2, dy3;
            //The following variables are required to construct the structure tensor.
            double dxdx0, dydy1, dxdx2, dydy3, dydx0, dydx2;

            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            int halfwidth = colCount / 2;

            //accumulate info in the positive direction
            //var avMatrixValues = new double[2, 2];
            var avStructTensor = new double[2, 2];
            //var window = new double[2, 2];

            //search for ridge in central vertical cells
            int N = 0;
            dx0 = 0.0;
            dy1 = 0.0;
            dx2 = 0.0;
            dy3 = 0.0;
            dxdx0 = 0.0;
            dydy1 = 0.0;
            dxdx2 = 0.0;
            dydy3 = 0.0;
            dydx0 = 0.0;
            dydx2 = 0.0;
            //avoid the edge rows
            for (int r = 1; r < rowCount-1; r++)
            {
                // calculate gradients in four directions
                dx0 += M[r, halfwidth + 1] - M[r, halfwidth];
                dy1 += M[r - 1, halfwidth] - M[r, halfwidth];
                dx2 += M[r, halfwidth - 1] - M[r, halfwidth];
                dy3 += M[r + 1, halfwidth] - M[r, halfwidth];
                // accumulate structure tensor components
                dxdx0 += dx0 * dx0;
                dydy1 += dy1 * dy1;
                dxdx2 += dx2 * dx2;
                dydy3 += dy3 * dy3;
                dydx0 += dx0 * dy1;
                dydx2 += dx2 * dy3;

                // count number of contributing cells so can get average 
                N++;
            }
            // get average
            dx0 /= N;
            dy1 /= N;
            dx2 /= N;
            dy3 /= N;
            dxdx0 /= N;
            dydy1 /= N;
            dxdx2 /= N;
            dydy3 /= N;
            dydx0 /= N;
            dydx2 /= N;

            double[,] stM1 = {
                         { dxdx0, dydx0},
                         { dydx0, dydy1}
                      };
            double[] eigenvalues1 = CalculateEigenValues(stM1);
            double[,] stM2 = {
                         { dxdx2, dydx2},
                         { dydx2, dydy3}
                      };
            double[] eigenvalues2 = CalculateEigenValues(stM2);

            double ridgeMagnitude = (dx0 + dx2) / (double)2;
            if ((dx0 < 3.0) || (dx2 < 3.0)) ridgeMagnitude = 0.0;

            //accumulate results
            RidgeTensorResult result = new RidgeTensorResult();

            // this magnitude is equivalent to Sobel calculation over three columns
            result.AvMagnitude = ridgeMagnitude;
            double eigenDominance1 = Math.Abs((eigenvalues1[0] - eigenvalues1[1]) / (eigenvalues1[0] + eigenvalues1[1]));
            double eigenDominance2 = Math.Abs((eigenvalues2[0] - eigenvalues2[1]) / (eigenvalues2[0] + eigenvalues2[1]));
            result.AvDominance = (eigenDominance1 + eigenDominance2) / (double)2;


            double radiansE = Math.Atan2(dy1, dx0);
            double radiansW = Math.Atan2(dy3, dx2);
            double degrees1 = radiansE * (180 / Math.PI);
            double degrees2 = radiansW * (180 / Math.PI);

            result.AvRadians   = (radiansE + radiansW) / (double)2;
            result.DirectionDifference = (radiansE - radiansW);
            result.DirectionIndecision = result.DirectionDifference / (radiansE + radiansW);

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
            //over-ride direction for vertical
            result.RidgeDirectionCategory = 4;
            return result;
        }

        public static void Test1StructureTensor()
        {
            // create a local image matrix
            double[,] image = { {0.9, 10.0,},
                                {10.0, 0.0,}
                                };

            var structureTensorMatrix = StructureTensor.CalculateStructureTensor(image);

            StructureTensor.StructureTensorResult result = StructureTensor.GetStructureTensorInfo(image, structureTensorMatrix);
        }

        /// <summary>
        ///  used to test ridge detection using structure tensor.
        /// </summary>
        public static void Test2StructureTensor()
            {
               // create a local image matrix
                double[,] image = { {0.1, 0.1, 0.1, 10.1, 0.1, 0.1, 0.1 },
                                    {0.1, 0.1, 0.1, 10.0, 0.1, 0.1, 0.1 },
                                    {0.1, 0.1, 0.1,  9.5, 0.1, 0.1, 0.1 },
                                    {0.1, 0.1, 0.1, 10.0, 0.1, 0.1, 0.1 },
                                    {0.1, 0.1, 0.1,  9.9, 0.1, 0.1, 0.1 },
                                    {0.1, 0.1, 0.1, 10.0, 0.1, 0.1, 0.1 },
                                    {0.1, 0.1, 0.1, 10.2, 0.1, 0.1, 0.1 },
                                  };
                StructureTensor.RidgeTensorResult result = StructureTensor.RidgeDetection_VerticalDirection(image);
            }


    }
}
