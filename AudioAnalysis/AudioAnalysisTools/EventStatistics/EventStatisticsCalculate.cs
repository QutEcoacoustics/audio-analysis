// <copyright file="EventStatisticsCalculate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Tools.Wav;
    using DSP;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    public static class EventStatisticsCalculate
    {
        /// <summary>
        /// Calculate summary statistics for supplied temporal and spectral targets.
        /// </summary>
        /// <remarks>
        /// The acoustic statistics calculated in this method are based on methods outlined in
        /// "Acoustic classification of multiple simultaneous bird species: A multi-instance multi-label approach",
        /// by Forrest Briggs, Balaji Lakshminarayanan, Lawrence Neal, Xiaoli Z.Fern, Raviv Raich, Sarah J.K.Hadley, Adam S. Hadley, Matthew G. Betts, et al.
        /// The Journal of the Acoustical Society of America v131, pp4640 (2012); doi: http://dx.doi.org/10.1121/1.4707424
        /// ..
        /// The Briggs feature are calculated from the column (freq bin) and row (frame) sums of the extracted spectrogram.
        /// 1. Gini Index for frame and bin sums. A measure of dispersion. Problem with gini is that its value is dependent on the row or column count.
        ///    We use entropy instead because value not dependent on row or column count because it is normalized.
        /// For the following meausres of k-central moments, the freq and time values are normalized in 0,1 to width of the event.
        /// 2. freq-mean
        /// 3. freq-variance
        /// 4. freq-skew and kurtosis
        /// 5. time-mean
        /// 6. time-variance
        /// 7. time-skew and kurtosis
        /// 8. freq-max (normalized)
        /// 9. time-max (normalized)
        /// 10. Briggs et al also calculate a 16 value histogram of gradients for each event mask. We do not do that here although we could.
        /// ...
        /// NOTE 1: There are differences between our method of noise reduction and Briggs. Briggs does not convert to decibels
        /// and instead works with power values. He obtains a noise profile from the 20% of frames having the lowest energy sum.
        /// NOTE 2: To NormaliseMatrixValues for noise, they divide the actual energy by the noise value. This is equivalent to subtraction when working in decibels.
        ///         There are advantages and disadvantages to Briggs method versus ours. In our case, we hve to convert decibel values back to
        ///         energy values when calculating the statistics for the extracted acoustic event.
        /// NOTE 3: We do not calculate the higher central moments of the time/frequency profiles, i.e. skew and kurtosis.
        ///         Ony mean and standard deviation.
        /// ..
        /// NOTE 4: This method assumes that the passed event occurs totally within the passed recording,
        /// AND that the passed recording is of sufficient duration to obtain reliable BGN noise profile
        /// BUT not so long as to cause memory constipation.
        /// </remarks>
        /// <param name="recording">as type AudioRecording which contains the event</param>
        /// <param name="temporalTarget">Both start and end bounds - relative to the supplied recording</param>
        /// <param name="spectralTarget">both bottom and top bounds in Hertz</param>
        /// <param name="config">parameters that determine the outcome of the analysis</param>
        /// <param name="segmentStartOffset">How long since the start of the recording this event occurred</param>
        /// <returns>an instance of EventStatistics</returns>
        public static EventStatistics AnalyzeAudioEvent(
            AudioRecording recording,
            Range<TimeSpan> temporalTarget,
            Range<double> spectralTarget,
            EventStatisticsConfiguration config,
            TimeSpan segmentStartOffset)
        {
            var stats = new EventStatistics
            {
                EventStartSeconds = temporalTarget.Minimum.TotalSeconds,
                EventEndSeconds = temporalTarget.Maximum.TotalSeconds,
                LowFrequencyHertz = spectralTarget.Minimum,
                HighFrequencyHertz = spectralTarget.Maximum,
                SegmentDurationSeconds = recording.Duration.TotalSeconds,
                SegmentStartSeconds = segmentStartOffset.TotalSeconds,
            };

            // temporal target is supplied relative to recording, but not the supplied audio segment
            // shift coordinates relative to segment
            var localTemporalTarget = temporalTarget.Shift(-segmentStartOffset);

            if (!recording
                .Duration
                .AsRangeFromZero()
                .IntersectsWith(localTemporalTarget, Topology.MinimumInclusiveMaximumExclusive))
            {
                stats.Error = true;
                stats.ErrorMessage =
                    $"Audio not long enough ({recording.Duration}) to analyze target ({localTemporalTarget})";

                return stats;
            }

            // convert recording to spectrogram
            int sampleRate = recording.SampleRate;
            double epsilon = recording.Epsilon;

            // extract the spectrogram
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(recording, config.FrameSize, config.FrameStep);

            double hertzBinWidth = dspOutput1.FreqBinWidth;
            var stepDurationInSeconds = config.FrameStep / (double)sampleRate;
            var startFrame = (int)Math.Ceiling(localTemporalTarget.Minimum.TotalSeconds / stepDurationInSeconds);

            // subtract 1 frame because want to end before start of end point.
            var endFrame = (int)Math.Floor(localTemporalTarget.Maximum.TotalSeconds / stepDurationInSeconds) - 1;

            var bottomBin = (int)Math.Floor(spectralTarget.Minimum / hertzBinWidth);
            var topBin = (int)Math.Ceiling(spectralTarget.Maximum / hertzBinWidth);

            // Events can have their high value set to the nyquist.
            // Since the submatrix call below uses an inclusive upper bound an index out of bounds exception occurs in
            // these cases. So we just ask for the bin below.
            if (topBin >= config.FrameSize / 2)
            {
                topBin = (config.FrameSize / 2) - 1;
            }

            // Convert amplitude spectrogram to deciBels and calculate the dB background noise profile
            double[,] decibelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput1.AmplitudeSpectrogram, dspOutput1.WindowPower, sampleRate, epsilon);
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(decibelSpectrogram);

            decibelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogram, spectralDecibelBgn);
            decibelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogram, nhThreshold: 2.0);

            // extract the required acoustic event
            var eventMatrix = MatrixTools.Submatrix(decibelSpectrogram, startFrame, bottomBin, endFrame, topBin);

            // Get the SNR of the event. This is just the max value in the matrix because noise reduced
            MatrixTools.MinMax(eventMatrix, out _, out double max);
            stats.SnrDecibels = max;

            // Now need to convert event matrix back to energy values before calculating other statistics
            eventMatrix = MatrixTools.Decibels2Power(eventMatrix);

            var columnAverages = MatrixTools.GetColumnAverages(eventMatrix);
            var rowAverages = MatrixTools.GetRowAverages(eventMatrix);

            // calculate the mean and temporal standard deviation in decibels
            NormalDist.AverageAndSD(rowAverages, out double mean, out double stddev);
            stats.MeanDecibels = 10 * Math.Log10(mean);
            stats.TemporalStdDevDecibels = 10 * Math.Log10(stddev);

            // calculate the frequency standard deviation in decibels
            NormalDist.AverageAndSD(columnAverages, out mean, out stddev);
            stats.FreqBinStdDevDecibels = 10 * Math.Log10(stddev);

            // calculate relative location of the temporal maximum
            int maxRowId = DataTools.GetMaxIndex(rowAverages);
            stats.TemporalMaxRelative = maxRowId / (double)rowAverages.Length;

            // calculate the entropy dispersion/concentration indices
            stats.TemporalEnergyDistribution = 1 - DataTools.Entropy_normalised(rowAverages);
            stats.SpectralEnergyDistribution = 1 - DataTools.Entropy_normalised(columnAverages);

            // calculate the spectral centroid and the dominant frequency
            double binCentroid = CalculateSpectralCentroid(columnAverages);
            stats.SpectralCentroid = (int)Math.Round(hertzBinWidth * binCentroid) + (int)spectralTarget.Minimum;
            int maxColumnId = DataTools.GetMaxIndex(columnAverages);
            stats.DominantFrequency = (int)Math.Round(hertzBinWidth * maxColumnId) + (int)spectralTarget.Minimum;

            // remainder of this method is to produce debugging images. Can comment out when not debugging.
            /*
            var normalisedIndex = DataTools.NormaliseMatrixValues(columnAverages);
            var image4 = GraphsAndCharts.DrawGraph("columnSums", normalisedIndex, 100);
            string path4 = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\columnSums.png";
            image4.Save(path4);
            normalisedIndex = DataTools.NormaliseMatrixValues(rowAverages);
            image4 = GraphsAndCharts.DrawGraph("rowSums", normalisedIndex, 100);
            path4 = @"C:\SensorNetworks\Output\Sonograms\UnitTestSonograms\rowSums.png";
            image4.Save(path4);
            */
            return stats;
        }

        /// <summary>
        /// Returns the id of the bin which contains the spectral centroid.
        /// </summary>
        public static double CalculateSpectralCentroid(double[] spectrum)
        {
            double centroidBin = 0;

            double powerSum = spectrum.Sum();

            for (int bin = 0; bin < spectrum.Length; bin++)
            {
                centroidBin += bin * spectrum[bin] / powerSum;
            }

            return centroidBin;
        }
    }
}
