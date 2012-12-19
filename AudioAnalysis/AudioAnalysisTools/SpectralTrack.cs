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

        // scale
        public double framesPerSecond;
        public TimeSpan timeOffset; // time from beginning of the current recording where track starts
        public double herzPerBin;
        public int    herzOffset; // the bottom of the spectrogram may not be 0 Herz. required for accurate determination of track frequency

        // PARAMETERS
        public static int herzTolerance = 110; // do not continue an existing track if the next freq peak is > this freq distance from existing track freq
        public  const int    MAX_FREQ_BOUND        = 6000;  // herz
        private const double MIN_TRACK_DENSITY     = 0.3;
        public static TimeSpan MIN_TRACK_DURATION = TimeSpan.FromMilliseconds(20);     // milliseconds - this is defulat value - can be over-ridden
        public static TimeSpan MAX_INTRASYLLABLE_GAP = TimeSpan.FromMilliseconds(30);  // milliseconds


        public SpectralTrack(int _start, int _bin, double _framesPerSecond, double _herzPerBin, int _herzOffset)
        {
            startFrame = _start;
            endFrame   = _start;
            bottomBin  = _bin;
            topBin     = _bin;
            avBin      = _bin;
            status = 1;
            track = new List<int>();
            track.Add(_bin);
            TimeSpan t = TimeSpan.FromSeconds(_start / _framesPerSecond); 
            SetTimeAndFreqScales(_framesPerSecond, t, _herzPerBin, _herzOffset);
        }

        public void SetTimeAndFreqScales(double _framesPerSecond, TimeSpan t, double _herzPerBin, int _herzOffset)
        {
            framesPerSecond = _framesPerSecond;
            timeOffset      = t;
            herzPerBin      = _herzPerBin;
            herzOffset      = _herzOffset;
        }

        int FrameCountEquivalent(TimeSpan duration)
        {
            return FrameCountEquivalent(duration, framesPerSecond);          
        }

        int BinCount(int herz)
        {
            return (int)Math.Round(herz / (double)herzPerBin);
        }

        public bool TrackTerminated(int currentFrame, TimeSpan maxGap)
        {
            bool trackTerminated = true;
            int permittedFrameGap = this.FrameCountEquivalent(maxGap);
            if ((this.endFrame + permittedFrameGap) > currentFrame) trackTerminated = false;
            return trackTerminated;
        }

        public double Density()
        {
            double density = this.track.Count / (double)this.Length;
            return density;
        }

        public TimeSpan Duration()
        {
            double seconds = this.Length / framesPerSecond;  
            return TimeSpan.FromSeconds(seconds);
        }


        public bool ExtendTrack(int currentFrame, int currentValue, double binTolerance)
        {
            if ((currentValue > (this.avBin + binTolerance)) || (currentValue < (this.avBin - binTolerance)))  //current position NOT within range of this track
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

        public int GetFrequency(int t)
        {
            return (int)Math.Round((this.track[t] * this.herzPerBin) + this.herzOffset);
        }

        public void DrawTrack(Graphics g, double sonogramFramesPerSecond, double sonogramFreqBinWidth, int sonogramHeight)
        {
            Pen p1 = new Pen(AcousticEvent.DEFAULT_BORDER_COLOR, 2); // default colour
            double secondsPerTrackFrame = 1 / this.framesPerSecond;

            double startSec = this.timeOffset.TotalSeconds;
            int frame1 = (int)Math.Round(startSec * sonogramFramesPerSecond);
            for (int i = 1; i < track.Count - 1; i++)
            {
                double endSec = startSec + (i * secondsPerTrackFrame);
                int frame2 = (int)Math.Round(endSec * sonogramFramesPerSecond);
                //int freqBin = (int)Math.Round(this.MinFreq / freqBinWidth);
                int f1 = this.GetFrequency(i);
                int f1Bin = (int)Math.Round(f1 / sonogramFreqBinWidth);
                int y1 = sonogramHeight - f1Bin - 1;
                int f2 = this.GetFrequency(i+1);
                int f2Bin = (int)Math.Round(f2 / sonogramFreqBinWidth);
                int y2 = sonogramHeight - f2Bin - 1;
                g.DrawLine(p1, frame1, y1, frame2, y2);
                //startSec = endSec;
                frame1 = frame2;
            }
            //g.DrawString(this.Name, new Font("Tahoma", 8), Brushes.Black, new PointF(t1, y - 1));
        }



        //public void DrawEvent(Graphics g, double framesPerSecond, double freqBinWidth, int sonogramHeight)
        //{
        //    Pen p1 = new Pen(AcousticEvent.DEFAULT_BORDER_COLOR); // default colour
        //    Pen p2 = new Pen(AcousticEvent.DEFAULT_SCORE_COLOR);
        //    if (this.BorderColour != null) p1 = new Pen(this.BorderColour);

        //    //calculate top and bottom freq bins
        //    int minFreqBin = (int)Math.Round(this.MinFreq / freqBinWidth);
        //    int maxFreqBin = (int)Math.Round(this.MaxFreq / freqBinWidth);
        //    int height = maxFreqBin - minFreqBin + 1;
        //    int y = sonogramHeight - maxFreqBin - 1;

        //    //calculate start and end time frames
        //    int t1 = 0;
        //    int tWidth = 0;
        //    double duration = this.TimeEnd - this.TimeStart;
        //    if ((duration != 0.0) && (framesPerSecond != 0.0))
        //    {
        //        t1 = (int)Math.Round(this.TimeStart * framesPerSecond); //temporal start of event
        //        tWidth = (int)Math.Round(duration * framesPerSecond);
        //    }
        //    else if (this.oblong != null)
        //    {
        //        t1 = this.oblong.r1; //temporal start of event
        //        tWidth = this.oblong.r2 - t1 + 1;
        //    }

        //    g.DrawRectangle(p1, t1, y, tWidth, height);

        //    //draw the score bar to indicate relative score
        //    int scoreHt = (int)Math.Round(height * this.ScoreNormalised);
        //    int y1 = y + height;
        //    int y2 = y1 - scoreHt;
        //    g.DrawLine(p2, t1 + 1, y1, t1 + 1, y2);
        //    g.DrawLine(p2, t1 + 2, y1, t1 + 2, y2);
        //    //g.DrawLine(p2, t1 + 3, y1, t1 + 3, y2);
        //    g.DrawString(this.Name, new Font("Tahoma", 8), Brushes.Black, new PointF(t1, y - 1));
        //}


        //#########################################################################################################################################################
        //#########################################################################################################################################################
        //#########################################################################################################################################################


        public static int FrameCountEquivalent(TimeSpan duration, double framesPerSecond)
        {
            return (int)Math.Round(framesPerSecond * duration.TotalSeconds);
        }

        /// <summary>
        /// returns an array showing which freq bin in each frame has the maximum amplitude
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static int[] GetSpectralMaxima(double[,] spectrogram, double threshold)
        {
            int rowCount = spectrogram.GetLength(0);
            int colCount = spectrogram.GetLength(1);
            var maxFreqArray = new int[rowCount]; //array (one element per frame) indicating which freq bin has max amplitude.
            //var hitsMatrix = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);
                spectrum = DataTools.filterMovingAverage(spectrum, 3); // smoothing to remove noise
                //find local freq maxima and store in freqArray & hits matrix.
                int maxFreqbin = DataTools.GetMaxIndex(spectrum);
                if (spectrum[maxFreqbin] > threshold) //only record spectral peak if it is above threshold.
                {
                    maxFreqArray[r] = maxFreqbin;
                    //hitsMatrix[r + nh, maxFreqbin] = 1.0;
                }
            }
            return maxFreqArray;
        } // GetSpectralMaxima()

        /// <summary>
        /// returns an array showing which freq bin in each frame has the maximum amplitude.
        /// However only returns values for those frames in the neighbourhood of an envelope peak.
        /// </summary>
        /// <param name="decibelsPerFrame"></param>
        /// <param name="spectrogram"></param>
        /// <param name="threshold"></param>
        /// <param name="nhLimit"></param>
        /// <returns></returns>
        public static System.Tuple<int[], double[,]> GetSpectralMaxima(double[] decibelsPerFrame, double[,] spectrogram, double threshold, int nhLimit)
        {
            int rowCount = spectrogram.GetLength(0);
            int colCount = spectrogram.GetLength(1);

            var peaks = DataTools.GetPeakValues(decibelsPerFrame);

            var maxFreqArray = new int[rowCount]; //array (one element per frame) indicating which freq bin has max amplitude.
            var hitsMatrix   = new double[rowCount, colCount];
            for (int r = nhLimit; r < rowCount - nhLimit; r++)
            {
                if (peaks[r] < threshold) continue;
                //find local freq maxima and store in freqArray & hits matrix.
                for (int nh = -nhLimit; nh < nhLimit; nh++)
                {
                    double[] spectrum = MatrixTools.GetRow(spectrogram, r + nh);
                    spectrum[0] = 0.0; // set DC = 0.0 just in case it is max.
                    int maxFreqbin = DataTools.GetMaxIndex(spectrum);
                    if (spectrum[maxFreqbin] > threshold) //only record spectral peak if it is above threshold.
                    {
                        maxFreqArray[r + nh] = maxFreqbin;
                        //if ((spectrum[maxFreqbin] > dBThreshold) && (sonogram.Data[r, maxFreqbin] >= sonogram.Data[r - 1, maxFreqbin]) && (sonogram.Data[r, maxFreqbin] >= sonogram.Data[r + 1, maxFreqbin]))
                        hitsMatrix[r + nh, maxFreqbin] = 1.0;
                    }
                }
            }
            return System.Tuple.Create(maxFreqArray, hitsMatrix);
        } // GetSpectralMaxima()

        public static List<SpectralTrack> GetSpectralPeakTracks(double[,] spectrogram, double framesPerSecond, double herzPerBin, int herzOffset, double threshold, TimeSpan minDuration, TimeSpan permittedGap, int maxFreq)
        {
            int[] spectralPeakArray = GetSpectralMaxima(spectrogram, threshold);
            var tracks = GetSpectraltracks(spectralPeakArray, framesPerSecond, herzPerBin, herzOffset, minDuration, permittedGap, maxFreq);
            // WriteHistogramOftrackLengths(tracks);
            return tracks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectralPeakArray">array (one element per frame) indicating which freq bin has max amplitude</param>
        /// <param name="_framesPerSecond">time scale</param>
        /// <param name="_herzPerBin">freq scale</param>
        /// <returns></returns>
        public static List<SpectralTrack> GetSpectraltracks(int[] spectralPeakArray, double _framesPerSecond, double _herzPerBin, int _herzOffset, TimeSpan minDuration, TimeSpan permittedGap, int maxFreq)
        {
            double binTolerance = SpectralTrack.herzTolerance / _herzPerBin;
            var tracks = new List<SpectralTrack>();
            for (int r = 0; r < spectralPeakArray.Length - 1; r++)
            {
                if (spectralPeakArray[r] == 0) continue;  //skip frames with zero value i.e. did not have peak > threshold.
                PruneTracks(tracks, r, minDuration, permittedGap, maxFreq);
                if (!ExtendTrack(tracks, r, spectralPeakArray[r], binTolerance))
                    tracks.Add(new SpectralTrack(r, spectralPeakArray[r], _framesPerSecond, _herzPerBin, _herzOffset));
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
        public static void PruneTracks(List<SpectralTrack> tracks, int currentFrame, TimeSpan minDuration, TimeSpan permittedGap, int maxFreq)
        {
            if ((tracks == null) || (tracks.Count == 0)) return;

            int maxFreqBin = (int)Math.Round(maxFreq / tracks[0].herzPerBin);

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                if (tracks[i].status == 0) continue;

                if (tracks[i].TrackTerminated(currentFrame, permittedGap))  //this track has terminated
                {
                    tracks[i].status = 0; //set track status to closed
                    //int minFrameLength = tracks[i].FrameCountEquivalent(minimumDuration);
                    if ((tracks[i].Duration() < minDuration) || (tracks[i].avBin > maxFreqBin) || (tracks[i].Density() < MIN_TRACK_DENSITY)) 
                        tracks.RemoveAt(i);
                }
            }
        } //PruneTracks()


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracks"></param>
        /// <param name="currentFrame"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public static bool ExtendTrack(List<SpectralTrack> tracks, int currentFrame, int currentValue, double binTolerance)
        {
            if ((tracks == null) || (tracks.Count == 0)) return false;

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                if (tracks[i].status == 0) continue; //already closed
                if (tracks[i].ExtendTrack(currentFrame, currentValue, binTolerance)) //extend track if possible and return true when a track has been extended
                    return true;
            }
            return false; //no track was able to be extended.
        } //ExtendTrack()


        public static void WriteHistogramOftrackLengths(List<SpectralTrack> tracks)
        {
            var lengths = new int[tracks.Count];
            for(int i = 0; i < tracks.Count; i++)
            {
                lengths[i] = tracks[i].Length;
            }
            DataTools.writeConciseHistogram(lengths);
            int[] histo = DataTools.Histo_FixedWidth(lengths, 1, 0, 20);
            DataTools.writeBarGraph(histo);
        }

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
                ae.BorderColour = Color.Blue;
                ae.DominantFreq = track.AverageBin * track.herzPerBin;
                ae.Periodicity  = track.avPeriodicity;
                ae.Score        = track.avPeriodicityScore;
                list.Add(ae);
            }

            return list;
        }

    } //class SpectralTrack
}
