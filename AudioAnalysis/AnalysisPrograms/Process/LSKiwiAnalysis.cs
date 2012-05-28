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
    using Acoustics.Shared;

    public class LSKiwiAnalysis : IAnalysis
    {
        public string DisplayName
        {
            get { return "Little Spotted Kiwi"; }
        }

        public string Identifier
        {
            get { return "Towsey.LSKiwi1"; }
        }

        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMaxDuration = TimeSpan.FromMinutes(5),
                    SegmentOverlapDuration = TimeSpan.FromSeconds(10),
                    SegmentTargetSampleRate = 17640,
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    ConfigStringInput = string.Empty
                };
            }
        }

        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var config1 = new Configuration(analysisSettings.ConfigFile.FullName);
            var config = config1.GetTable();

            int minHzMale = Configuration.GetInt(LSKiwi.key_MIN_HZ_MALE, config);
            int maxHzMale = Configuration.GetInt(LSKiwi.key_MAX_HZ_MALE, config);
            int minHzFemale = Configuration.GetInt(LSKiwi.key_MIN_HZ_FEMALE, config);
            int maxHzFemale = Configuration.GetInt(LSKiwi.key_MAX_HZ_FEMALE, config);
            int frameLength = Configuration.GetInt(LSKiwi.key_FRAME_LENGTH, config);
            double frameOverlap = Configuration.GetDouble(LSKiwi.key_FRAME_OVERLAP, config);
            double dctDuration = Configuration.GetDouble(LSKiwi.key_DCT_DURATION, config);
            double dctThreshold = Configuration.GetDouble(LSKiwi.key_DCT_THRESHOLD, config);
            double minPeriod = Configuration.GetDouble(LSKiwi.key_MIN_PERIODICITY, config);
            double maxPeriod = Configuration.GetDouble(LSKiwi.key_MAX_PERIODICITY, config);
            double eventThreshold = Configuration.GetDouble(LSKiwi.key_EVENT_THRESHOLD, config);
            double minDuration = Configuration.GetDouble(LSKiwi.key_MIN_DURATION, config); //minimum event duration to qualify as species call
            double maxDuration = Configuration.GetDouble(LSKiwi.key_MAX_DURATION, config); //maximum event duration to qualify as species call
            double drawSonograms = Configuration.GetInt(LSKiwi.key_DRAW_SONOGRAMS, config);
            double segmentDuration = Configuration.GetDouble(LSKiwi.key_SEGMENT_DURATION, config);

            AudioRecording recordingSegment = new AudioRecording(analysisSettings.AudioFile.FullName);

            var results = LSKiwi.Execute_KiwiDetect(recordingSegment, minHzMale, maxHzMale, minHzFemale, maxHzFemale, frameLength, frameOverlap, dctDuration, dctThreshold,
                                                            minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);

            var eventResults = results.Item5;

            

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
