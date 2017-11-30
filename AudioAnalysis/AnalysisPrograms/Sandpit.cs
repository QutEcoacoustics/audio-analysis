// <copyright file="Sandpit.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using AnalyseLongRecordings;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    /// <summary>
    /// Activity Code for this class:= sandpit
    ///
    /// Activity Codes for other tasks to do with spectrograms and audio files:
    ///
    /// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
    /// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
    /// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
    /// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
    /// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
    /// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
    ///
    /// audiofilecheck - Writes information about audio files to a csv file.
    /// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
    /// audiocutter - Cuts audio into segments of desired length and format
    /// createfoursonograms
    /// </summary>
    public class Sandpit
    {
        //public const string imageViewer = @"C:\Windows\system32\mspaint.exe";

        public class Arguments
        {
        }

        /// <summary>
        /// Uncomment the lines in this method for the required analysis.
        /// </summary>
        public static void Dev(Arguments arguments)
        {
            var tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString(CultureInfo.InvariantCulture));

            //Audio2CsvOverOneFile();
            //Audio2CsvOverMultipleFiles();
            //DrawLongDurationSpectrogram();
            ConcatenateIndexFilesAndSpectrograms();
            //TestReadingFileOfSummaryIndices();
            //TestsOfFrequencyScales();
            //TestAnalyseLongRecordingUsingArtificialSignal();
            //TestArbimonSegmentationAlgorithm();
            //TestMatrix3dClass();
            //CubeHelixDrawTestImage();
            //TestChannelIntegrity();
            //TEST_FilterMovingAverage();
            //TestImageProcessing();
            //TestStructureTensor();
            //TestEigenValues();
            //TestWavelets();
            //TestFft2D();
            //TestTernaryPlots();
            //TestDirectorySearchAndFileSearch();
            //ConcatenateMarineImages();
            //ConcatenateImages();
            //ConcatenateTwelveImages();
            //KarlHeinzFrommolt();
            //HerveGlotinMethods();
            //AnalyseFrogDataSet();
            //OTSU_TRHESHOLDING();

            Console.WriteLine("# Finished Sandpit Task!");
        }

        /// <summary>
        /// Call this method to analyse multiple files using audio2csv
        /// </summary>
        public static void Audio2CsvOverMultipleFiles()
        {
            string drive = "G";
            string outputDir = $"{ drive}:\\SensorNetworks\\Output\\IvanCampos\\Indexdata";

            // (1) calculate the indices looping over mulitple files.
            if (false)
            {
                string recordingDir = $"{drive}:\\SensorNetworks\\WavFiles\\IvanCampos";
                string configPath =
                    @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";
                string searchPattern = "*.wav";

                //FileInfo[] csvFiles = IndexMatrices.GetFilesInDirectories(subDirectories, pattern);
                string[] files = Directory.GetFiles(recordingDir, searchPattern);
                LoggedConsole.WriteLine("File Count = " + files.Length);

                for (int i = 0; i < files.Length; i++)
                {
                    string outputDirectory = $"{outputDir}\\{i:d3}";
                    var devArguments = new AnalyseLongRecording.Arguments
                    {
                        Source = files[i].ToFileInfo(),
                        Config = configPath.ToFileInfo(),
                        Output = outputDirectory.ToDirectoryInfo(),
                        MixDownToMono = true,
                    };
                    AnalyseLongRecording.Execute(devArguments);
                }
            }

            // (2) now do the CONCATENATION
            DirectoryInfo[] dataDirs =
            {
                new DirectoryInfo(outputDir),
            };
            string directoryFilter = "Towsey.Acoustic";  // this is a directory filter to locate only the required files
            string opFileStem = "IvanCampos_INCIPO01_20161031";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\IvanCampos";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatSpectrogramFalseColourConfig.yml");

            // start and end dates INCLUSIVE
            var dtoStart = new DateTimeOffset(2016, 10, 31, 0, 0, 0, TimeSpan.Zero);
            var dtoEnd = new DateTimeOffset(2016, 10, 31, 0, 0, 0, TimeSpan.Zero);

            // there are three options for rendering of gaps/missing data: NoGaps, TimedGaps and EchoGaps.
            string gapRendering = "NoGaps";
            var indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml");

            var concatArgs = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = new DirectoryInfo(opPath),
                DirectoryFilter = directoryFilter,
                FileStemName = opFileStem,
                StartDate = dtoStart,
                EndDate = dtoEnd,
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_PMN_SPT,
                ConcatenateEverythingYouCanLayYourHandsOn = false,
                GapRendering = (ConcatMode)Enum.Parse(typeof(ConcatMode), gapRendering),
                TimeSpanOffsetHint = TimeSpan.FromHours(-5), // default = Brisbane time,
                SunRiseDataFile = null,
                DrawImages = true,
                Verbose = true,

                // following used to add in a recognizer score track
                // Used only to get Event Recognizer files - set eventDirs=null if not used
                EventDataDirectories = null,
                EventFilePattern = string.Empty,
            };

            ConcatenateIndexFiles.Execute(concatArgs);
        }

        /// <summary>
        /// TO GET TO HERE audio2csv MUST BE ONLY COMMAND LINE ARGUMENT
        /// If you end up with indices and no images, then, to draw the false-colour spectrograms,
        ///          you must use the activity code "colourspectrogram"
        /// This calls AnalysisPrograms.DrawLongDurationSpectrograms.Execute() to produce LD FC spectrograms from matrices of indices.
        /// See, for example, using the Pillaga Forest data.
        /// If your index calculation duration (ICD) is less than 60s a false-colour spectrogram will not be produced.
        /// Instead you need to do additional step:- Use Action code = ColourSpectrogram
        ///     and enter program through AnalysisPrograms.DrawLongDurationSpectrograms
        /// </summary>
        public static void Audio2CsvOverOneFile()
        {
            // DEV CONFIG OPTIONS
            //C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisPrograms\App.config

            // Use the following paths for the COMMAND LINE
            // COMMAND LINES FOR  ACOUSTIC INDICES, the <audio2csv> task.
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"         "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"           "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"

            // BIG DATA testing
            // "F:\Projects\QUT\qut-svn-trunk\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe" audio2csv -source "F:\Projects\test-audio\cabin_EarlyMorning4_CatBirds20091101-000000.wav" -config "F:\Projects\QUT\qut-svn-trunk\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg" -output "F:\Projects\test-audio\results" -tempdir "F:\Projects\test-audio\results\temp"

            // TESTING for Sheryn Brodie
            // This is a six-hour recording and quite good for debugging calculation of acoustic indices
            //string recordingPath = @"D:\SensorNetworks\WavFiles\Frogs\SherynBrodie\con1To6.wav";
            //string outputPath = @"D:\SensorNetworks\Output\Frogs\TestOfRecognizers-2017August\";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Ecosounds.MultiRecognizer.yml";

            // ACOUSTIC_INDICES_SUNSHINE_COAST SITE1
            // audio2csv  "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SunshineCoast\Acoustic\Site1"
            // audio2csv  "C:\SensorNetworks\WavFiles\SunshineCoast\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SunshineCoast"
            //    Source = @"D:\Anthony escience Experiment data\4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000.mp3".ToFileInfo(),
            //    Config = @"C:\Work\Sensors\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    Output = @"C:\tmp\results\".ToDirectoryInfo()

            //    Source = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036.MP3".ToFileInfo(),
            //    Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    Output = @"C:\SensorNetworks\Output\SunshineCoast\Site1\".ToDirectoryInfo()

            //FOR  MULTI-ANALYSER and CROWS
            //audio2csv  "C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\Test1"

            // TSHERING DEMA BHUTAN RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TsheringDema\WBH12HOURS-D_20160403_120000.wav";
            //    @"Y:\Tshering\WBH_Walaytar\201505 - second deployment\Site2_Waklaytar\24Hours WBH_28032016\WBH12HOURS-D_20160403_120000.wav";
            // string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\WBH12HOURS-N_20160403_064548.wav";
            // string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\Heron_commonCall_downsampled.wav";

            //string outputPath    = @"C:\SensorNetworks\Output\TsheringDema";
            //    @Y:\Results\2016Dec06-094005 - Tshering, Towsey.Indices, ICD=10.0, #133\Tshering\WBH_Walaytar\201505 - second deployment\Site2_Waklaytar\24Hours WBH_28032016

            // BHUTAN: Tsherng Dema: This file contains lots of white heron calls.     WBH12HOURS-N_20160403_000000.wav
            // This is a six-hour recording and quite good for debugging calculation of acoustic indices
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\SecondDeployment\WBH12HOURS-N_20160403_000000.wav";
            //string outputPath = @"C:\SensorNetworks\Output\TsheringDema";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

            // string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.ArdeaInsignis.yml";

            //MARINE
            //string recordingPath = @"C:\SensorNetworks\WavFiles\MarineRecordings\20130318_171500.wav";
            //string configPath  = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.SonogramMarine.yml";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticMarine.yml";
            //string outputPath = @"C:\SensorNetworks\Output\MarineSonograms\Test1";

            //RAIN
            //audio2csv "C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg" "C:\SensorNetworks\Output\Rain"
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Human.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Crow.cfg";

            // CHECKING 16Hz PROBLEMS
            // audio2csv  "C:\SensorNetworks\WavFiles\Frogs\Curramore\CurramoreSelection-mono16kHz.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"
            // audio2csv  "C:\SensorNetworks\WavFiles\16HzRecording\CREDO1_20120607_063200.mp3"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"

            // FALSE-COLOUR SPECTROGRAMS
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Italy.Acoustic.Parallel.yml";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";
            //Output = @"C:\SensorNetworks\Output\FalseColourSpectrograms".ToDirectoryInfo()

            // Brad Law Data
            //string recordingPath = @"D:\SensorNetworks\WavFiles\BradLaw\PillagaForestSite18a\PILLIGA_20121125_052500.wav";
            //string recordingPath = @"D:\SensorNetworks\WavFiles\BradLaw\PillagaForestSite18a\PILLIGA_20121125_194900.wav";
            // next recording contains koala calls
            //string recordingPath = @"D:\SensorNetworks\WavFiles\BradLaw\WilliWilliNP_K48\Data\K48_20161104_211749.wav";
            //string outputPath = @"D:\SensorNetworks\Output\BradLawData\WilliWilliNP";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticHiRes.yml";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\BradLaw\PillagaForestSite18a\PILLIGA_20121125_233900.wav";
            //string outputPath = @"G:\SensorNetworks\Output\BradLaw\Pillaga24";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

            // Ivan Campos recordings
            string recordingPath = @"G:\SensorNetworks\WavFiles\Ivancampos\INCIPO01_20161031_024006_898.wav";
            string outputPath = @"G:\SensorNetworks\Output\IvanCampos\17";
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

            // Test recordings from Wildlife Acoustics demonstrating their compression algorithm
            //string recordingPath = @"D:\SensorNetworks\WildLifeAcoustics\sm4_compression_demo\S4A00068_20160506_063000.wav";
            //string recordingPath = @"D:\SensorNetworks\WildLifeAcoustics\sm4_compression_demo\S4A00068_20160506_063000_new50.wav";
            //string outputPath = @"D:\SensorNetworks\Output\WildLifeAcoustics";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticHiRes.yml";

            // Recording from Andrew Skeoch at Australian Wildlife Audio Recording Group
            //string recordingPath = @"D:\SensorNetworks\WavFiles\BradLaw\TopTrapDam Pilliga 22050 16bit.wav";
            //string outputPath = @"D:\SensorNetworks\Output\BradLawData\AWARG";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticHiRes.yml";

            // Recording from YVONNE - GYMPIE NP - nighttime @ 96kHz listening for bats.
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\GYMPIE_BATS_20170808_180000+1000.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Bats";
            //string configPath = @"C:\SensorNetworks\Output\Bats\config\Towsey.Acoustic.yml";

            // ARTIFICIAL TEST RECORDING
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_4min_artificial.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\Test\Test2";
            //string outputPath    = @"C:\SensorNetworks\Output\FalseColourSpectrograms";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_1min_artificial.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Test\Test";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_7min_artificial.wav";
            //            string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_Farmstay_ECLIPSE3_20121114-060001+1000.wav";
            //            string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";
            //            string outputPath    = @"C:\SensorNetworks\Output\Test\Test2";

            //CHANNEL INTEGRITY
            //string recordingPath = @"Y:\Yvonne\Cooloola\2015Oct04\GympieNP\20151001-064550+1000.wav";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.ChannelIntegrity.yml";
            //string outputPath = @"C:\SensorNetworks\Output\ChannelIntegrity";

            //MARINE
            // Georgia recordings from Cornell
            //string recordingPath = @"C:\SensorNetworks\WavFiles\MarineRecordings\20130318_171500.wav";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticMarine.yml";
            //string outputPath = @"C:\SensorNetworks\Output\MarineSonograms\Test1";

            // Great Barrier Reef (GBR) recordings from Jasco
            //string recordingPath = @"C:\SensorNetworks\WavFiles\MarineRecordings\JascoGBR\AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56-16bit.wav";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticMarine.yml";
            //string outputPath = @"C:\SensorNetworks\Output\MarineJasco";

            // SERF TAGGED RECORDINGS FROM OCT 2010
            // audio2csv  "Z:\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SERF\2013Analysis\13Oct2010"
            //       Source = @"Z:\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3".ToFileInfo(),
            //       Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //       Output = @"C:\SensorNetworks\Output\SERF\AfterRefactoring".ToDirectoryInfo()

            // GROUND PARROT
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\groundParrot_Perigian_TEST.wav";

            // KOALA RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav"; //2 min recording
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3";

            //string outputPath = @"C:\SensorNetworks\Output\KoalaMale\HiRes";

            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.cfg";

            // TUI TERRACE RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\Kiwi\TEST_TUITCE_20091215_220004.wav";

            //string outputPath = @"C:\SensorNetworks\Output\LSKiwi3\Test_Dec2013";
            //string outputPath = @"C:\SensorNetworks\Output\LSKiwi3\Test_07April2014";
            //string outputPath = @"C:\SensorNetworks\Output\Test\TestKiwi";
            //string outputPath = @"C:\SensorNetworks\Output\LSKiwi3\Test18May2017";

            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

            // COMMAND LINE FOR  LITTLE SPOTTED KIWI3
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg" C:\SensorNetworks\Output\LSKiwi3\
            // ACOUSTIC_INDICES_LSK_TUITCE_20091215_220004
            //           Source = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav".ToFileInfo(),
            //           Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //           Output = @"C:\SensorNetworks\Output\LSKiwi3\AfterRefactoring".ToDirectoryInfo()

            // CANETOAD RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\CaneToads_rural1_20.mp3";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\Canetoad\CaneToads_rural1_20.mp3";
            //string recordingPath = @"F:\SensorNetworks\WavFiles\CaneToad\CaneToad Release Call 270213-8.wav";
            //string recordingPath = @"F:\SensorNetworks\WavFiles\CaneToad\UndetectedCalls-2014\KiyomiUndetected210214-1.mp3";

            // Used these to check for Paul.  January 2017.
            //string recordingPath = @"Y:\Groote\2016 March\Emerald River\CardA\Data\EMERALD_20150703_103506.wav";
            //string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.RhinellaMarina.yml";
            //string outputPath    = @"C:\SensorNetworks\Output\Frogs\Canetoad\Rural1";

            // OTHER FROGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LimnodynastesSpecies\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\LitOlong.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\Limnodynastes_convexiusculus\10 Limnodynastes convexiusculus.mp3";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSp\53 Litoria fallax.mp3";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\FrogRecording_2.wav";
            //string recordingPath = @"C:\SensorNetworks\Output\Frogs\FrogPondSamford\FrogPond_Samford_SE_555_20101023-000000.mp3";
            //string recordingPath = @"C:\SensorNetworks\Output\Frogs\FrogPondSamford\FrogPond_Samford_SE_555_20101023-000000_0min.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Frogs_TockAndRachet_GympieDam_JasonsDad.wav";

            //string outputPath    = @"C:\SensorNetworks\Output\Frogs\TestOfHiResIndices-2016August\Test";
            //string outputPath    = @"C:\SensorNetworks\Output\Frogs\CanetoadAcousticIndices";
            //string outputPath    = @"C:\SensorNetworks\Output\Frogs\SamfordTest";

            // ECLIPSE FARMSTAY
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_Farmstay_ECLIPSE3_20121114-060001+1000.wav";
            //string recordingPath = @"Y:\Eclipise 2012\Eclipse\Site 4 - Farmstay\ECLIPSE3_20121115_040001.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Eclipse2012\Farmstay_ECLIPSE3_20121114_060001TEST.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\ZoomImages2";

            // ST BEES KOALA RECORDING TWO HOURS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\WESTKNOLL_20140905-001853+1000.wav";
            //string outputPath = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016";

            /*
            //LEWIN'S RAIL
            // BAC recordings
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC2_20071008-085040.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\BAC\";
            string recordingPath = @"G:\SensorNetworks\WavFiles\LewinsRail\FromLizZnidersic\Data Priory property D.Chapple August 2016\SM304290_0+1_20160824_102329.wav";
            string outputPath    = @"C:\SensorNetworks\Output\LewinsRail\Results2017";
            string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LewiniaPectoralis.yml";
            */

            /*
            //BIRD50 recordings from Herve Glotin
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Glotin-Bird50\AmazonBird50_testing_input\ID1268.wav";
            //string outputPath = @"C:\SensorNetworks\Output\BIRD50\";

            // EASTERN BRISTLE BIRD
            //string recordingPath = @"F:\SensorNetworks\WavFiles\EasternBristlebird\CURRUMBIN_20150529-142503+1000.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\BristleBird";
            */

            // CONFIG FILES ######################################################################################################
            // Use these configs for Summary and Spectral Indices
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Italy.Acoustic.Parallel.yml";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

            // MULTI-RECOGNISER:    Use this config when doing multiple species recognisers
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Ecosounds.MultiRecognizer.yml";

            // Use these config files when looking for individual species.
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LitoriaFallax.yml";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Stark.LitoriaOlong.yml";

            // Use these configs for Call recognition Indices
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Human.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Crow.cfg";

            // DEV CONFIG OPTIONS
            //C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisPrograms\App.config

            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath.ToFileInfo(),
                Config = configPath.ToFileInfo(),
                Output = outputPath.ToDirectoryInfo(),
                MixDownToMono = true,
            };

            // #########  NOTE: All other parameters are set in the <Ecosounds.MultiRecognizer.yml> file
            if (!arguments.Source.Exists)
            {
                LoggedConsole.WriteWarnLine(" >>>>>>>>>>>> WARNING! The Source Recording file cannot be found! This will cause an exception.");
            }

            if (!arguments.Config.Exists)
            {
                LoggedConsole.WriteWarnLine(" >>>>>>>>>>>> WARNING! The Configuration file cannot be found! This will cause an exception.");
            }

            AnalyseLongRecording.Execute(arguments);
        }

        public static void DrawLongDurationSpectrogram()
        {
            // the default ld fc spectrogram config file
            var spectrogramConfigFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramFalseColourConfig.yml";

            // the default index properties file
            string indexPropertiesFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml";

            // INPUT and OUTPUT DIRECTORIES
            //MARINE JASCO TEST
            //var ipdir = @"C:\SensorNetworks\Output\MarineJasco\Towsey.Acoustic";
            //var opdir = @"C:\SensorNetworks\Output\MarineJasco\Towsey.Acoustic\Images";
            //indexPropertiesFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesMarineConfig.yml";

            // INPUT and OUTPUT DIRECTORIES
            //2010 Oct 13th
            //var ipdir = @"C:\SensorNetworks\Output\SERF\2014May06_100720 Indices OCT2010 SERF\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            //var opdir = @"C:\SensorNetworks\Output\SERF\SERF_falseColourSpectrogram\SE";

            //2010 Oct 13th
            //string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014May06-100720 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\RibbonTest";

            //string ipdir = @"G:\SensorNetworks\OutputDataSets\2014May06-100720 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Test_2016Sept";

            //string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct13_SpectralIndices";

            //2010 Oct 14th
            //string ipFileName = "b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct14_SpectralIndices";

            //2010 Oct 15th
            //string ipFileName = "d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct15_SpectralIndices";

            //2010 Oct 16th
            //string ipFileName = "418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct16_SpectralIndices";

            //2010 Oct 17th
            //string ipFileName = "0f2720f2-0caa-460a-8410-df24b9318814_101017-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct17_SpectralIndices";

            // exclude the analysis type from file name i.e. "Indices"
            //string ipFileName = "BYR4_20131029_Towsey.Acoustic";
            //string ipdir = @"Y:\Results\2014Nov28-083415 - False Color, Mt Byron PRA, For Jason\to upload\Mt Byron\PRA\report\joined\BYR4_20131029.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\RibbonTest";

            // false-colour spectrograms
            //string ipFileName = "TEST_Farmstay_ECLIPSE3_20121114-060001+1000"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"

            //string ipdir = @"C:\SensorNetworks\Output\Test\Test2\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test2";
            //string ipdir = @"C:\SensorNetworks\Output\QueenMaryUL\concatenated\frogmary-concatenated\20160117";
            //string opdir = @"C:\SensorNetworks\Output\QueenMaryUL\concatenated";

            // false-colour spectrograms
            //string ipFileName = "Farmstay_ECLIPSE3_20121114_060001TEST"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.60sppx.EclipseFarmstay";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            //string ipFileName = "Farmstay_ECLIPSE3_20121114-060001+1000_TEST"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.60sppx.EclipseFarmstay";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";

            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Farmstay_ECLIPSE3_20121114_060001TEST\Indices\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Farmstay_ECLIPSE3_20121114_060001TEST\Spectrograms";

            // zoomable spectrograms
            //string ipFileName = "TEST_TUITCE_20091215_220004"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";

            //2010 Oct 13th
            //var ipdir = @"C:\SensorNetworks\Output\TsheringDema\Towsey.Acoustic_OLD4";
            //var opdir = @"C:\SensorNetworks\Output\TsheringDema\Towsey.Acoustic";

            //var ipdir = @"C:\SensorNetworks\Output\LSKiwi3\Test18May2017\Towsey.Acoustic";
            //var opdir = @"C:\SensorNetworks\Output\LSKiwi3\Test18May2017";

            // PILLAGA FOREST RECORDINGS OF BRAD LAW - High Resolution analysis
            //string ipdir = @"D:\SensorNetworks\Output\BradLawData\WilliWilliNP\Towsey.Acoustic";
            //string opdir = @"D:\SensorNetworks\Output\BradLawData\WilliWilliNP";
            //spectrogramConfigFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramConfigHiRes.yml";
            //indexPropertiesFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";
            string ipdir = @"D:\SensorNetworks\Output\BradLawData\WilliWilliNP\Towsey.Acoustic";
            string opdir = @"D:\SensorNetworks\Output\BradLawData\WilliWilliNP";
            spectrogramConfigFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramConfigHiRes.yml";
            indexPropertiesFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

            // USA WILD-LIFE ACOUSTICS TEST RECORDINGS OF LOSSY COMPRESSION - High Resolution analysis
            //string ipdir = @"D:\SensorNetworks\Output\WildLifeAcoustics\Towsey.Acoustic";
            //string opdir = @"D:\SensorNetworks\Output\WildLifeAcoustics";
            //spectrogramConfigFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramConfigHiRes.yml";
            //indexPropertiesFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

            // Australian WILD-LIFE ACOUSTICS RECORDING Group - from Andrew Skeoch - High Resolution analysis
            //string ipdir = @"D:\SensorNetworks\Output\BradLawData\AWARG\Towsey.Acoustic";
            //string opdir = @"D:\SensorNetworks\Output\BradLawData\AWARG";
            //spectrogramConfigFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramConfigHiRes.yml";
            //indexPropertiesFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

            // Recording from YVONNE - GYMPIE NP - night time @ 96kHz listening for bats.
            //string ipdir = @"C:\SensorNetworks\Output\Bats\Towsey.Acoustic_icd15s";
            //string opdir = @"C:\SensorNetworks\Output\Bats";
            //spectrogramConfigFile = @"C:\SensorNetworks\Output\Bats\config\SpectrogramFalseColourConfig.yml";
            //indexPropertiesFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml";

            var args = new DrawLongDurationSpectrograms.Arguments
            {
                InputDataDirectory = new DirectoryInfo(ipdir),
                OutputDirectory = new DirectoryInfo(opdir),
                IndexPropertiesConfig = new FileInfo(indexPropertiesFile),
                SpectrogramConfigPath = new FileInfo(spectrogramConfigFile),
            };
            DrawLongDurationSpectrograms.Execute(args);
        }

        /// <summary>
        /// This action item = "concatenateIndexFiles"
        /// </summary>
        public static void ConcatenateIndexFilesAndSpectrograms()
        {
            // set the default values here
            var indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml");
            var timeSpanOffsetHint = TimeSpan.FromHours(10); // default = Brisbane time
            var drawImages = true;

            // start and end dates INCLUSIVE
            DateTimeOffset? dtoStart = null;
            DateTimeOffset? dtoEnd = null;

            // files containing output from event recognizers.
            // Used only to get Event Recognizer files - set eventDirs=null if not used
            DirectoryInfo[] eventDirs = null;
            string eventFilePattern = string.Empty;

            // The drive: local = C; work = G; home = E
            string drive = "C"; // the default

            // SET DEFAULT COLOUR MAPS
            string colorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string colorMap2 = SpectrogramConstants.RGBMap_BGN_PMN_SPT;

            // there are three options for rendering of gaps/missing data: NoGaps, TimedGaps and EchoGaps.
            string gapRendering = "TimedGaps"; // the default
            bool concatenateEverythingYouCanLayYourHandsOn = false; // default is 24-hour blocks

            /*
            // ########################## CONCATENATION of Sarah Lowe's recordings
            // The drive: work = G; home = E
            drive = "G";
            // top level directory
            DirectoryInfo[] dataDirs =
            {
                new DirectoryInfo($"{drive}:\\SensorNetworks\\Output\\ConcatTesting\\TheData"),
            };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "SarahLowe";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\ConcatTesting\\ConcatOutput";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatSpectrogramFalseColourConfig.yml");
            FileInfo sunriseDatafile = null;
            bool concatenateEverythingYouCanLayYourHandsOn = false; // Set false to work in 24-hour blocks only
            dtoStart = new DateTimeOffset(2017, 06, 24, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2017, 07, 02, 0, 0, 0, TimeSpan.Zero);

            // change PMN to POW because PMN not available in these recordings
            colorMap2 = "BGN-PMN-R3D";
            // ########################## END of Sarah Lowe's recordings
            */

            /*
            // ########################## CONCATENATION of Yvonne's recordings of SM2 and SM4
            // The drive: work = G; home = E
            drive = "G";
            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\WavFiles\\TestRecordings\\CompareSM2versusSM4\\MicrophoneTest_AvailaeResult111\\Old_microphone_SM2test"),
            };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "SM2WithOldMics";
            //string opFileStem = "SM4WithNewMics";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\WildLifeAcoustics\\MicrophoneTests";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatSpectrogramFalseColourConfig.yml");
            FileInfo sunriseDatafile = null;
            bool concatenateEverythingYouCanLayYourHandsOn = false; // Set false to work in 24-hour blocks only
            dtoStart = new DateTimeOffset(2016, 08, 09, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2016, 08, 09, 0, 0, 0, TimeSpan.Zero);
            // change PMN to POW because PMN not available in these recordings
            colorMap2 = "BGN-POW-CLS";
            // ########################## END of Yvonne's recordings of SM2 and SM4
            */

            // ########################## CONCATENATION of Pillaga Forest recordings from Brad Law
            // The drive: work = G; home = E
            drive = "G";

            // top level directory AVAILAE JOB #181
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\Output\\BradLaw\\PillagaData"),
            };
            string directoryFilter = "Pillaga*";  // this is a directory filter to locate only the required files
            string opFileStem = "PillagaForest20121125";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\BradLaw";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\Output\\Bats\\config\\SpectrogramFalseColourConfig.yml");
            FileInfo sunriseDatafile = null;

            concatenateEverythingYouCanLayYourHandsOn = true;

            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2012, 08, 08, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2012, 08, 08, 0, 0, 0, TimeSpan.Zero);

            // there are three options for rendering of gaps/missing data: NoGaps, TimedGaps and EchoGaps.
            gapRendering = "EchoGaps";

            // ########################## END of Pillaga Forest recordings

            /*
            // ########################## CONCATENATION of Yvonne's BAT recordings
            // The drive: work = G; home = E
            drive = "G";

            // top level directory AVAILAE JOB #181
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\OutputDataSets\\YvonneBats_Gympie20170906"),
            };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "GympieBATS_2017August";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\Bats\\BatsTestTimeGaps";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\Output\\Bats\\config\\SpectrogramFalseColourConfig.yml");
            FileInfo sunriseDatafile = null;

            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2017, 08, 08, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2017, 08, 08, 0, 0, 0, TimeSpan.Zero);

            // there are three options for rendering of gaps/missing data: NoGaps, TimedGaps and EchoGaps.
            gapRendering = "TimedGaps";

            // ########################## END of Yvonne's BAT recordings
            */

            /*
            // ########################## CONCATENATION of Tshering's Bhutan recordings
            // The drive: work = G; home = E
            drive = "G";

            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\Output\\Bhutan\\DebugConcatenateSourceData"),
            };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "BhutanTest";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\Bhutan\\DebugConcatenateOutput";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\TestConcatenation\\Data\\ConcatSpectrogramFalseColourConfig.yml");
            FileInfo sunriseDatafile = null;
            bool concatenateEverythingYouCanLayYourHandsOn = false; // Set false to work in 24-hour blocks only
            dtoStart = new DateTimeOffset(2017, 02, 03, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2017, 02, 03, 0, 0, 0, TimeSpan.Zero);
            // ########################## END of Tshering's recordings
            */

            /*
            // ########################## CONCATENATION of Kerry Mengersens Data, Puma, South America
            // The drive: work = G; home = E
            drive = "G";

            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\Output\\Mengersen\\NightsA\\Data"),
                                       };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "MengersenNightA";
            //string opFileStem = "MengersenNightB";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\Mengersen\\NightAConcatenated";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Data\\SpectrogramFalseColourConfig.yml");
            timeSpanOffsetHint = TimeSpan.FromHours(-5);
            FileInfo sunriseDatafile = null;
            bool concatenateEverythingYouCanLayYourHandsOn = false; // Set false to work in 24-hour blocks only
            dtoStart = new DateTimeOffset(2016, 08, 20, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2016, 08, 20, 0, 0, 0, TimeSpan.Zero);

            //dtoStart = new DateTimeOffset(2017, 01, 17, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd   = new DateTimeOffset(2017, 01, 24, 02, 23, 29, TimeSpan.Zero);
            //dtoStart = new DateTimeOffset(2016, 08, 21, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd = new DateTimeOffset(2016, 08, 22, 02, 23, 29, TimeSpan.Zero);
            // colour maps for this job
            // colorMap1 = "ACI-ENT-RHZ";
            // colorMap2 = "BGN-POW-SPT";
            // ########################## END of Kerry Mengersens Data, Puma, South America
            */

            /*
            // ########################## CONCATENATION of LIZ Znidersic Recordings, Lewin's rail, Tasmania.
            // The drive: work = G; home = E
            drive = "G";
            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\AvailaeFolders\\LizZnidersic\\Data Tasman Island Unit 2 Mez"),
                                        };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "LizZnidersic_TasmanIsU2Mez";
            string opPath = $"{drive}:\\SensorNetworks\\AvailaeFolders\\LizZnidersic\\Test_IndexDistributions";
            //string opPath = $"{drive}:\\AvailaeFolders\\LizZnidersic\\TEST_missingData"; //was used to put results for testing missing data
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Data\\SpectrogramFalseColourConfig.yml");
            timeSpanOffsetHint = TimeSpan.FromHours(8);
            FileInfo sunriseDatafile = null;
            bool concatenateEverythingYouCanLayYourHandsOn = false; // Set false to work in 24-hour blocks only
            dtoStart = new DateTimeOffset(2015, 11, 09, 0, 0, 0, TimeSpan.Zero);
            dtoEnd   = new DateTimeOffset(2015, 11, 19, 0, 0, 0, TimeSpan.Zero);
            //dtoStart = new DateTimeOffset(2017, 01, 17, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd   = new DateTimeOffset(2017, 01, 24, 02, 23, 29, TimeSpan.Zero);
            //dtoStart = new DateTimeOffset(2016, 08, 21, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd = new DateTimeOffset(2016, 08, 22, 02, 23, 29, TimeSpan.Zero);
            // colour maps for this job
            colorMap1 = "ACI-ENT-RHZ";
            colorMap2 = "BGN-POW-SPT";
            // ########################## END of LIZ Znidersic ARGUMENTS
            */

            /*
            // ################################ CONCATENATE GROOTE DATA
            // This data derived from Groote recordings I brought back from JCU, July 2016.
            // top level directory
            //DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\SensorNetworks\\Output\\Frogs\\Canetoad\\2016Oct28-174219 - Michael, Towsey.Indices, #120\\SD Card A"),
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"G:\\SensorNetworks\\OutputDataSets\\GrooteAcousticIndices_Job120\\SD Card A"),
                                                   };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string testPath = $"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\ExpectedOutput";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\SensorNetworks\\SoftwareTests\\Test_Concatenation\\Data\\TEST_SpectrogramFalseColourConfig.yml");
            timeSpanOffsetHint = TimeSpan.FromHours(9.5);
            FileInfo sunriseDatafile = null;
            string opFileStem = "ConcatGrooteJCU";
            string opPath = $"{drive}:\\SensorNetworks\\Output\\Frogs\\Canetoad\\ConcatGroote_Job120";
            bool concatenateEverythingYouCanLayYourHandsOn = false; // 24 hour blocks only
            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2016, 08, 03, 0, 0, 0, TimeSpan.Zero);
            dtoEnd   = new DateTimeOffset(2016, 08, 03, 0, 0, 0, TimeSpan.Zero);

            eventDirs = new DirectoryInfo[1];
            eventDirs[0] = new DirectoryInfo(@"G:\SensorNetworks\OutputDataSets\GrooteCaneToad_Job120\\SD Card A");
            string eventFilePattern = "*_Towsey.RhinellaMarina.Events.csv";
            */

            //// ########################## MARINE RECORDINGS
            //// top level directory
            ////DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Dec14-094058 - Michael, Towsey.Indices, ICD=30.0, #70\towsey\MarineRecordings\Cornell\2013March-April"),
            ////                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\SensorNetworks\WavFiles\MarineRecordings\Cornell\2013March-April"),
            //                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\SensorNetworks\WavFiles\MarineRecordings\Cornell\2013March-April"),
            //                           };
            //string directoryFilter = "201303";
            //string opPath = @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013March";
            ////string opPath = @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April";
            //dtoStart = new DateTimeOffset(2013, 03, 01, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd   = new DateTimeOffset(2013, 03, 31, 0, 0, 0, TimeSpan.Zero);
            //string opFileStem = "CornellMarine";
            //indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesMarineConfig.yml");

            // ########################## YVONNE'S RECORDINGS
            // top level directory
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48"),
            //                             new DirectoryInfo(@"Y:\Results\2015Aug20-154235 - Yvonne, Indices, ICD=60.0, #50")
            //                           };

            //            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"G:\SensorNetworks\Output\YvonneResults\DataFiles_62_93\2015Nov1"),
            //                                       };

            //below directory was to check a bug - missing 6 hours of recording
            //DirectoryInfo[] dataDirs = {
            //    new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48\Yvonne\Cooloola"),
            //                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Aug06-123245 - Yvonne, Indices, ICD=60.0, #48\Yvonne\Cooloola\2015July26\Woondum3"),
            //                           };
            //string directoryFilter = "20150725-000000+1000.wav";

            //The recording siteName is used as filter pattern to select directories. It is also used for naming the output files
            //            string directoryFilter = "Woondum3";
            //string directoryFilter = "GympieNP";   // this is a directory filter to locate only the required files

            //            string opPath = @"G:\SensorNetworks\Output\YvonneResults\ConcatenatedFiles_62_93";
            //
            //            dtoStart = new DateTimeOffset(2015, 10, 26, 0, 0, 0, TimeSpan.Zero);
            //            dtoEnd   = new DateTimeOffset(2015, 10, 28, 0, 0, 0, TimeSpan.Zero);
            //            string opFileStem = directoryFilter;

            // string sunriseDatafile = @"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv";

            /*
            // ########################## LENN'S RECORDINGS
            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Oct19-173501 - Lenn, Indices, ICD=60.0, #61\Berndt\Lenn\Week 1\Card1302_Box1302"),
                                       };

            // The recording siteName is used as filter pattern to select directories. It is also used for naming the output files
            string directoryFilter = "Towsey.Acoustic"; // this is a directory filter to locate only the required files
            string opFileStem = "Card1302_Box1302";
            string opPath = @"C:\SensorNetworks\Output\LennsResults";

            dtoStart = new DateTimeOffset(2015, 09, 27, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2015, 09, 30, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd   = new DateTimeOffset(2015, 10, 11, 0, 0, 0, TimeSpan.Zero);
    */

            // ########################## STURT RECORDINGS
            // The recording siteName is used as filter pattern to select directories. It is also used for naming the output files

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\SensorNetworks\WavFiles\SturtRecordings\Thompson"), };
            //string directoryFilter = "Thompson";   // this is a directory filter to locate only the required files
            //string opFileStem = "Sturt-Thompson";
            //string opPath = @"F:\SensorNetworks\WavFiles\SturtRecordings\";

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\SensorNetworks\WavFiles\SturtRecordings\Stud"), };
            //string directoryFilter = "Stud";   // this is a directory filter to locate only the required files
            //string opFileStem = "Sturt-Stud";
            //string opPath = @"F:\SensorNetworks\WavFiles\SturtRecordings\";

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\SensorNetworks\WavFiles\SturtRecordings\Sturt1"), };
            //string directoryFilter = "Sturt1";   // this is a directory filter to locate only the required files
            //string opFileStem      = "Sturt-Sturt1";
            //string opPath = @"F:\SensorNetworks\WavFiles\SturtRecordings\";

            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Jul29-110950 - Jason, Towsey.Indices, ICD=60.0, #43\Sturt\2015July\Mistletoe"), };
            //string directoryFilter = "STURT2";          // this is a directory filter to locate only the required files
            //string opFileStem = "Sturt-Mistletoe";
            //string opPath = @"F:\SensorNetworks\Output\Sturt\";

            //dtoStart = new DateTimeOffset(2015, 07, 01, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd = new DateTimeOffset(2015, 07, 06, 0, 0, 0, TimeSpan.Zero);

            // ########################## EDDIE GAME'S PNG RECORDINGS
            // top level directory
            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_32\";
            //string opFileStem = "TNC_Iwarame_20150704_BAR32";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_33\";
            //string opFileStem = "TNC_Iwarame_20150704_BAR33";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_35\";
            //string opFileStem = "TNC_Iwarame_20150704_BAR35";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_7-7-15\BAR\BAR_59\";
            //string opFileStem = "TNC_Iwarame_20150707_BAR59";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_9-7-15\BAR\BAR_79\";
            //string opFileStem = "TNC_Iwarame_20150709_BAR79";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Yavera_8-7-15\BAR\BAR_64\";
            //string opFileStem = "TNC_Yavera_20150708_BAR64";

            //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Musiamunat_3-7-15\BAR\BAR_18\";
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(dataPath) };
            //string opPath   = dataPath;
            //string directoryFilter = "Musimunat";  // this is a directory filter to locate only the required files
            //string opFileStem = "Musimunat_BAR18"; // this should be a unique site identifier
            //string opFileStem = "TNC_Musimunat_20150703_BAR18";

            // the default set of index properties is located in the AnalysisConfig directory.
            //IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo();
            // However the PNG data uses an older set of index properties prior to fixing a bug!
            //FileInfo indexPropertiesConfig = new FileInfo(@"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\IndexPropertiesOLDConfig.yml");

            // ########################## GRIFFITH - SIMON/TOBY FRESH-WATER RECORDINGS
            // top level directory
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings\Site1"),
            //                           };
            //string directoryFilter = "Site2";
            //string opPath = @"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings";
            ////string opPath = @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April";
            //dtoStart = new DateTimeOffset(2015, 07, 09, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd = new DateTimeOffset(2015, 07, 10, 0, 0, 0, TimeSpan.Zero);
            //string opFileStem = "Site1_20150709";
            // ########################## END of GRIFFITH - SIMON/TOBY FRESH-WATER RECORDINGS

            if (!indexPropertiesConfig.Exists)
            {
                LoggedConsole.WriteErrorLine("# indexPropertiesConfig FILE DOES NOT EXIST.");
            }

            // DISCUSS THE FOLLOWING WITH ANTHONY
            // Anthony says we would need to serialise the class. Skip this for the moment.
            // The following location data is used only to draw the sunrise/sunset tracks on images.
            //double? latitude = null;
            //double? longitude = null;
            //var siteDescription = new SiteDescription();
            //siteDescription.SiteName = siteName;
            //siteDescription.Latitude = latitude;
            //siteDescription.Longitude = longitude;

            var args = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = new DirectoryInfo(opPath),
                DirectoryFilter = directoryFilter,
                FileStemName = opFileStem,
                StartDate = dtoStart,
                EndDate = dtoEnd,
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = colorMap1,
                ColorMap2 = colorMap2,
                ConcatenateEverythingYouCanLayYourHandsOn = concatenateEverythingYouCanLayYourHandsOn,
                GapRendering = (ConcatMode)Enum.Parse(typeof(ConcatMode), gapRendering),
                TimeSpanOffsetHint = timeSpanOffsetHint,
                SunRiseDataFile = sunriseDatafile,
                DrawImages = drawImages,
                Verbose = true,

                // following used to add in a recognizer score track
                EventDataDirectories = eventDirs,
                EventFilePattern = eventFilePattern,
            };

            ConcatenateIndexFiles.Execute(args);
        }

        /// <summary>
        /// this is a test to read a file of summary indices.
        /// THis could be made a unit test???
        /// </summary>
        public static void TestReadingFileOfSummaryIndices()
        {
            var summaryIndices = new List<SummaryIndexValues>();
            var file = new FileInfo(@"C:\SensorNetworks\SoftwareTests\TestConcatenation\20160726_073000_Towsey.Acoustic.Indices.csv");

            if (!file.Exists)
            {
                LoggedConsole.WriteErrorLine("File does not exist");
                return;
            }

            var rowsOfCsvFile = Csv.ReadFromCsv<SummaryIndexValues>(file, throwOnMissingField: false);

            // summaryIndices.AddRange(rowsOfCsvFile);

            // track the row counts
            int partialRowCount = rowsOfCsvFile.Count();
        }

        /// <summary>
        /// The following are test methods to confirm that the frequency scale code is working
        /// They are also good tests for the making of standard sonograms.
        /// Did these before I started proper unit testing
        /// </summary>
        public static void TestsOfFrequencyScales()
        {
            // FrequencyScale.TESTMETHOD_LinearFrequencyScaleDefault();
            // FrequencyScale.TESTMETHOD_LinearFrequencyScale();
            // FrequencyScale.TESTMETHOD_OctaveFrequencyScale1();
            // FrequencyScale.TESTMETHOD_OctaveFrequencyScale2();

            //Audio2Sonogram.TESTMETHOD_DrawFourSpectrograms();
            //Oscillations2014.TESTMETHOD_DrawOscillationSpectrogram();

            // The following test methods test various configs of concatenation
            // ConcatenateIndexFiles.TESTMETHOD_ConcatenateIndexFilesTest1();
            // ConcatenateIndexFiles.TESTMETHOD_ConcatenateIndexFilesTest2();
            // ConcatenateIndexFiles.TESTMETHOD_ConcatenateIndexFilesTest3();
            // ConcatenateIndexFiles.TESTMETHOD_ConcatenateIndexFilesTest4();
            // SpectrogramTools.AverageAnArrayOfDecibelValues(null);

            // experiments with clustering the spectra within spectrograms
            // SpectralClustering.TESTMETHOD_SpectralClustering();
            // DspFilters.TestMethod_GenerateSignal1();
            // DspFilters.TestMethod_GenerateSignal2();
            // EventStatisticsCalculate.TestCalculateEventStatistics();
        }

        /// <summary>
        /// Unit test of AnalyseLongRecording() using artificial signal
        /// </summary>
        public static void TestAnalyseLongRecordingUsingArtificialSignal()
        {
            int sampleRate = 22050;
            double duration = 420; // signal duration in seconds = 7 minutes
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestRecording(sampleRate, duration, harmonics, WaveType.Consine);
            var outputDirectory = new DirectoryInfo(@"C:\SensorNetworks\SoftwareTests\TestLongDurationRecordings");
            var recordingPath = outputDirectory.CombineFile("TemporaryRecording.wav");
            WavWriter.WriteWavFileViaFfmpeg(recordingPath, recording.WavReader);
            var configPath = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml");

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
            var outputImagePath = outputDirectory.CombineFile("Signal1_LinearFreqScale.png");
            image.Save(outputImagePath.FullName, ImageFormat.Png);
            */

            var argumentsForAlr = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath,
                Config = configPath,
                Output = outputDirectory,
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(argumentsForAlr);
            var resultsDirectory = outputDirectory.Combine("Towsey.Acoustic");
            var listOfFiles = resultsDirectory.EnumerateFiles();
            int count = listOfFiles.Count();
            var csvCount = listOfFiles.Count(f => f.Name.EndsWith(".csv"));
            var jsonCount = listOfFiles.Count(f => f.Name.EndsWith(".json"));
            var pngCount = listOfFiles.Count(f => f.Name.EndsWith(".png"));

            var twoMapsImagePath = resultsDirectory.CombineFile("TemporaryRecording__2Maps.png");
            var twoMapsImage = ImageTools.ReadImage2Bitmap(twoMapsImagePath.FullName);

            // image is 7 * 652
            int width = twoMapsImage.Width;
            int height = twoMapsImage.Height;

            // test integrity of BGN file
            var bgnFile = resultsDirectory.CombineFile("TemporaryRecording__Towsey.Acoustic.BGN.csv");
            var bgnFileSize = bgnFile.Length;

            // cannot get following line or several variants to work, so resort to the subsequent four lines
            //var bgnArray = Csv.ReadMatrixFromCsv<string[]>(bgnFile);
            var lines = FileTools.ReadTextFile(bgnFile.FullName);
            var lineCount = lines.Count;
            var secondLine = lines[1].Split(',');
            var subarray = DataTools.Subarray(secondLine, 1, secondLine.Length - 2);
            var array = DataTools.ConvertStringArrayToDoubles(subarray);
            var columnCount = array.Length;

            // draw array just to check peaks are in correct places.
            var normalisedIndex = DataTools.normalise(array);
            var image2 = GraphsAndCharts.DrawGraph("LD BGN SPECTRUM", normalisedIndex, 100);
            var ldsBgnSpectrumFile = outputDirectory.CombineFile("Spectrum2.png");
            image2.Save(ldsBgnSpectrumFile.FullName);
        }

        /// <summary>
        /// experiments with Mitchell-Aide ARBIMON segmentation algorithm
        /// Three steps: (1) Flattening spectrogram by subtracting the median bin value from each freq bin.
        ///              (2) Recalculate the spectrogram using local range. Trim off the 5 percentiles.
        ///              (3) Set a global threshold.
        /// </summary>
        public static void TestArbimonSegmentationAlgorithm()
        {
            var outputPath = @"G:\SensorNetworks\Output\temp\AEDexperiments";
            var outputDirectory = new DirectoryInfo(outputPath);
            string recordingPath = @"G:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            AudioRecording recording = new AudioRecording(recordingPath);
            var recordingDuration = recording.WavReader.Time;

            const int frameSize = 1024;
            double windowOverlap = 0.0;
            NoiseReductionType noiseReductionType = SNR.KeyToNoiseReductionType("FlattenAndTrim");
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                //set default values - ignore those set by user
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                NoiseReductionType = noiseReductionType,
                NoiseReductionParameter = 0.0,
            };

            var aedConfiguration = new Aed.AedConfiguration
            {
                //AedEventColor = Color.Red;
                //AedHitColor = Color.FromArgb(128, AedEventColor),
                // This stops AED Wiener filter and noise removal.
                NoiseReductionType = noiseReductionType,
                //BgNoiseThreshold   = 3.5
                IntensityThreshold = 20.0,
                SmallAreaThreshold = 100,
            };

            double[] thresholdLevels = { 30.0, 25.0, 20.0, 15.0, 10.0, 5.0 };
            var imageList = new List<Image>();

            foreach (double th in thresholdLevels)
            {
                aedConfiguration.IntensityThreshold = th;
                var sonogram = (BaseSonogram)new SpectrogramStandard(sonoConfig, recording.WavReader);
                AcousticEvent[] events = Aed.CallAed(sonogram, aedConfiguration, TimeSpan.Zero, recordingDuration);
                LoggedConsole.WriteLine("AED # events: " + events.Length);

                //cluster events
                var clusters = AcousticEvent.ClusterEvents(events);
                AcousticEvent.AssignClusterIds(clusters);

                // see line 415 of AcousticEvent.cs for drawing the cluster ID into the sonogram image.

                var image = Aed.DrawSonogram(sonogram, events);
                imageList.Add(image);
            }

            var compositeImage = ImageTools.CombineImagesVertically(imageList);
            var debugPath = FilenameHelpers.AnalysisResultPath(outputDirectory, recording.BaseName, "AedExperiment_ThresholdStack", "png");
            compositeImage.Save(debugPath);
        }

        /// <summary>
        /// construct 3Dimage of audio
        /// </summary>
        public static void TestMatrix3dClass()
        {
            //TowseyLibrary.Matrix3D.TestMatrix3dClass();
            LdSpectrogram3D.Main(null);
        }

        /// <summary>
        /// do test of SNR calculation
        /// </summary>
        public static void CubeHelixDrawTestImage()
        {
            CubeHelix.DrawTestImage();
        }

        /// <summary>
        /// TEST TO DETERMINE whether one of the signal channels has microphone problems due to rain or whatever.
        /// </summary>
        public static void TestChannelIntegrity()
        {
            ChannelIntegrity.Execute(null);
        }

        /// <summary>
        /// TEST FilterMovingAverage
        /// </summary>
        public static void TEST_FilterMovingAverage()
        {
            // do test of new moving average method
            DataTools.TEST_FilterMovingAverage();
        }

        /// <summary>
        /// Method description
        /// </summary>
        public static void TestImageProcessing()
        {
            ImageTools.TestCannyEdgeDetection();

            //HoughTransform.Test1HoughTransform();
            HoughTransform.Test2HoughTransform();

            // call SURF image Feature extraction
            // SURFFeatures.SURF_TEST();
            SURFAnalysis.Main(null);
        }

        /// <summary>
        /// Test1StructureTensor
        /// </summary>
        public static void TestStructureTensor()
        {
            // used to test structure tensor code.
            StructureTensor.Test1StructureTensor();
            StructureTensor.Test2StructureTensor();
        }

        /// <summary>
        /// TestEigenValues
        /// </summary>
        public static void TestEigenValues()
        {
            // used to caluclate eigen values and singular valuse
            SvdAndPca.TestEigenValues();
        }

        /// <summary>
        /// TestWavelets
        /// </summary>
        public static void TestWavelets()
        {
            // test examples of wavelets
            // WaveletPacketDecomposition.ExampleOfWavelets_1();
            WaveletTransformContinuous.ExampleOfWavelets_1();
        }

        /// <summary>
        /// do 2D-FFT of an image.
        /// </summary>
        public static void TestFft2D()
        {
            FFT2D.TestFFT2D();
        }

        /// <summary>
        /// testing TERNARY PLOTS using spectral indices
        /// </summary>
        public static void TestTernaryPlots()
        {
            //string[] keys = { "BGN", "POW", "EVN"};
            string[] keys = { "ACI", "ENT", "EVN" };

            FileInfo[] indexFiles = { new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[0]+".csv"),
                new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[1]+".csv"),
                new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[2]+".csv"),
            };
            FileInfo opImage = new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622_TernaryPlot.png");

            var matrixDictionary = IndexMatrices.ReadSummaryIndexFiles(indexFiles, keys);

            string indexPropertiesConfigPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults" + @"\IndexPropertiesConfig.yml";
            FileInfo indexPropertiesConfigFileInfo = new FileInfo(indexPropertiesConfigPath);
            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfigFileInfo);
            dictIP = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIP);

            foreach (string key in keys)
            {
                IndexProperties indexProperties = dictIP[key];
                double min = indexProperties.NormMin;
                double max = indexProperties.NormMax;
                matrixDictionary[key] = MatrixTools.NormaliseInZeroOne(matrixDictionary[key], min, max);

                //matrix = MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter); // to de-demphasize the background small values
            }

            Image image = TernaryPlots.DrawTernaryPlot(matrixDictionary, keys);
            image.Save(opImage.FullName);
        }

        /// <summary>
        /// testing directory search and file search
        /// </summary>
        public static void TestDirectorySearchAndFileSearch()
        {
            string[] topLevelDirs =
            {
                @"C:\temp\DirA",
                @"C:\temp\DirB",
            };

            string sitePattern = "Subdir2";
            string dayPattern = "F2*.txt";

            List<string> dirList = new List<string>();
            foreach (string dir in topLevelDirs)
            {
                string[] dirs = Directory.GetDirectories(dir, sitePattern, SearchOption.AllDirectories);
                dirList.AddRange(dirs);
            }

            List<FileInfo> fileList = new List<FileInfo>();
            foreach (string subdir in topLevelDirs)
            {
                var files = IndexMatrices.GetFilesInDirectory(subdir, dayPattern);

                fileList.AddRange(files);
            }

            Console.WriteLine("The number of directories is {0}.", dirList.Count);
            foreach (string dir in dirList)
            {
                Console.WriteLine(dir);
            }

            Console.WriteLine("The number of files is {0}.", fileList.Count);
            foreach (var file in fileList)
            {
                Console.WriteLine(file.FullName);
            }
        }

        /// <summary>
        /// Concatenate marine spectrogram ribbons and add tidal info if available.
        /// </summary>
        public static void ConcatenateMarineImages()
        {
            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013March\CornellMarine"),
                new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April\CornellMarine"),
            };

            DirectoryInfo outputDirectory = new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms");
            string title = "Marine Spectrograms - 15km off Georgia Coast, USA.    Day 1= 01/March/2013      (Low tide=white; High tide=lime)";
            //indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesMarineConfig.yml");

            //string match = @"CornellMarine_*__ACI-ENT-EVN.SpectralRibbon.png";
            //string opFileStem = "CornellMarine.ACI-ENT-EVN.SpectralRibbon.2013MarchApril";

            string match = @"CornellMarine_*__BGN-POW-EVN.SpectralRibbon.png";
            string opFileStem = "CornellMarine.BGN-POW-EVN.SpectralRibbon.2013MarchApril";

            FileInfo tidalDataFile = new FileInfo(@"C:\SensorNetworks\OutputDataSets\GeorgiaTides2013.txt");
            //SunAndMoon.SunMoonTides[] tidalInfo = null;
            SunAndMoon.SunMoonTides[] tidalInfo = SunAndMoon.ReadGeorgiaTidalInformation(tidalDataFile);

            ConcatenateIndexFiles.ConcatenateRibbonImages(dataDirs, match, outputDirectory, opFileStem, title, tidalInfo);
        }

        /// <summary>
        /// Concatenate images horizontally or vertically.
        /// </summary>
        public static void ConcatenateImages()
        {
            // Concatenate three images for Dan Stowell.
            //var imageDirectory = new DirectoryInfo(@"H:\Documents\SensorNetworks\MyPapers\2016_QMUL_SchoolMagazine");
            //string fileName1 = @"TNC_Musiamunat_20150702_BAR10__ACI-ENT-EVNCropped.png";
            //string fileName2 = @"GympieNP_20150701__ACI-ENT-EVN.png";
            //string fileName3 = @"Sturt-Mistletoe_20150702__ACI-ENT-EVN - Corrected.png";
            //string opFileName = string.Format("ThreeLongDurationSpectrograms.png");

            // Concatenate two iimages for Paul Roe.
            var imageDirectory = new DirectoryInfo(@"D:\SensorNetworks\Output\WildLifeAcoustics");
            string fileName1 = @"S4A00068_20160506_063000__SummaryIndices.png";
            string fileName2 = @"S4A00068_20160506_063000_new50__SummaryIndices.png";
            string opFileName = "WildLifeAcoustics_TestLossyCompression3.png";

            var image1Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName1));
            var image2Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName2));
            //var image3Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName3));

            var imageList = new List<Image>();
            imageList.Add(Image.FromFile(image1Path.FullName));
            imageList.Add(Image.FromFile(image2Path.FullName));
            //imageList.Add(Image.FromFile(image3Path.FullName));

            Image combinedImage = ImageTools.CombineImagesVertically(imageList);

            //var combinedImage = ImageTools.CombineImagesInLine(imageList);

            combinedImage.Save(Path.Combine(imageDirectory.FullName, opFileName));
        }

        /// <summary>
        /// Concatenate twelve images for Simon and Toby
        /// </summary>
        public static void ConcatenateTwelveImages()
        {
            var imageDirectory = new DirectoryInfo(@"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings\Site1Images");
            var imageFiles = imageDirectory.GetFiles();
            var imageList = new List<Image>();

            foreach (FileInfo file in imageFiles)
            {
                imageList.Add(Image.FromFile(file.FullName));
            }

            Image combinedImage = ImageTools.CombineImagesInLine(imageList);
            combinedImage.Save(Path.Combine(imageDirectory.FullName, "Site1.png"));
        }

        /// <summary>
        /// Concatenate images for Karl-Heinz Frommolt
        /// </summary>
        public static void KarlHeinzFrommolt()
        {
            FrommoltProject.ConcatenateDays();
        }

        /// <summary>
        /// HERVE GLOTIN
        /// To produce observe feature spectra or SPECTRAL FEATURE TEMPLATES for each species
        /// This is used to analyse Herve Glotin's BIRD50 data set.
        /// </summary>
        public static void HerveGlotinMethods()
        {
            BirdClefExperiment1.Execute(null);

            //HERVE GLOTIN: To produce HIres spectrogram images
            // This is used to analyse Herve Glotin's BIRD50 data set.
            //   Joins images of the same species
            HerveGlotinCollaboration.HiRes4();

            //HERVE GLOTIN: This is used to analyse the BIRD50 data set.
            // In order to analyse the short recordings in BIRD50 dataset, need following change to code:
            // need to modify    AudioAnalysis.AnalysisPrograms.AcousticIndices.cs #line648
            // need to change    AnalysisMinSegmentDuration = TimeSpan.FromSeconds(20),
            // to                AnalysisMinSegmentDuration = TimeSpan.FromSeconds(1),
            HerveGlotinCollaboration.HiRes3();

            //HERVE GLOTIN: This is used to analyse the BIRD50 data set.
            // Combined audio2csv + zooming spectrogram task.
            HerveGlotinCollaboration.HiRes1();

            //HERVE GLOTIN: This is used to analyse the BIRD50 data set.
            // To produce HIres spectrogram images
            HerveGlotinCollaboration.HiRes2();

            HerveGlotinCollaboration.AnalyseBOMBYXRecordingsForSpermWhaleClicks();
        }

        /// <summary>
        /// FROG DATA SET
        /// To produce observe feature spectra
        /// This is used to analyse frog recordings of Lin Schwarzkopf.
        /// </summary>
        public static void AnalyseFrogDataSet()
        {
            HighResolutionAcousticIndices.Execute(null);
        }

        /// <summary>
        /// OTSU TRHESHOLDING FROM JIE XIE
        /// Used to threshold spectrograms to binary.
        ///  Jie uses the algorithm in his last 2016 papers.
        /// </summary>
        public static void OTSU_TRHESHOLDING()
        {
            OtsuThresholder.Execute(null);
        }
    }
}
