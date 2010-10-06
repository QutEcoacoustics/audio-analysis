using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;



namespace AnalysisPrograms
{


    /// <summary>
    /// This application scans a recording with a number of templates and returns the scores for each template
    /// There are three command line arguments:
    /// arg[0] = the recording to be scanned
    /// arg[1] = the path to a file containing the paths to template locations, one template per line 
    /// arg[2] = the output directory 
    /// </summary>
    class FeltTemplates_Use
    {

        //felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-003000.wav" C:\SensorNetworks\Output\FELT_MultiOutput\templateList.txt  C:\SensorNetworks\Output\FELT_MultiOutput


        //Following lines are used for SINGLE TEMPLATE command lines.
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


        public static void Dev(string[] args)
        {
            string title = "# FIND OTHER ACOUSTIC EVENTS LIKE THIS";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Log.WriteIfVerbose("# Recording     =" + args[0]);
            Log.WriteIfVerbose("# Template list =" + args[1]);
            Log.WriteIfVerbose("# Output folder =" + args[2]);

            Segment.CheckArguments(args);

            string recordingPath = args[0];    //the recording to be scanned
            string iniPath       = args[1];    //the path to a file containing the paths to template locations, one template per line
            string outputDir     = args[2];    //name of output dir 

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);

            //ii: Get zip paths
            List<string> zipList = FileTools.ReadTextFile(iniPath);
            System.Tuple<SpectralSonogram, List<AcousticEvent>, double[], double> results = null;
            foreach (string zipPath in zipList)
            {
                //i get params file
                FileTools.UnZip(outputDir, zipPath, true);
                string zipName    = Path.GetFileNameWithoutExtension(zipPath);
                string[] parts    = zipName.Split('_');
                string paramsPath = Path.Combine(outputDir, parts[0] + "_ParamsFile.txt");
                
                //ii: READ PARAMETER VALUES FROM INI FILE
                var config = new Configuration(paramsPath);
                Dictionary<string, string> dict = config.GetTable();
                //Dictionary<string, string>.KeyCollection keys = dict.Keys;

                if(zipName.EndsWith("binaryTemplate"))
                {
                    string templatePath = Path.Combine(outputDir, parts[0] + "_binary.bmp");
                    results = ScanWithBinaryTemplate(recording, dict, templatePath);
                }
                else
                if(zipName.EndsWith("trinaryTemplate"))
                {
                    string templatePath = Path.Combine(outputDir, parts[0] + "_trinary.bmp");
                    results = ScanWithBinaryTemplate(recording, dict, templatePath);
                }
                else
                if(zipName.EndsWith("syntacticTemplate"))
                {
                    string templatePath = Path.Combine(outputDir, parts[0] + "_spr.txt");
                    results = ScanWithBinaryTemplate(recording, dict, templatePath);
                }


                var sonogram          = results.Item1;
                var matchingEvents    = results.Item2;
                var scores            = results.Item3;
                double matchThreshold = results.Item4;
                Log.WriteLine("# Finished detecting events like the target.");
                int count = matchingEvents.Count;
                Log.WriteLine("# Matching Event Count = " + matchingEvents.Count());
                Log.WriteLine("           @ threshold = {0:f3}", matchThreshold);

                //v: write events count to results info file. 
                double sigDuration = sonogram.Duration.TotalSeconds;
                string fname = Path.GetFileName(recordingPath);
                string str = String.Format("{0}\t{1}\t{2}", fname, sigDuration, count);
                StringBuilder sb = AcousticEvent.WriteEvents(matchingEvents, str);
                FileTools.WriteTextFile("opPath", sb.ToString());



                //draw images of sonograms
                int DRAW_SONOGRAMS = Int32.Parse(dict[FeltTemplate_Create.key_DRAW_SONOGRAMS]);          //options to draw sonogram
                string opImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_matchingEvents.png";
                if (DRAW_SONOGRAMS == 2)
                {
                    DrawSonogram(sonogram, opImagePath, matchingEvents, matchThreshold, scores);
                }
                else
                if ((DRAW_SONOGRAMS == 1) && (matchingEvents.Count > 0))
                {
                    DrawSonogram(sonogram, opImagePath, matchingEvents, matchThreshold, scores);
                }

            } //foreach (string zipPath in zipList)

            Log.WriteLine("# Finished passing templates over recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        /// <summary>
        /// Scans a recording given a dicitonary of parameters and a binary template
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="dict"></param>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        public static System.Tuple<SpectralSonogram, List<AcousticEvent>, double[], double> ScanWithBinaryTemplate(AudioRecording recording, Dictionary<string, string> dict, string templatePath)
        {
        
            //string callName = dict[FeltTemplate_Create.key_CALL_NAME];
            double frameOverlap   = Double.Parse(dict[FeltTemplate_Create.key_FRAME_OVERLAP]);
            bool doSegmentation   = Boolean.Parse(dict[FeltTemplate_Create.key_DO_SEGMENTATION]);
            double smoothWindow   = Double.Parse(dict[FeltTemplate_Create.key_SMOOTH_WINDOW]);          //before segmentation 
            int minHz             = Int32.Parse(dict[FeltTemplate_Create.key_MIN_HZ]);
            int maxHz             = Int32.Parse(dict[FeltTemplate_Create.key_MAX_HZ]);
            double minDuration    = Double.Parse(dict[FeltTemplate_Create.key_MIN_DURATION]);           //min duration of event in seconds 
            double eventThreshold = Double.Parse(dict[FeltTemplate_Create.key_TEMPLATE_THRESHOLD]);     //min score for an acceptable event

            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("Min Duration: " + minDuration + " seconds");

            //iii: GET THE TARGET
            double[,] templateMatrix = FeltTemplate_Edit.ReadImage2BinaryMatrixDouble(templatePath);
            //double[,] templateMatrix = ReadChars2TrinaryMatrix(trinaryTemplatePath);


            //iv: Find matching events
            //#############################################################################################################################################
            var results = FindMatchingEvents.ExecuteFELT(templateMatrix, recording, doSegmentation, minHz, maxHz, frameOverlap, smoothWindow, eventThreshold, minDuration);
            //#############################################################################################################################################
            return results;
        } // ScanWithBinaryTemplate()


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
        } // DrawSonogram()

    }//end class
}
