using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;



namespace AnalysisPrograms
{
    class FindEventsLikeThis
    {

        //Following lines are used for the debug command line.
        //CANETOAD
        //felt  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\FELT_CaneToad\FELT_CaneToad_Params.txt events.txt
        //GECKO
        //felt "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3"          C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt FELT_Gecko1
        //felt "C:\SensorNetworks\WavFiles\Gecko\Gecko05012010\DM420008_26m_00s__28m_00s - Gecko.mp3" C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt FELT_Gecko1
        //KOALA MALE EXHALE
        //felt "C:\SensorNetworks\WavFiles\Koala_Male\Recordings\KoalaMale\LargeTestSet\WestKnoll_Bees_20091103-190000.wav" C:\SensorNetworks\Output\FELT_KoalaMaleExhale\KoalaMaleExhale_Params.txt events.txt
        //felt "C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav" C:\SensorNetworks\Output\FELT_KoalaMaleExhale\KoalaMaleExhale_Params.txt events.txt
        //KOALA MALE FOREPLAY
        //felt "C:\SensorNetworks\WavFiles\Koala_Male\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav" C:\SensorNetworks\Output\FELT_KoalaMaleForeplay_LargeTestSet\KoalaMaleForeplay_Params.txt events.txt
        //BRIDGE CREEK
        //felt "C:\SensorNetworks\WavFiles\Length1_2_4_8_16mins\BridgeCreek_1min.wav" C:\SensorNetworks\Output\TestWavDuration\DurationTest_Params.txt events.txt
        //CURLEW
        //felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\Top_Knoll_-_St_Bees_20090517-210000.wav" C:\SensorNetworks\Output\FELT_CURLEW\FELT_CURLEW_Params.txt FELT_Curlew1_Curated2_symbol.txt
        // @"C:\SensorNetworks\WavFiles\Curlew\Curlew_JasonTagged\West_Knoll_Bees_20091102-000000.mp3"
        //CURRAWONG
        //felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-003000.wav" C:\SensorNetworks\Output\FELT_Currawong\FELT_Currawong_Params.txt FELT_Currawong2_curatedBinary.txt


        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_CALL_NAME       = "CALL_NAME";
        public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_MIN_HZ          = "MIN_HZ";
        public static string key_MAX_HZ          = "MAX_HZ";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW   = "SMOOTH_WINDOW";
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_DYNAMIC_RANGE   = "DYNAMIC_RANGE";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";

        public static string eventsFile = "events.txt";

        /// <summary>
        /// reads matrix of chars into a trinary symbolisation of a call.
        /// </summary>
        /// <param name="symbolPath"></param>
        /// <returns></returns>
        public static double[,] ReadChars2TrinaryMatrix(string symbolPath)
        {
            List<string> lines = FileTools.ReadTextFile(symbolPath);
            int rows = lines.Count;
            int cols = lines[0].Length;
            var m = new double[rows,cols];
            for (int r = 0; r < rows; r++)
            {
                string line = lines[r];
                for (int c = 0; c < cols; c++)
                    if(line[c] == '+') m[r,c] = 1.0;
                    else if (line[c] == '-') m[r, c] = -1.0;
                    else if (line[c] == '0') m[r, c] = 0.0;
                    else
                    {
                        m[r, c] = 0.0;
                        Log.WriteLine("### WARNING WARNING WARNING! Non-standard CHAR in file: {0}", Path.GetFileName(symbolPath));
                        Log.WriteLine("###     Non-standard CHAR = {0} at position {1},{2}", line[c], r, c);
                    }
            }
            return m;
        }



        public static void DrawSonogram(BaseSonogram sonogram, string path, List<AcousticEvent> predictedEvents, double threshold, double[] scores)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                if (scores != null)
                {
                    double normMax = threshold * 4; //so normalised eventThreshold = 0.25
                    for (int i = 0; i < scores.Length; i++)
                    {
                        scores[i] /= normMax;
                        if (scores[i] > 1.0) scores[i] = 1.0;
                        if (scores[i] < 0.0) scores[i] = 0.0;
                    }

                    image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, 0.25));
                }
                image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }

    }//end class
}
