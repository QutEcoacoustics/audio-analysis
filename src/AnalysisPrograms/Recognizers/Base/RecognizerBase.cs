// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognizerBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the RecognizerBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;

    public abstract class RecognizerBase : AbstractStrongAnalyser, IEventRecognizer
    {
        public class RecognizerConfig : AnalyzerConfig
        {
            public RecognizerConfig()
            {
                this.Loaded += config =>
                    {
                        var file = ConfigFile.Resolve(this.HighResolutionIndicesConfig);
                        var indicesConfig = ConfigFile.Deserialize<AcousticIndices.AcousticIndicesConfig>(file);
                        this.HighResolutionIndices = indicesConfig;
                    };
            }

            public string HighResolutionIndicesConfig { get; set; }

            public AcousticIndices.AcousticIndicesConfig HighResolutionIndices { get; private set; }
        }

        public abstract string Author { get; }

        public abstract string SpeciesName { get; }

        public override string Identifier => this.Author + "." + this.SpeciesName;

        public override string DisplayName => this.SpeciesName;

        public override AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
            AnalysisMinSegmentDuration = TimeSpan.FromSeconds(1),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
            AnalysisTargetSampleRate = AppConfigHelper.DefaultTargetSampleRate,
        };

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            return ConfigFile.Deserialize<RecognizerConfig>(file);
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var recording = new AudioRecording(segmentSettings.SegmentAudioFile.FullName);

            // get indices configuration - extracted in BeforeAnalyze
            var acousticIndicesConfig = (RecognizerConfig)analysisSettings.Configuration;

            // get a lazily calculated indices function - if you never get the lazy value, the indices will never be calculated
            var lazyIndices = this.GetLazyIndices(
                recording,
                analysisSettings,
                segmentSettings,
                acousticIndicesConfig.HighResolutionIndices);

            // determine imageWidth for output images
            int imageWidth = (int)Math.Floor(
                recording.Duration.TotalSeconds
                / acousticIndicesConfig.HighResolutionIndices.IndexCalculationDuration);

            // execute actual analysis
            RecognizerResults results = this.Recognize(
                recording,
                analysisSettings.Configuration,
                segmentSettings.SegmentStartOffset,
                lazyIndices,
                segmentSettings.SegmentOutputDirectory,
                imageWidth);

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);

            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            var predictedEvents = results.Events;

            // double check all the events have the right offset in case it was missed
            foreach (var predictedEvent in predictedEvents)
            {
                predictedEvent.SegmentStartSeconds = segmentSettings.SegmentStartOffset.TotalSeconds;
            }

            analysisResults.Events = predictedEvents.ToArray();

            // compress high resolution indices - and save them.
            // IF they aren't used, empty values are returned.
            if (lazyIndices.IsValueCreated)
            {
                this.SummarizeHighResolutionIndices(
                    analysisResults,
                    lazyIndices.Value,
                    acousticIndicesConfig.HighResolutionIndices);
            }

            // write intermediate output if necessary
            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteEventsFile(segmentSettings.SegmentEventsFile, analysisResults.Events);
                analysisResults.EventsFile = segmentSettings.SegmentEventsFile;
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                analysisResults.SpectraIndicesFiles =
                    this.WriteSpectrumIndicesFiles(
                        segmentSettings.SegmentSpectrumIndicesDirectory,
                        segmentSettings.Segment.SourceMetadata.Identifier,
                        analysisResults.SpectralIndices);
            }

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                string imagePath = segmentSettings.SegmentImageFile.FullName;
                const double EventThreshold = 0.1;
                var plots = results.Plots ?? new List<Plot>();

                Image image = this.DrawSonogram(sonogram, hits, plots, predictedEvents, EventThreshold);
                image.Save(imagePath);
                analysisResults.ImageFile = segmentSettings.SegmentImageFile;

                // draw a fancy high res index image
                // IF indices aren't used, no image is drawn.
                if (lazyIndices.IsValueCreated)
                {
                    this.DrawLongDurationSpectrogram(
                        segmentSettings.SegmentOutputDirectory,
                        recording.BaseName,
                        results.ScoreTrack,
                        lazyIndices.Value,
                        acousticIndicesConfig.HighResolutionIndices);
                }
            }

            return analysisResults;
        }

        private void DrawLongDurationSpectrogram(
            DirectoryInfo outputDirectory,
            string fileStem,
            Image<Rgb24> scoreTrack,
            IndexCalculateResult[] indexResults,
            AcousticIndices.AcousticIndicesConfig acousticIndicesConfig)
        {
            var dictionaryOfSpectra = indexResults.Select(icr => icr.SpectralIndexValues).ToArray().ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.Rotate90ClockWise);

            FileInfo ipConfig = acousticIndicesConfig.IndexPropertiesConfig.ToFileInfo();
            double hiResScale = acousticIndicesConfig.IndexCalculationDuration;
            TimeSpan hiResTimeScale = TimeSpan.FromSeconds(hiResScale);

            FileInfo spectrogramConfig = ConfigFile.Resolve(acousticIndicesConfig["SpectrogramConfig"]);

            // Assemble arguments for drawing the GRAY-SCALE and RIDGE SPECTROGRAMS
            var output = outputDirectory.Combine("SpectrogramImages");
            var ldfcSpectrogramArguments = new DrawLongDurationSpectrograms.Arguments
            {
                // passed null for first InputDataDirectory on purpose: we don't want to read files off disk
                InputDataDirectory = null,
                OutputDirectory = output.FullName,
                FalseColourSpectrogramConfig = spectrogramConfig.FullName,
                IndexPropertiesConfig = ipConfig.FullName,
                ColourMap1 = "BGN-DMN-EVN",
                ColourMap2 = "RHZ-RVT-SPT", //R3D replaces PHN as new derived index
                TemporalScale = hiResTimeScale,
            };

            bool saveRidgeSpectrograms = acousticIndicesConfig.GetBoolOrNull("SaveRidgeSpectrograms") ?? false;
            if (saveRidgeSpectrograms)
            {
                // 1: DRAW the coloured ridge spectrograms

                // passed null for first argument on purpose: we don't want to read files off disk
                var ridgeSpectrogram = DrawLongDurationSpectrograms.DrawRidgeSpectrograms(null, ipConfig, fileStem, hiResScale, dictionaryOfSpectra);

                //var opImages = new List<Image>();
                //opImages.Add(ridgeSpectrogram);
                //opImages.Add(scoreTrackImage);
                // combine and save
                //Image opImage = ImageTools.CombineImagesVertically(opImages);

                var fileName = FilenameHelpers.AnalysisResultPath(output, fileStem, "Ridges", ".png");

                //opImage.Save(fileName);
                ridgeSpectrogram.Save(fileName);
            } // if (saveRidgeSpectrograms)

            // 2. DRAW the aggregated GREY-SCALE SPECTROGRAMS of SPECTRAL INDICES
            Image<Rgb24> opImage;
            bool saveGrayScaleSpectrograms = acousticIndicesConfig.GetBoolOrNull("SaveGrayScaleSpectrograms") ?? false;
            if (saveGrayScaleSpectrograms)
            {
                opImage = DrawLongDurationSpectrograms.DrawGrayScaleSpectrograms(ldfcSpectrogramArguments, fileStem, hiResTimeScale, dictionaryOfSpectra);
                var fileName = FilenameHelpers.AnalysisResultPath(output, fileStem, "CombinedGreyScale", ".png");
                opImage.Save(fileName);
            }

            // 3. DRAW False-colour Spectrograms
            bool saveTwoMapsSpectrograms = acousticIndicesConfig.GetBoolOrNull("SaveTwoMapsSpectrograms") ?? false;
            if (saveTwoMapsSpectrograms)
            {
                opImage = DrawLongDurationSpectrograms.DrawFalseColourSpectrograms(ldfcSpectrogramArguments, fileStem, dictionaryOfSpectra);
                var opImages = new [] { opImage, scoreTrack };
                opImage = ImageTools.CombineImagesVertically(opImages);
                var fileName = FilenameHelpers.AnalysisResultPath(output, fileStem, "TwoMaps", ".png");
                opImage.Save(fileName);
            }
        }

        /// <summary>
        /// Compress high resolution indices - intended to be used when summarizing results.
        /// Summarize method not yet written.
        /// </summary>
        /// <param name="analysisResults"></param>
        /// <param name="indexResults"></param>
        /// <param name="highResolutionParsedConfiguration"></param>
        private void SummarizeHighResolutionIndices(
            AnalysisResult2 analysisResults,
            IndexCalculateResult[] indexResults,
            AcousticIndices.AcousticIndicesConfig highResolutionParsedConfiguration)
        {
            // NOW COMPRESS THE HI-RESOLUTION SPECTRAL INDICES TO LOW RES
            double lowResolution = highResolutionParsedConfiguration.GetDoubleOrNull("LowResolution") ?? 60.0;
            TimeSpan imageScale = TimeSpan.FromSeconds(lowResolution);
            TimeSpan dataScale = highResolutionParsedConfiguration.IndexCalculationDuration.Seconds();

            var dictionaryOfSpectra = indexResults.Select(icr => icr.SpectralIndexValues).ToArray().ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.Rotate90ClockWise);

            var spectralSelection = IndexMatrices.CompressIndexSpectrograms(dictionaryOfSpectra, imageScale, dataScale);

            // check that have not compressed matrices to zero length
            double[,] matrix = spectralSelection.First().Value;
            if (matrix.GetLength(0) == 0 || matrix.GetLength(1) == 0)
            {
                LoggedConsole.WriteErrorLine("WARNING: SPECTRAL INDEX MATRICES compressed to zero length!!!!!!!!!!!!!!!!!!!!!!!!");
            }

            // Place LOW RESOLUTION SPECTRAL INDICES INTO analysisResults before returning.
            //int windowLength = (int?)highResolutionConfig[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
            var indexProperties = highResolutionParsedConfiguration.IndexProperties;
            SpectralIndexValues.CheckExistenceOfSpectralIndexValues(indexProperties);

            // Init a new spectral indices class and populate it with spectral indices
            var spectrums = SpectralIndexValues.ImportFromDictionary(spectralSelection);
            for (int i = 0; i < spectrums.Length; i++)
            {
                spectrums[i].ResultStartSeconds = (analysisResults.SegmentStartOffset + TimeSpan.FromSeconds(i * lowResolution)).TotalSeconds;
                spectrums[i].SegmentDurationSeconds = imageScale.TotalSeconds;
                spectrums[i].FileName = ((SegmentSettings<object>)analysisResults.SegmentSettings).Segment.SourceMetadata.Identifier;
            }

            // assign to the analysis result
            analysisResults.SpectralIndices = spectrums;

            // TODO TODO TODO
            // ALSO NEED TO COMPRESS THE analysisResults.SummaryIndices To LOW RESOLUTION
            //var summaryIndexValues = new SummaryIndexValues();
            //summaryIndexValues.BackgroundNoise = ETC;
            // ETC
            //var summaryiv = new SummaryIndexValues[1];
            //summaryiv[0] = summaryIndexValues;
            //analysisResults.SummaryIndices = summaryiv;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results.Select(x => (AcousticEvent)x));
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            if (!results.Any())
            {
                return null;
            }

            var selectors = results.First().GetSelectors();

            var spectralIndexFiles = new List<FileInfo>(selectors.Count);

            foreach (var kvp in selectors)
            {
                // write spectrum to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultPath(destination, fileNameBase, this.Identifier + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, results, kvp.Value);
            }

            return spectralIndexFiles;
        }

        /// <inheritdoc />
        public abstract RecognizerResults Recognize(AudioRecording audioRecording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth);

        protected virtual Image DrawSonogram(
            BaseSonogram sonogram,
            double[,] hits,
            List<Plot> scores,
            List<AcousticEvent> predictedEvents,
            double eventThreshold)
        {
            var image = SpectrogramTools.GetSonogramPlusCharts(sonogram, predictedEvents, scores, hits);
            return image;
        }

        /// <summary>
        /// Run once before each segment of analysis.
        /// </summary>
        /// <param name="analysisSettings">Settings used for the analysis.</param>
        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // no operation
            // TODO: michael edit as you like

            // called once after all analysis segments have been completed
        }

        private Lazy<IndexCalculateResult[]> GetLazyIndices<T>(
            AudioRecording recording,
            AnalysisSettings analysisSettings,
            SegmentSettings<T> segmentSettings,
            AcousticIndices.AcousticIndicesConfig acousticConfiguration)
        {
            // Convert the Config config to IndexCalculateConfig class and merge in the unnecesary parameters.

            IndexCalculateResult[] Callback()
            {
                IndexCalculateResult[] subsegmentResults = AcousticIndices.CalculateIndicesInSubsegments(
                    recording,
                    segmentSettings.SegmentStartOffset,
                    segmentSettings.AnalysisIdealSegmentDuration,
                    acousticConfiguration.IndexCalculationDuration.Seconds(),
                    acousticConfiguration.IndexProperties,
                    segmentSettings.Segment.SourceMetadata.SampleRate,
                    acousticConfiguration);

                return subsegmentResults;
            }

            return new Lazy<IndexCalculateResult[]>(Callback, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static Image<Rgb24> DrawDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            const bool doHighlightSubband = false;
            const bool add1KHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1KHzLines, doMelScale: false));

            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (var plot in scores)
                {
                    image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
                }
            }

            if (hits != null)
            {
                image.OverlayRainbowTransparency(hits);
            }

            if (events.Count > 0)
            {
                foreach (var ev in events) // set colour for the events
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }

                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }

            return image.GetImage().CloneAs<Rgb24>();
        }
    }
}
