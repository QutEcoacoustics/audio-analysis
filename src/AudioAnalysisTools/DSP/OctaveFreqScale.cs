// <copyright file="OctaveFreqScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using AudioAnalysisTools.StandardSpectrograms;
    using MathNet.Numerics;
    using TowseyLibrary;

    public static class OctaveFreqScale
    {
        /// <summary>
        /// Calculates the parameters for a standard mixed linear-octave frequency scale.
        /// IMPORTANT: It assumes that the nyquist, frame size and linear bound have already been set to valid values.
        /// What makes this scale "standard" is that the number of octaveDivsions/tones (T) is set equal to number of linear bins.
        /// The remainder of the spectrum will be reduced over T-tone octaves.
        /// Sensible values for linearUpperBound are 125, 250, 500, 1000.
        /// Note that when linearUpperBound = 500, the resulting spectrogram is very similar to the default MelScale.
        /// When nyquist=11025 and frameSize = 512, the default MelScale has 64 frequency bins and Linear500-octave has 66 frequency bands.
        /// </summary>
        public static FrequencyScale GetStandardOctaveScale(FrequencyScale scale)
        {
            int sr = scale.Nyquist * 2;
            var binWidth = sr / (double)scale.WindowSize;
            int linearUpperBound = scale.LinearBound;

            if (linearUpperBound < 32 || linearUpperBound > scale.Nyquist - 64)
            {
                    throw new ArgumentException("WARNING: Invalid LinearBound value for method GetStandardOctaveScale().");
            }

            // init tone steps within one octave. Note: piano octave = 12 steps per octave.
            scale.ToneCount = (int)Math.Round(scale.LinearBound / binWidth);
            scale.BinBounds = LinearToSplitLinearOctaveScale(scale.Nyquist, scale.WindowSize, scale.LinearBound, scale.Nyquist, scale.ToneCount);
            scale.FinalBinCount = scale.BinBounds.GetLength(0);

            // These only work for case where linearUpperScale = 1000 Hz
            int topLinearIndex = (int)Math.Round(linearUpperBound / binWidth);

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

        /// <summary>
        /// This method is octave frequency scale equivalent of MFCCStuff.DecibelSpectra(dspOutput.AmplitudeSpectrogram, dspOutput.WindowPower, sampleRate, epsilon)
        /// The MFCCStuff method is the proper way to convert amplitude spectrogram to decibels.
        /// It converts an amplitude spectrogram to a power spectrogram having specified frequency scale.
        /// It transforms the amplitude spectrogram in the following steps:
        /// (1) It removes the DC row or bin 0 iff there is odd number of spectrogram bins. ASSUMPTION: Bin count should be power of 2 from FFT.
        /// (2) Then reduce the linear scale to an octave scale depending on the sr and required number of bins or filters.
        /// (3) It converts spectral amplitudes to power, normalising for window power and sample rate.
        ///     The window contributes power to the signal which must subsequently be removed from the spectral power. Calculate power per sample.
        ///     See notes in the MFCCStuff.DecibelSpectra for further exp[lanaitons. These normalisations were adapted from MatLab MFCC code.
        /// (4) It converts the power spectrogram to decibels.
        /// </summary>
        /// <param name="amplitudeM">The amplitude spectrogram.</param>
        /// <param name="windowPower">FFT window power comes from DSP_Frames.WindowPower.</param>
        /// <param name="sampleRate">of the original signal.</param>
        /// <param name="epsilon">dependson bit-rate of the original signal.</param>
        /// <param name="freqScale">In this case an octave frquency scale.</param>
        /// <returns>The decibel spectrogram.</returns>
        public static double[,] ConvertAmplitudeSpectrogramToFreqScaledDecibels(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon, FrequencyScale freqScale)
        {
            int frameCount = amplitudeM.GetLength(0);
            int binCount = amplitudeM.GetLength(1);

            if (binCount.IsOdd())
            {
                // remove the DC freq bin 0.
                amplitudeM = MatrixTools.Submatrix(amplitudeM, 0, 1, frameCount - 1, binCount - 1);
            }

            var octaveScaleM = ConvertLinearSpectrogramToOctaveFreqScale(amplitudeM, freqScale);
            var powerSpectrogram = ConvertAmplitudeToPowerSpectrogram(octaveScaleM, windowPower, sampleRate);

            // Convert the power values to log using: dB = 10*log(power)
            var powerEpsilon = epsilon * epsilon / windowPower / sampleRate;
            var decibelSpectrogram = MatrixTools.SpectrogramPower2DeciBels(powerSpectrogram, powerEpsilon, out var min, out var max);
            return decibelSpectrogram;
        }

        /// <summary>
        /// Converts a spectrogram having linear freq scale to one having an Octave freq scale.
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
                //var octaveSpectrum = ConvertLinearSpectrumToOctaveScale(octaveBinBounds, linearSpectrum);
                var octaveSpectrum = SpectrogramTools.RescaleSpectrumUsingFilterbank(octaveBinBounds, linearSpectrum);

                //return the spectrum to output spectrogram.
                MatrixTools.SetRow(octaveSpectrogram, row, octaveSpectrum);
            }

            return octaveSpectrogram;
        }

        /// <summary>
        /// Converts Amplitude Spectrogram to Power Spectrogram.
        /// Square the amplitude values to get power.
        /// Power values must be adjusted for the power in the FFT window and also for the sample rate.
        /// Must divide by the window power to remove its contribution to amplitude values.
        /// Must divide by sample rate to get average power per signal sample.
        /// NOTE: Multiply by 2 to accomodate two spectral components, ie positive and neg freq.
        ///       BUT the last nyquist bin does not require a factor of two.
        ///       However this method is called only by octave reduced frequency scales where the nyquist bin is just one of several.
        /// </summary>
        /// <param name="amplitudeM">The frequency scaled amplitude spectrogram.</param>
        /// <param name="windowPower">Power of the FFT window.</param>
        /// <param name="sampleRate">The sample rate of the original recording.</param>
        /// <returns>The Power Spectrogram as matrix. Each spectrum is a matrix row.</returns>
        public static double[,] ConvertAmplitudeToPowerSpectrogram(double[,] amplitudeM, double windowPower, int sampleRate)
        {
            int frameCount = amplitudeM.GetLength(0);
            int binCount = amplitudeM.GetLength(1);

            // init the octave scaled spectrogram as a matrix of spectra
            double[,] powerSpectrogram = new double[frameCount, binCount];

            // Square the values to calculate power.
            // Must multiply by 2 to accomodate two spectral components, ie positive and neg freq.
            for (int j = 0; j < binCount; j++)
            {
                //foreach time step or frame
                for (int i = 0; i < frameCount; i++)
                {
                    powerSpectrogram[i, j] = amplitudeM[i, j] * amplitudeM[i, j] * 2 / windowPower / sampleRate;
                }
            }

            /*
            //calculate power of the Nyquist freq bin - last column of matrix
            //foreach time step or frame
            for (int i = 0; i < frameCount; i++)
            {
                powerSpectra[i, binCount - 1] = amplitudeM[i, binCount - 1] * amplitudeM[i, binCount - 1] / windowPower / sampleRate;
            }
            */

            return powerSpectrogram;
        }

        public static int[,] GetGridLineLocations(int nyquist, int linearBound, int[,] octaveBinBounds)
        {
            int[] gridLocationsHertz = { 62, 125, 250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000, 128000 };
            int binCount = octaveBinBounds.GetLength(0);
            var gridLineLocations = new List<int[]>();
            var upperBound = nyquist * 0.75;

            // get location index of the first required gridline.
            int glIndex = 0;
            while (gridLocationsHertz[glIndex] < linearBound)
            {
                glIndex++;
            }

            int glHertz = gridLocationsHertz[glIndex];

            for (int i = 0; i < binCount; i++)
            {
                if (octaveBinBounds[i, 1] < linearBound)
                {
                    continue;
                }

                if (octaveBinBounds[i, 1] > upperBound)
                {
                    break;
                }

                // if get to a grid location
                if (octaveBinBounds[i, 1] >= glHertz)
                {
                    var intArray = new int[2];
                    intArray[0] = i;
                    intArray[1] = glHertz; // octaveBinBounds[i, 1];
                    gridLineLocations.Add(intArray);

                    glIndex++;
                    glHertz = gridLocationsHertz[glIndex];
                }
            }

            // there is better way to do this.
            var returnMatrix = new int[gridLineLocations.Count, 2];
            for (int gl = 0; gl < gridLineLocations.Count; gl++)
            {
                var location = gridLineLocations[gl];
                returnMatrix[gl, 0] = location[0];
                returnMatrix[gl, 1] = location[1];
            }

            return returnMatrix;
        }

        /// <summary>
        /// Returns a matrix that is used to transform a spectrum having linear Hz scale into a spectrum having an octave freq scale.
        /// The returned matrix is size N x 2, where N = the length of the output spectrum.
        /// In fact the op spectrum has a split Herz scale - bottom part linear, top part octave scaled.
        /// Column 0 of the returned matrix contains the index into linear spectrum.
        /// Column 1 of the returned matrix contains the Hertz value of the corresponding index into the linear spectrum.
        /// </summary>
        public static int[,] LinearToSplitLinearOctaveScale(int nyquist, int frameSize, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {
            // Get the linear freq scale.
            int inputSpectrumSize = frameSize / 2;
            var linearFreqScale = FrequencyScale.GetLinearFreqScale(nyquist, inputSpectrumSize);

            // Get the octave freq scale.
            var octaveBandsLowerBounds = GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);

            double freqStep = nyquist / (double)inputSpectrumSize;
            int topLinearIndex = (int)Math.Round(lowerFreqBound / freqStep);
            int opSpectrumSize = topLinearIndex + octaveBandsLowerBounds.GetLength(0);

            // set up the transform matrix.
            var splitLinearOctaveIndexBounds = new int[opSpectrumSize, 2];

            // fill in the linear part of the freq scale
            for (int i = 0; i <= topLinearIndex; i++)
            {
                splitLinearOctaveIndexBounds[i, 0] = i;
                splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[i]);
            }

            for (int i = topLinearIndex + 1; i < opSpectrumSize; i++)
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
            splitLinearOctaveIndexBounds[opSpectrumSize - 1, 0] = linearFreqScale.Length - 1;
            splitLinearOctaveIndexBounds[opSpectrumSize - 1, 1] = (int)Math.Round(linearFreqScale[linearFreqScale.Length - 1]);

            return splitLinearOctaveIndexBounds;
        }

        /// <summary>
        /// This method assumes that the linear spectrum is derived from a 512 frame with sr = 22050.
        /// It is a split linear-octave scale.
        /// The linear part is from 0-2 kHz with reduction by factor of 8.
        /// The octave part is obtained by setting octave divisions or tone count = 5.
        /// </summary>
        /// <returns>a frequency scale for spectral-data reduction purposes.</returns>
        public static FrequencyScale GetDataReductionScale(FrequencyScale scale)
        {
            int sr = 22050;
            int frameSize = 512;
            scale.Nyquist = sr / 2;
            scale.WindowSize = frameSize;

            // linear reduction of the lower spectrum from 0 - 2 kHz.
            scale.LinearBound = 2000;
            int linearReductionFactor = 8;

            // Reduction of upper spectrum 2-11 kHz: Octave count and tone steps within one octave.
            scale.ToneCount = 5;

            var octaveBandsLowerBounds = GetFractionalOctaveBands(scale.LinearBound, scale.Nyquist, scale.ToneCount);
            int spectrumBinCount = frameSize / 2;
            var linearFreqScale = FrequencyScale.GetLinearFreqScale(scale.Nyquist, spectrumBinCount);

            double linearBinWidth = scale.Nyquist / (double)spectrumBinCount;
            int topLinearIndex = (int)Math.Round(scale.LinearBound / linearBinWidth);

            // calculate number of bins in linear portion. +1 because going to finish up at end of linear portion.
            int linearReducedBinCount = (topLinearIndex / linearReductionFactor) + 1;
            int finalBinCount = linearReducedBinCount + octaveBandsLowerBounds.Length;
            var splitLinearOctaveIndexBounds = new int[finalBinCount, 2];

            // fill in the linear part of the freq scale
            int z = 1;
            while (splitLinearOctaveIndexBounds[z - 1, 1] < scale.LinearBound)
            {
                splitLinearOctaveIndexBounds[z, 0] = z * linearReductionFactor;
                splitLinearOctaveIndexBounds[z, 1] = (int)Math.Round(linearFreqScale[z * linearReductionFactor]);
                z++;
            }

            // fill in the octave part of the freq scale
            for (int i = linearReducedBinCount + 1; i < finalBinCount; i++)
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
            var linearFreqScale = FrequencyScale.GetLinearFreqScale(nyquist, binCount);

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
    }
}