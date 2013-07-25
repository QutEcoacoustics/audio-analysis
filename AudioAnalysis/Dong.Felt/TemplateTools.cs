// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Template.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the TemplateTools type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using AudioAnalysisTools;
    using TowseyLib;
    using System.IO;
    using CsvHelper;
    using System.Text;

    /// <summary>
    /// The template tools.
    /// </summary>
    public class TemplateTools
    {
        /// <summary>
        /// The centroid frequency for LewinsRailTemplate.
        /// </summary>
        public static readonly int CentroidFrequencyOfLewinsRailTemplate = 91;


        /// <summary>
        /// The centroid frequency for Crow.
        /// </summary>
        public static readonly int CentroidFrequencyOfCrowTemplate = 80;

        //public const string VectorTypeHorizontal = "h";
        //public const string VectorTypeHorizontal = "v";
        //public const string VectorTypeHorizontal = "pd";
        //public const string VectorTypeHorizontal = "nd";

        //public static IEnumerable<FeatureVector> ReadCSVFileToFeatureVector(FileInfo filePath)
        //{
        //    using(var textReader = filePath.OpenText())
        //    using (var csv = new CsvReader(textReader))
        //    {
        //        while (csv.Read())
        //        {
        //            var frameNumber = csv.GetField<double>("FrameNumber");
        //            var distance = csv.GetField<double>("Distance");
        //            var sliceNumber = csv.GetField<int>("SliceNumber");
        //            var vectorDirection = csv.GetField<string>("VectorDirection");
        //            var values = csv.CurrentRecord.Skip(4);

        //        }
        //    }
        //}

        public static List<FeatureVector> Grey_Fantail1()
        {
            var result = new List<FeatureVector>();
            var slopeScore = new int[] { 0, 18, 0, 0, 0, 0, 0, 0, 0, 27, 9, 0, 3, 12, 0, 0, 0, 0, 0, 0, 3, 5, 9, 7, 7, 12, 0, 0, 0, 0, 0, 0, 3, 0, 1, 2, 7, 0, 0, 0 };
            var slopeItem1 = new int[] { 0, 3, 0, 0, 0, 0, 0, 0, 0, 3, 3, 0, 1, 3, 0, 0, 0, 0, 0, 0, 3, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 3, 0, 1, 1, 1, 0, 0, 0 };
            var slopeItem2 = new int[] { 0, 6, 0, 0, 0, 0, 0, 0, 0, 9, 3, 0, 3, 4, 0, 0, 0, 0, 0, 0, 1, 5, 9, 7, 7, 12, 0, 0, 0, 0, 0, 0, 1, 0, 1, 2, 7, 0, 0, 0 };
            var maxColIndex = 4651;
            var minColIndex = 4547;
            var maxRowIndex = 150;
            var minRowIndex = 85;
            for (int index = 0; index < slopeScore.Count(); index++)
            {
                result.Add(new FeatureVector(new Point(0, 0))
                {
                    Slope = new Tuple<int, int>(slopeItem1[index], slopeItem2[index]),
                    SlopeScore = slopeScore[index],
                    MaxColIndex = maxColIndex,
                    MinColIndex = minColIndex,
                    MaxRowIndex = maxRowIndex,
                    MinRowIndex = minRowIndex,

                });
            }
            return result;
        }

        public static List<FeatureVector> Grey_Shrikethrush4()
        {
            var result = new List<FeatureVector>();
            var slopeScore = new int[] { 15, 15, 0, 15, 0, 33, 13, 12, 18, 18, 0, 15, 0, 6, 0, 0 };
            var slopeItem1 = new int[] { 3, 3, 0, 3, 0, 3, 1, 1, 3, 3, 0, 3, 0, 3, 0, 0 };
            var slopeItem2 = new int[] { 5, 5, 0, 5, 0, 11, 13, 12, 6, 6, 0, 5, 0, 2, 0, 0 };
            var maxColIndex = 2386;
            var minColIndex = 2282;
            var maxRowIndex = 234;
            var minRowIndex = 208;
            for (int index = 0; index < slopeScore.Count(); index++)
            {
                result.Add(new FeatureVector(new Point(0, 0))
                {
                    Slope = new Tuple<int, int>(slopeItem1[index], slopeItem2[index]),
                    SlopeScore = slopeScore[index],
                    MaxColIndex = maxColIndex,
                    MinColIndex = minColIndex,
                    MaxRowIndex = maxRowIndex,
                    MinRowIndex = minRowIndex,

                });
            }
            return result;
        }

        public static List<FeatureVector> Scarlet_Honeyeater1()
        {
            var result = new List<FeatureVector>();
            var slopeScore = new int[] { 33, 0, 0, 0, 45, 9, 0, 0, 42, 0, 0, 9, 45, 2, 6, 0, 39, 39, 3, 0, 39, 13, 13, 3 };
            var slopeItem1 = new int[] {  3, 0, 0, 0,  3, 3, 0, 0,  3, 0, 0, 3,  3, 1, 3, 0,  3,  3, 3, 0,  3,  1,  1, 1 };
            var slopeItem2 = new int[] { 11, 0, 0, 0, 15, 3, 0, 0, 14, 0, 0, 3, 15, 2, 2, 0, 13, 13, 1, 0, 13, 13, 13, 3 };
            var maxColIndex = 1383;
            var minColIndex = 1331;
            var maxRowIndex = 143;
            var minRowIndex = 65;
            for (int index = 0; index < slopeScore.Count(); index++)
            {
                result.Add(new FeatureVector(new Point(0, 0))
                {
                    Slope = new Tuple<int, int>(slopeItem1[index], slopeItem2[index]),
                    SlopeScore = slopeScore[index],
                    MaxColIndex = maxColIndex,
                    MinColIndex = minColIndex,
                    MaxRowIndex = maxRowIndex,
                    MinRowIndex = minRowIndex,

                });
            }
            return result;
        }

        public static List<FeatureVector> Brown_Cuckoodove1()
        {
            var result = new List<FeatureVector>();
            var slopeScore = new int[] { 3, 13, 3 };
            var slopeItem1 = new int[] { 1, 1, 1 };
            var slopeItem2 = new int[] { 3, 13, 3 };
            var maxColIndex = 2976;
            var minColIndex = 2937;
            var maxRowIndex = 245;
            var minRowIndex = 232;
            for (int index = 0; index < slopeScore.Count(); index++)
            {
                result.Add(new FeatureVector(new Point(0, 0))
                {
                    Slope = new Tuple<int, int>(slopeItem1[index], slopeItem2[index]),
                    SlopeScore = slopeScore[index],
                    MaxColIndex = maxColIndex,
                    MinColIndex = minColIndex,
                    MaxRowIndex = maxRowIndex,
                    MinRowIndex = minRowIndex,

                });
            }
            return result;
        }

        public static double[,] ReadCSVFile2Matrix(string csvFileName)
        {
            System.Tuple<List<string>, List<double[]>> tuple = CsvTools.ReadCSVFile(csvFileName);
            List<double[]> columns = tuple.Item2;
            int rows = columns[0].Length;
            int cols = columns.Count;
            double[,] matrix = new double[rows, cols];

            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    matrix[r, c] = columns[c][r];
                }
            }
            return matrix;
        }

        public static void featureVectorToCSV(List<Tuple<double, int, List<FeatureVector>>> listOfPositions, string filePath)
        {
            var results = new List<string>();
            var timeScale = 0.0116;
            results.Add("FrameNumber, Distance, SliceNumber, VectorDirection, Values");
            foreach (var lp in listOfPositions)
            {
                var listOfFeatureVector = lp.Item3;
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "HorizontalVector", listOfFeatureVector[sliceIndex].HorizontalVector));
                }
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "VerticalVector", listOfFeatureVector[sliceIndex].VerticalVector));
                }
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "PositiveDiagonalVector", listOfFeatureVector[sliceIndex].PositiveDiagonalVector));
                }
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(FeatureVectorItemToString(listOfFeatureVector[sliceIndex].TimePosition * timeScale, lp.Item1, sliceIndex, "NegativeDiagonalVector", listOfFeatureVector[sliceIndex].NegativeDiagonalVector));
                }
            }
            File.WriteAllLines(filePath, results.ToArray());
        }

        public static string FeatureVectorItemToString(double frameNumber, double distance, int sliceNumber, string vectorDirection, int[] values)
        {
            var sb = new StringBuilder(string.Format("{0}, {1}, {2}, {3}", frameNumber, distance, sliceNumber, vectorDirection));

            for (int index = 0; index < values.Length; index++)
            {
                sb.Append(",");
                sb.Append(values[index]);
            }
            return sb.ToString();
        }

        public static List<FeatureVector> GreyShrike_thrush4(double[,] matrix, double[,] matrix1, double[,] matrix2, double[,] matrix3)
        {
            var result = new List<FeatureVector>();
            var rowsCount = matrix.GetLength(0);
            var colsCount = matrix.GetLength(1);
            var colsCount1 = matrix2.GetLength(1);
            for (int i = 0; i < rowsCount; i++)
            {
                var horizontalVector = new int[colsCount];
                var verticalVector = new int[colsCount];
                for (int j = 0; j < colsCount; j++)
                {
                    horizontalVector[j] = (int)matrix[i, j];
                    verticalVector[j] = (int)matrix1[i, j];
                }
                var positiveDiagonalVector = new int[colsCount1];
                var negativeDiagonalVector = new int[colsCount1];
                for (int j = 0; j < colsCount1; j++)
                {
                    positiveDiagonalVector[j] = (int)matrix2[i, j];
                    negativeDiagonalVector[j] = (int)matrix3[i, j];
                }
                result.Add(new FeatureVector(new Point(0, 0))
                {
                    HorizontalVector = horizontalVector,
                    VerticalVector = verticalVector,
                    PositiveDiagonalVector = positiveDiagonalVector,
                    NegativeDiagonalVector = negativeDiagonalVector
                });
            }
            return result;

        }

        /// <summary>
        /// The Lewins' Rail template.
        /// </summary>
        /// <param name="frameOffset">
        /// The frameOffset is actually equal to the duration between two components.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Point> LewinsRailTemplate(int frameOffset)
        {
            var template = new List<Point>()
                               {
                                   // centeroid
                                   new Point(0, 0 - 23),
                                   new Point(0 + frameOffset, 0 - 23),
                                   new Point(0 - frameOffset, 0  - 23),
                                   new Point(0 + 2 * frameOffset, 0 - 23),                                  
                                   new Point(0 - 2 * frameOffset, 0 - 23),                                                                  

                                   new Point(0, 0),
                                   new Point(0 + frameOffset, 0),
                                   new Point(0 - frameOffset, 0),
                                   new Point(0 + 2 * frameOffset, 0),                                   
                                   new Point(0 - 2 * frameOffset, 0),                                

                                   new Point(0, 0  + 11),
                                   new Point(0 + frameOffset, 0 + 11),
                                   new Point(0 - frameOffset, 0 + 12),
                                   new Point(0 + 2 * frameOffset, 0 + 11),                                   
                                   new Point(0 - 2 * frameOffset, 0 + 11),

                                   new Point(0, 0  + 20),
                                   new Point(0 + frameOffset, 0 + 20),
                                   new Point(0 - frameOffset, 0 + 20),
                                   new Point(0 + 2 * frameOffset, 0 + 20),                                   
                                   new Point(0 - 2 * frameOffset, 0 + 20),
                               };

            return template;
        }

        /// <summary>
        /// The one type of honey eater template, this is an exact representation of the template.
        /// </summary>
        /// <param name="frameOffset">
        /// The frame offset.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<PointOfInterest> HoneryEaterExactTemplate(List<PointOfInterest> poiList, int rowsCount, int colsCount)
        {
            var Matrix = PointOfInterest.TransferPOIsToMatrix(poiList, rowsCount, colsCount);
            int numberOfLeft;
            int numberOfRight;
            //var miniFrequencyForAnchor = 89;  // 84 around 3000hz  257 - 84 = 173
            //var maxiFrequencyForAnchor = 173;  //168 around 6000hz  257 - 168 = 89
            //var miniIndexX = 10000;
            //var miniIndexY = 10000;
            var thresholdForNumberOfVerticle = 4;
            var thresholdForNumberOfHorizontal = 4;
            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < colsCount; col++)
                {
                    if (Matrix[row, col] == null) continue;
                    if (Matrix[row, col].OrientationCategory == 4)
                    {
                        numberOfLeft = 0;
                        numberOfRight = 0;
                        var neighbourhoodSize = 9;
                        // From the beginning, trying to find verticle lines
                        // search in a small neighbourhood
                        for (int rowIndex = 0; rowIndex <= neighbourhoodSize; rowIndex++)
                        {
                            for (int colIndex = 0; colIndex <= neighbourhoodSize / 2; colIndex++)
                            {
                                if ((row + rowIndex) < rowsCount && (col + colIndex) < colsCount)
                                {
                                    if ((Matrix[row + rowIndex, col + colIndex] != null) && (Matrix[row + rowIndex, col + colIndex].OrientationCategory == 4))
                                    {
                                        numberOfLeft++;
                                    }
                                }
                            }
                        }
                        // search on the right
                        var neighbourhoolSize2 = 9;
                        for (int rowIndex = neighbourhoodSize; rowIndex <= (neighbourhoolSize2 + neighbourhoodSize) / 2; rowIndex++)
                        {
                            for (int colIndex = neighbourhoodSize / 2; colIndex <= neighbourhoolSize2 + neighbourhoodSize; colIndex++)
                            {
                                if ((row + rowIndex) < rowsCount && (col + colIndex) < colsCount)
                                {
                                    if ((Matrix[row + rowIndex, col + colIndex] != null) && (Matrix[row + rowIndex, col + colIndex].OrientationCategory == 0))
                                    {
                                        numberOfRight++;
                                    }
                                }
                            }
                        }
                        if (numberOfLeft < thresholdForNumberOfVerticle || numberOfRight < thresholdForNumberOfHorizontal)
                        {
                            Matrix[row, col] = null;
                        }
                    }
                    else
                    {
                        Matrix[row, col] = null;
                    }
                }
            }

            return PointOfInterest.TransferPOIMatrix2List(Matrix);
        }

        ///<summary>
        ///A template of honey eater is represented with percentage byte Vector.
        /// </summary>
        /// <param name="neighbourhoodSize">
        /// it will determine the size of search area to get the feature vector.
        /// </param>
        public static FeatureVector HoneyeaterPercentageTemplate(int neighbourhoodSize)
        {
            var result = new FeatureVector(new Point(0, 0));
            var percentageFeatureVector = new double[4];
            percentageFeatureVector[0] = 0.4;// 0.5;
            percentageFeatureVector[1] = 0.4;// 0.4;
            percentageFeatureVector[2] = 0.0; //0.0;
            percentageFeatureVector[3] = 0.2;//0.1;
            //result.PercentageByteVector = percentageFeatureVector;
            result.NeighbourhoodSize = neighbourhoodSize;
            return result;
        }

        ///<summary>
        ///A template of honey eater is represented with direction byte vector.
        /// </summary>
        public static FeatureVector HoneyeaterDirectionByteTemplate()
        {
            var result = new FeatureVector(new Point(0, 0));
            // fuzzy presentation
            //var verticalByte = new int[]   { 4, 4, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //var horizontalByte = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 2, 4, 6 };
            var verticalBitByte = new int[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var horizontaBitlByte = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 };
            result.VerticalBitVector = verticalBitByte;
            result.HorizontalBitVector = horizontaBitlByte;
            result.NeighbourhoodSize = verticalBitByte.Count();
            return result;
        }

        /// <summary>
        /// The get centroid. It will calculate the centroid in a fixed area where many points are. 
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <returns>
        /// The <see cref="Point"/>.
        /// </returns>
        public static Point GetCentroid(List<Point> points)
        {
            var centeroid = new Point();
            var numberOfVertex = points.Count;
            var distance = new double[numberOfVertex];
            var minimumDistance = double.MaxValue;

            var minX = points.Min(p => p.X);
            var minY = points.Max(p => p.Y);
            var maxX = points.Min(p => p.X);
            var maxY = points.Max(p => p.Y);
            var centeroidX = (maxX + minX) / 2;
            var centeroidY = (maxY + minY) / 2;
            var tempCenteroid = new Point(centeroidX, centeroidY);

            // find the nearest point the to centeroid
            for (int j = 0; j < numberOfVertex; j++)
            {
                distance[j] = Distance.EuclideanDistanceForPoint(tempCenteroid, points[j]);
                if (distance[j] < minimumDistance)
                {
                    minimumDistance = distance[j];
                    centeroid = new Point(points[j].X, points[j].Y);
                }
            }

            return centeroid;
        }

        /// <summary>
        /// Convert from frequency to frequency bin.
        /// </summary>
        /// <param name="frequency">
        /// The frequency.
        /// </param>
        /// <param name="frequencyBinWidth">
        /// The frequency bin width.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int FrequencyToFrequencyBin(int frequency, double frequencyBinWidth)
        {
            return (int)(frequency / frequencyBinWidth);
        }

        /// <summary>
        /// Convert from million seconds to Frame index.
        /// </summary>
        /// <param name="millionSecond">
        /// The million second.
        /// </param>
        /// <param name="framePerSecond">
        /// The frame per second.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int MillionSecondsToFrame(int millionSecond, double framePerSecond)
        {
            var second = millionSecond / 1000;
            return (int)(second * framePerSecond);
        }

        /// <summary>
        /// The pixel per million second.
        /// </summary>
        /// <param name="framePerSecond">
        /// The frame Per Second.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int PixelPerMillionsecond(double framePerSecond)
        {
            const int SecondToMillionsecond = 1000;
            return (int)(framePerSecond / SecondToMillionsecond);
        }


    }
}
