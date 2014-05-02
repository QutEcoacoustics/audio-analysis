using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutBioacosutics.Xie.Configuration;
using AudioAnalysisTools.StandardSpectrograms;
using TowseyLibrary;

namespace QutBioacosutics.Xie.FrogIndices
{
    public static class CalculateIndexForLitoriaNasuta
    {

        public static double[,] GetPeakHitsNasuta(NasutaConfiguration nasutaConfig, SpectrogramStandard spectrogramLong)
        {
            var peakHitsNasuta = FindLocalPeaks.LocalPeaks(spectrogramLong, nasutaConfig.AmplitudeThresholdNasuta, nasutaConfig.RangeNasuta, nasutaConfig.DistanceNasuta,
                                        nasutaConfig.FrequencyLowNasuta, nasutaConfig.FrequencyHighNasuta);

            return peakHitsNasuta;
        }


        public static CombinedIndex GetFrogTracksFallax(NasutaConfiguration nasutaConfig, SpectrogramStandard spectrogramLong,
                                                         double[,] peakHitsNasuta)
        {
            var peakHitsNasutaRotated = MatrixTools.MatrixRotate90Anticlockwise(peakHitsNasuta);

            var trackHitsNasuta = ExtractTracks.GetTracks(spectrogramLong, peakHitsNasutaRotated, nasutaConfig.FrequencyLowNasuta, nasutaConfig.FrequencyHighNasuta,
                                        nasutaConfig.BinToleranceNasuta, nasutaConfig.FrameThresholdNasuta, nasutaConfig.TrackDurationThresholdNasuta,
                                        nasutaConfig.TrackThresholdNasuta, nasutaConfig.MaximumTrackDurationNasuta, nasutaConfig.MinimumTrackDurationNasuta,
                                        nasutaConfig.BinDifferenceNasuta, nasutaConfig.DoSlopeNasuta);

            
            var harmonicHitsNasuta = FindHarmonics.GetHarmonic(trackHitsNasuta.Item4, nasutaConfig.HarmonicComponentNasuta,
                                                                    nasutaConfig.HarmonicSensityNasuta, nasutaConfig.HarmonicDiffrangeNasuta);

            var combinedIndex = new CombinedIndex();
            combinedIndex.HarmonicHitsNasuta = harmonicHitsNasuta;
            combinedIndex.TrackHitsNasuta = trackHitsNasuta;

            return combinedIndex;
        
        }

        public class CombinedIndex
        {
            public double[,] HarmonicHitsNasuta { get; set; }
            public Tuple<double[], double[,], double[], double[,]> TrackHitsNasuta { get; set; }

        }



        public static double[,] GetOscillationRate(NasutaConfiguration nasutaConfig, SpectrogramStandard spectrogramShort)
        {

            var nasutaOscillationHits = FindOscillation.CalculateOscillationRate(spectrogramShort, nasutaConfig.MinimumFrequencyNasuta,
                                                                                    nasutaConfig.MaximumFrequencyNasuta, nasutaConfig.Dct_DurationNasuta,
                                                                                    nasutaConfig.Dct_ThresholdNasuta, nasutaConfig.MinimumOscillationNumberNasuta,
                                                                                    nasutaConfig.MaximumOscillationNumberNasuta);

            var nasutaOscillationResults = RemoveSparseHits.PruneHits(nasutaOscillationHits);

            return nasutaOscillationHits;

        }

    }
}
