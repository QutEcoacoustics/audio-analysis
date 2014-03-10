using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    using PowerArgs;

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


            // experiments with clustering the spectra within spectrograms
            if (false)
            {
                SpectralClustering.Sandpit();
            } // end if (true)
            


            // experiments with Sobel ridge detector
            if (false)
            {
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC2_20071005-235040.wav";
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\BAC\BAC5_20080520-040000_silence.wav";
                //string wavFilePath = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav";
                string wavFilePath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";

                string outputDir = @"C:\SensorNetworks\Output\Test";
                string imageFname = "test3.png";
                string annotatedImageFname = "BAC2_annotatedTEST.png";
                double magnitudeThreshold = 7.0; // of ridge height above neighbours

                //var testImage = (Bitmap)(Image.FromFile(imagePath));
                var recording = new AudioRecording(wavFilePath);
                var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
                var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
                Plot scores = null; 
                double eventThreshold = 0.5; // dummy variable - not used
                List<AcousticEvent> list = null;
                Image image = DrawSonogram(spectrogram, scores, list, eventThreshold, null);
                string imagePath = Path.Combine(outputDir, imageFname);
                image.Save(imagePath, ImageFormat.Png);

                double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);

                List<PointOfInterest> poiList = new List<PointOfInterest>();
                double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
                var timeScale = TimeSpan.FromTicks((long)(secondsScale * TimeSpan.TicksPerSecond));
                double herzScale = spectrogram.FBinWidth;
                double freqBinCount = spectrogram.Configuration.FreqBinCount;
                int ridgeLength = 5; // dimension of NxN matrix to use for ridge detection - must be odd number
                int halfLength = ridgeLength / 2;

                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                for (int r = 5 + halfLength; r < rows - halfLength -14; r++) // avoid top 5 freq bins and bottom 14 bins
                {
                    for (int c = halfLength; c < cols - halfLength; c++)
                    {
                        var subM = MatrixTools.Submatrix(matrix, r - halfLength, c - halfLength, r + halfLength, c + halfLength); // extract NxN submatrix
                        double magnitude, direction;
                        bool isRidge = false;
                        TowseyLib.ImageTools.SobelRidgeDetection(subM, out isRidge, out magnitude, out direction);
                        //TowseyLib.ImageTools.Sobel5X5RidgeDetection(subM, out isRidge, out magnitude, out direction);
                        //TowseyLib.ImageTools.Sobel5X5CornerDetection(subM, out isRidge, out magnitude, out direction);
                        if (isRidge && (magnitude > magnitudeThreshold)) 
                        {
                            Point point = new Point(c, r);
                            //var poi = new PointOfInterest(point);
                            TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                            double herz = (freqBinCount-r -1) * herzScale;
                            var poi = new PointOfInterest(time, herz);
                            poi.Point = point;
                            poi.RidgeOrientation = direction;
                            poi.OrientationCategory = (int)Math.Round((direction * 8) / Math.PI);
                            poi.RidgeMagnitude = magnitude;
                            poi.Intensity = matrix[r, c];
                            poi.TimeScale = timeScale;
                            poi.HerzScale = herzScale;
                            poi.IsLocalMaximum = MatrixTools.CentreIsLocalMaximum(subM, magnitudeThreshold + 2.0); // local max must stick out!
                            poiList.Add(poi);
                        } // c++;
                    } // r++;
                }

                //double intensityThreshold = 6.0; // dB
                //PointOfInterest.RemoveLowIntensityPOIs(poiList, intensityThreshold);

                PointOfInterest.PruneSingletons(poiList, rows, cols);
                //PointOfInterest.PruneDoublets(poiList, rows, cols);
                //poiList = PointOfInterest.PruneAdjacentTracks(poiList, rows, cols);

                //Bitmap bmp = (Bitmap)image;
                //foreach (PointOfInterest poi in poiList)
                //{
                //    poi.DrawColor = Color.Crimson;
                //    bool multiPixel = true;
                //    poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                //    //poi.DrawOrientationPoint(bmp, (int)freqBinCount);

                //    // draw local max
                //    //poi.DrawColor = Color.Cyan;
                //    //poi.DrawLocalMax(bmp, (int)freqBinCount);
                //}

                int[,] poiMatrix = PointOfInterest.TransferPOIsToOrientationMatrix(poiList, rows, cols);
                int poiCount;
                double fraction;
                PointOfInterest.CountPOIsInMatrix(poiMatrix, out poiCount, out fraction);
                Console.WriteLine("poiCount={0};  fraction={1}", poiCount, fraction);

                poiMatrix = MatrixTools.MatrixRotate90Clockwise(poiMatrix);
                image = DrawSonogram(spectrogram, scores, poiMatrix);

                imagePath = Path.Combine(outputDir, annotatedImageFname);
                image.Save(imagePath, ImageFormat.Png);
                //image = (Image) bmp;
                //bmp.Save(imagePath);
                FileInfo fiImage = new FileInfo(imagePath);
                if (fiImage.Exists)
                {
                    TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                    process.Run(imagePath, outputDir);
                }

            } // experiments with Sobel ridge detector

            // INPUT FILES
            //string ipdir = @"C:\SensorNetworks\Output\Test2\Towsey.Acoustic"; //KIWI FILES
            //string ipFileName = @"TEST_TUITCE_20091215_220004";

            string ipdir = @"C:\SensorNetworks\Output\SERF\2013MonthlyAveraged"; // SERF
            //string ipFileName = @"7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            string ipFileName1 = "April.monthAv";
            string ipFileName2 = "June.monthAv";
            string ipSdFileName1 = "April.monthSd";
            string ipSdFileName2 = "June.monthSd";

            //string ipdir = @"C:\SensorNetworks\Output\TestSpectrograms";
            //string ipFileName = @"Test24hSpectrogram";


            // OUTPUT FILES
            //string opdir = @"C:\SensorNetworks\Output\Test2\tTestResults";
            //string opdir = @"C:\SensorNetworks\Output\SERF\2014Feb";
            string opdir = @"C:\SensorNetworks\Output\DifferenceSpectrograms\2014March10";


            // experiments with DIFFERENCE false-colour spectrograms
            if (false)
            {
                // set the X and Y axis scales for the spectrograms 
                int minOffset = 0;  // assume recording starts at zero minute of day i.e. midnight
                int xScale = 60;    // assume one minute spectra and hourly time lines
                int sampleRate = 17640; // default value - after resampling
                int frameWidth = 512;   // default value - from which spectrogram was derived
                string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_CVR; //CHANGE RGB mapping here.
                double backgroundFilterCoeff = 0.75; //must be value <=1.0

                string opFileName1 = ipFileName1;
                var cs1 = new ColourSpectrogram(minOffset, xScale, sampleRate, frameWidth, colorMap);
                cs1.ColorMODE = colorMap;
                cs1.BackgroundFilter = backgroundFilterCoeff;
                cs1.ReadCSVFiles(ipdir, ipFileName1, colorMap);
                //ColourSpectrogram.BlurSpectrogram(cs1);
                cs1.DrawGreyScaleSpectrograms(opdir, opFileName1);
                cs1.DrawFalseColourSpectrograms(opdir, opFileName1);
                string imagePath = Path.Combine(opdir, opFileName1 + ".COLNEG.png");
                Image spg1Image = ImageTools.ReadImage2Bitmap(imagePath);
                if (spg1Image == null)
                {
                    Console.WriteLine("SPECTROGRAM IMAGE DOES NOT EXIST: {0}", imagePath);
                    return;
                }
                int titleHt = 20;
                string title = String.Format("FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", ipFileName1, cs1.ColorMODE);
                Image titleBar = ColourSpectrogram.DrawTitleBarOfFalseColourSpectrogram(title, spg1Image.Width, titleHt);
                spg1Image = ColourSpectrogram.FrameSpectrogram(spg1Image, titleBar, minOffset, cs1.X_interval, cs1.Y_interval);

                string opFileName2 = ipFileName2;
                var cs2 = new ColourSpectrogram(minOffset, xScale, sampleRate, frameWidth, colorMap);
                cs2.ColorMODE = colorMap;
                cs2.BackgroundFilter = backgroundFilterCoeff;
                cs2.ReadCSVFiles(ipdir, ipFileName2, colorMap);
                cs2.DrawGreyScaleSpectrograms(opdir, opFileName2);
                cs2.DrawFalseColourSpectrograms(opdir, opFileName2);
                Image spg2Image = ImageTools.ReadImage2Bitmap(Path.Combine(opdir, opFileName2 + ".COLNEG.png"));
                title = String.Format("FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", ipFileName2, cs2.ColorMODE);
                titleBar = ColourSpectrogram.DrawTitleBarOfFalseColourSpectrogram(title, spg2Image.Width, titleHt);
                spg2Image = ColourSpectrogram.FrameSpectrogram(spg2Image, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);

                title = String.Format("DIFFERENCE SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
                double colourGain = 2.0;
                Image deltaSp = ColourSpectrogram.DrawDifferenceSpectrogram(cs2, cs1, colourGain);
                titleBar = ColourSpectrogram.DrawTitleBarOfFalseColourSpectrogram(title, deltaSp.Width, titleHt);
                deltaSp = ColourSpectrogram.FrameSpectrogram(deltaSp, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);
                string opFileName3 = ipFileName1 + ".Difference.COLNEG.png";
                deltaSp.Save(Path.Combine(opdir, opFileName3));

                string opFileName4 = ipFileName1 + ".EuclidianDist.COLNEG.png";
                deltaSp = ColourSpectrogram.DrawDistanceSpectrogram(cs1, cs2);
                Color[] colorArray = ColourSpectrogram.ColourChart2Array(ColourSpectrogram.GetDifferenceColourChart());
                titleBar = ColourSpectrogram.DrawTitleBarOfDifferenceSpectrogram(ipFileName1, ipFileName2, colorArray, deltaSp.Width, titleHt);
                deltaSp = ColourSpectrogram.FrameSpectrogram(deltaSp, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);
                deltaSp.Save(Path.Combine(opdir, opFileName4));

                string opFileName5 = ipFileName1 + ".THREEDist.COLNEG.png";
                deltaSp = ColourSpectrogram.FrameThreeSpectrograms(spg1Image, spg2Image, deltaSp, cs2.X_interval, cs2.Y_interval);
                deltaSp.Save(Path.Combine(opdir, opFileName5));
            }


            // Experiments with t-STATISTIC spectrograms 
            if (true)
            {
                // set the X and Y axis scales for the spectrograms 
                int minOffset = 0;  // assume recording starts at zero minute of day i.e. midnight
                int xScale = 60;  // assume one minute spectra and hourly time lines
                int sampleRate = 17640; // default value - after resampling
                int frameWidth = 512;   // default value - from which spectrogram was derived
                //CHANGE RGB mapping here
                string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_CVR; 
                double backgroundFilterCoeff = 1.0; //must be value <=1.0

                string opFileName1 = ipFileName1;
                var cs1 = new ColourSpectrogram(minOffset, xScale, sampleRate, frameWidth, colorMap);
                cs1.ColorMODE = colorMap;
                cs1.BackgroundFilter = backgroundFilterCoeff;
                cs1.ReadCSVFiles(ipdir, ipFileName1, colorMap);
                //cs1.DrawGreyScaleSpectrograms(opdir, opFileName1);
                cs1.DrawFalseColourSpectrograms(opdir, opFileName1);
                string imagePath = Path.Combine(opdir, opFileName1 + ".COLNEG.png");
                Image spg1Image = ImageTools.ReadImage2Bitmap(imagePath);
                if (spg1Image == null)
                {
                    Console.WriteLine("SPECTROGRAM IMAGE DOES NOT EXIST: {0}", imagePath);
                    return;
                }
                int titleHt = 20;
                string title = String.Format("FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", ipFileName1, cs1.ColorMODE);
                Image titleBar = ColourSpectrogram.DrawTitleBarOfFalseColourSpectrogram(title, spg1Image.Width, titleHt);
                spg1Image = ColourSpectrogram.FrameSpectrogram(spg1Image, titleBar, minOffset, cs1.X_interval, cs1.Y_interval);

                string opFileName2 = ipFileName2;
                var cs2 = new ColourSpectrogram(minOffset, xScale, sampleRate, frameWidth, colorMap);
                cs2.ColorMODE = colorMap;
                cs1.BackgroundFilter = backgroundFilterCoeff;
                cs2.ReadCSVFiles(ipdir, ipFileName2, colorMap);
                //cs2.DrawGreyScaleSpectrograms(opdir, opFileName2);
                cs2.DrawFalseColourSpectrograms(opdir, opFileName2);
                Image spg2Image = ImageTools.ReadImage2Bitmap(Path.Combine(opdir, opFileName2 + ".COLNEG.png"));
                title = String.Format("FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", ipFileName2, cs2.ColorMODE);
                titleBar = ColourSpectrogram.DrawTitleBarOfFalseColourSpectrogram(title, spg2Image.Width, titleHt);
                spg2Image = ColourSpectrogram.FrameSpectrogram(spg2Image, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);



                int N = 30;
                cs1.ReadStandardDeviationSpectrogramCSVs(ipdir, ipSdFileName1);
                cs1.SampleCount = N;
                cs2.ReadStandardDeviationSpectrogramCSVs(ipdir, ipSdFileName2);
                cs2.SampleCount = N;


                //double[] table_df_inf = { 0.25, 0.51, 0.67, 0.85, 1.05, 1.282, 1.645, 1.96, 2.326, 2.576, 3.09, 3.291 };
                //double[] table_df_15 =  { 0.26, 0.53, 0.69, 0.87, 1.07, 1.341, 1.753, 2.13, 2.602, 2.947, 3.73, 4.073 };
                //double[] alpha =        { 0.40, 0.30, 0.25, 0.20, 0.15, 0.10,  0.05,  0.025, 0.01, 0.005, 0.001, 0.0005 };
                double tStatThreshold = 1.645; // 0.05% confidence @ df=infinity
                double colourGain = 2.0;
                //Image deltaSp = ColourSpectrogram.DrawTStatisticSpectrogram(cs1, cs2);
                Image[] array = ColourSpectrogram.DrawTStatisticSpectrogram(cs1, cs2, tStatThreshold, colourGain);

                title = String.Format("t-STATISTIC SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
                Color[] colorArray = ColourSpectrogram.ColourChart2Array(ColourSpectrogram.GetDifferenceColourChart());
                titleBar = ColourSpectrogram.DrawTitleBarOfDifferenceSpectrogram(ipFileName1, ipFileName2, colorArray, array[0].Width, titleHt);
                array[0] = ColourSpectrogram.FrameSpectrogram(array[0], titleBar, minOffset, cs2.X_interval, cs2.Y_interval);
                string opFileName3 = ipFileName1 + ".tTest.COLNEG.png";
                array[0].Save(Path.Combine(opdir, opFileName3));

                title = String.Format("t-STATISTIC SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
                titleBar = ColourSpectrogram.DrawTitleBarOfDifferenceSpectrogram(ipFileName1, ipFileName2, colorArray, array[1].Width, titleHt);
                array[1] = ColourSpectrogram.FrameSpectrogram(array[1], titleBar, minOffset, cs2.X_interval, cs2.Y_interval);
                opFileName3 = ipFileName2 + ".tTest.COLNEG.png";
                array[1].Save(Path.Combine(opdir, opFileName3));

                string opFileName5 = ipFileName1 + ".2tTest.COLNEG.png";
                Image deltaSp = ColourSpectrogram.FrameTwoSpectrograms(array[0], array[1], cs2.X_interval, cs2.Y_interval);
                deltaSp.Save(Path.Combine(opdir, opFileName5));
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
