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

    using Acoustics.Shared;
    using Acoustics.Shared.Extensions;

    using AnalysisBase;
    using AudioAnalysisTools;

    using PowerArgs;

    using ServiceStack.Text;

    using TowseyLib;
    using log4net;
    using QutSensors.Shared;
    
    /// <summary>
    /// The felt analysis.
    /// </summary>
    public class FeltAnalysis : IAnalyser
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
 
            DongSandpit.Play();
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
            //    var path =  @"C:\XUEYAN\DICTA Conference data\Audio data\Edge detection\NW_NW273_20101013-051800-slice1.wav";
            //    var spectrogram = PoiAnalysis.AudioToSpectrogram(path, out audioRecording);
            //    Log.Info("AudioToSpectrogram");

            //    // Do the noise removal
            //    const int BackgroundThreshold = 5;
            //    var noiseReduction = PoiAnalysis.NoiseReductionToBinarySpectrogram(spectrogram, BackgroundThreshold, false, true);
            //    //var noiseReduction = PoiAnalysis.NoiseReductionToBinarySpectrogram(spectrogram, BackgroundThreshold, false, true);            
            //    Log.Info("NoiseReduction");

            //    //var differenceOfGaussian = StructureTensor.BasicDifferenceOfGaussian(StructureTensor.gaussianBlur5);
            //    //Log.Info("differenceOfGaussian");
            //    //var partialDifference = StructureTensor.DifferenceOfGaussianPartialDifference(noiseReduction, differenceOfGaussian.Item1, differenceOfGaussian.Item2);
            //    //Log.Info("partialDifference");
            //    //var structureTensor = StructureTensor.structureTensor(partialDifference.Item1, partialDifference.Item2);
            //    //Log.Info("structureTensor");
            //    //var eigenValue = StructureTensor.EignvalueDecomposition(structureTensor);
            //    //Log.Info("eigenValue");
            //    //var attention = StructureTensor.GetTheAttention(eigenValue);
            //    //Log.Info("attention");
            //    //var pointsOfInterest = StructureTensor.ExtractPointsOfInterest(attention);
            //    //Log.Info("pointsOfInterest");

            //    var imageResult = new Image_MultiTrack(spectrogram.GetImage(true, true));
            //    //imageResult.AddPoints(pointsOfInterest);
            //    imageResult.AddTrack(Image_Track.GetTimeTrack(spectrogram.Duration, spectrogram.FramesPerSecond));
            //    imageResult.Save(path + "1.png");
            //    Log.Info("Show the result of Final PointsOfInterest");
            ////}
         
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

        public class Arguments
        {
            [ArgDescription("The directory to operate on")]
            [ArgExistingDirectory()]
            [ArgPosition(1)]
            [ArgRequired]
            public DirectoryInfo TargetDirectory { get; set; }

            [ArgDescription("The path to the config file")]
            [ArgExistingFile()]
            [ArgRequired]
            public FileInfo Config { get; set; }

            //[ArgDescription("A directory to write output to")]
            //[ArgExistingDirectory()]
            //[ArgRequired]
            //public DirectoryInfo Output { get; set; }
        }

        /// <summary>
        /// This is the (first)entry point, while I am doing developing / testing.
        /// This method should set up any artificial testing parameters, and then call the execute method. 
        /// </summary>
        /// <param name="arguments">
        /// The arguments. 
        /// </param>
        public static void Dev(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = new Arguments();
                const string TempDirectory = @"C:\Test recordings\Test1";

                var testDirectory = @"C:\XUEYAN\targetDirectory";
                string testConfig = @"C:\XUEYAN\config.yml";

                arguments.TargetDirectory = TempDirectory.ToDirectoryInfo();
                arguments.Config = testConfig.ToFileInfo();

                string date = "# Date and Time:" + DateTime.Now;
                Log.Info("Read the wav. file path");
                Log.Info(date);
            }

            Execute(arguments);
        }

        /// <summary>
        /// This is the (second) main entry point, that my code will use when it is run on a super computer. 
        /// It should take all of the parameters from the arguments parameter.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.  
        /// </param>
        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new InvalidOperationException();
            }

            // create a new "analysis"
            var felt = new FeltAnalysis(); 
        
            // merge config settings with analysis settings
            var analysisSettings = felt.GetDefaultSettings;

            //analysisSettings.SourceFile = new FileInfo(recordingPath);
            analysisSettings.ConfigDict = new Dictionary<string, string>();
            analysisSettings.ConfigDict["my_custom_setting"] = "hello xueyan";

            var result = felt.Analyse(analysisSettings);
            string date = "# Date and Time:" + DateTime.Now;
            Log.Info("Finished, yay!");
        }

    }



    public class RidgeEvent : EventBase
    {
        public RidgeEvent(PointOfInterest pointOfInterest, AnalysisSettings analysisSettings, int binCount)
        {
            this.MinHz = pointOfInterest.Herz;
            this.Frame = pointOfInterest.Point.X;
            this.Bin = binCount - pointOfInterest.Point.Y;
            this.Magnitude = pointOfInterest.RidgeMagnitude;
            this.Orientation = (Direction)pointOfInterest.OrientationCategory;

            this.EventStartSeconds = pointOfInterest.TimeLocation.TotalSeconds;

            this.MinuteOffset = (int)(analysisSettings.StartOfSegment ?? TimeSpan.Zero).TotalMinutes;
            this.FileName = analysisSettings.SourceFile.FullName;
        }

        public int Frame { get; set; }

        public int Bin { get; set; }

        public double Magnitude { get; set; }

        public Direction Orientation { get; set; }
    }

    public class RidgeAnalysis : IAnalyser2
    {
        public AnalysisResult2 Analyse(AnalysisSettings analysisSettings)
        {
            var audioFile = analysisSettings.AudioFile;
            var startOffset = analysisSettings.StartOfSegment ?? TimeSpan.Zero;
            var result = new AnalysisResult2
                         {
                             AnalysisIdentifier = Identifier,
                             SettingsUsed = analysisSettings,
                             SegmentStartOffset = startOffset
                         };

            var recording = new AudioRecording(audioFile.FullName);
            result.AudioDuration = recording.Duration();
            if (recording.SampleRate != 22050)
            {
                throw new NotSupportedException();
            }

            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };
            var sonogram = new SpectralSonogram(config, recording.GetWavReader());

            // This config is to set up the parameters used in ridge Detection, the parameters can be changed. 
            var ridgeConfig = new RidgeDetectionConfiguration {
                RidgeDetectionmMagnitudeThreshold = 6.5,
                RidgeMatrixLength = 5,
                FilterRidgeMatrixLength = 7,
                MinimumNumberInRidgeInMatrix = 3
            };
            
            var ridges = POISelection.RidgeDetection(sonogram, ridgeConfig);;

            if (ridges.IsNullOrEmpty())
            {
                return result;
            }

            result.Data = new RidgeEvent[ridges.Count];
            for (int index = 0; index < ridges.Count; index++)
            {
                ((RidgeEvent[])result.Data)[index] = new RidgeEvent(ridges[index], analysisSettings, sonogram.Configuration.FreqBinCount);
            }

            if (analysisSettings.EventsFile != null)
            {
                CsvTools.WriteResultsToCsv(analysisSettings.EventsFile, result.Data);
            }

            if (analysisSettings.IndicesFile != null)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                result.Indexes = ConvertEventsToIndices(result.Data, unitTime, result.AudioDuration, 0);
            }

            if (analysisSettings.ImageFile != null)
            {
                throw new NotImplementedException();
            }

            result.AudioDuration = recording.Duration();
            return result;
        }

        public IEnumerable<ResultBase> ProcessCsvFile(FileInfo csvFile, FileInfo configFile)
        {
            throw new NotImplementedException();
        }

        public IndexBase[] ConvertEventsToIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold)
        {
            if (duration == TimeSpan.Zero)
            {
                return null;
            }

            double units = duration.TotalSeconds / unitTime.TotalSeconds;

            // get whole minutes
            int unitCount = (int)(units / 1);

            // add fractional minute
            if ((units % 1) > 0.0)
            {
                unitCount += 1;
            } 

            int[] eventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] bigEvsPerUnitTime = new int[unitCount]; //to store counts of high scoring events

            foreach (EventBase anEvent in events)
            {
                double eventStart = anEvent.EventStartAbsolute.Value;// (double)ev[AudioAnalysisTools.Keys.EVENT_START_ABS];
                double eventScore = anEvent.Score; // (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);

                // TODO: why not -gt, ask michael
                if (eventScore != 0.0)
                {
                    eventsPerUnitTime[timeUnit]++;
                }
                if (eventScore > scoreThreshold)
                {
                    bigEvsPerUnitTime[timeUnit]++;
                }
            }

            var indices = new IndexBase[eventsPerUnitTime.Length];

            for (int i = 0; i < eventsPerUnitTime.Length; i++)
            {
                var newIndex = new EventIndex();

                int unitId = (int) (i * unitTime.TotalMinutes);

                newIndex.MinuteOffset = unitId;
                newIndex.EventsTotal = eventsPerUnitTime[i];
                newIndex.EventsTotalThresholded = bigEvsPerUnitTime[i];

                indices[i] = newIndex;
            }

            return indices;
        }

        public string DisplayName
        {
            get
            {
                return "Ridge Detection";
            }
        }

        public string Identifier
        {
            get
            {
                return "Dong.RidgeDetection";
            }
        }

        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings()
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(20),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = 22050
                };
            }
        }

        #region ignored
        AnalysisResult IAnalyser.Analyse(AnalysisSettings analysisSettings)
        {
            throw new NotImplementedException();
        }

        Tuple<DataTable, DataTable> IAnalyser.ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            throw new NotImplementedException();
        }

        DataTable IAnalyser.ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }
        #endregion



    }
}
