// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogramConfig.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
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
        public LdSpectrogramConfig()
        {
            // default values
            this.XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            this.ColorMap1 = SpectrogramConstants.RGBMap_BGN_POW_CVR;
            this.ColorMap2 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;     
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// these parameters manipulate the colour map and appearance of the false-colour spectrogram
        /// </summary>
        public string ColorMap1 { get; set; }

        /// <summary>
        /// these parameters manipulate the colour map and appearance of the false-colour spectrogram
        ///  pass two colour maps because interesting to draw a double image.
        /// </summary>
        public string ColorMap2 { get; set; }

        /// <summary>
        /// The default is one minute spectra i.e. 60 per hour
        /// But as of January 2015, this is not fixed. The user can adjust
        ///  the tic interval to be appropriate to the time scale of the spectrogram.
        /// </summary>
        public TimeSpan XAxisTicInterval { get; set; }

        /// <summary>
        /// In seconds, the horizontal spacing between vertical grid lines for the x-Axis
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="frameWidth"></param>
        /// <returns></returns>
        public double CalculateYAxisTickInterval(double sampleRate, double frameWidth)
        {
             // convert 1000 Hz to a freq bin interval.
                double freqBinWidth = sampleRate / (double)frameWidth;
                return (int)Math.Round(this.yAxisTicInterval / freqBinWidth);
        }

        /// <summary>
        /// In hertz, the vertical spacing between horizontal grid lines for the y-Axis
        /// </summary>
        public int YAxisTicInterval
        {
            get
            {
               return this.yAxisTicInterval;
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


        public string[] GetKeys()
        {
            var keys = new List<string>(); 
            if((this.ColorMap1 != null) && (this.ColorMap1.Length == 11))
            {
                string[] codes = this.ColorMap1.Split('-');
                foreach (string str in codes) keys.Add(str);
                codes = this.ColorMap2.Split('-');
                foreach (string str in codes)
                {
                    if(! keys.Contains(str)) keys.Add(str);
                }
            }
            return keys.ToArray();
        }

        #endregion
    }
}