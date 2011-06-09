using System;
using System.Collections.Generic;
using System.Text;


namespace TowseyLib
{
    /// <summary>
    /// digital signal processing FILTERS methods
    /// 
    /// "Finite impulse response" (FIR) filters use only the input signals, 
    /// while an "infinite impulse response" filter (IIR) uses 
    /// both the input signal and previous samples of the output signal.
    /// FIR filters are always stable, while IIR filters may be unstable.

    /// </summary>
    public class DSP_IIRFilter
    {

        /// <summary>
        /// method to convert string codes to a specific IIR filter.
        /// FOR EACH NEW FILTER ADD LINE HERE AND WRITE NEW METHOD TO CREATE FILTER
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns></returns>
        public static System.Tuple<int, double[], double[], double> CreateFilter(string filterName)
        {
            if (filterName.StartsWith("Chebyshev_Highpass_400")) return Chebyshev_Highpass_400();
            else
            if (filterName.StartsWith("Chebyshev_Lowpass_1000")) return Chebyshev_Lowpass_1000();
            else
            if (filterName.StartsWith("Chebyshev_Lowpass_3000")) return Chebyshev_Lowpass_3000();
            else
            if (filterName.StartsWith("Chebyshev_Lowpass_5000")) return Chebyshev_Lowpass_5000();
            else
                {
                    System.Console.WriteLine("\nWARNING! There is no filter with name: " + filterName);
                    System.Console.ReadLine();
                }
            return null;
        }

        /// <summary>
        /// Create a Chebyshev_Highpass filter, shoulder=400, order=9; ripple=-0.1dB; sr=22050
        /// </summary>
        public static System.Tuple<int, double[], double[], double> Chebyshev_Highpass_400(/*no variables to pass*/)
        {
            int order = 9;
            double[] a_coeff = new double[order+1];
            a_coeff[9] =  -1.0;
            a_coeff[8] =   9.0;
            a_coeff[7] = -36.0;
            a_coeff[6] =  84.0; 
            a_coeff[5] =-126.0;
            a_coeff[4] = 126.0;
            a_coeff[3] = -84.0;
            a_coeff[2] =  36.0;
            a_coeff[1] =  -9.0;
            a_coeff[0] =   1.0;

            double[] b_coeff = new double[order+1];
            b_coeff[9] =   0.4245303487;
            b_coeff[8] =  -4.1924047356;
            b_coeff[7] =  18.4049669614;
            b_coeff[6] = -47.1530504520;
            b_coeff[5] =  77.7061040985;
            b_coeff[4] = -85.4326156183;
            b_coeff[3] =  62.6708517953;
            b_coeff[2] = -29.5822313447;
            b_coeff[1] =   8.1538488628;

            //double gain = 2018526051;  //gain at DC
            double gain = 1995420162;  //gain at centre
            return System.Tuple.Create(order, a_coeff, b_coeff, gain);
        }
        /// <summary>
        /// Create a Chebyshev_lowpass filter, shoulder=1000, order=9; ripple=-0.1dB; sr=22050
        /// </summary>
        public static System.Tuple<int, double[], double[], double> Chebyshev_Lowpass_1000(/*no variables to pass*/)
        {
            int order = 9;
            double[] a_coeff = new double[order+1];
            a_coeff[9] =   1.0;
            a_coeff[8] =   9.0;
            a_coeff[7] =  36.0;
            a_coeff[6] =  84.0;
            a_coeff[5] = 126.0;
            a_coeff[4] = 126.0;
            a_coeff[3] =  84.0;
            a_coeff[2] =  36.0;
            a_coeff[1] =   9.0;
            a_coeff[0] =   1.0;

            double[] b_coeff = new double[order+1];
            b_coeff[9] =  0.6209461413;
            b_coeff[8] = -5.7670078306;
            b_coeff[7] = 23.9296977418;
            b_coeff[6] =-58.2279247072;
            b_coeff[5] = 91.5701546702;
            b_coeff[4] =-96.5224922514;
            b_coeff[3] = 68.2009370551;
            b_coeff[2] =-31.1520875317;
            b_coeff[1] =  8.3477764588;

            double gain = 2018526051;  //gain at DC
            //double gain = 1995420162;  //gain at centre
            return System.Tuple.Create(order, a_coeff, b_coeff, gain);
        }
        /// <summary>
        /// Create a Chebyshev_lowpass filter, shoulder=3000, order=9; ripple=-0.1dB; sr=22050
        /// </summary>
        public static System.Tuple<int, double[], double[], double> Chebyshev_Lowpass_3000(/*no variables to pass*/)
        {
            int order = 9;
            double[] a_coeff = new double[order+1];
            a_coeff[9] =   1.0;
            a_coeff[8] =   9.0;
            a_coeff[7] =  36.0;
            a_coeff[6] =  84.0;
            a_coeff[5] = 126.0;
            a_coeff[4] = 126.0;
            a_coeff[3] =  84.0;
            a_coeff[2] =  36.0;
            a_coeff[1] =   9.0;
            a_coeff[0] =   1.0;

            double[] b_coeff = new double[order+1];
            b_coeff[9] =  0.2401212964;
            b_coeff[8] = -2.0732466453;
            b_coeff[7] =  8.3514809094;
            b_coeff[6] =-20.5774269817;
            b_coeff[5] = 34.1759653476;
            b_coeff[4] =-39.7166333903;
            b_coeff[3] = 32.3635071573;
            b_coeff[2] =-17.8943796613;
            b_coeff[1] =  6.1271000423;

            double gain = 145788.9707;   //gain at DC = 1.457889707e+05
            //double gain = 1.441201381e+05;  //gain at centre
            return System.Tuple.Create(order, a_coeff, b_coeff, gain);
        }
        /// <summary>
        /// Create a Chebyshev_lowpass filter, shoulder=5000, order=9; ripple=-0.1dB; sr=22050
        /// Shoulder located at 0.2267573696 Pi.
        /// </summary>
        public static System.Tuple<int, double[], double[], double> Chebyshev_Lowpass_5000(/*no variables to pass*/)
        {
            int order = 9;
            double[] a_coeff = new double[order + 1];
            a_coeff[9] = 1.0;
            a_coeff[8] = 9.0;
            a_coeff[7] = 36.0;
            a_coeff[6] = 84.0;
            a_coeff[5] = 126.0;
            a_coeff[4] = 126.0;
            a_coeff[3] = 84.0;
            a_coeff[2] = 36.0;
            a_coeff[1] = 9.0;
            a_coeff[0] = 1.0;

            double[] b_coeff = new double[order + 1];
            b_coeff[9] =  0.0935666122;
            b_coeff[8] = -0.5848434328;
            b_coeff[7] =  1.9318205319;
            b_coeff[6] = -4.2953935797;
            b_coeff[5] =  7.0142697632;
            b_coeff[4] = -8.7332273064;
            b_coeff[3] =  8.3100433803;
            b_coeff[2] = -6.0112693980;
            b_coeff[1] =  2.9946493876;

            double gain = 1826.066837;   //gain at DC = 1.826066837e+03
            //double gain = 1.805164023e+03;  //gain at centre
            return System.Tuple.Create(order, a_coeff, b_coeff, gain);
        }


        //================================================================================================
        //================================================================================================

        private double[] a;  //x coefficients
        private double[] b;  //y coefficients
        public int order {get; set;}
        public double gain;

        
        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="filterName"></param>
        public DSP_IIRFilter(string filterName)
        {
            var iir = CreateFilter(filterName);
            this.order = iir.Item1;
            this.a = iir.Item2;
            this.b = iir.Item3;
            this.gain = iir.Item4;
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// Pass your own filter coefficients
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public DSP_IIRFilter(double[] a, double[] b)
        {
            this.a = a;
            this.b = b;
            this.order = this.a.Length - 1;
        }


        //public void ApplyIIRFilter(double[] x, out double[] y)
        //{
        //    int order = a.Length - 1;
        //    int np    = x.Length - 1;

        //    if (np < order)
        //    {
        //        for(int k=0;k<order-np;k++)
        //            x[k] = 0.0;
        //        np = order;
        //    }

        //    y = new double[np + 1];
        //    for(int k=0;k<np+1;k++)
        //    {
        //        y[k] = 0.0;
        //    }
        //    int i, j;
        //    y[0] = b[0] * x[0];
        //    for (i = 1; i < order + 1; i++)
        //    {
        //        y[i] = 0.0;
        //        for (j = 0; j < i + 1; j++)
        //            y[i] = y[i] + b[j] * x[i - j];
        //        for (j = 0; j < i; j++)
        //            y[i] = y[i] - a[j + 1] * y[i - j - 1];
        //    }
        //    /* end of initial part */
        //    for (i = order + 1; i < np +1; i++)
        //    {
        //        y[i] = 0.0;
        //        for (j = 0; j < order + 1; j++)
        //            y[i] = y[i] + b[j] * x[i - j];
        //        for (j = 0; j < order; j++)
        //            y[i] = y[i] - a[j + 1] * y[i - j - 1];
        //    }
        //} //ApplyIIRFilter()

        public void ApplyIIRFilter(double[] x, out double[] y)
        {
            int np = x.Length; //signal length
            int size = this.order + 1; //Length of the A & B arrays;

            y = new double[np];

            int i, j;
            y[0] = a[0] * x[0];
            for (i = 1; i < size; i++)
            {
                for (j = 0; j <= i; j++) y[i] += (a[j] * x[i - j]);
                for (j = 1; j <= i; j++) y[i] += (b[j] * y[i - j]);
            }
            /* end of initial part */
            
            for (i = size; i < np; i++) //length of signal
            {
                y[i] += (a[0] * x[i]);
                for (j = 1; j < size; j++) y[i] += (a[j] * x[i - j]);
                for (j = 1; j < size; j++) y[i] += (b[j] * y[i - j]);
            }
            //adjust for gain
            //the factor of 2.30 is an approximate value to make up the difference between theoretical gain and my observed gain.
            //that is after correction the area under curve of impulse reponse should be close to 1.0.
            double myGain = this.gain * 2.30;
            for (i = 0; i < np; i++) 
            {
                y[i] /= myGain;
            }

        } //ApplyIIRFilter()


        public static void Main(string[] args)
        {
            Console.WriteLine("TESTING METHODS IN CLASS DSP_IIRFilter");



            //COPY THIS TEST TEMPLATE
            bool doit1 = true;
            if (doit1) //test Method(parameters)
            {
                System.Console.WriteLine("\nTest of METHOD ApplyIIRFilter()");
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\FrogPond_Samford_SE_555_20101023-000000.wav";
                //AudioRecording recording = new AudioRecording(recordingPath);
                //if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

                //SonogramConfig sonoConfig = new SonogramConfig(); //default values config
                //sonoConfig.SourceFName = recording.FileName;
                //sonoConfig.WindowOverlap = 0.5;      // set default value
                //sonoConfig.DoMelScale = false;
                //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
                //AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
                //SpectralSonogram sonogram = new SpectralSonogram(basegram);  //spectrogram has dim[N,257]

                // create filter
                string filterName = "Chebyshev_Lowpass_3000";
                DSP_IIRFilter filter = new DSP_IIRFilter(filterName);
                int order = filter.order;
                System.Console.WriteLine("\nTest " + filterName + ", order=" + order);

                // create impulse
                int inputLength = 400;
                double[] impulse = new double[inputLength];
                impulse[0] = 1;
                // create step funciton
                //for (int i = 1; i < inputLength; i++) impulse[i] = 1;

                double[] y;
                filter.ApplyIIRFilter(impulse, out y);
                
                //DataTools.writeArray(y);
                double myGain = 0.0;
                for (int i = 0; i < y.Length; i++) //length of signal
                {
                    myGain += Math.Abs(y[i]);
                    //myGain += (y[i] * y[i]); //power
                    y[i] *= 100;
                }
                System.Console.WriteLine("\nMy Gain (area under impulse response curve after DC gain removal.) = " + myGain);

                DataTools.writeBarGraph(y);

                System.Console.WriteLine("\nEnd Test");
            }//end test Method(string fName)



            Console.WriteLine("FINISHED!!");
            Console.ReadLine();
        }//end Main()

    }//end class DSP
}
