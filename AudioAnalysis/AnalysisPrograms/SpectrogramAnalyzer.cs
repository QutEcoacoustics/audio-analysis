// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrogramAnalyzer.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the SpectrogramAnalyzer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools;
    using AudioAnalysisTools.WavTools;

    /// <summary>
    /// This analyzer simply generates spectrograms and outputs them to CSV files.
    /// </summary>
    public class SpectrogramAnalyzer : IAnalyser2
    {
        public SpectrogramAnalyzer()
        {
            this.DisplayName = "Spectrogram Analyzer";
            this.Identifier = "SpectrogramAnalyzer";
            this.DefaultSettings = new AnalysisSettings()
                                       {
                                           SegmentMaxDuration = TimeSpan.FromMinutes(1),
                                           SegmentMinDuration = TimeSpan.FromSeconds(20),
                                           SegmentMediaType = MediaTypes.MediaTypeWav,
                                           SegmentOverlapDuration = TimeSpan.Zero
                                       };
        }

        public string DisplayName { get; private set; }

        public string Identifier { get; private set; }

        public AnalysisSettings DefaultSettings { get; private set; }

        public AnalysisResult2 Analyse(AnalysisSettings analysisSettings)
        {
            var audioFile = analysisSettings.AudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResult = new AnalysisResult2(analysisSettings, recording.Duration());
            dynamic configuration = Yaml.Deserialise(analysisSettings.ConfigFile);

            bool saveCsv = (bool?) configuration[AnalysisKeys.SaveIntermediateCsvFiles] ?? false;

            // generate spectrogram
            // TODO: Just call michael's new spectrogram generation code when that is merged into master!
            double[,] spectrogram;

            // this analysis produces no results!
            // but we still print images (that is the point)
            if (analysisSettings.ImageFile != null)
            {
                //image.Save(analysisSettings.ImageFile, ImageFormat.Png);
            }

            if (saveCsv)
            {
                var basename = Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name);
                var spectrogramCsvFile = outputDirectory.CombineFile(basename + ".Spectrogram.csv");
                Csv.WriteMatrixToCsv(spectrogramCsvFile, spectrogram);
            }

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

        public void WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(
            IEnumerable<EventBase> events,
            TimeSpan unitTime,
            TimeSpan duration,
            double scoreThreshold)
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
            throw new NotImplementedException();
        }
    }
}
