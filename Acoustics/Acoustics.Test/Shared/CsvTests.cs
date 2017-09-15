// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CsvTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the CsvTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using CsvHelper;
    using CsvHelper.TypeConversion;
    using Fasterflect;
    using global::AnalysisBase.ResultBases;
    using global::AnalysisPrograms.EventStatistics;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Indices;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

    [TestClass]
    public class CsvTests
    {
        private static readonly double[,] TestMatrix =
            {
                { 1.0, 2.0, 3.0, 4.0 },
                { 5.0, 6.0, 7.0, 8.0 },
                { 9.0, 10.0, 11.0, 12.0 },
                { 13.0, 14.0, 15.0, 16.0 },
                { 17.0, 18.0, 19.0, 20.0 },
            };

        private DirectoryInfo outputDirectory;
        private FileInfo testFile;

        static CsvTests()
        {
            // pump static class initializers - it seems when running multiple tests that sometimes
            // these classes are not discovered.

            AcousticEvent aev = new AcousticEvent();
            ImportedEvent iev = new ImportedEvent();

            var configuration = Csv.DefaultConfiguration;
            IDictionary csvMaps = (IDictionary)configuration.Maps.GetFieldValue("data");
            Debug.WriteLine("initializing static types" + aev + iev + csvMaps.Count);
        }

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();

            this.testFile = PathHelper.GetTempFile(".csv");
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.outputDirectory.Delete();
            this.testFile.Delete();
        }

        [TestMethod]
        public void TestWriteSimpleMatrix()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix);

            var expected = CsvExpectedHelper(
                new[] { 0, 1, 2, 3 },
                new[] { 4, 5, 6, 7 },
                new[] { 8, 9, 10, 11 },
                new[] { 12, 13, 14, 15 },
                new[] { 16, 17, 18, 19 });

            this.AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteSimpleMatrixColumnMajor()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.ColumnMajor);

            var expected = CsvExpectedHelper(
                new[] { 0, 4, 8, 12, 16 },
                new[] { 1, 5, 9, 13, 17 },
                new[] { 2, 6, 10, 14, 18 },
                new[] { 3, 7, 11, 15, 19 });

            this.AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteSimpleMatrixColumnMajorFlipped()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.ColumnMajorFlipped);

            var expected = CsvExpectedHelper(
                new[] { 16, 12, 8, 4, 0 },
                new[] { 17, 13, 9, 5, 1 },
                new[] { 18, 14, 10, 6, 2 },
                new[] { 19, 15, 11, 7, 3 });

            this.AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteSimpleMatrixAlternateName()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Normal);

            var expected = CsvExpectedHelper(
                new[] { 0, 1, 2, 3 },
                new[] { 4, 5, 6, 7 },
                new[] { 8, 9, 10, 11 },
                new[] { 12, 13, 14, 15 },
                new[] { 16, 17, 18, 19 });

            this.AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteSimpleMatrixColumnMajorAlternateName()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Transpose);

            var expected = CsvExpectedHelper(
                new[] { 0, 4, 8, 12, 16 },
                new[] { 1, 5, 9, 13, 17 },
                new[] { 2, 6, 10, 14, 18 },
                new[] { 3, 7, 11, 15, 19 });

            this.AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteSimpleMatrixColumnMajorFlippedAlternateName()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Rotate90ClockWise);

            var expected = CsvExpectedHelper(
                new[] { 16, 12, 8, 4, 0 },
                new[] { 17, 13, 9, 5, 1 },
                new[] { 18, 14, 10, 6, 2 },
                new[] { 19, 15, 11, 7, 3 });

            this.AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteAndReadSimpleMatrix()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.RowMajor);

            Debug.WriteLine(Json.SerialiseToString(TestMatrix, prettyPrint: true));
            Debug.WriteLine("Actual:");
            Debug.WriteLine(Json.SerialiseToString(matrix, true));

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestWriteAndReadSimpleMatrixColumnMajor()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.ColumnMajor);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.ColumnMajor);

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestWriteAndReadSimpleMatrixColumnMajorFlipped()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.ColumnMajorFlipped);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.ColumnMajorFlipped);

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestWriteAndThenReadDifferentOrders()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Rotate90ClockWise);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.RowMajor);

            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestTimeSpanRoundTrip()
        {
            int randomDataCount = 30;
            var random = TestHelpers.Random.GetRandom();

            // we'll create 30 timespans, across 30 orders of magnitude to test all possible versions of timespan
            // encoding
            var data = new CsvTestClass[randomDataCount];
            for (int i = 0; i < data.Length; i++)
            {
                var ticks = Math.Pow(i, 10) * random.NextDouble();
                data[i] = new CsvTestClass {
                    SomeNumber = random.Next(),
                    SomeTimeSpan = TimeSpan.FromTicks((long)ticks),
                };
            }

            var file = new FileInfo("testCsvRoundTrip.csv");
            Csv.WriteToCsv(file, data);

            var actual = Csv.ReadFromCsv<CsvTestClass>(file).ToArray();

            Assert.AreEqual(30, actual.Length);
            for (var i = 0; i < data.Length; i++)
            {
                var expectedRow = data[i];
                var actualRow = actual[i];

                Assert.AreEqual(expectedRow.SomeNumber, actualRow.SomeNumber);
                Assert.AreEqual(expectedRow.SomeTimeSpan.Ticks, actualRow.SomeTimeSpan.Ticks);
            }

            file.Delete();
        }

        [TestMethod]
        public void TestThatCsvDeserializerGivesHumanFriendlyErrors()
        {
            var file = Path.GetRandomFileName().ToFileInfo();

            var testString = @"SomeNumber,SomeTimeSpan
123,123.456";

            File.WriteAllText(file.FullName, testString);

            Exception actual = null;
            CsvTestClass[] data = null;
            try
            {
                data = Csv.ReadFromCsv<CsvTestClass>(file).ToArray();
            }
            catch (Exception ex)
            {
                actual = ex;
            }

            Assert.IsNull(data);
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(CsvTypeConverterException));
            Assert.IsNotNull(actual.InnerException);
            StringAssert.Contains(actual.Message, "Row");
            StringAssert.Contains(actual.Message, "Field Name");
        }

        [TestMethod]
        public void ReaderHookIsExposed()
        {
            var file = Path.GetRandomFileName().ToFileInfo();

            var testString = @"SomeNumber,SomeTimeSpan,A,B,C,D
123,0:0:0.456,1,2,3,4";

            File.WriteAllText(file.FullName, testString);

            string[] headers = null;
            var result = Csv.ReadFromCsv<CsvTestClass>(file, false, (reader) =>
            {
                headers = reader.FieldHeaders;
            });

            var expected = new[] { "SomeNumber", "SomeTimeSpan", "A", "B", "C", "D" };
            CollectionAssert.AreEqual(expected, headers);
        }

        [TestMethod]
        public void TestCsvClassMapsAreAutomaticallyRegistered()
        {
            // add a spot check of well known class maps to ensure the automatic searcher is finding the class maps

            var partialExpected = new[]
            {
                typeof(AcousticEvent.AcousticEventClassMap),
                typeof(ImportedEvent.ImportedEventNameClassMap),
            };

            CollectionAssert.IsSubsetOf(partialExpected, Csv.ClassMapsToRegister.Select(x => x.GetType()).ToArray());
        }

        [TestMethod]
        public void TestAcousticEventClassMap()
        {
            var ae = new AcousticEvent();

            var result = new StringBuilder();
            using (var str = new StringWriter(result))
            {
                var writer = new CsvWriter(str, Csv.DefaultConfiguration);

                writer.WriteRecords(records: new[] { ae });
            }

            var actual = result.ToString();

            foreach (var property in AcousticEvent.AcousticEventClassMap.IgnoredProperties.Except(AcousticEvent.AcousticEventClassMap.RemappedProperties))
            {
                Assert.IsFalse(
                    actual.Contains(property, StringComparison.InvariantCultureIgnoreCase),
                    $"output CSV should not contain text '{property}'.{Environment.NewLine}Actual: {actual}");
            }

            StringAssert.Contains(actual, "EventEndSeconds");
        }

        [TestMethod]
        public void TestImportedEventClassMap()
        {
            string[] names = new[] { "AudioEventId", "audioEventId", "audio_event_id" };

            foreach (var name in names)
            {
                int value = Environment.TickCount;
                string csv = $"{name}{Environment.NewLine}{value}";

                var result = Csv.ReadFromCsv<ImportedEvent>(csv, throwOnMissingField: false).ToArray();

                Assert.AreEqual(1, result.Length);
                Assert.AreEqual(value, result[0].AudioEventId);
            }
        }

        [TestMethod]
        public void TestBaseTypesAreNotSerializedAsArray()
        {
            var exampleIndices = new SummaryIndexValues();
            SummaryIndexValues[] childArray = { exampleIndices, };
            SummaryIndexBase[] baseArray = { exampleIndices, };

            var baseExpected = $@"{nameof(SummaryIndexBase.RankOrder)},{nameof(SummaryIndexBase.FileName)},{nameof(SummaryIndexBase.ResultStartSeconds)},{nameof(SummaryIndexBase.SegmentDurationSeconds)},{nameof(SummaryIndexBase.ResultMinute)}
0,,0,0,0
".NormalizeToCrLf();
            var childExpected = $@"NoFile,ZeroSignal,HighAmplitudeIndex,ClippingIndex,AvgSignalAmplitude,BackgroundNoise,Snr,AvgSnrOfActiveFrames,Activity,EventsPerSecond,HighFreqCover,MidFreqCover,LowFreqCover,AcousticComplexity,TemporalEntropy,EntropyOfAverageSpectrum,AvgEntropySpectrum,EntropyOfVarianceSpectrum,VarianceEntropySpectrum,EntropyOfPeaksSpectrum,EntropyPeaks,EntropyOfCoVSpectrum,ClusterCount,ThreeGramCount,Ndsi,SptDensity,{nameof(SummaryIndexBase.RankOrder)},{nameof(SummaryIndexBase.FileName)},{nameof(SummaryIndexBase.ResultStartSeconds)},{nameof(SummaryIndexBase.SegmentDurationSeconds)},{nameof(SummaryIndexBase.ResultMinute)}
0,0,0,0,-100,-100,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,,0,0,0
".NormalizeToCrLf();

            Csv.WriteToCsv(this.testFile, childArray);

            var childText = File.ReadAllText(this.testFile.FullName);

            Csv.WriteToCsv(this.testFile, baseArray);

            var baseText = File.ReadAllText(this.testFile.FullName);

            Assert.AreNotEqual(childText, baseText);
            Assert.That.StringEqualWithDiff(baseExpected, baseText);
            Assert.That.StringEqualWithDiff(childExpected, childText);
        }

        [TestMethod] public void TestBaseTypesAreSerializedAsEnumerable()
        {
            var exampleIndices = new SummaryIndexValues();
            IEnumerable<SummaryIndexValues> childArray = exampleIndices.AsArray().AsEnumerable();
            IEnumerable<SummaryIndexBase> baseArray = exampleIndices.AsArray().AsEnumerable();

            Csv.WriteToCsv(this.testFile, childArray);

            var childText = File.ReadAllText(this.testFile.FullName);

            Csv.WriteToCsv(this.testFile, baseArray);

            var baseText = File.ReadAllText(this.testFile.FullName);

            Assert.AreEqual(childText, baseText);
        }

        private void AssertCsvEqual(string expected, FileInfo actual)
        {
            var lines = File.ReadAllText(actual.FullName);

            Assert.AreEqual(expected, lines);
            Debug.WriteLine(lines);
            CollectionAssert.AreEqual(expected.ToArray(), lines.ToArray());
        }

        private static string CsvExpectedHelper(params int[][] indexes)
        {
            return "Index," + string.Join(",", indexes[0].Select((s, i) => "c00000" + i)) + Environment.NewLine
                   + string.Join(
                       Environment.NewLine,
                       indexes.Select(
                           (row, rowIndex) => rowIndex + row.Aggregate(string.Empty, (s, i) => s + "," + GetValue(i))))
                   + Environment.NewLine;
        }

        private static string GetValue(int index)
        {
            return TestMatrix[index / TestMatrix.ColumnLength(), index % TestMatrix.ColumnLength()].ToString();
        }

        public class CsvTestClass
        {
            public int SomeNumber { get; set; }

            public TimeSpan SomeTimeSpan { get; set; }
        }
    }
}
