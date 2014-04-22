using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.StandardSpectrograms;
using QutBioacosutics.Xie.Configuration;
using TowseyLibrary;

namespace QutBioacosutics.Xie.FrogIndices
{
    class CalculateIndexForLitoriaCaerulea
    {

        public static double[,] GetPeakHits(CaeruleaConfiguration caeruleaConfig, SpectrogramStandard spectrogramLong)
        {
            var peakHitsCaerulea = FindLocalPeaks.LocalPeaks(spectrogramLong, caeruleaConfig.AmplitudeThresholdCaerulea, caeruleaConfig.RangeCaerulea, caeruleaConfig.DistanceCaerulea,
                                                  caeruleaConfig.FrequencyLowCaerulea, caeruleaConfig.FrequencyHighCaerulea);

            return peakHitsCaerulea;
        
        }


        public static System.Tuple<double[], double[,], double[], double[,]> GetFrogTracks(CaeruleaConfiguration caeruleaConfig, SpectrogramStandard spectrogramLong,
                                                                                                   double[,] peakHitsCaerulea)
 
        {
            var peakHitsCaeruleaRotated = MatrixTools.MatrixRotate90Anticlockwise(peakHitsCaerulea);

            var trackHitsCaerulea = ExtractTracks.GetTracks(spectrogramLong, peakHitsCaeruleaRotated, caeruleaConfig.FrequencyLowCaerulea, caeruleaConfig.FrequencyHighCaerulea,
                                                            caeruleaConfig.BinToreanceCaerulea, caeruleaConfig.FrameThresholdCaerulea, caeruleaConfig.TrackDurationThresholdCaerulea,
                                                            caeruleaConfig.TrackThresholdCaerulea, caeruleaConfig.MaximumTrackDurationCaerulea, caeruleaConfig.MinimumTrackDurationCaerulea,
                                                            caeruleaConfig.BinDifferencCaerulea, caeruleaConfig.DoSlopeCaerulea);

            return trackHitsCaerulea;
        
        }
        // Find the peaks based on tracks (# should be 2 or 3)

        public static double[,] GetOscillationRate(CaeruleaConfiguration caeruleaConfig, SpectrogramStandard spectrogramShort)
        {

            var caeruleaOscillationHits = FindOscillation.CalculateOscillationRate(spectrogramShort, caeruleaConfig.MinimumFrequencyCaerulea, caeruleaConfig.MaximumFrequencyCaerulea,
                                                                           caeruleaConfig.Dct_DurationCaerulea, caeruleaConfig.Dct_ThresholdCaerulea,
                                                                           caeruleaConfig.MinimumOscillationNumberCaerulea, caeruleaConfig.MaximumOscillationNumberCaerulea);
            return caeruleaOscillationHits;
        
        }

    }
}
