using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{



    public class Shape
    {
        public int r1 { get; set; }  //location of top left corner in parent matrix
        public int c1 { get; set; }  //location of top left corner in parent matrix
        public int r2 { get; set; }  //location of bot right corner in parent matrix
        public int c2 { get; set; }  //location of bot right corner in parent matrix

        //private int rowWidth;
        //public int RowWidth { get { row2 - row1 + 1; } }
        public int RowWidth { get; set; }
        public int ColWidth { get; set; }
        public int RandomNumber { get; set; }
        public int category { get; set; }
        public static bool Verbose { get; set; }

        //vars required to set up Fuzzy Sets for row centroid feature
        static int maxCol = 512;
        int countColCentroid_FS = 2; //number of fuzzy sets 
        int[] colCentroid_FS;


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="row1"></param>
        /// <param name="col1"></param>
        /// <param name="row2"></param>
        /// <param name="col2"></param>
        public Shape(int row1, int col1, int row2, int col2)
        {
            this.r1 = row1;
            this.c1 = col1;
            this.r2 = row2;
            this.c2 = col2;
            this.RowWidth = row2 - row1 + 1;
            this.ColWidth = col2 - col1 + 1;
            category = -1;


            //set up fuzzy sets
            FuzzySetCentres();
        }

        public int Area()
        {
            return (RowWidth * ColWidth);
        }


        public bool PointInside(int rowNumber, int colNumber)
        {
            //if ((rowNumber >= r1) && (rowNumber <= r2) && (colNumber >= c1) && (colNumber <= c2))
            //{
            //    Console.WriteLine(" true");
            //}
            if ((rowNumber >= this.r1) && (rowNumber <= this.r2) && (colNumber >= this.c1) && (colNumber <= this.c2)) return true;
            else return false;
        }

        public int[] Centroid()
        {
            int[] centre = new int[2];
            centre[0] = this.r1 + RowWidth/2;
            centre[1] = this.c1 + ColWidth/2;
            return centre;
        }

        public int row_Centroid()
        {
            return this.r1 + (RowWidth / 2);
        }
        public int col_Centroid()
        {
            return this.c1 + (ColWidth / 2);
        }

        public bool IncludesRow(int rowNumber)
        {
            if ((rowNumber >= this.r1) && (rowNumber <= this.r2)) return true;
            else return false;
        }

        public bool IncludesColumn(int colNumber)
        {
            if ((colNumber >= this.c1) && (colNumber <= this.c2)) return true;
            else return false;
        }

        
        public bool Overlaps(Shape s2)
        {
            bool rowOverlap = false;
            for(int i = s2.r1; i< s2.r2; i++) 
            {   if(this.IncludesRow(i)) 
                {   
                    rowOverlap = true;
                    break;
                }
            }
            bool colOverlap = false;
            for (int i = s2.c1; i < s2.c2; i++)
            {
                if (this.IncludesColumn(i))
                {
                    colOverlap = true;
                    break;
                }
            }
            if (rowOverlap && colOverlap) return true; 
            else return false;
        }


        public int OverlapArea(Shape s2)
        {
            bool row1Overlap = this.IncludesRow(s2.r1);
            bool row2Overlap = this.IncludesRow(s2.r2);
            bool col1Overlap = this.IncludesColumn(s2.c1);
            bool col2Overlap = this.IncludesColumn(s2.c2);
            if (row1Overlap && row2Overlap && col1Overlap && col2Overlap) return s2.Area();

            if (!row1Overlap && !row2Overlap && !col1Overlap && !col2Overlap && this.Overlaps(s2)) return this.Area();
            return 0;
        }


        public bool Encloses(Shape s2)
        {
            bool row1Overlap = this.IncludesRow(s2.r1);
            bool row2Overlap = this.IncludesRow(s2.r2);
            bool col1Overlap = this.IncludesColumn(s2.c1);
            bool col2Overlap = this.IncludesColumn(s2.c2);
            if (row1Overlap && row2Overlap && col1Overlap && col2Overlap) return true;
            return false;
        }


        public bool AdjacentRows(Shape s2)
        {
            //this.top-s2.bottom adjacency  OR  this.bottom-s2.top adjacency
            bool rowAdjacency = ((Math.Abs(this.r2 - s2.r1) == 1) || (Math.Abs(this.r1 - s2.r2) == 1));
            return rowAdjacency;
        }


        public double CentroidDistance(Shape s2)
        {
            int[] c1 = this.Centroid();
            int[] c2 = s2.Centroid();
            int dx = c2[1] - c1[1]; 
            int dy = c2[0] - c1[0];
            double dist = Math.Sqrt((dx * dx) + (dy * dy));
            return dist;
        }

        public int RowShift(Shape s2)
        {
            int dy = s2.row_Centroid() - this.row_Centroid();
            return dy;
        }


        public int ColumnShift(Shape s2)
        {
            int dx = s2.col_Centroid() - this.col_Centroid();
            return dx;
        }


        public void WriteBounds()
        {
            Console.WriteLine(" r1=" + this.r1 + " c1=" + this.c1 + " r2=" + this.r2 + " c2=" + this.c2);
        }

        //***********************************************************************************************************************
        //***********************************************************************************************************************
        //***********************************************************************************************************************

        public double[] Features()
        {
            int featureCount = countColCentroid_FS + 2;
            double[] features = new double[featureCount];
            double[] fuzzyMemberships = FuzzySetMemberships(this.col_Centroid());
            for (int i = 0; i < countColCentroid_FS; i++) features[0] = fuzzyMemberships[i];

            
          //  features[0] = this.col_Centroid(); //column centroid
          //  features[1] = this.col_Centroid(); //column centroid
          //  features[2] = this.col_Centroid(); //column centroid
            features[countColCentroid_FS]   = this.ColWidth; //column width
            features[countColCentroid_FS+1] = this.RowWidth; //row width
            return features;
        }
        public double[] Features_Normalised()
        {
            int featureCount = countColCentroid_FS + 2;
            int maxRows = 54; //to normalise rowWidth.  27 rows/sec. ie row width is the fraction of 2 seconds
            double[] features = Features(); //get raw feature values
            //features[0] /= (double)maxCols; //column centroid
            //features[1] /= (double)maxCols; //column centroid
            //features[2] /= (double)maxCols; //column centroid
            features[countColCentroid_FS]    /= (double)Shape.maxCol; //column width
            features[countColCentroid_FS + 1] /= (double)maxRows; //row width

            for (int i = 0; i < featureCount; i++)
            {
                if (features[i] < 0.0) features[i] = 0.0;
                if (features[i] > 1.0) features[i] = 1.0;
            }
            return features;
        }

        private void FuzzySetCentres()
        {
            int space = maxCol / (countColCentroid_FS-1);
            colCentroid_FS = new int[countColCentroid_FS];
            colCentroid_FS[0] = 0;
            colCentroid_FS[countColCentroid_FS - 1] = maxCol;
            for (int i = 1; i < countColCentroid_FS - 1; i++) colCentroid_FS[i] = (i * space);
            //Console.WriteLine("Array of " + countColCentroid_FS + " Fuzzy Set Centres");
            //DataTools.writeArray(colCentroid_FS);
            //Console.ReadLine();
        }

        private double[] FuzzySetMemberships(int row)
        {
            int n = countColCentroid_FS; //for clarity!!!!!
            double[] FM = new double[n]; //fuzzy memberships
            int x1, x2;
            double y1, y2;
            //row = 192; //for testing

            //calculate membership of fuzzy set 0;
            if ((row < 0) || (row >= colCentroid_FS[1])) FM[0] = 0;
            //else FM[0] = LinearInterpolate(x1, y1, x2, y2, x3);
            else
            {
                x1 = 0;
                x2 = colCentroid_FS[1];
                y1 = 1.0;
                y2 = 0.0;
                FM[0] = LinearInterpolate(x1, y1, x2, y2, row);
            }

            //calculate membership of fuzzy set n;
            if ((row > maxCol) || (row <= colCentroid_FS[n-2])) FM[n-1] = 0;
            else
            {
                x1 = colCentroid_FS[n - 2];
                x2 = colCentroid_FS[n - 1];
                y1 = 0.0;
                y2 = 1.0;
                FM[n-1] = LinearInterpolate(x1, y1, x2, y2, row);
            }

            //calculate membership of fuzzy sets 1 to n-1;
            for (int i = 1; i < countColCentroid_FS - 1; i++)
            {
                if ((row >= colCentroid_FS[i + 1]) || (row <= colCentroid_FS[i - 1])) FM[i] = 0;
                else //row below mode of membership function
                    if (row < colCentroid_FS[i])
                    {
                        x1 = colCentroid_FS[i - 1];
                        x2 = colCentroid_FS[i];
                        y1 = 0.0;
                        y2 = 1.0;
                        FM[i] = LinearInterpolate(x1, y1, x2, y2, row);
                    }
                    else //row above mode of membership function
                        if (row >= colCentroid_FS[i])
                        {
                            x1 = colCentroid_FS[i];
                            x2 = colCentroid_FS[i + 1];
                            y1 = 1.0;
                            y2 = 0.0;
                            FM[i] = LinearInterpolate(x1, y1, x2, y2, row);
                        }
            }//end for loop
            //Console.WriteLine("For row "+row+" memberships are:");
            //DataTools.writeArray(FM);
            //Console.ReadLine();
            return FM;
        }

        private static double LinearInterpolate(int x1, double y1, int x2, double y2, int x3)
        {
            if((x3 < x1)||(x3 > x2)||(x1>x2))
            {
                Console.WriteLine("ERROR with Linear Interpolation! ((x3 < x1)||(x3 > x2)||(x1>x2))!!");
                return Double.MaxValue;
            }
            double slope = (y2 - y1) / (double)(x2 - x1);
            double dy = slope * (x3 - x1);
            return (y1 + dy);
        }



        //***********************************************************************************************************************
        //***********************************************************************************************************************
        //***********************************************************************************************************************




        public static Shape Clone(Shape s)
        {
            return new Shape(s.r1, s.c1, s.r2, s.c2);
        }


        /// <summary>
        /// 
        /// assume that the input matrix is purely binary, i.e. zeros and ones
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static ArrayList Shapes_Detect(double[,] matrix)
        {
            ArrayList shapes = new ArrayList();
            RandomNumber random = new RandomNumber();

            int mHeight = matrix.GetLength(0);
            int mWidth = matrix.GetLength(1);

            for (int x = 5; x < mWidth; x++)
            {
                for (int y = 0; y < mHeight - 1; y++)
                {
                    if(matrix[y,x] != 1.0) continue; //not in an object

                    if (matrix[y + 1, x] != 1.0)
                    {
                        y++;
                        continue; // shape must be > 2 pixels wide
                    }

                    //explore shape in y dimension
                    int rowWidth = 0;
                    while (((rowWidth + y) < mHeight) && (matrix[y + rowWidth, x] == 1.0))
                    {
                        rowWidth++;
                    }
                    rowWidth--;//back off one place
                    int yCentre = y + (rowWidth / 2); //position in centre of shape

                    if(InExistingShape(yCentre, x, shapes)) continue;
                    

                    //explore shape in x dimension
                    int upDist = 0;
                    while (((x + upDist) < mWidth) && (matrix[yCentre, x + upDist] == 1.0)) upDist++;
                    if (matrix[yCentre, x + 1] == 0.0) upDist = 1;
                    int dnDist = 0;
                    while (((x - dnDist) > 0) && (matrix[yCentre, x - dnDist] == 1.0)) dnDist++;
                    dnDist--; //pull back one

                    // initialise possible shape.
                    int col1 = x - dnDist + 1;
                    int colWidth = upDist + dnDist - 2;
                    Shape shape = new Shape(y, col1, y + rowWidth - 1, col1 + colWidth -1);
                    shape.RandomNumber = random.GetInt(200); //set random number for id and color purposes

                    int[] centroid = shape.Centroid();
                    //Console.WriteLine("Centroid=" + centroid[0] + ", " + centroid[1]);
                    //Console.WriteLine("RowWidth=" + shape.RowWidth + "  ColWidth=" + shape.ColWidth);
                    shapes.Add(shape);
                    //more to end of shape
                    y = shape.r2;
                }
                x += 4; //jump through 5 lines at a time.
            }

            if (Shape.Verbose) Console.WriteLine("Number of shapes=" + shapes.Count);
            shapes = ProcessShapes(shapes);

            //Console.ReadLine();
            return shapes;
        }


        public static void Row_Width(double[,] m, int col, int row, out int rowWidth) //explore shape in y dimension
        {
            int rowCount = m.GetLength(0);
            rowWidth = 0;
            while (((row + rowWidth) < rowCount) && (m[row + rowWidth, col] == 1.0)) rowWidth++;
        }
        public static void Col_Width(double[,] m, int col, int row, out int colWidth) //explore shape in x dimension
        {
            int colCount = m.GetLength(1);
            colWidth = 0;
            while (((col + colWidth) < colCount) && (m[row, col + colWidth] == 1.0)) colWidth++;
        }


        public static bool InExistingShape(int row, int col, ArrayList shapes)
        {
            if (shapes == null) return false;
            foreach(Shape shape in shapes) if(shape.PointInside(row, col)) return true;
            return false;
        }


        public static Shape GetShape(int row, int col, ArrayList shapes)
        {
            if (shapes == null) return null;
            foreach(Shape shape in shapes) if(shape.PointInside(row, col)) return shape;
            return null;
        }

        public static ArrayList RemoveSmallShapes(ArrayList shapes)
        {
            //int count = shapes.Count;
            //int[] areas = new int[count];
            //int id = 0;
            //foreach (Shape shape in shapes)
            //{
            //    areas[id++] = shape.Area();
            //    Console.WriteLine("Area["+(id-1)+"]="+areas[id-1]);
            //}

            //int binCount = 200;
            //double binWidth;
            //int max = 0;
            //int min = Int32.MaxValue;
            //int[] histo = DataTools.Histo(areas, binCount, out binWidth, out min, out max);
            //DataTools.writeBarGraph(histo);
            //int maxIndex; 
            //DataTools.getMaxIndex(histo, out maxIndex);
            //int valueAtMaxindex = (int)((double)maxIndex*binWidth);
            //Console.WriteLine("Value AtMaxindex=" + valueAtMaxindex);

            int areaThreshold = 10;

            for(int i=shapes.Count-1; i>=0; i--)
            {    
                Shape s = (Shape)shapes[i];
                if (s.Area() < areaThreshold) shapes.RemoveAt(i);
            }
            return shapes;
        }


        public static ArrayList MergeCloseShapes(ArrayList shapes)
        {
            int distThreshold = 8; 

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Shape s1 = (Shape)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Shape s2 = (Shape)shapes[j];
                    double dist = s1.CentroidDistance(s2);
                    if (dist > distThreshold) continue;
                    if(! s1.Overlaps(s2)) continue;
                    s2.r1 = (s1.r1 + s2.r1) / 2;
                    s2.c1 = (s1.c1 + s2.c1) / 2;
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
            if (Shape.Verbose) Console.WriteLine("Merge shapes whose ends overlap.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Shape s1 = (Shape)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Shape s2 = (Shape)shapes[j];
                    if (!s1.Overlaps(s2)) continue;
                    double dy = s1.RowShift(s2);
                    if (Shape.Verbose) Console.WriteLine("dy=" + dy);
                    if (Math.Abs(dy) > dyThreshold) continue;
                    //Console.WriteLine("dy=" + dy);

                    s2.r1 = (s1.r1 + s2.r1) / 2;
                    s2.r2 = (s1.r2 + s2.r2) / 2;
                    if (dy > 0) //s2 to right of s1
                    {
                        s2.c1 = s1.c1;
                        s2.ColWidth = s2.c2 - s2.c1 + 1;

                    }
                    else
                    {
                        s2.c2 = s1.c2;
                        s2.ColWidth = s2.c2 - s2.c1 + 1;
                    }
                    shapes.RemoveAt(i);
                    //shapes[j] = s2;
                }
            }
            return shapes;
        }

        public static ArrayList MergeShapesWithAdjacentRows(ArrayList shapes, int dxThreshold, double ratio)
        {
            // merge shapes whose ends overlap
            if (Shape.Verbose) Console.WriteLine("Merge shapes whose sides are adjacent.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Shape s1 = (Shape)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Shape s2 = (Shape)shapes[j];
                    if (!s1.AdjacentRows(s2)) continue; //not adjacent

                    double dx = s1.ColumnShift(s2);
                    //Console.WriteLine("dx=" + dx);
                    if (Math.Abs(dx) > dxThreshold) continue; //too much centroid displacement

                    if (((s1.ColWidth / (double)s2.ColWidth) > ratio) || ((s2.ColWidth / (double)s1.ColWidth) > ratio)) continue; //too much difference in shape width  

                    //average the left and right column bounds
                    int s1w = s1.ColWidth;
                    int s2w = s2.ColWidth;
                    s2.c1 = ((s1.c1 * s1w) + (s2.c1 * s2w)) / (s1w + s2w); // weighted average
                    s2.c2 = ((s1.c2 * s1w) + (s2.c2 * s2w)) / (s1w + s2w);
                    //s2.c2 = (s1.c2  + s2.c2) / 2;

                    //adjust the row bounds
                    if (Math.Abs(s1.r2 - s2.r1) == 1) //s1 is above s2
                    {
                        s2.r1 = s1.r1;
                        s2.RowWidth = s2.r2 - s2.r1 + 1;
                    }
                    else  //s1 is below s2
                    {
                        s2.r2 = s1.r2;
                        s2.RowWidth = s2.r2 - s2.r1 + 1;
                    }
                    //Console.WriteLine("size="+shapes.Count+"  i="+i);
                    shapes.RemoveAt(i);  //remove s1
                    //shapes[j] = s2;    //keep s2
                    break; //break inner loop if get to here
                }//inner loop
            }
            return shapes;
        }

        public static ArrayList RemoveOverlappingShapes(ArrayList shapes)
        {
            if (Shape.Verbose) Console.WriteLine("Remove the smaller of any pair of overlapping shapes.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Shape s1 = (Shape)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Shape s2 = (Shape)shapes[j];
                    if (!s1.Overlaps(s2)) continue; //not overlapping
                    if (s1.Area() < s2.Area()) shapes.RemoveAt(i);  //remove s1
                    else
                    if (s1.Area() > s2.Area())
                    {
                        shapes[j] = Shape.Clone(s1);//copy s1 in place of s2
                        shapes.RemoveAt(i);  //remove s1
                    }
                }
            }
            return shapes;
        }

        public static ArrayList RemoveEnclosedShapes(ArrayList shapes)
        {
            if (Shape.Verbose) Console.WriteLine("Remove enclosed shapes.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Shape s1 = (Shape)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Shape s2 = (Shape)shapes[j];
                    if (s2.Encloses(s1))
                    {
                        shapes.RemoveAt(i);  //remove s1
                        continue;
                    }
                    if (s1.Encloses(s2))
                    {
                        shapes[j] = Shape.Clone(s1);//copy s1 in place of s2
                        shapes.RemoveAt(i);  //remove s1
                    }
                }//inner loop
            }//outer loop
            return shapes;
        }

        public static ArrayList RemoveSmall(ArrayList shapes, int minArea)
        {
            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Shape s1 = (Shape)shapes[i];
                if (s1.Area() < minArea) shapes.RemoveAt(i);
            }
            return shapes;
        }


        public static ArrayList ProcessShapes(ArrayList shapes)
        {
            shapes = RemoveSmallShapes(shapes);
            if (Shape.Verbose) Console.WriteLine("Number of shapes after removing small ones=" + shapes.Count);
            shapes = MergeCloseShapes(shapes);
            if (Shape.Verbose) Console.WriteLine("Number of shapes after merging close shapes=" + shapes.Count);
            return shapes;
        }

        public static double[,] FeatureMatrix(ArrayList shapes)
        {
            int count = shapes.Count;
            if (count == 0) return null;
            Shape s = (Shape)shapes[0]; //get first shape to identify dimensions
            double[] features = s.Features();
            double[,] data = new double[count, features.Length];

            for (int i = 0; i < count; i++)
            {
                s = (Shape)shapes[i];
                features = s.Features_Normalised();
                for (int j = 0; j < features.Length; j++) data[i, j] = features[j];
            }
            return data;
        }


        public static void WriteData2File(ArrayList shapes, string shapesDataFname)
        {
            double[,] data = FeatureMatrix(shapes);
            FileTools.WriteMatrix2File(data, shapesDataFname);
        }


        public static ArrayList AssignCategories(ArrayList shapes, int[] categories)
        {
            if (shapes == null) return null;
            if (categories == null) return shapes;

            for (int i=0; i< shapes.Count; i++)
            {
                Shape s = (Shape)shapes[i];
                s.category = categories[i];
                shapes[i] = s;
                //int colorID = ((Shape)shapes[i]).category;
            }

            return shapes;
        }

        /// <summary>
        /// returns a list of shapes that represent the averages of shapes in each category dervied
        /// from FuzzyART clustering.
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="categories"></param>
        /// <param name="categoryCount"></param>
        /// <returns></returns>
        public static ArrayList CategoryShapes(ArrayList shapes, int[] categories, int categoryCount)
        {
            if (shapes == null)     return null;
            if (categories == null) return null;
            if (categoryCount == 0) return null;

            ArrayList categoryShapes = new ArrayList();
            for (int c = 0; c < categoryCount; c++)
            {
                int r1 = 0;
                int c1 = 0;
                int r2 = 0;
                int c2 = 0;
                int count = 0;
                for (int i = 0; i < shapes.Count; i++)
                {
                    if (categories[i] != c) continue; // skip shapes not in category c
                    count++; //keep count of numbers in category c
                    Shape s = (Shape)shapes[i];
                    r1 += s.r1;
                    c1 += s.c1;
                    r2 += s.r2;
                    c2 += s.c2;
                }
                if (count == 0) continue; // no shapes assigned to this category
                r1 /= count;
                c1 /= count;
                r2 /= count;
                c2 /= count;
                Shape shape = new Shape(r1, c1, r2, c2);
                shape.category = c;

                categoryShapes.Add(shape);
            }
            return categoryShapes;
        }


        public static int[] Distribution(ArrayList shapes, int binCount)
        {
            if (shapes == null) return null;

            int binWidth = Shape.maxCol / binCount; 

            int[] distribution = new int[binCount];
            for (int i = 0; i < shapes.Count; i++)
            {
                Shape s = (Shape)shapes[i];
                int bin = s.col_Centroid() / binWidth;
                if (bin >= binCount) bin = binCount - 1;
                distribution[bin]++;
            }
            if (Shape.Verbose) WriteDistribution(distribution);
            return distribution;
        }

        public static void WriteDistribution(int[] distribution)
        {
            int binCount = distribution.Length; //number of bins
            Console.WriteLine("\nDistribution over "+binCount+" bins");
            for (int i = 0; i < binCount; i++)
            {
                Console.Write(i+"\t");
            }
            Console.WriteLine();
            for (int i = 0; i < binCount; i++)
            {
                Console.Write(distribution[i] + "\t");
            }
            Console.WriteLine("Total=" + DataTools.Sum(distribution));
        }



        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //  MAIN METHOD FOR UNIT TESTING 
        

        static void Main()
        {
            Console.WriteLine("TESTING METHODS IN CLASS DataTools");

            //string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\";

            if (false) //test1 method AREA, CENTROID and CENTROID-DISTANCE()
            {
                Console.WriteLine("Test Method Name()");
                int r1 = 10; int c1 = 10; int r2 = 20; int c2 = 20;
                Shape s1 = new Shape(r1, c1, r2, c2);
                s1.WriteBounds();
                int[] centroid1 = s1.Centroid();
                Console.WriteLine("Centroid1: r=" + centroid1[0] + "  c=" + centroid1[1]);
                Console.WriteLine("Area 1=" + s1.Area());
                Console.WriteLine();

                r1 = 17; c1 = 16; r2 = 23; c2 = 24;
                Shape s2 = new Shape(r1, c1, r2, c2);
                s2.WriteBounds();
                int[] centroid2 = s2.Centroid();
                Console.WriteLine("Centroid2: r=" + centroid2[0] + "  c=" + centroid2[1]);
                Console.WriteLine("Area 2=" + s2.Area());
                double dist = s1.CentroidDistance(s2);
                Console.WriteLine("Distance="+dist);

            } //end test1

            if (false) //test2 method IncludesRow(), IncludesColumn(), PointInside()
            {
                Console.WriteLine("Test Method Name()");
                int r1 = 10; int c1 = 10; int r2 = 20; int c2 = 20;
                Shape s1 = new Shape(r1, c1, r2, c2);
                s1.WriteBounds();
                r1 = 17; c1 = 16; r2 = 23; c2 = 24;
                Shape s2 = new Shape(r1, c1, r2, c2);
                s2.WriteBounds();
                r1 = 20; c1 = 20; r2 = 30; c2 = 30;
                Shape s3 = new Shape(r1, c1, r2, c2);
                s3.WriteBounds();

                Console.WriteLine();
                Console.WriteLine("Row10 in s1=" + s1.IncludesRow(10));
                Console.WriteLine("Row15 in s1=" + s1.IncludesRow(15));
                Console.WriteLine("Row20 in s1=" + s1.IncludesRow(20));
                Console.WriteLine("Row25 in s1=" + s1.IncludesRow(25));
                Console.WriteLine("Col05 in s1=" + s1.IncludesColumn(5));
                Console.WriteLine("Col10 in s1=" + s1.IncludesColumn(10));
                Console.WriteLine("Col15 in s1=" + s1.IncludesColumn(15));
                Console.WriteLine("Col20 in s1=" + s1.IncludesColumn(20));

                int py = 23;
                int px = 25;
                bool inside = s1.PointInside(py,px);
                Console.WriteLine("\nPoint ("+py+","+px+ ") inside s1 =" + inside);
                inside = s2.PointInside(py, px);
                Console.WriteLine("Point (" + py + "," + px + ") inside s2 =" + inside);
                inside = s3.PointInside(py, px);
                Console.WriteLine("Point (" + py + "," + px + ") inside s3 =" + inside);

                bool overlapped = s1.Overlaps(s3);
                Console.WriteLine("\ns1 and s3 overlap =" + overlapped);
                overlapped = s1.Overlaps(s2);
                Console.WriteLine("s1 and s2 overlap =" + overlapped);

            } //end test2

            if (true) //test Method MergeShapes()
            {
                Console.WriteLine("Test MergeShapes()");
                ArrayList list = new ArrayList();
                int r1 = 10; int c1 = 10; int r2 = 20; int c2 = 20;
                Shape s1 = new Shape(r1, c1, r2, c2);
                s1.WriteBounds();
                list.Add(s1);
                r1 = 17; c1 = 16; r2 = 23; c2 = 24;
                Shape s2 = new Shape(r1, c1, r2, c2);
                s2.WriteBounds();
                list.Add(s2);
                r1 = 20; c1 = 20; r2 = 30; c2 = 30;
                Shape s3 = new Shape(r1, c1, r2, c2);
                s3.WriteBounds();
                list.Add(s3);

                Console.WriteLine(" dy(s2-s1)= " + s1.RowShift(s2));
                Console.WriteLine(" dy(s3-s2)= " + s2.RowShift(s3));

                int dyThreshold = 6;
                list = MergeShapesWhoseEndsOverlap(list, dyThreshold);
                Console.WriteLine("List size="+list.Count);
                foreach (Shape s in list)
                {
                    s.WriteBounds();
                }

            
            } //end test3


            if (false) //test Method()
            {
                Console.WriteLine("Test Method Name()");
            } //end test4


            Console.WriteLine("\nFINISHED!!");
            Console.ReadLine();
        }// end Main()




    } //end class Shape 
}
