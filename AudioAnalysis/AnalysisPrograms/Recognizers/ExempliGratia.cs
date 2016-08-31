using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Recognizers
{
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
    /// This is a template recognizer
    /// </summary>
    class ExempliGratia : RecognizerBase
    {
        public override string Author => "Truskinger";

        public override string SpeciesName => "ExempliGratia";

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
        /// <param name="audioRecording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {


            // Get a value from the config file - with a backup default
            int minHz = (int?)configuration[AnalysisKeys.MinHz] ?? 600;

            // Get a value from the config file - with no default, throw an exception if value is not present
            //int maxHz = ((int?)configuration[AnalysisKeys.MaxHz]).Value;

            // Get a value from the config file - without a string accessor, as a double
            double someExampleSettingA = (double?)configuration.someExampleSettingA ?? 0.0;

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";



            // get samples
            var samples = audioRecording.WavReader.Samples;


            // make a spectrogram
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.STANDARD,
                NoiseReductionParameter = (double?)configuration[AnalysisKeys.NoiseBgThreshold] ?? 0.0
            };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            // get high resolution indices

            // when the value is accessed, the indices are calculated
            var indices = getSpectralIndexes.Value;

            // check if the indices have been calculated - you shouldn't actually need this
            if (getSpectralIndexes.IsValueCreated)
            {
                // then indices have been calculated before
            }

            var foundEvents = new List<AcousticEvent>();

            // some kind of loop where you scan through the audio

            // 'find' an event - if you find an event, store the data in the AcousticEvent class
            var anEvent = new AcousticEvent(
                new Oblong(50, 50, 100, 100),
                sonogram.NyquistFrequency,
                sonogram.Configuration.FreqBinCount,
                sonogram.FrameDuration,
                sonogram.FrameStep,
                sonogram.FrameCount);
            anEvent.Name = "FAKE!";

            foundEvents.Add(anEvent);

            // end loop

            return new RecognizerResults()
            {
                Events = foundEvents,
                Hits = null,
                ScoreTrack = null,
                //Plots = null,
                Sonogram = sonogram
            };
        }
    }
}
