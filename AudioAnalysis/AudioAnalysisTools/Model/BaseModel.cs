using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using MarkovModels;


namespace AudioAnalysisTools
{
    public enum LanguageModelType { UNDEFINED, ONE_PERIODIC_SYLLABLE, MM_ERGODIC, MM_TWO_STATE_PERIODIC }

	[Serializable]
    public abstract class BaseModel
    {
        #region Static Variables
        public static string opFolder { get; set; } //only used when storing unit testing results during app testing.
        #endregion

        #region Properties
        private LanguageModelType modeltype = LanguageModelType.UNDEFINED;
        public  LanguageModelType ModelType { get { return modeltype; } set { modeltype = value; } }
        public  string modelName { get { return ModelType.ToString(); } }

        public int WordCount { get; set; }
        public string[] WordNames { get; set; }
        public string[] WordExamples { get; set; }
        public double SongWindow { get; set; } //window duration in seconds - used to calculate statistics

        public double FrameOffset { get; private set; } //Time in seconds between commencement of consecutive frames
        public double FramesPerSecond { get { return 1 / FrameOffset; } }
        #endregion


        public BaseModel Load(Configuration config)
        {
            string modelName = config.GetString("MODEL_TYPE");
            LanguageModelType type = (LanguageModelType)Enum.Parse(typeof(LanguageModelType), modelName);

            if (type == LanguageModelType.UNDEFINED)
                return (new Model_Undefined());
            else
                if (type == LanguageModelType.ONE_PERIODIC_SYLLABLE)
                    return (new Model_OnePeriodicSyllable(config));
                else
                    if (type == LanguageModelType.MM_TWO_STATE_PERIODIC)
                        return (new Model_2StatePeriodic(config));
                    else
                        if (type == LanguageModelType.MM_ERGODIC)
                            return (new Model_MMErgodic(config));
            Log.WriteLine("BaseModel Load(): WARNING!! No model was defined.");
            return null;
        }

        protected void SetFrameOffset(Configuration config)
        {
            int sr = config.GetInt(ConfigKeys.Windowing.Key_SampleRate);
            int frameSize = config.GetInt(ConfigKeys.Windowing.Key_WindowSize);
            double frameOverlap = config.GetDouble(ConfigKeys.Windowing.Key_WindowOverlap);
            double frameDuration = frameSize / (double)sr; // Duration of full frame or window in seconds
            this.FrameOffset = frameDuration * (1 - frameOverlap); // Duration of non-overlapped part of window/frame in seconds
        }

        public static string[] GetSequences(Configuration config)
        {
            TrainingSet ts = GetTrainingSet(config);
            return ts.GetSequences();
        }

        public static TrainingSet GetTrainingSet(Configuration config)
        {
            int count = config.GetInt("NUMBER_OF_WORDS");
            if (count < 1)
                throw new ArgumentException("Configuration file is invalid - No words defined for language model.");

            Log.WriteIfVerbose("\tReading Training Set of " + count + " word(s).");
            TrainingSet ts = new TrainingSet();
            for (int n = 0; n < count; n++)
            {
                string name = config.GetString("WORD" + (n + 1) + "_NAME");
                if (name == null)
                    throw new ArgumentException("Configuration file is invalid - WORD" + (n + 1) + "_NAME is not defined.");
                Log.WriteIfVerbose("\tAdd examples for <" + name + ">");
                for (int w = 0; w < 100; w++) // do not allow more than 100 examples
                {
                    string word = config.GetString("WORD" + (n + 1) + "_EXAMPLE" + (w + 1));
                    if (word == null)
                        break;
                    ts.AddSequence(name ?? "WORD" + (n + 1), word);
                }

            } // end for loop over all words
            return ts;
        }



        public abstract void ScanSymbolSequenceWithModel(BaseResult result, double frameOffset);

        public abstract void Save(TextWriter writer);

             

    } //end class BaseModel


    public class Model_Undefined : BaseModel
    {
        public Model_Undefined()
        {
            Log.WriteIfVerbose("INIT LanguageModel Model_Undefined CONSTRUCTOR 1");
            this.ModelType = LanguageModelType.UNDEFINED;
            WordCount = 0; 
            WordExamples = null;
            SongWindow = 0.0;
        }

        public override void Save(TextWriter writer)
        {
            Log.WriteIfVerbose("START Model_Undefined.Save()");

            writer.WriteLine("#**************** INFO ABOUT THE LANGUAGE MODEL ***************");
            writer.WriteConfigValue("MODEL_TYPE", ModelType);
            writer.WriteConfigValue("NUMBER_OF_WORDS", 0);
            writer.WriteConfigValue("SONG_WINDOW", 0.0);
            writer.WriteLine("#");
            writer.Flush();
            Log.WriteIfVerbose("END Model_Undefined.Save()");
        }

        public override void ScanSymbolSequenceWithModel(BaseResult result, double frameOffset)
        {
            Log.WriteIfVerbose("Model_Undefined.ScanSymbolSequenceWithModel(): This method should not be called!!");
        }

    } //end class Model_Undefined

}
