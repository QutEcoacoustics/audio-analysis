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
            /* 
            var spectrogramLongConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = windowSize };
            var spectrogramLong = new SpectrogramStandard(spectrogramLongConfig, recording.GetWavReader());
            
            */
            // B. Generate spectrogram for extracting oscillation rate

            //*************************************************************//
                                   
            // Calculate windowOverlap
            //double windowOverlap = XieFunction.CalculateRequiredWindowOverlap(recording.SampleRate, windowSize, canetoadConfig.MaximumOscillationNumberCanetoad);

            var spectrogramShortConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.NONE, WindowOverlap = 0.5, WindowSize = windowSize };
            var spectrogramShort = new SpectrogramStandard(spectrogramShortConfig, recording.GetWavReader());
            
            
            // Step.2 Produce features

            // A. Tracks &  B. Entropy

            //***********************Canetoad*************************//

            /*

            var peakHitsCanetoad = FindLocalPeaks.Max(spectrogramLong, canetoadConfig.AmplitudeThresholdCanetoad, canetoadConfig.FrequencyLowCanetoad, 
                                                        canetoadConfig.FrequencyHighCanetoad);

            var trackHitsCanetoad = ExtractTracks.GetTracks(spectrogramLong, peakHitsCanetoad, canetoadConfig.FrequencyLowCanetoad,canetoadConfig.FrequencyHighCanetoad,
                                                            canetoadConfig.BinToreanceCanetoad, canetoadConfig.FrameThresholdCanetoad, canetoadConfig.TrackDurationThresholdCanetoad,
                                                            canetoadConfig.TrackThresholdCanetoad, canetoadConfig.MaximumTrackDurationCanetoad,canetoadConfig.MinimumTrackDurationCanetoad,
                                                            canetoadConfig.BinDifferencCanetoad, canetoadConfig.DoSlopeCanetoad);
            
            // C. Oscillation rate
            

            
            
            // 1. Cane_toad detection  
            var canetoadOscillationHits = FindOscillation.CalculateOscillationRate(spectrogramShort, canetoadConfig.MinimumFrequencyCanetoad, canetoadConfig.MaximumFrequencyCanetoad,
                                                                                   canetoadConfig.Dct_DurationCanetoad, canetoadConfig.Dct_ThresholdCanetoad,
                                                                                   canetoadConfig.MinimumOscillationNumberCanetoad, canetoadConfig.MaximumOscillationNumberCanetoad);           
            // Find canetoad events
            var canetoadOscillationResults = RemoveSparseHits.PruneHits(canetoadOscillationHits);

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
            var peakHitsNasuta = FindLocalPeaks.LocalPeaks(spectrogramLong, nasutaConfig.AmplitudeThresholdNasuta, nasutaConfig.RangeNasuta, nasutaConfig.DistanceNasuta,
                                                            nasutaConfig.FrequencyLowNasuta, nasutaConfig.FrequencyHighNasuta);
            var peakHitsNasutaRotated = MatrixTools.MatrixRotate90Anticlockwise(peakHitsNasuta);



            var trackHitsNasuta = ExtractTracks.GetTracks(spectrogramLong, peakHitsNasutaRotated, nasutaConfig.FrequencyLowNasuta, nasutaConfig.FrequencyHighNasuta,
                                                nasutaConfig.BinToreanceNasuta, nasutaConfig.FrameThresholdNasuta, nasutaConfig.TrackDurationThresholdNasuta,
                                                nasutaConfig.TrackThresholdNasuta, nasutaConfig.MaximumTrackDurationNasuta, nasutaConfig.MinimumTrackDurationNasuta,
                                                nasutaConfig.BinDifferencNasuta, nasutaConfig.DoSlopeNasuta);
            
            //D. Harmonic
            var harmonicHitsNasuta = FindHarmonics.GetHarmonic(trackHitsNasuta.Item4,nasutaConfig.HarmonicComponentNasuta, nasutaConfig.HarmonicSensityNasuta);

            var NasutaOscillationHits = FindOscillation.CalculateOscillationRate(spectrogramShort, nasutaConfig.MinimumFrequencyNasuta, nasutaConfig.MaximumFrequencyNasuta,
                                                                                   nasutaConfig.Dct_DurationNasuta, nasutaConfig.Dct_ThresholdNasuta, 
                                                                                   nasutaConfig.MinimumOscillationNumberNasuta, nasutaConfig.MaximumOscillationNumberNasuta);

            var nasutaOscillationResults = RemoveSparseHits.PruneHits(NasutaOscillationHits);

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
            */
            // Find the peaks based on tracks (# should be 2 or 3)

            var CaeruleaOscillationHits = FindOscillation.CalculateOscillationRate(spectrogramShort, caeruleaConfig.MinimumFrequencyCaerulea, caeruleaConfig.MaximumFrequencyCaerulea,
                                                                                   caeruleaConfig.Dct_DurationCaerulea, caeruleaConfig.Dct_ThresholdCaerulea,
                                                                                   caeruleaConfig.MinimumOscillationNumberCaerulea, caeruleaConfig.MaximumOscillationNumberCaerulea);

            



            // Step.3 Draw spectrogram

            double[,] spectrogramMatrix = DataTools.normalise(spectrogramShort.Data);
            var result = MatrixTools.MatrixRotate90Anticlockwise(CaeruleaOscillationHits);
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
                    if (result[j, i] != 0)
                    {
                        bmp.SetPixel((cols - j), i, Color.Blue);
                    }

                }
            }

            bmp.Save(saveImagePath);


            // Step.4 Draw false-color spectrogram



            Log.Info("OK");
            

            
            //FileInfo path = ((string)configuration.file).ToFileInfo();

            //if (source != null)
            //{
            //    path = source;
            //}

            //Log.Info(@path);

            //string outpath;
            //var fileName = path.Name;

            //outpath = Path.GetFileNameWithoutExtension(fileName);

            //string outPath = Path.Combine("c:\\jie\\output\\csv", outpath);
            //string outPath2 = Path.ChangeExtension(outPath, ".csv");
            
            //string imagePath = outPath2;
            //string imagePath = configuration.image_path;
            //string ipDirStr = @"C:\Jie\output\index1";
            //string opDirStr = @"C:\Jie\output\index1";
            //double amplitudeThreshold = configuration.amplitude_threshold;
            //int range = configuration.range;
            //int distance = configuration.distance;
            //double binToreance = configuration.binToreance;
            //int frameThreshold = configuration.frameThreshold;
            //int duraionThreshold = configuration.duraionThreshold;
            //double trackThreshold = configuration.trackThreshold;
            //int maximumDuration = configuration.maximumDuration;
            //int minimumDuration = configuration.minimumDuration;
            //double maximumDiffBin = configuration.maximumDiffBin;

            //int colThreshold = configuration.colThreshold;
            //int zeroBinIndex = configuration.zeroBinIndex;

            // Change seconds to framesize



            ////..........................................................//
            ////Draw False color spectrogram
            //string fileName = "frogs_DATE";
            //var ipDir = new DirectoryInfo(ipDirStr);
            //var opDir = new DirectoryInfo(opDirStr);
            //int startMinute = 19 * 60;

            //LDSpectrogramConfig spgConfig = new LDSpectrogramConfig(fileName, ipDir, opDir);
            ////spgConfig.ColourMap = "TRC-OSC-HAR";
            //spgConfig.ColourMap = "OSC-HAR-TRC";
            //spgConfig.MinuteOffset = startMinute;
            //spgConfig.FrameWidth = 256;
            ////spgConfig.SampleRate = 17640;
            //spgConfig.SampleRate = 22050;
            //FileInfo path = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
            //spgConfig.WritConfigToYAML(path);
            ////LDSpectrogramRGB.DrawFalseColourSpectrograms(spgConfig);
            //XieFunction.DrawFalseColourSpectrograms(spgConfig);

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

            // bool noiseReduction = (int)configuration.do_noise_reduction == 1;

            //float noiseThreshold = configuration.noise_threshold;
            //Dictionary<string, string> complexSettings = configuration.complex_settings;
            //string[] simpleArray = configuration.array_example;
            //int[] simpleArray2 = configuration.array_example_2;
            //int? missingValue = configuration.i_dont_exist;
            //string doober = configuration.doobywacker;

            // the following will always throw an exception
            //int missingValue2 = configuration.i_also_dont_exist;

            // Execute analysis

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


            //    //int numVal = 0;
            //    //if (num.Length == 11)
            //    //{
            //    //    string subnum = num.Substring(7, 1);
            //    //    numVal = Int32.Parse(subnum);
            //    //}

            //    //if (num.Length == 12)
            //    //{
            //    //    string subnum = num.Substring(7, 2);
            //    //    numVal = Int32.Parse(subnum);
            //    //}

            //    //if (num.Length == 13)
            //    //{
            //    //    string subnum = num.Substring(7, 3);
            //    //    numVal = Int32.Parse(subnum);
            //    //}


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
            
            // the dominant frequency of Litoria fallax is 4750Hz, thus the frequency band of this frog is 4500Hz - 5000Hz, the duration is 430ms, the cycles per second is 70.
            //var fallaxLowBin = (int)(4500 / spectrogramLongTrack.FBinWidth);
            //var fallaxHighBin = (int)(5000 / spectrogramLongTrack.FBinWidth);

            //var fallaxminDuration = (int) (spectrogramLongTrack.FramesPerSecond * 400 / 1000);
            //var fallaxmaxDuration = (int) (spectrogramLongTrack.FramesPerSecond * 460 / 1000);

            //// find the maximum in the specify frequency band
            //var fallaxMatrix = new double[rows, cols];
            //var findnasutaPeaks = new FindLocalPeaks();
            //fallaxMatrix = findnasutaPeaks.MaximumOfBand(matrix, amplitudeThreshold, fallaxHighBin, fallaxLowBin);

            //var image = ImageTools.DrawMatrix(fallaxMatrix);
            //image.Save(imagePath);



            //var fallaxTracks = new ExtractTracks();
            //var fallaxresult = fallaxTracks.GetTracks(fallaxMatrix, 3, frameThreshold, duraionThreshold, trackThreshold, fallaxmaxDuration, fallaxminDuration, 10);



            //var rowLow = (int)(2000 / spectrogramLongTrack.FBinWidth);
            //var rowHigh = (int)(2500 / spectrogramLongTrack.FBinWidth);

            //var tempArray = new double[cols - 1];

            //for (int c = 0; c < (cols - 1); c++)
            //{
            //    double energyDiff = 0;
            //    for (int r = rowLow; r < rowHigh; r++)
            //    {
            //        //var tempDiff = matrix[r, c] * Math.Pow(matrix[r, c] - matrix[r, c + 1], 2);
            //        var tempDiff = Math.Pow(matrix[r, c], 2);
            //        energyDiff = energyDiff + tempDiff;
            //    }

            //    tempArray[c] = energyDiff;
            //}

            ////var result = DataTools.AutoCorrelation(tempArray,0,tempArray.Length);

            //DataTools.writeBarGraph(tempArray);

 

            //double[,] spectrogramMatrix = DataTools.normalise(spectrogramLongTrack.Data);            
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

            //bmp.Save(imagePath);


            //var image = ImageTools.DrawMatrix(spectrogramLongTrack.Data);
            //image.Save(imagePath);



            // find the oscillation through all the recordings

            //spectrogramOscillation.Data = MatrixTools.MatrixRotate90Anticlockwise(spectrogramOscillation.Data);




            //var image = ImageTools.DrawMatrix(oscillationArray);
            //image.Save(@"C:\Jie\output\a.png");


            //var norOscArray = oscillationArray;
            


            //peakMatrix = MatrixTools.MatrixRotate90Anticlockwise(peakMatrix);
            //var image = ImageTools.DrawMatrix(trackLongMatrix);
            //image.Save(imagePath);

            

            //var image = ImageTools.DrawMatrix(peakMatrix);
            //image.Save(imagePath);



            //var image = ImageTools.DrawMatrix(trackMatrix);
            //image.Save(imagePath);

            // find the harmonic structure based on tracks

            //trackMatrix = MatrixTools.MatrixRotate90Anticlockwise(trackMatrix);
            //double[,] spectrogramMatrix = DataTools.normalise(spectrogramOscillation.Data);
            //spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramMatrix);

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
            //        if (trackMatrix[i, j] == 1)
            //        {
            //            bmp.SetPixel(j, i, Color.Blue);
            //        }

            //    }
            //}

            //for (int i = 0; i < rows; i++)
            //{
            //    for (int j = 0; j < cols; j++)
            //    {
            //        if (trackMatrix[i, j] == 2)
            //        {
            //            bmp.SetPixel(j, i, Color.Red);
            //        }

            //    }
            //}

            //bmp.Save(imagePath);

            //double[,] spectrogramLongMatrix = DataTools.normalise(LongTrackSmoothMatrix);
            //spectrogramLongMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramLongMatrix);

            //int rows = spectrogramLongMatrix.GetLength(0);
            //int cols = spectrogramLongMatrix.GetLength(1);

            //Color[] grayScale = ImageTools.GrayScale();
            //Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            ////bmp.Save(imagePath);

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
