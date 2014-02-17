using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // False-colour assignment IDs
        //If you create a new colour assignment, you will need to code it in the class Colourspectrogram, 
        //            method DrawFalseColourSpectrogramOfIndices(string colorSchemeID, int X_interval, int Y_interval, double[,] avgMatrix, double[,] cvrMatrix, double[,] aciMatrix, double[,] tenMatrix)
        public const string RGBMap_DEFAULT     = "ACI-TEN-CVR"; //R-G-B
        public const string RGBMap_ACI_TEN_AVG = "ACI-TEN-AVG"; //R-G-B
        public const string RGBMap_ACI_TEN_CVR = "ACI-TEN-CVR"; //R-G-B
        public const string RGBMap_ACI_TEN_BGN = "ACI-TEN-BGN"; //R-G-B
        public const string RGBMap_ACI_CVR_TEN = "ACI-CVR-TEN";
        public const string RGBMap_ACI_TEN_CVRAVG = "ACI-TEN-CVR_AVG";




    }
}
