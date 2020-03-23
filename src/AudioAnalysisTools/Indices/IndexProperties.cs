// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexProperties.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// <summary>
//   This class stores the properties of a particular index.
//   THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX - the value is stored in class IndexValues.
//   This class stores default values, normalisation bounds and provides methods for the correct display of a SUMMARY INDEX in a tracks image.
//   Display of SPECTRAL INDICES is handled in the class LDSpectrogramRGB.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.ImageSharp;
    using AnalysisBase;
    using Newtonsoft.Json;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using YamlDotNet.Serialization;

    public interface IIndexPropertyReferenceConfiguration : IConfig
    {
        string IndexPropertiesConfig { get; set; }

        IndexPropertiesCollection IndexProperties { get; }
    }

    public abstract class AnalyzerConfigIndexProperties : AnalyzerConfig, IIndexPropertyReferenceConfiguration
    {
        protected AnalyzerConfigIndexProperties()
        {
            void OnLoaded(IConfig config)
            {
                var indicesPropertiesConfig = Indices.IndexProperties.Find(this);
                this.IndexPropertiesConfig = indicesPropertiesConfig.FullName;
                this.IndexProperties = ConfigFile.Deserialize<IndexPropertiesCollection>(this.IndexPropertiesConfig);
            }

            this.Loaded += OnLoaded;
        }

        public string IndexPropertiesConfig { get; set; }

        public IndexPropertiesCollection IndexProperties { get; private set; }
    }

    public class IndexPropertiesCollection : Dictionary<string, IndexProperties>, IConfig
    {
        static IndexPropertiesCollection()
        {
            ConfigFile.Defaults.Add(typeof(IndexPropertiesCollection), "IndexPropertiesConfig.yml");
        }

        public IndexPropertiesCollection()
        {
            void OnLoaded(IConfig config)
            {
                int i = 0;
                foreach (var kvp in this)
                {
                    // assign the key to the object for consistency
                    kvp.Value.Key = kvp.Key;

                    // HACK: infer order of properties for visualization based on order of for-each
                    kvp.Value.Order = i;
                    i++;
                }
            }

            this.Loaded += OnLoaded;
        }

        public event Action<IConfig> Loaded;

        public string ConfigPath { get; set; }

        void IConfig.InvokeLoaded()
        {
            this.Loaded?.Invoke(this);
        }
    }

    /// <summary>
    /// This class stores the properties of a particular index.
    /// THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX - the value is stored in class IndexValues.
    /// This class stores default values, normalisation bounds and provides methods for the correct display of a SUMMARY INDEX in a tracks image.
    /// Display of SPECTRAL INDICES is handled in the class LDSpectrogramRGB.
    /// </summary>
    public class IndexProperties
    {
        private string dataType;

        private double defaultValue;

        static IndexProperties()
        {
            ConfigFile.Defaults.Add(typeof(Dictionary<string, IndexProperties>), "IndexPropertiesConfig.yml");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexProperties"/> class.
        /// constructor sets default values.
        /// </summary>
        public IndexProperties()
        {
            this.Key = "NOT SET";
            this.Name = string.Empty;
            this.DataType = "double";
            this.DefaultValue = default;
            this.ProjectId = "NOT SET";
            this.Comment = "Relax - everything is OK";
            this.DoDisplay = true;
            this.NormMin = 0.0;
            this.NormMax = 1.0;
            this.CalculateNormBounds = false;
            this.Units = string.Empty;
        }

        // ignored because we don't want to dump this info in ConfigFile log
        [JsonIgnore]
        public string Key { get; set; }

        // ignored because we don't want to dump this info in ConfigFile log
        [JsonIgnore]
        public string Name { get; set; }

        public string DataType
        {
            get => this.dataType;
            set
            {
                this.dataType = value;
                this.UpdateTypedDefault();
            }
        }

        [YamlIgnore]
        [JsonIgnore]

        // TODO: this information should really be encoded rather than inferred
        public bool IsSpectralIndex => this.DataType == "double[]";

        public double DefaultValue
        {
            get => this.defaultValue;
            set
            {
                this.defaultValue = value;
                this.UpdateTypedDefault();
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public object DefaultValueCasted { get; private set; }

        [YamlIgnore]
        [JsonIgnore]
        public int Order { get; set; }

        // ignored because we don't want to dump this info in ConfigFile log
        [JsonIgnore]
        public string ProjectId { get; set; }

        // ignored because we don't want to dump this info in ConfigFile log
        [JsonIgnore]
        public string Comment { get; set; }

        // for display purposes only
        public bool DoDisplay { get; set; }

        public double NormMin { get; set; }

        public double NormMax { get; set; }

        public bool CalculateNormBounds { get; set; }

        // ignored because we don't want to dump this info in ConfigFile log
        [JsonIgnore]
        public string Units { get; set; }

        public double NormaliseValue(double value) => DataTools.NormaliseInZeroOne(value, this.NormMin, this.NormMax);

        /// <summary>
        /// Units for indices include: dB, ms, % and dimensionless.
        /// </summary>
        public string GetPlotAnnotation()
        {
            if (this.Units == string.Empty)
            {
                return $" {this.Name} ({this.NormMin:f2} .. {this.NormMax:f2} {this.Units})";
            }

            if (this.Units == "%")
            {
                return $" {this.Name} ({this.NormMin:f0} .. {this.NormMax:f0}{this.Units})";
            }

            if (this.Units == "dB")
            {
                return $" {this.Name} ({this.NormMin:f0} .. {this.NormMax:f0} {this.Units})";
            }

            if (this.Units == "ms")
            {
                return $" {this.Name} ({this.NormMin:f0} .. {this.NormMax:f0}{this.Units})";
            }

            if (this.Units == "s")
            {
                return $" {this.Name} ({this.NormMin:f1} .. {this.NormMax:f1}{this.Units})";
            }

            return $" {this.Name} ({this.NormMin:f2} .. {this.NormMax:f2} {this.Units})";
        }

        public Image<Rgb24> GetPlotImage(double[] array, List<GapsAndJoins> errors = null) => this.GetPlotImage(array, Color.White, errors);

        /// <summary>
        /// This method called from Indexdisplay.DrawImageOfSummaryIndices().
        /// It draws a single plot/track of one summary index.
        /// </summary>
        public Image<Rgb24> GetPlotImage(double[] array, Color backgroundColour, List<GapsAndJoins> errors = null)
        {
            int dataLength = array.Length;
            string annotation = this.GetPlotAnnotation();
            double[] values = DataTools.NormaliseInZeroOne(array, this.NormMin, this.NormMax);

            int trackWidth = dataLength + IndexDisplay.TrackEndPanelWidth;
            int trackHeight = IndexDisplay.DefaultTrackHeight;

            var bmp = Drawing.NewImage(trackWidth, trackHeight, backgroundColour);

            // for pixels in the line
            for (int i = 0; i < dataLength; i++)
            {
                double value = values[i];

                Color barColor;
                int barHeight;
                if (double.IsNaN(value))
                {
                    // expect normalised data
                    barHeight = trackHeight;
                    barColor = Color.Gray;
                }
                else
                {
                    if (value > 1.0)
                    {
                        value = 1.0; // expect normalised data
                    }

                    barHeight = (int)Math.Round(value * trackHeight);
                    barColor = Color.Black;
                }

                for (int y = 0; y < barHeight; y++)
                {
                    bmp[i, trackHeight - y - 1] = barColor;
                }

                // draw upper boundary
                bmp[i, 0] = Color.Gray;
            }

            bmp.Mutate(g =>
            {
                var font = Drawing.Arial9;
                g.DrawText(annotation, font, Color.Black, new PointF(dataLength, 5));
            });

            // now add in image patches for possible erroneous segments
            bool errorsExist = errors != null && errors.Count > 0;
            if (errorsExist)
            {
                bmp = (Image<Rgb24>)GapsAndJoins.DrawErrorSegments(bmp, errors, false);
            }

            return bmp;
        }

        /// <summary>
        /// Returns a cached set of configuration properties.
        /// WARNING CACHED!.
        /// </summary>
        public static IndexPropertiesCollection GetIndexProperties(FileInfo configFile)
        {
            return ConfigFile.Deserialize<IndexPropertiesCollection>(configFile);
        }

        /// <summary>
        /// Loads the IndexProperties config file, specified by the property <see cref="IIndexPropertyReferenceConfiguration.IndexPropertiesConfig"/>
        ///  defined on  <paramref name="configuration" /> which may be found
        /// relative to the original parent config file <see cref="IConfig.ConfigPath"/>.
        /// </summary>
        /// <remarks>
        /// This method is intended for use when a config file references another config file, in this case
        /// an IndexProperties config files, as a string property in the config file.
        /// </remarks>
        /// <exception cref="ConfigFileException">
        /// if <paramref name="configuration" /> is not null or empty, and does not exist.
        /// </exception>
        /// <param name="configuration">
        /// The configuration object that has the <see cref="IIndexPropertyReferenceConfiguration.IndexPropertiesConfig"/>
        /// key defined. If <see cref="IIndexPropertyReferenceConfiguration.IndexPropertiesConfig"/> is not rooted it
        /// is treated as relative to the parent config file <see cref="IConfig.ConfigPath"/>'s directory.
        /// </param>
        /// <returns>
        /// <code>null</code> if <paramref name="configuration" /> is null or empty, otherwise a reference to
        /// the desired config file.
        /// </returns>
        public static FileInfo Find(IIndexPropertyReferenceConfiguration configuration)
        {
            return Find(configuration?.IndexPropertiesConfig, configuration?.ConfigPath?.ToFileInfo());
        }

        /// <summary>
        /// Loads the IndexProperties config file, specified by <paramref name="relativePath" /> which may be found
        /// relative to the original config file <paramref name="originalConfigFile"/>.
        /// </summary>
        /// <remarks>
        /// This method is intended for use when a config file references another config file, in this case
        /// an IndexProperties config files, as a string property in the config file.
        /// </remarks>
        /// <exception cref="ConfigFileException">
        /// if <paramref name="relativePath" /> is not null or empty, and does not exist.
        /// </exception>
        /// <param name="relativePath">
        /// The path to the config file to find.
        /// If it is not rooted it is treated as relative to the <paramref name="originalConfigFile"/>'s directory.
        /// </param>
        /// <param name="originalConfigFile">
        /// The config file were the path to <paramref name="relativePath"/> was originally extracted.
        /// </param>
        /// <returns>
        /// <code>null</code> if <paramref name="relativePath" /> is null or empty, otherwise a reference to
        /// the desired config file.
        /// </returns>
        public static FileInfo Find(string relativePath, FileInfo originalConfigFile)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            return ConfigFile.Resolve(
                relativePath,
                originalConfigFile?.Directory?.AsArray());
        }

        private void UpdateTypedDefault()
        {
            if (this.DataType == "int")
            {
                this.DefaultValueCasted = (int)this.DefaultValue;
            }
            else if (this.DataType == "double" || this.dataType == "double[]")
            {
                this.DefaultValueCasted = this.DefaultValue;
            }
            else if (this.DataType == "TimeSpan")
            {
                this.DefaultValueCasted = TimeSpan.FromSeconds(this.DefaultValue);
            }
            else
            {
                throw new InvalidOperationException("Unknown data type");
            }
        }
    }
}
