// -----------------------------------------------------------------------
// <copyright file="HarmonicRecogniserAnalysis.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AnalysisPrograms.Process
{
    using System;
    using System.Data;
    using System.Linq;

    using Acoustics.Shared;

    using AnalysisBase;

    using TowseyLib;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HarmonicRecogniserAnalysis : IAnalysis
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return "Harmonic Recogniser";
            }
        }

        /// <summary>
        /// Gets Identifier.
        /// </summary>
        public string Identifier
        {
            get
            {
                return "hd";
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
            var dict = config.GetTable();

            var callName = dict[HarmonicRecogniser.key_CALL_NAME];
            var nrt = SNR.Key2NoiseReductionType(dict[HarmonicRecogniser.key_NOISE_REDUCTION_TYPE]);
            var minHz = int.Parse(dict[HarmonicRecogniser.key_MIN_HZ]);
            var maxHz = int.Parse(dict[HarmonicRecogniser.key_MAX_HZ]);
            var frameOverlap = double.Parse(dict[HarmonicRecogniser.key_FRAME_OVERLAP]);
            var amplitudeThreshold = double.Parse(dict[HarmonicRecogniser.key_MIN_AMPLITUDE]);
            var harmonicCount = int.Parse(dict[HarmonicRecogniser.key_EXPECTED_HARMONIC_COUNT]);
            var minDuration = double.Parse(dict[HarmonicRecogniser.key_MIN_DURATION]);
            var maxDuration = double.Parse(dict[HarmonicRecogniser.key_MAX_DURATION]);

            string audioFileName = analysisSettings.AudioFile.Name;

            var results =
                HarmonicRecogniser.Execute_HDDetect(
                    audioFileName,
                    nrt,
                    minHz,
                    maxHz,
                    frameOverlap,
                    harmonicCount,
                    amplitudeThreshold,
                    minDuration,
                    maxDuration,
                    audioFileName,
                    callName);

            var eventResults = results.Item4;

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
