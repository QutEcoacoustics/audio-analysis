// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelIntegrity.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ChannelIntegrity type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Production;
    using AudioAnalysisTools;
    using AudioAnalysisTools.WavTools;

    public class ChannelIntegrityAnalyzer : AbstractStrongAnalyser
    {
        public override string Description => "[ALPHA] Experimental code produced for Y.Phillip's thesis";

        public override string DisplayName => "ChannelIntegrity";

        public override string Identifier => "Towsey.ChannelIntegrity";

        public override Status Status => Status.Alpha;

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            // boilerplate Analyzer
            var audioFile = segmentSettings.SegmentAudioFile;
            var sampleRate = segmentSettings.Segment.SourceMetadata.SampleRate;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);
            analysisResults.AnalysisIdentifier = this.Identifier;

            var result = new ChannelIntegrityIndices()
            {
                ResultStartSeconds = segmentSettings.SegmentStartOffset.TotalSeconds,
            };

            // do some sanity checks
            if (recording.WavReader.Channels != 2)
            {
                throw new InvalidAudioChannelException($"The channel integrity analyzer requires exactly two channels but {recording.WavReader.Channels} channels found in file ({audioFile.FullName}");
            }

            // actual analysis
            double[] channelLeft = recording.WavReader.GetChannel(0);
            double[] channelRight = recording.WavReader.GetChannel(1);
            double epsilon = recording.WavReader.Epsilon;

            ChannelIntegrity.SimilarityIndex(
                channelLeft,
                channelRight,
                epsilon,
                sampleRate,
                out var similarityIndex,
                out var decibelIndex,
                out var avDecibelBias,
                out var medianDecibelBias,
                out var lowDecibelBias,
                out var midDecibelBias,
                out var highDecibelBias);

            //double similarityIndex = ChannelIntegrity.SimilarityIndex(channelLeft, channelRight, epsilon, sampleRate.Value);
            result.ChannelSimilarity = similarityIndex;
            result.ChannelDiffDecibels = decibelIndex;
            result.AverageDecibelBias = avDecibelBias;
            result.MedianDecibelBias = medianDecibelBias;
            result.LowFreqDecibelBias = lowDecibelBias;
            result.MidFreqDecibelBias = midDecibelBias;
            result.HighFreqDecibelBias = highDecibelBias;

            ChannelIntegrity.ZeroCrossingIndex(channelLeft, channelRight, out var zeroCrossingFractionLeft, out var zeroCrossingFractionRight);
            result.ZeroCrossingFractionLeft = zeroCrossingFractionLeft;
            result.ZeroCrossingFractionRight = zeroCrossingFractionRight;

            // finish the analyzer
            analysisResults.Events = new EventBase[0];
            analysisResults.SummaryIndices = new SummaryIndexBase[] { result };
            analysisResults.SpectralIndices = new SpectralIndexBase[0];

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
                analysisResults.SummaryIndicesFile = segmentSettings.SegmentSummaryIndicesFile;
            }

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                throw new NotImplementedException();
            }

            if (false && analysisSettings.AnalysisDataSaveBehavior)
            {
                throw new NotImplementedException();
            }

            return analysisResults;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
        }
    }
}