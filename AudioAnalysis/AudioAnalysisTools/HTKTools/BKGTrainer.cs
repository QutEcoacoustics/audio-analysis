using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioAnalysisTools.HTKTools
{
    public class BKGTrainer
    {
        #region Variables
        static HTKConfig htkConfig;
        bool teeModel = false;
        string bkgLabel;

        #endregion

        #region Constructor
        public BKGTrainer(HTKConfig htkConf)
        {
            htkConfig = htkConf;
        }
        #endregion

        public void EstimateModel()
        {
            try
            {
                //Clear Prototype settings
                HMMSettings.confProtoDict.Clear();
                if (!teeModel)
                {
                    //Clear Sillable list
                    HTKHelper.SyllableList.Clear();
                }
                
                //Create Code config file for the extraction of MFCC features
                if (!Directory.Exists(htkConfig.ConfigDirBkg)) Directory.CreateDirectory(htkConfig.ConfigDirBkg);                
                htkConfig.WriteMfccConfigFile(htkConfig.MfccConfigFNBkg);

                //Create Train Code File for training the model
                ReadTCF(htkConfig.MfccConfigFNBkg, htkConfig.MfccConfig2FNBkg);

                //Create Prototype Config File
                if (!Directory.Exists(htkConfig.ProtoConfDirBkg)) Directory.CreateDirectory(htkConfig.ProtoConfDirBkg);
                WriteBkgPrototypeFile(htkConfig.ProtoConfDirBkg);               

                //Extract MFCC features from the recordings
                //This method also creates the files:
                // - 'codetrain.scp': used for extracting the MFCC features from the recordings
                // - 'train.scp': used for model re-estimation
                HTKHelper.HCopy(htkConfig.aOptionsStr, htkConfig, true);

                //Create Word Level Transcription file (phone.mlf)
                HTKHelper.CreateWLT(htkConfig, ref bkgLabel, false);

                //Create Dictionary file and Monophones file (what we called SillableList)
                HTKHelper.WriteDictionary(htkConfig);

                //Read in Prototype Configuration Files
                HMMSettings.ReadPCF(htkConfig.ProtoConfDirBkg);                 

                HMMSettings.WriteHMMprototypeFile(htkConfig.prototypeHMMBkg);

                if (teeModel)
                {
                    HTKHelper.InitSys(htkConfig.aOptionsStr, htkConfig);
                    //Read in the trained SILENCE model
                    //ReadSilModel();
                }
                else
                {
                    //Flat start the model
                    HTKHelper.InitSys(htkConfig.aOptionsStr, htkConfig);
                }

                //Re-estimate model parameters
                HTKHelper.HERest(htkConfig.numBkgIterations, htkConfig.aOptionsStr, "", htkConfig);

                //Clear Lists and Tables
                HMMSettings.confProtoDict.Clear();
                HTKHelper.SyllableList.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
        }

        public void WriteBkgPrototypeFile(string protoTypeDir)
        {
            //- proto.pcf
            //If a file 'WORD'.pcf exists in which the variable 'nStates' has a value
            //different from the one in 'proto', the program will look for a file 'WORD' 
            //that is supposed to contain the related transition matrix of size (nStates+2)x(nStates+2)
            string content =
                "<BEGINproto_config_file>\n" +
                "<COMMENT>\n\tThis PCF produces a " + htkConfig.numHmmBkgStates + " state prototype system\n" +
                "<BEGINsys_setup>\n\tnStates: " + htkConfig.numHmmBkgStates + "\n" +
                   "\tsWidths: " + htkConfig.VecSize + "\n" +      //################################################# 
                   "\t#mixes: 1\n" +
                   "\tparmKind: " + htkConfig.TARGETKIND + "\n" +  //#################################################
                   "\tvecSize: " + htkConfig.VecSize + "\n" +      //#################################################
                   "\t#outDir: " + htkConfig.protoFN + "\n\n" +
                "<ENDsys_setup>\n" +
                "<ENDproto_config_file>\n";
                        
            StreamWriter wltWriter = null;

            try
            {
                wltWriter = File.CreateText(protoTypeDir + "\\" + htkConfig.protoFN + ".pcf");
                wltWriter.WriteLine(content);
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

        public void ReadTCF(string mfcConfFN, string mainConfTrainFN)
        {
            string txtLine = "";
            
            StreamReader fReader = null;
            StreamWriter fWriter = null;
            try
            {
                fReader = new StreamReader(mfcConfFN);
                try
                {
                    fWriter = File.CreateText(mainConfTrainFN);

                    while ((txtLine = fReader.ReadLine()) != null) //write all lines to file except SOURCEFORMAT
                    {
                        if (Regex.IsMatch(txtLine.ToUpper(), @"SOURCEFORMAT.*"))
                        {
                            continue; //skip this line
                        }
                        fWriter.WriteLine(txtLine);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("Could not create codetrain file.");
                    throw (e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw (e);
                }
                finally
                {
                    if (fWriter != null)
                    {
                        fWriter.Flush();
                        fWriter.Close();
                    }
                }

            }
            catch (IOException e)
            {
                Console.WriteLine("Could not find configuration file: {0}", mfcConfFN);
                throw (e);
            }
            finally
            {
                if (fReader != null)
                {
                    fReader.Close();
                }
            }
        }

        public void ScoreModel(bool trueSet)
        {                      
            StreamReader resultsReader = null;
            StreamWriter scriptWriter = null;
            StreamWriter modifResultsWriter = null;
            StreamReader bckScoreReader = null;

            string resultsReaderF, scriptWriterF, modifResultsWriterF, bckScoreReaderF;
            

            try
            {
                if (trueSet)
                {
                    resultsReaderF = htkConfig.resultTrue;
                    scriptWriterF = htkConfig.audioSegmTrue;
                    modifResultsWriterF = htkConfig.modifResultTrue;
                    bckScoreReaderF = htkConfig.bckScoreTrue;
                }
                else
                {
                    resultsReaderF = htkConfig.resultFalse;
                    scriptWriterF = htkConfig.audioSegmFalse;
                    modifResultsWriterF = htkConfig.modifResultFalse;
                    bckScoreReaderF = htkConfig.bckScoreFalse;
                }

                resultsReader = new StreamReader(resultsReaderF); 
                scriptWriter = File.CreateText(scriptWriterF);

                //Create script containing logical files: segments of the mfc files indicized by frame numbers
                //Also close the stream writer as the file will be used by HVite
                if(!CreateAudioSegmentsScript(resultsReader, scriptWriter))
                {
                    Console.WriteLine("No '{0}' call found in {1}. Nothing to score.", htkConfig.CallName, resultsReaderF);
                    return;
                }

                if (scriptWriter != null)
                {
                    scriptWriter.Flush();
                    scriptWriter.Close();
                }
                if (resultsReader != null) resultsReader.Close();

                //Score the BKG model over the VOCALIZATION frames
                HTKHelper.HVite(htkConfig.MfccConfig2FN, htkConfig.tgtDir2Bkg, scriptWriterF, htkConfig.wordNetBkg,
                                htkConfig.DictFileBkg, bckScoreReaderF, htkConfig.monophonesBkg, htkConfig.HViteExecutable);
                             
                resultsReader = new StreamReader(resultsReaderF);
                modifResultsWriter = new StreamWriter(modifResultsWriterF);
                //Open the result file produced by HVite
                bckScoreReader = new StreamReader(bckScoreReaderF);

                //Modify the result file by adding the BKG scores
                AddBkgScores(resultsReader, bckScoreReader, modifResultsWriter);

                if (modifResultsWriter != null)
                {
                    modifResultsWriter.Flush();
                    modifResultsWriter.Close();
                }
                if (resultsReader != null) resultsReader.Close();
                if (bckScoreReader != null) bckScoreReader.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Could not score the BACKGROUND model.");
                throw (e);
            }
        }

        public void ScoreModel(string MfccConfig2FN, string tgtDir2Bkg, string resultsReaderF,
                            string wordNetBkg, string DictFileBkg, string monophonesBkg,
                            string scriptWriterF, string modifResultsWriterF, string bckScoreReaderF)
        {
            StreamReader resultsReader = null;
            StreamWriter scriptWriter = null;
            StreamWriter modifResultsWriter = null;
            StreamReader bckScoreReader = null;

            try
            {

                resultsReader = new StreamReader(resultsReaderF);
                scriptWriter = File.CreateText(scriptWriterF);

                //Create script containing logical files: segments of the mfc files indicized by frame numbers
                //Also close the stream writer as the file will be used by HVite
                if (!CreateAudioSegmentsScript(resultsReader, scriptWriter))
                {
                    Console.WriteLine("No '{0}' call found in {1}. Nothing to score.", htkConfig.CallName, resultsReaderF);
                    return;
                }

                if (scriptWriter != null)
                {
                    scriptWriter.Flush();
                    scriptWriter.Close();
                }
                if (resultsReader != null) resultsReader.Close();

                //Score the BKG model over the VOCALIZATION frames
                HTKHelper.HVite(MfccConfig2FN, tgtDir2Bkg, scriptWriterF, 
                                wordNetBkg, DictFileBkg, bckScoreReaderF, monophonesBkg, htkConfig.HViteExecutable);

                resultsReader = new StreamReader(resultsReaderF);
                modifResultsWriter = new StreamWriter(modifResultsWriterF);
                //Open the result file produced by HVite
                bckScoreReader = new StreamReader(bckScoreReaderF);

                //Modify the result file by adding the BKG scores
                AddBkgScores(resultsReader, bckScoreReader, modifResultsWriter);

                if (modifResultsWriter != null)
                {
                    modifResultsWriter.Flush();
                    modifResultsWriter.Close();
                }
                if (resultsReader != null) resultsReader.Close();
                if (bckScoreReader != null) bckScoreReader.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Could not score the BACKGROUND model.");
                throw (e);
            }
        }

        public bool CreateAudioSegmentsScript(StreamReader fReader, StreamWriter fWriter)
        {
            string regex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+\w+";
            string txtLine = "";
            int intIndex = 0;
            string logicalFile, physicalFile, currPath = null;
            bool valid = false;
            bool callPresent = false;

            while ((txtLine = fReader.ReadLine()) != null)
            {
                if (txtLine == ".")
                {
                    intIndex = 0;
                    valid = false;
                    continue;
                }

                if (txtLine.StartsWith("\""))
                {
                    //Unquote string
                    txtLine = txtLine.Replace("\"", "");
                    currPath = Path.ChangeExtension(txtLine, "mfc");
                    valid = true;
                }

                if (Regex.IsMatch(txtLine, regex)
                    && valid)
                {
                    string[] param = Regex.Split(txtLine, @"\s+");
                    long start = long.Parse(param[0]);
                    long end = long.Parse(param[1]);
                    string name = param[2];
                    float score = float.Parse(param[3]);

                    if (name == "SIL")
                        continue;

                    float denomin = float.Parse(htkConfig.TARGETRATE);
                    int frameStart = (int)(start / denomin);
                    int frameEnd = (int)(end / denomin) - 1;

                    logicalFile = Path.GetFileNameWithoutExtension(currPath);
                    logicalFile += "_" + intIndex++ + ".mfc";
                    physicalFile = currPath + "[" + frameStart.ToString() + "," + frameEnd.ToString() + "]";

                    fWriter.WriteLine(logicalFile + "=" + physicalFile);

                    callPresent = true;
                }
            }            
            return callPresent;
        }

        public void AddBkgScores(StreamReader resultsReader, StreamReader bckScoreReader, StreamWriter modifResultsWriter)
        {
            string bckScoreLine = "";
            string resultsLine = "";

            string regex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+\w+";


            while ((bckScoreLine = bckScoreReader.ReadLine()) != null)
            {
                if (Regex.IsMatch(bckScoreLine, regex))
                {
                    string[] bkgParam = Regex.Split(bckScoreLine, @"\s+");
                    float score = float.Parse(bkgParam[3]);

                    while ((resultsLine = resultsReader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(resultsLine, regex))
                        {                                
                            string[] resParam = Regex.Split(resultsLine, @"\s+");
                            string name = resParam[2];
                            if (name != "SIL")
                            {                                    
                                resultsLine += " " + score.ToString();
                                modifResultsWriter.WriteLine(resultsLine);
                                break;
                            }
                        }
                        modifResultsWriter.WriteLine(resultsLine);
                    }                       
                }
            }
            //closing char for the mlf file
            modifResultsWriter.WriteLine(".");

        }
 
    }
}
