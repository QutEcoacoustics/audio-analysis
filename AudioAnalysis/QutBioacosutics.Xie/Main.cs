using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.WavTools;
using AudioAnalysisTools.DSP;
using System.IO;
using MathNet.Numerics;
using QutBioacosutics.Xie.FrogIndices;
using QutBioacosutics.Xie.LDSpectrograms;



namespace QutBioacosutics.Xie
{
    using AudioAnalysisTools;
    using log4net;
    using TowseyLibrary;
    using System.Drawing;
    using System.Drawing.Imaging;
    using QutBioacosutics.Xie.Configuration;
    using LDSpectrograms;
    using AudioAnalysisTools.LongDurationSpectrograms;

    public static class Main
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Main));

        public static void Entry(dynamic configuration, FileInfo source)
        {

            Log.Info("Enter into Jie's personal workspace");

            // Frog species:Mixophyes fasciolatus, Litoria caerulea, Litoria fallax, Litoria gracilenta, Litoria nasuta, 
            // Litoria verreauxii, Litoria rothii, Litoria latopalmata, Cane_Toad.
            // Calculate the oscillation rate for 9 frog species.
            // Parameters for different frog species: 1. Frequency Band, 2. Dct duration, 3.Minimum OscFreq, 4. Maximum OscFreq, 5. Min amplitude, 6. Min duration, 7. Max duration.
            // Step.1: divide the frequency band into several bands for 9 frog species properly
            // Step.2: for each frequency band, If there is only one frog species,just find the maximum to make tracks. 
            // otherwise find the local maximum to make tracks
            // Step.3: According to tracks, calculate oscillation rate in different frequency bands.

            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */

            //DrawLDSpectrogram.Execute(null);
            //Console.WriteLine("FINISHED!!");
            //Console.ReadLine();

            //Parameters setting

            //********************Canetoad********************************//
            CanetoadConfiguration canetoadConfig = new CanetoadConfiguration(configuration);

            //********************Gracillenta********************************//

            GracillentaConfiguration gracillentaConfig = new GracillentaConfiguration(configuration);

            //********************Nasuta********************************//

            NasutaConfiguration nasutaConfig = new NasutaConfiguration(configuration);

            //********************Caerulea********************************//

            CaeruleaConfiguration caeruleaConfig = new CaeruleaConfiguration(configuration);

            //********************Fallax********************************//

            FallaxConfiguration fallaxConfig = new FallaxConfiguration(configuration);

            //********************Latopalmata********************************//

            LatopalmataConfiguration latopalmataConfig = new LatopalmataConfiguration(configuration);

            //****************************************************************//
            // Path for saving images---Used for testing temporary result
            string saveImagePath = configuration.SaveImagePath;

            // SpectrogramConfiguration for oscillation
            
            int windowSize = configuration.WindowSize;

            //****************************************************************//
            // Path of loaded recording

            //string path = configuration.LoadedFilePath;
                      
            var fileEntries = Directory.GetFiles("C:\\Jie\\data\\Segment_JCU_01");
            var fileCount = fileEntries.Count();

            // Canetoad
            //var canetoadTrack = new double[257, 726];
            //var canetoadEnergy = new double[257, 726];
            //var canetoadOscillation = new double[257, 726];

            // Gracillenta
            //var gracillentaTrack = new double[257, 726];
            //var gracillentaEnergy = new double[257, 726];

            // Nasuta
            var nasutaTrack = new double[257, 726];
            var nasutaHarmonic = new double[257, 726];
            var nasutaOscillation = new double[257, 726];

            // Caerulea
            var caeruleaTrack = new double[257, 726];
            var caeruleaEnergy = new double[257, 726];
            var caeruleaOscillation = new double[257,726];

            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                Log.Info(fileIndex);

                string path = Path.GetFileName(fileEntries[fileIndex]);

                var indexOfRecording = Path.GetFileNameWithoutExtension(path);

                int indexNumber = Convert.ToInt32(indexOfRecording.Split('_', 'm')[1]);

                //var fullPath = Path.Combine("C:\\Jie\\data\\Segment_JCU_01", path);

                string fullPath = @"C:\Jie\data\Segment_JCU_01\020313_393min.wav";
                var recording = new AudioRecording(fullPath);

                // Step.1 Generate spectrogarm
                // A. Generate spectrogram for extracting tracks, entropy and harmonic

                var spectrogramLongConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = windowSize };
                var spectrogramLong = new SpectrogramStandard(spectrogramLongConfig, recording.GetWavReader());

                // B. Generate spectrogram for extracting oscillation rate

                //*************************************************************//

                // Calculate windowOverlap
                //double windowOverlap = XieFunction.CalculateRequiredWindowOverlap(recording.SampleRate, windowSize, canetoadConfig.MaximumOscillationNumberCanetoad);

                var spectrogramShortConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.NONE, WindowOverlap = 0.5, WindowSize = windowSize };
                var spectrogramShort = new SpectrogramStandard(spectrogramShortConfig, recording.GetWavReader());



                // Step.2 Produce features

                //***********************Canetoad*************************//
                //var peakHitsCanetoad = CalculateIndexForCanetoad.GetPeakHits(canetoadConfig, spectrogramLong);
                //var trackCanetaod = CalculateIndexForCanetoad.GetFrogTracks(canetoadConfig, spectrogramLong, peakHitsCanetoad);
                //var oscillationCanetaodRotate = CalculateIndexForCanetoad.GetOscillationRate(canetoadConfig, spectrogramShort);
                //var oscillationCanetoad = MatrixTools.MatrixRotate90Anticlockwise(oscillationCanetaodRotate);
                

                //***********************Gracillenta*************************//

                //var peakHitsGracillenta = CalculateIndexForLitoriaGracillenta.GetPeakHitsGracillenta(gracillentaConfig, spectrogramLong);
                //var trackGracillenta = CalculateIndexForLitoriaGracillenta.GetFrogTracksGracillenta(gracillentaConfig, spectrogramLong, peakHitsGracillenta);


                //***********************Nasuta*************************//

                var peakHitsNasuta = CalculateIndexForLitoriaNasuta.GetPeakHitsNasuta(nasutaConfig, spectrogramLong);
                var trackNasuta = CalculateIndexForLitoriaNasuta.GetFrogTracksFallax(nasutaConfig, spectrogramLong, peakHitsNasuta);
                var oscillationNasutaRotate = CalculateIndexForLitoriaNasuta.GetOscillationRate(nasutaConfig, spectrogramShort);
                var oscillationNasuta = MatrixTools.MatrixRotate90Anticlockwise(oscillationNasutaRotate);



                // Step.3 Draw spectrogram
                //Image image = XieFunction.DrawSonogram(spectrogramLong);
                //Bitmap bmp = (Bitmap)image;
                //double[,] spectrogramMatrix = DataTools.normalise(spectrogramLong.Data);
                //int rows = spectrogramMatrix.GetLength(0);
                //int cols = spectrogramMatrix.GetLength(1);
                //Color[] grayScale = ImageTools.GrayScale();
                //for (int r = 0; r < rows; r++)
                //{
                //    for (int c = 0; c < cols; c++)
                //    {
                //        if (trackCanetaod.Item4[c, r] != 0)
                //            bmp.SetPixel(r, c, Color.Blue);
                //    }
                //}
                //bmp.Save(saveImagePath);


                //***********************Caerulea*************************//

                var peakHitsCaerulea = CalculateIndexForLitoriaCaerulea.GetPeakHits(caeruleaConfig, spectrogramLong);
                var trackCaerulea = CalculateIndexForLitoriaCaerulea.GetFrogTracks(caeruleaConfig, spectrogramLong, peakHitsCaerulea);
                var oscillationCaerulea = CalculateIndexForLitoriaCaerulea.GetOscillationRate(caeruleaConfig, spectrogramShort);

                //***********************Fallax*************************//

                //var peakHitsFallax = CalculateIndexForLitoriaFallax.GetPeakHitsFallax(fallaxConfig, spectrogramLong);
                //var trackFallax = CalculateIndexForLitoriaFallax.GetFrogTracksFallax(fallaxConfig, spectrogramLong, peakHitsFallax);

                //********************Latopalmata********************************//

                //var peakHitsLatopalmata = CalculateIndexForLitoriaLatopalmata.GetPeakHits(latopalmataConfig,spectrogramLong);
                //var trackLatopalmata = CalculateIndexForLitoriaLatopalmata.GetFrogTracks(latopalmataConfig,spectrogramLong,peakHitsLatopalmata);


                // Create 21 matrix to save the features
                // Canetoad: Item1-arrayresult, Item4-energy

                //var trackFeatureCanetoad = trackCanetaod.Item1;
                //var energyFeatureCanetoad = trackCanetaod.Item3; // Need to be normalised

                //var oscillationFeatureCanetoad = new double[oscillationCanetoad.GetLength(0)];
                //for (int i = 0; i < oscillationCanetoad.GetLength(0); i++)
                //{
                //    double tempOscillation = 0;
                //    for (int j = 0; j < oscillationCanetoad.GetLength(1); j++)
                //    {
                //        tempOscillation = tempOscillation + oscillationCanetoad[i, j];
                //    }
                //    oscillationFeatureCanetoad[i] = tempOscillation;
                //}

                //for (int i = 0; i < 257; i++)
                //{
                //    canetoadTrack[i, indexNumber] = trackFeatureCanetoad[i];
                //    canetoadEnergy[i, indexNumber] = energyFeatureCanetoad[i];
                //    canetoadOscillation[i, indexNumber] = oscillationFeatureCanetoad[i];
                //}

                //// Gracillenta
                //var trackFeatureGracillenta = trackGracillenta.Item1;
                //var energyFeatureGracillenta = trackGracillenta.Item3; // Need to be normalised

                //for (int i = 0; i < 257; i++)
                //{
                //    gracillentaTrack[i, indexNumber] = trackFeatureGracillenta[i];
                //    gracillentaEnergy[i, indexNumber] = energyFeatureGracillenta[i];
                //}

                //// Nasuta
                var trackFeatureNasuta = trackNasuta.TrackHitsNasuta.Item1;
                var harmonicNasuta = trackNasuta.HarmonicHitsNasuta; // Need to be normalised

                var harmonicArrayNasuta = new double[harmonicNasuta.GetLength(0)];
                for (int i = 0; i < harmonicNasuta.GetLength(0); i++)
                {
                    double tempHarmonic = 0;
                    for (int j = 0; j < harmonicNasuta.GetLength(1); j++)
                    {
                        tempHarmonic = tempHarmonic + harmonicNasuta[i, j];
                    }

                    harmonicArrayNasuta[i] = tempHarmonic;
                }

                var oscillationFeatureNasuta = new double[oscillationNasuta.GetLength(0)];
                for (int i = 0; i < oscillationNasuta.GetLength(0); i++)
                {
                    double tempOscillation = 0;
                    for (int j = 0; j < oscillationNasuta.GetLength(1); j++)
                    {
                        tempOscillation = tempOscillation + oscillationNasuta[i, j];
                    }
                    oscillationFeatureNasuta[i] = tempOscillation;
                }

                for (int i = 0; i < 257; i++)
                {
                    nasutaTrack[i, indexNumber] = trackFeatureNasuta[i];
                    nasutaHarmonic[i, indexNumber] = harmonicArrayNasuta[i];
                    nasutaOscillation[i, indexNumber] = oscillationFeatureNasuta[i];
                }

                // Caerulea

                var trackFeatureCaerulea = trackCaerulea.Item1;
                var energyFeatureCaerulea = trackCaerulea.Item3; // Need to be normalised

                var oscillationFeatureCaerulea = new double[oscillationCaerulea.GetLength(0)];
                for (int i = 0; i < oscillationCaerulea.GetLength(0); i++)
                {
                    double tempOscillation = 0;
                    for (int j = 0; j < oscillationCaerulea.GetLength(1); j++)
                    {
                        tempOscillation = tempOscillation + oscillationCaerulea[i, j];
                    }
                    oscillationFeatureCaerulea[i] = tempOscillation;
                }

                for (int i = 0; i < 257; i++)
                {
                    caeruleaTrack[i, indexNumber] = trackFeatureCaerulea[i];
                    caeruleaEnergy[i, indexNumber] = energyFeatureCaerulea[i];
                    caeruleaOscillation[i, indexNumber] = oscillationFeatureCaerulea[i];
                }


                // Fallax

                // Latopalmata

            }
            // Write 7 frog species with 3 features to csv files  
            // Canetoad


            //FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(canetoadTrack), @"C:\Jie\output\indexCanetoad\canetoadTrack.csv");
            //FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(canetoadEnergy), @"C:\Jie\output\indexCanetoad\canetoadEnergy.csv");
            //FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(canetoadOscillation), @"C:\Jie\output\indexCanetoad\canetoadOscillation.csv");
            
            // Gracillenta
            //FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(gracillentaTrack), @"C:\Jie\output\indexGracillenta\gracillentaTrack.csv");
            //FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(gracillentaEnergy), @"C:\Jie\output\indexGracillenta\gracillentaEnergy.csv");

            // Nasuta
            FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(nasutaTrack), @"C:\Jie\output\indexNasuta\nasutaTrack.csv");
            FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(nasutaHarmonic), @"C:\Jie\output\indexNasuta\nasutaHarmonic.csv");
            FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(nasutaOscillation), @"C:\Jie\output\indexNasuta\nasutaOscillation.csv");

            // Caerulea
            FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(caeruleaTrack), @"C:\Jie\output\indexNasuta\caeruleaTrack.csv");
            FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(caeruleaEnergy), @"C:\Jie\output\indexNasuta\caeruleaHarmonic.csv");
            FileTools.WriteMatrix2File(MatrixTools.MatrixRotate90Clockwise(caeruleaOscillation), @"C:\Jie\output\indexNasuta\caeruleaOscillation.csv");

            // Fallax

            // Latopalmata

            //DrawLDSpectrogram.DrawSpectrogramsFromSpectralIndices(arguments.SpectrogramConfigPath, arguments.IndexPropertiesConfig);

            Log.Info("FINISHED!!!");

        }

    }
}
