// <copyright file="EventStatistics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using AnalysisBase.ResultBases;
    using DSP;
    using TowseyLibrary;
    using WavTools;

    public class EventStatistics : EventBase
    {
        public TimeSpan Duration { get; private set; }

        public int BandWidth { get; private set; }

        public double AverageAmplitude { get; private set; }

        public int DominantFrequency { get; private set; }

        /// <summary>
        /// The acoustic statistics calculated in this method are based on methods outlined in
        /// "Acoustic classification of multiple simultaneous bird species: A multi-instance multi-label approach",
        /// by Forrest Briggs, Balaji Lakshminarayanan, Lawrence Neal, Xiaoli Z.Fern, Raviv Raich, Sarah J.K.Hadley, Adam S. Hadley, Matthew G. Betts, et al.
        /// The Journal of the Acoustical Society of America v131, pp4640 (2012); doi: http://dx.doi.org/10.1121/1.4707424
        /// ..
        /// Note that EventBase already has getters/setters for:
        /// TimeSpan SegmentStartOffset
        /// double Score
        /// double EventStartSeconds
        /// double? MinHz
        /// ..
        /// NOTE: When MinHz equals null, this indicates that the event is broad band or has undefined frequency. The event is an instant.
        ///       When MinHz has a value, this indicates the event is a point in time/frequency space.
        /// </summary>
        /// <param name="recording">as type AudioRecording which contains the event</param>
        /// <param name="temporalTarget">both start and end bounds</param>
        /// <param name="spectralTarget">both bottom and top bounds in Herz</param>
        /// <param name="config">parameters that determine the outcome of the analysis</param>
        /// <returns>an instance of EventStatistics</returns>
        public static EventStatistics AnalyzeAudioEvent(
            AudioRecording recording,
            (TimeSpan start, TimeSpan end) temporalTarget,
            (int start, int end) spectralTarget,
            AnalyzeAudioEventConfiguration config)
        {
            var stats = new EventStatistics();

            // should first deal with case where MinHz=null or has value

            stats.Duration = temporalTarget.end - temporalTarget.start;
            stats.BandWidth = spectralTarget.end - spectralTarget.start;

            // now cut out the signal
            int sampleRate = recording.SampleRate;
            int startSample = (int)(temporalTarget.start.TotalSeconds * sampleRate);
            int endSample = (int)(temporalTarget.end.TotalSeconds * sampleRate);
            double framesTotal = (endSample - startSample + 1) / (double)config.FrameStep;
            int frameCount = (int)Math.Ceiling(framesTotal);
            int sampleCount = frameCount * config.FrameStep;

            double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, startSample, sampleCount);
            var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
            var eventRecording = new AudioRecording(wr);

            // convert recording to spectrogram
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(eventRecording, config.FrameSize, config.FrameStep);
            double herzPerBin = dspOutput1.FreqBinWidth;

            // Assume linear scale.
            int nyquist = eventRecording.SampleRate / 2;
            var freqScale = new FrequencyScale(nyquist: nyquist, frameSize: config.FrameSize, herzLinearGridInterval: 1000);
            var spectrogram = dspOutput1.AmplitudeSpectrogram;

            var columnAverages = MatrixTools.GetColumnsAverages(spectrogram);
            int maxId = DataTools.GetMaxIndex(columnAverages);

            // TODO
            NormalDist.AverageAndSD(spectrogram, out double av, out double sd);
            stats.AverageAmplitude = av;
            stats.DominantFrequency = (int)Math.Round(herzPerBin * (maxId + 1));

            return stats;
        }
    }

    public class AnalyzeAudioEventConfiguration
    {
        public int FrameSize { get; set; }

        public int FrameStep { get; set; }
    }
}
