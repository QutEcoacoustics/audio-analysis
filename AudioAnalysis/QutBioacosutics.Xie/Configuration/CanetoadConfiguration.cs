using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie.Configuration
{
    public class CanetoadConfiguration
    {

        public double AmplitudeThresholdCanetoad { get; set; }
        public int RangeCanetoad { get; set; }
        public int DistanceCanetoad { get; set; }

        public double BinToreanceCanetoad { get; set; }
        public int FrameThresholdCanetoad { get; set; }
        public int TrackDurationThresholdCanetoad { get; set; }
        public double TrackThresholdCanetoad { get; set; }
        public int MaximumTrackDurationCanetoad { get; set; }
        public int MinimumTrackDurationCanetoad { get; set; }
        public double BinDifferencCanetoade { get; set; }

        public int FrequencyLowCanetoad { get; set; }
        public int FrequencyHighCanetoad { get; set; }

        public int MinimumOscillationNumberCanetoad { get; set; }
        public int MaximumOscillationNumberCanetoad { get; set; }
        public int MinimumFrequencyCanetoad { get; set; }
        public int MaximumFrequencyCanetoad { get; set; }
        public double Dct_DurationCanetoad { get; set; }
        public double Dct_ThresholdCanetoad { get; set; }

    }
}
