namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MoreLinq.Extensions;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;
    using Point = SixLabors.ImageSharp.Point;
    using Rectangle = SixLabors.ImageSharp.Rectangle;
    using RectangleF = SixLabors.ImageSharp.RectangleF;
    using Size = SixLabors.ImageSharp.Size;

    public class TestImage
    {
        private static readonly Rgb24 DefaultBackground = Color.White;
        private readonly Image<Rgb24> image;
        private readonly List<Func<Point, IImageProcessingContext, Point>> operations;
        private readonly Stack<(int Repeats, int startIndex)> loops = new Stack<(int Repeats, int startIndex)>();

        public TestImage(int width, int height, Rgb24? backgroundColor)
        {
            this.Cursor = new Point(0, 0);
            this.operations = new List<Func<Point, IImageProcessingContext, Point>>();
            this.image = new Image<Rgb24>(Configuration.Default, width, height, backgroundColor ?? DefaultBackground);

        }

        public Point Cursor { get; private set; }

        public TestImage Fill(int width, int height, Rgb24 color)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                context.Fill(color, new Rectangle(this.Cursor, new Size(width, height)));
                cursor.Offset(width, height);
                return cursor;
            }

            this.operations.Add(Action);
            return this;
        }

        public TestImage FillHorizontalSplit(int width, int height, params Rgb24[] colors)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                float segmentWidth = width / (float)colors.Length;
                for (int i = 0; i < colors.Length; i++)
                {
                    var x = this.Cursor.X + (i * segmentWidth);

                    context.Fill(colors[i], new RectangleF(x, this.Cursor.Y, segmentWidth, height));
                }

                cursor.Offset(width, height);
                return cursor;
            }

            this.operations.Add(Action);
            return this;
        }

        public TestImage Move(int x, int y)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                cursor.Offset(x, y);
                return cursor;
            }

            this.operations.Add(Action);
            return this;
        }

        public TestImage Move(Horizontal x, int y)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                switch (x)
                {
                    case Horizontal.Left:
                        cursor = new Point(0, cursor.Y + y);
                        break;
                    case Horizontal.Right:
                        cursor = new Point(this.image.Width, cursor.Y + y);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(x), x, null);
                }

                return cursor;
            }

            this.operations.Add(Action);
            return this;
        }

        public TestImage Move(int x, Vertical y)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                switch (y)
                {
                    case Vertical.Top:
                        cursor = new Point(cursor.X + x, 0);
                        break;
                    case Vertical.Bottom:
                        cursor = new Point(cursor.X + x, this.image.Height);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(y), y, null);
                }

                return cursor;
            }

            this.operations.Add(Action);
            return this;
        }

        public TestImage Move(Edge edge)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                if (edge.HasFlag(Edge.Top))
                {
                    cursor = new Point(cursor.X, 0);
                }

                if (edge.HasFlag(Edge.Bottom))
                {
                    cursor = new Point(cursor.X, this.image.Height);
                }

                if (edge.HasFlag(Edge.Right))
                {
                    cursor = new Point( this.image.Width, cursor.Y);
                }

                if (edge.HasFlag(Edge.Left))
                {
                    cursor = new Point(0, cursor.Y);
                }

                return cursor;
            }

            this.operations.Add(Action);
            return this;
        }

        public TestImage Repeat(int count)
        {
            this.loops.Push((count, this.operations.Count));
            return this;
        }

        public TestImage EndRepeat()
        {
            var (count, startIndex) = this.loops.Pop();

            var newOperations = this.operations.TakeLast(this.operations.Count - startIndex).ToArray();

            const int operationsAlreadyAppliedOnce = 1;
            for (var i = 0; i < count - operationsAlreadyAppliedOnce; i++)
            {
                this.operations.AddRange(newOperations);
            }

            return this;
        }

        public Image<Rgb24> Finish(FileInfo saveFile = null)
        {
            foreach (var operation in this.operations)
            {
                this.image.Mutate(context =>
                {
                    this.Cursor = operation.Invoke(this.Cursor, context);
                });
            }

            if (saveFile != null)
            {
                this.image.Save(saveFile.FullName);
            }

            return this.image;
        }
    }
}