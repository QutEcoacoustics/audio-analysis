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
        /// USED TO SEGMENT A RECORDING INTO SILENCE AND VOCALISATION
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
            double backgroundThreshold = 4.0;   //SETS MIN DECIBEL BOUND
            double[,] mnr = matrix;
            mnr = SNR.RemoveModalNoise(mnr, modalNoise);
            mnr = SNR.RemoveBackgroundNoise(mnr, backgroundThreshold);
            return mnr;
        }

        public static double[,] NoiseReduce_Standbye(double[,] matrix, double[] modalNoise, double dynamicRange)
        {
            double[,] mnr = NoiseReduce_Standard(matrix, modalNoise);
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);
            return mnr;
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_FixedRange(double[,] matrix, double dynamicRange)
        {
            double[,] mnr = SNR.NoiseReduce_Standard(matrix);
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);
            return mnr;
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// The passed matrix is a sonogram with values in dB. wrt 0dB.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_PeakTracking(double[,] matrix, double dynamicRange)
        {
            double[,] mnr = matrix;
            int startFrameCount = 9;
            int smoothingWindow = 7;

            double[] modalNoise = CalculateModalNoiseUsingStartFrames(mnr, startFrameCount);
            modalNoise = DataTools.filterMovingAverage(modalNoise, smoothingWindow); //smooth the noise profile
            mnr = NoiseReduce_Standard(matrix, modalNoise);
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);

            byte[,] binary = SNR.IdentifySpectralRidges(mnr);
            double[,] op = SNR.SpectralRidges2Intensity(binary, mnr);
            return op;
        }

        public static double[,] NoiseReduce_Peaks(double[,] matrix, double dynamicRange)
        {
            double[,] mnr = matrix;
            int startFrameCount = 9;
            int smoothingWindow = 7;

            int NH = 11;
            mnr = ImageTools.WienerFilter(mnr, NH);

            double[] modalNoise = CalculateModalNoiseUsingStartFrames(mnr, startFrameCount);
            modalNoise = DataTools.filterMovingAverage(modalNoise, smoothingWindow); //smooth the noise profile
            mnr = NoiseReduce_Standard(matrix, modalNoise);
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);

            double[,] peaks = SNR.IdentifySpectralPeaks(mnr);
            //double[,] outM = SpectralRidges2Intensity(peaks, mnr);
            //return outM;
            return peaks;
        }


        //public static double[,] NoiseReduce_Sobel(double[,] matrix, double dynamicRange)
        //{
        //    double[,] mnr = matrix;
        //    int startFrameCount = 9;
        //    int smoothingWindow = 7;

        //    //int NH = 11;
        //    //mnr = ImageTools.WienerFilter(mnr, NH);

        //    double[] modalNoise = CalculateModalNoiseUsingStartFrames(mnr, startFrameCount);
        //    modalNoise = DataTools.filterMovingAverage(modalNoise, smoothingWindow); //smooth the noise profile
        //    mnr = NoiseReduce_Standard(matrix, modalNoise);
        //    mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);

        //    double[,] outM = ImageTools.SobelRidgeDetection(mnr);
        //    outM = JoinDisconnectedRidgesInBinaryMatrix(outM);
        //    outM = SNR.RemoveOrphanOnesInBinaryMatrix(outM);
        //    //double[,] outM = SpectralRidges2Intensity(peaks, mnr);
        //    return outM;
        //}

        // #############################################################################################################################
        // ################################# NOISE REDUCTION METHODS #################################################################

        /// <summary>
        /// Removes the supplied modal noise value for each freq bin and sets negative values to zero.
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


        public static double[] CalculateModalNoiseUsingStartFrames(double[,] matrix, int frameCount)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[] modalNoise = new double[colCount];

            for (int row = 0; row < frameCount; row++) //for firt N rows
            {
                for (int col = 0; col < colCount; col++)//for all cols i.e. freq bins
                {
                    modalNoise[col] += matrix[row, col];
                }
            }//end for all cols
            for (int col = 0; col < colCount; col++) modalNoise[col] /= frameCount;
            
            return modalNoise;
        }


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


        public static byte[,] IdentifySpectralRidges(double[,] matrix)
        {
            var m1 = matrix;

            var binary1 = IdentifySpectralRidgesInTemporalDirection(m1);
            binary1 = JoinDisconnectedRidgesInBinaryMatrix1(binary1, m1);

            var m2 = DataTools.MatrixTranspose(m1);
            var binary2 = IdentifySpectralRidgesInFreqDirection(m2);
            binary2 = JoinDisconnectedRidgesInBinaryMatrix1(binary2, m2);
            binary2 = DataTools.MatrixTranspose(binary2);

            //merge the two binary matrices
            int rows = binary1.GetLength(0);
            int cols = binary1.GetLength(1);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    if (binary2[r, c] == 1) binary1[r, c] = 1;
                }

            //int rows = matrix.GetLength(0);
            //int cols = matrix.GetLength(1);
            //byte[,] binary1 = new byte[rows,cols];
            //for (int r = 0; r < rows; r++)
            //    for (int c = 0; c < cols; c++)
            //    {
            //        if ((r % 3 == 0) && (c % 3 == 0)) binary1[r, c] = 1;
            //    }

            return binary1;
        }

        public static byte[,] IdentifySpectralRidgesInFreqDirection(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL RIDGES
            var binary = new byte[rows, cols];
            for (int r = 0; r < rows; r++) //row at a time, each row = one frame.
            {
                double[] row = DataTools.GetRow(matrix, r);
                row = DataTools.filterMovingAverage(row, 3);//## SMOOTH FREQ BIN - high value breaks up vertical tracks
                for (int c = 3; c < cols - 3; c++)
                {
                    double d1 = row[c] - row[c - 1];
                    double d2 = row[c] - row[c + 1];
                    double d3 = row[c] - row[c - 2];
                    double d4 = row[c] - row[c + 2];
                    //identify a peak
                    if ((d1 > 0.0) && (d2 > 0.0)
                        && (d3 > 0.0) && (d4 > 0.0)
                        && (row[c] > row[c - 3]) && (row[c] > row[c + 3])
                        //&& (d1 > d2)
                        )
                        binary[r, c] = 1;
                } //end for every col
            } //end for every row
            return binary;
        }
        public static byte[,] IdentifySpectralRidgesInTemporalDirection(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL RIDGES
            var binary = new byte[rows, cols];
            for (int r = 0; r < rows; r++) //row at a time, each row = one frame.
            {
                double[] row = DataTools.GetRow(matrix, r);
                row = DataTools.filterMovingAverage(row, 3);//## SMOOTH FRAME SPECTRUM - high value breaks up horizontal tracks
                for (int c = 3; c < cols - 3; c++)
                {
                    //identify a peak
                    if (   (row[c] > row[c - 1]) && (row[c] > row[c + 1])
                        && (row[c] > row[c - 2]) && (row[c] > row[c + 2])
                        && (row[c] > row[c - 3]) && (row[c] > row[c + 3])
                        //&& (row[c] > row[c - 4]) && (row[c] > row[c + 4])
                        //&& (row[c] > row[c - 4]) && (row[c] > row[c - 5])
                        )
                        binary[r, c] = 1;
                } //end for every col
            } //end for every row
            return binary;
        }


    /// <summary>
    ///JOINS DISCONNECTED RIDGES
    /// </summary>
    /// <returns></returns>
        public static byte[,] JoinDisconnectedRidgesInBinaryMatrix(byte[,] binary, double[,] matrix)
        {
            double threshold = 20.0; 
        int rows = binary.GetLength(0);
        int cols = binary.GetLength(1);
        byte[,] newM = new byte[rows, cols];
        
            for (int r = 0; r < rows - 3; r++) //row at a time, each row = one frame.
            {
                for (int c = 3; c < cols - 3; c++)
                {
                    if (binary[r, c] == 0)   continue;       //no peak to join
                    if (matrix[r, c] < threshold)
                    {
                        binary[r, c] = 0;
                        continue; //peak too weak to join
                    }

                    newM[r, c] = 1; //pixel r,c = 1.0
                    // skip if adjacent pixels in next row also = 1.0
                    if (binary[r + 1, c]     == 1) continue;
                    if (binary[r + 1, c - 1] == 1) continue;
                    if (binary[r + 1, c + 1] == 1) continue;

                    //fill in the same column
                    if ((binary[r + 3, c] == 1.0)) newM[r + 2, c] = 1; //fill gap
                    if ((binary[r + 2, c] == 1.0)) newM[r + 1, c] = 1; //fill gap

                    if ((binary[r + 2, c - 3] == 1.0)) newM[r + 1, c - 2] = 1; //fill gap
                    if ((binary[r + 2, c + 3] == 1.0)) newM[r + 1, c + 2] = 1; //fill gap


                    //if ((binary[r + 2, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    //if ((binary[r + 2, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap

                    if ((binary[r + 1, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    if ((binary[r + 1, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap
                }
            }
            return newM;
    }

        public static byte[,] JoinDisconnectedRidgesInBinaryMatrix1(byte[,] binary, double[,] matrix)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] newM = new byte[rows, cols];

            for (int r = 0; r < rows - 3; r++) //row at a time, each row = one frame.
            {
                for (int c = 3; c < cols - 3; c++)
                {
                    if (binary[r, c] == 0.0) continue;

                    newM[r, c] = 1;
                    // pixel r,c = 1.0 - skip if adjacent pixels in next row also = 1.0
                    if (binary[r + 1, c] == 1) continue;
                    if (binary[r + 1, c - 1] == 1) continue;
                    if (binary[r + 1, c + 1] == 1) continue;

                    //fill in the same column
                    if ((binary[r + 3, c] == 1.0)) newM[r + 2, c] = 1; //fill gap
                    if ((binary[r + 2, c] == 1.0)) newM[r + 1, c] = 1; //fill gap

                    if ((binary[r + 2, c - 3] == 1.0)) newM[r + 1, c - 2] = 1; //fill gap
                    if ((binary[r + 2, c + 3] == 1.0)) newM[r + 1, c + 2] = 1; //fill gap


                    //if ((binary[r + 2, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    //if ((binary[r + 2, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap

                    if ((binary[r + 1, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    if ((binary[r + 1, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap
                }
            }
            return newM;
        }

        
        
        /// <summary>
        /// REMOVE ORPHAN PEAKS
        /// </summary>
        /// <param name="binary"></param>
        /// <returns></returns>
        public static byte[,] RemoveOrphanOnesInBinaryMatrix(byte[,] binary)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] newM = new byte[rows, cols];
            for (int r = 1; r < rows - 1; r++) //row at a time, each row = one frame.
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    if (binary[r, c] == 0.0) continue;
                    newM[r, c] = 1;
                    if ((binary[r - 1, c] == 0)     && (binary[r + 1, c] == 0.0) &&  
                        (binary[r + 1, c + 1] == 0) && (binary[r, c + 1] == 0.0) && (binary[r - 1, c + 1] == 0.0) &&
                        (binary[r + 1, c - 1] == 0) && (binary[r, c - 1] == 0.0) && (binary[r - 1, c - 1] == 0.0))
                         newM[r, c] = 0;
                }
            }
            return newM;
        }

        public static byte[,] ThresholdBinarySpectrum(byte[,] binary, double[,] m, double threshold)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] mOut = new byte[rows, cols];
            for (int r = 1; r < rows - 1; r++) //row at a time, each row = one frame.
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    //Console.WriteLine("m[r, c]=" + m[r, c]);
                    if (binary[r, c] == 0) continue;
                    if (m[r, c] < threshold) { mOut[r, c] = 0;
                    }
                    else mOut[r, c] = 1;
                }
            }
            return mOut;
        }


        public static double[,] IdentifySpectralPeaks(double[,] matrix)
        {
            double buffer = 3.0; //dB peak requirement
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL PEAKS
            double[,] binary = new double[rows, cols];
            for (int r = 2; r < rows-2; r++) //row at a time, each row = one frame.
            {
                for (int c = 2; c < cols - 2; c++)
                {
                    //identify a peak
                    if ((matrix[r, c] > matrix[r, c - 2] + buffer) && (matrix[r, c] > matrix[r, c + 2] + buffer) //same row
                     && (matrix[r, c] > matrix[r - 2, c] + buffer) && (matrix[r, c] > matrix[r + 2, c] + buffer) //same col
                     && (matrix[r, c] > matrix[r - 1, c - 1] + buffer) && (matrix[r, c] > matrix[r + 1, c + 1] + buffer) //diagonal
                     && (matrix[r, c] > matrix[r - 1, c + 1] + buffer) && (matrix[r, c] > matrix[r + 1, c - 1] + buffer))//other diag
                    {
                        binary[r, c] = 1.0; // maxIntensity;
                        binary[r-1, c-1] = 1.0; // maxIntensity;
                        binary[r+1, c+1] = 1.0; // maxIntensity;
                        binary[r-1, c+1] = 1.0; // maxIntensity;
                        binary[r + 1, c - 1] = 1.0; // maxIntensity;
                        binary[r, c - 1] = 1.0; // maxIntensity;
                        binary[r, c + 1] = 1.0; // maxIntensity;
                        binary[r - 1, c] = 1.0; // maxIntensity;
                        binary[r + 1, c] = 1.0; // maxIntensity;
                    }
                    //else binary[r, c] = 0.0; // minIntensity;
                } //end for every col
                //binary[r, 0] = 0; // minIntensity;
                //binary[r, 1] = 0; // minIntensity;
                //binary[r, cols - 2] = 0; //minIntensity;
                //binary[r, cols - 1] = 0; //minIntensity;
            } //end for every row

            return binary;
        }

        
        /// <summary>
        /// CONVERTs a binary matrix of spectral peak tracks to an output matrix containing the acoustic intensity
        /// in the neighbourhood of those peak tracks.
        /// </summary>
        /// <param name="binary">The spectral peak tracks</param>
        /// <param name="matrix">The original sonogram</param>
        /// <returns></returns>
        public static double[,] SpectralRidges2Intensity(byte[,] binary, double[,] sonogram)
        {
            //speak track neighbourhood
            int rNH = 5;
            int cNH = 1;

            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(sonogram, out minIntensity, out maxIntensity);

            int rows = sonogram.GetLength(0);
            int cols = sonogram.GetLength(1);
            double[,] outM = new double[rows, cols];
            //initialise the output matrix/sonogram to the minimum acoustic intensity
            for (int r = 0; r < rows; r++) //init matrix to min
            {
                for (int c = 0; c < cols; c++) outM[r, c] = minIntensity; //init output matrix to min value
            }

            double localdb;
            for (int r = rNH; r < rows - rNH; r++) //row at a time, each row = one frame.
            {
                for (int c = cNH; c < cols - cNH; c++)
                {
                    if (binary[r, c] == 0.0)   continue;
                    
                    localdb = sonogram[r, c] - 3.0; //local lower bound = twice min perceptible difference
                    //scan neighbourhood
                    for (int i = r - rNH; i <= (r + rNH); i++)
                    {
                        for (int j = c - cNH; j <= (c + cNH); j++)
                        {
                            if (sonogram[i, j] > localdb) outM[i, j] = sonogram[i, j];
                            if (outM[i, j] < minIntensity) outM[i, j] = minIntensity;
                        }
                    }//end local NH
                }
            }
            return outM;
        }


        public static byte[,] PickOutLines(byte[,] binary)
        {
            int N = 7;
            int L = N - 1;
            int side = N / 2;
            int threshold = N-1; //6 out 7 matches required

            //initialise the syntactic elements - four straight line segments
            int[,] LH00 = new int[2,L];     //{ {0,0,0,0,0,0 }, {-3,-2,-1,1,2,3 } };
            for (int i = 0; i < L; i++) LH00[0, i] = 0;
            for (int i = 0; i < side; i++) LH00[1, i] = i-side;
            for (int i = 0; i < side; i++) LH00[1, side+i] = i+1;

            int[,] LV90 = new int[2, L];     // = { { -3, -2, -1, 1, 2, 3 }, { 0, 0, 0, 0, 0, 0 } };
            for (int i = 0; i < L; i++)    LV90[1, i] = 0;
            for (int i = 0; i < side; i++) LV90[0, i] = i - side;
            for (int i = 0; i < side; i++) LV90[0, side + i] = i + 1;


            int[,] Lp45 = { { 3, 2, 1, -1, -2, -3 }, { -3, -2, -1, 1, 2, 3 } };
            int[,] Lm45 = { { -3, -2, -1, 1, 2, 3 }, { -3, -2, -1, 1, 2, 3 } };
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);

            byte[,] op = new byte[rows, cols];
            for (int r = side; r < rows - side; r++) //row at a time, each row = one frame.
            {
                for (int c = side; c < cols - side; c++)
                {
                    int HL00sum = binary[r, c];
                    int VL90sum = binary[r, c];
                    int Lm45sum = binary[r, c];
                    int Lp45sum = binary[r, c];

                    for (int i = 0; i < L; i++)
                    {
                        if (binary[r + LH00[0, i], c + LH00[1, i]] == 1) HL00sum++;
                        if (binary[r + LV90[0, i], c + LV90[1, i]] == 1) VL90sum++;
                  //      if (binary[r + Lm45[0, i], c + Lm45[1, i]] == 1) Lm45sum++;
                  //      if (binary[r + Lp45[0, i], c + Lp45[1, i]] == 1) Lp45sum++;
                    }

                    int[] scores = new int[4];
                    scores[0] = HL00sum;
                    scores[1] = Lp45sum;
                    scores[2] = VL90sum;
                    scores[3] = Lm45sum;
                    int maxIndex = 0;
                    DataTools.getMaxIndex(scores, out maxIndex);

                    if ((maxIndex == 0) && (HL00sum >= threshold))
                    {
                        for (int i = 0; i < L; i++) op[r + LH00[0, i], c + LH00[1, i]] = 1;
                    }
                    //if ((maxIndex == 1) && (Lp45sum >= threshold))
                    //{
                    //    for (int i = 0; i < L; i++) op[r + Lp45[0, i], c + Lp45[1, i]] = 1;
                    //}
                    if ((maxIndex == 2) && (VL90sum >= threshold))
                    {
                        for (int i = 0; i < L; i++) op[r + LV90[0, i], c + LV90[1, i]] = 1;
                    }
                    //if ((maxIndex == 3) && (Lm45sum >= threshold))
                    //{
                    //    for (int i = 0; i < L; i++) op[r + Lm45[0, i], c + Lm45[1, i]] = 1;
                    //}

                }
            }
            return op;
        }


        public static double[,] RemoveEcho(double[,] sonogram)
        {
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(sonogram, out minIntensity, out maxIntensity);
            double range = maxIntensity = minIntensity;

            int rows = sonogram.GetLength(0);
            int cols = sonogram.GetLength(1);
            double[,] outM = new double[rows, cols];
            double momentum;

            //initialise the output matrix/sonogram to the minimum acoustic intensity
            for (int c = 0; c < cols; c++)
            {
                outM[rows - 1, c] = sonogram[rows - 1, c];
                for (int r = rows-2; r >=0 ; r--) //init matrix to min
                {
                    if (sonogram[r, c] < sonogram[r+1, c]) //ie positive slope ie rising towards peak
                    {
                        outM[r, c] = sonogram[r, c];
                        continue;
                    }
                    momentum = (maxIntensity - sonogram[r, c]) / range;
                    outM[r, c] = (momentum * sonogram[r, c]) + ((1 - momentum) * outM[r + 1, c]); //init output matrix to min value
                    //Console.WriteLine("m=" + momentum.ToString("f2") + "  ip[r, c]=" + sonogram[r, c] + "  op[r+1, c]=" + outM[r + 1, c] + "  outM[r, c]=" + outM[r, c]);
                }
            }

            return outM;
        } //end of method RemoveEcho()


    }// end class
}
