using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioAnalysis
{
    public class AcousticEvent
    {
        public static int    FreqBinCount;
        public static double FreqBinWidth;
        public static double FrameDuration;
        public static double FramesPerSecond;


        //'Start Time (s)' 'Duration (s)' 'Lowest Freq' 'Highest Freq' 'I1 Mean dB' 'I1 Var dB' 'I2 Mean dB' 'I2 Var dB' 'I3 Mean dB' 'I3 Var dB'

        public double StartTime { get; set; } // (s),
        public double Duration; // (s),
        public int    MinFreq;  //,
        public int    MaxFreq;  //,
        //double I1MeandB; //mean intensity of pixels in the event prior to noise subtraction 
        //double I1VardB;  //,
        //double I2MeandB; //mean intensity of pixels in the event after Wiener filter, prior to noise subtraction 
        //double I2VardB;  //,
        double I3Mean;   //mean intensity of pixels in the event AFTER noise reduciton 
        double I3Var;    //variance of intensity of pixels in the event.
        public int FreqRange { get { return(MaxFreq - MinFreq + 1); } }

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
            oblong = null;// have no info to convert time/Hz values to coordinates
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AcousticEvent(double startTime, double duration, double minFreq, double maxFreq, bool doMelscale)
        {
            this.StartTime = startTime;
            this.Duration = duration;
            this.MinFreq = (int)minFreq;
            this.MaxFreq = (int)maxFreq;

            //first check that the static variables required to initialise an oblong object have been initialised.
            CheckIfStaticsAreInitialised();


            //translate time/freq dimensions to coordinates in a matrix.
            //columns of matrix are the freq bins. Origin is top left - as per matrix in the sonogram class.
            //First translate time dimension = frames = matrix rows.
            int topRow; int bottomRow;
            Time2RowIDs(startTime, duration, out topRow, out bottomRow);

            //Second translate freq dimension = freq bins = matrix columns.
            int leftCol; int rightCol;
            Freq2BinIDs(doMelscale, this.MinFreq, this.MaxFreq, out leftCol, out rightCol);

            this.oblong   = new Oblong(topRow, leftCol, bottomRow, rightCol);
        }

        public AcousticEvent(Oblong o)
        {
            //first check that the static variables required to initialise an oblong object have been initialised.
            CheckIfStaticsAreInitialised();

            this.oblong = o;

            double startTime; double duration;
            RowIDs2Time(o.r1, o.r2, out startTime, out duration);
            this.StartTime = startTime;
            this.Duration = duration;
            int minF; int maxF;
            BinIDs2Freq(o.c1, o.c2, out minF, out maxF);
            this.MinFreq = minF;
            this.MaxFreq = maxF;
        }


        private void CheckIfStaticsAreInitialised()
        {            
            if (AcousticEvent.FreqBinCount == 0)
            {
                Console.WriteLine("WARNING!! ######## Frequency bin count has not been set");
                throw new Exception("FATAL ERROR - TERMINATE");
            }
            if (AcousticEvent.FreqBinWidth == 0.0)
            {
                Console.WriteLine("WARNING!! ######## Frequency bin width (Herz) has not been set");
                throw new Exception("FATAL ERROR - TERMINATE");
            }
            if (AcousticEvent.FrameDuration == 0.0)
            { 
                Console.WriteLine("WARNING!! ######## Frame duration (seconds)   has not been set");
                throw new Exception("FATAL ERROR - TERMINATE");
            }
        }
    
        /// <summary>
        /// sets static variables
        /// WARNING!! ASSUMES THAT FRAMES ARE OVERLAPPED 50% ##############################################################################
        ///           NEED TO REWRITE IF THIS IS NOT THE CASE.
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="windowSize"></param>
        public static void SetStaticVariables(int samplingRate, int windowSize)
        {
            AcousticEvent.FrameDuration   = windowSize / (double)samplingRate;
            AcousticEvent.FramesPerSecond = 2 * samplingRate / (double)windowSize; //twice the expected because frames overlap 50%
            AcousticEvent.FreqBinCount    = windowSize / 2;
            AcousticEvent.FreqBinWidth    = samplingRate / (double)windowSize;
        }

        /// <summary>
        /// converts frequency bounds of an event to left and right columns of object in sonogram matrix
        /// </summary>
        /// <param name="minF"></param>
        /// <param name="maxF"></param>
        /// <param name="leftCol"></param>
        /// <param name="rightCol"></param>
        public static void Freq2BinIDs(bool doMelscale, int minFreq, int maxFreq, out int leftCol, out int rightCol)
        {
            Freq2BinIDs(doMelscale, minFreq, maxFreq, AcousticEvent.FreqBinCount, AcousticEvent.FreqBinWidth, out leftCol, out rightCol);
        }
        public static void Freq2BinIDs(bool doMelscale, int minFreq, int maxFreq, int binCount, double binWidth, 
            out int leftCol, out int rightCol)
        {
            leftCol  = (int)Math.Round(minFreq / binWidth);
            rightCol = (int)Math.Round(maxFreq / binWidth);
            if (rightCol >= binCount) rightCol = binCount - 1;

            if (doMelscale) //convert min max Hz to mel scale
            {
                double nyquistFrequency = binCount * binWidth;
                double maxMel = Speech.Mel(binCount * binWidth); // the Nyquist Frequency
                int melRange  = (int)(maxMel - 0 + 1);
                double pixelPerMel = binCount / (double)melRange;
                leftCol  = (int)Math.Round((double)Speech.Mel(minFreq) * pixelPerMel);
                rightCol = (int)Math.Round((double)Speech.Mel(maxFreq) * pixelPerMel);
            }

        }
        /// <summary>
        /// converts left and right column IDs to min and max frequency bounds of an event
        /// WARNING!!! ONLY WORKS FOR LINEAR FREQ SCALE> NEED TO REWRITE FOR MEL SCALE ##############################################
        /// </summary>
        /// <param name="leftCol"></param>
        /// <param name="rightCol"></param>
        /// <param name="minFreq"></param>
        /// <param name="maxFreq"></param>
        public static void BinIDs2Freq(int leftCol, int rightCol, out int minFreq, out int maxFreq)
        {
            minFreq = (int)Math.Round(leftCol  * FreqBinWidth);
            maxFreq = (int)Math.Round(rightCol * FreqBinWidth);
            //if (doMelscale) //convert min max Hz to mel scale
            //{
            //}
        }

        public static void RowIDs2Time(int topRow, int bottomRow, out double startTime, out double duration)
        {
            startTime  = topRow        * AcousticEvent.FrameDuration;
            double end = (bottomRow+1) * AcousticEvent.FrameDuration;
            duration = end - startTime;
        }

        public static void Time2RowIDs(double startTime, double duration, out int topRow, out int bottomRow)
        {
            topRow    = (int)Math.Round(startTime / AcousticEvent.FrameDuration);
            bottomRow = (int)Math.Round((startTime + duration) / AcousticEvent.FrameDuration);
            //if (topRow < 0) topRow = 0;
        }

        public void SetNetIntensityAfterNoiseReduction(double mean, double var)
        {
            this.I3Mean = mean; //
            this.I3Var  = var;  //
        }

        public string WriteProperties()
        {
            return " min-max=" + MinFreq + "-" + MaxFreq + ",  " + oblong.c1 + "-" + oblong.c2;
        }

    }
}
