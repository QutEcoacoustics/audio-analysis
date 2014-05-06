using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.StandardSpectrograms;
using QutBioacosutics.Xie.Configuration;

namespace QutBioacosutics.Xie.FrogIndices
{
    class CalculateIndexForLitoriaGracillenta
    {

        public static double[,] GetPeakHitsGracillenta(GracillentaConfiguration gracillentaConfig, SpectrogramStandard spectrogramLong)
        {
            var peakHitsGracillenta = FindLocalPeaks.Max(spectrogramLong, gracillentaConfig.AmplitudeThresholdGracillenta, gracillentaConfig.FrequencyLowGracillenta,
                                        gracillentaConfig.FrequencyHighGracillenta);

            return peakHitsGracillenta;
        }

        public static System.Tuple<double[], double[,], double[], double[,]> GetFrogTracksGracillenta(GracillentaConfiguration gracillentaConfig, SpectrogramStandard spectrogramLong,
                                                                                                        double[,] peakHitsGracillenta)
        {
            var trackHitsGracillenta = ExtractTracks.GetTracks(spectrogramLong, peakHitsGracillenta, gracillentaConfig.FrequencyLowGracillenta, gracillentaConfig.FrequencyHighGracillenta,
                                        gracillentaConfig.BinToleranceGracillenta, gracillentaConfig.FrameThresholdGracillenta, gracillentaConfig.TrackDurationThresholdGracillenta,
                                        gracillentaConfig.TrackThresholdGracillenta, gracillentaConfig.MaximumTrackDurationGracillenta, gracillentaConfig.MinimumTrackDurationGracillenta,
                                        gracillentaConfig.BinDifferenceGracillenta, gracillentaConfig.DoSlopeGracillenta);


            return trackHitsGracillenta;
        }

    }
}
