using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Recognizers
{
    using System.Drawing;
    using System.IO;
    using System.Reflection;

    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;

    /// <summary>
    /// This is a frog rcogniser based on the "ribit" or "washboard" template
    /// It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    /// 
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs 
    /// To call this recogniser, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recogniser can be called via the MultiRecognizer.
    /// 
    /// </summary>
    class LitoriaPallida : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaPallida";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording recording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int maxHz = (int)configuration[AnalysisKeys.MaxHz];

            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);

            // duration of DCT in seconds 
            double dctDuration = (double)configuration[AnalysisKeys.DctDuration];

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = (double)configuration[AnalysisKeys.DctThreshold];  

            // ignore oscillations below this threshold freq
            int minOscilFreq = (int)configuration[AnalysisKeys.MinOscilFreq];

            // ignore oscillations above this threshold freq
            int maxOscilFreq = (int)configuration[AnalysisKeys.MaxOscilFreq];

            // min duration of event in seconds 
            double minDuration = (double)configuration[AnalysisKeys.MinDuration];

            // max duration of event in seconds                 
            double maxDuration = (double)configuration[AnalysisKeys.MaxDuration];
            
            // The default was 512 for Canetoad.
            // Framesize = 128 seems to work for Littoria fallax.
            // frame size
            int frameSize = (int)configuration[AnalysisKeys.KeyFrameSize];

            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                frameSize,
                maxOscilFreq);
            //windowOverlap = 0.75; // previous default

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.FileName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = NoiseReductionType.STANDARD,
                NoiseReductionParameter = 0.2,
            };

            TimeSpan recordingDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            // double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            // This window is used to smooth the score array before extracting events.
            // A short window preserves sharper score edges to define events but also keeps noise.
            int scoreSmoothingWindow = 5;
            double[] scores; // predefinition of score array
            List<AcousticEvent> acousticEvents;
            double[,] hits;
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
                scoreSmoothingWindow,
                out scores,
                out acousticEvents,
                out hits);

            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = speciesName;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recordingDuration;
                ae.Name = abbreviatedSpeciesName;
            });

            var plot = new Plot(this.DisplayName, scores, eventThreshold);
            var plots = new List<Plot>();
            plots.Add(plot);


            //DEBUG IMAGE this recogniser only. MUST set false for deployment. 
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                Image debugImage = DisplayDebugImage(sonogram, acousticEvents, plots, hits);
                string debugDir = @"C:\SensorNetworks\Output\Frogs\TestOfRecognisers-2016Sept\Test\";
                var fileName = Path.GetFileNameWithoutExtension(recording.FileName);
                string debugPath = Path.Combine(debugDir, fileName + ".DebugSpectrogram_"+ SpeciesName + ".png");
                debugImage.Save(debugPath);
            }

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = acousticEvents
            };

        }



        public static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (Plot plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);

            if (events.Count > 0)
            {
                foreach (AcousticEvent ev in events) // set colour for the events
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
