using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

using AnalysisBase;

using TowseyLib;
using Acoustics.Tools.Audio;
using AudioAnalysisTools.Sonogram;
using Acoustics.Shared;

namespace AudioAnalysisTools
{
    public static class SpectrogramTools
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fiAudio"></param>
        /// <param name="fiConfig"></param>
        /// <param name="fiImage"></param>
        /// <returns></returns>
        public static Image GetImageFromAudioSegment(FileInfo fiAudio, FileInfo fiConfig, FileInfo fiImage, IAnalyser analyser)
        {
            var config = new ConfigDictionary(fiConfig.FullName); //read in config file

            bool doAnnotate = config.GetBoolean(Keys.ANNOTATE_SONOGRAM);
            //bool doNoiseReduction = config.GetBoolean(Keys.NOISE_DO_REDUCTION);
            //double bgNoiseThreshold = config.GetDouble(Keys.NOISE_BG_REDUCTION);

            var diOutputDir = new DirectoryInfo(Path.GetDirectoryName(fiImage.FullName));
            //Image image = null;

            if (doAnnotate)
            {
                if (analyser == null)
                {
                    string analyisName = config.GetString(Keys.ANALYSIS_NAME);
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
                settings.SampleRateOfOriginalAudioFile = Int32.Parse(settings.ConfigDict[Keys.RESAMPLE_RATE]);
                var results = analyser.Analyse(settings);
                if (results.ImageFile == null) image = null;
                else image = Image.FromFile(results.ImageFile.FullName);
                analyser = null;
                return image;
            }
            else
            {
                analyser = null;
                Image image = SpectrogramTools.Audio2SonogramImage(fiAudio, config.GetDictionary());
                if (image != null)
                {
                    if (fiImage.Exists) fiImage.Delete();
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
            BaseSonogram sonogram = SpectrogramTools.Audio2Sonogram(fiAudio, configDict);
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



        public static BaseSonogram Audio2Sonogram(FileInfo fiAudio, Dictionary<string, string> configDict)
        {
            int frameLength = 512; // default value
            if (configDict.ContainsKey(Keys.FRAME_LENGTH))
                frameLength = ConfigDictionary.GetInt(Keys.FRAME_LENGTH, configDict);

            double frameOverlap = 0.0; // default value
            if (configDict.ContainsKey(Keys.FRAME_OVERLAP))
                frameOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, configDict);

            AudioRecording recordingSegment = new AudioRecording(fiAudio.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recordingSegment.FileName;
            sonoConfig.WindowSize = frameLength;
            sonoConfig.WindowOverlap = frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recordingSegment.GetWavReader());
            return sonogram;
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
            //double smoothWindow = Double.Parse(configDict[Keys.SMOOTHING_WINDOW]);   //smoothing window (seconds) before segmentation
            //double thresholdSD = Double.Parse(configDict[Keys.THRESHOLD]);           //segmentation threshold in noise SD
            //int lowFrequencyBound = Double.Int(configDict[Keys.LOW_FREQ_BOUND]);     //lower bound of the freq band to be displayed
            //int hihFrequencyBound = Double.Int(configDict[Keys.HIGH_FREQ_BOUND]);    //upper bound of the freq band to be displayed

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
            bool doNoiseReduction = false;
            if (configDict.ContainsKey(Keys.NOISE_DO_REDUCTION))
                doNoiseReduction = ConfigDictionary.GetBoolean(Keys.NOISE_DO_REDUCTION, configDict);
            if (doNoiseReduction)
            {
                //LoggedConsole.WriteLine("PERFORMING NOISE REDUCTION");
                double bgThreshold = 3.0;
                if (configDict.ContainsKey(Keys.NOISE_BG_THRESHOLD))
                    bgThreshold = ConfigDictionary.GetDouble(Keys.NOISE_BG_THRESHOLD, configDict);
                var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, bgThreshold);
                sonogram.Data = tuple.Item1;   // store data matrix
            }

            //ADD time and frequency scales
            bool addScale = false;
            if (configDict.ContainsKey(Keys.ADD_TIME_SCALE)) addScale = ConfigDictionary.GetBoolean(Keys.ADD_TIME_SCALE, configDict);
            else
            if (configDict.ContainsKey(Keys.ADD_AXES))       addScale = ConfigDictionary.GetBoolean(Keys.ADD_AXES, configDict);
            bool add1kHzLines = addScale;


            System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            Image_MultiTrack mti = new Image_MultiTrack(img);
            if (addScale) mti.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond)); //add time scale
            bool addSegmentationTrack = false;

            //add segmentation track
            if (configDict.ContainsKey(Keys.ADD_SEGMENTATION_TRACK))
                addSegmentationTrack = ConfigDictionary.GetBoolean(Keys.ADD_SEGMENTATION_TRACK, configDict);
            if (addSegmentationTrack) mti.AddTrack(Image_Track.GetSegmentationTrack(sonogram)); //add segmentation track
            return mti;
        }//Sonogram2MultiTrackImage()


        public static Image Sonogram2Image(BaseSonogram sonogram, Dictionary<string, string> configDict, 
                                           double[,] hits, List<Plot> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Image_MultiTrack image = Sonogram2MultiTrackImage(sonogram, configDict);

            if (scores != null)
            {
                foreach (Plot plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }

            if (hits != null) 
                image.OverlayRainbowTransparency(hits);

            if (predictedEvents.Count > 0) 
                image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);

            return image.GetImage();
        } //Sonogram2Image()


        public static Image Matrix2SonogramImage(double[,] matrix, SonogramConfig config)
        {
            bool doHighlightSubband = false;
            //ADD time and frequency scales
            bool addScale = false;
            //if (configDict.ContainsKey(Keys.ADD_TIME_SCALE)) addScale = ConfigDictionary.GetBoolean(Keys.ADD_TIME_SCALE, configDict);
            //else
            //    if (configDict.ContainsKey(Keys.ADD_AXES)) addScale = ConfigDictionary.GetBoolean(Keys.ADD_AXES, configDict);
            bool add1kHzLines = addScale;

            BaseSonogram sonogram = new SpectralSonogram(config, matrix);
            sonogram.Configuration.MaxFreqBand = 8800;
            System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            Image_MultiTrack image = new Image_MultiTrack(img);
            //if (addScale) image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond)); //add time scale
            return image.GetImage();
        } //CSV2SonogramImage()



        public static int MakeSonogramWithSox(FileInfo fiAudio, Dictionary<string, string> configDict, FileInfo output)
        {
            //string sourceMimeType = "wav";
            //string outputMimeType = "png";
            //Acoustics.Tools.SpectrogramRequest request = new Acoustics.Tools.SpectrogramRequest();
            
            string soxPath = @"C:\SensorNetworks\Software\Extra Assemblies\sox\sox.exe";
            if (configDict.ContainsKey(Keys.SOX_PATH))
            {
                soxPath = configDict[Keys.SOX_PATH];
                var fiSOX = new FileInfo(soxPath);
                if (!fiSOX.Exists)
                {
                    LoggedConsole.WriteLine("SOX ERROR: Path does not exist: <{0}>", fiSOX.FullName);
                    return 1;
                }
            }
            string soxCmd = "" + soxPath + ""; //must quote the path because has a space in it.

            string title = "";
            if (configDict.ContainsKey(Keys.SONOGRAM_TITLE))
            {
                title = " -t " + configDict[Keys.SONOGRAM_TITLE];
            }
            string comment = "";
            if (configDict.ContainsKey(Keys.SONOGRAM_COMMENT))
            {
                comment = " -c " + configDict[Keys.SONOGRAM_COMMENT];
            }
            string axes = "";
            if (configDict.ContainsKey(Keys.ADD_AXES) && (! ConfigDictionary.GetBoolean(Keys.ADD_AXES, configDict)))
            {
                axes = " -r ";
            }
            string coloured = " -m "; //default
            if (configDict.ContainsKey(Keys.SONOGRAM_COLOURED) && (ConfigDictionary.GetBoolean(Keys.SONOGRAM_COLOURED, configDict)))
            {
                coloured = "";
            }
            string quantisation = " -q 64 "; //default
            if (configDict.ContainsKey(Keys.SONOGRAM_QUANTISATION))
            {
                quantisation = " -q " + ConfigDictionary.GetInt(Keys.SONOGRAM_QUANTISATION, configDict);
            }

            //          Path\sox.exe  -V "sourcefile.wav" -n rate 22050 spectrogram -m -r -l -a -q 249 -w hann -y 257 -X 43.06640625 -z 100 -o "imagefile.png"
            //string soxCommandLineArguments = " -V \"{0}\" -n rate 22050 spectrogram -m -r -l -a -q 249 -w hann -y 257 -X 43.06640625 -z 100 -o \"{1}\"";  //greyscale only
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -m -l -o \"{1}\"";  //greyscale with time, freq and intensity scales
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -m -o \"{1}\"";     //reverse image greyscale with time, freq and intensity scales
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -l -o \"{1}\"";     //colour with time, freq and intensity scales
            //string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -m -q 64  -l -o \"{1}\"";    //64 grey scale, with time, freq and intensity scales
            string soxCommandLineArguments = " -V \"{0}\" -n spectrogram -l {1} {2} {3} {4} {5} -o \"{6}\"";    //64 grey scale, with time, freq and intensity scales

            //FOR COMMAND LINE OPTIONS SEE:  http://sox.sourceforge.net/sox.html
            //−a     Suppress display of axis lines. This is sometimes useful in helping to discern artefacts at the spectrogram edges.
            //-l     Print firendly monochrome spectrogram.
            //−m     Creates a monochrome spectrogram (the default is colour).
            //-q     Number of intensity quanitisation levels/colors - try -q 64
            //−r     Raw spectrogram: suppress the display of axes and legends.
            //−t text  Set the image title - text to display above the spectrogram.
            //−c text  Set (or clear) the image comment - text to display below and to the left of the spectrogram.
            //trim 20 30  displays spectrogram of 30 seconds duratoin starting at 20 seconds.

            var args = string.Format(soxCommandLineArguments, fiAudio.FullName, title, comment, axes, coloured, quantisation, output.FullName);
            var process = new TowseyLib.ProcessRunner(soxCmd);
            process.Run(args, output.DirectoryName);
            return 0;
        } //MakeSonogramWithSox



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




    } //class
}
