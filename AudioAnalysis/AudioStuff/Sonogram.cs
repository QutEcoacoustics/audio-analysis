using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using TowseyLib;

namespace AudioStuff
{
	public sealed class Sonogram
	{
        public const int binWidth = 1000; //1 kHz bands for calculating acoustic indices 



        private SonoConfig state = new SonoConfig();  //class containing state of all application parameters
        public SonoConfig State { get { return state; } set { state = value; } }

        public string BmpFName { get { return state.BmpFName; } }
 


        private double[,] matrix; //the original sonogram
        public  double[,] Matrix { get { return matrix; } /*set { matrix = value; }*/ }
        private double[,] gradM; //the gradient version of the sonogram
        public  double[,] GradM  { get { return gradM; } /*set { gradM = value; }*/ }
        private double[,] melFM; //the Mel Frequency version of the sonogram
        public  double[,] MelFM  { get { return melFM; } /*set { melFM = value; }*/ }
        private double[,] cepsM; //the Mel Frequency Cepstral version of the sonogram
        public  double[,] CepsM  { get { return cepsM; } /*set { cepsM = value; }*/ }
        private double[,] shapeM; //the Shape outline version of the sonogram
        public  double[,] ShapeM { get { return shapeM; } set { shapeM = value; } }


        //  RESULTS variables
        //public double NoiseAv { get { return results.NoiseAv; } }
        //public double NoiseSD { get { return results.NoiseSd; } }
        //public double[] ActivityHisto { get { return results.ActivityHisto; } }


        //****************************************************************************************************
        //****************************************************************************************************
        //****************************************************************************************************
        //  CONSTRUCTORS
        

        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="props"></param>
        /// <param name="wav"></param>
        public Sonogram(Configuration cfg, WavReader wav)
        {
            state.ReadConfig(cfg);
            state.WavFileDir = wav.WavFileDir;
            state.WavFName = wav.WavFileName;
            state.WavFName = state.WavFName.Substring(0, state.WavFName.Length - 4);
            state.SignalMax = wav.GetMaxValue();
            state.SetDateAndTime(state.WavFName);
            if (wav.GetMaxValue() == 0.0) throw new ArgumentException("Wav file has zero signal");
            Make(wav);
            if(state.Verbosity!=0) WriteInfo();
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavFName"></param>
        public Sonogram(string iniFName, string wavPath)
        {
            state.ReadConfig(iniFName);

            FileInfo fi = new FileInfo(wavPath);
            state.WavFileDir = fi.DirectoryName;
            state.WavFName = fi.Name.Substring(0, fi.Name.Length - 4);
            state.WavFileExt = fi.Extension;
            state.SetDateAndTime(state.WavFName);

            //read the .WAV file
            WavReader wav = new WavReader(wavPath);
            if (wav.GetMaxValue() == 0.0) throw new ArgumentException("Wav file has zero signal");
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }

        /// <summary>
        /// CONSTRUCTOR 3
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavPath"></param>
        /// <param name="wavBytes"></param>
        /// <returns></returns>
        public Sonogram(string iniFName, string wavPath, byte[] wavBytes)
        {
            state.ReadConfig(iniFName);

            FileInfo fi = new FileInfo(wavPath);
            state.WavFileDir = fi.DirectoryName;
            state.WavFName = fi.Name.Substring(0, fi.Name.Length - 4);
            state.WavFileExt = fi.Extension;
            state.SetDateAndTime(state.WavFName);

            //initialise WAV class with bytes array
            WavReader wav = new WavReader(wavBytes, state.WavFName);
            if (wav.GetMaxValue() == 0.0) throw new ArgumentException("Wav file has zero signal");
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }

        /// <summary>
        /// CONSTRUCTOR 4
        /// </summary>
        /// <param name="iniFName"></param>
        /// <param name="wavPath"></param>
        /// <param name="rawData"></param>
        /// <param name="sampleRate"></param>
        public Sonogram(string iniFName, string sigName, double[] rawData, int sampleRate)
        {
            state.ReadConfig(iniFName);
            state.WavFName = sigName;
            state.WavFileExt = "sig";

            //initialise WAV class with double array
            WavReader wav = new WavReader(rawData, sampleRate, sigName);
            if (wav.GetMaxValue() == 0.0) throw new ArgumentException("Wav file has zero signal");
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }

        private void Make(WavReader wav)
        {
            //store essential parameters for this sonogram
            this.state.SignalMax  = wav.GetMaxValue();
            this.state.WavFName   = wav.WavFileName;
            this.state.SampleRate = wav.SampleRate;
            this.state.SampleCount = wav.SampleLength;
            this.state.AudioDuration = state.SampleCount / (double)state.SampleRate;
            this.state.MaxFreq = state.SampleRate / 2;
            this.state.WindowDuration = state.WindowSize / (double)state.SampleRate; // window duration in seconds
            this.state.NonOverlapDuration = this.state.WindowDuration * (1 - this.state.WindowOverlap);// duration in seconds
            this.state.FreqBinCount = this.state.WindowSize / 2; // other half is phase info
            this.state.FBinWidth = this.state.MaxFreq / (double)this.state.FreqBinCount;
            this.state.SpectrumCount = (int)(this.state.AudioDuration / this.state.NonOverlapDuration);
            this.state.SpectraPerSecond = 1 / this.state.NonOverlapDuration;

            // init the class which calculates the FFT
            FFT fft = new FFT(this.state.WindowSize, this.state.WindowFnc);
            int step = (int)(this.state.WindowSize * (1 - this.state.WindowOverlap));

            //generate the spectrum
            double min;
            double max;
            double avg;
            this.matrix = GenerateSpectrogram(wav, fft, step, out min, out avg, out max);
            this.state.MinPower = min;
            this.state.MaxPower = max;
            this.state.AvgPower = avg;
            //this.state.MinCut = 0.0;
            //this.state.MaxCut = 1.0;
            NormalizeAndBound();  //normalise and bound the values

            this.state.SpectrumCount = this.matrix.GetLength(0);
        }

        public double[,] GenerateSpectrogram(WavReader wav, FFT fft, int step, out double min, out double avg, out double max)
		{
            if (step < 1)
                throw new ArgumentException("Frame Step must be at least 1");
            if (step > fft.WindowSize)
                throw new ArgumentException("Frame Step must be <=" + fft.WindowSize);


			double[] data = wav.Samples;
            int width     = (data.Length - fft.WindowSize) / step;
            if (width < 2) throw new ArgumentException("Sonogram width must be at least 2");
            int height = fft.WindowSize / 2;

            //calculate a minimum amplitude to prevent taking log of small number
            //this would increase the range when normalising
            double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
            int smoothingWindow = 5; //to smooth the spectrum 
	
			double offset = 0.0;
			double[,] sonogram = new double[width, height];
			min = Double.MaxValue; 
            max = Double.MinValue;
            double sum = 0.0;

			for (int i = 0; i < width; i++)//foreach time step
			{
				double[] f1 = fft.Invoke(data, (int)Math.Floor(offset));
                f1 = DataTools.filterMovingAverage(f1, smoothingWindow); //to smooth the spectrum - reduce variance
                for (int j = 0; j < height; j++) //foreach freq bin
				{
                    double amplitude = f1[j + 1];
                    if (amplitude < epsilon) amplitude = epsilon; //to prevent possible log of a very small number
                    double power = amplitude * amplitude; //convert amplitude to power
                    power = 10 * Math.Log10(power);    //convert to decibels
                    ////NOTE: the decibels calculation should be a ratio. 
                    //// Here the ratio is implied ie relative to the power in the normalised wav signal
                    if (power < min) min = power;
                    else
                    if (power > max) max = power;
                    sonogram[i, j] = power;
                    sum += power;
				}
				offset += step;
			} //end matrix
            avg = sum / (width * height);
            return sonogram;
		}



        // following method not used
        //public double[] GetPowerSpectrum(double[,] f2, double max)
        //{
        //    int width = f2.GetLength(0);
        //    int height = f2.GetLength(1);
        //    double[] f1 = new double[height];

        //    for (int y = 0; y < height; y++)
        //    {
        //        double sum = 0.0;
        //        for (int x = 0; x < width; x++)
        //            sum += Math.Pow(10.0, 2.0 * (f2[x, y] - max));
        //        f1[y] = Math.Log10(sum / width) + 2.0 * max;
        //    }
        //    return f1;
        //}




        public void Gradient()
        {
            double gradThreshold = 2.0;
            int fWindow = 11;
            int tWindow = 9;
            double[,] blurM = ImageTools.Blur(this.matrix, fWindow, tWindow);
            int height = blurM.GetLength(0);
            int width  = blurM.GetLength(1);
            this.gradM = new double[height, width];

            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            for (int x = 0; x < width; x++) this.gradM[0, x] = 0.5; //patch in first  time step with zero gradient
            for (int x = 0; x < width; x++) this.gradM[1, x] = 0.5; //patch in second time step with zero gradient
           // for (int x = 0; x < width; x++) this.gradM[2, x] = 0.5; //patch in second time step with zero gradient

            for (int y = 2; y < height - 1; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double grad1 = blurM[y, x] - blurM[y - 1, x];//calculate one step gradient
                    double grad2 = blurM[y, x] - blurM[y - 2, x];//calculate two step gradient

                    //get min and max gradient
                    if (grad1 < min) min = grad1;
                    else
                    if (grad1 > max) max = grad1;

                    // quantize the gradients
                    if (grad1 < -gradThreshold) this.gradM[y, x] = 0.0;
                    else
                        if (grad1 > gradThreshold) this.gradM[y, x] = 1.0;
                        else
                            if (grad2 < -gradThreshold) this.gradM[y, x] = 0.0;
                            else
                                if (grad2 > gradThreshold) this.gradM[y, x] = 1.0;
                                else this.gradM[y, x] = 0.5;
                }
            }

            //results.MinGrad = min;
            //results.MaxGrad = max;

            //for (int x = 0; x < width; x++) this.gradM[height - 1, x] = 0.5; //patch in last time step with medium gradient
            this.state.MinCut = 0.0;
            this.state.MaxCut = 1.0;
        }


        public void MelFreqSonogram(int melBandCount)
        {
            int M = this.Matrix.GetLength(0); //number of spectra or time steps
            int N = this.Matrix.GetLength(1); //number of Hz bands
            double[,] outData = new double[M, melBandCount];
            double Nyquist    = this.state.MaxFreq;
            double linBand = this.State.FBinWidth;
            double melBand = Speech.Mel(Nyquist) / (double)melBandCount;  //width of mel band
            double min = double.PositiveInfinity; //to obtain mel min and max
            double max = double.NegativeInfinity;

            for (int i = 0; i < M; i++) //for all spectra or time steps
                for (int j = 0; j < melBandCount; j++) //for all mel bands
                {
                    double a = Speech.InverseMel(j * melBand) / linBand;       //location of lower f in Hz bin units
                    double b = Speech.InverseMel((j + 1) * melBand) / linBand; //location of upper f in Hz bin units
                    int ai = (int)Math.Ceiling(a);
                    int bi = (int)Math.Floor(b);

                    double sum = 0.0;

                    if (bi < ai) //a and b are in same Hz band
                    {
                        ai = (int)Math.Floor(a);
                        bi = (int)Math.Ceiling(b);
                        double ya = Speech.LinearInterpolate((double)ai, bi, this.Matrix[i, ai], this.Matrix[i, bi], a);
                        double yb = Speech.LinearInterpolate((double)ai, bi, this.Matrix[i, ai], this.Matrix[i, bi], b);
                        //sum = Speech.LinearIntegral(a, b, ya, yb);
                        sum = Speech.MelIntegral(a * linBand, b * linBand, ya, yb);
                    }
                    else
                    {
                        if (ai > 0)
                        {
                            double ya = Speech.LinearInterpolate((double)(ai - 1), (double)ai, this.Matrix[i, ai - 1], this.Matrix[i, ai], a);
                            //sum += Speech.LinearIntegral(a, (double)ai, ya, this.Matrix[i, ai]);
                            sum += Speech.MelIntegral(a * linBand, ai * linBand, ya, this.Matrix[i, ai]);
                        }
                        for (int k = ai; k < bi; k++)
                        {
                            sum += Speech.MelIntegral(k * linBand, (k + 1) * linBand, this.Matrix[i, k], this.Matrix[i, k + 1]);
                            //sum += Speech.LinearIntegral(k, (k + 1), this.Matrix[i, k], this.Matrix[i, k + 1]);
                        }
                        if (bi < (N - 1)) //this.Bands in Greg's original code
                        {
                            double yb = Speech.LinearInterpolate((double)bi, (double)(bi+1), this.Matrix[i, bi], this.Matrix[i, bi+1], b);
                            sum += Speech.MelIntegral(bi * linBand, b * linBand, this.Matrix[i, bi], yb);
                            //sum += Speech.LinearIntegral((double)bi, b, this.Matrix[i, bi], yb);
                        }
                    }
                    sum /= melBand; //to obtain power per mel

                    outData[i, j] = sum;
                    if (sum < min) min = sum;
                    if (sum > max) max = sum;
                }
            this.melFM = outData;
            this.State.MelBinCount = melBandCount;
            this.State.MinMelPower = min;
            this.State.MaxMelPower = max;
            this.State.MaxMel = Speech.Mel(Nyquist);
            //return new Spectrum() { Data = outData, Min = min, Epsilon = this.Epsilon, Max = max, Nyquist = Mel(Nyquist) };
        }



        public void CepstralSonogram(double[,] sMatrix)
        {
            int M = sMatrix.GetLength(0); //number of spectra or time steps
            int inN = sMatrix.GetLength(1); //number of Hz or mel bands
			int outN = inN / 2 + 1;
			double[,] outData = new double[M, outN];
            FFT fft = new FFT(inN);
            double min = Double.MaxValue, max = 0.0;

			for (int i = 0; i < M; i++) //for all time steps or spectra
			{
				double[] inSpectrum = new double[inN];
                for (int j = 0; j < inN; j++) inSpectrum[j] = sMatrix[i, j];
				double[] outSpectrum = fft.Invoke(inSpectrum, 0);
				for (int j = 0; j < outN; j++)
				{
					double amplitude = outSpectrum[j];
					if (amplitude < min) min = amplitude;
					if (amplitude > max) max = amplitude;
					outData[i, j] = amplitude;
				}
			}

            this.cepsM = outData;
            this.State.CepBinCount = outN;
            this.State.MinCepPower = min;
            this.State.MaxCepPower = max;
            //return new Spectrum() { Data = outData, Min = min, Epsilon = double.NaN, Max = max, Nyquist = double.NaN };
        }


        public double[] CalculatePowerHisto()
        {
            int bandCount = this.State.MaxFreq / Sonogram.binWidth;
            this.State.FreqBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.matrix.GetLength(0); //time dimension
            int width = this.matrix.GetLength(1);
            double[] power = new double[bandCount];


            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 0; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        power[f] += this.matrix[y, x]; //sum the power
                    }
                }

            }

            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                histo[f] = power[f] / (double)tracksPerBand / state.SpectrumCount;
            }
            return histo;
        }


        public double[] CalculateEventHisto()
        {
            int bandCount = this.State.MaxFreq / Sonogram.binWidth;
            this.State.FreqBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.matrix.GetLength(0); //time dimension
            int width = this.matrix.GetLength(1);
            int[] counts = new int[bandCount];

            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 1; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        if (this.gradM[y, x] != this.gradM[y-1, x]) counts[f]++; //count any gradient change
                    }
                }
            }
            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                histo[f] = counts[f] / (double)tracksPerBand / state.AudioDuration;
            }
            return histo;
        }
        public double[] CalculateEvent2Histo()
        {
            int bandCount = this.State.MaxFreq / Sonogram.binWidth;
            this.State.FreqBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.matrix.GetLength(0); //time dimension
            int width  = this.matrix.GetLength(1);
            double[] positiveGrad = new double[bandCount];
            double[] negitiveGrad = new double[bandCount];


            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 0; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        double d = this.gradM[y,x];
                        if (d == 0) negitiveGrad[f]++;
                        else if (d == 1) positiveGrad[f]++;
                    }
                }
            }
            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                if (positiveGrad[f] > negitiveGrad[f]) histo[f] = positiveGrad[f] / (double)tracksPerBand / state.AudioDuration;
                else                                   histo[f] = negitiveGrad[f] / (double)tracksPerBand / state.AudioDuration;
            }
            return histo;
        }

        public double[] CalculateActivityHisto()
        {
            int bandCount = this.State.MaxFreq / Sonogram.binWidth;
            this.State.FreqBandCount = bandCount;
            int tracksPerBand = this.State.FreqBinCount / bandCount;
            int height = this.matrix.GetLength(0); //time dimension
            int width = this.matrix.GetLength(1);
            double[] activity = new double[bandCount];


            for (int f = 0; f < bandCount; f++) // over all 11 bands
            {
                int minTrack = f * tracksPerBand;
                int maxTrack = ((f + 1) * tracksPerBand) - 1;
                for (int y = 0; y < height; y++) //full duration of recording
                {
                    for (int x = minTrack; x < maxTrack; x++) //full width of freq band
                    {
                        activity[f] += (this.gradM[y, x] * this.gradM[y, x]); //add square of gradient
                    }
                }

            }

            double[] histo = new double[bandCount];
            for (int f = 0; f < bandCount; f++)
            {
                histo[f] = activity[f] / (double)tracksPerBand / state.AudioDuration;
            }
            return histo;
        }


        public void CalculateIndices()
        {
            Gradient();
//            this.EventHisto   = CalculateEventHisto(); //calculate statistics
//            this.PowerHisto   = CalculatePowerHisto();
//            this.EventEntropy = DataTools.RelativeEntropy(DataTools.NormaliseProbabilites(this.EventHisto));
        }

        //normalise and compress/bound the values
        public void NormalizeAndBound()
        {
            double minCut;
            double maxCut;
            DataTools.PercentileCutoffs(this.matrix, this.state.MinPercentile, this.state.MaxPercentile, out minCut, out maxCut);
            this.state.MinCut = minCut;
            this.state.MaxCut = maxCut;
            this.matrix = DataTools.boundMatrix(this.matrix, minCut, maxCut);
        }


        public void WriteInfo()
        {
            Console.WriteLine("\nSONOGRAM INFO");
            Console.WriteLine(" WavSampleRate=" + this.state.SampleRate + " SampleCount=" + this.state.SampleCount + "  Duration=" + (this.state.SampleCount / (double)this.state.SampleRate).ToString("F3") + "s");
            Console.WriteLine(" Window Size=" + this.state.WindowSize + "  Max FFT Freq =" + this.state.MaxFreq);
            Console.WriteLine(" Window Overlap=" + this.state.WindowOverlap + " Window duration=" + this.state.WindowDuration + "ms. (non-overlapped=" + this.state.NonOverlapDuration + "ms)");
            Console.WriteLine(" Freq Bin Width=" + (this.state.MaxFreq / (double)this.state.FreqBinCount).ToString("F3") + "hz");
            Console.WriteLine(" Min power=" + this.state.MinPower.ToString("F3") + " Avg power=" + this.state.AvgPower.ToString("F3") + " Max power=" + this.state.MaxPower.ToString("F3"));
            Console.WriteLine(" Min percentile=" + this.state.MinPercentile.ToString("F2") + "  Max percentile=" + this.state.MaxPercentile.ToString("F2"));
            Console.WriteLine(" Min cutoff=" + this.state.MinCut.ToString("F3") + "  Max cutoff=" + this.state.MaxCut.ToString("F3"));
        }

        public void WriteStatistics()
        {
            Console.WriteLine("\nSONOGRAM STATISTICS");
            Console.WriteLine(" Max power=" + this.State.MaxPower.ToString("F3") + " dB");
            Console.WriteLine(" Avg power=" + this.State.AvgPower.ToString("F3") + " dB");
            //results.WritePowerHisto();
            //results.WritePowerEntropy();
            //results.WriteEventHisto();
            //results.WriteEventEntropy();
        }


//***********************************************************************************************************************************
        //         IMAGE SAVING METHODS

        /// <summary>
        /// save bmp image with a zscore track at the bottom. Method assumes zscores and truncates below zero.
        /// if zscores==null, no score track is drawn
        /// </summary>
        /// <param name="zscores"></param>
        public void SaveImage(double[] zscores)
        {
            int type = 0; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.matrix, zscores, type);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }
        public void SaveImage(double[,] matrix, double[] zscores)
        {
            int type = 0; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(matrix, zscores, type);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveImage(string opDir, double[] zscores)
        {
            int type = 0; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.matrix, zscores, type);

            string fName = opDir + "//" + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveGradientImage()
        {
            int type = 0; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.gradM, null, type);

            string fName = this.state.SonogramDir + this.state.WavFName + "_grad" + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveMelImage(double[] zscores)
        {
            int type = 1; //image is mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.melFM, zscores, type);

            string fName = this.state.SonogramDir + this.state.WavFName + "_melScale" + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveCepImage(double[] zscores)
        {
            int type = 2; //image is cepstral
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.cepsM, zscores, type);

            string fName = this.state.SonogramDir + this.state.WavFName + "_cepstrum" + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveShapeImage(ArrayList shapes)
        {
            //int type = 0; //image is linear scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.shapeM, shapes);

            string fName = this.state.SonogramDir + this.state.WavFName + "_shape" + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }


    } //end class Sonogram



    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************



    
    public class SonoConfig
    {

        private string wavFileExt = ".wav"; //default value
        public string WavFileExt { get { return wavFileExt; } set { wavFileExt = value; } }
        private string bmpFileExt = ".bmp";//default value
        public string BmpFileExt { get { return bmpFileExt; } set { bmpFileExt = value; } }


        //wav file info
        public string  WavFileDir { get; set; }
        public string  WavFName { get; set; }
        public double  SignalMax { get; set; }
        public string  DeployName { get; set; }
        public string  Date { get; set; }
        public int  Hour { get; set; }
        public int  Minute { get; set; }
        public int  TimeSlot { get; set; }
        
        public int WindowSize { get; set; }
        public double WindowOverlap { get; set; }
        public string WindowFncName { get; set; }
        public FFT.WindowFunc WindowFnc { get; set; }

        public int SampleRate { get; set; }
        public int SampleCount { get; set; }
        public int MaxFreq { get; set; }               //Nyquist frequency = half audio sampling freq
        public double AudioDuration { get; set; }
        public double WindowDuration { get; set; }     //duration of full window in seconds
        public double NonOverlapDuration { get; set; } //duration of non-overlapped part of window in seconds

        public int SpectrumCount { get; set; }
        public double SpectraPerSecond { get; set; }
        public int FreqBinCount { get; set; }  //number of spectral values 
        public int FreqBandCount { get; set; } //number of one kHz bands
        public double FBinWidth { get;set; }

        public double MinPower { get; set; }//min power in sonogram
        public double AvgPower { get; set; }//average power in sonogram
        public double MaxPower { get; set; }//max power in sonogram
        public double MinPercentile { get; set; }
        public double MaxPercentile { get; set; }
        public double MinCut { get; set; } //power of min percentile
        public double MaxCut { get; set; } //power of max percentile

        public int    MelBinCount { get; set; } //number of mel spectral values 
        public double MinMelPower { get; set; } //min power in mel sonogram
        public double MaxMelPower { get; set; } //max power in mel sonogram
        public double MaxMel { get; set; }      //Nyquist frequency on Mel scale

        public int    CepBinCount { get; set; } //number of cepstral values 
        public double MinCepPower { get; set; } //min value in cepstral sonogram
        public double MaxCepPower { get; set; } //max value in cepstral sonogram

        //freq bins of the scanned part of sonogram
        public int TopScanBin { get; set; }
        public int MidScanBin { get; set; }
        public int BottomScanBin { get; set; }
   //     public int MidTemplateFreq { get; set; }

        public string SonogramDir { get; set; }
        public string BmpFName { get; set; }
        public bool AddGrid { get; set; }
        public int BlurWindow { get; set; }
        public int BlurWindow_time { get; set; }
        public int BlurWindow_freq { get; set; }
        public bool NormSonogram { get; set; }

        public int ZscoreSmoothingWindow { get; set; }
        public double ZScoreThreshold { get; set; }
        public int Verbosity { get; set; }

        /// <summary>
        /// converts wave file names into component info 
        /// wave file name have following format: "BAC1_20071008-081607"
        /// </summary>
        /// <param name="FName"></param>
        public void SetDateAndTime(string fName)
        {
            string[] parts = fName.Split('_');
            this.DeployName = parts[0];
            parts = parts[1].Split('-');
            this.Date = parts[0];
            this.Hour = Int32.Parse(parts[1].Substring(0,2));
            this.Minute = Int32.Parse(parts[1].Substring(2, 2));
            this.TimeSlot = ((this.Hour*60)+Minute)/30; //convert to half hour time slots
        }

        
        public void ReadConfig(string iniFName)
        {
            Configuration cfg = new Configuration(iniFName);
            ReadConfig(cfg);
        }

        public void ReadConfig(Configuration cfg)
        {
            this.wavFileExt = cfg.GetString("WAV_FILEEXT");
            this.SonogramDir = cfg.GetString("SONOGRAM_DIR");
            this.WindowSize = cfg.GetInt("WINDOW_SIZE");
            this.WindowOverlap = cfg.GetDouble("WINDOW_OVERLAP");
            this.WindowFncName = cfg.GetString("WINDOW_FUNCTION");
            this.WindowFnc = FFT.GetWindowFunction(this.WindowFncName);
            this.MinPercentile = cfg.GetDouble("MIN_PERCENTILE");
            this.MaxPercentile = cfg.GetDouble("MAX_PERCENTILE");
            this.wavFileExt = cfg.GetString("WAV_FILEEXT");
            this.bmpFileExt = cfg.GetString("BMP_FILEEXT");
            this.AddGrid = cfg.GetBoolean("ADDGRID");
            this.Verbosity = cfg.GetInt("VERBOSITY");
            this.BlurWindow = cfg.GetInt("BLUR_NEIGHBOURHOOD");
            this.BlurWindow_time = cfg.GetInt("BLUR_TIME_NEIGHBOURHOOD");
            this.BlurWindow_freq = cfg.GetInt("BLUR_FREQ_NEIGHBOURHOOD");
            this.NormSonogram = cfg.GetBoolean("NORMALISE_SONOGRAM");
            this.ZscoreSmoothingWindow = cfg.GetInt("ZSCORE_SMOOTHING_WINDOW");
            this.ZScoreThreshold = cfg.GetDouble("ZSCORE_THRESHOLD");
        }


    } //end class SonoConfig

}