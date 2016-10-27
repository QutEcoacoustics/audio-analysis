// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CriniaTinnula.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Acoustics.Shared;
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
    /// This is a frog recognizer is designed to detect a short honk (30ms long) that has the structure of stacked harmonics, similar to crow, female koala etc.
    /// 
    /// Step 1: It detects energy in relevant frequency band.
    /// 
    /// This type recognizer was first developed for Crinia tinnula and could be duplicated for other frogs with similar call structure
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// 
    /// </summary>
 /*   class CriniaTinnula2 : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "CriniaTinnula";

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
            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
     
            // min and max frequency of event in Hz
            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int maxHz = (int)configuration[AnalysisKeys.MaxHz];

            // min & max duration of event in seconds 
            double minDuration = (double)configuration[AnalysisKeys.MinDuration];                
            double maxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            // check sample rate is 22050
            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            // Set the Frame Size
            // Currently set at 256; if not good enough, try 512
            const int FrameSize = 256;

            // Make a spectrogram
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = FrameSize,
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = NoiseReductionType.STANDARD,
                NoiseReductionParameter = 0.1
            };
            sonoConfig.WindowOverlap = 0.0;

            // sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            TimeSpan recordingDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            double eventThresholdDb = 10.0;

            // minimum number of bins covering frequency bandwidth of L.convex call
            int callBinWidth = 632;
            // double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            // This window is used to smooth the score array before extracting events.
            // A short window (e.g. 3) preserves sharper score edges to define events but also keeps noise.
            int scoreSmoothingWindow = 13;
            double[] scores; // predefinition of score array
            List<AcousticEvent> acousticEvents;
            double[,] hits;

            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = speciesName;
                ae.SegmentDuration = recordingDuration;
                ae.Name = abbreviatedSpeciesName;
            });

            var plot = new Plot(this.DisplayName, scores, eventThreshold);
            var plots = new List<Plot> { plot };


            this.WriteDebugImage(recording, outputDirectory, sonogram, acousticEvents, plots, hits);

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plots,
                Events = acousticEvents
            };
        }

        private void WriteDebugImage(
            AudioRecording recording,
            DirectoryInfo outputDirectory,
            BaseSonogram sonogram,
            List<AcousticEvent> acousticEvents,
            List<Plot> plots,
            double[,] hits)
        {
            //DEBUG IMAGE this recogniser only. MUST set false for deployment. 
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                Image debugImage1 = LitoriaRothii.DisplayDebugImage(sonogram, acousticEvents, plots, hits);
                var debugPath1 =
                    outputDirectory.Combine(
                        FilenameHelpers.AnalysisResultName(
                            Path.GetFileNameWithoutExtension(recording.BaseName),
                            this.Identifier,
                            "png",
                            "DebugSpectrogram1"));
                debugImage1.Save(debugPath1.FullName);

                // save new image with longer frame
                var sonoConfig2 = new SonogramConfig
                    {
                        SourceFName = recording.BaseName,
                        WindowSize = 1024,
                        WindowOverlap = 0,
                        NoiseReductionType = NoiseReductionType.NONE,
                        //NoiseReductionType = NoiseReductionType.STANDARD,
                        //NoiseReductionParameter = 0.1
                    };
                BaseSonogram sonogram2 = new SpectrogramStandard(sonoConfig2, recording.WavReader);

                var debugPath2 =
                    outputDirectory.Combine(
                        FilenameHelpers.AnalysisResultName(
                            Path.GetFileNameWithoutExtension(recording.BaseName),
                            this.Identifier,
                            "png",
                            "DebugSpectrogram2"));
                Image debugImage2 = LitoriaRothii.DisplayDebugImage(sonogram2, acousticEvents, plots, null);
                debugImage2.Save(debugPath2.FullName);
            }
        }
    }*/
}
