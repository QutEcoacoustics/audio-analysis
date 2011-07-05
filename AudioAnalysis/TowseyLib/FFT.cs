using System;
using System.Collections.Generic;
using System.Text;
//using MathNet.Numerics;
using MathNet.Numerics.Transformations;


namespace TowseyLib
{

    public enum WindowFunctions { NONE, HAMMING };



    public sealed class FFT
    {
        RealFourierTransformation rft; //only used if calling the .NET numerical math library

        public const string Key_HammingWindow = "HAMMING";

        public delegate double WindowFunc(int n, int N);

        private double windowPower; //power of the window
        public double WindowPower { get { return windowPower; } private set { windowPower = value; } }

        private int windowSize;
        public int WindowSize { get { return windowSize; } private set { windowSize = value; } }
        private int coeffCount;
        public int CoeffCount { get { return coeffCount; } private set { coeffCount = value; } }

        private double[] windowWeights;
        public double[] WindowWeights { get { return windowWeights; } private set { windowWeights = value; } }

        public FFT(int windowSize) : this(windowSize, null)
        {
        }


        /// <summary>
        /// wrapper for FFT. 
        /// Window Power equals sum of squared window values. Default window is Hamming.
        /// </summary>
        /// <param name="windowSize"></param>
        /// <param name="w"></param>
        public FFT(int windowSize, WindowFunc w)
        {
            if (!IsPowerOf2(windowSize)) throw new ArgumentException("WindowSize must be a power of 2.");

            this.WindowSize = windowSize;
            this.CoeffCount = (windowSize / 2) + 1; //f[0]=DC;  f[256]=Nyquist  

            //calculate the window weights and power
            this.WindowPower = windowSize; //the default power of a rectangular window.
            if (w != null)
            {
                //set up the FFT window
                this.WindowWeights = new double[windowSize];
                for (int i = 0; i < windowSize; i++) this.WindowWeights[i] = w(i, windowSize);

                //calculate power of the FFT window
                double power = 0.0;
                for (int i = 0; i < windowSize; i++)
                {
                    power += (this.WindowWeights[i] * this.WindowWeights[i]);
                }
                this.windowPower = power;
            }
        }



        public FFT(int windowSize, WindowFunc w, Boolean dotNetVersion)
        {
            if (!IsPowerOf2(windowSize)) throw new ArgumentException("WindowSize must be a power of 2.");

            //rft = new RealFourierTransformation();
            rft = new RealFourierTransformation(TransformationConvention.Matlab);

            this.WindowSize = windowSize;
            this.CoeffCount = (windowSize / 2) + 1; //f[0]=DC;  f[256]=Nyquist  

            //calculate the window weights and power
            this.WindowPower = windowSize; //the default power of a rectangular window.
            if (w != null)
            {
                //set up the FFT window
                this.WindowWeights = new double[windowSize];
                for (int i = 0; i < windowSize; i++) this.WindowWeights[i] = w(i, windowSize);

                //calculate power of the FFT window
                double power = 0.0;
                for (int i = 0; i < windowSize; i++)
                {
                    power += (this.WindowWeights[i] * this.WindowWeights[i]);
                }
                this.windowPower = power;
            }
        }

        /// <summary>
        /// Invokes an FFT on the given data array.
        /// cdata contains the real and imaginary terms of the coefficients representing cos and sin components respectively.
        /// cdata is symmetrical about terms 512 & 513. Can ignore all coefficients 512 and above .
        /// </summary>
        /// <param name="data">a single frame of signal values</param>
        /// <param name="coeffCount">number of coefficients to return</param>
        /// <returns></returns>
        public double[] Invoke(double[] data)
        {
            double[] cdata = new double[2 * this.WindowSize];//to contain the complex coefficients 

            //apply the window
            if (this.WindowWeights != null)
                for (int i = 0; i < this.WindowSize; i++) cdata[2 * i] = this.WindowWeights[i] * data[i];
            else
                for (int i = 0; i < this.WindowSize; i++) cdata[2 * i] = data[i];
            //do the FFT
            four1(cdata); //array contains real and imaginary values

            double[] f = new double[this.coeffCount]; //array to contain amplitude data
            for (int i = 0; i < this.coeffCount; i++) //calculate amplitude
                //f[i] = hypot(cdata[2 * i], cdata[2 * i + 1]);
                //f[i] = (cdata[2 * i] * cdata[2 * i]) + (cdata[2 * i + 1] * cdata[2 * i + 1]);
                f[i] = Math.Sqrt((cdata[2 * i] * cdata[2 * i]) + (cdata[2 * i + 1] * cdata[2 * i + 1]));

            return f;
        }


        public double[] Invoke(double[] data, int offset)
        {
            double[] cdata = new double[2 * this.WindowSize];//to contain the complex coefficients

            //apply the window
            if (this.WindowWeights != null)
                for (int i = 0; i < this.WindowSize; i++) cdata[2 * i] = this.WindowWeights[i] * data[offset + i];
            else
                for (int i = 0; i < this.WindowSize; i++) cdata[2 * i] = data[offset + i];
            //do the FFT
            four1(cdata);

            double[] f = new double[coeffCount]; //array to contain amplitude data
            for (int i = 0; i < coeffCount; i++) //calculate amplitude
                f[i] = hypot(cdata[2 * i], cdata[2 * i + 1]);

            return f;
        }

        private static double hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        // from http://www.nrbook.com/a/bookcpdf/c12-2.pdf
        private static void four1(double[] data)
        {
            int nn = data.Length / 2;
            int n = nn << 1;
            int j = 1;
            for (int i = 1; i < n; i += 2)
            {
                if (j > i)
                {
                    double tmp;
                    tmp = data[j - 1];
                    data[j - 1] = data[i - 1];
                    data[i - 1] = tmp;
                    tmp = data[j];
                    data[j] = data[i];
                    data[i] = tmp;
                }
                int m = nn;
                while (m >= 2 && j > m)
                {
                    j -= m;
                    m >>= 1;
                }
                j += m;
            }

            int mmax = 2;
            while (n > mmax)
            {
                int istep = mmax << 1;
                double theta = 2.0 * Math.PI / mmax;
                double wtemp = Math.Sin(0.5 * theta);
                double wpr = -2.0 * wtemp * wtemp;
                double wpi = Math.Sin(theta);
                double wr = 1.0;
                double wi = 0.0;
                for (int m = 1; m < mmax; m += 2)
                {
                    for (int i = m; i <= n; i += istep)
                    {
                        j = i + mmax;
                        double tempr = wr * data[j - 1] - wi * data[j];
                        double tempi = wr * data[j] + wi * data[j - 1];
                        data[j - 1] = data[i - 1] - tempr;
                        data[j] = data[i] - tempi;
                        data[i - 1] += tempr;
                        data[i] += tempi;
                    }
                    wr = (wtemp = wr) * wpr - wi * wpi + wr;
                    wi = wi * wpr + wtemp * wpi + wi;
                }
                mmax = istep;
            }
        }

        private static bool IsPowerOf2(int n)
        {
            while (n > 1)
            {
                if (n == 2) return true;
                n >>= 1;
            }
            return false;
        }

        /// <summary>
        /// This .NET FFT library was downloaded from  http://www.mathdotnet.com/Iridium.aspx
        /// The documentation and various examples of code are available at http://www.mathdotnet.com/doc/IridiumFFT.ashx
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public double[] InvokeDotNetFFT(double[] data)
        {
            if (this.WindowSize != data.Length) return null;
            //int half = WindowSize >> 1; //original dot net code returns N/2 coefficients.
            int half = this.CoeffCount;

            //apply the window
            if (WindowWeights != null) //apply the window
                for (int i = 0; i < WindowSize; i++) data[i] = WindowWeights[i] * data[i]; //window

            double[] freqReal, freqImag;
            rft.TransformForward(data, out freqReal, out freqImag);

            double[] amplitude = new double[half];
            for (int i = 0; i < half; i++)
                amplitude[i] = Math.Sqrt((freqReal[i] * freqReal[i]) + (freqImag[i] * freqImag[i]));
            return amplitude;
        }




        #region Window functions
        // from http://en.wikipedia.org/wiki/Window_function

        public static readonly WindowFunc Hamming = delegate(int n, int N)
        {
            double x = 2.0 * Math.PI * n / (N - 1);
            //return 0.53836 - 0.46164 * Math.Cos(x);
            return 0.54 - 0.46 * Math.Cos(x); //MATLAB code uses these value and says it is better!
        };

        public static WindowFunc Gauss(double sigma)
        {
            if (sigma <= 0.0 || sigma > 0.5) throw new ArgumentOutOfRangeException("sigma");
            return delegate(int n, int N)
            {
                double num = n - 0.5 * (N - 1);
                double den = sigma * 0.5 * (N - 1);
                double quot = num / den;
                return Math.Exp(-0.5 * quot * quot);
            };
        }

        public static readonly WindowFunc Lanczos = delegate(int n, int N)
        {
            double x = 2.0 * n / (N - 1) - 1.0;
            return x != 0.0 ? Math.Sin(x) / x : 1.0;
        };

        public static readonly WindowFunc Nuttall = delegate(int n, int N) { return lrw(0.355768, 0.487396, 0.144232, 0.012604, n, N); };

        public static readonly WindowFunc BlackmanHarris = delegate(int n, int N) { return lrw(0.35875, 0.48829, 0.14128, 0.01168, n, N); };

        public static readonly WindowFunc BlackmanNuttall = delegate(int n, int N) { return lrw(0.3635819, 0.4891775, 0.1365995, 0.0106411, n, N); };

        private static double lrw(double a0, double a1, double a2, double a3, int n, int N)
        {
            double c1 = Math.Cos(2.0 * Math.PI * n / (N - 1));
            double c2 = Math.Cos(4.0 * Math.PI * n / (N - 1));
            double c3 = Math.Cos(6.0 * Math.PI * n / (N - 1));
            return a0 - a1 * c1 + a2 * c2 - a3 * c3;
        }

        public static readonly WindowFunc FlatTop = delegate(int n, int N)
        {
            double c1 = Math.Cos(2.0 * Math.PI * n / (N - 1));
            double c2 = Math.Cos(4.0 * Math.PI * n / (N - 1));
            double c3 = Math.Cos(6.0 * Math.PI * n / (N - 1));
            double c4 = Math.Cos(8.0 * Math.PI * n / (N - 1));
            return 1.0 - 1.93 * c1 + 1.29 * c2 - 0.388 * c3 + 0.032 * c4;
        };
        #endregion

        public static FFT.WindowFunc GetWindowFunction(string name)
        {
            //FFT.WindowFunc windowFnc;
            if (name.StartsWith(Key_HammingWindow)) return FFT.Hamming;
            else return null;
        }
    }//end class FFT
}