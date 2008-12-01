using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using TowseyLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace AudioStuff
{
	public abstract class BaseSonogram
	{
		#region Statics
		public const double MinLogEnergy = -7.0;        // typical noise value for BAC2 recordings = -4.5
		public const double MaxLogEnergy = -0.60206;    // = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
		#endregion

		#region Properties
		public BaseSonogramConfig Configuration { get; private set; }

		public double MaxAmplitude { get; private set; }
		public int SampleRate { get; private set; }
		public int NyquistFrequency { get { return SampleRate / 2; } }
		public TimeSpan Duration { get; private set; }

		public double FrameDuration { get { return Configuration.WindowSize / (double)SampleRate; } } // Duration of full frame or window in seconds
		public double FrameOffset { get { return FrameDuration * (1 - Configuration.WindowOverlap); } } // Duration of non-overlapped part of window/frame in seconds
		public double FBinWidth { get { return (SampleRate / 2) / (double)Configuration.FreqBinCount; } }
		public double FramesPerSecond { get { return 1 / FrameOffset; } }
		public int FrameCount { get; private set; } // Originally temporarily set to (int)(Duration.TotalSeconds / FrameOffset) then reset later

		public double[] FrameEnergy { get; private set; } // Energy per signal frame
		public double[] Decibels { get; private set; } // Normalised decibels per signal frame

		public double NoiseSubtracted { get; private set; } // Noise (dB) subtracted from each frame decibel value
		public double FrameMax_dB { get; private set; }
		public double FrameNoise_dB { get; private set; }
		public double Frame_SNR { get { return FrameMax_dB - FrameNoise_dB; } }
		public double MinDecibelReference { get; private set; } // Min reference dB value after noise substraction
		public double MaxDecibelReference { get; private set; } // Max reference dB value after noise substraction

		public double FreqBandMax_dB { get; private set; }
		public double FreqBandNoise_dB { get; private set; }
		public double FreqBand_SNR { get; private set; }

		public int[] SigState { get; private set; } // Integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.

		public double[,] Data { get; protected set; }
		#endregion

		public BaseSonogram(BaseSonogramConfig config, WavReader wav)
		{
			Configuration = config;

			SampleRate = wav.SampleRate;
			Duration = wav.Time;

			MaxAmplitude = wav.CalculateMaximumAmplitude();

			int minFreqBand = config.MinFreqBand ?? 0;
			int maxFreqBand = config.MaxFreqBand ?? NyquistFrequency;
			bool doFreqBandAnalysis = minFreqBand > 0 || maxFreqBand < NyquistFrequency;

			double[] signal = wav.Samples;

			// SIGNAL PRE-EMPHASIS helps with speech signals
			if (config.DoPreemphasis)
				signal = DSP.PreEmphasis(signal, 0.96);

			// FRAME WINDOWING
			int step = (int)(config.WindowSize * (1 - config.WindowOverlap));
			double[,] frames = DSP.Frames(signal, config.WindowSize, step);
			FrameCount = frames.GetLength(0);

			// ENERGY PER FRAME
			FrameEnergy = DSP.SignalLogEnergy(frames, Sonogram.minLogEnergy, Sonogram.maxLogEnergy);

			// FRAME NOISE SUBTRACTION: subtract background noise to produce decibels array in which zero dB = average noise
			double minEnergyRatio = Sonogram.minLogEnergy - Sonogram.maxLogEnergy;
			double Q;
			double min_dB;
			double max_dB;
			Decibels = DSP.NoiseSubtract(FrameEnergy, out min_dB, out max_dB, minEnergyRatio, Sonogram.noiseThreshold, out Q);
			NoiseSubtracted = Q;
			FrameNoise_dB = min_dB; //min decibels of all frames 
			FrameMax_dB = max_dB;
			MinDecibelReference = min_dB - Q;
			MaxDecibelReference = (Sonogram.maxLogEnergy * 10) - Q;

			// ZERO CROSSINGS
			//this.zeroCross = DSP.ZeroCrossings(frames);

			double k1; double k2; int k1_k2delay; int syllableDelay; int minPulse;
			SigState = DetermineEndpointsOfVocalisations(out k1, out k2, out k1_k2delay, out syllableDelay, out minPulse);

			var fractionOfHighEnergyFrames = Speech.FractionHighEnergyFrames(Decibels, k2);
			if ((fractionOfHighEnergyFrames > 0.8) && (Configuration.DoNoiseReduction))
			{
				Log.WriteLine("\n\t################### Sonogram.Make(WavReader wav): WARNING ##########################################");
				Log.WriteLine("\t################### This is a high energy recording. The fraction of high energy frames = "
																+ fractionOfHighEnergyFrames.ToString("F2") + " > 80%");
				Log.WriteLine("\t################### Noise reduction algorithm may not work well in this instance!\n");
			}

			//generate the spectra of FFT AMPLITUDES
			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
			var amplitudeM = MakeAmplitudeSpectra(frames, TowseyLib.FFT.GetWindowFunction(config.FftConfiguration.WindowFunction), epsilon);
			Log.WriteIfVerbose("\tDim of amplitude spectrum =" + amplitudeM.GetLength(1));

			//EXTRACT REQUIRED FREQUENCY BAND
			if (config.DoFreqBandAnalysis)
			{
				int c1 = (int)(minFreqBand / FBinWidth);
				int c2 = (int)(maxFreqBand / FBinWidth);
				amplitudeM = DataTools.Submatrix(amplitudeM, 0, c1, amplitudeM.GetLength(0) - 1, c2);
				Log.WriteIfVerbose("\tDim of required sub-band  =" + amplitudeM.GetLength(1));
				//DETERMINE ENERGY IN FFT FREQ BAND
				Decibels = FreqBandEnergy(amplitudeM);
				//DETERMINE ENDPOINTS OF VOCALISATIONS
				SigState = Speech.VocalizationDetection(Decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);
			}

			Make(amplitudeM);
		} //end Make(WavReader wav)

		int[] DetermineEndpointsOfVocalisations(out double k1, out double k2, out int k1_k2delay, out int syllableDelay, out int minPulse)
		{
			var config = Configuration.EndpointDetectionConfiguration;
			k1 = MinDecibelReference + config.SegmentationThresholdK1;
			k2 = MinDecibelReference + config.SegmentationThresholdK2;
			k1_k2delay = (int)(config.K1K2Latency / FrameOffset); //=5  frames delay between signal reaching k1 and k2 thresholds
			syllableDelay = (int)(config.VocalDelay / FrameOffset); //=10 frames delay required to separate vocalisations 
			minPulse = (int)(config.MinPulseDuration / FrameOffset); //=2 frames is min vocal length
			//Console.WriteLine("k1_k2delay=" + k1_k2delay + "  syllableDelay=" + syllableDelay + "  minPulse=" + minPulse);
			return Speech.VocalizationDetection(Decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);
		}

		double[,] MakeAmplitudeSpectra(double[,] frames, TowseyLib.FFT.WindowFunc w, double epsilon)
		{
			int frameCount = frames.GetLength(0);
			int N = frames.GetLength(1);  //= the FFT windowSize 
			int binCount = (N / 2) + 1;  // = fft.WindowSize/2 +1 for the DC value;

			var fft = new TowseyLib.FFT(N, w); // init class which calculates the FFT

			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			int smoothingWindow = 3; //to smooth the spectrum 

			double[,] sonogram = new double[frameCount, binCount];

			for (int i = 0; i < frameCount; i++)//foreach time step
			{
				double[] data = DataTools.GetRow(frames, i);
				double[] f1 = fft.Invoke(data);
				f1 = DataTools.filterMovingAverage(f1, smoothingWindow); //to smooth the spectrum - reduce variance
				for (int j = 0; j < binCount; j++) //foreach freq bin
				{
					double amplitude = f1[j];
					if (amplitude < epsilon)
						amplitude = epsilon; // to prevent possible log of a very small number
					sonogram[i, j] = amplitude;
				}
			} //end of all frames
			return sonogram;
		}

		double[] FreqBandEnergy(double[,] fftAmplitudes)
		{
			//Console.WriteLine("minDefinedLogEnergy=" + Sonogram.minLogEnergy.ToString("F2") + "  maxLogEnergy=" + Sonogram.maxLogEnergy);
			double[] logEnergy = DSP.SignalLogEnergy(fftAmplitudes, Sonogram.minLogEnergy, Sonogram.maxLogEnergy);

			//NOTE: FreqBand LogEnergy levels are higher than Frame levels but SNR remains same.
			//double min; double max;
			//DataTools.MinMax(logEnergy, out min, out max);
			//Console.WriteLine("FrameNoise_dB   =" + State.FrameNoise_dB    + "  FrameMax_dB   =" + State.FrameMax_dB    + "  SNR=" + State.Frame_SNR);
			//Console.WriteLine("FreqBandNoise_dB=" + State.FreqBandNoise_dB + "  FreqBandMax_dB=" + State.FreqBandMax_dB + "  SNR=" + State.FreqBand_SNR);
			//Console.WriteLine("FreqBandNoise_dB=" + (min*10) + "  FreqBandMax_dB=" + (max*10) + "  SNR=" + State.FreqBand_SNR);

			//noise reduce the energy array to produce decibels array
			double minFraction = MinLogEnergy - MaxLogEnergy;
			double Q; double min_dB; double max_dB;
			double[] decibels = DSP.NoiseSubtract(logEnergy, out min_dB, out max_dB, minFraction, Sonogram.noiseThreshold, out Q);
			NoiseSubtracted = Q;
			FreqBandNoise_dB = min_dB; //min decibels of all frames 
			FreqBandMax_dB = max_dB;
			FreqBand_SNR = max_dB - min_dB;
			MinDecibelReference = min_dB - NoiseSubtracted;
			MaxDecibelReference = MinDecibelReference + FreqBand_SNR;
			//State.MaxDecibelReference = (Sonogram.maxLogEnergy * 10) - State.NoiseSubtracted;
			//Console.WriteLine("Q=" + State.NoiseSubtracted + "  MinDBReference=" + State.MinDecibelReference + "  MaxDecibelReference=" + State.MaxDecibelReference);
			return decibels;
		}

		protected abstract void Make(double[,] amplitudeM);

		public Image GetImage()
		{
			return GetImage(1, null, null);
		}

		protected virtual Image GetImage(int binHeight, int? minHighlightFreq, int? maxHighlightFreq)
		{
			var data = Data;
			int width = data.GetLength(0); // Number of spectra in sonogram
            int imageHeight = data.GetLength(1) * binHeight; // image ht = sonogram ht. Later include grid and score scales
            
			double min; double max;
			DataTools.MinMax(data, out min, out max);
			double range = max - min;

			Color[] grayScale = ImageTools.GrayScale();

			Bitmap bmp = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
			int yOffset = imageHeight;
			for (int y = 0; y < data.GetLength(1); y++) //over all freq bins
			{
				for (int r = 0; r < binHeight; r++) //repeat this bin if ceptral image
				{
					for (int x = 0; x < width; x++) //for pixels in the line
					{
						// normalise and bound the value - use min bound, max and 255 image intensity range
						double value = (data[x, y] - min) / (double)range;
						int c = 255 - (int)Math.Floor(255.0 * value); //original version
						if (c < 0) c = 0;
						else if (c >= 256) c = 255;

						int g = c + 40; // green tinge used in the template scan band 
						if (g >= 256) g = 255;
						Color col = IsInBand(y, minHighlightFreq, maxHighlightFreq) ? Color.FromArgb(c, g, c) : grayScale[c];
						bmp.SetPixel(x, yOffset - 1, col);
					}//for all pixels in line
					yOffset--;
				} //end repeats over one track
			}//end over all freq bins
			return bmp;
        }

		bool IsInBand(int y, int? minFreq, int? maxFreq)
		{
			if (minFreq == null && maxFreq == null)
				return false;
			return (minFreq == null || y > minFreq) && (maxFreq == null || y < maxFreq);
		}
	}

	public class SpectralSonogram : BaseSonogram
	{
		public SpectralSonogram(BaseSonogramConfig config, WavReader wav)
			: base(config, wav)
		{ }

		protected override void Make(double[,] amplitudeM)
		{
			Data = MakeSpectrogram(amplitudeM);
		}

		/// <summary>
		/// Converts amplitude spectra to power spectra
		/// Does NOT apply filter bank i.e. returns full bandwidth spectrogram
		/// </summary>
		double[,] MakeSpectrogram(double[,] matrix)
		{
			Log.WriteIfVerbose(" MakeSpectrogram(double[,] matrix)");
			double[,] m = Speech.DecibelSpectra(matrix);

			if (Configuration.DoNoiseReduction)
			{
				Log.WriteIfVerbose("\t... doing noise reduction.");
				m = ImageTools.NoiseReduction(m); //Mel scale conversion should be done before noise reduction
			}
			return m;
		}
	}

	public class CepstralSonogram : BaseSonogram
	{
		public CepstralSonogram(CepstralSonogramConfig config, WavReader wav)
			: base(config, wav)
		{ }

		public double MaxMel { get; private set; }      // Nyquist frequency on Mel scale

		protected override void Make(double[,] amplitudeM)
		{
			var config = Configuration as CepstralSonogramConfig;
			Data = MakeCepstrogram(amplitudeM, Decibels, config.MfccConfiguration.CcCount, config.MfccConfiguration.IncludeDelta, config.MfccConfiguration.IncludeDoubleDelta);
		}

		protected double[,] MakeCepstrogram(double[,] matrix, double[] decibels, int ccCount, bool includeDelta, bool includeDoubleDelta)
		{
			Log.WriteIfVerbose(" MakeCepstrogram(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ")");

			double[,] m = ApplyFilterBank(matrix);
			m = Speech.DecibelSpectra(m);

			if (Configuration.DoNoiseReduction)
			{
				Log.WriteIfVerbose("\t... doing noise reduction.");
				m = ImageTools.NoiseReduction(m); //Mel scale conversion should be done before noise reduction
			}

			// not sure if we really need this... commented out for the moment because it'll use lots of memory
			//SpectralM = m; //stores the reduced bandwidth, filtered, noise reduced spectra as new spectrogram

			//calculate cepstral coefficients and normalise
			m = Speech.Cepstra(m, ccCount);
			m = DataTools.normalise(m);

			//calculate the full range of MFCC coefficients ie including energy and deltas, etc
			//normalise energy between 0.0 decibels and max decibels.
			double[] E = Speech.NormaliseEnergyArray(decibels, MinDecibelReference, MaxDecibelReference);
			return Speech.AcousticVectors(m, E, includeDelta, includeDoubleDelta);
		}

		double[,] ApplyFilterBank(double[,] matrix)
		{
			Log.WriteIfVerbose(" ApplyFilterBank(double[,] matrix)");
			//error check that filterBankCount < FFTbins
			int FFTbins = Configuration.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin
            var config = Configuration as CepstralSonogramConfig;
			if (config.MfccConfiguration.FilterbankCount > FFTbins)
				throw new Exception("####### FATAL ERROR:- Sonogram.ApplyFilterBank():- Cannot calculate cepstral coefficients. FilterbankCount > FFTbins. (" + config.MfccConfiguration.FilterbankCount + " > " + FFTbins + ")\n\n");

			MaxMel = Speech.Mel(NyquistFrequency);
			//this is the filter count for full bandwidth 0-Nyquist. This number is trimmed proportionately to fit the required bandwidth. 
			int bandCount = config.MfccConfiguration.FilterbankCount;
			double[,] m = matrix;
			Log.WriteIfVerbose("\tDim prior to filter bank  =" + m.GetLength(1));

			if (config.MfccConfiguration.DoMelScale)
				m = Speech.MelFilterBank(m, bandCount, NyquistFrequency, Configuration.MinFreqBand ?? 0, Configuration.MaxFreqBand ?? NyquistFrequency); // using the Greg integral
			else
				m = Speech.LinearFilterBank(m, bandCount, NyquistFrequency, Configuration.MinFreqBand ?? 0, Configuration.MaxFreqBand ?? NyquistFrequency);
			Log.WriteIfVerbose("\tDim after use of filter bank=" + m.GetLength(1) + " (Max filter bank=" + bandCount + ")");

			return m;
		} //end ApplyFilterBank(double[,] matrix)

		protected override Image GetImage(int binHeight, int? minHighlightFreq, int? maxHighlightFreq)
		{
			int sonogramHeight = Data.GetLength(1);
			if (minHighlightFreq != null || maxHighlightFreq != null)
			{
				double hzBin = (SampleRate / 2) / (double)sonogramHeight;

				if (((CepstralSonogramConfig)Configuration).MfccConfiguration.DoMelScale)
				{
					double melBin = Speech.Mel(NyquistFrequency) / (double)sonogramHeight;
					if (maxHighlightFreq != null)
					{
						double topMel = Speech.Mel(maxHighlightFreq.Value * hzBin);
						maxHighlightFreq = (int)(topMel / melBin);
					}
					if (minHighlightFreq != null)
					{
						double botMel = Speech.Mel(minHighlightFreq.Value * hzBin);
						minHighlightFreq = (int)(botMel / melBin);
					}
				}
			}
			return base.GetImage(binHeight * (256 / sonogramHeight), minHighlightFreq, maxHighlightFreq);
		}
	}

	public class AcousticVectorsSonogram : CepstralSonogram
	{
		public AcousticVectorsSonogram(AcousticVectorsSonogramConfig config, WavReader wav)
			: base(config, wav)
		{ }

		protected override void Make(double[,] amplitudeM)
		{
			var config = Configuration as AcousticVectorsSonogramConfig;
			Data = MakeAcousticVectors(amplitudeM, Decibels, config.MfccConfiguration.CcCount, config.MfccConfiguration.IncludeDelta, config.MfccConfiguration.IncludeDoubleDelta, config.DeltaT);
		}

		double[,] MakeAcousticVectors(double[,] matrix, double[] decibels, int ccCount, bool includeDelta, bool includeDoubleDelta, int deltaT)
		{
			Log.WriteIfVerbose(" MakeAcousticVectors(matrix, decibels, includeDelta=" + includeDelta + ", includeDoubleDelta=" + includeDoubleDelta + ", deltaT=" + deltaT + ")");

			double[,] m = MakeCepstrogram(matrix, decibels, ccCount, includeDelta, includeDoubleDelta);

			//initialise feature vector for template - will contain three acoustic vectors - for T-dT, T and T+dT
			int frameCount = m.GetLength(0);
			int cepstralL = m.GetLength(1);  // length of cepstral vector 
			int featurevL = 3 * cepstralL;   // to accomodate cepstra for T-2, T and T+2

			double[] featureVector = new double[featurevL];
			double[,] acousticM = new double[frameCount, featurevL]; //init the matrix of acoustic vectors
			for (int i = deltaT; i < frameCount - deltaT; i++)
			{
				double[] rowTm2 = DataTools.GetRow(m, i - deltaT);
				double[] rowT = DataTools.GetRow(m, i);
				double[] rowTp2 = DataTools.GetRow(m, i + deltaT);

				for (int j = 0; j < cepstralL; j++) acousticM[i, j] = rowTm2[j];
				for (int j = 0; j < cepstralL; j++) acousticM[i, cepstralL + j] = rowT[j];
				for (int j = 0; j < cepstralL; j++) acousticM[i, cepstralL + cepstralL + j] = rowTp2[j];
			}

			return acousticM;
		}
	}

	public class SobelEdgeSonogram : BaseSonogram
	{
		public SobelEdgeSonogram(BaseSonogramConfig config, WavReader wav)
			: base(config, wav)
		{
		}

		protected override void Make(double[,] amplitudeM)
		{
			Data = SobelEdgegram(amplitudeM);
		}

		double[,] SobelEdgegram(double[,] matrix)
		{
			double[,] m = Speech.DecibelSpectra(matrix);
			if (Configuration.DoNoiseReduction)
				m = ImageTools.NoiseReduction(m);
			return ImageTools.SobelEdgeDetection(m);
		}
	}
}