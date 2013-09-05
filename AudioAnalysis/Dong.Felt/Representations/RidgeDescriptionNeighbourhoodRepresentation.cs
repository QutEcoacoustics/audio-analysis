using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Dong.Felt.Representations
{
    public class RidgeDescriptionNeighbourhoodRepresentation : NeighbourhoodRepresentation
    {
        #region Properties

        /// <summary>
        /// Gets or sets the dominant orientation type of the neighbourhood.
        /// </summary>
        public int dominantOrientationType { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) in the neighbourhood.
        /// </summary>
        public int dominantPOICount { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with dominant orientation in the neighbourhood.
        /// </summary>
        public double dominantMagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 1 of the neighbourhood.
        /// </summary>
        public int orientationType1 { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 2 of the neighbourhood.
        /// </summary>
        public int orientationType2 { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 3 of the neighbourhood.
        /// </summary>
        public int orientationType3 { get; set; }

        /// <summary>
        /// Gets or sets the orientation type 4 of the neighbourhood.
        /// </summary>
        public int orientationType4 { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 1 in the neighbourhood.
        /// </summary>
        public int orientationType1POICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 2 in the neighbourhood.
        /// </summary>
        public int orientationType2POICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 3 in the neighbourhood.
        /// </summary>
        public int orientationType3POICount { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with orentation type 4 in the neighbourhood.
        /// </summary>
        public int orientationType4POICount { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 1 in the neighbourhood.
        /// </summary>
        public double orentationType1MagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 2 in the neighbourhood.
        /// </summary>
        public double orentationType2MagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 3 in the neighbourhood.
        /// </summary>
        public double orentationType3MagnitudeSum { get; set; }

        /// <summary>
        /// Gets or sets the sum of the magnitude of pois with the orientation type 4 in the neighbourhood.
        /// </summary>
        public double orentationType4MagnitudeSum { get; set; }

        #endregion

        #region public method

        /// <summary>
        /// This method is used to get the dominantOrientationType, dominantPOICount, and  dominantMagnitudeSum of the neighbourhood, the neighbourhood is composed
        /// a matrix of PointOfInterest.
        /// </summary>
        /// <param name="neighbourhood">This is a fix neighbourhood which contains a list of points of interest.</param>
        /// <param name="PointX">This value is the X coordinate of centroid of neighbourhood.</param>
        /// <param name="PointY">This value is the Y coordinate of centroid of neighbourhood.</param>
        public void SetDominantNeighbourhoodRepresentation(PointOfInterest[,] neighbourhood, int pointX, int pointY)
        {
            var timeScale = 11.6; // ms
            var frequencyScale = 43.0; // hz           

            var ridgeNeighbourhoodFeatureVector = RectangularRepresentation.SliceRidgeRepresentation(neighbourhood, pointX, pointY);
            var ridgeDominantOrientationRepresentation = RectangularRepresentation.SliceMainSlopeRepresentation(ridgeNeighbourhoodFeatureVector);
            dominantOrientationType = ridgeDominantOrientationRepresentation.Item1;
            dominantPOICount = ridgeDominantOrientationRepresentation.Item2;
            
            int maximumRowIndex = neighbourhood.GetLength(0);
            int maximumColIndex = neighbourhood.GetLength(1);

            for (int rowIndex = 0; rowIndex < maximumRowIndex; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maximumColIndex; colIndex++)
                {
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == dominantOrientationType)
                    {
                        dominantMagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                }
            }
            
            // baseclass properties
            RowIndex = (int)(pointY * timeScale);
            ColIndex = (int)(pointX * frequencyScale);
            WidthPx = ridgeNeighbourhoodFeatureVector.neighbourhoodWidth;
            HeightPx = ridgeNeighbourhoodFeatureVector.neighbourhoodHeight;
            Duration = TimeSpan.FromMilliseconds(neighbourhood.GetLength(1) * timeScale);
            FrequencyRange = neighbourhood.GetLength(0) * frequencyScale;
        }

        /// <summary>
        /// This method is used for obtaining the general representation based on different orientations. 
        /// </summary>
        /// <param name="neighbourhood"></param>
        public void SetGeneralNeighbourhoodRepresentation(PointOfInterest[,] neighbourhood)
        {
            int maximumRowIndex = neighbourhood.GetLength(1);
            int maximumColIndex = neighbourhood.GetLength(2);

            for (int rowIndex = 0; rowIndex < maximumColIndex; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maximumColIndex; colIndex++)
                {
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                    {
                        orientationType1POICount++;
                        orentationType1MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthEast)
                    {
                        orientationType2POICount++;
                        orentationType2MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                    {
                        orientationType3POICount++;
                        orentationType3MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                    if (neighbourhood[rowIndex, colIndex].OrientationCategory == (int)Direction.NorthWest)
                    {
                        orientationType4POICount++;
                        orentationType4MagnitudeSum += neighbourhood[rowIndex, colIndex].RidgeMagnitude;
                    }
                }
            }
        }

        public static RidgeDescriptionNeighbourhoodRepresentation FromFeatureVector(PointOfInterest[,] matrix, int rowIndex, int colIndex)
        {
            var ridgeNeighbourhoodRepresentation = new RidgeDescriptionNeighbourhoodRepresentation();
            ridgeNeighbourhoodRepresentation.SetDominantNeighbourhoodRepresentation(matrix, rowIndex, colIndex); 
            return ridgeNeighbourhoodRepresentation;
        }

        public static RidgeNeighbourhoodFeatureVector ToFeatureVector(IEnumerable<string[]> lines)
        {
            return null;
        }

        public static RidgeDescriptionNeighbourhoodRepresentation FromNeighbourhoodCsv(IEnumerable<string> lines)
        {
            // assume csv file is laid out as we expect it to be.
            var listLines = lines.ToList();

            var nh = new RidgeDescriptionNeighbourhoodRepresentation()
            {               
                ColIndex = int.Parse(listLines[0]),
                RowIndex = int.Parse(listLines[1]),
                WidthPx = int.Parse(listLines[2]),
                HeightPx = int.Parse(listLines[3]),
                Duration = TimeSpan.FromMilliseconds(double.Parse(listLines[4])),
                FrequencyRange = double.Parse(listLines[5]),
                dominantOrientationType = int.Parse(listLines[6]),
                dominantPOICount = int.Parse(listLines[7]),
            };
            return nh;
        }

        public static RidgeDescriptionNeighbourhoodRepresentation FromRegionCsv(IEnumerable<string> lines)
        {
            // assume csv file is laid out as we expect it to be.
            var listLines = lines.ToList();

            var nh = new RidgeDescriptionNeighbourhoodRepresentation()
            {              
                ColIndex = (int)double.Parse(listLines[1]),
                RowIndex = int.Parse(listLines[2]),
                dominantOrientationType = int.Parse(listLines[3]),
                dominantPOICount = int.Parse(listLines[4]),
            };
            return nh;
        }

        //show ridge neighbourhood representation on image
        public static void RidgeNeighbourhoodRepresentationToImage(Graphics graphics, RidgeDescriptionNeighbourhoodRepresentation nhRepresentation)
        {
            int neighbourhoodLength = 13;
            int nhRadius = neighbourhoodLength / 2;
            int maxFrequencyBand = 256;
            int x = StatisticalAnalysis.MilliSecondsToFrameIndex(nhRepresentation.ColIndex);
            int y = maxFrequencyBand - StatisticalAnalysis.FrequencyToFruencyBandIndex(nhRepresentation.RowIndex);
            int dominantOrientationCategory = nhRepresentation.dominantOrientationType;
            int dominantPOICount = nhRepresentation.dominantPOICount;
            //int score = dominantOrientationCategory * dominantPOICount;
            var brush = new SolidBrush(Color.Black);
            if (dominantOrientationCategory == 1)
            {
                if (dominantPOICount <= neighbourhoodLength)
                {
                    //var brush = new SolidBrush(Color.Red);
                    graphics.FillRectangle(brush, x + nhRadius - dominantPOICount / 2, y - nhRadius, dominantPOICount, 1);
                }
                else
                {
                    if (dominantPOICount <= 2 * neighbourhoodLength)
                    {
                        //var brush = new SolidBrush(Color.Red);
                        graphics.FillRectangle(brush, x, y - nhRadius, neighbourhoodLength, 1);
                        graphics.FillRectangle(brush, x + nhRadius - (dominantPOICount - neighbourhoodLength) / 2, y - nhRadius + 1, dominantPOICount - neighbourhoodLength, 1);
                    }
                    else
                    {
                        if (dominantPOICount <= 3 * neighbourhoodLength)
                        {
                            //var brush = new SolidBrush(Color.Red);
                            graphics.FillRectangle(brush, x, y - nhRadius, 13, 1);
                            graphics.FillRectangle(brush, x, y - nhRadius + 1, 13, 1);
                            graphics.FillRectangle(brush, x + nhRadius - (dominantPOICount - 2 * neighbourhoodLength) / 2, y - nhRadius - 1, dominantPOICount - 2 * neighbourhoodLength, 1);
                        }
                    }
                }
            }
            else
            {
                if (dominantOrientationCategory == 2)
                {
                    if (dominantPOICount <= neighbourhoodLength)
                    {
                        //var brush = new SolidBrush(Color.Green);
                        graphics.FillRectangle(brush, x, y - 7, 1, dominantPOICount);
                    }
                    else
                    {
                        if (dominantPOICount <= 2 * neighbourhoodLength)
                        {
                            //var brush = new SolidBrush(Color.Green);
                            graphics.FillRectangle(brush, x, y - 7, 1, 13);
                            graphics.FillRectangle(brush, x - 1, y - 7, 1, dominantPOICount - 13);
                        }
                        else
                        {
                            if (dominantPOICount <= 3 * neighbourhoodLength)
                            {
                                //var brush = new SolidBrush(Color.Green);
                                graphics.FillRectangle(brush, x, y - 7, 1, 13);
                                graphics.FillRectangle(brush, x - 1, y - 7, 1, 13);
                                graphics.FillRectangle(brush, x + 1, y - 7, 1, dominantPOICount - 26);
                            }
                            
                        }
                    }
                }
                else
                {
                    if (dominantOrientationCategory == 3)
                    {
                        if (dominantPOICount <= neighbourhoodLength)
                        {
                            //var brush = new SolidBrush(Color.Blue);
                            graphics.FillRectangle(brush, x + nhRadius, y - nhRadius, 1, dominantPOICount);
                        }
                        else
                        {
                            if (dominantPOICount <= 2 * neighbourhoodLength)
                            {
                                //var brush = new SolidBrush(Color.Blue);
                                graphics.FillRectangle(brush, x + nhRadius, y - nhRadius, 1, neighbourhoodLength);
                                graphics.FillRectangle(brush, x + nhRadius - 1, y - nhRadius, 1, dominantPOICount - neighbourhoodLength);
                            }
                            else
                            {
                                if (dominantPOICount <= 3 * neighbourhoodLength)
                                {
                                    //var brush = new SolidBrush(Color.Blue);
                                    graphics.FillRectangle(brush, x + nhRadius, y - nhRadius, 1, neighbourhoodLength);
                                    graphics.FillRectangle(brush, x + nhRadius - 1, y - nhRadius, 1, dominantPOICount - neighbourhoodLength);
                                    graphics.FillRectangle(brush, x + nhRadius + 1, y - nhRadius, 1, dominantPOICount - 2 * neighbourhoodLength);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (dominantOrientationCategory == 4)
                        {
                            if (dominantPOICount <= 13)
                            {
                                //var brush = new SolidBrush(Color.Purple);
                                graphics.FillRectangle(brush, x, y - 7, 1, dominantPOICount);
                            }
                            else
                            {
                                if (dominantPOICount <= 26)
                                {
                                    //var brush = new SolidBrush(Color.Purple);
                                    graphics.FillRectangle(brush, x, y - 7, 1, 13);
                                    graphics.FillRectangle(brush, x - 1, y - 7, 1, dominantPOICount - 13);
                                }
                                else
                                {
                                    if (dominantPOICount <= 39)
                                    {
                                       // var brush = new SolidBrush(Color.Purple);
                                        graphics.FillRectangle(brush, x, y - 7, 1, 13);
                                        graphics.FillRectangle(brush, x - 1, y - 7, 1, 13);
                                        graphics.FillRectangle(brush, x + 1, y - 7, 1, dominantPOICount - 26);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
