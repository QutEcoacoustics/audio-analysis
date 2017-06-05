// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IctalurusFurcatus.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This is a Blue Catfish recognizer (Ictalurus furcatus)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// This is a Blue Catfish recognizer (Ictalurus furcatus)
    /// </summary>
    class IctalurusFurcatus : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "IctalurusFurcatus";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        internal class CatFishCallData
        {
            public TimeSpan Timehms { get; set; } //(hh:mm:ss)
            public double TimeSeconds { get; set; }
            public int Sample { get; set; }
            public int Rating { get; set; }
            public int Waveform { get; set; }
        }



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
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            const double minAmplitudeThreshold = 0.1;
            const int percentile = 5;
            const double scoreThreshold = 0.3;
            const bool doFiltering = true;
            const int windowWidth = 1024;
            const int signalBuffer = windowWidth * 2;

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
            double[] bandPassFilteredSignal = null;

            if (doFiltering)
            {
                // high pass filter
                int windowLength = 71;
                double[] highPassFilteredSignal;
                DSP_IIRFilter.ApplyMovingAvHighPassFilter(samples, windowLength, out highPassFilteredSignal);

                //DSP_IIRFilter filter2 = new DSP_IIRFilter("Chebyshev_Highpass_400");
                //int order2 = filter2.order;
                //filter2.ApplyIIRFilter(samples, out highPassFilteredSignal);

                // Amplify 40dB and clip to +/-1.0;
                double factor = 100; // equiv to 20dB
                highPassFilteredSignal = DspFilters.AmplifyAndClip(highPassFilteredSignal, factor);

                //low pass filter
                string filterName = "Chebyshev_Lowpass_5000, scale*5";
                DSP_IIRFilter filter = new DSP_IIRFilter(filterName);
                int order = filter.order;
                //System.LoggedConsole.WriteLine("\nTest " + filterName + ", order=" + order);
                filter.ApplyIIRFilter(highPassFilteredSignal, out bandPassFilteredSignal);

            }
            else // do not filter because already filtered - using Chris's filtered recording
            {
                bandPassFilteredSignal = samples;
            }

            // calculate an amplitude threshold that is above Nth percentile of amplitudes in the subsample
            int[] histogramOfAmplitudes;
            double minAmplitude;
            double maxAmplitude;
            double binWidth;
            int window = 66;
            Histogram.GetHistogramOfWaveAmplitudes(bandPassFilteredSignal, window, out histogramOfAmplitudes, out minAmplitude, out maxAmplitude, out binWidth);
            int percentileBin = Histogram.GetPercentileBin(histogramOfAmplitudes, percentile);

            double amplitudeThreshold = (percentileBin + 1) * binWidth;
            if (amplitudeThreshold < minAmplitudeThreshold)
            {
                amplitudeThreshold = minAmplitudeThreshold;
            }



            bool doAnalysisOfKnownExamples = true;
            if (doAnalysisOfKnownExamples)
            {
                // go to fixed location to check
                //1:02.07, 1:07.67, 1:12.27, 1:12.42, 1:12.59, 1:12.8, 1.34.3, 1:35.3, 1:40.16, 1:50.0, 2:05.9, 2:06.62, 2:17.57, 2:21.0
                //2:26.33, 2:43.07, 2:43.15, 3:16.55, 3:35.09, 4:22.44, 4:29.9, 4:42.6, 4:51.48, 5:01.8, 5:21.15, 5:22.72, 5:32.37, 5.36.1,
                //5:42.82, 6:03.5, 6:19.93, 6:21.55, 6:42.0, 6:42.15, 6:46.44, 7:12.17, 7:42.65, 7:45.86, 7:46.18, 7:52.38, 7:59.11, 8:10.63,
                //8:14.4, 8:14.63, 8_15_240, 8_46_590, 8_56_590, 9_25_77, 9_28_94, 9_30_5, 9_43_9, 10_03_19, 10_24_26, 10_24_36, 10_38_8,
                //10_41_08, 10_50_9, 11_05_13, 11_08_63, 11_44_66, 11_50_36, 11_51_2, 12_04_93, 12_10_05, 12_20_78, 12_27_0, 12_38_5,
                //13_02_25, 13_08_18, 13_12_8, 13_25_24, 13_36_0, 13_50_4, 13_51_2, 13_57_87, 14_15_00, 15_09_74, 15_12_14, 15_25_79

                //double[] times = { 2.2, 26.589, 29.62 };
                //double[] times = { 2.2, 3.68, 10.83, 24.95, 26.589, 27.2, 29.62 };
                //double[] times = { 2.2, 3.68, 10.83, 24.95, 26.589, 27.2, 29.62, 31.39, 62.1, 67.67, 72.27, 72.42, 72.59, 72.8, 94.3, 95.3,
                //                   100.16, 110.0, 125.9, 126.62, 137.57, 141.0, 146.33, 163.07, 163.17, 196.55, 215.09, 262.44, 269.9, 282.6,
                //                   291.48, 301.85, 321.18, 322.72, 332.37, 336.1, 342.82, 363.5, 379.93, 381.55, 402.0, 402.15, 406.44, 432.17,
                //                   462.65, 465.86, 466.18, 472.38, 479.14, 490.63, 494.4, 494.63, 495.240, 526.590, 536.590, 565.82, 568.94,
                //                   570.5, 583.9, 603.19, 624.26, 624.36, 638.8, 641.08, 650.9, 65.13, 68.63, 704.66,
                //                   710.36, 711.2, 724.93, 730.05, 740.78, 747.05, 758.5, 782.25, 788.18, 792.8,
                //                   805.24, 816.03, 830.4, 831.2, 837.87, 855.02, 909.74, 912.14, 925.81  };

                var filePath = new FileInfo(@"C:\SensorNetworks\WavFiles\Freshwater\GruntSummaryRevisedAndEditedByMichael.csv");
                List<CatFishCallData> data = Csv.ReadFromCsv<CatFishCallData>(filePath, true).ToList();
                //var catFishCallDatas = data as IList<CatFishCallData> ?? data.ToList();
                int count = data.Count();


                var subSamplesDirectory = outputDirectory.CreateSubdirectory("testSubsamples_5000LPFilter");
                //for (int t = 0; t < times.Length; t++)
                foreach (var fishCall in data)
                {
                    //Image bmp1 = IctalurusFurcatus.AnalyseLocation(bandPassFilteredSignal, sr, times[t], windowWidth);


                    // use following line where using time in seconds
                    //int location = (int)Math.Round(times[t] * sr); //assume location points to start of grunt
                    //double[] subsample = DataTools.Subarray(bandPassFilteredSignal, location - signalBuffer, 2 * signalBuffer);

                    // use following line where using sample
                    int location1 = fishCall.Sample / 2; //assume Chris's sample location points to centre of grunt. Divide by 2 because original recording was 44100.
                    int location = (int)Math.Round(fishCall.TimeSeconds * sr); //assume location points to centre of grunt


                    double[] subsample = DataTools.Subarray(bandPassFilteredSignal, location - signalBuffer, 2 * signalBuffer);

                    // calculate an amplitude threshold that is above 95th percentile of amplitudes in the subsample
                    //int[] histogramOfAmplitudes;
                    //double minAmplitude;
                    //double maxAmplitude;
                    //double binWidth;
                    //int window = 70;
                    //int percentile = 90;
                    //Histogram.GetHistogramOfWaveAmplitudes(subsample, window, out histogramOfAmplitudes, out minAmplitude, out maxAmplitude, out binWidth);
                    //int percentileBin = Histogram.GetPercentileBin(histogramOfAmplitudes, percentile);

                    //double amplitudeThreshold = (percentileBin + 1) * binWidth;
                    //if (amplitudeThreshold < minAmplitudeThreshold) amplitudeThreshold = minAmplitudeThreshold;

                    double[] scores1 = AnalyseWaveformAtLocation(subsample, amplitudeThreshold, scoreThreshold);
                    string title1 = $"scores={fishCall.Timehms}";
                    Image bmp1 = GraphsAndCharts.DrawGraph(title1, scores1, subsample.Length, 300, 1);
                    //bmp1.Save(path1.FullName);

                    string title2 = $"tStart={fishCall.Timehms}";
                    Image bmp2 = GraphsAndCharts.DrawWaveform(title2, subsample, 1);
                    var path1 = subSamplesDirectory.CombineFile($"scoresForTestSubsample_{fishCall.TimeSeconds}secs.png");
                    //var path2 = subSamplesDirectory.CombineFile($@"testSubsample_{times[t]}secs.wav.png");
                    Image[] imageList = { bmp2, bmp1 };
                    Image bmp3 = ImageTools.CombineImagesVertically(imageList);
                    bmp3.Save(path1.FullName);

                    //write wave form to txt file for later work in XLS
                    //var path3 = subSamplesDirectory.CombineFile($@"testSubsample_{times[t]}secs.wav.csv");
                    //signalBuffer = 800;
                    //double[] subsample2 = DataTools.Subarray(bandPassFilteredSignal, location - signalBuffer, 3 * signalBuffer);
                    //FileTools.WriteArray2File(subsample2, path3.FullName);
                }

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
            int binCount = windowWidth / 2;
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(windowWidth, wf);
            int maxHz = 1000;
            double hzPerBin = nyquist / (double)binCount;
            int requiredBinCount = (int)Math.Round(maxHz / hzPerBin);

            // init list of events
            List<AcousticEvent> events = new List<AcousticEvent>();
            double[] scores = new double[signalLength]; // init of score array

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
                // convert to power
                spectrum = DataTools.SquareValues(spectrum);
                spectrum = DataTools.filterMovingAverageOdd(spectrum, 3);
                spectrum = DataTools.normalise(spectrum);
                var subBandSpectrum = DataTools.Subarray(spectrum, 1, requiredBinCount); // ignore DC in bin zero.


                // now do some tests on spectrum to determine if it is a candidate grunt
                bool eventFound = false;

                double[] scoreArray = CalculateScores(subBandSpectrum, windowWidth);
                double score = scoreArray[0];

                if (score > scoreThreshold)
                {
                    eventFound = true;
                }

                if (eventFound)
                {
                    for (int i = location-binCount; i < location + binCount; i++)
                    {
                        scores[location] = score;
                    }

                    var startTime = TimeSpan.FromSeconds((location - binCount) / (double)sr);
                    string startLabel = startTime.Minutes + "." + startTime.Seconds+ "." + startTime.Milliseconds;
                    Image image4 = GraphsAndCharts.DrawWaveAndFft(subsampleWav, sr, startTime, spectrum, maxHz*2, scoreArray);

                    var path4 = outputDirectory.CreateSubdirectory("subsamples").CombineFile($@"subsample_{location}_{startLabel}.png");
                    image4.Save(path4.FullName);

                    // have an event, store the data in the AcousticEvent class
                    double duration = 0.2;
                    int minFreq = 50;
                    int maxFreq = 1000;
                    var anEvent = new AcousticEvent(startTime.TotalSeconds, duration, minFreq, maxFreq);
                    anEvent.Name = "grunt";
                    //anEvent.Name = DataTools.WriteArrayAsCsvLine(subBandSpectrum, "f4");
                    anEvent.Score = score;
                    events.Add(anEvent);

                }
                id++;
            }


            // make a spectrogram
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = (double?)configuration[AnalysisKeys.NoiseBgThreshold] ?? 0.0,
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
                Sonogram = sonogram,
            };
        }


        public static Image AnalyseLocation(double[] signal, int sr, double startTimeInSeconds, int windowWidth)
        {
            int binCount = windowWidth / 2;

            int location = (int)Math.Round(startTimeInSeconds * sr); //assume location points to start of grunt

            if (location >= signal.Length)
            {
                LoggedConsole.WriteErrorLine("WARNING: Location is beyond end of signal.");
                return null;
            }
            int nyquist = sr / 2;
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(windowWidth, wf);
            int maxHz = 1000;  // max frequency to display in fft image
            double hzPerBin = nyquist / (double)binCount;
            int requiredBinCount = (int)Math.Round(maxHz / hzPerBin);

            double[] subsampleWav = DataTools.Subarray(signal, location, windowWidth);
            var spectrum = fft.Invoke(subsampleWav);
            // convert to power
            spectrum = DataTools.SquareValues(spectrum);
            spectrum = DataTools.filterMovingAverageOdd(spectrum, 3);
            spectrum = DataTools.normalise(spectrum);
            var subBandSpectrum = DataTools.Subarray(spectrum, 1, requiredBinCount); // ignore DC in bin zero.


            var startTime = TimeSpan.FromSeconds(startTimeInSeconds);
            double[] scoreArray = CalculateScores(subBandSpectrum, windowWidth);
            Image image4 = GraphsAndCharts.DrawWaveAndFft(subsampleWav, sr, startTime, spectrum, maxHz * 2, scoreArray);
            return image4;
        }


        public static double[] AnalyseWaveformAtLocation(double[] signal, double amplitudeThreshold, double scoreThreshold)
        {
            double[] waveTemplate = {-0.000600653,-0.000451427,-0.000193289,-1.91083E-05,0.000133366,0.000256465,0.000387591,0.000533758,0.000645976,0.000670944,
                 0.000622045, 0.000569337, 0.000553455, 0.000535552,0.000448935,0.000286928,0.000120322,2.26704E-05,-1.67728E-05,-9.42369E-05,
                -0.000289969,-0.000565902,-0.00079103, -0.000883849,-0.000898074,-0.000932191,-0.000985956,-0.00097387,-0.00093,-0.00088,
                -0.00088,    -0.00087,   -0.000883903, -0.000831651,-0.000759236,-0.000668421,-0.00052348,-0.000361632,-0.00028245,-0.000292255,
                -0.00025,    -8.71854E-05, 0.00011,     0.00014,     0.00010031,  0.00013,    0.00016,     0.000240405, 0.000332283,0.000357989,
                 0.000312231, 0.000222307, 0.000138079, 9.49253E-05, 6.74344E-05, -7.81347E-07, -0.000103014, -0.00014973,
            };
            int templateLength = waveTemplate.Length;

            double[] normalTemplate = DataTools.normalise2UnitLength(waveTemplate);

            double[] scores = new double[signal.Length];

            for (int i = 2; i < signal.Length- templateLength; i++)
            {
                // look for a local minimum
                if ((signal[i] > signal[i - 1]) || (signal[i] > signal[i - 2]) || (signal[i] > signal[i + 1]) || (signal[i] > signal[i + 2]))
                {
                    continue;
                }
                double[] subsampleWav = DataTools.Subarray(signal, i, templateLength);
                double min, max;
                DataTools.MinMax(subsampleWav, out min, out max);
                if ((max - min) < amplitudeThreshold) continue;

                double[] normalwav = DataTools.normalise2UnitLength(subsampleWav);

                // calculate cosine angle as similarity score
                double score = DataTools.DotProduct(normalTemplate, normalwav);
                if (score > scoreThreshold)
                {
                    for (int j = 0; j < templateLength; j++)
                    {
                        if((score > scores[i + j])) scores[i + j] = score;
                    }
                }
            }
            //scores = DataTools.NormaliseMatrixValues(scores);
            //scores = DataTools.filterMovingAverageOdd(scores, 3);
            return scores;
        }


        public static double[] CalculateScores(double[] subBandSpectrum, int windowWidth)
        {
            double[] scores = { 0, 0, 0 };

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

            // TEST TWO (A)
            // these are used for scoring
            //double[] truePositives1 = { 0.0000, 0.0000, 0.0000, 0.0000, 0.0001, 0.0006, 0.0014, 0.0015, 0.0010, 0.0002, 0.0001, 0.0001, 0.0000, 0.0000, 0.0000, 0.0000, 0.0003, 0.0005, 0.0006, 0.0005, 0.0003, 0.0002, 0.0001, 0.0002, 0.0007, 0.0016, 0.0026, 0.0035, 0.0037, 0.0040, 0.0046, 0.0040, 0.0031, 0.0022, 0.0048, 0.0133, 0.0149, 0.0396, 0.1013, 0.1647, 0.2013, 0.2236, 0.2295, 0.1836, 0.1083, 0.0807, 0.0776, 0.0964, 0.1116, 0.0987, 0.1065, 0.1575, 0.3312, 0.4829, 0.5679, 0.5523, 0.4412, 0.2895, 0.2022, 0.2622, 0.2670, 0.2355, 0.1969, 0.2220, 0.6600, 0.9023, 1.0000, 0.8099, 0.8451, 0.8210, 0.5511, 0.1756, 0.0319, 0.0769, 0.0738, 0.2235, 0.3901, 0.4565, 0.4851, 0.3703, 0.3643, 0.2497, 0.2705, 0.3456, 0.3096, 0.1809, 0.0710, 0.0828, 0.0857, 0.0953, 0.1308, 0.1387, 0.0590 };
            //double[] truePositives2 = { 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0001, 0.0001, 0.0001, 0.0001, 0.0000, 0.0000, 0.0001, 0.0001, 0.0003, 0.0004, 0.0004, 0.0002, 0.0001, 0.0001, 0.0003, 0.0003, 0.0006, 0.0007, 0.0020, 0.0127, 0.0256, 0.0426, 0.0512, 0.0560, 0.0414, 0.0237, 0.0133, 0.0107, 0.0091, 0.0077, 0.0085, 0.0165, 0.0144, 0.0308, 0.0416, 0.0454, 0.0341, 0.0191, 0.0128, 0.0058, 0.0026, 0.0081, 0.0139, 0.0313, 0.0404, 0.0493, 0.0610, 0.1951, 0.4083, 0.5616, 0.5711, 0.5096, 0.4020, 0.2917, 0.1579, 0.1421, 0.1461, 0.1406, 0.2098, 0.1676, 0.2758, 0.2875, 0.6513, 0.9374, 1.0000, 0.7576, 0.4130, 0.2622, 0.1495, 0.0973, 0.0623, 0.0425, 0.0205, 0.0034, 0.0065, 0.0054, 0.0089, 0.0138, 0.0208, 0.0204, 0.0168, 0.0136, 0.0149, 0.0155, 0.0106, 0.0086, 0.0099, 0.0187 };
            //double[] truePositivesA = NormalDist.Convert2ZScores(truePositivesA);
            //double[] truePositivesB = NormalDist.Convert2ZScores(truePositivesB);



            // TEST TWO (B)
            // Use these spectra when using my filtering (i.e. not Chris's prefiltered)
            // these spectra are used for scoring when the window size is 2048
            //double[] truePositives1 = { 0.0014, 0.0012, 0.0009, 0.0003, 0.0001, 0.0005, 0.0008, 0.0029, 0.0057, 0.0070, 0.0069, 0.0063, 0.0053, 0.0032, 0.0013, 0.0011, 0.0011, 0.0007, 0.0000, 0.0006, 0.0010, 0.0013, 0.0008, 0.0009, 0.0022, 0.0046, 0.0069, 0.0082, 0.0070, 0.0065, 0.0082, 0.0078, 0.0052, 0.0021, 0.0132, 0.0357, 0.0420, 0.0996, 0.2724, 0.4557, 0.5739, 0.6366, 0.6155, 0.4598, 0.2334, 0.1468, 0.1410, 0.1759, 0.2157, 0.1988, 0.2131, 0.3072, 0.6161, 0.8864, 1.0000, 0.9290, 0.6983, 0.4208, 0.2690, 0.3190, 0.3109, 0.2605, 0.1896, 0.2118, 0.5961, 0.8298, 0.9290, 0.7363, 0.6605, 0.5840, 0.3576, 0.1019, 0.0162, 0.0400, 0.0405, 0.1106, 0.1803, 0.2083, 0.2058, 0.1475, 0.1387, 0.0870, 0.0804, 0.0975, 0.0848, 0.0490, 0.0193, 0.0217, 0.0210, 0.0214, 0.0253, 0.0254, 0.0072 };
            //double[] truePositives2 = { 0.0090, 0.0106, 0.0138, 0.0134, 0.0088, 0.0026, 0.0002, 0.0002, 0.0003, 0.0000, 0.0001, 0.0006, 0.0013, 0.0019, 0.0020, 0.0015, 0.0008, 0.0004, 0.0002, 0.0015, 0.0022, 0.0073, 0.0195, 0.0628, 0.2203, 0.4031, 0.5635, 0.5445, 0.4828, 0.2869, 0.1498, 0.0588, 0.0500, 0.0542, 0.0641, 0.1188, 0.1833, 0.1841, 0.2684, 0.3062, 0.2831, 0.1643, 0.0606, 0.0336, 0.0136, 0.0056, 0.0187, 0.0301, 0.0700, 0.1103, 0.1559, 0.2449, 0.5303, 0.8544, 1.0000, 0.8361, 0.6702, 0.4839, 0.3463, 0.1525, 0.1049, 0.1201, 0.1242, 0.2056, 0.1653, 0.2685, 0.2947, 0.5729, 0.7024, 0.6916, 0.4765, 0.2488, 0.1283, 0.0543, 0.0326, 0.0236, 0.0187, 0.0108, 0.0021, 0.0028, 0.0019, 0.0024, 0.0041, 0.0063, 0.0066, 0.0055, 0.0036, 0.0025, 0.0018, 0.0014, 0.0013, 0.0008, 0.0010 };
            // these spectra are used for scoring when the window size is 1024
            double[] truePositives1 = { 0.0007, 0.0004, 0.0000, 0.0025, 0.0059, 0.0069, 0.0044, 0.0012, 0.0001, 0.0006, 0.0013, 0.0032, 0.0063, 0.0067, 0.0070, 0.0033, 0.0086, 0.0128, 0.1546, 0.4550, 0.6197, 0.4904, 0.2075, 0.0714, 0.1171, 0.4654, 0.8634, 1.0000, 0.7099, 0.2960, 0.1335, 0.3526, 0.6966, 0.9215, 0.6628, 0.3047, 0.0543, 0.0602, 0.0931, 0.1364, 0.1314, 0.1047, 0.0605, 0.0204, 0.0128, 0.0114 };
            double[] truePositives2 = { 0.0126, 0.0087, 0.0043, 0.0002, 0.0000, 0.0010, 0.0018, 0.0016, 0.0005, 0.0002, 0.0050, 0.1262, 0.4054, 0.5111, 0.3937, 0.1196, 0.0156, 0.0136, 0.0840, 0.1598, 0.1691, 0.0967, 0.0171, 0.0152, 0.0234, 0.3648, 0.8243, 1.0000, 0.6727, 0.2155, 0.0336, 0.0240, 0.2661, 0.6240, 0.7523, 0.5098, 0.1493, 0.0149, 0.0046, 0.0020, 0.0037, 0.0061, 0.0061, 0.0036, 0.0010, 0.0008 };

            var zscores = NormalDist.Convert2ZScores(subBandSpectrum);
            double correlationScore = 0.0;
            double score1 = AutoAndCrossCorrelation.CorrelationCoefficient(zscores, truePositives1);
            double score2 = AutoAndCrossCorrelation.CorrelationCoefficient(zscores, truePositives2);
            correlationScore = score1;
            if (score2 > correlationScore) correlationScore = score2;

            // TEST THREE: sharpness and height of peaks
            // score the four heighest peaks
            double peaksScore = 0;
            double[] spectrumCopy = new double[subBandSpectrum.Length];
            for (int i = 0; i < subBandSpectrum.Length; i++)
            {
                spectrumCopy[i] = subBandSpectrum[i];
            }

            // set spectrum bounds
            int lowerBound = subBandSpectrum.Length / 4;
            int upperBound = subBandSpectrum.Length * 7 / 8;
            for (int p = 0; p < 4; p++)
            {
                int peakLocation = DataTools.GetMaxIndex(spectrumCopy);
                if (peakLocation < lowerBound) continue; // peak location cannot be too low
                if (peakLocation > upperBound) continue; // peak location cannot be too high

                double peakHeight = spectrumCopy[peakLocation];
                int nh = 3;
                if (windowWidth == 2048) nh = 6;
                double peakSides = (subBandSpectrum[peakLocation - nh] + subBandSpectrum[peakLocation + nh]) / (double)2;
                peaksScore += (peakHeight - peakSides);
                //now zero peak and peak neighbourhood
                if (windowWidth == 2048) nh = 9;
                for (int n = 0; n < nh; n++)
                {
                    spectrumCopy[peakLocation + n] = 0;
                    spectrumCopy[peakLocation - n] = 0;
                }
            } // for 4 peaks
            // take average of four peaks
            peaksScore /= 4;

            // TEST FOUR: peak position ratios
            //
            //int[] peakLocationCentres = { 3, 10, 37, 44, 54, 67 };
            int[] peakLocationCentres = { 2, 5, 19, 22, 27, 33 };

            int nh2 = 6;
            if (windowWidth == 1024)
            {
                nh2 = 3;
            }
            int[] actualPeakLocations = new int[6];
            double[] relativePeakHeights = new double[6];
            for (int p = 0; p < 6; p++)
            {
                double max = -double.MaxValue;
                int maxId = peakLocationCentres[p];
                for (int id = peakLocationCentres[p] - 4; id < peakLocationCentres[p] + 4; id++)
                {
                    if (id < 0) id = 0;
                    if (subBandSpectrum[id] > max)
                    {
                        max = subBandSpectrum[id];
                        maxId = id;
                    }
                }
                actualPeakLocations[p] = maxId;
                int lowerPosition = maxId - nh2;
                if (lowerPosition < 0) lowerPosition = 0;
                relativePeakHeights[p] = subBandSpectrum[maxId] - subBandSpectrum[lowerPosition] - subBandSpectrum[maxId+nh2];
            }
            double[] targetHeights = { 0.1, 0.1, 0.5, 0.5, 1.0, 0.6 };
            var zscores1 = NormalDist.Convert2ZScores(relativePeakHeights);
            var zscores2 = NormalDist.Convert2ZScores(targetHeights);
            double relativePeakScore = AutoAndCrossCorrelation.CorrelationCoefficient(zscores1, zscores2);

            //###########################################################################################
            // PROCESS SCORES
            //if (score1 > scoreThreshold) eventFound = true;
            //if ((score1 > scoreThreshold) || (score2 > scoreThreshold)) eventFound = true;
            //double score = (correlationScore * 0.3) + (peaksScore * 0.7);
            double score = (relativePeakScore * 0.4) + (peaksScore * 0.6);
            scores[0] = score;
            scores[1] = relativePeakScore;
            scores[2] = peaksScore;
            return scores;
        }

    }
}
