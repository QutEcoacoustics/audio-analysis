// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RhinellaMarina.cs" company="QutBioacoustics">
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
    using System.Text;

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
    /// AKA: The bloody canetoad
    /// This is a frog recognizer based on the "ribit" or "washboard" template
    /// It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    /// 
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs 
    /// e.g. Litoria rothii and Litoria olongburesnsis.
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>
    class RhinellaMarina : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "RhinellaMarina";

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
        /// <param name="audioRecording"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording recording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";


            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int maxHz = (int)configuration[AnalysisKeys.MaxHz];

            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);
            // duration of DCT in seconds 
            double dctDuration = (double)configuration[AnalysisKeys.DctDuration];

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            // ignore oscillations below this threshold freq
            int minOscilFreq = (int)configuration[AnalysisKeys.MinOscilFreq];

            // ignore oscillations above this threshold freq
            int maxOscilFreq = (int)configuration[AnalysisKeys.MaxOscilFreq];

            // min duration of event in seconds 
            double minDuration = (double)configuration[AnalysisKeys.MinDuration];

            // max duration of event in seconds                 
            double maxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            // this default framesize seems to work for Canetoad
            const int FrameSize = 512;
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                FrameSize,
                maxOscilFreq);
            //windowOverlap = 0.75; // previous default


            // DEBUG: Following line used to search for where indeterminism creeps into the spectrogram values which vary from run to run. 
            //FileTools.AddArrayAdjacentToExistingArrays(Path.Combine(outputDirectory.FullName, recording.BaseName+"_RecordingSamples.csv"), recording.WavReader.GetChannel(0));



            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = FrameSize,
                WindowOverlap = windowOverlap,
                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),
                // if do not use noise reduction can get a more sensitive recogniser.
                NoiseReductionType = NoiseReductionType.NONE
            };

            // sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            TimeSpan recordingDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            /* #############################################################################################################################################
             * window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
             * 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
             * 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
             * 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz
             */

            // int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            // int maxbin = minBin + numberOfBins - 1;
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            // DEBUG: Following lines used to search for where indeterminism creeps into the spectrogram values which vary from run to run. 
            //double[] array = DataTools.Matrix2Array(sonogram.Data);
            //FileTools.AddArrayAdjacentToExistingArrays(Path.Combine(outputDirectory.FullName, recording.BaseName+".csv"), array);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            double minDurationOfAdvertCall = minDuration; // this boundary duration should = 5.0 seconds as of 4 June 2015.
            double minDurationOfReleaseCall = 1.0;
            double[] scores; // predefinition of score array
            List<AcousticEvent> events;
            double[,] hits;
            Oscillations2012.Execute(
                (SpectrogramStandard)sonogram,
                minHz,
                maxHz,
                dctDuration,
                minOscilFreq,
                maxOscilFreq,
                dctThreshold,
                eventThreshold,
                minDurationOfReleaseCall,
                maxDuration,
                out scores,
                out events,
                out hits);


            // DEBUG: Following line used to search for where indeterminism creeps into the event detection
            //FileTools.AddArrayAdjacentToExistingArrays(Path.Combine(outputDirectory.FullName, recording.BaseName+"_ScoreArray.csv"), scores);


            var prunedEvents = new List<AcousticEvent>();

            for (int i = 0; i < events.Count; i++)
            {
                AcousticEvent ae = events[i];
                if (ae.Duration < minDurationOfReleaseCall)
                {
                    continue;
                }

                // add additional info
                ae.SpeciesName = speciesName;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recordingDuration;

                if (ae.Duration >= minDurationOfAdvertCall)
                {
                    ae.Name = abbreviatedSpeciesName + ".AdvertCall";
                    prunedEvents.Add(ae);
                    continue;
                }

                // release calls are shorter and we require higher score to reduce chance of false-positive.
                if (ae.Score > (eventThreshold + 0.4))
                {
                    ae.Name = abbreviatedSpeciesName + ".ReleaseCall";
                    prunedEvents.Add(ae);
                }
            };

            // do a recognizer test.
            RecognizerTest(scores, new FileInfo(recording.FilePath));
            RecognizerTest(prunedEvents, new FileInfo(recording.FilePath));

            var plot = new Plot(this.DisplayName, scores, eventThreshold);
            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = prunedEvents
                //Events = events
            };

        }



        /// <summary>
        /// This test checks a score array (array of doubles) against a standard or benchmark previously stored.
        /// If the benchmark file does not exist then the passed score array is written to become the benchmark.
        /// </summary>
        /// <param name="scoreArray"></param>
        /// <param name="wavFile"></param>
        public static void RecognizerTest(double[] scoreArray, FileInfo wavFile)
        {
            Log.Info("# TESTING: Starting benchmark test for the Canetoad recognizer:");
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
                bool allOK = true;
                var scoreLines = FileTools.ReadTextFile(scoreFilePath);
                for (int i = 0; i < scoreLines.Count; i++)
                {
                    string str = scoreArray[i].ToString();
                    if (!scoreLines[i].Equals(str))
                    {
                        Log.Warn(String.Format("Line {0}: {1} NOT= benchmark <{2}>", i, str, scoreLines[i]));
                        allOK = false;
                    }
                }
                if (allOK)
                {
                    Log.Info("   SUCCESS! Passed the SCORE ARRAY TEST.");
                }
                else
                {
                    Log.Warn("   FAILED THE SCORE ARRAY TEST");
                }
            }
            Log.Info("Completed benchmark test for the Canetoad recognizer.");
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
            Log.Info("# TESTING: Starting benchmark test for the Canetoad recognizer:");
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
                        Log.Warn(String.Format("Line {0}: {1} NOT= benchmark <{2}>", i, testEventLines[i], newEventLines[i]));
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
            Log.Info("Completed benchmark test for the Canetoad recognizer.");
        }



    }
}
