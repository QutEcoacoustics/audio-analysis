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
        private const string templateStemName = "template";
        private const string templateFExt = ".ini";
        private const string fvectorFExt = ".txt";
        

        public int CallID { get; set; }
        public bool DoMelConversion { get; set; }
        private SonoConfig templateState;
        public  SonoConfig TemplateState { get { return templateState; } set { templateState = value; } }
        private Sonogram   sonogram;
        public  Sonogram   Sonogram { get { return sonogram; } set { sonogram = value; } }

        
        //file names and directory
        private string templateDir;
        public  string TemplateDir  { get { return templateDir; }  set { templateDir = value; } }
        private string templatePath;
        public  string TemplatePath { get { return templatePath; } set { templatePath = value; } }

        //the FEATURE VECTORS
        private FeatureVector[] featureVectors;
        public  FeatureVector[] FeatureVectors { get { return featureVectors; } set { featureVectors = value; } }


        //info about OLD TEMPLATE EXTRACTION
        int x1; int y1; //image coordinates for top left of selection
        int x2; int y2; //image coordinates for bottom right of selection
        int t1; int t2; //sonogram time interval selection
        int bin1; int bin2; //sonogram freq bin interval 
        //double templateDuration; //duration of template in seconds
        //int templateSpectralCount; //number of spectra in template        


        //info about TEMPLATE SCORING
        public double NoiseAv { get { return this.templateState.NoiseAv; } }
        public double NoiseSD { get { return this.templateState.NoiseSd; } }

        private bool verbose = true;




        /// <summary>
        /// CONSTRUCTOR 1
        /// Use this constructor to read an existing template file
        /// </summary>
        /// <param name="callID"></param>
        public Template(string iniFPath, int id)
        {
            if (this.verbose) Console.WriteLine("\n#####  READING APPLICATION INI FILE :=" + iniFPath);
            this.templateState = new SonoConfig();
            this.templateState.ReadConfig(iniFPath);//read the ini file for default parameters
            if (this.templateState.Verbosity > 0) this.verbose = true;
            else                                  this.verbose = false;
            
            this.templateDir = this.templateState.TemplateDir;
            this.CallID = id;

            string templatePath = this.templateDir+templateStemName + "_"+id+templateFExt;
            if (this.verbose) Console.WriteLine("\n#####  READING TEMPLATE INI FILE :=" + templatePath);
            int status = ReadTemplateFile(templatePath, this.templateState);//read the template configuration file


            //INITIALIZE FEATURE VECTORS
            FeatureVector.Verbose = this.verbose;
            int fvCount = this.templateState.FeatureVectorCount + 1; //+1 so can put noise feature vector in position zero
            this.featureVectors = new FeatureVector[fvCount]; 
            for (int n = 1; n < fvCount; n++) //later will put noise FV in position zero
            {
                string path = this.templateState.FeatureVectorPaths[n];
                int fvLength = this.templateState.FeatureVectorLength;
                this.featureVectors[n] = new FeatureVector(path, fvLength, n);
                this.featureVectors[n].FrameIndices = FeatureVector.ConvertFrameIndices(this.templateState.FeatureVector_SelectedFrames[n]);
                this.featureVectors[n].SourceFile = this.templateState.FVSourceFiles[n];
            }
        }


        /// <summary>
        /// CONSTRUCTOR 2
        /// Use this constructor to create a new template
        /// </summary>
        /// <param name="iniFPath"></param>
        /// <param name="callID"></param>
        /// <param name="callName"></param>
        /// <param name="callComment"></param>
        /// <param name="sourceFileStem"></param>
        public Template(string iniFPath, int callID, string callName, string callComment, string sourceFileStem)
        {
            if (this.verbose) Console.WriteLine("\n#####  READING APPLICATION INI FILE :=" + iniFPath);
            this.templateState = new SonoConfig();
            this.templateState.ReadConfig(iniFPath);//read the ini file for default parameters
            if (this.templateState.Verbosity > 0) this.verbose = true;
            else                                  this.verbose = false;

            this.CallID = callID;
            this.templateState.CallID = callID;
            this.templateState.CallName = callName;
            this.templateState.CallComment = callComment;
            this.templateState.SourceFStem = sourceFileStem;
            this.templateState.SourceFName = sourceFileStem + this.templateState.WavFileExt;
            this.templateState.SourceFPath = this.templateState.WavFileDir + "\\" + this.templateState.SourceFName;
            this.templatePath = this.templateState.TemplateDir + Template.templateStemName + "_" + callID + templateFExt;
            if (this.verbose) Console.WriteLine("templatePath=" + templatePath);
        }


        public void SetSonogram(string wavPath)
        {
            FileInfo fi = new FileInfo(wavPath);
            this.templateState.WavFileDir = fi.DirectoryName;
            this.templateState.WavFName   = fi.Name.Substring(0, fi.Name.Length - 4);//remove the file extention
            this.templateState.WavFileExt = fi.Extension;
            if (this.templateState.WavFName != null)
            {
                this.templateState.SetDateAndTime(this.templateState.WavFName);
            }

            //read the .WAV file
            WavReader wav = new WavReader(wavPath);
            //check the sampling rate
            int sr = wav.SampleRate;
            if (sr != this.templateState.SampleRate)
                throw new Exception("Template.SetSonogram(string wavPath):- Sampling rate of wav file not equal to that of template:  wavFile(" + sr + ") != template(" + this.templateState.SampleRate + ")");
            if (this.verbose) Console.WriteLine("Template.SetSonogram(string wavPath):- Sampling rates of wav file and template are equal: " + sr + " = " + this.templateState.SampleRate);

            //initialise Sonogram which also makes the sonogram
            this.sonogram = new Sonogram(this.templateState, wav);
        }

        public void SetExtractionParameters(FV_Source fvSource, FV_Extraction fvExtraction, bool doFvAveraging)
        {
            this.templateState.FeatureVectorSource = fvSource;
            this.templateState.FeatureVectorExtraction = fvExtraction;
            this.templateState.FeatureVector_DoAveraging = doFvAveraging;
        }


        public void SetFrequencyBounds(int minFreq, int maxFreq)
        {
            this.templateState.FreqBand_Min    = minFreq;
            this.templateState.FreqBand_Max    = maxFreq;
            this.templateState.MinTemplateFreq = minFreq;
            this.templateState.MaxTemplateFreq = maxFreq;
            this.templateState.MidTemplateFreq = minFreq + ((maxFreq - minFreq) / 2); //Hz
        }

        public void SetMarqueeBounds(int minFreq, int maxFreq, int marqueeStart, int marqueeEnd)
        {
            SetFrequencyBounds(minFreq, maxFreq);
            this.templateState.MarqueeStart = marqueeStart;
            this.templateState.MarqueeEnd   = marqueeEnd;
        }


        public void SetLanguageModel(string[] words, bool highSensitivitySearch)
        {
            this.templateState.Words = words;
            this.templateState.WordCount =  words.Length;
            //USE_HIGH_SENSITIVITY_SEARCH will increase false positives i.e. reduce specificity
            this.templateState.HighSensitivitySearch = highSensitivitySearch; 
        }

        public void SetScoringParameters(ScoringProtocol sp, int zw, double zThreshold, int period)
        {
            this.templateState.ScoringProtocol = sp;//three options are HOTSPOTS, WORDMATCH, PERIODICITY
            this.templateState.ZscoreSmoothingWindow = 3;
            this.templateState.ZScoreThreshold = zThreshold; //options are 1.98, 2.33, 2.56, 3.1
            this.templateState.CallPeriodicity_ms = period;
        }



        /// <summary>
        /// NOTE: All these parameters are set for each template. Their values override the values set in the ini file.
        /// </summary>
        /// <param name="frameSize"></param>
        /// <param name="frameOverlap"></param>
        /// <param name="dynamicRange"></param>
        /// <param name="filterBankCount"></param>
        /// <param name="doMelConversion"></param>
        /// <param name="ceptralCoeffCount"></param>
        /// <param name="deltaT"></param>
        /// <param name="includeDeltaFeatures"></param>
        /// <param name="includeDoubleDeltaFeatures"></param>
        public void SetMfccParameters(int frameSize, double frameOverlap, double dynamicRange, int filterBankCount, bool doMelConversion, 
                                int ceptralCoeffCount, int deltaT, bool includeDeltaFeatures, bool includeDoubleDeltaFeatures)
        {
            this.templateState.WindowSize      = frameSize;
            this.templateState.WindowOverlap   = frameOverlap;
            this.templateState.SonogramType = SonogramType.acousticVectors; //to MAKE MATRIX OF dim 3x39 ACOUSTIC VECTORS
            this.DoMelConversion = doMelConversion;
            this.templateState.DeltaT = deltaT;
            this.templateState.IncludeDelta = includeDeltaFeatures;
            this.templateState.IncludeDoubleDelta = includeDoubleDeltaFeatures;
        }

        public void SetExtractionInterval(int fvExtractionInterval)
        {
            this.templateState.FeatureVectorExtractionInterval = fvExtractionInterval;
        }

        public void SetSelectedFrames(string selectedFrames)
        {
            this.templateState.FeatureVector_SelectedFrames = new string[1];
            this.templateState.FeatureVector_SelectedFrames[0] = selectedFrames;
        }

        public void ExtractTemplateFromSonogram()
        {
            Console.WriteLine("\nEXTRACTING TEMPLATE USING SUPPLIED PARAMETERS");
            FeatureVector.Verbose = this.verbose;
             
            if (this.templateState.FeatureVectorSource == FV_Source.SELECTED_FRAMES)
            {
                this.featureVectors = GetFeatureVectorsFromFrames();
            }else
            if (this.templateState.FeatureVectorSource == FV_Source.MARQUEE)
            {
                if (this.templateState.FeatureVectorExtraction == FV_Extraction.AT_ENERGY_PEAKS)
                {
                    this.featureVectors = GetFeatureVectorsFromMarquee();
                }else
                if (this.templateState.FeatureVectorExtraction == FV_Extraction.AT_FIXED_INTERVALS)
                {
                    this.featureVectors = GetFeatureVectorsFromMarquee();
                }else
                {
                    Console.WriteLine("Template.ExtractTemplateFromSonogram(: WARNING!! INVALID FV EXTRACTION OPTION!)");
                }
            }else
            {
                Console.WriteLine("Template.ExtractTemplateFromSonogram(: WARNING!! INVALID FV SOURCE OPTION!)");
            }


            if (this.verbose) Console.WriteLine("SAVING TEMPLATE FILES");
            //accumulate the acoustic vectors from multiple frames into an averaged feature vector
            FeatureVector avFV = null;
            if (this.templateState.FeatureVector_DoAveraging)
            {
                if (this.verbose) Console.WriteLine("   Do Template Averaging");
                int id = 1;
                avFV = FeatureVector.AverageFeatureVectors(featureVectors, id);
                //write all files
                string path = this.templateState.TemplateDir + templateStemName + "_" + this.CallID + "_FV1"+ fvectorFExt;
                if (avFV != null) avFV.SaveDataAndImageToFile(path, this.templateState);
                WriteTemplateIniFile();
            }
            else
            {
                for (int i = 0; i < featureVectors.Length; i++)
                {
                    string path = this.templateState.TemplateDir + templateStemName + "_" + this.CallID + "_FV" + i + fvectorFExt;
                    featureVectors[i].SaveDataAndImageToFile(path, this.templateState);
                }
                WriteTemplateIniFile();
            }

        } // end ExtractTemplateFromSonogram()   



        public FeatureVector[] GetFeatureVectorsFromFrames()
        {
            //init Sonogram. SonogramType already set to make matrix of acoustic vectors
            this.sonogram = new Sonogram(this.templateState, this.templateState.SourceFPath);
            double[,] M = this.sonogram.CepstralM;

            if (this.verbose) Console.WriteLine("\nEXTRACTING FEATURE VECTORS FROM FRAMES:- method Template.GetFeatureVectorsFromFrames()");
            //Get frame indices. Assume, when extracting a FeatureVector, that there is only one supplied string of integers
            string istr = this.templateState.FeatureVector_SelectedFrames[0];
            int[] frameIndices = FeatureVector.ConvertFrameIndices(istr);

            //initialise feature vectors for template. Each frame provides one vector in three parts
            int coeffcount = M.GetLength(1);  //number of MFCC deltas etcs
            int featureCount = coeffcount * 3;
            int indicesL = frameIndices.Length;
            int dT = this.templateState.DeltaT;
            FeatureVector[] fvs = new FeatureVector[indicesL];
            for (int i = 0; i < indicesL; i++)
            {
                if (this.verbose) Console.WriteLine("   Init FeatureVector[" + i + "] from frame " + frameIndices[i]);
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, frameIndices[i], dT); //combines  frames T-dT, T and T+dT
                fvs[i] = new FeatureVector(acousticV, i);
                fvs[i].SourceFile = this.templateState.SourceFName; //assume all FVs have same source file
                fvs[i].SetFrameIndices(this.templateState.FeatureVector_SelectedFrames[0]);
            }
            return fvs;
        }





        public FeatureVector[] GetFeatureVectorsFromMarquee()
        {
            //init Sonogram. SonogramType already set to make matrix of acoustic vectors
            this.sonogram = new Sonogram(this.templateState, this.templateState.SourceFPath);
            double[,] M = this.sonogram.CepstralM;

            int start = this.templateState.MarqueeStart;
            int end   = this.templateState.MarqueeEnd;
            int interval = this.TemplateState.FeatureVectorExtractionInterval;

            //initialise feature vectors for template. Each frame provides one vector in three parts
            int coeffcount = M.GetLength(1);  //number of MFCC deltas etcs
            int featureCount = coeffcount * 3;

            int[] frameIndices = FeatureVector.GetFrameIndices(start, end, interval);
            DataTools.writeArray(frameIndices);
            int indicesL = frameIndices.Length;
            int dT = this.templateState.DeltaT;
            FeatureVector[] fvs = new FeatureVector[indicesL];
            for (int i = 0; i < indicesL; i++)
            {
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, frameIndices[i], dT); //combines  frames T-dT, T and T+dT
                this.featureVectors[i] = new FeatureVector(acousticV, i);
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
        //    this.templateDuration = templateSpectralCount * this.templateState.FrameOffset; // timeBin; //duration of template
        //    int min = (int)(bin1 * this.templateState.FBinWidth);
        //    int max = (int)(bin2 * this.templateState.FBinWidth);
        //    this.templateState.MinTemplateFreq = min; 
        //    this.templateState.MaxTemplateFreq = max;
        //    this.templateState.MidTemplateFreq = min + ((max - min) / 2); //Hz
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
        //    this.sonogram.State.FreqBinCount = sMatrix.GetLength(1);
        //    //this.spectrumCount = timeStepCount;
        //    //this.hzBin = this.sonogram.State.NyquistFreq / (double)this.sonogram.State.FreqBinCount;

        //    this.Matrix = s.AmplitudM;
        //    ConvertImageCoords2SonogramCoords(this.sonogram.State.FreqBinCount, imageCoords);
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
            data.Add("#TEMPLATE DATA");
            data.Add("TEMPLATE_ID=" + this.templateState.CallID);
            data.Add("CALL_NAME=" + this.templateState.CallName);
            data.Add("COMMENT=" + this.templateState.CallComment);
            data.Add("THIS_FILE=" + this.templatePath);

            data.Add("#");
            data.Add("#INFO ABOUT ORIGINAL .WAV FILE");
            data.Add("WAV_FILE_NAME=" + this.templateState.SourceFName);
            data.Add("WAV_SAMPLE_RATE=" + this.templateState.SampleRate);
            data.Add("WAV_DURATION=" + this.templateState.TimeDuration.ToString("F3"));

            data.Add("#");
            data.Add("#INFO ABOUT FRAMES");
            data.Add("FRAME_SIZE=" + this.templateState.WindowSize);
            data.Add("FRAME_OVERLAP=" + this.templateState.WindowOverlap);
            data.Add("FRAME_DURATION_MS=" + (this.templateState.FrameDuration * 1000).ToString("F3"));//convert to milliseconds
            data.Add("FRAME_OFFSET_MS=" + (this.templateState.FrameOffset * 1000).ToString("F3"));//convert to milliseconds
            data.Add("NUMBER_OF_FRAMES=" + this.templateState.FrameCount);
            data.Add("FRAMES_PER_SECOND=" + this.templateState.FramesPerSecond.ToString("F3"));

            data.Add("#");
            data.Add("#INFO ABOUT FEATURE EXTRACTION");
            data.Add("NYQUIST_FREQ=" + this.templateState.NyquistFreq);
            data.Add("WINDOW_FUNCTION=" + this.templateState.WindowFncName);
            data.Add("NUMBER_OF_FREQ_BINS=" + this.templateState.FreqBinCount);
            data.Add("FREQ_BIN_WIDTH=" + this.templateState.FBinWidth.ToString("F2"));
            data.Add("MIN_FREQ=" + this.templateState.MinTemplateFreq);
            data.Add("MAX_FREQ=" + this.templateState.MaxTemplateFreq);
            data.Add("MID_FREQ=" + this.templateState.MidTemplateFreq);
            data.Add("DO_MEL_CONVERSION=" + this.DoMelConversion);
            data.Add("DELTA_T=" + this.templateState.DeltaT);
            data.Add("INCLUDE_DELTA=" + this.templateState.IncludeDelta);
            data.Add("INCLUDE_DOUBLEDELTA=" + this.templateState.IncludeDoubleDelta);
            data.Add("FILTERBANK_COUNT=" + this.templateState.FilterbankCount);
            data.Add("CC_COUNT=" + this.templateState.ccCount);
            data.Add("DYNAMIC_RANGE=" + this.templateState.MaxDecibelReference); //decibels above noise level #### YET TO DO THIS PROPERLY
            data.Add("#");


            data.Add("#**************** FV EXTRACTION OPTIONS **************************");
            data.Add("FV_SOURCE="+FV_Source.SELECTED_FRAMES.ToString());
            if (this.templateState.FeatureVectorSource == FV_Source.SELECTED_FRAMES)
            {
                data.Add("FV_SELECTED_FRAMES=" + this.templateState.FeatureVector_SelectedFrames[0]);
            } else
            if (this.templateState.FeatureVectorSource == FV_Source.MARQUEE)
            {
                data.Add("FV_SELECTED_FRAMES=" + this.templateState.FeatureVector_SelectedFrames[0]);
                data.Add("MARQUEE_START="+this.templateState.MarqueeStart);
                data.Add("MARQUEE_END=" + this.templateState.MarqueeEnd);
                if (this.templateState.FeatureVectorExtraction == FV_Extraction.AT_ENERGY_PEAKS)
                {
                    data.Add("FV_EXTRACTION=AT_ENERGY_PEAKS");
                } else
                if (this.templateState.FeatureVectorExtraction == FV_Extraction.AT_FIXED_INTERVALS)
                {
                    data.Add("FV_EXTRACTION=AT_FIXED_INTERVALS");
                    data.Add("FV_EXTRACTION=AT_INTERVALS_OF_" + this.templateState.FeatureVectorExtractionInterval + "_MS");
                } 
            }
            data.Add("FV_DO_AVERAGING="+this.templateState.FeatureVector_DoAveraging);
            data.Add("#");


            data.Add("#**************** INFO ABOUT FEATURE VECTORS - THE ACOUSTIC MODEL ***************");
            int fvCount = this.featureVectors.Length;
            data.Add("FEATURE_VECTOR_LENGTH=" + this.featureVectors[0].FvLength); //117
            data.Add("NUMBER_OF_FEATURE_VECTORS="+fvCount);
            for (int n = 0; n < fvCount; n++) 
            {
                data.Add("FV1_FILE="+this.templateState.FeatureVectorPaths[n]);
                //data.Add("FV1_SELECTED_FRAMES=" + frameIDs.ToString());
                //FV1_FILE=C:\SensorNetworks\Templates\template_2_FV1.txt
                //FV1_SELECTED_FRAMES=1784,1828,1848,2113,2132,2152,
                //FV1_SOURCE_FILE=C:\SensorNetworks\WavFiles\BAC2_20071008-085040.wav
            }
            data.Add(@"FV_DEFAULT_NOISE_FILE=C:\SensorNetworks\Templates\template_2_DefaultNoise.txt");
            data.Add("#");
            data.Add("#");

            data.Add("#************* THRESHOLDS FOR THE ACOUSTIC MODELS ***************");
            //ZSCORE_SMOOTHING_WINDOW=3
            //##zscore threshold for p=0.001
            //#ZSCORE_THRESHOLD=3.1
            //##zscore threshold for p=0.005
            //#ZSCORE_THRESHOLD=2.58
            //##zscore threshold for p=0.01
            //#ZSCORE_THRESHOLD=2.33
            //##zscore threshold for p=0.03
            //#ZSCORE_THRESHOLD=2.15
            //##zscore threshold for p=0.05
            //ZSCORE_THRESHOLD=1.98
            //#ZSCORE_THRESHOLD=1.5

            data.Add("#");
            data.Add("#");


            data.Add("#INFO ABOUT LANGUAGE MODEL");
            //#
            //#*****************LANGUAGE MODEL
            //NUMBER_OF_WORDS=3
            //WORD1=111
            //WORD2=11
            //WORD3=1
            //#if select high sensitivity search, then specificity reduced i.e. produces a greater number of false positives 
            //USE_HIGH_SENSITIVITY_SEARCH=true
            //#
            data.Add("#**************** SCORING PROTOCOL");
            //#Three choices (1)hot spots (2)sequence (3)periodicity
            //#SCORING_PROTOCOL=HOTSPOTS
            //#SCORING_PROTOCOL=WORDMATCH
            //SCORING_PROTOCOL=PERIODICITY
            //CALL_PERIODICITY_MS=208
            data.Add("#");
            data.Add("#");

            //maxSyllables=
            //double maxSyllableGap = 0.25; //seconds
            //double maxSong=

            //write template to file
            FileTools.WriteTextFile(this.templatePath, data);
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
            state.DoMelScale = cfg.GetBoolean("DO_MEL_CONVERSION");
            state.ccCount = cfg.GetInt("CC_COUNT");
            state.IncludeDelta = cfg.GetBoolean("INCLUDE_DELTA");
            state.IncludeDoubleDelta = cfg.GetBoolean("INCLUDE_DOUBLEDELTA");
            state.DeltaT = cfg.GetInt("DELTA_T");

            //FEATURE VECTORS
            GetFVSource("FV_SOURCE", cfg, state);
            GetFVExtraction("FV_EXTRACTION", cfg, state);
            state.FeatureVector_DoAveraging = cfg.GetBoolean("FV_DO_AVERAGING");

            int fvCount = cfg.GetInt("NUMBER_OF_FEATURE_VECTORS");
            state.FeatureVectorCount  = fvCount;
            state.FeatureVectorLength = cfg.GetInt("FEATURE_VECTOR_LENGTH");
            fvCount += 1; //to accomodate noise FV in position 0
            state.FeatureVectorPaths = new string[fvCount];
            state.FeatureVectorPaths[0] = cfg.GetString("FV_DEFAULT_NOISE_FILE");
            for (int n = 1; n < fvCount; n++) state.FeatureVectorPaths[n] = cfg.GetString("FV" + n + "_FILE");
            state.FeatureVector_SelectedFrames = new string[fvCount];
            for (int n = 1; n < fvCount; n++) state.FeatureVector_SelectedFrames[n] = cfg.GetString("FV" + n + "_SELECTED_FRAMES");
            state.FVSourceFiles = new string[fvCount];
            for (int n = 1; n < fvCount; n++) state.FVSourceFiles[n] = cfg.GetString("FV" + n + "_SOURCE_FILE");



            //classifier parameters
            state.ZscoreSmoothingWindow = 3;  // DEFAULT zscore SmoothingWindow
            int i = cfg.GetInt("ZSCORE_SMOOTHING_WINDOW");
            if (i == -Int32.MaxValue) Console.WriteLine("WARNING!! ZSCORE_SMOOTHING_WINDOW NOT SET IN TEMPLATE INI FILE. USING DEFAULT VALUE");
            else state.ZscoreSmoothingWindow = i; 

            state.ZScoreThreshold = 1.98;  // DEFAULT zscore threshold for p=0.05
            double value = cfg.GetDouble("ZSCORE_THRESHOLD");
            if (value == -Double.MaxValue) Console.WriteLine("WARNING!! ZSCORE_THRESHOLD NOT SET IN TEMPLATE INI FILE. USING DEFAULT VALUE");
            else state.ZScoreThreshold = value;

            //the Language Model
            state.HighSensitivitySearch = cfg.GetBoolean("USE_HIGH_SENSITIVITY_SEARCH");
            int wordCount = cfg.GetInt("NUMBER_OF_WORDS");
            state.WordCount  = wordCount;
            state.Words = new string[wordCount];
            for (int n = 0; n < wordCount; n++) state.Words[n] = cfg.GetString("WORD" + (n+1));
            //Console.WriteLine("NUMBER OF WORDS=" + state.WordCount);
            //for (int n = 0; n < wordCount; n++) Console.WriteLine("WORD"+n+"="+state.Words[n]);

            // SCORING PROTOCOL
            state.ScoringProtocol = ScoringProtocol.WORDMATCH;  //the default
            string protocol = cfg.GetString("SCORING_PROTOCOL");
            if (protocol.StartsWith("HOTSPOTS"))         state.ScoringProtocol = ScoringProtocol.HOTSPOTS;
            else if (protocol.StartsWith("PERIODICITY")) state.ScoringProtocol = ScoringProtocol.PERIODICITY;
            state.CallPeriodicity_ms = 0;
            int period_ms = cfg.GetInt("CALL_PERIODICITY_MS");
            if (period_ms == -Int32.MaxValue) Console.WriteLine("CALL_PERIODICITY WILL NOT BE ANALYSED. NO ENTRY IN TEMPLATE INI FILE.");
            else state.CallPeriodicity_ms = period_ms;

            int period_frame = (int)Math.Round(period_ms / state.FrameOffset / (double)1000);
            state.CallPeriodicity_frames = period_frame;
            state.CallPeriodicity_NH_frames = (int)Math.Floor(period_frame / (double)7); //arbitrary NH
            state.CallPeriodicity_NH_ms     = (int)Math.Floor(period_ms    / (double)7); //arbitrary NH
            //Console.WriteLine("period_ms=" + period_ms + "  period_frame=" + period_frame + "+/-" + state.CallPeriodicity_NH);

            return status;
        } //end of ReadTemplateFile()

        public void GetFVSource(string key, Configuration cfg, SonoConfig state)
        {
            bool keyExists = cfg.ContainsKey(key);
            if (!keyExists) 
            {   Console.WriteLine("Template.GetFVSource():- WARNING! NO SOURCE FOR FEATURE VECTORS IS DEFINED!");
                Console.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
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
                    Console.WriteLine("Template.GetFVSource():- WARNING! INVALID SOURCE FOR FEATURE VECTORS IS DEFINED! " + value);
                    Console.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
                    state.FeatureVectorSource = FV_Source.SELECTED_FRAMES;
                    return;
                }
            
            //now read other parameters relevant to the Feature Vector source
            //TODO ###########################################################################
        }//end GetFVSource



        //    public enum FV_Extraction { AT_ENERGY_PEAKS, AT_INTERVALS_OF_}
        //#FV_EXTRACTION=AT_ENERGY_PEAKS
        //#FV_EXTRACTION=AT_INTERVALS_OF_200_MS

        public void GetFVExtraction(string key, Configuration cfg, SonoConfig state)
        {
            bool keyExists = cfg.ContainsKey(key);
            if (!keyExists)
            {
                Console.WriteLine("Template.GetFVExtraction():- WARNING! NO EXTRACTION PROCESS IS DEFINED FOR FEATURE VECTORS!");
                Console.WriteLine("                             SET THE DEFAULT:- FV_Extraction = AT_ENERGY_PEAKS");
                state.FeatureVectorExtraction = FV_Extraction.AT_ENERGY_PEAKS;
                return;
            }
            string value = cfg.GetString(key);

            if (value.StartsWith("AT_ENERGY_PEAKS")) state.FeatureVectorExtraction = FV_Extraction.AT_ENERGY_PEAKS;
            else
                if (value.StartsWith("AT_INTERVALS_OF_"))
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
                        System.Console.WriteLine("Template.GetFVExtraction():- WARNING! INVALID INTEGER:- " + words[3]);
                        System.Console.WriteLine(ex);
                        int32 = 0;
                    }
                    state.FeatureVectorExtractionInterval = int32;
                }
                else
                {
                    Console.WriteLine("Template.GetFVExtraction():- WARNING! INVALID EXTRACTION VALUE IS DEFINED FOR FEATURE VECTORS! " + value);
                    Console.WriteLine("                             SET THE DEFAULT:- FV_Extraction = AT_ENERGY_PEAKS");
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
            Console.WriteLine(" Template name: " + this.templateState.CallName);
            Console.WriteLine(" Comment: " + this.templateState.CallComment);
            Console.WriteLine(" Template dir     : " + this.templateState.TemplateDir);
            Console.WriteLine(" Template ini file: " + this.TemplatePath);
            Console.WriteLine(" Bottom freq=" + this.templateState.MinTemplateFreq + "  Mid freq=" + this.templateState.MidTemplateFreq + " Top freq=" + this.templateState.MaxTemplateFreq);
        }


    }//end Class Template


}
