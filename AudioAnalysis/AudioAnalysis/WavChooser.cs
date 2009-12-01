using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AudioAnalysis
{
    static class WavChooser
    {


        public static void ChooseWavFile(out string wavDirName, out string wavFileName, out AudioRecording recording)
        {
            //BRISBANE AIRPORT CORP
            //wavDirName = @"C:\SensorNetworks\WavFiles\";
            //wavFileName = "sineSignal";
            //wavFileName = "golden-whistler";
            //wavFileName = "BAC2_20071008-085040";            //Lewin's rail kek keks - source file for template 2.
            //wavFileName = "BAC1_20071008-084607";            //faint kek-kek call
            //wavFileName = "BAC2_20071011-182040_cicada";     //repeated cicada chirp 5 hz bursts of white noise
            //wavFileName = "dp3_20080415-195000";             //ZERO SIGNAL silent room recording using dopod
            //wavFileName = "BAC2_20071010-042040_rain";       //contains rain and was giving spurious results with call template 2
            //wavFileName = "BAC2_20071018-143516_speech";
            //wavFileName = "BAC2_20071014-022040nightnoise";  //night with no signal in Kek-kek band.
            //wavFileName = "BAC2_20071008-195040";            //kek-kek track completely clear
            //wavFileName = "BAC2_20071015-045040";              //used by Birgit and Brad for Acoustic event detection
            //wavFileName = "BAC3_20070924-153657_wind";
            //wavFileName = "BAC3_20071002-070657";
            //wavFileName = "BAC3_20071001-203657";
            //wavFileName = "BAC5_20080520-040000_silence";
            //wavFileName = "Samford13Pre-Deploy_20081004-061500";
            //wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec

            //SAMFORD
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Samford02\";
            //string wavFileName = "SA0220080221-022657";
            //string wavFileName = "SA0220080222-015657";
            //string wavFileName = "SA0220080223-215657";

            //AUSTRALIAN BIRD CALLS
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\VoicesOfSubtropicalRainforests\";
            //string wavFileName = "06 Logrunner";

            //WEBSTER
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Websters\";
            //string wavFileName = "BOOBOOK";
            //string wavFileName = "CAPPRE";
            //string wavFileName = "KINGPAR";

            //JINHAI
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Jinhai\";
            //string wavFileName = "vanellus-miles";
            //string wavFileName = "En_spinebill";
            //string wavFileName = "kookaburra";
            //string wavFileName = "magpie";
            //string wavFileName = "raven";

            //KOALA recordings  - training files etc
            //wavDirName = @"C:\SensorNetworks\WavFiles\Koala\";
            //string wavFileName = "Jackaroo_20080715-103940";  //recording from Bill Ellis.
            //wavFileName = "Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000";

            //ST BEES
            wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
            //wavFileName = "WestKnoll_StBees_KoalaBellow20080919-073000"; //source file for template 6
            //wavFileName = "Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000";
            //wavFileName = "West_Knoll_St_Bees_WindRain_20080917-123000";
            //wavFileName = "West_Knoll_St_Bees_FarDistantKoala_20080919-000000";
            //wavFileName = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
            wavFileName = "West_Knoll_St_Bees_KoalaBellowFaint_20080919-010000";
            //wavFileName = "West_Knoll_St_Bees_FlyBirdCicada_20080917-170000";
            //wavFileName = "West_Knoll_St_Bees_Currawong1_20080923-120000";
            //wavFileName = "West_Knoll_St_Bees_Currawong2_20080921-053000";
            //wavFileName = "West_Knoll_St_Bees_Currawong3_20080919-060000"; //source file for template 3 and 8
            //wavFileName = "Top_Knoll_St_Bees_Curlew1_20080922-023000";
            //wavFileName = "Top_Knoll_St_Bees_Curlew2_20080922-030000";
            //wavFileName = "Honeymoon_Bay_St_Bees_Curlew3_20080914-003000";  //source file for template
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet1_20080918-080000";
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet2_20080916-160000";
            //wavFileName = "Honeymoon_Bay_St_Bees_20090312-060000_PheasantCoucal";

            //KOALA short training clips - i.e. < 1 sec long
            //wavDirName = @"C:\SensorNetworks\Templates\Template_KOALAMALE1\data\train\KOALA1_E\";
            //wavFileName = "koalaE_006";


            //JENNIFER'S CD
            //string wavDirName = @"C:\SensorNetworks\WavFiles\JenniferCD\";
            //string wavFileName = "Track02";           //Lewin's rail kek keks.

            //JENNIFER'S DATA
            //wavDirName = @"C:\SensorNetworks\WavFiles\Jennifer_BAC10\BAC10\";
            //wavFileName = "BAC10_20081101-045000";

            //BRIDGECREEK
            //wavDirName = @"C:\SensorNetworks\WavFiles\BridgeCreek\";
            //wavFileName = "cabin_earlyMorning";
            //wavFileName = "cabin_earlyMorning_StormBird_file0131";
            //wavFileName = "file0044_22kHz";
            //wavFileName = "file0026_22kHz16bit";
            //wavFileName = "butcherBird6";

            //BARAKULA
            //wavDirName = @"C:\SensorNetworks\WavFiles\Barakula\";
            //wavFileName = "20090508-071000.palm";
            //wavFileName = "20090508-071000.dopod";


            //GROUND PARROTS - SCOTT BURNETT
            //wavDirName = @"C:\SensorNetworks\Software\AudioAnalysis\Matlab\EPR\GroundParrot\";
            //wavFileName = "GParrots_JB2_20090607-173000.wav_minute_3";

            //MISCELLANEOUS
            //wavDirName = @"C:\SensorNetworks\Software\AudioAnalysis\Matlab\EPR\";
            //wavFileName = "BAC8_20080605-020000_selection"; //test file from BAC

            //--------------------------------------------------------------------------------------------------------------

            string wavPath = wavDirName + wavFileName + ".wav";
            recording = new AudioRecording(wavPath);

        } //end ChooseWavFile()



        public static void DownloadBytesFile(out string opDir, out string fileName, out AudioRecording recording)
        {
            opDir = @"C:\SensorNetworks\Output\";

            //string recordingName = "Samford+23/20090408-000000.mp3";
            //string recordingName = "BAC10/20081017-045000.mp3";
            //string recordingName = "BAC+JB3+-+Velma/20081116-042000.mp3";
            //string recordingName = "BAC8/20080612-040000.mp3";    //able to download
            //string recordingName = "BAC10/20081206-072000.mp3";
            string recordingName = "BAC8/20080605-020000.mp3";


            //process the recording name
            recordingName = recordingName.Replace('/', '_');
            recordingName = recordingName.Replace("+", "");
            Console.WriteLine("Get recording:- " + recordingName);
            fileName = Path.GetFileNameWithoutExtension(recordingName);

            //get bytes and write them to file and then read it.
            byte[] bytes = TowseyLib.RecordingFetcher.GetRecordingByFileName(recordingName);
            Console.WriteLine("Recording size=" + bytes.Length+ " bytes");
            string opPath = opDir + recordingName;
            File.WriteAllBytes(opPath, bytes);
            recording = new AudioRecording(opPath);
        }


    }//end class
}
