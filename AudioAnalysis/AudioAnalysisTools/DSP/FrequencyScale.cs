using System;
using System.ComponentModel;
using System.Drawing;
using AudioAnalysisTools.LongDurationSpectrograms;
using MathNet.Numerics.Distributions;

namespace AudioAnalysisTools.DSP
{

    /// IMPORTANT NOTE: If you are converting Herz scale from LINEAR to OCTAVE, this conversion MUST be done BEFORE noise reduction
    /*
    All the below octave scale options are designed for a final freq scale having 256 bins.
    Scale name indicates its structure.  You cannot vary the structure.
    */
    public enum FreqScaleType
    {
        Linear,
        Mel,
        Linear62Octaves7Tones31Nyquist11025,
        Linear125Octaves6Tones30Nyquist11025,
        Octaves24Nyquist32000,
        Linear125Octaves7Tones28Nyquist32000
    }



    public class FrequencyScale
    {
        private FreqScaleType linear;

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
        /// Number of bands or tones per octave
        /// </summary>
        public int ToneCount { get; set; }

        /// <summary>
        /// The bin bounds of the frequency bands for octave scale
        /// </summary>
        public int[,] OctaveBinBounds { get; set; }
        
        /// <summary>
        /// The location of gridlines (first column) and the Hz value for the grid lines (second column of matrix)
        /// </summary>
        public int[,] GridLineLocations { get; set; }


        /// <summary>
        /// CONSTRUCTOR
        /// Calling this constructor assumes linear freq scale is required
        /// </summary>
        /// <param name="nyquist"></param>
        /// <param name="frameSize"></param>
        /// <param name="herzInterval"></param>
        public FrequencyScale(int nyquist, int frameSize, int herzInterval)
        {
            ScaleType  = FreqScaleType.Linear;
            Nyquist    = nyquist;
            WindowSize = frameSize;
            FinalBinCount = frameSize/2;
            HerzInterval = herzInterval;
            LinearBound = nyquist;
            GridLineLocations = GetLinearGridLineLocations(nyquist, HerzInterval, FinalBinCount);
        }


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public FrequencyScale(FreqScaleType fst)
        {
            ScaleType = fst;
            if (fst == FreqScaleType.Linear)
            {
                LoggedConsole.WriteErrorLine("WARNING: Assigning DEFAULT parameters for Linear FREQUENCY SCALE.");
                LoggedConsole.WriteErrorLine("         Call other CONSTUCTOR to control linear scale.");
                Nyquist = 11025;
                WindowSize = 512;
                FinalBinCount = 256;
                HerzInterval = 1000;
                LinearBound = Nyquist;
                GridLineLocations = GetLinearGridLineLocations(Nyquist, HerzInterval, 256);
            }
            else if (fst == FreqScaleType.Mel)
            {
                // Do not have Mel scale working yet.
                LoggedConsole.WriteErrorLine("WARNING: Mel Scale needs to be debugged.");
                LoggedConsole.WriteErrorLine("         Assigning parameters for DEFAULT Linear FREQUENCY SCALE.");
                LoggedConsole.WriteErrorLine("         Call other CONSTUCTOR to control linear scale.");
                Nyquist = 11025;
                WindowSize = 512;
                FinalBinCount = 256;
                HerzInterval = 1000;
                LinearBound = Nyquist;
                GridLineLocations = GetLinearGridLineLocations(Nyquist, HerzInterval, 256);
            }
            else // assume octave scale is only other option.
            {
                OctaveFreqScale.GetOctaveScale(this);
            }
        }


        /// <summary>
        /// T.
        /// </summary>
        /// <param name="nyquist"></param>
        /// <param name="herzInterval"></param>
        /// <param name="binCount"></param>
        public static int[,] GetLinearGridLineLocations(int nyquist, int herzInterval, int binCount)
        {
            // Draw in horizontal grid lines
            double yInterval = binCount / (nyquist / (double)herzInterval);
            int gridCount = (int)(binCount / yInterval);

            var gridLineLocations = new int[gridCount, 2];

            for (int i = 0; i < gridCount; i++)
            {
                int row = (int)((i+1) * yInterval);
                gridLineLocations[i, 0] = row;
                gridLineLocations[i, 1] = (i + 1)*herzInterval;
            }
            return gridLineLocations;
        } 



        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="doMelScale"></param>
        /// <param name="nyquist"></param>
        /// <param name="freqBinWidth"></param>
        public static void Draw1KHzLines(Bitmap bmp, bool doMelScale, int nyquist, double freqBinWidth)
        {
            const int kHzInterval = 1000;
            //double kHzBinWidth = kHzInterval / freqBinWidth;
            int binCount = bmp.Height;

            //int bandCount = (int)Math.Floor(binCount / kHzBinWidth);
            int[,] gridLineLocations;

            //get melscale locations
            if (doMelScale)
            {
                gridLineLocations = CreateMelYaxis(kHzInterval, nyquist, binCount);
                    // WARNING!!!! NEED TO DEBUG/REWORK THIS BUT NOW SELDOM USED
            }
            else
            {
                gridLineLocations = GetLinearGridLineLocations(nyquist, kHzInterval, binCount);
            }


            DrawFrequencyLinesOnImage(bmp, gridLineLocations);
            //g.Flush();
        }//end AddGridLines()



        /// <summary>
        /// This method is only called from Basesonogram.GetImage_ReducedSonogram(int factor, bool drawGridLines)
        ///   when drawing a reduced sonogram.
        /// </summary>
        /// <param name="herzInterval"></param>
        /// <param name="nyquistFreq"></param>
        /// <param name="imageHt"></param>
        /// <returns></returns>
        public static int[] CreateLinearYaxis(int herzInterval, int nyquistFreq, int imageHt)
        {
            int minFreq = 0;
            int maxFreq = nyquistFreq;
            int freqRange = maxFreq - minFreq + 1;
            double pixelPerHz = imageHt / (double)freqRange;
            int[] vScale = new int[imageHt];

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    int hzOffset = f - minFreq;
                    int pixelId = (int)(hzOffset * pixelPerHz) + 1;
                    if (pixelId >= imageHt) pixelId = imageHt - 1;
                    //LoggedConsole.WriteLine("f=" + f + " hzOffset=" + hzOffset + " pixelID=" + pixelID);
                    vScale[pixelId] = 1;
                }
            }
            return vScale;
        }


        /// <summary>
        /// THIS METHOD NEEDS TO BE DEBUGGED.  HAS NOT BEEN USED IN YEARS!
        /// Use this method to generate grid lines for mel scale image
        /// Currently this method is only called from FrequncyScale.Draw1kHzLines(Bitmap bmp, bool doMelScale, int nyquist, double freqBinWidth)
        /// and when bool doMelScale = true;
        /// </summary>
        public static int[,] CreateMelYaxis(int herzInterval, int nyquistFreq, int imageHt)
        {
            int minFreq = 0;
            int maxFreq = nyquistFreq;
            double minMel = MFCCStuff.Mel(minFreq);
            int melRange = (int)(MFCCStuff.Mel(maxFreq) - minMel + 1);
            double pixelPerMel = imageHt / (double)melRange;

            // assume mel scale grid lines will only go up to 10 kHz.
            var vScale = new int[10, 2];
            //LoggedConsole.WriteLine("minMel=" + minMel.ToString("F1") + " melRange=" + melRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerMel=" + pixelPerMel);

            for (int f = minFreq + 1; f < maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    //int hzOffset  = f - this.minFreq;
                    int melOffset = (int)(MFCCStuff.Mel(f) - minMel);
                    int pixelId = (int)(melOffset * pixelPerMel) + 1;
                    if (pixelId >= imageHt) pixelId = imageHt - 1;
                    //LoggedConsole.WriteLine("f=" + f + " melOffset=" + melOffset + " pixelID=" + pixelID);
                    vScale[0, 0] = pixelId;
                    vScale[0, 1] = f;
                }
            }
            return vScale;
        }


        public static void DrawFrequencyLinesOnImage(Bitmap bmp, int[,] gridLineLocations)
        {
            // attempt to determine background colour of spectrogram i.e. dark false-colour or light.
            Color bgnColour = bmp.GetPixel(2, 2);
            float brightness = bgnColour.GetBrightness();
            var txtColour = Brushes.White;
            if (brightness > 0.5)
            {
                txtColour = Brushes.Black;
            }

            int width = bmp.Width;
            int height = bmp.Height;
            int bandCount = gridLineLocations.GetLength(0);

            Graphics g = Graphics.FromImage(bmp);

            for (int b = 0; b < bandCount; b++) //over each band
            {
                int y = height - gridLineLocations[b, 0];
                if (y < 0)
                {
                    LoggedConsole.WriteErrorLine("   WarningException: Negative image index for gridline!");
                    continue;
                }
                for (int x = 1; x < width - 3; x++)
                {
                    bmp.SetPixel(x, y, Color.White);
                    x += 3;
                    bmp.SetPixel(x, y, Color.Black);
                    x += 2;
                }
                g.DrawString((gridLineLocations[b, 1] + ""), new Font("Thachoma", 8), txtColour, 1, y);
            }
        }//end AddHzGridLines()


        public static void DrawFrequencyLinesOnImage(Bitmap bmp, FrequencyScale freqScale )
        {
            DrawFrequencyLinesOnImage(bmp, freqScale.GridLineLocations);
        }


    }
}
