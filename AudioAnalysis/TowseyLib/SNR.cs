using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLib
{

    public enum NoiseReductionType { NONE, STANDARD, MODAL, BINARY, FIXED_DYNAMIC_RANGE, PEAK_TRACKING, HARMONIC_DETECTION }


    public class SNR
    {
        //CONSTANT
        // FIND MAX VALUE IN BOTTOM 80% OF RANGE.
        // this Constant sets an upper limit on the value returned as the modal noise.
        public static double FRACTIONAL_BOUND_FOR_MODE = 0.8;


        public struct key_Snr
        {
            public const string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
            public const string key_DYNAMIC_RANGE        = "DYNAMIC_RANGE";
            //public const string Key_SilenceRecording   = "SILENCE_RECORDING_PATH"; //used to determin silence model.
        }


        //reference logEnergies for signal segmentation, energy normalisation etc
        public const double MinLogEnergyReference = -6.0;    // = -60dB. Typical noise value for BAC2 recordings = -4.5 = -45dB
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
        public double Snr { get; set; }             //sig/noise ratio i.e. max dB - modal noise
        public double NoiseRange { get; set; }        //difference between min_dB and the modal noise dB
        public double MaxReference_dBWrtNoise { get; set; } //max reference dB wrt modal noise = 0.0dB. Used for normalisaion
        public double[] ModalNoiseProfile { get; set; }




        /// <summary>
        /// Removes noise from a spectrogram. Choice of methods.
        /// Make sure that do MelScale reduction BEFORE applying noise filter.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="nrt"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static System.Tuple<double[,], double[]> NoiseReduce(double[,] m, NoiseReductionType nrt, double parameter)
        {
            double[] smoothedArray = null;
            if (nrt == NoiseReductionType.STANDARD)
            {
                double[] modalNoise = SNR.CalculateModalNoise(m);             //calculate noise profile - assumes a dB spectrogram.
                smoothedArray = DataTools.filterMovingAverage(modalNoise, 7); //smooth the noise profile
                m = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(m, smoothedArray);
                double backgroundThreshold = parameter;
                m = SNR.RemoveBackgroundNoise(m, backgroundThreshold);
            }
            else
            if (nrt == NoiseReductionType.MODAL)
            {
                double[] modalValues = SNR.CalculateModalValues(m);            //calculate modal profile - any matrix of values
                smoothedArray = DataTools.filterMovingAverage(modalValues, 7); //smooth the modal profile
                m = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(m, smoothedArray);
            }
            else
            if (nrt == NoiseReductionType.BINARY)
            {
                double[] modalValues = SNR.CalculateModalValues(m);            //calculate modal profile
                smoothedArray = DataTools.filterMovingAverage(modalValues, 7); //smooth the modal profile
                m = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(m, smoothedArray); //remove BG noise
                double backgroundThreshold = parameter;
                m = SNR.RemoveBackgroundNoise(m, backgroundThreshold);         //smooth background further
                m = DataTools.Matrix2Binary(m, 2*backgroundThreshold);         //convert to binary 
            }
            else
            if (nrt == NoiseReductionType.FIXED_DYNAMIC_RANGE)
            {
                Log.WriteIfVerbose("\tNoise reduction: FIXED DYNAMIC RANGE = " + parameter);
                m = SNR.NoiseReduce_FixedRange(m, parameter);
            }
            else
            if (nrt == NoiseReductionType.PEAK_TRACKING)
            {
                Log.WriteIfVerbose("\tNoise reduction: PEAK_TRACKING. Dynamic range= " + parameter);
                m = SNR.NoiseReduce_PeakTracking(m, parameter);
            }
            else
            if (nrt == NoiseReductionType.HARMONIC_DETECTION)
            {
                Log.WriteIfVerbose("\tNoise reduction: HARMONIC_DETECTION");
                m = SNR.NoiseReduce_HarmonicDetection(m);
            }
            var tuple = System.Tuple.Create(m, smoothedArray);
            return tuple;
        }



        /// <summary>
        /// Converts a string interpreted as a key to a NoiseReduction Type.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static NoiseReductionType Key2NoiseReductionType(string key)
        {
            if (key.Equals("NONE")) return NoiseReductionType.NONE;
            else
                if (key.Equals("STANDARD")) return NoiseReductionType.STANDARD;
                else
                    if (key.Equals("FIXED_DYNAMIC_RANGE")) return NoiseReductionType.FIXED_DYNAMIC_RANGE;
                    else
                        if (key.Equals("PEAK_TRACKING")) return NoiseReductionType.PEAK_TRACKING;
                        else
                            if (key.Equals("HARMONIC_DETECTION")) return NoiseReductionType.HARMONIC_DETECTION;
            return NoiseReductionType.NONE;
        }



        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="frames">all the overlapped frames of a signal</param>
        public SNR(double[,] frames)
        {
            this.LogEnergy = SignalLogEnergy(frames);
            this.Decibels = ConvertLogEnergy2Decibels(this.LogEnergy); //convert logEnergy to decibels.
            SubtractBackgroundNoise_dB();
            this.NoiseRange = this.Min_dB - this.NoiseSubtracted;
            //need an appropriate dB reference level for normalising dB arrays.
            //this.MaxReference_dBWrtNoise = this.Snr;                        // OK
            this.MaxReference_dBWrtNoise = this.Max_dB - this.Min_dB;         // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal">signal </param>
        /// <param name="frameIDs">starts and end index of each frame</param>
        public SNR(double[] signal, int[,] frameIDs)
        {
            this.LogEnergy = Signal2LogEnergy(signal, frameIDs);
            this.Decibels = ConvertLogEnergy2Decibels(this.LogEnergy); //convert logEnergy to decibels.
            SubtractBackgroundNoise_dB();
            this.NoiseRange = this.Min_dB - this.NoiseSubtracted;
            this.MaxReference_dBWrtNoise = this.Max_dB - this.Min_dB;         // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }

        /// <summary>
        /// Returns the frame energy of the signal.
        /// The energy of a frame/window is the log of the summed energy of all the samples in the frame.
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
        public static double[] Signal2LogEnergy(double[] signal, int[,] frameIDs)
        {
            int frameCount = frameIDs.GetLength(0);
            int N          = frameIDs[0,1] + 1; //window or frame width
            double[] logEnergy = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double sum = 0.0;
                for (int j = 0; j < N; j++)  //foreach sample in frame
                {
                    sum += Math.Pow(signal[frameIDs[i,0] + j], 2); //sum the energy = amplitude squared
                }
                double e = sum / (double)N; //normalise to frame size i.e. average energy per sample
                //Console.WriteLine("e=" + e);
                //if (e > 0.25) Console.WriteLine("e > 0.25 = " + e);

                if (e == Double.MinValue) //to guard against log(0) but this should never happen!
                {
                    Console.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
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


        public static double[] Signal2Power(double[] signal)
        {
            int L = signal.Length;
            double[] energy = new double[L];
            for (int i = 0; i < L; i++) //foreach signal sample
            {
                energy[i] =  signal[i] * signal[i]; //energy = amplitude squared
            }
            return energy;
        }


        public static double[] Signal2Decibels(double[] signal)
        {
            int L = signal.Length;
            double[] dB = new double[L];
            for (int i = 0; i < L; i++) //foreach signal sample
            {
                dB[i] = 20 * Math.Log10(signal[i]); //10 times log of amplitude squared
            }
            return dB;
        }

        /// <summary>
        /// this calculation of frame log energy is used only to calculate the log energy in each frame 
        /// of a sub-band of the sonogram.
        /// </summary>
        /// <param name="frames"></param>
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
                {
                    Console.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
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
            return logEnergy;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBMatrix"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="freqBinWidth">herz per freq bin</param>
        /// <returns></returns>
        public static double[] DecibelsInSubband(double[,] dBMatrix, int minHz, int maxHz, double freqBinWidth)
        {
            int frameCount = dBMatrix.GetLength(0);
            int N = dBMatrix.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            double[] db = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double sum = 0.0;
                for (int j = minBin; j <= maxBin; j++)  // foreach bin in the bandwidth in frame
                {
                    sum += dBMatrix[i, j];              // sum the dB values
                }
                db[i] = sum;
            }
            return db;
        }
        /// <summary>
        /// returns a spectrogram with reduced number of frequency bins
        /// </summary>
        /// <param name="inSpectro">input spectrogram</param>
        /// <param name="subbandCount">numbre of req bands in output spectrogram</param>
        /// <returns></returns>
        public static double[,] ReduceFreqBinsInSpectrogram(double[,] inSpectro, int subbandCount)
        {
            int frameCount = inSpectro.GetLength(0);
            int N = inSpectro.GetLength(1);
            double[,] outSpectro = new double[frameCount, subbandCount];
            int binWidth = N / subbandCount;
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                int startBin;
                int endBin;
                double sum = 0.0;
                for (int j = 0; j < subbandCount-1; j++)  // foreach output band EXCEPT THE LAST
                {
                    startBin = j * binWidth;
                    endBin   = startBin + binWidth;
                    for (int b = startBin; b < endBin; b++)  // foreach output band
                    {
                        sum += inSpectro[i, b];              // sum the spectral values
                    }
                    outSpectro[i, j] = sum;
                }
                //now do the top most freq band
                startBin = (subbandCount-1) * binWidth;
                for (int b = startBin; b < N; b++)  // foreach output band
                {
                    sum += inSpectro[i, b];              // sum the spectral values
                }
                outSpectro[i, subbandCount-1] = sum;
            }
            return outSpectro;
        }






        /// <summary>
        /// Calculates and subtracts the modal value from an array of double.
        /// Used for calculating and removing the background noise and setting baseline = zero.
        /// Values below zero set equal to zero.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Q">The modal value</param>
        /// <param name="oneSD">Standard deviation of the baseline noise</param>
        /// <returns></returns>
        public static double[] NoiseSubtractMode(double[] array, out double Q, out double oneSD)
        {
            int L = array.Length;
            //CALCULATE THE MIN AND MAX OF THE ARRAY
            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            for (int i = 0; i < L; i++)
            {
                if (array[i] < min) min = array[i];
                else
                if (array[i] > max) max = array[i];
            }

            //set up Histogram
            int binCount = 100;
            double binWidth = (max - min) / binCount;
            int[] histo = new int[binCount];

            for (int i = 0; i < L; i++)
            {
                    int id = (int)((array[i] - min) / binWidth);
                    if (id >= binCount)
                    {
                        id = binCount - 1;
                    }
                    else
                        if (id < 0) id = 0;
                    histo[id]++;
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            // FIND MAX VALUE IN BOTTOM FRACTION OF RANGE. ASSUMES NOISE IS GAUSSIAN and that their is some signal.
            int upperBound = (int)(binCount * SNR.FRACTIONAL_BOUND_FOR_MODE);
            for (int i = upperBound; i < binCount; i++) smoothHisto[i] = 0;//set top 50% of intensity bins = 0. 
            int peakID = DataTools.GetMaxIndex(smoothHisto);
            Q = min + ((peakID + 1) * binWidth); //modal noise level

            //calculate SD of the background noise
            //oneSD = (Q - min) / 3;
            double total = 0;
            double ssd = 0.0; //sum of squared deviations
            for (int i = 0; i < peakID; i++)
            {
                total += smoothHisto[i];
                double dev = (peakID - i) * binWidth;
                ssd += dev * dev; //sum of squared deviations
            }
            
            if (peakID > 0) oneSD = Math.Sqrt(ssd / total);
            else            oneSD = Math.Sqrt((binWidth * binWidth) / smoothHisto[0]); //deal with case where peakID = 0 to prevent division by 0;
            

            // subtract modal noise and return array.
            double[] newArray = new double[L];
            for (int i = 0; i < L; i++)
            {
                newArray[i] = array[i] - Q;
                if (newArray[i] < 0.0) newArray[i] = 0.0;
            }
            return newArray;
        }

        public static double[] TruncateNegativeValues2Zero(double[] inArray)
        {
            int L = inArray.Length;
            var outArray = new double[L];
            for (int i = 0; i < L; i++) //foreach row
            {
                if (inArray[i] < 0.0) outArray[i] = 0.0;
                else                  outArray[i] = inArray[i];
            }
            return outArray;
        }
        public static double[,] TruncateNegativeValues2Zero(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var M = new double[rows, cols];
            for (int r = 0; r < rows; r++) //foreach row
            {
                for (int c = 0; c < cols; c++)  
                {
                    if (m[r, c] < 0.0) M[r, c] = 0.0;
                    else               M[r, c] = m[r, c];
                }
            }
            return M;
        }


        /// <summary>
        /// subtract background noise to produce a decibels array in which zero dB = modal noise
        /// DOES NOT TRUNCATE BELOW ZERO VALUES.
        /// </summary>
        /// <param name="logEnergy"></param>
        /// <returns></returns>
        public void SubtractBackgroundNoise_dB()
        {
            var results = SubtractBackgroundNoise_dB(this.Decibels);
            this.Decibels = results.Item1;
            this.NoiseSubtracted = results.Item2; //Q
            this.Min_dB   = results.Item3;   //min decibels of all frames 
            this.Max_dB   = results.Item4;   //max decibels of all frames 
            this.Snr      = results.Item5;   // = max_dB - Q;
        }

        /// <summary>
        /// subtract background noise to produce a decibels array in which zero dB = modal noise
        /// DOES NOT TRUNCATE BELOW ZERO VALUES.
        /// RETURNS: 1) noise reduced decibel array; 2) Q - the modal BG level; 3) min value 4) max value; 5) snr
        /// </summary>
        /// <param name="dBarray"></param>
        /// <returns>System.Tuple.Create(decibels, Q, min_dB, max_dB, snr); System.Tuple(double[], double, double, double, double) 
        /// </returns>
        public static System.Tuple<double[], double, double, double, double> SubtractBackgroundNoise_dB(double[] dBarray)
        {
            double Q;
            double min_dB;
            double max_dB;
            double[] decibels = SubtractBackgroundNoise(dBarray, out min_dB, out max_dB, out Q);
            double snr = max_dB - Q;
            return System.Tuple.Create(decibels, Q, min_dB, max_dB, snr);
        }
       
        /// </summary>
        /// <param name="sonogram">sonogram of signal - values in dB</param>
        /// <param name="minHz">min of freq band to sample</param>
        /// <param name="maxHz">max of freq band to sample</param>
        /// <param name="nyquist">signal nyquist - used to caluclate hz per bin</param>
        /// <param name="smoothDuration">window width (in seconds) to smooth sig intenisty</param>
        /// <param name="framesPerSec">time scale of the sonogram</param>
        /// <returns></returns>
        public static System.Tuple<double[], double, double> SubbandIntensity_NoiseReduced(double[,] sonogram, int minHz, int maxHz, int nyquist, double smoothDuration, double framesPerSec)
        {
            //A: CALCULATE THE INTENSITY ARRAY
            double[] intensity = CalculateFreqBandAvIntensity(sonogram, minHz, maxHz, nyquist);
            //B: SMOOTH THE INTENSITY ARRAY
            int smoothWindow = (int)Math.Round(framesPerSec * smoothDuration);
            if ((smoothWindow != 0) && (smoothWindow % 2) == 0) smoothWindow += 1;  //Convert to odd number for smoothing
            intensity = DataTools.filterMovingAverage(intensity, smoothWindow);
            //C: REMOVE NOISE FROM INTENSITY ARRAY
            double Q; //modal noise in DB
            double oneSD; //one sd of modal noise in dB
            intensity = SNR.NoiseSubtractMode(intensity, out Q, out oneSD);
            var tuple = System.Tuple.Create(intensity, Q, oneSD);
            return tuple;
        }

        /// <summary>
        /// Calculates the mean intensity in a freq band defined by its min and max freq.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="nyquist"></param>
        /// <returns></returns>
        public static double[] CalculateFreqBandAvIntensity(double[,] sonogram, int minHz, int maxHz, int nyquist)
        {
            int frameCount = sonogram.GetLength(0);
            int binCount = sonogram.GetLength(1);
            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);
            int bandCount = maxBin - minBin + 1;
            double[] intensity = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                for (int j = minBin; j < maxBin; j++) intensity[i] += sonogram[i, j];
                intensity[i] /= bandCount;
            }
            return intensity;
        }


        public static double[] ConvertLogEnergy2Decibels(double[] logEnergy)
        {
            var dB = new double[logEnergy.Length];
            for (int i = 0; i < logEnergy.Length; i++) dB[i] = logEnergy[i] * 10; //Convert log energy to decibels.
            return dB;
        }


        /// <summary>
        /// Implements the "Adaptive Level Equalisatsion" algorithm of Lamel et al, 1981 - with modifications for our signals.
        /// This method subtracts the estimated background noise from each frame.
        /// It has the effect of setting average background noise level = 0 dB.
        /// Values below zero dB are NOT truncated. 
        /// The algorithm is described in Lamel et al, 1981.
        /// USED TO SEGMENT A RECORDING INTO SILENCE AND VOCALISATION
        /// NOTE: noiseThreshold is passed as decibels. Algorithm ONLY SEARCHES in range min to 10dB above min.
        /// Units are assumed to be decibels.
        /// </summary>
        /// <param name="dBarray">NOTE: the decibel values are assumed to lie between -70 dB and 0 dB</param>
        /// <param name="noiseThreshold_dB">Sets dB range in which to find value for background noise.</param>
        /// <param name="min_dB"></param>
        /// <param name="max_dB"></param>
        /// <param name="Q">noise in decibels subtracted from each frame</param>
        /// <returns></returns>
        public static double[] SubtractBackgroundNoise(double[] dBarray, double noiseThreshold_dB, out double min_dB, out double max_dB, out double Q)
        {

            int L = dBarray.Length;

            //ignore first N and last N frames when calculating background noise level because 
            // sometimes these frames have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level
            //HOWEVER do not ignore them for short recordings!
            if (L < 1000) buffer = 0; //ie recording is < approx 11 seconds long


            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            double minDecibels = (SNR.MinLogEnergyReference - SNR.MaxLogEnergyReference) * 10;  // = -70dB
            for (int i = buffer; i < L - buffer; i++)
            {
                if (dBarray[i] <= minDecibels) continue; //ignore lowest values in establishing noise level
                if (dBarray[i] < min) min = dBarray[i];
                else
                if (dBarray[i] > max) max = dBarray[i];
            }
            min_dB = min;  // return out
            max_dB = max;

            int binCount = 100;
            double binWidth = noiseThreshold_dB / binCount;
            int[] histo = new int[binCount];
            double absThreshold = min_dB + noiseThreshold_dB;

            for (int i = 0; i < L; i++)
            {
                //double dB = dBarray[i];
                if (dBarray[i] <= absThreshold)
                {
                    int id = (int)((dBarray[i] - min_dB) / binWidth);
                    if (id >= binCount) id = binCount - 1;
                    else if (id < 0) id = 0;
                    histo[id]++;
                }
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            int peakID = DataTools.GetMaxIndex(smoothHisto);
            Q = min_dB + ((peakID + 1) * binWidth); //modal noise level

            // subtract noise.
            double[] dBFrames = new double[L];
            for (int i = 0; i < L; i++) dBFrames[i] = dBarray[i] - Q;
            //Console.WriteLine("minDB=" + min_dB + "  max_dB=" + max_dB);
            //Console.WriteLine("peakID=" + peakID + "  Q=" + Q);

            return dBFrames;
        }

        /// <summary>
        /// Implements the "Adaptive Level Equalisatsion" of Lamel et al, 1981.
        /// Value of 10 dB is a default used by Lamel et al.
        /// </summary>
        /// <param name="dBarray"></param>
        /// <param name="min_dB"></param>
        /// <param name="max_dB"></param>
        /// <param name="Q"></param>
        /// <returns></returns>
        public static double[] SubtractBackgroundNoise(double[] dBarray, out double min_dB, out double max_dB, out double Q)
        {
            //Following const used to normalise dB values to the background noise. It has the effect of setting background noise level to 0 dB.
            //Value of 10dB is in Lamel et al, 1981. They call it "Adaptive Level Equalisatsion".
            const double noiseThreshold_dB = 10.0; //dB
            return SubtractBackgroundNoise(dBarray, noiseThreshold_dB, out min_dB, out max_dB, out Q);
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
        /// normalise the power values using the passed reference decibel level.
        /// NOTE: This method assumes that the energy values are in decibels and that they have been scaled
        /// so that the modal noise value = 0 dB. Simply truncate all values below this to zero dB
        /// </summary>
        /// <param name="dB"></param>
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

        /// <summary>
        /// expects a spectrogram in dB values
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="modalNoise"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_Standard(double[,] matrix, double[] modalNoise)
        {
            double backgroundThreshold = 4.0;   //SETS MIN DECIBEL BOUND
            double[,] mnr = matrix;
            mnr = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(mnr, modalNoise);
            mnr = SNR.RemoveBackgroundNoise(mnr, backgroundThreshold);
            return mnr;
        }

        /// <summary>
        /// This method is specifically to preprocess the spectrogram prior to identification of harmonic stacks.
        /// (1) The spectrogram is smoothed in temporal direction only. THIS IS OPTIONAL - WORKS WITHOUT DOING
        /// (2) After smoothing the modal noise is calculated as per standard approach.
        /// (3) The modal noise is subtracted but WITHOUT thresholding. i.e. keep negative values.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_HarmonicDetection(double[,] matrix)
        {
            //double[,] smoothMatrix = SNR.SmoothInTemporalDirectionOnly(matrix, 3); //3=smootihng window
            double[,] smoothMatrix = matrix;
            double[]  modalNoise   = SNR.CalculateModalNoise(smoothMatrix);        //calculate modal noise profile
            modalNoise             = DataTools.filterMovingAverage(modalNoise, 7); //smooth the noise profile
            return SubtractBgNoiseFromSpectrogram(smoothMatrix, modalNoise);                   //subtract modal noise but do NOT threshold
            //return RemoveModalNoise(smoothMatrix, modalNoise);                     //subtract modal noise AND threshold at ZERO
        }
        
        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_FixedRange(double[,] matrix, double dynamicRange)
        {
            //calculate modal noise for each freq bin
            double[] modalNoise = SNR.CalculateModalNoise(matrix);     //calculate modal noise profile
            modalNoise = DataTools.filterMovingAverage(modalNoise, 7); //smooth the noise profile
            double[,] mnr = SNR.SubtractBgNoiseFromSpectrogram(matrix, modalNoise);
            mnr = SNR.SetDynamicRange(matrix, 0.0, dynamicRange);
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



        // #############################################################################################################################
        // ################################# NOISE REDUCTION METHODS #################################################################

        /// <summary>
        /// Subtracts the supplied modal noise value for each freq bin AND sets values less than backgroundThreshold to ZERO.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] TruncateModalNoise(double[,] matrix, double[] modalNoise, double backgroundThreshold)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount];          //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++)//for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++)  //for all rows
                {
                    outM[y, col] = matrix[y, col] - modalNoise[col];
                    if (outM[y, col] < backgroundThreshold) outM[y, col] = 0.0;
                }//end for all rows
            }//end for all cols
            return outM;
        }// end of TruncateModalNoise()

        /// <summary>
        /// Subtracts the supplied modal noise value for each freq bin AND sets negative values to ZERO.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] SubtractBgNoiseFromSpectrogramAndTruncate(double[,] matrix, double[] modalNoise)
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
        }// end of TruncateModalNoise()

        /// <summary>
        /// Subtracts the supplied modal noise value for each freq bin BUT DOES NOT set negative values to zero.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] SubtractBgNoiseFromSpectrogram(double[,] matrix, double[] modalNoise)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount, colCount];          //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++)//for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++)  //for all rows
                {
                    outM[y, col] = matrix[y, col] - modalNoise[col];
                }//end for all rows
            }//end for all cols
            return outM;
        }// end of SubtractModalNoise()


        /// <summary>
        /// Calculates the modal noise value for each freq bin.
        /// Does so using a series of overlapped matrices.
        /// TODO!!!! COULD SIMPLY THIS METHOD. JUST CALCULATE MODE FOR EACH FREQ BIN WITHOUT OVERLAP ....
        /// .... AND THEN APPLY MORE SEVERE SMOOTHING TO THE MODAL NOISE PROFILE IN PREVIOUS METHOD.
        /// 
        /// COMPARE THIS METHOD WITH SNR.SubtractModalNoise();
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[] CalculateModalNoise(double[,] matrix)
        {
            //set parameters for noise histograms based on overlapping bands.
            //*******************************************************************************************************************
            int bandWidth = 3;  // should be an odd number
            int binCount = 64;  // number of pixel intensity bins
            double upperLimitForMode = 0.666; // sets upper limit to modal noise bin. Higher values = more severe noise removal.
            int binLimit = (int)(binCount * upperLimitForMode);
            //*******************************************************************************************************************


            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double binWidth = (maxIntensity - minIntensity) / binCount;  // width of an intensity bin
            // Console.WriteLine("minIntensity=" + minIntensity + "  maxIntensity=" + maxIntensity + "  binWidth=" + binWidth);

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            if (bandWidth > colCount) bandWidth = colCount - 1;
            int halfWidth = bandWidth / 2;

            // init matrix from which histogram derived
            double[,] submatrix = DataTools.Submatrix(matrix, 0, 0, rowCount - 1, bandWidth);
            double[] modalNoise = new double[colCount];

            for (int col = 0; col < colCount; col++) // for all cols i.e. freq bins
            {
                // construct new submatrix to calculate modal noise
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
        /// IMPORTANT: this method assumes that the first N frames (N=frameCount) DO NOT contain signal.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="frameCount"></param>
        /// <returns></returns>
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
        /// Calculates the modal value for each column in a matrix.
        /// Matrix is expected to be a spectrogram (transposed) but is quite general.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[] CalculateModalValues(double[,] matrix)
        {
            //set parameters for histograms
            //*******************************************************************************************************************
            int histoBarCount = 100;  // number of pixel intensity bins
            double upperLimitForMode = 0.666; // sets upper limit to modal noise bin. Higher values = more severe noise removal.
            int binLimit = (int)(histoBarCount * upperLimitForMode);
            //*******************************************************************************************************************

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            // init matrix from which histogram derived
            double[] ModalValues = new double[colCount];

            for (int col = 0; col < colCount; col++) // for all cols i.e. freq bins
            {
                double binWidth, min, max;
                double[] column = DataTools.GetColumn(matrix, col);
                int[] histo = DataTools.Histo(column, histoBarCount, out binWidth, out min, out max);
                //DataTools.writeBarGraph(histo);
                double[] smoothHisto = DataTools.filterMovingAverage(histo, 7);
                int maxindex; //mode
                DataTools.getMaxIndex(smoothHisto, out maxindex); //this is mode of histogram
                if (maxindex > binLimit) maxindex = binLimit;
                ModalValues[col] = min + (maxindex * binWidth);
            }//end for all cols
            //DataTools.writeBarGraph(ModalValues);
            return ModalValues;
        }// end of CalculateModalValues(double[,] matrix)



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


        /// <summary>
        /// This method sets a sonogram pixel value = minimum value in sonogram if average pixel value in its neighbourhood is less than min+threshold.
        /// Typically would expect min value in sonogram = zero.
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="threshold">user defined threshold in dB i.e. typically 3-4 dB</param>
        /// <returns></returns>
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
                    //double Xe2 = 0.0;
                    int count = 0;
                    for (int i = r - rNH; i <= (r + rNH); i++)
                    {
                        if (i < 0) continue;
                        if (i >= rows) continue;
                        for (int j = c - cNH; j <= (c + cNH); j++)
                        {
                            if (j < 0) continue;
                            if (j >= cols) continue;
                            count++;           //to accomodate edge effects
                            X += matrix[i, j];
                            //Xe2 += (matrix[i, j] * matrix[i, j]);                 
                            //Console.WriteLine(i+"  "+j+"   count="+count);
                            //Console.ReadLine();
                        }
                    }//end local NH
                    double mean = X / count;
                    //double variance = (Xe2 / count) - (mean * mean);

                    //if ((c<(cols/5))&&(mean < (threshold+1.0))) outM[r, c] = min;
                    //else
                    if (mean < threshold) outM[r, c] = min;
                    else                  outM[r, c] = matrix[r, c];
                    //Console.WriteLine((outM[r, c]).ToString("F1") + "   " + (matrix[r, c]).ToString("F1") + "  mean=" + mean + "  variance=" + variance);
                    //Console.ReadLine();
                }
            }
            return outM;
        }// end RemoveBackgroundNoise()


        public static double[,] SmoothInTemporalDirectionOnly(double[,] matrix, int window)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var smoothMatrix = new double[rows,cols];
            for (int c = 0; c < cols; c++)
            {
                var array = DataTools.GetColumn(matrix, c);
                array = DataTools.filterMovingAverage(array, window);
                DataTools.SetColumn(smoothMatrix, c, array);
            }
            return smoothMatrix; 
        }

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



    }// end class
}
