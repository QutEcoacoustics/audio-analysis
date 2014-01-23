using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace AudioAnalysisTools
{

    /// <summary>
    /// a set of indices to describe level of acoustic activity in recording.
    /// </summary>
    public struct Activity
    {
        public double activeFrameCover, activeAvDB;
        public TimeSpan avSegmentDuration;
        public int activeFrameCount, segmentCount;
        public bool[] activeFrames, segmentLocations;

        public Activity(bool[] _activeFrames, int _activeFrameCount, double _activity, double _activeAvDB, int _segmentCount, TimeSpan _avSegmentLength, bool[] _segments)
        {
            activeFrames = _activeFrames;
            activeFrameCount = _activeFrameCount;
            activeFrameCover = _activity;
            activeAvDB = _activeAvDB;
            segmentCount = _segmentCount;
            avSegmentDuration = _avSegmentLength;
            segmentLocations = _segments;
        }
    } // struct Activity



    public static class ActivityAndCover
    {

        public const double DEFAULT_activityThreshold_dB = 3.0; // used to select frames that have 3dB > background


        /// <summary>
        /// reutrns the number of active frames and acoustic segments and their average duration in milliseconds
        /// only counts a segment if it is LONGER than one frame. 
        /// count segments as number of transitions from active to non-active frame
        /// </summary>
        /// <param name="activeFrames"></param>
        /// <param name="frameDuration">frame duration in seconds</param>
        /// <returns></returns>
        public static Activity CalculateActivity(double[] dBarray, TimeSpan frameDuration, double db_Threshold)
        {
            bool[] activeFrames = new bool[dBarray.Length];
            bool[] segments = new bool[dBarray.Length];
            double activeAvDB = 0.0;
            int activeFrameCount = 0;

            // get frames with activity >= threshold dB above background and count
            for (int i = 0; i < dBarray.Length; i++)
            {
                if (dBarray[i] >= DEFAULT_activityThreshold_dB)
                {
                    activeFrames[i] = true;
                    activeAvDB += dBarray[i];
                    activeFrameCount++;
                }
            }

            //int activeFrameCount = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB));  // this more elegant but want to keep active frame array
            double percentActivity = activeFrameCount / (double)dBarray.Length;
            if (activeFrameCount != 0) activeAvDB /= (double)activeFrameCount;

            if (activeFrameCount <= 1)
                return new Activity(activeFrames, activeFrameCount, percentActivity, activeAvDB, 0, TimeSpan.Zero, segments);


            // store record of segments longer than one frame
            segments = activeFrames;
            for (int i = 1; i < activeFrames.Length - 1; i++)
            {
                if (!segments[i - 1] && segments[i] && !segments[i + 1])
                    segments[i] = false; //remove solitary active frames
            }

            int segmentCount = 0;
            for (int i = 2; i < activeFrames.Length; i++)
            {
                if (!segments[i] && segments[i - 1] && segments[i - 2]) //count the ends of active segments
                    segmentCount++;
            }

            if (segmentCount == 0)
                return new Activity(activeFrames, activeFrameCount, percentActivity, activeAvDB, segmentCount, TimeSpan.Zero, segments);

            int segmentFrameCount = DataTools.CountTrues(segments);
            var avSegmentDuration = TimeSpan.Zero;

            if (segmentFrameCount > 0)
                avSegmentDuration = TimeSpan.FromSeconds(frameDuration.TotalSeconds * segmentFrameCount / (double)segmentCount);   //av segment duration in milliseconds

            return new Activity(activeFrames, activeFrameCount, percentActivity, activeAvDB, segmentCount, avSegmentDuration, segments);
        } // CalculateActivity()






        /// <summary>
        /// returns fraction coverage of the low, middle and high freq bands of the spectrum
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="bgThreshold"></param>
        /// <param name="lowFreqBound">Herz</param>
        /// <param name="midFreqBound">Herz</param>
        /// <param name="nyquist">Herz</param>
        /// <param name="binWidth">Herz per bin i.e. column in spectrogram - spectrogram rotated wrt to normal view.</param>
        /// <returns></returns>
        public static Tuple<double, double, double, double[]> CalculateSpectralCoverage(double[,] spectrogram, double bgThreshold, int lowFreqBound, int midFreqBound, double binWidth)
        {
            //calculate boundary between hi, mid and low frequency spectrum
            //int freqBinCount = spectrogram.GetLength(1);
            int lowFreqBinIndex = (int)Math.Ceiling(lowFreqBound / binWidth);
            int midFreqBinIndex = (int)Math.Ceiling(midFreqBound / binWidth);
            int highFreqBinIndex = spectrogram.GetLength(1) - 1; // avoid top row which can have edge effects
            int rows = spectrogram.GetLength(0); // frames
            int cols = spectrogram.GetLength(1); // # of freq bins

            double[] coverSpectrum = new double[cols];
            for (int c = 0; c < cols; c++) // calculate coverage for each freq band
            {
                int cover = 0;
                for (int r = 0; r < rows; r++) // for all rows of spectrogram
                {
                    if (spectrogram[r, c] >= bgThreshold) cover++;
                }
                coverSpectrum[c] = cover / (double)rows;
            }
            //calculate coverage for low freq band
            int count = 0;
            double sum = 0;
            for (int j = 0; j < lowFreqBinIndex; j++) { sum += coverSpectrum[j]; count++; }
            double lowFreqCover = sum / (double)count;
            //calculate coverage for mid freq band
            count = 0;
            sum = 0;
            for (int j = lowFreqBinIndex; j < midFreqBinIndex; j++) { sum += coverSpectrum[j]; count++; }
            double midFreqCover = sum / (double)count;
            //calculate coverage for high freq band
            count = 0;
            sum = 0;
            for (int j = midFreqBinIndex; j < highFreqBinIndex; j++) { sum += coverSpectrum[j]; count++; }
            double highFreqCover = sum / (double)count;

            return System.Tuple.Create(lowFreqCover, midFreqCover, highFreqCover, coverSpectrum);
        } //CalculateSpectralCoverage()



    }
}
