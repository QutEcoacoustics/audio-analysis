using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;
using AudioTools;
using QutSensors;

namespace AudioStuff
{
	public class MMTemplate : TemplateParameters
	{
		#region Statics
		public static MMTemplate Load(string configFile)
		{
			return new MMTemplate(new Configuration(configFile));
		}
		#endregion

		public MMTemplate(Configuration config) : base(config)
		{
			SonogramConfiguration = new AcousticVectorsSonogramConfig(config);
			FeatureVectorParameters = new FeatureVectorParameters(config);
			LanguageModel = new LanguageModel(config, FeatureVectorParameters.FeatureVectorCount, SonogramConfiguration, SampleRate);

			if (config.Source != null)
				NoiseFVPath = Path.Combine(Path.GetDirectoryName(config.Source), Path.GetFileNameWithoutExtension(config.Source) + "_NoiseFV.txt");
		}

		public void Save(string targetPath)
		{
			using (var file = new StreamWriter(targetPath))
			{
				SonogramConfiguration.Save(file);
				FeatureVectorParameters.Save(file, Path.GetDirectoryName(targetPath));
				LanguageModel.Save(file);
			}
		}

		public void SetParameters(GUI gui)
		{
			if (gui.Fv_Source == FV_Source.SELECTED_FRAMES)
			{
				SetSelectedFrames(gui.SelectedFrames);
				SetFrequencyBounds(gui.Min_Freq, gui.Max_Freq);
			}
			else if (gui.Fv_Source == FV_Source.MARQUEE)
			{
				SetFrequencyBounds(gui.Min_Freq, gui.Max_Freq);
				FeatureVectorParameters.MarqueeStart = gui.MarqueeStart;
				FeatureVectorParameters.MarqueeEnd = gui.MarqueeEnd;
				if (gui.Fv_Extraction == FV_Extraction.AT_FIXED_INTERVALS)
					FeatureVectorParameters.FeatureVectorExtractionInterval = gui.FvExtractionInterval;
			}
			SetSonogram(gui.FrameSize, gui.FrameOverlap, gui.DynamicRange, gui.FilterBankCount,
							gui.DoMelConversion, gui.DoNoiseReduction, gui.CeptralCoeffCount,
							   gui.DeltaT, gui.IncludeDeltaFeatures, gui.IncludeDoubleDeltaFeatures);

			FeatureVectorParameters.FeatureVectorSource = gui.Fv_Source;
			FeatureVectorParameters.FeatureVectorExtraction = gui.Fv_Extraction;
			FeatureVectorParameters.FeatureVector_DoAveraging = gui.DoFvAveraging;
			FeatureVectorParameters.FeatureVector_DefaultNoiseFile = gui.FvDefaultNoiseFile;
			LanguageModel.HmmType = gui.HmmType;
			LanguageModel.HmmName = gui.HmmName;
			LanguageModel.Words = null;
			LanguageModel.WordCount = 0;
		}

		/// <summary>
		/// this method is called from the user interface.
		/// It expects a comma separate list of one or more integers
		/// </summary>
		public void SetSelectedFrames(string selectedFrames)
		{
			string[] IDs = selectedFrames.Split(',');
			int count = IDs.Length;
			string[] indices = new string[count];
			for (int i = 0; i < count; i++)
				indices[i] = IDs[i];
			FeatureVectorParameters.FeatureVector_SelectedFrames = IDs;
		}

		public void SetFrequencyBounds(int minFreq, int maxFreq)
		{
			SonogramConfiguration.MinFreqBand = minFreq;
			SonogramConfiguration.MaxFreqBand = maxFreq;
			MinTemplateFreq = minFreq;
			MaxTemplateFreq = maxFreq;
		}

		public void SetExtractionParameters(FV_Source fvSource, FV_Extraction fvExtraction, bool doFvAveraging,
											string defaultNoiseFile, double zThreshold)
		{
			FeatureVectorParameters.FeatureVectorSource = fvSource;
			FeatureVectorParameters.FeatureVectorExtraction = fvExtraction;
			FeatureVectorParameters.FeatureVector_DoAveraging = doFvAveraging;
			FeatureVectorParameters.FeatureVector_DefaultNoiseFile = defaultNoiseFile;
		}

		/// <summary>
		/// CREATES a new template and extracts feature vectors. 
		/// NOTE: All these template parameters override the default values set in the application's sonogram.ini file.
		/// </summary>
		public void SetSonogram(int frameSize, double frameOverlap, double dynamicRange, int filterBankCount,
								bool doMelConversion, bool doNoiseReduction,
								int ceptralCoeffCount, int deltaT, bool includeDeltaFeatures, bool includeDoubleDeltaFeatures)
		{
			SonogramConfiguration.WindowSize = frameSize;
			SonogramConfiguration.WindowOverlap = frameOverlap;
			SonogramConfiguration.MfccConfiguration.FilterbankCount = filterBankCount;
			SonogramConfiguration.MfccConfiguration.DoMelScale = doMelConversion;
			SonogramConfiguration.DoNoiseReduction = doNoiseReduction;
			SonogramConfiguration.DeltaT = deltaT;
			SonogramConfiguration.MfccConfiguration.IncludeDelta = includeDeltaFeatures;
			SonogramConfiguration.MfccConfiguration.IncludeDoubleDelta = includeDoubleDeltaFeatures;
		}

		#region Properties
		public AcousticVectorsSonogramConfig SonogramConfiguration { get; set; }
		public FeatureVectorParameters FeatureVectorParameters { get; set; }
		public LanguageModel LanguageModel { get; set; }
		private double zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
		public double ZScoreThreshold { get { return zScoreThreshold; } }
		public int MinTemplateFreq { get; set; }
		public int MaxTemplateFreq { get; set; }
		public string NoiseFVPath { get; private set; }
		#endregion

		/// <summary>
		/// LOGIC FOR EXTRACTION OF FEATURE VECTORS FROM SONOGRAM ****************************************************************
		/// </summary>
		public void ExtractTemplateFromSonogram(WavReader wav)
		{
			FeatureVector[] featureVectors;
			switch (FeatureVectorParameters.FeatureVectorSource)
			{
				case FV_Source.SELECTED_FRAMES:
					featureVectors = GetFeatureVectorsFromFrames(wav);
					break;
				case FV_Source.MARQUEE:
					switch (FeatureVectorParameters.FeatureVectorExtraction)
					{
						case FV_Extraction.AT_ENERGY_PEAKS:
							featureVectors = GetFeatureVectorsFromMarquee(wav);
							break;
						case FV_Extraction.AT_FIXED_INTERVALS:
							featureVectors = GetFeatureVectorsFromMarquee(wav);
							break;
						default:
							throw new InvalidCastException("ExtractTemplateFromSonogram(: WARNING!! INVALID FV EXTRACTION OPTION!)");
					}
					break;
				default:
					throw new InvalidOperationException("ExtractTemplateFromSonogram(: WARNING!! INVALID FV SOURCE OPTION!)");
			}
				
			// Accumulate the acoustic vectors from multiple frames into an averaged feature vector
			if (FeatureVectorParameters.FeatureVector_DoAveraging)
			{
				featureVectors = new FeatureVector[] { FeatureVector.AverageFeatureVectors(featureVectors, 1) };
				FeatureVectorParameters.FeatureVectorCount = 1;
				FeatureVectorParameters.FeatureVectorLength = featureVectors[0].FvLength;
			}
			else //save the feature vectors separately
			{
				FeatureVectorParameters.FeatureVectorCount = featureVectors.Length;
				FeatureVectorParameters.FeatureVectorLength = featureVectors[0].FvLength;
			}

			// Save Feature Vectors to disk
			FeatureVectorParameters.FeatureVectors = featureVectors;
		} // end ExtractTemplateFromSonogram()

		FeatureVector[] GetFeatureVectorsFromFrames(WavReader wav)
		{
			Log.WriteIfVerbose("\nEXTRACTING FEATURE VECTORS FROM FRAMES:- method Template.GetFeatureVectorsFromFrames()");
			//Get frame indices. Assume, when extracting a FeatureVector, that there is only one frame ID per FVector
			string[] IDs = FeatureVectorParameters.FeatureVector_SelectedFrames;
			int indicesL = IDs.Length;

			//initialise feature vectors for template. Each frame provides one vector in three parts
			var sonogram = new CepstralSonogram(SonogramConfiguration, wav);
			double[,] M = sonogram.Data;

			int dT = SonogramConfiguration.DeltaT;
			FeatureVector[] fvs = new FeatureVector[indicesL];
			for (int i = 0; i < indicesL; i++)
			{
				int id = Int32.Parse(IDs[i]);
				Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + id);
				//init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
				double[] acousticV = Speech.GetAcousticVector(M, id, dT); //combines  frames T-dT, T and T+dT
				fvs[i] = new FeatureVector(acousticV);
				// Wav source may not be from a file
				//fvs[i].SourceFile = TemplateState.WavFilePath; // Assume all FVs have same source file
				fvs[i].FrameIndices = IDs[i];
			}
			return fvs;
		}

		FeatureVector[] GetFeatureVectorsFromMarquee(WavReader wav)
		{
			int start = FeatureVectorParameters.MarqueeStart;
			int end = FeatureVectorParameters.MarqueeEnd;
			int marqueeFrames = end - start + 1;
			var frameDuration = SonogramConfiguration.GetFrameDuration(wav.SampleRate);
			double marqueeDuration = marqueeFrames * frameDuration;
			Log.WriteIfVerbose("\tMarquee start=" + start + ",  End=" + end + ",  Duration= " + marqueeFrames + "frames =" + marqueeDuration.ToString("F2") + "s");
			int[] frameIndices = null;

			var sonogram = new CepstralSonogram(SonogramConfiguration, wav);

			switch (FeatureVectorParameters.FeatureVectorExtraction)
			{
				case FV_Extraction.AT_FIXED_INTERVALS:
					int interval = (int)(FeatureVectorParameters.FeatureVectorExtractionInterval / frameDuration / (double)1000);
					Log.WriteIfVerbose("\tFrame interval=" + interval + "ms");
					frameIndices = FeatureVector.GetFrameIndices(start, end, interval);
					break;
				case FV_Extraction.AT_ENERGY_PEAKS:
					double[] frameEnergy = sonogram.Decibels;
					double energyThreshold = SonogramConfiguration.EndpointDetectionConfiguration.SegmentationThresholdK1;
					frameIndices = FeatureVector.GetFrameIndices(start, end, frameEnergy, energyThreshold);
					Log.WriteIfVerbose("\tEnergy threshold=" + energyThreshold.ToString("F2"));
					break;
				default:
					Log.WriteLine("Template.GetFeatureVectorsFromMarquee():- WARNING!!! INVALID FEATURE VECTOR EXTRACTION OPTION");
					break;
			}

			string indices = DataTools.writeArray2String(frameIndices);
			Log.WriteIfVerbose("\tExtracted frame indices are:-" + indices);

			//initialise feature vectors for template. Each frame provides one vector in three parts
			//int coeffcount = M.GetLength(1);  //number of MFCC deltas etcs
			//int featureCount = coeffcount * 3;
			int indicesL = frameIndices.Length;
			int dT = SonogramConfiguration.DeltaT;
			double[,] M = sonogram.Data;

			FeatureVector[] fvs = new FeatureVector[indicesL];
			for (int i = 0; i < indicesL; i++)
			{
				Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + frameIndices[i]);
				//init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
				double[] acousticV = Speech.GetAcousticVector(M, frameIndices[i], dT); //combines  frames T-dT, T and T+dT
				fvs[i] = new FeatureVector(acousticV);
				// Wav source may not be from a file
				//fvs[i].SourceFile = TemplateState.WavFilePath; //assume all FVs have same source file
				fvs[i].SetFrameIndex(frameIndices[i]);
			}
			return fvs;
		}
	}

	public class LanguageModel
	{
		public LanguageModel(Configuration config, int fvCount, BaseSonogramConfig sonogramConfig, int sampleRate)
		{
			FrameOffset = GetFrameOffset(config, sonogramConfig, sampleRate);

			// THE LANGUAGE MODEL
			HmmType = MarkovModel.GetHmmType(config.GetString("MM_TYPE"));
			if (HmmType == HMMType.UNDEFINED)
				throw new ArgumentException("Configuration file is invalid - HmmType unrecognised");

			string mmName = config.GetString("MM_NAME");

			// READ TRAINING SEQUENCES
			WordCount = config.GetInt("NUMBER_OF_WORDS");
			if (WordCount < 1)
				throw new ArgumentException("Configuration file is invalid - No words defined in language model.");

			TrainingSequences ts = new TrainingSequences();
			for (int n = 0; n < WordCount; n++)
			{
				string name = config.GetString("WORD" + (n + 1) + "_NAME");
				for (int w = 0; w < 100; w++) // do not allow more than 100 examples
				{
					string word = config.GetString("WORD" + (n + 1) + "_EXAMPLE" + (w + 1));
					if (word == null)
						break;
					ts.AddSequence(name, word);
				}

			} // end for loop over all words
			Words = ts.GetSequences();

			MarkovModel mm;
			if (HmmType == HMMType.OLD_PERIODIC)
			{
				int period_ms = config.GetInt("PERIODICITY_MS");
				if (period_ms == -Int32.MaxValue)
					throw new ArgumentException("Configuration file is invalid - no periodicity specified..");

				mm = new MarkovModel(mmName, HmmType, period_ms, FrameOffset); //special constructor for two state periodic MM 
				mm.TrainModel(ts);
			}
			else if (HmmType == HMMType.TWO_STATE_PERIODIC)
			{
				int? gap_ms = config.GetIntNullable("GAP_MS");
				if (gap_ms == null)
					throw new ArgumentException("Configuration file is invalid - two state MM cannot be defined because gap duration is not definied in configuration.");
				mm = new MarkovModel(mmName, HmmType, gap_ms.Value, FrameOffset); //special constructor for two state periodic MM 
				mm.TrainModel(ts);
			}
			else
			{
				int numberOfStates = fvCount + 2; //because need extra for noise and for garbage
				mm = new MarkovModel(mmName, HmmType, numberOfStates);
				mm.DeltaT = FrameOffset; //the sequence time step
				mm.TrainModel(ts);
			}
			WordModel = mm; //one markov model per template
			//end setting up markov model

			SongWindow = config.GetDoubleNullable("SONG_WINDOW") ?? 1.0;
		}

		public void Save(TextWriter writer)
		{
			Configuration.WriteValue(writer, "MM_TYPE", HmmType);
			if (WordModel != null)
			{
				Configuration.WriteValue(writer, "MM_NAME", WordModel.Name);
				Configuration.WriteValue(writer, "PERIODICITY_MS", WordModel.Periodicity_ms);
				Configuration.WriteValue(writer, "GAP_MS", WordModel.Gap_ms);
			}
			Configuration.WriteValue(writer, "NUMBER_OF_WORDS", WordCount);
			// Although when read in the Words are split into different tags with multiple examples this information
			// is not stored (or used) so we can not persist it back. Instead we just write as if each word
			// is separate with 1 example each
			Configuration.WriteArray(writer, "WORD{0}_EXAMPLE1", Words);
			Configuration.WriteValue(writer, "SONG_WINDOW", SongWindow);
		}

		private double GetFrameOffset(Configuration config, BaseSonogramConfig sonogramConfig, int sampleRate)
		{
			return sonogramConfig.GetFrameDuration(sampleRate) * (1 - sonogramConfig.WindowOverlap); // Duration of non-overlapped part of window/frame in seconds
		}

		#region Properties
		public int WordCount { get; set; }
		public string[] Words { get; set; }
		public MarkovModel WordModel { get; set; }
		public HMMType HmmType { get; set; }
		public string HmmName { get; set; }
		public double SongWindow { get; set; } //window duration in seconds - used to calculate statistics
		public double FrameOffset { get; private set; }
		public double FramesPerSecond { get { return 1 / FrameOffset; } }		
		#endregion
	}

	public class FeatureVectorParameters
	{
		public FeatureVectorParameters(Configuration config)
		{
			//FEATURE VECTORS
			FeatureVectorSource = GetFVSource(config);
			switch (FeatureVectorSource)
			{
				case FV_Source.SELECTED_FRAMES:
					int? wordCount;
					FeatureVectorExtraction = GetFVExtraction(config, out wordCount);
					FeatureVectorExtractionInterval = wordCount;
					break;
				case FV_Source.MARQUEE:
					MarqueeStart = config.GetInt("MARQUEE_START");
					MarqueeEnd = config.GetInt("MARQUEE_END");
					break;
			}

			FeatureVector_DoAveraging = config.GetBoolean("FV_DO_AVERAGING");

			FeatureVectorCount = config.GetIntNullable("NUMBER_OF_FEATURE_VECTORS") ?? 0;
			FeatureVectorLength = config.GetIntNullable("FEATURE_VECTOR_LENGTH") ?? 0;
			FeatureVectorPaths = new string[FeatureVectorCount];
			for (int n = 0; n < FeatureVectorCount; n++)
				FeatureVectorPaths[n] = config.GetPath("FV" + (n + 1) + "_FILE");
			FeatureVector_SelectedFrames = new string[FeatureVectorCount];
			for (int n = 0; n < FeatureVectorCount; n++)
				FeatureVector_SelectedFrames[n] = config.GetString("FV" + (n + 1) + "_SELECTED_FRAMES");
			FVSourceFiles = new string[FeatureVectorCount];
			for (int n = 0; n < FeatureVectorCount; n++)
				FVSourceFiles[n] = config.GetString("FV" + (n + 1) + "_SOURCE_FILE");
			DefaultNoiseFVFile = config.GetPath("FV_DEFAULT_NOISE_FILE");
		}

		public void Save(TextWriter writer, string featureVectorFolder)
		{
			Configuration.WriteValue(writer, "MARQUEE_START", MarqueeStart);
			Configuration.WriteValue(writer, "MARQUEE_END", MarqueeEnd);
			Configuration.WriteValue(writer, "FV_DO_AVERAGING", FeatureVector_DoAveraging);
			Configuration.WriteValue(writer, "NUMBER_OF_FEATURE_VECTORS", FeatureVectorCount);
			Configuration.WriteValue(writer, "FEATURE_VECTOR_LENGTH", FeatureVectorLength);

			Configuration.WriteArray(writer, "FV{0}_FILE", FeatureVectorPaths);
			Configuration.WriteArray(writer, "FV{0}_SELECTED_FRAMES", FeatureVector_SelectedFrames);
			Configuration.WriteArray(writer, "FV{0}_SOURCE_FILE", FVSourceFiles);

			SaveFeatureVectors(featureVectorFolder, "FV{0}.txt");
		}

		#region Properties
		public FV_Source FeatureVectorSource { get; set; }
		public string[] FeatureVector_SelectedFrames { get; set; } //store frame IDs as string array
		public int MarqueeStart { get; set; }
		public int MarqueeEnd { get; set; }
		public FV_Extraction FeatureVectorExtraction { get; set; }
		public int? FeatureVectorExtractionInterval { get; set; }
		public bool FeatureVector_DoAveraging { get; set; }
		public string FeatureVector_DefaultNoiseFile { get; set; }

		public int FeatureVectorCount { get; set; }
		public int FeatureVectorLength { get; set; }
		public string[] FeatureVectorPaths { get; set; }
		public string[] FVSourceFiles { get; set; }
		public string DefaultNoiseFVFile { get; set; }
		public int ZscoreSmoothingWindow = 3; //NB!!!! THIS IS NO LONGER A USER DETERMINED PARAMETER 
		#endregion

		#region Feature Vector Parameter Reading
		public FV_Source GetFVSource(Configuration config)
		{
			if (!config.ContainsKey("FV_SOURCE"))
			{
				Log.WriteLine("Template.GetFVSource():- WARNING! NO SOURCE FOR FEATURE VECTORS IS DEFINED!");
				Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
				return FV_Source.SELECTED_FRAMES;
			}

			string value = config.GetString("FV_SOURCE");
			if (value.StartsWith("MARQUEE"))
			{
				return FV_Source.MARQUEE;

			}
			else if (value.StartsWith("SELECTED_FRAMES"))
				return FV_Source.SELECTED_FRAMES;
			else
			{
				Log.WriteLine("Template.GetFVSource():- WARNING! INVALID SOURCE FOR FEATURE VECTORS IS DEFINED! " + value);
				Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
				return FV_Source.SELECTED_FRAMES;
			}
		}

		public FV_Extraction GetFVExtraction(Configuration config, out int? wordCount)
		{
			wordCount = null;

			if (!config.ContainsKey("FV_EXTRACTION"))
			{
				Log.WriteLine("Template.GetFVExtraction():- WARNING! NO EXTRACTION PROCESS IS DEFINED FOR FEATURE VECTORS!");
				Log.WriteLine("                             SET THE DEFAULT:- FV_Extraction = AT_ENERGY_PEAKS");
				return FV_Extraction.AT_ENERGY_PEAKS;
			}

			string value = config.GetString("FV_EXTRACTION");
			if (value.StartsWith("AT_ENERGY_PEAKS"))
				return FV_Extraction.AT_ENERGY_PEAKS;
			else if (value.StartsWith("AT_FIXED_INTERVALS_OF_"))
			{
				string[] words = value.Split('_');
				int i;
				if (!int.TryParse(words[3], out i))
				{
					Log.WriteLine("Template.GetFVExtraction():- WARNING! INVALID INTEGER:- " + words[3]);
					wordCount = 0;
				}
				else
					wordCount = i;
				return FV_Extraction.AT_FIXED_INTERVALS;
			}
			else
			{
				Log.WriteLine("Template.GetFVExtraction():- WARNING! INVALID EXTRACTION VALUE IS DEFINED FOR FEATURE VECTORS! " + value);
				Log.WriteLine("                             SET THE DEFAULT:- FV_Extraction = AT_ENERGY_PEAKS");
				return FV_Extraction.AT_ENERGY_PEAKS;
			}
		}
		#endregion

		FeatureVector[] featureVectors;
		public FeatureVector[] FeatureVectors
		{
			get
			{
				if (featureVectors == null)
					featureVectors = LoadFeatureVectors();
				return featureVectors;
			}
			set
			{
				featureVectors = value;
				FeatureVectorPaths = null;
			}
		}

		FeatureVector[] LoadFeatureVectors()
		{
			var retVal = new FeatureVector[FeatureVectorCount];

			for (int n = 0; n < FeatureVectorCount; n++)
			{
				retVal[n] = new FeatureVector(FeatureVectorPaths[n], FeatureVectorLength)
				{
					FrameIndices = FeatureVector_SelectedFrames[n],
					SourceFile = FVSourceFiles[n]
				};
			}
			return retVal;
		}

		public void SaveFeatureVectors(string folder, string pattern)
		{
			Validation.Begin()
						.IsStateNotNull(featureVectors, "No feature vectors available to save.")
						.IsNotNull(folder, "Target folder must be supplied")
						.IsNotNull(pattern, "A pattern for feature vector filenames must be provided")
						.Check();

			FeatureVectorPaths = new string[featureVectors.Length];

			for (int i = 0; i < featureVectors.Length; i++)
			{
				var path = Path.Combine(folder, string.Format(pattern, i));
				featureVectors[i].SaveDataToFile(path);
				FeatureVectorPaths[i] = path;
			}
		}
	}
}