// <copyright file="LDSpectrogramDifference.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using Acoustics.Shared.ConfigFile;

    using TowseyLibrary;

    public static class LdSpectrogramDifference
    {
        //PARAMETERS
        // set DEFAULT values for parameters
        private static TimeSpan minuteOffset = SpectrogramConstants.MINUTE_OFFSET;  // assume recording starts at zero minute of day i.e. midnight
        private static TimeSpan xScale = SpectrogramConstants.X_AXIS_TIC_INTERVAL;  // assume one minute spectra and hourly time lines
        private static int sampleRate = SpectrogramConstants.SAMPLE_RATE;      // default value - after resampling
        private static int frameWidth = SpectrogramConstants.FRAME_LENGTH;      // default value - from which spectrogram was derived

        private static string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR; //CHANGE default RGB mapping here.
        private static double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF; //must be value <=1.0

        // depracated May 2017
        // private static double colourGain = SpectrogramConstants.COLOUR_GAIN;

        public static void DrawDifferenceSpectrogram(Config configuration)
        {
            var ipdir = configuration["InputDirectory"].ToDirectoryInfo();
            var ipFileName1 = configuration["IndexFile1"].ToFileInfo();
            var ipFileName2 = configuration["IndexFile2"].ToFileInfo();
            var opdir = configuration["OutputDirectory"].ToDirectoryInfo();
            string map = configuration.GetStringOrNull("ColorMap");

            // assigns indices to RGB
            colorMap = map ?? SpectrogramConstants.RGBMap_ACI_ENT_CVR;

            backgroundFilterCoeff = configuration.GetDoubleOrNull("BackgroundFilterCoeff") ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;

            // depracated May 2017
            // colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            minuteOffset = configuration.GetTimeSpanOrNull("MinuteOffset") ?? SpectrogramConstants.MINUTE_OFFSET;   // default = zero minute of day i.e. midnight
            xScale = configuration.GetTimeSpanOrNull("X_Scale") ?? SpectrogramConstants.X_AXIS_TIC_INTERVAL; // default is one minute spectra i.e. 60 per hour
            sampleRate = configuration.GetIntOrNull("SampleRate") ?? SpectrogramConstants.SAMPLE_RATE;
            frameWidth = configuration.GetIntOrNull("FrameWidth") ?? SpectrogramConstants.FRAME_LENGTH;

            DrawDifferenceSpectrogram(ipdir, ipFileName1, ipFileName2, opdir);
        }

        /// <summary>
        /// This method compares the acoustic indices derived from two different long duration recordings of the same length.
        /// It takes as input six csv files of acoustic indices in spectrogram columns, three csv files for each of the original recordings to be compared.
        /// The method produces one spectrogram image files:
        /// 1) A false-colour difference spectrogram, where the difference is shown as a plus/minus departure from grey.
        /// </summary>
        public static void DrawDifferenceSpectrogram(DirectoryInfo ipdir, FileInfo ipFileName1, FileInfo ipFileName2, DirectoryInfo opdir)
        {
            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap)
            {
                FileName = ipFileName1.Name,
                ColorMode = colorMap,
                BackgroundFilter = backgroundFilterCoeff,
            };
            string[] keys = colorMap.Split('-');
            cs1.ReadCsvFiles(ipdir, ipFileName1.Name, keys);
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                LoggedConsole.WriteLine("There are no spectrogram matrices in cs1.dictionary.");
                return;
            }

            var cs2 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap)
            {
                FileName = ipFileName2.Name,
                ColorMode = colorMap,
                BackgroundFilter = backgroundFilterCoeff,
            };
            cs2.ReadCsvFiles(ipdir, ipFileName2.Name, keys);
            if (cs2.GetCountOfSpectrogramMatrices() == 0)
            {
                LoggedConsole.WriteLine("There are no spectrogram matrices in cs2.dictionary.");
                return;
            }

            //string title1 = String.Format("DIFFERENCE SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
            //Image deltaSp1 = LDSpectrogramDifference.DrawDifferenceSpectrogram(cs1, cs2, colourGain);
            //Image titleBar1 = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title1, deltaSp1.Width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            //deltaSp1 = LDSpectrogramRGB.FrameSpectrogram(deltaSp1, titleBar1, minuteOffset, cs1.X_interval, cs1.Y_interval);
            //string opFileName1 = ipFileName1 + ".Difference.COLNEG.png";
            //deltaSp1.Save(Path.Combine(opdir.FullName, opFileName1));

            //Draw positive difference spectrograms in one image.
            double colourGain = 2.0;
            Image[] images = DrawPositiveDifferenceSpectrograms(cs1, cs2, colourGain);

            int nyquist = cs1.SampleRate / 2;
            int herzInterval = 1000;
            string title =
                $"DIFFERENCE SPECTROGRAM where {ipFileName1} > {ipFileName2}.      (scale:hours x kHz)       (colour: R-G-B={cs1.ColorMode})";
            var titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, images[0].Width);
            images[0] = LDSpectrogramRGB.FrameLDSpectrogram(images[0], titleBar, cs1, nyquist, herzInterval);

            title = string.Format("DIFFERENCE SPECTROGRAM where {1} > {0}      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMode);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, images[1].Width);
            images[1] = LDSpectrogramRGB.FrameLDSpectrogram(images[1], titleBar, cs1, nyquist, herzInterval);
            Image combinedImage = ImageTools.CombineImagesVertically(images);
            string opFileName = ipFileName1 + "-" + ipFileName2 + ".Difference.png";
            combinedImage.Save(Path.Combine(opdir.FullName, opFileName));
        }

        public static Image DrawDifferenceSpectrogram(LDSpectrogramRGB target, LDSpectrogramRGB reference, double colourGain)
        {
            string[] keys = target.ColorMap.Split('-');
            double[,] tgtRedM = target.GetNormalisedSpectrogramMatrix(keys[0]);
            double[,] tgtGrnM = target.GetNormalisedSpectrogramMatrix(keys[1]);
            double[,] tgtBluM = target.GetNormalisedSpectrogramMatrix(keys[2]);

            double[,] refRedM = reference.GetNormalisedSpectrogramMatrix(keys[0]);
            double[,] refGrnM = reference.GetNormalisedSpectrogramMatrix(keys[1]);
            double[,] refBluM = reference.GetNormalisedSpectrogramMatrix(keys[2]);

            // assume all matricies are normalised and of the same dimensions
            int rows = tgtRedM.GetLength(0); //number of rows
            int cols = tgtRedM.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int maxRGBValue = 255;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    var d1 = (tgtRedM[row, column] - refRedM[row, column]) * colourGain;
                    var d2 = (tgtGrnM[row, column] - refGrnM[row, column]) * colourGain;
                    var d3 = (tgtBluM[row, column] - refBluM[row, column]) * colourGain;

                    var i1 = 127 + Convert.ToInt32(d1 * maxRGBValue);
                    i1 = Math.Max(0, i1);
                    i1 = Math.Min(maxRGBValue, i1);
                    var i2 = 127 + Convert.ToInt32(d2 * maxRGBValue);
                    i2 = Math.Max(0, i2);
                    i2 = Math.Min(maxRGBValue, i2);
                    var i3 = 127 + Convert.ToInt32(d3 * maxRGBValue);
                    i3 = Math.Max(0, i3);
                    i3 = Math.Min(maxRGBValue, i3);

                    //Color colour = Color.FromArgb(i1, i2, i3);
                    bmp.SetPixel(column, row, Color.FromArgb(i1, i2, i3));
                }
            }

            return bmp;
        }

        public static Image[] DrawPositiveDifferenceSpectrograms(LDSpectrogramRGB target, LDSpectrogramRGB reference, double colourGain)
        {
            string[] keys = target.ColorMap.Split('-');

            double[,] tgtRedM = target.GetNormalisedSpectrogramMatrix(keys[0]);
            double[,] tgtGrnM = target.GetNormalisedSpectrogramMatrix(keys[1]);
            double[,] tgtBluM = target.GetNormalisedSpectrogramMatrix(keys[2]);

            double[,] refRedM = reference.GetNormalisedSpectrogramMatrix(keys[0]);
            double[,] refGrnM = reference.GetNormalisedSpectrogramMatrix(keys[1]);
            double[,] refBluM = reference.GetNormalisedSpectrogramMatrix(keys[2]);

            // assume all matricies are normalised and of the same dimensions
            int rows = tgtRedM.GetLength(0); //number of rows
            int cols = tgtRedM.GetLength(1); //number

            var spg1Image = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            var spg2Image = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            int maxRgbValue = 255;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    var dR = (tgtRedM[row, column] - refRedM[row, column]) * colourGain;
                    var dG = (tgtGrnM[row, column] - refGrnM[row, column]) * colourGain;
                    var dB = (tgtBluM[row, column] - refBluM[row, column]) * colourGain;

                    var iR1 = 0;
                    var iR2 = 0;
                    var iG1 = 0;
                    var iG2 = 0;
                    var iB1 = 0;
                    var iB2 = 0;

                    var value = Convert.ToInt32(Math.Abs(dR) * maxRgbValue);
                    value = Math.Min(maxRgbValue, value);
                    if (dR > 0.0)
                    {
                        iR1 = value;
                    }
                    else
                    {
                        iR2 = value;
                    }

                    value = Convert.ToInt32(Math.Abs(dG) * maxRgbValue);
                    value = Math.Min(maxRgbValue, value);
                    if (dG > 0.0)
                    {
                        iG1 = value;
                    }
                    else
                    {
                        iG2 = value;
                    }

                    value = Convert.ToInt32(Math.Abs(dB) * maxRgbValue);
                    value = Math.Min(maxRgbValue, value);
                    if (dB > 0.0)
                    {
                        iB1 = value;
                    }
                    else
                    {
                        iB2 = value;
                    }

                    var colour1 = Color.FromArgb(iR1, iG1, iB1);
                    var colour2 = Color.FromArgb(iR2, iG2, iB2);
                    spg1Image.SetPixel(column, row, colour1);
                    spg2Image.SetPixel(column, row, colour2);
                }
            }

            Image[] images = new Image[2];
            images[0] = spg1Image;
            images[1] = spg2Image;
            return images;
        }
    }
}
