using System;
using System.Collections.Generic;
using System.Text;
using TowseyLib;
using AudioTools;

namespace AudioStuff
{
	public class GUI
    {

        //****************** DEFAULT CALL PARAMETERS
        private string callName = "NO NAME";
        public string CallName { get { return callName; } }
        private string callComment = "DEFAULT COMMENT";
        public string CallComment { get { return callComment; } }

        private string destinationFileDescriptor = "Descriptor"; //should be short ie < 10 chars
        public string DestinationFileDescriptor { get { return destinationFileDescriptor; } }
        public string WavDirName;
        private string sourcePath = "NO_PATH";
        public string SourcePath { get { return sourcePath; } }
        private string sourceFile = "NO_NAME";
        public string SourceFile { get { return sourceFile; } }

        //ENERGY AND NOISE PARAMETERS
        private double dynamicRange = 30.0; //decibels above noise level #### YET TO DO THIS PROPERLY
        public double DynamicRange { get { return dynamicRange; } }
        //private backgroundFilter= //noise reduction??

        //MFCC PARAMETERS
        //int sampleRate; //determined by source WAV file
        private int frameSize = 512;
        public int FrameSize { get { return frameSize; } }
        private double frameOverlap = 0.5;
        public double FrameOverlap { get { return frameOverlap; } }
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
        private string selectedFrames = "0";
        public string SelectedFrames { get { return selectedFrames; } }
        private int min_Freq = 0; //Hz
        public int Min_Freq { get { return min_Freq; } }
        private int max_Freq = 9999; //Hz
        public int Max_Freq { get { return max_Freq; } }
        private int marqueeStart = 999;
        public int MarqueeStart { get { return marqueeStart; } }
        private int marqueeEnd = 999;
        public int MarqueeEnd { get { return marqueeEnd; } }

        // PARAMETERS FOR THE ACOUSTIC MODELS ***************
        private FV_Extraction fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS;  //AT_FIXED_INTERVALS
        public FV_Extraction Fv_Extraction { get { return fv_Extraction; } }
        private int fvExtractionInterval = 999; //milliseconds
        public int FvExtractionInterval { get { return fvExtractionInterval; } }
        private bool doFvAveraging = false;
        public bool DoFvAveraging { get { return doFvAveraging; } }
        private string fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";
        public string FvDefaultNoiseFile { get { return fvDefaultNoiseFile; } }
        private double zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
        public double ZScoreThreshold { get { return zScoreThreshold; } }

        //LANGUAGE MODEL
        private HMMType hmmType = HMMType.UNDEFINED; //the default hmm type  
        public HMMType HmmType { get { return hmmType; } }
        private string hmmName = "NO NAME ASSIGNED"; //default
        public string HmmName { get { return hmmName; } }

        private int numberOfWords = 0; //number of defined song variations 
        public int NumberOfWords { get { return numberOfWords; } }
        private string[] words = { "999" };
        public string[] Words  { get { return words; } }
        private int maxSyllables = 1;  //NOT YET USED
        public int MaxSyllables { get { return maxSyllables; } }
        private double maxSyllableGap = 0.25; //seconds  NOT YET USED
        public double MaxSyllableGap { get { return maxSyllableGap; } }
        private double songWindow = 1.000; //seconds USED TO CALCULATE SONG POISSON STATISTICS
        public double SongWindow { get { return songWindow; } }
        private int callPeriodicity = 999;
        public int CallPeriodicity { get { return callPeriodicity; } }

        public GUI(int callID, string wavDirName)
        {
            this.WavDirName = wavDirName;
                //************* CALL 1 PARAMETERS ***************
                if (callID == 1)
                {
                    callName = "Lewin's Rail Kek-kek";
                    callComment = "Template consists of a single KEK!";
                    destinationFileDescriptor = "Descriptor"; //should be short ie < 10 chars
                    wavDirName = @"C:\SensorNetworks\WavFiles\";
                    sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
					sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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

                    // PARAMETERS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1


                    //LANGUAGE MODEL
                    hmmType = HMMType.TWO_STATE_PERIODIC;
                    hmmName = "Lewin's Rail";
                    numberOfWords = 3; //number of defined song variations
                    words = new string[numberOfWords];
                    words[0] = "111"; words[1] = "11"; words[2] = "1";
                    //maxSyllables=
                    //double maxSyllableGap = 0.25; //seconds
                    //double maxSong=
                    callPeriodicity = 208;
                } //end of if (callID == 1)


                //******************************************************************************************************************
                //************* CALL 2 PARAMETERS ***************
                if (callID == 2)
                {
                    callName = "Lewin's Rail Kek-kek";
                    callComment = "Template consists of a single KEK!";
                    wavDirName = @"C:\SensorNetworks\WavFiles\";
                    sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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

                    //FEATURE VECTOR PREPARATION DETAILS
                    destinationFileDescriptor = "kek"; //should be short ie < 10 chars
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    selectedFrames = "1784,1828,1848,2113,2132,2152";
                    min_Freq = 1500; //Hz
                    max_Freq = 5500; //Hz
                    doFvAveraging = true;

                    // PARAMETERS FOR THE ACOUSTIC MODELS ***************
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";
                    zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    hmmType = HMMType.TWO_STATE_PERIODIC;
                    hmmName = callName;
                    //numberOfWords = 3; //number of defined song variations 
                    //words = new string[numberOfWords];
                    //words[0] = "111"; words[1] = "11"; words[2] = "1";
                    callPeriodicity = 208;
                }//end of if (callID == 2)


                //******************************************************************************************************************
                //************* CALL 3 PARAMETERS ***************
                if (callID == 3)
                {
                    callName = "Soulful-tuneful";
                    callComment = "Unknown species in faint kek-kek file!";
                    destinationFileDescriptor = "syll5Av"; //should be short ie < 10 chars
                    wavDirName = @"C:\SensorNetworks\WavFiles\";
                    sourceFile = "BAC1_20071008-084607";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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
                    //                selectedFrames = "337,376,413,1161,1197,2110,3288,3331,4767"; //syllable 1 frames
                    //                selectedFrames = "433,437,446,450,1217,1222,1229,1234,3355,3359,3372"; //syllable 2 frames
                    selectedFrames = "496,1281,2196,3418,4852"; //syllable 5 frames
                    min_Freq = 600; //Hz
                    max_Freq = 3700; //Hz
                    //fv_Source = FV_Source.MARQUEE;  //options are SELECTED_FRAMES or MARQUEE
                    //marqueeStart = 4760;  //frame id
                    //marqueeEnd   = 4870;
                    doFvAveraging = true;

                    //fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    hmmType = HMMType.ERGODIC;
                    hmmName = callName;
                } //end of if (callID == 3)


                //******************************************************************************************************************
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


                //******************************************************************************************************************
                //************* CALL 5 PARAMETERS ***************
                if (callID == 5)
                {

                    callName = "Cricket";
                    callComment = "High freq warble";
                    destinationFileDescriptor = "Descriptor"; //should be short ie < 10 chars
                    wavDirName = @"C:\SensorNetworks\WavFiles\";
                    sourceFile = "BAC2_20071008-085040";  //Lewin's rail kek keks.
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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


                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.MARQUEE;  //options are SELECTED_FRAMES or MARQUEE
                    min_Freq = 7000; //Hz
                    max_Freq = 9000; //Hz
                    marqueeStart = 1555;  //frame id
                    marqueeEnd = 1667;

                    fv_Extraction = FV_Extraction.AT_FIXED_INTERVALS;  //AT_ENERGY_PEAKS or AT_FIXED_INTERVALS
                    fvExtractionInterval = 200; //milliseconds
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    hmmType = HMMType.ERGODIC;
                    hmmName = callName;
                    //numberOfWords = 3; //number of defined song variations
                    //words = new string[numberOfWords];
                    //words[0] = "1"; words[1] = "2"; words[2] = "3";
                    //maxSyllables=
                    //double maxSyllableGap = 0.25; //seconds
                }//end of if (callID == 5)


                //******************************************************************************************************************
                //******************************************************************************************************************
                //************* CALL 6 PARAMETERS ***************
                if (callID == 6)
                {
                    callName = "Koala Bellow";
                    //callComment = "Presumed exhalation snort of a koala bellow!";
                    //callComment = "Presumed inhalation/huff of a koala bellow!";
                    callComment = "Additional bellow syllable 3!";
                    destinationFileDescriptor = "syl3"; //should be short ie < 10 chars
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_-_St_Bees_KoalaBellow20080919-073000";  //Koala Bellows
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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

                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    //selectedFrames = "826,994,1140,1156,1469,1915,2103,2287,2676,3137,4314,4604";  //frames for PUFF
                    //selectedFrames = "595,640,752,897,957,1092,1691,1840,2061,2241,2604,4247";   //frames for HUFF
                    selectedFrames = "39,51,66,80,93,134,294";  //frames for SYLLABLE3
                    //selectedFrames = "10051,10092,10106,10080";  //frames for DISTANT BELLOW
                    min_Freq = 200; //Hz
                    max_Freq = 3000; //Hz
                    doFvAveraging = true;

                    // THE ACOUSTIC MODEL ***************
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";
                    zScoreThreshold = 1.4; //keep this as initial default. Later options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    //LANGUAGE MODEL
                    hmmType = HMMType.ERGODIC;
                    hmmName = callName;
                    //numberOfWords = 3; //number of defined song variations
                    //words = new string[numberOfWords];
                    //words[0] = "111"; words[1] = "11"; words[2] = "1";
                } //end of if (callID == 6)



                //******************************************************************************************************************
                //******************************************************************************************************************
                //************* CALL 7 PARAMETERS ***************
                if (callID == 7)
                {
                    callName = "Fruit bat";
                    callComment = "Single fruit bat chirps";
                    destinationFileDescriptor = "bat1"; //should be short ie < 10 chars
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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
                    selectedFrames = "1112,1134,1148,1167,1172,1180,1184,1188,1196"; //
                    min_Freq = 1000; //Hz
                    max_Freq = 7000; //Hz
                    //fv_Source = FV_Source.MARQUEE;  //options are SELECTED_FRAMES or MARQUEE
                    //marqueeStart = 4760;  //frame id
                    //marqueeEnd   = 4870;
                    //doFvAveraging = true;

                    //fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 4.0; //options are 1.98, 2.33, 2.56, 3.1, 3.3

                    //LANGUAGE MODEL
                    hmmType = HMMType.ERGODIC;
                    hmmName = callName;
                    songWindow = 2.0; //seconds
                } //end of if (callID == 7)

                //******************************************************************************************************************
                //************* CALL 8 PARAMETERS ***************
                if (callID == 8)
                {
                    callName = "Currawong";
                    callComment = "From St Bees";
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_St_Bees_Currawong3_20080919-060000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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
                    destinationFileDescriptor = "syll1"; //should be short ie < 10 chars
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    selectedFrames = "4753,5403,6029,6172,6650,6701,6866,9027";          //syllable 1 frames
                    //selectedFrames = "4758,5408,6034,6175,6655,6704,6871,9030"; //syllable 2 frames
                    //selectedFrames = "4762,5412,6039,6178,6659,6707,6875,9033"; //syllable 3 frames
                    //selectedFrames = "4766,5416,6043,6183,6664,6712,6880,9037"; //syllable 4 frames
                    min_Freq = 1000; //Hz
                    max_Freq = 8000; //Hz
                    doFvAveraging = true;

                    //fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 8.0; //options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    hmmType = HMMType.UNDEFINED;
                    hmmName = callName;
                    songWindow = 0.8; //seconds
                } //end of if (callID == 8)


                //******************************************************************************************************************
                //************* CALL 9 PARAMETERS ***************
                if (callID == 9)
                {
                    callName = "Curlew";
                    callComment = "From St Bees";
                    destinationFileDescriptor = "syll6"; //should be short ie < 10 chars
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "Honeymoon_Bay_St_Bees_Curlew3_20080914-003000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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


                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    //selectedFrames = "6881,7041,7179,7276"; //syllable 1 frames
                    //selectedFrames = "6858,6901,7015,7156,7258"; //syllable 2 frames
                    //selectedFrames = "7051,7186,7282"; //syllable 3 frames
                    //selectedFrames = "7352,7416,7471,7540"; //syllable 4 frames
                    //selectedFrames = "7334,7400,7456,7522"; //syllable 5 frames
                    selectedFrames = "7357,7420,7475,7544"; //syllable 6 frames
                    min_Freq = 1000; //Hz
                    max_Freq = 9000; //Hz
                    doFvAveraging = true;

                    //fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 4.0; //options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    hmmType = HMMType.ERGODIC;
                    hmmName = callName;
                    songWindow = 0.8; //seconds
                } //end of if (callID == 9)


            //******************************************************************************************************************
                //************* CALL 10 PARAMETERS ***************
                if (callID == 10)
                {
                    callName = "Rainbow Lorikeet";
                    callComment = "From St Bees";
                    destinationFileDescriptor = "syll1"; //should be short ie < 10 chars
                    wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
                    sourceFile = "West_Knoll_St_Bees_RainbowLorikeet1_20080918-080000";
                    sourcePath = wavDirName + sourceFile + WavReader.WavFileExtension;

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


                    //FEATURE VECTOR EXTRACTION PARAMETERS
                    fv_Source = FV_Source.SELECTED_FRAMES;  //options are SELECTED_FRAMES or MARQUEE
                    selectedFrames = "813,896,923,956,1048,1108,1140"; //
                    min_Freq = 1000; //Hz
                    max_Freq = 9000; //Hz
                    doFvAveraging = true;

                    //fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS; // AT_FIXED_INTERVALS;
                    fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";

                    // THRESHOLDS FOR THE ACOUSTIC MODELS ***************
                    zScoreThreshold = 4.0; //options are 1.98, 2.33, 2.56, 3.1

                    //LANGUAGE MODEL
                    hmmType = HMMType.ERGODIC;
                    hmmName = callName;
                    songWindow = 0.25; //seconds
                } //end of if (callID == 10)





            }//end CONSTRUCTOR
    }//end class

}
