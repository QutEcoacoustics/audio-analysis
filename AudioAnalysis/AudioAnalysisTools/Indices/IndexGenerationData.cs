// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexGenerationData.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;

    using AudioAnalysisTools.LongDurationSpectrograms;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;

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



        // ********************************************************************************************************************
        // STATIC METHODS

        /// <summary>
        /// Returns the index generation data from file in passed directory.
        /// </summary>
        /// <param name="directory">
        /// </param>
        /// <returns>
        /// </returns>
        public static IndexGenerationData GetIndexGenerationData(DirectoryInfo directory)
        {
            return Json.Deserialise<IndexGenerationData>(FindFile(directory));
        }

        public static FileInfo FindFile(DirectoryInfo directory)
        {
            const string Pattern = "*" + FileNameFragment + "*";
            return directory.GetFiles(Pattern).Single();
        }

        public static IndexGenerationData GetIndexGenerationDataAndAddStartTime(DirectoryInfo directory, string fileName, TimeSpan? offsetHint = null)
        {
            var indexGenerationData = IndexGenerationData.GetIndexGenerationData(directory);

            // Get the start time from the file name.
            // DateTimeOffset startTime = IndexMatrices.GetFileStartTime(fileName);   // ##################### CHANGE TO ANTHONY'S METHOD
            DateTimeOffset startTime;
            if (!FileDateHelpers.FileNameContainsDateTime(fileName, out startTime, offsetHint))
            {
                LoggedConsole.WriteLine("WARNING from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(" + fileName + ") ");
                LoggedConsole.WriteLine("  File name <{0}> does not contain a valid DateTime = {0}", fileName);
            }

            indexGenerationData.RecordingStartDate = startTime;
            return indexGenerationData;
        }


    }
}
