using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{

    /// <summary>
    ///  Code for CUBEHELIX adapted from javascript code obtained from following website: https://www.mrao.cam.ac.uk/~dag/CUBEHELIX/
    /// </summary>
    public class ColourCubeHelix
    {
        public const string DEFAULT = "default";
        public const string GRAYSCALE = "grayscale";

        public class HSLColour 
        {
            public int Hue { get; set; }
            public double Sat { get; set; }
            public double Lit { get; set; }

            public HSLColour(int H, double S, double L)
            {
                Hue = H;
                Sat = S;
                Lit = L;
            }
        }

        public Color[] ColourPallette;

        public const int maxPalletteSize = 256;
        public int maxPalletteIndex = maxPalletteSize - 1;


        public ColourCubeHelix(string mode)
        {
            if (mode.Equals(ColourCubeHelix.DEFAULT))
            {
                HSLColour c1 = new HSLColour( 300, 0.5, 0.0);
                HSLColour c2 = new HSLColour(-240, 0.5, 1.0);
                this.SetDefaultCubeHelix(c1, c2);
            }
            else
                if (mode.Equals(ColourCubeHelix.GRAYSCALE))
                {
                    ColourPallette = ImageTools.GrayScale();
                }
                else
                {
                    LoggedConsole.WriteErrorLine("WARNING: {0} is UNKNOWN COLOUR PALLETTE!", mode);
                }
        }


        public Color GetColour(int colourID)
        {
            return ColourPallette[colourID];
        }

        /// <summary>
        /// This method assumes that the intensity lies in [0,1]
        /// </summary>
        /// <param name="intensity"></param>
        /// <returns></returns>
        public Color GetColour(double intensity)
        {
            int colourID = (int)Math.Floor(intensity * maxPalletteIndex);
            return ColourPallette[colourID];
        }


        /// <summary>
        /// Draws matrix without normalising the values in the matrix.
        /// Assumes some form of normalisation already done.
        /// </summary>
        /// <param name="matrix">the data</param>
        /// <param name="pathName"></param>
        public Image DrawMatrixWithoutNormalisation(double[,] matrix)
        {
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int colourID = (int)Math.Floor(matrix[r, c] * maxPalletteIndex);

                    if (colourID < 0) { colourID = 0; }
                    else
                    { if (colourID > maxPalletteIndex) colourID = maxPalletteIndex; }

                    bmp.SetPixel(c, r, this.ColourPallette[colourID]);
                }//end all columns
            }//end all rows
            return bmp;
        }


        public void SetDefaultCubeHelix(HSLColour hslc1, HSLColour hslc2)
        {
            var pallette = new Color[maxPalletteSize];
            int gamma = 1;

            var radians = Math.PI / 180;

            double ah = (hslc1.Hue + 120) * radians;
            double bh = (hslc2.Hue + 120) * radians - ah;
            double as_ = hslc1.Sat;
            double bs = hslc2.Sat - as_;
            double al = hslc1.Lit;
            double bl = hslc2.Lit - al;

      if (Double.IsNaN(bs)) 
      {
          bs = 0; 
          as_ = Double.IsNaN(as_) ? hslc2.Sat : as_;
      }

      if (Double.IsNaN(bh))
      {
          bh = 0;
          ah = Double.IsNaN(ah) ? hslc2.Hue : ah;
      }

      int t = 1;
      double h = ah + bh * t,
            l = Math.Pow(al + bl * t, gamma),
            a = (as_ + bs * t) * l * (1 - l);


      //return "#"
      //    + hex(l + a * (-0.14861 * Math.cos(h) + 1.78277 * Math.sin(h)))
      //    + hex(l + a * (-0.29227 * Math.cos(h) - 0.90649 * Math.sin(h)))
      //    + hex(l + a * (+1.97294 * Math.cos(h)));



  //function d3_interpolateCubehelix(γ) {
  //  return function(a, b) {
      //a = d3.hsl(a);
      //b = d3.hsl(b);

      //var ah = (a.h + 120) * radians,
      //    bh = (b.h + 120) * radians - ah,
      //    as = a.s,
      //    bs = b.s - as,
      //    al = a.l,
      //    bl = b.l - al;

      //if (isNaN(bs)) bs = 0, as = isNaN(as) ? b.s : as;
      //if (isNaN(bh)) bh = 0, ah = isNaN(ah) ? b.h : ah;

      //return function(double t) 
      // {
      //  var h = ah + bh * t,
      //      l = Math.pow(al + bl * t, γ),
      //      a = (as + bs * t) * l * (1 - l);


      //  return "#"
      //      + hex(l + a * (-0.14861 * Math.cos(h) + 1.78277 * Math.sin(h)))
      //      + hex(l + a * (-0.29227 * Math.cos(h) - 0.90649 * Math.sin(h)))
      //      + hex(l + a * (+1.97294 * Math.cos(h)));
  //    };
  //  };
  //}



            this.ColourPallette = pallette;
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

  function d3_interpolateCubehelix(γ) {
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
            l = Math.pow(al + bl * t, γ),
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
    return v < 0x10 ? "0" + s : s;
  }
  
         * 
         * 
         * 
         * **/


    }
}
