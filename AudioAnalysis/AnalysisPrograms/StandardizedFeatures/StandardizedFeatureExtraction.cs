// <copyright file="StandardizedFeatureExtraction.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.StandardizedFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Security.Policy;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net.Repository.Hierarchy;
    using PowerArgs;
    using TowseyLibrary;

    public class StandardizedFeatureExtraction : AbstractStrongAnalyser
    {
        private const string Sentence = "Hello World";

        // Input is a class object of Arguments, class is made below
        public static void Execute(Arguments arguments)
        {
            LoggedConsole.WriteLine("The sentence was printed {0} times", arguments.Multiplication);
            for (int i = 0; i < arguments.Multiplication; i++)
            {
                LoggedConsole.WriteLine(Sentence);
            }
        }

        // Creates a class that constructs arguments, the description is shown in help StandardizedFeatureExtraction
        // The variable Multiplication can later be used
        public class Arguments
        {
            [ArgDescription("How many times hello world")]
            public int Multiplication { get; set; }
        }

        // Implemented from AbstractStrongAnalyser
        public override string DisplayName
        {
            get { return "Standardized Feature Extraction"; }
        }

        public override string Identifier
        {
            get { return "Ecosounds.StandardizedFeatures"; }
        }

        public virtual string Description
        {
            get { return "Performs a standardized feature extraction."; }
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;
            //int sampleRate = recording.WavReader.SampleRate;

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);

            // Convert the dynamic config to IndexCalculateConfig class and merge in the unnecesary parameters.
            IndexCalculateConfig config = IndexCalculateConfig.GetConfig(analysisSettings.Configuration, false);

            int frameSize = config.FrameLength;
            int frameStep = frameSize;
            //var indexCalculationDuration = config.IndexCalculationDuration;

            //// get duration in seconds and sample count and frame count
            //double subsegmentDurationInSeconds = indexCalculationDuration.TotalSeconds;
            //int subsegmentSampleCount = (int)(subsegmentDurationInSeconds * sampleRate);
            //double subsegmentFrameCount = subsegmentSampleCount / (double)frameStep;
            //subsegmentFrameCount = (int)Math.Ceiling(subsegmentFrameCount);

            //// In order not to lose the last fractional frame, round up the frame number
            //// and get the exact number of samples in the integer number of frames.
            //// Do this because when IndexCalculationDuration = 100ms, the number of frames is only 8.
            //subsegmentSampleCount = (int)(subsegmentFrameCount * frameStep);

            //// get start and end samples of the subsegment and noise segment
            //double localOffsetInSeconds = segmentSettings.SegmentStartOffset.TotalSeconds;
            //int startSample = (int)(localOffsetInSeconds * sampleRate);
            //int endSample = startSample + subsegmentSampleCount - 1;

            // Default behaviour: set SUBSEGMENT = total recording
            AudioRecording subsegmentRecording = recording;

            //double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, startSample, subsegmentSampleCount);
            //var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
            //subsegmentRecording = new AudioRecording(wr);

            // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(subsegmentRecording, frameSize, frameStep);

            analysisSettings.AnalysisImageSaveBehavior = SaveBehavior.Always;

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave())
            {
                string imagePath = Path.Combine(outputDirectory.FullName, segmentSettings.SegmentImageFile.Name);

                //prepare amplitude spectrogram
                double[,] amplitudeSpectrogramData = dspOutput1.AmplitudeSpectrogram; // get amplitude spectrogram.
                var image = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(amplitudeSpectrogramData));
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = new FileInfo(imagePath);
                LoggedConsole.WriteLine("See {0} for spectrogram pictures", imagePath);
            }

            return analysisResults;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // no op
        }
    }
}