// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogramConfig.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;

    public enum FreqScaleType
    {
        Linear,
        Mel,
        Linear62Octaves31Nyquist11025,
        Linear125Octaves30Nyquist11025,
        Octaves24Nyquist32000,
        Linear125Octaves28Nyquist32000,
        Linear125Octaves7Tones28Nyquist32000
    }


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
        /// This parameter sets the type of freq scale. Obvious possibilities are "linear", "octave" and "mel".
        /// Linear is the default. Mel option is not currently functional.
        /// There are two octave options: for sr=22050 and for sr=64000 
        /// </summary>
        public string FreqScale { get; set; }

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
        /// Determines colour intensity of the lower index values relative to the higher index values. Good value is 2.0
        /// </summary>
        public double? ColourGain { get; set; }

        /// <summary>
        /// Value must be less than 1.0. Good value is 0.75
        /// </summary>
        public double? ColourFilter { get; set; }

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
            return GetKeys(this.ColorMap1, this.ColorMap2);
        }





        /// <summary>
        /// NOTE: As of August 2015, we are using EVN (event count) in both spectrograms because 
        /// CVR (cover) is too highly correlated with POW.
        /// </summary>
        /// <returns></returns>
        public static LdSpectrogramConfig GetDefaultConfig()
        {
            var ldSpectrogramConfig = new LdSpectrogramConfig
            {
                YAxisTicInterval = 1000,
                XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_EVN,
                //ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_CVR,
            };
            return ldSpectrogramConfig;
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static LdSpectrogramConfig GetDefaultConfig(string colourMap1, string colourMap2)
        {
            var ldSpectrogramConfig = new LdSpectrogramConfig
            {
                YAxisTicInterval = 1000,
                XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                ColorMap1 = colourMap1,
                ColorMap2 = colourMap2,
            };
            return ldSpectrogramConfig;
        }



        public static string[] GetKeys(string colorMap1, string colorMap2)
        {
            var keys = new List<string>(); 
            if((colorMap1 != null) && (colorMap1.Length == 11))
            {
                string[] codes = colorMap1.Split('-');
                foreach (string str in codes) keys.Add(str);
                codes = colorMap2.Split('-');
                foreach (string str in codes)
                {
                    if(! keys.Contains(str)) keys.Add(str);
                }
            }
            return keys.ToArray();
        }

        public static string[] GetKeys(string colorMap)
        {
            string[] keys = null;
            if ((colorMap != null) && (colorMap.Length == 11))
            {
                keys = colorMap.Split('-');
            }
            return keys;
        }

        #endregion
    }
}