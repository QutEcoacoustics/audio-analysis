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
    using System.Drawing;
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using Newtonsoft.Json;
    using TowseyLibrary;
    using YamlDotNet.Serialization;
    using Zio;

    public interface IIndexPropertyReferenceConfiguration
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
                var indicesPropertiesConfig = Indices.IndexProperties.Find(this, this.ConfigPath);
                this.IndexPropertiesConfig = indicesPropertiesConfig.Path.ToOsPath();
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
        private static readonly Dictionary<string, Dictionary<string, IndexProperties>> CachedProperties = new Dictionary<string, Dictionary<string, IndexProperties>>();

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
            this.DefaultValue = default(double);
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

        public Image GetPlotImage(double[] array, List<GapsAndJoins> errors = null) => this.GetPlotImage(array, Color.White, errors);

        /// <summary>
        /// This method called from Indexdisplay.DrawImageOfSummaryIndices().
        /// It draws a single plot/track of one summary index.
        /// </summary>
        public Image GetPlotImage(double[] array, Color backgroundColour, List<GapsAndJoins> errors = null)
        {
            int dataLength = array.Length;
            string annotation = this.GetPlotAnnotation();
            double[] values = DataTools.NormaliseInZeroOne(array, this.NormMin, this.NormMax);

            int trackWidth = dataLength + IndexDisplay.TrackEndPanelWidth;
            int trackHeight = IndexDisplay.DefaultTrackHeight;

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(backgroundColour);

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
                    bmp.SetPixel(i, trackHeight - y - 1, barColor);
                }

                // draw upper boundary
                bmp.SetPixel(i, 0, Color.Gray);
            }

            var font = new Font("Arial", 9.0f, FontStyle.Regular);
            g.DrawString(annotation, font, Brushes.Black, new PointF(dataLength, 5));

            // now add in image patches for possible erroneous segments
            bool errorsExist = errors != null && errors.Count > 0;
            if (errorsExist)
            {
                bmp = (Bitmap)GapsAndJoins.DrawErrorSegments(bmp, errors, false);
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

        public static FileInfo Find(Config configuration, FileInfo originalConfigFile, bool allowDefault = false)
        {
            if (configuration == null)
            {
                return null;
            }

            return Find(configuration[AnalysisKeys.KeyIndexPropertiesConfig], originalConfigFile?.ToFileEntry(), allowDefault)?.ToFileInfo();
        }

        /// <summary>
        /// Locate and IndexPropertiesConfig.yml file from the IndexPropertiesConfig key in a config file.
        /// </summary>
        public static FileEntry Find(IIndexPropertyReferenceConfiguration configuration, string originalConfigpath, bool allowDefault = false)
        {
            if (configuration == null)
            {
                return null;
            }

            return Find(configuration.IndexPropertiesConfig, originalConfigpath.ToFileEntry());
        }

        /// <summary>
        /// Locate and IndexPropertiesConfig.yml file from the IndexPropertiesConfig key in a config file.
        /// </summary>
        public static FileEntry Find(IIndexPropertyReferenceConfiguration configuration, FileEntry originalConfigFile, bool allowDefault = false)
        {
            if (configuration == null)
            {
                return null;
            }

            return Find(configuration.IndexPropertiesConfig, originalConfigFile, allowDefault);
        }

        public static FileEntry Find(string relativePath, FileEntry originalConfigFile, bool allowDefault = false)
        {
            var found = ConfigFile.TryResolve(
                relativePath,
                new[] { originalConfigFile.Parent },
                out var configFile);

            return found ? configFile : null;
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
