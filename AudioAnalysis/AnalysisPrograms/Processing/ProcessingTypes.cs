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
    using QutSensors.AudioAnalysis.AED;
    using QutSensors.Shared;

    using TowseyLib;

    /// <summary>
    /// The processing types.
    /// </summary>
    public static class ProcessingTypes
    {
        /// <summary>
        /// Segmentation Utility.
        /// </summary>
        /// <param name="settingsFile">Settings file.</param>
        /// <param name="audioFile">Audio file.</param>
        /// <returns>Processing results.</returns>
        internal static IEnumerable<ProcessorResultTag> RunSegmentation(FileInfo settingsFile, FileInfo audioFile)
        {
            var config = new Configuration(settingsFile.FullName);
            var dict = config.GetTable();

            var minHz = Int32.Parse(dict[Segment.key_MIN_HZ]);
            var maxHz = Int32.Parse(dict[Segment.key_MAX_HZ]);
            var frameOverlap = Double.Parse(dict[Segment.key_FRAME_OVERLAP]);
            var smoothWindow = Double.Parse(dict[Segment.key_SMOOTH_WINDOW]);
            var thresholdSd = Double.Parse(dict[Segment.key_THRESHOLD]);      
            var minDuration = Double.Parse(dict[Segment.key_MIN_DURATION]);   
            var maxDuration = Double.Parse(dict[Segment.key_MAX_DURATION]);   

            var results = Segment.Execute_Segmentation(
                audioFile.FullName, 
                minHz, 
                maxHz, 
                frameOverlap,
                smoothWindow,
                thresholdSd,
                minDuration, 
                maxDuration);
            var events = results.Item2;

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae,
                        new ResultProperty(ae.Name, ae.ScoreNormalised,
                            new Dictionary<string, string>
                                {
                                    { "Description", "Normalised score" }
                                })));
        }

        /// <summary>
        /// Acoustic event detection utility.
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
        internal static IEnumerable<ProcessorResultTag> RunAed(FileInfo settingsFile, FileInfo audioFile)
        {
            var config = new Configuration(settingsFile.FullName);
            var dict = config.GetTable();

            var intensityThreshold = Default.intensityThreshold;
            var smallAreaThreshold = Default.smallAreaThreshold;

            if (dict.ContainsKey(AED.key_INTENSITY_THRESHOLD) && dict.ContainsKey(AED.key_SMALLAREA_THRESHOLD))
            {
                intensityThreshold = Convert.ToDouble(dict[AED.key_INTENSITY_THRESHOLD]);
                smallAreaThreshold = Convert.ToInt32(dict[AED.key_SMALLAREA_THRESHOLD]);
            }

            // execute
            var result = AED.Detect(audioFile.FullName, intensityThreshold, smallAreaThreshold);
            var events = result.Item2;

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae,
                        new ResultProperty(ae.Name, null,
                            new Dictionary<string, string>
                                {
                                    { "Description", "Normalised score is not applicable to AED." }
                                })));
        }

        /// <summary>
        /// signal to noise ratio utility.
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
        /// <exception cref="NotImplementedException">Not sure what to return as results.</exception>
        internal static IEnumerable<ProcessorResultTag> RunSnr(FileInfo settingsFile, FileInfo audioFile)
        {
            var config = new Configuration(settingsFile.FullName);
            var dict = config.GetTable();

            var frameSize = Int32.Parse(dict[SnrAnalysis.key_FRAME_SIZE]);
            var frameOverlap = Double.Parse(dict[SnrAnalysis.key_FRAME_OVERLAP]);
            var windowFunction = dict[SnrAnalysis.key_WINDOW_FUNCTION];
            var nPointSmoothFft = Int32.Parse(dict[SnrAnalysis.key_N_POINT_SMOOTH_FFT]);
            var noiseReduceType = dict[SnrAnalysis.key_NOISE_REDUCTION_TYPE];

            var minHz = Int32.Parse(dict[SnrAnalysis.key_MIN_HZ]);
            var maxHz = Int32.Parse(dict[SnrAnalysis.key_MAX_HZ]);
            var segK1 = Double.Parse(dict[SnrAnalysis.key_SEGMENTATION_THRESHOLD_K1]);
            var segK2 = Double.Parse(dict[SnrAnalysis.key_SEGMENTATION_THRESHOLD_K2]);
            var latency = Double.Parse(dict[SnrAnalysis.key_K1_K2_LATENCY]);
            var vocalGap = Double.Parse(dict[SnrAnalysis.key_VOCAL_GAP]);

            var results = SnrAnalysis.Execute_Sonogram(
                audioFile.FullName,
                frameSize,
                frameOverlap,
                windowFunction,
                nPointSmoothFft,
                noiseReduceType,
                minHz,
                maxHz,
                segK1,
                segK2,
                latency,
                vocalGap);

            var sonogram = results.Item1;
            var snrFullbandEvent = results.Item2;
            var snrSubbandEvent = results.Item3;

            // TODO: Results are not regions, but information about the recording. This information should just be stored and displayed.
            throw new NotImplementedException();
            ////return null;
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
        internal static IEnumerable<ProcessorResultTag> RunOd(FileInfo settingsFile, FileInfo audioFile)
        {
            // settings
            var config = new Configuration(settingsFile.FullName);
            var dict = config.GetTable();

            bool doSegmentation = Boolean.Parse(dict[OscillationRecogniser.key_DO_SEGMENTATION]);
            var minHz = Int32.Parse(dict[OscillationRecogniser.key_MIN_HZ]);
            var maxHz = Int32.Parse(dict[OscillationRecogniser.key_MAX_HZ]);
            var frameOverlap = Double.Parse(dict[OscillationRecogniser.key_FRAME_OVERLAP]);
            var dctDuration  = Double.Parse(dict[OscillationRecogniser.key_DCT_DURATION]);
            var dctThreshold = Double.Parse(dict[OscillationRecogniser.key_DCT_THRESHOLD]);
            var minOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MIN_OSCIL_FREQ]);
            var maxOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MAX_OSCIL_FREQ]);
            var eventThreshold = Double.Parse(dict[OscillationRecogniser.key_EVENT_THRESHOLD]);
            var minDuration = Double.Parse(dict[OscillationRecogniser.key_MIN_DURATION]);
            var maxDuration = Double.Parse(dict[OscillationRecogniser.key_MAX_DURATION]);

            // execute
            var results =
                OscillationRecogniser.Execute_ODDetect(
                    audioFile.FullName,
                    doSegmentation,
                    minHz,
                    maxHz,
                    frameOverlap,
                    dctDuration,
                    dctThreshold,
                    minOscilFreq,
                    maxOscilFreq,
                    eventThreshold,
                    minDuration,
                    maxDuration);
            var events = results.Item4;

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae,
                        new ResultProperty(ae.Name, ae.ScoreNormalised,
                            new Dictionary<string, string>
                                {
                                    { "Description", "Normalised score" }
                                })));
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
        internal static IEnumerable<ProcessorResultTag> RunHd(FileInfo settingsFile, FileInfo audioFile)
        {
            var config = new Configuration(settingsFile.FullName);
            var dict = config.GetTable();

            var minHz = Int32.Parse(dict[HarmonicRecogniser.key_MIN_HZ]);
            var maxHz = Int32.Parse(dict[HarmonicRecogniser.key_MAX_HZ]);
            var frameOverlap = Double.Parse(dict[HarmonicRecogniser.key_FRAME_OVERLAP]);
            var minPeriod = Int32.Parse(dict[HarmonicRecogniser.key_MIN_HARMONIC_PERIOD]);
            var maxPeriod = Int32.Parse(dict[HarmonicRecogniser.key_MAX_HARMONIC_PERIOD]);
            var minAmplitude = Double.Parse(dict[HarmonicRecogniser.key_MIN_AMPLITUDE]);
            var eventThreshold = Double.Parse(dict[HarmonicRecogniser.key_EVENT_THRESHOLD]);
            var expectedDuration = Double.Parse(dict[HarmonicRecogniser.key_DURATION]);

            var results =
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

            var predictedEvents = results.Item4;

            // AcousticEvent results
            var prts =
                predictedEvents.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae,
                        new ResultProperty(
                        ae.Name,
                        ae.ScoreNormalised,
                        new Dictionary<string, string> { { "Description", "Normalised score" } })));

            return prts;
        }

        /// <summary>
        /// event pattern recogniser.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings File.
        /// </param>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        internal static IEnumerable<ProcessorResultTag> RunEpr(FileInfo settingsFile, FileInfo audioFile)
        {
            // no settings, yet

            // execute
            var result = GroundParrotRecogniser.Detect(audioFile.FullName);
            var events = result.Item2;

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae,
                        new ResultProperty(ae.Name, ae.ScoreNormalised,
                            new Dictionary<string, string>
                            {
                                { "Description", "Normalised score" }
                            })));
        }

        /// <summary>
        /// Spectral Peak Tracking Recogniser.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file.
        /// </param>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <returns>Processing results.
        /// </returns>
        /// <exception cref="NotImplementedException">Not completed.
        /// </exception>
        internal static IEnumerable<ProcessorResultTag> RunSpt(FileInfo settingsFile, FileInfo audioFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// HTK template Recogniser.
        /// </summary>
        /// <param name="resourceFile">
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
        internal static IEnumerable<ProcessorResultTag> RunHtk(FileInfo resourceFile, DirectoryInfo workingDirectory, FileInfo audioFile)
        {
            var results = HTKRecogniser.Execute(audioFile.FullName, resourceFile.FullName, workingDirectory.FullName);

            var events = results.Item2;

            // AcousticEvent results
            var prts =
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae,
                        new ResultProperty(ae.Name,ae.ScoreNormalised,
                            new Dictionary<string, string>
                                {
                                    { "Description", "Normalised score" }
                                })));

            return prts;
        }

        /// <summary>
        /// MFCC OD Regoniser.
        /// </summary>
        /// <param name="resourceFile">Compressed resource file.</param>
        /// <param name="runDir">Working directory.</param>
        /// <param name="audioFile">Audio file to analyse.</param>
        /// <returns>Processing results.</returns>
        internal static IEnumerable<ProcessorResultTag> RunMfccOd(FileInfo resourceFile, DirectoryInfo runDir, FileInfo audioFile)
        {
            throw new NotImplementedException();
        }
    }
}