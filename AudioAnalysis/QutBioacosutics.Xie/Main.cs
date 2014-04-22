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

            string path = configuration.LoadedFilePath;

            if (path == null)
            {
                path = @"C:\Jie\data\Segment_JCU_01\020313_429min.wav";
            }
            
            var recording = new AudioRecording(path);

            // Step.1 Generate spectrogarm
            // A. Generate spectrogram for extracting tracks, entropy and harmonic
             
            var spectrogramLongConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = windowSize };
            var spectrogramLong = new SpectrogramStandard(spectrogramLongConfig, recording.GetWavReader());
            
            
            // B. Generate spectrogram for extracting oscillation rate

            //*************************************************************//
            /*                      
            // Calculate windowOverlap
            //double windowOverlap = XieFunction.CalculateRequiredWindowOverlap(recording.SampleRate, windowSize, canetoadConfig.MaximumOscillationNumberCanetoad);

            var spectrogramShortConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.NONE, WindowOverlap = 0.5, WindowSize = windowSize };
            var spectrogramShort = new SpectrogramStandard(spectrogramShortConfig, recording.GetWavReader());
            
            */
            // Step.2 Produce features

            // A. Tracks &  B. Entropy

            //***********************Canetoad*************************//

            /*



            */

            //***********************Gracillenta*************************//
            // 2. Gracillenta detection  (Frequency band is overlapped with Nasuta, but the duration is different)

            /*
            var peakHitsGracillenta = FindLocalPeaks.Max(spectrogramLong, gracillentaConfig.AmplitudeThresholdGracillenta, gracillentaConfig.FrequencyLowGracillenta,
                                                            gracillentaConfig.FrequencyHighGracillenta);

            var trackHitsGracillenta = ExtractTracks.GetTracks(spectrogramLong, peakHitsGracillenta, gracillentaConfig.FrequencyLowGracillenta, gracillentaConfig.FrequencyHighGracillenta,
                                                gracillentaConfig.BinToreanceGracillenta, gracillentaConfig.FrameThresholdGracillenta, gracillentaConfig.TrackDurationThresholdGracillenta,
                                                gracillentaConfig.TrackThresholdGracillenta, gracillentaConfig.MaximumTrackDurationGracillenta, gracillentaConfig.MinimumTrackDurationGracillenta,
                                                gracillentaConfig.BinDifferencGracillenta, gracillentaConfig.DoSlopeGracillenta);

            */
            //***********************Nasuta*************************//

            // 3. Nasuta detection (Harmonic structure)
            /*


            */

            //***********************Caerulea*************************//
            /*
            var peakHitsCaerulea = FindLocalPeaks.LocalPeaks(spectrogramLong, caeruleaConfig.AmplitudeThresholdCaerulea, caeruleaConfig.RangeCaerulea, caeruleaConfig.DistanceCaerulea,
                                                            caeruleaConfig.FrequencyLowCaerulea, caeruleaConfig.FrequencyHighCaerulea);

            var peakHitsCaeruleaRotated = MatrixTools.MatrixRotate90Anticlockwise(peakHitsCaerulea);

            var trackHitsCaerulea = ExtractTracks.GetTracks(spectrogramLong, peakHitsCaeruleaRotated, caeruleaConfig.FrequencyLowCaerulea, caeruleaConfig.FrequencyHighCaerulea,
                                                            caeruleaConfig.BinToreanceCaerulea, caeruleaConfig.FrameThresholdCaerulea, caeruleaConfig.TrackDurationThresholdCaerulea,
                                                            caeruleaConfig.TrackThresholdCaerulea, caeruleaConfig.MaximumTrackDurationCaerulea, caeruleaConfig.MinimumTrackDurationCaerulea,
                                                            caeruleaConfig.BinDifferencCaerulea, caeruleaConfig.DoSlopeCaerulea);
            
            // Find the peaks based on tracks (# should be 2 or 3)

            var CaeruleaOscillationHits = FindOscillation.CalculateOscillationRate(spectrogramShort, caeruleaConfig.MinimumFrequencyCaerulea, caeruleaConfig.MaximumFrequencyCaerulea,
                                                                                   caeruleaConfig.Dct_DurationCaerulea, caeruleaConfig.Dct_ThresholdCaerulea,
                                                                                   caeruleaConfig.MinimumOscillationNumberCaerulea, caeruleaConfig.MaximumOscillationNumberCaerulea);

            */



            //********************Latopalmata********************************//

            var peakHitsLatopalmata = FindLocalPeaks.LocalPeaks(spectrogramLong, latopalmataConfig.AmplitudeThresholdLatopalmata, latopalmataConfig.RangeLatopalmata, 
                                                                latopalmataConfig.DistanceLatopalmata,latopalmataConfig.FrequencyLowLatopalmata, latopalmataConfig.FrequencyHighLatopalmata);

            var peakHitsLatopalmataRotated = MatrixTools.MatrixRotate90Anticlockwise(peakHitsLatopalmata);

            var trackHitsLatopalmata = ExtractTracks.GetTracks(spectrogramLong, peakHitsLatopalmataRotated, latopalmataConfig.FrequencyLowLatopalmata,
                                                               latopalmataConfig.FrequencyHighLatopalmata, latopalmataConfig.BinToreanceLatopalmata,
                                                               latopalmataConfig.FrameThresholdLatopalmata, latopalmataConfig.TrackDurationThresholdLatopalmata,
                                                               latopalmataConfig.TrackThresholdLatopalmata, latopalmataConfig.MaximumTrackDurationLatopalmata,
                                                               latopalmataConfig.MinimumTrackDurationLatopalmata, latopalmataConfig.BinDifferencLatopalmata,
                                                               latopalmataConfig.DoSlopeLatopalmata);


            // Contain harmonic structure & 
            var harmonicHitsLatopalmata = FindHarmonics.GetHarmonic(trackHitsLatopalmata.Item4, latopalmataConfig.HarmonicComponentLatopalmata,
                                                                    latopalmataConfig.HarmonicSensityLatopalmata, latopalmataConfig.HarmonicDiffrangeLatopalmata);







            // Step.3 Draw spectrogram

            double[,] spectrogramMatrix = DataTools.normalise(spectrogramLong.Data);
            //var result = MatrixTools.MatrixRotate90Anticlockwise(peakHitsLatopalmata);
            //var spectrogramMatrix = MatrixTools.MatrixRotate90Clockwise(result);

            int rows = spectrogramMatrix.GetLength(0);
            int cols = spectrogramMatrix.GetLength(1);

            Color[] grayScale = ImageTools.GrayScale();
            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int greyId = (int)Math.Floor(spectrogramMatrix[r, c] * 255);
                    if (greyId < 0) greyId = 0;
                    else
                        if (greyId > 255) greyId = 255;

                    greyId = 255 - greyId;
                    bmp.SetPixel(c, r, grayScale[greyId]);
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (harmonicHitsLatopalmata[j, i] != 0)
                    {
                        bmp.SetPixel((cols - j), i, Color.Blue);
                    }

                }
            }

            bmp.Save(saveImagePath);


            // Step.4 Draw false-color spectrogram



            Log.Info("OK");
            

            
            //string ipDirStr = @"C:\Jie\output\index1";
            //string opDirStr = @"C:\Jie\output\index1";

            ////..........................................................//
            ////Draw False color spectrogram
            //LDSpectrogramConfig spgConfig = new LDSpectrogramConfig(fileName, ipDir, opDir);
            ////spgConfig.ColourMap = "TRC-OSC-HAR";
            //spgConfig.ColourMap = "OSC-HAR-TRC";
            //spgConfig.MinuteOffset = startMinute;
            //spgConfig.FrameWidth = 256;
            ////spgConfig.SampleRate = 17640;
            //spgConfig.SampleRate = 22050;
            //FileInfo path = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
            //spgConfig.WritConfigToYAML(path);
            //DrawLDSpectrogram.DrawFalseColourSpectrograms(spgConfig);

            ////..........................................................//
            ////Read csc files and save them to make three indexes

            //var trackResult = new double[726, 257];
            //var longtrackResult = new double[726, 257];
            //var oscillationResult = new double[726, 257];
            //var harmonicResult = new double[726, 257];

            //var csvFiles = Directory.GetFiles("C:\\Jie\\output\\csv");

            //var csvCount = csvFiles.Count();

            //for (int csvIndex = 0; csvIndex < csvCount; csvIndex++)
            //{
            //    var csvfile = CsvTools.ReadCSVFile2Matrix(csvFiles[csvIndex]);


            //    string fullName = Path.GetFileNameWithoutExtension(csvFiles[csvIndex]);

            //    string num = Path.GetFileNameWithoutExtension(fullName);
            //    int numVal = 0;
            //    if (num.Length == 11)
            //    {
            //        string subnum = num.Substring(7, 1);
            //        numVal = Int32.Parse(subnum);
            //    }

            //    if (num.Length == 12)
            //    {
            //        string subnum = num.Substring(7, 2);
            //        numVal = Int32.Parse(subnum);
            //    }

            //    if (num.Length == 13)
            //    {
            //        string subnum = num.Substring(7, 3);
            //        numVal = Int32.Parse(subnum);
            //    }


            //    for (int i = 0; i < csvfile.GetLength(0); i++)
            //    {
            //        trackResult[numVal, i] = csvfile[i, 0];
            //        longtrackResult[numVal, i] = csvfile[i, 1];
            //        oscillationResult[numVal, i] = csvfile[i, 2];
            //        harmonicResult[numVal, i] = csvfile[i, 3];
            //    }
            //}

            //FileTools.WriteMatrix2File(trackResult, @"C:\Jie\output\index2\track.csv");
            //FileTools.WriteMatrix2File(oscillationResult, @"C:\Jie\output\index2\oscillation.csv");
            //FileTools.WriteMatrix2File(harmonicResult, @"C:\Jie\output\index2\harmonic.csv");

            
            ////Write 3 index matirxes to csv file
            //int csvRow = trackResult.GetLength(0);
            //int csvCol = trackResult.GetLength(1);

            //for (int c = 0; c < csvCol; c++)
            //{
            //    var lines = new string[csvRow + 1];
            //    for (int r = 0; r < csvRow; r++)
            //    {
            //        lines[r] = trackResult[r, c].ToString();
            //    }

            //    FileTools.WriteTextFile(@"C:\\Jie\\output\\index\\track.csv", lines);
            //}

            //FileTools.WriteTextFile(@"C:\\Jie\\output\\index\\track.csv", trackResult);
          

            //var fileEntries = Directory.GetFiles("C:\\Jie\\data\\Segment_JCU_01");

            //var fileCount = fileEntries.Count();



            //for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            //{
            //    //string path = fileEntries[319];
            //    string path = @"C:\Jie\data\canetoad2.wav";

            //    string num = Path.GetFileNameWithoutExtension(path);

            //    string outpath = Path.GetFileNameWithoutExtension(num);

            //    string outPath = Path.Combine("c:\\jie\\output\\csv", outpath);
            //    string outPath2 = Path.ChangeExtension(outPath, ".csv");



            //    var FrogIndexList = new List<FrogIndex>();
            //    for (int i = (norHArray.Length - 1); i > 0; i--)
            //    {
            //        var FrogIndex = new FrogIndex();
            //        FrogIndex.Track = norTArray[i];
            //        //FrogIndex.LongTrack = norLongTArray[i];
            //        FrogIndex.Oscillation = oscillationArray[i];
            //        FrogIndex.Harmonic = norHArray[i];

            //        FrogIndexList.Add(FrogIndex);
            //    }

            //    //var FrogIndex = new List<List<string>>();

            //    //FrogIndex.Add(new List<string> { norTArray.ToString(), norOscArray.ToString(), norHArray.ToString() });

            //    FileInfo fileInfo = new FileInfo(outPath2);

            //    CsvTools.WriteResultsToCsv(fileInfo, FrogIndexList);


            //    // Write the index to three matrix

            //    //for (int r = 0; r < norHArray.Length; r++)
            //    //{
            //    //    trackResult[r, numVal] = norTArray[r];
            //    //}

            //    //for (int r = 0; r < norHArray.Length; r++)
            //    //{
            //    //    oscillationResult[r, numVal] = oscillationArray[r];
            //    //}

            //    //for (int r = 0; r < norHArray.Length; r++)
            //    //{
            //    //    harmonicResult[r, numVal] = norHArray[r];
            //    //}

            //    //Log.Info(numVal);
            //}

            //// Write 3 index into csv file
            ////var FrogIndexList = new List<FrogIndex>();
            ////var FrogIndex = new FrogIndex();

            ////FrogIndex.Track = trackResult;
            ////FrogIndex.Oscillation = oscillationResult;
            ////FrogIndex.Harmonic = harmonicResult;

            ////FrogIndexList.Add(FrogIndex);

            ////FileInfo fileInfo = new FileInfo(imagePath);
            ////CsvTools.WriteResultsToCsv(fileInfo, FrogIndexList);

            //Log.Info("Analysis complete");


            //var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramLongTrack.Data);


            // save the arrays (1-trackArray,2-oscillationArray,3-harmonicArray) to CSV file.
            //var NormaliseHarmonicArray = norHArray.Reverse();
            //var NormaliseOscillationArray = norOscArray.Reverse();
            //var NormaliseTrackArray = norTArray.Reverse();

            //var FrogIndexList = new List<FrogIndex>();
            //for (int i = (norHArray.Length - 1); i > 0; i--)
            //{
            //    var FrogIndex = new FrogIndex();
            //    FrogIndex.Track = norTArray[i];
            //    //FrogIndex.LongTrack = norLongTArray[i];
            //    FrogIndex.Oscillation = norOscArray[i];
            //    FrogIndex.Harmonic = norHArray[i];

            //    FrogIndexList.Add(FrogIndex);
            //}

            ////var FrogIndex = new List<List<string>>();

            ////FrogIndex.Add(new List<string> { norTArray.ToString(), norOscArray.ToString(), norHArray.ToString() });

            //FileInfo fileInfo = new FileInfo(imagePath);

            //CsvTools.WriteResultsToCsv(fileInfo, FrogIndexList);


        }

        
    }
}
