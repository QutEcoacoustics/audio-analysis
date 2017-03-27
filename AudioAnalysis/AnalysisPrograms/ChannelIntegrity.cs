// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelIntegrity.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
    using System.Linq;
    using System.Text;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Production;
    using AudioAnalysisTools;
    using AudioAnalysisTools.WavTools;

    public class ChannelIntegrityAnalyzer : AbstractStrongAnalyser
    {
        public override string DisplayName => "ChannelIntegrity";

        public override string Identifier => "Towsey.ChannelIntegrity";

        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            // boilerplate Analyzer
            var audioFile = analysisSettings.AudioFile;
            var sampleRate = analysisSettings.SampleRateOfOriginalAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResults = new AnalysisResult2(analysisSettings, recording.Duration());
            analysisResults.AnalysisIdentifier = this.Identifier;

            var result = new ChannelIntegrityIndices()
                {
                    StartOffset = analysisSettings.SegmentStartOffset.Value,
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

            double similarityIndex;
            double decibelIndex;
            double avDecibelBias;
            double medianDecibelBias;
            double lowDecibelBias;
            double midDecibelBias;
            double highDecibelBias;

            ChannelIntegrity.SimilarityIndex(channelLeft, channelRight, epsilon, sampleRate.Value, out similarityIndex,
                                              out decibelIndex, out avDecibelBias, out medianDecibelBias,
                                              out lowDecibelBias, out midDecibelBias, out highDecibelBias);

            //double similarityIndex = ChannelIntegrity.SimilarityIndex(channelLeft, channelRight, epsilon, sampleRate.Value);
            result.ChannelSimilarity   = similarityIndex;
            result.ChannelDiffDecibels = decibelIndex;
            result.AverageDecibelBias  = avDecibelBias;
            result.MedianDecibelBias   = medianDecibelBias;
            result.LowFreqDecibelBias  = lowDecibelBias;
            result.MidFreqDecibelBias  = midDecibelBias;
            result.HighFreqDecibelBias = highDecibelBias;


            double zeroCrossingFractionLeft;
            double zeroCrossingFractionRight;
            ChannelIntegrity.ZeroCrossingIndex(channelLeft, channelRight, out zeroCrossingFractionLeft, out zeroCrossingFractionRight);
            result.ZeroCrossingFractionLeft = zeroCrossingFractionLeft;
            result.ZeroCrossingFractionRight = zeroCrossingFractionRight;

            // finish the analyzer
            analysisResults.Events = new EventBase[0];
            analysisResults.SummaryIndices = new SummaryIndexBase[] { result };
            analysisResults.SpectralIndices = new SpectralIndexBase[0];

            if (analysisSettings.SummaryIndicesFile != null)
            {
                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
                analysisResults.SummaryIndicesFile = analysisSettings.SummaryIndicesFile;
            }

            if (analysisSettings.SegmentSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                throw new NotImplementedException();
            }

            if (analysisSettings.SpectrumIndicesDirectory != null)
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
