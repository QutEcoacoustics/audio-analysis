namespace AnalysisPrograms.Process
{
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;

    using AnalysisBase;

    using AnalysisPrograms.Processing;

    using TowseyLib;

    /// <summary>
    /// Oscillation Recogniser analysis.
    /// </summary>
    public class OscillationRecogniserAnalysis : IAnalysis
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
                        AnalysisName = "Oscillation Recogniser",
                        SegmentMaxDuration = TimeSpan.FromMinutes(1),
                        SegmentOverlapDuration = TimeSpan.Zero,
                        SegmentTargetSampleRate = 22050
                    };
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

            bool doSegmentation = bool.Parse(dict[OscillationRecogniser.key_DO_SEGMENTATION]);
            var minHz = int.Parse(dict[OscillationRecogniser.key_MIN_HZ]);
            var maxHz = int.Parse(dict[OscillationRecogniser.key_MAX_HZ]);
            var frameOverlap = double.Parse(dict[OscillationRecogniser.key_FRAME_OVERLAP]);
            var dctDuration = double.Parse(dict[OscillationRecogniser.key_DCT_DURATION]);
            var dctThreshold = double.Parse(dict[OscillationRecogniser.key_DCT_THRESHOLD]);
            var minOscilFreq = int.Parse(dict[OscillationRecogniser.key_MIN_OSCIL_FREQ]);
            var maxOscilFreq = int.Parse(dict[OscillationRecogniser.key_MAX_OSCIL_FREQ]);
            var eventThreshold = double.Parse(dict[OscillationRecogniser.key_EVENT_THRESHOLD]);
            var minDuration = double.Parse(dict[OscillationRecogniser.key_MIN_DURATION]);
            var maxDuration = double.Parse(dict[OscillationRecogniser.key_MAX_DURATION]);

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

                newRow["Information"] = "No Information";
            }

            return new AnalysisResult(table);
        }
    }
}
