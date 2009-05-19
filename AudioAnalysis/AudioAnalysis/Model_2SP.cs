using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MarkovModels;
using TowseyLib;


namespace AudioAnalysis
{

    /// <summary>
    /// ######################################### WARNING ##################################################
    /// WARNING!! This Model and associated Markov Model calss have not been debugged
    /// </summary>
    [Serializable]
    public class Model_2StatePeriodic : BaseModel
    {

        #region Properties
        public MM_Base markovModel { get; set; }
        public int Gap_ms { get; set; }
       
        #endregion



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="config"></param>
        public Model_2StatePeriodic(Configuration config)
        {
            Log.WriteIfVerbose("INIT Model_2StatePeriodic CONSTRUCTOR 1");
            this.ModelType = ModelType.MM_TWO_STATE_PERIODIC;
            SetFrameOffset(config); //set sample rate, frame duration etc

            // READ TRAINING SEQUENCES
            WordCount = config.GetInt("NUMBER_OF_WORDS"); //number of defined song variations 
            Gap_ms = config.GetInt("GAP_MS");
            TrainingSet ts = GetTrainingSet(config);
            //Words = GetSequences(config);

            //one markov model per template
            int fvCount = config.GetInt("FV_COUNT");
            if (fvCount != 1)
                throw new Exception("WARNING! Must only have one Feature Vector to init this MM");
            markovModel = new MM_2State_Periodic(Gap_ms, this.FrameOffset);
            //markovModel.TrainModel(ts);
            //SongWindow = config.GetDoubleNullable("SONG_WINDOW") ?? 1.0;
        }// end of Constructor 1




        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="config"></param>
        /// <param name="fvCount"></param>
        /// <param name="sonogramConfig"></param>
        /// <param name="sampleRate"></param>
        public Model_2StatePeriodic(Configuration config, int fvCount, SonogramConfig sonogramConfig, int sampleRate)
        {
            Log.WriteIfVerbose("INIT Model_2StatePeriodic CONSTRUCTOR 2");
            this.ModelType = ModelType.MM_TWO_STATE_PERIODIC;
            WordCount = config.GetInt("NUMBER_OF_WORDS"); //number of defined song variations 
            Gap_ms = config.GetInt("GAP_MS");

            //config.SetPair("WORD1_NAME", "word");   //default value for template creation
            //config.SetPair("WORD1_EXAMPLE1", "1");  //default value for template creation
            SetFrameOffset(config);


            // READ TRAINING SEQUENCES
            WordCount = config.GetInt("NUMBER_OF_WORDS");
            if (WordCount < 1)
                throw new ArgumentException("Configuration file is invalid - No words defined in language model.");

            TrainingSet ts = new TrainingSet();
            for (int n = 0; n < WordCount; n++)
            {
                string name = config.GetString("WORD" + (n + 1) + "_NAME");
                for (int w = 0; w < 100; w++) // do not allow more than 100 examples
                {
                    string word = config.GetString("WORD" + (n + 1) + "_EXAMPLE" + (w + 1));
                    if (word == null)
                        break;
                    ts.AddSequence(name ?? "WORD" + (n + 1), word);
                }

            } // end for loop over all words
            Words = ts.GetSequences();

            //MM_Base mm;
            //    int? gap_ms = config.GetIntNullable("GAP_MS");
            //    if (gap_ms == null)
            //        throw new ArgumentException("Configuration file is invalid - two state MM cannot be defined because gap duration is not definied in configuration.");
            //    mm = new MM_2State_Periodic(mmName, gap_ms.Value, FrameOffset); //special constructor for two state periodic MM 
            //    mm.TrainModel(ts);
            //WordModel = mm; //one markov model per template

            SongWindow = config.GetDoubleNullable("SONG_WINDOW") ?? 1.0;
        }// end of Constructor 2


        #region ScanSymbolSequenceWithMM and associates

        public override void ScanSymbolSequenceWithModel(BaseResult r, double frameOffset)
        {
            Log.WriteIfVerbose("\nSTART Model_MM2SP.ScanSymbolSequenceWithModel()");

            var result = r as Result_1PS; //###########################################CHANGE TO RESULT_2SP WHEN AVAILABLE

            double[,] acousticMatrix = result.AcousticMatrix;
            string symbolSequence = result.SyllSymbols;
            //int[] integerSequence = result.SyllableIDs;
            int frameCount = symbolSequence.Length;

            //markovModel.WriteInfo(false);
//            MMResults mmResults = markovModel.ScoreSequence(symbolSequence);
            // out scores, out hitCount, out bestHit, out bestLocation
            //double[] scores = null;
            //int hitCount;
            //double bestHit;
            //int bestLocation;
//            result.VocalScores = mmResults.Scores;
//            result.VocalCount = mmResults.HitCount;
//            result.VocalBest = mmResults.BestHit;
//            result.VocalBestLocation = (double)mmResults.BestLocation * frameOffset;

//                Log.WriteLine("#### VocalCount={0} VocalBest={1} bestFrame={2:F3} @ {3:F1}s", hitCount, bestHit, bestLocation, result.VocalBestLocation);
        }

        #endregion


        public override void Save(TextWriter writer)
        {
            Log.WriteIfVerbose("START Model_2StatePeriodic.Save()");

            writer.WriteLine("#**************** INFO ABOUT THE LANGUAGE MODEL ***************");
            writer.WriteConfigValue("MODEL_TYPE", ModelType);
                //writer.WriteConfigValue("MM_NAME", WordModel.Name);
            writer.WriteConfigValue("GAP_MS", Gap_ms);
            
            writer.WriteConfigValue("NUMBER_OF_WORDS", WordCount);
            // Although when read in the Words are split into different tags with multiple examples this information
            // is not stored (or used) so we can not persist it back. Instead we just write as if each word
            // is separate with 1 example each
            //writer.WriteConfigArray("WORD{0}_EXAMPLE1", Words);
            writer.WriteConfigValue("SONG_WINDOW", SongWindow);
            writer.WriteLine("#");
            writer.Flush();
            Log.WriteIfVerbose("END Model_2StatePeriodic.Save()");
        }


    } //end class Model_2StatePeriodic
}
