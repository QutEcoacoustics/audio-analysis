using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools.Indices
{
    using AudioAnalysisTools.LongDurationSpectrograms;

    public class IndexGenerationData
    {
        public const string FileNameFragment = "IndexGenerationData";

        public IndexGenerationData()
        {
            //these are default values only. Must be reset if different
            this.RecordingType = "undefined";
            this.IndexCalculationDuration = TimeSpan.FromMinutes(1.0);
            this.SampleRateOriginal  = SpectrogramConstants.SAMPLE_RATE;
            this.SampleRateResampled = SpectrogramConstants.SAMPLE_RATE;
            this.RecordingStart       = DateTimeOffset.MinValue;
            this.MinuteOffset = SpectrogramConstants.MINUTE_OFFSET;
            this.FrameWidth   = SpectrogramConstants.FRAME_WIDTH;
            this.FrameStep    = SpectrogramConstants.FRAME_WIDTH;
            this.BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
        }

        /// <summary>
        /// 
        /// </summary>
        public string RecordingType { get; set; }

        /// <summary>

        /// <summary>
        /// 
        /// </summary>
        public double BackgroundFilterCoeff { get; set; }

        /// <summary>
        ///  default value for frame width from which spectrogram was derived.
        /// </summary>
        public int FrameWidth { get; set; }

        /// <summary>
        ///  default value for frame step from which spectrogram was derived. There may be overlap.
        /// </summary>
        public int FrameStep { get; set; }
        public DateTimeOffset RecordingStart { get; set; }
        public TimeSpan MinuteOffset { get; set; }
         
        public int SampleRateOriginal { get; set; }
        public int SampleRateResampled { get; set; }

        /// <summary>
        /// The default is one minute spectra i.e. 60 per hour.  However, as of January 2015, this is not fixed. 
        /// User must enter the time span over which indices are calculated.
        /// This TimeSpan is used to calculate a tic interval that is appropriate to the time scale of the spectrogram.
        /// </summary>
        public TimeSpan IndexCalculationDuration { get; set; }

        /// <summary>
        /// The default is the entire segment i.e. typically of one minute duration.  However, as of January 2015, this is not fixed. 
        /// User must enter the time span over which indices are calculated.
        /// If IndexCalculationDuration is set to a brief duration such as 0.2 seconds, then
        /// the backgroundnoise will be calculated from N seconds before the current subsegment to N seconds after => N secs + subseg duration + N secs
        /// </summary>
        public TimeSpan BGNoiseNeighbourhood { get; set; }
    }
}
