using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using TowseyLib;

namespace AudioStuff
{
	public class NewSonogram
	{
		public static NewSonogram Create(BaseSonogramConfig config, string wavPath)
		{
			return new NewSonogram(config, new WavReader(wavPath));
		}

		public NewSonogram(BaseSonogramConfig config, WavReader wav)
		{
			/*Configuration = config;

			SampleRate = wav.SampleRate;
			Duration = wav.Time;

			MaxAmplitude = wav.CalculateMaximumAmplitude();

			int nyquistFrequency = SampleRate / 2;

			int minFreqBand = config.MinFreqBand ?? 0;
			int maxFreqBand = config.MaxFreqBand ?? nyquistFrequency;
			bool doFreqBandAnalysis = minFreqBand > 0 || maxFreqBand < nyquistFreq;

			FrameDuration = config.WindowSize / (double)SampleRate; // window duration in seconds
			FrameOffset = FrameDuration * (1 - WindowOverlap); // duration in seconds
			FreqBinCount = WindowSize / 2; // other half is phase info
			MelBinCount = FreqBinCount; // same has number of Hz bins
			FBinWidth = nyquistFrequency / (double)FreqBinCount;
			FrameCount = (int)(TimeDuration / FrameOffset);
			FramesPerSecond = 1 / FrameOffset;

			double[] signal = wav.Samples;

			//SIGNAL PRE-EMPHASIS helps with speech signals
			bool doPreemphasis = false;
			if (doPreemphasis)
			{
				double coeff = 0.96;
				signal = DSP.PreEmphasis(signal, coeff);
			}

			//FRAME WINDOWING
			int step = (int)(State.WindowSize * (1 - State.WindowOverlap));
			double[,] frames = DSP.Frames(signal, State.WindowSize, step);
			State.FrameCount = frames.GetLength(0);

			//ENERGY PER FRAME
			FrameEnergy = DSP.SignalLogEnergy(frames, Sonogram.minLogEnergy, Sonogram.maxLogEnergy);
			//Console.WriteLine("FrameNoiseDecibels=" + State.FrameNoiseLogEnergy + "  FrameMaxDecibels=" + State.FrameMaxLogEnergy);

			//FRAME NOISE SUBTRACTION: subtract background noise to produce decibels array in which zero dB = average noise
			double minEnergyRatio = Sonogram.minLogEnergy - Sonogram.maxLogEnergy;
			double Q;
			double min_dB;
			double max_dB;
			Decibels = DSP.NoiseSubtract(FrameEnergy, out min_dB, out max_dB, minEnergyRatio, Sonogram.noiseThreshold, out Q);
			State.NoiseSubtracted = Q;
			State.FrameNoise_dB = min_dB; //min decibels of all frames 
			State.FrameMax_dB = max_dB;
			State.Frame_SNR = max_dB - min_dB;
			State.MinDecibelReference = min_dB - Q;
			State.MaxDecibelReference = (Sonogram.maxLogEnergy * 10) - Q;

			// ZERO CROSSINGS
			//this.zeroCross = DSP.ZeroCrossings(frames);

			//DETERMINE ENDPOINTS OF VOCALISATIONS
			double k1 = State.MinDecibelReference + State.SegmentationThreshold_k1;
			double k2 = State.MinDecibelReference + State.SegmentationThreshold_k2;
			int k1_k2delay = (int)(State.k1_k2Latency / State.FrameOffset); //=5  frames delay between signal reaching k1 and k2 thresholds
			int syllableDelay = (int)(State.vocalDelay / State.FrameOffset); //=10 frames delay required to separate vocalisations 
			int minPulse = (int)(State.minPulseDuration / State.FrameOffset); //=2 frames is min vocal length
			//Console.WriteLine("k1_k2delay=" + k1_k2delay + "  syllableDelay=" + syllableDelay + "  minPulse=" + minPulse);
			SigState = Speech.VocalizationDetection(Decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);
			State.FractionOfHighEnergyFrames = Speech.FractionHighEnergyFrames(Decibels, k2);
			if ((State.FractionOfHighEnergyFrames > 0.8) && (State.DoNoiseReduction))
			{
				Console.WriteLine("\n\t################### Sonogram.Make(WavReader wav): WARNING ##########################################");
				Console.WriteLine("\t################### This is a high energy recording. The fraction of high energy frames = "
																+ State.FractionOfHighEnergyFrames.ToString("F2") + " > 80%");
				Console.WriteLine("\t################### Noise reduction algorithm may not work well in this instance!\n");
			}

			//generate the spectra of FFT AMPLITUDES
			//calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
			double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
			AmplitudM = MakeAmplitudeSpectra(frames, State.WindowFnc, epsilon);
			Log.WriteIfVerbose("\tDim of amplitude spectrum =" + AmplitudM.GetLength(1));

			//EXTRACT REQUIRED FREQUENCY BAND
			if (State.doFreqBandAnalysis)
			{
				int c1 = (int)(State.freqBand_Min / State.FBinWidth);
				int c2 = (int)(State.freqBand_Max / State.FBinWidth);
				AmplitudM = DataTools.Submatrix(AmplitudM, 0, c1, AmplitudM.GetLength(0) - 1, c2);
				Log.WriteIfVerbose("\tDim of required sub-band  =" + AmplitudM.GetLength(1));
				//DETERMINE ENERGY IN FFT FREQ BAND
				Decibels = FreqBandEnergy(AmplitudM);
				//DETERMINE ENDPOINTS OF VOCALISATIONS
				SigState = Speech.VocalizationDetection(Decibels, k1, k2, k1_k2delay, syllableDelay, minPulse, null);
			}

			//POST-PROCESS to final SPECTROGRAM
			switch (State.SonogramType)
			{
				case SonogramType.spectral:
					SpectralM = MakeSpectrogram(AmplitudM);
					break;
				case SonogramType.cepstral:
					CepstralM = MakeCepstrogram(AmplitudM, Decibels, State.IncludeDelta, State.IncludeDoubleDelta);
					break;
				case SonogramType.acousticVectors:
					AcousticM = MakeAcousticVectors(AmplitudM, Decibels, State.IncludeDelta, State.IncludeDoubleDelta, State.DeltaT);
					break;
				case SonogramType.sobelEdge:
					SpectralM = SobelEdgegram(AmplitudM);
					break;
			}*/
		} //end Make(WavReader wav)

		#region Properties
		public BaseSonogramConfig Configuration { get; set; }

		public double MaxAmplitude { get; set; }
		public int SampleRate { get; set; }
		public TimeSpan Duration { get; set; }

		public double FrameDuration { get; set; }
		public double FrameOffset { get; set; }
		#endregion
	}
}