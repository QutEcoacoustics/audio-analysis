using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie.Configuration
{
    public class NasutaConfiguration
    {
        public double AmplitudeThresholdNasuta { get; set; }
        public int RangeNasuta { get; set; }
        public int DistanceNasuta { get; set; }

        public double BinToreanceNasuta { get; set; }
        public int FrameThresholdNasuta { get; set; }
        public int TrackDurationThresholdNasuta { get; set; }
        public double TrackThresholdNasuta { get; set; }
        public int MaximumTrackDurationNasuta { get; set; }
        public int MinimumTrackDurationNasuta { get; set; }
        public double BinDifferencNasuta { get; set; }

        public int FrequencyLowNasuta { get; set; }
        public int FrequencyHighNasuta { get; set; }

        public int MinimumOscillationNumberNasuta { get; set; }
        public int MaximumOscillationNumberNasuta { get; set; }
        public int MinimumFrequencyNasuta { get; set; }
        public int MaximumFrequencyNasuta { get; set; }
        public double Dct_DurationNasuta { get; set; }
        public double Dct_ThresholdNasuta { get; set; }

        public bool DoSlopeNasuta { get; set; }

        public int HarmonicComponentNasuta { get; set; }
        public int HarmonicSensityNasuta { get; set; }
    }
}
