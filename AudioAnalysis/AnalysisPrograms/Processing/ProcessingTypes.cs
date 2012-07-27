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
    using System.Text;
    using System.Threading;
    using System.Xml.Serialization;

    using Acoustics.Shared;

    using AudioAnalysisTools;
    using AudioAnalysisTools.HTKTools;

    using TowseyLib;

    /// <summary>
    /// The processing types.
    /// </summary>
    public static class ProcessingTypes
    {
        private static Dictionary<string, string> DefaultNormalisedScore
        {
            get
            {
                return new Dictionary<string, string> { { "Description", "Normalised score" } };
            }
        }

        /// <summary>
        /// Save events to image.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        /// <param name="audioFilePath">
        /// The audio file path.
        /// </param>
        public static void SaveAeImage(List<AcousticEvent> events, string workingDir, string audioFilePath)
        {
            // don't want to save image or csv when run on cluster.
            ////return;

            ////SaveAeCsv(events, workingDir, audioFilePath);

            if (events != null && events.Count > 0)
            {
                var recording = new AudioRecording(audioFilePath);
                if (recording.SampleRate != 22050)
                {
                    recording.ConvertSampleRate22kHz();
                }

                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };

                var sonogram = new SpectralSonogram(config, recording.GetWavReader());

                string imagePath = Path.Combine(workingDir, Path.GetFileNameWithoutExtension(audioFilePath) + ".z" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");

                using (var image = new Image_MultiTrack(sonogram.GetImage(false, true)))
                {
                    image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                    image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                    image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
                    image.Save(imagePath);
                }
            }
        }

        /// <summary>
        /// Save events to csv file. Used in other areas.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        /// <param name="audioFilePath">
        /// The audio file path.
        /// </param>
        public static void SaveAeCsv(IEnumerable<AcousticEvent> events, string workingDir, string audioFilePath)
        {
            var aes = events.Select(e => new { StartTime = e.TimeStart, EndTime = e.TimeEnd, e.MinFreq, e.MaxFreq });

            var sb = new StringBuilder();
            sb.AppendLine("Start time, duration, min freq, max freq");

            foreach (var item in aes)
            {
                sb.AppendLine(
                    String.Format("{0},{1},{2},{3}", item.StartTime, item.EndTime - item.StartTime, item.MinFreq, item.MaxFreq));
            }

            string fileName = Path.GetFileNameWithoutExtension(audioFilePath) +
                              Guid.NewGuid().ToString().Substring(0, 5) + ".csv";

            File.WriteAllText(Path.Combine(workingDir, fileName), sb.ToString());
        }

        /// <summary>
        /// Segmentation Utility.
        /// </summary>
        /// <param name="settingsFile">Settings file.</param>
        /// <param name="audioFile">Audio file.</param>
        /// <returns>Processing results.</returns>
        internal static IEnumerable<ProcessorResultTag> RunSegment(FileInfo settingsFile, FileInfo audioFile)
        {
            var expected = new List<string>
                {
                    ////Segment.key_DRAW_SONOGRAMS, // not used
                    Segment.key_FRAME_OVERLAP,
                    Segment.key_MAX_DURATION,
                    Segment.key_MAX_HZ,
                    Segment.key_MIN_DURATION,
                    Segment.key_MIN_HZ,
                    Segment.key_SMOOTH_WINDOW,
                    Segment.key_THRESHOLD
                };

            var config = new ConfigDictionary(settingsFile.FullName);
            var dict = ProcessingUtils.RemoveEmpty(config.GetTable());

            ProcessingUtils.CheckParams(expected, dict.Select(d => d.Key));

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

            SaveAeImage(events, settingsFile.DirectoryName, audioFile.FullName);

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, new ResultProperty(ae.Name, ae.ScoreNormalised, DefaultNormalisedScore)));
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
            var expected = new List<string>
                {
                    ////SnrAnalysis.key_AED_INTENSITY_THRESHOLD, // not used
                    ////SnrAnalysis.key_AED_SMALL_AREA_THRESHOLD, // not used
                    ////SnrAnalysis.key_DRAW_SONOGRAMS, // not used
                    SnrAnalysis.key_FRAME_OVERLAP,
                    SnrAnalysis.key_FRAME_SIZE,
                    SnrAnalysis.key_K1_K2_LATENCY,
                    SnrAnalysis.key_MAX_HZ,
                    SnrAnalysis.key_MIN_HZ,
                    ////SnrAnalysis.key_MIN_VOCAL_DURATION, // not used
                    SnrAnalysis.key_N_POINT_SMOOTH_FFT,
                    //SnrAnalysis.key_NOISE_REDUCTION_TYPE,
                    SnrAnalysis.key_SEGMENTATION_THRESHOLD_K1,
                    SnrAnalysis.key_SEGMENTATION_THRESHOLD_K2,
                    ////SnrAnalysis.key_SILENCE_RECORDING_PATH, // not used
                    SnrAnalysis.key_VOCAL_GAP,
                    SnrAnalysis.key_WINDOW_FUNCTION
                };

            var config = new ConfigDictionary(settingsFile.FullName);
            var dict = ProcessingUtils.RemoveEmpty(config.GetTable());

            ProcessingUtils.CheckParams(expected, dict.Select(d => d.Key));

            var frameSize = Int32.Parse(dict[SnrAnalysis.key_FRAME_SIZE]);
            var frameOverlap = Double.Parse(dict[SnrAnalysis.key_FRAME_OVERLAP]);
            var windowFunction = dict[SnrAnalysis.key_WINDOW_FUNCTION];
            var nPointSmoothFft = Int32.Parse(dict[SnrAnalysis.key_N_POINT_SMOOTH_FFT]);

            //var noiseReduceType = dict[SnrAnalysis.key_NOISE_REDUCTION_TYPE];
            string noiseReduceType = string.Empty;

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

            /*
             * Results are not regions, but information about the recording. This information should just be stored and displayed.
             * uses Name, Score and ScoreComment
             */

            var fullbandEvent = new ResultProperty
                {
                    Key = snrFullbandEvent.Name,
                    Value = snrFullbandEvent.Score
                };
            fullbandEvent.AddInfo("Score Comment", snrFullbandEvent.ScoreComment);

            var subbandEvent = new ResultProperty
            {
                Key = snrSubbandEvent.Name,
                Value = snrSubbandEvent.Score
            };
            subbandEvent.AddInfo("Score Comment", snrSubbandEvent.ScoreComment);

            var snrResult = new ProcessorResultTag();
            snrResult.ExtraDetail.Add(fullbandEvent);
            snrResult.ExtraDetail.Add(subbandEvent);
            //todo: fill in start/end/freq for whole recording

            return new List<ProcessorResultTag> { snrResult };
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
            // TODO: mark delete when you check i did this right
            /*
            var expected = new List<string>
                {
                    AED.key_INTENSITY_THRESHOLD,
                    AED.key_SMALLAREA_THRESHOLD
                };

            var config = new Configuration(settingsFile.FullName);
            var dict = ProcessingUtils.RemoveEmpty(config.GetTable());

            ProcessingUtils.CheckParams(expected, dict.Select(d => d.Key));

            var intensityThreshold = Default.intensityThreshold;
            var smallAreaThreshold = Default.smallAreaThreshold;

            if (dict.ContainsKey(AED.key_INTENSITY_THRESHOLD) && dict.ContainsKey(AED.key_SMALLAREA_THRESHOLD))
            {
                intensityThreshold = Convert.ToDouble(dict[AED.key_INTENSITY_THRESHOLD]);
                smallAreaThreshold = Convert.ToInt32(dict[AED.key_SMALLAREA_THRESHOLD]);
            }*/
            double intensityThreshold;
            double bandPassFilterMaximum;
            double bandPassFilterMinimum;
            int smallAreaThreshold;
            AED.GetAedParametersFromConfigFileOrDefaults(settingsFile.ToString(), out intensityThreshold, out bandPassFilterMaximum, out bandPassFilterMinimum, out smallAreaThreshold);

            // execute
            var result = AED.Detect(audioFile.FullName, intensityThreshold, smallAreaThreshold);
            var events = result.Item2;

            SaveAeImage(events, settingsFile.DirectoryName, audioFile.FullName);

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(ae, new ResultProperty(ae.Name, null, DefaultNormalisedScore)));
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
            var expected = new List<string>
                {
                    OscillationRecogniser.key_DCT_DURATION,
                    OscillationRecogniser.key_DCT_THRESHOLD,
                    OscillationRecogniser.key_DO_SEGMENTATION,
                    ////OscillationRecogniser.key_DRAW_SONOGRAMS, // not used
                    OscillationRecogniser.key_EVENT_THRESHOLD,
                    OscillationRecogniser.key_FRAME_OVERLAP,
                    OscillationRecogniser.key_MAX_DURATION,
                    OscillationRecogniser.key_MAX_HZ,
                    OscillationRecogniser.key_MAX_OSCIL_FREQ,
                    OscillationRecogniser.key_MIN_DURATION,
                    OscillationRecogniser.key_MIN_HZ,
                    OscillationRecogniser.key_MIN_OSCIL_FREQ,
                };

            // settings
            var config = new ConfigDictionary(settingsFile.FullName);
            var dict = ProcessingUtils.RemoveEmpty(config.GetTable());

            ProcessingUtils.CheckParams(expected, dict.Select(d => d.Key));

            bool doSegmentation = Boolean.Parse(dict[OscillationRecogniser.key_DO_SEGMENTATION]);
            var minHz = Int32.Parse(dict[OscillationRecogniser.key_MIN_HZ]);
            var maxHz = Int32.Parse(dict[OscillationRecogniser.key_MAX_HZ]);
            var frameOverlap = Double.Parse(dict[OscillationRecogniser.key_FRAME_OVERLAP]);
            var dctDuration = Double.Parse(dict[OscillationRecogniser.key_DCT_DURATION]);
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

            SaveAeImage(events, settingsFile.DirectoryName, audioFile.FullName);

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, new ResultProperty(ae.Name, ae.ScoreNormalised, DefaultNormalisedScore)));
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
            double intensityThreshold;
            double bandPassFilterMaximum;
            double bandPassFilterMinimum;
            int smallAreaThreshold;
            AED.GetAedParametersFromConfigFileOrDefaults(settingsFile.ToString(), out intensityThreshold, out bandPassFilterMaximum, out bandPassFilterMinimum, out smallAreaThreshold);

            // aed first
            Tuple<BaseSonogram, List<AcousticEvent>> aed = AED.Detect(audioFile.FullName, intensityThreshold, smallAreaThreshold, bandPassFilterMinimum, bandPassFilterMaximum);

            // save aed image
            SaveAeImage(aed.Item2, settingsFile.DirectoryName, audioFile.FullName);

            // epr settings
            double normalisedMinScore; // 0-1
            GroundParrotRecogniser.GetEprParametersFromConfigFileOrDefaults(settingsFile.ToString(), out normalisedMinScore);

            // execute - only for ground parrot for now.
            Tuple<BaseSonogram, List<AcousticEvent>> result = GroundParrotRecogniser.Detect(aed, normalisedMinScore, audioFile.FullName);
            var events = result.Item2;

            SaveAeImage(events, settingsFile.DirectoryName, audioFile.FullName);

            // AcousticEvent results
            return
                events.Select(
                    ae =>
                    ProcessingUtils.GetProcessorResultTag(
                        ae, new ResultProperty(ae.Name, ae.ScoreNormalised, DefaultNormalisedScore)));
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
        /// Syntactic pattern recognition.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file.
        /// </param>
        /// <param name="audioFile">
        /// The audio file.
        /// </param>
        /// <returns>Processing results.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Not completed.
        /// </exception>
        //internal static IEnumerable<ProcessorResultTag> RunSpr(FileInfo settingsFile, FileInfo audioFile)
        //{
        //    var expected = new List<string>
        //        {
        //            SPR.key_CALL_NAME,
        //            ////SPR.key_DO_SEGMENTATION, // not used
        //            ////SPR.key_DRAW_SONOGRAMS, // not used
        //            SPR.key_EVENT_THRESHOLD,
        //            SPR.key_FRAME_OVERLAP,
        //            SPR.key_MAX_DURATION,
        //            SPR.key_MIN_DURATION,
        //            SPR.key_SPT_INTENSITY_THRESHOLD,
        //            SPR.key_SPT_SMALL_LENGTH_THRESHOLD,
        //            SPR.key_WHIP_DURATION,
        //            SPR.key_WHIP_MAX_HZ,
        //            SPR.key_WHIP_MIN_HZ,
        //            SPR.key_WHISTLE_DURATION,
        //            SPR.key_WHISTLE_MAX_HZ,
        //            SPR.key_WHISTLE_MIN_HZ
        //        };

        //    // settings
        //    var config = new ConfigDictionary(settingsFile.FullName);
        //    var dict = ProcessingUtils.RemoveEmpty(config.GetTable());

        //    ProcessingUtils.CheckParams(expected, dict.Select(d => d.Key));

        //    string callName = dict[SPR.key_CALL_NAME];
        //    double frameOverlap = Convert.ToDouble(dict[SPR.key_FRAME_OVERLAP]);

        //    // SPT PARAMETERS
        //    double intensityThreshold = Convert.ToDouble(dict[SPR.key_SPT_INTENSITY_THRESHOLD]);
        //    int smallLengthThreshold = Convert.ToInt32(dict[SPR.key_SPT_SMALL_LENGTH_THRESHOLD]);

        //    // WHIPBIRD PARAMETERS
        //    int whistleMinHz = Int32.Parse(dict[SPR.key_WHISTLE_MIN_HZ]);
        //    int whistleMaxHz = Int32.Parse(dict[SPR.key_WHISTLE_MAX_HZ]);
        //    double optimumWhistleDuration = Double.Parse(dict[SPR.key_WHISTLE_DURATION]);
        //    int whipMinHz = Int32.Parse(dict[SPR.key_WHIP_MIN_HZ]);
        //    int whipMaxHz = Int32.Parse(dict[SPR.key_WHIP_MAX_HZ]);
        //    double whipDuration = Double.Parse(dict[SPR.key_WHIP_DURATION]);

        //    // CURLEW PARAMETERS
        //    double minDuration = Double.Parse(dict[SPR.key_MIN_DURATION]);
        //    double maxDuration = Double.Parse(dict[SPR.key_MAX_DURATION]);

        //    double eventThreshold = Double.Parse(dict[SPR.key_EVENT_THRESHOLD]);

        //    // B: CHECK to see if conversion from .MP3 to .WAV is necessary
        //    var destinationAudioFile = audioFile.FullName;

        //    // LOAD RECORDING AND MAKE SONOGRAM
        //    BaseSonogram sonogram;
        //    using (var recording = new AudioRecording(destinationAudioFile))
        //    {
        //        if (recording.SampleRate != 22050)
        //        {
        //            // down sample if necessary
        //            recording.ConvertSampleRate22kHz();
        //        }

        //        var sonoConfig = new SonogramConfig
        //        {
        //            NoiseReductionType = NoiseReductionType.NONE,
        //            ////NoiseReductionType = NoiseReductionType.STANDARD,
        //            WindowOverlap = frameOverlap
        //        };
        //        sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
        //    }

        //    List<AcousticEvent> predictedEvents = null;

        //    var audioFileName = Path.GetFileNameWithoutExtension(destinationAudioFile);

        //    // execute  - only for whip bird for now.
        //    if (callName.Equals("WHIPBIRD"))
        //    {
        //        // SPT
        //        var result1 = SPT.doSPT(sonogram, intensityThreshold, smallLengthThreshold);

        //        // SPR
        //        Log.WriteLine("SPR start: intensity threshold = " + intensityThreshold);
        //        int slope = 0;
        //        double sensitivity = 0.7;
        //        var mHori = SPR.MarkLine(result1.Item1, slope, smallLengthThreshold, intensityThreshold, sensitivity);
        //        slope = 87;
        //        sensitivity = 0.8;
        //        var mVert = SPR.MarkLine(result1.Item1, slope, smallLengthThreshold - 4, intensityThreshold + 1, sensitivity);
        //        Log.WriteLine("SPR finished");
        //        Log.WriteLine("Extract Whipbird calls - start");

        //        int minBoundWhistle = (int)(whistleMinHz / sonogram.FBinWidth);
        //        int maxBoundWhistle = (int)(whistleMaxHz / sonogram.FBinWidth);
        //        int whistleFrames = (int)(sonogram.FramesPerSecond * optimumWhistleDuration);
        //        int minBoundWhip = (int)(whipMinHz / sonogram.FBinWidth);
        //        int maxBoundWhip = (int)(whipMaxHz / sonogram.FBinWidth);
        //        int whipFrames = (int)(sonogram.FramesPerSecond * whipDuration);
        //        var result3 = SPR.DetectWhipBird(
        //            mHori,
        //            mVert,
        //            minBoundWhistle,
        //            maxBoundWhistle,
        //            whistleFrames,
        //            minBoundWhip,
        //            maxBoundWhip,
        //            whipFrames,
        //            smallLengthThreshold);
        //        double[] scores = result3.Item1;

        //        predictedEvents = AcousticEvent.ConvertScoreArray2Events(scores, whipMinHz, whipMaxHz,
        //                                                      sonogram.FramesPerSecond, sonogram.FBinWidth, eventThreshold, minDuration, maxDuration);
        //        foreach (AcousticEvent ev in predictedEvents)
        //        {
        //            ev.SourceFileName = audioFileName;
        //            ev.Name = callName;
        //        }

        //        sonogram.Data = result1.Item1;
        //        Log.WriteLine("Extract Whipbird calls - finished");
        //    }

        //    SaveAeImage(predictedEvents, settingsFile.DirectoryName, audioFile.FullName);

        //    // AcousticEvent results
        //    var prts =
        //        predictedEvents.Select(
        //            ae =>
        //            ProcessingUtils.GetProcessorResultTag(
        //                ae, new ResultProperty(ae.Name, ae.ScoreNormalised, DefaultNormalisedScore)));

        //    return prts;
        //}

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
        //internal static IEnumerable<ProcessorResultTag> RunHd(FileInfo settingsFile, FileInfo audioFile)
        //{
        //    var config = new ConfigDictionary(settingsFile.FullName);
        //    var dict = config.GetTable();

        //    var callName = dict[HarmonicRecogniser.key_CALL_NAME];
        //    var nrt = SNR.Key2NoiseReductionType(dict[HarmonicRecogniser.key_NOISE_REDUCTION_TYPE]);
        //    var minHz = Int32.Parse(dict[HarmonicRecogniser.key_MIN_HZ]);
        //    var maxHz = Int32.Parse(dict[HarmonicRecogniser.key_MAX_HZ]);
        //    var frameOverlap = Double.Parse(dict[HarmonicRecogniser.key_FRAME_OVERLAP]);
        //    var amplitudeThreshold = Double.Parse(dict[HarmonicRecogniser.key_MIN_AMPLITUDE]);
        //    var harmonicCount = Int32.Parse(dict[HarmonicRecogniser.key_EXPECTED_HARMONIC_COUNT]);
        //    var minDuration = Double.Parse(dict[HarmonicRecogniser.key_MIN_DURATION]);
        //    var maxDuration = Double.Parse(dict[HarmonicRecogniser.key_MAX_DURATION]);

        //    string audioFileName = audioFile.Name;

        //    var results =
        //        HarmonicRecogniser.Execute_HDDetect(
        //            audioFile.FullName,
        //            nrt,
        //            minHz,
        //            maxHz,
        //            frameOverlap,
        //            harmonicCount,
        //            amplitudeThreshold,
        //            minDuration,
        //            maxDuration,
        //            audioFileName,
        //            callName);

        //    var predictedEvents = results.Item4;

        //    SaveAeImage(predictedEvents, settingsFile.DirectoryName, audioFile.FullName);

        //    // AcousticEvent results
        //    var prts =
        //        predictedEvents.Select(
        //            ae =>
        //            ProcessingUtils.GetProcessorResultTag(
        //                ae, new ResultProperty(ae.Name, ae.ScoreNormalised, DefaultNormalisedScore)));

        //    return prts;
        //}

        ///// <summary>
        ///// MFCC OD Regoniser.
        ///// </summary>
        ///// <param name="settingsFile">
        ///// The settings File.
        ///// </param>
        ///// <param name="audioFile">
        ///// Audio file to analyse.
        ///// </param>
        ///// <param name="resourceFile">
        ///// Compressed resource file.
        ///// </param>
        ///// <param name="runDir">
        ///// Working directory.
        ///// </param>
        ///// <returns>
        ///// Processing results.
        ///// </returns>
        //internal static IEnumerable<ProcessorResultTag> RunMfccOd(FileInfo settingsFile, FileInfo audioFile, FileInfo resourceFile, DirectoryInfo runDir)
        //{
        //    var expected = new List<string>
        //        {
        //            MFCC_OD.key_CC_COUNT,
        //            MFCC_OD.key_DCT_DURATION,
        //            MFCC_OD.key_DELTA_T,
        //            MFCC_OD.key_DO_MELSCALE,
        //            MFCC_OD.key_DRAW_SONOGRAMS,
        //            MFCC_OD.key_DYNAMIC_RANGE,
        //            MFCC_OD.key_EVENT_THRESHOLD,
        //            MFCC_OD.key_FRAME_OVERLAP,
        //            MFCC_OD.key_FRAME_SIZE,
        //            MFCC_OD.key_INCLUDE_DELTA,
        //            MFCC_OD.key_INCLUDE_DOUBLE_DELTA,
        //            MFCC_OD.key_MAX_DURATION,
        //            MFCC_OD.key_MAX_HZ,
        //            MFCC_OD.key_MAX_OSCIL_FREQ,
        //            MFCC_OD.key_MIN_AMPLITUDE,
        //            MFCC_OD.key_MIN_DURATION,
        //            MFCC_OD.key_MIN_HZ,
        //            MFCC_OD.key_MIN_OSCIL_FREQ,
        //            MFCC_OD.key_NOISE_REDUCTION_TYPE
        //        };

        //    // upzip resources file into new folder in working dir.
        //    const string ZipFolderName = "UnzipedResources";
        //    var unzipDir = Path.Combine(runDir.FullName, ZipFolderName);
        //    if (!Directory.Exists(unzipDir))
        //    {
        //        Directory.CreateDirectory(unzipDir);
        //    }

        //    ZipUnzip.UnZip(unzipDir, resourceFile.FullName, true);

        //    // only used for lewin's rail for now.

        //    // list of doubles
        //    var doublesFile = Path.Combine(unzipDir, "FV1_KEKKEK1.txt");
        //    double[] fv = FileTools.ReadDoubles2Vector(doublesFile);

        //    // ini file
        //    var iniFile = Path.Combine(unzipDir, "Template_KEKKEK1.txt");

        //    // append to settings file
        //    File.AppendAllText(settingsFile.FullName, File.ReadAllText(iniFile));

        //    // settings
        //    var config = new ConfigDictionary(settingsFile.FullName);
        //    var dict = ProcessingUtils.RemoveEmpty(config.GetTable());

        //    ProcessingUtils.CheckParams(expected, dict.Select(d => d.Key));

        //    int windowSize = Int32.Parse(dict[MFCC_OD.key_FRAME_SIZE]);
        //    double frameOverlap = Double.Parse(dict[MFCC_OD.key_FRAME_OVERLAP]);
        //    NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict[MFCC_OD.key_NOISE_REDUCTION_TYPE]);
        //    double dynamicRange = Double.Parse(dict[MFCC_OD.key_DYNAMIC_RANGE]);
        //    int minHz = Int32.Parse(dict[MFCC_OD.key_MIN_HZ]);
        //    int maxHz = Int32.Parse(dict[MFCC_OD.key_MAX_HZ]);
        //    int ccCount = Int32.Parse(dict[MFCC_OD.key_CC_COUNT]);
        //    bool doMelScale = Boolean.Parse(dict[MFCC_OD.key_DO_MELSCALE]);
        //    bool includeDelta = Boolean.Parse(dict[MFCC_OD.key_INCLUDE_DELTA]);
        //    bool includeDoubleDelta = Boolean.Parse(dict[MFCC_OD.key_INCLUDE_DOUBLE_DELTA]);
        //    int deltaT = Int32.Parse(dict[MFCC_OD.key_DELTA_T]);
        //    double dctDuration = Double.Parse(dict[MFCC_OD.key_DCT_DURATION]);
        //    int minOscilFreq = Int32.Parse(dict[MFCC_OD.key_MIN_OSCIL_FREQ]);
        //    int maxOscilFreq = Int32.Parse(dict[MFCC_OD.key_MAX_OSCIL_FREQ]);
        //    double minAmplitude = Double.Parse(dict[MFCC_OD.key_MIN_AMPLITUDE]);
        //    double eventThreshold = Double.Parse(dict[MFCC_OD.key_EVENT_THRESHOLD]);
        //    double minDuration = Double.Parse(dict[MFCC_OD.key_MIN_DURATION]);
        //    double maxDuration = Double.Parse(dict[MFCC_OD.key_MAX_DURATION]);
        //    int DRAW_SONOGRAMS = Int32.Parse(dict[MFCC_OD.key_DRAW_SONOGRAMS]);

        //    var results = MFCC_OD.Execute_CallDetect(
        //        audioFile.FullName,
        //        minHz,
        //        maxHz,
        //        windowSize,
        //        frameOverlap,
        //        nrt,
        //        dynamicRange,
        //        doMelScale,
        //        ccCount,
        //        includeDelta,
        //        includeDoubleDelta,
        //        deltaT,
        //        fv,
        //        dctDuration,
        //        minOscilFreq,
        //        maxOscilFreq,
        //        minAmplitude,
        //        eventThreshold,
        //        minDuration,
        //        maxDuration);

        //    var events = results.Item4;

        //    SaveAeImage(events, settingsFile.DirectoryName, audioFile.FullName);

        //    // AcousticEvent results
        //    return
        //        events.Select(
        //            ae =>
        //            ProcessingUtils.GetProcessorResultTag(
        //                ae, new ResultProperty(ae.Name, ae.ScoreNormalised, DefaultNormalisedScore)));
        //}

        /// <summary>
        /// HTK template Recogniser.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings File.
        /// </param>
        /// <param name="audioFile">
        /// Audio file to analyse.
        /// </param>
        /// <param name="resourceFile">
        /// A zip file containing the resources required to run HTK.
        /// </param>
        /// <param name="runDir">
        /// The run Dir.
        /// </param>
        /// <returns>
        /// Processing results.
        /// </returns>
        //internal static IEnumerable<ProcessorResultTag> RunHtk(FileInfo settingsFile, FileInfo audioFile, FileInfo resourceFile, DirectoryInfo runDir)
        //{
        //    // upzip resources file into new folder in working dir.
        //    const string ZipFolderName = "UnzipedResources";
        //    var unzipDir = Path.Combine(runDir.FullName, ZipFolderName);
        //    if (!Directory.Exists(unzipDir))
        //    {
        //        Directory.CreateDirectory(unzipDir);
        //    }

        //    ZipUnzip.UnZip(unzipDir, resourceFile.FullName, true);

        //    // ini file
        //    var iniFile = Path.Combine(unzipDir, "segmentation.ini");

        //    // append to settings file
        //    File.AppendAllText(settingsFile.FullName, File.ReadAllText(iniFile));

        //    // htk config
        //    // need to change some of the properties for cluster.
        //    var htkConfig = new HTKConfig(settingsFile.FullName)
        //        {
        //            ConfigDir = unzipDir,
        //            WorkingDir = runDir.FullName,
        //            HTKDir = Path.Combine(unzipDir, "HTK"),
        //            DataDir = Path.Combine(runDir.FullName, "data"),
        //            ResultsDir = Path.Combine(runDir.FullName, "results")
        //        };

        //    // delete and recreate data dir if it exists
        //    if (Directory.Exists(htkConfig.DataDir))
        //    {
        //        Directory.Delete(htkConfig.DataDir, true);
        //    }

        //    Directory.CreateDirectory(htkConfig.DataDir);

        //    string processedAudioFile = Path.GetFileName(audioFile.FullName);
        //    var destinationAudioFile = Path.Combine(htkConfig.DataDir, Path.GetFileNameWithoutExtension(audioFile.FullName) + ".wav");
        //    File.Copy(audioFile.FullName, destinationAudioFile, true);

        //    // D: SCAN RECORDING WITH RECOGNISER AND RETURN A RESULTS FILE
        //    Log.WriteLine("Executing HTK_Recogniser - scanning recording: " + processedAudioFile);
        //    string resultsPath = HTKScanRecording.Execute(processedAudioFile, runDir.FullName, htkConfig);

        //    // E: PARSE THE RESULTS FILE TO RETURN ACOUSTIC EVENTS
        //    Log.WriteLine("Parse the HMM results file and return Acoustic Events");
        //    var events = HTKScanRecording.GetAcousticEventsFromHTKResults(resultsPath, unzipDir);

        //    SaveAeImage(events, settingsFile.DirectoryName, audioFile.FullName);

        //    // AcousticEvent results
        //    var prts =
        //        events.Select(
        //            ae =>
        //            ProcessingUtils.GetProcessorResultTag(
        //                ae, new ResultProperty(ae.Name, ae.ScoreNormalised, DefaultNormalisedScore)));

        //    return prts;
        //}
    }
}