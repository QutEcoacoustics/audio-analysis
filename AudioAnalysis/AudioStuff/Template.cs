using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using TowseyLib;
using Props = TowseyLib.Configuration;


namespace AudioStuff
{
    /// <summary>
    /// Defines a single sound event in a sonogram
    /// </summary>
    /// 
    /// <remarks>
    /// This class defines a template for a specific natural sound event.
    /// This could be the single syllable of a bird or frog call or a short slice of
    /// rain or cicada noise.
    /// </remarks>
    /// 
    public class Template
    {
        private TemplateConfig templateState;
        public TemplateConfig TemplateState { get { return templateState; } set { templateState = value; } }
        private SonoConfig sonogramState;
        public SonoConfig SonogramState { get { return sonogramState; } set { sonogramState = value; } }

        private const string templateStemName = "template";
        private const string callStemName = "call";
        private const string bmpFileExt = ".bmp";
        private const string wavFileExt = ".wav";
        
        private int callID;
        public int CallID { get { return callID; } set { callID = value; } }
        private string callName;
        public string CallName { get { return callName; } set { callName = value; } }
        private string callComment;
        public string CallComment { get { return callComment; } set { callComment = value; } }

        //file names and directory
        private string templateDir;
        public string TemplateDir { get { return templateDir; } set { templateDir = value; } }
        private string matrixFName;
        public string MatrixFName { get { return matrixFName; } set { matrixFName = value; } }
        private string dataFName;
        public string DataFName { get { return dataFName; } set { dataFName = value; } }
        private string sonogramImageFname;
        public string SonogramImageFname { get { return sonogramImageFname; } set { sonogramImageFname = value; } }
        private string imageFName;
        public string ImageFName { get { return imageFName; } set { imageFName = value; } }



        //info about original .WAV file
        private string wavFname;
        public string WavFname { get { return wavFname; } set { wavFname = value; } }
        private double timeBin; //duration of non-overlapped part of one window

        //info about original SONOGRAM
        private int spectrumCount;//number of spectra in original sonogram
        private double spectraPerSecond;
        public double SpectraPerSecond { get { return spectraPerSecond; } set { spectraPerSecond = value; } }//sonogram sample rate
        private double spectrumDuration;
        private string windowFunction = "Hamming";
        private double hzBin;
        

        //info about TEMPLATE EXTRACTION
        int x1; int y1; //image coordinates for top left of selection
        int x2; int y2; //image coordinates for bottom right of selection
        int t1; int t2; //sonogram time interval selection
        int bin1; int bin2; //sonogram freq bin interval 
        double templateDuration; //duration of template in seconds
        int templateSpectralCount; //number of spectra in template
        int maxTemplateFreq;
        int minTemplateFreq;
        private int midTemplateFreq; //Hz
        public int MidTemplateFreq { get { return midTemplateFreq; } set { midTemplateFreq = value; } }
        private double minTemplatePower; // min and max power in template
        private double maxTemplatePower;

        //info about TEMPLATE SCORING
        public double NoiseAv { get { return this.templateState.NoiseAv; } }
        public double NoiseSD { get { return this.templateState.NoiseSd; } }

        
        //THE TEMPLATE MATRIX
        private double[,] matrix = null;
        public double[,] Matrix
        {   get{ return GetMatrix();} set { matrix = value; } }

        double[,] GetMatrix()
        {
            double[,] t = { //each line represents a column
                            {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
                            {1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0},
            };
            if (matrix != null) return matrix;
            else
            {
                Console.WriteLine("WARNING! Using default rectangular 2*21 template!");
                return t;
            }
        }




        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="callID"></param>
        public Template(int callID)
        {
            this.callID = callID;
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// Reads a call template from file using CallID for identifier
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="templateDir"></param>
        public Template(int callID, string templateDir)
        {
               this.callID = callID;
               this.TemplateDir = templateDir;
               this.dataFName = callStemName + "_" + callID + ".txt";
               this.matrixFName = templateStemName + "_" + callID + ".txt";
               this.imageFName = templateStemName + "_" + callID + ".bmp";

               int status = ReadTemplateConfigFile();
               if (status != 0) throw new System.Exception("Failed to read call info file. Exist status = " + status);
               status = ReadTemplateFile();
               if (status != 0) throw new System.Exception("Failed to read call matrix file. Exist status = " + status);
           }
         
        /// <summary>
        /// CONSTRUCTOR 3
        /// Creates a new call template using info provided
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="callName"></param>
        /// <param name="callComment"></param>
        public Template(int callID, string callName, string callComment, string templateDir)
        {
            this.callID = callID;
            this.callName = callName;
            this.callComment = callComment;
            this.TemplateDir = templateDir;
            this.dataFName = callStemName + "_" + callID + ".txt";
            this.matrixFName = templateStemName + "_" + callID + ".txt";
            this.imageFName = templateStemName + "_" + callID + Template.bmpFileExt;
            this.templateState = new TemplateConfig();
        }

        public void SetWavFileName(string wavFileName)
        {
            this.wavFname    = wavFileName+wavFileExt;
        }

        /// <summary>
        /// this method called from first line of ExtractTemplateUsingImageCoordinates()
        /// </summary>
        /// <param name="s"></param>
        public void SetSonogramInfo(Sonogram s)
        {
            this.sonogramState = s.State;
            //this.templateState = s.State;

            this.TemplateState.AudioDuration = s.State.AudioDuration;
            this.templateState.MaxFreq = s.State.MaxFreq;
            this.templateState.SampleRate = s.State.SampleRate;
            this.templateState.WindowSize  = s.State.WindowSize;
            this.templateState.WindowOverlap  = s.State.WindowOverlap;
            this.templateState.WindowFncName  = s.State.WindowFncName;
            this.templateState.SampleRate  = s.State.SampleRate;
            this.templateState.SampleCount  = s.State.SampleCount;
            this.templateState.MaxFreq  = s.State.MaxFreq;
            this.templateState.WindowDuration  = s.State.WindowDuration;
            this.templateState.NonOverlapDuration  = s.State.NonOverlapDuration;
            this.templateState.SpectrumCount  = s.State.SpectrumCount;
            this.templateState.SpectraPerSecond  = s.State.SpectraPerSecond;
            this.templateState.FreqBinCount  = s.State.FreqBinCount;

            this.timeBin = this.sonogramState.AudioDuration / (double)this.sonogramState.SpectrumCount;
            this.spectraPerSecond = this.sonogramState.SpectrumCount / (double)this.sonogramState.AudioDuration;
            this.spectrumDuration = 1/spectraPerSecond;
        }


        public void ConvertImageCoords2SonogramCoords(int freqBinCount, params int[] imageCoords)
        {
            this.x1 = imageCoords[0];
            this.y1 = imageCoords[1];
            this.x2 = imageCoords[2];
            this.y2 = imageCoords[3];
            //convert image coordinates to sonogram coords
            //sonogram: rows=timesteps; sonogram cols=freq bins
            this.t1 = imageCoords[0]; //imageCoords[0]=x1
            this.t2 = imageCoords[2]; //imageCoords[2]=x2
            this.bin1 = freqBinCount - imageCoords[3]; //imageCoords[3]=y2
            this.bin2 = freqBinCount - imageCoords[1] - 1;//imageCoords[1]=y1

            this.templateSpectralCount=t2-t1+1; //number of spectra in template
            this.templateDuration = templateSpectralCount*this.timeBin; //duration of template in seconds
            this.maxTemplateFreq = (int)(bin2 * this.hzBin);
            this.minTemplateFreq = (int)(bin1 * this.hzBin);
            this.midTemplateFreq = minTemplateFreq + ((maxTemplateFreq - minTemplateFreq) / 2); //Hz

        }



        /// <summary>
        /// Extracts a template (submatrix) from the passed sonogram but 
        /// using coordinates that the user would have obtained from the BMP image
        /// of the sonogram.
        /// Must first convert the image coordinates to sonogram coordinates.
        /// The image matrix is effectively rotated 90 degrees clockwise to
        /// map to the sonogram.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="imageCoords"></param>
        /// <returns></returns>
        public void ExtractTemplateUsingImageCoordinates(Sonogram s, params int[] imageCoords)
        {
            SetSonogramInfo(s);

            //convert image coordinates to sonogram coords
            //sonogram: rows=timesteps; sonogram cols=freq bins
            double[,] sMatrix = s.Matrix;
            int timeStepCount = sMatrix.GetLength(0);
            this.sonogramState.FreqBinCount = sMatrix.GetLength(1);
            this.spectrumCount = timeStepCount;
            this.hzBin = this.sonogramState.MaxFreq / (double)this.sonogramState.FreqBinCount;

            this.Matrix = s.Matrix;
            ConvertImageCoords2SonogramCoords(this.sonogramState.FreqBinCount, imageCoords);
            this.Matrix = DataTools.Submatrix(sMatrix, this.t1, this.bin1, this.t2, this.bin2);
            DataTools.MinMax(this.Matrix, out this.minTemplatePower, out this.maxTemplatePower);
        }//end ExtractTemplate



        public void ExtractTemplateFromImage2File(Sonogram s, params int[] imageCoords)
        {
            ExtractTemplateUsingImageCoordinates(s, imageCoords);
            FileTools.WriteMatrix2File_Formatted(this.matrix, this.TemplateDir + this.matrixFName, "F5");
        }


        
        public void WriteTemplateConfigFile()
        {
            //write the call data to a file
            ArrayList data = new ArrayList();
            data.Add("DATE=" + DateTime.Now.ToString("u"));
            data.Add("#\n#TEMPLATE DATA");
            data.Add("CALL_ID=" + this.callID);
            data.Add("CALL_NAME=" + this.callName);
            data.Add("CALL_COMMENT=" + this.callComment);
            data.Add("THIS_FILE=" + this.dataFName);

            data.Add("#\n#INFO ABOUT ORIGINAL .WAV FILE");
            data.Add(" WAV_FILE_NAME=" + this.wavFname);
            data.Add(" WAV_SAMPLE_RATE=" + this.sonogramState.SampleRate);
            data.Add(" WAV_DURATION=" + this.sonogramState.AudioDuration.ToString("F3"));

            data.Add("#\n#INFO ABOUT SONOGRAM");
            data.Add(" FFT_WINDOW_SIZE=" + this.sonogramState.WindowSize);
            data.Add(" FFT_WINDOW_OVERLAP=" + this.sonogramState.WindowOverlap);
            data.Add(" WINDOW_DURATION_MS=" + (this.sonogramState.WindowDuration * 1000).ToString("F3"));//convert to milliseconds
            data.Add(" NONOVERLAP_WINDOW_DURATION_MS=" + (this.sonogramState.NonOverlapDuration * 1000).ToString("F3"));//convert to milliseconds
            data.Add(" NUMBER_OF_SPECTRA=" + spectrumCount);
            data.Add(" SPECTRA_PER_SECOND=" + spectraPerSecond.ToString("F3"));
            data.Add(" WINDOW_FUNCTION=" + windowFunction);
            data.Add(" MAX_FREQ=" + this.sonogramState.MaxFreq);
            data.Add(" NUMBER_OF_FREQ_BINS=" + this.sonogramState.FreqBinCount);
            data.Add(" FREQ_BIN_WIDTH=" + hzBin.ToString("F2")+"hz");
            data.Add(" MIN_POWER=" + this.sonogramState.MinPower.ToString("F3"));
            data.Add(" AVG_POWER=" + this.sonogramState.AvgPower.ToString("F3"));
            data.Add(" MAX_POWER=" + this.sonogramState.MaxPower.ToString("F3"));
            data.Add(" MIN_CUTOFF=" + this.sonogramState.MinCut.ToString("F3"));
            data.Add(" MAX_CUTOFF=" + this.sonogramState.MaxCut.ToString("F3"));
            //data.Add(" SONOGRAM_IMAGE_FILE=" + this.SonogramImageFname);

            data.Add("#\n#INFO ABOUT CALL TEMPLATE");
            data.Add(" TEMPLATE_MATRIX_FILE=" + matrixFName);
            data.Add(" # NOTE: Each row of the template matrix is the power spectrum for a given time step.");
            data.Add(" #       That is, rows are time steps and columns are frequency bins.");
            data.Add(" # IMAGE COORDINATES USED TO EXTRACT CALL");
            data.Add(" X1=" + this.x1);
            data.Add(" Y1=" + this.y1);
            data.Add(" X2=" + this.x2);
            data.Add(" Y2=" + this.y2);
            data.Add(" # CORRESPONDING SONOGRAM COORDINATES");
            data.Add(" TIMESTEP1=" + this.t1);
            data.Add(" TIMESTEP2=" + this.t2);
            data.Add(" FREQ_BIN1=" + this.bin1);
            data.Add(" FREQ_BIN2=" + this.bin2);
            data.Add(" TEMPLATE_IMAGE_FILE=" + this.imageFName);
            data.Add(" TEMPLATE_DURATION=" + this.templateDuration.ToString("F3")+"s");
            data.Add(" TEMPLATE_SPEC_COUNT=" + this.templateSpectralCount+ "(time-steps)");
            data.Add(" TEMPLATE_FBIN_COUNT=" + (this.bin2 - this.bin1+1));
            data.Add(" TEMPLATE_MAX_FREQ="  + this.maxTemplateFreq);
            data.Add(" TEMPLATE_MID_FREQ="  + this.midTemplateFreq);
            data.Add(" TEMPLATE_MIN_FREQ="  + this.minTemplateFreq);
            data.Add(" TEMPLATE_MIN_POWER=" + this.minTemplatePower.ToString("F3"));
            data.Add(" TEMPLATE_MAX_POWER=" + this.maxTemplatePower.ToString("F3"));

            //data.Add("#");
            //data.Add("#INFO ABOUT SCORE PROCESSING");
            //data.Add("#NIGHT NOISE RESPONSE");
            //data.Add("NOISE_AV=-0.03421");
            //data.Add("NOISE_SD=0.00043");
            //data.Add("#RAIN NOISE RESPONSE");
            //data.Add("#NOISE_AV=-0.02976");
            //data.Add("#NOISE_SD=0.00042");



            //write data to file
            FileTools.WriteTextFile(this.templateDir + this.dataFName, data);

        } // end of WriteCallData2File()


        public int ReadTemplateConfigFile()
        {
            int status = 0;
            Console.WriteLine("\n#####  READING TEMPLATE INFO");
            Console.WriteLine("       FILE NAME=" + dataFName);
            Configuration cfg = new Props(TemplateDir + dataFName);
            this.callName = cfg.GetString("CALL_NAME"); 
            this.callComment = cfg.GetString("CALL_COMMENT");

            this.templateState = new TemplateConfig();
            this.templateState.SampleRate = cfg.GetInt("WAV_SAMPLE_RATE");
            this.templateState.MaxFreq = cfg.GetInt("MAX_FREQ");
            this.templateState.AudioDuration = cfg.GetDouble("WAV_DURATION");
            this.templateState.FreqBinCount = cfg.GetInt("NUMBER_OF_FREQ_BINS");
            this.templateState.SpectrumCount = cfg.GetInt("NUMBER_OF_SPECTRA");
            this.templateState.SpectraPerSecond = cfg.GetDouble("SPECTRA_PER_SECOND");

            this.templateState.WindowSize = cfg.GetInt("FFT_WINDOW_SIZE");
            this.templateState.WindowOverlap = cfg.GetDouble("FFT_WINDOW_OVERLAP");
            this.templateState.WindowDuration = cfg.GetDouble("WINDOW_DURATION_MS") / (double)1000; //convert ms to seconds
            this.templateState.NonOverlapDuration = cfg.GetDouble("NONOVERLAP_WINDOW_DURATION_MS") / (double)1000; //convert ms to seconds

            this.midTemplateFreq = cfg.GetInt("TEMPLATE_MID_FREQ");
            this.templateState.NoiseAv = cfg.GetDouble("NOISE_AV");
            this.templateState.NoiseSd = cfg.GetDouble("NOISE_SD");
            return status;
        } //end of ReadCallDataFile()


        public void SaveDataAndImageToFile()
        {
            WriteTemplateConfigFile();
            SaveImage();
        }

        public int ReadTemplateFile()
        {
            Console.WriteLine("\n#####  READING TEMPLATE DATA");
            Console.WriteLine("       FILE NAME=" + matrixFName);
            int status = 0;
            this.matrix = FileTools.ReadDoubles2Matrix(TemplateDir + matrixFName); ;
            return status;
        } //end of ReadTemplateFile()
        
        /// <summary>
        /// prepare Bitmap image of Template
        /// 
        /// </summary>
        /// <returns></returns>
        public Bitmap GetImage()
        {
            SonoImage bmps = new SonoImage(this.sonogramState);
            return bmps.CreateBitMapOfTemplate(Matrix);
        }

        public void SaveImage()
        {
            string imageFileName = TemplateDir + this.imageFName;
            Bitmap bmp = GetImage();
            bmp.Save(imageFileName);
        }

        public void WriteInfo()
        {
            Console.WriteLine("\nTEMPLATE INFO");
            Console.WriteLine(" Template ID: " + this.CallID);
            Console.WriteLine(" Template name: " + this.CallName);
            Console.WriteLine(" Comment: " + this.CallComment);
            Console.WriteLine(" Template directory: " + this.TemplateDir);
            Console.WriteLine(" Template data  in file " + this.DataFName);
            Console.WriteLine(" Template image in file " + this.ImageFName);
            Console.WriteLine(" Template matrix in file " + this.MatrixFName);
            Console.WriteLine(" Bottom freq=" + this.minTemplateFreq + "  Mid freq=" + this.MidTemplateFreq + " Top freq=" + this.maxTemplateFreq);
            //Console.WriteLine(" Top scan bin=" + this.TopScanBin + "  Mid scan bin=" + s.MidScanBin + "  Bottom scan bin=" + s.BottomScanBin);
        }

    }//end Class Template


        /// <summary>
    /// 
    /// </summary>
    public class TemplateConfig
    {
        private string wavFileExt = ".wav"; //default value
        public string WavFileExt { get { return wavFileExt; } set { wavFileExt = value; } }
        private string bmpFileExt = ".bmp";//default value
        public string BmpFileExt { get { return bmpFileExt; } set { bmpFileExt = value; } }


        //wav file info
        public string WavFileDir { get; set; }
        public string WavFName { get; set; }

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

        public double FBinWidth { get; set; }
        public double MinPower { get; set; }//min power in sonogram
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

        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }
        public int ZscoreSmoothingWindow { get; set; }
        public double ZScoreThreshold { get; set; }
        public int Verbosity { get; set; }



        public void CopyConfig(Configuration cfg)
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
            this.ZscoreSmoothingWindow = cfg.GetInt("ZSCORE_SMOOTHING_WINDOW");
            this.ZScoreThreshold = cfg.GetDouble("ZSCORE_THRESHOLD");
            this.Verbosity = cfg.GetInt("VERBOSITY");
            this.BlurNH = cfg.GetInt("BLUR_NEIGHBOURHOOD");
            this.BlurNH_time = cfg.GetInt("BLUR_TIME_NEIGHBOURHOOD");
            this.BlurNH_freq = cfg.GetInt("BLUR_FREQ_NEIGHBOURHOOD");
            this.NormSonogram = cfg.GetBoolean("NORMALISE_SONOGRAM");
        }
    }//end class TemplateConfig


}
