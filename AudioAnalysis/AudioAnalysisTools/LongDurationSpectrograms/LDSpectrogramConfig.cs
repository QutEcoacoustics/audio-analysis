// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogramConfig.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.IO;

    using Acoustics.Shared;

    using YamlDotNet.Serialization;

    /// <summary>
    ///     CONFIG CLASS FOR the class LDSpectrogramRGB
    /// </summary>
    public class LdSpectrogramConfig
    {
        #region Fields
        /// <summary>
        ///  mark 1 kHz intervals
        /// </summary>
        private int yAxisTicInterval = 1000;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LdSpectrogramConfig"/> class. 
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="fileName">
        /// </param>
        /// <param name="inputDirectory">
        /// </param>
        /// <param name="outputDirectory">
        /// </param>
        public LdSpectrogramConfig(string fileName, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory)
            : this()
        {
            this.FileName = fileName;
            this.InputDirectoryInfo = inputDirectory;
            this.OutputDirectoryInfo = outputDirectory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdSpectrogramConfig"/> class. 
        /// CONSTRUCTOR
        /// </summary>
        public LdSpectrogramConfig()
        {
            // default values
            this.AnalysisType = SpectrogramConstants.DefaultAnalysisType;
            this.XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            this.IndexCalculationDuration = TimeSpan.FromMinutes(1.0);
            this.SampleRate = SpectrogramConstants.SAMPLE_RATE;
            this.MinuteOffset = SpectrogramConstants.MINUTE_OFFSET;
            this.FrameWidth = SpectrogramConstants.FRAME_WIDTH;
            this.ColourMap1 = SpectrogramConstants.RGBMap_BGN_AVG_EVN;
            this.ColourMap2 = SpectrogramConstants.RGBMap_ACI_ENT_CVR;
            this.BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;        
        }

        #endregion

        #region Public Properties

        private string comment1 = "LIST OF PARAMETER SETTINGS USED TO OBTAIN THE SUMMARY AND SPECTRAL INDICES IN THIS DIRECTORY.";
        public  string COMMENT1 { get { return this.comment1; } set { this.comment1 = value; } }
        private string comment2 = "RESULTS OBTAINED WITH CLASS: AnalyseLongRecording.         ACTIVITY: audio2csv";
        public  string COMMENT2 { get { return this.comment2; } set { this.comment2 = value; } }
        private string comment3 = "THE BELOW PARAMETER VALUES ARE RELEVANT FOR CONSTRUCTING THE TILES OF A ZOOMING SPECTROGRAM.";
        public  string COMMENT3 { get { return this.comment3; } set { this.comment3 = value; } }

        /// <summary>
        /// Name of the analysis type used in file name extentions etc.
        /// </summary>
        public string AnalysisType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double BackgroundFilterCoeff { get; set; }

        /// <summary>
        /// these parameters manipulate the colour map and appearance of the false-colour spectrogram
        /// </summary>
        public string ColourMap1 { get; set; }

        /// <summary>
        /// these parameters manipulate the colour map and appearance of the false-colour spectrogram
        ///  pass two colour maps because interesting to draw a double image.
        /// </summary>
        public string ColourMap2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; set; }

        private string comment4 = "FRAME WIDTH is used without overlap to calculate spectral indices";
        public  string COMMENT4 { get { return this.comment4; } set { this.comment4 = value; } }
        /// <summary>
        ///  default value for frame width from which spectrogram was derived.
        /// </summary>
        public int FrameWidth { get; set; }

        private string comment5 = "FRAME STEP (in samples) is only used for saving spectrogram data. Not used when calculating indices";
        public  string COMMENT5 { get { return this.comment5; } set { this.comment5 = value; } }
        /// <summary>
        ///  default value for frame step from which spectrogram was derived. There may be overlap.
        /// </summary>
        public int FrameStep { get; set; }

        [YamlIgnore]
        public DirectoryInfo InputDirectoryInfo { get; set; }

        public string InputDirectory
        {
            get
            {
                return this.InputDirectoryInfo.FullName;
            }

            set
            {
                this.InputDirectoryInfo = value.ToDirectoryInfo();
            }
        }

        /// <summary>
        /// default recording starts at zero minute of day i.e. midnight
        /// </summary>
        public TimeSpan MinuteOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [YamlIgnore]
        public DirectoryInfo OutputDirectoryInfo { get; set; }

        public string OutputDirectory
        {
            get
            {
                return this.OutputDirectoryInfo.FullName;
            }

            set
            {
                this.OutputDirectoryInfo = value.ToDirectoryInfo();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SampleRate { get; set; }



        private string comment6 = "IndexCalculationDuration (TimeSpan in seconds) is used to calculate summary and spectral indices";
        public  string COMMENT6 { get { return this.comment6; } set { this.comment6 = value; } }
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




        /// <summary>
        /// The default is one minute spectra i.e. 60 per hour
        /// But as of January 2015, this is not fixed. The user can adjust
        ///  the tic interval to be appropriate to the time scale of the spectrogram.
        /// </summary>
        public TimeSpan XAxisTicInterval { get; set; }

        public int YAxisTicInterval
        {
            get
            {
                // convert 1000 Hz to a freq bin interval.
                double freqBinWidth = this.SampleRate / (double)this.FrameWidth;
                return (int)Math.Round(this.yAxisTicInterval / freqBinWidth);
            }

            set
            {
                this.yAxisTicInterval = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// READS A YAML CONFIG FILE into a dynamic variable and then transfers all values into the appropriate config class
        /// </summary>
        /// <param name="path">
        /// </param>
        /// <returns>
        /// The <see cref="LdSpectrogramConfig"/>.
        /// </returns>
        public static LdSpectrogramConfig ReadYamlToConfig(FileInfo path)
        {
            return Yaml.Deserialise<LdSpectrogramConfig>(path);
        }

        public void WriteConfigToYaml(FileInfo path)
        {
            Yaml.Serialise(path, this);
        }
        #endregion
    }
}