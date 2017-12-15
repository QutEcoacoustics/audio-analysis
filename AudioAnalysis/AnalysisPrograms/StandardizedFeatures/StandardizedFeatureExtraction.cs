﻿// <copyright file="StandardizedFeatureExtraction.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.StandardizedFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
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

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // Construct variable 'configuration' that stores the properties of config file in non-dynamic way
            base.BeforeAnalyze(analysisSettings);
            StandardizedFeatureExtractionConfig configuration = Yaml.Deserialise<StandardizedFeatureExtractionConfig>(analysisSettings.ConfigFile);
            analysisSettings.AnalysisAnalyzerSpecificConfiguration = configuration;
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
            StandardizedFeatureExtractionConfig configuration = (StandardizedFeatureExtractionConfig)analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);

            // Default behaviour: set SUBSEGMENT = total recording
            AudioRecording subsegmentRecording = recording;

            // Create list to store images so they can be combined later
            var list = new List<Image>();
            string imagePath = Path.Combine(outputDirectory.FullName, segmentSettings.SegmentImageFile.Name);
            int maxImageWidth = 0;

            foreach (var band in configuration.Bands)
            {
                int frameSize = band.FftWindow;
                int frameStep = frameSize;

                // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
                var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(subsegmentRecording, frameSize, frameStep);

                // Prepare amplitude spectrogram
                double[,] amplitudeSpectrogramData = dspOutput1.AmplitudeSpectrogram; // get amplitude spectrogram.
                var image = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(amplitudeSpectrogramData));

                // Add image to list
                list.Add(image);

                // Update maximal width of image
                if (image.Width > maxImageWidth)
                {
                    maxImageWidth = image.Width;
                }
            }

            // Set savebehavior to always so it saves image
            analysisSettings.AnalysisImageSaveBehavior = SaveBehavior.Always;

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave())
            {
                Image finalImage = ImageTools.CombineImagesVertically(list, maxImageWidth);
                finalImage.Save(imagePath, ImageFormat.Png);
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