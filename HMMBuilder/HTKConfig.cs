using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HMMBuilder
{
    public class HTKConfig
    {
        //public string WorkingDir  { get; set; }
        public string WorkingDir { get; set; }
        public string DataDir     { get; set; }
        public string ConfigDir   { get; set; }
        public string HTKDir      { get; set; }
        public string SegmentationDir { get; set; }
        public string ResultsDir { get; set; }
        public string Author      { get; set; }
        public string CallName    { get; set; }
        public string Comment     { get; set; }
        public string numHmmStates { get; set; }

        public string SampleRate  { get; set; }
        public string FrameOverlap { get; set; }
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
        
        /// <summary>
        /// Table used for computing feature vectors size 
        /// </summary>
        private Dictionary<string, int> smFeatureDict = new Dictionary<string, int>();
      

        public string trnDirPath      { get { return DataDir + "\\train"; } }
        public string tstFalseDirPath { get { return DataDir + "\\test\\testFalse"; } }
        public string tstTrueDirPath  { get { return DataDir + "\\test\\testTrue"; } }

        public string ProtoConfDir      { get { return ConfigDir + "\\protoConfigs"; } }
        public string ConfigFN          { get { return ConfigDir + "\\monPlainM1S1.dcf"; } }
        public string MfccConfigFN      { get { return ConfigDir + "\\mfccConfig"; } }
        public string MfccConfig2FN     { get { return ConfigDir + "\\mfccConfig.txt"; } } //need this copy for training


        public string DictFile     { get { return ConfigDir + "\\dict"; } }
        public string cTrainF      { get { return ConfigDir + "\\codetrain.scp"; } }
        public string cTestFalseF  { get { return ConfigDir + "\\codetestfalse.scp"; } }
        public string cTestTrueF   { get { return ConfigDir + "\\codetesttrue.scp"; } }
        public string trainF       { get { return ConfigDir + "\\train.scp"; } }
        public string tFalseF      { get { return ConfigDir + "\\testfalse.scp"; } }
        public string tTrueF       { get { return ConfigDir + "\\testtrue.scp"; } }
        public string LabelSeqF    { get { return ConfigDir + "\\labSeq"; } }
        public string wltF         { get { return ConfigDir + "\\phones.mlf";  } }//file containing segmentation info into SONG SYLLABLES + SILENCE
        public string wordNet      { get { return ConfigDir + "\\phone.net"; } }
        //for scanning a single test file
        public string TestFileCode { get { return ConfigDir + "\\Test_CodeSingle.scp"; } }
        public string TestFile     { get { return ConfigDir + "\\Test_Single.scp"; } }

        //lists directory
        //public string ListsDir   { get { return ConfigDir + "\\lists"; } }
        public string monophones { get { return ConfigDir + "\\labelList.txt"; } } //contains list of syllables to recognise including SIL

        //HMM files
        public string HmmDir       { get { return ConfigDir + "\\hmms"; } }
        public string tgtDir0      { get { return HmmDir + "\\hmm.0"; } }
        public string tgtDir1      { get { return HmmDir + "\\hmm.1"; }}
        public string tgtDir2      { get { return HmmDir + "\\hmm.2"; }}
        public string tgtDirTmp    { get { return HmmDir + "\\tmp"; } }
        public string macrosFN        = "macros";
        public string hmmdefFN        = "hmmdefs";
        public string protoFN         = "proto"; //CANNOT CHANGE THIS NAME !!??
        public string vFloorsFN       = "vFloors";
        public string prototypeHMM { get { return ConfigDir + "\\" + protoFN; } }

        //results files
        public string resultTrue  { get { return ResultsDir + "\\recountTrue.mlf";} }
        public string resultFalse { get { return ResultsDir + "\\recountFalse.mlf"; } }
        public string resultTest  { get { return ResultsDir + "\\TestScan.mlf"; } } //for scanning a single file

        // file extentions
        public string mfcExt       = ".mfc";
        public string wavExt       = ".wav";
        public string labelFileExt = ".lab";
        public string segmentFileExt    = ".segmentation.txt";
        public string noiseModelExt     = ".noiseModel";
        public string segmentationIniFN = "segmentation.ini";
        public string segmentationExe   = "VocalSegmentation.exe";

        //HTK executable files
        public string HBuildExecutable { get { return HTKDir + "\\HBuild.exe"; } }
        public string HCompVExecutable { get { return HTKDir + "\\HCompV.exe"; } }
        public string HCopyExecutable  { get { return HTKDir + "\\HCopy.exe"; } }
        public string HERestExecutable { get { return HTKDir + "\\HERest.exe"; } }
        public string HInitExecutable  { get { return HTKDir + "\\HInit.exe"; } }
        public string HViteExecutable  { get { return HTKDir + "\\HVite.exe"; } }

        public string aOptionsStr = "-A -D -T 1"; //options string for HTK HCopy funciton


        public const int ERROR_FILE_NOT_FOUND = 2;

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
        
        public void WriteMfccConfigFile(string filename)
        {
            string content = 
                         "SOURCEFORMAT = WAV\n" +
                         "TARGETKIND   = " + TARGETKIND + "\n" +
                         //"TARGETKIND   = MFCC_E_D\n" +   //##################################################
                         "TARGETRATE = " + TARGETRATE + "\n" +
                         "SAVECOMPRESSED = T\n" +
                         "SAVEWITHCRC = T\n" +
                         "WINDOWSIZE = " + WINDOWDURATION + "\n" +
                         "USEHAMMING = T\n" +
                         "PREEMCOEF = 0.97\n" +
                         "NUMCHANS = 26\n" +
                         "CEPLIFTER = 22\n" +
                         "NUMCEPS = 12\n" +
                         "LOFREQ = " + LOFREQ + "\n" +
                         "HIFREQ = " + HIFREQ + "\n";

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
                    "HERest_Iter: 5\n" +
                    "#HERest_par_mode: n\n" +
                    "#Clean_up: n\n" +
                    "Trace_tool_calls: y\n\n" +
        
                    "<ENDsys_setup>:\n\n" +

                    "<BEGINtool_steps>\n\n" +

                    "HCopy: y\n" +
                    "#HList: n\n" +
                    "#HQuant: n\n" +
                    "#HLEd: y\n" +
                    "#HInit: y\n" +
                    "#HRest: n\n" +
                    "HERest: y\n" +
                    "#HSmooth: n\n" +
                    "HVite: y\n" +
                    "HBuild: y\n\n" +

                    "<ENDtool_steps>:\n\n" +

                    "<ENDtest_config_file>:";

            WriteTextFile(filename, content);
        }//end method


        public void WritePrototypeFiles(string protoTypeDir)
        {
            string content =
                "<BEGINproto_config_file>\n\n<COMMENT>\n\tThis PCF produces a 3 state prototype system\n\n" +
                "<BEGINsys_setup>\n\tnStates: 3\n<ENDsys_setup>\n\n" +
                "<ENDproto_config_file>";
            WriteTextFile(protoTypeDir + "\\SIL.pcf", content);

            content =
            "0.000e+0 1.000e+0 0.000e+0 0.000e+0 0.000e+0\n" +
            "0.000e+0 4.000e-1 2.000e-1 4.000e-1 0.000e+0\n" +
            "0.000e+0 0.000e+0 6.000e-1 4.000e-1 0.000e+0\n" +
            "0.000e+0 4.000e-1 0.000e+0 3.000e-1 3.000e-1\n" +
            "0.000e+0 0.000e+0 0.000e+0 0.000e+0 0.000e+0\n";
            WriteTextFile(protoTypeDir+"\\SIL", content);

            //- proto.pcf
            //If a file 'WORD'.pcf exists in which the variable 'nStates' has a value
            //different from the one in 'proto', the program will look for a file 'WORD' 
            //that is supposed to contain the related transition matrix of size (nStates+2)x(nStates+2)
            content =
                "<BEGINproto_config_file>\n" +
                "<COMMENT>\n\tThis PCF produces a 8 state prototype system\n" +
                "<BEGINsys_setup>\n\tnStates: "+numHmmStates+"\n"+
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



        public void WriteSegmentationIniFile(string iniFN)
        {
            string content =
                "DATE=2009-07-02\n" +
                "AUTHOR="+this.Author+"\n" +
                "#\n" +
                "CALL_NAME="+ this.CallName+"\n" +
                "COMMENT="  + this.Comment+"\n" +
                "#\n" +
                "#**************** INFO ABOUT ORIGINAL .WAV FILE[s]\n" +
                "#WAV_DIR_NAME="+WorkingDir+"\\data\\train\n" +
                "SAMPLE_RATE="  +SampleRate+"\n" +
                "#\n" +
                "#**************** INFO ABOUT FRAMES\n" +
                "FRAME_SIZE="+FRAMESIZE+"\n" +
                "FRAME_OVERLAP="+FrameOverlap+"\n" +
                "WINDOW_FUNCTION=HAMMING\n" + //DEFAULT
                "N_POINT_SMOOTH_FFT=3\n" +
                "DO_MEL_CONVERSION=false\n" +
                "#\n" +
                "#**************** INFO ABOUT SONOGRAM\n" +
                "MIN_FREQ = " + LOFREQ + "\n" +
                "MAX_FREQ = " + HIFREQ + "\n" +
                "NOISE_REDUCTION_TYPE=SILENCE_MODEL\n" +
                "SILENCE_RECORDING_PATH="+SilenceModelPath + "\n" +
                "#\n" +
                "#**************** INFO ABOUT SEGMENTATION:- ENDPOINT DETECTION of VOCALISATIONS \n" +
                "# See Lamel et al 1981.\n" +
                "# They use k1, k2, k3 and k4, minimum pulse length and k1_k2Latency.\n" +
                "# Here we set k1 = k3, k4 = k2,  k1_k2Latency = 0.186s (5 frames)\n" +
                "#                  and \"minimum pulse length\" = 0.075s (2 frames) \n" +
                "# SEGMENTATION_THRESHOLD_K1 = decibels above the minimum level\n" +
                "# SEGMENTATION_THRESHOLD_K2 = decibels above the minimum level\n" +
                "# K1_K2_LATENCY = seconds delay between signal reaching k1 and k2 thresholds\n" +
                "# VOCAL_GAP = gap (in seconds) required to separate vocalisations \n" +
                "# MIN_VOCAL_DURATION = minimum length of energy pulse - do not use this - accept all pulses.\n" +
                "SEGMENTATION_THRESHOLD_K1=3.5\n" +
                "SEGMENTATION_THRESHOLD_K2=6.0\n" +
                "K1_K2_LATENCY=0.05\n" +
                "VOCAL_GAP=0.2\n" +
                "MIN_VOCAL_DURATION=0.075";

            WriteTextFile(iniFN, content);
        }


        public static void WriteTextFile(string path, string text)
        {
            //Console.WriteLine("");
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


        #region Constructor
        public HTKConfig()
        {
            //initialize dictionary
            smFeatureDict.Add("MFCC", 1);
            smFeatureDict.Add("0", 2);
            smFeatureDict.Add("E", 2);
            smFeatureDict.Add("D", 3);
            smFeatureDict.Add("A", 3);
        }
        #endregion

    }//end class

}
