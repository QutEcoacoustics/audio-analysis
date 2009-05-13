using System;
using TowseyLib;
using System.IO;
using AudioTools;
//using System.Collections;
using System.Collections.Generic;

namespace AudioAnalysis
{
    public enum Feature_Type { UNDEFINED, MFCC, CC_AUTO, DCT_2D }
    public enum Mode { UNDEFINED, CREATE_NEW_TEMPLATE, READ_EXISTING_TEMPLATE }

	[Serializable]
	public abstract class BaseTemplate
	{
        #region Static Variables
        public static bool InTestMode = false;   //set this true when doing a functional test
        #endregion


        #region Properties
        public Mode mode { get; set; }   //MODE in which the template is operating
        public string AuthorName { get; set; }
        public int CallID { get; set; }
        public string CallName { get; set; }
        public string Comment { get; set; }
        public string DataPath   { get; set; } // path of saved template file
        public string DataDir { get; set; }    // dir containing saved template data
        public string SourcePath { get; set; } // Path to original audio recording used to generate the template
        public string SourceDir  { get; set; } // Dir of original audio recording

        public Feature_Type FeatureExtractionType { get; set; }
        public AVSonogramConfig SonogramConfig { get; set; }
        public FVConfig FeatureVectorConfig { get; set; }
        public Acoustic_Model AcousticModel { get; set; }
        public BaseModel Model { get; set; }
        public ModelType Modeltype { get; set; }
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

            //STEP TWO: Extract template
            template.ExtractTemplateFromRecording(recording);
            template.Save(opTemplatePath);

            //STEP THREE: Verify fv extraction by observing output from acoustic model.
            template.GenerateSymbolSequenceAndSave(recording, gui.opDir);

            //STEP FIVE: save an image of the sonogram with symbol sequence track added
            var imagePath = Path.Combine(gui.opDir, Path.GetFileNameWithoutExtension(template.SourcePath) + ".png");
            template.SonogramConfig.DisplayFullBandwidthImage = true;
            var spectralSono = new SpectralSonogram(template.SonogramConfig, recording.GetWavReader());
            //spectralSono.CalculateSubbandSNR(new WavReader(wavPath), (int)template.SonogramConfig.MinFreqBand, (int)template.SonogramConfig.MaxFreqBand); 
            spectralSono.CalculateSubbandSNR(recording.GetWavReader(), (int)template.SonogramConfig.MinFreqBand, (int)template.SonogramConfig.MaxFreqBand);
            template.SaveSyllablesImage(spectralSono, imagePath);

            return template;
        }

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

            Feature_Type featureExtractionType = (Feature_Type)Enum.Parse(typeof(Feature_Type), featureExtractionName);
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
            LoadDefaultConfig(); //just in case the appCOnfig does not exist

            //set up the config table
            var config = new Configuration();
            if (File.Exists(appConfigFile)) config = new Configuration(appConfigFile);

            config.SetPair("AUTHOR", gui.AuthorName.ToString());
            config.SetPair("TEMPLATE_ID", gui.CallID.ToString());
            config.SetPair("CALL_NAME", gui.CallName);
            config.SetPair("COMMENT", gui.Comment);

            //**************** INFO ABOUT ORIGINAL .WAV FILE
            config.SetPair("TRAINING_DIR", gui.TrainingDirName); //location of training vocalisations
            config.SetPair("TESTING_DIR",  gui.TestDirName);     //location of testing vocalisations
            config.SetPair("WAV_DIR", gui.WavDirName);           //wavDirName = @"C:\SensorNetworks\WavFiles\";
            config.SetPair("WAV_FILE_NAME", gui.SourceFile);     //file containing source vocalisation.
            config.SetPair("WAV_FILE_PATH", gui.SourcePath);
            //config.SetPair("NYQUIST_FREQ", gui.); //default value set in appConfig File
            //config.SetPair("WAV_SAMPLE_RATE",);   //default value set in appConfig File
            //config.SetPair("WAV_DURATION", gui.); //default value set in appConfig File

            //**************** INFO ABOUT FRAMES
            config.SetPair(ConfigKeys.Windowing.Key_WindowSize, gui.FrameSize.ToString());
            config.SetPair(ConfigKeys.Windowing.Key_WindowOverlap, gui.FrameOverlap.ToString());
            //config.SetPair(ConfigKeys.Windowing.Key_SubSample, gui.);

            //**************** FEATURE PARAMETERS
            config.SetPair("FEATURE_TYPE", gui.FeatureType.ToString());
            //config.SetPair("DYNAMIC_RANGE", gui.DynamicRange.ToString()); 
            config.SetPair("MIN_FREQ",   gui.Min_Freq.ToString());
            config.SetPair("MAX_FREQ",   gui.Max_Freq.ToString());
            config.SetPair("START_TIME", gui.StartTime.ToString()); //used for defining a marqueed vocalisation
            config.SetPair("END_TIME",   gui.EndTime.ToString());   //used for defining a marqueed vocalisation

            config.SetPair(ConfigKeys.Mfcc.Key_FilterbankCount, gui.FilterBankCount.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_DoMelScale, gui.DoMelConversion.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_DoNoiseReduction, gui.DoNoiseReduction.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_CcCount, gui.CeptralCoeffCount.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDelta, gui.IncludeDeltaFeatures.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDoubleDelta, gui.IncludeDoubleDeltaFeatures.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_DeltaT, gui.DeltaT.ToString());

            //**************** FV EXTRACTION OPTIONS **************************
            config.SetPair("NUMBER_OF_SYLLABLES", gui.NumberOfSyllables.ToString());//CC_AUTO option
            config.SetPair("FV_SOURCE", gui.Fv_Source.ToString()); //MFCC option
            //FV INIT is a table of strings. Must deconstruct
            if (gui.FvInit != null)
            {
                int fvCount = gui.FvInit.GetLength(0);
                config.SetPair("FV_COUNT", fvCount.ToString());
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

        public static void LoadDefaultConfig()
        {
            Log.Verbosity = 0;
            FftConfiguration.WindowFunction = "Hamming";
            FftConfiguration.NPointSmoothFFT = 3;
            EndpointDetectionConfiguration.K1Threshold = 3.5;
            EndpointDetectionConfiguration.K2Threshold = 6.0;
            EndpointDetectionConfiguration.K1K2Latency = 0.05;
            EndpointDetectionConfiguration.VocalDelay = 0.2;
            EndpointDetectionConfiguration.MinPulseDuration = 0.075;
        }

        public static void LoadStaticConfig(string appConfigFile)
        {
            var config = new Configuration(appConfigFile);
            Log.Verbosity=config.GetInt("VERBOSITY");
            //SAMPLE_RATE=0
            //SUBSAMPLE=0
            //WINDOW_OVERLAP=0.5
            //WINDOW_SIZE=512
            FftConfiguration.WindowFunction  = config.GetString(ConfigKeys.Fft.Key_WindowFunction);
            FftConfiguration.NPointSmoothFFT = config.GetInt(ConfigKeys.Fft.Key_NPointSmoothFFT);
            EndpointDetectionConfiguration.K1Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold);
            EndpointDetectionConfiguration.K2Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold);
            EndpointDetectionConfiguration.K1K2Latency = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1K2Latency);
            EndpointDetectionConfiguration.VocalDelay = config.GetDouble(ConfigKeys.EndpointDetection.Key_VocalDelay);
            EndpointDetectionConfiguration.MinPulseDuration = config.GetDouble(ConfigKeys.EndpointDetection.Key_MinVocalDuration);
            //DO_MELSCALE=false
            //NOISE_REDUCE=false
            //FILTERBANK_COUNT=64
            //CC_COUNT=12            
            //INCLUDE_DOUBLE_DELTA=true
            //INCLUDE_DELTA=true
            //DELTA_T=2
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
            this.FeatureExtractionType = (Feature_Type)Enum.Parse(typeof(Feature_Type), featureExtractionName);
 
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
            writer.WriteConfigValue("WAV_SAMPLE_RATE", SonogramConfig.SampleRate);
            writer.WriteLine("#");
            writer.Flush();
        }




        public void LoadFeatureVectorsFromFile()
        {
            this.FeatureVectorConfig.LoadFromFile(this.DataDir);
        }

        public void GenerateSymbolSequenceAndSave(AudioRecording ar, string opDir)
        {
            GenerateSymbolSequence(ar.GetWavReader());
            this.AcousticModel.SaveSymbolSequence(Path.Combine(opDir, "symbolSequences.txt"), false);
        }

        public void GenerateSymbolSequence(WavReader wav)
        {
            //generate info about symbol sequence
            var avSonogram = new AcousticVectorsSonogram(this.SonogramConfig, wav);
            this.AcousticModel.GenerateSymbolSequence(avSonogram, this);
            //Console.WriteLine("End of the Line");
            //Console.ReadLine();
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
            this.SonogramConfig.DisplayFullBandwidthImage = true;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav);
            SaveResultsImage(spectralSono, imagePath, result);
        }

        public virtual void SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)
        {
            Log.WriteIfVerbose("Basetemplate.SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, 0.0, 0.0));
            image.Save(path);
        }

        public virtual BaseResult GetBlankResultCard()
        {
            ModelType type = this.Model.ModelType;
            if (type == ModelType.MM_ERGODIC) return new Result_MMErgodic(this);
                else
                if (type == ModelType.ONE_PERIODIC_SYLLABLE) return new Result_1PS(this);
                else
                {
                    Log.WriteIfVerbose("\n WARNING: BaseTemplate.GetBlankResultCard(): UNKNOWN MODEL TYPE = "+type.ToString());
                    return null;
                }
        }


    }//end class
}
