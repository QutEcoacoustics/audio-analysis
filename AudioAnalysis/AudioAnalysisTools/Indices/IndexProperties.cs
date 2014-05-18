using Acoustics.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLibrary;


namespace AudioAnalysisTools
{
    /// <summary>
    /// This class stores the properties of a particular index.
    /// THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX - the value is stored in class IndexValues.
    /// This class stores default values, normalisation bounds and provides methods for the correct display of a SUMMARY INDEX in a tracks image.
    /// Display of SPECTRAL INDICES is handled in the class LDSpectrogramRGB.
    /// </summary>
    public class IndexProperties
    {

        public string Key {set; get; }
        public string Name { set; get; }
        public Type DataType { set; get; }
        public double DefaultValue {  set; get; }
        public string ProjectID { set; get; }
        public string Comment { set; get; }


        // for display purposes only
        public bool DoDisplay { set; get; }
        public double NormMin { set; get; }
        public double NormMax { set; get; }
        public string Units { set; get; }

        // use these when calculated combination index.
        public bool includeInComboIndex { set; get; }
        public double comboWeight { set; get; }

        /// <summary>
        /// constructor sets default values
        /// </summary>
        public IndexProperties()
        {
            Key = "NOT SET";
            Name = String.Empty;
            DataType = typeof(double);
            DefaultValue = 0.0;
            ProjectID = "NOT SET";
            Comment = "Relax - everything is OK";

            DoDisplay = true;
            NormMin = 0.0;
            NormMax = 1.0;
            Units = String.Empty;

            includeInComboIndex = false;
            comboWeight = 0.0;
        }

        public double NormaliseValue(double val)
        {
            double range = this.NormMax - this.NormMin;
            double norm = (val - this.NormMin) / range;
            if (norm > 1.0) norm = 1.0;
            else
                if (norm < 0.0) norm = 0.0;
            return norm;
        }

        public double[] NormaliseIndexValues(double[] val)
        {
            double range = this.NormMax - this.NormMin;
            double[] norms = new double[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                norms[i] = (val[i] - this.NormMin) / range;
                if (norms[i] > 1.0) norms[i] = 1.0;
                else
                    if (norms[i] < 0.0) norms[i] = 0.0;
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
                    M2return[r,c] = (M[r,c] - this.NormMin) / range;
                    if (M2return[r, c] > 1.0) M2return[r, c] = 1.0;
                    else
                        if (M2return[r, c] < 0.0) M2return[r, c] = 0.0;
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
                if (norms[i] > 1.0) norms[i] = 1.0;
                else
                    if (norms[i] < 0.0) norms[i] = 0.0;
            }
            return norms;
        }
        /// <summary>
        /// units for indices include: dB, ms, % and dimensionless
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPlotAnnotation()
        {
            if (this.Units == "") 
                return String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
            if (this.Units == "%")
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})",  this.Name, this.NormMin, this.NormMax, this.Units);
            if (this.Units == "dB")
                return String.Format(" {0} ({1:f0} .. {2:f0} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
            if (this.Units == "ms")
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})",  this.Name, this.NormMin, this.NormMax, this.Units);
            if (this.Units == "s")
                return String.Format(" {0} ({1:f1} .. {2:f1}{3})",  this.Name, this.NormMin, this.NormMax, this.Units);

            return     String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.NormMin, this.NormMax, this.Units);
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
            string annotation = GetPlotAnnotation();
            double[] values = this.NormaliseIndexValues(array);

            int trackWidth = dataLength + DrawSummaryIndices.TRACK_END_PANEL_WIDTH;
            int trackHeight = DrawSummaryIndices.DEFAULT_TRACK_HEIGHT;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(grayScale[240]);
            for (int i = 0; i < dataLength; i++) //for pixels in the line
            {
                double value = values[i];
                if (value > 1.0) value = 1.0; //expect normalised data
                int barHeight = (int)Math.Round(value * trackHeight);
                for (int y = 0; y < barHeight; y++) bmp.SetPixel(i, trackHeight - y - 1, Color.Black);
                bmp.SetPixel(i, 0, Color.Gray); //draw upper boundary
            }//end over all pixels

            int endWidth = trackWidth - dataLength;
            var font = new Font("Arial", 9.0f, FontStyle.Regular);
            g.FillRectangle(Brushes.Black, dataLength + 1, 0, endWidth, trackHeight);
            g.DrawString(annotation, font, Brushes.White, new PointF(dataLength + 5, 2));
            return bmp;
        } // GetPlotImage()



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
                if (datatype == "TimeSpan") ip.DataType = typeof(TimeSpan);
                else if (datatype == "double[]") ip.DataType = typeof(double[]);
                else if (datatype == "int") ip.DataType = typeof(int);
                ip.Comment = config.Comment;
                ip.DefaultValue = (double)config.DefaultValue;
                ip.ProjectID = config.ProjectID;

                // for display purposes only
                string doDisplay = config.DoDisplay;
                ip.DoDisplay = false;
                if ((doDisplay == "Yes") || (doDisplay == "true") || (doDisplay == "True")) ip.DoDisplay = true;
                ip.NormMin = (double)config.NormMin;
                ip.NormMax = (double)config.NormMax;
                ip.Units = config.Units;

                // use these when calculated combination index.
                string asComboIndex = config.includeInComboIndex;
                ip.includeInComboIndex = false;
                if ((asComboIndex == "Yes") || (asComboIndex == "true") || (asComboIndex == "True")) ip.includeInComboIndex = true;
                ip.comboWeight = (double)config.comboWeight;

                dict.Add(ip.Key, ip);
            }
            return dict;
        } // GetIndexProperties()


    } // IndexProperties
}
