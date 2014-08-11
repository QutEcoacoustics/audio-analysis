using AudioAnalysisTools.StandardSpectrograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AudioAnalysisTools
{
    class RidgeDetectionh
    {
        public static double ridgeDetectionmMagnitudeThreshold = 0.2;
        public static int ridgeMatrixLength = 5;
        public static int filterRidgeMatrixLength = 5;
        public static int minimumNumberInRidgeInMatrix = 6;

        //var ridgeConfig = new RidgeDetectionConfiguration
        //{
        //    RidgeDetectionmMagnitudeThreshold = ridgeDetectionmMagnitudeThreshold,
        //    RidgeMatrixLength = ridgeMatrixLength,
        //    FilterRidgeMatrixLength = filterRidgeMatrixLength,
        //    MinimumNumberInRidgeInMatrix = minimumNumberInRidgeInMatrix
        //};




        //public static List<PointOfInterest> PostRidgeDetection(SpectrogramStandard spectrogram, RidgeDetectionConfiguration ridgeConfig)
        //{
        //    var instance = new POISelection(new List<PointOfInterest>());
        //    instance.FourDirectionsRidgeDetection(spectrogram, ridgeConfig);
        //    return instance.poiList;
        //}


    
    
    }

}
