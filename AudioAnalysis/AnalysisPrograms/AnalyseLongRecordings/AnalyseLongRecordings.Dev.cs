// <copyright file="AnalyseLongRecordings.Dev.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.AnalyseLongRecordings
{
    using System;

    public partial class AnalyseLongRecording
    {
        public static Arguments Dev()
        {
            // TO GET TO HERE audio2csv MUST BE ONLY COMMAND LINE ARGUMENT
            // If you end up with indices and no images, then, to draw the false-colour spectrograms,
            //          you must use the activity code "colourspectrogram"
            // This calls AnalysisPrograms.DrawLongDurationSpectrograms.Execute() to produce LD FC spectrograms from matrices of indices.
            // See line 176 for example using the Pillaga Forest data.

            // If your index calculation duration (ICD) < 60s then this call will NOT additionally produce false-colour spectrograms.
            // As next step, you need to use Action code = ColourSpectrogram
            //     and enter program through AnalysisPrograms.DrawLongDurationSpectrograms

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

            var arguments = new Arguments
                                {
                                    Source = recordingPath.ToFileInfo(),
                                    Config = configPath.ToFileInfo(),
                                    Output = outputPath.ToDirectoryInfo(),
                                    MixDownToMono = true,
            };

            // #########  NOTE: All other parameters are set in the <Ecosounds.MultiRecognizer.yml> file
            if (!arguments.Source.Exists)
            {
                Log.Warn(" >>>>>>>>>>>> WARNING! The Source Recording file cannot be found! This will cause an exception.");
            }

            if (!arguments.Config.Exists)
            {
                Log.Warn(" >>>>>>>>>>>> WARNING! The Configuration file cannot be found! This will cause an exception.");
            }

            return arguments;
        }
    }
}