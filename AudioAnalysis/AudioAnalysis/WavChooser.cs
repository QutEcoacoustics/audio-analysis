using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
    static class WavChooser
    {


        public static void ChooseWavFile(out string wavDirName, out string wavFileName)
        {
            //BRISBANE AIRPORT CORP
            //wavDirName = @"C:\SensorNetworks\WavFiles\";
            //wavFileName = "sineSignal";
            //wavFileName = "golden-whistler";
            //wavFileName = "BAC2_20071008-085040";           //Lewin's rail kek keks //source file for template.
            //string wavFileName = "BAC1_20071008-084607";             //faint kek-kek call
            //string wavFileName = "BAC2_20071011-182040_cicada";    //repeated cicada chirp 5 hz bursts of white noise
            //string wavFileName = "dp3_20080415-195000";            //ZERO SIGNAL silent room recording using dopod
            //string wavFileName = "BAC2_20071010-042040_rain";      //contains rain and was giving spurious results with call template 2
            //string wavFileName = "BAC2_20071018-143516_speech";
            //string wavFileName = "BAC2_20071014-022040nightnoise"; //night with no signal in Kek-kek band.
            //string wavFileName = "BAC2_20071008-195040";           //kek-kek track completely clear
            //string wavFileName = "BAC3_20070924-153657_wind";
            //string wavFileName = "BAC3_20071002-070657";
            //string wavFileName = "BAC3_20071001-203657";
            //string wavFileName = "BAC5_20080520-040000_silence";
            //string wavFileName = "Samford13Pre-Deploy_20081004-061500";
            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec

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
            //const string wavDirName = @"C:\SensorNetworks\Koala\";
            //const string opDirName  = @"C:\SensorNetworks\Koala\";
            //string wavFileName = "Jackaroo_20080715-103940";  //recording from Bill Ellis.

            //ST BEES
            wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
            //wavFileName = "West_Knoll_St_Bees_KoalaBellow20080919-073000"; //source file for template
            wavFileName = "Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000";
            //wavFileName = "West_Knoll_St_Bees_WindRain_20080917-123000";
            //wavFileName = "West_Knoll_St_Bees_FarDistantKoala_20080919-000000";
            //wavFileName = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
            //wavFileName = "West_Knoll_St_Bees_KoalaBellowFaint_20080919-010000";
            //wavFileName = "West_Knoll_St_Bees_FlyBirdCicada_20080917-170000";
            //wavFileName = "West_Knoll_St_Bees_Currawong1_20080923-120000";
            //wavFileName = "West_Knoll_St_Bees_Currawong2_20080921-053000";
            //wavFileName = "West_Knoll_St_Bees_Currawong3_20080919-060000";
            //wavFileName = "Top_Knoll_St_Bees_Curlew1_20080922-023000";
            //wavFileName = "Top_Knoll_St_Bees_Curlew2_20080922-030000";
            //wavFileName = "Honeymoon_Bay_St_Bees_Curlew3_20080914-003000";  //source file for template
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet1_20080918-080000";
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet2_20080916-160000";

            //JENNIFER'S CD
            //string wavDirName = @"C:\SensorNetworks\WavFiles\JenniferCD\";
            //string wavFileName = "Track02";           //Lewin's rail kek keks.

            //JENNIFER'S DATA
            //wavDirName = @"C:\SensorNetworks\WavFiles\Jennifer_BAC10\BAC10\";
            //wavFileName = "BAC10_20081101-045000";
        } //end ChooseWavFile()

    }
}
