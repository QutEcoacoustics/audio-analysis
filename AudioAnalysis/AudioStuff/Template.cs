using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using TowseyLib;
using Props = TowseyLib.Configuration;

namespace AudioStuff
{
    public enum FV_Source { SELECTED_FRAMES, MARQUEE}
    public enum FV_Extraction { AT_ENERGY_PEAKS, AT_FIXED_INTERVALS }
    public enum TheGrammar { WORD_ORDER_RANDOM, WORD_ORDER_FIXED, WORDS_PERIODIC }

    /// <summary>
    /// Defines a single sound event in a sonogram
    /// </summary>
    /// <remarks>
    /// This class defines a template for a specific natural sound event.
    /// This could be the single syllable of a bird or frog call or a short slice of rain or cicada noise.
    /// </remarks>
    public class Template
	{
		#region Constants
		private const string templateDirName = "Template";
        private const string templateStemName = "template";
        private const string templateFExt = ".ini";
        private const string fvectorFExt  = ".txt";
        internal const double FractionalNH = 0.20; //arbitrary neighbourhood around user defined periodicity
		#endregion

		#region Static Utilities
		public static string GetTemplateFilePath(SonoConfig config, int callID)
		{
			return Utilities.PathCombine(config.TemplateParentDir, templateDirName + "_" + callID, templateStemName + "_" + callID + templateFExt);
		}
		#endregion

        public bool DoMelConversion { get; set; }
		public SonoConfig TemplateState { get; protected set; }
		public Sonogram Sonogram { get; protected set; }

		public FeatureVector[] FeatureVectors { get; protected set; }
      
        //info about TEMPLATE SCORING
        public double NoiseAv { get { return TemplateState.NoiseAv; } }
        public double NoiseSD { get { return TemplateState.NoiseSd; } }

        private bool verbose = true;

		public Template(SonoConfig config)
		{
			TemplateState = config;
			verbose = TemplateState.Verbosity > 0;

			InitializeFeatureVectors();
		}

        /// <summary>
        /// CONSTRUCTOR 1
        /// Use this constructor to read an existing template file
        /// </summary>
        public Template(string iniFPath, int id)
        {
            LogIfVerbose("\n#####  READING APPLICATION INI FILE :=" + iniFPath);
            TemplateState = new SonoConfig();
            TemplateState.ReadDefaultConfig(iniFPath);
			verbose = TemplateState.Verbosity > 0;

			var templateIniPath = GetTemplateFilePath(TemplateState, id);
			LogIfVerbose("\n#####  READING TEMPLATE INI FILE :=" + templateIniPath);
			TemplateState.ReadTemplateFile(templateIniPath);

			InitializeFeatureVectors();
        }

		public Template(string iniFPath, int callID, CallDescriptor callDescriptor)
			: this(iniFPath, callID, callDescriptor.callName, callDescriptor.callComment, callDescriptor.sourcePath, callDescriptor.destinationFileDescriptor)
		{}

        /// <summary>
        /// CONSTRUCTOR 2
        /// Use this constructor to create a new template
        /// </summary>
        public Template(string iniFPath, int callID, string callName, string callComment, string sourcePath, string destinationFileDescriptor)
        {
			LogIfVerbose("\n#####  READING APPLICATION INI FILE :=" + iniFPath);
            TemplateState = new SonoConfig();
            TemplateState.ReadDefaultConfig(iniFPath);
			verbose = TemplateState.Verbosity > 0;

            TemplateState.CallID = callID;
            TemplateState.CallName = callName;
            TemplateState.CallComment = callComment;
            TemplateState.FileDescriptor = destinationFileDescriptor;
            FileInfo fi = new FileInfo(sourcePath);
            string[] splitName = FileTools.SplitFileName(sourcePath);
            TemplateState.WavFileDir  = splitName[0];
            TemplateState.SourceFStem = splitName[1];
            TemplateState.WavFileExt  = splitName[2];
            TemplateState.SourceFName = splitName[1] + splitName[2];
            TemplateState.SourceFPath = Path.Combine(TemplateState.WavFileDir, TemplateState.SourceFName);
            var templateIniPath = GetTemplateFilePath(TemplateState, callID);
			LogIfVerbose("\ttemplatePath=" + templateIniPath);
			LogIfVerbose("\tsourcePath  =" + this.TemplateState.SourceFPath);
        }

		void InitializeFeatureVectors()
		{
			//INITIALIZE FEATURE VECTORS
			LogIfVerbose("\n#####  INITIALIZE FEATURE VECTORS");
			FeatureVector.Verbose = verbose;
			int fvCount = TemplateState.FeatureVectorCount;
			LogIfVerbose("        fvCount=" + fvCount);

			FeatureVectors = new FeatureVector[fvCount];
			for (int n = 0; n < fvCount; n++)
			{
				string path = TemplateState.FeatureVectorPaths[n];
				int fvLength = TemplateState.FeatureVectorLength;
				FeatureVectors[n] = new FeatureVector(path, fvLength, n + 1);
				FeatureVectors[n].FrameIndices = TemplateState.FeatureVector_SelectedFrames[n];
				FeatureVectors[n].SourceFile = TemplateState.FVSourceFiles[n];
			}
		}

		void LogIfVerbose(string format, params object[] args)
		{
			if (verbose)
				Log.WriteLine(format, args);
		}

        /// <summary>
        /// There are two Template.SetSonogram() methods.
        /// This one is called when READING AN EXISTING template to scan a new WAV recording. 
        /// </summary>
        public void SetSonogram(string wavPath)
        {
            Log.WriteLine("wavPath=" + wavPath);
            FileInfo fi = new FileInfo(wavPath);
            TemplateState.WavFileDir = fi.DirectoryName;
            TemplateState.WavFName   = fi.Name.Substring(0, fi.Name.Length - 4);//remove the file extention
            TemplateState.WavFileExt = fi.Extension;
            if (TemplateState.WavFName != null)
                TemplateState.SetDateAndTime(this.TemplateState.WavFName);

            TemplateState.SonogramType = SonogramType.acousticVectors; //to MAKE MATRIX OF dim 3x39 ACOUSTIC VECTORS

            //read the .WAV file
            WavReader wav = new WavReader(wavPath);
            //check the sampling rate
            int sr = wav.SampleRate;
            if (sr != TemplateState.SampleRate)
                throw new Exception("Template.SetSonogram(string wavPath):- Sampling rate of wav file not equal to that of template:  wavFile(" + sr + ") != template(" + this.TemplateState.SampleRate + ")");
			LogIfVerbose("Template.SetSonogram(string wavPath):- Sampling rates of wav file and template are equal: " + sr + " = " + this.TemplateState.SampleRate);

            //initialise Sonogram which also makes the sonogram
            Sonogram = new Sonogram(TemplateState, wav);
            Log.WriteLine("sonogram=" + Sonogram);
            Log.WriteLine("matrix dim =" + Sonogram.AcousticM.GetLength(0));
        }

		public void SetSonogram(AudioTools.StreamedWavReader wav)
		{
			TemplateState.SonogramType = SonogramType.acousticVectors; //to MAKE MATRIX OF dim 3x39 ACOUSTIC VECTORS

			//check the sampling rate
			if (wav.SampleRate != TemplateState.SampleRate)
				throw new Exception("Template.SetSonogram(string wavPath):- Sampling rate of wav file not equal to that of template:  wavFile(" + wav.SampleRate + ") != template(" + this.TemplateState.SampleRate + ")");
			LogIfVerbose("Template.SetSonogram(string wavPath):- Sampling rates of wav file and template are equal: " + wav.SampleRate + " = " + this.TemplateState.SampleRate);

			//initialise Sonogram which also makes the sonogram
			Sonogram = new Sonogram(TemplateState, wav);
			Log.WriteLine("sonogram=" + Sonogram);
			Log.WriteLine("matrix dim =" + Sonogram.AcousticM.GetLength(0));
		}

		public void SetExtractionParameters(FV_Source fvSource, FVExtractionParameters fvExtractionParameters)
		{
			SetExtractionParameters(fvSource, fvExtractionParameters.fv_Extraction, fvExtractionParameters.doFvAveraging, fvExtractionParameters.fvDefaultNoiseFile);
		}

        public void SetExtractionParameters(FV_Source fvSource, FV_Extraction fvExtraction, bool doFvAveraging, string defaultNoiseFile)
        {
            TemplateState.FeatureVectorSource = fvSource;
            TemplateState.FeatureVectorExtraction = fvExtraction;
            TemplateState.FeatureVector_DoAveraging = doFvAveraging;
            TemplateState.FeatureVector_DefaultNoiseFile = defaultNoiseFile;
        }

        public void SetFrequencyBounds(int minFreq, int maxFreq)
        {
            TemplateState.FreqBand_Min    = minFreq;
            TemplateState.FreqBand_Max    = maxFreq;
            TemplateState.MinTemplateFreq = minFreq;
            TemplateState.MaxTemplateFreq = maxFreq;
            TemplateState.MidTemplateFreq = minFreq + ((maxFreq - minFreq) / 2); //Hz
			LogIfVerbose("\tFreq bounds = " + this.TemplateState.MinTemplateFreq + " Hz - " + this.TemplateState.MaxTemplateFreq + " Hz");
        }

        public void SetMarqueeBounds(int minFreq, int maxFreq, int marqueeStart, int marqueeEnd)
        {
            SetFrequencyBounds(minFreq, maxFreq);
            TemplateState.MarqueeStart = marqueeStart;
            TemplateState.MarqueeEnd   = marqueeEnd;
        }

		public void SetLanguageModel(LanguageModel languageModel)
		{
			SetLanguageModel(languageModel.words, languageModel.grammar);
		}

        public void SetLanguageModel(string[] words, TheGrammar sp)
        {
            TemplateState.Words = words;
            TemplateState.WordCount =  words.Length;
            TemplateState.GrammarModel = sp;//three options are HOTSPOTS, WORDMATCH, PERIODICITY
        }

        public void SetScoringParameters(double zThreshold, int period_ms)
        {
            TemplateState.ZScoreThreshold = zThreshold; //options are 1.98, 2.33, 2.56, 3.1
            TemplateState.WordPeriodicity_ms = period_ms;
            int period_frame = (int)Math.Round(period_ms / TemplateState.FrameOffset / (double)1000);
            TemplateState.WordPeriodicity_frames = period_frame;
			TemplateState.WordPeriodicity_NH_frames = (int)Math.Floor(period_frame * Template.FractionalNH); //arbitrary NH
			TemplateState.WordPeriodicity_NH_ms = (int)Math.Floor(period_ms * Template.FractionalNH); //arbitrary NH
            //Log.WriteLine("period_ms="    + period_ms    + "+/-" + this.TemplateState.CallPeriodicity_NH_ms);
            //Log.WriteLine("period_frame=" + period_frame + "+/-" + this.TemplateState.CallPeriodicity_NH_frames);
        }

		public void SetSonogram(MfccParameters mfccParameters, double dynamicRange)
		{
			SetSonogram(mfccParameters.frameSize, mfccParameters.frameOverlap, dynamicRange, mfccParameters.filterBankCount,
				mfccParameters.doMelConversion, mfccParameters.doNoiseReduction, mfccParameters.ceptralCoeffCount, 
				mfccParameters.deltaT, mfccParameters.includeDeltaFeatures, mfccParameters.includeDoubleDeltaFeatures);
		}

        /// <summary>
        /// There are two Template.SetSonogram() methods.
        /// This one is called when CREATING a new template and extracting feature vectors. 
        /// NOTE: All these template parameters override the default values set in the application's sonogram.ini file.
        /// </summary>
        public void SetSonogram(int frameSize, double frameOverlap, double dynamicRange, int filterBankCount,
                                bool doMelConversion, bool doNoiseReduction,
                                int ceptralCoeffCount, int deltaT, bool includeDeltaFeatures, bool includeDoubleDeltaFeatures)
        {
            TemplateState.WindowSize    = frameSize;
            TemplateState.WindowOverlap = frameOverlap;
            TemplateState.SonogramType  = SonogramType.acousticVectors; //to MAKE MATRIX OF dim 3x39 ACOUSTIC VECTORS
            DoMelConversion = doMelConversion;
            TemplateState.DoNoiseReduction = doNoiseReduction;
            TemplateState.DeltaT = deltaT;
            TemplateState.IncludeDelta = includeDeltaFeatures;
            TemplateState.IncludeDoubleDelta = includeDoubleDeltaFeatures;

            //init Sonogram. SonogramType already set to make matrix of acoustic vectors
            Sonogram = new Sonogram(TemplateState, TemplateState.SourceFPath);

            //this.state.FrameDuration = state.WindowSize / (double)state.SampleRate; // window duration in seconds
            //this.state.FrameOffset   = this.state.FrameDuration * (1 - this.state.WindowOverlap);// duration in seconds
            //this.state.FreqBinCount  = this.state.WindowSize / 2; // other half is phase info
            //this.state.MelBinCount   = this.state.FreqBinCount; // same has number of Hz bins
            //this.state.FBinWidth     = this.state.NyquistFreq / (double)this.state.FreqBinCount;
            //this.state.FrameCount = (int)(this.state.TimeDuration / this.state.FrameOffset);
            //this.state.FramesPerSecond = 1 / this.state.FrameOffset;
        }

        public void SetExtractionInterval(int fvExtractionInterval)
        {
            TemplateState.FeatureVectorExtractionInterval = fvExtractionInterval;
        }
        /// <summary>
        /// this method is called from the user interface.
        /// It expects a comma separate list of one or more integers
        /// </summary>
        /// <param name="selectedFrames"></param>
        public void SetSelectedFrames(string selectedFrames)
        {
            string[] IDs = selectedFrames.Split(',');
            int count = IDs.Length;
            string[] indices = new string[count];
            for (int i = 0; i < count; i++) indices[i] = IDs[i];
            TemplateState.FeatureVector_SelectedFrames = IDs;
        }

		public void SetSongParameters(LanguageModel languageModel)
		{
			SetSongParameters(languageModel.maxSyllables, languageModel.maxSyllableGap, languageModel.SongWindow);
		}

        public void SetSongParameters(int maxSyllables, double maxSyllableGap, double songWindow)
        {
            TemplateState.SongWindow = songWindow;
        }

        /// <summary>
        /// LOGIC FOR EXTRACTION OF FEATURE VECTORS FROM SONOGRAM ****************************************************************
        /// </summary>
        public void ExtractTemplateFromSonogram(int callID)
        {
            Log.WriteLine("\nEXTRACTING TEMPLATE USING SUPPLIED PARAMETERS");
            FeatureVector.Verbose = this.verbose;

			if (TemplateState.FeatureVectorSource == FV_Source.SELECTED_FRAMES)
				FeatureVectors = GetFeatureVectorsFromFrames();
			else
			{
				if (TemplateState.FeatureVectorSource == FV_Source.MARQUEE)
				{
					if (TemplateState.FeatureVectorExtraction == FV_Extraction.AT_ENERGY_PEAKS)
						FeatureVectors = GetFeatureVectorsFromMarquee();
					else if (TemplateState.FeatureVectorExtraction == FV_Extraction.AT_FIXED_INTERVALS)
						FeatureVectors = GetFeatureVectorsFromMarquee();
					else
						Log.WriteLine("Template.ExtractTemplateFromSonogram(: WARNING!! INVALID FV EXTRACTION OPTION!)");
				}
				else
				{
					Log.WriteLine("Template.ExtractTemplateFromSonogram(: WARNING!! INVALID FV SOURCE OPTION!)");
				}
			}

            //SAVE FEATURE VECTORS TO DISK
            int fvCount  = FeatureVectors.Length;
            string dirPath = TemplateState.TemplateParentDir + templateDirName + "_" + callID+"\\";
            TemplateState.TemplateDir = dirPath; 
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            dir.Create();

            //accumulate the acoustic vectors from multiple frames into an averaged feature vector
            if (this.TemplateState.FeatureVector_DoAveraging)
            {
				LogIfVerbose("\nSAVING SINGLE TEMPLATE: as average of " + fvCount + " FEATURE VECTORS");
                int id = 1;
                FeatureVector avFV = FeatureVector.AverageFeatureVectors(FeatureVectors, id);
				string path = dirPath + templateStemName + "_" + callID + "_" + TemplateState.FileDescriptor + "_FV1" + fvectorFExt;
                if (avFV != null) avFV.SaveDataAndImageToFile(path, TemplateState);
                //save av fv in place of originals
                FeatureVectors = new FeatureVector[1];
                FeatureVectors[0] = avFV;
                TemplateState.FeatureVectorCount = 1;
                TemplateState.FeatureVectorLength = avFV.FvLength;
                WriteTemplateIniFile(GetTemplateFilePath(TemplateState, callID));
            }
            else //save the feature vectors separately
            {
				LogIfVerbose("SAVING " + fvCount + " SEPARATE TEMPLATE FILES");
                for (int i = 0; i < fvCount; i++)
                {
					string path = dirPath + templateStemName + "_" + callID + "_" + this.TemplateState.FileDescriptor + "_FV" + (i + 1) + fvectorFExt;
                    FeatureVectors[i].SaveDataAndImageToFile(path, this.TemplateState);
                    TemplateState.FeatureVectorCount = FeatureVectors.Length;
                    TemplateState.FeatureVectorLength = FeatureVectors[0].FvLength;
                }
				WriteTemplateIniFile(GetTemplateFilePath(TemplateState, callID));
            }

        } // end ExtractTemplateFromSonogram()   

        public FeatureVector[] GetFeatureVectorsFromFrames()
        {
			LogIfVerbose("\nEXTRACTING FEATURE VECTORS FROM FRAMES:- method Template.GetFeatureVectorsFromFrames()");
            //Get frame indices. Assume, when extracting a FeatureVector, that there is only one frame ID per FVector
            string[] IDs = this.TemplateState.FeatureVector_SelectedFrames;
            int indicesL = IDs.Length;

            //initialise feature vectors for template. Each frame provides one vector in three parts
            int dT = this.TemplateState.DeltaT;
            double[,] M = Sonogram.CepstralM;

            FeatureVector[] fvs = new FeatureVector[indicesL];
            for (int i = 0; i < indicesL; i++)
            {
                int id = Int32.Parse(IDs[i]);
				LogIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + id);
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, id, dT); //combines  frames T-dT, T and T+dT
                fvs[i] = new FeatureVector(acousticV, i+1); //avoid FV id = 0. Reserve this for noise vector
                fvs[i].SourceFile = this.TemplateState.SourceFName; //assume all FVs have same source file
                fvs[i].FrameIndices = IDs[i];
            }
            return fvs;
        }

        public FeatureVector[] GetFeatureVectorsFromMarquee()
        {
            int start = this.TemplateState.MarqueeStart;
            int end   = this.TemplateState.MarqueeEnd;
            int marqueeFrames = end - start + 1;
            double marqueeDuration = marqueeFrames * this.TemplateState.FrameDuration;
			LogIfVerbose("\tMarquee start=" + start + ",  End=" + end + ",  Duration= " + marqueeFrames + "frames =" + marqueeDuration.ToString("F2") + "s");
            int[] frameIndices = null;

            if (this.TemplateState.FeatureVectorExtraction == FV_Extraction.AT_FIXED_INTERVALS)
            {
                int interval = (int)(this.TemplateState.FeatureVectorExtractionInterval / this.TemplateState.FrameDuration /(double)1000);
				LogIfVerbose("\tFrame interval=" + interval + "ms");
                frameIndices = FeatureVector.GetFrameIndices(start, end, interval);
            }
            else if (this.TemplateState.FeatureVectorExtraction == FV_Extraction.AT_ENERGY_PEAKS)
            {
                double[] frameEnergy = this.Sonogram.Decibels;
                double energyThreshold = this.TemplateState.SegmentationThreshold_k1;
                frameIndices = FeatureVector.GetFrameIndices(start, end, frameEnergy, energyThreshold);
                LogIfVerbose("\tEnergy threshold=" + energyThreshold.ToString("F2"));
            }
            else
                Log.WriteLine("Template.GetFeatureVectorsFromMarquee():- WARNING!!! INVALID FEATURE VECTOR EXTRACTION OPTION");

            string indices = DataTools.writeArray2String(frameIndices);
			LogIfVerbose("\tExtracted frame indices are:-" + indices);

            //initialise feature vectors for template. Each frame provides one vector in three parts
            //int coeffcount = M.GetLength(1);  //number of MFCC deltas etcs
            //int featureCount = coeffcount * 3;
            int indicesL = frameIndices.Length;
            int dT = this.TemplateState.DeltaT;
            double[,] M = Sonogram.CepstralM;

            FeatureVector[] fvs = new FeatureVector[indicesL];
            for (int i = 0; i < indicesL; i++)
            {
				LogIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + frameIndices[i]);
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, frameIndices[i], dT); //combines  frames T-dT, T and T+dT
                fvs[i] = new FeatureVector(acousticV, i + 1); //avoid FV id = 0. Reserve this for noise vector
                fvs[i].SourceFile = this.TemplateState.SourceFName; //assume all FVs have same source file
                fvs[i].SetFrameIndex(frameIndices[i]);
            }
            return fvs;
        }

        public void WriteTemplateIniFile(string path)
        {
            //write the call data to a file
            ArrayList data = new ArrayList();
            data.Add("DATE=" + DateTime.Now.ToString("u"));
            data.Add("#");
            data.Add("#**************** TEMPLATE DATA");
            data.Add("TEMPLATE_ID=" + this.TemplateState.CallID);
            data.Add("CALL_NAME=" + this.TemplateState.CallName);
            data.Add("COMMENT=" + this.TemplateState.CallComment);
			data.Add("THIS_FILE=" + path);

            data.Add("#");
            data.Add("#**************** INFO ABOUT ORIGINAL .WAV FILE");
            data.Add("WAV_FILE_NAME=" + this.TemplateState.SourceFName);
            data.Add("WAV_SAMPLE_RATE=" + this.TemplateState.SampleRate);
            data.Add("WAV_DURATION=" + this.TemplateState.TimeDuration.ToString("F3"));

            data.Add("#");
            data.Add("#**************** INFO ABOUT FRAMES");
            data.Add("FRAME_SIZE=" + this.TemplateState.WindowSize);
            data.Add("FRAME_OVERLAP=" + this.TemplateState.WindowOverlap);
            data.Add("FRAME_DURATION_MS=" + (this.TemplateState.FrameDuration * 1000).ToString("F3"));//convert to milliseconds
            data.Add("FRAME_OFFSET_MS=" + (this.TemplateState.FrameOffset * 1000).ToString("F3"));//convert to milliseconds
            data.Add("NUMBER_OF_FRAMES=" + this.TemplateState.FrameCount);
            data.Add("FRAMES_PER_SECOND=" + this.TemplateState.FramesPerSecond.ToString("F3"));

            data.Add("#");
            data.Add("#**************** INFO ABOUT FEATURE EXTRACTION");
            data.Add("NYQUIST_FREQ=" + this.TemplateState.NyquistFreq);
            data.Add("WINDOW_FUNCTION=" + this.TemplateState.WindowFncName);
            data.Add("NUMBER_OF_FREQ_BINS=" + this.TemplateState.FreqBinCount);
            data.Add("FREQ_BIN_WIDTH=" + this.TemplateState.FBinWidth.ToString("F2"));
            data.Add("MIN_FREQ=" + this.TemplateState.MinTemplateFreq);
            data.Add("MAX_FREQ=" + this.TemplateState.MaxTemplateFreq);
            data.Add("MID_FREQ=" + this.TemplateState.MidTemplateFreq);
            data.Add("DO_MEL_CONVERSION=" + this.DoMelConversion);
            data.Add("DO_NOISE_REDUCTION=" + this.TemplateState.DoNoiseReduction);

            data.Add("DELTA_T=" + this.TemplateState.DeltaT);
            data.Add("INCLUDE_DELTA=" + this.TemplateState.IncludeDelta);
            data.Add("INCLUDE_DOUBLEDELTA=" + this.TemplateState.IncludeDoubleDelta);
            data.Add("FILTERBANK_COUNT=" + this.TemplateState.FilterbankCount);
            data.Add("CC_COUNT=" + this.TemplateState.ccCount);
            data.Add("DYNAMIC_RANGE=" + this.TemplateState.MaxDecibelReference); //decibels above noise level #### YET TO DO THIS PROPERLY
            data.Add("#");


            data.Add("#**************** FV EXTRACTION OPTIONS **************************");
            data.Add("FV_SOURCE="+FV_Source.SELECTED_FRAMES.ToString());
            if (this.TemplateState.FeatureVectorSource == FV_Source.SELECTED_FRAMES)
            {
                data.Add("FV_SELECTED_FRAMES=" + this.TemplateState.FeatureVector_SelectedFrames[0]);
            } else
            if (this.TemplateState.FeatureVectorSource == FV_Source.MARQUEE)
            {
                data.Add("MARQUEE_START="+this.TemplateState.MarqueeStart);
                data.Add("MARQUEE_END=" + this.TemplateState.MarqueeEnd);
                if (this.TemplateState.FeatureVectorExtraction == FV_Extraction.AT_ENERGY_PEAKS)
                {
                    data.Add("FV_EXTRACTION=AT_ENERGY_PEAKS");
                } else
                if (this.TemplateState.FeatureVectorExtraction == FV_Extraction.AT_FIXED_INTERVALS)
                {
                    data.Add("FV_EXTRACTION=AT_FIXED_INTERVALS_OF_" + this.TemplateState.FeatureVectorExtractionInterval + "_MS");
                } 
            }
            data.Add("FV_DO_AVERAGING="+this.TemplateState.FeatureVector_DoAveraging);
            data.Add("#");


            data.Add("#**************** INFO ABOUT FEATURE VECTORS - THE ACOUSTIC MODEL ***************");
            int fvCount = FeatureVectors.Length;
            data.Add("FEATURE_VECTOR_LENGTH=" + FeatureVectors[0].FvLength); //117
            data.Add("NUMBER_OF_FEATURE_VECTORS="+fvCount);
            for (int n = 0; n < fvCount; n++) 
            {
                FeatureVector fv = FeatureVectors[n];
                data.Add("FV" + (n + 1) + "_FILE="+fv.VectorFPath);
                data.Add("FV" + (n + 1) + "_SELECTED_FRAMES=" + fv.FrameIndices);
                data.Add("FV" + (n + 1) + "_SOURCE_FILE=" + fv.SourceFile);
            }
            data.Add(@"FV_DEFAULT_NOISE_FILE=" + this.TemplateState.FeatureVector_DefaultNoiseFile);
            data.Add("#");

            data.Add("#THRESHOLDS FOR THE ACOUSTIC MODEL");
            data.Add("#THRESHOLD OPTIONS: 3.1(p=0.001), 2.58(p=0.005), 2.33(p=0.01), 2.15(p=0.03), 1.98(p=0.05),");
            data.Add("ZSCORE_THRESHOLD=" + this.TemplateState.ZScoreThreshold);
            data.Add("#");

            data.Add("#**************** INFO ABOUT LANGUAGE MODEL");

            if (this.TemplateState.GrammarModel == TheGrammar.WORD_ORDER_RANDOM)
            {
                this.TemplateState.WordCount = fvCount;
                data.Add("    When the LANGUAGE MODEL == WORD_ORDER_RANDOM, there is automatically one syllable/word per feature vector");
                data.Add("NUMBER_OF_WORDS=" + fvCount);
                for (int n = 0; n < fvCount; n++) data.Add("WORD" + (n + 1) + "=" + FeatureVector.alphabet[n + 1]);
            }
            else  //automate the language, one symbol per feature vector
            {
                data.Add("NUMBER_OF_WORDS=" + this.TemplateState.WordCount);
                for (int n = 0; n < this.TemplateState.WordCount; n++)
                {
                    data.Add("WORD" + (n + 1) + "=" + this.TemplateState.Words[n]);
                }
            }

            data.Add("#There are three choices of grammar(1)WORD_ORDER_RANDOM (2)WORD_ORDER_FIXED (3)WORDS_PERIODIC");
            data.Add("GRAMMAR=" + this.TemplateState.GrammarModel);
            if (this.TemplateState.GrammarModel == TheGrammar.WORDS_PERIODIC)
            {
                data.Add("WORD_PERIODICITY_MS=" + this.TemplateState.WordPeriodicity_ms);
            }else
            {
                data.Add("SONG_WINDOW=" + this.TemplateState.SongWindow.ToString("F1"));
            }
            data.Add("#");

            //maxSyllables=
            //double maxSyllableGap = 0.25; //seconds
            //double maxSong=

            //write template to file
			FileTools.WriteTextFile(path, data);
        } // end of WriteTemplateConfigFile()

		//public void WriteTemplateConfigFile_OLD()
        //{

        //    data.Add("#\n#INFO ABOUT CALL TEMPLATE");
        //    data.Add("FEATURE_VECTOR_FILE=" + matrixFName);
        //    data.Add("# NOTE: Each row of the template matrix is the power spectrum for a given time step.");
        //    data.Add("#       That is, rows are time steps and columns are frequency bins.");
        //    data.Add("# IMAGE COORDINATES USED TO EXTRACT CALL");
        //    data.Add("X1=" + this.x1);
        //    data.Add("Y1=" + this.y1);
        //    data.Add("X2=" + this.x2);
        //    data.Add("Y2=" + this.y2);
        //    data.Add("# CORRESPONDING SONOGRAM COORDINATES");
        //    data.Add("TIMESTEP1=" + this.t1);
        //    data.Add("TIMESTEP2=" + this.t2);
        //    data.Add("FREQ_BIN1=" + this.bin1);
        //    data.Add("FREQ_BIN2=" + this.bin2);

        //    data.Add("TEMPLATE_DURATION=" + this.templateDuration.ToString("F3") + "s");
        //    data.Add("TEMPLATE_SPEC_COUNT=" + this.templateSpectralCount + "(time-steps)");
        //    //write data to file
        //    FileTools.WriteTextFile(this.templatePath, data);

        //} // end of WriteTemplateConfigFile()

        public void WriteInfo2STDOUT()
        {
            Log.WriteLine("\nTEMPLATE INFO");
            Log.WriteLine(" Template ID: " + TemplateState.CallID);
            Log.WriteLine(" Template name: " + TemplateState.CallName);
            Log.WriteLine(" Comment: " + TemplateState.CallComment);
            Log.WriteLine(" Template dir     : " + TemplateState.TemplateParentDir);
            Log.WriteLine(" Template ini file: ");
            Log.WriteLine(" Bottom freq=" + TemplateState.MinTemplateFreq + "  Mid freq=" + TemplateState.MidTemplateFreq + " Top freq=" + TemplateState.MaxTemplateFreq);

            Log.WriteLine(" NUMBER_OF_FEATURE_VECTORS=" + TemplateState.FeatureVectorCount);
            Log.WriteLine(" FEATURE_VECTOR_LENGTH=" + TemplateState.FeatureVectorLength);
            Log.WriteLine(" Feature Vector Source Method = "+ TemplateState.FeatureVectorSource);
            if (TemplateState.FeatureVectorSource != FV_Source.SELECTED_FRAMES)
                Log.WriteLine("     Feature Vector Extraction Method = " + TemplateState.FeatureVectorExtraction);
            Log.WriteLine(" NUMBER_OF_WORDS=" + TemplateState.WordCount);
            Log.WriteLine(" Scoring Protocol = " + TemplateState.GrammarModel);
       }
    }//end Class Template
}