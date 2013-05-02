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

    /// <summary>
    /// The acoustic event detection.
    /// </summary>
    public class PoiAnalysis
    {
        private static readonly SonogramConfig StandardConfig = new SonogramConfig();

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
        public static List<PointOfInterest> PickLocalMaximum(double[,] m, int neighborWindowSize)
        {
            Contract.Requires(neighborWindowSize % 2 == 1, "Neighbourhood window size must be odd");
            Contract.Requires(neighborWindowSize >= 1, "Neighbourhood window size must be at least 1");
            int centerOffset = -(int)(neighborWindowSize / 2.0);
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

        //  Calculate the difference between current pixel and its neighborhood pixel
        public static Tuple<double[,], double[,]> PartialDifference(double[,] m)
        {
            int MaximumXIndex = m.GetLength(0);
            int MaximumYIndex = m.GetLength(1);

            var partialDifferenceX = new double[MaximumXIndex, MaximumYIndex];
            var partialDifferenceY = new double[MaximumXIndex, MaximumYIndex];

            for (int row = 0; row < MaximumXIndex; row++)
            {
                for (int col = 0; col < MaximumYIndex; col++)
                {
                    if (m.PointIntersect(row + 1, col))
                    {
                        partialDifferenceX[row, col] = m[row + 1, col] - m[row, col];
                    }
                    else
                    {
                        partialDifferenceX[row, col] = 0.0;
                    }
                    if (m.PointIntersect(row, col + 1))
                    {
                        partialDifferenceY[row, col] = m[row, col + 1] - m[row, col];
                    }
                    else
                    {
                        partialDifferenceY[row, col] = 0.0;
                    }
                 }      
            }
            var result = Tuple.Create(partialDifferenceX, partialDifferenceY);
            return result;
        } 
        
        // A 7 * 7 gaussian blur
        public static double[,] gaussianBlur = {{0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00038771,	0.01330373,	0.11098164,	0.22508352,	0.11098164,	0.01330373,	0.00038771},
                                                {0.00019117,	0.00655965,	0.05472157,	0.11098164,	0.05472157,	0.00655965,	0.00019117},
                                                {0.00002292,	0.00078633,	0.00655965,	0.01330373,	0.00655965,	0.00078633,	0.00002292},
                                                {0.00000067,	0.00002292,	0.00019117,	0.00038771,	0.00019117,	0.00002292,	0.00000067}};

        // calculate the structure tensor for each point
        public static List<Tuple<PointOfInterest, double[,]>> GaussianStructureTensor(double[,] gaussianBlur, double[,] partialDifferenceX, double[,] partialDifferenceY)
        {
            
            var sizeOfGaussianBlur = Math.Max(gaussianBlur.GetLength(0), gaussianBlur.GetLength(0));
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

                    // check whether the current point can be in the center of gaussian blur 
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
                }            
            }           
            return result;
        }

        // Bardeli: calculate the structure tensor
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

        // Bardeli: calculate the mean of the structure tensor
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

        // For each structure tensor matrix of each point, calculate its eigenvalues
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

        // get the attention value for each structure tensor at each point, and keep the greater one
        public static List<Tuple<PointOfInterest, double>> GetAttention(List<Tuple<PointOfInterest, double[]>> eigenValue)
        {
            var result = new List<Tuple<PointOfInterest, double>>();

            foreach (var ev in eigenValue)
            {
                // by default, the eigenvalue is in a ascend order, so just check whether they are equal
                if (ev.Item2[0] == ev.Item2[1])
                {
                    ev.Item2[1] = 0.0;
                    result.Add(Tuple.Create(new PointOfInterest(ev.Item1.Point), ev.Item2[1]));
                }
                else
                {
                    result.Add(Tuple.Create(new PointOfInterest(ev.Item1.Point), ev.Item2[1]));
                }           
            }

            return result;
        }

        // get the threshold for keeping poi
        public static double GetThreshold(List<Tuple<PointOfInterest, double>> attention)
        {
            const int numberOfColumn = 1000;
            var maxAttention = MaximumOfAttention(attention);
            var l = GetMaximumLenth(attention, maxAttention);

            return l * maxAttention / numberOfColumn;  
        }

        // find out the maximum  of attention in a window with 1000 columns width
        public static double MaximumOfAttention(List<Tuple<PointOfInterest, double>> listOfAttention)
        {
            if (listOfAttention.Count == 0)
            {
                throw new InvalidOperationException("Empty list");
            }
            double maxAttention = double.MinValue;

            foreach (var la in listOfAttention)
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
            var p = 0.03;  
            //var p = 0.96;  //  a fixed parameterl
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

            // each distinct part with 1000 columns has a threshold 
            var threshold = new double[maxIndexOfPart];
                    
            var result = new List<PointOfInterest>();

            // calculate the threshold for each distinct part
            for (int i = 0; i < maxIndexOfPart; i++)
            {
                // first, it is required to divided the original data into several parts with the width of 1000 colomn
                // for each part, it will have a distinct threshold
                // keep a list of attention and tempfor calculating threshold
                var tempAttention = new List<Tuple<PointOfInterest, double>>();

                if (numberOfColumn >= numberOfIncludedBins * (i + 1))
                {
               
                    // var tempAttention = new List<Tuple<Point, double>>();
                    foreach (var a in attention)
                    {
                        if (a.Item1.Point.X >= i * numberOfIncludedBins && a.Item1.Point.X  < (i + 1) * numberOfIncludedBins)
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
        public static List<PointOfInterest> GetAbsoluteTemplate2(List<PointOfInterest> matchedPoi)
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