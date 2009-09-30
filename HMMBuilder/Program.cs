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

            //htkConfig.CallName = "CURLEW1";
            //htkConfig.Comment = "Parameters for Curlew";
            //htkConfig.LOFREQ = "600";
            //htkConfig.HIFREQ = "8000"; //try 6000, 7000 and 8000 Hz as max for Curlew
            //float threshold = -2000f;  //default = 1900


            htkConfig.CallName = "CURRAWONG1";
            htkConfig.Comment = "Parameters for Currawong";
            htkConfig.LOFREQ = "800";
            htkConfig.HIFREQ = "8000";     //try 6000, 7000 and 8000 Hz
            htkConfig.numHmmStates = "6";  //number of hmm states for call model
            float threshold = -1900f;      //magic number for Currawong is -1900f


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

            htkConfig.WorkingDir  = Directory.GetCurrentDirectory();
            htkConfig.TemplateDir = "C:\\SensorNetworks\\Templates\\Template_" + htkConfig.CallName;
            htkConfig.DataDir     = htkConfig.TemplateDir  + "\\data";
            htkConfig.ConfigDir   = htkConfig.TemplateDir  + "\\config_" + htkConfig.CallName;
            htkConfig.ResultsDir  = htkConfig.TemplateDir  + "\\results";
            htkConfig.SilenceModelFN = htkConfig.ConfigDir + "\\SilenceModel\\West_Knoll_St_Bees_Currawong1_20080923-120000.wav\n";
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
                        htkConfig.WriteSegmentationIniFile(htkConfig.SegmentationIniFN);
                        string fn = System.IO.Path.GetFileName(htkConfig.SegmentationIniFN);
                        System.IO.File.Copy(htkConfig.SegmentationIniFN, htkConfig.trnDirPath + "\\" + fn,true);
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
                    Console.WriteLine("Number of iterations for training set = " + numIters);

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
                        HTKHelper.WriteDictionary(htkConfig);
                    }
                    catch
                    {
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
                        good = false;
                        break; 
                    }
                    #endregion


                    #region Test the HMMs
                    try
                    {
                        if (HMMSettings.ConfigParam.TryGetValue("HBUILD", out tmpVal))
                        {
                            if (tmpVal.Equals("Y")) //Generate the network file
                            {
                                HTKHelper.HBuild(htkConfig.monophones, htkConfig.wordNet);
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
                                        htkConfig.DictFile, htkConfig.resultTrue, htkConfig.monophones);
                        //False calls
                        HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.tFalseF, htkConfig.wordNet,
                                        htkConfig.DictFile, htkConfig.resultFalse, htkConfig.monophones);
                    }
                    catch
                    {
                        //Console.WriteLine(ex.ToString());
                        good = false;
                        break; 
                    }
                    #endregion


                    #region Accuracy Measurements: Accuracy = (TruePositives + TrueNegative)/TotalSamples
                
                    //Read the output files
                    try
                    {
                        float accuracy = Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, ref vocalization, threshold);
                        Console.WriteLine("System Accuracy: {0}%", accuracy*100);
                    }
                    catch 
                    {
                        good = false;
                        break;  
                    }

                    #endregion

                    break;

            } //end SWITCH

            //ZIP THE CONFIG DIRECTORY
            string Dir2Compress = htkConfig.ConfigDir;
            string OutZipFile   = htkConfig.ConfigDir + ".zip";
            ZipUnzip.ZipDirectoryRecursive(Dir2Compress, OutZipFile, true);
            Console.WriteLine("Zipped config placed in:- " + OutZipFile);



            Console.WriteLine("FINISHED!");
            Console.ReadLine();
            return good ? 0 : -1;

        }// end Main()

    } //end CLASS
}
