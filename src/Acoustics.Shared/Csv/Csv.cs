// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Csv.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Generic methods for reading and writing Csv file.
//   *** DO NOT CHANGE THIS CLASS UNLESS INSTRUCTED TOO ***
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using Fasterflect;
    using log4net;

    using Zio;
    using MissingFieldException = CsvHelper.MissingFieldException;

    /// <summary>
    /// Generic methods for reading and writing Csv file.
    /// .
    /// *** DO NOT CHANGE THIS CLASS UNLESS INSTRUCTED TOO ***
    /// </summary>
    public static class Csv
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TypeConverterCache ConverterCache = new TypeConverterCache();
        private static readonly TypeConverterOptionsCache ConverterOptions = new TypeConverterOptionsCache();

        public static ReadOnlyCollection<ClassMap> ClassMapsToRegister { get; }

        static Csv()
        {
            // Registers CsvHelper type converters that can allow serialization of complex types.
            ConverterCache.AddConverter<ISet<Point>>(new CsvSetPointConverter());

            // ensure dates are always formatted as ISO8601 dates - note: R cannot by default parse proper ISO8601 dates
            ConverterOptions.AddOptions<DateTimeOffset>(new TypeConverterOptions() { DateTimeStyle = DateTimeStyles.RoundtripKind });
            ConverterOptions.AddOptions<DateTime>(new TypeConverterOptions() { DateTimeStyle = DateTimeStyles.RoundtripKind });

            // Find all of our custom class maps
            // initialize and store
            var classMaps = new List<ClassMap>(10);
            foreach (var classMapType in Meta.GetTypesFromQutAssemblies<ClassMap>())
            {
                var instance = Activator.CreateInstance(classMapType) as ClassMap;
                classMaps.Add(instance);
            }

            ClassMapsToRegister = new ReadOnlyCollection<ClassMap>(classMaps);
        }

        public static CsvConfiguration DefaultConfiguration
        {
            get
            {
                // change the defaults here if you want
                // ensure we always use InvariantCulture - only reliable way to serialize data
                // Additionally R can parse invariant representations of Double.Infinity and
                // Double.NaN (whereas it can't in other cultures).
                var settings = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,

                    // acoustic workbench used to output faulty data with padded headers
                    PrepareHeaderForMatch = (x, i) => x.Trim(),

                    CultureInfo = CultureInfo.InvariantCulture,
                    TypeConverterCache = ConverterCache,
                    TypeConverterOptionsCache = ConverterOptions,
                };

                foreach (var classMap in ClassMapsToRegister)
                {
                    settings.RegisterClassMap(classMap);
                }

                return settings;
            }
        }

        /// <summary>
        /// Serialize results to CSV - if you want the concrete type to be serialized you need to ensure
        /// it is downcast before using this method.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="destination">The file to create.</param>
        /// <param name="results">The data to serialize.</param>
        public static void WriteToCsv<T>(FileInfo destination, IEnumerable<T> results)
        {
            Contract.Requires(destination != null);

            // using CSV Helper
            using (var stream = destination.CreateText())
            {
                var writer = new CsvWriter(stream, DefaultConfiguration);
                writer.WriteRecords(results);
            }
        }

        /// <summary>
        /// Read an object from a CSV file.
        /// </summary>
        /// <remarks>
        /// IMPORTANT NOTE:
        /// If I get an exception, how do I tell what line the exception is on?
        /// There is a lot of information held in Exception.Data["CsvHelper"]
        /// </remarks>
        public static IEnumerable<T> ReadFromCsv<T>(
            FileInfo source,
            bool throwOnMissingField = true,
            Action<CsvReader> readerHook = null)
        {
            Contract.Requires(source != null);

            // using CSV Helper
            using (var stream = source.OpenText())
            {
                return ReadFromCsv<T>(readerHook, stream);
            }
        }

        public static IEnumerable<T> ReadFromCsv<T>(
            string csvText,
            bool throwOnMissingField = true,
            Action<CsvReader> readerHook = null)
        {
            Contract.Requires(csvText != null);

            // using CSV Helper
            using (var stream = new StringReader(csvText))
            {
                return ReadFromCsv<T>(readerHook, stream);
            }
        }

        private static IEnumerable<T> ReadFromCsv<T>(Action<CsvReader> readerHook, TextReader stream)
        {
            try
            {
                var configuration = DefaultConfiguration;
                configuration.MissingFieldFound = null;
                var reader = new CsvReader(stream, configuration);

                IEnumerable<T> results = reader.GetRecords<T>();

                var readFromCsv = results.ToArray();

                readerHook?.Invoke(reader);

                return readFromCsv;
            }
            catch (TypeConverterException tce)
            {
                Log.Debug($"Error doing type conversion - dictionary contains {tce.Data.Count} entries. The error was: `{tce.Message}`");

                // The following should no longer be needed since the type converter exception is much more detailed.
                // // The CsvHelper exception messages are particularly unhelpful... let us fix this
                // if (tce.Data.Count > 0)
                // {
                //     var parserData = tce.Data.ToDictDebugString();
                //     var newMessage = tce.Message + Environment.NewLine + parserData;
                //
                //     throw new CsvTypeConverterException(newMessage, tce);
                // }
                //
                // throw;

                throw;

            }
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
                writer.WriteField(i);
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
            if (!reader.ReadHeader())
            {
                rowCount = 0;
                columnCount = 0;
                return new List<T[]>();
            }

            var headers = reader.Context.HeaderRecord;
            if (includeRowIndex && headers[0] != "Index")
            {
                throw new CsvHelperException(reader.Context,"Expected an index header and there was none");
            }

            if (!includeRowIndex && headers[0] == "Index")
            {
                throw new CsvHelperException(reader.Context, "Did not expect an index header and there was one");
            }

            var rowIndex = includeRowIndex ? 1 : 0;
            columnCount = headers.Length - rowIndex;
            var csvRows = new List<T[]>(1440);

            rowCount = 0;
            do
            {
                var row = new T[columnCount];

                for (int i = rowIndex; i <= columnCount; i++)
                {
                    row[i - rowIndex] = reader.GetField<T>(i);
                }

                csvRows.Add(row);
                rowCount++;
            }
            while (reader.Read());

            return csvRows;
        }

        private static T[,] DecodeMatrix<T>(this CsvReader reader, TwoDimensionalArray dimensionality,
            bool includeRowIndex)
        {
            var csvRows = DecodeMatrix<T>(reader, includeRowIndex, out var rowCount, out var columnCount);

            var result = dimensionality == TwoDimensionalArray.None
                ? new T[rowCount, columnCount]
                : new T[columnCount, rowCount];

            for (int r = 0; r < csvRows.Count; r++)
            {
                var row = csvRows[r];
                for (int c = 0; c < row.Length; c++)
                {
                    switch (dimensionality)
                    {
                        case TwoDimensionalArray.None:
                            result[r, c] = row[c];
                            break;
                        case TwoDimensionalArray.Transpose:
                            result[c, r] = row[c];
                            break;
                        case TwoDimensionalArray.Rotate90ClockWise:
                            // note these operations are reversed and look wrong, but because they
                            // are being done on the LHS of assignment, the operations need to be inversed
                            result[c, rowCount - 1 - r] = row[c];
                            break;
                        case TwoDimensionalArray.Rotate90AntiClockWise:
                            // note these operations are reversed and look wrong, but because they
                            // are being done on the LHS of assignment, the operations need to be inversed
                            result[columnCount - 1 - c, r] = row[c];

                            break;
                        default:
                            throw new NotImplementedException("Other dimensionalities not implemented");
                    }
                }
            }

            return result;
        }

        public static void WriteMatrixToCsv<T>(FileInfo destination, T[,] matrix, TwoDimensionalArray dimensionality = TwoDimensionalArray.None)
        {
            Contract.Requires(destination != null);

            // not tested!
            using (var stream = destination.CreateText())
            {
                var writer = new CsvWriter(stream, DefaultConfiguration);

                var transformedMatrix = new TwoDimArrayMapper<T>(matrix, dimensionality);

                EncodeMatrixInner(writer, transformedMatrix, true);
            }
        }

        public static T[,] ReadMatrixFromCsv<T>(FileInfo source, TwoDimensionalArray transform = TwoDimensionalArray.None)
        {
            return ReadMatrixFromCsv<T>(source.ToFileEntry(), transform);
        }

        public static T[,] ReadMatrixFromCsv<T>(FileEntry source, TwoDimensionalArray transform = TwoDimensionalArray.None)
        {
            Contract.Requires(source != null);

            using (var stream = source.OpenText())
            {
                var reader = new CsvReader(stream, DefaultConfiguration);

                return reader.DecodeMatrix<T>(transform, true);
            }
        }

        public static void WriteMatrixToCsv<T>(FileInfo destination, IEnumerable<T[]> matrix)
        {
            Contract.Requires(destination != null);

            // not tested!
            using (var stream = destination.CreateText())
            {
                var writer = new CsvWriter(stream, DefaultConfiguration);

                var transformedMatrix = new EnumerableMapper<T>(matrix);

                EncodeMatrixInner(writer, transformedMatrix, true);
            }
        }

        public static IEnumerable<T[]> ReadMatrixFromCsvAsEnumerable<T>(FileInfo source)
        {
            Contract.Requires(source != null);

            // not tested!
            List<T[]> matrix;
            using (var stream = new StreamReader(source.FullName))
            {
                var reader = new CsvReader(stream, DefaultConfiguration);

                matrix = reader.DecodeMatrix<T>(true, out var rowCount, out var columnCount);
            }

            return matrix;
        }

        public static void WriteMatrixToCsv<T, U>(FileInfo destination, IEnumerable<U> matrix, Func<U, T[]> selector)
        {
            Contract.Requires(destination != null);

            using (var stream = destination.CreateText())
            {
                var writer = new CsvWriter(stream, DefaultConfiguration);

                var transformedMatrix = new ObjectArrayMapper<U, T>(matrix, selector);

                EncodeMatrixInner(writer, transformedMatrix, true);
            }
        }
    }
}