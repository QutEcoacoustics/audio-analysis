using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using TowseyLib;
using Props = TowseyLib.Params;


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
    public class CallTemplate
    {
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
        private int sampleRate;
        public int SampleRate { get { return sampleRate; } set { sampleRate = value; } }
        private int sampleCount;
        private double recordingLength;
        public double RecordingLength { get { return recordingLength; } set { recordingLength = value; } }
        private double timeBin; //duration of non-overlapped part of one window

        //info about original SONOGRAM
        private int windowSize;
        private double windowOverlap;
        private int spectrumCount;//number of spectra in original sonogram
        private double spectraPerSecond;
        private double spectrumDuration;
        private double nonOverlapDuration; //duration of non-overlapped part of window
        private string windowFunction = "Hamming";
        private int maxFreq;
        public int MaxFreq { get { return maxFreq; } set { maxFreq = value; } }
        private int freqBinCount;
        private double hzBin;
        private double minSgPower; // min and max power in sonogram
        private double maxSgPower;
        private double minSgCutoff; // min and max percentile cutoffs in sonogram
        private double maxSgCutoff;
        

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

        private int smoothingWindowWidth=3;
        public int SmoothingWindowWidth { get { return smoothingWindowWidth; } set { smoothingWindowWidth = value; } }
        //private int verbosity = 0;



        /// <summary>
        /// CONSTRUCTOR 1
        /// </summary>
        /// <param name="callID"></param>
        public CallTemplate(int callID)
        {
            this.callID = callID;
        }

        /// <summary>
        /// CONSTRUCTOR 2
        /// Reads a call template from file using CallID for identifier
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="templateDir"></param>
        public CallTemplate(int callID, string templateDir)
        {
               this.callID = callID;
               this.TemplateDir = templateDir;
               this.dataFName = callStemName + "_" + callID + ".txt";
               this.matrixFName = templateStemName + "_" + callID + ".txt";
               this.imageFName = templateStemName + "_" + callID + ".bmp";

               int status = ReadCallDataFile();
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
        public CallTemplate(int callID, string callName, string callComment, string templateDir)
        {
            this.callID = callID;
            this.callName = callName;
            this.callComment = callComment;
            this.TemplateDir = templateDir;
            this.dataFName = callStemName + "_" + callID + ".txt";
            this.matrixFName = templateStemName + "_" + callID + ".txt";
            this.imageFName = templateStemName + "_" + callID + CallTemplate.bmpFileExt;
        }

        public void SetWavFileName(string wavFileName)
        {
            this.wavFname    = wavFileName+wavFileExt;
        }


        public void SetSonogramInfo(Sonogram s)
        {
            //wav file info
            this.sampleRate = s.SampleRate;
            this.sampleCount = s.SampleCount;
            this.maxFreq = s.MaxFreq;
            this.recordingLength = s.AudioDuration;

            //sonogram info
            this.spectrumCount = s.SpectrumCount;
            this.windowSize = s.WindowSize;
            this.windowOverlap = s.WindowOverlap;
            this.windowFunction = s.WindowFncName;
            this.minSgPower = s.MinP;
            this.maxSgPower = s.MaxP;
            this.minSgCutoff = s.MinCut;
            this.maxSgCutoff = s.MaxCut;
            this.timeBin = this.recordingLength / (double)this.spectrumCount;
            this.spectraPerSecond = spectrumCount / (double)recordingLength;
            this.spectrumDuration = windowSize / (double)sampleRate;
            this.nonOverlapDuration = spectrumDuration * (1 - windowOverlap);
            //this.spectrumDuration = recordingLength / (double)spectrumCount;
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
            this.freqBinCount = sMatrix.GetLength(1);
            this.spectrumCount = timeStepCount;
            this.hzBin = this.maxFreq / (double)this.freqBinCount;

            this.Matrix = s.Matrix;
            ConvertImageCoords2SonogramCoords(freqBinCount, imageCoords);
            this.Matrix = DataTools.Submatrix(sMatrix, this.t1, this.bin1, this.t2, this.bin2);
            DataTools.getMinMax(this.Matrix, out this.minTemplatePower, out this.maxTemplatePower);
            //this.Template = DataTools.normalise(this.Template);
        }//end ExtractTemplate

        public void ExtractTemplateFromImage2File(Sonogram s, params int[] imageCoords)
        {
            ExtractTemplateUsingImageCoordinates(s, imageCoords);
            FileTools.WriteMatrix2File_Formatted(this.matrix, this.TemplateDir + this.matrixFName, "F5");
        }

        public void WriteCallData2File()
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
            data.Add(" WAV_SAMPLE_RATE="+this.sampleRate);
            data.Add(" WAV_DURATION="+recordingLength.ToString("F3"));

            data.Add("#\n#INFO ABOUT SONOGRAM");
            data.Add(" FFT_WINDOW_SIZE=" + windowSize);
            data.Add(" FFT_WINDOW_OVERLAP=" + windowOverlap);
            data.Add(" WINDOW_DURATION_MS=" + (int)(this.spectrumDuration * 1000));
            data.Add(" NONOVERLAP_WINDOW_DURATION_MS=" + (int)(this.nonOverlapDuration * 1000));
            data.Add(" NUMBER_OF_SPECTRA=" + spectrumCount);
            data.Add(" SPECTRA_PER_SECOND=" + spectraPerSecond.ToString("F3"));
            data.Add(" WINDOW_FUNCTION=" + windowFunction);
            data.Add(" MAX_FREQ=" + maxFreq);
            data.Add(" NUMBER_OF_FREQ_BINS=" + freqBinCount);
            data.Add(" FREQ_BIN_WIDTH=" + hzBin.ToString("F2")+"hz");
            data.Add(" MIN_POWER=" + minSgPower.ToString("F3"));
            data.Add(" MAX_POWER=" + maxSgPower.ToString("F3"));
            data.Add(" MIN_CUTOFF=" + minSgCutoff.ToString("F3"));
            data.Add(" MAX_CUTOFF=" + maxSgCutoff.ToString("F3"));
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
            data.Add(" SMOOTH_WINDOW_WIDTH=" + this.SmoothingWindowWidth.ToString());

            //write data to file
            FileTools.WriteTextFile(this.templateDir + this.dataFName, data);

        } // end of WriteCallData2File()

        public int ReadCallDataFile()
        {
            Console.WriteLine("#####  READING CALL DATA FILE");
            Console.WriteLine("       FILE NAME=" + dataFName);
            int status = 0;
            Props props = new Props(TemplateDir + dataFName);
            this.callName = props.GetValue("CALL_NAME"); 
            //Console.WriteLine("  Call Name=" + this.callName);
            this.callComment = props.GetValue("CALL_COMMENT");
            this.sampleRate = props.GetInt("WAV_SAMPLE_RATE");
            this.maxFreq = props.GetInt("MAX_FREQ");
            this.recordingLength = props.GetDouble("WAV_DURATION");
            this.freqBinCount = props.GetInt("NUMBER_OF_FREQ_BINS");
            this.midTemplateFreq = props.GetInt("TEMPLATE_MID_FREQ");
            this.smoothingWindowWidth = props.GetInt("SMOOTH_WINDOW_WIDTH");
            return status;
        } //end of ReadCallDataFile()


        public void SaveDataAndImageToFile()
        {
            WriteCallData2File();
            SaveImage();
        }

        public int ReadTemplateFile()
        {
            Console.WriteLine("#####  READING TEMPLATE DATA FILE");
            Console.WriteLine("       FILE NAME=" + matrixFName);
            int status = 0;
            this.matrix = FileTools.ReadDoubles2Matrix(TemplateDir + matrixFName); ;
            return status;
        } //end of ReadTemplateFile()

        public Bitmap GetImage()
        {
            // prepare Bitmap image of Template
            double Audioduration = -Double.MaxValue; //not required
            double threshold = 0.0;
            BitMaps bmps = new BitMaps(SampleRate, Audioduration, threshold);
            //Bitmap bmp = bmps.CreateBitmap(Matrix, minSgPower, maxSgPower, false, null, 0, 0);
            Bitmap bmp = bmps.CreateBitmap(Matrix, minSgCutoff, maxSgCutoff, false, null, 0, 0);
            return bmp;
        }

        public void SaveImage()
        {
            Bitmap bmp = GetImage();
            string imageFileName = TemplateDir + this.imageFName;
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
            Console.WriteLine(" Top freq=" + this.maxTemplateFreq + "  Mid freq=" + this.MidTemplateFreq + "  Bottom freq=" + this.minTemplateFreq);
            //Console.WriteLine(" Top scan bin=" + this.TopScanBin + "  Mid scan bin=" + s.MidScanBin + "  Bottom scan bin=" + s.BottomScanBin);
            Console.WriteLine(" Width of Score Smoothing Window=" + this.smoothingWindowWidth);
        }

    }//end Class CallTemplate
}
