// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyAnalysis.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   The purpose of this analyser is to make inter-program parallelisation easier to develop
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;

    /// <summary>
    /// The purpose of this analyser is to make inter-program parallelisation easier to develop.
    /// </summary>
    public class DummyAnalysis
    {
        public const string CommandName = "FakeAnalysis";

        [Command(
            CommandName,
            Description = "A program designed to simulate load - does nothing other than burn CPU;")]
        public class Arguments : SubCommandBase
        {
            [Option(Description = "Burn load on multiple CPU threads?")]
            public bool Parallel { get; set; }

            [Option(ShortName = "d", Description = "How many seconds to run for (roughly)")]
            [InRange(0, 3600)]
            public double DurationSeconds { get; set; } = 30;

            [Option(Description = "How much jitter should be applied to execution time of each thread. A random amount is chosen where 0 <= `Jitter` <= 1.")]
            [InRange(0.0, 1.0)]
            public double Jitter { get; set; } = 0.1;

            [Option(Description = "Supply a seed to repeat the same randomnesss as a previous run")]
            public int? Seed { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                return DummyAnalysis.ExecuteAync(this);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan LogEvery = TimeSpan.FromSeconds(10);

        public static async Task<int> ExecuteAync(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = new Arguments()
                {
                    DurationSeconds = 20.0,
                    Jitter = 0.2,
                    Parallel = true,
                    Seed = null,
                };
            }

            Log.Info("Starting dummy analysis");

            Random random;
            if (arguments.Seed.HasValue)
            {
                Log.InfoFormat("Using given seed #{0}", arguments.Seed.Value);
                random = new Random(arguments.Seed.Value);
            }
            else
            {
                int seed = Environment.TickCount;
                random = new Random(seed);
                Log.InfoFormat("Using a generated seed #{0}", seed);
            }

            int tasks = arguments.Parallel ? Environment.ProcessorCount : 1;
            TimeSpan[] durations = new TimeSpan[tasks];

            double normalDuration = arguments.DurationSeconds;
            double jitterDuration = normalDuration * arguments.Jitter;
            for (int i = 0; i < durations.Length; i++)
            {
                double sample = random.NextDouble();
                var duration = normalDuration + ((jitterDuration * sample * 2.0) - jitterDuration);

                durations[i] = TimeSpan.FromSeconds(duration);
            }

            Log.InfoFormat("Starting {0} threads", tasks);
            var task = Task.WhenAll(durations.Select((d, i) => ConcurrentTask(d, i)));

            var result = await LoggedConsole.WriteWaitingLineAndWait(task);

            //Parallel.ForEach(durations, ParallelTask);

            Log.Info("Completed all work: " + result.Aggregate(string.Empty, (s, l) => s + ", " + l));

            return ExceptionLookup.Ok;
        }

        private static Task<long> ConcurrentTask(TimeSpan timeSpan, long index)
        {
            return Task.Run(() => ParallelTask(timeSpan, null, index));
        }

        private static long ParallelTask(TimeSpan timeSpan, ParallelLoopState parallelLoopState, long index)
        {
            Log.InfoFormat("Starting parallel branch {0} for {1}", index, timeSpan);
            var stopWatch = Stopwatch.StartNew();

            long lastLog = 0;

            long nowTicks;
            while ((nowTicks = stopWatch.ElapsedTicks) < timeSpan.Ticks)
            {
                // Like Thread.Sleep but does not give thread control to others
                // approx 0.54 seconds at ~2.7Ghz
                Thread.SpinWait(100_000_000);

                if (nowTicks - lastLog > LogEvery.Ticks)
                {
                    var percentage = nowTicks / (double)timeSpan.Ticks;
                    Log.InfoFormat("Branch {0}, {1:##0.00%} completed", index, percentage);
                    lastLog = nowTicks;
                }
            }

            stopWatch.Stop();

            Log.Info("Completed branch " + index);
            return stopWatch.ElapsedTicks;
        }
    }
}