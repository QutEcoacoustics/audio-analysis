using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TowseyLib;


namespace AudioAnalysisTools
{
    public static class LDSpectrogramDifference
    {
        //PARAMETERS
        // set DEFAULT values for parameters
        private static int minuteOffset = SpectrogramConstants.MINUTE_OFFSET;  // assume recording starts at zero minute of day i.e. midnight
        private static int xScale = SpectrogramConstants.X_AXIS_SCALE;         // assume one minute spectra and hourly time lines
        private static int sampleRate = SpectrogramConstants.SAMPLE_RATE;      // default value - after resampling
        private static int frameWidth = SpectrogramConstants.FRAME_WIDTH;      // default value - from which spectrogram was derived

        private static string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_CVR; //CHANGE default RGB mapping here.
        private static double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF; //must be value <=1.0
        private static double colourGain = SpectrogramConstants.COLOUR_GAIN;


        public static void DrawDifferenceSpectrogram(dynamic configuration)
        {
            string ipdir = configuration.InputDirectory;
            string ipFileName1 = configuration.IndexFile1;
            string ipFileName2 = configuration.IndexFile2;
            string opdir = configuration.OutputDirectory;

            //          First, need to make an optional cast:
            //          int? minuteOffset = confirguration.MinuteOffset;

            //          Second,  have a few options:
            //          if (!minuteOffset.HasValue) {
            //               minuteOffset = 0;
            //          }
            //OR       minuteOffset = minuteOffset == null ? 0 : minuteoffset.Value;

            //ORRR - all the above in one line!
            //int minuteOffset = (int?)configuration.MinuteOffset ?? 0;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string map = configuration.ColorMap;
            colorMap = map != null ? map : SpectrogramConstants.RGBMap_ACI_TEN_CVR;           // assigns indices to RGB

            backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF; 
            colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            minuteOffset = (int?)configuration.MinuteOffset ?? SpectrogramConstants.MINUTE_OFFSET;   // default = zero minute of day i.e. midnight
            xScale = (int?)configuration.X_Scale ?? SpectrogramConstants.X_AXIS_SCALE; // default is one minute spectra i.e. 60 per hour
            sampleRate = (int?)configuration.SampleRate ?? SpectrogramConstants.SAMPLE_RATE; 
            frameWidth = (int?)configuration.FrameWidth ?? SpectrogramConstants.FRAME_WIDTH; 

            DrawDifferenceSpectrogram(new DirectoryInfo(ipdir), new FileInfo(ipFileName1), new FileInfo(ipFileName2), new DirectoryInfo(opdir));
        }



        /// <summary>
        /// This method compares the acoustic indices derived from two different long duration recordings of the same length. 
        /// It takes as input six csv files of acoustic indices in spectrogram columns, three csv files for each of the original recordings to be compared.
        /// The method produces one spectrogram image files:
        /// 1) A false-colour difference spectrogram, where the difference is shown as a plus/minus departure from grey.   
        /// </summary>
        /// <param name="ipdir"></param>
        /// <param name="ipFileName1"></param>
        /// <param name="ipFileName2"></param>
        /// <param name="opdir"></param>
        public static void DrawDifferenceSpectrogram(DirectoryInfo ipdir, FileInfo ipFileName1, FileInfo ipFileName2, DirectoryInfo opdir)
        {

            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = ipFileName1.Name;
            cs1.ColorMODE = colorMap;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.ReadCSVFiles(ipdir, ipFileName1.Name, colorMap);
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                Console.WriteLine("There are no spectrogram matrices in cs1.dictionary.");
                return;
            }

            var cs2 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs2.FileName = ipFileName2.Name;
            cs2.ColorMODE = colorMap;
            cs2.BackgroundFilter = backgroundFilterCoeff;
            cs2.ReadCSVFiles(ipdir, ipFileName2.Name, colorMap);
            if (cs2.GetCountOfSpectrogramMatrices() == 0)
            {
                Console.WriteLine("There are no spectrogram matrices in cs2.dictionary.");
                return;
            }

            //string title1 = String.Format("DIFFERENCE SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
            //Image deltaSp1 = LDSpectrogramDifference.DrawDifferenceSpectrogram(cs1, cs2, colourGain);
            //Image titleBar1 = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title1, deltaSp1.Width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            //deltaSp1 = LDSpectrogramRGB.FrameSpectrogram(deltaSp1, titleBar1, minuteOffset, cs1.X_interval, cs1.Y_interval);
            //string opFileName1 = ipFileName1 + ".Difference.COLNEG.png";
            //deltaSp1.Save(Path.Combine(opdir.FullName, opFileName1));

            //Draw positive difference spectrograms in one image.
            Image[] images = LDSpectrogramDifference.DrawPositiveDifferenceSpectrograms(cs1, cs2, colourGain);

            string title = String.Format("DIFFERENCE SPECTROGRAM where {0} > {1}.      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, images[0].Width);
            images[0] = LDSpectrogramRGB.FrameSpectrogram(images[0], titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);

            title = String.Format("DIFFERENCE SPECTROGRAM where {1} > {0}      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, images[1].Width);
            images[1] = LDSpectrogramRGB.FrameSpectrogram(images[1], titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            Image combinedImage = ImageTools.CombineImagesVertically(images);
            string opFileName = ipFileName1 +"-"+ ipFileName2 + ".Difference.png";
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

            int MaxRGBValue = 255;
            double d1, d2, d3;
            int i1, i2, i3;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    d1 = (tgtRedM[row, column] - refRedM[row, column]) * colourGain;
                    d2 = (tgtGrnM[row, column] - refGrnM[row, column]) * colourGain;
                    d3 = (tgtBluM[row, column] - refBluM[row, column]) * colourGain;

                    i1 = 127 + Convert.ToInt32(d1 * MaxRGBValue);
                    i1 = Math.Max(0, i1);
                    i1 = Math.Min(MaxRGBValue, i1);
                    i2 = 127 + Convert.ToInt32(d2 * MaxRGBValue);
                    i2 = Math.Max(0, i2);
                    i2 = Math.Min(MaxRGBValue, i2);
                    i3 = 127 + Convert.ToInt32(d3 * MaxRGBValue);
                    i3 = Math.Max(0, i3);
                    i3 = Math.Min(MaxRGBValue, i3);

                    //Color colour = Color.FromArgb(i1, i2, i3);
                    bmp.SetPixel(column, row, Color.FromArgb(i1, i2, i3));
                }//end all columns
            }//end all rows
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

            Bitmap spg1Image = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            Bitmap spg2Image = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            double dR, dG, dB;
            int iR1, iR2, iG1, iG2, iB1, iB2, value;
            Color colour1, colour2;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    dR = (tgtRedM[row, column] - refRedM[row, column]) * colourGain;
                    dG = (tgtGrnM[row, column] - refGrnM[row, column]) * colourGain;
                    dB = (tgtBluM[row, column] - refBluM[row, column]) * colourGain;

                    iR1 = 0; iR2 = 0; iG1 = 0; iG2 = 0; iB1 = 0; iB2 = 0;

                    value = Convert.ToInt32(Math.Abs(dR) * MaxRGBValue);
                    value = Math.Min(MaxRGBValue, value);
                    if (dR > 0.0) { iR1 = value; }
                    else          { iR2 = value; }

                    value = Convert.ToInt32(Math.Abs(dG) * MaxRGBValue);
                    value = Math.Min(MaxRGBValue, value);
                    if (dG > 0.0) { iG1 = value; }
                    else          { iG2 = value; }

                    value = Convert.ToInt32(Math.Abs(dB) * MaxRGBValue);
                    value = Math.Min(MaxRGBValue, value);
                    if (dB > 0.0) { iB1 = value; }
                    else          { iB2 = value; }

                    colour1 = Color.FromArgb(iR1, iG1, iB1);
                    colour2 = Color.FromArgb(iR2, iG2, iB2);
                    spg1Image.SetPixel(column, row, colour1);
                    spg2Image.SetPixel(column, row, colour2);
                }//end all columns
            }//end all rows

            Image[] images = new Image[2];
            images[0] = spg1Image;
            images[1] = spg2Image;
            return images;
        }

    } // class SpectrogramDifference
}
