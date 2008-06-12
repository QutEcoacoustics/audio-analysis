using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{



    public class Shape
    {
        public int RowWidth { get; set; }
        public int ColWidth { get; set; }
        public int RandomNumber { get; set; }

        public int r1 { get; set; }  //location of top left corner in parent matrix
        public int c1 { get; set; }  //location of top left corner in parent matrix
        public int r2 { get; set; }  //location of bot right corner in parent matrix
        public int c2 { get; set; }  //location of bot right corner in parent matrix

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

        public void WriteBounds()
        {
            Console.WriteLine(" r1=" + this.r1 + " c1=" + this.c1 + " r2=" + this.r2 + " c2=" + this.c2);
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
                    shape.RandomNumber = random.getInt(200); //set random number for id and color purposes

                    int[] centroid = shape.Centroid();
                    //Console.WriteLine("Centroid=" + centroid[0] + ", " + centroid[1]);
                    //Console.WriteLine("RowWidth=" + shape.RowWidth + "  ColWidth=" + shape.ColWidth);
                    shapes.Add(shape);
                    //more to end of shape
                    y = shape.r2;
                }
                x += 4; //jump through 5 lines at a time.
            }

            Console.WriteLine("Number of shapes="+shapes.Count);
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

        public static ArrayList MergeShapes(ArrayList shapes, int dyThreshold)
        {
            // merge shapes whose ends overlap

            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                Shape s1 = (Shape)shapes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    Shape s2 = (Shape)shapes[j];
                    if (!s1.Overlaps(s2)) continue;
                    double dy = s1.RowShift(s2);
                    Console.WriteLine("dy=" + dy);
                    if (Math.Abs(dy) > dyThreshold) continue;
                    //Console.WriteLine("dy=" + dy);

                    s2.r1 = (s1.r1 + s2.r1) / 2;
                    s2.r2 = (s1.r2 + s2.r2) / 2;
                    if (dy > 0) //s2 to right of s1
                    {
                        s2.c1 = s1.c1;
                    }
                    else
                    {
                        s2.c2 = s1.c2;
                    }
                    shapes.RemoveAt(i);
                    //shapes[j] = s2;
                }
            }
            return shapes;
        }

        public static ArrayList ProcessShapes(ArrayList shapes)
        {
            shapes = RemoveSmallShapes(shapes);
            Console.WriteLine("Number of shapes after removing small ones=" + shapes.Count);
            shapes = MergeCloseShapes(shapes);
            Console.WriteLine("Number of shapes after merging close shapes=" + shapes.Count);
            return shapes;
        }




        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //****************************************************************************************************************
        //  MAIN METHOD FOR UNIT TESTING 
        
        private static string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\";

        static void Main()
        {
            Console.WriteLine("TESTING METHODS IN CLASS DataTools");


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
                list = MergeShapes(list, dyThreshold);
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
