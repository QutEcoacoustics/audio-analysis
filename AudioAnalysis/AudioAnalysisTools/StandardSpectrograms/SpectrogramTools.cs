// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrogramTools.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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
    using System.Drawing.Imaging;
    using System.IO;

    using Acoustics.Shared;

    using AnalysisBase;

    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;

    using TowseyLibrary;
    using ColorMine.ColorSpaces;

    public static class SpectrogramTools
    {

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

                Image image = null;
                AnalysisSettings settings = new AnalysisSettings();
                settings.AudioFile = fiAudio;
                settings.ConfigDict = config.GetDictionary();
                settings.ConfigFile = fiConfig;
                settings.ImageFile = fiImage;
                settings.AnalysisInstanceOutputDirectory = diOutputDir;
                // want to psas SampleRate of the original file.
                settings.SampleRateOfOriginalAudioFile = Int32.Parse(settings.ConfigDict[AnalysisKeys.ResampleRate]);
                var results = analyser.Analyse(settings);
                if (results.ImageFile == null) image = null;
                else image = Image.FromFile(results.ImageFile.FullName);
                analyser = null;
                return image;
            }
            else
            {
                analyser = null;
                var configDict = config.GetDictionary();
                BaseSonogram sonogram = SpectrogramTools.Audio2DecibelSonogram(fiAudio, configDict);
                var mti = SpectrogramTools.Sonogram2MultiTrackImage(sonogram, configDict);
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
        /// <param name="fiConfig"></param>
        /// <returns></returns>
        public static Image Audio2SonogramImage(FileInfo fiAudio, Dictionary<string, string> configDict)
        {
            BaseSonogram sonogram = SpectrogramTools.Audio2DecibelSonogram(fiAudio, configDict);
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
            int cellArea = timeRedFactor * freqRedFactor;
            for (int r = 0; r < timeReducedCount; r++)
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
                        for (int j = 0; j < freqRedFactor; j++)
                        {
                            if (max < data[or + i, oc + j]) max = data[or + i, oc + j];
                        }
                    reducedMatrix[r, c] = max;
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


        public static double[,] NormaliseSpectrogramMatrix(double[,] matrix, double truncateMin, double truncateMax, double backgroundFilterCoeff)
        {
            double[,] m = MatrixTools.NormaliseInZeroOne(matrix, truncateMin, truncateMax);
            m = MatrixTools.FilterBackgroundValues(m, backgroundFilterCoeff); // to de-demphasize the background small values
            return m;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fiAudio"></param>
        /// <param name="fiConfig"></param>
        /// <param name="fiImage"></param>
        /// <returns></returns>
        public static Image_MultiTrack Sonogram2MultiTrackImage(BaseSonogram sonogram, Dictionary<string, string> configDict)
        {
            bool doHighlightSubband = false;


            //check if doing a reduced sonogram
            //int timeReductionFactor = 1;
            //if (configDict.ContainsKey(Keys.TIME_REDUCTION_FACTOR))
            //    timeReductionFactor = ConfigDictionary.GetInt(Keys.TIME_REDUCTION_FACTOR, configDict);
            //int freqReductionFactor = 1;
            //if (configDict.ContainsKey(Keys.FREQ_REDUCTION_FACTOR))
            //    freqReductionFactor = ConfigDictionary.GetInt(Keys.FREQ_REDUCTION_FACTOR, configDict);
            //if (!((timeReductionFactor == 1) && (freqReductionFactor == 1)))
            //{
            //    sonogram.Data = ReduceDimensionalityOfSpectrogram(sonogram.Data, timeReductionFactor, freqReductionFactor);               
            //    return sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //}

            
            // (iii) NOISE REDUCTION
            //bool doNoiseReduction = false;
            //if (configDict.ContainsKey(AnalysisKeys.NoiseDoReduction))
            //    doNoiseReduction = ConfigDictionary.GetBoolean(AnalysisKeys.NoiseDoReduction, configDict);
            //if (doNoiseReduction)
            //{
            //    //LoggedConsole.WriteLine("PERFORMING NOISE REDUCTION");
            //    double bgThreshold = 3.0;
            //    if (configDict.ContainsKey(AnalysisKeys.NoiseBgThreshold))
            //        bgThreshold = ConfigDictionary.GetDouble(AnalysisKeys.NoiseBgThreshold, configDict);
            //    var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, bgThreshold);
            //    sonogram.Data = tuple.Item1;   // store data matrix
            //}

            //ADD time and frequency scales
            bool addScale = false;
            if (configDict.ContainsKey(AnalysisKeys.AddTimeScale)) addScale = ConfigDictionary.GetBoolean(AnalysisKeys.AddTimeScale, configDict);
            else
            if (configDict.ContainsKey(AnalysisKeys.AddAxes))      addScale = ConfigDictionary.GetBoolean(AnalysisKeys.AddAxes, configDict);
            bool add1kHzLines = addScale;


            System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            Image_MultiTrack mti = new Image_MultiTrack(img);
            if (addScale) mti.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond)); //add time scale
            bool addSegmentationTrack = false;

            //add segmentation track
            if (configDict.ContainsKey(AnalysisKeys.AddSegmentationTrack))
                addSegmentationTrack = ConfigDictionary.GetBoolean(AnalysisKeys.AddSegmentationTrack, configDict);
            if (addSegmentationTrack) 
                mti.AddTrack(Image_Track.GetSegmentationTrack(sonogram)); //add segmentation track
            return mti;
            //mti.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram)); //add segmentation track
        }//Sonogram2MultiTrackImage()

        public static Image Sonogram2Image(BaseSonogram sonogram, Dictionary<string, string> configDict, 
                                           double[,] hits, List<Plot> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Image_MultiTrack multiTrackImage = Sonogram2MultiTrackImage(sonogram, configDict);

            if (scores != null)
            {
                foreach (Plot plot in scores)
                    multiTrackImage.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }

            if (hits != null) 
                multiTrackImage.OverlayRainbowTransparency(hits);

            if (predictedEvents.Count > 0) 
                multiTrackImage.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);

            return multiTrackImage.GetImage();
        } //Sonogram2Image()


        public static Image Matrix2SonogramImage(double[,] matrix, SonogramConfig config)
        {
            bool doHighlightSubband = false;
            //ADD time and frequency scales
            bool addScale = true;
            //if (configDict.ContainsKey(Keys.ADD_TIME_SCALE)) addScale = ConfigDictionary.GetBoolean(Keys.ADD_TIME_SCALE, configDict);
            //else
            //    if (configDict.ContainsKey(Keys.ADD_AXES)) addScale = ConfigDictionary.GetBoolean(Keys.ADD_AXES, configDict);
            bool add1kHzLines = addScale;

            BaseSonogram sonogram = new SpectrogramStandard(config, matrix);
            System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            Image_MultiTrack image = new Image_MultiTrack(img);
            //if (addScale) image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond)); //add time scale
            return image.GetImage();
        } //CSV2SonogramImage()


        public static Image CreateFalseColourDecibelSpectrogram(double[,] dbSpectrogramData, double[,] nrSpectrogramData, byte[,] hits)
        {
            double truncateMin = -120.0;
            double truncateMax = -30.0;
            double filterCoefficient = 1.0;
            double[,] dbSpectrogramNorm = SpectrogramTools.NormaliseSpectrogramMatrix(dbSpectrogramData, truncateMin, truncateMax, filterCoefficient);
            truncateMin = 0;
            truncateMax = 60;
            double[,] nrSpectrogramNorm = SpectrogramTools.NormaliseSpectrogramMatrix(nrSpectrogramData, truncateMin, truncateMax, filterCoefficient);

            int width = dbSpectrogramData.GetLength(0);
            int height = dbSpectrogramData.GetLength(1);
            Bitmap image = new Bitmap(width, height);
            Color colour;
            Hsv myHsv;
            Rgb myRgb;
            Color[] ridgeColours = { Color.Red, Color.DarkMagenta, Color.Black, Color.LightPink };

            for (int y = 0; y < height; y++) //over all freq bins
            {
                    for (int x = 0; x < width; x++) //for pixels in the line
                    {
                        // normalise and bound the value - use min bound, max and 255 image intensity range
                        double dbValue = dbSpectrogramNorm[x, y];
                        int c1 = 255 - (int)Math.Floor(255.0 * dbValue); //original version
                        //int c1 = (int)Math.Floor(255.0 * dbValue);
                        if (c1 < 0) c1 = 0;
                        else
                            if (c1 > 255) c1 = 255;
                        colour = Color.FromArgb(c1, c1, c1);

                        if (nrSpectrogramNorm[x, y] > 0)
                        {
                            // use HSV colour space
                            int bottomColour = 30;    // to avoid using the reds
                            int topColour    = 320;   // to avoid using the magentas
                            int hueRange = topColour - bottomColour;
                            int hue = bottomColour + (int)Math.Floor(hueRange * nrSpectrogramNorm[x, y]);

                            double saturation = 1.0;
                            //double saturation = 0.75 + (nrSpectrogramNorm[x, y] * 0.25);
                            //double saturation = nrSpectrogramNorm[x, y] * 0.5;
                            //double saturation = (1 - nrSpectrogramNorm[x, y]) * 0.5;

                            double value = 1.0;
                            //double value = 0.60 + (nrSpectrogramNorm[x, y] * 0.40);

                            myHsv = new Hsv { H = hue, S = saturation, V = value };
                            myRgb = myHsv.To<Rgb>();
                            colour = Color.FromArgb((int)myRgb.R, (int)myRgb.G, (int)myRgb.B);


                            // get colour for noise reduced portion
                            // superimpose ridge detection
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
            }//end over all freq bins

            //image.Save(@"C:\SensorNetworks\Output\Sonograms\TEST3.png", ImageFormat.Png);

            return image;
        }

        public static Image CreateFalseColourAmplitudeSpectrogram(double[,] spectrogramData, double[,] nrSpectrogramData, byte[,] hits)
        {
            //double min, max;
            //MatrixTools.MinMax(spectrogramData, out min, out max);

            double truncateMin = 0.0;
            double truncateMax = 2.0;
            double filterCoefficient = 1.0;
            double[,] spectrogramNorm = SpectrogramTools.NormaliseSpectrogramMatrix(spectrogramData, truncateMin, truncateMax, filterCoefficient);
            //truncateMin = 0.5;
            //truncateMax = 2.0;
            //double[,] nrSpectrogramNorm = SpectrogramTools.NormaliseSpectrogramMatrix(nrSpectrogramData, truncateMin, truncateMax, filterCoefficient);

            int width = spectrogramData.GetLength(0);
            int height = spectrogramData.GetLength(1);
            Bitmap image = new Bitmap(width, height);
            Color colour;
            Hsv myHsv;
            Rgb myRgb;
            Color[] ridgeColours = { Color.Red, Color.Lime, Color.Blue, Color.Lime };

            for (int y = 0; y < height; y++) //over all freq bins
            {
                for (int x = 0; x < width; x++) //for pixels in the line
                {
                    // normalise and bound the value - use min bound, max and 255 image intensity range
                    double dbValue = spectrogramNorm[x, y];
                    int c1 = 255 - (int)Math.Floor(255.0 * dbValue); //original version
                    //int c1 = (int)Math.Floor(255.0 * dbValue);
                    if (c1 < 0) c1 = 0;
                    else
                        if (c1 > 255) c1 = 255;
                    colour = Color.FromArgb(c1, c1, c1);

                    //if (nrSpectrogramNorm[x, y] > 0)
                    //{
                    //    // use HSV colour space
                    //    int bottomColour = 30;    // to avoid using the reds
                    //    int topColour = 320;   // to avoid using the magentas
                    //    int hueRange = topColour - bottomColour;
                    //    int hue = bottomColour + (int)Math.Floor(hueRange * nrSpectrogramNorm[x, y]);

                    //    double saturation = 1.0;
                    //    //double saturation = 0.75 + (nrSpectrogramNorm[x, y] * 0.25);
                    //    //double saturation = nrSpectrogramNorm[x, y] * 0.5;
                    //    //double saturation = (1 - nrSpectrogramNorm[x, y]) * 0.5;

                    //    double value = 1.0;
                    //    //double value = 0.60 + (nrSpectrogramNorm[x, y] * 0.40);

                    //    myHsv = new Hsv { H = hue, S = saturation, V = value };
                    //    myRgb = myHsv.To<Rgb>();
                    //    colour = Color.FromArgb((int)myRgb.R, (int)myRgb.G, (int)myRgb.B);
                    //}

                    // superimpose ridge detection
                    if (hits[x, y] > 0)
                    {
                        //value = 0.60 + (nrSpectrogramNorm[x, y] * 0.40);
                        //myHsv = new Hsv { H = 260, S = saturation, V = value };
                        //myRgb = myHsv.To<Rgb>();
                        //colour = Color.FromArgb((int)myRgb.R, (int)myRgb.G, (int)myRgb.B);
                        colour = ridgeColours[hits[x, y] - 1];
                    }
                    image.SetPixel(x, height - y - 1, colour);
                }
            }//end over all freq bins

            //image.Save(@"C:\SensorNetworks\Output\Sonograms\TEST3.png", ImageFormat.Png);

            return image;
        }





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
            if (configDict.ContainsKey(AnalysisKeys.AddAxes) && (!ConfigDictionary.GetBoolean(AnalysisKeys.AddAxes, configDict)))
            {
                axes = string.Empty;
            }

            string coloured = " -m "; // default
            if (configDict.ContainsKey(AnalysisKeys.SonogramColored) && (ConfigDictionary.GetBoolean(AnalysisKeys.SonogramColored, configDict)))
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
            const string SoxCommandLineArguments = " -V \"{0}\" -n spectrogram -m {1} -q 64 -l -o \"{6}\""; //64 grey scale, with time, freq and intensity scales
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
            var args = string.Format(SoxCommandLineArguments, fiAudio.FullName, title, comment, axes, coloured, quantisation, output.FullName);
            var process = new TowseyLibrary.ProcessRunner(soxCmd);
            process.Run(args, output.DirectoryName);
        }



        /// <summary>
        /// Returns AVERAGE POWER SPECTRUM and VARIANCE OF POWER SPECTRUM
        /// Have been passed the amplitude spectrum but square amplitude values to get power or energy.
        /// </summary>
        /// <param name="spectrogram">this is an amplitude spectrum. Must square values to get power</param>
        /// <returns></returns>
        public static Tuple<double[], double[]> CalculateSpectralAvAndVariance(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] avgSpectrum = new double[freqBinCount];   // for average  of the spectral bins
            double[] varSpectrum = new double[freqBinCount];   // for variance of the spectral bins
            for (int j = 0; j < freqBinCount; j++)             // for all frequency bins
            {
                var freqBin = new double[frameCount];          // set up an array to take all values in a freq bin i.e. column of matrix
                for (int r = 0; r < frameCount; r++)
                {
                    freqBin[r] = spectrogram[r, j] * spectrogram[r, j];  //convert amplitude to energy or power.
                }
                double av, sd;
                NormalDist.AverageAndSD(freqBin, out av, out sd);
                avgSpectrum[j] = av; // store average of the bin
                varSpectrum[j] = sd * sd; // store var of the bin
            }
            return System.Tuple.Create(avgSpectrum, varSpectrum);
        } // CalculateSpectralAvAndVariance()




        /// <summary>
        /// Returns a HISTORGRAM OF THE DISTRIBUTION of SPECTRAL maxima.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="peakThreshold">required amplitude threshold to qualify as peak</param>
        /// <param name="nyquistFreq"></param>
        /// <returns></returns>
        public static Tuple<int[], int[]> HistogramOfSpectralPeaks(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);

            int[] peakBins = new int[frameCount];   // store bin id of peaks - use later for imaging purposes
            int[] histogram = new int[freqBinCount]; // histogram of peak locations
            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);

                int j = DataTools.GetMaxIndex(spectrum); //locate maximum peak
                //if (spectrogram[r, j] > peakThreshold) 
                //{
                histogram[j]++; //
                peakBins[r] = j;  //store bin of peak
                //}
            } // over all frames in dB array

            //DataTools.writeBarGraph(histogram);
            return System.Tuple.Create(histogram, peakBins);
        }



        public static double[,] ExtractFreqSubband(double[,] m, int minHz, int maxHz, bool doMelscale, int binCount, double binWidth)
        {
            int c1;
            int c2;
            AcousticEvent.Freq2BinIDs(doMelscale, minHz, maxHz, binCount, binWidth, out c1, out c2);
            return DataTools.Submatrix(m, 0, c1, m.GetLength(0) - 1, c2);
        }

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
        /// <param name="Nyquist">full freq range 0-Nyquist</param>
        /// <param name="binWidth">the frequency scale i.e. herz per bin width - assumes linear scale</param>
        /// <returns></returns>
        public static double[,] ExtractEvent(double[,] m, double start, double end, double frameOffset,
                                             int minHz, int maxHz, bool doMelscale, int Nyquist, double binWidth)
        {
            int r1;
            int r2;
            AcousticEvent.Time2RowIDs(start, end - start, frameOffset, out r1, out r2);
            int c1;
            int c2;
            AcousticEvent.Freq2BinIDs(doMelscale, minHz, maxHz, Nyquist, binWidth, out c1, out c2);
            return DataTools.Submatrix(m, r1, c1, r2, c2);
        }


        public static double[] ExtractModalNoiseSubband(double[] modalNoise, int minHz, int maxHz, bool doMelScale, int Nyquist, double binWidth)
        {
            //extract subband modal noise profile
            int c1, c2;
            AcousticEvent.Freq2BinIDs(doMelScale, minHz, maxHz, Nyquist, binWidth, out c1, out c2);
            int subbandCount = c2 - c1 + 1;
            var subband = new double[subbandCount];
            for (int i = 0; i < subbandCount; i++) subband[i] = modalNoise[c1 + i];
            return subband;
        }

        // #######################################################################################################################################
        // ### BELOW METHODS DRAW GRID LINES ON SPECTROGRAMS ####################################################################################
        // #######################################################################################################################################


        public static void DrawGridLinesOnImage(Bitmap bmp, TimeSpan minOffset, TimeSpan xAxisTicInterval, TimeSpan xAxisPixelDuration,
                                                    int nyquist, int herzInterval)
        {
            double Y_interval = bmp.Height / (double)(nyquist / (double)herzInterval);
            DrawGridLinesOnImage(bmp, minOffset, xAxisTicInterval, xAxisPixelDuration, Y_interval);
        }

        public static void DrawGridLinesOnImage(Bitmap bmp, TimeSpan minOffset, TimeSpan xAxisTicInterval, TimeSpan xAxisPixelDuration, double Y_interval)
        {
            int rows = bmp.Height;
            int cols = bmp.Width;

            Graphics g = Graphics.FromImage(bmp);

            // for rows draw in Y-axis line
            // number of horozntal grid lines
            int gridCount = (int)(rows / Y_interval);
            for (int i = 1; i < gridCount; i++)
            {
                int row = (int)(i * Y_interval);
                int rowFromBottom = rows - row;
                for (int column = 0; column < cols - 1; column++)
                {
                    bmp.SetPixel(column, rowFromBottom, Color.Black);
                    bmp.SetPixel(column + 1, rowFromBottom, Color.White);
                    column += 2;
                }
                int band = (int)(rowFromBottom / Y_interval);
                g.DrawString(((band) + " kHz"), new Font("Thachoma", 8), Brushes.Black, 2, row - 5);
            }

            // for columns, draw in X-axis hour lines
            int xInterval = (int)Math.Round((xAxisTicInterval.TotalMilliseconds / xAxisPixelDuration.TotalMilliseconds));
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
        } // DrawGridLInesOnImage()



        public static void Draw1kHzLines(Bitmap bmp, bool doMelScale, int nyquist, double freqBinWidth)
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
                gridLineLocations = SpectrogramTools.CreateMelYaxis(kHz, nyquist, height); // WARNING!!!! NEED TO REWORK THIS BUT NOW SELDOM USED

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


        /// <summary>
        /// currently, this method is only called when drawing a reduced sonogram. 
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
        /// Currently this method is only called from Draw1kHzLines(Bitmap bmp, bool doMelScale, int nyquist, double freqBinWidth)
        /// and when bool doMelScale = true;
        /// </summary>
        public static int[] CreateMelYaxis(int herzInterval, int nyquistFreq, int imageHt)
        {
            int minFreq = 0;
            int maxFreq = nyquistFreq;
            //int freqRange = maxFreq - minFreq + 1;
            double minMel = DSP.MFCCStuff.Mel(minFreq);
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

        // #######################################################################################################################################
        // ### ABOVE METHODS DRAW GRID LINES ON SPECTROGRAMS ####################################################################################
        // #######################################################################################################################################




    }
}
