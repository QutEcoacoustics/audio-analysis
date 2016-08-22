using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Recognizers
{
    using System.Reflection;

    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;
    using System.Drawing;

    /// <summary>
    /// This is a template recognizer
    /// </summary>
    class BlueCatfish : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "BlueCatfish";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, int? imageWidth)
        {

            bool doFiltering = false;
            //string path = @"C:\SensorNetworks\WavFiles\Freshwater\savedfortest.wav";
            //audioRecording.Save(path); // this does not work
            int sr = audioRecording.SampleRate;
            int nyquist = audioRecording.Nyquist;

            // Get a value from the config file - with a backup default
            //int minHz = (int?)configuration[AnalysisKeys.MinHz] ?? 600;

            // Get a value from the config file - with no default, throw an exception if value is not present
            //int maxHz = ((int?)configuration[AnalysisKeys.MaxHz]).Value;

            // Get a value from the config file - without a string accessor, as a double
            //double someExampleSettingA = (double?)configuration.someExampleSettingA ?? 0.0;

            // common properties
            //string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            //string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            // get samples
            var samples = audioRecording.WavReader.Samples;

            // scale parameters
            int waveScalingFactor = 10000;
            int imageHeight = 400;

            double scoreThreshold = 0.75;
            double[] bandPassFilteredSignal = null;

            double[] truePositivesA;
            double[] truePositivesB;

            if (doFiltering)
            {
                waveScalingFactor = 10000;

                // high pass filter
                double[] hiPassFilteredSignal = DSP_Filters.PreEmphasis(samples, 1.0);

                //low pass filter
                string filterName = "Chebyshev_Lowpass_1000, scale*5";
                DSP_IIRFilter filter = new DSP_IIRFilter(filterName);
                int order = filter.order;
                System.LoggedConsole.WriteLine("\nTest " + filterName + ", order=" + order);
                double[] signalLowPassFiltered;
                filter.ApplyIIRFilter(hiPassFilteredSignal, out signalLowPassFiltered);

                // high pass filter
                bandPassFilteredSignal = DSP_Filters.PreEmphasis(signalLowPassFiltered, 1.0);

                // these are used for scoring 
                double[] truePositives1 = { 0.0000, 0.0000, 0.0000, 0.0000, 0.0001, 0.0006, 0.0014, 0.0015, 0.0010, 0.0002, 0.0001, 0.0001, 0.0000, 0.0000, 0.0000, 0.0000, 0.0003, 0.0005, 0.0006, 0.0005, 0.0003, 0.0002, 0.0001, 0.0002, 0.0007, 0.0016, 0.0026, 0.0035, 0.0037, 0.0040, 0.0046, 0.0040, 0.0031, 0.0022, 0.0048, 0.0133, 0.0149, 0.0396, 0.1013, 0.1647, 0.2013, 0.2236, 0.2295, 0.1836, 0.1083, 0.0807, 0.0776, 0.0964, 0.1116, 0.0987, 0.1065, 0.1575, 0.3312, 0.4829, 0.5679, 0.5523, 0.4412, 0.2895, 0.2022, 0.2622, 0.2670, 0.2355, 0.1969, 0.2220, 0.6600, 0.9023, 1.0000, 0.8099, 0.8451, 0.8210, 0.5511, 0.1756, 0.0319, 0.0769, 0.0738, 0.2235, 0.3901, 0.4565, 0.4851, 0.3703, 0.3643, 0.2497, 0.2705, 0.3456, 0.3096, 0.1809, 0.0710, 0.0828, 0.0857, 0.0953, 0.1308, 0.1387, 0.0590 };
                double[] truePositives2 = { 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0001, 0.0001, 0.0001, 0.0001, 0.0000, 0.0000, 0.0001, 0.0001, 0.0003, 0.0004, 0.0004, 0.0002, 0.0001, 0.0001, 0.0003, 0.0003, 0.0006, 0.0007, 0.0020, 0.0127, 0.0256, 0.0426, 0.0512, 0.0560, 0.0414, 0.0237, 0.0133, 0.0107, 0.0091, 0.0077, 0.0085, 0.0165, 0.0144, 0.0308, 0.0416, 0.0454, 0.0341, 0.0191, 0.0128, 0.0058, 0.0026, 0.0081, 0.0139, 0.0313, 0.0404, 0.0493, 0.0610, 0.1951, 0.4083, 0.5616, 0.5711, 0.5096, 0.4020, 0.2917, 0.1579, 0.1421, 0.1461, 0.1406, 0.2098, 0.1676, 0.2758, 0.2875, 0.6513, 0.9374, 1.0000, 0.7576, 0.4130, 0.2622, 0.1495, 0.0973, 0.0623, 0.0425, 0.0205, 0.0034, 0.0065, 0.0054, 0.0089, 0.0138, 0.0208, 0.0204, 0.0168, 0.0136, 0.0149, 0.0155, 0.0106, 0.0086, 0.0099, 0.0187 };
                truePositivesA = truePositives1;
                truePositivesB = truePositives2;
            }
            else // do not filter because already filtered
            {
                bandPassFilteredSignal = samples;
                waveScalingFactor = 5;

                // these are used for scoring 
                double[] truePositives1 = { 0.0014, 0.0012, 0.0009, 0.0003, 0.0001, 0.0005, 0.0008, 0.0029, 0.0057, 0.0070, 0.0069, 0.0063, 0.0053, 0.0032, 0.0013, 0.0011, 0.0011, 0.0007, 0.0000, 0.0006, 0.0010, 0.0013, 0.0008, 0.0009, 0.0022, 0.0046, 0.0069, 0.0082, 0.0070, 0.0065, 0.0082, 0.0078, 0.0052, 0.0021, 0.0132, 0.0357, 0.0420, 0.0996, 0.2724, 0.4557, 0.5739, 0.6366, 0.6155, 0.4598, 0.2334, 0.1468, 0.1410, 0.1759, 0.2157, 0.1988, 0.2131, 0.3072, 0.6161, 0.8864, 1.0000, 0.9290, 0.6983, 0.4208, 0.2690, 0.3190, 0.3109, 0.2605, 0.1896, 0.2118, 0.5961, 0.8298, 0.9290, 0.7363, 0.6605, 0.5840, 0.3576, 0.1019, 0.0162, 0.0400, 0.0405, 0.1106, 0.1803, 0.2083, 0.2058, 0.1475, 0.1387, 0.0870, 0.0804, 0.0975, 0.0848, 0.0490, 0.0193, 0.0217, 0.0210, 0.0214, 0.0253, 0.0254, 0.0072 };
                double[] truePositives2 = { 0.0090, 0.0106, 0.0138, 0.0134, 0.0088, 0.0026, 0.0002, 0.0002, 0.0003, 0.0000, 0.0001, 0.0006, 0.0013, 0.0019, 0.0020, 0.0015, 0.0008, 0.0004, 0.0002, 0.0015, 0.0022, 0.0073, 0.0195, 0.0628, 0.2203, 0.4031, 0.5635, 0.5445, 0.4828, 0.2869, 0.1498, 0.0588, 0.0500, 0.0542, 0.0641, 0.1188, 0.1833, 0.1841, 0.2684, 0.3062, 0.2831, 0.1643, 0.0606, 0.0336, 0.0136, 0.0056, 0.0187, 0.0301, 0.0700, 0.1103, 0.1559, 0.2449, 0.5303, 0.8544, 1.0000, 0.8361, 0.6702, 0.4839, 0.3463, 0.1525, 0.1049, 0.1201, 0.1242, 0.2056, 0.1653, 0.2685, 0.2947, 0.5729, 0.7024, 0.6916, 0.4765, 0.2488, 0.1283, 0.0543, 0.0326, 0.0236, 0.0187, 0.0108, 0.0021, 0.0028, 0.0019, 0.0024, 0.0041, 0.0063, 0.0066, 0.0055, 0.0036, 0.0025, 0.0018, 0.0014, 0.0013, 0.0008, 0.0010 };
                truePositivesA = truePositives1;
                truePositivesB = truePositives2;
            }

            int signalLength = bandPassFilteredSignal.Length;
            // count number of 1000 sample segments
            int blockLength = 1000;
            int blockCount = signalLength / blockLength;
            int[] indexOfMax = new int[blockCount];
            double[] maxInBlock = new double[blockCount];
            
            for (int i = 0; i < blockCount; i++)
            {
                double max = -2.0;
                int blockStart = blockLength * i;
                for (int s = 0; s < blockLength; s++)
                {
                    double absValue = Math.Abs(bandPassFilteredSignal[blockStart + s]);
                    if (absValue > max)
                    {
                        max = absValue;
                        maxInBlock[i] = max;
                        indexOfMax[i] = blockStart + s;
                    }
                }
            }

            // transfer max values to a list
            var indexList = new List<int>();
            for (int i = 1; i < blockCount-1; i++)
            {
                // only find the blocks that contain a max value that is > neighbouring blocks
                if ((maxInBlock[i] > maxInBlock[i - 1]) && (maxInBlock[i] > maxInBlock[i + 1]))
                {
                    indexList.Add(indexOfMax[i]);
                }

                //ALTERNATIVELY
                // look at max in each block
                //indexList.Add(indexOfMax[i]);
            }

            // now process neighbourhood of each max
            int windowWidth = 2048;
            int binCount = windowWidth / 2;
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(windowWidth, wf);
            int maxHz = nyquist / 11;
            double hzPerBin = nyquist / (double)binCount;

            // init list of events
            List<AcousticEvent> events = new List<AcousticEvent>();
            double[] scores = new double[signalLength]; // init of score array
            truePositivesA = NormalDist.Convert2ZScores(truePositivesA);
            truePositivesB = NormalDist.Convert2ZScores(truePositivesB);

            int id = 0;
            foreach (int location in indexList)
            {
                //System.LoggedConsole.WriteLine("Location " + location + ", id=" + id);

                int start = location - binCount;
                if (start < 0) continue;
                int end = location + binCount;
                if (end >= signalLength) continue;

                double[] subsampleWav = DataTools.Subarray(bandPassFilteredSignal, start, windowWidth);

                var spectrum = fft.Invoke(subsampleWav);
                int requiredBinCount = spectrum.Length / 11; // this assumes that nyquiust = 11,025
                var subBandSpectrum = DataTools.Subarray(spectrum, 1, requiredBinCount); // ignore DC in bin zero.
                // convert to power
                subBandSpectrum = DataTools.SquareValues(subBandSpectrum);
                subBandSpectrum = DataTools.filterMovingAverageOdd(subBandSpectrum, 3);
                subBandSpectrum = DataTools.normalise(subBandSpectrum);


                // now do some tests on spectrum to determine if it is a candidate grunt
                bool eventFound = false;

                //TEST ONE
                /*
                double totalAreaUnderSpectrum = subBandSpectrum.Sum();
                double areaUnderLowest24bins = 0.0;
                for (int i = 0; i < 24; i++)
                {
                    areaUnderLowest24bins += subBandSpectrum[i];
                }
                double areaUnderHighBins = totalAreaUnderSpectrum - areaUnderLowest24bins;
                double areaUnderBins4to7 = 0.0;
                for (int i = 4; i < 7; i++)
                {
                    areaUnderBins4to7 += subBandSpectrum[i];
                }
                double ratio1 = areaUnderBins4to7 / areaUnderLowest24bins;

                double areaUnderBins38to72 = 0.0;
                for (int i = 38; i < 44; i++)
                {
                    areaUnderBins38to72 += subBandSpectrum[i];
                }
                for (int i = 52; i < 57; i++)
                {
                    areaUnderBins38to72 += subBandSpectrum[i];
                }
                for (int i = 64; i < 72; i++)
                {
                    areaUnderBins38to72 += subBandSpectrum[i];
                }
                double ratio2 = areaUnderBins38to72 / areaUnderHighBins;
                double score = (ratio1 * 0.2) + (ratio2 * 0.8);
                double[] truePositives = { 0.0000, 0.0000, 0.0000, 0.0000, 0.0001, 0.0006, 0.0014, 0.0015, 0.0010, 0.0002, 0.0001, 0.0001, 0.0000, 0.0000, 0.0000, 0.0000, 0.0003, 0.0005, 0.0006, 0.0005, 0.0003, 0.0002, 0.0001, 0.0002, 0.0007, 0.0016, 0.0026, 0.0035, 0.0037, 0.0040, 0.0046, 0.0040, 0.0031, 0.0022, 0.0048, 0.0133, 0.0149, 0.0396, 0.1013, 0.1647, 0.2013, 0.2236, 0.2295, 0.1836, 0.1083, 0.0807, 0.0776, 0.0964, 0.1116, 0.0987, 0.1065, 0.1575, 0.3312, 0.4829, 0.5679, 0.5523, 0.4412, 0.2895, 0.2022, 0.2622, 0.2670, 0.2355, 0.1969, 0.2220, 0.6600, 0.9023, 1.0000, 0.8099, 0.8451, 0.8210, 0.5511, 0.1756, 0.0319, 0.0769, 0.0738, 0.2235, 0.3901, 0.4565, 0.4851, 0.3703, 0.3643, 0.2497, 0.2705, 0.3456, 0.3096, 0.1809, 0.0710, 0.0828, 0.0857, 0.0953, 0.1308, 0.1387, 0.0590 };

                if (score > 0.4)
                    eventFound = true;
                if ((areaUnderHighBins/3) < areaUnderLowest24bins)
                //if (ratio1 > ratio2)
                {
                    eventFound = false;
                }
                */

                // TEST TWO
                var zscores   = NormalDist.Convert2ZScores(subBandSpectrum);
                double score1 = AutoAndCrossCorrelation.CorrelationCoefficient(zscores, truePositivesA);
                double score2 = AutoAndCrossCorrelation.CorrelationCoefficient(zscores, truePositivesB);


                // PROCESS SCORES
                //if (score1 > scoreThreshold) eventFound = true;
                if ((score1 > scoreThreshold) || (score2 > scoreThreshold)) eventFound = true;
                double score = score1;
                if (score2 > score) score = score2;

                if (eventFound)
                {
                    for (int i = location-binCount; i < location + binCount; i++)
                    {
                        scores[location] = score;
                    }


                    var timespanStart = TimeSpan.FromSeconds((location - binCount) / (double)sr);
                    var timespanMid   = TimeSpan.FromSeconds(location / (double)sr);
                    var timespanEnd   = TimeSpan.FromSeconds((location + binCount) / (double)sr);

                    string title1 = String.Format("Bandpass filtered: tStart={0},  tMiddle={1},  tEnd={2}",
                                                     timespanStart.ToString(), timespanMid.ToString(), timespanEnd.ToString());
                    Image image4a = ImageTools.DrawWaveform(title1, subsampleWav, subsampleWav.Length, imageHeight, waveScalingFactor);

                    string title2 = String.Format("FFT 1->{0}Hz.,    hz/bin={1:f1},    score={2:f3}", maxHz, hzPerBin, score);
                    Image image4b = ImageTools.DrawGraph(title2, subBandSpectrum, subsampleWav.Length, imageHeight, 1);

                    var imageList = new List<Image>();
                    imageList.Add(image4a);
                    imageList.Add(image4b);
                    var image4 = ImageTools.CombineImagesVertically(imageList);
                    string path4 = String.Format(@"C:\SensorNetworks\Output\Freshwater\subsamples\subsample_{0}_{1}.png", id, location);
                    image4.Save(path4);

                    // have an event, store the data in the AcousticEvent class
                    double startTime = timespanMid.TotalSeconds;
                    double duration = 0.2; 
                    int minFreq = 50; 
                    int maxFreq = 1000;
                    var anEvent = new AcousticEvent(startTime, duration, minFreq, maxFreq);
                    anEvent.Name = "grunt";
                    anEvent.Score = score;
                    anEvent.Name = DataTools.WriteArrayAsCsvLine(subBandSpectrum, "f4");
                    events.Add(anEvent);

                }
                id++;
            } //foreach


            // make a spectrogram
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.STANDARD,
                NoiseReductionParameter = (double?)configuration[AnalysisKeys.NoiseBgThreshold] ?? 0.0
            };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            //// when the value is accessed, the indices are calculated
            //var indices = getSpectralIndexes.Value;

            //// check if the indices have been calculated - you shouldn't actually need this
            //if (getSpectralIndexes.IsValueCreated)
            //{
            //    // then indices have been calculated before
            //}

            var plot = new Plot(this.DisplayName, scores, eventThreshold);

            return new RecognizerResults()
            {
                Events = events,
                Hits = null,
                //ScoreTrack = null,
                Plots = plot.AsList(),
                Sonogram = sonogram
            };
        }
    }
}
