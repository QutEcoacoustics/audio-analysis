// <copyright file="Random.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Diagnostics;

    internal static class Random
    {
        public static System.Random GetRandom(int? seed = null)
        {
            seed = seed ?? Environment.TickCount;

            Debug.WriteLine("\n\nRandom seed used: " + seed.Value);
            LoggedConsole.WriteWarnLine($"Random seed: {seed}");

            return new System.Random(seed.Value);
        }
    }
}
