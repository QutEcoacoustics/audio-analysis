using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Acoustics.Shared;


namespace AudioAnalysisTools.LongDurationSpectrograms
{
    /// <summary>
    /// CONFIG CLASS FOR the class LDSpectrogramRGB
    /// </summary>
    public class LDSpectrogramConfig
    {

        private string fileName;  // File name from which spectrogram was derived.
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        private DirectoryInfo ipDir;
        public DirectoryInfo InputDirectory
        {
            get { return ipDir; }
            set { ipDir = value; }
        }
        private DirectoryInfo opDir;
        public DirectoryInfo OutputDirectory
        {
            get { return opDir; }
            set { opDir = value; }
        }

        //these parameters manipulate the colour map and appearance of the false-colour spectrogram
        private string colourmap1 = SpectrogramConstants.RGBMap_BGN_AVG_CVR;  // CHANGE default RGB mapping here.
        public string ColourMap1
        {
            get { return colourmap1; }
            set { colourmap1 = value; }
        }
        //these parameters manipulate the colour map and appearance of the false-colour spectrogram
        // pass two colour maps because interesting to draw a double image.
        private string colourmap2 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;  // CHANGE default RGB mapping here.
        public string ColourMap2
        {
            get { return colourmap2; }
            set { colourmap2 = value; }
        }
        private double backgroundFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF; // must be value <=1.0
        public double BackgroundFilterCoeff
        {
            get { return backgroundFilter; }
            set { backgroundFilter = value; }
        }

        // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
        private TimeSpan minOffset = SpectrogramConstants.MINUTE_OFFSET;    // default recording starts at zero minute of day i.e. midnight
        public TimeSpan MinuteOffset
        {
            get { return minOffset; }
            set { minOffset = value; }
        }
        private int frameWidth = SpectrogramConstants.FRAME_WIDTH; // default value for frame width from which spectrogram was derived. Assume no frame overlap.
        public int FrameWidth           // used only to calculate scale of Y-axis to draw grid lines
        {
            get { return frameWidth; }
            set { frameWidth = value; }
        }
        private int sampleRate = SpectrogramConstants.SAMPLE_RATE; // default value - after resampling
        public int SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }

        private TimeSpan x_axis_TicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL;    // assume one minute spectra and hourly time lines
        public TimeSpan X_Axis_TicInterval
        {
            get { return x_axis_TicInterval; }
            set { x_axis_TicInterval = value; }
        }
        private int y_axis_TicInterval = 1000;  // mark 1 kHz intervals
        public int Y_Axis_TicInterval
        {
            get   // convert 1000 Hz to a freq bin interval.
            {
                double freqBinWidth = sampleRate / (double)frameWidth;
                return (int)Math.Round(y_axis_TicInterval / freqBinWidth);
            }
            set { y_axis_TicInterval = value; }
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="_fileName"></param>
        /// <param name="_ipDir"></param>
        /// <param name="_opDir"></param>
        public LDSpectrogramConfig(string _fileName, DirectoryInfo _ipDir, DirectoryInfo _opDir)
        {
            fileName = _fileName;
            ipDir = _ipDir;
            opDir = _opDir;
            // DEFAULT VALUES are set for the remaining parameters            
        }

        /// <summary>
        /// READS A YAML CONFIG FILE into a dynamic variable and then transfers all values into the appropriate config class
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static LDSpectrogramConfig ReadYAMLToConfig(FileInfo path)
        {
            // load YAML configuration
            dynamic configuration = Yaml.Deserialise(path);
            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside this method. 
             * Extract all params below.
             */

            DirectoryInfo ipDir = new DirectoryInfo((string)configuration.InputDirectory);
            DirectoryInfo opDir = new DirectoryInfo((string)configuration.OutputDirectory);

            LDSpectrogramConfig config = new LDSpectrogramConfig((string)configuration.FileName, ipDir, opDir);

            //these parameters manipulate the colour map and appearance of the false-colour spectrogram
            config.ColourMap1 = (string)configuration.ColourMap1;
            config.ColourMap2 = (string)configuration.ColourMap2;
            config.BackgroundFilterCoeff = (double)configuration.BackgroundFilterCoeff; // must be value <=1.0

            // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
            config.SampleRate = (int)configuration.SampleRate;
            config.FrameWidth = (int)configuration.FrameWidth;       // frame width from which spectrogram was derived. Assume no frame overlap.
            config.MinuteOffset = TimeSpan.FromMinutes((double)configuration.MinuteOffset);   // default is recording starts at the zero-eth minute of the day i.e. midnight
            config.X_Axis_TicInterval = TimeSpan.FromMinutes((double)configuration.XaxisTicInterval);     // default is one minute spectra and hourly time lines
            config.Y_Axis_TicInterval = (int)configuration.YaxisTicInterval;              // default is 1000 Herz

            return config;
        } // ReadYAMLToConfig()

        public void WritConfigToYAML(FileInfo path)
        {
            // WRITE THE YAML CONFIG FILE
            Yaml.Serialise(path, new
            {
                //paths to required directories and files
                FileName = this.FileName,
                InputDirectory = this.InputDirectory.FullName,
                OutputDirectory = this.OutputDirectory.FullName,

                //these parameters manipulate the colour map and appearance of the false-colour spectrogram
                ColourMap1 = this.ColourMap1,
                ColourMap2 = this.ColourMap2,
                BackgroundFilterCoeff = this.BackgroundFilterCoeff, // must be value <=1.0

                // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
                SampleRate = this.SampleRate,
                FrameWidth = this.FrameWidth,       // frame width from which spectrogram was derived. Assume no frame overlap.
                MinuteOffset = this.MinuteOffset.TotalMinutes,   // default is recording starts at zero minute of day i.e. midnight
                XaxisTicInterval = this.X_Axis_TicInterval.TotalMinutes,  // default is one minute spectra and hourly time lines
                YaxisTicInterval = this.Y_Axis_TicInterval                // default is 1000 Herz
            });
        } // WritConfigToYAML()

    } //LDSpectrogramConfig

}
