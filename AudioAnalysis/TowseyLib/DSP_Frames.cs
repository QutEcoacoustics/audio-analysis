using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
    /// <summary>
    /// digital signal processing methods
    /// </summary>
    public static class DSP_Frames
    {
        public const double pi = Math.PI;



        /// <summary>
        /// returns the start and end index of all frames in a long audio signal
        /// </summary>
        public static int[,] FrameStartEnds(int dataLength, int windowSize, double windowOverlap)
        {
            int step = (int)(windowSize * (1 - windowOverlap));

            if (step < 1)
                throw new ArgumentException("Frame Step must be at least 1");
            if (step > windowSize)
                throw new ArgumentException("Frame Step must be <=" + windowSize);

            int overlap = windowSize - step;
            int framecount = (dataLength - overlap) / step; //this truncates residual samples
            if (framecount < 2) throw new ArgumentException("Signal must produce at least two frames!");

            int offset = 0;
            int[,] frames = new int[framecount, 2]; //col 0 =start; col 1 =end

            for (int i = 0; i < framecount; i++) //foreach frame
            {
                frames[i, 0] = offset;                  //start of frame
                frames[i, 1] = offset + windowSize - 1; //end of frame
                offset += step;
            }
            return frames;
        }


        public static double[,] Frames(double[] data, int[,] startEnds)
        {
            int windowSize = startEnds[0, 1] + 1;
            int framecount = startEnds.GetLength(0);
            double[,] frames = new double[framecount, windowSize];

            for (int i = 0; i < framecount; i++) //for each frame
            {
                for (int j = 0; j < windowSize; j++) frames[i, j] = data[startEnds[i, 0] + j];
            } //end matrix
            return frames;
        }

        
        /// <summary>
        /// Breaks a long audio signal into frames with given step
        /// IMPORTANT: THIS METHOD PRODUCES A LARGE MEMORY-HUNGRY MATRIX.  BEST TO USE THE FrameStartEnds() METHOD.
        /// </summary>
        public static double[,] Frames(double[] data, int windowSize, double windowOverlap)
        {
            int step = (int)(windowSize * (1 - windowOverlap));

            if (step < 1)
                throw new ArgumentException("Frame Step must be at least 1");
            if (step > windowSize)
                throw new ArgumentException("Frame Step must be <=" + windowSize);

            int overlap = windowSize - step;
            int framecount = (data.Length - overlap) / step; //this truncates residual samples
            if (framecount < 2) throw new ArgumentException("Sonogram width must be at least 2");

            int offset = 0;
            double[,] frames = new double[framecount, windowSize];

            for (int i = 0; i < framecount; i++) //foreach frame
            {
                for (int j = 0; j < windowSize; j++) //foreach sample
                    frames[i, j] = data[offset + j];
                offset += step;
            } //end matrix
            return frames;
        }


        /// <summary>
        /// returns the min and max values in each frame. Signal values range from -1 to +1.
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="minAmp"></param>
        /// <param name="maxAmp"></param>
        public static void SignalEnvelope(double[,] frames, out double[] minAmp, out double[] maxAmp)
        {
            int frameCount = frames.GetLength(0);
            int N  = frames.GetLength(1);
            minAmp = new double[frameCount];
            maxAmp = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double min =  Double.MaxValue;
                double max = -Double.MaxValue;
                for (int j = 0; j < N; j++)  //foreach sample in frame
                {
                    if (min > frames[i, j]) min = frames[i, j];
                    else 
                    if (max < frames[i, j]) max = frames[i, j];
                }
                minAmp[i] = min;
                maxAmp[i] = max;
            }
        }




        /// <summary>
        /// counts the zero crossings in each frame
        /// This info is used for determing the begin and end points for vocalisations.
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static int[] ZeroCrossings(double[,] frames)
        {
            int frameCount = frames.GetLength(0);
            int N = frames.GetLength(1);
            int[] zc = new int[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                int count = 0;
                for (int j = 1; j < N; j++)  //foreach sample in frame
                {
                    count += Math.Abs(Math.Sign(frames[i, j]) - Math.Sign(frames[i, j - 1]));
                }
                zc[i] = count / 2;
            }
            return zc;
        }


        public static void DisplaySignal(double[] sig)
        {
                double[] newSig = DataTools.normalise(sig);

                foreach (double value in newSig)
                {
                    int count = (int)(value * 50);
                    for (int i = 0; i < count; i++) Console.Write("=");
                    Console.WriteLine("=");
                }
        }

        public static void DisplaySignal(double[] sig, bool showIndex)
        {
            double[] newSig = DataTools.normalise(sig);

            for (int n = 0; n < sig.Length; n++)
            {
                if (showIndex) Console.Write(n.ToString("D3") + "|");
                int count = (int)(newSig[n] * 50);
                for (int i = 0; i < count; i++)
                {
                    Console.Write("=");
                }
                Console.WriteLine("=");
            }
        }




        static void Main()
        {
            Console.WriteLine("TESTING METHODS IN CLASS DSP_Frames");



            //COPY THIS TEST TEMPLATE
            bool doit1 = false;
            if (doit1) //test Method(parameters)
            {   
                System.Console.WriteLine("\nTest of METHOD)");
            }//end test Method(string fName)



            //bool doit2 = true;
            //if (doit2) //test Method(parameters)
            //{
            //    System.Console.WriteLine("\nTest of Filter_DecayingSinusoid()");
            //    double sf = 100;
            //    double tHalf = 0.2;//seconds
            //    double period = 0.2; //seconds
            //    double filterDuration = 1.0; //seconds
            //    int signalLength= 100;
                
            //    //set up the impulse signal
            //    double[] signal = new double[signalLength];
            //    signal[10] = 1.0;
            //    double[] newSig = Filter_DecayingSinusoid(signal, sf, tHalf, period, filterDuration);
            //    DisplaySignal(newSig, true);
            //}//end test Method(string fName)



            //bool doit3 = false;
            //if (doit3) //test Filter_DecayingSinusoid()
            //{
            //    System.Console.WriteLine("\nTest of Filter_DecayingSinusoid()");
            //    int signalLength= 100;
            //    //set up the impulse signal
            //    double[] signal = new double[signalLength];
            //    signal[10] = 1.0;

            //    //filter constatns
            //    double stepDecay= 0.05 ;
            //    double stepRadians = 0.4;
            //    int filterLength = 50;//number of time delays or coefficients in the filter
            //    double[] newSig = Filter_DecayingSinusoid(signal, stepDecay, stepRadians, filterLength);
            //    DisplaySignal(newSig, true);
            //}

            Console.WriteLine("FINISHED!!");
            Console.ReadLine();
        }//end Main()

    }//end class DSP
}
