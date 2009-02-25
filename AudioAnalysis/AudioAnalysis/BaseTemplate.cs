using System;
using TowseyLib;
using System.IO;
using AudioTools;



namespace AudioAnalysis
{

    public enum Feature_Type { UNDEFINED, MFCC }
    public enum Task { UNDEFINED, EXTRACT_FV, CREATE_ACOUSTIC_MODEL, VERIFY_MODEL }



	public abstract class BaseTemplate
	{

        public static Task task { get; set; }
        public static bool InTestMode = false;   //set this true when doing a unit test

        #region Properties
        public int CallID { get; set; }
        public string CallName { get; set; }
        public string Comment { get; set; }
        public string OPPath { get; set; } //path of opfile containing saved template data
        public string SourcePath { get; set; } // Path to original audio recording used to generate the template
        public string SourceDir { get; set; } // Dir of original audio recording

        private double zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
        public double ZScoreThreshold { get { return zScoreThreshold; } }
        public int MinTemplateFreq { get; set; }
        public int MaxTemplateFreq { get; set; }


        public AVSonogramConfig SonogramConfig { get; set; }
        public FVConfig FeatureVectorConfig { get; set; }
        public AcousticModel AcousticModelConfig { get; set; }
        public BaseModel Model { get; set; }
        public ModelType Modeltype { get; set; }
        public BaseResult Result { get; set; }
        #endregion


        #region Static Methods

        /// <summary>
        /// use this Load method when reading a template from previously saved tamplate file
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public static BaseTemplate Load(string configFile)
        {
            var config = new Configuration(configFile);
            config.SetPair("MODE", "READ_TEMPLATE"); //may be not necessary - have not used so far
            return Load(config);
        }

        /// <summary>
        /// use this Load method when creating a template from user generated params
        /// </summary>
        /// <param name="appConfigFile"></param>
        /// <param name="gui"></param>
        /// <returns></returns>
        public static BaseTemplate Load(string appConfigFile, GUI gui)
        {
            var config = MergeProperties(appConfigFile, gui);
            config.SetPair("MODE", "CREATE_TEMPLATE"); //may be not necessary - have not used so far
            return Load(config);
        }

        public static BaseTemplate Load(Configuration config)
        {
            var modelName = config.GetString("MODEL_TYPE");

            ModelType modelType = (ModelType)Enum.Parse(typeof(ModelType), modelName);
            if (modelName.StartsWith("UNDEFINED")) return new Template_MFCC(config);
            else
                if (modelName.StartsWith("MM_ERGODIC")) return new Template_MFCC(config);
            else
                    if (modelName.StartsWith("MM_TWO_STATE_PERIODIC")) return new Template_MFCC(config);
                else
                        if (modelName.StartsWith("ONE_PERIODIC_SYLLABLE")) return new Template_MFCC(config);
                    else
                    {
                        Log.Write("ERROR at BaseTemplate Load(Configuration config);\n" +
                            "The ModelType = " + modelName + " which is an unknown Type");
                        throw new ArgumentException("Unrecognised template type.");
                    }
        }


        public static Configuration MergeProperties(string appConfigFile, GUI gui)
        {
            var config = new Configuration(appConfigFile);
            config.SetPair("TEMPLATE_ID", gui.CallID.ToString());
            config.SetPair("CALL_NAME", gui.CallName);
            config.SetPair("COMMENT", gui.CallComment);
            //**************** INFO ABOUT ORIGINAL .WAV FILE
            config.SetPair("WAV_DIR", gui.WavDirName); //wavDirName = @"C:\SensorNetworks\WavFiles\";
            config.SetPair("WAV_FILE_NAME", gui.SourceFile);  //Lewin's rail kek keks.
            config.SetPair("WAV_FILE_PATH", gui.SourcePath);
            //config.SetPair("NYQUIST_FREQ", gui.); //default value set in appConfig File
            //config.SetPair("WAV_SAMPLE_RATE",);   //default value set in appConfig File
            //config.SetPair("WAV_DURATION", gui.); //default value set in appConfig File
            //**************** INFO ABOUT FRAMES
            config.SetPair("FRAME_SIZE", gui.FrameSize.ToString());
            config.SetPair("FRAME_OVERLAP", gui.FrameOverlap.ToString());
            //config.SetPair("", gui.DynamicRange.ToString()); 
            //**************** FEATURE PARAMETERS
            config.SetPair("FEATURE_TYPE", gui.FeatureType.ToString());

            config.SetPair("MIN_FREQ", gui.Min_Freq.ToString());
            config.SetPair("MAX_FREQ", gui.Max_Freq.ToString());
            config.SetPair("FILTERBANK_COUNT", gui.FilterBankCount.ToString());
            config.SetPair("DO_MEL_CONVERSION", gui.DoMelConversion.ToString());
            config.SetPair("DO_NOISE_REDUCTION", gui.DoNoiseReduction.ToString());
            config.SetPair("CC_COUNT", gui.CeptralCoeffCount.ToString());
            config.SetPair("INCLUDE_DELTA", gui.IncludeDeltaFeatures.ToString());
            config.SetPair("INCLUDE_DOUBLEDELTA", gui.IncludeDoubleDeltaFeatures.ToString());
            config.SetPair("DELTA_T", gui.DeltaT.ToString());
            //**************** FV EXTRACTION OPTIONS **************************
            config.SetPair("FV_SOURCE", gui.Fv_Source.ToString());

            //FV INIT is a table of strings. Must deconstruct
            int fvCount = gui.FvInit.GetLength(0);
            config.SetPair("FV_COUNT", fvCount.ToString());
            for (int i = 0; i < fvCount; i++)
            {
                config.SetPair("FV"+(i+1)+"_DATA", gui.FvInit[i,0]+"\t"+gui.FvInit[i,1]);
            }

            config.SetPair("MARQUEE_TYPE", gui.Fv_Extraction.ToString());
            config.SetPair("MARQUEE_START", gui.MarqueeStart.ToString());
            config.SetPair("MARQUEE_END", gui.MarqueeEnd.ToString());
            config.SetPair("MARQUEE_INTERVAL", gui.FvExtractionInterval.ToString());
            //config.SetPair("FV_DESCRIPTOR", gui.FVDescriptor);
            //config.SetPair("FV_DO_AVERAGING", gui.DoFvAveraging.ToString());
            config.SetPair("FV_DEFAULT_NOISE_FILE", gui.FvDefaultNoiseFile);

            //**************** INFO ABOUT FEATURE VECTORS - THE ACOUSTIC MODEL ***************
            //config.SetPair("FEATURE_VECTOR_LENGTH", gui.MmType.ToString());
            //config.SetPair("NUMBER_OF_FEATURE_VECTORS", gui.MmType.ToString());


            //THRESHOLDS FOR THE ACOUSTIC MODEL
            //THRESHOLD OPTIONS: 3.1(p=0.001), 2.58(p=0.005), 2.33(p=0.01), 2.15(p=0.03), 1.98(p=0.05),
            config.SetPair("ZSCORE_THRESHOLD", gui.ZScoreThreshold.ToString());

            //**************** INFO ABOUT LANGUAGE MODEL
            //There are 3 choices of MODEl TYPE: (1)UNDEFINED (2)ERGODIC (3)TWO_STATE_PERIODIC (4)OLD_PERIODIC
            config.SetPair("MODEL_TYPE", gui.ModelType.ToString());
            //There are 3 choices of MM: (1)UNDEFINED (2)ERGODIC (3)TWO_STATE_PERIODIC (4)OLD_PERIODIC
            //config.SetPair("MM_TYPE", gui.MmType.ToString());
            config.SetPair("GAP_MS", "999");       //default value for template creation
            config.SetPair("PERIODICITY_MS", gui.CallPeriodicity.ToString());
            config.SetPair("NUMBER_OF_WORDS", "1"); //default value for template creation
            config.SetPair("WORD1_NAME", "word");   //default value for template creation
            config.SetPair("WORD1_EXAMPLE1", "1");  //default value for template creation

            return config;
        }

        #endregion



        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
		public BaseTemplate(Configuration config)
		{
            CallID  = config.GetInt("TEMPLATE_ID");
            CallName = config.GetString("CALL_NAME");   //e.g.  Lewin's Rail Kek-kek
            Comment = config.GetString("COMMENT");  //e.g.Template consists of a single KEK!
            SourcePath = config.GetString("WAV_FILE_PATH");
            SourceDir  = Path.GetDirectoryName(SourcePath);
		}

        public void ExtractTemplateFromSonogram(string wavPath)
        {
            ExtractTemplateFromSonogram(new WavReader(wavPath));
        }

        public void ExtractTemplateFromSonogram(WavReader wav)
        {
            ExtractTemplateFromSonogram(new CepstralSonogram(SonogramConfig, wav));
        }

        public void ExtractTemplateFromSonogram(CepstralSonogram sono)
        {
            FVExtractor.ExtractFVsFromSonogram(sono, FeatureVectorConfig, SonogramConfig);
        }

        public void LoadFeatureVectorsFromFile(string templateDir)
        {
            this.FeatureVectorConfig.LoadFromFile(templateDir);
        }

        public void GenerateAndSaveSymbolSequence(AcousticVectorsSonogram sonogram, string opDir)
        {
            this.AcousticModelConfig.GenerateSymbolSequence(sonogram, this);
            this.AcousticModelConfig.SaveSymbolSequence(Path.Combine(opDir, "symbolSequences.txt"), false);
        }

        public void GenerateSymbolSequence(AcousticVectorsSonogram sonogram)
        {
            this.AcousticModelConfig.GenerateSymbolSequence(sonogram, this);
        }


        public void SaveSyllablesImage(AcousticVectorsSonogram sonogram, string path)
        {
            var image = new Image_MultiTrack(sonogram.GetImage());
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModelConfig.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModelConfig.SyllableIDs, garbageID));
            image.Save(path);
        }
        public void SaveEnergyImage(SpectralSonogram sonogram, string path)
        {
            Log.WriteIfVerbose("Basetemplate.SaveEnergyImage(SpectralSonogram sonogram, string path)");
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            image.Save(path);
        }

        public void SaveSyllablesImage(WavReader wav, string imagePath)
        {
            bool doExtractSubband = false;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav, doExtractSubband);
            SaveSyllablesImage(spectralSono, imagePath);
        }

        public void SaveSyllablesImage(SpectralSonogram sonogram, string path)
        {
            Log.WriteIfVerbose("Basetemplate.SaveSyllablesImage(SpectralSonogram sonogram, string path)");
            //want full bandwidth image with green band highlight and gridlines
          //  bool isSubband = sonogram.ExtractSubband;
            bool doHighlightSubband = true;
            bool add1kHzLines       = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModelConfig.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModelConfig.SyllableIDs, garbageID));
            image.Save(path);
        }


        public virtual void SaveResultsImage(WavReader wav, string imagePath, BaseResult result)
        {
            bool doExtractSubband = false;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav, doExtractSubband);
            SaveResultsImage(spectralSono, imagePath, result);
        }

        public virtual void SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)
        {
            Log.WriteIfVerbose("Basetemplate.SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModelConfig.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModelConfig.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, 0.0, 0.0));
            image.Save(path);
        }



        public virtual void Save(string targetPath)
        {
            this.OPPath = targetPath;
            if (File.Exists(targetPath)) File.Copy(targetPath, targetPath + ".OLD", true); //overwrite
            using (var file = new StreamWriter(targetPath))
            {
                Save(file);
            }
        }

		public virtual void Save(TextWriter writer)
		{
            writer.WriteLine("DATE="+DateTime.Now.ToString("u"));  //u format=2008-11-05 14:40:28Z
            writer.WriteLine("#");
            writer.WriteLine("#**************** TEMPLATE DATA");
            writer.WriteConfigValue("TEMPLATE_ID", CallID);
            writer.WriteConfigValue("CALL_NAME", CallName); //CALL_NAME=Lewin's Rail Kek-kek
            writer.WriteConfigValue("COMMENT", Comment);    //COMMENT=Template consists of a single KEK!
            writer.WriteConfigValue("THIS_FILE", OPPath);   //THIS_FILE=C:\SensorNetworks\Templates\Template_2\template_2.ini
            writer.WriteLine("#");
            writer.WriteLine("#**************** INFO ABOUT ORIGINAL .WAV FILE");
            writer.WriteConfigValue("DIR_LOCATION", SourceDir);  //WAV_FILE_PATH=C:\SensorNetworks\WavFiles\
            writer.WriteConfigValue("WAV_FILE_NAME", Path.GetFileName(SourcePath));  //WAV_FILE_PATH=BAC2_20071008-085040.wav
            writer.WriteConfigValue("WAV_DURATION", SonogramConfig.Duration.TotalSeconds);
            writer.WriteConfigValue("WAV_SAMPLE_RATE", SonogramConfig.SampleRate);
            writer.WriteLine("#");
            writer.Flush();
		}




	}
}