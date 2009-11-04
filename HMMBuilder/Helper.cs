using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HMMBuilder
{
    class Helper
    {


        public static void AverageCallDuration(string file, string regex, string vocalization, out double mean, out double sd)
        {
            Dictionary<string, double> meanDuration = new Dictionary<string, double>();
            Dictionary<string, double> varianceDuration = new Dictionary<string, double>();
            Helper.ComputePDF(file, regex, ref meanDuration, ref varianceDuration, vocalization);
            double vari;
            meanDuration.TryGetValue(vocalization, out mean);
            varianceDuration.TryGetValue(vocalization, out vari);
            sd = Math.Sqrt(vari);
        }

       
        /// <summary>
        /// Computes a Probability Distribution function 
        /// </summary>
        /// <returns>Void</returns>   
        public static void ComputePDF(string masterLabelFile, 
                                      string matchStr,
                                      ref Dictionary<string,double> meanDuration,
                                      ref Dictionary<string,double> varianceDuration,
                                      string vocalization)
        {
            //this regex is to match numbers with exponents: [-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?
            string regex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+\w+\s?$";
            //string regex = vocalisation;
            if (matchStr != null) regex = matchStr;



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
                    if (Regex.IsMatch(txtLine, regex))
                    {
                        string[] param = Regex.Split(txtLine, @"\s+");
                        if (param[2].Equals(vocalization))
                        {
                            long start = long.Parse(param[0],System.Globalization.NumberStyles.Float);
                            long end = long.Parse(param[1], System.Globalization.NumberStyles.Float);
                            double seconds = TimeSpan.FromTicks(end - start).TotalSeconds;
                            durations.Add(seconds); //duration in seconds
                            Console.WriteLine("Duration= " + seconds.ToString("f4") + "  starts " + start+">>"+end);
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

        /// <summary>
        /// Computes a Probability Distribution function 
        /// </summary>
        /// <returns>Void</returns>   
        //public static void ComputePDF(List<string> results,
        //                              string matchStr,
        //                              ref Dictionary<string, double> meanDuration,
        //                              ref Dictionary<string, double> varianceDuration,
        //                              string vocalization)
        //{
        //    //this regex is to match numbers with exponents: [-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?
        //    string regex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?\s+\w+\s?$";
        //    //string regex = vocalisation;
        //    if (matchStr != null) regex = matchStr;



        //    //check if the mlf exists
        //    StreamReader mlfReader = null;
        //    string txtLine = null;
        //    List<double> durations = new List<double>();

        //    double mean, variance, stdDev;

        //    try
        //    {
        //        int hitCount = results.Count;
        //        for (int i = 1; i < hitCount; i++)
        //        {
        //            if ((results[i] == "") || (results[i].StartsWith("."))) continue;
        //            if ((results[i].StartsWith("\"")) || (results[i].StartsWith("#"))) continue;
        //            long start;
        //            long end;
        //            string className;
        //            double score;
        //            Helper.ParseResultLine(results[i], out start, out end, out className, out score);
        //            if (!className.StartsWith(vocalization)) continue; //skip irrelevant lines

        //            //calculate hmm and quality scores
        //            double duration = TimeSpan.FromTicks(end - start).TotalSeconds; //call duration in seconds


        //                    double seconds = TimeSpan.FromTicks(end - start).TotalSeconds;
        //                    durations.Add(seconds); //duration in seconds
        //                    Console.WriteLine("Duration= " + seconds.ToString("f4") + "  starts " + start + ">>" + end);
        //        }
                    
                
        //        mean = getMean(durations);
        //        variance = getVariance(durations);
        //        stdDev = getStandardDeviation(variance);

        //        meanDuration.Add(vocalization, mean);
        //        varianceDuration.Add(vocalization, variance);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw (e);
        //    }
        //    finally
        //    {

        //    }
        //}

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
        /// <param name="frameRate">number of frames per second</param>
        /// <param name="vocalization"></param>
        /// <param name="threshold"></param>
        /// <param name="tpPercent"></param>
        /// <param name="tnPercent"></param>
        /// <param name="accuracy"></param>
        /// <param name="avTPScore"></param>
        /// <param name="avFPScore"></param>
        public static void ComputeAccuracy(string resultTrue, string resultFalse, 
                                            double mean, double variance, double frameRate,
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

            CountHits(resultTrue,  ref vocalization, mean, variance, frameRate, threshold, out tpCount, out trueSCount,  out avTPScore);
            CountHits(resultFalse, ref vocalization, mean, variance, frameRate, threshold, out fpCount, out falseSCount, out avFPScore);

            int tnCount = falseSCount - fpCount;
            tpPercent = (float)(tpCount) * 100 / (trueSCount + falseSCount);
            tnPercent = (float)(tnCount) * 100 / (trueSCount + falseSCount);
            accuracy = tpPercent + tnPercent;
        } //end method ComputeAccuracy()

        
        /// <summary>
        /// Computes a normsalised HMM score and a QULAITY SCORE based on hit duration
        /// </summary>
        /// <param name="resultFile">contains the info returned by HTK</param>
        /// <param name="vocalization">the name of the vocalisation</param>
        /// <param name="mean">mean of the training call durations</param>
        /// <param name="variance">the variance of the trianing call duraitons</param>
        /// <param name="frameRate">number of frames per second</param>
        /// <param name="scoreThreshold">use on the normalised score</param>
        /// <param name="hits">number of hits recorded over all instances</param>
        /// <param name="total">number off instances</param>
        /// <param name="avScore">average score the hits</param>
        public static void CountHits(string resultFile, ref string vocalization, 
                                     double mean , double variance, double frameRate, 
                                     float scoreThreshold, out int hits, out int total, out float avScore)
        {
            //TO DO: check if the file exists
            StreamReader reader = null;
            StreamWriter writer = null;
            //double lengthProb = 0.0f;
            double stddev = Math.Sqrt(variance);
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
                    if (Regex.IsMatch(txtLine, @"^\d+\s+\d+\s+\w+") && valid)
                    {
                        //Console.WriteLine(txtLine);
                        string[] param = Regex.Split(txtLine, @"\s+");
                        long start = long.Parse(param[0]);
                        long end = long.Parse(param[1]);
                        string name = param[2];
                        float score = float.Parse(param[3]);

                        //if (param[2].Equals(vocalization) && score >= threshold)
                        if (name.Equals(vocalization))
                        {
                            //normalise the score
                            double duration   = TimeSpan.FromTicks(end - start).TotalSeconds; //duration in seconds
                            double qualityThreshold = 1.96;
                            double normScore, durationScore;
                            bool isHit;
                            ComputeHit(score, duration, frameRate, mean, stddev, scoreThreshold, qualityThreshold, 
                                       out normScore, out durationScore, out isHit);

                            if (isHit)
                            {
                                //Console.WriteLine("duration=" + duration.ToString("f3") + " (p=" + lengthProb.ToString("f3") + ")   normScore=" + normScore.ToString("f0"));
                                avScore += (float)normScore;
                                hits++;
                                valid = false;
                                txtLine += " " + normScore.ToString("f1") + "  " + durationScore.ToString("f5") + "  ###HIT!! scorethreshold=" + scoreThreshold.ToString("f1");
                            }
                            else
                            {
                                txtLine += " " + normScore.ToString("f1") + "  " + durationScore.ToString("f5") + "  ##MISS!!";

                            }
                        } //end if vocalisation
                    }
                    writer.WriteLine(txtLine);

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
        } //end Method CountHits()



        public static void ComputeHit(double score, double duration, double frameRate, double mean, double sd,
                                      double scoreThreshold, double qualityThreshold, 
                                      out double normScore, out double qualityScore, out bool isHit)
        {
            //normalise the score
            double frameCount = duration * frameRate;
            normScore = score / frameCount;
            //qualityScore = PDFvalue(duration, mean, sd);
            qualityScore = Math.Abs((duration - mean) / sd);

            //if (normScore > scoreThreshold) isHit = true; else isHit = false;  //ignore quality score
            //use of quality score gives better result on false test instances
            if ((normScore > scoreThreshold) && (qualityScore < qualityThreshold)) isHit = true; else isHit = false;
        }

        
        //private static readonly double OneOverRoot2Pi = 1.0 / Math.Sqrt(2 * Math.PI);



        /// <summary>
        /// Returns a probability, that is, the value of a PDF at a given value of x. 
        /// </summary>
        /// <returns>Void</returns> 
        public static double PDFvalue(double x, double mean, double sigma)
        {
            double OneOverRoot2Pi = 1.0 / Math.Sqrt(2 * Math.PI);
            if (sigma <= 0.0)
            {
                string msg = string.Format("Expected sigma > 0 in NormalDistribution. Found sigma = {0}", sigma);
                throw new Exception(msg);
            }

            //double sigma = Math.Sqrt(variance);
            double oneOverSigma = 1.0 / sigma;
            double oneOverSigmaSqr = oneOverSigma * oneOverSigma;
            double c = oneOverSigma * OneOverRoot2Pi;
            double y = (x - mean);
            double xMinusMuSqr = y * y;

            return c * Math.Exp(-0.5 * xMinusMuSqr * oneOverSigmaSqr);
        }



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
