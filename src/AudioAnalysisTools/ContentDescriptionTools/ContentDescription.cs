// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.Csv;
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
    }
}
