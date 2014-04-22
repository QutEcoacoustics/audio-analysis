using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TowseyLibrary;
using AudioAnalysisTools.DSP;


namespace AudioAnalysisTools
{


    public static class SpectrogramConstantsJie
    {
        // False-colour map to acoustic indices
        public const string RGBMap_DEFAULT     = "ACI-ENT-CVR"; //R-G-B

        public const string RGBMap_BGN_AVG_CVR = "BGN-AVG-CVR"; //R-G-B
        public const string RGBMap_BGN_AVG_EVN = "BGN-AVG-EVN"; //R-G-B
        public const string RGBMap_BGN_AVG_SPT = "BGN-AVG-SPT"; //R-G-B
        public const string RGBMap_BGN_AVG_CLS = "BGN-AVG-CLS"; //R-G-B

        public const string RGBMap_ACI_ENT_AVG = "ACI-ENT-AVG"; //R-G-B
        public const string RGBMap_ACI_ENT_CVR = "ACI-ENT-CVR"; //R-G-B
        public const string RGBMap_ACI_ENT_CLS = "ACI-ENT-CLS"; //R-G-B
        public const string RGBMap_ACI_ENT_EVN = "ACI-ENT-EVN"; //R-G-B
        public const string RGBMap_ACI_ENT_SPT = "ACI-ENT-SPT"; //R-G-B
        public const string RGBMap_ACI_CVR_ENT = "ACI-CVR-ENT";


        //these parameters manipulate the colour map and appearance of the false-colour LONG DURATION spectrogram
        public const double BACKGROUND_FILTER_COEFF = 0.75; //must be value <=1.0
        public const double COLOUR_GAIN = 2.0;

        // These parameters describe the time and frequency scales for drawing X and Y axes on LONG DURATION spectrograms
        public const int X_AXIS_SCALE = 60;    // assume one minute spectra and 60 spectra per hour
        public const int MINUTE_OFFSET = 0;    // assume recording starts at zero minute of day i.e. midnight
        public const int SAMPLE_RATE = 17640;  // default value - after resampling
        public const int FRAME_WIDTH = 512;    // default value - from which spectrogram was derived
        public const int HEIGHT_OF_TITLE_BAR = 20;





        //############################################################################################################################################################
        // THE METHODS BELOW SHOULD BE REMOVED IN CONJUNCTION WITH CHANGING JEI's CODE
        // NOW IN INITIALISE INDEX PROPERTIES class.
        //############################################################################################################################################################

        // CONST string for referring to different types of spectrogram - these should really be an enum                
        public const string KEY_AcousticComplexityIndex = "ACI";
        public const string KEY_Average = "AVG";
        public const string KEY_BackgroundNoise = "BGN";
        public const string KEY_Combined = "CMB";
        public const string KEY_Cluster  = "CLS";
        public const string KEY_Colour = "COL";
        public const string KEY_BinCover = "CVR";
        public const string KEY_SpPeakTracks = "SPT";
        public const string KEY_TemporalEntropy = "TEN";
        public const string KEY_Variance = "VAR";

        // THESE SHOULD BE REMOVED IN CONJUNCTION WITH CHANGING JEI's CODE
        // NOW IN INITIALISE INDEX PROPERTIES class.
        // FOR FROG SPECTROGRAMS
        public const string KEY_Tracks = "TRC";
        public const string KEY_Oscillations = "OSC";
        public const string KEY_Harmonics = "HAR";



        // THESE SHOULD BE REMOVED IN CONJUNCTION WITH CHANGING JEI's CODE
        // NOW IN INITIALISE INDEX PROPERTIES class.
        // NORMALISING CONSTANTS FOR INDICES
        //public const double ACI_MIN = 0.0;
        //public const double ACI_MAX = 1.0;
        public const double ACI_MIN = 0.4;
        public const double ACI_MAX = 0.8;
        public const double AVG_MIN = 0.0;
        public const double AVG_MAX = 50.0;
        public const double BGN_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20; //-20 adds more contrast into bgn image
        public const double BGN_MAX = -20.0;
        public const double CLS_MIN = 0.0;
        public const double CLS_MAX = 30.0;
        public const double CVR_MIN = 0.0;
        public const double CVR_MAX = 0.3;
        public const double TEN_MIN = 0.4;
        public const double TEN_MAX = 0.95;
        public const double SDV_MIN = 0.0; // for the variance bounds
        public const double SDV_MAX = 100.0;
        public const double VAR_MIN = SDV_MIN * SDV_MIN;
        public const double VAR_MAX = SDV_MAX * SDV_MAX; // previously 30000.0

        // THESE SHOULD BE REMOVED IN CONJUNCTION WITH CHANGING JEI's CODE
        // NOW IN INITIALISE INDEX PROPERTIES class.
        // FOR FROG SPECTROGRAMS
        public const double TRC_MIN = 0.0;
        public const double TRC_MAX = 0.065;
        public const double OSC_MIN = 0.0;
        public const double OSC_MAX = 0.5;
        public const double HAR_MIN = 0.0;
        public const double HAR_MAX = 0.025;

        //public const string RGBMap_BGN_AVG_CVR = "BGN-AVG-CVR"; //R-G-B
        public const string RGBMap_TRC_OSC_HAR = "TRC-OSC-HAR";

        public const string ALL_KNOWN_KEYS = "ACI-AVG-BGN-CLS-CVR-SPT-TEN-VAR-TRC-OSC-HAR";

    } // SpectrogramConstants
}
