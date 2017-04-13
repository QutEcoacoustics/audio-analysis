namespace QutBioacosutics.Xie.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class GracillentaConfiguration
    {
        public double AmplitudeThresholdGracillenta { get; set; }
        //public int RangeGracillenta { get; set; }
        //public int DistanceGracillenta { get; set; }

        public double BinToleranceGracillenta { get; set; }
        public int FrameThresholdGracillenta { get; set; }
        public int TrackDurationThresholdGracillenta { get; set; }
        public double TrackThresholdGracillenta { get; set; }
        public int MaximumTrackDurationGracillenta { get; set; }
        public int MinimumTrackDurationGracillenta { get; set; }
        public double BinDifferenceGracillenta { get; set; }

        public int FrequencyLowGracillenta { get; set; }
        public int FrequencyHighGracillenta { get; set; }

        //public int MinimumOscillationNumberGracillenta { get; set; }
        //public int MaximumOscillationNumberGracillenta { get; set; }
        //public int MinimumFrequencyGracillenta { get; set; }
        //public int MaximumFrequencyGracillenta { get; set; }
        //public double Dct_DurationGracillenta { get; set; }
        //public double Dct_ThresholdGracillenta { get; set; }

        public bool DoSlopeGracillenta { get; set; }

        public GracillentaConfiguration(dynamic configuration)
        {
            // Peak parameters
            this.AmplitudeThresholdGracillenta = configuration.AmplitudeThresholdGracillenta;
            //RangeGracillenta = rangeGracillenta,
            //DistanceGracillenta = distanceGracillenta,
            // Track parameters
            this.BinToleranceGracillenta = configuration.BinToleranceGracillenta;
            this.FrameThresholdGracillenta = configuration.FrameThresholdGracillenta;
            this.TrackDurationThresholdGracillenta = configuration.TrackDurationThresholdGracillenta;
            this.TrackThresholdGracillenta = configuration.TrackThresholdGracillenta;
            this.MaximumTrackDurationGracillenta = configuration.MaximumTrackDurationGracillenta;
            this.MinimumTrackDurationGracillenta = configuration.MinimumTrackDurationGracillenta;
            this.BinDifferenceGracillenta = configuration.BinDifferenceGracillenta;
            // Band tracks parameters
            this.FrequencyLowGracillenta = configuration.FrequencyLowGracillenta;
            this.FrequencyHighGracillenta = configuration.FrequencyHighGracillenta;
            // DCT
            //MinimumOscillationNumberGracillenta = minimumOscillationNumberGracillenta,
            //MaximumOscillationNumberGracillenta = maximumOscillationNumberGracillenta,
            //MinimumFrequencyGracillenta = minimumFrequencyGracillenta,
            //MaximumFrequencyGracillenta = maximumFrequencyGracillenta,
            //Dct_DurationGracillenta = dct_DurationGracillenta,
            //Dct_ThresholdGracillenta = dct_ThresholdGracillenta,

            this.DoSlopeGracillenta = configuration.DoSlopeGracillenta;
        }

    } //Class
}
