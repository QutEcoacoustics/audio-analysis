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

        public double BinToleranceNasuta { get; set; }
        public int FrameThresholdNasuta { get; set; }
        public int TrackDurationThresholdNasuta { get; set; }
        public double TrackThresholdNasuta { get; set; }
        public int MaximumTrackDurationNasuta { get; set; }
        public int MinimumTrackDurationNasuta { get; set; }
        public double BinDifferenceNasuta { get; set; }

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
        public int HarmonicDiffrangeNasuta { get; set; }
    

        public NasutaConfiguration(dynamic configuration)
        {                            
            // Peak parameters
            AmplitudeThresholdNasuta = configuration.AmplitudeThresholdNasuta;   // Decibel---the minimum amplitude value
            RangeNasuta = configuration.RangeNasuta;                                // Frame---the distance in either side for selecting peaks
            DistanceNasuta = configuration.DistanceNasuta;                          // Frame---remove near peaks
            // Track parameters
            BinToleranceNasuta = configuration.BinToleranceNasuta;                 // Bin---the fluctuation of the dominant frequency bin 
            FrameThresholdNasuta = configuration.FrameThresholdNasuta;              // Frame---frame numbers of the silence    
            TrackDurationThresholdNasuta = configuration.TrackDurationThresholdNasuta;
            TrackThresholdNasuta = configuration.TrackThresholdNasuta;           // Used for calculating the percent of peaks in one track    
            MaximumTrackDurationNasuta = configuration.MaximumTrackDurationNasuta;  // Minimum duration of tracks
            MinimumTrackDurationNasuta = configuration.MinimumTrackDurationNasuta;  // Maximum duration of tracks   
            BinDifferenceNasuta = configuration.BinDifferenceNasuta;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            FrequencyLowNasuta = configuration.FrequencyLowNasuta;
            FrequencyHighNasuta = configuration.FrequencyHighNasuta;
            // DCT
            MinimumOscillationNumberNasuta = configuration.minimumOscillationNumberNasuta;
            MaximumOscillationNumberNasuta = configuration.maximumOscillationNumberNasuta;
            MinimumFrequencyNasuta = configuration.MinimumFrequencyNasuta;
            MaximumFrequencyNasuta = configuration.MaximumFrequencyNasuta;
            Dct_DurationNasuta = configuration.Dct_DurationNasuta;
            Dct_ThresholdNasuta = configuration.Dct_ThresholdNasuta;

            DoSlopeNasuta = configuration.DoSlopeNasuta;

            HarmonicComponentNasuta = configuration.HarmonicComponentNasuta;
            HarmonicSensityNasuta = configuration.HarmonicSensityNasuta;
            HarmonicDiffrangeNasuta = configuration.HarmonicDiffrangeNasuta;
        } // consturctor
            
    } // class

}
