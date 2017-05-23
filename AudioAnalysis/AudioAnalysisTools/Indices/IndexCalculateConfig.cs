// <copyright file="IndexCalculateConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools.Indices
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared;
    using DSP;

    /// <summary>
    /// CONFIG CLASS FOR the class IndexCalculate.cs
    /// </summary>
    public class IndexCalculateConfig
    {
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 22050) THEN frame duration = 5.805ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 22050) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 512 AND sample rate = 22050) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 11025) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 11025) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 17640) THEN frame duration = 18.576ms.

        public const int DefaultResampleRate = 22050;
        public const int DefaultWindowSize = 512;
        public const int DefaultIndexCalculationDurationInSeconds = 60;

        // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        // The midband, 1000Hz to 8000Hz, covers the bird-band in SERF & Gympie recordings.
        public const int DefaultHighFreqBound = 11000;
        public const int DefaultMidFreqBound = 8000;
        public const int DefaultLowFreqBound = 1000;

        public const string DefaultFrequencyScaleType = "Linear";

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexCalculateConfig"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public IndexCalculateConfig()
        {
            // default values
            this.AnalysisName = "Towsey.Analysis";
            this.SegmentDuration = TimeSpan.FromSeconds(60);
            this.SegmentOverlap = TimeSpan.Zero;
            this.IndexCalculationDuration = TimeSpan.FromSeconds(DefaultIndexCalculationDurationInSeconds);
            this.BgNoiseBuffer = TimeSpan.FromSeconds(5);

            this.FrameLength = DefaultWindowSize;

            // this framestep gives an exact 20ms frame when doing hi-resolution zooming spectrograms.
            this.FrameStep = 441;
            this.ResampleRate = DefaultResampleRate;

            this.LowFreqBound = DefaultLowFreqBound;
            this.MidFreqBound = DefaultMidFreqBound;
            this.SetTypeOfFreqScale(DefaultFrequencyScaleType);

            this.SaveIntermediateFiles = "Never";
            this.SaveSonogramImages = "Never";

            this.SaveSonogramData = false;
            this.DisplayCsvImage = false;
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
        public TimeSpan SegmentDuration { get; set; }

        /// <summary>
        /// Gets or sets the SegmentOverlap
        /// Default value = 0.
        /// Units=seconds
        /// </summary>
        public TimeSpan SegmentOverlap { get; set; }

        /// <summary>
        /// Gets or sets the Timespan (in seconds) over which summary and spectral indices are calculated
        /// Default=
        /// Units=seconds
        /// </summary>
        public TimeSpan IndexCalculationDuration { get; set; }

        /// <summary>
        /// Gets or sets bG noise for any location is calculated by extending the region of index calculation from 5 seconds before start to 5 sec after end of current index interval.
        /// Ten seconds is considered a minimum interval to obtain a reliable estimate of BG noise.
        /// The  BG noise interval is not extended beyond start or end of recording segment.
        /// Consequently for a 60sec Index calculation duration, the  BG noise is calculated form the 60sec segment only.
        /// Default=5 seconds
        /// Units=seconds
        /// </summary>
        public TimeSpan BgNoiseBuffer { get; set; }

        /// <summary>
        /// Gets or sets the FrameWidth.
        /// FrameWidth is used WITHOUT overlap to calculate the spectral indices.
        /// Default value = 512.
        /// Units=samples
        /// </summary>
        public int FrameLength { get; set; }

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
        /// Frequency scale is Linear or OCtave
        /// </summary>
        private FreqScaleType frequencyScaleType;

        /// <summary>
        /// Gets or sets the type of Herz frequency scale
        /// </summary>
        public void SetTypeOfFreqScale(string scaleName)
        {
            if (scaleName == "Linear")
            {
                this.frequencyScaleType = FreqScaleType.Linear;
            }
            else
            if (scaleName == "Octave")
            {
                this.frequencyScaleType = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            }
            else
            {
                throw new Exception("ERROR:    Invalid FrequencyScaleType");
            }
        }

        public FreqScaleType GetTypeOfFreqScale()
        {
            return this.frequencyScaleType;
        }

        /// <summary>
        /// Gets or sets the options to SaveIntermediatefiles
        /// Available options (case-sensitive): [False/Never | True/Always | WhenEventsDetected]
        /// The default = "Never"
        /// </summary>
        public string SaveIntermediateFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether one-minute spectrograms can be saved in any analysis task.
        /// Available options (case-sensitive): [False/Never | True/Always | WhenEventsDetected]
        /// The default = false
        /// </summary>
        public string SaveSonogramImages { get; set; }

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

        public static IndexCalculateConfig GetDefaultConfig()
        {
            return new IndexCalculateConfig();
        }

        // ##############################################################################################################################
        // THE FOLLOWING THREE PROPERITIES SHOULD BE REMOVED

        /// <summary>
        /// Gets a value indicating whether to DisplayCsvImage
        /// DisplayCsvImage is obsolete - ensure it remains set to: false
        /// </summary>
        public bool DisplayCsvImage { get; }

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
        private int FrameStep { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to SaveSonogramData
        /// SAVE SONOGRAM DATA FILES FOR SUBSEQUENT ZOOMING SPECTROGRAMS
        /// Next two parameters are used only when creating images for zooming spectrograms.
        /// WARNING: IndexCalculationDuration must be set = 0.2  when SaveSonogramData = true
        /// # TODO: this option should be refactored out into the spectrogram generation analyzer - currently confusing implementation
        /// The default = false
        /// </summary>
        private bool SaveSonogramData { get; set; }

        // ##############################################################################################################################
        // THE FOLLOWING STATIC METHODS RETURN CONFIG FILES

        /// <summary>
        /// Link method to one which does the real work.
        /// </summary>
        /// <param name="configFile">the config file to be read dynamically</param>
        public static IndexCalculateConfig GetConfig(FileInfo configFile)
        {
            dynamic dynamicConfig = Yaml.Deserialise(configFile);
            var config = IndexCalculateConfig.GetConfig(dynamicConfig, false);
            return config;
        }

        /// <summary>
        /// WARNING: This method does not incorporate all the parameters in the config.yml file.
        /// Only those that are likely to change.
        /// If you want to change a config parameter in the yml file make sure it appears in this method.
        /// </summary>
        /// <param name="dynamicConfig">the dynamic config</param>
        /// <param name="writeParameters">default = false</param>
        public static IndexCalculateConfig GetConfig(dynamic dynamicConfig, bool writeParameters = false)
        {
            var config = new IndexCalculateConfig
            {
                AnalysisName = (string)dynamicConfig[AnalysisKeys.AnalysisName] ?? "Towsey.Acoustic",

                // SegmentDuration, //Should ever need to change
                // SegmentOverlap,  //Should ever need to change
                ResampleRate = (int?)dynamicConfig[AnalysisKeys.ResampleRate] ?? DefaultResampleRate,
                FrameLength = (int?)dynamicConfig[AnalysisKeys.FrameLength] ?? DefaultWindowSize,
                MidFreqBound = (int?)dynamicConfig[AnalysisKeys.MidFreqBound] ?? DefaultMidFreqBound,
                LowFreqBound = (int?)dynamicConfig[AnalysisKeys.LowFreqBound] ?? DefaultLowFreqBound,

                SaveIntermediateFiles = (string)dynamicConfig["SaveIntermediateFiles"] ?? "Never",
                SaveSonogramImages = (string)dynamicConfig["SaveIntermediateFiles"] ?? "Never",

                //SaveCsvFile,       //Should ever need to change
                //SaveSonogramData,  //Should ever need to change

                ParallelProcessing = (bool?)dynamicConfig["ParallelProcessing"] ?? false,
                TileImageOutput = (bool?)dynamicConfig["TileImageOutput"] ?? false,
                RequireDateInFilename = (bool?)dynamicConfig["RequireDateInFilename"] ?? false,
            };

            double duration = (double?)dynamicConfig[AnalysisKeys.IndexCalculationDuration] ?? DefaultIndexCalculationDurationInSeconds;
            config.IndexCalculationDuration = TimeSpan.FromSeconds(duration);
            duration = (double?)dynamicConfig[AnalysisKeys.BGNoiseNeighbourhood] ?? 5.0;
            config.BgNoiseBuffer = TimeSpan.FromSeconds(duration);
            string stringvalue = (string)dynamicConfig["FrequencyScale"];
            config.SetTypeOfFreqScale(stringvalue);

            if (writeParameters)
            {
                // print out the sonogram parameters
                LoggedConsole.WriteLine("\nPARAMETERS");
                //foreach (KeyValuePair<string, string> kvp in configDict)
                //{
                //    LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                //}
            }

            return config;
        }
    }
}
