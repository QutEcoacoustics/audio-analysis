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

        public double BinToreanceCaerulea { get; set; }
        public int FrameThresholdCaerulea { get; set; }
        public int TrackDurationThresholdCaerulea { get; set; }
        public double TrackThresholdCaerulea { get; set; }
        public int MaximumTrackDurationCaerulea { get; set; }
        public int MinimumTrackDurationCaerulea { get; set; }
        public double BinDifferencCaerulea { get; set; }

        public int FrequencyLowCaerulea { get; set; }
        public int FrequencyHighCaerulea { get; set; }

        public int MinimumOscillationNumberCaerulea { get; set; }
        public int MaximumOscillationNumberCaerulea { get; set; }
        public int MinimumFrequencyCaerulea { get; set; }
        public int MaximumFrequencyCaerulea { get; set; }
        public double Dct_DurationCaerulea { get; set; }
        public double Dct_ThresholdCaerulea { get; set; }

        public bool DoSlopeCaerulea { get; set; }

    }
}
