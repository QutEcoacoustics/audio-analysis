using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioAnalysis
{
    class AcousticEvent
    {
        public static int    FreqBinCount;
        public static double FreqBinWidth;
        public static double FrameDuration;
        public static double FramesPerSecond;

        //'Start Time (s)' 'Duration (s)' 'Lowest Freq' 'Highest Freq' 'I1 Mean dB' 'I1 Var dB' 'I2 Mean dB' 'I2 Var dB' 'I3 Mean dB' 'I3 Var dB'

        double StartTime { get; set; } // (s),
        double Duration; // (s),
        int    MinFreq;  //,
        int    MaxFreq;  //,
        //double I1MeandB; //mean intensity of pixels in the event prior to noise subtraction 
        //double I1VardB;  //,
        //double I2MeandB; //mean intensity of pixels in the event after Wiener filter, prior to noise subtraction 
        //double I2VardB;  //,
        double I3Mean;   //mean intensity of pixels in the event AFTER noise reduciton 
        double I3Var;    //variance of intensity of pixels in the event.
        int FreqRange { get { return(MaxFreq - MinFreq + 1); } }

        public Oblong oblong { get; private set;} 

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AcousticEvent(double startTime, double duration, double minFreq, double maxFreq)
        {
            this.StartTime = startTime;
            this.Duration = duration;
            this.MinFreq = (int)minFreq;
            this.MaxFreq = (int)maxFreq;
            if (FreqBinCount == 0)
            {
                Console.WriteLine("WARNING!! ######## Frequency bin count has not been set");
                throw new Exception("FATAL ERROR - TERMINATE");
            }
            if (FreqBinWidth == 0.0)
            {
                Console.WriteLine("WARNING!! ######## Frequency bin width (Herz) has not been set");
                throw new Exception("FATAL ERROR - TERMINATE");
            }
            if (FrameDuration == 0.0)
            { 
                Console.WriteLine("WARNING!! ######## Frame duration (seconds)   has not been set");
                throw new Exception("FATAL ERROR - TERMINATE");
            }

            //translate time/freq dimensions to coordinates in a matrix.
            //rows of matrix are the frames
            //columns of matrix are the freq bins. Origin is top left - as [per matrix in the sonogram class.
            int topRow    = (int)Math.Round(StartTime / FrameDuration);
            int bottomRow = (int)Math.Round((StartTime+Duration) / FrameDuration);
            //if (topRow < 0) topRow = 0;
            int leftCol   = (int)Math.Round(minFreq / FreqBinWidth);
            int rightCol  = (int)Math.Round(maxFreq / FreqBinWidth);
            if (rightCol == FreqBinCount) rightCol = FreqBinCount - 1;
            this.oblong   = new Oblong(topRow, leftCol, bottomRow, rightCol);
        }
        public void SetNetIntensityAfterNoiseReduction(double mean, double var)
        {
            this.I3Mean = mean; //,
            this.I3Var  = var; //
        }

        public string WriteProperties()
        {
            return " min-max=" + MinFreq + "-" + MaxFreq + ",  " + oblong.c1 + "-" + oblong.c2;
        }

    }
}
