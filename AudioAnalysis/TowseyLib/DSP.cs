using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{

    /// <summary>
    /// digital signal processing methods
    /// </summary>
    public class DSP
    {
        public const double pi = Math.PI;



        public static double[] GetSignal(int sampleRate, double duration, int[] freq)
        {
            int length = (int)(sampleRate * duration); 
            double[] data = new double[length];
            int count = freq.Length;
            double[] omega = new double[count];

            //for (int f = 0; f < count; f++)
            //{
            //    omega[f] = 2.0 * Math.PI * freq[f] / (double)sampleRate;
            //}


            for (int i = 0; i < length; i++)
            {
                //for (int f = 0; f < count; f++) data[i] += Math.Sin(omega[f] * i);
                for (int f = 0; f < count; f++) data[i] += Math.Sin(2.0 * Math.PI * freq[f] * i / (double)sampleRate);
            }
            return data;
        }

        /// <summary>
        /// converts passed arguments into step decay and step radians ie radians per sample or OMEGA
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sf">sampling frequency</param>
        /// <param name="tHalf">half life in seconds</param>
        /// <param name="period">of the cycle of interest</param>
        /// <param name="filterLength">length of filter in seconds</param>
        /// <returns></returns>
        public static double[] Filter_DecayingSinusoid(double[] signal, double sf, double tHalf, double period, double filterDuration)
        {
            double t = 1/sf; //inverse of sampling frequency (in seconds)

            double samplesPerTHalf = tHalf*sf;
            double stepDecay = 0.5 / samplesPerTHalf; 
            double samplesPerPeriod = period*sf;
            double stepRadians = 2 * pi / samplesPerPeriod;
            int filterLength = (int)(filterDuration * sf); 
            double[] newSig = Filter_DecayingSinusoid(signal, stepDecay, stepRadians, filterLength);
            return newSig;

        }


        public static double[] Filter_DecayingSinusoid(double[] signal, double stepDecay, double stepRadians, int filterLength)
        {   
            double B = stepDecay; // beta = decay per signal sample
            double W = stepRadians; // OMEGA = radians per signal sample
            
            double[] coeff = new double[filterLength];
            int signalLength = signal.Length;
            double[] newSig = new double[signalLength];

            // set up the coefficients
            for(int n=0; n<filterLength; n++)
            {
                double angle = W*n;
                double decay = B*n;
                coeff[filterLength-n-1] = Math.Cos(angle)*Math.Exp(-decay);
            }


            // transfer initial partially filtered values
            for (int i = 0; i < filterLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < filterLength; j++)
                {
                    if ((i - j) < 0) break;
                    sum += (coeff[filterLength - j - 1] * signal[i - j]);
                }
                newSig[i] = sum;
            }
            // transfer filtered values
            for(int i=filterLength; i<signalLength; i++)
            {   
                double sum = 0.0;
                for(int j=0; j<filterLength; j++) sum += (coeff[filterLength-j-1] * signal[i-j]);
                newSig[i] = sum;
            }
            //System.Console.WriteLine("FilterGain="+DSP.GetGain(coeff));
            return newSig;
        } //Filter_DecayingSinusoid()

        public static double[] Filter(double[] signal, double[] filterCoeff)
        {
            int signalLength = signal.Length;
            double[] newSig = new double[signalLength];

            int filterLength = filterCoeff.Length;
            // transfer initial partially filtered values
            for (int i = 0; i < filterLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < filterLength; j++)
                {
                    if ((i - j) < 0) break;
                    sum += (filterCoeff[filterLength - j - 1] * signal[i - j]);
                }
                newSig[i] = sum;
            }
            // transfer filtered values
            for (int i = filterLength; i < signalLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < filterLength; j++) sum += (filterCoeff[filterLength - j - 1] * signal[i - j]);
                newSig[i] = sum;
            }
            return newSig;
        } //Filter()

        public static double GetGain(double[] filterCoeff)
        {
            int filterLength = filterCoeff.Length;
            //set up the impulse signal
            double[] impulse = new double[3 * filterLength];
            impulse[filterLength] = 1.0;
            double[] newSig = Filter(impulse, filterCoeff);
            double gain = 0.0;
            for (int j = 0; j < impulse.Length; j++) gain += newSig[j];
            return gain;
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
            Console.WriteLine("TESTING METHODS IN CLASS DataTools");



            //COPY THIS TEST TEMPLATE
            if (false) //test Method(parameters)
            {   
                System.Console.WriteLine("\nTest of METHOD)");
            }//end test Method(string fName)



            if (true) //test Method(parameters)
           {
                System.Console.WriteLine("\nTest of Filter_DecayingSinusoid()");
                double sf = 100;
                double tHalf = 0.2;//seconds
                double period = 0.2; //seconds
                double filterDuration = 1.0; //seconds
                int signalLength= 100;
                
                //set up the impulse signal
                double[] signal = new double[signalLength];
                signal[10] = 1.0;
                double[] newSig = Filter_DecayingSinusoid(signal, sf, tHalf, period, filterDuration);
                DisplaySignal(newSig, true);
            }//end test Method(string fName)



            if (false) //test Filter_DecayingSinusoid()
            {
                System.Console.WriteLine("\nTest of Filter_DecayingSinusoid()");
                int signalLength= 100;
                //set up the impulse signal
                double[] signal = new double[signalLength];
                signal[10] = 1.0;

                //filter constatns
                double stepDecay= 0.05 ;
                double stepRadians = 0.4;
                int filterLength = 50;//number of time delays or coefficients in the filter
                double[] newSig = Filter_DecayingSinusoid(signal, stepDecay, stepRadians, filterLength);
                DisplaySignal(newSig, true);
            }

            Console.WriteLine("FINISHED!!");
            Console.ReadLine();
        }//end Main()

    }//end class DSP
}
