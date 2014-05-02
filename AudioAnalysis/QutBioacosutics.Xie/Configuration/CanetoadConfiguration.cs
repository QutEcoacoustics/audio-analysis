using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie.Configuration
{
    public class CanetoadConfiguration
    {
        public double AmplitudeThresholdCanetoad { get; set; }
        //public int RangeCanetoad { get; set; }
        //public int DistanceCanetoad { get; set; }

        public double BinToleranceCanetoad { get; set; }
        public int FrameThresholdCanetoad { get; set; }
        public int TrackDurationThresholdCanetoad { get; set; }
        public double TrackThresholdCanetoad { get; set; }
        public int MaximumTrackDurationCanetoad { get; set; }
        public int MinimumTrackDurationCanetoad { get; set; }
        public double BinDifferenceCanetoad { get; set; }

        public int FrequencyLowCanetoad { get; set; }
        public int FrequencyHighCanetoad { get; set; }

        public int MinimumOscillationNumberCanetoad { get; set; }
        public int MaximumOscillationNumberCanetoad { get; set; }
        public int MinimumFrequencyCanetoad { get; set; }
        public int MaximumFrequencyCanetoad { get; set; }
        public double Dct_DurationCanetoad { get; set; }
        public double Dct_ThresholdCanetoad { get; set; }

        public bool DoSlopeCanetoad { get; set; }


        public CanetoadConfiguration(dynamic configuration)
        {
            //***************************Canetoad*****************************//  
            // Peak parameters
            AmplitudeThresholdCanetoad = configuration.AmplitudeThresholdCanetoad;   // Decibel---the minimum amplitude value,
            //RangeCanetoad = rangeCanetoad,
            //DistanceCanetoad = distanceCanetoad,
            // Track parameters
            BinToleranceCanetoad = configuration.BinToleranceCanetoad;
            FrameThresholdCanetoad = configuration.FrameThresholdCanetoad;
            TrackDurationThresholdCanetoad = configuration.TrackDurationThresholdCanetoad;
            TrackThresholdCanetoad = configuration.TrackThresholdCanetoad;
            MaximumTrackDurationCanetoad = configuration.MaximumTrackDurationCanetoad;
            MinimumTrackDurationCanetoad = configuration.MinimumTrackDurationCanetoad;
            BinDifferenceCanetoad = configuration.BinDifferenceCanetoad;
            // Band tracks parameters
            FrequencyLowCanetoad = configuration.FrequencyLowCanetoad;
            FrequencyHighCanetoad = configuration.FrequencyHighCanetoad;
            // DCT
            MinimumOscillationNumberCanetoad = configuration.MinimumOscillationNumberCanetoad;
            MaximumOscillationNumberCanetoad = configuration.MaximumOscillationNumberCanetoad;
            MinimumFrequencyCanetoad = configuration.MinimumFrequencyCanetoad;
            MaximumFrequencyCanetoad = configuration.MaximumFrequencyCanetoad;
            Dct_DurationCanetoad = configuration.Dct_DurationCanetoad;
            Dct_ThresholdCanetoad = configuration.Dct_ThresholdCanetoad;

            DoSlopeCanetoad = configuration.DoSlopeCanetoad;
        } // consturctor

    } //class

}
