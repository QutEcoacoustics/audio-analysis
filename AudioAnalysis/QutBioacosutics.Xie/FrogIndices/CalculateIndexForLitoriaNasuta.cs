﻿using System;
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


        public static System.Tuple<double[], double[,], double[], double[,]> GetFrogTracksFallax(NasutaConfiguration nasutaConfig, SpectrogramStandard spectrogramLong,
                                                                                                    double[,] peakHitsNasuta)
        {
            var peakHitsNasutaRotated = MatrixTools.MatrixRotate90Anticlockwise(peakHitsNasuta);

            var trackHitsNasuta = ExtractTracks.GetTracks(spectrogramLong, peakHitsNasutaRotated, nasutaConfig.FrequencyLowNasuta, nasutaConfig.FrequencyHighNasuta,
                                        nasutaConfig.BinToreanceNasuta, nasutaConfig.FrameThresholdNasuta, nasutaConfig.TrackDurationThresholdNasuta,
                                        nasutaConfig.TrackThresholdNasuta, nasutaConfig.MaximumTrackDurationNasuta, nasutaConfig.MinimumTrackDurationNasuta,
                                        nasutaConfig.BinDifferencNasuta, nasutaConfig.DoSlopeNasuta);

            return trackHitsNasuta;
        
        }



        //public static double(NasutaConfiguration nasutaConfig, System.Tuple trackHitsNasuta)
        //{
        //            var harmonicHitsNasuta = FindHarmonics.GetHarmonic(trackHitsNasuta.Item4, nasutaConfig.HarmonicComponentNasuta, nasutaConfig.HarmonicSensityNasuta,
        //                                                   nasutaConfig.HarmonicDiffrangeNasuta);
        //    return harmonicHitsNasuta;
        //}


        //public static 
        
        //var NasutaOscillationHits = FindOscillation.CalculateOscillationRate(spectrogramShort, nasutaConfig.MinimumFrequencyNasuta, nasutaConfig.MaximumFrequencyNasuta,
        //                                                                       nasutaConfig.Dct_DurationNasuta, nasutaConfig.Dct_ThresholdNasuta,
        //                                                                       nasutaConfig.MinimumOscillationNumberNasuta, nasutaConfig.MaximumOscillationNumberNasuta);

        //var nasutaOscillationResults = RemoveSparseHits.PruneHits(NasutaOscillationHits);

    }
}
