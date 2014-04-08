using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using TowseyLib;

namespace AudioAnalysisTools.Indices
{
    public static class  RainIndices
    {
        public const string header_rain = "rain";
        public const string header_cicada = "cicada";
        public const string header_negative = "none";



        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct RainStruct
        {
            public double snr, bgNoise, activity, spikes, avSig_dB, temporalEntropy; //amplitude indices
            public double lowFreqCover, midFreqCover, hiFreqCover, spectralEntropy;  //, entropyOfVarianceSpectrum; //spectral indices
            public double ACI;

            public RainStruct(double _snr, double _bgNoise, double _avSig_dB, double _activity, double _spikes,
                            double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                            double _entropyOfAvSpectrum, double _ACI)
            {
                snr = _snr;
                bgNoise = _bgNoise;
                activity = _activity;
                spikes = _spikes;
                avSig_dB = _avSig_dB;
                temporalEntropy = _entropyAmp;
                hiFreqCover = _hiFreqCover;
                midFreqCover = _midFreqCover;
                lowFreqCover = _lowFreqCover;
                spectralEntropy = _entropyOfAvSpectrum;
                ACI = _ACI;
            }
        } //struct Indices






        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal">envelope of the original signal</param>
        /// <param name="audioDuration"></param>
        /// <param name="frameDuration"></param>
        /// <param name="spectrogram">the original amplitude spectrum BUT noise reduced</param>
        /// <param name="lowFreqBound"></param>
        /// <param name="midFreqBound"></param>
        /// <param name="binWidth">derived from original nyquist and window/2</param>
        /// <returns></returns>
        public static DataTable GetIndices(double[] signal, TimeSpan audioDuration, TimeSpan frameDuration, double[,] spectrogram, int lowFreqBound, int midFreqBound, double binWidth)
        {
            int chunkDuration = 10; //seconds - assume that signal is not less than one minute duration

            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            int chunkCount = (int)Math.Round(audioDuration.TotalSeconds / (double)chunkDuration);
            int framesPerChunk = (int)(chunkDuration * framesPerSecond);
            int nyquistBin = spectrogram.GetLength(1);

            string[] classifications = new string[chunkCount];

            //get acoustic indices and convert to rain indices.
            var sb = new StringBuilder();
            for (int i = 0; i < chunkCount; i++)
            {
                int startSecond = i * chunkDuration;
                int start = (int)(startSecond * framesPerSecond);
                int end = start + framesPerChunk;
                if (end >= signal.Length) end = signal.Length - 1;
                double[] chunkSignal = DataTools.Subarray(signal, start, framesPerChunk);
                if (chunkSignal.Length < 50) continue;  //an arbitrary minimum length
                double[,] chunkSpectro = DataTools.Submatrix(spectrogram, start, 1, end, nyquistBin - 1);

                RainStruct rainIndices = Get10SecondIndices(chunkSignal, chunkSpectro, lowFreqBound, midFreqBound, binWidth);
                string classification = RainIndices.ConvertAcousticIndices2Classifcations(rainIndices);
                classifications[i] = classification;

                //write indices and clsasification info to console
                string separator = ",";
                string line = String.Format("{1:d2}{0} {2:d2}{0} {3:f1}{0} {4:f1}{0} {5:f1}{0} {6:f2}{0} {7:f3}{0} {8:f2}{0} {9:f2}{0} {10:f2}{0} {11:f2}{0} {12:f2}{0} {13:f2}{0} {14}", separator,
                                      startSecond, (startSecond + chunkDuration),
                                      rainIndices.avSig_dB, rainIndices.bgNoise, rainIndices.snr,
                                      rainIndices.activity, rainIndices.spikes, rainIndices.ACI,
                                      rainIndices.lowFreqCover, rainIndices.midFreqCover, rainIndices.hiFreqCover,
                                      rainIndices.temporalEntropy, rainIndices.spectralEntropy, classification);

                //if (verbose)
                if (false)
                {
                    LoggedConsole.WriteLine(line);
                }
                //FOR PREPARING SEE.5 DATA  -------  write indices and clsasification info to file
                //sb.AppendLine(line);
            }

            //FOR PREPARING SEE.5 DATA   ------    write indices and clsasification info to file
            //string opDir = @"C:\SensorNetworks\Output\Rain";
            //string opPath = Path.Combine(opDir, recording.FileName + ".Rain.csv");
            //FileTools.WriteTextFile(opPath, sb.ToString());

            var dt = ConvertClassifcations2Datatable(classifications);
            foreach (DataRow row in dt.Rows)
            {
                row[Keys.SEGMENT_TIMESPAN] = (double)audioDuration.TotalSeconds;
            }
            return dt;
        } //Analysis()

        /// <summary>
        /// returns some indices relevant to rain and cicadas from a short (10seconds) chunk of audio
        /// </summary>
        /// <param name="signal">signal envelope of a 10s chunk of audio</param>
        /// <param name="spectrogram">spectrogram of a 10s chunk of audio</param>
        /// <param name="lowFreqBound"></param>
        /// <param name="midFreqBound"></param>
        /// <param name="binWidth"></param>
        /// <returns></returns>
        public static RainStruct Get10SecondIndices(double[] signal, double[,] spectrogram, int lowFreqBound, int midFreqBound, double binWidth)
        {
            // i: FRAME ENERGIES - 
            double StandardDeviationCount = 0.1;
            var results3 = SNR.SubtractBackgroundNoiseFromWaveform_dB(SNR.Signal2Decibels(signal), StandardDeviationCount); //use Lamel et al.
            var dBarray = SNR.TruncateNegativeValues2Zero(results3.noiseReducedSignal);

            bool[] activeFrames = new bool[dBarray.Length]; //record frames with activity >= threshold dB above background and count
            for (int i = 0; i < dBarray.Length; i++) if (dBarray[i] >= ActivityAndCover.DEFAULT_activityThreshold_dB) activeFrames[i] = true;
            //int activeFrameCount = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB)); 
            int activeFrameCount = DataTools.CountTrues(activeFrames);

            double spikeThreshold = 0.05;
            double spikeIndex = RainIndices.CalculateSpikeIndex(signal, spikeThreshold);
            //Console.WriteLine("spikeIndex=" + spikeIndex);
            //DataTools.writeBarGraph(signal);

            RainStruct rainIndices; // struct in which to store all indices
            rainIndices.activity = activeFrameCount / (double)dBarray.Length;  //fraction of frames having acoustic activity 
            rainIndices.bgNoise = results3.NoiseMode;                         //bg noise in dB
            rainIndices.snr = results3.Snr;                               //snr
            rainIndices.avSig_dB = 20 * Math.Log10(signal.Average());        //10 times log of amplitude squared 
            rainIndices.temporalEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(signal)); //ENTROPY of ENERGY ENVELOPE
            rainIndices.spikes = spikeIndex;

            // ii: calculate the bin id of boundary between mid and low frequency spectrum
            int lowBinBound = (int)Math.Ceiling(lowFreqBound / binWidth);
            var midbandSpectrogram = MatrixTools.Submatrix(spectrogram, 0, lowBinBound, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);

            // iii: ENTROPY OF AVERAGE SPECTRUM and VARIANCE SPECTRUM - at this point the spectrogram is still an amplitude spectrogram
            var tuple = SpectrogramTools.CalculateSpectralAvAndVariance(midbandSpectrogram);
            rainIndices.spectralEntropy = DataTools.Entropy_normalised(tuple.Item1); //ENTROPY of spectral averages
            if (double.IsNaN(rainIndices.spectralEntropy)) rainIndices.spectralEntropy = 1.0;

            // iv: CALCULATE Acoustic Complexity Index on the AMPLITUDE SPECTRUM
            var aciArray = AcousticComplexityIndex.CalculateACI(midbandSpectrogram);
            rainIndices.ACI = aciArray.Average();

            //v: remove background noise from the spectrogram
            double spectralBgThreshold = 0.015;      // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            //double[] modalValues = SNR.CalculateModalValues(spectrogram); //calculate modal value for each freq bin.
            //modalValues = DataTools.filterMovingAverage(modalValues, 7);  //smooth the modal profile
            //spectrogram = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogram, modalValues);
            //spectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogram, spectralBgThreshold);

            //vi: SPECTROGRAM ANALYSIS - SPECTRAL COVER. NOTE: spectrogram is still a noise reduced amplitude spectrogram
            var tuple3 = ActivityAndCover.CalculateSpectralCoverage(spectrogram, spectralBgThreshold, lowFreqBound, midFreqBound, binWidth);
            rainIndices.lowFreqCover = tuple3.Item1;
            rainIndices.midFreqCover = tuple3.Item2;
            rainIndices.hiFreqCover = tuple3.Item3;
            // double[] coverSpectrum = tuple3.Item4;

            return rainIndices;
        }

        public static double CalculateSpikeIndex(double[] envelope, double spikeThreshold)
        {
            int length = envelope.Length;
            // int isolatedSpikeCount = 0;
            double peakIntenisty = 0.0;
            double spikeIntensity = 0.0;

            var peaks = DataTools.GetPeaks(envelope);
            int peakCount = 0;
            for (int i = 1; i < length - 1; i++)
            {
                if (!peaks[i]) continue; //count spikes
                peakCount++;
                double diffMinus1 = Math.Abs(envelope[i] - envelope[i - 1]);
                double diffPlus1 = Math.Abs(envelope[i] - envelope[i + 1]);
                double avDifference = (diffMinus1 + diffPlus1) / 2;
                peakIntenisty += avDifference;
                if (avDifference > spikeThreshold)
                {
                    //isolatedSpikeCount++; // count isolated spikes
                    spikeIntensity += avDifference;
                }
            }
            if (peakCount == 0) return 0.0;
            return spikeIntensity / peakIntenisty;
        } //CalculateSpikeIndex()


        public static DataTable ConvertClassifcations2Datatable(string[] classifications)
        {
            string[] headers = { Keys.INDICES_COUNT, Keys.START_MIN, Keys.SEGMENT_TIMESPAN, header_rain, header_cicada };
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double) };

            int length = classifications.Length;
            int rainCount = 0;
            int cicadaCount = 0;
            for (int i = 0; i < length; i++)
            {
                if (classifications[i] == header_rain) rainCount++;
                if (classifications[i] == header_cicada) cicadaCount++;
            }

            var dt = DataTableTools.CreateTable(headers, types);
            dt.Rows.Add(0, 0.0, 0.0,  //add dummy values to the first three columns. These will be entered later.
                        (rainCount / (double)length), (cicadaCount / (double)length)
                        );
            return dt;
        }

        /// <summary>
        /// The values in this class were derived from See5 runs data extracted from 
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static string ConvertAcousticIndices2Classifcations(RainStruct indices)
        {
            string classification = header_negative;
            if (indices.spikes > 0.2)
            {
                if (indices.hiFreqCover > 0.24) return header_rain;
                else return header_negative;
            }
            else
            {
                if (indices.spectralEntropy < 0.61) return header_cicada;
                if (indices.bgNoise > -24) return header_cicada;
            }
            return classification;
        }


    } // end Class
}
