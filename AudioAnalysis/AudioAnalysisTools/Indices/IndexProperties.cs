// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexProperties.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using TowseyLibrary;

    using YamlDotNet.Dynamic;
    using YamlDotNet.Serialization;

    /// <summary>
    /// This class stores the properties of a particular index.
    /// THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX - the value is stored in class IndexValues.
    /// This class stores default values, normalisation bounds and provides methods for the correct display of a SUMMARY INDEX in a tracks image.
    /// Display of SPECTRAL INDICES is handled in the class LDSpectrogramRGB.
    /// </summary>
    public class IndexProperties
    {

        public string Key { get; set; }

        public string Name { get; set; }

        public string DataType
        {
            get
            {
                return this.dataType;
            }

            set
            {
                this.dataType = value;
                this.UpdateTypedDefault();
            }
        }

        [YamlIgnore]
        public bool IsSpectralIndex
        {
            get
            {
                // TODO: this information should really be encoded rather than inferred
                return this.DataType == "double[]";
            }
        }

        public double DefaultValue
        {
            get
            {
                return this.defaultValue;
            }

            set
            {
                this.defaultValue = value;
                this.UpdateTypedDefault();
            }
        }

        [YamlIgnore]
        public object DefaultValueCasted { get; private set; }

        [YamlIgnore]
        public int Order { get; set; }

        public string ProjectID { get; set; }

        public string Comment { get; set; }

        // for display purposes only
        public bool DoDisplay { get; set; }

        public double NormMin { get; set; }
        public bool CalculateNormMin { get; set; }

        public double NormMax { get; set; }
        public bool CalculateNormMax { get; set; }

        public string Units { get; set; }

        // use these when calculated combination index.
        public bool IncludeInComboIndex { get; set; }

        public double ComboWeight { get; set; }

        /// <summary>
        /// constructor sets default values
        /// </summary>
        public IndexProperties()
        {
            // TODO: why not initialise these to null, the proper empty value?
            this.Key = "NOT SET";
            this.Name = String.Empty;
            this.DataType = "double";
            this.DefaultValue = default(double);
            this.ProjectID = "NOT SET";
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
        /// <returns></returns>
        public string GetPlotAnnotation()
        {
            if (this.Units == String.Empty)
            {
                return String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "%")
            {
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "dB")
            {
                return String.Format(" {0} ({1:f0} .. {2:f0} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "ms")
            {
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            if (this.Units == "s")
            {
                return String.Format(" {0} ({1:f1} .. {2:f1}{3})", this.Name, this.NormMin, this.NormMax, this.Units);
            }

            return String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
        }

        /// <summary>
        /// For writing this method:
        ///    See CLASS: DrawSummaryIndices
        ///       METHOD: Bitmap ConstructVisualIndexImage(DataTable dt, string title, int timeScale, double[] order, bool doNormalise)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public Image GetPlotImage(double[] array, List<ErroneousIndexSegments> errors = null)
        {
            int dataLength = array.Length;
            string annotation = this.GetPlotAnnotation();
            //double[] values = this.NormaliseIndexValues(array);
            double[] values = DataTools.NormaliseInZeroOne(array, this.NormMin, this.NormMax);


            int trackWidth = dataLength + IndexDisplay.TrackEndPanelWidth;
            int trackHeight = IndexDisplay.DefaultTrackHeight;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(grayScale[240]);

            Color barColor;
            int barHeight = 0;

            // for pixels in the line
            for (int i = 0; i < dataLength; i++)
            {
                double value = values[i];

                if (Double.IsNaN(value))
                {
                    // expect normalised data
                    barHeight = trackHeight;
                    barColor = Color.Gray;
                }
                else
                {
                    if (value > 1.0) value = 1.0; // expect normalised data
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
            if ((errors != null) && (errors.Count > 0))
            { 
                bool verticalText = false;
                foreach (ErroneousIndexSegments errorSegment in errors)
                {
                    var errorBmp = errorSegment.DrawErrorPatch(trackHeight - 2, verticalText);
                    g.DrawImage(errorBmp, errorSegment.StartPosition, 1);
                }
            }
            return bmp;
        }




        private static readonly Dictionary<string, Dictionary<string, IndexProperties>> CachedProperties = new Dictionary<string, Dictionary<string, IndexProperties>>();

        private string dataType;

        private double defaultValue;

        /// <summary>
        /// Returns a cached set of configuration properties.
        /// WARNING CACHED!
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public static Dictionary<string, IndexProperties> GetIndexProperties(FileInfo configFile)
        {
            // AT: the effects of this method have been significantly altered
            // a) caching introduced - unknown effects for parallelism and dodgy file rewriting stuff
            // b) static deserialization utilized (instead of dynamic)
            lock (CachedProperties)
            {
                Dictionary<string, IndexProperties> props;
                if (CachedProperties.TryGetValue(configFile.FullName, out props))
                {
                    return props;
                }
                else
                {
                    var deserialized = Yaml.Deserialise<Dictionary<string, IndexProperties>>(configFile);

                    int i = 0;
                    foreach (var kvp in deserialized)
                    {
                        // assign the key to the object for consistency
                        kvp.Value.Key = kvp.Key;

                        // HACK: infer order of properties for visualization based on order of for-each
                        kvp.Value.Order = i;
                        i++;
                    }

                    CachedProperties.Add(configFile.FullName, deserialized);
                    return deserialized;
                }
            }

            /*
            dynamic configuration = Yaml.Deserialise(configFile);

            var dict = new Dictionary<string, IndexProperties>();

            foreach (dynamic config in configuration.Children)
            {
                var ip = new IndexProperties();          
                ip.Key = config.Key;
                ip.Name = config.Name;
                string datatype = config.DataType;
                ip.DataType = typeof(double);
                if (datatype == "TimeSpan")
                {
                    ip.DataType = typeof(TimeSpan);
                }
                else if (datatype == "double[]")
                {
                    ip.DataType = typeof(double[]);
                }
                else if (datatype == "int")
                {
                    ip.DataType = typeof(int);
                }

                ip.Comment = config.Comment;
                ip.DefaultValue = (double)config.DefaultValue;
                ip.ProjectID = config.ProjectID;

                // for display purposes only
                ip.DoDisplay = (bool?)config.DoDisplay ?? false;

                ip.NormMin = (double)config.NormMin;
                ip.NormMax = (double)config.NormMax;
                ip.Units = config.Units;

                // use these when calculated combination index.
                ip.IncludeInComboIndex = (bool?)config.includeInComboIndex ?? false;

                ip.ComboWeight = (double)config.comboWeight;

                dict.Add(ip.Key, ip);
            }

            return dict;*/
        }

        public static FileInfo Find(dynamic configuration, FileInfo originalConfigFile)
        {
            if (configuration == null)
            {
                return null;
            }

            return Find((string)configuration[AnalysisKeys.KeyIndexPropertiesConfig], originalConfigFile);
        }

        public static FileInfo Find(IIndexPropertyReferenceConfiguration configuration, FileInfo originalConfigFile)
        {
            if (configuration == null)
            {
                return null;
            }

            return Find(configuration.IndexPropertiesConfig, originalConfigFile);
        }

        public static FileInfo Find(string relativePath, FileInfo originalConfigFile)
        {
            FileInfo configFile;
            var found = ConfigFile.TryResolveConfigFile(
                relativePath,
                new[] { originalConfigFile.Directory },
                out configFile);

            return found ? configFile : null;
        }
    }

    public interface IIndexPropertyReferenceConfiguration
    {
        string IndexPropertiesConfig { get; set; }
    }
}
