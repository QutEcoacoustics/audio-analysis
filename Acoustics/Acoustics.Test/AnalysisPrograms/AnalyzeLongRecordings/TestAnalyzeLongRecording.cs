// <copyright file="TestAnalyzeLongRecording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.AnalyzeLongRecordings
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using global::AnalysisPrograms.AnalyseLongRecordings;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

    /// <summary>
    /// Test methods for the various standard Sonograms or Spectrograms
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialization format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710
    /// (5) Wherever possible, don't use test assets
    /// </summary>
    [TestClass]
    public class TestAnalyzeLongRecording
    {
        private DirectoryInfo outputDirectory;

        public TestAnalyzeLongRecording()
        {
            // setup logic here
        }

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        /// <summary>
        /// Tests the analysis of an artificial seven minute long recording consisting of five harmonics.
        /// Acoustic indices as calculated from Linear frequency scale spectrogram.
        /// </summary>
        [TestMethod]
        public void TestAnalyzeSr22050Recording()
        {
            int sampleRate = 22050;
            double duration = 420; // signal duration in seconds = 7 minutes
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics, "cos");
            var recordingPath = this.outputDirectory.CombineFile("TemporaryRecording1.wav");
            WavWriter.WriteWavFileViaFfmpeg(recordingPath, recording.WavReader);

            // draw the signal as spectrogram just for debugging purposes
            /*
            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);
            var sonoConfig = new SonogramConfig
            {
                WindowSize = 512,
                WindowOverlap = 0.0,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 2.0,
            };
            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM", freqScale.GridLineLocations);
            var outputImagePath = this.outputDirectory.CombineFile("Signal1_LinearFreqScale.png");
            image.Save(outputImagePath.FullName, ImageFormat.Png);
            */

            var configPath = PathHelper.ResolveConfigFile("Towsey.Acoustic.yml");

            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath,
                Config = configPath,
                Output = this.outputDirectory,
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(arguments);

            var resultsDirectory = this.outputDirectory.Combine("Towsey.Acoustic");
            var listOfFiles = resultsDirectory.EnumerateFiles();

            int count = listOfFiles.Count();
            Assert.AreEqual(33, count);

            var csvCount = listOfFiles.Count(f => f.Name.EndsWith(".csv"));
            Assert.AreEqual(16, csvCount);

            var jsonCount = listOfFiles.Count(f => f.Name.EndsWith(".json"));
            Assert.AreEqual(2, jsonCount);

            var pngCount = listOfFiles.Count(f => f.Name.EndsWith(".png"));
            Assert.AreEqual(15, pngCount);

            var twoMapsImagePath = resultsDirectory.CombineFile("TemporaryRecording__2Maps.png");
            var twoMapsImage = ImageTools.ReadImage2Bitmap(twoMapsImagePath.FullName);

            // image is 7 * 652
            Assert.AreEqual(7, twoMapsImage.Width);
            Assert.AreEqual(632, twoMapsImage.Height);

            // test integrity of BGN file
            var bgnFile = resultsDirectory.CombineFile("TemporaryRecording__Towsey.Acoustic.BGN.csv");
            Assert.AreEqual(34_080, bgnFile.Length);
            var actualByteArray = File.ReadAllBytes(bgnFile.FullName);

            var resourcesDir = PathHelper.ResolveAssetPath("LongDuration");
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\BgnMatrix.LinearScale.bin");

            // uncomment the following line when first produce the array
            // File.WriteAllBytes(expectedSpectrumFile.FullName, actualByteArray);

            // compare actual BGN file with expected file.
            var expectedByteArray = File.ReadAllBytes(expectedSpectrumFile.FullName);
            CollectionAssert.AreEqual(expectedByteArray, actualByteArray);

            // cannot get following line or several variants to work, so resort to the subsequent four lines
            //var bgnArray = Csv.ReadMatrixFromCsv<string[]>(bgnFile);
            var lines = FileTools.ReadTextFile(bgnFile.FullName);
            var secondLine = lines[1].Split(',');
            var subarray = DataTools.Subarray(secondLine, 1, secondLine.Length - 1);
            var array = DataTools.ConvertStringArrayToDoubles(subarray);

            Assert.AreEqual(8, lines.Count);
            Assert.AreEqual(256, array.Length);

            // draw array just to check peaks are in correct places - just for debugging purposes
            var ldsBgnSpectrumFile = this.outputDirectory.CombineFile("Spectrum1.png");
            GraphsAndCharts.DrawGraph(array, "LD BGN SPECTRUM Linear", ldsBgnSpectrumFile);
        }

        /// <summary>
        /// Tests the analysis of an artificial seven minute long recording consisting of five harmonics.
        /// NOTE: The Acoustic indices are calculated from an Octave frequency scale spectrogram.
        /// TODO: There is a bug in this unit test - still working on it.
        /// </summary>
        [TestMethod]
        public void TestAnalyzeSr64000Recording()
        {
            int sampleRate = 64000;
            double duration = 420; // signal duration in seconds = 7 minutes
            // double duration = 240; // signal duration in seconds = 4 minutes
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics, "cos");
            string recordingName = "TemporaryRecording2";
            var recordingPath = this.outputDirectory.CombineFile(recordingName + ".wav");
            WavWriter.WriteWavFileViaFfmpeg(recordingPath, recording.WavReader);

            // draw the signal as spectrogram just for debugging purposes
            // but can only draw a four minute spectrogram when sr=64000 - change duration above.
            /*
            var fst = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            var sonogram = OctaveFreqScale.ConvertRecordingToOctaveScaleSonogram(recording, fst);

            // Get sonogram image and save
            var freqScale = new FrequencyScale(fst);
            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM", freqScale.GridLineLocations);
            var outputImagePath = this.outputDirectory.CombineFile("SignalSpectrogram_OctaveFreqScale.png");
            image.Save(outputImagePath.FullName, ImageFormat.Png);
            */

            // Now need to rewrite the config file with new parameter settings
            var configPath = PathHelper.ResolveConfigFile("Towsey.Acoustic.yml");

            // Convert the dynamic config to IndexCalculateConfig class and merge in the unnecesary parameters.
            //dynamic configuration = Yaml.Deserialise(configPath);
            //IndexCalculateConfig config = IndexCalculateConfig.GetConfig(configuration, false);

            // because of difficulties in dealing with dynamic config files, just edit the text file!!!!!
            var configLines = File.ReadAllLines(configPath.FullName);
            configLines[configLines.IndexOf(x => x.StartsWith("IndexCalculationDuration: "))] = "IndexCalculationDuration: 15.0";
            //configLines[configLines.IndexOf(x => x.StartsWith("BgNoiseBuffer: "))] = "BgNoiseBuffer: 5.0";
            configLines[configLines.IndexOf(x => x.StartsWith("FrequencyScale: Linear"))] = "FrequencyScale: Octave";

            // the is the only octave scale currently functioning for IndexCalculate class
            var freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);
            configLines[configLines.IndexOf(x => x.StartsWith("FrameLength"))] = $"FrameLength: {freqScale.WindowSize}";
            configLines[configLines.IndexOf(x => x.StartsWith("ResampleRate: "))] = "ResampleRate: 64000";

            // write the edited Config file to temporary output directory
            var newConfigPath = this.outputDirectory.CombineFile("Towsey.Acoustic.yml");
            File.WriteAllLines(newConfigPath.FullName, configLines);

            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath,
                Config = newConfigPath,
                Output = this.outputDirectory,
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(arguments);

            var resultsDirectory = this.outputDirectory.Combine("Towsey.Acoustic");
            var listOfFiles = resultsDirectory.EnumerateFiles();

            int count = listOfFiles.Count();
            Assert.AreEqual(20, count);

            var csvCount = listOfFiles.Count(f => f.Name.EndsWith(".csv"));
            Assert.AreEqual(16, csvCount);

            var jsonCount = listOfFiles.Count(f => f.Name.EndsWith(".json"));
            Assert.AreEqual(2, jsonCount);

            var pngCount = listOfFiles.Count(f => f.Name.EndsWith(".png"));
            Assert.AreEqual(2, pngCount);

            // test integrity of BGN file
            var bgnFile = resultsDirectory.CombineFile(recordingName + "__Towsey.Acoustic.BGN.csv");
            Assert.AreEqual(131_013, bgnFile.Length);
            var actualByteArray = File.ReadAllBytes(bgnFile.FullName);

            var resourcesDir = PathHelper.ResolveAssetPath("LongDuration");
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\BgnMatrix.OctaveScale.bin");

            // uncomment the following line when first produce the array
            // File.WriteAllBytes(expectedSpectrumFile.FullName, actualByteArray);
            // compare actual BGN file with expected file.
            var expectedByteArray = File.ReadAllBytes(expectedSpectrumFile.FullName);
            CollectionAssert.AreEqual(expectedByteArray, actualByteArray);

            // cannot get following line or several variants to work, so resort to the subsequent four lines
            //var bgnArray = Csv.ReadMatrixFromCsv<string[]>(bgnFile);
            var lines = FileTools.ReadTextFile(bgnFile.FullName);
            var secondLine = lines[1].Split(',');
            var subarray = DataTools.Subarray(secondLine, 1, secondLine.Length - 1);
            var array = DataTools.ConvertStringArrayToDoubles(subarray);

            Assert.AreEqual(29, lines.Count);
            Assert.AreEqual(256, array.Length);

            // draw array just to check peaks are in correct places - just for debugging purposes
            var ldsBgnSpectrumFile = this.outputDirectory.CombineFile("Spectrum2.png");
            GraphsAndCharts.DrawGraph(array, "LD BGN SPECTRUM Octave", ldsBgnSpectrumFile);

            // ##########################################
            // SECOND part of test is to create the LD spectrograms because they are not created when IndexCalcDuration < 60 seconds
            // first read in the index generation data
            var icdPath = resultsDirectory.CombineFile(recordingName + "__IndexGenerationData.json");
            var indexConfigData = Json.Deserialise<IndexGenerationData>(icdPath);

            var indexPropertiesConfig = PathHelper.ResolveAssetPath("ConfigFiles");

            var ldSpectrogramConfig = new LdSpectrogramConfig();

            // finally read in the dictionary of spectra
            string analysisType = "Towsey.Acoustic";
            var keys = LDSpectrogramRGB.GetArrayOfAvailableKeys();
            var dictionaryOfSpectra = IndexMatrices.ReadCsvFiles(resourcesDir.ToDirectoryInfo(), recordingName + "__" + analysisType, keys);

            Tuple<Image, string>[] images = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                    inputDirectory: resultsDirectory,
                    outputDirectory: resultsDirectory,
                    ldSpectrogramConfig: ldSpectrogramConfig,
                    indexPropertiesConfigPath: indexPropertiesConfig.ToFileInfo(),
                    indexGenerationData: indexConfigData,
                    basename: recordingName,
                    analysisType: analysisType,
                    indexSpectrograms: dictionaryOfSpectra
                    //indexStatistics: indexDistributions
                    //imageChrome: (!tileOutput).ToImageChrome()
                    );

            foreach (var item in images)
            {
                var image = item.Item1;
                var imagePath = resultsDirectory.CombineFile(recordingName + "__" + item.Item2, ".png");
                image.Save(imagePath.FullName);
            }

            var twoMapsImagePath = resultsDirectory.CombineFile(recordingName + "__2Maps.png");
            var twoMapsImage = ImageTools.ReadImage2Bitmap(twoMapsImagePath.FullName);

            // image is 7 * 652
            Assert.AreEqual(7, twoMapsImage.Width);
            Assert.AreEqual(632, twoMapsImage.Height);
        }
    }
}