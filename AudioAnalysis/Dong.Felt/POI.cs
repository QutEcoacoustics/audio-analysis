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
        /// <returns>
        /// return a tuple composed of each pixel's coordinate and its amplitude after the noise removal.
        /// </returns>
        public static Tuple<double[,], double[]> NoiseReductionToBinarySpectrogram(
            SpectralSonogram spectralSonogram, double backgroundThreshold, bool makeBinary = true)
        {
            if (!makeBinary)
            {
                return SNR.NoiseReduce(spectralSonogram.Data, NoiseReductionType.BINARY, backgroundThreshold);
            }
            else
            {
                return SNR.NoiseReduce(spectralSonogram.Data, NoiseReductionType.STANDARD, backgroundThreshold);
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
            for (int row = 0; row < m.GetLength(1); row++)
            {
                for (int col = 0; col < m.GetLength(0); col++)
                {
                    // assume local maxium
                    double localMaximum = m[col, row];
                    bool maximum = true;

                    // check if it is really the local maximum in the neighbourhood
                    for (int i = centerOffset; i < neighborWindowSize; i++)
                    {
                        for (int j = centerOffset; j < neighborWindowSize; j++)
                        {
                            if (m.PointIntersect(col + j, row + i))
                            {
                                var current = m[col + j, row + i];

                                // don't check the middle point
                                if (localMaximum <= current && !(i == 0 && j == 0))
                                {
                                    // actually not a local maximum
                                    maximum = false;
                                }
                            }
                        }
                    }

                    // iff it is indeed the local maximum, then add it
                    if (maximum)
                    {
                        results.Add(new PointOfInterest(new Point(col, row)) { Intensity = m[col, row] });
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
        public static List<PointOfInterest> RemoveClosePoint(List<PointOfInterest> pointsOfInterest, int offset)
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
                    var frequencyOffset = Math.Abs(poi.Point.Y - TemplateTools.CentroidFrequency);
                    if (frequencyOffset < 6)
                    {
                        poi.DrawColor = PointOfInterest.HitsColor;
                    }
                   
                    result.Add(poi);
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
    }
}