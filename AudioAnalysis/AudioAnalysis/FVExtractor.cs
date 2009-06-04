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

        public static void ExtractFVsFromRecording(AudioRecording ar, FVConfig FVParams, CepstralSonogramConfig sonoConfig)
        {
            Log.WriteIfVerbose("START method FVExtractor.ExtractFVsFromRecording()");
            WavReader wav = ar.GetWavReader();
            var sonogram = new CepstralSonogram(sonoConfig, wav);
            //transfer parameters to where required
            sonoConfig.Duration = sonogram.Duration;
            sonoConfig.FftConfig.SampleRate = wav.SampleRate;

            //prepare the feature vectors
            FVParams.FVArray = GetFeatureVectorsFromFrames(sonogram, FVParams, sonoConfig);

            Log.WriteIfVerbose("END method FVExtractor.ExtractFVsFromRecording()");
        } // end ExtractFVsFromSonogram()


        private static FeatureVector[] GetFeatureVectorsFromFrames(BaseSonogram sonogram, FVConfig FVParams, CepstralSonogramConfig sonoConfig)
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
        public static void ExtractFVsFromVocalisations(FileInfo[] files, FVConfig FVParams, CepstralSonogramConfig cepstralConfig)
        {
            Log.WriteIfVerbose("START FVExtractor.ExtractFVsFromVocalisations()");
            Log.WriteIfVerbose("\tNumber of vocalisations = " + files.Length);
            int interval = FVParams.ExtractionInterval; //extract feature vectors at this interval

            double avFrameCount = 0.0; //to determine average number of frames in training vocalisations
            List<FeatureVector> list = new List<FeatureVector>();
            foreach (FileInfo f in files)
            {
                //Make sonogram of each recording
                //Console.WriteLine("Recording = "+f.Name);
                AudioRecording recording = new AudioRecording(f.FullName);
                WavReader wav = recording.GetWavReader();
                cepstralConfig.SourceFName = Path.GetFileNameWithoutExtension(f.Name);
                cepstralConfig.FftConfig.SampleRate = wav.SampleRate;

               //Extract the FVs from each
                //Log.WriteIfVerbose("\tInit CepstralSonogram(cepstralConfig, wav)"); 
                var cs = new CepstralSonogram(cepstralConfig, wav);
                avFrameCount += cs.FrameCount;
                List<FeatureVector> fvs = GetFeatureVectorsAtFixedIntervals(cs, FVParams, cepstralConfig);
                //Log.WriteIfVerbose("Have extracted " + fvs.Count+ " feature vectors.");
                list.AddRange(fvs);

                //use following lines to save image of sonogram for debug purposes
                //cepstralConfig.SaveSonogramImage = true;
                //cepstralConfig.ImageDir = files[0].DirectoryName;
                //var ss      = new SpectralSonogram(cepstralConfig, wav);
                //var image   = new Image_MultiTrack(ss.GetImage(false, false));
                //string path = ((CepstralSonogramConfig)ss.Configuration).ImageDir + "\\" + ss.Configuration.SourceFName + ".png";
                //image.Save(path);
                //Console.WriteLine("Saved sonogram image to " + path);
                //Console.WriteLine("");
            } //end of all training vocalisations

            avFrameCount /= files.Count();
            Log.WriteIfVerbose("\nHave extracted " + list.Count + " feature vectors from " + files.Count() + " files.");
            Log.WriteIfVerbose("\t\tat intervals of " + interval+" frames.");
            Log.WriteIfVerbose("\tAverage frame count = " + avFrameCount+" per recording or file.");

            //Calculate the number of centroids. This will become the number of MM states.
            if (FVParams.FVCount == 0) FVParams.FVCount = (int)(avFrameCount / interval);
            Log.WriteIfVerbose("\tNumber of FVs centroids (size of VQ codebook)= " + FVParams.FVCount);
            // Console.ReadLine();

            //Apply VECTOR QUANTISATION to get the feature vector centroids
            Log.WriteIfVerbose("\nSTART VECTOR QUANTISATION");
            Cluster cluster = new Cluster(FeatureVector.GetVectors(list));
            VQ vq = new VQ(cluster, FVParams.FVCount);
            vq.Train();

            //use centroids as feature vectors
            //FVParams.FVArray = vq.GetCentroids();
            int fvCount = FVParams.FVCount;
            FVParams.FVArray = new FeatureVector[fvCount];
            for (int i = 0; i < fvCount; i++)
            {
                Log.WriteIfVerbose("   Min error centroid[" + (i + 1) + "] is average of " + vq.Clusters[i].Size + " vectors.");
                string fvName = "centroid"+(i+1);
                //Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] Name=" + fvName);
                FVParams.FVArray[i] = new FeatureVector(vq.MinErrorCentroids[i], fvName);
                FVParams.FVArray[i].SourceFile = "Derived from automated VQ"; 
            }

            FVParams.FVLength = FVParams.FVArray[0].FvLength;
            
            Log.WriteIfVerbose("END method FVExtractor.ExtractFVsFromVocalisations()");
            //Console.ReadLine();
        } // end ExtractFVsFromVocalisations()


        public static void ExtractSymbolSequencesFromVocalisations(FileInfo[] files, Template_CCAuto template)
        {
            Log.WriteIfVerbose("\nFVExtractor.ExtractSymbolSequencesFromVocalisations(): EXTRACTING SYMBOL STRINGS FROM TRAINING VOCALISATIONS");
            int prevVerbosity = Log.Verbosity;
            Log.Verbosity = 0; //suppress output
            CepstralSonogramConfig cepstralConfig = template.SonogramConfig;

            List<String> symbolSequences = new List<String>();
            int id = 0;
            foreach (FileInfo f in files)
            {
                id++;
                //Make sonogram of each recording
                AudioRecording recording = new AudioRecording(f.FullName);
                WavReader wav = recording.GetWavReader();
                cepstralConfig.SourceFName = Path.GetFileNameWithoutExtension(f.Name);
                cepstralConfig.FftConfig.SampleRate = wav.SampleRate;
                var avSonogram = new AcousticVectorsSonogram(cepstralConfig, wav);
                template.AcousticModel.GenerateSymbolSequence(avSonogram, template);
                string sylseq = Acoustic_Model.MassageSyllableSequence(template.AcousticModel.SyllSymbols);
                symbolSequences.Add(sylseq);
                //display output with file name;
                string opstr = "Seq(" + id + ") =";
                opstr = opstr.PadRight(11) + sylseq;
                opstr = opstr.PadRight(50) + " (" + f.Name + ")";
                Log.WriteLine(opstr);

            } //end of all training vocalisations
            template.WordExamples = symbolSequences.ToArray();

            Log.Verbosity = prevVerbosity;
            Log.WriteIfVerbose("END method FVExtractor.ExtractSymbolSequencesFromVocalisations()");
            //Console.ReadLine();
        } // end ExtractSymbolSequencesFromVocalisations()


        private static List<FeatureVector> GetFeatureVectorsAtFixedIntervals(CepstralSonogram sonogram, FVConfig FVParams, CepstralSonogramConfig sonoConfig)
        {
            int fvCount = sonogram.FrameCount;
            int interval = FVParams.ExtractionInterval - 1;//reduce one because will be added during loop

            //initialise feature vectors for template. Each frame provides one vector in three parts
            int dT = sonoConfig.DeltaT;
            double[,] M = sonogram.Data;

            List<FeatureVector> list = new List<FeatureVector>();
            for (int i = dT; i < fvCount - dT; i++)
            {
                //Console.WriteLine("frame "+i+" dB = "+sonogram.DecibelsPerFrame[i]);
                if (sonogram.DecibelsPerFrame[i] < 7.5) continue; //ignore low dB frames
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, i, dT); //combines  frames T-dT, T and T+dT
                FeatureVector fv = new FeatureVector(acousticV, sonogram.Configuration.SourceFName);
                fv.SetFrameIndex(i);
                list.Add(fv);
                i += interval;
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




    } //end class FVExtractor


}
