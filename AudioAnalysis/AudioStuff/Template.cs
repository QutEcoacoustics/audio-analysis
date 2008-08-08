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
    /// This could be the single syllable of a bird or frog call or a short slice of rain or cicada noise.
    /// </remarks>
    /// 
    public class Template
    {
        private const string templateStemName = "template";
        private const string callStemName = "call";

        public int CallID { get; set; }
        public bool DoMelConversion { get; set; }
        private SonoConfig templateState;
        public  SonoConfig TemplateState { get { return templateState; } set { templateState = value; } }
        //private SonoConfig sonogramState;
        //public  SonoConfig SonogramState { get { return sonogramState; } set { sonogramState = value; } }
        private Sonogram   sonogram;
        public  Sonogram   Sonogram { get { return sonogram; } set { sonogram = value; } }

        
        //file names and directory
        private string templateDir;
        public string TemplateDir { get { return templateDir; } set { templateDir = value; } }
        private string vectorFName;
        public string VectorFName { get { return vectorFName; } set { vectorFName = value; } }
        private string matrixFName;
        public string MatrixFName { get { return matrixFName; } set { matrixFName = value; } }
        private string dataFName;
        public string DataFName { get { return dataFName; } set { dataFName = value; } }
        //private string sonogramImageFname;
        //public string SonogramImageFname { get { return sonogramImageFname; } set { sonogramImageFname = value; } }
        private string imageFName;
        public string ImageFName { get { return imageFName; } set { imageFName = value; } }


        //info about OLD TEMPLATE EXTRACTION
        int x1; int y1; //image coordinates for top left of selection
        int x2; int y2; //image coordinates for bottom right of selection
        int t1; int t2; //sonogram time interval selection
        int bin1; int bin2; //sonogram freq bin interval 
        double templateDuration; //duration of template in seconds
        int templateSpectralCount; //number of spectra in template        
        private double minTemplatePower; // min and max power in template
        private double maxTemplatePower;


        //info about NEW TEMPLATE EXTRACTION
        private int[] timeIndices;
        private double[] featureVector;
        public  double[] FeatureVector { get { return featureVector; } }


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
        /// Use this constructor to read an existing template file
        /// </summary>
        /// <param name="callID"></param>
        public Template(string iniFPath, int callID)
        {
            this.templateState = new SonoConfig();
            this.templateState.ReadConfig(iniFPath);//read the ini file for default parameters

            this.CallID = callID;
        }

         
        /// <summary>
        /// CONSTRUCTOR 2
        /// Creates a new call template using info provided
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="callName"></param>
        /// <param name="callComment"></param>
        //public Template(int callID, string callName, string callComment)
        //{
        //    this.callID = callID;
        //    this.callName = callName;
        //    this.callComment = callComment;
        //    this.templateState = new TemplateConfig();
        //}

        
        public Template(string iniFPath, int callID, string callName, string callComment, string sourceFileStem)
        {
            this.templateState = new SonoConfig();
            this.templateState.ReadConfig(iniFPath);//read the ini file for default parameters
            this.CallID = callID;
            this.templateState.CallID = callID;
            this.templateState.CallName = callName;
            this.templateState.CallComment = callComment;
            this.templateState.SourceFStem = sourceFileStem;
            this.templateState.SourceFName = sourceFileStem + this.templateState.WavFileExt;
            this.templateState.SourceFPath = this.templateState.WavFileDir + "\\" + this.templateState.SourceFName;
            this.dataFName   = this.templateState.TemplateDir + Template.callStemName + "_" + callID + ".txt";
            this.matrixFName = this.templateState.TemplateDir + Template.templateStemName + "_" + callID + ".txt";
            Console.WriteLine("dataFName=" + dataFName);
        }


        public void SetMfccParameters(int frameSize, double frameOverlap, int minFreq, int maxFreq, 
                                double dynamicRange, int filterBankCount, bool doMelConversion, 
                                int ceptralCoeffCount, int deltaT, bool includeDeltaFeatures, bool includeDoubleDeltaFeatures)
        {
            this.templateState.WindowSize      = frameSize;
            this.templateState.WindowOverlap   = frameOverlap;
            this.templateState.FreqBand_Min    = minFreq;
            this.templateState.FreqBand_Max    = maxFreq;
            this.templateState.MinTemplateFreq = minFreq;
            this.templateState.MaxTemplateFreq = maxFreq;
            this.templateState.MidTemplateFreq = minFreq + ((maxFreq - minFreq) / 2); //Hz

            this.DoMelConversion = doMelConversion;
            if (doMelConversion) this.templateState.SonogramType = SonogramType.melCepstral;
            else                 this.templateState.SonogramType = SonogramType.linearCepstral;
            this.templateState.DeltaT = deltaT;
            this.templateState.IncludeDelta = includeDeltaFeatures;
            this.templateState.IncludeDoubleDelta = includeDoubleDeltaFeatures;
        }


        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
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
            //double FrameOffset = duration of non-overlapped part of window/frame in seconds
            this.templateDuration = templateSpectralCount * this.templateState.FrameOffset; // timeBin; //duration of template
            int min = (int)(bin1 * this.templateState.FBinWidth);
            int max = (int)(bin2 * this.templateState.FBinWidth);
            this.templateState.MinTemplateFreq = min; 
            this.templateState.MaxTemplateFreq = max;
            this.templateState.MidTemplateFreq = min + ((max - min) / 2); //Hz
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
            //convert image coordinates to sonogram coords
            //sonogram: rows=timesteps; sonogram cols=freq bins
            double[,] sMatrix = s.Matrix;
            int timeStepCount = sMatrix.GetLength(0);
            this.sonogram.State.FreqBinCount = sMatrix.GetLength(1);
            //this.spectrumCount = timeStepCount;
            //this.hzBin = this.sonogram.State.NyquistFreq / (double)this.sonogram.State.FreqBinCount;

            this.Matrix = s.Matrix;
            ConvertImageCoords2SonogramCoords(this.sonogram.State.FreqBinCount, imageCoords);
            this.Matrix = DataTools.Submatrix(sMatrix, this.t1, this.bin1, this.t2, this.bin2);
            DataTools.MinMax(this.Matrix, out this.minTemplatePower, out this.maxTemplatePower);
        }//end ExtractTemplate

        public void ExtractTemplateFromImage2File(Sonogram s, params int[] imageCoords)
        {
            ExtractTemplateUsingImageCoordinates(s, imageCoords);
            FileTools.WriteMatrix2File_Formatted(this.matrix, this.matrixFName, "F5");
        }
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************
        //*************************************************************************************************************************


        public void ExtractTemplateFromSonogram(int[] timeIndices)
        {
            this.timeIndices = timeIndices;
            //init Sonogram. THis also makes the sonogram.
            this.sonogram = new Sonogram(this.templateState, this.templateState.SourceFPath);


            //Console.WriteLine("  deltaT=" + this.templateState.DeltaT + "  doDelta=" + this.templateState.IncludeDelta + "  DoDoubleDelta=" + this.templateState.IncludeDoubleDelta);

            //normalise energy between 0.0 decibels and max decibels.
            int L = this.sonogram.Decibels.Length;
            double[]  E = new double[L];
            double min = this.templateState.MinDecibelReference;
            double max = this.templateState.MaxDecibelReference;
            double range = max - min;
            for (int i = 0; i < L; i++) E[i] = (this.sonogram.Decibels[i] - min) / range;

            //normalise the MFCC spectrogram
            double[,] M = DataTools.normalise(this.sonogram.Specgram);

            //get first acoustic vector
            int dT = this.templateState.DeltaT; 
            bool doD  = this.templateState.IncludeDelta; 
            bool doDD = this.templateState.IncludeDoubleDelta;
            double[] acousticV = Speech.GetFeatureVector(E, M, timeIndices[0], dT, doD, doDD);
            int avL = acousticV.Length;
            int indicesL = timeIndices.Length;
            //Console.WriteLine(" avL=" + avL);

            for(int i = 1; i < indicesL; i++)
            {
                double[] v = Speech.GetFeatureVector(E, M, timeIndices[i], dT, doD, doDD);
                for (int j = 0; j < avL; j++) acousticV[j] += v[j];
            }

            //initialise feature vector for template and transfer values
            this.featureVector = new double[avL]; 
            //transfer average of values
            for(int i = 0; i < avL; i++)
            {
                this.featureVector[i] = acousticV[i] / (double)indicesL;
            }

            //write all files
            SaveDataAndImageToFile();
        }



        
        public void WriteTemplateConfigFile()
        {
            //write the call data to a file
            ArrayList data = new ArrayList();
            data.Add("DATE=" + DateTime.Now.ToString("u"));
            data.Add("#\n#TEMPLATE DATA");
            data.Add("CALL_ID=" + this.templateState.CallID);
            data.Add("CALL_NAME=" + this.templateState.CallName);
            data.Add("CALL_COMMENT=" + this.templateState.CallComment);
            data.Add("THIS_FILE=" + this.dataFName);

            data.Add("#\n#INFO ABOUT ORIGINAL .WAV FILE");
            data.Add("WAV_FILE_NAME=" + this.templateState.SourceFName);
            data.Add("WAV_SAMPLE_RATE=" + this.templateState.SampleRate);
            data.Add("WAV_DURATION=" + this.templateState.TimeDuration.ToString("F3"));

            data.Add("#\n#INFO ABOUT FRAMES");
            data.Add("FRAME_SIZE=" + this.templateState.WindowSize);
            data.Add("FRAME_OVERLAP=" + this.templateState.WindowOverlap);
            data.Add("FRAME_DURATION_MS=" + (this.templateState.FrameDuration * 1000).ToString("F3"));//convert to milliseconds
            data.Add("FRAME_OFFSET_MS=" + (this.templateState.FrameOffset * 1000).ToString("F3"));//convert to milliseconds
            data.Add("NUMBER_OF_FRAMES=" + this.templateState.SpectrumCount);
            data.Add("FRAMES_PER_SECOND=" + this.templateState.SpectraPerSecond.ToString("F3"));


            data.Add("#\n#INFO ABOUT SONOGRAM");
            data.Add("WINDOW_FUNCTION=" + this.templateState.WindowFncName);
            data.Add("NYQUIST_FREQ=" + this.templateState.NyquistFreq);
            data.Add("NUMBER_OF_FREQ_BINS=" + this.templateState.FreqBinCount);
            data.Add("FREQ_BIN_WIDTH=" + this.templateState.FBinWidth.ToString("F2") + "hz");
            //data.Add("MIN_POWER=" + this.sonogram.State.PowerMin.ToString("F3"));
            //data.Add("AVG_POWER=" + this.sonogram.State.PowerAvg.ToString("F3"));
            //data.Add("MAX_POWER=" + this.sonogram.State.PowerMax.ToString("F3"));
            //data.Add("MIN_CUTOFF=" + this.sonogram.State.MinCut.ToString("F3"));
            //data.Add("MAX_CUTOFF=" + this.sonogram.State.MaxCut.ToString("F3"));
            //data.Add("SONOGRAM_IMAGE_FILE=" + this.SonogramImageFname);

            data.Add("#\n#INFO ABOUT TEMPLATE");
            data.Add("TEMPLATE_VECTOR_FILE=" + matrixFName);
            data.Add("MIN_FREQ=" + this.templateState.MinTemplateFreq);
            data.Add("MAX_FREQ=" + this.templateState.MaxTemplateFreq);
            data.Add("MID_FREQ=" + this.templateState.MidTemplateFreq);
            data.Add("DO_MEL_CONVERSION=" + this.DoMelConversion);
            data.Add("DELTA_T=" + this.templateState.DeltaT);
            data.Add("INCLUDE_DELTA=" + this.templateState.IncludeDelta);
            data.Add("INCLUDE_DOUBLEDELTA=" + this.templateState.IncludeDoubleDelta);
            data.Add("FILTERBANK_COUNT=" + this.templateState.FilterbankCount);
            data.Add("CC_COUNT=" + this.templateState.ccCount);

            data.Add("DYNAMIC_RANGE=" + this.templateState.MaxDecibelReference); //decibels above noise level #### YET TO DO THIS PROPERLY
            //backgroundFilter= //noise reduction??
            //maxSyllables=
            //double maxSyllableGap = 0.25; //seconds
            //double maxSong=
            StringBuilder frameIDs = new StringBuilder();
            for (int i = 0; i < timeIndices.Length; i++)
            {
                frameIDs.Append(this.timeIndices[i]);
                frameIDs.Append(",");
            }

            data.Add("SELECTED_FRAMES=" + frameIDs.ToString());

            //write data to file
            FileTools.WriteTextFile(this.dataFName, data);

        } // end of WriteCallData2File()



        public void WriteTemplateConfigFile_OLD()
        {
            //write the call data to a file
            ArrayList data = new ArrayList();
            data.Add("DATE=" + DateTime.Now.ToString("u"));
            data.Add("#\n#TEMPLATE DATA");
            data.Add("CALL_ID=" + this.templateState.CallID);
            data.Add("CALL_NAME=" + this.templateState.CallName);
            data.Add("CALL_COMMENT=" + this.templateState.CallComment);
            data.Add("THIS_FILE=" + this.dataFName);

            data.Add("#\n#INFO ABOUT ORIGINAL .WAV FILE");
            data.Add("WAV_FILE_NAME=" + this.templateState.SourceFName);
            data.Add("WAV_SAMPLE_RATE=" + this.templateState.SampleRate);
            data.Add("WAV_DURATION=" + this.templateState.TimeDuration.ToString("F3"));

            data.Add("#\n#INFO ABOUT FRAMES");
            data.Add("FRAME_SIZE=" + this.templateState.WindowSize);
            data.Add("FRAME_OVERLAP=" + this.templateState.WindowOverlap);
            data.Add("FRAME_DURATION_MS=" + (this.templateState.FrameDuration * 1000).ToString("F3"));//convert to milliseconds
            data.Add("FRAME_OFFSET_MS=" + (this.templateState.FrameOffset * 1000).ToString("F3"));//convert to milliseconds
            data.Add("NUMBER_OF_FRAMES=" + this.templateState.SpectrumCount);
            data.Add("FRAMES_PER_SECOND=" + this.templateState.SpectraPerSecond.ToString("F3"));


            data.Add("#\n#INFO ABOUT SONOGRAM");
            data.Add("WINDOW_FUNCTION=" + this.templateState.WindowFncName);
            data.Add("NYQUIST_FREQ=" + this.templateState.NyquistFreq);
            data.Add("NUMBER_OF_FREQ_BINS=" + this.templateState.FreqBinCount);
            data.Add("FREQ_BIN_WIDTH=" + this.templateState.FBinWidth.ToString("F2") + "hz");
            data.Add("MIN_POWER=" + this.templateState.PowerMin.ToString("F3"));
            data.Add("AVG_POWER=" + this.templateState.PowerAvg.ToString("F3"));
            data.Add("MAX_POWER=" + this.templateState.PowerMax.ToString("F3"));
            data.Add("MIN_CUTOFF=" + this.templateState.MinCut.ToString("F3"));
            data.Add("MAX_CUTOFF=" + this.templateState.MaxCut.ToString("F3"));
            //data.Add("SONOGRAM_IMAGE_FILE=" + this.SonogramImageFname);

            data.Add("#\n#INFO ABOUT CALL TEMPLATE");
            data.Add("TEMPLATE_MATRIX_FILE=" + matrixFName);
            data.Add("# NOTE: Each row of the template matrix is the power spectrum for a given time step.");
            data.Add("#       That is, rows are time steps and columns are frequency bins.");
            data.Add("# IMAGE COORDINATES USED TO EXTRACT CALL");
            data.Add("X1=" + this.x1);
            data.Add("Y1=" + this.y1);
            data.Add("X2=" + this.x2);
            data.Add("Y2=" + this.y2);
            data.Add("# CORRESPONDING SONOGRAM COORDINATES");
            data.Add("TIMESTEP1=" + this.t1);
            data.Add("TIMESTEP2=" + this.t2);
            data.Add("FREQ_BIN1=" + this.bin1);
            data.Add("FREQ_BIN2=" + this.bin2);
            data.Add("TEMPLATE_IMAGE_FILE=" + this.imageFName);
            data.Add("TEMPLATE_DURATION=" + this.templateDuration.ToString("F3") + "s");
            data.Add("TEMPLATE_SPEC_COUNT=" + this.templateSpectralCount + "(time-steps)");
            data.Add("TEMPLATE_FBIN_COUNT=" + (this.bin2 - this.bin1 + 1));
            data.Add("TEMPLATE_MAX_FREQ=" + this.templateState.MaxTemplateFreq);
            data.Add("TEMPLATE_MID_FREQ=" + this.templateState.MidTemplateFreq);
            data.Add("TEMPLATE_MIN_FREQ=" + this.templateState.MinTemplateFreq);
            //data.Add("TEMPLATE_MIN_POWER=" + this.minTemplatePower.ToString("F3"));
            //data.Add("TEMPLATE_MAX_POWER=" + this.maxTemplatePower.ToString("F3"));

            //data.Add("#");
            //data.Add("#INFO ABOUT SCORE PROCESSING");
            //data.Add("#NIGHT NOISE RESPONSE");
            //data.Add("NOISE_AV=-0.03421");
            //data.Add("NOISE_SD=0.00043");
            //data.Add("#RAIN NOISE RESPONSE");
            //data.Add("#NOISE_AV=-0.02976");
            //data.Add("#NOISE_SD=0.00042");



            //write data to file
            FileTools.WriteTextFile(this.dataFName, data);

        } // end of WriteCallData2File()


        //public int ReadTemplateConfigFile()
        //{
        //    int status = 0;
        //    Console.WriteLine("\n#####  READING TEMPLATE INFO");
        //    Console.WriteLine("       FILE NAME=" + dataFName);
        //    Configuration cfg = new Props(this.templateState.TemplateDir + dataFName);
        //    this.templateState.CallName = cfg.GetString("CALL_NAME");
        //    this.templateState.CallComment = cfg.GetString("CALL_COMMENT");

        //    this.templateState = new TemplateConfig();
        //    this.templateState.SampleRate = cfg.GetInt("WAV_SAMPLE_RATE");
        //    this.templateState.NyquistFreq = cfg.GetInt("MAX_FREQ");
        //    this.templateState.AudioDuration = cfg.GetDouble("WAV_DURATION");
        //    this.templateState.FreqBinCount = cfg.GetInt("NUMBER_OF_FREQ_BINS");
        //    this.templateState.SpectrumCount = cfg.GetInt("NUMBER_OF_SPECTRA");
        //    this.templateState.SpectraPerSecond = cfg.GetDouble("SPECTRA_PER_SECOND");

        //    this.templateState.WindowSize = cfg.GetInt("FFT_WINDOW_SIZE");
        //    this.templateState.WindowOverlap = cfg.GetDouble("FFT_WINDOW_OVERLAP");
        //    this.templateState.WindowDuration = cfg.GetDouble("WINDOW_DURATION_MS") / (double)1000; //convert ms to seconds
        //    this.templateState.NonOverlapDuration = cfg.GetDouble("NONOVERLAP_WINDOW_DURATION_MS") / (double)1000; //convert ms to seconds

        //    this.midTemplateFreq = cfg.GetInt("TEMPLATE_MID_FREQ");
        //    this.templateState.NoiseAv = cfg.GetDouble("NOISE_AV");
        //    this.templateState.NoiseSd = cfg.GetDouble("NOISE_SD");

//                    if (doMelConversion) this.templateState.SonogramType = SonogramType.melCepstral;
//            else this.templateState.SonogramType = SonogramType.linearCepstral;



        //    return status;
        //} //end of ReadCallDataFile()


        public void SaveDataAndImageToFile()
        {
            FileTools.WriteArray2File_Formatted(this.featureVector, this.matrixFName, "F5");
            WriteTemplateConfigFile();

            //save the image
            //SonoImage bmps = new SonoImage(this.templateState, SonogramType.linearScale, TrackType.none);
            //Bitmap bmp = bmps.CreateBitMapOfTemplate(Matrix);
            //bmp.Save(this.imageFName);
        }

        public int ReadTemplateFile()
        {
            Console.WriteLine("\n#####  READING TEMPLATE DATA");
            Console.WriteLine("       FILE NAME=" + matrixFName);
            int status = 0;
            this.matrix = FileTools.ReadDoubles2Matrix(TemplateDir + matrixFName); ;
            return status;
        } //end of ReadTemplateFile()
        

        public void SaveImage()
        {
            SonoImage bmps = new SonoImage(this.templateState, SonogramType.linearScale, TrackType.none);
            Bitmap bmp = bmps.CreateBitMapOfTemplate(Matrix);
            bmp.Save(this.imageFName);
        }

        public void WriteInfo2STDOUT()
        {
            Console.WriteLine("\nTEMPLATE INFO");
            Console.WriteLine(" Template ID: " + this.CallID);
            Console.WriteLine(" Template name: " + this.templateState.CallName);
            Console.WriteLine(" Comment: " + this.templateState.CallComment);
            Console.WriteLine(" Template directory: " + this.templateState.TemplateDir);
            Console.WriteLine(" Template data  in file " + this.DataFName);
            Console.WriteLine(" Template image in file " + this.ImageFName);
            Console.WriteLine(" Template matrix in file " + this.MatrixFName);
            Console.WriteLine(" Bottom freq=" + this.templateState.MinTemplateFreq + "  Mid freq=" + this.templateState.MidTemplateFreq + " Top freq=" + this.templateState.MaxTemplateFreq);
        }

    }//end Class Template


}
