// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YamlTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the YamlTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Acoustics.Test.Shared
{
    using System;
    using System.IO;

    using Acoustics.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using YamlDotNet.Core;

    [TestClass]
    public class YamlTests
    {
        private const string TestObjectYaml = @"---
TestFile: C:\Temp\test.tmp
SomeProperty: Hello world
PrivateSetter: 123456
...";

        private const string WrapperDocument = @"
---

InfoA: &BASE
    TestFile: C:\Temp\test.tmp
    SomeProperty: Hello world

InfoB:
    <<: *BASE
    SomeProperty: A different Hello

InfoC:
    <<: *BASE
    TestFile: C:\Temp\a_different_test.tmp

...
";

        private const string ConfigDocument = @"
---
# Summary: Calculates acoustic indices
#
# The csv files this analysis outputs can be used to construct:
#     1. long-duration false-color spectrograms
#     2. a focused stack of zooming false-color spectrograms
#     3. the tiles for zooming false-color spectrograms
#
#

AnalysisName: Towsey.Acoustic
#AnalysisIdealSegmentDuration: units=minutes;     SegmentOverlap: units=seconds;
AnalysisIdealSegmentDuration: 1
SegmentOverlap: 0

# IndexCalculationDuration: units=seconds (default=60 seconds; use 0.2 for zooming spectrogram tiles)
# The Timespan (in seconds) over which summary and spectral indices are calculated
IndexCalculationDuration: 60.0

# BgNoiseNeighbourhood: units=seconds (default IndexCalculationDuration = 60 seconds)
# BG noise for any location is calculated by extending the region of index calculation from 5 seconds before start to 5 sec after end of current index interval.
#    Ten seconds is considered a minimum interval to obtain a reliable estimate of BG noise.
#    The  BG noise interval is not extended beyond start or end of recording segment.
#    Consequently for a 60sec Index calculation duration, the  BG noise is calculated form the 60sec segment only.
BgNoiseNeighbourhood: 5

# FRAME LENGTH. units=samples
# FrameWidth is used without overlap to calculate the spectral indices. Typical value=512
FrameLength: 512

# Resample rate must be 2 X the desired Nyquist
# ResampleRate: 17640
ResampleRate: 22050

#Default values in code are LowFreqBound=500Hz & MidFreqBound=4000
LowFreqBound: 1000
MidFreqBound: 8000

DisplayWeightedIndices: false

# SAVE INTERMEDIARY FILES
SaveIntermediateWavFiles: false
SaveIntermediateCsvFiles: false

# SAVE SONOGRAM DATA FILES FOR SUBSEQUENT ZOOMING SPECTROGRAMS
# Next two parameters are used only when creating images for zooming spectrograms.
# Warning: IndexCalculationDuration must be set = 0.2  when SaveSonogramData = true
# TODO: this option should be refactored out into the spectrogram generation analyzer - currently confusing implementation
SaveSonogramData: false
# Frame step. units=samples
# NOTE: The value for FrameStep is used only when calculating a standard spectrogram within the ZOOMING spectrogram function.
#       FrameStep is NOT used when calculating Summary and Spectral indices.
#       However the FrameStep entry must NOT be deleted from this file. Keep the value for when it is required.
#       The value 441 should NOT be changed because it has been calculated specifically for current ZOOMING spectrogram set-up.
# TODO: this option should be refactored out into the spectrogram generation analyzer - currently confusing implementation
FrameStep: 441

# One-minute spectrograms can be saved in any analysis task.
SaveSonogramImages: false

DisplayCsvImage: false
DoNoiseReduction: true
BgNoiseThreshold: 3.0
SonogramBackgroundThreshold: 4.0
ParallelProcessing: false

# if true an additional set of images will be produced that are tiles
# if true, RequireDateInFilename must be set
TileImageOutput: false

# if true, an unambiguous date time must be provided in the source file's name.
# if true, an exception will be thrown if no such date is found
# if false, and a valid date is still found in file name, it will still be parsed
# supports formats like:
#     prefix_20140101T235959+1000.mp3
#     prefix_20140101T235959+Z.mp3
#     prefix_20140101-235959+1000.mp3
#     prefix_20140101-235959+Z.mp3
RequireDateInFilename: false

IndexPropertiesConfig: './IndexPropertiesConfig.yml'
EventThreshold: 0.2
...
";

        private static readonly YamlTestDataClass TestObject = new YamlTestDataClass
        {
            TestFile = "C:\\Temp\\test.tmp",
            SomeProperty = "Hello world",
        };

        private readonly YamlTestWrapperClass wrapperTestCase = new YamlTestWrapperClass
        {
            InfoA = new YamlTestDataClass { SomeProperty = "Hello world", TestFile = "C:\\Temp\\test.tmp" },
            InfoB = new YamlTestDataClass { SomeProperty = "A different Hello", TestFile = "C:\\Temp\\test.tmp" },
            InfoC = new YamlTestDataClass { SomeProperty = "Hello world", TestFile = "C:\\Temp\\a_different_test.tmp" },
        };

        private FileInfo testDocument;

        [TestMethod]
        public void OurDefaultDeserializerSupportsMergingDocuments()
        {
            var wrapper = Yaml.Deserialize<YamlTestWrapperClass>(this.testDocument);

            Assert.AreEqual(this.wrapperTestCase.InfoA.SomeProperty, wrapper.InfoA.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoB.SomeProperty, wrapper.InfoB.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoC.SomeProperty, wrapper.InfoC.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoA.TestFile, wrapper.InfoA.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoB.TestFile, wrapper.InfoB.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoC.TestFile, wrapper.InfoC.TestFile);
        }

        [TestMethod]
        public void OurDefaultDeserializerSupportsMergingDocumentsAndZio()
        {
            var wrapper = Yaml.Deserialize<YamlTestWrapperClass>(this.testDocument);

            Assert.AreEqual(this.wrapperTestCase.InfoA.SomeProperty, wrapper.InfoA.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoB.SomeProperty, wrapper.InfoB.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoC.SomeProperty, wrapper.InfoC.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoA.TestFile, wrapper.InfoA.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoB.TestFile, wrapper.InfoB.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoC.TestFile, wrapper.InfoC.TestFile);
        }

        [TestMethod]
        public void SerializerCanDecodePrivateSetters()
        {
            YamlTestDataClass testObject;
            using (var stream = new StringReader(TestObjectYaml))
            {
                testObject = Yaml.Deserialize<YamlTestDataClass>(stream);
            }

            Assert.AreEqual("C:\\Temp\\test.tmp", testObject.TestFile);
            Assert.AreEqual("Hello world", testObject.SomeProperty);
            Assert.IsTrue(testObject.PrivateSetter.HasValue);
            Assert.AreEqual(123456, testObject.PrivateSetter.Value);
        }

        [TestMethod]
        public void CanDeserializeNullableEnums()
        {
            // related to https://github.com/aaubry/YamlDotNet/issues/544
            var testCase = @"
A: WAVEFORM
B: ~
C: Spectrogram
";

            // bug in yamldotnet should fail unless our patch is added
            // if not fail, then patch no longer needed
            var exception = Assert.ThrowsException<YamlException>(
                () =>
                {
                    using var stream = new StringReader(testCase);
                    var @default = new YamlDotNet.Serialization.Deserializer();
                    @default.Deserialize<YamlEnumTestClass>(stream);
                });
            Assert.IsInstanceOfType(exception.InnerException, typeof(FormatException));
            StringAssert.Contains( "Input string was not in a correct format.", exception.InnerException.Message);

            using var stream = new StringReader(testCase);
            var actual = Yaml.Deserialize<YamlEnumTestClass>(stream);

            Assert.AreEqual(SpectrogramType.WaveForm, actual.A);
            Assert.AreEqual(null, actual.B);
            Assert.AreEqual(SpectrogramType.Spectrogram, actual.C);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(this.testDocument.FullName);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this.testDocument = TempFileHelper.NewTempFile();
            File.WriteAllText(this.testDocument.FullName, WrapperDocument);
        }

        public class YamlEnumTestClass
        {
                public SpectrogramType A { get; set; }

                public SpectrogramType? B { get; set; }

                public SpectrogramType? C { get; set; }
        }

        public class YamlTestDataClass
        {
            public string SomeProperty { get; set; }

            public string TestFile { get; set; }

            public int? PrivateSetter { get; private set; }
        }

        public class YamlTestWrapperClass
        {
            public YamlTestDataClass InfoA { get; set; }

            public YamlTestDataClass InfoB { get; set; }

            public YamlTestDataClass InfoC { get; set; }
        }
    }
}