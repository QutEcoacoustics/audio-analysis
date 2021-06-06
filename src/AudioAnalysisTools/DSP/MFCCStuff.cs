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
        /// Converts amplitude spectra (in a spectrogram) to dB spectra, normalising for window power and sample rate.
        /// NOTE 1: This calculation is done in three separate steps in order to avoid duplicating the tricky
        ///         calculations in the method GetLogEnergySpectrogram().
        /// NOTE 2: The decibels value is a ratio. Here the ratio is implied.
        ///         dB = 10*log(amplitude ^2) but in this method adjust power to account for power of Hamming window and SR.
        /// </summary>
        /// <param name="amplitudeM"> the amplitude spectra. </param>
        /// <param name="windowPower">value for window power normalisation.</param>
        /// <param name="sampleRate">to NormaliseMatrixValues for the sampling rate.</param>
        /// <param name="epsilon">small value to avoid log of zero.</param>
        /// <returns>a spectrogram of decibel values.</returns>
        public static double[,] DecibelSpectra(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon)
        {
            //conver amplitude values to energy
            double[,] energyM = MatrixTools.SquareValues(amplitudeM);

            // take log of power values and multiply by 10 to convert to decibels.
            double[,] decibelM = GetLogEnergySpectrogram(energyM, windowPower, sampleRate, epsilon * epsilon);
            decibelM = MatrixTools.MultiplyMatrixByFactor(decibelM, 10);
            return decibelM;
        }

        /// <summary>
        /// This method converts the passed matrix of spectrogram energy values, (i.e. squared amplitude values) to log-energy values.
        /// This method is used when calculating standard, mel-freq and mfcc spectrograms.
        /// In the case of mel-scale, the passed energy spectrogram is output from the mel-frequency filter bank,
        /// and the energy values are converted directly to log-energy, normalising for window power and sample rate.
        /// Note that the output is log-energy, not decibels: decibels =  10 * log-energy
        /// NOTE 1: THIS METHOD ASSUMES THAT THE LAST FREQ BIN (ie the last matrix column) IS THE NYQUIST FREQ BIN
        /// NOTE 2: THIS METHOD ASSUMES THAT THE FIRST FREQ BIN (ie the first matrix column) IS THE MEAN or DC FREQ BIN.
        /// NOTE 3: The window contributes power to the signal which must subsequently be removed from the spectral power.
        /// NOTE 4: Spectral power must be normalised for sample rate. Effectively calculate freq power per sample.
        /// NOTE 5: The power in all freq bins except f=0 must be doubled because the power spectrum is an even function about f=0;
        ///         This is due to the fact that the spectrum actually consists of 512 + 1 values, the centre value being for f=0.
        /// </summary>
        /// <param name="energyM"> the amplitude spectra. </param>
        /// <param name="windowPower">value for window power normalisation.</param>
        /// <param name="sampleRate">to NormaliseMatrixValues for the sampling rate.</param>
        /// <param name="epsilon">small value to avoid log of zero.</param>
        /// <returns>a spectrogram of decibel values.</returns>
        public static double[,] GetLogEnergySpectrogram(double[,] energyM, double windowPower, int sampleRate, double epsilon)
        {
            int frameCount = energyM.GetLength(0);
            int binCount = energyM.GetLength(1);
            double minLogEnergy = Math.Log10(epsilon / windowPower / sampleRate);
            double minLogEnergy2 = Math.Log10(epsilon * 2 / windowPower / sampleRate);

            double[,] decibelM = new double[frameCount, binCount];

            //calculate power of the DC value - first column of matrix
            for (int i = 0; i < frameCount; i++)
            {
                if (energyM[i, 0] < epsilon)
                {
                    decibelM[i, 0] = minLogEnergy;
                }
                else
                {
                    decibelM[i, 0] = Math.Log10(energyM[i, 0] / windowPower / sampleRate);
                }
            }

            // calculate power in frequency bins - must multiply by 2 to accomodate two spectral components, ie positive and neg freq.
            for (int j = 1; j < binCount - 1; j++)
            {
                // foreach time step or frame
                for (int i = 0; i < frameCount; i++)
                {
                    if (energyM[i, j] < epsilon)
                    {
                        decibelM[i, j] = minLogEnergy2;
                    }
                    else
                    {
                        decibelM[i, j] = Math.Log10(energyM[i, j] * 2 / windowPower / sampleRate);
                    }
                }
            } //end of all freq bins

            //calculate power of the Nyquist freq bin - last column of matrix
            for (int i = 0; i < frameCount; i++)
            {
                //calculate power of the DC value
                if (energyM[i, binCount - 1] < epsilon)
                {
                    decibelM[i, binCount - 1] = minLogEnergy;
                }
                else
                {
                    decibelM[i, binCount - 1] = Math.Log10(energyM[i, binCount - 1] / windowPower / sampleRate);
                }
            }

            return decibelM;
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
        /// Does conversion from linear frequency scale to mel-scale for any frequency band given by minFreq and maxFreq.
        /// Uses Greg's MelIntegral
        /// The first step is to calculate the number of filters for the required frequency sub-band.
        /// </summary>
        /// <param name="matrix">the spectrogram.</param>
        /// <param name="filterBankCount">number of filters over full freq range 0 Hz - Nyquist.</param>
        /// <param name="nyquist">max frequency in original spectra.</param>
        /// <param name="minFreq">min freq in the passed sonogram matrix.</param>
        /// <param name="maxFreq">max freq in the passed sonogram matrix.</param>
        public static double[,] MelFilterBank(double[,] matrix, int filterBankCount, double nyquist, int minFreq, int maxFreq)
        {
            double freqRange = maxFreq - minFreq;
            if (freqRange <= 0)
            {
                throw new Exception("FATAL ERROR: Speech.LinearFilterBank(): Freq range = zero. Check values of min & max freq.");
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
        //******************************* CALCULATION OF CEPTRAL COEFFICIENTS USING DCT AND COSINES

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

        /*
        private static int[,] Zigzag12X12 =
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
        */

        //********************************************************************************************************************
        //********************************************************************************************************************
        //********************************************************************************************************************
        //*********************************************** GET ACOUSTIC VECTORS

        /// <summary>
        /// This method assumes that the supplied mfcc matrix DOES NOT contain frame dB (log energy) values in column zero.
        /// These are added in from the supplied array of frame log-energies.
        /// </summary>
        /// <param name="mfcc">A matrix of mfcc coefficients. Column zero is empty.</param>
        /// <param name="frameDbNormed">log-energy values for the frames.</param>
        /// <param name="includeDelta">Whether or not to add delta features.</param>
        /// <param name="includeDoubleDelta">Whether or not to add double delta features.</param>
        /// <returns>A matrix of complete mfcc values with additional deltas, frame energies etc.</returns>
        public static double[,] AcousticVectors(double[,] mfcc, double[] frameDbNormed, bool includeDelta, bool includeDoubleDelta)
        {
            //both the matrix of mfcc's and the array of decibels have been normed in 0-1.
            int frameCount = mfcc.GetLength(0); //number of time frames
            int mfccCount = mfcc.GetLength(1);  //number of MFCC coefficients
            int coeffcount = mfccCount + 1;     //number of MFCCs + 1 for energy
            int dim = coeffcount;
            if (includeDelta)
            {
                dim += coeffcount;
            }

            if (includeDoubleDelta)
            {
                dim += coeffcount;
            }

            // create matrix to take the required set of features, mfccs, deltas and double deltas.
            double[,] acousticM = new double[frameCount, dim];

            // loop through the time frames and create feature vector for each frame.
            for (int t = 0; t < frameCount; t++)
            {
                double[] fv = GetMfccFeatureVector(frameDbNormed, mfcc, t, includeDelta, includeDoubleDelta); //get feature vector for frame (t)

                //transfer feature vector to the matrix of acoustic features.
                for (int i = 0; i < dim; i++)
                {
                    acousticM[t, i] = fv[i];
                }
            }

            return acousticM;
        }

        /// <summary>
        /// Constructs a feature vector of MFCCs including deltas and double deltas as requested by user.
        /// The dB array has been normalised in 0-1.
        /// </summary>
        /// <param name="dB">log-energy values for the frames.</param>
        /// <param name="matrix">A matrix of mfcc coefficients. Column zero is empty.</param>
        /// <param name="timeId">index for the required timeframe.</param>
        /// <param name="includeDelta">Whether or not to add delta features.</param>
        /// <param name="includeDoubleDelta">Whether or not to add double-delta features.</param>
        /// <returns>a mfcc feature vector for a single time-frame.</returns>
        public static double[] GetMfccFeatureVector(double[] dB, double[,] matrix, int timeId, bool includeDelta, bool includeDoubleDelta)
        {
            int mfccCount = matrix.GetLength(1);  //number of MFCCs
            int coeffcount = mfccCount + 1;       //number of MFCCs + 1 for frame energy
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

            // add in the log-energy value for the time-frame.
            fv[0] = dB[timeId];

            //add in the CEPSTRAL coefficients
            for (int i = 0; i < mfccCount; i++)
            {
                fv[1 + i] = matrix[timeId, i];
            }

            //add in the DELTA coefficients
            int offset = coeffcount;
            if (includeDelta)
            {
                // First deal with edge effects
                if (timeId <= 0)
                {
                    for (int i = offset; i < dim; i++)
                    {
                        fv[i] = 0.5;
                    }

                    return fv;
                }

                // add in DELTA of the log-energy value for the time-frame.
                fv[offset] = dB[timeId] - dB[timeId - 1];

                // add in DELTAs of the cepstral coefficients.
                for (int i = 0; i < mfccCount; i++)
                {
                    fv[1 + offset + i] = matrix[timeId, i] - matrix[timeId - 1, i];
                }

                //Normalise Values that potentially range from -1 to +1
                for (int i = offset; i < offset + mfccCount + 1; i++)
                {
                    fv[i] = (fv[i] + 1) / 2;
                }
            }

            //add in the DOUBLE DELTA coefficients
            if (includeDoubleDelta)
            {
                //deal with edge effects
                offset += coeffcount;
                if (timeId <= 1)
                {
                    for (int i = offset; i < dim; i++)
                    {
                        fv[i] = 0.5;
                    }

                    return fv;
                }

                // add in DELTA-DELTAs of the log-energy value for the time-frame.
                fv[offset] = (dB[timeId] - dB[timeId - 1]) - (dB[timeId - 1] - dB[timeId - 2]);

                // add in DELTA-DELTAs of the cepstral coefficients.
                for (int i = 0; i < mfccCount; i++)
                {
                    fv[1 + offset + i] = matrix[timeId, i] - matrix[timeId - 1, i] - (matrix[timeId - 1, i] - matrix[timeId - 2, i]);
                }

                //Normalise Matrix Values values that potentially range from -2 to +2
                for (int i = offset; i < offset + mfccCount + 1; i++)
                {
                    fv[i] = (fv[i] + 2) / 4;
                }
            }

            return fv;
        }
    }
}