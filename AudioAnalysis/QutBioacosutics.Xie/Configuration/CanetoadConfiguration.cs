namespace QutBioacosutics.Xie.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
            this.AmplitudeThresholdCanetoad = configuration.AmplitudeThresholdCanetoad;   // Decibel---the minimum amplitude value,
            //RangeCanetoad = rangeCanetoad,
            //DistanceCanetoad = distanceCanetoad,
            // Track parameters
            this.BinToleranceCanetoad = configuration.BinToleranceCanetoad;
            this.FrameThresholdCanetoad = configuration.FrameThresholdCanetoad;
            this.TrackDurationThresholdCanetoad = configuration.TrackDurationThresholdCanetoad;
            this.TrackThresholdCanetoad = configuration.TrackThresholdCanetoad;
            this.MaximumTrackDurationCanetoad = configuration.MaximumTrackDurationCanetoad;
            this.MinimumTrackDurationCanetoad = configuration.MinimumTrackDurationCanetoad;
            this.BinDifferenceCanetoad = configuration.BinDifferenceCanetoad;
            // Band tracks parameters
            this.FrequencyLowCanetoad = configuration.FrequencyLowCanetoad;
            this.FrequencyHighCanetoad = configuration.FrequencyHighCanetoad;
            // DCT
            this.MinimumOscillationNumberCanetoad = configuration.MinimumOscillationNumberCanetoad;
            this.MaximumOscillationNumberCanetoad = configuration.MaximumOscillationNumberCanetoad;
            this.MinimumFrequencyCanetoad = configuration.MinimumFrequencyCanetoad;
            this.MaximumFrequencyCanetoad = configuration.MaximumFrequencyCanetoad;
            this.Dct_DurationCanetoad = configuration.Dct_DurationCanetoad;
            this.Dct_ThresholdCanetoad = configuration.Dct_ThresholdCanetoad;

            this.DoSlopeCanetoad = configuration.DoSlopeCanetoad;
        } // consturctor

    } //class

}
