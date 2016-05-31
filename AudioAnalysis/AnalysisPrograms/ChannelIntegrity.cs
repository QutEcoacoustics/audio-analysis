// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelIntegrity.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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

            var result = new ChannelIntegrityIndexes();

            // do some sanity checks
            


            // actual analysis
            double[] channelLeft = { }; // TODO - broken
            double[] channelRight = { }; // TODO - broken
            double epsilon = Double.NaN; // TODO - broken
            double differenceIndex = ChannelIntegrity.DifferenceIndex(channelLeft, channelRight, epsilon, sampleRate.Value);
            result.ChannelDifference = differenceIndex;

            double zeroCrossingFractionLeft;
            double crossingFractionRight;
            ChannelIntegrity.ZeroCrossingIndex(channelLeft, channelRight, out zeroCrossingFractionLeft, out crossingFractionRight);
            result.ZeroCrossingFractionLeft = zeroCrossingFractionLeft;
            result.ZeroCrossingFractionRight = crossingFractionRight;

            // finish the analyzer
            analysisResults.Events = new EventBase[0];
            analysisResults.SummaryIndices = new SummaryIndexBase[] { result };
            analysisResults.SpectralIndices = new SpectralIndexBase[0];

            if (analysisSettings.SummaryIndicesFile != null)
            {
                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
                analysisResults.SummaryIndicesFile = analysisSettings.SummaryIndicesFile;
            }

            if (analysisSettings.ImageFile != null)
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

        public override void WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
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
            throw new NotImplementedException();
        }
    }
}
