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
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";  // why we need this?

        public static void Play()
        {

            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());

            // experiments with Sobel ridge detector
            if (true)
            {
                /// Read a bunch of recordings  
                //string[] files = Directory.GetFiles(analysisSettings.SourceFile.FullName);


                /// Read one specific file name/path 
                // with human beings
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage2\TestImage2.png")); 

                // just simple shapes
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage3\TestImage3.png")); 

                // real spectrogram
                //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM420036_min430Crows-1minute.wav-noiseReduction-1Klines.png"));               
                //string outputPath = @"C:\Test recordings\Crows\Test\TestImage3\TestImage3-GaussianBlur-thre-7-sigma-1.0-SobelEdgeDetector-thre-0.15.png";
                //string outputFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result";
                //string imageFileName = "CannyEdgeDetector1.png";

                // read one specific recording
                string wavFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM4420036_min430Crows-1minute.wav";
                string outputDirectory = @"C:\Test recordings\Output\Test";
                string imageFileName = "test.png";
                string annotatedImageFileName = "annotatedTEST8-similarBitCount-10-9-v-7-h-5-DrawBox.png";
                double magnitudeThreshold = 7.0; // of ridge height above neighbours
                //double intensityThreshold = 5.0; // dB

                var recording = new AudioRecording(wavFilePath);
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
                var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                Plot scores = null;
                double eventThreshold = 0.5; // dummy variable - not used
                List<AcousticEvent> list = null;
                var poiList1 = new List<PointOfInterest>();
                Image image = DrawSonogram(spectrogram, scores, list, eventThreshold, poiList1);
                string imagePath = Path.Combine(outputDirectory, imageFileName);
                //image.Save(imagePath, ImageFormat.Png);

                Bitmap bmp = (Bitmap)image;

                double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);

                List<PointOfInterest> poiList = new List<PointOfInterest>();
                double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
                var timeScale = TimeSpan.FromTicks((long)(secondsScale * TimeSpan.TicksPerSecond)); // Time scale here is millionSecond? 
                double herzScale = spectrogram.FBinWidth;
                double freqBinCount = spectrogram.Configuration.FreqBinCount;
                int ridgeLength = 5; // dimension of NxN matrix to use for ridge detection - must be odd number
                int halfLength = ridgeLength / 2;

                /* just an example
                var convert = poiList
                    .OrderBy(poi => poi.Point.X)
                    .ThenBy(poi => poi.Point.Y)
                    .Aggregate<PointOfInterest, double[,]>(
                        new double[poiList.Max(poi => poi.Point.X), poiList.Max(poi => poi.Point.Y)], 
                        (double[,] aggregation, PointOfInterest current) =>
                        {
                            aggregation[current.Point.X, current.Point.Y] = current.Intensity;
                            return aggregation;
                        }
                );
                */

                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                for (int r = halfLength; r < rows - halfLength; r++)
                {
                    for (int c = halfLength; c < cols - halfLength; c++)
                    {

                        var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                        double magnitude;
                        double direction;
                        bool isRidge = false;
                        TowseyLib.ImageTools.Sobel5X5RidgeDetection(subM, out isRidge, out magnitude, out direction);
                        if (magnitude > magnitudeThreshold)
                        {
                            Point point = new Point(c, r);
                            TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                            double herz = (freqBinCount - r - 1) * herzScale;
                            var poi = new PointOfInterest(time, herz);
                            poi.Point = point;
                            poi.RidgeOrientation = direction;
                            poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                            poi.RidgeMagnitude = magnitude;
                            poi.Intensity = matrix[r, c];
                            poi.TimeScale = timeScale;
                            poi.HerzScale = herzScale;
                            poiList.Add(poi);
                        }

                    }
                }

                /// filter out some redundant poi                
                //PointOfInterest.RemoveLowIntensityPOIs(poiList, intensityThreshold);
                //PointOfInterest.PruneSingletons(poiList, rows, cols);
                //PointOfInterest.PruneDoublets(poiList, rows, cols);
                poiList = ImageAnalysisTools.PruneAdjacentTracks(poiList, rows, cols);
                //poiList = PointOfInterest.PruneAdjacentTracks(poiList, rows, cols);
                var filterPoiList = ImageAnalysisTools.RemoveIsolatedPoi(poiList, rows, cols, 7, 3);
                //var featureVector = FeatureVector.GeneratePercentageOfFeatureVectors(filterPoiList, rows, cols, 9);               
                var featureVector = FeatureVector.GenerateBitOfFeatureVectors(filterPoiList, rows, cols, 11);
                //var hitPoiList = TemplateTools.UnknownTemplate(poiList, rows, cols);
                var finalPoiList = new List<PointOfInterest>();
                // draw poi with paint

                //foreach (PointOfInterest poi in filterPoiList)
                foreach (FeatureVector fv in featureVector)
                {
                    //poi.DrawColor = Color.Crimson;
                    //bool multiPixel = false;
                    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                    //poi.DrawOrientationPoint(bmp, (int)freqBinCount); 
                    // percentageFeatureVector 
                    //var percentageFeatureVector = new double[4];
                    //percentageFeatureVector[0] = 0.4;// 0.5;
                    //percentageFeatureVector[1] = 0.4;// 0.4;
                    //percentageFeatureVector[2] = 0.0; //0.0;
                    //percentageFeatureVector[3] = 0.2;//0.1;

                    // bit feature Vector
                    var verticalBit = new int[11] { 6, 4, 2, 2, 0, 1, 0, 2, 0, 0, 2 };
                    var horizontalBit = new int[11] { 0, 0, 0, 0, 0, 0, 3, 3, 2, 3, 5 };
                    //var similarityScore = TemplateTools.CalculateSimilarityScoreForPercentagePresention(fv, TemplateTools.HoneyeaterTemplate(percentageFeatureVector));
                    var similarityScore = TemplateTools.CalculateSimilarityScoreForBitPresentation(fv, TemplateTools.HoneyeaterTemplate(verticalBit, horizontalBit));
                    if (similarityScore > 0)
                    {
                        finalPoiList.Add(new PointOfInterest(new Point(fv.point.Y, fv.point.X)));
                        //bmp.SetPixel(fv.point.Y, fv.point.X, Color.Crimson);
                    }
                    // draw local max
                    //poi.DrawColor = Color.Cyan;
                    //poi.DrawLocalMax(bmp, (int)freqBinCount);
                }
                finalPoiList = LocalMaxima.RemoveClosePoints(finalPoiList, 5);
                //foreach (var fiv in finalPoiList)
                //{
                //    bmp.SetPixel(fiv.Point.X, fiv.Point.Y, Color.Crimson);
                //}             
                image = DrawSonogram(spectrogram, scores, list, eventThreshold, finalPoiList);
                imagePath = Path.Combine(outputDirectory, annotatedImageFileName);
                image.Save(imagePath, ImageFormat.Png);
                FileInfo fileImage = new FileInfo(imagePath);
                if (fileImage.Exists)
                {
                    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                    process.Run(imagePath, outputDirectory);
                }
            }

            // experiments with false colour images - categorising/discretising the colours
            if (false)
            {
                Console.WriteLine("Reading image");
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                //string inputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.colSpectrum.png";
                //string outputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.discreteColSpectrum.png";

                string inputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\DM420036.colSpectrum.png";
                string outputPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\DM420036.discreteColSpectrum.png";

                const int R = 0;
                const int G = 1;
                const int B = 2;
                double[,] discreteIndices = new double[12, 3]; // Ht, ACI and Ampl values in 0,1
                discreteIndices[0, R] = 0.00; discreteIndices[0, G] = 0.00; discreteIndices[0, B] = 0.00; // white
                discreteIndices[1, R] = 0.20; discreteIndices[1, G] = 0.00; discreteIndices[1, B] = 0.00; // pale blue
                discreteIndices[2, R] = 0.60; discreteIndices[2, G] = 0.20; discreteIndices[2, B] = 0.10; // medium blue

                discreteIndices[3, R] = 0.00; discreteIndices[3, G] = 0.00; discreteIndices[3, B] = 0.40; // pale yellow
                discreteIndices[4, R] = 0.00; discreteIndices[4, G] = 0.05; discreteIndices[4, B] = 0.70; // bright yellow
                discreteIndices[5, R] = 0.20; discreteIndices[5, G] = 0.05; discreteIndices[5, B] = 0.80; // yellow/green
                discreteIndices[6, R] = 0.50; discreteIndices[6, G] = 0.05; discreteIndices[6, B] = 0.50; // yellow/green
                discreteIndices[7, R] = 0.99; discreteIndices[7, G] = 0.30; discreteIndices[7, B] = 0.70; // green

                discreteIndices[8, R] = 0.10; discreteIndices[8, G] = 0.95; discreteIndices[8, B] = 0.10;    // light magenta
                discreteIndices[9, R] = 0.50; discreteIndices[9, G] = 0.95; discreteIndices[9, B] = 0.50;    // medium magenta
                discreteIndices[10, R] = 0.70; discreteIndices[10, G] = 0.95; discreteIndices[10, B] = 0.70; // dark magenta
                discreteIndices[11, R] = 0.95; discreteIndices[11, G] = 0.95; discreteIndices[11, B] = 0.95; // black

                int N = 12; // number of discrete colours
                byte[,] discreteColourValues = new byte[N, 3]; // Ht, ACI and Ampl values in 0,255
                for (int r = 0; r < discreteColourValues.GetLength(0); r++)
                {
                    for (int c = 0; c < discreteColourValues.GetLength(1); c++)
                    {
                        discreteColourValues[r, c] = (byte)Math.Floor((1 - discreteIndices[r, c]) * 255);
                    }
                }

                // set up the colour pallette.
                Color[] colourPalette = new Color[N]; //palette
                for (int c = 0; c < N; c++)
                {
                    colourPalette[c] = Color.FromArgb(discreteColourValues[c, R], discreteColourValues[c, G], discreteColourValues[c, B]);
                }

                // read in the image
                Bitmap image = ImageTools.ReadImage2Bitmap(inputPath);
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color imageCol = image.GetPixel(x, y);
                        byte[] imageColorVector = new byte[3];
                        imageColorVector[0] = imageCol.R;
                        imageColorVector[1] = imageCol.G;
                        imageColorVector[2] = imageCol.B;
                        // get colour from palette closest to the existing colour
                        double[] distance = new double[N];
                        for (int c = 0; c < N; c++)
                        {
                            byte[] colourVector = new byte[3];
                            colourVector[0] = discreteColourValues[c, 0];
                            colourVector[1] = discreteColourValues[c, 1];
                            colourVector[2] = discreteColourValues[c, 2];
                            distance[c] = DataTools.EuclidianDistance(imageColorVector, colourVector);
                        }
                        int minindex, maxindex;
                        double min, max;
                        DataTools.MinMax(distance, out minindex, out maxindex, out  min, out max);

                        //if ((col.R > 200) && (col.G > 200) && (col.B > 200))
                        image.SetPixel(x, y, colourPalette[minindex]);
                    }
                }

                ImageTools.WriteBitmap2File(image, outputPath);

            } // experiments with false colour images - categorising/discretising the colours

            Log.WriteLine("# Finished!");
            Console.ReadLine();
            System.Environment.Exit(666);
        } // Dev()

        public static Image DrawSonogram(BaseSonogram sonogram, Plot scores, List<AcousticEvent> poi, double eventThreshold, List<PointOfInterest> poiList)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //Add this line below
            image.AddPoints(poiList);
            if (scores != null) image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            if ((poi != null) && (poi.Count > 0))
                image.AddEvents(poi, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()

    } // class dong.sandpit
}
