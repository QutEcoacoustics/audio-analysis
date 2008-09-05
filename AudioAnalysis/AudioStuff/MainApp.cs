using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using TowseyLib;


namespace AudioStuff
{
    /// <summary>
    /// This program runs in several modes:
    /// MakeSonogram: Reads .wav file and converts data to a sonogram 
    /// ExtractTemplate: Extracts a call template from the sonogram 
    /// ReadTemplateAndScan: Scans the sonogram with a previously prepared template
    /// </summary>
    enum Mode { ArtificialSignal, MakeSonogram, IdentifyAcousticEvents, CreateTemplate, CreateTemplateAndScan, 
                ReadTemplateAndScan, ScanMultipleRecordingsWithTemplate, AnalyseMultipleRecordings, ERRONEOUS
    }

    static class MainApp
    {


        /// <summary>
        /// 
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //******************** USER PARAMETERS ***************************
            //Mode userMode = Mode.ArtificialSignal;
            //Mode userMode = Mode.MakeSonogram;
            //Mode userMode = Mode.IdentifySyllables;
            //Mode userMode = Mode.CreateTemplate;
            Mode userMode = Mode.CreateTemplateAndScan;
            //Mode userMode = Mode.ReadTemplateAndScan;
            //Mode userMode = Mode.ScanMultipleRecordingsWithTemplate;
            //Mode userMode = Mode.AnalyseMultipleRecordings;
            

            // directory structure
            const string iniFPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            //const string templateDir = @"C:\SensorNetworks\Templates\";
            //const string opDirName = @"C:\SensorNetworks\TestOutput_Exp6\";
            const string opDirName = @"C:\SensorNetworks\Sonograms\";
            //const string artDirName = @"C:\SensorNetworks\ART\";
            const string wavFExt = WavReader.wavFExt;

            //BRISBANE AIRPORT CORP
            const string wavDirName = @"C:\SensorNetworks\WavFiles\";
            //string wavFileName = "sineSignal";
            //string wavFileName = "golden-whistler";
            //string wavFileName = "BAC2_20071008-085040";  //Lewin's rail kek keks used for obtaining kek-kek template.
            string wavFileName = "BAC1_20071008-084607";  //faint kek-kek call
            //string wavFileName = "BAC2_20071011-182040_cicada";  //repeated cicada chirp 5 hz bursts of white noise
            //string wavFileName = "dp3_20080415-195000"; //ZERO SIGNAL silent room recording using dopod
            //string wavFileName = "BAC2_20071010-042040_rain";  //contains rain and was giving spurious results with call template 2
            //string wavFileName = "BAC2_20071018-143516_speech";
            //string wavFileName = "BAC2_20071014-022040nightnoise"; //night with no signal in Kek-kek band.
            //string wavFileName = "BAC2_20071008-195040"; // kek-kek track completely clear
            //string wavFileName = "BAC3_20070924-153657_wind";
            //string wavFileName = "BAC3_20071002-070657";
            //string wavFileName = "BAC3_20071001-203657";
            //string wavFileName = "BAC5_20080520-040000_silence";

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

            //test wav files
            const string testDirName = @"C:\SensorNetworks\TestWavFiles\";
            //const string testDirName = @"C:\SensorNetworks\WavDownloads\BAC2\";

            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec









            Console.WriteLine("\nMODE=" + Mode.GetName(typeof(Mode), userMode));

            //******************************************************************************************************************
            //******************************************************************************************************************
            //SET TEMPLATE HERE  ***********************************************************************************************
            int callID = 3;
            //******************************************************************************************************************
            //******************************************************************************************************************

            //****************** DEFAULT CALL PARAMETERS
            string callName = "NO NAME";
            string callComment = "DEFAULT VALUE";
            string sourceFile = "NO_NAME"; 

            //ENERGY AND NOISE PARAMETERS
            double dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
            //backgroundFilter= //noise reduction??

            //MFCC PARAMETERS
            int sampleRate; //determined by source WAV file
            int frameSize = 512;
            double frameOverlap = 0.5;
            int filterBankCount = 64;
            bool doMelConversion = true;
            int ceptralCoeffCount = 12;
            int deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
            bool includeDeltaFeatures = true;
            bool includeDoubleDeltaFeatures = true;

            //FEATURE VECTOR EXTRACTION PARAMETERS
            FV_Source fv_Source = FV_Source.SELECTED_FRAMES;  //FV_Source.MARQUEE;
            string selectedFrames = "0";
            int min_Freq     = 0; //Hz
            int max_Freq     = 9999; //Hz
            int marqueeStart = 999;
            int marqueeEnd   = 999;

            FV_Extraction fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS;  //AT_FIXED_INTERVALS
            int fvExtractionInterval = 999; //milliseconds
            bool doFvAveraging = false;
            string fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

            // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
            int zScoreSmoothingWindow = 3;
            double zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

            //LANGUAGE MODEL
            int numberOfWords = 0; //number of defined song variations 
            string[] words = { "999" };
            //if select high sensitivity search, then specificity reduced i.e. produces a greater number of false positives 
            bool doHighSensitivitySearch = true;
            int maxSyllables = 1;  //NOT YET USED
            double maxSyllableGap = 0.25; //seconds  NOT YET USED
            double typicalSongDuration = 1.000; //seconds USED WHEN SCORING FOR HOTSPOTS

            // SCORING PROTOCOL
            ScoringProtocol scoringProtocol = ScoringProtocol.PERIODICITY; //three options are HOTSPOTS, WORDMATCH, PERIODICITY
            int callPeriodicity = 999;




            //************* CALL 1 PARAMETERS ***************
            if (callID == 1)
            {
                callName = "Lewin's Rail Kek-kek";
                callComment = "Template consists of a single KEK!";
                sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.

                //ENERGY AND NOISE PARAMETERS
                dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
                //backgroundFilter= //noise reduction??

                //MFCC PARAMETERS
                frameSize = 512;
                frameOverlap = 0.5;
                filterBankCount = 64;
                doMelConversion = true;
                ceptralCoeffCount = 12;
                deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
                includeDeltaFeatures = true;
                includeDoubleDeltaFeatures = true;


                //FEATURE VECTOR EXTRACTION PARAMETERS
                fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                selectedFrames = "1784,1828,1848,2113,2132,2152";
                min_Freq = 1500; //Hz
                max_Freq = 5500; //Hz
                //marqueeStart = 999;
                //marqueeEnd   = 999;

                //fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
                //fvExtractionInterval = 200; //milliseconds
                doFvAveraging = true;
                fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";


                //LANGUAGE MODEL
                numberOfWords = 3; //number of defined song variations
                words = new string[numberOfWords];
                words[0] = "111"; words[1] = "11"; words[2] = "1"; 
                //if select high sensitivity search, then specificity reduced i.e. produces a greater number of false positives 
                doHighSensitivitySearch = false;
                //maxSyllables=
                //double maxSyllableGap = 0.25; //seconds
                //double maxSong=


                // SCORING PARAMETERS PROTOCOL
                // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                zScoreSmoothingWindow = 3;
                zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
                scoringProtocol = ScoringProtocol.PERIODICITY; //three options are HOTSPOTS, WORDMATCH, PERIODICITY
                callPeriodicity = 208;
            } //end of if (callID == 1)


            //******************************************************************************************************************
            //************* CALL 2 PARAMETERS ***************
            if (callID == 2)
            {
                callName = "Lewin's Rail Kek-kek";
                callComment = "Template consists of a single KEK!";
                sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
                frameSize = 512;
                frameOverlap = 0.5;
                min_Freq = 1500; //Hz
                max_Freq = 5500; //Hz
                dynamicRange = 30.0; //decibels above noise level #### YET TO TO DO THIS PROPERLY
                //backgroundFilter= //noise reduction??
                filterBankCount = 64;
                doMelConversion = true;
                ceptralCoeffCount = 12;
                deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
                includeDeltaFeatures = true;
                includeDoubleDeltaFeatures = true;

                //FEATURE VECTOR PREPARATION DETAILS
                fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                selectedFrames = "1784,1828,1848,2113,2132,2152";

                // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                zScoreSmoothingWindow = 3;
                zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

                //LANGUAGE MODEL
                numberOfWords = 3; //number of defined song variations 
                words = new string[numberOfWords];
                words[0] = "111"; words[1] = "11"; words[2] = "1";
                //if select high sensitivity search, then specificity reduced i.e. produces a greater number of false positives 
                doHighSensitivitySearch = true;

                // SCORING PROTOCOL
                scoringProtocol = ScoringProtocol.PERIODICITY; //three options are HOTSPOTS, WORDMATCH, PERIODICITY
                callPeriodicity = 208;
            }//end of if (callID == 2)


            //******************************************************************************************************************
            //************* CALL 3 PARAMETERS ***************
            if (callID == 3)
            {
                callName = "Soulful-tuneful";
                callComment = "Unknown species in faint kek-kek file!";
                sourceFile = "BAC1_20071008-084607"; 

                //ENERGY AND NOISE PARAMETERS
                dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
                //backgroundFilter= //noise reduction??

                //MFCC PARAMETERS
                frameSize = 512;
                frameOverlap = 0.5;
                filterBankCount = 64;
                doMelConversion = true;
                ceptralCoeffCount = 12;
                deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
                includeDeltaFeatures = true;
                includeDoubleDeltaFeatures = true;


                //FEATURE VECTOR EXTRACTION PARAMETERS
                fv_Source = FV_Source.MARQUEE;  //options are SELECTED_FRAMES or MARQUEE
                min_Freq = 600; //Hz
                max_Freq = 3700; //Hz
                marqueeStart = 4760;  //frame id
                marqueeEnd   = 4870;

                fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
                //fvExtractionInterval = 200; //milliseconds
                fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";


                //LANGUAGE MODEL = automated for HOTSPOT scoring
                //numberOfWords = 3; //number of defined song variations
                //words = new string[numberOfWords];
                //words[0] = "1"; words[1] = "2"; words[2] = "3";
                //if select high sensitivity search, then specificity reduced i.e. produces a greater number of false positives 
                doHighSensitivitySearch = false;
                //maxSyllables=
                //double maxSyllableGap = 0.25; //seconds
                typicalSongDuration = 2.000; //seconds

                // SCORING PARAMETERS PROTOCOL
                // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                zScoreSmoothingWindow = 3;
                zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
                scoringProtocol = ScoringProtocol.HOTSPOTS; //three options are HOTSPOTS, WORDMATCH, PERIODICITY
            } //end of if (callID == 3)


            //******************************************************************************************************************
            //************* CALL 4 PARAMETERS ***************
            //coordinates to extract template using bitmap image of sonogram
            //image coordinates: rows=freqBins; cols=timeSteps
            if (callID == 4)
            {
                callName = "Cicada";
                callComment = "Broadband Chirp Repeated @ 5Hz";
                int y1 = 115; int x1 = 545;
                int y2 = 415; int x2 = 552;
            }//end of if (callID == 4)










            //************** END OF USER PARAMETERS ***************************

            Console.WriteLine("DATE AND TIME:"+DateTime.Now);
            
            Sonogram s;

            //SWITCH USER MODES
            switch (userMode)
            {
                case Mode.ArtificialSignal:
                    try
                    {
                        int sigSampleRate = 22050;
                        double duration = 30.245; //sig duration in seconds
                        string sigName = "artificialSignal";
                        //int[] harmonics = { 1500, 3000, 4500, 6000 };
                        int[] harmonics = { 1000, 4000 };
                        double[] signal = DSP.GetSignal(sigSampleRate, duration, harmonics);
                        s = new Sonogram(iniFPath, sigName, signal, sigSampleRate);
                        s.SetVerbose(1);
                        //double[,] m = s.Matrix;
                        double[,] m = s.SpectralM;

                        s.SaveImage(m, null);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED ON ARTIFICIAL SIGNAL");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.MakeSonogram:     //make sonogram and bmp image
                    string wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFPath, wavPath);
                        //double[,] m = s.AmplitudM;
                        double[,] m = s.SpectralM;
                        //double[,] m = s.CepstralM;
                        //double[,] m = s.AcousticM;

                        //m = ImageTools.DetectHighEnergyRegions(m, threshold); //binary matrix showing areas of high acoustic energy
                        //m = ImageTools.Shapes_lines(m); //binary matrix showing high energy lines
                        //m = ImageTools.Convolve(m, Kernal.HorizontalLine5);
                        //double[,] m = ImageTools.Convolve(s.Matrix, Kernal.DiagLine2);
                        //double[,] m = ImageTools.Convolve(s.Matrix, Kernal.Laplace4);
                        s.SaveImage(m, null);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.IdentifyAcousticEvents:     //make sonogram and detect shapes
                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        s = new Sonogram(iniFPath, wavPath);
                        //Console.WriteLine("sigAbsMax=" + s.State.SignalAbsMax + "  sigAvMax=" + s.State.SignalAvMax);

                        double[,] m = s.AmplitudM;
                        m = ImageTools.NoiseReduction(m);

                        //extract syllables from sonogram and calculate their distribution
                        //Color col = Color.DarkBlue;
                        Color col = Color.Red;
                        ArrayList syllables = ImageTools.Shapes5(m);
                        //calculate distribution of syllables over frequency columns 
                        int[] syllableDistribution = Shape.Distribution(syllables, Results.analysisBandCount);
                        //if (true) { s.SaveImage(m, syllables, col); Console.WriteLine("Finished Syllable Extraction"); break; }


                        //cluster the shapes using FuzzyART
                        int categoryCount;
                        double[,] data = Shape.FeatureMatrix(syllables); //derive data set from syllables
                        int[] categories = Shape.ClusterShapesWithFuzzyART(data, out categoryCount);
                        Console.WriteLine("Number of categories = " + categoryCount);
                        syllables = Shape.AssignCategories(syllables, categories);

                        //derive average shape of each category
                        ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
                        int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

                        //Console.WriteLine("Syllable count=" + DataTools.Sum(syllableDistribution) + "  Category count=" + DataTools.Sum(categoryDistribution));

                        s.SaveImage(m, syllables, col);
                        //s.SaveImageOfSolids(m, syllables, col);
                        //s.SaveImage(m, categoryAvShapes, col);
                        //s.SaveImageOfCentroids(m, categoryAvShapes, col);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\nFAILED TO EXTRACT SONOGRAM OR SUBSEQUENT STEP");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.CreateTemplate:  //extract template from sonogram

                    try
                    {
                        Console.WriteLine("\nCREATING TEMPLATE "+ callID);
                        Template t = new Template(iniFPath, callID, callName, callComment, sourceFile);
                        if (fv_Source == FV_Source.SELECTED_FRAMES)
                        {
                            t.SetSelectedFrames(selectedFrames);
                            t.SetFrequencyBounds(min_Freq, max_Freq);
                        }else
                        if (fv_Source == FV_Source.MARQUEE)
                        {
                            t.SetMarqueeBounds(min_Freq, max_Freq, marqueeStart, marqueeEnd);
                            if (fv_Extraction == FV_Extraction.AT_FIXED_INTERVALS) t.SetExtractionInterval(fvExtractionInterval);
                        }
                        t.SetSonogram(frameSize, frameOverlap, dynamicRange, filterBankCount, doMelConversion, ceptralCoeffCount,
                                                         deltaT, includeDeltaFeatures, includeDoubleDeltaFeatures);
                        t.SetExtractionParameters(fv_Source, fv_Extraction, doFvAveraging, fvDefaultNoiseFile);
                        t.SetSongParameters(maxSyllables, maxSyllableGap, typicalSongDuration);
                        t.SetLanguageModel(words, doHighSensitivitySearch);
                        t.SetScoringParameters(scoringProtocol, zScoreSmoothingWindow, zScoreThreshold, callPeriodicity);
                        t.ExtractTemplateFromSonogram();
                        t.WriteInfo2STDOUT();        //writes to System.Console.
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO CREATE TEMPLATE");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.CreateTemplateAndScan:

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try
                    {
                        Console.WriteLine("\nCREATING TEMPLATE " + callID);
                        Template t = new Template(iniFPath, callID, callName, callComment, sourceFile);
                        if (fv_Source == FV_Source.SELECTED_FRAMES)
                        {
                            t.SetSelectedFrames(selectedFrames);
                            t.SetFrequencyBounds(min_Freq, max_Freq);
                        }
                        else
                            if (fv_Source == FV_Source.MARQUEE)
                            {
                                t.SetMarqueeBounds(min_Freq, max_Freq, marqueeStart, marqueeEnd);
                                if (fv_Extraction == FV_Extraction.AT_FIXED_INTERVALS) t.SetExtractionInterval(fvExtractionInterval);
                            }
                        t.SetSonogram(frameSize, frameOverlap, dynamicRange, filterBankCount, doMelConversion, ceptralCoeffCount,
                                                         deltaT, includeDeltaFeatures, includeDoubleDeltaFeatures);
                        t.SetExtractionParameters(fv_Source, fv_Extraction, doFvAveraging, fvDefaultNoiseFile);
                        t.SetSongParameters(maxSyllables, maxSyllableGap, typicalSongDuration);
                        t.SetLanguageModel(words, doHighSensitivitySearch);
                        t.SetScoringParameters(scoringProtocol, zScoreSmoothingWindow, zScoreThreshold, callPeriodicity);
                        t.ExtractTemplateFromSonogram();
                        t.WriteInfo2STDOUT();        //writes to System.Console.
                        //t.Sonogram.SaveImage(t.Sonogram.AcousticM, null);

                        Console.WriteLine("\nCREATING CLASSIFIER");
                        //Classifier cl = new Classifier(t, t.Sonogram);
                        //double[,] m = t.Sonogram.SpectralM;
                        ////double[,] m = t.Sonogram.AcousticM;

                        //t.Sonogram.SaveImage(m, cl.Zscores);
                        Classifier cl = new Classifier(t);
                        t.Sonogram.SaveImage(t.Sonogram.SpectralM, cl.Zscores);
                        cl.WriteResults();
                        Console.WriteLine("# Template Hits =" + cl.Results.Hits);
                        Console.WriteLine("# Periodicity   =" + cl.Results.CallPeriodicity_ms + " ms");
                        Console.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
                        Console.WriteLine("# Best Score At =" + cl.Results.BestScoreLocation.ToString("F1") + " sec");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO CREATE TEMPLATE AND SCAN");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.ReadTemplateAndScan:

                    wavPath = wavDirName + "\\" + wavFileName + wavFExt;
                    try{
                        Console.WriteLine("\nREADING TEMPLATE " + callID);
                        Template t = new Template(iniFPath, callID);
                        Console.WriteLine("\nREADING WAV FILE");
                        t.SetSonogram(wavPath);
                        
                        Console.WriteLine("\nCREATING CLASSIFIER");
                        Classifier cl = new Classifier(t);
                        t.Sonogram.SaveImage(t.Sonogram.SpectralM, cl.Zscores);
                        cl.WriteResults();
                        Console.WriteLine("# Template Hits =" + cl.Results.Hits);
                        Console.WriteLine("# Periodicity   =" + cl.Results.CallPeriodicity_ms+" ms");
                        Console.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
                        Console.WriteLine("# Best Score At =" + cl.Results.BestScoreLocation.ToString("F1")+" sec");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case Mode.ScanMultipleRecordingsWithTemplate:
                    DirectoryInfo d = new DirectoryInfo(testDirName);
                    FileInfo[] files = d.GetFiles("*" + wavFExt);
                    ArrayList array = new ArrayList();
                    array.Add(Classifier.ResultsHeader());

                    try
                    {
                        Console.WriteLine("\nREADING TEMPLATE");
                        Template t = new Template(iniFPath, callID);
                        int count = 1;
                        foreach (FileInfo fi in files) if (fi.Extension == wavFExt)
                            {
                                string fName = fi.Name;
                                Console.WriteLine("\n##########################################");
                                Console.WriteLine("##### " + (count++) + " File=" + fName);
                                wavPath = testDirName + "\\" + fName;
                                try
                                {
                                    t.SetSonogram(wavPath);
                                    Classifier cl = new Classifier(t);
                                    t.Sonogram.SaveImage(t.Sonogram.SpectralM, cl.Zscores);
                                    Console.WriteLine("# Template Hits =" + cl.Results.Hits);
                                    Console.WriteLine("# Periodicity   =" + cl.Results.CallPeriodicity_ms + " ms");
                                    Console.WriteLine("# Periodic Hits =" + cl.Results.NumberOfPeriodicHits);
                                    //Console.WriteLine("# Best Score at =" + cl.Results.BestCallScore);
                                    Console.WriteLine("# Best Score At =" + cl.Results.BestScoreLocation.ToString("F1") + " sec");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("FAILED TO EXTRACT SONOGRAM");
                                    Console.WriteLine(e.ToString());
                                }

                            }//end all wav files
                    }//end try
                    catch (Exception e)
                    {
                        Console.WriteLine("UNCAUGHT ERROR!!");
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        string opPath = opDirName + "\\outputCall" + callID + ".txt";
                        FileTools.WriteTextFile(opPath, array);
                        Console.WriteLine("\n\n##### DATA WRITTEN TO FILE> " + opPath);
                    }
                    break;

                case Mode.AnalyseMultipleRecordings:
                    d = new DirectoryInfo(testDirName);
                    files = d.GetFiles("*" + wavFExt);  //FileInfo[] 
                    ArrayList lines = new ArrayList();
                    lines.Add(Results.AnalysisHeader());

                    try
                    {
                        int count = 1; //wav file counter
                        foreach (FileInfo fi in files) if (fi.Extension == wavFExt) //for all .wav files
                        {
                            string fName = fi.Name;
                            Console.WriteLine("\n##########################################");
                            Console.WriteLine("##### " + (count++) + " File=" + fName);
                            wavPath = testDirName + "\\" + fName;
                            s = new Sonogram(iniFPath, wavPath);
                            double[,] m = s.SpectralM;

                            //extract syllables from sonogram and calculate their distribution
                            Color col = Color.Black;
                            ArrayList syllables = ImageTools.Shapes5(m);
                            //calculate distribution of syllables over frequency columns 
                            int[] syllableDistribution = Shape.Distribution(syllables, Results.analysisBandCount);

                            //cluster the shapes using FuzzyART
                            int categoryCount = 0;
                            double[,] data = Shape.FeatureMatrix(syllables); //derive data set from syllables

                            int[] categories = Shape.ClusterShapesWithFuzzyART(data, out categoryCount);
                            Console.WriteLine("Number of categories = " + categoryCount);
                            syllables = Shape.AssignCategories(syllables, categories);

                            //derive average shape of each category
                            ArrayList categoryAvShapes = Shape.CategoryShapes(syllables, categories, categoryCount);
                            int[] categoryDistribution = Shape.Distribution(categoryAvShapes, Results.analysisBandCount);

                            s.SaveImage(m, syllables, col);

                            //Console.WriteLine("sigAbsMax=" + s.State.SignalAbsMax + "  sigAvMax=" + s.State.SignalAvMax);
                            //SignalAvMax  SignalAbsMax  syllableDistribution  categoryDistribution
                            lines.Add(s.OneLineResult(count, syllableDistribution, categoryDistribution, categoryCount));
                            count++;
                            //if (count == 10) break;
                        }//end all wav files
                    }//end try
                    catch (Exception e)
                    {
                        Console.WriteLine("UNCAUGHT ERROR!!");
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        string opPath = opDirName + "\\outputAnalysis" + callID + ".txt";
                        FileTools.WriteTextFile(opPath, lines);
                        Console.WriteLine("\n\n##### ANALYSIS DATA WRITTEN TO FILE> " + opPath);
                    }
                    break;

                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }// end switch

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();

        } //end Main




    }//end class Program
}
