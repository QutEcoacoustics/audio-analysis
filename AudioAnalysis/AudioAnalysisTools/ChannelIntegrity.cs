// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelIntegrity.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Tools;
    using Acoustics.Tools.Wav;

    using AnalysisBase.ResultBases;

    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;

    using TowseyLibrary;


    public static class ChannelIntegrity
    {

        public static Arguments Dev()
        {
            //FileInfo audioFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad.wav");
            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad.2hours.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad.2hours.RIGHT.wav");

            //FileInfo audioFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad_1MinExtractAt4h06min.wav");
            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad_1MinExtractAt4h06min.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20151029_064553_Gympie_bad_1MinExtractAt4h06min.RIGHT.wav");

            FileInfo audioFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150725_064552_Gympie_bad_1MinExtractAt5h16m.wav");
            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150725_064552_Gympie_bad_1MinExtractAt5h16m.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150725_064552_Gympie_bad_1MinExtractAt5h16m.RIGHT.wav");

            //FileInfo ipFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150819_133146_gym_good_1MinExtractAt5h40min.wav");
            //FileInfo audioFileL = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150819_133146_gym_good_1MinExtractAt5h40min.LEFT.wav");
            //FileInfo audioFileR = new FileInfo(@"C:\SensorNetworks\WavFiles\Gympie\20150819_133146_gym_good_1MinExtractAt5h40min.RIGHT.wav");
            var opDirectory = new DirectoryInfo(@"C:\SensorNetworks\output\ChannelIntegrity");

            int targetSampleRateHz = 22050;
            string outputMediaType = "Audio/wav";

            var startOffset = TimeSpan.Zero;
            var endOffset = TimeSpan.FromSeconds(60);


            var arguments = new Arguments
            {
                Source = audioFile,
                OpDir = opDirectory,
                OutputMediaType = outputMediaType,
                SamplingRate = targetSampleRateHz,
                StartOffset = startOffset,
                EndOffset = endOffset,
            };
            return arguments;
        }

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
            if (arguments == null)
            {
                arguments = Dev();
            }
            FileInfo ipFile = arguments.Source;

            double[] channelL;
            double[] channelR;
            double epsilon;
            SeparateChannels(arguments, ipFile, out channelL, out channelR, out epsilon);

            double similarityIndex;
            double similarityIndexDecibel;
            double decibelBiasIndex;
            SimilarityIndex(channelL, channelR, epsilon, arguments.SamplingRate, out similarityIndex, out similarityIndexDecibel, out decibelBiasIndex);
            //double similarityIndex = SimilarityIndex2(channelL, channelR, epsilon, arguments.SamplingRate);


            double zeroCrossingFractionL;
            double zeroCrossingFractionR;
            ZeroCrossingIndex(channelL, channelR, out zeroCrossingFractionL, out zeroCrossingFractionR);
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
                    MixDownToMono = false
                };
            var audioFile = AudioFilePreparer.PrepareFile(args.OpDir, ipFile, args.OutputMediaType, request, args.OpDir);

            var wavReader = new WavReader(audioFile);

            var recording = new AudioRecording(wavReader);
            samplesL = recording.WavReader.GetChannel(0);
            samplesR = recording.WavReader.GetChannel(1);
            epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
        }

        public static void SimilarityIndex(double[] channelL, double[] channelR, double epsilon, int sampleRate, 
                                             out double similarityIndex, out double decibelIndex, out double decibelBiasIndex)
        {
            //var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFFTs(subsegmentRecording, frameSize, frameStep);
            int frameSize = 512;
            int frameStep = 512;

            var dspOutputL = DSP_Frames.ExtractEnvelopeAndFFTs(channelL, sampleRate, epsilon, frameSize, frameStep);
            var avSpectrumL = MatrixTools.GetColumnsAverages(dspOutputL.amplitudeSpectrogram);

            var dspOutputR = DSP_Frames.ExtractEnvelopeAndFFTs(channelR, sampleRate, epsilon, frameSize, frameStep);
            var avSpectrumR = MatrixTools.GetColumnsAverages(dspOutputR.amplitudeSpectrogram);

            similarityIndex = 0.0;
            decibelIndex    = 0.0;
            decibelBiasIndex = 0.0;
            for (int i = 0; i < avSpectrumR.Length; i++)
            {
                double min = Math.Min(avSpectrumL[i], avSpectrumR[i]);
                double max = Math.Max(avSpectrumL[i], avSpectrumR[i]);
                double index = 0;
                if (max <= 0.000001) max = 0.000001;  // to prevent division by zero.

                // index = min / max; 
                index = (min*min) / (max*max); 
                similarityIndex += index; 

                double dBmin = 20 * Math.Log10(min);
                double dBmax = 20 * Math.Log10(max);
                decibelIndex += (dBmax - dBmin);

                double dbLeft = 20 * Math.Log10(avSpectrumL[i]);
                double dbRight = 20 * Math.Log10(avSpectrumR[i]);
                decibelBiasIndex += (dbLeft - dbRight);
            }

            similarityIndex  /= (double)(avSpectrumR.Length);
            decibelIndex     /= (double)(avSpectrumR.Length);
            decibelBiasIndex /= (double)(avSpectrumR.Length);

            //return similarityIndex / (double)(avSpectrumR.Length);
        }

        public static double SimilarityIndex2(double[] channelL, double[] channelR, double epsilon, int sampleRate)
        {
            //var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFFTs(subsegmentRecording, frameSize, frameStep);
            int frameSize = 512;
            int frameStep = 512;

            var dspOutputL = DSP_Frames.ExtractEnvelopeAndFFTs(channelL, sampleRate, epsilon, frameSize, frameStep);
            var spgrmL = dspOutputL.amplitudeSpectrogram;

            var dspOutputR = DSP_Frames.ExtractEnvelopeAndFFTs(channelR, sampleRate, epsilon, frameSize, frameStep);
            var spgrmR = dspOutputR.amplitudeSpectrogram;

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
                    { index = min / 0.000001; } // to prevent division by zero.
                    else
                    //{ index = min / max; }
                    { index = (min * min) / (max * max); }
                    similarityIndex += index; // / Math.Max(L, R);
                }
            }

            return similarityIndex / (double)(rowCount * colCount);
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
            double mean1;
            double stde1;
            NormalDist.AverageAndSD(samplesL, out mean1, out stde1);
            double mean2;
            double stde2;
            NormalDist.AverageAndSD(samplesR, out mean2, out stde2);
            double t = Statistics.tStatistic(mean1, stde1, samplesL.Length, mean2, stde2, samplesR.Length);
            string stats = Statistics.tStatisticAndSignificance(mean1, stde1, samplesL.Length, mean2, stde2, samplesR.Length);
            Console.WriteLine(stats);
        }

    }

    public class ChannelIntegrityIndexes : SummaryIndexBase
    {
        public double ZeroCrossingFractionLeft { get; set; }

        public double ZeroCrossingFractionRight { get; set; }

        public double ChannelSimilarity { get; set; }

        public double ChannelDiffDecibels { get; set; }

        public double ChannelBiasDecibels { get; set; }
    }
}
