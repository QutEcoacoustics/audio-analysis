using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using TowseyLib;


namespace AudioAnalysisTools.HTKTools
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public static class HTKHelper
    {
        #region Variables

        const int ERROR_FILE_NOT_FOUND = 2;
        private static List<string> syllableList = new List<string>(); //list of words/syllables/phones to recognise

        public static List<string> SyllableList
        {
            get { return syllableList;  }  
        }

        #endregion

        #region HTK Helpers

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it’s new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        } //end method CopyAll()


        public static void SegmentDataFiles(HTKConfig htkConfig, ref string vocalization)
        {
            string segmentationExecutable = htkConfig.SegmentationDir + "\\" + HTKConfig.segmentationExe;
            string segmentationIniFile    = htkConfig.SegmentationDir + "\\" + HTKConfig.segmentationIniFN;

            string cmdLineArgument = Path.GetFullPath(htkConfig.trnDirPath);//get dir contining training data
            cmdLineArgument = "\"" + cmdLineArgument + "\""; //enclose line in quotes in case have sapce
            //Console.WriteLine(" commandLine=" + cmdLineArgument);

            //check that the directory contains a file called "segmentation.ini"`
            if (File.Exists(segmentationIniFile))
            {
                Console.WriteLine(" Found segmentIni file: " + segmentationIniFile);
            }
            else
              throw new Exception(" The directory <" + cmdLineArgument + "> must contain a file called " + segmentationIniFile);
            
            if (File.Exists(segmentationExecutable))
            {
                Console.WriteLine(" Found executable file: " + segmentationExecutable);
            }
            else
            {
                Console.WriteLine(" Cannot find Executable: " + segmentationExecutable);
                throw new Exception(" Cannot find Executable: " + segmentationExecutable);
            }

            //segment all wav files in data dir.
            CallProcess(segmentationExecutable, cmdLineArgument);
        }

        
        /// <summary>
        /// Writes the Segmentation info files showing how wav files are segmented
        /// </summary>
        /// <param name="htkConfig"></param>
        /// <param name="vocalization"></param>
        /// <param name="extractLabels">True by default - i.e. always do it</param>
        public static void CreateWLT(HTKConfig htkConfig, ref string vocalization, bool extractLabels)
        {
            string segmentationFileExt = HTKConfig.segmentFileExt;
            string labelFileExt = HTKConfig.labelFileExt;
            string txtLine = "";

            if (extractLabels) //True by default - i.e. always do this
            {
                try
                {
                    //TWO - Read in segmentation files and transcribe them into PHONES.MLF
                    //read the labelSeq file containing the label sequence

                    StreamReader wltReader = null;
                    StreamWriter wltWriter = null;
                    string heading = "#!MLF!#";
                    string[] param = null;

                    try
                    {
                        Console.WriteLine("Writing Phones Segmentation File: <" + htkConfig.wltF+">");
                        wltWriter = File.CreateText(htkConfig.wltF);
                        wltWriter.WriteLine(heading);

                        List<string> trnDirList = new List<string>();
                        if (htkConfig.multiSyllableList.Count == 0)
                        {
                            //the list will only have one element
                            trnDirList.Add(htkConfig.trnDirPath);
                        }
                        else
                        {
                            //full path of all training directories
                            foreach (string word in htkConfig.multiSyllableList)
                            {
                                trnDirList.Add(htkConfig.trnDirPath + "\\" + word);
                            }
                        }

                        foreach (string currTrnDir in trnDirList)
                        {
                            DirectoryInfo Dir = new DirectoryInfo(currTrnDir);
                            FileInfo[] FileList = Dir.GetFiles("*" + HTKConfig.wavExt, SearchOption.TopDirectoryOnly);
                            string currLine = "";
                            string word = "";
                            string srtTime = "";
                            float sTime = 0f;
                            string endTime = "";
                            float eTime = 0f;

                            foreach (FileInfo FI in FileList)
                            {

                                currLine = "\"*/" + Path.GetFileNameWithoutExtension(FI.FullName) + labelFileExt + "\"";

                                wltWriter.WriteLine(currLine);
                                
                                //read related label file
                                string segFile = Path.GetFileNameWithoutExtension(FI.FullName) + segmentationFileExt;
                                wltReader = new StreamReader(currTrnDir + "\\" + segFile);
                                wltReader.ReadLine(); //remove first line

                                if (!htkConfig.noSegmentation) //consider SIL/VOCALIZATION segmentents
                                {
                                    while ((txtLine = wltReader.ReadLine()) != null)
                                    {
                                        param = Regex.Split(txtLine, @"\s+");
                                        if (param[0].StartsWith("SIL"))
                                            word = "SIL";
                                        else
                                        {
                                            word = param[0];
                                            vocalization = word;
                                        }

                                        //add word to word list
                                        if (!syllableList.Contains(word))
                                            syllableList.Add(word);

                                        sTime = float.Parse(param[2]);
                                        sTime *= 1e+7f; //conversion to HTK units
                                        srtTime = sTime.ToString();
                                        eTime = float.Parse(param[4]);
                                        eTime *= 1e+7f; //conversion to HTK units
                                        endTime = eTime.ToString();
                                        currLine = srtTime + " " + endTime + " " + word;
                                        wltWriter.WriteLine(currLine);                                        
                                    }
                                    wltWriter.WriteLine(".");
                                }
                                else
                                {
                                    string bottomLine = "";
                                    while ((txtLine = wltReader.ReadLine()) != null)
                                    {
                                        bottomLine = txtLine;
                                    }
                                    param = Regex.Split(bottomLine, @"\s+");

                                    sTime *= 0.0f; srtTime = sTime.ToString();
                                    eTime = float.Parse(param[4]);
                                    eTime *= 1e+7f; //conversion to HTK units
                                    endTime = eTime.ToString();
                                    word = htkConfig.CallName;
                                    currLine = srtTime + " " + endTime + " " + word;
                                    wltWriter.WriteLine(currLine);
                                    wltWriter.WriteLine(".");
                                    //populate syllable list
                                    if (!syllableList.Contains(word))
                                        syllableList.Add(word);
                                    vocalization = word;
                                }                                
                            }

                            //TO DO: check if each entry of labParam has a related .pcf file
                        }

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
                        if (wltReader != null)
                        {
                            wltReader.Close();
                        }

                    }// end finally

                }
                catch (Win32Exception e)
                {
                    Console.WriteLine("ERROR 1: FAILED TO COMPLETE METHOD: CreateWLT(HTKConfig htkConfig, ref string vocalization, bool extractLabels)");
                    if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                    {
                        Console.WriteLine(e.Message + ". Check the path.");
                    }
                    throw (e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR 2: FAILED TO COMPLETE METHOD: CreateWLT(HTKConfig htkConfig, ref string vocalization, bool extractLabels)");
                    Console.WriteLine(e.Message);
                    throw (e);
                }

                Console.WriteLine("HMMBuilder.HTKHelper: Finished writing phone segmentation file.");

            } //end if (extractLabels)
            else //DO NOT EXTRACT LABELS
            {
                //htkConfig.singleWord contains the one-word model to train               
                StreamWriter wltWriter = null;
                string heading = "#!MLF!#";

                try
                {
                    if (!syllableList.Contains(htkConfig.BkgName))
                        syllableList.Add(htkConfig.BkgName); 
                    
                    vocalization = htkConfig.BkgName;

                    Console.WriteLine("Writing Phones Segmentation File: <" + htkConfig.wltFBkg + ">");
                    wltWriter = File.CreateText(htkConfig.wltFBkg);
                    wltWriter.WriteLine(heading);

                    DirectoryInfo Dir = new DirectoryInfo(htkConfig.trnDirPathBkg);
                    FileInfo[] FileList = Dir.GetFiles("*" + HTKConfig.wavExt, SearchOption.TopDirectoryOnly);

                    string currLine = "";                        
                    foreach (FileInfo FI in FileList)
                    {
                        currLine = "\"*/" + Path.GetFileNameWithoutExtension(FI.FullName) + labelFileExt + "\"";
                        wltWriter.WriteLine(currLine);
                        wltWriter.WriteLine(htkConfig.BkgName);
                        wltWriter.WriteLine(".");
                    }
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
                }

            } //end if NOT extractLabels
        } //end METHOD CreateWLT()


        /// <summary>
        /// This method used to be at end of previous method CreateWLT()
        /// </summary>
        /// <param name="htkConfig"></param>
        public static void WriteDictionary(HTKConfig htkConfig)
        {            
            //THREE: generate dictionary file 'dict' and monophones file 'monophones'
            if(!htkConfig.bkgTraining)
                Console.WriteLine("HMMBuilder: Generate dictionary file '{0}' and monophones file '{1}'",
                                                            htkConfig.DictFile, htkConfig.monophones);
            else
                Console.WriteLine("BKGTrainer: Generate dictionary file '{0}' and monophones file '{1}'",
                                                            htkConfig.DictFileBkg, htkConfig.monophonesBkg);
            StreamReader dictionaryStreamReader = null;
            try
            {
                List<string> bcpList = new List<string>();  //monophones list
                List<string> dictList = new List<string>(); //dictionary list

                //adding SENT_START and END to build dictionary
                syllableList.Add("SENT_START");
                syllableList.Add("SENT_END");
                //sort dictionary
                syllableList.Sort();
                
                //while ((txtLine = dictStrm.ReadLine()) != null)
                foreach(string word in syllableList)
                {
                    //param = Regex.Split(txtLine, @"\s+\[\w*\]\s+");
                    //if (!Regex.IsMatch(param[0].ToUpper(), @"SENT_END") &&
                    //    !Regex.IsMatch(param[0].ToUpper(), @"SENT_START"))
                    if(!word.Equals("SENT_START") && !word.Equals("SENT_END"))
                    {
                        bcpList.Add(word);
                        dictList.Add(word + "\t[" + word + "]\t" + word);
                    }
                    else
                        dictList.Add(word + "\t[]\t" + "SIL");                    
                }

                syllableList.Remove("SENT_START");
                syllableList.Remove("SENT_END");

                //Save list of phones to be recognised to files
                //Check if the target directory exists. If not, create it.
                string bcpDir;
                if(!htkConfig.bkgTraining)
                    bcpDir = Path.GetDirectoryName(htkConfig.monophones);
                else
                    bcpDir = Path.GetDirectoryName(htkConfig.monophonesBkg);

                if (Directory.Exists(bcpDir) == false)
                {
                    Directory.CreateDirectory(bcpDir);
                }
                StreamWriter bcpWriter  = null; //to contain list of names of syllables/phones to recognise
                StreamWriter dictWriter = null; //contains another list of phones/syllables

                if (!htkConfig.bkgTraining)
                {
                    bcpWriter = File.CreateText(htkConfig.monophones);
                    dictWriter = File.CreateText(htkConfig.DictFile);
                }
                else
                {
                    bcpWriter = File.CreateText(htkConfig.monophonesBkg);
                    dictWriter = File.CreateText(htkConfig.DictFileBkg);
                }

                foreach (string word in bcpList)
                {
                    bcpWriter.WriteLine(word);
                }
                foreach (string word in dictList)
                {
                    dictWriter.WriteLine(word);
                }

                bcpWriter.Flush();
                bcpWriter.Close();
                dictWriter.Flush();
                dictWriter.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            finally
            {
                if (dictionaryStreamReader != null)
                {
                    dictionaryStreamReader.Close();
                }
            }
        } //end WriteDictionary()


        /// <summary>
        /// Parses the grammar file.
        /// </summary>
        /// <param name="source grammar file"></param>
        /// <param name="destination word network"></param>
        /// <param name="htkConfig"></param>
        /// <returns>Void</returns> 
        public static void HParse(string sourceF, string destF, HTKConfig htkConfig)
        {         
            string commandLineArguments = sourceF + " " + destF;
            Console.WriteLine("\nHTKHelper.HParse");
            Console.WriteLine("  Command Line Arguments = " + commandLineArguments);

            CallProcess(htkConfig.HParseExecutable, commandLineArguments);
        }


        /// <summary>
        /// Prepares script files that contain lists of other files.
        /// Must prepare two lists of training data, two lists of true test data and two lists of false test data.
        /// Script files have the extention .scp.
        /// Then extracts features from .wav files and stores in .mfc files
        /// 
        /// </summary>
        /// <param name="optStr"></param>
        /// <param name="htkConfig"></param>
        /// <param name="fvToExtract"></param>
        public static void HCopy(string optStr, HTKConfig htkConfig, bool fvToExtract)
        {
            //write the script files for training and test data
            try
            {
                //check that we are not training the BACKGROUND model right now
                if (!htkConfig.bkgTraining)
                {
                    //check if the call is multisyllabic
                    if (htkConfig.multiSyllableList.Count == 0)
                    {
                        WriteScriptFiles(htkConfig.trnDirPath, htkConfig.cTrainF, htkConfig.trainF, HTKConfig.wavExt, HTKConfig.mfcExt);
                    }
                    else
                    {
                        bool firtWord = true;
                        foreach (string word in htkConfig.multiSyllableList)
                        {
                            string trnDir = htkConfig.trnDirPath + "\\" + word;
                            string tstTrueDir = htkConfig.tstTrueDirPath + "\\" + word;
                            string tstFalseDir = htkConfig.tstFalseDirPath + "\\" + word;

                            if (firtWord)
                            {
                                WriteScriptFiles(trnDir, htkConfig.cTrainF, htkConfig.trainF, HTKConfig.wavExt, HTKConfig.mfcExt);
                                //WriteScriptFiles(tstTrueDir, htkConfig.cTestTrueF, htkConfig.tTrueF, htkConfig.wavExt, htkConfig.mfcExt);                            
                                firtWord = false;
                            }
                            else
                            {
                                AppendScriptFiles(trnDir, htkConfig.cTrainF, htkConfig.trainF, HTKConfig.wavExt, HTKConfig.mfcExt);
                                //AppendScriptFiles(tstTrueDir, htkConfig.cTestTrueF, htkConfig.tTrueF, htkConfig.wavExt, htkConfig.mfcExt);                            
                            }
                        }
                    }

                    WriteScriptFiles(htkConfig.tstTrueDirPath, htkConfig.cTestTrueF, htkConfig.tTrueF, HTKConfig.wavExt, HTKConfig.mfcExt);
                    WriteScriptFiles(htkConfig.tstFalseDirPath, htkConfig.cTestFalseF, htkConfig.tFalseF, HTKConfig.wavExt, HTKConfig.mfcExt);
                }
                else //we are training the BACKGROUND model
                {
                    WriteScriptFiles(htkConfig.trnDirPathBkg, htkConfig.cTrainFBkg, htkConfig.trainFBkg, HTKConfig.wavExt, HTKConfig.mfcExt);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Could not create code files.");
                throw (e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }

            //THREE - extract feature vectors for train and test sets
            Console.WriteLine("\nHTKHelper.HCopy: fvToExtract=" + fvToExtract + "   options=" + optStr);
            Console.WriteLine("\nExtracting feature vectors from the training.wav files into .mfc files");

            if (!htkConfig.bkgTraining)
            {
                if (fvToExtract)
                {
                    ExtractFeatures(optStr, htkConfig.MfccConfigFN, htkConfig.cTrainF, htkConfig.HCopyExecutable); //training data
                    ExtractFeatures(optStr, htkConfig.MfccConfigFN, htkConfig.cTestTrueF, htkConfig.HCopyExecutable);  //test data
                    ExtractFeatures(optStr, htkConfig.MfccConfigFN, htkConfig.cTestFalseF, htkConfig.HCopyExecutable); //test data

                } //end if do extraction of features
            }
            else
            {
                ExtractFeatures(optStr, htkConfig.MfccConfigFN, htkConfig.cTrainFBkg, htkConfig.HCopyExecutable); //training data
            }
            //Console.WriteLine("HMMBuilder: GOT TO HERE 1");
            //Console.ReadLine();
        } //end Method HCopy()

   
        public static void AppendScriptFiles(string dirPath, string scriptFN_code, string scriptFN, string sourceExt, string outExt)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            FileInfo[] FileList = dirInfo.GetFiles("*" + sourceExt, SearchOption.TopDirectoryOnly);

            StreamWriter objWriter = File.AppendText(scriptFN_code);//list of .wav > .mfc files
            StreamWriter objWriter2 = File.AppendText(scriptFN);

            foreach (FileInfo FI in FileList)
            {
                string currLine = dirPath + "\\" + FI.Name + " " + dirPath + "\\";
                string fileName = Path.GetFileNameWithoutExtension(FI.FullName);
                currLine += fileName + outExt;
                objWriter.WriteLine(currLine);
                currLine = dirPath + "\\" + fileName + outExt;
                objWriter2.WriteLine(currLine);
            }

            objWriter.Flush();
            objWriter2.Flush();
            objWriter.Close();
            objWriter2.Close();
        } //end method


        //call as follows  WriteScriptFiles(htkConfig.tstTrueDirPath, htkConfig.cTestTrueF, htkConfig.tTrueF, htkConfig.wavExt, htkConfig.mfcExt)
        public static void WriteScriptFiles(string dirPath, string scriptFN_code, string scriptFN, string sourceExt, string outExt)
        {                    
            Console.WriteLine("WRITING TWO SCRIPT FILES");

            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            FileInfo[] FileList = dirInfo.GetFiles("*" + sourceExt, SearchOption.TopDirectoryOnly);

            StreamWriter objWriter  = File.CreateText(scriptFN_code);//list of .wav > .mfc files
            StreamWriter objWriter2 = File.CreateText(scriptFN);

            foreach (FileInfo FI in FileList)
            {
                string currLine = dirPath + "\\" + FI.Name + " " + dirPath + "\\";
                string fileName = Path.GetFileNameWithoutExtension(FI.FullName);
                currLine += fileName + outExt;
                objWriter.WriteLine(currLine);
                currLine = dirPath + "\\" + fileName + outExt;
                objWriter2.WriteLine(currLine);
            }

            objWriter.Flush();
            objWriter2.Flush();
            objWriter.Close();
            objWriter2.Close();
        } //end method


        //call as follows: ExtractFeatures(optStr, htkConfig.MfccConfigFN, htkConfig.cTrainF) 
        public static void ExtractFeatures(string optStr, string mfccConfigFN, string scriptF, string HCopyExecutable) 
        {
            Console.WriteLine("\nHTKHelper.ExtractFeatures: options=" + optStr);
            Console.WriteLine("\nExtracting feature vectors from the training.wav files into .mfc files");
            
            //const string HCopyExecutable = "HCopy.exe";

            StreamReader stdErr = null;
            StreamReader stdOut = null;
            string output = null;
            string error = null;
            //string options = optStr;
                
            //Extract feature vectors for train data
            string commandLineArguments = optStr + " -C " + mfccConfigFN + " -S " + scriptF;
            Console.WriteLine("  Command Line Arguments=" + commandLineArguments);
            if (File.Exists(mfccConfigFN)) Console.WriteLine("  Found Script file=" + scriptF);
            else                           Console.WriteLine("  WARNING Could NOT FIND Script file=" + scriptF);
            //look for HTK.HCopy file
            if (File.Exists(HCopyExecutable)) Console.WriteLine("  Found HCopy.exe file=" + HCopyExecutable);
            else                              Console.WriteLine("  WARNING Could NOT FIND file=" + HCopyExecutable);

            try
            {
                    Process hcopy = new Process();
                    ProcessStartInfo psI = new ProcessStartInfo(HCopyExecutable);
                    psI.UseShellExecute = false;
                    psI.RedirectStandardOutput = true;
                    psI.RedirectStandardError = true;
                    psI.CreateNoWindow = true;
                    psI.WindowStyle = ProcessWindowStyle.Hidden;
                    psI.Arguments = commandLineArguments;
                    hcopy.StartInfo = psI;
                    hcopy.Start();
                    //hcopy.WaitForExit();
                    stdErr = hcopy.StandardError;
                    stdOut = hcopy.StandardOutput;
                    output = stdOut.ReadToEnd();
                    error = stdErr.ReadToEnd();
                    //Console.WriteLine(output);  //writes contents of the training script file
                    if (error.Contains("ERROR"))
                    {
                        throw new Exception();
                    }
            }
            catch (Win32Exception e)
            {
                    if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                    {
                        Console.WriteLine(error);
                        Console.WriteLine(e.Message + "..... Check the path.");
                    }
                    throw (e);
            }
            catch (Exception e)
            {
                    Console.WriteLine(error);
                    throw (e);
            }
        } //end method ExtractFeatures(string optStr) 



        public static void InitSys(string aOtpStr, HTKConfig htkConfig)
        {
            string protoCfgDir;
            string prototypeHMM;
            string tgtDir;
            string mfccConf2;
            string trainF;
            
            if (!htkConfig.bkgTraining)
            {
                protoCfgDir = htkConfig.ProtoConfDir;
                prototypeHMM = htkConfig.prototypeHMM;
                tgtDir = htkConfig.tgtDir0;
                mfccConf2 = htkConfig.MfccConfig2FN;
                trainF = htkConfig.trainF;
            }
            else
            {
                protoCfgDir = htkConfig.ProtoConfDirBkg;
                prototypeHMM = htkConfig.prototypeHMMBkg;
                tgtDir = htkConfig.tgtDir0Bkg;
                mfccConf2 = htkConfig.MfccConfig2FNBkg;
                trainF = htkConfig.trainFBkg;
            }

            string HCompVExecutable = htkConfig.HTKDir + "\\HCompV.exe";
            StreamReader stdErr = null;
            StreamReader stdOut = null;
            string output = null;
            string error = null;
            try
            {
                // Determine whether the target directory exists. Create it if does not exist
                if (!Directory.Exists(tgtDir))
                {
                    Directory.CreateDirectory(tgtDir);
                }
                else
                {
                    // Remove its content
                    //Directory.Delete(tgtDir, true);
                    //Directory.CreateDirectory(tgtDir);
                }

                //Calling HCompV.exe with following arguments creates the proto and vFloors in dir hmms\hmm.0 
                //HCompV.exe -A -D -T 1 -C config_train -f 0.01 -m -S train.scp -M hmm.0 proto    
                string commandLine = " " + aOtpStr + " -C " + mfccConf2 + " -f 0.01 -m -S " + trainF 
                                   + " -M " + tgtDir + " " + prototypeHMM;
                Console.WriteLine("commandLine = "+commandLine);

                Process hcompv = new Process();
                ProcessStartInfo psI = new ProcessStartInfo(HCompVExecutable);
                psI.UseShellExecute = false;
                psI.RedirectStandardOutput = true;
                psI.RedirectStandardError = true;
                psI.CreateNoWindow = true;
                psI.WindowStyle = ProcessWindowStyle.Hidden;
                psI.Arguments = commandLine;
                hcompv.StartInfo = psI;
                hcompv.Start();
                hcompv.WaitForExit();
                stdErr = hcompv.StandardError;
                stdOut = hcompv.StandardOutput;
                output = stdOut.ReadToEnd();
                error = stdErr.ReadToEnd();
                Console.WriteLine(output);
                if (error.Contains("ERROR"))
                {
                    throw new Exception();
                }
                //wait for the process to finish


                //create a HMM for each WORD in the syllableList.
                //If the file 'WORD' is not found a default transition matrix will be used.

                string prototypeFN = Path.GetFileName(prototypeHMM);
                
                //StreamWriter protoWriter = null;
                StreamReader protoReader = null;

                protoReader = new StreamReader(tgtDir + "\\" + prototypeFN); //file exists, we checked that before

                Queue<string> protoLineQueue = new Queue<string>();
                Queue<string> hmmDefsQueue = new Queue<string>();
                
                string txtLine = null;
                while ((txtLine = protoReader.ReadLine()) != null)
                {
                    protoLineQueue.Enqueue(txtLine); //enqueue 'proto' file lines
                }

                bool protoWordPresent = false;

                Dictionary<string, string> tmpProtoDict = new Dictionary<string, string>();


                //Check if 'proto.pcf' exists
                if (!HMMSettings.confProtoDict.TryGetValue(prototypeFN, out tmpProtoDict))
                {
                    Console.WriteLine("File 'proto.pcf' not found in directory {0}.", protoCfgDir);
                    throw new Exception();
                }

                
                foreach(string match in syllableList)
                {
                    Console.WriteLine("syllable = " + match + " in syllableList");

                    //check if there are other .pcf files
                    Dictionary<string, string> tmpWordDict = new Dictionary<string, string>();
                    if (HMMSettings.confProtoDict.TryGetValue(match, out tmpWordDict))
                        protoWordPresent = true;
                    else 
                        protoWordPresent = false;
                    
                    //Score Threshold defined?  
                    int tmpThr = 0;
                    string strScoreThr = "";
                    if (protoWordPresent && tmpWordDict.TryGetValue("SCORETHR", out strScoreThr))
                    {
                        if(htkConfig.threshold.TryGetValue(match, out tmpThr))
                            htkConfig.threshold[match] = tmpThr; //override the old value
                        else
                            htkConfig.threshold.Add(match, int.Parse(strScoreThr));
                    }

                    
                    int maxStates = int.MaxValue;
                    string strMaxStates = "";
                    if (protoWordPresent && tmpWordDict.TryGetValue("NSTATES", out strMaxStates))
                    {
                        maxStates = int.Parse(strMaxStates);
                    }

                    try
                    {
                        //Create Word Proto file
                        //protoWriter = File.CreateText(tgtDir + "\\" + match);

                        bool valid = false;
                        //Populate the file
                        foreach(string protoLine in protoLineQueue)
                        {
                                                      
                            if(Regex.IsMatch(protoLine, @"~h"))
                            {
                                hmmDefsQueue.Enqueue("~h \"" + match + "\"");
                                valid = false;
                            }
                            else if(Regex.IsMatch(protoLine, @"<NUMSTATES>"))
                            {
                                if (maxStates < int.MaxValue) //number of states has been redefined
                                {
                                    int tmpInt = maxStates + 2;
                                    string tmpStr = "<NUMSTATES> " + tmpInt.ToString();
                                    hmmDefsQueue.Enqueue(tmpStr);
                                    valid = false;
                                }
                                else
                                {
                                    valid = true;
                                }
                            }
                            else if(Regex.IsMatch(protoLine, @"<STATE>"))
                            {
                                string[] param = Regex.Split(protoLine, @">\s+");
                                int currState = int.Parse(param[1]);
                                if (currState -1 <= maxStates) 
                                {                                    
                                    valid = true; 
                                }
                                else
                                {
                                    valid = false;
                                }
                            }
                            else if(Regex.IsMatch(protoLine, @"<TRANSP>")) 
                            {
                                string[] param = Regex.Split(protoLine, @">\s+");
                                if (maxStates < int.MaxValue) //nStates has been re-defined ...                                   
                                {
                                    if(int.Parse(param[1]) != maxStates+2) // ... and is different from 'proto'
                                    {
                                        int tmpInt = maxStates + 2;
                                        string tmpStr = "<TRANSP> " + tmpInt.ToString();
                                        hmmDefsQueue.Enqueue(tmpStr);
                                        //transition matrix required
                                        if (File.Exists(protoCfgDir + "\\" + match))
                                        {
                                            //Read the transition matrix
                                            StreamReader pReader = new StreamReader(protoCfgDir + "\\" + match);
                                            string matxLine = null;
                                            while ((matxLine = pReader.ReadLine()) != null)
                                            {
                                                hmmDefsQueue.Enqueue(matxLine);
                                            }
                                            pReader.Close();
                                            valid = false;
                                        }
                                        else
                                        {
                                            //Console.WriteLine("A file containing the transition matrix is required in order to build the prototype for {0}", match);
                                            //throw new Exception();

                                            //DONE: alternatively build a standard <NUMSTATES>x<NUMSTATES> transition matrix

                                            //write Transition Matrix
                                            Console.WriteLine("Difference found between {0} and the prototype: number of states. As no transition matrix is given a standard matrix will be used.", match);

                                            for (int i = 1; i <= maxStates + 2; i++)
                                            {
                                                string tmpLine = null;
                                                for (int j = 1; j <= maxStates + 2; j++)
                                                {
                                                    if ((i == 1) && (j == 2))
                                                    {
                                                        tmpLine += "  1.000e+0";
                                                    }
                                                    else
                                                        if ((i == j) && (i != 1) && (i != maxStates + 2))
                                                        {
                                                            if (i != maxStates + 1) tmpLine += "  6.000e-1";
                                                            else tmpLine += "  7.000e-1";
                                                        }
                                                        else
                                                            if (i == (j - 1))
                                                            {
                                                                if (i != maxStates + 1) tmpLine += "  4.000e-1";
                                                                else tmpLine += "  3.000e-1";
                                                            }
                                                            else
                                                            {
                                                                tmpLine += "  0.000e+0";
                                                            }
                                                }
                                                hmmDefsQueue.Enqueue(tmpLine);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        //TO DO: the user may want to define its own initial transition matrix
                                        
                                        valid = true; 
                                    }
                                }
                                else //nStates has not been re-defined
                                {
                                    //transition matrix file not required. Check if present ...
                                    if (File.Exists(protoCfgDir + "\\" + match))
                                    {
                                        //write the tag <TRANSP>
                                        hmmDefsQueue.Enqueue(protoLine);
                                        //Read the transition matrix
                                        StreamReader pReader = new StreamReader(protoCfgDir + "\\" + match);
                                        string matxLine = null;
                                        while ((matxLine = pReader.ReadLine()) != null)
                                        {
                                            hmmDefsQueue.Enqueue(matxLine);
                                        }
                                        pReader.Close();
                                        valid = false;
                                    } 
                                    else
                                    {
                                        valid = true; //if matrix is not provided copy it from the 'proto' file
                                    }
                                }
                            }
                            else if (Regex.IsMatch(protoLine, @"<ENDHMM>"))
                            {
                                valid = false;
                                hmmDefsQueue.Enqueue(protoLine);
                            }
                            else if (Regex.IsMatch(protoLine, @"<BEGINHMM>"))
                            {
                                valid = true;
                            }
                            
                            if (valid) hmmDefsQueue.Enqueue(protoLine);   
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Could not create the '{0}' HMM model.", match);
                        throw (e);
                    }
                    catch (Exception e)
                    {                        
                        throw (e);
                    }
                    finally
                    {

                    }
                }
                protoReader.Close();



                //create 'hmmdefs' and 'macros' files
                StreamWriter hmmDefs = null;
                StreamWriter macros = null;
                StreamReader vFloors = null;
                try
                {
                    hmmDefs = File.CreateText(tgtDir + "\\" + htkConfig.hmmdefFN);
                    foreach (string hmmDefsLine in hmmDefsQueue)
                    {
                        hmmDefs.WriteLine(hmmDefsLine);
                    }

                    macros = File.CreateText(tgtDir + "\\" + htkConfig.macrosFN);

                    bool valid = false;
                    foreach(string protoLine in protoLineQueue)
                    {
                        if (Regex.IsMatch(protoLine, @"~o"))
                        {
                            valid = true;
                        }

                        if (Regex.IsMatch(protoLine, @"~h"))
                        {
                            valid = false;
                        }

                        if (valid)
                            macros.WriteLine(protoLine);
                    }

                    vFloors = new StreamReader(tgtDir + "\\" + htkConfig.vFloorsFN);
                    while ((txtLine = vFloors.ReadLine()) != null)
                    {
                        macros.WriteLine(txtLine); //enqueue lines of the proto file
                    }
                    vFloors.Close();

                }
                catch (IOException e)
                {
                    Console.WriteLine("Could not create master macro files 'hmmdefs' and/or 'macros'.");
                    throw (e);
                }
                catch (Exception e)
                {
                    throw (e);
                }
                finally
                {
                    if (hmmDefs != null)
                    {
                        hmmDefs.Flush();
                        hmmDefs.Close();
                    }
                    if (macros != null)
                    {
                        macros.Flush();
                        macros.Close();
                    }
                }//END OF create 'hmmdefs' and 'macros' files

            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                {
                    Console.WriteLine(e.Message + ". Check the path.");
                }
                throw(e);
            }            
            catch (Exception e)
            {
                Console.WriteLine(error);
                throw (e);
            }
            finally
            {

            }
        } //end method
        


        public static void HERest(int numIters, string aOtpStr, string pOptStr, HTKConfig htkConfig)
        {
            Console.WriteLine("Model re-estimation: HERest");

            string HERestExecutable = htkConfig.HTKDir + "\\HERest.exe";

            //tgtDir1 == srcD
            //tgtDir2 == tgtD

            StreamReader stdErr = null;

            //StreamReader stdOut = null;
            //string output = null;
            string error = null;

            //Create directories
            string tmpD;
            string tgtDir0;
            string tgtDir1;
            string tgtDir2;
            string MfccConfig2FN;
            string wltF;
            string trainF;
            string monophones;

            if (!htkConfig.bkgTraining)
            {
                tmpD = htkConfig.tgtDirTmp;
                tgtDir1 = htkConfig.tgtDir1;
                tgtDir2 = htkConfig.tgtDir2;
                tgtDir0 = htkConfig.tgtDir0;
                MfccConfig2FN = htkConfig.MfccConfig2FN; 
                wltF = htkConfig.wltF; 
                trainF = htkConfig.trainF; 
                monophones = htkConfig.monophones;
            }
            else
            {
                tmpD = htkConfig.tgtDirTmpBkg;
                tgtDir1 = htkConfig.tgtDir1Bkg;
                tgtDir2 = htkConfig.tgtDir2Bkg;
                tgtDir0 = htkConfig.tgtDir0Bkg;
                MfccConfig2FN = htkConfig.MfccConfig2FNBkg;
                wltF = htkConfig.wltFBkg;
                trainF = htkConfig.trainFBkg;
                monophones = htkConfig.monophonesBkg;
            }

            try
            {
                if (Directory.Exists(tgtDir1)) // Remove hmm1 dir if it exists
                {
                    Directory.Delete(tgtDir1, true);
                }
                Directory.CreateDirectory(tgtDir1);
                DirectoryInfo srcDir = new DirectoryInfo(tgtDir1); //hmm1 becomes source dir

                if (Directory.Exists(tgtDir2))// Remove hmm2 dir if it exists
                {
                    Directory.Delete(tgtDir2, true);
                }
                Directory.CreateDirectory(tgtDir2);
                DirectoryInfo tgtDir = new DirectoryInfo(tgtDir2); //hmm2 becomes target dir

                if (Directory.Exists(tmpD)) // Remove temp dir if exists
                {                   
                    Directory.Delete(tmpD, true);
                }
                Directory.CreateDirectory(tmpD);
                DirectoryInfo tmpDir = new DirectoryInfo(tmpD); // create temporary directory



                //Copy hmm0 to hmm1. hmm0 contains the initial parameter values
                DirectoryInfo hmm0 = new DirectoryInfo(tgtDir0);
                CopyAll(hmm0, srcDir);
                if (File.Exists(tgtDir1 + "\\" + htkConfig.protoFN))
                    File.Delete(tgtDir1 + "\\" + htkConfig.protoFN);
                if (File.Exists(tgtDir1 + "\\" + htkConfig.vFloorsFN))
                    File.Delete(tgtDir1 + "\\" + htkConfig.vFloorsFN);


                //Now do HMM training
                try
                {
                    int i = 1;
                    while(i<=numIters)
                    {
                        Console.WriteLine("HMM Iteration {0}", i);
                        
                        //SET UP COMMAND LINE FOR HERest.exe
                        //HERest.exe -A -D -T 1 
                        //      -C ./configs/config_train 
                        //      -I ./configs/phones.mlf 
                        //      -t 150.0 
                        //      -S ./configs/train.scp 
                        //      -H ./hmms/hmmx/macros 
                        //      -H ./hmms/hmmx/hmmdefs 
                        //      -M ./hmms/hmm(x+1) ./lists/bcplist

                        string commandLine = "";
                        commandLine = " " + aOtpStr + " -C " + MfccConfig2FN + " -I " + wltF +
                                      " " + pOptStr +
                                      " -S " + trainF +
                                      " -H " + srcDir.ToString() + "\\" + htkConfig.macrosFN +
                                      " -H " + srcDir.ToString() + "\\" + htkConfig.hmmdefFN +
                                      " -M " + tgtDir.ToString() + " " + monophones;

                        Process herest = new Process();
                        ProcessStartInfo psI = new ProcessStartInfo(HERestExecutable);
                        psI.UseShellExecute = false;
                        //psI.RedirectStandardOutput = true;
                        psI.RedirectStandardError = true;
                        psI.CreateNoWindow = true;
                        psI.Arguments = commandLine;
                        herest.StartInfo = psI;
                        herest.Start();
                        herest.WaitForExit();
                        stdErr = herest.StandardError;
                        //stdOut = herest.StandardOutput;
                        //output = stdOut.ReadToEnd();
                        error = stdErr.ReadToEnd();
                        //Console.WriteLine(output);
                        if (error.Contains("ERROR"))
                        {
                            throw new Exception();
                        } 
                        if(numIters >1)
                        {
                            CopyAll(tgtDir,tmpDir);
                            srcDir = tmpDir;
                        }
                        i++;
                    }
                }
                catch (Win32Exception e)
                {
                    if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                    {
                        Console.WriteLine(e.Message + ". Check the path.");
                    }
                    throw(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(error);
                    throw (e);
                }

            }        
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            finally
            {

            }
        }

        public static void HRest(string aOtpStr, HTKConfig htkConfig, string callName)
        {
            Console.WriteLine("Model '{0}' re-estimation: HRest", callName);

            string HRestExecutable = htkConfig.HTKDir + "\\HRest.exe";

            StreamReader stdErr = null;

            //StreamReader stdOut = null;
            //string output = null;
            string error = null;

            //Create directories
            string tmpD;
            string tgtDir0;
            string tgtDir1;
            string tgtDir2;
            string MfccConfig2FN;
            string wltF;
            string trainF;
            string monophones;

            if (!htkConfig.bkgTraining)
            {
                tmpD = htkConfig.tgtDirTmp;
                tgtDir1 = htkConfig.tgtDir1;
                tgtDir2 = htkConfig.tgtDir2;
                tgtDir0 = htkConfig.tgtDir0;
                MfccConfig2FN = htkConfig.MfccConfig2FN;
                wltF = htkConfig.wltF;
                trainF = htkConfig.trainF;
                monophones = htkConfig.monophones;
            }
            else
            {
                tmpD = htkConfig.tgtDirTmpBkg;
                tgtDir1 = htkConfig.tgtDir1Bkg;
                tgtDir2 = htkConfig.tgtDir2Bkg;
                tgtDir0 = htkConfig.tgtDir0Bkg;
                MfccConfig2FN = htkConfig.MfccConfig2FNBkg;
                wltF = htkConfig.wltFBkg;
                trainF = htkConfig.trainFBkg;
                monophones = htkConfig.monophonesBkg;
            }

            try
            {
                if (!Directory.Exists(tgtDir1)) // Remove hmm1 dir if it exists
                {
                    Directory.CreateDirectory(tgtDir1);
                }                
                DirectoryInfo srcDir = new DirectoryInfo(tgtDir1); //hmm1 becomes source dir

                if (!Directory.Exists(tgtDir2))// Remove hmm2 dir if it exists
                {
                    Directory.CreateDirectory(tgtDir2);
                }                
                DirectoryInfo tgtDir = new DirectoryInfo(tgtDir2); //hmm2 becomes target dir

                if (!Directory.Exists(tmpD)) // Remove temp dir if exists
                {
                    Directory.CreateDirectory(tmpD);
                }                
                DirectoryInfo tmpDir = new DirectoryInfo(tmpD); // create temporary directory


                //Copy hmm0 to hmm1. hmm0 contains the initial parameter values
                DirectoryInfo hmm0 = new DirectoryInfo(tgtDir0);
                CopyAll(hmm0, srcDir);
                if (File.Exists(tgtDir1 + "\\" + htkConfig.protoFN))
                    File.Delete(tgtDir1 + "\\" + htkConfig.protoFN);
                if (File.Exists(tgtDir1 + "\\" + htkConfig.vFloorsFN))
                    File.Delete(tgtDir1 + "\\" + htkConfig.vFloorsFN);


                //Now do HMM training
                try
                {
                    int i = 1;

                    //SET UP COMMAND LINE FOR HRest.exe
                    //HRest.exe -A -D -T 1 
                    //      -C ./configs/config_train 
                    //      -I ./configs/phones.mlf 
                    //      -S ./configs/train.scp 
                    //      -H ./hmms/hmmx/macros 
                    //      -H ./hmms/hmmx/hmmdefs 
                    //      -M ./hmms/hmm(x+1) 
                    //      VOCALIZATION

                    string commandLine = "";
                    commandLine = " " + aOtpStr + " -C " + MfccConfig2FN + " -I " + wltF +
                                  " -S " + trainF +
                                  " -H " + srcDir.ToString() + "\\" + htkConfig.macrosFN +
                                  " -H " + srcDir.ToString() + "\\" + htkConfig.hmmdefFN +
                                  " -M " + tgtDir.ToString() + " " + callName;

                    Process herest = new Process();
                    ProcessStartInfo psI = new ProcessStartInfo(HRestExecutable);
                    psI.UseShellExecute = false;
                    //psI.RedirectStandardOutput = true;
                    psI.RedirectStandardError = true;
                    psI.CreateNoWindow = true;
                    psI.Arguments = commandLine;
                    herest.StartInfo = psI;
                    herest.Start();
                    herest.WaitForExit();
                    stdErr = herest.StandardError;
                    //stdOut = herest.StandardOutput;
                    //output = stdOut.ReadToEnd();
                    error = stdErr.ReadToEnd();
                    //Console.WriteLine(output);
                    if (error.Contains("ERROR"))
                    {
                        throw new Exception();
                    }
                    CopyAll(tgtDir, tmpDir);
                }
                catch (Win32Exception e)
                {
                    if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                    {
                        Console.WriteLine(e.Message + ". Check the path.");
                    }
                    throw (e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(error);
                    throw (e);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            finally
            {               
            }
        }



        public static void HBuild(string monophones_test, string wordNet, string HBuildExecutable)
        {
            //./HBuild ./configs/monophones_test ./configs/phone.net
            StreamReader stdErr = null;
            StreamReader stdOut = null;
            string output = null;
            string error = null;
            try
            {
                string commandLine = " " + monophones_test + " " + wordNet;
                Process hbuild = new Process();
                ProcessStartInfo psI = new ProcessStartInfo(HBuildExecutable);
                psI.UseShellExecute = false;
                psI.RedirectStandardOutput = true;
                psI.RedirectStandardError = true;
                psI.CreateNoWindow = true;
                psI.Arguments = commandLine;
                hbuild.StartInfo = psI;
                hbuild.Start();
                hbuild.WaitForExit();
                stdErr = hbuild.StandardError;
                stdOut = hbuild.StandardOutput;
                output = stdOut.ReadToEnd();
                error = stdErr.ReadToEnd();
                Console.WriteLine(output);
                if (error.Contains("ERROR"))
                {
                    throw new Exception();
                } 
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                {
                    Console.WriteLine("ERROR 1!! FAILED TO COMPLETE HTKHelper.HBuild(string monophones_test, string wordNet)");                    
                    Console.WriteLine(e.Message + ". Check the path.");
                }
                throw(e);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR 2!! FAILED TO COMPLETE HTKHelper.HBuild(string monophones_test, string wordNet)");
                Console.WriteLine(error);
                throw (e);
            }
            finally
            {

            }
        } // end HBuild()


        public static void HVite(string confTrain, string tgtDir2, string testF,
                                 string wordNet, string dict, string resultPath, string monophones_test, string HViteExecutable)
        {
            //Console.WriteLine(
            //    "\n confTrain      ="+confTrain+
            //    "\n tgtDir2        =" + tgtDir2 +
            //    "\n testF          =" + testF +
            //    "\n wordNet        =" + wordNet +
            //    "\n dict           =" + dict +
            //    "\n resultPath     =" + resultPath +
            //    "\n monophones_test=" + monophones_test
            //);

            //const string HViteExecutable = "HVite.exe";
            //string exePath = htkDir + "\\" + HViteExecutable;

            //look for HTK.HVite file
            if (File.Exists(HViteExecutable)) Console.WriteLine("  Found HVite.exe file=" + HViteExecutable);
            else                              Console.WriteLine("  WARNING! Could NOT FIND HVite=" + HViteExecutable);


            //HVite.exe -C ./configs/config_train -H ./hmms/hmm.2/macros -H ./hmms/hmm.2/hmmdefs 
            //  -S ./configs/testfalse.scp -i ./results/recountFalse.mlf -w ./configs/phone.net 
            //  ./configs/dict ./configs/monophones_test
            StreamReader stdErr = null;
            StreamReader stdOut = null;
            string output = null;
            string error = null;
            try
            {
                ////check if the call is multisyllabic
                //if (htkConfig.multiSyllableList.Count == 0)
                //{ 
                
                //}                
                
                //Check for results directory
                string resultsDir = Path.GetDirectoryName(resultPath);
                if (!Directory.Exists(resultsDir))
                {
                    Directory.CreateDirectory(resultsDir);
                }
                
                string commandLine = " -C " + confTrain + " -H " + tgtDir2 + "\\macros" + 
                    " -H " + tgtDir2 + "\\hmmdefs" + " -S " + testF + " -i " + resultPath +
                    " -w " + wordNet + " " + dict + " " + monophones_test;

                Process hvite = new Process();
                ProcessStartInfo psI = new ProcessStartInfo(HViteExecutable);
                psI.UseShellExecute = false;
                psI.RedirectStandardOutput = true;
                psI.RedirectStandardError = true;
                psI.CreateNoWindow = true;
                psI.Arguments = commandLine;
                hvite.StartInfo = psI;
                hvite.Start();
                hvite.WaitForExit();
                stdErr = hvite.StandardError;
                stdOut = hvite.StandardOutput;
                output = stdOut.ReadToEnd();
                error = stdErr.ReadToEnd();
                Console.WriteLine(output);
                if (error.Contains("ERROR"))
                {
                    throw new Exception();
                }
            }            
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                {
                    Console.WriteLine(e.Message + ". Check the path.");
                }
                throw(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(error);
                throw (e);
            }
            finally
            {

            }
        } //end HVite
        #endregion



        public static void CallProcess(string exePath, string cmdLineArguments)
        {
            StreamReader stdErr = null;
            //StreamReader stdOut = null;
            //string output = null;
            string error = null;
            try
            {
                //Process currentProcess = Process.GetCurrentProcess();
                //String currentExePath = currentProcess.StartInfo.FileName;

                Process vSegment = new Process();
                ProcessStartInfo psI = new ProcessStartInfo(exePath);
                psI.UseShellExecute = false;
                psI.WindowStyle = ProcessWindowStyle.Hidden;
                //psI.WorkingDirectory = "";
                //psI.RedirectStandardOutput = true;
                psI.RedirectStandardError = true;
                psI.CreateNoWindow = false;
                psI.Arguments = cmdLineArguments;
                vSegment.StartInfo = psI;
                vSegment.Start();
                vSegment.WaitForExit();
                stdErr = vSegment.StandardError;

                //stdOut = vSegment.StandardOutput;
                //output = stdOut.ReadToEnd();
                error = stdErr.ReadToEnd();
                //Console.WriteLine(output);
                if (error.Contains("ERROR"))
                {
                    throw new Exception();
                }
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                {
                    Console.WriteLine(e.Message + ". Check the path.");
                }
                throw (e);
            }
            catch (Exception e)
            {
                Console.WriteLine(error);
                throw (e);
            }
            finally
            {
                Console.WriteLine(" Finished executable: " + Path.GetFileNameWithoutExtension(exePath));
            }

        } //end method CallProcess()

    }
}