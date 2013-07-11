namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using TowseyLib;
    using AudioAnalysisTools;
    using System.IO;

    public class StructureTensorTest
    {

        // using Microsoft.VisualStudio.TestTools.UnitTesting;
        public double[,] testMatrix1 = {{0.0, 0.0, 0.0, 0.0, 0.0},
                                        {0.0, 0.0, 0.0, 0.0, 0.0},
                                        {13.86, 18.03, 18.81, 14.9, 0.0},
                                        {6.66, 18.8, 16.69, 3.12, 0.0},
                                        {2.3, 18.0, 17.9, 5.9, 5.9}};
        //1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25
        public static int[,] testMatrixForRepresentation = {{5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //1
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //2 
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //3
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //4 
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //5
                                                            {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //6 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //7 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //8
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //9 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //10 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //11 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //12 
                                                            {5, 5, 5, 5, 5, 5, 4, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //13 
                                                            {5, 5, 5, 5, 6, 5, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //14
                                                            {5, 5, 5, 5, 5, 6, 4, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //15
                                                            {5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //16
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //17
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}, //18
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //19
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}, //20
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //21
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}, //22
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //23
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},  //24
                                                            {5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}}; //25
        // construct a fake bitmap
        public static Bitmap createNullBitmap()
        {
            int height = 4133;
            int width = 257;
            Bitmap bmp = new Bitmap(width, height);


            // Remember fillRectangular is more efficient than pixel

            Graphics g = Graphics.FromImage(bmp);
            // for the first 1000 frames

            Rectangle rect1 = new Rectangle(20, 500, 1, 30);
            // for the second 1000 frames
            Rectangle rect2 = new Rectangle(40, 1200, 200, 1);
            // for the third 1000 frames
            g.DrawEllipse(new Pen(Color.White), 200, 2500, 4, 4);

            // for the fourth 1000 frames
            Rectangle rect4 = new Rectangle(100, 3200, 100, 20);
            // for the fifth 1000 frames
            Rectangle rect5 = new Rectangle(200, 4002, 1, 1);
            SolidBrush brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, rect1);
            g.FillRectangle(brush, rect2);
            g.FillRectangle(brush, rect4);
            g.FillRectangle(brush, rect5);

            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    var currentColor = bmp.GetPixel(col, row);
                    if (currentColor == Color.FromArgb(0, 0, 0, 0))
                    {
                        bmp.SetPixel(col, row, Color.White);
                    }
                    else
                    {
                        bmp.SetPixel(col, row, Color.Black);
                    }

                }
            }
            return bmp;
        }
        /// <summary>
        /// Create a false pointOfinterest from a test edge matrix.  
        /// </summary>
        /// <param name="RepresentationMatrix"></param>
        /// <returns></returns>
        public static List<PointOfInterest> createFalsePoiFromMatrix(int[,] RepresentationMatrix)
        {
            var result = new List<PointOfInterest>();
            var rowCount = RepresentationMatrix.GetLength(0);
            var colCount = RepresentationMatrix.GetLength(1);

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    result.Add(new PointOfInterest(new Point(col, row)) { RidgeOrientation = RepresentationMatrix[row, col] });
                }
            }
            return result;
        }

        /// <summary>
        /// I am trying to represent the query, but it's not easy. From a box into a pointofInterest matrix.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static PointOfInterest[,] TransferPOIsToMatrix(List<PointOfInterest> list, int rows, int cols)
        {
            PointOfInterest[,] m = new PointOfInterest[rows, cols];
            foreach (PointOfInterest poi in list)
            {
                m[poi.Point.X, poi.Point.Y] = poi;
            }
            return m;
        }

        //public static void featureVectorToCSV(List<FeatureVector> listOfFeatureVector, string path)
        public static void featureVectorToCSV(List<Tuple<double, int, List<FeatureVector>>> listOfPositions)
        {
            var results = new List<string>();
            results.Add("FrameNumber, Distance, SliceNumber, HorizontalVector");
            foreach (var lp in listOfPositions)
            {
                //var newPath = Path.Combine(outputDirectory, " " + lp.Item2);
                var listOfFeatureVector = lp.Item3;
                for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
                {
                    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}",
                    listOfFeatureVector[sliceIndex].TimePosition * 0.0116, lp.Item1, sliceIndex, listOfFeatureVector[sliceIndex].HorizontalVector[0],
                    listOfFeatureVector[sliceIndex].HorizontalVector[1],
                    listOfFeatureVector[sliceIndex].HorizontalVector[2],
                    listOfFeatureVector[sliceIndex].HorizontalVector[3],
                    listOfFeatureVector[sliceIndex].HorizontalVector[4],
                    listOfFeatureVector[sliceIndex].HorizontalVector[5],
                    listOfFeatureVector[sliceIndex].HorizontalVector[6],
                    listOfFeatureVector[sliceIndex].HorizontalVector[7],
                    listOfFeatureVector[sliceIndex].HorizontalVector[8],
                    listOfFeatureVector[sliceIndex].HorizontalVector[9],
                    listOfFeatureVector[sliceIndex].HorizontalVector[10],
                    listOfFeatureVector[sliceIndex].HorizontalVector[11],
                    listOfFeatureVector[sliceIndex].HorizontalVector[12], " "));
                }
                File.WriteAllLines(@"C:\Test recordings\Output\Candidates-horizontalVector-improvedNeighbourhood.csv", results.ToArray());
                //StructureTensorTest.featureVectorToCSV(lp.Item3);
            }
            

            //var results1 = new List<string>();
            //results1.Add("SliceNumber, VerticalVector");
            //for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
            //{
            //    results1.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}",
            //    sliceIndex, listOfFeatureVector[sliceIndex].VerticalVector[0],
            //    listOfFeatureVector[sliceIndex].VerticalVector[1],
            //    listOfFeatureVector[sliceIndex].VerticalVector[2],
            //    listOfFeatureVector[sliceIndex].VerticalVector[3],
            //    listOfFeatureVector[sliceIndex].VerticalVector[4],
            //    listOfFeatureVector[sliceIndex].VerticalVector[5],
            //    listOfFeatureVector[sliceIndex].VerticalVector[6],
            //    listOfFeatureVector[sliceIndex].VerticalVector[7],
            //    listOfFeatureVector[sliceIndex].VerticalVector[8],
            //    listOfFeatureVector[sliceIndex].VerticalVector[9],
            //    listOfFeatureVector[sliceIndex].VerticalVector[10],
            //    listOfFeatureVector[sliceIndex].VerticalVector[11],
            //    listOfFeatureVector[sliceIndex].VerticalVector[12], " "));

            //}
            //File.WriteAllLines(@"C:\Test recordings\Output\CandidatesVerticalFeatureVector.csv", results1.ToArray());

            //var results2 = new List<string>();
            //results2.Add("SliceNumber, PositiveDiagonalVector");
            //for (var sliceIndex = 0; sliceIndex < listOfFeatureVector.Count(); sliceIndex++)
            //{
            //    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20},                       {21}, {22}, {23}, {24}, {25}, {26}",
            //    sliceIndex, listOfFeatureVector[sliceIndex].PositiveDiagonalVector[0],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[1],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[2],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[3],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[4],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[5],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[6],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[7],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[8],
            //                listOfFeatureVector[sliceIndex].PositiveDiagonalVector[9],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[10],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[11],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[12],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[13],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[14],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[15],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[16],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[17],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[18],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[19],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[20],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[21],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[22],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[23],
            //    listOfFeatureVector[sliceIndex].PositiveDiagonalVector[24], " "));
            //}
            //File.WriteAllLines(@"C:\Test recordings\Output\CandidatesPositiveDiagonalFeatureVector.csv", results2.ToArray());
            //results.Add("SliceNumber, NegativeDiagonalVector");
            //for (var sliceIndex = 0; sliceIndex < queryFeatureVector.Count(); sliceIndex++)
            //{
            //    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20},                       {21}, {22}, {23}, {24}, {25}, {26}",
            //    sliceIndex, queryFeatureVector[sliceIndex].NegativeDiagonalVector[0],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[1],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[2],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[3],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[4],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[5],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[6],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[7],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[8],
            //                queryFeatureVector[sliceIndex].NegativeDiagonalVector[9],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[10],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[11],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[12],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[13],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[14],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[15],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[16],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[17],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[18],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[19],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[20],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[21],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[22],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[23],
            //    queryFeatureVector[sliceIndex].NegativeDiagonalVector[24], " "));

            //}
            //File.WriteAllLines(@"C:\Test recordings\Output\queryNegativeDiagonalFeatureVector.csv", results.ToArray());

            //for (var sliceIndex = 0; sliceIndex < queryFeatureVector.Count(); sliceIndex++)
            //{
            //    results.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}",
            //    sliceIndex, queryFeatureVector[sliceIndex].VerticalVector[0],
            //    queryFeatureVector[sliceIndex].VerticalVector[1],
            //    queryFeatureVector[sliceIndex].VerticalVector[2],
            //    queryFeatureVector[sliceIndex].VerticalVector[3],
            //    queryFeatureVector[sliceIndex].VerticalVector[4],
            //    queryFeatureVector[sliceIndex].VerticalVector[5],
            //    queryFeatureVector[sliceIndex].VerticalVector[6],
            //    queryFeatureVector[sliceIndex].VerticalVector[7],
            //    queryFeatureVector[sliceIndex].VerticalVector[8],
            //    queryFeatureVector[sliceIndex].VerticalVector[9],
            //    queryFeatureVector[sliceIndex].VerticalVector[10],
            //    queryFeatureVector[sliceIndex].VerticalVector[11],
            //    queryFeatureVector[sliceIndex].VerticalVector[12], " "));

            //}
            //File.WriteAllLines(@"C:\Test recordings\Output\queryVerticalFeatureVector.csv", results.ToArray());
        }



    }
}
