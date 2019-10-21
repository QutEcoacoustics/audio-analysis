// <copyright file="DataProcessing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.Csv;
    using AudioAnalysisTools.DSP;
    using TowseyLibrary;

    public static class DataProcessing
    {
        /// <summary>
        /// Converts an array of templates to dictionary.
        /// </summary>
        /// <param name="array">An array of templates.</param>
        /// <returns>A dictionary of templates.</returns>
        public static Dictionary<string, TemplateManifest> ConvertTemplateArrayToDictionary(TemplateManifest[] array)
        {
            var dictionary = new Dictionary<string, TemplateManifest>();
            foreach (var template in array)
            {
                dictionary.Add(template.Name, template);
            }

            return dictionary;
        }

        /// <summary>
        /// Reads in all the index matrices whose keys are in the above array of IndexNames.
        /// </summary>
        /// <param name="filePath">Partial path to the index files.</param>
        /// <returns>a Dictionary of matrices containing normalised index values.</returns>
        public static Dictionary<string, double[,]> ReadIndexMatrices(string filePath)
        {
            var dictionary = new Dictionary<string, double[,]>();
            var dir = Path.GetDirectoryName(filePath) ?? throw new ArgumentNullException(nameof(filePath) + " does not exist.");
            var baseName = Path.GetFileNameWithoutExtension(filePath) + ".";

            foreach (string key in ContentSignatures.IndexNames)
            {
                // construct a path to the required matrix and read in the matrix
                var indexMatrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(Path.Combine(dir, baseName + key + ".csv")));

                // normalize the matrix values
                var indexBounds = ContentSignatures.IndexValueBounds[key];
                var normalisedMatrix = DataTools.NormaliseInZeroOne(indexMatrix, indexBounds[0], indexBounds[1]);
                dictionary.Add(key, normalisedMatrix);
            }

            return dictionary;
        }

        /// <summary>
        /// This method assumes that the start and end minute for reading from index matrices is first and last row respectively of matrices - assuming one minute per row.
        /// </summary>
        /// <param name="dir">the directory containing the matrices.</param>
        /// <param name="baseName">base name of the files.</param>
        /// <returns>a matrix of indices from required start time to required end time.</returns>
        public static double[,] ReadSpectralIndicesFromIndexMatrices(DirectoryInfo dir, string baseName)
        {
            var startTime = TimeSpan.Zero;
            var duration = TimeSpan.FromMinutes(30);
            var matrix = ReadSpectralIndicesFromIndexMatrices(dir, baseName, startTime, duration);
            return matrix;
        }

        /// <summary>
        /// Read five sets of acoustic indices into a matrix each row of which is a combined feature vector.
        /// </summary>
        public static double[,] ReadSpectralIndicesFromIndexMatrices(DirectoryInfo dir, string baseName, TimeSpan startTime, TimeSpan duration)
        {
            //get start and end minutes
            int startMinute = (int)startTime.TotalMinutes;
            int minuteSpan = (int)duration.TotalMinutes;
            int endMinute = startMinute + minuteSpan;

            // obtain a matrix to see what size data we are dealing with
            // assume all matrices have the same dimensions.
            // construct a path to the required matrix
            var key = ContentSignatures.IndexNames[0];
            var path = Path.Combine(dir.FullName, baseName + "__Towsey.Acoustic." + key + ".csv");

            // read in the matrix and get its dimensions
            var indexMatrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(path));
            var rowCount = indexMatrix.GetLength(0);
            var colCount = indexMatrix.GetLength(1);
            if (rowCount < endMinute)
            {
                throw new ArgumentOutOfRangeException(string.Empty, "Not enough rows in matrix to read the given timespan.");
            }

            // set up the return Matrix
            // indexCount will be number of indices X number of frequency bins
            var indexCount = ContentSignatures.IndexNames.Length * colCount;
            var opMatrix = new double[minuteSpan, indexCount];

            for (int i = 1; i < ContentSignatures.IndexNames.Length; i++)
            {
                key = ContentSignatures.IndexNames[i];

                // construct a path to the required matrix
                path = Path.Combine(dir.FullName, baseName + "__Towsey.Acoustic." + key + ".csv");

                // read in the matrix
                indexMatrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(path));

                for (int r = 0; r < rowCount; r++)
                {
                    // copy in index[key] row
                    var row = MatrixTools.GetRow(indexMatrix, r);
                    int startColumn = colCount * i;
                    for (int c = 0; c < colCount; c++)
                    {
                        var normalisedValue = row[c];
                        opMatrix[r, startColumn + c] = normalisedValue;
                    }
                }
            }

            return opMatrix;
        }

        public static Dictionary<string, double[]> GetIndicesForOneMinute(Dictionary<string, double[,]> allIndices, int rowId)
        {
            var opIndices = new Dictionary<string, double[]>();

            var keys = allIndices.Keys;
            foreach (string key in keys)
            {
                var success = allIndices.TryGetValue(key, out double[,] matrix);
                if (success)
                {
                    opIndices.Add(key, MatrixTools.GetRow(matrix, rowId));
                }
            }

            return opIndices;
        }

        public static Dictionary<string, double[]> AverageIndicesOverMinutes(Dictionary<string, double[,]> allIndices, int startRowId, int endRowId)
        {
            var opIndices = new Dictionary<string, double[]>();

            var keys = allIndices.Keys;
            foreach (string key in keys)
            {
                var success = allIndices.TryGetValue(key, out double[,] matrix);
                if (success)
                {
                    var colCount = matrix.GetLength(1);
                    var subMatrix = MatrixTools.Submatrix(matrix, startRowId, 0, endRowId, colCount - 1);
                    opIndices.Add(key, MatrixTools.GetColumnAverages(subMatrix));
                }
            }

            return opIndices;
        }

        /// <summary>
        /// Reduces a dictionary of vectors by a factor. It is assumed that the input vectors are a power of 2 in length i.e. FFT spectra.
        /// It is assumed that the factor of reduction will also be a power of 2, typically 8 or 16.
        /// </summary>
        /// <returns>The dictionary of reduced vectors.</returns>
        public static Dictionary<string, double[]> ReduceIndicesByFactor(Dictionary<string, double[]> indices, int factor)
        {
            var opIndices = new Dictionary<string, double[]>();

            var keys = indices.Keys;
            foreach (string key in keys)
            {
                var success = indices.TryGetValue(key, out double[] vector);
                if (success)
                {
                    var opVector = DataTools.VectorReduceLength(vector, factor);
                    opIndices.Add(key, opVector);
                }
            }

            return opIndices;
        }

        /// <summary>
        /// Returns the bin bounds assuming that the full spectrum consists of the default value = 256.
        /// </summary>
        /// <param name="bottomFrequency">Units = Hertz.</param>
        /// <param name="topFrequency">Hertz.</param>
        public static int[] GetFreqBinBounds(int bottomFrequency, int topFrequency) => GetFreqBinBounds(bottomFrequency, topFrequency, ContentSignatures.FreqBinCount);

        public static int[] GetFreqBinBounds(int bottomFrequency, int topFrequency, int binCount)
        {
            double binWidth = ContentSignatures.Nyquist / (double)binCount;
            int bottomBin = (int)Math.Floor(bottomFrequency / binWidth);
            int topBin = (int)Math.Ceiling(topFrequency / binWidth);
            return new[] { bottomBin, topBin };
        }

        public static Dictionary<string, double[]> ApplyBandPass(Dictionary<string, double[]> indices, int bottomBin, int topBin)
        {
            int length = topBin - bottomBin + 1;
            var opIndices = new Dictionary<string, double[]>();

            var keys = indices.Keys;
            foreach (string key in keys)
            {
                var success = indices.TryGetValue(key, out double[] vector);
                if (success)
                {
                    var opVector = DataTools.Subarray(vector, bottomBin, length);
                    opIndices.Add(key, opVector);
                }
            }

            return opIndices;
        }

        /// <summary>
        /// THis method assumes that the passed template contains only one value for each key.
        /// </summary>
        /// <param name="templateDict"> Each kvp = string, double.</param>
        /// <param name="oneMinuteIndices">the indices.</param>
        /// <returns>A spectrum of similarity-distance scores.</returns>
        public static double[] ScanSpectrumWithTemplate(Dictionary<string, double[]> templateDict, Dictionary<string, double[]> oneMinuteIndices)
        {
            // convert the template dictionary to an array of averaged values
            var dictionaryOfIndexAverages = AverageIndicesInDictionary(templateDict);
            var templateVector = ConvertDictionaryToVector(dictionaryOfIndexAverages);

            // the score spectrum to be returned
            int spectrumLength = oneMinuteIndices.First().Value.Length;
            var spectralScores = new double[spectrumLength];

            // scan the spectrum of indices
            for (int i = 0; i < spectrumLength; i++)
            {
                var binVector = GetFreqBinVector(oneMinuteIndices, i);

                // does not appear to make any difference whether use Manhattan or Euclidean distance.
                //var distance = DataTools.EuclideanDistance(templateVector, binVector);
                //distance /= Math.Sqrt(templateVector.Length);
                var distance = DataTools.ManhattanDistance(templateVector, binVector);
                distance /= templateVector.Length;
                spectralScores[i] = 1 - distance;
            }

            return spectralScores;
        }

        public static Dictionary<string, double> AverageIndicesInDictionary(Dictionary<string, double[]> dictionary)
        {
            var dictionaryOfIndexAverages = new Dictionary<string, double>();
            foreach (var kvp in dictionary)
            {
                var array = dictionary[kvp.Key];
                dictionaryOfIndexAverages.Add(kvp.Key, array.Average());
            }

            return dictionaryOfIndexAverages;
        }

        public static double[] ConvertDictionaryToVector(Dictionary<string, double[]> dictionary)
        {
            var list = new List<double>();
            var keys = dictionary.Keys;
            foreach (string key in keys)
            {
                var success = dictionary.TryGetValue(key, out double[] indices);
                if (success)
                {
                    list.AddRange(indices);
                }
            }

            return list.ToArray();
        }

        public static double[] ConvertDictionaryToVector(Dictionary<string, double> dictionary)
        {
            var list = new List<double>();
            var keys = dictionary.Keys;
            foreach (string key in keys)
            {
                var success = dictionary.TryGetValue(key, out double index);
                if (success)
                {
                    list.Add(index);
                }
            }

            return list.ToArray();
        }

        public static Dictionary<string, Dictionary<string, double[]>> ExtractDictionaryOfTemplateDictionaries(TemplateManifest[] templates)
        {
            var opDictionary = new Dictionary<string, Dictionary<string, double[]>>();
            foreach (TemplateManifest template in templates)
            {
                var name = template.Name;
                var dictOfIndices = template.Template;
                opDictionary.Add(name, dictOfIndices);
            }

            return opDictionary;
        }

        public static double[,] ConvertDictionaryOfIndicesToMatrix(Dictionary<string, double[]> dictionary)
        {
            var indexCount = ContentSignatures.IndexNames.Length;

            var colCount = dictionary.First().Value.Length;
            var opMatrix = new double[indexCount, colCount];

            for (int i = 0; i < indexCount; i++)
            {
                var success = dictionary.TryGetValue(ContentSignatures.IndexNames[i], out double[] indices);
                if (success)
                {
                    MatrixTools.SetRow(opMatrix, i, indices);
                }
            }

            return opMatrix;
        }

        public static double[] GetFreqBinVector(Dictionary<string, double[]> dictionary, int id)
        {
            var list = new List<double>();
            var keys = dictionary.Keys;
            foreach (string key in keys)
            {
                var success = dictionary.TryGetValue(key, out double[] indices);
                if (success)
                {
                    list.Add(indices[id]);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Converts individual results to a dictionary of plots.
        /// </summary>
        /// <param name="results">a list of results for each content type in every minute.</param>
        /// <param name="arrayLength">The plot length will the total number of minutes scanned, typically 1440 or one day.</param>
        /// <param name="arrayStart">time start.</param>
        public static Dictionary<string, double[]> ConvertResultsToArrays(List<DescriptionResult> results, int arrayLength, int arrayStart)
        {
            var arrays = new Dictionary<string, double[]>();

            foreach (DescriptionResult result in results)
            {
                var time = (int)Math.Round(result.StartTimeInCurrentRecordingFile.TotalMinutes);
                var dict = result.GetDescriptionDictionary();
                foreach (KeyValuePair<string, double> kvp in dict)
                {
                    var name = kvp.Key;
                    var value = kvp.Value;

                    if (!arrays.ContainsKey(name))
                    {
                        var scores = new double[arrayLength];
                        arrays.Add(name, scores);
                    }

                    var scoreArray = arrays[name];
                    scoreArray[arrayStart + time] = value;
                }
            }

            return arrays;
        }

        /// <summary>
        /// Converts individual results to a dictionary of plots.
        /// </summary>
        /// <param name="results">a list of results for each content type in every minute.</param>
        /// <param name="plotLength">The plot length will the total number of minutes scanned, typically 1440 or one day.</param>
        /// <param name="plotStart">time start.</param>
        public static Dictionary<string, Plot> ConvertResultsToPlots(List<DescriptionResult> results, int plotLength, int plotStart)
        {
            var plots = new Dictionary<string, Plot>();

            foreach (DescriptionResult result in results)
            {
                var time = (int)Math.Round(result.StartTimeInCurrentRecordingFile.TotalMinutes);
                var dict = result.GetDescriptionDictionary();
                foreach (KeyValuePair<string, double> kvp in dict)
                {
                    var name = kvp.Key;
                    var value = kvp.Value;

                    if (!plots.ContainsKey(name))
                    {
                        var scores = new double[plotLength];
                        var plot = new Plot(name, scores, 0.25); // NOTE: The threshold can be changed later.
                        plots.Add(name, plot);
                    }

                    plots[name].data[plotStart + time] = value;
                }
            }

            return plots;
        }

        public static List<Plot> SubtractMeanPlusSd(List<Plot> plots)
        {
            var opPlots = new List<Plot>();

            // subtract average from each plot array
            foreach (Plot plot in plots)
            {
                var scores = plot.data;
                NormalDist.AverageAndSD(scores, out double average, out double sd);

                // normalize the scores to z-scores
                for (int i = 0; i < scores.Length; i++)
                {
                    // Convert scores to z-scores
                    scores[i] = (scores[i] - average) / sd;
                    if (scores[i] < 0.0)
                    {
                        scores[i] = 0.0;
                    }

                    if (scores[i] > 4.0)
                    {
                        scores[i] = 4.0;
                    }

                    // normalize full scale to 4 SDs.
                    scores[i] /= 4.0;
                }

                // when normalizing the scores this way the range of the plot will be 0 to 4 SD above the mean.
                // Consequently we set the plot threshold to 0.5, which is two SDs or a p value = 5%.
                plot.threshold = 0.5;
                opPlots.Add(plot);
            }

            return opPlots;
        }

        /// <summary>
        /// THis method normalizes a score array by subtracting the mode rather than the average of the array.
        /// THis is because the noise is often not normally distributed but rather skewed.
        /// However, did not work well.
        /// </summary>
        public static List<Plot> SubtractModeAndSd(List<Plot> plots)
        {
            var opPlots = new List<Plot>();

            // subtract average from each plot array
            foreach (var plot in plots)
            {
                var scores = plot.data;
                var bgn = SNR.CalculateModalBackgroundNoiseInSignal(scores, 1.0);
                var mode = bgn.NoiseMode;
                var sd = bgn.NoiseSd;

                // normalize the scores to z-scores
                for (int i = 0; i < scores.Length; i++)
                {
                    // Convert scores to z-scores
                    scores[i] = (scores[i] - mode) / sd;
                    if (scores[i] < 0.0)
                    {
                        scores[i] = 0.0;
                    }

                    if (scores[i] > 4.0)
                    {
                        scores[i] = 4.0;
                    }

                    // normalize full scale to 4 SDs.
                    scores[i] /= 4.0;
                }

                opPlots.Add(plot);
            }

            return opPlots;
        }

        /// <summary>
        /// Subtract the percentile value from the scores and normalize the remaining values in 0,1.
        /// </summary>
        public static List<Plot> PercentileThresholding(List<Plot> plots, int percentile)
        {
            var opPlots = new List<Plot>();

            // subtract average from each plot array
            foreach (var plot in plots)
            {
                var scores = plot.data;
                var threshold = Statistics.GetPercentileValue(scores, percentile);

                // normalize the scores - first subtract the percentile threshold
                for (int i = 0; i < scores.Length; i++)
                {
                    // Normalize scores relative to threshold
                    scores[i] = scores[i] - threshold;
                    if (scores[i] < 0.0)
                    {
                        scores[i] = 0.0;
                    }
                }

                scores = DataTools.NormaliseByScalingMaxValueToOne(scores);
                plot.data = scores;
                plot.threshold = 0.25;
                opPlots.Add(plot);
            }

            return opPlots;
        }

        public static List<Plot> ConvertPlotDictionaryToPlotList(Dictionary<string, Plot> dict)
        {
            var list = new List<Plot>();
            foreach (KeyValuePair<string, Plot> kvp in dict)
            {
                list.Add(kvp.Value);
            }

            return list;
        }

        /// <summary>
        /// used for experimental purposes.
        /// </summary>
        public static Plot GetRandomNumberArray(int length)
        {
            var rn = new RandomNumber();
            var scores = RandomNumber.GetRandomVector(length, rn);
            var plot = new Plot("Random numbers", scores, 0.25);
            return plot;
        }
    }
}
