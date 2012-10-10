using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using TowseyLib;

namespace AudioAnalysisTools
{
    public class SpectralTrack
    {
        private int startFrame;
        public int StartFrame { get {return startFrame;} }
        private int endFrame;
        public int EndFrame { get { return endFrame; } }
        public int Length { get { return (endFrame - startFrame + 1); } }
        int bottomBin;
        int topBin;
        private double avBin;
        public double AverageBin { get { return avBin; } }
        List<int> track;
        int status = 0;  // 0=closed;   1= open and active;

        //public double[] amplitude;
        public double[] periodicity;
        public double[] periodicityScore;
        public double avPeriodicity { get { // calculate periodicity form midpoint of the array
            int midPoint = periodicity.Length /2;
            double average = (periodicity[midPoint - 2] + periodicity[midPoint] + periodicity[midPoint+ 2]) / 3;
            return average; 
        } }
        public double avPeriodicityScore { get { return periodicityScore.Average(); } }

        double framesPerSecond;
        public double herzPerBin;

        double tolerance = 2.5; // do not accept new track if new peak is > this distance from old track.
        public  const int    MAX_FREQ_BOUND        = 6000;  // herz
        private const int    MAX_INTRASYLLABLE_GAP = 30;  // milliseconds
        private const double MIN_TRACK_DENSITY     = 0.3;
        public  const int MIN_TRACK_DURATION = 20;  // milliseconds




        //public SpectralTrack(int _start, int _bin)
        //{
        //    startFrame = _start;
        //    endFrame   = _start;
        //    bottomBin  = _bin;
        //    topBin     = _bin;
        //    avBin      = _bin;
        //    status = 1;
        //    track = new List<int>();
        //    track.Add(_bin);
        //}

        public SpectralTrack(int _start, int _bin, double _framesPerSecond, double _herzPerBin)
        {
            startFrame = _start;
            endFrame   = _start;
            bottomBin  = _bin;
            topBin     = _bin;
            avBin      = _bin;
            status = 1;
            track = new List<int>();
            track.Add(_bin);
            SetTimeAndFreqScales(_framesPerSecond, _herzPerBin);
        }

        public void SetTimeAndFreqScales(double _framesPerSecond, double _herzPerBin)
        {
            framesPerSecond = _framesPerSecond;
            herzPerBin      = _herzPerBin;
        }

        int FrameCount(int milliseconds)
        {
            return ConvertMilliseconds2FrameCount(milliseconds, framesPerSecond);
        }

        int BinCount(int herz)
        {
            return (int)Math.Round(herz / (double)herzPerBin);
        }

        public bool TrackTerminated(int currentFrame)
        {
            bool trackTerminated = true;
            int minFrameGap = this.FrameCount(MAX_INTRASYLLABLE_GAP);
            if ((this.endFrame + minFrameGap) > currentFrame) trackTerminated = false;
            return trackTerminated;
        }

        public double Density()
        {
            double density = this.track.Count / (double)this.Length;
            return density;
        }


        public bool ExtendTrack(int currentFrame, int currentValue)
        {
            if ((currentValue > (this.avBin + tolerance)) || (currentValue < (this.avBin - tolerance)))  //current position NOT within range of this track
            {
                return false;
            }

            //can extend this track
            this.endFrame = currentFrame;
            if (bottomBin > currentValue) bottomBin = currentValue;
            else
            if (topBin < currentValue) topBin = currentValue;
            this.track.Add(currentValue);
            double av, sd;
            NormalDist.AverageAndSD(this.track.ToArray(), out av, out sd);
            this.avBin = av;
            return true;
        }

        public void CropTrack(List<double[]> listOfFrequencyBins, double severity)
        {
            int length = listOfFrequencyBins[0].Length; // assume all bins of same length
            int binID  = (int)this.AverageBin;

            double[] subArray = DataTools.Subarray(listOfFrequencyBins[binID], this.StartFrame, this.Length);
            subArray = DataTools.filterMovingAverage(subArray, 3); // smooth to remove aberrant peaks
            int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, severity);

            this.endFrame = this.StartFrame + bounds[1];
            this.startFrame += bounds[0];
        }

        public void CropTrack(BaseSonogram sonogram, double threshold)
        {
            //int length = sonogram.FrameCount;
            int binID = (int)this.AverageBin;
            double[] freqBin = MatrixTools.GetColumn(sonogram.Data, binID);

            double[] subArray = DataTools.Subarray(freqBin, this.StartFrame, this.Length);
            int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, threshold);

            this.endFrame = this.StartFrame + bounds[1];
            this.startFrame += bounds[0];
        }


        //#########################################################################################################################################################
        //#########################################################################################################################################################
        //#########################################################################################################################################################

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectralPeakArray">array (one element per frame) indicating which freq bin has max amplitude</param>
        /// <param name="_framesPerSecond">time scale</param>
        /// <param name="_herzPerBin">freq scale</param>
        /// <returns></returns>
        public static List<SpectralTrack> GetSpectraltracks(int[] spectralPeakArray, double _framesPerSecond, double _herzPerBin)
        {
            var tracks = new List<SpectralTrack>();
            for (int r = 0; r < spectralPeakArray.Length - 1; r++)
            {
                if (spectralPeakArray[r] == 0) continue;  //skip frames with zero value i.e. did not have peak > threshold.
                PruneTracks(tracks, r);
                if (!ExtendTrack(tracks, r, spectralPeakArray[r]))
                    tracks.Add(new SpectralTrack(r, spectralPeakArray[r], _framesPerSecond, _herzPerBin));
            }
            return tracks;
        }

        /// <summary>
        /// Prunes a list of tracks.
        /// A track is a consecutive series of peaks in the same or adjacent frequency bins.
        /// This method removes tracks that do not satisfy THREE conditions:
        /// 1: length is less than default number of frames (threshold given in seconds)
        /// 2: average freq of the track is below a threshold frequency
        /// 3: track density is lower than threshold - density means that over given duration, % frames having that freq max exceeds a threshold. 
        /// </summary>
        /// <param name="tracks">current list of tracks</param>
        /// <param name="currentFrame"></param>
        public static void PruneTracks(List<SpectralTrack> tracks, int currentFrame)
        {
            if ((tracks == null) || (tracks.Count == 0)) return;

            int maxFreqBin = UpperTrackBound(tracks[0].herzPerBin);

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                if (tracks[i].status == 0) continue;

                if (tracks[i].TrackTerminated(currentFrame))  //this track has terminated
                {
                    tracks[i].status = 0; //set track status to closed
                    int minFrameLength = tracks[i].FrameCount(MIN_TRACK_DURATION);
                    if ((tracks[i].Length < minFrameLength) || (tracks[i].avBin > maxFreqBin) || (tracks[i].Density() < MIN_TRACK_DENSITY)) 
                        tracks.RemoveAt(i);
                }
            }
        } //PruneTracks()

        public static int UpperTrackBound(double herzPerBin)
        {
            return (int)Math.Round(MAX_FREQ_BOUND / herzPerBin);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracks"></param>
        /// <param name="currentFrame"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public static bool ExtendTrack(List<SpectralTrack> tracks, int currentFrame, int currentValue)
        {
            if ((tracks == null) || (tracks.Count == 0)) return false;

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                if (tracks[i].status == 0) continue; //already closed
                if (tracks[i].ExtendTrack(currentFrame, currentValue)) //extend track if possible and return true when a track has been extended
                    return true;
            }
            return false; //no track was able to be extended.
        } //ExtendTrack()


        public static void DetectTrackPeriodicity(SpectralTrack track, int xCorrelationLength, List<double[]> listOfSpectralBins, double framesPerSecond)
        {
            int halfSample = xCorrelationLength / 2;
            int lowerBin = (int)Math.Round(track.AverageBin);
            int upperBin = lowerBin + 1;
            int length = track.Length;
            //only sample the middle third of track
            int start = length / 3;
            int end = start + start - 1; 
            //init score track and periodicity track
            double[] score = new double[start];
            double[] period = new double[start];

            for (int r = start; r < end; r++) // for each position in centre third of track
            {
                int sampleStart = track.StartFrame - halfSample + r;
                if (sampleStart < 0) sampleStart = 0;
                double[] lowerSubarray = DataTools.Subarray(listOfSpectralBins[lowerBin], sampleStart, xCorrelationLength);
                double[] upperSubarray = DataTools.Subarray(listOfSpectralBins[upperBin], sampleStart, xCorrelationLength);

                if ((lowerSubarray == null) || (upperSubarray == null)) break; //reached end of array
                if ((lowerSubarray.Length != xCorrelationLength) || (upperSubarray.Length != xCorrelationLength)) break; //reached end of array
                lowerSubarray = DataTools.SubtractMean(lowerSubarray); // zero mean the arrays
                upperSubarray = DataTools.SubtractMean(upperSubarray);
                //upperSubarray = lowerSubarray;

                var xCorSpectrum = CrossCorrelation.CrossCorr(lowerSubarray, upperSubarray); //sub-arrays already normalised
                //DataTools.writeBarGraph(xCorSpectrum);

                //Set the minimum OscilFreq of interest = 8 per second. Therefore max period ~ 125ms;
                //int 0.125sec = 2 * xCorrelationLength / minInterestingID / framesPerSecond; //
                double maxPeriod = 0.05; //maximum period of interest
                int minInterestingID = (int)Math.Round(2 * xCorrelationLength / maxPeriod / framesPerSecond); //
                for (int s = 0; s <= minInterestingID; s++) xCorSpectrum[s] = 0.0;  //in real data these low freq/long period bins are dominant and hide other frequency content
                int maxIdXcor = DataTools.GetMaxIndex(xCorSpectrum);
                period[r-start] = 2 * xCorrelationLength / (double)maxIdXcor / framesPerSecond; //convert maxID to period in seconds
                score[r - start] = xCorSpectrum[maxIdXcor];
            } // for loop
            track.periodicityScore = score;
            track.periodicity      = period;
            //if (track.score.Average() < 0.3) track = null;

        }


        public static List<AcousticEvent> ConvertTracks2Events(List<SpectralTrack> tracks /*, double framesPerSecond, double herzPerBin*/)
        {
            if (tracks == null) return null;
            var list = new List<AcousticEvent>();
            if (tracks.Count == 0) return list;

            foreach (SpectralTrack track in tracks)
            {
                double startTime = track.startFrame / track.framesPerSecond;
                int frameDuration = track.endFrame - track.startFrame + 1;
                double duration = frameDuration / track.framesPerSecond;
                double minFreq = track.herzPerBin * (track.avBin - 1);
                double maxFreq = track.herzPerBin * (track.avBin + 1);
                AcousticEvent ae = new AcousticEvent(startTime, duration, minFreq, maxFreq);
                ae.SetTimeAndFreqScales(track.framesPerSecond, track.herzPerBin);
                ae.Name = "";
                ae.colour = Color.Blue;
                ae.DominantFreq = track.AverageBin * track.herzPerBin;
                ae.Periodicity  = track.avPeriodicity;
                ae.Score        = track.avPeriodicityScore;
                list.Add(ae);
            }

            return list;
        }


        public static int ConvertMilliseconds2FrameCount(int milliseconds, double framesPerSecond)
        {
            return (int)Math.Round(framesPerSecond * milliseconds / (double)1000);
        }


    } //class SpectralTrack
}
