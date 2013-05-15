﻿// --------------------------------------------------------------------------------------------------------------------
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

            // Read one specific file 
            var testImage = (Bitmap)(Image.FromFile(@"C:\Test recordings\Crows\Test\TestImage4\Test4.png"));
            string lewinsRail = @"C:\Test recordings\LewinsRail\BAC2_20071008-075040-result\BAC2_20071008-075040.wav";
            //// Read a bunch of recording files  
            //string[] Files = Directory.GetFiles(analysisSettings.SourceFile.FullName);

            AudioRecording audioRecording;
            var spectrogram = PoiAnalysis.AudioToSpectrogram(lewinsRail, out audioRecording);
            Log.Info("AudioToSpectrogram");

            //// Do the noise removal
            //const int BackgroundThreshold = 5;
            //var noiseReduction = PoiAnalysis.NoiseReductionToBinarySpectrogram(spectrogram, BackgroundThreshold, false, true);
            ////var noiseReduction = PoiAnalysis.NoiseReductionToBinarySpectrogram(spectrogram, BackgroundThreshold, false, true);            
            //Log.Info("NoiseReduction");

            //// Find the local Maxima
            //const int NeibourhoodWindowSize = 7;
            //var localMaxima = LocalMaxima.PickLocalMaxima(noiseReduction, NeibourhoodWindowSize);

            //// Filter out points
            //const int AmplitudeThreshold = 10;
            //var filterOutPoints = LocalMaxima.FilterOutPoints(localMaxima, AmplitudeThreshold); // pink noise model threshold                

            //// Remove points which are too close
            //const int DistanceThreshold = 7;
            //var finalPoi = LocalMaxima.RemoveClosePoints(filterOutPoints, DistanceThreshold);

            //var imageResult = new Image_MultiTrack(spectrogram.GetImage(false, true));
            //imageResult.AddPoints(finalPoi);
            //imageResult.AddTrack(Image_Track.GetTimeTrack(spectrogram.Duration, spectrogram.FramesPerSecond));
            //imageResult.Save(@"C:\Test recordings\LewinsRail\BAC2_20071008-075040-result\BAC2_20071008-075040-localMaxima.png");
            //Log.Info("Show the result of Final PointsOfInterest");
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

            // For test image 
            var testMatrix1 = TowseyLib.ImageTools.GreyScaleImage2Matrix(testImage);
            var testMatrix = TowseyLib.DataTools.MatrixTranspose(testMatrix1); //  Why I have to transpose it?

            var differenceOfGaussian = StructureTensor.DifferenceOfGaussian(StructureTensor.gaussianBlur5);
            Log.Info("differenceOfGaussian");
            var partialDifference = StructureTensor.CannyPartialDifference(testMatrix);
            Log.Info("partialDifference");
            var magnitude = StructureTensor.MagnitudeOfPartialDifference(partialDifference.Item1, partialDifference.Item2);
            Log.Info("magnitude");
            var phase = StructureTensor.PhaseOfPartialDifference(partialDifference.Item1, partialDifference.Item2);
            Log.Info("phase");
            var structureTensor = StructureTensor.structureTensor(partialDifference.Item1, partialDifference.Item2);
            Log.Info("structureTensor");
            var eigenValue = StructureTensor.EignvalueDecomposition(structureTensor);
            Log.Info("eigenValue");
            var coherence = StructureTensor.Coherence(eigenValue);
            Log.Info("coherence");
            var hitCoherence = StructureTensor.hitCoherence(coherence);
            Log.Info("hitCoherence");
            
            //var numberOfVetex = structureTensor.Count;
            //var results = new List<string>();

            //results.Add("eigenValue1, eigenValue2, coherence");
            //for (int i = 0; i < numberOfVetex; i++)
            //{
            //    //Console.WriteLine(eigenValue[i].Item2[0] + eigenValue[i].Item2[1] + magnitude[i] + phase[i] + coherence[i]);
            //    //Log.Info(string.Format("eigenValue1:{0}, eigenValue2:{1}, magnitude:{2}, phase:{3}, coherence:{4}",
            //    //    eigenValue[i].Item2[0], eigenValue[i].Item2[1], magnitude[i], phase[i], coherence[i]));

            //    results.Add(string.Format("{0}, {1}, {2}", eigenValue[i].Item2[0], eigenValue[i].Item2[1],  coherence[i].Item2));
            //}
            //File.WriteAllLines(@"C:\Test recordings\Crows\Test\TestImage4\text5.csv", results.ToArray());

            //var results1 = new List<string>();
            //results1.Add("partialDifferenceX, partialDifferenceY, magnitude, phase");

            //var maximumXindex = partialDifference.Item1.GetLength(0);
            //var maximumYindex = partialDifference.Item1.GetLength(1);
            //for (int i = 0; i < maximumXindex; i++)
            //{
            //    for (int j = 0; j < maximumYindex; j++)
            //    {
            //        results1.Add(string.Format("{0}, {1}, {2}, {3}", partialDifference.Item1[i,j], partialDifference.Item2[i, j], magnitude[i, j], phase[i,j]));
            //    }
            //}
            //File.WriteAllLines(@"C:\Test recordings\Crows\Test\TestImage4\text6.csv", results.ToArray());

            foreach (var poi in hitCoherence)
            {
                testImage.SetPixel(poi.Point.X, poi.Point.Y, Color.Crimson);
            }
            testImage.Save(@"C:\Test recordings\Crows\Test\TestImage4\Test4-cannydetector-hitCoherence0.png");
            
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
