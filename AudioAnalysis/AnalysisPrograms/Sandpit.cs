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


            if (true)  // call SURF image Feature extraction
            {
                //SURFFeatures.SURF_TEST();
                SURFAnalysis.Main(null);
                LoggedConsole.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)  // do test of SNR calculation
            {
                Audio2InputForConvCNN.Main(null);
                //SNR.Calculate_SNR_ofXueyans_data();
                LoggedConsole.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }




            if (false)  // do test of new moving average method
            {
                DataTools.TEST_FilterMovingAverage();
                LoggedConsole.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)
            {
                ImageTools.TestCannyEdgeDetection();
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
                int spectralDim = 256;
                double[] spectralIndices = new double[spectralDim];
                for (int p = 0; p < spectralDim; p++)
                {
                    spectralIndices[p] = 1.0;
                }
                int maxdistance = 100;

                //half life in meters
                double halfLife = 30.0;
                Bitmap bmp = new Bitmap(maxdistance, spectralDim);

                double log2 = Math.Log(2.0);
                double tau = halfLife / log2;

                for (int d = 0; d < 100; d++)
                {
                    double[] array = LDSpectrogramRGB.CalculateDecayedSpectralIndices(spectralIndices, d, halfLife);
                    for (int p = 0; p < spectralDim; p++)
                    {
                        int colourIndex = (int)(255 * array[p]);
                        if (colourIndex > 255) colourIndex = 255;
                        else
                            if (colourIndex < 0) colourIndex = 0;
                        bmp.SetPixel(d, spectralDim - p - 1, Color.FromArgb(colourIndex, colourIndex, colourIndex));
                    }

                }
                bmp.Save(@"C:\SensorNetworks\Output\Test\Test1\decay.png");

                //for (int d = 0; d < 100; d++)
                //{
                //    double exponent = d / tau;
                //    Color c = bmp.GetPixel(d, spectralDim - 1);
                //    Console.WriteLine("d={0} array[0]={1}    {2}", d, (c.R / (double)255), Math.Pow(Math.E, -exponent));
                //}

                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            if (false)  // concatenating spectrogram images with gaps between them.
            {
                LDSpectrogramStitching.StitchPartialSpectrograms();

                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            }


            // code to merge all files of acoustic indeces derived from 24 hours of recording,
            // problem is that Jason cuts them up into 6 hour blocks.
            if (false)
            {
                LDSpectrogramStitching.ConcatenateSpectralIndexFiles();
                Log.WriteLine("FINSIHED");
                Console.ReadLine();
                System.Environment.Exit(0);
            } // end if (true)



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

            Log.WriteLine("# Finished!");
        }


        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold, double[,] overlay)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            if ((poi != null) && (poi.Count > 0))
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            if (overlay != null)
            {
                var m = MatrixTools.ThresholdMatrix2Binary(overlay, 0.5);
                image.OverlayDiscreteColorMatrix(m);
            }
            return image.GetImage();
        } //DrawSonogram()

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, int[,] overlay)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.OverlayDiscreteColorMatrix(overlay);
            return image.GetImage();
        } //DrawSonogram()
    }
}
