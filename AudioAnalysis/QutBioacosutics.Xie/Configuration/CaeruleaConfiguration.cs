using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie.Configuration
{
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
            AmplitudeThresholdCaerulea = configuration.AmplitudeThresholdCaerulea;   // Decibel---the minimum amplitude value
            RangeCaerulea = configuration.RangeCaerulea;                                // Frame---the distance in either side for selecting peaks
            DistanceCaerulea = configuration.DistanceCaerulea;                          // Frame---remove near peaks
            // Track parameters
            BinToleranceCaerulea = configuration.BinToleranceCaerulea;                 // Bin---the fluctuation of the dominant frequency bin 
            FrameThresholdCaerulea = configuration.FrameThresholdCaerulea;              // Frame---frame numbers of the silence    
            TrackDurationThresholdCaerulea = configuration.TrackDurationThresholdCaerulea;
            TrackThresholdCaerulea = configuration.TrackThresholdCaerulea;           // Used for calculating the percent of peaks in one track    
            MaximumTrackDurationCaerulea = configuration.MaximumTrackDurationCaerulea;  // Minimum duration of tracks
            MinimumTrackDurationCaerulea = configuration.MinimumTrackDurationCaerulea;  // Maximum duration of tracks   
            BinDifferenceCaerulea = configuration.BinDifferenceCaerulea;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            FrequencyLowCaerulea = configuration.FrequencyLowCaerulea;
            FrequencyHighCaerulea = configuration.FrequencyHighCaerulea;
            // DCT
            MinimumOscillationNumberCaerulea = configuration.minimumOscillationNumberCaerulea;
            MaximumOscillationNumberCaerulea = configuration.maximumOscillationNumberCaerulea;
            MinimumFrequencyCaerulea = configuration.MinimumFrequencyCaerulea;
            MaximumFrequencyCaerulea = configuration.MaximumFrequencyCaerulea;
            Dct_DurationCaerulea = configuration.Dct_DurationCaerulea;
            Dct_ThresholdCaerulea = configuration.Dct_ThresholdCaerulea;

            DoSlopeCaerulea = configuration.DoSlopeCaerulea;
        } // consturctor

    } // Class
}
