// <copyright file="Sandpit.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools.Wav;
    using AnalyseLongRecordings;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;
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
    /// createfoursonograms.
    /// </summary>
    // TODO: [OPENSOURCE] empty out this file
    public class Sandpit
    {
        public const string CommandName = "Sandpit";

        [Command(
        CommandName,
        Description = "[UNMAINTAINED] Michael's personal experimental area.",
        ShowInHelpText = false)]
        public class Arguments : SubCommandBase
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                var tStart = DateTime.Now;
                Log.Verbosity = 1;
                Log.WriteLine("# Start Time = " + tStart.ToString(CultureInfo.InvariantCulture));

                //AnalyseFrogDataSet();
                //Audio2CsvOverOneFile();
                //Audio2CsvOverMultipleFiles();

                // used to get files from availae for Black rail and Least Bittern papers.
                //CodeToExtractFeatureVectorOfIndices();
                //CodeToGetLdfcSpectrogramsFromAvailae();
                //CodeToPlaceScoreTracksUnderLdfcSpectrograms();
                //CodeToPlaceScoreTracksUnderSingleImage();

                //ConcatenateIndexFilesAndSpectrograms();
                //ConcatenateGreyScaleSpectrogramImages();
                //ConcatenateMarineImages();
                //ConcatenateImages();
                //ConcatenateTwelveImages();
                //CubeHelixDrawTestImage();
                //DrawLongDurationSpectrogram();
                //DrawClusterSequence();
                //DrawStandardSpectrograms();
                Test_DrawFourSpectrograms();

                //ExtractSpectralFeatures();
                //HerveGlotinMethods();
                //KarlHeinzFrommolt();
                //OTSU_TRHESHOLDING();
                //ResourcesForEventPatternRecognition();
                //ResourcesForRheobatrachusSilusRecogniser();
                //TestAnalyseLongRecordingUsingArtificialSignal();
                //TestArbimonSegmentationAlgorithm();
                //TestEigenValues();
                //TestChannelIntegrity();
                //TestDct();
                //TEST_FilterMovingAverage();
                //TestImageProcessing();
                //TestMatrix3dClass();
                //TestsOfFrequencyScales();
                //TestReadingFileOfSummaryIndices();
                //TestStructureTensor();
                //TestWavelets();
                //TestFft2D();
                //TestTernaryPlots();
                //TestDirectorySearchAndFileSearch();
                //TestNoiseReduction();
                //Oscillations2014.TESTMETHOD_DrawOscillationSpectrogram();
                //Oscillations2014.TESTMETHOD_GetSpectralIndex_Osc();

                Console.WriteLine("# Finished Sandpit Task!    Press any key to exit.");
                return this.Ok();
            }
        }

        public static void DrawClusterSequence()
        {
            for (int i = 1; i <= 60; i++)
            {
                DrawClusterSequence2(i);
            }
        }

        public static void DrawClusterSequence1(int clusterId)
        {
            // set up an image into which to draw presence/absence of each cluster.
            int height = 400;
            int width = 1440;
            var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var g = Graphics.FromImage(image);
            g.Clear(Color.White);

            DirectoryInfo dir =
                new DirectoryInfo(
                    @"H:\Documents\SensorNetworks\Students\AniekRoelofs\Results 22Feb2018\Complete\SummaryIndices");
            string fileName = "Cluster60Sequence_Gympie2015June22_SuI.csv";
            string path = Path.Combine(dir.FullName, fileName);

            using (TextReader reader = new StreamReader(path))
            {
                // read the header and ignore
                string line = reader.ReadLine();
                int counter = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    //read one line at a time in string array
                    string[] words = line.Split(',');

                    if (line.StartsWith("NaN"))
                    {
                        int day = counter / width;
                        int minuteId = counter % width;
                        image.SetPixel(minuteId, day, Color.Red);
                    }
                    else if (clusterId == int.Parse(words[0]))
                    {
                        int day = counter / width;
                        int minuteId = int.Parse(words[1]);
                        image.SetPixel(minuteId, day, Color.Black);
                    }

                    counter++;
                } // end while

                Console.WriteLine("Counter=" + counter);
            } //using

            g.DrawString("Cluster" + clusterId, new Font("Tahoma", 20), Brushes.Black, new PointF(10, 10));

            string imageName = $"cluster{clusterId}.png";
            string savePath = Path.Combine(dir.FullName + "\\ClusterImages", imageName);
            image.Save(savePath);
            //Console.ReadLine();
        }

        public static void DrawClusterSequence2(int requiredCusterId)
        {
            // set up an image into which to draw presence/absence of each cluster.
            int height = 400;
            int width = 1440;

            Color[] colorchart =
            {
                Color.Red, Color.Orange, Color.Green, Color.Aqua, Color.Blue, Color.BlueViolet, Color.Black,
                Color.DarkMagenta, Color.LawnGreen, Color.DarkRed
            };
            var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var g = Graphics.FromImage(image);
            g.Clear(Color.White);

            DirectoryInfo dir =
                new DirectoryInfo(@"H:\Documents\SensorNetworks\Students\AniekRoelofs\Results 14March2018");
            string fileName = "Cluster60_Hidstate10_Gympie2015June22_SuI_bestof4.csv";
            string path = Path.Combine(dir.FullName, fileName);
            int clusterSize = 0;

            using (TextReader reader = new StreamReader(path))
            {
                // read the header and ignore
                string line = reader.ReadLine();
                int minCounter = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    //read one line at a time in string array
                    string[] words = line.Split(',');
                    int minuteOfday = int.Parse(words[0]);
                    int day = minCounter / width;
                    int hidStateId = int.Parse(words[2]);

                    if (words[1].StartsWith("NaN"))
                    {
                        image.SetPixel(minuteOfday, day, Color.Gray);
                    }
                    else if (requiredCusterId == int.Parse(words[1]))
                    {
                        clusterSize++;
                        image.SetPixel(minuteOfday, day, colorchart[hidStateId - 1]);
                    }

                    minCounter++;
                } // end while

                Console.WriteLine("Counter=" + minCounter);
            } //using

            g.DrawString("Cluster" + requiredCusterId + " Size=" + clusterSize, new Font("Tahoma", 20), Brushes.Black,
                new PointF(10, 10));

            string imageName = $"cluster{requiredCusterId}.png";
            string savePath = Path.Combine(dir.FullName + "\\ClusterImages", imageName);
            image.Save(savePath);

            // Console.ReadLine();
        }

        /// <summary>
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

            // Test on STANDARD 24-HOUR RECORDING
            string recordingPath = @"C:\Ecoacoustics\WavFiles\LizZnidersic\TasmanIsland2015_Unit2_Mez\SM304256_0+1_20151114_131652.wav";
            string outputPath = @"C:\Ecoacoustics\Output\Test\Test24HourRecording\TasmanIslandMez\14";
            string configPath = @"C:\Work\GitHub\audio-analysis\src\AnalysisConfigFiles\Towsey.Acoustic.yml";

            // Ivan Campos recordings
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Ivancampos\INCIPO01_20161031_024006_898.wav";
            //string outputPath = @"G:\SensorNetworks\Output\IvanCampos\17";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

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

            // SERF RECORDINGS FROM 19th June 2013
            // these are six hour recordings
            //string recordingPath = @"G:\Ecoacoustics\WavFiles\SERF\2013June19\SERF_20130619_064615_000.wav";
            //string outputPath = @"C:\Ecoacoustics\Output\SERF\SERFIndicesNew_2013June19";
            //string configPath = @"C:\Work\GitHub\audio-analysis\src\AnalysisConfigFiles\Towsey.Acoustic.yml";

          // USE 24-hour data or parts of from MEZ, TASMAn ISLAND, liz Znidersic
            // these are six hour recordings
            //string recordingPath = @"C:\Ecoacoustics\WavFiles\LizZnidersic\TasmanIsland2015_Unit2_Mez\SM304256_0+1_20151114_031652.wav";
            //string outputPath = @"C:\Ecoacoustics\Output\Test\Test24HourRecording\TasmanIslandMez\04";
            //string configPath = @"C:\Work\GitHub\audio-analysis\src\AnalysisConfigFiles\Towsey.Acoustic.yml";

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
                Config = configPath,
                Output = outputPath.ToDirectoryInfo(),
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(arguments);
        }

        public static void Test_DrawFourSpectrograms()
        {
            var sourceRecording = @"C:\SensorNetworks\SoftwareTests\TestRecordings\BAC2_20071008-085040.wav".ToFileInfo();
            var output = @"C:\SensorNetworks\SoftwareTests\TestFourSonograms".ToDirectoryInfo();
            var configFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo();
            //Audio2Sonogram.TESTMETHOD_DrawFourSpectrograms(sourceRecording, output, configFile);
        }

        /// <summary>
        /// Draws a standard spectrogram, w/wo noise removal & melscale/linear.
        /// This worked Feb 2019.
        /// </summary>
        public static void DrawStandardSpectrograms()
        {
            var audioFile = @"C:\Ecoacoustics\WavFiles\TestRecordings\BAC\BAC2_20071008-085040.wav";
            var recording = new WavReader(audioFile);

            var settings = new SpectrogramSettings()
            {
                SourceFileName = "BAC2_20071008-085040",
                WindowSize = 1024,
                WindowOverlap = 0.0,
                DoMelScale = true,
                MelBinCount = 256,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var amplSpectrogram = new AmplitudeSpectrogram(settings, recording);
            var dbSpectrogram = new DecibelSpectrogram(settings, recording);
            dbSpectrogram.DrawSpectrogram(@"C:\Ecoacoustics\WavFiles\TestRecordings\BAC\2019Output\BAC2_20071008-085040_MelNoNoiseRemoval.png");

            var energySpectro = new EnergySpectrogram(settings, recording);
            energySpectro.DrawLogPsd(@"C:\Ecoacoustics\WavFiles\TestRecordings\BAC\2019Output\BAC2_20071008-085040_MelLogPSD_NoNoiseRemoval.png");
        }

        public static void DrawLongDurationSpectrogram()
        {
            // the default ld fc spectrogram config file
            var spectrogramConfigFile =
                @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramFalseColourConfig.yml";

            // the default index properties file
            string indexPropertiesFile =
                @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml";

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
            spectrogramConfigFile =
                @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramConfigHiRes.yml";
            indexPropertiesFile =
                @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";

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
                InputDataDirectory = ipdir,
                OutputDirectory = opdir,
                IndexPropertiesConfig = indexPropertiesFile,
                FalseColourSpectrogramConfig = spectrogramConfigFile,
            };
            DrawLongDurationSpectrograms.Execute(args);
        }

        /// <summary>
        /// This action item = "concatenateIndexFiles".
        /// </summary>
        public static void ConcatenateIndexFilesAndSpectrograms()
        {
            // set the default values here
            var indexPropertiesConfig =
                @"C:\Work\GitHub\audio-analysis\src\AnalysisConfigFiles\IndexPropertiesConfig.yml";
            var timeSpanOffsetHint = TimeSpan.FromHours(10); // default = Brisbane time
            var drawImages = true;

            // start and end dates INCLUSIVE
            DateTimeOffset? dtoStart = null;
            DateTimeOffset? dtoEnd = null;

            // files containing output from event recognizers.
            // Used only to get Event Recognizer files - set eventDirs=null if not used
            string[] eventDirs = null;
            string eventFilePattern = string.Empty;

            // The drive: local = C; work = G; home = E
            string drive = "C"; // the default

            // SET DEFAULT COLOUR MAPS
            string colorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string colorMap2 = SpectrogramConstants.RGBMap_BGN_PMN_OSC;

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
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\Ecoacoustics\\WavFiles\\TestRecordings\\CompareSM2versusSM4\\MicrophoneTest_AvailaeResult111\\Old_microphone_SM2test"),
            };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "SM2WithOldMics";
            //string opFileStem = "SM4WithNewMics";
            string opPath = $"{drive}:\\Ecoacoustics\\Output\\WildLifeAcoustics\\MicrophoneTests";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\Ecoacoustics\\SoftwareTests\\TestConcatenation\\Data\\ConcatSpectrogramFalseColourConfig.yml");
            FileInfo sunriseDatafile = null;
            bool concatenateEverythingYouCanLayYourHandsOn = false; // Set false to work in 24-hour blocks only
            dtoStart = new DateTimeOffset(2016, 08, 09, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2016, 08, 09, 0, 0, 0, TimeSpan.Zero);
            // change PMN to POW because PMN not available in these recordings
            colorMap2 = "BGN-POW-CLS";
            // ########################## END of Yvonne's recordings of SM2 and SM4
            */

            // ########################## CONCATENATION of 24-hour TEST  recordings from Liz Znidersic
            // top level directory
            string[] dataDirs =
            {
                @"C:\Ecoacoustics\Output\Test\Test24HourRecording\TasmanIslandMez",
            };
            string directoryFilter = @"Towsey.Acoustic"; // this is a directory filter to locate only the required files
            string opFileStem = "Testing";
            string opPath = @"C:\Ecoacoustics\Output\Test\Test24HourRecording\Concat3";
            var falseColourSpgConfig = $"C:\\Work\\GitHub\\audio-analysis\\src\\AnalysisConfigFiles\\SpectrogramFalseColourConfig.yml";
            concatenateEverythingYouCanLayYourHandsOn = true;

            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2016, 12, 31, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2016, 12, 31, 0, 0, 0, TimeSpan.Zero);

            // there are three options for rendering of gaps/missing data: NoGaps, TimedGaps and EchoGaps.
            gapRendering = "TimedGaps";

            // ########################## END of 24-hour TEST recordings

            /*
            // ########################## CONCATENATION of MARINA SCARPELLI recordings from Brazil
            // The drive: work = G; home = E
            drive = "C";

            // top level directory
            string[] dataDirs =
            {
                $"{drive}:\\Ecoacoustics\\Collaborations\\MarinaScarpelli\\Indices",
            };
            string directoryFilter = "LEEC02_0_20161231_*.wav"; // this is a directory filter to locate only the required files
            string opFileStem = "LEEC02_20161231_";
            string opPath = $"{drive}:\\Ecoacoustics\\Collaborations\\MarinaScarpelli\\Concat";
            var falseColourSpgConfig = $"{drive}:\\Work\\GitHub\\audio-analysis\\src\\AnalysisConfigFiles\\SpectrogramFalseColourConfig.yml";
            concatenateEverythingYouCanLayYourHandsOn = true;

            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2016, 12, 31, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2016, 12, 31, 0, 0, 0, TimeSpan.Zero);

            // there are three options for rendering of gaps/missing data: NoGaps, TimedGaps and EchoGaps.
            gapRendering = "TimedGaps";

            // ########################## END of MARINA SCARPELLI recordings
            */

            /*
            // ########################## CONCATENATION of Pillaga Forest recordings from Brad Law
            // The drive: work = G; home = E
            drive = "G";

            // top level directory AVAILAE JOB #181
            string[] dataDirs =
            {
                $"{drive}:\\Ecoacoustics\\Output\\BradLaw\\PillagaData",
            };
            string directoryFilter = "Pillaga*"; // this is a directory filter to locate only the required files
            string opFileStem = "PillagaForest20121125";
            string opPath = $"{drive}:\\Ecoacoustics\\Output\\BradLaw";
            var falseColourSpgConfig =
                $"{drive}:\\Ecoacoustics\\Output\\Bats\\config\\SpectrogramFalseColourConfig.yml";
            FileInfo sunriseDatafile = null;

            concatenateEverythingYouCanLayYourHandsOn = true;

            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2012, 08, 08, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2012, 08, 08, 0, 0, 0, TimeSpan.Zero);

            // there are three options for rendering of gaps/missing data: NoGaps, TimedGaps and EchoGaps.
            gapRendering = "EchoGaps";

            // ########################## END of Pillaga Forest recordings
            */

            /*
            // ########################## CONCATENATION of Yvonne's BAT recordings
            // The drive: work = G; home = E
            drive = "G";

            // top level directory AVAILAE JOB #181
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\Ecoacoustics\\OutputDataSets\\YvonneBats_Gympie20170906"),
            };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "GympieBATS_2017August";
            string opPath = $"{drive}:\\Ecoacoustics\\Output\\Bats\\BatsTestTimeGaps";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\Ecoacoustics\\Output\\Bats\\config\\SpectrogramFalseColourConfig.yml");
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
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\Ecoacoustics\\Output\\Bhutan\\DebugConcatenateSourceData"),
            };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string opFileStem = "BhutanTest";
            string opPath = $"{drive}:\\Ecoacoustics\\Output\\Bhutan\\DebugConcatenateOutput";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\Ecoacoustics\\SoftwareTests\\TestConcatenation\\Data\\ConcatSpectrogramFalseColourConfig.yml");
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
            string opPath = $"{drive}:\\Ecoacoustics\\Output\\Mengersen\\NightAConcatenated";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\Ecoacoustics\\SoftwareTests\\Test_Concatenation\\Data\\SpectrogramFalseColourConfig.yml");
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
            string opPath = $"{drive}:\\Ecoacoustics\\AvailaeFolders\\LizZnidersic\\Test_IndexDistributions";
            //string opPath = $"{drive}:\\AvailaeFolders\\LizZnidersic\\TEST_missingData"; //was used to put results for testing missing data
            var falseColourSpgConfig = new FileInfo($"{drive}:\\Ecoacoustics\\SoftwareTests\\Test_Concatenation\\Data\\SpectrogramFalseColourConfig.yml");
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
            //DirectoryInfo[] dataDirs = { new DirectoryInfo($"{drive}:\\Ecoacoustics\\Output\\Frogs\\Canetoad\\2016Oct28-174219 - Michael, Towsey.Indices, #120\\SD Card A"),
            DirectoryInfo[] dataDirs = { new DirectoryInfo($"G:\\Ecoacoustics\\OutputDataSets\\GrooteAcousticIndices_Job120\\SD Card A"),
                                                   };
            string directoryFilter = "*.wav";  // this is a directory filter to locate only the required files
            string testPath = $"{drive}:\\Ecoacoustics\\SoftwareTests\\Test_Concatenation\\ExpectedOutput";
            var falseColourSpgConfig = new FileInfo($"{drive}:\\Ecoacoustics\\SoftwareTests\\Test_Concatenation\\Data\\TEST_SpectrogramFalseColourConfig.yml");
            timeSpanOffsetHint = TimeSpan.FromHours(9.5);
            FileInfo sunriseDatafile = null;
            string opFileStem = "ConcatGrooteJCU";
            string opPath = $"{drive}:\\Ecoacoustics\\Output\\Frogs\\Canetoad\\ConcatGroote_Job120";
            bool concatenateEverythingYouCanLayYourHandsOn = false; // 24 hour blocks only
            // start and end dates INCLUSIVE
            dtoStart = new DateTimeOffset(2016, 08, 03, 0, 0, 0, TimeSpan.Zero);
            dtoEnd   = new DateTimeOffset(2016, 08, 03, 0, 0, 0, TimeSpan.Zero);

            eventDirs = new DirectoryInfo[1];
            eventDirs[0] = new DirectoryInfo(@"G:\Ecoacoustics\OutputDataSets\GrooteCaneToad_Job120\\SD Card A");
            string eventFilePattern = "*_Towsey.RhinellaMarina.Events.csv";
            */

            //// ########################## MARINE RECORDINGS
            //// top level directory
            ////DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\2015Dec14-094058 - Michael, Towsey.Indices, ICD=30.0, #70\towsey\MarineRecordings\Cornell\2013March-April"),
            ////                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\Ecoacoustics\WavFiles\MarineRecordings\Cornell\2013March-April"),
            //                           };
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\Ecoacoustics\WavFiles\MarineRecordings\Cornell\2013March-April"),
            //                           };
            //string directoryFilter = "201303";
            //string opPath = @"C:\Ecoacoustics\Output\MarineSonograms\LdFcSpectrograms2013March";
            ////string opPath = @"C:\Ecoacoustics\Output\MarineSonograms\LdFcSpectrograms2013April";
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
            ////string opPath = @"C:\Ecoacoustics\Output\MarineSonograms\LdFcSpectrograms2013April";
            //dtoStart = new DateTimeOffset(2015, 07, 09, 0, 0, 0, TimeSpan.Zero);
            //dtoEnd = new DateTimeOffset(2015, 07, 10, 0, 0, 0, TimeSpan.Zero);
            //string opFileStem = "Site1_20150709";
            // ########################## END of GRIFFITH - SIMON/TOBY FRESH-WATER RECORDINGS

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
                InputDataDirectories = dataDirs.Select(FileInfoExtensions.ToDirectoryInfo).ToArray(),
                OutputDirectory = opPath.ToDirectoryInfo(),
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
                DrawImages = drawImages,

                // following used to add in a recognizer score track
                EventDataDirectories = eventDirs,
                EventFilePattern = eventFilePattern,
            };

            ConcatenateIndexFiles.Execute(args);
        }

        /// <summary>
        /// TODO Combine the grey scale spectrograms produced by AnalysisPrograms.exe
        /// This method will be useful for comparing the response of different spectral indices to the same acoustic event.
        /// Use this when you want best acoustic features for doing ML using spectral index features.
        /// </summary>
        public static void ConcatenateGreyScaleSpectrogramImages()
        {

            var ipDirInfo = new DirectoryInfo(@"C:\Ecoacoustics\Output\Test\Test24HourRecording\TasmanIslandMez\04\Towsey.Acoustic");
            var opDirInfo = new DirectoryInfo(@"C:\Ecoacoustics\Output\Test\Test24HourRecording\TasmanIslandMez\04\Towsey.Acoustic");
            var opFileName = "SM304256_0+1_20151114_031652";

            //string[] keys = { "ACI", "BGN", "CVR", "ENT", "EVN", "OSC", "PMN", "R3D", "RHZ", "RNG", "RPS", "RVT", "SPT" };
            // the following line gets all current spectral indices, including DIF and SUM which we will want to exclude.
            var keys = SpectralIndexValues.Keys;

            //Read list of images into List
            var listOfImages = new List<Image>();

            foreach (var key in keys)
            {
                if (key == "DIF" || key == "SUM")
                {
                    continue;
                }

                // construct the path
                //var path = Path.Combine(ipDirInfo.FullName, opFileName + key + ".png");
                var path = FilenameHelpers.AnalysisResultPath(ipDirInfo, opFileName, key, "png");
                var indexImage = ImageTools.ReadImage2Bitmap(path);

                listOfImages.Add(indexImage);
            }

            var opPath = FilenameHelpers.AnalysisResultPath(opDirInfo, opFileName, "KEYS", "png");

            // check how wide combined image will be. If tracks are wider than 180 = 3 hours, then go vertical
            int imageCount = listOfImages.Count;
            if (listOfImages[0].Width * imageCount > 180 * imageCount)
            {
                var combinedImage = ImageTools.CombineImagesVertically(listOfImages);
                combinedImage?.Save(opPath);
            }
            else
            {
                var combinedImage = ImageTools.CombineImagesInLine(listOfImages);
                combinedImage?.Save(opPath);
            }
        }

        /// <summary>
        /// read a set of Spectral index files and extract values from frequency band
        /// This work done for Liz Znidersic paper.
        /// End of the method requires access to Liz tagging info.
        /// </summary>
        public static void ExtractSpectralFeatures()
        {
            // parameters
            string dir =
                @"H:\Documents\SensorNetworks\MyPapers\2017_DavidWatson\CaseStudy1 Liz\MachineLearningExercise";
            string fileName = "LizZnidersic_TasmanIsTractor_20151111__Towsey.Acoustic";
            string[] indexNames = { "ACI", "ENT", "PMN", "SPT", "RHZ" };
            var framecount =
                1440; // could read this from first matrix but easier to declare it. Need it for reading in tagged data.
            int startOffsetMinute = 47; // 24 hours of recording starts at 12:47am. Need this as an offset.
            int bottomBin = 3;
            int topBin = 22;
            int binCount = topBin - bottomBin + 1;

            // read spectral index matrices, extract required freq band, and store sub-band in dictionary of matrices.
            var dict = new Dictionary<string, double[,]>();
            foreach (string id in indexNames)
            {
                var fileinfo = new FileInfo(Path.Combine(dir, $"{fileName}.{id}.csv"));
                var matrix = Csv.ReadMatrixFromCsv<double>(fileinfo, TwoDimensionalArray.Rotate90ClockWise);

                //framecount = matrix.GetLength(1);
                //Console.WriteLine("\n" + id);
                //Console.WriteLine(matrix[3, 0]);
                //Console.WriteLine(matrix[3, 1]);
                //Console.WriteLine(matrix[3, 2]);

                // use following line for skipping normalisation
                //var normedM = MatrixTools.Submatrix(matrix, bottomBin, 0, topBin, framecount - 1);

                // use following line to normalise
                double minPercentile = 0.01;
                double maxPercentile = 0.99;
                double minCut;
                double maxCut;
                MatrixTools.PercentileCutoffs(matrix, minPercentile, maxPercentile, out minCut, out maxCut);
                var normedM = MatrixTools.BoundMatrix(matrix, minCut, maxCut);
                normedM = MatrixTools.NormaliseMatrixValues(normedM);

                normedM = MatrixTools.Submatrix(normedM, bottomBin, 0, topBin, framecount - 1);
                dict.Add(id, normedM);
            }

            // Read in labelling info from Liz Znidersic
            string path = Path.Combine(dir, "lerafcis20151111.csv");
            var tags = ReadFileOfTagsFromLizZnidersic(path, framecount, startOffsetMinute);

            // Concatenate feature vectors, one frame at a time.
            // Then add tag at the end.
            // Then add to matrix data set.
            int featureVectorLength = indexNames.Length * binCount;
            var dataSet = new double[framecount, featureVectorLength + 1];
            for (int frame = 0; frame < framecount; frame++)
            {
                var list = new List<double>();

                // get the time-frame and conatenate
                foreach (string id in indexNames)
                {
                    var featureVector = MatrixTools.GetColumn(dict[id], frame);
                    list.AddRange(featureVector);
                }

                // add in the tag and then set row in data matrix
                list.Add(tags[frame] > 0 ? 1 : 0);

                var array = list.ToArray();
                MatrixTools.SetRow(dataSet, frame, array);
            }

            // write dataset to file
            var opFileinfo = new FileInfo(Path.Combine(dir, $"{fileName}.FeatureVectors.csv"));
            Csv.WriteMatrixToCsv(opFileinfo, dataSet);

            // save dataset as image
            dataSet = MatrixTools.SubtractValuesFromOne(dataSet);
            var image = ImageTools.DrawMatrixWithoutNormalisation(MatrixTools.MatrixRotate90Anticlockwise(dataSet));

            // add tags to image of feature vectors
            var g = Graphics.FromImage(image);
            var pen = new Pen(Color.Red);
            for (int i = 0; i < framecount; i++)
            {
                if (tags[i] == 0)
                {
                    continue;
                }

                if (tags[i] == 1)
                {
                    pen = new Pen(Color.Red);
                }

                if (tags[i] == 2)
                {
                    pen = new Pen(Color.Green);
                }

                if (tags[i] == 3)
                {
                    pen = new Pen(Color.Blue);
                }

                g.DrawLine(pen, i, 2, i, 5);
                g.DrawLine(pen, i, 20, i, 25);
                g.DrawLine(pen, i, 40, i, 45);
                g.DrawLine(pen, i, 60, i, 65);
            }

            var path3 = Path.Combine(dir, $"{fileName}.FeatureVectors.png");
            image.Save(path3);

            // add tags to false-colour spectrogram
            //string path1 = Path.Combine(dir, "LizZnidersic_TasmanIsTractor_20151111__Tagged.png");
            //var image1 = ImageTools.ReadImage2Bitmap(path1);
            //var g1 = Graphics.FromImage(image1);
            //for (int i = 0; i < framecount; i++)
            //{
            //    if (tags[i] == 0) continue;
            //    if (tags[i] == 1) pen = new Pen(Color.Red);
            //    if (tags[i] == 2) pen = new Pen(Color.Green);
            //    if (tags[i] == 3) pen = new Pen(Color.Blue);

            //    g1.DrawLine(pen, i, image1.Height - 18, i, image1.Height - 4);
            //}

            //var path2 = Path.Combine(dir, $"{fileName}.Tagged.png");
            //image1.Save(path2);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        /// <summary>
        /// Read in labelling info from Liz Znidersic
        /// Returns an array of tags, one for each minute of the original recording.
        /// </summary>
        /// <param name="path">file to be erad</param>
        /// <param name="arraySize">size of array to return</param>
        /// <param name="startMinute">start offset in minutes for the entire 24 hour recording</param>
        public static int[] ReadFileOfTagsFromLizZnidersic(string path, int arraySize, int startMinute)
        {
            var array = new int[arraySize];
            var data = FileTools.ReadTextFile(path);
            for (int i = 1; i < data.Count; i++)
            {
                var words = data[i].Split(',');

                // get the time location of the hit
                var word = words[3];
                word = word.PadLeft(4);
                string hour = word.Remove(2);
                string min = word.Substring(2);
                int minuteCount = (int.Parse(hour) * 60) + int.Parse(min) - startMinute + 1;

                // get the difficulty of the hit
                int hitDifficulty = 1; //easy one
                if (words[5].StartsWith("difficult"))
                {
                    hitDifficulty = 2;
                }

                if (words[5].StartsWith("very d"))
                {
                    hitDifficulty = 3;
                }

                // add to the array
                array[minuteCount] = hitDifficulty;
            }

            return array;
        }

        /// <summary>
        /// this is a test to read a file of summary indices.
        /// THis could be made a unit test???
        /// </summary>
        public static void TestReadingFileOfSummaryIndices()
        {
            var summaryIndices = new List<SummaryIndexValues>();
            var file = new FileInfo(
                @"C:\SensorNetworks\SoftwareTests\TestConcatenation\20160726_073000_Towsey.Acoustic.Indices.csv");

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
            // FrequencyScale.TESTMETHOD_MelFrequencyScale();
            // FrequencyScale.TESTMETHOD_OctaveFrequencyScale1();
            // FrequencyScale.TESTMETHOD_OctaveFrequencyScale2();

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

            FrequencyScale.TESTMETHOD_DrawFrequencyLinesOnImage();
        }

        /// <summary>
                /// Unit test of AnalyseLongRecording() using artificial signal
                /// </summary>
                public static void TestAnalyseLongRecordingUsingArtificialSignal()
        {
            int sampleRate = 22050;
            double duration = 420; // signal duration in seconds = 7 minutes
            int[] harmonics = {500, 1000, 2000, 4000, 8000};
            var recording = DspFilters.GenerateTestRecording(sampleRate, duration, harmonics, WaveType.Consine);
            var outputDirectory = @"C:\SensorNetworks\SoftwareTests\TestLongDurationRecordings";
            var recordingPath = Path.Combine(outputDirectory, "TemporaryRecording.wav");
            WavWriter.WriteWavFileViaFfmpeg(recordingPath.ToFileInfo(), recording.WavReader);
            var configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

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
                Source = recordingPath.ToFileInfo(),
                Config = configPath,
                Output = outputDirectory.ToDirectoryInfo(),
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(argumentsForAlr);
            var outputDir = outputDirectory.ToDirectoryInfo();
            var resultsDirectory = outputDir.Combine("Towsey.Acoustic");
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
            var ldsBgnSpectrumFile = outputDir.CombineFile("Spectrum2.png");
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

            double[] thresholdLevels = { 30.0, 25.0, 20.0, 15.0, 10.0, 5.0};
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
            var debugPath = FilenameHelpers.AnalysisResultPath(
                outputDirectory,
                recording.BaseName,
                "AedExperiment_ThresholdStack",
                "png");
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

            if (false)
            {
                // quickie to calculate entropy of some matrices - used for Yvonne acoustic transition matrices

                string dir =
                    @"H:\Documents\SensorNetworks\MyPapers\2016_EcoAcousticCongress_Abstract\TransitionMatrices";
                string filename = @"transition_matrix_BYR4_16Oct.csv";

                //string filename = @"transition_matrix_SE_13Oct.csv";
                //double[,] M = CsvTools.ReadCSVFile2Matrix(Path.Combine(dir, filename)); //DEPRACATED
                //double[] v = DataTools.Matrix2Array(M);

                // these are actual call counts for ~60 bird species calling each day at SERF - see comment at end of each line
                //double[] v = {9, 1, 4, 1, 58, 9, 28, 11, 24, 54, 1, 36, 12, 23, 12, 228, 66, 5, 15, 13, 4, 9, 21, 85, 5, 19, 1, 4, 44, 2, 47, 3, 0, 38, 62, 10, 2, 22, 384, 19, 4, 5, 629, 9, 25, 35, 141, 86, 21, 5, 16, 1, 121, 4, 3, 70, 6, 11, 1, 139, 11, 84, 1, 39, 254}; // NE 13thOct2010
                //double[] v = {5, 2, 3, 44, 40, 40, 22, 42, 30, 2, 21, 27, 249, 58, 20, 4, 18, 1, 11, 9, 67, 30, 24, 83, 34, 1, 1, 47, 5, 4, 1, 12, 415, 43, 13, 3, 428, 26, 101, 253, 72, 68, 0, 16, 1, 1, 1, 90, 1, 70, 22, 1, 1, 110, 14, 146, 1, 52, 731 }; // NE 14thOct2010
                //double[] v = { 7, 1, 14, 6, 24, 1, 8, 10, 45, 62, 2, 31, 5, 7, 216, 1, 42, 50, 66, 18, 9, 6, 10, 19, 38, 9, 20, 29, 12, 5, 17, 258, 0, 10, 31, 22, 183, 219, 3, 7, 644, 10, 94, 476, 130, 1, 9, 9, 1, 90, 6, 2, 12, 29, 1, 249, 50, 25, 1, 10, 33 }; // NW 13thOct2010
                //double[] v = { 1, 4, 2, 1, 7, 6, 14, 1, 4, 20, 36, 35, 3, 26, 4, 48, 235, 15, 52, 68, 24, 31, 7, 12, 2, 49, 60, 6, 1, 11, 12, 1, 51, 1, 282, 0, 0, 0, 8, 58, 201, 315, 3, 363, 20, 266, 506, 124, 2, 94, 2, 3, 24, 251, 53, 37, 6, 27 }; // NW 14thOct2010
                //double[] v = { 6, 5, 30, 24, 21, 111, 6, 52, 20, 68, 74, 1, 45, 2, 11, 644, 184, 32, 12, 32, 9, 39, 120, 100, 11, 30, 1, 77, 463, 12, 2, 11, 6, 73, 150, 12, 164, 132, 7, 393, 1, 946, 178, 93, 41, 15, 13, 8, 33, 520, 1, 2, 44, 1, 15, 15, 343, 10, 243, 94, 126 }; // SE 13thOct2010
                //double[] v = { 3, 7, 3, 1, 35, 34, 43, 10, 50, 3, 39, 54, 11, 22, 2, 650, 91, 20, 4, 21, 11, 17, 97, 106, 10, 1, 1, 3, 7, 389, 11, 7, 17, 42, 123, 5, 157, 174, 8, 323, 646, 135, 83, 15, 12, 15, 535, 8, 19, 3, 8, 1, 248, 11, 171, 59, 103 }; // SE 14thOct2010
                //double[] v = { 7, 18, 1, 7, 66, 1, 157, 6, 30, 28, 83, 2, 15, 29, 19, 323, 1, 56, 4, 31, 10, 1, 6, 10, 152, 13, 36, 1, 30, 1, 10, 15, 1, 333, 141, 7, 47, 1315, 2, 113, 723, 1, 33, 16, 1, 20, 2, 182, 186, 27, 20, 2, 8, 8, 18, 4, 194, 6, 105, 11, 109 }; // SW 13thOct2010
                //double[] v = { 1,4,19,6,14,45,89,2,3,24,59,3,5,4,74,19,443,1,31,9,17,9,1,3,4,150,44,2,2,47,1,3,1,20,1,22,3,397,151,19,63,810,4,114,969,2,25,34,4,2,19,202,255,8,10,1,249,9,137,10,157}; // SW 14thOct2010
                // the following lines are call counts per minute over all species.
                //double[] v = { 0,0,1,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,1,1,0,1,4,4,3,4,2,2,2,2,3,2,2,5,3,4,5,4,4,6,6,5,8,11,10,10,10,11,11,10,11,13,12,11,12,8,8,6,8,6,7,8,9,4,7,7,5,7,8,10,7,11,9,10,8,6,7,7,9,13,8,9,8,9,9,10,7,9,11,8,8,7,8,7,10,8,8,9,7,6,5,6,5,7,9,9,7,6,8,11,10,6,7,6,7,7,8,7,6,9,10,7,9,6,7,8,7,8,8,5,5,5,8,7,9,9,9,7,9,7,8,9,9,9,9,8,7,7,7,6,8,8,6,8,10,8,9,10,9,10,12,8,7,7,5,6,4,6,7,9,9,11,7,9,11,10,9,9,10,10,10,10,8,9,7,11,10,11,5,7,9,6,9,12,9,7,10,7,9,9,7,6,6,7,7,8,10,8,8,4,8,9,11,8,5,4,4,5,7,4,7,7,9,12,9,9,8,7,6,7,8,7,8,5,11,7,6,4,7,7,9,9,8,8,9,9,5,7,7,4,7,7,5,10,6,8,6,9,5,3,5,5,6,6,7,5,8,11,11,7,10,8,11,10,10,7,10,6,8,7,1,4,6,9,9,9,7,3,3,2,4,7,4,6,8,7,5,9,9,6,9,8,8,10,11,7,11,9,7,7,5,8,9,13,10,10,6,7,6,4,6,5,8,2,3,1,4,3,3,6,5,4,5,7,9,4,6,5,7,3,5,4,6,5,3,4,6,4,7,7,6,6,4,5,5,2,3,4,4,8,7,6,5,6,5,5,7,8,8,6,6,6,7,6,4,4,5,6,6,3,3,2,5,4,6,3,4,4,5,4,4,7,7,5,3,5,5,3,6,4,2,3,2,4,4,3,4,4,6,4,4,4,4,4,3,1,4,5,3,3,4,5,6,3,1,4,3,7,5,6,4,3,1,4,2,3,4,3,4,4,3,3,5,3,6,6,6,3,6,9,11,5,6,9,8,6,4,5,4,4,4,3,3,4,4,4,6,3,0,6,7,6,7,7,5,5,7,6,8,6,8,10,9,7,5,6,5,6,5,4,5,5,4,2,7,5,5,9,9,5,4,6,1,0,1,1,3,1,3,1,3,8,3,6,5,7,7,7,6,8,6,3,6,6,5,6,8,6,6,6,5,5,5,3,3,3,5,8,9,5,5,6,5,6,5,11,10,8,6,7,3,2,2,3,4,4,4,1,1,2,4,2,3,3,4,4,6,2,2,3,9,3,5,5,7,4,5,4,4,4,4,6,5,7,4,8,8,5,9,3,4,5,4,6,6,7,6,5,8,6,4,3,6,5,5,6,4,7,11,11,12,10,10,7,6,8,5,5,3,6,3,3,5,4,5,7,8,9,5,5,4,6,2,3,5,8,7,3,6,5,3,4,6,4,4,5,5,3,3,3,3,5,5,3,2,3,3,5,2,1,6,6,5,3,2,4,2,7,9,9,6,5,7,5,5,7,8,7,7,8,6,3,6,6,3,4,2,3,2,1,3,8,4,6,6,7,5,5,3,5,5,3,3,3,3,5,6,7,4,2,3,2,4,7,7,3,4,2,2,4,7,5,6,3,4,4,3,4,3,5,6,5,6,6,4,5,5,1,3,3,3,4,3,5,5,3,3,5,6,5,6,6,5,4,5,4,5,8,5,8,5,6,7,4,3,3,5,3,4,5,7,5,6,6,7,3,3,4,5,3,6,3,3,1,3,1,5,3,3,0,2,4,3,6,5,4,5,5,6,5,5,6,5,5,5,3,2,6,5,4,4,4,4,3,4,6,4,3,5,9,4,8,3,5,4,1,3,4,3,2,2,5,1,2,3,4,5,5,4,4,3,2,3,3,2,2,3,3,2,1,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }; // SW 13thOct2010
                double[] v =
                {
                    0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 2, 0, 2, 3, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 4, 3, 6, 4, 3, 3, 4, 4, 4, 4, 3, 4, 3, 3, 1, 7, 5, 5, 5, 8, 7,
                    6, 7, 8, 8, 7, 7, 7, 7, 7, 6, 8, 8, 9, 7, 13, 8, 10, 10, 6, 11, 6, 8, 7, 7, 9, 6, 8, 8, 7, 4, 8, 4,
                    4, 4, 6, 7, 11, 8, 8, 6, 5, 4, 5, 6, 9, 6, 8, 9, 4, 2, 5, 3, 5, 3, 4, 8, 8, 8, 9, 7, 8, 8, 7, 5, 5,
                    6, 4, 7, 9, 6, 5, 2, 6, 9, 10, 8, 5, 7, 8, 7, 7, 4, 9, 8, 7, 12, 9, 10, 14, 12, 10, 9, 12, 8, 9, 9,
                    7, 9, 9, 5, 6, 7, 10, 10, 5, 7, 8, 7, 6, 6, 6, 7, 4, 3, 7, 8, 6, 5, 5, 7, 5, 7, 5, 6, 6, 7, 7, 10,
                    8, 5, 4, 6, 9, 6, 9, 8, 5, 6, 4, 8, 10, 8, 7, 7, 6, 6, 6, 5, 6, 5, 4, 8, 7, 6, 6, 5, 6, 7, 7, 5, 5,
                    6, 6, 7, 8, 8, 7, 6, 5, 4, 4, 4, 4, 3, 5, 6, 7, 9, 8, 6, 6, 4, 7, 4, 3, 6, 7, 4, 7, 6, 3, 8, 5, 6,
                    6, 5, 4, 6, 5, 7, 4, 4, 5, 6, 7, 5, 9, 7, 4, 6, 7, 6, 5, 4, 7, 4, 4, 8, 8, 3, 6, 5, 5, 4, 5, 4, 4,
                    4, 5, 7, 8, 7, 6, 7, 3, 2, 4, 7, 9, 7, 7, 6, 6, 6, 4, 5, 3, 3, 3, 3, 7, 6, 5, 4, 4, 3, 4, 6, 5, 2,
                    3, 2, 5, 2, 3, 1, 3, 2, 5, 3, 4, 5, 6, 5, 7, 3, 8, 6, 2, 5, 5, 5, 3, 2, 4, 2, 2, 3, 4, 1, 2, 1, 2,
                    0, 1, 3, 7, 5, 2, 3, 2, 2, 6, 3, 2, 2, 2, 5, 3, 4, 2, 4, 3, 2, 2, 4, 5, 3, 3, 2, 2, 3, 4, 2, 3, 5,
                    3, 4, 3, 3, 2, 3, 5, 3, 3, 1, 1, 2, 2, 2, 5, 4, 5, 3, 3, 2, 2, 2, 3, 3, 2, 3, 3, 2, 3, 2, 4, 2, 3,
                    6, 5, 1, 3, 2, 2, 5, 4, 5, 2, 4, 5, 2, 1, 1, 2, 4, 4, 0, 3, 4, 4, 2, 1, 2, 0, 0, 0, 1, 0, 5, 3, 5,
                    5, 6, 3, 4, 2, 2, 4, 4, 5, 3, 2, 3, 1, 0, 0, 2, 2, 3, 4, 5, 5, 5, 4, 3, 4, 2, 4, 3, 3, 4, 4, 1, 3,
                    4, 6, 2, 3, 4, 2, 4, 2, 5, 3, 3, 5, 1, 4, 2, 5, 4, 2, 4, 5, 2, 2, 3, 3, 2, 4, 3, 5, 6, 7, 4, 4, 4,
                    4, 3, 6, 4, 3, 5, 3, 5, 7, 6, 5, 4, 7, 2, 2, 4, 4, 4, 4, 4, 2, 2, 2, 4, 4, 4, 4, 4, 3, 3, 4, 5, 4,
                    3, 3, 3, 2, 4, 5, 3, 4, 4, 4, 3, 1, 3, 3, 1, 2, 4, 4, 2, 3, 3, 5, 5, 3, 3, 2, 4, 3, 3, 4, 5, 5, 6,
                    6, 4, 5, 2, 2, 2, 5, 7, 2, 4, 3, 4, 3, 5, 3, 2, 2, 2, 2, 3, 5, 3, 5, 4, 4, 4, 3, 3, 3, 1, 3, 5, 5,
                    4, 4, 2, 3, 1, 1, 4, 5, 2, 2, 3, 4, 3, 2, 3, 4, 6, 5, 3, 1, 2, 3, 3, 1, 0, 2, 1, 5, 2, 1, 1, 3, 3,
                    1, 2, 2, 5, 2, 4, 1, 1, 2, 2, 2, 5, 5, 3, 1, 1, 0, 1, 0, 3, 0, 1, 1, 2, 2, 0, 2, 3, 4, 3, 2, 1, 3,
                    1, 1, 1, 3, 1, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2, 5, 2, 3, 2, 2, 2, 2, 3, 3, 1, 2, 3, 2, 4, 3, 2,
                    2, 1, 1, 3, 4, 4, 3, 1, 1, 2, 3, 3, 2, 3, 4, 4, 3, 4, 4, 3, 4, 6, 4, 4, 6, 7, 8, 4, 4, 6, 6, 4, 4,
                    6, 3, 4, 4, 1, 4, 1, 1, 2, 6, 3, 3, 3, 1, 3, 7, 3, 3, 4, 2, 4, 3, 2, 3, 4, 4, 4, 5, 4, 4, 4, 5, 3,
                    3, 3, 4, 4, 3, 6, 4, 4, 4, 6, 4, 4, 6, 3, 2, 5, 1, 1, 1, 3, 0, 1, 3, 2, 5, 2, 3, 6, 4, 4, 4, 4, 3,
                    3, 4, 2, 2, 3, 4, 3, 3, 2, 4, 3, 2, 3, 3, 3, 3, 3, 1, 1, 2, 1, 2, 1, 2, 3, 1, 0, 1, 0, 0, 1, 1, 2,
                    1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                }; // NW 13thOct2010
                double entropy = DataTools.EntropyNormalised(v);
            } // end if (true)

            // add tags to false-colour spectrogram
            //string path1 = Path.Combine(dir, "LizZnidersic_TasmanIsTractor_20151111__Tagged.png");
            //var image1 = ImageTools.ReadImage2Bitmap(path1);
            //var g1 = Graphics.FromImage(image1);
            //for (int i = 0; i < framecount; i++)
            //{
            //    if (tags[i] == 0) continue;
            //    if (tags[i] == 1) pen = new Pen(Color.Red);
            //    if (tags[i] == 2) pen = new Pen(Color.Green);
            //    if (tags[i] == 3) pen = new Pen(Color.Blue);

            //HoughTransform.Test1HoughTransform();
            HoughTransform.Test2HoughTransform();

            // call SURF image Feature extraction
            // SURFFeatures.SURF_TEST();
            SurfAnalysis.Main(null);
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
            //string[] keys = { "BGN", "PMN", "EVN"};
            string[] keys = { "ACI", "ENT", "EVN"};

            FileInfo[] indexFiles =
            {
                new FileInfo(
                    @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__" +
                    keys[0] + ".csv"),
                new FileInfo(
                    @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__" +
                    keys[1] + ".csv"),
                new FileInfo(
                    @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__" +
                    keys[2] + ".csv"),
            };
            FileInfo opImage = new FileInfo(
                @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622_TernaryPlot.png");

            var matrixDictionary = IndexMatrices.ReadSummaryIndexFiles(indexFiles, keys);

            string indexPropertiesConfigPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults" +
                                               @"\IndexPropertiesConfig.yml";
            FileInfo indexPropertiesConfigFileInfo = new FileInfo(indexPropertiesConfigPath);
            Dictionary<string, IndexProperties>
                dictIP = null; //IndexProperties.GetIndexProperties(indexPropertiesConfigFileInfo);
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
            var imageDirectory =
                new DirectoryInfo(@"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings\Site1Images");
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
        /// Concatenate marine spectrogram ribbons and add tidal info if available.
        /// </summary>
        public static void ConcatenateMarineImages()
        {
            DirectoryInfo[] dataDirs =
            {
                new DirectoryInfo(
                    @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013March\CornellMarine"),
                new DirectoryInfo(
                    @"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April\CornellMarine"),
            };

            // To CALCULATE MUTUAL INFORMATION BETWEEN SPECIES DISTRIBUTION AND FREQUENCY INFO
            // This method calculates a seperate value of MI for each frequency bin
            // See the next method for single value of MI that incorporates all freq bins combined.
            if (false)
            {
                // set up IP and OP directories
                string parentDir = @"C:\SensorNetworks\Output\BIRD50";
                string key = "RHZ"; //"RHZ";
                int valueResolution = 6;
                string miFileName = parentDir + @"\MutualInformation." + valueResolution + "catNoSkew." + key + ".txt";

                //double[] bounds = { 0.0, 3.0, 6.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 8.0 };
                double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0}; // noSkew

                //double[] bounds = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 10.0 };
                //double[] bounds = { 0.0, 1.0, 2.0, 4.0, 6.0, 10.0 }; // skew left
                //double[] bounds = { 0.0, 2.0, 4.0, 5.0, 6.0, 8.0 }; // skew centre
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew

                string inputDir = parentDir + @"\TrainingCSV";
                int speciesNumber = 50;

                string speciesCountFile = parentDir + @"\AmazonBird50_training_Counts.txt";
                var lines = FileTools.ReadTextFile(speciesCountFile);
                int[] speciesCounts = new int[speciesNumber];
                for (int i = 0; i < speciesNumber; i++)
                {
                    string[] words = lines[i].Split(',');
                    speciesCounts[i] = int.Parse(words[1]);
                }

                double Hspecies = DataTools.EntropyNormalised(speciesCounts);
                Console.WriteLine("Species Entropy = " + Hspecies);

                int freqBinCount = 256;
                int reducedBinCount = freqBinCount;

                //int reductionFactor = 1;
                //reducedBinCount = freqBinCount / reductionFactor;
                reducedBinCount = 100 + (156 / 2); // exotic style

                // data structure to contain probability info
                int[,,] probSgivenF = new int[reducedBinCount, speciesNumber, valueResolution];

                DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
                string pattern = "*." + key + ".csv";
                FileInfo[] filePaths = inputDirInfo.GetFiles(pattern);

                // read through all the files
                int fileCount = filePaths.Length;

                //fileCount = 3;
                for (int i = 0; i < fileCount; i++)
                {
                    //ID0001_Species01.EVN.csv
                    char[] delimiters = { '.', 's'};
                    string fileName = filePaths[i].Name;
                    string[] parts = fileName.Split(delimiters);
                    int speciesId = int.Parse(parts[1]);
                    double[,] matrix = null;
                    if (filePaths[i].Exists)
                    {
                        int binCount;
                        matrix = IndexMatrices.ReadSpectrogram(filePaths[i], out binCount);

                        //string match = @"CornellMarine_*__ACI-ENT-EVN.SpectralRibbon.png";
                        //string opFileStem = "CornellMarine.ACI-ENT-EVN.SpectralRibbon.2013MarchApril";
                        // WARNING: POW was removed in December 2018
                        string match = @"CornellMarine_*__BGN-POW-EVN.SpectralRibbon.png";
                        string opFileStem = "CornellMarine.BGN-POW-EVN.SpectralRibbon.2013MarchApril";

                        FileInfo tidalDataFile = new FileInfo(@"C:\SensorNetworks\OutputDataSets\GeorgiaTides2013.txt");

                        //SunAndMoon.SunMoonTides[] tidalInfo = null;
                        SunAndMoon.SunMoonTides[] tidalInfo = SunAndMoon.ReadGeorgiaTidalInformation(tidalDataFile);

//                        ConcatenateIndexFiles.ConcatenateRibbonImages(
//                            dataDirs,
//                            match,
//                            outputDirectory,
//                            opFileStem,
//                            title,
//                            tidalInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Concatenate images horizontally or vertically.
        /// </summary>
        public static void ConcatenateImages2()
        {
            // Concatenate three images for Dan Stowell.
            //var imageDirectory = new DirectoryInfo(@"H:\Documents\SensorNetworks\MyPapers\2016_QMUL_SchoolMagazine");
            //string fileName1 = @"TNC_Musiamunat_20150702_BAR10__ACI-ENT-EVNCropped.png";
            //string fileName2 = @"GympieNP_20150701__ACI-ENT-EVN.png";
            //string fileName3 = @"Sturt-Mistletoe_20150702__ACI-ENT-EVN - Corrected.png";
            //string opFileName = string.Format("ThreeLongDurationSpectrograms.png");

//                    for (int r = 0; r < speciesNumber; r++)
//                    {
//                        for (int c = 0; c < valueResolution; c++)
//                        {
//                            m[r, c] = probSgivenF[i, r, c];
//                        }
//                    }
//                    double[]  array = DataTools.Matrix2Array(m);
//                    double entropy = DataTools.EntropyNormalised(array);
//                    mi[i] = entropy;
//
//
//            var image1Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName1));
//            var image2Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName2));
            //var image3Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName3));

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
        }

        /// <summary>
        /// FROG DATA SET
        /// To produce observe feature spectra
        /// This is used to analyse frog recordings of Lin Schwarzkopf.
        /// </summary>
        public static void AnalyseFrogDataSet()
        {
            //HighResolutionAcousticIndices.Execute(null);
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

        /// <summary>
        /// This method was scavanged from Human1.cs It indicates where wav files and results may still be found
        /// </summary>
        public static void HumanVoiceRecogniser()
        {
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows_Cassandra\Crows111216-001Mono5-7min.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\PramukSpeech_20090615.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Wimmer_DM420011.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\BAC2_20071018-143516_speech.wav";
            string recordingPath =
                @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
            string configPath = @"C:\SensorNetworks\Output\Human\Human.cfg";
            string outputDir = @"C:\SensorNetworks\Output\Human\";
        }

        public static void ResourcesForRheobatrachusSilusRecogniser()
        {
            string AnalysisName = "RheobatrachusSilus";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav";  //POSITIVE
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Adelotus_brevis_TuskedFrog_BridgeCreek.wav";   //NEGATIVE walking on dry leaves
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min602.wav";   //NEGATIVE  rain
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Noise\BAC3_20070924-153657_noise.wav";  //NEGATIVE  noise
            string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Compilation6_Mono.mp3"; //FROG COMPILATION
            string configPath =
                @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.RheobatrachusSilus.cfg";
            string outputDir = @"C:\SensorNetworks\Output\Frogs\";
        }

        public static void ResourcesForCrowCallRecognition()
        {
            string recordingPath = @"C:\SensorNetworks\WavFiles\Crows_Cassandra\Crows111216-001Mono5-7min.mp3";
            string configPath = @"C:\SensorNetworks\Output\Crow\Crow.cfg";
            string outputDir = @"C:\SensorNetworks\Output\Crow\";
            string title = "# FOR DETECTION OF CROW CALLS - version 2";
        }

        public static void ResourcesForEventPatternRecognition()
        {
            //EPR was used for ground parrot detection.
            // See EPR.cs
            // GROUND PARROT recording at
            // epr2 "C:\SensorNetworks\WavFiles\GroundParrot\Aug2010_Site1\audio\DM420013_0342m_00s__0344m_00s.mp3"
            //       C:\SensorNetworks\Output\EPR_GroundParrot\EPR_GroundParrot_Params.txt gp1
        }

        /// <summary>
        /// Experiments with noise reduction of spectrograms
        /// THIS METHOD IS JUST A CONTAINER FOR TESTING SNIPPETS OF CODE TO DO WITH NOISE REMOVAL FROM SPECTROGRAMS
        /// THE FOLLOWING CODE tests the use of the Noise Reduction types listed in the enum SNR.NoiseReductionType
        /// The enum types include NONE, STANDARD, MODAL, Binary, etc.
        /// </summary>
        public static void TestNoiseReduction()
        {
            //#######################################################################################################################################
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            string wavFilePath = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC\BAC2_20071008-085040.wav";
            string outputDir = @"C:\SensorNetworks\Output\Test\Test2018";
            string profileName = "Median";
            //string profileName = "Mean";
            //string profileName = "Mean";
            string imageFname = "test_" + profileName + "NoiseRemoval.png";

            string imagePath = Path.Combine(outputDir, imageFname);
            string imageViewer = @"C:\Windows\system32\mspaint.exe";

            var recording = new AudioRecording(wavFilePath);

            int nyquist = recording.Nyquist;
            int frameSize = 1024;
            int finalBinCount = 512;
            int hertzInterval = 1000;
            FreqScaleType scaleType = FreqScaleType.Linear;

            //FreqScaleType scaleType = FreqScaleType.Mel;
            var freqScale = new FrequencyScale(scaleType, nyquist, frameSize, finalBinCount, hertzInterval);

            var fst = freqScale.ScaleType;
            var config = new SonogramConfig
            {
                //NoiseReductionType = NoiseReductionType.Modal,
                //NoiseReductionType = NoiseReductionType.Mean,
                NoiseReductionType = NoiseReductionType.Median,
                WindowOverlap = 0.0,
                WindowSize = frameSize,
            };
            config.NoiseReductionParameter = 0.0; // backgroundNeighbourhood noise reduction in dB

            // DRAW SPECTROGRAM
            var spectrogram = new SpectrogramStandard(config, recording.WavReader);
            var image = spectrogram.GetImageFullyAnnotated(spectrogram.GetImage(),
                "SPECTROGRAM: " + fst + " " + profileName, freqScale.GridLineLocations);
            image.Save(imagePath, ImageFormat.Png);
            FileInfo fiImage = new FileInfo(imagePath);

            // displaying images no longer supported, line removed

            // #################################################################################################
            // THE FOLLOWING CODE tests the effect of changing the order of 1) CONVERT TO dB 2) NOISE REMOVAL
            //                                                     versus  1) NOISE REMOVAL 2) CONVERT TO dB.
            // THe results are very different. The former is GOOD. The latter is A MESS.
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            //string wavFilePath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
            //string outputDir = @"C:\SensorNetworks\Output\Test";
            //string imageFname = "test3.png";

            //string imagePath = Path.Combine(outputDir, imageFname);
            //string imageViewer = @"C:\Windows\system32\mspaint.exe";

            //var recording = new AudioRecording(wavFilePath);
            //int frameSize = 512;
            //double windowOverlap = 0.0;
            //// i: EXTRACT ENVELOPE and FFTs
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);

            //// get amplitude spectrogram and remove the DC column ie column zero.
            //double[,] spectrogramData = results2.Spectrogram;
            //spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, spectrogramData.GetLength(1) - 1);
            //double epsilon = Math.Pow(0.5, 16 - 1);
            //double windowPower = frameSize * 0.66; //power of a rectangular window =frameSize. Hanning is less

            //// convert spectrum to decibels BEFORE noise removal
            ////spectrogramData = Speech.DecibelSpectra(spectrogramData, windowPower, recording.SampleRate, epsilon);

            //// vi: remove background noise from the spectrogram
            //double SD_COUNT = 0.1;
            //double SpectralBgThreshold = 0.003; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            //SNR.NoiseProfile profile = SNR.CalculateNoiseProfile(spectrogramData, SD_COUNT); //calculate noise profile - assumes a dB spectrogram.
            //double[] noiseValues = DataTools.filterMovingAverage(profile.noiseThreshold, 7);      // smooth the noise profile
            //spectrogramData = SNR.NoiseReduce_Standard(spectrogramData, noiseValues, SpectralBgThreshold);

            //// convert spectrum to decibels AFTER noise removal
            ////spectrogramData = Speech.DecibelSpectra(spectrogramData, windowPower, recording.SampleRate, epsilon);

            //spectrogramData = MatrixTools.MatrixRotate90Anticlockwise(spectrogramData);
            //ImageTools.DrawMatrix(spectrogramData, imagePath);
            //FileInfo fiImage = new FileInfo(imagePath);
            //if (fiImage.Exists) // Display the image using MsPaint.exe
            //{
            //    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
            //    process.Run(imagePath, outputDir);
            //}
        }

        public static void TestDct(string[] args)
        {
            int dctLength = 32;
            int coeffCount = 32;

            double[,] cosines = MFCCStuff.Cosines(dctLength, coeffCount + 1); //set up the cosine coefficients

            var array = new double[dctLength];

            //linear spectrum
            // for (int i = 0; i < DCTLength; i++) if((i % 16)==0) array[i] = 10;

            //sinusoid spectrum
            int k = 4;
            double kPiOnM = k * 2 * Math.PI / dctLength;
            for (int i = 0; i < dctLength; i++)
            {
                array[i] = 10 * Math.Cos(kPiOnM * (i + 0.5)); //can also be Cos(kPiOnM * (m - 0.5)
            }

            //array = DataTools.SubtractMean(array);
            array = DataTools.normalise2UnitLength(array);

            //array = DataTools.NormaliseMatrixValues(array);
            DataTools.writeBarGraph(array);

            double[] dct = MFCCStuff.DCT(array, cosines);
            for (int i = 0; i < dct.Length; i++)
            {
                dct[i] = Math.Abs(dct[i] * 10);
            }

            dct[0] = 0.0; //dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0;

            //int maxIndex = DataTools.GetMaxIndex(dct);
            //double max = dct[maxIndex];
            //DataTools.MinMax(dct, out min, out max);
            DataTools.writeBarGraph(dct);
            LoggedConsole.WriteLine("FINISHED");
            Console.ReadLine();
        }

        /*
        /// <summary>
        /// this code written for Manoosh to demonstrate drawing images of clusters
        /// </summary>
        public static void CodeToDrawClusterImage()
        {
            string pathToClusterCsvFile = "theFilePath";
            string pathToOutputImageFile = "theImagePath";

            List<double[,]> clusterList = new FileTools.ReadClusterDataFromFile(pathToClusterCsvFile);

            var images = new List<Image>();

            int spacerWidth = 2;
            int binCount = 128;
            Image spacer = new Bitmap(spacerWidth, binCount);
            Graphics g = Graphics.FromImage(spacer);
            g.Clear(Color.BlanchedAlmond);

            int[] sortOrder = { 14, 45, 3, 19 }; //etc

            for (int i = 0; i < sortOrder.Length; i++)
            {
                Image image = ImageTools.DrawNormalisedMatrix(clusterList[sortOrder[i]]);
                // OR
                // adapt the following method to draw matrix scaled up in size
                //Image image = ImageTools.DrawMatrix(double[,] matrix, string pathName, bool doScale);

                images.Add(image);
                images.Add(spacer);
            }

            Bitmap combinedImage = (Bitmap)ImageTools.CombineImagesInLine(images);

            // set up the mel frequency scale
            int finalBinCount = 128;
            var frequencyScale = new FrequencyScale(FreqScaleType.Mel, 11025, 512, finalBinCount, hertzGridInterval: 1000);

            FrequencyScale.DrawFrequencyLinesOnImage(combinedImage, frequencyScale.GridLineLocations, includeLabels: false);
            combinedImage.Save(pathToOutputImageFile);
        } */

        /// <summary>
        /// This code used to get LDFC spectrograms from availae
        /// It cycles through all the subdirecotries in a dir.
        /// All depends on the consistency of file naming.
        /// Check the call method for index names and bounds.
        /// </summary>
        public static void CodeToGetLdfcSpectrogramsFromAvailae()
        {
            // Lewin's Rail
            //var sourceDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\Original concatenated index files ARU10");
            //var sourceDir = new DirectoryInfo(superDir + @"\David Watson_Liz_Tasmania_ARU10\ARU 10 27.12.2016 Data");

            // Black Rail
            //string superDir = @"Y:\Results\2017Apr13-135831 - Liz, Towsey.Indices, ICD=60.0, #154\ConcatResults";
            //var sourceDir = new DirectoryInfo(superDir + @"\David Watson_Liz_USA - South Carolina_ARU UNIT 7");
            //var sourceDir = new DirectoryInfo(superDir + @"\David Watson_Liz_USA - South Carolina_ARU UNIT 10");
            //var outputDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\BlackRail\UnlabelledDataSets\Job154_2017Apr13_135831 SouthCarolina\ARU10_spectrograms");
            //var outputDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\BlackRail\UnlabelledDataSets\Job154_2017Apr13_135831 SouthCarolina\ARU7_spectrograms");
            //string searchPattern = "2016*";

            // Least Bittern
            string superDir = @"Y:\Results\2017Jun26-111643- Liz, Towsey.Indices, ICD=60.0, #160\ConcatResults2\David Watson_Liz_Oak Ridge";
            //var sourceDir = new DirectoryInfo(superDir + @"\ORNL ARU 2 6.6.2017 Powerhouse trail Data");
            //var sourceDir = new DirectoryInfo(superDir + @"\ORNL ARU 2 17.5.2017 Data");
            var sourceDir = new DirectoryInfo(superDir + @"\ORNL ARU 2 28.5.2017");
            var outputDir = new DirectoryInfo(@"G:\SensorNetworks\Collaborations\LizZnidersic\LeastBittern\ARU2_ORNL_2017_LEBI_LZ_Spectrograms");
            string searchPattern = "20170*";

            if (!outputDir.Exists)
            {
                outputDir.Create();
            }

            DirectoryInfo[] dirs = sourceDir.GetDirectories(searchPattern, SearchOption.AllDirectories);
            Console.WriteLine("Dir Count = " + dirs.Length);
            foreach (DirectoryInfo dir in dirs)
            {
                // assume this file exists
                var fileinfo = dir.GetFiles("*__ACI-ENT-EVN.png");
                string site = fileinfo[0].Name.Split('_')[0];
                //string site = sourceDir.Name;
                string date = dir.Name;
                string siteAndDate = site + "_" + date;
                string opFileName = siteAndDate + "_ACI-ENT-EVN.png";

                foreach (var file in fileinfo)
                {
                    Console.WriteLine("Copying file:: " + file.Name);
                    var opFileInfo = new FileInfo(Path.Combine(outputDir.FullName, opFileName));
                    file.CopyTo(opFileInfo.FullName);
                }
            }
        }

        /// <summary>
        /// Places score tracks under a Single Image, assumed to be a spectrogram
        /// </summary>
        public static void CodeToPlaceScoreTracksUnderSingleImage()
        {
            // construct paths to files
            string dirName = @"C:\SensorNetworks\Collaborations\LizZnidersic\LeastBittern";
            //string imageName = "ORNL ARU 2 6.6.2017 Powerhouse trail Data_20170601__2Maps.png";
            //var scoreFile = new FileInfo(Path.Combine(dirName, "ARU2_ORNL_20170601_LEBI_LZ_TrainingLabels.csv"));
            string imageName = "ORNL ARU 2 6.6.2017 Powerhouse trail Data_20170604__2Maps.png";
            var scoreFile = new FileInfo(Path.Combine(dirName, "ARU2_ORNL_20170604_LEBI_LZ_ValidationLabels.csv"));

            var imageFile = new FileInfo(Path.Combine(dirName, imageName));
            var opFile = Path.Combine(dirName, imageName + "WithScores.png");

            double threshold = 3.5;
            double maxScore = 15.0;
            CodeToPlaceScoreTrackUnderImageFile(imageFile, scoreFile, opFile, maxScore, threshold);
        }

        /// <summary>
        /// This code used to get LDFC spectrograms from availae
        /// It cycles through all the subdirecotries in a dir.
        /// All depends on the consistency of file naming.
        /// Check the call method for index names and bounds.
        /// </summary>
        public static void CodeToPlaceScoreTracksUnderLdfcSpectrograms()
        {
            // Black rail directories
            //var scoreDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\BlackRail\UnlabelledDataSets\Job154_2017Apr13_135831 SouthCarolina\ARU7_predictions");
            //var imageDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\BlackRail\UnlabelledDataSets\Job154_2017Apr13_135831 SouthCarolina\ARU7_spectrograms");
            //var outputDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\BlackRail\UnlabelledDataSets\Job154_2017Apr13_135831 SouthCarolina\ARU7_spectrogramsWithScores");

            // Least Bittern directories
            var scoreDir = new DirectoryInfo(@"G:\SensorNetworks\Collaborations\LizZnidersic\LeastBittern\UnlabelledDataSets_Job160_predictions");
            var imageDir = new DirectoryInfo(@"G:\SensorNetworks\Collaborations\LizZnidersic\LeastBittern\ARU2_ORNL_2017_LEBI_LZ_Spectrograms");
            var outputDir = new DirectoryInfo(@"G:\SensorNetworks\Collaborations\LizZnidersic\LeastBittern\ARU2_ORNL_2017_LEBI_LZ_SpectrogramsWithScores");

            if (!outputDir.Exists)
            {
                outputDir.Create();
            }

            var imageFiles = imageDir.GetFiles("*ACI-ENT-EVN.png");
            double threshold = 3.5;
            double maxScore = 15.0;

            Console.WriteLine("File Count= " + imageFiles.Length);
            foreach (var spImageFile in imageFiles)
            {
                // assume this file exists
                string site = spImageFile.Name.Split('_')[0];
                string date = spImageFile.Name.Split('_')[1];
                string scoreFileName = site + "_" + date + "_FeatureSet3.csv";

                // get the corresponding csv file of scores
                var scoreFile = new FileInfo(Path.Combine(scoreDir.FullName, scoreFileName));

                // construct path to output file
                var opFile = Path.Combine(outputDir.FullName, spImageFile.Name);
                CodeToPlaceScoreTrackUnderImageFile(spImageFile, scoreFile, opFile, maxScore, threshold);
            }
        }

        public static void CodeToPlaceScoreTrackUnderImageFile(FileInfo imageFile, FileInfo scoreFile, string opFile, double maxScore, double threshold)
        {
            // read in the image file - assumed to be spectrogram
            Image spectrogram = ImageTools.ReadImage2Bitmap(imageFile.FullName);

            // Cannot get the following line to work, so use the depracated method
            //var data1 = Csv.ReadMatrixFromCsv<double>(scoreFile);
            var data = CsvTools.ReadColumnOfCsvFile(scoreFile.FullName, 1, out string header);

            // create a score track
            string name = "Predictions (max=" + maxScore + ")";
            var scoreTrack = ImageTrack.GetNamedScoreTrack(data, scoreMin: 0.0, scoreMax: maxScore, scoreThreshold: threshold, name: name);

            // attach score track to the LDFC spectrogram
            var scoreImage = new Bitmap(spectrogram.Width, 40);
            scoreTrack.DrawTrack(scoreImage);
            Image[] images = { spectrogram, scoreImage };
            var combinedImage = ImageTools.CombineImagesVertically(images);

            // write image to file
            //Console.WriteLine("Copying file:: " + fileinfo.Name);
            combinedImage.Save(opFile);
        }

        /// <summary>
        /// This code can be used to extract acoustic indices for recognisers.
        /// It cycles through all the subdirecotries in a dir.
        /// All depends on the consistency of file naming.
        /// Check the call method for index names and bounds.
        /// </summary>
        public static void CodeToExtractFeatureVectorOfIndices()
        {
            /*
            // THESE ARE PATHS FOR LEWINS RAIL PROJECT
            //var sourceDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\Original concatenated index files ARU10");
            //var sourceDir = new DirectoryInfo(superDir + @"\David Watson_Liz_Tasmania_ARU10\ARU 10 27.12.2016 Data");
            */

            /*
            // THESE ARE PATHS FOR BLACK RAIL PROJECT
            //string superDir = @"Y:\Results\2017Jun05-113313 - Liz, Towsey.Indices, ICD=60.0, #154\ConcatResults";
            string superDir = @"Y:\Results\2017Apr13-135831 - Liz, Towsey.Indices, ICD=60.0, #154\ConcatResults";
            //var sourceDir = new DirectoryInfo(superDir + @"\David Watson_Liz_USA - South Carolina_ARU UNIT 7");
            var sourceDir = new DirectoryInfo(superDir + @"\David Watson_Liz_USA - South Carolina_ARU UNIT 10");
            string searchPattern = "2016*";

            var outputDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\BlackRail\UnlabelledDataSets\Job154_2017Apr13_135831 SouthCarolina\ARU10");
            //var outputDir = new DirectoryInfo(@"C:\SensorNetworks\Collaborations\LizZnidersic\BlackRail\UnlabelledDataSets\Job154_2017Apr13_135831 SouthCarolina\ARU7");
            */

            // THESE ARE PATHS FOR LEAST BITTERN PROJECT
            // data on Y drive
            //string superDir = @"Y:\Results\2017Jun26-111643- Liz, Towsey.Indices, ICD=60.0, #160\ConcatResults";
            //var sourceDir = new DirectoryInfo(superDir + @"\David Watson_Liz_Oak Ridge\ORNL ARU 2 6.6.2017 Powerhouse trail Data");

            // data from G drive
            string superDir = @"G:\SensorNetworks\Collaborations\LizZnidersic\LeastBittern\OriginalResultsData";
            //var sourceDir = new DirectoryInfo(superDir + @"\ORNL ARU 2 6.6.2017 Powerhouse trail Data");
            //var sourceDir = new DirectoryInfo(superDir + @"\ORNL ARU 2 17.5.2017 Data");
            var sourceDir = new DirectoryInfo(superDir + @"\ORNL ARU 2 6.6.2017 Powerhouse trail Data");

            string searchPattern = "20170*";

            var outputDir = new DirectoryInfo(@"G:\SensorNetworks\Collaborations\LizZnidersic\LeastBittern\UnlabelledDataSets_Job160");

            if (!outputDir.Exists)
            {
                outputDir.Create();
            }

            DirectoryInfo[] dirs = sourceDir.GetDirectories(searchPattern, SearchOption.AllDirectories);
            Console.WriteLine("Dir Count = " + dirs.Length);
            foreach (DirectoryInfo dir in dirs)
            {
                // assume this file exists
                var fileinfo = dir.GetFiles("*.ACI.csv");
                string site = fileinfo[0].Name.Split('_')[0];
                //string site = sourceDir.Name;
                string date = dir.Name;
                string siteAndDate = site + "_" + date;

                string filePrefix = siteAndDate + "__Towsey.Acoustic.";
                string opFileName = siteAndDate + "_FeatureSet3.csv";
                var opFileInfo = new FileInfo(Path.Combine(outputDir.FullName, opFileName));
                Console.WriteLine("Extracting dir " + dir.Name);
                ExtractFeatureVectorOfIndices(dir, filePrefix, siteAndDate, opFileInfo);
            }
        }

        /// <summary>
        /// This code used to extract acoustic indices for recognisers.
        /// </summary>
        public static void ExtractFeatureVectorOfIndices(DirectoryInfo sourceDir, string filePrefix, string siteAndDate, FileInfo opFileInfo)
        {
            // acooustic indices/features to select and the start and end frequency bins
            /*
            // BLACK RAIL
            string[] indexCodes = { "ACI", "ENT", "EVN" };
            int startBin = 22;
            int endBin = 74;
            */

            // LEAST BITTERN
            //string[] indexCodes = { "ACI", "ENT", "EVN", "R3D" };
            string[] indexCodes = { "ACI", "ENT", "EVN" };
            int startBin = 12;
            int endbin = 18;

            int indexCount = indexCodes.Length;
            int length = endbin - startBin + 1;

            // matrix of string
            var extractedLines = new List<List<string>>();

            // init a new header line showing source INDEX
            var newHeader = new StringBuilder();

            // loop through all required index files and create the header line
            for (int keyId = 0; keyId < indexCount; keyId++)
            {
                string indexKey = indexCodes[keyId];

                for (int i = 0; i < length; i++)
                {
                    int id = i + startBin;
                    newHeader.Append(indexKey + id.ToString("D4") + ",");
                }
            }

            newHeader.Append("Target");

            for (int keyId = 0; keyId < indexCount; keyId++)
            {
                string indexKey = indexCodes[keyId];
                var fileInfo = new FileInfo(Path.Combine(sourceDir.FullName, filePrefix + indexKey + ".csv"));

                // init var to hold required data columns
                var lines = new List<string>();

                // read the file, line at a time and extract the required columns from the data matrix
                using (TextReader reader = new StreamReader(fileInfo.FullName))
                {
                    // read and ignore the first line in source file which is a header.
                    string line = reader.ReadLine();

                    while ((line = reader.ReadLine()) != null)
                    {
                        //read one line at a time into an array of string
                        var words = line.Split(',');

                        // take subarray. +1 because the first column containing ID is ignored.
                        var subArray = DataTools.Subarray(words, startBin + 1, length);
                        var newLine = DataTools.Array2String(subArray);
                        lines.Add(newLine);
                    }
                } //end using

                extractedLines.Add(lines);
            }

            // prepare output file
            using (StreamWriter sw = new StreamWriter(opFileInfo.FullName))
            {
                using (TextWriter ssw = TextWriter.Synchronized(sw))
                {
                    ssw.WriteLine(newHeader.ToString());

                    // now join the lines into a feature vector
                    int lineCount = extractedLines[0].Count;
                    for (int i = 0; i < lineCount; i++)
                    {
                        string line = string.Empty;
                        for (int j = 0; j < indexCodes.Length; j++)
                        {
                            line += extractedLines[j][i];
                        }

                        // add '?' as place holder for an unknown category to be predicted
                        // line += "?";
                        // add '0' as place holder for a numeric value to be predicted
                        line += "0";
                        ssw.WriteLine(line);
                    }
                }
            }
        } // end CodeToExtractFeatureVectorOfIndices()
    }
}
