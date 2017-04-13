namespace QutBioacosutics.Xie.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class CaeruleaConfiguration
    {
        public double AmplitudeThresholdCaerulea { get; set; }
        public int RangeCaerulea { get; set; }
        public int DistanceCaerulea { get; set; }

        public double BinToleranceCaerulea { get; set; }
        public int FrameThresholdCaerulea { get; set; }
        public int TrackDurationThresholdCaerulea { get; set; }
        public double TrackThresholdCaerulea { get; set; }
        public int MaximumTrackDurationCaerulea { get; set; }
        public int MinimumTrackDurationCaerulea { get; set; }
        public double BinDifferenceCaerulea { get; set; }

        public int FrequencyLowCaerulea { get; set; }
        public int FrequencyHighCaerulea { get; set; }

        public int MinimumOscillationNumberCaerulea { get; set; }
        public int MaximumOscillationNumberCaerulea { get; set; }
        public int MinimumFrequencyCaerulea { get; set; }
        public int MaximumFrequencyCaerulea { get; set; }
        public double Dct_DurationCaerulea { get; set; }
        public double Dct_ThresholdCaerulea { get; set; }

        public bool DoSlopeCaerulea { get; set; }


        public CaeruleaConfiguration(dynamic configuration)
        {
            // Peak parameters
            this.AmplitudeThresholdCaerulea = configuration.AmplitudeThresholdCaerulea;   // Decibel---the minimum amplitude value
            this.RangeCaerulea = configuration.RangeCaerulea;                                // Frame---the distance in either side for selecting peaks
            this.DistanceCaerulea = configuration.DistanceCaerulea;                          // Frame---remove near peaks
            // Track parameters
            this.BinToleranceCaerulea = configuration.BinToleranceCaerulea;                 // Bin---the fluctuation of the dominant frequency bin
            this.FrameThresholdCaerulea = configuration.FrameThresholdCaerulea;              // Frame---frame numbers of the silence
            this.TrackDurationThresholdCaerulea = configuration.TrackDurationThresholdCaerulea;
            this.TrackThresholdCaerulea = configuration.TrackThresholdCaerulea;           // Used for calculating the percent of peaks in one track
            this.MaximumTrackDurationCaerulea = configuration.MaximumTrackDurationCaerulea;  // Minimum duration of tracks
            this.MinimumTrackDurationCaerulea = configuration.MinimumTrackDurationCaerulea;  // Maximum duration of tracks
            this.BinDifferenceCaerulea = configuration.BinDifferenceCaerulea;             // Difference between the highest and lowest bins
            // Band tracks parameters
            this.FrequencyLowCaerulea = configuration.FrequencyLowCaerulea;
            this.FrequencyHighCaerulea = configuration.FrequencyHighCaerulea;
            // DCT
            this.MinimumOscillationNumberCaerulea = configuration.minimumOscillationNumberCaerulea;
            this.MaximumOscillationNumberCaerulea = configuration.maximumOscillationNumberCaerulea;
            this.MinimumFrequencyCaerulea = configuration.MinimumFrequencyCaerulea;
            this.MaximumFrequencyCaerulea = configuration.MaximumFrequencyCaerulea;
            this.Dct_DurationCaerulea = configuration.Dct_DurationCaerulea;
            this.Dct_ThresholdCaerulea = configuration.Dct_ThresholdCaerulea;

            this.DoSlopeCaerulea = configuration.DoSlopeCaerulea;
        } // consturctor

    } // Class
}
