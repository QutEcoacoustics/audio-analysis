using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HMMBuilder
{
    class Helper
    {
        public static void ComputeAccuracy(string resultTrue, string resultFalse, ref string vocalization, float threshold,
                             out float tpPercent, out float tnPercent, out float accuracy, out float avTPScore, out float avFPScore)
        {
            int tpCount = 0;  //true positives
            int fpCount = 0;  //false positives
            int trueSCount  = 0;
            int falseSCount = 0;
            avTPScore = 0.0f;
            avFPScore = 0.0f;

            CountHits(resultTrue,  ref vocalization, threshold, out tpCount, out trueSCount,  out avTPScore);
            CountHits(resultFalse, ref vocalization, threshold, out fpCount, out falseSCount, out avFPScore);

            int tnCount = falseSCount - fpCount;
            tpPercent = (float)(tpCount) * 100 / (trueSCount + falseSCount);
            tnPercent = (float)(tnCount) * 100 / (trueSCount + falseSCount);
            accuracy = tpPercent + tnPercent;
        } //end method ComputeAccuracy()




        public static void CountHits(string resultFile, ref string vocalization, float threshold, out int hits, out int total, out float avScore)
        {
            //TO DO: check if the file exists
            StreamReader reader = null;
            hits  = 0;
            total = 0;
            avScore = 0.0f;

            string txtLine = null;
            try
            {
                reader = new StreamReader(resultFile);
                bool valid = true;
                while ((txtLine = reader.ReadLine()) != null)
                {
                    if (Regex.IsMatch(txtLine, @"^\d+\s+\d+\s+\w+") &&
                        valid)
                    {
                        string[] param = Regex.Split(txtLine, @"\s+");
                        float score = float.Parse(param[3]);
                        if (param[2].Equals(vocalization) &&
                            float.Parse(param[3]) >= threshold)
                        {
                            avScore += score;
                            hits++;
                            valid = false;
                        }
                    }
                    if (Regex.IsMatch(txtLine, @"^\.$"))
                    {
                        total++;
                        valid = true;
                    }
                }
            }// end try
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
            finally
            {
                avScore /= hits;
                //Console.WriteLine("hits=" + hits + "/" + total + "   avScore=" + avScore);
                if (reader != null) reader.Close();
            } //end finally
        } //end Mehtod CountHits()


    } //end class Helper
} //namespace
