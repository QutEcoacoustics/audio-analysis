using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HMMBuilder
{
    public class BKGTrainer
    {
        #region Variables
        static HTKConfig htkConfig;
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
                if (!Directory.Exists(htkConfig.ConfigDirBkg)) Directory.CreateDirectory(htkConfig.ConfigDirBkg);                
                htkConfig.WriteMfccConfigFile(htkConfig.MfccConfigFNBkg);       //Write the mfcc for FV extraction

                ReadTCF(htkConfig.MfccConfigFNBkg, htkConfig.MfccConfig2FNBkg); //Write the mfcc for training
                
                if (!Directory.Exists(htkConfig.ProtoConfDirBkg)) Directory.CreateDirectory(htkConfig.ProtoConfDirBkg);
                WriteBkgPrototypeFile(htkConfig.ProtoConfDirBkg);  //prototype file

                //HTKHelper.HCopy(htkConfig.aOptionsStr, htkConfig, true);
                
                HTKHelper.CreateWLT(htkConfig, ref bkgLabel, false);

                HTKHelper.WriteDictionary(htkConfig);

                HMMSettings.ReadPCF(htkConfig.ProtoConfDirBkg);

                HMMSettings.WriteHMMprototypeFile(htkConfig.prototypeHMMBkg);

                HTKHelper.InitSys(htkConfig.aOptionsStr, htkConfig);

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
            
            StreamReader mfcReader = null;
            StreamWriter mfcWriter = null;
            try
            {
                mfcReader = new StreamReader(mfcConfFN);
                try
                {
                    mfcWriter = File.CreateText(mainConfTrainFN);

                    while ((txtLine = mfcReader.ReadLine()) != null) //write all lines to file except SOURCEFORMAT
                    {
                        if (Regex.IsMatch(txtLine.ToUpper(), @"SOURCEFORMAT.*"))
                        {
                            continue; //skip this line
                        }
                        mfcWriter.WriteLine(txtLine);
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
                    if (mfcWriter != null)
                    {
                        mfcWriter.Flush();
                        mfcWriter.Close();
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
                if (mfcReader != null)
                {
                    mfcReader.Close();
                }
            }
        }
    }
}
