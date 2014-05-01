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
            // Step.2: for each frequency band, If there is only one frog species,just find the maximum to form tracks. 
            // otherwise find the local maximum to form tracks
            // Step.3: According to tracks, calculate oscillation rate in different frequency bands.

            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */


            // Step.4 Draw false-color spectrogram
            string ipDirStr = @"C:\Jie\output\indexCanetoad";
            string opDirStr = @"C:\Jie\output\indexCanetoad";
            string fileName = "canetoad_DATE";
            var ipDir = new DirectoryInfo(ipDirStr);
            var opDir = new DirectoryInfo(opDirStr);
            int startMinute = 19 * 60;

            //..........................................................//
            //Draw False color spectrogram
            LDSpectrogramConfig spgConfig = new LDSpectrogramConfig(fileName, ipDir, opDir);
            //spgConfig.ColourMap = "TRC-OSC-HAR";
            spgConfig.ColourMap1 = "OSC-HAR-TRC";
            spgConfig.MinuteOffset = startMinute;
            spgConfig.FrameWidth = 256;
            //spgConfig.SampleRate = 17640;
            spgConfig.SampleRate = 22050;
            FileInfo outPath = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
            spgConfig.WritConfigToYAML(outPath);
            DrawLDSpectrogram.DrawFalseColourSpectrograms(spgConfig);



            //***************************************************************//
            //Parameters setting
            //***************************Canetoad*****************************//  
            // Peak parameters
            double amplitudeThresholdCanetoad = configuration.AmplitudeThresholdCanetoad;   // Decibel---the minimum amplitude value
            //int rangeCanetoad = configuration.RangeCanetoad;                                // Frame---the distance in either side for selecting peaks
            //int distanceCanetoad = configuration.DistanceCanetoad;                          // Frame---remove near peaks
            // Track parameters
            double binToreanceCanetoad = configuration.BinToreanceCanetoad;                 // Bin---the fluctuation of the dominant frequency bin 
            int frameThresholdCanetoad = configuration.FrameThresholdCanetoad;              // Frame---frame numbers of the silence    
            int trackDurationThresholdCanetoad = configuration.TrackDurationThresholdCanetoad;      
            double trackThresholdCanetoad = configuration.TrackThresholdCanetoad;           // Used for calculating the percent of peaks in one track    
            int maximumTrackDurationCanetoad = configuration.MaximumTrackDurationCanetoad;  // Minimum duration of tracks
            int minimumTrackDurationCanetoad = configuration.MinimumTrackDurationCanetoad;  // Maximum duration of tracks   
            double binDifferenceCanetoad = configuration.BinDifferenceCanetoad;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            int frequencyLowCanetoad = configuration.FrequencyLowCanetoad;
            int frequencyHighCanetoad = configuration.FrequencyHighCanetoad;
            // DCT
            int minimumOscillationNumberCanetoad = configuration.minimumOscillationNumberCanetoad;
            int maximumOscillationNumberCanetoad = configuration.maximumOscillationNumberCanetoad;
            int minimumFrequencyCanetoad = configuration.MinimumFrequencyCanetoad;
            int maximumFrequencyCanetoad = configuration.MaximumFrequencyCanetoad;
            double dct_DurationCanetoad = configuration.Dct_DurationCanetoad;
            double dct_ThresholdCanetoad = configuration.Dct_ThresholdCanetoad;

            bool doSlopeCanetoad = configuration.DoSlopeCanetoad;

            var canetoadConfig = new CanetoadConfiguration
            {
                AmplitudeThresholdCanetoad = amplitudeThresholdCanetoad,
                //RangeCanetoad = rangeCanetoad,
                //DistanceCanetoad = distanceCanetoad,

                BinToreanceCanetoad = binToreanceCanetoad,
                FrameThresholdCanetoad = frameThresholdCanetoad,
                TrackDurationThresholdCanetoad = trackDurationThresholdCanetoad,
                TrackThresholdCanetoad = trackThresholdCanetoad,
                MaximumTrackDurationCanetoad = maximumTrackDurationCanetoad,
                MinimumTrackDurationCanetoad = minimumTrackDurationCanetoad,
                BinDifferencCanetoad = binDifferenceCanetoad,

                FrequencyLowCanetoad = frequencyLowCanetoad,
                FrequencyHighCanetoad = frequencyHighCanetoad,

                MinimumOscillationNumberCanetoad = minimumOscillationNumberCanetoad,
                MaximumOscillationNumberCanetoad = maximumOscillationNumberCanetoad,
                MinimumFrequencyCanetoad = minimumFrequencyCanetoad,
                MaximumFrequencyCanetoad = maximumFrequencyCanetoad,
                Dct_DurationCanetoad = dct_DurationCanetoad,
                Dct_ThresholdCanetoad = dct_ThresholdCanetoad,

                DoSlopeCanetoad = doSlopeCanetoad,
            };

            //***************************************************************//

            //// Caerulea
            //int frequencyLowCaerulea = configuration.FrequencyLowCaerulea;
            //int frequencyHighCaerulea = configuration.FrequencyHighCaerulea;


            //********************Gracillenta********************************//

            // Peak parameters
            double amplitudeThresholdGracillenta = configuration.AmplitudeThresholdGracillenta;   // Decibel---the minimum amplitude value
            //int rangeGracillenta = configuration.RangeGracillenta;                              // Frame---the distance in either side for selecting peaks
            //int distanceGracillenta = configuration.DistanceGracillenta;                        // Frame---remove near peaks
            // Track parameters
            double binToreanceGracillenta = configuration.BinToreanceGracillenta;                 // Bin---the fluctuation of the dominant frequency bin 
            int frameThresholdGracillenta = configuration.FrameThresholdGracillenta;              // Frame---frame numbers of the silence    
            int trackDurationThresholdGracillenta = configuration.TrackDurationThresholdGracillenta;
            double trackThresholdGracillenta = configuration.TrackThresholdGracillenta;           // Used for calculating the percent of peaks in one track    
            int maximumTrackDurationGracillenta = configuration.MaximumTrackDurationGracillenta;  // Minimum duration of tracks
            int minimumTrackDurationGracillenta = configuration.MinimumTrackDurationGracillenta;  // Maximum duration of tracks   
            double binDifferenceGracillenta = configuration.BinDifferenceGracillenta;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            int frequencyLowGracillenta = configuration.FrequencyLowGracillenta;
            int frequencyHighGracillenta = configuration.FrequencyHighGracillenta;
            // DCT
            //int minimumOscillationNumberGracillenta = configuration.minimumOscillationNumberGracillenta;
            //int maximumOscillationNumberGracillenta = configuration.maximumOscillationNumberGracillenta;
            //int minimumFrequencyGracillenta = configuration.MinimumFrequencyGracillenta;
            //int maximumFrequencyGracillenta = configuration.MaximumFrequencyGracillenta;
            //double dct_DurationGracillenta = configuration.Dct_DurationGracillenta;
            //double dct_ThresholdGracillenta = configuration.Dct_ThresholdGracillenta;

            bool doSlopeGracillenta = configuration.DoSlopeGracillenta;

            var gracillentaConfig = new GracillentaConfiguration
            {
                AmplitudeThresholdGracillenta = amplitudeThresholdGracillenta,
                //RangeGracillenta = rangeGracillenta,
                //DistanceGracillenta = distanceGracillenta,

                BinToreanceGracillenta = binToreanceGracillenta,
                FrameThresholdGracillenta = frameThresholdGracillenta,
                TrackDurationThresholdGracillenta = trackDurationThresholdGracillenta,
                TrackThresholdGracillenta = trackThresholdGracillenta,
                MaximumTrackDurationGracillenta = maximumTrackDurationGracillenta,
                MinimumTrackDurationGracillenta = minimumTrackDurationGracillenta,
                BinDifferencGracillenta = binDifferenceGracillenta,

                FrequencyLowGracillenta = frequencyLowGracillenta,
                FrequencyHighGracillenta = frequencyHighGracillenta,

                //MinimumOscillationNumberGracillenta = minimumOscillationNumberGracillenta,
                //MaximumOscillationNumberGracillenta = maximumOscillationNumberGracillenta,
                //MinimumFrequencyGracillenta = minimumFrequencyGracillenta,
                //MaximumFrequencyGracillenta = maximumFrequencyGracillenta,
                //Dct_DurationGracillenta = dct_DurationGracillenta,
                //Dct_ThresholdGracillenta = dct_ThresholdGracillenta,

                DoSlopeGracillenta = doSlopeGracillenta,
            };

            //// Latopalmata                       
            //int frequencyLowLatopalmata = configuration.FrequencyLowLatopalmata;
            //int frequencyHighLatopalmata = configuration.FrequencyHighLatopalmata;


            //********************Nasuta********************************//

            // Peak parameters
            double amplitudeThresholdNasuta = configuration.AmplitudeThresholdNasuta;   // Decibel---the minimum amplitude value
            int rangeNasuta = configuration.RangeNasuta;                                // Frame---the distance in either side for selecting peaks
            int distanceNasuta = configuration.DistanceNasuta;                          // Frame---remove near peaks
            // Track parameters
            double binToreanceNasuta = configuration.BinToreanceNasuta;                 // Bin---the fluctuation of the dominant frequency bin 
            int frameThresholdNasuta = configuration.FrameThresholdNasuta;              // Frame---frame numbers of the silence    
            int trackDurationThresholdNasuta = configuration.TrackDurationThresholdNasuta;
            double trackThresholdNasuta = configuration.TrackThresholdNasuta;           // Used for calculating the percent of peaks in one track    
            int maximumTrackDurationNasuta = configuration.MaximumTrackDurationNasuta;  // Minimum duration of tracks
            int minimumTrackDurationNasuta = configuration.MinimumTrackDurationNasuta;  // Maximum duration of tracks   
            double binDifferenceNasuta = configuration.BinDifferenceNasuta;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            int frequencyLowNasuta = configuration.FrequencyLowNasuta;
            int frequencyHighNasuta = configuration.FrequencyHighNasuta;
            // DCT
            int minimumOscillationNumberNasuta = configuration.minimumOscillationNumberNasuta;
            int maximumOscillationNumberNasuta = configuration.maximumOscillationNumberNasuta;
            int minimumFrequencyNasuta = configuration.MinimumFrequencyNasuta;
            int maximumFrequencyNasuta = configuration.MaximumFrequencyNasuta;
            double dct_DurationNasuta = configuration.Dct_DurationNasuta;
            double dct_ThresholdNasuta = configuration.Dct_ThresholdNasuta;

            bool doSlopeNasuta = configuration.DoSlopeNasuta;

            int harmonicComponentNasuta = configuration.HarmonicComponentNasuta;
            int harmonicSensityNasuta = configuration.HarmonicSensityNasuta;
            int harmonicDiffrangeNasuta = configuration.HarmonicDiffrangeNasuta;

            var nasutaConfig = new NasutaConfiguration
            {
                AmplitudeThresholdNasuta = amplitudeThresholdNasuta,
                RangeNasuta = rangeNasuta,
                DistanceNasuta = distanceNasuta,

                BinToreanceNasuta = binToreanceNasuta,
                FrameThresholdNasuta = frameThresholdNasuta,
                TrackDurationThresholdNasuta = trackDurationThresholdNasuta,
                TrackThresholdNasuta = trackThresholdNasuta,
                MaximumTrackDurationNasuta = maximumTrackDurationNasuta,
                MinimumTrackDurationNasuta = minimumTrackDurationNasuta,
                BinDifferencNasuta = binDifferenceNasuta,

                FrequencyLowNasuta = frequencyLowNasuta,
                FrequencyHighNasuta = frequencyHighNasuta,

                MinimumOscillationNumberNasuta = minimumOscillationNumberNasuta,
                MaximumOscillationNumberNasuta = maximumOscillationNumberNasuta,
                MinimumFrequencyNasuta = minimumFrequencyNasuta,
                MaximumFrequencyNasuta = maximumFrequencyNasuta,
                Dct_DurationNasuta = dct_DurationNasuta,
                Dct_ThresholdNasuta = dct_ThresholdNasuta,

                DoSlopeNasuta = doSlopeNasuta,

                HarmonicComponentNasuta = harmonicComponentNasuta,
                HarmonicSensityNasuta = harmonicSensityNasuta,
                HarmonicDiffrangeNasuta = harmonicDiffrangeNasuta,
            };

            //********************Caerulea********************************//
            // Peak parameters
            double amplitudeThresholdCaerulea = configuration.AmplitudeThresholdCaerulea;   // Decibel---the minimum amplitude value
            int rangeCaerulea = configuration.RangeCaerulea;                                // Frame---the distance in either side for selecting peaks
            int distanceCaerulea = configuration.DistanceCaerulea;                          // Frame---remove near peaks
            // Track parameters
            double binToreanceCaerulea = configuration.BinToreanceCaerulea;                 // Bin---the fluctuation of the dominant frequency bin 
            int frameThresholdCaerulea = configuration.FrameThresholdCaerulea;              // Frame---frame numbers of the silence    
            int trackDurationThresholdCaerulea = configuration.TrackDurationThresholdCaerulea;
            double trackThresholdCaerulea = configuration.TrackThresholdCaerulea;           // Used for calculating the percent of peaks in one track    
            int maximumTrackDurationCaerulea = configuration.MaximumTrackDurationCaerulea;  // Minimum duration of tracks
            int minimumTrackDurationCaerulea = configuration.MinimumTrackDurationCaerulea;  // Maximum duration of tracks   
            double binDifferenceCaerulea = configuration.BinDifferenceCaerulea;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            int frequencyLowCaerulea = configuration.FrequencyLowCaerulea;
            int frequencyHighCaerulea = configuration.FrequencyHighCaerulea;
            // DCT
            int minimumOscillationNumberCaerulea = configuration.minimumOscillationNumberCaerulea;
            int maximumOscillationNumberCaerulea = configuration.maximumOscillationNumberCaerulea;
            int minimumFrequencyCaerulea = configuration.MinimumFrequencyCaerulea;
            int maximumFrequencyCaerulea = configuration.MaximumFrequencyCaerulea;
            double dct_DurationCaerulea = configuration.Dct_DurationCaerulea;
            double dct_ThresholdCaerulea = configuration.Dct_ThresholdCaerulea;

            bool doSlopeCaerulea = configuration.DoSlopeCaerulea;

            var caeruleaConfig = new CaeruleaConfiguration
            {
                AmplitudeThresholdCaerulea = amplitudeThresholdCaerulea,
                RangeCaerulea = rangeCaerulea,
                DistanceCaerulea = distanceCaerulea,

                BinToreanceCaerulea = binToreanceCaerulea,
                FrameThresholdCaerulea = frameThresholdCaerulea,
                TrackDurationThresholdCaerulea = trackDurationThresholdCaerulea,
                TrackThresholdCaerulea = trackThresholdCaerulea,
                MaximumTrackDurationCaerulea = maximumTrackDurationCaerulea,
                MinimumTrackDurationCaerulea = minimumTrackDurationCaerulea,
                BinDifferencCaerulea = binDifferenceCaerulea,

                FrequencyLowCaerulea = frequencyLowCaerulea,
                FrequencyHighCaerulea = frequencyHighCaerulea,

                MinimumOscillationNumberCaerulea = minimumOscillationNumberCaerulea,
                MaximumOscillationNumberCaerulea = maximumOscillationNumberCaerulea,
                MinimumFrequencyCaerulea = minimumFrequencyCaerulea,
                MaximumFrequencyCaerulea = maximumFrequencyCaerulea,
                Dct_DurationCaerulea = dct_DurationCaerulea,
                Dct_ThresholdCaerulea = dct_ThresholdCaerulea,

                DoSlopeCaerulea = doSlopeCaerulea,

            };

            //********************Fallax********************************//
            // Peak parameters
            double amplitudeThresholdFallax = configuration.AmplitudeThresholdFallax;   // Decibel---the minimum amplitude value
            //int rangeFallax = configuration.RangeFallax;                                // Frame---the distance in either side for selecting peaks
            //int distanceFallax = configuration.DistanceFallax;                          // Frame---remove near peaks
            // Track parameters
            double binToreanceFallax = configuration.BinToreanceFallax;                 // Bin---the fluctuation of the dominant frequency bin 
            int frameThresholdFallax = configuration.FrameThresholdFallax;              // Frame---frame numbers of the silence    
            int trackDurationThresholdFallax = configuration.TrackDurationThresholdFallax;
            double trackThresholdFallax = configuration.TrackThresholdFallax;           // Used for calculating the percent of peaks in one track    
            int maximumTrackDurationFallax = configuration.MaximumTrackDurationFallax;  // Minimum duration of tracks
            int minimumTrackDurationFallax = configuration.MinimumTrackDurationFallax;  // Maximum duration of tracks   
            double binDifferenceFallax = configuration.BinDifferenceFallax;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            int frequencyLowFallax = configuration.FrequencyLowFallax;
            int frequencyHighFallax = configuration.FrequencyHighFallax;
            // DCT
            //int minimumOscillationNumberFallax = configuration.minimumOscillationNumberFallax;
            //int maximumOscillationNumberFallax = configuration.maximumOscillationNumberFallax;
            //int minimumFrequencyFallax = configuration.MinimumFrequencyFallax;
            //int maximumFrequencyFallax = configuration.MaximumFrequencyFallax;
            //double dct_DurationFallax = configuration.Dct_DurationFallax;
            //double dct_ThresholdFallax = configuration.Dct_ThresholdFallax;

            bool doSlopeFallax = configuration.DoSlopeFallax;

            var fallaxConfig = new FallaxConfiguration
            {
                AmplitudeThresholdFallax = amplitudeThresholdFallax,
                //RangeFallax = rangeFallax,
                //DistanceFallax = distanceFallax,

                BinToreanceFallax = binToreanceFallax,
                FrameThresholdFallax = frameThresholdFallax,
                TrackDurationThresholdFallax = trackDurationThresholdFallax,
                TrackThresholdFallax = trackThresholdFallax,
                MaximumTrackDurationFallax = maximumTrackDurationFallax,
                MinimumTrackDurationFallax = minimumTrackDurationFallax,
                BinDifferencFallax = binDifferenceFallax,

                FrequencyLowFallax = frequencyLowFallax,
                FrequencyHighFallax = frequencyHighFallax,

                //MinimumOscillationNumberFallax = minimumOscillationNumberFallax,
                //MaximumOscillationNumberFallax = maximumOscillationNumberFallax,
                //MinimumFrequencyFallax = minimumFrequencyFallax,
                //MaximumFrequencyFallax = maximumFrequencyFallax,
                //Dct_DurationFallax = dct_DurationFallax,
                //Dct_ThresholdFallax = dct_ThresholdFallax,

                DoSlopeFallax = doSlopeFallax,
            };

            //********************Latopalmata********************************//


            // Peak parameters
            double amplitudeThresholdLatopalmata = configuration.AmplitudeThresholdLatopalmata;   // Decibel---the minimum amplitude value
            int rangeLatopalmata = configuration.RangeLatopalmata;                                // Frame---the distance in either side for selecting peaks
            int distanceLatopalmata = configuration.DistanceLatopalmata;                          // Frame---remove near peaks
            // Track parameters
            double binToreanceLatopalmata = configuration.BinToreanceLatopalmata;                 // Bin---the fluctuation of the dominant frequency bin 
            int frameThresholdLatopalmata = configuration.FrameThresholdLatopalmata;              // Frame---frame numbers of the silence    
            int trackDurationThresholdLatopalmata = configuration.TrackDurationThresholdLatopalmata;
            double trackThresholdLatopalmata = configuration.TrackThresholdLatopalmata;           // Used for calculating the percent of peaks in one track    
            int maximumTrackDurationLatopalmata = configuration.MaximumTrackDurationLatopalmata;  // Minimum duration of tracks
            int minimumTrackDurationLatopalmata = configuration.MinimumTrackDurationLatopalmata;  // Maximum duration of tracks   
            double binDifferenceLatopalmata = configuration.BinDifferenceLatopalmata;             // Difference between the highest and lowest bins   
            // Band tracks parameters
            int frequencyLowLatopalmata = configuration.FrequencyLowLatopalmata;
            int frequencyHighLatopalmata = configuration.FrequencyHighLatopalmata;
            // DCT
            //int minimumOscillationNumberLatopalmata = configuration.minimumOscillationNumberLatopalmata;
            //int maximumOscillationNumberLatopalmata = configuration.maximumOscillationNumberLatopalmata;
            //int minimumFrequencyLatopalmata = configuration.MinimumFrequencyLatopalmata;
            //int maximumFrequencyLatopalmata = configuration.MaximumFrequencyLatopalmata;
            //double dct_DurationLatopalmata = configuration.Dct_DurationLatopalmata;
            //double dct_ThresholdLatopalmata = configuration.Dct_ThresholdLatopalmata;

            bool doSlopeLatopalmata = configuration.DoSlopeLatopalmata;

            int harmonicComponentLatopalmata = configuration.HarmonicComponentLatopalmata;
            int harmonicSensityLatopalmata = configuration.HarmonicSensityLatopalmata;
            int harmonicDiffrangeLatopalmata = configuration.HarmonicDiffrangeLatopalmata;

            var latopalmataConfig = new LatopalmataConfiguration
            {
                AmplitudeThresholdLatopalmata = amplitudeThresholdLatopalmata,
                RangeLatopalmata = rangeLatopalmata,
                DistanceLatopalmata = distanceLatopalmata,

                BinToreanceLatopalmata = binToreanceLatopalmata,
                FrameThresholdLatopalmata = frameThresholdLatopalmata,
                TrackDurationThresholdLatopalmata = trackDurationThresholdLatopalmata,
                TrackThresholdLatopalmata = trackThresholdLatopalmata,
                MaximumTrackDurationLatopalmata = maximumTrackDurationLatopalmata,
                MinimumTrackDurationLatopalmata = minimumTrackDurationLatopalmata,
                BinDifferencLatopalmata = binDifferenceLatopalmata,

                FrequencyLowLatopalmata = frequencyLowLatopalmata,
                FrequencyHighLatopalmata = frequencyHighLatopalmata,

                //MinimumOscillationNumberLatopalmata = minimumOscillationNumberLatopalmata,
                //MaximumOscillationNumberLatopalmata = maximumOscillationNumberLatopalmata,
                //MinimumFrequencyLatopalmata = minimumFrequencyLatopalmata,
                //MaximumFrequencyLatopalmata = maximumFrequencyLatopalmata,
                //Dct_DurationLatopalmata = dct_DurationLatopalmata,
                //Dct_ThresholdLatopalmata = dct_ThresholdLatopalmata,

                DoSlopeLatopalmata = doSlopeLatopalmata,

                HarmonicComponentLatopalmata = harmonicComponentLatopalmata,
                HarmonicSensityLatopalmata = harmonicSensityLatopalmata,
                HarmonicDiffrangeLatopalmata = harmonicDiffrangeLatopalmata,
            };

            //****************************************************************//
            // Path for saving images
            string saveImagePath = configuration.SaveImagePath;

            // SpectrogramConfiguration for oscillation
            
            int windowSize = configuration.WindowSize;

            //****************************************************************//

  
            // Path of loaded recording

            //string path = configuration.LoadedFilePath;

                      
            var fileEntries = Directory.GetFiles("C:\\Jie\\data\\Segment_JCU_01");

            var fileCount = fileEntries.Count();

            // Canetoad
            var canetoadTrack = new double[257, 726];
            var canetoadEnergy = new double[257, 726];
            var canetoadOscillation = new double[257, 726];

            // Gracillenta
            var gracillentaTrack = new double[257, 726];
            var gracillentaEnergy = new double[257, 726];

            // Nasuta
            var nasutaTrack = new double[257, 726];
            var nasutaHarmonic = new double[257, 726];
            var nasutaOscillation = new double[257, 726];


            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                Log.Info(fileIndex);

                var path = Path.GetFileName(fileEntries[fileIndex]);
                var fullPath = Path.Combine("C:\\Jie\\data\\Segment_JCU_01", path);

                //string fullPath = @"C:\Jie\data\Segment_JCU_01\020313_103min.wav";

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
                var peakHitsCanetoad = CalculateIndexForCanetoad.GetPeakHits(canetoadConfig, spectrogramLong);
                var trackCanetaod = CalculateIndexForCanetoad.GetFrogTracks(canetoadConfig, spectrogramLong, peakHitsCanetoad);
                var oscillationCanetaodRotate = CalculateIndexForCanetoad.GetOscillationRate(canetoadConfig, spectrogramShort);
                var oscillationCanetoad = MatrixTools.MatrixRotate90Anticlockwise(oscillationCanetaodRotate);

                //***********************Gracillenta*************************//

                var peakHitsGracillenta = CalculateIndexForLitoriaGracillenta.GetPeakHitsGracillenta(gracillentaConfig, spectrogramLong);
                var trackGracillenta = CalculateIndexForLitoriaGracillenta.GetFrogTracksGracillenta(gracillentaConfig, spectrogramLong, peakHitsGracillenta);


                //***********************Nasuta*************************//

                var peakHitsNasuta = CalculateIndexForLitoriaNasuta.GetPeakHitsNasuta(nasutaConfig, spectrogramLong);
                var trackNasuta = CalculateIndexForLitoriaNasuta.GetFrogTracksFallax(nasutaConfig, spectrogramLong, peakHitsNasuta);
                var oscillationNasutaRotate = CalculateIndexForLitoriaNasuta.GetOscillationRate(nasutaConfig, spectrogramShort);
                var oscillationNasuta = MatrixTools.MatrixRotate90Anticlockwise(oscillationNasutaRotate);




                //***********************Caerulea*************************//

                //var peakHitsCaerulea = CalculateIndexForLitoriaCaerulea.GetPeakHits(caeruleaConfig, spectrogramLong);
                //var trackCaerulea = CalculateIndexForLitoriaCaerulea.GetFrogTracks(caeruleaConfig, spectrogramLong, peakHitsCaerulea);
                //var oscillationCaerulea = CalculateIndexForLitoriaCaerulea.GetOscillationRate(caeruleaConfig,spectrogramShort);

                //***********************Fallax*************************//

                //var peakHitsFallax = CalculateIndexForLitoriaFallax.GetPeakHitsFallax(fallaxConfig, spectrogramLong);
                //var trackFallax = CalculateIndexForLitoriaFallax.GetFrogTracksFallax(fallaxConfig, spectrogramLong, peakHitsFallax);

                //********************Latopalmata********************************//

                //var peakHitsLatopalmata = CalculateIndexForLitoriaLatopalmata.GetPeakHits(latopalmataConfig,spectrogramLong);
                //var trackLatopalmata = CalculateIndexForLitoriaLatopalmata.GetFrogTracks(latopalmataConfig,spectrogramLong,peakHitsLatopalmata);



                // Create 21 matrix to save the features
                // Canetoad: Item1-arrayresult, Item4-energy
                
                var trackFeatureCanetoad = trackCanetaod.Item1;
                var energyFeatureCanetoad = trackCanetaod.Item3; // Need to be normalised

                var oscillationFeatureCanetoad = new double[oscillationCanetoad.GetLength(0)];
                for (int i = 0; i < oscillationCanetoad.GetLength(0); i++)
                {
                    double tempOscillation = 0;
                    for (int j = 0; j < oscillationCanetoad.GetLength(1); j++)
                    {
                        tempOscillation = tempOscillation + oscillationCanetoad[i, j];                    
                    }
                    oscillationFeatureCanetoad[i] = tempOscillation;
                }

                for (int i = 0; i < 257; i++)
                {
                    canetoadTrack[i, fileIndex] = trackFeatureCanetoad[i];
                    canetoadEnergy[i, fileIndex] = energyFeatureCanetoad[i];
                    canetoadOscillation[i, fileIndex] = oscillationFeatureCanetoad[i];
                }
                

                // Gracillenta
                var trackFeatureGracillenta = trackGracillenta.Item1;
                var energyFeatureGracillenta = trackGracillenta.Item3; // Need to be normalised

                for (int i = 0; i < 257; i++)
                {
                    gracillentaTrack[i, fileIndex] = trackFeatureGracillenta[i];
                    gracillentaEnergy[i, fileIndex] = energyFeatureGracillenta[i];
                }

                // Nasuta
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
                    nasutaTrack[i, fileIndex] = trackFeatureNasuta[i];
                    nasutaHarmonic[i, fileIndex] = harmonicArrayNasuta[i];
                    nasutaOscillation[i, fileIndex] = oscillationFeatureNasuta[i];
                }


                // Caerulea

                // Fallax

                // Latopalmata

            }
            // Write 7 frog species with 3 features to csv files  
            // Canetoad
            
            FileTools.WriteMatrix2File(canetoadTrack, @"C:\Jie\output\indexCanetoad\canetoadTrack.csv");
            FileTools.WriteMatrix2File(canetoadEnergy, @"C:\Jie\output\indexCanetoad\canetoadEnergy.csv");
            FileTools.WriteMatrix2File(canetoadOscillation, @"C:\Jie\output\indexCanetoad\canetoadOscillation.csv");
            
            // Gracillenta
            FileTools.WriteMatrix2File(gracillentaTrack, @"C:\Jie\output\indexGracillenta\gracillentaTrack.csv");
            FileTools.WriteMatrix2File(gracillentaEnergy, @"C:\Jie\output\indexGracillenta\gracillentaEnergy.csv");

            // Nasuta
            FileTools.WriteMatrix2File(nasutaTrack, @"C:\Jie\output\indexNasuta\nasutaTrack.csv");
            FileTools.WriteMatrix2File(nasutaHarmonic, @"C:\Jie\output\indexNasuta\nasutaHarmonic.csv");
            FileTools.WriteMatrix2File(nasutaOscillation, @"C:\Jie\output\indexNasuta\nasutaOscillation.csv");


            // Caerulea

            // Fallax

            // Latopalmata

            Log.Info("OK");


            //// Step.3 Draw spectrogram

            //double[,] spectrogramMatrix = DataTools.normalise(spectrogramLong.Data);

            //int rows = spectrogramMatrix.GetLength(0);
            //int cols = spectrogramMatrix.GetLength(1);

            //Color[] grayScale = ImageTools.GrayScale();
            //Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            //for (int r = 0; r < rows; r++)
            //{
            //    for (int c = 0; c < cols; c++)
            //    {
            //        int greyId = (int)Math.Floor(spectrogramMatrix[r, c] * 255);
            //        if (greyId < 0) greyId = 0;
            //        else
            //            if (greyId > 255) greyId = 255;

            //        greyId = 255 - greyId;
            //        bmp.SetPixel(c, r, grayScale[greyId]);
            //    }
            //}

            //for (int i = 0; i < rows; i++)
            //{
            //    for (int j = 0; j < cols; j++)
            //    {
            //        if (trackNasuta.HarmonicHitsNasuta[j, i] != 0)
            //        {
            //            bmp.SetPixel((cols - j), i, Color.Blue);
            //        }

            //    }
            //}

            //bmp.Save(saveImagePath);




        }
        
    }
}
