using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using AudioTools;

namespace AudioStuff
{
	public class MMRecogniser : BaseClassifier
	{
		const int NoiseSampleCount = 5000; // Number of samples to use when generating noise vector

		internal MMRecogniser(MMTemplate template)
		{
			Template = template;
		}

		#region Properties
		public MMTemplate Template { get; private set; }
		#endregion

		public override BaseResult Analyse(AudioRecording recording)
		{
			BaseSonogram sonogram;
			return Analyse(recording, out sonogram);
		}

		public override BaseResult Analyse(AudioRecording recording, out BaseSonogram sonogram)
		{
			AcousticVectorsSonogram avSonogram;
			var result = GenerateSymbolSequence(recording, out avSonogram);
			ScanSymbolSequenceWithMM(result, avSonogram.FrameOffset);
			sonogram = avSonogram;
			return result;
		}

		public MMResult GenerateSymbolSequence(AudioRecording recording, out AcousticVectorsSonogram sonogram)
		{
			sonogram = new AcousticVectorsSonogram(Template.SonogramConfiguration, recording.GetWavData());
			MMResult result = new MMResult(Template);
			result.AcousticMatrix = GenerateAcousticMatrix(sonogram, Template.NoiseFVPath);
			AcousticMatrix2SymbolSequence(result);
			return result;
		}
		
		/// <summary>
		/// Transfers feature vectors from the template to the classifier.
		/// Need to insert an additional NOISE feature vector in the zero index
		/// The noise fv will later be used to assess the statistical significance of the template scores
		/// </summary>
		FeatureVector[] GetFeatureVectors(FeatureVector[] featureVectors)
		{
			var retVal = new FeatureVector[featureVectors.Length + 1];
			Array.Copy(featureVectors, 0, retVal, 1, featureVectors.Length);
			return retVal;
		}

		/// <summary>
		/// Scans a sonogram with predefined feature vectors using a previously loaded template.
		/// </summary>
		double[,] GenerateAcousticMatrix(AcousticVectorsSonogram s, string noiseFVPath)
		{
			Log.WriteIfVerbose("\nSCAN SONOGRAM WITH TEMPLATE");
			//##################### DERIVE NOISE FEATURE VECTOR OR READ PRE-COMPUTED NOISE FILE
			Log.WriteIfVerbose("\tStep 1: Derive NOISE Feature Vector, FV[0], from the passed SONOGRAM");
			var FVs = GetFeatureVectors(Template.FeatureVectorParameters.FeatureVectors);
			int fvCount = FVs.Length;

			int count;
			double dbThreshold = s.MinDecibelReference + Template.SonogramConfiguration.EndpointDetectionConfiguration.SegmentationThresholdK2;  // FreqBandNoise_dB;
			FVs[0] = GetNoiseFeatureVector(s.Data, s.Decibels, dbThreshold, out count);

			if (FVs[0] == null) // If sonogram does not have sufficient noise frames read default noise FV from file
			{
				Log.WriteIfVerbose("\tDerive NOISE Feature Vector from file: " + Template.FeatureVectorParameters.FeatureVectorPaths[0]);
				FVs[0] = new FeatureVector(Template.FeatureVectorParameters.FeatureVectorPaths[0], Template.FeatureVectorParameters.FeatureVectorLength);
			}
			else if (noiseFVPath != null) // Write noise vector to file. It can then be used as a sample noise vector.
				SaveNoiseVector(noiseFVPath, FVs[0], s.NyquistFrequency);

			//##################### PREPARE MATRIX OF NOISE VECTORS AND THEN SET NOISE RESPONSE FOR EACH feature vector
			Log.WriteIfVerbose("\n\tStep 2: Obtain noise response for each feature vector");
			double[,] noiseM = GetRandomNoiseMatrix(s.Data, NoiseSampleCount);
			//following alternative to above method only gets noise estimate from low energy frames
			//double[,] noiseM = GetRandomNoiseMatrix(s.AcousticM, this.noiseSampleCount, this.decibels, this.decibelThreshold);
			for (int i = 0; i < FVs.Length; i++)
				FVs[i].SetNoiseResponse(noiseM, i);

			//##################### DERIVE ACOUSTIC MATRIX OF SYLLABLE Z-SCORES
			Log.WriteIfVerbose("\n\tStep 3: Obtain ACOUSTIC MATRIX of syllable z-scores");
			int frameCount = s.Data.GetLength(0);
			int window = Template.FeatureVectorParameters.ZscoreSmoothingWindow;
			double[,] acousticMatrix = new double[frameCount, fvCount];
			for (int n = 0; n < fvCount; n++)  //for all feature vectors
			{
				Log.WriteIfVerbose("\t... with FV " + n);

				//now calculate z-score for each score value
				double[] zscores = FVs[n].Scan_CrossCorrelation(s.Data);
				zscores = DataTools.filterMovingAverage(zscores, window);  //smooth the Z-scores

				for (int i = 0; i < frameCount; i++) acousticMatrix[i, n] = zscores[i];// transfer z-scores to matrix of acoustic z-scores
			}//end for loop over all feature vectors

			return acousticMatrix;
		}//end GenerateAcousticMatrix()

		void SaveNoiseVector(string noiseFVPath, FeatureVector noiseFV, int nyquistFrequency)
		{
			Log.WriteIfVerbose("\tWriting noise template to file:- " + noiseFVPath);
			noiseFV.SaveDataAndImageToFile(noiseFVPath, Template, nyquistFrequency);
		}

		/// <summary>
		/// Extracts all those frames passed sonogram matrix whose signal energy is below the threshold and 
		///                     returns an average of the feature vectors derived from those frames.
		/// If there are not enough low energy frames, then the method returns null and caller must get
		/// noise FV from another source.
		/// </summary>
		FeatureVector GetNoiseFeatureVector(double[,] acousticM, double[] decibels, double decibelThreshold, out int count)
		{
			int rows = acousticM.GetLength(0);
			int cols = acousticM.GetLength(1);

			double[] noiseFV = new double[cols];

			count = decibels.Count(d => (d <= decibelThreshold)); // Number of frames below the noise threshold

			int targetCount = rows / 5; // Want a minimum of 20% of frames for a noise estimate
			if (count < targetCount)
			{
				Log.WriteLine("  TOO FEW LOW ENERGY FRAMES.");
				Log.WriteLine("  Low energy frame count=" + count + " < targetCount=" + targetCount
					+ "   @ decibelThreshold=" + decibelThreshold.ToString("F3") + " = reference+k2threshold.");
				Log.WriteLine("  TOO FEW LOW ENERGY FRAMES. READ DEFAULT NOISE FEATURE VECTOR.");
				return null;
			}
			else
				Log.WriteIfVerbose("        NOISE Vector is average of " + count + " frames having energy < " + decibelThreshold.ToString("F2") + " dB.");

			// Now transfer low energy frames to noise vector
			for (int i = 0; i < rows; i++)
			{
				if (decibels[i] <= decibelThreshold)
				{
					for (int j = 0; j < cols; j++)
						noiseFV[j] += acousticM[i, j];
				}
			}

			// Take average
			for (int j = 0; j < cols; j++)
				noiseFV[j] /= (double)count;

			return new FeatureVector(noiseFV);
		}

		/// <summary>
		/// Returns a matrix of noise vectors. Each noise vector is a random sample from the original sonogram.
		/// </summary>
		double[,] GetRandomNoiseMatrix(double[,] dataMatrix, int noiseCount)
		{
			int frameCount = dataMatrix.GetLength(0);
			int featureCount = dataMatrix.GetLength(1);

			double[,] noise = new double[noiseCount, featureCount];
			RandomNumber rn = new RandomNumber();

			for (int i = 0; i < noiseCount; i++)
			{
				for (int j = 0; j < featureCount; j++)
				{
					int id = rn.GetInt(frameCount);
					noise[i, j] = dataMatrix[id, j];
				}
			}

			return noise;
		} //end GetRandomNoiseMatrix()

		/// <summary>
		/// Generates a symbol sequence from the acoustic matrix
		/// </summary>
		void AcousticMatrix2SymbolSequence(MMResult result)
		{
			int[] integerSequence = null;
			double threshold = Template.ZScoreThreshold;

			Log.WriteIfVerbose("\n\tStep 4: Convert ACOUSTIC MATRIX >> SYMBOL SEQUENCE");
			Log.WriteIfVerbose("\t\tThreshold=" + threshold.ToString("F2"));

			int frameCount = result.AcousticMatrix.GetLength(0);
			int fvCount = result.AcousticMatrix.GetLength(1); // Number of feature vectors or syllables types

			StringBuilder sb = new StringBuilder();
			integerSequence = new int[frameCount];
			for (int i = 0; i < frameCount; i++)
			{
				double[] fvScores = new double[fvCount]; // Init the FV scores
				for (int n = 0; n < fvCount; n++)
					fvScores[n] = result.AcousticMatrix[i, n]; // Transfer the frame scores to array

				// Get the maximum score
				int maxIndex;
				DataTools.getMaxIndex(fvScores, out maxIndex);
				if (maxIndex == 0) // This frame is noise
				{
					sb.Append('n');
					continue;
				}
				char c = 'x';
				int val = Int32.MaxValue; // Used as integer to represent 'x' or garbage.
				if (fvScores[maxIndex] >= threshold) // Only set symbol or int if fv score exceeds threshold  
				{
					c = DataTools.Integer2Char(maxIndex);
					val = maxIndex;
				}
				sb.Append(c);
				integerSequence[i] = val;
			}//end of frames

			//need to convert the garbage integer in the integer sequence.
			//garbage symbol = 'x' = Int32.MaxValue. Convert Int32 to numberOfStates-1
			//states will be represented by integers: noise=0, fv=1..N, garbage=N+1
			int garbageID = fvCount;
			for (int i = 0; i < frameCount; i++)
				if (integerSequence[i] == Int32.MaxValue)
					integerSequence[i] = garbageID;

			result.SyllSymbols = sb.ToString();
			result.SyllableIDs = integerSequence;
		}

		#region ScanSymbolSequenceWithMM and associates
		void ScanSymbolSequenceWithMM(MMResult result, double frameOffset)
		{
			double[,] acousticMatrix = result.AcousticMatrix;
			string symbolSequence = result.SyllSymbols;
			int[] integerSequence = result.SyllableIDs;
			int frameCount = integerSequence.Length;

			//##################### PARSE SYMBOL STREAM USING MARKOV MODELS
			double windowLength = Template.LanguageModel.SongWindow;
			int clusterWindow = (int)Math.Floor(windowLength * (1 / frameOffset));
			double zThreshold = Template.ZScoreThreshold;
			MarkovModel mm = Template.LanguageModel.WordModel;
			if (Log.Verbosity > 0)
			{
				Log.WriteLine("\nLANGUAGE MODEL");
				mm.WriteInfo(false);
			}

			if (mm.GraphType == HMMType.OLD_PERIODIC)
			{
				double[] scores = WordSearch(symbolSequence, acousticMatrix, Template.LanguageModel.Words);
				result.VocalScores = scores;
				result.VocalCount = DataTools.CountPositives(scores);
				if (result.VocalCount <= 1)
					return; // Cannot do anything more in this case

				//find peaks and process them
				bool[] peaks = DataTools.GetPeaks(scores);
				peaks = RemoveSubThresholdPeaks(scores, peaks, Template.ZScoreThreshold);
				scores = ReconstituteScores(scores, peaks);
				//transfer scores for all frames to score matrix
				result.VocalScores = scores;
				result.VocalCount = DataTools.CountPositives(scores);

				int period_ms = mm.Periodicity_ms; //set in template
				Log.WriteIfVerbose("\n\tPeriodicity = " + period_ms + " ms");
				if ((result.VocalCount < 2) || (period_ms <= 0))
					Log.WriteLine("### Classifier.ScanSymbolSequenceWithMM(): WARNING!!!!   PERIODICITY CANNOT BE ANALYSED.");
				else
				{
					int maxIndex = DataTools.GetMaxIndex(scores);
					result.VocalBest = scores[maxIndex];
					result.VocalBestLocation = (double)maxIndex * frameOffset;

					result.CallPeriodicity_ms = period_ms;
					int period_frames = mm.Periodicity_frames;
					result.CallPeriodicity_frames = period_frames;
					int period_NH = mm.Periodicity_NH_frames;
					bool[] periodPeaks = Periodicity(peaks, period_frames, period_NH);
					result.NumberOfPeriodicHits = DataTools.CountTrues(periodPeaks);
					for (int i = 0; i < frameCount; i++) if (!periodPeaks[i]) scores[i] = 0.0;
				}
			} //end of periodic analysis
			else //normal MARKOV MODEL
			{
				double[] scores = null;
				int hitCount;
				double bestHit;
				int bestLocation;
				mm.ScoreSequence_v2(integerSequence, out scores, out hitCount, out bestHit, out bestLocation);
				result.VocalScores = scores;
				result.VocalCount = hitCount;
				result.VocalBest = bestHit;
				result.VocalBestLocation = (double)bestLocation * frameOffset;

				Log.WriteLine("#### VocalCount={0} VocalBest={1} bestFrame={2:F3} @ {3:F1}s", hitCount, bestHit, bestLocation, result.VocalBestLocation);
			}
		}

		/// <summary>
		/// Scans a symbol string for the passed words and returns for each position in the string the match score of
		/// that word which obtained the maximum score. The matchscore is derived from a zscore matrix.
		/// NOTE: adding z-scores is similar to adding the logs of probabilities derived from a Gaussian distribution.
		///     log(p) = -log(sd) - log(sqrt(2pi)) - (Z^2)/2  = Constant - (Z^2)/2
		///         I am adding Z-scores instead of the squares of Z-scores.
		/// </summary>
		static double[] WordSearch(string symbolSequence, double[,] zscoreMatrix, string[] words)
		{
			int sequenceLength = symbolSequence.Length;
			int symbolCount = zscoreMatrix.GetLength(1);
			int wordCount = words.Length;
			double[] wordScores = new double[sequenceLength];
			int maxWordLength = words[0].Length; // User must place longest word first in the list !!!

			for (int i = 0; i < sequenceLength - maxWordLength; i++) // WARNING: user must place longest word first in the list !!!
			{
				if ((symbolSequence[i] == 'x') || (symbolSequence[i] == 'n'))
				{
					wordScores[i] = 0.0;
					continue;
				}
				//have a possible word so check what it is
				double[] scores = new double[wordCount];
				for (int w = 0; w < wordCount; w++)
				{
					int wordLength = words[w].Length;
					int[] intArray = MarkovModel.String2IntegerArray(words[w]);
					double sum = 0.0;
					for (int s = 0; s < wordLength; s++)
					{
						if (intArray[s] >= symbolCount)
							throw new Exception("WORD <" + words[w] + "> IN GRAMMAR CONTAINS ILLEGAL SYMBOL.");
						sum += zscoreMatrix[i + s, intArray[s]];
					}
					scores[w] = sum;

				} //end of getting word scores for this position

				// Get the maximum score
				int maxIndex;
				DataTools.getMaxIndex(scores, out maxIndex);
				wordScores[i] = scores[maxIndex];
			} //end of symbol string
			return wordScores;
		}

		static bool[] RemoveSubThresholdPeaks(double[] scores, bool[] peaks, double threshold)
		{
			int length = peaks.Length;
			bool[] newPeaks = new bool[length];
			for (int n = 0; n < length; n++)
			{
				newPeaks[n] = peaks[n];
				if (scores[n] < threshold)
					newPeaks[n] = false;
			}
			return newPeaks;
		}

		/// <summary>
		/// returns a reconstituted array of zscores.
		/// Only gives values to score elements in vicinity of a peak.
		/// </summary>
		static double[] ReconstituteScores(double[] scores, bool[] peaks)
		{
			int length = scores.Length;
			double[] newScores = new double[length];
			for (int n = 0; n < length; n++)
				if (peaks[n])
					newScores[n] = scores[n];
			return newScores;
		}

		bool[] Periodicity(bool[] peaks, int period_frame, int period_NH)
		{
			int L = peaks.Length;
			bool[] hits = new bool[L];
			int index = 0;

			//find the first peak
			for (int n = 0; n < L; n++)
			{
				index = n;
				if (peaks[n])
					break;
			}
			if (index == L - 1)
				return hits; // i.e. no peaks in the array

			// have located index of the first peak. Now look for peaks correct distance apart
			int minDist = period_frame - period_NH;
			int maxDist = period_frame + period_NH;
			for (int n = index + 1; n < L; n++)
			{
				if (peaks[n])
				{
					int period = n - index;
					if ((period >= minDist) && (period <= maxDist))
					{
						hits[index] = true;
						hits[n] = true;
					}
					index = n; //set new position
				}
			}

			return hits;
		}
		#endregion
	}
}