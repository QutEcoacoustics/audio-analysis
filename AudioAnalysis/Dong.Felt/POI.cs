// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticEventDetection.cs" company="MQUTeR">
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
    using System.Text;

    using AnalysisBase;

    using AudioAnalysisTools;

    using TowseyLib;

    using Acoustics.Shared.Extensions;

    /// <summary>
    /// The acoustic event detection.
    /// </summary>
    public class POI
    {
        private static readonly SonogramConfig standardConfig = new SonogramConfig();

        //  pick the highest point in the spectrogram
        /// <summary>
        /// AudioToSpectrogram.  
        /// </summary>
        /// <param name="wavFilePath">
        /// </param>
        /// <param name="recording">
        /// The recording.
        /// </param>
        /// <returns>
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
        /// the result is binary spectrogram.
        /// </summary>
        /// <param name="spectralSonogram">
        /// The spectral sonogram.
        /// </param>
        /// <param name="threshold">
        /// The threshold. ranging from 3.0 or 4.0 - decible value
        /// </param>
        public static System.Tuple<double[,], double[]> NoiseReductionToBinarySpectrogram(
            SpectralSonogram spectralSonogram, double backgroundthreshold, bool makeBinary = false)
        {

            return SNR.NoiseReduce(spectralSonogram.Data, NoiseReductionType.STANDARD, backgroundthreshold);
            //double[] modalNoise = SNR.CalculateModalNoise(spectralSonogram.Data);
            //double[,] matrix = spectralSonogram.Data;

            //int rowCount = matrix.GetLength(0);
            //int colCount = matrix.GetLength(1);
            //double[,] outMatrix = new double[rowCount, colCount]; 

            //for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            //{
            //    for (int y = 0; y < rowCount; y++) //for all rows
            //    {
            //        outMatrix[y, col] = matrix[y, col] - modalNoise[col];
            //        if (outMatrix[y, col] < threshold)
            //        {
            //            outMatrix[y, col] = 0.0;
            //        }
            //        else
            //        {
            //            if (makeBinary)
            //            {
            //                outMatrix[y, col] = 1.0;
            //            }
            //        }
            //    } 
            //}
            //spectralSonogram.Data = outMatrix;
            //return spectralSonogram.Data;
            //spectralSonogram.Data = outM;

            //var imageResult = new Image_MultiTrack(spectralSonogram.GetImage(false, false));
            //imageResult.Save("C:\\Test recordings\\Test6.png");
        }

        /// <summary>
        /// The pick local maximum.
        /// </summary>
        /// <param name="m">
        /// The m. matrix.
        /// </param>
        /// <param name="neighborWindowSize">
        /// The neighbor window size. It should be odd number. 
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<Point, double>> PickLocalMaximum(double[,] m, int neighborWindowSize)
        {
            Contract.Requires(neighborWindowSize % 2 == 1, "Neighbourhood window size must be odd");
            Contract.Requires(neighborWindowSize >= 1, "Neighbourhood window size must be at least 1");
            int centerOffset = -(int)(neighborWindowSize / 2.0);

            var results = new List<Tuple<Point, double>>();

            // scan the whole matrix
            //for (int row = 0; row < m.GetLength(1); row++)
            //{
            //    for (int col = 0; col < m.GetLength(0); col++)
            //    {
            //        // assume local maxium
            //        double localMaximum = m[col, row];
            //        bool maximum = true;

            //        // check if it really is the local maximum in the neighbourhood
            //        for (int i = centerOffset; i < neighborWindowSize; i++)
            //        {
            //            for (int j = centerOffset; j < neighborWindowSize; j++)
            //            {
            //                if (m.PointIntersect(col + j, row + i))
            //                {
            //                    var current = m[col + j, row + i];

            //                    // don't check the middle point
            //                    if (localMaximum <= current && !(i == 0 && j == 0))
            //                    {
            //                        // actually not a local maximum
            //                        maximum = false;
            //                    }
            //                }
            //            }
            //        }

            //        // iff it is indeed the local maximum, then add it
            //        if (maximum)
            //        {
            //            results.Add(Tuple.Create(new Point(col, row), m[col, row]));
            //        }
            //    }
            //}
            //return results;
            // scan fixed range of recording
            for (int row = m.GetLength(1) / 4; row < 2 * m.GetLength(1) / 5; row++)
            {
                for (int col = 3096; col < 3180; col++)  // 3010 =  35s * frame/second(86)
                {
                    // assume local maxium
                    double localMaximum = m[col, row];
                    bool maximum = true;
                    // check if it really is the local maximum in the neighbourhood
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
                        results.Add(Tuple.Create(new Point(col, row), m[col, row]));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// The filter out points.   Pick up points whose amplitude value is more than a threshold.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <param name="threshold">
        /// The threshold.   It should be a dB value.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<Point, double>> FilterOutPoints(
            List<Tuple<Point, double>> list, int amplitudethreshold)
        {
            var results = new List<Tuple<Point, double>>();

            results = list.Where(item => item.Item2 > amplitudethreshold).ToList();

            return results;
        }

        /// <summary>
        /// The merge close point.
        /// </summary>
        /// <param name="pointsOfInterest">
        /// The points of interest.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<Point, double>> RemoveClosePoint(List<Tuple<Point, double>> pointsOfInterest, int offset)
        {
            var maxIndex = pointsOfInterest.Count;
            var results = new List<Tuple<Point, double>>();
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
                        int deltaX = Math.Abs(poi[i].Item1.X - poi[j].Item1.X);
                        int deltaY = Math.Abs(poi[i].Item1.Y - poi[j].Item1.Y);

                        // if they are close, check whose power is strong
                        if (deltaX < offset && deltaY < offset)
                        {
                            if (poi[j].Item2 >= poi[i].Item2)
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
            return results = pointsOfInterest;
        }

        /// <summary>
        /// The lewins rail template.
        /// </summary>
        /// <param name="fillOutPoints">
        /// The fill out points.
        /// </param>
        /// <param name="pixelOffset">
        /// The pixel offset.  // should be 18 or 19.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Point> LewinsRailTemplate(int pixelOffset)
        {
            var template = new List<Point>()
                               {
                                   //the first five points in the same frequency bin 67 & have same time space
                                   new Point(0, 67),
                                   new Point(0 + pixelOffset, 67),
                                   new Point(0 + 2 * pixelOffset, 67),
                                   //new Point(0 + 3 * pixelOffset, 67),
                                  // new Point(0 + 4 * pixelOffset, 67),

                                   // the second five points in the same frequency bin
                                   new Point(0, 90),
                                   new Point(0 + pixelOffset, 90),
                                   new Point(0 + 2 * pixelOffset, 90),
                                  // new Point(0 + 3 * pixelOffset, 90),
                                   //new Point(0 + 4 * pixelOffset, 90),

                                   new Point(0, 101),
                                   new Point(0 + pixelOffset, 101),
                                   new Point(0 + 2 * pixelOffset, 101),
                                  // new Point(0 + 3 * pixelOffset, 101),
                                 //  new Point(0 + 4 * pixelOffset, 101),

                                   new Point(0, 113),
                                   new Point(0 + pixelOffset, 113),
                                   new Point(0 + 2 * pixelOffset, 113),
                                  // new Point(0 + 3 * pixelOffset, 113),
                                 //  new Point(0 + 4 * pixelOffset, 113),
                               };
            return template;
        }

        public static Point GetCenteroid(List<Point> points)
        {
            var centeroid = new Point();
            var numberOfVertex = points.Count;
            var centerIndex = (numberOfVertex - 1) / 2;
            centeroid = points[centerIndex];

            return centeroid;
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
        /// The <see cref="List"/>.  List<Tuple<Point, double>>
        /// </returns>
        public static double[] AverageDistanceScore(List<Point> template, List<Tuple<Point, double>> pointsOfInterest)
        {
            var numberOfVertexes = template.Count;
            var distance = new double[pointsOfInterest.Count];                     
            int relativeFrame;
            //var result = new List<Tuple<Point, double>>();
            double minimumDistance = 0.0;
            var avgDistance = new double[pointsOfInterest.Count];
            var tempDistance = new double[numberOfVertexes];
            
            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                var poi = pointsOfInterest;
                double temp = 0.0;
                distance[i] = 0.0;
                Point anchorPoint = poi[i].Item1;
                var absoluteTemplate = GetAbsoluteTemplate(anchorPoint, template);
                    // skip the anchor point, we already know the distance
                for (int j = 0; j < poi.Count; j++)
                {                   
                    //distance[j] = 66666666666;
                    if (poi[j] == poi[i])
                    {
                            continue;
                    }
                    else
                    {
                         for (int k = 1; k < numberOfVertexes; k++)
                         {
                             distance[j] += EuclideanDistance(template[k], poi[j].Item1);                    
                         }
                     }                       
                    // choose the smallest distance 
                    //minimumDistance = distance.Min();                   
                }
                Array.Sort(distance);
                Array.Copy(distance, tempDistance, numberOfVertexes-1);
                for (int index = 0; index < numberOfVertexes-1; index++)
                {                   
                    temp = temp + tempDistance[index];
                }               
                avgDistance[i] = temp / (numberOfVertexes - 1);
                //return result.Add(Tuple.Create(poi[i].Item1, avgDistance));     
            }
            return avgDistance;
        }

        public static bool CheckTheDistance(Point p1, Point p2, int threshold)
        {
            int deltaX = Math.Abs(p1.X - p2.X);
            int deltaY = Math.Abs(p1.Y - p2.Y);

            if (deltaX < threshold && deltaY < threshold)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Point> GetAbsoluteTemplate(Point anchorPoint, List<Point> template)
        {
            var numberOfVertexes = template.Count;
            int relativeFrame = anchorPoint.X;
            // this loop I want to get an absolute template
            for (int Index = 0; Index < numberOfVertexes; Index++)
            {
                var tempTemplate = template[Index];
                tempTemplate.X += relativeFrame;
                template[Index] = tempTemplate;
            }

            return template;
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
        /// The <see cref="double"/>.
        /// </returns>
        public static double EuclideanDistance(Point p1, Point p2)
        {
            var deltaX = Math.Pow(p2.X - p1.X, 2);
            var deltaY = Math.Pow(p2.Y - p1.Y, 2);

            return Math.Sqrt(deltaX + deltaY);
        }

        /// <summary>
        /// The fill out points.
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <param name="lowFrequency">
        /// The low frequency.
        /// </param>
        /// <param name="highFrequency">
        /// The high frequency.
        /// </param>
        /// <param name="frequencyBinWidth">
        /// The frequency bin width.
        /// </param>
        /// <param name="pixelOffset">
        /// The pixel offset.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Point> FilterOutPointsForLewins(
            List<Point> points, int lowFrequency, int highFrequency, double frequencyBinWidth, int pixelOffset)
        {
            var maxIndex = points.Count;

            // for Lewins 3000/86;
            var lowFrequencyBin = lowFrequency / frequencyBinWidth;

            // for Lewins 4000/86;
            var highFrequencyBin = highFrequency / frequencyBinWidth;

            var result = new List<Point>(maxIndex);
            for (int index = 0; index < maxIndex; index++)
            {
                var topPoints = false;
                var bottomPoints = false;
                var deltaTopY = (int)Math.Abs(points[index].Y - highFrequencyBin);
                var deltaBottomY = (int)Math.Abs(points[index].Y - lowFrequencyBin);

                if (deltaTopY <= pixelOffset)
                {
                    topPoints = true;
                }
                if (deltaBottomY <= pixelOffset)
                {
                    bottomPoints = true;
                }
                if (topPoints || bottomPoints)
                {
                    result.Add(points[index]);
                }
            }
            return result;
        }

        /// <summary>
        /// To generate a binary spectrogram, a amplitudeThreshold is required
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
        public void GenerateBinarySpectrogram(AmplitudeSonogram amplitudeSpectrogram, double amplitudeThreshold)
        {
            var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;
            ////var maximumOfFrame = (int)Math.Round(amplitudeSpectrogram.FrameDuration / amplitudeSpectrogram.FramesPerSecond);
            ////var maximumOfFrequencyBin = (int)Math.Round(amplitudeSpectrogram.NyquistFrequency / amplitudeSpectrogram.FBinWidth);
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
        /// </param>
        /// <returns>
        /// strings of out of points. 
        /// </returns>
        public static string PeakAmplitudeDetection(AmplitudeSonogram amplitudeSpectrogram, int minFreq, int maxFreq, double slideWindowDuation = 2.0)
        {
            var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;

            var numberOfWindows = (int)(amplitudeSpectrogram.Duration.Seconds / slideWindowDuation);
            // by default SlideWindowDuation = 2.0 sec

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

        //const double customizedTime = 15.0;
        //var events = new List<AcousticEvent>() { 
        //new AcousticEvent(peakPoints[0].Item1.X / amplitudeSpectrogram.FramesPerSecond, customizedTime / amplitudeSpectrogram.FramesPerSecond, peakPoints[0].Item1.Y * 43 - 15,peakPoints[0].Item1.Y*43),   
        //new AcousticEvent(peakPoints[1].Item1.X / amplitudeSpectrogram.FramesPerSecond, customizedTime / amplitudeSpectrogram.FramesPerSecond, peakPoints[1].Item1.Y * 43 - 15,peakPoints[1].Item1.Y*43),
        //  //new AcousticEvent(11.0,2.0,500,1000),
        //  //new AcousticEvent(14.0,2.0,500,1000),
        //  //new AcousticEvent(17.0,2.0,500,1000),
        //};
        //foreach (var e in events)
        //{
        //     e.BorderColour = AcousticEvent.DEFAULT_BORDER_COLOR;
        //}
        //var image = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
        //image.AddTrack(Image_Track.GetTimeTrack(amplitudeSpectrogram.Duration, amplitudeSpectrogram.FramesPerSecond));
        //image.AddTrack(Image_Track.GetSegmentationTrack(amplitudeSpectrogram));
        //image.AddEvents(events, amplitudeSpectrogram.NyquistFrequency, amplitudeSpectrogram.Configuration.FreqBinCount, amplitudeSpectrogram.FramesPerSecond);

        //image.Save("C:\\Test recordings\\Test5.png");    
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
        /// The <see>
        ///         <cref>AcousticEvent[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public AcousticEvent[] MakeFakeAcousticEvents(
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

            //var recording = new AudioRecording(wavFilePath);
            //var events = new List<AcousticEvent>() { 
            //    new AcousticEvent(5.0,2.0,500,1000),   
            //    new AcousticEvent(8.0,2.0,500,1000),
            //    new AcousticEvent(11.0,2.0,500,1000),
            //    new AcousticEvent(14.0,2.0,500,1000),
            //    new AcousticEvent(17.0,2.0,500,1000),
            //};
            //foreach (var e in events)
            //{
            //    e.BorderColour = AcousticEvent.DEFAULT_BORDER_COLOR;
            //}
            ////generate spectrogram
            //var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };
            //var spectrogram = new SpectralSonogram(config, recording.GetWavReader());

            //var image = new Image_MultiTrack(spectrogram.GetImage(false, true));
            //image.AddTrack(Image_Track.GetTimeTrack(spectrogram.Duration, spectrogram.FramesPerSecond));
            ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(spectrogram));
            //image.AddEvents(events, spectrogram.NyquistFrequency, spectrogram.Configuration.FreqBinCount, spectrogram.FramesPerSecond);
            //image.Save("C:\\Test recordings\\Test1.png");
        }

        /// <summary>
        /// Draw a line on the spectrogram
        /// 
        /// Side affect: writes image to disk
        /// </summary>
        /// <param name="wavFilePath"></param>
        public void DrawLine(string wavFilePath, double startTime, double endTime, int minFrequency, int maxFrequency)
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
        /// Draw a box on a customerized frequency and time range
        /// Side affect: writes file to disk
        /// </summary>
        /// <param name="wavFilePath"></param>
        public static void DrawCustomizedBox(string wavFilePath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(wavFilePath));
            Contract.Requires(File.Exists(wavFilePath));

            var recording = new AudioRecording(wavFilePath);
            var amplitudeSpectrogram = new AmplitudeSonogram(standardConfig, recording.GetWavReader());
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

// print configure dictionary
         //string printMessage = analysisSettings.ConfigDict["my_custom_setting"];
         //Log.Info(printMessage);

/*
 //  not necessary part - you shouldn't ever work on recordings that aren't in 22050Hz
            //if (recording.SampleRate != 22050)
            //{
            //    recording.ConvertSampleRate22kHz();
            //}
*/


//private static bool Filter(Tuple<Point, double> item)
//{
//    return item.Item2 < 10;
//}