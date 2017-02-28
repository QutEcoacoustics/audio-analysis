// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityAndCover.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   a set of indices to describe level of acoustic activity and number of acoustic events in recording.
//   Location of acoustic events also called segmentation in some literature.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TowseyLibrary;

    /// <summary>
    /// a set of indices to describe level of acoustic activity and number of acoustic events in recording.
    /// Location of acoustic events also called segmentation in some literature.
    /// </summary>
    public class SummaryActivity
    {
        public double fractionOfActiveFrames, activeAvDB, eventCount;
        //public TimeSpan avEventDuration;
        public int activeFrameCount;
        public bool[] activeFrames, eventLocations;

        //public SummaryActivity(bool[] _activeFrames, int _activeFrameCount, double _activeAvDB, bool[] _events, double _eventCount, TimeSpan _avEventDuration)
        public SummaryActivity(bool[] _activeFrames, int _activeFrameCount, double _activeAvDB, bool[] _events, double _eventCount)
        {
            activeFrames = _activeFrames;
            activeFrameCount = _activeFrameCount;
            fractionOfActiveFrames = activeFrameCount / (double)activeFrames.Length;
            activeAvDB = _activeAvDB;
            eventCount = _eventCount;
            //avEventDuration = _avEventDuration;
            eventLocations = _events;
        }
    }

    public class SpectralActivity
    {

        public double lowFreqBandCover, midFreqBandCover, highFreqBandCover;
        public double[] coverSpectrum, eventSpectrum;
        public SpectralActivity(double[] _eventSpectrum, double[] _coverSpectrum, double _lowFreqCover, double _midFreqCover, double _highFreqCover)
        {
            eventSpectrum     = _eventSpectrum;
            coverSpectrum     = _coverSpectrum;
            lowFreqBandCover  = _lowFreqCover;
            midFreqBandCover  = _midFreqCover;
            highFreqBandCover = _highFreqCover;
        }
    }

    public static class ActivityAndCover
    {

        public const double DefaultActivityThresholdDb = 6.0; // used to select frames that have 3dB > background
        // used to remove short events from consideration - NOT USED ANY MORE
        //public static TimeSpan DEFAULT_MinimumEventDuration = TimeSpan.FromMilliseconds(100); 


        public static SummaryActivity CalculateActivity(double[] dBarray, TimeSpan frameStepDuration)
        {
            return CalculateActivity(dBarray, frameStepDuration, DefaultActivityThresholdDb);
        }

        /// <summary>
        /// Returns the number of active frames and acoustic events and their average duration in milliseconds
        /// Only counts an event if it is LONGER than one frame. 
        /// Count events as number of transitions from active to non-active frame
        /// </summary>
        /// <param name="activeFrames"></param>
        /// <param name="frameStepDuration">frame duration in seconds</param>
        /// <returns></returns>
        public static SummaryActivity CalculateActivity(double[] dBarray, TimeSpan frameStepDuration, double dbThreshold)
        {
            // minimum frame length for recognition of a valid event
            //int minFrameCount = (int)Math.Round(ActivityAndCover.DEFAULT_MinimumEventDuration.TotalMilliseconds / frameStepDuration.TotalMilliseconds);

            bool[] activeFrames = new bool[dBarray.Length];
            double activeAvDB = 0.0;
            int activeFrameCount = 0;

            // get frames with activity >= threshold dB above background and count
            for (int i = 0; i < dBarray.Length; i++)
            {
                if (dBarray[i] >= ActivityAndCover.DefaultActivityThresholdDb)
                {
                    activeFrames[i] = true;
                    activeAvDB += dBarray[i];
                    activeFrameCount++;
                }
            }

            // following line is more elegant but want to keep active frame array
            //int activeFrameCount = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB));  
            if (activeFrameCount != 0) activeAvDB /= (double)activeFrameCount;

            if (activeFrameCount <= 1)
                return new SummaryActivity(activeFrames, activeFrameCount, activeAvDB, new bool[dBarray.Length], 0);


            // store record of events
            bool[] events = (bool[]) activeFrames.Clone();
            // remove one frame events
            for (int i = 1; i < activeFrames.Length - 1; i++)
            {
                if (!events[i - 1] && events[i] && !events[i + 1])
                    events[i] = false; //remove solitary active frames
            }

            //bool[] events2 = {false, false, true, true, true, false, true, true, false, false, true, true, true}; //3 events; lengths = 3, 2, 3
            //List<int> eventList = DataTools.GetEventLengths(events);
            //var listOfFilteredEvents = eventList.Where(x => x >= minFrameCount);
            //int eventCount = listOfFilteredEvents.Count();

            double eventCount = 0;
            for (int i = 0; i < activeFrames.Length - 1; i++)
            {
                if ((!events[i] && events[i+1]) || (events[i] && !events[i + 1]))
                    eventCount += 1.0; // count the starts and ends of events
            }
            eventCount /= 2;  // divide by 2 because counted starts and ends
            return new SummaryActivity(activeFrames, activeFrameCount, activeAvDB, events, eventCount);
        }


        /// <summary>
        /// Returns the number of acoustic events per second in the each frequency bin.
        /// Also returns the fractional cover in each freq bin, that is, the fraction of frames where amplitude > threshold.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="bgThreshold"></param>
        /// <param name="lowFreqBound">Herz</param>
        /// <param name="midFreqBound">Herz</param>
        /// <param name="nyquist">Herz</param>
        /// <param name="binWidth">Herz per bin i.e. column in spectrogram - spectrogram rotated wrt to normal view.</param>
        /// <returns></returns>
        public static SpectralActivity CalculateSpectralEvents(double[,] spectrogram, double dbThreshold, TimeSpan frameStepDuration, int lowFreqBound, int midFreqBound, double binWidth)
        {
            //calculate boundaries between hi, mid and low frequency spectrum
            //int freqBinCount = spectrogram.GetLength(1);
            int lowFreqBinIndex = (int)Math.Ceiling(lowFreqBound / binWidth);
            int midFreqBinIndex = (int)Math.Ceiling(midFreqBound / binWidth);
            int highFreqBinIndex = spectrogram.GetLength(1) - 1; // avoid top row which can have edge effects
            int rows = spectrogram.GetLength(0); // frames
            int cols = spectrogram.GetLength(1); // # of freq bins
            double recordingDurationInSeconds = rows * frameStepDuration.TotalSeconds;

            SummaryActivity activity;
            double[] coverSpectrum = new double[cols];
            double[] eventSpectrum = new double[cols];
            // for each frequency bin, calculate coverage
            for (int c = 0; c < cols; c++) 
            {
                // get the freq bin containing dB values
                double[] bin = MatrixTools.GetColumn(spectrogram, c); 

                // get activity and event info
                activity = ActivityAndCover.CalculateActivity(bin, frameStepDuration, dbThreshold);
                //bool[] a1 = activity.activeFrames;
                //int a2 = activity.activeFrameCount;
                coverSpectrum[c] = activity.fractionOfActiveFrames; 
                //double a4 = activity.activeAvDB;
                eventSpectrum[c] = activity.eventCount / recordingDurationInSeconds; 
                //TimeSpan a6 = activity.avEventDuration;
                //bool[] a7 = activity.eventLocations;
            }


            // calculate coverage for low freq band as a percentage
            int count = 0;
            double sum = 0;
            for (int j = 0; j < lowFreqBinIndex; j++) { sum += coverSpectrum[j]; count++; }
            double lowFreqCover = sum / (double)count;
            
            // calculate coverage for mid freq band as a percentage
            count = 0;
            sum = 0;
            for (int j = lowFreqBinIndex; j < midFreqBinIndex; j++)
            {
                sum += coverSpectrum[j];
                count++;
            }
            double midFreqCover = sum / (double)count;
            
            // calculate coverage for high freq band as a percentage
            count = 0;
            sum = 0;
            for (int j = midFreqBinIndex; j < highFreqBinIndex; j++)
            {
                sum += coverSpectrum[j];
                count++;
            }
            double highFreqCover = sum / (double)count;

            return new SpectralActivity(eventSpectrum, coverSpectrum, lowFreqCover, midFreqCover, highFreqCover);
        }
    }
}
