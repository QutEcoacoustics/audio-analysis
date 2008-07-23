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


        private double[] energy; //energy per signal frame
        public  double[] Energy { get { return energy; } /*set { energy = value; }*/ }

        private double[,] matrix; //the original sonogram
        public  double[,] Matrix { get { return matrix; } /*set { matrix = value; }*/ }



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
            if (wav.WavFileName != null)
            {
                state.WavFName = state.WavFName.Substring(0, state.WavFName.Length - 4);
                state.SetDateAndTime(state.WavFName);
            }

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
            Make(wav);
            if (state.Verbosity != 0) WriteInfo();
        }

        private void Make(WavReader wav)
        {
            //store essential parameters for this sonogram
            if (wav.Amplitude_AbsMax == 0.0) throw new ArgumentException("Wav file has zero signal. Cannot make sonogram.");
            this.state.SignalAbsMax = wav.Amplitude_AbsMax;
            this.state.SignalAvgMax = wav.Amplitude_AvMax;
            this.state.WavFName = wav.WavFileName;
            this.state.SampleRate = wav.SampleRate;
            this.state.SampleCount = wav.SampleCount;
            this.state.AudioDuration = state.SampleCount / (double)state.SampleRate;
            this.state.MaxFreq = state.SampleRate / 2;
            this.state.WindowDuration = state.WindowSize / (double)state.SampleRate; // window duration in seconds
            this.state.NonOverlapDuration = this.state.WindowDuration * (1 - this.state.WindowOverlap);// duration in seconds
            this.state.FreqBinCount = this.state.WindowSize / 2; // other half is phase info
            this.state.FBinWidth = this.state.MaxFreq / (double)this.state.FreqBinCount;
            this.state.SpectrumCount = (int)(this.state.AudioDuration / this.state.NonOverlapDuration);
            this.state.SpectraPerSecond = 1 / this.state.NonOverlapDuration;

            double[] signal = wav.Samples;
            //SIGNAL PRE-EMPHASIS helps with speech signals
            bool doPreemphasis = false;
            if (doPreemphasis)
            {
                double coeff = 0.96;
                signal = PreEmphasis(signal, coeff);
            }

            //FRAME WINDOWING
            int step = (int)(this.state.WindowSize * (1 - this.state.WindowOverlap));
            double[,] frames = Frames(signal, this.state.WindowSize, step);
            this.state.SpectrumCount = frames.GetLength(0);

            //ENERGY PER FRAME
            this.energy = SignalEnergy(frames);


            //generate the spectra
            //calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
            double epsilon = Math.Pow(0.5, wav.BitsPerSample - 1);
            this.matrix = GenerateSpectra(frames, this.state.WindowFnc, epsilon);
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
        public double[] PreEmphasis(double[] signal, double coeff)
        {
            int L = signal.Length;
            double[] newSig = new double[L-1];
            for (int i = 0; i < L-1; i++) newSig[i] = signal[i+1] - (coeff* signal[i]); 
            return newSig;
        }

        /// <summary>
        /// Breaks a long audio signal into frames with given step
        /// </summary>
        /// <param name="data"></param>
        /// <param name="windowSize"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public double[,] Frames(double[] data, int windowSize, int step)
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
                {
                    frames[i, j] = data[offset+j];
                }
                offset += step;
            } //end matrix
            return frames;
        }


        /// <summary>
        /// Frame energy is the log of the summed energy of the samples.
        /// Need to normalise. Energy normalisation formula taken from Lecture Notes of Prof. Bryan Pellom
        /// Automatic Speech Recognition: From Theory to Practice.
        /// http://www.cis.hut.fi/Opinnot/T-61.184/ September 27th 2004.
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public double[] SignalEnergy(double[,] frames)
        {
            const double minLogEnergy = -5.0;
            double maxLogEnergy = Math.Log(0.25);//assumes max average amplitude in a signal = 0.5
            
            int frameCount = frames.GetLength(0);
            int N          = frames.GetLength(1);
            double[] energy = new double[frameCount];
            for (int i = 0; i < frameCount; i++) //foreach frame
            {
                double sum = 0.0;
                for (int j = 0; j < N; j++)  //foreach sample in frame
                {
                    sum += (frames[i,j] * frames[i,j]); //sum the energy
                }
                double e = sum / (double)N;
                if (e <= 0.0) energy[i] = minLogEnergy;
                else          energy[i] = Math.Log(e);
            }
            //double maxEnergy = energy[DataTools.getMaxIndex()];

            //normalise to an absolute energy value
            for (int i = 0; i < frameCount; i++) //foreach time step
            {
                energy[i] = ((energy[i] - maxLogEnergy) * 0.1) + 1.0; //see method header for reference 
            }
            return energy;
        }


        public double[,] GenerateSpectra(double[,] frames, FFT.WindowFunc w, double epsilon)
        {
            int frameCount = frames.GetLength(0);
            int N = frames.GetLength(1);  //= the FFT windowSize 
            int binCount = (N / 2) + 1;  // = fft.WindowSize/2 +1 for the DC value;

            FFT fft = new FFT(N, w); // init class which calculates the FFT

            //calculate a minimum amplitude to prevent taking log of small number. This would increase the range when normalising
            int smoothingWindow = 3; //to smooth the spectrum 

            double[,] sonogram = new double[frameCount, binCount];

            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                double[] data = DataTools.GetRow(frames, i);
                double[] f1 = fft.Invoke(data);
                f1 = DataTools.filterMovingAverage(f1, smoothingWindow); //to smooth the spectrum - reduce variance
                for (int j = 0; j < binCount; j++) //foreach freq bin
                {
                    double amplitude = f1[j];
                    if (amplitude < epsilon) amplitude = epsilon; //to prevent possible log of a very small number
                    sonogram[i, j] = amplitude;
                }
            } //end of all frames
            return sonogram;
        }


        /// <summary>
        /// trims the values of the passed spectrogram using the Min and Max percentile values in the ini file.
        /// First calculate the value cut-offs for the given percentiles.
        /// Second, calculate the min, avg and max values of the spectrogram.
        /// </summary>
        /// <param name="SPEC"></param>
        /// <param name="min"></param>
        /// <param name="avg"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double[,] Trim(double[,] SPEC, out double min, out double avg, out double max)
        {
            int frameCount = SPEC.GetLength(0);
            int binCount   = SPEC.GetLength(1);

            //normalise and compress/bound the values
            double minCut;
            double maxCut;
            DataTools.PercentileCutoffs(SPEC, this.state.MinPercentile, this.state.MaxPercentile, out minCut, out maxCut);
            this.state.MinCut = minCut;
            this.state.MaxCut = maxCut;
            this.matrix = DataTools.boundMatrix(this.matrix, minCut, maxCut);

            min = Double.MaxValue;
            max = Double.MinValue;
            double sum = 0.0;

            for (int i = 0; i < frameCount; i++)//foreach time step
            {
                for (int j = 0; j < binCount; j++) //foreach freq bin
                {
                    double value = SPEC[i, j];
                    if (value < min) min = value;
                    else
                        if (value > max) max = value;
                    sum += value;
                }
            } //end of all frames
            avg = sum / (frameCount * binCount);
            return SPEC;
        }




        public double[,] MelScale(double[,] matrix, int melBandCount)
        {
            double Nyquist = this.state.MaxFreq;
            double melBand = Speech.Mel(Nyquist) / (double)melBandCount;  //width of mel band

            this.State.MelBinCount = melBandCount;
            this.State.MaxMel = Speech.Mel(Nyquist);

            return Speech.MelScale(matrix, melBandCount, Nyquist);
        }


        public double[,] MFCCs(double[,] matrix, int melBandCount, int coeffCount)
        {
            double Nyquist = this.state.MaxFreq;
            double melBand = Speech.Mel(Nyquist) / (double)melBandCount;  //width of mel band

            this.State.MelBinCount = melBandCount;
            this.State.MaxMel = Speech.Mel(Nyquist);

            return Speech.MFCCs(matrix, melBandCount, Nyquist, coeffCount);
        }


        public double[,] Gradient()
        {
            double gradThreshold = 2.0;
            int fWindow = 11;
            int tWindow = 9;
            double[,] blurM = ImageTools.Blur(this.matrix, fWindow, tWindow);
            int height = blurM.GetLength(0);
            int width  = blurM.GetLength(1);
            double[,] outData = new double[height, width];

            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            for (int x = 0; x < width; x++) outData[0, x] = 0.5; //patch in first  time step with zero gradient
            for (int x = 0; x < width; x++) outData[1, x] = 0.5; //patch in second time step with zero gradient
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
                    if (grad1 < -gradThreshold) outData[y, x] = 0.0;
                    else
                        if (grad1 > gradThreshold) outData[y, x] = 1.0;
                        else
                            if (grad2 < -gradThreshold) outData[y, x] = 0.0;
                            else
                                if (grad2 > gradThreshold) outData[y, x] = 1.0;
                                else outData[y, x] = 0.5;
                }
            }

            //for (int x = 0; x < width; x++) this.gradM[height - 1, x] = 0.5; //patch in last time step with medium gradient
            return outData;
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


        public double[] CalculateEventHisto(double[,] gradM)
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
                        if (gradM[y, x] != gradM[y-1, x]) counts[f]++; //count any gradient change
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
        public double[] CalculateEvent2Histo(double[,] gradM)
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
                        double d = gradM[y,x];
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

        public double[] CalculateActivityHisto(double[,] gradM)
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
                        activity[f] += (gradM[y, x] * gradM[y, x]); //add square of gradient
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



        public void WriteInfo()
        {
            Console.WriteLine("\nSONOGRAM INFO");
            Console.WriteLine(" WavSampleRate=" + this.state.SampleRate + " SampleCount=" + this.state.SampleCount + "  Duration=" + (this.state.SampleCount / (double)this.state.SampleRate).ToString("F3") + "s");
            Console.WriteLine(" Window Size=" + this.state.WindowSize + "  Max FFT Freq =" + this.state.MaxFreq);
            Console.WriteLine(" Window Overlap=" + this.state.WindowOverlap + " Window duration=" + this.state.WindowDuration + "ms. (non-overlapped=" + this.state.NonOverlapDuration + "ms)");
            Console.WriteLine(" Freq Bin Width=" + (this.state.MaxFreq / (double)this.state.FreqBinCount).ToString("F3") + "hz");
            Console.WriteLine(" Min power=" + this.state.PowerMin.ToString("F3") + " Avg power=" + this.state.PowerAvg.ToString("F3") + " Max power=" + this.state.PowerMax.ToString("F3"));
            Console.WriteLine(" Min percentile=" + this.state.MinPercentile.ToString("F2") + "  Max percentile=" + this.state.MaxPercentile.ToString("F2"));
            Console.WriteLine(" Min cutoff=" + this.state.MinCut.ToString("F3") + "  Max cutoff=" + this.state.MaxCut.ToString("F3"));
        }

        public void WriteStatistics()
        {
            Console.WriteLine("\nSONOGRAM STATISTICS");
            Console.WriteLine(" Max power=" + this.State.PowerMax.ToString("F3") + " dB");
            Console.WriteLine(" Avg power=" + this.State.PowerAvg.ToString("F3") + " dB");
            //results.WritePowerHisto();
            //results.WritePowerEntropy();
            //results.WriteEventHisto();
            //results.WriteEventEntropy();
        }


        public void SetOutputDir(string dir)
        {
            this.state.SonogramDir = dir;
        }

//***********************************************************************************************************************************
        //         IMAGE SAVING METHODS


        public void SaveImage(double[,] matrix, double[] zscores)
        {
            ImageType type = ImageType.linearScale; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(matrix, zscores, type);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveImage(double[,] matrix, double[] zscores, ImageType type)
        {
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(matrix, zscores, type);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveImage(double[,] matrix, ArrayList shapes, Color col)
        {
            ImageType type = ImageType.linearScale; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(matrix, null, type);
            if (shapes != null) bmp = image.AddShapeBoundaries(bmp, shapes, col);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }
        public void SaveImage(double[,] matrix, ArrayList shapes, Color col, ImageType type)
        {
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(matrix, null, type);
            if (shapes != null) bmp = image.AddShapeBoundaries(bmp, shapes, col);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }


        public void SaveImageOfSolids(double[,] matrix, ArrayList shapes, Color col)
        {
            ImageType type = ImageType.linearScale; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(matrix, null, type);
            if (shapes != null) bmp = image.AddShapeSolids(bmp, shapes, col);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }

        public void SaveImageOfCentroids(double[,] matrix, ArrayList shapes, Color col)
        {
            ImageType type = ImageType.linearScale; //image is linear scale not mel scale
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(matrix, null, type);
            if (shapes != null) bmp = image.AddCentroidBoundaries(bmp, shapes, col);

            string fName = this.state.SonogramDir + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }


        public void SaveImage(string opDir, double[] zscores, ImageType type)
        {
            SonoImage image = new SonoImage(this.state);
            Bitmap bmp = image.CreateBitmap(this.matrix, zscores, type);

            string fName = opDir + "//" + this.state.WavFName + this.state.BmpFileExt;
            this.state.BmpFName = fName;
            bmp.Save(fName);
        }



        /// <summary>
        /// WARNING!! This method must be consistent with the ANALYSIS HEADER line declared in Results.AnalysisHeader()
        /// </summary>
        /// <param name="id"></param>
        /// <param name="syllableDistribution"></param>
        /// <param name="categoryDistribution"></param>
        /// <param name="categoryCount"></param>
        /// <returns></returns>
        public string OneLineResult(int id, int[] syllableDistribution, int[] categoryDistribution, int categoryCount)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(id + Results.spacer); //CALLID
            //sb.Append(DateTime.Now.ToString("u") + spacer); //DATE
            sb.Append(this.State.WavFName.ToString() + Results.spacer); //sonogram FNAME
            sb.Append(this.State.Date.ToString() + Results.spacer); //sonogram date
            sb.Append(this.State.DeployName + Results.spacer); //Deployment name
            sb.Append(this.State.AudioDuration.ToString("F2") + Results.spacer); //length of recording
            sb.Append(this.State.Hour + Results.spacer); //hour when recording made
            sb.Append(this.State.Minute + Results.spacer); //hour when recording made
            sb.Append(this.State.TimeSlot + Results.spacer); //half hour when recording made

            sb.Append(this.State.SignalAbsMax.ToString("F4") + Results.spacer);
            sb.Append(this.State.SignalAvgMax.ToString("F4") + Results.spacer);
            double sigRatio = (this.State.SignalAvgMax / this.State.SignalAbsMax);
            sb.Append(sigRatio.ToString("F4") + Results.spacer);
            sb.Append(this.State.PowerMax.ToString("F3") + Results.spacer);
            sb.Append(this.State.PowerAvg.ToString("F3") + Results.spacer);

            //syllable distribution
            if ((categoryCount == 0) || (syllableDistribution==null))
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append("0  " + Results.spacer);
            else
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append(syllableDistribution[f].ToString() + Results.spacer);
            sb.Append(DataTools.Sum(syllableDistribution) + Results.spacer);

            //category distribution
            if ((categoryCount == 0) || (syllableDistribution == null))
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append("0  " + Results.spacer);
            else
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append(categoryDistribution[f].ToString() + Results.spacer);
            sb.Append(categoryCount + Results.spacer);

            //monotony index
            double sum = 0.0;
            double monotony = 0.0;
            if ((categoryCount == 0) || (syllableDistribution == null))
            {
                for (int f = 0; f < Results.analysisBandCount; f++) sb.Append("0.0000" + Results.spacer);
                sb.Append("0.0000" + Results.spacer);
            }
            else
            {
                for (int f = 0; f < Results.analysisBandCount; f++)
                {
                    if (categoryDistribution[f] == 0) monotony = 0.0;
                    else                              monotony = syllableDistribution[f] / (double)categoryDistribution[f];
                    sb.Append(monotony.ToString("F4") + Results.spacer);
                    sum += monotony;
                }
                double av = sum / (double)Results.analysisBandCount;
                sb.Append(av.ToString("F4") + Results.spacer);
            }
            sb.Append(this.State.WavFName.ToString() + Results.spacer);
            
            return sb.ToString();
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
        public double  SignalAbsMax { get; set; }
        public double  SignalAvgMax { get; set; }
        public string  DeployName { get; set; }
        public string  Date { get; set; }
        public int     Hour { get; set; }
        public int     Minute { get; set; }
        public int     TimeSlot { get; set; }
        
        public int    WindowSize { get; set; }
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

        public double PowerMin { get; set; }//min power in sonogram
        public double PowerAvg { get; set; }//average power in sonogram
        public double PowerMax { get; set; }//max power in sonogram
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
            if(parts.Length == 1)
            {
                this.DeployName = fName;
                this.Date = "000000";
                this.Hour = 0;
                this.Minute = 0;
                this.TimeSlot = 0; 
                return;
            }
            this.DeployName = parts[0];
            parts = parts[1].Split('-');
            this.Date = parts[0];
            this.Hour = Int32.Parse(parts[1].Substring(0,2));
            this.Minute = Int32.Parse(parts[1].Substring(2, 2));
            //############ WARNING!!! THE FOLLOWING LINE MUST BE CONSISTENT WITH TIMESLOT CONSTANT
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