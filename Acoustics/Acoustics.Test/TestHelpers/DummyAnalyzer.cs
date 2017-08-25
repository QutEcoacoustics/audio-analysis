// <copyright file="DummyAnalyzer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::AnalysisBase;
    using global::AnalysisBase.ResultBases;

    public class DummyAnalyzer : AbstractStrongAnalyser
    {
        private readonly bool block;

        private Task waitingFor;
        private CancellationTokenSource cancellation;

        public DummyAnalyzer(bool block)
        {
            this.block = block;
        }

        public override string DisplayName { get; } = "Testing Placeholder Analysis (temporary substitute)";

        public override string Identifier { get; } = "Ecosounds.TempusSubstitutus";

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            this.Pause("Analyze" + segmentSettings.SegmentStartOffset.TotalMinutes);

            return new AnalysisResult2(analysisSettings, segmentSettings, TimeSpan.FromSeconds(60.0));
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

        public void Pump(bool wait = true)
        {
            if (!this.block)
            {
                return;
            }

            if (this.waitingFor != null)
            {
                this.cancellation.Cancel(false);

                // need to wait for the next wait to be ready
                if (wait)
                {
                    do
                    {
                        Task.Delay(100).Wait();
                    } while (this.waitingFor == null);
                }
            }

            return;
        }

        public void Pause(string description)
        {
            Debug.WriteLine($"{this.Identifier} analysis has paused at stage `{description}`");
            if (!this.block)
            {
                return;
            }

            // pause indefinitely
            this.cancellation = new CancellationTokenSource();
            this.waitingFor = Task.Delay(-1, this.cancellation.Token);
            try
            {
                this.waitingFor.Wait();
            }
            catch (AggregateException aex)
            {
                if (!(aex.GetBaseException() is TaskCanceledException))
                {
                    throw;
                }

                Debug.WriteLine($"{this.Identifier} analysis has canceled at stage `{description}`");
            }

            this.waitingFor = null;
        }

        public bool IsPaused
        {
            get { return !this.waitingFor?.IsCompleted ?? false; }
        }
    }
}
