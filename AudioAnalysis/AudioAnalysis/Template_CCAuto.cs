using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using MarkovModels;
using System.IO;
using AudioTools;
using QutSensors.Data;

namespace AudioAnalysis
{
    [Serializable]
    class Template_CCAuto : BaseTemplate
    {
        public string TrainingDirName { get; set; }
        public string TestingDirName { get; set; }
        public string[] WordNames    { get; set; } // name of call or vocalisation 
        public string[] WordExamples { get; set; } // symbolSequences - examples of a single call. Derived from automatic template creation 

        /// <summary>
        /// call this Load method when creating a new template from user provided params
        /// using only one line of code!
        /// </summary>
        public static BaseTemplate Load(string appConfigFile, GUI gui, FileInfo[] recordingFiles, string templateDir, string templateFName)
        {
            Log.WriteIfVerbose("\nSTEP ONE: Initialise template with parameters");
            var config = MergeProperties(appConfigFile, gui);
            config.SetPair(ConfigKeys.Template.Key_TemplateDir, templateDir);
            config.SetPair(ConfigKeys.Recording.Key_RecordingDirName, recordingFiles[0].DirectoryName);//assume all files in same dir
            config.SetPair("MODE", Mode.CREATE_NEW_TEMPLATE.ToString());
            //STEP ONE: Initialise template with parameters
            var template = new Template_CCAuto(config);
            //STEP TWO: Extract template
            Log.WriteIfVerbose("\nSTEP TWO: Extract template");
            template.ExtractTemplateFromRecordings(recordingFiles);
            //STEP THREE: Extract template
            Log.WriteIfVerbose("\nSTEP THREE: Create language model");
            template.CreateLanguageModel(config);
            //STEP FOUR: Save template
            Log.WriteIfVerbose("\nSTEP FOUR: Save template");
            string opTemplatePath = templateDir + templateFName;
            template.Save(opTemplatePath);
            return template;
        }


        public Template_CCAuto(Configuration config) : base(config)
		{
            //Initialise all components of template with parameters
            //this.CallName = config.GetString("WORD" + (i + 1) + "_NAME");
            this.CallName        = config.GetString("WORD1_NAME");
            this.TrainingDirName = config.GetString(ConfigKeys.Recording.Key_TrainingDirName);
            this.TestingDirName  = config.GetString(ConfigKeys.Recording.Key_TestingDirName);
            SonogramConfig       = new CepstralSonogramConfig(config);
            FeatureVectorConfig  = new FVConfig(config);
            //TODO: enter this next parameter through GUI
            FeatureVectorConfig.ExtractionInterval = 5; //extract feature vectors at this interval
            AcousticModel = new Acoustic_Model(config);
        }

        /// <summary>
        /// THIS METHOD NOT REQUIRED WHEN CREATING TEMPLATE AUTOMATICALLY
        /// </summary>
        /// <param name="ar"></param>
        protected override void ExtractTemplateFromRecording(AudioRecording ar)
        {
            Log.WriteIfVerbose("START Template_CCAuto.ExtractTemplateFromRecording()");
        }


        protected void ExtractTemplateFromRecordings(FileInfo[] recordingFiles)
        {
            Log.WriteIfVerbose("\nSTART Template_CCAuto.ExtractTemplateFromRecordings()");
            FVExtractor.ExtractFVsFromVocalisations(recordingFiles, FeatureVectorConfig, SonogramConfig);
            FVExtractor.ExtractSymbolSequencesFromVocalisations(recordingFiles, this);

            Log.WriteIfVerbose("END   Template_CCAuto.ExtractTemplateFromRecordings()\n");
        }

        protected void CreateLanguageModel(Configuration config)
        {
            //DEAL WITH THE VOCALISATION MODEL TYPE
            //set up the config file using info obtained from feature extraction
            config.SetPair(ConfigKeys.Template.Key_FVCount, this.FeatureVectorConfig.FVCount.ToString());

            int wordCount = config.GetInt(ConfigKeys.Template.Key_WordCount); //number of distinct songs or calls
            this.WordNames = new string[wordCount];
            for (int i = 0; i < wordCount; i++)
            {
                this.WordNames[i] = config.GetString("WORD" + (i + 1) + "_NAME");
                //Log.WriteIfVerbose("WORD" + (i + 1) + "_NAME=" + this.WordNames[i]);
            }

            int exampleCount = this.WordExamples.Length;

            for (int i = 0; i < exampleCount; i++)
            {
                config.SetPair("WORD" + wordCount + "_EXAMPLE" + (i+1), this.WordExamples[i]);
            }

            //initialise the language model with config
            var modelName = config.GetString(ConfigKeys.Template.Key_ModelType);
            LanguageModelType modelType = (LanguageModelType)Enum.Parse(typeof(LanguageModelType), modelName);
            this.Modeltype = modelType;
            if (modelType == LanguageModelType.UNDEFINED) LanguageModel = new Model_Undefined();
            else if (modelType == LanguageModelType.ONE_PERIODIC_SYLLABLE) LanguageModel = new Model_OnePeriodicSyllable(config);
            else if (modelType == LanguageModelType.MM_TWO_STATE_PERIODIC) LanguageModel = new Model_2StatePeriodic(config);
            else if (modelType == LanguageModelType.MM_ERGODIC) LanguageModel = new Model_MMErgodic(config);
        }
        
        public override void Save(string targetPath)
		{
            Log.WriteIfVerbose("START Template_CCAuto.Save(targetPath=" + targetPath + ")");
            this.DataPath = targetPath;
            string opDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(opDir)) Directory.CreateDirectory(opDir);

            if (File.Exists(targetPath)) File.Copy(targetPath, targetPath + "OLD.txt", true); //overwrite
  
            Save(new StreamWriter(targetPath), opDir);
		}


		public void Save(TextWriter writer, string opDir)
		{
			//throw new NotImplementedException("MMTemplate requires the path to be saved to. Use the Save(string) overload instead");
            writer.WriteLine("DATE=" + DateTime.Now.ToString("u"));  //u format=2008-11-05 14:40:28Z
            writer.WriteConfigValue("AUTHOR", AuthorName);
            writer.WriteLine("#");
            writer.WriteLine("#**************** TEMPLATE DATA");
            writer.WriteConfigValue("TEMPLATE_ID", CallID);
            writer.WriteConfigValue("CALL_NAME", CallName); //CALL_NAME=Lewin's Rail Kek-kek
            writer.WriteConfigValue("COMMENT", Comment);    //COMMENT=Template consists of a single KEK!
            writer.WriteConfigValue("THIS_FILE", DataPath);   //THIS_FILE=C:\SensorNetworks\Templates\Template_2\template_2.ini
            writer.WriteLine("#");
            writer.WriteLine("#**************** INFO ABOUT ORIGINAL .WAV FILE[s]");
            writer.WriteConfigValue(ConfigKeys.Recording.Key_TrainingDirName, this.TrainingDirName);
            writer.WriteConfigValue(ConfigKeys.Recording.Key_TestingDirName,  this.TestingDirName);
            writer.WriteConfigValue(ConfigKeys.Windowing.Key_SampleRate, SonogramConfig.FftConfig.SampleRate);
            writer.WriteLine("#");
            writer.Flush();

            SonogramConfig.Save(writer);
            //FftConfiguration.Save(writer); //do not print here because printed by FeatureVectorConfig
            FeatureVectorConfig.SaveConfigAndFeatureVectors(writer, opDir, this);
            AcousticModel.Save(writer);
            LanguageModel.Save(writer);
            writer.Flush();
        }

        public override void SaveResultsImage(SpectralSonogram sonogram, string imagePath, BaseResult result)
        {
            Log.WriteIfVerbose("Template_CCAuto.SaveResultsImage(SpectralSonogram sonogram, string imagePath, Results result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, result.MaxDisplayScore, result.DisplayThreshold));
            image.Save(imagePath);
        }





        //**********************************************************************************************************************
        //**********************************************************************************************************************
        //**********************************************************************************************************************
        //USE THE NEXT THREE METHODS TO DISPLAY RESULTS FROM ALFREDO's HMM
        public void SaveResultsImage(WavReader wav, string imagePath, BaseResult result, List<string> hmmResults)
        {
            this.SonogramConfig.DoFullBandwidth = true;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav);
            SaveResultsImage(spectralSono, imagePath, result, hmmResults);
        }

        public void SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result, List<string> hmmResults)
        {
            Log.WriteIfVerbose("Basetemplate.SaveResultsImage(SpectralSonogram sonogram, string path, BaseResult result, List<string> hmmResults)");
            double[] hmmScores = ParseHmmScores(hmmResults, this.SonogramConfig.Duration, sonogram.FrameCount);

            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, 8.0, 1.0));
            image.AddTrack(Image_Track.GetScoreTrack(hmmScores, 8.0, 1.0));
            image.Save(path);
        }
        public double[] ParseHmmScores(List<string> results, TimeSpan duration, int frameCount)
        {
            //Console.WriteLine("duration.TotalSeconds=" + duration.TotalSeconds);
            double[] scores = new double[frameCount];
            int hitCount = results.Count;
            for (int i = 1; i < hitCount; i++)
            {
                string[] words = results[i].Split(' ');
                long start = long.Parse(words[0]);
                double startSec = start / (double)10000000;  //start in seconds
                long end = long.Parse(words[1]);
                double endSec = end / (double)10000000;  //start in seconds
                string className = words[2];
                double score = Double.Parse(words[3]);
                int startFrame = (int)((startSec / (double)duration.TotalSeconds) * frameCount);
                int endFrame = (int)((endSec / (double)duration.TotalSeconds) * frameCount);
                //Console.WriteLine("startSec=" + startSec + "    endSec=" + endSec + "  startFrame=" + startFrame + "    endFrame=" + endFrame);
                if (className.StartsWith("CURRAWONG"))
                    for (int s = startFrame; s <= endFrame; s++)
                    {
                        scores[s] = 5.0;
                    }
            }
            return scores;
        }

    } // end class Template_CCAuto
}
