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


            Console.WriteLine("# Finished!");
            Console.ReadLine();
            System.Environment.Exit(0);
        }





    }
}
