// -----------------------------------------------------------------------
// <copyright file="CrowHdAnalysis.cs" company="QUT">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AnalysisPrograms.Process
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using AnalysisBase;

    using AudioAnalysisTools;

    using TowseyLib;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CrowHdAnalysis : IAnalysis
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        public string DisplayName
        {
            get { return "Crow (HD)"; }
        }

        /// <summary>
        /// Gets Identifier.
        /// </summary>
        public string Identifier
        {
            get
            {
                return "Towsey.CrowHd";
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
                    SegmentMediaType = MediaTypes.MediaTypeWav
                };
            }
        }

        /// <summary>
        /// Run analysis using the given analysis settings.
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
            var configDict = config.GetTable();

            string analysisName = configDict[Crow.key_ANALYSIS_NAME];
            double segmentDuration = Double.Parse(configDict[Crow.key_SEGMENT_DURATION]);
            //NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict[key_NOISE_REDUCTION_TYPE]);
            int minHz = Int32.Parse(configDict[Crow.key_MIN_HZ]);
            //double minDuration = Double.Parse(configDict[key_MIN_DURATION]);          // lower bound for the duration of an event
            //double maxDuration = Double.Parse(configDict[key_MAX_DURATION]);          // upper bound for the duration of an event
            int DRAW_SONOGRAMS = Int32.Parse(configDict[Crow.key_DRAW_SONOGRAMS]);         // options to draw sonogram
            int minFormantgap = Int32.Parse(configDict[Crow.key_MIN_FORMANT_GAP]);
            int maxFormantgap = Int32.Parse(configDict[Crow.key_MAX_FORMANT_GAP]);
            double decibelThreshold = Double.Parse(configDict[Crow.key_DECIBEL_THRESHOLD]); ;   //dB
            double harmonicIntensityThreshold = Double.Parse(configDict[Crow.key_HARMONIC_INTENSITY_THRESHOLD]); //in 0-1
            double callDuration = Double.Parse(configDict[Crow.key_CALL_DURATION]);  // seconds

            AudioRecording recording = new AudioRecording(analysisSettings.AudioFile.FullName);

            //var results = Crow.Analysis(recording, minHz, decibelThreshold, harmonicIntensityThreshold, minFormantgap, maxFormantgap, callDuration);
            var results = Crow.Analysis(0, analysisSettings.AudioFile, configDict, analysisSettings.AnalysisRunDirectory, "audio.wav");

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var eventResults = results.Item4;
            var predictedEvents = results.Item4;
            Console.WriteLine("# Event Count = " + eventResults.Count());

            //write event count to results file.            
            //WriteEventsInfo2TextFile(predictedEvents, opPath);
            double displayThreshold = 0.2; //relative position of threhsold in image of score track.
            double normMax = harmonicIntensityThreshold / displayThreshold; //threshold
            //double normMax = threshold * 4; //previously used for 4 dB threshold - so normalised eventThreshold = 0.25
            for (int i = 0; i < scores.Length; i++) scores[i] /= normMax;
            string imagePath = Path.Combine(analysisSettings.AnalysisRunDirectory.FullName, Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name) + ".png");

            if (DRAW_SONOGRAMS == 2)
            {
                Console.WriteLine("\tMin score={0:f3}  Max score={1:f3}", scores.Min(), scores.Max());
                Image image = Crow.DrawSonogram(sonogram, hits, scores, predictedEvents, displayThreshold);
                image.Save(imagePath, ImageFormat.Png);
            }
            else
                if ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
                {
                    Image image = Crow.DrawSonogram(sonogram, hits, scores, predictedEvents, displayThreshold);
                    image.Save(imagePath, ImageFormat.Png);
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
