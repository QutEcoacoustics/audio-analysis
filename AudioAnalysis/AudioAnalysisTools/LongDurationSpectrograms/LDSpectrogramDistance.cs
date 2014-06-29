// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogramDistance.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using TowseyLibrary;

    public static class LDSpectrogramDistance
    {
        // set DEFAULT values for parameters
        #region Static Fields

        // must be value <=1.0
        private static double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF;

        // CHANGE default RGB mapping here.
        private static string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR; 

        // default value - from which spectrogram was derived
        private static int frameWidth = SpectrogramConstants.FRAME_WIDTH;

        // assume recording starts at zero minute of day i.e. midnight
        private static TimeSpan minuteOffset = SpectrogramConstants.MINUTE_OFFSET;

        // default value - after resampling
        private static int sampleRate = SpectrogramConstants.SAMPLE_RATE; 

        // assume one minute spectra and hourly time lines
        private static TimeSpan xScale = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
        #endregion

        #region Public Methods and Operators

        public static void DrawDistanceSpectrogram(dynamic configuration)
        {
            var inputDirectory = ((string)configuration.InputDirectory).ToDirectoryInfo();
            var inputFileName1 = ((string)configuration.IndexFile1).ToFileInfo();
            var inputFileName2 = ((string)configuration.IndexFile2).ToFileInfo();
            var outputDirectory = ((string)configuration.OutputDirectory).ToDirectoryInfo();

            // First, need to make an optional cast:
            // int? minuteOffset = confirguration.MinuteOffset;

            // Second,  have a few options:
            // if (!minuteOffset.HasValue) {
            // minuteOffset = 0;
            // }
            // OR       minuteOffset = minuteOffset == null ? 0 : minuteoffset.Value;

            // ORRR - all in one line!
            // int minuteOffset = (int?)configuration.MinuteOffset ?? 0;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string map = configuration.ColorMap;

            // assigns indices to RGB
            colorMap = map ?? SpectrogramConstants.RGBMap_ACI_ENT_CVR;
            backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? backgroundFilterCoeff;

            // must be value <=1.0

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            // default recording starts at zero minute of day i.e. midnight
            minuteOffset = (TimeSpan?)configuration.MinuteOffset ?? TimeSpan.Zero;

            xScale = (TimeSpan?)configuration.X_Scale ?? SpectrogramConstants.X_AXIS_TIC_INTERVAL;

            // default is one minute spectra i.e. 60 per hour
            sampleRate = (int?)configuration.SampleRate ?? sampleRate; // default value - after resampling
            frameWidth = (int?)configuration.FrameWidth ?? frameWidth;

            // frame width from which spectrogram was derived. Assume no frame overlap.
            DrawDistanceSpectrogram(inputDirectory, inputFileName1, inputFileName2, outputDirectory);
        }

        /// <summary>
        /// This method compares the acoustic indices derived from two different long duration recordings of the same length.
        ///     It takes as input any number of csv files of acoustic indices in spectrogram columns.
        ///     Typically there will be at least three indices csv files for each of the original recordings to be compared.
        ///     The method produces four spectrogram image files:
        ///     1) A negative false-colour spectrogram derived from the indices of recording 1.
        ///     2) A negative false-colour spectrogram derived from the indices of recording 2.
        ///     3) A spectrogram of euclidean distances bewteen the two input files.
        ///     4) The above three spectrograms combined in one image.
        /// </summary>
        /// <param name="inputDirectory">
        /// </param>
        /// <param name="inputFileName1">
        /// </param>
        /// <param name="inputFileName2">
        /// </param>
        /// <param name="outputDirectory">
        /// </param>
        public static void DrawDistanceSpectrogram(
            DirectoryInfo inputDirectory, 
            FileInfo inputFileName1, 
            FileInfo inputFileName2, 
            DirectoryInfo outputDirectory)
        {
            // PARAMETERS
            string outputFileName1 = inputFileName1.Name;
            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.ColorMode = colorMap;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            string[] keys = colorMap.Split('-');
            cs1.ReadCSVFiles(inputDirectory, inputFileName1.Name, keys);

            // ColourSpectrogram.BlurSpectrogram(cs1);
            // cs1.DrawGreyScaleSpectrograms(opdir, opFileName1);
            cs1.DrawNegativeFalseColourSpectrogram(outputDirectory, outputFileName1);
            string imagePath = Path.Combine(outputDirectory.FullName, outputFileName1 + ".COLNEG.png");
            Image spg1Image = ImageTools.ReadImage2Bitmap(imagePath);
            if (spg1Image == null)
            {
                LoggedConsole.WriteLine("SPECTROGRAM IMAGE DOES NOT EXIST: {0}", imagePath);
                return;
            }

            string title =
                string.Format(
                    "FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", 
                    inputFileName1, 
                    cs1.ColorMode);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, spg1Image.Width);
            spg1Image = LDSpectrogramRGB.FrameSpectrogram(
                spg1Image, 
                titleBar, 
                minuteOffset, 
                cs1.XInterval, 
                cs1.YInterval);

            string outputFileName2 = inputFileName2.Name;
            var cs2 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs2.ColorMode = colorMap;
            cs2.BackgroundFilter = backgroundFilterCoeff;
            cs2.ReadCSVFiles(inputDirectory, inputFileName2.Name, keys);

            // cs2.DrawGreyScaleSpectrograms(opdir, opFileName2);
            cs2.DrawNegativeFalseColourSpectrogram(outputDirectory, outputFileName2);
            imagePath = Path.Combine(outputDirectory.FullName, outputFileName2 + ".COLNEG.png");
            Image spg2Image = ImageTools.ReadImage2Bitmap(imagePath);
            if (spg2Image == null)
            {
                LoggedConsole.WriteLine("SPECTROGRAM IMAGE DOES NOT EXIST: {0}", imagePath);
                return;
            }

            title = string.Format(
                "FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", 
                inputFileName2, 
                cs2.ColorMode);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, spg2Image.Width);
            spg2Image = LDSpectrogramRGB.FrameSpectrogram(
                spg2Image, 
                titleBar, 
                minuteOffset, 
                cs2.XInterval, 
                cs2.YInterval);

            string outputFileName4 = inputFileName1 + ".EuclidianDistance.png";
            Image deltaSp = DrawDistanceSpectrogram(cs1, cs2);
            Color[] colorArray = LDSpectrogramRGB.ColourChart2Array(GetDifferenceColourChart());
            titleBar = DrawTitleBarOfEuclidianDistanceSpectrogram(
                inputFileName1.Name, 
                inputFileName2.Name, 
                colorArray, 
                deltaSp.Width, 
                SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            deltaSp = LDSpectrogramRGB.FrameSpectrogram(deltaSp, titleBar, minuteOffset, cs2.XInterval, cs2.YInterval);
            deltaSp.Save(Path.Combine(outputDirectory.FullName, outputFileName4));

            string outputFileName5 = inputFileName1 + ".2SpectrogramsAndDistance.png";
            var images = new Image[3];
            images[0] = spg1Image;
            images[1] = spg2Image;
            images[2] = deltaSp;
            Image combinedImage = ImageTools.CombineImagesVertically(images);
            combinedImage.Save(Path.Combine(outputDirectory.FullName, outputFileName5));
        }

        public static Image DrawDistanceSpectrogram(LDSpectrogramRGB cs1, LDSpectrogramRGB cs2)
        {
            string[] keys = cs1.ColorMap.Split('-');

            string key = keys[0];
            double[,] m1Red = cs1.GetNormalisedSpectrogramMatrix(key);
            LDSpectrogramRGB.SpectraStats stats = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(m1Red);
            cs1.IndexStats.Add(key, stats);
            m1Red = MatrixTools.Matrix2ZScores(m1Red, stats.Mode, stats.StandardDeviation);

            ////LoggedConsole.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);
            key = keys[1];
            double[,] m1Grn = cs1.GetNormalisedSpectrogramMatrix(key);
            stats = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(m1Grn);
            cs1.IndexStats.Add(key, stats);
            m1Grn = MatrixTools.Matrix2ZScores(m1Grn, stats.Mode, stats.StandardDeviation);

            ////LoggedConsole.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);
            key = keys[2];
            double[,] m1Blu = cs1.GetNormalisedSpectrogramMatrix(key);
            stats = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(m1Blu);
            cs1.IndexStats.Add(key, stats);
            m1Blu = MatrixTools.Matrix2ZScores(m1Blu, stats.Mode, stats.StandardDeviation);

            ////LoggedConsole.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);
            key = keys[0];
            double[,] m2Red = cs2.GetNormalisedSpectrogramMatrix(key);
            stats = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(m2Red);
            cs2.IndexStats.Add(key, stats);
            m2Red = MatrixTools.Matrix2ZScores(m2Red, stats.Mode, stats.StandardDeviation);

            ////LoggedConsole.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);
            key = keys[1];
            double[,] m2Grn = cs2.GetNormalisedSpectrogramMatrix(key);
            stats = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(m2Grn);
            cs2.IndexStats.Add(key, stats);
            m2Grn = MatrixTools.Matrix2ZScores(m2Grn, stats.Mode, stats.StandardDeviation);

            ////LoggedConsole.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);
            key = keys[2];
            double[,] m2Blu = cs2.GetNormalisedSpectrogramMatrix(key);
            stats = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(m2Blu);
            cs2.IndexStats.Add(key, stats);
            m2Blu = MatrixTools.Matrix2ZScores(m2Blu, stats.Mode, stats.StandardDeviation);

            ////LoggedConsole.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);
            var v1 = new double[3];
            double[] mode1 =
                {
                    cs1.IndexStats[keys[0]].Mode, cs1.IndexStats[keys[1]].Mode, 
                    cs1.IndexStats[keys[2]].Mode
                };
            double[] stDv1 =
                {
                    cs1.IndexStats[keys[0]].StandardDeviation, cs1.IndexStats[keys[1]].StandardDeviation, 
                    cs1.IndexStats[keys[2]].StandardDeviation
                };
            LoggedConsole.WriteLine(
                "1: avACI={0:f3}+/-{1:f3};   avTEN={2:f3}+/-{3:f3};   avCVR={4:f3}+/-{5:f3}", 
                mode1[0], 
                stDv1[0], 
                mode1[1], 
                stDv1[1], 
                mode1[2], 
                stDv1[2]);

            var v2 = new double[3];
            double[] mode2 =
                {
                    cs2.IndexStats[keys[0]].Mode, cs2.IndexStats[keys[1]].Mode, 
                    cs2.IndexStats[keys[2]].Mode
                };
            double[] stDv2 =
                {
                    cs2.IndexStats[keys[0]].StandardDeviation, cs2.IndexStats[keys[1]].StandardDeviation, 
                    cs2.IndexStats[keys[2]].StandardDeviation
                };
            LoggedConsole.WriteLine(
                "2: avACI={0:f3}+/-{1:f3};   avTEN={2:f3}+/-{3:f3};   avCVR={4:f3}+/-{5:f3}", 
                mode2[0], 
                stDv2[0], 
                mode2[1], 
                stDv2[1], 
                mode2[2], 
                stDv2[2]);

            // assume all matricies are normalised and of the same dimensions
            int rows = m1Red.GetLength(0); // number of rows
            int cols = m1Red.GetLength(1); // number
            var d12Matrix = new double[rows, cols];
            var d11Matrix = new double[rows, cols];
            var d22Matrix = new double[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    v1[0] = m1Red[row, col];
                    v1[1] = m1Grn[row, col];
                    v1[2] = m1Blu[row, col];

                    v2[0] = m2Red[row, col];
                    v2[1] = m2Grn[row, col];
                    v2[2] = m2Blu[row, col];

                    d12Matrix[row, col] = DataTools.EuclidianDistance(v1, v2);
                    d11Matrix[row, col] = (v1[0] + v1[1] + v1[2]) / 3; // get average of the normalised values
                    d22Matrix[row, col] = (v2[0] + v2[1] + v2[2]) / 3;

                    // following lines are for debugging purposes
                    // if ((row == 150) && (col == 1100))
                    // {
                    // LoggedConsole.WriteLine("V1={0:f3}, {1:f3}, {2:f3}", v1[0], v1[1], v1[2]);
                    // LoggedConsole.WriteLine("V2={0:f3}, {1:f3}, {2:f3}", v2[0], v2[1], v2[2]);
                    // LoggedConsole.WriteLine("EDist12={0:f4};   ED11={1:f4};   ED22={2:f4}", d12Matrix[row, col], d11Matrix[row, col], d22Matrix[row, col]);
                    // }
                }
            }
 // rows

            double[] array = DataTools.Matrix2Array(d12Matrix);
            double avDist, sdDist;
            NormalDist.AverageAndSD(array, out avDist, out sdDist);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    d12Matrix[row, col] = (d12Matrix[row, col] - avDist) / sdDist;
                }
            }

            // int MaxRGBValue = 255;
            // int v;
            double zScore;
            Dictionary<string, Color> colourChart = GetDifferenceColourChart();
            Color colour;

            var bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    zScore = d12Matrix[row, col];

                    if (d11Matrix[row, col] >= d22Matrix[row, col])
                    {
                        if (zScore > 3.08)
                        {
                            colour = colourChart["+99.9%"];
                        }
 // 99.9% conf
                        else
                        {
                            if (zScore > 2.33)
                            {
                                colour = colourChart["+99.0%"];
                            }
 // 99.0% conf
                            else
                            {
                                if (zScore > 1.65)
                                {
                                    colour = colourChart["+95.0%"];
                                }
 // 95% conf
                                else
                                {
                                    if (zScore < 0.0)
                                    {
                                        colour = colourChart["NoValue"];
                                    }
                                    else
                                    {
                                        // v = Convert.ToInt32(zScore * MaxRGBValue);
                                        // colour = Color.FromArgb(v, 0, v);
                                        colour = colourChart["+NotSig"];
                                    }
                                }
                            }
                        }
 // if() else
                        bmp.SetPixel(col, row, colour);
                    }
                    else
                    {
                        if (zScore > 3.08)
                        {
                            colour = colourChart["-99.9%"];
                        }
 // 99.9% conf
                        else
                        {
                            if (zScore > 2.33)
                            {
                                colour = colourChart["-99.0%"];
                            }
 // 99.0% conf
                            else
                            {
                                if (zScore > 1.65)
                                {
                                    colour = colourChart["-95.0%"];
                                }
 // 95% conf
                                else
                                {
                                    if (zScore < 0.0)
                                    {
                                        colour = colourChart["NoValue"];
                                    }
                                    else
                                    {
                                        // v = Convert.ToInt32(zScore * MaxRGBValue);
                                        // if()
                                        // colour = Color.FromArgb(0, v, v);
                                        colour = colourChart["-NotSig"];
                                    }
                                }
                            }
                        }
 // if() else
                        bmp.SetPixel(col, row, colour);
                    }
                }
 // all rows
            }
 // all rows

            return bmp;
        }
 // DrawDistanceSpectrogram()

        public static Image DrawTitleBarOfEuclidianDistanceSpectrogram(
            string name1, 
            string name2, 
            Color[] colorArray, 
            int width, 
            int height)
        {
            Image colourChart = ImageTools.DrawColourChart(width, height, colorArray);

            var bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            var pen = new Pen(Color.White);
            var stringFont = new Font("Arial", 9);

            // Font stringFont = new Font("Tahoma", 9);
            var stringSize = new SizeF();

            string text = string.Format("EUCLIDIAN DISTANCE SPECTROGRAM (scale:hours x kHz)");
            int X = 4;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += stringSize.ToSize().Width + 70;
            text = name1 + "  +99.9%conf";
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += stringSize.ToSize().Width + 1;
            g.DrawImage(colourChart, X, 1);

            X += colourChart.Width;
            text = "-99.9%conf   " + name2;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));
            stringSize = g.MeasureString(text, stringFont);
            X += stringSize.ToSize().Width + 1; // distance to end of string

            text = string.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X)
            {
                g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));
            }

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0); // draw upper boundary

            // g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }

        public static Dictionary<string, Color> GetDifferenceColourChart()
        {
            var colourChart = new Dictionary<string, Color>();
            colourChart.Add("+99.9%", Color.FromArgb(255, 190, 20));
            colourChart.Add("+99.0%", Color.FromArgb(240, 50, 30)); // +99% conf
            colourChart.Add("+95.0%", Color.FromArgb(200, 30, 15)); // +95% conf
            colourChart.Add("+NotSig", Color.FromArgb(50, 5, 5)); // + not significant
            colourChart.Add("NoValue", Color.Black);

            // no value
            colourChart.Add("-99.9%", Color.FromArgb(20, 255, 230));
            colourChart.Add("-99.0%", Color.FromArgb(30, 240, 50)); // +99% conf
            colourChart.Add("-95.0%", Color.FromArgb(15, 200, 30)); // +95% conf
            colourChart.Add("-NotSig", Color.FromArgb(10, 50, 20)); // + not significant
            return colourChart;
        }

        #endregion
    } // class LDSpectrogramDistance
}