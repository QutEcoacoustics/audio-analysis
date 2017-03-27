// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YamlTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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

    using YamlDotNet.Serialization;

    [TestClass]
    public class YamlTests
    {
        private const string TestObjectYaml = @"---
TestFile: C:\\Temp\\test.tmp
SomeProperty: Hello world
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
#SegmentDuration: units=minutes;     SegmentOverlap: units=seconds;
SegmentDuration: 1
SegmentOverlap: 0

# IndexCalculationDuration: units=seconds (default=60 seconds; use 0.2 for zooming spectrogram tiles)
# The Timespan (in seconds) over which summary and spectral indices are calculated
IndexCalculationDuration: 60.0

# BGNoiseNeighbourhood: units=seconds (default IndexCalculationDuration = 60 seconds)
# BG noise for any location is calculated by extending the region of index calculation from 5 seconds before start to 5 sec after end of current index interval.
#    Ten seconds is considered a minimum interval to obtain a reliable estimate of BG noise.
#    The  BG noise interval is not extended beyond start or end of recording segment.
#    Consequently for a 60sec Index calculation duration, the  BG noise is calculated form the 60sec segment only.
BGNoiseNeighbourhood: 5

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
            var wrapper = Yaml.Deserialise<YamlTestWrapperClass>(this.testDocument);

            Assert.AreEqual(this.wrapperTestCase.InfoA.SomeProperty, wrapper.InfoA.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoB.SomeProperty, wrapper.InfoB.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoC.SomeProperty, wrapper.InfoC.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoA.TestFile, wrapper.InfoA.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoB.TestFile, wrapper.InfoB.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoC.TestFile, wrapper.InfoC.TestFile);
        }

        [TestMethod]
        public void OurDefaultDeserializerSupportsMergingDocumentsDynamic()
        {
            dynamic wrapper = Yaml.Deserialise(this.testDocument);


            Assert.AreEqual(this.wrapperTestCase.InfoA.SomeProperty, (string)wrapper.InfoA.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoB.SomeProperty, (string)wrapper.InfoB.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoC.SomeProperty, (string)wrapper.InfoC.SomeProperty);
            Assert.AreEqual(this.wrapperTestCase.InfoA.TestFile, (string)wrapper.InfoA.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoB.TestFile, (string)wrapper.InfoB.TestFile);
            Assert.AreEqual(this.wrapperTestCase.InfoC.TestFile, (string)wrapper.InfoC.TestFile);
        }

        [TestMethod]
        public void OurDefaultDeserializerSupportDynamic()
        {
            File.WriteAllText(this.testDocument.FullName, ConfigDocument);

            dynamic config = Yaml.Deserialise(this.testDocument);


            Assert.AreEqual(60.0, (double)config.IndexCalculationDuration);
            Assert.AreEqual(1000, (int)config.LowFreqBound);
            Assert.AreEqual(false, (bool)config.SaveIntermediateWavFiles);
            Assert.AreEqual("./IndexPropertiesConfig.yml", (string)config.IndexPropertiesConfig);
            Assert.AreEqual("Towsey.Acoustic", (string)config.AnalysisName);
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

//        [TestMethod]
//        [ExpectedException(typeof(InvalidCastException))]
//        public void TestYamlFileInfoDeserializerFails()
//        {
//            var stringStream = new StringReader(TestObjectYaml);
//
//            using (var stream = stringStream)
//            {
//                var deserializer = new Deserializer();
//                deserializer.Deserialize<YamlTestDataClass>(stream);
//            }
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
//        public void TestYamlFileInfoSerializerFails()
//        {
//            using (var stream = new StringWriter())
//            {
//                var serializer = new Serializer(SerializationOptions.EmitDefaults);
//                serializer.Serialize(stream, TestObject);
//            }
//        }
//
//        [TestMethod]
//        public void TestYamlFileInfoSerializerWithResolver()
//        {
//            var fileInfoResolver = new YamlFileInfoConverter();
//
//            // this functionality is blocked by the yaml library not properly traversing object graphs
//            // see: https://github.com/aaubry/YamlDotNet/issues/103.
//            Assert.Inconclusive();
//            using (var stream = new StringWriter())
//            {
//                var serialiser = new Serializer(SerializationOptions.EmitDefaults);
//                serialiser.RegisterTypeConverter(fileInfoResolver);
//                serialiser.Serialize(stream, TestObject);
//            }
//        }

        public class YamlTestDataClass
        {
            public string SomeProperty { get; set; }

            public string TestFile { get; set; }
        }

        public class YamlTestWrapperClass
        {
            public YamlTestDataClass InfoA { get; set; }

            public YamlTestDataClass InfoB { get; set; }

            public YamlTestDataClass InfoC { get; set; }
        }
    }
}