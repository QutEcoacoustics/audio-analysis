// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Oblong.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace TowseyLibrary
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using CsvHelper.Configuration;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public class Oblong
    {
        public sealed class OblongClassMap : ClassMap<Oblong>
        {
            public OblongClassMap()
            {
                this.Map(m => m.ColumnLeft).Name("Bottom");
                this.Map(m => m.ColumnRight).Name("Top");
                this.Map(m => m.RowTop).Name("Left");
                this.Map(m => m.RowBottom).Name("Right");
            }
        }

        // number of fuzzy membership values over range of col centroid values
        private const int CountColCentroidFuzzySet = 2;

        private const int FeatureCount = CountColCentroidFuzzySet + 2; // centroid location + freqWidth + time duration.

        private int[] colCentroidFuzzySet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Oblong"/> class.
        /// CONSTRUCTOR.
        /// </summary>
        /// <param name="row1">
        /// </param>
        /// <param name="col1">
        /// </param>
        /// <param name="row2">
        /// </param>
        /// <param name="col2">
        /// </param>
        public Oblong(int row1, int col1, int row2, int col2)
            : this(row1, col1, row2, col2, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Oblong"/> class.
        /// CONSTRUCTOR.
        /// </summary>
        /// <param name="row1">
        /// </param>
        /// <param name="col1">
        /// </param>
        /// <param name="row2">
        /// </param>
        /// <param name="col2">
        /// </param>
        public Oblong(int row1, int col1, int row2, int col2, ISet<Point> hitElements)
        {
            this.RowTop = row1;
            this.ColumnLeft = col1;
            this.RowBottom = row2;
            this.ColumnRight = col2;
            this.HitElements = hitElements;
            this.RowWidth = row2 - row1 + 1;
            this.ColWidth = col2 - col1 + 1;
            this.Category = -1;

            // set up fuzzy sets
            this.FuzzySetCentres();
        }

        public static int MaxCol { get; set; } = 256;

        /// <summary>
        ///     Gets location of Oblong's centre column in parent matrix.
        /// </summary>
        public int ColCentroid => this.ColumnLeft + ((this.ColumnRight - this.ColumnLeft + 1) / 2);

        public int ColWidth { get; private set; }

        public int RandomNumber { get; set; }

        public int RowWidth { get; private set; }

        /// <summary>
        /// Gets or sets the location of Oblong's left most column in parent matrix.
        /// </summary>
        public int ColumnLeft { get; set; }

        /// <summary>
        /// Gets or sets the location of Oblong's right most column in parent matrix.
        /// </summary>
        public int ColumnRight { get; set; }

        /// <summary>
        /// Gets or sets the collection of points that form the perimeter of the oblong.
        /// </summary>
        public ISet<Point> HitElements { get; set; }

        public int Category { get; set; }

        /// <summary>
        /// Gets or sets the location of Oblong's top row in parent matrix.
        /// </summary>
        public int RowTop { get; set; }

        /// <summary>
        /// Gets or sets the location of Oblong's bottom row in parent matrix.
        /// </summary>
        public int RowBottom { get; set; }

        public static void AssignCategories(List<Oblong> shapes, int[] categories)
        {
            if (shapes == null)
            {
                return;
            }

            if (categories == null)
            {
                return;
            }

            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Category = categories[i];
            }
        }

        /// <summary>
        /// returns a list of shapes that represent the averages of shapes in each category dervied
        ///     from FuzzyART clustering.
        /// </summary>
        public static List<Oblong> CategoryShapes(List<Oblong> shapes, int[] categories, int categoryCount)
        {
            if (shapes == null)
            {
                return null;
            }

            if (categories == null)
            {
                return null;
            }

            if (categoryCount == 0)
            {
                return null;
            }

            var categoryShapes = new List<Oblong>();
            for (int c = 0; c < categoryCount; c++)
            {
                int r1 = 0;
                int c1 = 0;
                int r2 = 0;
                int c2 = 0;
                int count = 0;
                for (int i = 0; i < shapes.Count; i++)
                {
                    if (categories[i] != c)
                    {
                        continue; // skip shapes not in category c
                    }

                    count++; // keep count of numbers in category c
                    Oblong s = shapes[i];
                    r1 += s.RowTop;
                    c1 += s.ColumnLeft;
                    r2 += s.RowBottom;
                    c2 += s.ColumnRight;
                }

                if (count == 0)
                {
                    continue; // no shapes assigned to this category
                }

                r1 /= count;
                c1 /= count;
                r2 /= count;
                c2 /= count;
                var shape = new Oblong(r1, c1, r2, c2);
                shape.Category = c;

                categoryShapes.Add(shape);
            }

            return categoryShapes;
        }

        public static Oblong Clone(Oblong s)
        {
            return new Oblong(s.RowTop, s.ColumnLeft, s.RowBottom, s.ColumnRight);
        }

        public static void ColumnWidth(double[,] m, int col, int row, out int colWidth)
        {
            // explore shape in x dimension
            int colCount = m.GetLength(1);
            colWidth = 0;
            while (col + colWidth < colCount && m[row, col + colWidth] == 1.0)
            {
                colWidth++;
            }
        }

        public static int ColumnOverlap(Oblong o1, Oblong o2)
        {
            if (o1.ColumnRight < o2.ColumnLeft)
            {
                return 0;
            }

            if (o2.ColumnRight < o1.ColumnLeft)
            {
                return 0;
            }

            // at this point the two events overlap
            int overlap = 0;
            if (o1.IncludesColumn(o2.ColumnLeft))
            {
                overlap = o1.ColumnRight - o2.ColumnLeft + 1;
            }
            else if (o2.IncludesColumn(o1.ColumnLeft))
            {
                overlap = o2.ColumnRight - o1.ColumnLeft + 1;
            }

            return overlap;
        }

        public static Oblong RotateOblongForStandardSpectrogram(Oblong oblong, int rowCount, int colCount)
        {
            // Translate time dimension = frames = matrix rows.
            int newTopRow = rowCount - oblong.ColumnRight;
            int newBottomRow = rowCount - oblong.ColumnLeft;

            //Translate freq dimension = freq bins = matrix columns.
            int newLeftCol = oblong.RowTop;
            int newRightCol = oblong.RowBottom;

            return new Oblong(newTopRow, newBottomRow, newLeftCol, newRightCol);
        }

        /// <summary>
        /// Reurns the distribution of the column-centroids.
        ///     The rectangular shapes are assumed to exist in a matrix
        ///     whose rows are time frames and whose columns are freq bins.
        ///     The returned distribution is therefore over freq domain.
        /// </summary>
        public static int[] Distribution(List<Oblong> shapes, int binCount)
        {
            if (shapes == null)
            {
                return null;
            }

            int binWidth = MaxCol / binCount;

            var distribution = new int[binCount];
            for (int i = 0; i < shapes.Count; i++)
            {
                int bin = shapes[i].colCentroid() / binWidth;
                if (bin >= binCount)
                {
                    bin = binCount - 1;
                }

                distribution[bin]++;
            }

            LoggedConsole.WriteLine("Number of data columns = " + MaxCol);

            LoggedConsole.WriteLine("One bin = " + binWidth + " of the original data columns.");

            WriteDistribution(distribution, binWidth);

            return distribution;
        }

        public static double[,] FeatureMatrix(List<Oblong> shapes)
        {
            int count = shapes.Count;
            if (count == 0)
            {
                return null;
            }

            double[] features = shapes[0].Features(); // use first shape to identify dimensions
            var data = new double[count, features.Length];

            for (int i = 0; i < count; i++)
            {
                features = shapes[i].FeaturesNormalised();
                for (int j = 0; j < features.Length; j++)
                {
                    data[i, j] = features[j];
                }
            }

            return data;
        }

        public static Oblong GetShape(int row, int col, ArrayList shapes)
        {
            if (shapes == null)
            {
                return null;
            }

            foreach (Oblong shape in shapes)
            {
                if (shape.PointInside(row, col))
                {
                    return shape;
                }
            }

            return null;
        }

        public static bool InExistingShape(int row, int col, ArrayList shapes)
        {
            if (shapes == null)
            {
                return false;
            }

            foreach (Oblong shape in shapes)
            {
                if (shape.PointInside(row, col))
                {
                    return true;
                }
            }

            return false;
        }

        public static ArrayList MergeCloseShapes(ArrayList shapes)
        {
            int distThreshold = 8;

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                var s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    var s2 = (Oblong)shapes[j];
                    double dist = s1.CentroidDistance(s2);
                    if (dist > distThreshold)
                    {
                        continue;
                    }

                    if (!s1.Overlaps(s2))
                    {
                        continue;
                    }

                    s2.RowTop = (s1.RowTop + s2.RowTop) / 2;
                    s2.ColumnLeft = (s1.ColumnLeft + s2.ColumnLeft) / 2;
                    s2.RowWidth = (s1.RowWidth + s2.RowWidth) / 2;
                    s2.ColWidth = (s1.ColWidth + s2.ColWidth) / 2;
                    shapes.RemoveAt(i);
                    break;
                }
            }

            return shapes;
        }

        public static ArrayList MergeShapesWhoseEndsOverlap(ArrayList shapes, int dyThreshold)
        {
            // merge shapes whose ends overlap
            LoggedConsole.WriteLine("Merge shapes whose ends overlap.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                var s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    var s2 = (Oblong)shapes[j];
                    if (!s1.Overlaps(s2))
                    {
                        continue;
                    }

                    double dy = s1.RowShift(s2);
                    LoggedConsole.WriteLine("dy=" + dy);

                    if (Math.Abs(dy) > dyThreshold)
                    {
                        continue;
                    }

                    // LoggedConsole.WriteLine("dy=" + dy);
                    s2.RowTop = (s1.RowTop + s2.RowTop) / 2;
                    s2.RowBottom = (s1.RowBottom + s2.RowBottom) / 2;
                    if (dy > 0)
                    {
                        // s2 to right of s1
                        s2.ColumnLeft = s1.ColumnLeft;
                        s2.ColWidth = s2.ColumnRight - s2.ColumnLeft + 1;
                    }
                    else
                    {
                        s2.ColumnRight = s1.ColumnRight;
                        s2.ColWidth = s2.ColumnRight - s2.ColumnLeft + 1;
                    }

                    shapes.RemoveAt(i);

                    // shapes[j] = s2;
                }
            }

            return shapes;
        }

        public static ArrayList MergeShapesWithAdjacentRows(ArrayList shapes, int dxThreshold, double ratio)
        {
            // merge shapes whose ends overlap
            LoggedConsole.WriteLine("Merge shapes whose sides are adjacent.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                var s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    var s2 = (Oblong)shapes[j];
                    if (!s1.AdjacentRows(s2))
                    {
                        continue; // not adjacent
                    }

                    double dx = s1.ColumnShift(s2);

                    // LoggedConsole.WriteLine("dx=" + dx);
                    if (Math.Abs(dx) > dxThreshold)
                    {
                        continue; // too much centroid displacement
                    }

                    if (s1.ColWidth / (double)s2.ColWidth > ratio || s2.ColWidth / (double)s1.ColWidth > ratio)
                    {
                        continue; // too much difference in shape width
                    }

                    // average the left and right column bounds
                    int s1w = s1.ColWidth;
                    int s2w = s2.ColWidth;
                    s2.ColumnLeft = ((s1.ColumnLeft * s1w) + (s2.ColumnLeft * s2w)) / (s1w + s2w); // weighted average
                    s2.ColumnRight = ((s1.ColumnRight * s1w) + (s2.ColumnRight * s2w)) / (s1w + s2w);

                    // s2.ColumnRight = (s1.ColumnRight  + s2.ColumnRight) / 2;

                    // adjust the row bounds
                    if (Math.Abs(s1.RowBottom - s2.RowTop) == 1)
                    {
                        // s1 is above s2
                        s2.RowTop = s1.RowTop;
                        s2.RowWidth = s2.RowBottom - s2.RowTop + 1;
                    }
                    else
                    {
                        // s1 is below s2
                        s2.RowBottom = s1.RowBottom;
                        s2.RowWidth = s2.RowBottom - s2.RowTop + 1;
                    }

                    // LoggedConsole.WriteLine("size="+shapes.Count+"  i="+i);
                    shapes.RemoveAt(i); // remove s1

                    // shapes[j] = s2;    //keep s2
                    break; // break inner loop if get to here
                }

                // inner loop
            }

            return shapes;
        }

        public static ArrayList ProcessShapes(ArrayList shapes)
        {
            shapes = RemoveSmallShapes(shapes);
            LoggedConsole.WriteLine("Number of shapes after removing small ones=" + shapes.Count);

            shapes = MergeCloseShapes(shapes);
            LoggedConsole.WriteLine("Number of shapes after merging close shapes=" + shapes.Count);

            return shapes;
        }

        public static ArrayList RemoveEnclosedShapes(ArrayList shapes)
        {
            LoggedConsole.WriteLine("Remove enclosed shapes.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                var s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    var s2 = (Oblong)shapes[j];
                    if (s2.Encloses(s1))
                    {
                        shapes.RemoveAt(i); // remove s1
                        continue;
                    }

                    if (s1.Encloses(s2))
                    {
                        shapes[j] = Clone(s1); // copy s1 in place of s2
                        shapes.RemoveAt(i); // remove s1
                    }
                }

                // inner loop
            }

            // outer loop
            return shapes;
        }

        public static ArrayList RemoveOverlappingShapes(ArrayList shapes)
        {
            LoggedConsole.WriteLine("Remove the smaller of any pair of overlapping shapes.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                var s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    var s2 = (Oblong)shapes[j];
                    if (!s1.Overlaps(s2))
                    {
                        continue; // not overlapping
                    }

                    if (s1.Area() < s2.Area())
                    {
                        shapes.RemoveAt(i); // remove s1
                    }
                    else if (s1.Area() > s2.Area())
                    {
                        shapes[j] = Clone(s1); // copy s1 in place of s2
                        shapes.RemoveAt(i); // remove s1
                    }
                }
            }

            return shapes;
        }

        public static ArrayList RemoveSmall(ArrayList shapes, int minArea)
        {
            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                var s1 = (Oblong)shapes[i];
                if (s1.Area() < minArea)
                {
                    shapes.RemoveAt(i);
                }
            }

            return shapes;
        }

        public static ArrayList RemoveSmallShapes(ArrayList shapes)
        {
            // int count = shapes.Count;
            // int[] areas = new int[count];
            // int id = 0;
            // foreach (Shape shape in shapes)
            // {
            // areas[id++] = shape.Area();
            // LoggedConsole.WriteLine("Area["+(id-1)+"]="+areas[id-1]);
            // }

            // int binCount = 200;
            // double binWidth;
            // int max = 0;
            // int min = Int32.MaxValue;
            // int[] histo = DataTools.Histo(areas, binCount, out binWidth, out min, out max);
            // DataTools.writeBarGraph(histo);
            // int maxIndex;
            // DataTools.getMaxIndex(histo, out maxIndex);
            // int valueAtMaxindex = (int)((double)maxIndex*binWidth);
            // LoggedConsole.WriteLine("Value AtMaxindex=" + valueAtMaxindex);
            int areaThreshold = 10;

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                var s = (Oblong)shapes[i];
                if (s.Area() < areaThreshold)
                {
                    shapes.RemoveAt(i);
                }
            }

            return shapes;
        }

        public static int RowOverlap(Oblong o1, Oblong o2)
        {
            if (o1.RowBottom < o2.RowTop)
            {
                return 0;
            }

            if (o2.RowBottom < o1.RowTop)
            {
                return 0;
            }

            // at this point the two events overlap
            int overlap = 0;
            if (o1.IncludesRow(o2.RowTop))
            {
                overlap = o1.RowBottom - o2.RowTop + 1;
            }
            else if (o2.IncludesRow(o1.RowTop))
            {
                overlap = o2.RowBottom - o1.RowTop + 1;
            }

            return overlap;
        }

        public static void Row_Width(double[,] m, int col, int row, out int rowWidth)
        {
            // explore shape in y dimension
            int rowCount = m.GetLength(0);
            rowWidth = 0;
            while (row + rowWidth < rowCount && m[row + rowWidth, col] == 1.0)
            {
                rowWidth++;
            }
        }

        public static void SaveImageOfCentroids(double[,] matrix, List<Oblong> shapes, Color colour, string opPath)
        {
            if (shapes == null)
            {
                return;
            }

            int rows = matrix.GetLength(0); // number of rows
            int cols = matrix.GetLength(1); // number
            var bmp = new Image<Rgb24>(cols, rows);

            foreach (Oblong shape in shapes)
            {
                // RowTop += 10;
                // int ColumnLeft = shape.ColumnLeft;
                // int RowBottom = RowTop + shape.RowWidth;
                // int ColumnRight = shape.ColumnRight;
                // int colorCount = ImageTools.darkColors.Length;
                // int colorID = shape.category % colorCount;
                // if (shape.category == -1) shapeColor = col;
                // else shapeColor = ImageTools.darkColors[colorID];

                // TransformCoordinates(RowTop, ColumnLeft, RowBottom, ColumnRight, out x1, out y1, out x2, out y2, mWidth);
                // for (int r = shape.RowTop; r <= shape.RowBottom; r++)
                // for (int c = shape.ColumnLeft; c <= shape.ColumnRight; c++)
                // bmp[c, r] = colour;
                for (int r = shape.RowTop; r <= shape.RowBottom; r++)
                {
                    bmp[shape.ColumnLeft, r] = colour;
                }

                for (int r = shape.RowTop; r <= shape.RowBottom; r++)
                {
                    bmp[shape.ColumnRight, r] = colour;
                }

                for (int c = shape.ColumnLeft; c <= shape.ColumnRight; c++)
                {
                    bmp[c, shape.RowTop] = colour;
                }

                for (int c = shape.ColumnLeft; c <= shape.ColumnRight; c++)
                {
                    bmp[c, shape.RowBottom] = colour;
                }

                // bmp[c, r] = colour;
                // RowTop += shape.RowWidth;
            }

            bmp.Mutate(x => x.Rotate(RotateMode.Rotate270));
            bmp.Save(opPath);
        }

        /// <summary>
        /// assume that the input matrix is purely binary, i.e. zeros and ones.
        /// </summary>
        /// <param name="matrix">
        /// </param>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        public static ArrayList ShapesDetect(double[,] matrix)
        {
            var shapes = new ArrayList();
            var random = new RandomNumber();

            int mHeight = matrix.GetLength(0);
            int mWidth = matrix.GetLength(1);

            for (int x = 5; x < mWidth; x++)
            {
                for (int y = 0; y < mHeight - 1; y++)
                {
                    if (matrix[y, x] != 1.0)
                    {
                        continue; // not in an object
                    }

                    if (matrix[y + 1, x] != 1.0)
                    {
                        y++;
                        continue; // shape must be > 2 pixels wide
                    }

                    // explore shape in y dimension
                    int rowWidth = 0;
                    while (rowWidth + y < mHeight && matrix[y + rowWidth, x] == 1.0)
                    {
                        rowWidth++;
                    }

                    rowWidth--; // back off one place
                    int yCentre = y + (rowWidth / 2); // position in centre of shape

                    if (InExistingShape(yCentre, x, shapes))
                    {
                        continue;
                    }

                    // explore shape in x dimension
                    int upDist = 0;
                    while (x + upDist < mWidth && matrix[yCentre, x + upDist] == 1.0)
                    {
                        upDist++;
                    }

                    if (matrix[yCentre, x + 1] == 0.0)
                    {
                        upDist = 1;
                    }

                    int dnDist = 0;
                    while (x - dnDist > 0 && matrix[yCentre, x - dnDist] == 1.0)
                    {
                        dnDist++;
                    }

                    dnDist--; // pull back one

                    // initialise possible shape.
                    int col1 = x - dnDist + 1;
                    int colWidth = upDist + dnDist - 2;
                    var shape = new Oblong(y, col1, y + rowWidth - 1, col1 + colWidth - 1);
                    shape.RandomNumber = random.GetInt(200); // set random number for id and color purposes

                    int[] centroid = shape.Centroid();

                    // LoggedConsole.WriteLine("Centroid=" + centroid[0] + ", " + centroid[1]);
                    // LoggedConsole.WriteLine("RowWidth=" + shape.RowWidth + "  ColWidth=" + shape.ColWidth);
                    shapes.Add(shape);

                    // more to end of shape
                    y = shape.RowBottom;
                }

                x += 4; // jump through 5 lines at a time.
            }

            LoggedConsole.WriteLine("Number of shapes=" + shapes.Count);

            shapes = ProcessShapes(shapes);

            // Console.ReadLine();
            return shapes;
        }

        public static List<Oblong> SortByColumnCentroid(List<Oblong> list1)
        {
            var list2 = new List<Oblong>();
            list2.AddRange(list1);
            list2.Sort(delegate (Oblong x, Oblong y) { return x.ColCentroid.CompareTo(y.ColCentroid); });
            return list2;
        }

        public static void WriteData2File(List<Oblong> shapes, string shapesDataFname)
        {
            double[,] data = FeatureMatrix(shapes);
            FileTools.WriteMatrix2File(data, shapesDataFname);
        }

        public static void WriteDistribution(int[] distribution, double binWidth)
        {
            int binCount = distribution.Length; // number of bins
            LoggedConsole.WriteLine("\nDistribution over " + binCount + " bins");
            for (int i = 0; i < binCount; i++)
            {
                LoggedConsole.Write(i + "\t");
            }

            LoggedConsole.WriteLine();
            for (int i = 0; i < binCount; i++)
            {
                LoggedConsole.Write((int)(i * binWidth) + "\t");
            }

            LoggedConsole.WriteLine("(Total " + (int)(binWidth * binCount) + " bins)");
            for (int i = 0; i < binCount; i++)
            {
                LoggedConsole.Write(distribution[i] + "\t");
            }

            LoggedConsole.WriteLine("Total instances=" + DataTools.Sum(distribution));
        }

        public bool AdjacentRows(Oblong s2)
        {
            // this.top-s2.bottom adjacency  OR  this.bottom-s2.top adjacency
            bool rowAdjacency = Math.Abs(this.RowBottom - s2.RowTop) == 1 || Math.Abs(this.RowTop - s2.RowBottom) == 1;
            return rowAdjacency;
        }

        public int Area()
        {
            return this.RowWidth * this.ColWidth;
        }

        public int[] Centroid()
        {
            var centre = new int[2];
            centre[0] = this.RowTop + (this.RowWidth / 2);
            centre[1] = this.ColumnLeft + (this.ColWidth / 2);
            return centre;
        }

        public double CentroidDistance(Oblong s2)
        {
            int[] c1 = this.Centroid();
            int[] c2 = s2.Centroid();
            int dx = c2[1] - c1[1];
            int dy = c2[0] - c1[0];
            double dist = Math.Sqrt((dx * dx) + (dy * dy));
            return dist;
        }

        public int ColumnShift(Oblong s2)
        {
            int dx = s2.colCentroid() - this.colCentroid();
            return dx;
        }

        public bool Encloses(Oblong s2)
        {
            bool row1Overlap = this.IncludesRow(s2.RowTop);
            bool row2Overlap = this.IncludesRow(s2.RowBottom);
            bool col1Overlap = this.IncludesColumn(s2.ColumnLeft);
            bool col2Overlap = this.IncludesColumn(s2.ColumnRight);
            if (row1Overlap && row2Overlap && col1Overlap && col2Overlap)
            {
                return true;
            }

            return false;
        }

        public double[] Features()
        {
            var features = new double[FeatureCount];
            double[] fuzzyMemberships = this.FuzzySetMemberships(this.colCentroid());
            for (int i = 0; i < CountColCentroidFuzzySet; i++)
            {
                features[0] = fuzzyMemberships[i];
            }

            // features[0] = this.colCentroid(); //column centroid
            // features[1] = this.colCentroid(); //column centroid
            // features[2] = this.colCentroid(); //column centroid
            features[CountColCentroidFuzzySet] = this.ColWidth; // column width - the frequency range of oblong
            features[CountColCentroidFuzzySet + 1] = this.RowWidth; // row width    - the time duration   of oblong
            return features;
        }

        public double[] FeaturesNormalised()
        {
            int maxRows = 54; // to NormaliseMatrixValues rowWidth.  27 rows/sec. ie row width is the fraction of 2 seconds
            double[] features = this.Features(); // get raw feature values

            // features[0] /= (double)maxCols; //column centroid
            // features[1] /= (double)maxCols; //column centroid
            // features[2] /= (double)maxCols; //column centroid
            features[CountColCentroidFuzzySet] /= MaxCol; // column width
            features[CountColCentroidFuzzySet + 1] /= maxRows; // row width

            for (int i = 0; i < FeatureCount; i++)
            {
                if (features[i] < 0.0)
                {
                    features[i] = 0.0;
                }

                if (features[i] > 1.0)
                {
                    features[i] = 1.0;
                }
            }

            return features;
        }

        public bool IncludesColumn(int colNumber)
        {
            if (colNumber >= this.ColumnLeft && colNumber <= this.ColumnRight)
            {
                return true;
            }

            return false;
        }

        public bool IncludesRow(int rowNumber)
        {
            if (rowNumber >= this.RowTop && rowNumber <= this.RowBottom)
            {
                return true;
            }

            return false;
        }

        public int OverlapArea(Oblong s2)
        {
            bool row1Overlap = this.IncludesRow(s2.RowTop);
            bool row2Overlap = this.IncludesRow(s2.RowBottom);
            bool col1Overlap = this.IncludesColumn(s2.ColumnLeft);
            bool col2Overlap = this.IncludesColumn(s2.ColumnRight);
            if (row1Overlap && row2Overlap && col1Overlap && col2Overlap)
            {
                return s2.Area();
            }

            if (!row1Overlap && !row2Overlap && !col1Overlap && !col2Overlap && this.Overlaps(s2))
            {
                return this.Area();
            }

            return 0;
        }

        public bool Overlaps(Oblong s2)
        {
            bool rowOverlap = false;
            for (int i = s2.RowTop; i < s2.RowBottom; i++)
            {
                if (this.IncludesRow(i))
                {
                    rowOverlap = true;
                    break;
                }
            }

            bool colOverlap = false;
            for (int i = s2.ColumnLeft; i < s2.ColumnRight; i++)
            {
                if (this.IncludesColumn(i))
                {
                    colOverlap = true;
                    break;
                }
            }

            if (rowOverlap && colOverlap)
            {
                return true;
            }

            return false;
        }

        public bool PointInside(int rowNumber, int colNumber)
        {
            // if ((rowNumber >= RowTop) && (rowNumber <= RowBottom) && (colNumber >= ColumnLeft) && (colNumber <= ColumnRight))
            // {
            // LoggedConsole.WriteLine(" true");
            // }
            if (rowNumber >= this.RowTop && rowNumber <= this.RowBottom && colNumber >= this.ColumnLeft && colNumber <= this.ColumnRight)
            {
                return true;
            }

            return false;
        }

        public int RowCentroid()
        {
            return this.RowTop + (this.RowWidth / 2);
        }

        public int RowShift(Oblong s2)
        {
            int dy = s2.RowCentroid() - this.RowCentroid();
            return dy;
        }

        public void WriteBounds()
        {
            LoggedConsole.WriteLine(" RowTop=" + this.RowTop + " ColumnLeft=" + this.ColumnLeft + " RowBottom=" + this.RowBottom + " ColumnRight=" + this.ColumnRight);
        }

        public void WriteProperties()
        {
            LoggedConsole.WriteLine(
                "Row count=" + this.RowWidth + "\tCol bandwidth=" + this.ColWidth + "\t ColCentroid="
                + this.colCentroid());
        }

        public int colCentroid()
        {
            return this.ColumnLeft + (this.ColWidth / 2);
        }

        private static double LinearInterpolate(int x1, double y1, int x2, double y2, int x3)
        {
            if (x3 < x1 || x3 > x2 || x1 > x2)
            {
                LoggedConsole.WriteLine("ERROR with Linear Interpolation! ((x3 < x1)||(x3 > x2)||(x1>x2))!!");
                return double.MaxValue;
            }

            double slope = (y2 - y1) / (x2 - x1);
            double dy = slope * (x3 - x1);
            return y1 + dy;
        }

        /// <summary>
        /// MAIN METHOD FOR UNIT TESTING.
        /// </summary>
        private static void Main()
        {
            LoggedConsole.WriteLine("TESTING METHODS IN CLASS DataTools");

            // string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\";
            bool doit1 = false;
            if (doit1)
            {
                // test1 method AREA, CENTROID and CENTROID-DISTANCE()
                LoggedConsole.WriteLine("Test Method Name()");
                int r1 = 10;
                int c1 = 10;
                int r2 = 20;
                int c2 = 20;
                var s1 = new Oblong(r1, c1, r2, c2);
                s1.WriteBounds();
                int[] centroid1 = s1.Centroid();
                LoggedConsole.WriteLine("Centroid1: r=" + centroid1[0] + "  c=" + centroid1[1]);
                LoggedConsole.WriteLine("Area 1=" + s1.Area());
                LoggedConsole.WriteLine();

                r1 = 17;
                c1 = 16;
                r2 = 23;
                c2 = 24;
                var s2 = new Oblong(r1, c1, r2, c2);
                s2.WriteBounds();
                int[] centroid2 = s2.Centroid();
                LoggedConsole.WriteLine("Centroid2: r=" + centroid2[0] + "  c=" + centroid2[1]);
                LoggedConsole.WriteLine("Area 2=" + s2.Area());
                double dist = s1.CentroidDistance(s2);
                LoggedConsole.WriteLine("Distance=" + dist);
            }

            // end test1

            bool doit2 = false;
            if (doit2)
            {
                // test2 method IncludesRow(), IncludesColumn(), PointInside()
                LoggedConsole.WriteLine("Test Method Name()");
                int r1 = 10;
                int c1 = 10;
                int r2 = 20;
                int c2 = 20;
                var s1 = new Oblong(r1, c1, r2, c2);
                s1.WriteBounds();
                r1 = 17;
                c1 = 16;
                r2 = 23;
                c2 = 24;
                var s2 = new Oblong(r1, c1, r2, c2);
                s2.WriteBounds();
                r1 = 20;
                c1 = 20;
                r2 = 30;
                c2 = 30;
                var s3 = new Oblong(r1, c1, r2, c2);
                s3.WriteBounds();

                LoggedConsole.WriteLine();
                LoggedConsole.WriteLine("Row10 in s1=" + s1.IncludesRow(10));
                LoggedConsole.WriteLine("Row15 in s1=" + s1.IncludesRow(15));
                LoggedConsole.WriteLine("Row20 in s1=" + s1.IncludesRow(20));
                LoggedConsole.WriteLine("Row25 in s1=" + s1.IncludesRow(25));
                LoggedConsole.WriteLine("Col05 in s1=" + s1.IncludesColumn(5));
                LoggedConsole.WriteLine("Col10 in s1=" + s1.IncludesColumn(10));
                LoggedConsole.WriteLine("Col15 in s1=" + s1.IncludesColumn(15));
                LoggedConsole.WriteLine("Col20 in s1=" + s1.IncludesColumn(20));

                int py = 23;
                int px = 25;
                bool inside = s1.PointInside(py, px);
                LoggedConsole.WriteLine("\nPoint (" + py + "," + px + ") inside s1 =" + inside);
                inside = s2.PointInside(py, px);
                LoggedConsole.WriteLine("Point (" + py + "," + px + ") inside s2 =" + inside);
                inside = s3.PointInside(py, px);
                LoggedConsole.WriteLine("Point (" + py + "," + px + ") inside s3 =" + inside);

                bool overlapped = s1.Overlaps(s3);
                LoggedConsole.WriteLine("\ns1 and s3 overlap =" + overlapped);
                overlapped = s1.Overlaps(s2);
                LoggedConsole.WriteLine("s1 and s2 overlap =" + overlapped);
            }

            // end test2

            if (true)
            {
                // test Method MergeShapes()
                LoggedConsole.WriteLine("Test MergeShapes()");
                var list = new ArrayList();
                int r1 = 10;
                int c1 = 10;
                int r2 = 20;
                int c2 = 20;
                var s1 = new Oblong(r1, c1, r2, c2);
                s1.WriteBounds();
                list.Add(s1);
                r1 = 17;
                c1 = 16;
                r2 = 23;
                c2 = 24;
                var s2 = new Oblong(r1, c1, r2, c2);
                s2.WriteBounds();
                list.Add(s2);
                r1 = 20;
                c1 = 20;
                r2 = 30;
                c2 = 30;
                var s3 = new Oblong(r1, c1, r2, c2);
                s3.WriteBounds();
                list.Add(s3);

                LoggedConsole.WriteLine(" dy(s2-s1)= " + s1.RowShift(s2));
                LoggedConsole.WriteLine(" dy(s3-s2)= " + s2.RowShift(s3));

                int dyThreshold = 6;
                list = MergeShapesWhoseEndsOverlap(list, dyThreshold);
                LoggedConsole.WriteLine("List size=" + list.Count);
                foreach (Oblong s in list)
                {
                    s.WriteBounds();
                }
            }

            // end test3

            // if (false) //test Method()
            // {
            // LoggedConsole.WriteLine("Test Method Name()");
            // } //end test4
            LoggedConsole.WriteLine("\nFINISHED!!");
            Console.ReadLine();
        }

        private void FuzzySetCentres()
        {
            int space = MaxCol / (CountColCentroidFuzzySet - 1);
            this.colCentroidFuzzySet = new int[CountColCentroidFuzzySet];
            this.colCentroidFuzzySet[0] = 0;
            this.colCentroidFuzzySet[CountColCentroidFuzzySet - 1] = MaxCol;
            for (int i = 1; i < CountColCentroidFuzzySet - 1; i++)
            {
                this.colCentroidFuzzySet[i] = i * space;
            }

            // LoggedConsole.WriteLine("Array of " + countColCentroidFuzzySet + " Fuzzy Set Centres");
            // DataTools.writeArray(colCentroidFuzzySet);
            // Console.ReadLine();
        }

        private double[] FuzzySetMemberships(int row)
        {
            int n = CountColCentroidFuzzySet; // for clarity!!!!!
            var FM = new double[n]; // fuzzy memberships
            int x1, x2;
            double y1, y2;

            // row = 192; //for testing

            // calculate membership of fuzzy set 0;
            if (row < 0 || row >= this.colCentroidFuzzySet[1])
            {
                FM[0] = 0;
            }

            // else FM[0] = LinearInterpolate(x1, y1, x2, y2, x3);
            else
            {
                x1 = 0;
                x2 = this.colCentroidFuzzySet[1];
                y1 = 1.0;
                y2 = 0.0;
                FM[0] = LinearInterpolate(x1, y1, x2, y2, row);
            }

            // calculate membership of fuzzy set n;
            if (row > MaxCol || row <= this.colCentroidFuzzySet[n - 2])
            {
                FM[n - 1] = 0;
            }
            else
            {
                x1 = this.colCentroidFuzzySet[n - 2];
                x2 = this.colCentroidFuzzySet[n - 1];
                y1 = 0.0;
                y2 = 1.0;
                FM[n - 1] = LinearInterpolate(x1, y1, x2, y2, row);
            }

            // calculate membership of fuzzy sets 1 to n-1;
            for (int i = 1; i < CountColCentroidFuzzySet - 1; i++)
            {
                if (row >= this.colCentroidFuzzySet[i + 1] || row <= this.colCentroidFuzzySet[i - 1])
                {
                    FM[i] = 0;
                }
                else // row below mode of membership function
                    if (row < this.colCentroidFuzzySet[i])
                {
                    x1 = this.colCentroidFuzzySet[i - 1];
                    x2 = this.colCentroidFuzzySet[i];
                    y1 = 0.0;
                    y2 = 1.0;
                    FM[i] = LinearInterpolate(x1, y1, x2, y2, row);
                }
                else // row above mode of membership function
                        if (row >= this.colCentroidFuzzySet[i])
                {
                    x1 = this.colCentroidFuzzySet[i];
                    x2 = this.colCentroidFuzzySet[i + 1];
                    y1 = 1.0;
                    y2 = 0.0;
                    FM[i] = LinearInterpolate(x1, y1, x2, y2, row);
                }
            }

            // end for loop

            // LoggedConsole.WriteLine("For row "+row+" memberships are:");
            // DataTools.writeArray(FM);
            // Console.ReadLine();
            return FM;
        }
    }
}