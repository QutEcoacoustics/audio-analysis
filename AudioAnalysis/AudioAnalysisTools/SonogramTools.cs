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
using Acoustics.Shared;

namespace AudioAnalysisTools
{
    public static class SonogramTools
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
                    Console.WriteLine("\nWARNING: Could not construct annotated image because analysis name not recognized:");
                    Console.WriteLine("\t " + analyisName);
                    return null;
                }

                Image image = null;
                AnalysisSettings settings = new AnalysisSettings();
                settings.AudioFile = fiAudio;
                settings.ConfigDict = config.GetDictionary();
                settings.ConfigFile = fiConfig;
                settings.ImageFile = fiImage;
                settings.AnalysisRunDirectory = diOutputDir;
                var results = analyser.Analyse(settings);
                if (results.ImageFile == null) image = null;
                else image = Image.FromFile(results.ImageFile.FullName);
                analyser = null;
                return image;
            }
            else
            {
                analyser = null;
                Image image = MakeSonogram(fiAudio, config.GetDictionary());
                if (image != null)
                {
                    if (fiImage.Exists) fiImage.Delete();
                    image.Save(fiImage.FullName, ImageFormat.Png);
                }
                return image;
            }
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="fiAudio"></param>
        /// <param name="fiConfig"></param>
        /// <param name="fiImage"></param>
        /// <returns></returns>
        public static Image MakeSonogram(FileInfo fiAudio, Dictionary<string, string> configDict)
        {
            int frameLength = 512;
            if (configDict.ContainsKey(Keys.FRAME_LENGTH))
                frameLength = ConfigDictionary.GetInt(Keys.FRAME_LENGTH, configDict);

            double frameOverlap = 0.0;
            if (configDict.ContainsKey(Keys.FRAME_OVERLAP))
                frameOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, configDict);

            //double smoothWindow = Double.Parse(configDict[Keys.SMOOTHING_WINDOW]);   //smoothing window (seconds) before segmentation
            //double thresholdSD = Double.Parse(configDict[Keys.THRESHOLD]);           //segmentation threshold in noise SD
            //int lowFrequencyBound = Double.Int(configDict[Keys.LOW_FREQ_BOUND]);     //lower bound of the freq band to be displayed
            //int hihFrequencyBound = Double.Int(configDict[Keys.HIGH_FREQ_BOUND]);    //upper bound of the freq band to be displayed

            AudioRecording recordingSegment = new AudioRecording(fiAudio.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recordingSegment.FileName;
            sonoConfig.WindowSize = frameLength;
            sonoConfig.WindowOverlap = frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recordingSegment.GetWavReader());
            bool doHighlightSubband = false;
            bool addScale = false;
            bool add1kHzLines = false;


            //check if doing a reduced sonogram
            int timeReductionFactor = 1;
            if (configDict.ContainsKey(Keys.TIME_REDUCTION_FACTOR))
                timeReductionFactor = ConfigDictionary.GetInt(Keys.TIME_REDUCTION_FACTOR, configDict);
            int freqReductionFactor = 1;
            if (configDict.ContainsKey(Keys.FREQ_REDUCTION_FACTOR))
                freqReductionFactor = ConfigDictionary.GetInt(Keys.FREQ_REDUCTION_FACTOR, configDict);
            if (!((timeReductionFactor == 1) && (freqReductionFactor == 1)))
            {
                sonogram.Data = ReduceDimensionalityOfSpectrogram(sonogram.Data, timeReductionFactor, freqReductionFactor);               
                return sonogram.GetImage(doHighlightSubband, add1kHzLines);
            }

            
            // (iii) NOISE REDUCTION
            bool doNoiseReduction = false;
            if (configDict.ContainsKey(Keys.NOISE_DO_REDUCTION))
                doNoiseReduction = ConfigDictionary.GetBoolean(Keys.NOISE_DO_REDUCTION, configDict);
            if (doNoiseReduction)
            {
                //Console.WriteLine("PERFORMING NOISE REDUCTION");
                double bgThreshold = 3.0;
                if (configDict.ContainsKey(Keys.NOISE_BG_REDUCTION))
                    bgThreshold = ConfigDictionary.GetDouble(Keys.NOISE_BG_REDUCTION, configDict);
                var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, bgThreshold);
                sonogram.Data = tuple.Item1;   // store data matrix
            }

            //prepare the image
            //ADD time and frequency scales
            if (configDict.ContainsKey(Keys.ADD_TIME_SCALE))
                addScale = ConfigDictionary.GetBoolean(Keys.ADD_TIME_SCALE, configDict);
            if (addScale) add1kHzLines = true;



            System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            Image_MultiTrack mti = new Image_MultiTrack(img);
            if (addScale) mti.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond)); //add time scale
            bool addSegmentationTrack = false;

            //add segmentation track
            if (configDict.ContainsKey(Keys.ADD_SEGMENTATION_TRACK))
                addSegmentationTrack = ConfigDictionary.GetBoolean(Keys.ADD_SEGMENTATION_TRACK, configDict);
            if (addSegmentationTrack) mti.AddTrack(Image_Track.GetSegmentationTrack(sonogram)); //add segmentation track
            var image = mti.GetImage();

            return image;
        }//MakeSonogram()

        public static void MakeSonogramWithSox(FileInfo fiAudio, Dictionary<string, string> configDict, FileInfo output)
        {
            //string sourceMimeType = "wav";
            //string outputMimeType = "png";
            //Acoustics.Tools.SpectrogramRequest request = new Acoustics.Tools.SpectrogramRequest();
            string soxPath = @"C:\SensorNetworks\Software\Extra Assemblies\sox\sox.exe";
            //var soxExe = new FileInfo(soxPath);
            //IAudioUtility audioUtility;
            //MasterAudioUtility.Segment(
            //                            fiAudio,
            //                            fiOutputSegment,
            //                            new AudioUtilityRequest
            //                            {
            //                                SampleRate = resampleRate,
            //                                OffsetStart = TimeSpan.FromMilliseconds(startMilliseconds),
            //                                OffsetEnd = TimeSpan.FromMilliseconds(endMilliseconds),
            //                                //Channel = 2 // set channel number or mixdowntomono=true  BUT NOT BOTH!!!
            //                                //MixDownToMono  =true
            //                            });

            //Path\sox.exe  -V "sourcefile.wav" -n rate 22050 spectrogram -m -r -l -a -q 249 -w hann -y 257 -X 43.06640625 -z 100 -o "imagefile.png"
            //string soxCommandLineArguments = String.Format("-V {1}   -m -r -l -a -q 249 -w hann -y 257 -X 43.06640625 -z 100  -o {2}", soxPath, fiAudio.FullName, output.FullName);
            //string soxCommandLineArguments = String.Format("-V {0} -o {1}", fiAudio.FullName, output.FullName);
            //string soxCommandLineArguments = String.Format("-V {0}", fiAudio.FullName);
            string soxCommandLineArguments = String.Format(fiAudio.FullName);


            var fiSox = new FileInfo(soxPath);
            if (!fiSox.Exists)
            {
                Console.WriteLine("SOX ERROR");
                return;
            }
            ProcessRunner process = new ProcessRunner(soxPath);
            process.Run(soxCommandLineArguments, output.DirectoryName, false);

            //var soxUtility = new SoxSpectrogramUtility(audioUtility, soxExe);
            //soxUtility.Create(fiAudio, sourceMimeType, output, outputMimeType, request);
        }
    }
}
