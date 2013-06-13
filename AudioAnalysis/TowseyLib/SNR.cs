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
        public const double FRACTIONAL_BOUND_FOR_MODE = 0.8; // used when removing noise from decibel waveform

        public struct key_Snr
        {
            public const string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
            public const string key_DYNAMIC_RANGE        = "DYNAMIC_RANGE";
            //public const string Key_SilenceRecording   = "SILENCE_RECORDING_PATH"; //used to determin silence model.
        }


        //reference logEnergies for signal segmentation, energy normalisation etc
        public const double MinLogEnergyReference = -6.0; // = -60dB. Typical noise value for BAC2 recordings = -4.5 = -45dB

        //public const double MaxLogEnergyReference = -0.602;// = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
        //public const double MaxLogEnergyReference = -0.310;// = Math.Log10(0.49) which assumes max average frame amplitude = 0.7
        public const double MaxLogEnergyReference = 0.0; // = Math.Log10(1.00) which assumes max frame amplitude = 1.0

        //note that the cicada recordings reach max average frame amplitude = 0.55



        public double[] LogEnergy { get; set; }

        public double[] Decibels { get; set; }

        public double Min_dB { get; set; }

        public double Max_dB { get; set; }

        public double minEnergyRatio { get; set; }

        public double NoiseSubtracted { get; set; }

        //the modal noise in dB
        public double Snr { get; set; }

        //sig/noise ratio i.e. max dB - modal noise
        public double NoiseRange { get; set; }

        //difference between min_dB and the modal noise dB
        public double MaxReference_dBWrtNoise { get; set; }

        //max reference dB wrt modal noise = 0.0dB. Used for normalisaion
        public double[] ModalNoiseProfile { get; set; }




        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="frames">all the overlapped frames of a signal</param>
        public SNR(double[,] frames)
        {
            this.LogEnergy = SignalLogEnergy(frames);
            this.Decibels = ConvertLogEnergy2Decibels(this.LogEnergy); // convert logEnergy to decibels.
            SubtractBackgroundNoise_dB();
            this.NoiseRange = this.Min_dB - this.NoiseSubtracted;

            // need an appropriate dB reference level for normalising dB arrays.
            ////this.MaxReference_dBWrtNoise = this.Snr;                        // OK
            this.MaxReference_dBWrtNoise = this.Max_dB - this.Min_dB; // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="signal">signal </param>
        /// <param name="frameIDs">starts and end index of each frame</param>
        public SNR(double[] signal, int[,] frameIDs)
        {
            this.LogEnergy = Signal2LogEnergy(signal, frameIDs);
            this.Decibels = ConvertLogEnergy2Decibels(this.LogEnergy); //convert logEnergy to decibels.
            SubtractBackgroundNoise_dB();
            this.NoiseRange = this.Min_dB - this.NoiseSubtracted;
            this.MaxReference_dBWrtNoise = this.Max_dB - this.Min_dB; // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }


        /// <summary>
        /// subtract background noise to produce a decibels array in which zero dB = modal noise
        /// DOES NOT TRUNCATE BELOW ZERO VALUES.
        /// </summary>
        /// <param name="logEnergy"></param>
        /// <returns></returns>
        public void SubtractBackgroundNoise_dB()
        {
            var results = SubtractBackgroundNoiseFromWaveform_dB(this.Decibels);
            this.Decibels = results.DBFrames;
            this.NoiseSubtracted = results.NoiseSd; //Q
            this.Min_dB = results.MinDb; //min decibels of all frames 
            this.Max_dB = results.MaxDb; //max decibels of all frames 
            this.Snr = results.Snr; // = max_dB - Q;
        }


        public double FractionHighEnergyFrames(double dbThreshold)
        {
            return FractionHighEnergyFrames(this.Decibels, dbThreshold);
        }

        public double[] NormaliseDecibelArray_ZeroOne(double maxDecibels)
        {
            return NormaliseDecibelArray_ZeroOne(this.Decibels, maxDecibels);
        }



        //# END CLASS METHODS ####################################################################################################################################
        //########################################################################################################################################################
        //# START STATIC METHODS TO DO WITH CALCULATIONS OF SIGNAL ENERGY AND DECIBELS############################################################################


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
        /// Returns the frame energy of the signal.
        /// The energy of a frame/window is the log of the summed energy of all the samples in the frame.
        /// Normally, if the passed frames are FFT spectra, then would multiply by 2 because spectra are symmetrical about Nyquist.
        /// BUT this method returns the AVERAGE sample energy, which therefore normalises for frame length / sample number. 
        /// <para>
        /// Energy normalisation formula taken from Lecture Notes of Prof. Bryan Pellom
        /// Automatic Speech Recognition: From Theory to Practice.
        /// http://www.cis.hut.fi/Opinnot/T-61.184/ September 27th 2004.
        /// </para>
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
            int N = frameIDs[0, 1] + 1; //window or frame width
            double[] logEnergy = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double sum = 0.0;
                for (int j = 0; j < N; j++) //foreach sample in frame
                {
                    sum += Math.Pow(signal[frameIDs[i, 0] + j], 2); //sum the energy = amplitude squared
                }
                double e = sum / (double)N; //normalise to frame size i.e. average energy per sample
                //LoggedConsole.WriteLine("e=" + e);
                //if (e > 0.25) LoggedConsole.WriteLine("e > 0.25 = " + e);

                if (e == Double.MinValue) //to guard against log(0) but this should never happen!
                {
                    LoggedConsole.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
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
                energy[i] = signal[i] * signal[i]; //energy = amplitude squared
            }
            return energy;
        }

        public static double Amplitude2Decibels(double value)
        {
            return 20 * Math.Log10(value); // 10 times log of amplitude squared

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
                for (int j = 0; j < N; j++) //foreach sample in frame
                {
                    sum += (frames[i, j] * frames[i, j]); //sum the energy = amplitude squared
                }
                double e = sum / (double)N; //normalise to frame size i.e. average energy per sample
                //LoggedConsole.WriteLine("e=" + e);
                //if (e > 0.25) LoggedConsole.WriteLine("e > 0.25 = " + e);

                if (e == Double.MinValue) //to guard against log(0) but this should never happen!
                {
                    LoggedConsole.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
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


        public static double[] ConvertLogEnergy2Decibels(double[] logEnergy)
        {
            var dB = new double[logEnergy.Length];
            for (int i = 0; i < logEnergy.Length; i++) dB[i] = logEnergy[i] * 10; //Convert log energy to decibels.
            return dB;
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
                for (int j = minBin; j <= maxBin; j++) // foreach bin in the bandwidth in frame
                {
                    sum += dBMatrix[i, j]; // sum the dB values
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
            double[,] outSpectro = new double[frameCount,subbandCount];
            int binWidth = N / subbandCount;
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                int startBin;
                int endBin;
                double sum = 0.0;
                for (int j = 0; j < subbandCount - 1; j++) // foreach output band EXCEPT THE LAST
                {
                    startBin = j * binWidth;
                    endBin = startBin + binWidth;
                    for (int b = startBin; b < endBin; b++) // foreach output band
                    {
                        sum += inSpectro[i, b]; // sum the spectral values
                    }
                    outSpectro[i, j] = sum;
                }
                //now do the top most freq band
                startBin = (subbandCount - 1) * binWidth;
                for (int b = startBin; b < N; b++) // foreach output band
                {
                    sum += inSpectro[i, b]; // sum the spectral values
                }
                outSpectro[i, subbandCount - 1] = sum;
            }
            return outSpectro;
        }

        //***********************************************************

        public static List<int[]> SegmentArrayOfIntensityvalues(double[] values, double threshold, int minLength)
        {
            int count = values.Length;
            var events = new List<int[]>();
            bool isHit = false;
            int startID = 0;

            for (int i = 0; i < count; i++) //pass over all elements in array
            {
                if ((isHit == false) && (values[i] > threshold)) //start of an event
                {
                    isHit = true;
                    startID = i;
                }
                else //check for the end of an event
                    if ((isHit == true) && (values[i] <= threshold)) //this is end of an event, so initialise it
                    {
                        isHit = false;
                        int endID = i;
                        int segmentLength = endID - startID + 1;
                        if (segmentLength < minLength) continue; //skip events with duration shorter than threshold
                        var ev = new int[2];
                        ev[0] = startID;
                        ev[1] = endID;
                        events.Add(ev);
                    }
            } //end 
            return events;
        } //end SegmentArrayOfIntensityvalues()




        // #######################################################################################################################################################
        // STATIC METHODS TO DO WITH SUBBAND of a SPECTROGRAM 
        // #######################################################################################################################################################

        /// </summary>
        /// <param name="sonogram">sonogram of signal - values in dB</param>
        /// <param name="minHz">min of freq band to sample</param>
        /// <param name="maxHz">max of freq band to sample</param>
        /// <param name="nyquist">signal nyquist - used to caluclate hz per bin</param>
        /// <param name="smoothDuration">window width (in seconds) to smooth sig intenisty</param>
        /// <param name="framesPerSec">time scale of the sonogram</param>
        /// <returns></returns>
        public static System.Tuple<double[], double, double> SubbandIntensity_NoiseReduced(
            double[,] sonogram, int minHz, int maxHz, int nyquist, double smoothDuration, double framesPerSec)
        {
            //A: CALCULATE THE INTENSITY ARRAY
            double[] intensity = CalculateFreqBandAvIntensity(sonogram, minHz, maxHz, nyquist);
            //B: SMOOTH THE INTENSITY ARRAY
            int smoothWindow = (int)Math.Round(framesPerSec * smoothDuration);
            if ((smoothWindow != 0) && (smoothWindow % 2) == 0) smoothWindow += 1; //Convert to odd number for smoothing
            intensity = DataTools.filterMovingAverage(intensity, smoothWindow);
            //C: REMOVE NOISE FROM INTENSITY ARRAY
            double Q; //modal noise in DB
            double oneSD; //one sd of modal noise in dB
            intensity = SNR.SubtractBackgroundNoiseFromWaveform(intensity, out Q, out oneSD);
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




        //########################################################################################################################################################
        //# START STATIC METHODS TO DO WITH NOISE REDUCTION FROM WAVEFORMS E.g. not spectrograms. See further below for spectrograms #############################
        //########################################################################################################################################################


        /// <summary>
        /// Calculates and subtracts the background noise value from an array of double.
        /// Used for calculating and removing the background noise and setting baseline = zero.
        /// ASSUMES ADDITIVE MODEL with GAUSSIAN NOISE.
        /// Values below zero set equal to zero.
        /// This method can be called for any array of signal values
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Q">The modal value</param>
        /// <param name="oneSD">Standard deviation of the baseline noise</param>
        /// <returns></returns>
        public static double[] SubtractBackgroundNoiseFromWaveform(double[] array, out double Q, out double oneSD)
        {
            int binCount = 100;
            double SDCount = 0.1;
            double min, max, binWidth;
            int[] histo = Histogram.Histo(array, binCount, out min, out max, out binWidth);
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            // this Constant sets an upper limit on the value returned as the modal noise.
            int maxIndex = (int)(binCount * SNR.FRACTIONAL_BOUND_FOR_MODE); // default for bound = 0.8
            int indexOfMode, indexOfOneSD;
            Histogram.GetModeAndOneStandardDeviation(smoothHisto, maxIndex, out indexOfMode, out indexOfOneSD);

            Q = min + ((indexOfMode + 1) * binWidth); //modal noise level
            oneSD = (indexOfMode - indexOfOneSD) * binWidth; // SD of the noise
            double threshold = Q + (oneSD * SDCount);

            // subtract modal noise and return array.
            double[] outArray = SubtractAndTruncate2Zero(array, threshold);
            return outArray;
        }


        /// <summary>
        /// Implements the "Adaptive Level Equalisatsion" algorithm of Lamel et al, 1981 - with modifications for our signals.
        /// Units are assumed to be decibels.
        /// Returns the min and max frame dB AND the estimate MODAL or BACKGROUND noise for the signal array
        /// IF This modal noise is subtracted from each frame dB, the effect is to set set average background noise level = 0 dB.
        /// The algorithm is described in Lamel et al, 1981.
        /// USED TO SEGMENT A RECORDING INTO SILENCE AND VOCALISATION
        /// NOTE: noiseThreshold is passed as decibels. Original algorithm ONLY SEARCHES in range min to 10dB above min.
        /// </summary>
        /// <param name="dBarray">signal in decibel values</param>
        /// <param name="minDecibels">ignore signal values less than minDecibels when calculating background noise. Likely to be spurious
        ///                            This is a safety device because some mobile phone signals had min values.</param>
        /// <param name="noiseThreshold_dB">Sets dB range in which to find value for background noise.</param>
        /// <param name="min_dB"></param>
        /// <param name="max_dB"></param>
        /// <param name="mode_noise">modal or background noise in decibels</param>
        /// <param name="sd_noise">estimated sd of the noies - assuming noise to be guassian</param>
        /// <returns></returns>
        public static void CalculateModalNoise(
            double[] dBarray,
            double minDecibels,
            double noiseThreshold_dB,
            out double min_dB,
            out double max_dB,
            out double mode_noise,
            out double sd_noise)
        {

            int binCount = 100; // number of bins for histogram

            //ignore first N and last N frames when calculating background noise level because 
            // sometimes these frames have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level
            //HOWEVER do not ignore them for short recordings!
            int L = dBarray.Length;
            if (L < 1000) buffer = 0; //ie recording is < approx 11 seconds long

            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            for (int i = buffer; i < L - buffer; i++)
            {
                if (dBarray[i] <= minDecibels) continue; //ignore lowest values when establishing noise level
                if (dBarray[i] < min) min = dBarray[i];
                else if (dBarray[i] > max)
                {
                    max = dBarray[i];
                    //LoggedConsole.WriteLine("max="+max+"    at index "+i);
                }
            }
            min_dB = min; // return out
            max_dB = max;

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
            mode_noise = min_dB + ((peakID + 1) * binWidth); //modal noise level
            sd_noise = (mode_noise - min_dB) / 2.5; //assumes one side of Guassian noise to be 2.5 SD's



            // find peak of lowBins histogram
            // this Constant sets an upper limit on the value returned as the modal noise.
            double FRACTIONAL_BOUND_FOR_MODE = 0.8;
            int maxIndex = (int)(binCount * FRACTIONAL_BOUND_FOR_MODE);
            int indexOfMode, indexOfOneSD;
            Histogram.GetModeAndOneStandardDeviation(smoothHisto, maxIndex, out indexOfMode, out indexOfOneSD);

            mode_noise = min + ((indexOfMode + 1) * binWidth);    // modal noise level
            sd_noise   = (indexOfMode - indexOfOneSD) * binWidth; // SD of the noise
        }



        /// <summary>
        /// Calls method to implement "Adaptive Level Equalisatsion" (Lamel et al, 1981)
        /// Assumes that passed signal is in deciBels.
        /// But differs from Lamel et al because they only search in range 10dB above min dB.
        /// and then subtracts modal noise from the signal - so now zero dB = modal noise
        /// Sets default values for min dB value and the noise threshold. 10 dB is a default used by Lamel et al.
        /// RETURNS: 1) noise reduced decibel array; 2) Q - the modal BG level; 3) min value 4) max value; 5) snr; and 6) SD of the noise
        /// </summary>
        /// <param name="dBarray"></param>
        /// <returns>System.Tuple.Create(decibels, Q, min_dB, max_dB, snr); System.Tuple(double[], double, double, double, double) 
        /// </returns>
        /// 
        public static SubtractedNoise SubtractBackgroundNoiseFromWaveform_dB(double[] dBarray)
        {
            // Following const used to normalise dB values to the background noise. It has the effect of setting background noise level to 0 dB.
            // Value of 10dB is in Lamel et al, 1981. They call it "Adaptive Level Equalisatsion".
            double StandardDeviationCount = 0.1;
            const double noiseThreshold_dB = 10.0; // dB
            double minDecibels = (SNR.MinLogEnergyReference - SNR.MaxLogEnergyReference) * 10; // = -70dB

            double noise_mode, noise_SD;
            double min_dB;
            double max_dB;

            // Implements the algorithm in Lamel et al, 1981.
            SNR.CalculateModalNoise(dBarray, minDecibels, noiseThreshold_dB, out min_dB, out max_dB, out noise_mode, out noise_SD);

            // subtract noise.
            double snr = max_dB - noise_mode;
            double threshold = noise_mode + (noise_SD * StandardDeviationCount);
            double[] dBFrames = SubtractAndTruncate2Zero(dBarray, threshold);
            return new SubtractedNoise(dBFrames, noise_mode, min_dB, max_dB, snr, noise_SD);
        }

        public static double[] SubtractAndTruncate2Zero(double[] inArray, double threshold)
        {
            var outArray = new double[inArray.Length];
            for (int i = 0; i < inArray.Length; i++)
            {
                outArray[i] = inArray[i] - threshold;
                if (outArray[i] < 0.0) outArray[i] = 0.0;
            }
            return outArray;
        }

        public static double[] TruncateNegativeValues2Zero(double[] inArray)
        {
            int L = inArray.Length;
            var outArray = new double[L];
            for (int i = 0; i < L; i++) //foreach row
            {
                if (inArray[i] < 0.0) outArray[i] = 0.0;
                else outArray[i] = inArray[i];
            }
            return outArray;
        }


       
        //########################################################################################################################################################
        //# START STATIC METHODS TO DO WITH CHOICE OF NOISE REDUCTION METHOD FROM SPECTROGRAM ####################################################################
        //########################################################################################################################################################

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
                double[] modalNoise = SNR.CalculateModalNoise(m); //calculate noise profile - assumes a dB spectrogram.
                smoothedArray = DataTools.filterMovingAverage(modalNoise, 7); //smooth the noise profile
                m = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(m, smoothedArray);
                double backgroundThreshold = parameter;
                m = SNR.RemoveNeighbourhoodBackgroundNoise(m, backgroundThreshold);
            }
            else if (nrt == NoiseReductionType.MODAL)
            {
                double[] modalValues = SNR.CalculateModalValues(m); //calculate modal profile - any matrix of values
                smoothedArray = DataTools.filterMovingAverage(modalValues, 7); //smooth the modal profile
                m = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(m, smoothedArray);
            }
            else if (nrt == NoiseReductionType.BINARY)
            {
                double[] modalValues = SNR.CalculateModalValues(m); //calculate modal profile
                smoothedArray = DataTools.filterMovingAverage(modalValues, 7); //smooth the modal profile
                m = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(m, smoothedArray); //remove BG noise
                double backgroundThreshold = parameter;
                m = SNR.RemoveNeighbourhoodBackgroundNoise(m, backgroundThreshold); //smooth background further
                m = DataTools.Matrix2Binary(m, 2 * backgroundThreshold); //convert to binary 
            }
            else if (nrt == NoiseReductionType.FIXED_DYNAMIC_RANGE)
            {
                Log.WriteIfVerbose("\tNoise reduction: FIXED DYNAMIC RANGE = " + parameter);
                m = SNR.NoiseReduce_FixedRange(m, parameter);
            }
            else if (nrt == NoiseReductionType.PEAK_TRACKING)
            {
                Log.WriteIfVerbose("\tNoise reduction: PEAK_TRACKING. Dynamic range= " + parameter);
                m = SNR.NoiseReduce_PeakTracking(m, parameter);
            }
            else if (nrt == NoiseReductionType.HARMONIC_DETECTION)
            {
                m = SNR.NoiseReduce_HarmonicDetection(m);
            }
            var tuple = System.Tuple.Create(m, smoothedArray);
            return tuple;
        }



        /// <summary>
        /// Converts a string interpreted as a key to a NoiseReduction Type.
        /// </summary>
        /// <param name="key">The string to convert.</param>
        /// <returns>A NoiseReductionType enumeration.</returns>
        public static NoiseReductionType Key2NoiseReductionType(string key)
        {
            NoiseReductionType result;
            switch (key)
            {
                case "NONE":
                    result = NoiseReductionType.NONE;
                    break;
                case "STANDARD":
                    result = NoiseReductionType.STANDARD;
                    break;
                case "FIXED_DYNAMIC_RANGE":
                    result = NoiseReductionType.FIXED_DYNAMIC_RANGE;
                    break;
                case "PEAK_TRACKING":
                    result = NoiseReductionType.PEAK_TRACKING;
                    break;
                case "HARMONIC_DETECTION":
                    result = NoiseReductionType.HARMONIC_DETECTION;
                    break;
                default:
                    result = NoiseReductionType.NONE;
                    break;
            }

            return result;
        }



        //########################################################################################################################################################
        //# START STATIC METHODS TO DO WITH NOISE REDUCTION FROM SPECTROGRAMS ####################################################################################
        //########################################################################################################################################################

        

        public static double[,] TruncateNegativeValues2Zero(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var M = new double[rows,cols];
            for (int r = 0; r < rows; r++) //foreach row
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] < 0.0) M[r, c] = 0.0;
                    else M[r, c] = m[r, c];
                }
            }
            return M;
        }



        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_Standard(double[,] matrix)
        {
            //calculate modal noise for each freq bin
            double[] modalNoise = SNR.CalculateModalNoise(matrix); //calculate modal noise profile
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
            double backgroundThreshold = 4.0; //SETS MIN DECIBEL BOUND
            double[,] mnr = matrix;
            mnr = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(mnr, modalNoise);
            mnr = SNR.RemoveNeighbourhoodBackgroundNoise(mnr, backgroundThreshold);
            return mnr;
        }

        public static double[,] NoiseReduce_HarmonicDetection(double[,] matrix)
        {
            double[,] smoothMatrix = MatrixTools.SmoothInTemporalDirectionOnly(matrix, 3); //3=smootihng window
            return NoiseReduce_Standard(smoothMatrix);
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_FixedRange(double[,] matrix, double dynamicRange)
        {
            //calculate modal noise for each freq bin
            double[] modalNoise = SNR.CalculateModalNoise(matrix); //calculate modal noise profile
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

            byte[,] binary = MatrixTools.IdentifySpectralRidges(mnr);
            double[,] op = MatrixTools.SpectralRidges2Intensity(binary, mnr);
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

            double[,] peaks = MatrixTools.IdentifySpectralPeaks(mnr);
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
            double[,] outM = new double[rowCount,colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] - modalNoise[col];
                    if (outM[y, col] < backgroundThreshold) outM[y, col] = 0.0;
                } //end for all rows
            } //end for all cols
            return outM;
        }

        // end of TruncateModalNoise()

        /// <summary>
        /// Subtracts the supplied modal noise value for each freq bin AND sets negative values to ZERO.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] SubtractBgNoiseFromSpectrogramAndTruncate(double[,] matrix, double[] modalNoise)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount,colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] - modalNoise[col];
                    if (outM[y, col] < 0.0) outM[y, col] = 0.0;
                } //end for all rows
            } //end for all cols
            return outM;
        }

        // end of TruncateModalNoise()

        /// <summary>
        /// Subtracts the supplied modal noise value for each freq bin BUT DOES NOT set negative values to zero.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] SubtractBgNoiseFromSpectrogram(double[,] matrix, double[] modalNoise)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount,colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] - modalNoise[col];
                } //end for all rows
            } //end for all cols
            return outM;
        }

        // end of SubtractModalNoise()


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
            int bandWidth = 3; // should be an odd number
            int binCount = 64; // number of pixel intensity bins
            double upperLimitForMode = 0.666;
                // sets upper limit to modal noise bin. Higher values = more severe noise removal.
            int binLimit = (int)(binCount * upperLimitForMode);
            //*******************************************************************************************************************


            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double binWidth = (maxIntensity - minIntensity) / binCount; // width of an intensity bin
            // LoggedConsole.WriteLine("minIntensity=" + minIntensity + "  maxIntensity=" + maxIntensity + "  binWidth=" + binWidth);

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
                int start = col - halfWidth; //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= colCount) stop = colCount - 1;
                submatrix = DataTools.Submatrix(matrix, 0, start, rowCount - 1, stop);
                int[] histo = Histogram.Histo(submatrix, binCount, minIntensity, maxIntensity, binWidth);
                //DataTools.writeBarGraph(histo);
                double[] smoothHisto = DataTools.filterMovingAverage(histo, 7);
                int maxindex; //mode
                DataTools.getMaxIndex(smoothHisto, out maxindex); //this is mode of histogram
                if (maxindex > binLimit) maxindex = binLimit;
                modalNoise[col] = minIntensity + (maxindex * binWidth);
                //LoggedConsole.WriteLine("  modal index=" + maxindex + "  modalIntensity=" + modalIntensity.ToString("F3"));
            } //end for all cols
            return modalNoise;
        }

        // end of CalculateModalNoise(double[,] matrix)

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
                for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
                {
                    modalNoise[col] += matrix[row, col];
                }
            } //end for all cols
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
            int histoBarCount = 500; // number of pixel intensity bins
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
                int[] histo = Histogram.Histo(column, histoBarCount, out binWidth, out min, out max);
                //DataTools.writeBarGraph(histo);
                double[] smoothHisto = DataTools.filterMovingAverage(histo, 7);
                int maxindex; //mode
                DataTools.getMaxIndex(smoothHisto, out maxindex); //this is mode of histogram
                if (maxindex > binLimit) maxindex = binLimit;
                ModalValues[col] = min + (maxindex * binWidth);
            } //end for all cols
            //DataTools.writeBarGraph(ModalValues);
            return ModalValues;
        }

        // end of CalculateModalValues(double[,] matrix)



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
            double[,] normM = new double[rowCount,colCount];
            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int row = 0; row < rowCount; row++) //for all rows
                {
                    normM[row, col] = m[row, col] + shift;
                    if (normM[row, col] < minDB) normM[row, col] = 0;
                }
            }
            return normM;
        }

        //end NormaliseIntensity(double[,] m, double minDB, double maxDB)


        /// <summary>
        /// This method sets a sonogram pixel value = minimum value in sonogram if average pixel value in its neighbourhood is less than min+threshold.
        /// Typically would expect min value in sonogram = zero.
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="threshold">user defined threshold in dB i.e. typically 3-4 dB</param>
        /// <returns></returns>
        public static double[,] RemoveNeighbourhoodBackgroundNoise(double[,] matrix, double threshold)
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
            double[,] outM = new double[rows,cols];
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
                            count++; //to accomodate edge effects
                            X += matrix[i, j];
                            //Xe2 += (matrix[i, j] * matrix[i, j]);                 
                            //LoggedConsole.WriteLine(i+"  "+j+"   count="+count);
                            //Console.ReadLine();
                        }
                    } //end local NH
                    double mean = X / count;
                    //double variance = (Xe2 / count) - (mean * mean);

                    //if ((c<(cols/5))&&(mean < (threshold+1.0))) outM[r, c] = min;
                    //else
                    if (mean < threshold) outM[r, c] = min;
                    else outM[r, c] = matrix[r, c];
                    //LoggedConsole.WriteLine((outM[r, c]).ToString("F1") + "   " + (matrix[r, c]).ToString("F1") + "  mean=" + mean + "  variance=" + variance);
                    //Console.ReadLine();
                }
            }
            return outM;
        } // end RemoveBackgroundNoise()



        public class SubtractedNoise
        {
            public double[] DBFrames { get; set; }

            public double NoiseMode { get; set; }

            public double MinDb { get; set; }

            public double MaxDb { get; set; }

            public double Snr { get; set; }

            public double NoiseSd { get; set; }

            public SubtractedNoise(
                double[] dBFrames, double noiseMode, double min_DB, double max_DB, double snr, double noiseSd)
            {
                this.DBFrames = dBFrames;
                this.NoiseMode = noiseMode;
                this.MinDb = min_DB;
                this.MaxDb = max_DB;
                this.Snr = snr;
                this.NoiseSd = noiseSd;
            }
        } //class SubtractedNoise


        //************************************************************
    } // end class
}
