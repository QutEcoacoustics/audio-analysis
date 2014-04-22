using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TowseyLibrary;
using AudioAnalysisTools.DSP;


namespace AudioAnalysisTools
{


    public static class SpectrogramConstants
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


    } // SpectrogramConstants
}
