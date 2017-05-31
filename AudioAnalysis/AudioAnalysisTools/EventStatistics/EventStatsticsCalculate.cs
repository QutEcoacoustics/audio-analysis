// <copyright file="EventStatsticsCalculate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Tools.Wav;
    using DSP;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    public static class EventStatsticsCalculate
    {
        /// <summary>
        /// The acoustic statistics calculated in this method are based on methods outlined in
        /// "Acoustic classification of multiple simultaneous bird species: A multi-instance multi-label approach",
        /// by Forrest Briggs, Balaji Lakshminarayanan, Lawrence Neal, Xiaoli Z.Fern, Raviv Raich, Sarah J.K.Hadley, Adam S. Hadley, Matthew G. Betts, et al.
        /// The Journal of the Acoustical Society of America v131, pp4640 (2012); doi: http://dx.doi.org/10.1121/1.4707424
        /// ..
        /// The Briggs feature are calculated from the column (freq bin) and row (frame) sums of the extracted spectrogram.
        /// 1. Gini Index for frame and bin sums. A measure of dispersion. Problem with gini is that its value is dependent on the row or colukmn count.
        ///    We use entropy instead because value not dependent on row or column count because it is normalised.
        /// For the following meausres of k-central moments, the freq and time values are normalised in 0,1 to width of the event.
        /// 2. freq-mean
        /// 3. freq-variance
        /// 4. freq-skew
        /// 5. time-mean
        /// 6. time-variance
        /// 7. time-skew
        /// 8. freq-max (normalised)
        /// 9. time-max (normalised)
        /// ..
        /// Briggs et al also calculate a 16 value histogram of gradients for each event mask. We do not do that here although we could.
        /// ..
        /// NOTE: This method assumes that the passed event occurs totally within the passed recording,
        /// AND that the passed recording is of sufficient duration to obtain reliable BGN noise profile
        /// BUT not so long as to cause memory constipation.
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
            EventStatisticsConfiguration config)
        {
            var stats = new EventStatistics
            {
                EventStart = temporalTarget.start,
                EventStartSeconds = temporalTarget.start.TotalSeconds,
                EventEnd = temporalTarget.end,
                Duration = temporalTarget.end - temporalTarget.start,
                FreqLow = spectralTarget.start,
                FreqTop = spectralTarget.end,
                BandWidth = spectralTarget.end - spectralTarget.start,
            };

            // convert recording to spectrogram
            int sampleRate = recording.SampleRate;
            double epsilon = recording.Epsilon;

            // extract the spectrogram
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(recording, config.FrameSize, config.FrameStep);

            //int nyquist = dspOutput1.NyquistFreq;
            //var spectrogram = dspOutput1.AmplitudeSpectrogram;
            //var frameDuration = TimeSpan.FromSeconds(config.FrameSize / (double)sampleRate);
            double herzPerBin = dspOutput1.FreqBinWidth;
            var stepDurationInSeconds = config.FrameStep / (double)sampleRate;
            var startFrame = (int)Math.Ceiling(temporalTarget.start.TotalSeconds / stepDurationInSeconds);
            var endFrame = (int)Math.Floor(temporalTarget.end.TotalSeconds / stepDurationInSeconds);

            var bottomBin = (int)Math.Floor(spectralTarget.start / herzPerBin);
            var topBin = (int)Math.Ceiling(spectralTarget.end / herzPerBin);

            // Convert amplitude spectrogram to deciBels and calculate the dB background noise profile
            double[,] decibelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.AmplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(decibelSpectrogram);

            decibelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogram, spectralDecibelBgn);
            decibelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogram, nhThreshold: 2.0);

            // extract the required acoustic event
            var eventMatrix = MatrixTools.Submatrix(decibelSpectrogram, startFrame, bottomBin, endFrame, topBin);

            var columnSums = MatrixTools.GetColumnSums(eventMatrix);
            int maxColumnId = DataTools.GetMaxIndex(columnSums);
            var rowSums = MatrixTools.GetRowSums(eventMatrix);
            int maxRowId = DataTools.GetMaxIndex(rowSums);

            // calculate the temporal mean and standard deviation
            NormalDist.AverageAndSD(rowSums, out double mean, out double stddev);
            stats.TemporalMean = mean;
            stats.TemporalStdDev = stddev;

            // calculate the frequency mean and standard deviation
            NormalDist.AverageAndSD(columnSums, out mean, out stddev);
            stats.FreqMean = mean;
            stats.FreqStdDev = stddev;

            // calculate relative location of the temporal maximum
            stats.TemporalMaxRelative = maxRowId / (double)rowSums.Length;

            // calculate the entropy dispersion/concentration indices
            stats.TemporalEnergyDistribution = 1 - DataTools.Entropy_normalised(rowSums);
            stats.SpectralEnergyDistribution = 1 - DataTools.Entropy_normalised(columnSums);

            // calculate the spectral centroid and the dominant frequency
            int binId = CalculateSpectralCentroid(columnSums);
            stats.SpectralCentroid = (int)Math.Round(herzPerBin * binId) + spectralTarget.start;
            stats.DominantFrequency = (int)Math.Round(herzPerBin * maxColumnId) + spectralTarget.start;

            // remainder of this method is to produce debugging images. Can comment out when not debugging.
            var normalisedIndex = DataTools.normalise(columnSums);
            var image4 = GraphsAndCharts.DrawGraph("columnSums", normalisedIndex, 100);
            string path4 = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\columnSums.png";
            image4.Save(path4);
            normalisedIndex = DataTools.normalise(rowSums);
            image4 = GraphsAndCharts.DrawGraph("rowSums", normalisedIndex, 100);
            path4 = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\rowSums.png";
            image4.Save(path4);

            return stats;
        }

        public static void TestCalculateEventStatistics()
        {
            int sampleRate = 22050;
            double duration = 28; // signal duration in seconds = 7 minutes
            int[] harmonics1 = { 500 };
            int[] harmonics2 = { 500, 1000, 2000, 4000, 8000 };
            var recording1 = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics1);
            var recording2 = DspFilters.GenerateTestSignal(sampleRate, 4, harmonics2);
            var recording3 = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics1);
            var list = new List<double[]>
            {
                recording1.WavReader.Samples,
                recording2.WavReader.Samples,
                recording3.WavReader.Samples,
            };
            var signal = DataTools.ConcatenateVectors(list);
            var wr = new WavReader(signal, 1, 16, sampleRate);
            var recording = new AudioRecording(wr);

            var start = TimeSpan.FromSeconds(28);
            var end = TimeSpan.FromSeconds(32);
            int lowFreq = 1500;
            int topFreq = 8500;

            var statsConfig = new EventStatisticsConfiguration()
            {
                FrameSize = 512,
                FrameStep = 512,
            };

            EventStatistics stats = AnalyzeAudioEvent(recording, (start, end), (lowFreq, topFreq), statsConfig);

            LoggedConsole.WriteLine($"Stats: Temporal entropy = {stats.TemporalEnergyDistribution}");
            LoggedConsole.WriteLine($"Stats: Spectral entropy = {stats.SpectralEnergyDistribution}");
            LoggedConsole.WriteLine($"Stats: Spectral centroid= {stats.SpectralCentroid}");
            LoggedConsole.WriteLine($"Stats: DominantFrequency= {stats.DominantFrequency}");

            // Assume linear scale.
            int nyquist = sampleRate / 2;
            var freqScale = new FrequencyScale(nyquist: nyquist, frameSize: statsConfig.FrameSize, herzLinearGridInterval: 1000);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = statsConfig.FrameSize,
                WindowStep = statsConfig.FrameSize,
                WindowOverlap = 0.0,
                SourceFName = "SineSignal3",
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.12,
            };
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            var image = sonogram.GetImage();
            string title = $"Spectrogram of Harmonics: SR={sampleRate}  Window={freqScale.WindowSize}";
            image = sonogram.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            string path = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\SineSignal3.png";
            image.Save(path);

            // get spectrum from row 1300
            var normalisedIndex = DataTools.normalise(MatrixTools.GetRow(sonogram.Data, 1300));
            var image2 = GraphsAndCharts.DrawGraph("SPECTRUM", normalisedIndex, 100);
            string path2 = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\Spectrum3.png";
            image2.Save(path2);
        }

        /// <summary>
        /// Returns the id of the bin which contains the spectral centroid.
        /// </summary>
        public static int CalculateSpectralCentroid(double[] spectrum)
        {
            int bin = 0;
            return bin;
        }
    }
}
