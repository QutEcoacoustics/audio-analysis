// <copyright file="AnalysisCoordinatorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisBase
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Fasterflect;
    using global::AnalysisBase;
    using global::AnalysisBase.Segment;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SmartFormat;
    using TestHelpers;

    [TestClass]
    public class AnalysisCoordinatorTests
    {
        protected DirectoryInfo outputDirectory;

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("Deleting output directory:" + this.outputDirectory.FullName);

            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        public DirectoryInfo TestTemp => this.outputDirectory.Combine("Temp");

        public DirectoryInfo FallbackTemp => this.outputDirectory.Combine("FallbackTemp");

        public DirectoryInfo AnalysisOutput => this.outputDirectory.Combine("Output");

        private class State
        {
            /// <summary>
            /// - {temp} is a supplied temp directory (which can be any valid dir, including {output}
            /// - {tempNull} is an automatically defined temp directory which is used IFF {temp} is NULL
            ///   We use it by patching the private <see cref="AnalysisSettings.AnalysisTempDirectoryFallback"/> field.
            /// </summary>
            private readonly string[] allFiles =
            {
                "{output}",
                "{output}/{fragment}",
                "{output}/{fragment}/{source}_0min.wav",
                "{output}/{fragment}/{source}_1min.wav",
                "{output}/{fragment}/{unique1}",
                "{output}/{fragment}/{unique2}",
                "{output}/{fragment}/{unique1}/{source}_0min.wav",
                "{output}/{fragment}/{unique1}/{source}_1min.wav",
                "{output}/{fragment}/{unique2}/{source}_1min.wav",
                "{temp}",
                "{temp}/{fragment}/{source}_0min.wav",
                "{temp}/{fragment}/{source}_1min.wav",
                "{temp}/{fragment}/{unique1}",
                "{temp}/{fragment}/{unique2}",
                "{temp}/{fragment}/{unique1}/{source}_0min.wav",
                "{temp}/{fragment}/{unique1}/{source}_1min.wav",
                "{temp}/{fragment}/{unique2}/{source}_1min.wav",
                "{tempNull}",
                "{tempNull}/{fragment}/{source}_0min.wav",
                "{tempNull}/{fragment}/{source}_1min.wav",
                "{tempNull}/{fragment}/{unique1}",
                "{tempNull}/{fragment}/{unique2}",
                "{tempNull}/{fragment}/{unique1}/{source}_0min.wav",
                "{tempNull}/{fragment}/{unique2}/{source}_0min.wav",
                "{tempNull}/{fragment}/{unique2}/{source}_1min.wav",
            };

            public State()
            {
                this.ShouldNotExist = new HashSet<string>(this.allFiles);
            }

            public HashSet<string> ShouldExist { get; } = new HashSet<string>();

            public HashSet<string> ShouldNotExist { get; }

            public State Exist(params string[] shouldExist)
            {
                this.ShouldExist.UnionWith(shouldExist);
                this.ShouldNotExist.ExceptWith(shouldExist);
                return this;
            }

            public State NotExist(params string[] shouldNotExist)
            {
                this.ShouldNotExist.UnionWith(shouldNotExist);
                this.ShouldExist.ExceptWith(shouldNotExist);
                return this;
            }
        }

        [TestMethod]
        public void TestNamedDirectoryWorks()
        {
            var analyzer = new DummyAnalyzer(false);
            var settings = analyzer.DefaultSettings;
            settings.AnalysisOutputDirectory = this.outputDirectory;

            var actual = AnalysisCoordinator.GetNamedDirectory(settings.AnalysisOutputDirectory, analyzer);
            var expected = this.outputDirectory.Combine("Ecosounds.TempusSubstitutus");
            Assert.AreEqual(expected.FullName, actual.FullName);
        }

        [TestMethod]
        public void TestNamedDirectoryWorksWithSubFolders()
        {
            var analyzer = new DummyAnalyzer(false);
            var settings = analyzer.DefaultSettings;
            settings.AnalysisOutputDirectory = this.outputDirectory;

            var actual = AnalysisCoordinator.GetNamedDirectory(settings.AnalysisOutputDirectory, analyzer, "a", "b", "c");
            var expected = this.outputDirectory.Combine("Ecosounds.TempusSubstitutus/a/b/c");
            Assert.AreEqual(expected.FullName, actual.FullName);
        }

        [TestMethod]
        public void FailsIfSegmentTooShort()
        {
            // an empty non-existent file
            var source = TempFileHelper.NewTempFile(this.outputDirectory).Touch();

            ISegment<FileInfo>[] segments = { new FileSegment(source, duration: 1.0.Seconds(), sampleRate: 123456) };
            Assert.ThrowsException<AudioRecordingTooShortException>(() =>
            {
                this.TestAnalysisCoordinator(segments);
            });
        }

        [TestMethod]
        public void RemovesShortSegmentsAfterSplitting()
        {
            // an empty non-existent file
            var sourceA = TempFileHelper.NewTempFile(this.outputDirectory).Touch();
            var sourceB = TempFileHelper.NewTempFile(this.outputDirectory).Touch();

            // this segment list would create 4 segments of {60, 5, 60, 5}
            // two of which are invalid because they are too short
            ISegment<FileInfo>[] segments =
            {
                new FileSegment(sourceA, duration: 65.Seconds(), sampleRate: 123456),
                new FileSegment(sourceB, duration: 65.Seconds(), sampleRate: 123456),
            };

            var results = this.TestAnalysisCoordinator(segments);

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual(60, ((SegmentSettings<FileInfo>)results[0].SegmentSettings).Segment.EndOffsetSeconds);
            Assert.AreEqual(
                sourceA.FullName,
                ((SegmentSettings<FileInfo>)results[0].SegmentSettings).Segment.Source.FullName);
            Assert.AreEqual(60, ((SegmentSettings<FileInfo>)results[1].SegmentSettings).Segment.EndOffsetSeconds);
            Assert.AreEqual(
                sourceB.FullName,
                ((SegmentSettings<FileInfo>)results[1].SegmentSettings).Segment.Source.FullName);
        }

        [TestMethod]
        public void RemovesDuplicateSegmentsAfterSplitting()
        {
            // an empty non-existent file
            var sourceA = TempFileHelper.NewTempFile(this.outputDirectory).Touch();

            // this segment list would create 3 segments of {60(A0-60), 60(A60-120), 60(A0-60)}
            // the whole segment requests are not identical before they are split,
            // but after splitting, the first and last segment are identical.
            ISegment<FileInfo>[] segments =
            {
                new FileSegment(sourceA, duration: 120.Seconds(), sampleRate: 123456),
                new FileSegment(sourceA, duration: 120.Seconds(), sampleRate: 123456)
                {
                    SegmentStartOffset = 0.Seconds(),
                    SegmentEndOffset = 60.0.Seconds(),

                },
            };

            var results = this.TestAnalysisCoordinator(segments);

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual(60, ((SegmentSettings<FileInfo>)results[0].SegmentSettings).Segment.EndOffsetSeconds);
            Assert.AreEqual(
                sourceA.FullName,
                ((SegmentSettings<FileInfo>)results[0].SegmentSettings).Segment.Source.FullName);
            Assert.AreEqual(120, ((SegmentSettings<FileInfo>)results[1].SegmentSettings).Segment.EndOffsetSeconds);
            Assert.AreEqual(
                sourceA.FullName,
                ((SegmentSettings<FileInfo>)results[1].SegmentSettings).Segment.Source.FullName);
        }

        [TestMethod]
        public void ShouldRejectIdenticalSegments()
        {
            // an empty non-existent file
            var source = TempFileHelper.NewTempFile(this.outputDirectory).Touch();

            // two segments from the same file, with the same offsets... should throw
            ISegment<FileInfo>[] segments =
            {
                new FileSegment(source, duration: 65.Seconds(), sampleRate: 123456),
                new FileSegment(source, duration: 65.Seconds(), sampleRate: 123456),
            };

            Assert.ThrowsException<InvalidSegmentException>(
                () =>
                {
                    this.TestAnalysisCoordinator(segments);
                },
                "duplicate");
        }

        [TestMethod]
        public void FailsWithInvalidSegment()
        {
            // an empty non-existent file
            var source = TempFileHelper.NewTempFile(this.outputDirectory).Touch();

            var segment = new FileSegment(source, duration: 65.Seconds(), sampleRate: 123456)
            {
                SegmentEndOffset = 0.Seconds(),
                SegmentStartOffset = 25.Seconds(),
            };

            Assert.ThrowsException<InvalidSegmentException>(() =>
            {
                this.TestAnalysisCoordinator(new[] { segment });
            });
        }

        private AnalysisResult2[] TestAnalysisCoordinator(ISegment<FileInfo>[] segments)
        {
            Contract.Requires(segments != null);

            var preparer = new DummySourcePreparer();

            AnalysisCoordinator coordinator = new AnalysisCoordinator(
                preparer,
                saveIntermediateWavFiles: SaveBehavior.Always,
                uniqueDirectoryPerSegment: false,
                isParallel: false);

            IAnalyser2 dummyAnalyzer = new DummyAnalyzer(false);
            var settings = dummyAnalyzer.DefaultSettings;

            settings.AnalysisOutputDirectory = this.outputDirectory;
            settings.AnalysisTempDirectory = this.outputDirectory.Combine("Temp");

            return coordinator.Run(segments, dummyAnalyzer, settings);
        }

        [TestMethod]
        public void Test︳Save︳Unique︳Temp()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                .Exist("{output}", "{temp}"),

                // while processing segment 1, and unique, all should exists
                new State()
                .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{unique1}",
                    "{temp}",
                    "{temp}/{fragment}",
                    "{temp}/{fragment}/{unique1}",
                    "{temp}/{fragment}/{unique1}/{source}_0min.wav"),

                // while processing segment 2, and unique, all should exists ( as well as previous segment
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique2}",
                        "{temp}/{fragment}/{unique2}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav",
                        "{temp}"),

                // end (temp not deleted since we did not create it)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav",
                        "{temp}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Always, unique: true, temp: this.TestTemp, states: states);
        }

        [TestMethod]
        public void Test︳Never︳Unique︳Temp()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                    .Exist("{output}", "{temp}"),

                // while processing segment 1, and unique, all should exist (except wav file in output)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique1}/{source}_0min.wav"),

                // while processing segment 2, and unique, all should exists ( as well as previous segment)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique2}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique2}",
                        "{temp}/{fragment}/{unique2}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique2}",
                        "{temp}"),

                // end (temp not deleted since we did not create it)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique2}",
                        "{temp}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Never, unique: true, temp: this.TestTemp, states: states);
        }

        [TestMethod]
        public void Test︳Save︳Same︳Temp()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                    .Exist("{output}", "{temp}"),

                // while processing segment 1, and not unique, all should exists
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{temp}",
                    "{temp}/{fragment}",
                    "{temp}/{fragment}/{source}_0min.wav"),

                // while processing segment 2, and not unique, all should exists ( as well as previous segment
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{source}_0min.wav",
                    "{temp}",
                    "{temp}/{fragment}",
                    "{temp}/{fragment}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{source}_0min.wav",
                    "{output}/{fragment}/{source}_1min.wav",
                    "{temp}"),

                // end (temp not deleted since we did not create it)
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{source}_0min.wav",
                    "{output}/{fragment}/{source}_1min.wav",
                    "{temp}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Always, unique: false, temp: this.TestTemp, states: states);
        }

        [TestMethod]
        public void Test︳Never︳Same︳Temp()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                    .Exist("{output}", "{temp}"),

                // while processing segment 1, and not unique, all should exist (except wav file in output)
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{temp}", "{temp}/{fragment}/{source}_0min.wav"),

                // while processing segment 2, and not unique, all should exists ( as well as previous segment)
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{temp}", "{temp}/{fragment}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{temp}"),

                // end (temp not deleted since we did not create it)
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{temp}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Never, unique: false, temp: this.TestTemp, states: states);
        }

        [TestMethod]
        public void Test︳Save︳Unique︳Null()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                    .Exist("{output}", "{tempNull}"),

                // while processing segment 1, and unique, all should exists
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{unique1}",
                    "{tempNull}",
                    "{tempNull}/{fragment}",
                    "{tempNull}/{fragment}/{unique1}",
                    "{tempNull}/{fragment}/{unique1}/{source}_0min.wav"),

                // while processing segment 2, and unique, all should exists (as well as previous segment)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{tempNull}",
                        "{tempNull}/{fragment}",
                        "{tempNull}/{fragment}/{unique2}",
                        "{tempNull}/{fragment}/{unique2}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav"),

                // end (temp deleted since we created it)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Always, unique: true, temp: null, states: states);
        }

        [TestMethod]
        public void Test︳Never︳Unique︳Null()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                    .Exist("{output}", "{tempNull}"),

                // while processing segment 1, and unique, all should exist (except wav file in output)
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{output}/{fragment}/{unique1}", "{tempNull}", "{tempNull}/{fragment}/{unique1}", "{tempNull}/{fragment}/{unique1}/{source}_0min.wav"),

                // while processing segment 2, and unique, all should exists (as well as previous segment)
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{unique1}",
                    "{output}/{fragment}/{unique2}",
                    "{tempNull}",
                    "{tempNull}/{fragment}/{unique2}",
                    "{tempNull}/{fragment}/{unique2}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{output}/{fragment}/{unique1}", "{output}/{fragment}/{unique2}"),

                // end (temp deleted since we created it)
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{output}/{fragment}/{unique1}", "{output}/{fragment}/{unique2}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Never, unique: true, temp: null, states: states);
        }

        [TestMethod]
        public void Test︳Save︳Same︳Null()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                    .Exist("{output}", "{tempNull}"),

                // while processing segment 1, and not unique, all should exists
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{tempNull}",
                    "{tempNull}/{fragment}",
                    "{tempNull}/{fragment}/{source}_0min.wav"),

                // while processing segment 2, and not unique, all should exists ( as well as previous segment
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{source}_0min.wav",
                    "{tempNull}",
                    "{tempNull}/{fragment}",
                    "{tempNull}/{fragment}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{source}_0min.wav",
                    "{output}/{fragment}/{source}_1min.wav"),

                // end (temp deleted since we created it)
                new State()
                    .Exist(
                    "{output}",
                    "{output}/{fragment}",
                    "{output}/{fragment}/{source}_0min.wav",
                    "{output}/{fragment}/{source}_1min.wav"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Always, unique: false, temp: null, states: states);
        }

        [TestMethod]
        public void Test︳Never︳Same︳Null()
        {
            var states = new[]
            {
                // before analyze only output directory should exist
                new State()
                    .Exist("{output}", "{tempNull}"),

                // while processing segment 1, and not unique, all should exist (except wav file in output)
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{tempNull}", "{tempNull}/{fragment}", "{tempNull}/{fragment}/{source}_0min.wav"),

                // while processing segment 2, and not unique, all should exists ( as well as previous segment)
                new State()
                    .Exist("{output}", "{output}/{fragment}", "{tempNull}", "{tempNull}/{fragment}", "{tempNull}/{fragment}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist("{output}", "{output}/{fragment}"),

                // end (temp deleted since we created it)
                new State()
                    .Exist("{output}", "{output}/{fragment}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Never, unique: false, temp: null, states: states);
        }

        [TestMethod]
        public void Test︳Save︳Unique︳Output()
        {
            var states = new[]
            {
                // before analyze only output directory should exist (output and temp are the same)
                new State()
                    .Exist("{output}", "{temp}"),

                // while processing segment 1, and unique, all should exist
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique1}/{source}_0min.wav"),

                // while processing segment 2, and unique, all should exists (as well as previous segment)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique1}/{source}_0min.wav",
                        "{temp}/{fragment}/{unique2}",
                        "{temp}/{fragment}/{unique2}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique1}/{source}_0min.wav",
                        "{temp}/{fragment}/{unique2}",
                        "{temp}/{fragment}/{unique2}/{source}_1min.wav"),

                // end (temp not deleted since we did not create it)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{output}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique1}/{source}_0min.wav",
                        "{temp}/{fragment}/{unique2}",
                        "{temp}/{fragment}/{unique2}/{source}_1min.wav"),
            };

            this.TestAnalysisCoordinatorPaths(
                wav: SaveBehavior.Always,
                unique: true,
                temp: this.AnalysisOutput,
                states: states);
        }

        [TestMethod]
        public void Test︳Never︳Unique︳Output()
        {
            var states = new[]
            {
                // before analyze only output directory should exist (output and temp are the same)
                new State()
                    .Exist("{output}", "{temp}"),

                // while processing segment 1, and unique, all should exist (except wav file in output)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique1}/{source}_0min.wav",
                        "{temp}/{fragment}/{unique1}/{source}_0min.wav"),

                // while processing segment 2, and unique, all should exists ( as well as previous segment)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique2}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique2}",
                        "{output}/{fragment}/{unique2}/{source}_1min.wav",
                        "{temp}/{fragment}/{unique2}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique2}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique2}"),

                // end (temp not deleted since we did not create it)
                // (also we can't delete fragment folders since other results may be present)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{unique1}",
                        "{output}/{fragment}/{unique2}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{unique1}",
                        "{temp}/{fragment}/{unique2}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Never, unique: true, temp: this.AnalysisOutput, states: states);
        }

        [TestMethod]
        public void Test︳Save︳Same︳Output()
        {
            var states = new[]
            {
                // before analyze only output directory should exist  (output and temp are the same)
                new State()
                    .Exist("{output}", "{temp}"),

                // while processing segment 1, and not unique, all should exists
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{output}/{fragment}/{source}_0min.wav",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{temp}/{fragment}/{source}_0min.wav"),

                // while processing segment 2, and not unique, all should exists (as well as previous segment)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{output}/{fragment}/{source}_0min.wav",
                        "{output}/{fragment}/{source}_1min.wav",
                        "{temp}/{fragment}/{source}_0min.wav",
                        "{temp}/{fragment}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{output}/{fragment}/{source}_0min.wav",
                        "{output}/{fragment}/{source}_1min.wav",
                        "{temp}/{fragment}/{source}_0min.wav",
                        "{temp}/{fragment}/{source}_1min.wav"),

                // end (temp not deleted since we did not create it)
                // (also we can't delete fragment folders since other results may be present)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{output}/{fragment}/{source}_0min.wav",
                        "{output}/{fragment}/{source}_1min.wav",
                        "{temp}/{fragment}/{source}_0min.wav",
                        "{temp}/{fragment}/{source}_1min.wav"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Always, unique: false, temp: this.AnalysisOutput, states: states);
        }

        [TestMethod]
        public void Test︳Never︳Same︳Output()
        {
            var states = new[]
            {
                // before analyze only output directory should exist (output and temp are the same)
                new State()
                    .Exist("{output}", "{temp}"),

                // while processing segment 1, and not unique, all should exist (except wav file in output)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{output}/{fragment}/{source}_0min.wav",
                        "{temp}/{fragment}/{source}_0min.wav"),

                // while processing segment 2, and not unique, all should exists (but not previous segment)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{temp}",
                        "{temp}/{fragment}",
                        "{output}/{fragment}/{source}_1min.wav",
                        "{temp}/{fragment}/{source}_1min.wav"),

                // after summarize
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{temp}",
                        "{temp}/{fragment}"),

                // end (temp not deleted since we did not create it)
                // (also we can't delete fragment folders since other results may be present)
                new State()
                    .Exist(
                        "{output}",
                        "{output}/{fragment}",
                        "{temp}",
                        "{temp}/{fragment}"),
            };

            this.TestAnalysisCoordinatorPaths(wav: SaveBehavior.Never, unique: false, temp: this.AnalysisOutput, states: states);
        }

        private void TestAnalysisCoordinatorPaths(SaveBehavior wav, bool unique, DirectoryInfo temp, State[] states)
        {
            Contract.Requires(states.Length == 5);

            var preparer = new DummySourcePreparer();

            AnalysisCoordinator coordinator = new AnalysisCoordinator(
                preparer,
                saveIntermediateWavFiles: wav,
                uniqueDirectoryPerSegment: unique,
                isParallel: false);

            // an empty non-existent file
            var source = TempFileHelper.NewTempFile(this.outputDirectory).Touch();

            FileSegment segment = new FileSegment(source, duration: 120.0.Seconds(), sampleRate: 123456);

            var dummyAnalyzer = new DummyAnalyzer(true);
            var settings = dummyAnalyzer.DefaultSettings;

            Debug.WriteLine("Class output directory:" + this.outputDirectory.FullName);

            settings.AnalysisMaxSegmentDuration = 60.Seconds();
            settings.AnalysisOutputDirectory = this.AnalysisOutput;
            settings.AnalysisTempDirectory = temp;
            settings.SetFieldValue("fallbackTempDirectory", this.FallbackTemp.FullName);

            var task = Task.Run(() =>
            {
                var results = coordinator.Run(segment, dummyAnalyzer, settings);

                // summarize is currently called manually
                dummyAnalyzer.SummariseResults(settings, segment, null, null, null, results);
            });

            // set up path strings
            string basename = Path.GetFileNameWithoutExtension(source.Name);
            var paths = new
            {
                output = this.AnalysisOutput.FullName,

                // some tests temp is null, so we want to test that the TestTemp hasn't been used... so we always need
                // to template the path
                temp = (temp ?? this.TestTemp).FullName,
                tempNull = this.FallbackTemp.FullName,
                fragment = "Ecosounds.TempusSubstitutus",
                unique1 = basename + "_000000.00",
                unique2 = basename + "_000060.00",
                source = basename,
            };

            // wait for the analyzer to pause
            while (!dummyAnalyzer.IsPaused)
            {
                Thread.Sleep(0);
            }

            // manually pump the analysis
            // before analyze
            this.AssertFilesAreAsExpected(0, states[0], paths);

            dummyAnalyzer.Pump();

            // segment 1
            this.AssertFilesAreAsExpected(1, states[1], paths);

            dummyAnalyzer.Pump();

            // segment 2
            this.AssertFilesAreAsExpected(2, states[2], paths);

            dummyAnalyzer.Pump();

            // after summarize
            this.AssertFilesAreAsExpected(3, states[3], paths);

            // complete
            dummyAnalyzer.Pump(false);
            task.Wait(1.0.Seconds());

            this.AssertFilesAreAsExpected(4, states[4], paths);

            Assert.IsTrue(task.IsCompleted);
        }

        private void AssertFilesAreAsExpected(int stage, State state, object paths)
        {
            foreach (var file in state.ShouldExist)
            {
                Assert.That.PathExists(Smart.Format(file, paths), $"(stage: {stage}, pre-templated string: \"{file}\")");
            }

            foreach (var file in state.ShouldNotExist)
            {
                Assert.That.PathNotExists(Smart.Format(file, paths), $"(stage: {stage}, pre-templated string: \"{file}\")");
            }
        }
    }
}
