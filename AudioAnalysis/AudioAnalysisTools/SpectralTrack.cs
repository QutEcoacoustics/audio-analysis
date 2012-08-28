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
        int bottomBin;
        int topBin;
        double avBin;
        List<int> track;
        int status = 0;  //0=closed;   1= open and active;

        double tolerance = 2.0; //do not accept new track if new peak is > this distance from old track.
        int maxGap = 10;
        int maxFreqBin = 10;



        public SpectralTrack(int _start, int _bin)
        {
            startFrame = _start;
            endFrame   = _start;
            bottomBin  = _bin;
            topBin     = _bin;
            avBin      = _bin;
            status = 1;
            track = new List<int>();
            track.Add(_bin);
        }

        public bool TrackTerminated(int currentFrame)
        {
            bool trackTerminated = true;
            if ((this.endFrame + maxGap) > currentFrame) trackTerminated = false;
            return trackTerminated;
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
        
        
        public static List<SpectralTrack> GetSpectraltracks(int[] spectralPeakArray, int maxFreqBin)
        {
            var tracks = new List<SpectralTrack>();
            for (int r = 0; r < spectralPeakArray.Length - 1; r++)
            {
                if (spectralPeakArray[r] == 0) continue;  //skip frames with below threshold peak.
                PruneTracks(tracks, r, maxFreqBin);
                if (!ExtendTrack(tracks, r, spectralPeakArray[r])) tracks.Add(new SpectralTrack(r, spectralPeakArray[r]));
            }
            return tracks;
        }


        public static void PruneTracks(List<SpectralTrack> tracks, int currentFrame, int maxFreqBin)
        {
            if ((tracks == null) ||(tracks.Count == 0)) return;

            int minTrackLength = 20;

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                if (tracks[i].status == 0) continue;

                if (tracks[i].TrackTerminated(currentFrame))  //this track has terminated
                {
                    tracks[i].status = 0; //closed
                    int length = tracks[i].endFrame - tracks[i].startFrame + 1;
                    if ((length < minTrackLength)||(tracks[i].avBin > maxFreqBin)) 
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

        public static void ProcessNextFrame(List<SpectralTrack> tracks, int currentFrame, int currentValue)
        {
            if (!ExtendTrack(tracks, currentFrame, currentValue)) tracks.Add(new SpectralTrack(currentFrame, currentValue));
        }


        public static List<AcousticEvent> ConvertTracks2Events(List<SpectralTrack> tracks, double framesPerSecond, double herzPerBin)
        {
            if (tracks == null) return null;
            var list = new List<AcousticEvent>();
            if (tracks.Count == 0) return list;

            foreach (SpectralTrack track in tracks)
            {
                double startTime = track.startFrame / framesPerSecond;
                int frameDuration = track.endFrame - track.startFrame + 1;
                double duration = frameDuration / framesPerSecond;
                double minFreq = herzPerBin * (track.avBin-1);
                double maxFreq = herzPerBin * (track.avBin+1);
                AcousticEvent ae = new AcousticEvent(startTime, duration, minFreq, maxFreq);
                ae.SetTimeAndFreqScales(framesPerSecond, herzPerBin);
                ae.Name = "";
                ae.colour = Color.Blue;
                list.Add(ae);
            }

            return list;
        }


    } //class SpectralTrack
}
