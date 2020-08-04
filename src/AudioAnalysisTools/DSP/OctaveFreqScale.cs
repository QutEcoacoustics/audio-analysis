// <copyright file="OctaveFreqScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using MathNet.Numerics;
    using TowseyLibrary;

    public static class OctaveFreqScale
    {
        /// IMPORTANT NOTE: If you are converting Herz scale from LINEAR to OCTAVE, this conversion MUST be done BEFORE noise reduction
        /// <summary>
        /// CONSTRUCTION OF Frequency Scales
        /// WARNING!: Changing the constants for the octave scales will have undefined effects.
        ///           The options below have been debugged to give what is required.
        ///           However other values have not been debugged - so user should check the output to ensure it is what is required.
        /// </summary>
        public static void GetOctaveScale(FrequencyScale scale)
        {
            int sr, frameSize;

            // NOTE: octaveDivisions = the number of fractional Hz steps within one octave. Piano octave contains 12 steps per octave.

            var fst = scale.ScaleType;

            switch (fst)
            {
                case FreqScaleType.LinearOctaveStandard:
                    //This is a split linear-octave frequency scale.
                    // Valid values for linearUpperBound are 125, 250, 500, 1000.
                    int linearUpperBound = 1000;
                    GetStandardOctaveScale(scale, linearUpperBound);
                    return;

                case FreqScaleType.OctaveDataReduction:
                    // This spectral conversion is for data reduction purposes.
                    // It is a split linear-octave frequency scale.
                    GetDataReductionScale(scale);
                    return;

                case FreqScaleType.Linear62OctaveTones31Nyquist11025:
                    sr = 22050;
                    frameSize = 8192;
                    scale.LinearBound = 62;
                    scale.ToneCount = 31; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.FinalBinCount = 253;
                    break;

                case FreqScaleType.Linear125OctaveTones30Nyquist11025:
                    // constants required for split linear-octave scale when sr = 22050
                    sr = 22050;
                    frameSize = 8192;
                    scale.LinearBound = 125;
                    scale.ToneCount = 32; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.FinalBinCount = 255;
                    break;

                case FreqScaleType.Octaves24Nyquist32000:
                    //// constants required for full octave scale when sr = 64000
                    sr = 64000;
                    frameSize = 16384;
                    scale.LinearBound = 15;
                    scale.ToneCount = 24; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.FinalBinCount = 253;
                    break;

                case FreqScaleType.Linear125OctaveTones28Nyquist32000:
                    // constants required for split linear-octave scale when sr = 64000
                    sr = 64000;
                    frameSize = 16384; // = 2*8192   or 4*4096;
                    scale.LinearBound = 125;
                    scale.ToneCount = 28; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.FinalBinCount = 253;
                    break;

                default:
                    LoggedConsole.WriteErrorLine("WARNING: GetOctaveScale() was passed UNKNOWN OCTAVE SCALE.");
                    return;
            }

            scale.Nyquist = sr / 2;
            scale.WindowSize = frameSize; // = 2*8192   or 4*4096
            scale.BinBounds = LinearToSplitLinearOctaveScale(sr, frameSize, scale.LinearBound, scale.Nyquist, scale.ToneCount);
            scale.GridLineLocations = GetGridLineLocations(fst, scale.BinBounds);
        }

        /// <summary>
        /// Calculates the parameters for a mixed linear-octave frequency scale.
        /// Works only for "standard" recordings, i.e. sr = 22050 and frame = 512.
        /// The number of octaveDivsions/tones (T) is set equal to number of linear bins.
        /// The remainder of the spectrum will be reduced over T-tone octaves.
        /// Valid values for linearUpperBound are 125, 250, 500, 1000.
        /// Note that when linearUpperBound = 500, the resulting spectrogram is very similar to the default MelScale.
        /// The default MelScale has 64 frequency bins and Linear500-octave has 66 frequency bands.
        /// </summary>
        /// <param name="linearUpperBound">The upper limit of the linear frqeuency band in Hertz.</param>
        public static FrequencyScale GetStandardOctaveScale(FrequencyScale scale, int linearUpperBound)
        {
            int sr = 22050;
            int frameSize = 512;
            scale.WindowSize = frameSize;
            scale.Nyquist = sr / 2;
            scale.LinearBound = linearUpperBound;
            var binWidth = sr / (double)frameSize;

            if (linearUpperBound < 64 || linearUpperBound > scale.Nyquist - 64)
            {
                    throw new ArgumentException("WARNING: Illegal parameter passed to method GetStandardOctaveScale(int linearUpperBound).");
            }

            // init tone steps within one octave. Note: piano = 12 steps per octave.
            scale.ToneCount = (int)Math.Round(scale.LinearBound / binWidth);
            scale.BinBounds = LinearToSplitLinearOctaveScale(sr, frameSize, scale.LinearBound, scale.Nyquist, scale.ToneCount);
            scale.FinalBinCount = scale.BinBounds.GetLength(0);

            // These only work for case where linearUpperScale = 1000 Hz
            double freqStep = sr / frameSize;
            int topLinearIndex = (int)Math.Round(linearUpperBound / freqStep);

            var gridLineLocations = new int[4, 2];
            gridLineLocations[0, 0] = topLinearIndex;
            gridLineLocations[0, 1] = 1000;
            gridLineLocations[1, 0] = topLinearIndex * 2;
            gridLineLocations[1, 1] = 2000;

            if (linearUpperBound > 256)
            {
                gridLineLocations[2, 0] = topLinearIndex * 3;
                gridLineLocations[2, 1] = 4000;
                gridLineLocations[3, 0] = topLinearIndex * 4;
                gridLineLocations[3, 1] = 8000;
            }

            scale.GridLineLocations = gridLineLocations;
            return scale;
        }

        public static double[,] ConvertAmplitudeSpectrogramToDecibelOctaveScale(double[,] inputSpgram, FrequencyScale freqScale)
        {
            //square the values to produce power spectrogram
            var dataMatrix = MatrixTools.SquareValues(inputSpgram);

            //convert spectrogram to octave scale
            var newMatrix = ConvertLinearSpectrogramToOctaveFreqScale(dataMatrix, freqScale);
            newMatrix = MatrixTools.Power2DeciBels(newMatrix, out var min, out var max);
            return newMatrix;
        }

        /// <summary>
        /// Converts a spectrogram having linear freq scale to one having an Octave freq scale.
        /// Note that the sample rate (sr) and the frame size both need to be apporpriate to the choice of FreqScaleType.
        /// TODO: SHOULD DEVELOP A SEPARATE UNIT TEST for this method.
        /// </summary>
        public static double[,] ConvertLinearSpectrogramToOctaveFreqScale(double[,] inputSpgram, FrequencyScale freqScale)
        {
            if (freqScale == null)
            {
                throw new ArgumentNullException(nameof(freqScale));
            }

            if (freqScale.ScaleType == FreqScaleType.Linear)
            {
                LoggedConsole.WriteLine("Linear Hz Scale is not valid for this Octave method.");
                throw new ArgumentNullException(nameof(freqScale));
            }

            // get the octave bin bounds for this octave scale type
            var octaveBinBounds = freqScale.BinBounds;
            int newBinCount = octaveBinBounds.GetLength(0);

            // set up the new octave spectrogram
            int frameCount = inputSpgram.GetLength(0);

            //int binCount = inputSpgram.GetLength(1);
            double[,] octaveSpectrogram = new double[frameCount, newBinCount];

            for (int row = 0; row < frameCount; row++)
            {
                //get each frame or spectrum in turn
                var linearSpectrum = MatrixTools.GetRow(inputSpgram, row);

                // convert the spectrum to its octave form
                var octaveSpectrum = OctaveSpectrum(octaveBinBounds, linearSpectrum);

                //return the spectrum to output spectrogram.
                MatrixTools.SetRow(octaveSpectrogram, row, octaveSpectrum);
            }

            return octaveSpectrogram;
        }

        public static double[,] DecibelSpectra(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon, FrequencyScale freqScale)
        {
            double[,] powerSpectra = PowerSpectra(amplitudeM, windowPower, sampleRate, epsilon, freqScale);

            // Convert the power values to log using: dB = 10*log(power)
            var decibelSpectra = MatrixTools.Power2DeciBels(powerSpectra, out var min, out var max);
            return decibelSpectra;
        }

        public static double[,] AmplitudeSpectra(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon, FrequencyScale freqScale)
        {
            double[,] powerSpectra = PowerSpectra(amplitudeM, windowPower, sampleRate, epsilon, freqScale);

            // Convert the power values back to amplitude by taking the square root.
            var amplitudeSpectra = MatrixTools.SquareRootOfValues(powerSpectra);
            return amplitudeSpectra;
        }

        /// <summary>
        /// Converts an amplitude spectrogram to a power spectrogram having an octave frequency scale.
        /// It transforms the amplitude spectrogram in the following steps:
        /// (1) It removes the DC row or bin 0 iff there is odd number of spectrogram bins. ASSUMPTION: Bin count should be power of 2 from FFT.
        /// (1) It converts spectral amplitudes to power, normalising for window power and sample rate.
        ///     The window contributes power to the signal which must subsequently be removed from the spectral power. Calculate power per sample.
        ///     See notes in the MFCCStuff.DecibelSpectra for further exp[lanaitons. These normalisations were adapted from MatLab MFCC code.
        /// (2) Then reduce the linear scale toan octave scale depending on the sr and required number of bins or filters.
        /// </summary>
        /// <param name="amplitudeM"> the amplitude spectra. </param>
        /// <param name="windowPower">value for window power normalisation.</param>
        /// <param name="sampleRate">to NormaliseMatrixValues for the sampling rate.</param>
        /// <param name="epsilon">small value to avoid log of zero.</param>
        /// <param name="freqScale">the kind of frequency scale.</param>
        public static double[,] PowerSpectra(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon, FrequencyScale freqScale)
        {
            int frameCount = amplitudeM.GetLength(0);
            int binCount = amplitudeM.GetLength(1);

            double minPow = epsilon * epsilon / windowPower / sampleRate;
            double minPow2 = epsilon * epsilon * 2 / windowPower / sampleRate;

            if (binCount.IsOdd())
            {
                // remove the DC freq bin 0.
                amplitudeM = MatrixTools.Submatrix(amplitudeM, 0, 1, frameCount - 1, binCount - 1);
            }

            // init the spectrogram as a matrix of spectra
            double[,] powerSpectra = new double[frameCount, binCount];

            // first square the values to calculate power.
            // Must multiply by 2 to accomodate two spectral components, ie positive and neg freq.
            for (int j = 0; j < binCount - 1; j++)
            {
                //foreach time step or frame
                for (int i = 0; i < frameCount; i++)
                {
                    if (amplitudeM[i, j] < epsilon)
                    {
                        powerSpectra[i, j] = minPow2;
                    }
                    else
                    {
                        powerSpectra[i, j] = amplitudeM[i, j] * amplitudeM[i, j] * 2 / windowPower / sampleRate;
                    }
                } //end of all frames
            } //end of all freq bins

            //calculate power of the Nyquist freq bin - last column of matrix
            //foreach time step or frame
            for (int i = 0; i < frameCount; i++)
            {
                //calculate power of the DC value
                if (amplitudeM[i, binCount - 1] < epsilon)
                {
                    powerSpectra[i, binCount - 1] = minPow;
                }
                else
                {
                    powerSpectra[i, binCount - 1] = amplitudeM[i, binCount - 1] * amplitudeM[i, binCount - 1] / windowPower / sampleRate;
                }
            }

            powerSpectra = ConvertLinearSpectrogramToOctaveFreqScale(powerSpectra, freqScale);
            return powerSpectra;
        }

        public static int[,] GetGridLineLocations(FreqScaleType ost, int[,] octaveBinBounds)
        {
            int[,] gridLineLocations = null;

            switch (ost)
            {
                case FreqScaleType.Linear62OctaveTones31Nyquist11025:
                    gridLineLocations = new int[8, 2];
                    LoggedConsole.WriteErrorLine("This Octave Scale does not currently have grid data provided.");
                    break;

                case FreqScaleType.Linear125OctaveTones30Nyquist11025:
                    gridLineLocations = new int[7, 2];
                    gridLineLocations[0, 0] = 46; //  125 Hz
                    gridLineLocations[1, 0] = 79; //  250
                    gridLineLocations[2, 0] = 111; //  500
                    gridLineLocations[3, 0] = 143; // 1000
                    gridLineLocations[4, 0] = 175; // 2000
                    gridLineLocations[5, 0] = 207; // 4000
                    gridLineLocations[6, 0] = 239; // 8000

                    // enter the Hz value
                    gridLineLocations[0, 1] = 125; //  125 Hz
                    gridLineLocations[1, 1] = 250; //  250
                    gridLineLocations[2, 1] = 500; //  500
                    gridLineLocations[3, 1] = 1000; // 1000
                    gridLineLocations[4, 1] = 2000; // 2000
                    gridLineLocations[5, 1] = 4000; // 4000
                    gridLineLocations[6, 1] = 8000; // 8000
                    break;

                case FreqScaleType.OctaveDataReduction:
                    //This Octave Scale does not require grid lines. It is for data reduction purposes only
                    gridLineLocations = new int[6, 2];
                    break;

                case FreqScaleType.Octaves24Nyquist32000:
                    gridLineLocations = new int[8, 2];
                    LoggedConsole.WriteErrorLine("This Octave Scale does not currently have grid data provided.");
                    break;

                case FreqScaleType.Linear125OctaveTones28Nyquist32000:
                    gridLineLocations = new int[9, 2];
                    gridLineLocations[0, 0] = 34; //  125 Hz
                    gridLineLocations[1, 0] = 62; //  250
                    gridLineLocations[2, 0] = 89; //  500
                    gridLineLocations[3, 0] = 117; // 1000
                    gridLineLocations[4, 0] = 145; // 2000
                    gridLineLocations[5, 0] = 173; // 4000
                    gridLineLocations[6, 0] = 201; // 8000
                    gridLineLocations[7, 0] = 229; //16000
                    gridLineLocations[8, 0] = 256; //32000

                    // enter the Hz values
                    gridLineLocations[0, 1] = 125; //  125 Hz
                    gridLineLocations[1, 1] = 250; //  250
                    gridLineLocations[2, 1] = 500; //  500
                    gridLineLocations[3, 1] = 1000; // 1000
                    gridLineLocations[4, 1] = 2000; // 2000
                    gridLineLocations[5, 1] = 4000; // 4000
                    gridLineLocations[6, 1] = 8000; // 8000
                    gridLineLocations[7, 1] = 16000; //16000
                    gridLineLocations[8, 1] = 32000; //32000

                    break;
                default:
                    LoggedConsole.WriteErrorLine("Not a valid Octave Scale.");
                    break;
            }

            return gridLineLocations;
        }

        /// <summary>
        /// Converts a single linear spectrum to octave scale spectrum.
        /// WARNING: THis method assumes that the ocatve spectrum is to be same length as linear spectrum.
        /// Therefore the index values in the octaveBinBounds matrix should NOT exceed bounds of the linear spectrum.
        /// </summary>
        public static double[] OctaveSpectrum(int[,] octaveBinBounds, double[] linearSpectrum)
        {
            int length = octaveBinBounds.GetLength(0);
            var octaveSpectrum = new double[length];

            // Fill in the first value of the octave spectrum
            int lowIndex1 = octaveBinBounds[0, 0];
            int centreIndex1 = octaveBinBounds[0, 0];
            int highIndex1 = octaveBinBounds[1, 0];
            octaveSpectrum[0] = FilterbankIntegral(linearSpectrum, lowIndex1, centreIndex1, highIndex1);

            // fill in remainer except last
            for (int i = 1; i < length - 1; i++)
            {
                int lowIndex = octaveBinBounds[i - 1, 0];
                int centreIndex = octaveBinBounds[i, 0];
                int highIndex = octaveBinBounds[i + 1, 0];
                if (highIndex >= linearSpectrum.Length)
                {
                    highIndex = linearSpectrum.Length - 1;
                }

                octaveSpectrum[i] = FilterbankIntegral(linearSpectrum, lowIndex, centreIndex, highIndex);
            }

            // now fill in the last value of the octave spectrum
            int lowIndex2 = octaveBinBounds[length - 2, 0];
            int centreIndex2 = octaveBinBounds[length - 1, 0];
            int highIndex2 = octaveBinBounds[length - 1, 0];

            if (highIndex2 != 0)
            {
                octaveSpectrum[length - 1] = FilterbankIntegral(linearSpectrum, lowIndex2, centreIndex2, highIndex2);
            }

            return octaveSpectrum;
        }

        /// <summary>
        /// Returns the index bounds for a split herz scale - bottom part linear, top part octave scaled.
        /// </summary>
        public static int[,] LinearToSplitLinearOctaveScale(int sr, int frameSize, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {
            var octaveBandsLowerBounds = GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);
            int nyquist = sr / 2;
            int spectrumBinCount = frameSize / 2;
            var linearFreqScale = GetLinearFreqScale(nyquist, spectrumBinCount);

            double freqStep = nyquist / (double)spectrumBinCount;
            int topLinearIndex = (int)Math.Round(lowerFreqBound / freqStep);
            int finalBinCount = topLinearIndex + octaveBandsLowerBounds.GetLength(0);
            var splitLinearOctaveIndexBounds = new int[finalBinCount, 2];

            // fill in the linear part of the freq scale
            for (int i = 0; i <= topLinearIndex; i++)
            {
                splitLinearOctaveIndexBounds[i, 0] = i;
                splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[i]);
            }

            for (int i = topLinearIndex + 1; i < finalBinCount; i++)
            {
                for (int j = 0; j < linearFreqScale.Length; j++)
                {
                    if (linearFreqScale[j] > octaveBandsLowerBounds[i - topLinearIndex])
                    {
                        splitLinearOctaveIndexBounds[i, 0] = j;
                        splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[j]);
                        break;
                    }
                }
            }

            // make sure last index extends to last bin of the linear spectrum.
            splitLinearOctaveIndexBounds[finalBinCount - 1, 0] = linearFreqScale.Length - 1;
            splitLinearOctaveIndexBounds[finalBinCount - 1, 1] = (int)Math.Round(linearFreqScale[linearFreqScale.Length - 1]);

            return splitLinearOctaveIndexBounds;
        }

        /// <summary>
        /// This method assumes that the linear spectrum is derived from a 512 frame with sr = 22050.
        /// It is a split linear-octave scale.
        /// The linear part is from 0-2 kHz with reduction by averaging every 6 frequency bins.
        /// The octave part is obtained by setting octave divisions or tone count = 5.
        /// </summary>
        /// <returns>a frequency scale for spectral-data reduction purposes.</returns>
        public static FrequencyScale GetDataReductionScale(FrequencyScale scale)
        {
            int sr = 22050;
            int frameSize = 512;
            scale.Nyquist = sr / 2;

            // linear reduction of the lower spectrum from 0 - 2 kHz.
            scale.LinearBound = 2000;
            int linearReductionFactor = 6;

            // Reduction of upper spectrum 2-11 kHz: Octave count and tone steps within one octave.
            double octaveCount = 2.7;
            scale.ToneCount = 5;

            var octaveBandsLowerBounds = GetFractionalOctaveBands(scale.LinearBound, scale.Nyquist, scale.ToneCount);
            int spectrumBinCount = frameSize / 2;
            var linearFreqScale = GetLinearFreqScale(scale.Nyquist, spectrumBinCount);

            double linearBinWidth = scale.Nyquist / (double)spectrumBinCount;
            int topLinearIndex = (int)Math.Round(scale.LinearBound / linearBinWidth);
            int linearReducedBinCount = topLinearIndex / linearReductionFactor;
            int finalBinCount = linearReducedBinCount + (int)Math.Floor(octaveCount * scale.ToneCount);
            var splitLinearOctaveIndexBounds = new int[finalBinCount, 2];

            // fill in the linear part of the freq scale
            for (int i = 0; i < linearReducedBinCount; i++)
            {
                splitLinearOctaveIndexBounds[i, 0] = i;
                splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[i * linearReductionFactor]);
            }

            // fill in the octave part of the freq scale
            for (int i = linearReducedBinCount; i < finalBinCount; i++)
            {
                for (int j = 0; j < linearFreqScale.Length; j++)
                {
                    if (linearFreqScale[j] > octaveBandsLowerBounds[i - linearReducedBinCount])
                    {
                        splitLinearOctaveIndexBounds[i, 0] = j;
                        splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[j]);
                        break;
                    }
                }
            }

            // make sure last index extends to last bin of the linear spectrum.
            splitLinearOctaveIndexBounds[finalBinCount - 1, 0] = linearFreqScale.Length - 1;
            splitLinearOctaveIndexBounds[finalBinCount - 1, 1] = (int)Math.Round(linearFreqScale[linearFreqScale.Length - 1]);

            scale.BinBounds = splitLinearOctaveIndexBounds;
            return scale;
        }

        /// <summary>
        /// Returns the index bounds for a full octave scale - from lowest freq set by user to top freq.
        /// </summary>
        /// <param name="sr">Sample rate of the source recording.</param>
        /// <param name="frameSize">Frame size of the source recording.</param>
        /// <param name="finalBinCount">Final Bin Count.</param>
        /// <param name="lowerFreqBound">Lower bound of the octave part of the final scale.</param>
        /// <param name="upperFreqBound">Upper bound of the octave scale, most likely the Nyquist.</param>
        /// <param name="octaveDivisions">Number of tones/divisions per octave.</param>
        public static int[,] LinearToFullOctaveScale(int sr, int frameSize, int finalBinCount, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {
            var bandBounds = GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);
            int nyquist = sr / 2;
            int binCount = frameSize / 2;
            var linearFreqScale = GetLinearFreqScale(nyquist, binCount);

            var octaveIndexBounds = new int[finalBinCount, 2];

            for (int i = 0; i < finalBinCount; i++)
            {
                for (int j = 0; j < linearFreqScale.Length; j++)
                {
                    if (linearFreqScale[j] > bandBounds[i])
                    {
                        octaveIndexBounds[i, 0] = j;
                        octaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[j]);
                        break;
                    }
                }
            }

            return octaveIndexBounds;
        }

        public static double[] GetFractionalOctaveBands(double minFreq, double maxFreq, int octaveDivisions)
        {
            double[] octaveLowerBounds = { 15.625, 31.25, 62.5, 125, 250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000 };

            var list = new List<double>();

            for (int i = 0; i < octaveLowerBounds.Length; i++)
            {
                // ignore this octave floor if below that required.
                if (octaveLowerBounds[i] < minFreq)
                {
                    continue;
                }

                // stop when octave floor is above that required.
                if (octaveLowerBounds[i] > maxFreq)
                {
                    break;
                }

                // get the frequency tones in the given octave.
                double[] tonesInOctave = GetFractionalOctaveBands(octaveLowerBounds[i], octaveDivisions);

                for (int j = 0; j < octaveDivisions; j++)
                {
                    double toneFloor = tonesInOctave[j];
                    if (toneFloor < minFreq)
                    {
                        continue;
                    }

                    if (toneFloor > maxFreq)
                    {
                        break;
                    }

                    list.Add(toneFloor);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Returns an array of tones in one octave.
        /// The units are frequency in Hertz.
        /// NOTE: The octave is divided geometrically.
        /// </summary>
        /// <param name="lowerBound">The lower frquency bound of the octave.</param>
        /// <param name="subbandCount">The number of tones or frequency bins in the octave.</param>
        /// <returns>The frequency of each tone in the octave.</returns>
        public static double[] GetFractionalOctaveBands(double lowerBound, int subbandCount)
        {
            double[] fractionalOctaveBands = new double[subbandCount];
            fractionalOctaveBands[0] = lowerBound;
            double exponent = 1 / (double)subbandCount;

            // calculate the frequency increment factor between each tone and the next.
            double factor = Math.Pow(2, exponent);
            for (int i = 1; i < subbandCount; i++)
            {
                fractionalOctaveBands[i] = fractionalOctaveBands[i - 1] * factor;
            }

            return fractionalOctaveBands;
        }

        /// <summary>
        /// THis method assumes that the frameSize will be power of 2
        /// FOR DEBUG PURPOSES, when sr = 22050 and frame size = 8192, the following Hz are located at index:
        /// Hz      Index
        /// 15        6
        /// 31       12
        /// 62       23
        /// 125      46
        /// 250      93
        /// 500     186
        /// 1000    372.
        /// </summary>
        public static double[] GetLinearFreqScale(int nyquist, int binCount)
        {
            double freqStep = nyquist / (double)binCount;
            double[] linearFreqScale = new double[binCount];

            for (int i = 0; i < binCount; i++)
            {
                linearFreqScale[i] = freqStep * i;
            }

            return linearFreqScale;
        }

        public static double FilterbankIntegral(double[] spectrum, int lowIndex, int centreIndex, int highIndex)
        {
            // let k = index into spectral vector.
            // for all k < lowIndex,  filterBank[k] = 0;
            // for all k > highIndex, filterBank[k] = 0;

            // for all k in range (lowIndex    <= k < centreIndex), filterBank[k] = (k-lowIndex) /(centreIndex - lowIndex)
            // for all k in range (centreIndex <= k <= highIndex),  filterBank[k] = (highIndex-k)/(highIndex - centreIndex)

            double area = 0.0;
            double integral = 0.0;
            int delta = centreIndex - lowIndex;
            if (delta > 0)
            {
                for (int k = lowIndex; k < centreIndex; k++)
                {
                    double weight = (k - lowIndex) / (double)delta;
                    integral += weight * spectrum[k];
                    area += weight;
                }
            }

            integral += spectrum[centreIndex];
            area += 1.0;

            delta = highIndex - centreIndex;
            if (delta > 0)
            {
                for (int k = centreIndex + 1; k <= highIndex; k++)
                {
                    if (delta == 0)
                    {
                        continue;
                    }

                    double weight = (highIndex - k) / (double)delta;
                    integral += weight * spectrum[k];
                    area += weight;
                }
            }

            // NormaliseMatrixValues to area of the triangular filter
            integral /= area;
            return integral;
        }
    }
}