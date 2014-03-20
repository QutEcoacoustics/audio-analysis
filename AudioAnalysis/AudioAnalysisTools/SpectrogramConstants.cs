using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TowseyLib;


namespace AudioAnalysisTools
{
    public static class SpectrogramConstants
    {

        // CONST string for referring to different types of spectrogram - these should really be an enum                
        public const string KEY_AcousticComplexityIndex = "ACI";
        public const string KEY_Average = "AVG";
        public const string KEY_BackgroundNoise = "BGN";
        public const string KEY_Combined = "CMB";
        public const string KEY_Colour = "COL";
        public const string KEY_BinCover = "CVR";
        public const string KEY_TemporalEntropy = "TEN";
        public const string KEY_Variance = "VAR";

        // NORMALISING CONSTANTS FOR INDICES
        //public const double ACI_MIN = 0.0;
        //public const double ACI_MAX = 1.0;
        public const double ACI_MIN = 0.35;
        public const double ACI_MAX = 0.8;
        public const double AVG_MIN = 0.0;
        public const double AVG_MAX = 50.0;
        public const double BGN_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 15; //-20 adds more contrast into bgn image
        public const double BGN_MAX = -20.0;
        public const double CVR_MIN = 0.0;
        public const double CVR_MAX = 0.3;
        public const double TEN_MIN = 0.5;
        public const double TEN_MAX = 0.95;
        public const double VAR_MIN = 1000.0;
        public const double VAR_MAX = 30000.0;

        // False-colour map to acoustic indices
        //If you create a new colour assignment, you will need to code it in the class LDSpectrogramRGB, 
        //   method DrawFalseColourSpectrogramOfIndices(string colorSchemeID, int X_interval, int Y_interval, double[,] avgMatrix, double[,] cvrMatrix, double[,] aciMatrix, double[,] tenMatrix)
        public const string RGBMap_DEFAULT     = "ACI-TEN-CVR"; //R-G-B
        public const string RGBMap_ACI_TEN_AVG = "ACI-TEN-AVG"; //R-G-B
        public const string RGBMap_ACI_TEN_CVR = "ACI-TEN-CVR"; //R-G-B
        public const string RGBMap_ACI_TEN_BGN = "ACI-TEN-BGN"; //R-G-B
        public const string RGBMap_ACI_CVR_TEN = "ACI-CVR-TEN";
        public const string RGBMap_ACI_TEN_CVRAVG = "ACI-TEN-CVR_AVG";

        //these parameters manipulate the colour map and appearance of the false-colour LONG DURATION spectrogram
        public const double BACKGROUND_FILTER_COEFF = 0.75; //must be value <=1.0
        public const double COLOUR_GAIN = 1.5;

        // These parameters describe the time and frequency scales for drawing X and Y axes on LONG DURATION spectrograms
        public const int X_AXIS_SCALE = 60;    // assume one minute spectra and 60 spectra per hour
        public const int MINUTE_OFFSET = 0;    // assume recording starts at zero minute of day i.e. midnight
        public const int SAMPLE_RATE = 17640;  // default value - after resampling
        public const int FRAME_WIDTH = 512;    // default value - from which spectrogram was derived
        public const int HEIGHT_OF_TITLE_BAR = 20;





        public static Dictionary<string, Color> GetDifferenceColourChart()
        {
            Dictionary<string, Color> colourChart = new Dictionary<string, Color>();
            colourChart.Add("+99.9%", Color.FromArgb(255, 190, 20));
            colourChart.Add("+99.0%", Color.FromArgb(240, 50, 30)); //+99% conf
            colourChart.Add("+95.0%", Color.FromArgb(200, 30, 15)); //+95% conf
            colourChart.Add("+NotSig", Color.FromArgb(50, 5, 5));   //+ not significant
            colourChart.Add("NoValue", Color.Black);
            //no value
            colourChart.Add("-99.9%", Color.FromArgb(20, 255, 230));
            colourChart.Add("-99.0%", Color.FromArgb(30, 240, 50)); //+99% conf
            colourChart.Add("-95.0%", Color.FromArgb(15, 200, 30)); //+95% conf
            colourChart.Add("-NotSig", Color.FromArgb(10, 50, 20)); //+ not significant
            return colourChart;
        }



    }
}
