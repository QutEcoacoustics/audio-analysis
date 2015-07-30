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
using AudioAnalysisTools.DSP;
using AudioAnalysisTools.WavTools;
using Acoustics.Shared;

namespace AnalysisPrograms
{
    using PowerArgs;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using System.Text.RegularExpressions;


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


            // code to merge all files of acoustic indeces derived from 24 hours of recording,
            // problem is that Jason cuts them up into 6 hour blocks.
            if (false)
            {
                LDSpectrogramStitching.ConcatenateSpectralIndexFiles1();
                //LDSpectrogramStitching.ConcatenateSpectralIndexImages();
                //LDSpectrogramClusters.ExtractSOMClusters();
            } // end if (true)



            // concatenating csv files of spectral indices
            if (true)
            {
                //string path = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_32\20150704_unnamed\20150704_132321_Day1.wav\Towsey.Acoustic";
                string path = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency\BAR\Iwarame_4-7-15\BAR\BAR_32\";
                var dataDir = new DirectoryInfo(path);

                string metadataDir = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency";
                // string outputDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013April01";
                // string outputDirectory = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency";
                var metaDataDir = new DirectoryInfo(metadataDir);

                string outputDirectory = @"C:\SensorNetworks\Output\Test\Test3";
                // string outputDirectory = @"C:\SensorNetworks\Output\SERF\SERFIndices_2013April01";
                // string outputDirectory = @"Y:\Results\2015Jul26-215038 - Eddie, Indices, ICD=60.0, #47\TheNatureConservency";
                var opDir = new DirectoryInfo(outputDirectory);

                // 20150704_173000_Day1__Towsey.Acoustic.ACI.csv
                string fileStem = "TNC_20150704";

                //string[] keys = {"ACI", "TEN", "CVR", "BGN", "AVG", "VAR"};
                string[] keys = {"ACI", "ENT", "EVN", "BGN", "POW"};
                var dictionary = IndexMatrices.GetSpectralIndexFilesAndConcatenate(path, keys);

                // get first file name and get the start time.
                string pattern = "*ACI.csv";
                FileInfo[] files = IndexMatrices.GetFilesInDirectory(path, pattern);

                pattern = @"2015\d\d\d\d_\d\d\d\d\d\d_";
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                Match m = r.Match(files[0].Name); // get name of first file

                Group group = m.Groups[0];
                string[] array = group.ToString().Split('_');
                int year = Int32.Parse(array[0].Substring(0, 4));
                int mnth = Int32.Parse(array[0].Substring(4, 2));
                int day  = Int32.Parse(array[0].Substring(6, 2));
                int hour = Int32.Parse(array[1].Substring(0, 2));
                int min  = Int32.Parse(array[1].Substring(2, 2));
                int sec  = Int32.Parse(array[1].Substring(4, 2));
                DateTimeOffset dto = new DateTimeOffset(year, mnth, day, hour, min, sec, TimeSpan.Zero);


                LDSpectrogramStitching.ConcatenateSpectralIndexFiles2(dataDir, metaDataDir, fileStem, dictionary, dto, opDir);
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

            Console.WriteLine("# Finished!");
            Console.ReadLine();
            System.Environment.Exit(0);
        }

    }
}
