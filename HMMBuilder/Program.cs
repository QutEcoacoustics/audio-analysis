using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using TowseyLib;
using AudioAnalysis;

namespace HMMBuilder
{
    public class Program
    {
        static void Main(string[] args)
        {
            #region Variables

            HTKConfig htkConfig = new HTKConfig();

            //htkConfig.CallName = "CURLEW1";
            //htkConfig.Comment = "Parameters for Curlew";
            //htkConfig.LOFREQ = "1000";
            //htkConfig.HIFREQ = "7000"; //try 6000, 7000 and 8000 Hz as max for Curlew
            //htkConfig.numHmmStates = "6";  //number of hmm states for call model

            //htkConfig.CallName = "CURRAWONG1";
            //htkConfig.Comment = "Parameters for Currawong";
            //htkConfig.LOFREQ = "800";
            //htkConfig.HIFREQ = "6000";     //try 6000, 7000 and 8000 Hz
            //htkConfig.numHmmStates = "6";  //number of hmm states for call model

            //htkConfig.CallName = "WHIPBIRD1";
            //htkConfig.Comment = "Parameters for whip bird";
            //htkConfig.LOFREQ = "500";
            //htkConfig.HIFREQ = "9000"; 
            //htkConfig.numHmmStates = "6";  //number of hmm states for call model

            htkConfig.CallName = "KOALAFEMALE1";
            htkConfig.Comment = "Parameters for female koala";
            htkConfig.LOFREQ = "500";
            htkConfig.HIFREQ = "7000";
            htkConfig.numHmmStates = "14";  //number of hmm states for call model

            //==================================================================================================================
            //==================================================================================================================


            htkConfig.Author       = "Michael Towsey";
            htkConfig.SOURCEFORMAT = "WAV";
            htkConfig.TARGETKIND   = "MFCC"; //components to include in feature vector

            //FRAMING PARAMETERS
            htkConfig.SampleRate     = "22050";    //samples per second //this must be put first inlist of framing parameters
            htkConfig.TARGETRATE     = "116100.0"; //x10e-7 seconds - that is a frame every 11.6 millisconds.
            htkConfig.WINDOWDURATION = "232200.0"; //=23.22 milliseconds

            //parse all the above strings to ints or reals
            double tr;
            Double.TryParse(htkConfig.TARGETRATE, out tr);
            double wd; //window duration
            Double.TryParse(htkConfig.WINDOWDURATION, out wd);
            int sr;  //not actually used - HTK does not need. Segmentation requires only framing info derived from SR below.
            Int32.TryParse(htkConfig.SampleRate, out sr); 

            htkConfig.FRAMESIZE = (Math.Floor(wd / 10000000 * sr)).ToString(); 
            htkConfig.FrameOverlap = (tr / wd).ToString();
            htkConfig.SAVECOMPRESSED = "T";
            htkConfig.SAVEWITHCRC = "T";

            //MFCC PARAMETERS
            htkConfig.USEHAMMING = "T";
            htkConfig.PREEMCOEF  = "0.97"; //pre-emphasis filter removes low frequency content and gives more importance to high freq content.
            htkConfig.NUMCHANS   = "26";   //size of filter bank
            htkConfig.CEPLIFTER  = "22";
            htkConfig.NUMCEPS    = "12";   //number of cepstral coefficients

            //htkConfig.WorkingDir  = Directory.GetCurrentDirectory();
            htkConfig.WorkingDir  = "C:\\SensorNetworks\\Templates\\Template_" + htkConfig.CallName;
            htkConfig.HTKDir      = "C:\\SensorNetworks\\Software\\HTK";
            htkConfig.SegmentationDir = "C:\\SensorNetworks\\Software\\HMMBuilder\\VocalSegmentation";
            htkConfig.DataDir     = htkConfig.WorkingDir + "\\data";
            htkConfig.ConfigDir   = htkConfig.WorkingDir + "\\" + htkConfig.CallName;
            htkConfig.ResultsDir  = htkConfig.WorkingDir + "\\results";
            htkConfig.SilenceModelPath = "C:\\SensorNetworks\\Software\\HMMBuilder\\SilenceModel\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav";
            htkConfig.NoiseModelFN = Path.GetFileNameWithoutExtension(htkConfig.SilenceModelPath) + htkConfig.noiseModelExt;
            
            Console.WriteLine("CWD=" + htkConfig.WorkingDir);
            Console.WriteLine("CFG=" + htkConfig.ConfigDir);
            Console.WriteLine("DAT=" + htkConfig.DataDir);

            string vocalization = "";
            string tmpVal       = "";
            string aOptionsStr = htkConfig.aOptionsStr;
            string pOptionsStr = ""; //-t 150.0"; //pruning option for HErest.exe BUT does not appear to make any difference
            bool good = true;
            int numIters = 0;  //number of training iterations
            #endregion



            switch (args.Length)
            {
                //case 0:
                //    Console.WriteLine("USAGE: HMMBuilder ConfigFile");
                //    break;

                default:
                    #region ONE: Write Configuration Files
                    Console.WriteLine("WRITE FIVE CONFIGURATION FILES");
                    try
                    {
                        htkConfig.ComputeFVSize(); //Compute Feature Vectors size given htkConfig.TARGETKIND
                        if(! Directory.Exists(htkConfig.ConfigDir))   Directory.CreateDirectory(htkConfig.ConfigDir);
                        if(! Directory.Exists(htkConfig.ProtoConfDir))Directory.CreateDirectory(htkConfig.ProtoConfDir);
                        htkConfig.WriteMfccConfigFile(htkConfig.MfccConfigFN);  //Write the mfcc file
                        htkConfig.WriteHmmConfigFile(htkConfig.ConfigFN);       //Write the dcf file
                        htkConfig.WritePrototypeFiles(htkConfig.ProtoConfDir);  //prototype files
                        //segmentation ini file
                        string segmentationIniFile = htkConfig.SegmentationDir + "\\" + htkConfig.segmentationIniFN;
                        htkConfig.WriteSegmentationIniFile(segmentationIniFile);
                        //IMPORTANT: WRITE PROTOTYPE FILES FOR BIRD CALL OF INTEREST
                    }
                    catch
                    {
                        Console.WriteLine("ERROR! FAILED TO WRITE FIVE CONFIGURATION FILES");
                        good = false;
                        break;
                    }        
                    #endregion


                    #region TWO: DATA PREPARATION TOOLS:- Read Two Configuration Files and do Feature Extraction
                    Console.WriteLine("DATA PREPARATION TOOLS:- READ TWO CONFIGURATION FILES");
                    try
                    {
                        Console.WriteLine("Read  MFCC params from file: " + htkConfig.MfccConfigFN);
                        Console.WriteLine("Write MFCC params to   file: " + htkConfig.MfccConfig2FN);
                        HMMSettings.ReadTCF(htkConfig.ConfigFN, htkConfig.MfccConfigFN, htkConfig.MfccConfig2FN);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE METHOD HMMSettings.ReadTCF()");
                        good = false;
                        break;
                    }

                    //if (HMMSettings.ConfParam.TryGetValue("COVKIND", out tmpVal))
                    //{
                    //    cK = tmpVal;
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Covariance kind set to 'FULLC'");
                    //    cK = "F";
                    //}

                    if (HMMSettings.ConfigParam.TryGetValue("HEREST_ITER", out tmpVal))
                    {
                        numIters = int.Parse(tmpVal);
                    }
                    else
                    {
                        numIters = 3;
                    }
                    Console.WriteLine("\n\nNumber of iterations for training set = " + numIters);

                    if (HMMSettings.ConfigParam.TryGetValue("TRACE_TOOL_CALLS", out tmpVal))
                    {
                        if (tmpVal.Equals("Y")) aOptionsStr = htkConfig.aOptionsStr; //aOptionsStr = "-A -D -T 1";
                    }

                    // HCOPY is the HTK tool to parameterise .wav files ie to extract mfcc features
                    // - AT PRESENT WE ALWAYS DO FEATURE EXTRACTION
                    if (HMMSettings.ConfigParam.TryGetValue("HCOPY", out tmpVal))
                    {
                        if (tmpVal.Equals("Y"))//feature vectors have not been extracted yet 
                        {                            
                            try
                            {
                                HTKHelper.HCopy(aOptionsStr, htkConfig, true);
                            }
                            catch
                            {
                                Console.WriteLine("ERROR!! FAILED TO COMPLETE METHOD HTKHelper.HCopy(aOptionsStr, htkConfig, true)");
                                //Console.WriteLine(ex.ToString());
                                good = false;
                                break; 
                            }
                        }
                        else //feature vectors have already been extracted
                        {                            
                            try
                            {
                                HTKHelper.HCopy(aOptionsStr, htkConfig, false);
                            }
                            catch
                            {
                                Console.WriteLine("ERROR!! FAILED TO COMPLETE METHOD HTKHelper.HCopy(aOptionsStr, htkConfig, false)");
                                good = false;
                                break; 
                            }
                        }
                    } //end HCOPY
                    #endregion


                    #region THREE: Data Preparation (see manual 2.3.1):- Segment the training data; Get PHONE LABELS
                    try
                    {
                        bool extractLabels = true;
                        if (extractLabels) //True by default - i.e. always segment the training data files
                        {
                            Console.WriteLine("\nABOUT TO SEGMENT WAV TRAINING FILES");
                            //copy segmentation ini file to the data directory.
                            string segmentationIniFile = htkConfig.SegmentationDir + "\\" + htkConfig.segmentationIniFN;
                            string fn = System.IO.Path.GetFileName(segmentationIniFile);
                            System.IO.File.Copy(segmentationIniFile, htkConfig.trnDirPath + "\\" + fn, true);

                            //REWORKED FOLLOWING LINE TO CALL METHOD DIRECTLY AND NOT EXECUTE PROCESS
                            //HTKHelper.SegmentDataFiles(htkConfig, ref vocalization);
                            int verbosity = 0;
                            Main_CallSegmentation2.Execute(htkConfig.trnDirPath, htkConfig.trnDirPath, verbosity);
                        }
                        HTKHelper.CreateWLT(htkConfig, ref vocalization, extractLabels);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE Data Preparation Region");
                        good = false;
                        break;
                    }
                    #endregion


                    #region FOUR: WriteDictionary consisting of PHONE LABELS
                    try
                    {
                        HTKHelper.WriteDictionary(htkConfig);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE Write Dicitonary Region");
                        good = false;
                        break;
                    }
                    #endregion


                    #region FIVE: TRAINING TOOLS:-  (see manual 2.3.2)
                    Console.WriteLine("\nTRAINING: READING THE PROTOTYPE CONFIGURATION FILES");
                    try
                    {
                        HMMSettings.ReadPCF(htkConfig.ProtoConfDir);
                        HMMSettings.WriteHMMprototypeFile(htkConfig.prototypeHMM);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE TRAINING TOOLS Region");
                        Console.WriteLine(ex.ToString());
                        good = false;
                        break; 
                    }


                    Console.WriteLine("\nTRAINING: INITIALISE SYSTEM FILES");
                    try
                    {
                        //HTKHelper.InitSys(aOtpStr, protoConfDir, protoDir, tgtDir0, configTrain, trainF);
                        HTKHelper.InitSys(aOptionsStr, htkConfig);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        good = false;
                        break; 
                    }


                    Console.WriteLine("\nTRAINING: HMM Model re-estimation using HTK.HERest");
                    try
                    {
                        if (HMMSettings.ConfigParam.TryGetValue("HEREST_ITER", out tmpVal))
                        {
                            numIters = int.Parse(tmpVal);
                        }
                        else
                        {
                            Console.WriteLine("'HEREST_ITER' parameter not specified. Default (3) value used.");
                            numIters = 3;
                        }

                        HTKHelper.HERest(numIters, aOptionsStr, pOptionsStr, htkConfig);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE HTKHelper.HERest(numIters, aOptionsStr, pOptionsStr, htkConfig)");
                        good = false;
                        break; 
                    }
                    #endregion


                    #region SIX: Test the HMMs
                    try
                    {
                        if (HMMSettings.ConfigParam.TryGetValue("HBUILD", out tmpVal))
                        {
                            if (tmpVal.Equals("Y")) //Generate the network file
                            {
                                HTKHelper.HBuild(htkConfig.monophones, htkConfig.wordNet, htkConfig.HBuildExecutable);
                            }
                            else
                            {
                                //TO DO: Ask the user for the word network file
                            }
                        }
                        else
                        {
                            //TO DO: Ask the user for the word network file
                        }
                        //True calls
                        HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.tTrueF, htkConfig.wordNet,
                                        htkConfig.DictFile, htkConfig.resultTrue, htkConfig.monophones, htkConfig.HViteExecutable);
                        //False calls
                        HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.tFalseF, htkConfig.wordNet,
                                        htkConfig.DictFile, htkConfig.resultFalse, htkConfig.monophones, htkConfig.HViteExecutable);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE HTKHelper.HVite()");
                        good = false;
                        break; 
                    }
                    #endregion


                    #region SEVEN: Accuracy Measurements: Accuracy = (TruePositives + TrueNegative)/TotalSamples

                    //calculate frame rate = 1sec / frame duration
                    double frameRate = 10000000 / double.Parse(htkConfig.TARGETRATE);
                    Console.WriteLine("\nFrame rate = "+ frameRate+"\n");

                    //calculate the mean and sd of the training call durations
                    string masterLabelFile = htkConfig.ConfigDir + "\\phones.mlf";
                    double mean;
                    double sd;
                    string regex = null;
                    Helper.AverageCallDuration(masterLabelFile, regex, vocalization, out mean, out sd);
                    Console.WriteLine("Training song durations= " + mean.ToString("f4") + "+/-" + sd.ToString("f4") + " seconds or " +
                                      (mean * frameRate).ToString("f1") + " frames\n");

                    //calculate the mean and sd of the testing call durations
                    masterLabelFile = htkConfig.WorkingDir + "\\results\\recountTrue.mlf";
                    double mean2;
                    double sd2;
                    regex = @"^\d+\s+\d+\s+\w+";
                    Helper.AverageCallDuration(masterLabelFile, regex, vocalization, out mean2, out sd2);
                    Console.WriteLine("Testing song durations= " + mean2.ToString("f4") + "+/-" + sd2.ToString("f4") + " seconds or "+
                                      (mean2 * frameRate).ToString("f1") + " frames\n");


                    //Read the output files
                    int optimumThreshold = -Int32.MaxValue;
                    double variance = sd * sd;
                    int tpCount = 0;  //true positives
                    int fpCount = 0;  //false positives
                    int trueSCount  = 0;
                    int falseSCount = 0;
                    try
                    {
                        float tppercent = 0.0f;
                        float tnpercent = 0.0f;
                        float accuracy  = 0.0f;
                        float avTPScore = 0.0f;
                        float avFPScore = 0.0f;
                        float threshold = -50f;      //set a central threshold value suitable to create a ROC curve  
                        int step = 2; //to step the threshold
                        double maxScore = -Double.MaxValue;
                        int maxTpCount = 0;
                        int maxTnCount = 0;
                        for (int i = 9; i >= -8; i--)
                        {
                            trueSCount = 0;
                            falseSCount = 0;
                            float t = threshold - (i * step);
                            Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, mean, variance, frameRate, 
                                              ref vocalization, t, out tpCount, out fpCount, out trueSCount,  out falseSCount,
                                              out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);
                            Console.WriteLine("TP={0:f1}\tTN={1:f1}\tAcc={2:f1}%\tavTPscore={3:f0}\tavFPscore={4:f0} \t(threshold={5})", tppercent, tnpercent, accuracy, avTPScore, avFPScore, t);
                            if (accuracy > maxScore)
                            {
                                maxScore = accuracy;
                                optimumThreshold = (int)t;
                                maxTpCount = tpCount;
                                maxTnCount = falseSCount-fpCount;
                            }
                        }
                        //calculate optimum so can save best data.
                        Console.WriteLine("Max score = " + maxScore + "  at threshold= " + optimumThreshold);
                        Console.WriteLine("TP=" + maxTpCount + "/" + trueSCount + "  TN=" + maxTnCount + "/" + falseSCount);
                        Console.WriteLine("FN=" + (trueSCount - maxTpCount)  + "     FP=" + (falseSCount - maxTnCount));
                        //repeat in order to print the PDF file of individual results
                        Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, mean, variance, frameRate,
                               ref vocalization, optimumThreshold, out tpCount, out fpCount, out trueSCount, out falseSCount,
                               out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);
                        Console.WriteLine("You can check individual hits in the template's results directory.");

                    }
                    catch 
                    {
                        good = false;
                        break;  
                    }

                    #endregion



                    #region EIGHT: SET UP THE TEMPLATE ZIP FILE
                    string oldSegmentDir = htkConfig.SegmentationDir;
                    string newSegmentDir = htkConfig.ConfigDir;
                    //Directory.CreateDirectory(newSegmentDir);

                    //COPY SILENCE MODEL FILES TO CONFIG\\SEGMENTATION DIR
                    string oldNoiseDir = Path.GetDirectoryName(htkConfig.SilenceModelPath);
                    string noiseModelFN = Path.GetFileNameWithoutExtension(htkConfig.SilenceModelPath);
                    string ext = htkConfig.noiseModelExt;
                    string oldNoiseModel = oldNoiseDir   + "\\" + noiseModelFN + ext;
                    string newNoiseModel = newSegmentDir + "\\" + noiseModelFN + ext;
                    File.Copy(oldNoiseModel, newNoiseModel, true);

                    //COPY SEGMENTATION FILES TO CONFIG\\SEGMENTATION DIR
                    //string oldSegmentExePath = oldSegmentDir + "\\" + htkConfig.segmentationExe;
                    //string newSegmentExePath = newSegmentDir + "\\" + htkConfig.segmentationExe;
                    string oldSegmentIniPath = oldSegmentDir + "\\" + htkConfig.segmentationIniFN;
                    string newSegmentIniPath = newSegmentDir + "\\" + htkConfig.segmentationIniFN;
                    //File.Copy(oldSegmentExePath, newSegmentExePath, true);
                    File.Copy(oldSegmentIniPath, newSegmentIniPath, true);
                    //Append optimum threshold and duration threshold info to segmentation ini file
                    string line = "#CALL THRESHOLDS FOR HMM AND QUALITY/DURATION\n" +
                                  "#    NOTE 1: HMM threshold is valid for HMM scores normalised to hit duration.\n" +
                                  "#    NOTE 2: Duration values in seconds.\n" +
                                  "#    NOTE 3: SD thrshold = number of SD either side of mean. 1.96=95% confidence\n" +
                                  "HTK_THRESHOLD=" + optimumThreshold+"\n"+
                                  "DURATION_MEAN=" + mean.ToString("f6") + "\n" +
                                  "DURATION_SD=" + sd.ToString("f6")  + "\n" +
                                  "SD_THRESHOLD=2.57";  //1.96 for p=95% :: 2.57 for p=99%
                    FileTools.Append2TextFile(newSegmentIniPath, line, false);


                    Console.WriteLine("\n\nWRITE HTK FILES");
                    try
                    {
                        //COPY HTK FILES ACROSS TO CONFIG DIR.
                        string oldHTKDir = htkConfig.HTKDir;
                        string newHTKDir = htkConfig.ConfigDir + "\\HTK";
                        //if (!Directory.Exists(newHTKDir)) 
                        string hcopyFN = Path.GetFileName(htkConfig.HCopyExecutable);
                        Directory.CreateDirectory(newHTKDir);
                        string oldHcopyFN = oldHTKDir + "\\" + hcopyFN;
                        string newHcopyFN = newHTKDir + "\\" + hcopyFN;
                        File.Copy(oldHcopyFN, newHcopyFN, true);
                        string hviteFN = Path.GetFileName(htkConfig.HViteExecutable);
                        string oldHviteFN = oldHTKDir + "\\" + hviteFN;
                        string newHviteFN = newHTKDir + "\\" + hviteFN;
                        File.Copy(oldHviteFN, newHviteFN, true);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("ERROR! FAILED TO WRITE HTK FILES");
                        Console.WriteLine(ex.ToString());
                        good = false;
                        //break;
                    }

                
                //ZIP THE CONFIG DIRECTORY
                    string Dir2Compress = htkConfig.ConfigDir;
                    string OutZipFile = htkConfig.ConfigDir + ".zip";
                    ZipUnzip.ZipDirectoryRecursive(Dir2Compress, OutZipFile, true);
                    Console.WriteLine("Zipped config placed in:- " + OutZipFile);
                    
                    #endregion

                    break;

            } //end SWITCH


            Console.WriteLine("FINISHED!");
            Console.ReadLine();
            //return good ? 0 : -1;

        }// end Main()












    } //end CLASS
}
