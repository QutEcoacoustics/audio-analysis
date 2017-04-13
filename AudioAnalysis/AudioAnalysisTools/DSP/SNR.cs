// <copyright file="SNR.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using StandardSpectrograms;
    using WavTools;
    using TowseyLibrary;

    // IMPORTANT NOTE: If you are converting Herz to Mel scale, this conversion must be done BEFORE noise reduction

    public enum NoiseReductionType { None, Standard, Modal, Binary, FixedDynamicRange, Mean, Median, LowestPercentile, BriggsPercentile, ShortRecording, FlattenAndTrim }

    public class SNR
    {
        public const double FRACTIONAL_BOUND_FOR_MODE = 0.95;          // used when removing modal noise from a signal waveform
        public const double FRACTIONAL_BOUND_FOR_LOW_PERCENTILE = 0.2; // used when removing lowest percentile noise from a signal waveform

                                                                       //reference dB levels for different signals
        public const double MinimumDbBoundForZeroSignal = -80; // used as minimum bound when normalising dB values. Calculated from actual zero signal.
        public const double MinimumDbBoundForEnvirNoise = -70; // might also be used as minimum bound. Calculated from actual silent environmental recording.

        //reference logEnergies for signal segmentation, energy normalisation etc
        public const double MinLogEnergyReference = -6.0;    // = -60dB. Typical noise value for BAC2 recordings = -4.5 = -45dB
        public const double MaxLogEnergyReference = 0.0;     // = Math.Log10(1.00) which assumes max frame amplitude = 1.0
        //public const double MaxLogEnergyReference = -0.602;// = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
        //public const double MaxLogEnergyReference = -0.310;// = Math.Log10(0.49) which assumes max average frame amplitude = 0.7
        //note that the cicada recordings reach max average frame amplitude = 0.55

        // number of noise standard deviations included in noise threshold - determines severity of noise reduction.
        public const double DefaultStddevCount = 0.0;

        //SETS MINIMUM DECIBEL BOUND when removing local backgroundnoise
        public const double DefaultNhBgThreshold = 2.0;

        public struct KeySnr
        {
            public const string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
            public const string key_DYNAMIC_RANGE = "DYNAMIC_RANGE";
        }

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
        /// Initializes a new instance of the <see cref="SNR"/> class.
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="frames">all the overlapped frames of a signal</param>
        public SNR(double[,] frames)
        {
            this.LogEnergy = SignalLogEnergy(frames);
            this.Decibels = ConvertLogEnergy2Decibels(this.LogEnergy); // convert logEnergy to decibels.
            this.SubtractBackgroundNoise_dB();
            this.NoiseRange = this.Min_dB - this.NoiseSubtracted;

            // need an appropriate dB reference level for normalising dB arrays.
            ////this.MaxReference_dBWrtNoise = this.Snr;                        // OK
            this.MaxReference_dBWrtNoise = this.Max_dB - this.Min_dB; // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SNR"/> class.
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="signal">signal </param>
        /// <param name="frameIDs">the start and end index of every frame</param>
        public SNR(double[] signal, int[,] frameIDs)
        {
            this.LogEnergy = Signal2LogEnergy(signal, frameIDs);
            this.Decibels = ConvertLogEnergy2Decibels(this.LogEnergy); //convert logEnergy to decibels.
            this.SubtractBackgroundNoise_dB();
            this.NoiseRange = this.Min_dB - this.NoiseSubtracted;
            this.MaxReference_dBWrtNoise = this.Max_dB - this.Min_dB; // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }

        /// <summary>
        /// subtract background noise to produce a decibels array in which zero dB = modal noise
        /// DOES NOT TRUNCATE BELOW ZERO VALUES.
        /// </summary>
        public void SubtractBackgroundNoise_dB()
        {
            var results = SubtractBackgroundNoiseFromWaveform_dB(this.Decibels, DefaultStddevCount);
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

                if (e == double.MinValue) //to guard against log(0) but this should never happen!
                {
                    LoggedConsole.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
                    logEnergy[i] = MinLogEnergyReference - MaxLogEnergyReference; //normalise to absolute scale
                    continue;
                }

                double logE = Math.Log10(e);

                //normalise to ABSOLUTE energy value i.e. as defined in header of Sonogram class
                if (logE < MinLogEnergyReference)
                {
                    logEnergy[i] = MinLogEnergyReference - MaxLogEnergyReference;
                }
                else logEnergy[i] = logE - MaxLogEnergyReference;
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

                if (e == double.MinValue) //to guard against log(0) but this should never happen!
                {
                    LoggedConsole.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
                    logEnergy[i] = MinLogEnergyReference - MaxLogEnergyReference; //normalise to absolute scale
                    continue;
                }
                double logE = Math.Log10(e);

                //normalise to ABSOLUTE energy value i.e. as defined in header of Sonogram class
                if (logE < MinLogEnergyReference)
                {
                    logEnergy[i] = MinLogEnergyReference - MaxLogEnergyReference;
                }
                else logEnergy[i] = logE - MaxLogEnergyReference;
            }

            return logEnergy;
        }

        public static double[] ConvertLogEnergy2Decibels(double[] logEnergy)
        {
            var dB = new double[logEnergy.Length];
            for (int i = 0; i < logEnergy.Length; i++)
            {
                dB[i] = logEnergy[i] * 10; //Convert log energy to decibels.
            }

            return dB;
        }

        public static double[] DecibelsInSubband(double[,] dBMatrix, int minHz, int maxHz, double freqBinWidth)
        {
            int frameCount = dBMatrix.GetLength(0);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            double[] db = new double[frameCount];

            // foreach frame
            for (int i = 0; i < frameCount; i++)
            {
                // foreach bin in the bandwidth in frame
                double sum = 0.0;
                for (int j = minBin; j <= maxBin; j++)
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
            }

            return events;
        } //end SegmentArrayOfIntensityvalues()

        // #######################################################################################################################################################
        // STATIC METHODS TO DO WITH SUBBAND of a SPECTROGRAM
        // #######################################################################################################################################################

        /// <param name="sonogram">sonogram of signal - values in dB</param>
        /// <param name="minHz">min of freq band to sample</param>
        /// <param name="maxHz">max of freq band to sample</param>
        /// <param name="nyquist">signal nyquist - used to caluclate hz per bin</param>
        /// <param name="smoothDuration">window width (in seconds) to smooth sig intenisty</param>
        /// <param name="framesPerSec">time scale of the sonogram</param>
        public static Tuple<double[], double, double> SubbandIntensity_NoiseReduced(
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
            BackgroundNoise bgn = SubtractBackgroundNoiseFromSignal(intensity, StandardDeviationCount);
            var tuple = Tuple.Create(bgn.NoiseReducedSignal, bgn.NoiseMode, bgn.NoiseSd);
            return tuple;
        }

        /// <summary>
        /// Calculates the mean intensity in a freq band defined by its min and max freq.
        /// </summary>
        public static double[] CalculateFreqBandAvIntensity(double[,] sonogram, int minHz, int maxHz, int nyquist)
        {
            int frameCount = sonogram.GetLength(0);
            int binCount = sonogram.GetLength(1);
            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);
            int binCountInBand = maxBin - minBin + 1;
            double[] intensity = new double[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                for (int j = minBin; j < maxBin; j++)
                {
                    intensity[i] += sonogram[i, j];
                }

                intensity[i] /= binCountInBand;
            }

            return intensity;
        }

        // ########################################################################################################################################################
        // # NEXT FOUR METHODS USED TO CALCULATE SNR OF SHORT RECORDINGS  #########################################################################################
        // # INFO USED FOR XUEYAN QUERIES.                                #########################################################################################
        // ########################################################################################################################################################

        /// <summary>
        /// The calculation of SNR in this method assumes that background noise has already been removed.
        /// That is, the maximum value is with respect to zero.
        /// SNR should be calculated based on power values
        ///     i.e. SNR = 10log(PowerOfSignal / PowerOfNoise);
        ///     or   SNR = 20log(Signal amplitude) - 20log(Noise amplitude);
        ///     If the passed sonogram data is amplitude or energy values (rather than decibel values) then the returned SNR value needs to be appropriately corrected.
        /// </summary>
        public static SnrStatistics CalculateSNRInFreqBand(double[,] sonogramData, int startframe, int frameSpan, int minBin, int maxBin, double threshold)
        {
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            double[,] bandSonogram = MatrixTools.Submatrix(sonogramData, 0, minBin, frameCount - 1, maxBin);

            // estimate low energy content independently for each freq bin.
            // estimate from the lowest quintile (20%) of frames in the bin.
            int lowEnergyFrameCount = frameCount / 5;
            int binCountInBand = maxBin - minBin + 1;

            double[,] callMatrix = new double[frameSpan, binCountInBand];

            // loop over all freq bins in the band
            for (int bin = 0; bin < binCountInBand; bin++)
            {
                double[] freqBin = MatrixTools.GetColumn(bandSonogram, bin);
                double[] orderedArray = (double[])freqBin.Clone();
                Array.Sort(orderedArray);

                double sum = 0.0;
                for (int i = 0; i < lowEnergyFrameCount; i++)
                {
                    sum += orderedArray[i];
                }

                double bgnEnergyInBin = sum / (double)lowEnergyFrameCount;

                // NOW get the required time frame
                double[] callBin = DataTools.Subarray(freqBin, startframe, frameSpan);

                // subtract the background noise
                for (int i = 0; i < callBin.Length; i++)
                {
                    callBin[i] -= bgnEnergyInBin;
                }

                MatrixTools.SetColumn(callMatrix, bin, callBin);
            }

            // now calculate SNR from the call matrix
            double snr = 0.0;
            for (int frame = 0; frame < frameSpan; frame++)
            {
                for (int bin = 0; bin < binCountInBand; bin++)
                {
                    if (callMatrix[frame, bin] > snr) snr = callMatrix[frame, bin];
                }
            }

            // now calculate % of frames having high energy.
            // only count cells which actually have activity
            double[] frameAverages = new double[frameSpan];
            for (int frame = 0; frame < frameSpan; frame++)
            {
                int count = 0;
                double sum = 0.0;
                for (int bin = 0; bin < binCountInBand; bin++)
                {
                    if (callMatrix[frame, bin] > 0.0)
                    {
                        count++;
                        sum += callMatrix[frame, bin];
                    }
                }
                frameAverages[frame] = sum / (double)count;
            }

            // count the number of spectrogram frames where the energy exceeds the threshold
            double thirdSNR = snr * 0.3333;
            int framesExceedingThreshold = 0;
            int framesExceedingThirdSNR = 0;
            for (int frame = 0; frame < frameSpan; frame++)
            {

                if (frameAverages[frame] > threshold)
                {
                    framesExceedingThreshold++;
                }

                if (frameAverages[frame] > thirdSNR)
                {
                    framesExceedingThirdSNR++;
                }
            }

            var stats = new SnrStatistics
            {
                Threshold = threshold,
                Snr = snr,
                FractionOfFramesExceedingThreshold = framesExceedingThreshold / (double) frameSpan,
                FractionOfFramesExceedingOneThirdSnr = framesExceedingThirdSNR / (double) frameSpan
            };

            return stats;
        }

        /// <summary>
        /// Calculates the matrix row/column bounds given the real world bounds.
        /// Axis scales are obtained form the passed sonogram instance.
        /// </summary>
        public static SnrStatistics CalculateSNRInFreqBand(BaseSonogram sonogram, TimeSpan startTime, TimeSpan extractDuration, int minHz, int maxHz, double threshold)
        {
            // calculate temporal bounds
            int frameCount = sonogram.Data.GetLength(0);
            double frameDuration = sonogram.FrameDuration;

            //take a bit extra afound the given temporal bounds
            int bufferFrames = (int)Math.Round(0.25 / frameDuration);

            // calculate temporal bounds
            int startFrame = (int)Math.Round(startTime.TotalSeconds / frameDuration) - bufferFrames;
            int frameSpan = (int)Math.Round(extractDuration.TotalSeconds / frameDuration) + bufferFrames;
            if (startFrame < 0) startFrame = 0;
            int endframe = startFrame + frameSpan;
            if (endframe >= frameCount)
            {
                frameSpan = frameSpan - (endframe - frameCount) - 1;
            }

            // calculate frequency bounds
            int binCount = sonogram.Data.GetLength(1);
            double binWidth = sonogram.NyquistFrequency / (double)binCount;
            int bufferBins = (int)Math.Round(500 / binWidth);

            int lowFreqBin = (int)Math.Round(minHz / binWidth) - bufferBins;
            int hiFreqBin = (int)Math.Round(maxHz / binWidth) + bufferBins;
            if (lowFreqBin < 0) lowFreqBin = 0;
            if (hiFreqBin >= binCount) hiFreqBin = binCount - 1;

            SnrStatistics stats = CalculateSNRInFreqBand(sonogram.Data, startFrame, frameSpan, lowFreqBin, hiFreqBin, threshold);
            stats.ExtractDuration = sonogram.Duration;
            if (extractDuration < sonogram.Duration) stats.ExtractDuration = extractDuration;

            return stats;
        }

        /// <summary>
        /// This method written 18-09-2014 to process Xueyan's query recordings.
        /// Calculate the SNR statistics for each recording and then write info back to csv file
        /// </summary>
        public static SnrStatistics Calculate_SNR_ShortRecording(FileInfo sourceRecording, Dictionary<string, string> configDict, TimeSpan start, TimeSpan duration, int minHz, int maxHz, double threshold)
        {
            configDict["NoiseReductionType"] = "None";

            // 1) get recording
            AudioRecording recordingSegment = new AudioRecording(sourceRecording.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(configDict); // default values config

            // 2) get decibel spectrogram
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            // remove the DC column
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);

            return CalculateSNRInFreqBand(sonogram, start, duration, minHz, maxHz, threshold);
        }

        /// <summary>
        /// This method written 18-09-2014 to process Xueyan's query recordings.
        /// Calculate the SNR statistics for each recording and then write info back to csv file
        /// </summary>
        public static void Calculate_SNR_ofXueyans_data()
        {
            FileInfo configFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo();
            // csv file containing recording info, call bounds etc
            FileInfo csvFileInfo = @"C:\SensorNetworks\WavFiles\XueyanQueryCalls\CallBoundsForXueyanDataSet_19thSept2014.csv".ToFileInfo();
            var sourceDir = csvFileInfo.Directory;
            var opDir     = csvFileInfo.Directory;

            dynamic configuration = Yaml.Deserialise(configFile);
            var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);

            //set up text for the output file
            var opText = new List<string>();

            // set a decibel threshold for determining energy distribution in call
            double threshold = 9.0;

            string strLine;
            try
            {
                FileStream aFile = new FileStream(csvFileInfo.FullName, FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                // read the header
                strLine = sr.ReadLine();
                opText.Add(strLine + ",Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTHalfSNR");
                while ((strLine = sr.ReadLine()) != null)
                {
                    //    // cannot use next line because column headers contain illegal characters
                    //    //var data = Csv.ReadFromCsv<string[]>(csvInfo).ToList();
                    //    List<string> rows = FileTools.ReadTextFile(csvFile.FullName);

                    //    // remove trailing commas, spaces etc
                    //    for (int i = 0; i < rows.Count; i++)
                    //    {
                    //        if (rows[i].Length == 0) continue;
                    //        while ((rows[i].EndsWith(",")) || (rows[i].EndsWith(" ")))
                    //        {
                    //            rows[i] = rows[i].Substring(0, rows[i].Length - 1);
                    //        }
                    //    }

                    // split and parse elements of data line
                    var line = strLine.Split(',');
                    string filename = line[0];
                    int minHz = int.Parse(line[1]);
                    int maxHz = int.Parse(line[2]);
                    TimeSpan start    = TimeSpan.FromSeconds(1.0);
                    TimeSpan duration = TimeSpan.FromSeconds(double.Parse(line[5]));

                    FileInfo sourceRecording = Path.Combine(sourceDir.FullName, filename).ToFileInfo();

                    if (sourceRecording.Exists)
                    {
                        SnrStatistics stats = Calculate_SNR_ShortRecording(sourceRecording, configDict, start, duration, minHz, maxHz, threshold);
                        opText.Add(string.Format(strLine + ",{0},{1},{2},{3}", stats.Threshold, stats.Snr, stats.FractionOfFramesExceedingThreshold, stats.FractionOfFramesExceedingOneThirdSnr));
                    }
                    else
                    {
                        opText.Add(string.Format(strLine + ", ######### WARNING: FILE DOES NOT EXIST >>>" + sourceRecording.Name + "<<<"));
                    }
                }

                sr.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("An IO exception has been thrown!");
                Console.WriteLine(e.ToString());
                return;
            }

            string path = Path.Combine(opDir.FullName, "SNRDataFromMichaelForXueyan_19thSept2014.csv");
            FileTools.WriteTextFile(path, opText, true);
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
        ///
        public static BackgroundNoise SubtractBackgroundNoiseFromWaveform_dB(double[] dBarray, double SD_COUNT)
        {
            double noise_mode, noise_SD;
            double min_dB;
            double max_dB;

            // Implements the algorithm in Lamel et al, 1981.
            NoiseRemovalModal.CalculateNoise_LamelsAlgorithm(dBarray, out min_dB, out max_dB, out noise_mode, out noise_SD);

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
                NoiseThreshold = threshold,
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
        public static BackgroundNoise SubtractBackgroundNoiseFromSignal(double[] array, double SD_COUNT)
        {
            BackgroundNoise bgn = CalculateModalBackgroundNoiseFromSignal(array, SD_COUNT);
            bgn.NoiseReducedSignal = SubtractAndTruncate2Zero(array, bgn.NoiseThreshold);
            return bgn;
        }

        public static BackgroundNoise CalculateModalBackgroundNoiseFromSignal(double[] array, double sdCount)
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

            int indexOfMode, indexOfOneStdDev;
            GetModeAndOneStandardDeviation(smoothHisto, out indexOfMode, out indexOfOneStdDev);

            double Q = min + ((indexOfMode + 1) * binWidth); // modal noise level
            double noiseSd = (indexOfMode - indexOfOneStdDev) * binWidth; // SD of the noise
            // check for noiseSd = zero which can cause possible division by zero later on
            if (indexOfMode == indexOfOneStdDev) noiseSd = binWidth;
            double threshold = Q + (noiseSd * sdCount);
            double snr = max - threshold;
            return new BackgroundNoise()
                       {
                           NoiseReducedSignal = null,
                           NoiseMode = Q,
                           MinDb = min,
                           MaxDb = max,
                           Snr = snr,
                           NoiseSd = noiseSd,
                           NoiseThreshold = threshold,
                       };
        }

        /// <summary>
        /// This is the important part of Lamel's algorithm.
        /// Assuming the passed histogram represents the values of a waveform in which a signal is added to Gaussian noise,
        /// this method determines the average and one SD of the noise.
        /// </summary>
        public static void GetModeAndOneStandardDeviation(double[] histo, out int indexOfMode, out int indexOfOneSD)
        {
            // this Constant sets an upper limit on the value returned as the modal noise.
            int upperBoundOfMode = (int)(histo.Length * FRACTIONAL_BOUND_FOR_MODE);
            indexOfMode = DataTools.GetMaxIndex(histo);
            if (indexOfMode > upperBoundOfMode)
            {
                indexOfMode = upperBoundOfMode;
            }

            //calculate SD of the background noise
            double totalAreaUnderLowerCurve = 0.0;
            for (int i = 0; i <= indexOfMode; i++)
            {
                totalAreaUnderLowerCurve += histo[i];
            }

            indexOfOneSD = indexOfMode;
            double partialSum = 0.0; //sum
            double thresholdSum = totalAreaUnderLowerCurve * 0.68; // 0.68 = area under one standard deviation
            for (int i = indexOfMode; i > 0; i--)
            {
                partialSum += histo[i];
                indexOfOneSD = i;
                if (partialSum > thresholdSum)
                {
                    // we have passed the one SD point
                    break;
                }
            }
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

            // foreach row
            for (int i = 0; i < L; i++)
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
        public static NoiseReductionType KeyToNoiseReductionType(string key)
        {
            NoiseReductionType nrt;
            Enum.TryParse(key, true, out nrt);

            return nrt;
        }

        /// <summary>
        /// Removes noise from a spectrogram. Choice of methods.
        /// Make sure that do MelScale reduction BEFORE applying noise filter.
        /// </summary>
        public static Tuple<double[,], double[]> NoiseReduce(double[,] m, NoiseReductionType nrt, double parameter)
        {
            double[] bgNoiseProfile = null;
            switch (nrt)
            {
                case NoiseReductionType.Standard:
                    {
                        //calculate noise profile - assumes a dB spectrogram.
                        var profile = NoiseProfile.CalculateModalNoiseProfile(m, DefaultStddevCount);

                        // smooth the noise profile
                        bgNoiseProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 5);

                        // parameter = nhBackgroundThreshold
                        m = NoiseReduce_Standard(m, bgNoiseProfile, parameter); // parameter = nhBackgroundThreshold
                    }
                    break;
                case NoiseReductionType.Modal:
                    {
                        double SD_COUNT = parameter;
                        var profile = NoiseProfile.CalculateModalNoiseProfile(m, SD_COUNT); //calculate modal profile - any matrix of values
                        bgNoiseProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 5); //smooth the modal profile
                        m = TruncateBgNoiseFromSpectrogram(m, bgNoiseProfile);
                    }
                    break;
                case NoiseReductionType.LowestPercentile:
                    {
                        bgNoiseProfile = NoiseProfile.GetNoiseProfile_fromLowestPercentileFrames(m, (int)parameter);
                        bgNoiseProfile = DataTools.filterMovingAverage(bgNoiseProfile, 5); //smooth the modal profile
                        m = TruncateBgNoiseFromSpectrogram(m, bgNoiseProfile);
                    }
                    break;
                case NoiseReductionType.ShortRecording:
                    {
                        bgNoiseProfile = NoiseProfile.GetNoiseProfile_BinWiseFromLowestPercentileCells(m, (int)parameter);
                        bgNoiseProfile = DataTools.filterMovingAverage(bgNoiseProfile, 5); //smooth the modal profile
                        m = TruncateBgNoiseFromSpectrogram(m, bgNoiseProfile);
                    }
                    break;
                case NoiseReductionType.BriggsPercentile:
                    // Briggs filters twice
                    m = NoiseRemoval_Briggs.NoiseReduction_byDivisionAndSqrRoot(m, (int)parameter);
                    m = NoiseRemoval_Briggs.NoiseReduction_byDivisionAndSqrRoot(m, (int)parameter);
                    break;
                case NoiseReductionType.Binary:
                    {
                        NoiseProfile profile = NoiseProfile.CalculateModalNoiseProfile(m, DefaultStddevCount); //calculate noise profile
                        bgNoiseProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7); //smooth the noise profile
                        m = NoiseReduce_Standard(m, bgNoiseProfile, parameter); // parameter = nhBackgroundThreshold
                        m = DataTools.Matrix2Binary(m, 2 * parameter);             //convert to binary with backgroundThreshold = 2*parameter
                    }
                    break;
                case NoiseReductionType.FixedDynamicRange:
                    Log.WriteIfVerbose("\tNoise reduction: FIXED DYNAMIC RANGE = " + parameter); //parameter should have value = 50 dB approx
                    m = NoiseReduce_FixedRange(m, parameter, DefaultStddevCount);
                    break;
                case NoiseReductionType.FlattenAndTrim:
                    Log.WriteIfVerbose("\tNoise reduction: FLATTEN & TRIM: StdDev Count=" + parameter);
                    m = NoiseReduce_FlattenAndTrim(m, parameter);
                    break;
                case NoiseReductionType.Mean:
                    Log.WriteIfVerbose("\tNoise reduction: PEAK_TRACKING. Dynamic range= " + parameter);
                    m = NoiseReduce_Mean(m, parameter);
                    break;
                case NoiseReductionType.Median:
                    Log.WriteIfVerbose("\tNoise reduction: PEAK_TRACKING. Dynamic range= " + parameter);
                    m = NoiseReduce_Median(m, parameter);
                    break;
                case NoiseReductionType.None:
                default:
                    Log.WriteIfVerbose("No noise reduction applied");
                    break;
            }

            return Tuple.Create(m, bgNoiseProfile);
        }

        //########################################################################################################################################################
        //# START STATIC METHODS TO DO WITH NOISE REDUCTION FROM SPECTROGRAMS ####################################################################################
        //########################################################################################################################################################

        /// <summary>
        /// expects a spectrogram in dB values
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// Uses default values for severity of noise reduction and neighbourhood threshold
        /// </summary>
        public static double[,] NoiseReduce_Standard(double[,] matrix)
        {
            //SETS MIN DECIBEL BOUND
            double nhBackgroundThreshold = DefaultNhBgThreshold;
            //calculate modal noise profile
            NoiseProfile profile = NoiseProfile.CalculateModalNoiseProfile(matrix, DefaultStddevCount);
            //smooth the noise profile
            double[] smoothedProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7);
            return NoiseReduce_Standard(matrix, smoothedProfile, nhBackgroundThreshold);
        }

        /// <summary>
        /// expects a spectrogram in dB values
        /// </summary>
        public static double[,] NoiseReduce_Standard(double[,] matrix, double[] noiseProfile, double nhBackgroundThreshold)
        {
            double[,] mnr = matrix;
            mnr = TruncateBgNoiseFromSpectrogram(mnr, noiseProfile);
            mnr = RemoveNeighbourhoodBackgroundNoise(mnr, nhBackgroundThreshold);
            return mnr;
        }

        /// <summary>
        /// IMPORTANT: Mel scale conversion should be done before noise reduction
        /// </summary>
        public static double[,] NoiseReduce_FixedRange(double[,] matrix, double dynamicRange, double sdCount)
        {
            NoiseProfile profile = NoiseProfile.CalculateModalNoiseProfile(matrix, sdCount); //calculate modal noise profile
            double[] smoothedProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 7); //smooth the noise profile
            double[,] mnr = SubtractBgNoiseFromSpectrogram(matrix, smoothedProfile);
            mnr = SetDynamicRange(mnr, 0.0, dynamicRange);
            return mnr;
        }

        public static double[,] NoiseReduce_FlattenAndTrim(double[,] matrix, double stdDevCount)
        {
            int upperPercentileTrim = 95;
            var profile = NoiseProfile.CalculateModalNoiseProfile(matrix, stdDevCount); //calculate modal noise profile
            double[] smoothedProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, 5); //smooth the noise profile
            //double[,] mnr = SNR.SubtractBgNoiseFromSpectrogram(matrix, smoothedProfile);
            double[,] mnr = TruncateBgNoiseFromSpectrogram(matrix, smoothedProfile);
            const int temporalNh = 5;
            const int freqBinNh = 9;
            mnr = SetLocalBounds(mnr, 0, upperPercentileTrim, temporalNh, freqBinNh);
            return mnr;
        }

        /// <summary>
        /// The passed matrix is a sonogram with values in dB. wrt 0dB.
        /// </summary>
        public static double[,] NoiseReduce_Mean(double[,] matrix, double dynamicRange)
        {
            double[,] mnr = matrix;
            int startFrameCount = 9;
            int smoothingWindow = 7;

            double[] modalNoise = NoiseProfile.CalculateModalNoiseUsingStartFrames(mnr, startFrameCount);
            modalNoise = DataTools.filterMovingAverage(modalNoise, smoothingWindow); //smooth the noise profile
            mnr = NoiseReduce_Standard(matrix, modalNoise, DefaultNhBgThreshold);
            mnr = SetDynamicRange(mnr, 0.0, dynamicRange);

            const double ridgeThreshold = 0.1;
            byte[,] binary = RidgeDetection.IdentifySpectralRidges(mnr, ridgeThreshold);
            double[,] op = RidgeDetection.SpectralRidges2Intensity(binary, mnr);
            return op;
        }

        public static double[,] NoiseReduce_Median(double[,] matrix, double dynamicRange)
        {
            double[,] mnr = matrix;
            int startFrameCount = 9;

            mnr = ImageTools.WienerFilter(mnr, 11);

            double[] modalNoise = NoiseProfile.CalculateModalNoiseUsingStartFrames(mnr, startFrameCount);
            modalNoise = DataTools.filterMovingAverage(modalNoise, 5); //smooth the noise profile
            mnr = NoiseReduce_Standard(matrix, modalNoise, DefaultNhBgThreshold);
            mnr = SetDynamicRange(mnr, 0.0, dynamicRange);

            double[,] peaks = RidgeDetection.IdentifySpectralPeaks(mnr);
            return peaks;
        }

        // #############################################################################################################################
        // ################################# NOISE REDUCTION METHODS #################################################################

        /// <summary>
        /// Subtracts the supplied noise profile from spectorgram AND sets values less than backgroundThreshold to ZERO.
        /// </summary>
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
                }
            }

            return outM;
        } // end of TruncateModalNoise()

        /// <summary>
        /// Subtracts the supplied noise profile from spectorgram AND sets negative values to ZERO.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="noiseProfile"></param>
        public static double[,] TruncateBgNoiseFromSpectrogram(double[,] matrix, double[] noiseProfile)
        {
            double backgroundThreshold = 0.0;
            return SubtractAndTruncateNoiseProfile(matrix, noiseProfile, backgroundThreshold);
        }

        /// <summary>
        /// Subtracts the supplied modal noise value for each freq bin BUT DOES NOT set negative values to zero.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="noiseProfile"></param>
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
            }

            return outM;
        }

        public static double[,] TruncateNegativeValues2Zero(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var M = new double[rows, cols];
            for (int r = 0; r < rows; r++)
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
        /// <param name="minDb">minimum decibel value</param>
        /// <param name="maxDb">maximum decibel value</param>
        public static double[,] SetDynamicRange(double[,] m, double minDb, double maxDb)
        {
            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(m, out minIntensity, out maxIntensity);
            double shift = maxDb - maxIntensity;

            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            double[,] normM = new double[rowCount,colCount];
            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int row = 0; row < rowCount; row++) //for all rows
                {
                    normM[row, col] = m[row, col] + shift;
                    if (normM[row, col] < minDb) normM[row, col] = 0;
                }
            }

            return normM;
        }

        /// <summary>
        /// </summary>
        /// <param name="m">The spectral sonogram passes as matrix of doubles</param>
        /// <param name="minPercentileBound">minimum decibel value</param>
        /// <param name="maxPercentileBound">maximum decibel value</param>
        public static double[,] SetGlobalBounds(double[,] m, int minPercentileBound, int maxPercentileBound)
        {
            int binCount = 100; // histogram width is adjusted to length of signal
            double minIntensity, maxIntensity, binWidth;
            int[] histo = Histogram.Histo(m, binCount, out binWidth, out minIntensity, out maxIntensity);

            int lowerBinBound = Histogram.GetPercentileBin(histo, minPercentileBound);
            int upperBinBound = Histogram.GetPercentileBin(histo, maxPercentileBound);

            double lowerBound = minIntensity + (lowerBinBound * binWidth);
            double upperBound = minIntensity + (upperBinBound * binWidth);

            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            double[,] normM = new double[rowCount, colCount];
            for (int col = 0; col < colCount; col++) //for all cols i.e. freq bins
            {
                for (int row = 0; row < rowCount; row++) //for all rows
                {
                    normM[row, col] = m[row, col];
                    if (normM[row, col] < lowerBound)
                    {
                        normM[row, col] = lowerBound;
                    }
                    else
                    if (normM[row, col] > upperBound)
                    {
                        normM[row, col] = upperBound;
                    }
                }
            }

            return normM;
        }

        /// <summary>
        /// </summary>
        /// <param name="m">The spectral sonogram passes as matrix of doubles</param>
        /// <param name="minPercentileBound">minimum decibel value</param>
        /// <param name="maxPercentileBound">maximum decibel value</param>
        /// <param name="temporalNh"></param>
        /// <param name="freqBinNh"></param>
        /// <returns></returns>
        public static double[,] SetLocalBounds(double[,] m, int minPercentileBound, int maxPercentileBound, int temporalNh, int freqBinNh)
        {
            int binCount = 100; // histogram width is adjusted to length of signal
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            double[,] normM = new double[rowCount, colCount];

            for (int col = freqBinNh; col < colCount - freqBinNh; col++) //for all cols i.e. freq bins
            {
                for (int row = temporalNh; row < rowCount - temporalNh; row++) //for all rows i.e. frames
                {
                    var localMatrix = MatrixTools.Submatrix(m, row- temporalNh, col- freqBinNh, row+ temporalNh, col+ freqBinNh);
                    double minIntensity, maxIntensity, binWidth;
                    int[] histo = Histogram.Histo(localMatrix, binCount, out binWidth, out minIntensity, out maxIntensity);
                    int lowerBinBound = Histogram.GetPercentileBin(histo, minPercentileBound);
                    int upperBinBound = Histogram.GetPercentileBin(histo, maxPercentileBound);
                    // double lowerBound = minIntensity + (lowerBinBound * binWidth);
                    // double upperBound = minIntensity + (upperBinBound * binWidth);
                    // calculate the range = upperBound - lowerBound
                    //                     = (minIntensity + (upperBinBound * binWidth)) - (minIntensity + (lowerBinBound * binWidth));
                    //                     = (upperBinBound - lowerBinBound) * binWidth;
                    normM[row, col] = (upperBinBound - lowerBinBound) * binWidth;
                }
            }

            return normM;
        }

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
        }

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
        /// used to store info about the SNR in a signal using db units
        /// </summary>
        public class SnrStatistics
        {
            /// <summary>
            /// Duration of the event under consideration.
            /// It may be shorter or longer than the actual recording we have.
            /// If longer then the event, then duration := recording duration.
            /// Rest was truncated in original data extraction.
            /// </summary>
            public TimeSpan ExtractDuration { get; set; }

            /// <summary>
            /// decibel threshold used to calculate cover and average SNR
            /// </summary>
            public double Threshold { get; set; }

            /// <summary>
            /// maximum dB value in the signal or spectrogram - relative to zero dB background
            /// </summary>
            public double Snr { get; set; }

            /// <summary>
            /// fraction of frames in the call where the average energy exceeds the user specified threshold.
            /// </summary>
            public double FractionOfFramesExceedingThreshold { get; set; }

            /// <summary>
            /// Gets or sets fraction of frames in the call where the average energy exceeds half the calculated SNR.
            /// </summary>
            public double FractionOfFramesExceedingOneThirdSnr { get; set; }
        }
    }
}
