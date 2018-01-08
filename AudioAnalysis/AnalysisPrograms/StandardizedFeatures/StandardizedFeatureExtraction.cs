// <copyright file="StandardizedFeatureExtraction.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.StandardizedFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Security.Policy;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
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

            //double epsilon = recording.Epsilon;
            //int sampleRate = recording.WavReader.SampleRate;
            //int nyquist = sampleRate / 2;

            //// Fileinfo class for the index properties configuration file
            //FileInfo IndexPropertiesConfig = new FileInfo(configuration.IndexPropertiesConfig);
            //var indexProperties = IndexProperties.GetIndexProperties(IndexPropertiesConfig);
            //TimeSpan IndexCalculationDuration = new TimeSpan(0, 0, (int)configuration.IndexCalculationDuration);

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);
            analysisResults.AnalysisIdentifier = this.Identifier;

            // Default behaviour: set SUBSEGMENT = total recording
            AudioRecording subsegmentRecording = recording;

            // Only for debug create image
            // Create list to store images so they can be combined later
            var list = new List<Image>();
            string imagePath = segmentSettings.SegmentImageFile.FullName;
            int maxImageWidth = 0;

            foreach (var band in configuration.Bands)
            {
                int frameSize = band.FftWindow;
                int frameStep = frameSize;

                //int freqBinCount = frameSize / 2;

                //int midFreqBound = configuration.MidFreqBound;
                //int lowFreqBound = configuration.LowFreqBound;

                // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
                var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(subsegmentRecording, frameSize, frameStep);

                //var dspOutput2 = dspOutput1;

                //// Linear or Octave frequency scale? Set Linear as default.
                //var freqScale = new FrequencyScale(nyquist: nyquist, frameSize: frameSize, hertzLinearGridInterval: 1000);
                //var freqScaleType = configuration.FrequencyScale;
                //bool octaveScale = freqScaleType == "Octave";
                //if (octaveScale)
                //{
                //    // only allow one octave scale at the moment - for Jasco marine recordings.
                //    // ASSUME fixed Occtave scale - USEFUL ONLY FOR JASCO 64000sr MARINE RECORDINGS
                //    // If you wish to use other octave scale types then need to put in the config file and and set up recovery here.
                //    freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);

                //    // Recalculate the spectrogram according to octave scale. This option works only when have high SR recordings.
                //    dspOutput1.AmplitudeSpectrogram = OctaveFreqScale.AmplitudeSpectra(
                //        dspOutput1.AmplitudeSpectrogram,
                //        dspOutput1.WindowPower,
                //        sampleRate,
                //        epsilon,
                //        freqScale);
                //    dspOutput1.NyquistBin = dspOutput1.AmplitudeSpectrogram.GetLength(1) - 1; // ASSUMPTION!!! Nyquist is in top Octave bin - not necessarily true!!
                //}

                // Prepare amplitude spectrogram
                double[,] amplitudeSpectrogramData = dspOutput1.AmplitudeSpectrogram; // get amplitude spectrogram.
                double[,] m;

                // Transform from Frequency to Mel Scale
                // band.Melscale defines the number of bins that will be reduced to
                if (band.MelScale != 0)
                {
                    m = MFCCStuff.MelFilterBank(amplitudeSpectrogramData, band.MelScale, recording.Nyquist, 0, recording.Nyquist);
                }
                else
                {
                    m = amplitudeSpectrogramData;
                }

                // Select band determined by min and max bandwidth
                int minBand = (int)(m.GetLength(1) * band.Bandwidth.Min);
                int maxBand = (int)(m.GetLength(1) * band.Bandwidth.Max) - 1;

                double[,] mband = MatrixTools.Submatrix(m, 0, minBand, m.GetLength(0) - 1, maxBand);

                // Create image of amplitude spectrogram
                var image = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(mband));

                // Add image to list
                list.Add(image);

                // Update maximal width of image
                if (image.Width > maxImageWidth)
                {
                    maxImageWidth = image.Width;
                }

                //    // INITIALISE a RESULTS STRUCTURE TO return
                //    // initialize a result object in which to store SummaryIndexValues and SpectralIndexValues etc.
                //    var result = new IndexCalculateResult(freqBinCount, indexProperties, IndexCalculationDuration, subsegmentOffsetTimeSpan);
                //    SummaryIndexValues summaryIndices = result.SummaryIndexValues;
                //    SpectralIndexValues spectralIndices = result.SpectralIndexValues;

                //    // (B) ################################## EXTRACT SPECTRAL INDICES FROM THE AMPLITUDE SPECTROGRAM ##################################

                //    // i: CALCULATE SPECTRUM OF THE SUM OF FREQ BIN AMPLITUDES - used for later calculation of ACI
                //    spectralIndices.SUM = MatrixTools.SumColumns(m);

                //    // Calculate lower and upper boundary bin ids.
                //    // Boundary between low & mid frequency bands is to avoid low freq bins containing anthropogenic noise. These biased index values away from biophony.
                //    // Boundary of upper bird-band is to avoid high freq artefacts due to mp3.
                //    int lowerBinBound = (int)Math.Ceiling(lowFreqBound / dspOutput1.FreqBinWidth);
                //    int middleBinBound = (int)Math.Ceiling(midFreqBound / dspOutput1.FreqBinWidth);

                //    // calculate number of freq bins in the bird-band.
                //    int midBandBinCount = middleBinBound - lowerBinBound + 1;

                //    if (octaveScale)
                //    {
                //        // the above frequency bin bounds do not apply with octave scale. Need to recalculate them suitable for Octave scale recording.
                //        lowFreqBound = freqScale.LinearBound;
                //        lowerBinBound = freqScale.GetBinIdForHerzValue(lowFreqBound);

                //        midFreqBound = 8000; // This value appears suitable for Jasco Marine recordings. Not much happens above 8kHz.

                //        //middleBinBound = freqScale.GetBinIdForHerzValue(midFreqBound);
                //        middleBinBound = freqScale.GetBinIdInReducedSpectrogramForHerzValue(midFreqBound);
                //        midBandBinCount = middleBinBound - lowerBinBound + 1;
                //    }

                //    // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than SR/2.
                //    // original sample rate can be anything 11.0-44.1 kHz.
                //    int sampleRateOfOriginalAudioFile = segmentSettings.Segment.SourceMetadata.SampleRate;
                //    int originalNyquist = sampleRateOfOriginalAudioFile / 2;

                //    // if upsampling has been done
                //    if (dspOutput1.NyquistFreq > originalNyquist)
                //    {
                //        dspOutput1.NyquistFreq = originalNyquist;
                //        dspOutput1.NyquistBin = (int)Math.Floor(originalNyquist / dspOutput1.FreqBinWidth); // note that binwidth does not change
                //    }

                //    // ii: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
                //    spectralIndices.DIF = AcousticComplexityIndex.SumOfAmplitudeDifferences(m);

                //    double[] aciSpectrum = AcousticComplexityIndex.CalculateACI(m);
                //    spectralIndices.ACI = aciSpectrum;

                //    // remove low freq band of ACI spectrum and store average ACI value
                //    double[] reducedAciSpectrum = DataTools.Subarray(aciSpectrum, lowerBinBound, midBandBinCount);
                //    summaryIndices.AcousticComplexity = reducedAciSpectrum.Average();

                //    // iii: CALCULATE the H(t) or Temporal ENTROPY Spectrum and then reverse the values i.e. calculate 1-Ht for energy concentration
                //    double[] temporalEntropySpectrum = AcousticEntropy.CalculateTemporalEntropySpectrum(m);
                //    for (int i = 0; i < temporalEntropySpectrum.Length; i++)
                //    {
                //        temporalEntropySpectrum[i] = 1 - temporalEntropySpectrum[i];
                //    }

                //    spectralIndices.ENT = temporalEntropySpectrum;

                //    // iv: remove background noise from the amplitude spectrogram
                //    //     First calculate the noise profile from the amplitude sepctrogram
                //    double[] spectralAmplitudeBgn = NoiseProfile.CalculateBackgroundNoise(dspOutput2.AmplitudeSpectrogram);
                //    m = SNR.TruncateBgNoiseFromSpectrogram(m, spectralAmplitudeBgn);

                //    // AMPLITUDE THRESHOLD for smoothing background, nhThreshold, assumes background noise ranges around -40dB.
                //    // This value corresponds to approximately 6dB above backgorund.
                //    m = SNR.RemoveNeighbourhoodBackgroundNoise(m, nhThreshold: 0.015);
                //    ////ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
                //    ////DataTools.writeBarGraph(modalValues);

                //    // v: ENTROPY OF AVERAGE SPECTRUM & VARIANCE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
                //    var tuple = AcousticEntropy.CalculateSpectralEntropies(m, lowerBinBound, midBandBinCount);

                //    // ENTROPY of spectral averages - Reverse the values i.e. calculate 1-Hs and 1-Hv, and 1-Hcov for energy concentration
                //    summaryIndices.EntropyOfAverageSpectrum = 1 - tuple.Item1;

                //    // ENTROPY of spectrum of Variance values
                //    summaryIndices.EntropyOfVarianceSpectrum = 1 - tuple.Item2;

                //    // ENTROPY of spectrum of Coefficient of Variation values
                //    summaryIndices.EntropyOfCoVSpectrum = 1 - tuple.Item3;

                //    // vi: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
                //    //     First extract High band SPECTROGRAM which is now noise reduced
                //    double entropyOfPeaksSpectrum = AcousticEntropy.CalculateEntropyOfSpectralPeaks(m, lowerBinBound, middleBinBound);
                //    summaryIndices.EntropyOfPeaksSpectrum = 1 - entropyOfPeaksSpectrum;
                
                // Calculate spectral indices

                TimeSpan IndexCalculationDuration = new TimeSpan(0, 0, (int)configuration.IndexCalculationDuration);
                FileInfo IndexPropertiesConfig = new FileInfo(configuration.IndexPropertiesConfig);

                // calculate indices for each subsegment for each band
                IndexCalculateResult[] subsegmentResults = Acoustic.CalculateIndicesInSubsegments(
                    recording,
                    segmentSettings.SegmentStartOffset,
                    segmentSettings.AnalysisIdealSegmentDuration,
                    IndexCalculationDuration,
                    configuration.BgNoiseNeighbourhood,
                    IndexPropertiesConfig,
                    segmentSettings.Segment.SourceMetadata.SampleRate,
                    analysisSettings.Configuration);

                var trackScores = new List<Plot>(subsegmentResults.Length);
                var tracks = new List<SpectralTrack>(subsegmentResults.Length);

                analysisResults.SummaryIndices = new SummaryIndexBase[subsegmentResults.Length];
                analysisResults.SpectralIndices = new SpectralIndexBase[subsegmentResults.Length];
                for (int i = 0; i < subsegmentResults.Length; i++)
                {
                    var indexCalculateResult = subsegmentResults[i];
                    indexCalculateResult.SummaryIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;
                    indexCalculateResult.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;

                    analysisResults.SummaryIndices[i] = indexCalculateResult.SummaryIndexValues;
                    analysisResults.SpectralIndices[i] = indexCalculateResult.SpectralIndexValues;
                    trackScores.AddRange(indexCalculateResult.TrackScores);
                    if (indexCalculateResult.Tracks != null)
                    {
                        tracks.AddRange(indexCalculateResult.Tracks);
                    }
                }

                analysisSettings.AnalysisDataSaveBehavior = true;
                if (analysisSettings.AnalysisDataSaveBehavior)
                {
                    this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
                    analysisResults.SummaryIndicesFile = segmentSettings.SegmentSummaryIndicesFile;
                }

                if (analysisSettings.AnalysisDataSaveBehavior)
                {
                    analysisResults.SpectraIndicesFiles =
                        this.WriteSpectrumIndicesFiles(
                            segmentSettings.SegmentSpectrumIndicesDirectory,
                            Path.GetFileNameWithoutExtension(segmentSettings.SegmentAudioFile.Name),
                            analysisResults.SpectralIndices);
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
            Csv.WriteToCsv(destination, results.Cast<SummaryIndexValues>());
        }

        public List<FileInfo> WriteSpectrumIndicesFilesCustom(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            var selectors = results.First().GetSelectors();

            var spectralIndexFiles = new List<FileInfo>(selectors.Count);

            foreach (var kvp in selectors)
            {
                // write spectrogram to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultPath(destination, fileNameBase, this.Identifier + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, results, kvp.Value);
            }

            return spectralIndexFiles;
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            return this.WriteSpectrumIndicesFilesCustom(destination, fileNameBase, results);
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