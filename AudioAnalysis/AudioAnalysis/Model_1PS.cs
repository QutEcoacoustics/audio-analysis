using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using MarkovModels;
using TowseyLib;

namespace AudioAnalysis
{



    public class Model_OnePeriodicSyllable : BaseModel
    {

        private static double fractionalNH = 0.30; //arbitrary neighbourhood around user defined periodicity

        #region Properties

        public int Periodicity_ms { get; private set; }
        public int Periodicity_frames { get; private set; }
        public int Periodicity_NH_frames  { get; private set; }
        public int Periodicity_NH_ms  { get; private set; }

        public double ZScoreThreshold { get; set; }
       
        #endregion



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="config"></param>
        public Model_OnePeriodicSyllable(Configuration config)
        {
            Log.WriteIfVerbose("INIT Model_OnePeriodicSyllable: CONSTRUCTOR 1");
            this.ModelType = ModelType.ONE_PERIODIC_SYLLABLE;
            //modelName = config.GetString("MODEL_TYPE");
            //ModelType modelType = (ModelType)Enum.Parse(typeof(ModelType), modelName);
            WordCount = config.GetInt("NUMBER_OF_WORDS"); //number of defined song variations 
            SetFrameOffset(config);

            Periodicity_ms = config.GetInt("PERIODICITY_MS");
            SetPeriodicityParameters();
            ZScoreThreshold = config.GetDouble("ZSCORE_THRESHOLD");
            SongWindow = config.GetDoubleNullable("SONG_WINDOW") ?? 1.0;

            // READ TRAINING SEQUENCES
            WordCount = config.GetInt("NUMBER_OF_WORDS");
            Words = GetSequences(config);
        }// end of Constructor 1





        #region ScanSymbolSequenceWithModel and associates
        public override void ScanSymbolSequenceWithModel(Results result, double frameOffset)
        {
            Log.WriteIfVerbose("\nSTART Model_1PeriodicSyllable.ScanSymbolSequenceWithModel()");
            double[,] acousticMatrix = result.AcousticMatrix;
            string symbolSequence = result.SyllSymbols;
            int[] integerSequence = result.SyllableIDs;
            int frameCount = integerSequence.Length;

            //##################### PARSE SYMBOL STREAM USING MARKOV MODELS
            //int clusterWindow = (int)Math.Floor(SongWindow * (1 / frameOffset));
            
            double[] scores = AcousticModel.WordSearch(symbolSequence, acousticMatrix, Words);
            result.VocalScores = scores;
            result.VocalCount = DataTools.CountPositives(scores);
            //Console.WriteLine("VocalCOUNT=" + ((MMResult)result).VocalCount);
            //for (int i = 0; i < result.VocalScores.Length; i++) 
            //   if (result.VocalScores[i] != 0.0) Console.WriteLine(i + "  " + result.VocalScores[i]);
            if (result.VocalCount <= 1)
                return; // Cannot do anything more in this case

            //find peaks and process them
            bool[] peaks = DataTools.GetPeaks(scores);
            peaks = Model_OnePeriodicSyllable.RemoveSubThresholdPeaks(scores, peaks, ZScoreThreshold);
            scores = Model_OnePeriodicSyllable.ReconstituteScores(scores, peaks);

            //transfer score results so far to result object
            result.VocalScores = scores;
            result.VocalCount = DataTools.CountPositives(scores);
            int maxIndex = DataTools.GetMaxIndex(scores);
            result.VocalBestScore = scores[maxIndex];
            result.VocalBestLocation = (double)maxIndex * frameOffset;


            if ((result.VocalCount < 2) || (Periodicity_ms <= 0))
            {
                Log.WriteLine("### Model_1PeriodicSyllable.ScanSymbolSequenceWithModel(): WARNING!!!!  PERIODICITY CANNOT BE ANALYSED.");
                Log.WriteLine("                                                           WARNING!!!!  Hit Count < 2 = " + result.VocalCount);
                return;
            }

            //finally do periodicity analysis
            result.CallPeriodicity_ms = this.Periodicity_ms;
            result.CallPeriodicity_frames = this.Periodicity_frames;
            bool[] periodPeaks = Model_OnePeriodicSyllable.Periodicity(peaks, Periodicity_frames, Periodicity_NH_frames);
            result.NumberOfPeriodicHits = DataTools.CountTrues(periodPeaks);
            //adjust score array for periodicity
            for (int i = 0; i < frameCount; i++) if (!periodPeaks[i]) scores[i] = 0.0;


            Log.WriteIfVerbose("END Model_1PeriodicSyllable.ScanSymbolSequenceWithModel()");
        } //end ScanSymbolSequenceWithMM()

        #endregion


        public void SetPeriodicityParameters()
        {
            if (Periodicity_ms <= 0)
                throw new ArgumentException("Configuration file is invalid - no periodicity specified..");

            int period_frame = (int)Math.Round(Periodicity_ms / FrameOffset / (double)1000);
            this.Periodicity_frames = period_frame;
            this.Periodicity_NH_frames = (int)Math.Floor(period_frame * Model_OnePeriodicSyllable.fractionalNH); //arbitrary NH
            this.Periodicity_NH_ms = (int)Math.Floor(Periodicity_ms * Model_OnePeriodicSyllable.fractionalNH); //arbitrary NH
            //Console.WriteLine("\tperiod_ms="    + period_ms    + "+/-" + this.Periodicity_NH_ms);
            //Console.WriteLine("\tperiod_frame=" + period_frame + "+/-" + this.Periodicity_NH_frames);
        }



        public override void Save(TextWriter writer)
        {
            Log.WriteIfVerbose("START Model_OnePeriodicSyllable.Save()");

            writer.WriteLine("#**************** INFO ABOUT THE LANGUAGE MODEL ***************");
            writer.WriteLine("#Options: UNDEFINED, ONE_PERIODIC_SYLLABLE, MM_ERGODIC, MM_TWO_STATE_PERIODIC");

            if ((BaseTemplate.task == Task.EXTRACT_FV) || (BaseTemplate.task == Task.CREATE_ACOUSTIC_MODEL))
            {
                writer.WriteLine("MODEL_TYPE=UNDEFINED");
                writer.WriteLine("#MODEL_TYPE=ONE_PERIODIC_SYLLABLE");
            }
            else writer.WriteConfigValue("MODEL_TYPE", ModelType);
            writer.WriteConfigValue("PERIODICITY_MS", Periodicity_ms);
            
            writer.WriteConfigValue("NUMBER_OF_WORDS", WordCount);
            // Although when read in the Words are split into different tags with multiple examples this information
            // is not stored (or used) so we can not persist it back. Instead we just write as if each word
            // is separate with 1 example each

            for (int i = 0; i < WordCount; i++)
            {
                writer.WriteConfigArray("WORD{0}_EXAMPLE1", Words);
            }
            //writer.WriteConfigArray("WORD{0}_EXAMPLE1", Words);
            writer.WriteLine("#");
            writer.WriteConfigValue("SONG_WINDOW", SongWindow);
            writer.WriteLine("#");
            writer.Flush();
            Log.WriteIfVerbose("END Model_OnePeriodicSyllable.Save()");
        }

        public static bool[] RemoveSubThresholdPeaks(double[] scores, bool[] peaks, double threshold)
        {
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];
            for (int n = 0; n < length; n++)
            {
                newPeaks[n] = peaks[n];
                if (scores[n] < threshold)
                    newPeaks[n] = false;
            }
            return newPeaks;
        }

        /// <summary>
        /// returns a reconstituted array of zscores.
        /// Only gives values to score elements in vicinity of a peak.
        /// </summary>
        public static double[] ReconstituteScores(double[] scores, bool[] peaks)
        {
            int length = scores.Length;
            double[] newScores = new double[length];
            for (int n = 0; n < length; n++)
                if (peaks[n])
                    newScores[n] = scores[n];
            return newScores;
        }

        public static bool[] Periodicity(bool[] peaks, int period_frame, int period_NH)
        {
            int L = peaks.Length;
            bool[] hits = new bool[L];
            int index = 0;

            //find the first peak
            for (int n = 0; n < L; n++)
            {
                index = n;
                if (peaks[n])
                    break;
            }
            if (index == L - 1)
                return hits; // i.e. no peaks in the array

            // have located index of the first peak. Now look for peaks correct distance apart
            int minDist = period_frame - period_NH;
            int maxDist = period_frame + period_NH;
            for (int n = index + 1; n < L; n++)
            {
                if (peaks[n])
                {
                    int period = n - index;
                    if ((period >= minDist) && (period <= maxDist))
                    {
                        hits[index] = true;
                        hits[n] = true;
                    }
                    index = n; //set new position
                }
            }

            return hits;
        }


    } // end of class Model_OnePeriodicSyllable

}
