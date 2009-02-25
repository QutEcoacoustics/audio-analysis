using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace MarkovModels
{
    class SongDuration
    {
        string[] Sequences { set; get; }
        public double AvSongLength { set; get; }
        public double[] pdf { private set; get; }
        private double symbolDuration { set; get; }


        public SongDuration(string[] sequences, double deltaT)
        {
            this.symbolDuration = deltaT;
            this.Sequences = sequences;
            this.AvSongLength = TrainingSet.AverageSequenceLength(sequences);
            this.pdf = CalculateSongLengthPDF(sequences);
        }

        public double[] CalculateSongLengthPDF(string[] sequences)
        {
            int maxLength = (int)Math.Round(this.AvSongLength * 2);
            double[] probs = new double[maxLength];
            for (int w = 0; w < sequences.Length; w++) probs[sequences[w].Length] += 1.0;
            probs = DataTools.filterMovingAverage(probs, 5);
            probs = DataTools.NormaliseProbabilites(probs);
            return probs;
        }

        public double GetSongDurationProb(int symbolLength)
        {
            if (symbolLength >= pdf.Length) return 0.0; //return zero probability when length out of range
            return pdf[symbolLength];
        }

        public double GetSongDurationProb(double secLength)
        {
            int id = (int)Math.Round(secLength / symbolDuration);
            return pdf[id];
        }


        public string WritePdf2String()
        {
            StringBuilder sb = new StringBuilder(pdf[0].ToString("F3") + ", ");
            for (int i = 1; i < pdf.Length; i++)
            {
                if (i % 10 == 0) sb.Append("\n");
                sb.Append(pdf[i].ToString("F3") + ", ");
            }
            return sb.ToString();
        }

    }//end class
}
