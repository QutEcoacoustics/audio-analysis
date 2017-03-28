﻿namespace AudioAnalysisTools.WavTools
{
    using System;
    using System.Collections.Generic;

    using Acoustics.Tools;
    using Acoustics.Tools.Wav;
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using TowseyLibrary;
    using AudioAnalysisTools.DSP;
    using AnalysisBase;


	public class AudioRecording : IDisposable
	{
        private readonly WavReader wavReader = null;

        #region Properties

        /// <summary>
        /// The file name without the extension
        /// </summary>
		public string BaseName { get; private set; }
        public string FilePath { get; private set; }
        public byte[] Bytes { get; set; }
        public int SampleRate    { get { if (wavReader != null) return wavReader.SampleRate;    else return -999; } }
        public int Nyquist       { get { if (wavReader != null) return wavReader.SampleRate/2;  else return -999; } }
        public int BitsPerSample { get { if (wavReader != null) return wavReader.BitsPerSample; else return -999; } }
        #endregion

        /// <summary>
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(byte[] bytes)
        {
            this.FilePath = "UNKNOWN";
            this.Bytes = bytes;
            if (Bytes != null) this.wavReader = new WavReader(bytes);
        }

        /// <summary>
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(string path)
        {
            this.FilePath  = path;
            this.BaseName  = Path.GetFileNameWithoutExtension(path);
            this.wavReader = new WavReader(path);
        }

        /// <summary>
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(byte[] bytes, string name)
        {
            this.FilePath = name;
            this.BaseName = Path.GetFileNameWithoutExtension(name);
            this.Bytes    = bytes;
            if (Bytes != null)
                this.wavReader = new WavReader(bytes);
        }

        /// <summary>
        /// Wrapper for the wav reader.
        /// Audio must be in wav format.
        /// Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public AudioRecording(WavReader wavReader)
        {
            this.wavReader = wavReader;
        }

        public AudioRecording(FileInfo audioFile) : this(audioFile.FullName)
        {
        }

        /// <summary>
        ///     Wrapper for the wav reader.
        ///     Audio must be in wav format.
        ///     Use MasterAudioUtility to convert or segment the audio first.
        /// </summary>
        public WavReader WavReader => this.wavReader;

	    /// <summary>
        /// NOTE: from Michael Towsey May 2014
        /// WARNING!!!! THIS METHOD IS UNSAFE AND SHOULD BE DEPRACATED
        /// It subsamples without first removing high frequency content.
        ///
        /// Reduces the signal sample rate to 22050Hz.
        /// Requires the existing signal to be either 44100Hz or 88200 Hz.
        /// </summary>
        //[Obsolete]
        //public void ConvertSampleRate22kHz()
        //{
        //    LoggedConsole.WriteError("ConvertSampleRate22kHz is deprecated - DO NOT USE");

        //    int sr = wavReader.SampleRate;
        //    if(sr == 22050) return; //signal already has required sr

        //    if (sr == 44100)
        //    {
        //        wavReader.SubSample(2);
        //        Log.WriteLine("Original signal Sample Rate=44100 - Downsampled to 22050.");
        //        return;
        //    }else
        //    if (sr == 88200)
        //    {
        //        wavReader.SubSample(4);
        //        Log.WriteLine("Original signal Sample Rate=88200 - Downsampled to 22050.");
        //        return;
        //    }
        //    Log.WriteLine("WARNING: Signal sample rate not 22050Hz and cannot reduce to this value.");
        //}

        /// <summary>
        /// returns Time Span of the recording
        /// </summary>
        /// <returns></returns>
        public TimeSpan Duration()
        {
            return WavReader.Time;
        }

        ///// <summary> OBSOLETE - SHOULD NEVER BE USED
        ///// Reduces the signal sample rate by a factor of N if sample rate.
        ///// Requires the existing signal to be either 44100Hz or 88200 Hz.
        ///// </summary>
        //public void ReduceSampleRateByFactor(int factor)
        //{
        //    int sr = wavReader.SampleRate;
        //    wavReader.SubSample(factor);
        //    Log.WriteLine("SIGNAL DOWN-SAMPLED: Original sample rate=" + sr + " >>> Downsampled to " + wavReader.SampleRate + ".");
        //}

        ///// <summary> OBSOLETE - SHOULD NEVER BE USED
        ///// Reduces the signal sample rate by a factor of N if sample rate exceed passed threshold.
        ///// Requires the existing signal to be either 44100Hz or 88200 Hz.
        ///// </summary>
        //public void ReduceSampleRateByFactor(int threshold, int factor)
        //{
        //    int sr = wavReader.SampleRate;
        //    if (sr > threshold)
        //    {
        //        wavReader.SubSample(factor);
        //        Log.WriteLine("SIGNAL DOWN-SAMPLED: Original sample rate={1} >>> Downsampled to {2}.", sr, wavReader.SampleRate);
        //        return;
        //    }
        //}

        //public void Filter_IIR(string filterName)
        //{
        //    DSP_IIRFilter filter = new DSP_IIRFilter(filterName);
        //    double[] output;
        //    filter.ApplyIIRFilter(this.wavReader.Samples, out output);
        //    //int channels = this.wavReader.Channels;
        //    //int bitsPerSample = this.BitsPerSample;
        //    //int sampleRate = this.SampleRate;
        //    //WavReader wr = new WavReader(output, channels, bitsPerSample, sampleRate);
        //    //var ar = new AudioRecording(wr);
        //    this.wavReader.Samples = output;
        //}

        static public AudioRecording Filter_IIR(AudioRecording audio, string filterName)
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
        /// <param name="length"></param>
        /// <returns></returns>
        public double[,] GetWaveForm(int length)
        {
            double[,] envelope = new double[2, length];

            //get the signal samples
            var wavData = WavReader;
            var data = wavData.Samples;
            int sampleCount = data.GetLength(0); // Number of samples in signal
            int subSample = sampleCount / length;

            for (int w = 0; w < length; w++)
            {
                int start = w * subSample;
                int end = ((w + 1) * subSample) - 1;
                double min = Double.MaxValue;
                double max = -Double.MaxValue;
                for (int x = start; x < end; x++)
                {
                    if (min > data[x]) min = data[x];
                    else
                    if (max < data[x]) max = data[x];
                }
                envelope[0, w] = min;
                envelope[1, w] = max;
            }

            return envelope;
        }

        public double[,] GetWaveFormDB(int length, double dBMin)
        {
            double[,] wf = GetWaveForm(length);
            double[,] wfDB = new double[2, length];
            for (int w = 0; w < length; w++)
            {
                if (wf[0, w] >= -0.0001) wfDB[0, w] = dBMin;
                else                     wfDB[0, w] = 10 * Math.Log10(Math.Abs(wf[0, w]));
                if (wf[1, w] <= 0.0001)  wfDB[1, w] = dBMin;
                else                     wfDB[1, w] = 10 * Math.Log10(Math.Abs(wf[1, w]));
                //LoggedConsole.WriteLine(wf[0, w] + " >> " + (wfDB[0, w]).ToString("F5"));
                //LoggedConsole.WriteLine(wf[1, w] + " >> " + (wfDB[1, w]).ToString("F5"));
                //Console.ReadLine();
            }
            return wfDB;
        }


        public Image GetWaveForm(int imageWidth, int imageHeight)
        {
            double[,] envelope = GetWaveForm(imageWidth);
            int halfHeight = imageHeight / 2;
            Color c = Color.FromArgb(10, 200, 255);

            //set up min, max, range for normalising of dB values
            Bitmap bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            for (int w = 0; w < imageWidth; w++)
            {
                int minID = halfHeight + (int)Math.Round(envelope[0, w] * halfHeight);
                int maxID = halfHeight + (int)Math.Round(envelope[1, w] * halfHeight);
                for (int z = minID; z < maxID; z++) bmp.SetPixel(w, imageHeight-z-1, c);
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


        public Image GetWaveFormDB(int imageWidth, int imageHeight, double dBMin)
        {
            double[,] envelope = GetWaveFormDB(imageWidth, dBMin);
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
                int minID = halfHeight - (int)Math.Round(minLinear * halfHeight);
                int maxID = halfHeight + (int)Math.Round(maxLinear * halfHeight);
                for (int z = 0; z < imageHeight; z++)
                {
                    if (z >= minID && z < maxID)
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

        //public AudioRecording ExportSignal(double startTime, double endTime)
        //{
        //    Log.WriteLine("AudioRecording.Extract()");
        //    int startIndex = (int)(startTime * this.SampleRate);
        //    int endIndex   = (int)(endTime   * this.SampleRate);
        //    Log.WriteLine("start=" + startTime.ToString("F1") + "s = " + startIndex);
        //    Log.WriteLine("end  =" + endTime.ToString("F1") + "s = " + endIndex);
        //    int sampleCount = endIndex - startIndex + 1;
        //    double[] signal = new double[sampleCount];
        //    //must multiply signal in [-1,+1] to signal in signed 16 bit integer range ie multiply by 2^15
        //    for (int i = 0; i < sampleCount; i++) signal[i] = this.wavReader.Samples[startIndex+i] * 32768; //65536
        //    //for (int i = 0; i < 100; i++) LoggedConsole.WriteLine(signal[i]); //debug check for integers
        //    int channels = 1;
        //    WavReader wav = new WavReader(signal, channels, this.BitsPerSample, this.SampleRate);
        //    var ar = new AudioRecording(wav);
        //    return ar;
        //}

        //public void Save(string path)
        //{
        //    // int sampleRate = 22050;
        //    //double duration = 30.245; //sig duration in seconds
        //    //int[] harmonics = { 500, 1000, 2000, 4000 };
        //    //double[] signal2 = DSP.GetSignal(sampleRate, duration, harmonics);
        //    //WavWriter.WriteWavFile(signal2, sampleRate, path);
        //    WavWriter.Write16bitWavFile(this.wavReader.Samples, this.SampleRate, path);
        //}


        #region IDisposable Members

        public void Dispose()
        {
            wavReader.Dispose();
        }

        #endregion



        //########################################################################################################################################################################
        //########################################################################################################################################################################
        //##  STATIC METHODS BELOW ###############################################################################################################################################
        //########################################################################################################################################################################
        //########################################################################################################################################################################

        /// <summary>
        /// returns an audio recording given a file path
        /// </summary>
        /// <param name="recordingPath"></param>
        /// <param name="resampleRate"></param>
        /// <returns></returns>
        //public static AudioRecording GetAudioRecording(string recordingPath, int resampleRate)
        //{
        //    //OLD CODE
        //    //AudioRecording recording = new AudioRecording(recordingPath);
        //    //if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
        //    //MORE OLD CODE
        //    //AudioRecording recording = new AudioRecording(recordingPath);
        //    //string filterName = "Chebyshev_Lowpass_5000";
        //    //recording.Filter_IIR(filterName); //filter audio recording.
        //    //recording.ReduceSampleRateByFactor(2);



        //    AudioRecording recording = new AudioRecording(recordingPath);

        //    // WRITE FILTERED SIGNAL IF NEED TO DEBUG
        //    //write the signal: IMPORTANT: ENSURE VALUES ARE IN RANGE -32768 to +32768
        //    //int bitRate = 16;
        //    //WavWriter.WriteWavFile(recording.GetWavReader().Samples, filteredRecording.SampleRate, bitRate, recordingPath + "filtered.wav");

        //    return recording;
        //}

        /// <summary>
        /// TODO - this is long winded way to get file. Need to talk to Mark.
        /// </summary>
        /// <param name="diOpDir"></param>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public static AudioRecording GetAudioRecording(FileInfo sourceFile, int resampleRate, string opDir, string opFileName)
        {
            if (! sourceFile.Exists) return null;
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
        /// <param name="fiSource"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="buffer"></param>
        /// <param name="resampleRate"></param>
        /// <param name="fiOutputSegment"></param>
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



    }// end class AudioRecording
}