// <copyright file="DSP_Filters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Tools.Wav;
    using SixLabors.ImageSharp;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// digital signal processing FILTERS methods
    /// </summary>
    public static partial class DspFilters
    {
        public const double Pi = Math.PI;

        public static void TestMethod_GenerateSignal1()
        {
            int sampleRate = 22050;
            double duration = 20; // signal duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            int windowSize = 512;
            var freqScale = new FrequencyScale(sampleRate / 2, windowSize, 1000);
            string path = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\SineSignal1.png";

            var recording = GenerateTestRecording(sampleRate, duration, harmonics, WaveType.Cosine);
            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.0,
                SourceFName = "Signal1",
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.12,
            };
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);

            // pick a row, any row
            var oneSpectrum = MatrixTools.GetRow(sonogram.Data, 40);
            oneSpectrum = DataTools.normalise(oneSpectrum);
            var peaks = DataTools.GetPeaks(oneSpectrum, 0.5);
            for (int i = 2; i < peaks.Length - 2; i++)
            {
                if (peaks[i])
                {
                    LoggedConsole.WriteLine($"bin ={freqScale.BinBounds[i, 0]},  Herz={freqScale.BinBounds[i, 1]}-{freqScale.BinBounds[i + 1, 1]}  ");
                }
            }

            if (peaks[11] && peaks[22] && peaks[45] && peaks[92] && peaks[185])
            {
                LoggedConsole.WriteSuccessLine("Spectral Peaks found at correct places");
            }
            else
            {
                LoggedConsole.WriteErrorLine("Spectral Peaks found at INCORRECT places");
            }

            foreach (int h in harmonics)
            {
                LoggedConsole.WriteLine($"Harmonic {h}Herz  should be in bin  {freqScale.GetBinIdForHerzValue(h)}");
            }

            // spectrogram without framing, annotation etc
            var image = sonogram.GetImage();
            string title = $"Spectrogram of Harmonics: {DataTools.Array2String(harmonics)}   SR={sampleRate}  Window={windowSize}";
            image = sonogram.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            image.Save(path);
        }

        public static void TestMethod_GenerateSignal2()
        {
            int sampleRate = 64000;
            double duration = 30; // signal duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);
            string path = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\SineSignal2.png";
            var recording = GenerateTestRecording(sampleRate, duration, harmonics, WaveType.Cosine);

            // init the default sonogram config
            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.2,
                SourceFName = "Signal2",
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // pick a row, any row
            var oneSpectrum = MatrixTools.GetRow(sonogram.Data, 40);
            oneSpectrum = DataTools.normalise(oneSpectrum);
            var peaks = DataTools.GetPeaks(oneSpectrum, 0.5);

            var peakIds = new List<int>();
            for (int i = 5; i < peaks.Length - 5; i++)
            {
                if (peaks[i])
                {
                    int peakId = freqScale.BinBounds[i, 0];
                    peakIds.Add(peakId);
                    LoggedConsole.WriteLine($"Spectral peak located in bin {peakId},  Herz={freqScale.BinBounds[i, 1]}");
                }
            }

            //if (peaks[129] && peaks[257] && peaks[513] && peaks[1025] && peaks[2049])
            if (peakIds[0] == 129 && peakIds[1] == 257 && peakIds[2] == 513 && peakIds[3] == 1025 && peakIds[4] == 2049)
            {
                LoggedConsole.WriteSuccessLine("Spectral Peaks found at correct places");
            }
            else
            {
                LoggedConsole.WriteErrorLine("Spectral Peaks found at INCORRECT places");
            }

            foreach (int h in harmonics)
            {
                LoggedConsole.WriteLine($"Harmonic {h}Hertz should be in bin {freqScale.GetBinIdForHerzValue(h)}");
            }

            // spectrogram without framing, annotation etc
            var image = sonogram.GetImage();
            string title = $"Spectrogram of Harmonics: {DataTools.Array2String(harmonics)}   SR={sampleRate}  Window={freqScale.WindowSize}";
            image = sonogram.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            image.Save(path);
        }

        public static AudioRecording GenerateTestRecording(int sampleRate, double duration, int[] harmonics, WaveType waveType)
        {
            var signal = GenerateTestSignal(sampleRate, duration, harmonics, waveType);

            var wr = new WavReader(signal, 1, 16, sampleRate);
            var recording = new AudioRecording(wr);
            return recording;
        }

        public static double[] GenerateTestSignal(int sampleRate, double duration, int[] harmonics, WaveType waveType)
        {
            double[] signal = null;
            if (waveType == WaveType.Cosine)
            {
                signal = GetSignalOfAddedCosines(sampleRate, duration, harmonics);
            }
            else if (waveType == WaveType.Sine)
            {
                signal = GetSignalOfAddedSines(sampleRate, duration, harmonics);
            }
            else
            {
                throw new ArgumentException("Unknown WaveType", nameof(waveType));
            }

            if (signal == null)
            {
                throw new Exception("A signal was not generated. Fatal error!");
            }

            return signal;
        }

        /// <summary>
        /// returns a digital signal having sample rate, duration and harmonic content passed by user.
        /// Harmonics array should contain Hertz values of harmonics. i.e. int[] harmonics = { 500, 1000, 2000, 4000 };
        /// Phase is not taken into account.
        /// Generate Cos waves rather than Sin because amplitude should return to 1.0 if done correctly.
        /// </summary>
        /// <param name="sampleRate">sr of output signal</param>
        /// <param name="duration">signal duration in seconds</param>
        /// <param name="freq">an array of frequency harmonics in Hertz</param>
        public static double[] GetSignalOfAddedCosines(int sampleRate, double duration, int[] freq)
        {
            double amplitude = 0.999 / freq.Length;
            int length = (int)(sampleRate * duration);
            double[] data = new double[length];
            int count = freq.Length;

            for (int i = 0; i < length; i++)
            {
                for (int f = 0; f < count; f++)
                {
                    //data[i] +=           Math.Cos(omega[f] * i);
                    data[i] += amplitude * Math.Cos(2.0 * Math.PI * freq[f] * i / sampleRate);
                }
            }

            return data;
        }

        public static double[] GetSignalOfAddedSines(int sampleRate, double duration, int[] freq)
        {
            double amplitude = 1.0 / freq.Length;
            int length = (int)(sampleRate * duration);
            double[] data = new double[length];
            int count = freq.Length;

            for (int i = 0; i < length; i++)
            {
                for (int f = 0; f < count; f++)
                {
                    //data[i] +=  ampl   * Math.Sin(omega[f] * i);
                    data[i] += amplitude * Math.Sin(2.0 * Math.PI * freq[f] * i / sampleRate);
                }
            }

            return data;
        }

        /// <summary>
        /// The source signal for voiced speech, that is, the vibration generated by the glottis or vocal chords,
        /// has a spectral content with more power in low freq than in high. The spectrum has roll off of -6dB/octave.
        /// Many speech analysis methods work better when the souce signal is spectrally flattened.
        /// This is achieved by a high pass filter.
        /// </summary>
        public static double[] PreEmphasis(double[] signal, double coeff)
        {
            int length = signal.Length;
            double[] newSig = new double[length - 1];
            for (int i = 0; i < length - 1; i++)
            {
                newSig[i] = signal[i + 1] - (coeff * signal[i]);
            }

            return newSig;
        }

        public static double[] SubtractBaseline(double[] signal, int nh)
        {
            int sideNh = nh / 2;
            int length = signal.Length;
            double[] newSig = new double[length];
            for (int i = nh; i < length - nh; i++)
            {
                double[] subarray = DataTools.Subarray(signal, i - sideNh, nh);
                newSig[i] = signal[i] - subarray.Min();
            }

            return newSig;
        }

        /// <summary>
        /// This is ultracrude device but ................
        /// </summary>
        public static double[] AmplifyAndClip(double[] signal, double amplifificationFactor)
        {
            int length = signal.Length;
            double[] newSig = new double[length];
            for (int i = 0; i < length; i++)
            {
                newSig[i] = signal[i] * amplifificationFactor;

                if (Math.Abs(newSig[i]) > 1.0)
                {
                    newSig[i] = Math.Sign(signal[i]) * 1.0;
                }
            }

            return newSig;
        }

        /// <summary>
        /// converts passed arguments into step decay and step radians ie radians per sample or OMEGA
        /// </summary>
        /// <param name="signal">the signal</param>
        /// <param name="sf">sampling frequency</param>
        /// <param name="tHalf">half life in seconds</param>
        /// <param name="period">of the cycle of interest</param>
        /// <param name="filterDuration">length of filter in seconds</param>
        public static double[] Filter_DecayingSinusoid(double[] signal, double sf, double tHalf, double period, double filterDuration)
        {
            double samplesPerTHalf = tHalf * sf;
            double stepDecay = 0.5 / samplesPerTHalf;
            double samplesPerPeriod = period * sf;
            double stepRadians = 2 * Pi / samplesPerPeriod;
            int filterLength = (int)(filterDuration * sf);
            double[] newSig = Filter_DecayingSinusoid(signal, stepDecay, stepRadians, filterLength);
            return newSig;
        }

        public static double[] Filter_DecayingSinusoid(double[] signal, double stepDecay, double stepRadians, int filterLength)
        {
            double beta = stepDecay; // beta = decay per signal sample
            double omega = stepRadians; // OMEGA = radians per signal sample

            double[] coeff = new double[filterLength];
            int signalLength = signal.Length;
            double[] newSig = new double[signalLength];

            // set up the coefficients
            for (int n = 0; n < filterLength; n++)
            {
                double angle = omega * n;
                double decay = beta * n;
                coeff[filterLength - n - 1] = Math.Cos(angle) * Math.Exp(-decay);
            }

            // transfer initial partially filtered values
            for (int i = 0; i < filterLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < filterLength; j++)
                {
                    if (i - j < 0)
                    {
                        break;
                    }

                    sum += coeff[filterLength - j - 1] * signal[i - j];
                }

                newSig[i] = sum;
            }

            // transfer filtered values
            for (int i = filterLength; i < signalLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < filterLength; j++)
                {
                    sum += coeff[filterLength - j - 1] * signal[i - j];
                }

                newSig[i] = sum;
            }

            //System.LoggedConsole.WriteLine("FilterGain="+DSP.GetGain(coeff));
            return newSig;
        } //Filter_DecayingSinusoid()

        /// <summary>
        /// A "finite impulse response" (FIR) filter uses only the input signals,
        /// while an "infinite impulse response" filter (IIR) uses
        /// both the input signal and previous samples of the output signal.
        ///
        /// FIR filters are always stable, while IIR filters may be unstable.
        /// This filter is linear, causal and time-invariant.
        /// </summary>
        /// <param name="signal">input signal</param>
        /// <param name="filterCoeff">filter coefficients</param>
        /// <returns>the filtered signal</returns>
        public static double[] FIR_Filter(double[] signal, double[] filterCoeff)
        {
            int signalLength = signal.Length;
            double[] newSig = new double[signalLength];

            int filterLength = filterCoeff.Length;

            // transfer initial partially filtered values
            for (int i = 0; i < filterLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < filterLength; j++)
                {
                    if (i - j < 0)
                    {
                        break;
                    }

                    sum += filterCoeff[filterLength - j - 1] * signal[i - j];
                }

                newSig[i] = sum;
            }

            // transfer filtered values
            for (int i = filterLength; i < signalLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < filterLength; j++)
                {
                    sum += filterCoeff[filterLength - j - 1] * signal[i - j];
                }

                newSig[i] = sum;
            }

            return newSig;
        } //FIR_Filter()

        public static double GetGain(double[] filterCoeff)
        {
            int filterLength = filterCoeff.Length;

            //set up the impulse signal
            double[] impulse = new double[3 * filterLength];
            impulse[filterLength] = 1.0;
            double[] newSig = FIR_Filter(impulse, filterCoeff);
            double gain = 0.0;
            for (int j = 0; j < impulse.Length; j++)
            {
                gain += newSig[j];
            }

            return gain;
        }

        public static void DisplaySignal(double[] sig)
        {
            double[] newSig = DataTools.normalise(sig);

            foreach (double value in newSig)
            {
                int count = (int)(value * 50);
                for (int i = 0; i < count; i++)
                {
                    LoggedConsole.Write("=");
                }

                LoggedConsole.WriteLine("=");
            }
        }

        public static void DisplaySignal(double[] sig, bool showIndex)
        {
            double[] newSig = DataTools.normalise(sig);

            for (int n = 0; n < sig.Length; n++)
            {
                if (showIndex)
                {
                    LoggedConsole.Write(n.ToString("D3") + "|");
                }

                int count = (int)(newSig[n] * 50);
                for (int i = 0; i < count; i++)
                {
                    LoggedConsole.Write("=");
                }

                LoggedConsole.WriteLine("=");
            }
        }

        public static void Tests()
        {
            if (true)
            {
                LoggedConsole.WriteLine("\nTest of Filter_DecayingSinusoid()");
                double sf = 100;
                double tHalf = 0.2; //seconds
                double period = 0.2; //seconds
                double filterDuration = 1.0; //seconds
                int signalLength = 100;

                //set up the impulse signal
                double[] signal = new double[signalLength];
                signal[10] = 1.0;
                double[] newSig = Filter_DecayingSinusoid(signal, sf, tHalf, period, filterDuration);
                DisplaySignal(newSig, true);
            }

            if (true)
            {
                //test Filter_DecayingSinusoid()
                LoggedConsole.WriteLine("\nTest of Filter_DecayingSinusoid()");
                int signalLength = 100;

                //set up the impulse signal
                double[] signal = new double[signalLength];
                signal[10] = 1.0;

                //filter constatns
                double stepDecay = 0.05;
                double stepRadians = 0.4;
                int filterLength = 50; //number of time delays or coefficients in the filter
                double[] newSig = Filter_DecayingSinusoid(signal, stepDecay, stepRadians, filterLength);
                DisplaySignal(newSig, true);
            }
        }
    }
}
