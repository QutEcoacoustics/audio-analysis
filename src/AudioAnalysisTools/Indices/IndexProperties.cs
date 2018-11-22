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
            this.Loaded += config =>
                {
                var indicesPropertiesConfig = Indices.IndexProperties.Find(this, this.ConfigPath);
                this.IndexPropertiesConfig = indicesPropertiesConfig.Path.ToOsPath();
                this.IndexProperties = ConfigFile.Deserialize<IndexPropertiesCollection>(this.IndexPropertiesConfig);
            };
        }

        public string IndexPropertiesConfig { get; set; }

        public IndexPropertiesCollection IndexProperties { get; private set; }
    }

    public class IndexPropertiesCollection : Dictionary<string, IndexProperties>, IConfig
    {
        public IndexPropertiesCollection()
        {
            this.Loaded += (config) =>
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
            };
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
        /// constructor sets default values
        /// </summary>
        public IndexProperties()
        {
            // TODO: why not initialise these to null, the proper empty value?
            this.Key = "NOT SET";
            this.Name = string.Empty;
            this.DataType = "double";
            this.DefaultValue = default(double);
            this.ProjectId = "NOT SET";
            this.Comment = "Relax - everything is OK";
            this.DoDisplay = true;
            this.NormMin = 0.0;
            this.NormMax = 1.0;
            this.CalculateNormMin = false;
            this.CalculateNormMax = false;
            this.Units = string.Empty;
            this.IncludeInComboIndex = false;
            this.ComboWeight = 0.0;
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

        public bool CalculateNormMin { get; set; }

        public double NormMax { get; set; }

        public bool CalculateNormMax { get; set; }

        // ignored because we don't want to dump this info in ConfigFile log
        [JsonIgnore]
        public string Units { get; set; }

        // use these when calculated combination index.
        public bool IncludeInComboIndex { get; set; }

        public double ComboWeight { get; set; }

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

        public double NormaliseValue(double value)
        {
            return DataTools.NormaliseInZeroOne(value, this.NormMin, this.NormMax);
        }

       //public double[,] NormaliseIndexValues(double[,] M)
        //{
        //    return MatrixTools.NormaliseInZeroOne(M, this.NormMin, this.NormMax);
        //}

        /// <summary>
        /// Units for indices include: dB, ms, % and dimensionless
        /// </summary>
        public string GetPlotAnnotation()
        {
            if (this.Units == string.Empty)
            {
                return string.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "%")
            {
                return string.Format(" {0} ({1:f0} .. {2:f0}{3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "dB")
            {
                return string.Format(" {0} ({1:f0} .. {2:f0} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "ms")
            {
                return string.Format(" {0} ({1:f0} .. {2:f0}{3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "s")
            {
                return string.Format(" {0} ({1:f1} .. {2:f1}{3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            return string.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
        }

        /// <summary>
        /// For writing this method:
        ///    See CLASS: DrawSummaryIndices
        ///       METHOD: Bitmap ConstructVisualIndexImage(DataTable dt, string title, int timeScale, double[] order, bool doNormalise)
        /// </summary>
        public Image GetPlotImage(double[] array, List<GapsAndJoins> errors = null)
        {
            int dataLength = array.Length;
            string annotation = this.GetPlotAnnotation();
            double[] values = DataTools.NormaliseInZeroOne(array, this.NormMin, this.NormMax);

            int trackWidth = dataLength + IndexDisplay.TrackEndPanelWidth;
            int trackHeight = IndexDisplay.DefaultTrackHeight;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(grayScale[240]);

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

            // end over all pixels
            int endWidth = trackWidth - dataLength;
            var font = new Font("Arial", 9.0f, FontStyle.Regular);
            g.FillRectangle(Brushes.Black, dataLength + 1, 0, endWidth, trackHeight);
            g.DrawString(annotation, font, Brushes.White, new PointF(dataLength + 5, 2));

            // now add in image patches for possible erroneous index segments
            if (errors != null && errors.Count > 0)
            {
                foreach (GapsAndJoins errorSegment in errors)
                {
                    var errorBmp = errorSegment.DrawErrorPatch(trackHeight - 2, textInVerticalOrientation: false);
                    if (errorBmp != null)
                    {
                        g.DrawImage(errorBmp, errorSegment.StartPosition, 1);
                    }
                }
            }

            return bmp;
        }

        /// <summary>
        /// Returns a cached set of configuration properties.
        /// WARNING CACHED!
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
    }
}
