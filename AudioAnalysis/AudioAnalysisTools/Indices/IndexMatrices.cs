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

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;

    using TowseyLibrary;

    public static class IndexMatrices
    {

        public static void test()
        {
            var matrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(""));
        }


        public static Dictionary<string, double[,]> GetSpectralIndexFilesAndConcatenate(string path, string[] keys)
        {
            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();

            foreach (string key in keys)
            {
                DateTime now1 = DateTime.Now;
                string pattern = "*" + key + ".csv";
                FileInfo[] files = IndexMatrices.GetFilesInDirectory(path, pattern);

                var m = IndexMatrices.ReadAndConcatenateSpectrogramCSVFiles(files);
                m = MatrixTools.MatrixRotate90Anticlockwise(m);
                spectrogramMatrices.Add(key, m);

                
                DateTime now2 = DateTime.Now;
                TimeSpan et = now2 - now1;
                LoggedConsole.WriteLine("Time to read <" + key + "> spectral index files = " + et.TotalSeconds + " seconds");
            }

            return spectrogramMatrices;
        }

        /// <summary>
        /// Returns a sorted list of file paths, sorted on file name.
        /// IMPORTANT: Sorts on alphanumerics, NOT on time. 
        ///            Make sure file name is correctly formatted if intend to sort for time.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static FileInfo[] GetFilesInDirectory(string path, string pattern)
        {
            var dirInfo = new DirectoryInfo(path);

            if (!dirInfo.Exists)
            {
                var directoryNotFoundException = new DirectoryNotFoundException(path);
                LoggedConsole.WriteFatalLine("DIRECTORY DOES NOT EXIST", directoryNotFoundException);
                throw directoryNotFoundException;
            }

            var list = new List<string>();
            FileInfo[] files = dirInfo.GetFiles(pattern, SearchOption.AllDirectories);
            if ((files == null) || (files.Length == 0))
            {
                LoggedConsole.WriteErrorLine("No match - Empty list of files");
            }
            
            Array.Sort(files, (f1, f2) => f1.Name.CompareTo(f2.Name));

            return files;
        }



        public static double[,] ReadAndConcatenateSpectrogramCSVFiles(FileInfo[] paths)
        {
            string warning = null;

            var list =  new List<double[,]>();

            // get the path containing keys[i]
            foreach (FileInfo file in paths)
            {
                if (file.Exists)
                {
                    int freqBinCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);
                    list.Add(matrix);
                }
            }

            var M = MatrixTools.ConcatenateMatrixRows(list);


            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            return M;
        }


        public static double[,] ReadSpectrogram(FileInfo csvPath, out int binCount)
        {
            //TwoDimensionalArray dimensionality = TwoDimensionalArray.RowMajor;
            //double[,] matrix = Csv.ReadMatrixFromCsv<double>(csvPath, dimensionality);
            // MICHAEL: the new Csv class can read this in, and optionally transpose as it reads
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath.FullName);
            binCount = matrix.GetLength(1) - 1; // -1 because first bin is the index numbers 
            // calculate the window/frame that was used to generate the spectra. This value is only used to place grid lines on the final images

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 1);
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


    }
}
