using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TowseyLib
{
    public class FileTools
    {
        private static string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\"; 
        
        
        static void Main()
        {
            Console.WriteLine("TESTING METHODS IN CLASS FileTools\n\n");


            if (false) //test ReadTextFile(string fName)
            {
                string fName = testDir + "testTextFile.txt";
                ArrayList array = ReadTextFile(fName);
                foreach(string line in array)
                {    Console.WriteLine(line);
                }
            }//end test ReadTextFile(string fName)

            if (false) //test WriteTextFile(string fName)
            {
                string fName = testDir + "testOfWritingATextFile.txt";
                ArrayList array = new ArrayList();
                array.Add("string1");
                array.Add("string2");
                array.Add("string3");
                array.Add("string4");
                array.Add("string5");
                WriteTextFile(fName, array);
            }//end test WriteTextFile(string fName)

            
            if (false) //test ReadDoubles2Matrix(string fName)
            {
                string fName = testDir + "testOfReadingMatrixFile.txt";
                double[,] matrix = ReadDoubles2Matrix(fName);
                int rowCount = matrix.GetLength(0);//height
                int colCount = matrix.GetLength(1);//width
                //Console.WriteLine("rowCount=" + rowCount + "  colCount=" + colCount);
                DataTools.writeMatrix(matrix);
            }//end test ReadDoubles2Matrix(string fName)

            if (true) //test Method(parameters)
            {
                string fName = testDir + "testWriteOfMatrix2File.txt";
                double[,] matrix = { {0.1,0.2,0.3,0.4,0.5,0.6},
                    {0.5,0.6,0.7,0.8,0.9,1.0},
                    {0.9,1.0,1.1,1.2,1.3,1.4}
                };
                WriteMatrix2File(matrix, fName);
                Console.WriteLine("Wrote following matrix to file " + fName);
                DataTools.writeMatrix(matrix);
            }//end test Method(string fName)
            


            //COPY THIS TEST TEMPLATE
            if (false) //test Method(parameters)
            {
            }//end test Method(string fName)



            Console.WriteLine("\nFINISHED"); //end
            Console.WriteLine("CLOSE CONSOLE"); //end

        } //end MAIN


        public static FileInfo[] GetFilesInDirectory(string dirPath)
        {
            DirectoryInfo d = new DirectoryInfo(dirPath);
            FileInfo[] files = d.GetFiles(); //gets all files
            return files;
        }

        public static FileInfo[] GetFilesInDirectory(string dirPath, string ext)
        {
            DirectoryInfo d = new DirectoryInfo(dirPath);
            FileInfo[] files = d.GetFiles("*" + ext); //gets all files with required extention
            return files;
        }

        public static string GetFileExt(string path)
        {
            FileInfo f = new FileInfo(path);
            return f.Extension;
        }

        public static string[] SplitFileName(string path)
        {
            FileInfo f = new FileInfo(path);
            string dir = f.DirectoryName;
            string stem = f.Name;
            string ext = f.Extension;
            string[] split = new string[3];
            int nameLength = stem.Length - ext.Length;
            split[0] = dir+@"\";
            split[1] = stem.Substring(0,nameLength);
            split[2] = ext;
            //Console.WriteLine("SPLIT FILE NAME = " + dir + "   " + split[1] + "    " + ext);
            return split;
        }

        public static string ChangeFileExtention(string path, string newExt)
        {
            string[] split = SplitFileName(path);
            string newName = split[0] + split[1] + newExt;
            //Console.WriteLine("NEW NAME = " + newName);
            return newName;
        }

        public static ArrayList ReadTextFile(string fName)
        {
            ArrayList lines = new ArrayList();
            using (TextReader reader = new StreamReader(fName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //read one line at a time in string array
                    lines.Add(line);
                }//end while
            }//end using
            return lines;
        }// end ReadtextFile()


        public static void WriteTextFile(string fName, ArrayList array)
        {
            int count = array.Count;
            using (TextWriter writer = new StreamWriter(fName))
            {
                foreach (string line in array)
                {
                    writer.WriteLine(line);
                }
            }//end using
        }// end WriteTextFile()


        public static void Append2TextFile(string fPath, string line)
        {
            ArrayList list;
            if (File.Exists(fPath)) list = ReadTextFile(fPath);
            else                    list = new ArrayList();
            list.Add(line);
            WriteTextFile(fPath, list);
        }// end Append2TextFile()

        public static void Append2TextFile(string fPath, ArrayList list)
        {
            ArrayList oldList = new ArrayList();
            if (File.Exists(fPath)) oldList = ReadTextFile(fPath);
            oldList.Add(list);
            WriteTextFile(fPath, list);
        }// end Append2TextFile()

        /// <summary>
        /// reads a file of doubles assuming one value per line with no punctuation
        /// </summary>
        /// <param name="fName"></param>
        /// <returns></returns>
        public static double[] ReadDoubles2Vector(string fName)
        {
            ArrayList lines = ReadTextFile(fName);
            int count = lines.Count;

            double[] V = new double[count];
            for (int i = 0; i < count; i++)
            {
                double value = Double.Parse((string)lines[i]);
                //Console.WriteLine("i=" + i + lines[i] + " " + value);
                V[i] = value;
            }

            return V;
        }

        public static double[,] ReadDoubles2Matrix(string fName)
        {
            ArrayList lines = ReadTextFile(fName);
            string line = (string)lines[0];
            String[] words = line.Split(',');
            int rowCount = lines.Count;
            int colCount = words.Length;

            double[,] matrix = new double[rowCount,colCount];
            for(int i=0; i< rowCount; i++)
            {
                line = (string)lines[i];
                words = line.Split(',');
                for (int j = 0; j < colCount; j++)
                {
                    double value = Double.Parse(words[j]);
                    //Console.WriteLine("i,j=" + i + "," + j + " " + words[j] + " " + value);
                    matrix[i, j] = value;
                }
            }

            return matrix;
        }

        public static void WriteMatrix2File(double[,] matrix, string fName)
        {
            int rowCount = matrix.GetLength(0);//height
            int colCount = matrix.GetLength(1);//width

            ArrayList lines = new ArrayList();

            for (int i = 0; i < rowCount; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    sb.Append(matrix[i, j]);
                    if (j < colCount - 1) sb.Append(",");
                }
                lines.Add(sb.ToString());
            }//end of all rows
            WriteTextFile(fName, lines); //write matrix to file

        } //end of WriteMatrix2File\

        public static void WriteMatrix2File_Formatted(double[,] matrix, string fName, string formatString)
        {
            int rowCount = matrix.GetLength(0);//height
            int colCount = matrix.GetLength(1);//width

            ArrayList lines = new ArrayList();

            for (int i = 0; i < rowCount; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    sb.Append(matrix[i, j].ToString(formatString));
                    if (j < colCount - 1) sb.Append(",");
                }
                lines.Add(sb.ToString());
            }//end of all rows
            WriteTextFile(fName, lines); //write matrix to file

        } //end of WriteMatrix2File

        public static void WriteArray2File_Formatted(double[] array, string fName, string formatString)
        {
            int length = array.Length;

            ArrayList lines = new ArrayList();
            for (int i = 0; i < length; i++)
            {
                string line = array[i].ToString(formatString);
                lines.Add(line);
            }//end of all rows
            WriteTextFile(fName, lines); //write matrix to file

        } //end of WriteArray2File_Formatted

        public static void WriteArray2File_Formatted(int[] array, string fName, string formatString)
        {
            int length = array.Length;

            ArrayList lines = new ArrayList();
            for (int i = 0; i < length; i++)
            {
                string line = array[i].ToString(formatString);
                lines.Add(line);
            }//end of all rows
            WriteTextFile(fName, lines); //write matrix to file

        } //end of WriteArray2File_Formatted





        public static Hashtable ReadPropertiesFile(string fName)
        {
            Hashtable table = new Hashtable();
            using (TextReader reader = new StreamReader(fName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //read one line at a time and process
                    string trimmed = line.Trim();
                    if (trimmed == null) continue;
                    if (trimmed.StartsWith("#")) continue;
                    string[] words = trimmed.Split('=');
                    if (words.Length == 1) continue;
                    string key = words[0];
                    string value = words[1];
                    table.Add(key, value);
                }//end while
            }//end using
            return table;
        }// end ReadPropertiesFile()


    }// end class
}
