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
            Log.WriteIfVerbose("\nINIT LANGUAGE MODEL Model_MMErgodic CONSTRUCTOR 1");
            this.ModelType = LanguageModelType.MM_ERGODIC;
            SetFrameOffset(config); //set sample rate, frame duration etc

            // READ TRAINING SEQUENCES
            this.WordCount = config.GetInt(ConfigKeys.Template.Key_WordCount); //number of different 
            this.WordNames = new String[this.WordCount];
            for (int i = 0; i < this.WordCount; i++)
                this.WordNames[i] = config.GetString("WORD" + (i + 1) + "_NAME");  

            TrainingSet ts = GetTrainingSet(config);
            int exampleCount = ts.Count;
            this.WordExamples = new string[exampleCount];
            for (int i = 0; i < exampleCount; i++) //assume only have examples for one word
                this.WordExamples[i] = config.GetString("WORD1_EXAMPLE"+ (i + 1));  

            //one markov model per template
            int fvCount = config.GetInt(ConfigKeys.Template.Key_FVCount);
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
        ///> The null hypothesis is rejected means that the test sequence is NOT recognised by the model.
        /// The null hypothesis is rejected if  LLR < -6.64.
        ///
        ///> For display purposes, we must shift the display into positive territory.
        ///  In addition, a quality score is calculated. If the quality score fails to exceed a threshold, then the
        ///  display score is set to zero.
        ///  The final score displayed in image is shifted to the range 0-10.
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="frameOffset"></param>
        public override void ScanSymbolSequenceWithModel(BaseResult r, double frameOffset)
        {
            Log.WriteIfVerbose("\nSTART Model_MMErgodic.ScanSymbolSequenceWithModel()");
            var result = r as Result_MMErgodic; //caste to appropriate result type.

            List<string> unitTestMonitor = new List<string>(); // only used when Unit testing

            //double[,] acousticMatrix = result.AcousticMatrix; //not used for this model
            string symbolSequence = result.SyllSymbols;
            int frameCount = symbolSequence.Length;
            
            //obtain list of partial vocalisations that have been detected by the MM
            MMResults mmResults = markovModel.ScoreSequence(symbolSequence);
            List<Vocalisation> list = mmResults.PartialVocalisations; //each vocalisation represented as symbol string and scores
            int listLength = list.Count;

            //result.PartialVocalisations = list;

            //ANALYSE THE MM RESULTS FOR EACH VOCALISATION - CALCULATE LLR etc
            //init the results variables
            result.MaxDisplayScore = 10.0;  //a suitable value for the expected range of LLR and Quality Scores
            double bestHit = -Double.MaxValue;
            int bestFrame  = -1;
            double[] scores = new double[frameCount];

            for (int i = 0; i < listLength; i++) //
            //for (int i = 0; i < 50; i++) //
                {
                Vocalisation vocalEvent = list[i];
                int[] array = MMTools.String2IntegerArray('n' + vocalEvent.SymbolSequence + 'n');

                //song duration filter
                double durationProb = vocalEvent.DurationProbability;
                //Log.WriteIfVerbose((i + 1).ToString("D2") + " Prob(Song duration) = " + durationProb.ToString("F3"));
                unitTestMonitor.Add((i + 1).ToString("D2") + " Prob(Song duration) = " + durationProb.ToString("F3"));

                double llr = vocalEvent.Score - mmResults.probOfAverageTrainingSequenceGivenModel;

                //now SCALE THE SCORE ARRAY so that it can be displayed in range +0 to +10.
                double? displayScore = llr + vocalEvent.QualityScore;
                displayScore += result.MaxDisplayScore; //add positve value because max score expected to be zero.
                if (displayScore > result.MaxDisplayScore) displayScore = result.MaxDisplayScore;
                if (displayScore < 0) displayScore = 0.0;
                if (vocalEvent.QualityScore < mmResults.qualityThreshold) displayScore = 0.0;
                scores[vocalEvent.Start] = (double)displayScore; //assign score to beginning of vocalisation event

                if (displayScore > bestHit)
                {
                    bestHit = (double)displayScore;
                    bestFrame = vocalEvent.Start;
                }
                //Log.WriteIfVerbose((i + 1).ToString("D2") + " LLR=" + llr.ToString("F2") + " \tQual=" + vocalEvent.QualityScore.ToString("F2") + "\t" + vocalEvent.Sequence);
                unitTestMonitor.Add((i + 1).ToString("D2") + " LLRScore=" + llr.ToString("F2") + "\t" + vocalEvent.SymbolSequence);
            }//end of scanning all vocalisations


            //LLR_THRESHOLD = Chi2 statistic for DOF=1 and alpha=0.01
            result.LLRThreshold = result.MaxDisplayScore - LLR_THRESHOLD; //hit threshold
            //smooth the score array because want to count acoustic events which exceed threshold
            result.Scores           = DataTools.filterMovingAverage(scores, 5); //smooth the score array
            result.DisplayThreshold = result.LLRThreshold;

            //summary scores
            int hitCount = GetHitCount(result.Scores, (double)result.LLRThreshold);
            result.VocalCount = hitCount;        // number of detected vocalisations
            result.RankingScoreValue= bestHit;   // the highest score obtained in recording
            result.FrameWithMaxScore= bestFrame;
            double bestTimePoint = (double)bestFrame * frameOffset;
            result.TimeOfMaxScore = bestTimePoint;

            string str1 = String.Format("\n#### Number of calls recognised={0}", hitCount);
            if (hitCount > 0) str1 = String.Format("\n#### Number of calls recognised={0}. Highest score={1:F3} at frame {2:D} = {3:F1}s.",
                                                                                hitCount, bestHit, bestFrame, bestTimePoint);
            Log.WriteIfVerbose(str1);
            unitTestMonitor.Add(str1);

            if (BaseTemplate.InTestMode)
            {
                string path = BaseModel.opFolder + "\\markovModelParams.txt";
                FileTools.WriteTextFile(path, unitTestMonitor);
                Log.WriteLine("COMPARE FILES OF INTERMEDIATE MARKOV MODEL PARAMETERS");
                FunctionalTests.AssertAreEqual(new FileInfo(path), new FileInfo(path + "OLD.txt"), true);
            } //end TEST MODE


            Log.WriteIfVerbose("END Model_MMErgodic.ScanSymbolSequenceWithModel()");
        } //end ScanSymbolSequenceWithModel()
        #endregion

        /// <summary>
        /// returns the number of time the score value transits the threshold
        /// Use this as estimate of number of vocalisations detected in recording.
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private int GetHitCount(double[] scores, double threshold)
        {
            //Console.WriteLine("threshold=" + threshold);
            int hits = 0;
            for (int i = 1; i < scores.Length; i++) 
            {
                if ((scores[i-1] < threshold) && (scores[i] >= threshold)) hits++;
                //if ((scores[i - 1] < threshold) && (scores[i] >= threshold)) Console.WriteLine(scores[i - 1] + ">>" + scores[i]);
            }
            return hits;
        }


        public override void Save(TextWriter writer)
        {
            Log.WriteIfVerbose("START Model_MMErgodic.Save()");

            writer.WriteLine("#**************** INFO ABOUT THE LANGUAGE MODEL ***************");
            writer.WriteLine("#Options: UNDEFINED, ONE_PERIODIC_SYLLABLE, MM_ERGODIC, MM_TWO_STATE_PERIODIC");
            writer.WriteLine("#MODEL_TYPE=UNDEFINED");
            writer.WriteConfigValue("MODEL_TYPE", ModelType);

            writer.WriteConfigValue("NUMBER_OF_WORDS", WordCount);
            for (int i = 0; i < WordCount; i++)
            {
                writer.WriteConfigValue("WORD" + (i + 1) + "_NAME", this.WordNames[i]);
            }

            double avLength = 0.0; //to determine average length of training vocalisations
            for (int i = 0; i < this.WordExamples.Length; i++) //assume only one word for moment
            {
                writer.WriteConfigValue("WORD1_EXAMPLE" + (i + 1), WordExamples[i]);
                avLength += WordExamples[i].Length;
            }
            avLength /= WordExamples.Length;
            writer.WriteLine("#AVERAGE LENGTH OF EXAMPLE CALLS = " + avLength.ToString("F1"));

            writer.WriteLine("#");
            writer.WriteConfigValue("SONG_WINDOW", SongWindow);
            writer.WriteLine("#");
            writer.Flush();
            Log.WriteIfVerbose("END Model_MMErgodic.Save()");
        }


    } // end of class Model_MMErgodic
}
