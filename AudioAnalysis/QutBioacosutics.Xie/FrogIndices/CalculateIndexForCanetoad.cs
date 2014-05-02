using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.StandardSpectrograms;
using QutBioacosutics.Xie.Configuration;

namespace QutBioacosutics.Xie.FrogIndices
{
    public static class CalculateIndexForCanetoad
    {
        public static double[,] GetPeakHits(CanetoadConfiguration canetoadConfig, SpectrogramStandard spectrogramLong)
        {

            var peakHitsCanetoad = FindLocalPeaks.Max(spectrogramLong, canetoadConfig.AmplitudeThresholdCanetoad, canetoadConfig.FrequencyLowCanetoad,
                                                canetoadConfig.FrequencyHighCanetoad);

            return peakHitsCanetoad;
        }

        public static System.Tuple<double[], double[,], double[], double[,]> GetFrogTracks(CanetoadConfiguration canetoadConfig, SpectrogramStandard spectrogramLong, 
                                                                                            double[,] peakHitsCanetoad)
        {
            var trackHitsCanetoad = ExtractTracks.GetTracks(spectrogramLong, peakHitsCanetoad, canetoadConfig.FrequencyLowCanetoad, canetoadConfig.FrequencyHighCanetoad,
                                                            canetoadConfig.BinToleranceCanetoad, canetoadConfig.FrameThresholdCanetoad, canetoadConfig.TrackDurationThresholdCanetoad,
                                                            canetoadConfig.TrackThresholdCanetoad, canetoadConfig.MaximumTrackDurationCanetoad, canetoadConfig.MinimumTrackDurationCanetoad,
                                                            canetoadConfig.BinDifferenceCanetoad, canetoadConfig.DoSlopeCanetoad);
            return trackHitsCanetoad;
        
        }

        public static double[,] GetOscillationRate(  CanetoadConfiguration canetoadConfig,SpectrogramStandard spectrogramShort)
        { 
        
            var canetoadOscillationHits = FindOscillation.CalculateOscillationRate( spectrogramShort, canetoadConfig.MinimumFrequencyCanetoad, 
                                                                                    canetoadConfig.MaximumFrequencyCanetoad,canetoadConfig.Dct_DurationCanetoad, 
                                                                                    canetoadConfig.Dct_ThresholdCanetoad,canetoadConfig.MinimumOscillationNumberCanetoad, 
                                                                                    canetoadConfig.MaximumOscillationNumberCanetoad);

            var canetoadOscillationResults = RemoveSparseHits.PruneHits(canetoadOscillationHits);

            return canetoadOscillationHits;   
                
        }

    }

    //====================
    // below here put config class for canetoad.


}
