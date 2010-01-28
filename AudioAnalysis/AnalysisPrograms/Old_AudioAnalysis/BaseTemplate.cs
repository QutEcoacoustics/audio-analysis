using System;
using TowseyLib;
using System.IO;
using AudioTools;
//using System.Collections;
using System.Collections.Generic;

namespace AudioAnalysis
{
    public enum Mode { UNDEFINED, CREATE_NEW_TEMPLATE, READ_EXISTING_TEMPLATE }

	[Serializable]
	public abstract class BaseTemplate
	{
        #region Static Variables
        public static bool InTestMode = false;   //set this true when doing a functional test
        #endregion


        #region Properties
        public Mode mode { get; set; }         //MODE in which the template is operating
        public string AuthorName { get; set; }
        public int CallID { get; set; }
        public string CallName { get; set; }
        public string Comment { get; set; }
        public string DataPath   { get; set; } // path of saved template file
        public string DataDir { get; set; }    // dir containing saved template data
        public string SourcePath { get; set; } // Path to original audio recording used to generate the template
        public string SourceDir  { get; set; } // Dir of original audio recording

        public ConfigKeys.Feature_Type FeatureExtractionType { get; set; }
        public CepstralSonogramConfig SonogramConfig { get; set; }
        public FVConfig FeatureVectorConfig { get; set; }
        public Acoustic_Model AcousticModel { get; set; }
        public BaseModel LanguageModel { get; set; }
        public LanguageModelType Modeltype { get; set; }
        #endregion

        #region Static LOAD TEMPLATE Methods

        /// <summary>
        /// call this Load method when creating a new template from user provided params
        /// </summary>
        /// <param name="appConfigFile"></param>
        /// <param name="gui"></param>
        /// <returns></returns>
        public static BaseTemplate Load(string appConfigFile, GUI gui)
        {
            var config = MergeProperties(appConfigFile, gui);
            config.SetPair("MODE", Mode.CREATE_NEW_TEMPLATE.ToString());
            BaseTemplate template = Load(config);
            return template;
        }

        /// <summary>
        /// call this Load method when creating a new template from user provided params
        /// using only one line of code!
        /// </summary>
        /// <param name="appConfigFile">default param values</param>
        /// <param name="gui">param values for this specific template</param>
        /// <param name="recording">the recording from which templtae to be extracted</param>
        /// <param name="templateFName">the path to which template info is to be saved</param>
        /// <returns></returns>
        public static BaseTemplate Load(string appConfigFile, GUI gui, AudioRecording recording, string templateDir, string templateFName)
        {
            //STEP ONE: Initialise template with parameters
            string opTemplatePath = templateDir + templateFName;
            var config = MergeProperties(appConfigFile, gui);
            config.SetPair("WAV_FILE_NAME", recording.FilePath);
            config.SetPair("MODE", Mode.CREATE_NEW_TEMPLATE.ToString());
            config.SetPair("TEMPLATE_DIR", templateDir);
            BaseTemplate template = Load(config);

            //STEP TWO: Extract template and save
            template.ExtractTemplateFromRecording(recording);
            template.Save(opTemplatePath);
            return template;
        }

        /// <summary>
        /// Verify the template - what happens depends on the feature extraction type of template
        /// STEP THREE: Verify fv extraction by observing output from acoustic model.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="recording"></param>
        /// <param name="templateDir"></param>
        public static void VerifyTemplate(BaseTemplate template, AudioRecording recording, string templateDir)
        {

            Log.WriteIfVerbose("\nSTEP THREE: Verify template and save scanned recording as image");
            //Set up a spectral sonogram for image with symbol sequence track added
            bool existingValue = template.SonogramConfig.DoFullBandwidth; //store current value of this boolean 
            template.SonogramConfig.DoFullBandwidth = true;
            var spectralSono = new SpectralSonogram(template.SonogramConfig, recording.GetWavReader());
            spectralSono.CalculateSubbandSNR(recording.GetWavReader(), (int)template.SonogramConfig.MinFreqBand, (int)template.SonogramConfig.MaxFreqBand);

            if (template.FeatureExtractionType == ConfigKeys.Feature_Type.DCT_2D)
            {
                ((Template_DCT2D)template).ScanRecording(recording, templateDir);
                //Save an image of the spectralsonogram with symbol sequence track added
                var imagePath = Path.Combine(templateDir, Path.GetFileNameWithoutExtension(template.SourcePath) + ".png");
                ((Template_DCT2D)template).SaveScanImage(spectralSono, imagePath);
            }
            else
            if (template.FeatureExtractionType == ConfigKeys.Feature_Type.CC_AUTO)
            {
                template.GenerateSymbolSequenceAndSave(recording, templateDir);
                //Save an image of the spectralsonogram with symbol sequence track added
                var imagePath = Path.Combine(templateDir, Path.GetFileNameWithoutExtension(recording.FileName) + ".png");
                template.SaveSyllablesImage(spectralSono, imagePath);
            }
            else
            {
                template.GenerateSymbolSequenceAndSave(recording, templateDir);
                //Save an image of the spectralsonogram with symbol sequence track added
                var imagePath = Path.Combine(templateDir, Path.GetFileNameWithoutExtension(template.SourcePath) + ".png");
                template.SaveSyllablesImage(spectralSono, imagePath);
            }
            template.SonogramConfig.DoFullBandwidth = existingValue; //restore value of this boolean 
        } //end VerifyTemplate()



        /// <summary>
        /// use this Load method when reading a template from previously saved tamplate file
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public static BaseTemplate Load(string appConfigPath, string templatePath)
        {
            var config = new TowseyLib.Configuration(appConfigPath, templatePath); //merge config into one class
            config.SetPair("TEMPLATE_PATH", templatePath); //inform template of its location in file system
            config.SetPair("TEMPLATE_DIR", Path.GetDirectoryName(templatePath)); //inform template of location of feature vector files
            config.SetPair("MODE", Mode.READ_EXISTING_TEMPLATE.ToString());
            var template = BaseTemplate.Load(config);
            template.LoadFeatureVectorsFromFile();
            return template;
        }

        public static BaseTemplate Load(Configuration config)
        {
            var featureExtractionName = config.GetString("FEATURE_TYPE");

            ConfigKeys.Feature_Type featureExtractionType = (ConfigKeys.Feature_Type)Enum.Parse(typeof(ConfigKeys.Feature_Type), featureExtractionName);
            if (featureExtractionName.StartsWith("MFCC")) return new Template_CC(config);
            else
            if (featureExtractionName.StartsWith("CC_AUTO")) return new Template_CCAuto(config);
            else
            if (featureExtractionName.StartsWith("DCT_2D")) return new Template_DCT2D(config);
            else
            if (featureExtractionName.StartsWith("UNDEFINED"))
            {
                throw new Exception("The feature extraction type <" + featureExtractionName + "> is undefined. FATAL ERROR!");
            }
            else
            {
               Log.Write("ERROR at BaseTemplate Load(Configuration config);\n" +
               "The Feature Extraction Type = " + featureExtractionName + " which is an unknown.");
               throw new ArgumentException("Unrecognised type.");
            }
        }


        public static Configuration MergeProperties(string appConfigFile, GUI gui)
        {
            LoadDefaultConfig(); //just in case the appConfig does not exist

            if (!File.Exists(appConfigFile)) return null;
 
            //read the config table
            var config = new Configuration(appConfigFile);

            //**************** LOAD STATIC PARAMETERS
            LoadStaticConfig(config);

            //**************** ID info
            config.SetPair("AUTHOR", gui.AuthorName.ToString());
            config.SetPair("TEMPLATE_ID", gui.CallID.ToString());
            config.SetPair("CALL_NAME", gui.CallName);
            config.SetPair("COMMENT", gui.Comment);

            //**************** INFO ABOUT ORIGINAL .WAV FILE
            config.SetPair(ConfigKeys.Recording.Key_TrainingDirName, gui.TrainingDirName); //location of training vocalisations
            config.SetPair(ConfigKeys.Recording.Key_TestingDirName,  gui.TestDirName);     //location of testing vocalisations
            config.SetPair("WAV_DIR", gui.WavDirName);           //wavDirName = @"C:\SensorNetworks\WavFiles\";
            config.SetPair("WAV_FILE_NAME", gui.SourceFile);     //file containing source vocalisation.
            config.SetPair("WAV_FILE_PATH", gui.SourcePath);

            //**************** INFO ABOUT FRAMES
            config.SetPair(ConfigKeys.Windowing.Key_WindowSize, gui.FrameSize.ToString());
            config.SetPair(ConfigKeys.Windowing.Key_WindowOverlap, gui.FrameOverlap.ToString());
            //config.SetPair(ConfigKeys.Windowing.Key_SubSample, gui.);

            //**************** FEATURE PARAMETERS
            //config.SetPair("WAV_SAMPLE_RATE",);   //set when recording is loaded
            config.SetPair("FEATURE_TYPE", gui.FeatureType.ToString());
            config.SetPair(ConfigKeys.Snr.Key_DynamicRange, gui.DynamicRange.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_MinFreq, gui.Min_Freq.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_MaxFreq, gui.Max_Freq.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_StartTime, gui.StartTime.ToString()); //used for defining a marqueed vocalisation
            config.SetPair(ConfigKeys.Mfcc.Key_EndTime, gui.EndTime.ToString());   //used for defining a marqueed vocalisation

            config.SetPair(ConfigKeys.Mfcc.Key_FilterbankCount, gui.FilterBankCount.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_DoMelScale, gui.DoMelConversion.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_NoiseReductionType, gui.NoiseReductionType.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_CcCount, gui.CeptralCoeffCount.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDelta, gui.IncludeDeltaFeatures.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDoubleDelta, gui.IncludeDoubleDeltaFeatures.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_DeltaT, gui.DeltaT.ToString());

            //**************** FV EXTRACTION OPTIONS **************************
            config.SetPair(ConfigKeys.Template.Key_ExtractInterval, gui.ExtractionInterval.ToString());
            config.SetPair("NUMBER_OF_SYLLABLES", gui.NumberOfSyllables.ToString());//CC_AUTO option
            config.SetPair("FV_SOURCE", gui.Fv_Source.ToString()); //MFCC option
            //FV INIT is a table of strings. Must deconstruct
            if (gui.FvInit != null)
            {
                int fvCount = gui.FvInit.GetLength(0);
                config.SetPair(ConfigKeys.Template.Key_FVCount, fvCount.ToString());
               for (int i = 0; i < fvCount; i++)
               {
                   config.SetPair("FV"+(i+1)+"_DATA", gui.FvInit[i,0]+"\t"+gui.FvInit[i,1]);
               }
            }
            config.SetPair("FV_DEFAULT_NOISE_FILE", gui.FvDefaultNoiseFile);


            //******************* THRESHOLDS FOR THE ACOUSTIC MODEL ****************
            //THRESHOLD OPTIONS: 3.1(p=0.001), 2.58(p=0.005), 2.33(p=0.01), 2.15(p=0.03), 1.98(p=0.05),
            config.SetPair("ZSCORE_THRESHOLD", gui.ZScoreThreshold.ToString());

            //**************** INFO ABOUT LANGUAGE MODEL ***************************
            //There are 3 choices of MARKOV MODEL TYPE: (1)UNDEFINED (2)ERGODIC (3)TWO_STATE_PERIODIC (4)OLD_PERIODIC
            config.SetPair(ConfigKeys.Template.Key_ModelType, gui.ModelType.ToString());
            config.SetPair("GAP_MS", "999");       //default value for template creation
            config.SetPair("PERIODICITY_MS", gui.CallPeriodicity.ToString());
            int wordCount = 1; //default value
            if (gui.NumberOfWords > 1) wordCount = gui.NumberOfWords;
            config.SetPair(ConfigKeys.Template.Key_WordCount, wordCount.ToString());
            for (int i = 0; i < wordCount; i++)
                config.SetPair("WORD"+(i+1)+"_NAME", gui.WordNames[i]);  
            config.SetPair("WORD1_EXAMPLE1", "1");  //default value for template creation
            return config;
        }

        public static void LoadDefaultConfig()
        {
            Log.Verbosity = 0;
            EndpointDetectionConfiguration.K1Threshold = 3.5;
            EndpointDetectionConfiguration.K2Threshold = 6.0;
            EndpointDetectionConfiguration.K1K2Latency = 0.05;
            EndpointDetectionConfiguration.VocalGap = 0.2;
            EndpointDetectionConfiguration.MinPulseDuration = 0.075;
        }

        public static Configuration LoadStaticConfig(string appConfigFile)
        {
            var config = new Configuration(appConfigFile);
            return LoadStaticConfig(config);
        }
        public static Configuration LoadStaticConfig(Configuration config)
        {
            Log.Verbosity = config.GetInt("VERBOSITY");
            //SAMPLE_RATE=0
            //SUBSAMPLE=0
            //WINDOW_OVERLAP=0.5
            //WINDOW_SIZE=512
            EndpointDetectionConfiguration.K1Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold);
            EndpointDetectionConfiguration.K2Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold);
            EndpointDetectionConfiguration.K1K2Latency = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1K2Latency);
            EndpointDetectionConfiguration.VocalGap    = config.GetDouble(ConfigKeys.EndpointDetection.Key_VocalGap);
            EndpointDetectionConfiguration.MinPulseDuration = config.GetDouble(ConfigKeys.EndpointDetection.Key_MinVocalDuration);
            //DO_MELSCALE=false
            //NOISE_REDUCE=false
            //FILTERBANK_COUNT=64
            //CC_COUNT=12            
            //INCLUDE_DOUBLE_DELTA=true
            //INCLUDE_DELTA=true
            //DELTA_T=2
            return config;
        }

        #endregion

		public BaseTemplate()
		{
		}

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
		public BaseTemplate(Configuration config)
		{
            Log.Verbosity = config.GetInt("VERBOSITY");
            Log.WriteIfVerbose("BaseTemplate CONSTRUCTOR: VERBOSITY = " + Log.Verbosity);

            string modeStr = config.GetString("MODE");   // Mode.READ_EXISTING_TEMPLATE;
            if (modeStr == null) mode = Mode.UNDEFINED;
            else                 mode = (Mode)Enum.Parse(typeof(Mode), modeStr);

            AuthorName = config.GetString("AUTHOR");    //e.g. Michael Towsey
            CallID = config.GetInt("TEMPLATE_ID");
            CallName = config.GetString("CALL_NAME");   //e.g.  Lewin's Rail Kek-kek

            Log.WriteIfVerbose("\n\nINITIALISING TEMPLATE: mode=" + mode.ToString() + " name=" + CallName + " id=" + CallID);

            Comment = config.GetString("COMMENT");  //e.g.Template consists of a single KEK!
            SourcePath = config.GetString("WAV_FILE_PATH");
            SourceDir  = Path.GetDirectoryName(SourcePath);
            DataPath = config.GetString("TEMPLATE_PATH");
            DataDir = config.GetString("TEMPLATE_DIR");
            if ((SourcePath == null) ||(SourcePath == "")) 
                SourcePath = "Source path not set!!"; //string must be given a value to enable later serialisation check
            if ((SourceDir  == null) ||(SourceDir == ""))  
                SourceDir  = "Source dir not set!!";  //string must be given a value to enable later serialisation check  

            var featureExtractionName = config.GetString("FEATURE_TYPE");
            this.FeatureExtractionType = (ConfigKeys.Feature_Type)Enum.Parse(typeof(ConfigKeys.Feature_Type), featureExtractionName);
            EndpointDetectionConfiguration.SetConfig(config);
        }


        protected abstract void ExtractTemplateFromRecording(AudioRecording ar);



        public virtual void Save(string targetPath)
        {
            this.DataPath = targetPath;
            if (File.Exists(targetPath)) File.Copy(targetPath, targetPath + "OLD.txt", true); //overwrite
            using (var file = new StreamWriter(targetPath))
            {
                Save(file);
            }
        }

        public virtual void Save(TextWriter writer)
        {
            writer.WriteLine("DATE=" + DateTime.Now.ToString("u"));  //u format=2008-11-05 14:40:28Z
            writer.WriteConfigValue("AUTHOR", AuthorName);
            writer.WriteLine("#");
            writer.WriteLine("#**************** TEMPLATE DATA");
            writer.WriteConfigValue("TEMPLATE_ID", CallID);
            writer.WriteConfigValue("CALL_NAME", CallName); //CALL_NAME=Lewin's Rail Kek-kek
            writer.WriteConfigValue("COMMENT", Comment);    //COMMENT=Template consists of a single KEK!
            writer.WriteConfigValue("THIS_FILE", DataPath);   //THIS_FILE=C:\SensorNetworks\Templates\Template_2\template_2.ini
            writer.WriteLine("#");
            writer.WriteLine("#**************** INFO ABOUT ORIGINAL .WAV FILE");
            writer.WriteConfigValue("DIR_LOCATION", SourceDir);  //WAV_FILE_PATH=C:\SensorNetworks\WavFiles\
            writer.WriteConfigValue("WAV_FILE_NAME", Path.GetFileName(SourcePath));  //WAV_FILE_PATH=BAC2_20071008-085040.wav
            writer.WriteConfigValue("WAV_DURATION", SonogramConfig.Duration.TotalSeconds);
            writer.WriteConfigValue(ConfigKeys.Windowing.Key_SampleRate, SonogramConfig.FftConfig.SampleRate);
            writer.WriteLine("#");
            writer.Flush();
        }




        public void LoadFeatureVectorsFromFile()
        {
            this.FeatureVectorConfig.LoadFromFile(this.DataDir);
        }

        public void GenerateSymbolSequenceAndSave(AudioRecording ar, string opDir)
        {
            //GenerateSymbolSequence(ar.GetWavReader());
            var wav = ar.GetWavReader();
            var avSonogram = new AcousticVectorsSonogram(this.SonogramConfig, wav);
            this.AcousticModel.GenerateSymbolSequence(avSonogram, this);
            this.AcousticModel.SaveSymbolSequence(Path.Combine(opDir, "symbolSequences.txt"), false);
        }

        //public void GenerateSymbolSequence(WavReader wav)
        //{
        //    //generate info about symbol sequence
        //    var avSonogram = new AcousticVectorsSonogram(this.SonogramConfig, wav);
        //    this.AcousticModel.GenerateSymbolSequence(avSonogram, this);
        //    //Console.WriteLine("ss="+this.AcousticModel.SyllSymbols);
        //    //Console.WriteLine("End of the Line");
        //    //Console.ReadLine();
        //}

        public void SaveEnergyImage(SpectralSonogram sonogram, string path)
        {
            Log.WriteIfVerbose("Basetemplate.SaveEnergyImage(SpectralSonogram sonogram, string path)");
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            image.Save(path);
        }

        public void SaveSyllablesImage(SpectralSonogram sonogram, string path)
        {
            Log.WriteIfVerbose("Basetemplate.SaveSyllablesImage(SpectralSonogram sonogram, string path)");
            //want full bandwidth image with green band highlight and gridlines
            bool doHighlightSubband = true;
            bool add1kHzLines       = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image4.Image.Width));
            //image.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.Save(path);
        }


        public void SaveResultsImage(WavReader wav, string imagePath, BaseResult result)
        {
            bool value = this.SonogramConfig.DoFullBandwidth; //store existing value for this bool
            this.SonogramConfig.DoFullBandwidth = true;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav);
            SaveResultsImage(spectralSono, imagePath, result);
            this.SonogramConfig.DoFullBandwidth = value;      //restore bool value
        }

        public virtual void SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)
        {
            Log.WriteIfVerbose("Basetemplate.SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, result.MinDisplayScore, result.MaxDisplayScore, result.DisplayThreshold));
            image.Save(path);
        }

        public void SaveSyllablesAndResultsImage(WavReader wav, string imagePath, BaseResult result)
        {
            bool value = this.SonogramConfig.DoFullBandwidth; //store existing value for this bool
            this.SonogramConfig.DoFullBandwidth = true;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav);
            SaveSyllablesAndResultsImage(spectralSono, imagePath, result);
            this.SonogramConfig.DoFullBandwidth = value;      //restore bool value
        }
        public virtual void SaveSyllablesAndResultsImage(SpectralSonogram sonogram, string path, BaseResult result)
        {
            Log.WriteIfVerbose("Basetemplate.SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, result.MinDisplayScore, result.MaxDisplayScore, result.DisplayThreshold));
            image.Save(path);
        }
        public void SaveSyllablesAndResultsImage(WavReader wav, string imagePath, BaseResult result, List<AcousticEvent> list)
        {
            bool value = this.SonogramConfig.DoFullBandwidth; //store existing value for this bool
            this.SonogramConfig.DoFullBandwidth = true;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav);
            SaveSyllablesAndResultsImage(spectralSono, imagePath, result, list);
            this.SonogramConfig.DoFullBandwidth = value;      //restore bool value
        }
        public virtual void SaveSyllablesAndResultsImage(SpectralSonogram sonogram, string path, BaseResult result, List<AcousticEvent> list)
        {
            Log.WriteIfVerbose("Basetemplate.SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddEvents(list);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, result.MinDisplayScore, result.MaxDisplayScore, result.DisplayThreshold));
            image.Save(path);
        }


        public virtual BaseResult GetBlankResultCard()
        {
            LanguageModelType type = this.LanguageModel.ModelType;
            if (type == LanguageModelType.MM_ERGODIC) return new Result_MMErgodic(this);
                else
                if (type == LanguageModelType.ONE_PERIODIC_SYLLABLE) return new Result_1PS(this);
                else
                {
                    Log.WriteIfVerbose("\n WARNING: BaseTemplate.GetBlankResultCard(): UNKNOWN MODEL TYPE = "+type.ToString());
                    return null;
                }
        }


    }//end class
}
