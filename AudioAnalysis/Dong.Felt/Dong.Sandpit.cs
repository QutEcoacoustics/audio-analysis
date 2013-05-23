namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Drawing;
    using TowseyLib;
    using AudioAnalysisTools;
    using System.Drawing.Imaging;

    class Dong
    {
        public const int RESAMPLE_RATE = 17640;

        public static void Dev(string[] args)
        {

            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());

            // experiments with Sobel ridge detector
            if (true)
            {
                // Read one specific file name/path 

                // with human beings
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage2\TestImage2.png")); 

                // just simple shapes
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage3\TestImage3.png")); 
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage5\TestImage5.png"));   still need to fix a tiny problem

                // real spectrogram
                var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM420036_min430Crows-1minute.wav-noiseReduction-1Klines.png"));
                
                //string outputPath = @"C:\Test recordings\Crows\Test\TestImage3\TestImage3-GaussianBlur-thre-7-sigma-1.0-SobelEdgeDetector-thre-0.15.png";
                string outputFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result";
                string imageFileName = "CannyEdgeDetector1.png";
                
                // read one specific recording
                string wavFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM4420036_min430Crows-1minute.wav";
                // Read a bunch of recordings  
                //string[] files = Directory.GetFiles(analysisSettings.SourceFile.FullName);

                //string imageFname = "test3.png";
                double magnitudeThreshold = 2.0; // of canny edge detector for getting the real edge
                //double intensityThreshold = 5.0; // dB

                var recording = new AudioRecording(wavFilePath);
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
                var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                Plot scores = null;
                double eventThreshold = 0.5; // dummy variable - not used
                List<AcousticEvent> list = null;
                Image image = DrawSonogram(spectrogram, scores, list, eventThreshold);
                string imagePath = Path.Combine(outputFilePath, imageFileName);
                image.Save(imagePath, ImageFormat.Png);

                Bitmap bmp = (Bitmap)image;

                double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);

                List<PointOfInterest> poiList = new List<PointOfInterest>();
                double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
                var timeScale = TimeSpan.FromTicks((long)(secondsScale * TimeSpan.TicksPerSecond));
                double herzScale = spectrogram.FBinWidth;
                double freqBinCount = spectrogram.Configuration.FreqBinCount;
                int ridgeLength = 9; // dimension of NxN matrix to use for ridge detection - must be odd number
                int halfLength = ridgeLength / 2;

                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                for (int r = halfLength; r < rows - halfLength; r++)
                {
                    for (int c = halfLength; c < cols - halfLength; c++)
                    {
                       double[,] magnitude; 
                       double[,] direction;
                       ImageAnalysisTools.CannyEdgeDetector(matrix, out magnitude, out direction);
                       if ( magnitude[r, c] > magnitudeThreshold)
                       {
                            Point point = new Point(c, r);
                            //var poi = new PointOfInterest(point);
                            TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                            double herz = (freqBinCount - r - 1) * herzScale;
                            var poi = new PointOfInterest(time, herz);
                            poi.Point = point;
                            poi.RidgeOrientation = direction[r, c];
                            poi.OrientationCategory = (int)Math.Round((direction[r, c] * 8) / Math.PI);
                            poi.RidgeMagnitude = magnitude[r, c];
                            poi.Intensity = matrix[r, c];
                            poi.TimeScale = timeScale;
                            poi.HerzScale = herzScale;
                            poiList.Add(poi);
                        }
                        //c++;
                    }
                    //r++;
                }

                //PointOfInterest.RemoveLowIntensityPOIs(poiList, intensityThreshold);

                PointOfInterest.PruneSingletons(poiList, rows, cols);
                //PointOfInterest.PruneDoublets(poiList, rows, cols);
                poiList = PointOfInterest.PruneAdjacentTracks(poiList, rows, cols);

                foreach (PointOfInterest poi in poiList)
                {
                    poi.DrawColor = Color.Crimson;
                    //bool multiPixel = false;
                    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                    poi.DrawOrientationPoint(bmp, (int)freqBinCount);

                    // draw local max
                    //poi.DrawColor = Color.Cyan;
                    //poi.DrawLocalMax(bmp, (int)freqBinCount);
                }
            } // experiments with Sobel ridge detector

            Log.WriteLine("# Finished!");
            Console.ReadLine();
            System.Environment.Exit(666);
        } // Dev()

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            if ((poi != null) && (poi.Count > 0))
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()


        // For sobel edge detector on the test image
        //var testMatrix = TowseyLib.ImageTools.GreyScaleImage2Matrix(testImage);
        //var testMatrixTranspose = TowseyLib.DataTools.MatrixTranspose(testMatrix);
        //var gaussianKernel = ImageAnalysisTools.GenerateGaussianKernel(7, 1.0);
        //var gaussianblur = ImageAnalysisTools.GaussianFilter(testMatrixTranspose, gaussianKernel);
        //// Sobel edge/Ridge detector
        ////var SobelRidgeMatrix = TowseyLib.ImageTools.SobelRidgeDetection(testMatrixTranspose);
        //var SobelEdgeMatrix = TowseyLib.ImageTools.SobelEdgeDetection(gaussianblur, 0.15);
        //var IndexX = SobelEdgeMatrix.GetLength(0);
        //var IndexY = SobelEdgeMatrix.GetLength(1);
        //for (int i = 0; i < IndexX; i++)
        //{
        //    for (int j = 0; j < IndexY; j++)
        //    {
        //        if (SobelEdgeMatrix[i, j] == 1)
        //        {
        //            testImage.SetPixel(i, j, Color.Crimson);
        //        }
        //    }
        //}
        //testImage.Save(outputPath);

    }
}
