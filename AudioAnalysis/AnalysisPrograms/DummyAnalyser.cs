﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyAnalyser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   The purpose of this analyser is to make inter-program parallelisation easier to develop
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Production;

    using log4net;

    using PowerArgs;

    /// <summary>
    /// The purpose of this analyser is to make inter-program parallelisation easier to develop
    /// </summary>
    public class DummyAnalyser
    {
        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments
        {
            [DefaultValue(true)]
            public bool Parallel { get; set; }

            [DefaultValue(20)]
            [ArgRange(0, 3600)]
            public double DurationSeconds { get; set; }

            [DefaultValue(0.0)]
            [ArgRange(0.0, 1.0)]
            public double Jitter { get; set; }

            public int? Seed { get; set; }

            public static string Description()
            {
                return "An anaysis program to simulate load";
            }

            public static string AdditionalNotes()
            {
                // add long explantory notes here if you need to
                return "";
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan LogEvery = TimeSpan.FromSeconds(10);

        public static void Execute(Arguments arguments)
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
            var task = TaskEx.WhenAll(durations.Select((d, i) => ConcurrentTask(d, i)));
            LoggedConsole.WriteWaitingLine(task);

            //Parallel.ForEach(durations, ParallelTask);

            Log.Info("Completed all work");
        }

        private static Task<long> ConcurrentTask(TimeSpan timeSpan, long index)
        {
            return TaskEx.Run(() =>
            {
                ParallelTask(timeSpan, null, index);
                return timeSpan.Ticks;
            });
        }

        private static void ParallelTask(TimeSpan timeSpan, ParallelLoopState parallelLoopState, long index)
        {
            Log.InfoFormat("Starting parallel branch {0} for {1}", index, timeSpan);
            DateTime completeBy = DateTime.Now + timeSpan;

            var lastLog = DateTime.Now;
            while (completeBy >= DateTime.Now)
            {
                // Like Thread.Sleep but does not give thread control to others
                // approx 0.54 seconds at ~2.7Ghz
                Thread.SpinWait(100_000_000);

                var now = DateTime.Now;
                if ((now - lastLog) > LogEvery)
                {
                    var percentage = (timeSpan - (completeBy - now)).Ticks / (double)timeSpan.Ticks;
                    Log.InfoFormat("Branch {0}, {1:##0.00%} completed", index, percentage);
                    lastLog = now;
                }
            }

            Log.Info("Completed branch " + index);
        }
    }
}
