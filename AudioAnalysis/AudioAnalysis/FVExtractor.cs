using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using NeuralNets;
using AudioTools;

namespace AudioAnalysis
{


    static class FVExtractor
    {

        public static void ExtractFVsFromRecording(AudioRecording ar, FVConfig FVParams, AVSonogramConfig sonoConfig)
        {
            Log.WriteIfVerbose("START method FVExtractor.ExtractFVsFromRecording()");
            WavReader wav = ar.GetWavReader();
            var sonogram = new CepstralSonogram(sonoConfig, wav);
            //transfer parameters to where required
            sonoConfig.SampleRate = sonogram.SampleRate;
            sonoConfig.Duration = sonogram.Duration;
            FftConfiguration.SetSampleRate(sonoConfig.SampleRate);

            //prepare the feature vectors
            FVParams.FVArray = GetFeatureVectorsFromFrames(sonogram, FVParams, sonoConfig);

            Log.WriteIfVerbose("END method FVExtractor.ExtractFVsFromRecording()");
        } // end ExtractFVsFromSonogram()


        private static FeatureVector[] GetFeatureVectorsFromFrames(BaseSonogram sonogram, FVConfig FVParams, AVSonogramConfig sonoConfig)
        {
            Log.WriteIfVerbose("\nEXTRACTING FEATURE VECTORS FROM FRAMES:- method FVExtractor.GetFeatureVectorsFromFrames()");

            int fvCount = FVParams.FVCount;
            int dT = sonoConfig.DeltaT;
            double[,] M = sonogram.Data;
            FeatureVector[] fvs = new FeatureVector[fvCount];

            for (int i = 0; i < fvCount; i++)
            {
                string[] data = FVParams.FVIniData[i].Split('\t');//returns two strings neede to init FV
                string fvName   = data[0];
                string frameIDs = data[1];
                Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] Name=" + fvName + " from frames " + frameIDs);
                fvs[i] = ExtractFeatureVectorsFromSelectedFramesAndAverage(M, frameIDs, dT, fvName);
                fvs[i].name = fvName;
                fvs[i].FrameIndices = frameIDs;
                fvs[i].SourceFile = sonoConfig.SourceFName;
            }

            FVParams.FVLength = fvs[0].FvLength;
            return fvs;
        }

        /// <summary>
        /// called from FVExtractor.GetFeatureVectorsFromFrames(BaseSonogram sonogram, FVConfig FVParams, AVSonogramConfig sonoConfig)
        /// </summary>
        /// <param name="M"></param>
        /// <param name="frames"></param>
        /// <param name="dT"></param>
        /// <param name="fvName"></param>
        /// <returns></returns>
        private static FeatureVector ExtractFeatureVectorsFromSelectedFramesAndAverage(double[,] M, string frames, int dT, string fvName)
        {
            //initialise feature vectors for template. Each frame provides one vector
            string[] frameIDs = frames.Split(',');
            int count = frameIDs.Length;

            FeatureVector[] fvs = new FeatureVector[count];
            for (int i = 0; i < count; i++)
            {
                int frameID = Int32.Parse(frameIDs[i]);
                Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + frameID);
                fvs[i] = ExtractFeatureVectorFromOneFrame(M, frameID, dT, fvName);
            }
            return FeatureVector.AverageFeatureVectors(fvs, 1);
        }


        /// <summary>
        /// This method is called when constructing a CC AUTO template.
        /// </summary>
        /// <param name="ar"></param>
        /// <param name="FVParams"></param>
        /// <param name="sonoConfig"></param>
        public static void ExtractFVsFromVocalisations(AudioRecording ar, FVConfig FVParams, AVSonogramConfig sonoConfig)
        {
            Log.WriteIfVerbose("START FVExtractor.ExtractFVsFromVocalisations()");

            WavReader wav = ar.GetWavReader();
            var sonogram = new CepstralSonogram(sonoConfig, wav);
            //transfer parameters to where required
            sonoConfig.SampleRate = sonogram.SampleRate;
            sonoConfig.Duration = sonogram.Duration;
            FftConfiguration.SetSampleRate(sonoConfig.SampleRate);

            //Get List of Vocalisation Recordings
            string ext = ".wav";
            string samplesDir = FVParams.FVSourceDir;
            FileInfo[] files = FileTools.GetFilesInDirectory(samplesDir, ext);
            List<FeatureVector> list = new List<FeatureVector>(); 
            foreach (FileInfo f in files)
            {
                //Make sonogram of each recording
                Console.WriteLine("Recording = "+f.Name);
                AudioRecording recording = new AudioRecording(f.FullName);
                WavReader wr = recording.GetWavReader();
                var ss = new SpectralSonogram(sonoConfig, wr);
                var image = new Image_MultiTrack(ss.GetImage(false, false));
                string path = samplesDir + Path.GetFileNameWithoutExtension(f.Name)+ ".png";
                image.Save(path);

               //Extract the FVs from each
                var cs = new CepstralSonogram(sonoConfig, wr);
                List<FeatureVector> fvs = GetFeatureVectorsAtFixedIntervals(cs, FVParams, sonoConfig);
                Log.WriteIfVerbose("Have extracted " + fvs.Count+ " feature vectors.");
                list.AddRange(fvs);
            }

            //Cluster the FVs
            Log.WriteIfVerbose("Have extracted "+list.Count+" feature vectors.");

            //Get the centroids
            Cluster cluster = new Cluster(FeatureVector.GetVectors(list));
            VQ vq = new VQ(cluster, FVParams.FVCount);
            vq.Train();

            //use centroids as feature vectors
            //FVParams.FVArray = vq.GetCentroids();
            int fvCount = FVParams.FVCount;
            FVParams.FVArray = new FeatureVector[fvCount];
            for (int i = 0; i < fvCount; i++)
            {
                string fvName = "centroid"+(i+1);
                Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] Name=" + fvName);
                FVParams.FVArray[i] = new FeatureVector(vq.MinErrorCentroids[i], fvName);
            }

            FVParams.FVLength = FVParams.FVArray[0].FvLength;
            Log.WriteIfVerbose("END method FVExtractor.ExtractFVsFromVocalisations()");
            //Console.ReadLine();
        } // end ExtractFVsFromSonogram()


        private static List<FeatureVector> GetFeatureVectorsAtFixedIntervals(CepstralSonogram sonogram, FVConfig FVParams, AVSonogramConfig sonoConfig)
        {
            int interval = 3; //extract feature vector at this interval
            int fvCount = sonogram.FrameCount / interval;

            //initialise feature vectors for template. Each frame provides one vector in three parts
            int dT = sonoConfig.DeltaT;
            double[,] M = sonogram.Data;

            List<FeatureVector> list = new List<FeatureVector>();
            for (int i = dT; i < fvCount - dT; i++)
            {
                int id = i * interval;
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, id, dT); //combines  frames T-dT, T and T+dT
                FeatureVector fv = new FeatureVector(acousticV, sonogram.Configuration.SourceFName);
                fv.SetFrameIndex(id);
                list.Add(fv);
            }
            return list;
        }




        private static FeatureVector ExtractFeatureVectorFromOneFrame(double[,] M, int frameNumber, int dT, string fvName)
        {
            //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
            double[] acousticV = Speech.GetAcousticVector(M, frameNumber, dT); //combines  frames T-dT, T and T+dT
            var fv = new FeatureVector(acousticV, fvName);
            return fv;
        }

        /// <summary>
        /// This method is called when constructing a DCT_2D template.
        /// </summary>
        /// <param name="ar"></param>
        /// <param name="FVParams"></param>
        /// <param name="sonoConfig"></param>
        public static void ExtractFVsFromMarquee(AudioRecording ar, FVConfig FVParams, AVSonogramConfig sonoConfig)
        {
            if(Log.Verbosity==1) Console.WriteLine("START FVExtractor.ExtractFVsFromMarquee()");
            Log.WriteIfVerbose("Start time = " + FVParams.StartTime.ToString("F3") + " seconds from start of recording");
            Log.WriteIfVerbose("End   time = " + FVParams.EndTime.ToString("F3")   + " seconds from start of recording");
            FVParams.FVArray = new FeatureVector[1];
            FVParams.FVCount = 1;

            //extract marquee using start, end values
            Log.WriteIfVerbose("");


            //pad to fixed number of frames

            //do the DCT
 
            //store as single FV



            Console.WriteLine("End of the Line");
            Console.ReadLine();
        }

    } //end class FVExtractor


}
