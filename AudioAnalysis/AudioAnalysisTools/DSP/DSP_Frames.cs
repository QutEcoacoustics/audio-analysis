﻿namespace AudioAnalysisTools.DSP
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools.WavTools;
    //using MathNet.Numerics;using MathNet.Numerics.Transformations;
    using TowseyLibrary;

    /// <summary>
    /// digital signal processing methods
    /// </summary>
    public static class DSP_Frames
    {
        public const double Pi = Math.PI;


        public class EnvelopeAndFft
        {
            public double MinSignalValue { get; set; }
            public double MaxSignalValue { get; set; }
            public double[] Envelope { get; set; }
            public double[] FrameEnergy { get; set; }
            public double[,] AmplitudeSpectrogram { get; set; }
            public double WindowPower { get; set; }
            public double[] Average { get; set; }
            public int NyquistFreq { get; set; }
            public double FreqBinWidth { get; set; }
            public int NyquistBin { get; set; }
            public int MaxAmplitudeCount { get; set; }
            public int ClipCount { get; set; }
        }

        /// <summary>
        ///             //int frameStep = (int)(frameSize * (1 - overlap));
        /// </summary>
        /// <param name="windowSize"></param>
        /// <param name="windowOverlap"></param>
        /// <returns></returns>
        public static int FrameStep(int windowSize, double windowOverlap)
        {
            int step = (int)(windowSize * (1 - windowOverlap));
            return step;
        }

        public static int[,] FrameStartEnds(int dataLength, int frameSize, double windowOverlap)
        {
            int frameStep = (int)(frameSize * (1 - windowOverlap));
            return FrameStartEnds(dataLength, frameSize, frameStep);
        }

        /// <summary>
        /// returns the start and end index of all frames in a long audio signal
        /// </summary>
        public static int[,] FrameStartEnds(int dataLength, int frameSize, int frameStep)
        {

            if (frameStep < 1)
                throw new ArgumentException("Frame Step must be at least 1");
            if (frameStep > frameSize)
                throw new ArgumentException("Frame Step must be <=" + frameSize);

            int overlap = frameSize - frameStep;
            int framecount = (dataLength - overlap) / frameStep; // this truncates residual samples
            if (framecount < 2)
            {
                // NOTE: this exception was reenabled by Anthony because returning null makes it difficult to track down bugs
                throw new ArgumentException("Signal must produce at least two frames!");
                //return null;
            }

            int offset = 0;
            int[,] frames = new int[framecount, 2]; //col 0 =start; col 1 =end

            for (int i = 0; i < framecount; i++) //foreach frame
            {
                frames[i, 0] = offset;                  //start of frame
                frames[i, 1] = offset + frameSize - 1; //end of frame
                offset += frameStep;
            }
            return frames;
        }


        public static double[,] Frames(double[] data, int[,] startEnds)
        {
            int windowSize = startEnds[0, 1] + 1;
            int framecount = startEnds.GetLength(0);
            double[,] frames = new double[framecount, windowSize];

            for (int i = 0; i < framecount; i++) //for each frame
            {
                for (int j = 0; j < windowSize; j++) frames[i, j] = data[startEnds[i, 0] + j];
            } //end matrix
            return frames;
        }


        /// <summary>
        /// Breaks a long audio signal into frames with given step
        /// IMPORTANT: THIS METHOD PRODUCES A LARGE MEMORY-HUNGRY MATRIX.  BEST TO USE THE FrameStartEnds() METHOD.
        /// </summary>
        public static double[,] Frames(double[] data, int windowSize, double windowOverlap)
        {
            int step = (int)(windowSize * (1 - windowOverlap));

            if (step < 1)
                throw new ArgumentException("Frame Step must be at least 1");
            if (step > windowSize)
                throw new ArgumentException("Frame Step must be <=" + windowSize);

            int overlap = windowSize - step;
            int framecount = (data.Length - overlap) / step; //this truncates residual samples
            if (framecount < 2) throw new ArgumentException("Sonogram width must be at least 2");

            int offset = 0;
            double[,] frames = new double[framecount, windowSize];

            for (int i = 0; i < framecount; i++) //foreach frame
            {
                for (int j = 0; j < windowSize; j++) //foreach sample
                    frames[i, j] = data[offset + j];
                offset += step;
            } //end matrix
            return frames;
        }

        public static EnvelopeAndFft ExtractEnvelopeAndFFTs(AudioRecording recording, int frameSize, double overlap)
        {
            int frameStep = (int)(frameSize * (1 - overlap));
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            return ExtractEnvelopeAndFFTs(recording.WavReader.Samples, recording.SampleRate, epsilon, frameSize, frameStep);
        }

        public static EnvelopeAndFft ExtractEnvelopeAndFFTs(AudioRecording recording, int frameSize, int frameStep)
        {
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            return ExtractEnvelopeAndFFTs(recording.WavReader.Samples, recording.SampleRate, epsilon, frameSize, frameStep);
        }

        public static EnvelopeAndFft ExtractEnvelopeAndFFTs(double[] signal, int sampleRate, double epsilon, int frameSize, double overlap)
        {
            int frameStep = (int)(frameSize * (1 - overlap));
            return ExtractEnvelopeAndFFTs(signal, sampleRate, epsilon, frameSize, frameStep);
        }


        /// <summary>
        /// returns following values wrapped in class EnvelopeAndFft
        /// 1) the minimum and maximum signal values
        /// 2) the average of absolute amplitudes for each frame
        /// 3) the signal envelope as vector. i.e. the maximum of absolute amplitudes for each frame.
        /// 4) vector of frame energies
        /// 5) the high amplitdue and clipping counts
        /// 6) the signal amplitude spectrogram
        /// 7) the power of the FFT Window, i.e. sum of squared window values.
        /// 8) the nyquist
        /// 9) the width of freq bin in Hz
        /// 10) the byquist bin ID
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleRate"></param>
        /// <param name="epsilon"></param>
        /// <param name="frameSize"></param>
        /// <param name="frameStep"></param>
        /// <returns></returns>
        public static EnvelopeAndFft ExtractEnvelopeAndFFTs(double[] signal, int sampleRate, double epsilon, int frameSize, int frameStep)
        {
            //int frameStep = (int)(frameSize * (1 - overlap));
            int[,] frameIDs = DSP_Frames.FrameStartEnds(signal.Length, frameSize, frameStep);
            if (frameIDs == null) return null;
            int frameCount = frameIDs.GetLength(0);

            double[] average = new double[frameCount];
            double[] envelope = new double[frameCount];
            double[] frameEnergy = new double[frameCount];

            // set up the FFT parameters
            FFT.WindowFunc w = FFT.GetWindowFunction(FFT.Key_HammingWindow);
            var fft = new FFT(frameSize, w, true); // init class which calculates the MATLAB compatible .NET FFT
            double[,] spectrogram = new double[frameCount, fft.CoeffCount]; // init amplitude sonogram
            double minSignalValue = double.MaxValue;
            double maxSignalValue = double.MinValue;

            // cycle through the frames
            for (int i = 0; i < frameCount; i++)
            {
                int start = i * frameStep;
                int end = start + frameSize;

                // get average and envelope
                double frameDC = signal[start];
                double total = Math.Abs(signal[start]);
                double maxValue = total;
                double energy = 0;
                for (int x = start + 1; x < end; x++)
                {
                    if (signal[x] > maxSignalValue) maxSignalValue = signal[x];
                    if (signal[x] < minSignalValue) minSignalValue = signal[x];
                    frameDC += signal[x];
                    double absValue = Math.Abs(signal[x]);
                    total += absValue; // go through current frame to get signal (absolute) average
                    if (absValue > maxValue) maxValue = absValue;
                    energy += (signal[x] * signal[x]);
                }
                frameDC /= frameSize;
                average[i] = total / frameSize;
                envelope[i] = maxValue;
                frameEnergy[i] = energy / frameSize;

                // remove DC value from signal values
                double[] signalMinusAv = new double[frameSize];
                for (int j = 0; j < frameSize; j++)
                    signalMinusAv[j] = signal[start + j] - frameDC;

                // generate the spectra of FFT AMPLITUDES - NOTE: f[0]=DC;  f[64]=Nyquist
                var f1 = fft.InvokeDotNetFFT(signalMinusAv); // the fft

                ////f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
                ////f1 = fft.Invoke(DataTools.GetRow(frames, i));          //returns fft amplitude spectrum

                f1 = DataTools.filterMovingAverage(f1, 3); //smooth spectrum to reduce variance
                for (int j = 0; j < fft.CoeffCount; j++)   //foreach freq bin
                    spectrogram[i, j] = f1[j];             //transfer amplitude

            } // end frames

            // check the envelope for clipping. Accept a clip if two consecutive frames have max value = 1,0
            int maxAmplitudeCount, clipCount;
            Clipping.GetClippingCount(signal, envelope, frameStep, epsilon, out maxAmplitudeCount, out clipCount);

            // Remove the DC column ie column zero from amplitude spectrogram.
            double[,] amplSpectrogram = MatrixTools.Submatrix(spectrogram, 0, 1, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);

            int nyquistFreq = sampleRate / 2;
            double binWidth = nyquistFreq / (double)amplSpectrogram.GetLength(1);
            int nyquistBin = amplSpectrogram.GetLength(1) - 1;

            return new EnvelopeAndFft
            {
                MinSignalValue = minSignalValue,
                MaxSignalValue = maxSignalValue,
                Average = average,
                Envelope = envelope,
                FrameEnergy = frameEnergy,
                MaxAmplitudeCount = maxAmplitudeCount,
                ClipCount = clipCount,
                AmplitudeSpectrogram = amplSpectrogram,
                WindowPower = fft.WindowPower,
                NyquistFreq = nyquistFreq,
                FreqBinWidth = binWidth,
                NyquistBin = nyquistBin,
            };
        }


        /// <summary>
        /// Does same as the method above but returns values for octave scale spectrograms.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleRate"></param>
        /// <param name="epsilon"></param>
        /// <param name="frameSize"></param>
        /// <param name="frameStep"></param>
        /// <returns></returns>
        //public static EnvelopeAndFft ExtractEnvelopeAndFftForOctaveScale(double[] signal, int sampleRate, double epsilon, int frameSize, int frameStep)
        //{
        //    int[,] frameIDs = DSP_Frames.FrameStartEnds(signal.Length, frameSize, frameStep);
        //    if (frameIDs == null) return null;
        //    int frameCount = frameIDs.GetLength(0);

        //    double[] average = new double[frameCount];
        //    double[] envelope = new double[frameCount];
        //    double[] frameEnergy = new double[frameCount];

        //    // set up the FFT parameters
        //    FFT.WindowFunc w = FFT.GetWindowFunction(FFT.Key_HammingWindow);
        //    var fft = new FFT(frameSize, w, true); // init class which calculates the MATLAB compatible .NET FFT
        //    double[,] spectrogram = new double[frameCount, fft.CoeffCount]; // init amplitude sonogram
        //    double minSignalValue = double.MaxValue;
        //    double maxSignalValue = double.MinValue;

        //    // cycle through the frames
        //    for (int i = 0; i < frameCount; i++)
        //    {
        //        int start = i * frameStep;
        //        int end = start + frameSize;

        //        // get average and envelope
        //        double frameDC = signal[start];
        //        double total = Math.Abs(signal[start]);
        //        double maxValue = total;
        //        double energy = 0;
        //        for (int x = start + 1; x < end; x++)
        //        {
        //            if (signal[x] > maxSignalValue) maxSignalValue = signal[x];
        //            if (signal[x] < minSignalValue) minSignalValue = signal[x];
        //            frameDC += signal[x];
        //            double absValue = Math.Abs(signal[x]);
        //            total += absValue; // go through current frame to get signal (absolute) average
        //            if (absValue > maxValue) maxValue = absValue;
        //            energy += (signal[x] * signal[x]);
        //        }
        //        frameDC /= frameSize;
        //        average[i] = total / frameSize;
        //        envelope[i] = maxValue;
        //        frameEnergy[i] = energy / frameSize;

        //        // remove DC value from signal values
        //        double[] signalMinusAv = new double[frameSize];
        //        for (int j = 0; j < frameSize; j++)
        //            signalMinusAv[j] = signal[start + j] - frameDC;

        //        // generate the spectra of FFT AMPLITUDES - NOTE: f[0]=DC;  f[64]=Nyquist  
        //        var f1 = fft.InvokeDotNetFFT(signalMinusAv); // the fft
        //        ////f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
        //        ////f1 = fft.Invoke(DataTools.GetRow(frames, i));          //returns fft amplitude spectrum

        //        f1 = DataTools.filterMovingAverage(f1, 3); //smooth spectrum to reduce variance
        //        for (int j = 0; j < fft.CoeffCount; j++)   //foreach freq bin
        //            spectrogram[i, j] = f1[j];             //transfer amplitude

        //    } // end frames

        //    // check the envelope for clipping. Accept a clip if two consecutive frames have max value = 1,0
        //    int maxAmplitudeCount, clipCount;
        //    Clipping.GetClippingCount(signal, envelope, frameStep, epsilon, out maxAmplitudeCount, out clipCount);

        //    // Remove the DC column ie column zero from amplitude spectrogram.
        //    double[,] amplSpectrogram = MatrixTools.Submatrix(spectrogram, 0, 1, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);

        //    int nyquistFreq = sampleRate / 2;
        //    double binWidth = nyquistFreq / (double)amplSpectrogram.GetLength(1);
        //    int nyquistBin = amplSpectrogram.GetLength(1) - 1;

        //    return new EnvelopeAndFft
        //    {
        //        MinSignalValue = minSignalValue,
        //        MaxSignalValue = maxSignalValue,
        //        Average = average,
        //        Envelope = envelope,
        //        FrameEnergy = frameEnergy,
        //        MaxAmplitudeCount = maxAmplitudeCount,
        //        ClipCount = clipCount,
        //        AmplitudeSpectrogram = amplSpectrogram,
        //        WindowPower = fft.WindowPower,
        //        NyquistFreq = nyquistFreq,
        //        FreqBinWidth = binWidth,
        //        NyquistBin = nyquistBin
        //    };
        //}


        public static Tuple<double[], double[], double[], double[], double[]> ExtractEnvelopeAndZeroCrossings(double[] signal, int sr, int windowSize, double overlap)
        {
            int length = signal.Length;
            int frameOffset = (int)(windowSize * (1 - overlap));
            int frameCount = (length - windowSize + frameOffset) / frameOffset;
            double[] average = new double[frameCount];
            double[] envelope = new double[frameCount];
            double[] zeroCrossings = new double[frameCount]; // count of zero crossings
            double[] zcPeriod = new double[frameCount];      // sample count between zero crossings
            double[] sdPeriod = new double[frameCount];      // standard deviation of sample count between zc.
            for (int i = 0; i < frameCount; i++)
            {
                List<int> periodList = new List<int>();
                int start = i * frameOffset;
                int end = start + windowSize;

                //get average and envelope
                double maxValue = -Double.MaxValue;
                double total = signal[start];
                for (int x = start + 1; x < end; x++)
                {
                    total += signal[x]; // go through current frame to get signal average/DC
                    double absValue = Math.Abs(signal[x]);
                    if (absValue > maxValue) maxValue = absValue;
                }
                average[i]  = total / windowSize;
                envelope[i] = maxValue;

                //remove the average from signal
                double[] signalMinusAv = new double[windowSize];
                for (int j = 0; j < windowSize; j++)
                    signalMinusAv[j] = signal[start + j] - average[i];

                //get zero crossings and periods
                int zeroCrossingCount = 0;
                int prevLocation = 0;
                double prevValue = signalMinusAv[0];
                for (int j = 1; j < windowSize; j++) // go through current frame
                {
                    //double absValue = Math.Abs(signalMinusAv[j]);
                    if (signalMinusAv[j] * prevValue < 0.0) // ie zero crossing
                    {
                        if (zeroCrossingCount > 0) periodList.Add(j - prevLocation); // do not want to accumulate counts prior to first ZC.
                        zeroCrossingCount++; // count zero crossings
                        prevLocation = j;
                        prevValue = signalMinusAv[j];
                    }
                } // end current frame

                zeroCrossings[i] = zeroCrossingCount;
                int[] periods = periodList.ToArray();
                double av;
                double sd;
                NormalDist.AverageAndSD(periods, out av, out sd);
                zcPeriod[i] = av;
                sdPeriod[i] = sd;
            }
            return Tuple.Create(average, envelope, zeroCrossings, zcPeriod, sdPeriod);
        }


        public static int[] ConvertZeroCrossings2Hz(double[] zeroCrossings, int frameWidth, int sampleRate)
        {
            int length = zeroCrossings.Length;
            var freq = new int[length];
            for (int i = 0; i < length; i++) freq[i] = (int)(zeroCrossings[i] * sampleRate / 2 / frameWidth);
            return freq;
        }

        public static double[] ConvertSamples2Milliseconds(double[] sampleCounts, int sampleRate)
        {
            var tValues = new double[sampleCounts.Length];
            for (int i = 0; i < sampleCounts.Length; i++) tValues[i] = sampleCounts[i] * 1000 / sampleRate;
            return tValues;
        }




        /// <summary>
        /// returns the min and max values in each frame. Signal values range from -1 to +1.
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="minAmp"></param>
        /// <param name="maxAmp"></param>
        public static void SignalEnvelope(double[,] frames, out double[] minAmp, out double[] maxAmp)
        {
            int frameCount = frames.GetLength(0);
            int n  = frames.GetLength(1);
            minAmp = new double[frameCount];
            maxAmp = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double min =  Double.MaxValue;
                double max = -Double.MaxValue;
                for (int j = 0; j < n; j++)  //foreach sample in frame
                {
                    if (min > frames[i, j]) min = frames[i, j];
                    else
                    if (max < frames[i, j]) max = frames[i, j];
                }
                minAmp[i] = min;
                maxAmp[i] = max;
            }
        }


        /// <summary>
        /// counts the zero crossings in each frame
        /// This info is used for determing the begin and end points for vocalisations.
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static int[] ZeroCrossings(double[,] frames)
        {
            int frameCount = frames.GetLength(0);
            int n = frames.GetLength(1);
            int[] zc = new int[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                int count = 0;
                for (int j = 1; j < n; j++)  //foreach sample in frame
                {
                    count += Math.Abs(Math.Sign(frames[i, j]) - Math.Sign(frames[i, j - 1]));
                }
                zc[i] = count / 2;
            }
            return zc;
        }


        public static void DisplaySignal(double[] sig)
        {
                double[] newSig = DataTools.normalise(sig);

                foreach (double value in newSig)
                {
                    int count = (int)(value * 50);
                    for (int i = 0; i < count; i++) LoggedConsole.Write("=");
                    LoggedConsole.WriteLine("=");
                }
        }

        public static void DisplaySignal(double[] sig, bool showIndex)
        {
            double[] newSig = DataTools.normalise(sig);

            for (int n = 0; n < sig.Length; n++)
            {
                if (showIndex) LoggedConsole.Write(n.ToString("D3") + "|");
                int count = (int)(newSig[n] * 50);
                for (int i = 0; i < count; i++)
                {
                    LoggedConsole.Write("=");
                }
                LoggedConsole.WriteLine("=");
            }
        }
    }//end class DSP
}
