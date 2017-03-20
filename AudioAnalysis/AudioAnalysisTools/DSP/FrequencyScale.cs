using System;
using System.Drawing;

namespace AudioAnalysisTools.DSP
{

    /// IMPORTANT NOTE: If you are converting Herz scale from LINEAR to OCTAVE, this conversion MUST be done BEFORE noise reduction
    /*
    All the below octave scale options are designed for a final freq scale having 256 bins
    Use the below OctaveScaleTypes as follows:
    (1)  Constants required for full octave scale when sr = 22050:           FreqScaleType ost = FreqScaleType.Octaves27Sr22050;
    (2)  Constants required for split linear-octave scale when sr = 22050:   FreqScaleType ost = FreqScaleType.Linear62Octaves31Nyquist11025;
    (3)  Constants required for full octave scale when sr = 64000:           FreqScaleType ost = FreqScaleType.Octaves24Nyquist32000;
    (4)  Constants required for split linear-octave scale when sr = 64000    FreqScaleType ost = FreqScaleType.Linear125Octaves28Nyquist32000;
*/
    public enum FreqScaleType
    {
        Linear,
        Mel,
        Linear62Octaves31Nyquist11025,
        Linear125Octaves30Nyquist11025,
        Octaves24Nyquist32000,
        Linear125Octaves28Nyquist32000
    }



    public class FrequencyScale
    {
        private int[,] _gridLineLocations;


        /// <summary>
        /// half the sample rate
        /// </summary>
        public int Nyquist { get; set; }

        /// <summary>
        /// Frame size for the FFT
        /// </summary>
        public int WindowSize { get; set; }

        /// <summary>
        /// Step size for the FFT window
        /// </summary>
        public int FrameStep { get; set; }

        /// <summary>
        /// number of frequency bins in the final spectrogram
        /// </summary>
        public int FinalBinCount { get; set; }

        /// <summary>
        /// The scale type i.e. linear, octave etc.
        /// </summary>
        public FreqScaleType ScaleType { get; set; }

        /// <summary>
        /// Herz interval between gridlines when using a linear scale
        /// </summary>
        public int HerzInterval { get; set; }

        /// <summary>
        /// Top end of the linear part of an Octave Scale spectrogram
        /// </summary>
        public int LinearBound { get; set; }

        /// <summary>
        /// Number of octave to appear above the linear part of scale
        /// </summary>
        public int OctaveCount { get; set; }

        /// <summary>
        /// The centres of the frequency bands for octave scale
        /// </summary>
        public int[] OctaveCentres { get; set; }

        /// <summary>
        /// The location of gridlines (first column) and the Hz value for the grid lines (second column of matrix)
        /// </summary>
        public int[,] GridLineLocations { get { return _gridLineLocations; }
                                          set { _gridLineLocations = value; } }


        /// <summary>
        /// CONSTRUCTOR
        /// Calling this constructor assumes linear freq scale is required
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="frameSize"></param>
        /// <param name="frameStep"></param>
        /// <param name="herzInterval"></param>
        public FrequencyScale(int sr, int frameSize, int herzInterval)
        {
            ScaleType  = FreqScaleType.Linear;
            Nyquist    = sr/2;
            WindowSize = frameSize;
            HerzInterval = herzInterval;
        }


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public FrequencyScale(FreqScaleType fst)
        {
            ScaleType = fst;
            FinalBinCount = 256;
            var octaveBinBounds = GetOctaveScale(fst);
            GridLineLocations = OctaveFreqScale.GetGridLineLocations(fst, octaveBinBounds);
        }


        /// <summary>
        /// CONSTRUCTION OF Frequency Scales
        /// WARNING!: Changing the constants for the octave scales will have undefined effects.
        ///           The options below have been debugged to give what is required.
        ///           However other values have not been debugged - so user should check the output to ensure it is what is required.
        /// </summary>
        /// <param name="fst"></param>
        /// <returns></returns>
        public int[,] GetOctaveScale(FreqScaleType fst)
        {
            int finalBinCount = 256;
            int sr, frameSize, lowerHzBound, upperHzBound, octaveDivisions;
            // NOTE: octaveDivisions = the number of fractional Hz steps within one octave. Piano octave contains 12 steps per octave.

            int[,] octaveBinBounds = null;

            switch (fst)
            {
                case FreqScaleType.Linear:
                    {
                        //// constants required for split linear-octave scale when sr = 22050
                        sr = 22050;
                        frameSize = 8192;
                        lowerHzBound = 62;
                        upperHzBound = 11025;
                    }
                    break;
                case FreqScaleType.Linear62Octaves31Nyquist11025:
                    {
                        //// constants required for split linear-octave scale when sr = 22050
                        sr = 22050;
                        frameSize = 8192;
                        lowerHzBound = 62;
                        upperHzBound = 11025;
                        octaveDivisions = 31; // fraction steps within one octave. Note: piano = 12 steps per octave.
                        octaveBinBounds = OctaveFreqScale.LinearToSplitLinearOctaveScale(sr, frameSize, finalBinCount, lowerHzBound, upperHzBound, octaveDivisions);
                    }
                    break;
                case FreqScaleType.Linear125Octaves30Nyquist11025:
                    {
                        //// constants required for split linear-octave scale when sr = 22050
                        sr = 22050;
                        frameSize = 8192;
                        lowerHzBound = 125;
                        upperHzBound = 11025;
                        octaveDivisions = 32; // fraction steps within one octave. Note: piano = 12 steps per octave.
                        octaveBinBounds = OctaveFreqScale.LinearToSplitLinearOctaveScale(sr, frameSize, finalBinCount, lowerHzBound, upperHzBound, octaveDivisions);
                    }
                    break;
                case FreqScaleType.Octaves24Nyquist32000:
                    {
                        //// constants required for full octave scale when sr = 64000
                        sr = 64000;
                        frameSize = 16384;  // = 2*8192   or 4*4096;;
                        lowerHzBound = 15;
                        upperHzBound = 32000;
                        octaveDivisions = 24; // fraction steps within one octave. Note: piano = 12 steps per octave.
                        octaveBinBounds = OctaveFreqScale.LinearToFullOctaveScale(sr, frameSize, finalBinCount, lowerHzBound, upperHzBound, octaveDivisions);
                    }
                    break;
                case FreqScaleType.Linear125Octaves28Nyquist32000:
                    {
                        //// constants required for split linear-octave scale when sr = 64000
                        sr = 64000;
                        frameSize = 16384;  // = 2*8192   or 4*4096;;
                        lowerHzBound = 125;
                        upperHzBound = 32000;
                        octaveDivisions = 28; // fraction steps within one octave. Note: piano = 12 steps per octave.
                        octaveBinBounds = OctaveFreqScale.LinearToSplitLinearOctaveScale(sr, frameSize, finalBinCount, lowerHzBound, upperHzBound, octaveDivisions);
                    }
                    break;
                default:
                    {
                        LoggedConsole.WriteErrorLine("WARNING: UNKNOWN FREQUENCY SCALE. Will default to Linear.");
                        fst = FreqScaleType.Linear;
                    }
                    break;
            }
            return octaveBinBounds;
        }


        public static void DrawLinearFrequencyLinesOnImage(Bitmap bmp, int nyquist, int herzInterval)
        {
            int rows = bmp.Height;
            int cols = bmp.Width;

            // Draw in horizontal grid lines
            double yInterval = bmp.Height / (double)(nyquist / (double)herzInterval);
            int gridCount = (int)(rows / yInterval);
            for (int i = 1; i <= gridCount; i++)
            {
                int row = (int)(i * yInterval);
                int rowFromBottom = rows - row;
                for (int column = 0; column < cols - 3; column++)
                {
                    bmp.SetPixel(column, rowFromBottom, Color.Black);
                    column += 3;
                    bmp.SetPixel(column, rowFromBottom, Color.White);
                    column += 2;
                }
                //int band = (int)(rowFromBottom / yInterval);
                //g.DrawString(((band * kHzInterval) + " kHz"), new Font("Thachoma", 8), Brushes.Gray, 2, row - 5);
            }
        } // DrawFrequencyLinesOnImage()

        public static void Draw1KHzLines(Bitmap bmp, bool doMelScale, int nyquist, double freqBinWidth)
        {
            const int kHz = 1000;
            double kHzBinWidth = kHz / freqBinWidth;
            int width = bmp.Width;
            int height = bmp.Height;

            int bandCount = (int)Math.Floor(height / kHzBinWidth);
            int[] gridLineLocations = new int[bandCount];
            for (int b = 0; b < bandCount; b++)
            {
                gridLineLocations[b] = (int)(height - ((b + 1) * kHzBinWidth));
            }

            //get melscale locations
            if (doMelScale)
                gridLineLocations = FrequencyScale.CreateMelYaxis(kHz, nyquist, height); // WARNING!!!! NEED TO REWORK THIS BUT NOW SELDOM USED

            Graphics g = Graphics.FromImage(bmp);
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            for (int b = 0; b < bandCount; b++) //over each band
            {
                int y = gridLineLocations[b];
                for (int x = 1; x < width; x++)
                {
                    bmp.SetPixel(x - 1, y, Color.White);
                    bmp.SetPixel(x, y, Color.Black);
                    x++;
                }
                g.DrawString(((b + 1) + " kHz"), new Font("Thachoma", 8), Brushes.Black, 2, y + 1);
            }
            //g.Flush();
        }//end AddGridLines()


        public static void DrawKHzLines(Bitmap bmp, int[,] gridLineLocations)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            int bandCount = gridLineLocations.GetLength(0);

            Graphics g = Graphics.FromImage(bmp);

            for (int b = 0; b < bandCount; b++) //over each band
            {
                int y = gridLineLocations[b, 0];
                for (int x = 1; x < width; x++)
                {
                    bmp.SetPixel(x - 1, y, Color.White);
                    bmp.SetPixel(x, y, Color.Black);
                    x++;
                }
                g.DrawString((gridLineLocations[b, 1] + "Hz"), new Font("Thachoma", 8), Brushes.Black, 2, y + 1);
            }
        }//end AddHzGridLines()


        /// <summary>
        /// This method is only called when drawing a reduced sonogram.
        /// Called from BaseSonogram. 
        /// </summary>
        /// <param name="herzInterval"></param>
        /// <param name="nyquistFreq"></param>
        /// <param name="imageHt"></param>
        /// <returns></returns>
        public static int[] CreateLinearYaxis(int herzInterval, int nyquistFreq, int imageHt)
        {
            //int freqRange = this.maxFreq - this.minFreq + 1;
            int minFreq = 0;
            int maxFreq = nyquistFreq;
            int freqRange = maxFreq - minFreq + 1;
            double pixelPerHz = imageHt / (double)freqRange;
            int[] vScale = new int[imageHt];
            //LoggedConsole.WriteLine("freqRange=" + freqRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerHz=" + pixelPerHz);

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    int hzOffset = f - minFreq;
                    int pixelID = (int)(hzOffset * pixelPerHz) + 1;
                    if (pixelID >= imageHt) pixelID = imageHt - 1;
                    //LoggedConsole.WriteLine("f=" + f + " hzOffset=" + hzOffset + " pixelID=" + pixelID);
                    vScale[pixelID] = 1;
                }
            }
            return vScale;
        }


        /// <summary>
        /// Use this method to generate grid lines for mel scale image
        /// Currently this method is only called from FrequncyScale.Draw1kHzLines(Bitmap bmp, bool doMelScale, int nyquist, double freqBinWidth)
        /// and when bool doMelScale = true;
        /// </summary>
        public static int[] CreateMelYaxis(int herzInterval, int nyquistFreq, int imageHt)
        {
            int minFreq = 0;
            int maxFreq = nyquistFreq;
            //int freqRange = maxFreq - minFreq + 1;
            double minMel = MFCCStuff.Mel(minFreq);
            int melRange = (int)(MFCCStuff.Mel(maxFreq) - minMel + 1);
            //double pixelPerHz = imageHt / (double)freqRange;
            double pixelPerMel = imageHt / (double)melRange;
            int[] vScale = new int[imageHt];
            //LoggedConsole.WriteLine("minMel=" + minMel.ToString("F1") + " melRange=" + melRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerMel=" + pixelPerMel);

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    //int hzOffset  = f - this.minFreq;
                    int melOffset = (int)(MFCCStuff.Mel(f) - minMel);
                    int pixelID = (int)(melOffset * pixelPerMel) + 1;
                    if (pixelID >= imageHt) pixelID = imageHt - 1;
                    //LoggedConsole.WriteLine("f=" + f + " melOffset=" + melOffset + " pixelID=" + pixelID);
                    vScale[pixelID] = 1;
                }
            }
            return vScale;
        }


        //public static void DrawFrequencyLinesOnImage(Bitmap bmp, int nyquist, int herzInterval, string freqScale)
        public static void DrawFrequencyLinesOnImage(Bitmap bmp, FrequencyScale freqScale )
        {
            int rows = bmp.Height;
            int cols = bmp.Width;

            // Draw in horizontal grid lines
            for (int i = 1; i <= freqScale.GridLineLocations.Length; i++)
            {
                //int rowFromBottom = rows - gridLineLocations[0, i];
                int rowFromBottom = freqScale.GridLineLocations[0, i];
                for (int column = 0; column < cols - 3; column++)
                {
                    bmp.SetPixel(column, rowFromBottom, Color.Black);
                    column += 3;
                    bmp.SetPixel(column, rowFromBottom, Color.White);
                    column += 2;
                }
                //int band = (int)(rowFromBottom / yInterval);
                //g.DrawString(((band * kHzInterval) + " kHz"), new Font("Thachoma", 8), Brushes.Gray, 2, row - 5);
            }
        }


    }
}
