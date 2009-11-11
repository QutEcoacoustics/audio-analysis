using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioAnalysis
{
    public class AcousticEvent
    {

        //DIMENSIONS OF THE EVENT
        public double StartTime { get; set; } // (s),
        public double Duration; // in secondss
        public int    MinFreq;  //
        public int    MaxFreq;  //
        public int    FreqRange { get { return(MaxFreq - MinFreq + 1); } }
        public bool   IsMelscale { get; set; } 
        public Oblong oblong { get; private set;}

        public int FreqBinCount { get; private set;}     //required for conversions to & from MEL scale
        public double FreqBinWidth { get; private set; } //required for freq-binID conversions
        public double FrameDuration;    //frame duration in seconds
        public double FrameOffset;      //time between frame starts in seconds
        public double FramesPerSecond;  //inverse of the frame offset


        //PROPERTIES OF THE EVENTS I.E. SCORE ETC
        public double Score { get; set; }
        //double I1MeandB; //mean intensity of pixels in the event prior to noise subtraction 
        //double I1Var;  //,
        //double I2MeandB; //mean intensity of pixels in the event after Wiener filter, prior to noise subtraction 
        //double I2Var;  //,
        double I3Mean;   //mean intensity of pixels in the event AFTER noise reduciton 
        double I3Var;    //variance of intensity of pixels in the event.


                /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AcousticEvent(double startTime, double duration, double minFreq, double maxFreq)
        {
            this.StartTime = startTime;
            this.Duration = duration;
            this.MinFreq = (int)minFreq;
            this.MaxFreq = (int)maxFreq;
            this.IsMelscale = false;
            oblong = null;// have no info to convert time/Hz values to coordinates
        }

        ///// <summary>
        ///// CONSTRUCTOR
        ///// </summary>
        //public AcousticEvent(double startTime, double duration, double minFreq, double maxFreq, bool doMelscale)
        //{
        //    this.StartTime = startTime;
        //    this.Duration = duration;
        //    this.MinFreq = (int)minFreq;
        //    this.MaxFreq = (int)maxFreq;
        //    this.IsMelscale = doMelscale;

        //    //first check that the static variables required to initialise an oblong object have been initialised.
        //    CheckIfStaticsAreInitialised();
        //    this.oblong   = ConvertEvent2Oblong();
        //}

        /// <summary>
        /// This constructor works ONLY for linear Herz scale events
        /// </summary>
        /// <param name="o"></param>
        /// <param name="binWidth"></param>
        public AcousticEvent(Oblong o, double frameOffset, double binWidth)
        {
            this.oblong       = o;
            this.FreqBinWidth = binWidth;
            this.FrameOffset  = frameOffset;
            this.IsMelscale   = false;

            double startTime; double duration;
            RowIDs2Time(o.r1, o.r2, frameOffset, out startTime, out duration);
            this.StartTime = startTime;
            this.Duration = duration;
            int minF; int maxF;
            HerzBinIDs2Freq(o.c1, o.c2, binWidth, out minF, out maxF);
            this.MinFreq = minF;
            this.MaxFreq = maxF;
        }

        public void DoMelScale(bool doMelscale, int freqBinCount)
        {
            this.IsMelscale   = doMelscale;
            this.FreqBinCount = freqBinCount;
        }

        public void SetTimeAndFreqScales(int samplingRate, int windowSize, int windowOffset)
        {
            double frameDuration, frameOffset, framesPerSecond;
            CalculateTimeScale(samplingRate, windowSize, windowOffset,
                                         out frameDuration, out frameOffset, out framesPerSecond);
            this.FrameDuration  = frameDuration;    //frame duration in seconds
            this.FrameOffset    = frameOffset;
            this.FramesPerSecond= framesPerSecond;  //inverse of the frame offset

            int binCount;
            double binWidth;
            CalculateFreqScale(samplingRate, windowSize, out binCount, out binWidth);
            this.FreqBinCount = binCount; //required for conversions to & from MEL scale
            this.FreqBinWidth = binWidth; //required for freq-binID conversions

            if (this.oblong == null) this.oblong = ConvertEvent2Oblong();

        }


        public Oblong ConvertEvent2Oblong()
        {
            //translate time/freq dimensions to coordinates in a matrix.
            //columns of matrix are the freq bins. Origin is top left - as per matrix in the sonogram class.
            //Translate time dimension = frames = matrix rows.
            int topRow; int bottomRow;
            Time2RowIDs(this.StartTime, this.Duration, this.FrameOffset, out topRow, out bottomRow);

            //Translate freq dimension = freq bins = matrix columns.
            int leftCol; int rightCol;
            Freq2BinIDs(this.IsMelscale, this.MinFreq, this.MaxFreq, this.FreqBinCount, this.FreqBinWidth, out leftCol, out rightCol);

            return new Oblong(topRow, leftCol, bottomRow, rightCol);
        }

        public string WriteProperties()
        {
            return " min-max=" + MinFreq + "-" + MaxFreq + ",  " + oblong.c1 + "-" + oblong.c2;
        }



        //#################################################################################################################
        //METHODS TO CONVERT BETWEEN FREQ BIN AND HERZ OR MELS 

        /// <summary>
        /// converts frequency bounds of an event to left and right columns of object in sonogram matrix
        /// </summary>
        /// <param name="minF"></param>
        /// <param name="maxF"></param>
        /// <param name="leftCol"></param>
        /// <param name="rightCol"></param>
        public static void Freq2BinIDs(bool doMelscale, int minFreq, int maxFreq, int binCount, double binWidth,
                                                                                              out int leftCol, out int rightCol)
        {
            if(doMelscale)
                Freq2MelsBinIDs(minFreq, maxFreq, binWidth, binCount, out leftCol, out rightCol);
            else
                Freq2HerzBinIDs(minFreq, maxFreq, binWidth, out leftCol, out rightCol);
        }
        public static void Freq2HerzBinIDs(int minFreq, int maxFreq, double binWidth, out int leftCol, out int rightCol)
        {
            leftCol  = (int)Math.Round(minFreq / binWidth);
            rightCol = (int)Math.Round(maxFreq / binWidth);
        }
        public static void Freq2MelsBinIDs(int minFreq, int maxFreq, double binWidth, int binCount, out int leftCol, out int rightCol)
        {
                double nyquistFrequency = binCount * binWidth;
                double maxMel = Speech.Mel(nyquistFrequency); 
                int melRange = (int)(maxMel - 0 + 1);
                double binsPerMel = binCount / (double)melRange;
                leftCol  = (int)Math.Round((double)Speech.Mel(minFreq) * binsPerMel);
                rightCol = (int)Math.Round((double)Speech.Mel(maxFreq) * binsPerMel);
        }

        /// <summary>
        /// converts left and right column IDs to min and max frequency bounds of an event
        /// WARNING!!! ONLY WORKS FOR LINEAR HERZ SCALE. NEED TO WRITE ANOTHER METHOD FOR MEL SCALE ############################
        /// </summary>
        /// <param name="leftCol"></param>
        /// <param name="rightCol"></param>
        /// <param name="minFreq"></param>
        /// <param name="maxFreq"></param>
        public static void HerzBinIDs2Freq(int leftCol, int rightCol, double binWidth, out int minFreq, out int maxFreq)
        {
            minFreq = (int)Math.Round(leftCol * binWidth);
            maxFreq = (int)Math.Round(rightCol * binWidth);
            //if (doMelscale) //convert min max Hz to mel scale
            //{
            //}
        }


        
        
        //#################################################################################################################
        //METHODS TO CONVERT BETWEEN TIME BIN AND SECONDS 
        public static void RowIDs2Time(int topRow, int bottomRow, double frameOffset, out double startTime, out double duration)
        {
            startTime  = topRow * frameOffset;
            double end = (bottomRow + 1) * frameOffset;
            duration = end - startTime;
        }

        public static void Time2RowIDs(double startTime, double duration, double frameOffset, out int topRow, out int bottomRow)
        {
            topRow = (int)Math.Round(startTime / frameOffset);
            bottomRow = (int)Math.Round((startTime + duration) / frameOffset);
        }

        public void SetNetIntensityAfterNoiseReduction(double mean, double var)
        {
            this.I3Mean = mean; //
            this.I3Var  = var;  //
        }

        /// <summary>
        /// returns the frame duration and offset duration in seconds
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="windowSize"></param>
        /// <param name="windowOffset"></param>
        /// <param name="frameDuration">units = seconds</param>
        /// <param name="frameOffset">units = seconds</param>
        /// <param name="framesPerSecond"></param>
        public static void CalculateTimeScale(int samplingRate, int windowSize, int windowOffset,
                                                        out double frameDuration, out double frameOffset, out double framesPerSecond)
        {
            frameDuration = windowSize / (double)samplingRate;
            frameOffset = windowOffset / (double)samplingRate;
            framesPerSecond = 1 / frameOffset;
        }
        public static void CalculateFreqScale(int samplingRate, int windowSize, out int binCount, out double binWidth)
        {
            binCount = windowSize / 2;
            binWidth = samplingRate / (double)windowSize; //= Nyquist / binCount
        }


    }
}
