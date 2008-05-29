using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AudioTools
{
	public sealed class Spectrum
	{
		public double[,] Data { get; private set; }
		public int Frames { get { return Data.GetLength(0); } }
		public int Bands { get { return Data.GetLength(1) - 1; } }

		// Amplitude
		public double Min { get; private set; }
		public double Max { get; private set; }
        //public double Epsilon { get; private set; }
        public double SafeMin { get { return Math.Max(Min, Epsilon); } }
		
		// Frequency
		public double Nyquist { get; private set; }

		public static Spectrum Generate(WavReader wav, FFT fft, int step)
		{
			double[] signal = wav.Samples;
			int width = (signal.Length - fft.WindowSize + 1) / step;
			int height = fft.WindowSize / 2 + 1;

			int offset = 0;
			double[,] frames = new double[width, height];
			double min = Double.MaxValue, max = 0.0;
			for (int i = 0; i < width; i++)
			{
				double[] spectrum = fft.Invoke(signal, offset);
				for (int j = 0; j < height; j++)
				{
					double amplitude = spectrum[j];
					if (amplitude < min) min = amplitude;
					if (amplitude > max) max = amplitude;
					frames[i, j] = amplitude;
				}
				offset += step;
			}
			return new Spectrum() { Data = frames, Min = min, Epsilon = wav.Epsilon, Max = max, Nyquist = 0.5 * wav.SampleRate };
		}

		public static Spectrum AutoCorrelation(WavReader wav, int low, int high, int lagstep, int n, int step)
		{
			double[] signal = wav.Samples;
			int width = signal.Length / step;
			int height = (high-low)/lagstep;
			FFT fft = new FFT(height);

			int offset = 0;
			double[,] frames = new double[width, height+1];
			double min = Double.MaxValue, max = 0.0;
			for (int i = 0; i < width; i++)
			{
				int j = 0;
				double[] r = new double[height];
				for (int lag = low; lag < high; lag += lagstep)
				{
					//int lag = (int)(wav.SampleRate / f);
					int begin = offset - n;
					int end = offset + n;
					if (begin < 0) continue;
					if (end + lag >= signal.Length) continue;
					for (int k = begin; k <= end; k++)
						r[j] += signal[k] * signal[k + lag];
					j++;
				}
				r = fft.Invoke(r, 0);
				for(j = 0; j < r.Length; j++)
				{
					if (r[j] < min) min = r[j];
					if (r[j] > max) max = r[j];
					frames[i, j] = r[j];
				}
				offset += step;
			}
			return new Spectrum() { Data = frames, Min = Math.Max(min, wav.Epsilon), Max = max, Nyquist = 0.5 * wav.SampleRate };
		}

		public void GetDynamicRange(double minPercentile, double maxPercentile, out double minOut, out double maxOut)
		{
			if (maxPercentile < minPercentile) throw new ArgumentException("maxPercentile must be greater than or equal to minPercentile");
			if (minPercentile < 0.0) throw new ArgumentException("minPercentile must be at least 0.0");
			if (maxPercentile > 1.0) throw new ArgumentException("maxPercentile must be at most 1.0");

			const int n = 1024;
			int[] bins = new int[n];
			double[,] data = this.Data;
			double minIn = this.SafeMin;
			double maxIn = this.Max;
			int M = data.GetLength(0);
			int N = data.GetLength(1);
			double range = maxIn - minIn;
			for(int i = 0; i < M; i++)
				for (int j = 0; j < N; j++)
				{
					int k = (int)Math.Floor(n * (data[i, j] - minIn) / range);
					if (k < 0) k = 0;
					if (k >= n) k = n - 1;
					bins[k]++;
				}

			int minThres = (int)Math.Floor(minPercentile * M * N);
			minOut = minIn;
			for (int k = 0; k < n; k++)
			{
				minThres -= bins[k];
				if (minThres < 0.0)
				{
					minOut = minIn + k * range / n;
					break;
				}
			}

			int maxThres = (int)Math.Ceiling((1.0 - maxPercentile) * M * N);
			maxOut = maxIn;
			for (int k = n; k > 0; k--)
			{
				maxThres -= bins[k-1];
				if (maxThres < 0.0)
				{
					maxOut = minIn + k * range / n;
					break;
				}
			}
		}

		public Spectrum DecibelSpectrum()
		{
			double[,] inData = this.Data;
			int M = inData.GetLength(0);
			int N = inData.GetLength(1);
			double[,] outData = new double[M,N];
			for (int i = 0; i < M; i++)
				for (int j = 0; j < N; j++)
					outData[i, j] = Decibel(inData[i, j]);
			return new Spectrum() { Data = outData, Min = Decibel(this.Min), Epsilon = Decibel(this.Epsilon), Max = 10.0 * Math.Log10(this.Max), Nyquist = this.Nyquist };
		}

		public static double Decibel(double x)
		{
			return 10.0 * Math.Log10(x);
		}

		public Spectrum Clip(double min, double max)
		{
			double[,] inData = this.Data;
			int M = inData.GetLength(0);
			int N = inData.GetLength(1);
			double[,] outData = new double[M, N];
			for (int i = 0; i < M; i++)
				for (int j = 0; j < N; j++)
				{
					double x = inData[i, j];
					if (x < min) outData[i, j] = min;
					else if (x > max) outData[i, j] = max;
					else outData[i, j] = x;
				}
			return new Spectrum() { Data = outData, Min = Math.Max(min,this.Min), Epsilon = this.Epsilon, Max = Math.Min(max,this.Max), Nyquist = this.Nyquist };
		}

		public Spectrum MelSpectrum(int bands)
		{
			double[,] inData = this.Data;
			int M = inData.GetLength(0);
			int N = inData.GetLength(1);
			double[,] outData = new double[M, bands];
			double linBand = Nyquist / Bands;
			double melBand = Mel(Nyquist) / bands;
			double min = double.PositiveInfinity, max = double.NegativeInfinity;
			for (int i = 0; i < M; i++)
				for (int j = 0; j < bands; j++)
				{
					double a = InverseMel(j * melBand) / linBand;
					double b = InverseMel((j + 1) * melBand) / linBand;
					int ai = (int)Math.Ceiling(a);
					int bi = (int)Math.Floor(b);

					double sum = 0.0;
					if (ai > 0)
					{
						double ya = (1.0 - ai + a) * inData[i, ai - 1] + (ai - a) * inData[i, ai];
						sum += MelIntegral(a * linBand, ai * linBand, ya, inData[i, ai]);
					}
					for (int k = ai; k < bi; k++)
					{
						sum += MelIntegral(k * linBand, (k + 1) * linBand, inData[i, k], inData[i, k + 1]);
					}
					if (bi < this.Bands)
					{
						double yb = (b - bi) * inData[i, bi] + (1.0 - b + bi) * inData[i, bi + 1];
						sum += MelIntegral(bi * linBand, b * linBand, inData[i, bi], yb);
					}
					sum /= melBand;

					outData[i, j] = sum;
					if(sum < min) min = sum;
					if(sum > max) max = sum;
				}
			return new Spectrum() { Data = outData, Min = min, Epsilon = this.Epsilon, Max = max, Nyquist = Mel(Nyquist) };
		}

		private static double MelIntegral(double f0, double f1, double y0, double y1)
		{
			double p = 2595.0 / Math.Log(10.0);
			const double q = 700.0;
			double x = (f0 - f1) / (q + f1);
			double x1 = Math.Log(x + 1.0);
			if (Math.Abs(x1 - x) > 1.0e-16)
				return p * ((y1 - y0) + (y0 - y1 * (x + 1.0)) * (x1 / x));
			else return 0.0;
		}

		public static double Mel(double f)
		{
			return 2595.0 * Math.Log10(1.0 + f / 700.0);
		}

		public static double InverseMel(double m)
		{
			return (Math.Pow(10.0, m / 2595.0) - 1.0) * 700.0;
		}

		public Spectrum RemoveDC()
		{
			double[,] inData = this.Data;
			int M = inData.GetLength(0);
			int inN = inData.GetLength(1);
			int outN = inN - 1;
			double[,] outData = new double[M, outN];
			double min = Double.PositiveInfinity, max = double.NegativeInfinity;
			for (int i = 0; i < M; i++)
			{
				for (int j = 0; j < outN; j++)
				{
					double amplitude = inData[i, j+1];
					if (amplitude < min) min = amplitude;
					if (amplitude > max) max = amplitude;
					outData[i, j] = amplitude;
				}
			}
			return new Spectrum() { Data = outData, Min = min, Epsilon = this.Epsilon, Max = max, Nyquist = this.Nyquist };
		}

		public Spectrum Cepstrum()
		{
			double[,] inData = this.Data;
			int M = inData.GetLength(0);
			int inN = inData.GetLength(1);
			FFT fft = new FFT(inN);
			int outN = inN / 2 + 1;
			double[,] outData = new double[M, outN];
			double min = Double.MaxValue, max = 0.0;
			for (int i = 0; i < M; i++)
			{
				double[] inSpectrum = new double[inN];
				for (int j = 0; j < inN; j++) inSpectrum[j] = inData[i, j];
				double[] outSpectrum = fft.Invoke(inSpectrum, 0);
				for (int j = 0; j < outN; j++)
				{
					double amplitude = outSpectrum[j];
					if (amplitude < min) min = amplitude;
					if (amplitude > max) max = amplitude;
					outData[i, j] = amplitude;
				}
			}
			return new Spectrum() { Data = outData, Min = min, Epsilon = double.NaN, Max = max, Nyquist = double.NaN };
		}

		public double[] GetPowerSpectrum()
		{
            double[,] inData = this.Data;
			int M = inData.GetLength(0);
			int N = inData.GetLength(1);
			double[] outData = new double[N];

			for (int j = 0; j < N; j++)
			{
				double sum = 0.0;
                for (int i = 0; i < M; i++)
                    sum += inData[i, j];
                outData[j] = sum;
			}

			return outData;
		}

        public Spectrum Decimate(int n)
        {
            double[,] inData = this.Data;
            int M = inData.GetLength(0) / n;
            int N = inData.GetLength(1);
            double[,] outData = new double[M, N];
            double min = double.PositiveInfinity, max = double.NegativeInfinity;
            for (int i = 0; i < M; i++)
                for (int j = 0; j < N; j++)
                {
                    for (int k = n * i; k < n * (i + 1); k++)
                        outData[i, j] += inData[k, j];
                    outData[i, j] /= n;
                    if (outData[i, j] < min) min = outData[i, j];
                    else if (outData[i, j] > max) max = outData[i, j];
                }
            return new Spectrum() { Data = outData, Min = min, Epsilon = this.Epsilon, Max = max, Nyquist = this.Nyquist };
        }

		public Bitmap CreateBitmap(double min, double max)
		{
			if (max < min) throw new ArgumentException("max must be greater than or equal to min");

			double[,] f2 = Data;
			int width = f2.GetLength(0);
			int height = f2.GetLength(1);
			double range = max - min;
			Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
			if(bmpData.Stride <0) throw new NotSupportedException("Bottum-up bitmaps");

			unsafe
			{
				byte* p0 = (byte*)bmpData.Scan0;
				for (int y = height; y > 0; y--)
				{
					byte* p1 = p0;
					for (int x = 0; x < width; x++)
					{
						int c = (int)Math.Floor(255.0 * (f2[x, y-1] - min) / range);
						if (c < 0) c = 0;
						if (c >= 256) c = 255;
						*p1++ = (byte)c; //b
						*p1++ = (byte)(c/2); //g
						*p1++ = (byte)0; //r
					}
					p0 += bmpData.Stride;
				}
			}

			bmp.UnlockBits(bmpData);
			return bmp;
		}
	}

	public sealed class FFT
	{
		public delegate double WindowFunc(int n, int N);

		public int WindowSize { get; private set; }

		public double[] WindowWeights { get; private set; }

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
				f[i] = mod2(cdata[2 * i], cdata[2 * i + 1]);
			return f;
		}

        private static double mod2(double x, double y)
		{
			return x * x + y * y;
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
			return (n & (n - 1)) == 0;
		}
	}

	public sealed class WavReader
	{
		public int Channels { get; private set; }

		public int SampleRate { get; private set; }

		public int BitsPerSample { get; private set; }

		public double[] Samples { get; private set; }

		public TimeSpan Time
		{
			get { return TimeSpan.FromSeconds(((double)Samples.Length) / SampleRate); }
		}

		public double Epsilon
		{
			get { return Math.Pow(0.5, BitsPerSample - 1); }
		}

		public WavReader(string path)
		{
			ParseData(File.ReadAllBytes(path));
		}

		public WavReader(byte[] wavData)
		{
			ParseData(wavData);
		}

		public WavReader(double[] rawData, int sampleRate)
		{
			this.Channels = 1;
			this.BitsPerSample = 16;
			this.SampleRate = sampleRate;
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

			int sampleLength = dataLength / bytesPerSample;
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