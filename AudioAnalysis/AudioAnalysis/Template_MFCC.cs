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
	public class Template_MFCC : BaseTemplate
	{

        public string NoiseFVPath { get; private set; }


		public Template_MFCC(Configuration config) : base(config)
		{
            SonogramConfig = new AVSonogramConfig(config);
            EndpointDetectionConfiguration.SetEndpointDetectionParams(config);
            FeatureVectorConfig   = new FVConfig(config);
            AcousticModelConfig   = new AcousticModel(config);
            if (config.Source != null)
                NoiseFVPath = Path.Combine(Path.GetDirectoryName(config.Source), Path.GetFileNameWithoutExtension(config.Source) + "_NoiseFV.txt");

            var modelName = config.GetString("MODEL_TYPE");
            ModelType modelType = (ModelType)Enum.Parse(typeof(ModelType), modelName);
            this.Modeltype = modelType;

            //do not init a Model if task is only to extract FVs and create an Acoustic Model.
            if ((BaseTemplate.task == Task.CREATE_ACOUSTIC_MODEL) || (BaseTemplate.task == Task.EXTRACT_FV))
            {
                //Model = new Model_Undefined();
                return;
            }

            if (modelType == ModelType.UNDEFINED) Model = new Model_Undefined();
            else if (modelType == ModelType.ONE_PERIODIC_SYLLABLE) Model = new Model_OnePeriodicSyllable(config);
            else if (modelType == ModelType.MM_TWO_STATE_PERIODIC) Model = new Model_2StatePeriodic(config);
            else if (modelType == ModelType.MM_ERGODIC)            Model = new Model_MMErgodic(config);

        }

		public override void Save(string targetPath)
		{
            Log.WriteIfVerbose("START MMTemplate.Save(targetPath=" + targetPath + ")");
            this.OPPath = targetPath;
            string opDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(opDir)) Directory.CreateDirectory(opDir);

            if (File.Exists(targetPath)) File.Copy(targetPath, targetPath + ".OLD", true); //overwrite
  
            Save(new StreamWriter(targetPath), opDir);
		}


		public void Save(TextWriter writer, string opDir)
		{
			//throw new NotImplementedException("MMTemplate requires the path to be saved to. Use the Save(string) overload instead");

            base.Save(writer);
            SonogramConfig.Save(writer);
            FeatureVectorConfig.SaveConfigAndFeatureVectors(writer, opDir);
            AcousticModelConfig.Save(writer);

            //write the default language model if only creating a new template
            if ((BaseTemplate.task == Task.EXTRACT_FV) || (BaseTemplate.task == Task.CREATE_ACOUSTIC_MODEL))
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

        public override void SaveResultsImage(WavReader wav, string imagePath, BaseResult result)
        {
            bool doExtractSubband = false;
            var spectralSono = new SpectralSonogram(this.SonogramConfig, wav, doExtractSubband);
            SaveResultsImage(spectralSono, imagePath, result);
        }

        public override void SaveResultsImage(SpectralSonogram sonogram, string imagePath, BaseResult result)
        {
            Log.WriteIfVerbose("Template_MFCC.SaveResultsImage(SpectralSonogram sonogram, string imagePath, Results result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModelConfig.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModelConfig.SyllableIDs, garbageID));
            double? scoreMax  = ((Results)result).MaxScore;
            double? threhsold = ((Results)result).LLRThreshold;
            image.AddTrack(Image_Track.GetScoreTrack(((Results)result).VocalScores, scoreMax, threhsold));
            image.Save(imagePath);
        }

    } // end of class MMTemplate : TemplateParameters

}