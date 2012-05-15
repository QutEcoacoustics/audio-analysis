using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnalysisBase;
using TowseyLib;
using AudioAnalysisTools;
using System.Data;

namespace AnalysisPrograms.Process
{
    public class HumanSpeechAnalysis : IAnalysis
    {

        public string DisplayName
        {
            get { return "Human Speech"; }
        }

        public string Identifier
        {
            get { return "Towsey.human"; }
        }

        public PreparerSettings DefaultFileSettings
        {
            get
            {
                return new PreparerSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = 17640
                };
            }
        }

        public string DefaultConfiguration
        {
            get { 
                return string.Empty; 
            }
        }

        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var config = new Configuration(analysisSettings.ConfigFile.FullName);
            var dict = config.GetTable();

            string analysisName = dict[Human.key_ANALYSIS_NAME];
            NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict[Human.key_NOISE_REDUCTION_TYPE]);
            int minHz = Int32.Parse(dict[Human.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[Human.key_MAX_HZ]);
            int frameLength = Int32.Parse(dict[Human.key_FRAME_LENGTH]);
            double frameOverlap = Double.Parse(dict[Human.key_FRAME_OVERLAP]);
            double minAmplitude = Double.Parse(dict[Human.key_MIN_AMPLITUDE]);        // minimum acceptable value of harmonic ocsillation in dB
            int harmonicCount = Int32.Parse(dict[Human.key_EXPECTED_HARMONIC_COUNT]); // expected number of harmonics to find in spectrum
            double minDuration = Double.Parse(dict[Human.key_MIN_DURATION]);          // lower bound for the duration of an event
            double maxDuration = Double.Parse(dict[Human.key_MAX_DURATION]);          // upper bound for the duration of an event
            double intensityThreshold = Double.Parse(dict[Human.key_INTENSITY_THRESHOLD]);
            int DRAW_SONOGRAMS = Int32.Parse(dict[Human.key_DRAW_SONOGRAMS]);         // options to draw sonogram

            int minFormantgap = 90;
                int maxFormantgap = 250;

            string audioFileName = analysisSettings.AudioFile.FullName;
            AudioRecording recording = new AudioRecording(audioFileName);

            var results = Human.Execute_HDDetect(
                recording, minHz, intensityThreshold, minFormantgap, maxFormantgap, minDuration, maxDuration);

            var eventResults = results.Item4;

            var table = new DataTable(this.Identifier);

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
