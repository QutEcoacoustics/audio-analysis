// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroundParrotRecogniser.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    //using AnalysisPrograms.Processing;

    using AudioAnalysisTools;

    using QutSensors.AudioAnalysis.AED;

    using TowseyLib;

    /// <summary>
    /// The ground parrot recogniser.
    /// </summary>
    internal class GroundParrotRecogniser
    {
        // Keys to recognise identifiers in PARAMETERS - INI file. 
        #region Constants and Fields

        /// <summary>
        /// The Key Normalised Min Score.
        /// </summary>
        public static string KeyNormalisedMinScore = "NORMALISED_MIN_SCORE";

        /// <summary>
        /// This is the ground parrot template used by Birigt and hard coded by Brad.
        /// It defines a set of 15 chirps enclosed in a rectangle.
        /// Each row represents one rectanlge or chirp.
        /// Col1: start of the rectangle in seconds from beginning of the recording
        /// Col2: end   of the rectangle in seconds from beginning of the recording
        /// Col3: maximum freq (Hz) of the rectangle
        /// Col4: minimum freq (Hz) of the rectangle
        /// </summary>
        public static double[,] groundParrotTemplate1 =
        {
            {13.374694, 13.548844, 3832.910156, 3617.578125},
            {13.664943, 13.792653, 3919.042969, 3660.644531},
            {13.920363, 14.117732, 3962.109375, 3703.710938},
            {14.257052, 14.349932, 4005.175781, 3832.910156},
            {14.512472, 14.640181, 4048.242188, 3919.042969},
            {14.814331, 14.895601, 4220.507813, 4048.242188},
            {15.046531, 15.232290, 4349.707031, 4048.242188},
            {15.371610, 15.499320, 4435.839844, 4177.441406},
            {15.615420, 15.812789, 4478.906250, 4220.507813},
            {16.277188, 16.462948, 4608.105469, 4263.574219},
            {16.590658, 16.695147, 4694.238281, 4392.773438},
            {16.834467, 17.020227, 4694.238281, 4392.773438},
            {17.147937, 17.264036, 4737.304688, 4478.906250},
            {17.391746, 17.577506, 4823.437500, 4478.906250},
            {17.705215, 17.821315, 4780.371094, 4521.972656} 
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Detect using EPR.
        /// </summary>
        /// <param name="wavFilePath">
        /// The wav file path.
        /// </param>
        /// <param name="intensityThreshold">
        /// The intensity Threshold.
        /// </param>
        /// <param name="bandPassFilterMaximum">
        /// The band Pass Filter Maximum.
        /// </param>
        /// <param name="bandPassFilterMinimum">
        /// The band Pass Filter Minimum.
        /// </param>
        /// <param name="smallAreaThreshold">
        /// The small Area Threshold.
        /// </param>
        /// <param name="eprNormalisedMinScore">
        /// The epr Normalised Min Score.
        /// </param>
        /// <returns>
        /// Tuple containing base Sonogram and list of acoustic events.
        /// </returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(
            string wavFilePath,
            double intensityThreshold,
            double bandPassFilterMaximum,
            double bandPassFilterMinimum,
            int smallAreaThreshold,
            double eprNormalisedMinScore)
        {
            Tuple<BaseSonogram, List<AcousticEvent>> aed = AED.Detect(wavFilePath, intensityThreshold, smallAreaThreshold, bandPassFilterMinimum, bandPassFilterMaximum);

            return Detect(aed, eprNormalisedMinScore, wavFilePath);
        }

        /// <summary>
        /// Epr Detect.
        /// </summary>
        /// <param name="aed">
        /// The AED results.
        /// </param>
        /// <param name="eprNormalisedMinScore">
        /// The epr normalised min score.
        /// </param>
        /// <param name="wavFilePath">
        /// The wav file path.
        /// </param>
        /// <returns>
        /// Sonogram and events.
        /// </returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(Tuple<BaseSonogram, List<AcousticEvent>> aed, double eprNormalisedMinScore, string wavFilePath)
        {
            var events = new List<Util.Rectangle<double, double>>();
            foreach (AcousticEvent ae in aed.Item2)
            {
                events.Add(Util.fcornersToRect(ae.TimeStart, ae.TimeEnd, ae.MaxFreq, ae.MinFreq));
            }

            Log.WriteLine("EPR start");

            IEnumerable<Tuple<Util.Rectangle<double, double>, double>> eprRects =
                EventPatternRecog.DetectGroundParrots(events, eprNormalisedMinScore);
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
                
                throw new AnalysisOptionInvalidArgumentsException();
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

                Tuple<BaseSonogram, List<AcousticEvent>> result = Detect(wavFilePath, intensityThreshold, bandPassFilterMaximum, bandPassFilterMinimum, smallAreaThreshold, Default.eprNormalisedMinScore);
                List<AcousticEvent> eprEvents = result.Item2;

                eprEvents.Sort((ae1, ae2) => ae1.TimeStart.CompareTo(ae2.TimeStart));

                Console.WriteLine();
                foreach (AcousticEvent ae in eprEvents)
                {
                    Console.WriteLine(ae.TimeStart + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                }

                Console.WriteLine();

                string outputFolder = Path.GetDirectoryName(iniPath) ?? @"C:\SensorNetworks\Output\";
                AED.GenerateImage(wavFilePath, outputFolder, result.Item1, eprEvents);
                //ProcessingTypes.SaveAeCsv(eprEvents, outputFolder, wavFilePath);

                Log.WriteLine("Finished");
            }
        }


        #endregion

        #region helper methods

        /// <summary>
        /// Get epr parameters from init file.
        /// </summary>
        /// <param name="iniPath">
        /// The ini path.
        /// </param>
        /// <param name="normalisedMinScore">
        /// The normalised min score.
        /// </param>
        internal static void GetEprParametersFromConfigFileOrDefaults(string iniPath, out double normalisedMinScore)
        {
            var config = new ConfigDictionary(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            int propertyUsageCount = 0;

            normalisedMinScore = Default.eprNormalisedMinScore;

            if (dict.ContainsKey(KeyNormalisedMinScore))
            {
                normalisedMinScore = Convert.ToDouble(dict[KeyNormalisedMinScore]);
                propertyUsageCount++;
            }

            Log.WriteIfVerbose("Using {0} file params and {1} EPR defaults", propertyUsageCount, 1 - propertyUsageCount);
        }


        /// <summary>
        /// Takes the template defined by Birgit and converts it to integer bins using hte user supplied time & hz scales
        /// </summary>
        /// <param name="timeScale">seconds per frame.</param>
        /// <param name="hzScale">herz per freq bin</param>
        /// <returns></returns>
        public static int[,] ReadGroundParrotTemplateAsMatrix(double timeScale, int hzScale)
        {
            int rows = groundParrotTemplate1.GetLength(0);
            int cols = groundParrotTemplate1.GetLength(1);
            double timeOffset = groundParrotTemplate1[0, 0];
            var gpTemplate = new int[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                gpTemplate[r, 0] = (int)Math.Round((groundParrotTemplate1[r, 0] - timeOffset) / timeScale);
                gpTemplate[r, 1] = (int)Math.Round((groundParrotTemplate1[r, 1] - timeOffset) / timeScale);
                gpTemplate[r, 2] = (int)Math.Round((groundParrotTemplate1[r, 2] / hzScale));
                gpTemplate[r, 3] = (int)Math.Round((groundParrotTemplate1[r, 3] - groundParrotTemplate1[r, 2]) / hzScale);
            }
            return gpTemplate;
        }

        public static List<AcousticEvent> ReadGroundParrotTemplateAsList(double timeScale, int hzScale)
        {
            int rows = groundParrotTemplate1.GetLength(0);
            int cols = groundParrotTemplate1.GetLength(1);
            double timeOffset = groundParrotTemplate1[0, 0];
            var gpTemplate = new List<AcousticEvent>();
            for (int r = 0; r < rows; r++)
            {
                int t1 = (int)Math.Round((groundParrotTemplate1[r, 0] - timeOffset) / timeScale);
                int t2 = (int)Math.Round((groundParrotTemplate1[r, 1] - timeOffset) / timeScale);
                int f2 = (int)Math.Round(groundParrotTemplate1[r, 2] / hzScale);
                int f1 = (int)Math.Round(groundParrotTemplate1[r, 3] / hzScale);
                Oblong o = new Oblong(t1,f1, t2, f2);
                gpTemplate.Add(new AcousticEvent(o, timeScale, hzScale));
            }
            return gpTemplate;
        }

        #endregion
    }
}