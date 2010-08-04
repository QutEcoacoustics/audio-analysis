using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace TowseyLib
{
    public static class FileTools
    {
        private static string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\";

        static void Main()
        {
            Log.WriteLine("TESTING METHODS IN CLASS FileTools\n\n");

            bool doit1 = false;
            if (doit1) //test ReadTextFile(string fName)
            {
                string fName = testDir + "testTextFile.txt";
                var array = ReadTextFile(fName);
                foreach (string line in array)
                    Console.WriteLine(line);
            }//end test ReadTextFile(string fName)

            bool doit2 = false;
            if (doit2) //test WriteTextFile(string fName)
            {
                string fName = testDir + "testOfWritingATextFile.txt";
                var array = new List<string>();
                array.Add("string1");
                array.Add("string2");
                array.Add("string3");
                array.Add("string4");
                array.Add("string5");
                WriteTextFile(fName, array);
            }//end test WriteTextFile(string fName)

            bool doit3 = false;
            if (doit3) //test ReadDoubles2Matrix(string fName)
            {
                string fName = testDir + "testOfReadingMatrixFile.txt";
                double[,] matrix = ReadDoubles2Matrix(fName);
                int rowCount = matrix.GetLength(0);//height
                int colCount = matrix.GetLength(1);//width
                //Console.WriteLine("rowCount=" + rowCount + "  colCount=" + colCount);
                DataTools.writeMatrix(matrix);
            }//end test ReadDoubles2Matrix(string fName)

            bool doit4 = true;
            if (doit4) //test Method(parameters)
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
            bool doit5 = false;
            if (doit5) //test Method(parameters)
            {
            }//end test Method(string fName)

            Log.WriteLine("\nFINISHED"); //end
            Log.WriteLine("CLOSE CONSOLE"); //end
        } //end MAIN

        public static bool BackupFile(string path)
        {
            Log.WriteLine("COPYING FILE:- " + path);
            try
            {
                string[] split = SplitFileName(path);
                string newPath = split[0] + "copy_of_" + split[1] + split[2];
                FileInfo fi = new FileInfo(path);
                fi.CopyTo(newPath, true); //overwrite = true
                Log.WriteLine("FILE COPIED TO:- " + newPath);
                return true;
            }
            catch { return false; }
        }

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

        public static string[] SplitFileName(string path)
        {
            FileInfo f = new FileInfo(path);
            string dir = f.DirectoryName;
            string stem = f.Name;
            string ext = f.Extension;
            string[] split = new string[3];
            int nameLength = stem.Length - ext.Length;
            split[0] = dir + @"\";
            split[1] = stem.Substring(0, nameLength);
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

        public static List<string> ReadTextFile(string fName)
        {
            var lines = new List<string>();
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

        public static byte[] ReadSerialisedObject(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(path).Length;
            return br.ReadBytes((int)numBytes);
        }// end ReadSerialisedObject()


        public static void WriteSerialisedObject(string path, byte[] array)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(array);
                bw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("ERROR! WriteTextFile(string path, byte[] array) failed to write array.");
            }
        }

        public static void WriteTextFile(string path, List<string> array)
        {
            if (File.Exists(path)) File.Copy(path, path + "OLD.txt", true); //overwrite

            int count = array.Count;
            using (TextWriter writer = new StreamWriter(path))
                foreach (string line in array)
                    writer.WriteLine(line);
        }// end WriteTextFile()

        public static void WriteTextFile(string path, List<string> array, bool saveExistingFile)
        {
            if ((File.Exists(path)) && (saveExistingFile)) File.Copy(path, path + "OLD.txt", true); //overwrite

            int count = array.Count;
            using (TextWriter writer = new StreamWriter(path))
                foreach (string line in array)
                    writer.WriteLine(line);
        }// end WriteTextFile()

        //public static void WriteTextFile(string path, string line)
        //{
        //    if (File.Exists(path)) File.Copy(path, path + "OLD.txt", true); //overwrite
        //    using (TextWriter writer = new StreamWriter(path))
        //    {
        //            writer.WriteLine(line);
        //    }//end using
        //}// end WriteTextFile()


        public static void WriteTextFile(string path, string text)
        {
            if (File.Exists(path)) File.Copy(path, path + "OLD.txt", true); //overwrite
            StreamWriter wltWriter = null;
            try
            {
                wltWriter = File.CreateText(path);
                wltWriter.WriteLine(text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            finally
            {
                if (wltWriter != null)
                {
                    wltWriter.Flush();
                    wltWriter.Close();
                }

            }// end finally
        }

        public static void Append2TextFile(string fPath, string line)
        {
            bool saveExistingFile = false;
            Append2TextFile(fPath, line, saveExistingFile);
        }// end Append2TextFile()

        public static void Append2TextFile(string fPath, string line, bool saveExistingFile)
        {
            var list = File.Exists(fPath) ? ReadTextFile(fPath) : new List<string>();
            list.Add(line);
            WriteTextFile(fPath, list, saveExistingFile);
        }// end Append2TextFile()

        public static void Append2TextFile(string fPath, List<string> list)
        {
            var oldList = File.Exists(fPath) ? ReadTextFile(fPath) : new List<string>();
            oldList.AddRange(list);
            WriteTextFile(fPath, list);
        }// end Append2TextFile()

        /// <summary>
        /// reads a file of doubles assuming one value per line with no punctuation
        /// </summary>
        public static double[] ReadDoubles2Vector(string fName)
        {
            var lines = ReadTextFile(fName);
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

        /// <summary>
        /// reads a text file of doubles formatted in rows and columns 
        /// </summary>
        /// <param name="fName"></param>
        /// <returns></returns>
        public static double[,] ReadDoubles2Matrix(string fName)
        {
            var lines = ReadTextFile(fName);
            string line = (string)lines[0];
            String[] words = line.Split(',');
            int rowCount = lines.Count;
            int colCount = words.Length;

            double[,] matrix = new double[rowCount, colCount];
            for (int i = 0; i < rowCount; i++)
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

            var lines = new List<string>();

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


        public static void WriteMatrix2File(char[,] matrix, string fName)
        {
            int rowCount = matrix.GetLength(0);//height
            int colCount = matrix.GetLength(1);//width

            var lines = new List<string>();

            for (int i = 0; i < rowCount; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    sb.Append(matrix[i, j]);
                }
                lines.Add(sb.ToString());
            }//end of all rows
            WriteTextFile(fName, lines); //write matrix to file

        } //end of WriteMatrix2File\



        public static void WriteMatrix2File_Formatted(double[,] matrix, string fName, string formatString)
        {
            int rowCount = matrix.GetLength(0);//height
            int colCount = matrix.GetLength(1);//width

            var lines = new List<string>();

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

        public static void WriteArray2File_Formatted(double[] array, string path, string formatString)
        {
            int length = array.Length;

            var lines = new List<string>();
            for (int i = 0; i < length; i++)
            {
                string line = array[i].ToString(formatString);
                lines.Add(line);
            }//end of all rows
            WriteTextFile(path, lines); //write matrix to file

        } //end of WriteArray2File_Formatted

        public static void WriteArray2File_Formatted(int[] array, string path, string formatString)
        {
            int length = array.Length;

            var lines = new List<string>();
            for (int i = 0; i < length; i++)
            {
                string line = array[i].ToString(formatString);
                lines.Add(line);
            }//end of all rows
            WriteTextFile(path, lines); //write matrix to file

        } //end of WriteArray2File_Formatted

        public static string ReadPropertyFromFile(string fName, string key)
        {
            Dictionary<string, string> dict = ReadPropertiesFile(fName);
            string value;
            dict.TryGetValue(key, out value);
            return value;
        }

        public static Dictionary<string, string> ReadPropertiesFile(string fName)
        {
            var table = new Dictionary<string, string>();
            using (TextReader reader = new StreamReader(fName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // read one line at a time and process
                    string trimmed = line.Trim();
                    if (trimmed == null)
                    {
                        continue;
                    }

                    if (trimmed.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] words = trimmed.Split('=');
                    if (words.Length == 1)
                    {
                        continue;
                    }

                    string key = words[0].Trim(); // trim because may have spaces around the = sign i.e. ' = '
                    string value = words[1].Trim();
                    if (!table.ContainsKey(key))
                    {
                        table.Add(key, value); // this may not be a good idea!
                    }
                } // end while
            } // end using
            return table;
        } // end ReadPropertiesFile()


        public static string PathCombine(params string[] paths)
        {
            return paths.Aggregate("", (s1, s2) => Path.Combine(s1, s2));
        }

        public static string UrlCombine(params string[] segments)
        {
            return segments.Aggregate("", (a, b) =>
            {
                if (string.IsNullOrEmpty(a))
                    return b;
                else
                {
                    if (a.EndsWith("\\") || a.EndsWith("/"))
                        a = a.Substring(0, a.Length - 1);
                    if (b.StartsWith("\\") || b.StartsWith("/"))
                        b = b.Substring(1);
                    return a + "/" + b;
                }
            });
        } // end of UrlCombine(params string[] segments)




    }// end class
}