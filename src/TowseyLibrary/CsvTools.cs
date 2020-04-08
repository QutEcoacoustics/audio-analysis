// <copyright file="CsvTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    [Obsolete]
    public static class CsvTools
    {
        //READING A TABLE FROM A CSV FILE

        /// <summary>
        /// loads a data table with data in given csv file.
        /// If the column types are not given then default to string
        /// CALLED ONLY BY KIWI RECOGNIZER TO READ GROUND TRUTH TABLE.
        /// </summary>
        public static DataTable ReadCSVToTable(string filePath, bool isFirstRowHeader, Type[] types)
        {
            string[] csvRows = File.ReadAllLines(filePath);
            var dt = new DataTable();
            if (isFirstRowHeader)
            {
                string[] headers = csvRows[0].Split(',');
                for (int i = 0; i < headers.Length; i++)
                {
                    if (types == null)
                    {
                        dt.Columns.Add(headers[i], typeof(string));
                    }
                    else
                        if (types.Length <= i)
                    {
                        dt.Columns.Add(headers[i], typeof(double));
                    }
                    else
                    {
                        dt.Columns.Add(headers[i], types[i]);
                    }
                }

                csvRows[0] = null; //remove header row
            }

            foreach (string csvRow in csvRows)
            {
                if (csvRow == null)
                {
                    continue; //skip header row
                }

                var fields = csvRow.Split(',');
                var row = dt.NewRow();
                row.ItemArray = MakeItemArray(fields, types);
                dt.Rows.Add(row);
            }

            return dt;
        }

        /// <summary>
        /// reads a CSV file into a Datatable and deduces the data type in each column.
        /// </summary>
        public static DataTable ReadCSVToTable(string filePath, bool isFirstRowHeader)
        {
            string[] csvRows = File.ReadAllLines(filePath);
            if (csvRows.Length == 0)
            {
                return null;
            }

            //convert rows 1-300 toList of strings so can deduce their types.
            var listOfStringArrays = ConvertCSVRowsToListOfStringArrays(csvRows, 1, 300);
            Type[] types = DataTools.GetArrayTypes(listOfStringArrays);

            //initialise the DataTable
            var dt = new DataTable();
            if (isFirstRowHeader)
            {
                string[] headers = csvRows[0].Split(',');
                for (int i = 0; i < headers.Length; i++)
                {
                    dt.Columns.Add(headers[i], types[i]);
                }

                csvRows[0] = null; //remove header row
            }
            else
            {
                for (int i = 0; i < types.Length; i++)
                {
                    dt.Columns.Add("Field" + i, types[i]);
                }
            }

            //fill the DataTable
            foreach (string csvRow in csvRows)
            {
                if (csvRow == null)
                {
                    continue; //skip header row
                }

                var fields = csvRow.Split(',');
                var row = dt.NewRow();
                row.ItemArray = MakeItemArray(fields, types);
                dt.Rows.Add(row);
            }

            return dt;
        }

        /// <summary>
        /// this method is called by the previous method when reading in a CSV file.
        /// It is used only to get csv data into a column format so that the data type for each field can be determined.
        /// </summary>
        public static List<string[]> ConvertCSVRowsToListOfStringArrays(string[] csvRows, int rowStart, int rowEnd)
        {
            if (rowEnd >= csvRows.Length)
            {
                rowEnd = csvRows.Length - 1;
            }

            List<string[]> listOfStringArrays = new List<string[]>();
            int fieldCount = csvRows[0].Split(',').Length;
            int arrayLength = rowEnd - rowStart + 1;

            //init arrays
            for (int c = 0; c < fieldCount; c++)
            {
                listOfStringArrays.Add(new string[arrayLength]);
            }

            // fill the empty arrays
            for (int r = rowStart; r <= rowEnd; r++)
            {
                string[] fields = csvRows[r].Split(',');
                for (int f = 0; f < fieldCount; f++)
                {
                    listOfStringArrays[f][r - rowStart] = fields[f];
                }
            }

            return listOfStringArrays;
        }

        public static object[] MakeItemArray(string[] fields, Type[] types)
        {
            int length = fields.Length;
            object[] output = new object[length];
            for (int i = 0; i < length; i++)
            {
                if (fields[i] == null || fields[i] == string.Empty)
                {
                    output[i] = null;
                }
                else
                if (types[i] == typeof(int))
                {
                    output[i] = int.Parse(fields[i]);
                }
                else
                if (types[i] == typeof(double))
                {
                    output[i] = double.Parse(fields[i]);
                }
                else
                if (types[i] == typeof(bool))
                {
                    output[i] = bool.Parse(fields[i]);
                }
                else
                {
                    output[i] = fields[i];
                }
            }

            return output;
        }

        //#######################################################################################
        //WRITE A CSV FILE FROM A MATRIX and headers
        public static void WriteDictionaryOfDoubles2CSV(Dictionary<string, double[]> dictionary, FileInfo opFile)
        {
            if (dictionary == null)
            {
                return;
            }

            string[] headers = dictionary.Keys.ToArray();
            int rowCount = dictionary[headers[0]].Length; // assume all arrays of the same length
            int colCount = dictionary.Count;

            StreamWriter sw = new StreamWriter(opFile.FullName, false);

            // First we will write the headers.
            sw.Write(headers[0]);
            for (int i = 1; i < colCount; i++)
            {
                sw.Write("," + headers[i]);
            }

            sw.Write(sw.NewLine);

            // Now write all the rows.
            for (int r = 0; r < rowCount; r++)
            {
                double value = dictionary[headers[0]][r];
                string str = $"{value:f4}";
                sw.Write(str);
                for (int c = 1; c < colCount; c++)
                {
                    value = dictionary[headers[c]][r];
                    if (!Convert.IsDBNull(value))
                    {
                        str = $"{value:f4}";
                        sw.Write("," + str);
                    }
                }

                sw.Write(sw.NewLine);
            }

            sw.Close();
        } // WriteDictionaryOfDoubles2CSV()

        //WRITE A CSV FILE FROM A TABLE
        public static void DataTable2CSV(DataTable dt, string strFilePath)
        {
            if (dt == null)
            {
                return;
            }

            Type[] types = DataTableTools.GetColumnTypes(dt);

            // Create the CSV file to which grid data will be exported.
            StreamWriter sw = new StreamWriter(strFilePath, false);

            // First we will write the headers.
            //DataTable dt = m_dsProducts.Tables[0];

            int iColCount = dt.Columns.Count;
            for (int i = 0; i < iColCount; i++)
            {
                sw.Write(dt.Columns[i]);
                if (i < iColCount - 1)
                {
                    sw.Write(",");
                }
            }

            sw.Write(sw.NewLine);

            // Now write all the rows.
            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        if (types[i] == typeof(double))
                        {
                            string str = $"{dr[i]:f4}";
                            sw.Write(str);
                        }
                        else
                            if (types[i] == typeof(TimeSpan))
                        {
                            var ts = (TimeSpan)dr[i];
                            string str = $"{ts.TotalSeconds:f4}";
                            sw.Write(str);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }

                    if (i < iColCount - 1)
                    {
                        sw.Write(",");
                    }
                }

                sw.Write(sw.NewLine);
            }

            sw.Close();
        } // DataTable2CSV()

        /// <summary>
        /// returns a list of the column values in a csv file plus the column headings
        /// ASSUMED to be doubles
        /// returns as lists of type double
        /// TODO Anthony to use the new U-BEAUT csv file reader.
        /// </summary>
        public static Tuple<List<string>, List<double[]>> ReadCSVFile(string csvFileName)
        {
            var lines = FileTools.ReadTextFile(csvFileName);

            int lineCount = lines.Count;
            string[] words = lines[0].Split(',');
            int columnCount = words.Length;

            //GET the CSV COLUMN HEADINGS
            var headers = new List<string>();
            for (int c = 0; c < columnCount; c++)
            {
                headers.Add(words[c]);
            }

            //GET the CSV COLUMN HEADINGS
            //set up the matrix as List of arrays
            List<double[]> values = new List<double[]>();
            for (int c = 0; c < columnCount; c++)
            {
                double[] array = new double[lineCount - 1];
                values.Add(array);
            }

            //fill the arrays
            for (int r = 1; r < lineCount; r++)
            {
                words = lines[r].Split(',');
                for (int c = 0; c < columnCount; c++)
                {
                    double value = 0.0;
                    if (double.TryParse(words[c], out value))
                    {
                        values[c][r - 1] = value;
                    }
                    else
                    {
                        values[c][r - 1] = 0.0;
                    }
                }
            }

            return Tuple.Create(headers, values);
        }

        public static List<double[]> ReadCSVFileOfDoubles(string csvFileName, bool skipHeader, bool skipFirstColumn)
        {
            var list = new List<double[]>();
            using (TextReader reader = new StreamReader(csvFileName))
            {
                string line;
                int firstIndex = 0;
                if (skipFirstColumn)
                {
                    firstIndex = 1;
                }

                if (skipHeader)
                {
                    line = reader.ReadLine(); // skip first header line
                }

                while ((line = reader.ReadLine()) != null)
                {
                    //read one line at a time in string array
                    var words = line.Split(',');
                    var values = new double[words.Length - firstIndex];
                    for (int c = firstIndex; c < words.Length; c++)
                    {
                        values[c - firstIndex] = double.Parse(words[c]);
                    }

                    list.Add(values);
                }//end while
            }//end using

            return list;
        }

        /// <summary>
        /// returns a Dictionary of the column values in a csv file with column headings as keys
        /// ASSUMED to be doubles
        /// TODO Anthony to use the new U-BEAUT csv file reader.
        /// </summary>
        public static Dictionary<string, double[]> ReadCSVFile2Dictionary(string csvFileName)
        {
            var lines = FileTools.ReadTextFile(csvFileName);

            int lineCount = lines.Count;
            string[] words = lines[0].Split(',');
            int columnCount = words.Length;

            //GET the CSV COLUMN HEADINGS
            var headers = new List<string>();
            for (int c = 0; c < columnCount; c++)
            {
                headers.Add(words[c]);
            }

            //GET the CSV COLUMN HEADINGS
            //set up the matrix as List of arrays
            var values = new List<double[]>();
            for (int c = 0; c < columnCount; c++)
            {
                double[] array = new double[lineCount - 1];
                values.Add(array);
            }

            //fill the arrays
            for (int r = 1; r < lineCount; r++)
            {
                words = lines[r].Split(',');
                for (int c = 0; c < columnCount; c++)
                {
                    var parsed = double.TryParse(words[c], out var d);

                    if (parsed)
                    {
                        values[c][r - 1] = d;
                    }
                    else
                    {
                        parsed = TimeSpan.TryParse(words[c], out var ts);
                        if (parsed)
                        {
                            values[c][r - 1] = ts.TotalSeconds;
                        }
                        else
                        {
                            values[c][r - 1] = double.NaN;
                        }
                    }
                }
            }

            Dictionary<string, double[]> dict = new Dictionary<string, double[]>();
            for (int c = 0; c < columnCount; c++)
            {
                dict.Add(headers[c], values[c]);
            }

            return dict;
        }

        public static double[,] ReadCSVFile2Matrix(string csvFileName)
        {
            Tuple<List<string>, List<double[]>> tuple = ReadCSVFile(csvFileName);
            List<double[]> columns = tuple.Item2;
            int rows = columns[0].Length;
            int cols = columns.Count;
            double[,] matrix = new double[rows, cols];

            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    matrix[r, c] = columns[c][r];
                }
            }

            return matrix;
        }

        /// <summary>
        /// Returns the requested column of data from a CSV file and also returns the column header.
        /// </summary>
        public static double[] ReadColumnOfCsvFile(string fileName, int colNumber, out string header)
        {
            List<string> lines = FileTools.ReadTextFile(fileName);
            string[] words = lines[0].Split(',');
            header = words[colNumber];

            // -1 because ignore header
            double[] array = new double[lines.Count - 1];

            // read csv data into arrays. Ignore first line = header.
            for (int i = 1; i < lines.Count; i++)
            {
                words = lines[i].Split(',');
                if (words.Length <= colNumber)
                {
                    array[i - 1] = 0.0;
                    LoggedConsole.WriteErrorLine("WARNING: Error while reading line " + i + "of CSV file.");
                }
                else
                {
                    array[i - 1] = double.TryParse(words[colNumber], out var value) ? value : 0.0;
                }
            }

            return array;
        }
    }
}