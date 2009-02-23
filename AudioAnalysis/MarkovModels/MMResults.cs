using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkovModels
{
    public class MMResults
    {

        public List<Vocalisation> VocalList { get; set; }
        public double probOfAverageTrainingSequenceGivenModel { set; get; }

        public MMResults(List<Vocalisation> list)
        {
            this.VocalList = list;
        }

    }//end class MMResults


    /// <summary>
    /// Used to store information about a putative vocalisation represented as symbol sequence
    /// that has been extracted from the symbol sequence output by the acoustic model.
    /// </summary>
    public class Vocalisation
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Length { get; set; }
        public string Sequence { get; set; }
        public bool IsCorrectDuration { get; set; }
        public double Score { get; set; }
        public double DurationProbability { get; set; }


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="sequence"></param>
        public Vocalisation(int start, int end, string sequence)
        {
            this.Start = start;
            this.End = end;
            this.Length = end - start + 1;
            this.Sequence = sequence;
        } //end CONSTRUCTOR

    }//end class Vocalisation


}
