// -----------------------------------------------------------------------
// <copyright file="HarmonicRecogniserAnalysis.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AnalysisPrograms.Process
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using AnalysisBase;

    using AnalysisPrograms.Processing;

    using TowseyLib;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HarmonicRecogniserAnalysis : IAnalysis
    {
        /// <summary>
        /// Gets the initial settings for the analysis.
        /// </summary>
        public AnalysisSettings InitialSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    AnalysisName = "Harmonic Recogniser",
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = 22050
                };
            }
        }

        /// <summary>
        /// Prepare the analysis processing. This could involve creating files or directories in the working directory,
        /// setting additional settings, or any other preparation.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis settings.
        /// </param>
        /// <returns>
        /// The analysis settings to use to run the analysis.
        /// </returns>
        public AnalysisSettings PrepareAnalysis(AnalysisSettings analysisSettings)
        {
            // TODO: is there any setup required?
            return analysisSettings;
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
                newRow["EventStartSeconds"] = eventResult.StartTime;
                newRow["EventEndSeconds"] = eventResult.EndTime;
                newRow["EventFrequencyMaxSeconds"] = eventResult.MinFreq;
                newRow["EventFrequencyMinSeconds"] = eventResult.MaxFreq;

                if (eventResult.ResultPropertyList != null && eventResult.ResultPropertyList.Any())
                {
                    newRow["Information"] =
                        eventResult.ResultPropertyList.Where(i => i != null).Select(i => i.ToString());
                }

                newRow["Information"] = "No Information";
            }

            return new AnalysisResult(table);
        }
    }
}
