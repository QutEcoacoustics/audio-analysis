// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrogramTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SpectrogramTools type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.Contracts;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.ColorSpaces;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    public static class SpectrogramTools
    {
        /// <summary>
        /// Used to normalise a spectrogram in 0,1.
        /// </summary>
        /// <param name="matrix">the spectrogram data.</param>
        /// <param name="truncateMin">set all values above to 1.0.</param>
        /// <param name="truncateMax">set all values below to zero.</param>
        /// <param name="backgroundFilterCoeff">used to de-emphisize the background.</param>
        /// <returns>a normalised matrix of spectrogram data.</returns>
        public static double[,] NormaliseSpectrogramMatrix(double[,] matrix, double truncateMin, double truncateMax, double backgroundFilterCoeff)
        {
            double[,] m = MatrixTools.NormaliseInZeroOne(matrix, truncateMin, truncateMax);
            m = MatrixTools.FilterBackgroundValues(m, backgroundFilterCoeff); // to de-demphasize the background small values
            return m;
        }

        /// <summary>
        /// This method draws a spectrogram with other useful information attached.
        /// </summary>
        /// <param name="sonogram">of BaseSonogram class.</param>
        /// <param name="events">a list of acoustic events.</param>
        /// <param name="plots">a list of plots relevant to the spectrogram scores.</param>
        /// <param name="hits">not often used - can be null.</param>
        public static Image<Rgb24> GetSonogramPlusCharts(
            BaseSonogram sonogram,
            List<AcousticEvent> events,
            List<Plot> plots,
            double[,] hits)
        {
            var spectrogram = sonogram.GetImage(doHighlightSubband: false, add1KHzLines: true, doMelScale: false);
            Contract.RequiresNotNull(spectrogram, nameof(spectrogram));

            var height = spectrogram.Height;
            var frameSize = sonogram.Configuration.WindowSize;

            // init with linear frequency scale and draw freq grid lines on image
            int hertzInterval = 1000;
            if (height < 200)
            {
                hertzInterval = 2000;
            }

            var freqScale = new FrequencyScale(sonogram.NyquistFrequency, frameSize, hertzInterval);
            FrequencyScale.DrawFrequencyLinesOnImage(spectrogram, freqScale.GridLineLocations, includeLabels: true);

            // draw event outlines onto spectrogram.
            if (events != null && events.Count > 0)
            {
                // set colour for the events
                foreach (AcousticEvent ev in events)
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                    ev.DrawEvent(spectrogram, sonogram.FramesPerSecond, sonogram.FBinWidth, height);
                }
            }

            // now add in hits to the spectrogram image.
            if (hits != null)
            {
                spectrogram = Image_MultiTrack.OverlayScoresAsRedTransparency(spectrogram, hits);

                // following line needs to be reworked if want to call OverlayRainbowTransparency(hits); 
                //image.OverlayRainbowTransparency(hits);
            }

            int pixelWidth = spectrogram.Width;
            var titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram("TITLE", pixelWidth);
            var timeTrack = ImageTrack.DrawTimeTrack(sonogram.Duration, pixelWidth);

            var imageList = new List<Image<Rgb24>>
            {
                titleBar,
                timeTrack,
                spectrogram,
                timeTrack,
            };

            if (plots != null)
            {
                foreach (var plot in plots)
                {
                    // Next line assumes plot data normalised in 0,1
                    var plotImage = plot.DrawAnnotatedPlot(ImageTrack.DefaultHeight);

                    // the following draws same plot without the title.
                    //var plotImage = ImageTrack.DrawScoreArrayTrack(plot.data, plot.threshold, pixelWidth);
                    imageList.Add(plotImage);
                }
            }

            var compositeImage = ImageTools.CombineImagesVertically(imageList);

            return compositeImage;
        }

        /// <summary>
        /// TODO: This method needs a unit test.
        /// This is experimental method to explore colour rendering of standard spectrograms
        /// Used to convert a standard decibel spectrogram into a colour version using
        /// a colour rendering for three separate properties.
        /// </summary>
        /// <param name="dbSpectrogramData">the raw decibel spectrogram data - assigned to red channel.</param>
        /// <param name="nrSpectrogramData">the noise reduced decibel spectrogram data - assigned to green channel.</param>
        /// <param name="hits">assigned to ridge colours.</param>
        /// <returns>coloured-rendered spectrogram as image.</returns>
        public static Image<Rgb24> CreateFalseColourDecibelSpectrogram(double[,] dbSpectrogramData, double[,] nrSpectrogramData, byte[,] hits)
        {
            double truncateMin = -120.0;
            double truncateMax = -30.0;
            double filterCoefficient = 1.0;
            double[,] dbSpectrogramNorm = NormaliseSpectrogramMatrix(dbSpectrogramData, truncateMin, truncateMax, filterCoefficient);
            truncateMin = 0;
            truncateMax = 60;
            double[,] nrSpectrogramNorm = NormaliseSpectrogramMatrix(nrSpectrogramData, truncateMin, truncateMax, filterCoefficient);

            int width = dbSpectrogramData.GetLength(0);
            int height = dbSpectrogramData.GetLength(1);
            Image<Rgb24> image = new Image<Rgb24>(width, height);
            var converter = new SixLabors.ImageSharp.ColorSpaces.Conversion.ColorSpaceConverter();
            Color[] ridgeColours = { Color.Red, Color.DarkMagenta, Color.Black, Color.LightPink };

            // for all freq bins
            for (int y = 0; y < height; y++)
            {
                //for pixels in freq bin
                for (int x = 0; x < width; x++)
                {
                    // NormaliseMatrixValues and bound the value - use min bound, max and 255 image intensity range
                    double dbValue = dbSpectrogramNorm[x, y];
                    int c1 = 255 - (int)Math.Floor(255.0 * dbValue); //original version

                    //int c1 = (int)Math.Floor(255.0 * dbValue);
                    if (c1 < 0)
                    {
                        c1 = 0;
                    }
                    else
                        if (c1 > 255)
                    {
                        c1 = 255;
                    }

                    var colour = Color.FromRgb((byte)c1, (byte)c1, (byte)c1);

                    if (nrSpectrogramNorm[x, y] > 0)
                    {
                        // use HSV colour space
                        int bottomColour = 30;    // to avoid using the reds
                        int topColour = 320;   // to avoid using the magentas
                        int hueRange = topColour - bottomColour;
                        int hue = bottomColour + (int)Math.Floor(hueRange * nrSpectrogramNorm[x, y]);

                        double saturation = 1.0;

                        //double saturation = 0.75 + (nrSpectrogramNorm[x, y] * 0.25);
                        //double saturation = nrSpectrogramNorm[x, y] * 0.5;
                        //double saturation = (1 - nrSpectrogramNorm[x, y]) * 0.5;

                        //Convert HSV color space to RGB
                        // for this require instance of a SixLabors colour converter.
                        var myHsv = new Hsv(hue, (float)saturation, 1.0f);
                        var myRgb = converter.ToRgb(myHsv);
                        colour = Color.FromRgb((byte)(myRgb.R * 255), (byte)(myRgb.G * 255), (byte)(myRgb.B * 255));

                        // get colour for noise reduced portion
                        // superimpose ridge detection
                        // Have experimented with a bunch of ideas
                        if (hits[x, y] > 0)
                        {
                            //value = 0.60 + (nrSpectrogramNorm[x, y] * 0.40);
                            //myHsv = new Hsv { H = 260, S = saturation, V = value };
                            //myRgb = myHsv.To<Rgb>();
                            //colour = Color.FromRgb((int)myRgb.R, (int)myRgb.G, (int)myRgb.B);
                            colour = ridgeColours[hits[x, y] - 1];
                        }
                    }

                    image[x, height - y - 1] = colour;
                }
            } // freq bins

            return image;
        }

        /// <summary>
        /// Creates a false-coloured spectrogram from spectral frame data.
        /// That is, uses normal spectrogram data but draws the raw data in red and then superimposes the noise reduced decibel data
        /// Also uses the spectral "hits" data for highlighting the spectrogram.
        /// ### IMPORTANT WARNING!!!! THIS METHOD ASSUMES THAT BOTH SPECTRAL MATRICES HAVE BEEN NORMALISED IN [0,1].
        /// </summary>
        /// <param name="dbSpectrogramNorm">the raw decibel spectrogram data - assigned to red channel.</param>
        /// <param name="nrSpectrogramNorm">the noise reduced decibel spectrogram data - assigned to green channel.</param>
        /// <param name="hits">assigned to ridge colours.</param>
        /// <returns>coloured-rendered spectrogram as image.</returns>
        public static Image<Rgb24> CreateFalseColourDecibelSpectrogramForZooming(double[,] dbSpectrogramNorm, double[,] nrSpectrogramNorm, byte[,] hits)
        {
            int width = dbSpectrogramNorm.GetLength(0);
            int height = dbSpectrogramNorm.GetLength(1);
            Image<Rgb24> image = new Image<Rgb24>(width, height);

            // get red scale pallette
            var rsp = new CubeHelix("redscale");

            // get the colour cube helix
            var cch = CubeHelix.GetCubeHelix();

            //var csp = new CubeHelix("cyanscale");

            //over all freq bins
            for (int y = 0; y < height; y++)
            {
                //for pixels in the line
                for (int x = 0; x < width; x++)
                {
                    var colour = rsp.GetColorFromPallette(dbSpectrogramNorm[x, y]);

                    if (nrSpectrogramNorm[x, y] > 0.15)
                    {
                        // get colour for noise reduced portion
                        int colourId = cch.GetColorID(nrSpectrogramNorm[x, y]);

                        // superimpose ridge detection
                        if (hits[x, y] > 0)
                        {
                            colourId += 20;
                            if (colourId > 255)
                            {
                                colourId = 255;
                            }
                        }

                        colour = cch.GetColorFromPallette(colourId);
                    }

                    image[x, height - y - 1] = colour;
                }
            } // freq bins

            return image;
        }

        /// <summary>
        /// Another experimental method to colour render spectrograms, this time amplitude spectrograms.
        /// </summary>
        public static Image<Rgb24> CreateFalseColourAmplitudeSpectrogram(double[,] spectrogramData, double[,] nrSpectrogramData, byte[,] hits)
        {
            double truncateMin = 0.0;
            double truncateMax = 2.0;
            double filterCoefficient = 1.0;
            double[,] spectrogramNorm = NormaliseSpectrogramMatrix(spectrogramData, truncateMin, truncateMax, filterCoefficient);

            int width = spectrogramData.GetLength(0);
            int height = spectrogramData.GetLength(1);
            Image<Rgb24> image = new Image<Rgb24>(width, height);
            Color[] ridgeColours = { Color.Red, Color.Lime, Color.Blue, Color.Lime };

            //over all freq bins
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // NormaliseMatrixValues and bound the value - use min bound, max and 255 image intensity range
                    double dbValue = spectrogramNorm[x, y];
                    int c1 = 255 - (int)Math.Floor(255.0 * dbValue); //original version

                    //int c1 = (int)Math.Floor(255.0 * dbValue);
                    if (c1 < 0)
                    {
                        c1 = 0;
                    }
                    else
                        if (c1 > 255)
                    {
                        c1 = 255;
                    }

                    var colour = Color.FromRgb((byte)c1, (byte)c1, (byte)c1);

                    // superimpose ridge detection
                    if (hits[x, y] > 0)
                    {
                        colour = ridgeColours[hits[x, y] - 1];
                    }

                    image[x, height - y - 1] = colour;
                }
            }

            return image;
        }

        /*
        /// <summary>
        /// Method to make spectrogram with SOX
        /// But the ConfigDictionary clsas is now obsolete.
        /// The method should be depracted some time.
        /// </summary>
        public static void MakeSonogramWithSox(FileInfo fiAudio, Dictionary<string, string> configDict, FileInfo output)
        {
            var soxPath = new FileInfo(AppConfigHelper.SoxExe);
            if (!soxPath.Exists)
            {
                LoggedConsole.WriteLine("SOX ERROR: Path does not exist: <{0}>", soxPath.FullName);
                throw new FileNotFoundException("SOX ERROR: Path for executable does not exist.", soxPath.FullName);
            }

            // must quote the path because has a space in it.
            string soxCmd = "\"" + AppConfigHelper.SoxExe + "\"";

            string title = string.Empty;
            if (configDict.ContainsKey(AnalysisKeys.SonogramTitle))
            {
                title = " -t " + configDict[AnalysisKeys.SonogramTitle];
            }

            string comment = string.Empty;
            if (configDict.ContainsKey(AnalysisKeys.SonogramComment))
            {
                comment = " -c " + configDict[AnalysisKeys.SonogramComment];
            }

            string axes = "-r";
            if (configDict.ContainsKey(AnalysisKeys.AddAxes) && !ConfigDictionary.GetBoolean(AnalysisKeys.AddAxes, configDict))
            {
                axes = string.Empty;
            }

            string coloured = " -m "; // default
            if (configDict.ContainsKey(AnalysisKeys.SonogramColored) && ConfigDictionary.GetBoolean(AnalysisKeys.SonogramColored, configDict))
            {
                coloured = string.Empty;
            }

            string quantisation = " -q 64 "; // default
            if (configDict.ContainsKey(AnalysisKeys.SonogramQuantisation))
            {
                quantisation = " -q " + ConfigDictionary.GetInt(AnalysisKeys.SonogramQuantisation, configDict);
            }

            //          Path\sox.exe  -V "sourcefile.wav" -n rate 22050 spectrogram -m -r -l -a -q 249 -w hann -y 257 -X 43.06640625 -z 100 -o "imagefile.png"
            //string soxCommandLineArguments = " -V \"{0}\" -n rate 22050 spectrogram -m -r -l -a -q 249 -w hann -y 257 -X 43.06640625 -z 100 -o \"{1}\"";  //greyscale only
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -m -l -o \"{1}\"";  //greyscale with time, freq and intensity scales
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -m -o \"{1}\"";     //reverse image greyscale with time, freq and intensity scales
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -l -o \"{1}\"";     //colour with time, freq and intensity scales
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -m -q 64 -r -l -o \"{6}\"";    //64 grey scale, with time, freq and intensity scales
            const string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -m {1} -q 64 -l -o \"{6}\""; //64 grey scale, with time, freq and intensity scales

            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -l {1} {2} {3} {4} {5} -o \"{6}\"";    //64 grey scale, with time, freq and intensity scales

            // FOR COMMAND LINE OPTIONS SEE:  http://sox.sourceforge.net/sox.html
            // −a     Suppress display of axis lines. This is sometimes useful in helping to discern artefacts at the spectrogram edges.
            // -l     Print firendly monochrome spectrogram.
            // −m     Creates a monochrome spectrogram (the default is colour).
            // -q     Number of intensity quanitisation levels/colors - try -q 64
            // −r     Raw spectrogram: suppress the display of axes and legends.
            // −t text  Set the image title - text to display above the spectrogram.
            // −c text  Set (or clear) the image comment - text to display below and to the left of the spectrogram.
            // trim 20 30  displays spectrogram of 30 seconds duratoin starting at 20 seconds.
            var args = string.Format(soxCommandLineArguments, fiAudio.FullName, title, null, null, null, null, output.FullName);
            using (var process = new ProcessRunner(soxCmd))
            {
                process.Run(args, output.DirectoryName);
            }
        }
        */

        /// <summary>
        /// NOTE: This method should not be used to average a decibel spectrogram.
        /// Use only for power spectrograms.
        /// </summary>
        public static double[] CalculateAvgSpectrumFromEnergySpectrogram(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] avgSpectrum = new double[freqBinCount];   // for average  of the spectral bins

            //double[] varSpectrum = new double[freqBinCount]; // for variance of the spectral bins
            //double[] covSpectrum = new double[freqBinCount]; // for coeff of variance of the spectral bins
            for (int j = 0; j < freqBinCount; j++)
            {
                // set up an array to take all values in a freq bin i.e. column of matrix
                var freqBin = new double[frameCount];
                for (int r = 0; r < frameCount; r++)
                {
                    freqBin[r] = spectrogram[r, j];
                }

                NormalDist.AverageAndSD(freqBin, out var av, out var sd);
                avgSpectrum[j] = av; // store average of the bin

                //varSpectrum[j] = sd * sd; // store var of the bin
                //covSpectrum[j] = sd * sd / av; //store the coefficient of variation of the bin
            }

            return avgSpectrum;
        }

        /// <summary>
        /// Use this method to average a decibel spectrogram.
        /// </summary>
        public static double[] CalculateAvgDecibelSpectrumFromDecibelSpectrogram(double[,] spectrogram)
        {
            int freqBinCount = spectrogram.GetLength(1);
            double[] avgSpectrum = new double[freqBinCount];
            for (int j = 0; j < freqBinCount; j++)
            {
                var freqBin = MatrixTools.GetColumn(spectrogram, j);
                double av = AverageAnArrayOfDecibelValues(freqBin);
                avgSpectrum[j] = av;
            }

            return avgSpectrum;
        }

        /// <summary>
        /// Here is some test data for this method: array = new[] { 96.0, 100.0, 90.0, 97.0 };
        /// The return value should = 96.988 dB
        /// First need to calculate the original value i.e. exponential or antilog.
        /// See also DataTools.AntiLogBase10(double value).
        /// </summary>
        /// <param name="array">an array of decibel values.</param>
        /// <returns>a decibel value.</returns>
        public static double AverageAnArrayOfDecibelValues(double[] array)
        {
            int count = array.Length;
            double sum = 0.0;
            for (int j = 0; j < count; j++)
            {
                sum += Math.Exp(array[j] / 10 * Math.Log(10));
            }

            double av = sum / count;
            double dB = Math.Log10(av) * 10;
            return dB;
        }

        /// <summary>
        /// Returns AVERAGE POWER SPECTRUM (PSD) and VARIANCE OF POWER SPECTRUM.
        /// Have been passed the amplitude spectrum but square amplitude values to get power or energy.
        ///
        /// This method assumes that the passed amplitude spectrogram has been prepared according to method of P.D. Welch.
        /// It is the standard method used now to calculate a PSD.
        /// Welch's method splits time series into overlapping segments and windows them.
        /// It is the windowing that makes Welche's method different. Normally overlap windows because windows decay at edges and therefore loss of info.
        /// Can now do FFT. Does not need to be FFT, but if so then window must be power of 2.
        /// Square the FFT coefficients >>>> energy. Then take average in each frquncy bin. Averaging reduces the variance.
        /// Welch's method is an improvement on the standard periodogram spectrum estimating method and on Bartlett's method,
        /// in that it reduces noise in the estimated power spectra in exchange for reducing the frequency resolution.
        /// The end result is an array of power measurements vs. frequency "bin".
        ///
        /// As well as calculating the av power spectrum, this method also returns a variance spectrum and a spectrum of the Coeff of Variation = var/mean.
        /// </summary>
        /// <param name="amplitudeSpectrogram">this is an amplitude spectrum. Must square values to get power.</param>
        /// <returns>three spectral indices.</returns>
        public static Tuple<double[], double[], double[]> CalculateAvgSpectrumAndVarianceSpectrumFromAmplitudeSpectrogram(double[,] amplitudeSpectrogram)
        {
            int frameCount = amplitudeSpectrogram.GetLength(0);
            int freqBinCount = amplitudeSpectrogram.GetLength(1);
            double[] avgSpectrum = new double[freqBinCount];   // for average  of the spectral bins
            double[] varSpectrum = new double[freqBinCount];   // for variance of the spectral bins
            double[] covSpectrum = new double[freqBinCount];   // for coeff of variance of the spectral bins

            // for all frequency bins
            for (int j = 0; j < freqBinCount; j++)
            {
                var freqBin = new double[frameCount];          // set up an array to take all values in a freq bin i.e. column of matrix
                for (int r = 0; r < frameCount; r++)
                {
                    freqBin[r] = amplitudeSpectrogram[r, j] * amplitudeSpectrogram[r, j];  //convert amplitude to energy or power.
                }

                NormalDist.AverageAndSD(freqBin, out var av, out var sd);
                avgSpectrum[j] = av; // store average of the bin
                varSpectrum[j] = sd * sd; // store var of the bin
                covSpectrum[j] = sd * sd / av; //store the coefficient of variation of the bin
            }

            return Tuple.Create(avgSpectrum, varSpectrum, covSpectrum);
        }

        /// <summary>
        /// Calculates Stuart Gage's NDSI acoustic index from the Power Spectrum derived from a spectrogram.
        /// This method assumes P.D. Welch's method has been used to calculate the PSD.
        /// See method above: CalculateAvgSpectrumAndVarianceSpectrumFromAmplitudeSpectrogram().
        /// </summary>
        /// <param name="psd">power spectral density.</param>
        /// <param name="samplerate">original sample rate of the recording. Only used to get nyquist.</param>
        /// <param name="lowBound">low ndsi bound.</param>
        /// <param name="midBound">mid ndsi bound.</param>
        /// <param name="topBound">top ndsi bound.</param>
        /// <returns>ndsi.</returns>
        public static double CalculateNdsi(double[] psd, int samplerate, int lowBound, int midBound, int topBound)
        {
            int nyquist = samplerate / 2;
            int binCount = psd.Length;
            double binWidth = nyquist / (double)binCount;

            // skip lower 1kHz bin;
            int countOf1KHbin = (int)Math.Floor(lowBound / binWidth);
            int countOf2KHbin = (int)Math.Floor(midBound / binWidth);
            int countOf8KHbin = (int)Math.Floor(topBound / binWidth);

            // error checking - required for marine recordings where SR=2000.
            // all this is arbitrary hack to get something working for marine recordings. Will not affect terrestrial recordings
            if (countOf8KHbin >= binCount)
            {
                countOf8KHbin = binCount - 2;
            }

            if (countOf2KHbin >= countOf8KHbin)
            {
                countOf2KHbin = countOf8KHbin - 100;
            }

            if (countOf1KHbin >= countOf2KHbin)
            {
                countOf1KHbin = countOf2KHbin - 10;
            }

            double anthropoEnergy = 0.0;
            for (int i = countOf1KHbin; i < countOf2KHbin; i++)
            {
                anthropoEnergy += psd[i];
            }

            double biophonyEnergy = 0.0;
            for (int i = countOf2KHbin; i < countOf8KHbin; i++)
            {
                biophonyEnergy += psd[i];
            }

            double ndsi = (biophonyEnergy - anthropoEnergy) / (biophonyEnergy + anthropoEnergy);
            return ndsi;
        }

        /// <summary>
        /// Returns a HISTORGRAM OF THE DISTRIBUTION of SPECTRAL maxima.
        /// </summary>
        public static Tuple<int[], int[]> HistogramOfSpectralPeaks(double[,] spectrogram)
        {
            if (spectrogram == null)
            {
                throw new ArgumentNullException(nameof(spectrogram));
            }

            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);

            int[] peakBins = new int[frameCount]; // store bin id of peaks
            int[] histogram = new int[freqBinCount]; // histogram of peak locations

            // for all frames in dB array
            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);

                //locate maximum peak
                int j = DataTools.GetMaxIndex(spectrum);

                //if (spectrogram[r, j] > peakThreshold)
                //{
                histogram[j]++;

                //store bin of peak
                peakBins[r] = j;

                //}
            }

            return Tuple.Create(histogram, peakBins);
        }

        public static double[,] ExtractFreqSubband(double[,] m, int minHz, int maxHz, bool doMelscale, int binCount, double binWidth)
        {
            AcousticEvent.ConvertHertzToFrequencyBin(doMelscale, minHz, maxHz, binCount, binWidth, out var c1, out var c2);
            return DataTools.Submatrix(m, 0, c1, m.GetLength(0) - 1, c2);
        }

        public static double[] ExtractModalNoiseSubband(double[] modalNoise, int minHz, int maxHz, bool doMelScale, int nyquist, double binWidth)
        {
            //extract subband modal noise profile
            AcousticEvent.ConvertHertzToFrequencyBin(doMelScale, minHz, maxHz, nyquist, binWidth, out var c1, out var c2);
            int subbandCount = c2 - c1 + 1;
            var subband = new double[subbandCount];
            for (int i = 0; i < subbandCount; i++)
            {
                subband[i] = modalNoise[c1 + i];
            }

            return subband;
        }

        // #######################################################################################################################################
        // ### BELOW METHODS DRAW GRID LINES ON SPECTROGRAMS #####################################################################################
        // #######################################################################################################################################

        /// <summary>
        /// Only calls method to draw frequency lines but may in future want to add the times scale.
        /// </summary>
        /// <param name="bmp">the spectrogram image.</param>
        /// <param name="startOffset">start Offset.</param>
        /// <param name="fullDuration">full Duration.</param>
        /// <param name="xAxisTicInterval">xAxis Tic Interval.</param>
        /// <param name="freqScale">freq Scale.</param>
        public static void DrawGridLinesOnImage(Image<Rgb24> bmp, TimeSpan startOffset, TimeSpan fullDuration, TimeSpan xAxisTicInterval, FrequencyScale freqScale)
        {
            FrequencyScale.DrawFrequencyLinesOnImage(bmp, freqScale, includeLabels: true);

            // We have stopped drawing temporal gridlines on these spectrograms. Create unnecessary clutter.
            //DrawTimeLinesOnImage(bmp, startOffset, fullDuration, xAxisTicInterval);
        }

        public static void DrawTimeLinesOnImage(Image<Rgb24> bmp, TimeSpan startOffset, TimeSpan fullDuration, TimeSpan xAxisTicInterval)
        {
            int rows = bmp.Height;
            int cols = bmp.Width;
            double xAxisPixelDurationInMilliseconds = fullDuration.TotalMilliseconds / cols;
            int xInterval = (int)Math.Round(xAxisTicInterval.TotalMilliseconds / xAxisPixelDurationInMilliseconds);
            for (int column = 1; column < cols; column++)
            {
                if (column % xInterval == 0)
                {
                    for (int row = 0; row < rows - 1; row++)
                    {
                        bmp[column, row] = Color.Black;
                        bmp[column, row + 1] = Color.White;
                        row += 2;
                    }
                }
            }
        }

        // #######################################################################################################################################
        // ### ABOVE METHODS DRAW TIME GRID LINES ON SPECTROGRAMS ####################################################################################
        // #######################################################################################################################################

        public static Image<Rgb24> GetImageFullyAnnotated(Image<Rgb24> image, string title, int[,] gridLineLocations, TimeSpan duration)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            FrequencyScale.DrawFrequencyLinesOnImage((Image<Rgb24>)image, gridLineLocations, includeLabels: true);

            var titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            var timeBmp = ImageTrack.DrawTimeTrack(duration, image.Width);
            var compositeImage = ImageTools.CombineImagesVertically(titleBar, timeBmp, image, timeBmp);
            return compositeImage;
        }

        public static Image<Rgb24> GetImage(double[,] data, int nyquist, bool DoMel)
        {
            int subBandMinHz = 1000;
            int subBandMaxHz = 9000;
            bool doHighlightSubband = false;

            int maxFrequency = nyquist;
            var image = BaseSonogram.GetSonogramImage(data, nyquist, maxFrequency, DoMel, 1, doHighlightSubband, subBandMinHz, subBandMaxHz);
            return image;
        }
    }
}