using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{



    /// <summary>
    /// this struct describes spectral tracks ie whistles and chirps.
    /// </summary>
    public struct TrackInfo
    {
        public List<SpectralTrack> tracks;
        public TimeSpan totalTrackDuration;
        public int percentDuration; // percent of recording length
        public TrackInfo(List<SpectralTrack> _tracks, TimeSpan _totalTrackDuration, int _percentDuration)
        {
            tracks = _tracks;
            totalTrackDuration = _totalTrackDuration;
            percentDuration = _percentDuration;
        }
    } // TrackInfo()


    public static class SpectralTracks
    {


        public static TrackInfo GetTrackIndices(double[,] spectrogram, double framesPerSecond, double binWidth, int herzOffset, double threshold)
        {
            var minDuration = TimeSpan.FromMilliseconds(150);
            var permittedGap = TimeSpan.FromMilliseconds(100);
            int maxFreq = 10000;

            var tracks = SpectralTrack.GetSpectralPeakTracks(spectrogram, framesPerSecond, binWidth, herzOffset, threshold, minDuration, permittedGap, maxFreq);
            var duration = TimeSpan.Zero;
            int trackLength = 0;
            foreach (SpectralTrack track in tracks)
            {
                duration += track.Duration();
                trackLength += track.Length;
            }
            int percentDuration = (int)Math.Round(100 * trackLength / (double)spectrogram.GetLength(0));
            return new TrackInfo(tracks, duration, percentDuration);
        }



    }
}
