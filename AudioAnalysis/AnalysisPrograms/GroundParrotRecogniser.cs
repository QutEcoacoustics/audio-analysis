// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroundParrotRecogniser.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;

    using AnalysisPrograms.Processing;

    using AudioAnalysisTools;

    using QutSensors.AudioAnalysis.AED;

    using TowseyLib;

    /// <summary>
    /// The ground parrot recogniser.
    /// </summary>
    internal class GroundParrotRecogniser
    {
        #region Public Methods

        /// <summary>
        /// The detect.
        /// </summary>
        /// <param name="wavFilePath">
        /// The wav file path.
        /// </param>
        /// <returns>
        /// </returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(string wavFilePath, double intensityThreshold, double bandPassFilterMaximum, double bandPassFilterMinimum, int smallAreaThreshold)
        {
            Tuple<BaseSonogram, List<AcousticEvent>> aed = AED.Detect(wavFilePath, intensityThreshold, smallAreaThreshold, bandPassFilterMinimum, bandPassFilterMaximum);

            // TODO: hacks remove
            ////ProcessingTypes.SaveAeCsv(aed.Item2, @"C:\SensorNetworks\Output\", wavFilePath);

            var events = new List<Util.Rectangle<double, double>>();
            foreach (AcousticEvent ae in aed.Item2)
            {
                events.Add(Util.fcornersToRect(ae.StartTime, ae.EndTime, ae.MaxFreq, ae.MinFreq));
            }

            Log.WriteLine("EPR start");

            IEnumerable<Tuple<Util.Rectangle<double, double>, double>> eprRects =
                EventPatternRecog.DetectGroundParrots(events);
            Log.WriteLine("EPR finished");

            SonogramConfig config = aed.Item1.Configuration;
            double framesPerSec = 1 / config.GetFrameOffset(); // Surely this should go somewhere else
            double freqBinWidth = config.fftConfig.NyquistFreq / (double)config.FreqBinCount;
                
                // TODO this is common with AED
            var eprEvents = new List<AcousticEvent>();
            foreach (var rectScore in eprRects)
            {
                var ae = new AcousticEvent(
                    rectScore.Item1.Left, rectScore.Item1.Width, rectScore.Item1.Bottom, rectScore.Item1.Top);
                ae.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                ae.SetScores(rectScore.Item2, 0, 1);
                eprEvents.Add(ae);
            }

            return Tuple.Create(aed.Item1, eprEvents);
        }

        /// <summary>
        /// The standard dev method.
        /// </summary>
        /// <param name="args">
        /// The args passed into executable.
        /// </param>
        public static void Dev(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply a .wav recording as a command line argument.");
                Console.WriteLine(
                    "Example: \"trunk\\AudioAnalysis\\Matlab\\EPR\\Ground Parrot\\GParrots_JB2_20090607-173000.wav_minute_3.wav\"");
                Environment.Exit(1);
            }
            else
            {
                Log.Verbosity = 1;
                string wavFilePath = args[0];
                string iniPath = args[1];

                // READ PARAMETER VALUES FROM INI FILE
                double intensityThreshold;
                double bandPassFilterMaximum;
                double bandPassFilterMinimum;
                int smallAreaThreshold;
                AED.GetAedParametersFromConfigFileOrDefaults(iniPath, out intensityThreshold, out bandPassFilterMaximum, out bandPassFilterMinimum, out smallAreaThreshold);

                Tuple<BaseSonogram, List<AcousticEvent>> result = Detect(wavFilePath, intensityThreshold, bandPassFilterMaximum, bandPassFilterMinimum, smallAreaThreshold);
                List<AcousticEvent> eprEvents = result.Item2;

                eprEvents.Sort(
                    delegate(AcousticEvent ae1, AcousticEvent ae2) { return ae1.StartTime.CompareTo(ae2.StartTime); });

                Console.WriteLine();
                foreach (AcousticEvent ae in eprEvents)
                {
                    Console.WriteLine(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                }

                Console.WriteLine();

                AED.GenerateImage(wavFilePath, @"C:\SensorNetworks\Output\", result.Item1, eprEvents);
                ProcessingTypes.SaveAeCsv(eprEvents, @"C:\SensorNetworks\Output\", wavFilePath);

                Log.WriteLine("Finished");
            }
        }


        #endregion
    }
}