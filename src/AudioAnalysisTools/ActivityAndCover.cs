// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityAndCover.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   a set of indices to describe level of acoustic activity and number of acoustic events in recording.
//   Location of acoustic events also called segmentation in some literature.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using TowseyLibrary;

    /// <summary>
    /// a set of indices to describe level of acoustic activity and number of acoustic events in recording.
    /// Location of acoustic events also called segmentation in some literature.
    /// </summary>
    public class SummaryActivity
    {
        public SummaryActivity(bool[] activeFrames, int activeFrameCount, double activeAvDecibels, bool[] events, double eventCount)
        {
            this.ActiveFrames = activeFrames;
            this.ActiveFrameCount = activeFrameCount;
            this.FractionOfActiveFrames = this.ActiveFrameCount / (double)this.ActiveFrames.Length;
            this.ActiveAvDb = activeAvDecibels;
            this.EventCount = eventCount;
            this.EventLocations = events;
        }

        public double FractionOfActiveFrames { get; }

        public double ActiveAvDb { get; }

        public double EventCount { get; }

        public int ActiveFrameCount { get; }

        public bool[] ActiveFrames { get; }

        public bool[] EventLocations { get; }
    }

    public class SpectralActivity
    {
        public SpectralActivity(double[] eventSp, double[] coverSp, double lowFreqCvr, double midFreqCvr, double highFreqCvr)
        {
            this.EventSpectrum = eventSp;
            this.CoverSpectrum = coverSp;
            this.LowFreqBandCover = lowFreqCvr;
            this.MidFreqBandCover = midFreqCvr;
            this.HighFreqBandCover = highFreqCvr;
        }

        public double LowFreqBandCover { get; }

        public double MidFreqBandCover { get; }

        public double HighFreqBandCover { get; }

        public double[] CoverSpectrum { get; }

        public double[] EventSpectrum { get; }
    }

    public static class ActivityAndCover
    {
        public const double DefaultActivityThresholdDb = 6.0; // used to select frames having dB value above background

        public static SummaryActivity CalculateActivity(double[] dBarray, TimeSpan frameStepDuration)
        {
            return CalculateActivity(dBarray, frameStepDuration, DefaultActivityThresholdDb);
        }

        /// <summary>
        /// Returns the number of active frames and acoustic events and their average duration in milliseconds
        /// Only counts an event if it is LONGER than one frame.
        /// Count events as number of transitions from active to non-active frame.
        /// </summary>
        /// <param name="dBarray">array of DB values.</param>
        /// <param name="frameStepDuration">frame duration in seconds.</param>
        /// <param name="dbThreshold">threshold in decibels.</param>
        public static SummaryActivity CalculateActivity(double[] dBarray, TimeSpan frameStepDuration, double dbThreshold)
        {
            bool[] activeFrames = new bool[dBarray.Length];
            double activeAvDecibels = 0.0;
            int activeFrameCount = 0;

            // get frames with activity >= threshold dB above background and count
            for (int i = 0; i < dBarray.Length; i++)
            {
                if (dBarray[i] >= dbThreshold)
                {
                    activeFrames[i] = true;
                    activeAvDecibels += dBarray[i];
                    activeFrameCount++;
                }
            }

            // following line is more elegant but want to keep active frame array
            //int activeFrameCount = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB));
            if (activeFrameCount != 0)
            {
                activeAvDecibels /= activeFrameCount;
            }

            if (activeFrameCount <= 1)
            {
                return new SummaryActivity(activeFrames, activeFrameCount, activeAvDecibels, new bool[dBarray.Length], 0);
            }

            // store record of events
            bool[] events = (bool[])activeFrames.Clone();

            // remove one frame events
            for (int i = 1; i < activeFrames.Length - 1; i++)
            {
                if (!events[i - 1] && events[i] && !events[i + 1])
                {
                    events[i] = false; //remove solitary active frames
                }
            }

            //bool[] events2 = {false, false, true, true, true, false, true, true, false, false, true, true, true}; //3 events; lengths = 3, 2, 3
            //List<int> eventList = DataTools.GetEventLengths(events);
            //var listOfFilteredEvents = eventList.Where(x => x >= minFrameCount);
            //int eventCount = listOfFilteredEvents.Count();

            double eventCount = 0;
            for (int i = 0; i < activeFrames.Length - 1; i++)
            {
                if ((!events[i] && events[i + 1]) || (events[i] && !events[i + 1]))
                {
                    eventCount += 1.0; // count the starts and ends of events
                }
            }

            eventCount /= 2;  // divide by 2 because counted starts and ends
            return new SummaryActivity(activeFrames, activeFrameCount, activeAvDecibels, events, eventCount);
        }

        /// <summary>
        /// Returns the number of acoustic events per second in the each frequency bin.
        /// Also returns the fractional cover in each freq bin, that is, the fraction of frames where amplitude > threshold.
        /// WARNING NOTE: This method assumes that a linear herz scale, i.e. that herz bin width is constant over the frequency scale.
        /// If you have octave freq scale, then call the following method with bin bounds pre-calculated.
        /// Bin width = Herz per bin i.e. column in spectrogram - spectrogram rotated wrt to normal view.
        /// </summary>
        public static SpectralActivity CalculateSpectralEvents(
                double[,] spectrogram,
                double dbThreshold,
                TimeSpan frameStepDuration,
                int lowFreqBound,
                int midFreqBound,
                double binWidth)
        {
            //calculate boundaries between hi, mid and low frequency spectrum
            //int freqBinCount = spectrogram.GetLength(1);
            int lowFreqBinIndex = (int)Math.Ceiling(lowFreqBound / binWidth);
            int midFreqBinIndex = (int)Math.Ceiling(midFreqBound / binWidth);
            return CalculateSpectralEvents(spectrogram, dbThreshold, frameStepDuration, lowFreqBinIndex, midFreqBinIndex);
        }

        /// <summary>
        /// Returns the number of acoustic events per second in the each frequency bin.
        /// Also returns the fractional cover in each freq bin, that is, the fraction of frames where amplitude > threshold.
        /// WARNING NOTE: If you call this method, you must provide the low and mid-freq bounds as BIN IDs, NOT as Herz values.
        /// </summary>
        public static SpectralActivity CalculateSpectralEvents(double[,] spectrogram, double dbThreshold, TimeSpan frameStepDuration, int lowFreqBinIndex, int midFreqBinIndex)
        {
            int rows = spectrogram.GetLength(0); // frames
            int cols = spectrogram.GetLength(1); // # of freq bins
            double recordingDurationInSeconds = rows * frameStepDuration.TotalSeconds;

            double[] coverSpectrum = new double[cols];
            double[] eventSpectrum = new double[cols];

            // for each frequency bin, calculate coverage
            for (int c = 0; c < cols; c++)
            {
                // get the freq bin containing dB values
                double[] bin = MatrixTools.GetColumn(spectrogram, c);

                // get activity and event info
                var activity = CalculateActivity(bin, frameStepDuration, dbThreshold);

                //bool[] a1 = activity.activeFrames;
                //int a2 = activity.activeFrameCount;
                coverSpectrum[c] = activity.FractionOfActiveFrames;

                //double a4 = activity.activeAvDB;
                eventSpectrum[c] = activity.EventCount / recordingDurationInSeconds;

                //TimeSpan a6 = activity.avEventDuration;
                //bool[] a7 = activity.eventLocations;
            }

            // calculate coverage for low freq band as a percentage
            int count = 0;
            double sum = 0;
            for (int j = 0; j < lowFreqBinIndex; j++)
            {
                sum += coverSpectrum[j];
                count++;
            }

            double lowFreqCover = sum / count;

            // calculate coverage for mid freq band as a percentage
            count = 0;
            sum = 0;
            for (int j = lowFreqBinIndex; j < midFreqBinIndex; j++)
            {
                sum += coverSpectrum[j];
                count++;
            }

            double midFreqCover = sum / count;

            // calculate coverage for high freq band as a percentage
            // avoid top row which can have edge effects
            int highFreqBinIndex = spectrogram.GetLength(1) - 1;
            count = 0;
            sum = 0;
            for (int j = midFreqBinIndex; j < highFreqBinIndex; j++)
            {
                sum += coverSpectrum[j];
                count++;
            }

            double highFreqCover = sum / count;
            return new SpectralActivity(eventSpectrum, coverSpectrum, lowFreqCover, midFreqCover, highFreqCover);
        }
    }
}