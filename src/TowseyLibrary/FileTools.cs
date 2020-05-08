// <copyright file="FileTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public static class FileTools
    {
        private static readonly string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\";

        private static void Main()
        {
            throw new NotSupportedException("THIS WILL FAIL IN PRODUCTION");
            Log.WriteLine("TESTING METHODS IN CLASS FileTools\n\n");

            bool doit1 = false;
            if (doit1) //test ReadTextFile(string fName)
            {
                string fName = testDir + "testTextFile.txt";
                var array = ReadTextFile(fName);
                foreach (string line in array)
                {
                    LoggedConsole.WriteLine(line);
                }
            }

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
            }

            bool doit3 = false;
            if (doit3) //test ReadDoubles2Matrix(string fName)
            {
                string fName = testDir + "testOfReadingMatrixFile.txt";
                double[,] matrix = ReadDoubles2Matrix(fName);
                int rowCount = matrix.GetLength(0); //height
                int colCount = matrix.GetLength(1); //width

                //LoggedConsole.WriteLine("rowCount=" + rowCount + "  colCount=" + colCount);
                DataTools.writeMatrix(matrix);
            }

            bool doit4 = true;
            if (doit4) //test Method(parameters)
            {
                string fName = testDir + "testWriteOfMatrix2File.txt";
                double[,] matrix =
                {
                {
                    0.1, 0.2, 0.3, 0.4, 0.5, 0.6,
                },
                {
                        0.5, 0.6, 0.7, 0.8, 0.9, 1.0,
                },
                {
                        0.9, 1.0, 1.1, 1.2, 1.3, 1.4,
                },
                };
                WriteMatrix2File(matrix, fName);
                LoggedConsole.WriteLine("Wrote following matrix to file " + fName);
                DataTools.writeMatrix(matrix);
            }

            //COPY THIS TEST TEMPLATE
            bool doit5 = false;
            if (doit5) //test Method(parameters)
            {
            }

            Log.WriteLine("\nFINISHED"); //end
            Log.WriteLine("CLOSE CONSOLE"); //end
        } //end MAIN

        public static string TimeStamp2FileName(DateTime datetime)
        {
            string name = $"{datetime.Year}{datetime.Month:D2}{datetime.Day:D2}_{datetime.Hour:D2}{datetime.Minute:D2}";
            return name;
        }

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
            catch
            {
                return false;
            }
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

            //LoggedConsole.WriteLine("SPLIT FILE NAME = " + dir + "   " + split[1] + "    " + ext);
            return split;
        }

        public static string ChangeFileExtention(string path, string newExt)
        {
            string[] split = SplitFileName(path);
            string newName = split[0] + split[1] + newExt;

            //LoggedConsole.WriteLine("NEW NAME = " + newName);
            return newName;
        }

        public static string AppendToFileName(string ipPath, string appendix)
        {
            string dir = Path.GetDirectoryName(ipPath);
            string fn = Path.GetFileNameWithoutExtension(ipPath);
            string fext = Path.GetExtension(ipPath);
            string opPath = dir + @"\" + fn + appendix + fext;
            return opPath;
        }

        public static int CountLinesOfTextFile(string fName)
        {
            var lineCount = 0;
            using (var reader = File.OpenText(@"C:\file.txt"))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }

            return lineCount;
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
                }
            }

            return lines;
        }

        public static List<string> ReadSelectedLinesOfCsvFile(string fName, string key, int value)
        {
            var lines = new List<string>();
            using (TextReader reader = new StreamReader(fName))
            {
                //read header line
                string line = reader.ReadLine();
                string[] array = line.Split(',');

                // determine which CSV column contains the key
                int columnID = -1;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Equals(key))
                    {
                        columnID = i;
                        break;
                    }
                }

                // the key was not found
                if (columnID == -1)
                {
                    LoggedConsole.WriteErrorLine("THE KEY <" + key + "> WAS NOT FOUND IN FILE <" + fName + ">");
                    return null;
                }

                while ((line = reader.ReadLine()) != null)
                {
                    //read one line at a time in string array
                    array = line.Split(',');
                    if (int.Parse(array[columnID]) == value)
                    {
                        lines.Add(line);
                    }
                }
            }

            return lines;
        }

        public static byte[] ReadSerialisedObject(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(path).Length;
            return br.ReadBytes((int)numBytes);
        }

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
                LoggedConsole.WriteLine(ex.Message);
                LoggedConsole.WriteLine("ERROR! WriteTextFile(string path, byte[] array) failed to write array.");
            }
        }

        public static void WriteTextFile(string path, IEnumerable<string> array, bool saveExistingFile = true)
        {
            WriteTextFile(path.ToFileInfo(), array, saveExistingFile);
        }

        public static void WriteTextFile(FileInfo path, IEnumerable<string> array, bool saveExistingFile = true)
        {
            if (path == null)
            {
                throw new ArgumentException();
            }

            if (path.Exists && saveExistingFile)
            {
                File.Copy(path.FullName, path + "OLD.txt", true); //overwrite
            }

            //            int count = array.Count;
            //            using (TextWriter writer = new StreamWriter(path))
            //                foreach (string line in array)
            //                    writer.WriteLine(line);

            File.WriteAllLines(path.FullName, array);
        }

        //public static void WriteTextFile(string path, string line)
        //{
        //    if (File.Exists(path)) File.Copy(path, path + "OLD.txt", true); //overwrite
        //    using (TextWriter writer = new StreamWriter(path))
        //    {
        //            writer.WriteLine(line);
        //    }
        //}

        public static void WriteTextFile(string path, string text)
        {
            if (File.Exists(path))
            {
                File.Copy(path, path + "OLD.txt", true); //overwrite
            }

            StreamWriter wltWriter = null;
            try
            {
                wltWriter = File.CreateText(path);
                wltWriter.WriteLine(text);
            }
            catch (Exception e)
            {
                LoggedConsole.WriteLine(e);
                throw e;
            }
            finally
            {
                if (wltWriter != null)
                {
                    wltWriter.Flush();
                    wltWriter.Close();
                }
            }
        }

        public static void WriteDictionaryToFile(Dictionary<string, double[]> dictionary, string fPath)
        {
            var text = new StringBuilder();
            foreach (KeyValuePair<string, double[]> kvp in dictionary)
            {
                text.Append(kvp);
                var vector = kvp.Value;
                for (int i = 0; i < vector.Length; i++)
                {
                    text.Append($", {vector[i]:F3}");
                }

                text.Append("\n");
            }

            WriteTextFile(fPath, text.ToString());
        }

        /// <summary>
        /// Write a dictionary of arrays as a csv file where dictionary keys are column headers and the
        /// arrays are the column entries.
        /// WARNING: Assume that all arrays are of the same size!.
        /// </summary>
        /// <param name="dictionary">a dictionary of arrays of double.</param>
        /// <param name="fPath">The file path.</param>
        public static void WriteDictionaryAsCsvFile(Dictionary<string, double[]> dictionary, string fPath)
        {
            var kvp1 = dictionary.First();
            var arrayLength = kvp1.Value.Length;

            // set up an array of lines with first line for headers
            var lines = new string[arrayLength + 1];

            // now build up the lines
            foreach (KeyValuePair<string, double[]> kvp in dictionary)
            {
                // add in the header
                lines[0] += kvp.Key + ",";

                // now add in values
                var array = kvp.Value;
                for (int i = 0; i < array.Length; i++)
                {
                    lines[i + 1] += $"{array[i]:F3}, ";
                }
            }

            WriteTextFile(fPath, lines);
        }

        public static void Append2TextFile(string fPath, string line)
        {
            bool saveExistingFile = false;
            Append2TextFile(fPath, line, saveExistingFile);
        }

        public static void Append2TextFile(string fPath, string line, bool saveExistingFile)
        {
            var list = File.Exists(fPath) ? ReadTextFile(fPath) : new List<string>();
            list.Add(line);
            WriteTextFile(fPath, list, saveExistingFile);
        }

        public static void Append2TextFile(string fPath, List<string> list)
        {
            var oldList = File.Exists(fPath) ? ReadTextFile(fPath) : new List<string>();
            oldList.AddRange(list);
            WriteTextFile(fPath, oldList);
        }

        /// <summary>
        /// THis method adds another column to an existing .csv file containing columns of data.
        /// It assumes that the number of elements in the list are same as rows in the existing file.
        /// </summary>
        public static void AddArrayAdjacentToExistingArrays(string fPath, double[] array)
        {
            var oldList = File.Exists(fPath) ? ReadTextFile(fPath) : new List<string>();
            var newList = new List<string>();
            int count = oldList.Count;

            if (count == 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    newList.Add($"{array[i]}");
                }

                WriteTextFile(fPath, newList);
                return;
            }

            string separator = ",";
            for (int i = 0; i < count; i++)
            {
                string line = oldList[i] + separator + array[i];
                newList.Add(line);
            }

            WriteTextFile(fPath, newList);
        }

        /// <summary>
        /// reads a file of doubles assuming one value per line with no punctuation.
        /// </summary>
        public static double[] ReadDoubles2Vector(string fName)
        {
            var lines = ReadTextFile(fName);
            int count = lines.Count;

            double[] V = new double[count];
            for (int i = 0; i < count; i++)
            {
                double value = double.Parse(lines[i]);

                //LoggedConsole.WriteLine("i=" + i + lines[i] + " " + value);
                V[i] = value;
            }

            return V;
        }

        /// <summary>
        /// reads a text file of doubles formatted in rows and columns.
        /// </summary>
        public static double[,] ReadDoubles2Matrix(string fName)
        {
            var lines = ReadTextFile(fName);
            string line = lines[0];
            string[] words = line.Split(',');
            int rowCount = lines.Count;
            int colCount = words.Length;

            double[,] matrix = new double[rowCount, colCount];
            for (int i = 0; i < rowCount; i++)
            {
                line = lines[i];
                words = line.Split(',');
                for (int j = 0; j < colCount; j++)
                {
                    double value = double.Parse(words[j]);

                    //LoggedConsole.WriteLine("i,j=" + i + "," + j + " " + words[j] + " " + value);
                    matrix[i, j] = value;
                }
            }

            return matrix;
        }

        public static void WriteArray2File(double[] array, string fName)
        {
            var lines = new List<string>();

            for (int i = 0; i < array.Length; i++)
            {
                lines.Add(array[i].ToString());
            }

            WriteTextFile(fName, lines); //write to file
        } //end of WriteArray2File

        public static void WriteArray2File(int[] array, bool addLineNumbers, string fName)
        {
            var lines = new List<string>();

            for (int i = 0; i < array.Length; i++)
            {
                string line = array[i].ToString();
                if (addLineNumbers)
                {
                    line = i + 1 + "," + line;
                }

                lines.Add(line);
            }

            WriteTextFile(fName, lines); //write to file
        } //end of WriteArray2File

        public static void WriteMatrix2File(double[,] matrix, string fName)
        {
            int rowCount = matrix.GetLength(0); //height
            int colCount = matrix.GetLength(1); //width

            var lines = new List<string>();

            for (int i = 0; i < rowCount; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    sb.Append(matrix[i, j]);
                    if (j < colCount - 1)
                    {
                        sb.Append(",");
                    }
                }

                lines.Add(sb.ToString());
            }

            WriteTextFile(fName, lines); //write matrix to file
        } //end of WriteMatrix2File\

        public static void WriteMatrix2File(int[,] matrix, string fName)
        {
            int rowCount = matrix.GetLength(0); //height
            int colCount = matrix.GetLength(1); //width

            var lines = new List<string>();

            for (int i = 0; i < rowCount; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    sb.Append(matrix[i, j]);
                    if (j < colCount - 1)
                    {
                        sb.Append(",");
                    }
                }

                lines.Add(sb.ToString());
            }

            WriteTextFile(fName, lines); //write matrix to file
        } //end of WriteMatrix2File\

        public static void WriteMatrix2File(char[,] matrix, string fName)
        {
            int rowCount = matrix.GetLength(0); //height
            int colCount = matrix.GetLength(1); //width

            var lines = new List<string>();

            for (int i = 0; i < rowCount; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    sb.Append(matrix[i, j]);
                }

                lines.Add(sb.ToString());
            }

            WriteTextFile(fName, lines); //write matrix to file
        } //end of WriteMatrix2File\

        public static void WriteMatrix2File_Formatted(double[,] matrix, string fName, string formatString)
        {
            int rowCount = matrix.GetLength(0); //height
            int colCount = matrix.GetLength(1); //width

            var lines = new List<string>();

            for (int i = 0; i < rowCount; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    sb.Append(matrix[i, j].ToString(formatString));
                    if (j < colCount - 1)
                    {
                        sb.Append(",");
                    }
                }

                lines.Add(sb.ToString());
            }

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
            }

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
            }

            WriteTextFile(path, lines); //write matrix to file
        } //end of WriteArray2File_Formatted

        public static string PathCombine(params string[] paths)
        {
            return paths.Aggregate(string.Empty, (s1, s2) => Path.Combine(s1, s2));
        }

        public static string UrlCombine(params string[] segments)
        {
            return segments.Aggregate(string.Empty, (a, b) =>
            {
                if (string.IsNullOrEmpty(a))
                {
                    return b;
                }
                else
                {
                    if (a.EndsWith("\\") || a.EndsWith("/"))
                    {
                        a = a.Substring(0, a.Length - 1);
                    }

                    if (b.StartsWith("\\") || b.StartsWith("/"))
                    {
                        b = b.Substring(1);
                    }

                    return a + "/" + b;
                }
            });
        } // end of UrlCombine(params string[] segments)
    }
}