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
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    using Acoustics.Shared;

    using TowseyLibrary;

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

        public Type DataType { get; set; }

        public double DefaultValue { get; set; }

        public string ProjectID { get; set; }

        public string Comment { get; set; }

        // for display purposes only
        public bool DoDisplay { get; set; }

        public double NormMin { get; set; }

        public double NormMax { get; set; }

        public string Units { get; set; }

        // use these when calculated combination index.
        public bool includeInComboIndex { get; set; }

        public double comboWeight { get; set; }

        /// <summary>
        /// constructor sets default values
        /// </summary>
        public IndexProperties()
        {
            this.Key = "NOT SET";
            this.Name = String.Empty;
            this.DataType = typeof(double);
            this.DefaultValue = 0.0;
            this.ProjectID = "NOT SET";
            this.Comment = "Relax - everything is OK";

            this.DoDisplay = true;
            this.NormMin = 0.0;
            this.NormMax = 1.0;
            this.Units = String.Empty;

            this.includeInComboIndex = false;
            this.comboWeight = 0.0;
        }

        public double NormaliseValue(double val)
        {
            double range = this.NormMax - this.NormMin;
            double norm = (val - this.NormMin) / range;
            if (norm > 1.0)
            {
                norm = 1.0;
            }
            else if (norm < 0.0)
            {
                norm = 0.0;
            }

            return norm;
        }

        public double[] NormaliseIndexValues(double[] val)
        {
            double range = this.NormMax - this.NormMin;
            double[] norms = new double[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                norms[i] = (val[i] - this.NormMin) / range;
                if (norms[i] > 1.0)
                {
                    norms[i] = 1.0;
                }
                else if (norms[i] < 0.0)
                {
                    norms[i] = 0.0;
                }
            }

            return norms;
        }

        public double[,] NormaliseIndexValues(double[,] M)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double range = this.NormMax - this.NormMin;
            double[,] M2return = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    M2return[r, c] = (M[r, c] - this.NormMin) / range;
                    if (M2return[r, c] > 1.0)
                    {
                        M2return[r, c] = 1.0;
                    }
                    else if (M2return[r, c] < 0.0)
                    {
                        M2return[r, c] = 0.0;
                    }
                }
            }

            return M2return;
        }

        public double[] NormaliseValues(int[] val)
        {
            double range = this.NormMax - this.NormMin;
            double[] norms = new double[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                norms[i] = (val[i] - this.NormMin) / range;
                if (norms[i] > 1.0)
                {
                    norms[i] = 1.0;
                }
                else if (norms[i] < 0.0)
                {
                    norms[i] = 0.0;
                }
            }

            return norms;
        }

        /// <summary>
        /// Units for indices include: dB, ms, % and dimensionless
        /// </summary>
        /// <returns></returns>
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
        ///    See CLASS: IndicesCsv2Display
        ///       METHOD: Bitmap ConstructVisualIndexImage(DataTable dt, string title, int timeScale, double[] order, bool doNormalise)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public Image GetPlotImage(double[] array)
        {
            int dataLength = array.Length;
            string annotation = this.GetPlotAnnotation();
            double[] values = this.NormaliseIndexValues(array);

            int trackWidth = dataLength + DrawSummaryIndices.TRACK_END_PANEL_WIDTH;
            int trackHeight = DrawSummaryIndices.DEFAULT_TRACK_HEIGHT;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(grayScale[240]);

            // for pixels in the line
            for (int i = 0; i < dataLength; i++) 
            {
                double value = values[i];
                if (value > 1.0)
                {
                    // expect normalised data
                    value = 1.0; 
                }

                int barHeight = (int)Math.Round(value * trackHeight);
                for (int y = 0; y < barHeight; y++)
                {
                    bmp.SetPixel(i, trackHeight - y - 1, Color.Black);
                }

                // draw upper boundary
                bmp.SetPixel(i, 0, Color.Gray);
            }

            // end over all pixels
            int endWidth = trackWidth - dataLength;
            var font = new Font("Arial", 9.0f, FontStyle.Regular);
            g.FillRectangle(Brushes.Black, dataLength + 1, 0, endWidth, trackHeight);
            g.DrawString(annotation, font, Brushes.White, new PointF(dataLength + 5, 2));
            return bmp;
        }





        public static Dictionary<string, IndexProperties> GetIndexProperties(FileInfo configFile)
        {
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
                ip.includeInComboIndex = (bool?)config.includeInComboIndex ?? false;

                ip.comboWeight = (double)config.comboWeight;

                dict.Add(ip.Key, ip);
            }

            return dict;
        }
    }
}
