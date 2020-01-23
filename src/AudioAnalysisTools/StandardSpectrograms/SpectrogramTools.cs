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
    using System.Drawing;
    using System.IO;
    using Acoustics.Shared;
    using AudioAnalysisTools.WavTools;
    using ColorMine.ColorSpaces;
    using DSP;
    using LongDurationSpectrograms;
    using TowseyLibrary;

    public static class SpectrogramTools
    {
        /*
        /// <summary>
        ///
        /// </summary>
        /// <param name="fiAudio"></param>
        /// <param name="fiConfig"></param>
        /// <param name="fiImage"></param>
        /// <returns></returns>
        public static Image GetImageFromAudioSegment(FileInfo fiAudio, FileInfo fiConfig, FileInfo fiImage, IAnalyser2 analyser)
        {
            var config = new ConfigDictionary(fiConfig.FullName); //read in config file

            bool doAnnotate = config.GetBoolean(AnalysisKeys.AnnotateSonogram);

            //bool doNoiseReduction = config.GetBoolean(Keys.NOISE_DO_REDUCTION);
            //double bgNoiseThreshold = config.GetDouble(Keys.NOISE_BG_REDUCTION);

            var diOutputDir = new DirectoryInfo(Path.GetDirectoryName(fiImage.FullName));

            //Image image = null;

            if (doAnnotate)
            {
                if (analyser == null)
                {
                    string analyisName = config.GetString(AnalysisKeys.AnalysisName);
                    LoggedConsole.WriteLine("\nWARNING: Could not construct annotated image because analysis name not recognized:");
                    LoggedConsole.WriteLine("\t " + analyisName);
                    return null;
                }

                throw new NotSupportedException("Code intentionally broken because it is out of date and not used");
            }
            else
            {
                analyser = null;
                var configDict = config.GetDictionary();
                BaseSonogram sonogram = Audio2DecibelSonogram(fiAudio, configDict);
                var mti = Sonogram2MultiTrackImage(sonogram, configDict);
                var image = mti.GetImage();

                if (image != null)
                {
                    if (fiImage.Exists)
                    {
                        fiImage.Delete();
                    }

                    image.Save(fiImage.FullName, ImageFormat.Png);
                }

                return image;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fiAudio"></param>
        /// <param name="configDict"></param>
        /// <returns></returns>
        public static Image Audio2SonogramImage(FileInfo fiAudio, Dictionary<string, string> configDict)
        {
            BaseSonogram sonogram = Audio2DecibelSonogram(fiAudio, configDict);
            var mti = Sonogram2MultiTrackImage(sonogram, configDict);
            var image = mti.GetImage();
            return image;
        }

        public static double[,] ReduceDimensionalityOfSpectrogram(double[,] data, int timeRedFactor, int freqRedFactor)
        {
            int frameCount = data.GetLength(0);
            int freqBinCount = data.GetLength(1);

            int timeReducedCount = frameCount / timeRedFactor;
            int freqReducedCount = freqBinCount / freqRedFactor;

            var reducedMatrix = new double[timeReducedCount, freqReducedCount];

            //int cellArea = timeRedFactor * freqRedFactor;
            for (int r = 0; r < timeReducedCount; r++)
            {
                for (int c = 0; c < freqReducedCount; c++)
                {
                    int or = r * timeRedFactor;
                    int oc = c * freqRedFactor;

                    //display average of the cell
                    //double sum = 0.0;
                    //for (int i = 0; i < timeRedFactor; i++)
                    //    for (int j = 0; j < freqRedFactor; j++)
                    //    {
                    //        sum += data[or + i, oc + j];
                    //    }
                    //reducedMatrix[r, c] = sum / cellArea;

                    //display the maximum in the cell
                    double max = -100000000.0;
                    for (int i = 0; i < timeRedFactor; i++)
                    {
                        for (int j = 0; j < freqRedFactor; j++)
                        {
                            if (max < data[or + i, oc + j])
                            {
                                max = data[or + i, oc + j];
                            }
                        }
                    }

                    reducedMatrix[r, c] = max;
                }
            }

            return reducedMatrix;
        }//end AI_DimRed

        public static List<double[]> Sonogram2ListOfFreqBinArrays(BaseSonogram sonogram, double dynamicRange)
        {
            //int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            //set up a list of normalised arrays representing the spectrum - one array per freq bin
            var listOfFrequencyBins = new List<double[]>();
            for (int c = 0; c < colCount; c++)
            {
                double[] array = MatrixTools.GetColumn(sonogram.Data, c);
                array = DataTools.NormaliseInZeroOne(array, 0, 50); //##IMPORTANT: ABSOLUTE NORMALISATION 0-50 dB #######################################
                listOfFrequencyBins.Add(array);
            }

            return listOfFrequencyBins;
        } // Sonogram2ListOfFreqBinArrays()

        public static BaseSonogram Audio2DecibelSonogram(FileInfo fiAudio, Dictionary<string, string> configDict)
        {
            AudioRecording recordingSegment = new AudioRecording(fiAudio.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(configDict); //default values config
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            return sonogram;
        }
    */

        /// <summary>
        /// Used to normalise a spectrogram in 0,1
        /// </summary>
        /// <param name="matrix">the spectrogram data</param>
        /// <param name="truncateMin">set all values above to 1.0</param>
        /// <param name="truncateMax">set all values below to zero</param>
        /// <param name="backgroundFilterCoeff">used to de-emphisize the background</param>
        /// <returns>a normalised matrix of spectrogram data</returns>
        public static double[,] NormaliseSpectrogramMatrix(double[,] matrix, double truncateMin, double truncateMax, double backgroundFilterCoeff)
        {
            double[,] m = MatrixTools.NormaliseInZeroOne(matrix, truncateMin, truncateMax);
            m = MatrixTools.FilterBackgroundValues(m, backgroundFilterCoeff); // to de-demphasize the background small values
            return m;
        }

        /// <summary>
        /// THis method draws a sonogram with other useful information attached.
        /// </summary>
        /// <param name="sonogram">of BaseSonogram class.</param>
        /// <param name="events">a list of acoustic events.</param>
        /// <param name="plots">a list of plots relevant to the spectrogram scores.</param>
        /// <param name="hits">not often used - can be null.</param>
        public static Image GetSonogramPlusCharts(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> plots, double[,] hits)
        {
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband: false, add1KHzLines: true, doMelScale: false));
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (plots != null)
            {
                foreach (var plot in plots)
                {
                    image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
                }
            }

            if (hits != null)
            {
                image.OverlayRainbowTransparency(hits);
            }

            if (events != null && events.Count > 0)
            {
                // set colour for the events
                foreach (AcousticEvent ev in events)
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }

                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }

            return image.GetImage();
        }

        /*
        /// <summary>
        ///
        /// </summary>
        public static Image_MultiTrack Sonogram2MultiTrackImage(BaseSonogram sonogram, Dictionary<string, string> configDict)
        {
            bool doHighlightSubband = false;

            //ADD time and frequency scales
            bool addScale = false;
            if (configDict.ContainsKey(AnalysisKeys.AddTimeScale))
            {
                addScale = ConfigDictionary.GetBoolean(AnalysisKeys.AddTimeScale, configDict);
            }
            else
            if (configDict.ContainsKey(AnalysisKeys.AddAxes))
            {
                addScale = ConfigDictionary.GetBoolean(AnalysisKeys.AddAxes, configDict);
            }

            Image img = sonogram.GetImage(doHighlightSubband, add1KHzLines: addScale, doMelScale: false);
            Image_MultiTrack mti = new Image_MultiTrack(img);
            if (addScale)
            {
                mti.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond)); //add time scale
            }

            bool addSegmentationTrack = false;

            //add segmentation track
            if (configDict.ContainsKey(AnalysisKeys.AddSegmentationTrack))
            {
                addSegmentationTrack = ConfigDictionary.GetBoolean(AnalysisKeys.AddSegmentationTrack, configDict);
            }

            if (addSegmentationTrack)
            {
                mti.AddTrack(ImageTrack.GetSegmentationTrack(sonogram)); //add segmentation track
            }

            return mti;
        }
        */

        /*
        public static Image Sonogram2Image(BaseSonogram sonogram, Dictionary<string, string> configDict, double[,] hits, List<Plot> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Image_MultiTrack multiTrackImage = Sonogram2MultiTrackImage(sonogram, configDict);

            if (scores != null)
            {
                foreach (Plot plot in scores)
                {
                    multiTrackImage.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
                }
            }

            if (hits != null)
            {
                multiTrackImage.OverlayRainbowTransparency(hits);
            }

            if (predictedEvents.Count > 0)
            {
                multiTrackImage.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }

            return multiTrackImage.GetImage();
        } //Sonogram2Image()
*/

        /// <summary>
        /// This is experimental method to explore colour rendering of standard spectrograms
        /// Used to convert a standard decibel spectrogram into a colour version using
        /// a colour rendering for three separate properties.
        /// </summary>
        /// <param name="dbSpectrogramData">the raw decibel spectrogram data - assigned to red channel</param>
        /// <param name="nrSpectrogramData">the noise reduced decibel spectrogram data - assigned to green channel</param>
        /// <param name="hits">assigned to ridge colours</param>
        /// <returns>coloured-rendered spectrogram as image</returns>
        public static Image CreateFalseColourDecibelSpectrogram(double[,] dbSpectrogramData, double[,] nrSpectrogramData, byte[,] hits)
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
            Bitmap image = new Bitmap(width, height);
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

                    var colour = Color.FromArgb(c1, c1, c1);

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

                        double value = 1.0;

                        //double value = 0.60 + (nrSpectrogramNorm[x, y] * 0.40);

                        var myHsv = new Hsv { H = hue, S = saturation, V = value };
                        var myRgb = myHsv.To<Rgb>();
                        colour = Color.FromArgb((int)myRgb.R, (int)myRgb.G, (int)myRgb.B);

                        // get colour for noise reduced portion
                        // superimpose ridge detection
                        // Have experimented with a bunch of ideas
                        if (hits[x, y] > 0)
                        {
                            //value = 0.60 + (nrSpectrogramNorm[x, y] * 0.40);
                            //myHsv = new Hsv { H = 260, S = saturation, V = value };
                            //myRgb = myHsv.To<Rgb>();
                            //colour = Color.FromArgb((int)myRgb.R, (int)myRgb.G, (int)myRgb.B);
                            colour = ridgeColours[hits[x, y] - 1];
                        }
                    }

                    image.SetPixel(x, height - y - 1, colour);
                }
            } // freq bins

            //image.Save(@"C:\SensorNetworks\Output\Sonograms\TEST3.png", ImageFormat.Png);

            return image;
        }

        /// <summary>
        /// Creates a false-coloured spectrogram from spectral frame data.
        /// That is, uses normal spectrogram data but draws the raw data in red and then superimposes the noise reduced decibel data
        /// Also uses the spectral "hits" data for highlighting the spectrogram.
        /// ### IMPORTANT WARNING!!!! THIS METHOD ASSUMES THAT BOTH SPECTRAL MATRICES HAVE BEEN NORMALISED IN [0,1].
        /// </summary>
        /// <param name="dbSpectrogramNorm">the raw decibel spectrogram data - assigned to red channel</param>
        /// <param name="nrSpectrogramNorm">the noise reduced decibel spectrogram data - assigned to green channel</param>
        /// <param name="hits">assigned to ridge colours</param>
        /// <returns>coloured-rendered spectrogram as image</returns>
        public static Image CreateFalseColourDecibelSpectrogramForZooming(double[,] dbSpectrogramNorm, double[,] nrSpectrogramNorm, byte[,] hits)
        {
            int width = dbSpectrogramNorm.GetLength(0);
            int height = dbSpectrogramNorm.GetLength(1);
            Bitmap image = new Bitmap(width, height);

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

                    image.SetPixel(x, height - y - 1, colour);
                }
            } // freq bins

            return image;
        }

        /// <summary>
        /// Another experimental method to colour render spectrograms, this time amplitude spectrograms.
        /// </summary>
        public static Image CreateFalseColourAmplitudeSpectrogram(double[,] spectrogramData, double[,] nrSpectrogramData, byte[,] hits)
        {
            double truncateMin = 0.0;
            double truncateMax = 2.0;
            double filterCoefficient = 1.0;
            double[,] spectrogramNorm = NormaliseSpectrogramMatrix(spectrogramData, truncateMin, truncateMax, filterCoefficient);

            int width = spectrogramData.GetLength(0);
            int height = spectrogramData.GetLength(1);
            Bitmap image = new Bitmap(width, height);
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

                    var colour = Color.FromArgb(c1, c1, c1);

                    // superimpose ridge detection
                    if (hits[x, y] > 0)
                    {
                        colour = ridgeColours[hits[x, y] - 1];
                    }

                    image.SetPixel(x, height - y - 1, colour);
                }
            }

            return image;
        }

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

                double av, sd;
                NormalDist.AverageAndSD(freqBin, out av, out sd);
                avgSpectrum[j] = av; // store average of the bin

                //varSpectrum[j] = sd * sd; // store var of the bin
                //covSpectrum[j] = sd * sd / av; //store the coefficient of variation of the bin
            }

            return avgSpectrum;
        }

        /// <summary>
        /// Use this method to average a decibel spectrogram
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
        /// See also DataTools.AntiLogBase10(double value);
        /// </summary>
        /// <param name="array">an array of decibel values</param>
        /// <returns>a decibel value</returns>
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

        /*
        public static double[] CalculateSumSpectrumFromSpectrogram(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);

            // for average  of the spectral bins
            double[] sumSpectrum = new double[freqBinCount];

            // for all frequency bins
            for (int j = 0; j < freqBinCount; j++)
            {
                for (int r = 0; r < frameCount; r++)
                {
                    // add to store for the bin
                    sumSpectrum[j] += spectrogram[r, j];
                }
            }

            return sumSpectrum;
        }
        */

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
        /// <param name="amplitudeSpectrogram">this is an amplitude spectrum. Must square values to get power</param>
        /// <returns>three spectral indices</returns>
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

                double av, sd;
                NormalDist.AverageAndSD(freqBin, out av, out sd);
                avgSpectrum[j] = av; // store average of the bin
                varSpectrum[j] = sd * sd; // store var of the bin
                covSpectrum[j] = sd * sd / av; //store the coefficient of variation of the bin
            }

            return Tuple.Create(avgSpectrum, varSpectrum, covSpectrum);
        }

        /// <summary>
        /// Calculates Stuart gage's NDSI acoustic index from the Power Spectrum derived from a spectrogram.
        /// This method assumes P.D. Welch's method has been used to calculate the PSD.
        /// See method above: CalculateAvgSpectrumAndVarianceSpectrumFromAmplitudeSpectrogram()
        /// </summary>
        /// <param name="psd">power spectral density</param>
        /// <param name="samplerate">original sample rate of the recording. Only used to get nyquist</param>
        /// <param name="lowBound">low ndsi bound</param>
        /// <param name="midBound">mid ndsi bound</param>
        /// <param name="topBound">top ndsi bound</param>
        /// <returns>ndsi</returns>
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
            int c1;
            int c2;
            AcousticEvent.Freq2BinIDs(doMelscale, minHz, maxHz, binCount, binWidth, out c1, out c2);
            return DataTools.Submatrix(m, 0, c1, m.GetLength(0) - 1, c2);
        }

        /*
        /// <summary>
        /// Extracts an acoustic event from a sonogram given the location of a user defined rectangular marquee.
        /// NOTE: Nyquist value is used ONLY if using mel scale.
        /// </summary>
        /// <param name="m">the sonogram data as matrix of reals</param>
        /// <param name="start">start time in seconds</param>
        /// <param name="end">end time in seconds</param>
        /// <param name="frameOffset">the time scale: i.e. the duration in seconds of each frame</param>
        /// <param name="minHz">lower freq bound of the event</param>
        /// <param name="maxHz">upper freq bound of the event</param>
        /// <param name="doMelscale">informs whether the sonogram data is linear or mel scale</param>
        /// <param name="nyquist">full freq range 0-Nyquist</param>
        /// <param name="binWidth">the frequency scale i.e. herz per bin width - assumes linear scale</param>
        /// <returns></returns>
        public static double[,] ExtractEvent(double[,] m, double start, double end, double frameOffset,
                                             int minHz, int maxHz, bool doMelscale, int nyquist, double binWidth)
        {
            int r1;
            int r2;
            AcousticEvent.Time2RowIDs(start, end - start, frameOffset, out r1, out r2);
            int c1;
            int c2;
            AcousticEvent.Freq2BinIDs(doMelscale, minHz, maxHz, nyquist, binWidth, out c1, out c2);
            return DataTools.Submatrix(m, r1, c1, r2, c2);
        }
        */

        public static double[] ExtractModalNoiseSubband(double[] modalNoise, int minHz, int maxHz, bool doMelScale, int nyquist, double binWidth)
        {
            //extract subband modal noise profile
            int c1, c2;
            AcousticEvent.Freq2BinIDs(doMelScale, minHz, maxHz, nyquist, binWidth, out c1, out c2);
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
        /// <param name="bmp">the spectrogram image</param>
        /// <param name="startOffset">start Offset</param>
        /// <param name="fullDuration">full Duration</param>
        /// <param name="xAxisTicInterval">xAxis Tic Interval</param>
        /// <param name="freqScale">freq Scale</param>
        public static void DrawGridLinesOnImage(Bitmap bmp, TimeSpan startOffset, TimeSpan fullDuration, TimeSpan xAxisTicInterval, FrequencyScale freqScale)
        {
            FrequencyScale.DrawFrequencyLinesOnImage(bmp, freqScale, includeLabels: true);

            // We have stopped drawing temporal gridlines on these spectrograms. Create unnecessary clutter.
            //DrawTimeLinesOnImage(bmp, startOffset, fullDuration, xAxisTicInterval);
        }

        public static void DrawTimeLinesOnImage(Bitmap bmp, TimeSpan startOffset, TimeSpan fullDuration, TimeSpan xAxisTicInterval)
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
                        bmp.SetPixel(column, row, Color.Black);
                        bmp.SetPixel(column, row + 1, Color.White);
                        row += 2;
                    }
                }
            }
        }

        // #######################################################################################################################################
        // ### ABOVE METHODS DRAW TIME GRID LINES ON SPECTROGRAMS ####################################################################################
        // #######################################################################################################################################


        public static Image GetImageFullyAnnotated(Image image, string title, int[,] gridLineLocations, TimeSpan duration)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            FrequencyScale.DrawFrequencyLinesOnImage((Bitmap)image, gridLineLocations, includeLabels: true);

            var titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            var timeBmp = ImageTrack.DrawTimeTrack(duration, image.Width);
            var list = new List<Image> { titleBar, timeBmp, image, timeBmp };
            var compositeImage = ImageTools.CombineImagesVertically(list);
            return compositeImage;
        }

        public static Image GetImage(double[,] data, int nyquist, bool DoMel)
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
