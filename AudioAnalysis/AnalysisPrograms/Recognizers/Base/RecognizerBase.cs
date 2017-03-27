// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognizerBase.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the RecognizerBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;

    using TowseyLibrary;

    public abstract class RecognizerBase : AbstractStrongAnalyser, IEventRecognizer
    {

        public abstract string Author { get; }

        public abstract string SpeciesName { get; }

        public override string Identifier => this.Author + "." + this.SpeciesName;

        public override string DisplayName => this.SpeciesName;

        public override AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            SegmentMaxDuration = TimeSpan.FromMinutes(1),
            SegmentMinDuration = TimeSpan.FromSeconds(1),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
            SegmentTargetSampleRate = AppConfigHelper.DefaultTargetSampleRate,
        };

        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            FileInfo audioFile = analysisSettings.AudioFile;
            var recording = new AudioRecording(audioFile.FullName);

            // get indices configuration - extracted in BeforeAnalyze
            var acousticIndicesParsedConfiguration = (Acoustic.AcousticIndicesParsedConfiguration)analysisSettings.AnalyzerSpecificConfiguration;

            // get a lazily calculated indices function - if you never get the lazy value, the indices will never be calculated
            var lazyIndices = this.GetLazyIndices(recording, analysisSettings, acousticIndicesParsedConfiguration);

            // determine imageWidth for output images
            int imageWidth = (int)Math.Floor(recording.Duration().TotalSeconds / acousticIndicesParsedConfiguration.IndexCalculationDuration.TotalSeconds);

            // execute actual analysis
            dynamic configuration = analysisSettings.Configuration;
            RecognizerResults results = this.Recognize(
                recording,
                analysisSettings.Configuration,
                analysisSettings.SegmentStartOffset.Value,
                lazyIndices,
                analysisSettings.AnalysisInstanceOutputDirectory,
                imageWidth);

            var analysisResults = new AnalysisResult2(analysisSettings, recording.Duration());

            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            var predictedEvents = results.Events;

            foreach (var predictedEvent in predictedEvents)
            {
                predictedEvent.SegmentStartOffset = analysisSettings.SegmentStartOffset.Value;
            }

            analysisResults.Events = predictedEvents.ToArray();

            // convert events to summary index values
            // Not needed: this is done by AnalyzeLongRecording.cs#259
            //analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(analysisResults.Events, analysisSettings.SegmentMaxDuration.Value, analysisResults.SegmentAudioDuration, 0);

            // compress high resolution indices - and save them.
            // IF they aren't used, empty values are returned.
            if (lazyIndices.IsValueCreated)
            {
                this.SummarizeHighResolutionIndices(analysisResults, lazyIndices.Value, acousticIndicesParsedConfiguration);
            }

            // write intermediate output if necessary
            if (analysisSettings.EventsFile != null)
            {
                this.WriteEventsFile(analysisSettings.EventsFile, analysisResults.Events);
                analysisResults.EventsFile = analysisSettings.EventsFile;
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
            }

            if (analysisSettings.SpectrumIndicesDirectory != null)
            {
                analysisResults.SpectraIndicesFiles =
                    this.WriteSpectrumIndicesFiles(
                        analysisSettings.SpectrumIndicesDirectory,
                        Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name),
                        analysisResults.SpectralIndices);
            }

            if (analysisSettings.SegmentSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                const double EventThreshold = 0.1;
                var plots = results.Plots ?? new List<Plot>();

                Image image = this.DrawSonogram(sonogram, hits,plots, predictedEvents, EventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;

                // draw a fancy high res index image
                // IF indices aren't used, no image is drawn.
                if (lazyIndices.IsValueCreated)
                {
                    this.DrawLongDurationSpectrogram(
                        analysisSettings.AnalysisInstanceOutputDirectory,
                        recording.BaseName,
                        results.ScoreTrack,
                        lazyIndices.Value,
                        acousticIndicesParsedConfiguration);
                }
            }

            return analysisResults;
        }

        private void DrawLongDurationSpectrogram(
            DirectoryInfo outputDirectory,
            string fileStem,
            Image scoreTrack,
            IndexCalculateResult[] indexResults,
            Acoustic.AcousticIndicesParsedConfiguration acousticIndicesParsedConfiguration)
        {
            dynamic highResolutionConfiguration = acousticIndicesParsedConfiguration.Configuration;
            var dictionaryOfSpectra = indexResults.Select(icr => icr.SpectralIndexValues).ToArray().ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);

            FileInfo ipConfig = acousticIndicesParsedConfiguration.IndexPropertiesFile;
            double hiResScale = acousticIndicesParsedConfiguration.IndexCalculationDuration.TotalSeconds;
            TimeSpan hiResTimeScale = TimeSpan.FromSeconds((double)hiResScale);

            FileInfo spectrogramConfig = ConfigFile.ResolveConfigFile((string)highResolutionConfiguration.SpectrogramConfig);

            // Assemble arguments for drawing the GRAY-SCALE and RIDGE SPECTROGRAMS
            var ldfcSpectrogramArguments = new DrawLongDurationSpectrograms.Arguments
            {
                // passed null for first InputDataDirectory on purpose: we don't want to read files off disk
                InputDataDirectory = null,
                OutputDirectory = outputDirectory.Combine("SpectrogramImages"),
                SpectrogramConfigPath = spectrogramConfig,
                IndexPropertiesConfig = acousticIndicesParsedConfiguration.IndexPropertiesFile,
                ColourMap1 = "BGN-POW-EVN",
                ColourMap2 = "PHN-RVT-SPT", //PHN is new derived index
                                            //ColourMap2 = "RHZ-RPS-RNG",

                TemporalScale = hiResTimeScale,
            };
            // Create output directory if it does not exist
            if (!ldfcSpectrogramArguments.OutputDirectory.Exists)
            {
                ldfcSpectrogramArguments.OutputDirectory.Create();
            }


            bool saveRidgeSpectrograms = (bool?)highResolutionConfiguration["SaveRidgeSpectrograms"] ?? false;
            if (saveRidgeSpectrograms)
            {
                // 1: DRAW the coloured ridge spectrograms

                // passed null for first argument on purpose: we don't want to read files off disk
                var ridgeSpectrogram = DrawLongDurationSpectrograms.DrawRidgeSpectrograms(null, ipConfig, fileStem, (double)hiResScale, dictionaryOfSpectra);
                //var opImages = new List<Image>();
                //opImages.Add(ridgeSpectrogram);
                //opImages.Add(scoreTrackImage);
                // combine and save
                //Image opImage = ImageTools.CombineImagesVertically(opImages);

                var fileName = FilenameHelpers.AnalysisResultPath(ldfcSpectrogramArguments.OutputDirectory, fileStem, "Ridges", ".png");
                //opImage.Save(fileName);
                ridgeSpectrogram.Save(fileName);
            } // if (saveRidgeSpectrograms)

            // 2. DRAW the aggregated GREY-SCALE SPECTROGRAMS of SPECTRAL INDICES
            Image opImage;
            bool saveGrayScaleSpectrograms = (bool?)highResolutionConfiguration["SaveGrayScaleSpectrograms"] ?? false;
            if (saveGrayScaleSpectrograms)
            {
                opImage = DrawLongDurationSpectrograms.DrawGrayScaleSpectrograms(ldfcSpectrogramArguments, fileStem, hiResTimeScale, dictionaryOfSpectra);
                var fileName = FilenameHelpers.AnalysisResultPath(ldfcSpectrogramArguments.OutputDirectory, fileStem, "CombinedGreyScale", ".png");
                opImage.Save(fileName);
            }

            // 3. DRAW False-colour Spectrograms
            bool saveTwoMapsSpectrograms = (bool?)highResolutionConfiguration["SaveTwoMapsSpectrograms"] ?? false;
            if (saveTwoMapsSpectrograms)
            {
                opImage = DrawLongDurationSpectrograms.DrawFalseColourSpectrograms(ldfcSpectrogramArguments, fileStem, dictionaryOfSpectra);
                var opImages = new List<Image> {opImage, scoreTrack};
                opImage = ImageTools.CombineImagesVertically(opImages);
                var fileName = FilenameHelpers.AnalysisResultPath(ldfcSpectrogramArguments.OutputDirectory, fileStem, "TwoMaps", ".png");
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
            Acoustic.AcousticIndicesParsedConfiguration highResolutionParsedConfiguration)
        {
            dynamic highResolutionConfig = highResolutionParsedConfiguration.Configuration;

            // NOW COMPRESS THE HI-RESOLUTION SPECTRAL INDICES TO LOW RES
            double lowResolution = (double?)highResolutionConfig["LowResolution"] ?? 60.0;
            TimeSpan imageScale = TimeSpan.FromSeconds(lowResolution);
            TimeSpan dataScale = highResolutionParsedConfiguration.IndexCalculationDuration;

            var dictionaryOfSpectra = indexResults.Select(icr => icr.SpectralIndexValues).ToArray().ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);

            var spectralSelection = IndexMatrices.CompressIndexSpectrograms(dictionaryOfSpectra, imageScale, dataScale);
            // check that have not compressed matrices to zero length
            double[,] matrix = spectralSelection.First().Value;
            if ((matrix.GetLength(0) == 0) || (matrix.GetLength(1) == 0))
            {
                LoggedConsole.WriteErrorLine("WARNING: SPECTRAL INDEX MATRICES compressed to zero length!!!!!!!!!!!!!!!!!!!!!!!!");
            }

            // Place LOW RESOLUTION SPECTRAL INDICES INTO analysisResults before returning.
            //int windowLength = (int?)highResolutionConfig[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
            var indexProperties = IndexProperties.GetIndexProperties(highResolutionParsedConfiguration.IndexPropertiesFile);
            SpectralIndexValues.CheckExistenceOfSpectralIndexValues(indexProperties);

            // Init a new spectral indices class and populate it with spectral indices
            var spectrums = SpectralIndexValues.ImportFromDictionary(spectralSelection);
            for (int i = 0; i < spectrums.Length; i++)
            {
                spectrums[i].StartOffset = analysisResults.SegmentStartOffset + TimeSpan.FromSeconds(i * lowResolution);
                spectrums[i].SegmentDuration = imageScale;
                spectrums[i].FileName = analysisResults.SettingsUsed.SourceFile.Name;
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
            Csv.WriteToCsv(destination, results);
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
        public abstract RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth);

        protected virtual Image DrawSonogram(
            BaseSonogram sonogram,
            double[,] hits,
            List<Plot> scores,
            List<AcousticEvent> predictedEvents,
            double eventThreshold)
        {
            const bool doHighlightSubband = false;
            const bool add1KHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1KHzLines));

            ////System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            ////img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            ////Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            if (scores != null)
            {
                foreach (var plot in scores)
                {
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title));
                }
            }

            if (hits != null)
            {
                image.OverlayRedTransparency(hits);
            }

            if ((predictedEvents != null) && (predictedEvents.Count > 0))
            {
                image.AddEvents(
                    predictedEvents,
                    sonogram.NyquistFrequency,
                    sonogram.Configuration.FreqBinCount,
                    sonogram.FramesPerSecond);
            }

            var result =  image.GetImage();

            return result;
        }


        /// <summary>
        /// Run once before each segment of analysis
        /// </summary>
        /// <param name="analysisSettings"></param>
        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            var configuration = analysisSettings.Configuration;

            FileInfo indicesConfigFile = ConfigFile.ResolveConfigFile(
                (string)configuration.HighResolutionIndicesConfig,
                analysisSettings.ConfigFile.Directory
            );

            var indicesConfiguration = Yaml.Deserialise(indicesConfigFile);

            // extract settings for generating indices
            var acousticConfiguration = Acoustic.AcousticIndicesParsedConfiguration.FromConfigFile(
                indicesConfiguration,
                indicesConfigFile,
                analysisSettings.SegmentMaxDuration.Value);

            analysisSettings.AnalyzerSpecificConfiguration = acousticConfiguration;
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

        private Lazy<IndexCalculateResult[]> GetLazyIndices(AudioRecording recording, AnalysisSettings analysisSettings, Acoustic.AcousticIndicesParsedConfiguration acousticConfiguration)
        {
            Func<IndexCalculateResult[]> callback = () =>
                {
                    IndexCalculateResult[] subsegmentResults = Acoustic.CalculateIndicesInSubsegments(
                      recording,
                      analysisSettings.SegmentStartOffset.Value,
                      analysisSettings.SegmentDuration.Value,
                      acousticConfiguration.IndexCalculationDuration,
                      acousticConfiguration.BgNoiseNeighborhood,
                      acousticConfiguration.IndexPropertiesFile,
                      analysisSettings.SampleRateOfOriginalAudioFile.Value,
                      analysisSettings.Configuration);

                    return subsegmentResults;
                };
            return new Lazy<IndexCalculateResult[]>(callback, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static Image DrawDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            const bool doHighlightSubband = false;
            const bool add1KHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1KHzLines));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (var plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);

            if (events.Count > 0)
            {
                foreach (var ev in events) // set colour for the events
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }
                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }
            return image.GetImage();
        }
    }
}
