using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{
    public class SpectralTrack
    {
        int startFrame;
        int endFrame;
        int minBin;
        int maxBin;
        double avBin;
        List<int> track;
        int status = 0;  //0=closed;   1= open and active;

        double tolerance = 2.0; //do not accept new track if new peak is > this distance from old track.


        public SpectralTrack(int _start, int _bin)
        {
            startFrame = _start;
            track = new List<int>();
            track.Add(_bin);
        }



        public static List<SpectralTrack> GetSpectraltracks(int[] peakArray)
        {
            var list = new List<SpectralTrack>();
            for (int r = 0; r < peakArray.Length - 4; r++)
            {
                if ((peakArray[r] == peakArray[r + 1]) && (peakArray[r] == peakArray[r + 2]) && (peakArray[r] == peakArray[r + 3]) && (peakArray[r] == peakArray[r + 4]))
                {
                    SpectralTrack track = new SpectralTrack(r, peakArray[r]);
                    track.endFrame = r + 4;
                    track.minBin = peakArray[r] - 1;
                    track.maxBin = peakArray[r] + 1;
                    list.Add(track);
                    r += 4;
                }
            }
            return list;
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
                double minFreq = herzPerBin * track.minBin;
                double maxFreq = herzPerBin * track.maxBin;
                AcousticEvent ae = new AcousticEvent(startTime, duration, minFreq, maxFreq);
                ae.SetTimeAndFreqScales(framesPerSecond, herzPerBin);
                ae.Name = "";
                list.Add(ae);
            }

            return list;
        }


    } //class SpectralTrack
}
