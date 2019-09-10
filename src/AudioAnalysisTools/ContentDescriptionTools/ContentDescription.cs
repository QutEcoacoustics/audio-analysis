// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using AudioAnalysisTools.ContentDescriptionTools.ContentTypes;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    public class ContentDescription
    {
        /// <summary>
        /// The following min and max bounds are same as those defined in the IndexPropertiesConfig.yml file as of August 2019.
        /// </summary>
        public static Dictionary<string, double[]> IndexValueBounds = new Dictionary<string, double[]>
        {
            ["ACI"] = new[] { 0.4, 0.7 },
            ["ENT"] = new[] { 0.0, 0.6 },
            ["EVN"] = new[] { 0.0, 2.0 },
            ["BGN"] = new[] { -100.0, -30.0 },
            ["PMN"] = new[] { 0.0, 5.5 },
        };

        public static string[] IndexNames { get; } = { "ACI", "ENT", "EVN", "BGN", "PMN" };

        public static List<Plot> ContentDescriptionOfMultipleRecordingFiles(DirectoryInfo[] directories, string[] baseNames)
        {
            // init a list to collect description results
            var completeListOfResults = new List<DescriptionResult>();

            // cycle through the directories
            // WARNING: Assume one-hour duration for each recording
            for (int i = 0; i < directories.Length; i++)
            {
                // read the spectral indices for the current file
                var dictionary = ContentDescription.ReadIndexMatrices(directories[i], baseNames[i]);

                // Draw the index matrices for check/debug purposes
                // var dir1 = new DirectoryInfo(@"C:\Ecoacoustics\Output\ContentDescription");
                // ContentDescription.DrawNormalisedIndexMatrices(dir1, baseName, dictionary);

                // get the rows and do something with them one by one.
                var results = ContentDescription.AnalyseMinutes(dictionary, i * 60); // WARNING: HACK: ASSUME ONE HOUR FILES
                completeListOfResults.AddRange(results);
            }

            var plotDict = ContentDescription.ConvertResultsToPlots(completeListOfResults, 1440, 0);
            var contentPlots = ContentDescription.ConvertPlotDictionaryToPlotList(plotDict);
            contentPlots = SubtractMeanPlusSd(contentPlots);
            return contentPlots;
        }

        /// <summary>
        /// Reads in all the index matrices whose keys are in the above array of IndexNames.
        /// </summary>
        /// <param name="dir">directory containing the index matrices.</param>
        /// <param name="baseName">base name of the files.</param>
        /// <returns>a Dictionary of matrices containing normalised index values.</returns>
        public static Dictionary<string, double[,]> ReadIndexMatrices(DirectoryInfo dir, string baseName)
        {
            var dictionary = new Dictionary<string, double[,]>();

            foreach (string key in IndexNames)
            {
                var indexBounds = IndexValueBounds[key];

                // construct a path to the required matrix
                var path = Path.Combine(dir.FullName, baseName + "__Towsey.Acoustic." + key + ".csv");

                // read in the matrix
                var indexMatrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(path));

                // normalise the matrix values
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
            var key = IndexNames[0];
            var path = Path.Combine(dir.FullName, baseName + "__Towsey.Acoustic." + key + ".csv");

            // read in the matrix and get its dimensions
            var indexMatrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(path));
            var rowCount = indexMatrix.GetLength((0));
            var colCount = indexMatrix.GetLength((1));
            if (rowCount < endMinute)
            {
                throw new ArgumentOutOfRangeException(string.Empty, "Not enough rows in matrix to read the given timespan.");
            }

            // set up the return Matrix
            // indexCount will be number of indices X number of frequency bins
            var indexCount = IndexNames.Length * colCount;
            var opMatrix = new double[minuteSpan, indexCount];

            for (int i = 1; i < IndexNames.Length; i++)
            {
                key = IndexNames[i];
                var indexBounds = IndexValueBounds[key];

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

        public static List<DescriptionResult> AnalyseMinutes(Dictionary<string, double[,]> dictionary, int elapsedMinutes)
        {
            int rowCount = dictionary[ContentDescription.IndexNames[0]].GetLength(0);
            int freqBinCount = dictionary[ContentDescription.IndexNames[0]].GetLength(1);
            var results = new List<DescriptionResult>();

            // over all rows assuming one minute per row.
            for (int i = 0; i < rowCount; i++)
            {
                var oneMinuteOfIndices = GetIndicesForOneMinute(dictionary, i);
                var descriptionResult = new DescriptionResult(elapsedMinutes + i);

                // now send indices to various content searches
                descriptionResult.AddDescription(WindStrong1.GetContent(oneMinuteOfIndices));
                descriptionResult.AddDescription(WindLight1.GetContent(oneMinuteOfIndices));
                descriptionResult.AddDescription(RainLight1.GetContent(oneMinuteOfIndices));

                // yet to do following
                //descriptionResult.AddDescription(RainHeavy1.GetContent(oneMinuteOfIndices));
                //descriptionResult.AddDescription(RainHeavy2.GetContent(oneMinuteOfIndices));

                results.Add(descriptionResult);
            }

            return results;
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
                        var plot = new Plot(name, scores, 0.25);
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

                // normalise the scores to z-scores
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

                    // normalise full scale to 4 SDs.
                    scores[i] /= 4.0;
                }

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
