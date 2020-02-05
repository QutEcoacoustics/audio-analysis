// <copyright file="OctaveFreqScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using MathNet.Numerics;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

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
            int finalBinCount = 256;
            int sr, frameSize, octaveDivisions;

            // NOTE: octaveDivisions = the number of fractional Hz steps within one octave. Piano octave contains 12 steps per octave.

            FreqScaleType fst = scale.ScaleType;

            switch (fst)
            {
                case FreqScaleType.Linear62Octaves7Tones31Nyquist11025:
                    // constants required for split linear-octave scale when sr = 22050
                    sr = 22050;
                    frameSize = 8192;
                    scale.OctaveCount = 7;
                    octaveDivisions = 31; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.LinearBound = 62;
                    scale.Nyquist = 11025;
                    break;

                case FreqScaleType.Linear125Octaves6Tones30Nyquist11025:
                    // constants required for split linear-octave scale when sr = 22050
                    sr = 22050;
                    frameSize = 8192;
                    scale.OctaveCount = 6;
                    octaveDivisions = 32; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.LinearBound = 125;
                    scale.Nyquist = 11025;
                    break;

                case FreqScaleType.Octaves24Nyquist32000:
                    //// constants required for full octave scale when sr = 64000
                    sr = 64000;
                    frameSize = 16384;
                    scale.OctaveCount = 8;
                    octaveDivisions = 24; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.LinearBound = 15;
                    scale.Nyquist = 32000;
                    break;

                case FreqScaleType.Linear125Octaves7Tones28Nyquist32000:
                    // constants required for split linear-octave scale when sr = 64000
                    sr = 64000;
                    frameSize = 16384; // = 2*8192   or 4*4096;
                    scale.OctaveCount = 7;
                    octaveDivisions = 28; // tone steps within one octave. Note: piano = 12 steps per octave.
                    scale.LinearBound = 125;
                    scale.Nyquist = 32000;
                    break;

                default:
                    LoggedConsole.WriteErrorLine("WARNING: UNKNOWN OCTAVE SCALE.");
                    return;
            }

            scale.WindowSize = frameSize; // = 2*8192   or 4*4096
            scale.FinalBinCount = finalBinCount;
            scale.ToneCount = octaveDivisions;
            scale.BinBounds = LinearToSplitLinearOctaveScale(sr, frameSize, finalBinCount, scale.LinearBound, scale.Nyquist, scale.ToneCount);
            scale.GridLineLocations = GetGridLineLocations(fst, scale.BinBounds);
        }

        /// <summary>
        /// This method takes an audio recording and returns an octave scale spectrogram.
        /// At the present time it only works for recordings with 64000 sample rate and returns a 256 bin sonogram.
        /// TODO: generalise this method for other recordings and octave scales.
        /// </summary>
        public static BaseSonogram ConvertRecordingToOctaveScaleSonogram(AudioRecording recording, FreqScaleType fst)
        {
            var freqScale = new FrequencyScale(fst);
            double windowOverlap = 0.75;
            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = windowOverlap,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // Generate amplitude sonogram and then conver to octave scale
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);

            // THIS IS THE CRITICAL LINE.
            // TODO: SHOULD DEVELOP A SEPARATE UNIT TEST for this method
            sonogram.Data = ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            int windowSize = freqScale.FinalBinCount * 2;
            sonogram.Configuration.WindowSize = windowSize;
            sonogram.Configuration.WindowStep = (int)Math.Round(windowSize * (1 - windowOverlap));
            return sonogram;
        }

        public static double[,] ConvertAmplitudeSpectrogramToDecibelOctaveScale(double[,] inputSpgram, FrequencyScale freqScale)
        {
            //var dataMatrix = MatrixTools.Submatrix(inputSpgram, 0, 1, inputSpgram.GetLength(0) - 1, inputSpgram.GetLength(1) - 1);
            //square the values to produce power spectrogram
            var dataMatrix = MatrixTools.SquareValues(inputSpgram);

            //convert spectrogram to octave scale
            dataMatrix = ConvertLinearSpectrogramToOctaveFreqScale(dataMatrix, freqScale);
            dataMatrix = MatrixTools.Power2DeciBels(dataMatrix, out var min, out var max);
            return dataMatrix;
        }

        /// <summary>
        /// Converts a spectrogram having linear freq scale to one having an Octave freq scale.
        /// Note that the sample rate (sr) and the frame size both need to be apporpriate to the choice of FreqScaleType.
        /// TODO: SHOULD DEVELOP A SEPARATE UNIT TEST for this method
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

            //var octaveBinBounds = GetOctaveScale(freqScale.ScaleType);

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
        /// This method has been copied from a method of same name in the class MFCCStuff.cs and adapted to produce an octave freq scale.
        /// It transforms the amplitude spectrogram in the following steps:
        /// (1) It removes the DC row or bin 0 iff there is odd number of spectrogram bins. ASSUMPTION: Bin count should be power of 2 from FFT.
        /// (1) It converts spectral amplitudes to power, normalising for window power and sample rate.
        ///     The window contributes power to the signal which must subsequently be removed from the spectral power. Calculate power per sample.
        ///     See notes in the MFCCStuff.DecibelSpectra for further exp[lanaitons. These normalisations were adapted from MatLab MFCC code.
        /// (2) Then reduce the linear scale toan octave scale depending on the sr and required number of bins or filters.
        /// </summary>
        /// <param name="amplitudeM"> the amplitude spectra </param>
        /// <param name="windowPower">value for window power normalisation</param>
        /// <param name="sampleRate">to NormaliseMatrixValues for the sampling rate</param>
        /// <param name="epsilon">small value to avoid log of zero.</param>
        /// <param name="freqScale">the kind of frequency scale</param>
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
                case FreqScaleType.Linear62Octaves7Tones31Nyquist11025:
                    gridLineLocations = new int[8, 2];
                    LoggedConsole.WriteErrorLine("This Octave Scale does not currently have grid data provided.");
                    break;

                case FreqScaleType.Linear125Octaves6Tones30Nyquist11025:
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
                case FreqScaleType.Octaves24Nyquist32000:
                    gridLineLocations = new int[8, 2];
                    LoggedConsole.WriteErrorLine("This Octave Scale does not currently have grid data provided.");
                    break;

                case FreqScaleType.Linear125Octaves7Tones28Nyquist32000:
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
        /// Converts a single linear spectrum to octave scale spectrum
        /// </summary>
        public static double[] OctaveSpectrum(int[,] octaveBinBounds, double[] linearSpectrum)
        {
            int length = octaveBinBounds.GetLength(0);
            var octaveSpectrum = new double[length];
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

            // now fill in the first value of the octave spectrum
            int lowIndex1 = octaveBinBounds[0, 0];
            int centreIndex1 = octaveBinBounds[0, 0];
            int highIndex1 = octaveBinBounds[1, 0];
            octaveSpectrum[0] = FilterbankIntegral(linearSpectrum, lowIndex1, centreIndex1, highIndex1);

            // now fill in the last value of the octave spectrum
            int lowIndex2 = octaveBinBounds[length - 2, 0];
            int centreIndex2 = octaveBinBounds[length - 1, 0];
            int highIndex2 = octaveBinBounds[length - 1, 0];
            octaveSpectrum[length - 1] = FilterbankIntegral(linearSpectrum, lowIndex2, centreIndex2, highIndex2);
            return octaveSpectrum;
        }

        /// <summary>
        /// Returns the index bounds for a split herz scale - bottom part linear, top part octave scaled.
        /// </summary>
        public static int[,] LinearToSplitLinearOctaveScale(int sr, int frameSize, int finalBinCount, int lowerFreqBound, int upperFreqBound, int octaveDivisions)
        {
            var bandBounds = GetFractionalOctaveBands(lowerFreqBound, upperFreqBound, octaveDivisions);
            int nyquist = sr / 2;
            int binCount = frameSize / 2;
            var linearFreqScale = GetLinearFreqScale(nyquist, binCount);

            var splitLinearOctaveIndexBounds = new int[finalBinCount, 2];
            double freqStep = nyquist / (double)binCount;
            int topLinearIndex = (int)Math.Round(lowerFreqBound / freqStep) + 1;

            // fill in the linear part of the freq scale
            for (int i = 0; i < topLinearIndex; i++)
            {
                splitLinearOctaveIndexBounds[i, 0] = i;
                splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[i]);
            }

            for (int i = topLinearIndex; i < finalBinCount; i++)
            {
                for (int j = 0; j < linearFreqScale.Length; j++)
                {
                    if (linearFreqScale[j] > bandBounds[i - topLinearIndex])
                    {
                        splitLinearOctaveIndexBounds[i, 0] = j;
                        splitLinearOctaveIndexBounds[i, 1] = (int)Math.Round(linearFreqScale[j]);
                        break;
                    }
                }
            }

            // make sure last index has values
            splitLinearOctaveIndexBounds[finalBinCount - 1, 0] = linearFreqScale.Length - 1;
            splitLinearOctaveIndexBounds[finalBinCount - 1, 1] = (int)Math.Round(linearFreqScale[linearFreqScale.Length - 1]);

            // A HACK!!! Make sure second last index has values if they are zero
            if (splitLinearOctaveIndexBounds[finalBinCount - 2, 0] == 0)
            {
                splitLinearOctaveIndexBounds[finalBinCount - 2, 0] = linearFreqScale.Length - 1;
                splitLinearOctaveIndexBounds[finalBinCount - 2, 1] = (int)Math.Round(linearFreqScale[linearFreqScale.Length - 1]);
            }

            return splitLinearOctaveIndexBounds;
        }

        /// <summary>
        /// Returns the index bounds for a full octave scale - from lowest freq set by user to top freq.
        /// </summary>
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
            double[] freqBandCentres = { 15.625, 31.25, 62.5, 125, 250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000 };

            var list = new List<double>();

            for (int i = 0; i < freqBandCentres.Length; i++)
            {
                if (freqBandCentres[i] < minFreq)
                {
                    continue;
                }

                if (freqBandCentres[i] > maxFreq)
                {
                    break;
                }

                double[] fractionalOctaveBands = GetFractionalOctaveBands(freqBandCentres[i], octaveDivisions);

                for (int j = 0; j < octaveDivisions; j++)
                {
                    double floor = fractionalOctaveBands[j]; // sqrt2;
                    if (floor < minFreq)
                    {
                        continue;
                    }

                    list.Add(floor);
                }
            }

            return list.ToArray();
        }

        public static double[] GetFractionalOctaveBands(double lowerBound, int subbandCount)
        {
            double[] fractionalOctaveBands = new double[subbandCount];
            fractionalOctaveBands[0] = lowerBound;
            double exponent = 1 / (double)subbandCount;
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
        /// 1000    372
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

        /// <summary>
        /// Returns a simple spectrogram for test purposes.
        /// Write code for simple test. Different spectra tried so far:
        /// (1) Uniform spectrum = 1.0
        /// (2) Ramp spectrum
        /// (3) SPike spectrum
        /// </summary>
        public static double[] GetSimpleTestSpectrum(int sr, int frameSize)
        {
            // int nyquist = sr / 2;
            int binCount = frameSize / 2;
            double[] spectrum = new double[binCount];

            // return a linear frequency scale
            // double freqStep = nyquist / (double)binCount;
            for (int i = 0; i < binCount; i++)
            {
                // ramp spectrum
                //spectrum[i] = freqStep * i;

                //Uniform spectrum
                spectrum[i] = 1.0;
            }

            // Spike spectrum
            //spectrum[500] = 1.0;

            return spectrum;
        }

        public static void TestOctaveScale(FreqScaleType fst)
        {
            var freqScale = new FrequencyScale(fst);
            var octaveBinBounds = freqScale.BinBounds;

            // now test the octave scale using a test spectrum
            int sr = 22050;
            int frameSize = 8192; // default for sr = 22050

            if (fst == FreqScaleType.Octaves24Nyquist32000 || fst == FreqScaleType.Linear125Octaves7Tones28Nyquist32000)
            {
                sr = 64000;
                frameSize = 16384; // default for sr = 64000
            }

            // Get a simple test spectrum
            var linearSpectrum = GetSimpleTestSpectrum(sr, frameSize);

            //do the test
            var octaveSpectrum = OctaveSpectrum(octaveBinBounds, linearSpectrum);

            // write output
            int rowCount = octaveBinBounds.GetLength(0);
            for (int i = 0; i < rowCount; i++)
            {
                Console.WriteLine(i + "   bin-" + octaveBinBounds[i, 0] + "  " + octaveBinBounds[i, 1] + "Hz      " + octaveSpectrum[i]);
            }
        }
    }
}
