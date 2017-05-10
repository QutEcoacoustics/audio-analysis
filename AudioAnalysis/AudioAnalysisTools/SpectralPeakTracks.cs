// <copyright file="SpectralPeakTracks.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using System.Linq;
    using DSP;
    using StandardSpectrograms;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// Finds and stores info about spectral peak tracks ie whistles and chirps in the passed spectrogram.
    /// </summary>
    public class SpectralPeakTracks
    {
        private static readonly string[] RidgeKeys = { "SPT", "RVT", "RHZ", "RPS", "RNG", "R3D" };

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectralPeakTracks"/> class.
        /// CONSTRUCTOR NOTE: Orientation of passed spectrogram is: row = spectral frames, columns = frequency bins
        /// </summary>
        public SpectralPeakTracks(double[,] dBSpectrogram, double peakThreshold)
        {
            // double framesStepsPerSecond = 1 / frameStepTimeSpan.TotalSeconds;
            this.GetPeakTracksSpectrum(dBSpectrogram, peakThreshold);

            // this method was written just before leaving for Toulon to work with Herve Glotin.
            // It was changed while in Toulon to the following line which does not require a threshold.
            // this.GetRidgeSpectraVersion1(dBSpectrogram, ridgeThreshold: 4.0);
            this.GetRidgeSpectraVersion2(dBSpectrogram);
            this.CalculateCombinationOfThreeDirections();
        }

        public static string[] GetDefaultRidgeKeys()
        {
            return RidgeKeys;
        }

        public double[,] Peaks { get; private set; }

        //public int TotalTrackCount { get; private set; }
        //public TimeSpan AvTrackDuration { get; private set; }

        /// <summary>
        /// Gets average number of tracks per frame
        /// </summary>
        public double TrackDensity { get; private set; }

        /// <summary>
        /// Gets the fractional peak cover; i.e. fraction of frames in freq bin that are a spectral peak.
        /// </summary>
        public double[] SptSpectrum { get; private set; }

        /// <summary>
        /// Gets spectrum of horizontal ridges
        /// </summary>
        public double[] RhzSpectrum { get; private set; }

        /// <summary>
        /// Gets spectrum of vertical ridges
        /// </summary>
        public double[] RvtSpectrum { get; private set; }

        /// <summary>
        /// Gets spectrum of positive slope ridges
        /// </summary>
        public double[] RpsSpectrum { get; private set; }

        /// <summary>
        /// gets spectrum of negative slope ridges
        /// </summary>
        public double[] RngSpectrum { get; private set; }

        /// <summary>
        /// gets three directions ridge value
        /// </summary>
        public double[] R3DSpectrum { get; private set; }

        public void GetRidgeSpectraVersion1(double[,] dbSpectrogramData, double ridgeThreshold)
        {
            int rowCount = dbSpectrogramData.GetLength(0);
            int colCount = dbSpectrogramData.GetLength(1);
            int spanCount = rowCount - 4; // 4 because 5x5 grid means buffer of 2 on either side

            double[,] matrix = dbSpectrogramData;

            //double[,] matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            // returns a byte matrix of ridge directions
            // 0 = no ridge detected or below magnitude threshold.
            // 1 = ridge direction = horizontal or slope = 0;
            // 2 = ridge is positive slope or pi/4
            // 3 = ridge is vertical or pi/2
            // 4 = ridge is negative slope or 3pi/4.
            //byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);
            byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionVersion1(matrix, ridgeThreshold);

            //image for debugging
            //ImageTools.DrawMatrix(hits, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram.png");

            double[] spectrum = new double[colCount];
            byte[] freqBin;

            //Now aggregate hits to get ridge info
            //note that the Spectrograms were passed in flat-rotated orientation.
            //Therefore need to assign ridge number to re-oriented values.
            // Accumulate info for the horizontal ridges
            for (int col = 0; col < colCount; col++)
            {
                // i.e. for each frequency bin
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x == 3);
                if (count < 2)
                {
                    continue; // i.e. not a track.
                }

                spectrum[col] = count / (double)spanCount;
            }

            this.RhzSpectrum = spectrum;

            // accumulate info for the vertical ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++)
            {
                // i.e. for each frequency bin
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x == 1);
                if (count < 2)
                {
                    continue; // i.e. not a track.
                }

                spectrum[col] = count / (double)spanCount;
            }

            this.RvtSpectrum = spectrum;

            // accumulate info for the up slope ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++)
            {
                // i.e. for each frequency bin
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x == 4);
                spectrum[col] = count / (double)spanCount;
            }

            this.RpsSpectrum = spectrum;

            // accumulate info for the down slope ridges
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++)
            {
                // i.e. for each frequency bin
                freqBin = MatrixTools.GetColumn(hits, col);
                int count = freqBin.Count(x => x == 2);
                spectrum[col] = count / (double)spanCount;
            }

            this.RngSpectrum = spectrum;
        }

        public void GetRidgeSpectraVersion2(double[,] dbSpectrogramData)
        {
            int rowCount = dbSpectrogramData.GetLength(0);
            int colCount = dbSpectrogramData.GetLength(1);

            // calculate span = number of cells over which will take average of a feature.
            // -4 because 5x5 grid means buffer of 2 on either side
            int spanCount = rowCount - 4;

            double[,] matrix = dbSpectrogramData;

            //ImageTools.DrawMatrix(matrix, @"C:\SensorNetworks\Output\BIRD50\temp\SpectrogramBeforeWeinerFilter.png");

            // DO NOT USE WIENER FILTERING because smooths the ridges and lose definition
            //matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            //ImageTools.DrawMatrix(matrix, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogramAfterWeinerFilter.png");

            // returns a byte matrix of ridge directions
            // 0 = ridge direction = horizontal or slope = 0;
            // 1 = ridge is positive slope or pi/4
            // 2 = ridge is vertical or pi/2
            // 3 = ridge is negative slope or 3pi/4.
            List<double[,]> hits = RidgeDetection.Sobel5X5RidgeDetection_Version2(matrix);

            //image for debugging
            //ImageTools.DrawMatrix(hits[0], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram0.png");
            //ImageTools.DrawMatrix(hits[1], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram1.png");
            //ImageTools.DrawMatrix(hits[2], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram2.png");
            //ImageTools.DrawMatrix(hits[3], 0, 10.0, @"C:\SensorNetworks\Output\BIRD50\temp\hitsSpectrogram3.png");

            double[] spectrum = new double[colCount];
            double sum;

            //Now aggregate hits to get ridge info
            //note that the Spectrograms were passed in flat-rotated orientation.
            //Therefore need to assign ridge number to re-oriented values.

            // Accumulate info for the horizontal ridges
            var M = hits[2];

            // for each frequency bin
            for (int col = 0; col < colCount; col++)
            {
                sum = 0;
                for (int row = 2; row < rowCount - 2; row++)
                { sum += M[row, col]; }
                spectrum[col] = sum / (double)spanCount;
            }

            this.RhzSpectrum = spectrum;

            // accumulate info for the vertical ridges
            M = hits[0];
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++)
            {
                // i.e. for each frequency bin
                sum = 0;
                for (int row = 2; row < rowCount - 2; row++)
                { sum += M[row, col]; }
                spectrum[col] = sum / (double)spanCount;
            }

            this.RvtSpectrum = spectrum;

            // accumulate info for the positive/up-slope ridges
            M = hits[3];
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                sum = 0;
                // i.e. for each row or frame
                for (int row = 2; row < rowCount - 2; row++)
                {
                    sum += M[row, col];
                }

                spectrum[col] = sum / (double)spanCount;
            }

            this.RpsSpectrum = spectrum;

            // accumulate info for the negative/down slope ridges
            M = hits[1];
            spectrum = new double[colCount];
            for (int col = 0; col < colCount; col++) // i.e. for each frequency bin
            {
                sum = 0;
                for (int row = 2; row < rowCount - 2; row++)
                { sum += M[row, col]; }
                spectrum[col] = sum / (double)spanCount;
            }

            this.RngSpectrum = spectrum;
        }

        public void GetPeakTracksSpectrum(double[,] dBSpectrogram, double dBThreshold)
        {
            var rowCount = dBSpectrogram.GetLength(0);
            var colCount = dBSpectrogram.GetLength(1);
            int spanCount = rowCount - 4;

            this.Peaks = LocalSpectralPeaks(dBSpectrogram, dBThreshold);

            double[] spectrum = new double[colCount];
            int cummulativeFrameCount = 0;

            for (int col = 0; col < colCount; col++)
            {
                double sum = 0;
                int cover = 0;
                // i.e. for each row or frame
                for (int row = 2; row < rowCount - 2; row++)
                {
                    sum += this.Peaks[row, col];
                    if(this.Peaks[row, col] > 0.0) cover++;
                }

                spectrum[col] = sum / (double)spanCount;
                cummulativeFrameCount += cover;

                //freqBin = MatrixTools.GetColumn(this.Peaks, col);
                ////var tracksInOneBin = new TracksInOneFrequencyBin(col, freqBin, framesPerSecond);
                ////spectrum[col] = tracksInOneBin.CompositeTrackScore();  // add data to spectrum
                //int cover = freqBin.Count(x => x > 0.0);
                //if (cover < 3) continue; // i.e. not a track.
                //spectrum[col] = cover / (double)rowCount;
                //cummulativeFrameCount += cover;                         // accumulate track frames over all frequency bins
                ////this.TotalTrackCount += tracksInOneBin.TrackCount;    // accumulate counts over all frequency bins
            }

            this.SptSpectrum = spectrum;
            this.TrackDensity = cummulativeFrameCount / (double)spanCount;

            //double avFramesPerTrack = 0.0;
            //if (totalTrackCount > 0)
            //    avFramesPerTrack = cummulativeFrameCount / (double)totalTrackCount;
            //this.TotalTrackCount = totalTrackCount;
            //this.AvTrackDuration = TimeSpan.FromSeconds(avFramesPerTrack / framesPerSecond);
        }

        /// <summary>
        /// Calculates the max of the Horizontal, positive and negative slope ridges.
        /// Could alternatively calculate the sum of the Horizontal, positive and negative slope ridges.
        /// </summary>
        public void CalculateCombinationOfThreeDirections()
        {
            this.R3DSpectrum = new double[this.RhzSpectrum.Length];
            for (int i = 0; i < this.RhzSpectrum.Length; i++)
            {
                //var array = new double[] { this.RhzSpectrum[i], this.RpsSpectrum[i], this.RngSpectrum[i] };
                //this.R3DSpectrum[i] = array.Max();
                this.R3DSpectrum[i] = this.RhzSpectrum[i] + this.RpsSpectrum[i] + this.RngSpectrum[i];
            }
        }

        // ################################### STATIC METHODS BELOW HERE ########################################

        /// <summary>
        /// Finds local spectral peaks in a spectrogram, one frame at a time.
        /// IMPORTANT: Assume that the spectrogram matrix is oriented 90 degrees to visual orientation.
        /// i.e the rows = spectra; columns = freq bins.
        /// </summary>
        public static double[,] LocalSpectralPeaks(double[,] dBSpectrogram, double dBThreshold)
        {
            var rowCount = dBSpectrogram.GetLength(0);
            var colCount = dBSpectrogram.GetLength(1);

            double[,] localpeaks = new double[rowCount, colCount];
            int columnBuffer = 2;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = columnBuffer; col < colCount - columnBuffer; col++)
                {
                    if (dBSpectrogram[row, col] <= dBThreshold)
                    {
                        continue; // skip small values
                    }

                    if (dBSpectrogram[row, col] > dBSpectrogram[row, col + 1]
                        && dBSpectrogram[row, col] > dBSpectrogram[row, col - 1]
                        && dBSpectrogram[row, col] > dBSpectrogram[row, col + 2]
                        && dBSpectrogram[row, col] > dBSpectrogram[row, col - 2])

                       // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 3])
                       // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 3])
                       // if (((dBSpectrogram[row, col] - dBSpectrogram[row, col + 1]) > 0.0)
                       // && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 1]) > 0.0)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 2]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 2]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col + 3]) > dBThreshold)
                       //// && ((dBSpectrogram[row, col] - dBSpectrogram[row, col - 3]) > dBThreshold))
                    {
                        // localpeaks[row, col] = dBSpectrogram[row, col] - ((dBSpectrogram[row, col+2] + dBSpectrogram[row, col-2]) * 0.5);
                        localpeaks[row, col] = dBSpectrogram[row, col];
                    }
                }
            }

            return localpeaks;
        } // LocalPeaks()

        /// <summary>
        /// CALCULATEs SPECTRAL PEAK TRACKS.
        /// NOTE: We require a noise reduced decibel spectrogram
        /// FreqBinWidth can be accessed, if required, through dspOutput1.FreqBinWidth,
        /// </summary>
        public static SpectralPeakTracks CalculateSpectralPeakTracks(AudioRecording recording, int sampleStart, int sampleEnd, int frameSize, bool octaveScale, double peakThreshold)
        {
            double epsilon = recording.Epsilon;
            int sampleRate = recording.WavReader.SampleRate;
            int bufferFrameCount = 2; // 2 because must allow for edge effects when using 5x5 grid to find ridges.
            int ridgeBuffer = frameSize * bufferFrameCount;
            var ridgeRecording = AudioRecording.GetRecordingSubsegment(recording, sampleStart, sampleEnd, ridgeBuffer);
            int frameStep = frameSize;
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFfts(ridgeRecording, frameSize, frameStep);

            // Generate the ridge SUBSEGMENT deciBel spectrogram from the SUBSEGMENT amplitude spectrogram
            // i: generate the SUBSEGMENT deciBel spectrogram from the SUBSEGMENT amplitude spectrogram
            double[,] decibelSpectrogram;
            if (octaveScale)
            {
                var freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);
                decibelSpectrogram = OctaveFreqScale.DecibelSpectra(dspOutput.AmplitudeSpectrogram, dspOutput.WindowPower, sampleRate, epsilon, freqScale);
            }
            else
            {
                decibelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput.AmplitudeSpectrogram, dspOutput.WindowPower, sampleRate, epsilon);
            }

            // calculate the noise profile
            var spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(decibelSpectrogram);
            decibelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogram, spectralDecibelBgn);
            double nhDecibelThreshold = 2.0; // SPECTRAL dB THRESHOLD for smoothing background
            decibelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogram, nhDecibelThreshold);

            // thresholds in decibels
            // double frameStepDuration = frameStep / (double)sampleRate; // fraction of a second
            // TimeSpan frameStepTimeSpan = TimeSpan.FromTicks((long)(frameStepDuration * TimeSpan.TicksPerSecond));

            var sptInfo = new SpectralPeakTracks(decibelSpectrogram, peakThreshold);
            return sptInfo;
        }

        /// <summary>
        /// This method only called from Indexcalculate when returning image of the sonogram for the passed recording segment.
        /// </summary>
        public static double[] ConvertSpectralPeaksToNormalisedArray(double[,] spectrogram)
        {
            // convert spectral peaks to frequency and frames
            var tupleDecibelPeaks = SpectrogramTools.HistogramOfSpectralPeaks(spectrogram);

            // Item2 is length of Score Array and stores the bin in which the max peak is located.
            // Normalise this for display in score track
            return DataTools.normalise(tupleDecibelPeaks.Item2);
        }
    }
}
