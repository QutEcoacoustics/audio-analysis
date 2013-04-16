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

            // Writing my code here
            // get wav.file path
            string wavFilePath = analysisSettings.SourceFile.FullName;      
            
            // Read the .wav file
            AudioRecording audioRecording;
            var spectrogram = PoiAnalysis.AudioToSpectrogram(wavFilePath, out audioRecording);

            // Do the noise removal
            const int BackgroundThreshold = 5;
            var noiseReduction = PoiAnalysis.NoiseReductionToBinarySpectrogram(spectrogram, BackgroundThreshold);
            
            // Find the local Maxima
            const int NeibourhoodWindowSize = 7;
            var localMaxima = PoiAnalysis.PickLocalMaximum(noiseReduction, NeibourhoodWindowSize);  
            var imageResult = new Image_MultiTrack(spectrogram.GetImage(false, true));
            imageResult.AddPoints(localMaxima);
            imageResult.AddTrack(Image_Track.GetTimeTrack(spectrogram.Duration, spectrogram.FramesPerSecond));
            imageResult.Save(@"C:\Test recordings\Crows\localMaxima.png");

            const int NumberOfTemplatePoints = 18;
            var centeroid = TemplateTools.GetCentroid(TemplateTools.LewinsRailTemplate(NumberOfTemplatePoints));

            // Filter out points
            const int AmplitudeThreshold = 7;
            var filterOutPoints = PoiAnalysis.FilterOutPoints(localMaxima, AmplitudeThreshold); // pink noise model threshold
            
            // Remove points which are too close
            const int DistanceThreshold = 7;
            var finalPois = PoiAnalysis.RemoveClosePoint(filterOutPoints, DistanceThreshold);
            

            // Calculate the distance between poi and points in the template
            var avgDistanceScores = PoiAnalysis.AverageDistanceScores(TemplateTools.LewinsRailTemplate(NumberOfTemplatePoints), finalPois);

            // Get the metched anchor point (centroid)
            const int AvgDistanceScoreThreshold = 6;
            var matchedPoi = PoiAnalysis.MatchedPointsOfInterest(finalPois, avgDistanceScores, AvgDistanceScoreThreshold);

            // Get the absolute template for each matchedPoi
            var templatePoints = PoiAnalysis.GetAbsoluteTemplate2(matchedPoi);
            
            // Draw circle on different types of points 
            
            // .addPoints( templatePoints);
            // imageResult.AddPoints(templatePoints);
            // imageResult.AddPoints(matchedPoi);
            // addEvents(templateBoundingBoxes); 
            
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
            const string TempFile = @"C:\Test recordings\Crows\DM420036_min430Crows-1minute.wav";

            arguments = new string[2];
            arguments[0] = "-input";
            arguments[1] = TempFile;

            // if (arguments.Length == 0)
            // {
            //    var testDirectory = @"C:\XUEYAN\targetDirectory";
            //    string testConfig = @"C:\XUEYAN\config.yml";
            //    arguments = new[] { testConfig, testDirectory };
            // }
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
            if (!File.Exists(recordingPath))
            {
                throw new Exception("Can't find this recordingfile path: "  + recordingPath);
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
