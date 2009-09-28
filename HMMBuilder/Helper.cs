using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HMMBuilder
{
    class Helper
    {
        public static float ComputeAccuracy(string resultTrue, string resultFalse, ref string vocalization, float threshold)
        {
            float accuracy = 0;

            //TO DO: check if the files exists

            StreamReader trueReader = null;
            StreamReader falseReader = null;

            int tpCount = 0;  //true positives
            int fpCount = 0;  //false positives
            int trueSCount = 0;
            int falseSCount = 0;

            string txtLine = null;
            try
            {
                trueReader = new StreamReader(resultTrue);
                falseReader = new StreamReader(resultFalse);
                bool valid = true;
                while ((txtLine = trueReader.ReadLine()) != null)
                {                    
                    if (Regex.IsMatch(txtLine, @"^\d+\s+\d+\s+\w+") &&
                        valid)
                    {
                        string[] param = Regex.Split(txtLine, @"\s+");
                        if(param[2].Equals(vocalization) &&
                            float.Parse(param[3]) >= threshold)
                        {
                            tpCount++;
                            valid = false;
                        }
                    }
                    if(Regex.IsMatch(txtLine, @"^\.$"))
                    {
                        trueSCount++;
                        valid = true;
                    }
                }
                valid = true;

                while ((txtLine = falseReader.ReadLine()) != null)
                {
                    if (Regex.IsMatch(txtLine, @"^\d+\s+\d+\s+\w+") &&
                        valid)
                    {
                        string[] param = Regex.Split(txtLine, @"\s+");
                        if (param[2].Equals(vocalization) &&
                            float.Parse(param[3]) >= threshold)
                        {
                            fpCount++;
                            valid = false;
                        }
                    }
                    if(Regex.IsMatch(txtLine, @"^\.$"))
                    {
                        falseSCount++;
                        valid = true;
                    }
                }

                int tnCount = falseSCount - fpCount;

                accuracy = (float)(tpCount + tnCount) / (trueSCount + falseSCount);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            finally
            {
                if (trueReader != null)
                {
                    trueReader.Close();
                }
                if (trueReader != null)
                {
                    falseReader.Close();
                }
            }

            return accuracy;
        }
    }
}
