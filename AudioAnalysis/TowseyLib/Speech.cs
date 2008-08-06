using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
    public class Speech
    {



        public static double[,] DecibelSpectra(double[,] spectra)
        {
            int frameCount = spectra.GetLength(0);
            int binCount = spectra.GetLength(1);

            double[,] SPEC = new double[frameCount, binCount];

            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                for (int j = 0; j < binCount; j++) //foreach freq bin
                {
                    double amplitude = spectra[i, j];
                    double power = amplitude * amplitude; //convert amplitude to power
                    power = 10 * Math.Log10(power);    //convert to decibels
                    ////NOTE: the decibels calculation should be a ratio. 
                    //// Here the ratio is implied ie relative to the power in the normalised wav signal
                    SPEC[i, j] = power;
                }
            } //end of all frames
            return SPEC;
        }



        public static int[] VocalizationDetection(double[] decibels, double lowerDBThreshold, double upperDBThreshold, int k1_k2delay, int syllableDelay, int minPulse, int[] zeroCrossings)
        {
            int L = decibels.Length;
            int[] state = new int[L];
            int lowEnergyID = 0;
            int hiEnergyID  = 0;
            for (int i = 0; i < L; i++)//foreach time step
            {
                if (decibels[i] < lowerDBThreshold)
                {
                    lowEnergyID = i;
                    int delay = i - hiEnergyID;
                    if (delay < k1_k2delay) for (int j = 1; j < delay; j++) state[i - j] = 2;
                    state[i] = 0;
                }
                if (decibels[i] > upperDBThreshold)
                {
                    hiEnergyID = i;
                    int delay = i - lowEnergyID;
                    if (delay < k1_k2delay) for (int j = 1; j < delay; j++) state[i - j] = 2;
                    state[i] = 2;
                }
            }

            // fill in probable inter-syllable gaps
            bool sig = true;
            int count = 0;
            for (int i = 0; i < L; i++)//foreach time step
            {
                if (state[i] == 0)
                {
                    sig = false;
                    count++;
                }
                else
                if (state[i] == 2)
                {
                    //Console.WriteLine("count["+i+"]="+count);
                    sig = true;
                    if (count < syllableDelay) for (int j = 1; j <= count; j++) state[i - j] = 1;
                    count = 0;
                }
            }
            return state;
        }


        public static double LinearInterpolate(double x0, double x1, double y0, double y1, double x2)
        {
            double dX = x1 - x0;
            double dY = y1 - y0;
            double ratio = (x2-x0) / dX;
            double y2 = y0 + (ratio * dY);
            return y2;
        }
        public static double LinearIntegral(double x0, double x1, double y0, double y1)
        {
            double dX = x1 - x0;
            double dY = y1 - y0;
            double area = (dX * y0) + (dX * dY * 0.5);
            return area;
        }
        public static double LinearIntegral(int x0, int x1, double y0, double y1)
        {
            double dX = x1 - x0;
            double dY = y1 - y0;
            double area = (dX * y0) + (dX * dY * 0.5);
            return area;
        }

        public static double MelIntegral(double f0, double f1, double y0, double y1)
        {
            //double p = 2595.0 / Math.Log(10.0);
            const double p = 1127.01048;
            const double q = 700.0;
            double dF = f0 - f1;// if reverse this, image intensity is reversed
            double x = dF / (q + f1);
            double x1 = Math.Log(x + 1.0);
            if (Math.Abs(x1 - x) > 1.0e-10)
                 return p * ((y1 - y0) + (y0 - y1 * (x + 1.0)) * (x1 / x));
            else return 0.0;
        }

        public static double Mel(double f)
        {
            if (f <= 1000) return f; //linear below 1 kHz
            return 2595.0 * Math.Log10(1.0 + f / 700.0);
        }

        public static double InverseMel(double m)
        {
            if (m <= 1000) return m; //linear below 1 kHz
            return (Math.Pow(10.0, m / 2595.0) - 1.0) * 700.0;
        }

        public static double HerzTranform(double f, double C, double div )
        {
            return C * Math.Log10(1.0 + f / div);
        }

        public static double InverseHerzTranform(double m, double C, double div)
        {
            return (Math.Pow(10.0, m / C) - 1.0) * div;
        }


        public static double[,] CreateLinearFilterBank(int filterBankCount, int FFTbins, double Nyquist)
        {
            double hzGap     = Nyquist / FFTbins;
            double filterGap = Nyquist / (double)(filterBankCount);  //gap between filter centres
            //Console.WriteLine(" Nyquist=" + Nyquist + " filterGap=" + filterGap);

            double[] filterCentres = new double[filterBankCount + 2]; //+2 for outside edges
            for (int i = 1; i <= filterBankCount + 1; i++) filterCentres[i] = (i * filterGap);
            //DataTools.writeArray(filterCentres);

            double[] filterBases = new double[filterBankCount + 2]; //excludes outside edges
            for (int i = 1; i <= filterBankCount; i++) filterBases[i] = filterCentres[i + 1] - filterCentres[i - 1];
            //DataTools.writeArray(filterBases);

            double[] filterHeights = new double[filterBankCount + 2]; //excludes outside edges which have zero height
            for (int i = 1; i <= filterBankCount; i++) filterHeights[i] = 2 / filterBases[i];
            //DataTools.writeArray(filterHeights);

            double[,] filters = new double[filterBankCount, FFTbins];
            for (int i = 1; i < filterBankCount; i++)
            {
                int lowerIndex = (int)Math.Truncate(filterCentres[i - 1] / hzGap);
                int centreIndex = (int)Math.Round(filterCentres[i] / hzGap);
                int upperIndex = (int)Math.Ceiling(filterCentres[i + 1] / hzGap);
                //set up ascending side of triangle
                int halfBase = centreIndex - lowerIndex;
                for (int j = lowerIndex; j < centreIndex; j++)
                {
                    filters[i, j] = filterHeights[i] * (j - lowerIndex) / (double)halfBase;
                    //Console.WriteLine(i + "  " + j + "  " + filters[i, j]);
                }
                //set up decending side of triangle
                halfBase = upperIndex - centreIndex;
                for (int j = centreIndex; j < upperIndex; j++)
                {
                    filters[i, j] = filterHeights[i] * (upperIndex - j) / (double)halfBase;
                    //Console.WriteLine(i + "  " + j + "  " + filters[i, j]);
                }

            }//end over all filters
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\filterBank.txt";
            //FileTools.WriteMatrix2File_Formatted(filters, fPath, "F3");

            return filters;
        }

        /// <summary>
        /// uses the Matlab algorithm. Slow, inefficient method that creates very sparce matrices.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="filterBankCount"></param>
        /// <param name="Nyquist"></param>
        /// <returns></returns>
        public static double[,] LinearFilterBank(double[,] matrix, int filterBankCount, double Nyquist)
        {
            //Console.WriteLine(" LinearFilterBank(double[,] matrix, int filterBankCount, double Nyquist) -- uses the Matlab algorithm");
            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of Hz bands = 2^N +1
            int FFTbins = N - 1;
            
            //create arrays containing the filter centre and bases etc.
            double hzGap = Nyquist / FFTbins;
            double filterGap = Nyquist / (double)(filterBankCount);  //gap between filter centres
            //Console.WriteLine(" Nyquist=" + Nyquist + " filterGap=" + filterGap);

            double[] filterCentres = new double[filterBankCount + 2]; //+2 for outside edges
            for (int i = 1; i <= filterBankCount + 1; i++) filterCentres[i] = (i * filterGap);
            //DataTools.writeArray(filterCentres);

            double[] filterBases = new double[filterBankCount + 2]; //excludes outside edges
            for (int i = 1; i <= filterBankCount; i++) filterBases[i] = filterCentres[i + 1] - filterCentres[i - 1];
            //DataTools.writeArray(filterBases);

            double[] filterHeights = new double[filterBankCount + 2]; //excludes outside edges which have zero height
            for (int i = 1; i <= filterBankCount; i++) filterHeights[i] = 2 / filterBases[i];
            //DataTools.writeArray(filterHeights);


            double[,] outData = new double[M, filterBankCount];
            //Console.WriteLine("T=" + M + "  filterBankCount=" + filterBankCount);
            for (int i = 0; i < M; i++) //for all spectra or time steps
            {
                for (int j = 0; j < filterBankCount; j++) //for all mel bands
                {
                    //set up ascending side of triangle
                    int lowerIndex = (int)Math.Truncate(filterCentres[i - 1] / hzGap);
                    int centreIndex = (int)Math.Round(filterCentres[i] / hzGap);
                    int halfBase = centreIndex - lowerIndex;
                    //set up descending side of triangle
                    int upperIndex = (int)Math.Ceiling(filterCentres[i + 1] / hzGap);
                    double sum = 0.0;
                    
                    
                    for (int f = lowerIndex; f < centreIndex; f++)
                    {
                        sum += (filterHeights[j] * (f - lowerIndex) / (double)halfBase);
                        //Console.WriteLine(j + "  " + f + "  " + filters[j, f]);
                    }
                    //set up decending side of triangle
                    halfBase = upperIndex - centreIndex;
                    for (int f = centreIndex; f < upperIndex; f++)
                    {
                        sum += (filterHeights[j] * (upperIndex - f) / (double)halfBase);
                        //Console.WriteLine(j + "  " + f + "  " + filters[j, f]);
                    }

                    outData[i, j] = sum; //
                } //end of for all freq bands
            }//end of for all spectra or time steps

            return outData;
        }


        /// <summary>
        /// Does linear filterbank conversion for sonogram for any frequency band given by minFreq and maxFreq.
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist</param>
        /// <param name="Nyquist">max frequency in original spectra</param>
        /// <param name="minFreq">min freq in passed sonogram</param>
        /// <param name="maxFreq">max freq in passed sonogram</param>
        /// <returns></returns>
        public static double[,] LinearConversion(double[,] matrix, int filterBankCount, double Nyquist, int minFreq, int maxFreq)
        {

            double freqRange = maxFreq - minFreq;
            double fraction = freqRange / Nyquist;
            filterBankCount = (int)Math.Ceiling(filterBankCount * fraction);

            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of bins in freq band
            double[,] outData = new double[M, filterBankCount];
            double linBand = freqRange / N;
            double opBand  = freqRange / (double)filterBankCount;  //width of output band
            Console.WriteLine(" NCount=" + N + " filterCount=" + filterBankCount + " freqRange=" + freqRange.ToString("F1") + " linBand=" + linBand.ToString("F1") + " opBand=" + opBand.ToString("F1"));

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < filterBankCount; j++) //for all mel bands in the frequency range
                {
                    // find top and bottom freq bin id's corresponding to the mel interval
                    double freqA = (j * opBand) + minFreq;
                    double freqB = ((j + 1) * opBand) + minFreq;
                    double a = (freqA - minFreq) / linBand;   //location of lower f in Hz bin units
                    double b = (freqB - minFreq) / linBand;   //location of upper f in Hz bin units
                    int ai = (int)Math.Ceiling(a);
                    int bi = (int)Math.Floor(b);
                    if (i < 2) Console.WriteLine("i="+i+" j="+j+": a=" + a.ToString("F1") + " b=" + b.ToString("F1") + " ai=" + ai + " bi=" + bi);
                    double sum = 0.0;

                    if (bi < ai) //a and b are in same Hz band
                    {
                        ai = (int)Math.Floor(a);
                        bi = (int)Math.Ceiling(b);
                        double ya = Speech.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], a);
                        double yb = Speech.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], b);
                        sum = Speech.LinearIntegral(a * linBand, b * linBand, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = Speech.LinearInterpolate((double)(ai - 1), (double)ai, matrix[i, ai - 1], matrix[i, ai], a);
                            //sum += Speech.LinearIntegral(a, (double)ai, ya, matrix[i, ai]);
                            sum += Speech.LinearIntegral(a * linBand, ai * linBand, ya, matrix[i, ai]);
                            //sum += Speech.MelIntegral(a * linBand, ai * linBand, ya, matrix[i, ai]);
                        }
                        for (int k = ai; k < bi; k++)
                        {
                            if ((k + 1) >= filterBankCount) break;//to prevent out of range index with Koala recording
                            //sum += Speech.MelIntegral(k * linBand, (k + 1) * linBand, matrix[i, k], matrix[i, k + 1]);
                            sum += Speech.LinearIntegral(k * linBand, (k + 1) * linBand, matrix[i, k], matrix[i, k + 1]);
                            //sum += Speech.LinearIntegral(k, (k + 1), matrix[i, k], matrix[i, k + 1]);
                        }
                        if (bi < (N - 1))
                        {
                            double yb = Speech.LinearInterpolate((double)bi, (double)(bi + 1), matrix[i, bi], matrix[i, bi + 1], b);
                            //sum += Speech.MelIntegral(bi * linBand, b * linBand, matrix[i, bi], yb);
                            sum += Speech.LinearIntegral(bi * linBand, b * linBand, matrix[i, bi], yb);
                            //sum += Speech.LinearIntegral((double)bi, b, this.Matrix[i, bi], yb);
                        }
                    }


                    double width = freqB - freqA;
                    outData[i, j] = sum / width; //to obtain power per mel
                    //outData[i, j] = sum / opBand; //to obtain power per mel
                    if (outData[i, j] < 0.0001) outData[i, j] = 0.0001;
                    //if (i < 2) Console.WriteLine("i=" + i + " j=" + j + ": sum=" + sum.ToString("F1"));
                } //end of for all mel bands
            //implicit end of for all spectra or time steps

            return outData;
        }






        /// <summary>
        /// Does mel conversion for passed sonogram matrix.
        /// IMPORTANT !!!!! Assumes that min freq of sonogram = 0 Hz and maxFreq = Nyquist.
        /// Uses Greg's MelIntegral
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist</param>
        /// <param name="Nyquist">max frequency in original spectra</param>
        /// <returns></returns>
        public static double[,] MelConversion(double[,] matrix, int filterBankCount, double Nyquist)
        {
            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of Hz bands = 2^N +1
            double[,] outData = new double[M, filterBankCount];
            double linBand = Nyquist / N;
            double melBand = Speech.Mel(Nyquist) / (double)filterBankCount;  //width of mel band
            //Console.WriteLine(" linBand=" + linBand + " melBand=" + melBand);

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < filterBankCount; j++) //for all mel bands
                {
                    double a = Speech.InverseMel(j * melBand) / linBand;       //location of lower f in Hz bin units
                    double b = Speech.InverseMel((j + 1) * melBand) / linBand; //location of upper f in Hz bin units
                    int ai = (int)Math.Ceiling(a);
                    int bi = (int)Math.Floor(b);

                    double sum = 0.0;

                    if (bi < ai) //a and b are in same Hz band
                    {
                        ai = (int)Math.Floor(a);
                        bi = (int)Math.Ceiling(b);
                        double ya = Speech.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], a);
                        double yb = Speech.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], b);
                        //sum = Speech.LinearIntegral(a, b, ya, yb);
                        sum = Speech.MelIntegral(a * linBand, b * linBand, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = Speech.LinearInterpolate((double)(ai - 1), (double)ai, matrix[i, ai - 1], matrix[i, ai], a);
                            //sum += Speech.LinearIntegral(a, (double)ai, ya, this.Matrix[i, ai]);
                            sum += Speech.MelIntegral(a * linBand, ai * linBand, ya, matrix[i, ai]);
                        }
                        for (int k = ai; k < bi; k++)
                        {
                            if ((k + 1) >= filterBankCount) break;//to prevent out of range index with Koala recording
                            //Console.WriteLine(" i=" + i + " k+1=" + (k + 1));
                            sum += Speech.MelIntegral(k * linBand, (k + 1) * linBand, matrix[i, k], matrix[i, k + 1]);
                            //sum += Speech.LinearIntegral(k, (k + 1), this.Matrix[i, k], this.Matrix[i, k + 1]);
                        }
                        if (bi < (N - 1)) //this.Bands in Greg's original code
                        {
                            double yb = Speech.LinearInterpolate((double)bi, (double)(bi + 1), matrix[i, bi], matrix[i, bi + 1], b);
                            sum += Speech.MelIntegral(bi * linBand, b * linBand, matrix[i, bi], yb);
                            //sum += Speech.LinearIntegral((double)bi, b, this.Matrix[i, bi], yb);
                        }
                    }
                  
                    outData[i, j] = sum / melBand; //to obtain power per mel
                } //end of for all mel bands
            //implicit end of for all spectra or time steps

            return outData;
        }


        /// <summary>
        /// Does mel conversion for sonogram for any frequency band given by minFreq and maxFreq.
        /// Uses Greg's MelIntegral
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist</param>
        /// <param name="Nyquist">max frequency in original spectra</param>
        /// <param name="minFreq">min freq in passed sonogram</param>
        /// <param name="maxFreq">max freq in passed sonogram</param>
        /// <returns></returns>
        public static double[,] MelConversion(double[,] matrix, int filterBankCount, double Nyquist, int minFreq, int maxFreq)
        {

            double freqRange  = maxFreq - minFreq;
            double melNyquist = Speech.Mel(Nyquist);
            double minMel     = Speech.Mel(minFreq);
            double maxMel     = Speech.Mel(maxFreq);
            double melRange   = maxMel - minMel;
            double fraction   = melRange / melNyquist; 
            filterBankCount   = (int)Math.Ceiling(filterBankCount * fraction);

            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of bins in freq band
            double[,] outData = new double[M, filterBankCount];
            double linBand = freqRange / N;
            double melBand = melRange / (double)filterBankCount;  //width of mel band
            //Console.WriteLine(" N     Count=" + N + " freqRange=" + freqRange.ToString("F1") + " linBand=" + linBand.ToString("F1"));
            //Console.WriteLine(" filterCount=" + filterBankCount + " melRange=" + melRange.ToString("F1") + " melBand=" + melBand.ToString("F1"));

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < filterBankCount; j++) //for all mel bands in the frequency range
                {
                    // find top and bottom freq bin id's corresponding to the mel interval
                    double melA = (j * melBand) + minMel;
                    double melB = ((j + 1) * melBand) + minMel;
                    double a = (Speech.InverseMel(melA) - minFreq) / linBand;   //location of lower f in Hz bin units
                    double b = (Speech.InverseMel(melB) - minFreq) / linBand;   //location of upper f in Hz bin units
                    int ai = (int)Math.Ceiling(a);
                    int bi = (int)Math.Floor(b);
                    //if (i < 2) Console.WriteLine("i="+i+" j="+j+": a=" + a.ToString("F1") + " b=" + b.ToString("F1") + " ai=" + ai + " bi=" + bi);
                    double sum = 0.0;

                    if (bi < ai) //a and b are in same Hz band
                    {
                        ai = (int)Math.Floor(a);
                        bi = (int)Math.Ceiling(b);
                        double ya = Speech.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], a);
                        double yb = Speech.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], b);
                        //sum = Speech.LinearIntegral(a, b, ya, yb);
                        sum = Speech.MelIntegral(a * linBand, b * linBand, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = Speech.LinearInterpolate((double)(ai - 1), (double)ai, matrix[i, ai - 1], matrix[i, ai], a);
                            //sum += Speech.LinearIntegral(a, (double)ai, ya, this.Matrix[i, ai]);
                            sum += Speech.MelIntegral(a * linBand, ai * linBand, ya, matrix[i, ai]);
                        }
                        for (int k = ai; k < bi; k++)
                        {
                            //if ((k + 1) >= filterBankCount) break;//to prevent out of range index with Koala recording
                            //Console.WriteLine(" i=" + i + " k+1=" + (k + 1));
                            sum += Speech.MelIntegral(k * linBand, (k + 1) * linBand, matrix[i, k], matrix[i, k + 1]);
                            //sum += Speech.LinearIntegral(k, (k + 1), this.Matrix[i, k], this.Matrix[i, k + 1]);
                        }
                        if (bi < (N - 1)) //this.Bands in Greg's original code
                        {
                            double yb = Speech.LinearInterpolate((double)bi, (double)(bi + 1), matrix[i, bi], matrix[i, bi + 1], b);
                            sum += Speech.MelIntegral(bi * linBand, b * linBand, matrix[i, bi], yb);
                            //sum += Speech.LinearIntegral((double)bi, b, this.Matrix[i, bi], yb);
                        }
                    }


                    //double melAi = Speech.Mel(ai + minFreq);
                    //double melBi = Speech.Mel(bi + minFreq);
                    //double width = melBi - melAi;
                    outData[i, j] = sum / melBand; //to obtain power per mel
                } //end of for all mel bands
            //implicit end of for all spectra or time steps

            return outData;
        }


        public static double[,] MelFilterbank(double[,] matrix, int filterBankCount, double Nyquist)
        {
            Console.WriteLine(" MelFilterbank(double[,] matrix, int filterBankCount, double Nyquist) -- uses the Matlab algorithm");
            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of Hz bands = 2^N +1
            int FFTbins = N - 1;
            double[,] filterBank = CreateMelFilterBank(filterBankCount, FFTbins, Nyquist);
            //string fPath = @"C:\SensorNetworks\Sonograms\filterbank.bmp";
            //ImageTools.DrawMatrix(filterBank, fPath);


            double[,] outData = new double[M, filterBankCount];

            for (int i = 0; i < M; i++) //for all spectra or time steps
            {
                double sum = 0.0;
                for (int j = 0; j < filterBankCount; j++) //for all mel bands
                {
                    for (int f = 0; f < filterBankCount; f++) sum += (filterBank[j,f] * matrix[i,f]);
                    outData[i, j] = sum; //
                } //end of for all mel bands
            }//end of for all spectra or time steps

            return outData;
        }

        public static double[,] CreateMelFilterBank(int filterBankCount, int FFTbins, double Nyquist)
        {
            double hzGap = Nyquist / FFTbins;
            double melGap = Speech.Mel(Nyquist) / (double)(filterBankCount);  //mel gap between filter centres
            //Console.WriteLine(" melNyquist=" + Speech.Mel(Nyquist) + " melGap=" + melGap);

            double[] filterCentres = new double[filterBankCount + 2]; //+2 for outside edges
            for (int i = 1; i <= filterBankCount+1; i++) filterCentres[i] = Speech.InverseMel(i * melGap);
            //DataTools.writeArray(filterCentres);

            double[] filterBases   = new double[filterBankCount + 2]; //excludes outside edges
            for (int i = 1; i <= filterBankCount; i++) filterBases[i] = filterCentres[i+1] - filterCentres[i-1];
            //DataTools.writeArray(filterBases);

            double[] filterHeights = new double[filterBankCount + 2]; //excludes outside edges which have zero height
            for (int i = 1; i <= filterBankCount; i++) filterHeights[i] = 2 / filterBases[i];
            //DataTools.writeArray(filterHeights);

            double[,] filters = new double[filterBankCount, FFTbins];
            for (int i = 1; i < filterBankCount; i++)
            {
                int lowerIndex  = (int)Math.Truncate(filterCentres[i - 1] / hzGap);
                int centreIndex = (int)Math.Round(filterCentres[i] / hzGap);
                int upperIndex  = (int)Math.Ceiling(filterCentres[i + 1] / hzGap);
                //set up ascending side of triangle
                int halfBase = centreIndex - lowerIndex;
                for (int j = lowerIndex; j < centreIndex; j++)
                {
                    filters[i, j] = filterHeights[i] * (j - lowerIndex) / (double)halfBase;
                    //Console.WriteLine(i + "  " + j + "  " + filters[i, j]);
                }
                //set up decending side of triangle
                halfBase = upperIndex - centreIndex;
                for (int j = centreIndex; j < upperIndex; j++)
                {
                    filters[i, j] = filterHeights[i] * (upperIndex - j) / (double)halfBase;
                    //Console.WriteLine(i + "  " + j + "  " + filters[i, j]);
                }

            }//end over all filters
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\filterBank.txt";
            //FileTools.WriteMatrix2File_Formatted(filters, fPath, "F3");

            return filters;
        }


        //********************************************************************************************************************
        //********************************************************************************************************************
        //********************************************************************************************************************
        //******************************* CEPTRA COEFFICIENTS USING DCT AND COSINES

        public static double[,] Cepstra(double[,] spectra, int coeffCount)
        {
            int frameCount = spectra.GetLength(0);  //number of frames
            int binCount = spectra.GetLength(1);    // number of filters in filter bank

            double[,] M = spectra;

            double[,] cosines = Cosines(binCount, coeffCount + 1); //set up the cosine coefficients

            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");
            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);


            double[,] OP = new double[frameCount, coeffCount];
            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                double[] spectrum = DataTools.GetRow(M, i); //transfer matrix row to vector
                double[] cepstrum = DCT(spectrum, cosines);

                for (int j = 0; j < coeffCount; j++) OP[i, j] = cepstrum[j+1]; //skip first DC value
            } //end of all frames
            return OP;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectrumLength">Same as bin count or filter bank count ie length of spectrum = N</param>
        /// <param name="coeffCount"></param>
        /// <returns></returns>
        public static double[,] Cosines(int spectrumLength, int coeffCount)
        {
            double[,] cosines = new double[coeffCount + 1, spectrumLength]; //get an extra coefficient because do not want DC coeff
            for (int k = 0; k < coeffCount + 1; k++)//foreach coeff
            {
                double kPiOnM = k * Math.PI / spectrumLength;
                for (int m = 0; m < spectrumLength; m++) // spectral bin
                {
                    cosines[k, m] = Math.Cos(kPiOnM * (m + 0.5)); //can also be Cos(kPiOnM * (m - 0.5)
                }
            }
            return cosines;
        }

        public static double[] DCT(double[] spectrum, double[,] cosines)
        {
            int L = spectrum.Length;
            int coeffCount = cosines.GetLength(0);

            double k0factor = 1 / Math.Sqrt(L);
            double kLfactor = Math.Sqrt(2/(double)L);
            double[] cepstrum = new double[coeffCount];
            for (int k = 0; k < coeffCount; k++)//foreach coeff
            {
                double factor = kLfactor;
                if (k == 0) factor = k0factor;
                double sum = 0.0;
                for (int m = 0; m < L; m++) // over all spectral bins
                {
                    sum += (spectrum[m] * cosines[k,m]);
                }
                cepstrum[k] = factor*sum;
            }
            return cepstrum;
        }

    }//end of class Speech
}
