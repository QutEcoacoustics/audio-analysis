using System;
using System.Collections.Generic;
using System.Text;
using TowseyLibrary;

namespace AudioAnalysisTools.DSP
{
    public class MFCCStuff
    {

        public static void Main(string[] args)
        {
            int DCTLength = 32;
            int coeffCount = 32;

            double[,] cosines = MFCCStuff.Cosines(DCTLength, coeffCount + 1); //set up the cosine coefficients

            var array = new double[DCTLength];

            //linear spectrum
//            for (int i = 0; i < DCTLength; i++) if((i % 16)==0) array[i] = 10;
            //sinusoid spectrum
            int k = 4;
            double kPiOnM = k * 2 * Math.PI / DCTLength;
            for (int i = 0; i < DCTLength; i++) // spectral bin
            {
                array[i] = 10 * Math.Cos(kPiOnM * (i + 0.5)); //can also be Cos(kPiOnM * (m - 0.5)
            }



            //array = DataTools.SubtractMean(array);
            array = DataTools.normalise2UnitLength(array);
            //array = DataTools.normalise(array);
            DataTools.writeBarGraph(array);


            double[] dct = MFCCStuff.DCT(array, cosines);
            for (int i = 0; i < dct.Length; i++) dct[i] = Math.Abs(dct[i]*10);
            dct[0] = 0.0; //dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0;
            //int maxIndex = DataTools.GetMaxIndex(dct);
            //double max = dct[maxIndex]; 
            //DataTools.MinMax(dct, out min, out max);
            DataTools.writeBarGraph(dct);
            LoggedConsole.WriteLine("FINISHED");
            Console.ReadLine();
        }


        /// <summary>
        /// NOTE: the decibels value is a ratio. Here the ratio is implied.
        /// This method does not account for power of the window if one is used.
        /// </summary>
        /// <param name="amplitudeM"></param>
        /// <returns></returns>
        //public static double[,] DecibelSpectra(double[,] amplitudeM)
        //{
        //    int frameCount = amplitudeM.GetLength(0);
        //    int binCount   = amplitudeM.GetLength(1);

        //    double[,] spectra = new double[frameCount, binCount];

        //    for (int i = 0; i < frameCount; i++)//foreach time step
        //    {
        //        for (int j = 0; j < binCount; j++) //foreach freq bin
        //        {
        //            double amplitude = amplitudeM[i, j];
        //            //double power = amplitude * amplitude;
        //            double power = 20 * Math.Log10(amplitude); //convert amplitude to decibels dB = 10*log(amplitude ^2)
        //            spectra[i, j] = power;
        //        }
        //    } //end of all frames
        //    return spectra;
        //}


        /// <summary>
        /// Converts spectral amplitudes directly to dB, normalising for window power and sample rate.
        /// NOTE 1: The window contributes power to the signal which must subsequently be removed from the spectral power.
        /// NOTE 2: Spectral power must be normaliesd for sample rate. Effectively calculate freq power per sample.
        /// NOTE 3: The power in all freq bins except f=0 must be doubled because the power spectrum is an even function about f=0;
        ///         This is due to the fact that the spectrum actually consists of 512 + 1 values, the centre value being for f=0.
        /// NOTE 4: The decibels value is a ratio. Here the ratio is implied.
        ///         dB = 10*log(amplitude ^2) but in this method adjust power to account for power of Hamming window and SR.
        /// NOTE 5: THIS METHOD ASSUMES THAT THE LAST BIN IS THE NYQUIST FREQ BIN        
        /// </summary>
        /// <param name="amplitudeM">the amplitude spectra</param>
        /// <param name="windowPower">value for window power normalisation</param>
        /// <param name="sampleRate">to normalise for the sampling rate</param>
        /// <param name="epsilon">small value to avoid log of zero.</param>
        /// <returns></returns>
        public static double[,] DecibelSpectra(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon)
        {
            int frameCount = amplitudeM.GetLength(0);
            int binCount = amplitudeM.GetLength(1);
            double minDB  = 10 * Math.Log10(epsilon * epsilon     / windowPower / sampleRate);
            double min2DB = 10 * Math.Log10(epsilon * epsilon * 2 / windowPower / sampleRate);
            //double thresholdDB = -1;

            double[,] spectra = new double[frameCount, binCount];

            //calculate power of the DC value - first column of matrix
            for (int i = 0; i < frameCount; i++)//foreach time step or frame
            {
                if (amplitudeM[i, 0] < epsilon)
                    spectra[i, 0] = minDB;
                else
                    spectra[i, 0] = 10 * Math.Log10(amplitudeM[i, 0] * amplitudeM[i, 0] / windowPower / sampleRate);
                    //spectra[i, 0] = amplitudeM[i, 0] * amplitudeM[i, 0] / windowPower; //calculates power
                //if (spectra[i, 0] < thresholdDB) spectra[i, 0] = minDB;
            }


            //calculate power in frequency bins - must multiply by 2 to accomodate two spectral components, ie positive and neg freq.
            for (int j = 1; j < binCount-1; j++) 
            {                     
                for (int i = 0; i < frameCount; i++)//foreach time step or frame
                {
                    if (amplitudeM[i, j] < epsilon)
                        spectra[i, j] = min2DB; 
                    else
                        spectra[i, j] = 10 * Math.Log10(amplitudeM[i, j] * amplitudeM[i, j] *2 / windowPower / sampleRate);
                        //spectra[i, j] = amplitudeM[i, j] * amplitudeM[i, j] * 2 / windowPower; //calculates power
                    //if (spectra[i, 0] < thresholdDB) spectra[i, 0] = minDB;
                }//end of all frames
            } //end of all freq bins


            //calculate power of the Nyquist freq bin - last column of matrix
            for (int i = 0; i < frameCount; i++) //foreach time step or frame
            {
                //calculate power of the DC value
                if (amplitudeM[i, binCount - 1] < epsilon)
                    spectra[i, binCount - 1] = minDB;
                else
                    spectra[i, binCount - 1] = 10 * Math.Log10(amplitudeM[i, binCount - 1] * amplitudeM[i, binCount - 1] / windowPower / sampleRate);
                //spectra[i, 0] = amplitudeM[i, 0] * amplitudeM[i, 0] / windowPower; //calculates power
                //if (spectra[i, 0] < thresholdDB) spectra[i, 0] = minDB;
            }

            return spectra;
        }


        public static int[] VocalizationDetection(double[] decibels, double lowerDBThreshold, double upperDBThreshold, int k1_k2delay, int syllableGap, int minPulse, int[] zeroCrossings)
        {
            int L = decibels.Length;
            int[] state = new int[L];
            int lowEnergyID = 0;
            int hiEnergyID = -k1_k2delay; //to prevent setting early frames to state=2
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
            } //end  foreach time step

            // fill in probable inter-syllable gaps
            bool sig = true;
            int count = syllableGap; //do not want silence before first vocal frame to be treated as gap
            for (int i = 1; i < decibels.Length; i++) //foreach time step
            {
                if (state[i] == 0)
                {
                    sig = false;
                    count++;
                }
                else
                    if (state[i] == 2)
                    {
                        //LoggedConsole.WriteLine("count["+i+"]="+count);
                        if ((sig == false) && (count < syllableGap))
                            for (int j = 1; j <= count; j++) state[i - j] = 1;//fill gap with state = 1;
                        sig = true;
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

        /// <summary>
        /// returns a Mel value for the passed Herz value
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
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




        /// <summary>
        /// Does linear filterbank conversion for sonogram for any frequency band given by minFreq and maxFreq.
        /// Performs linear integral as opposed to Mel integral
        /// The first step is to calculate the number of filters for the required frequency sub-band.
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist</param>
        /// <param name="Nyquist">max frequency in original spectra</param>
        /// <param name="minFreq">min freq in passed sonogram matrix</param>
        /// <param name="maxFreq">max freq in passed sonogram matrix</param>
        /// <returns></returns>
        public static double[,] LinearFilterBank(double[,] matrix, int filterBankCount, double Nyquist, int minFreq, int maxFreq)
        {

            int freqRange = maxFreq - minFreq;
            if (freqRange <= 0)
            {
                Log.WriteLine("Speech.LinearFilterBank(): WARNING!!!! Freq range = zero");
                throw new Exception("Speech.LinearFilterBank(): WARNING!!!! Freq range = zero. Check values of min & max freq.");
            }

            double fraction = freqRange / Nyquist;
            filterBankCount = (int)Math.Ceiling(filterBankCount * fraction);

            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of bins in freq band
            double[,] outData = new double[M, filterBankCount];
            double ipBand = freqRange / (double)N;                //width of input freq band
            double opBand = freqRange / (double)filterBankCount;  //width of output freq band
            //LoggedConsole.WriteLine(" NCount=" + N + " filterCount=" + filterBankCount + " freqRange=" + freqRange + " ipBand=" + ipBand.ToString("F1") + " opBand=" + opBand.ToString("F1"));

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < filterBankCount; j++) //for all output bands in the frequency range
                {
                    // find top and bottom input bin id's corresponding to the output interval
                    double opA = (j * opBand) + minFreq;
                    double opB = ((j + 1) * opBand) + minFreq;
                    double ipA = (opA - minFreq) / ipBand;   //location of lower f in Hz bin units
                    double ipB = (opB - minFreq) / ipBand;   //location of upper f in Hz bin units
                    int ipAint = (int)Math.Ceiling(ipA);
                    int ipBint = (int)Math.Floor(ipB);
                    double sum = 0.0;
                    //if (i < 2) LoggedConsole.WriteLine("i=" + i + " j=" + j + ": ai=" + ipAint + " bi=" + ipBint + " b-a=" + (ipBint - ipAint));

                    if (ipAint > 0)
                    {
                        double ya = MFCCStuff.LinearInterpolate((double)(ipAint - 1), (double)ipAint, matrix[i, ipAint - 1], matrix[i, ipAint], ipA);
                        sum += MFCCStuff.LinearIntegral(ipA * ipBand, ipAint * ipBand, ya, matrix[i, ipAint]);
                    }

                    for (int k = ipAint; k < ipBint; k++)
                    {
                        if ((k + 1) >= N) break;  //to prevent out of range index
                        sum += MFCCStuff.LinearIntegral(k * ipBand, (k + 1) * ipBand, matrix[i, k], matrix[i, k + 1]);
                    }

                    if (ipBint < N)
                    {
                        double yb = MFCCStuff.LinearInterpolate((double)ipBint, (double)(ipBint + 1), matrix[i, ipBint], matrix[i, ipBint + 1], ipB);
                        sum += MFCCStuff.LinearIntegral(ipBint * ipBand, ipB * ipBand, matrix[i, ipBint], yb);
                    }

                    double width = ipB - ipA;
                    outData[i, j] = sum / width; //to obtain power per Hz
                    if (outData[i, j] < 0.0001) outData[i, j] = 0.0001;
                } //end of for all freq bands
            //implicit end of for all spectra or time steps

            return outData;
        }






        /// <summary>
        /// Does MelFilterBank for passed sonogram matrix.
        /// IMPORTANT !!!!! Assumes that min freq of passed sonogram matrix = 0 Hz and maxFreq = Nyquist.
        /// Uses Greg's MelIntegral
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist</param>
        /// <param name="Nyquist">max frequency in original spectra</param>
        /// <returns></returns>
        public static double[,] MelFilterBank(double[,] matrix, int filterBankCount, double Nyquist)
        {
            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of Hz bands = 2^N +1
            double[,] outData = new double[M, filterBankCount];
            double linBand = Nyquist / (double)N;
            double melBand = MFCCStuff.Mel(Nyquist) / (double)filterBankCount;  //width of mel band
            //LoggedConsole.WriteLine(" linBand=" + linBand + " melBand=" + melBand);

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < filterBankCount; j++) //for all mel bands
                {
                    double a = MFCCStuff.InverseMel(j * melBand) / linBand;       //location of lower f in Hz bin units
                    double b = MFCCStuff.InverseMel((j + 1) * melBand) / linBand; //location of upper f in Hz bin units
                    int ai = (int)Math.Ceiling(a);
                    int bi = (int)Math.Floor(b);

                    double sum = 0.0;

                    if (bi < ai) //a and b are in same Hz band
                    {
                        ai = (int)Math.Floor(a);
                        bi = (int)Math.Ceiling(b);
                        double ya = MFCCStuff.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], a);
                        double yb = MFCCStuff.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], b);
                        sum = MFCCStuff.MelIntegral(a * linBand, b * linBand, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = MFCCStuff.LinearInterpolate((double)(ai - 1), (double)ai, matrix[i, ai - 1], matrix[i, ai], a);
                            sum += MFCCStuff.MelIntegral(a * linBand, ai * linBand, ya, matrix[i, ai]);
                        }
                        for (int k = ai; k < bi; k++)
                        {
                            if ((k + 1) >= N) break;//to prevent out of range index with Koala recording
                            sum += MFCCStuff.MelIntegral(k * linBand, (k + 1) * linBand, matrix[i, k], matrix[i, k + 1]);
                        }
                        if (bi < N)
                        {
                            double yb = MFCCStuff.LinearInterpolate((double)bi, (double)(bi + 1), matrix[i, bi], matrix[i, bi + 1], b);
                            sum += MFCCStuff.MelIntegral(bi * linBand, b * linBand, matrix[i, bi], yb);
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
        /// The first step is to calculate the number of filters for the required frequency sub-band.
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist</param>
        /// <param name="Nyquist">max frequency in original spectra</param>
        /// <param name="minFreq">min freq in the passed sonogram matrix</param>
        /// <param name="maxFreq">max freq in the passed sonogram matrix</param>
        /// <returns></returns>
        public static double[,] MelFilterBank(double[,] matrix, int filterBankCount, double Nyquist, int minFreq, int maxFreq)
        {

            double freqRange  = maxFreq - minFreq;
            if (freqRange <= 0)
            {
                Log.WriteLine("Speech.MelFilterBank(): WARNING!!!! Freq range = zero");
                throw new Exception("Speech.LinearFilterBank(): WARNING!!!! Freq range = zero. Check values of min & max freq.");
            }
            
            double melNyquist = MFCCStuff.Mel(Nyquist);
            double minMel     = MFCCStuff.Mel(minFreq);
            double maxMel     = MFCCStuff.Mel(maxFreq);
            double melRange   = maxMel - minMel;
            double fraction   = melRange / melNyquist; 
            filterBankCount   = (int)Math.Ceiling(filterBankCount * fraction);

            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of bins in freq band
            double[,] outData = new double[M, filterBankCount];
            double linBand = freqRange / (N-1); //(N-1) because have additional DC band
            double melBand = melRange / (double)filterBankCount;  //width of mel band
            //LoggedConsole.WriteLine(" N     Count=" + N + " freqRange=" + freqRange.ToString("F1") + " linBand=" + linBand.ToString("F3"));
            //LoggedConsole.WriteLine(" filterCount=" + filterBankCount + " melRange=" + melRange.ToString("F1") + " melBand=" + melBand.ToString("F3"));

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < filterBankCount; j++) //for all mel bands in the frequency range
                {
                    // find top and bottom freq bin id's corresponding to the mel interval
                    double melA = (j * melBand) + minMel;
                    double melB = ((j + 1) * melBand) + minMel;
                    double ipA = (MFCCStuff.InverseMel(melA) - minFreq) / linBand;   //location of lower f in Hz bin units
                    double ipB = (MFCCStuff.InverseMel(melB) - minFreq) / linBand;   //location of upper f in Hz bin units
                    int ai = (int)Math.Ceiling(ipA);
                    int bi = (int)Math.Floor(ipB);
                    //if (i < 2) LoggedConsole.WriteLine("i="+i+" j="+j+": a=" + a.ToString("F1") + " b=" + b.ToString("F1") + " ai=" + ai + " bi=" + bi);
                    double sum = 0.0;

                    if (bi < ai) //a and b are in same Hz band
                    {
                        ai = (int)Math.Floor(ipA);
                        bi = (int)Math.Ceiling(ipB);
                        double ya = MFCCStuff.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], ipA);
                        double yb = MFCCStuff.LinearInterpolate((double)ai, bi, matrix[i, ai], matrix[i, bi], ipB);
                        sum = MFCCStuff.MelIntegral(ipA * linBand, ipB * linBand, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = MFCCStuff.LinearInterpolate((double)(ai - 1), (double)ai, matrix[i, ai - 1], matrix[i, ai], ipA);
                            sum += MFCCStuff.MelIntegral(ipA * linBand, ai * linBand, ya, matrix[i, ai]);
                        }
                        for (int k = ai; k < bi; k++)
                        {
                            //if ((k + 1) >= N) LoggedConsole.WriteLine("k=" + k + "  N=" + N);
                            if ((k + 1) > N) break;//to prevent out of range index
                            sum += MFCCStuff.MelIntegral(k * linBand, (k + 1) * linBand, matrix[i, k], matrix[i, k + 1]);
                        }
                        if (bi < (N-1))
                        {
                            double yb = MFCCStuff.LinearInterpolate((double)bi, (double)(bi + 1), matrix[i, bi], matrix[i, bi + 1], ipB);
                            sum += MFCCStuff.MelIntegral(bi * linBand, ipB * linBand, matrix[i, bi], yb);
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


        // Following two commented methods were an attempt to emulate the MATLAB code for performing the Mel Converison
        //In the end decided to stick with the INTEGRATION APPROACH.

        //public static double[,] MelFilterbank(double[,] matrix, int filterBankCount, double Nyquist)
        //{
        //    LoggedConsole.WriteLine(" MelFilterbank(double[,] matrix, int filterBankCount, double Nyquist) -- uses the Matlab algorithm");
        //    int M = matrix.GetLength(0); //number of spectra or time steps
        //    int N = matrix.GetLength(1); //number of Hz bands = 2^N +1
        //    int FFTbins = N - 1;
        //    double[,] filterBank = CreateMelFilterBank(filterBankCount, FFTbins, Nyquist);
        //    //string fPath = @"C:\SensorNetworks\Sonograms\filterbank.bmp";
        //    //ImageTools.DrawMatrix(filterBank, fPath);


        //    double[,] outData = new double[M, filterBankCount];

        //    for (int i = 0; i < M; i++) //for all spectra or time steps
        //    {
        //        double sum = 0.0;
        //        for (int j = 0; j < filterBankCount; j++) //for all mel bands
        //        {
        //            for (int f = 0; f < filterBankCount; f++) sum += (filterBank[j,f] * matrix[i,f]);
        //            outData[i, j] = sum; //
        //        } //end of for all mel bands
        //    }//end of for all spectra or time steps

        //    return outData;
        //}

        //public static double[,] CreateMelFilterBank(int filterBankCount, int FFTbins, double Nyquist)
        //{
        //    double hzGap = Nyquist / FFTbins;
        //    double melGap = Speech.Mel(Nyquist) / (double)(filterBankCount);  //mel gap between filter centres
        //    //LoggedConsole.WriteLine(" melNyquist=" + Speech.Mel(Nyquist) + " melGap=" + melGap);

        //    double[] filterCentres = new double[filterBankCount + 2]; //+2 for outside edges
        //    for (int i = 1; i <= filterBankCount+1; i++) filterCentres[i] = Speech.InverseMel(i * melGap);
        //    //DataTools.writeArray(filterCentres);

        //    double[] filterBases   = new double[filterBankCount + 2]; //excludes outside edges
        //    for (int i = 1; i <= filterBankCount; i++) filterBases[i] = filterCentres[i+1] - filterCentres[i-1];
        //    //DataTools.writeArray(filterBases);

        //    double[] filterHeights = new double[filterBankCount + 2]; //excludes outside edges which have zero height
        //    for (int i = 1; i <= filterBankCount; i++) filterHeights[i] = 2 / filterBases[i];
        //    //DataTools.writeArray(filterHeights);

        //    double[,] filters = new double[filterBankCount, FFTbins];
        //    for (int i = 1; i < filterBankCount; i++)
        //    {
        //        int lowerIndex  = (int)Math.Truncate(filterCentres[i - 1] / hzGap);
        //        int centreIndex = (int)Math.Round(filterCentres[i] / hzGap);
        //        int upperIndex  = (int)Math.Ceiling(filterCentres[i + 1] / hzGap);
        //        //set up ascending side of triangle
        //        int halfBase = centreIndex - lowerIndex;
        //        for (int j = lowerIndex; j < centreIndex; j++)
        //        {
        //            filters[i, j] = filterHeights[i] * (j - lowerIndex) / (double)halfBase;
        //            //LoggedConsole.WriteLine(i + "  " + j + "  " + filters[i, j]);
        //        }
        //        //set up decending side of triangle
        //        halfBase = upperIndex - centreIndex;
        //        for (int j = centreIndex; j < upperIndex; j++)
        //        {
        //            filters[i, j] = filterHeights[i] * (upperIndex - j) / (double)halfBase;
        //            //LoggedConsole.WriteLine(i + "  " + j + "  " + filters[i, j]);
        //        }

        //    }//end over all filters
        //    //following two lines write matrix of cos values for checking.
        //    //string fPath = @"C:\SensorNetworks\Sonograms\filterBank.txt";
        //    //FileTools.WriteMatrix2File_Formatted(filters, fPath, "F3");

        //    return filters;
        //}


        //********************************************************************************************************************
        //********************************************************************************************************************
        //********************************************************************************************************************
        //******************************* CEPTRA COEFFICIENTS USING DCT AND COSINES

        public static double[,] Cepstra(double[,] spectra, int coeffCount)
        {
            int frameCount = spectra.GetLength(0);  //number of frames
            int binCount   = spectra.GetLength(1);  //number of filters in filter bank

            //set up the cosine coefficients. Need one extra to compensate for DC coeff.
            double[,] cosines = MFCCStuff.Cosines(binCount, coeffCount + 1); 
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);

            double[,] OP = new double[frameCount, coeffCount];
            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                double[] spectrum = DataTools.GetRow(spectra, i); //transfer matrix row=i to vector
                double[] cepstrum = DCT(spectrum, cosines);

                for (int j = 0; j < coeffCount; j++) OP[i, j] = cepstrum[j+1]; //+1 in order to skip first DC value
            } //end of all frames
            return OP;
        }
        /// <summary>
        /// use this version when want to make matrix of Cosines only one time.
        /// </summary>
        /// <param name="spectra"></param>
        /// <param name="coeffCount"></param>
        /// <param name="cosines"></param>
        /// <returns></returns>
        public static double[,] Cepstra(double[,] spectra, int coeffCount, double[,] cosines)
        {
            int frameCount = spectra.GetLength(0);  //number of frames
            //int binCoun = spectra.GetLength(1);    // number of filters in filter bank
            double[,] OP = new double[frameCount, coeffCount];
            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                double[] spectrum = DataTools.GetRow(spectra, i); //transfer matrix row=i to vector
                double[] cepstrum = DCT(spectrum, cosines);

                for (int j = 0; j < coeffCount; j++) OP[i, j] = cepstrum[j + 1]; //+1 in order to skip first DC value
            } //end of all frames
            return OP;
        }




        public static double[,] DCT_2D(double[,] spectra, int coeffCount)
        {
            int frameCount = spectra.GetLength(0);  //number of frames
            int binCount = spectra.GetLength(1);    // number of filters in filter bank

            double[,] OP = Cepstra(spectra, coeffCount);
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


        public static int[,] zigzag12x12 = {
        {  1,  2,  6,  7, 15, 16, 28, 29, 45, 46, 66, 67},
        {  3,  5,  8, 14, 17, 27, 30, 44, 47, 65, 68, 89},
        {  4,  9, 13, 18, 26, 31, 43, 48, 64, 69, 88, 90},
        { 10, 12, 19, 25, 32, 42, 49, 63, 70, 87, 91,108},
        { 11, 20, 24, 33, 41, 50, 62, 71, 86, 92,107,109},
        { 21, 23, 34, 40, 51, 61, 72, 85, 93,106,110,123},
        { 22, 35, 39, 52, 60, 73, 84, 94,105,111,122,124},
        { 36, 38, 53, 59, 74, 83, 95,104,112,121,125,134},
        { 37, 54, 58, 75, 82, 96,103,113,120,126,133,135},
        { 55, 57, 76, 81, 97,102,114,119,127,132,136,141},
        { 56, 77, 80, 98,101,115,118,128,131,137,140,142},
        { 78, 79, 99,100,116,117,129,130,138,139,143,144},
        };



        //********************************************************************************************************************
        //********************************************************************************************************************
        //********************************************************************************************************************
        //*********************************************** GET ACOUSTIC VECTORS


        /// <summary>
        /// This method assumes that the supplied mfcc matrix contains dB values in column one.
        /// These are added in from the supplied dB array.
        /// </summary>
        /// <param name="mfcc"></param>
        /// <param name="includeDelta"></param>
        /// <param name="includeDoubleDelta"></param>
        /// <returns></returns>
        public static double[,] AcousticVectors(double[,] mfcc, bool includeDelta, bool includeDoubleDelta)
        {
            //both the matrix of mfcc's and the array of decibels have been normed in 0-1.
            int frameCount = mfcc.GetLength(0); //number of frames
            int coeffcount = mfcc.GetLength(1); //number of MFCCs + 1 for energy
            int dim = coeffcount; //
            if (includeDelta) dim += coeffcount;
            if (includeDoubleDelta) dim += coeffcount;

            double[,] acousticM = new double[frameCount, dim];
            for (int t = 0; t < frameCount; t++) //for all spectra or time steps
            {
                double[] fv = GetFeatureVector(mfcc, t, includeDelta, includeDoubleDelta);//get feature vector for frame (t)
                for (int i = 0; i < dim; i++) acousticM[t, i] = fv[i];  //transfer feature vector to acoustic matrix.
            }
            return acousticM;
        } //AcousticVectors()

        
        /// <summary>
        /// This method assumes that the supplied mfcc matrix DOES NOT contain dB values in column one.
        /// These are added in from the supplied dB array.
        /// </summary>
        /// <param name="mfcc"></param>
        /// <param name="dBNormed"></param>
        /// <param name="includeDelta"></param>
        /// <param name="includeDoubleDelta"></param>
        /// <returns></returns>
        public static double[,] AcousticVectors(double[,] mfcc, double[] dBNormed, bool includeDelta, bool includeDoubleDelta)
        {
            //both the matrix of mfcc's and the array of decibels have been normed in 0-1.
            int frameCount = mfcc.GetLength(0); //number of frames
            int mfccCount  = mfcc.GetLength(1); //number of MFCCs
            int coeffcount = mfccCount + 1; //number of MFCCs + 1 for energy
            int dim = coeffcount; //
            if (includeDelta)       dim += coeffcount;
            if (includeDoubleDelta) dim += coeffcount;
            //LoggedConsole.WriteLine(" mfccCount=" + mfccCount + " coeffcount=" + coeffcount + " dim=" + dim);

            double[,] acousticM = new double[frameCount, dim];
            for (int t = 0; t < frameCount; t++) //for all spectra or time steps
            {
                double[] fv = GetFeatureVector(dBNormed, mfcc, t, includeDelta, includeDoubleDelta);//get feature vector for frame (t)
                for (int i = 0; i < dim; i++) acousticM[t, i] = fv[i];  //transfer feature vector to acoustic matrix.
            }
            return acousticM;
        } //AcousticVectors()



        public static double[] AcousticVector(int index, double[,] mfcc, double[] dB, bool includeDelta, bool includeDoubleDelta)
        {
            //both the matrix of mfcc's and the array of decibels have been normed in 0-1.
            int mfccCount = mfcc.GetLength(1); //number of MFCCs
            int coeffcount = mfccCount + 1; //number of MFCCs + 1 for energy
            int dim = coeffcount; //
            if (includeDelta) dim += coeffcount;
            if (includeDoubleDelta) dim += coeffcount;
            //LoggedConsole.WriteLine(" mfccCount=" + mfccCount + " coeffcount=" + coeffcount + " dim=" + dim);

            double[] acousticV = new double[dim];
            double[] fv = GetFeatureVector(dB, mfcc, index, includeDelta, includeDoubleDelta);//get feature vector for frame (t)
            for (int i = 0; i < dim; i++) acousticV[i] = fv[i];  //transfer feature vector to acoustic Vector.
            return acousticV;
        } //AcousticVectors()



        /// <summary>
        /// returns full feature vector from the passed matrix of energy+cepstral+delta+deltaDelta coefficients
        /// </summary>
        /// <param name="cepstralM"></param>
        /// <param name="timeID"></param>
        /// <returns></returns>
        public static double[] GetTriAcousticVector(double[,] cepstralM, int timeID, int deltaT)
        {
            int frameCount = cepstralM.GetLength(0); //number of frames
            int coeffcount = cepstralM.GetLength(1); //number of MFCC deltas etcs
            int featureCount = coeffcount;
            if (deltaT > 0) featureCount *= 3;
            //LoggedConsole.WriteLine("frameCount=" + frameCount + " coeffcount=" + coeffcount + " featureCount=" + featureCount + " deltaT=" + deltaT);
            double[] fv = new double[featureCount];

            if (deltaT == 0)
            {
                for (int i = 0; i < coeffcount; i++) fv[i] = cepstralM[timeID, i];
                return fv;
            }
            //else extract tri-acoustic vector
            for (int i = 0; i < coeffcount; i++) fv[i] = cepstralM[timeID - deltaT, i];
            for (int i = 0; i < coeffcount; i++) fv[coeffcount+i] = cepstralM[timeID, i];
            for (int i = 0; i < coeffcount; i++) fv[coeffcount + coeffcount+ i] = cepstralM[timeID + deltaT, i];

            return fv;
        }


        public static double[] GetFeatureVector(double[,] M, int timeID, bool includeDelta, bool includeDoubleDelta)
        {
            int frameCount = M.GetLength(0); //number of frames
            int coeffcount = M.GetLength(1); //number of MFCCs + 1 for energy
            int dim = coeffcount; //
            if (includeDelta)       dim += coeffcount;
            if (includeDoubleDelta) dim += coeffcount;
            double[] fv = new double[dim];

            //add in the CEPSTRAL coefficients
            for (int i = 0; i < coeffcount; i++) fv[i] = M[timeID, i];

            //add in the DELTA coefficients
            int offset = coeffcount;
            if (includeDelta)
            {
                if (((timeID + 1) >= frameCount) || ((timeID - 1) < 0)) //deal with edge effects
                {
                    for (int i = offset; i < dim; i++) fv[i] = 0.5;
                    return fv;
                }
                for (int i = 0; i < coeffcount; i++) fv[offset + i] = M[timeID + 1, i] - M[timeID - 1, i];
                
                for (int i = offset; i < offset + coeffcount; i++)
                {
                    fv[i] = (fv[i] + 1) / 2;   //normalise values that potentially range from -1 to +1
                    //if (fv[i] < 0.0) fv[i] = 0.0;
                    //if (fv[i] > 1.0) fv[i] = 1.0;
                }
            }

            //add in the DOUBLE DELTA coefficients
            if (includeDoubleDelta)
            {
                offset += coeffcount;
                if (((timeID + 2) >= frameCount) || ((timeID - 2) < 0)) //deal with edge effects
                {
                    for (int i = offset; i < dim; i++) fv[i] = 0.5;
                    return fv;
                }
                for (int i = 0; i < coeffcount; i++)
                {
                    fv[offset + i] = (M[timeID + 2, i] - M[timeID, i]) - (M[timeID, i] - M[timeID - 2, i]);
                }
                for (int i = offset; i < offset + coeffcount; i++)
                {
                    fv[i] = (fv[i] + 2) / 4;   //normalise values that potentially range from -2 to +2
                    //if (fv[i] < 0.0) fv[i] = 0.0;
                    //if (fv[i] > 1.0) fv[i] = 1.0;
                }
            }

            return fv;
        }



        public static double[] GetFeatureVector(double[] dB, double[,] M, int timeID, bool includeDelta, bool includeDoubleDelta)
        {
            //the dB array has been normalised in 0-1.
            int frameCount = M.GetLength(0); //number of frames
            int mfccCount = M.GetLength(1);  //number of MFCCs
            int coeffcount = mfccCount + 1;  //number of MFCCs + 1 for energy
            int dim = coeffcount; //
            if (includeDelta) dim += coeffcount;
            if (includeDoubleDelta) dim += coeffcount;
            double[] fv = new double[dim];

            //add in the CEPSTRAL coefficients
            fv[0] = dB[timeID];
            for (int i = 0; i < mfccCount; i++) fv[1 + i] = M[timeID, i];

            //add in the DELTA coefficients
            int offset = coeffcount;
            if (includeDelta)
            {
                if (((timeID + 1) >= frameCount) || ((timeID - 1) < 0)) //deal with edge effects
                {
                    for (int i = offset; i < dim; i++) fv[i] = 0.5;
                    return fv;
                }
                fv[offset] = dB[timeID + 1] - dB[timeID - 1];
                for (int i = 0; i < mfccCount; i++)
                {
                    fv[1 + offset + i] = M[timeID + 1, i] - M[timeID - 1, i];
                }
                for (int i = offset; i < offset + mfccCount + 1; i++) 
                    fv[i] = (fv[i] + 1) / 2;    //normalise values that potentially range from -1 to +1
            }

            //add in the DOUBLE DELTA coefficients
            if (includeDoubleDelta)
            {
                offset += coeffcount;
                if (((timeID + 2) >= frameCount) || ((timeID - 2) < 0)) //deal with edge effects
                {
                    for (int i = offset; i < dim; i++) fv[i] = 0.5;
                    return fv;
                }
                fv[offset] = (dB[timeID + 2] - dB[timeID]) - (dB[timeID] - dB[timeID - 2]);
                for (int i = 0; i < mfccCount; i++)
                {
                    fv[1 + offset + i] = (M[timeID + 2, i] - M[timeID, i]) - (M[timeID, i] - M[timeID - 2, i]);
                }
                for (int i = offset; i < offset + mfccCount + 1; i++)
                {
                    fv[i] = (fv[i] + 2) / 4;   //normalise values that potentially range from -2 to +2
                    //if (fv[i] < 0.0) fv[i] = 0.0;
                    //if (fv[i] > 1.0) fv[i] = 1.0;
                }
            }

            return fv;
        }



    }//end of class Speech
}
