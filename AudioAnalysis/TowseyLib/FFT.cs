using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
    public sealed class FFT
    {
        public delegate double WindowFunc(int n, int N);

        //public int WindowSize { get; private set; }
        private int windowSize;
        public int WindowSize { get { return windowSize; } private set { windowSize = value; } }

        //public double[] WindowWeights { get; private set; }
        private double[] windowWeights;
        public double[] WindowWeights { get { return windowWeights; } private set { windowWeights = value; } }

        public FFT(int windowSize)
            : this(windowSize, null)
        {
        }

        public FFT(int windowSize, WindowFunc w)
        {
            if (!IsPowerOf2(windowSize)) throw new ArgumentException("WindowSize must be a power of 2.");

            this.WindowSize = windowSize;
            if (w != null)
            {
                this.WindowWeights = new double[windowSize];
                for (int i = 0; i < windowSize; i++)
                    this.WindowWeights[i] = w(i, windowSize);
            }
        }

        public double[] Invoke(double[] data, int offset)
        {
            double[] cdata = new double[2 * WindowSize];
            if (WindowWeights != null)
                for (int i = 0; i < WindowSize; i++)
                    cdata[2 * i] = WindowWeights[i] * data[offset + i];
            else
                for (int i = 0; i < WindowSize; i++)
                    cdata[2 * i] = data[offset + i];

            four1(cdata);

            double[] f = new double[WindowSize / 2 + 1];
            for (int i = 0; i < WindowSize / 2 + 1; i++)
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




        #region Window functions
        // from http://en.wikipedia.org/wiki/Window_function

        public static readonly WindowFunc Hamming = delegate(int n, int N)
        {
            double x = 2.0 * Math.PI * n / (N - 1);
            return 0.53836 - 0.46164 * Math.Cos(x);
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
            if (name.StartsWith("Hamming")) return FFT.Hamming;
            else return null;
        }
    }//end class FFT
}
