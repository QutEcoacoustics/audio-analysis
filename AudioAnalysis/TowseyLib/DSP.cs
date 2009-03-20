﻿using System;
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


        //reference logEnergies for signal segmentation, energy normalisation etc
        public const double MinEnergyReference = -7.0;      // typical noise value for BAC2 recordings = -4.5
        public const double MaxEnergyReference = -0.60206;  // = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
        //public const double MaxLogEnergy = -0.444;        // = Math.Log10(0.36) which assumes max average frame amplitude = 0.6
        //public const double MaxLogEnergy = -0.310;        // = Math.Log10(0.49) which assumes max average frame amplitude = 0.7
        //public const double MaxLogEnergy = 0.0;           // = Math.Log10(1.00) which assumes max frame amplitude = 1.0
        //note that the cicada recordings reach max average frame amplitude = 0.55


        /// <summary>
        /// Breaks a long audio signal into frames with given step
        /// </summary>
        public static double[,] Frames(double[] data, int windowSize, int step)
        {
            if (step < 1)
                throw new ArgumentException("Frame Step must be at least 1");
            if (step > windowSize)
                throw new ArgumentException("Frame Step must be <=" + windowSize);

            int framecount = (data.Length - windowSize) / step; //this truncates residual samples
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

        public static double[] GetSignal(int sampleRate, double duration, int[] freq)
        {
            double ampl = 10000;
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
                for (int f = 0; f < count; f++) data[i] += (ampl * Math.Sin(2.0 * Math.PI * freq[f] * i / (double)sampleRate));
            }
            return data;
        }


        /// <summary>
        /// The source signal for voiced speech, that is, the vibration generated by the glottis or vocal chords,
        /// has a spectral content with more power in low freq than in high. The spectrum has roll off of -6dB/octave.
        /// Many speech analysis methods work better when the souce signal is spectrally flattened.
        /// This is achieved by a high pass filter.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="coeff"></param>
        /// <returns></returns>
        public static double[] PreEmphasis(double[] signal, double coeff)
        {
            int L = signal.Length;
            double[] newSig = new double[L - 1];
            for (int i = 0; i < L - 1; i++) newSig[i] = signal[i + 1] - (coeff * signal[i]);
            return newSig;
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
        /// Frame energy is the log of the summed energy of the samples.
        /// Normally, if the passed frames are FFT spectra, then would multiply by 2 because spectra are symmetrical about Nyquist.
        /// BUT this method returns the AVERAGE sample energy, which therefore normalises for frame length / sample number. 
        /// 
        /// Energy normalisation formula taken from Lecture Notes of Prof. Bryan Pellom
        /// Automatic Speech Recognition: From Theory to Practice.
        /// http://www.cis.hut.fi/Opinnot/T-61.184/ September 27th 2004.
        /// 
        /// Calculate normalised energy of frame as  energy[i] = logEnergy - maxLogEnergy;
        /// This is same as log10(logEnergy / maxLogEnergy) ie normalised to a fixed maximum energy value.
        /// </summary>
        /// <param name="frames">a matrix containing signal values grouped as overlapping frames</param>
        /// <param name="minLogEnergy">an arbitrary minimum to prevent large negative log values</param>
        /// <param name="maxLogEnergy">absolute max to which we normalise</param>
        /// <returns></returns>
        public static double[] SignalLogEnergy(double[,] frames)
        {
            int frameCount = frames.GetLength(0);
            int N = frames.GetLength(1);
            double[] logEnergy = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double sum = 0.0;
                for (int j = 0; j < N; j++)  //foreach sample in frame
                {
                    sum += (frames[i, j] * frames[i, j]); //sum the energy = amplitude squared
                }
                double e = sum / (double)N; //normalise to frame size i.e. average energy per sample
                //Console.WriteLine("e=" + e);
                //if (e > 0.25) Console.WriteLine("e > 0.25 = " + e);

                if (e == Double.MinValue) //to guard against log(0) but this should never happen!
                //if (e == 0.0000000000) //to guard against log(0) but this should never happen!
                {
                    System.Console.WriteLine("DSP.SignalLogEnergy() Warning!!! Zero Energy in frame " + i);
                    logEnergy[i] = DSP.MinEnergyReference - DSP.MaxEnergyReference; //normalise to absolute scale
                    continue;
                }
                double logE = Math.Log10(e);
                //if(i==100) Console.ReadLine();

                //normalise to ABSOLUTE energy value i.e. as defined in header of Sonogram class
                if (logE < DSP.MinEnergyReference)
                {
                    //System.Console.WriteLine("DSP.SignalLogEnergy() NOTE!!! LOW LogEnergy[" + i + "]=" + logEnergy[i].ToString("F6"));
                    logEnergy[i] = DSP.MinEnergyReference - DSP.MaxEnergyReference;
                }
                else logEnergy[i] = logE - DSP.MaxEnergyReference;
                //if (logE > maxLogEnergy) Console.WriteLine("logE > maxLogEnergy - " + logE +">"+ maxLogEnergy);
                //if (i < 20) Console.WriteLine("e="+logEnergy[i]);
            }

            //normalise to RELATIVE energy value i.e. max in the current frame
            //double maxEnergy = energy[DataTools.getMaxIndex()];
            //for (int i = 0; i < frameCount; i++) //foreach time step
            //{
            //    //energy[i] = ((energy[i] - maxEnergy) * 0.1) + 1.0; //see method header for reference 
            //}
            return logEnergy;
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

        
         
        /// <summary>
        /// This method subtracts the estimated background noise from the frame energies and converts all values to dB.
        /// algorithm described in Lamel et al, 1981.
        /// NOTE: noiseThreshold is passed as decibels
        /// energy array is log energy ie not yet converted to decibels.
        /// Return energy converted to decibels i.e. multiply by 10.
        /// </summary>
        /// <param name="logEnergy">NOTE: the log energy values are normalised to global constants</param>
        /// <param name="min_dB"></param>
        /// <param name="max_dB"></param>
        /// <param name="noiseThreshold_dB"></param>
        /// <param name="Q">noise in decibels subtracted from each frame</param>
        /// <returns></returns>
        public static double[] NoiseSubtract(double[] logEnergy, out double min_dB, out double max_dB, double minEnergyRatio, out double Q)
        {
            //Following const used to normalise the logEnergy values to the background noise.
            //Has the effect of setting background noise level to 0 dB.
            //Value of 10dB is in Lamel et al, 1981. They call it "Adaptive Level Equalisatsion".
            const double noiseThreshold_dB = 10.0; //dB


            //ignore first N and last N frames when calculating background noise level because sometimes these frames
            // have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level

            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            //Console.WriteLine("minFractionEnergy = " + minFraction);
            for (int i = buffer; i < logEnergy.Length-buffer; i++)
            {
                if (logEnergy[i] == minEnergyRatio) continue; //ignore lowest values in establishing noise level
                if (logEnergy[i] < min) min = logEnergy[i];
                else
                if (logEnergy[i] > max) max = logEnergy[i];
            }
            min_dB = min * 10;  //multiply by 10 to convert to decibels
            max_dB = max * 10;

            int binCount = 100;
            double binWidth = noiseThreshold_dB / binCount;
            int[] histo = new int[binCount];
            int L = logEnergy.Length;
            double absThreshold = min_dB + noiseThreshold_dB;

            for (int i = 0; i < L; i++)
            {
                double dB = 10 * logEnergy[i];
                if (dB <= absThreshold)
                {
                    int id = (int)((dB - min_dB) / binWidth);
                    if (id >= binCount)
                    {
                        id = binCount - 1;
                    } else
                    if (id < 0) id = 0;
                    histo[id]++;    
                }
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            int peakID = DataTools.GetMaxIndex(smoothHisto);
            Q = min_dB + ((peakID+1) * binWidth); //modal noise level
            
            // subtract noise energy` and return relative energy as decibel values.
            double[] en = new double[L];
            for (int i = 0; i < L; i++) en[i] = (logEnergy[i]*10) - Q;
            //Console.WriteLine("minDB=" + min_dB + "  max_dB=" + max_dB);
            //Console.WriteLine("peakID=" + peakID + "  Q=" + Q);

            return en;
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
