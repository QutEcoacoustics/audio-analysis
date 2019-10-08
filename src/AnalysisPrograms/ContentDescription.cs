// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Acoustics.Shared;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net;

    public class ContentDescription : AbstractStrongAnalyser
    {
        public const string AnalysisName = "ContentDescription";

        // TASK IDENTIFIERS
        //public const string TaskAnalyse = AnalysisName;
        //public const string TaskLoadCsv = "loadCsv";
        public const string TowseyAcoustic = "Towsey." + AnalysisName;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const int ResampleRate = 17640;

        public override string DisplayName => "Content Description";

        public override string Identifier => TowseyAcoustic;

        public override string Description => "[BETA] Generates six spectral indices for Content Description.";

        public override AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
            AnalysisMinSegmentDuration = TimeSpan.FromSeconds(30),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
            AnalysisTargetSampleRate = ResampleRate,
        };

        //public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        //{
        //    var configuration = (AcousticIndices.AcousticIndicesConfig)analysisSettings.Configuration;

        //    configuration.Validate(analysisSettings.AnalysisMaxSegmentDuration.Value);

        //    analysisSettings.AnalysisAnalyzerSpecificConfiguration = configuration;
        //}

        //public static DescriptionResult Analysis(FileInfo segmentOfSourceFile, IDictionary<string, string> configDict, TimeSpan segmentStartOffset)
        //{
        //    var results = new DescriptionResult((int)Math.Floor(segmentStartOffset.TotalMinutes));
        //    return results;
        //}

        /// <summary>
        /// THis method calls others to do the work!.
        /// TODO: SIMPLIFY THIS METHOD.
        /// </summary>
        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            //var acousticIndicesConfiguration = (AcousticIndices.AcousticIndicesConfig)analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            //var indexCalculationDuration = acousticIndicesConfiguration.IndexCalculationDuration.Seconds();

            var config = new IndexCalculateConfig();
            var indexCalculationDuration = TimeSpan.FromSeconds(60);

            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            //var outputDirectory = segmentSettings.SegmentOutputDirectory;

            // calculate indices for each one minute recording segment
            IndexCalculateResult segmentResults = CalculateIndicesInOneMinuteSegmentOfRecording(
                recording,
                segmentSettings.SegmentStartOffset,
                //segmentSettings.AnalysisIdealSegmentDuration,
                indexCalculationDuration,
                //acousticIndicesConfiguration.IndexProperties,
                segmentSettings.Segment.SourceMetadata.SampleRate,
                config);

            //var trackScores = new List<Plot>(segmentResults.Length);
            //var tracks = new List<SpectralTrack>(segmentResults.Length);

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration)
            {
                AnalysisIdentifier = this.Identifier,
                SpectralIndices = new SpectralIndexBase[1],
            };
            segmentResults.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;
            analysisResults.SpectralIndices[0] = segmentResults.SpectralIndexValues;

            /*
            for (int i = 0; i < subsegmentResults.Length; i++)
            {
                var indexCalculateResult = segmentResults[i];
                indexCalculateResult.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;

                //analysisResults.SummaryIndices[i] = indexCalculateResult.SummaryIndexValues;
                analysisResults.SpectralIndices[i] = indexCalculateResult.SpectralIndexValues;
                //trackScores.AddRange(indexCalculateResult.TrackScores);
                //if (indexCalculateResult.Tracks != null)
                //{
                //    tracks.AddRange(indexCalculateResult.Tracks);
                //}
            }
            */

            return analysisResults;
        }

        /// <summary>
        /// TODO: REWRITE THIS METHOD.
        /// </summary>
        public static IndexCalculateResult CalculateIndicesInOneMinuteSegmentOfRecording(
            AudioRecording recording,
            TimeSpan segmentStartOffset,
            TimeSpan indexCalculationDuration,
            //Dictionary<string, IndexProperties> indexProperties,
            int sampleRateOfOriginalAudioFile,
            IndexCalculateConfig config)
        {
            //TODO: SET UP NEW class CalculateSizIndices.cs.
            var resultsForSixIndices = IndexCalculate.Analysis(
                recording,
                segmentStartOffset,
                null, //indexProperties,
                sampleRateOfOriginalAudioFile,
                segmentStartOffset,
                config);

            return resultsForSixIndices;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            var list = new List<FileInfo>();
            return list;
        }

        public override void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment,
            EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results)
        {
        }
    }
}
