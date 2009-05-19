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
	public class Template_CC : BaseTemplate
	{

		public Template_CC(Configuration config) : base(config)
		{
            SonogramConfig = new CepstralSonogramConfig(config);
            EndpointDetectionConfiguration.SetEndpointDetectionParams(config);
            FeatureVectorConfig   = new FVConfig(config);
            AcousticModel   = new Acoustic_Model(config);

            var modelName = config.GetString("MODEL_TYPE");
            ModelType modelType = (ModelType)Enum.Parse(typeof(ModelType), modelName);
            this.Modeltype = modelType;

            //do not init a Model if in crete new template mode.
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
            FVExtractor.ExtractFVsFromRecording(ar, FeatureVectorConfig, SonogramConfig);
        }

		public override void Save(string targetPath)
		{
            Log.WriteIfVerbose("START Template_CC.Save(targetPath=" + targetPath + ")");
            this.DataPath = targetPath;
            string opDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(opDir)) Directory.CreateDirectory(opDir);

            if (File.Exists(targetPath)) File.Copy(targetPath, targetPath + "OLD.txt", true); //overwrite
  
            Save(new StreamWriter(targetPath), opDir);
		}


		public void Save(TextWriter writer, string opDir)
		{
			//throw new NotImplementedException("MMTemplate requires the path to be saved to. Use the Save(string) overload instead");
            base.Save(writer);
            //FftConfiguration.Save(writer); //do not print here because printed by FeatureVectorConfig
            SonogramConfig.Save(writer);
            FeatureVectorConfig.SaveConfigAndFeatureVectors(writer, opDir, this);
            AcousticModel.Save(writer);

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
            Log.WriteIfVerbose("Template_MFCC.SaveResultsImage(SpectralSonogram sonogram, string imagePath, Results result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, result.MaxDisplayScore, result.DisplayThreshold));
            image.Save(imagePath);
        }


//#######################################################################################################################################
        //USE THE NEXT THREE METHODS TO DISPLAY RESULTS FROM ALFREDO's HMM
        public void SaveResultsImage(WavReader wav, string imagePath, BaseResult result, List<string> hmmResults)
        {
            this.SonogramConfig.DisplayFullBandwidthImage = true;
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

        public static double[] ParseHmmScores(List<string> results, TimeSpan duration, int frameCount)
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

    } // end of class MMTemplate : TemplateParameters

}