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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Test.TestHelpers;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using global::AnalysisBase.ResultBases;
    using global::AnalysisPrograms.EventStatistics;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.EventStatistics;
    using global::AudioAnalysisTools.Indices;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            _ = new AcousticEvent();
            _ = new ImportedEvent();
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

            AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteSimpleMatrixRotateAntiClockwise()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Rotate90AntiClockWise);

            var expected = CsvExpectedHelper(
                new[] { 3, 7, 11, 15, 19 },
                new[] { 2, 6, 10, 14, 18 },
                new[] { 1, 5, 9, 13, 17 },
                new[] { 0, 4, 8, 12, 16 });

            AssertCsvEqual(expected, this.testFile);
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

            AssertCsvEqual(expected, this.testFile);
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

            AssertCsvEqual(expected, this.testFile);
        }

        [TestMethod]
        public void TestWriteAndReadSimpleMatrix()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.None);

            Debug.WriteLine(Json.SerializeToString(TestMatrix, prettyPrint: true));
            Debug.WriteLine("Actual:");
            Debug.WriteLine(Json.SerializeToString(matrix, true));

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestWriteAndReadSimpleMatrixColumnMajor()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Transpose);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.Transpose);

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestWriteAndReadSimpleMatrix90Clockwise()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Rotate90ClockWise);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.Rotate90AntiClockWise);

            Debug.WriteLine(Json.SerializeToString(TestMatrix, prettyPrint: true));
            Debug.WriteLine("Actual:");
            Debug.WriteLine(Json.SerializeToString(matrix, true));

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestWriteAndReadSimpleMatrix90AntiClockwise()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Rotate90AntiClockWise);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.Rotate90ClockWise);

            Debug.WriteLine(Json.SerializeToString(TestMatrix, prettyPrint: true));
            Debug.WriteLine("Actual:");
            Debug.WriteLine(Json.SerializeToString(matrix, true));

            CollectionAssert.AreEqual(TestMatrix, matrix);
        }

        [TestMethod]
        public void TestWriteAndThenReadDifferentOrders()
        {
            Csv.WriteMatrixToCsv(this.testFile, TestMatrix, TwoDimensionalArray.Rotate90ClockWise);

            double[,] matrix = Csv.ReadMatrixFromCsv<double>(this.testFile, TwoDimensionalArray.None);

            Debug.WriteLine(Json.SerializeToString(TestMatrix, prettyPrint: true));
            Debug.WriteLine("Actual:");
            Debug.WriteLine(Json.SerializeToString(matrix, true));

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
                data[i] = new CsvTestClass
                {
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
            Assert.IsInstanceOfType(actual, typeof(FormatException));
            Assert.IsInstanceOfType(actual.InnerException, typeof(TypeConverterException));

            //Assert.IsNotNull(actual.InnerException);
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
                headers = reader.Context.HeaderRecord;
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
                (typeof(AcousticEvent), typeof(AcousticEvent.AcousticEventClassMap)),
                (typeof(Oblong), typeof(Oblong.OblongClassMap)),
                (typeof(EventStatistics), typeof(EventStatistics.EventStatisticsClassMap)),
                (typeof(ImportedEvent), typeof(ImportedEvent.ImportedEventNameClassMap)),
            };

            // test reflection is working
            var actual = Meta.GetTypesFromQutAssemblies<ClassMap>().ToArray();
            // Debug.WriteLine("Actual classmaps:\n" + actual.FormatList());
            // Debug.WriteLine("Actual assemblies:\n" + Meta.QutAssemblies.Select(x => x.FullName).FormatList());

            CollectionAssert.AreEquivalent(
                partialExpected.Select(x => x.Item2).ToArray(),
                actual);

            foreach (var (type, classMapType) in partialExpected)
            {
                var mapping = Csv.DefaultConfiguration.Maps[type];
                Assert.IsNotNull(mapping, $"Mapping for type {type} was null");
                Assert.AreEqual(classMapType, mapping.GetType());
            }
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

            //.Except(AcousticEvent.AcousticEventClassMap.RemappedProperties)
            foreach (var property in AcousticEvent.AcousticEventClassMap.IgnoredProperties)
            {
                Assert.IsFalse(
                    actual.Contains(property, StringComparison.InvariantCultureIgnoreCase),
                    $"output CSV should not contain text '{property}'.{Environment.NewLine}Actual: {actual}");
            }

            StringAssert.Contains(actual, "EventEndSeconds");
            StringAssert.Contains(actual, "EventStartSeconds");
            StringAssert.Contains(actual, "LowFrequencyHertz");
            StringAssert.Contains(actual, "HighFrequencyHertz");
            StringAssert.That.NotContains(actual, nameof(Oblong.ColCentroid));
        }

        [TestMethod]
        public void TestImportedEventClassMap()
        {
            string[] names = { "AudioEventId", "audioEventId", "audio_event_id" };

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
            var childExpected = $@"ZeroSignal,HighAmplitudeIndex,ClippingIndex,AvgSignalAmplitude,BackgroundNoise,Snr,AvgSnrOfActiveFrames,Activity,EventsPerSecond,HighFreqCover,MidFreqCover,LowFreqCover,AcousticComplexity,TemporalEntropy,EntropyOfAverageSpectrum,EntropyOfVarianceSpectrum,EntropyOfPeaksSpectrum,EntropyOfCoVSpectrum,ClusterCount,ThreeGramCount,Ndsi,SptDensity,{nameof(SummaryIndexBase.RankOrder)},{nameof(SummaryIndexBase.FileName)},{nameof(SummaryIndexBase.ResultStartSeconds)},{nameof(SummaryIndexBase.SegmentDurationSeconds)},{nameof(SummaryIndexBase.ResultMinute)}
0,0,0,-100,-100,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,,0,0,0
".NormalizeToCrLf();

            Csv.WriteToCsv(this.testFile, childArray);

            var childText = File.ReadAllText(this.testFile.FullName);

            Csv.WriteToCsv(this.testFile, baseArray);

            var baseText = File.ReadAllText(this.testFile.FullName);

            Assert.AreNotEqual(childText, baseText);
            Assert.That.StringEqualWithDiff(baseExpected, baseText);
            Assert.That.StringEqualWithDiff(childExpected, childText);
        }

        /// <summary>
        /// For some inane reason CsvHelper does not downcast to derived types.
        /// </summary>
        [TestMethod]
        public void TestChildTypesAreSerializedWhenWrappedAsEnumerableParentType()
        {
            var exampleIndices = new SummaryIndexValues();
            IEnumerable<SummaryIndexValues> childArray = exampleIndices.AsArray().AsEnumerable();
            IEnumerable<SummaryIndexBase> baseArray = exampleIndices.AsArray().AsEnumerable();

            Csv.WriteToCsv(this.testFile, childArray);

            var childText = File.ReadAllText(this.testFile.FullName);

            Csv.WriteToCsv(this.testFile, baseArray);

            var baseText = File.ReadAllText(this.testFile.FullName);

            Assert.AreNotEqual(childText, baseText);
        }

        /// <summary>
        /// For some inane reason CsvHelper does not downcast to derived types.
        /// </summary>
        [TestMethod]
        public void TestChildTypesAreSerializedWhenWrappedAsEnumerableParentType_AcousticEvent()
        {
            var exampleEvent = new AcousticEvent(100.Seconds(), 15, 4, 100, 3000);
            var exampleEvent2 = new AcousticEvent(100.Seconds(), 15, 4, 100, 3000);
            AcousticEvent[] childArray = { exampleEvent, exampleEvent2 };
            EventBase[] baseArray = { exampleEvent, exampleEvent2 };

            Csv.WriteToCsv(this.testFile, childArray);

            var childText = File.ReadAllText(this.testFile.FullName);

            Csv.WriteToCsv(this.testFile, baseArray);

            var baseText = File.ReadAllText(this.testFile.FullName);

            Assert.AreNotEqual(childText, baseText);
        }

        [TestMethod]
        public void TestInvariantCultureIsUsed()
        {
            var now = new DateTime(1234567891011121314);
            var nowOffset = new DateTimeOffset(1234567891011121314, TimeSpan.FromHours(10));

            var o = new CultureDataTester
            {
                Value = -789123.456,
                Infinity = double.NegativeInfinity,
                Nan = double.NaN,
                Date = now,
                DateOffset = nowOffset,
            };
            Csv.WriteToCsv(
                this.testFile,
                new CultureDataTester[] { o });

            var actual = File.ReadAllText(this.testFile.FullName);
            var expected = $@"{nameof(CultureDataTester.Value)},{nameof(CultureDataTester.Infinity)},{nameof(CultureDataTester.Nan)},{nameof(CultureDataTester.Date)},{nameof(CultureDataTester.DateOffset)}
-789123.456,-Infinity,NaN,3913-03-12T00:31:41.1121314,3913-03-12T00:31:41.1121314+10:00
".NormalizeToCrLf();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInvariantCultureIsUsedMatrix()
        {
            Csv.WriteMatrixToCsv(
                this.testFile,
                new[,] { { -789123.456, double.NegativeInfinity,  double.NaN } });

            var actual = File.ReadAllText(this.testFile.FullName);

            var expected = $@"Index,c000000,c000001,c000002
0,-789123.456,-Infinity,NaN
".NormalizeToCrLf();

            Assert.AreEqual(expected, actual);
        }

        private static void AssertCsvEqual(string expected, FileInfo actual)
        {
            var lines = File.ReadAllText(actual.FullName);

            Assert.That.StringEqualWithDiff(expected, lines);

            //Debug.WriteLine(lines);

            CollectionAssert.AreEqual(expected.ToArray(), lines.ToArray());
        }

        private static string CsvExpectedHelper(params int[][] indexes)
        {
            // per https://tools.ietf.org/html/rfc4180
            const string csvNewline = "\r\n";
            return "Index," + string.Join(",", indexes[0].Select((s, i) => "c00000" + i)) + csvNewline
                   + string.Join(
                       csvNewline,
                       indexes.Select(
                           (row, rowIndex) => rowIndex + row.Aggregate(string.Empty, (s, i) => s + "," + GetValue(i))))
                   + csvNewline;
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

        public class CultureDataTester
        {
            public double Value { get; set; }

            public double Infinity { get; set; }

            public double Nan { get; set; }

            public DateTime Date { get; set; }

            public DateTimeOffset DateOffset { get; set; }
        }
    }
}
