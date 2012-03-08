using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioAnalysisTools
{
    public static class SprTools
    {

        //char[] code = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q' }; //10 degree jumps
        //90 degree angle = symbol 'i'   i.e. the vertical
        //int resolutionAngle = 10;
        public static char[] code = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l' };   //15 degree jumps
        //90 degree angle = symbol 'g' i.e. the vertical
        public const int resolutionAngle = 15;
        

        /// <summary>
        /// returns the angle difference between two angle symbols
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static int SymbolDifference(char c1, char c2)
        {
            if (c1 == c2) return 0;
            int angle = Math.Abs((int)c1 - (int)c2) * resolutionAngle;
            if (angle > 90) angle = 180 - angle;
            return angle;
        }

        public static double[,] Target2SpectralTracks(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] tracks = new double[rows, cols];

            for (int i = 1; i < rows - 1; i++)
            {
                for (int j = 1; j < cols - 1; j++)
                {
                    //if (matrix[i,j] < threshold) continue;
                    if (((matrix[i, j] > matrix[i, j + 1]) && (matrix[i, j] > matrix[i, j - 1])) ||
                        ((matrix[i, j] > matrix[i + 1, j]) && (matrix[i, j] > matrix[i - 1, j])))
                        tracks[i, j] = matrix[i, j];
                }
            }

            return tracks;
        }




        public static char[,] Target2SymbolicTracks(double[,] matrix, double threshold, int lineLength)
        {
            var m = ImageTools.WienerFilter(matrix, 5);
            m = Target2SpectralTracks(m, threshold);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //initialise symbolic matrix with hyphens
            char[,] symbolic = new char[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++) symbolic[r, c] = '-';
            }

            double[,] intensityScores = new double[rows, cols];

            double sumThreshold = lineLength * threshold; //require average of threshold dB per pixel.

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var result = ImageTools.DetectLine(m, r, c, lineLength, threshold, resolutionAngle);
                    if (result != null)
                    {
                        int degrees = result.Item1;
                        double intensity = result.Item2;
                        
                        if ((intensity > sumThreshold) && (intensity > intensityScores[r, c]))
                        {
                            //if (intensity > intensityScores[r, c]) intensityScores[r, c] = intensity; // store the intensity
                            double cosAngle = Math.Cos(Math.PI * degrees / 180);
                            double sinAngle = Math.Sin(Math.PI * degrees / 180);
                            //symbolic[r, c] = code[degrees / resolutionAngle];
                            for (int j = 0; j < lineLength; j++)
                            {
                                int row = r + (int)(cosAngle * j);
                                int col = c + (int)(sinAngle * j);
                                if (intensity > intensityScores[row, col]) 
                                symbolic[row, col] = code[degrees / resolutionAngle];
                            } // line length

                        }
                    }
                } // columns
            } // rows

            //FileTools.WriteMatrix2File(symbolic, "C:\\SensorNetworks\\Output\\FELT_LewinsRail1\\charogram.txt"); //view the char-ogram
            CleanSymbolicTracks(symbolic);
            return symbolic;
        } // Target2SymbolicTracks()


        /// <summary>
        /// Cleans up a symbolic matrix.
        /// Removes a symbol if it is isolated.
        /// </summary>
        /// <param name="inputM"></param>
        public static void CleanSymbolicTracks(char[,] inputM)
        {
            int rows = inputM.GetLength(0);
            int cols = inputM.GetLength(1);
            for (int r = 1; r < rows-1; r++)
            {
                for (int c = 1; c < cols-1; c++)
                { 
                    if (inputM[r,c] == '-') continue;

                    if ((inputM[r, c - 1] == '-') && (inputM[r, c + 1] == '-') && (inputM[r-1, c] == '-') && (inputM[r+1, c] == '-'))
                    {
                        inputM[r, c]   = '-';
                        //inputM[r, c+1] = '-';
                    }
                } // cols
            } // rows
        }

        /// <summary>
        /// counts the symbols in an SPR template. Exclude the '-' char which is just background instaed of space.
        /// </summary>
        /// <param name="templateMatrix"></param>
        /// <returns></returns>
        public static int CountTemplateChars(char[,] templateMatrix)
        {
            int rows = templateMatrix.GetLength(0);
            int cols = templateMatrix.GetLength(1);
            int count = 0;

            for (int i = 1; i < rows - 1; i++)
            {
                for (int j = 1; j < cols - 1; j++)
                {
                    if (templateMatrix[i, j] != '-') count++;
                }
            }

            return count;
        }



    }
}
