// <copyright file="AudioRecording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.WavTools
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools.Wav;
    using DSP;
    using TowseyLibrary;

    public class AudioRecording : IDisposable
    {
        private readonly WavReader wavReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioRecording"/> class.
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(WavReader wavReader)
        {
            this.wavReader = wavReader;
        }

        public AudioRecording(FileInfo audioFile)
            : this(audioFile.FullName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioRecording"/> class.
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(byte[] bytes)
        {
            this.FilePath = "UNKNOWN";
            this.Bytes = bytes;
            if (this.Bytes != null)
            {
                this.wavReader = new WavReader(bytes);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioRecording"/> class.
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(string path)
        {
            this.FilePath = path;
            this.BaseName = Path.GetFileNameWithoutExtension(path);
            this.wavReader = new WavReader(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioRecording"/> class.
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(byte[] bytes, string name)
        {
            this.FilePath = name;
            this.BaseName = Path.GetFileNameWithoutExtension(name);
            this.Bytes = bytes;
            if (this.Bytes != null)
            {
                this.wavReader = new WavReader(bytes);
            }
        }

        /// <summary>
        /// Gets the file name without the extension
        /// </summary>
        public string BaseName { get; private set; }

        public string FilePath { get; private set; }

        public byte[] Bytes { get; set; }

        public int SampleRate
        {
            get
            {
                if (this.wavReader != null)
                {
                    return this.wavReader.SampleRate;
                }

                return -int.MaxValue;
            }
        }

        public int Nyquist
        {
            get
            {
                if (this.wavReader != null)
                {
                    return this.wavReader.SampleRate / 2;
                }

                return -int.MaxValue;
            }
        }

        public int BitsPerSample
        {
            get
            {
                if (this.wavReader != null)
                {
                    return this.wavReader.BitsPerSample;
                }

                return -int.MaxValue;
            }
        }

        public double Epsilon
        {
            get
            {
                if (this.wavReader != null)
                {
                    return this.wavReader.Epsilon;
                }

                return -double.MaxValue;
            }
        }

        /// <summary>
        /// Gets a wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public WavReader WavReader => this.wavReader;

        /// <summary>
        /// Gets returns Time Span of the recording
        /// </summary>
        public TimeSpan Duration => this.WavReader.Time;

        public static AudioRecording Filter_IIR(AudioRecording audio, string filterName)
        {
            DSP_IIRFilter filter = new DSP_IIRFilter(filterName);
            double[] output;
            filter.ApplyIIRFilter(audio.wavReader.Samples, out output);
            int channels = audio.wavReader.Channels;
            int bitsPerSample = audio.BitsPerSample;
            int sampleRate = audio.SampleRate;
            WavReader wr = new WavReader(output, channels, bitsPerSample, sampleRate);
            var ar = new AudioRecording(wr);
            return ar;
        }

        /// <summary>
        /// returns the wave form representation of the signal
        /// </summary>
        public double[,] GetWaveForm(int length)
        {
            double[,] envelope = new double[2, length];

            //get the signal samples
            var wavData = this.WavReader;
            var data = wavData.Samples;
            int sampleCount = data.GetLength(0); // Number of samples in signal
            int subSample = sampleCount / length;

            for (int w = 0; w < length; w++)
            {
                int start = w * subSample;
                int end = ((w + 1) * subSample) - 1;
                double min = double.MaxValue;
                double max = -double.MaxValue;
                for (int x = start; x < end; x++)
                {
                    if (min > data[x])
                    {
                        min = data[x];
                    }
                    else
                    if (max < data[x])
                    {
                        max = data[x];
                    }
                }

                envelope[0, w] = min;
                envelope[1, w] = max;
            }

            return envelope;
        }

        public double[,] GetWaveFormDecibels(int length, double dBMin)
        {
            double[,] wf = this.GetWaveForm(length);
            double[,] wfDecibels = new double[2, length];
            for (int w = 0; w < length; w++)
            {
                if (wf[0, w] >= -0.0001)
                {
                    wfDecibels[0, w] = dBMin;
                }
                else
                {
                    wfDecibels[0, w] = 10 * Math.Log10(Math.Abs(wf[0, w]));
                }

                if (wf[1, w] <= 0.0001)
                {
                    wfDecibels[1, w] = dBMin;
                }
                else
                {
                    wfDecibels[1, w] = 10 * Math.Log10(Math.Abs(wf[1, w]));
                }
            }

            return wfDecibels;
        }

        public Image GetWaveForm(int imageWidth, int imageHeight)
        {
            double[,] envelope = this.GetWaveForm(imageWidth);
            int halfHeight = imageHeight / 2;
            Color c = Color.FromArgb(10, 200, 255);

            //set up min, max, range for normalising of dB values
            Bitmap bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            for (int w = 0; w < imageWidth; w++)
            {
                int minId = halfHeight + (int)Math.Round(envelope[0, w] * halfHeight);
                int maxId = halfHeight + (int)Math.Round(envelope[1, w] * halfHeight);
                for (int z = minId; z < maxId; z++)
                {
                    bmp.SetPixel(w, imageHeight - z - 1, c);
                }

                bmp.SetPixel(w, halfHeight, c); //set zero line in case it was missed

                //mark clipping in red
                if (envelope[0, w] < -0.99)
                {
                    bmp.SetPixel(w, imageHeight - 1, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 2, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 3, Color.OrangeRed);
                }

                if (envelope[1, w] > 0.99)
                {
                    bmp.SetPixel(w, 0, Color.OrangeRed);
                    bmp.SetPixel(w, 1, Color.OrangeRed);
                    bmp.SetPixel(w, 2, Color.OrangeRed);
                }
            }

            return bmp;
        }

        public Image GetWaveFormInDecibels(int imageWidth, int imageHeight, double dBMin)
        {
            double[,] envelope = this.GetWaveFormDecibels(imageWidth, dBMin);

            //envelope values should all lie in [-40.0, 0.0].
            double slope = -(1 / dBMin);
            int halfHeight = imageHeight / 2;
            Color c = Color.FromArgb(0x6F, 0xa1, 0xdc);
            Color b = Color.FromArgb(0xd8, 0xeb, 0xff);

            //set up min, max, range for normalising of dB values

            Bitmap bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);

            for (int w = 0; w < imageWidth; w++)
            {
                //Convert log values to interval [0,1]
                double minLinear = (slope * envelope[0, w]) + 1.0;  // y = mx + c
                double maxLinear = (slope * envelope[1, w]) + 1.0;
                int minId = halfHeight - (int)Math.Round(minLinear * halfHeight);
                int maxId = halfHeight + (int)Math.Round(maxLinear * halfHeight);
                for (int z = 0; z < imageHeight; z++)
                {
                    if (z >= minId && z < maxId)
                    {
                        bmp.SetPixel(w, imageHeight - z - 1, c);
                    }
                    else
                    {
                        bmp.SetPixel(w, imageHeight - z - 1, b);
                    }
                }

                //LoggedConsole.WriteLine(envelope[0, w] + " >> " + maxLinear);
                //Console.ReadLine();

                bmp.SetPixel(w, halfHeight, c); //set zero line in case it was missed

                //mark clipping in red
                if (minLinear < -0.99)
                {
                    bmp.SetPixel(w, imageHeight - 1, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 2, Color.OrangeRed);
                    bmp.SetPixel(w, imageHeight - 3, Color.OrangeRed);
                }

                if (maxLinear > 0.99)
                {
                    bmp.SetPixel(w, 0, Color.OrangeRed);
                    bmp.SetPixel(w, 1, Color.OrangeRed);
                    bmp.SetPixel(w, 2, Color.OrangeRed);
                }
            }

            return bmp;
        }

        public void Dispose()
        {
            this.wavReader.Dispose();
        }

        //########################################################################################################################################################################
        //########################################################################################################################################################################
        //##  STATIC METHODS BELOW ###############################################################################################################################################
        //########################################################################################################################################################################
        //########################################################################################################################################################################

        /// <summary>
        /// TODO - this is long winded way to get file. Need to talk to Mark.
        /// </summary>
        public static AudioRecording GetAudioRecording(FileInfo sourceFile, int resampleRate, string opDir, string opFileName)
        {
            if (!sourceFile.Exists)
            {
                return null;
            }

            string opPath = Path.Combine(opDir, opFileName); //path location/name of extracted recording segment
            IAudioUtility audioUtility = new MasterAudioUtility();
            var info = audioUtility.Info(sourceFile); // Get duration of the source file
            int startMilliseconds = 0;
            int endMilliseconds = (int)info.Duration.Value.TotalMilliseconds;

            MasterAudioUtility.SegmentToWav(
                sourceFile,
                new FileInfo(opPath),
                new AudioUtilityRequest
                    {
                        TargetSampleRate = resampleRate,
                        OffsetStart = TimeSpan.FromMilliseconds(startMilliseconds),
                        OffsetEnd = TimeSpan.FromMilliseconds(endMilliseconds),
                    });

            return new AudioRecording(opPath);
        }

        /// <summary>
        /// This method extracts a recording segment and saves it to disk at the location fiOutputSegment.
        /// </summary>
        public static void ExtractSegment(FileInfo fiSource, TimeSpan start, TimeSpan end, TimeSpan buffer, int sampleRate, FileInfo fiOutputSegment)
        {
            // EXTRACT RECORDING SEGMENT
            int startMilliseconds = (int)(start.TotalMilliseconds - buffer.TotalMilliseconds);
            int endMilliseconds = (int)(end.TotalMilliseconds + buffer.TotalMilliseconds);
            if (startMilliseconds < 0)
            {
                startMilliseconds = 0;
            }

            ////if (endMilliseconds <= 0) endMilliseconds = (int)(segmentDuration * 60000) - 1;//no need to worry about end
            MasterAudioUtility.SegmentToWav(
                fiSource,
                fiOutputSegment,
                new AudioUtilityRequest
                    {
                        TargetSampleRate = sampleRate,
                        OffsetStart = TimeSpan.FromMilliseconds(startMilliseconds),
                        OffsetEnd = TimeSpan.FromMilliseconds(endMilliseconds),
                        ////Channel = 2 // set channel number or mixdowntomono=true  BUT NOT BOTH!!!
                        ////MixDownToMono  =true
                    });
        }

        public static FileInfo CreateTemporaryAudioFile(FileInfo sourceRecording, DirectoryInfo outDir, int resampleRate)
        {
            // put temp FileSegment in same directory as the required output image.
            var tempAudioSegment = new FileInfo(Path.Combine(outDir.FullName, "tempWavFile.wav"));

            // delete the temp audio file if it already exists.
            if (File.Exists(tempAudioSegment.FullName))
            {
                File.Delete(tempAudioSegment.FullName);
            }

            // This line creates a temporary version of the source file downsampled as per entry in the config file
            MasterAudioUtility.SegmentToWav(sourceRecording, tempAudioSegment, new AudioUtilityRequest() { TargetSampleRate = resampleRate });
            return tempAudioSegment;
        }

        /// <summary>
        /// returns a subsample of a recording with a buffer on either side.
        /// Main complication is dealing with edge effects.
        /// </summary>
        public static AudioRecording GetRecordingSubsegment(AudioRecording recording, int sampleStart, int sampleEnd, int sampleBuffer)
        {
            int signalLength = recording.WavReader.Samples.Length;
            int subsampleStart = sampleStart - sampleBuffer;
            int subsampleEnd = sampleEnd + sampleBuffer;
            int subsampleDuration = sampleEnd - sampleStart + 1 + (2 * sampleBuffer);
            if (subsampleStart < 0)
            {
                subsampleStart = 0;
                subsampleEnd = subsampleDuration - 1;
            }

            if (subsampleEnd >= signalLength)
            {
                subsampleEnd = signalLength - 1;
                subsampleStart = signalLength - subsampleDuration;
            }

            // catch case where subsampleDuration < recording length.
            if (subsampleStart < 0)
            {
                subsampleStart = 0;
            }

            int subsegmentSampleCount = subsampleEnd - subsampleStart + 1;
            var subsegmentRecording = recording;
            if (subsegmentSampleCount <= signalLength)
            {
                double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, subsampleStart, subsegmentSampleCount);
                var wr = new WavReader(subsamples, 1, 16, recording.SampleRate);
                subsegmentRecording = new AudioRecording(wr);
            }

            return subsegmentRecording;
        }
    }
}