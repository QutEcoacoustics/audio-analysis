// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexMatrices.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using log4net;
    using TowseyLibrary;

    public static class IndexMatrices
    {
        public const string MissingRowString = "<missing row>";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// All the passed files will be concatenated. Filtering needs to be done somewhere else.
        /// </summary>
        /// <param name="files">array of file names</param>
        /// <param name="indexCalcDuration">used to match rows of indices to elapsed time in file names</param>
        public static List<SummaryIndexValues> ConcatenateSummaryIndexFilesWithTimeCheck(FileInfo[] files, TimeSpan indexCalcDuration)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            DateTimeOffset[] dtoArray = new DateTimeOffset[files.Length];
            var summaryIndices = new List<SummaryIndexValues>();

            // accumulate the start times for each of the files
            for (int f = 0; f < files.Length; f++)
            {
                if (!files[f].Exists)
                {
                    LoggedConsole.WriteWarnLine($"WARNING: Concatenation Time Check: MISSING FILE: {files[f].FullName}");
                    continue;
                }

                if (!FileDateHelpers.FileNameContainsDateTime(files[f].Name, out var date, offsetHint))
                {
                    LoggedConsole.WriteWarnLine($"WARNING: Concatenation Time Check: INVALID DateTime in File Name {files[f].Name}");
                }

                dtoArray[f] = date;
            }

            // we use the fileName field to distinguish unique input source files
            // this Set allows us to check they are unique and render joins
            var sourceFileNames = new HashSet<string>();

            // now loop through the files again to extract the indices
            var missingRowCounter = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Exists)
                {
                    continue;
                }

                // Log.Debug("Reading of file started: " + files[i].FullName);
                var rowsOfCsvFile = Csv.ReadFromCsv<SummaryIndexValues>(files[i], throwOnMissingField: false);

                // check all rows have fileName set
                var thisSourceFileNames = new HashSet<string>();
                foreach (var summaryIndexValues in rowsOfCsvFile)
                {
                    if (summaryIndexValues.FileName.IsNullOrEmpty())
                    {
                        throw new InvalidOperationException($"A supplied summary index file did not have the `{nameof(SummaryIndexValues.FileName)}` field populated. File: {files[i].FullName}");
                    }

                    thisSourceFileNames.Add(summaryIndexValues.FileName);
                }

                // check all found filenames are unique
                foreach (var sourceFileName in thisSourceFileNames)
                {
                    if (sourceFileNames.Contains(sourceFileName))
                    {
                        throw new InvalidOperationException(
                            $"The summary index files already read previously contained the filename {sourceFileName} - duplicates are not allowed. File: {files[i].FullName}");
                    }

                    sourceFileNames.Add(sourceFileName);
                }

                summaryIndices.AddRange(rowsOfCsvFile);

                // track the row counts
                int partialRowCount = rowsOfCsvFile.Count();

                // calculate elapsed time from the rows
                int accumulatedRowMinutes = (int)Math.Round(partialRowCount * indexCalcDuration.TotalMinutes);

                // calculate the partial elapsed minutes as indexed by file names.
                var elapsedMinutesInFileNames = 0;
                if (i < files.Length - 1)
                {
                    TimeSpan elapsedTimeAccordingtoFileNames = dtoArray[i + 1] - dtoArray[i];
                    elapsedMinutesInFileNames = (int)Math.Round(elapsedTimeAccordingtoFileNames.TotalMinutes);
                }
                else
                {
                    elapsedMinutesInFileNames = accumulatedRowMinutes; // a hack for the last file
                }

                // Check for Mismatch error in concatenation.
                if (accumulatedRowMinutes != elapsedMinutesInFileNames)
                {
                    string str1 = $"Concatenation: Elapsed Time Mismatch ERROR in csvFile {i + 1}/{files.Length}: {accumulatedRowMinutes} accumulatedRowMinutes != {elapsedMinutesInFileNames} elapsedMinutesInFileNames";
                    LoggedConsole.WriteWarnLine(str1);

                    //dictionary = RepairDictionaryOfArrays(dictionary, rowCounts[i], partialMinutes);
                    int scalingfactor = (int)Math.Round(60.0 / indexCalcDuration.TotalSeconds);
                    int minutesToAdd = elapsedMinutesInFileNames - accumulatedRowMinutes;
                    int rowsToAdd = minutesToAdd * scalingfactor;

                    // add in the missing summary index rows
                    for (int j = 0; j < rowsToAdd; j++)
                    {
                        var vector = new SummaryIndexValues { FileName = MissingRowString };
                        summaryIndices.Add(vector);
                    }
                }
            }

            // Can prune the list of summary indices as required.
            //int expectedRowCount = (int)Math.Round(numberOfMinutesInDay / indexCalcDuration.TotalMinutes);
            //if (totalRowCount != expectedRowCount)
            //{
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine("WARNING: INCONSISTENT ELAPSED TIME CHECK from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck() ");
            //    string str = String.Format("   Final Data Row Count = {0}     Estimated Cumulative Duration = {1} minutes", totalRowCount, expectedRowCount);
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine(str);
            //    dictionary = RepairDictionaryOfArrays(dictionary, totalRowCount, expectedRowCount);
            //}

            return summaryIndices;
        }

        /// <summary>
        /// WARNING: THIS METHOD ONLY GETS FIXED LIST OF INDICES.
        ///             Also it requires every index to be of type DOUBLE even when htis is not appropriate.
        /// TODO: This needs to be generalized
        /// </summary>
        public static Dictionary<string, double[]> GetDictionaryOfSummaryIndices(List<SummaryIndexValues> summaryIndices)
        {
            var dictionary = new Dictionary<string, double[]>
            {
                { GapsAndJoins.KeyZeroSignal, summaryIndices.Select(x => x.ZeroSignal).ToArray() },
                { "ClippingIndex", summaryIndices.Select(x => x.ClippingIndex).ToArray() },
                { "BackgroundNoise", summaryIndices.Select(x => x.BackgroundNoise).ToArray() },
                { "Snr", summaryIndices.Select(x => x.Snr).ToArray() },
                { "EventsPerSecond", summaryIndices.Select(x => x.EventsPerSecond).ToArray() },
                { "Activity", summaryIndices.Select(x => x.Activity).ToArray() },
                { "HighFreqCover", summaryIndices.Select(x => x.HighFreqCover).ToArray() },
                { "MidFreqCover", summaryIndices.Select(x => x.MidFreqCover).ToArray() },
                { "LowFreqCover", summaryIndices.Select(x => x.LowFreqCover).ToArray() },
                { "TemporalEntropy", summaryIndices.Select(x => x.TemporalEntropy).ToArray() },
                { "EntropyOfAverageSpectrum", summaryIndices.Select(x => x.EntropyOfAverageSpectrum).ToArray() },
                { "EntropyOfPeaksSpectrum", summaryIndices.Select(x => x.EntropyOfPeaksSpectrum).ToArray() },
                { "AcousticComplexity", summaryIndices.Select(x => x.AcousticComplexity).ToArray() },
                { "ClusterCount", summaryIndices.Select(x => x.ClusterCount).ToArray() },
                { "ThreeGramCount", summaryIndices.Select(x => x.ThreeGramCount).ToArray() },
            };

            return dictionary;
        }

        /*
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
        */

        public static Dictionary<string, double[,]> GetSpectralIndexFilesAndConcatenate(
            DirectoryInfo[] dirs,
            string analysisType,
            string[] keys,
            IndexGenerationData indexGenerationData,
            bool verbose = false)
        {
            TimeSpan indexCalcTimeSpan = indexGenerationData.IndexCalculationDuration;
            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();

            foreach (string key in keys)
            {
                //DateTime now1 = DateTime.Now;
                string pattern = "*__" + analysisType + "." + key + ".csv";
                var files = GetFilesInDirectories(dirs, pattern);

                if (files.Length == 0)
                {
                    LoggedConsole.WriteWarnLine($"{key} WARNING: No csv files found for KEY=" + key);
                    continue;
                }

                List<double[,]> matrices = ConcatenateSpectralIndexFilesWithTimeCheck(files, indexCalcTimeSpan, key);
                double[,] m = MatrixTools.ConcatenateMatrixRows(matrices);

                //Dictionary<string, double[,]> dict = spectralIndexValues.ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);
                m = MatrixTools.MatrixRotate90Anticlockwise(m);
                spectrogramMatrices.Add(key, m);

                //var now2 = DateTime.Now;
                //var et = now2 - now1;
                //if (verbose)
                //{
                //    LoggedConsole.WriteLine($"\t\tTime to read <{key}> spectral index files = {et.TotalSeconds:f2} seconds");
                //}
            }

            return spectrogramMatrices;
        }

        /// <summary>
        /// Concatenates a series of Spectral Index files with a time check,
        ///  i.e. check elapse time in file names against accumulated rows of indices.
        /// </summary>
        /// <param name="files">All the passed files will be concatenated. Filtering needs to be done somewhere else.</param>
        /// <param name="indexCalcDuration">used to match rows of indices to elapsed time in file names</param>
        /// <param name="key">this is used only in case need to write an error message. It identifies the key.</param>
        public static List<double[,]> ConcatenateSpectralIndexFilesWithTimeCheck(FileInfo[] files, TimeSpan indexCalcDuration, string key)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            var datesAndFiles = new (DateTimeOffset date, FileInfo file)[files.Length];
            var matrices = new List<double[,]>();

            // accumulate the start times for each of the files
            for (int f = 0; f < files.Length; f++)
            {
                var file = files[f];
                if (!file.Exists)
                {
                    LoggedConsole.WriteWarnLine($"WARNING: {key} Concatenation Time Check: MISSING FILE: {files[f].FullName}");
                    continue;
                }

                DateTimeOffset startDto;
                if (!FileDateHelpers.FileNameContainsDateTime(file.Name, out startDto, offsetHint))
                {
                    LoggedConsole.WriteWarnLine($"WARNING: {key} Concatenation Time Check: INVALID DateTime in File Name {file.Name}");
                }

                datesAndFiles[f] = (startDto, file);
            }

            // list of file needs to be sorted (relying on system sorting is not reliable)
            datesAndFiles = datesAndFiles.OrderBy(df => df.date).ToArray();

            string fileName = datesAndFiles[0].file.Name;
            string fileExt = fileName.Substring(fileName.Length - 7);

            // now loop through the files again to extract the indices
            for (int i = 0; i < datesAndFiles.Length; i++)
            {
                var file = datesAndFiles[i].file;
                if (!file.Exists)
                {
                    continue;
                }

                var matrix = Csv.ReadMatrixFromCsv<double>(file, TwoDimensionalArray.Normal);
                matrices.Add(matrix);

                // track the row counts
                int partialRowCount = matrix.GetLength(0);

                // calculate elapsed time from the rows
                int accumulatedRowMinutes = (int)Math.Round(partialRowCount * indexCalcDuration.TotalMinutes);

                //track the elapsed minutes
                // calculate the partial elapsed time indexed by file names.
                var elapsedMinutesInFileNames = 0;
                var length = datesAndFiles.Length;
                if (i < length - 1)
                {
                    TimeSpan partialElapsedTime = datesAndFiles[i + 1].date - datesAndFiles[i].date;
                    elapsedMinutesInFileNames = (int)Math.Round(partialElapsedTime.TotalMinutes);
                }
                else
                {
                    elapsedMinutesInFileNames = accumulatedRowMinutes; // a hack for the last file
                }

                if (accumulatedRowMinutes < elapsedMinutesInFileNames)
                {
                    string str1 = $"{key} Concatenation: Elapsed Time Mismatch ERROR in csvFile {i + 1}/{files.Length}: {accumulatedRowMinutes} accumulatedRowMinutes != {elapsedMinutesInFileNames} elapsedMinutesInFileNames";
                    LoggedConsole.WriteWarnLine(str1);

                    int scalingfactor = (int)Math.Round(60.0 / indexCalcDuration.TotalSeconds);
                    int minutesToAdd = elapsedMinutesInFileNames - accumulatedRowMinutes;
                    int rows2Add = minutesToAdd * scalingfactor;

                    int columnCount = matrices[0].GetLength(1);
                    var emptyMatrix = new double[rows2Add, columnCount];
                    if (fileExt.StartsWith("BGN"))
                    {
                        for (int r = 0; r < rows2Add; r++)
                        {
                            for (int c = 0; c < columnCount; c++)
                            {
                                // initialise with low decibel value
                                // TODO: This should be set equal to a global constant somewhere. May need to change value to -150 dB.
                                emptyMatrix[r, c] = -100.0;
                            }
                        }
                    }

                    matrices.Add(emptyMatrix);
                }
            }

            // Can prune the list of summary indices as required.
            //int expectedRowCount = (int)Math.Round(numberOfMinutesInDay / indexCalcDuration.TotalMinutes);
            //if (totalRowCount != expectedRowCount)
            //{
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine("WARNING: INCONSISTENT ELAPSED TIME CHECK from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck() ");
            //    string str = String.Format("   Final Data Row Count = {0}     Estimated Cumulative Duration = {1} minutes", totalRowCount, expectedRowCount);
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine(str);
            //    dictionary = RepairDictionaryOfArrays(dictionary, totalRowCount, expectedRowCount);
            //}

            return matrices;
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
            if (files.Length == 0)
            {
                LoggedConsole.WriteErrorLine($"No file names match pattern <{pattern}>. Returns empty list of files");
            }

            Array.Sort(files, (f1, f2) => f1.Name.CompareTo(f2.Name));

            return files;
        }

        /// <summary>
        /// Returns a sorted list of file paths, sorted on file name.
        /// IMPORTANT: Sorts on alphanumerics, NOT on date or time encoded in the file name.
        /// </summary>
        public static FileInfo[] GetFilesInDirectories(DirectoryInfo[] directories, string pattern)
        {
            var fileList = new List<FileInfo>();

            foreach (var dir in directories)
            {
                if (!dir.Exists)
                {
                    var directoryNotFoundException = new DirectoryNotFoundException(dir.FullName);
                    LoggedConsole.WriteFatalLine("DIRECTORY DOES NOT EXIST", directoryNotFoundException);
                    throw directoryNotFoundException;
                }

                //FileInfo[] files = dir.GetFiles(pattern, SearchOption.TopDirectoryOnly);
                var files = dir.GetFiles(pattern, SearchOption.AllDirectories);
                fileList.AddRange(files);
            }

            //if (fileList.Count == 0)
            //{
            //    // No need for this warning. It comes later.
            //    LoggedConsole.WriteErrorLine($"No file names match pattern <{pattern}>. Returns empty list of files");
            //}

            FileInfo[] returnList = fileList.ToArray();
            Array.Sort(returnList, (f1, f2) => f1.Name.CompareTo(f2.Name));

            return returnList;
        }

        public static Dictionary<string, double[,]> AddDerivedIndices(Dictionary<string, double[,]> spectrogramMatrices)
        {
            string key = "POW";
            string newKey = "Sqrt" + key;
            if (spectrogramMatrices.ContainsKey(key) && !spectrogramMatrices.ContainsKey(newKey))
            {
                // add another matrix with square root and log transform  of values for lop-sided distributions
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.SquareRootOfValues(m));
                newKey = "Log" + key;
                spectrogramMatrices.Add(newKey, MatrixTools.LogTransform(m));
            }

            // add another matrix with square root and log transform of values for lop-sided distributions
            key = "ENT";
            newKey = "Sqrt" + key;
            if (spectrogramMatrices.ContainsKey(key) && !spectrogramMatrices.ContainsKey(newKey))
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.SquareRootOfValues(m));
            }

            newKey = "Log" + key;
            if (spectrogramMatrices.ContainsKey(key) && !spectrogramMatrices.ContainsKey(newKey))
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.LogTransform(m));
            }

            return spectrogramMatrices;
        }

        /// <summary>
        /// DO NOT DELETE THIS METHOD DESPITE NO REFERENCES
        /// It can be useful in future.
        /// </summary>
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
                summaryIndices = AddNDSI_GageGauge(summaryIndices, ndsiKey);
            }

            ndsiKey = "NDSI-MH";
            if (!summaryIndices.ContainsKey(ndsiKey))
            {
                summaryIndices = AddNDSI_GageGauge(summaryIndices, ndsiKey);
            }

            return summaryIndices;
        }

        /// <summary>
        /// This method reads spectrogram csv files where the first row contains column names
        /// and the first column contains row/time names.
        /// </summary>
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

        /// <summary>
        /// Returns dictionary of spectral indices.
        /// Assumes both arrays of same length and keys correspond to file name.
        /// TODO: Do this better one day!
        /// </summary>
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

        public static Dictionary<string, double[,]> ReadCsvFiles(DirectoryInfo ipdir, string fileName, string[] keys)
        {
            // parallel reading of CSV files
            var readData = keys
                .AsParallel()
                .Select(key => ReadInSingleCsvFile(ipdir, fileName, key))
                .Where(x => x != null);

            // actual work done here
            // ReSharper disable PossibleInvalidOperationException
            var spectrogramMatrices = readData.ToDictionary(kvp => kvp.Value.Key, kvp => kvp.Value.Value);

            if (spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method IndexMatrices.ReadCsvFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }

            return spectrogramMatrices;
        }

        private static KeyValuePair<string, double[,]>? ReadInSingleCsvFile(DirectoryInfo ipdir, string fileName, string indexKey)
        {
            //Log.Info($"Starting to read CSV file for index {indexKey}");
            //Stopwatch timer = Stopwatch.StartNew();

            FileInfo file = new FileInfo(Path.Combine(ipdir.FullName, fileName + "." + indexKey + ".csv"));
            double[,] matrix;
            if (file.Exists)
            {
                int freqBinCount;
                matrix = ReadSpectrogram(file, out freqBinCount);
                matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            }
            else
            {
                Log.Warn(
                    "\nWARNING: from method IndexMatrices.ReadCsvFiles()"
                    + $"\n      {indexKey} File does not exist: {file.FullName}");
                return null;
            }

            //timer.Stop();
            //Log.Info($"Time to read spectral index file <{indexKey}> = {timer.Elapsed.TotalSeconds} seconds");
            return new KeyValuePair<string, double[,]>(indexKey, matrix);
        }

        /// <summary>
        /// compresses the spectral index data in the temporal direction by a factor dervied from the data scale and required image scale.
        /// In all cases, the compression is done by taking the average.
        /// The method got more complicated in June 2016 when refactored it to cope with recording blocks less than one minute long.
        /// </summary>
        public static Dictionary<string, double[,]> CompressIndexSpectrograms(Dictionary<string, double[,]> spectra, TimeSpan imageScale, TimeSpan dataScale)
        {
            int scalingFactor = (int)Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);
            var compressedSpectra = new Dictionary<string, double[,]>();
            int step = scalingFactor - 1;

            // if there's no need to compress, simply return
            if (step == 0)
            {
                return spectra;
            }

            foreach (string key in spectra.Keys)
            {
                double[,] matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                int colCount = matrix.GetLength(1);

                int compressionWindow = scalingFactor;
                int compressedLength = colCount / scalingFactor;
                if (compressedLength < 1)
                {
                    compressedLength = 1;
                }

                var newMatrix = new double[rowCount, compressedLength];
                double[] tempArray = new double[scalingFactor];
                int maxColCount = colCount - scalingFactor;
                if (maxColCount < 0)
                {
                    maxColCount = matrix.GetLength(1);
                    compressionWindow = maxColCount;
                }

                // the ENTROPY matrix requires separate calculation
                if ((key == "ENT") && (scalingFactor > 1))
                {
                    matrix = spectra["SUM"];
                    for (int r = 0; r < rowCount; r++)
                    {
                        for (int c = 0; c <= maxColCount; c += step)
                        {
                            var colIndex = c / scalingFactor;
                            for (int i = 0; i < compressionWindow; i++)
                            {
                                // square the amplitude to give energy
                                tempArray[i] = matrix[r, c + i] * matrix[r, c + i];
                            }

                            double entropy = DataTools.Entropy_normalised(tempArray);
                            if (double.IsNaN(entropy))
                            {
                                entropy = 1.0;
                            }

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
                            for (int c = 0; c <= maxColCount; c += step)
                            {
                                var colIndex = c / scalingFactor;
                                for (int i = 0; i < compressionWindow; i++)
                                {
                                    DIFArray[i] = spectra["DIF"][r, c + i];
                                    SUMArray[i] = spectra["SUM"][r, c + i];
                                }

                                newMatrix[r, colIndex] = DIFArray.Sum() / SUMArray.Sum();
                            }
                        }
                    }
                    else
                    {
                        // average all other spectral indices
                        matrix = spectra[key];
                        for (int r = 0; r < rowCount; r++)
                        {
                            for (int c = 0; c <= maxColCount; c += step)
                            {
                                var colIndex = c / scalingFactor;
                                for (int i = 0; i < compressionWindow; i++)
                                {
                                    tempArray[i] = matrix[r, c + i];
                                }

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
            var dict = new Dictionary<string, double[,]>();
            string warning = null;
            freqBinCount = 256; // the default
            for (int key = 0; key < keys.Length; key++)
            {
                var file = new FileInfo(Path.Combine(ipdir.FullName, fileName + "." + keys[key] + ".csv"));
                if (file.Exists)
                {
                    int binCount;
                    double[,] matrix = ReadSpectrogram(file, out binCount);
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

                    warning += $"\n      {keys[key]} File does not exist: {file.FullName}";
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
            const string highKey = "HighFreqCover";
            const string midKey = "MidFreqCover";
            const string lowKey = "LowFreqCover";
            if (newKey.EndsWith("-LM"))
            {
                if (!dictionaryOfCsvColumns.ContainsKey(midKey))
                {
                    return null;
                }

                if (!dictionaryOfCsvColumns.ContainsKey(lowKey))
                {
                    return null;
                }

                double[] midArray = dictionaryOfCsvColumns[midKey];
                double[] lowArray = dictionaryOfCsvColumns[lowKey];
                if (lowArray.Length != midArray.Length)
                {
                    return null;
                }

                var array = new double[lowArray.Length];
                for (int i = 0; i < lowArray.Length; i++)
                {
                    array[i] = (midArray[i] - lowArray[i]) / (midArray[i] + lowArray[i]);
                }

                dictionaryOfCsvColumns.Add(newKey, array);
            }
            else if (newKey.EndsWith("-MH"))
            {
                if (!dictionaryOfCsvColumns.ContainsKey(midKey))
                {
                    return null;
                }

                if (!dictionaryOfCsvColumns.ContainsKey(highKey))
                {
                    return null;
                }

                double[] midArray = dictionaryOfCsvColumns[midKey];
                double[] highArray = dictionaryOfCsvColumns[highKey];
                if (highArray.Length != midArray.Length)
                {
                    return null;
                }

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
