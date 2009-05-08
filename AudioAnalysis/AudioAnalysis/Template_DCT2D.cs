using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using MarkovModels;
using AudioTools;
using QutSensors.Data;


namespace AudioAnalysis
{
    class Template_DCT2D : BaseTemplate
    {
        public Template_DCT2D(Configuration config) : base(config)
		{
            SonogramConfig = new AVSonogramConfig(config);
            EndpointDetectionConfiguration.SetEndpointDetectionParams(config);
            FeatureVectorConfig   = new FVConfig(config);
            AcousticModelConfig   = new AcousticModel(config);

            //DEAL WITH THE VOCALISATION MODEL TYPE
            var modelName = config.GetString("MODEL_TYPE");
            ModelType modelType = (ModelType)Enum.Parse(typeof(ModelType), modelName);
            this.Modeltype = modelType;

            //do not init a Model if in create new template mode.
            if (this.mode == Mode.CREATE_NEW_TEMPLATE) return;       

            if (modelType == ModelType.UNDEFINED) Model = new Model_Undefined();
            else if (modelType == ModelType.ONE_PERIODIC_SYLLABLE) Model = new Model_OnePeriodicSyllable(config);
            else if (modelType == ModelType.MM_TWO_STATE_PERIODIC) Model = new Model_2StatePeriodic(config);
            else if (modelType == ModelType.MM_ERGODIC)            Model = new Model_MMErgodic(config);
        }

        /// <summary>
        /// The call to static method FVExtractor.ExtractFVsFromRecording() results in the
        /// creation of an array of feature vectors each representing a portion of a vocalisation.
        /// </summary>
        /// <param name="ar"></param>
        protected override void ExtractTemplateFromRecording(AudioRecording ar)
        {
            if (Log.Verbosity == 1) Console.WriteLine("START Template_DCT2D.ExtractTemplateFromRecording()");
            string opDir = this.DataDir;
            double startTime = this.FeatureVectorConfig.StartTime;
            double endTime   = this.FeatureVectorConfig.EndTime;

            //save full length spectrogram image
            var ss = new SpectralSonogram(this.SonogramConfig, ar.GetWavReader());
            //var image = new Image_MultiTrack(ss.GetImage(false, false));
            string path = opDir + ar.FileName + ".png";
            //image.Save(path);
            Console.WriteLine("Full length of sonogram = "+ss.FrameCount+" frames = "+ss.Duration.TotalSeconds+" s");


            //extract portion of spectrogram
            var s2 = new SpectralSonogram(ss, startTime, endTime);
            var image = new Image_MultiTrack(s2.GetImage(false, false));
            string fName = ar.FileName + "_" + startTime.ToString("F0") + "s-" + endTime.ToString("F0") + "s";
            path = opDir + fName + ".png";
            image.Save(path);

            //extract and save part of wav file
            AudioRecording ar2 = ar.ExportSignal(startTime, endTime);            
            path = opDir + fName + ".wav";
            ar2.Save(path);

            //save extracted portion of wav file as spectrogram image - to compare with preivous verison
            //ss = new SpectralSonogram(this.SonogramConfig, ar2.GetWavReader());
            //image = new Image_MultiTrack(ss.GetImage(false, false));
            //path = opDir + fName + "alternative.png";
            //image.Save(path);

            FVExtractor.Extract2D_DCTFromMarquee(s2, FeatureVectorConfig);
        }

		public override void Save(string targetPath)
		{
            Log.WriteIfVerbose("START Template_DCT2D.Save(targetPath=" + targetPath + ")");
            this.DataPath = targetPath;
            string opDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(opDir)) Directory.CreateDirectory(opDir);

            if (File.Exists(targetPath)) File.Copy(targetPath, targetPath + "OLD.txt", true); //overwrite
  
            Save(new StreamWriter(targetPath), opDir);
		}


		public void Save(TextWriter writer, string opDir)
		{
            base.Save(writer);
            //FftConfiguration.Save(writer); //do not print here because printed by FeatureVectorConfig
            SonogramConfig.Save(writer);
            FeatureVectorConfig.SaveConfigAndFeatureVectors(writer, opDir, this);
            AcousticModelConfig.Save(writer);

            //write the default language model if only creating a new template
            if (this.mode == Mode.CREATE_NEW_TEMPLATE)
            {
                writer.WriteLine("#**************** INFO ABOUT THE LANGUAGE MODEL ***************");
                writer.WriteLine("#Options: UNDEFINED, ONE_PERIODIC_SYLLABLE, MM_ERGODIC, MM_TWO_STATE_PERIODIC");
                writer.WriteLine("MODEL_TYPE=UNDEFINED");
                writer.WriteLine("#MODEL_TYPE=" + this.Modeltype);
                writer.WriteLine("NUMBER_OF_WORDS=1");
                writer.WriteLine("WORD1_NAME=Dummy");
                writer.WriteLine("WORD1_EXAMPLE1=1234");
                writer.WriteLine("WORD1_EXAMPLE2=5678");
                writer.WriteLine("#");
                writer.Flush();
            }
            else Model.Save(writer);
        }

        public override void SaveResultsImage(SpectralSonogram sonogram, string imagePath, BaseResult result)
        {
            Log.WriteIfVerbose("Template_DCT2D.SaveResultsImage(SpectralSonogram sonogram, string imagePath, Results result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModelConfig.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModelConfig.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, result.MaxDisplayScore, result.DisplayThreshold));
            image.Save(imagePath);
        }


    }//end class
}
