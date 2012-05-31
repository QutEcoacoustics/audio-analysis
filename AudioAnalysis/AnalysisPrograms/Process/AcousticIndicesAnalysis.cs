namespace AnalysisPrograms
{
    using System;

    using Acoustics.Shared;

    using AnalysisBase;
    using System.Configuration;
    using AudioAnalysisTools;
    using System.Data;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Acoustic Indices Audio Analysis.
    /// </summary>
    public class AcousticIndicesAudioAnalysis : IAnalysis
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return "Acoustic Indices";
            }
        }

        /// <summary>
        /// Gets Identifier.
        /// </summary>
        public string Identifier
        {
            get
            {
                return "Towsey.AcousticIndices";
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
            Dictionary<string, string> dict = new Dictionary<string, string>();  //set up the default parameters
            dict.Add(AcousticIndices.key_FRAME_LENGTH, AcousticIndices.DEFAULT_WINDOW_SIZE.ToString());
            //dict.Add(AcousticIndices.key_LOW_FREQ_BOUND, "500");
            dict.Add(AcousticIndices.key_LOW_FREQ_BOUND, "3500");
            //dict.Add(key_RESAMPLE_RATE, resampleRate.ToString());
            int iterationNumber = 1;
            var fiRecording = analysisSettings.AudioFile;
            dict.Add(AcousticIndices.key_STORE_INTERMEDATE_RESULTS, "false");
            var results = AcousticIndices.Analysis(iterationNumber, fiRecording, dict);

            var analysisResults = new AnalysisResult
            {
                Data = results,
                AnalysisIdentifier = this.Identifier,
                SettingsUsed = analysisSettings,
            };

            return analysisResults;
        }
    }
}
