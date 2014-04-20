using System;
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

            //FOR  LITTLE SPOTTED KIWI3
            // audio2csv  "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg" C:\SensorNetworks\Output\LSKiwi3\

            //RAIN
            // audio2csv "C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg" "C:\SensorNetworks\Output\Rain"

            // CHECKING 16Hz PROBLEMS
            // audio2csv  "C:\SensorNetworks\WavFiles\Frogs\Curramore\CurramoreSelection-mono16kHz.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"
            // audio2csv  "C:\SensorNetworks\WavFiles\16HzRecording\CREDO1_20120607_063200.mp3"          "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\AudioToCSV"

            // SERF TAGGED RECORDINGS FROM OCT 2010
            // audio2csv  "Z:\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"  "C:\SensorNetworks\Output\SERF\2013Analysis\13Oct2010" 

            // DEV RECORDINGS
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav"; //2 min recording
            //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\DaguilarGoldCreek1_DM420157_0000m_00s__0059m_47s_49h.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
            //string recordingPath = @"Y:\Eclipise 2012\Eclipse\Site 4 - Farmstay\ECLIPSE3_20121115_040001.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_TUITCE_20091215_220004.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\TEST_4min_artificial.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\groundParrot_Perigian_TEST.wav";

            // DEV CONFIG OPTIONS
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Human.cfg";
            //string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Crow.cfg";
            return new AnalyseLongRecording.Arguments
            {
                Source = recordingPath.ToFileInfo(),
                Config = configPath.ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\LSKiwi3\Test_Dec2013".ToDirectoryInfo()
                //Output = @"C:\SensorNetworks\Output\LSKiwi3\Test_07April2014".ToDirectoryInfo()
                Output = @"C:\SensorNetworks\Output\Test\Test_19April2014".ToDirectoryInfo()
            };

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
