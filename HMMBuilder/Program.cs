using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace HMMBuilder
{
    class Program
    {
        static int Main(string[] args)
        {
            #region Variables

            HTKConfig htkConfig = new HTKConfig();

            htkConfig.CallName = "CURLEW1";
            htkConfig.Comment = "Parameters for Curlew";
            htkConfig.LOFREQ = "1000";
            htkConfig.HIFREQ = "5000"; //try 6000, 7000 and 8000 Hz as max for Curlew
            htkConfig.numHmmStates = "6";  //number of hmm states for call model
            float threshold = -5000f;  //default = 1900


            //htkConfig.CallName = "CURRAWONG1";
            //htkConfig.Comment = "Parameters for Currawong";
            //htkConfig.LOFREQ = "800";
            //htkConfig.HIFREQ = "8000";     //try 6000, 7000 and 8000 Hz
            //htkConfig.numHmmStates = "6";  //number of hmm states for call model
            //float threshold = -1900f;      //magic number for Currawong is -1900f


            //==================================================================================================================
            //==================================================================================================================


            htkConfig.Author       = "Michael Towsey";
            htkConfig.SOURCEFORMAT = "WAV";
            htkConfig.TARGETKIND   = "MFCC";

            //FRAMING PARAMETERS
            htkConfig.SampleRate     = "22050";    //samples per second //this must be put first inlist of framing parameters
            htkConfig.TARGETRATE     = "116100.0"; //x10e-7 seconds
            htkConfig.WINDOWDURATION = "232200.0"; //=23.22 milliseconds

            double tr;
            Double.TryParse(htkConfig.TARGETRATE, out tr);
            double wd;
            Double.TryParse(htkConfig.WINDOWDURATION, out wd);
            int sr;
            Int32.TryParse(htkConfig.SampleRate, out sr);

            htkConfig.FRAMESIZE = (Math.Floor(wd / 10000000 * sr)).ToString(); 
            htkConfig.FrameOverlap = (tr / wd).ToString();
            htkConfig.SAVECOMPRESSED = "T";
            htkConfig.SAVEWITHCRC = "T";

            //MFCC PARAMETERS
            htkConfig.USEHAMMING = "T";
            htkConfig.PREEMCOEF  = "0.97";
            htkConfig.NUMCHANS   = "26";
            htkConfig.CEPLIFTER  = "22";
            htkConfig.NUMCEPS    = "12";

            //htkConfig.WorkingDir  = Directory.GetCurrentDirectory();
            htkConfig.WorkingDir  = "C:\\SensorNetworks\\Templates\\Template_" + htkConfig.CallName;
            htkConfig.HTKDir      = "C:\\SensorNetworks\\Software\\HTK";
            htkConfig.SegmentationDir = "C:\\SensorNetworks\\Software\\HMMBuilder\\VocalSegmentation";
            htkConfig.DataDir     = htkConfig.WorkingDir + "\\data";
            htkConfig.ConfigDir   = htkConfig.WorkingDir  + "\\" + htkConfig.CallName;
            htkConfig.ResultsDir  = htkConfig.WorkingDir  + "\\results";
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
                    #region ONE: WRITE Two Configuration Files
                    Console.WriteLine("WRITE FIVE CONFIGURATION FILES");
                    try
                    {
                        if(! Directory.Exists(htkConfig.ConfigDir))   Directory.CreateDirectory(htkConfig.ConfigDir);
                        if(! Directory.Exists(htkConfig.ProtoConfDir))Directory.CreateDirectory(htkConfig.ProtoConfDir);
                        htkConfig.WriteMfccConfigFile(htkConfig.MfccConfigFN);  //Write the mfcc file
                        htkConfig.WriteHmmConfigFile(htkConfig.ConfigFN);       //Write the dcf file
                        htkConfig.WritePrototypeFiles(htkConfig.ProtoConfDir);  //prototype files
                        //segmentation ini file
                        string segmentationIniFile = htkConfig.SegmentationDir + "\\" + htkConfig.segmentationIniFN;
                        htkConfig.WriteSegmentationIniFile(segmentationIniFile);
                        string fn = System.IO.Path.GetFileName(segmentationIniFile);
                        System.IO.File.Copy(segmentationIniFile, htkConfig.trnDirPath + "\\" + fn, true);
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


                    #region Data Preparation (see manual 2.3.1):- Segment the training data; Get PHONE LABELS
                    try
                    {
                        bool extractLabels = true;
                        HTKHelper.CreateWLT(htkConfig, ref vocalization, extractLabels);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE Data Preparation Region");
                        good = false;
                        break; 
                    }
                    #endregion


                    #region WriteDictionary consisting of PHONE LABELS
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


                    #region nTRAINING TOOLS:-  (see manual 2.3.2)
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


                    #region THREE Test the HMMs
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


                    #region Accuracy Measurements: Accuracy = (TruePositives + TrueNegative)/TotalSamples
                
                    //Read the output files
                    try
                    {
                        float tppercent = 0.0f;
                        float tnpercent = 0.0f;
                        Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, ref vocalization, threshold-2000, out tppercent, out tnpercent);
                        float accuracy = tppercent + tnpercent;
                        Console.WriteLine("TP={0:f1} TN={1:f1} Acc= {2:f1}%   at threshold={3}", tppercent * 100, tnpercent * 100, accuracy * 100, threshold - 2000);

                        Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, ref vocalization, threshold - 1000, out tppercent, out tnpercent);
                        accuracy = tppercent + tnpercent;
                        Console.WriteLine("TP={0:f1} TN={1:f1} Acc= {2:f1}%   at threshold={3}", tppercent * 100, tnpercent * 100, accuracy * 100, threshold - 1000);
                        
                        Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, ref vocalization, threshold, out tppercent, out tnpercent);
                        accuracy = tppercent + tnpercent;
                        Console.WriteLine("TP={0:f1} TN={1:f1} Acc= {2:f1}%   at threshold={3}", tppercent * 100, tnpercent * 100, accuracy * 100, threshold);
                        
                        Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, ref vocalization, threshold + 1000, out tppercent, out tnpercent);
                        accuracy = tppercent + tnpercent;
                        Console.WriteLine("TP={0:f1} TN={1:f1} Acc= {2:f1}%   at threshold={3}", tppercent * 100, tnpercent * 100, accuracy * 100, threshold + 1000);
                        
                        Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, ref vocalization, threshold + 2000, out tppercent, out tnpercent);
                        accuracy = tppercent + tnpercent;
                        Console.WriteLine("TP={0:f1} TN={1:f1} Acc= {2:f1}%   at threshold={3}", tppercent * 100, tnpercent * 100, accuracy * 100, threshold + 2000);
                    }
                    catch 
                    {
                        good = false;
                        break;  
                    }

                    #endregion



                    #region SET UP THE TEMPLATE ZIP FILE
                    //COPY SILENCE MODEL FILES TO CONFIG DIR
                    string newSilenceDir = htkConfig.ConfigDir + "\\SilenceModel";
                    Directory.CreateDirectory(newSilenceDir);
                    string silenceWavFN = Path.GetFileName(htkConfig.SilenceModelPath);
                    string newSilencePath = newSilenceDir + "\\" + silenceWavFN;
                    File.Copy(htkConfig.SilenceModelPath, newSilencePath, true);

                    string oldNoiseDir  = Path.GetDirectoryName(htkConfig.SilenceModelPath);
                    string noiseModelFN = Path.GetFileNameWithoutExtension(htkConfig.SilenceModelPath);
                    string ext = htkConfig.noiseModelExt;
                    string oldNoiseModel = oldNoiseDir   + "\\" + noiseModelFN + ext;
                    string newNoiseModel = newSilenceDir + "\\" + noiseModelFN + ext;
                    File.Copy(oldNoiseModel, newNoiseModel, true);

                    //COPY SEGMENTATION FILES TO CONFIG DIR
                    string oldSegmentDir = htkConfig.SegmentationDir;
                    string newSegmentDir = htkConfig.ConfigDir + "\\Segmentation";
                    string oldSegmentExePath = oldSegmentDir + "\\" + htkConfig.segmentationExe;
                    string newSegmentExePath = newSegmentDir + "\\" + htkConfig.segmentationExe;
                    string oldSegmentIniPath = oldSegmentDir + "\\" + htkConfig.segmentationIniFN;
                    string newSegmentIniPath = newSegmentDir + "\\" + htkConfig.segmentationIniFN;
                    Directory.CreateDirectory(newSegmentDir);
                    File.Copy(oldSegmentExePath, newSegmentExePath, true);
                    File.Copy(oldSegmentIniPath, newSegmentIniPath, true);


                    Console.WriteLine("WRITE HTK FILES");
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
            return good ? 0 : -1;

        }// end Main()

    } //end CLASS
}
