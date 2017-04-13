﻿namespace QutBioacosutics.Xie.FrogIndices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools.StandardSpectrograms;
    using Configuration;
    using TowseyLibrary;

    class CalculateIndexForLitoriaLatopalmata
    {
        public static double[,] GetPeakHits(LatopalmataConfiguration latopalmataConfig, SpectrogramStandard spectrogramLong)
        {
            var peakHitsLatopalmata = FindLocalPeaks.LocalPeaks(spectrogramLong, latopalmataConfig.AmplitudeThresholdLatopalmata, latopalmataConfig.RangeLatopalmata,
                                                latopalmataConfig.DistanceLatopalmata, latopalmataConfig.FrequencyLowLatopalmata, latopalmataConfig.FrequencyHighLatopalmata);

            return peakHitsLatopalmata;

        }


        public static CombinedIndex GetFrogTracks(LatopalmataConfiguration latopalmataConfig, SpectrogramStandard spectrogramLong,
                                                    double[,] peakHitsLatopalmata)
        {
            var peakHitsLatopalmataRotated = MatrixTools.MatrixRotate90Anticlockwise(peakHitsLatopalmata);

            var trackHitsLatopalmata = ExtractTracks.GetTracks(spectrogramLong, peakHitsLatopalmataRotated, latopalmataConfig.FrequencyLowLatopalmata,
                                                               latopalmataConfig.FrequencyHighLatopalmata, latopalmataConfig.BinToleranceLatopalmata,
                                                               latopalmataConfig.FrameThresholdLatopalmata, latopalmataConfig.TrackDurationThresholdLatopalmata,
                                                               latopalmataConfig.TrackThresholdLatopalmata, latopalmataConfig.MaximumTrackDurationLatopalmata,
                                                               latopalmataConfig.MinimumTrackDurationLatopalmata, latopalmataConfig.BinDifferenceLatopalmata,
                                                               latopalmataConfig.DoSlopeLatopalmata);

            // Contain harmonic structure
            var harmonicHitsLatopalmata = FindHarmonics.GetHarmonic(trackHitsLatopalmata.Item4, latopalmataConfig.HarmonicComponentLatopalmata,
                                                                    latopalmataConfig.HarmonicSensityLatopalmata, latopalmataConfig.HarmonicDiffrangeLatopalmata);

            var combinedIndex = new CombinedIndex();
            combinedIndex.HarmonicHitsLatopalmata = harmonicHitsLatopalmata;
            combinedIndex.TrackHitsLatopalmata = trackHitsLatopalmata;

            return combinedIndex;
        }

        public class CombinedIndex
        {
            public double[,] HarmonicHitsLatopalmata{ get; set; }
            public Tuple<double[], double[,], double[], double[,]> TrackHitsLatopalmata { get; set; }

        }


    }
}
