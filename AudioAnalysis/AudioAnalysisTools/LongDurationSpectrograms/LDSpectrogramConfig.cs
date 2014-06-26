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

    /// <summary>
    ///     CONFIG CLASS FOR the class LDSpectrogramRGB
    /// </summary>
    public class LDSpectrogramConfig
    {
        // these parameters manipulate the colour map and appearance of the false-colour spectrogram
        #region Fields

        // these parameters manipulate the colour map and appearance of the false-colour spectrogram
        // pass two colour maps because interesting to draw a double image.

        // default value for frame width from which spectrogram was derived. Assume no frame overlap.

        // default recording starts at zero minute of day i.e. midnight

        // assume one minute spectra and hourly time lines

        // mark 1 kHz intervals
        private int yAxisTicInterval = 1000; 

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LDSpectrogramConfig"/> class. 
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="fileName">
        /// </param>
        /// <param name="inputDirectory">
        /// </param>
        /// <param name="outputDirectory">
        /// </param>
        public LDSpectrogramConfig(string fileName, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory)
        {
            this.XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            this.SampleRate = SpectrogramConstants.SAMPLE_RATE;
            this.MinuteOffset = SpectrogramConstants.MINUTE_OFFSET;
            this.FrameWidth = SpectrogramConstants.FRAME_WIDTH;
            this.ColourMap2 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            this.ColourMap1 = SpectrogramConstants.RGBMap_BGN_AVG_CVR;
            this.BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            this.FileName = fileName;
            this.InputDirectory = inputDirectory;
            this.OutputDirectory = outputDirectory;

            // DEFAULT VALUES are set for the remaining parameters            
        }

        #endregion

        #region Public Properties

        public double BackgroundFilterCoeff { get; set; }

        public string ColourMap1 { get; set; }

        public string ColourMap2 { get; set; }

        public string FileName { get; set; }

        public int FrameWidth { get; set; }

        public DirectoryInfo InputDirectory { get; set; }

        public TimeSpan MinuteOffset { get; set; }

        public DirectoryInfo OutputDirectory { get; set; }

        public int SampleRate { get; set; }

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
        /// The <see cref="LDSpectrogramConfig"/>.
        /// </returns>
        public static LDSpectrogramConfig ReadYAMLToConfig(FileInfo path)
        {
            // load YAML configuration
            dynamic configuration = Yaml.Deserialise(path);

            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside this method. 
             * Extract all params below.
             */
            var inputDirectory = new DirectoryInfo((string)configuration.InputDirectory);
            var outputDirectory = new DirectoryInfo((string)configuration.OutputDirectory);

            var config = new LDSpectrogramConfig((string)configuration.FileName, inputDirectory, outputDirectory);

            // these parameters manipulate the colour map and appearance of the false-colour spectrogram
            config.ColourMap1 = (string)configuration.ColourMap1;
            config.ColourMap2 = (string)configuration.ColourMap2;
            config.BackgroundFilterCoeff = (double)configuration.BackgroundFilterCoeff; // must be value <=1.0

            // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
            config.SampleRate = (int)configuration.SampleRate;
            config.FrameWidth = (int)configuration.FrameWidth;
                
                // frame width from which spectrogram was derived. Assume no frame overlap.
            config.MinuteOffset = TimeSpan.FromMinutes((double)configuration.MinuteOffset);
                
                // default is recording starts at the zero-eth minute of the day i.e. midnight
            config.XAxisTicInterval = TimeSpan.FromMinutes((double)configuration.XaxisTicInterval);
                
                // default is one minute spectra and hourly time lines
            config.YAxisTicInterval = (int)configuration.YaxisTicInterval; // default is 1000 Herz

            return config;
        }

        public void WriteConfigToYaml(FileInfo path)
        {
            // WRITE THE YAML CONFIG FILE
            Yaml.Serialise(
                path, 
                new
                    {
                        // paths to required directories and files
                        this.FileName, 
                        InputDirectory = this.InputDirectory.FullName, 
                        OutputDirectory = this.OutputDirectory.FullName, 

                        // these parameters manipulate the colour map and appearance of the false-colour spectrogram
                        this.ColourMap1, 
                        this.ColourMap2, 
                        this.BackgroundFilterCoeff, // must be value <=1.0

                        // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
                        this.SampleRate, 
                        this.FrameWidth, // frame width from which spectrogram was derived. Assume no frame overlap.
                        MinuteOffset = this.MinuteOffset.TotalMinutes, 
                        
                        // default is recording starts at zero minute of day i.e. midnight
                        XaxisTicInterval = this.XAxisTicInterval.TotalMinutes, 
                        
                        // default is one minute spectra and hourly time lines
                        YaxisTicInterval = this.YAxisTicInterval // default is 1000 Herz
                    });
        }

        #endregion

        // WritConfigToYAML()
    } // LDSpectrogramConfig
}