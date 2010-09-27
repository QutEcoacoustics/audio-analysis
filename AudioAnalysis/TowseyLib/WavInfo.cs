// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WavInfo.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Wav Info.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TowseyLib
{
    using System;
    using System.Linq;

    /// <summary>
    /// Wav Info.
    /// </summary>
    public class WavInfo
    {
        /// <summary>
        /// Gets or sets Number of channels.
        /// </summary>
        public short Channels { get; set; }

        /// <summary>
        /// Gets or sets Sample rate of audio.
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets or sets Bits per sample.
        /// </summary>
        public short BitsPerSample { get; set; }

        /// <summary>
        /// Bytes Per Sample / Block Align.
        /// </summary>
        public short BytesPerSample { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public short CompressionCode { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public int BytesPerSecond { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public double[] Samples { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public short[][] SamplesSplit { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public double Epsilon
        {
            get { return Math.Pow(0.5, this.BitsPerSample - 1); }
        }

        public int Frames { get; set; }

        /// <summary>
        /// Create a sine wave.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="amp"></param>
        /// <param name="phase"></param>
        /// <param name="duration"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public WavInfo(double freq, double amp, double phase, TimeSpan duration, int sampleRate)
        {
            int length = (int)Math.Floor(duration.TotalSeconds * sampleRate);
            double[] data = new double[length];

            for (int i = 0; i < length; i++)
            {
                data[i] = amp * Math.Sin(phase + 2.0 * Math.PI * freq * i / sampleRate);
            }

            this.Channels = 1;
            this.BitsPerSample = 16;
            this.SampleRate = sampleRate;
            this.Samples = data;
            this.Duration = duration;
        }

        /// <summary>
        /// Create a Sine wav with multiple frequencies.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="amp"></param>
        /// <param name="phase"></param>
        /// <param name="duration"></param>
        /// <param name="sampleRate"></param>
        public WavInfo(double[] freq, double amp, double phase, TimeSpan duration, int sampleRate)
        {
            int length = (int)Math.Floor(duration.TotalSeconds * sampleRate);
            double[] data = new double[length];

            int count = freq.Length;

            for (int i = 0; i < length; i++)
            {
                for (int f = 0; f < count; f++)
                {
                    data[i] += (amp * Math.Sin(phase + 2.0 * Math.PI * freq[f] * i / (double)sampleRate));
                }
            }

            this.Channels = 1;
            this.BitsPerSample = 16;
            this.SampleRate = sampleRate;
            this.Samples = data;
            this.Duration = duration;
        }

        /// <summary>
        /// Create a wav info using data, channels, bits per sample and sample rate.
        /// </summary>
        /// <param name="rawData">Raw samples.</param>
        /// <param name="channels">Number of channels.</param>
        /// <param name="bitsPerSample">Bits per sample.</param>
        /// <param name="sampleRate">Sample rate.</param>
        public WavInfo(double[] rawData, short channels, short bitsPerSample, int sampleRate)
        {
            this.Channels = channels;
            this.BitsPerSample = bitsPerSample;
            this.SampleRate = sampleRate;
            this.Samples = rawData;
        }

        public WavInfo()
        {
        }

        /// <summary>
        /// Subsamples audio.
        /// </summary>
        /// <param name="interval">
        /// Keeps every <paramref name="interval"/> sample.
        /// </param>
        /// <param name="wavInfo">Wav info.</param>
        public static WavInfo SubSample(WavInfo wavInfo, int interval)
        {
            if (interval <= 1)
            {
                // no changes required.
                return wavInfo;
            }

            int length = wavInfo.Samples.Length;
            int newLength = length / interval;
            double[] newSamples = new double[newLength];

            //want length to be exact mulitple of interval
            length = newLength * interval;

            // copy only required samples
            for (int i = 0; i < newLength; i++)
            {
                newSamples[i] = wavInfo.Samples[i * interval];
            }

            return new WavInfo(newSamples, wavInfo.Channels, wavInfo.BitsPerSample, wavInfo.SampleRate / interval);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavInfo"></param>
        /// <returns></returns>
        public static double CalculateMaximumAmplitude(WavInfo wavInfo)
        {
            // Max expects at least 1 item in array.
            if (wavInfo.Samples.Length > 0)
            {
                return wavInfo.Samples.Max();
            }
            return 0;
        }
    }
}
