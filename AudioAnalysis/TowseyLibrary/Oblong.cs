﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace TowseyLibrary
{

    public class Oblong
    {
        /// <summary>
        /// location of Oblong's top row in parent matrix
        /// </summary>
        public int r1 { get; set; }  
        /// <summary>
        /// location of Oblong's left most column in parent matrix
        /// </summary>
        public int c1 { get; set; } 
        /// <summary>
        /// location of Oblong's bottom row in parent matrix
        /// </summary>
        public int r2 { get; set; } 
        /// <summary>
        /// location of Oblong's right most column in parent matrix
        /// </summary>
        public int c2 { get; set; } 
        /// <summary>
        /// location of Oblong's centre column in parent matrix
        /// </summary>
        public int ColCentroid { get { return c1 + (c2-c1+1)/2; } } 

        //private int rowWidth;
        //public int RowWidth { get { row2 - row1 + 1; } }
        public int RowWidth { get; private set; }
        public int ColWidth { get; private set; }
        public int RandomNumber { get; set; }
        public int category { get; set; }
        public static bool Verbose { get; set; }

        //vars required to set up Fuzzy Sets for row centroid feature
        private static int maxCol = 256; //default value
        public  static int MaxCol { get { return maxCol; } set { maxCol = value; } } 
        public  static int countColCentroid_FS = 2;  //number of fuzzy membership values over range of col centroid values 
        int featureCount = countColCentroid_FS + 2;  //centroid location + freqWidth + time duration.

        int[] colCentroid_FS; //FS = fuzzy set


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="row1"></param>
        /// <param name="col1"></param>
        /// <param name="row2"></param>
        /// <param name="col2"></param>
        public Oblong(int row1, int col1, int row2, int col2)
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
            //    LoggedConsole.WriteLine(" true");
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

        
        public bool Overlaps(Oblong s2)
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

        public static int RowOverlap(Oblong o1, Oblong o2)
        {
            if (o1.r2 < o2.r1) return 0;
            if (o2.r2 < o1.r1) return 0;

            //at this point the two events overlap
            int overlap = 0;
            if (o1.IncludesRow(o2.r1)) overlap = o1.r2 - o2.r1 + 1;
            else
            if (o2.IncludesRow(o1.r1)) overlap = o2.r2 - o1.r1 + 1;

            return overlap;
        }

        public static int ColumnOverlap(Oblong o1, Oblong o2)
        {
            if (o1.c2 < o2.c1) return 0;
            if (o2.c2 < o1.c1) return 0;

            //at this point the two events overlap
            int overlap = 0;
            if (o1.IncludesColumn(o2.c1)) overlap = o1.c2 - o2.c1 + 1;
            else
            if (o2.IncludesColumn(o1.c1)) overlap = o2.c2 - o1.c1 + 1;
            return overlap;
        }

        public int OverlapArea(Oblong s2)
        {
            bool row1Overlap = this.IncludesRow(s2.r1);
            bool row2Overlap = this.IncludesRow(s2.r2);
            bool col1Overlap = this.IncludesColumn(s2.c1);
            bool col2Overlap = this.IncludesColumn(s2.c2);
            if (row1Overlap && row2Overlap && col1Overlap && col2Overlap) return s2.Area();

            if (!row1Overlap && !row2Overlap && !col1Overlap && !col2Overlap && this.Overlaps(s2)) return this.Area();
            return 0;
        }


        public bool Encloses(Oblong s2)
        {
            bool row1Overlap = this.IncludesRow(s2.r1);
            bool row2Overlap = this.IncludesRow(s2.r2);
            bool col1Overlap = this.IncludesColumn(s2.c1);
            bool col2Overlap = this.IncludesColumn(s2.c2);
            if (row1Overlap && row2Overlap && col1Overlap && col2Overlap) return true;
            return false;
        }


        public bool AdjacentRows(Oblong s2)
        {
            //this.top-s2.bottom adjacency  OR  this.bottom-s2.top adjacency
            bool rowAdjacency = ((Math.Abs(this.r2 - s2.r1) == 1) || (Math.Abs(this.r1 - s2.r2) == 1));
            return rowAdjacency;
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

        public int RowShift(Oblong s2)
        {
            int dy = s2.row_Centroid() - this.row_Centroid();
            return dy;
        }


        public int ColumnShift(Oblong s2)
        {
            int dx = s2.col_Centroid() - this.col_Centroid();
            return dx;
        }


        public void WriteBounds()
        {
            LoggedConsole.WriteLine(" r1=" + this.r1 + " c1=" + this.c1 + " r2=" + this.r2 + " c2=" + this.c2);
        }
        public void WriteProperties()
        {
            LoggedConsole.WriteLine("Row count=" + this.RowWidth + "\tCol bandwidth=" + this.ColWidth + "\t ColCentroid=" + this.col_Centroid());
        }

        public static List<Oblong> SortByColumnCentroid(List<Oblong> list1){
            var list2 = new List<Oblong>();
            list2.AddRange(list1);
            list2.Sort(delegate(Oblong x, Oblong y) {
                return x.ColCentroid.CompareTo(y.ColCentroid);
            });
            return list2;
        }

        //***********************************************************************************************************************
        //***********************************************************************************************************************
        //***********************************************************************************************************************

        public double[] Features()
        {
            double[] features = new double[featureCount];
            double[] fuzzyMemberships = FuzzySetMemberships(this.col_Centroid());
            for (int i = 0; i < countColCentroid_FS; i++) features[0] = fuzzyMemberships[i];

            
          //  features[0] = this.col_Centroid(); //column centroid
          //  features[1] = this.col_Centroid(); //column centroid
          //  features[2] = this.col_Centroid(); //column centroid
            features[Oblong.countColCentroid_FS]     = this.ColWidth; //column width - the frequency range of oblong
            features[Oblong.countColCentroid_FS + 1] = this.RowWidth; //row width    - the time duration   of oblong
            return features;
        }
        public double[] Features_Normalised()
        {
            int maxRows = 54; //to normalise rowWidth.  27 rows/sec. ie row width is the fraction of 2 seconds
            double[] features = Features(); //get raw feature values
            //features[0] /= (double)maxCols; //column centroid
            //features[1] /= (double)maxCols; //column centroid
            //features[2] /= (double)maxCols; //column centroid
            features[countColCentroid_FS]     /= (double)Oblong.maxCol; //column width
            features[countColCentroid_FS + 1] /= (double)maxRows;       //row width

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
            //LoggedConsole.WriteLine("Array of " + countColCentroid_FS + " Fuzzy Set Centres");
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
            //LoggedConsole.WriteLine("For row "+row+" memberships are:");
            //DataTools.writeArray(FM);
            //Console.ReadLine();
            return FM;
        }

        private static double LinearInterpolate(int x1, double y1, int x2, double y2, int x3)
        {
            if((x3 < x1)||(x3 > x2)||(x1>x2))
            {
                LoggedConsole.WriteLine("ERROR with Linear Interpolation! ((x3 < x1)||(x3 > x2)||(x1>x2))!!");
                return Double.MaxValue;
            }
            double slope = (y2 - y1) / (double)(x2 - x1);
            double dy = slope * (x3 - x1);
            return (y1 + dy);
        }



        //***********************************************************************************************************************
        //***********************************************************************************************************************
        //***********************************************************************************************************************




        public static Oblong Clone(Oblong s)
        {
            return new Oblong(s.r1, s.c1, s.r2, s.c2);
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
                    Oblong shape = new Oblong(y, col1, y + rowWidth - 1, col1 + colWidth -1);
                    shape.RandomNumber = random.GetInt(200); //set random number for id and color purposes

                    int[] centroid = shape.Centroid();
                    //LoggedConsole.WriteLine("Centroid=" + centroid[0] + ", " + centroid[1]);
                    //LoggedConsole.WriteLine("RowWidth=" + shape.RowWidth + "  ColWidth=" + shape.ColWidth);
                    shapes.Add(shape);
                    //more to end of shape
                    y = shape.r2;
                }
                x += 4; //jump through 5 lines at a time.
            }

            if (Oblong.Verbose) LoggedConsole.WriteLine("Number of shapes=" + shapes.Count);
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
            foreach(Oblong shape in shapes) if(shape.PointInside(row, col)) return true;
            return false;
        }


        public static Oblong GetShape(int row, int col, ArrayList shapes)
        {
            if (shapes == null) return null;
            foreach(Oblong shape in shapes) if(shape.PointInside(row, col)) return shape;
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
            //    LoggedConsole.WriteLine("Area["+(id-1)+"]="+areas[id-1]);
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
            //LoggedConsole.WriteLine("Value AtMaxindex=" + valueAtMaxindex);

            int areaThreshold = 10;

            for(int i=shapes.Count-1; i>=0; i--)
            {    
                Oblong s = (Oblong)shapes[i];
                if (s.Area() < areaThreshold) shapes.RemoveAt(i);
            }
            return shapes;
        }


        public static ArrayList MergeCloseShapes(ArrayList shapes)
        {
            int distThreshold = 8; 

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Oblong s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Oblong s2 = (Oblong)shapes[j];
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
            if (Oblong.Verbose) LoggedConsole.WriteLine("Merge shapes whose ends overlap.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Oblong s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Oblong s2 = (Oblong)shapes[j];
                    if (!s1.Overlaps(s2)) continue;
                    double dy = s1.RowShift(s2);
                    if (Oblong.Verbose) LoggedConsole.WriteLine("dy=" + dy);
                    if (Math.Abs(dy) > dyThreshold) continue;
                    //LoggedConsole.WriteLine("dy=" + dy);

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
            if (Oblong.Verbose) LoggedConsole.WriteLine("Merge shapes whose sides are adjacent.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Oblong s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Oblong s2 = (Oblong)shapes[j];
                    if (!s1.AdjacentRows(s2)) continue; //not adjacent

                    double dx = s1.ColumnShift(s2);
                    //LoggedConsole.WriteLine("dx=" + dx);
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
                    //LoggedConsole.WriteLine("size="+shapes.Count+"  i="+i);
                    shapes.RemoveAt(i);  //remove s1
                    //shapes[j] = s2;    //keep s2
                    break; //break inner loop if get to here
                }//inner loop
            }
            return shapes;
        }

        public static ArrayList RemoveOverlappingShapes(ArrayList shapes)
        {
            if (Oblong.Verbose) LoggedConsole.WriteLine("Remove the smaller of any pair of overlapping shapes.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Oblong s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Oblong s2 = (Oblong)shapes[j];
                    if (!s1.Overlaps(s2)) continue; //not overlapping
                    if (s1.Area() < s2.Area()) shapes.RemoveAt(i);  //remove s1
                    else
                    if (s1.Area() > s2.Area())
                    {
                        shapes[j] = Oblong.Clone(s1);//copy s1 in place of s2
                        shapes.RemoveAt(i);  //remove s1
                    }
                }
            }
            return shapes;
        }

        public static ArrayList RemoveEnclosedShapes(ArrayList shapes)
        {
            if (Oblong.Verbose) LoggedConsole.WriteLine("Remove enclosed shapes.");

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Oblong s1 = (Oblong)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Oblong s2 = (Oblong)shapes[j];
                    if (s2.Encloses(s1))
                    {
                        shapes.RemoveAt(i);  //remove s1
                        continue;
                    }
                    if (s1.Encloses(s2))
                    {
                        shapes[j] = Oblong.Clone(s1);//copy s1 in place of s2
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
                Oblong s1 = (Oblong)shapes[i];
                if (s1.Area() < minArea) shapes.RemoveAt(i);
            }
            return shapes;
        }


        public static ArrayList ProcessShapes(ArrayList shapes)
        {
            shapes = RemoveSmallShapes(shapes);
            if (Oblong.Verbose) LoggedConsole.WriteLine("Number of shapes after removing small ones=" + shapes.Count);
            shapes = MergeCloseShapes(shapes);
            if (Oblong.Verbose) LoggedConsole.WriteLine("Number of shapes after merging close shapes=" + shapes.Count);
            return shapes;
        }

        public static double[,] FeatureMatrix(List<Oblong> shapes)
        {
            int count = shapes.Count;
            if (count == 0) return null;
            double[] features = shapes[0].Features();//use first shape to identify dimensions
            double[,] data = new double[count, features.Length];

            for (int i = 0; i < count; i++)
            {
                features = shapes[i].Features_Normalised();
                for (int j = 0; j < features.Length; j++) data[i, j] = features[j];
            }
            return data;
        }


        public static void WriteData2File(List<Oblong> shapes, string shapesDataFname)
        {
            double[,] data = FeatureMatrix(shapes);
            FileTools.WriteMatrix2File(data, shapesDataFname);
        }


        public static void AssignCategories(List<Oblong> shapes, int[] categories)
        {
            if (shapes == null) return;
            if (categories == null) return;

            for (int i=0; i< shapes.Count; i++)
            {
                shapes[i].category = categories[i];
            }
        }

        /// <summary>
        /// returns a list of shapes that represent the averages of shapes in each category dervied
        /// from FuzzyART clustering.
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="categories"></param>
        /// <param name="categoryCount"></param>
        /// <returns></returns>
        public static List<Oblong> CategoryShapes(List<Oblong> shapes, int[] categories, int categoryCount)
        {
            if (shapes == null)     return null;
            if (categories == null) return null;
            if (categoryCount == 0) return null;

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
                    if (categories[i] != c) continue; // skip shapes not in category c
                    count++; //keep count of numbers in category c
                    Oblong s = (Oblong)shapes[i];
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
                Oblong shape = new Oblong(r1, c1, r2, c2);
                shape.category = c;

                categoryShapes.Add(shape);
            }
            return categoryShapes;
        }

        /// <summary>
        /// Reurns the distribution of the column-centroids.
        /// The rectangular shapes are assumed to exist in a matrix 
        ///     whose rows are time frames and whose columns are freq bins.
        /// The returned distribution is therefore over freq domain.    
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="binCount"></param>
        /// <returns></returns>
        public static int[] Distribution(List<Oblong> shapes, int binCount)
        {
            if (shapes == null) return null;

            int binWidth = Oblong.maxCol / binCount; 

            int[] distribution = new int[binCount];
            for (int i = 0; i < shapes.Count; i++)
            {
                int bin = shapes[i].col_Centroid() / binWidth;
                if (bin >= binCount) bin = binCount - 1;
                distribution[bin]++;
            }
            if (Oblong.Verbose) LoggedConsole.WriteLine("Number of data columns = "+Oblong.maxCol);
            if (Oblong.Verbose) LoggedConsole.WriteLine("One bin = " + binWidth + " of the original data columns.");
            if (Oblong.Verbose) WriteDistribution(distribution, binWidth);
            return distribution;
        }

        public static void WriteDistribution(int[] distribution, double binWidth)
        {
            int binCount = distribution.Length; //number of bins
            LoggedConsole.WriteLine("\nDistribution over "+binCount+" bins");
            for (int i = 0; i < binCount; i++)
            {
                LoggedConsole.Write(i+"\t");
            }
            LoggedConsole.WriteLine();
            for (int i = 0; i < binCount; i++)
            {
                LoggedConsole.Write((int)(i*binWidth) + "\t");
            }
            LoggedConsole.WriteLine("(Total " + (int)(binWidth * binCount) + " bins)");
            for (int i = 0; i < binCount; i++)
            {
                LoggedConsole.Write(distribution[i] + "\t");
            }
            LoggedConsole.WriteLine("Total instances=" + DataTools.Sum(distribution));
        }


        public static void SaveImageOfCentroids(double[,] matrix, List<Oblong> shapes, Color colour, string opPath)
        {
            if (shapes == null) return;
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number
            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            
            foreach (Oblong shape in shapes)
            {
                //r1 += 10;
                //int c1 = shape.c1;
                //int r2 = r1 + shape.RowWidth;
                //int c2 = shape.c2;
                //int colorCount = ImageTools.darkColors.Length;
                //int colorID = shape.category % colorCount;
                //if (shape.category == -1) shapeColor = col;
                //else shapeColor = ImageTools.darkColors[colorID];

                //TransformCoordinates(r1, c1, r2, c2, out x1, out y1, out x2, out y2, mWidth);
                //for (int r = shape.r1; r <= shape.r2; r++) 
                //    for (int c = shape.c1; c <= shape.c2; c++)
                //     bmp.SetPixel(c, r, colour);
                for (int r = shape.r1; r <= shape.r2; r++) bmp.SetPixel(shape.c1, r, colour);
                for (int r = shape.r1; r <= shape.r2; r++) bmp.SetPixel(shape.c2, r, colour);
                for (int c = shape.c1; c <= shape.c2; c++) bmp.SetPixel(c, shape.r1, colour);
                for (int c = shape.c1; c <= shape.c2; c++) bmp.SetPixel(c, shape.r2, colour);
                //        bmp.SetPixel(c, r, colour);
                //r1 += shape.RowWidth;
            }

            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            bmp.Save(opPath);
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
            LoggedConsole.WriteLine("TESTING METHODS IN CLASS DataTools");

            //string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\";
            Boolean doit1 = false;
            if (doit1) //test1 method AREA, CENTROID and CENTROID-DISTANCE()
            {
                LoggedConsole.WriteLine("Test Method Name()");
                int r1 = 10; int c1 = 10; int r2 = 20; int c2 = 20;
                Oblong s1 = new Oblong(r1, c1, r2, c2);
                s1.WriteBounds();
                int[] centroid1 = s1.Centroid();
                LoggedConsole.WriteLine("Centroid1: r=" + centroid1[0] + "  c=" + centroid1[1]);
                LoggedConsole.WriteLine("Area 1=" + s1.Area());
                LoggedConsole.WriteLine();

                r1 = 17; c1 = 16; r2 = 23; c2 = 24;
                Oblong s2 = new Oblong(r1, c1, r2, c2);
                s2.WriteBounds();
                int[] centroid2 = s2.Centroid();
                LoggedConsole.WriteLine("Centroid2: r=" + centroid2[0] + "  c=" + centroid2[1]);
                LoggedConsole.WriteLine("Area 2=" + s2.Area());
                double dist = s1.CentroidDistance(s2);
                LoggedConsole.WriteLine("Distance="+dist);

            } //end test1

            Boolean doit2 = false;
            if (doit2) //test2 method IncludesRow(), IncludesColumn(), PointInside()
            {
                LoggedConsole.WriteLine("Test Method Name()");
                int r1 = 10; int c1 = 10; int r2 = 20; int c2 = 20;
                Oblong s1 = new Oblong(r1, c1, r2, c2);
                s1.WriteBounds();
                r1 = 17; c1 = 16; r2 = 23; c2 = 24;
                Oblong s2 = new Oblong(r1, c1, r2, c2);
                s2.WriteBounds();
                r1 = 20; c1 = 20; r2 = 30; c2 = 30;
                Oblong s3 = new Oblong(r1, c1, r2, c2);
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
                bool inside = s1.PointInside(py,px);
                LoggedConsole.WriteLine("\nPoint ("+py+","+px+ ") inside s1 =" + inside);
                inside = s2.PointInside(py, px);
                LoggedConsole.WriteLine("Point (" + py + "," + px + ") inside s2 =" + inside);
                inside = s3.PointInside(py, px);
                LoggedConsole.WriteLine("Point (" + py + "," + px + ") inside s3 =" + inside);

                bool overlapped = s1.Overlaps(s3);
                LoggedConsole.WriteLine("\ns1 and s3 overlap =" + overlapped);
                overlapped = s1.Overlaps(s2);
                LoggedConsole.WriteLine("s1 and s2 overlap =" + overlapped);

            } //end test2

            if (true) //test Method MergeShapes()
            {
                LoggedConsole.WriteLine("Test MergeShapes()");
                ArrayList list = new ArrayList();
                int r1 = 10; int c1 = 10; int r2 = 20; int c2 = 20;
                Oblong s1 = new Oblong(r1, c1, r2, c2);
                s1.WriteBounds();
                list.Add(s1);
                r1 = 17; c1 = 16; r2 = 23; c2 = 24;
                Oblong s2 = new Oblong(r1, c1, r2, c2);
                s2.WriteBounds();
                list.Add(s2);
                r1 = 20; c1 = 20; r2 = 30; c2 = 30;
                Oblong s3 = new Oblong(r1, c1, r2, c2);
                s3.WriteBounds();
                list.Add(s3);

                LoggedConsole.WriteLine(" dy(s2-s1)= " + s1.RowShift(s2));
                LoggedConsole.WriteLine(" dy(s3-s2)= " + s2.RowShift(s3));

                int dyThreshold = 6;
                list = MergeShapesWhoseEndsOverlap(list, dyThreshold);
                LoggedConsole.WriteLine("List size="+list.Count);
                foreach (Oblong s in list)
                {
                    s.WriteBounds();
                }

            
            } //end test3


            //if (false) //test Method()
            //{
            //    LoggedConsole.WriteLine("Test Method Name()");
            //} //end test4


            LoggedConsole.WriteLine("\nFINISHED!!");
            Console.ReadLine();
        }// end Main()




    } //end class Shape 
}
