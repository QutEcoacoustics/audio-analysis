﻿// --------------------------------------------------------------------------------------------------------------------
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
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;
    using AForge.Imaging.Filters;
    using Accord.Math.Decompositions;
    using Dong.Felt.Preprocessing;
    using Dong.Felt.Representations;
    using Dong.Felt.SpectrogramDrawing;

    // several types of points of interest (for later use)
    public enum FeatureType { NONE, LOCALMAXIMA, RIDGE, STRUCTURE_TENSOR }

    public class PointOfInterestAnalysis
    {
        
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
            var amplitudeSpectrogram = new SpectrogramStandard(config, recording.WavReader);
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
        public static void DrawBox(string wavFilePath, SonogramConfig config)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(wavFilePath));
            Contract.Requires(File.Exists(wavFilePath));

            var recording = new AudioRecording(wavFilePath);
            var amplitudeSpectrogram = Preprocessing.AudioPreprosessing.AudioToSpectrogram(config, wavFilePath);
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

        // Why I have to this step? I forgot. 
        public static List<PointOfInterest> ConnectPOI(List<PointOfInterest> poi)
        {
            var result = new List<PointOfInterest>();
            foreach (var p in poi)
            {
                result.Add(p);
                foreach (var p1 in poi)
                {
                    if (p != p1)
                    {
                        var diffX = Math.Abs(p.Point.X - p1.Point.X);
                        var diffY = Math.Abs(p.Point.Y - p1.Point.Y);
                        var stepDiff = 2;
                        if (p.OrientationCategory == p1.OrientationCategory && (diffX < stepDiff) && (diffY < stepDiff))
                        {
                            if ((p.Point.X - p1.Point.X) < 0)
                            {
                                Point point = new Point(p.Point.X, p.Point.Y);
                                var secondsScale = 11.6;
                                var herzScale = 43;
                                var freqBinCount = 256;
                                TimeSpan time = TimeSpan.FromSeconds(p.Point.X * secondsScale);
                                double herz = (freqBinCount - p.Point.Y - 1) * herzScale;
                                var poi1 = new PointOfInterest(time, herz);
                                poi1.Point = point;
                                //poi.RidgeOrientation = direction;
                                // convert the orientation into - pi/2 to pi / 2 from 0 ~ pi
                                poi1.RidgeOrientation = p.RidgeOrientation;
                                poi1.OrientationCategory = p.OrientationCategory;                               
                                poi1.RidgeMagnitude = p.RidgeMagnitude;
                                poi1.Intensity = p.Intensity;
                                poi1.TimeScale = p.TimeScale;
                                poi1.HerzScale = p.HerzScale;
                                result.Add(poi1);
                            }
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Gaussian blur on ridge point of interest. 
        /// </summary>
        /// <param name="audioFileDirectory"></param>
        /// <param name="config"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="sigma">by default 1.0</param>
        /// <param name="size">by default 3</param>
        public static void GaussianBlur2(string audioFileDirectory, SonogramConfig config,
            RidgeDetectionConfiguration ridgeConfig, double sigma, int size)
        {
            if (Directory.Exists(audioFileDirectory))
            {
                var audioFiles = Directory.GetFiles(audioFileDirectory, @"*.wav", SearchOption.TopDirectoryOnly);
                var audioFilesCount = audioFiles.Count();
                for (int i = 0; i < audioFilesCount; i++)
                {
                    var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, audioFiles[i]);
                    /// spectrogram drawing setting
                    var scores = new List<double>();
                    scores.Add(1.0);
                    var acousticEventlist = new List<AcousticEvent>();
                    var poiList = new List<PointOfInterest>();
                    double eventThreshold = 0.5; // dummy variable - not used   
                    var rows = spectrogram.Data.GetLength(1) - 1;
                    var cols = spectrogram.Data.GetLength(0);
                    //Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, acousticEventlist, eventThreshold, null);
                    var ridges = POISelection.PostRidgeDetection4Dir(spectrogram, ridgeConfig);                   
                    var smoothedRidges = ClusterAnalysis.SmoothRidges(ridges, rows, cols, 5,3, 1.0, 3);
                    var smoothedRidgesList = StatisticalAnalysis.TransposeMatrixToPOIlist(smoothedRidges);
                    var verSegmentList = new List<AcousticEvent>();
                    var horSegmentList = new List<AcousticEvent>();
                    var posDiSegmentList = new List<AcousticEvent>();
                    var negDiSegmentList = new List<AcousticEvent>();
                    var dividedPOIList = POISelection.POIListDivision(smoothedRidgesList);
                    
                    ClusterAnalysis.RidgeListToEvent(spectrogram, dividedPOIList[0], dividedPOIList[1], dividedPOIList[2], dividedPOIList[3],
                        rows, cols, out verSegmentList, out horSegmentList,
                        out posDiSegmentList, out negDiSegmentList);
                    //var groupedEventsList = ClusterAnalysis.GroupeSepEvents(verSegmentList, horSegmentList, posDiSegmentList, negDiSegmentList);
                    //var groupedRidges = ClusterAnalysis.GroupeSepRidges(verSegmentList, horSegmentList, posDiSegmentList, negDiSegmentList);
                    Image image = DrawSpectrogram.DrawSonogram(spectrogram, scores, verSegmentList, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in dividedPOIList[0])
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                        Point point = new Point(poi.Point.Y, poi.Point.X);
                        double secondsScale = spectrogram.Configuration.GetFrameOffset(spectrogram.SampleRate); // 0.0116
                        var timeScale = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * secondsScale)); // Time scale here is millionSecond?
                        double herzScale = spectrogram.FBinWidth; //43 hz
                        TimeSpan time = TimeSpan.FromSeconds(poi.Point.Y * secondsScale);
                        double herz = (256 - poi.Point.X) * herzScale;
                        // time will be assigned to timelocation of the poi, herz will go to frequencyposition of the poi. 
                        var poi1 = new PointOfInterest(time, herz);
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                    }
                    var FileName = new FileInfo(audioFiles[i]);
                    string annotatedImageFileName = Path.ChangeExtension(FileName.Name, "-Ridge detection-horizontal ridges.png");
                    string annotatedImagePath = Path.Combine(audioFileDirectory, annotatedImageFileName);
                    image = (Image)bmp;
                    image.Save(annotatedImagePath);
                }
            }
        }

        public static double MeasureHLineOfBestfit(PointOfInterest[,] poiMatrix, double lineOfSlope, double intersect)
        {
            var r = 0.0;
            var Sreg = 0.0;
            var Stot = 0.0;
            var poiMatrixLength = poiMatrix.GetLength(0);
            var matrixRadius = poiMatrixLength / 2;
            var improvedRowIndex = 0.0;
            var poiCount = 0;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                        poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                    {
                        int tempColIndex = colIndex - matrixRadius;
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance = lineOfSlope * tempColIndex + intersect - tempRowIndex;
                        improvedRowIndex += tempRowIndex;
                        Sreg += Math.Pow(verticalDistance, 2.0);
                        poiCount++;
                    }
                }
            }
            var nullLineYIntersect = improvedRowIndex / poiCount;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                        poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.East)
                    {
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance1 = tempRowIndex - nullLineYIntersect;
                        Stot += Math.Pow(verticalDistance1, 2.0);
                    }
                }
            }
            if (Stot != 0)
            {
                r = 1 - Sreg / Stot;
            }
            else
            {
                r = 1;
            }
            return r;
        }

        public static double MeasureVLineOfBestfit(PointOfInterest[,] poiMatrix, double lineOfSlope, double intersect)
        {
            var r = 0.0;
            var Sreg = 0.0;
            var Stot = 0.0;
            var poiMatrixLength = poiMatrix.GetLength(0);
            var matrixRadius = poiMatrixLength / 2;
            var improvedRowIndex = 0.0;
            var poiCount = 0;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                        poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                    {
                        int tempColIndex = colIndex - matrixRadius;
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance = lineOfSlope * tempColIndex + intersect - tempRowIndex;
                        improvedRowIndex += tempRowIndex;
                        Sreg += Math.Pow(verticalDistance, 2.0);
                        poiCount++;
                    }
                }
            }
            var nullLineYIntersect = improvedRowIndex / poiCount;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0 &&
                         poiMatrix[rowIndex, colIndex].OrientationCategory == (int)Direction.North)
                    {
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance1 = tempRowIndex - nullLineYIntersect;
                        Stot += Math.Pow(verticalDistance1, 2.0);
                    }
                }
            }
            if (lineOfSlope == Math.PI / 2)
            {
                r = 1;
            }
            else
            {
                if (Stot != 0)
                {
                    r = 1 - Sreg / Stot;
                }
                else
                {
                    r = 1;
                }
            }
            return r;
        }


        public static double MeasureLineOfBestfit(PointOfInterest[,] poiMatrix, double lineOfSlope, double intersect)
        {
            var r = 0.0;
            var Sreg = 0.0;
            var Stot = 0.0;
            var poiMatrixLength = poiMatrix.GetLength(0);
            var matrixRadius = poiMatrixLength / 2;
            var improvedRowIndex = 0.0;
            var poiCount = 0;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0)
                    {
                        int tempColIndex = colIndex - matrixRadius;
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance = lineOfSlope * tempColIndex + intersect - tempRowIndex;
                        improvedRowIndex += tempRowIndex;
                        Sreg += Math.Pow(verticalDistance, 2.0);
                        poiCount++;
                    }
                }
            }
            var nullLineYIntersect = improvedRowIndex / poiCount;
            for (int rowIndex = 0; rowIndex < poiMatrixLength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < poiMatrixLength; colIndex++)
                {
                    if (poiMatrix[rowIndex, colIndex].RidgeMagnitude != 100.0)
                    {
                        int tempRowIndex = matrixRadius - rowIndex;
                        double verticalDistance1 = tempRowIndex - nullLineYIntersect;
                        Stot += Math.Pow(verticalDistance1, 2.0);
                    }
                }
            }

            if (Stot != 0)
            {
                r = 1 - Sreg / Stot;
            }
            else
            {
                r = 1;
            }
            return r;
        }
    }
}