using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkovModels
{
    public class MMResults
    {
        public List<Vocalisation> PartialVocalisations { get; set; }
        public double probOfAverageTrainingSequenceGivenModel { set; get; }
        public double qualityThreshold { set; get; }

        public MMResults(List<Vocalisation> list)
        {
            this.PartialVocalisations = list;
        }

    }//end class MMResults


    /// <summary>
    /// Used to store information about a putative vocalisation represented as symbol sequence
    /// that has been extracted from the symbol sequence output by the acoustic model.
    /// </summary>
    public class Vocalisation
    {
        public string SymbolSequence { get; set; }
        public int    Start { get; set; }
        public int    End { get; set; }
        public int    Length { get; set; }
        public double LengthZscore { set; get; }
        public double DurationProbability { get; set; }

        public int    TransitionCount { set; get; }
        public double TransitionZscore { set; get; }
        public double QualityScore { set; get; }//at present = sum of LengthZscore and TransitionZscore.
        public double Score { get; set; }       //obtained from the MM.



        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="sequence"></param>
        public Vocalisation(int start, int end, string sequence)
        {
            this.Start    = start;
            this.End      = end;
            this.Length   = end - start + 1;
            this.SymbolSequence = sequence;
            int count = 0;
            for (int i = 1; i < sequence.Length; i++) if(sequence[i] != sequence[i-1]) count++;
            this.TransitionCount = count;
        } //end CONSTRUCTOR

    }//end class Vocalisation


}
