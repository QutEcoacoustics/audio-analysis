using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioAnalysisTools
{



    /// <summary>
    /// this struct describes spectral peak tracks ie whistles and chirps.
    /// </summary>
    public struct SPTrackInfo
    {
        public double[,] peaks;
        public List<SpectralTrack> listOfSPTracks;
        public TimeSpan totalTrackDuration;
        public int trackCount;
        public int percentDuration; // percent of recording length
        public double[] spSpectrum;

        public SPTrackInfo(double[,] _peaks, List<SpectralTrack> _tracks, TimeSpan _totalTrackDuration, int _percentDuration, double[] _spSpectrum)
        {
            peaks = _peaks;
            listOfSPTracks = _tracks;
            totalTrackDuration = _totalTrackDuration;
            percentDuration = _percentDuration;
            spSpectrum = _spSpectrum;
            trackCount = 0; // set value elsewhere
        }

    } // SPTrackInfo()


    public static class SpectralTracks
    {

        /// <summary>
        /// Called only from AcousticIndicesCalculate.Analysis()
        /// NOTE: Orientation of passed spectrogram is: row = frames, columns = frequency bins
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="binWidth"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static SPTrackInfo GetSpectralPeakIndices(double[,] spectrogram, double framesPerSecond, double threshold)
        {
            //var minDuration = TimeSpan.FromMilliseconds(150);
            //var permittedGap = TimeSpan.FromMilliseconds(100);
            //int maxFreq = 10000;

            var rowCount = spectrogram.GetLength(0);
            var colCount = spectrogram.GetLength(1);


            double[,] peaks = LocalPeaks(spectrogram, threshold);

            double[] spectrum = new double[colCount];
            double[] freqBin;
            int numberOfSeconds = (int)(rowCount / framesPerSecond);
            int trackCount, totalFrameLength;
            int totalTrackCount = 0;
            int cummulativeFrameCount = 0;
            for (int col = 0; col < colCount; col++)
            {
                freqBin = MatrixTools.GetColumn(peaks, col);
                //int count = ProcessFrequencyBinForTracks1(freqBin, framesPerSecond);
                ProcessFrequencyBinForTracks2(freqBin, framesPerSecond, out trackCount, out totalFrameLength); // returns total frames in tracks
                // add data to spectrum
                spectrum[col] = totalFrameLength / (double)rowCount;  // fraction of frames covered by tracks
                spectrum[col] *= 2.0; //just to scale it up to look brighter in spectrogram!!! a fudge - needs more work!!
                if (spectrum[col] > 1.0) spectrum[col] = 1.0; // normalise

                totalTrackCount += trackCount;             // accumulate counts over all frequency bins
                cummulativeFrameCount += totalFrameLength; // accumulate track frames over all frequency bins 
                //spectrum[col] = totalTrackCount / (double)numberOfSeconds; //track count per second
            }

            List<SpectralTrack> tracks = null;
            TimeSpan totalTrackDuration = TimeSpan.FromSeconds((double)totalTrackCount);
            int percentDuration = (int)(totalTrackDuration.TotalSeconds * 100 / numberOfSeconds);

            var info = new SPTrackInfo(peaks, tracks, totalTrackDuration, percentDuration, spectrum);
            info.trackCount = totalTrackCount;
            return info;
        }


        public static int ProcessFrequencyBinForTracks1(double[] freqBin, double framesPerSecond)
        {
            int framesPerHalfSecond = (int)(framesPerSecond / 2);
            int framesPerQuaterSecond = (int)(framesPerSecond / 4);
            //int numberOfHalfSeconds = (int)(rowCount / framesPerHalfSecond);
            int framesPerSegment = (int)framesPerSecond;
            int numberOfSeconds = (int)(freqBin.Length / framesPerSecond);
            int count = 0;
            int start = 0;
            for (int hs = 0; hs < numberOfSeconds; hs++)
            {
                double[] segment = DataTools.Subarray(freqBin, start, framesPerSegment);
                int peakCount = segment.Count(value => (value > 0.0));
                if (peakCount > framesPerQuaterSecond) count++;
                if (peakCount > framesPerHalfSecond)
                    count++; // give extra weight to segments with longer tracks
                if (peakCount >= framesPerSegment - 3)
                    count++; // give extra weight to segments with longer tracks
                //double sum = segment.Sum();
                //if (sum > threshold) count++;
                //if (sum > peakThreshold) count++; //effectively give extra weight to segments with greater amplitude
                start += framesPerSegment;
            }
            return count;
        }

        public static void ProcessFrequencyBinForTracks2(double[] freqBin, double framesPerSecond, out int trackCount, out int totalFrameLength)
        {
            double[] smoothedBin = DataTools.filterMovingAverage(freqBin, 3); // to join up gaps of 1 - 2
            List<int> trackLengths = new List<int>();
            bool inTrack = false;
            int trackLength = 0;
            double threshold = 0.0;
            for (int i = 0; i < freqBin.Length; i++)
            {
                if ((!inTrack) && (smoothedBin[i] > threshold)) // start a track
                {
                    inTrack = true;
                    trackLength = 1;
                }
                else
                    if (inTrack && (smoothedBin[i] > threshold)) // extend a track
                    {
                        inTrack = true;
                        trackLength ++;
                    }
                    else
                        if (inTrack && (smoothedBin[i] <= threshold)) // end a track
                        {
                            inTrack = false;
                            trackLengths.Add(trackLength);
                            trackLength = 0;
                        }
            }

            trackCount = 0;
            totalFrameLength = 0;

            if (trackLengths.Count == 0) return;

            //convert list of track lengths to histogram.
            int min, max;
            int[] histogram = Histogram.Histo(trackLengths.ToArray(), out min, out max);

            int minimumTrackLength = 333; // milliseconds
            int minimumFrameLength = (int)(minimumTrackLength * framesPerSecond / 1000.0);
            int startBin = minimumFrameLength - min + 1;
            if (startBin < 0) startBin = 0;
           
            // sum the track lengths
            for (int i = startBin; i < histogram.Length; i++)
            {
                trackCount += histogram[i];
                totalFrameLength += (histogram[i] * (min + i));
            }
        }

        /// <summary>
        /// DEPRACATED
        /// Called only from AcousticIndicesCalculate.Analysis()
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="binWidth"></param>
        /// <param name="dBThreshold"></param>
        /// <returns></returns>
        //public static SPTrackInfo GetSpectralPeackTrackIndices(double[,] spectrogram, double framesPerSecond, double binWidth, int herzOffset, double threshold)
        //{
        //    var minDuration = TimeSpan.FromMilliseconds(150);
        //    var permittedGap = TimeSpan.FromMilliseconds(100);
        //    int maxFreq = 10000;

        //    var spTracks = SpectralTrack.GetSpectralPeakTracks(spectrogram, framesPerSecond, binWidth, herzOffset, threshold, minDuration, permittedGap, maxFreq);
        //    var duration = TimeSpan.Zero;
        //    int trackLength = 0;
        //    foreach (SpectralTrack track in spTracks)
        //    {
        //        duration += track.Duration();
        //        trackLength += track.Length;
        //    }
        //    int percentDuration = (int)Math.Round(100 * trackLength / (double)spectrogram.GetLength(0));
        //    return new SPTrackInfo(spTracks, duration, percentDuration);
        //}

        public static double[,] LocalPeaks(double[,] dBSpectrogram, double dBThreshold)
        {

            var rowCount = dBSpectrogram.GetLength(0);
            var colCount = dBSpectrogram.GetLength(1);

            double[,] localpeaks = new double[rowCount, colCount];
            int columnBuffer = 1;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = columnBuffer; col < (colCount - columnBuffer); col++)
                {
                    if (dBSpectrogram[row, col] <= dBThreshold) continue; // skip small values

                    if (   (dBSpectrogram[row, col] > dBSpectrogram[row, col + 1])
                        && (dBSpectrogram[row, col] > dBSpectrogram[row, col - 1])
                        //&& (dBSpectrogram[row, col] > dBSpectrogram[row, col + 2])
                        //&& (dBSpectrogram[row, col] > dBSpectrogram[row, col - 2])
                        // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 3])
                        // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 3])
                        )
                       // if (((dBSpectrogram[row, col] - dBSpectrogram[row, col + 1]) > 0.0)
                       // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 1]) > 0.0)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 2]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 2]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 3]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 3]) > dBThreshold)
                       // )
                    {
                        localpeaks[row, col] = dBSpectrogram[row, col];
                    }
                }
            }
            return localpeaks;
        } // LocalPeaks()




    }
}
