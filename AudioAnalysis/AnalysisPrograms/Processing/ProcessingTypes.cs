// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessingTypes.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Processing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using AudioAnalysisTools;
    using AudioAnalysisTools.HTKTools;

    using QutSensors.AudioAnalysis.AED;
    using QutSensors.Shared;

    using TowseyLib;

    /// <summary>
    /// The processing types.
    /// </summary>
    public static class ProcessingTypes
    {
        /// <summary>
        /// acoustic event detection.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file.
        /// </param>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        internal static IEnumerable<ProcessorResultTag> RunAED(FileInfo settingsFile, FileInfo audioFile)
        {
            // settings
            string INTENSITY_THRESHOLD = "intensityThreshold".ToUpperInvariant();
            double intensityThreshold = Default.intensityThreshold;

            string SMALL_AREA_THRESHOLD = "smallAreaThreshold".ToUpperInvariant();
            int smallAreaThreshold = Default.smallAreaThreshold;

            var config = new Configuration(settingsFile.FullName);
            Dictionary<string, string> dict = config.GetTable();

            if (dict.ContainsKey(INTENSITY_THRESHOLD))
            {
                intensityThreshold = Convert.ToDouble(dict[INTENSITY_THRESHOLD]);
            }

            if (dict.ContainsKey(SMALL_AREA_THRESHOLD))
            {
                smallAreaThreshold = Convert.ToInt32(dict[SMALL_AREA_THRESHOLD]);
            }

            // execute
            Tuple<BaseSonogram, List<AcousticEvent>> result = AED.Detect(
                audioFile.FullName, intensityThreshold, smallAreaThreshold);
            List<AcousticEvent> events = result.Item2;

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, 
                        new ResultProperty(
                        ae.Name, 
                        null, 
                        new Dictionary<string, string>
                            {
                               { "Description", "Normalised score is not applicable to AED." } 
                            })));
        }

        /// <summary>
        /// Oscillation Recogniser.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file.
        /// </param>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        internal static IEnumerable<ProcessorResultTag> RunOD(FileInfo settingsFile, FileInfo audioFile)
        {
            // settings
            var config = new Configuration(settingsFile.FullName);
            Dictionary<string, string> dict = config.GetTable();

            int minHz = Int32.Parse(dict[OscillationRecogniser.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[OscillationRecogniser.key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[OscillationRecogniser.key_FRAME_OVERLAP]);
            double dctDuration = Double.Parse(dict[OscillationRecogniser.key_DCT_DURATION]);
            int minOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MIN_OSCIL_FREQ]);
            int maxOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MAX_OSCIL_FREQ]);
            double minAmplitude = Double.Parse(dict[OscillationRecogniser.key_MIN_AMPLITUDE]);
            double eventThreshold = Double.Parse(dict[OscillationRecogniser.key_EVENT_THRESHOLD]);
            double minDuration = Double.Parse(dict[OscillationRecogniser.key_MIN_DURATION]);
            double maxDuration = Double.Parse(dict[OscillationRecogniser.key_MAX_DURATION]);

            // execute
            Tuple<BaseSonogram, double[,], double[], List<AcousticEvent>, double[], TimeSpan> results =
                OscillationRecogniser.Execute_ODDetect(
                    audioFile.FullName, 
                    minHz, 
                    maxHz, 
                    frameOverlap, 
                    dctDuration, 
                    minOscilFreq, 
                    maxOscilFreq, 
                    minAmplitude, 
                    eventThreshold, 
                    minDuration, 
                    maxDuration);
            List<AcousticEvent> events = results.Item4;

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, 
                        new ResultProperty(
                        ae.Name, 
                        ae.NormalisedScore, 
                        new Dictionary<string, string> { { "Description", "Normalised score" } })));
        }

        /// <summary>
        /// Harmonic Recogniser.
        /// </summary>
        /// <param name="settingsFile">
        /// Settings text file.
        /// </param>
        /// <param name="audioFile">
        /// Audio file to analyse.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        internal static IEnumerable<ProcessorResultTag> RunHD(FileInfo settingsFile, FileInfo audioFile)
        {
            var config = new Configuration(settingsFile.FullName);
            Dictionary<string, string> dict = config.GetTable();

            int minHz = Int32.Parse(dict[HarmonicRecogniser.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[HarmonicRecogniser.key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[HarmonicRecogniser.key_FRAME_OVERLAP]);
            int minPeriod = Int32.Parse(dict[HarmonicRecogniser.key_MIN_HARMONIC_PERIOD]);
            int maxPeriod = Int32.Parse(dict[HarmonicRecogniser.key_MAX_HARMONIC_PERIOD]);
            double minAmplitude = Double.Parse(dict[HarmonicRecogniser.key_MIN_AMPLITUDE]);
            double eventThreshold = Double.Parse(dict[HarmonicRecogniser.key_EVENT_THRESHOLD]);
            double expectedDuration = Double.Parse(dict[HarmonicRecogniser.key_DURATION]);

            Tuple<BaseSonogram, double[,], double[], List<AcousticEvent>> results =
                HarmonicRecogniser.Execute_HDDetect(
                    audioFile.FullName,
                    minHz,
                    maxHz,
                    frameOverlap,
                    minPeriod,
                    maxPeriod,
                    minAmplitude,
                    eventThreshold,
                    expectedDuration);

            List<AcousticEvent> predictedEvents = results.Item4;

            // AcousticEvent results
            IEnumerable<ProcessorResultTag> prts =
                predictedEvents.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae,
                        new ResultProperty(
                        ae.Name,
                        ae.NormalisedScore,
                        new Dictionary<string, string> { { "Description", "Normalised score" } })));

            return prts;
        }

        /// <summary>
        /// event pattern recognition.
        /// </summary>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        internal static IEnumerable<ProcessorResultTag> RunEPR(FileInfo audioFile)
        {
            // no settings, yet

            // execute
            Tuple<BaseSonogram, List<AcousticEvent>> result = GroundParrotRecogniser.Detect(audioFile.FullName);
            List<AcousticEvent> events = result.Item2;

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, 
                        new ResultProperty(
                        ae.Name, 
                        ae.NormalisedScore, 
                        new Dictionary<string, string> { { "Description", "Normalised score" } })));
        }

        /// <summary>
        /// signal to noise ratio.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file.
        /// </param>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        internal static IEnumerable<ProcessorResultTag> RunSNR(FileInfo settingsFile, FileInfo audioFile)
        {
            var config = new Configuration(settingsFile.FullName);
            Dictionary<string, string> dict = config.GetTable();

            int frameSize = Int32.Parse(dict[SnrAnalysis.key_FRAME_SIZE]);
            double frameOverlap = Double.Parse(dict[SnrAnalysis.key_FRAME_OVERLAP]);
            string windowFunction = dict[SnrAnalysis.key_WINDOW_FUNCTION];
            int N_PointSmoothFFT = Int32.Parse(dict[SnrAnalysis.key_N_POINT_SMOOTH_FFT]);
            string noiseReduceType = dict[SnrAnalysis.key_NOISE_REDUCTION_TYPE];

            int minHz = Int32.Parse(dict[SnrAnalysis.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[SnrAnalysis.key_MAX_HZ]);
            double segK1 = Double.Parse(dict[SnrAnalysis.key_SEGMENTATION_THRESHOLD_K1]);
            double segK2 = Double.Parse(dict[SnrAnalysis.key_SEGMENTATION_THRESHOLD_K2]);
            double latency = Double.Parse(dict[SnrAnalysis.key_K1_K2_LATENCY]);
            double vocalGap = Double.Parse(dict[SnrAnalysis.key_VOCAL_GAP]);

            double intensityThreshold = Default.intensityThreshold;
            if (dict.ContainsKey(SnrAnalysis.key_AED_INTENSITY_THRESHOLD))
            {
                intensityThreshold = Double.Parse(dict[SnrAnalysis.key_AED_INTENSITY_THRESHOLD]);
            }

            int smallAreaThreshold = Default.smallAreaThreshold;
            if (dict.ContainsKey(SnrAnalysis.key_AED_SMALL_AREA_THRESHOLD))
            {
                smallAreaThreshold = Int32.Parse(dict[SnrAnalysis.key_AED_SMALL_AREA_THRESHOLD]);
            }

            Tuple<BaseSonogram, AcousticEvent, AcousticEvent> results = SnrAnalysis.Execute_Sonogram(
                audioFile.FullName, 
                frameSize, 
                frameOverlap, 
                windowFunction, 
                N_PointSmoothFFT, 
                noiseReduceType, 
                minHz, 
                maxHz, 
                segK1, 
                segK2, 
                latency, 
                vocalGap);

            BaseSonogram sonogram = results.Item1;
            AcousticEvent SNR_fullbandEvent = results.Item2;
            AcousticEvent SNR_subbandEvent = results.Item3;
            List<AcousticEvent> predictedEvents = AED.Detect(sonogram, intensityThreshold, smallAreaThreshold);

            // AcousticEvent results
            IEnumerable<ProcessorResultTag> prts =
                predictedEvents.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, 
                        new ResultProperty(
                        ae.Name, 
                        ae.NormalisedScore, 
                        new Dictionary<string, string> { { "Description", "Normalised score" } })));

            return prts;
        }

        /// <summary>
        /// Runs a prepared HTK template over a file.
        /// </summary>
        /// <param name="templateFile">
        /// A zip file containing the resources required to run HTK.
        /// </param>
        /// <param name="workingDirectory">
        /// Working Directory.
        /// </param>
        /// <param name="audioFile">
        /// Audio file to analyse.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        internal static IEnumerable<ProcessorResultTag> RunHTK(
            FileInfo templateFile, DirectoryInfo workingDirectory, FileInfo audioFile)
        {
            Tuple<HTKConfig, List<AcousticEvent>> results = HTKRecogniser.Execute(
                audioFile.FullName, templateFile.FullName, workingDirectory.FullName);

            List<AcousticEvent> events = results.Item2;

            // AcousticEvent results
            IEnumerable<ProcessorResultTag> prts =
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, 
                        new ResultProperty(
                        ae.Name, 
                        ae.NormalisedScore, 
                        new Dictionary<string, string> { { "Description", "Normalised score" } })));

            return prts;
        }
    }
}