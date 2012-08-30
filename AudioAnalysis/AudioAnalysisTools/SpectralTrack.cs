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
        int startFrame;
        int endFrame;
        public int Length { get {return (endFrame - startFrame + 1);} }
        int bottomBin;
        int topBin;
        double avBin;
        List<int> track;
        int status = 0;  // 0=closed;   1= open and active;

        double framesPerSecond;
        double herzPerBin;

        double tolerance = 2.5; // do not accept new track if new peak is > this distance from old track.
        public  const int    MAX_FREQ_BOUND        = 6000;  // herz
        private const int    MAX_INTRASYLLABLE_GAP = 30;  // milliseconds
        private const int    MIN_TRACK_DURATION    = 20;  // milliseconds
        private const double MIN_TRACK_DENSITY     = 0.3;




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
            return (int)Math.Round(framesPerSecond * milliseconds / (double)1000);
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


        //#########################################################################################################################################################
        //#########################################################################################################################################################
        //#########################################################################################################################################################


        public static List<SpectralTrack> GetSpectraltracks(int[] spectralPeakArray, double _framesPerSecond, double _herzPerBin)
        {
            var tracks = new List<SpectralTrack>();
            for (int r = 0; r < spectralPeakArray.Length - 1; r++)
            {
                if (spectralPeakArray[r] == 0) continue;  //skip frames with below threshold peak.
                PruneTracks(tracks, r);
                if (!ExtendTrack(tracks, r, spectralPeakArray[r])) tracks.Add(new SpectralTrack(r, spectralPeakArray[r], _framesPerSecond, _herzPerBin));
            }
            return tracks;
        }


        public static void PruneTracks(List<SpectralTrack> tracks, int currentFrame)
        {
            if ((tracks == null) ||(tracks.Count == 0)) return;

            int maxFreqBin = (int)(MAX_FREQ_BOUND / tracks[0].herzPerBin);

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                if (tracks[i].status == 0) continue;

                if (tracks[i].TrackTerminated(currentFrame))  //this track has terminated
                {
                    tracks[i].status = 0; //closed
                    int length = tracks[i].Length;
                    int minFrameLength = tracks[i].FrameCount(MIN_TRACK_DURATION);
                    if ((length < minFrameLength) || (tracks[i].avBin > maxFreqBin) || (tracks[i].Density() < MIN_TRACK_DENSITY)) 
                        tracks.RemoveAt(i);
                }
            }
        } //PruneTracks()


        public static bool ExtendTrack(List<SpectralTrack> tracks, int currentFrame, int currentValue)
        {
            if ((tracks == null) || (tracks.Count == 0)) return false;

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                if (tracks[i].status == 0) continue; //already closed
                if (tracks[i].ExtendTrack(currentFrame, currentValue)) return true;
            }
            return false;
        } //ExtendTrack()

        //public static void ProcessNextFrame(List<SpectralTrack> tracks, int currentFrame, int currentValue)
        //{
        //    if (!ExtendTrack(tracks, currentFrame, currentValue)) 
        //        tracks.Add(new SpectralTrack(currentFrame, currentValue, _framesPerSecond, _herzPerBin));
        //}


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
                list.Add(ae);
            }

            return list;
        }


    } //class SpectralTrack
}
