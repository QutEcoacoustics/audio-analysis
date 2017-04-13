﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Dong.Felt.Main.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   The felt analysis.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using Dong.Felt;
    using Dong.Felt.Representations;

    using log4net;

    using PowerArgs;

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
            // these are command line arguments
            var args = EntryArguments;

            // these are configuration settings
            dynamic configuration = Yaml.Deserialise(analysisSettings.ConfigFile);

            args.Input = args.Input ?? ((string)configuration.InputDirectory).ToDirectoryInfo();
            args.Output = args.Output ?? ((string)configuration.OutputDirectory).ToDirectoryInfo();

            if (args.TempDir == null)
            {
                args.TempDir = new DirectoryInfo(Path.Combine(args.Output.FullName, "temp"));
            }

            Directory.CreateDirectory(args.TempDir.FullName);

            //DongSandpit.Play(configuration, args.Input, args.Output);
            DongSandpit.ParameterMixture(configuration, args.FeaturePropertySet, args.Input, args.Output, args.TempDir);
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
        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
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
        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments
        {

            [ArgDescription("The source directory to analyse")]
            [Production.ArgExistingDirectory()]
            public DirectoryInfo Input { get; set; }

            [ArgDescription("The set of feature properties to use to represent a feature")]
            [DefaultValue(RidgeDescriptionNeighbourhoodRepresentation.FeaturePropSet1)]
            public string FeaturePropertySet { get; set; }

            [ArgDescription("The path to the config file")]
            [Production.ArgExistingFile()]
            [ArgRequired]
            public FileInfo Config { get; set; }

            [ArgDescription("The ouput directory")]
            [Production.ArgExistingDirectory()]
            public DirectoryInfo Output { get; set; }

            [ArgDescription("The directory to store temporary files")]
            public DirectoryInfo TempDir { get; set; }

            public static string Description()
            {
                return "Xueyan's workspace for her research. FELT related stuff.";
            }

            public static string AdditionalNotes()
            {
                return "The majority of the options for this analysis are in the config file or are build constants.";
            }

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
                // Xueyan is not using this functionality
            }

            Execute(arguments);
        }

        private static Arguments EntryArguments;

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
                // Xueyan is not using the Dev functionality
                //throw new InvalidOperationException();
            }

            // create a new "analysis"
            var felt = new FeltAnalysis();

            // merge config settings with analysis settings
            var analysisSettings = felt.GetDefaultSettings;

            analysisSettings.ConfigFile = arguments.Config;

            // ultra dodgy - future Anthony/Mark please don't hate me
            EntryArguments = arguments;

            // not used
            /*var result = */felt.Analyse(analysisSettings);

            string date = "# Date and Time:" + DateTime.Now;
            Log.Info("Finished, yay!");
        }

    }



    public class RidgeEvent : EventBase
    {
        public RidgeEvent(PointOfInterest pointOfInterest, AnalysisSettings analysisSettings, SpectrogramStandard sonogram)
        {
            this.SegmentStartOffset = analysisSettings.SegmentStartOffset.Value;
            this.MinHz = pointOfInterest.Herz;
            this.Frame = pointOfInterest.Point.X;
            this.Bin = sonogram.Configuration.FreqBinCount - pointOfInterest.Point.Y;
            this.Magnitude = pointOfInterest.RidgeMagnitude;
            this.Orientation = (Direction)pointOfInterest.OrientationCategory;
            this.FrameMaximum = sonogram.FrameCount;
            this.BinMaximum = sonogram.Configuration.FreqBinCount;

            this.EventStartSeconds = pointOfInterest.TimeLocation.TotalSeconds;
            this.FileName = analysisSettings.SourceFile.FullName;

        }

        public int BinMaximum { get; set; }

        public int Frame { get; set; }

        public int Bin { get; set; }

        public double Magnitude { get; set; }

        public Direction Orientation { get; set; }

        public int FrameMaximum { get; set; }
    }

    public class RidgeAnalysis : AbstractStrongAnalyser
    {
        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            var audioFile = analysisSettings.AudioFile;
            var startOffset = analysisSettings.SegmentStartOffset ?? TimeSpan.Zero;

            var recording = new AudioRecording(audioFile.FullName);

            var result = new AnalysisResult2(analysisSettings, recording.Duration())
                         {
                             AnalysisIdentifier = this.Identifier,
                         };

            if (recording.SampleRate != 22050)
            {
                throw new NotSupportedException();
            }

            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.Standard, WindowOverlap = 0.5};
            var sonogram = new SpectrogramStandard(config, recording.WavReader);

            // This config is to set up the parameters used in ridge Detection, the parameters can be changed.
            var ridgeConfig = new RidgeDetectionConfiguration {
                RidgeDetectionmMagnitudeThreshold = 5.5,
                RidgeMatrixLength = 5,
                FilterRidgeMatrixLength = 7,
                MinimumNumberInRidgeInMatrix = 3,
            };

            var ridges = POISelection.RidgeDetection(sonogram, ridgeConfig);

            if (ridges.IsNullOrEmpty())
            {
                return result;
            }

            result.Events = new RidgeEvent[ridges.Count];
            for (int index = 0; index < ridges.Count; index++)
            {
                ((RidgeEvent[])result.Events)[index] = new RidgeEvent(ridges[index], analysisSettings, sonogram);
            }

            if (analysisSettings.EventsFile != null)
            {
                this.WriteEventsFile(analysisSettings.EventsFile, result.Events);
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                result.SummaryIndices = this.ConvertEventsToSummaryIndices(result.Events, unitTime, result.SegmentAudioDuration, 0);

                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, result.SummaryIndices);
            }

            if (analysisSettings.SegmentSaveBehavior.ShouldSave(result.Events.Length))
            {
                throw new NotImplementedException();
            }

            return result;
        }

        public IEnumerable<SummaryIndexBase> ProcessCsvFile(FileInfo csvFile, FileInfo configFile)
        {
            throw new NotImplementedException();
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<RidgeEvent>());
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<EventIndex>());
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // no-op
        }

        public override string DisplayName
        {
            get
            {
                return "Ridge Detection";
            }
        }

        public override string Identifier
        {
            get
            {
                return "Dong.RidgeDetection";
            }
        }

        public override AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings()
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(20),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = 22050,
                };
            }
        }



    }
}
