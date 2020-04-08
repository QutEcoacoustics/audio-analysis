// <copyright file="SpectrogramGenerator.Analyzer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SpectrogramGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;

    /// <summary>
    /// This analyzer simply generates short (i.e. one minute) spectrograms and outputs them to CSV files.
    /// It does not accumulate data or other indices over a long recording.
    /// </summary>
    public partial class SpectrogramGenerator : IAnalyser2
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SpectrogramGenerator()
        {
            this.DisplayName = "Spectrogram Analyzer";
            this.Identifier = "Towsey.SpectrogramGenerator";
            this.DefaultSettings = new AnalysisSettings()
            {
                AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
                AnalysisMinSegmentDuration = TimeSpan.FromSeconds(20),
                SegmentMediaType = MediaTypes.MediaTypeWav,
                SegmentOverlapDuration = TimeSpan.Zero,
            };
        }

        public string DisplayName { get; private set; }

        public string Identifier { get; private set; }

        public string Description => "This analyzer simply generates short (i.e. one minute) spectrograms and outputs them to CSV files. It does not accumulate data or other indices over a long recording.";

        public AnalysisSettings DefaultSettings { get; private set; }

        public AnalyzerConfig ParseConfig(FileInfo file)
        {
            var config = ConfigFile.Deserialize<SpectrogramGeneratorConfig>(file);

            if (config.SaveSonogramImages != SaveBehavior.Always)
            {
                Log.Warn($"The spectrogram generator is not configured to save any images! Set `{nameof(SpectrogramGeneratorConfig.SaveSonogramImages)}` to `{SaveBehavior.Always}` to save the spectrograms!");
            }

            if (config.Images.IsNullOrEmpty())
            {
                Log.Warn($"The spectrogram generator is not configured to produce any image types!"
                         + $" Set `{nameof(SpectrogramGeneratorConfig.Images)}` to any of `{typeof(SpectrogramImageType).PrintEnumOptions()}`."
                         + " Choosing decibel spectrogram noise reduced only by default!");
            }

            return config;
        }

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var sourceRecordingName = recording.BaseName;
            var configuration = (SpectrogramGeneratorConfig)analysisSettings.Configuration;

            var analysisResult = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);

            var spectrogramResult = GenerateSpectrogramImages(audioFile, configuration, sourceRecordingName);

            // this analysis produces no results! But we still print images (that is the point)
            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResult.Events.Length))
            {
                ImageExtensions.Save(spectrogramResult.CompositeImage, segmentSettings.SegmentImageFile.FullName);
            }

            //if (saveCsv)
            //{
            //    var basename = Path.GetFileNameWithoutExtension(segmentSettings.SegmentAudioFile.Name);
            //    var spectrogramCsvFile = outputDirectory.CombineFile(basename + ".Spectrogram.csv");
            //    Csv.WriteMatrixToCsv(spectrogramCsvFile, spectrogramResult.DecibelSpectrogram.Data, TwoDimensionalArray.None);
            //}

            return analysisResult;
        }

        public void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        public void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // no-op
        }
    }
}