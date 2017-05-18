// <copyright file="IndexCalculateConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools.Indices
{
    /// <summary>
    /// CONFIG CLASS FOR the class IndexCalculate.cs
    /// </summary>
    public class IndexCalculateConfig
    {
        private bool displayWeightedIndices;
        private static int defaultWindowSize = 512;
        private static int defaultLowFreqBound = 1000;
        private static int defaultMidFreqBound = 8000;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexCalculateConfig"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public IndexCalculateConfig()
        {
            // default values
            this.AnalysisName = "Towsey.Analysis";
            this.SegmentDuration = 1;
            this.SegmentOverlap = 0;
            this.IndexCalculationDuration = 60.0;
            this.BgNoiseBuffer = 5.0;

            this.FrameLength = defaultWindowSize;
            this.FrameStep = 441;
            this.ResampleRate = 22050;

            this.LowFreqBound = defaultLowFreqBound;
            this.MidFreqBound = defaultMidFreqBound;

            this.EventThreshold = 0.2;

            this.SaveIntermediateFiles = "WhenEventsDetected";
            this.SaveSonogramData = false;
            this.SaveSonogramImages = false;
            this.DisplayCsvImage = false;
            this.displayWeightedIndices = false;
            this.IndexPropertiesConfig = @"./IndexPropertiesConfig.yml";

            this.ParallelProcessing = false;
            this.TileImageOutput = false;
            this.RequireDateInFilename = false;
        }

        /// <summary>
        /// Gets or sets the Analysis name
        /// The default = Towsey.Acoustic
        /// </summary>
        public string AnalysisName { get; set; }

        /// <summary>
        /// Gets or sets the SegmentDuration
        /// 60 seconds; use 0.2 for zooming spectrogram tiles
        /// Default value = 1.0
        /// Units=minutes
        /// </summary>
        public int SegmentDuration { get; set; }

        /// <summary>
        /// Gets or sets the SegmentOverlap
        /// Default value = 0.
        /// Units=seconds
        /// </summary>
        public int SegmentOverlap { get; set; }

        /// <summary>
        /// Gets or sets the Timespan (in seconds) over which summary and spectral indices are calculated
        /// Default=
        /// Units=seconds
        /// </summary>
        public double IndexCalculationDuration { get; set; }

        /// <summary>
        /// Gets or sets bG noise for any location is calculated by extending the region of index calculation from 5 seconds before start to 5 sec after end of current index interval.
        /// Ten seconds is considered a minimum interval to obtain a reliable estimate of BG noise.
        /// The  BG noise interval is not extended beyond start or end of recording segment.
        /// Consequently for a 60sec Index calculation duration, the  BG noise is calculated form the 60sec segment only.
        /// Default=5 seconds
        /// Units=seconds
        /// </summary>
        public double BgNoiseBuffer { get; set; }

        /// <summary>
        /// Gets or sets the FrameWidth.
        /// FrameWidth is used WITHOUT overlap to calculate the spectral indices.
        /// Default value = 512.
        /// Units=samples
        /// </summary>
        public int FrameLength { get; set; }

        /// <summary>
        /// Gets the FrameStep.
        /// FrameWidth is used WITHOUT overlap to calculate the spectral indices.
        /// Default step value when calculating summary and spectral indices = FrameLength = 512.
        /// Default step value when calculating spectral indices for ZOOMING spectrograms = 441.
        /// Units=samples
        /// IMPORTANT NOTE: The value for FrameStep is used ONLY when calculating a standard spectrogram within the ZOOMING spectrogram function.
        /// FrameStep is NOT used when calculating Summary and Spectral indices.
        /// However the FrameStep entry must NOT be deleted from the config. Must keep its value for when it is required.
        /// The value 441 should NOT be changed because it has been calculated specifically for current ZOOMING spectrogram set-up.
        /// TODO: this option should be refactored out into the spectrogram generation analyzer - currently confusing implementation
        /// </summary>
        public int FrameStep { get; }

        /// <summary>
        /// Gets or sets the ResampleRate.
        /// ResampleRate must be 2 X the desired Nyquist.
        /// Default value = 22050.
        /// Once upon a time we used 17640.
        /// Units=samples
        /// </summary>
        public int ResampleRate { get; set; }

        /// <summary>
        /// Gets or sets the LowFreqBound.
        /// Default value = 1000.
        /// Units=Herz
        /// </summary>
        public int LowFreqBound { get; set; }

        /// <summary>
        /// Gets or sets the MidFreqBound.
        /// Default value = 8000.
        /// Units=Herz
        /// </summary>
        public int MidFreqBound { get; set; }

        /// <summary>
        /// Gets or sets the options to SaveIntermediatefiles
        /// Available options (case-sensitive): [False/Never | True/Always | WhenEventsDetected]
        /// The default = "Never"
        /// </summary>
        public string SaveIntermediateFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to SaveSonogramData
        /// SAVE SONOGRAM DATA FILES FOR SUBSEQUENT ZOOMING SPECTROGRAMS
        /// Next two parameters are used only when creating images for zooming spectrograms.
        /// WARNING: IndexCalculationDuration must be set = 0.2  when SaveSonogramData = true
        /// # TODO: this option should be refactored out into the spectrogram generation analyzer - currently confusing implementation
        /// The default = false
        /// </summary>
        public bool SaveSonogramData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether one-minute spectrograms can be saved in any analysis task.
        /// Available options (case-sensitive): [False/Never | True/Always | WhenEventsDetected]
        /// The default = false
        /// </summary>
        public bool SaveSonogramImages { get; set; }

        /// <summary>
        /// Gets a value indicating whether to DisplayCsvImage
        /// DisplayCsvImage is obsolete - ensure it remains set to: false
        /// </summary>
        public bool DisplayCsvImage { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to TileImageOutput
        /// Default value: false
        /// </summary>
        public bool ParallelProcessing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to do ParallelProcessing
        /// If true, an additional set of images will be produced that are tiles
        /// If true, RequireDateInFilename must be set
        /// Default value: false
        /// </summary>
        public bool TileImageOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether RequireDateInFilename
        /// If true, an unambiguous date time must be provided in the source file's name.
        /// If true, an exception will be thrown if no such date is found
        /// If false, and a valid date is still found in file name, it will still be parsed
        /// Supports formats like:
        ///         prefix_20140101T235959+1000.mp3
        ///         prefix_20140101T235959+Z.mp3
        ///         prefix_20140101-235959+1000.mp3
        ///         prefix_20140101-235959+Z.mp3
        /// Default value: false
        /// </summary>
        public bool RequireDateInFilename { get; set; }

        /// <summary>
        /// Gets or sets location of the IndexPropertiesConfig file
        /// Available options (case-sensitive): [False/Never | True/Always | WhenEventsDetected]
        /// The default = './IndexPropertiesConfig.yml'
        /// </summary>
        public string IndexPropertiesConfig { get; set; }

        /// <summary>
        /// Gets or sets threshold value for detecting an acoustic event.
        /// Forgotten what htis is used for!!!!!!
        /// Default=0.2
        /// Units=none
        /// </summary>
        public double EventThreshold { get; set; }

        public static IndexCalculateConfig GetDefaultConfig()
        {
            return new IndexCalculateConfig();
        }

        public static IndexCalculateConfig GetDefaultConfig(dynamic dynamicConfig)
        {
            var config = new IndexCalculateConfig();

            //var indexCalculationDuration = (int?)dynamicConfig[AnalysisKeys.FrameLength] ?? defaultWindowSize;
            config.FrameLength = (int?)dynamicConfig[AnalysisKeys.FrameLength] ?? defaultWindowSize;
            config.MidFreqBound = (int?)dynamicConfig[AnalysisKeys.MidFreqBound] ?? defaultMidFreqBound;
            config.LowFreqBound = (int?)dynamicConfig[AnalysisKeys.LowFreqBound] ?? defaultLowFreqBound;

            //config.OctaveScale = (bool?)dynamicConfig["OctaveFreqScale"] ?? false;
            return config;
        }
    }
}
