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


        public static double[,] MelScale(double[,] matrix, int melBandCount, double Nyquist)
        {
            int M = matrix.GetLength(0); //number of spectra or time steps
            int N = matrix.GetLength(1); //number of Hz bands
            double[,] outData = new double[M, melBandCount];
            double linBand = Nyquist / N;
            double melBand = Speech.Mel(Nyquist) / (double)melBandCount;  //width of mel band
            double min = double.PositiveInfinity; //to obtain mel min and max
            double max = double.NegativeInfinity;

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < melBandCount; j++) //for all mel bands
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
                    sum /= melBand; //to obtain power per mel

                    outData[i, j] = sum;
                    if (sum < min) min = sum;
                    if (sum > max) max = sum;
                }
            // min;  //could return min and max via out
            // max;
            return outData;
        }



        public static double[,] MFCCs(double[,] spectra, int filterBankCount, double Nyquist, int coeffCount)
        {
            int frameCount = spectra.GetLength(0);
            int binCount = spectra.GetLength(1);

            double[,] M = spectra;
            //M = MelScale(spectra, filterBankCount, Nyquist);
            M = DecibelSpectra(M);
            //M = ImageTools.NoiseReduction(M);

            FFT fft = new FFT(filterBankCount);
            double[,] cosines = Cosines(binCount, coeffCount + 1);

            double[,] OP = new double[frameCount, coeffCount];
            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                double[] spectrum = DataTools.GetRow(M, i); //transfer matrix row to vector
                double[] cepstrum = DCT(spectrum, cosines);
                //double[] cepstrum = fft.Invoke(spectrum);

                for (int j = 0; j < coeffCount; j++) OP[i, j] = cepstrum[j+1]; //skip first DC value
            } //end of all frames
            return OP;
        }

        public static double[,] Cosines(int spectrumLength, int coeffCount)
        {
            double[,] cosines = new double[spectrumLength, coeffCount + 1];
            for (int k = 0; k < coeffCount + 1; k++)//foreach coeff
            {
                double kPiOnM = k * Math.PI / spectrumLength;
                for (int m = 0; m < spectrumLength; m++) // spectral bin
                {
                    cosines[m, k] = Math.Cos(kPiOnM * (m - 0.5));
                }
            }
            return cosines;
        }

        public static double[] DCT(double[] spectrum, double[,] cosines)
        {
            int L = spectrum.Length;
            int coeffCount = cosines.GetLength(1);
            double[] cepstrum = new double[coeffCount+1];
            for (int k = 0; k < coeffCount; k++)//foreach coeff
            {
                double sum = 0.0;
                for (int m = 0; m < L; m++) // spectral bin
                {
                    sum += (spectrum[m] * cosines[m,k]);
                }
                cepstrum[k] = sum;
            }
            return cepstrum;
        }

    }//end of class Speech
}
