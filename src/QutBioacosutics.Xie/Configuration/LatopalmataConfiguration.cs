namespace QutBioacosutics.Xie.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class LatopalmataConfiguration
    {
        public double AmplitudeThresholdLatopalmata { get; set; }
        public int RangeLatopalmata { get; set; }
        public int DistanceLatopalmata { get; set; }

        public double BinToleranceLatopalmata { get; set; }
        public int FrameThresholdLatopalmata { get; set; }
        public int TrackDurationThresholdLatopalmata { get; set; }
        public double TrackThresholdLatopalmata { get; set; }
        public int MaximumTrackDurationLatopalmata { get; set; }
        public int MinimumTrackDurationLatopalmata { get; set; }
        public double BinDifferenceLatopalmata { get; set; }

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

        public LatopalmataConfiguration(dynamic configuration)
        {
            // Peak parameters
            this.AmplitudeThresholdLatopalmata = configuration.AmplitudeThresholdLatopalmata;   // Decibel---the minimum amplitude value
            this.RangeLatopalmata = configuration.RangeLatopalmata;                                // Frame---the distance in either side for selecting peaks
            this.DistanceLatopalmata = configuration.DistanceLatopalmata;                          // Frame---remove near peaks
            // Track parameters
            this.BinToleranceLatopalmata = configuration.BinToleranceLatopalmata;                 // Bin---the fluctuation of the dominant frequency bin
            this.FrameThresholdLatopalmata = configuration.FrameThresholdLatopalmata;              // Frame---frame numbers of the silence
            this.TrackDurationThresholdLatopalmata = configuration.TrackDurationThresholdLatopalmata;
            this.TrackThresholdLatopalmata = configuration.TrackThresholdLatopalmata;           // Used for calculating the percent of peaks in one track
            this.MaximumTrackDurationLatopalmata = configuration.MaximumTrackDurationLatopalmata;  // Minimum duration of tracks
            this.MinimumTrackDurationLatopalmata = configuration.MinimumTrackDurationLatopalmata;  // Maximum duration of tracks
            this.BinDifferenceLatopalmata = configuration.BinDifferenceLatopalmata;             // Difference between the highest and lowest bins
            // Band tracks parameters
            this.FrequencyLowLatopalmata = configuration.FrequencyLowLatopalmata;
            this.FrequencyHighLatopalmata = configuration.FrequencyHighLatopalmata;
            // DCT
            //int minimumOscillationNumberLatopalmata = configuration.minimumOscillationNumberLatopalmata;
            //int maximumOscillationNumberLatopalmata = configuration.maximumOscillationNumberLatopalmata;
            //int minimumFrequencyLatopalmata = configuration.MinimumFrequencyLatopalmata;
            //int maximumFrequencyLatopalmata = configuration.MaximumFrequencyLatopalmata;
            //double dct_DurationLatopalmata = configuration.Dct_DurationLatopalmata;
            //double dct_ThresholdLatopalmata = configuration.Dct_ThresholdLatopalmata;

            this.DoSlopeLatopalmata = configuration.DoSlopeLatopalmata;

            this.HarmonicComponentLatopalmata = configuration.HarmonicComponentLatopalmata;
            this.HarmonicSensityLatopalmata = configuration.HarmonicSensityLatopalmata;
            this.HarmonicDiffrangeLatopalmata = configuration.HarmonicDiffrangeLatopalmata;

        } //consturctor

    }
}
