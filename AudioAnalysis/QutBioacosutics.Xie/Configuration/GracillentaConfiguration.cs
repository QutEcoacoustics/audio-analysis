using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie.Configuration
{
    public class GracillentaConfiguration
    {
        public double AmplitudeThresholdGracillenta { get; set; }
        //public int RangeGracillenta { get; set; }
        //public int DistanceGracillenta { get; set; }

        public double BinToreanceGracillenta { get; set; }
        public int FrameThresholdGracillenta { get; set; }
        public int TrackDurationThresholdGracillenta { get; set; }
        public double TrackThresholdGracillenta { get; set; }
        public int MaximumTrackDurationGracillenta { get; set; }
        public int MinimumTrackDurationGracillenta { get; set; }
        public double BinDifferencGracillenta { get; set; }

        public int FrequencyLowGracillenta { get; set; }
        public int FrequencyHighGracillenta { get; set; }

        //public int MinimumOscillationNumberGracillenta { get; set; }
        //public int MaximumOscillationNumberGracillenta { get; set; }
        //public int MinimumFrequencyGracillenta { get; set; }
        //public int MaximumFrequencyGracillenta { get; set; }
        //public double Dct_DurationGracillenta { get; set; }
        //public double Dct_ThresholdGracillenta { get; set; }

        public bool DoSlopeGracillenta { get; set; }
    }
}
