using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLib
{
    public class SNR
    {

        //reference logEnergies for signal segmentation, energy normalisation etc
        public const double MinLogEnergyReference = -7.0;    // typical noise value for BAC2 recordings = -4.5
        //public const double MaxLogEnergyReference = -0.602;// = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
        //public const double MaxLogEnergyReference = -0.310;// = Math.Log10(0.49) which assumes max average frame amplitude = 0.7
        public const double MaxLogEnergyReference = 0.0;     // = Math.Log10(1.00) which assumes max frame amplitude = 1.0
        //note that the cicada recordings reach max average frame amplitude = 0.55



        public double[] LogEnergy {get; set;}
        public double[] Decibels {get; set;}
        public double Min_dB {get; set;}
        public double Max_dB {get; set;}
        public double minEnergyRatio {get; set;}
        public double NoiseSubtracted { get; set; } //the modal noise in dB
        public double NoiseRange {get; set;}        //difference between min_dB and the modal noise dB
        public double MaxReference_dBWrtNoise {get; set;} //max reference dB wrt modal noise = 0.0dB. Used for normalisaion
        public double Snr { get; set; }             //sig/noise ratio i.e. max dB wrt modal noise = 0.0
        public double[] ModalNoiseProfile { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="frames"></param>
        public SNR(double[,] frames)
        {
            this.LogEnergy = SignalLogEnergy(frames);
            CalculateDecibelsPerFrame();
        }



        /// <summary>
        /// Frame energy is the log of the summed energy of the samples.
        /// Normally, if the passed frames are FFT spectra, then would multiply by 2 because spectra are symmetrical about Nyquist.
        /// BUT this method returns the AVERAGE sample energy, which therefore normalises for frame length / sample number. 
        /// 
        /// Energy normalisation formula taken from Lecture Notes of Prof. Bryan Pellom
        /// Automatic Speech Recognition: From Theory to Practice.
        /// http://www.cis.hut.fi/Opinnot/T-61.184/ September 27th 2004.
        /// 
        /// Calculate normalised energy of frame as  energy[i] = logEnergy - maxLogEnergy;
        /// This is same as log10(logEnergy / maxLogEnergy) ie normalised to a fixed maximum energy value.
        /// </summary>
        /// <param name="frames">a matrix containing signal values grouped as overlapping frames</param>
        /// <param name="minLogEnergy">an arbitrary minimum to prevent large negative log values</param>
        /// <param name="maxLogEnergy">absolute max to which we normalise</param>
        /// <returns></returns>
        public static double[] SignalLogEnergy(double[,] frames)
        {
            int frameCount = frames.GetLength(0);
            int N = frames.GetLength(1);
            double[] logEnergy = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double sum = 0.0;
                for (int j = 0; j < N; j++)  //foreach sample in frame
                {
                    sum += (frames[i, j] * frames[i, j]); //sum the energy = amplitude squared
                }
                double e = sum / (double)N; //normalise to frame size i.e. average energy per sample
                //Console.WriteLine("e=" + e);
                //if (e > 0.25) Console.WriteLine("e > 0.25 = " + e);

                if (e == Double.MinValue) //to guard against log(0) but this should never happen!
                //if (e == 0.0000000000) //to guard against log(0) but this should never happen!
                {
                    System.Console.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
                    logEnergy[i] = SNR.MinLogEnergyReference - SNR.MaxLogEnergyReference; //normalise to absolute scale
                    continue;
                }
                double logE = Math.Log10(e);

                //normalise to ABSOLUTE energy value i.e. as defined in header of Sonogram class
                if (logE < SNR.MinLogEnergyReference)
                {
                    logEnergy[i] = SNR.MinLogEnergyReference - SNR.MaxLogEnergyReference;
                }
                else logEnergy[i] = logE - SNR.MaxLogEnergyReference;
            }

            //could alternatively normalise to RELATIVE energy value i.e. max frame energy in the current signal
            //double maxEnergy = logEnergy[DataTools.getMaxIndex(logEnergy)];
            //for (int i = 0; i < frameCount; i++) //foreach time step
            //{
            //    logEnergy[i] = ((logEnergy[i] - maxEnergy) * 0.1) + 1.0; //see method header for reference 
            //}
            return logEnergy;
        }



        /// <summary>
        /// This method subtracts the estimated background noise from the frame energies and converts all values to dB.
        /// algorithm described in Lamel et al, 1981.
        /// NOTE: noiseThreshold is passed as decibels
        /// energy array is log energy ie not yet converted to decibels.
        /// Return energy converted to decibels i.e. multiply by 10.
        /// </summary>
        /// <param name="logEnergy">NOTE: the log energy values are normalised to global constants</param>
        /// <param name="min_dB"></param>
        /// <param name="max_dB"></param>
        /// <param name="noiseThreshold_dB"></param>
        /// <param name="Q">noise in decibels subtracted from each frame</param>
        /// <returns></returns>
        public static double[] NoiseSubtract(double[] logEnergy, out double min_dB, out double max_dB, out double Q)
        {
            //Following const used to normalise the logEnergy values to the background noise.
            //Has the effect of setting background noise level to 0 dB.
            //Value of 10dB is in Lamel et al, 1981. They call it "Adaptive Level Equalisatsion".
            const double noiseThreshold_dB = 10.0; //dB
            double minEnergyRatio = SNR.MinLogEnergyReference - SNR.MaxLogEnergyReference;


            //ignore first N and last N frames when calculating background noise level because sometimes these frames
            // have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level
            //HOWEVER do not ignore them for short recordings!
            if (logEnergy.Length < 1000) buffer = 0; //ie recording is < approx 11 seconds long

            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            //Console.WriteLine("minFractionEnergy = " + minFraction);
            for (int i = buffer; i < logEnergy.Length - buffer; i++)
            {
                if (logEnergy[i] == minEnergyRatio) continue; //ignore lowest values in establishing noise level
                if (logEnergy[i] < min) min = logEnergy[i];
                else
                    if (logEnergy[i] > max) max = logEnergy[i];
            }
            min_dB = min * 10;  //multiply by 10 to convert to decibels
            max_dB = max * 10;

            int binCount = 100;
            double binWidth = noiseThreshold_dB / binCount;
            int[] histo = new int[binCount];
            int L = logEnergy.Length;
            double absThreshold = min_dB + noiseThreshold_dB;

            for (int i = 0; i < L; i++)
            {
                double dB = 10 * logEnergy[i];
                if (dB <= absThreshold)
                {
                    int id = (int)((dB - min_dB) / binWidth);
                    if (id >= binCount)
                    {
                        id = binCount - 1;
                    }
                    else
                        if (id < 0) id = 0;
                    histo[id]++;
                }
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            int peakID = DataTools.GetMaxIndex(smoothHisto);
            Q = min_dB + ((peakID + 1) * binWidth); //modal noise level

            // subtract noise energy` and return relative energy as decibel values.
            double[] dBFrames = new double[L];
            for (int i = 0; i < L; i++) dBFrames[i] = (logEnergy[i] * 10) - Q;
            //Console.WriteLine("minDB=" + min_dB + "  max_dB=" + max_dB);
            //Console.WriteLine("peakID=" + peakID + "  Q=" + Q);

            return dBFrames;
        }

        /// <summary>
        /// subtract background noise to produce a decibels array in which zero dB = modal noise
        /// </summary>
        /// <param name="logEnergy"></param>
        /// <returns></returns>
        public void CalculateDecibelsPerFrame()
        {
            double Q;
            double min_dB;
            double max_dB;
            this.Decibels = NoiseSubtract(this.LogEnergy, out min_dB, out max_dB, out Q);
            this.NoiseSubtracted = Q;
            this.Min_dB = min_dB; //min decibels of all frames 
            this.Max_dB = max_dB;
            this.NoiseRange = min_dB - Q;
            this.Snr = max_dB - Q;
            //need an appropriate dB reference level for normalising dB arrays.
            //this.MaxReference_dBWrtNoise = (SNR.MaxEnergyReference *10) -Q; // NO GOOD!
            //this.MaxReference_dBWrtNoise = max_dB - Q;                      // OK
            this.MaxReference_dBWrtNoise = max_dB - min_dB;                   // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }

        public double FractionHighEnergyFrames(double dbThreshold)
        {
            return FractionHighEnergyFrames(this.Decibels, dbThreshold);
        }

        public static double FractionHighEnergyFrames(double[] dbArray, double dbThreshold)
        {
            int L = dbArray.Length;
            int count = 0;
            for (int i = 0; i < L; i++) //foreach time step
            {
                if (dbArray[i] > dbThreshold) count++;
            }
            return (count / (double)L);
        }

        public double[] NormaliseDecibelArray_ZeroOne(double maxDecibels)
        {
            return NormaliseDecibelArray_ZeroOne(this.Decibels, maxDecibels);
        }

        /// <summary>
        /// normalise the power values using the passed reference decibel levels
        /// NOTE: This method assumes that the energy values are in decibels and that they have been scaled
        /// so that the modal noise value = 0 dB. Simply truncate all values below this to zero dB
        /// </summary>
        /// <param name="energy"></param>
        /// <param name="maxDecibels"></param>
        /// <returns></returns>
        public static double[] NormaliseDecibelArray_ZeroOne(double[] dB, double maxDecibels)
        {
            //normalise power between 0.0 decibels and max decibels.
            int L = dB.Length;
            double[] E = new double[L];
            for (int i = 0; i < L; i++)
            {
                E[i] = dB[i];
                if (E[i] <= 0.0) E[i] = 0.0;
                else E[i] = dB[i] / maxDecibels;
                if (E[i] > 1.0) E[i] = 1.0;
            }
            return E;
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_Standard(double[,] matrix)
        {
            //calculate modal noise for each freq bin
            double[] modalNoise = SNR.CalculateModalNoise(matrix);     //calculate modal noise profile
            modalNoise = DataTools.filterMovingAverage(modalNoise, 7); //smooth the noise profile
            return NoiseReduce_Standard(matrix, modalNoise);
        }

        public static double[,] NoiseReduce_Standard(double[,] matrix, double[] modalNoise)
        {
            double decibelThreshold = 6.5;   //SETS MIN DECIBEL BOUND

            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double[,] mnr = matrix;
            //mnr = ImageTools.WienerFilter(mnr); //has slight blurring effect and so decide not to use
            mnr = SNR.RemoveModalNoise(mnr, modalNoise);
            mnr = SNR.RemoveBackgroundNoise(mnr, decibelThreshold);
            return mnr;
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_FixedRange(double[,] matrix, double dynamicRange)
        {
            double decibelThreshold = 6.5;   //SETS MIN DECIBEL BOUND
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double[,] mnr = matrix; //matrix - noise reduced
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);
            mnr = SNR.RemoveBackgroundNoise(mnr, decibelThreshold);
            return mnr;
        }

        public static double[,] NoiseReduce_Standbye(double[,] matrix, double[] modalNoise, double dynamicRange)
        {
            double decibelThreshold = 6.5;   //SETS MIN DECIBEL BOUND

            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double[,] mnr = matrix;
            //mnr = ImageTools.WienerFilter(mnr); //has slight blurring effect and so decide not to use
            mnr = SNR.RemoveModalNoise(mnr, modalNoise);
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);
            mnr = SNR.RemoveBackgroundNoise(mnr, decibelThreshold);
            return mnr;
        }



        // #############################################################################################################################
        // ################################# NOISE REDUCTION ALGORITHM #################################################################

        /// <summary>
        /// Calculates the modal noise value for each freq bin and subtracts same.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] RemoveModalNoise(double[,] matrix, double[] modalNoise)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount];          //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++)//for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++)  //for all rows
                {
                    outM[y, col] = matrix[y, col] - modalNoise[col];
                    if (outM[y, col] < 0.0) outM[y, col] = 0.0;
                }//end for all rows
            }//end for all cols
            return outM;
        }// end of RemoveModalNoise()


        public static double[] CalculateModalNoise(double[,] matrix, int smoothingWindow)
        {
            var m = CalculateModalNoise(matrix);
            return DataTools.filterMovingAverage(m, smoothingWindow); //smooth the noise profile
        }

        /// <summary>
        /// Calculates the modal noise value for each freq bin.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[] CalculateModalNoise(double[,] matrix)
        {
            //set parameters for noise histograms based on overlapping bands.
            //*******************************************************************************************************************
            int bandWidth = 3;  // should be an odd number
            int binCount = 64;  //number of pixel intensity bins
            int binLimit = (int)(binCount * 0.666); //sets upper limit to modal noise bin. Higher values = more severe noise removal.
            //*******************************************************************************************************************


            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double binWidth = (maxIntensity - minIntensity) / binCount;  //width of an intensity bin
            //Console.WriteLine("minIntensity=" + minIntensity + "  maxIntensity=" + maxIntensity + "  binWidth=" + binWidth);

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            if (bandWidth > colCount) bandWidth = colCount - 1;
            int halfWidth = bandWidth / 2;

            //init matrix from which histogram derived
            double[,] submatrix = DataTools.Submatrix(matrix, 0, 0, rowCount - 1, bandWidth);
            double[] modalNoise = new double[colCount];

            for (int col = 0; col < colCount; col++)//for all cols i.e. freq bins
            {
                //construct new submatrix to calculate modal noise
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= colCount) stop = colCount - 1;
                submatrix = DataTools.Submatrix(matrix, 0, start, rowCount - 1, stop);
                int[] histo = DataTools.Histo(submatrix, binCount, minIntensity, maxIntensity, binWidth);
                //DataTools.writeBarGraph(histo);
                double[] smoothHisto = DataTools.filterMovingAverage(histo, 7);
                int maxindex; //mode
                DataTools.getMaxIndex(smoothHisto, out maxindex); //this is mode of histogram
                if (maxindex > binLimit) maxindex = binLimit;
                modalNoise[col] = minIntensity + (maxindex * binWidth);
                //Console.WriteLine("  modal index=" + maxindex + "  modalIntensity=" + modalIntensity.ToString("F3"));
            }//end for all cols
            return modalNoise;
        }// end of CalculateModalNoise(double[,] matrix)


        /// <summary>
        /// sets the dynamic range in dB for a sonogram. 
        /// All intensity values are shifted so that the max intensity value = maxDB parameter.
        /// All values which fall below the minDB parameter are then set = to minDB.
        /// </summary>
        /// <param name="m">The spectral sonogram passes as matrix of doubles</param>
        /// <param name="minDB">minimum decibel value</param>
        /// <param name="maxDB">maximum decibel value</param>
        /// <returns></returns>
        public static double[,] SetDynamicRange(double[,] m, double minDB, double maxDB)
        {
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(m, out minIntensity, out maxIntensity);
            double shift = maxDB - maxIntensity;

            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            double[,] normM = new double[rowCount, colCount];
            for (int col = 0; col < colCount; col++)//for all cols i.e. freq bins
            {
                for (int row = 0; row < rowCount; row++) //for all rows
                {
                    normM[row, col] = m[row, col] + shift;
                    if (normM[row, col] < minDB) normM[row, col] = 0;
                }
            }
            return normM;
        }//end NormaliseIntensity(double[,] m, double minDB, double maxDB)



        public static double[,] RemoveBackgroundNoise(double[,] matrix, double threshold)
        {
            int M = 3; // each row is a frame or time instance
            int N = 9; // each column is a frequency bin
            int rNH = M / 2;
            int cNH = N / 2;

            double min;
            double max;
            DataTools.MinMax(matrix, out min, out max);
            threshold += min;
            //int[] h = DataTools.Histo(matrix, 50);
            //DataTools.writeBarGraph(h);

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    //if (matrix[r, c] <= 70.0) continue;
                    double X = 0.0;
                    double Xe2 = 0.0;
                    int count = 0;
                    for (int i = r - rNH; i <= (r + rNH); i++)
                    {
                        if (i < 0) continue;
                        if (i >= rows) continue;
                        for (int j = c - cNH; j <= (c + cNH); j++)
                        {
                            if (j < 0) continue;
                            if (j >= cols) continue;
                            X += matrix[i, j];
                            Xe2 += (matrix[i, j] * matrix[i, j]);
                            count++;
                            //Console.WriteLine(i+"  "+j+"   count="+count);
                            //Console.ReadLine();
                        }
                    }//end local NH
                    double mean = X / count;
                    //double variance = (Xe2 / count) - (mean * mean);

                    //if ((c<(cols/5))&&(mean < (threshold+1.0))) outM[r, c] = min;
                    //else
                    if (mean < threshold) outM[r, c] = min;
                    else outM[r, c] = matrix[r, c];
                    //Console.WriteLine((outM[r, c]).ToString("F1") + "   " + (matrix[r, c]).ToString("F1") + "  mean=" + mean + "  variance=" + variance);
                    //Console.ReadLine();
                }
            }
            return outM;
        }// end RemoveBackgroundNoise()


    }// end class
}
