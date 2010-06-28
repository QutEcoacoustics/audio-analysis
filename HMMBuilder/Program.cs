using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using TowseyLib;
using AudioAnalysisTools.HTKTools;
using AudioAnalysisTools;
using System.Text.RegularExpressions;

namespace HMMBuilder
{
    public class Program
    {
        static void Main(string[] args)
        {
            #region Variables

            //string callIdentifier = "CURLEW2";                                              //arg[0]
            string callIdentifier = "CURRAWONG2";                                           //arg[0]
            //string callIdentifier = "KOALAMALE_EXHALE2";                                      //arg[0]
            string resourcesDir   = "C:\\SensorNetworks\\Templates\\";                        //arg[1]
            string htkExes        = "C:\\SensorNetworks\\Software\\Extra Assemblies\\HTK\\";  //arg[2]

            //GET PARAMETERS AND SET UP DIRECTORY STRUCTURE
            HTKConfig htkConfig = new HTKConfig(resourcesDir, callIdentifier);            
            BKGTrainer bkgModel = new BKGTrainer(htkConfig);

            //HTK DIR
            htkConfig.HTKDir = htkExes;

            Console.WriteLine("WORK DIR=" + htkConfig.WorkingDir);
            Console.WriteLine("CNFG DIR=" + htkConfig.ConfigDir);
            Console.WriteLine("DATA DIR=" + htkConfig.DataDir);

            string vocalization = "";
            string tmpVal       = "";
            string aOptionsStr  = htkConfig.aOptionsStr;
            string pOptionsStr  = ""; //-t 150.0"; //pruning option for HErest.exe BUT does not appear to make any difference

            bool htkTimestamps = false;

            bool good = true;
            int numIters    = htkConfig.numIterations;       //number of training iterations
            int numBkgIters = htkConfig.numBkgIterations;    //number of background training iterations
            #endregion

            switch (args.Length)
            {

                default:

                    #region ONE: Write Configuration Files
                    Console.WriteLine("1: WRITE ALL CONFIGURATION FILES");
                    try //to write config files
                    {
                        //1.create directories and write the MFCC, HMM files
                        htkConfig.ComputeFVSize();         //Compute Feature Vectors size given htkConfig.TARGETKIND
                        if(! Directory.Exists(htkConfig.ConfigDir))   Directory.CreateDirectory(htkConfig.ConfigDir);
                        if(! Directory.Exists(htkConfig.ProtoConfDir))Directory.CreateDirectory(htkConfig.ProtoConfDir);
                        htkConfig.WriteMfccConfigFile(htkConfig.MfccConfigFN);  //Write the mfcc file
                        htkConfig.WriteHmmConfigFile(htkConfig.ConfigFN);       //Write the dcf file
                        htkConfig.WritePrototypeFiles(htkConfig.ProtoConfDir);  //prototype files

                        //IMPORTANT: WRITE PROTOTYPE FILES FOR BIRD CALL OF INTEREST
                        //           ALSO COPY INI FILE TO THE TRAINING DATA DIRECTORIES
                        if (htkConfig.multisyllabic)
                        {
                            Console.WriteLine("PREPARING MULTI-SYLLABIC TEMPLATE");
                            //try  //Parse the grammar file: creates the word network file 'htkConfig.wordNet'
                            //{
                            //    HTKHelper.HParse(htkConfig.grammarF, htkConfig.wordNet, htkConfig);
                            //}
                            //catch
                            //{
                            //    Console.WriteLine("ERROR! FAILED TO CREATE NETWORK FILE: {0}", htkConfig.wordNet);
                            //    good = false;
                            //    break;
                            //}
                            //// 1. Populate syllable list
                            //htkConfig.PopulateSyllableList(htkConfig.wordNet);
                            //// 2. Create as many iniFiles as the number of syllables.
                            ////    Each, specifying the related vocalization to use for segmentation                            
                            //foreach (string word in htkConfig.multiSyllableList)
                            //{
                            //    //check if the training folder exists
                            //    string trnDir = htkConfig.trnDirPath + "\\" + word;
                            //    if (!Directory.Exists(trnDir))
                            //    {
                            //        Console.WriteLine("ERROR! Could not find folder '{0}'", trnDir);
                            //        throw new Exception();
                            //    }

                            //    //segmentationIniFile = trnDir + "\\" + HTKConfig.segmentationIniFN;
                            //    string tmpString = htkConfig.CallName;
                            //    htkConfig.CallName = word;
                            //    //htkConfig.WriteSegmentationIniFile(segmentationIniFile);
                            //    htkConfig.CallName = tmpString;
                           
                            //} // foreach
                        } // end if (multisyllabic)
                    } //end writing config files
                    catch
                    {
                        Console.WriteLine("ERROR! FAILED TO WRITE FIVE CONFIGURATION FILES");
                        good = false;
                        break;
                    }        
                    #endregion


                    #region TWO: DATA PREPARATION TOOLS:- Read Two Configuration Files and do Feature Extraction
                    Console.WriteLine("2: DATA PREPARATION TOOLS:- READ TWO CONFIGURATION FILES");
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

                    if (HMMSettings.ConfigParam.TryGetValue("TRACE_TOOL_CALLS", out tmpVal))
                    {
                        if (tmpVal.Equals("Y")) aOptionsStr = htkConfig.aOptionsStr; //aOptionsStr = "-A -D -T 1";
                    }

                    // HCOPY is the HTK tool to parameterise .wav files ie to extract mfcc features
                    // AT PRESENT WE ALWAYS DO FEATURE EXTRACTION
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
                    Console.WriteLine("\n3: ABOUT TO SEGMENT WAV TRAINING FILES");
                    try
                    {
                        if (! htkConfig.multisyllabic)
                        {
                           //copy segmentation ini file to the data directory.
                            System.IO.File.Copy(htkConfig.ConfigPath, htkConfig.trnDirPath + "\\" + HTKConfig.segmentationIniFN, true);
                            int verbosity = 1;
                            AudioSegmentation.Execute(htkConfig.trnDirPath, htkConfig.trnDirPath, verbosity);
                        }
                        else // multisyllabic call
                        {
                            int verbosity = 0;
                            foreach (string word in htkConfig.multiSyllableList)
                            {
                                string trnDir = htkConfig.trnDirPath + "\\" + word;
                                AudioSegmentation.Execute(trnDir, trnDir, verbosity);
                            }
                        }
                        HTKHelper.CreateWLT(htkConfig, ref vocalization, true);
                    }
                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine(e.Message);
                        good = false;
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
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
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE Write Dictionary Region");
                        good = false;
                        break;
                    }
                    #endregion

                    #region FIVE: TRAINING TOOLS:-  (see manual 2.3.2)
                    Console.WriteLine("\n5: TRAINING: READING THE PROTOTYPE CONFIGURATION FILES");
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

                    Console.WriteLine("\nTRAINING: HMM Model re-estimation");                   

                    try
                    {
                        if (HMMSettings.ConfigParam.TryGetValue("HEREST", out tmpVal))
                            if (!tmpVal.Equals("Y") && htkConfig.doSegmentation)
                            {
                                if (!htkConfig.LLRusesBKG) //use SIL model and train both SIL and WORD models on 'our' timestamps
                                {
                                    //1-estimate SIL model
                                    HTKHelper.HRest(aOptionsStr, htkConfig, "SIL");

                                    string tmpSrcLine = "";                                    
                                    //2-copy the SIL info into the BKG model folder for LLR normalization
#if !TEMP_TEST
                                    if (htkConfig.LLRNormalization)
                                    {
                                        //copy model file
                                        string sourceF = Path.Combine(htkConfig.tgtDir2, htkConfig.hmmdefFN);
                                        string destF = Path.Combine(htkConfig.tgtDir2Bkg, htkConfig.hmmdefFN);
                                        StreamReader sourceReader = new StreamReader(sourceF);
                                        StreamWriter destWriter = new StreamWriter(destF);                                        
                                        while ((tmpSrcLine = sourceReader.ReadLine()) != null)
                                        {
                                            if (tmpSrcLine.StartsWith("~h"))
                                                tmpSrcLine = "~h \"BACKGROUND\"";
                                            destWriter.WriteLine(tmpSrcLine);
                                        }
                                        sourceReader.Close();
                                        destWriter.Close();

                                        //copy macros file
                                        sourceF = Path.Combine(htkConfig.tgtDir2, htkConfig.macrosFN);
                                        destF = Path.Combine(htkConfig.tgtDir2Bkg, htkConfig.macrosFN);
                                        File.Copy(sourceF, destF, true);
                                    }
#endif

                                    //3-make a temporary copy of SIL model
                                    FileTools.BackupFile(htkConfig.tgtDir2 + "\\" + htkConfig.hmmdefFN);
                                    //4-estimate WORD model
                                    HTKHelper.HRest(aOptionsStr, htkConfig, htkConfig.CallName);
#if TEMP_TEST
                                    if (htkConfig.LLRNormalization)
                                    {
                                        //copy model file
                                        string sourceF = Path.Combine(htkConfig.tgtDir2, htkConfig.hmmdefFN);
                                        string destF = Path.Combine(htkConfig.tgtDir2Bkg, htkConfig.hmmdefFN);
                                        StreamReader sourceReader = new StreamReader(sourceF);
                                        StreamWriter destWriter = new StreamWriter(destF);
                                        while ((tmpSrcLine = sourceReader.ReadLine()) != null)
                                        {
                                            if (tmpSrcLine.StartsWith("~h"))
                                                tmpSrcLine = "~h \"BACKGROUND\"";
                                            destWriter.WriteLine(tmpSrcLine);
                                        }
                                        sourceReader.Close();
                                        destWriter.Close();

                                        //copy macros file
                                        sourceF = Path.Combine(htkConfig.tgtDir2, htkConfig.macrosFN);
                                        destF = Path.Combine(htkConfig.tgtDir2Bkg, htkConfig.macrosFN);
                                        File.Copy(sourceF, destF, true);
                                    }
#endif
                                    //5-merge models
                                    //The 'tmp' dir contains a copy of the estimated SIL model
                                    string dstHmm = htkConfig.tgtDir2 + "\\" + htkConfig.hmmdefFN;
                                    string srcHmm = htkConfig.tgtDir2 + "\\copy_of_" + htkConfig.hmmdefFN;

                                    StreamReader srcFileReader = File.OpenText(srcHmm);
                                    string bodyToAdd = "";
                                    bool validLines = false;
                                    while ((tmpSrcLine = srcFileReader.ReadLine()) != null)
                                    {
                                        if (tmpSrcLine.StartsWith("~h"))
                                            validLines = true;
                                        
                                        if (validLines)
                                            bodyToAdd += tmpSrcLine + "\n";
                                    }
                                    FileTools.Append2TextFile(dstHmm, bodyToAdd, false);

                                }
                                else //use BKG model and train the WORD model on our timestamps
                                    HTKHelper.HRest(aOptionsStr, htkConfig, htkConfig.CallName);
                            }
                            else
                            {
                                htkTimestamps = true; //HTK will line up SIL and WORD timestamps

                                if (HMMSettings.ConfigParam.TryGetValue("HEREST_ITER", out tmpVal))
                                {
                                    numIters = int.Parse(tmpVal);
                                    if (numIters <= 0) //backward compatibility
                                        numIters = 3;
                                }
                                else
                                {
                                    Console.WriteLine("'HEREST_ITER' parameter not specified. Default value (3) will be used.");
                                    numIters = 3;
                                }
                                Console.WriteLine("\n\nNumber of iterations for re-estimating the VOCALIZATION/SIL models set to : " + numIters);

                                HTKHelper.HERest(numIters, aOptionsStr, pOptionsStr, htkConfig);

                                //copy the SIL info into the BKG model folder for LLR normalization
                                if (!htkConfig.LLRusesBKG && htkConfig.LLRNormalization && htkConfig.doSegmentation) 
                                { 
                                    //1-extract SIL model and copy it into the BKG folder
                                    string sourceF = Path.Combine(htkConfig.tgtDir2, htkConfig.hmmdefFN);
                                    string destF = Path.Combine(htkConfig.tgtDir2Bkg, htkConfig.hmmdefFN);
                                    StreamReader sourceReader = new StreamReader(sourceF);
                                    StreamWriter destWriter = new StreamWriter(destF);
                                    string tmpSrcLine = "";
                                    bool validLines = true;
                                    while ((tmpSrcLine = sourceReader.ReadLine()) != null)
                                    {
                                        if (tmpSrcLine.StartsWith("~h"))
                                        {
                                            string[] param = Regex.Split(tmpSrcLine, @"\s+");
#if !TEMP_TEST
                                            if (param[1].Equals("\"SIL\""))
#else
                                            if (param[1].Equals("\""+htkConfig.CallName+"\""))
#endif
                                            {
                                                tmpSrcLine = "~h \"BACKGROUND\"";
                                                validLines = true;
                                            }
                                            else
                                                validLines = false;
                                        }
                                        if (validLines) destWriter.WriteLine(tmpSrcLine);
                                    }
                                    sourceReader.Close();
                                    destWriter.Close();

                                    //2-copy macros file to the BKG folder
                                    sourceF = Path.Combine(htkConfig.tgtDir2, htkConfig.macrosFN);
                                    destF = Path.Combine(htkConfig.tgtDir2Bkg, htkConfig.macrosFN);
                                    File.Copy(sourceF, destF, true);                                    
                                }
                            }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE PARAMETERS ESTIMATION");
                        Console.WriteLine(ex);
                        good = false;
                        break; 
                    }
                    #endregion

                    #region SIX: Background Model Estimation
                    Console.WriteLine("\n6: Estimation of Background Noise Model");
                    if (HMMSettings.ConfigParam.TryGetValue("HERESTBKG", out tmpVal))
                    {
                        if (tmpVal.Equals("Y"))
                        {
                            Console.WriteLine("\n\nEstimating the BACKGROUND model ...");                           

                            if (HMMSettings.ConfigParam.TryGetValue("HERESTBKG_ITER", out tmpVal))
                            {
                                numBkgIters = int.Parse(tmpVal);
                                if(numBkgIters<=0)
                                    numBkgIters = 3;
                            }
                            else
                            {
                                numBkgIters = 3;
                            }
                            Console.WriteLine("\n\nNumber of iterations for estimating the BACKGROUND model set to: " + numBkgIters);

                            //Estimate BACKGROUND model
                            try
                            {
                                htkConfig.bkgTraining = true;  //BACKGROUND training mode on
                                bkgModel.EstimateModel();
                                htkConfig.bkgTraining = false; //BACKGROUND training mode off 
                            }
                            catch
                            {
                                Console.WriteLine("Failed to estimate the BACKGROUND model.");
                                good = false;
                                break;
                            }
                        }
                        else //the BG model does not need to be re-trained
                        {
                            htkConfig.bkgTraining = false;
                            //TO DO: check if the BKG HMM exists
                        }
                    }
                    #endregion

                    #region SEVEN: Test the HMMs
                    Console.WriteLine("\n7: Test the HMMs");
                    try
                    {
                        if (HMMSettings.ConfigParam.TryGetValue("HBUILD", out tmpVal))
                        {
                            if (tmpVal.Equals("Y")) //Generate the network file
                            {
                                if (!htkConfig.multisyllabic)
                                    HTKHelper.HBuild(htkConfig.monophones, htkConfig.wordNet, htkConfig.HBuildExecutable);
                            }
                            else
                            {
                                if (!htkConfig.multisyllabic)
                                {
                                    //TO DO: Ask the user for the word network file
                                }
                            }
                        }
                        else
                        {
                            //TO DO: Ask the user for the word network file
                        }
                        
                        //Use the BACKGROUND model rather than the SIL model. 
                        if (! htkConfig.doSegmentation || (htkConfig.doSegmentation && htkConfig.LLRusesBKG && !htkTimestamps))
                        {
                            //The 'tmp' dir contains a copy of the estimated BKG model
                            string dstHmm = htkConfig.tgtDir2 + "\\" + htkConfig.hmmdefFN;
                            string srcHmm = htkConfig.tgtDir2Bkg + "\\" + htkConfig.hmmdefFN;
                            
                            StreamReader srcFileReader = File.OpenText(srcHmm);
                            string tmpSrcLine = ""; string bodyToAdd = "";
                            bool validLines = false;
                            while ((tmpSrcLine = srcFileReader.ReadLine()) != null)
                            {
                                if (tmpSrcLine.StartsWith("~h")) //turn 'BACKGROUND' into 'SIL'
                                {
                                    tmpSrcLine = "~h \"SIL\"";
                                    validLines = true;
                                }
                                if (validLines)
                                    bodyToAdd += tmpSrcLine + "\n";
                            }
                            FileTools.Append2TextFile(dstHmm, bodyToAdd, false);
                        }

                        //True calls
                        HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.tTrueF, htkConfig.wordNet,
                                        htkConfig.DictFile, htkConfig.resultTrue, htkConfig.monophones, htkConfig.HViteExecutable);
                        //False calls
                        HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2, htkConfig.tFalseF, htkConfig.wordNet,
                                        htkConfig.DictFile, htkConfig.resultFalse, htkConfig.monophones, htkConfig.HViteExecutable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("ERROR!! FAILED TO COMPLETE HTKHelper.HVite()");
                        good = false;
                        break; 
                    }
                    #endregion

                    #region EIGHT: Score BACKGROUND model
                    Console.WriteLine("\n8: Score Background Noise Model");
                    //Modify the output from HVite so as to include the scores from background model
                    if (! htkConfig.doSegmentation || (htkConfig.doSegmentation && htkConfig.LLRNormalization))
                    {
                        //Generate the word network for the backgroung word
                        HTKHelper.HBuild(htkConfig.monophonesBkg, htkConfig.wordNetBkg, htkConfig.HBuildExecutable);
                        //Score the BCK model on the true set
                        bkgModel.ScoreModel(true);
                        //Score the BCK model on the false set
                        bkgModel.ScoreModel(false);
                        //Copy the model files to the working directory
                        string newBKGDir = htkConfig.ConfigDir + htkConfig.HmmDirBkgLLR;
                        if (Directory.Exists(newBKGDir))
                            Directory.Delete(newBKGDir, true);
                        Directory.CreateDirectory(newBKGDir);
                        string sourceFN = htkConfig.tgtDir2Bkg + "\\" + htkConfig.macrosFN;
                        string destFN = newBKGDir + "\\" + htkConfig.macrosFN;
                        File.Copy(sourceFN, destFN, true);
                        sourceFN = htkConfig.tgtDir2Bkg + "\\" + htkConfig.hmmdefFN;
                        destFN = newBKGDir + "\\" + htkConfig.hmmdefFN;
                        File.Copy(sourceFN, destFN, true);
                        sourceFN = htkConfig.wordNetBkg;
                        destFN = newBKGDir + "\\phone.net";
                        File.Copy(sourceFN, destFN, true);
                        sourceFN = htkConfig.DictFileBkg;
                        destFN = newBKGDir + "\\dict";
                        File.Copy(sourceFN, destFN, true);
                        sourceFN = htkConfig.monophonesBkg;
                        destFN = newBKGDir + "\\labelList.txt";
                        File.Copy(sourceFN, destFN, true);
                     }
                    #endregion

                    #region NINE: Accuracy Measurements: Accuracy = (TruePositives + TrueNegative)/TotalSamples
                    Console.WriteLine("\n9: Accuracy Measurements");

                    //calculate frame rate = 1sec / frame duration
                    double frameRate = 10000000 / double.Parse(htkConfig.TARGETRATE);
                    Console.WriteLine("\nFrame rate = "+ frameRate+"\n");

                    //calculate the TRUE mean and sd of the training call durations
                    string masterLabelFile = htkConfig.ConfigDir + "\\phones.mlf";
                    double durationMean = 0;
                    double trnSD = 0;
                    string regex = null;
                    int optimumThreshold = -Int32.MaxValue; //to be removed from here
                    if (!htkConfig.multisyllabic)
                    {
                        Helper.AverageCallDuration(htkConfig, masterLabelFile, regex, vocalization, out durationMean, out trnSD);

                        Console.WriteLine("Training song durations= " + durationMean.ToString("f4") + "+/-" + trnSD.ToString("f4") + " seconds or " +
                                          (durationMean * frameRate).ToString("f1") + " frames\n");

                        //calculate the PROBABLE mean and sd of the testing call durations
                        masterLabelFile = htkConfig.WorkingDir + "\\results\\recountTrue.mlf";
                        double mean2;
                        double sd2;
                        regex = @"^\d+\s+\d+\s+\w+";
                        Helper.AverageCallDuration(htkConfig, masterLabelFile, regex, vocalization, out mean2, out sd2);
                        Console.WriteLine("Testing song durations= " + mean2.ToString("f4") + "+/-" + sd2.ToString("f4") + " seconds or " +
                                          (mean2 * frameRate).ToString("f1") + " frames\n");


                        //Read the output files
                        int tpCount = 0;  //true positives
                        int fpCount = 0;  //false positives
                        int trueSCount = 0;
                        int falseSCount = 0;
                        try
                        {
                            float tppercent = 0.0f;
                            float tnpercent = 0.0f;
                            float accuracy = 0.0f;
                            float avTPScore = 0.0f;
                            float avFPScore = 0.0f;
                            float threshold = 0.0f;

                            //set a central threshold value suitable to create a ROC curve  
                            if (!htkConfig.doSegmentation || (htkConfig.doSegmentation && htkConfig.LLRNormalization))
                                threshold = 2000; //2500;  for noSegmentation 
                            else
                                threshold = -50;  //-50

                            int step = 50; //to step the threshold
                            double maxScore = -Double.MaxValue;
                            int maxTpCount = 0;
                            int maxTnCount = 0;
                            for (int i = 9; i >= -8; i--)
                            {
                                trueSCount = 0;
                                falseSCount = 0;
                                float t = threshold - (i * step);

                                if (!htkConfig.doSegmentation || (htkConfig.doSegmentation && htkConfig.LLRNormalization))
                                    Helper.ComputeAccuracy2(htkConfig.modifResultTrue, htkConfig.modifResultFalse, durationMean, trnSD, frameRate,
                                                  vocalization, t, out tpCount, out fpCount, out trueSCount, out falseSCount,
                                                  out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);
                                else
                                    Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, durationMean, trnSD, frameRate,
                                                      vocalization, t, out tpCount, out fpCount, out trueSCount, out falseSCount,
                                                      out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);

                                Console.WriteLine("TP={0:f1}\tTN={1:f1}\tAcc={2:f1}%\tavTPscore={3:f0}\tavFPscore={4:f0} \t(threshold={5})", tppercent, tnpercent, accuracy, avTPScore, avFPScore, t);
                                if (accuracy > maxScore)
                                {
                                    maxScore = accuracy;
                                    optimumThreshold = (int)t;
                                    maxTpCount = tpCount;
                                    maxTnCount = falseSCount - fpCount;
                                }
                            }
                            //calculate optimum so can save best data.
                            Console.WriteLine("Max score = " + maxScore + "  at threshold= " + optimumThreshold);
                            Console.WriteLine("TP=" + maxTpCount + "/" + trueSCount + "  TN=" + maxTnCount + "/" + falseSCount);
                            Console.WriteLine("FN=" + (trueSCount - maxTpCount) + "     FP=" + (falseSCount - maxTnCount));
                            //repeat in order to print the PDF file of individual results

                            if (!htkConfig.doSegmentation || (htkConfig.doSegmentation && htkConfig.LLRNormalization))
                                Helper.ComputeAccuracy2(htkConfig.modifResultTrue, htkConfig.modifResultFalse, durationMean, trnSD, frameRate,
                                                vocalization, optimumThreshold, out tpCount, out fpCount, out trueSCount, out falseSCount,
                                                out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);
                            else
                                Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, durationMean, trnSD, frameRate,
                                                    vocalization, optimumThreshold, out tpCount, out fpCount, out trueSCount, out falseSCount,
                                                    out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);


                            Console.WriteLine("You can check individual hits in the template's results directory.");

                            //Append threshold and quality info to ini file
                            //htkConfig.AppendThresholdInfo2IniFile(vocalization, optimumThreshold, durationMean, trnSD);

                        }
                        catch
                        {
                            good = false;
                            return;
                        }
                    }
                    else //multisyllabic case
                    {
                        
                        foreach (string syllName in htkConfig.multiSyllableList)
                        {
                            
                            //TO DO: manage acc meas for multisyllabic calls
                            Helper.AverageCallDuration(htkConfig, masterLabelFile, regex, syllName, out durationMean, out trnSD);
                            Console.WriteLine("Training song durations for '" + syllName + "' = " 
                                                    + durationMean.ToString("f4") + "+/-" 
                                                    + trnSD.ToString("f4") 
                                                    + " seconds or " 
                                                    + (durationMean * frameRate).ToString("f1") + " frames\n");
                            
                            //calculate the mean and sd of the testing call durations
                            string tmpMlf = masterLabelFile;
                            masterLabelFile = htkConfig.WorkingDir + "\\results\\recountTrue.mlf";
                            double mean2;
                            double sd2;
                            regex = @"^\d+\s+\d+\s+\w+";
                            Helper.AverageCallDuration(htkConfig, masterLabelFile, regex, syllName, out mean2, out sd2);
                            
                            Console.WriteLine("Testing song durations for '" + syllName + "' = " 
                                                    + mean2.ToString("f4") + "+/-" 
                                                    + sd2.ToString("f4") 
                                                    + " seconds or " 
                                                    + (mean2 * frameRate).ToString("f1") + " frames\n");

                            masterLabelFile = tmpMlf;
                            //Read the output files
                            int tpCount = 0;  //true positives
                            int fpCount = 0;  //false positives
                            int trueSCount = 0;
                            int falseSCount = 0;
                            try
                            {
                                float tppercent = 0.0f;
                                float tnpercent = 0.0f;
                                float accuracy = 0.0f;
                                float avTPScore = 0.0f;
                                float avFPScore = 0.0f;
                                //float threshold = -50f;      //set a central threshold value suitable to create a ROC curve  
                                //check if the threshold has been defined
                                int threshold = 0;
                                if (!htkConfig.threshold.TryGetValue(syllName, out threshold))
                                {
                                    Console.WriteLine("No Score Threshold defined for '{0}'", syllName);
                                    throw new Exception();
                                }
                                    
                                int step = 2; //to step the threshold
                                double maxScore = -Double.MaxValue;
                                int maxTpCount = 0;
                                int maxTnCount = 0;
                                for (int i = 9; i >= -8; i--)
                                {
                                    trueSCount = 0;
                                    falseSCount = 0;
                                    float t = threshold - (i * step);

                                    
                                    Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, durationMean, trnSD, frameRate,
                                                      syllName, t, out tpCount, out fpCount, out trueSCount, out falseSCount,
                                                      out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);
                                    Console.WriteLine("TP={0:f1}\tTN={1:f1}\tAcc={2:f1}%\tavTPscore={3:f0}\tavFPscore={4:f0} \t(threshold={5})", tppercent, tnpercent, accuracy, avTPScore, avFPScore, t);
                                    if (accuracy > maxScore)
                                    {
                                        maxScore = accuracy;
                                        optimumThreshold = (int)t; //########################################
                                        maxTpCount = tpCount;
                                        maxTnCount = falseSCount - fpCount;
                                    }
                                }
                                //calculate optimum so can save best data.
                                Console.WriteLine("Max score = " + maxScore + "  at threshold= " + optimumThreshold);
                                Console.WriteLine("TP=" + maxTpCount + "/" + trueSCount + "  TN=" + maxTnCount + "/" + falseSCount);
                                Console.WriteLine("FN=" + (trueSCount - maxTpCount) + "     FP=" + (falseSCount - maxTnCount));
                                //repeat in order to print the PDF file of individual results

                                Helper.ComputeAccuracy(htkConfig.resultTrue, htkConfig.resultFalse, durationMean, trnSD, frameRate,
                                       syllName, optimumThreshold, out tpCount, out fpCount, out trueSCount, out falseSCount,
                                       out tppercent, out tnpercent, out accuracy, out avTPScore, out avFPScore);

                                Console.WriteLine("Check individual hits in the template's results directory.####################");

                                //Append threshold and quality info to ini file
                                //htkConfig.AppendThresholdInfo2IniFile(syllName, optimumThreshold, durationMean, trnSD);

                            }
                            catch
                            {
                                good = false;
                                return;
                            }

                        } //end multi-syllable list
                    }
                    #endregion

                    #region TEN: SET UP THE TEMPLATE ZIP FILE
                    Console.WriteLine("\n\n10: WRITE HTK FILES");

                    //WRITE CONFIG FILE
                    string iniPath = htkConfig.ConfigDir + "\\" + HTKConfig.segmentationIniFN;
                    htkConfig.WriteSegmentationIniFile(iniPath);
                    htkConfig.AppendThresholdInfo2IniFile(iniPath, vocalization, optimumThreshold, durationMean, trnSD);

                    //Console.WriteLine(" {0} {1} {2} {3} ", vocalization, optimumThreshold, durationMean, trnSD);

                    //try
                    //{
                    //    //COPY SILENCE MODEL FILES TO CONFIG DIR
                    //    string oldNoiseDir = Path.GetDirectoryName(htkConfig.SilenceModelPath);
                    //    string noiseModelFN = Path.GetFileNameWithoutExtension(htkConfig.SilenceModelPath);
                    //    string ext = HTKConfig.noiseModelExt;
                    //    string oldNoiseModel = oldNoiseDir + "\\" + noiseModelFN + ext;
                    //    string newNoiseModel = htkConfig.ConfigDir + "\\" + noiseModelFN + ext;
                    //    File.Copy(oldNoiseModel, newNoiseModel, true);
                    //}
                    //catch (IOException ex)
                    //{
                    //    Console.WriteLine("ERROR! FAILED TO COPY SILENCE FILES");
                    //    Console.WriteLine(ex.ToString());
                    //    good = false;
                    //}

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
