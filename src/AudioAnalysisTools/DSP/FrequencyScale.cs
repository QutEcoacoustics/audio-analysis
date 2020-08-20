// <copyright file="FrequencyScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using Acoustics.Shared.ImageSharp;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    // IMPORTANT NOTE: If you are converting Hertz scale from LINEAR to MEL or OCTAVE, this conversion MUST be done BEFORE noise reduction

    /// <summary>
    /// All the below octave scale options are designed for a final freq scale having 256 bins.
    /// Scale name indicates its structure. You cannot vary the structure.
    /// </summary>
    public enum FreqScaleType
    {
        Linear = 0,
        Mel = 1,
        OctaveCustom = 2,
        OctaveStandard = 3,
        OctaveDataReduction = 4,
    }

    public class FrequencyScale
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR
        /// Calling this constructor assumes the standard linear 0-nyquist freq scale is required.
        /// </summary>
        public FrequencyScale(int nyquist, int frameSize, int hertzGridInterval)
        {
            this.ScaleType = FreqScaleType.Linear;
            this.Nyquist = nyquist;
            this.WindowSize = frameSize;
            this.FinalBinCount = frameSize / 2;
            this.HertzGridInterval = hertzGridInterval;
            this.LinearBound = nyquist;
            this.BinBounds = this.GetLinearBinBounds();
            this.GridLineLocations = GetLinearGridLineLocations(nyquist, this.HertzGridInterval, this.FinalBinCount);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR
        /// Call this constructor when want to change freq scale but keep it linear.
        /// </summary>
        public FrequencyScale(int nyquist, int frameSize, int finalBinCount, int hertzGridInterval)
        {
            this.ScaleType = FreqScaleType.Linear;
            this.Nyquist = nyquist;
            this.WindowSize = frameSize;
            this.FinalBinCount = finalBinCount;
            this.HertzGridInterval = hertzGridInterval;
            this.LinearBound = nyquist;
            this.BinBounds = this.GetLinearBinBounds();
            this.GridLineLocations = GetLinearGridLineLocations(nyquist, this.HertzGridInterval, this.FinalBinCount);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR
        /// Calling this constructor assumes either Linear or Mel is required but not Octave.
        /// </summary>
        public FrequencyScale(FreqScaleType type, int nyquist, int frameSize, int finalBinCount, int hertzGridInterval)
        {
            this.Nyquist = nyquist;
            this.WindowSize = frameSize;
            this.FinalBinCount = finalBinCount;
            this.HertzGridInterval = hertzGridInterval;
            if (type == FreqScaleType.Mel)
            {
                this.BinBounds = MFCCStuff.GetMelBinBounds(this.Nyquist, this.FinalBinCount);
                this.GridLineLocations = SpectrogramMelScale.GetMelGridLineLocations(this.HertzGridInterval, nyquist, this.FinalBinCount);
                this.LinearBound = 1000;
            }
            else if (type == FreqScaleType.OctaveStandard || type == FreqScaleType.OctaveCustom)
            {
                throw new Exception("Wrong Constructor. Use the Octave scale constructor.");
            }
            else
            {
                // linear is the default
                this.BinBounds = this.GetLinearBinBounds();
                this.GridLineLocations = GetLinearGridLineLocations(nyquist, this.HertzGridInterval, this.FinalBinCount);
                this.LinearBound = nyquist;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTION OF OCTAVE Frequency Scales
        /// IMPORTANT NOTE: If you are converting Herz scale from LINEAR to OCTAVE, this conversion MUST be done BEFORE noise reduction
        /// WARNING!: Changing the constants for the octave scales will have undefined effects.
        ///           The options below have been debugged to give what is required.
        ///           However other values have not been debugged - so user should check the output to ensure it is what is required.
        /// NOTE: octaveToneCount = the number of fractional Hz steps within one octave. A piano octave contains 12 steps per octave.
        /// NOTE: Not all combinations of parameter values are effective. nor have they all been tested.
        ///       The following have been tested:
        ///         nyquist=11025, linearBound=125 t0 1000, octaveToneCount=31, 32.
        ///         nyquist=11025, linearBound=125, octaveToneCount=31.
        ///         nyquist=32000, linearBound=125, octaveToneCount=28.
        /// </summary>
        /// <param name="type">Scale type must be an OCTAVE type.</param>
        /// <param name="nyquist">sr / 2.</param>
        /// <param name="frameSize">Assumes that frame size is power of 2.</param>
        /// <param name="linearBound">The bound (Hz value) that divides lower linear and upper octave parts of the frequency scale.</param>
        /// <param name="octaveToneCount">Number of fractional Hz steps within one octave. This is ignored in case of custom scale.</param>
        /// <param name="hertzGridInterval">Only used where appropriate to draw frequency gridlines.</param>
        public FrequencyScale(FreqScaleType type, int nyquist, int frameSize, int linearBound, int octaveToneCount, int hertzGridInterval)
        {
            this.ScaleType = type;
            this.Nyquist = nyquist;
            this.WindowSize = frameSize;
            this.LinearBound = linearBound;

            if (type == FreqScaleType.OctaveStandard)
            {
                // this scale automatically sets the octave tone count.
                OctaveFreqScale.GetStandardOctaveScale(this);
            }
            else if (type == FreqScaleType.OctaveCustom)
            {
                this.ToneCount = octaveToneCount;
                this.BinBounds = OctaveFreqScale.LinearToSplitLinearOctaveScale(nyquist, frameSize, linearBound, nyquist, octaveToneCount);
                this.FinalBinCount = this.BinBounds.GetLength(0);
                this.GridLineLocations = OctaveFreqScale.GetGridLineLocations(nyquist, linearBound, this.BinBounds);
            }
            else
            {
                throw new Exception("Unknown Octave scale.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyScale"/> class.
        /// CONSTRUCTOR.
        /// </summary>
        public FrequencyScale(FreqScaleType fst)
        {
            this.ScaleType = fst;
            if (fst == FreqScaleType.Linear)
            {
                LoggedConsole.WriteErrorLine("WARNING: Assigning DEFAULT parameters for Linear FREQUENCY SCALE.");
                LoggedConsole.WriteErrorLine("         Call other CONSTUCTOR to control linear scale.");
                this.Nyquist = 11025;
                this.WindowSize = 512;
                this.FinalBinCount = 256;
                this.HertzGridInterval = 1000;
                this.LinearBound = this.Nyquist;
                this.BinBounds = this.GetLinearBinBounds();
                this.GridLineLocations = GetLinearGridLineLocations(this.Nyquist, this.HertzGridInterval, 256);
            }
            else if (fst == FreqScaleType.Mel)
            {
                // WARNING: Making this call will return a standard Mel scale where sr = 22050 and frameSize = 512
                // MEL SCALE spectrograms are available by direct call to SpectrogramGenerator.Core.GetMelScaleSpectrogram()
                SpectrogramMelScale.GetStandardMelScale(this);
            }
            else if (fst == FreqScaleType.OctaveDataReduction)
            {
                // This spectral conversion is for data reduction purposes.
                // It is a split linear-octave frequency scale.
                OctaveFreqScale.GetDataReductionScale(this);

                //The data reduction Octave Scale does not require grid lines.
                this.GridLineLocations = new int[6, 2];
            }
            else if (fst == FreqScaleType.OctaveStandard || fst == FreqScaleType.OctaveCustom)
            {
                throw new Exception("Not enough info. Use an Octave scale constructor.");
            }
            else
            {
                throw new Exception("Unknown frequency scale type.");
            }
        }

        /// <summary>
        /// Gets or sets half the sample rate.
        /// </summary>
        public int Nyquist { get; set; }

        /// <summary>
        /// Gets or sets frame size for the FFT.
        /// </summary>
        public int WindowSize { get; set; }

        /// <summary>
        /// Gets or sets step size for the FFT window.
        /// </summary>
        public int WindowStep { get; set; }

        /// <summary>
        /// Gets or sets number of frequency bins in the final spectrogram.
        /// </summary>
        public int FinalBinCount { get; set; }

        /// <summary>
        /// Gets or sets the scale type i.e. linear, octave etc.
        /// </summary>
        public FreqScaleType ScaleType { get; set; }

        /// <summary>
        /// Gets or sets herz interval between gridlines when using a linear or mel scale.
        /// </summary>
        public int HertzGridInterval { get; set; }

        /// <summary>
        /// Gets or sets top end of the linear part of an Octave Scale spectrogram.
        /// </summary>
        public int LinearBound { get; set; }

        /// <summary>
        /// Gets or sets number of bands or tones per octave.
        /// </summary>
        public int ToneCount { get; set; }

        /// <summary>
        /// Gets or sets the bin bounds of the frequency bands for octave scale
        /// bin id in first column and the Hz value in second column of matrix.
        /// </summary>
        public int[,] BinBounds { get; set; }

        /// <summary>
        /// Gets or sets the location of gridlines (first column) and the Hz value for the grid lines (second column of matrix).
        /// </summary>
        public int[,] GridLineLocations { get; set; }

        /// <summary>
        /// returns the binId for freq bin closest to the passed Hertz value.
        /// </summary>
        public int GetBinIdForHerzValue(int hertzValue)
        {
            int binId = 0;
            while (this.BinBounds[binId, 1] < hertzValue)
            {
                binId++;
            }

            return binId;
        }

        /// <summary>
        /// returns the binId for the grid line closest to the passed frequency.
        /// </summary>
        public int GetBinIdInReducedSpectrogramForHerzValue(int herzValue)
        {
            int binId = 0;
            int binCount = this.BinBounds.GetLength(0);

            for (int i = 1; i < binCount; i++)
            {
                if (this.BinBounds[i, 1] > herzValue)
                {
                    binId = i;
                    break;
                }
            }

            // subtract 1 because have actually extracted the upper bin bound
            return binId - 1;
        }

        /// <summary>
        /// Returns an [N, 2] matrix with bin ID in column 1 and lower Herz bound in column 2.
        /// </summary>
        public int[,] GetLinearBinBounds()
        {
            double herzInterval = this.Nyquist / (double)this.FinalBinCount;
            double scaleFactor = this.WindowSize / 2 / (double)this.FinalBinCount;

            var binBounds = new int[this.FinalBinCount, 2];

            for (int i = 0; i < this.FinalBinCount; i++)
            {
                binBounds[i, 0] = (int)Math.Round(i * scaleFactor);
                binBounds[i, 1] = (int)Math.Round(i * herzInterval);
            }

            return binBounds;
        }

        /// <summary>
        /// THis method assumes that the frameSize will be power of 2
        /// FOR DEBUG PURPOSES, when sr = 22050 and frame size = 8192, the following Hz are located at index:
        /// Hz      Index
        /// 15        6
        /// 31       12
        /// 62       23
        /// 125      46
        /// 250      93
        /// 500     186
        /// 1000    372.
        /// </summary>
        public static double[] GetLinearFreqScale(int nyquist, int binCount)
        {
            double freqStep = nyquist / (double)binCount;
            double[] linearFreqScale = new double[binCount];

            for (int i = 0; i < binCount; i++)
            {
                linearFreqScale[i] = freqStep * i;
            }

            return linearFreqScale;
        }

        /// <summary>
        /// T.
        /// </summary>
        public static int[,] GetLinearGridLineLocations(int nyquist, int herzInterval, int binCount)
        {
            // Draw in horizontal grid lines
            double yInterval = binCount / (nyquist / (double)herzInterval);
            int gridCount = (int)(binCount / yInterval);

            var gridLineLocations = new int[gridCount, 2];

            for (int i = 0; i < gridCount; i++)
            {
                int row = (int)((i + 1) * yInterval);
                gridLineLocations[i, 0] = row;
                gridLineLocations[i, 1] = (i + 1) * herzInterval;
            }

            return gridLineLocations;
        }

        public static void DrawFrequencyLinesOnImage(Image<Rgb24> bmp, FrequencyScale freqScale, bool includeLabels)
        {
            DrawFrequencyLinesOnImage(bmp, freqScale.GridLineLocations, includeLabels);
        }

        public static void DrawFrequencyLinesOnImage(Image<Rgb24> bmp, int[,] gridLineLocations, bool includeLabels)
        {
            int minimumSpectrogramWidth = 10;
            if (bmp.Width < minimumSpectrogramWidth)
            {
                // there is no point drawing grid lines on a very narrow image.
                return;
            }

            // attempt to determine background colour of spectrogram i.e. dark false-colour or light.
            // get the average brightness in a neighbourhood of m x n pixels.
            int pixelCount = 0;
            float brightness = 0.0F;
            for (int m = 5; m < minimumSpectrogramWidth; m++)
            {
                for (int n = 5; n < minimumSpectrogramWidth; n++)
                {
                    var bgnColour = bmp[m, n];

                    brightness += bgnColour.GetBrightness();
                    pixelCount++;
                }
            }

            brightness /= pixelCount;
            var txtColour = Color.White;
            if (brightness > 0.5)
            {
                txtColour = Color.Black;
            }

            int width = bmp.Width;
            int height = bmp.Height;
            int bandCount = gridLineLocations.GetLength(0);

            if (gridLineLocations == null || bmp.Height < 50)
            {
                // there is no point placing gridlines on a narrow image. It obscures too much spectrogram.
                return;
            }

            // draw the grid line for each frequency band
            for (int b = 0; b < bandCount; b++)
            {
                int y = height - gridLineLocations[b, 0];
                if (y < 0)
                {
                    LoggedConsole.WriteErrorLine("   WarningException: Negative image index for gridline!");
                    continue;
                }

                for (int x = 1; x < width - 3; x++)
                {
                    bmp[x, y] = Color.White;
                    x += 3;
                    bmp[x, y] = Color.Black;
                    x += 2;
                }
            }

            if (!includeLabels || bmp.Width < 30)
            {
                // there is no point placing Hertz label on a narrow image. It obscures too much spectrogram.
                return;
            }

            bmp.Mutate(g =>
            {
                // draw Hertz label on each band
                for (int b = 0; b < bandCount; b++)
                {
                    int y = height - gridLineLocations[b, 0];
                    int hertzValue = gridLineLocations[b, 1];

                    if (y > 1)
                    {
                        g.DrawTextSafe($"{hertzValue}", Drawing.Tahoma8, txtColour, new PointF(1, y));
                    }
                }
            });
        }
    }
}