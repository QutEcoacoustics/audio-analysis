﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArdeaInsignis.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   AKA: The bloody canetoad
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;
    using Acoustics.Shared.Csv;

    /// <summary>
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs 
    /// e.g. Litoria rothii and Litoria olongburesnsis.
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>
    class ArdeaInsignis : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "ArdeaInsignis";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording recording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";


            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int maxHz = (int)configuration[AnalysisKeys.MaxHz];

            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);
            // duration of DCT in seconds 
            //double dctDuration = (double)configuration[AnalysisKeys.DctDuration];

            // minimum acceptable value of a DCT coefficient
            //double dctThreshold = (double)configuration[AnalysisKeys.DctThreshold];
            double noiseReductionParameter = (double?)configuration["SeverityOfNoiseRemoval"] ?? 2.0;
            double decibelThreshold = (double)configuration["DecibelThreshold"];


            double minPeriod = (double)configuration["MinPeriod"]; //: 0.18
            //double maxPeriod = (double)configuration["MaxPeriod"]; //

            int maxOscilRate = (int)Math.Ceiling(1 /minPeriod);
            //int minOscilRate = (int)Math.Floor(1 /maxPeriod);

            // min duration of event in seconds 
            double minDuration = (double)configuration[AnalysisKeys.MinDuration];

            // max duration of event in seconds                 
            double maxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            // this default framesize and overlap is best for the White Hrron of Bhutan.
            const int frameSize = 2048;
            double windowOverlap = 0.0;
            
            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = noiseReductionParameter
            };

            TimeSpan recordingDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            int maxBin = (int)Math.Round(maxHz / freqBinWidth) + 1;

            /* #############################################################################################################################################
             * window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
             * 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
             * 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
             * 2048     17640      116.1ms           8.6         8.6    7430ms           551hz          1100hz
             * 2048     22050       92.8ms          21.5        10.7666 1472ms          
             */

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            
            //var templates = GetTemplatesForAlgorithm1(14);

            double[] amplitudeArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxBin);
            bool[] peakArray = new bool[rowCount];
            double[] amplitudeScores = new double[rowCount];
            double[,] hits = new double[rowCount, colCount];

            const int maxTemplateLength = 20;
            const int templateOffset = 14;
            const int minimumGap = 4;
            const int maximumGap = 100;


            // first find the amplitude peaks
            for (int j = 1; j < amplitudeArray.Length - 1; j++)
            {
                if (amplitudeArray[j] < decibelThreshold) continue;
                if ((amplitudeArray[j] > amplitudeArray[j - 1]) && (amplitudeArray[j] > amplitudeArray[j + 1]))
                {
                    peakArray[j] = true;
                }

            }

            // get template for end of Herron call
            var endTemplate = GetEndTemplateForAlgorithm2();

            // now search for peaks that are the correct distance apart.
            for (int i = 2; i < amplitudeArray.Length - maxTemplateLength; i++)
            {
                if (!peakArray[i]) continue;

                // calculate distance to next peak
                int distanceToNextPeak = CalculateDistanceToNextPeak(peakArray, i);


                if (distanceToNextPeak < minimumGap)
                {
                    continue;
                }

                // skip large gaps
                if (distanceToNextPeak > maximumGap)
                {
                    continue;
                }

                // check for end of call using end template
                if (distanceToNextPeak > maxTemplateLength)
                {
                    int start = i - templateOffset;
                    if (start < 0) start = 0;
                    var endLocality = DataTools.Subarray(amplitudeArray, start, endTemplate.Length); 
                    double endScore = DataTools.CosineSimilarity(endLocality, endTemplate);
                    for (int t = -templateOffset; t < (endTemplate.Length- templateOffset); t++)
                    {
                        if (endScore > amplitudeScores[i + t])
                        {
                            amplitudeScores[i + t] = endScore;
                            //hits[i, minBin] = 10;
                        }
                    }

                    for (int k = 2; k < maxTemplateLength; k++)
                    {
                        amplitudeScores[i + k] = 0.0;
                    }
                    continue;                  
                }

                // Get the start template which depends on distance to next peak.
                var startTemplate = GetTemplateForAlgorithm2(distanceToNextPeak);

                // now calculate similarity of locality with the startTemplate 
                var locality = DataTools.Subarray(amplitudeArray, i-2, startTemplate.Length); // i-2 because first two places should be zero.
                double score = DataTools.CosineSimilarity(locality, startTemplate);
                for (int t = 0; t < startTemplate.Length; t++)
                {
                    if (score > amplitudeScores[i + t])
                    {
                        amplitudeScores[i + t] = score;
                        hits[i, minBin] = 10;
                    }
                }

            } // loop over peak array


            var smoothedScores = DataTools.filterMovingAverageOdd(amplitudeScores, 3);

            //iii: CONVERT decibel sum-diff SCORES TO ACOUSTIC EVENTS
            var predictedEvents = AcousticEvent.ConvertScoreArray2Events(smoothedScores, minHz, maxHz, sonogram.FramesPerSecond,
                                                                          freqBinWidth, eventThreshold, minDuration, maxDuration);

            var prunedEvents = new List<AcousticEvent>();
            foreach (var ae in predictedEvents)
            {
                if (ae.Duration < minDuration)
                {
                    continue;
                }

                // add additional info
                ae.SpeciesName = speciesName;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recordingDuration;
                ae.Name = abbreviatedSpeciesName;
                prunedEvents.Add(ae);
            };

            // do a recognizer test.
            //CompareArrayWithBenchmark(scores, new FileInfo(recording.FilePath));
            //CompareArrayWithBenchmark(prunedEvents, new FileInfo(recording.FilePath));

            var plot = new Plot(this.DisplayName, amplitudeScores, eventThreshold);
            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = prunedEvents
            };

        }


        /// <summary>
        /// Constructs a simple template for the White Herron oscillation call.
        /// </summary>
        /// <param name="callBinWidth">Typical value = 13</param>
        /// <returns></returns>
        public static List<double[]> GetTemplatesForAlgorithm1(int callBinWidth)
        {
            var templates = new List<double[]>();
            // template 1
            double[] t1 = new double[callBinWidth];
            t1[2] = 1.0;
            t1[3] = 1.0;
            t1[8] = 1.0;
            t1[9] = 1.0;
            templates.Add(t1);

            // template 2
            double[] t2 = new double[callBinWidth];
            t1[2] = 1.0;
            t1[3] = 1.0;
            t2[9] = 1.0;
            t2[10] = 1.0;
            templates.Add(t2);

            // template 3
            double[] t3 = new double[callBinWidth];
            t1[2] = 1.0;
            t1[3] = 1.0;
            t2[10] = 1.0;
            t2[11] = 1.0;
            templates.Add(t3);

            // template 4
            double[] t4 = new double[callBinWidth];
            t1[2] = 1.0;
            t1[3] = 1.0;
            t2[11] = 1.0;
            t2[12] = 1.0;
            templates.Add(t4);

            return templates;
        }


        public static double[] GetTemplateForAlgorithm2(int gapBetweenPeaks)
        {
            int templateLength = gapBetweenPeaks + 7;
            var template = new double[templateLength];
            template[2] = 1.0;
            template[3] = 1.0;
            template[templateLength - 4] = 1.0;
            template[templateLength - 3] = 1.0;

            return template;
        }

        public static double[] GetEndTemplateForAlgorithm2()
        {
            int templateLength = 16;
            var template = new double[templateLength];
            template[5] = 0.2;
            template[6] = 0.3;
            template[7] = 0.4;
            template[8] = 0.5;
            template[9] = 0.6;
            template[10] = 0.7;
            template[11] = 0.8;
            template[12] = 0.9;
            template[13] = 1.0;

            return template;
        }

        private static int CalculateDistanceToNextPeak(bool[] peakArray, int currentLocation)
        {
            int distanceToNextPeak = 0;
            for (int i = currentLocation+1; i < peakArray.Length; i++)
            {
                distanceToNextPeak++;
                if (peakArray[i])
                {
                    return distanceToNextPeak;
                }
            }
                return distanceToNextPeak;
        }



        /// <summary>
        /// This test checks a score array (array of doubles) against a standard or benchmark previously stored.
        /// If the benchmark file does not exist then the passed score array is written to become the benchmark.
        /// </summary>
        /// <param name="scoreArray"></param>
        /// <param name="wavFile"></param>
        public static void RecognizerTest(double[] scoreArray, FileInfo wavFile)
        {
            Log.Info("# TESTING: Starting benchmark test for the Bhutan Herron recognizer:");
            string subDir = "/TestData";
            var dir = wavFile.DirectoryName;
            var fileName = wavFile.Name;
            fileName = fileName.Substring(0, fileName.Length - 4);
            var scoreFilePath  = Path.Combine(dir + subDir, fileName + ".TestScores.csv");
            var scoreFile  = new FileInfo(scoreFilePath);
            if (! scoreFile.Exists)
            {
                Log.Warn("   Score Test file does not exist.    Writing output as future score-test file");
                FileTools.WriteArray2File(scoreArray, scoreFilePath);
            }
            else // else if the scores file exists then do a compare.
            {
                bool ok = true;
                var scoreLines = FileTools.ReadTextFile(scoreFilePath);
                for (int i = 0; i < scoreLines.Count; i++)
                {
                    var str = scoreArray[i].ToString();
                    if (!scoreLines[i].Equals(str))
                    {
                        Log.Warn($"Line {i}: {str} NOT= benchmark <{scoreLines[i]}>");
                        ok = false;
                    }
                }
                if (ok)
                {
                    Log.Info("   SUCCESS! Passed the SCORE ARRAY TEST.");
                }
                else
                {
                    Log.Warn("   FAILED THE SCORE ARRAY TEST");
                }
            }
            Log.Info("Completed benchmark test for the Bhutan Herron recognizer.");
        }



        /// <summary>
        /// This test checks an array of acoustic events (array of EventBase) against a standard or benchmark previously stored.
        /// If the benchmark file does not exist then the array of EventBase is written to a text file.
        /// If a benchmark does exist the current array is first written to file and then both
        /// current (test) file and the benchmark file are read as text files and compared.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="wavFile"></param>
        public static void RecognizerTest(IEnumerable<EventBase> events, FileInfo wavFile)
        {
            Log.Info("# TESTING: Starting benchmark test for the ArdeaInsignis recognizer:");
            string subDir = "/TestData";
            var dir = wavFile.DirectoryName;
            var fileName = wavFile.Name;
            fileName = fileName.Substring(0, fileName.Length - 4);
            var testEventsFilePath = Path.Combine(dir + subDir, fileName + ".TestEvents.txt");
            var eventsFile = new FileInfo(testEventsFilePath);

            if (!eventsFile.Exists)
            {
                Log.Warn("   Events Test file does not exist.");
                if (events.Count() == 0)
                {
                    Log.Warn("   There are no events, so an events test file will not be written.");
                }
                else
                {
                    Log.Warn("   Writing events array as future test file");
                    Csv.WriteToCsv<EventBase>(eventsFile, events);
                }
            }
            else // else if the events file exists then do a compare.
            {

                bool AOK = true;
                var newEventsFilePath = Path.Combine(dir + subDir, fileName + ".NewEvents.txt");
                var newEventsFile = new FileInfo(newEventsFilePath);
                Csv.WriteToCsv(newEventsFile, events);
                var testEventLines = FileTools.ReadTextFile(testEventsFilePath);
                var newEventLines = FileTools.ReadTextFile(newEventsFilePath);
                for (int i = 0; i < testEventLines.Count; i++)
                {
                    if (!testEventLines[i].Equals(newEventLines[i]))
                    {
                        Log.Warn($"Line {i}: {testEventLines[i]} NOT= benchmark <{newEventLines[i]}>");
                        AOK = false;
                    }
                }
                if (AOK)
                {
                    Log.Info("   SUCCESS! Passed the EVENTS ARRAY TEST.");
                }
                else
                {
                    Log.Warn("   FAILED THE EVENTS ARRAY TEST");
                }
            }
            Log.Info("Completed benchmark test for the ArdeaInsignis recognizer.");
        }



    }
}
