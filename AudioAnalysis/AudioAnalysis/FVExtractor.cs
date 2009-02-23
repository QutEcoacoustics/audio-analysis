using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioTools;

namespace AudioAnalysis
{


    static class FVExtractor
    {

        /// <summary>
        /// LOGIC FOR EXTRACTION OF FEATURE VECTORS FROM SONOGRAM ****************************************************************
        /// </summary>
        public static void ExtractFVsFromSonogram(CepstralSonogram sonogram, FVConfig FVParams, AVSonogramConfig sonoConfig)
        {
            Log.WriteIfVerbose("START method FVExtractor.ExtractFVsFromSonogram()");
            //transfer parameters to where required
            sonoConfig.SampleRate = sonogram.SampleRate;
            sonoConfig.Duration = sonogram.Duration;
            FftConfiguration.SetSampleRate(sonoConfig.SampleRate);

            //prepare the feature vectors
            FeatureVector[] featureVectors;
            switch (FVParams.FVSourceType)
            {
                case FV_Source.SELECTED_FRAMES:
                    featureVectors = GetFeatureVectorsFromFrames(sonogram, FVParams, sonoConfig);
                    break;
                case FV_Source.MARQUEE:
                    switch (FVParams.FVMarqueeType)
                    {
                        case FV_MarqueeType.AT_ENERGY_PEAKS:
                            featureVectors = GetFeatureVectorsFromMarquee(sonogram, FVParams, sonoConfig);
                            break;
                        case FV_MarqueeType.AT_FIXED_INTERVALS:
                            featureVectors = GetFeatureVectorsFromMarquee(sonogram, FVParams, sonoConfig);
                            break;
                        default:
                            throw new InvalidCastException("ExtractTemplateFromSonogram(: WARNING!! INVALID FV EXTRACTION OPTION!)");
                    }
                    break;
                default:
                    throw new InvalidOperationException("ExtractTemplateFromSonogram(: WARNING!! INVALID FV SOURCE OPTION!)");
            }

            FVParams.FVArray = featureVectors;

            if (FVParams.FVSourceType == FV_Source.MARQUEE)
            {
                int count = featureVectors.Length;
                FVParams.FVCount = count;
                FVParams.FVArray = featureVectors;
                FVParams.FVLength = featureVectors[0].FvLength; //assume all same length

                FVParams.FVfNames = new string[count];
                for (int n = 0; n < count; n++) FVParams.FVfNames[n] = "template" + FVParams.CallID + "_FV"+(n+1)+".txt";
                FVParams.FVSourceFiles = new string[count];
                for (int n = 0; n < count; n++) FVParams.FVSourceFiles[n] = featureVectors[n].SourceFile;

            }
            Log.WriteIfVerbose("END method FVExtractor.ExtractFVsFromSonogram()");
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
                fvs[i] = ExtractFeatureVectorsFromSelectedFramesAndAverage(M, frameIDs, dT);
                fvs[i].VectorFName = fvName;
                fvs[i].FrameIndices = frameIDs;
                fvs[i].SourceFile = sonoConfig.SourceFName;
            }

            FVParams.FVLength = fvs[0].FvLength;
            return fvs;
        }

        private static FeatureVector[] GetFeatureVectorsFromMarquee(BaseSonogram sonogram, FVConfig FVParams, AVSonogramConfig sonoConfig)
        {
            int start = FVParams.MarqueeStart;
            int end = FVParams.MarqueeEnd;
            int marqueeFrames = end - start + 1;
            var frameDuration = sonoConfig.GetFrameDuration(sonogram.SampleRate);
            double marqueeDuration = marqueeFrames * frameDuration;
            Log.WriteIfVerbose("\tMarquee start=" + start + ",  End=" + end + ",  Duration= " + marqueeFrames + "frames =" + marqueeDuration.ToString("F2") + "s");
            int[] frameIndices = null;


            switch (FVParams.FVMarqueeType)
            {
                case FV_MarqueeType.AT_FIXED_INTERVALS:
                    int interval = (int)(FVParams.MarqueeInterval / frameDuration / (double)1000);
                    Log.WriteIfVerbose("\tFrame interval=" + interval + "ms");
                    frameIndices = FeatureVector.GetFrameIndices(start, end, interval);
                    break;
                case FV_MarqueeType.AT_ENERGY_PEAKS:
                    double[] frameEnergy = sonogram.Decibels;
                    double energyThreshold = EndpointDetectionConfiguration.SegmentationThresholdK1;
                    frameIndices = FeatureVector.GetFrameIndices(start, end, frameEnergy, energyThreshold);
                    Log.WriteIfVerbose("\tEnergy threshold=" + energyThreshold.ToString("F2"));
                    break;
                default:
                    Log.WriteLine("Template.GetFeatureVectorsFromMarquee():- WARNING!!! INVALID FEATURE VECTOR EXTRACTION OPTION");
                    break;
            }

            string indices = DataTools.writeArray2String(frameIndices);
            Log.WriteIfVerbose("\tExtracted frame indices are:-" + indices);

            //initialise feature vectors for template. Each frame provides one vector in three parts
            //int coeffcount = M.GetLength(1);  //number of MFCC deltas etcs
            //int featureCount = coeffcount * 3;
            int indicesL = frameIndices.Length;
            int dT = sonoConfig.DeltaT;
            double[,] M = sonogram.Data;

            FeatureVector[] fvs = new FeatureVector[indicesL];
            for (int i = 0; i < indicesL; i++)
            {
                Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + frameIndices[i]);
                //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
                double[] acousticV = Speech.GetAcousticVector(M, frameIndices[i], dT); //combines  frames T-dT, T and T+dT
                fvs[i] = new FeatureVector(acousticV);
                // Wav source may not be from a file
                //fvs[i].SourceFile = TemplateState.WavFilePath; //assume all FVs have same source file
                fvs[i].SetFrameIndex(frameIndices[i]);
            }
            return fvs;
        }

        private static FeatureVector ExtractFeatureVectorsFromSelectedFramesAndAverage(double[,] M, string frames, int dT)
        {
            //initialise feature vectors for template. Each frame provides one vector
            string[] frameIDs = frames.Split(',');
            int count = frameIDs.Length;

            FeatureVector[] fvs = new FeatureVector[count];
            for (int i = 0; i < count; i++)
            {
                int frameID = Int32.Parse(frameIDs[i]);
                Log.WriteIfVerbose("   Init FeatureVector[" + (i + 1) + "] from frame " + frameID);
                fvs[i] = ExtractFeatureVectorFromOneFrame(M, frameID, dT);
            }
            return FeatureVector.AverageFeatureVectors(fvs, 1);
        }



        private static FeatureVector ExtractFeatureVectorFromOneFrame(double[,]M, int frameNumber, int dT)
        {
            //init vector. Each one contains three acoustic vectors - for T-dT, T and T+dT
            double[] acousticV = Speech.GetAcousticVector(M, frameNumber, dT); //combines  frames T-dT, T and T+dT
            var fv = new FeatureVector(acousticV);
            return fv;
        }



    } //end class FVExtractor


}
