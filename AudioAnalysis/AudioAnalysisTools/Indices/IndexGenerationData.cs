// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexGenerationData.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;

    using AudioAnalysisTools.LongDurationSpectrograms;

    public class IndexGenerationData
    {
        /// <summary>
        /// Gets or sets the configuration options used to draw long duration spectrograms
        /// </summary>
        public LdSpectrogramConfig LongDurationSpectrogramConfig { get; set; }

        public const string FileNameFragment = "IndexGenerationData";

        public IndexGenerationData()
        {
            /* Ant: 
             *  I Disabled these defaults. They do not make sense.
             *  The index generation data is NOT valid if it is missing values.
             *  That is not an error that should be automatically compensated for.
             *  Left the code in for clarity.
             *  
            // these are default values only. Must be reset if different
            this.RecordingType = "undefined";
            this.IndexCalculationDuration = TimeSpan.FromMinutes(1.0);
            this.SampleRateOriginal  = SpectrogramConstants.SAMPLE_RATE;
            this.SampleRateResampled = SpectrogramConstants.SAMPLE_RATE;
            this.RecordingStartDate       = DateTimeOffset.MinValue;
            this.MinuteOffset = SpectrogramConstants.MINUTE_OFFSET;
            this.FrameLength   = SpectrogramConstants.FRAME_LENGTH;
            this.FrameStep    = SpectrogramConstants.FRAME_LENGTH;
            this.BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            */
        }

        /// <summary>
        /// The extension of the original audio file.
        /// </summary>
        public string RecordingType { get; set; }

        /// <summary>
        /// BackgroundFilterCoeff is used to adjust colour contrast of false-colour images. Default = 0.75.
        /// </summary>
        public double BackgroundFilterCoeff { get; set; }

        /// <summary>
        ///  default value for frame width from which spectrogram was derived.
        /// </summary>
        public int FrameLength { get; set; }

        /// <summary>
        ///  default value for frame step from which spectrogram was derived. There may be overlap.
        /// </summary>
        public int FrameStep { get; set; }

        /// <summary>
        /// Gets or sets the date the audio was recorded. Originally parsed from the file name by <c>FileDateHelpers</c>.
        /// </summary>
        public DateTimeOffset? RecordingStartDate { get; set; }

        /*
         * DISABLED
         * None of these properties are relevant to index generation and should not be included in IndexGenerationData
         * 
        /// <summary>
        /// The site at which the recording was made.
        /// This and latitude and longitude info are put here because an instance of IndexGenerationData is used to pass 
        /// all possible info when drawing the FC spectrogram.
        /// </summary>
        public string SiteName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
         * */

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
