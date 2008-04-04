using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using TowseyLib;

namespace AudioStuff
{
	public sealed class Sonogram
	{
        private string wavFileDir;
        private string wavFileExt = ".wav"; //default value
        private string wavFName;
        public  string WavFName { get { return wavFName; } set { wavFName = value; } }
        
        private int windowSize;
        public int WindowSize { get { return windowSize; } }
        private double windowOverlap;
        public double WindowOverlap { get { return windowOverlap; } set { windowOverlap = value; } }
        private FFT.WindowFunc windowFnc;
        private string windowFncName;
        public string WindowFncName { get { return windowFncName; } set { windowFncName = value; } }
        private int sampleRate;
        public int SampleRate { get { return sampleRate; } }
        private int maxFreq;
        public int MaxFreq { get { return maxFreq; } }
        private int sampleCount;
        public int SampleCount { get { return sampleCount; } }
        private double audioDuration;
        public double AudioDuration { get { return audioDuration; } set { audioDuration = value; } }
        private int windowDuration;
        private int nonOverlapDuration;

        private int spectrumCount;
        public int SpectrumCount { get { return spectrumCount; } }
        private int freqBinCount;
        public int FreqBinCount { get { return freqBinCount; } }

        private double fBinWidth;
        public double FBinWidth { get { return fBinWidth; } }

        private double[,] matrix;
        public double[,] Matrix { get { return matrix; } /*set { matrix = value; }*/ }
        private double minP; //min power in sonogram
        public double MinP { get { return minP; } set { minP = value; } }
        private double maxP; //max power in sonogram
        public double MaxP { get { return maxP; } set { maxP = value; } }

        private double minPercentile;
        public double MinPercentile { get { return minPercentile; } set { minPercentile = value; } }
        private double maxPercentile;
        public double MaxPercentile { get { return maxPercentile; } set { maxPercentile = value; } }
        private double minCut; //power of min percentile
        public double MinCut { get { return minCut; } set { minCut = value; } }
        private double maxCut; //power of max percentile
        public double MaxCut { get { return maxCut; } set { maxCut = value; } }

        //freq bins of the scanned part of sonogram
        private int topScanBin;
        public int TopScanBin { get { return topScanBin; } set { topScanBin = value; } }
        private int midScanBin;
        public int MidScanBin { get { return midScanBin; } set { midScanBin = value; } }
        private int bottomScanBin;
        public int BottomScanBin { get { return bottomScanBin; } set { bottomScanBin = value; } }
        private int midTemplateFreq;
        public int MidTemplateFreq { get { return midTemplateFreq; } set { midTemplateFreq = value; } }

        private string sonogramDir;
        public string SonogramDir { get { return sonogramDir; } set { sonogramDir = value; } }
        private string bmpFName = null;
        public string BmpFName { get { return bmpFName; } set { bmpFName = value; } }
        private string bmpFileExt = ".bmp";//default value
        private bool addGrid;
        private int blurNH;
        private int blurNH_time;
        private int blurNH_freq;
        private bool normSonogram;

        private double noiseAv;
        public double NoiseAv { get { return noiseAv; } set { noiseAv = value; } }
        private double noiseSD;
        public double NoiseSD { get { return noiseSD; } set { noiseSD = value; } }
        private double zScoreThreshold;
        public double ZScoreThreshold { get { return zScoreThreshold; } set { zScoreThreshold = value; } }
        private int verbosity = 0;

        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="props"></param>
        /// <param name="wav"></param>
        public Sonogram(Params props, WavReader wav)
        {
            this.wavFileDir = wav.WavFileDir;
            this.wavFName = wav.WavFileName;
            this.wavFName = wavFName.Substring(0, wavFName.Length - 4);
            CopyParams(props);
            Make(wav);
            if(this.verbosity!=0) WriteInfo();
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavFName"></param>
        public Sonogram(string iniFName, string wavPath)
        {
            FileInfo fi = new FileInfo(wavPath);
            this.wavFileDir = fi.DirectoryName;
            this.wavFName = fi.Name.Substring(0,fi.Name.Length-4);
            this.wavFileExt = fi.Extension;

            TowseyLib.Params props = new TowseyLib.Params(iniFName);
            CopyParams(props);
            //read the .WAV file
            WavReader wav = new WavReader(wavPath);
            Make(wav);
            if (this.verbosity != 0) WriteInfo();
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavPath"></param>
        /// <param name="wavBytes"></param>
        /// <returns></returns>
        public Sonogram(string iniFName, string wavPath, byte[] wavBytes)
        {
            TowseyLib.Params props = new TowseyLib.Params(iniFName);

            FileInfo fi = new FileInfo(wavPath);
            this.wavFileDir = fi.DirectoryName;
            this.wavFName   = fi.Name.Substring(0, fi.Name.Length - 4);
            this.wavFileExt = fi.Extension;

            //initialise WAV class with bytes array
            WavReader wav = new WavReader(wavBytes, wavFName);
            Make(wav);
            if (this.verbosity != 0) WriteInfo();
        }

        private void CopyParams(Params props)
        {
            this.wavFileExt = props.GetValue("WAV_FILEEXT");
            this.sonogramDir = props.GetValue("SONOGRAM_DIR");
            this.windowSize = props.GetInt("WINDOW_SIZE");
            this.windowOverlap = props.GetDouble("WINDOW_OVERLAP");
            this.windowFncName = props.GetValue("WINDOW_FUNCTION");
            this.windowFnc = FFT.GetWindowFunction(windowFncName);
            this.minPercentile = props.GetDouble("MIN_PERCENTILE");
            this.maxPercentile = props.GetDouble("MAX_PERCENTILE");
            this.wavFileExt = props.GetValue("WAV_FILEEXT");
            this.bmpFileExt = props.GetValue("BMP_FILEEXT");
            this.addGrid = props.GetBoolean("ADDGRID");
            this.zScoreThreshold = props.GetDouble("ZSCORE_THRESHOLD");
            this.verbosity = props.GetInt("VERBOSITY");
            this.blurNH = props.GetInt("BLUR_NEIGHBOURHOOD");
            this.blurNH_time = props.GetInt("BLUR_TIME_NEIGHBOURHOOD");
            this.blurNH_freq = props.GetInt("BLUR_FREQ_NEIGHBOURHOOD");
            this.normSonogram = props.GetBoolean("NORMALISE_SONOGRAM");
        }

        private void Make(WavReader wav)
        {
            //string waveFileDir = props.GetValue("WAV_DIR");

            this.wavFName = wav.WavFileName;
            this.sampleRate = wav.SampleRate;
            this.sampleCount = wav.SampleLength;
            this.audioDuration = sampleCount / (double)sampleRate;
            this.maxFreq = sampleRate / 2;
            this.windowDuration = windowSize * 1000 / sampleRate;
            this.nonOverlapDuration = (int)(windowDuration * (1 - windowOverlap));
            this.freqBinCount = windowSize / 2;//other half is phase info
            this.fBinWidth = this.maxFreq / (double)freqBinCount;

            FFT fft = new FFT(windowSize, windowFnc);
            int step = (int)(windowSize * (1 - windowOverlap));

            this.matrix = GenerateSpectrogram(wav, fft, step, out minP, out maxP);
            this.matrix = DataTools.Blur(this.matrix, this.blurNH_freq, this.blurNH_time);
            //normalise and bound the values
            if (this.normSonogram) NormalizeAndBound();

            this.spectrumCount = this.matrix.GetLength(0);
        }

		public double[,] GenerateSpectrogram(WavReader wav, FFT fft, int step, out double min, out double max)
		{
			//if (width < 2) throw new ArgumentException("width must be at least 2");
            if (step < 1)
                throw new ArgumentException("Step must be at least 1");
            if (step > fft.WindowSize)
                throw new ArgumentException("Step must be <=" + fft.WindowSize);


			double[] data = wav.Samples;
            int width     = (data.Length - fft.WindowSize) / step;
			int height    = fft.WindowSize / 2;

			double offset = 0.0;
			double[,] sonogram = new double[width, height];
			min = Double.MaxValue; max = Double.MinValue;
			for (int i = 0; i < width; i++)//foreach time step
			{
				double[] f1 = fft.Invoke(data, (int)Math.Floor(offset));
                for (int j = 0; j < height; j++)//foreach freq bin
				{
					double bels = Math.Log10(f1[j + 1]);//convert to Bels
                    //NOTE: this should be the log of a ratio. 
                    // here we assume the reference power = 1.0.
                    if (bels < min) min = bels;
                    if (bels > max) max = bels;
                    sonogram[i, j] = bels;
				}
				offset += step;
			}
			if (min < wav.Epsilon) min = wav.Epsilon;
            return sonogram;
		}


		public double[] GetPowerSpectrum(double[,] f2, double max)
		{
			int width = f2.GetLength(0);
			int height = f2.GetLength(1);
			double[] f1 = new double[height];

			for (int y = 0; y < height; y++)
			{
				double sum = 0.0;
				for (int x = 0; x < width; x++)
					sum += Math.Pow(10.0, 2.0 * (f2[x, y] - max));
				f1[y] = Math.Log10(sum / width) + 2.0 * max;
			}
			return f1;
		}

		
        //public double Normalize(double[] data)
        //{
        //    double max = 0.0;
        //    for (int i = 0; i < data.Length; i++)
        //        max = Math.Max(max, Math.Abs(data[i]));
        //    max = 1.0 / max;
        //    for (int i = 0; i < data.Length; i++)
        //        data[i] *= max;
        //    return max;
        //}


        //normalise and compress/bound the values
        public void Normalize(double minPercentile, double maxPercentile)
        {
            this.minPercentile = minPercentile;
            this.maxPercentile = maxPercentile;
            DataTools.GetPercentileCutoffs(this.matrix, minP, maxP, minPercentile, maxPercentile, out minCut, out maxCut);
            this.matrix = DataTools.boundMatrix(this.matrix, minCut, maxCut);
        }
        //normalise and compress/bound the values
        public void NormalizeAndBound()
        {
            DataTools.GetPercentileCutoffs(this.matrix, minP, maxP, this.minPercentile, this.maxPercentile, out minCut, out maxCut);
            this.matrix = DataTools.boundMatrix(this.matrix, minCut, maxCut);
        }


        public void WriteInfo()
        {
            Console.WriteLine("\nSONOGRAM INFO");
            Console.WriteLine(" WavSampleRate=" + this.sampleRate + " SampleCount=" + sampleCount + "  Duration=" + (sampleCount / (double)sampleRate).ToString("F3") + "s");
            Console.WriteLine(" Window Size=" + this.windowSize + "  Max FFT Freq =" + maxFreq);
            Console.WriteLine(" Window Overlap=" + this.windowOverlap + " Window duration=" + windowDuration + "ms. (non-overlapped=" + nonOverlapDuration + "ms)");
            Console.WriteLine(" Freq Bin Width=" + (this.maxFreq / (double)freqBinCount).ToString("F3") + "hz");
            Console.WriteLine(" Min power=" + this.minP.ToString("F3") + "  Max power=" + this.maxP.ToString("F3"));
            Console.WriteLine(" Min percentile=" + this.minPercentile.ToString("F2") + "  Max percentile=" + this.maxPercentile.ToString("F2"));
            Console.WriteLine(" Min cutoff=" + this.minCut.ToString("F3") + "  Max cutoff=" + this.maxCut.ToString("F3"));
        }


        public Bitmap GetImage(bool addGrid)
        {
            // prepare Bitmap image
            BitMaps bmps = new BitMaps(SampleRate, AudioDuration, 0.0);
            Bitmap bmp = bmps.CreateBitmap(matrix, minCut, maxCut, addGrid, null, 0, 0);
            return bmp;
        }
        public Bitmap GetImage(bool addGrid, double[] scoreArray, double threshold)
        {
            BitMaps bmps = new BitMaps(SampleRate, AudioDuration, threshold);
            Bitmap bmp = bmps.CreateBitmap(matrix, minCut, maxCut,
                                    addGrid, scoreArray, topScanBin, bottomScanBin);
            return bmp;
        }

        public void SaveImage()
        {
            string imageFileName = this.sonogramDir + this.wavFName + this.bmpFileExt;
            this.bmpFName = imageFileName;
            Bitmap bmp = GetImage(this.addGrid);
            bmp.Save(imageFileName);
        }
        public void SaveImage(double[] scoreArray)
        {
            string imageFileName = this.sonogramDir + this.wavFName + this.bmpFileExt;
            this.bmpFName = imageFileName;
            Bitmap bmp = GetImage(this.addGrid, scoreArray, this.zScoreThreshold);
            bmp.Save(imageFileName);
        }


    } //end class Sonogram





    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************

	public sealed class FFT
	{
		public delegate double WindowFunc(int n, int N);

		//public int WindowSize { get; private set; }
		private int windowSize;
		public int WindowSize { get { return windowSize; } private set { windowSize = value; } }

		//public double[] WindowWeights { get; private set; }
		private double[] windowWeights;
		public double[] WindowWeights { get { return windowWeights; } private set { windowWeights = value; } }

		public FFT(int windowSize)
			: this(windowSize, null)
		{
		}

		public FFT(int windowSize, WindowFunc w)
		{
			if (!IsPowerOf2(windowSize)) throw new ArgumentException("WindowSize must be a power of 2.");

			this.WindowSize = windowSize;
			if (w != null)
			{
				this.WindowWeights = new double[windowSize];
				for (int i = 0; i < windowSize; i++)
					this.WindowWeights[i] = w(i, windowSize);
			}
		}

		public double[] Invoke(double[] data, int offset)
		{
			double[] cdata = new double[2 * WindowSize];
			if (WindowWeights != null)
				for (int i = 0; i < WindowSize; i++)
					cdata[2 * i] = WindowWeights[i] * data[offset + i];
			else
				for (int i = 0; i < WindowSize; i++)
					cdata[2 * i] = data[offset + i];

			four1(cdata);

			double[] f = new double[WindowSize / 2 + 1];
			for (int i = 0; i < WindowSize / 2 + 1; i++)
				f[i] = hypot(cdata[2 * i], cdata[2 * i + 1]);
			return f;
		}

		private static double hypot(double x, double y)
		{
			return Math.Sqrt(x * x + y * y);
		}

		// from http://www.nrbook.com/a/bookcpdf/c12-2.pdf
		private static void four1(double[] data)
		{
			int nn = data.Length / 2;
			int n = nn << 1;
			int j = 1;
			for (int i = 1; i < n; i += 2)
			{
				if (j > i)
				{
					double tmp;
					tmp = data[j - 1];
					data[j - 1] = data[i - 1];
					data[i - 1] = tmp;
					tmp = data[j];
					data[j] = data[i];
					data[i] = tmp;
				}
				int m = nn;
				while (m >= 2 && j > m)
				{
					j -= m;
					m >>= 1;
				}
				j += m;
			}

			int mmax = 2;
			while (n > mmax)
			{
				int istep = mmax << 1;
				double theta = 2.0 * Math.PI / mmax;
				double wtemp = Math.Sin(0.5 * theta);
				double wpr = -2.0 * wtemp * wtemp;
				double wpi = Math.Sin(theta);
				double wr = 1.0;
				double wi = 0.0;
				for (int m = 1; m < mmax; m += 2)
				{
					for (int i = m; i <= n; i += istep)
					{
						j = i + mmax;
						double tempr = wr * data[j - 1] - wi * data[j];
						double tempi = wr * data[j] + wi * data[j - 1];
						data[j - 1] = data[i - 1] - tempr;
						data[j] = data[i] - tempi;
						data[i - 1] += tempr;
						data[i] += tempi;
					}
					wr = (wtemp = wr) * wpr - wi * wpi + wr;
					wi = wi * wpr + wtemp * wpi + wi;
				}
				mmax = istep;
			}
		}

		#region Window functions
		// from http://en.wikipedia.org/wiki/Window_function

		public static readonly WindowFunc Hamming = delegate(int n, int N)
		{
			double x = 2.0 * Math.PI * n / (N - 1);
			return 0.53836 - 0.46164 * Math.Cos(x);
		};

		public static WindowFunc Gauss(double sigma)
		{
			if (sigma <= 0.0 || sigma > 0.5) throw new ArgumentOutOfRangeException("sigma");
			return delegate(int n, int N)
			{
				double num = n - 0.5 * (N - 1);
				double den = sigma * 0.5 * (N - 1);
				double quot = num / den;
				return Math.Exp(-0.5 * quot * quot);
			};
		}

		public static readonly WindowFunc Lanczos = delegate(int n, int N) {
			double x = 2.0 * n / (N - 1) - 1.0;
			return x != 0.0 ? Math.Sin(x) / x : 1.0;
		};

		public static readonly WindowFunc Nuttall = delegate(int n, int N) { return lrw(0.355768, 0.487396, 0.144232, 0.012604, n, N); };

		public static readonly WindowFunc BlackmanHarris = delegate(int n, int N) { return lrw(0.35875, 0.48829, 0.14128, 0.01168, n, N); };

		public static readonly WindowFunc BlackmanNuttall = delegate(int n, int N) { return lrw(0.3635819, 0.4891775, 0.1365995, 0.0106411, n, N); };

		private static double lrw(double a0, double a1, double a2, double a3, int n, int N)
		{
			double c1 = Math.Cos(2.0 * Math.PI * n / (N - 1));
			double c2 = Math.Cos(4.0 * Math.PI * n / (N - 1));
			double c3 = Math.Cos(6.0 * Math.PI * n / (N - 1));
			return a0 - a1 * c1 + a2 * c2 - a3 * c3;
		}

		public static readonly WindowFunc FlatTop = delegate(int n, int N) {
			double c1 = Math.Cos(2.0 * Math.PI * n / (N - 1));
			double c2 = Math.Cos(4.0 * Math.PI * n / (N - 1));
			double c3 = Math.Cos(6.0 * Math.PI * n / (N - 1));
			double c4 = Math.Cos(8.0 * Math.PI * n / (N - 1));
			return 1.0 - 1.93 * c1 + 1.29 * c2 - 0.388 * c3 + 0.032 * c4;
		};
		#endregion

		private static bool IsPowerOf2(int n)
		{
			while (n > 1)
			{
				if (n == 2) return true;
				n >>= 1;
			}
			return false;
		}

        public static FFT.WindowFunc GetWindowFunction(string name)
        {
            //FFT.WindowFunc windowFnc;
            if(name.StartsWith("Hamming")) return FFT.Hamming;
            else return null;
        }
	}



    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
 
    
    
    public sealed class WavReader
	{
		//declare variables, getters and setters
        private int channels;
		public int Channels { get { return channels; } private set { channels = value; } }
		private int sampleRate;
		public int SampleRate   { get { return sampleRate; }   private set { sampleRate   = value; } }
        private int sampleLength;
        public int SampleLength { get { return sampleLength; } private set { sampleLength = value; } }
		private int bitsPerSample;
		public int BitsPerSample { get { return bitsPerSample; } private set { bitsPerSample = value; } }
		private double[] samples;
		public double[] Samples { get { return samples; } private set { samples = value; } }
        
        private string wavFileDir;
        public string WavFileDir { get { return wavFileDir; } private set { wavFileDir = value; } }
        private string wavFileName;
        public string WavFileName { get { return wavFileName; } private set { wavFileName = value; } }


		public TimeSpan Time
		{
			get { return TimeSpan.FromSeconds(((double)Samples.Length) / SampleRate); }
		}

		public double Epsilon
		{
			get { return Math.Log10(Math.Pow(0.5, bitsPerSample - 1)); }
		}

        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="waveFileDir"></param>
        /// <param name="wavFileName"></param>
        public WavReader(string wavFileDir, string wavFileName, string wavFileExt)
        {
            this.wavFileDir = wavFileDir;
            this.wavFileName = wavFileName;
            string path = wavFileDir + wavFileName + wavFileExt;
            ParseData(File.ReadAllBytes(path));
        }
        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="waveFileDir"></param>
        /// <param name="wavFileName"></param>
        public WavReader(string wavPath)
        {
            FileInfo fi = new FileInfo(wavPath);
            this.wavFileDir = fi.DirectoryName;
            this.wavFileName = fi.Name;
            this.wavFileName = wavFileName.Substring(0, wavFileName.Length - 4);
            ParseData(File.ReadAllBytes(wavPath));
        }
        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="wavData"></param>
        public WavReader(byte[] wavData)
        {
            ParseData(wavData);
        }
        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="wavBytes"></param>
        /// <param name="wavFName"></param>
        public WavReader(byte[] wavBytes, string wavFName)
        {
            this.wavFileName = wavFName;
            ParseData(wavBytes);
        }
        /// <summary>
        /// CONSTRUCTOR 4
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="sampleRate"></param>
		public WavReader(double[] rawData, int sampleRate)
		{
			this.Channels = 1;
			this.BitsPerSample = 16;
			this.SampleRate   = sampleRate;
            this.SampleLength = rawData.Length;
            this.Samples = rawData;
		}

        private void ParseData(byte[] data)
		{
			// http://technology.niagarac.on.ca/courses/ctec1631/WavFileFormat.html

			if (!BitConverter.IsLittleEndian)
				throw new NotSupportedException("System.BitConverter does not read little endian.");

			// "RIFF"
			if (data[0] != 0x52 || data[1] != 0x49 || data[2] != 0x46 || data[3] != 0x46)
				throw new InvalidOperationException("Cannot parse WAV header.");

			// Total Length Of Package To Follow
			if (BitConverter.ToUInt32(data, 4) < 36u)
				throw new InvalidOperationException("Cannot parse WAV header.");

			// "WAVE"
			if (data[8] != 0x57 || data[9] != 0x41 || data[10] != 0x56 || data[11] != 0x45)
				throw new InvalidOperationException("Cannot parse WAV header.");

			// "fmt "
			if (data[12] != 0x66 || data[13] != 0x6D || data[14] != 0x74 || data[15] != 0x20)
				throw new InvalidOperationException("Cannot parse WAV header.");

			// Length Of FORMAT Chunk
			int p = (int)BitConverter.ToUInt32(data, 16) - 16;
			if (p < 0) throw new InvalidOperationException("Cannot parse WAV header."); 

			// Always 0x01
			if (data[20] != 0x01 || data[21] != 0x00)
				throw new InvalidOperationException("Cannot parse WAV header.");

			// Channel Numbers 
			this.Channels = BitConverter.ToUInt16(data, 22);

			// Sample Rate
			this.SampleRate = (int)BitConverter.ToUInt32(data, 24);

			// Bytes Per Second
			BitConverter.ToUInt32(data, 28);

			// Bytes Per Sample
			int bytesPerSample = BitConverter.ToUInt16(data, 32);

			// Bits Per Sample
			this.BitsPerSample = BitConverter.ToUInt16(data, 34);

			// "data"
			if (data[36+p] != 0x64 || data[37+p] != 0x61 || data[38+p] != 0x74 || data[39+p] != 0x61)
				throw new InvalidOperationException("Cannot parse WAV header.");

			// Length Of Data To Follow
			int dataLength = (int)BitConverter.ToUInt32(data, 40+p);
			int headerLength = 44 + p;
			if (dataLength == 0 || dataLength > data.Length - headerLength)
				dataLength = data.Length - headerLength;

			this.SampleLength = dataLength / bytesPerSample;
			this.Samples = new double[sampleLength];

			switch (this.BitsPerSample)
			{
				case 8:
					for (int i = 0, offset = headerLength; i < sampleLength; i++, offset += bytesPerSample)
						this.Samples[i] = data[offset] / 128.0;
					break;
				case 16:
					for (int i = 0, offset = headerLength; i < sampleLength; i++, offset += bytesPerSample)
						this.Samples[i] = BitConverter.ToInt16(data, offset) / 32768.0;
					break;
				default:
					throw new NotSupportedException("Bits per sample other than 8 and 16.");
			}
		}

		public static WavReader SineWave(double freq, double amp, double phase, TimeSpan length, int sampleRate)
		{
			int n = (int)Math.Floor(length.TotalSeconds * sampleRate);
			double[] data = new double[n];
			for (int i = 0; i < n; i++)
				data[i] = amp * Math.Sin(phase + 2.0 * Math.PI * freq * i / sampleRate);
			return new WavReader(data, sampleRate);
		}





	}
}