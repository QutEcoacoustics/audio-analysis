// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Dong.Felt.Main.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   The felt analysis.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Drawing;
    using AnalysisBase;
    using AudioAnalysisTools;
    using TowseyLib;
    using log4net;
    
    /// <summary>
    /// The felt analysis.
    /// </summary>
    public class FeltAnalysis : IAnalyser, IUsage
    {
        private const string StandardConfigFileName = "Dong.Felt.yml";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName
        {
            get { return "Xueyan Dong's FELT work"; }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public string Identifier
        {
            get { return "Dong.Felt"; }
        }

        public AnalysisSettings DefaultSettings { get; private set; }

        /// <summary>
        /// Gets default analysis settings.
        /// </summary>
        public AnalysisSettings GetDefaultSettings
        {
            get
            {
                return new AnalysisSettings();
            }
        }

        /// <summary>
        /// This is the main analysis method.
        /// At this point, there should be no parsing of command line parameters. This method should be called by the execute method.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis settings.
        /// </param>
        /// <returns>
        /// The AnalysisResult.
        /// </returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            // XUEYAN　－　You should start writing your analysis in here
            // read the config file
            // object settings;
            // using (var reader = new StringReader(analysisSettings.ConfigFile.FullName)) {
            //    //var yaml = new YamlStream();
            //    //yaml.Load(reader);
            //    var serializer = new YamlSerializer();
            //    settings = serializer.Deserialize(reader, new DeserializationOptions() { });
            // }  

            var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM420036_min430Crows-1minute.png"));
            //var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage3\TestImage3.png"));
            //string outputPath = @"C:\Test recordings\Crows\Test\TestImage3\TestImage3-GaussianBlur-thre-7-sigma-1.0-SobelEdgeDetector-thre-0.15.png";
            string outputFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result\2CannyEdgeDetection-threshold-2.0-1.0-guassianblur-5-1.0.png";
            //string outputFilePath = @"C:\Test recordings\Crows\Test\TestImage3\TestImage3-CannyEdgeDetection-1.0.png";
            var testMatrix = TowseyLib.ImageTools.GreyScaleImage2Matrix(testImage);
            var testMatrixTranspose = TowseyLib.DataTools.MatrixTranspose(testMatrix); 
            
            double[,] magnitude, direction;
            ImageAnalysisTools.CannyEdgeDetector(testMatrixTranspose, out magnitude, out direction);

            string wavFilePath = @"C:\Test recordings\Crows\DM4420036_min430Crows-result\DM420036_min430Crows-1minute.wav";
            var recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5 };
            var spectrogram = new SpectralSonogram(config, recording.GetWavReader());
            List<PointOfInterest> poiList = new List<PointOfInterest>();
            double secondsScale = spectrogram.Configuration.GetFrameOffset(recording.SampleRate);
            var timeScale = TimeSpan.FromTicks((long)(secondsScale * TimeSpan.TicksPerSecond));
            double herzScale = spectrogram.FBinWidth;
            double freqBinCount = spectrogram.Configuration.FreqBinCount;
            double[,] matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);
            Plot scores = null; 
            double eventThreshold = 0.5; // dummy variable - not used               
            List<AcousticEvent> list = null;
            Image image = ImageAnalysisTools.DrawSonogram(spectrogram, scores, list, eventThreshold);
            Bitmap bmp = (Bitmap)image;
          
            double magnitudeThreshold = 1.0;
            //int rows = testMatrixTranspose.GetLength(0);
            //int cols = testMatrixTranspose.GetLength(1);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // strong edge
                    if (magnitude[c, r] > magnitudeThreshold)
                    {
                        //testImage.SetPixel(r, c, Color.Crimson);
                        Point point = new Point(c, r);
                        //var poi = new PointOfInterest(point);
                        TimeSpan time = TimeSpan.FromSeconds(c * secondsScale);
                        double herz = (freqBinCount - r - 1) * herzScale;
                        var poi = new PointOfInterest(time, herz);
                        poi.Point = point;
                        poi.RidgeOrientation = direction[c, r];
                        poi.OrientationCategory = (int)Math.Round((direction[c, r] * 8) / Math.PI);
                        poi.RidgeMagnitude = magnitude[c, r];
                        poi.Intensity = matrix[r, c];
                        poi.TimeScale = timeScale;
                        poi.HerzScale = herzScale;
                        //poi.IsLocalMaximum = MatrixTools.CentreIsLocalMaximum(subM, magnitudeThreshold + 2.0); // local max must stick out!
                        poiList.Add(poi);
                        testImage.Save(outputFilePath);
                    }
                    else
                    {
                        if (magnitude[c, r] > 0)
                        {
                            testImage.SetPixel(c, r, Color.Blue);
                        }
                    }
                }
            }
            PointOfInterest.PruneSingletons(poiList, rows, cols);
                //PointOfInterest.PruneDoublets(poiList, rows, cols);
                poiList = PointOfInterest.PruneAdjacentTracks(poiList, rows, cols);

                foreach (PointOfInterest poi in poiList)
                {
                    poi.DrawColor = Color.Crimson;
                    bool multiPixel = false;
                    //poi.DrawPoint(bmp, (int)freqBinCount, multiPixel);
                    poi.DrawOrientationPoint(bmp, (int)freqBinCount);

                    // draw local max
                    //poi.DrawColor = Color.Cyan;
                    //poi.DrawLocalMax(bmp, (int)freqBinCount);
                }
            
            image.Save(outputFilePath);


            // Batch Process
            //foreach (string path in Files)
            //{
            //    // Writing my code here
            //    if (!File.Exists(path))
            //    {
            //        throw new Exception("Can't find this recording file path: " + path);
            //    }

            //    // Get wav.file path
            //    string wavFilePath = analysisSettings.SourceFile.FullName;
            //    // Read the .wav file
            //    AudioRecording audioRecording;
            //    var spectrogram = PoiAnalysis.AudioToSpectrogram(path, out audioRecording);
            //    Log.Info("AudioToSpectrogram");

            //    // Do the noise removal
            //    const int BackgroundThreshold = 5;
            //    var noiseReduction = PoiAnalysis.NoiseReductionToBinarySpectrogram(spectrogram, BackgroundThreshold, false, true);
            //    //var noiseReduction = PoiAnalysis.NoiseReductionToBinarySpectrogram(spectrogram, BackgroundThreshold, false, true);            
            //    Log.Info("NoiseReduction");

            //    var differenceOfGaussian = StructureTensor.BasicDifferenceOfGaussian(StructureTensor.gaussianBlur5);
            //    Log.Info("differenceOfGaussian");
            //    var partialDifference = StructureTensor.DifferenceOfGaussianPartialDifference(noiseReduction, differenceOfGaussian.Item1, differenceOfGaussian.Item2);
            //    Log.Info("partialDifference");
            //    var structureTensor = StructureTensor.structureTensor(partialDifference.Item1, partialDifference.Item2);
            //    Log.Info("structureTensor");
            //    var eigenValue = StructureTensor.EignvalueDecomposition(structureTensor);
            //    Log.Info("eigenValue");
            //    var attention = StructureTensor.GetTheAttention(eigenValue);
            //    Log.Info("attention");
            //    var pointsOfInterest = StructureTensor.ExtractPointsOfInterest(attention);
            //    Log.Info("pointsOfInterest");
                
            //    var imageResult = new Image_MultiTrack(spectrogram.GetImage(false, true));
            //    imageResult.AddPoints(pointsOfInterest);
            //    imageResult.AddTrack(Image_Track.GetTimeTrack(spectrogram.Duration, spectrogram.FramesPerSecond));
            //    imageResult.Save(path + ".png");
            //    Log.Info("Show the result of Final PointsOfInterest");
            //}
           
            var result = new AnalysisResult();
            return result;
        }

        /// <summary>
        /// The process csv file.
        /// </summary>
        /// <param name="fiCsvFile">
        /// The fi csv file.
        /// </param>
        /// <param name="fiConfigFile">
        /// The fi config file.
        /// </param>
        /// <returns>
        /// The <see cref="Tuple"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Tuple<System.Data.DataTable, System.Data.DataTable> ProcessCsvFile(System.IO.FileInfo fiCsvFile, System.IO.FileInfo fiConfigFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The convert events 2 indices.
        /// </summary>
        /// <param name="dt">
        /// The dt.
        /// </param>
        /// <param name="unitTime">
        /// The unit time.
        /// </param>
        /// <param name="timeDuration">
        /// The time duration.
        /// </param>
        /// <param name="scoreThreshold">
        /// The score threshold.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public System.Data.DataTable ConvertEvents2Indices(System.Data.DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is the (first)entry point, while I am doing developing / testing.
        /// This method should set up any artificial testing parameters, and then call the execute method. 
        /// </summary>
        /// <param name="arguments">
        /// The arguments. 
        /// </param>
        public static void Dev(string[] arguments)
        {
            
            const string TempDirectory = @"C:\Test recordings\Test1";
            
            arguments = new string[2];
            arguments[0] = "-input";
            arguments[1] = TempDirectory;

            if (arguments.Length == 0)
            {
                var testDirectory = @"C:\XUEYAN\targetDirectory";
                string testConfig = @"C:\XUEYAN\config.yml";
                arguments = new[] { testConfig, testDirectory };
            }

            string date = "# Date and Time:" + DateTime.Now;
            Log.Info("Read the wav. file path");
            Log.Info(date);

            Execute(arguments);
        }

        /// <summary>
        /// This is the (second) main entry point, that my code will use when it is run on a super computer. 
        /// It should take all of the parameters from the arguments parameter.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.  
        /// </param>
        public static void Execute(string[] arguments)
        {
            if (arguments.Length % 2 != 0)
            {
                throw new Exception("odd number of arguments and values");
            }

            // create a new "analysis"
            var felt = new FeltAnalysis(); 
        
            // merge config settings with analysis settings
            var analysisSettings = felt.GetDefaultSettings;

            // var configFile = arguments.Skip(arguments.IndexOf("-configFile");
            // if (!File.Exists(ConfigFilePath))
            // {
            // throw new Exception("Can't find config file");
            // }
            // Log.Info("Using config file: " + ConfigFilePath);
            // analysisSettings.ConfigFile = new FileInfo(ConfigFilePath);

            // get the file path from arguments
            string recordingPath = arguments[1];
            if (!Directory.Exists(recordingPath))
            {
                throw new Exception("Can't find this recording file path: "  + recordingPath);
            }

            analysisSettings.SourceFile = new FileInfo(recordingPath);
            analysisSettings.ConfigDict = new Dictionary<string, string>();
            analysisSettings.ConfigDict["my_custom_setting"] = "hello xueyan";

            var result = felt.Analyse(analysisSettings);
            Log.Info("Finished, yay!");
        }

        /// <summary>
        /// The usage.
        /// </summary>
        /// <param name="stringBuilder">
        /// The string Builder.
        /// </param>
        /// <returns>
        /// The <see cref="StringBuilder"/>.
        /// </returns>
        public StringBuilder Usage(StringBuilder stringBuilder)
        {
            stringBuilder.Append("Dong.FELT usage:");
            stringBuilder.Append("... dong.felt configurationFile.yml testdirectory");

            return stringBuilder;
        }
    }
}
