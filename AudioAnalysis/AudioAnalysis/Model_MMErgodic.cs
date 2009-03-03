using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using MarkovModels;



namespace AudioAnalysis
{
    [Serializable]
    public class Model_MMErgodic : BaseModel
    {

        public const double LLR_THRESHOLD = 6.63; //Chi2 statistic for DOF=1 and alpha=0.01


        #region Properties
        public MMSD_Ergodic markovModel { get; set; }
       
        #endregion



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="config"></param>
        public Model_MMErgodic(Configuration config)
        {
            Log.WriteIfVerbose("INIT LanguageModel Model_MMErgodic CONSTRUCTOR 1");
            this.ModelType = ModelType.MM_ERGODIC;
            SetFrameOffset(config); //set sample rate, frame duration etc

            // READ TRAINING SEQUENCES
            WordCount = config.GetInt("NUMBER_OF_WORDS"); //number of defined song variations 
            TrainingSet ts = GetTrainingSet(config);
            //one markov model per template
            int fvCount = config.GetInt("FV_COUNT");
            int numberOfStates = fvCount + 1; //because need extra state for start and end noise
            markovModel = new MMSD_Ergodic(numberOfStates, this.FrameOffset);
            markovModel.TrainModel(ts);

        }// end of Constructor 1


        #region ScanSymbolSequenceWithMM and associates


        /// <summary>
        /// This method obtains a list of potential vocalisations from a markov model. 
        /// The valid vocalisations have been assigned a probability given the Markov Model (MM).
        /// The problem is (1) to assign a meaningful score and (2) a significance to that score.
        /// (1) The assigned score is a pseudo LLR as follows. 
        /// Let str1 be a test vocalisation and let str2 be an 'average' vocalisation used to train the MM.
        /// Let p1 = p(str1 | MM). Let p2 = p(str2 | MM).
        /// LLR = log (p1 /p2).
        /// Note that the LLR takes negative values because p1 < p2.
        ///>  Note also that LRR's max value = 0.0 (i.e. test str has same prob has training string)
        /// 
        /// (2) The significance of a standard LLR is obtained from a Chi2 table.
        /// Assuming 1 degree of freedom, to obtain alpha <= 0.01, the LLR must be >= 6.64.
        /// Note in this situation, we interpret as follows: 
        /// The null hypothesis is that the test sequence is no different from a training sequence.
        /// The null hypothesis is accepted if  -6.64 <= LLR <=0.
        ///>The null hypothesis is rejected means that the test sequence is NOT recognised by the model.
        /// The null hypothesis is rejected if  LLR < -6.64.
        ///
        ///> For display purposes, we must shift the display into positive territory.
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="frameOffset"></param>
        public override void ScanSymbolSequenceWithModel(Results result, double frameOffset)
        {
            Log.WriteIfVerbose("\nSTART Model_MMErgodic.ScanSymbolSequenceWithModel()");

            List<string> mmMonitor = new List<string>(); // only used when Unit testing

            double[,] acousticMatrix = result.AcousticMatrix;
            string symbolSequence = result.SyllSymbols;
            int frameCount = symbolSequence.Length;
            
            //obtain vocalisations that have been scored by the MM
            MMResults mmResults = markovModel.ScoreSequence(symbolSequence);

            //obtain the list of vocalisations represented as symbol strings
            List<Vocalisation> list = mmResults.VocalList;
            int listLength = list.Count;

            //ANALYSE THE MM RESULTS FOR EACH VOCALISATION - CALCULATE LLR etc
            //init the results variables
            int hitCount = list.Count;
            int correctDurationCount = 0;
            double bestHit = -Double.MaxValue;
            int bestFrame = -1;
            double maxDisplayScore = 10.0;
            double llrThreshold = LLR_THRESHOLD;
            double[] scores = new double[frameCount];

            for (int i = 0; i < listLength; i++) //
            {
                Vocalisation vocalEvent = list[i];
                //Log.WriteIfVerbose.WriteLine(i + " " + extract.Sequence);
                int[] array = MMTools.String2IntegerArray('n' + vocalEvent.Sequence + 'n');

                //song duration filter - skip vocalisations that are not of sensible length
                double durationProb = vocalEvent.DurationProbability;
                Log.WriteIfVerbose((i + 1).ToString("D2") + " Prob(Song duration) = " + durationProb.ToString("F3"));
                mmMonitor.Add((i + 1).ToString("D2") + " Prob(Song duration) = " + durationProb.ToString("F3"));
                if (! vocalEvent.IsCorrectDuration)
                {
                    Log.WriteIfVerbose("\tDuration probability for " + vocalEvent.Length + " frames is too low.");
                    mmMonitor.Add("\tDuration probability for " + vocalEvent.Length + " frames is too low.");
                    continue;
                }

                correctDurationCount++;
                //double score = DataTools.AntiLogBase10(logScore) / DataTools.AntiLogBase10(probOfAverageTrainingSequenceGivenModel);
                double llr = vocalEvent.Score - mmResults.probOfAverageTrainingSequenceGivenModel;

                //now SCALE THE SCORE ARRAY so that it can be displayed
                double displayScore = llr;
                result.MaxScore = maxDisplayScore;
                displayScore += maxDisplayScore; //add positve value because max score expected to be zero.
                if (displayScore > maxDisplayScore) displayScore = maxDisplayScore;
                if (displayScore < 0) displayScore = 0;
                for (int j = 0; j < vocalEvent.Length; j++) scores[vocalEvent.Start + j] = displayScore;

                if (llr > bestHit)
                {
                    bestHit = llr;
                    bestFrame = vocalEvent.Start;
                }
                Log.WriteIfVerbose((i + 1).ToString("D2") + " LLRScore=" + llr.ToString("F2") + "\t" + vocalEvent.Sequence);
                mmMonitor.Add((i + 1).ToString("D2") + " LLRScore=" + llr.ToString("F2") + "\t" + vocalEvent.Sequence);
            }//end of scanning all vocalisations

            double bestTimePoint = (double)bestFrame * frameOffset;
            string str1 = String.Format("\n#### VocalCount={0} VocalValid={1} VocalBest={2:F3} bestFrame={3:D} @ {4:F1}s",
                hitCount, correctDurationCount, bestHit, bestFrame, bestTimePoint);
            Log.WriteIfVerbose(str1);
            mmMonitor.Add(str1);
            result.LLRThreshold     = result.MaxScore - llrThreshold;  //display threshold
            result.VocalScores      = scores;
            result.VocalCount       = hitCount; // number of detected vocalisations
            result.VocalValid       = correctDurationCount;
            result.VocalBestScore   = bestHit;   // the highest score obtained over all vocalisations
            result.VocalBestFrame   = bestFrame;
            result.VocalBestLocation= bestTimePoint;


            if (BaseTemplate.InTestMode)
            {
                string path = BaseModel.opFolder + "\\markovModelParams.txt";
                FileTools.WriteTextFile(path, mmMonitor);
                Log.WriteLine("COMPARE FILES OF INTERMEDIATE PARAMETER VALUES");
                FunctionalTests.AssertAreEqual(new FileInfo(path), new FileInfo(path + ".OLD"), true);
            } //end TEST MODE


            Log.WriteIfVerbose("END Model_MMErgodic.ScanSymbolSequenceWithModel()");
        } //end ScanSymbolSequenceWithModel()
        #endregion


        public override void Save(TextWriter writer)
        {
            Log.WriteIfVerbose("START Model_MMErgodic.Save()");

            writer.WriteLine("#**************** INFO ABOUT THE LANGUAGE MODEL ***************");
            writer.WriteLine("#Options: UNDEFINED, ONE_PERIODIC_SYLLABLE, MM_ERGODIC, MM_TWO_STATE_PERIODIC");
            writer.WriteConfigValue("MODEL_TYPE", ModelType);

            writer.WriteConfigValue("NUMBER_OF_WORDS", WordCount);
            // Although when read in the Words are split into different tags with multiple examples this information
            // is not stored (or used) so we can not persist it back. Instead we just write as if each word
            // is separate with 1 example each

            for (int i = 0; i < WordCount; i++)
            {
                writer.WriteConfigArray("WORD{0}_EXAMPLE1", Words);
            }
            writer.WriteLine("#");
            writer.WriteConfigValue("SONG_WINDOW", SongWindow);
            writer.WriteLine("#");
            writer.Flush();
            Log.WriteIfVerbose("END Model_MMErgodic.Save()");
        }


    } // end of class Model_MMErgodic
}
