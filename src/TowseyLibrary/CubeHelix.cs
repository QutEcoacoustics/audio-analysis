// <copyright file="CubeHelix.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.ColorSpaces;
    using SixLabors.ImageSharp.ColorSpaces.Conversion;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public class CubeHelix
    {
        private const double Radians = Math.PI / 180;

        private readonly Hsl colorA;
        private readonly Hsl colorB;
        private readonly double aHue;
        private readonly double bHue;
        private readonly double aSaturation;
        private readonly double bSaturation;
        private readonly double aLuminosity;
        private readonly double bLuminosity;

        public Color[] ColourPallette;

        public const int maxPalletteSize = 256;
        public int MaxPalletteIndex = maxPalletteSize - 1;
        private readonly ColorSpaceConverter converter;

        public CubeHelix(string mode)
        {
            if (mode.Equals(ColorCubeHelix.Default))
            {
                //Hsl colorARgb = new Hsl(300, 0.5, 0.0);
                //Hsl colorBRgb = new Hsl(-240, 0.5, 1.0);
                //Hsl colorARgb;
                //Hsl colorBRgb;

                //CubeHelix(colorARgb, colorBRgb);

                this.SetDefaultCubeHelix();

                //string path = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\ZoomImages\testImage.png";
                //TestImage(path);
            }
            else
                if (mode.Equals(ColorCubeHelix.Grayscale))
            {
                this.ColourPallette = ImageTools.GrayScale();
            }
            else
                    if (mode.Equals(ColorCubeHelix.RedScale))
            {
                this.SetRedScalePallette();
            }
            else
                        if (mode.Equals(ColorCubeHelix.CyanScale))
            {
                this.SetCyanScalePallette();
            }
            else
            {
                LoggedConsole.WriteErrorLine("WARNING: {0} is UNKNOWN COLOUR PALLETTE!", mode);
            }
        }

        public CubeHelix(Hsl colorA, Hsl colorB, double gamma = 1.0)
        {
            this.converter = new ColorSpaceConverter();
            this.Gamma = gamma;
            this.colorA = colorA;
            this.colorB = colorB;

            this.aHue = (this.colorA.H + 120) * Radians;
            this.bHue = ((this.colorB.H + 120) * Radians) - this.aHue;
            this.aSaturation = this.colorA.S;
            this.bSaturation = this.colorB.S - this.aSaturation;
            this.aLuminosity = this.colorA.L;
            this.bLuminosity = this.colorB.L - this.aLuminosity;

            if (double.IsNaN(this.bSaturation))
            {
                this.bSaturation = 0;

                if (double.IsNaN(this.aSaturation))
                {
                    this.aSaturation = this.colorB.S;
                }
            }

            if (double.IsNaN(this.bHue))
            {
                this.bHue = 0;

                if (double.IsNaN(this.aHue))
                {
                    this.aHue = this.colorB.H;
                }
            }
        }

        public double Gamma { get; private set; }

        public Rgb GetColor(double unitValue)
        {
            var hue = this.aHue + (this.bHue * unitValue);
            var luminosity = Math.Pow(this.aLuminosity + (this.bLuminosity * unitValue), this.Gamma);
            var amplitude = (this.aSaturation + (this.bSaturation * unitValue)) * luminosity * (1 - luminosity);
            var cosh = Math.Cos(hue);
            var sinh = Math.Sin(hue);

            return new Rgb(
                r: (float)(luminosity + (amplitude * ((-0.14861 * cosh) + (1.78277 * sinh)))),
                g: (float)(luminosity + (amplitude * ((-0.29227 * cosh) - (0.90649 * sinh)))),
                b: (float)(luminosity + (amplitude * (+1.97294 * cosh))));
        }

        public void SetDefaultCubeHelix()
        {
            int maxPalletteIndex = maxPalletteSize - 1;
            var pallette = new Color[maxPalletteSize];
            for (int c = 0; c < maxPalletteSize; c++)
            {
                double value = c / (double)maxPalletteIndex;
                Rgb rgbColour = this.GetColor(value);
                pallette[c] = new Color((Rgb24)rgbColour);
            }

            this.ColourPallette = pallette;
        }

        /// <summary>
        /// used for drawing the background noise in zooming spectrograms.
        /// </summary>
        public void SetRedScalePallette()
        {
            int maxPalletteIndex = maxPalletteSize - 1;
            var pallette = new Color[maxPalletteSize];
            for (int c = 0; c < maxPalletteSize; c++)
            {
                double value = c / (double)maxPalletteIndex;
                int R = (int)Math.Floor(255.0 * Math.Pow(value, 0.75));

                //int G = 0;
                int B = 0;
                int G = (int)Math.Floor(255.0 * value * value * value) / 3;

                //int B = R / 2;
                pallette[c] = Color.FromRgb((byte)R, (byte)G, (byte)B);
            }

            this.ColourPallette = pallette;
        }

        /// <summary>
        /// used for drawing the background noise in zooming spectrograms.
        /// </summary>
        public void SetCyanScalePallette()
        {
            int maxPalletteIndex = maxPalletteSize - 1;
            var pallette = new Color[maxPalletteSize];
            for (int c = 0; c < maxPalletteSize; c++)
            {
                double value = c / (double)maxPalletteIndex;

                //int R = 0;
                int R = (int)Math.Floor(255.0 * value * value * value * value);
                int G = (int)Math.Floor(255.0 * Math.Pow(value, 0.75));

                //int G = 0;
                int B = (int)Math.Floor(255.0 * value * value);
                pallette[c] = Color.FromRgb((byte)R, (byte)G, (byte)B);
            }

            this.ColourPallette = pallette;
        }

        /// <summary>
        /// Draws matrix without normalising the values in the matrix.
        /// Assumes some form of normalisation already done.
        /// </summary>
        public int GetColorID(double value)
        {
            int colourID = (int)Math.Floor(value * this.MaxPalletteIndex);

            if (colourID < 0)
            {
                return 0;
            }

            if (colourID > this.MaxPalletteIndex)
            {
                return this.MaxPalletteIndex;
            }

            return colourID;
        }

        /// <summary>
        /// Draws matrix without normalising the values in the matrix.
        /// Assumes some form of normalisation already done.
        /// </summary>
        public Color GetColorFromPallette(double value)
        {
            int colourID = (int)Math.Floor(value * this.MaxPalletteIndex);

            if (colourID < 0)
            {
                colourID = 0;
            }
            else
            {
                if (colourID > this.MaxPalletteIndex)
                {
                    colourID = this.MaxPalletteIndex;
                }
            }

            return this.ColourPallette[colourID];
        }

        /// <summary>
        /// Draws matrix without normalising the values in the matrix.
        /// Assumes some form of normalisation already done.
        /// </summary>
        public Color GetColorFromPallette(int colourID)
        {
            return this.ColourPallette[colourID];
        }

        public void TestImage(string path)
        {
            int width = maxPalletteSize;
            int height = 100;
            var image = new Image<Rgb24>(width, height);
            image.Mutate(g =>
            {
                for (int c = 0; c < maxPalletteSize; c++)
                {
                    var pen = new Pen(this.ColourPallette[c], 1);
                    g.DrawLine(pen, c, 0, c, height - 1);
                }
            });

            image.Save(path);
        }

        /// <summary>
        /// Draws matrix without normalising the values in the matrix.
        /// Assumes some form of normalisation already done.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public Image<Rgb24> DrawMatrixWithoutNormalisation(double[,] matrix)
        {
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number

            var image = new Image<Rgb24>(cols, rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int colourID = (int)Math.Floor(matrix[r, c] * this.MaxPalletteIndex);

                    if (colourID < 0)
                    {
                        colourID = 0;
                    }
                    else
                    {
                        if (colourID > this.MaxPalletteIndex)
                        {
                            colourID = this.MaxPalletteIndex;
                        }
                    }

                    image[c, r] = this.ColourPallette[colourID];
                }//end all columns
            }//end all rows

            return image;
        }

        /**
         * Obtained from following website.
https://www.mrao.cam.ac.uk/~dag/CUBEHELIX/

cubehelix.js#

(function() {
  var radians = Math.PI / 180;

  d3.scale.cubehelix = function() {
    return d3.scale.linear()
        .range([d3.hsl(300, .5, 0), d3.hsl(-240, .5, 1)])
        .interpolate(d3.interpolateCubehelix);
  };

  d3.interpolateCubehelix = d3_interpolateCubehelix(1);
  d3.interpolateCubehelix.gamma = d3_interpolateCubehelix;

  function d3_interpolateCubehelix(gamma) {
    return function(a, b) {
      a = d3.hsl(a);
      b = d3.hsl(b);

      var ah = (a.h + 120) * radians,
          bh = (b.h + 120) * radians - ah,
          as = a.s,
          bs = b.s - as,
          al = a.l,
          bl = b.l - al;

      if (isNaN(bs)) bs = 0, as = isNaN(as) ? b.s : as;
      if (isNaN(bh)) bh = 0, ah = isNaN(ah) ? b.h : ah;

      return function(t) {
        var h = ah + bh * t,
            l = Math.pow(al + bl * t, gamma),
            a = (as + bs * t) * l * (1 - l);
        return "#"
            + hex(l + a * (-0.14861 * Math.cos(h) + 1.78277 * Math.sin(h)))
            + hex(l + a * (-0.29227 * Math.cos(h) - 0.90649 * Math.sin(h)))
            + hex(l + a * (+1.97294 * Math.cos(h)));
      };
    };
  }

  function hex(v) {
    var s = (v = v <= 0 ? 0 : v >= 1 ? 255 : v * 255 | 0).toString(16);
    return v. < 0x10 ? "0" + s : s;
  }

         *
         *
         *
         * **/
        public static void DrawTestImage()
        {
            //Hsl colorARgb = new Hsl(300, 0.5, 0.0);
            //Hsl colorBRgb = new Hsl(-240, 0.5, 1.0);
            Hsl colorARgb = new Hsl(300, 0.5f, 0.0f);

            Hsl colorBRgb = new Hsl(-240, 0.5f, 1.0f);

            var cch = new CubeHelix(colorARgb, colorBRgb);
            cch.SetDefaultCubeHelix();
            string path = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\ZoomImages\testImage.png";
            cch.TestImage(path);
        }

        /// <summary>
        /// This HSL values in this method have been set specially for use with the high-resolution zooming spectrograms.
        /// There are limits to the values that can be used.
        /// The purpose for chaning the default values was to increase the colour saturation.
        /// </summary>
        public static CubeHelix GetCubeHelix()
        {
            //Hsl colorARgb = new Hsl(300, 0.5, 0.0); // DEFAULT - used prior to 26 June 2015
            //Hsl colorBRgb = new Hsl(-240, 0.5, 1.0); // DEFAULT - used prior to 26 June 2015
            Hsl colorARgb = new Hsl(300, 0.85f, 0.0f);

            Hsl colorBRgb = new Hsl(-240, 0.5f, 1.0f);

            var cch = new CubeHelix(colorARgb, colorBRgb);
            cch.SetDefaultCubeHelix();
            return cch;
        }
    }
}