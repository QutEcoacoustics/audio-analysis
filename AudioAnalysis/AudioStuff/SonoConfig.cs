using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using TowseyLib;
using System.Text.RegularExpressions;

namespace AudioStuff
{
    public class SonoConfig
    {
		public static SonoConfig Load(string path)
		{
			var retVal = new SonoConfig();
			retVal.ReadConfig(path);
			return retVal;
		}

        //files and directories
        public string TemplateParentDir { get; set; } //parent directory for all templates
        public string TemplateDir { get; set; }       //contains a single template for specific call ID
		public string WavFilePath {get; set;} // The path to the 'current' wav file
        public string OutputDir { get; set; }
        public string BmpFName { get; set; }
        public string BmpFileExt { get; set; }

        //wav file info
        public string DeploymentName { get; set; }
        public DateTime? Time { get; set; }
        public int TimeSlot { get; set; }
        //public double WavMax { get; set; }

        //SIGNAL PARAMETERS
        public int SampleRate { get; set; }
        //public int SampleCount { get; set; }
        public double TimeDuration { get; set; }

        // FRAMING or WINDOWING
        public int SubSample { get; set; }         //use this to reduce sampling rate esp if SR > 22 kHz
        public int WindowSize { get; set; }
        public double WindowOverlap { get; set; }  //percent overlap of frames
        public double FrameDuration { get; set; }  //duration of full frame or window in seconds
        public double FrameOffset { get; set; }    //duration of non-overlapped part of window/frame in seconds
        public int FrameCount { get; set; }        //number of frames
        public double FramesPerSecond { get; set; }

        //SIGNAL FRAME ENERGY AND SEGMENTATION PARAMETERS
        public double FrameMax_dB { get; set; }
        public double FrameNoise_dB { get; set; }
        public double Frame_SNR { get; set; }
        public double NoiseSubtracted { get; set; }         //noise (dB) subtracted from each frame decibel value
        public double MinDecibelReference { get; set; }     //min reference dB value after noise substraction
        public double MaxDecibelReference { get; set; }     //max reference dB value after noise substraction
        public double SegmentationThreshold_k1 { get; set; }//dB threshold for recognition of vocalisations
        public double SegmentationThreshold_k2 { get; set; }//dB threshold for recognition of vocalisations
        public double k1_k2Latency { get; set; }            //seconds delay between signal reaching k1 and k2 thresholds
        public double vocalDelay { get; set; }              //seconds delay required to separate vocalisations 
        public double minPulseDuration { get; set; }        //minimum length of energy pulse - do not use this
        public double FractionOfHighEnergyFrames { get; set; }//fraction of frames with energy above SegmentationThreshold_k2

        //SPECTRAL ENERGY AND SEGMENTATION PARAMETERS
        public double FreqBandMax_dB { get; set; }
        public double FreqBandNoise_dB { get; set; }
        public double FreqBand_SNR { get; set; }
        public double FreqBand_NoiseSubtracted { get; set; }         //noise (dB) subtracted from each frame decibel value
        public double FreqBand_MinDecibelReference { get; set; }     //min reference dB value after noise substraction
        public double FreqBand_MaxDecibelReference { get; set; }     //max reference dB value after noise substraction

        //SONOGRAM parameters
        public int MinFreq { get; set; }                   //default min freq = 0 Hz  
        public int NyquistFreq { get; set; }               //default max freq = Nyquist = half audio sampling freq
        public int FreqBinCount { get; set; }         //number of FFT values 
        public double FBinWidth { get; set; }
        public int kHzBandCount { get; set; }         //number of one kHz bands
        public int freqBand_Min = -1000;              //min of the freq band to be analysed  
        public int FreqBand_Min { get { return freqBand_Min; } set { freqBand_Min = value; } }
        public int freqBand_Max = -1000;              //max of the freq band to be analysed
        public int FreqBand_Max { get { return freqBand_Max; } set { freqBand_Max = value; } }
        public int FreqBand_Mid { get; set; }
        public bool doFreqBandAnalysis = false;
        public double PowerMin { get; set; }                //min power in sonogram
        public double PowerAvg { get; set; }                //average power in sonogram
        public double PowerMax { get; set; }                //max power in sonogram

        //FFT parameters
        public string WindowFncName { get; set; }
        public FFT.WindowFunc WindowFnc { get; set; }
        public int NPointSmoothFFT { get; set; }      //number of points to smooth FFT spectra

        // MEL SCALE PARAMETERS
        public int FilterbankCount { get; set; }
        public int MelBinCount { get; set; }    //number of mel spectral values 
        public double MinMelPower { get; set; } //min power in mel sonogram
        public double MaxMelPower { get; set; } //max power in mel sonogram
        public double MaxMel { get; set; }      //Nyquist frequency on Mel scale

        // MFCC parameters
        public SonogramType SonogramType { get; set; }
        public bool DoMelScale { get; set; }
        public bool DoNoiseReduction { get; set; }
        public int ccCount { get; set; }     //number of cepstral coefficients
        public double MinCepPower { get; set; } //min value in cepstral sonogram
        public double MaxCepPower { get; set; } //max value in cepstral sonogram
		public int DeltaT { get; set; }
		public bool IncludeDelta { get; set; }
		public bool IncludeDoubleDelta { get; set; }

        //FEATURE VECTOR PARAMETERS 
        public FV_Source FeatureVectorSource { get; set; }
        public string[] FeatureVector_SelectedFrames { get; set; } //store frame IDs as string array
        public int MarqueeStart { get; set; }
        public int MarqueeEnd { get; set; }
        public FV_Extraction FeatureVectorExtraction { get; set; }
        public int FeatureVectorExtractionInterval { get; set; }
        public bool FeatureVector_DoAveraging { get; set; }
        public string FeatureVector_DefaultNoiseFile { get; set; }

        public int FeatureVectorCount { get; set; }
        public int FeatureVectorLength { get; set; }
        public string[] FeatureVectorPaths { get; set; }
        public string[] FVSourceFiles { get; set; }
        public string DefaultNoiseFVFile { get; set; }
        public int ZscoreSmoothingWindow = 3; //NB!!!! THIS IS NO LONGER A USER DETERMINED PARAMETER

        //THE LANGUAGE MODEL
        public int WordCount { get; set; }
        public string[] Words { get; set; }
        public MarkovModel WordModel { get; set; }
        public HMMType HmmType { get; set; }
        public string HmmName { get; set; }
        public double SongWindow { get; set; } //window duration in seconds - used to calculate statistics

        //BITMAP IMAGE PARAMETERS 
        public bool AddGrid { get; set; }
        public TrackType TrackType { get; set; }

        public double MinPercentile { get; set; }
        public double MaxPercentile { get; set; }
        public double MinCut { get; set; } //power of min percentile
        public double MaxCut { get; set; } //power of max percentile

        //TEMPLATE PARAMETERS
        public int CallID { get; set; }
        public string CallName { get; set; }
        public string CallComment { get; set; }
        public string FileDescriptor { get; set; }
		public string SourceFilePath { get; set; } // The path to the wav file used to create the template

        //freq bins of the scanned part of sonogram
        public int MaxTemplateFreq { get; set; }
        public int MidTemplateFreq { get; set; }
        public int MinTemplateFreq { get; set; }

        public int BlurWindow { get; set; }
        public int BlurWindow_time { get; set; }
        public int BlurWindow_freq { get; set; }
        //public bool NormSonogram { get; set; }

        public double ZScoreThreshold { get; set; }
        public double NoiseAv { get; set; }
        public double NoiseSd { get; set; }

		static bool ParseFileName(string filname, out string deployment, out DateTime time)
		{
			var m = Regex.Match(filname, @"(.*?)_(.*)");
			if (m.Success)
			{
				deployment = m.Groups[1].Value;
				time = DateTime.ParseExact(m.Groups[2].Value, "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat);
			}
			else
			{
				deployment = null;
				time = DateTime.MinValue;
			}
			return m.Success;
		}

        /// <summary>
        /// converts wave file names into component info 
        /// wave file name have following format: "BAC1_20071008-081607"
        /// </summary>
        /// <param name="FName"></param>
        public void SetDateAndTime(string fName)
        {
			string deployment; DateTime time;
			if (string.IsNullOrEmpty(fName) || !ParseFileName(fName, out deployment, out time))
			{
				this.DeploymentName = fName ?? "noName";
				SetDefaultDateAndTime();
			}
			else
			{
				DeploymentName = deployment;
				Time = time;
				//############ WARNING!!! THE FOLLOWING LINE MUST BE CONSISTENT WITH TIMESLOT CONSTANT
				TimeSlot = ((time.Hour * 60) + time.Minute) / 30; //convert to half hour time slots
			}
        }

        public void SetDefaultDateAndTime()
        {
			Time = null;
            TimeSlot = 0;
        }

        public void ReadConfig(string iniFName)
        {
            Configuration cfg = new Configuration(iniFName);
			var basePath = Path.GetDirectoryName(iniFName);

            //general parameters
            Log.Verbosity = cfg.GetInt("VERBOSITY");

            //directory and file structure
            string dir = cfg.GetString("TEMPLATE_DIR");
            if (dir == null)
            {
                throw new Exception("###### FATAL ERROR! Could not read TEMPLATE directory from .ini file.");
            }
            this.TemplateParentDir = Path.Combine(basePath, dir);
            /*if (!FileTools.DirectoryExists(this.TemplateParentDir))
                throw new Exception("###### FATAL ERROR! Template directory <" + this.TemplateParentDir + "> does not exist.");*/

            dir = cfg.GetString("OP_DIR");
            if (dir == null)
                throw new Exception("###### FATAL ERROR! Could not read OUTPUT directory from .ini file.");
			this.OutputDir = Path.Combine(basePath, dir);
            Log.WriteLine("OPDIR=" + this.OutputDir);
            /*if (!FileTools.DirectoryExists(this.OutputDir))
                throw new Exception("###### FATAL ERROR! Output directory <" + this.OutputDir + "> does not exist.");*/

            dir = cfg.GetString("WAV_DIR");
            if (dir == null)
                throw new Exception("###### FATAL ERROR! Could not read WAV directory from .ini file.");
			//this.WavFileDir = Path.Combine(basePath, dir);
            /*if (!FileTools.DirectoryExists(this.WavFileDir))
                throw new Exception("###### FATAL ERROR! WAV directory <" + this.WavFileDir + "> does not exist.");*/

            //this.WavFileExt = cfg.GetString("WAV_FILEEXT");
            this.BmpFileExt = cfg.GetString("BMP_FILEEXT");

            //FRAMING PARAMETERS
            this.SubSample     = cfg.GetInt("SUBSAMPLE");
            this.WindowSize    = cfg.GetInt("WINDOW_SIZE");
            this.WindowOverlap = cfg.GetDouble("WINDOW_OVERLAP");

            //ENERGY AND SEGMENTATION PARAMETERS
            this.SegmentationThreshold_k1 = cfg.GetDouble("SEGMENTATION_THRESHOLD_K1"); //dB threshold for recognition of vocalisations
            this.SegmentationThreshold_k2 = cfg.GetDouble("SEGMENTATION_THRESHOLD_K2"); //dB threshold for recognition of vocalisations
            this.k1_k2Latency     = cfg.GetDouble("K1_K2_LATENCY");           //seconds delay between signal reaching k1 and k2 thresholds
            this.vocalDelay       = cfg.GetDouble("VOCAL_DELAY");             //seconds delay required to separate vocalisations 
            this.minPulseDuration = cfg.GetDouble("MIN_VOCAL_DURATION");      //minimum length of energy pulse - do not use this - 

            //FFT params
            this.WindowFncName = cfg.GetString("WINDOW_FUNCTION");
            this.WindowFnc = FFT.GetWindowFunction(this.WindowFncName);
            this.NPointSmoothFFT = cfg.GetInt("N_POINT_SMOOTH_FFT");

            // MFCC parameters
            this.SonogramType = Sonogram.SetSonogramType(cfg.GetString("SONOGRAM_TYPE"));
            this.DoMelScale = cfg.GetBoolean("DO_MELSCALE");
            this.freqBand_Min = cfg.GetInt("MIN_FREQ");    //min of the freq band to be analysed  
            this.freqBand_Max = cfg.GetInt("MAX_FREQ");    //max of the freq band to be analysed
            this.DoNoiseReduction = cfg.GetBoolean("NOISE_REDUCE");
            this.FilterbankCount = cfg.GetInt("FILTERBANK_COUNT");
            this.ccCount = cfg.GetInt("CC_COUNT"); //number of cepstral coefficients
            this.IncludeDelta = cfg.GetBoolean("INCLUDE_DELTA");
            this.IncludeDoubleDelta = cfg.GetBoolean("INCLUDE_DOUBLE_DELTA");
            this.DeltaT = cfg.GetInt("DELTA_T"); //frames between acoustic vectors

            //sonogram image parameters
            this.TrackType = Track.GetTrackType(cfg.GetString("TRACK_TYPE"));
            this.AddGrid = cfg.GetBoolean("ADDGRID");

            this.MinPercentile = cfg.GetDouble("MIN_PERCENTILE");
            this.MaxPercentile = cfg.GetDouble("MAX_PERCENTILE");
            this.BlurWindow = cfg.GetInt("BLUR_NEIGHBOURHOOD");
            this.BlurWindow_time = cfg.GetInt("BLUR_TIME_NEIGHBOURHOOD");
            this.BlurWindow_freq = cfg.GetInt("BLUR_FREQ_NEIGHBOURHOOD");
            //this.NormSonogram = cfg.GetBoolean("NORMALISE_SONOGRAM");
        }
    } //end class SonoConfig
}
