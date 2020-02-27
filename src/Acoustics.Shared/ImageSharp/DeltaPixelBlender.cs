// <copyright file="DeltaPixelBlender.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using SixLabors.ImageSharp.PixelFormats;

    /// <summary>
    /// Blends pixels based on their differences.
    /// Pixels that are equal return gray.
    /// Pixels where the source is less than backdrop return black.
    /// Pixels where the source is greater than the backdrop return white.
    /// </summary>
    /// <typeparam name="TPixel">The type of pixel to operate on.</typeparam>
    public class DeltaPixelBlender<TPixel> : PixelBlender<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        private static readonly Vector4 Middle = new Vector4(0.5f);
        private static readonly Vector4 Bottom = new Vector4(0.0f);
        private static readonly Vector4 Top = new Vector4(1.0f);

        public override TPixel Blend(TPixel background, TPixel source, float amount)
        {
            TPixel dest = default;

            dest.FromScaledVector4(Delta(background.ToScaledVector4(), source.ToScaledVector4(), amount));

            return dest;
        }

        protected override void BlendFunction(Span<Vector4> destination, ReadOnlySpan<Vector4> background,
            ReadOnlySpan<Vector4> source, float amount)
        {
            amount = amount.Clamp(0, 1);
            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = Delta(background[i], source[i], amount);
            }
        }

        protected override void BlendFunction(Span<Vector4> destination, ReadOnlySpan<Vector4> background,
            ReadOnlySpan<Vector4> source, ReadOnlySpan<float> amount)
        {
            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = Delta(background[i], source[i], amount[i].Clamp(0, 1));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 Delta(Vector4 backdrop, Vector4 source, float amount)
        {
            if (backdrop == source)
            {
                return Middle;
            }

            return source.LengthSquared() < backdrop.LengthSquared() ? Bottom : Top;
        }
    }
}
