// <copyright file="MFCCStuff.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using TowseyLibrary;

    public class MFCCStuff
    {
        /// <summary>
        /// Converts spectral amplitudes directly to dB, normalising for window power and sample rate.
        /// NOTE 1: The window contributes power to the signal which must subsequently be removed from the spectral power.
        /// NOTE 2: Spectral power must be normalised for sample rate. Effectively calculate freq power per sample.
        /// NOTE 3: The power in all freq bins except f=0 must be doubled because the power spectrum is an even function about f=0;
        ///         This is due to the fact that the spectrum actually consists of 512 + 1 values, the centre value being for f=0.
        /// NOTE 4: The decibels value is a ratio. Here the ratio is implied.
        ///         dB = 10*log(amplitude ^2) but in this method adjust power to account for power of Hamming window and SR.
        /// NOTE 5: THIS METHOD ASSUMES THAT THE LAST BIN IS THE NYQUIST FREQ BIN
        ///  NOTE 6: THIS METHOD ASSUMES THAT THE FIRST BIN IS THE MEAN or DC FREQ BIN.
        /// </summary>
        /// <param name="amplitudeM"> the amplitude spectra. </param>
        /// <param name="windowPower">value for window power normalisation.</param>
        /// <param name="sampleRate">to NormaliseMatrixValues for the sampling rate.</param>
        /// <param name="epsilon">small value to avoid log of zero.</param>
        /// <returns>a spectrogram of decibel values.</returns>
        public static double[,] DecibelSpectra(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon)
        {
            int frameCount = amplitudeM.GetLength(0);
            int binCount = amplitudeM.GetLength(1);
            double minDb = 10 * Math.Log10(epsilon * epsilon / windowPower / sampleRate);
            double min2Db = 10 * Math.Log10(epsilon * epsilon * 2 / windowPower / sampleRate);

            double[,] spectra = new double[frameCount, binCount];

            //calculate power of the DC value - first column of matrix
            for (int i = 0; i < frameCount; i++)
            {
                if (amplitudeM[i, 0] < epsilon)
                {
                    spectra[i, 0] = minDb;
                }
                else
                {
                    spectra[i, 0] = 10 * Math.Log10(amplitudeM[i, 0] * amplitudeM[i, 0] / windowPower / sampleRate);
                }
            }

            // calculate power in frequency bins - must multiply by 2 to accomodate two spectral components, ie positive and neg freq.
            for (int j = 1; j < binCount - 1; j++)
            {
                // foreach time step or frame
                for (int i = 0; i < frameCount; i++)
                {
                    if (amplitudeM[i, j] < epsilon)
                    {
                        spectra[i, j] = min2Db;
                    }
                    else
                    {
                        spectra[i, j] = 10 * Math.Log10(amplitudeM[i, j] * amplitudeM[i, j] * 2 / windowPower / sampleRate);
                    }
                }
            } //end of all freq bins

            //calculate power of the Nyquist freq bin - last column of matrix
            for (int i = 0; i < frameCount; i++)
            {
                //calculate power of the DC value
                if (amplitudeM[i, binCount - 1] < epsilon)
                {
                    spectra[i, binCount - 1] = minDb;
                }
                else
                {
                    spectra[i, binCount - 1] = 10 * Math.Log10(amplitudeM[i, binCount - 1] * amplitudeM[i, binCount - 1] / windowPower / sampleRate);
                }
            }

            return spectra;
        }

        public static int[] VocalizationDetection(double[] decibels, double lowerDbThreshold, double upperDbThreshold, int k1k2delay, int syllableGap, int minPulse, int[] zeroCrossings)
        {
            int length = decibels.Length;
            int[] state = new int[length];
            int lowEnergyId = 0;
            int hiEnergyId = -k1k2delay; // to prevent setting early frames to state=2
            for (int i = 0; i < length; i++)
            {
                if (decibels[i] < lowerDbThreshold)
                {
                    lowEnergyId = i;
                    int delay = i - hiEnergyId;
                    if (delay < k1k2delay)
                    {
                        for (int j = 1; j < delay; j++)
                        {
                            state[i - j] = 2;
                        }
                    }

                    state[i] = 0;
                }

                if (decibels[i] > upperDbThreshold)
                {
                    hiEnergyId = i;
                    int delay = i - lowEnergyId;
                    if (delay < k1k2delay)
                    {
                        for (int j = 1; j < delay; j++)
                        {
                            state[i - j] = 2;
                        }
                    }

                    state[i] = 2;
                }
            } //end  foreach time step

            // fill in probable inter-syllable gaps
            bool sig = true;
            int count = syllableGap; //do not want silence before first vocal frame to be treated as gap
            for (int i = 1; i < decibels.Length; i++)
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
                    if (sig == false && count < syllableGap)
                    {
                        for (int j = 1; j <= count; j++)
                        {
                            state[i - j] = 1; //fill gap with state = 1;
                        }
                    }

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
            double ratio = (x2 - x0) / dX;
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
            double dF = f0 - f1; // if reverse this, image intensity is reversed
            double x = dF / (q + f1);
            double x1 = Math.Log(x + 1.0);
            if (Math.Abs(x1 - x) > 1.0e-10)
            {
                return p * (y1 - y0 + ((y0 - (y1 * (x + 1.0))) * (x1 / x)));
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Returns a Mel value for the passed Herz value
        /// NOTE: According to Wikipedia there is no single objective mel(ody) scale conversion.
        /// Mel scale is based on just-noticeable difference in pitch by the ear with ascend pitch. I.E> THis is psycho-acoustic phenomenon.
        /// 1000Hz is used as the common reference point i.e. 1000Hz = 1000Mel.
        /// In speech processing, typically use a linear conversion below 1000Hz.
        /// </summary>
        public static double Mel(double f)
        {
            if (f <= 1000)
            {
                return f; //linear below 1 kHz
            }

            return 2595.0 * Math.Log10(1.0 + (f / 700.0));
        }

        /// <summary>
        /// Converts a Mel value to Herz.
        /// NOTE: By default this Mel scale is linear to 1000 Hz.
        /// </summary>
        /// <returns>the Herz value.</returns>
        public static double InverseMel(double mel)
        {
            if (mel <= 1000)
            {
                return mel; //linear below 1 kHz
            }

            return (Math.Pow(10.0, mel / 2595.0) - 1.0) * 700.0;
        }

        /// <summary>
        /// this method calculates a user customised version of the fixed mel frequency convernsion in
        /// the method Mel(double f).
        /// </summary>
        /// <param name="f">this is the linear frequncy in Herz.</param>
        /// <param name="c">this value = 2595.0 in the standard Mel transform.</param>
        /// <param name="div">this value = 700 in the standard Mel transform.</param>
        /// <returns>Mel frequency.</returns>
        public static double HerzTranform(double f, double c, double div)
        {
            return c * Math.Log10(1.0 + (f / div));
        }

        public static double InverseHerzTranform(double m, double c, double div)
        {
            return (Math.Pow(10.0, m / c) - 1.0) * div;
        }

        /// <summary>
        /// Does linear filterbank conversion for sonogram for any frequency band given by minFreq and maxFreq.
        /// Performs linear integral as opposed to Mel integral
        /// The first step is to calculate the number of filters for the required frequency sub-band.
        /// </summary>
        /// <param name="matrix">the sonogram.</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist.</param>
        /// <param name="nyquist">max frequency in original spectra.</param>
        /// <param name="minFreq">min freq in passed sonogram matrix.</param>
        /// <param name="maxFreq">max freq in passed sonogram matrix.</param>
        public static double[,] LinearFilterBank(double[,] matrix, int filterBankCount, double nyquist, int minFreq, int maxFreq)
        {
            int freqRange = maxFreq - minFreq;
            if (freqRange <= 0)
            {
                Log.WriteLine("Speech.LinearFilterBank(): WARNING!!!! Freq range = zero");
                throw new Exception("Speech.LinearFilterBank(): WARNING!!!! Freq range = zero. Check values of min & max freq.");
            }

            double fraction = freqRange / nyquist;
            filterBankCount = (int)Math.Ceiling(filterBankCount * fraction);

            int rowCount = matrix.GetLength(0); //number of spectra or time steps
            int colCount = matrix.GetLength(1); //number of bins in freq band
            double[,] outData = new double[rowCount, filterBankCount];
            double ipBand = freqRange / (double)colCount;                //width of input freq band
            double opBand = freqRange / (double)filterBankCount;  //width of output freq band

            //for all spectra or time steps
            for (int i = 0; i < rowCount; i++)
            {
                //for all output bands in the frequency range
                for (int j = 0; j < filterBankCount; j++)
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
                        double ya = LinearInterpolate(ipAint - 1, ipAint, matrix[i, ipAint - 1], matrix[i, ipAint], ipA);
                        sum += LinearIntegral(ipA * ipBand, ipAint * ipBand, ya, matrix[i, ipAint]);
                    }

                    for (int k = ipAint; k < ipBint; k++)
                    {
                        if (k + 1 >= colCount)
                        {
                            break;  //to prevent out of range index
                        }

                        sum += LinearIntegral(k * ipBand, (k + 1) * ipBand, matrix[i, k], matrix[i, k + 1]);
                    }

                    if (ipBint < colCount)
                    {
                        double yb = LinearInterpolate(ipBint, ipBint + 1, matrix[i, ipBint], matrix[i, ipBint + 1], ipB);
                        sum += LinearIntegral(ipBint * ipBand, ipB * ipBand, matrix[i, ipBint], yb);
                    }

                    double width = ipB - ipA;
                    outData[i, j] = sum / width; //to obtain power per Hz
                    if (outData[i, j] < 0.0001)
                    {
                        outData[i, j] = 0.0001;
                    }
                } //end of for all freq bands
            }

            //implicit end of for all spectra or time steps
            return outData;
        }

        /// <summary>
        /// Returns an [N, 2] matrix with bin ID in column 1 and lower Herz bound in column 2 but on Mel scale.
        /// </summary>
        public static int[,] GetMelBinBounds(int nyquist, int melBinCount)
        {
            double maxMel = (int)MFCCStuff.Mel(nyquist);
            double melPerBin = maxMel / melBinCount;

            var binBounds = new int[melBinCount, 2];

            for (int i = 0; i < melBinCount; i++)
            {
                binBounds[i, 0] = i;
                double mel = i * melPerBin;
                binBounds[i, 1] = (int)MFCCStuff.InverseMel(mel);
            }

            return binBounds;
        }

        /// <summary>
        /// Does MelFilterBank for passed sonogram matrix.
        /// IMPORTANT !!!!! Assumes that min freq of passed sonogram matrix = 0 Hz and maxFreq = Nyquist.
        /// Uses Greg's MelIntegral.
        /// </summary>
        /// <param name="matrix">the sonogram.</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist.</param>
        /// <param name="nyquist">max frequency in original spectra.</param>
        public static double[,] MelFilterBank(double[,] matrix, int filterBankCount, double nyquist)
        {
            int rowCount = matrix.GetLength(0); //number of spectra or time steps
            int colCount = matrix.GetLength(1); //number of Hz bands = 2^N +1

            double[,] outData = new double[rowCount, filterBankCount];
            double linBinWidth = nyquist / colCount;
            double melBinWidth = Mel(nyquist) / filterBankCount;  //width of single mel bin

            //for all spectra or frames
            for (int i = 0; i < rowCount; i++)
            {
                //for all mel bands
                for (int j = 0; j < filterBankCount; j++)
                {
                    double fa = InverseMel(j * melBinWidth) / linBinWidth;       //lower f in Hz units
                    double fb = InverseMel((j + 1) * melBinWidth) / linBinWidth; //upper f in Hz units
                    int ai = (int)Math.Ceiling(fa);
                    int bi = (int)Math.Floor(fb);

                    double sum = 0.0;

                    if (bi < ai)
                    {
                        //a and b are in same Hz band
                        ai = (int)Math.Floor(fa);
                        bi = (int)Math.Ceiling(fb);
                        double ya = LinearInterpolate(ai, bi, matrix[i, ai], matrix[i, bi], fa);
                        double yb = LinearInterpolate(ai, bi, matrix[i, ai], matrix[i, bi], fb);
                        sum = MelIntegral(fa * linBinWidth, fb * linBinWidth, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = LinearInterpolate(ai - 1, ai, matrix[i, ai - 1], matrix[i, ai], fa);
                            sum += MelIntegral(fa * linBinWidth, ai * linBinWidth, ya, matrix[i, ai]);
                        }

                        for (int k = ai; k < bi; k++)
                        {
                            if (k + 1 >= colCount)
                            {
                                break; //to prevent out of range index
                            }

                            sum += MelIntegral(k * linBinWidth, (k + 1) * linBinWidth, matrix[i, k], matrix[i, k + 1]);
                        }

                        if (bi < colCount)
                        {
                            double yb = LinearInterpolate(bi, bi + 1, matrix[i, bi], matrix[i, bi + 1], fb);
                            sum += MelIntegral(bi * linBinWidth, fb * linBinWidth, matrix[i, bi], yb);
                        }
                    }

                    outData[i, j] = sum / melBinWidth; //to obtain power per mel
                } //end of mel bins
            }

            return outData;
        }

        /// <summary>
        /// Does mel conversion for sonogram for any frequency band given by minFreq and maxFreq.
        /// Uses Greg's MelIntegral
        /// The first step is to calculate the number of filters for the required frequency sub-band.
        /// </summary>
        /// <param name="matrix">the sonogram.</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist.</param>
        /// <param name="nyquist">max frequency in original spectra.</param>
        /// <param name="minFreq">min freq in the passed sonogram matrix.</param>
        /// <param name="maxFreq">max freq in the passed sonogram matrix.</param>
        public static double[,] MelFilterBank(double[,] matrix, int filterBankCount, double nyquist, int minFreq, int maxFreq)
        {
            double freqRange = maxFreq - minFreq;
            if (freqRange <= 0)
            {
                Log.WriteLine("Speech.MelFilterBank(): WARNING!!!! Freq range = zero");
                throw new Exception("Speech.LinearFilterBank(): WARNING!!!! Freq range = zero. Check values of min & max freq.");
            }

            double melNyquist = Mel(nyquist);
            double minMel = Mel(minFreq);
            double maxMel = Mel(maxFreq);
            double melRange = maxMel - minMel;
            double fraction = melRange / melNyquist;
            filterBankCount = (int)Math.Ceiling(filterBankCount * fraction);

            int rowCount = matrix.GetLength(0); //number of spectra or time steps
            int colCount = matrix.GetLength(1); //number of bins in freq band
            double[,] outData = new double[rowCount, filterBankCount];
            double linBand = freqRange / (colCount - 1); //(N-1) because have additional DC band
            double melBand = melRange / filterBankCount;  //width of mel band

            //LoggedConsole.WriteLine(" N     Count=" + N + " freqRange=" + freqRange.ToString("F1") + " linBand=" + linBand.ToString("F3"));
            //LoggedConsole.WriteLine(" filterCount=" + filterBankCount + " melRange=" + melRange.ToString("F1") + " melBand=" + melBand.ToString("F3"));
            //for all spectra or time steps
            for (int i = 0; i < rowCount; i++)
            {
                //for all mel bands in the frequency range
                for (int j = 0; j < filterBankCount; j++)
                {
                    // find top and bottom freq bin id's corresponding to the mel interval
                    double melA = (j * melBand) + minMel;
                    double melB = ((j + 1) * melBand) + minMel;
                    double ipA = (InverseMel(melA) - minFreq) / linBand;   //location of lower f in Hz bin units
                    double ipB = (InverseMel(melB) - minFreq) / linBand;   //location of upper f in Hz bin units
                    int ai = (int)Math.Ceiling(ipA);
                    int bi = (int)Math.Floor(ipB);

                    //if (i < 2) LoggedConsole.WriteLine("i="+i+" j="+j+": a=" + a.ToString("F1") + " b=" + b.ToString("F1") + " ai=" + ai + " bi=" + bi);
                    double sum = 0.0;

                    if (bi < ai)
                    {
                        //a and b are in same Hz band
                        ai = (int)Math.Floor(ipA);
                        bi = (int)Math.Ceiling(ipB);
                        double ya = LinearInterpolate(ai, bi, matrix[i, ai], matrix[i, bi], ipA);
                        double yb = LinearInterpolate(ai, bi, matrix[i, ai], matrix[i, bi], ipB);
                        sum = MelIntegral(ipA * linBand, ipB * linBand, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = LinearInterpolate(ai - 1, ai, matrix[i, ai - 1], matrix[i, ai], ipA);
                            sum += MelIntegral(ipA * linBand, ai * linBand, ya, matrix[i, ai]);
                        }

                        for (int k = ai; k < bi; k++)
                        {
                            //if ((k + 1) >= N) LoggedConsole.WriteLine("k=" + k + "  N=" + N);
                            if (k + 1 > colCount)
                            {
                                break; //to prevent out of range index
                            }

                            sum += MelIntegral(k * linBand, (k + 1) * linBand, matrix[i, k], matrix[i, k + 1]);
                        }

                        if (bi < colCount - 1)
                        {
                            double yb = LinearInterpolate(bi, bi + 1, matrix[i, bi], matrix[i, bi + 1], ipB);
                            sum += MelIntegral(bi * linBand, ipB * linBand, matrix[i, bi], yb);
                        }
                    }

                    //double melAi = Speech.Mel(ai + minFreq);
                    //double melBi = Speech.Mel(bi + minFreq);
                    //double width = melBi - melAi;
                    outData[i, j] = sum / melBand; //to obtain power per mel
                } //end of for all mel bands
            }

            //implicit end of for all spectra or time steps
            return outData;
        }

        //********************************************************************************************************************
        //********************************************************************************************************************
        //********************************************************************************************************************
        //******************************* CEPTRA COEFFICIENTS USING DCT AND COSINES

        public static double[,] Cepstra(double[,] spectra, int coeffCount)
        {
            int frameCount = spectra.GetLength(0);  //number of frames
            int binCount = spectra.GetLength(1);  //number of filters in filter bank

            //set up the cosine coefficients. Need one extra to compensate for DC coeff.
            double[,] cosines = Cosines(binCount, coeffCount + 1);

            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);

            double[,] op = new double[frameCount, coeffCount];
            for (int i = 0; i < frameCount; i++)
            {
                double[] spectrum = DataTools.GetRow(spectra, i); //transfer matrix row=i to vector
                double[] cepstrum = DCT(spectrum, cosines);

                for (int j = 0; j < coeffCount; j++)
                {
                    op[i, j] = cepstrum[j + 1]; //+1 in order to skip first DC value
                }
            } //end of all frames

            return op;
        }

        /// <summary>
        /// use this version when want to make matrix of Cosines only one time.
        /// </summary>
        public static double[,] Cepstra(double[,] spectra, int coeffCount, double[,] cosines)
        {
            int frameCount = spectra.GetLength(0);  //number of frames
            double[,] op = new double[frameCount, coeffCount];
            for (int i = 0; i < frameCount; i++)
            {
                double[] spectrum = DataTools.GetRow(spectra, i); //transfer matrix row=i to vector
                double[] cepstrum = DCT(spectrum, cosines);

                for (int j = 0; j < coeffCount; j++)
                {
                    op[i, j] = cepstrum[j + 1]; //+1 in order to skip first DC value
                }
            } //end of all frames

            return op;
        }

        //public static double[,] DCT_2D(double[,] spectra, int coeffCount)
        //{
        //    double[,] op = Cepstra(spectra, coeffCount);
        //    return op;
        //}

        /// <summary>
        /// cosines.
        /// </summary>
        /// <param name="spectrumLength">Same as bin count or filter bank count ie length of spectrum = N.</param>
        /// <param name="coeffCount">count of coefficients.</param>
        public static double[,] Cosines(int spectrumLength, int coeffCount)
        {
            double[,] cosines = new double[coeffCount + 1, spectrumLength]; //get an extra coefficient because do not want DC coeff
            for (int k = 0; k < coeffCount + 1; k++)
            {
                double kPiOnM = k * Math.PI / spectrumLength;

                // for each spectral bin
                for (int m = 0; m < spectrumLength; m++)
                {
                    cosines[k, m] = Math.Cos(kPiOnM * (m + 0.5)); //can also be Cos(kPiOnM * (m - 0.5)
                }
            }

            return cosines;
        }

        public static double[] DCT(double[] spectrum, double[,] cosines)
        {
            int length = spectrum.Length;
            int coeffCount = cosines.GetLength(0);

            double k0Factor = 1 / Math.Sqrt(length);
            double kLFactor = Math.Sqrt(2 / (double)length);
            double[] cepstrum = new double[coeffCount];

            //foreach coeff
            for (int k = 0; k < coeffCount; k++)
            {
                double factor = kLFactor;
                if (k == 0)
                {
                    factor = k0Factor;
                }

                double sum = 0.0;

                // over all spectral bins
                for (int m = 0; m < length; m++)
                {
                    sum += spectrum[m] * cosines[k, m];
                }

                cepstrum[k] = factor * sum;
            }

            return cepstrum;
        }

        public static int[,] Zigzag12X12 =
        {
        {
            1,  2,  6,  7, 15, 16, 28, 29, 45, 46, 66, 67,
        },
        {
            3,  5,  8, 14, 17, 27, 30, 44, 47, 65, 68, 89,
        },
        {
            4,  9, 13, 18, 26, 31, 43, 48, 64, 69, 88, 90,
        },
        {
            10, 12, 19, 25, 32, 42, 49, 63, 70, 87, 91, 108,
        },
        {
            11, 20, 24, 33, 41, 50, 62, 71, 86, 92, 107, 109,
        },
        {
            21, 23, 34, 40, 51, 61, 72, 85, 93, 106, 110, 123,
        },
        {
            22, 35, 39, 52, 60, 73, 84, 94, 105, 111, 122, 124,
        },
        {
            36, 38, 53, 59, 74, 83, 95, 104, 112, 121, 125, 134,
        },
        {
            37, 54, 58, 75, 82, 96, 103, 113, 120, 126, 133, 135,
        },
        {
            55, 57, 76, 81, 97, 102, 114, 119, 127, 132, 136, 141,
        },
        {
            56, 77, 80, 98, 101, 115, 118, 128, 131, 137, 140, 142,
        },
        {
            78, 79, 99, 100, 116, 117, 129, 130, 138, 139, 143, 144,
        },
        };

        //********************************************************************************************************************
        //********************************************************************************************************************
        //********************************************************************************************************************
        //*********************************************** GET ACOUSTIC VECTORS

        /// <summary>
        /// This method assumes that the supplied mfcc matrix DOES NOT contain dB values in column one.
        /// These are added in from the supplied dB array.
        /// </summary>
        public static double[,] AcousticVectors(double[,] mfcc, double[] dBNormed, bool includeDelta, bool includeDoubleDelta)
        {
            //both the matrix of mfcc's and the array of decibels have been normed in 0-1.
            int frameCount = mfcc.GetLength(0); //number of frames
            int mfccCount = mfcc.GetLength(1); //number of MFCCs
            int coeffcount = mfccCount + 1; //number of MFCCs + 1 for energy
            int dim = coeffcount;
            if (includeDelta)
            {
                dim += coeffcount;
            }

            if (includeDoubleDelta)
            {
                dim += coeffcount;
            }

            double[,] acousticM = new double[frameCount, dim];
            for (int t = 0; t < frameCount; t++)
            {
                double[] fv = GetFeatureVector(dBNormed, mfcc, t, includeDelta, includeDoubleDelta); //get feature vector for frame (t)
                for (int i = 0; i < dim; i++)
                {
                    acousticM[t, i] = fv[i];  //transfer feature vector to acoustic matrix.
                }
            }

            return acousticM;
        }

        public static double[] AcousticVector(int index, double[,] mfcc, double[] dB, bool includeDelta, bool includeDoubleDelta)
        {
            //both the matrix of mfcc's and the array of decibels have been normed in 0-1.
            int mfccCount = mfcc.GetLength(1); //number of MFCCs
            int coeffcount = mfccCount + 1; //number of MFCCs + 1 for energy
            int dim = coeffcount;
            if (includeDelta)
            {
                dim += coeffcount;
            }

            if (includeDoubleDelta)
            {
                dim += coeffcount;
            }

            //LoggedConsole.WriteLine(" mfccCount=" + mfccCount + " coeffcount=" + coeffcount + " dim=" + dim);

            double[] acousticV = new double[dim];
            double[] fv = GetFeatureVector(dB, mfcc, index, includeDelta, includeDoubleDelta); //get feature vector for frame (t)
            for (int i = 0; i < dim; i++)
            {
                acousticV[i] = fv[i];  //transfer feature vector to acoustic Vector.
            }

            return acousticV;
        } //AcousticVectors()

        /// <summary>
        /// returns full feature vector from the passed matrix of energy+cepstral+delta+deltaDelta coefficients.
        /// </summary>
        public static double[] GetTriAcousticVector(double[,] cepstralM, int timeId, int deltaT)
        {
            int coeffcount = cepstralM.GetLength(1); //number of MFCC deltas etcs
            int featureCount = coeffcount;
            if (deltaT > 0)
            {
                featureCount *= 3;
            }

            //LoggedConsole.WriteLine("frameCount=" + frameCount + " coeffcount=" + coeffcount + " featureCount=" + featureCount + " deltaT=" + deltaT);
            double[] fv = new double[featureCount];

            if (deltaT == 0)
            {
                for (int i = 0; i < coeffcount; i++)
                {
                    fv[i] = cepstralM[timeId, i];
                }

                return fv;
            }

            //else extract tri-acoustic vector
            for (int i = 0; i < coeffcount; i++)
            {
                fv[i] = cepstralM[timeId - deltaT, i];
            }

            for (int i = 0; i < coeffcount; i++)
            {
                fv[coeffcount + i] = cepstralM[timeId, i];
            }

            for (int i = 0; i < coeffcount; i++)
            {
                fv[coeffcount + coeffcount + i] = cepstralM[timeId + deltaT, i];
            }

            return fv;
        }

        public static double[] GetFeatureVector(double[,] matrix, int timeId, bool includeDelta, bool includeDoubleDelta)
        {
            int frameCount = matrix.GetLength(0); //number of frames
            int coeffcount = matrix.GetLength(1); //number of MFCCs + 1 for energy
            int dim = coeffcount;
            if (includeDelta)
            {
                dim += coeffcount;
            }

            if (includeDoubleDelta)
            {
                dim += coeffcount;
            }

            double[] fv = new double[dim];

            //add in the CEPSTRAL coefficients
            for (int i = 0; i < coeffcount; i++)
            {
                fv[i] = matrix[timeId, i];
            }

            //add in the DELTA coefficients
            int offset = coeffcount;
            if (includeDelta)
            {
                //deal with edge effects
                if (timeId + 1 >= frameCount || timeId - 1 < 0)
                {
                    for (int i = offset; i < dim; i++)
                    {
                        fv[i] = 0.5;
                    }

                    return fv;
                }

                for (int i = 0; i < coeffcount; i++)
                {
                    fv[offset + i] = matrix[timeId + 1, i] - matrix[timeId - 1, i];
                }

                for (int i = offset; i < offset + coeffcount; i++)
                {
                    fv[i] = (fv[i] + 1) / 2;   //NormaliseMatrixValues values that potentially range from -1 to +1

                    //if (fv[i] < 0.0) fv[i] = 0.0;
                    //if (fv[i] > 1.0) fv[i] = 1.0;
                }
            }

            //add in the DOUBLE DELTA coefficients
            if (includeDoubleDelta)
            {
                offset += coeffcount;

                //deal with edge effects
                if (timeId + 2 >= frameCount || timeId - 2 < 0)
                {
                    for (int i = offset; i < dim; i++)
                    {
                        fv[i] = 0.5;
                    }

                    return fv;
                }

                for (int i = 0; i < coeffcount; i++)
                {
                    fv[offset + i] = matrix[timeId + 2, i] - matrix[timeId, i] - (matrix[timeId, i] - matrix[timeId - 2, i]);
                }

                for (int i = offset; i < offset + coeffcount; i++)
                {
                    //NormaliseMatrixValues values that potentially range from -2 to +2
                    fv[i] = (fv[i] + 2) / 4;

                    //if (fv[i] < 0.0) fv[i] = 0.0;
                    //if (fv[i] > 1.0) fv[i] = 1.0;
                }
            }

            return fv;
        }

        public static double[] GetFeatureVector(double[] dB, double[,] matrix, int timeId, bool includeDelta, bool includeDoubleDelta)
        {
            //the dB array has been normalised in 0-1.
            int frameCount = matrix.GetLength(0); //number of frames
            int mfccCount = matrix.GetLength(1);  //number of MFCCs
            int coeffcount = mfccCount + 1;  //number of MFCCs + 1 for energy
            int dim = coeffcount;
            if (includeDelta)
            {
                dim += coeffcount;
            }

            if (includeDoubleDelta)
            {
                dim += coeffcount;
            }

            double[] fv = new double[dim];

            //add in the CEPSTRAL coefficients
            fv[0] = dB[timeId];
            for (int i = 0; i < mfccCount; i++)
            {
                fv[1 + i] = matrix[timeId, i];
            }

            //add in the DELTA coefficients
            int offset = coeffcount;
            if (includeDelta)
            {
                if (timeId + 1 >= frameCount || timeId - 1 < 0) //deal with edge effects
                {
                    for (int i = offset; i < dim; i++)
                    {
                        fv[i] = 0.5;
                    }

                    return fv;
                }

                fv[offset] = dB[timeId + 1] - dB[timeId - 1];
                for (int i = 0; i < mfccCount; i++)
                {
                    fv[1 + offset + i] = matrix[timeId + 1, i] - matrix[timeId - 1, i];
                }

                for (int i = offset; i < offset + mfccCount + 1; i++)
                {
                    fv[i] = (fv[i] + 1) / 2;    //NormaliseMatrixValues values that potentially range from -1 to +1
                }
            }

            //add in the DOUBLE DELTA coefficients
            if (includeDoubleDelta)
            {
                //deal with edge effects
                offset += coeffcount;
                if (timeId + 2 >= frameCount || timeId - 2 < 0)
                {
                    for (int i = offset; i < dim; i++)
                    {
                        fv[i] = 0.5;
                    }

                    return fv;
                }

                fv[offset] = dB[timeId + 2] - dB[timeId] - (dB[timeId] - dB[timeId - 2]);
                for (int i = 0; i < mfccCount; i++)
                {
                    fv[1 + offset + i] = matrix[timeId + 2, i] - matrix[timeId, i] - (matrix[timeId, i] - matrix[timeId - 2, i]);
                }

                for (int i = offset; i < offset + mfccCount + 1; i++)
                {
                    fv[i] = (fv[i] + 2) / 4;   //NormaliseMatrixValues values that potentially range from -2 to +2

                    //if (fv[i] < 0.0) fv[i] = 0.0;
                    //if (fv[i] > 1.0) fv[i] = 1.0;
                }
            }

            return fv;
        }
    }
}