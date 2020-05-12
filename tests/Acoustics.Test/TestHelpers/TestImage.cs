// <copyright file="TestImage.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public class TestImage
    {
        private static readonly Rgb24 DefaultBackground = Color.White;

        private static readonly Dictionary<char, Rgb24> KnownColors = new Dictionary<char, Rgb24>()
        {
            { 'R', Color.Red },
            { 'G', Color.Lime },
            { 'B', Color.Blue },
            { 'Y', Color.Yellow },
            { 'P', Color.Purple },
            { 'O', Color.Orange },
            { 'W', Color.White },
            { 'E', Color.Black },
            { '.', Color.Black },
        };

        private readonly Image<Rgb24> image;
        private readonly List<Func<Point, IImageProcessingContext, Point>> operations;
        private readonly Stack<(int Repeats, int StartIndex)> loops = new Stack<(int Repeats, int StartIndex)>();

        public TestImage(int width, int height, Rgb24? backgroundColor)
        {
            this.Cursor = new Point(0, 0);
            this.operations = new List<Func<Point, IImageProcessingContext, Point>>();
            this.image = new Image<Rgb24>(Configuration.Default, width, height, backgroundColor ?? DefaultBackground);
        }

        public TestImage(int width, int height, string specification, Rgb24? backgroundColor)
            : this(width, height, backgroundColor)
        {
            this.FillPattern(specification);
        }

        public static Image<Rgb24> Create(int width, int height, Color color, string specification)
        {
            return new TestImage(width, height, specification, color).Finish();
        }

        public static bool AddKnownColor(char symbol, Color color)
        {
            if (KnownColors.ContainsKey(symbol))
            {
                return false;
            }

            KnownColors.Add(symbol, color);
            return true;
        }

        public Point Cursor
        { get; private set; }

        public TestImage FillPattern(string specification, Color? defaultBackground = null)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                var lines = specification.Split('\r', '\n');

                // each line starts with a repeat count
                // and then contains instructions for color and repeat

                // pixel buffer
                Span<Rgb24> buffer = stackalloc Rgb24[this.image.Width];

                int row = 0;
                foreach (var line in lines)
                {
                    if (line.IsNullOrEmpty())
                    {
                        continue;
                    }

                    var shouldSkip = line.StartsWith("⬇", StringComparison.InvariantCulture);
                    if (shouldSkip)
                    {
                        var skipAmount = int.Parse(line[1..]);
                        row += skipAmount;
                        continue;
                    }

                    var repeatIndex = line.IndexOf('×');
                    int repeats;
                    ReadOnlySpan<char> rest;
                    if (repeatIndex == -1)
                    {
                        repeats = 1;
                        rest = line;
                    }
                    else
                    {
                        repeats = int.Parse(line[..repeatIndex]);
                        rest = line[(repeatIndex + 1)..];
                    }

                    // now modify pixel buffer
                    ParseLine(rest, ref buffer, defaultBackground ?? DefaultBackground);

                    // finally repeat each buffer onto image rows
                    for (int r = 0; r < repeats; r++)
                    {
                        var rowData = this.image.GetPixelRowSpan(row);
                        buffer.CopyTo(rowData);
                        row++;
                    }
                }

                return new Point(0, cursor.Y + row);
            }

            this.operations.Add(Action);
            return this;
        }

        internal static void ParseLine(ReadOnlySpan<char> line, ref Span<Rgb24> buffer, Color fill)
        {

            int bufferIndex = 0;
            int current = 0;

            var depth = new Stack<int>(3);
            Rgb24 color = default;
            int count = 0;
            while (current < line.Length)
            {
                // push previous sequence onto buffer
                PushBuffer(ref buffer, ref bufferIndex, ref count, color);

                switch (line[current])
                {
                    case '(':

                        depth.Push(bufferIndex);
                        current++;
                        break;
                    case ')':
                        var startOfBracket = depth.Pop();

                        // move to next token
                        current++;

                        // consume previous tokens by copying everything from start to now
                        // by number. A bracket group must be followed by a count number
                        // e.g. (ABC)2
                        if (StartsWithNumber(line[current..], out var bracketNumCharCount, out var groupCount))
                        {
                            var source = buffer[startOfBracket..bufferIndex];

                            // fill the buffer with repeats
                            // skip the first group because it is already in buffer
                            for (int c = 1; c < groupCount; c++)
                            {
                                source.CopyTo(buffer[bufferIndex..]);
                                bufferIndex += source.Length;
                            }

                            // reset count?
                            count = 0;

                            // move to next token
                            current += bracketNumCharCount;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"A bracket group must be suffixed with a count at {current}");
                        }

                        break;
                    case char code when KnownColors.TryGetValue(code, out var newColor):
                        // clear out old sequence, set up for new sequence
                        color = newColor;

                        // seek forward and find a number
                        if (StartsWithNumber(line[(current + 1)..], out var countNumChars, out var colorCount))
                        {
                            // e.g. R100, or B123
                            count = colorCount;

                            // move to next token ( count + token length)
                            current += countNumChars + 1;
                        }
                        else
                        {
                            // e.g. a color token by itself without a following number
                            count = 1;

                            // move to next token
                            current++;
                        }

                        break;
                    default:
                        throw new InvalidOperationException("unknown or unexpected code in TestImage specification string: " + line[current]);
                }
            }

            // push last buffer
            PushBuffer(ref buffer, ref bufferIndex, ref count, color);

            // if no instructions, fill rest of buffer with default
            if (bufferIndex < buffer.Length)
            {
                buffer[bufferIndex..].Fill(fill);
            }

            static void PushBuffer(ref Span<Rgb24> target, ref int bufferIndex, ref int count, Rgb24 color)
            {
                if (count > 0)
                {
                    var newIndex = bufferIndex + count;
                    target[bufferIndex..newIndex].Fill(color);
                    bufferIndex = newIndex;
                    count = 0;
                }
            }
        }

        private static bool StartsWithNumber(ReadOnlySpan<char> scan, out int countNumChars, out int parsedValue)
        {
            parsedValue = 0;
            countNumChars = 0;
            while ((countNumChars) < scan.Length && char.IsDigit(scan[countNumChars]))
            {
                countNumChars++;
            }

            if (countNumChars > 0)
            {
                parsedValue = int.Parse(scan[..(countNumChars)]);
                return true;
            }

            return false;
        }

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

        public TestImage GoTo(int x, int y)
        {
            Point Action(Point cursor, IImageProcessingContext context)
            {
                cursor = new Point(x, y);
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
                    cursor = new Point(this.image.Width, cursor.Y);
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