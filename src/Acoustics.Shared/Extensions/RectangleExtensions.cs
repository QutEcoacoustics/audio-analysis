// <copyright file="RectangleExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace System.Drawing
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.Linq;
    using System.Text;

    public static class RectangleExtensions
    {
        public static int Area(this Rectangle rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }

        public static bool PointIntersect(this Rectangle rectangle, Point point)
        {
            return point.X >= rectangle.Left && point.X < rectangle.Right && point.Y >= rectangle.Bottom
                   && point.Y < rectangle.Top;
        }

        public static bool PointIntersect(this Rectangle rectangle, int x, int y)
        {
            return x >= rectangle.Left && x < rectangle.Right && y >= rectangle.Bottom
                   && y < rectangle.Top;
        }
    }
}
