using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnalysisBase;
using TowseyLib;
using AudioAnalysisTools;
using System.Data;
using System.IO;

namespace AnalysisPrograms.Process
{
    using Acoustics.Shared;

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

        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = 17640,
                    SegmentMediaType = MediaTypes.MediaTypeWav
                };
            }
        }

        public string DefaultConfiguration
        {
            get { 
                return string.Empty; 
            }
        }


        /// <summary>
        /// WARNING - THIS METHOD HAS BEEN BARSTARDIZED SO I CAN GET THE NEW HUMAn VOICE ANALYSER WORKING.
        /// </summary>
        /// <param name="analysisSettings"></param>
        /// <returns></returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var config = new Configuration(analysisSettings.ConfigFile.FullName);
            var dict = config.GetTable();

            string analysisName = dict[Human.key_ANALYSIS_NAME];
            //NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict[Human.key_NOISE_REDUCTION_TYPE]);
            int minHz = Int32.Parse(dict[Human.key_MIN_HZ]);
            //int maxHz = Int32.Parse(dict[Human.key_MAX_HZ]);
            //int frameLength = Int32.Parse(dict[Human.key_FRAME_LENGTH]);
            //double frameOverlap = Double.Parse(dict[Human.key_FRAME_OVERLAP]);
            //double minAmplitude = Double.Parse(dict[Human.key_MIN_AMPLITUDE]);        // minimum acceptable value of harmonic ocsillation in dB
            //int harmonicCount = Int32.Parse(dict[Human.key_EXPECTED_HARMONIC_COUNT]); // expected number of harmonics to find in spectrum
            double minDuration = Double.Parse(dict[Human.key_MIN_DURATION]);          // lower bound for the duration of an event
            double maxDuration = Double.Parse(dict[Human.key_MAX_DURATION]);          // upper bound for the duration of an event
            double intensityThreshold = Double.Parse(dict[Human.key_INTENSITY_THRESHOLD]);
            //int DRAW_SONOGRAMS = Int32.Parse(dict[Human.key_DRAW_SONOGRAMS]);         // options to draw sonogram

            int minFormantgap = 90;
            int maxFormantgap = 250;
            string audioFileName = analysisSettings.AudioFile.FullName;
            AudioRecording recording = new AudioRecording(audioFileName);

            var results = Human.Execute_HDDetect(
                recording, minHz, intensityThreshold, minFormantgap, maxFormantgap, minDuration, maxDuration);
            FileInfo fiSegmentOfSourceFile = new FileInfo(audioFileName);
            //int iter = 2;
            //DirectoryInfo diOutputDir = new DirectoryInfo("######");
            //string opFileName  = "##########";
            //var results = Human.Analysis(iter, fiSegmentOfSourceFile, configDict, diOutputDir, opFileName);


            var eventResults = results.Item4;

            var result = new AnalysisResult
            {
                AnalysisIdentifier = this.Identifier,
                SettingsUsed = analysisSettings,
                Data = AnalysisHelpers.BuildDefaultDataTable(eventResults),
            };

            return result;
        }
    }
}
