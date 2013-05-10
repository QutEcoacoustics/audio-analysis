// --------------------------------------------------------------------------------------------------------------------
// <copyright file="POI.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the AcousticEventDetection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.Extensions;
    using AudioAnalysisTools;
    using TowseyLib;
    using AForge.Imaging.Filters;
    using Accord.Math.Decompositions;

    // several types of points of interest
    public enum FeatureType { NONE, LOCALMAXIMA, STRUCTURE_TENSOR}
    
    /// <summary>
    /// The points of interest detection.
    /// </summary>
    public class PoiAnalysis
    {
        private static readonly SonogramConfig StandardConfig = new SonogramConfig();
        // A 7 * 7 gaussian blur
        public static double[,] gaussianBlur7 = {{0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00038771,	0.01330373,	0.11098164,	0.22508352,	0.11098164,	0.01330373,	0.00038771},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067}};

        // A 5 * 5 gaussian blur
        public static double[,] gaussianBlur5 = {{0.0000,       0.0000,     0.0002,     0.0000,    0.0000},
                                                 {0.0000,       0.0113,     0.0837,     0.0113,    0.0000},
                                                 {0.0002,       0.0837,     0.6187,     0.0837,    0.0002},
                                                 {0.0000,       0.0113,     0.0837,     0.0113,    0.0000},
                                                 {0.0000,       0.0000,     0.0002,     0.0000,    0.0000}};

        // It has a kernel which is 3 * 3, and all values is equal to 1. 
        public static GaussianBlur filter = new GaussianBlur(4, 1);
        
        /// <summary>
        /// AudioToSpectrogram transforms an audio to a spectrogram. 
        /// </summary>
        /// <param name="wavFilePath">
        /// get an audio file through "wavFilePath". 
        /// </param>
        /// <param name="recording">
        /// The recording.
        /// </param>
        /// <returns>
        /// return a spectrogram.
        /// </returns>
        public static SpectralSonogram AudioToSpectrogram(string wavFilePath, out AudioRecording recording)
        {
            recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };
            var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
            return spectrogram;
        }

        /// <summary>
        /// Reduce the pink noise. 
        /// the result could be a binary spectrogram or original spectrogram.
        /// </summary>
        /// <param name="spectralSonogram">
        /// The spectral sonogram.
        /// </param>
        /// <param name="backgroundThreshold">
        /// The background Threshold.
        /// </param>
        /// <param name="makeBinary">
        /// To make the spectrogram into a binary image.
        /// </param>
        /// <param name="changeOriginalData">
        /// The change Original Data.
        /// </param>
        /// <returns>
        /// return a tuple composed of each pixel's amplitude at each coordinates and  smoothArray after the noise removal.
        /// </returns>
        public static double[,] NoiseReductionToBinarySpectrogram(
            SpectralSonogram spectralSonogram, double backgroundThreshold, bool makeBinary = false, bool changeOriginalData = false)
        {
            double[,] result = spectralSonogram.Data;

            if (makeBinary)
            {
                return SNR.NoiseReduce(result, NoiseReductionType.BINARY, backgroundThreshold).Item1;
            }
            else
            {
                if (changeOriginalData)
                {
                    spectralSonogram.Data = SNR.NoiseReduce(result, NoiseReductionType.STANDARD, backgroundThreshold).Item1;
                    return spectralSonogram.Data;
                }
                else
                {
                    return SNR.NoiseReduce(result, NoiseReductionType.STANDARD, backgroundThreshold).Item1;
                }
            }
        }

        /// <summary>
        /// Extract a particular type of points of interest
        /// </summary>
        /// <param name="matrix">
        /// the original spectrogram/image data 
        /// </param>
        /// <param name="ft">
        /// a pariticular type of feature 
        /// </param>
        /// <returns>
        /// return a list of Points of Interest
        /// </returns>
        public static List<PointOfInterest> ExactPointsOfInterest(double[,] matrix, FeatureType ft)
        {
            var result = new List<PointOfInterest>();
            if (ft == FeatureType.LOCALMAXIMA)
            {
                result = HitLocalMaxima(matrix);          
            }
            else if (ft == FeatureType.STRUCTURE_TENSOR)
            {
                result = HitStructureTensor(matrix);
            }

            return result;
        }
        
        /// <summary>
        /// get a list of localMaxima hit 
        /// </summary>
        /// <param name="matrix">
        /// the original spectrogram/image data 
        /// </param>
        /// <returns>
        /// return a list of points of interest 
        /// </returns>
        public static List<PointOfInterest> HitLocalMaxima(double[,] matrix)
        {
            var result = new List<PointOfInterest>();

            // Find the local Maxima
            const int NeibourhoodWindowSize = 7;
            var localMaxima = PickLocalMaximum(matrix, NeibourhoodWindowSize);

            // Filter out points
            const int AmplitudeThreshold = 10;
            var filterOutPoints = FilterOutPoints(localMaxima, AmplitudeThreshold); // pink noise model threshold                

            // Remove points which are too close
            const int DistanceThreshold = 7;
            var finalPoi = RemoveClosePoints(filterOutPoints, DistanceThreshold);

            // Calculate the distance between poi and points in the template
            //var avgDistanceScores = AverageDistanceScores(TemplateTools.LewinsRailTemplate(17), finalPois);

            // Get the metched anchor point (centroid)
            //const double AvgDistanceScoreThreshold = 5;
            //var matchedPoi = PoiAnalysis.MatchedPointsOfInterest(finalPois, avgDistanceScores, AvgDistanceScoreThreshold);

            return result = finalPoi;
        }

        /// <summary>
        ///  get a list of structure Tensor hit 
        /// </summary>
        /// <param name="matrix">
        /// the original spectrogram/image data
        /// </param>
        /// <returns>
        /// return a list of points of interest 
        /// </returns>
        public static List<PointOfInterest> HitStructureTensor(double[,] matrix)
        {
            var result = new List<PointOfInterest>();

            var differenceOfGaussian = GetDifferenceOfGaussian(gaussianBlur5);
            var partialDifference = DoGPartialDifference(matrix, differenceOfGaussian.Item1, differenceOfGaussian.Item2);         
            var structureTensor = StructureTensor(partialDifference.Item1, partialDifference.Item2);
            var eigenValueDecomposition = EignvalueDecomposition(structureTensor);         
            var attention = GetAttention(eigenValueDecomposition);          
            var pointsOfInterst = ExactPointsOfInterest(attention);
           
            return result = pointsOfInterst; 
        }

        /// <summary>
        /// Pick the local maxima in a neighborhood,which means the maxima has an intensity peak.
        /// </summary>
        /// <param name="m">
        /// The m means a matrix which represents the coordinates of pixels in the spectrogram.
        /// </param>
        /// <param name="neighborWindowSize">
        /// The neighbor window size. It better be an odd number, like 3 or 5. 
        /// </param>
        /// <returns>
        /// return a list of PointOfInterest.
        /// </returns>
        public static List<PointOfInterest> PickLocalMaximum(double[,] m, int WindowSize)
        {
            Contract.Requires(WindowSize % 2 == 1, "Neighbourhood window size must be odd");
            Contract.Requires(WindowSize >= 1, "Neighbourhood window size must be at least 1");
            int centerOffset = -(int)(WindowSize / 2.0);
            var results = new List<PointOfInterest>();

            // scan the whole matrix, if want to scan a fixed part of matrix, the range of row and col might be changed.
            // e.g row ~ (m.GetLength(1) / 4 - 2 * m.GetLength(1) / 5), col ~ (3096 - 3180)
            for (int row = 0; row < m.GetLength(0); row++)
            {
                for (int col = 0; col < m.GetLength(1); col++)
                {
                    // assume it's a local maxium
                    double localMaximum = m[row, col];
                    var maximum = true;
                   
                    // check if it is really the local maximum in the neighbourhood
                    for (int i = centerOffset; i <= -centerOffset; i++)
                    {
                        for (int j = centerOffset; j <= -centerOffset; j++)
                        {
                            // check if it is out of range of m
                            if (m.PointIntersect(row + i, col + j))
                            {                              
                                var current = m[row + i, col + j];

                                // don't check the middle point
                                if (i == 0 && j == 0)
                                {
                                    if (!(Math.Abs(current - 0.0) > 0.01))
                                    {
                                        maximum = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (localMaximum < current)
                                    {
                                        // actually not a local maximum
                                        maximum = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                  
                    // if it is indeed the local maximum, then add it
                    if (maximum)
                    {
                        results.Add(new PointOfInterest(new Point(row, col)) { Intensity = m[row, col] });
                    }
                }
            }

            return results;
        }
      
        /// <summary>
        /// The filter out points whose amplitude value is less than a threshold.
        /// </summary>
        /// <param name="list">
        /// The list of PointOfInterest needs to be filter out.
        /// </param>
        /// <param name="amplitudeThreshold">
        /// The amplitude Threshold.
        /// </param>
        /// <returns>
        /// Return a list of PointOfInterest.
        /// </returns>
        public static List<PointOfInterest> FilterOutPoints(List<PointOfInterest> list, int amplitudeThreshold)
        {
            return list.Where(item => item.Intensity > amplitudeThreshold).ToList();
        }

        /// <summary>
        /// To remove points which are too close. 
        /// e.g. there are two close points, require to check whose amplitude is higher, then keep this one.
        /// </summary>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <returns>
        /// The list of PointOfInterest after remove too close points.
        /// </returns>
        public static List<PointOfInterest> RemoveClosePoints(List<PointOfInterest> pointsOfInterest, int offset)
        {
            var maxIndex = pointsOfInterest.Count;
            
            for (int i = 0; i < maxIndex; i++)
            {
                var poi = pointsOfInterest;
                for (int j = 0; j < maxIndex; j++)
                {
                    if (poi[i] == poi[j])
                    {
                        continue;
                    }
                    else
                    {
                        int deltaX = Math.Abs(poi[i].Point.X - poi[j].Point.X);
                        int deltaY = Math.Abs(poi[i].Point.Y - poi[j].Point.Y);

                        // if they are close, check whose power is strong
                        if (deltaX < offset && deltaY < offset)
                        {
                            if (poi[j].Intensity >= poi[i].Intensity)
                            {
                                pointsOfInterest.Remove(poi[i]);
                                maxIndex = pointsOfInterest.Count();
                            }
                            else
                            {
                                pointsOfInterest.Remove(poi[j]);
                                maxIndex = pointsOfInterest.Count();
                            }
                        }
                    }                    
                }
            }

            return pointsOfInterest; 
        }
              
        /// <summary>
        /// The average distance score.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <returns>
        /// The Average Distance Score for each poi to the template point. 
        /// </returns>
        public static double[] AverageDistanceScores(List<Point> template, List<PointOfInterest> pointsOfInterest)
        {            
            var numberOfVertexes = template.Count;
            var distance = new double[pointsOfInterest.Count];
            var minimumDistance = new double[numberOfVertexes];
            var avgDistanceScores = new double[pointsOfInterest.Count];
            var sum = 0.0;

            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                var poi = pointsOfInterest;               
                Point centeroid = poi[i].Point;
                var absoluteTemplate = GetAbsoluteTemplate(centeroid, template);

                for (int j = 0; j < numberOfVertexes; j++)
                {

                    for (int index = 0; index < poi.Count; index++)
                    {
                        // distance[index] = EuclideanDistance(absoluteTemplate[j], poi[index].Point);
                        distance[index] = ManhattanDistance(absoluteTemplate[j], poi[index].Point);
                    }
                    minimumDistance[j] = distance.Min();
                }

                for (int k = 0; k < numberOfVertexes; k++)
                {
                     sum += minimumDistance[k];
                }

                avgDistanceScores[i] = sum / numberOfVertexes;
                sum = 0.0;              
            }

            return avgDistanceScores;
        }

        /// <summary>
        /// The matched points of interest.
        /// </summary>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <param name="averageDistanceScores">
        /// The average Distance Scores.
        /// </param>
        /// <param name="threshold">
        /// The threshold.
        /// </param>
        /// <returns>
        /// The list of PointOfInterest.
        /// </returns>
        public static List<PointOfInterest> MatchedPointsOfInterest(
            List<PointOfInterest> pointsOfInterest, double[] averageDistanceScores, double threshold)
        {
            var numberOfVertex = pointsOfInterest.Count;
            var result = new List<PointOfInterest>();

            for (int i = 0; i < numberOfVertex; i++)
            {
                if (averageDistanceScores[i] < threshold)
                {
                    var poi = pointsOfInterest[i];
                    
                    // fix possible hit points around a fixed frequency-the frequency of LewinsRailTemplate's centroid   
                    var frequencyOffset = Math.Abs(poi.Point.Y - TemplateTools.CentroidFrequencyOfLewinsRailTemplate);

                    // probably need to fix this magic number
                    if (frequencyOffset < 6)
                    {
                        poi.DrawColor = PointOfInterest.HitsColor;
                    }                  
                    result.Add(poi);
                }
            }

            return result;
        }

        //public static List<PointOfInterest> FilterPoints(double[,] m)
        //{
        //    int MaximumXIndex = m.GetLength(0);
        //    int MaximumYIndex = m.GetLength(1);
        //    var result = new List<PointOfInterest>();

        //    for (int row = 0; row < MaximumXIndex - 1; row++)
        //    {
        //        for (int col = 0; col < MaximumYIndex - 1; col++)
        //        {
        //            var rightPosition = m[row + 1, col];
        //            var bottomPostion = m[row, col + 1];
        //            var current = m[row, col];
        //            var p1 = (current != 0.0);
        //            //var p2 = (rightPosition != 0.0);
        //            //var p3 = (bottomPostion != 0.0);
        //            if (p1)
        //            //if (p1 && p2 && p3)
        //            {
        //                result.Add(new PointOfInterest(new Point(row, col)) { Intensity = m[row, col] });
        //            }
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// The get the difference of Gaussian .
        /// </summary>
        /// <param name="gaussianBlur">
        /// a particular gaussianBlur
        /// </param>
        /// <returns>
        /// A tuple of DifferenceX and DifferenceY of Gaussian 
        /// </returns>
        public static Tuple<double[,], double[,]> GetDifferenceOfGaussian(double[,] gaussianBlur)
        {
            var maskSize = gaussianBlur.GetLength(0);

            var gLeft = new double[maskSize, maskSize + 1];
            var gRight = new double[maskSize, maskSize + 1];

            var gTop = new double[maskSize + 1, maskSize];
            var gBottom = new double[maskSize + 1, maskSize];

            var gDifferenceLR = new double[maskSize, maskSize + 1];
            var gDifferenceTB = new double[maskSize + 1, maskSize];

            var DifferenceOfGaussianX = new double[maskSize, maskSize];
            var DifferenceOfGaussianY = new double[maskSize, maskSize];

            for (int i = 1; i < maskSize + 1; i++)
            {
                for (int j = 1; j < maskSize + 1; j++)
                {
                    gLeft[i - 1, j - 1] = gaussianBlur[i - 1, j - 1];
                    gRight[i - 1, j] = gaussianBlur[i - 1, j - 1];
                    gTop[i - 1, j - 1] = gaussianBlur[i - 1, j - 1];
                    gBottom[i, j - 1] = gaussianBlur[i - 1, j - 1];
                }
            }

            for (int i = 0; i < maskSize; i++)
            {
                gLeft[i, maskSize] = 0.0;
                gRight[i, 0] = 0.0;
                gTop[maskSize,i] = 0.0;
                gBottom[0,i] = 0.0;
            } 
          
            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize + 1; j++)
                {
                    gDifferenceLR[i,j] = gLeft[i,j] - gRight[i,j];                 
                }
            }

            for (int i = 0; i < maskSize + 1; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    gDifferenceTB[i,j] = gTop[i,j] - gBottom[i,j];                  
                }
            }

            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    DifferenceOfGaussianX[i, j] = gDifferenceLR[i, j];
                    DifferenceOfGaussianY[i, j] = gDifferenceTB[i, j];
                }
            }

            var result = Tuple.Create(DifferenceOfGaussianX, DifferenceOfGaussianY);
            return result;
        }

        /// <summary>
        /// The get the partial difference in a neighbourhood by using difference of Gaussian .
        /// </summary>
        /// <param name="m">
        /// the original spectrogram / image data
        /// </param>
        /// <param name="differenceOfGaussianX">
        /// the difference of GaussianX
        /// </param>
        /// /// <param name="differenceOfGaussianY">
        /// the difference of GaussianY
        /// </param>
        /// <returns>
        /// A tuple of DifferenceX and DifferenceY of Gaussian 
        /// </returns>
        public static Tuple<double[,], double[,]> DoGPartialDifference(double[,] m, double[,] differenceOfGaussianX, double[,] differenceOfGaussianY)
        {
            var sizeOfGaussianBlur = differenceOfGaussianX.GetLength(0);
            var centerOffset = (int)(sizeOfGaussianBlur / 2);

            int MaximumXIndex = m.GetLength(0);
            int MaximumYIndex = m.GetLength(1);

            var partialDifferenceX = new double[MaximumXIndex, MaximumYIndex];
            var partialDifferenceY = new double[MaximumXIndex, MaximumYIndex];

            // Because the convolution class can only process the kernel with int[,] , here still use loops to do the convolution  
            for (int row = 0; row < MaximumXIndex - 1; row++)
            {
                for (int col = 0; col < MaximumYIndex - 1; col++)
                {
    
                    // check whether the current point can be in the center of gaussian blur 
                    for (int i = -centerOffset; i <= centerOffset; i++)
                    {
                        for (int j = -centerOffset; j <= centerOffset; j++)
                        {
                            // check whether it's in the range of partialDifferenceX
                            if (m.PointIntersect(row + j, col - i))
                            {
                                partialDifferenceX[row, col] = partialDifferenceX[row, col] + differenceOfGaussianX[i + centerOffset, j + centerOffset] * m[row + j, col - i];
                                partialDifferenceY[row, col] = partialDifferenceY[row, col] + differenceOfGaussianY[i + centerOffset, j + centerOffset] * m[row + j, col - i];
                            }
                        }
                    }
                }
            }

            var result = Tuple.Create(partialDifferenceX, partialDifferenceY);
            return result;
        }

        /// <summary>
        /// Calculate the magnitude of partialDifference.
        /// </summary>
        /// <param name="differenceOfGaussianX">
        /// the partialDifferenceX
        /// </param>
        /// /// <param name="differenceOfGaussianY">
        /// the particalDifferenceY
        /// </param>
        /// <returns>
        /// return the magnitude of the partical difference for each point
        /// </returns>
        public static double[,] MagnitudeOfPartialDifference(double[,] paritialDifferenceX, double[,] partialDifferenceY)
        {
            var MaximumXIndex = paritialDifferenceX.GetLength(0);
            var MaximumYIndex = paritialDifferenceX.GetLength(1);

            var result = new double[MaximumXIndex, MaximumYIndex];

            for (int i = 0; i < MaximumXIndex; i++)
            {
                for (int j = 0; j < MaximumYIndex; j++)
                {
                    result[i, j] = Math.Sqrt(Math.Pow(paritialDifferenceX[i, j], 2) + Math.Pow(partialDifferenceY[i, j], 2));
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate the phase of partialDifference.
        /// </summary>
        /// <param name="differenceOfGaussianX">
        /// the partialDifferenceX
        /// </param>
        /// /// <param name="differenceOfGaussianY">
        /// the particalDifferenceY
        /// </param>
        /// <returns>
        /// return the phase of partial difference for each point
        /// </returns>
        public static double[,] PhaseOfPartialDifference(double[,] paritialDifferenceX, double[,] partialDifferenceY)
        {
            var MaximumXIndex = paritialDifferenceX.GetLength(0);
            var MaximumYIndex = paritialDifferenceX.GetLength(1);

            var result = new double[MaximumXIndex, MaximumYIndex];

            for (int i = 0; i < MaximumXIndex; i++)
            {
                for (int j = 0; j < MaximumYIndex; j++)
                {
                    result[i, j] = Math.Atan2(partialDifferenceY[i, j], paritialDifferenceX[i, j]);
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate the structure tensor.
        /// </summary>
        /// <param name="partialDifferenceX">
        /// the partialDifferenceX
        /// </param>
        /// /// <param name="partialDifferenceY">
        /// the partialDifferenceY
        /// </param>
        /// <returns>
        /// return the structure tensor for each point
        /// </returns>
        public static List<Tuple<PointOfInterest, double[,]>> StructureTensor(double[,] partialDifferenceX, double[,] partialDifferenceY)
        {
            var rowMaximumIndex = partialDifferenceX.GetLongLength(0);
            var colMaximumIndex = partialDifferenceX.GetLongLength(1);

            var result = new List<Tuple<PointOfInterest, double[,]>>();

            for (int row = 0; row < rowMaximumIndex; row++)
            {
                for (int col = 0; col < colMaximumIndex; col++)
                {
                    var structureTensor = new double[2, 2];

                    structureTensor[0, 0] = Math.Pow(partialDifferenceX[row, col], 2);
                    structureTensor[0, 1] = partialDifferenceX[row, col] * partialDifferenceY[row, col];
                    structureTensor[1, 0] = partialDifferenceX[row, col] * partialDifferenceY[row, col];
                    structureTensor[1, 1] = Math.Pow(partialDifferenceY[row, col], 2);

                    result.Add(Tuple.Create(new PointOfInterest(new Point(row, col)), structureTensor));
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate the difference between the current pixel and its neighborhood pixel.
        /// </summary>
        /// <param name="m">
        /// the original spectrogram / image data
        /// </param>
        /// <returns>
        /// A tuple of partialDifferenceX and partialDifferenceY
        /// </returns>  
        public static Tuple<double[,], double[,]> PartialDifference(double[,] m)
        {
            int MaximumXIndex = m.GetLength(0);
            int MaximumYIndex = m.GetLength(1);

            var partialDifferenceX = new double[MaximumXIndex, MaximumYIndex];
            var partialDifferenceY = new double[MaximumXIndex, MaximumYIndex];

            for (int row = 0; row < MaximumXIndex - 1; row++)
            {
                for (int col = 0; col < MaximumYIndex - 1; col++)
                {
                    partialDifferenceX[row, col] = m[row + 1, col] - m[row, col];
                    partialDifferenceY[row, col] = m[row, col + 1] - m[row, col];
                }
            }
            //PointF
            var result = Tuple.Create(partialDifferenceX, partialDifferenceY);
            return result;
        }

        /// <summary>
        /// Calculate the structure tensor with GaussianBlur.
        /// </summary>
        /// <param name="gaussianBlur">
        /// a particular gaussianblur
        /// </param>
        /// <param name="partialDifferenceX">
        /// the partialDifferenceX
        /// </param>
        /// /// <param name="partialDifferenceY">
        /// the partialDifferenceY
        /// </param>
        /// <returns>
        /// return the structure tensor for each point
        /// </returns>
        public static List<Tuple<PointOfInterest, double[,]>> GaussianStructureTensor(double[,] gaussianBlur, double[,] partialDifferenceX, double[,] partialDifferenceY)
        {       
            var sizeOfGaussianBlur = Math.Max(gaussianBlur.GetLength(0), gaussianBlur.GetLength(1));
            var centerOffset = (int)(sizeOfGaussianBlur/2);
            var result = new List<Tuple<PointOfInterest, double[,]>>();

            for (int row = 0; row < partialDifferenceX.GetLength(0); row++)
            {
                for (int col = 0; col < partialDifferenceX.GetLength(1); col++)
                {
                    var structureTensor = new double[2, 2];
                    var sumTopLeft = 0.0;
                    var sumDiagonal = 0.0;
                    var sumBottomRight = 0.0;

                    // check whether the current point is in the center of gaussian blur 
                    for (int i = -centerOffset; i <= centerOffset; i++)
                    {
                         for (int j = -centerOffset; j <= centerOffset; j++)
                         {
                              // check whether it's in the range of partialDifferenceX
                              if (partialDifferenceX.PointIntersect(row + j, col - i))
                              {
                                  sumTopLeft = sumTopLeft + gaussianBlur[i + centerOffset, j + centerOffset] * Math.Pow(partialDifferenceX[row + j, col - i], 2);
                              }
                                
                              // check whether it's in the range of partialDifferenceX
                              if (partialDifferenceY.PointIntersect(row + j, col - i))
                              {
                                    sumDiagonal = sumDiagonal + gaussianBlur[i + centerOffset, j + centerOffset] * partialDifferenceX[row + j, col - i] * partialDifferenceY[row + j, col - i];
                                    sumBottomRight = sumBottomRight + gaussianBlur[i + centerOffset, j + centerOffset] * Math.Pow(partialDifferenceY[row + j, col - i], 2);
                              }
                         }
                    }

                    structureTensor[0, 0] = sumTopLeft;
                    structureTensor[0, 1] = sumDiagonal;
                    structureTensor[1, 0] = sumDiagonal;
                    structureTensor[1, 1] = sumBottomRight;

                    result.Add(Tuple.Create(new PointOfInterest(new Point(row, col)), structureTensor));
                   // col = col + 3;                    
                }            
            } 
          
            return result;
        }

        /// <summary>
        /// Calculate the mean structure tensor.
        /// </summary>
        /// <param name="structureTensor">
        /// the structureTensor
        /// </param>
        /// <param name="windowSize">
        /// calculate the mean structure tensor in the neighbourhood, it will give the size of neighbourhood 
        /// </param> 
        /// <returns>
        /// return the structure tensor for each point
        /// </returns>
        public static List<Tuple<PointOfInterest, double[,]>> MeanOfStructureTensor(List<Tuple<PointOfInterest, double[,]>> structureTensor, int windowSize)
        {
            var LengthOfStructureTensor = structureTensor.Count;
            var rowMaximumIndex = structureTensor[LengthOfStructureTensor - 1].Item1.Point.X;
            var colMaximumIndex = structureTensor[LengthOfStructureTensor - 1].Item1.Point.Y;

            var centerOffset = (int)windowSize / 2;
            var result = new List<Tuple<PointOfInterest, double[,]>>();

            foreach (var st in structureTensor)
            {
                var newSt = new double[2, 2];
                var sumStX = 0.0;
                var sumStDiagonal = 0.0;
                var sumStY = 0.0;
                for (int i = -centerOffset; i <= centerOffset; i++)
                {
                    for (int j = -centerOffset; j <= centerOffset; j++)
                    {
                        var xRange = st.Item1.Point.X + i;
                        var yRange = st.Item1.Point.Y + j;

                        if (xRange >= 0 && xRange <= rowMaximumIndex && yRange >= 0 && yRange <= colMaximumIndex)
                        {
                            sumStX = sumStX + st.Item2[0, 0];
                            sumStX = sumStX + st.Item2[1, 1];
                            sumStDiagonal = st.Item2[0, 1];
                        }
                    }
                }
                var averageStX = sumStX / Math.Pow(windowSize, 2);
                var averageStDiagonal = sumStDiagonal / Math.Pow(windowSize, 2);
                var averageStY = sumStY / Math.Pow(windowSize, 2);

                newSt[0, 0] = Math.Pow(averageStX, 2);
                newSt[0, 1] = averageStDiagonal;
                newSt[1, 0] = averageStDiagonal;
                newSt[1, 1] = Math.Pow(averageStY, 2);

                result.Add(Tuple.Create(new PointOfInterest(st.Item1.Point), newSt));
            }
            return result;
        }

        /// <summary>
        /// With eigenvalue decomposition, get the eigenvalues of the list of structure tensors.
        /// </summary>
        /// <param name="structureTensor">
        /// A list of structureTensors
        /// </param>
        /// <returns>
        /// return the eignvalue of each structure for a point 
        /// </returns>
        public static List<Tuple<PointOfInterest, double[]>> EignvalueDecomposition(List<Tuple<PointOfInterest, double[,]>> structureTensors)
        {
            var result = new List<Tuple<PointOfInterest, double[]>>();

            foreach (var st in structureTensors)
            {
                var evd = new EigenvalueDecomposition(st.Item2);
                var realEigenValue = evd.RealEigenvalues;
                result.Add(Tuple.Create(new PointOfInterest(st.Item1.Point), realEigenValue));
            }
            
            return result;
        }

        /// <summary>
        /// Get the attention from the eigenvalues, it's actually the largest eigenvalue in the eigenvalues.
        /// </summary>
        /// <param name="eigenValue">
        /// A list of eigenValues for each point
        /// </param>
        /// <returns>
        /// return the list of attentions  
        /// </returns>
        public static List<Tuple<PointOfInterest, double>> GetAttention(List<Tuple<PointOfInterest, double[]>> eigenValue)
        {
            var result = new List<Tuple<PointOfInterest, double>>();

            foreach (var ev in eigenValue)
            {
                // by default, the eigenvalue is in a ascend order, so just check whether they are equal
                if (ev.Item2[1] > 0.0)
                {
                    result.Add(Tuple.Create(new PointOfInterest(ev.Item1.Point), ev.Item2[1]));
                }       
            }

            return result;
        }

        /// <summary>
        /// Get the threshold for keeping points of interest
        /// </summary>
        /// <param name="attention">
        /// A list of attentions 
        /// </param>
        /// <returns>
        /// return a threshold  
        /// </returns>
        public static double GetThreshold(List<Tuple<PointOfInterest, double>> attention)
        {
            const int numberOfColumn = 1000;
            var maxAttention = MaximumOfAttention(attention);
            var l = GetMaximumLenth(attention, maxAttention);

            return l * maxAttention / numberOfColumn;  
        }

        /// <summary>
        /// Find out the maximum  of a list of attention
        /// </summary>
        /// <param name="attention">
        /// A list of attentions 
        /// </param>
        /// <returns>
        /// return the maximum attention
        /// </returns>
        public static double MaximumOfAttention(List<Tuple<PointOfInterest, double>> attention)
        {
            if (attention.Count == 0)
            {
                throw new InvalidOperationException("Empty list");
            }
            double maxAttention = double.MinValue;

            foreach (var la in attention)
            {
                if (la.Item2 > maxAttention)
                {
                    maxAttention = la.Item2;
                }
            }

            return maxAttention;
        }

        // bardeli: get the l(a scaling parameter) 
        public static int GetMaximumLenth(List<Tuple<PointOfInterest, double>> listOfAttention, double maxOfAttention)
        {
            const int numberOfBins = 1000;
            var sumOfLargePart = 0;
            var sumOfLowerPart = 0;
            var p = 0.0003;  //  a fixed parameterl Bardeli : 0.96
            var l = 0;

            if (listOfAttention.Count >= numberOfBins)
            {
                for (l = 1; l < numberOfBins; l++)
                {
                    sumOfLargePart = sumOfLargePart + CalculateHistogram(listOfAttention, maxOfAttention)[numberOfBins - l];
                    sumOfLowerPart = sumOfLowerPart + CalculateHistogram(listOfAttention, maxOfAttention)[l];
                    if (sumOfLargePart >= p * sumOfLowerPart)
                    {
                        break;
                    }
                }
            }
            else 
            {
                for (l = 1; l < listOfAttention.Count; l++)
                {
                    sumOfLargePart = sumOfLargePart + CalculateHistogram(listOfAttention, maxOfAttention)[numberOfBins - l];
                    sumOfLowerPart = sumOfLowerPart + CalculateHistogram(listOfAttention, maxOfAttention)[l];
                    if (sumOfLargePart >= p * sumOfLowerPart)
                    {                       
                        break;
                    }
                }
            }

            return l;
        }

        // according to Bardeli, Calculate the Histogram
        public static int[] CalculateHistogram(List<Tuple<PointOfInterest, double>> listOfAttention, double maxOfAttention)
        {   
            const int numberOfBins = 1000;
            var histogram = new int[numberOfBins];

            foreach (var la in listOfAttention)
            {
                var attentionValue = la.Item2 * numberOfBins / maxOfAttention;
                var temp = (int)attentionValue;
                if (temp < numberOfBins)
                {
                    histogram[temp]++;
                }
            }

            return histogram;
        }
        
        // according to Bardeli, keep points of interest, whose attention value is greater than the threshold
        public static List<PointOfInterest> ExactPointsOfInterest(List<Tuple<PointOfInterest, double>> attention)
        {
            const int numberOfIncludedBins = 1000;
            var LenghOfAttention = attention.Count();
            var numberOfColumn = attention[LenghOfAttention - 1 ].Item1.Point.X;
            var maxIndexOfPart = (int)(numberOfColumn / numberOfIncludedBins) + 1;

            // each part with 1000 columns has a different threshold 
            var threshold = new double[maxIndexOfPart];

            // for our data, the threshold is best between 150 - 200
            //double threshold = 150.0;                       
            var result = new List<PointOfInterest>();

            //foreach (var ev in attention)
            //{
            //    if (ev.Item2 > threshold)
            //    {
            //         result.Add(ev.Item1);
            //         ev.Item1.DrawColor = PointOfInterest.DefaultBorderColor;
            //    }
            //}

            /// calculate the threshold for each distinct part
            // calculate the threshold for each distinct part
            for (int i = 0; i < maxIndexOfPart; i++)
            {
                // first, it is required to divided the original data into several parts with the width of 1000 colomn
                var tempAttention = new List<Tuple<PointOfInterest, double>>();

                if (numberOfColumn >= numberOfIncludedBins * (i + 1))
                {
                    // var tempAttention = new List<Tuple<Point, double>>();
                    foreach (var a in attention)
                    {
                        if (a.Item1.Point.X >= i * numberOfIncludedBins && a.Item1.Point.X < (i + 1) * numberOfIncludedBins)
                        {
                            tempAttention.Add(Tuple.Create(new PointOfInterest(a.Item1.Point), a.Item2));
                        }
                    }
                   threshold[i] = GetThreshold(tempAttention);

                   foreach (var ev in tempAttention)
                   {
                       if (ev.Item2 > threshold[i])
                       {
                           result.Add(ev.Item1);
                           ev.Item1.DrawColor = PointOfInterest.DefaultBorderColor;
                       }
                    }
                }
                else
                {
                    foreach (var a in attention)
                    {
                        if (a.Item1.Point.X >= i * numberOfIncludedBins && a.Item1.Point.X <= numberOfColumn)
                        {
                            tempAttention.Add(Tuple.Create(new PointOfInterest(a.Item1.Point), a.Item2));
                        }
                    }
                    threshold[i] = GetThreshold(tempAttention);

                    foreach (var ev in tempAttention)
                    {
                        if (ev.Item2 > threshold[i])
                        {
                            result.Add(ev.Item1);
                            ev.Item1.DrawColor = PointOfInterest.DefaultBorderColor;
                        }
                    }
                }
            }

            return result; 
        }

        /// <summary>
        /// The get absolute template.
        /// </summary>
        /// <param name="anchorPoint">
        /// The anchor point.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// The list of Points represent the absoluteTemplate.
        /// </returns>
        public static List<Point> GetAbsoluteTemplate(Point anchorPoint, List<Point> template)
        {
            var numberOfVertexes = template.Count;
            int relativeFrame = anchorPoint.X;
            int relativeFrequency = anchorPoint.Y;
            // Make a copy of original template
            var result = new List<Point>(template);

            // get an absolute template
            for (int index = 0; index < numberOfVertexes; index++)
            {
                var temp = result[index];
                temp.X += relativeFrame;
                temp.Y += relativeFrequency;
                result[index] = temp;
            }

            return result;
        }

        /// <summary>
        /// The get absolute template.
        /// </summary>
        /// <param name="matchedPoi">
        /// The matched Poi.
        /// </param>
        /// <returns>
        /// The list of Points represent the absoluteTemplate.
        /// </returns>
        public static List<PointOfInterest> GetAbsoluteTemplate(List<PointOfInterest> matchedPoi)
        {
            var result = new List<PointOfInterest>();
            foreach (var poi in matchedPoi)
            {
                var tempPoints = GetAbsoluteTemplate(poi.Point, TemplateTools.LewinsRailTemplate(18));
                foreach (var tpoint in tempPoints)
                {
                    result.Add(new PointOfInterest(tpoint));
                }
            }

            foreach (PointOfInterest t in result)
            {
                t.DrawColor = PointOfInterest.TemplateColor;
            }

            return result;
        }

        /// <summary>
        /// The euclidean distance.
        /// </summary>
        /// <param name="p1">
        /// The p 1.
        /// </param>
        /// <param name="p2">
        /// The p 2.
        /// </param>
        /// <returns>
        /// The distance between two points.
        /// </returns>
        public static double EuclideanDistance(Point p1, Point p2)
        {
            var deltaX = Math.Pow(p2.X - p1.X, 2);
            var deltaY = Math.Pow(p2.Y - p1.Y, 2);

            return Math.Sqrt(deltaX + deltaY);
        }

        /// <summary>
        /// The manhanton distance.
        /// </summary>
        /// <param name="p1">
        /// The p 1.
        /// </param>
        /// <param name="p2">
        /// The p 2.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int ManhattanDistance(Point p1, Point p2)
        {
            var deltaX = Math.Abs(p2.X - p1.X);
            var deltaY = Math.Abs(p2.Y - p1.Y);

            return deltaX + deltaY;
        }

        /// <summary>
        /// To generate a binary spectrogram, an amplitudeThreshold is required
        /// Above the threshold, its amplitude value will be assigned to MAX (black), otherwise to MIN (white)
        /// Side affect: An image is saved
        /// Side affect: the original AmplitudeSonogram is modified.
        /// </summary>
        /// <param name="amplitudeSpectrogram">
        /// The amplitude Spectrogram.
        /// </param>
        /// <param name="amplitudeThreshold">
        /// The amplitude Threshold.
        /// </param>
        public static void GenerateBinarySpectrogram(AmplitudeSonogram amplitudeSpectrogram, double amplitudeThreshold)
        {
            var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;

            for (int i = 0; i < spectrogramAmplitudeMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < spectrogramAmplitudeMatrix.GetLength(1); j++)
                {
                    if (spectrogramAmplitudeMatrix[i, j] > amplitudeThreshold)
                    {
                        // by default it will be 0.028
                        spectrogramAmplitudeMatrix[i, j] = 1;
                    }
                    else
                    {
                        spectrogramAmplitudeMatrix[i, j] = 0;
                    }
                }
            }

            var imageResult = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
            imageResult.Save("C:\\Test recordings\\Test4.png");
        }

        /// <summary>
        /// PeakAmplitudeDetection applies a window of n seconds, starting every n seconds, to the recording. The window detects points
        /// that have a peakAmplitude value.
        /// </summary>
        /// <param name="amplitudeSpectrogram">
        /// An input amplitude sonogram.
        /// </param>
        /// <param name="minFreq">
        /// for slideWindowMinFrequencyBin.
        /// </param>
        /// <param name="maxFreq">
        /// for slideWindowMaxFrequencyBin.
        /// </param>
        /// <param name="slideWindowDuation">
        /// for slideWindowFrame.
        /// </param>
        /// <returns>
        /// strings of out of points. 
        /// </returns>
        public static string PeakAmplitudeDetection(AmplitudeSonogram amplitudeSpectrogram, int minFreq, int maxFreq, double slideWindowDuation = 2.0)
        {
            var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;

            var numberOfWindows = (int)(amplitudeSpectrogram.Duration.Seconds / slideWindowDuation);

            // get the offset frame of Window
            var slideWindowFrameOffset = (int)Math.Round(slideWindowDuation * amplitudeSpectrogram.FramesPerSecond);

            var slideWindowminFreqBin = (int)Math.Round(minFreq / amplitudeSpectrogram.FBinWidth);
            var slideWindowmaxFreqBin = (int)Math.Round(maxFreq / amplitudeSpectrogram.FBinWidth);

            var peakPoints = new Tuple<Point, double>[numberOfWindows];

            for (int windowIndex = 0; windowIndex < numberOfWindows; windowIndex++)
            {
                const double CurrentMaximum = double.NegativeInfinity;

                // scan along frames
                for (int i = 0; i < (windowIndex + 1) * slideWindowFrameOffset; i++)
                {
                    // scan through bins
                    for (int j = slideWindowminFreqBin; j < slideWindowmaxFreqBin; j++)
                    {
                        if (spectrogramAmplitudeMatrix[i, j] > CurrentMaximum)
                        {
                            peakPoints[windowIndex] = Tuple.Create(new Point(i, j), spectrogramAmplitudeMatrix[i, j]);
                        }
                    }
                }
            }

            var outputPoints = string.Empty;

            foreach (var point in peakPoints)
            {
                if (point != null)
                {
                    outputPoints += string.Format(
                        "Point found at x:{0}, y:{1}, value: {2}\n", point.Item1.X, point.Item1.Y, point.Item2);
                }
            }

            return outputPoints;
            ////Log.Info("Found points: \n" + outputPoints);
        }
  
        /// <summary>
        /// Make fake acoustic events randomly. 
        /// </summary>
        /// <param name="numberOfFakes">
        /// The number Of Fakes.
        /// </param>
        /// <param name="minTime">
        /// The min Time.
        /// </param>
        /// <param name="minFrequency">
        /// The min Frequency.
        /// </param>
        /// <param name="maxTime">
        /// The max Time.
        /// </param>
        /// <param name="maxFrequency">
        /// The max Frequency.
        /// </param>
        /// <returns>
        /// The array of AcousticEvent.
        /// </returns>
        public static AcousticEvent[] MakeFakeAcousticEvents(
            int numberOfFakes, double minTime, double minFrequency, double maxTime, double maxFrequency)
        {
            Contract.Requires(numberOfFakes > 0);

            var rand = new Random();
            var duration = maxTime - minTime;

            var events = new AcousticEvent[numberOfFakes];
            for (int index = 0; index < numberOfFakes; index++)
            {
                events[index] = new AcousticEvent(
                    rand.NextDouble() * minTime,
                    rand.NextDouble() * duration,
                    minFrequency * rand.NextDouble(),
                    maxFrequency * rand.NextDouble());
            }

            return events;
        }

        /// <summary>
        /// Draw a line on the spectrogram
        /// Side affect: writes image to disk.
        /// </summary>
        /// <param name="wavFilePath">
        /// Get the spectrogram through open a wavFilePath.
        /// </param>
        /// <param name="startTime">
        /// The start Time.
        /// </param>
        /// <param name="endTime">
        /// The end Time.
        /// </param>
        /// <param name="minFrequency">
        /// The min Frequency.
        /// </param>
        /// <param name="maxFrequency">
        /// The max Frequency.
        /// </param>
        public static void DrawLine(string wavFilePath, double startTime, double endTime, int minFrequency, int maxFrequency)
        {
            var recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig();
            var amplitudeSpectrogram = new SpectralSonogram(config, recording.GetWavReader());
            var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;

            var minFrame = (int)Math.Round(startTime * amplitudeSpectrogram.FramesPerSecond);
            var maxFrame = (int)Math.Round(endTime * amplitudeSpectrogram.FramesPerSecond);

            var minFrequencyBin = (int)Math.Round(minFrequency / amplitudeSpectrogram.FBinWidth);
            var maxFrequencyBin = (int)Math.Round(maxFrequency / amplitudeSpectrogram.FBinWidth);

            for (int i = minFrame; i < maxFrame; i++)
            {
                for (int j = minFrequencyBin; j < maxFrequencyBin; j++)
                {
                    spectrogramAmplitudeMatrix[i, j] = 1;
                }
            }

            var imageResult = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
            imageResult.Save("C:\\Test recordings\\Test2.png");
        }

        /// <summary>
        /// Draw a box on a fixed frequency and time range
        /// Side affect: writes file to disk.
        /// </summary>
        /// <param name="wavFilePath">
        /// Get the spectrogram through open a wavFilePath.
        /// </param>
        public static void DrawBox(string wavFilePath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(wavFilePath));
            Contract.Requires(File.Exists(wavFilePath));

            var recording = new AudioRecording(wavFilePath);
            var amplitudeSpectrogram = new AmplitudeSonogram(StandardConfig, recording.GetWavReader());
            var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;
            const int MinFreq = 2000;
            const int MaxFreq = 3500;
            var minFreqBin = (int)Math.Round(MinFreq / amplitudeSpectrogram.FBinWidth);
            var maxFreqBin = (int)Math.Round(MaxFreq / amplitudeSpectrogram.FBinWidth);

            const int StartTime = 16;
            const int EndTime = 22;
            var minFrameNum = (int)Math.Round(StartTime * amplitudeSpectrogram.FramesPerSecond);
            var maxFrameNum = (int)Math.Round(EndTime * amplitudeSpectrogram.FramesPerSecond);

            for (int i = minFrameNum; i < maxFrameNum; i++)
            {
                spectrogramAmplitudeMatrix[i, minFreqBin] = 1;
                spectrogramAmplitudeMatrix[i, maxFreqBin] = 1;
            }

            for (int j = minFreqBin; j < maxFreqBin; j++)
            {
                spectrogramAmplitudeMatrix[minFrameNum, j] = 1;
                spectrogramAmplitudeMatrix[maxFrameNum, j] = 1;
            }

            var imageResult = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
            imageResult.Save("C:\\Test recordings\\Test3.png");
        }

        public static Tuple<int, int, double[,]> result { get; set; }
    }
}