using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

using AnalysisBase;

using TowseyLib;

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
                Image image = MakeSonogram(fiAudio, fiConfig);
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
        /// <param name="args"></param>
        public static void GetSonogramFromAudioFile(string[] args)
        {
            FileInfo fiAudio  = new FileInfo(args[0]);
            FileInfo fiConfig = new FileInfo(args[1]);
            FileInfo fiImage  = new FileInfo(args[2]);
            using (Image image = MakeSonogram(fiAudio, fiConfig))
            {
                if (image != null)
                {
                    if (fiImage.Exists) fiImage.Delete();
                    image.Save(fiImage.FullName, ImageFormat.Png);
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fiAudio"></param>
        /// <param name="fiConfig"></param>
        /// <param name="fiImage"></param>
        /// <returns></returns>
        public static Image MakeSonogram(FileInfo fiAudio, FileInfo fiConfig/*, FileInfo fiImage*/)
        {
            var config = new ConfigDictionary(fiConfig.FullName);
            //Dictionary<string, string> configDict = configuration.GetTable();
            bool doNoiseReduction = config.GetBoolean(Keys.NOISE_DO_REDUCTION);
            double bgThreshold = config.GetDouble(Keys.NOISE_BG_REDUCTION);


            AudioRecording recordingSegment = new AudioRecording(fiAudio.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recordingSegment.FileName;
            sonoConfig.WindowSize = 1024;
            sonoConfig.WindowOverlap = 0.0;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recordingSegment.GetWavReader());


            // (iii) NOISE REDUCTION
            if (doNoiseReduction)
            {
                Console.WriteLine("PERFORMING NOISE REDUCTION");
                var tuple = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, bgThreshold);
                sonogram.Data = tuple.Item1;   // store data matrix
            }

            //prepare the image
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            Image_MultiTrack mti = new Image_MultiTrack(img);
            mti.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond)); //add time scale
            var image = mti.GetImage();

            return image;
        }//MakeSonogram()


    }
}
