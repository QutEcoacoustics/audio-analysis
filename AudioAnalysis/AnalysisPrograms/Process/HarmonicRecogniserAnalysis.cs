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
        public PreparerSettings DefaultFileSettings
        {
            get
            {
                return new PreparerSettings
                    {
                        SegmentMaxDuration = TimeSpan.FromMinutes(1),
                        SegmentOverlapDuration = TimeSpan.Zero,
                        SegmentTargetSampleRate = 22050
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

            var table = new DataTable("OscillationRecogniserAnalysisResults");

            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ScoreNormalised", typeof(double));
            table.Columns.Add("EventStartSeconds", typeof(double));
            table.Columns.Add("EventEndSeconds", typeof(double));
            table.Columns.Add("EventFrequencyMaxSeconds", typeof(double));
            table.Columns.Add("EventFrequencyMinSeconds", typeof(double));
            table.Columns.Add("Information", typeof(string));

            foreach (var eventResult in eventResults)
            {
                var newRow = table.NewRow();
                newRow["Name"] = eventResult.Name;
                newRow["ScoreNormalised"] = eventResult.ScoreNormalised;
                newRow["EventStartSeconds"] = eventResult.TimeStart;
                newRow["EventEndSeconds"] = eventResult.TimeEnd;
                newRow["EventFrequencyMaxSeconds"] = eventResult.MinFreq;
                newRow["EventFrequencyMinSeconds"] = eventResult.MaxFreq;

                if (eventResult.ResultPropertyList != null && eventResult.ResultPropertyList.Any())
                {
                    newRow["Information"] =
                        eventResult.ResultPropertyList.Where(i => i != null).Select(i => i.ToString());
                }
                else
                {
                    newRow["Information"] = "No Information";
                }
            }

            var result = new AnalysisResult
                {
                    AnalysisIdentifier = this.Identifier,
                    AnalysisSettingsUsed = analysisSettings,
                    Results = table
                };

            return result;
        }
    }
}
