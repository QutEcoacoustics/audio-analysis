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

            SpectralSonogram sonogram = null;
            var allEvents     = new List<AcousticEvent>();
            var scoresList    = new List<double[]>(); 
            var thresholdList = new List<double>();

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);

            //ii: Get zip paths
            List<string> zipList = FileTools.ReadTextFile(iniPath);
            System.Tuple<SpectralSonogram, List<AcousticEvent>, double[], double> results = null; //set up the results Tuple
            foreach (string zipPath in zipList)
            {
                if(zipPath.StartsWith("#")) continue; // commented line

                //i get params file
                FileTools.UnZip(outputDir, zipPath, true);
                string zipName    = Path.GetFileNameWithoutExtension(zipPath);
                string[] parts    = zipName.Split('_');
                string paramsPath = Path.Combine(outputDir, parts[0] + "_" + parts[1] + "_Params.txt");
                
                //ii: READ PARAMETER VALUES FROM INI FILE
                var config = new Configuration(paramsPath);
                Dictionary<string, string> dict = config.GetTable();
                //Dictionary<string, string>.KeyCollection keys = dict.Keys;
                //int DRAW_SONOGRAMS = Int32.Parse(dict[FeltTemplate_Create.key_DRAW_SONOGRAMS]);          //options to draw sonogram


                if (zipName.EndsWith("binaryTemplate"))
                {
                    string templatePath = Path.Combine(outputDir, parts[0] + "_" + parts[1] + "_binary.bmp");
                    double[,] templateMatrix = FindMatchingEvents.ReadImage2BinaryMatrixDouble(templatePath);
                    results = FELTWithBinaryTemplate(recording, dict, templateMatrix);
                }
                else
                if (zipName.EndsWith("trinaryTemplate"))
                {
                    string templatePath = Path.Combine(outputDir, parts[0] + "_" + parts[1] + "_trinary.bmp");
                    double[,] templateMatrix = FindMatchingEvents.ReadImage2TrinaryMatrix(templatePath);
                    results = FELTWithBinaryTemplate(recording, dict, templateMatrix);
                }
                else
                if (zipName.EndsWith("syntacticTemplate"))
                {
                    //string templatePath = Path.Combine(outputDir, parts[0]  + "_" + parts[1] + "_spr.txt");
                    //results = ScanWithBinaryTemplate(recording, dict, templatePath);
                    Log.WriteLine("TO DO! YET TO IMPLEMENT syntacticTemplate for:" + zipName);
                    continue;
                }
                else
                {
                    Log.WriteLine("ERROR! UNKNOWN TEMPLATE: Zip file has unrecognised suffix:" + zipName);        
                    continue;
                }
                
                sonogram              = results.Item1;
                var matchingEvents    = results.Item2;
                allEvents.AddRange(matchingEvents);
                var scores            = results.Item3;
                scoresList.Add(scores);
                double matchThreshold = results.Item4;
                thresholdList.Add(matchThreshold);
                Log.WriteLine("# Finished detecting events like target: " + parts[0] + "_" + parts[1]);
                int count = matchingEvents.Count; // 
                Log.WriteLine("# Matching Event Count = " + matchingEvents.Count());
                Log.WriteLine("           @ threshold = {0:f3}", matchThreshold);

                //v: write events count to results info file. 
                double sigDuration = sonogram.Duration.TotalSeconds;
                string fname = Path.GetFileName(recordingPath);
                string str = String.Format("{0}\t{1}\t{2}", fname, sigDuration, count);
                StringBuilder sb = AcousticEvent.WriteEvents(matchingEvents, str);
                FileTools.WriteTextFile("opPath", sb.ToString());


            } // foreach (string zipPath in zipList)


            //draw images of sonograms
            int DRAW_SONOGRAMS = 2;
            string opImagePath = outputDir + "\\" + Path.GetFileNameWithoutExtension(recordingPath) + "_matchingEvents.png";
            if (DRAW_SONOGRAMS == 2)
            {
                DrawSonogram(sonogram, opImagePath, allEvents, thresholdList, scoresList);
            }
            else
                if ((DRAW_SONOGRAMS == 1) && (allEvents.Count > 0))
                {
                    DrawSonogram(sonogram, opImagePath, allEvents, thresholdList, scoresList);
                }



            Log.WriteLine("# Finished passing all templates over recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()



        /// <summary>
        /// Scans a recording given a dicitonary of parameters and a binary template
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="dict"></param>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        public static System.Tuple<SpectralSonogram, List<AcousticEvent>, double[], double> FELTWithBinaryTemplate(AudioRecording recording, Dictionary<string, string> dict, double[,] templateMatrix)
        {
            //i: get parameters from dicitonary
            //string callName = dict[FeltTemplate_Create.key_CALL_NAME];
            double frameOverlap = Double.Parse(dict[FeltTemplate_Create.key_FRAME_OVERLAP]);
            bool doSegmentation = Boolean.Parse(dict[FeltTemplate_Create.key_DO_SEGMENTATION]);
            double smoothWindow = Double.Parse(dict[FeltTemplate_Create.key_SMOOTH_WINDOW]);          //before segmentation 
            int minHz = Int32.Parse(dict[FeltTemplate_Create.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[FeltTemplate_Create.key_MAX_HZ]);
            double minDuration = Double.Parse(dict[FeltTemplate_Create.key_MIN_DURATION]);           //min duration of event in seconds 
            double templateThreshold = Double.Parse(dict[FeltTemplate_Create.key_TEMPLATE_THRESHOLD]);     //min score for an acceptable event

            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("Min Duration: " + minDuration + " seconds");

            //ii: CHECK RECORDING
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //iii: MAKE SONOGRAM
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.DoMelScale = false;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            SpectralSonogram sonogram = new SpectralSonogram(basegram);  //spectrogram has dim[N,257]
            //recording.Dispose(); //DO NOT DISPOSE BECAUSE REQUIRE AGAIN

            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap * 100);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            double templateDuration = templateMatrix.GetLength(1) / sonogram.FramesPerSecond;
            Log.WriteIfVerbose("Template duration = {0:f3} seconds or {1} frames.", templateDuration, templateMatrix.GetLength(1));

            //iii: DO SEGMENTATION
            double maxDuration = Double.MaxValue;  //Do not constrain maximum length of events.
            var tuple1 = AcousticEvent.GetSegmentationEvents((SpectralSonogram)sonogram, doSegmentation, minHz, maxHz, smoothWindow, templateThreshold, minDuration, maxDuration);
            var segmentEvents = tuple1.Item1;

            //iv: Score sonogram for events matching template
            //#############################################################################################################################################
            var tuple2 = FindMatchingEvents.Execute_Bi_or_TrinaryMatch(templateMatrix, sonogram, segmentEvents, minHz, maxHz, minDuration);
            //var tuple2 = FindMatchingEvents.Execute_StewartGage(target, dynamicRange, (SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, minDuration);
            //var tuple2 = FindMatchingEvents.Execute_SobelEdges(target, dynamicRange, (SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, minDuration);
            //var tuple2 = FindMatchingEvents.Execute_MFCC_XCOR(target, dynamicRange, sonogram, segmentEvents, minHz, maxHz, minDuration);
            var scores = tuple2.Item1;
            //#############################################################################################################################################

            //v: PROCESS SCORE ARRAY
            //scores = DataTools.filterMovingAverage(scores, 3);
            double eventThreshold = 4.0; // dB threshold
            Console.WriteLine("Scores: min={0:f4}, max={1:f4}, threshold={2:f2}dB", scores.Min(), scores.Max(), eventThreshold);
            //Set (scores < 0.0) = 0.0;
            for (int i = 0; i < scores.Length; i++) if (scores[i] < 0.0) scores[i] = 0.0;

            //vi: EXTRACT EVENTS
            List<AcousticEvent> matchEvents = AcousticEvent.ConvertScoreArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, eventThreshold,
                                                                            minDuration, maxDuration, sonogram.Configuration.SourceFName, sonogram.Configuration.CallName);
            
            // correct the start time, duration and end of events to match the max score and length of the template.
            // Also correct the scores because we want the max match for a template
            foreach (AcousticEvent ae in matchEvents)
            {
                //ae.StartTime       = ae.Score_TimeOfMaxInEvent;
                ae.Duration        = templateDuration;
                ae.EndTime         = ae.StartTime + templateDuration;
                if (ae.EndTime > sonogram.Duration.TotalSeconds) ae.EndTime = sonogram.Duration.TotalSeconds; //check for overflow.
                ae.oblong          = AcousticEvent.ConvertEvent2Oblong(ae);
                ae.Score           = ae.Score_MaxInEvent;
                ae.ScoreNormalised = ae.Score / ae.Score_MaxPossible;  // normalised to the user supplied threshold
            }

            return System.Tuple.Create(sonogram, matchEvents, scores, eventThreshold);
        } // FELTWithBinaryTemplate()



        public static void DrawSonogram(BaseSonogram sonogram, string path, List<AcousticEvent> predictedEvents, List<double> thresholdList, List<double[]> scoresList)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            Log.WriteLine("# Convert score arrays to correct length for display.");
            scoresList = ConvertScoreArrayLengths(scoresList, sonogram.FrameCount);

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                
                // Add in score tracks
                for (int s = 0; s < scoresList.Count; s++)
                {
                    double[] scores = scoresList[s];
                    if (scores != null)
                    {
                        double normMax = thresholdList[s] * 4; //so normalised eventThreshold = 0.25
                        for (int i = 0; i < scores.Length; i++)
                        {
                            scores[i] /= normMax;
                            if (scores[i] > 1.0) scores[i] = 1.0;
                            if (scores[i] < 0.0) scores[i] = 0.0;
                        }

                        image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, 0.25));
                    }
                } //end adding in score tracks

                image.AddEvents(predictedEvents);
                image.Save(path);
            } // using
        } // DrawSonogram()

        public static List<double[]> ConvertScoreArrayLengths(List<double[]> scoresList, int frameCount)
        {
            var newList = new List<double[]>();
            for (int s = 0; s < scoresList.Count; s++)
            {
                double[] scores    = scoresList[s];
                if ((scores == null) || (scores.Length == frameCount))
                {
                    newList.Add(scores);
                    continue;
                } else
                if(scores.Length == frameCount * 2)
                {
                    double[] newScores = new double[frameCount];
                    for (int i = 0; i < frameCount; i++) newScores[i] = scores[i*2]; // take every second sample
                    newList.Add(newScores);
                    continue;
                } else
                if (frameCount == scores.Length * 2)
                {
                    double[] newScores = new double[frameCount];
                    for (int i = 0; i < frameCount; i++) newScores[i] = scores[i / 2]; // take every sample twice
                    newList.Add(newScores);
                    continue;
                }
                else Console.WriteLine("WARNING: WARNING: Score array not of correct length for display.");
                
            }
            return newList;
        } // ConvertScoreArrayLengths()

    }//end class
}
