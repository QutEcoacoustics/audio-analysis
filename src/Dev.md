This file contains a collection of arguments that used to be in our source code.

# audio-analysis\src\AnalysisPrograms\AcousticIndices.cs

```
if (arguments == null)
            {
                arguments = new object();

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
                //string configPath = @"C:\SensorNetworks\Output\AcousticIndices\Indices.cfg";
                //string outputDir = @"C:\SensorNetworks\Output\AcousticIndices\";
                //string csvPath = @"C:\SensorNetworks\Output\AcousticIndices\AcousticIndices.csv";

                string recordingPath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg";
                string outputDir = @"C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.Acoustic";
                string csvPath = @"C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.Acoustic\DM420036_min407_Towsey.Acoustic.Indices.csv";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows\Crows111216-001Mono5-7min.mp3";
                //string configPath = @"C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.Acoustic\temp.cfg";
                //string outputDir = @"C:\SensorNetworks\Output\Crow\";
                //string csvPath = @"C:\SensorNetworks\Output\Crow\Towsey.Acoustic.Indices.csv";

                string title = "# FOR EXTRACTION OF Acoustic Indices";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + outputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));

                int startMinute = 0;
                int durationSeconds = 0; //set zero to get entire recording
                var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
                var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);

                //var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                //var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);

//                if (true)
//                {
//                    // task_ANALYSE
//                    arguments.Task = TaskAnalyse;
//                    arguments.Source = recordingPath.ToFileInfo();
//                    arguments.Config = configPath.ToFileInfo();
//                    arguments.Output = outputDir.ToDirectoryInfo();
//                    arguments.TmpWav = segmentFName;
//                    arguments.Indices = indicesFname;
//                    arguments.Start = (int?)tsStart.TotalSeconds;
//                    arguments.Duration = (int?)tsDuration.TotalSeconds;
//                }
//
//                if (false)
//                {
//                    // task_LOAD_CSV
//                    ////string indicesImagePath = "some path or another";
//                    arguments.Task = TaskLoadCsv;
//                    arguments.InputCsv = csvPath.ToFileInfo();
//                    arguments.Config = configPath.ToFileInfo();
//                }
//
//                string indicesPath = Path.Combine(arguments.Output.FullName, arguments.Indices);
//                FileInfo fiCsvIndices = new FileInfo(indicesPath);
//                if (!fiCsvIndices.Exists)
//                {
//                    Log.InfoFormat(
//                        "\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{1}>.",
//                        arguments.Start,
//                        arguments.Source.FullName);
//                }
//                else
//                {
//                    LoggedConsole.WriteLine("\n");
//                    DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
//                    DataTableTools.WriteTable2Console(dt);
//                }
//
//                LoggedConsole.WriteLine("\n\n# Finished analysis:- " + arguments.Source.FullName);
            }
```

# audio-analysis\src\AnalysisPrograms\CanetoadOld_OBSOLETE.cs

```
            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                const string RecordingPath =
                    //@"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
                    //@"Y:\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav";
                    //@"C:\SensorNetworks\WavFiles\Canetoad\020313.MP3\Towsey.CanetoadOld\020313_608min.wav";
                    //@"C:\SensorNetworks\WavFiles\Canetoad\020313.MP3\Towsey.CanetoadOld\020313_619min.wav";
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.CanetoadOld\020313_619min.wav";
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.CanetoadOld\020313_375min.wav"; // 42, 316,375,422,704
                    //@"Y:\Results\2014Nov11-083640 - Towsey.Canetoad JCU Campus Test 020313\JCU\Campus\020313.MP3\Towsey.CanetoadOld\020313_297min.wav";
                    //@"F:\SensorNetworks\WavFiles\CaneToad\CaneToad Release Call 270213-8.wav";
                    @"F:\SensorNetworks\WavFiles\CaneToad\UndetectedCalls-2014\KiyomiUndetected210214-1.mp3";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_2_16bitPCM.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1_16bitPCM.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Canetoad\RuralCanetoads_9Jan\toads_rural_9jan2010\toads_rural1_16.mp3";
                const string ConfigPath =
                    @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.yml";
                const string OutputDir = @"C:\SensorNetworks\Output\Frogs\Canetoad\";

                ////string csvPath       = @"C:\SensorNetworks\Output\Test\TEST_Indices.csv";
                string title = "# FOR DETECTION OF CANETOAD using DCT OSCILLATION DETECTION";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + OutputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(RecordingPath));

                TowseyLibrary.Log.Verbosity = 1;
                const int StartMinute = 0;
                const int DurationSeconds = 0; // set zero to get entire recording
                TimeSpan start = TimeSpan.FromMinutes(StartMinute); // hours, minutes, seconds
                TimeSpan duration = TimeSpan.FromSeconds(DurationSeconds); // hours, minutes, seconds
                string segmentFileStem = Path.GetFileNameWithoutExtension(RecordingPath);
                string segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, StartMinute);
                string sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, StartMinute);
                string eventsFname = string.Format(
                    "{0}_{1}min.{2}.Events.csv",
                    segmentFileStem,
                    StartMinute,
                    "Towsey." + AnalysisName);
                string indicesFname = string.Format(
                    "{0}_{1}min.{2}.Indices.csv",
                    segmentFileStem,
                    StartMinute,
                    "Towsey." + AnalysisName);

                if (true)
                {
                    arguments.Source = RecordingPath.ToFileInfo();
                    arguments.Config = ConfigPath.ToFileInfo();
                    arguments.Output = OutputDir.ToDirectoryInfo();
                    arguments.Start = start.TotalSeconds;
                    arguments.Duration = duration.TotalSeconds;
                }

                if (false)
                {
                    // loads a csv file for visualisation
                    ////string indicesImagePath = "some path or another";
                    ////var fiCsvFile    = new FileInfo(restOfArgs[0]);
                    ////var fiConfigFile = new FileInfo(restOfArgs[1]);
                    ////var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                    ////IAnalysis analyser = new AnalysisTemplate();
                    ////var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                    ////returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                }
            }
```

# audio-analysis\src\AnalysisPrograms\FindEventsLikeThis.cs

```
            string title = "# FIND OTHER ACOUSTIC EVENTS LIKE THIS ONE";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Segment.CheckArguments(args);


            string recordingPath = args[0];    //the recording
            string iniPath       = args[1];    //parameters / ini file
            string targetFName   = args[2];    //name of target file 

            string targetName = Path.GetFileNameWithoutExtension(targetFName);
            string outputDir  = Path.GetDirectoryName(iniPath) + "\\";
            string opPath     = outputDir + targetName + "_info.txt";
            string matrixPath = outputDir + targetName + "_matrix.txt";
            string imagePath  = outputDir + targetName + "_target.png";
            string binaryTemplatePath  = outputDir + targetFName;
           // string trinaryTemplatePath = outputDir + targetName + "_curatedTrinary.txt";
            
            Log.WriteIfVerbose("# Output folder =" + outputDir);


            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            string callName     = dict[key_CALL_NAME];
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            bool doSegmentation = Boolean.Parse(dict[key_DO_SEGMENTATION]);
            //double dynamicRange = Double.Parse(dict[key_DYNAMIC_RANGE]);      //dynamic range for target events
            double smoothWindow = Double.Parse(dict[key_SMOOTH_WINDOW]);      //before segmentation 
            int minHz = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[key_MAX_HZ]);
            double minDuration    = Double.Parse(dict[key_MIN_DURATION]);     //min duration of event in seconds 
            double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);  //min score for an acceptable event
            int DRAW_SONOGRAMS    = Int32.Parse(dict[key_DRAW_SONOGRAMS]);    //options to draw sonogram

            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz.)", minHz, maxHz);
            Log.WriteIfVerbose("Min Duration: " + minDuration + " seconds");

            //iii: GET THE TARGET
            double[,] targetMatrix = ReadChars2TrinaryMatrix(binaryTemplatePath);
            //double[,] targetMatrix = ReadChars2TrinaryMatrix(trinaryTemplatePath);

            //iv: Find matching events
            //#############################################################################################################################################
            var results = FindMatchingEvents.ExecuteFELT(targetMatrix, recording, doSegmentation, minHz, maxHz, frameOverlap, smoothWindow, eventThreshold, minDuration);
            var sonogram       = results.Item1;
            var matchingEvents = results.Item2;
            var scores         = results.Item3;
            double matchThreshold = results.Item4;
            Log.WriteLine("# Finished detecting events like the target.");
            int count = matchingEvents.Count;
            Log.WriteLine("# Matching Event Count = " + matchingEvents.Count());
            Log.WriteLine("           @ threshold = {0:f3}", matchThreshold);
            //#############################################################################################################################################

            //v: write events count to results info file. 
            double sigDuration = sonogram.Duration.TotalSeconds;
            string fname = Path.GetFileName(recordingPath);
            string str = String.Format("{0}\t{1}\t{2}", fname, sigDuration, count);
            StringBuilder sb = AcousticEvent.WriteEvents(matchingEvents, str);
            FileTools.WriteTextFile(opPath, sb.ToString());


            //draw images of sonograms
            string opImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_matchingEvents.png";
            if (DRAW_SONOGRAMS == 2)
            {
                DrawSonogram(sonogram, opImagePath, matchingEvents, matchThreshold, scores);
            }
            else
            if ((DRAW_SONOGRAMS == 1) && (matchingEvents.Count > 0))
            {
                DrawSonogram(sonogram, imagePath, matchingEvents, matchThreshold, scores);
            }

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
```            

# audio-analysis\src\AnalysisPrograms\FrogRibit_OBSOLETE.cs

```
            if (arguments == null)
            {
                arguments = new Arguments();
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\Rheobatrachus_silus_MONO.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\FrogPond_Samford_SE_555_SELECTION_2.03-2.43.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\DavidStewart-northernlaughingtreefrog.wav";
                 string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\DataSet\CaneToads_rural1_20_MONO.wav";
                arguments.Source = new FileInfo(recordingPath);
            }

            Execute(arguments);
```
            
# audio-analysis\src\AnalysisPrograms\Frogs_OBSOLETE.cs

```
Log.Verbosity = 1;
            bool debug = MainEntry.InDEBUG;

            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav";  //POSITIVE
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Adelotus_brevis_TuskedFrog_BridgeCreek.wav";   // NEGATIVE walking on dry leaves
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";   //NEGATIVE  rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav";   //NEGATIVE  rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min602.wav";   //NEGATIVE  rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Noise\BAC3_20070924-153657_noise.wav";               // NEGATIVE  noise
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\FrogPond_Samford_SE_555_20101023-000000.mp3";  // FROGs AT SAMFORD
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Crinia_signifera_july08.wav";                  // Crinia signifera
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Frogs_BridgeCreek_Night_Extract1-31-00.mp3";   // FROGs at Bridgecreek

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Compilation6_Mono.mp3";                          // FROG COMPILATION
                string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Curramore\CurramoreSelection-mono16kHz.mp3";
                    // Curramore COMPILATION

                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Frogs.cfg";

                string outputDir = @"C:\SensorNetworks\Output\Frogs\";

                // example
                // "C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav" C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.RheobatrachusSilus.cfg" "C:\SensorNetworks\Output\Frogs\"

                const int StartMinute = 0;
                //int startMinute = 1;
                const int DurationSeconds = 60; //set zero to get entire recording
                var tsStart = new TimeSpan(0, StartMinute, 0); //hours, minutes, seconds
                var tsDuration = new TimeSpan(0, 0, DurationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(arguments.Source.Name);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, StartMinute);
                var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, StartMinute);
                var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, StartMinute, "Towsey." + AnalysisName);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, StartMinute, "Towsey." + AnalysisName);

                arguments = new Arguments
                {
                    Source = recordingPath.ToFileInfo(),
                    Config = configPath.ToFileInfo(),
                    Output = outputDir.ToDirectoryInfo(),
                    Start = tsStart.TotalSeconds,
                    Duration = tsDuration.TotalSeconds,
                };
            }

            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("# FOR DETECTION OF 'FROG SPECIES' ");
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Output folder:  " + arguments.Output);
            LoggedConsole.WriteLine("# Recording file: " + arguments.Source.Name);
            var diOutputDir = arguments.Output;

            Execute(arguments);
```
            
# audio-analysis\src\AnalysisPrograms\GroundParrotRecogniser.cs

```
            // "Example: \"trunk\\AudioAnalysis\\Matlab\\EPR\\Ground Parrot\\GParrots_JB2_20090607-173000.wav_minute_3.wav\""
```            

# audio-analysis\src\AnalysisPrograms\KoalaMale.cs

```
            bool executeDev = arguments == null;
            if (executeDev)
            {
                string recordingPath =
                    //@"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080905-001000.wav";
                    //@"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080909-013000.wav";
                    //@"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\TopKnoll_StBees_20080909-003000.wav";
                    @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\TopKnoll_StBees_VeryFaint_20081221-003000.wav";
                string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.KoalaMale.yml";
                string outputDir = @"C:\SensorNetworks\Output\KoalaMale\";

                string title = "# FOR DETECTION OF MALE KOALA using DCT OSCILLATION DETECTION";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + outputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));

                Log.Verbosity = 1;
                int startMinute = 0;

                // set zero to get entire recording
                int durationSeconds = 0;

                // hours, minutes, seconds
                TimeSpan start = TimeSpan.FromMinutes(startMinute);

                // hours, minutes, seconds
                TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                string segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                string segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
                string sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                string eventsFname = string.Format(
                    "{0}_{1}min.{2}.Events.csv",
                    segmentFileStem,
                    startMinute,
                    "Towsey." + AnalysisName);
                string indicesFname = string.Format(
                    "{0}_{1}min.{2}.Indices.csv",
                    segmentFileStem,
                    startMinute,
                    "Towsey." + AnalysisName);

                if (true)
                {
                    arguments = new Arguments
                    {
                        Source = recordingPath.ToFileInfo(),
                        Config = configPath.ToFileInfo(),
                        Output = outputDir.ToDirectoryInfo(),
                        Start = start.TotalSeconds,
                        Duration = duration.TotalSeconds,
                    };
                }

                if (false)
                {
                    // loads a csv file for visualisation
                    ////string indicesImagePath = "some path or another";
                    ////var fiCsvFile    = new FileInfo(restOfArgs[0]);
                    ////var fiConfigFile = new FileInfo(restOfArgs[1]);
                    ////var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                    ////IAnalysis analyser = new AnalysisTemplate();
                    ////var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                    // returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                }
            }

            Execute(arguments);
```
            
# audio-analysis\src\AnalysisPrograms\LewinsRail3OBSOLETE.cs

```
var executeDev = arguments == null;
            if (executeDev)
            {
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-084607.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav";
                string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-075040.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                string configPath = @"C:\SensorNetworks\Output\LewinsRail\LewinsRail.cfg";
                string outputDir = @"C:\SensorNetworks\Output\LewinsRail\";

                string title = "# FOR DETECTION OF LEWIN'S RAIL using CROSS-CORRELATION & FFT";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + outputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
                var diOutputDir = new DirectoryInfo(outputDir);

                Log.Verbosity = 1;
                int startMinute = 0;
                int durationSeconds = 60; //set zero to get entire recording
                var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
                var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
                var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);

                arguments = new Arguments
                {
                    Source = recordingPath.ToFileInfo(),
                    Config = configPath.ToFileInfo(),
                    Output = outputDir.ToDirectoryInfo(),
                    Start = tsStart.TotalSeconds,
                    Duration = tsDuration.TotalSeconds,
                };
            }

            //Execute(arguments);
```            

# audio-analysis\src\AnalysisPrograms\LimnodynastesConvex_OBSOLETE.cs

```
            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                const string RecordingPath =
                    @"F:\SensorNetworks\WavFiles\CaneToad\UndetectedCalls-2014\KiyomiUndetected210214-1.mp3";

                const string ConfigPath =
                            @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Limnodynastes_convexiusculus.yml";
                const string OutputDir = @"C:\SensorNetworks\Output\Frogs\LimnodynastesConvex\";

                string title = "# FOR DETECTION OF LIM_CON";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + OutputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(RecordingPath));

                Log.Verbosity = 1;
                const int StartMinute = 0;
                const int DurationSeconds = 0; // set zero to get entire recording
                TimeSpan start = TimeSpan.FromMinutes(StartMinute); // hours, minutes, seconds
                TimeSpan duration = TimeSpan.FromSeconds(DurationSeconds); // hours, minutes, seconds
                string segmentFileStem = Path.GetFileNameWithoutExtension(RecordingPath);
                string segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, StartMinute);
                string sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, StartMinute);
                string eventsFname = string.Format(
                    "{0}_{1}min.{2}.Events.csv",
                    segmentFileStem,
                    StartMinute,
                    "Towsey." + AnalysisName);
                string indicesFname = string.Format(
                    "{0}_{1}min.{2}.Indices.csv",
                    segmentFileStem,
                    StartMinute,
                    "Towsey." + AnalysisName);

                if (true)
                {
                    arguments.Source = RecordingPath.ToFileInfo();
                    arguments.Config = ConfigPath.ToFileInfo();
                    arguments.Output = OutputDir.ToDirectoryInfo();
                    arguments.Start = start.TotalSeconds;
                    arguments.Duration = duration.TotalSeconds;
                }

                if (false)
                {
                    // loads a csv file for visualisation
                    ////string indicesImagePath = "some path or another";
                    ////var fiCsvFile    = new FileInfo(restOfArgs[0]);
                    ////var fiConfigFile = new FileInfo(restOfArgs[1]);
                    ////var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                    ////IAnalysis analyser = new AnalysisTemplate();
                    ////var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                    ////returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                }
            }

            Execute(arguments);
```

# audio-analysis\src\AnalysisPrograms\LitoriaFallax_OBSOLETE.cs

```
                const string RecordingPath =
                    @"F:\SensorNetworks\WavFiles\CaneToad\UndetectedCalls-2014\KiyomiUndetected210214-1.mp3";

                const string ConfigPath =
                            @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaFallax.yml";
                const string OutputDir = @"C:\SensorNetworks\Output\Frogs\LitoriaFallax\";
```
                
# audio-analysis\src\AnalysisPrograms\LSKiwi1.cs

Nothing useful

# audio-analysis\src\AnalysisPrograms\LSKiwi2.cs

```
            //Following lines are used for the debug command line.
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // 8 min test recording  // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\Kiwi\Results_MixedTest\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"   "C:\SensorNetworks\WavFiles\Kiwi\Results\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt"

            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi2.cfg";

            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500_40m0s.wav";
            string outputDir = @"C:\SensorNetworks\Output\LSKiwi2\Tower_20100208_204500\";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2-20100219-202900_Airplane.mp3";
            //string outputDir     = @"C:\SensorNetworks\Output\Kiwi\KAPITI2_20100219_202900\";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\KAPITI2_20100219_202900_ANDREWS_SELECTIONS.csv";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
            //string outputDir     = @"C:\SensorNetworks\Output\Kiwi\TUITCE_20091215_220004\";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\TUITCE_20091215_220004_ANDREWS_SELECTIONS.csv";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_210000.wav";
            //string outputDir     = @"C:\SensorNetworks\Output\LSKiwi2\TuiTce_20091215_210000\";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\TUITCE_20091215_210000_ANDREWS_SELECTIONS.csv";
```            

# audio-analysis\src\AnalysisPrograms\LSKiwi3.cs

```
//Following lines are used for the debug command line.
                // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
                // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
                // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
                // 8 min test recording  // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\Kiwi\Results_MixedTest\lskiwi_Params.txt"
                // kiwi "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"   "C:\SensorNetworks\WavFiles\Kiwi\Results\lskiwi_Params.txt"
                // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt"

                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.LSKiwi3.cfg";

                //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";
                string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500_40m0s.wav";
                string outputDir = @"C:\SensorNetworks\Output\LSKiwi3\Dev\";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav";
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2-20100219-202900_Airplane.mp3";
                //string outputDir     = @"C:\SensorNetworks\Output\Kiwi\KAPITI2_20100219_202900\";
                //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\KAPITI2_20100219_202900_ANDREWS_SELECTIONS.csv";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
                //string outputDir     = @"C:\SensorNetworks\Output\Kiwi\TUITCE_20091215_220004\";
                //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\TUITCE_20091215_220004_ANDREWS_SELECTIONS.csv";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_210000.wav";
                //string outputDir     = @"C:\SensorNetworks\Output\LSKiwi2\TuiTce_20091215_210000\";
                //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\TUITCE_20091215_210000_ANDREWS_SELECTIONS.csv";
```
                
# audio-analysis\src\AnalysisPrograms\Rain_OBSOLETE.cs

```
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min602.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min944.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min1031.wav";  //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min1036.wav";  //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min1101.wav";  //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\Jackaroo_20080715-103940.wav";   //koala
                //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080909-013000.wav";   //koala & mobile spikes
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Adelotus_brevis_TuskedFrog_BridgeCreek.wav";   //walking on dry leaves
                //string recordingPath = @"C:\SensorNetworks\Output\SunshineCoast\Acoustic\Site1\DM420036_min1081.wav";   //cicada
                string recordingPath = @"C:\SensorNetworks\Output\SunshineCoast\Acoustic\Site1\DM420036_min1076.wav";
                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Rain.cfg";
                string outputDir = @"C:\SensorNetworks\Output\Rain\";
                //string csvPath       = @"C:\SensorNetworks\Output\Rain\RainIndices.csv";
```
                
# audio-analysis\src\AnalysisPrograms\Sandpit.cs

Nothing useful

# audio-analysis\src\AnalysisPrograms\SpeciesAccumulationCurve.cs

Nothing useful

# audio-analysis\src\TowseyLibrary\TernaryPlots.cs

Nothing useful

# audio-analysis\src\analysisprograms\audio2inputforconvcnn.cs

```
                // prior to processing
                // Y:\Results\2014Aug29-000000 - ConvDNN Data Export\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.csv
                // audio_event_id   audio_recording_id  audio_recording_uuid    event_created_at_utc    projects    site_id site_name   event_start_date_utc    event_start_seconds event_end_seconds   event_duration_seconds  low_frequency_hertz high_frequency_hertz    padding_start_time_seconds  padding_end_time_seconds    common_tags species_tags    other_tags  listen_url  library_url

                // Y:\Results\2014Aug29-000000 - ConvDNN Data Export\Output\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv
                // audio_event_id   audio_recording_id  audio_recording_uuid    event_created_at_utc    projects    site_id site_name   event_start_date_utc    event_start_seconds event_end_seconds   event_duration_seconds  low_frequency_hertz high_frequency_hertz    padding_start_time_seconds  padding_end_time_seconds    common_tags species_tags    other_tags  listen_url  library_url path    download_success    skipped

                // csv file containing recording info, call bounds etc
                //Source = @"C:\SensorNetworks\Output\ConvDNN\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv".ToFileInfo(),
                Source = @"Y:\Results\2014Aug29-000000 - Mangalam Data Export\Output\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv".ToFileInfo(),

                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Mangalam.Sonogram.yml",

                Output = (@"C:\SensorNetworks\Output\ConvDNN\" + datestamp).ToDirectoryInfo(),
```

# audio-analysis\src\analysisprograms\audio2sonogram.cs

```
                //MARINE
                //Source = @"C:\SensorNetworks\WavFiles\MarineRecordings\20130318_171500.wav".ToFileInfo(),
                //Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.SonogramMarine.yml".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\MarineSonograms\".ToDirectoryInfo(),

                // LEWINs RAIL
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\LewinsRail".ToDirectoryInfo(),
                Source = @"G:\SensorNetworks\WavFiles\LewinsRail\FromLizZnidersic\Lewinsrail_TasmanIs_Tractor_SM304253_0151119_0640_1min.wav".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\LewinsRail\LewinsRail_ThreeCallTypes".ToDirectoryInfo(),

                //CANETOAD
                //Source = @"Y:\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav".ToFileInfo(),

                //Source = @"C:\SensorNetworks\WavFiles\Frogs\JCU\Litoria fellax1.mp3".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Frogs\MiscillaneousDataSet\CaneToads_rural1_20_MONO.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\NW_NW273_20101013-051200-0514-1515-Brown Cuckoo-dove1.wav".ToFileInfo(),

                //Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\Kanowski_651_233394_20120831_072112_4.0__.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\Melaleuca_Middle_183_192469_20101123_013009_4.0__.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\SE_399_188293_20101014_132950_4.0__.wav".ToFileInfo(),

                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml",

                //Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Mangalam.Sonogram.yml".ToFileInfo(),
```

# audio-analysis\src\AnalysisPrograms\Draw\Zooming\DrawZoomingSpectrograms.Dev.cs

```
            // INPUT and OUTPUT DIRECTORIES
            // 2010 Oct 13th
            // string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014May06-100720 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\RibbonTest";

            // string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct13_SpectralIndices";

            // 2010 Oct 14th
            // string ipFileName = "b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct14_SpectralIndices";

            // 2010 Oct 15th
            // string ipFileName = "d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct15_SpectralIndices";

            // 2010 Oct 16th
            // string ipFileName = "418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct16_SpectralIndices";

            // 2010 Oct 17th
            // string ipFileName = "0f2720f2-0caa-460a-8410-df24b9318814_101017-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct17_SpectralIndices";

            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.OneSecondIndices";
            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.200msIndicesKIWI-TEST";

            // KOALA RECORDING AT ST BEES
            //string ipdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016\SpectrogramFocalZoom";
            //string opdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016";

            // TEST recordings
            //string ipdir = @"C:\SensorNetworks\Output\Test\Test\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\TestHiResRidge";

            // BAC
            //string ipdir = @"C:\SensorNetworks\Output\BAC\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\BAC\HiResRidge";

            // BIRD50
            //string ipdir = @"C:\SensorNetworks\Output\BIRD50\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\BIRD50";

            // ANTHONY'S TEST DATA
            //string ipdir = @"G:\SensorNetworks\ZoomingTestDataFromAnthony\testAnalysis\indices\Towsey.Acoustic";
            string ipdir = @"G:\SensorNetworks\ZoomingTestDataFromAnthony\testAnalysis\indices2\Towsey.Acoustic";
            string opdir = @"G:\SensorNetworks\ZoomingTestDataFromAnthony\testAnalysis\zoomingTowsey3";

            // ECLIPSE FARMSTAY
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Eclipse\EclipseFarmstay.200ms\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImage";

            //BRISTLE BIRD
            //string ipdir = @"C:\SensorNetworks\Output\BristleBird\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImageBristleBird";

            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImage";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramTileZoom\TiledImages";

            // ################ TEST a colour scheme for the high resolution frame spectrograms.
            //var cch = TowseyLibrary.CubeHelix.GetCubeHelix();
            //cch.TestImage(Path.Combine(opdir, "testImageColorHelixScale.png"));
            //var rsp = new TowseyLibrary.CubeHelix("redscale");
            //rsp.TestImage(Path.Combine(opdir, "testImageRedScale1.png"));
            //var csp = new TowseyLibrary.CubeHelix("cyanscale");
            //csp.TestImage(Path.Combine(opdir, "testImageCyanScale1.png"));
            // ################ TEST a colour scheme for the high resolution frame spectrograms.

            //string config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramScalingConfig.json";
            string config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramZoomingConfig.yml";

            //string config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramHiResConfig.yml";

            return null; /*new Arguments
            {
                // use the default set of index properties in the AnalysisConfig directory.
                SourceDirectory = ipdir.ToDirectoryInfo(),
                Output = opdir.ToDirectoryInfo(),
                SpectrogramTilingConfig = config.ToFileInfo(),

                // draw a focused multi-resolution pyramid of images
                //ZoomAction = Arguments.ZoomActionType.Tile,
                ZoomAction = Arguments.ZoomActionType.Focused,

                //FocusMinute = 1,
                FocusMinute = 15,
            };*/
```

# audio-analysis\src\analysisprograms\draweasyimage.cs

```
            DateTimeOffset? dtoStart = null;
            DateTimeOffset? dtoEnd = null;

            FileInfo indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigForEasyImages.yml");
            FileInfo sunrisesetData = new FileInfo(@"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv");

            // ########################## CSV FILES CONTAINING SUMMARY INDICES IN 24 hour BLOCKS
            // top level directory
            string opFileStem = "GympieNP-2015";
            DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP"), };

            //string opFileStem = "Woondum3-2015";
            //DirectoryInfo[] dataDirs = { new DirectoryInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\Woondum3"),   };

            // The filter pattern finds summary index files
            string fileFilter = "*SummaryIndices.csv";
            string opPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults";

            dtoStart = new DateTimeOffset(2015, 06, 22, 0, 0, 0, TimeSpan.Zero);
            dtoEnd = new DateTimeOffset(2015, 10, 11, 0, 0, 0, TimeSpan.Zero);
```

# audio-analysis\src\analysisprograms\drawsummaryindextracks.cs

```
            //use the following for the command line for the <indicesCsv2Image> task.
            //indicesCsv2Image  "C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.Indices.csv"            "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.MultiAnalyser.cfg"  C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.MultiAnalyser\DM420036_Towsey.MultiAnalyser.IndicesNEW.png
            //indicesCsv2Image  "C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.csv" ""       C:\SensorNetworks\Output\Frogs\ShiloDebugOct2012\IndicesCsv2Image\DM420044_20111020_000000_Towsey.Acoustic.Indices.png
            //indicesCsv2Image  "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.csv"  "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg"   "C:\SensorNetworks\Output\LSKiwi3\Towsey.Acoustic\TOWER_20100208_204500_Towsey.Acoustic.Indices.png
            //return new Arguments
            //{
            //    InputCsv              = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM420036.Towsey.Acoustic\DM420036_Towsey.Acoustic.Indices.csv".ToFileInfo(),
            //    ImageConfig           = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),
            //    IndexPropertiesConfig = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
            //    Output                = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM420036.Towsey.Acoustic\DM420036_Towsey.Acoustic.Indices2.png".ToFileInfo()
            //};

            return new Arguments
            {
                //IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
                //ImageConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg".ToFileInfo(),

                //2010 Oct 13th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct13_SummaryIndices.png".ToFileInfo()

                //2010 Oct 14th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct14_SummaryIndices.png".ToFileInfo()

                //2010 Oct 15th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct15_SummaryIndices.png".ToFileInfo()

                //2010 Oct 16th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct16_SummaryIndices.png".ToFileInfo()

                //2010 Oct 17th
                //InputCsv = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000.mp3\Towsey.Acoustic\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000_Towsey.Acoustic.Indices.csv".ToFileInfo(),
                //Output   = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct17_SummaryIndices.png".ToFileInfo(),
            };
```

# audio-analysis\src\analysisprograms\oscillationsgeneric.cs

```
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),

                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav".ToFileInfo(),
                //Source = @"Y:\Jie Frogs\Recording_1.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Frogs\MiscillaneousDataSet\CaneToads_rural1_20_MONO.wav".ToFileInfo(),

                //Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.OscillationsGeneric.yml".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms".ToDirectoryInfo(),
```

# audio-analysis\src\analysisprograms\recognizers\base\recognizerentry.cs

```
            // The MULTI-RECOGNISER
            /*
            // Canetoad, Litoria fallax and Limnodynastes convex.
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\MultiLabel\Gympie_CaneToad_Lnasuta_Lfallax.wav";
            //string outputPath    = @"G:\SensorNetworks\Output\Frogs\Multirecognizer_2016December";
            */

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\LitoriaWotjulumensisAndRothii\Lrothii_emerald_river_1014_252497_20131216_180244_30_0.wav";
            //string recordingPath = @"D:\SensorNetworks\WavFiles\Frogs\SherynBrodie\Lconvex_Lbicolor_GolfC_20170213_183122.wav";
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Ecosounds.MultiRecognizer.yml";
            string outputPath = @"D:\SensorNetworks\Output\Frogs\TestOfRecognizers-2017August";

            //Ardea insignis (The White-bellied Herron
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\Heron_commonCall_downsampled.wav";
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Bhutan\waklaytar_site3_1379_347077_20160409_061730_40_0.wav";
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Bhutan\waklaytar_site3_1379_347093_20160404_061431_130_0.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\Both call types of Heron.wav";
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Bhutan\waklaytar_site3_1379_347103_20160329_133319_130_0.wav";
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Bhutan\waklaytar_site3_1379_347093_20160404_062131_40_0.wav";
            //string outputPath = @"G:\SensorNetworks\Output\Bhutan\";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TsheringDema\WBH12HOURS-D_20160403_120000_238min.wav";

            //string recordingPath = @"G:\SensorNetworks\WavFiles\Bhutan\SecondDeployment\WBH12HOURS-N_20160426_000010.wav";
            //string outputPath = @"C:\SensorNetworks\Output\TsheringDema";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.ArdeaInsignis.yml";

            // Path from Anthony
            // "C:\Users\Administrator\Desktop\Sensors Analysis\ParallelExecutables\2\AnalysisPrograms.exe" audio2csv - source "Y:\Tshering\WBH_Walaytar\201505 - second deployment\Site2_Waklaytar\24Hours WBH_28032016\WBH12HOURS-D_20160403_120000.wav" - config "C:\Users\Administrator\Desktop\Sensors Analysis\Towsey.ArdeaInsignis.Parallel.yml" - output "Y:\Results\2016Oct31-145124\Tshering\WBH_Walaytar\201505 - second deployment\Site2_Waklaytar\24Hours WBH_28032016\WBH12HOURS-D_20160403_120000.wav" - tempdir F:\2 - m True - n

            /*
            // Canetoad
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Canetoad\Groote_20160803_151738_Canetoad_LinsPlayback.wav";   // Positive call
            //string recordingPath = @"C:\Work\GitHub\recognizer-tests\tests\species\Rhinella_marina\data\CaneToad_Gympie.wav";         // Positive call
            //string recordingPath = @"C:\Work\GitHub\recognizer-tests\tests\species\Rhinella_marina\data\Lwotjulumensis_trill_bickerton_20131212_214430.wav";    // Positive call
            //string recordingPath = @"C:\Work\GitHub\recognizer-tests\tests\species\Rhinella_marina\data\TruckMotor_20150603_004248.wav"; // Negative call
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Canetoad\FalsePositives\FalsePosFromPaul_2015-06-02-031015_downsampled.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Canetoad\FalsePositives\FalsePosFromPaul_2015-06-03-004248_downsampled.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\CaneToads_rural1_20.mp3";
            //string recordingPath = @"Y:\Groote\2016 March\Emerald River\CardA\Data\EMERALD_20150703_103506.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\Canetoad\Rural1";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.RhinellaMarina.yml";
            */

            /*
            //Crinia remota
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\CriniaSpecies\EmeraldRiver_CriniaRemota_20140206_032030.wav";
            //string outputPath    = @"G:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016November";
            string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.CriniaRemota.yml";
            */

            /*
            //Crinia tinnula
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\Crinia\CriniaTinnula.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016October";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Stark.CriniaTinnula.yml";
            */

            /*
            // Cyclorana novaehollandiae
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\28 Cyclorana novaehollandiae.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.CycloranaNovaeholl.yml";
            */

            /*
            // Lewin's Rail  --  Lewinia pectoralis
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-084607.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-075040.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            //string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LewiniaPectoralis.yml";
            //string outputPath     = @"C:\SensorNetworks\Output\LewinsRail\";
            */

            /*
            // LEWIN'S RAIL TEST
            //string recordingPath = @"G:\SensorNetworks\SoftwareTests\UnitTest_Towsey.LewiniaPectoralis\Data\BAC2_20071008-085040.wav";
            //string configPath = @"G:\SensorNetworks\SoftwareTests\UnitTest_Towsey.LewiniaPectoralis\Data\Towsey.LewiniaPectoralis.yml";
            //string outputPath = @"G:\SensorNetworks\SoftwareTests\UnitTest_Towsey.LewiniaPectoralis";
            string recordingPath = @"G:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LewiniaPectoralis.yml";
            string outputPath = @"C:\SensorNetworks\Output\LewinsRail\Results2017";
            */

            // Limnodynastes convex
            //string recordingPath = @"D:\SensorNetworks\WavFiles\Frogs\LimnodynastesSpecies\10 Limnodynastes convexiusculusMONO.wav";
            //string recordingPath = @"D:\SensorNetworks\WavFiles\Frogs\SherynBrodie\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30.wav";
            string recordingPath = @"D:\SensorNetworks\WavFiles\Frogs\SherynBrodie\Lconvex_Paradise_20170206_0220.wav";

            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\LimnoConvex";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LimnodynastesConvex.yml";

            // Litoria bicolor
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Bickerton\bicolor_bickerton_island_1013_255205_20131211_191621_30_0.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Bickerton\bicolor_bickerton_island_1013_255205_20131211_195821_30_0.wav";
            //string outputPath = @"G:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016October\";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LitoriaBicolor.yml";

            // Litoria caerulea Common green tree frog
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Groote\EmeraldRiver_LitoriaCaerulea_Lrothii_20131223_220522.wav";
            //string outputPath = @"G:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016November";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.LitoriaCaerulea.yml";

            // Litoria fallax
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\53 Litoria fallax.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaFallax.yml";

            // Litoria nasuta - WORKING ON THIS JUST BEFORE 2016 CHRISTMAS BREAK
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\43 Litoria nasuta.mp3";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Groote\EmeraldRiver_LitoriaNasuta_Lbicolor_20131225_223700_30_0.wav";
            //string outputPath    = @"G:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016December";
            //string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaNasuta.yml";

            // Litoria olongburensis
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\TEST_16000Hz_LitoriaOlongburensis.wav";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016Sept\Canetoad";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Canetoad.yml";

            // Litoria rothii.
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\49 Litoria rothii.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\69 Litoria rothii.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\LitoriaWotjulumensisAndRothii\bickerton_island_1013_255205_20131211_194041_30_0.wav";
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Bickerton\rothii_bickerton_island_1013_255213_20131212_205130_30_0.wav";
            //string recordingPath = @"D:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Bickerton\rothii_bickerton_island_1013_255213_20131212_205630_30_0.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\LitoriaWotjulumensisAndRothii\Lrothii_emerald_river_1014_252497_20131216_180244_30_0.wav";
            //string outputPath = @"D:\SensorNetworks\Output\Frogs\TestOfRecognizers-2017August\";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaRothii.yml";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.CriniaRemota.yml";

            // Litoria rubella
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\50 Litoria rubella.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaRubella.yml";

            // Litoria wotjulumensis
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\LitoriaSpecies\Bickerton\wotjulumensis_bickerton_island_1013_255205_20131211_192951_30_0.wav";
            //string outputPath    = @"G:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016October\";
            //string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.LitoriaWatjulumensis.yml";

            // Platyplectrum ornatum
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Frogs\PlatyplectrumSp\p_ornatum_bickerton_island_1013_255599_20140213_214500_30_0.wav";
            //string outputPath = @"E:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016October\";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\RecognizerConfigFiles\Towsey.PlatyplectrumOrnatum.yml";

            // Uperoleia inundata
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Frogs\UperoleiaSp\u_inundata_bickerton_island_1013_255713_20140112_212900_30_0.wav";
            //string recordingPath = @"E:\SensorNetworks\WavFiles\Frogs\UperoleiaSp\u_inundata_bickerton_island_1013_255713_20140112_213030_30_0.wav";
            //string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.UperoleiaInundata.yml";
            //string outputPath    = @"E:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016October";

            // Uperoleia lithomoda
            //string recordingPath = @"G:\SensorNetworks\WavFiles\Frogs\UperoleiaSpecies\UperoleiaLithomoda_BickertonIsland_140128_201100.mp3";
            //string outputPath = @"G:\SensorNetworks\Output\Frogs\TestOfRecognizers-2016December";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.UperoleiaLithomoda.yml";

            // Uperoleia mimula
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\23 Uperoleia mimula.mp3";
            //string outputPath = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test";
            //string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.UperoleiaMimula.yml";

            /*
            // Fresh water blue cat fish
            string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_ChrisAfterFiltering.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_LeftChannel_First60s.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_LeftChannel.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Freshwater\BlueCatfish_LonePine_ChrisFilteredLeftChFirst60s.wav";
            string outputPath = @"C:\SensorNetworks\Output\FreshWater";
            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.IctalurusFurcatus.yml";
            */

            var arguments = new Arguments
            {
//                Source = recordingPath.ToFileInfo(),
//                Config = configPath.ToFileInfo(),
//                Output = outputPath.ToDirectoryInfo(),
            };

            // #########  NOTE: All other parameters are set in the .yml file assigned to configPath variable above.
            return arguments;
```

# audio-analysis\src\analysisprograms\spt.cs

```
            //spt C:\SensorNetworks\WavFiles\BridgeCreek\cabin_GoldenWhistler_file0127_extract1.mp3 C:\SensorNetworks\Output\SPT\ 2.0
```

# audio-analysis\src\audioanalysistools\channelintegrity.cs

```
            //FileInfo audioFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad.wav");
            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad.2hours.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad.2hours.RIGHT.wav");

            //FileInfo audioFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad_1MinExtractAt4h06min.wav");
            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad_1MinExtractAt4h06min.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad_1MinExtractAt4h06min.RIGHT.wav");

            FileInfo audioFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150725_064552_Gympie_bad_1MinExtractAt5h16m.wav");

            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150725_064552_Gympie_bad_1MinExtractAt5h16m.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150725_064552_Gympie_bad_1MinExtractAt5h16m.RIGHT.wav");

            //FileInfo ipFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150819_133146_gym_good_1MinExtractAt5h40min.wav");
            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150819_133146_gym_good_1MinExtractAt5h40min.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150819_133146_gym_good_1MinExtractAt5h40min.RIGHT.wav");
            var opDirectory = new DirectoryInfo(@"C:\SensorNetworks\output\ChannelIntegrity");
```

# audio-analysis\src\audioanalysistools\longdurationspectrograms\ldspectrogram3d.cs

```
            DateTime time = DateTime.Now;
            string datestamp = $"{time.Year}{time.Month:d2}{time.Day:d2}";
            var dev = new Arguments();
            dev.IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml"
                .ToFileInfo();
            dev.BrisbaneSunriseDatafile = @"C:\SensorNetworks\OutputDataSets\SunRiseSet\SunriseSet2013Brisbane.csv".ToFileInfo();
            dev.InputDir = @"C:\SensorNetworks\OutputDataSets\SERF - November 2013 Download".ToDirectoryInfo();
            dev.TableDir = @"C:\SensorNetworks\OutputDataSets\Spectrograms3D\".ToDirectoryInfo();
            dev.OutputDir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Spectrograms3D\"
                .ToDirectoryInfo();
            dev.SampleRate = 17640;
            dev.FrameSize = 512;
            dev.Verbose = true;
            return dev;
```

# audio-analysis\src\towseylibrary\otsuthresholder.cs

```
            // INPUT and OUTPUT DIRECTORIES

            // set up IP and OP directories
            //string InputFile = @"C:\Work\GitHub\audio-analysis\Extra Assemblies\OtsuThreshold\harewood.jpg";
            //string InputFile = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-085040.png";
            string InputFile = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013June19\SERF_20130619_064615_000_0156h.png";

            //string imageInputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingRidgeImages";
            string OutputDir = @"C:\SensorNetworks\Output\ThresholdingExperiments";
            string outputFilename = "binary3.png";

            //string imagOutputDireOutputDir = @"C:\SensorNetworks\Output\BIRD50\TestingRidgeImages";

            FileInfo ipImage = new FileInfo(InputFile);
            DirectoryInfo opDir = new DirectoryInfo(OutputDir);

            //FileInfo fiSpectrogramConfig = null;
            FileInfo fiSpectrogramConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramFalseColourConfig.yml");
```
        