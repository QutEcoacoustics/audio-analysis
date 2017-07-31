// <copyright file="DummyAnalyzer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::AnalysisBase;
    using global::AnalysisBase.ResultBases;

    public class DummyAnalyzer : AbstractStrongAnalyser
    {
        private readonly bool block;

        public DummyAnalyzer(bool block)
        {
            this.block = block;
            this.cancellation = new CancellationTokenSource();
        }

        private Task waitingFor;
        private CancellationTokenSource cancellation;

        public override string DisplayName { get; } = "Testing Placeholder Analysis (temporary substitute)";

        public override string Identifier { get; } = "Ecosounds.TempusSubstitutus";

        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            this.Pause("Analyze");

            return new AnalysisResult2(analysisSettings, TimeSpan.FromSeconds(60.0));
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
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
            this.Pause("Summarize");
        }

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            base.BeforeAnalyze(analysisSettings);

            this.Pause("BeforeAnalyze");
        }

        public void Pump()
        {
            if (!this.block)
            {
                return;
            }

            this.cancellation.Cancel(false);
        }

        public void Pause(string description)
        {
            if (!this.block)
            {
                return;
            }

            // pause indefinitely
            this.waitingFor = Task.Delay(-1, this.cancellation.Token);
            this.waitingFor.Wait();
        }
    }
}
