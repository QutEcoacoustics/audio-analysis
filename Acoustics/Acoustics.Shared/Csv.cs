using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Acoustics.Shared.Extensions;
using CsvHelper;
using CsvHelper.Configuration;

namespace Acoustics.Shared
{
    /// <summary>
    /// Generic methods for reading and writing Csv file.
    /// 
    /// *** DO NOT CHANGE THIS CLASS UNLESS INSTRUCTED TOO ***
    /// </summary>
    public static class Csv
    {
        public static CsvConfiguration DefaultConfiguration
        {
            get
            {
                var settings = new CsvConfiguration();

                // change the defaults here if you want

                return settings;
            }
        }

        /// <summary>
        /// Serialise results to CSV - if you want the concrete type to be serialized you need to ensure it is downcast before using this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination"></param>
        /// <param name="results"></param>
        public static void WriteToCsv<T>(FileInfo destination, IEnumerable<T> results)
        {
            // using CSV Helper
            using (var stream = destination.CreateText())
            {
                var writer = new CsvWriter(stream, DefaultConfiguration);
                writer.WriteRecords(results);
            }
        }


        /// <summary>
        /// This has not been tested yet! Contact anthony if you have problems.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> ReadFromCsv<T>(FileInfo source)
        {
            // using CSV Helper

            using (var stream = source.OpenText())
            {
                var reader = new CsvReader(stream, DefaultConfiguration);

                IEnumerable<T> results = reader.GetRecords<T>().ToArray();
                foreach (var result in results)
                {
                    yield return result;
                }
            }
        }

        #region matrix/readers writers

        internal static void Encode2DMatrix<T>(this CsvWriter writer, T[,] matrix, TwoDimensionalArray dimensionality,
            bool includeRowIndex)
        {
            var transformedMatrix = new MatrixMapper<T>(matrix, dimensionality);

            EncodeMatrixInner(writer, transformedMatrix, includeRowIndex);
        }

        internal static void EncodeMatrix<T>(this CsvWriter writer, IEnumerable<T[]> matrix, bool includeRowIndex)
        {
            var transformedMatrix = new MatrixMapper<T>(matrix);

            EncodeMatrixInner(writer, transformedMatrix, includeRowIndex);
        }

        private static void EncodeMatrixInner<T>(this CsvWriter writer, MatrixMapper<T> matrix, bool includeRowIndex)
        {
            int columns = matrix.Columns;

            // write header
            if (includeRowIndex)
            {
                writer.WriteField("Index");
            }
            for (int i = 0; i < columns; i++)
            {
                writer.WriteField("c" + i.ToString("000000"));
            }
            writer.NextRecord();

            // write rows
            foreach (var i in matrix)
            {
                for (int j = 0; j < columns; j++)
                {
                    writer.WriteField(matrix[i, j]);
                }
                writer.NextRecord();
            }
        }

        private static List<T[]> DecodeMatrix<T>(this CsvReader reader, bool includeRowIndex, out int rowCount,
            out int columnCount)
        {
            // read header
            var headers = reader.FieldHeaders;
            if (includeRowIndex && headers[0] != "Index")
            {
                throw new CsvHelperException("Expected an index header and there was none");
            }
            if (!includeRowIndex && headers[0] == "Index")
            {
                throw new CsvHelperException("Did not expect an index header and there was one");
            }

            columnCount = headers.Length;
            var csvRows = new List<T[]>(1440);

            rowCount = 0;
            while (reader.Read())
            {
                var row = new T[columnCount];
                for (int i = includeRowIndex ? 1 : 0; i < columnCount; i++)
                {
                    row[i] = reader.GetField<T>(i);
                }
                csvRows.Add(row);

                rowCount++;
            }

            return csvRows;
        }

        private static T[,] DecodeMatrix<T>(this CsvReader reader, TwoDimensionalArray dimensionality,
            bool includeRowIndex)
        {
            int rowCount;
            int columnCount;
            var csvRows = DecodeMatrix<T>(reader, includeRowIndex, out columnCount, out rowCount);

            var result = dimensionality == TwoDimensionalArray.RowMajor
                ? new T[rowCount, columnCount]
                : new T[columnCount, rowCount];

            for (int i = 0; i < csvRows.Count; i++)
            {
                var row = csvRows[i];
                for (int j = 0; j < row.Length; j++)
                {
                    if (dimensionality == TwoDimensionalArray.RowMajor)
                    {
                        result[i, j] = row[j];
                    }
                    else
                    {
                        result[j, i] = row[j];
                    }
                }
            }
            return result;
        }


        public static void WriteMatrixToCsv<T>(FileInfo destination, T[,] matrix,
            TwoDimensionalArray dimnesionality = TwoDimensionalArray.RowMajor)
        {
            // not tested!
            using (var stream = destination.CreateText())
            {
                var writer = new CsvWriter(stream, DefaultConfiguration);

                writer.Encode2DMatrix(matrix, dimnesionality, true);
            }
        }

        public static T[,] ReadMatrixFromCsv<T>(FileInfo source,
            TwoDimensionalArray dimensionality = TwoDimensionalArray.RowMajor)
        {
            // not tested!
            using (var stream = source.OpenText())
            {
                var reader = new CsvReader(stream, DefaultConfiguration);

                return reader.DecodeMatrix<T>(dimensionality, true);
            }
        }

        public static void WriteMatrixToCsv<T>(FileInfo destination, IEnumerable<T[]> matrix)
        {
            // not tested!
            using (var stream = destination.CreateText())
            {
                var writer = new CsvWriter(stream, DefaultConfiguration);

                writer.EncodeMatrix(matrix, true);
            }
        }

        public static IEnumerable<T[]> ReadMatrixFromCsv<T>(FileInfo source)
        {
            // not tested!
            List<T[]> matrix;
            using (var stream = source.OpenText())
            {
                var reader = new CsvReader(stream, DefaultConfiguration);

                int rowCount;
                int columnCount;
                matrix = reader.DecodeMatrix<T>(true, out rowCount, out columnCount);
            }

            return matrix;
        }

        #endregion
    }
}