using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.StandardSpectrograms;
using QutBioacosutics.Xie.Configuration;

namespace QutBioacosutics.Xie.FrogIndices
{
    public static class CalculateIndexForLitoriaFallax
    {

        public static double[,] GetPeakHitsFallax(FallaxConfiguration fallaxConfig, SpectrogramStandard spectrogramLong)
        {
            var peakHitsFallax = FindLocalPeaks.Max(spectrogramLong, fallaxConfig.AmplitudeThresholdFallax, fallaxConfig.FrequencyLowFallax, fallaxConfig.FrequencyHighFallax);
            return peakHitsFallax;
        }

        public static System.Tuple<double[], double[,], double[], double[,]> GetFrogTracksFallax(FallaxConfiguration fallaxConfig, SpectrogramStandard spectrogramLong, double[,] peakHitsFallax)
        {
            var trackHitsFallax = ExtractTracks.GetTracks(spectrogramLong, peakHitsFallax, fallaxConfig.FrequencyLowFallax, fallaxConfig.FrequencyHighFallax,
                                                          fallaxConfig.BinToleranceFallax, fallaxConfig.FrameThresholdFallax, fallaxConfig.TrackDurationThresholdFallax,
                                                          fallaxConfig.TrackThresholdFallax, fallaxConfig.MaximumTrackDurationFallax, fallaxConfig.MinimumTrackDurationFallax,
                                                          fallaxConfig.BinDifferencFallax, fallaxConfig.DoSlopeFallax);
            return trackHitsFallax;
        }

        // Config for Litoria Fallax

    } // class CalculateIndexForLitoriaFallax
}
