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



        // THESE SHOULD BE REMOVED IN CONJUNCTION WITH CHANGING JEI's CODE
        // NOW IN INITIALISE INDEX PROPERTIES class.
        // FOR FROG SPECTROGRAMS
        public const double TRK_MIN = 0.0;
        public const double TRK_MAX = 0.065;
        public const double OSC_MIN = 0.0;
        public const double OSC_MAX = 0.5;
        public const double ENG_MIN = 0.0;
        public const double ENG_MAX = 0.025;

        //public const string RGBMap_BGN_AVG_CVR = "BGN-AVG-CVR"; //R-G-B
        public const string RGBMap_TRK_OSC_ENG = "TRK-OSC-ENG";

        public const string ALL_KNOWN_KEYS = "ACI-AVG-BGN-CLS-CVR-SPT-TEN-VAR-TRK-OSC-ENG";

    } // SpectrogramConstants
}
