using System;
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
 


        private double[,] matrix; //the actual sonogram
        public  double[,] Matrix { get { return matrix; } /*set { matrix = value; }*/ }
        private double[,] gradM; //the gradient version of the sonogram
        public  double[,] GradM  { get { return gradM; } /*set { gradM = value; }*/ }
        private double[,] melFM; //the Mel Frequency version of the sonogram
        public  double[,] MelFM  { get { return melFM; } /*set { melFM = value; }*/ }


        //  RESULTS variables
        private Results results =  new Results(); //set up a results file
        public Results Results { get { return results; } set { results = value; } }
        public double NoiseAv { get { return results.NoiseAv; } }
        public double NoiseSD { get { return results.NoiseSd; } }
        public double[] ActivityHisto { get { return results.ActivityHisto; } }


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
            //this.results = new Results(); //set up a results file

            state.WavFileDir = wav.WavFileDir;
            state.WavFName = wav.WavFileName;
            state.WavFName = state.WavFName.Substring(0, state.WavFName.Length - 4);
            state.SignalMax = wav.GetMaxValue();
            if (state.SignalMax == 0.0) throw new ArgumentException("Wav file has zero signal");
            state.SetDateAndTime(state.WavFName);
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
            //this.results = new Results(); //set up a results file

            FileInfo fi = new FileInfo(wavPath);
            state.WavFileDir = fi.DirectoryName;
            state.WavFName = fi.Name.Substring(0, fi.Name.Length - 4);
            state.WavFileExt = fi.Extension;

            //read the .WAV file
            WavReader wav = new WavReader(wavPath);
            state.SignalMax = wav.GetMaxValue();
            //Console.WriteLine("Max Value=" + state.SignalMax);
            if (state.SignalMax == 0.0) throw new ArgumentException("Wav file has zero signal");
            state.SetDateAndTime(state.WavFName);
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
            //this.results = new Results(); //set up a results file


            FileInfo fi = new FileInfo(wavPath);
            state.WavFileDir = fi.DirectoryName;
            state.WavFName = fi.Name.Substring(0, fi.Name.Length - 4);
            state.WavFileExt = fi.Extension;

            //initialise WAV class with bytes array
            WavReader wav = new WavReader(wavBytes, state.WavFName);
            state.SignalMax = wav.GetMaxValue();
            //Console.WriteLine("Max Value=" + state.SignalMax);
            if (state.SignalMax == 0.0) throw new ArgumentException("Wav file has zero signal");
            state.SetDateAndTime(state.WavFName);
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }


        private void Make(WavReader wav)
        {
            //store essential parameters for this sonogram
            this.state.WavFName = wav.WavFileName;
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
            double minP = Double.MaxValue;
            double avgP = -Double.MaxValue;
            double maxP = -Double.MaxValue;
            this.matrix = GenerateSpectrogram(wav, fft, step, out minP, out avgP, out maxP);
            this.state.MinPower = minP;
            this.state.AvgPower = avgP;
            this.state.MaxPower = maxP;
            this.matrix = DataTools.Blur(this.matrix, this.state.BlurNH_freq, this.state.BlurNH_time);
            //normalise and bound the values
            if (this.state.NormSonogram) NormalizeAndBound();

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
	

			double offset = 0.0;
			double[,] sonogram = new double[width, height];
			min = Double.MaxValue; 
            max = Double.MinValue;
            double sum = 0.0;

			for (int i = 0; i < width; i++)//foreach time step
			{
				double[] f1 = fft.Invoke(data, (int)Math.Floor(offset));
                for (int j = 0; j < height; j++) //foreach freq bin
				{
                    double amplitude = f1[j + 1];
                    if (amplitude < epsilon) amplitude = epsilon; //to prevent log of a very small number
                    double dBels = 20 * Math.Log10(amplitude);    //convert to decibels
                    //NOTE: the decibels calculation should be a ratio. 
                    // Here the ratio is implied ie relative to the power in the normalised wav signal
                    if (dBels <= min) min = dBels;
                    else
                    if (dBels >= max) max = dBels;
                    sonogram[i, j] = dBels;
                    sum += dBels;
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
            double gradThreshold = 1.5;
            int fBlurNH = 5;
            int tBlurNH = 4;
            this.gradM = DataTools.Blur(this.matrix, fBlurNH, tBlurNH);
            int height = this.gradM.GetLength(0);
            int width = this.gradM.GetLength(1);
            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            for (int y = 0; y < height-1; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    this.gradM[y, x] = gradM[y + 1, x] - gradM[y, x];//calculate gradient

                    //get min and max gradient
                    if (this.gradM[y, x] < min)      min = this.gradM[y, x];
                    else
                    if (this.gradM[y, x] > max)      max = this.gradM[y, x];

                    // quantize the gradients
                    if (this.gradM[y, x] < -gradThreshold) this.gradM[y, x] = 0.0;
                    else
                    if (this.gradM[y, x] > gradThreshold)  this.gradM[y, x] = 1.0;
                    else                                   this.gradM[y, x] = 0.5;
                }
            }

            results.MinGrad = min;
            results.MaxGrad = max;

            for (int x = 0; x < width; x++) this.gradM[height - 1, x] = 0.5; //patch in last time step with medium gradient
            this.state.MinCut = 0.0;
            this.state.MaxCut = 1.0;
        }


        public void Convert2MelFreq(int bandCount)
        {
            double[,] inData = this.Matrix;
            int M = inData.GetLength(0);
            int N = inData.GetLength(1);
            double[,] outData = new double[M, bandCount];
            double Nyquist    = this.state.MaxFreq;
            double linBand    = Nyquist / bandCount;
            double melBand    = Speech.Mel(Nyquist) / bandCount;
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            for (int i = 0; i < M; i++)
                for (int j = 0; j < bandCount; j++)
                {
                    double a = Speech.InverseMel(j * melBand) / linBand;
                    double b = Speech.InverseMel((j + 1) * melBand) / linBand;
                    int ai = (int)Math.Ceiling(a);
                    int bi = (int)Math.Floor(b);

                    double sum = 0.0;
                    if (ai > 0)
                    {
                        double ya = (1.0 - ai + a) * inData[i, ai - 1] + (ai - a) * inData[i, ai];
                        sum += Speech.MelIntegral(a * linBand, ai * linBand, ya, inData[i, ai]);
                    }
                    for (int k = ai; k < bi; k++)
                    {
                        sum += Speech.MelIntegral(k * linBand, (k + 1) * linBand, inData[i, k], inData[i, k + 1]);
                    }
                    if (bi < (N - 1)) //this.Bands in Greg's original code
                    {
                        double yb = (b - bi) * inData[i, bi] + (1.0 - b + bi) * inData[i, bi + 1];
                        sum += Speech.MelIntegral(bi * linBand, b * linBand, inData[i, bi], yb);
                    }
                    sum /= melBand;

                    outData[i, j] = sum;
                    if (sum < min) min = sum;
                    if (sum > max) max = sum;
                }
            this.melFM = outData;
            //return new Spectrum() { Data = outData, Min = min, Epsilon = this.Epsilon, Max = max, Nyquist = Mel(Nyquist) };
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
            results.EventHisto   = CalculateEventHisto(); //calculate statistics
            results.PowerHisto   = CalculatePowerHisto();
            //results.PowerEntropy = DataTools.RelativeEntropy(DataTools.NormaliseProbabilites(results.PowerHisto));
            results.EventEntropy = DataTools.RelativeEntropy(DataTools.NormaliseProbabilites(results.EventHisto));
        }


        //normalise and compress/bound the values
        public void NormalizeAndBound(double minPercentile, double maxPercentile)
        {
            this.state.MinPercentile = minPercentile;
            this.state.MaxPercentile = maxPercentile;
            double minCut;
            double maxCut;
            DataTools.GetPercentileCutoffs(this.matrix, this.state.MinPower, this.state.MaxPower, minPercentile, maxPercentile, out minCut, out maxCut);
            this.state.MinCut = minCut;
            this.state.MaxCut = maxCut;
            this.matrix = DataTools.boundMatrix(this.matrix, this.state.MinCut, this.state.MaxCut);
        }

        //normalise and compress/bound the values
        public void NormalizeAndBound()
        {
            double minCut;
            double maxCut;
            DataTools.GetPercentileCutoffs(this.matrix, this.state.MinPower, this.state.MaxPower, this.state.MinPercentile, this.state.MaxPercentile, out minCut, out maxCut);
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
            results.WritePowerHisto();
            //results.WritePowerEntropy();
            results.WriteEventHisto();
            results.WriteEventEntropy();
        }

        public void SaveGradientImage()
        {
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.gradM, null);

            string fName = this.state.SonogramDir + this.state.WavFName + "_grad" + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveMelImage()
        {
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.melFM, null);

            string fName = this.state.SonogramDir + this.state.WavFName + "_melScale" + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        /// <summary>
        /// save bmp image with a zscore track at the bottom. Method assumes zscores and truncates below zero.
        /// if zscores==null, no score track is drawn
        /// </summary>
        /// <param name="zscores"></param>
        public void SaveImage(double[] zscores)
        {
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.matrix, zscores);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }
        public void SaveImage(string opDir, double[] zscores)
        {
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.matrix, zscores);

            string fName = opDir + "//" + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }


    } //end class Sonogram

    
    /// <summary>
    /// 
    /// </summary>
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
        public int MaxFreq { get; set; }
        public double AudioDuration { get; set; }
        public double WindowDuration { get; set; }     //duration of full window in seconds
        public double NonOverlapDuration { get; set; } //duration of non-overlapped part of window in seconds

        public int SpectrumCount { get; set; }
        public double SpectraPerSecond { get; set; }
        public int FreqBinCount { get; set; }
        public int FreqBandCount { get; set; }

        public double FBinWidth { get;set; }
        public double MinPower { get; set; }//min power in sonogram
        public double AvgPower { get; set; }//average power in sonogram
        public double MaxPower { get; set; }//max power in sonogram
        public double MinPercentile { get; set; }
        public double MaxPercentile { get; set; }
        public double MinCut { get; set; } //power of min percentile
        public double MaxCut { get; set; } //power of max percentile


        //freq bins of the scanned part of sonogram
        public int TopScanBin { get; set; }
        public int MidScanBin { get; set; }
        public int BottomScanBin { get; set; }
   //     public int MidTemplateFreq { get; set; }

        public string SonogramDir { get; set; }
        public string BmpFName { get; set; }
        public bool AddGrid { get; set; }
        public int BlurNH { get; set; }
        public int BlurNH_time { get; set; }
        public int BlurNH_freq { get; set; }
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
            this.BlurNH = cfg.GetInt("BLUR_NEIGHBOURHOOD");
            this.BlurNH_time = cfg.GetInt("BLUR_TIME_NEIGHBOURHOOD");
            this.BlurNH_freq = cfg.GetInt("BLUR_FREQ_NEIGHBOURHOOD");
            this.NormSonogram = cfg.GetBoolean("NORMALISE_SONOGRAM");
            this.ZscoreSmoothingWindow = cfg.GetInt("ZSCORE_SMOOTHING_WINDOW");
            this.ZScoreThreshold = cfg.GetDouble("ZSCORE_THRESHOLD");
        }


    } //end class SonoConfig



    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************
    //***********************************************************************************

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

		public static readonly WindowFunc Lanczos = delegate(int n, int N) {
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

		public static readonly WindowFunc FlatTop = delegate(int n, int N) {
			double c1 = Math.Cos(2.0 * Math.PI * n / (N - 1));
			double c2 = Math.Cos(4.0 * Math.PI * n / (N - 1));
			double c3 = Math.Cos(6.0 * Math.PI * n / (N - 1));
			double c4 = Math.Cos(8.0 * Math.PI * n / (N - 1));
			return 1.0 - 1.93 * c1 + 1.29 * c2 - 0.388 * c3 + 0.032 * c4;
		};
		#endregion

		private static bool IsPowerOf2(int n)
		{
			while (n > 1)
			{
				if (n == 2) return true;
				n >>= 1;
			}
			return false;
		}

        public static FFT.WindowFunc GetWindowFunction(string name)
        {
            //FFT.WindowFunc windowFnc;
            if(name.StartsWith("Hamming")) return FFT.Hamming;
            else return null;
        }
	}//end class FFT

}