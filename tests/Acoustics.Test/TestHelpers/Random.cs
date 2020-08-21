// <copyright file="Random.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using MoreLinq;

    internal static class Random
    {
        public static System.Random GetRandom(int? seed = null)
        {
            seed ??= Environment.TickCount;

            Trace.WriteLine("\n\nRandom seed used: " + seed.Value);
            LoggedConsole.WriteWarnLine($"Random seed: {seed}");

            return new System.Random(seed.Value);
        }

        /// <summary>
        /// Gets a random subset always uses a fixed seed in #DEBUG mode.
        /// Useful for pulling consistent subsets of data tests when debugging.
        /// If the subset is truly random the test tooling can't cope with the test
        /// names changing every time it tries to gather metadata.
        /// In #DEBUG 100 items are returned if count is not specified.
        /// In #RELEASE the full set is returned if count is not specified.
        /// </summary>
        /// <returns>The full subset in RELEASE or a random subset in DEBUG.</returns>
        public static IEnumerable<T> DebugGetFixedRandomSubset<T>(this IEnumerable<T> set, int? count)
        {
            System.Random random;

#if DEBUG
            random = GetRandom(12345);
            return set.RandomSubset(count ?? 100, random);
#else
            if (count == null) {
                return set;
            }

            random = GetRandom();
            return set.RandomSubset(count.Value, random);
#endif
        }
    }
}