using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace AudioAnalysisTools.DSP
{

    /// IMPORTANT NOTE: If you are converting Herz to Mel scale, this conversion must be done BEFORE noise reduction


    public enum NoiseReductionType { NONE, STANDARD, MODAL, BINARY, FIXED_DYNAMIC_RANGE, MEAN, MEDIAN, LOWEST_PERCENTILE, BRIGGS_PERCENTILE }


    public class SNR
    {
        public const double FRACTIONAL_BOUND_FOR_MODE = 0.95;          // used when removing modal noise from a signal waveform
        public const double FRACTIONAL_BOUND_FOR_LOW_PERCENTILE = 0.2; // used when removing lowest percentile noise from a signal waveform

        public struct key_Snr
        {
            public const string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
            public const string key_DYNAMIC_RANGE        = "DYNAMIC_RANGE";
        }

        //reference dB levels for different signals
        public const double MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL = -80; // used as minimum bound when normalising dB values. Calculated from actual zero signal.
        public const double MINIMUM_dB_BOUND_FOR_ENVIR_NOISE = -70; // might also be used as minimum bound. Calculated from actual silent environmental recording.

        //reference logEnergies for signal segmentation, energy normalisation etc
        public const double MinLogEnergyReference = -6.0;    // = -60dB. Typical noise value for BAC2 recordings = -4.5 = -45dB
        public const double MaxLogEnergyReference = 0.0;     // = Math.Log10(1.00) which assumes max frame amplitude = 1.0
        //public const double MaxLogEnergyReference = -0.602;// = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
        //public const double MaxLogEnergyReference = -0.310;// = Math.Log10(0.49) which assumes max average frame amplitude = 0.7
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
            double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            var results = SubtractBackgroundNoiseFromWaveform_dB(this.Decibels, StandardDeviationCount);
            this.Decibels = results.NoiseReducedSignal;
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
            double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            BackgroundNoise bgn = SNR.SubtractBackgroundNoiseFromSignal(intensity, StandardDeviationCount);
            var tuple = System.Tuple.Create(bgn.NoiseReducedSignal, bgn.NoiseMode, bgn.NoiseSd);
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
            for (int i = 0; i < frameCount; i++)
            {
                for (int j = minBin; j < maxBin; j++)
                {
                    intensity[i] += sonogram[i, j];
                }

                intensity[i] /= bandCount;
            }
            return intensity;
        }




        // ########################################################################################################################################################
        // # START STATIC METHODS TO DO WITH NOISE REDUCTION FROM WAVEFORMS E.g. not spectrograms. See further below for spectrograms #############################
        // ########################################################################################################################################################


        /// <summary>
        /// Calls method to implement the "Adaptive Level Equalisatsion" algorithm of (Lamel et al, 1981)
        /// It has the effect of setting background noise level to 0 dB.
        /// Passed signal array MUST be in deciBels.
        /// ASSUMES an ADDITIVE MODEL with GAUSSIAN NOISE.
        /// Calculates the average and standard deviation of the noise and then calculates a noise threshold. 
        /// Then subtracts threshold noise from the signal - so now zero dB = threshold noise
        /// Sets default values for min dB value and the noise threshold. 10 dB is a default used by Lamel et al.
        /// RETURNS: 1) noise reduced decibel array; 2) Q - the modal BG level; 3) min value 4) max value; 5) snr; and 6) SD of the noise
        /// </summary>
        /// <param name="dBarray"></param>
        /// <returns>System.Tuple.Create(decibels, Q, min_dB, max_dB, snr); System.Tuple(double[], double, double, double, double) 
        /// </returns>
        /// 
        public static BackgroundNoise SubtractBackgroundNoiseFromWaveform_dB(double[] dBarray, double SD_COUNT)
        {
            double noise_mode, noise_SD;
            double min_dB;
            double max_dB;

            // Implements the algorithm in Lamel et al, 1981.
            NoiseRemoval_Modal.CalculateNoise_LamelsAlgorithm(dBarray, out min_dB, out max_dB, out noise_mode, out noise_SD);

            // subtract noise.
            double threshold = noise_mode + (noise_SD * SD_COUNT);
            double snr = max_dB - threshold;
            double[] dBFrames = SubtractAndTruncate2Zero(dBarray, threshold);
            return new BackgroundNoise()
            {
                NoiseReducedSignal = dBFrames,
                NoiseMode = noise_mode,
                MinDb = min_dB,
                MaxDb = max_dB,
                Snr = snr,
                NoiseSd = noise_SD,
                NoiseThreshold = threshold
            };
        }



        /// <summary>
        /// Calculates and subtracts the background noise value from an array of double.
        /// Used for calculating and removing the background noise and setting baseline = zero.
        /// Implements a MODIFIED version of Lamel et al. They only search in range 10dB above min dB whereas
        /// this method sets upper limit to 66% of range of intensity values.
        /// ASSUMES ADDITIVE MODEL with GAUSSIAN NOISE.
        /// Values below zero set equal to zero.
        /// This method can be called for any array of signal values but is PRESUMED TO BE A WAVEFORM or FREQ BIN OF HISTOGRAM 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static BackgroundNoise SubtractBackgroundNoiseFromSignal(double[] array, double SD_COUNT)
        {
            BackgroundNoise bgn = CalculateModalBackgroundNoiseFromSignal(array, SD_COUNT);         
            bgn.NoiseReducedSignal = SubtractAndTruncate2Zero(array, bgn.NoiseThreshold);
            return bgn;
        }

        public static BackgroundNoise CalculateModalBackgroundNoiseFromSignal(double[] array, double SD_COUNT)
        {
            int binCount = (int)(array.Length / 4); // histogram width is adjusted to length of signal
            if (binCount > 500)
            {
                binCount = 500;
            }
            double min, max, binWidth;
            int[] histo = Histogram.Histo(array, binCount, out binWidth, out min, out max);
            ////Log.WriteLine("BindWidth = "+ binWidth);

            int smoothingwindow = 3;
            if (binCount > 250) smoothingwindow = 5;
            double[] smoothHisto = DataTools.filterMovingAverage(histo, smoothingwindow);
            ////DataTools.writeBarGraph(histo);

            int indexOfMode, indexOfOneSD;
            SNR.GetModeAndOneStandardDeviation(smoothHisto, out indexOfMode, out indexOfOneSD);

            double Q = min + ((indexOfMode + 1) * binWidth); // modal noise level
            double noise_SD = (indexOfMode - indexOfOneSD) * binWidth; // SD of the noise
            double threshold = Q + (noise_SD * SD_COUNT);
            double snr = max - threshold;
            return new BackgroundNoise()
                       {
                           NoiseReducedSignal = null,
                           NoiseMode = Q,
                           MinDb = min,
                           MaxDb = max,
                           Snr = snr,
                           NoiseSd = noise_SD,
                           NoiseThreshold = threshold
                       };
        }

        /// <summary>
        /// This is the important part of Lamel's algorithm.
        /// Assuming the passed histogram represents the values of a waveform in which a signal is added to Gaussian noise,
        /// this method determines the average and one SD of the noise.
        /// </summary>
        /// <param name="histo"></param>
        /// <param name="indexOfMode"></param>
        /// <param name="indexOfOneSD"></param>
        public static void GetModeAndOneStandardDeviation(double[] histo, out int indexOfMode, out int indexOfOneSD)
        {
            // this Constant sets an upper limit on the value returned as the modal noise.
            int upperBoundOfMode = (int)(histo.Length * SNR.FRACTIONAL_BOUND_FOR_MODE); 
            indexOfMode = DataTools.GetMaxIndex(histo);
            if (indexOfMode > upperBoundOfMode) indexOfMode = upperBoundOfMode;

            //calculate SD of the background noise
            double totalAreaUnderLowerCurve = 0.0;
            for (int i = 0; i <= indexOfMode; i++) totalAreaUnderLowerCurve += histo[i];

            indexOfOneSD = indexOfMode;
            double partialSum = 0.0; //sum
            double thresholdSum = totalAreaUnderLowerCurve * 0.68; // 0.68 = area under one standard deviation
            for (int i = indexOfMode; i > 0; i--)
            {
                partialSum += histo[i];
                indexOfOneSD = i;
                if (partialSum > thresholdSum) // we have passed the one SD point
                {
                    break;
                }
            } // for loop
        } // GetModeAndOneStandardDeviation()


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
                case "Modal":
                    result = NoiseReductionType.MODAL;
                    break;
                case "FIXED_DYNAMIC_RANGE":
                    result = NoiseReductionType.FIXED_DYNAMIC_RANGE;
                    break;
                case "Mean":
                    result = NoiseReductionType.MEAN;
                    break;
                case "Median":
                    result = NoiseReductionType.MEDIAN;
                    break;
                case "LowestPercentile":
                    result = NoiseReductionType.LOWEST_PERCENTILE;
                    break;
                case "BriggsPercentile":
                    result = NoiseReductionType.BRIGGS_PERCENTILE;
                    break;
                default:
                    result = NoiseReductionType.NONE;
                    break;
            }

            return result;
        }

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
            double SD_COUNT = 0.0; // number of noise standard deviations included in noise threshold - determines severity of noise reduction.
            // Can be over-ridden by the passed parameter.
            double[] smoothedNoiseProfile = null;
            if (nrt == NoiseReductionType.STANDARD)
            {
                NoiseProfile profile = SNR.CalculateModalNoiseProfile(m, SD_COUNT); //calculate noise profile - assumes a dB spectrogram.
                smoothedNoiseProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7); //smooth the noise profile
                m = SNR.NoiseReduce_Standard(m, smoothedNoiseProfile, parameter); // parameter = nhBackgroundThreshold
            }
            else if (nrt == NoiseReductionType.MODAL)
            {
                SD_COUNT = parameter;
                NoiseProfile profile = SNR.CalculateModalNoiseProfile(m, SD_COUNT); //calculate modal profile - any matrix of values
                smoothedNoiseProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7); //smooth the modal profile
                m = SNR.TruncateBgNoiseFromSpectrogram(m, smoothedNoiseProfile);
            }
            else if (nrt == NoiseReductionType.LOWEST_PERCENTILE) //
            {
                double[] profile = NoiseRemoval_Briggs.GetNoiseProfile_LowestPercentile(m, parameter);
                smoothedNoiseProfile = DataTools.filterMovingAverage(profile, 7); //smooth the modal profile
                m = SNR.TruncateBgNoiseFromSpectrogram(m, smoothedNoiseProfile);
            }
            else if (nrt == NoiseReductionType.BRIGGS_PERCENTILE) //
            {
                m = NoiseRemoval_Briggs.BriggsNoiseFilterTwice(m, parameter); 
            }
            else if (nrt == NoiseReductionType.BINARY)
            {
                NoiseProfile profile = SNR.CalculateModalNoiseProfile(m, SD_COUNT); //calculate noise profile
                smoothedNoiseProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7); //smooth the noise profile
                m = SNR.NoiseReduce_Standard(m, smoothedNoiseProfile, parameter); // parameter = nhBackgroundThreshold
                m = DataTools.Matrix2Binary(m, 2 * parameter);             //convert to binary with backgroundThreshold = 2*parameter
            }
            else if (nrt == NoiseReductionType.FIXED_DYNAMIC_RANGE)
            {
                Log.WriteIfVerbose("\tNoise reduction: FIXED DYNAMIC RANGE = " + parameter); //parameter should have value = 50 dB approx
                m = SNR.NoiseReduce_FixedRange(m, parameter, SD_COUNT);
            }
            else if (nrt == NoiseReductionType.MEAN) 
            {
                Log.WriteIfVerbose("\tNoise reduction: PEAK_TRACKING. Dynamic range= " + parameter);
                m = SNR.NoiseReduce_Mean(m, parameter);
            }
            else if (nrt == NoiseReductionType.MEDIAN)
            {
                Log.WriteIfVerbose("\tNoise reduction: PEAK_TRACKING. Dynamic range= " + parameter);
                m = SNR.NoiseReduce_Median(m, parameter);
            }
            var tuple = System.Tuple.Create(m, smoothedNoiseProfile);
            return tuple;
        }



        //########################################################################################################################################################
        //# START STATIC METHODS TO DO WITH NOISE REDUCTION FROM SPECTROGRAMS ####################################################################################
        //########################################################################################################################################################


        /// <summary>
        /// expects a spectrogram in dB values
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// Uses default values for severity of noise reduction and neighbourhood threshold
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_Standard(double[,] matrix)
        {
            double SD_COUNT = 0.1; // number of noise standard deviations used to calculate noise threshold - determines severity of noise reduction
            double backgroundThreshold = 2.0; //SETS MIN DECIBEL BOUND
            NoiseProfile profile = SNR.CalculateModalNoiseProfile(matrix, SD_COUNT); //calculate modal noise profile            
            double[] smoothedProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7); //smooth the noise profile
            return NoiseReduce_Standard(matrix, smoothedProfile, backgroundThreshold);
        }

        /// <summary>
        /// expects a spectrogram in dB values
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="modalNoise"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_Standard(double[,] matrix, double[] noiseProfile, double nhBackgroundThreshold)
        {
            double[,] mnr = matrix;
            mnr = SNR.TruncateBgNoiseFromSpectrogram(mnr, noiseProfile);
            mnr = SNR.RemoveNeighbourhoodBackgroundNoise(mnr, nhBackgroundThreshold);
            return mnr;
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_FixedRange(double[,] matrix, double dynamicRange, double SD_COUNT)
        {
            NoiseProfile profile = SNR.CalculateModalNoiseProfile(matrix, SD_COUNT); //calculate modal noise profile
            double[] smoothedProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7); //smooth the noise profile
            double[,] mnr = SNR.SubtractBgNoiseFromSpectrogram(matrix, smoothedProfile);
            mnr = SNR.SetDynamicRange(matrix, 0.0, dynamicRange);
            return mnr;
        }

        /// <summary>
        /// The passed matrix is a sonogram with values in dB. wrt 0dB.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] NoiseReduce_Mean(double[,] matrix, double dynamicRange)
        {
            double[,] mnr = matrix;
            int startFrameCount = 9;
            int smoothingWindow = 7;
            double neighbourhoodBackgroundThreshold = 4.0; //SETS MIN DECIBEL BOUND

            double[] modalNoise = NoiseRemoval_Modal.CalculateModalNoiseUsingStartFrames(mnr, startFrameCount);
            modalNoise = DataTools.filterMovingAverage(modalNoise, smoothingWindow); //smooth the noise profile
            mnr = NoiseReduce_Standard(matrix, modalNoise, neighbourhoodBackgroundThreshold);
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);

            byte[,] binary = MatrixTools.IdentifySpectralRidges(mnr);
            double[,] op = MatrixTools.SpectralRidges2Intensity(binary, mnr);
            return op;
        }

        public static double[,] NoiseReduce_Median(double[,] matrix, double dynamicRange)
        {
            double[,] mnr = matrix;
            int startFrameCount = 9;
            int smoothingWindow = 7;
            double neighbourhoodBackgroundThreshold = 4.0; //SETS MIN DECIBEL BOUND

            int NH = 11;
            mnr = ImageTools.WienerFilter(mnr, NH);

            double[] modalNoise = NoiseRemoval_Modal.CalculateModalNoiseUsingStartFrames(mnr, startFrameCount);
            modalNoise = DataTools.filterMovingAverage(modalNoise, smoothingWindow); //smooth the noise profile
            mnr = NoiseReduce_Standard(matrix, modalNoise, neighbourhoodBackgroundThreshold);
            mnr = SNR.SetDynamicRange(mnr, 0.0, dynamicRange);

            double[,] peaks = MatrixTools.IdentifySpectralPeaks(mnr);
            //double[,] outM = SpectralRidges2Intensity(peaks, mnr);
            //return outM;
            return peaks;
        }



        // #############################################################################################################################
        // ################################# NOISE REDUCTION METHODS #################################################################

        /// <summary>
        /// calculates the background noise in a spectrogram
        /// (The spectrogram is rotated so that matrix columns are frequency bins)
        /// i.e. the origin is top-left.
        /// </summary>
        /// <param name="matrix">the rotated spectrogram</param>
        /// <param name="SD_COUNT"></param>
        /// <returns></returns>
        public static NoiseProfile CalculateModalNoiseProfile(double[,] matrix, double SD_COUNT)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[] noiseMode = new double[colCount];
            double[] noiseSD = new double[colCount];
            double[] noiseThreshold = new double[colCount];
            double[] minsOfBins = new double[colCount];
            double[] maxsOfBins = new double[colCount];
            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                double[] freqBin = MatrixTools.GetColumn(matrix, col);
                BackgroundNoise binNoise = SNR.CalculateModalBackgroundNoiseFromSignal(freqBin, SD_COUNT);
                noiseMode[col] = binNoise.NoiseMode;
                noiseSD[col] = binNoise.NoiseSd;
                noiseThreshold[col] = binNoise.NoiseThreshold;
                minsOfBins[col] = binNoise.MinDb;
                maxsOfBins[col] = binNoise.MaxDb;
            }

            var profile = new NoiseProfile()
                              {
                                  NoiseMode = noiseMode,
                                  NoiseSd = noiseSD,
                                  NoiseThresholds = noiseThreshold,
                                  MinDb = minsOfBins,
                                  MaxDb = maxsOfBins
                              };
            return profile;
        }


        /// <summary>
        /// Subtracts the supplied noise profile from spectorgram AND sets values less than backgroundThreshold to ZERO.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] SubtractAndTruncateNoiseProfile(double[,] matrix, double[] noiseProfile, double backgroundThreshold)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount,colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] - noiseProfile[col];
                    if (outM[y, col] < backgroundThreshold) outM[y, col] = 0.0;
                } //end for all rows
            } //end for all cols
            return outM;
        } // end of TruncateModalNoise()

        /// <summary>
        /// Subtracts the supplied noise profile from spectorgram AND sets negative values to ZERO.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] TruncateBgNoiseFromSpectrogram(double[,] matrix, double[] noiseProfile)
        {
            double backgroundThreshold = 0.0;
            return SubtractAndTruncateNoiseProfile(matrix, noiseProfile, backgroundThreshold);
        } // end of TruncateModalNoise()



        /// <summary>
        /// Subtracts the supplied modal noise value for each freq bin BUT DOES NOT set negative values to zero.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] SubtractBgNoiseFromSpectrogram(double[,] matrix, double[] noiseProfile)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] outM = new double[rowCount,colCount]; //to contain noise reduced matrix

            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    outM[y, col] = matrix[y, col] - noiseProfile[col];
                } //end for all rows
            } //end for all cols
            return outM;
        }  // end of SubtractModalNoise()



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
                    else M[r, c] = m[r, c];
                }
            }
            return M;
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
        } //end NormaliseIntensity(double[,] m, double minDB, double maxDB)


        /// <summary>
        /// This method sets a sonogram pixel value = minimum value in sonogram if average pixel value in its neighbourhood is less than min+threshold.
        /// Typically would expect min value in sonogram = zero.
        /// </summary>
        /// <param name="matrix">the sonogram</param>
        /// <param name="threshold">user defined threshold in dB i.e. typically 3-4 dB</param>
        /// <returns></returns>
        public static double[,] RemoveNeighbourhoodBackgroundNoise(double[,] matrix, double nhThreshold)
        {
            int M = 3; // each row is a frame or time instance
            int N = 9; // each column is a frequency bin
            int rNH = M / 2;
            int cNH = N / 2;

            double min;
            double max;
            DataTools.MinMax(matrix, out min, out max);
            nhThreshold += min;
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
                    if (mean < nhThreshold) outM[r, c] = min;
                    else outM[r, c] = matrix[r, c];
                    //LoggedConsole.WriteLine((outM[r, c]).ToString("F1") + "   " + (matrix[r, c]).ToString("F1") + "  mean=" + mean + "  variance=" + variance);
                    //Console.ReadLine();
                }
            }
            return outM;
        } // end RemoveBackgroundNoise()







        /// <summary>
        /// THIS METHOD IS JUST A CONTAINER FOR TESTING SNIPPETS OF CODE TO DO WITH NOISE REMOVAL FROM SPECTROGRAMS
        /// CUT and PASTE these snippets into the SANDPIT class.;
        /// the following libraries are required to run these tests. They are in the SANDPIT class
        /// using System.Drawing;
        /// using System.Drawing.Imaging;
        /// using System.IO;
        /// using AudioAnalysisTools;
        /// </summary>
        public static void SandPit()
        {
            //#######################################################################################################################################
            // experiments with noise reduction of spectrograms
            //THE FOLLOWING CODE tests the use of the Noise Reduction types listed in the enum SNR.NoiseReductionType
            //The enum types include NONE, STANDARD, MODAL, Binary, etc.
            //COPY THE FOLLOWING CODE INTO THE CLASS Sandpit.cs to do the testing.
            if (true)
            {
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                string wavFilePath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
                string outputDir = @"C:\SensorNetworks\Output\Test";
                string imageFname = "test3.png";
                //string imagePath = Path.Combine(outputDir, imageFname);
                //string imageViewer = @"C:\Windows\system32\mspaint.exe";

                //var recording = new AudioRecording(wavFilePath);
                //var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.0 };
                //config.NoiseReductionParameter = 0.0; // backgroundNeighbourhood noise reduction in dB
                //var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                //Plot scores = null;
                //double eventThreshold = 0.5; // dummy variable - not used
                //Image image = DrawSonogram(spectrogram, scores, null, eventThreshold);
                //image.Save(imagePath, ImageFormat.Png);                
                //FileInfo fiImage = new FileInfo(imagePath);
                //if (fiImage.Exists) // Display the image using MsPaint.exe
                //{   
                //    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                //    process.Run(imagePath, outputDir);
                //}
            } // if(true)


            //#######################################################################################################################################
            //THE FOLLOWING CODE tests the effect of changing the order of 1) CONVERT TO dB 2) NOISE REMOVAL
            //                                                     versus  1) NOISE REMOVAL 2) CONVERT TO dB.
            //THe results are very different. The former is GOOD. The latter is A MESS.
            if (true)
            {
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                string wavFilePath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
                string outputDir = @"C:\SensorNetworks\Output\Test";
                string imageFname = "test3.png";
                //string imagePath = Path.Combine(outputDir, imageFname);
                //string imageViewer = @"C:\Windows\system32\mspaint.exe";

                //var recording = new AudioRecording(wavFilePath);
                //int frameSize = 512;
                //double windowOverlap = 0.0;
                //// i: EXTRACT ENVELOPE and FFTs
                //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);

                //// get amplitude spectrogram and remove the DC column ie column zero.
                //double[,] spectrogramData = results2.Spectrogram;
                //spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, spectrogramData.GetLength(1) - 1);
                //double epsilon = Math.Pow(0.5, 16 - 1);
                //double windowPower = frameSize * 0.66; //power of a rectangular window =frameSize. Hanning is less

                //// convert spectrum to decibels BEFORE noise removal
                ////spectrogramData = Speech.DecibelSpectra(spectrogramData, windowPower, recording.SampleRate, epsilon);

                //// vi: remove background noise from the spectrogram
                //double SD_COUNT = 0.1;
                //double SpectralBgThreshold = 0.003; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
                //SNR.NoiseProfile profile = SNR.CalculateNoiseProfile(spectrogramData, SD_COUNT); //calculate noise profile - assumes a dB spectrogram.
                //double[] noiseValues = DataTools.filterMovingAverage(profile.noiseThreshold, 7);      // smooth the noise profile
                //spectrogramData = SNR.NoiseReduce_Standard(spectrogramData, noiseValues, SpectralBgThreshold);

                //// convert spectrum to decibels AFTER noise removal
                ////spectrogramData = Speech.DecibelSpectra(spectrogramData, windowPower, recording.SampleRate, epsilon);

                //spectrogramData = MatrixTools.MatrixRotate90Anticlockwise(spectrogramData);
                //ImageTools.DrawMatrix(spectrogramData, imagePath);
                //FileInfo fiImage = new FileInfo(imagePath);
                //if (fiImage.Exists) // Display the image using MsPaint.exe
                //{
                //    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                //    process.Run(imagePath, outputDir);
                //}
            } // if(true)
        } // sandpit



        public class BackgroundNoise
        {
            public double[] NoiseReducedSignal { get; set; }

            public double NoiseMode { get; set; }

            public double NoiseSd { get; set; }

            public double NoiseThreshold { get; set; }

            public double MinDb { get; set; }

            public double MaxDb { get; set; }

            public double Snr { get; set; }
        }

        /// <summary>
        /// contains info re noise profile of an entire spectrogram
        /// </summary>
        public class NoiseProfile 
        {
            public double[] NoiseMode { get; set; }

            public double[] NoiseSd { get; set; }

            public double[] NoiseThresholds { get; set; }

            public double[] MinDb { get; set; }

            public double[] MaxDb { get; set; }
        }
    }
}
