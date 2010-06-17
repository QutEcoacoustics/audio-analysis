using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TowseyLib;

namespace AudioAnalysisTools.HTKTools
{
    public class HTKConfig
    {

        public static string Key_CALL_NAME     = "CALL_NAME";
        public static string Key_AUTHOR        = "AUTHOR";
        public static string Key_COMMENT       = "COMMENT";
        public static string Key_FRAME_SIZE    = "FRAME_SIZE";
        public static string Key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string Key_SAMPLE_RATE   = "SAMPLE_RATE";
        public static string Key_MIN_FREQ      = "MIN_FREQ";
        public static string Key_MAX_FREQ      = "MAX_FREQ";
        public static string Key_HTK_THRESHOLD = "HTK_THRESHOLD";
        public static string Key_DURATION_MEAN = "DURATION_MEAN";
        public static string Key_DURATION_SD   = "DURATION_SD";
        public static string Key_SD_THRESHOLD  = "SD_THRESHOLD";

        //###########################################################################################################################
        // MAJOR STRUCTURAL TRAINING PARAMETERS -- DEFAULT VALUES
        // SET VALUES In CONSTRUCTOR
        public bool multisyllabic   = false;   //default: single syllable call
        //public bool multisyllabic = true;    //Parse the grammar file: creates the word network file 'htkConfig.wordNet'
        public bool useBKGModel     = true;        //true -> use BKG model;    false -> use SIL model
        public bool LLRNormalization = true;   //perform LLR normalization on the output from HVite
        public bool noSegmentation  = false;    //true -> each recording is considered as single word. No SIL model is generated/trained from the recordings.
        public string UseHERest;     //y -> use HERest i.e. use my labels, use HTK timestamps; n -> use HRest i.e. use my labels AND my timestamps 
        public string UseHERestBKG;  //train BACKGROUND model
        //false-> each recording segmented. SIL model is generated/trained from the traiing recordings.
        //###########################################################################################################################


        public string Author      { get; private set; }
        public string CallName    { get; set; }
        public string BkgName     { get; private set; }
        public string SilName     { get; private set; }
        public string Comment     { get; private set; }

        public string WorkingDir   { get; set; }
        public string BkgWorkingDir{ get; set; }
        public string DataDir      { get; set; }
        public string BkgDataDir   { get; set; }
        public string ConfigDir    { get; set; }
        public string BkgConfigDir { get; set; }
        public string HTKDir       { get; set; }
        public string SegmentationDir { get; set; }
        public string ResultsDir   { get; set; }

        
        //PARAMETERS FOR TRAINING OF HMM MODEL & BACKGROUND MODEL 
        public string numHmmStates { get; set; }
        public string numHmmSilStates { get; set; }
        public string numHmmBkgStates { get; set; }
        public int    numIterations { get; set; }
        public int    numBkgIterations { get; set; }
        public bool   bkgTraining = false;
        
        public string SampleRate  { get; set; }
        public string FrameOverlap { get; set; }
        public string MinHz { get; set; }
        public string MaxHz { get; set; }
        public string SilenceModelSrc { get; set; }  // source of the silence .wav recording
        public string SilenceModelPath { get; set; } // the silence .wav recording
        public string NoiseModelFN { get; set; }     // noise model is derived from the silence .wav recording

        //Following parameters names used by HTK in the MFCC file.
        public string SOURCEFORMAT { get; set; }
        public string TARGETKIND   { get; set; }
        public string TARGETRATE { get; set; }
        public string SAVECOMPRESSED { get; set; }
        public string SAVEWITHCRC { get; set; }
        public string WINDOWDURATION { get; set; } //measured in 1/10s of nano-second i.e. 10-7 seconds
        public string FRAMESIZE { get; set; }
        public string USEHAMMING { get; set; }
        public string PREEMCOEF { get; set; }
        public string NUMCHANS { get; set; }
        public string CEPLIFTER { get; set; }
        public string NUMCEPS { get; set; }
        public string LOFREQ { get; set; }
        public string HIFREQ { get; set; }
        public int VecSize { get; set; }
        //Statistical Data: duration mean and variation of vocalizations in the training set
        public Dictionary<string, double> meanDuration = new Dictionary<string, double>();
        public Dictionary<string, double> varianceDuration = new Dictionary<string, double>();
        public Dictionary<string, int> threshold = new Dictionary<string, int>();
        

        /// <summary>
        /// Table used for computing feature vectors size 
        /// </summary>
        private Dictionary<string, int> smFeatureDict = new Dictionary<string, int>();

        //file names
        public static string segmentationIniFN = "segmentation.ini";
        public static string segmentationExe   = "VocalSegmentation.exe";
        public static string mfccConfigFN      = "mfccConfig.txt";
        public static string labelListFN       = "labelList.txt";

        //dir and path names
        public string trnDirPath       { get { return DataDir       + "\\train"; } }
        public string trnDirPathBkg    { get { return BkgDataDir ; } }
        public string tstFalseDirPath  { get { return DataDir       + "\\test\\testFalse"; } }
        public string tstTrueDirPath   { get { return DataDir       + "\\test\\testTrue"; } }

        public string ProtoConfDir     { get { return ConfigDir     + "\\protoConfigs"; } }
        public string ProtoConfDirBkg  { get { return BkgConfigDir  + "\\protoConfigs"; } }
        public string ConfigFN         { get { return ConfigDir     + "\\monPlainM1S1.dcf"; } }
        public string MfccConfigFN     { get { return ConfigDir     + "\\mfccConfig"; } }
        public string MfccConfigFNBkg  { get { return BkgConfigDir  + "\\mfccConfig"; } }
        public string MfccConfig2FN    { get { return ConfigDir     + "\\" + mfccConfigFN; } }   //need this copy for training
        public string MfccConfig2FNBkg { get { return BkgConfigDir + "\\mfccConfig.txt"; } }

        //grammar file for multisyllabic calls
        public string grammarF { get { return ConfigDir + "\\gram.bnf"; } }
        //syllable list for multisyllabic calls
        public List<string> multiSyllableList = new List<string>();

        public string DictFile     { get { return ConfigDir    + "\\dict"; } }
        public string DictFileBkg  { get { return BkgConfigDir + "\\dict"; } }
        public string cTrainF      { get { return ConfigDir    + "\\codetrain.scp"; } }
        public string cTrainFBkg   { get { return BkgConfigDir + "\\codetrain.scp"; } }
        public string cTestFalseF  { get { return ConfigDir    + "\\codetestfalse.scp"; } }
        public string cTestTrueF   { get { return ConfigDir    + "\\codetesttrue.scp"; } }
        public string trainF       { get { return ConfigDir    + "\\train.scp"; } }
        public string trainFBkg    { get { return BkgConfigDir + "\\train.scp"; } }
        public string tFalseF      { get { return ConfigDir    + "\\testfalse.scp"; } }
        public string tTrueF       { get { return ConfigDir    + "\\testtrue.scp"; } }        
        public string wltF         { get { return ConfigDir    + "\\phones.mlf";  } }//file containing segmentation info into SONG SYLLABLES + SILENCE
        public string wltFBkg      { get { return BkgConfigDir + "\\phones.mlf"; } }
        public string wordNet      { get { return ConfigDir    + "\\phone.net"; } }
        public string wordNetBkg   { get { return BkgConfigDir + "\\phone.net"; } }
        //for scanning a single test file
        public string TestFileCode { get { return ConfigDir + "\\Test_CodeSingle.scp"; } }
        public string TestFile     { get { return ConfigDir + "\\Test_Single.scp"; } }

        //lists directory
        //public string ListsDir   { get { return ConfigDir + "\\lists"; } }
        public string monophones   { get { return ConfigDir + "\\" + labelListFN; } } //contains list of syllables to recognise including SIL
        public string monophonesBkg   { get { return BkgConfigDir + "\\" + labelListFN; } }

        //HMM files
        public string HmmDir       { get { return ConfigDir + "\\hmms"; } }
        public string HmmDirBkg    { get { return BkgConfigDir + "\\hmms"; } }
        public string HmmDirBkgLLR { get { return "\\hmmBKG"; } }
        public string tgtDir0      { get { return HmmDir + "\\hmm.0"; } }
        public string tgtDir0Bkg   { get { return HmmDirBkg + "\\hmm.0"; } }
        public string tgtDir1      { get { return HmmDir + "\\hmm.1"; }}
        public string tgtDir1Bkg   { get { return HmmDirBkg + "\\hmm.1"; } }
        public string tgtDir2      { get { return HmmDir + "\\hmm.2"; }}
        public string tgtDir2Bkg   { get { return HmmDirBkg + "\\hmm.2"; } }
        public string tgtDirTmp    { get { return HmmDir + "\\tmp"; } }
        public string tgtDirTmpBkg { get { return HmmDirBkg + "\\tmp"; } }
        public string macrosFN        = "macros";
        public string hmmdefFN        = "hmmdefs";
        public string protoFN         = "proto"; //CANNOT CHANGE THIS NAME !!??
        public string vFloorsFN       = "vFloors";
        public string prototypeHMM    { get { return ConfigDir    + "\\" + protoFN; } }
        public string prototypeHMMBkg { get { return BkgConfigDir + "\\" + protoFN; } }

        //results files
        public string resultTrue  { get { return ResultsDir + "\\recountTrue.mlf";} }
        public string resultFalse { get { return ResultsDir + "\\recountFalse.mlf"; } }
        public string resultTest  { get { return ResultsDir + "\\TestScan.mlf"; } } //for scanning a single file

        //script files for scoring the BACKGROUND model
        public string audioSegmTrue { get { return ConfigDir + "\\audioSegmTrue.scp"; } }
        public string audioSegmFalse { get { return ConfigDir + "\\audioSegmFalse.scp"; } }
        public string bckScoreTrue { get { return ConfigDir + "\\bckScoreTrue.mlf"; } }
        public string bckScoreFalse { get { return ConfigDir + "\\bckScoreFalse.mlf"; } }
        public string modifResultTrue { get { return ResultsDir + "\\bckRecountTrue.mlf"; } }
        public string modifResultFalse { get { return ResultsDir + "\\bckRecountFalse.mlf"; } }

        // file extentions
        public static string mfcExt       = ".mfc";
        public static string wavExt       = ".wav";
        public static string labelFileExt = ".lab";
        public static string segmentFileExt    = ".segmentation.txt";
        public static string noiseModelExt     = ".noiseModel";

        //HTK executable files
        public string HBuildExecutable { get { return HTKDir + "\\HBuild.exe"; } }
        public string HCompVExecutable { get { return HTKDir + "\\HCompV.exe"; } }
        public string HCopyExecutable  { get { return HTKDir + "\\HCopy.exe"; } }
        public string HERestExecutable { get { return HTKDir + "\\HERest.exe"; } }
        public string HInitExecutable  { get { return HTKDir + "\\HInit.exe"; } }
        public string HViteExecutable  { get { return HTKDir + "\\HVite.exe"; } }
        public string HParseExecutable { get { return HTKDir + "\\HParse.exe"; } }

        public string aOptionsStr = "-A -D -T 1"; //options string for HTK HCopy funciton


        public const int ERROR_FILE_NOT_FOUND = 2;
        public const double  qualityThreshold = 2.57; // 1.96 for p=95% :: 2.57 for p=99%




        /// <summary>
        /// Compute Feature Vectors size given a specific TARGETKIND, 
        /// Current TARGETKIND types allowed: 
        ///     MFCC, 0, _E, _D, _A 
        /// </summary>
        /// <returns>Void</returns>        
        public void ComputeFVSize()
        {
            try
            {
                string tmpTargetKind = "";
                
                if (TARGETKIND.Length == 0)
                    throw new Exception("Parameter TARGETKIND unspecified.");
                else //Enter the state machine mechanism
                {
                    if (NUMCEPS.Length == 0)
                    {
                        Console.WriteLine("Parameter NUMCEPS unspecified, 12 ceptral coefficients will be used.");
                        NUMCEPS = "12";
                    }

                    int fvLength = int.Parse(NUMCEPS);

                    string[] param = Regex.Split(TARGETKIND, @"_");

                    List<string> checkList = new List<string>();
                    int tmpGroup = 0;
                    int group1 = 0;
                    int group2 = 0;
                    int group3 = 0;
                    foreach (string match in param)
                    {
                        if (!smFeatureDict.TryGetValue(match, out tmpGroup))
                        {
                            Console.WriteLine("Could not recognize parameter '{0}': ignored.", match);
                            continue;
                        }
                        
                        if (checkList.Contains(match))
                        {
                            throw new Exception("Malformed TARGETKIND string: repeated qualifiers.");
                        }
                        checkList.Add(match);

                        switch (tmpGroup)
                        {
                            case 1:
                                group1++;
                                break;
                            case 2:
                                group2++;
                                break;
                            case 3:                                
                                group3++;
                                break;
                        }
                        tmpTargetKind += match + "_";
                    }
                    if ((group1 > 1) || (group2 > 1))
                    {
                        throw new Exception("Ambiguous selection of features. Are you trying to use both 0 and E qualifiers?");
                    }
                    fvLength += group2;
                    fvLength *= (1+group3);

                    VecSize = fvLength;
                    Console.WriteLine("Original TARGETKIND: {0}", TARGETKIND);
                    TARGETKIND = tmpTargetKind.Remove(tmpTargetKind.Length-1);                    
                    Console.WriteLine("Filtered TARGETKIND: {0}", TARGETKIND);
                }
            }
            catch (Exception e)
            {
                    Console.WriteLine(e);
                    throw (e);
            }
        }

        /// <summary>
        /// Populate the list of syllables read from the parsed grammar file (WORD NETWORK file).
        /// Syllables 'SIL', 'SENTENCE_START' and 'SENTENCE_END' will not be included.
        /// </summary>
        /// <returns>Void</returns>
        public void PopulateSyllableList(string wordNetworkF)
        {
            string txtLine = "";
            try
            {
                StreamReader fileReader = new StreamReader(wordNetworkF);
                while ((txtLine = fileReader.ReadLine()) != null) //write all lines to file except SOURCEFORMAT
                {
                    if(!txtLine.StartsWith("I="))
                        continue;

                    string[] param = Regex.Split(txtLine, @"\s+[wW]=[^\w]*");
                    //remove white character at the end of the string
                    string word = Regex.Replace(param[1], @"\s+", "");
                    if (!word.Equals("SIL") &&
                        !word.Equals("SENT_START") &&
                        !word.Equals("SENT_END") &&
                        !word.Equals("NULL"))
                        {
                            if (!multiSyllableList.Contains(word))
                                multiSyllableList.Add(word);
                        }
                    
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }        
        }
        
        public void WriteMfccConfigFile(string filename)
        {
            string content = 
                         "SOURCEFORMAT = " + this.SOURCEFORMAT+"\n" +
                         "TARGETKIND   = " + this.TARGETKIND + "\n" +
                         "TARGETRATE   = " + this.TARGETRATE + "\n" +
                         "SAVECOMPRESSED = " + this.SAVECOMPRESSED + "\n" +
                         "SAVEWITHCRC = "  + this.SAVEWITHCRC + "\n" +
                         "WINDOWSIZE = "   + this.WINDOWDURATION + "\n" +
                         "USEHAMMING = "   + this.USEHAMMING + "\n" +
                         "PREEMCOEF = "    + this.PREEMCOEF + "\n" + //pre-emphasis filter removes low freq content, emphasises high freq content.
                         "NUMCHANS = "     + this.NUMCHANS + "\n" +  //size of filter bank - default = 26
                         "CEPLIFTER = "    + this.CEPLIFTER + "\n" +
                         "NUMCEPS = "      + this.NUMCEPS + "\n" +   //number of cepstral coefficients - default = 12
                         "LOFREQ = "       + this.LOFREQ + "\n" +
                         "HIFREQ = "       + this.HIFREQ + "\n";
            WriteTextFile(filename, content);
        }//end method

        public void WriteHmmConfigFile(string filename)
        {
            string content = 
            "<BEGINtest_config_file>\n\n"+
                "<COMMENT>\n" +
                    "This TCF produces a plain single mixture, single stream diagonal covariance monophone system\n\n" +

                "<BEGINsys_setup>\n\n" +
                    "#hsKind: P\n" +
                    "#covKind: F\n" +
                    "#nStreams: 1\n" +
                    "#nMixes: 1\n" +
                    "#Context: M\n" +
                    "#TiedState: n\n" +
                    "#VQ_clust: L\n" +
                    "HERest_Iter: "    + numIterations.ToString() + "\n" + //############## NUMBER OF ITERATIONS ##############
                    "HERestBKG_Iter: " + numBkgIterations.ToString() + "\n" + 
                    "Trace_tool_calls: y\n\n" +       
                "<ENDsys_setup>:\n\n" +

                "<BEGINtool_steps>\n\n" +
                    "HCopy: y\n" +
                    "HERest: y\n" +    //y -> use HERest i.e. use my labels, use HTK timestamps; n -> use HRest i.e. use my labels AND my timestamps 
                    "HERestBKG: n\n" + //train BACKGROUND model
                    "HVite: y\n" +
                    "HBuild: y\n\n" +
                "<ENDtool_steps>:\n\n" +
            "<ENDtest_config_file>:";

            WriteTextFile(filename, content);
        }//end method


        public void WritePrototypeFiles(string protoTypeDir)
        {
            string content =
                "<BEGINproto_config_file>\n\n<COMMENT>\n\tThis PCF produces a " + numHmmSilStates + " state prototype system\n\n" +
                "<BEGINsys_setup>\n\tnStates:  " + numHmmSilStates + " \n<ENDsys_setup>\n\n" +
                "<ENDproto_config_file>";
            WriteTextFile(protoTypeDir + "\\SIL.pcf", content);

            //if (int.Parse(numHmmSilStates) == 3)
            //{
            //    content =
            //            "0.000e+0 1.000e+0 0.000e+0 0.000e+0 0.000e+0\n" +
            //            "0.000e+0 4.000e-1 2.000e-1 4.000e-1 0.000e+0\n" +
            //            "0.000e+0 0.000e+0 6.000e-1 4.000e-1 0.000e+0\n" +
            //            "0.000e+0 4.000e-1 0.000e+0 3.000e-1 3.000e-1\n" +
            //            "0.000e+0 0.000e+0 0.000e+0 0.000e+0 0.000e+0";
            //    WriteTextFile(protoTypeDir+"\\SIL", content);
            //}

            //- proto.pcf
            //If a file 'WORD'.pcf exists in which the variable 'nStates' has a value
            //different from the one in 'proto', the program will look for a file 'WORD' 
            //that is supposed to contain the related transition matrix of size (nStates+2)x(nStates+2)
            content =
                "<BEGINproto_config_file>\n" +
                "<COMMENT>\n\tThis PCF produces a " + numHmmStates + " state prototype system\n" +
                "<BEGINsys_setup>\n\tnStates: " + numHmmStates + "\n"+
                   "\tsWidths: " + VecSize + "\n" +      //################################################# 
                   "\t#mixes: 1\n"+
                   "\tparmKind: " + TARGETKIND + "\n" +  //#################################################
                   "\tvecSize: " + VecSize + "\n" +      //#################################################
                   "\t#outDir: "+protoFN+"\n\n" +
                "<ENDsys_setup>\n" +
                "<ENDproto_config_file>\n";
            WriteTextFile(protoTypeDir + "\\" + protoFN + ".pcf", content);

            WriteRequiredFilesInfo(protoTypeDir + "\\required_files.txt");
        }

        public void WriteRequiredFilesInfo(string infoFN)
        {
            string content =
                "- " + protoFN + ".pcf\n\n" +
                "if a file 'WORD'.pcf exists in which the variable 'nStates' has a value\n" +
                "different from the one in 'proto', the program will look for a file 'WORD' \n" +
                "that is supposed to contain the related transition matrix of size (nStates+2)x(nStates+2)\n";
            WriteTextFile(infoFN, content);
        }



        //public void WriteSegmentationIniFile(string iniFN)
        //{
        //    string content =
        //        "DATE=2009-07-02\n" +
        //        "AUTHOR="+this.Author+"\n" +
        //        "#\n" +
        //        "CALL_NAME="+ this.CallName+"\n" +
        //        "COMMENT="  + this.Comment+"\n" +
        //        "#\n" +
        //        "#**************** INFO ABOUT ORIGINAL .WAV FILE[s]\n" +
        //        "#WAV_DIR_NAME="+WorkingDir+"\\data\\train\n" +
        //        "SAMPLE_RATE="  +SampleRate+"\n" +
        //        "#\n" +
        //        "#**************** INFO ABOUT FRAMES\n" +
        //        "FRAME_SIZE="+FRAMESIZE+"\n" +
        //        "FRAME_OVERLAP="+FrameOverlap+"\n" +
        //        "WINDOW_FUNCTION=HAMMING\n" + //DEFAULT
        //        "N_POINT_SMOOTH_FFT=3\n" +
        //        "DO_MEL_CONVERSION=false\n" +
        //        "#\n" +
        //        "#**************** INFO ABOUT SONOGRAM\n" +
        //        "MIN_FREQ = " + LOFREQ + "\n" +
        //        "MAX_FREQ = " + HIFREQ + "\n" +
        //        "#\n" + 
        //        "#**************** NOISE REDUCTION\n" +
        //        "#NOISE_REDUCTION_TYPE=NONE\n" +
        //        "#NOISE_REDUCTION_TYPE=STANDARD\n" +
        //        "NOISE_REDUCTION_TYPE=FIXED_DYNAMIC_RANGE\n" +
        //        "DYNAMIC_RANGE=60.0\n" +
        //        "#\n" +
        //        "#**************** INFO ABOUT SEGMENTATION:- ENDPOINT DETECTION of VOCALISATIONS \n" +
        //        "# See Lamel et al 1981.\n" +
        //        "# They use k1, k2, k3 and k4, minimum pulse length and k1_k2Latency.\n" +
        //        "# Here we set k1 = k3, k4 = k2,  k1_k2Latency = 0.186s (5 frames)\n" +
        //        "#                  and \"minimum pulse length\" = 0.075s (2 frames) \n" +
        //        "# SEGMENTATION_THRESHOLD_K1 = decibels above the minimum level\n" +
        //        "# SEGMENTATION_THRESHOLD_K2 = decibels above the minimum level\n" +
        //        "# K1_K2_LATENCY = seconds delay between signal reaching k1 and k2 thresholds\n" +
        //        "# VOCAL_GAP = gap (in seconds) required to separate vocalisations \n" +
        //        "# MIN_VOCAL_DURATION = minimum length of energy pulse - do not use this - accept all pulses.\n" +
        //        "SEGMENTATION_THRESHOLD_K1=3.0\n" +
        //        "SEGMENTATION_THRESHOLD_K2=5.0\n" +
        //        "K1_K2_LATENCY=0.05\n" +
        //        "VOCAL_GAP=0.2\n" +
        //        "MIN_VOCAL_DURATION=0.075";

        //    WriteTextFile(iniFN, content);
        //}


        public static void WriteTextFile(string path, string text)
        {
            StreamWriter wltWriter = null;
            try
            {
                wltWriter = File.CreateText(path);
                wltWriter.WriteLine(text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            finally
            {
                if (wltWriter != null)
                {
                    wltWriter.Flush();
                    wltWriter.Close();
                }

            }// end finally
        }


        public static List<string> GetSyllableNames(string fileName)
        {
            string txtLine = "";
            List<string> list = new List<string>();
            try
            {
                StreamReader fileReader = new StreamReader(fileName);
                while ((txtLine = fileReader.ReadLine()) != null) //write all lines to file except SOURCEFORMAT
                {
                    //if(!txtLine.StartsWith("I=")) continue;

                    //string[] param = Regex.Split(txtLine, @"\s+[wW]=[^\w]*");
                    //remove white character at the end of the string
                    //string word = Regex.Replace(param[1], @"\s+", "");
                    string word = txtLine.Trim();
                    if (word.Equals("SIL")) continue;
                        //&& !word.Equals("SENT_START") &&
                        //!word.Equals("SENT_END") &&
                        //!word.Equals("NULL")
                    if (!list.Contains(word)) list.Add(word);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            return list;
        }//end method GetSyllableNames()


        public void AppendThresholdInfo2IniFile(string syllName, double htkThreshold, double durationMean, double durationSD)
        {
            //Append optimum threshold and duration threshold info to segmentation ini file
            string ToAppend = "#\n#**************** CALL THRESHOLDS FOR " + syllName + " HMM AND QUALITY/DURATION\n" +
                      "#    NOTE 1: HMM threshold is valid for HMM scores normalised to hit duration.\n" +
                      "#    NOTE 2: Duration values in seconds.\n" +
                      "#    NOTE 3: SD threshold = number of SD either side of mean. 1.96=95% confidence\n" +
                      "#            that you are NOT excluding a call with a valid duration.\n" +
                      syllName+"_HTK_THRESHOLD=" + htkThreshold + "\n" +
                      syllName + "_DURATION_MEAN=" + durationMean.ToString("f6") + "\n" +
                      syllName + "_DURATION_SD=" + durationSD.ToString("f6") + "\n" +
                      "SD_THRESHOLD=" + qualityThreshold;  //1.96 for p=95% :: 2.57 for p=99%

            string iniPath = this.ConfigDir + "\\" + HTKConfig.segmentationIniFN;
            FileTools.Append2TextFile(iniPath, ToAppend, false);
        }


        #region Constructor
        public HTKConfig()
        {
            // MAJOR STRUCTURAL TRAINING PARAMETERS -- DEFAULT VALUES
            this.multisyllabic = false;     //single syllable call
            this.useBKGModel = true;        //true -> use BKG model;    false -> use SIL model
            this.LLRNormalization = true;   //perform LLR normalization on the output from HVite
            this.noSegmentation = false;    //true -> each recording is considered as single word. No SIL model is generated/trained from the recordings.
            this.UseHERest      = "y";      //y -> use HERest i.e. use my labels, use HTK timestamps; n -> use HRest i.e. use my labels AND my timestamps 
            this.UseHERestBKG   = "n";      //train BACKGROUND model

            this.numHmmStates    = "10";    //number of hmm states for CALL model
            this.numHmmSilStates = "3";     //number of hmm states for SILENCE model
            this.numIterations   = 5;       //number of iterations for re-estimating the 

            this.numHmmBkgStates  = "1";    //number of hmm states for BKG model
            this.numBkgIterations = 5;      //number of iterations for re-estimating the BG model

            //###########################################################################################################################
            this.SOURCEFORMAT   = "WAV";
            this.SAVECOMPRESSED = "T";
            this.SAVEWITHCRC    = "T";

            //MFCC PARAMETERS
            this.TARGETKIND = "MFCC";  //components to include in feature vector
            this.USEHAMMING = "T";
            this.PREEMCOEF  = "0.97";    //pre-emphasis filter removes low frequency content and gives more importance to high freq content.
            this.NUMCHANS   = "26";      //size of filter bank - default = 26
            this.CEPLIFTER  = "22";
            this.NUMCEPS    = "12";      //number of cepstral coefficients - default = 12
            
            //initialize dictionary
            smFeatureDict.Add("MFCC", 1);
            smFeatureDict.Add("0", 2);
            smFeatureDict.Add("E", 2);
            smFeatureDict.Add("D", 3);
            smFeatureDict.Add("A", 3);

            this.SilName = "SIL";
            this.BkgName = "BACKGROUND";
        }

        /// <summary>
        /// This constuctor sets up the directory structure.
        /// </summary>
        /// <param name="workingDir"></param>
        /// <param name="templateName"></param>
        public HTKConfig(string parentDir, string iniPath) : this()
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            this.CallName   = dict[Key_CALL_NAME];
            this.Author     = dict[Key_AUTHOR];
            this.Comment    = dict[Key_COMMENT];


            //FRAMING PARAMETERS
            this.SampleRate = dict[Key_SAMPLE_RATE];
            this.FRAMESIZE = dict[Key_FRAME_SIZE];
            this.FrameOverlap = dict[Key_FRAME_OVERLAP];
            this.TARGETRATE = "116100.0"; //=10e-7 seconds - that is a frame every 11.6 millisconds.
            this.WINDOWDURATION = "232200.0"; //=23.22 milliseconds
            //parse all the above strings to ints or reals
            //double tr;
            //Double.TryParse(this.TARGETRATE, out tr);
            //double wd; //window duration
            //Double.TryParse(this.WINDOWDURATION, out wd);
            //int sr;  //not actually used - HTK does not need. Segmentation requires only framing info derived from SR below.
            //Int32.TryParse(this.SampleRate, out sr);
            //this.FRAMESIZE = (Math.Floor(wd / 10000000 * sr)).ToString();
            //this.FrameOverlap = (tr / wd).ToString();


            //BANDWIDTH
            this.MinHz = dict[Key_MIN_FREQ];
            this.MaxHz = dict[Key_MAX_FREQ];
            this.LOFREQ = dict[Key_MIN_FREQ];
            this.HIFREQ = dict[Key_MAX_FREQ];


            //SET UP DIRECTORY STRUCTURE
            //htkConfig.WorkingDir    = Directory.GetCurrentDirectory();
            this.WorkingDir = parentDir + "Template_" + this.CallName;
            this.ConfigDir  = this.WorkingDir + "\\" + this.CallName;
            this.DataDir    = this.WorkingDir + "\\data";
            this.ResultsDir = this.WorkingDir + "\\results";
            this.SegmentationDir = this.WorkingDir + "\\Segmentation";  //NOT USED FOR TESTING

            //SET UP BACKGROUND DIRECTORY STRUCTURE
            this.BkgWorkingDir = parentDir + "Template_BACKGROUND";
            this.BkgConfigDir  = this.BkgWorkingDir;
            this.BkgDataDir    = this.BkgWorkingDir + "\\data";

            //RESOURCES DIR
            string resourcesDir = "C:\\SensorNetworks\\Software\\";
            this.HTKDir         = resourcesDir + "Extra Assemblies\\HTK\\";

            this.SilenceModelSrc  = resourcesDir + "HMMBuilder\\SilenceModel\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav";
            this.SilenceModelPath = this.SegmentationDir + "\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav"; //NOT USED IN TESTING
            this.NoiseModelFN     = Path.GetFileNameWithoutExtension(this.SilenceModelPath) + HTKConfig.noiseModelExt;
        }

        #endregion


    }//end class

}
