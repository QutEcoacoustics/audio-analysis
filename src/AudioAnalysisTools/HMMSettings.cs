using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioAnalysisTools.HTKTools
{
    public class HMMSettings
    {

        #region Variables
        //static Dictionary<string, string> dictionary = new Dictionary<string, string>();        
        static Dictionary<string, string> confParam = new Dictionary<string, string>();

        //configuration dictionary. Contains params for each HMM to build
        public static Dictionary<string, Dictionary<string, string>> confProtoDict = new Dictionary<string, Dictionary<string, string>>();
        
        static string separator = @"\s*:\s*";
        static string parameter;
        static bool validData = false;
        
        #endregion

        #region Properties
        public static Dictionary<string, string> ConfigParam
        {
            get { return confParam; }
        }

        //public static Dictionary<string, Dictionary<string, string>> ConfProtoDict
        //{
        //    get { return confProtoDict; }
        //}

        #endregion

        #region Constructor
        #endregion

        #region Methods
        public static void ReadTCF(string mainConfigFN, string mfcConfFN, string mainConfTrainFN)
        {
            string txtLine = "";
            //Check if the MFC configuration file exists
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
                    LoggedConsole.WriteLine("Could not create codetrain file.");
                    throw (e);
                }
                catch (Exception e)
                {
                    LoggedConsole.WriteLine(e);
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
                LoggedConsole.WriteLine("Could not find configuration file: {0}", mfcConfFN);
                throw (e);
            }
            finally
            {
                if (mfcReader != null)
                {
                    mfcReader.Close();
                }
            }

            //string cwd = Directory.GetCurrentDirectory();
            //if(File.Exists(fileName))
            
            StreamReader objReader = null;
            try
            {
                LoggedConsole.WriteLine("Reading Main    Config file: " + mainConfigFN);
                objReader = new StreamReader(mainConfigFN);
                while ((txtLine = objReader.ReadLine()) != null)
                {
                    if (Regex.IsMatch(txtLine.ToUpper(), @"<ENDSYS_SETUP>") ||
                        Regex.IsMatch(txtLine.ToUpper(), @"<ENDTOOL_STEPS>"))
                    {
                        validData = false;
                    }

                    if (validData)
                    {
                        if (Regex.IsMatch(txtLine, @"^\s*$")) //take off space characters
                            continue;

                        string[] param = Regex.Split(txtLine, separator);
                        Match m = Regex.Match(param[0].ToUpper(), @"\b\w+\b");
                        parameter = m.ToString();

                        if (parameter.CompareTo("NMIXES") == 0)
                        {
                            //TO DO: handle Gaussian Mixtures Input
                        }

                        string value = "";
                        if (confParam.TryGetValue(parameter, out value))
                        {
                            confParam[parameter] = param[1].ToUpper();
                        }
                        else
                        {
                            confParam.Add(parameter, param[1].ToUpper());
                        }

                    }

                    if (Regex.IsMatch(txtLine.ToUpper(), @"<BEGINSYS_SETUP>") ||
                            Regex.IsMatch(txtLine.ToUpper(), @"<BEGINTOOL_STEPS>"))
                    {
                        validData = true;
                    }

                }
                //print config file
                LoggedConsole.WriteLine("Main Configuration File");
                LoggedConsole.WriteLine("=======================");
                LoggedConsole.WriteLine("{0,-18}{1,-1}", "Parameter", "Value");
                LoggedConsole.WriteLine("-----------------------");
                foreach (KeyValuePair<string, string> pair in confParam)
                {
                    LoggedConsole.WriteLine("{0,-18}{1,-1:D}", pair.Key, pair.Value);
                }
            }
            catch (IOException e)
            {
                LoggedConsole.WriteLine("Could not find configuration file: {0}", mainConfigFN);
                throw (e);
            }
            catch (Exception e)
            {
                LoggedConsole.WriteLine(e);
                throw (e);
            }
            finally
            {
                if (objReader != null)
                {
                    objReader.Close();
                }
            }
        }



        //public static void ReadPCF(string protoCfgDir, Dictionary<string, string> settings)
        public static void ReadPCF(string protoCfgDir)
        {
            LoggedConsole.WriteLine("\nStarting static method: HMMBuilder.HMMSettings.ReadPCF()");
            //read configuration for each file in protoConfigs directory
            LoggedConsole.WriteLine(" Read prototype configuration (.pcf) files in directory: " + protoCfgDir);

            string pcfWildCard = "*.pcf";

            int maxStates =  int.MinValue;

            StreamReader objReader = null;
            try
            {
                DirectoryInfo Dir = new DirectoryInfo(protoCfgDir);
                FileInfo[] FileList = Dir.GetFiles(pcfWildCard, SearchOption.TopDirectoryOnly);

                string txtLine = "";
                foreach (FileInfo FI in FileList)
                {
                    objReader = new StreamReader(FI.FullName);
                    Dictionary<string, string> confProto = new Dictionary<string, string>();

                    while ((txtLine = objReader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(txtLine.ToUpper(), @"<ENDSYS_SETUP>"))
                        {
                            validData = false;
                        }

                        if (validData)
                        {
                            if (Regex.IsMatch(txtLine, @"^\s*$")) //take off white lines
                                continue;

                            string[] param = Regex.Split(txtLine, separator);
                            Match m = Regex.Match(param[0].ToUpper(), @"\b\w+\b");
                            parameter = m.ToString();

                            if (parameter.Equals("NSTATES"))
                                if (int.Parse(param[1]) > maxStates)
                                    maxStates = int.Parse(param[1]);

                            string value = "";
                            if (confProto.TryGetValue(parameter, out value))
                            {
                                confProto[parameter] = param[1];
                            }
                            else
                            {
                                confProto.Add(parameter, param[1]);
                            }

                        }

                        if (Regex.IsMatch(txtLine.ToUpper(), @"<BEGINSYS_SETUP>"))
                        {
                            validData = true;
                        }

                    }
                    if (objReader != null)
                    {
                        objReader.Close();
                    }

                    //print config file
                    LoggedConsole.WriteLine("===========================");
                    LoggedConsole.WriteLine("Configuration for {0}", Path.GetFileNameWithoutExtension(FI.FullName));
                    LoggedConsole.WriteLine("===========================");
                    LoggedConsole.WriteLine("{0,-18}{1,-1}", "Parameter", "Value");
                    LoggedConsole.WriteLine("-----------------------");
                    foreach (KeyValuePair<string, string> pair in confProto)
                    {
                        LoggedConsole.WriteLine("{0,-18}{1,-1:D}", pair.Key, pair.Value);
                    }

                    confProtoDict.Add(Path.GetFileNameWithoutExtension(FI.FullName), confProto);

                } //end FOREACH PCF file in the protoConfig directory

                //makes sure that 'proto', if exsists, contains the biggest number of states among all pcf files
                if (!confProtoDict.ContainsKey("proto"))
                    throw new Exception("Prototype file 'proto' not found in '" + protoCfgDir + "'.");
                confProtoDict["proto"]["NSTATES"] = maxStates.ToString();

            }
            catch (Exception e)
            {
                LoggedConsole.WriteLine(e);
                throw (e);
            }

            //LoggedConsole.Write("\nPress ENTER key to continue:");
            //Console.ReadLine();
        } //end METHOD ReadPCF() to read and train prototype configurations

        #endregion




        public static void WriteHMMprototypeFile(string prototypeHMM)
        {
            LoggedConsole.WriteLine("\nStarting static method: HMMBuilder.HMMSettings.WriteHMMprototypeFile()");

            //Create prototype HMM based on the call (non-SIL) parameters
            LoggedConsole.WriteLine(" Create prototype HMM based on the call (non-SIL) parameters in file <" + prototypeHMM + ">");

            string prototypeDir = Path.GetDirectoryName(prototypeHMM);
            StreamWriter protoWriter = null;
            try
            {               
                // Create prototype dir if it does not exist.
                if (!Directory.Exists(prototypeDir))
                {
                    LoggedConsole.WriteLine(" Create prototype dir: " + prototypeDir);
                    Directory.CreateDirectory(prototypeDir);
                }

                LoggedConsole.WriteLine(" Create HMM prototype file: " + prototypeHMM);
                protoWriter = File.CreateText(prototypeHMM);

                //try to get the key 'proto' from the dictionary
                Dictionary<string, string> tmpDictVal = new Dictionary<string, string>();

                confProtoDict.TryGetValue("proto", out tmpDictVal);

                //write global options
                string vecSize = "";
                string parmKind = "";
                if (tmpDictVal.TryGetValue("VECSIZE", out vecSize))
                {
                    if (tmpDictVal.TryGetValue("PARMKIND", out parmKind))
                    {
                        protoWriter.WriteLine("~o <VecSize> " + vecSize + " <" + parmKind + ">");
                        protoWriter.WriteLine("~h \"proto\"");
                    }
                    else
                    {
                        LoggedConsole.WriteLine("Parameter 'ParmKind' not specified in 'proto.pcf'");
                        //TO DO: create custom exception. For now throw something :-)
                        throw new Exception("Parameter 'ParmKind' not specified in 'proto.pcf'");
                    }
                }
                else
                {
                    LoggedConsole.WriteLine("Parameter 'VecSize' not specified in 'proto.pcf'");
                    //TO DO: create custom exception. For now throw something :-)
                    throw new Exception("Parameter 'VecSize' not specified in 'proto.pcf'");

                }

                protoWriter.WriteLine("<BeginHMM>");

                string nStates = "";
                if (tmpDictVal.TryGetValue("NSTATES", out nStates))
                {
                    protoWriter.WriteLine("  <NumStates> {0}", int.Parse(nStates) + 2);
                }
                else
                {
                    LoggedConsole.WriteLine("Parameter 'nStates' not specified in 'proto.pcf'");
                    //TO DO: create custom exception. For now throw something :-)
                    throw new Exception("Parameter 'nStates' not specified in 'proto.pcf'");
                }

                //write states
                string sWidths = "";
                if (tmpDictVal.TryGetValue("SWIDTHS", out sWidths))
                {
                    for (int i = 1; i <= int.Parse(nStates); i++)
                    {
                        protoWriter.WriteLine("  <State> {0}", i + 1);
                        protoWriter.WriteLine("    <Mean> " + sWidths);
                        string pad = "      ";
                        string tmpLine = "";
                        for (int j = 1; j <= int.Parse(sWidths); j++)
                        {
                            tmpLine += "0.0 ";
                        }
                        protoWriter.WriteLine(pad + tmpLine);
                        tmpLine = "";
                        protoWriter.WriteLine("    <Variance> " + sWidths);
                        for (int j = 1; j <= int.Parse(sWidths); j++)
                        {
                            tmpLine += "1.0 ";
                        }
                        protoWriter.WriteLine(pad + tmpLine);
                    }
                }
                else
                {
                    LoggedConsole.WriteLine("Parameter 'sWidths' not specified in 'proto.pcf'");
                    //TO DO: create custom exception. For now throw something :-)
                    throw new Exception("Parameter 'sWidths' not specified in 'proto.pcf'");
                }

                //write Trans Matrix
                protoWriter.WriteLine("<TransP> {0}", int.Parse(nStates) + 2);
                for (int i = 1; i <= int.Parse(nStates) + 2; i++)
                {
                    for (int j = 1; j <= int.Parse(nStates) + 2; j++)
                    {
                        if ((i == 1) && (j == 2))
                        {
                            protoWriter.Write("  1.000e+0");
                        }
                        else
                            if ((i == j) && (i != 1) && (i != int.Parse(nStates) + 2))
                            {
                                if (i != int.Parse(nStates) + 1) protoWriter.Write("  6.000e-1");
                                else protoWriter.Write("  7.000e-1");
                            }
                            else
                                if (i == (j - 1))
                                {
                                    if (i != int.Parse(nStates) + 1) protoWriter.Write("  4.000e-1");
                                    else protoWriter.Write("  3.000e-1");
                                }
                                else
                                {
                                    protoWriter.Write("  0.000e+0");
                                }
                    }
                    protoWriter.Write("\n");
                }

                protoWriter.WriteLine("<EndHMM>");

            } //end try for writing the HMM prototype file
            catch (IOException e)
            {
                LoggedConsole.WriteLine("Could not create the 'proto' file.");
                LoggedConsole.WriteLine(e.ToString());
                throw (e);
            }
            finally
            {
                if (protoWriter != null)
                {
                    protoWriter.Flush();
                    protoWriter.Close();
                }
            }

        } //end METHOD ReadPCF() to read and train prototype configurations



    }
}
