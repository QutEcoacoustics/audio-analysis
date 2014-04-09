using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLibrary;
using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.DSP;




namespace AnalysisPrograms
{
    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    /// <summary>
    /// This application scans a recording with a number of templates and returns the scores for each template
    /// There are three command line arguments:
    /// arg[0] = the recording to be scanned
    /// arg[1] = the path to a file containing the paths to template locations, one template per line 
    /// arg[2] = the output directory 
    /// </summary>
    public class FeltTemplates_Use
    {
        public class Arguments : SourceConfigOutputDirArguments
        {
        }
        // IMPORTANT NOTE: FOLLOWING FRAMING PARAMETERS ARE FIXED AS CONSTANTS FOR FELT
        //                 This is to speed COMPUTATION. OTHERWISE must COMPUTE NEW SPECTROGRAM FOR EVERY TEMPLATE.
        //                 This is a compromise because detection of koalas using oscilations works better at overlap=0.75.
        public const int    FeltSampleRate     = 22050;
        public const int    FeltWindow         = 512;
        public const double FeltFrameOverlap   = 0.5;
        public const int    FeltNyquist        = 11025;
        public const double FeltFrameDuration  = 0.02321995464852;
        public const double FeltFrameOffset    = 0.01160997732426;
        public const double FeltFramePerSecond = 86.1328125;
        public const int    FeltFreqBinCount   = 256;
        public const double FeltFreqBinWidth   = 43.06640625;



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
        //CURRAWONG
        //felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-003000.wav" C:\SensorNetworks\Output\FELT_Currawong\FELT_Currawong_Params.txt FELT_Currawong2_curatedBinary.txt

        //MULTIPLE PASSES USING ZIPPED TEMPLATES
        // felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-003000.wav"  C:\SensorNetworks\Output\FELT_MultiOutput\templateList.txt  C:\SensorNetworks\Output\FELT_MultiOutput
        // felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20080929-210000.wav"               C:\SensorNetworks\Output\FELT_MultiOutput\templateList.txt  C:\SensorNetworks\Output\FELT_MultiOutput

        // felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-050000.wav"  C:\SensorNetworks\Output\FELT_MultiOutput_5templates\templateList.txt  C:\SensorNetworks\Output\FELT_MultiOutput_5templates

        public static Arguments Dev()
        {
            throw new NotImplementedException();
            //return new Arguments();
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            string title = "# FIND OTHER ACOUSTIC EVENTS LIKE THIS";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Log.WriteIfVerbose("# Recording     =" + arguments.Source);//the recording to be scanned
            Log.WriteIfVerbose("# Template list =" + arguments.Config);//the path to a file containing the paths to template locations, one template per line
            Log.WriteIfVerbose("# Output folder =" + arguments.Output);//name of output dir 

            var allEvents     = new List<AcousticEvent>();
            var scoresList    = new List<double[]>(); 
            var thresholdList = new List<double>();

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(arguments.Source.FullName);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: MAKE SONOGRAM
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowOverlap = FeltFrameOverlap;      // set default value
            sonoConfig.DoMelScale = false;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            SpectrogramStandard sonogram = new SpectrogramStandard(basegram);  //spectrogram has dim[N,257]
            recording.Dispose(); //DO NOT DISPOSE BECAUSE REQUIRE AGAIN

            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, FeltFrameOverlap * 100);


            //iii: Get zip paths and the results Tuple
            List<string> zipList = FileTools.ReadTextFile(arguments.Config.FullName);
            System.Tuple<SpectrogramStandard, List<AcousticEvent>, double[]> results = null; //set up the results Tuple

            foreach (string zipPath in zipList)
            {
                if (zipPath.StartsWith("#")) continue;  // commented line
                if (zipPath.Length < 2)      continue;  // empty line

                //i: get params file
                FileTools.UnZip(arguments.Output.FullName, zipPath, true);
                string zipName    = Path.GetFileNameWithoutExtension(zipPath);
                string[] parts    = zipName.Split('_');
                string paramsPath = Path.Combine(arguments.Output.FullName, parts[0] + "_" + parts[1] + "_Params.txt");

                string id = parts[0] + "_" + parts[1];
                Log.WriteIfVerbose("################################################### "+id+" ########################################################");
                
                //ii: READ PARAMETER VALUES FROM INI FILE
                var config = new ConfigDictionary(paramsPath);
                Dictionary<string, string> dict = config.GetTable();
                //Dictionary<string, string>.KeyCollection keys = dict.Keys;
                //int DRAW_SONOGRAMS = Int32.Parse(dict[FeltTemplate_Create.key_DRAW_SONOGRAMS]);          //options to draw sonogram
                dict[FeltTemplate_Create.key_DECIBEL_THRESHOLD] = "4.0";
                dict[FeltTemplate_Create.key_MIN_DURATION]      = "0.02";

                if (zipName.EndsWith("binaryTemplate"))
                {
                    string templatePath = Path.Combine(arguments.Output.FullName, id + "_binary.bmp");
                    double[,] templateMatrix = FindMatchingEvents.ReadImage2BinaryMatrixDouble(templatePath);
                    results = FELTWithBinaryTemplate(sonogram, dict, templateMatrix);
                }
                else
                if (zipName.EndsWith("trinaryTemplate"))
                {
                    string templatePath = Path.Combine(arguments.Output.FullName, id + "_trinary.bmp");
                    double[,] templateMatrix = FindMatchingEvents.ReadImage2TrinaryMatrix(templatePath);
                    results = FELTWithBinaryTemplate(sonogram, dict, templateMatrix);
                }
                else
                if (zipName.EndsWith("syntacticTemplate"))
                {
                    string templatePath = Path.Combine(arguments.Output.FullName, id + "_spr.txt");
                    char[,] templateMatrix = FindMatchingEvents.ReadTextFile2CharMatrix(templatePath);
                    results = FELTWithSprTemplate(sonogram, dict, templateMatrix);  
                }
                else
                {
                    Log.WriteLine("ERROR! UNKNOWN TEMPLATE: Zip file has unrecognised suffix:" + zipName);        
                    continue;
                }

                //get results
                sonogram = results.Item1;
                var matchingEvents = results.Item2;
                var scores = results.Item3;

                double matchThreshold = Double.Parse(dict[FeltTemplate_Create.key_DECIBEL_THRESHOLD]);
                Log.WriteLine("# Finished detecting events like target: " + id);
                Log.WriteLine("# Matching Event Count = " + matchingEvents.Count);
                Log.WriteLine("           @ threshold = {0:f2}", matchThreshold);


                // accumulate results
                allEvents.AddRange(matchingEvents);
                scoresList.Add(scores);
                thresholdList.Add(matchThreshold);

                //v: write events count to results info file. 
                double sigDuration = sonogram.Duration.TotalSeconds;
                string fname = arguments.Source.Name;
                string str = String.Format("{0}\t{1}\t{2}", fname, sigDuration, matchingEvents.Count);
                StringBuilder sb = AcousticEvent.WriteEvents(matchingEvents, str);
                FileTools.WriteTextFile("opPath", sb.ToString());

            } // foreach (string zipPath in zipList)

            Log.WriteLine("\n\n\n##########################################################");
            Log.WriteLine("# Finished detecting events");
            Log.WriteLine("# Event Count = " + allEvents.Count);
            foreach (AcousticEvent ae in allEvents)
            {
                Log.WriteLine("# Event name = {0}  ############################", ae.Name);
                Log.WriteLine("# Event time = {0:f2} to {1:f2} (frames {2}-{3}).", ae.TimeStart, ae.TimeEnd, ae.oblong.r1, ae.oblong.r2);
                Log.WriteLine("# Event score= {0:f2}.", ae.Score);
            }

            int percentOverlap = 50;
            allEvents = PruneOverlappingEvents(allEvents, percentOverlap);
            Log.WriteLine("\n##########################################################");
            Log.WriteLine("# Finished pruning events");
            Log.WriteLine("# Event Count = " + allEvents.Count);
            WriteEventNames(allEvents);


            //WriteScoreAverages2Console(scoresList);

            //draw images of sonograms
            int DRAW_SONOGRAMS = 2;
            FileInfo opImagePath =  arguments.Output.CombineFile(Path.GetFileNameWithoutExtension(arguments.Source.Name) + "_matchingEvents.png");
            if (DRAW_SONOGRAMS == 2)
            {
                DrawSonogram(sonogram, opImagePath, allEvents, thresholdList, scoresList);
            }
            else
            if ((DRAW_SONOGRAMS == 1) && (allEvents.Count > 0))
            {
                DrawSonogram(sonogram, opImagePath, allEvents, thresholdList, scoresList);
            }

            Log.WriteLine("# FINISHED passing all templates over recording:- " + arguments.Source.Name);
        }



        /// <summary>
        /// Scans a recording given a dicitonary of parameters and a binary template
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="dict"></param>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        public static System.Tuple<SpectrogramStandard, List<AcousticEvent>, double[]> FELTWithBinaryTemplate(SpectrogramStandard sonogram, Dictionary<string, string> dict, double[,] templateMatrix)
        {
            //i: get parameters from dicitonary
            string callName = dict[FeltTemplate_Create.key_CALL_NAME];
            bool doSegmentation = Boolean.Parse(dict[FeltTemplate_Create.key_DO_SEGMENTATION]);
            double smoothWindow = Double.Parse(dict[FeltTemplate_Create.key_SMOOTH_WINDOW]);          //before segmentation 
            int minHz = Int32.Parse(dict[FeltTemplate_Create.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[FeltTemplate_Create.key_MAX_HZ]);
            double minDuration = Double.Parse(dict[FeltTemplate_Create.key_MIN_DURATION]);         //min duration of event in seconds 
            double dBThreshold = Double.Parse(dict[FeltTemplate_Create.key_DECIBEL_THRESHOLD]);   // = 9.0; // dB threshold
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteLine("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            //ii: TEMPLATE INFO
            double templateDuration = templateMatrix.GetLength(1) / sonogram.FramesPerSecond;
            Log.WriteIfVerbose("Template duration = {0:f3} seconds or {1} frames.", templateDuration, templateMatrix.GetLength(1));
            Log.WriteIfVerbose("Min Duration: " + minDuration + " seconds");

            //iii: DO SEGMENTATION
            double segmentationThreshold = 2.0;     // Standard deviations above backgorund noise
            double maxDuration = Double.MaxValue;   // Do not constrain maximum length of events.
            var tuple1 = AcousticEvent.GetSegmentationEvents((SpectrogramStandard)sonogram, doSegmentation, minHz, maxHz, smoothWindow, segmentationThreshold, minDuration, maxDuration);
            var segmentEvents = tuple1.Item1;

            //iv: Score sonogram for events matching template
            //#############################################################################################################################################
            var tuple2 = FindMatchingEvents.Execute_Bi_or_TrinaryMatch(templateMatrix, sonogram, segmentEvents, minHz, maxHz, dBThreshold);
            var scores = tuple2.Item1;
            //#############################################################################################################################################

            // v: PROCESS SCORE ARRAY
            // scores = DataTools.filterMovingAverage(scores, 3);
            LoggedConsole.WriteLine("Scores: min={0:f4}, max={1:f4}, User threshold={2:f2}dB", scores.Min(), scores.Max(), dBThreshold);
            for (int i = 0; i < scores.Length; i++) if (scores[i] < 0.0) scores[i] = 0.0;  // Set (scores < 0.0) = 0.0;

            //vi: EXTRACT EVENTS
            List<AcousticEvent> matchEvents = AcousticEvent.ConvertScoreArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, dBThreshold,
                                                                            minDuration, maxDuration);
            foreach (AcousticEvent ev in matchEvents)
            {
                ev.SourceFileName = sonogram.Configuration.SourceFName;
                ev.Name = sonogram.Configuration.CallName;
            }


            // Edit the events to correct the start time, duration and end of events to match the max score and length of the template.
            AdjustEventLocation(matchEvents, callName, templateDuration, sonogram.Duration.TotalSeconds);

            return System.Tuple.Create(sonogram, matchEvents, scores);
        } // FELTWithBinaryTemplate()


        /// <summary>
        /// Scans a recording given a dicitonary of parameters and a syntactic template
        /// Template has a different orientation to others.
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="dict"></param>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        public static System.Tuple<SpectrogramStandard, List<AcousticEvent>, double[]> FELTWithSprTemplate(SpectrogramStandard sonogram, Dictionary<string, string> dict, char[,] templateMatrix)
        {
            //i: get parameters from dicitonary
            string callName = dict[FeltTemplate_Create.key_CALL_NAME];
            bool doSegmentation = Boolean.Parse(dict[FeltTemplate_Create.key_DO_SEGMENTATION]);
            double smoothWindow = Double.Parse(dict[FeltTemplate_Create.key_SMOOTH_WINDOW]);         //before segmentation 
            int minHz = Int32.Parse(dict[FeltTemplate_Create.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[FeltTemplate_Create.key_MAX_HZ]);
            double minDuration = Double.Parse(dict[FeltTemplate_Create.key_MIN_DURATION]);           //min duration of event in seconds 
            double dBThreshold = Double.Parse(dict[FeltTemplate_Create.key_DECIBEL_THRESHOLD]);      // = 9.0; // dB threshold
            dBThreshold = 4.0;
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteLine("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            //ii: TEMPLATE INFO
            double templateDuration = templateMatrix.GetLength(0) / sonogram.FramesPerSecond;
            Log.WriteIfVerbose("Template duration = {0:f3} seconds or {1} frames.", templateDuration, templateMatrix.GetLength(0));
            Log.WriteIfVerbose("Min Duration: " + minDuration + " seconds");

            //iii: DO SEGMENTATION
            double segmentationThreshold = 2.0;      // Standard deviations above backgorund noise
            double maxDuration = Double.MaxValue;    // Do not constrain maximum length of events.
            var tuple1 = AcousticEvent.GetSegmentationEvents((SpectrogramStandard)sonogram, doSegmentation, minHz, maxHz, smoothWindow, segmentationThreshold, minDuration, maxDuration);
            var segmentEvents = tuple1.Item1;

            //iv: Score sonogram for events matching template
            //#############################################################################################################################################
            var tuple2 = FindMatchingEvents.Execute_Spr_Match(templateMatrix, sonogram, segmentEvents, minHz, maxHz, dBThreshold);
            //var tuple2 = FindMatchingEvents.Execute_StewartGage(target, dynamicRange, (SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, minDuration);
            //var tuple2 = FindMatchingEvents.Execute_SobelEdges(target, dynamicRange, (SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, minDuration);
            //var tuple2 = FindMatchingEvents.Execute_MFCC_XCOR(target, dynamicRange, sonogram, segmentEvents, minHz, maxHz, minDuration);
            var scores = tuple2.Item1;
            //#############################################################################################################################################

            //v: PROCESS SCORE ARRAY
            //scores = DataTools.filterMovingAverage(scores, 3);
            LoggedConsole.WriteLine("Scores: min={0:f4}, max={1:f4}, threshold={2:f2}dB", scores.Min(), scores.Max(), dBThreshold);
            //Set (scores < 0.0) = 0.0;
            for (int i = 0; i < scores.Length; i++) if (scores[i] < 0.0) scores[i] = 0.0;

            //vi: EXTRACT EVENTS
            List<AcousticEvent> matchEvents = AcousticEvent.ConvertScoreArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, dBThreshold,
                                                                            minDuration, maxDuration);
            foreach (AcousticEvent ev in matchEvents)
            {
                ev.SourceFileName = sonogram.Configuration.SourceFName;
                ev.Name = sonogram.Configuration.CallName;
            }

            
            // Edit the events to correct the start time, duration and end of events to match the max score and length of the template.
            AdjustEventLocation(matchEvents, callName, templateDuration, sonogram.Duration.TotalSeconds);

            return System.Tuple.Create(sonogram, matchEvents, scores);
        } // FELTWithSprTemplate()



        /// <summary>
        /// Edits the passed events to adjust start and end locations to position of maximum score
        /// Also correct the scores because we want the max match for a template
        /// Also correct the time scale
        /// </summary>
        /// <param name="matchEvents"></param>
        /// <param name="callName"></param>
        /// <param name="templateDuration"></param>
        /// <param name="sonogramDuration"></param>
        public static void AdjustEventLocation(List<AcousticEvent> matchEvents, string callName, double templateDuration, double sonogramDuration)
        {
            Log.WriteLine("# ADJUST EVENT LOCATIONS");
            Log.WriteLine("# Event: "+callName);
            foreach (AcousticEvent ae in matchEvents)
            {
                Log.WriteLine("# Old  event frame= {0} to {1}.", ae.oblong.r1, ae.oblong.r2);
                ae.Name = callName;
                ae.TimeStart = ae.Score_TimeOfMaxInEvent;
                ae.Duration  = templateDuration;
                ae.TimeEnd   = ae.TimeStart + templateDuration;
                if (ae.TimeEnd > sonogramDuration) ae.TimeEnd = sonogramDuration; // check for overflow.
                ae.oblong = AcousticEvent.ConvertEvent2Oblong(ae);
                ae.Score = ae.Score_MaxInEvent;
                ae.ScoreNormalised = ae.Score / ae.Score_MaxPossible;  // normalised to the user supplied threshold
                Log.WriteLine("# New event time = {0:f2} to {1:f2}.", ae.TimeStart, ae.TimeEnd);
                Log.WriteLine("# New event frame= {0} to {1}.", ae.oblong.r1, ae.oblong.r2);
            }
        }



        public static List<AcousticEvent> PruneOverlappingEvents(List<AcousticEvent> events, int percentOverlap)
        {
            double thresholdOverlap = percentOverlap / 100.0;
            int count = events.Count;
            for(int i = 0; i < count-1; i++)
                for(int j = i+1; j < count; j++)
                {
                    if (AcousticEvent.EventFractionalOverlap(events[i], events[j]) > thresholdOverlap)
                    {
                        //determine the event with highest score
                        if (events[i].Score >= events[j].Score) events[j].Name = null;
                        else                                    events[i].Name = null;
                    }
                }

            List<AcousticEvent> pruned = new List<AcousticEvent>();
            foreach (AcousticEvent ae in events)
            {
                if (ae.Name != null) pruned.Add(ae);
            }

            return pruned;
        }


        public static void WriteEventNames(List<AcousticEvent> events)
        {
            var names = new Dictionary<string, int>();
            foreach (AcousticEvent ae in events)
            {
                if (names.ContainsKey(ae.Name)) names[ae.Name]++;
                else                            names.Add(ae.Name, 1);
            }
            Log.WriteLine("Event name        Count.");
            foreach (string key in names.Keys)
            {
                Log.WriteLine("{0}     {1}.", key, names[key].ToString());
            }
        }


        public static void DrawSonogram(BaseSonogram sonogram, FileInfo path, List<AcousticEvent> predictedEvents, List<double> thresholdList, List<double[]> scoresList)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            // DO NOT NEED FOLLOWING TWO LINES BECAUSE HAVE CHANGED CODE TO ENSURE THAT ALL TEMPLATES USE THE SAME FRAME OVERLAP
            // AND THEREFORE ALL SCORE ARRAYS ARE OF THE SAME LENGTH FOR GIVEN RECORDING 
            //Log.WriteLine("# Convert score arrays to correct length for display = {0}.", sonogram.FrameCount);
            //scoresList = ConvertScoreArrayLengths(scoresList, sonogram.FrameCount);

            // DO NOT NEED FOLLOWING LINES BECAUSE HAVE CHANGED CODE TO ENSURE THAT ALL TEMPLATES USE THE SAME FRAME OVERLAP
            // AND THEREFORE ALL SCORE ARRAYS ARE OF THE SAME LENGTH FOR GIVEN RECORDING 
            // Edit the events because they will not necessarily correspond to the timescale of the display image
            //Log.WriteLine("# Convert time scale of events.");
            //foreach (AcousticEvent ae in predictedEvents)
            //{
            //    Log.WriteLine("# Event frame= {0} to {1}.", ae.oblong.r1, ae.oblong.r2);
            //    ae.FrameOffset = sonogram.FrameOffset;
            //    ae.FramesPerSecond = sonogram.FramesPerSecond;
            //    ae.oblong = AcousticEvent.ConvertEvent2Oblong(ae);
            //    Log.WriteLine("# Event time = {0:f2} to {1:f2}.", ae.StartTime, ae.EndTime);
            //    Log.WriteLine("# Event frame= {0} to {1}.", ae.oblong.r1, ae.oblong.r2);
            //}

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                
                // Add in score tracks
                for (int s = 0; s < scoresList.Count; s++)
                {
                    if (scoresList[s] == null) continue;
                    double[] scores = scoresList[s];
                    
                    double normMax = thresholdList[s] * 4; //so normalised eventThreshold = 0.25
                    for (int i = 0; i < scores.Length; i++)
                    {
                        scores[i] /= normMax;
                        if (scores[i] > 1.0) scores[i] = 1.0;
                        if (scores[i] < 0.0) scores[i] = 0.0;
                    }

                    image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, 0.25));
                } //end adding in score tracks

                image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond); 
                image.Save(path.FullName);
            } // using
        } // DrawSonogram()

        public static List<double[]> ConvertScoreArrayLengths(List<double[]> scoresList, int frameCount)
        {
            var newList = new List<double[]>();
            for (int s = 0; s < scoresList.Count; s++)
            {
                double[] scores    = scoresList[s];
                double[] newScores = new double[frameCount];
                if ((scores == null) || (scores.Length == frameCount))
                {
                    newScores = scores;
                } else
                if(scores.Length == frameCount * 2)
                {
                    for (int i = 0; i < frameCount; i++) newScores[i] = scores[i*2]; // take every second sample
                } else
                if (frameCount == scores.Length * 2)
                {
                    for (int i = 0; i < frameCount; i++) newScores[i] = scores[i / 2]; // take every sample twice
                }
                else
                {
                    LoggedConsole.WriteLine("WARNING: Score array has been recalculated from {0} items to {1} items for display.", scores.Length, frameCount);
                    double ratio = scores.Length / frameCount;
                    for (int i = 0; i < frameCount; i++)
                    {
                        int index = (int)Math.Round(i * ratio);
                        newScores[i] = scores[index]; 
                    }
                }

                newList.Add(newScores);
                continue;
            }
            return newList;
        } // ConvertScoreArrayLengths()


        public static void WriteScoreAverages2Console(List<double[]> scoresList)
        {
            foreach (double[] array in scoresList)
            {
                LoggedConsole.WriteLine("\n# SCORE ARRAY");
                //LoggedConsole.WriteLine(NormalDist.formatAvAndSD(array, 2));
                NormalDist.writeScoreStatistics(array);
            }
        }
    }
}
