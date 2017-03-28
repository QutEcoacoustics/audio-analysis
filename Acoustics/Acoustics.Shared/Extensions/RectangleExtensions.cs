namespace Acoustics.Shared.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    public static class RectangleExtensions
    {
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
