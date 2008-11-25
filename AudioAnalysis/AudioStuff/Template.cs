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

    /// <summary>
    /// Defines a single sound event in a sonogram
    /// </summary>
    /// 
    /// <remarks>
    /// This class defines a template for a specific natural sound event.
    /// This could be the single syllable of a bird or frog call or a short slice of rain or cicada noise.
    /// </remarks>
    /// 
    public class Template
    {
        private const string templateDirName = "Template";
        private const string templateStemName = "template";
        private const string templateFExt = ".ini"; //.tmpl
        private const string fvectorFExt  = ".txt";

        public int CallID { get; set; }
		public SonoConfig TemplateState { get; set; }
		public Sonogram Sonogram { get; set; }

        //file names and directory
		public string TemplateDir { get; set; }
		public string TemplateIniPath { get; set; }

        //the FEATURE VECTORS
		public FeatureVector[] FeatureVectors { get; private set; }

        //info about OLD TEMPLATE EXTRACTION
        //int x1; int y1; //image coordinates for top left of selection
        //int x2; int y2; //image coordinates for bottom right of selection
        //int t1; int t2; //sonogram time interval selection
        //int bin1; int bin2; //sonogram freq bin interval 
        //double templateDuration; //duration of template in seconds
        //int templateSpectralCount; //number of spectra in template

        //info about TEMPLATE SCORING
        /*public double NoiseAv { get { return TemplateState.NoiseAv; } }
        public double NoiseSD { get { return TemplateState.NoiseSd; } }*/

        /// <summary>
        /// CONSTRUCTOR 1
        /// Use this constructor to read an EXISTING template
        /// </summary>
        public Template(string iniFPath, int id)
        {
            this.CallID = id;
            this.TemplateState = new SonoConfig();

            Log.WriteIfVerbose("\n##### TEMPLATE CONSTRUCTOR 1: READING APPLICATION INI FILE :=" + iniFPath);
            this.TemplateState.ReadConfig(iniFPath); //read the ini file for default parameters

            this.TemplateDir = Path.Combine(Path.GetDirectoryName(iniFPath), Template.templateDirName + "_" + id);
            string templatePath = Path.Combine(TemplateDir, templateStemName + "_" + id + templateFExt);

			ReadTemplateFile(templatePath);
        }

		public Template(SonoConfig config, string templateFile)
		{
			TemplateState = config;
			ReadTemplateFile(templateFile);
		}

		private void ReadTemplateFile(string templatePath)
		{
			Log.WriteIfVerbose("\n#####  READING TEMPLATE INI FILE :=" + templatePath);
			int status = ReadTemplateFile(templatePath, this.TemplateState);//read the template configuration file

			//INITIALIZE FEATURE VECTORS
			Log.WriteIfVerbose("\n#####  INITIALIZE FEATURE VECTORS");
			int fvCount = TemplateState.FeatureVectorCount;
			Log.WriteIfVerbose("        fvCount=" + fvCount);
			this.FeatureVectors = new FeatureVector[fvCount];
			for (int n = 0; n < fvCount; n++)
			{
				string path = TemplateState.FeatureVectorPaths[n];
				int fvLength = TemplateState.FeatureVectorLength;
				FeatureVectors[n] = new FeatureVector(path, fvLength, n + 1);
				FeatureVectors[n].FrameIndices = TemplateState.FeatureVector_SelectedFrames[n];
				FeatureVectors[n].SourceFile = TemplateState.FVSourceFiles[n];
			}
		}

        /// <summary>
        /// CONSTRUCTOR 2
        /// Use this constructor to create a NEW template
        /// </summary>
        public Template(string iniFPath, int callID, string callName, string callComment, string sourcePath, string destinationFileDescriptor)
        {
            Log.WriteIfVerbose("\n#####  READING APPLICATION INI FILE :=" + iniFPath);
            TemplateState = new SonoConfig();
            TemplateState.ReadConfig(iniFPath);//read the ini file for default parameters

            CallID = callID;
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
            TemplateState.SourceFStem = splitName[1];
            TemplateState.SourceFPath = this.TemplateState.WavFileDir + this.TemplateState.SourceFName;

            TemplateDir = Path.Combine(Path.GetDirectoryName(iniFPath), Template.templateDirName + "_" + callID);
            TemplateIniPath = this.TemplateDir + Template.templateStemName + "_" + callID + templateFExt;

            Log.WriteIfVerbose("\ttemplatePath=" + TemplateIniPath);
            Log.WriteIfVerbose("\tsourcePath  =" + this.TemplateState.SourceFPath);
        }

        public void SetExtractionParameters(FV_Source fvSource, FV_Extraction fvExtraction, bool doFvAveraging, 
                                                                                        string defaultNoiseFile, double zThreshold)
        {
            TemplateState.FeatureVectorSource = fvSource;
            TemplateState.FeatureVectorExtraction = fvExtraction;
            TemplateState.FeatureVector_DoAveraging = doFvAveraging;
            TemplateState.FeatureVector_DefaultNoiseFile = defaultNoiseFile;
            TemplateState.ZScoreThreshold = zThreshold; //options are 1.98, 2.33, 2.56, 3.1
        }

        public void SetFrequencyBounds(int minFreq, int maxFreq)
        {
            TemplateState.FreqBand_Min    = minFreq;
            TemplateState.FreqBand_Max    = maxFreq;
            TemplateState.MinTemplateFreq = minFreq;
            TemplateState.MaxTemplateFreq = maxFreq;
            TemplateState.MidTemplateFreq = minFreq + ((maxFreq - minFreq) / 2); //Hz
			Log.WriteIfVerbose("\tFreq bounds = {0} Hz - {1} Hz", this.TemplateState.MinTemplateFreq, this.TemplateState.MaxTemplateFreq + "");
        }

        public void SetMarqueeBounds(int minFreq, int maxFreq, int marqueeStart, int marqueeEnd)
        {
            SetFrequencyBounds(minFreq, maxFreq);
            TemplateState.MarqueeStart = marqueeStart;
            TemplateState.MarqueeEnd   = marqueeEnd;
        }

        public void SetLanguageModel(HMMType type, string name)
        {
			//Log.WriteLine("\nGOT TO SetLanguageModel");
            TemplateState.HmmType = type;
            TemplateState.HmmName = name;
            TemplateState.Words = null;
            TemplateState.WordCount = 0;
        }

        /// <summary>
        /// CREATES a new template and extracts feature vectors. 
        /// NOTE: All these template parameters override the default values set in the application's sonogram.ini file.
        /// </summary>
        public void SetSonogram(int frameSize, double frameOverlap, double dynamicRange, int filterBankCount,
                                bool doMelConversion, bool doNoiseReduction,
                                int ceptralCoeffCount, int deltaT, bool includeDeltaFeatures, bool includeDoubleDeltaFeatures)
        {
            TemplateState.WindowSize    = frameSize;
            TemplateState.WindowOverlap = frameOverlap;
            //TemplateState.DynamicRange = dynamicRange; //not yet implemented
            TemplateState.FilterbankCount = filterBankCount;
            TemplateState.DoMelScale = doMelConversion;
            TemplateState.DoNoiseReduction = doNoiseReduction;
            TemplateState.DeltaT = deltaT;
            TemplateState.IncludeDelta = includeDeltaFeatures;
            TemplateState.IncludeDoubleDelta = includeDoubleDeltaFeatures;

            //initialise sonogram. First set SonogramType to make matrix of acoustic vectors
            TemplateState.SonogramType = SonogramType.acousticVectors; //to MAKE MATRIX OF dim 3x39 ACOUSTIC VECTORS
            Sonogram = new Sonogram(TemplateState, TemplateState.SourceFPath);

            //state.FrameDuration = state.WindowSize / (double)state.SampleRate; // window duration in seconds
            //state.FrameOffset   = this.state.FrameDuration * (1 - this.state.WindowOverlap);// duration in seconds
            //state.FreqBinCount  = this.state.WindowSize / 2; // other half is phase info
            //state.MelBinCount   = this.state.FreqBinCount; // same has number of Hz bins
            //state.FBinWidth     = this.state.NyquistFreq / (double)this.state.FreqBinCount;
            //state.FrameCount = (int)(this.state.TimeDuration / this.state.FrameOffset);
            //state.FramesPerSecond = 1 / this.state.FrameOffset;
        }

        public void SetExtractionInterval(int fvExtractionInterval)
        {
            this.TemplateState.FeatureVectorExtractionInterval = fvExtractionInterval;
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
            this.TemplateState.FeatureVector_SelectedFrames = IDs;
        }

        public void SetSongParameters(int maxSyllables, double maxSyllableGap, double songWindow)
        {
            this.TemplateState.SongWindow = songWindow;
        }

        /// <summary>
        /// LOGIC FOR EXTRACTION OF FEATURE VECTORS FROM SONOGRAM ****************************************************************
        /// </summary>
        public void ExtractTemplateFromSonogram()
        {
			Log.WriteLine("\nEXTRACTING TEMPLATE USING SUPPLIED PARAMETERS");
			Log.WriteLine("\tTemplate.ExtractTemplateFromSonogram()");
			Log.WriteLine("\tSource of feature vectors = " + this.TemplateState.FeatureVectorSource);
             
            if (this.TemplateState.FeatureVectorSource == FV_Source.SELECTED_FRAMES)
                this.FeatureVectors = GetFeatureVectorsFromFrames();
			else if (this.TemplateState.FeatureVectorSource == FV_Source.MARQUEE)
            {
				switch (TemplateState.FeatureVectorExtraction)
				{
					case FV_Extraction.AT_ENERGY_PEAKS:
						this.FeatureVectors = GetFeatureVectorsFromMarquee();
						break;
					case FV_Extraction.AT_FIXED_INTERVALS:
						this.FeatureVectors = GetFeatureVectorsFromMarquee();
						break;
					default:
						Log.WriteLine("Template.ExtractTemplateFromSonogram(: WARNING!! INVALID FV EXTRACTION OPTION!)");
						break;
				}
            }
			else
				Log.WriteLine("Template.ExtractTemplateFromSonogram(: WARNING!! INVALID FV SOURCE OPTION!)");

            //SAVE FEATURE VECTORS TO DISK
            int fvCount  = FeatureVectors.Length;
            string dirPath = this.TemplateState.TemplateParentDir + templateDirName + "_" + this.CallID+"\\";
            this.TemplateState.TemplateDir = dirPath; 
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            dir.Create();

            //accumulate the acoustic vectors from multiple frames into an averaged feature vector
            if (this.TemplateState.FeatureVector_DoAveraging)
            {
                Log.WriteIfVerbose("\nSAVING SINGLE TEMPLATE: as average of " + fvCount + " FEATURE VECTORS");
                int id = 1;
                FeatureVector avFV = FeatureVector.AverageFeatureVectors(this.FeatureVectors, id);
                string path = dirPath + templateStemName + "_" + this.CallID + "_" +this.TemplateState.FileDescriptor+"_FV1" + fvectorFExt;
                if (avFV != null) avFV.SaveDataAndImageToFile(path, this.TemplateState);
                //save av fv in place of originals
                FeatureVectors = new FeatureVector[1];
                FeatureVectors[0] = avFV;
                TemplateState.FeatureVectorCount = 1;
                TemplateState.FeatureVectorLength = avFV.FvLength;
                WriteTemplateIniFile();
            }
            else //save the feature vectors separately
            {
                Log.WriteIfVerbose("SAVING " + fvCount + " SEPARATE TEMPLATE FILES");
                for (int i = 0; i < fvCount; i++)
                {
                    string path = dirPath + templateStemName + "_" + this.CallID + "_" + this.TemplateState.FileDescriptor + "_FV" + (i + 1) + fvectorFExt;
                    FeatureVectors[i].SaveDataAndImageToFile(path, this.TemplateState);
                    TemplateState.FeatureVectorCount = FeatureVectors.Length;
                    TemplateState.FeatureVectorLength = FeatureVectors[0].FvLength;
                }
                WriteTemplateIniFile();
            }

        } // end ExtractTemplateFromSonogram()   

        public FeatureVector[] GetFeatureVectorsFromFrames()
        {
            Log.WriteIfVerbose("\nEXTRACTING FEATURE VECTORS FROM FRAMES:- method Template.GetFeatureVectorsFromFrames()");
            //Get frame indices. Assume, when extracting a FeatureVector, that there is only one frame ID per FVector
            string[] IDs = this.TemplateState.FeatureVector_SelectedFrames;
            int indicesL = IDs.Length;

            //initialise feature vectors for template. Each frame provides one vector in three parts
            int dT = this.TemplateState.DeltaT;
            double[,] M = this.Sonogram.CepstralM;

            FeatureVector[] fvs = new FeatureVector[indicesL];
            for (int i = 0; i < indicesL; i++)
            {
                int id = Int32.Parse(IDs[i]);
                Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + id);
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
            Log.WriteIfVerbose("\tMarquee start=" + start + ",  End=" + end + ",  Duration= " + marqueeFrames + "frames =" + marqueeDuration.ToString("F2")+"s");
            int[] frameIndices = null;

			switch (TemplateState.FeatureVectorExtraction)
			{
				case FV_Extraction.AT_FIXED_INTERVALS:
					int interval = (int)(this.TemplateState.FeatureVectorExtractionInterval / this.TemplateState.FrameDuration /(double)1000);
					Log.WriteIfVerbose("\tFrame interval="+interval+"ms");
					frameIndices = FeatureVector.GetFrameIndices(start, end, interval);
					break;
				case FV_Extraction.AT_ENERGY_PEAKS:
					double[] frameEnergy = this.Sonogram.Decibels;
					double energyThreshold = this.TemplateState.SegmentationThreshold_k1;
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
            int dT = this.TemplateState.DeltaT;
            double[,] M = this.Sonogram.CepstralM;

            FeatureVector[] fvs = new FeatureVector[indicesL];
            for (int i = 0; i < indicesL; i++)
            {
                Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + frameIndices[i]);
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, frameIndices[i], dT); //combines  frames T-dT, T and T+dT
                fvs[i] = new FeatureVector(acousticV, i + 1); //avoid FV id = 0. Reserve this for noise vector
                fvs[i].SourceFile = this.TemplateState.SourceFName; //assume all FVs have same source file
                fvs[i].SetFrameIndex(frameIndices[i]);
            }
            return fvs;
        }

        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //public void ConvertImageCoords2SonogramCoords(int freqBinCount, params int[] imageCoords)
        //{
        //    this.x1 = imageCoords[0];
        //    this.y1 = imageCoords[1];
        //    this.x2 = imageCoords[2];
        //    this.y2 = imageCoords[3];
        //    //convert image coordinates to sonogram coords
        //    //sonogram: rows=timesteps; sonogram cols=freq bins
        //    this.t1 = imageCoords[0]; //imageCoords[0]=x1
        //    this.t2 = imageCoords[2]; //imageCoords[2]=x2
        //    this.bin1 = freqBinCount - imageCoords[3]; //imageCoords[3]=y2
        //    this.bin2 = freqBinCount - imageCoords[1] - 1;//imageCoords[1]=y1

        //    this.templateSpectralCount=t2-t1+1; //number of spectra in template
        //    //double FrameOffset = duration of non-overlapped part of window/frame in seconds
        //    this.templateDuration = templateSpectralCount * this.TemplateState.FrameOffset; // timeBin; //duration of template
        //    int min = (int)(bin1 * this.TemplateState.FBinWidth);
        //    int max = (int)(bin2 * this.TemplateState.FBinWidth);
        //    this.TemplateState.MinTemplateFreq = min; 
        //    this.TemplateState.MaxTemplateFreq = max;
        //    this.TemplateState.MidTemplateFreq = min + ((max - min) / 2); //Hz
        //}

        ///// <summary>
        ///// Extracts a template (submatrix) from the passed sonogram but 
        ///// using coordinates that the user would have obtained from the BMP image
        ///// of the sonogram.
        ///// Must first convert the image coordinates to sonogram coordinates.
        ///// The image matrix is effectively rotated 90 degrees clockwise to
        ///// map to the sonogram.
        ///// </summary>
        ///// <param name="s"></param>
        ///// <param name="imageCoords"></param>
        ///// <returns></returns>
        //public void ExtractTemplateUsingImageCoordinates(Sonogram s, params int[] imageCoords)
        //{
        //    //convert image coordinates to sonogram coords
        //    //sonogram: rows=timesteps; sonogram cols=freq bins
        //    double[,] sMatrix = s.AmplitudM;
        //    int timeStepCount = sMatrix.GetLength(0);
        //    this.Sonogram.State.FreqBinCount = sMatrix.GetLength(1);
        //    //this.spectrumCount = timeStepCount;
        //    //this.hzBin = this.Sonogram.State.NyquistFreq / (double)this.Sonogram.State.FreqBinCount;

        //    this.Matrix = s.AmplitudM;
        //    ConvertImageCoords2SonogramCoords(this.Sonogram.State.FreqBinCount, imageCoords);
        //    this.Matrix = DataTools.Submatrix(sMatrix, this.t1, this.bin1, this.t2, this.bin2);
        //    DataTools.MinMax(this.Matrix, out this.minTemplatePower, out this.maxTemplatePower);
        //}//end ExtractTemplate

        //public void ExtractTemplateFromImage2File(Sonogram s, params int[] imageCoords)
        //{
        //    ExtractTemplateUsingImageCoordinates(s, imageCoords);
        //    FileTools.WriteMatrix2File_Formatted(this.matrix, this.matrixFName, "F5");
        //}
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************

        public void WriteTemplateIniFile()
        {
            //write the call data to a file
            ArrayList data = new ArrayList();
            data.Add("DATE=" + DateTime.Now.ToString("u"));
            data.Add("#");
            data.Add("#**************** TEMPLATE DATA");
            data.Add("TEMPLATE_ID=" + this.TemplateState.CallID);
            data.Add("CALL_NAME=" + this.TemplateState.CallName);
            data.Add("COMMENT=" + this.TemplateState.CallComment);
            data.Add("THIS_FILE=" + this.TemplateIniPath);

            data.Add("#");
            data.Add("#**************** INFO ABOUT ORIGINAL .WAV FILE");
            data.Add("WAV_FILE_PATH="  + this.TemplateState.SourceFPath);
            data.Add("WAV_FILE_NAME=" + this.TemplateState.SourceFStem);
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
            data.Add("DYNAMIC_RANGE=" + this.TemplateState.MaxDecibelReference); //decibels above noise level #### YET TO DO THIS PROPERLY
            data.Add("#Dynamic range = the dB difference between min av and max frame energy.");

            data.Add("#");
            data.Add("#**************** INFO ABOUT FEATURE EXTRACTION");
            data.Add("NYQUIST_FREQ=" + this.TemplateState.NyquistFreq);
            data.Add("WINDOW_FUNCTION=" + this.TemplateState.WindowFncName);
            data.Add("NUMBER_OF_FREQ_BINS=" + this.TemplateState.FreqBinCount);
            data.Add("FREQ_BIN_WIDTH=" + this.TemplateState.FBinWidth.ToString("F2"));
            data.Add("MIN_FREQ=" + this.TemplateState.MinTemplateFreq);
            data.Add("MAX_FREQ=" + this.TemplateState.MaxTemplateFreq);
            data.Add("MID_FREQ=" + this.TemplateState.MidTemplateFreq);
            data.Add("DO_MEL_CONVERSION=" + this.TemplateState.DoMelScale);
            data.Add("DO_NOISE_REDUCTION=" + this.TemplateState.DoNoiseReduction);
            data.Add("FILTERBANK_COUNT=" + this.TemplateState.FilterbankCount);
            data.Add("CC_COUNT=" + this.TemplateState.ccCount);

            data.Add("INCLUDE_DELTA=" + this.TemplateState.IncludeDelta);
            data.Add("INCLUDE_DOUBLEDELTA=" + this.TemplateState.IncludeDoubleDelta);
            data.Add("DELTA_T=" + this.TemplateState.DeltaT);
            data.Add("#");


            data.Add("#**************** FV EXTRACTION OPTIONS **************************");
            StringBuilder sb;
            data.Add("FV_SOURCE="+FV_Source.SELECTED_FRAMES.ToString());
            if (this.TemplateState.FeatureVectorSource == FV_Source.SELECTED_FRAMES)
            {
                sb = new StringBuilder("FV_SELECTED_FRAMES=");
                int L = this.TemplateState.FeatureVector_SelectedFrames.Length;
                for (int i = 0; i < L; i++) sb.Append(this.TemplateState.FeatureVector_SelectedFrames[i]+",");
                data.Add(sb.ToString());
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
            int fvCount = this.FeatureVectors.Length;
            data.Add("FEATURE_VECTOR_LENGTH=" + this.FeatureVectors[0].FvLength); //117
            data.Add("NUMBER_OF_FEATURE_VECTORS="+fvCount);
            for (int n = 0; n < fvCount; n++) 
            {
                FeatureVector fv = this.FeatureVectors[n];
                data.Add("FV" + (n + 1) + "_FILE="+fv.VectorFPath);
                data.Add("FV" + (n + 1) + "_SELECTED_FRAMES=" + fv.FrameIndices);
                data.Add("FV" + (n + 1) + "_SOURCE_FILE=" + fv.SourceFile);
            }
            data.Add("FV_DEFAULT_NOISE_FILE=" + this.TemplateState.FeatureVector_DefaultNoiseFile);
            data.Add("#");

            data.Add("#THRESHOLDS FOR THE ACOUSTIC MODEL");
            data.Add("#THRESHOLD OPTIONS: 3.1(p=0.001), 2.58(p=0.005), 2.33(p=0.01), 2.15(p=0.03), 1.98(p=0.05),");
            data.Add("ZSCORE_THRESHOLD=" + this.TemplateState.ZScoreThreshold);
            data.Add("#");

            data.Add("#**************** INFO ABOUT LANGUAGE MODEL");
            // MARKOV MODEL PARAMETERS
            Type type = typeof(HMMType);
            string[] names = Enum.GetNames(type);
            sb = new StringBuilder("#There are " + names.Length + " choices of HMM:");
            for (int n = 0; n < names.Length; n++) sb.Append(" (" + (n + 1) + ")" + names[n]);
            data.Add(sb.ToString());

            data.Add("MM_TYPE=" + this.TemplateState.HmmType);
            data.Add("MM_NAME=" + this.TemplateState.HmmName);
            data.Add("NUMBER_OF_WORDS=" + this.TemplateState.WordCount);
            data.Add("#WORD1_NAME=no_word");
            data.Add("#WORD1_EXAMPLE1=1111");
            data.Add("#WORD1_EXAMPLE2=11111");
            data.Add("#WORD2_NAME=no_word");
            data.Add("#WORD2_EXAMPLE1=22");
            data.Add("#WORD2_EXAMPLE2=222");

            //add additional parameters
            data.Add("#SONG_WINDOW=" + this.TemplateState.SongWindow.ToString("F1"));
            data.Add("#WORD_PERIODICITY_MS=???");
            data.Add("#");

            //maxSyllables=
            //double maxSyllableGap = 0.25; //seconds
            //double maxSong=

            //check if a file already exists and if so make a copy
			if (FileTools.FileExists(this.TemplateIniPath)) FileTools.CopyFile(this.TemplateIniPath);

            //write template to file
			FileTools.WriteTextFile(this.TemplateIniPath, data);
            //write template a second time as copy which will not be overwritten
            string secondPath = this.TemplateDir + Template.templateStemName + this.CallID + "_" + this.TemplateState.FileDescriptor +templateFExt;
            FileTools.WriteTextFile(secondPath, data);
        } // end of WriteTemplateConfigFile()

        /// <summary>
        /// reads the template configuration file and writes values into the state of configuration.
        /// These values over-write the default values read in the sono.ini file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int ReadTemplateFile(string path, SonoConfig state)
        {
            int status = 0;
            Configuration cfg = new Props(path);
            state.CallID = cfg.GetInt("TEMPLATE_ID");
            state.CallName = cfg.GetString("CALL_NAME");
            state.CallComment = cfg.GetString("COMMENT");
            //set up the template dir. Parent dir is read from the app.ini file
            TemplateDir = state.TemplateDir = Path.GetDirectoryName(path);
            state.SourceFName = cfg.GetString("WAV_FILE_NAME");
            state.SourceFPath = cfg.GetString("WAV_FILE_PATH");

            //the wav file
            state.SampleRate = cfg.GetInt("WAV_SAMPLE_RATE");
            state.TimeDuration = cfg.GetDouble("WAV_DURATION");

            //frame parameters
            state.WindowSize = cfg.GetInt("FRAME_SIZE");
            state.WindowOverlap = cfg.GetDouble("FRAME_OVERLAP"); //fractional overlap of frames
            state.FrameCount = cfg.GetInt("NUMBER_OF_FRAMES");
            state.FramesPerSecond = cfg.GetDouble("FRAMES_PER_SECOND");
            state.FrameDuration = cfg.GetDouble("FRAME_DURATION_MS") / (double)1000; //convert ms to seconds
            state.FrameOffset = cfg.GetDouble("FRAME_OFFSET_MS") / (double)1000; //convert ms to seconds

            //MFCC parameters
            state.NyquistFreq = cfg.GetInt("NYQUIST_FREQ");
            state.WindowFncName = cfg.GetString("WINDOW_FUNCTION");
            state.WindowFnc = FFT.GetWindowFunction(state.WindowFncName);
            state.FreqBinCount = cfg.GetInt("NUMBER_OF_FREQ_BINS");
            state.FBinWidth = cfg.GetDouble("FREQ_BIN_WIDTH");
            state.FreqBand_Min = cfg.GetInt("MIN_FREQ");
            state.FreqBand_Mid = cfg.GetInt("MID_FREQ");
            state.FreqBand_Max = cfg.GetInt("MAX_FREQ");
            if ((state.FreqBand_Min > state.MinFreq) || (state.FreqBand_Max < state.NyquistFreq)) state.doFreqBandAnalysis = true;
            state.FilterbankCount = cfg.GetInt("FILTERBANK_COUNT");
            state.DoMelScale = cfg.GetBoolean("DO_MEL_CONVERSION");
            state.DoNoiseReduction = cfg.GetBoolean("DO_NOISE_REDUCTION");

            state.ccCount = cfg.GetInt("CC_COUNT");
            state.IncludeDelta = cfg.GetBoolean("INCLUDE_DELTA");
            state.IncludeDoubleDelta = cfg.GetBoolean("INCLUDE_DOUBLEDELTA");
            state.DeltaT = cfg.GetInt("DELTA_T");

            //FEATURE VECTORS
            GetFVSource("FV_SOURCE", cfg, state);
            if (state.FeatureVectorSource != FV_Source.SELECTED_FRAMES) GetFVExtraction("FV_EXTRACTION", cfg, state);
            state.FeatureVector_DoAveraging = cfg.GetBoolean("FV_DO_AVERAGING");

            int fvCount = cfg.GetInt("NUMBER_OF_FEATURE_VECTORS");
            state.FeatureVectorCount  = fvCount;
            state.FeatureVectorLength = cfg.GetInt("FEATURE_VECTOR_LENGTH");
            state.FeatureVectorPaths = new string[fvCount];
            for (int n = 0; n < fvCount; n++) state.FeatureVectorPaths[n] = ResolvePath(cfg.GetString("FV" + (n+1) + "_FILE"));
            state.FeatureVector_SelectedFrames = new string[fvCount];
            for (int n = 0; n < fvCount; n++) state.FeatureVector_SelectedFrames[n] = cfg.GetString("FV" + (n + 1) + "_SELECTED_FRAMES");
            state.FVSourceFiles = new string[fvCount];
            for (int n = 0; n < fvCount; n++) state.FVSourceFiles[n] = cfg.GetString("FV" + (n + 1) + "_SOURCE_FILE");
			state.DefaultNoiseFVFile = ResolvePath(cfg.GetString("FV_DEFAULT_NOISE_FILE"));

            //ACOUSTIC MODEL
            state.ZscoreSmoothingWindow = 3;  // DEFAULT zscore SmoothingWindow
            state.ZScoreThreshold = 1.98;  // DEFAULT zscore threshold for p=0.05
            double value = cfg.GetDouble("ZSCORE_THRESHOLD");
			if (value == -Double.MaxValue) Log.WriteLine("WARNING!! ZSCORE_THRESHOLD NOT SET IN TEMPLATE INI FILE. USING DEFAULT VALUE=" + state.ZScoreThreshold);
            else state.ZScoreThreshold = value;

            // THE LANGUAGE MODEL
            MarkovModel mm = null;
            Type type = typeof(HMMType);
            string typeName = cfg.GetString("MM_TYPE");
            HMMType mmType = MarkovModel.GetHmmType(typeName);
            state.HmmType = mmType;
            if (mmType == HMMType.UNDEFINED)  //a INVALID markov model is defined in 
            {
				Log.WriteLine("##WARNING! Template.Read():- <" + typeName + "> IS AN UNDEFINED HMM TYPE");
                return status;
            }

            string mmName = cfg.GetString("MM_NAME");
            int numberOfStates = fvCount + 2; //because need extra for noise and for garbage


            // READ TRAINING SEQUENCES
            int wordCount = cfg.GetInt("NUMBER_OF_WORDS");
            if (wordCount < 1)
            {
				Log.WriteLine("  TRAINING DATA REQUIRED. NO WORDS DEFINED IN TEMPLATE INI FILE.");
                return status;
            }
            state.WordCount = wordCount;
			//Log.WriteLine("NUMBER OF WORDS=" + state.WordCount);
            TrainingSequences ts = new TrainingSequences();
            for (int n = 0; n < wordCount; n++)
            {
                string name = cfg.GetString("WORD" + (n + 1) + "_NAME");
                for (int w = 0; w < 100; w++) //do not allow more than 100 examples
                {
                    string word = cfg.GetString("WORD" + (n + 1) + "_EXAMPLE" + (w + 1));
                    if (word == null) break;
                    ts.AddSequence(name, word);
                }

            }//end for loop over all words
            state.Words = ts.GetSequences();
            //ts.WriteComposition();


            //PERIODICITY INFO
            if (mmType == HMMType.OLD_PERIODIC)
            {
                int period_ms = cfg.GetInt("PERIODICITY_MS");
                if (period_ms == -Int32.MaxValue)
                {
					Log.WriteLine("  PERIODICITY WILL NOT BE ANALYSED. NO ENTRY IN TEMPLATE INI FILE.");
                    return status;
                }
                mm = new MarkovModel(mmName, mmType, period_ms, state.FrameOffset); //special constructor for two state periodic MM 
                mm.TrainModel(ts);
            }
            else
            if (mmType == HMMType.TWO_STATE_PERIODIC)
            {
                int gap_ms = cfg.GetInt("GAP_MS");
                if (gap_ms == -Int32.MaxValue)
                {
					Log.WriteLine("  TWO_STATE MARKOV MODEL CANNOT BE DEFINED BECUASE GAP DURATION IS NOT DEFINED IN TEMPLATE INI FILE.");
                    return status;
                }
                mm = new MarkovModel(mmName, mmType, gap_ms, state.FrameOffset); //special constructor for two state periodic MM 
                mm.TrainModel(ts);
            }
            else
            {
                mm = new MarkovModel(mmName, mmType, numberOfStates);
                mm.DeltaT = state.FrameOffset; //the sequence time step
                mm.TrainModel(ts);
            }
            state.WordModel = mm; //one markov model per template
            //end setting up markov model

            state.SongWindow = cfg.GetDouble("SONG_WINDOW");
            if (state.SongWindow == -Double.MaxValue) state.SongWindow = 1.0; //the DEFAULT VALUE in seconds


            return status;
        }

		private string ResolvePath(string path)
		{
			if (!Path.IsPathRooted(path))
				return Path.Combine(TemplateDir, path);
			return path;
		}

        public void GetFVSource(string key, Configuration cfg, SonoConfig state)
        {
            bool keyExists = cfg.ContainsKey(key);
            if (!keyExists)
			{
				Log.WriteLine("Template.GetFVSource():- WARNING! NO SOURCE FOR FEATURE VECTORS IS DEFINED!");
				Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
                state.FeatureVectorSource = FV_Source.SELECTED_FRAMES;
            return;
            }
            string value = cfg.GetString(key);

            if (value.StartsWith("MARQUEE"))
            {
                state.FeatureVectorSource = FV_Source.MARQUEE;
                state.MarqueeStart = cfg.GetInt("MARQUEE_START");
                state.MarqueeEnd   = cfg.GetInt("MARQUEE_END");
            }
            else
                if (value.StartsWith("SELECTED_FRAMES")) state.FeatureVectorSource = FV_Source.SELECTED_FRAMES;
                else
                {
					Log.WriteLine("Template.GetFVSource():- WARNING! INVALID SOURCE FOR FEATURE VECTORS IS DEFINED! " + value);
					Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
                    state.FeatureVectorSource = FV_Source.SELECTED_FRAMES;
                    return;
                }
            
            //now read other parameters relevant to the Feature Vector source
            //TODO ###########################################################################
        }//end GetFVSource

        public void GetFVExtraction(string key, Configuration cfg, SonoConfig state)
        {
            bool keyExists = cfg.ContainsKey(key);
            if (!keyExists)
            {
				Log.WriteLine("Template.GetFVExtraction():- WARNING! NO EXTRACTION PROCESS IS DEFINED FOR FEATURE VECTORS!");
				Log.WriteLine("                             SET THE DEFAULT:- FV_Extraction = AT_ENERGY_PEAKS");
                state.FeatureVectorExtraction = FV_Extraction.AT_ENERGY_PEAKS;
                return;
            }
            string value = cfg.GetString(key);

            if (value.StartsWith("AT_ENERGY_PEAKS")) state.FeatureVectorExtraction = FV_Extraction.AT_ENERGY_PEAKS;
            else
                if (value.StartsWith("AT_FIXED_INTERVALS_OF_"))
                {
                    state.FeatureVectorExtraction = FV_Extraction.AT_FIXED_INTERVALS;
                    string[] words = value.Split('_');
                    int int32;
                    try
                    {
                        int32 = Int32.Parse(words[3]);
                    }
                    catch (System.FormatException ex)
                    {
						Log.WriteLine("Template.GetFVExtraction():- WARNING! INVALID INTEGER:- " + words[3]);
						Log.WriteLine(ex);
                        int32 = 0;
                    }
                    state.FeatureVectorExtractionInterval = int32;
                }
                else
                {
					Log.WriteLine("Template.GetFVExtraction():- WARNING! INVALID EXTRACTION VALUE IS DEFINED FOR FEATURE VECTORS! " + value);
					Log.WriteLine("                             SET THE DEFAULT:- FV_Extraction = AT_ENERGY_PEAKS");
                    state.FeatureVectorExtraction = FV_Extraction.AT_ENERGY_PEAKS;
                    return;
                }
            //now read other parameters relevant to the Feature Vector Extraction
            //TODO ###########################################################################
        }//end GetFVExtraction

        public void WriteInfo2STDOUT()
        {
            Console.WriteLine("\nTEMPLATE INFO");
            Console.WriteLine(" Template ID: " + this.CallID);
            Console.WriteLine(" Template name: " + this.TemplateState.CallName);
            Console.WriteLine(" Comment: " + this.TemplateState.CallComment);
            Console.WriteLine(" Template dir     : " + this.TemplateState.TemplateParentDir);
            Console.WriteLine(" Template ini file: " + this.TemplateIniPath);
            Console.WriteLine(" Bottom freq=" + this.TemplateState.MinTemplateFreq + "  Mid freq=" + this.TemplateState.MidTemplateFreq + " Top freq=" + this.TemplateState.MaxTemplateFreq);

            Console.WriteLine(" NUMBER_OF_FEATURE_VECTORS="+this.TemplateState.FeatureVectorCount);
            Console.WriteLine(" FEATURE_VECTOR_LENGTH=" + this.TemplateState.FeatureVectorLength);
            Console.WriteLine(" Feature Vector Source Method = "+ this.TemplateState.FeatureVectorSource);
            if (this.TemplateState.FeatureVectorSource != FV_Source.SELECTED_FRAMES)
                Console.WriteLine("     Feature Vector Extraction Method = " + this.TemplateState.FeatureVectorExtraction);
            Console.WriteLine(" NUMBER_OF_WORDS=" + this.TemplateState.WordCount);
            Console.WriteLine(" HMM type = " + this.TemplateState.HmmType);
            Console.WriteLine(" HMM name = " + this.TemplateState.HmmName);
        }
    }//end Class Template
}
