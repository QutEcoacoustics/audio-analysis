﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.AnalyseLongRecordings
{
    public partial class AnalyseLongRecording
    {
        public static Arguments Dev()
        {
            //use the following paths for the command line.

            // TO GET TO HERE audio2csv MUST BE ONLY COMMAND LINE ARGUMENT

            // testing for running on bigdata
            // "F:\Projects\QUT\qut-svn-trunk\AudioAnalysis\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe" audio2csv -source "F:\Projects\test-audio\cabin_EarlyMorning4_CatBirds20091101-000000.wav" -config "F:\Projects\QUT\qut-svn-trunk\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg" -output "F:\Projects\test-audio\results" -tempdir "F:\Projects\test-audio\results\temp"

            // COMMAND LINES FOR  ACOUSTIC INDICES
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"         "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"           "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\LSKiwi3"
            // SUNSHINE COAST
            // audio2csv  "Z:\Sunshine Coast\Site1\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SunshineCoast\Acoustic\Site1"
            // audio2csv  "C:\SensorNetworks\WavFiles\SunshineCoast\DM420036.MP3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SunshineCoast"

            // THE COMMAND LINES DERIVED FROM ABOVE for the <audio2csv> task. 
            //FOR  MULTI-ANALYSER and CROWS
            //audio2csv  "C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg" "C:\SensorNetworks\Output\Test1"

            //MARINE 
            //string recordingPath = @"C:\SensorNetworks\WavFiles\MarineRecordings\20130318_171500.wav";
            //string configPath  = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.SonogramMarine.yml";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticMarine.yml";
            //string outputPath = @"C:\SensorNetworks\Output\MarineSonograms\Test1";


            //RAIN
            // audio2csv "C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg" "C:\SensorNetworks\Output\Rain"

            // CHECKING 16Hz PROBLEMS
            // audio2csv  "C:\SensorNetworks\WavFiles\Frogs\Curramore\CurramoreSelection-mono16kHz.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"
            // audio2csv  "C:\SensorNetworks\WavFiles\16HzRecording\CREDO1_20120607_063200.mp3"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"

            // SERF TAGGED RECORDINGS FROM OCT 2010
            // audio2csv  "Z:\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SERF\2013Analysis\13Oct2010" 

            // ARTIFICIAL TEST RECORDING
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_4min_artificial.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\Test\Test2";
            //string outputPath    = @"C:\SensorNetworks\Output\FalseColourSpectrograms";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_1min_artificial.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Test\Test";

            // GROUND PARROT
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\groundParrot_Perigian_TEST.wav";

            // KOALA RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav"; //2 min recording
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\KoalaMale\HiRes";

            // TUI TERRACE RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_Farmstay_ECLIPSE3_20121114-060001+1000.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_TUITCE_20091215_220004.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\LSKiwi3\Test_Dec2013";
            //string outputPath    = @"C:\SensorNetworks\Output\LSKiwi3\Test_07April2014";
            //string outputPath = @"C:\SensorNetworks\Output\Test\TestKiwi";


            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
            //string recordingPath = @"Y:\Eclipise 2012\Eclipse\Site 4 - Farmstay\ECLIPSE3_20121115_040001.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Eclipse2012\Farmstay_ECLIPSE3_20121114_060001TEST.wav";
            //COMMAND LINE FOR  LITTLE SPOTTED KIWI3
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg" C:\SensorNetworks\Output\LSKiwi3\

            // CANETOAD RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\CaneToads_rural1_20.mp3";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Canetoad\CaneToads_rural1_20.mp3";
            //string recordingPath = @"F:\SensorNetworks\WavFiles\CaneToad\CaneToad Release Call 270213-8.wav";
            //string recordingPath = @"F:\SensorNetworks\WavFiles\CaneToad\UndetectedCalls-2014\KiyomiUndetected210214-1.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\FrogRecording_2.wav";
            //string recordingPath = @"C:\SensorNetworks\Output\Frogs\FrogPondSamford\FrogPond_Samford_SE_555_20101023-000000.mp3";
            //string recordingPath = @"C:\SensorNetworks\Output\Frogs\FrogPondSamford\FrogPond_Samford_SE_555_20101023-000000_0min.wav";

            string outputPath    = @"C:\SensorNetworks\Output\Frogs\Canetoad\Test2016-3";
            //string outputPath    = @"C:\SensorNetworks\Output\Frogs\CanetoadAcousticIndices";
            //string outputPath    = @"C:\SensorNetworks\Output\Frogs\SamfordTest";

            // ECLIPSE FARMSTAY
            //string recordingPath = @"Y:\Eclipise 2012\Eclipse\Site 4 - Farmstay\ECLIPSE3_20121115_040001.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Eclipse2012\Farmstay_ECLIPSE3_20121114_060001TEST.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_Farmstay_ECLIPSE3_20121114-060001+1000.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\ZoomImages2";

            // ST BEES KOALA RECORDING TWO HOURS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\WESTKNOLL_20140905-001853+1000.wav";
            //string outputPath = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016";

            // BAC recordings
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC2_20071008-085040.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\BAC\";

            //BIRD50 recordings from Herve Glotin
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Glotin-Bird50\AmazonBird50_testing_input\ID1268.wav";
            //string outputPath = @"C:\SensorNetworks\Output\BIRD50\";

            // EASTERN BRISTLE BIRD
            //string recordingPath = @"F:\SensorNetworks\WavFiles\EasternBristlebird\CURRUMBIN_20150529-142503+1000.wav";
            //string outputPath    = @"C:\SensorNetworks\Output\BristleBird";





            // CONFIG FILES ######################################################################################################
            // Use these configs for Summary and Spectral Indices
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Italy.Acoustic.Parallel.yml";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";

            // Use this config when doing high resolution indices PLUS species recognisers
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticHiResPlusRecognisers.yml";

            // Use these configs for Call recognition Indices
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.yml";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Human.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Crow.cfg";

            // DEV CONFIG OPTIONS
            //C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisPrograms\App.config


            var arguments = new Arguments
                                {
                                    Source = recordingPath.ToFileInfo(),
                                    Config = configPath.ToFileInfo(),
                                    Output = outputPath.ToDirectoryInfo()
            };

            if (!arguments.Source.Exists)
            {
                Log.Warn(" >>>>>>>>>>>> WARNING! The Source Recording file cannot be found! This will cause an exception.");
            }
            if (!arguments.Config.Exists)
            {
                Log.Warn(" >>>>>>>>>>>> WARNING! The Configuration file cannot be found! This will cause an exception.");
            }

            return arguments;

            // ACOUSTIC_INDICES_LSK_TUITCE_20091215_220004
            /*return new Arguments
                   {
                       Source = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav".ToFileInfo(),
                       Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
                       Output = @"C:\SensorNetworks\Output\LSKiwi3\AfterRefactoring".ToDirectoryInfo()
                   };*/

            // ACOUSTIC_INDICES_SERF_SE_2010OCT13
            //return new Arguments
            //   {
            //       Source = @"Z:\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3".ToFileInfo(),
            //       Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //       Output = @"C:\SensorNetworks\Output\SERF\AfterRefactoring".ToDirectoryInfo()
            //   };

            // ACOUSTIC_INDICES_SUNSHINE_COAST SITE1 
            //return new Arguments
            //{
            //    Source = @"D:\Anthony escience Experiment data\4c77b524-1857-4550-afaa-c0ebe5e3960a_101013-0000.mp3".ToFileInfo(),
            //    Config = @"C:\Work\Sensors\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    Output = @"C:\tmp\results\".ToDirectoryInfo()
            //};
            //return new Arguments
            //{
            //    Source = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036.MP3".ToFileInfo(),
            //    Config = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    Output = @"C:\SensorNetworks\Output\SunshineCoast\Site1\".ToDirectoryInfo()
            //};


            throw new NotImplementedException();
        }
    }
}