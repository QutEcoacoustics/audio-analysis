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
        /// The centroid frequency.
        /// </summary>
        public static readonly int CentroidFrequencyOfLewinsRailTemplate = 91;
        public static readonly int CentroidFrequencyOfCrowTemplate = 80;


        ///<summary>
        ///general template for honey eater
        /// </summary>
        public static FeatureVector HoneyeaterTemplate(double[] feacutreVector)
        {
            var result = new FeatureVector(feacutreVector);
            result.PercentageBitVector = feacutreVector;
            return result;
        }

        ///<summary>
        ///general template for honey eater at bitFeatureVector
        /// </summary>
        public static FeatureVector HoneyeaterTemplate(int[] verticalBit, int[] horizontalBit)
        {
            var result = new FeatureVector(new Point(0,0));
            result.VerticalBitVector = verticalBit;
            result.HorizontalBitVector = horizontalBit;
            return result;
        }
        //public static FeatureVector CalculateSimilarityScore(FeatureVector instance, FeatureVector template)
        public static double CalculateSimilarityScoreForPercentagePresention(FeatureVector instance, FeatureVector template)
        {
            //var result = new FeatureVector(new Point(instance.point.X, instance.point.Y)) {PercentageBitVector = instance.PercentageBitVector};
            double similarityScore = 0.0;
            if (Math.Abs(instance.Vertical - template.PercentageBitVector[0]) < 0.1
                    && Math.Abs(instance.Horizontal - template.PercentageBitVector[1]) < 0.1
                    && Math.Abs(instance.PositiveDiagonal - template.PercentageBitVector[2]) < 0.1
                    && Math.Abs(instance.NegativeDiagonal - template.PercentageBitVector[3]) <= 0.2)
            {
                 similarityScore = 0.9;
            }
          
            return similarityScore;
        }

        public static double CalculateSimilarityScoreForBitPresentation(FeatureVector instance, FeatureVector template)
        {
            var count = instance.HorizontalBitVector.Count();

            double similarityScore = 0.0;
            var numberOfhorizontal = 0;
            var numberOfvertical = 0;
            for (int i = 0; i < count; i++)
            {
                if (Math.Abs(instance.HorizontalBitVector[i] - template.HorizontalBitVector[i]) < 7)
                {
                    numberOfhorizontal++;                    
                }
                if (Math.Abs(instance.VerticalBitVector[i] - template.VerticalBitVector[i]) < 3)
                {
                    numberOfvertical++;
                }
            }
            
            if (numberOfhorizontal > 9 && numberOfvertical > 9)
            {
                similarityScore = 1;
            }
            return similarityScore;
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
        /// The one type of honey eater template.
        /// </summary>
        /// <param name="frameOffset">
        /// The frame offset.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<PointOfInterest> UnknownTemplate(List<PointOfInterest> poiList, int rows,int cols)
        {        
            var M = PointOfInterest.TransferPOIsToMatrix(poiList, rows, cols);
            int numberOfLeft;
            int numberOfRight;
            //var miniFrequencyForAnchor = 89;  // 84 around 3000hz  257 - 84 = 173
            //var maxiFrequencyForAnchor = 173;  //168 around 6000hz  257 - 168 = 89
            //var miniIndexX = 10000;
            //var miniIndexY = 10000;
            var thresholdForNumberOfVerticle = 4;
            var thresholdForNumberOfHorizontal = 4;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (M[r, c] == null) continue;
                    if (M[r, c].OrientationCategory == 4)
                    {
                        numberOfLeft = 0;
                        numberOfRight = 0;
                        var neighbourhoodSize = 9;
                        // From the beginning, trying to find verticle lines
                        // search in a small neighbourhood
                        for (int i = 0; i <= neighbourhoodSize; i++)
                        {
                            for (int j = 0; j <= neighbourhoodSize / 2; j++)
                           {
                               if ((r + i) < rows && (c + j) < cols)
                               {
                                   if ((M[r + i, c + j] != null) && (M[r + i, c + j].OrientationCategory == 4))                                  
                                   { 
                                       numberOfLeft++; 
                                   }
                               }
                           }
                        }
                        // search on the right
                        var neighbourhoolSize2 = 9;
                        for (int i = neighbourhoodSize; i <= (neighbourhoolSize2 + neighbourhoodSize) / 2; i++)
                        {
                            for (int j = neighbourhoodSize / 2; j <= neighbourhoolSize2 + neighbourhoodSize; j++)
                            {
                                if ((r + i) < rows && (c + j) < cols)
                                {
                                    if ((M[r + i, c + j] != null) && (M[r + i, c + j].OrientationCategory == 0))
                                    {
                                        numberOfRight++;
                                    }
                                }
                            }
                        }
                        if (numberOfLeft < thresholdForNumberOfVerticle || numberOfRight < thresholdForNumberOfHorizontal)
                        {
                            M[r, c] = null;
                        }
                    }
                    else
                    {
                        M[r, c] = null;
                    }
                } // c
                
            } // for r loop

            return PointOfInterest.TransferPOIMatrix2List(M);
        // PruneAdjacentTracks()
        }

        
        /// <summary>
        /// The get centroid.
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
