using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using TowseyLibrary;
using AudioAnalysisTools;
using AudioAnalysisTools.Indices;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.LongDurationSpectrograms;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;
using Acoustics.Shared;

namespace AnalysisPrograms
{
    using PowerArgs;


    /// <summary>
    /// Activity Code for this class:= sandpit
    ///
    /// Activity Codes for other tasks to do with spectrograms and audio files:
    /// 
    /// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
    /// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
    /// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
    /// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
    /// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
    /// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
    ///
    /// audiofilecheck - Writes information about audio files to a csv file.
    /// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
    /// audiocutter - Cuts audio into segments of desired length and format
    /// createfoursonograms 
    /// </summary>
    public class Sandpit
    {
        public const int RESAMPLE_RATE = 17640;
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";

        public class Arguments
        {
        }

        public static void Dev(Arguments arguments)
        {

            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());

            if (false)  // 
            {
                CubeHelix.DrawTestImage();
                LoggedConsole.WriteLine("FINSIHED");
            }
            
            if (false)  // construct 3Dimage of audio
            {
                //TowseyLibrary.Matrix3D.TestMatrix3dClass();
                LDSpectrogram3D.Main(null);
                LoggedConsole.WriteLine("FINSIHED");
            }

            if (false)  // call SURF image Feature extraction
            {
                //SURFFeatures.SURF_TEST();
                SURFAnalysis.Main(null);
                LoggedConsole.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)  // do test of SNR calculation
            {
                //Audio2InputForConvCNN.Main(null);
                Audio2InputForConvCNN.ProcessMeriemsDataset();
                //SNR.Calculate_SNR_ofXueyans_data();
                LoggedConsole.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }

            // // TEST TO DETERMINE whether one of the signal channels has microphone problems due to rain or whatever.
            if (true)  
            {
                ChannelIntegrity.Execute(null);
            }





            if (false)  // do test of new moving average method
            {
                DataTools.TEST_FilterMovingAverage();
            }


            if (false)
            {
                ImageTools.TestCannyEdgeDetection();
            }


            if (false)
            {
                //HoughTransform.Test1HoughTransform();
                HoughTransform.Test2HoughTransform();
            }


            if (false)  // used to test structure tensor code.
            {
                StructureTensor.Test1StructureTensor();
                StructureTensor.Test2StructureTensor();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            /// used to caluclate eigen values and singular valuse
            if (false)  
            {

                SvdAndPca.TestEigenValues();
                Log.WriteLine("FINSIHED");
                 Console.ReadLine();
                 System.Environment.Exit(0);
            }


            if (false)  // test examples of wavelets
            {
                WaveletTransformContinuous.ExampleOfWavelets_1();
                //WaveletPacketDecomposition.ExampleOfWavelets_1();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)  // do 2D-FFT of an image.
            {
                FFT2D.TestFFT2D();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }



            if (false)  // concatenating spectrogram images with gaps between them.
            {
                LDSpectrogramStitching.StitchPartialSpectrograms();
                LDSpectrogramStitching.StitchPartialSpectrograms();

                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            // quickie to calculate entropy of some matrices - used for Yvonne acoustic transition matrices
            if (true)
            {
                string dir = @"H:\Documents\SensorNetworks\MyPapers\2016_EcoAcousticCongress_Abstract\TransitionMatrices";
                string filename = @"transition_matrix_BYR4_16Oct.csv";
                //string filename = @"transition_matrix_SE_13Oct.csv";
                //double[,] M = CsvTools.ReadCSVFile2Matrix(Path.Combine(dir, filename)); //DEPRACATED
                //double[] v = DataTools.Matrix2Array(M);

                // these are actual call counts for ~60 bird species calling each day at SERF - see comment at end of each line
                //double[] v = {9, 1, 4, 1, 58, 9, 28, 11, 24, 54, 1, 36, 12, 23, 12, 228, 66, 5, 15, 13, 4, 9, 21, 85, 5, 19, 1, 4, 44, 2, 47, 3, 0, 38, 62, 10, 2, 22, 384, 19, 4, 5, 629, 9, 25, 35, 141, 86, 21, 5, 16, 1, 121, 4, 3, 70, 6, 11, 1, 139, 11, 84, 1, 39, 254}; // NE 13thOct2010
                //double[] v = {5, 2, 3, 44, 40, 40, 22, 42, 30, 2, 21, 27, 249, 58, 20, 4, 18, 1, 11, 9, 67, 30, 24, 83, 34, 1, 1, 47, 5, 4, 1, 12, 415, 43, 13, 3, 428, 26, 101, 253, 72, 68, 0, 16, 1, 1, 1, 90, 1, 70, 22, 1, 1, 110, 14, 146, 1, 52, 731 }; // NE 14thOct2010
                //double[] v = { 7, 1, 14, 6, 24, 1, 8, 10, 45, 62, 2, 31, 5, 7, 216, 1, 42, 50, 66, 18, 9, 6, 10, 19, 38, 9, 20, 29, 12, 5, 17, 258, 0, 10, 31, 22, 183, 219, 3, 7, 644, 10, 94, 476, 130, 1, 9, 9, 1, 90, 6, 2, 12, 29, 1, 249, 50, 25, 1, 10, 33 }; // NW 13thOct2010
                //double[] v = { 1, 4, 2, 1, 7, 6, 14, 1, 4, 20, 36, 35, 3, 26, 4, 48, 235, 15, 52, 68, 24, 31, 7, 12, 2, 49, 60, 6, 1, 11, 12, 1, 51, 1, 282, 0, 0, 0, 8, 58, 201, 315, 3, 363, 20, 266, 506, 124, 2, 94, 2, 3, 24, 251, 53, 37, 6, 27 }; // NW 14thOct2010
                //double[] v = { 6, 5, 30, 24, 21, 111, 6, 52, 20, 68, 74, 1, 45, 2, 11, 644, 184, 32, 12, 32, 9, 39, 120, 100, 11, 30, 1, 77, 463, 12, 2, 11, 6, 73, 150, 12, 164, 132, 7, 393, 1, 946, 178, 93, 41, 15, 13, 8, 33, 520, 1, 2, 44, 1, 15, 15, 343, 10, 243, 94, 126 }; // SE 13thOct2010
                //double[] v = { 3, 7, 3, 1, 35, 34, 43, 10, 50, 3, 39, 54, 11, 22, 2, 650, 91, 20, 4, 21, 11, 17, 97, 106, 10, 1, 1, 3, 7, 389, 11, 7, 17, 42, 123, 5, 157, 174, 8, 323, 646, 135, 83, 15, 12, 15, 535, 8, 19, 3, 8, 1, 248, 11, 171, 59, 103 }; // SE 14thOct2010
                //double[] v = { 7, 18, 1, 7, 66, 1, 157, 6, 30, 28, 83, 2, 15, 29, 19, 323, 1, 56, 4, 31, 10, 1, 6, 10, 152, 13, 36, 1, 30, 1, 10, 15, 1, 333, 141, 7, 47, 1315, 2, 113, 723, 1, 33, 16, 1, 20, 2, 182, 186, 27, 20, 2, 8, 8, 18, 4, 194, 6, 105, 11, 109 }; // SW 13thOct2010
                //double[] v = { 1,4,19,6,14,45,89,2,3,24,59,3,5,4,74,19,443,1,31,9,17,9,1,3,4,150,44,2,2,47,1,3,1,20,1,22,3,397,151,19,63,810,4,114,969,2,25,34,4,2,19,202,255,8,10,1,249,9,137,10,157}; // SW 14thOct2010
                // the following lines are call counts per minute over all species.
                //double[] v = { 0,0,1,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,1,1,0,1,4,4,3,4,2,2,2,2,3,2,2,5,3,4,5,4,4,6,6,5,8,11,10,10,10,11,11,10,11,13,12,11,12,8,8,6,8,6,7,8,9,4,7,7,5,7,8,10,7,11,9,10,8,6,7,7,9,13,8,9,8,9,9,10,7,9,11,8,8,7,8,7,10,8,8,9,7,6,5,6,5,7,9,9,7,6,8,11,10,6,7,6,7,7,8,7,6,9,10,7,9,6,7,8,7,8,8,5,5,5,8,7,9,9,9,7,9,7,8,9,9,9,9,8,7,7,7,6,8,8,6,8,10,8,9,10,9,10,12,8,7,7,5,6,4,6,7,9,9,11,7,9,11,10,9,9,10,10,10,10,8,9,7,11,10,11,5,7,9,6,9,12,9,7,10,7,9,9,7,6,6,7,7,8,10,8,8,4,8,9,11,8,5,4,4,5,7,4,7,7,9,12,9,9,8,7,6,7,8,7,8,5,11,7,6,4,7,7,9,9,8,8,9,9,5,7,7,4,7,7,5,10,6,8,6,9,5,3,5,5,6,6,7,5,8,11,11,7,10,8,11,10,10,7,10,6,8,7,1,4,6,9,9,9,7,3,3,2,4,7,4,6,8,7,5,9,9,6,9,8,8,10,11,7,11,9,7,7,5,8,9,13,10,10,6,7,6,4,6,5,8,2,3,1,4,3,3,6,5,4,5,7,9,4,6,5,7,3,5,4,6,5,3,4,6,4,7,7,6,6,4,5,5,2,3,4,4,8,7,6,5,6,5,5,7,8,8,6,6,6,7,6,4,4,5,6,6,3,3,2,5,4,6,3,4,4,5,4,4,7,7,5,3,5,5,3,6,4,2,3,2,4,4,3,4,4,6,4,4,4,4,4,3,1,4,5,3,3,4,5,6,3,1,4,3,7,5,6,4,3,1,4,2,3,4,3,4,4,3,3,5,3,6,6,6,3,6,9,11,5,6,9,8,6,4,5,4,4,4,3,3,4,4,4,6,3,0,6,7,6,7,7,5,5,7,6,8,6,8,10,9,7,5,6,5,6,5,4,5,5,4,2,7,5,5,9,9,5,4,6,1,0,1,1,3,1,3,1,3,8,3,6,5,7,7,7,6,8,6,3,6,6,5,6,8,6,6,6,5,5,5,3,3,3,5,8,9,5,5,6,5,6,5,11,10,8,6,7,3,2,2,3,4,4,4,1,1,2,4,2,3,3,4,4,6,2,2,3,9,3,5,5,7,4,5,4,4,4,4,6,5,7,4,8,8,5,9,3,4,5,4,6,6,7,6,5,8,6,4,3,6,5,5,6,4,7,11,11,12,10,10,7,6,8,5,5,3,6,3,3,5,4,5,7,8,9,5,5,4,6,2,3,5,8,7,3,6,5,3,4,6,4,4,5,5,3,3,3,3,5,5,3,2,3,3,5,2,1,6,6,5,3,2,4,2,7,9,9,6,5,7,5,5,7,8,7,7,8,6,3,6,6,3,4,2,3,2,1,3,8,4,6,6,7,5,5,3,5,5,3,3,3,3,5,6,7,4,2,3,2,4,7,7,3,4,2,2,4,7,5,6,3,4,4,3,4,3,5,6,5,6,6,4,5,5,1,3,3,3,4,3,5,5,3,3,5,6,5,6,6,5,4,5,4,5,8,5,8,5,6,7,4,3,3,5,3,4,5,7,5,6,6,7,3,3,4,5,3,6,3,3,1,3,1,5,3,3,0,2,4,3,6,5,4,5,5,6,5,5,6,5,5,5,3,2,6,5,4,4,4,4,3,4,6,4,3,5,9,4,8,3,5,4,1,3,4,3,2,2,5,1,2,3,4,5,5,4,4,3,2,3,3,2,2,3,3,2,1,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }; // SW 13thOct2010
                double[] v = { 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 2, 0, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 4, 3, 6, 4, 3, 3, 4, 4, 4, 4, 3, 4, 3, 3, 1, 7, 5, 5, 5, 8, 7, 6, 7, 8, 8, 7, 7, 7, 7, 7, 6, 8, 8, 9, 7, 13, 8, 10, 10, 6, 11, 6, 8, 7, 7, 9, 6, 8, 8, 7, 4, 8, 4, 4, 4, 6, 7, 11, 8, 8, 6, 5, 4, 5, 6, 9, 6, 8, 9, 4, 2, 5, 3, 5, 3, 4, 8, 8, 8, 9, 7, 8, 8, 7, 5, 5, 6, 4, 7, 9, 6, 5, 2, 6, 9, 10, 8, 5, 7, 8, 7, 7, 4, 9, 8, 7, 12, 9, 10, 14, 12, 10, 9, 12, 8, 9, 9, 7, 9, 9, 5, 6, 7, 10, 10, 5, 7, 8, 7, 6, 6, 6, 7, 4, 3, 7, 8, 6, 5, 5, 7, 5, 7, 5, 6, 6, 7, 7, 10, 8, 5, 4, 6, 9, 6, 9, 8, 5, 6, 4, 8, 10, 8, 7, 7, 6, 6, 6, 5, 6, 5, 4, 8, 7, 6, 6, 5, 6, 7, 7, 5, 5, 6, 6, 7, 8, 8, 7, 6, 5, 4, 4, 4, 4, 3, 5, 6, 7, 9, 8, 6, 6, 4, 7, 4, 3, 6, 7, 4, 7, 6, 3, 8, 5, 6, 6, 5, 4, 6, 5, 7, 4, 4, 5, 6, 7, 5, 9, 7, 4, 6, 7, 6, 5, 4, 7, 4, 4, 8, 8, 3, 6, 5, 5, 4, 5, 4, 4, 4, 5, 7, 8, 7, 6, 7, 3, 2, 4, 7, 9, 7, 7, 6, 6, 6, 4, 5, 3, 3, 3, 3, 7, 6, 5, 4, 4, 3, 4, 6, 5, 2, 3, 2, 5, 2, 3, 1, 3, 2, 5, 3, 4, 5, 6, 5, 7, 3, 8, 6, 2, 5, 5, 5, 3, 2, 4, 2, 2, 3, 4, 1, 2, 1, 2, 0, 1, 3, 7, 5, 2, 3, 2, 2, 6, 3, 2, 2, 2, 5, 3, 4, 2, 4, 3, 2, 2, 4, 5, 3, 3, 2, 2, 3, 4, 2, 3, 5, 3, 4, 3, 3, 2, 3, 5, 3, 3, 1, 1, 2, 2, 2, 5, 4, 5, 3, 3, 2, 2, 2, 3, 3, 2, 3, 3, 2, 3, 2, 4, 2, 3, 6, 5, 1, 3, 2, 2, 5, 4, 5, 2, 4, 5, 2, 1, 1, 2, 4, 4, 0, 3, 4, 4, 2, 1, 2, 0, 0, 0, 1, 0, 5, 3, 5, 5, 6, 3, 4, 2, 2, 4, 4, 5, 3, 2, 3, 1, 0, 0, 2, 2, 3, 4, 5, 5, 5, 4, 3, 4, 2, 4, 3, 3, 4, 4, 1, 3, 4, 6, 2, 3, 4, 2, 4, 2, 5, 3, 3, 5, 1, 4, 2, 5, 4, 2, 4, 5, 2, 2, 3, 3, 2, 4, 3, 5, 6, 7, 4, 4, 4, 4, 3, 6, 4, 3, 5, 3, 5, 7, 6, 5, 4, 7, 2, 2, 4, 4, 4, 4, 4, 2, 2, 2, 4, 4, 4, 4, 4, 3, 3, 4, 5, 4, 3, 3, 3, 2, 4, 5, 3, 4, 4, 4, 3, 1, 3, 3, 1, 2, 4, 4, 2, 3, 3, 5, 5, 3, 3, 2, 4, 3, 3, 4, 5, 5, 6, 6, 4, 5, 2, 2, 2, 5, 7, 2, 4, 3, 4, 3, 5, 3, 2, 2, 2, 2, 3, 5, 3, 5, 4, 4, 4, 3, 3, 3, 1, 3, 5, 5, 4, 4, 2, 3, 1, 1, 4, 5, 2, 2, 3, 4, 3, 2, 3, 4, 6, 5, 3, 1, 2, 3, 3, 1, 0, 2, 1, 5, 2, 1, 1, 3, 3, 1, 2, 2, 5, 2, 4, 1, 1, 2, 2, 2, 5, 5, 3, 1, 1, 0, 1, 0, 3, 0, 1, 1, 2, 2, 0, 2, 3, 4, 3, 2, 1, 3, 1, 1, 1, 3, 1, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2, 5, 2, 3, 2, 2, 2, 2, 3, 3, 1, 2, 3, 2, 4, 3, 2, 2, 1, 1, 3, 4, 4, 3, 1, 1, 2, 3, 3, 2, 3, 4, 4, 3, 4, 4, 3, 4, 6, 4, 4, 6, 7, 8, 4, 4, 6, 6, 4, 4, 6, 3, 4, 4, 1, 4, 1, 1, 2, 6, 3, 3, 3, 1, 3, 7, 3, 3, 4, 2, 4, 3, 2, 3, 4, 4, 4, 5, 4, 4, 4, 5, 3, 3, 3, 4, 4, 3, 6, 4, 4, 4, 6, 4, 4, 6, 3, 2, 5, 1, 1, 1, 3, 0, 1, 3, 2, 5, 2, 3, 6, 4, 4, 4, 4, 3, 3, 4, 2, 2, 3, 4, 3, 3, 2, 4, 3, 2, 3, 3, 3, 3, 3, 1, 1, 2, 1, 2, 1, 2, 3, 1, 0, 1, 0, 0, 1, 1, 2, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // NW 13thOct2010
                double entropy = DataTools.Entropy_normalised(v);
            } // end if (true)




            // code to merge all files of acoustic indeces derived from 24 hours of recording,
            if (false)
            {
                //LDSpectrogramStitching.ConcatenateSpectralIndexFiles1(); //DEPRACATED
                //LDSpectrogramStitching.ConcatenateSpectralIndexImages();
                //LDSpectrogramClusters.ExtractSOMClusters();
            } // end if (true)


            // PAPUA NEW GUINEA DATA
            // concatenating csv files of spectral and summary indices
            if (false)
            {
                // top level directory
                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_32\";
                //string opFileStem = "TNC_Iwarame_20150704_BAR32";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_33\";
                //string opFileStem = "TNC_Iwarame_20150704_BAR33";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_35\";
                //string opFileStem = "TNC_Iwarame_20150704_BAR35";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_7-7-15\BAR\BAR_59\";
                //string opFileStem = "TNC_Iwarame_20150707_BAR59";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_9-7-15\BAR\BAR_79\";
                //string opFileStem = "TNC_Iwarame_20150709_BAR79";

                //string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Yavera_8-7-15\BAR\BAR_64\";
                //string opFileStem = "TNC_Yavera_20150708_BAR64";

                string dataPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Musiamunat_3-7-15\BAR\BAR_18\";
                string opFileStem = "TNC_Musiamunat_20150703_BAR18";


                DirectoryInfo[] dataDir = { new DirectoryInfo(dataPath) };

                string indexPropertiesConfigPath = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\IndexPropertiesOLDConfig.yml";
                FileInfo indexPropertiesConfigFileInfo = new FileInfo(indexPropertiesConfigPath);

                // string outputDirectory = @"C:\SensorNetworks\Output\Test\TNC";
                var opDir = new DirectoryInfo(dataPath);
                LDSpectrogramStitching.ConcatenateAllIndexFiles(dataDir, indexPropertiesConfigFileInfo, opDir, opFileStem);

            }


            // testing TERNARY PLOTS using spectral indices
            if (false)
            {
                string[] keys = { "ACI", "ENT", "EVN" };
                //string[] keys = { "BGN", "POW", "EVN"};

                FileInfo[] indexFiles = { new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[0]+".csv"),
                                          new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[1]+".csv"),
                                          new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622__"+keys[2]+".csv")
                };
                FileInfo opImage = new FileInfo(@"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults\GympieNP\20150622\GympieNP_20150622_TernaryPlot.png");

                var matrixDictionary = IndexMatrices.ReadSummaryIndexFiles(indexFiles, keys);

                string indexPropertiesConfigPath = @"Y:\Results\YvonneResults\Cooloola_ConcatenatedResults" + @"\IndexPropertiesConfig.yml";
                FileInfo indexPropertiesConfigFileInfo = new FileInfo(indexPropertiesConfigPath);
                Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfigFileInfo);
                dictIP = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIP);

                foreach (string key in keys)
                {
                    IndexProperties indexProperties = dictIP[key];
                    double min = indexProperties.NormMin;
                    double max = indexProperties.NormMax;
                    matrixDictionary[key] = MatrixTools.NormaliseInZeroOne(matrixDictionary[key], min, max);
                    //matrix = MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter); // to de-demphasize the background small values
                }
                Image image = TernaryPlots.DrawTernaryPlot(matrixDictionary, keys);
                image.Save(opImage.FullName);
            }




            // testing directory search and file search 
            if (false)
            {
                string[] topLevelDirs =
                {
                    @"C:\temp\DirA",
                    @"C:\temp\DirB"
                };

                string sitePattern = "Subdir2";
                string dayPattern  = "F2*.txt";

                List<string> dirList = new List<string>();
                foreach (string dir in topLevelDirs)
                {
                    string[] dirs = Directory.GetDirectories(dir, sitePattern, SearchOption.AllDirectories);
                    dirList.AddRange(dirs);
                }

                List<FileInfo> fileList = new List<FileInfo>();
                foreach (string subdir in topLevelDirs)
                {
                    var files = IndexMatrices.GetFilesInDirectory(subdir, dayPattern);

                    fileList.AddRange(files);
                }

                Console.WriteLine("The number of directories is {0}.", dirList.Count);
                foreach (string dir in dirList)
                {

                    Console.WriteLine(dir);
                }

                Console.WriteLine("The number of files is {0}.", fileList.Count);
                foreach (FileInfo file in fileList)
                {

                    Console.WriteLine(file.FullName);
                }
            }

            // experiments with clustering the spectra within spectrograms
            if (false)
            {
                SpectralClustering.Sandpit();
            } // end if (true)


            // experiments with false colour images - categorising/discretising the colours
            if (false)
            {
                LDSpectrogramDiscreteColour.DiscreteColourSpectrograms();
            }


            // experiments with false colour images - categorising/discretising the colours
            if (false)
            {

                DirectoryInfo[] dataDirs = { new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013March\CornellMarine"),
                                             new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms\LdFcSpectrograms2013April\CornellMarine")
                                           };

                DirectoryInfo outputDirectory = new DirectoryInfo(@"C:\SensorNetworks\Output\MarineSonograms");
                string title = "Marine Spectrograms - off Georgia Coast, USA - Day 1= 01/March/2013";
                //indexPropertiesConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesMarineConfig.yml");

                //string match = @"CornellMarine_*__ACI-ENT-EVN.SpectralRibbon.png";
                //string opFileStem = "CornellMarine.ACI-ENT-EVN.SpectralRibbon.2013MarchApril";

                string match = @"CornellMarine_*__BGN-POW-EVN.SpectralRibbon.png";
                string opFileStem = "CornellMarine.BGN-POW-EVN.SpectralRibbon.2013MarchApril";

                ConcatenateIndexFiles.ConcatenateRibbonImages(dataDirs, match, outputDirectory, opFileStem, title);
            }


<<<<<<< 6b3abed651991a0d50c957404cadec0b5147cc86
            Console.WriteLine("# Finished!");
=======
            // Concatenate three images for Dan Stowell.
            if (false)  // 
            {
                var imageDirectory = new DirectoryInfo(@"H:\Documents\SensorNetworks\MyPapers\2016_QMUL_SchoolMagazine");
                string fileName1 = @"TNC_Musiamunat_20150702_BAR10__ACI-ENT-EVNCropped.png";
                string fileName2 = @"GympieNP_20150701__ACI-ENT-EVN.png";
                string fileName3 = @"Sturt-Mistletoe_20150702__ACI-ENT-EVN - Corrected.png";
                var image1Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName1));
                var image2Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName2));
                var image3Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName3));

                var imageList = new List<Image>();


                imageList.Add(Bitmap.FromFile(image1Path.FullName));
                imageList.Add(Bitmap.FromFile(image2Path.FullName));
                imageList.Add(Bitmap.FromFile(image3Path.FullName));

                Image combinedImage = ImageTools.CombineImagesVertically(imageList);

                string fileName = String.Format("ThreeLongDurationSpectrograms.png");
                combinedImage.Save(Path.Combine(imageDirectory.FullName, fileName));
            }

            // Concatenate two images but add labels for EcoCongress.
            if (false)  // 
            {
                //var imageDirectory = new DirectoryInfo(@"H:\Documents\SensorNetworks\MyPapers\2016_QMUL_SchoolMagazine");
                //string fileName1 = @"TNC_Musiamunat_20150702_BAR10__ACI-ENT-EVNCropped.png";
                //string fileName2 = @"GympieNP_20150701__ACI-ENT-EVN.png";
                //string fileName3 = @"Sturt-Mistletoe_20150702__ACI-ENT-EVN - Corrected.png";
                //string fileName1 = @"TNC_Musiamunat_20150702_BAR10__ACI-ENT-EVNCropped.png";
                //string fileName2 = @"GympieNP_20150701__ACI-ENT-EVN.png";

                //var imageDirectory = new DirectoryInfo(@"C:\Users\Owner\Documents\QUT\SensorNetworks\MyPapers\2016_EcoacousticsCongress\24hour-2mapSpectrograms");
                //string fileName1 = @"NW_12140a87_101013-0000.ACI-ENT-EVN.png";
                //string fileName2 = @"SW_f8c71440_101013-0000.ACI-ENT-EVN.png";
                //string fileName1 = @"NW_6905bee9_101014-0000.ACI-ENT-EVN.png";
                //string fileName2 = @"SW_e8abdd2a_101014-0000.ACI-ENT-EVN.png";


                var imageDirectory = new DirectoryInfo(@"C:\Users\Owner\Documents\QUT\SensorNetworks\MyPapers\2016_EcoacousticsCongress\DotPlotsInColour");
                string fileName1 = @"colour_dot_plot_NW_13Oct.png";
                string fileName2 = @"colour_dot_plot_SW_13Oct.png";
                string name1 = "Closed canopy, dense Eucalyptus forest (13th Oct 2010)";
                string name2 = "Open Melaleuca paperbark forest (13th Oct 2010)";

                var opDirectory = new DirectoryInfo(@"C:\Users\Owner\Documents\QUT\SensorNetworks\MyPapers\2016_EcoacousticsCongress");
                string opFileName = String.Format("Comparison SERF DotPlots NWandSW 2010Oct13.png");


                var image1Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName1));
                var image2Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName2));
                //var image3Path = new FileInfo(Path.Combine(imageDirectory.FullName, fileName3));

                Image image1 = Bitmap.FromFile(image1Path.FullName);
                int width = image1.Width;
                int height = 40;

                Brush brush = Brushes.Black;
                Font stringFont = new Font("Tahoma", 20);

                Image title1 = new Bitmap(width, height);
                Graphics g1 = Graphics.FromImage(title1);
                g1.Clear(Color.LightGray);
                g1.DrawString(name1, stringFont, brush, new PointF(5, 5));


                //Graphics g = Graphics.FromImage(image1);
                //g.DrawImage(title1, 0, 0);

                Image title2 = new Bitmap(width, height);
                Graphics g2 = Graphics.FromImage(title2);
                g2.Clear(Color.LightGray);
                g2.DrawString(name2, stringFont, brush, new PointF(5, 5));
                //g.DrawImage(title2, 0, 0);

                var imageList = new List<Image>();
                //imageList.Add(title1);
                imageList.Add(image1);
                //imageList.Add(title2);
                imageList.Add(Bitmap.FromFile(image2Path.FullName));

                //Image combinedImage = ImageTools.CombineImagesVertically(imageList);
                Image combinedImage = ImageTools.CombineImagesInLine(imageList);
                combinedImage.Save(Path.Combine(opDirectory.FullName, opFileName));
            }


            // Concatenate twelve images for Simon and Toby
            if (false)  // 
            {
                var imageDirectory = new DirectoryInfo(@"F:\AvailaeFolders\Griffith\Toby\20160201_FWrecordings\Site1Images");
                var imageFiles = imageDirectory.GetFiles();
                var imageList = new List<Image>();

                foreach (FileInfo file in imageFiles)
                {
                    imageList.Add(Bitmap.FromFile(file.FullName));
                }

                Image combinedImage = ImageTools.CombineImagesInLine(imageList);

                string fileName = String.Format("Site1.png");
                combinedImage.Save(Path.Combine(imageDirectory.FullName, fileName));
            }





            // Concatenate images for Karl-Heinz Frommolt
            if (false)  // 
            {
                FrommoltProject.ConcatenateDays();
            }




            //HERVE GLOTIN: This is used to analyse the BIRD50 data set.
            // Combined audio2csv + zooming spectrogram task.
            // 
            if (false)
            {
                HerveGlotinCollaboration.HiRes1();

            }  // END combined audio2csv + zooming spectrogram task.


            //HERVE GLOTIN: This is used to analyse the BIRD50 data set.
            // To produce HIres spectrogram images
            // 
            if (false)
            {
                HerveGlotinCollaboration.HiRes2();

            }  // END 


            //HERVE GLOTIN: This is used to analyse the BIRD50 data set.
            // In order to analyse the short recordings in BIRD50 dataset, need following change to code:
            // need to modify    AudioAnalysis.AnalysisPrograms.AcousticIndices.cs #line648
            // need to change    SegmentMinDuration = TimeSpan.FromSeconds(20),  
            // to                SegmentMinDuration = TimeSpan.FromSeconds(1),
            if (false)
            {
                HerveGlotinCollaboration.HiRes3();

            }  // END 


            //HERVE GLOTIN: To produce HIres spectrogram images
            // This is used to analyse Herve Glotin's BIRD50 data set.
            //   Joins images of the same species
            if (false)
            {
                HerveGlotinCollaboration.HiRes4();

            }  // END 



            //HERVE GLOTIN
            // To produce observe feature spectra or SPECTRAL FEATURE TEMPLATES for each species
            // This is used to analyse Herve Glotin's BIRD50 data set.
            if (false)
            {
                BirdClefExperiment1.Execute(null);
            } // Herve Glotin's BIRD50 Dataset

            
            //FROG DATA SET
            // To produce observe feature spectra
            // This is used to analyse frog recordings of Lin Schwarzkopf.
            if (false)
            {
                HighResolutionAcousticIndices.Execute(null);
            } 

            //OTSU TRHESHOLDING FROM JIE XIE
            // Used to threshold spectrograms to binary.
            //  Jie uses the algorithm in his last 2016 papers. 
            // 
            if (false)
            {
                OtsuThresholder.Execute(null);
            } 

            if (false)
            {
                HerveGlotinCollaboration.AnalyseBOMBYXRecordingsForSpermWhaleClicks();

            } 
            


            // To CALCULATE MUTUAL INFORMATION BETWEEN SPECIES DISTRIBUTION AND FREQUENCY INFO
            // This method calculates a seperate value of MI for each frequency bin
            // See the next method for single value of MI that incorporates all freq bins combined.
            if (false)
            {
                // set up IP and OP directories
                string parentDir = @"C:\SensorNetworks\Output\BIRD50";
                string key = "RHZ";  //"RHZ";
                int valueResolution = 6;
                string miFileName = parentDir + @"\MutualInformation."+ valueResolution + "catNoSkew." + key + ".txt";
                //double[] bounds = { 0.0, 3.0, 6.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 8.0 };
                double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew
                //double[] bounds = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 10.0 };
                //double[] bounds = { 0.0, 1.0, 2.0, 4.0, 6.0, 10.0 }; // skew left
                //double[] bounds = { 0.0, 2.0, 4.0, 5.0, 6.0, 8.0 }; // skew centre
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew

                string inputDir = parentDir + @"\TrainingCSV";
                int speciesNumber = 50;

                string speciesCountFile = parentDir + @"\AmazonBird50_training_Counts.txt"; //
                var lines = FileTools.ReadTextFile(speciesCountFile);
                int[] speciesCounts = new int[speciesNumber];
                for (int i = 0; i < speciesNumber; i++)
                {
                    string[] words = lines[i].Split(','); 
                    speciesCounts[i] = Int32.Parse(words[1]);
                }
                double Hspecies = DataTools.Entropy_normalised(speciesCounts);
                Console.WriteLine("Species Entropy = " + Hspecies);

                int freqBinCount = 256;
                int reducedBinCount = freqBinCount;
                //int reductionFactor = 1;
                //reducedBinCount = freqBinCount / reductionFactor;
                reducedBinCount = 100 + (156 / 2); // exotic style
                // data structure to contain probability info
                int[,,] probSgivenF = new int[reducedBinCount, speciesNumber, valueResolution];

                DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
                string pattern = "*." + key + ".csv";
                FileInfo[] filePaths = inputDirInfo.GetFiles(pattern);

                // read through all the files
                int fileCount = filePaths.Length;
                //fileCount = 3;
                for (int i = 0; i < fileCount; i++)
                {
                    //ID0001_Species01.EVN.csv
                    char[] delimiters = { '.', 's' };
                    string fileName = filePaths[i].Name;
                    string[] parts = fileName.Split(delimiters);
                    int speciesID = Int32.Parse(parts[1]);
                    double[,] matrix = null;
                    if (filePaths[i].Exists)
                    {
                        int binCount;
                        matrix = IndexMatrices.ReadSpectrogram(filePaths[i], out binCount);

                        // column reduce the matrix
                        // try max pooling
                        //matrix = Sandpit.MaxPoolMatrixColumns(matrix, reducedBinCount);
                        //matrix = Sandpit.MaxPoolMatrixColumnsByFactor(matrix, reductionFactor);
                        matrix = BirdClefExperiment1.ExoticMaxPoolingMatrixColumns(matrix, reducedBinCount);
                    }

                    Console.WriteLine("Species ID = " + speciesID);

                    int rowCount = matrix.GetLength(0);
                    reducedBinCount = matrix.GetLength(1);

                    // calculate the conditional probabilities
                    // set up data structure to contain probability info
                    for (int r = 0; r < rowCount; r++)
                    {
                        var rowVector = MatrixTools.GetRow(matrix, r);
                        for (int c = 0; c < reducedBinCount; c++)
                        {
                            int valueCategory = 0;
                            for (int bound = 1; bound < bounds.Length; bound++)
                            {
                                if (rowVector[c] > bounds[bound]) valueCategory = bound;
                            }
                            probSgivenF[c, speciesID-1, valueCategory] ++;
                        }
                    }
                }

                // now process info in probabilities in data structure
                double[] mi = new double[reducedBinCount];
                for (int i = 0; i < reducedBinCount; i++)
                {
                    var m = new double[speciesNumber, valueResolution];


                    for (int r = 0; r < speciesNumber; r++)
                    {
                        for (int c = 0; c < valueResolution; c++)
                        {
                            m[r, c] = probSgivenF[i, r, c];
                        }
                    }
                    double[]  array = DataTools.Matrix2Array(m);
                    double entropy = DataTools.Entropy_normalised(array);
                    mi[i] = entropy;
                }

                for (int i = 0; i < reducedBinCount; i++)
                {
                    Console.WriteLine(String.Format("Bin{0}  {1}", i, mi[i]));
                }
                FileTools.WriteArray2File(mi, miFileName);

            } // CALCULATE MUTUAL INFORMATION



            // test 3-D matrix to array
            if (false)
            {
                double value = 0;
                var M3d = new double[2,2,2];
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            M3d[i, j, k] = value;
                            value += 1.0;
                        }
                    }
                }
                double[] array = DataTools.Matrix2Array(M3d);
            }


            // test MUTUAL INFORMATION
            if (false)
            {
                var M = new int[4, 4];
                M[0, 0] = 4; M[0, 1] = 2; M[0, 2] = 1; M[0, 3] = 1;
                M[1, 0] = 2; M[1, 1] = 4; M[1, 2] = 1; M[1, 3] = 1;
                M[2, 0] = 2; M[2, 1] = 2; M[2, 2] = 2; M[2, 3] = 2;
                M[3, 0] = 8; M[3, 1] = 0; M[3, 2] = 0; M[3, 3] = 0;
                double MI = DataTools.MutualInformation(M);
            }


            // EXPERIMENTS WITH HERVE
            // To CALCULATE MUTUAL INFORMATION BETWEEN SPECIES DISTRIBUTION AND FREQUENCY INFO
            // this method calculates a single MI value for the entire frequency band
            if (false)
            {
                // set up IP and OP directories
                string parentDir = @"C:\SensorNetworks\Output\BIRD50";
                string key = "POW";  //"RHZ";
                int valueCategoryCount = 10;
                string miFileName = parentDir + @"\MutualInformation." + valueCategoryCount + "cats." + key + ".txt";

                //double[] bounds = { 5.0 };
                //double[] bounds = { 0.0, 4.0, 6.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 8.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew
                //double[] bounds = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 10.0 };
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0 };
                double[] bounds = { 0.0, 4.0, 8.0, 12.0, 16.0, 20.0, 24.0, 28.0, 32.0, 36.0 };
                //double[] bounds = { 0.0, 1.0, 2.0, 4.0, 6.0, 10.0 }; // skew left
                //double[] bounds = { 0.0, 2.0, 4.0, 5.0, 6.0, 8.0 }; // skew centre
                //double[] bounds = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0 }; // noSkew

                string inputDir = parentDir + @"\TrainingCSV";

                // read Herve's file of metadata
                int speciesNumber = 50;
                string speciesCountFile = parentDir + @"\AmazonBird50_training_Counts.txt"; //
                var lines = FileTools.ReadTextFile(speciesCountFile);
                int[] speciesCounts = new int[speciesNumber];
                for (int i = 0; i < speciesNumber; i++)
                {
                    string[] words = lines[i].Split(',');
                    speciesCounts[i] = Int32.Parse(words[1]);
                }
                double Hspecies = DataTools.Entropy_normalised(speciesCounts);
                Console.WriteLine("Species Entropy = " + Hspecies);


                // set up the input data
                int freqBinCount = 256;
                int reducedBinCount = freqBinCount;

                // standard matrix reduction
                int minBin = 9;
                //int maxBin = 233;
                int maxBin = 218;
                reducedBinCount = maxBin - minBin + 1;

                // frequency bins used to reduce dimensionality of the 256 spectral values.
                //int startBin = 8;
                //int maxOf2Bin = 117;
                //int maxOf3Bin = 182;
                //int endBin = 234;
                //double[] testArray = new double[256];
                ////for (int i = 0; i < testArray.Length; i++) testArray[i] = i;
                //double[] reducedArray = Sandpit.MaxPoolingLimited(testArray, startBin, maxOf2Bin, maxOf3Bin, endBin);
                //int reducedBinCount = reducedArray.Length;

                // other ways to reduce the spectrum length
                //int reductionFactor = 1;
                //reducedBinCount = freqBinCount / reductionFactor;
                //reducedBinCount = 100 + (156 / 2); // exotic style


                // Length of the Input feature vector 
                int featureVectorLength = reducedBinCount * valueCategoryCount;

                // data structure to contain probability info
                //int[,,] probSgivenF = new int[reducedBinCount, speciesNumber, valueResolution];
                int[,] probSgivenF = new int[featureVectorLength, speciesNumber];
                int[] decibelDistribution = new int[100];

                DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
                string pattern = "*." + key + ".csv";
                FileInfo[] filePaths = inputDirInfo.GetFiles(pattern);

                // read through all the files
                int fileCount = filePaths.Length;
                //fileCount = 3;
                for (int i = 0; i < fileCount; i++)
                {
                    //ID0001_Species01.EVN.csv
                    char[] delimiters = { '.', 's' };
                    string fileName = filePaths[i].Name;
                    string[] parts = fileName.Split(delimiters);
                    int speciesID = Int32.Parse(parts[1]);
                    //Console.WriteLine("Species ID = " + speciesID);
                    // show user something is happening
                    Console.Write(".");

                    double[,] matrix = null;
                    if (filePaths[i].Exists)
                    {
                        int binCount;
                        matrix = IndexMatrices.ReadSpectrogram(filePaths[i], out binCount);

                        // column reduce the matrix
                        matrix = BirdClefExperiment1.ReduceMatrixColumns(matrix, minBin, maxBin);

                        // try max pooling
                        //matrix = Sandpit.MaxPoolingLimited(matrix, startBin, maxOf2Bin, maxOf3Bin, endBin, reducedBinCount);
                        //matrix = Sandpit.MaxPoolMatrixColumns(matrix, reducedBinCount);
                        //matrix = Sandpit.MaxPoolMatrixColumnsByFactor(matrix, reductionFactor);
                        //matrix = Sandpit.ExoticMaxPoolingMatrixColumns(matrix, reducedBinCount);
                    }

                    int rowCount = matrix.GetLength(0);
                    reducedBinCount = matrix.GetLength(1);


                    // calculate the conditional probabilities
                    // set up data structure to contain probability info
                    //int threshold = 0;
                    for (int r = 0; r < rowCount; r++) // for all time
                    {
                        var rowVector = MatrixTools.GetRow(matrix, r);
                        for (int c = 0; c < reducedBinCount; c++) // for all freq bins
                        {
                            double dBvalue = rowVector[c];
                            decibelDistribution[(int)Math.Floor(dBvalue)]++;

                            // use this line when have only a single binary variable
                            //if (dBvalue > threshold)
                            //{
                            //    probSgivenF[c, speciesID - 1] ++;
                            //    decibelDistribution[dBvalue]++;
                            //}
                            

                            // use next six lines when variable can have >=3 discrete values 
                            int valueCategory = 0;
                            for (int bound = 1; bound < bounds.Length; bound++)
                            {
                                if (dBvalue > bounds[bound]) valueCategory = bound;
                            }
                            int newIndex = (valueCategory * reducedBinCount) + c;
                            probSgivenF[newIndex, speciesID - 1]++;
                        }
                    }
                } // over all files

                // Now have the entire data in one structure.
                // Next process inf// in probabilities in data structure
                //int[] array = DataTools.Matrix2Array(probSgivenF);
                //double entropy = DataTools.Entropy_normalised(array);
                double MI = DataTools.MutualInformation(probSgivenF);


                Console.WriteLine(String.Format("\n\nFeature {0};  Category Count {1}", key, valueCategoryCount));
                Console.WriteLine(String.Format("Mutual Info = {0}", MI));

                //for (int i = 0; i < decibelDistribution.Length; i++)
                //{
                //    Console.WriteLine(String.Format("dB{0}  {1}", i, decibelDistribution[i])); 
                //}
                double sum = decibelDistribution.Sum();
                Console.WriteLine(String.Format("Dist sum = {0}", sum));

                double threshold = sum / 2;
                double median = 0;
                int medianIndex = 0;
                for (int i = 0; i < decibelDistribution.Length; i++)
                {
                    median += decibelDistribution[i];
                    if (median >= threshold)
                    {
                        medianIndex = i;
                        break;
                    }
                }
                Console.WriteLine(String.Format("Median occurs at {0} ", medianIndex)); 

                //for (int i = 0; i < reducedBinCount; i++)
                //{
                //Console.WriteLine(String.Format("Bin{0}  {1}", i, mi[i]));
                //}
                //FileTools.WriteArray2File(mi, miFileName);

            } // CALCULATE MUTUAL INFORMATION




            Console.WriteLine("# Finished Sandpit Task!");
>>>>>>> Create new class to check audio-channel integrity
            Console.ReadLine();
            System.Environment.Exit(0);
        }





    }
}
