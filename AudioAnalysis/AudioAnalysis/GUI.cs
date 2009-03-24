using System;
using System.Collections.Generic;
using System.Text;
using TowseyLib;
using AudioTools;

namespace AudioAnalysis
{
	public class GUI
    {

        //can reset output directory from default
        public string opDir { get; private set; }

        #region DEFAULT CALL PARAMETERS
        //****************** DEFAULT CALL PARAMETERS
        private string authorName = "NO NAME";
        public string AuthorName { get { return authorName; } }
        private int callID = -1; //invalid default value!
        public int CallID { get { return callID; } }
        private string callName = "NO NAME";
        public string CallName { get { return callName; } }
        private string callComment = "DEFAULT COMMENT";
        public string CallComment { get { return callComment; } }

        public string WavDirName;
        private string sourcePath = "NO_PATH";
        public string SourcePath { get { return sourcePath; } }
        private string sourceFile = "NO_NAME";
        public string SourceFile { get { return sourceFile; } }

        //ENERGY AND NOISE PARAMETERS
        private double dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
        public double DynamicRange { get { return dynamicRange; } }
        //private backgroundFilter= //noise reduction??

        //FRAMING PARAMETERS
        private int frameSize = 512;
        public int FrameSize { get { return frameSize; } }
        private double frameOverlap = 0.5;
        public double FrameOverlap { get { return frameOverlap; } }

        //FEATURE PARAMETERS
        //int sampleRate; //determined by source WAV file
        private Feature_Type featureType = Feature_Type.UNDEFINED;   //the default feature type  
        public Feature_Type FeatureType { get { return featureType; } }

        private int filterBankCount = 64;
        public int FilterBankCount { get { return filterBankCount; } }
        private bool doMelConversion = true;
        public bool DoMelConversion { get { return doMelConversion; } }
        private bool doNoiseReduction = false;
        public bool DoNoiseReduction { get { return doNoiseReduction; } }
        private int ceptralCoeffCount = 12;
        public int CeptralCoeffCount { get { return ceptralCoeffCount; } }
        private bool includeDeltaFeatures = true;
        public bool IncludeDeltaFeatures { get { return includeDeltaFeatures; } }
        private bool includeDoubleDeltaFeatures = true;
        public bool IncludeDoubleDeltaFeatures { get { return includeDoubleDeltaFeatures; } }
        private int deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
        public int DeltaT { get { return deltaT; } }

            //FEATURE VECTOR EXTRACTION PARAMETERS
        private FV_Source fv_Source = FV_Source.SELECTED_FRAMES;  //FV_Source.MARQUEE;
        public  FV_Source Fv_Source  { get { return fv_Source; } }
        private FV_MarqueeType fv_Extraction = FV_MarqueeType.AT_ENERGY_PEAKS;  //AT_FIXED_INTERVALS
        public FV_MarqueeType Fv_Extraction { get { return fv_Extraction; } }
        private int fvExtractionInterval = 999; //milliseconds
        public int FvExtractionInterval { get { return fvExtractionInterval; } }
        //private bool doFvAveraging = false;
        //public bool DoFvAveraging { get { return doFvAveraging; } }
        //private string selectedFrames = "0";
        //public string SelectedFrames { get { return selectedFrames; } }
        private string[,] fvInit = null;
        public string[,] FvInit { get { return fvInit; } }

        private int min_Freq = 0; //Hz
        public int Min_Freq { get { return min_Freq; } }
        private int max_Freq = 9999; //Hz
        public int Max_Freq { get { return max_Freq; } }
        private int marqueeStart = 999;
        public int MarqueeStart { get { return marqueeStart; } }
        private int marqueeEnd = 999;
        public int MarqueeEnd { get { return marqueeEnd; } }

        // PARAMETERS FOR THE ACOUSTIC MODELS ***************
        private string fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\DefaultNoise.txt";
        public string FvDefaultNoiseFile { get { return fvDefaultNoiseFile; } }
        private double zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
        public double ZScoreThreshold { get { return zScoreThreshold; } }

        //LANGUAGE MODEL
        private ModelType modelType = ModelType.UNDEFINED;   //the default hmm type  
        public ModelType ModelType { get { return modelType; } }
        private int numberOfWords = 0; //number of defined song variations 
        public  int NumberOfWords { get { return numberOfWords; } }
        private string[] wordNames = { "noname" };
        public  string[] WordNames  { get { return wordNames; } }
        private string[] wordExamples = { "noexample" };
        public  string[] WordExamples  { get { return wordExamples; } }
        private int maxSyllables = 1;  //NOT YET USED
        public  int MaxSyllables { get { return maxSyllables; } }
        private double maxSyllableGap = 0.25; //seconds  NOT YET USED
        public  double MaxSyllableGap { get { return maxSyllableGap; } }
        private double songWindow = 1.000; //seconds USED TO CALCULATE SONG POISSON STATISTICS
        public  double SongWindow { get { return songWindow; } }
        private int callPeriodicity = 999;
        public  int CallPeriodicity { get { return callPeriodicity; } }
        #endregion


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="wavDirName"></param>
        public GUI(int callID, string wavDirName)
        {
            this.callID = callID;
            this.opDir = opDir;
            
            #region CALL 1 PARAMETERS - FOR FUNCTIONAL TEST
            //************* CALL 1 PARAMETERS ***************
                if (callID == 1)
                {
                    authorName = "Michael Towsey";
                    callName = "FUNCTIONAL_TEST";
                    callComment = "FUNCITONAL TEST DERIVED FROM CURRAWONG RECORDING AT ST BEES - TEMPLATE #8";
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_St_Bees_Currawong3_20080919-060000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_1\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = true;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    deltaT = 3; // i.e. + and - three frames gap when constructing feature vector

                    min_Freq = 1000; //Hz
                    max_Freq = 8000; //Hz

                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    fvInit = new string[,] {
                        {"template1_syl1","4753,5403,6029,6172,6650,6701,6866,9027"},
                        {"template1_syl2","4758,5408,6034,6175,6655,6704,6871,9030"},
                        {"template1_syl3","4762,5412,6039,6178,6659,6707,6875,9033"},
                    //    {"template1_syl4","4766,5416,6043,6183,6664,6712,6880,9037"},
                    };



                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 7.0; //options are 1.98, 2.33, 2.56, 3.1
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\Template_1\template1_DefaultNoise.txt";

                    //LANGUAGE MODEL
                    //modelType = ModelType.UNDEFINED;
                    modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 1; //number of defined song variations
                    wordNames = new String[] { "syl1", "syl2", "syl3" };
                    //wordNames = new String[] { "syl1", "syl2", "syl3", "syl4" };
                    wordExamples = new String[] { "111", "11", "1" };
                } //end of if (callID == 1)
                #endregion

                //******************************************************************************************************************
                #region CALL 2 PARAMETERS
                //************* CALL 2 PARAMETERS ***************
                if (callID == 2)
                {
                    authorName = "Michael Towsey";
                    callName = "Lewin's Rail Kek-kek";
                    callComment = "Template consists of a single KEK!";
                    wavDirName = @"C:\SensorNetworks\WavFiles\";
                    sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_2\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = false;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
                    //dynamicRange = 30.0; //decibels above noise level #### YET TO TO DO THIS PROPERLY

                    min_Freq = 1500; //Hz
                    max_Freq = 5500; //Hz

                    //FEATURE VECTOR PREPARATION DETAILS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    fvInit = new string[,] {
                        {"FV_template2_kek.txt","1784,1828,1848,2113,2132,2152"}
                    };


                    // PARAMETERS FOR THE ACOUSTIC MODELS ***************
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";
                    zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    //modelType = ModelType.UNDEFINED;
                    modelType = ModelType.ONE_PERIODIC_SYLLABLE;
                    numberOfWords = 1; //number of defined song variations
                    wordNames    = new String[] { "kek" };
                    wordExamples = new String[] {"111", "11", "1"};
                    callPeriodicity = 208;
                }//end of if (callID == 2)
                #endregion

                //******************************************************************************************************************
                #region CALL 3 PARAMETERS
                //************* CALL 3 PARAMETERS ***************
                if (callID == 3)
                {
                    authorName = "Michael Towsey";
                    callName = "Soulful-tuneful";
                    callComment = "Unknown species in faint kek-kek file!";
                    wavDirName = @"C:\SensorNetworks\WavFiles\";
                    sourceFile = "BAC1_20071008-084607";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_3\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = true;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    deltaT = 3; // i.e. + and - three frames gap when constructing feature vector


                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    fvInit = new string[,] {
                        {"template3_syl1","337,376,413,1161,1197,2110,3288,3331,4767"},
                        {"template3_syl2","433,437,446,450,1217,1222,1229,1234,3355,3359,3372"},
                        {"template3_syl3","496,1281,2196,3418,4852"},
                    };

                    min_Freq = 600; //Hz
                    max_Freq = 3700; //Hz

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    //LANGUAGE MODEL
                    modelType = ModelType.UNDEFINED;
                    //modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 1; //number of defined song variations
                    wordNames = new String[] { "name" };
                    wordExamples = new String[] { "111", "11", "1" };
                } //end of if (callID == 3)
                #endregion

                //******************************************************************************************************************
                #region CALL 4 PARAMETERS
                //************* CALL 4 PARAMETERS ***************
                //coordinates to extract template using bitmap image of sonogram
                //image coordinates: rows=freqBins; cols=timeSteps
                if (callID == 4)
                {
                    Console.WriteLine("DATE AND TIME:" + DateTime.Now);
                    Console.WriteLine("ABORT!!  CAN ONLY READ TEMPLATE 4! CANNOT CREATE IT.");
                    Console.WriteLine("\t\tPRESS ANY KEY TO EXIT");
                    Console.ReadLine();
                    System.Environment.Exit(999);
                }
                #endregion

                //******************************************************************************************************************
                #region CALL 5 PARAMETERS
                //************* CALL 5 PARAMETERS ***************
                if (callID == 5)
                {

                    authorName = "Michael Towsey";
                    callName = "Cricket";
                    callComment = "High freq warble";
                    wavDirName = @"C:\SensorNetworks\WavFiles\";
                    sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_5\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = false;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;

                    min_Freq = 7000; //Hz
                    max_Freq = 9000; //Hz
                    
                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    fv_Extraction = FV_MarqueeType.AT_FIXED_INTERVALS;  //AT_ENERGY_PEAKS or AT_FIXED_INTERVALS
                    fvExtractionInterval = 200; //milliseconds
                    //marquee = from 1555 To 1667;
                    fvInit = new string[,] {
                        {"template5_syl1","1555"},
                        {"template5_syl2","1580"},
                        {"template5_syl3","1600"},
                        {"template5_syl4","1630"},
                        {"template5_syl5","1667"},
                    };


                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    //LANGUAGE MODEL
                    modelType = ModelType.UNDEFINED;
                    //modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 1; //number of defined song variations
                    wordNames = new String[] { "name" };
                    wordExamples = new String[] { "111", "11", "1" };
                }//end of if (callID == 5)
                #endregion

                //******************************************************************************************************************
                #region CALL 6 PARAMETERS
                //************* CALL 6 PARAMETERS ***************
                if (callID == 6)
                {
                    authorName = "Michael Towsey";
                    callName = "Koala Bellow";
                    callComment = "Various sounds - huff, puff and snort!";
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_-_St_Bees_KoalaBellow20080919-073000";  //Koala Bellows
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_6\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = true;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    min_Freq = 200; //Hz
                    max_Freq = 3000; //Hz

                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES
                    fvInit = new string[,] {
                        {"template6_puff","826,994,1140,1156,1449,1620,1914,2018,2103,2287,2676,2867,3020,3137,3291,3667,3970,4314,4604"},
                        {"template6_huff","595,640,660,752,790,897,901,957,1055,1110,1325,1520,1590,1661,1691,1964,1724,1840,2061,2198,2241,2604,4247"},
              //          {"template6_warmup","39,51,66,80,93,134,294"},
              //          {"template6_distant","9993,10034,10051,10092,10106,10080,10196"},
                    };
                    //doFvAveraging = true;

                    // THE ACOUSTIC MODEL ***************
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";
                    zScoreThreshold = 1.0; //keep this as initial default. Later options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 1; //number of defined song variations
                    //wordNames = new String[] { "huff","puff", "snort", "distant" };
                    wordNames = new String[] { "huffNpuff" };
                    wordExamples = new String[] { "111", "11", "1" };
                } //end of if (callID == 6)
                #endregion

                //******************************************************************************************************************
                #region CALL 7 PARAMETERS
                //************* CALL 7 PARAMETERS ***************
                if (callID == 7)
                {
                    authorName = "Michael Towsey";
                    callName = "Fruit bat";
                    callComment = "Single fruit bat chirps";
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_7\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = true;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    deltaT = 3; // i.e. + and - three frames gap when constructing feature vector
                    min_Freq = 1000; //Hz
                    max_Freq = 7000; //Hz

                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    fvInit = new string[,] {
                        {"template7_bat1","1112,1134,1148,1167,1172,1180,1184,1188,1196"}
                    };

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 4.0; //options are 1.98, 2.33, 2.56, 3.1, 3.3
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    //LANGUAGE MODEL
                    modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 1; //number of defined song variations
                    wordNames = new String[] { "squeak" };
                    wordExamples = new String[] { "111", "11", "1" };
                    songWindow = 2.0; //seconds
                } //end of if (callID == 7)
                #endregion

                //******************************************************************************************************************
                #region CALL 8 PARAMETERS
                //************* CALL 8 PARAMETERS ***************
                if (callID == 8)
                {
                    authorName = "Michael Towsey";
                    callName = "Currawong";
                    callComment = "From St Bees";
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_St_Bees_Currawong3_20080919-060000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_8\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = true;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    deltaT = 3; // i.e. + and - three frames gap when constructing feature vector

                    min_Freq = 1000; //Hz
                    max_Freq = 8000; //Hz

                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    fvInit = new string[,] {
                        {"template8_syl1","4753,5403,6029,6172,6650,6701,6866,9027"},
                        {"template8_syl2","4758,5408,6034,6175,6655,6704,6871,9030"},
                        {"template8_syl3","4762,5412,6039,6178,6659,6707,6875,9033"},
                        {"template8_syl4","4766,5416,6043,6183,6664,6712,6880,9037"},
                    };



                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 8.0; //options are 1.98, 2.33, 2.56, 3.1
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    //LANGUAGE MODEL
                    //modelType = ModelType.UNDEFINED;
                    modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 4; //number of defined song variations
                    wordNames = new String[] { "syll1", "syll2", "syll3", "syll4" };
                    wordExamples = new String[] { "111", "11", "1" };
                } //end of if (callID == 8)
                #endregion

                //******************************************************************************************************************
                #region CALL 9 PARAMETERS
                //************* CALL 9 PARAMETERS ***************
                if (callID == 9)
                {
                    authorName = "Michael Towsey";
                    callName = "Curlew";
                    callComment = "From St Bees";
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "Honeymoon_Bay_St_Bees_Curlew3_20080914-003000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_9\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = false;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    deltaT = 3; // i.e. + and - three frames gap when constructing feature vector

                    min_Freq = 1000; //Hz
                    max_Freq = 9000; //Hz

                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    //doFvAveraging = true;
                    fvInit = new string[,] {
                        {"template9_syl1","6881,7041,7179,7276"},
                        {"template9_syl2","6858,6901,7015,7156,7258"},
                        {"template9_syl3","7051,7186,7282"},
                        {"template9_syl4","7352,7416,7471,7540"},
                        {"template9_syl5","7334,7400,7456,7522"},
                        {"template9_syl6","7357,7420,7475,7544"},
                    };

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 4.0; //options are 1.98, 2.33, 2.56, 3.1
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    //LANGUAGE MODEL
                    modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 4; //number of defined song variations
                    wordNames = new String[] { "syll1", "syll2", "syll3", "syll4", "syll5", "syll6" };
                    wordExamples = new String[] { "111", "11", "1" };
                    songWindow = 0.8; //seconds
                } //end of if (callID == 9)
                #endregion

                //******************************************************************************************************************
                #region CALL 10 PARAMETERS
                //************* CALL 10 PARAMETERS ***************
                if (callID == 10)
                {
                    authorName = "Michael Towsey";
                    callName = "Rainbow Lorikeet";
                    callComment = "From St Bees";
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_St_Bees_RainbowLorikeet1_20080918-080000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;
                    opDir = @"C:\SensorNetworks\Templates\Template_10\";

                    //MFCC PARAMETERS
                    frameSize = 512;
                    frameOverlap = 0.5;
                    filterBankCount = 64;
                    doMelConversion = false;
                    doNoiseReduction = false;
                    ceptralCoeffCount = 12;
                    includeDeltaFeatures = true;
                    includeDoubleDeltaFeatures = true;
                    deltaT = 3; // i.e. + and - three frames gap when constructing feature vector

                    min_Freq = 1000; //Hz
                    max_Freq = 9000; //Hz

                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    //doFvAveraging = true;
                    fvInit = new string[,] {
                        {"template10_syl1","813,896,923,956,1048,1108,1140"}
                    };

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 4.0; //options are 1.98, 2.33, 2.56, 3.1
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    //LANGUAGE MODEL
                    modelType = ModelType.MM_ERGODIC;
                    numberOfWords = 1; //number of defined song variations
                    wordNames = new String[] { "squawk" };
                    wordExamples = new String[] { "111", "11", "1" };
                    songWindow = 0.25; //seconds
                } //end of if (callID == 10)
                #endregion
            
            #region CALL 11 PARAMETERS
            #endregion




        }//end CONSTRUCTOR
    }//end class

}
