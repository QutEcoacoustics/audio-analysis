using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HMMBuilder
{
    class Helper
    {

        private static readonly double OneOverRoot2Pi = 1.0 / Math.Sqrt(2 * Math.PI);       
        
        /// <summary>
        /// Returns the probability density function evaluated at a given value. 
        /// </summary>
        /// <returns>Void</returns> 
        public static double PDF(double x, double mean, double variance)
        {
            if (variance <= 0.0)
            {
                string msg = string.Format("Expected variance > 0 in NormalDistribution. Found variance = {0}", variance);
                throw new Exception(msg);
            }

            double sigma = Math.Sqrt(variance);
            double oneOverSigma = 1.0 / sigma;
            double oneOverSigmaSqr = oneOverSigma * oneOverSigma;
            double c = oneOverSigma * OneOverRoot2Pi;

            
            double y = (x - mean);
            double xMinusMuSqr = y * y;

            return c * Math.Exp(-0.5 * xMinusMuSqr * oneOverSigmaSqr);
        }
        
        
        /// <summary>
        /// Compute Probability Distribution 
        /// </summary>
        /// <returns>Void</returns>   
        public static void ComputePDF(string masterLabelFile, 
                                        ref Dictionary<string,double> meanDuration,
                                        ref Dictionary<string,double> varianceDuration,
                                        string vocalization)
        { 
            //check if the mlf exists
            StreamReader mlfReader = null;
            string txtLine = null;
            List<double> durations = new List<double>();

            double mean, variance, stdDev;

            try
            {
                mlfReader = new StreamReader(masterLabelFile);

                while ((txtLine = mlfReader.ReadLine()) != null) //write all lines to file except SOURCEFORMAT
                {
                    //this regex is to match numbers with exponents: [-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?
                    if (Regex.IsMatch(txtLine, @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+\w+\s?$"))
                    {
                        string[] param = Regex.Split(txtLine, @"\s+");
                        if (param[2].Equals(vocalization))
                        {
                            long start = long.Parse(param[0],System.Globalization.NumberStyles.Float);
                            long end = long.Parse(param[1], System.Globalization.NumberStyles.Float);
                            durations.Add(TimeSpan.FromTicks(end - start).TotalSeconds); //duration in seconds
                        }
                    }
                }
                mean = getMean(durations);
                variance = getVariance(durations);
                stdDev = getStandardDeviation(variance);
                                
                meanDuration.Add(vocalization, mean);
                varianceDuration.Add(vocalization, variance);            
            }
            catch (IOException e)
            {
                Console.WriteLine("Could not find the Master Label File '{0}'.", masterLabelFile);
                throw (e);
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

        public static double getVariance(List<double> nums)
        {
            if (nums.Count > 1)
            {

                // Get the average of the values
                double avg = getMean(nums);

                // Now figure out how far each point is from the mean
                // So we subtract from the number the average
                // Then raise it to the power of 2
                double sumOfSquares = 0.0;

                foreach (double num in nums)
                {
                    sumOfSquares += Math.Pow((num - avg), 2.0);
                }

                // Finally divide it by n - 1 (for standard deviation variance)
                // Or use length without subtracting one ( for population standard deviation variance)
                return sumOfSquares / (double)(nums.Count - 1);
            }
            else { return 0.0; }
        }

        // Square root the variance to get the standard deviation
        public static double getStandardDeviation(double variance)
        {
            return Math.Sqrt(variance);
        }


        // Get the average of our values in the array
        public static double getMean(List<double> nums)
        {
            double sum = 0.0f;

            if (nums.Count > 1)
            {

                // Sum up the values
                foreach (double num in nums)
                {
                    sum += num;
                }

                // Divide by the number of values
                return sum / (double)nums.Count;
            }
            else { return (double)nums[0]; }
        }



        /// <summary>
        /// Computes accuracy of HTK test.
        /// The method calls CountHits() which normalises the HTK output score. the HTK score is normalised to time duration in seconds. 
        /// IT SHOULD be normalised to frame count i.e. sequence length. 
        /// </summary>
        /// <param name="resultTrue"></param>
        /// <param name="resultFalse"></param>
		/// <param name="mean"></param>
		/// <param name="variance"></param>
        /// <param name="vocalization"></param>
        /// <param name="threshold"></param>
        /// <param name="tpPercent"></param>
        /// <param name="tnPercent"></param>
        /// <param name="accuracy"></param>
        /// <param name="avTPScore"></param>
        /// <param name="avFPScore"></param>
        public static void ComputeAccuracy(string resultTrue, string resultFalse, 
                                            double mean, double variance,
                                            ref string vocalization, float threshold,
                                            out float tpPercent, out float tnPercent, 
                                            out float accuracy, out float avTPScore, 
                                            out float avFPScore)
        {
            int tpCount = 0;  //true positives
            int fpCount = 0;  //false positives
            int trueSCount  = 0;
            int falseSCount = 0;
            avTPScore = 0.0f;
            avFPScore = 0.0f;

            CountHits(resultTrue,  ref vocalization, mean, variance, threshold, out tpCount, out trueSCount,  out avTPScore);
            CountHits(resultFalse, ref vocalization, mean, variance, threshold, out fpCount, out falseSCount, out avFPScore);

            int tnCount = falseSCount - fpCount;
            tpPercent = (float)(tpCount) * 100 / (trueSCount + falseSCount);
            tnPercent = (float)(tnCount) * 100 / (trueSCount + falseSCount);
            accuracy = tpPercent + tnPercent;
        } //end method ComputeAccuracy()

        /// <summary>
        /// New version of CountHits() made by Alfredo .
        /// It also computes a QULAITY SCORE based on hit duration
        /// </summary>
        /// <param name="resultFile"></param>
        /// <param name="vocalization"></param>
        /// <param name="mean"></param>
        /// <param name="variance"></param>
        /// <param name="threshold"></param>
        /// <param name="hits"></param>
        /// <param name="total"></param>
        /// <param name="avScore"></param>
        public static void CountHits(string resultFile, ref string vocalization, 
                                     double mean , double variance,
                                     float threshold, out int hits, out int total, out float avScore)
        {
            //TO DO: check if the file exists
            StreamReader reader = null;
            StreamWriter writer = null;
            double lengthProb = 0.0f;
            hits  = 0;
            total = 0;
            avScore = 0.0f;

            string txtLine = null;
            try
            {
                reader = new StreamReader(resultFile);
                writer = new StreamWriter(Path.ChangeExtension(resultFile, "PDF.txt"));
                bool valid = true;
                while ((txtLine = reader.ReadLine()) != null)
                {
                    if (Regex.IsMatch(txtLine, @"^\d+\s+\d+\s+\w+") &&
                        valid)
                    {
                        //Console.WriteLine(txtLine);
                        string[] param = Regex.Split(txtLine, @"\s+");
                        long start = long.Parse(param[0]);
                        long end = long.Parse(param[1]);
                        float score = float.Parse(param[3]);

                        double duration = TimeSpan.FromTicks(end - start).TotalSeconds; //duration in seconds
                        double normScore = score / duration; //### NO LONGER NEEDED
                        lengthProb = PDF(duration, mean, variance);

                        //if (param[2].Equals(vocalization) && score >= threshold)
                        if (param[2].Equals(vocalization) && normScore >= threshold)
                        {
                            //Console.WriteLine("duration=" + duration + "   normScore=" + normScore);
                            //avScore += score;
                            avScore += (float)normScore;
                            hits++;
                            valid = false;

                            writer.WriteLine(txtLine + " " + lengthProb.ToString());
                        }
                        else
                        {
                            writer.WriteLine(txtLine);
                        }
                    }
                    else 
                    {
                        writer.WriteLine(txtLine);
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
                if (hits == 0) avScore = 0.0f;
                else 
                avScore /= hits;
                //Console.WriteLine("hits=" + hits + "/" + total + "   avScore=" + avScore);
                if (reader != null) reader.Close();

                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }
            } //end finally
        } //end Mehtod CountHits()



        /// <summary>
        /// Computes accuracy of HTK test.
        /// The method calls CountHits() which normalises the HTK output score. the HTK score is normalised to time duration in seconds. 
        /// IT SHOULD be normalised to frame count i.e. sequence length. 
        /// </summary>
        /// <param name="resultTrue"></param>
        /// <param name="resultFalse"></param>
        /// <param name="vocalization"></param>
        /// <param name="threshold"></param>
        /// <param name="tpPercent"></param>
        /// <param name="tnPercent"></param>
        /// <param name="accuracy"></param>
        /// <param name="avTPScore"></param>
        /// <param name="avFPScore"></param>
        public static void ComputeAccuracy(string resultTrue, string resultFalse, ref string vocalization, float threshold,
                             out float tpPercent, out float tnPercent, out float accuracy, out float avTPScore, out float avFPScore)
        {
            int tpCount = 0;  //true positives
            int fpCount = 0;  //false positives
            int trueSCount = 0;
            int falseSCount = 0;
            avTPScore = 0.0f;
            avFPScore = 0.0f;

            CountHits(resultTrue, ref vocalization, threshold, out tpCount, out trueSCount, out avTPScore);
            CountHits(resultFalse, ref vocalization, threshold, out fpCount, out falseSCount, out avFPScore);

            int tnCount = falseSCount - fpCount;
            tpPercent = (float)(tpCount) * 100 / (trueSCount + falseSCount);
            tnPercent = (float)(tnCount) * 100 / (trueSCount + falseSCount);
            accuracy = tpPercent + tnPercent;
        } //end method ComputeAccuracy()


        /// <summary>
        /// old verison of CountHits before ALfredo made a 9 argument version.
        /// </summary>
        /// <param name="resultFile"></param>
        /// <param name="vocalization"></param>
        /// <param name="threshold"></param>
        /// <param name="hits"></param>
        /// <param name="total"></param>
        /// <param name="avScore"></param>
        public static void CountHits(string resultFile, ref string vocalization, float threshold, out int hits, out int total, out float avScore)
        {
            //TO DO: check if the file exists
            StreamReader reader = null;
            hits = 0;
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
                        long start;
                        long end;
                        string name;
                        double normScore;
                        ParseResultLine(txtLine, out start, out end, out name, out normScore);
                        if (name.Equals(vocalization) && normScore >= threshold)
                        {
                            //Console.WriteLine("duration=" + duration + "   normScore=" + normScore);
                            //avScore += score;
                            avScore += (float)normScore;
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
                if (hits == 0) avScore = 0.0f;
                else
                    avScore /= hits;
                //Console.WriteLine("hits=" + hits + "/" + total + "   avScore=" + avScore);
                if (reader != null) reader.Close();
            } //end finally
        } //end Mehtod CountHits()


        public static void ParseResultLine(string txtLine, out long  start, out long  end, out string vocalName, out double normScore)
        {
            string[] param = Regex.Split(txtLine, @"\s+");
            start     = long.Parse(param[0]);
            end       = long.Parse(param[1]);
            vocalName = param[2];
            float score = float.Parse(param[3]);
            double duration = TimeSpan.FromTicks(end - start).TotalSeconds; //duration in seconds
            normScore = score / duration; //IMPORTANT!!!! NORMALISE SCORE FOR DURATION
        }



    } //end class Helper
} //namespace
