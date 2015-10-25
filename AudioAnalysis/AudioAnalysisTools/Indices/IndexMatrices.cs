// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexMatrices.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the IndexMatrices type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using TowseyLibrary;

    public static class IndexMatrices
    {

        public static void test()
        {
            var matrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(""));
        }


        public static Dictionary<string, double[]> ConvertCsvData2DictionaryOfColumns(string[] headers, double[,] M)
        {
            Dictionary<string, double[]> dictionaryOfCsvDataColumns = new Dictionary<string, double[]>();
            for(int i = 0; i < headers.Length; i++)
            {
                dictionaryOfCsvDataColumns.Add(headers[i], MatrixTools.GetColumn(M, i));
            }
            return dictionaryOfCsvDataColumns;
        }


        /// <summary>
        /// WARNING! This method assumes that the total data required is 24 hours long and will trim csv files accordingly.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static Dictionary<string, double[]> GetSummaryIndexFilesAndConcatenateWithTimeCheck(FileInfo[] paths)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            DateTimeOffset startDTO;
            FileInfo file;
            DateTimeOffset[] dtoArray = new DateTimeOffset[paths.Length];
            // check that all file names contain valid date time format.
            for (int i = 0; i < paths.Length; i++)
            {
                file = paths[i];
                if (!FileDateHelpers.FileNameContainsDateTime(file.Name, out startDTO, offsetHint))
                {
                    LoggedConsole.WriteLine("WARNING from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + file.Name + ") ");
                    LoggedConsole.WriteLine("  File name <{0}> does not contain a valid DateTime = {0}", file.Name);
                }
                dtoArray[i] = startDTO;

            }

            // get first file
            string[] headers = null;
            var dictionary = new Dictionary<string, double[]>();
            int[] rowCounts = new int[paths.Length];

            // cycle through remaining files
            for (int i = 0; i < paths.Length; i++)
            {
                file = paths[i];
                if (file.Exists)
                {
                    //####################################################################
                    // REMEMBER THIS
                    // ALTERNATIVE WAY TO DO HTIS WHOLE BUSINESS
                    //var rowsOfCsvFile = Csv.ReadFromCsv<IndexSummaryValues>(file);
                    //var ACIvlauesForOneDAY = rowsOfCsvFile.Select(x => x.ACI).ToArray();

                    // ##################################### NEXT LINE STILL USING DEPRACATED METHOD
                    var intermediateDictionary = CsvTools.ReadCSVFile2Dictionary(file.FullName);

                    if(headers == null) headers = intermediateDictionary.Keys.ToArray(); // only take headers in first file at start of day

                    // now append the intermediate arrays
                    for (int h = 0; h < headers.Length; h++)
                    {
                        string key = headers[h];
                        double[] array2 = intermediateDictionary[headers[h]].ToArray();
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, array2);
                            rowCounts[i] = array2.Length; // assume all arrays of same length
                        }
                        else
                        {
                            // this is probably inefficient but it works!
                            double[] array1 = dictionary[headers[h]].ToArray();
                            double[] result = array1.Concat(array2).ToArray();
                            dictionary[headers[h]] = result;
                            rowCounts[i] = array2.Length; // assume all arrays of same length
                        }
                    }

                    if (i == paths.Length-1) break;

                    TimeSpan partialElapsedTime = dtoArray[i+1] - dtoArray[i];
                    int partialMinutes = (int)Math.Round(partialElapsedTime.TotalMinutes);
                    //int partialMinutes = (int)Math.Ceiling(partialElapsedTime.TotalMinutes);

                    if (rowCounts[i] != partialMinutes)
                    {
                        LoggedConsole.WriteLine("WARNING from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + file.Name + ") ");
                        string str = String.Format("  CsvFile {0}/{1}: Row Count={2} != {3} elapsed minutes", i + 1, paths.Length, rowCounts[i], partialMinutes);
                        dictionary = RepairDictionaryOfArrays(dictionary, rowCounts[i], partialMinutes);
                        int difference = partialMinutes - rowCounts[i];
                        rowCounts[i] += difference;
                    }

                }
                else
                {
                    LoggedConsole.WriteLine("WARNING: from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + file.Extension + ") ");
                    string str = String.Format("   MISSING FILE: {0}", file.FullName);
                    LoggedConsole.WriteLine(str);
                }
            }

            //int cumRowCount = rowCounts.Sum();  // a debug check
            int dataRowCount = dictionary[headers[0]].Length;

            int numberOfMinutesInDay = 1440;
            TimeSpan totalElapsedTime = dtoArray[paths.Length-1] - dtoArray[0];
            int totalElapsedMinutes = (int)Math.Round(totalElapsedTime.TotalMinutes) + rowCounts[paths.Length - 1];
            if (dataRowCount != totalElapsedMinutes)
            {
                LoggedConsole.WriteLine("WARNING: ELAPSED TIME CHECK from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck() ");
                string str = String.Format("   Final Data Row Count = {0}     Estimated Cumulative Duration = {1} minutes", dataRowCount, totalElapsedMinutes);
                LoggedConsole.WriteLine(str);
                dictionary = RepairDictionaryOfArrays(dictionary, dataRowCount, numberOfMinutesInDay);
            }
            //FileTools.WriteMatrix2File(M, @"C:\Users\towsey\temp\delete2.csv");

            return dictionary;
        }


        public static Dictionary<string, double[]> RepairDictionaryOfArrays(Dictionary<string, double[]> dictionary, int rowCount, int requiredCount)
        {
            if (rowCount > requiredCount)
            {
                LoggedConsole.WriteLine("  About to remove {0} rows", requiredCount);
                int countToRemove = rowCount - requiredCount;
                dictionary = RemoveValuesFromArraysInDictionary(dictionary, countToRemove);
            };

            if (rowCount < requiredCount)
            {
                LoggedConsole.WriteLine("  About to append {0} rows", requiredCount);
                int countToAdd = requiredCount - rowCount;
                dictionary = PadDictionaryArraysWithNulls(dictionary, countToAdd);
            };
            return dictionary;
        }



        /// <summary>
        /// WARNING! This method assumes that the total data required is 24 hours long and will trim csv files accordingly.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static Tuple<string[], double[,]> GetSummaryIndexFilesAndConcatenateWithTimeCheck1(FileInfo[] paths)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            var list = new List<double[,]>();
            FileInfo file = paths[0];
            Tuple<List<string>, List<double[]>> tuple = CsvTools.ReadCSVFile(file.FullName);
            string[] headers = tuple.Item1.ToArray();
            double[,] matrix = CreateRectangularArrayFromListOfColumnArrays(tuple.Item2);
            list.Add(matrix);

            DateTimeOffset startDTO;
            if (!FileDateHelpers.FileNameContainsDateTime(file.Name, out startDTO, offsetHint))
            {
                LoggedConsole.WriteLine("WARNING from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + file.Name + ") ");
                LoggedConsole.WriteLine("  File name <{0}> does not contain a valid DateTime = {0}", file.Name);
            }

            DateTimeOffset previousStartTime = startDTO;
            int partialMatrixLength = matrix.GetLength(0);
            int cumRowCount = partialMatrixLength;

            // cycle through remaining files
            for (int i = 1; i < paths.Length; i++)
            {
                file = paths[i];
                if (file.Exists)
                {
                    // ##################################### NEXT LINE STILL USING DEPRACATED METHOD
                    tuple = CsvTools.ReadCSVFile(file.FullName);
                    //matrix = IndexMatrices.ReadSummaryIndicesFromFile(file);
                    matrix = CreateRectangularArrayFromListOfColumnArrays(tuple.Item2);

                    DateTimeOffset thisDTO;
                    if (!FileDateHelpers.FileNameContainsDateTime(file.Name, out thisDTO, offsetHint))
                    {
                        LoggedConsole.WriteLine("WARNING from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + file.Name + ") ");
                        LoggedConsole.WriteLine("  File name <{0}> does not contain a valid DateTime = {0}", file.Name);
                    }

                    var partialElapsedTime = thisDTO - previousStartTime;
                    //int partialMinutes = (int)Math.Round(partialElapsedTime.TotalMinutes);
                    int partialMinutes = (int)Math.Ceiling(partialElapsedTime.TotalMinutes);

                    if (partialMatrixLength != partialMinutes)
                    {
                        LoggedConsole.WriteLine("WARNING from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + file.Name + ") ");
                        string str = String.Format("  Matrix {0}/{1}: Row Count={2} does not tally with Elapsed Minutes in File Name = {3} minutes", i, paths.Length, partialMatrixLength, partialMinutes);
                        LoggedConsole.WriteLine(str);
                        LoggedConsole.WriteLine("  About to append/remove rows to get correct length = {0}", partialMinutes);

                        // repair previous matrix
                        list[i - 1] = RepairMatrixRowCount(list[i - 1], partialMinutes);
                        //FileTools.WriteMatrix2File(matrix, @"C:\Users\towsey\temp\delete.csv");
                    };

                    partialMatrixLength = matrix.GetLength(0);
                    cumRowCount += partialMatrixLength;
                    previousStartTime = thisDTO;
                    list.Add(matrix);
                }
                else
                {
                    LoggedConsole.WriteLine("WARNING: from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + file.Extension + ") ");
                    string str = String.Format("   MISSING FILE: {0}", file.FullName);
                    LoggedConsole.WriteLine(str);
                }
            }

            var M = MatrixTools.ConcatenateMatrixRows(list);
            int matrixRowCount = M.GetLength(0);

            TimeSpan totalElapsedTime = previousStartTime - startDTO;
            int totalElapsedMinutes = (int)Math.Round(totalElapsedTime.TotalMinutes) + partialMatrixLength;
            if (matrixRowCount != totalElapsedMinutes)
            {
                LoggedConsole.WriteLine("WARNING: ELAPSED TIME CHECK from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck() ");
                string str = String.Format("   Final Matrix Row Count = {0}     Estimated Cumulative Duration = {1} minutes", M.GetLength(0), totalElapsedMinutes);
                LoggedConsole.WriteLine(str);
            }
            int numberOfMinutesInDay = 1440;
            if (matrixRowCount != numberOfMinutesInDay)
            {
                LoggedConsole.WriteLine("MATRIX ROW COUNT != {0} minutes in day. Append/remove rows to correct length!", numberOfMinutesInDay);
                M = RepairMatrixRowCount(M, numberOfMinutesInDay);
            }
            //FileTools.WriteMatrix2File(M, @"C:\Users\towsey\temp\delete2.csv");

            var opTuple = Tuple.Create(headers, M);
            return opTuple;
        }



        static T[,] CreateRectangularArray<T>(IList<T[]> arrays)
        {
            // TODO: Validation and special-casing for arrays.Count == 0
            int minorLength = arrays[0].Length;
            T[,] ret = new T[arrays.Count, minorLength];
            for (int i = 0; i < arrays.Count; i++)
            {
                var array = arrays[i];
                if (array.Length != minorLength)
                {
                    throw new ArgumentException
                        ("All arrays must be the same length");
                }
                for (int j = 0; j < minorLength; j++)
                {
                    ret[i, j] = array[j];
                }
            }
            return ret;
        }

        static T[,] CreateRectangularArrayFromListOfColumnArrays<T>(IList<T[]> arrays)
        {
            // TODO: Validation and special-casing for arrays.Count == 0
            int rowCount = arrays[0].Length;
            int colCount = arrays.Count;
            T[,] ret = new T[rowCount, colCount];
            for (int c = 0; c < colCount; c++)
            {
                var array = arrays[c];
                if (array.Length != rowCount)
                {
                    throw new ArgumentException
                        ("All arrays must be the same length");
                }
                for (int r = 0; r < rowCount; r++)
                {
                    ret[r, c] = array[r];
                }
            }
            return ret;
        }


        //public static double[,] GetSummaryIndexFilesAndConcatenate(string path, string pattern)
        //{
        //        DateTime now1 = DateTime.Now;
        //        FileInfo[] files = IndexMatrices.GetFilesInDirectory(path, pattern);

        //        var m = IndexMatrices.ReadAndConcatenateSpectrogramCSVFiles(files);

        //        //m = MatrixTools.MatrixRotate90Anticlockwise(m);

        //        DateTime now2 = DateTime.Now;
        //        TimeSpan et = now2 - now1;
        //        LoggedConsole.WriteLine("Time to read <" + pattern + "> summary index files = " + et.TotalSeconds + " seconds");
        //    return m;
        //}


        public static Dictionary<string, double[,]> GetSpectralIndexFilesAndConcatenate(DirectoryInfo[] dirs, string fileStemPattern, string[] keys)
        {
            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();

            foreach (string key in keys)
            {
                DateTime now1 = DateTime.Now;
                // string pattern = "*" + key + ".csv";
                string pattern = fileStemPattern + "." + key + ".csv";
                FileInfo[] files = IndexMatrices.GetFilesInDirectories(dirs, pattern);
                if (files.Length == 0) return spectrogramMatrices;

                //var m = IndexMatrices.ReadAndConcatenateSpectrogramCSVFiles(files);
                var m = IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(files);

                m = MatrixTools.MatrixRotate90Anticlockwise(m);
                spectrogramMatrices.Add(key, m);

                DateTime now2 = DateTime.Now;
                TimeSpan et = now2 - now1;
                LoggedConsole.WriteLine(String.Format("Time to read <{0}> spectral index files = {1:f2} seconds", key, et.TotalSeconds));
            }

            return spectrogramMatrices;
        }

        public static Dictionary<string, double[,]> GetSpectralIndexFilesAndConcatenate(string path, string fileStemPattern, string[] keys)
        {
            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();

            foreach (string key in keys)
            {
                DateTime now1 = DateTime.Now;
                // string pattern = "*" + key + ".csv";
                string pattern = fileStemPattern + "." + key + ".csv";
                FileInfo[] files = IndexMatrices.GetFilesInDirectory(path, pattern);
                if (files.Length == 0) return spectrogramMatrices;

                //var m = IndexMatrices.ReadAndConcatenateSpectrogramCSVFiles(files);
                var m = IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(files);
                
                m = MatrixTools.MatrixRotate90Anticlockwise(m);
                spectrogramMatrices.Add(key, m);

                DateTime now2 = DateTime.Now;
                TimeSpan et = now2 - now1;
                LoggedConsole.WriteLine("Time to read <" + key + "> spectral index files = " + et.TotalSeconds + " seconds");
            }

            return spectrogramMatrices;
        }


        public static FileInfo[] GetFilesInDirectory(string path, string pattern)
        {
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                var directoryNotFoundException = new DirectoryNotFoundException(path);
                LoggedConsole.WriteFatalLine("DIRECTORY DOES NOT EXIST", directoryNotFoundException);
                throw directoryNotFoundException;
            }

            FileInfo[] files = dirInfo.GetFiles(pattern, SearchOption.AllDirectories);
            if ((files == null) || (files.Length == 0))
            {
                LoggedConsole.WriteErrorLine("No match - Empty list of files");
            }

            Array.Sort(files, (f1, f2) => f1.Name.CompareTo(f2.Name));

            return files;
        }


        /// <summary>
        /// Returns a sorted list of file paths, sorted on file name.
        /// IMPORTANT: Sorts on alphanumerics, NOT on date or time encoded in the file name. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static FileInfo[] GetFilesInDirectories(DirectoryInfo[] directories, string pattern)
        {
            List<FileInfo> fileList = new List<FileInfo>();

            foreach (DirectoryInfo dir in directories)
            {
                if (!dir.Exists)
                {
                    var directoryNotFoundException = new DirectoryNotFoundException(dir.FullName);
                    LoggedConsole.WriteFatalLine("DIRECTORY DOES NOT EXIST", directoryNotFoundException);
                    throw directoryNotFoundException;
                }

                var list = new List<string>();
                FileInfo[] files = dir.GetFiles(pattern, SearchOption.AllDirectories);
                fileList.AddRange(files);
            }

            if ((fileList == null) || (fileList.Count == 0))
            {
                LoggedConsole.WriteErrorLine("No match - Empty list of files");
            }

            FileInfo[] returnList = fileList.ToArray();
            Array.Sort(returnList, (f1, f2) => f1.Name.CompareTo(f2.Name));

            return returnList;
        }


        public static double[,] ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(FileInfo[] paths)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            var list = new List<double[,]>();
            int freqBinCount;
            FileInfo file = paths[0];
            double[,] matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);
            list.Add(matrix);

            DateTimeOffset startDTO;
            if (! FileDateHelpers.FileNameContainsDateTime(file.Name, out startDTO, offsetHint))
            {
                LoggedConsole.WriteLine("WARNING from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(" + file.Name + ") ");
                LoggedConsole.WriteLine("  File name <{0}> does not contain a valid DateTime = {0}", file.Name);
            }

            DateTimeOffset previousStartTime = startDTO;
            int partialMatrixLength = matrix.GetLength(0);
            int cumRowCount = partialMatrixLength;

            // cycle through remaining files
            for (int i = 1; i < paths.Length; i++)
            {
                file = paths[i];
                if (file.Exists)
                {
                    matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);

                    DateTimeOffset thisDTO;
                    if (!FileDateHelpers.FileNameContainsDateTime(file.Name, out thisDTO, offsetHint))
                    {
                        LoggedConsole.WriteLine("WARNING from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(" + file.Name + ") ");
                        LoggedConsole.WriteLine("  File name <{0}> does not contain a valid DateTime = {0}", file.Name);
                    }

                    var partialElapsedTime = thisDTO - previousStartTime;
                    //int partialMinutes = (int)Math.Round(partialElapsedTime.TotalMinutes);
                    int partialMinutes = (int)Math.Ceiling(partialElapsedTime.TotalMinutes);

                    if (partialMatrixLength != partialMinutes)
                    {
                        LoggedConsole.WriteLine("WARNING from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(" + file.Name + ") ");
                        string str = String.Format("  Matrix {0}/{1}: Row Count={2} does not tally with Elapsed Minutes in File Name = {3} minutes", i, paths.Length, partialMatrixLength, partialMinutes);
                        LoggedConsole.WriteLine(str);
                        LoggedConsole.WriteLine("  About to append/remove rows to get correct length = {0}", partialMinutes);

                        // repair previous matrix
                        list[i - 1] = RepairMatrixRowCount(list[i - 1], partialMinutes);
                        //FileTools.WriteMatrix2File(matrix, @"C:\Users\towsey\temp\delete.csv");
                    };

                    partialMatrixLength = matrix.GetLength(0);
                    cumRowCount += partialMatrixLength;
                    previousStartTime = thisDTO;
                    list.Add(matrix);
                }
                else {
                    LoggedConsole.WriteLine("WARNING: from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(" + file.Extension + ") ");
                    string str = String.Format("   MISSING FILE: {0}", file.FullName);
                    LoggedConsole.WriteLine(str);
                }
            }

            var M = MatrixTools.ConcatenateMatrixRows(list);
            int matrixRowCount = M.GetLength(0);

            TimeSpan totalElapsedTime = previousStartTime - startDTO;
            int totalElapsedMinutes = (int)Math.Round(totalElapsedTime.TotalMinutes) + partialMatrixLength;
            if (matrixRowCount != totalElapsedMinutes)
            {
                LoggedConsole.WriteLine("WARNING: ELAPSED TIME CHECK from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck() ");
                string str = String.Format("   Final Matrix Row Count = {0}     Estimated Cumulative Duration = {1} minutes", M.GetLength(0), totalElapsedMinutes);
                LoggedConsole.WriteLine(str);
            }
            int numberOfMinutesInDay = 1440;
            if (matrixRowCount != numberOfMinutesInDay)
            {
                LoggedConsole.WriteLine("MATRIX ROW COUNT != {0} minutes in day. Append/remove rows to correct length!", numberOfMinutesInDay);
                M = RepairMatrixRowCount(M, numberOfMinutesInDay);
            }
            //FileTools.WriteMatrix2File(M, @"C:\Users\towsey\temp\delete2.csv");

            return M;
        }


        public static double[,] RepairMatrixRowCount(double[,] M, int requiredRowCount)
        {
            int currentRowCount = M.GetLength(0);

            if (currentRowCount < requiredRowCount)
            {
                int rowDeficiency = requiredRowCount - currentRowCount;
                double[,] M1 = MatrixTools.AddBlankRows(M, rowDeficiency);
                return M1;
            }

            if (currentRowCount > requiredRowCount)
            {
                int surplusRows = currentRowCount - requiredRowCount;
                double[,] M2 = MatrixTools.RemoveLastNRows(M, surplusRows);
                return M2;
            }
            return M; // must have correct row count
        }

        public static Dictionary<string, double[]> PadDictionaryArraysWithNulls(Dictionary<string, double[]> dict, int countToAdd)
        {
            double[] nullArray = new double[countToAdd];
            for (int i = 0; i < countToAdd; i++)
                nullArray[i] = Double.NaN;

            string[] keys = dict.Keys.ToArray();
            foreach(string key in keys)
            {
                double[] array = dict[key];
                double[] result = array.Concat(nullArray).ToArray();
                dict[key] = result;
            }

            return dict; 
        }

        public static Dictionary<string, double[]> RemoveValuesFromArraysInDictionary(Dictionary<string, double[]> dict, int countToRemove)
        {
            string[] keys = dict.Keys.ToArray();
            foreach (string key in keys)
            {
                double[] array = dict[key];
                int newArrayLength = array.Length - countToRemove;
                double[] result = DataTools.Subarray(array, 0, newArrayLength);
                dict[key] = result;
            }

            return dict;
        }





        public static Dictionary<string, double[,]> AddDerivedIndices(Dictionary<string, double[,]> spectrogramMatrices)
        {
            string key = "POW";
            string newKey = "Sqrt" + key;
            if ((spectrogramMatrices.ContainsKey(key)) && (! spectrogramMatrices.ContainsKey(newKey)))
            // add another matrix with square root and log transform  of values for lop-sided distributions
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.SquareRootOfValues(m));
                newKey = "Log" + key;
                spectrogramMatrices.Add(newKey, MatrixTools.LogTransform(m));
            }

            // add another matrix with square root and log transform of values for lop-sided distributions
            key = "ENT";
            newKey = "Sqrt" + key;
            if ((spectrogramMatrices.ContainsKey(key)) && (! spectrogramMatrices.ContainsKey(newKey)))
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.SquareRootOfValues(m));
            }

            newKey = "Log" + key;
            if ((spectrogramMatrices.ContainsKey(key)) && (! spectrogramMatrices.ContainsKey(newKey)))
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.LogTransform(m));
            }
            return spectrogramMatrices;
        }




        public static Dictionary<string, double[]> AddDerivedIndices(Dictionary<string, double[]> summaryIndices)
        {
            // insert some transformed data columns
            summaryIndices.Add("SqrtTempEntropy", DataTools.SquareRootOfValues(summaryIndices["TemporalEntropy"]));

            // insert some transformed data columns
            summaryIndices.Add("LogTempEntropy", DataTools.LogTransform(summaryIndices["TemporalEntropy"]));

            // Calculate Normalised Difference Soundscape Index if not already done
            // caluclate two ratios for three bands.  DO NOT CHANGE THESE KEYS
            string ndsiKey = "NDSI-LM";
            if (!summaryIndices.ContainsKey(ndsiKey))
            {
                summaryIndices = IndexMatrices.AddNDSI_GageGauge(summaryIndices, ndsiKey);
            }
            ndsiKey = "NDSI-MH";
            if (!summaryIndices.ContainsKey(ndsiKey))
            {
                summaryIndices = IndexMatrices.AddNDSI_GageGauge(summaryIndices, ndsiKey);
            }

            return summaryIndices;
        }



public static double[,] ReadSummaryIndicesFromFile(FileInfo csvPath)
        {
            Tuple<List<string>, List<double[]>> tuple = CsvTools.ReadCSVFile(csvPath.FullName);

            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath.FullName);
            // matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 1);
            return matrix;
        }


        /// <summary>
        /// This method reads spectrogram csv files where the first row contains column names 
        /// and the first column contains row/time names.
        /// </summary>
        /// <param name="csvPath"></param>
        /// <param name="binCount"></param>
        /// <returns></returns>
        public static double[,] ReadSpectrogram(FileInfo csvPath, out int binCount)
        {
            //TwoDimensionalArray dimensionality = TwoDimensionalArray.RowMajor;
            //double[,] matrix = Csv.ReadMatrixFromCsv<double>(csvPath, dimensionality);
            // MICHAEL: the new Csv class can read this in, and optionally transpose as it reads
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath.FullName);
            binCount = matrix.GetLength(1) - 1; // -1 because first bin is the index numbers 
            // calculate the window/frame that was used to generate the spectra. This value is only used to place grid lines on the final images

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, binCount);
            return matrix;
        }


        public static Dictionary<string, double[,]> ReadCSVFiles(FileInfo[] paths, string[] keys)
        {
            string warning = null;

            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();
            for (int i = 0; i < keys.Length; i++)
            {
                DateTime now1 = DateTime.Now;

                // get the path containing keys[i]
                FileInfo file = null;
                for (int p = 0; p < paths.Length; p++)
                {
                    if (paths[p].Name.Contains(keys[i]))
                    {
                        file = paths[p];
                        break;
                    }
                }

                if (file.Exists)
                {
                    int freqBinCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    spectrogramMatrices.Add(keys[i], matrix);
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method IndexMatrices.ReadCSVFiles()";
                    }

                    warning += "\n      {0} File does not exist: {1}".Format2(keys[i], file.FullName);
                }

                DateTime now2 = DateTime.Now;
                TimeSpan et = now2 - now1;
                LoggedConsole.WriteLine("Time to read spectral index file <" + keys[i] + "> = " + et.TotalSeconds + " seconds");
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method IndexMatrices.ReadCSVFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from the passed paths");
            }

            return spectrogramMatrices;
        }

        /// <summary>
        /// returns dictionary of spectral indices.
        /// Assumes both arrays of same length and keys correspond to file name. Just too lazy to do it better!
        /// </summary>
        /// <param name="files"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static Dictionary<string, double[,]> ReadSummaryIndexFiles(FileInfo[] files, string[] keys)
        {
            int count = files.Length;
            var dict = new Dictionary<string, double[,]>();
            for (int c = 0; c < count; c++)
            {
                int freqBinCount;
                double[,] matrix = ReadSpectrogram(files[c], out freqBinCount);
                dict.Add(keys[c], matrix);
            }
            return dict;
        }

        public static Dictionary<string, double[,]> ReadCSVFiles(DirectoryInfo ipdir, string fileName, string[] keys)
        {
            string warning = null;

            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();
            for (int i = 0; i < keys.Length; i++)
            {
                DateTime now1 = DateTime.Now;

                FileInfo file = new FileInfo(Path.Combine(ipdir.FullName, fileName + "." + keys[i] + ".csv"));
                if (file.Exists)
                {
                    int freqBinCount;
                    double[,] matrix = ReadSpectrogram(file, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    spectrogramMatrices.Add(keys[i], matrix);
                    //this.FrameLength = freqBinCount * 2;
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method IndexMatrices.ReadCSVFiles()";
                    }

                    warning += "\n      {0} File does not exist: {1}".Format2(keys[i], file.FullName);
                }

                DateTime now2 = DateTime.Now;
                TimeSpan et = now2 - now1;
                LoggedConsole.WriteLine("Time to read spectral index file <" + keys[i] + "> = " + et.TotalSeconds + " seconds");
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method IndexMatrices.ReadCSVFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }

            return spectrogramMatrices;
        }

        /// <summary>
        /// compresses the spectral index data in the temporal direction by a factor dervied from the data scale and required image scale.
        /// In all cases, the compression is done by taking the average
        /// </summary>
        /// <param name="spectra"></param>
        /// <param name="imageScale"></param>
        /// <param name="dataScale"></param>
        /// <returns></returns>
        public static Dictionary<string, double[,]> CompressIndexSpectrograms(Dictionary<string, double[,]> spectra, TimeSpan imageScale, TimeSpan dataScale)
        {
            int scalingFactor = (int)Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);
            var compressedSpectra = new Dictionary<string, double[,]>();
            int step = scalingFactor - 1;
            foreach (string key in spectra.Keys)
            {
                double[,] matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                int colCount = matrix.GetLength(1);
                int compressedLength = (colCount / scalingFactor);
                var newMatrix = new double[rowCount, compressedLength];
                double[] tempArray = new double[scalingFactor];

                // the ENTROPY matrix requires separate calculation
                if ((key == "ENT") && (scalingFactor > 1))
                {
                    matrix = spectra["SUM"];
                    for (int r = 0; r < rowCount; r++)
                    {
                        int colIndex = 0;
                        for (int c = 0; c <= colCount - scalingFactor; c += step)
                        {
                            colIndex = c / scalingFactor;
                            for (int i = 0; i < scalingFactor; i++)
                            {
                                // square the amplitude to give energy
                                tempArray[i] = matrix[r, c + i] * matrix[r, c + i];
                            }
                            double entropy = DataTools.Entropy_normalised(tempArray);
                            if (Double.IsNaN(entropy)) entropy = 1.0;
                            newMatrix[r, colIndex] = 1 - entropy;
                        }
                    }
                }
                else
                    // THE ACI matrix requires separate calculation
                    if ((key == "ACI") && (scalingFactor > 1))
                    {
                        double[] DIFArray = new double[scalingFactor];
                        double[] SUMArray = new double[scalingFactor];
                        for (int r = 0; r < rowCount; r++)
                        {
                            int colIndex = 0;
                            for (int c = 0; c <= colCount - scalingFactor; c += step)
                            {
                                colIndex = c / scalingFactor;
                                for (int i = 0; i < scalingFactor; i++)
                                {
                                    DIFArray[i] = spectra["DIF"][r, c + i];
                                    SUMArray[i] = spectra["SUM"][r, c + i];
                                }
                                newMatrix[r, colIndex] = DIFArray.Sum() / SUMArray.Sum();
                            }
                        }
                    }
                    else // average all other spectral indices
                    {
                        matrix = spectra[key];
                        for (int r = 0; r < rowCount; r++)
                        {
                            int colIndex = 0;
                            for (int c = 0; c <= colCount - scalingFactor; c += step)
                            {
                                colIndex = c / scalingFactor;
                                for (int i = 0; i < scalingFactor; i++) tempArray[i] = matrix[r, c + i];
                                newMatrix[r, colIndex] = tempArray.Average();
                            }
                        }
                    }
                compressedSpectra[key] = newMatrix;
            }
            return compressedSpectra;
        }

        public static Dictionary<string, double[,]> ReadSpectrogramCSVFiles(DirectoryInfo ipdir, string fileName, string indexKeys, out int freqBinCount)
        {
            string[] keys = indexKeys.Split('-');
            return ReadSpectrogramCSVFiles(ipdir, fileName, keys, out freqBinCount);
        }

        public static Dictionary<string, double[,]> ReadSpectrogramCSVFiles(DirectoryInfo ipdir, string fileName, string[] keys, out int freqBinCount)
        {
            Dictionary<string, double[,]> dict = new Dictionary<string, double[,]>();
            string warning = null;
            freqBinCount = 256; // the default
            for (int key = 0; key < keys.Length; key++)
            {
                var file = new FileInfo(Path.Combine(ipdir.FullName, fileName + "." + keys[key] + ".csv"));
                if (file.Exists)
                {
                    int binCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out binCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    dict.Add(keys[key], matrix);
                    freqBinCount = binCount;
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method IndexMatrices.ReadSpectrogramCSVFiles()";
                    }

                    warning += string.Format("\n      {0} File does not exist: {1}", keys[key], file.FullName);
                }
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (dict.Count != 0)
            {
                return dict;
            }

            LoggedConsole.WriteLine("WARNING: from method IndexMatrices.ReadSpectrogramCSVFiles()");
            LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);

            return dict;
        }


        public static Dictionary<string, double[]> AddNDSI_GageGauge(Dictionary<string, double[]> dictionaryOfCsvColumns, string newKey)
        {
            string highKey = "HighFreqCover";
            string midKey  = "MidFreqCover";
            string lowKey  = "LowFreqCover";
            if (newKey.EndsWith("-LM"))
            {
                if (!dictionaryOfCsvColumns.ContainsKey(midKey)) return null;
                if (!dictionaryOfCsvColumns.ContainsKey(lowKey)) return null;
                double[] midArray = dictionaryOfCsvColumns[midKey];
                double[] lowArray = dictionaryOfCsvColumns[lowKey];
                if (lowArray.Length != midArray.Length) return null;

                var array = new double[lowArray.Length];
                for (int i = 0; i < lowArray.Length; i++)
                {
                    array[i] = (midArray[i] - lowArray[i]) / (midArray[i] + lowArray[i]);
                }

                dictionaryOfCsvColumns.Add(newKey, array);
            }
            else if (newKey.EndsWith("-MH"))
            {
                if (!dictionaryOfCsvColumns.ContainsKey(midKey)) return null;
                if (!dictionaryOfCsvColumns.ContainsKey(highKey)) return null;
                double[] midArray  = dictionaryOfCsvColumns[midKey];
                double[] highArray = dictionaryOfCsvColumns[highKey];
                if (highArray.Length != midArray.Length) return null;

                var array = new double[highArray.Length];
                for (int i = 0; i < highArray.Length; i++)
                {
                    array[i] = (highArray[i] - midArray[i]) / (highArray[i] + midArray[i]);
                }

                dictionaryOfCsvColumns.Add(newKey, array);
            }


            return dictionaryOfCsvColumns;
        }


    }
}
