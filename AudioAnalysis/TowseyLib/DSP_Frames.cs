using System;
using System.Collections.Generic;
using System.Text;
//using MathNet.Numerics;
using MathNet.Numerics.Transformations;


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

        public static System.Tuple<double[], double[], double[,], double> ExtractEnvelopeAndFFTs(double[] signal, int sr, int windowSize, double overlap)
        {
            int length = signal.Length;
            int frameOffset = (int)(windowSize * (1 - overlap));
            int frameCount = (length - windowSize + frameOffset) / frameOffset;
            double[] average = new double[frameCount];
            double[] envelope = new double[frameCount];

            //set up the FFT parameters
            TowseyLib.FFT.WindowFunc w = TowseyLib.FFT.GetWindowFunction(FFT.Key_HammingWindow);
            var fft = new TowseyLib.FFT(windowSize, w, true); // init class which calculates the MATLAB compatible .NET FFT
            double[,] spectrogram = new double[frameCount, fft.CoeffCount]; //init amplitude sonogram
            double[] f1; //the fft

            //cycle through the frames
            for (int i = 0; i < frameCount; i++)
            {
                List<int> periodList = new List<int>();
                int start = i * frameOffset;
                int end = start + windowSize;

                //get average and envelope
                double maxValue = Math.Abs(signal[start]);
                double total = signal[start];
                for (int x = start + 1; x < end; x++)
                {
                    total += signal[x]; // go through current frame to get signal average/DC
                    double absValue = Math.Abs(signal[x]);
                    if (absValue > maxValue) maxValue = absValue;
                }
                average[i] = total / windowSize;
                envelope[i] = maxValue;

                //remove the average from signal
                double[] signalMinusAv = new double[windowSize];
                for (int j = 0; j < windowSize; j++)
                    signalMinusAv[j] = signal[start + j] - average[i];

                //generate the spectra of FFT AMPLITUDES - NOTE: f[0]=DC;  f[64]=Nyquist  
                f1 = fft.InvokeDotNetFFT(signalMinusAv);                 //returns fft amplitude spectrum
                //f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
                //f1 = fft.Invoke(DataTools.GetRow(frames, i));          //returns fft amplitude spectrum

                //if (smoothingWindow > 2) f1 = DataTools.filterMovingAverage(f1, smoothingWindow); //smooth spectrum to reduce variance
                for (int j = 0; j < fft.CoeffCount; j++) //foreach freq bin
                    spectrogram[i, j] = f1[j]; //transfer amplitude
                
            } // end frames
            return System.Tuple.Create(average, envelope, spectrogram, fft.WindowPower);
        }



        public static System.Tuple<double[], double[], double[], double[], double[]> ExtractEnvelopeAndZeroCrossings(double[] signal, int sr, int windowSize, double overlap)
        {
            int length = signal.Length;
            int frameOffset = (int)(windowSize * (1 - overlap));
            int frameCount = (length - windowSize + frameOffset) / frameOffset;
            double[] average = new double[frameCount];
            double[] envelope = new double[frameCount];
            double[] zeroCrossings = new double[frameCount]; // count of zero crossings
            double[] zcPeriod = new double[frameCount];      // sample count between zero crossings
            double[] sdPeriod = new double[frameCount];      // standard deviation of sample count between zc.
            for (int i = 0; i < frameCount; i++)
            {
                List<int> periodList = new List<int>();
                int start = i * frameOffset;
                int end = start + windowSize;

                //get average and envelope
                double maxValue = -Double.MaxValue;
                double total = signal[start];
                for (int x = start + 1; x < end; x++)
                {
                    total += signal[x]; // go through current frame to get signal average/DC
                    double absValue = Math.Abs(signal[x]);
                    if (absValue > maxValue) maxValue = absValue;
                }
                average[i]  = total / windowSize;
                envelope[i] = maxValue;

                //remove the average from signal
                double[] signalMinusAv = new double[windowSize];
                for (int j = 0; j < windowSize; j++)
                    signalMinusAv[j] = signal[start + j] - average[i];

                //get zero crossings and periods
                int zeroCrossingCount = 0;
                int prevLocation = 0;
                double prevValue = signalMinusAv[0];
                for (int j = 1; j < windowSize; j++) // go through current frame
                {
                    //double absValue = Math.Abs(signalMinusAv[j]);
                    if (signalMinusAv[j] * prevValue < 0.0) // ie zero crossing
                    {
                        if (zeroCrossingCount > 0) periodList.Add(j - prevLocation); // do not want to accumulate counts prior to first ZC.
                        zeroCrossingCount++; // count zero crossings
                        prevLocation = j;
                        prevValue = signalMinusAv[j];
                    }
                } // end current frame

                zeroCrossings[i] = zeroCrossingCount;
                int[] periods = periodList.ToArray();
                double av = 0.0;
                double sd = 0.0;
                NormalDist.AverageAndSD(periods, out av, out sd);
                zcPeriod[i] = av;
                sdPeriod[i] = sd;
            }
            return System.Tuple.Create(average, envelope, zeroCrossings, zcPeriod, sdPeriod);
        }


        public static int[] ConvertZeroCrossings2Hz(double[] zeroCrossings, int frameWidth, int sampleRate)
        {
            int L = zeroCrossings.Length;
            var freq = new int[L];
            for (int i = 0; i < L; i++) freq[i] = (int)(zeroCrossings[i] * sampleRate / 2 / frameWidth);
            return freq;
        }

        public static double[] ConvertSamples2Milliseconds(double[] sampleCounts, int sampleRate)
        {
            int L = sampleCounts.Length;
            var tValues = new double[L];
            for (int i = 0; i < L; i++) tValues[i] = sampleCounts[i] * 1000 / (double)sampleRate;
            return tValues;
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
