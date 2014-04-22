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

        public double BinToreanceFallax { get; set; }
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

    }
}
