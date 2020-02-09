// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KoalaMale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    /// <summary>
    ///  NOTES:
    /// (1) The main part of a male koala call consists of a series of inhlations and exhalations;
    ///     The inhalations are longer and sound like snoring. The exhalations are shorter and the sound is similar to belching.
    ///     For more on the koala bellow see http://theconversation.com/grunt-work-unique-vocal-folds-give-koalas-their-low-pitched-voice-20800
    ///     The article interviews Dr. Ben Charlton who came to work with us in 2012.
    ///
    /// (2) This class detects male koala calls by detecting the characteristic oscillations of their snoring or inhalations.
    ///     These snoring oscillations = approx 20-50 per second.
    ///     They are not constant but tend to increase in rate through the inhalation.
    ///
    /// (3) In order to detect 50 oscillations/sec, we need at the very least 100 frames/sec and preferably a frame rate = 150/sec
    ///        so that a period = 50/s sits near the middle of the array of DCT coefficients.
    ///
    /// (4) Frame rate is affected by three parameters: 1) SAMPLING RATE; 2) FRAME LENGTH; 3) FRAME OVERLAP.
    ///     If the SR ~= 170640, the FRAME LENGTH should = 256 or 512.
    ///     The best way to adjust frame rate is to adjust frame overlap. I finally decided on the option of automatically calculating the frame overlap
    ///     to suit the maximum oscillation to be detected.
    ///     This calculation is done by the method OscillationDetector.CalculateRequiredFrameOverlap();
    ///
    /// (5) One should not set the DCT length to be too long because (1) the DCT is expensive to calculate.
    ///      and (2) the koala oscillation is not constant but the DCT assumes stationarity. 0.3s is good for koala. 0.5s - 1.0s is OK for canetoad.
    ///
    /// (6) To reduce the probability of false-positives, the Koala Recognizer filters out oscillation events
    ///     that are not accompanied by neighbouring oscillation events within 4 seconds.
    ///     This filtering is done in the method KoalaMale.FilterMaleKoalaEvents().
    ///
    /// The action code for this analysis (to enter on the command line) is "KoalaMale".
    /// </summary>
    public class KoalaMale : AbstractStrongAnalyser
    {
        public const string AnalysisName = "KoalaMale";

        public const int ResampleRate = 17640;

        public override string Description => "[BETA/EXPERIMENTAL] Recogniser for male koalla bellow. Detects inhalation oscillations.";

        public string DefaultConfiguration => string.Empty;

        public override AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
            AnalysisMinSegmentDuration = TimeSpan.FromSeconds(30),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
            AnalysisTargetSampleRate = ResampleRate,
        };

        public override string DisplayName => "Koala Male";

        public override string Identifier => "Towsey." + AnalysisName;

        public static KoalaMaleResults Analysis(FileInfo segmentOfSourceFile, IDictionary<string, string> configDict, TimeSpan segmentStartOffset)
        {
            var recording = new AudioRecording(segmentOfSourceFile.FullName);
            var results = Analysis(recording, configDict, segmentStartOffset);
            return results;
        }

        /// <summary>
        /// THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="segmentOfSourceFile">
        ///     The file to process.
        /// </param>
        /// <param name="configDict">
        ///     The configuration for the analysis.
        /// </param>
        /// <param name="value"></param>
        /// <param name="segmentStartOffset"></param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        public static KoalaMaleResults Analysis(AudioRecording recording, IDictionary<string, string> configDict, TimeSpan segmentStartOffset)
        {
            int minHz = int.Parse(configDict[AnalysisKeys.MinHz]);
            int maxHz = int.Parse(configDict[AnalysisKeys.MaxHz]);

            // BETTER TO CALUCLATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);

            // duration of DCT in seconds
            double dctDuration = double.Parse(configDict[AnalysisKeys.DctDuration]);

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = double.Parse(configDict[AnalysisKeys.DctThreshold]);

            // ignore oscillations below this threshold freq
            int minOscilFreq = int.Parse(configDict[AnalysisKeys.MinOscilFreq]);

            // ignore oscillations above this threshold freq
            int maxOscilFreq = int.Parse(configDict[AnalysisKeys.MaxOscilFreq]);

            // min duration of event in seconds
            double minDuration = double.Parse(configDict[AnalysisKeys.MinDuration]);

            // max duration of event in seconds
            double maxDuration = double.Parse(configDict[AnalysisKeys.MaxDuration]);

            // min score for an acceptable event
            double eventThreshold = double.Parse(configDict[AnalysisKeys.EventThreshold]);

            // seems to work  -- frameSize = 512 and 1024 does not catch all oscillations;
            const int FrameSize = 256;

            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                FrameSize,
                maxOscilFreq);

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
                                 {
                                     SourceFName = recording.BaseName,
                                     WindowSize = FrameSize,
                                     WindowOverlap = windowOverlap,
                                     NoiseReductionType = NoiseReductionType.None,
                                 };

            ////sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            TimeSpan recordingDuration = recording.Duration;
            double freqBinWidth = recording.SampleRate / (double)sonoConfig.WindowSize;

            /* #############################################################################################################################################
             * window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
             * 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
             * 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
             * 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz
             */
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            //double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            // predefinition of score array
            Oscillations2012.Execute(
                (SpectrogramStandard)sonogram,
                minHz,
                maxHz,
                dctDuration,
                minOscilFreq,
                maxOscilFreq,
                dctThreshold,
                eventThreshold,
                minDuration,
                maxDuration,
                out var scores,
                out var events,
                out var hits,
                segmentStartOffset);

            // remove isolated koala events - this is to remove false positive identifications
            events = FilterMaleKoalaEvents(events);

            if (events == null)
            {
                events = new List<AcousticEvent>();
            }
            else
            {
                events.ForEach(
                    ae =>
                        {
                            ae.SpeciesName = configDict[AnalysisKeys.SpeciesName];
                            ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                            ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                        });
            }

            // ######################################################################
            var plot = new Plot(AnalysisName, scores, eventThreshold);

            return new KoalaMaleResults
                       {
                           Events = events,
                           Hits = hits,
                           Plot = plot,
                           RecordingtDuration = recordingDuration,
                           Sonogram = sonogram,
                       };
        }

        /// <summary>
        /// This method removes isolated koala events.
        ///     Expect at least N consecutive inhales with centres spaced between 1.5 and 2.5 seconds
        ///     N=3 seems best value.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<AcousticEvent> FilterMaleKoalaEvents(List<AcousticEvent> events)
        {
            int count = events.Count;
            const int ConsecutiveInhales = 3;

            // require three consecutive inhale events to be a koala bellow.
            if (count < ConsecutiveInhales)
            {
                return null;
            }

            // to store the centres of the events
            var eventCentres = new double[count];
            for (int i = 0; i < count; i++)
            {
                // centres in seconds
                eventCentres[i] = events[i].TimeStart + ((events[i].TimeEnd - events[i].TimeStart) / 2.0);
            }

            var partOfTriple = new bool[count];
            for (int i = 1; i < count - 1; i++)
            {
                double leftGap = eventCentres[i] - eventCentres[i - 1];
                double rghtGap = eventCentres[i + 1] - eventCentres[i];

                // oscillation centres should lie between between 1.0 and 2.6 s separated.
                // HOwever want to allow for a missed oscillation - therefore allow up to 4.0 seconds apart
                bool leftGapCorrect = leftGap > 1.0 && leftGap < 4.0;
                bool rghtGapCorrect = rghtGap > 1.0 && rghtGap < 4.0;

                if (leftGapCorrect && rghtGapCorrect)
                {
                    partOfTriple[i - 1] = true;
                    partOfTriple[i] = true;
                    partOfTriple[i + 1] = true;
                }
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (!partOfTriple[i])
                {
                    events.Remove(events[i]);
                }
            }

            if (events.Count == 0)
            {
                events = null;
            }

            return events;
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            FileInfo audioFile = segmentSettings.SegmentAudioFile;

            /* ###################################################################### */
            Dictionary<string, string> configuration = analysisSettings.Configuration.ToDictionary();
            KoalaMaleResults results = Analysis(audioFile, configuration, segmentSettings.SegmentStartOffset);

            /* ###################################################################### */
            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            Plot scores = results.Plot;

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, results.RecordingtDuration)
                                      {
                                          AnalysisIdentifier = this.Identifier,
                                      };

            analysisResults.Events = results.Events.ToArray();

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteEventsFile(segmentSettings.SegmentEventsFile, analysisResults.Events);
                analysisResults.EventsFile = segmentSettings.SegmentEventsFile;
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                // noop
            }

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                string imagePath = segmentSettings.SegmentImageFile.FullName;
                const double EventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, results.Events, EventThreshold);
                image.Save(imagePath);
                analysisResults.ImageFile = segmentSettings.SegmentImageFile;
            }

            return analysisResults;
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // noop
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<AcousticEvent>());
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<EventIndex>());
        }

        private static Image DrawSonogram(
            BaseSonogram sonogram,
            double[,] hits,
            Plot scores,
            List<AcousticEvent> predictedEvents,
            double eventThreshold)
        {
            var image = new Image_MultiTrack(sonogram.GetImage());

            ////System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);

            ////Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            if (scores != null)
            {
                image.AddTrack(ImageTrack.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            }

            ////if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null)
            {
                image.OverlayRainbowTransparency(hits);
            }

            if (predictedEvents != null && predictedEvents.Count > 0)
            {
                image.AddEvents(
                    predictedEvents,
                    sonogram.NyquistFrequency,
                    sonogram.Configuration.FreqBinCount,
                    sonogram.FramesPerSecond);
            }

            return image.GetImage();
        }

        public class KoalaMaleResults
        {
            public List<AcousticEvent> Events { get; set; }

            public double[,] Hits { get; set; }

            public Plot Plot { get; set; }

            public TimeSpan RecordingtDuration { get; set; }

            public BaseSonogram Sonogram { get; set; }
        }
    }
}