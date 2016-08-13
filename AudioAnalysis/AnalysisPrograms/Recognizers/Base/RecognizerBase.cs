using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Recognizers.Base
{
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools;
    using AudioAnalysisTools.WavTools;

    using TowseyLibrary;

    public abstract class RecognizerBase : AbstractStrongAnalyser, IEventRecognizer
    {
        public abstract string Author { get; }

        public override string Identifier => this.Author + "." + this.DisplayName;

        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            FileInfo audioFile = analysisSettings.AudioFile;
            var recording = new AudioRecording(audioFile.FullName);

            // execute actual analysis
            dynamic configuration = analysisSettings.Configuration;
            RecognizerResults results = this.Recognize(
                recording,
                analysisSettings.Configuration,
                analysisSettings.SegmentStartOffset.Value,
                (Func<WavReader, IEnumerable<SpectralIndexBase>>)(this.GetSpectralIndexes));

            var analysisResults = new AnalysisResult2(analysisSettings, recording.Duration());

            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            Plot scores = results.Plot;
            var predictedEvents = results.Events;

            analysisResults.Events = predictedEvents.ToArray();

            if (analysisSettings.EventsFile != null)
            {
                this.WriteEventsFile(analysisSettings.EventsFile, analysisResults.Events);
                analysisResults.EventsFile = analysisSettings.EventsFile;
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(analysisResults.Events, unitTime, analysisResults.SegmentAudioDuration, 0);

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

            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                const double EventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, EventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;
            }

            return analysisResults;
        }

        private IEnumerable<SpectralIndexBase> GetSpectralIndexes(WavReader WavReader)
        {
            throw new NotImplementedException();
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
            var selectors = results.First().GetSelectors();

            var spectralIndexFiles = new List<FileInfo>(selectors.Count);

            foreach (var kvp in selectors)
            {
                // write spectrogram to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultName(destination, fileNameBase, this.Identifier + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, results, kvp.Value);
            }

            return spectralIndexFiles;
        }

        public abstract RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Func<WavReader, IEnumerable<SpectralIndexBase>> getSpectralIndexes);

        protected virtual Image DrawSonogram(
            BaseSonogram sonogram,
            double[,] hits,
            Plot scores,
            List<AcousticEvent> predictedEvents,
            double eventThreshold)
        {
            const bool DoHighlightSubband = false;
            const bool Add1KHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(DoHighlightSubband, Add1KHzLines));

            ////System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            ////img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            ////Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null)
            {
                image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
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

            return image.GetImage();
        }
    }
}
