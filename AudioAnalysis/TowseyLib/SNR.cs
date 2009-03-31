using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLib
{
    public class SNR
    {

        //reference logEnergies for signal segmentation, energy normalisation etc
        public const double MinEnergyReference = -7.0;      // typical noise value for BAC2 recordings = -4.5
        public const double MaxEnergyReference = -0.60206;  // = Math.Log10(0.25) which assumes max average frame amplitude = 0.5
        //public const double MaxLogEnergy = -0.444;        // = Math.Log10(0.36) which assumes max average frame amplitude = 0.6
        //public const double MaxLogEnergy = -0.310;        // = Math.Log10(0.49) which assumes max average frame amplitude = 0.7
        //public const double MaxLogEnergy = 0.0;           // = Math.Log10(1.00) which assumes max frame amplitude = 1.0
        //note that the cicada recordings reach max average frame amplitude = 0.55



        public double[] LogEnergy {get; set;}
        public double[] Decibels {get; set;}
        public double Min_dB {get; set;}
        public double Max_dB {get; set;}
        public double minEnergyRatio {get; set;}
        public double NoiseSubtracted { get; set; } //the modal noise in dB
        public double NoiseRange {get; set;}        //difference between min_dB and the modal noise dB
        public double MaxReference_dBWrtNoise {get; set;} //max reference dB wrt modal noise = 0.0
        public double Snr { get; set; }             //sig/noise ratio i.e. max dB wrt modal noise = 0.0

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="frames"></param>
        public SNR(double[,] frames)
        {
            this.LogEnergy = SignalLogEnergy(frames);
            CalculateDecibelsPerFrame();
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
                    logEnergy[i] = SNR.MinEnergyReference - SNR.MaxEnergyReference; //normalise to absolute scale
                    continue;
                }
                double logE = Math.Log10(e);
                //if(i==100) Console.ReadLine();

                //normalise to ABSOLUTE energy value i.e. as defined in header of Sonogram class
                if (logE < SNR.MinEnergyReference)
                {
                    //System.Console.WriteLine("DSP.SignalLogEnergy() NOTE!!! LOW LogEnergy[" + i + "]=" + logEnergy[i].ToString("F6"));
                    logEnergy[i] = SNR.MinEnergyReference - SNR.MaxEnergyReference;
                }
                else logEnergy[i] = logE - SNR.MaxEnergyReference;
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
        public static double[] NoiseSubtract(double[] logEnergy, out double min_dB, out double max_dB, out double Q)
        {
            //Following const used to normalise the logEnergy values to the background noise.
            //Has the effect of setting background noise level to 0 dB.
            //Value of 10dB is in Lamel et al, 1981. They call it "Adaptive Level Equalisatsion".
            const double noiseThreshold_dB = 10.0; //dB
            double minEnergyRatio = SNR.MinEnergyReference - SNR.MaxEnergyReference;


            //ignore first N and last N frames when calculating background noise level because sometimes these frames
            // have atypically low signal values
            int buffer = 20; //ignore first N and last N frames when calculating background noise level

            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            //Console.WriteLine("minFractionEnergy = " + minFraction);
            for (int i = buffer; i < logEnergy.Length - buffer; i++)
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
                    }
                    else
                        if (id < 0) id = 0;
                    histo[id]++;
                }
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            int peakID = DataTools.GetMaxIndex(smoothHisto);
            Q = min_dB + ((peakID + 1) * binWidth); //modal noise level

            // subtract noise energy` and return relative energy as decibel values.
            double[] dBFrames = new double[L];
            for (int i = 0; i < L; i++) dBFrames[i] = (logEnergy[i] * 10) - Q;
            //Console.WriteLine("minDB=" + min_dB + "  max_dB=" + max_dB);
            //Console.WriteLine("peakID=" + peakID + "  Q=" + Q);

            return dBFrames;
        }

        /// <summary>
        /// subtract background noise to produce a decibels array in which zero dB = modal noise
        /// </summary>
        /// <param name="logEnergy"></param>
        /// <returns></returns>
        public void CalculateDecibelsPerFrame()
        {
            double Q;
            double min_dB;
            double max_dB;
            this.Decibels = NoiseSubtract(this.LogEnergy, out min_dB, out max_dB, out Q);
            this.NoiseSubtracted = Q;
            this.Min_dB = min_dB; //min decibels of all frames 
            this.Max_dB = max_dB;
            this.NoiseRange = min_dB - Q;
            this.Snr = max_dB - Q;
            //need an appropriate dB reference level for normalising dB arrays.
            //this.MaxReference_dBWrtNoise = (SNR.MaxEnergyReference * 10) - Q;  // NO GOOD!
            //this.MaxReference_dBWrtNoise = max_dB - Q;                         // OK
            this.MaxReference_dBWrtNoise = max_dB - min_dB;                      // BEST BECAUSE TAKES NOISE LEVEL INTO ACCOUNT
        }

        public double FractionHighEnergyFrames(double dbThreshold)
        {
            return FractionHighEnergyFrames(this.Decibels, dbThreshold);
        }

        public static double FractionHighEnergyFrames(double[] dbArray, double dbThreshold)
        {
            int L = dbArray.Length;
            int count = 0;
            for (int i = 0; i < L; i++) //foreach time step
            {
                if (dbArray[i] > dbThreshold) count++;
            }
            return (count / (double)L);
        }

        public double[] NormaliseDecibelArray()
        {
            return NormaliseDecibelArray(this.Decibels, this.MaxReference_dBWrtNoise);
        }

        /// <summary>
        /// normalise the power values using the passed reference decibel levels
        /// NOTE: This method assumes that the energy values are in decibels and that they have been scaled
        /// so that the modal noise value = 0 dB. Simply truncate all values below this to zero dB
        /// </summary>
        /// <param name="energy"></param>
        /// <param name="maxDecibels"></param>
        /// <returns></returns>
        public static double[] NormaliseDecibelArray(double[] dB, double maxDecibels)
        {
            //normalise power between 0.0 decibels and max decibels.
            int L = dB.Length;
            double[] E = new double[L];
            for (int i = 0; i < L; i++)
            {
                E[i] = dB[i];
                if (E[i] <= 0.0) E[i] = 0.0;
                else E[i] = dB[i] / maxDecibels;
                if (E[i] > 1.0) E[i] = 1.0;
            }
            return E;
        }

    }// end class
}
