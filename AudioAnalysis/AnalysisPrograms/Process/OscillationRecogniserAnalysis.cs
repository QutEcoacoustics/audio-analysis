namespace AnalysisPrograms.Process
{
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using AnalysisBase;

    using AnalysisPrograms.Processing;

    using AudioAnalysisTools;

    using TowseyLib;

    /// <summary>
    /// Oscillation Recogniser analysis.
    /// </summary>
    public class OscillationRecogniserAnalysis : IAnalysis
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return "Oscillation Recogniser";
            }
        }

        /// <summary>
        /// Gets Identifier.
        /// </summary>
        public string Identifier
        {
            get
            {
                return "od";
            }
        }

        /// <summary>
        /// Gets the initial (default) settings for the analysis.
        /// </summary>
        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = 22050,
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    ConfigStringInput = string.Empty
                };
            }
        }

        /// <summary>
        /// Gets the Default Configuration.
        /// </summary>
        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Run analysis over the given audio file, using the 
        /// settings from configuration file. Use the working directory.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var config = new Configuration(analysisSettings.ConfigFile.FullName);
            var dict = ProcessingUtils.RemoveEmpty(config.GetTable());

            bool doSegmentation = Boolean.Parse(dict[OscillationRecogniser.key_DO_SEGMENTATION]);
            int minHz = Int32.Parse(dict[OscillationRecogniser.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[OscillationRecogniser.key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[OscillationRecogniser.key_FRAME_OVERLAP]);
            double dctDuration = Double.Parse(dict[OscillationRecogniser.key_DCT_DURATION]);       //duration of DCT in seconds 
            double dctThreshold = Double.Parse(dict[OscillationRecogniser.key_DCT_THRESHOLD]);      //minimum acceptable value of a DCT coefficient
            int minOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MIN_OSCIL_FREQ]);      //ignore oscillations below this threshold freq
            int maxOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MAX_OSCIL_FREQ]);      //ignore oscillations above this threshold freq
            double minDuration = Double.Parse(dict[OscillationRecogniser.key_MIN_DURATION]);       //min duration of event in seconds 
            double maxDuration = Double.Parse(dict[OscillationRecogniser.key_MAX_DURATION]);       //max duration of event in seconds 
            double eventThreshold = Double.Parse(dict[OscillationRecogniser.key_EVENT_THRESHOLD]);  //min score for an acceptable event
            int DRAW_SONOGRAMS = Int32.Parse(dict[OscillationRecogniser.key_DRAW_SONOGRAMS]);      //options to draw sonogram

            // execute
            var results =
                OscillationRecogniser.Execute_ODDetect(
                    analysisSettings.AudioFile.FullName,
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

            var eventResults = results.Item4;

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var intensity = results.Item5;
            var analysisDuration = results.Item6;
            Log.WriteLine("# Event Count = " + predictedEvents.Count());
            int pcHIF = 100;
            if (intensity != null)
            {
                int hifCount = intensity.Count(p => p >= 0.001); //count of high intensity frames
                pcHIF = 100 * hifCount / sonogram.FrameCount;
            }

            //write event count to results file. 
            double sigDuration = sonogram.Duration.TotalSeconds;
            string fname = Path.GetFileName(analysisSettings.AudioFile.FullName);
            int count = predictedEvents.Count;
            //string str = String.Format("#RecordingName\tDuration(sec)\t#Ev\tCompT(ms)\t%hiFrames\n{0}\t{1}\t{2}\t{3}\t{4}\n", fname, sigDuration, count, analysisDuration.TotalMilliseconds, pcHIF);
            string str = String.Format("{0}\t{1}\t{2}\t{3}\t{4}", fname, sigDuration, count, analysisDuration.TotalMilliseconds, pcHIF);
            StringBuilder sb = AcousticEvent.WriteEvents(predictedEvents, str);
            FileTools.WriteTextFile(Path.Combine(analysisSettings.AnalysisRunDirectory.FullName, "results.txt"), sb.ToString());


            //draw images of sonograms
            string imagePath = Path.Combine(analysisSettings.AnalysisRunDirectory.FullName, Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name) + ".png");
            if (DRAW_SONOGRAMS == 2)
            {
                OscillationRecogniser.DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, eventThreshold, intensity);
            }
            else
                if ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
                {
                    OscillationRecogniser.DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, eventThreshold, intensity);
                }


            var result = new AnalysisResult
            {
                AnalysisIdentifier = this.Identifier,
                SettingsUsed = analysisSettings,
                Data = AnalysisHelpers.BuildDefaultDataTable(eventResults)
            };

            return result;
        }
    }
}
