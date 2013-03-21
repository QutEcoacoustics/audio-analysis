// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SammonsProjectionPostProcess.cs" company="MQUTeR">
//   Originally taken from http://www.codeproject.com/Articles/43123/Sammon-Projection
//   By Günther M. FOIDL, 20 Oct 2009
// </copyright>
// <summary>
//   Defines the Helper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace gfoidl.SammonProjection
{
    using System;
    using System.Drawing;
    using System.Linq;

    /// <summary>
    /// Provides mehtods for creating images for the Sammon's projection.
    /// </summary>
    public class SammonsProjectionPostProcess
    {
        #region Felder
        private SammonsProjection _sammon;
        #endregion
        //---------------------------------------------------------------------
        #region Eigenschaften
        private Color _backGroundColor = Color.Transparent;
        /// <summary>
        /// Backgroundcolor.
        /// </summary>
        public Color BackGroundColor
        {
            get { return _backGroundColor; }
            set { _backGroundColor = value; }
        }
        //---------------------------------------------------------------------
        private int _pointSize = 6;
        /// <summary>
        /// The size of the displayed points.
        /// </summary>
        public int PointSize
        {
            get { return _pointSize; }
            set { _pointSize = value; }
        }
        //---------------------------------------------------------------------
        private int _fontSize = 10;
        /// <summary>
        /// The size of the font.
        /// </summary>
        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }
        #endregion
        //---------------------------------------------------------------------
        #region Konstruktor
        /// <summary>
        /// Creates an instance for the visualization.
        /// </summary>
        /// <param name="sammon">Sammon's projection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sammon"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Only 2-dimensional projecitons can be postprocessed.
        /// </exception>
        public SammonsProjectionPostProcess(SammonsProjection sammon)
        {
            if (sammon == null)
                throw new ArgumentNullException("sammon");

            if (sammon.OutputDimension != 2)
                throw new ArgumentException();
            //-----------------------------------------------------------------            
            _sammon = sammon;
        }
        #endregion
        //---------------------------------------------------------------------
        #region Methoden
        /// <summary>
        /// Creates an image from the Sammon's projection.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>Image from the Sammon's projection.</returns>
        public Bitmap CreateImage(int width, int height)
        {
            return CreateImage(width, height, null, null);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Creates an image from the Sammon's projection with labeled points.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="labels">The labels for the points.</param>
        /// <returns>Image from the Sammon's projection.</returns>
        public Bitmap CreateImage(int width, int height, string[] labels)
        {
            return CreateImage(width, height, labels);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Creates an image from the Sammon's projection with colored points.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="colors">The colors for the points.</param>
        /// <returns>Image from the Sammon's projection.</returns>
        public Bitmap CreateImage(int width, int height, Color[] colors)
        {
            return CreateImage(width, height, null, colors);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Creates an image from the Sammon's projection with labeled and 
        /// colored points.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="labels">The labels for the points.</param>
        /// <param name="colors">The colors for the points.</param>
        /// <returns>Image from the Sammon's projection.</returns>
        /// <exception cref="ArgumentException">
        /// The dimension of <paramref name="labels"/> or <paramref name="colors"/>
        /// doesn't match the number of points.
        /// </exception>
        public Bitmap CreateImage(
            int width,
            int height,
            string[] labels,
            Color[] colors)
        {
            if (labels != null && labels.Length != _sammon.Count)
                throw new ArgumentException();

            if (colors != null && colors.Length != _sammon.Count)
                throw new ArgumentException();
            //-----------------------------------------------------------------
            Bitmap bmp = new Bitmap(width, height);

            double minX = _sammon.Projection.Min(p => p[0]);
            double maxX = _sammon.Projection.Max(p => p[0]);
            double minY = _sammon.Projection.Min(p => p[1]);
            double maxY = _sammon.Projection.Max(p => p[1]);

            double ratioX = (width - 20) / (maxX - minX);
            double ratioY = (height - 20) / (maxY - minY);

            Brush brush = new SolidBrush(Color.Black);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Font font = new Font("Arial", _fontSize))
            using (StringFormat format = new StringFormat())
            {
                g.Clear(_backGroundColor);

                format.Alignment = StringAlignment.Center;

                double[][] projection = _sammon.Projection;
                for (int i = 0; i < projection.Length; i++)
                {
                    double[] projectionI = projection[i];
                    double x = projectionI[0];
                    double y = projectionI[1];

                    x = (x - minX) * ratioX + 10;
                    y = (y - minY) * ratioY + 10;

                    if (colors != null)
                    {
                        brush.Dispose();
                        brush = new SolidBrush(colors[i]);
                    }

                    g.FillEllipse(
                        brush,
                        (float)(x - _pointSize / 2d),
                        (float)(y - _pointSize / 2d),
                        _pointSize,
                        _pointSize);

                    if (labels != null)
                    {
                        var size = g.MeasureString(labels[i], font);

                        g.DrawString(
                            labels[i],
                            font,
                            brush,
                            (float)(x - _pointSize / 2d),
                            (float)(y - _pointSize / 2d - size.Height),
                            format);
                    }
                }
            }

            brush.Dispose();

            return bmp;
        }
        #endregion
    }
}