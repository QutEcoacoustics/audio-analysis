using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AudioStuff
{
	public sealed class Spectrum
	{
		public double[,] GenerateSpectrogram(WavReader wav, FFT fft, int width, out double min, out double max)
		{
			if (width < 2) throw new ArgumentException("width must be at least 2");

			double[] data = wav.Samples;
			double step = ((double)(data.Length - fft.WindowSize)) / (width - 1);
			int height = fft.WindowSize / 2;

			double offset = 0.0;
			double[,] f2 = new double[width, height];
			min = Double.MaxValue; max = Double.MinValue;
			for (int i = 0; i < width; i++)
			{
				double[] f1 = fft.Invoke(data, (int)Math.Floor(offset));
				for (int j = 0; j < height; j++)
				{
					double f = Math.Log10(f1[j + 1]);
					if (f < min) min = f;
					if (f > max) max = f;
					f2[i, j] = f;
				}
				offset += step;
			}
			if (min < wav.Epsilon) min = wav.Epsilon;
			return f2;
		}

		public void NormalizeAndCompress(double[,] f2, double min, double max, double minPercentile, double maxPercentile, out double minCut, out double maxCut)
		{
			if (max < min) throw new ArgumentException("max must be greater than or equal to min");
			if (maxPercentile < minPercentile) throw new ArgumentException("maxPercentile must be greater than or equal to minPercentile");
			if (minPercentile < 0.0) throw new ArgumentException("minPercentile must be at least 0.0");
			if (maxPercentile > 1.0) throw new ArgumentException("maxPercentile must be at most 1.0");

			const int n = 1024;
			int[] bins = new int[n];
			int M = f2.GetLength(0);
			int N = f2.GetLength(1);
			double range = max - min;
			for(int i = 0; i < M; i++)
				for (int j = 0; j < N; j++)
				{
					int k = (int)Math.Floor(n * (f2[i, j] - min) / range);
					if (k < 0) k = 0;
					if (k >= n) k = n - 1;
					bins[k]++;
				}

			int minThres = (int)Math.Floor(minPercentile * M * N);
			minCut = min;
			for (int k = 0; k < n; k++)
			{
				minThres -= bins[k];
				if (minThres < 0.0)
				{
					minCut = min + k * range / n;
					break;
				}
			}

			int maxThres = (int)Math.Ceiling((1.0 - maxPercentile) * M * N);
			maxCut = max;
			for (int k = n; k > 0; k--)
			{
				maxThres -= bins[k-1];
				if (maxThres < 0.0)
				{
					maxCut = min + k * range / n;
					break;
				}
			}
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

		public Bitmap CreateBitmap(double[,] f2, double min, double max)
		{
			if (max < min) throw new ArgumentException("max must be greater than or equal to min");

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

		public double Normalize(double[] data)
		{
			double max = 0.0;
			for (int i = 0; i < data.Length; i++)
				max = Math.Max(max, Math.Abs(data[i]));
			max = 1.0 / max;
			for (int i = 0; i < data.Length; i++)
				data[i] *= max;
			return max;
		}
	}

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
	}

	public sealed class WavReader
	{
		//public int Channels { get; private set; }
		private int channels;
		public int Channels { get { return channels; } private set { channels = value; } }

		//public int SampleRate { get; private set; }
		private int sampleRate;
		public int SampleRate { get { return sampleRate; } private set { sampleRate = value; } }

		//public int BitsPerSample { get; private set; }
		private int bitsPerSample;
		public int BitsPerSample { get { return bitsPerSample; } private set { bitsPerSample = value; } }

		//public double[] Samples { get; private set; }
		private double[] samples;
		public double[] Samples { get { return samples; } private set { samples = value; } }

		public TimeSpan Time
		{
			get { return TimeSpan.FromSeconds(((double)Samples.Length) / SampleRate); }
		}

		public double Epsilon
		{
			get { return Math.Log10(Math.Pow(0.5, bitsPerSample - 1)); }
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