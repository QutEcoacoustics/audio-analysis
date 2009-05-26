using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace MarkovModels
{
    /// <summary>
    /// This clsas is used to store information about a set of sequences used to train or test a markov model.
    /// In particular it calculates the av and sd of the lengths of a set of sequences.
    /// It also calculates the number of state transitions, that is, number of instances where a symbol is not same as 
    /// the preceding symbol 
    /// </summary>

    [Serializable]
    class SequenceInfo
    {
        string[] Sequences { set; get; }
        public double AvSeqLength { set; get; }
        public double SdSeqLength { set; get; }
        public double[] SeqLengthPDF { private set; get; }
        public double AvTransitionCount { set; get; }
        public double SdTransitionCount { set; get; }
        private double symbolDuration { set; get; }


        public SequenceInfo(string[] sequences, double deltaT)
        {
            this.symbolDuration = deltaT;
            this.Sequences = sequences;
            int[] seqLengths = ArrayOfSequenceLengths(sequences);
            double av;
            double sd;
            NormalDist.AverageAndSD(seqLengths, out av, out sd);
            this.AvSeqLength = av;
            this.SdSeqLength = sd;
            this.SeqLengthPDF = CalculateSongLengthPDF(sequences);

            int[] transitionCounts = ArrayOfAvTransitionCounts(sequences);
            NormalDist.AverageAndSD(transitionCounts, out av, out sd);
            this.AvTransitionCount = av;
            this.SdTransitionCount = sd;
        }

        public double[] CalculateSongLengthPDF(string[] sequences)
        {
            int maxLength = (int)Math.Round(this.AvSeqLength * 5);
            double[] probs = new double[maxLength];
            for (int w = 1; w < probs.Length-1; w++) probs[w] = 0.01; //zero element = zero length, therefore p = 0.0;
            for (int w = 1; w < sequences.Length; w++) probs[sequences[w].Length] += 1.0;
            probs = DataTools.filterMovingAverage(probs, 9);
            probs = DataTools.NormaliseProbabilites(probs);
            return probs;
        }

        public static int[] ArrayOfSequenceLengths(string[] sequences)
        {
            int[] lengthArray = new int[sequences.Length];
            for (int w = 0; w < sequences.Length; w++) lengthArray[w] = sequences[w].Length;
            return lengthArray;
        }

        public static int[] ArrayOfAvTransitionCounts(string[] sequences)
        {
            int[] countArray = new int[sequences.Length];
            for (int w = 0; w < sequences.Length; w++)
            {
                int count = 0;
                for (int i = 1; i < sequences[w].Length; i++) if(sequences[w][i] != sequences[w][i-1]) count++;
                countArray[w] = count;
            }
            return countArray;
        }

        public double GetSongDurationProb(int symbolLength)
        {
            if (symbolLength >= SeqLengthPDF.Length) return 0.0; //return zero probability when length out of range
            return SeqLengthPDF[symbolLength];
        }

        public double GetSongDurationProb(double secLength)
        {
            int id = (int)Math.Round(secLength / symbolDuration);
            return SeqLengthPDF[id];
        }


        public string WritePdf2String()
        {
            StringBuilder sb = new StringBuilder(SeqLengthPDF[0].ToString("F3") + ", ");
            for (int i = 1; i < SeqLengthPDF.Length; i++)
            {
                if (i % 10 == 0) sb.Append("\n");
                sb.Append(SeqLengthPDF[i].ToString("F3") + ", ");
            }
            return sb.ToString();
        }

    }//end class
}
