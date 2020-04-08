// <copyright file="RandomExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.Extensions
{
    using System;
    using SixLabors.ImageSharp;

    public static class RandomExtensions
    {
        public static Guid NextGuid(this Random random)
        {
            var uuid = new byte[16];
            random.NextBytes(uuid);

            return new Guid(uuid);
        }

        public static int NextInSequence(this Random random, int minimum, int maximum, int step)
        {
            var steps = (maximum - minimum) / step;
            return minimum + (random.Next(0, steps) * step);
        }

        public static long NextInSequence(this Random random, long minimum, long maximum, long step)
        {
            var steps = (maximum - minimum) / step;
            return minimum + (random.NextLong(0, steps) * step);
        }

        public static T NextChoice<T>(this Random random, params T[] choices)
        {
            return choices[random.Next(0, choices.Length)];
        }

        public static long NextLong(this Random random)
        {
            return NextLong(random, long.MinValue, long.MaxValue);
        }

        public static long NextLong(this Random random, long min, long max)
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (max - min)) + min;
        }

        public static double[,] NextMatrix(
            this Random random,
            int length,
            int height)
        {
            var array = new double[length, height];

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = random.NextDouble();
                }
            }

            return array;
        }

        public static DateTimeOffset NextDate(this Random random, DateTimeOffset? minimum = null, DateTimeOffset? maximum = null)
        {
            minimum = minimum ?? DateTimeOffset.MinValue;
            maximum = maximum ?? DateTimeOffset.MaxValue;

            var randomTick = random.NextLong(minimum.Value.Ticks, maximum.Value.Ticks);

            return new DateTimeOffset(randomTick, (TimeSpan)minimum?.Offset);
        }

        public static Range<double> NextRange(this Random random, double min = 0, double max = 1.0)
        {
            var delta = max - min;
            var a = (random.NextDouble() * delta) + min;
            var b = (random.NextDouble() * delta) + min;

            if (a < b)
            {
                return new Range<double>(a, b);
            }
            else
            {
                return new Range<double>(b, a);
            }
        }

        public static Color NextColor(this Random random, byte alpha = 255)
        {
            var value = random.Next();
            var bytes = BitConverter.GetBytes(value);

            return Color.FromRgba(bytes[0], bytes[1], bytes[2], alpha);
        }
    }
}