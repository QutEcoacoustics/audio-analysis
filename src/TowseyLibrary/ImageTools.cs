// <copyright file="ImageTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.ImageSharp;
    using AForge.Imaging.Filters;
    using MathNet.Numerics.LinearAlgebra;
    using MoreLinq;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.ColorSpaces;
    using SixLabors.ImageSharp.ColorSpaces.Conversion;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public enum Kernal
    {
        LowPass,
        HighPass1,
        HighPass2,
        VerticalLine,
        HorizontalLine3,
        HorizontalLine5,
        DiagLine1,
        DiagLine2,
        GaussianBlur5,
        Grid2,
        Grid3,
        Grid4,
        Grid2Wave,
        Grid3Wave, //grid filters
        Laplace1,
        Laplace2,
        Laplace3,
        Laplace4,
        Erroneous,
        SobelX,
        SobelY,
    }

    public class ImageTools
    {

        // this is a list of predefined colors in the Color class.
        private static readonly string[] ColorNames = new[]
        {
            "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue", "BlueViolet",
            "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "Crimson", "Cyan",
            "DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGray", "DarkGreen", "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "DarkOrange",
            "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkTurquoise", "DarkViolet",
            "DeepPink", "DeepSkyBlue", "DimGray", "DodgerBlue", "Firebrick", "FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro",
            "GhostWhite", "Gold", "Goldenrod", "Gray", "Green", "GreenYellow", "Honeydew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki",
            "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan", "LightGoldenrodYellow",
            "LightGray", "LightGreen", "LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSteelBlue",
            "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta", "Maroon", "MediumAquamarine", "MediumBlue", "MediumOrchid",
            "MediumPurple", "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed",
            "MidnightBlue", "MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange",
            "OrangeRed", "Orchid", "PaleGoldenrod", "PaleGreen", "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru",
            "Pink", "Plum", "PowderBlue", "Purple", "Red", "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen",
            "SeaShell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGray", "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal",
            "Thistle", "Tomato", /*"Transparent",*/"Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen",
        };

        public static readonly Color[] Colors =
        {
            Color.AliceBlue, Color.AntiqueWhite, Color.Aqua, Color.Aquamarine, Color.Azure, Color.Beige, Color.Bisque, Color.Black,
            Color.BlanchedAlmond, Color.Blue, Color.BlueViolet, Color.Brown, Color.BurlyWood, Color.CadetBlue, Color.Chartreuse,
            Color.Chocolate, Color.Coral, Color.CornflowerBlue, Color.Cornsilk, Color.Crimson, Color.Cyan, Color.DarkBlue,
            Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta,
            Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen,
            Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue,
            Color.DimGray, Color.DodgerBlue, Color.Firebrick, Color.FloralWhite, Color.ForestGreen, Color.Fuchsia,
            Color.Gainsboro, Color.GhostWhite, Color.Gold, Color.Goldenrod, Color.Gray, Color.Green, Color.GreenYellow,
            Color.Honeydew, Color.HotPink, Color.IndianRed, Color.Indigo, Color.Ivory, Color.Khaki, Color.Lavender,
            Color.LavenderBlush, Color.LawnGreen, Color.LemonChiffon, Color.LightBlue, Color.LightCoral, Color.LightCyan,
            Color.LightGoldenrodYellow, Color.LightGray, Color.LightGreen, Color.LightPink, Color.LightSalmon,
            Color.LightSeaGreen, Color.LightSkyBlue, Color.LightSlateGray, Color.LightSteelBlue, Color.LightYellow, Color.Lime,
            Color.LimeGreen, Color.Linen, Color.Magenta, Color.Maroon, Color.MediumAquamarine, Color.MediumBlue,
            Color.MediumOrchid, Color.MediumPurple, Color.MediumSeaGreen, Color.MediumSlateBlue, Color.MediumSpringGreen,
            Color.MediumTurquoise, Color.MediumVioletRed, Color.MidnightBlue, Color.MintCream, Color.MistyRose, Color.Moccasin,
            Color.NavajoWhite, Color.Navy, Color.OldLace, Color.Olive, Color.OliveDrab, Color.Orange, Color.OrangeRed,
            Color.Orchid, Color.PaleGoldenrod, Color.PaleGreen, Color.PaleTurquoise, Color.PaleVioletRed, Color.PapayaWhip,
            Color.PeachPuff, Color.Peru, Color.Pink, Color.Plum, Color.PowderBlue, Color.Purple, Color.Red, Color.RosyBrown,
            Color.RoyalBlue, Color.SaddleBrown, Color.Salmon, Color.SandyBrown, Color.SeaGreen, Color.SeaShell, Color.Sienna,
            Color.Silver, Color.SkyBlue, Color.SlateBlue, Color.SlateGray, Color.Snow, Color.SpringGreen, Color.SteelBlue,
            Color.Tan, Color.Teal, Color.Thistle, Color.Tomato, /*Color.Transparent,*/ Color.Turquoise, Color.Violet, Color.Wheat,
            Color.White, Color.WhiteSmoke, Color.Yellow, Color.YellowGreen,
        };

        public static Color[] DarkColors =
        { /*Color.AliceBlue,*/ /*Color.Aqua, Color.Aquamarine, Color.Azure, Color.Bisque,*/
            Color.Black, Color.Blue, Color.BlueViolet, /*Color.Brown, Color.BurlyWood,*/ Color.CadetBlue, /*Color.Chartreuse,*/
            Color.Chocolate, /*Color.Coral,*/ /*Color.CornflowerBlue,*/ /*Color.Cornsilk,*/ Color.Crimson, Color.Cyan, Color.DarkBlue,
            Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta,
            Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen,
            Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue,
            Color.DimGray, Color.DodgerBlue, Color.Firebrick, Color.ForestGreen, Color.Fuchsia,
            Color.Gainsboro, Color.Gold, Color.Goldenrod, /*Color.Gray,*/ Color.Green, /*Color.GreenYellow,*/
            Color.Honeydew, Color.HotPink, Color.IndianRed, Color.Indigo, /*Color.Khaki,*/ Color.Lavender,
                             /*Color.LavenderBlush,*/ Color.LawnGreen, /*Color.LemonChiffon,*/ Color.Lime,
            Color.LimeGreen, /*Color.Linen,*/ Color.Magenta, Color.Maroon, Color.MediumAquamarine, Color.MediumBlue,
                             /*Color.MediumOrchid,*/ Color.MediumPurple, /*Color.MediumSeaGreen,*/ Color.MediumSlateBlue, Color.MediumSpringGreen,
            Color.MediumTurquoise, Color.MediumVioletRed, Color.MidnightBlue, /*Color.MistyRose,*/ /*Color.Moccasin,*/
            Color.Navy, /*Color.OldLace,*/ Color.Olive, /*Color.OliveDrab,*/ Color.Orange, Color.OrangeRed,
                             /*Color.Orchid, Color.PaleVioletRed, Color.PapayaWhip, */
                             /*Color.PeachPuff,*/ /*Color.Peru,*/ Color.Pink, Color.Plum, /*Color.PowderBlue,*/ Color.Purple, Color.Red, Color.RosyBrown,
            Color.RoyalBlue, Color.SaddleBrown, Color.Salmon, /*Color.SandyBrown,*/ Color.SeaGreen, /*Color.Sienna,*/
                             /*Color.Silver,*/ Color.SkyBlue, Color.SlateBlue, /*Color.SlateGray,*/ Color.SpringGreen, Color.SteelBlue,
                             /*Color.Tan,*/ Color.Teal, Color.Thistle, Color.Tomato, Color.Turquoise, Color.Violet, /*Color.Wheat,*/
                             /*Color.Yellow,*/ Color.YellowGreen,
        };

        private static readonly double[,] lowPassKernal =
        {
        {
            0.1, 0.1, 0.1,
        },
        {
                                                0.1, 0.2, 0.1,
        },
        {
                                                0.1, 0.1, 0.1,
        },
        };

        private static readonly double[,] highPassKernal1 =
        {
        {
            -1.0, -1.0, -1.0,
        },
        {
            -1.0, 9.0, -1.0,
        },
        {
            -1.0, -1.0, -1.0,
        },
        };

        private static readonly double[,] highPassKernal2 =
        {
        {
            -0.3, -0.3, -0.3, -0.3, -0.3,
        },
        {
                                                -0.3, -0.3, -0.3, -0.3, -0.3,
        },
        {
                                                -0.3, -0.3,  9.7, -0.3, -0.3,
        },
        {
                                                -0.3, -0.3, -0.3, -0.3, -0.3,
        },
        {
                                                -0.3, -0.3, -0.3, -0.3, -0.3,
        },
        };

        private static readonly double[,] vertLineKernal =
        {
        {
            -0.5, 1.0, -0.5,
        },
        {
            -0.5, 1.0, -0.5,
        },
        {
            -0.5, 1.0, -0.5,
        },
        };

        private static readonly double[,] horiLineKernal3 =
        {
        {
            -0.5, -0.5, -0.5,
        },
        {
            1.0, 1.0, 1.0,
        },
        {
            -0.5, -0.5, -0.5,
        },
        };

        private static readonly double[,] horiLineKernal5 =
        {
        {
            -0.5, -0.5, -0.5, -0.5, -0.5,
        },
        {
            1.0, 1.0, 1.0, 1.0, 1.0,
        },
        {
            -0.5, -0.5, -0.5, -0.5, -0.5,
        },
        };

        private static readonly double[,] diagLineKernal1 =
        {
        {
            2.0, -1.0, -1.0,
        },
        {
            -1.0, 2.0, -1.0,
        },
        {
            -1.0, -1.0, 2.0,
        },
        };

        private static readonly double[,] diagLineKernal2 =
        {
        {
            -1.0, -1.0, 2.0,
        },
        {
            -1.0, 2.0, -1.0,
        },
        {
            2.0, -1.0, -1.0,
        },
        };

        private static readonly double[,] Laplace1Kernal =
        {
        {
            0.0, -1.0, 0.0,
        },
        {
            -1.0, 4.0, -1.0,
        },
        {
            0.0, -1.0, 0.0,
        },
        };

        private static readonly double[,] Laplace2Kernal =
        {
        {
            -1.0, -1.0, -1.0,
        },
        {
            -1.0, 8.0, -1.0,
        },
        {
            -1.0, -1.0, -1.0,
        },
        };

        private static readonly double[,] Laplace3Kernal =
        {
        {
            1.0, -2.0, 1.0,
        },
        {
            -2.0, 4.0, -2.0,
        },
        {
            1.0, -2.0, 1.0,
        },
        };

        private static readonly double[,] Laplace4Kernal =
        {
        {
            -1.0, -1.0, -1.0,
        },
        {
            -1.0, 9.0, -1.0,
        },
        {
            -1.0, -1.0, -1.0,
        },
        }; //subtracts original

        private static readonly double[,] grid2 =
        {
        {
            -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5,
        },
        {
                                                -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5,
        },

//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
        {
                                                -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5,
        },
        };

        //static double[,] grid2Wave =      { { -0.5, 1.0, -1.5, 2.0, -1.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.5, 2.0, -1.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.5, 2.0, -1.5, 1.0, -0.5}};
        private static readonly double[,] grid3 =
        {
        {
            -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5,
        },
        {
                                                -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5,
        },
        {
                                                -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5,
        },
        {
                                                -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5,
        },
        {
                                                -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5,
        },
        {
                                                -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5,
        },
        {
                                                -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5,
        },
        };

        private static readonly double[,] grid4 =
        {
        {
            -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375,
        },
        {
                                                -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375,
        },
        {
                                                -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375,
        },
        {
                                                -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375,
        },
        {
                                                -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375,
        },
        {
                                                -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375,
        },
        {
                                                -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375,
        },
        };

        private static readonly double[,] grid2Wave =
        {
        {
            -0.5, -0.5, -0.5,
        },
        {
                                                1.0,  1.0,  1.0,
        },
        {
                                                -1.5, -1.5, -1.5,
        },
        {
                                                2.0,  2.0,  2.0,
        },
        {
                                                -1.5, -1.5, -1.5,
        },
        {
                                                1.0,  1.0,  1.0,
        },
        {
                                                -0.5, -0.5, -0.5,
        },
        };

        private static readonly double[,] grid3Wave =
        {
        {
            -0.5, -0.5, -0.5,
        },
        {
                                                1.0,  1.0,  1.0,
        },
        {
                                                -0.5, -0.5, -0.5,
        },
        {
                                                -1.0, -1.0, -1.0,
        },
        {
                                                2.0,  2.0,  2.0,
        },
        {
                                                -1.0, -1.0, -1.0,
        },
        {
                                                -0.5, -0.5, -0.5,
        },
        {
                                                1.0,  1.0,  1.0,
        },
        {
                                                -0.5, -0.5, -0.5,
        },
        };

        public static double[,] SobelX =
        {
        {
            -1.0,  0.0,  1.0,
        },
        {
                                                -2.0,  0.0,  -2.0,
        },
        {
                                                -1.0,  0.0,  1.0,
        },
        };

        public static double[,] SobelY =
        {
        {
            1.0,  2.0,  1.0,
        },
        {
                                                0.0,  0.0,  0.0,
        },
        {
                                                -1.0, -2.0, -1.0,
        },
        };

        private static readonly double[,] ridgeDir0Mask1 = new[,] {
            {
                -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                0.4, 0.4, 0.4, 0.4, 0.4,
            },
            {
                -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                -0.1, -0.1, -0.1, -0.1, -0.1,
            },
        };

        private static readonly double[,] ridgeDir1Mask1 = new[,] {
            {
                -0.1, -0.1, -0.1, -0.1, 0.4,
            },
            {
                -0.1, -0.1, -0.1, 0.4, -0.1,
            },
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                -0.1, 0.4, -0.1, -0.1, -0.1,
            },
            {
                0.4, -0.1, -0.1, -0.1, -0.1,
            },
        };

        private static readonly double[,] ridgeDir2Mask1 = new[,] {
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
        };

        private static readonly double[,] ridgeDir3Mask1 = new[,] {
            {
                0.4, -0.1, -0.1, -0.1, -0.1,
            },
            {
                -0.1, 0.4, -0.1, -0.1, -0.1,
            },
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                -0.1, -0.1, -0.1, 0.4, -0.1,
            },
            {
                -0.1, -0.1, -0.1, -0.1, 0.4,
            },
        };

        public static Image<Rgb24> ReadImage2Image(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            return Image.Load<Rgb24>(fileName);
        }

        public static void WriteImage2File(Image binaryBmp, string opPath)
        {
            binaryBmp.Save(opPath);
        }

        public static Image ApplyInvert(Image<Rgb24> ImageImage)
        {
            Image<Rgb24> returnImage = ImageImage.CloneAs<Rgb24>();

            for (int y = 0; y < ImageImage.Height; y++)
            {
                for (int x = 0; x < ImageImage.Width; x++)
                {
                    var pixelColor = ImageImage[x, y];

                    var R = (byte)(255 - pixelColor.R);
                    var G = (byte)(255 - pixelColor.G);
                    var B = (byte)(255 - pixelColor.B);
                    returnImage[x, y] = new Rgb24(R, G, B);
                }
            }

            return returnImage;
        }

        /// <summary>
        /// reads the intensity of a grey scale image into a matrix of double.
        /// Assumes gray scale is 0-255 and that color.R = color.G = color.B.
        /// </summary>
        public static double[,] GreyScaleImage2Matrix(Image<Rgb24> Image)
        {
            int height = Image.Height; //height
            int width = Image.Width;   //width

            var matrix = new double[height, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var color = Image[c, r];

                    //double value = (255 - color.R) / (double)255;
                    //if (value > 0.0) LoggedConsole.WriteLine(value);
                    matrix[r, c] = (255 - color.R) / (double)255;
                }
            }

            return matrix;
        }

        public static double[,] Convolve(double[,] matrix, Kernal name)
        {
            double[,] kernal;

            //SWITCH KERNALS
            switch (name)
            {
                case Kernal.LowPass:
                    kernal = lowPassKernal;
                    break;
                case Kernal.HighPass1:
                    kernal = highPassKernal1;
                    break;
                case Kernal.HighPass2:
                    kernal = highPassKernal2;
                    LoggedConsole.WriteLine("Applied highPassKernal2 Kernal");
                    break;
                case Kernal.HorizontalLine3:
                    kernal = horiLineKernal3;
                    break;
                case Kernal.HorizontalLine5:
                    kernal = horiLineKernal5;
                    LoggedConsole.WriteLine("Applied Horizontal Line5 Kernal");
                    break;
                case Kernal.VerticalLine:
                    kernal = vertLineKernal;
                    break;
                case Kernal.DiagLine1:
                    kernal = diagLineKernal1;
                    LoggedConsole.WriteLine("Applied diagLine1 Kernal");
                    break;
                case Kernal.DiagLine2:
                    kernal = diagLineKernal2;
                    LoggedConsole.WriteLine("Applied diagLine2 Kernal");
                    break;
                case Kernal.Laplace1:
                    kernal = Laplace1Kernal;
                    LoggedConsole.WriteLine("Applied Laplace1 Kernal");
                    break;
                case Kernal.Laplace2:
                    kernal = Laplace2Kernal;
                    LoggedConsole.WriteLine("Applied Laplace2 Kernal");
                    break;
                case Kernal.Laplace3:
                    kernal = Laplace3Kernal;
                    LoggedConsole.WriteLine("Applied Laplace3 Kernal");
                    break;
                case Kernal.Laplace4:
                    kernal = Laplace4Kernal;
                    LoggedConsole.WriteLine("Applied Laplace4 Kernal");
                    break;

                default:
                    throw new Exception("\nWARNING: INVALID MODE!");
            }

            int mRows = matrix.GetLength(0);
            int mCols = matrix.GetLength(1);
            int kRows = kernal.GetLength(0);
            int kCols = kernal.GetLength(1);
            int rNH = kRows / 2;
            int cNH = kCols / 2;

            if (rNH <= 0 && cNH <= 0)
            {
                return matrix; //no operation required
            }

            //int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);//area of rectangular neighbourhood

            //double[,] newMatrix = (double[,])matrix.Clone();
            double[,] newMatrix = new double[mRows, mCols]; //init new matrix to return

            // fix up the edges first
            for (int r = 0; r < mRows; r++)
            {
                for (int c = 0; c < cNH; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }

                for (int c = mCols - cNH; c < mCols; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }

            // fix up other edges
            for (int c = 0; c < mCols; c++)
            {
                for (int r = 0; r < rNH; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }

                for (int r = mRows - rNH; r < mRows; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }

            //now do bulk of image
            for (int r = rNH; r < mRows - rNH; r++)
            {
                for (int c = cNH; c < mCols - cNH; c++)
                {
                    double sum = 0.0;
                    for (int y = -rNH; y < rNH; y++)
                    {
                        for (int x = -cNH; x < cNH; x++)
                        {
                            sum += matrix[r + y, c + x] * kernal[rNH - y, cNH - x];
                        }
                    }

                    newMatrix[r, c] = sum; // / (double)area;
                }
            }

            return newMatrix;
        }

        public static double[,] GridFilter(double[,] m, Kernal name)
        {
            double[,] kernal;
            int noiseSampleCount = 500000;

            //double thresholdZScore = 3.1;  //zscore threshold for p=0.001
            //double thresholdZScore = 2.58; //zscore threshold for p=0.005
            //double thresholdZScore = 2.33; //zscore threshold for p=0.01
            double thresholdZScore = 1.98;   //zscore threshold for p=0.05

            //SWITCH KERNALS
            switch (name)
            {
                case Kernal.Grid2:
                    kernal = grid2;
                    LoggedConsole.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid3:
                    kernal = grid3;
                    LoggedConsole.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid4:
                    kernal = grid4;
                    LoggedConsole.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid2Wave:
                    kernal = grid2Wave;
                    LoggedConsole.WriteLine("Applied Grid Wave Kernal 2");
                    break;
                case Kernal.Grid3Wave:
                    kernal = grid3Wave;
                    LoggedConsole.WriteLine("Applied Grid Wave Kernal 3");
                    break;

                default:
                    throw new Exception("\nWARNING: INVALID MODE!");
            }

            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            int kRows = kernal.GetLength(0);
            int kCols = kernal.GetLength(1);
            int rNH = kRows / 2;
            int cNH = kCols / 2;
            if (rNH <= 0 && cNH <= 0)
            {
                return m; //no operation required
            }

            //int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);//area of rectangular neighbourhood

            double[,] normM = DataTools.normalise(m);

            double[] noiseScores = new double[noiseSampleCount];
            for (int n = 0; n < noiseSampleCount; n++)
            {
                double[,] noise = GetNoise(normM, kRows, kCols);
                double sum = 0.0;
                for (int i = 0; i < kRows; i++)
                {
                    for (int j = 0; j < kCols; j++)
                    {
                        sum += noise[i, j] * kernal[i, j];
                    }
                }

                noiseScores[n] = sum / kRows;
            }

            NormalDist.AverageAndSD(noiseScores, out var noiseAv, out var noiseSd);
            LoggedConsole.WriteLine("noiseAv=" + noiseAv + "   noiseSd=" + noiseSd);

            double[,] newMatrix = new double[mRows, mCols]; //init new matrix to return

            //now do bulk of image
            for (int r = rNH; r < mRows - rNH; r++)
            {
                for (int c = cNH; c < mCols - cNH; c++)
                {
                    double sum = 0.0;
                    for (int y = -rNH; y < rNH; y++)
                    {
                        for (int x = -cNH; x < cNH; x++)
                        {
                            sum += normM[r + y, c + x] * kernal[rNH + y, cNH + x];
                        }
                    }

                    sum /= kRows;
                    double zScore = (sum - noiseAv) / noiseSd;

                    if (zScore >= thresholdZScore)
                    {
                        newMatrix[r, c] = 1.0;
                        for (int n = -rNH; n < rNH; n++)
                        {
                            newMatrix[r + n, c] = 1.0;
                        }

                        //newMatrix[r, c] = zScore;
                        //newMatrix[r + 1, c] = zScore;
                    }

                    //else newMatrix[r, c] = 0.0;
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// Returns a small matrix of pixels chosen randomly from the passed matrix, m.
        /// The row and column is chosen randomly and then the reuired number of consecutive pixels is transferred.
        /// These noise matrices are used to obtain statistics for cross-correlation coefficients.
        /// </summary>
        public static double[,] GetNoise(double[,] m, int kRows, int kCols)
        {
            int mHeight = m.GetLength(0);
            int mWidth = m.GetLength(1);

            double[,] noise = new double[kRows, kCols];
            RandomNumber rn = new RandomNumber();
            for (int r = 0; r < kRows; r++)
            {
                int randomRow = rn.GetInt(mHeight - kRows);
                int randomCol = rn.GetInt(mWidth - kCols);
                for (int c = 0; c < kCols; c++)
                {
                    noise[r, c] = m[randomRow, randomCol + c];
                }
            }

            return noise;
        } //end getNoise()

        public static double[,] WienerFilter(double[,] matrix)
        {
            int NH = 3;
            return WienerFilter(matrix, NH);
        }

        public static double[,] WienerFilter(double[,] matrix, int NH)
        {
            int M = NH;
            int N = NH;
            int rNH = M / 2;
            int cNH = N / 2;

            //double totMean = 0.0;
            //double totSD = 0.0;
            //NormalDist.AverageAndSD(matrix, out totMean, out totSD);
            //double colVar = totSD * totSD;

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];
            for (int c = 0; c < cols; c++)
            {
                double[] column = DataTools.GetColumn(matrix, c);
                double colMean = 0.0;
                double colSD = 0.0;
                NormalDist.AverageAndSD(column, out colMean, out colSD);
                double colVar = colSD * colSD;

                for (int r = 0; r < rows; r++)
                {
                    double X = 0.0;
                    double Xe2 = 0.0;
                    int count = 0;
                    for (int i = r - rNH; i <= r + rNH; i++)
                    {
                        if (i < 0)
                        {
                            continue;
                        }

                        if (i >= rows)
                        {
                            continue;
                        }

                        for (int j = c - cNH; j <= c + cNH; j++)
                        {
                            if (j < 0)
                            {
                                continue;
                            }

                            if (j >= cols)
                            {
                                continue;
                            }

                            X += matrix[i, j];
                            Xe2 += matrix[i, j] * matrix[i, j];
                            count++;

                            //LoggedConsole.WriteLine(i+"  "+j+"   count="+count);
                            //Console.ReadLine();
                        }
                    }

                    //LoggedConsole.WriteLine("End NH count="+count);
                    //calculate variance of the neighbourhood
                    double mean = X / count;
                    double variance = (Xe2 / count) - (mean * mean);
                    double numerator = variance - colVar;
                    if (numerator < 0.0)
                    {
                        numerator = 0.0;
                    }

                    double denominator = variance;
                    if (colVar > denominator)
                    {
                        denominator = colVar;
                    }

                    double ratio = numerator / denominator;
                    outM[r, c] = mean + (ratio * (matrix[r, c] - mean));

                    // LoggedConsole.WriteLine((outM[r, c]).ToString("F1") + "   " + (matrix[r, c]).ToString("F1"));
                    // Console.ReadLine();
                }
            }

            return outM;
        }

        /// <summary>
        /// this method assumes that all the values in the passed matrix are between zero &amp; one.
        /// Will truncate all values > 1 to 1.0.
        /// Spurious results will occur if have negative values or values > 1.
        /// Should NormaliseMatrixValues matrix first if these conditions do not apply.
        /// </summary>
        public static double[,] ContrastStretching(double[,] M, double fractionalStretching)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            double[,] norm = MatrixTools.NormaliseMatrixValues(M);

            int binCount = 100;
            double binWidth = 0.01;
            double min = 0.0;
            double max = 1.0;
            int[] histo = Histogram.Histo(M, binCount, min, max, binWidth);

            int cellCount = rowCount * colCount;
            int thresholdCount = (int)(cellCount * fractionalStretching);

            // get low side stretching bound
            int bottomSideCount = 0;
            for (int i = 0; i < binCount; i++)
            {
                bottomSideCount += histo[i];
                if (bottomSideCount > thresholdCount)
                {
                    min = i * binWidth;
                    break;
                }
            }

            // get high side stretching bound
            int topSideCount = 0;
            for (int i = binCount - 1; i >= 0; i--)
            {
                topSideCount += histo[i];
                if (topSideCount > thresholdCount)
                {
                    max = 1 - ((binCount - i) * binWidth);
                    break;
                }
            }

            // truncate min and max and thereby contrast stretch.
            norm = MatrixTools.NormaliseInZeroOne(norm, min, max);
            return norm;
        }

        /// <summary>
        /// This method is a TEST method for Canny edge detection - see below.
        /// </summary>
        public static void TestCannyEdgeDetection()
        {
            //string path = @"C:\SensorNetworks\Output\Human\DM420036_min465Speech_0min.png";
            //string path = @"C:\SensorNetworks\Output\Sonograms\TestForHoughTransform.png";
            //string path = @"C:\SensorNetworks\Output\LewinsRail\BAC1_20071008-081607_0min.png";
            string path = @"C:\SensorNetworks\Output\LewinsRail\BAC2_20071008-085040_0min.png";
            FileInfo file = new FileInfo(path);
            Image<Rgb24> sourceImage = ReadImage2Image(file.FullName);
            ApplyInvert(sourceImage);
            byte lowThreshold = 0;
            byte highThreshold = 30;
            Image bmp2 = CannyEdgeDetection(sourceImage, lowThreshold, highThreshold);
            string path1 = @"C:\SensorNetworks\Output\LewinsRail\Canny.png";
            bmp2.Save(path1);
        }

        /// <summary>
        /// The below method is derived from the following site
        /// http://premsivakumar.wordpress.com/2010/12/13/edge-detection-using-c-and-aforge-net/
        /// The author references the following Afroge source code
        /// http://www.aforgenet.com/framework/features/edge_detectors_filters.html
        /// See the below link for how to set the thresholds etc
        /// http://homepages.inf.ed.ac.uk/rbf/HIPR2/canny.htm.
        ///
        /// </summary>
        public static Image CannyEdgeDetection(Image<Rgb24> bmp, byte lowThreshold, byte highThreshold)
        {

            // this filter converts standard pixel format to indexed as used by the hough transform
            // blur the result

            CannyEdgeDetector cannyFilter = new CannyEdgeDetector(lowThreshold, highThreshold, 2f, 3);
            Image edge = cannyFilter.Apply(bmp);
            return edge;
        }

        public static double[,] SobelEdgeDetection(double[,] m)
        {
            double relThreshold = 0.2;
            return SobelEdgeDetection(m, relThreshold);
        }

        /// <summary>
        /// This version of Sobel's edge detection taken from  Graig A. Lindley, Practical Image Processing
        /// which includes C code.
        /// </summary>
        public static double[,] SobelEdgeDetection(double[,] m, double relThreshold)
        {
            //define indices into grid using Lindley notation
            const int a = 0;
            const int b = 1;
            const int c = 2;
            const int d = 3;
            const int e = 4;
            const int f = 5;
            const int g = 6;
            const int h = 7;
            const int i = 8;
            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            double[,] normM = DataTools.normalise(m);
            double[,] newMatrix = new double[mRows, mCols]; //init new matrix to return
            double[] grid = new double[9]; //to represent 3x3 grid
            double min = double.MaxValue;
            double max = -double.MaxValue;

            for (int y = 1; y < mRows - 1; y++)
            {
                for (int x = 1; x < mCols - 1; x++)
                {
                    grid[a] = normM[y - 1, x - 1];
                    grid[b] = normM[y, x - 1];
                    grid[c] = normM[y + 1, x - 1];
                    grid[d] = normM[y - 1, x];
                    grid[e] = normM[y, x];
                    grid[f] = normM[y + 1, x];
                    grid[g] = normM[y - 1, x + 1];
                    grid[h] = normM[y, x + 1];
                    grid[i] = normM[y + 1, x + 1];
                    double[] differences = new double[4];
                    double DivideAEI_avBelow = (grid[d] + grid[g] + grid[h]) / 3;
                    double DivideAEI_avAbove = (grid[b] + grid[c] + grid[f]) / 3;
                    differences[0] = Math.Abs(DivideAEI_avAbove - DivideAEI_avBelow);

                    double DivideBEH_avBelow = (grid[a] + grid[d] + grid[g]) / 3;
                    double DivideBEH_avAbove = (grid[c] + grid[f] + grid[i]) / 3;
                    differences[1] = Math.Abs(DivideBEH_avAbove - DivideBEH_avBelow);

                    double DivideCEG_avBelow = (grid[f] + grid[h] + grid[i]) / 3;
                    double DivideCEG_avAbove = (grid[a] + grid[b] + grid[d]) / 3;
                    differences[2] = Math.Abs(DivideCEG_avAbove - DivideCEG_avBelow);

                    double DivideDEF_avBelow = (grid[g] + grid[h] + grid[i]) / 3;
                    double DivideDEF_avAbove = (grid[a] + grid[b] + grid[c]) / 3;
                    differences[3] = Math.Abs(DivideDEF_avAbove - DivideDEF_avBelow);
                    DataTools.MinMax(differences, out var gridMin, out var gridMax);

                    newMatrix[y, x] = gridMax;
                    if (min > gridMin)
                    {
                        min = gridMin;
                    }

                    if (max < gridMax)
                    {
                        max = gridMax;
                    }
                }
            }

            //double relThreshold = 0.2;
            double threshold = min + ((max - min) * relThreshold);

            for (int y = 1; y < mRows - 1; y++)
            {
                for (int x = 1; x < mCols - 1; x++)
                {
                    if (newMatrix[y, x] > threshold)
                    {
                        newMatrix[y, x] = 1.0;
                    }
                    else
                    {
                        newMatrix[y, x] = 0.0;
                    }
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// This version of Sobel's edge detection taken from  Graig A. Lindley, Practical Image Processing
        /// which includes C code.
        /// </summary>
        public static double[,] SobelRidgeDetection(double[,] m)
        {
            //define indices into grid using Lindley notation
            const int a = 0;
            const int b = 1;
            const int c = 2;
            const int d = 3;
            const int e = 4;
            const int f = 5;
            const int g = 6;
            const int h = 7;
            const int i = 8;
            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            double[,] normM = DataTools.normalise(m);
            double[,] newMatrix = new double[mRows, mCols]; //init new matrix to return
            double[] grid = new double[9]; //to represent 3x3 grid
            double min = double.MaxValue;
            double max = -double.MaxValue;

            for (int y = 1; y < mRows - 1; y++)
            {
                for (int x = 1; x < mCols - 1; x++)
                {
                    grid[a] = normM[y - 1, x - 1];
                    grid[b] = normM[y, x - 1];
                    grid[c] = normM[y + 1, x - 1];
                    grid[d] = normM[y - 1, x];
                    grid[e] = normM[y, x];
                    grid[f] = normM[y + 1, x];
                    grid[g] = normM[y - 1, x + 1];
                    grid[h] = normM[y, x + 1];
                    grid[i] = normM[y + 1, x + 1];
                    double[] differences = new double[4];
                    double DivideAEI_avBelow = (grid[d] + grid[g] + grid[h]) / 3;
                    double DivideAEI_avAbove = (grid[b] + grid[c] + grid[f]) / 3;

                    //differences[0] = Math.Abs(DivideAEI_avAbove - DivideAEI_avBelow);
                    differences[0] = grid[e] - DivideAEI_avAbove + (grid[e] - DivideAEI_avBelow);
                    if (differences[0] < 0.0)
                    {
                        differences[0] = 0.0;
                    }

                    double DivideBEH_avBelow = (grid[a] + grid[d] + grid[g]) / 3;
                    double DivideBEH_avAbove = (grid[c] + grid[f] + grid[i]) / 3;

                    //differences[1] = Math.Abs(DivideBEH_avAbove - DivideBEH_avBelow);
                    differences[1] = grid[e] - DivideBEH_avBelow + (grid[e] - DivideBEH_avAbove);
                    if (differences[1] < 0.0)
                    {
                        differences[1] = 0.0;
                    }

                    double DivideCEG_avBelow = (grid[f] + grid[h] + grid[i]) / 3;
                    double DivideCEG_avAbove = (grid[a] + grid[b] + grid[d]) / 3;

                    //differences[2] = Math.Abs(DivideCEG_avAbove - DivideCEG_avBelow);
                    differences[2] = grid[e] - DivideCEG_avBelow + (grid[e] - DivideCEG_avAbove);
                    if (differences[2] < 0.0)
                    {
                        differences[2] = 0.0;
                    }

                    double DivideDEF_avBelow = (grid[g] + grid[h] + grid[i]) / 3;
                    double DivideDEF_avAbove = (grid[a] + grid[b] + grid[c]) / 3;

                    //differences[3] = Math.Abs(DivideDEF_avAbove - DivideDEF_avBelow);
                    differences[3] = grid[e] - DivideDEF_avBelow + (grid[e] - DivideDEF_avAbove);
                    if (differences[3] < 0.0)
                    {
                        differences[3] = 0.0;
                    }

                    DataTools.MinMax(differences, out var diffMin, out var diffMax);

                    newMatrix[y, x] = diffMax;
                    if (min > diffMin)
                    {
                        min = diffMin; //store minimum difference value of entire matrix
                    }

                    if (max < diffMax)
                    {
                        max = diffMax; //store maximum difference value of entire matrix
                    }
                }
            }

            double threshold = min + ((max - min) / 4); //threshold is 1/5th of range above min

            for (int y = 1; y < mRows - 1; y++)
            {
                for (int x = 1; x < mCols - 1; x++)
                {
                    if (newMatrix[y, x] > threshold)
                    {
                        newMatrix[y, x] = 1.0;
                    }
                    else
                    {
                        newMatrix[y, x] = 0.0;
                    }
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// This version of Sobel's edge detection taken from  Graig A. Lindley, Practical Image Processing
        /// which includes C code.
        /// HOWEVER MODIFED TO PROCESS 5x5 matrix
        /// MATRIX must be square with odd number dimensions.
        /// </summary>
        public static void SobelRidgeDetection(double[,] m, out bool isRidge, out double magnitude, out double direction)
        {
            //for clarity, give matrix elements LETTERS using Lindley notation
            //ABCDE  00 01 02 03 04
            //FGHIJ  10 11 12 13 14
            //KLMNO  20 21 22 23 24
            //PQRST  30 31 32 33 34
            //UVWXY  40 41 42 43 44
            // We have eight possible ridges with slopes 0, Pi/8, Pi/4, 3Pi/8, pi/2, 5Pi/8, 3Pi/4, 7Pi/8
            // Slope categories are 0 to 7.
            // We calculate the ridge magnitude for each possible ridge direction.

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            if (rows != cols) // must be square matrix
            {
                isRidge = false;
                magnitude = 0.0;
                direction = 0.0;
                return;
            }

            int centreID = rows / 2;
            int cm1 = centreID - 1;
            int cp1 = centreID + 1;
            int cm2 = centreID - 2;
            int cp2 = centreID + 2;
            double[,] ridgeMagnitudes = new double[8, 3];

            //ridge magnitude having slope=0;
            double[] rowSums = MatrixTools.SumRows(m);
            ridgeMagnitudes[0, 1] = rowSums[centreID];
            for (int r = 0; r < centreID; r++)
            {
                ridgeMagnitudes[0, 0] += rowSums[r]; //positve  side magnitude
            }

            for (int r = centreID + 1; r < rows; r++)
            {
                ridgeMagnitudes[0, 2] += rowSums[r]; //negative side magnitude
            }

            ridgeMagnitudes[0, 0] /= centreID * cols;
            ridgeMagnitudes[0, 1] /= cols;
            ridgeMagnitudes[0, 2] /= centreID * cols;

            //ridge magnitude having slope=Pi/8;
            ridgeMagnitudes[1, 1] = (m[cm1, cp2] + m[centreID, cp1] + m[centreID, centreID] + m[centreID, cm1] + m[cp1, cm2]) / 5;

            //positve side magnitude
            ridgeMagnitudes[1, 0] = (m[cm2, cm2] + m[cm2, cm1] + m[cm2, centreID] + m[cm2, cp1] + m[cm2, cp2] + m[cm1, cm2] + m[cm1, cm1] + m[cm1, centreID] + m[cm1, cp1] + m[centreID, cm2]) / 10;

            //negative side magnitude
            ridgeMagnitudes[1, 2] = (m[cp2, cm2] + m[cp2, cm1] + m[cp2, centreID] + m[cp2, cp1] + m[cp2, cp2] + m[cp1, cm1] + m[cp1, centreID] + m[cp1, cp1] + m[cp1, cp2] + m[centreID, cp2]) / 10;

            //ridge magnitude having slope=2Pi/8;
            ridgeMagnitudes[2, 1] = MatrixTools.SumPositiveDiagonal(m) / cols;
            MatrixTools.AverageValuesInTriangleAboveAndBelowPositiveDiagonal(m, out var upperAv, out var lowerAv);
            ridgeMagnitudes[2, 0] = upperAv;
            ridgeMagnitudes[2, 2] = lowerAv;

            //ridge magnitude having slope=3Pi/8;
            ridgeMagnitudes[3, 1] = (m[cm2, cp1] + m[cm1, cp1] + m[centreID, centreID] + m[cp1, cm1] + m[cp2, cm1]) / 5;

            //positve side magnitude
            ridgeMagnitudes[3, 0] = (m[cm2, cm2] + m[cm2, cm1] + m[cm2, centreID] + m[cm1, cm2] + m[cm1, cm1] + m[cm1, centreID] + m[centreID, cm2] + m[centreID, cm1] + m[cp1, cm2] + m[cp2, cm2]) / 10;

            //negative side magnitude
            ridgeMagnitudes[3, 2] = (m[cp2, centreID] + m[cp2, cp1] + m[cp2, cp2] + m[cp1, centreID] + m[cp1, cp1] + m[cp1, cp2] + m[centreID, cp1] + m[centreID, cp2] + m[cm1, cp2] + m[cm2, cp2]) / 10;

            //ridge magnitude having slope=4Pi/8;
            double[] colSums = MatrixTools.SumColumns(m);
            ridgeMagnitudes[4, 1] = colSums[centreID];
            for (int c = 0; c < centreID; c++)
            {
                ridgeMagnitudes[4, 0] += colSums[c]; //positve  side magnitude
            }

            for (int c = centreID + 1; c < rows; c++)
            {
                ridgeMagnitudes[4, 2] += colSums[c]; //negative side magnitude
            }

            ridgeMagnitudes[4, 0] /= centreID * cols;
            ridgeMagnitudes[4, 1] /= cols;
            ridgeMagnitudes[4, 2] /= centreID * cols;

            //ridge magnitude having slope=5Pi/8;
            ridgeMagnitudes[5, 1] = (m[cm2, cm1] + m[cm1, centreID] + m[centreID, centreID] + m[cp1, centreID] + m[cp2, cp1]) / 5;

            //positve side magnitude
            ridgeMagnitudes[5, 0] = (m[cm2, cm2] + m[cm1, cm2] + m[cm1, cm1] + m[centreID, cm2] + m[centreID, cm1] + m[cp1, cm2] + m[cp1, cm1] + m[cp2, cm2] + m[cp2, cm1] + m[cp2, centreID]) / 10; //ABCDE FGHIJ

            //negative side magnitude
            ridgeMagnitudes[5, 2] = (m[cm2, centreID] + m[cm2, cp1] + m[cm2, cp2] + m[cm1, cp1] + m[cm1, cp2] + m[centreID, cp1] + m[centreID, cp2] + m[cp1, cp1] + m[cp1, cp2] + m[cp2, cp2]) / 10; //PQRST UVWXY

            //ridge magnitude having slope=6Pi/8;
            ridgeMagnitudes[6, 1] = MatrixTools.SumNegativeDiagonal(m) / cols;

            //double upperAv, lowerAv;
            MatrixTools.AverageValuesInTriangleAboveAndBelowNegativeDiagonal(m, out upperAv, out lowerAv);
            ridgeMagnitudes[6, 0] = upperAv;
            ridgeMagnitudes[6, 2] = lowerAv;

            //ridge magnitude having slope=7Pi/8;
            ridgeMagnitudes[7, 1] = (m[cm1, cm2] + m[cm1, cm1] + m[centreID, centreID] + m[cp1, cp1] + m[cp1, cp2]) / 5;

            //positve side magnitude
            ridgeMagnitudes[7, 0] = (m[centreID, cm2] + m[centreID, cm1] + m[cp1, cm2] + m[cp1, cm1] + m[cp1, centreID] + m[cp2, cm2] + m[cp2, cm1] + m[cp2, centreID] + m[cp2, cp1] + m[cp2, cp2]) / 10;

            //negative side magnitude
            ridgeMagnitudes[7, 2] = (m[cm2, cm2] + m[cm2, cm1] + m[cm2, centreID] + m[cm2, cp1] + m[cm2, cp2] + m[cm1, centreID] + m[cm1, cp1] + m[cm1, cp2] + m[centreID, cp1] + m[centreID, cp2]) / 10;

            double[] differences = new double[7]; // difference for each direction
            for (int i = 0; i < 7; i++)
            {
                differences[i] = ridgeMagnitudes[i, 1] - ridgeMagnitudes[i, 0] + (ridgeMagnitudes[i, 1] - ridgeMagnitudes[i, 2]);
                differences[i] /= 2; // want average of both differences because easier to select an appropiate decibel threshold for ridge magnitude.
            }

            DataTools.MinMax(differences, out var indexMin, out var indexMax, out var diffMin, out var diffMax);

            //double threshold = min + (max - min) / 4; //threshold is 1/5th of range above min
            double threshold = 0; // dB
            isRidge = ridgeMagnitudes[indexMax, 1] > ridgeMagnitudes[indexMax, 0] + threshold
                      && ridgeMagnitudes[indexMax, 1] > ridgeMagnitudes[indexMax, 2] + threshold;
            magnitude = diffMax;
            direction = indexMax * Math.PI / 8;
        }

        /// <summary>
        /// This version of Sobel's edge detection taken from  Graig A. Lindley, Practical Image Processing which includes C code.
        /// HOWEVER MODIFED TO PROCESS 5x5 matrix
        /// MATRIX must be square with odd number dimensions.
        /// </summary>
        public static void Sobel5X5RidgeDetection(double[,] m, out bool isRidge, out double magnitude, out int direction)
        {
            // We have four possible ridge directions with slopes 0, Pi/4, pi/2, 3Pi/4
            // Slope categories are 0 to 3.
            // We calculate the ridge magnitude for each possible ridge direction using masks.
            // 0 = ridge direction = horizontal or slope = 0;
            // 1 = ridge is positive slope or pi/4
            // 2 = ridge is vertical or pi/2
            // 3 = ridge is negative slope or 3pi/4.

            double[] ridgeMagnitudes = Sobel5X5RidgeDetection(m);

            if (ridgeMagnitudes == null)
            {
                // something has gone wrong
                isRidge = false;
                magnitude = 0.0;
                direction = 0;
                return;
            }

            DataTools.MinMax(ridgeMagnitudes, out var indexMin, out var indexMax, out var diffMin, out var diffMax);

            double threshold = 0; // dB
            isRidge = ridgeMagnitudes[indexMax] > threshold;
            magnitude = diffMax / 2;
            direction = indexMax;
        }

        public static double[] Sobel5X5RidgeDetection(double[,] m)
        {
            // We have four possible ridges with slopes 0, Pi/4, pi/2, 3Pi/4
            // Slope categories are 0 to 3.
            // We calculate the ridge magnitude for each possible ridge direction using masks.
            // 0 = ridge direction = horizontal or slope = 0;
            // 1 = ridge is positive slope or pi/4
            // 2 = ridge is vertical or pi/2
            // 3 = ridge is negative slope or 3pi/4.

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            // must be a square 5X5 matrix
            if (rows != cols || rows != 5)
            {
                return null;
            }

            double[] ridgeMagnitudes = new double[4];
            ridgeMagnitudes[0] = MatrixTools.DotProduct(ridgeDir0Mask1, m);
            ridgeMagnitudes[1] = MatrixTools.DotProduct(ridgeDir1Mask1, m);
            ridgeMagnitudes[2] = MatrixTools.DotProduct(ridgeDir2Mask1, m);
            ridgeMagnitudes[3] = MatrixTools.DotProduct(ridgeDir3Mask1, m);
            return ridgeMagnitudes;
        }

        /// <summary>
        /// This modifies Sobel's ridge detection by using mexican hat filter.
        /// The mexican hat is the difference of two gaussians on different scales.
        /// DoG is used in image processing to find ridges.
        /// MATRIX must be square with odd number dimensions.
        /// </summary>
        public static void MexicanHat5X5RidgeDetection(double[,] m, out bool isRidge, out double magnitude, out int direction)
        {
            // We have four possible ridges with slopes 0, Pi/4, pi/2, 3Pi/4
            // Slope categories are 0 to 3.
            // We calculate the ridge magnitude for each possible ridge direction using masks.

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            // must be square 5X5 matrix
            if (rows != cols || rows != 5)
            {
                isRidge = false;
                magnitude = 0.0;
                direction = 0;
                return;
            }

            double[,] ridgeDir0Mask =
            {
            {
                -0.2, -0.2, -0.2, -0.2, -0.2,
            },
            {
                                            -0.3, -0.3, -0.3, -0.3, -0.3,
            },
            {
                                            1.0, 1.0, 1.0, 1.0, 1.0,
            },
            {
                                            -0.3, -0.3, -0.3, -0.3, -0.3,
            },
            {
                                            -0.2, -0.2, -0.2, -0.2, -0.2,
            },
            };
            double[,] ridgeDir1Mask =
            {
            {
                -0.1, -0.2, -0.2, -0.3, 0.8,
            },
            {
                                            -0.2, -0.2, -0.3, 1.0, -0.3,
            },
            {
                                            -0.2, -0.3, 1.0, -0.3, -0.2,
            },
            {
                                            -0.3, 1.0, -0.3, -0.2, -0.2,
            },
            {
                                            0.8, -0.3, -0.2, -0.2, -0.1,
            },
            };
            double[,] ridgeDir2Mask =
            {
            {
                -0.2, -0.3, 1.0, -0.3, -0.2,
            },
            {
                                            -0.2, -0.3, 1.0, -0.3, -0.2,
            },
            {
                                            -0.2, -0.3, 1.0, -0.3, -0.2,
            },
            {
                                            -0.2, -0.3, 1.0, -0.3, -0.2,
            },
            {
                                            -0.2, -0.3, 1.0, -0.3, -0.2,
            },
            };
            double[,] ridgeDir3Mask =
            {
            {
                0.8, -0.3, -0.2, -0.2, -0.1,
            },
            {
                                            -0.3, 1.0, -0.3, -0.2, -0.2,
            },
            {
                                            -0.2, -0.3, 1.0, -0.3, -0.2,
            },
            {
                                            -0.2, -0.2, -0.3, 1.0, -0.3,
            },
            {
                                            -0.1, -0.2, -0.2, -0.3, 0.8,
            },
            };

            double[] ridgeMagnitudes = new double[4];
            ridgeMagnitudes[0] = MatrixTools.DotProduct(ridgeDir0Mask, m);
            ridgeMagnitudes[1] = MatrixTools.DotProduct(ridgeDir1Mask, m);
            ridgeMagnitudes[2] = MatrixTools.DotProduct(ridgeDir2Mask, m);
            ridgeMagnitudes[3] = MatrixTools.DotProduct(ridgeDir3Mask, m);

            DataTools.MinMax(ridgeMagnitudes, out var indexMin, out var indexMax, out var diffMin, out var diffMax);

            // dB
            double threshold = 0;
            isRidge = ridgeMagnitudes[indexMax] > threshold;
            magnitude = diffMax / 2;
            direction = indexMax;
        }

        public static void Sobel5X5CornerDetection(double[,] m, out bool isCorner, out double magnitude, out double direction)
        {
            // We have eight possible corners in directions 0, Pi/4, pi/2, 3Pi/4
            // Corner categories are 0 to 7.
            // We calculate the ridge magnitude for each possible ridge direction using masks.

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            // must be square 5X5 matrix
            if (rows != cols || rows != 5)
            {
                isCorner = false;
                magnitude = 0.0;
                direction = 0.0;
                return;
            }

            double[,] ridgeDir0Mask =
            {
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, 0.4, 0.4,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            };
            double[,] ridgeDir1Mask =
            {
            {
                0.4, -0.1, -0.1, -0.1, 0.4,
            },
            {
                                            -0.1, 0.4, -0.1, 0.4, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            };
            double[,] ridgeDir2Mask =
            {
            {
                -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            0.4, 0.4, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            };
            double[,] ridgeDir3Mask =
            {
            {
                0.4, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, 0.4, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, 0.4, -0.1, -0.1, -0.1,
            },
            {
                                            0.4, -0.1, -0.1, -0.1, -0.1,
            },
            };
            double[,] ridgeDir4Mask =
            {
            {
                -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            0.4, 0.4, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            };
            double[,] ridgeDir5Mask =
            {
            {
                -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, 0.4, -0.1, 0.4, -0.1,
            },
            {
                                            0.4, -0.1, -0.1, -0.1, 0.4,
            },
            };
            double[,] ridgeDir6Mask =
            {
            {
                -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, 0.4, 0.4,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            };
            double[,] ridgeDir7Mask =
            {
            {
                -0.1, -0.1, -0.1, -0.1, 0.4,
            },
            {
                                            -0.1, -0.1, -0.1, 0.4, -0.1,
            },
            {
                                            -0.1, -0.1, 0.4, -0.1, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, 0.4, -0.1,
            },
            {
                                            -0.1, -0.1, -0.1, -0.1, 0.4,
            },
            };

            double[] cornerMagnitudes = new double[8];
            cornerMagnitudes[0] = MatrixTools.DotProduct(ridgeDir0Mask, m);
            cornerMagnitudes[1] = MatrixTools.DotProduct(ridgeDir1Mask, m);
            cornerMagnitudes[2] = MatrixTools.DotProduct(ridgeDir2Mask, m);
            cornerMagnitudes[3] = MatrixTools.DotProduct(ridgeDir3Mask, m);
            cornerMagnitudes[4] = MatrixTools.DotProduct(ridgeDir4Mask, m);
            cornerMagnitudes[5] = MatrixTools.DotProduct(ridgeDir5Mask, m);
            cornerMagnitudes[6] = MatrixTools.DotProduct(ridgeDir6Mask, m);
            cornerMagnitudes[7] = MatrixTools.DotProduct(ridgeDir7Mask, m);

            DataTools.MinMax(cornerMagnitudes, out var indexMin, out var indexMax, out var diffMin, out var diffMax);

            // dB
            double threshold = 0;
            isCorner = cornerMagnitudes[indexMax] > threshold;
            magnitude = diffMax / 2;
            direction = indexMax * Math.PI / 8;
        }

        /// <summary>
        /// Reverses a 256 grey scale image.
        /// </summary>
        public static double[,] Reverse256GreyScale(double[,] m)
        {
            const int scaleMax = 256 - 1;
            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            double[,] newMatrix = DataTools.normalise(m);
            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                {
                    newMatrix[i, j] = scaleMax - newMatrix[i, j];
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// blurs an image using a square neighbourhood.
        /// </summary>
        /// <param name="matrix">the image ot be blurred.</param>
        /// <param name="nh">Note that neighbourhood is distance either side of central pixel.</param>
        public static double[,] Blur(double[,] matrix, int nh)
        {
            if (nh <= 0)
            {
                return matrix; //no blurring required
            }

            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            int cellCount = ((2 * nh) + 1) * ((2 * nh) + 1);

            //double[,] newMatrix = new double[M, N];
            double[,] newMatrix = (double[,])matrix.Clone();

            for (int i = nh; i < M - nh; i++)
            {
                for (int j = nh; j < N - nh; j++)
                {
                    double sum = 0.0;
                    for (int x = i - nh; x < i + nh; x++)
                    {
                        for (int y = j - nh; y < j + nh; y++)
                        {
                            sum += matrix[x, y];
                        }
                    }

                    double v = sum / cellCount;
                    newMatrix[i, j] = v;
                }
            }

            return newMatrix;
        }

        /// <summary>
        /// blurs and image using a rectangular neighbourhood.
        /// Note that in this method neighbourhood dimensions are full side or window.
        /// </summary>
        /// <param name="matrix">image to be blurred.</param>
        /// <param name="cWindow">column Window i.e. x-dimension.</param>
        /// <param name="rWindow">row Window i.e. y-dimension.</param>
        public static double[,] Blur(double[,] matrix, int cWindow, int rWindow)
        {
            if (cWindow <= 1 && rWindow <= 1)
            {
                return matrix; //no blurring required
            }

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int cNh = cWindow / 2;
            int rNh = rWindow / 2;

            //LoggedConsole.WriteLine("cNH=" + cNH + ", rNH" + rNH);
            int area = ((2 * cNh) + 1) * ((2 * rNh) + 1); //area of rectangular neighbourhood
            double[,] newMatrix = new double[rows, cols]; //init new matrix to return

            // fix up the edges first
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cNh; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }

                for (int c = cols - cNh; c < cols; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }

            // fix up other edges
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rNh; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }

                for (int r = rows - rNh; r < rows; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }

            for (int r = rNh; r < rows - rNh; r++)
            {
                for (int c = cNh; c < cols - cNh; c++)
                {
                    double sum = 0.0;
                    for (int y = r - rNh; y <= r + rNh; y++)
                    {
                        //System.LoggedConsole.WriteLine(r+", "+c+ "  y="+y);
                        for (int x = c - cNh; x <= c + cNh; x++)
                        {
                            sum += matrix[y, x];
                        }
                    }

                    newMatrix[r, c] = sum / area;
                }
            }

            return newMatrix;
        } //end method Blur()

        // ###################################################################################################################################

        /// <summary>
        /// returns the upper and lower thresholds for the pass upper and lower percentile cuts of matrix M
        /// Used for some of the noise reduciton algorithms.
        /// </summary>
        public static void PercentileThresholds(double[,] M, double lowerCut, double upperCut, out double lowerThreshold, out double upperThreshold)
        {
            int binCount = 50;
            int count = M.GetLength(0) * M.GetLength(1);
            int[] powerHisto = Histogram.Histo(M, binCount, out var binWidth, out var min, out var max);
            powerHisto[binCount - 1] = 0; //just in case it is the max ????????????????????????????????????? !!!!!!!!!!!!!!!
            double[] smooth = DataTools.filterMovingAverage(powerHisto, 3);
            DataTools.getMaxIndex(smooth, out var maxindex);

            //calculate threshold for upper percentile
            int clipCount = (int)(upperCut * count);
            int i = binCount - 1;
            int sum = 0;
            while (sum < clipCount && i > 0)
            {
                sum += powerHisto[i--];
            }

            upperThreshold = min + (i * binWidth);

            //calculate threshold for lower percentile
            clipCount = (int)(lowerCut * count);
            int j = 0;
            sum = 0;
            while (sum < clipCount && j < binCount)
            {
                sum += powerHisto[j++];
            }

            lowerThreshold = min + (j * binWidth);
        }

        public static double[,] TrimPercentiles(double[,] matrix)
        {
            //set up parameters for a set of overlapping bands. All numbers should be powers of 2
            int ncbbc = 8;  //number of columns between band centres
            int bandWidth = 64;
            double lowerPercentile = 0.7;
            double upperPercentile = 0.001;

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            int halfWidth = bandWidth / 2;
            int bandCount = width / ncbbc;

            double[,] tmpM = new double[height, ncbbc];
            double[,] outM = new double[height, width];
            double[,] thresholdSubatrix = DataTools.Submatrix(matrix, 0, 0, height - 1, bandWidth);
            PercentileThresholds(thresholdSubatrix, lowerPercentile, upperPercentile, out var lowerThreshold, out var upperThreshold);

            for (int col = 0; col < width; col++)
            {
                var tmpCol = col % ncbbc;
                if (tmpCol == 0 && !(col == 0))
                {
                    //NormaliseMatrixValues existing submatrix and transfer to the output matrix, outM
                    tmpM = DataTools.normalise(tmpM);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < ncbbc; x++)
                        {
                            int startCol = col - ncbbc;
                            outM[y, startCol + x] = tmpM[y, x];
                        }
                    }

                    //set up a new submatrix for processing
                    tmpM = new double[height, ncbbc];

                    //construct new threshold submatrix to recalculate the current threshold
                    int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                    if (start < 0)
                    {
                        start = 0;
                    }

                    int stop = col + halfWidth;
                    if (stop >= width)
                    {
                        stop = width - 1;
                    }

                    thresholdSubatrix = DataTools.Submatrix(matrix, 0, start, height - 1, stop);
                    PercentileThresholds(thresholdSubatrix, lowerPercentile, upperPercentile, out lowerThreshold, out upperThreshold);
                }

                for (int y = 0; y < height; y++)
                {
                    tmpM[y, tmpCol] = matrix[y, col];
                    if (tmpM[y, tmpCol] > upperThreshold)
                    {
                        tmpM[y, tmpCol] = upperThreshold;
                    }

                    if (tmpM[y, tmpCol] < lowerThreshold)
                    {
                        tmpM[y, tmpCol] = lowerThreshold;
                    }

                    //outM[y, col] = matrix[y, col] - upperThreshold;
                    //if (outM[y, col] < upperThreshold) outM[y, col] = upperThreshold;

                    //if (matrix[y, col] < upperThreshold) M[y, col] = 0.0;
                    //else M[y, col] = 1.0;
                }
            }

            return outM;
        }

        // ###################################################################################################################################

        /// <summary>
        /// Calculates the local signal to noise ratio in the neighbourhood of side=window
        /// SNR is defined as local mean / local std dev.
        /// Must check that the local std dev is not too small.
        /// </summary>
        public static double[,] Signal2NoiseRatio_Local(double[,] matrix, int window)
        {
            int nh = window / 2;
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            int cellCount = ((2 * nh) + 1) * ((2 * nh) + 1);
            double[,] newMatrix = new double[M, N];

            for (int i = nh; i < M - nh; i++)
            {
                for (int j = nh; j < N - nh; j++)
                {
                    int id = 0;
                    double[] values = new double[cellCount];
                    for (int x = i - nh + 1; x < i + nh; x++)
                    {
                        for (int y = j - nh + 1; y < j + nh; y++)
                        {
                            values[id++] = matrix[x, y];
                        }
                    }

                    NormalDist.AverageAndSD(values, out var av, out var sd);
                    if (sd < 0.0001)
                    {
                        sd = 0.0001;
                    }

                    newMatrix[i, j] = (matrix[i, j] - av) / sd;
                }
            }

            return newMatrix;
        }

        public static double[,] Signal2NoiseRatio_BandWise(double[,] matrix)
        {
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);

            double[,] M = new double[height, width];
            double[,] subMatrix = DataTools.Submatrix(matrix, 0, 0, height - 1, bandWidth);

            for (int col = 0; col < width; col++)
            {
                int start = col - halfWidth; //extend range of submatrix below col for smoother changes
                if (start < 0)
                {
                    start = 0;
                }

                int stop = col + halfWidth;
                if (stop >= width)
                {
                    stop = width - 1;
                }

                if (col % 8 == 0 && !(col == 0))
                {
                    subMatrix = DataTools.Submatrix(matrix, 0, start, height - 1, stop);
                }

                NormalDist.AverageAndSD(subMatrix, out var av, out var sd);
                if (sd < 0.0001)
                {
                    sd = 0.0001;  //to prevent division by zero
                }

                for (int y = 0; y < height; y++)
                {
                    M[y, col] = (matrix[y, col] - av) / sd;
                }
            }

            return M;
        }

        public static double[,] SubtractAverage_BandWise(double[,] matrix)
        {
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);

            double[,] M = new double[height, width];
            double[,] subMatrix = DataTools.Submatrix(matrix, 0, 0, height - 1, bandWidth);

            for (int col = 0; col < width; col++)
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0)
                {
                    start = 0;
                }

                int stop = col + halfWidth;
                if (stop >= width)
                {
                    stop = width - 1;
                }

                if (col % 8 == 0 && !(col == 0))
                {
                    subMatrix = DataTools.Submatrix(matrix, 0, start, height - 1, stop);
                }

                NormalDist.AverageAndSD(subMatrix, out var av, out var sd);

                //LoggedConsole.WriteLine(0 + "," + start + "," + (height - 1) + "," + stop + "   Threshold " + b + "=" + threshold);

                for (int y = 0; y < height; y++)
                {
                    M[y, col] = matrix[y, col] - av;
                }
            }

            return M;
        }

        /// <summary>
        /// Returns matrix after convolving with Gaussian blur.
        /// The blurring is in 2D, first blurred in x-direction, then in y-direction.
        /// Blurring function is {0.006,0.061, 0.242,0.383,0.242,0.061,0.006}.
        /// </summary>
        public static double[,] GaussianBlur_5cell(double[,] matrix)
        {
            double[] bf = { 0.006, 0.061, 0.242, 0.382, 0.242, 0.061, 0.006 }; //blurring function
            int edge = 4;
            int backtrack = edge - 1;

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);

            // first convolve in x-dimension, i.e. along a row
            double[,] M1 = (double[,])matrix.Clone();
            for (int r = edge; r < height - edge; r++)
            {
                for (int c = edge; c < width - edge; c++)
                {
                    double sum = 0.0;
                    for (int i = 0; i < bf.Length; i++)
                    {
                        sum += bf[i] * matrix[r, c - backtrack + i];
                    }

                    M1[r, c] = sum;
                }
            }

            // then convolve in y-dimension, i.e. along a col
            double[,] M2 = (double[,])M1.Clone();

            // for all rows
            for (int r = edge; r < height - edge; r++)
            {
                // for all cols
                for (int c = edge; c < width - edge; c++)
                {
                    double sum = 0.0;
                    for (int i = 0; i < bf.Length; i++)
                    {
                        sum += bf[i] * M1[r - backtrack + i, c];
                    }

                    M2[r, c] = sum;
                }
            }

            return M2;
        }

        /// <summary>
        /// Detect high intensity / high energy regions in an image using blurring
        /// followed by rules involving positive and negative gradients.
        /// </summary>
        public static double[,] DetectHighEnergyRegions1(double[,] matrix)
        {
            double gradThreshold = 1.2;
            int fWindow = 9;
            int tWindow = 9;
            int bandCount = 16;  // 16 bands, width=512pixels, 32pixels/band
            double lowerShoulder = 0.5;   //used to increase or decrease the threshold from modal value
            double upperShoulder = 0.05;

            double[,] blurM = Blur(matrix, fWindow, tWindow);

            int height = blurM.GetLength(0);
            int width = blurM.GetLength(1);
            double bandWidth = width / (double)bandCount;

            double[,] M = new double[height, width];

            for (int x = 0; x < width; x++)
            {
                M[0, x] = 0.0; //patch in first  time step with zero gradient
            }

            for (int x = 0; x < width; x++)
            {
                M[1, x] = 0.0; //patch in second time step with zero gradient
            }

            // for all bands
            for (int b = 0; b < bandCount; b++)
            {
                int start = (int)((b - 1) * bandWidth);   //extend range of submatrix below b for smoother changes
                if (start < 0)
                {
                    start = 0;
                }

                int stop = (int)((b + 2) * bandWidth);
                if (stop >= width)
                {
                    stop = width - 1;
                }

                double[,] subMatrix = DataTools.Submatrix(blurM, 0, start, height - 1, stop);
                PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out var lowerThreshold, out var upperThreshold);

                //LoggedConsole.WriteLine(0 + "," + start + "," + (height - 1) + "," + stop + "   Threshold " + b + "=" + threshold);

                for (int x = start; x < stop; x++)
                {
                    int state = 0;
                    for (int y = 2; y < height - 1; y++)
                    {
                        double grad1 = blurM[y, x] - blurM[y - 1, x]; //calculate one step gradient
                        double grad2 = blurM[y + 1, x] - blurM[y - 1, x]; //calculate two step gradient

                        if (blurM[y, x] < upperThreshold)
                        {
                            state = 0;
                        }
                        else
                            if (grad1 < -gradThreshold)
                        {
                            state = 0;    // local decrease
                        }
                        else
                                if (grad1 > gradThreshold)
                        {
                            state = 1;     // local increase
                        }
                        else
                                    if (grad2 < -gradThreshold)
                        {
                            state = 0;    // local decrease
                        }
                        else
                                        if (grad2 > gradThreshold)
                        {
                            state = 1;     // local increase
                        }

                        M[y, x] = state;
                    }
                }
            }

            int minRowWidth = 2;
            int minColWidth = 5;
            M = Shapes_RemoveSmall(M, minRowWidth, minColWidth);
            return M;
        }

        /// <summary>
        /// Detect high intensity / high energy regions in an image using blurring
        /// followed by bandwise thresholding.
        /// </summary>
        public static double[,] DetectHighEnergyRegions3(double[,] matrix)
        {
            double lowerShoulder = 0.3;   //used to increase/decrease the intensity threshold from modal value
            double upperShoulder = 0.4;
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;

            int fWindow = 7;
            int tWindow = 7;
            double[,] blurM = Blur(matrix, fWindow, tWindow);

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[,] M = new double[height, width];

            double[,] subMatrix = DataTools.Submatrix(blurM, 0, 0, height - 1, bandWidth);
            PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out var lowerThreshold, out var upperThreshold);

            // for all cols
            for (int col = 0; col < width; col++)
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0)
                {
                    start = 0;
                }

                int stop = col + halfWidth;
                if (stop >= width)
                {
                    stop = width - 1;
                }

                if (col % 8 == 0 && !(col == 0))
                {
                    subMatrix = DataTools.Submatrix(blurM, 0, start, height - 1, stop);
                    PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out lowerThreshold, out upperThreshold);
                }

                for (int y = 0; y < height; y++)
                {
                    if (blurM[y, col] < upperThreshold)
                    {
                        M[y, col] = 0.0;
                    }
                    else
                    {
                        M[y, col] = 1.0;
                    }
                }
            }

            return M;
        }

        public static double[,] Shapes3(double[,] m)
        {
            double[,] m1 = DetectHighEnergyRegions3(m); //detect blobs of high acoustic energy
            double[,] m2 = Shapes_lines(m);

            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] tmpM = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (m2[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (tmpM[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    Oblong.ColumnWidth(m2, x, y, out var colWidth);
                    int x2 = x + colWidth;
                    for (int j = x; j < x2; j++)
                    {
                        tmpM[y, j] = 1.0;
                    }

                    //find distance to nearest object in hi frequency direction
                    // and join the two if within threshold distance
                    int thresholdDistance = 15;
                    int dist = 1;
                    while (x2 + dist < width && m2[y, x2 + dist] == 0)
                    {
                        dist++;
                    }

                    if (x2 + dist < width && dist < thresholdDistance)
                    {
                        for (int d = 0; d < dist; d++)
                        {
                            tmpM[y, x2 + d] = 1.0;
                        }
                    }
                }
            }

            //transfer line objects to output matrix IF they overlap a high energy blob in m1
            double[,] outM = new double[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tmpM[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (outM[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    //int rowWidth; //rowWidth of object
                    //Shape.Row_Width(m2, x, y, out rowWidth);
                    Oblong.ColumnWidth(tmpM, x, y, out var colWidth);
                    int x2 = x + colWidth;

                    //check to see if object is in blob
                    bool overlapsHighEnergy = false;
                    for (int j = x; j < x2; j++)
                    {
                        if (m1[y, j] == 1.0)
                        {
                            overlapsHighEnergy = true;
                            break;
                        }
                    }

                    if (overlapsHighEnergy)
                    {
                        for (int j = x; j < x2; j++)
                        {
                            outM[y, j] = 1.0;
                        }
                    }
                }
            }

            outM = FillGaps(outM);
            int minRowWidth = 2;
            int minColWidth = 4;
            outM = Shapes_RemoveSmallUnattached(outM, minRowWidth, minColWidth);
            return outM;
        }

        public static ArrayList Shapes4(double[,] m)
        {
            double[,] m1 = DetectHighEnergyRegions3(m); //binary matrix showing areas of high acoustic energy
            double[,] m2 = Shapes_lines(m); //binary matrix showing high energy lines

            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] tmpM = new double[height, width];
            ArrayList shapes = new ArrayList();

            //transfer m2 lines spectrogram to temporary matrix and merge adjacent high energy objects
            // row at a time
            for (int y = 0; y < height; y++)
            {
                // transfer values to tmpM
                for (int x = 0; x < width; x++)
                {
                    if (m2[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (tmpM[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    Oblong.ColumnWidth(m2, x, y, out var colWidth);
                    int x2 = x + colWidth - 1;
                    for (int j = x; j < x2; j++)
                    {
                        tmpM[y, j] = 1.0;
                    }

                    //find distance to nearest object in hi frequency direction
                    // and join the two if within threshold distance
                    int thresholdDistance = 10;
                    int dist = 1;
                    while (x2 + dist < width && m2[y, x2 + dist] == 0)
                    {
                        dist++;
                    }

                    if (x2 + dist < width && dist < thresholdDistance)
                    {
                        for (int d = 0; d < dist; d++)
                        {
                            tmpM[y, x2 + d] = 1.0;
                        }
                    }
                }

                y++; //only even rows
            }

            //transfer line objects to output matrix IF they overlap a high energy region in m1
            int objectCount = 0;
            double[,] outM = new double[height, width];
            for (int y = 0; y < height - 2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tmpM[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (outM[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    Oblong.ColumnWidth(tmpM, x, y, out var colWidth);

                    int x2 = x + colWidth;

                    //check to see if object is in high energy region
                    bool overlapsHighEnergy = false;
                    for (int j = x; j < x2; j++)
                    {
                        if (m1[y + 1, j] == 1.0 || m1[y, j] == 1.0)
                        {
                            overlapsHighEnergy = true;
                            break;
                        }
                    }

                    if (overlapsHighEnergy)
                    {
                        shapes.Add(new Oblong(y, x, y + 1, x2));
                        objectCount++;
                        for (int j = x; j < x2; j++)
                        {
                            outM[y, j] = 1.0;
                        }

                        for (int j = x; j < x2; j++)
                        {
                            tmpM[y, j] = 0.0;
                        }

                        for (int j = x; j < x2; j++)
                        {
                            outM[y + 1, j] = 1.0;
                        }

                        for (int j = x; j < x2; j++)
                        {
                            tmpM[y + 1, j] = 0.0;
                        }
                    }
                }
            }

            //NOW DO SHAPE MERGING TO REDUCE NUMBERS
            LoggedConsole.WriteLine("Object Count 1 =" + objectCount);
            int dxThreshold = 25; //upper limit on centroid displacement - set higher for fewer bigger shapes
            double widthRatio = 5.0; //upper limit on difference in shape width - set higher for fewer bigger shapes
            shapes = Oblong.MergeShapesWithAdjacentRows(shapes, dxThreshold, widthRatio);
            LoggedConsole.WriteLine("Object Count 2 =" + shapes.Count);

            //shapes = Shape.RemoveEnclosedShapes(shapes);
            shapes = Oblong.RemoveOverlappingShapes(shapes);
            int minArea = 14;
            shapes = Oblong.RemoveSmall(shapes, minArea);
            LoggedConsole.WriteLine("Object Count 3 =" + shapes.Count);
            return shapes;
        }

        /// <summary>
        /// Returns an ArrayList of rectabgular shapes that represent acoustic events / syllables in the sonogram.
        /// </summary>
        public static ArrayList Shapes5(double[,] m)
        {
            //get binary matrix showing high energy regions
            int fWindow = 5;
            int tWindow = 3;
            double[,] tmp = Blur(m, fWindow, tWindow);
            double threshold = 0.2;
            double[,] m1 = DataTools.Threshold(tmp, threshold);

            //get binary matrix showing high energy lines
            double[,] m2 = Convolve(tmp, Kernal.HorizontalLine5);
            threshold = 0.2;
            m2 = DataTools.Threshold(m2, threshold);

            //prepare to extract acoustic events or shapes
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] tmpM = new double[height, width];
            ArrayList shapes = new ArrayList();

            //transfer m2 lines spectrogram to temporary matrix and join adjacent high energy objects
            // row at a time
            for (int y = 0; y < height; y++)
            {
                // transfer values to tmpM
                for (int x = 0; x < width; x++)
                {
                    if (m2[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (tmpM[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    Oblong.ColumnWidth(m2, x, y, out var colWidth);
                    int x2 = x + colWidth - 1;
                    for (int j = x; j < x2; j++)
                    {
                        tmpM[y, j] = 1.0;
                    }

                    //find distance to nearest object in hi frequency direction
                    // and join the two if within threshold distance
                    int thresholdDistance = 10;
                    int dist = 1;
                    while (x2 + dist < width && m2[y, x2 + dist] == 0)
                    {
                        dist++;
                    }

                    if (x2 + dist < width && dist < thresholdDistance)
                    {
                        for (int d = 0; d < dist; d++)
                        {
                            tmpM[y, x2 + d] = 1.0;
                        }
                    }
                }

                y++; //only even rows
            }

            //transfer line objects to output matrix IF they overlap a high energy region in m1
            int objectCount = 0;
            double[,] outM = new double[height, width];
            for (int y = 0; y < height - 2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tmpM[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (outM[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    Oblong.ColumnWidth(tmpM, x, y, out var colWidth);

                    int x2 = x + colWidth;

                    //check to see if object is in high energy region
                    bool overlapsHighEnergy = false;
                    for (int j = x; j < x2; j++)
                    {
                        if (m1[y + 1, j] == 1.0 || m1[y, j] == 1.0)
                        {
                            overlapsHighEnergy = true;
                            break;
                        }
                    }

                    if (overlapsHighEnergy)
                    {
                        shapes.Add(new Oblong(y, x, y + 1, x2));
                        objectCount++;
                        for (int j = x; j < x2; j++)
                        {
                            outM[y, j] = 1.0;
                        }

                        for (int j = x; j < x2; j++)
                        {
                            tmpM[y, j] = 0.0;
                        }

                        for (int j = x; j < x2; j++)
                        {
                            outM[y + 1, j] = 1.0;
                        }

                        for (int j = x; j < x2; j++)
                        {
                            tmpM[y + 1, j] = 0.0;
                        }
                    }
                }
            }

            //NOW DO SHAPE MERGING TO REDUCE NUMBERS
            LoggedConsole.WriteLine("Object Count 1 =" + objectCount);
            int dxThreshold = 25; //upper limit on centroid displacement - set higher for fewer bigger shapes
            double widthRatio = 4.0; //upper limit on difference in shape width - set higher for fewer bigger shapes
            shapes = Oblong.MergeShapesWithAdjacentRows(shapes, dxThreshold, widthRatio);
            LoggedConsole.WriteLine("Object Count 2 =" + shapes.Count);
            shapes = Oblong.RemoveEnclosedShapes(shapes);

            //shapes = Shape.RemoveOverlappingShapes(shapes);
            int minArea = 30;
            shapes = Oblong.RemoveSmall(shapes, minArea);
            LoggedConsole.WriteLine("Object Count 3 =" + shapes.Count);
            return shapes;
        }

        /// <summary>
        /// Returns a binary matrix containing high energy lines in the oriignal spectrogram.
        /// </summary>
        public static double[,] Shapes_lines(double[,] matrix)
        {
            double threshold = 0.3;

            int fWindow = 5;
            int tWindow = 3;
            double[,] tmpM = Blur(matrix, fWindow, tWindow);

            //double[,] tmpM = ImageTools.Convolve(matrix, Kernal.HorizontalLine5);
            tmpM = Convolve(tmpM, Kernal.HorizontalLine5);

            //tmpM = ImageTools.Convolve(tmpM, Kernal.HorizontalLine5);

            //int height = matrix.GetLength(0);
            //int width = matrix.GetLength(1);
            //double[,] M = new double[height, width];
            double[,] M = DataTools.Threshold(tmpM, threshold);
            return M;
        }

        /// <summary>
        /// Returns a binary matrix containing high energy lines in the original spectrogram
        /// calculates the threshold bandwise.
        /// </summary>
        public static double[,] Shapes_lines_bandwise(double[,] matrix)
        {
            double lowerShoulder = 0.7;   //used to increase or decrease the threshold from modal value
            double upperShoulder = 0.1;
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;

            int fWindow = 3;
            int tWindow = 3;
            double[,] tmpM = Blur(matrix, fWindow, tWindow);
            tmpM = Convolve(tmpM, Kernal.HorizontalLine5);
            tmpM = Convolve(tmpM, Kernal.HorizontalLine5);

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[,] M = new double[height, width];

            double[,] subMatrix = DataTools.Submatrix(tmpM, 0, 0, height - 1, bandWidth);
            PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out var lowerThreshold, out var upperThreshold);

            //  for all cols
            for (int col = 2; col < width; col++)
            {
                int start = col - halfWidth; //extend range of submatrix below col for smoother changes
                if (start < 0)
                {
                    start = 0;
                }

                int stop = col + halfWidth;
                if (stop >= width)
                {
                    stop = width - 1;
                }

                if (col % 8 == 0 && !(col == 0))
                {
                    subMatrix = DataTools.Submatrix(tmpM, 0, start, height - 1, stop);
                    PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out lowerThreshold, out upperThreshold);
                }

                for (int y = 1; y < height - 1; y++)
                {
                    bool evenRow = y % 2 == 0;
                    if (tmpM[y, col] > upperThreshold)
                    {
                        M[y, col] = 1;
                        if (evenRow)
                        {
                            M[y + 1, col] = 1;
                        }
                        else
                        {
                            M[y - 1, col] = 1;
                        }

                        //fill in gaps
                        if (M[y, col - 2] == 1.0 && M[y, col - 1] == 0.0)
                        {
                            M[y, col - 1] = 1;
                        }
                    }
                }
            }

            int minRowWidth = 2;
            int minColWidth = 5;
            M = Shapes_RemoveSmall(M, minRowWidth, minColWidth);
            return M;
        }

        public static double[,] Shapes_RemoveSmall(double[,] m, int minRowWidth, int minColWidth)
        {
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] M = new double[height, width];

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (m[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (M[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    Oblong.Row_Width(m, x, y, out var rowWidth);
                    Oblong.ColumnWidth(m, x, y, out var colWidth);
                    bool sizeOK = rowWidth >= minRowWidth && colWidth >= minColWidth;

                    if (sizeOK)
                    {
                        for (int c = 0; c < colWidth; c++)
                        {
                            for (int r = 0; r < minRowWidth; r++)
                            {
                                M[y + r, x + c] = 1.0;
                            }
                        }
                    }

                    y += rowWidth - 1;
                }
            }

            //M = m;

            return M;
        }

        public static double[,] Shapes_RemoveSmallUnattached(double[,] m, int minRowWidth, int minColWidth)
        {
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] M = new double[height, width];

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height - 3; y++)
                {
                    if (m[y, x] == 0.0)
                    {
                        continue; //nothing here
                    }

                    if (M[y, x] == 1.0)
                    {
                        continue; //already have something here
                    }

                    Oblong.Row_Width(m, x, y, out var rowWidth);
                    Oblong.ColumnWidth(m, x, y, out var colWidth);
                    bool sizeOK = false;
                    if (rowWidth >= minRowWidth && colWidth >= minColWidth)
                    {
                        sizeOK = true;
                    }

                    //now check if object is unattached to other object
                    bool attachedOK = false;
                    for (int j = x; j < x + colWidth; j++)
                    {
                        if (m[y - 1, j] == 1.0 || /*(m[y + 1, j] == 1.0) ||*/ m[y + 2, j] == 1.0 || m[y + 3, j] == 1.0)
                        {
                            attachedOK = true;
                            break;
                        }
                    }

                    //attachedOK = true;
                    if (sizeOK && attachedOK)
                    {
                        for (int c = 0; c < colWidth; c++)
                        {
                            //Shape.Row_Width(m, x+c, y, out rowWidth);
                            for (int r = 0; r < minRowWidth; r++)
                            {
                                M[y + r, x + c] = 1.0;
                            }
                        }
                    }
                }
            }

            //M = m;

            return M;
        }

        public static double[,] FillGaps(double[,] m)
        {
            double coverThreshold = 0.7;
            int cNH = 4; //neighbourhood
            int rNH = 11;

            int height = m.GetLength(0);
            int width = m.GetLength(1);

            //double[,] M = new double[height, width];
            int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);

            //LoggedConsole.WriteLine("area=" + area);

            for (int x = cNH; x < width - cNH; x++)
            {
                for (int y = rNH; y < height - rNH; y++)
                {
                    double sum = 0.0;
                    for (int r = -rNH; r < rNH; r++)
                    {
                        for (int c = -cNH; c < cNH; c++)
                        {
                            sum += m[y + r, x + c];
                        }
                    }

                    double cover = sum / area;

                    if (cover >= coverThreshold)
                    {
                        m[y, x] = 1.0;
                        m[y - 1, x] = 1.0;
                        m[y + 1, x] = 1.0;

                        //m[y - 2, x] = 1.0;
                        //m[y + 2, x] = 1.0;
                    }
                }
            }

            return m;
        }

        /// <summary>
        /// returns a palette of a variety of coluor.
        /// Used for displaying clusters identified by colour.
        /// </summary>
        public static List<Pen> GetRedGradientPalette()
        {
            var pens = new List<Pen>();
            for (byte c = 0; c < (byte)255; c++)
            {
                pens.Add(new Pen(Color.FromRgb(255, c, c), 1f));
            }

            return pens;
        }

        /// <summary>
        /// returns a palette of a variety of coluor.
        /// Used for displaying clusters identified by colour.
        /// </summary>
        public static List<Pen> GetColorPalette(int paletteSize)
        {
            var pens = new List<Pen>();
            pens.Add(new Pen(Color.Pink, 1f));
            pens.Add(new Pen(Color.Red, 1f));
            pens.Add(new Pen(Color.Orange, 1f));
            pens.Add(new Pen(Color.Yellow, 1f));
            pens.Add(new Pen(Color.Green, 1f));
            pens.Add(new Pen(Color.Blue, 1f));
            pens.Add(new Pen(Color.Crimson, 1f));
            pens.Add(new Pen(Color.LimeGreen, 1f));
            pens.Add(new Pen(Color.Tomato, 1f));

            //pens.Add(new Pen(Color.Indigo, 1));
            pens.Add(new Pen(Color.Violet, 1f));

            //now add random coloured pens
            int max = 255;
            var rn = new RandomNumber();
            for (int c = 10; c <= paletteSize; c++)
            {
                byte rd = (byte)rn.GetInt(max);
                byte gr = (byte)rn.GetInt(max);
                byte bl = (byte)rn.GetInt(max);
                pens.Add(new Pen(Color.FromRgb(rd, gr, bl), 1f));
            }

            return pens;
        }

        /// <summary>
        /// Returns an image of an array of the passed colour patches.
        /// </summary>
        public static Image DrawColourChart(int width, int ht, Color[] colorArray)
        {
            if (colorArray == null || colorArray.Length == 0)
            {
                return null;
            }

            int colourCount = colorArray.Length;
            int patchWidth = width / colourCount;
            int maxPathWidth = (int)(ht / 1.5);
            if (patchWidth > maxPathWidth)
            {
                patchWidth = maxPathWidth;
            }
            else if (width < 3)
            {
                width = 3;
            }

            Image colorScale = new Image<Rgb24>(colourCount * patchWidth, ht);

            int offset = width + 1;
            if (width < 5)
            {
                offset = width;
            }

            Image<Rgb24> colorBmp = new Image<Rgb24>(width - 1, ht);

            //Color c;
            int x = 0;

            for (int i = 0; i < colourCount; i++)
            {
                //c = Color.FromRgb(250, 15, 250);
                colorBmp.Mutate(o => o.Fill(colorArray[i]));

                //int x = 0;
                colorScale.Mutate(o => o.DrawImage(colorBmp, new Point(x, 0), 1f)); //dra

                //c = Color.FromRgb(250, 15, 15);
                //gr2.Clear(c);
                x += patchWidth;
                colorScale.Mutate(o => o.DrawImage(colorBmp, new Point(x, 0), 1f)); //dra
            }

            return colorScale;
        }

        /// <summary>
        /// returns a colour array of 256 gray scale values.
        /// </summary>
        public static Color[] GrayScale()
        {
            int max = byte.MaxValue;
            Color[] grayScale = new Color[256];

            // take care for byte arithmetic overflow here
            for (var c = 0; c <= max; c++)
            {
                var b = (byte)c;
                grayScale[c] = Color.FromRgb(b, b, b);
            }

            return grayScale;
        }

        /// <summary>
        /// returns a colour array of 256 green scale values.
        /// </summary>
        public static Color[] GreenScale()
        {
            int max = byte.MaxValue;
            Color[] greenScale = new Color[256];

            // take care for byte arithmetic overflow here
            for (var c = 0; c <= max; c++)
            {
                greenScale[c] = Color.FromRgb(0, (byte)c, 0);
            }

            return greenScale;
        }

        /// <summary>
        /// Normalises the matrix between zero and one.
        /// Then draws the reversed matrix and saves image to passed path.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static void DrawReversedMDNMatrix(Matrix<double> matrix, string pathName)
        {
            double[,] matrix1 = matrix.ToArray();
            Image bmp = DrawReversedMatrix(matrix1);
            bmp.Save(pathName);
        }

        /// <summary>
        /// Normalises the matrix between zero and one.
        /// Then draws the reversed matrix and saves image to passed path.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static void DrawReversedMatrix(double[,] matrix, string pathName)
        {
            Image bmp = DrawReversedMatrix(matrix);
            bmp.Save(pathName);
        }

        public static double[,] ByteMatrix2DoublesMatrix(byte[,] mb)
        {
            int rows = mb.GetLength(0); //number of rows
            int cols = mb.GetLength(1); //number

            double[,] matrix = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    matrix[r, c] = mb[r, c];
                }
            }

            return matrix;
        }

        public static void DrawMatrix(byte[,] mBytes, string pathName)
        {
            double[,] matrix = ByteMatrix2DoublesMatrix(mBytes);
            var bmp = DrawNormalisedMatrix(matrix);
            bmp.Save(pathName);
        }

        /// <summary>
        /// Draws matrix and save image.
        /// </summary>
        /// <param name="vector">the data.</param>
        public static void DrawMatrix(double[] vector, string pathName)
        {
            double[,] matrix = new double[1, vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                matrix[0, i] = vector[i];
            }

            Image<Rgb24> bmp = DrawNormalisedMatrix(matrix);
            bmp.Save(pathName);
        }

        /// <summary>
        /// Draws matrix and save image.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static void DrawMatrix(double[,] matrix, string pathName)
        {
            Image<Rgb24> bmp = DrawNormalisedMatrix(matrix);
            bmp.Save(pathName);
        }

        public static void DrawMatrix(double[,] matrix, double lowerBound, double upperBound, string pathName)
        {
            Image<Rgb24> bmp = DrawNormalisedMatrix(matrix, lowerBound, upperBound);
            bmp.Save(pathName);
        }

        /// <summary>
        /// Draws matrix after first normalising the data.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static Image<Rgb24> DrawNormalisedMatrix(double[,] matrix)
        {
            double[,] norm = DataTools.normalise(matrix);
            return DrawMatrixWithoutNormalisation(norm);
        }

        public static Image<Rgb24> DrawNormalisedMatrix(double[,] matrix, double lowerBound, double upperBound)
        {
            double[,] norm = DataTools.NormaliseInZeroOne(matrix, lowerBound, upperBound);
            return DrawMatrixWithoutNormalisation(norm);
        }

        /// <summary>
        /// Draws matrix after first normalising the data.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static Image<Rgb24> DrawReversedMatrix(double[,] matrix)
        {
            double[,] norm = DataTools.normalise(matrix);
            return DrawReversedMatrixWithoutNormalisation(norm);
        }

        /// <summary>
        /// Draws matrix without normkalising the values in the matrix.
        /// Assume some form of normalisation already done.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static Image<Rgb24> DrawReversedMatrixWithoutNormalisation(double[,] matrix)
        {
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number

            Color[] grayScale = GrayScale();

            Image<Rgb24> bmp = new Image<Rgb24>(cols, rows);
            int greyId = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (double.IsNaN(matrix[r, c]))
                    {
                        greyId = 128; //want NaN values in gray,
                    }
                    else
                    {
                        greyId = (int)Math.Floor(matrix[r, c] * 255);
                        if (greyId < 0)
                        {
                            greyId = 0;
                        }
                        else
                        {
                            if (greyId > 255)
                            {
                                greyId = 255;
                            }
                        }

                        greyId = 255 - greyId; // reverse image - want high values in black, low values in white
                    }

                    bmp[c, r] = grayScale[greyId];
                }
            }

            return bmp;
        }

        /// <summary>
        /// Draws matrix without normkalising the values in the matrix.
        /// Assume some form of normalisation already done.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static Image<Rgb24> DrawMatrixWithoutNormalisation(double[,] matrix)
        {
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number

            Color[] grayScale = GrayScale();

            var bmp = new Image<Rgb24>(cols, rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int greyId = (int)Math.Floor(matrix[r, c] * 255);

                    if (greyId < 0)
                    {
                        greyId = 0;
                    }
                    else
                    {
                        if (greyId > 255)
                        {
                            greyId = 255;
                        }
                    }

                    bmp[c, r] = grayScale[greyId];
                }
            }

            return bmp;
        }

        public static Image<Rgb24> DrawMatrixWithoutNormalisationGreenScale(double[,] matrix)
        {
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number

            Color[] grayScale = GreenScale();

            var bmp = new Image<Rgb24>(cols, rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int greyId = (int)Math.Floor(matrix[r, c] * 255);

                    if (greyId < 0)
                    {
                        greyId = 0;
                    }
                    else
                    {
                        if (greyId > 255)
                        {
                            greyId = 255;
                        }
                    }

                    bmp[c, r] = grayScale[greyId];
                } //end all columns
            } //end all rows

            return bmp;
        }

        public static Image<Rgb24> DrawRGBMatrix(double[,] matrixR, double[,] matrixG, double[,] matrixB)
        {
            int rows = matrixR.GetLength(0); //number of rows
            int cols = matrixR.GetLength(1); //number

            var bmp = new Image<Rgb24>(cols, rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int R = (int)Math.Floor(matrixR[r, c] * 255);
                    if (R < 0)
                    {
                        R = 0;
                    }
                    else
                    {
                        if (R > 255)
                        {
                            R = 255;
                        }
                    }

                    int G = (int)Math.Floor(matrixG[r, c] * 255);
                    if (G < 0)
                    {
                        G = 0;
                    }
                    else
                    {
                        if (G > 255)
                        {
                            G = 255;
                        }
                    }

                    int B = (int)Math.Floor(matrixB[r, c] * 255);
                    if (B < 0)
                    {
                        B = 0;
                    }
                    else
                    {
                        if (B > 255)
                        {
                            B = 255;
                        }
                    }

                    bmp[c, r] = Color.FromRgb((byte)R, (byte)G, (byte)B);
                }
            }

            return bmp;
        }

        /// <summary>
        /// Draws horizontal gridlines on Image.
        /// </summary>
        public static Image<Rgb24> DrawYaxisScale(Image<Rgb24> image, int scaleWidth, double yInterval, double yTicInterval, int yOffset)
        {
            int ticCount = (int)(image.Height / yTicInterval);
            var pen = new Pen(Color.White, 1f);
            var font = Drawing.Arial10;

            image.Mutate(g =>
            {

                for (int i = 1; i <= ticCount; i++)
                {
                    int y1 = image.Height - (int)(i * yTicInterval) + yOffset;

                    g.DrawLine(pen, 0, y1, image.Width - 1, y1);
                    string value = Math.Round(yInterval * i).ToString();
                    g.DrawTextSafe(value, font, Color.White, new PointF(2, y1 + 1));
                }
            });

            Image<Rgb24> yAxisImage = new Image<Rgb24>(scaleWidth, image.Height);
            yAxisImage.Mutate(g =>
            {
                pen = new Pen(Color.Black, 1.0f);
                g.Fill(Color.LightGray);
                for (int i = 1; i <= ticCount; i++)
                {
                    int y1 = yAxisImage.Height - (int)(i * yTicInterval) + yOffset;
                    g.DrawLine(pen, 0, y1, scaleWidth - 1, y1);
                    g.DrawLine(pen, 0, y1 - 1, scaleWidth - 1, y1 - 1);
                }

                g.DrawRectangle(pen, 0, 0, scaleWidth - 1, image.Height - 1);
            });

            var array = new Image<Rgb24>[] { yAxisImage, image };
            return CombineImagesInLine(array);
        }

        /// <summary>
        /// assumes the y-axis has already been drawn already.
        /// Therefore require an offset at bottom left to accommodate the width of the y-axis.
        /// </summary>
        public static Image<Rgb24> DrawXaxisScale(Image<Rgb24> image, int scaleHeight, double xInterval, double xTicInterval, int yScalePadding, int xOffset)
        {
            int ticCount = (int)((image.Width - scaleHeight) / xTicInterval);
            var pen = new Pen(Color.White, 1f);
            var font = Drawing.Arial10;

            image.Mutate(g =>
            {
                // draw on the grid lines
                for (int i = 1; i <= ticCount; i++)
                {
                    int x1 = yScalePadding + (int)(i * xTicInterval) + xOffset;
                    g.DrawLine(pen, x1, 0, x1, image.Height - 1);
                }
            });

            // create the x-axis scale
            var scaleImage = new Image<Rgb24>(image.Width, scaleHeight);
            scaleImage.Mutate(g =>
            {
                pen = new Pen(Color.Black, 1f);
                g.Clear(Color.LightGray);
                for (int i = 0; i <= ticCount; i++)
                {
                    int x1 = yScalePadding + (int)(i * xTicInterval) + xOffset;
                    g.DrawLine(pen, x1, 0, x1, scaleHeight - 1);

                    //g.DrawLine(pen, x1 + 1, 0, x1 + 1, scaleHeight - 1);

                    string value = Math.Round(xInterval * i).ToString();
                    g.DrawTextSafe(value, font, Color.Black, new PointF(x1, 0));
                }

                g.DrawRectangle(pen, 0, 0, image.Width - 1, scaleHeight - 1);
            });

            var array = new[] { image, scaleImage };
            return CombineImagesVertically(array);
        }

        /// <summary>
        /// Draws matrix but automatically determines the scale to fit 1000x1000 pixel image.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static Image<Rgb24> DrawMatrix(double[,] matrix, bool doScale)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int maxXpixels = cols;
            int yPixelsPerCell = 1;
            int xPixelsPerCell = 1;
            if (doScale)
            {
                var maxYpixels = 1000;
                maxXpixels = 2500;
                yPixelsPerCell = maxYpixels / rows;
                xPixelsPerCell = maxXpixels / cols;
                if (yPixelsPerCell == 0)
                {
                    yPixelsPerCell = 1;
                }

                if (xPixelsPerCell == 0)
                {
                    xPixelsPerCell = 1;
                }
            }

            int yPixels = yPixelsPerCell * rows;
            int xPixels = xPixelsPerCell * cols;

            Color[] grayScale = GrayScale();

            var bmp = new Image<Rgb24>(xPixels, yPixels);

            double[,] norm = DataTools.normalise(matrix);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int greyId = (int)Math.Floor(norm[r, c] * 255);
                    if (greyId < 0)
                    {
                        greyId = 0;
                    }

                    int xOffset = xPixelsPerCell * c;
                    int yOffset = yPixelsPerCell * r;
                    for (int x = 0; x < xPixelsPerCell; x++)
                    {
                        for (int y = 0; y < yPixelsPerCell; y++)
                        {
                            bmp[xOffset + x, yOffset + y] = grayScale[greyId];
                        }
                    }
                }
            }

            return bmp;
        }

        public static void DrawMatrixInColour(double[,] matrix, string pathName, bool doScale)
        {
            Image image = DrawMatrixInColour(matrix, doScale);
            image.Save(pathName);
        }

        /// <summary>
        /// Draws colour matrix but automatically determines the scale to fit 1000x1000 pixel image.
        /// </summary>
        /// <param name="matrix">the data.</param>
        public static Image DrawMatrixInColour(double[,] matrix, bool doScale)
        {
            int xscale = 10;
            int yscale = 5;
            return DrawMatrixInColour(matrix, xscale, yscale);
        }

        public static Image DrawVectorInColour(double[] vector, int cellWidth)
        {
            double[] norm = DataTools.normalise(vector);

            int bottomColour = 1;     // to avoid using the reds
            int topColour = 250;      // to avoid using the magentas
            int hueRange = topColour - bottomColour;

            int rows = vector.Length;
            int cols = 1;
            int yPixelCount = cellWidth * rows;
            int xPixelCount = cellWidth;
            var bmp = new Image<Rgb24>(xPixelCount, yPixelCount);

            // Can comment next two lines if want black
            var converter = new ColorSpaceConverter();
            bmp.Mutate(g =>
            {
                g.Clear(Color.Gray);

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int xOffset = cellWidth * c;
                        int yOffset = cellWidth * r;

                        // use HSV colour space
                        int hue = topColour - (int)Math.Floor(hueRange * norm[r]);

                        // double saturation = 0.75 + (norm[r, c] * 0.25);
                        // double saturation = norm[r, c] * 0.5;
                        // double saturation = (1 - norm[r, c]) * 0.5;
                        double saturation = 1.0;
                        var myHsv = new Hsv(hue, (float)saturation, 1.0f);
                        var myRgb = converter.ToRgb(myHsv);

                        // use black as background zero colour rather than blue
                        if (myRgb.B < 255.0)
                        {
                            var colour = new Rgb(myRgb.R, myRgb.G, myRgb.B / 3);
                            for (int x = 0; x < cellWidth; x++)
                            {
                                for (int y = 0; y < cellWidth; y++)
                                {
                                    bmp[xOffset + x, yOffset + y] = colour;
                                }
                            }
                        }
                    }
                }
            });

            return bmp;
        }

        public static Image DrawVectorInGrayScale(double[] vector, int cellWidth, int cellHeight)
        {
            var norm = DataTools.normalise(vector);
            var bmp = DrawVectorInGrayScaleWithoutNormalisation(norm, cellWidth, cellHeight, true);
            return bmp;
        }

        /// <summary>
        /// This method assumes that the vector has already been normalised by some means such that all values lie between 0.0 and 1.0.
        /// </summary>
        /// <param name="vector">the vector of normalised values.</param>
        /// <param name="cellWidth">the width of the image.</param>
        /// <param name="cellHeight">the height of each image row.</param>
        public static Image<Rgb24> DrawVectorInGrayScaleWithoutNormalisation(double[] vector, int cellWidth, int cellHeight, bool reverse)
        {
            int rows = vector.Length;
            int cols = 1;
            int yPixelCount = cellHeight * rows;
            int xPixelCount = cellWidth;
            var bmp = new Image<Rgb24>(xPixelCount, yPixelCount);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int xOffset = cellWidth * c;
                    int yOffset = cellHeight * r;

                    // use reverse gray scale i.e. white = low value, black = high value
                    byte gray;
                    if (reverse)
                    {
                        gray = (byte)Math.Floor(255 * vector[r]);
                    }
                    else
                    {
                        gray = (byte)(255 - (int)Math.Floor(255 * vector[r]));
                    }

                    var colour = Color.FromRgb(gray, gray, gray);
                    for (int x = 0; x < xPixelCount; x++)
                    {
                        for (int y = 0; y < cellHeight; y++)
                        {
                            bmp[xOffset + x, yOffset + y] = colour;
                        }
                    }
                }
            }

            return bmp;
        }

        public static Image DrawMatrixInColour(double[,] matrix, int xPixelsPerCell, int yPixelsPerCell)
        {
            double[,] norm = DataTools.normalise(matrix);

            int bottomColour = 1;     // to avoid using the reds
            int topColour = 250;      // to avoid using the magentas
            int hueRange = topColour - bottomColour;

            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1);
            int yPixelCount = yPixelsPerCell * rows;
            int xPixelCount = xPixelsPerCell * cols;
            var bmp = new Image<Rgb24>(xPixelCount, yPixelCount);

            var converter = new ColorSpaceConverter();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int xOffset = xPixelsPerCell * c;
                    int yOffset = yPixelsPerCell * r;

                    // use HSV colour space
                    // int hue = bottomColour + (int)Math.Floor(hueRange * norm[r, c]);
                    int hue = topColour - (int)Math.Floor(hueRange * norm[r, c]);

                    // double saturation = 0.75 + (norm[r, c] * 0.25);
                    // double saturation = norm[r, c] * 0.5;
                    // double saturation = (1 - norm[r, c]) * 0.5;
                    double saturation = 1.0;
                    var myHsv = new Hsv(hue, (float)saturation, 1.0f);
                    var myRgb = converter.ToRgb(myHsv);

                    // use black as background zero colour rather than blue
                    if (myRgb.B < 255.0)
                    {
                        var colour = new Rgb(myRgb.R, myRgb.G, myRgb.B / 3);
                        for (int x = 0; x < xPixelsPerCell; x++)
                        {
                            for (int y = 0; y < yPixelsPerCell; y++)
                            {
                                bmp[xOffset + x, yOffset + y] = colour;
                            }
                        }
                    }
                }
            }

            return bmp;
        }

        public static void DrawMatrix(double[,] matrix, int cellXpixels, int cellYpixels, string pathName)
        {
            var bmp = ImageTools.DrawMatrixInGrayScale(matrix, cellXpixels, cellYpixels, true);
            bmp.Save(pathName);
        }

        /// <summary>
        /// Draws matrix according to user defined scale.
        /// </summary>
        /// <param name="matrix">the data.</param>
        /// <param name="xPixelsPerCell">X axis scale - pixels per cell.</param>
        /// <param name="yPixelsPerCell">Y axis scale - pixels per cell.</param>
        /// <param name="reverse">determines black on white or white on black.</param>
        public static Image<Rgb24> DrawMatrixInGrayScale(double[,] matrix, int xPixelsPerCell, int yPixelsPerCell, bool reverse)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int yPixels = yPixelsPerCell * rows;
            int xPixels = xPixelsPerCell * cols;
            Color[] grayScale = GrayScale();
            var bmp = new Image<Rgb24>(xPixels, yPixels);

            double[,] norm = DataTools.normalise(matrix);
            for (int r = 0; r < rows; r++)
            {
                int colorId;
                for (int c = 0; c < cols; c++)
                {
                    if (reverse)
                    {
                        colorId = (int)Math.Floor(norm[r, c] * 255);
                    }
                    else
                    {
                        colorId = 255 - (int)Math.Floor(norm[r, c] * 255);
                    }

                    int xOffset = xPixelsPerCell * c;
                    int yOffset = yPixelsPerCell * r;

                    for (int x = 0; x < xPixelsPerCell; x++)
                    {
                        for (int y = 0; y < yPixelsPerCell; y++)
                        {
                            bmp[xOffset + x, yOffset + y] = grayScale[colorId];
                        }
                    }
                }
            }

            return bmp;
        }

        /// <summary>
        /// Stacks the passed images one on top of the other.
        /// Assumes that all images have the same width.
        /// </summary>
        public static Image<T> CombineImagesVertically<T>(List<Image<T>> list)
            where T : unmanaged, IPixel<T>
        {
            return CombineImagesVertically(null, list.ToArray());
        }

        public static Image<T> CombineImagesVertically<T>(List<Image<T>> list, int maxWidth)
            where T : unmanaged, IPixel<T>
        {
            return CombineImagesVertically(maxWidth, list.ToArray());
        }

        public static Image<T> CombineImagesVertically<T>(params Image<T>[] images)
            where T : unmanaged, IPixel<T>
        {
            return CombineImagesVertically<T>(maximumWidth: null, images);
        }

        /// <summary>
        /// Stacks the passed images one on top of the other.
        /// </summary>
        /// <param name="maximumWidth">The maximum width of the output images.</param>
        /// <param name="array">An array of Image.</param>
        /// <returns>A single image.</returns>
        public static Image<T> CombineImagesVertically<T>(int? maximumWidth, Image<T>[] array)
            where T : unmanaged, IPixel<T>
        {
            int width = 0;
            int compositeHeight = 0;
            foreach (var image in array)
            {
                if (image == null)
                {
                    continue;
                }

                compositeHeight += image.Height;

                if (image.Width > width)
                {
                    width = image.Width;
                }
            }

            // check width is not over the max
            if (width > maximumWidth)
            {
                width = (int)maximumWidth;
            }

            var compositeBmp = Drawing.NewImage<T>(width, compositeHeight, Color.DarkGray);
            int yOffset = 0;

            // TODO: Fix at some point. Using default configuration with parallelism there is some kind of batching bug that causes a crash
            compositeBmp.Mutate(Drawing.NoParallelConfiguration, gr =>
            {
                foreach (var image in array)
                {
                    if (image == null)
                    {
                        continue;
                    }

                    gr.DrawImage(image, new Point(0, yOffset), 1); //draw in the top image
                    yOffset += image.Height;
                }
            });

            return compositeBmp;
        }

        /// <summary>
        /// Stacks the passed images one on top of the other.
        /// </summary>
        /// <param name="list">A list of images.</param>
        /// <returns>A single image.</returns>
        public static Image<T> CombineImagesInLine<T>(List<Image<T>> list)
            where T : unmanaged, IPixel<T>
        {
            return CombineImagesInLine(list.ToArray());
        }

        /// <summary>
        /// Stacks the passed images one on top of the other.
        /// Assumes that all images have the same width.
        /// </summary>
        /// <param name="images">An array of images.</param>
        /// <returns>A single image.</returns>
        public static Image<T> CombineImagesInLine<T>(params Image<T>[] images)
            where T : unmanaged, IPixel<T>
        {
            return CombineImagesInLine(Color.Black, images);
        }

        public static Image<T> CombineImagesInLine<T>(Color fill, params Image<T>[] images)
            where T : unmanaged, IPixel<T>
        {
            var maxHeight = images.Max(i => i?.Height ?? 0);
            var totalWidth = images.Sum(i => i?.Width ?? 0);

            var composite = Drawing.NewImage<T>(totalWidth, maxHeight, fill);
            int xOffset = 0;
            composite.Mutate(x =>
            {
                foreach (var image in images)
                {
                    if (image == null)
                    {
                        continue;
                    }

                    x.DrawImage(image, new Point(xOffset, 0), 1f);
                    xOffset += image.Width;
                }
            });

            return composite;
        }

        public static Tuple<int, double> DetectLine(double[,] m, int row, int col, int lineLength, double centreThreshold, int resolutionAngle)
        {
            double endThreshold = centreThreshold / 2;

            if (m[row, col] < centreThreshold)
            {
                return null; //to not proceed if current pixel is low intensity
            }

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            int maxAngle = -1;
            double intensitySum = 0.0;

            // double sumThreshold = lineLength * sensitivity;
            int degrees = 0;

            while (degrees < 180) //loop over 180 degrees in jumps of 10 degrees.
            {
                double cosAngle = Math.Cos(Math.PI * degrees / 180);
                double sinAngle = Math.Sin(Math.PI * degrees / 180);

                //check if extreme end of line goes outside bound
                if (row + (int)(cosAngle * lineLength) >= rows || row + (int)(cosAngle * lineLength) < 0)
                {
                    degrees += resolutionAngle;
                    continue;
                }

                if (col + (int)(sinAngle * lineLength) >= cols || col + (int)(sinAngle * lineLength) < 0)
                {
                    degrees += resolutionAngle;
                    continue;
                }

                //check if extreme end of line is low intensity pixel
                if (m[row + (int)(cosAngle * lineLength), col + (int)(sinAngle * lineLength)] < endThreshold)
                {
                    degrees += resolutionAngle;
                    continue;
                }

                double sum = 0.0;
                for (int j = 0; j < lineLength; j++)
                {
                    sum += m[row + (int)(cosAngle * j), col + (int)(sinAngle * j)];
                }

                if (sum > intensitySum)
                {
                    maxAngle = degrees;
                    intensitySum = sum;
                }

                degrees += resolutionAngle;
            } // while loop

            return Tuple.Create(maxAngle, intensitySum);
        }

        /// <summary>
        /// Returns an image of the data matrix.
        /// Normalises the values from min->max to 0->1.
        /// Thus the grey-scale image pixels will range from 0 to 255.
        /// This method was originally written to draw sonograms,
        ///       hence the avoidance of outliers and references to freq bins.
        /// Perhaps this method should be put back in BaseSonogram.cs.
        /// </summary>
        public static Image<Rgb24> GetMatrixImage(double[,] data)
        {
            int width = data.GetLength(0); // Number of spectra in sonogram
            int binCount = data.GetLength(1);
            int binHeight = 1;

            int imageHeight = binCount * binHeight; // image ht = sonogram ht

            //set up min, max for normalising of sonogram values
            int minRank = 50;
            int maxRank = 1000;

            // find min and max of matrix avoiding outliers.
            double[] array = MatrixTools.Matrix2Array(data);
            double minValue = DataTools.GetNthSmallestValue(array, minRank);
            double maxValue = DataTools.GetNthLargestValue(array, maxRank);
            double[] minmax = { minValue, maxValue };
            double min = minmax[0];
            double max = minmax[1];
            double range = max - min;

            Color[] grayScale = GrayScale();
            var bmp = new Image<Rgb24>(width, imageHeight);
            int yOffset = imageHeight;

            // over all freq bins
            for (int y = 0; y < binCount; y++)
            {
                for (int r = 0; r < binHeight; r++)
                {
                    //repeat this bin if pixel rows per bin>1
                    for (int x = 0; x < width; x++)
                    {
                        //for pixels in the line - NormaliseMatrixValues and bound the value - use min bound, max and 255 image intensity range
                        double value = (data[x, y] - min) / range;
                        int c = 255 - (int)Math.Floor(255.0 * value); //original version
                        if (c < 0)
                        {
                            c = 0;
                        }
                        else
                        if (c >= 256)
                        {
                            c = 255;
                        }

                        bmp[x, yOffset - 1] = grayScale[c];
                    } //for all pixels in line

                    yOffset--;
                } //end repeats over one track
            } //end over all freq bins

            return bmp;
        }

        public static Dictionary<Color, double> GetColorHistogramNormalized(Image<Rgb24> image, Rectangle? region = null)
        {
            Rectangle definiteRegion = region ?? new Rectangle(0, 0, image.Width, image.Height);
            var histogram = new Dictionary<Color, int>(100);

            int sum = definiteRegion.Area();

            for (var i = definiteRegion.Left; i < definiteRegion.Right; i++)
            {
                for (var j = definiteRegion.Top; j < definiteRegion.Bottom; j++)
                {
                    var color = image[i, j];
                    if (histogram.ContainsKey(color))
                    {
                        histogram[color] = histogram[color] + 1;
                    }
                    else
                    {
                        histogram.Add(color, 1);
                    }
                }
            }

            return histogram.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / (double)sum);
        }
    }
}