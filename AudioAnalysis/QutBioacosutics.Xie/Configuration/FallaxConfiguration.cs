using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie.Configuration
{
    public class FallaxConfiguration
    {
        public double AmplitudeThresholdFallax { get; set; }
        //public int RangeFallax { get; set; }
        //public int DistanceFallax { get; set; }

        public double BinToleranceFallax { get; set; }
        public int FrameThresholdFallax { get; set; }
        public int TrackDurationThresholdFallax { get; set; }
        public double TrackThresholdFallax { get; set; }
        public int MaximumTrackDurationFallax { get; set; }
        public int MinimumTrackDurationFallax { get; set; }
        public double BinDifferencFallax { get; set; }

        public int FrequencyLowFallax { get; set; }
        public int FrequencyHighFallax { get; set; }

        public int MinimumOscillationNumberFallax { get; set; }
        public int MaximumOscillationNumberFallax { get; set; }
        public int MinimumFrequencyFallax { get; set; }
        public int MaximumFrequencyFallax { get; set; }
        public double Dct_DurationFallax { get; set; }
        public double Dct_ThresholdFallax { get; set; }

        public bool DoSlopeFallax { get; set; }


        public FallaxConfiguration(dynamic configuration)
        {
            // Peak parameters
            AmplitudeThresholdFallax = configuration.AmplitudeThresholdFallax;   // Decibel---the minimum amplitude value
            //int rangeFallax = configuration.RangeFallax;                                // Frame---the distance in either side for selecting peaks
            //int distanceFallax = configuration.DistanceFallax;                          // Frame---remove near peaks
            // Track parameters
            BinToleranceFallax = configuration.BinToleranceFallax;                 // Bin---the fluctuation of the dominant frequency bin 
            FrameThresholdFallax = configuration.FrameThresholdFallax;              // Frame---frame numbers of the silence    
            TrackDurationThresholdFallax = configuration.TrackDurationThresholdFallax;
            TrackThresholdFallax = configuration.TrackThresholdFallax;           // Used for calculating the percent of peaks in one track    
            MaximumTrackDurationFallax = configuration.MaximumTrackDurationFallax;  // Minimum duration of tracks
            MinimumTrackDurationFallax = configuration.MinimumTrackDurationFallax;  // Maximum duration of tracks   
            BinDifferencFallax = configuration.BinDifferenceFallax;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            FrequencyLowFallax = configuration.FrequencyLowFallax;
            FrequencyHighFallax = configuration.FrequencyHighFallax;
            // DCT
            //int minimumOscillationNumberFallax = configuration.minimumOscillationNumberFallax;
            //int maximumOscillationNumberFallax = configuration.maximumOscillationNumberFallax;
            //int minimumFrequencyFallax = configuration.MinimumFrequencyFallax;
            //int maximumFrequencyFallax = configuration.MaximumFrequencyFallax;
            //double dct_DurationFallax = configuration.Dct_DurationFallax;
            //double dct_ThresholdFallax = configuration.Dct_ThresholdFallax;

            DoSlopeFallax = configuration.DoSlopeFallax;

        }
    }
}
