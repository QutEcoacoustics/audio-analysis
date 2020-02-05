// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelIntegrity.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Purpose of this class is to determine whether one of the signal channels in a stereo recording
//   has microphone problems due to rain or whatever.
//   It contains two main methods:
//   (1) a method to calculate the difference between the amplitdue spectrograms of each channel
//   (2) a method to calculate the zero crossing rate in each channel.
//   Yvonne found method (1) in the R.SEEWAVE library called SIMSPEC.
//   Yvonne also found that a threshold of 0.2 separates good from bad recordings but it does
//   not determine which channel is bad.
//   Michael found that the zero crossing rate is higher for dud channels - at least in the few recordings provided by Yvonne.
//   These two methods are therefore called in series and info found is used to determine channel integrity.
//   NOTE FROM ANTHONY (May 2016):
//   there's two ways you could use this
//   a) generate a report for a file
//   b) while running another analysis, automatically switch channels
//   Either way, the only way this realistically works for a large number of files is by blocking them into one minute chunks
//   as always.Thus, you need tell me which mode you want and either way, i expect to see some API like this:
//   DudChannelDetector.Analyze(WavReader wavReader)
//   and
//   DudChannelDetector.Aggregate(xxxx[] minutes)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.IO;
    using Acoustics.Tools;
    using Acoustics.Tools.Wav;
    using AnalysisBase.ResultBases;
    using DSP;
    using TowseyLibrary;
    using WavTools;

    public static class ChannelIntegrity
    {
        public class Arguments
        {
            public FileInfo Source { get; set; }

            public DirectoryInfo OpDir { get; set; }

            public int SamplingRate { get; set; }

            public TimeSpan? StartOffset { get; set; }

            public TimeSpan? EndOffset { get; set; }

            public string OutputMediaType { get; set; }
        }

        public static void Execute(Arguments arguments)
        {
            FileInfo ipFile = arguments.Source;

            SeparateChannels(arguments, ipFile, out var channelL, out var channelR, out var epsilon);

            SimilarityIndex(channelL, channelR, epsilon, arguments.SamplingRate, out var similarityIndex, out var similarityIndexDecibel,
                                                                                 out var avDecibelBias, out var medianDecibelBias,
                                                                                 out var lowDecibelBias, out var midDecibelBias, out var hiDecibelBias);

            //double similarityIndex = SimilarityIndex2(channelL, channelR, epsilon, arguments.SamplingRate);

            ZeroCrossingIndex(channelL, channelR, out var zeroCrossingFractionL, out var zeroCrossingFractionR);

            Console.WriteLine($"Zero crossings: L={zeroCrossingFractionL:f3}   R={zeroCrossingFractionR:f3}");
            Console.WriteLine(
                $"Similarity Index: SimIndex={similarityIndex:f3}   SimIndexdB={similarityIndexDecibel:f3}   avBiasdB={avDecibelBias:f3}   medianBiasdB={medianDecibelBias:f3}");
            Console.WriteLine(
                $"dB Bias Index: low band={lowDecibelBias:f3}   mid band={midDecibelBias:f3}   high band={hiDecibelBias:f3}");
        }

        public static void SeparateChannels(
            Arguments args,
            FileInfo ipFile,
            out double[] samplesL,
            out double[] samplesR,
            out double epsilon)
        {
            //you'd then use wavreader on the resulting preparedFile
            //the channel select functionality does not currently exist in AnalyzeLongRecording.   I need to add it.
            var request = new AudioUtilityRequest
                {
                    OffsetStart = args.StartOffset,
                    OffsetEnd = args.EndOffset,
                    TargetSampleRate = args.SamplingRate,
                    Channels = new[] { 1, 2 },
                    MixDownToMono = false,
                };
            var audioFile = AudioFilePreparer.PrepareFile(args.OpDir, ipFile, args.OutputMediaType, request, args.OpDir).TargetInfo.SourceFile;

            var wavReader = new WavReader(audioFile);

            var recording = new AudioRecording(wavReader);
            samplesL = recording.WavReader.GetChannel(0);
            samplesR = recording.WavReader.GetChannel(1);
            epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
        }

        public static void SimilarityIndex(double[] channelL, double[] channelR, double epsilon, int sampleRate,
                                             out double similarityIndex, out double decibelIndex,
                                             out double avDecibelBias, out double medianDecibelBias,
                                             out double lowFreqDbBias, out double midFreqDbBias, out double hiFreqDbBias)
        {
            //var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFFTs(subsegmentRecording, frameSize, frameStep);
            int frameSize = 512;
            int frameStep = 512;
            frameSize *= 16; // take longer window to get low freq
            frameStep *= 16;

            var dspOutputL = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(channelL, sampleRate, epsilon, frameSize, frameStep);
            var avSpectrumL = MatrixTools.GetColumnAverages(dspOutputL.AmplitudeSpectrogram);

            //var medianSpectrumL = MatrixTools.GetColumnMedians(dspOutputL.amplitudeSpectrogram);

            var dspOutputR = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(channelR, sampleRate, epsilon, frameSize, frameStep);
            var avSpectrumR = MatrixTools.GetColumnAverages(dspOutputR.AmplitudeSpectrogram);

            //var medianSpectrumR = MatrixTools.GetColumnMedians(dspOutputR.amplitudeSpectrogram);

            similarityIndex = 0.0;
            decibelIndex = 0.0;
            for (int i = 0; i < avSpectrumR.Length; i++)
            {
                double min = Math.Min(avSpectrumL[i], avSpectrumR[i]);
                double max = Math.Max(avSpectrumL[i], avSpectrumR[i]);
                if (max <= 0.000001)
                {
                    max = 0.000001;  // to prevent division by zero.
                }

                // index = min / max;
                double index = min * min / (max * max);
                similarityIndex += index;

                double dBmin = 20 * Math.Log10(min);
                double dBmax = 20 * Math.Log10(max);
                decibelIndex += dBmax - dBmin;
            }

            similarityIndex /= avSpectrumR.Length;
            decibelIndex /= avSpectrumR.Length;

            double medianLeft = Statistics.GetMedian(avSpectrumL);
            double medianRight = Statistics.GetMedian(avSpectrumR);
            medianDecibelBias = medianLeft - medianRight;

            // init values
            avDecibelBias = 0.0;
            lowFreqDbBias = 0.0;

            // calculate the freq band bounds for 2kHz and 7khz.
            int lowBound = frameSize * 2000 / sampleRate;
            int midBound = frameSize * 7000 / sampleRate;
            for (int i = 0; i < lowBound; i++)
            {
                double dbLeft = 20 * Math.Log10(avSpectrumL[i]);
                double dbRight = 20 * Math.Log10(avSpectrumR[i]);
                avDecibelBias += dbLeft - dbRight;
                lowFreqDbBias += dbLeft - dbRight;
            }

            midFreqDbBias = 0.0;
            for (int i = lowBound; i < midBound; i++)
            {
                double dbLeft = 20 * Math.Log10(avSpectrumL[i]);
                double dbRight = 20 * Math.Log10(avSpectrumR[i]);
                avDecibelBias += dbLeft - dbRight;
                midFreqDbBias += dbLeft - dbRight;
            }

            hiFreqDbBias = 0.0;
            for (int i = midBound; i < avSpectrumR.Length; i++)
            {
                double dbLeft = 20 * Math.Log10(avSpectrumL[i]);
                double dbRight = 20 * Math.Log10(avSpectrumR[i]);
                avDecibelBias += dbLeft - dbRight;
                hiFreqDbBias += dbLeft - dbRight;
            }

            avDecibelBias /= avSpectrumR.Length;
            lowFreqDbBias /= lowBound;
            midFreqDbBias /= midBound - lowBound;
            hiFreqDbBias /= avSpectrumR.Length - midBound;
        }

        public static double SimilarityIndex2(double[] channelL, double[] channelR, double epsilon, int sampleRate)
        {
            //var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFFTs(subsegmentRecording, frameSize, frameStep);
            int frameSize = 512;
            int frameStep = 512;

            var dspOutputL = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(channelL, sampleRate, epsilon, frameSize, frameStep);
            var spgrmL = dspOutputL.AmplitudeSpectrogram;

            var dspOutputR = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(channelR, sampleRate, epsilon, frameSize, frameStep);
            var spgrmR = dspOutputR.AmplitudeSpectrogram;

            double similarityIndex = 0;

            // get spgrm dimensions - assume both spgrms have same dimensions
            int rowCount = spgrmL.GetLength(0);
            int colCount = spgrmL.GetLength(1);
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    double min = Math.Min(spgrmL[r, c], spgrmR[r, c]);
                    double max = Math.Max(spgrmL[r, c], spgrmR[r, c]);
                    double index = 0;
                    if (max <= 0.000001)
                    {
                        index = min / 0.000001;
                    } // to prevent division by zero.
                    else

                    //{ index = min / max; }
                    {
                        index = min * min / (max * max);
                    }

                    similarityIndex += index; // / Math.Max(L, R);
                }
            }

            return similarityIndex / (rowCount * colCount);
        }

        public static void ZeroCrossingIndex(double[] samplesL, double[] samplesR, out double zeroCrossingFractionL, out double zeroCrossingFractionR)
        {
            zeroCrossingFractionL = DataTools.ZeroCrossings(samplesL) / (double)samplesL.Length;
            zeroCrossingFractionR = DataTools.ZeroCrossings(samplesR) / (double)samplesR.Length;
        }

        /// <summary>
        /// Tried this but first attempt did not seem to provide discriminative information
        /// </summary>
        /// <param name="samplesL"></param>
        /// <param name="samplesR"></param>
        public static void ChannelMeanAndSD(double[] samplesL, double[] samplesR)
        {
            NormalDist.AverageAndSD(samplesL, out var mean1, out var stde1);
            NormalDist.AverageAndSD(samplesR, out var mean2, out var stde2);
            double t = Statistics.tStatistic(mean1, stde1, samplesL.Length, mean2, stde2, samplesR.Length);
            string stats = Statistics.tStatisticAndSignificance(mean1, stde1, samplesL.Length, mean2, stde2, samplesR.Length);
            Console.WriteLine(stats);
        }
    }

    public class ChannelIntegrityIndices : SummaryIndexBase
    {
        public double ZeroCrossingFractionLeft { get; set; }

        public double ZeroCrossingFractionRight { get; set; }

        public double ChannelSimilarity { get; set; }

        public double ChannelDiffDecibels { get; set; }

        public double AverageDecibelBias { get; set; }

        public double MedianDecibelBias { get; set; }

        public double LowFreqDecibelBias { get; set; }

        public double MidFreqDecibelBias { get; set; }

        public double HighFreqDecibelBias { get; set; }
    }
}
