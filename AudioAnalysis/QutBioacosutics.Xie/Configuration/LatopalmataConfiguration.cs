using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie.Configuration
{
    class LatopalmataConfiguration
    {
        public double AmplitudeThresholdLatopalmata { get; set; }
        public int RangeLatopalmata { get; set; }
        public int DistanceLatopalmata { get; set; }

        public double BinToreanceLatopalmata { get; set; }
        public int FrameThresholdLatopalmata { get; set; }
        public int TrackDurationThresholdLatopalmata { get; set; }
        public double TrackThresholdLatopalmata { get; set; }
        public int MaximumTrackDurationLatopalmata { get; set; }
        public int MinimumTrackDurationLatopalmata { get; set; }
        public double BinDifferencLatopalmata { get; set; }

        public int FrequencyLowLatopalmata { get; set; }
        public int FrequencyHighLatopalmata { get; set; }

        //public int MinimumOscillationNumberLatopalmata { get; set; }
        //public int MaximumOscillationNumberLatopalmata { get; set; }
        //public int MinimumFrequencyLatopalmata { get; set; }
        //public int MaximumFrequencyLatopalmata { get; set; }
        //public double Dct_DurationLatopalmata { get; set; }
        //public double Dct_ThresholdLatopalmata { get; set; }

        public bool DoSlopeLatopalmata { get; set; }

        public int HarmonicComponentLatopalmata { get; set; }
        public int HarmonicSensityLatopalmata { get; set; }
        public int HarmonicDiffrangeLatopalmata { get; set; }
            

    }
}
