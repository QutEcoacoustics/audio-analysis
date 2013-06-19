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
        public static List<PointOfInterest> HoneryEaterExactTemplate(List<PointOfInterest> poiList, int rowsCount,int colsCount)
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
            var result = new FeatureVector(new Point(0,0));
            var percentageFeatureVector = new double[4];
            percentageFeatureVector[0] = 0.4;// 0.5;
            percentageFeatureVector[1] = 0.4;// 0.4;
            percentageFeatureVector[2] = 0.0; //0.0;
            percentageFeatureVector[3] = 0.2;//0.1;
            result.PercentageByteVector = percentageFeatureVector;
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
            var verticalByte = new int[]   { 4, 4, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var horizontalByte = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 2, 4, 6 };
            result.VerticalByteVector = verticalByte;
            result.HorizontalByteVector = horizontalByte;
            result.NeighbourhoodSize = verticalByte.Count();
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
                distance[j] = Distance.EuclideanDistance(tempCenteroid, points[j]);
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
