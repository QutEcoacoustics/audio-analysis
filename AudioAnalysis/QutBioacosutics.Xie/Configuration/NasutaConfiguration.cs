namespace QutBioacosutics.Xie.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class NasutaConfiguration
    {
        public double AmplitudeThresholdNasuta { get; set; }
        public int RangeNasuta { get; set; }
        public int DistanceNasuta { get; set; }

        public double BinToleranceNasuta { get; set; }
        public int FrameThresholdNasuta { get; set; }
        public int TrackDurationThresholdNasuta { get; set; }
        public double TrackThresholdNasuta { get; set; }
        public int MaximumTrackDurationNasuta { get; set; }
        public int MinimumTrackDurationNasuta { get; set; }
        public double BinDifferenceNasuta { get; set; }

        public int FrequencyLowNasuta { get; set; }
        public int FrequencyHighNasuta { get; set; }

        public int MinimumOscillationNumberNasuta { get; set; }
        public int MaximumOscillationNumberNasuta { get; set; }
        public int MinimumFrequencyNasuta { get; set; }
        public int MaximumFrequencyNasuta { get; set; }
        public double Dct_DurationNasuta { get; set; }
        public double Dct_ThresholdNasuta { get; set; }

        public bool DoSlopeNasuta { get; set; }

        public int HarmonicComponentNasuta { get; set; }
        public int HarmonicSensityNasuta { get; set; }
        public int HarmonicDiffrangeNasuta { get; set; }


        public NasutaConfiguration(dynamic configuration)
        {
            // Peak parameters
            this.AmplitudeThresholdNasuta = configuration.AmplitudeThresholdNasuta;   // Decibel---the minimum amplitude value
            this.RangeNasuta = configuration.RangeNasuta;                                // Frame---the distance in either side for selecting peaks
            this.DistanceNasuta = configuration.DistanceNasuta;                          // Frame---remove near peaks
            // Track parameters
            this.BinToleranceNasuta = configuration.BinToleranceNasuta;                 // Bin---the fluctuation of the dominant frequency bin
            this.FrameThresholdNasuta = configuration.FrameThresholdNasuta;              // Frame---frame numbers of the silence
            this.TrackDurationThresholdNasuta = configuration.TrackDurationThresholdNasuta;
            this.TrackThresholdNasuta = configuration.TrackThresholdNasuta;           // Used for calculating the percent of peaks in one track
            this.MaximumTrackDurationNasuta = configuration.MaximumTrackDurationNasuta;  // Minimum duration of tracks
            this.MinimumTrackDurationNasuta = configuration.MinimumTrackDurationNasuta;  // Maximum duration of tracks
            this.BinDifferenceNasuta = configuration.BinDifferenceNasuta;             // Difference between the highest and lowest bins
            // Band tracks parameters
            this.FrequencyLowNasuta = configuration.FrequencyLowNasuta;
            this.FrequencyHighNasuta = configuration.FrequencyHighNasuta;
            // DCT
            this.MinimumOscillationNumberNasuta = configuration.minimumOscillationNumberNasuta;
            this.MaximumOscillationNumberNasuta = configuration.maximumOscillationNumberNasuta;
            this.MinimumFrequencyNasuta = configuration.MinimumFrequencyNasuta;
            this.MaximumFrequencyNasuta = configuration.MaximumFrequencyNasuta;
            this.Dct_DurationNasuta = configuration.Dct_DurationNasuta;
            this.Dct_ThresholdNasuta = configuration.Dct_ThresholdNasuta;

            this.DoSlopeNasuta = configuration.DoSlopeNasuta;

            this.HarmonicComponentNasuta = configuration.HarmonicComponentNasuta;
            this.HarmonicSensityNasuta = configuration.HarmonicSensityNasuta;
            this.HarmonicDiffrangeNasuta = configuration.HarmonicDiffrangeNasuta;
        } // consturctor

    } // class

}
