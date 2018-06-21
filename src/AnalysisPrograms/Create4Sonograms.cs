// <copyright file="Create4Sonograms.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;
    using TowseyLibrary;

    /// <summary>
    /// Call this class by using the activity (first command line argument) "Create4Sonograms"
    /// </summary>
    public static class Create4Sonograms
    {
        public const string CommandName = "DrawSpectrograms";

        [Command(
            CommandName,
            Description = "[BETA] Creates a set of four standard-scale spectrograms derived using different algorithms. For short recordings only.")]
        public class Arguments : SourceAndConfigArguments
        {
            [Option(Description = "A file path to write output to")]
            [NotExistingFile]
            [Required]
            [LegalFilePath]
            public string Output { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                Create4Sonograms.Main(this);
                return this.Ok();
            }
        }

        public static void Main(Arguments arguments)
        {

            //1. set up the necessary files
            //DirectoryInfo diSource = arguments.Source.Directory;
            FileInfo fiSourceRecording = arguments.Source;
            FileInfo fiConfig = arguments.Config.ToFileInfo();
            FileInfo fiImage = arguments.Output.ToFileInfo();

            fiImage.CreateParentDirectories();

            string title = "# CREATE FOUR (4) SONOGRAMS FROM AUDIO RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + fiSourceRecording.Name);
            LoggedConsole.WriteLine("# Output image file: " + fiImage);

            //2. get the config dictionary
            Config configuration = ConfigFile.Deserialize(fiConfig);

            //below three lines are examples of retrieving info from Config config
            //string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            //bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            //scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

            //3 transfer conogram parameters to a dictionary to be passed around
            var configDict = new Dictionary<string, string>();

            // #Resample rate must be 2 X the desired Nyquist. Default is that of recording.
            configDict["ResampleRate"] = (configuration.GetIntOrNull(AnalysisKeys.ResampleRate) ?? 17640).ToString();
            configDict["FrameLength"] = configuration[AnalysisKeys.FrameLength] ?? "512";
            int frameSize = configuration.GetIntOrNull(AnalysisKeys.FrameLength) ?? 512;

            // #Frame Overlap as fraction: default=0.0
            configDict["FrameOverlap"] = configuration[AnalysisKeys.FrameOverlap] ?? "0.0";
            double windowOverlap = configuration.GetDoubleOrNull(AnalysisKeys.FrameOverlap) ?? 0.0;

            // #MinHz: 500
            // #MaxHz: 3500
            // #NOISE REDUCTION PARAMETERS
            configDict["DoNoiseReduction"] = configuration["DoNoiseReduction"] ?? "true";
            configDict["BgNoiseThreshold"] = configuration["BgNoiseThreshold"] ?? "3.0";

            configDict["ADD_AXES"] = configuration["ADD_AXES"] ?? "true";
            configDict["AddSegmentationTrack"] = configuration["AddSegmentationTrack"] ?? "true";

            // 3: GET RECORDING
            var startOffsetMins = TimeSpan.Zero;
            var endOffsetMins = TimeSpan.Zero;

            FileInfo fiOutputSegment = fiSourceRecording;
            if (!(startOffsetMins == TimeSpan.Zero && endOffsetMins == TimeSpan.Zero))
            {
                var buffer = new TimeSpan(0, 0, 0);
                fiOutputSegment = new FileInfo(Path.Combine(fiImage.DirectoryName, "tempWavFile.wav"));

                //This method extracts segment and saves to disk at the location fiOutputSegment.
                var resampleRate = configuration.GetIntOrNull(AnalysisKeys.ResampleRate) ?? AppConfigHelper.DefaultTargetSampleRate;
                AudioRecording.ExtractSegment(fiSourceRecording, startOffsetMins, endOffsetMins, buffer, resampleRate, fiOutputSegment);
            }

            var recording = new AudioRecording(fiOutputSegment.FullName);

            // EXTRACT ENVELOPE and SPECTROGRAM
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFfts(recording, frameSize, windowOverlap);

            // average absolute value over the minute recording
            ////double[] avAbsolute = dspOutput.Average;

            // (A) ################################## EXTRACT INDICES FROM THE SIGNAL WAVEFORM ##################################
            // var wavDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);
            // double totalSeconds = wavDuration.TotalSeconds;

            // double[] signalEnvelope = dspOutput.Envelope;
            // double avSignalEnvelope = signalEnvelope.Average();
            // double[] frameEnergy = dspOutput.FrameEnergy;
            // double highAmplIndex = dspOutput.HighAmplitudeCount / totalSeconds;
            // double binWidth = dspOutput.BinWidth;
            // int nyquistBin = dspOutput.NyquistBin;
            // dspOutput.WindowPower,
            // dspOutput.FreqBinWidth
            int nyquistFreq = dspOutput.NyquistFreq;
            double epsilon = recording.Epsilon;

            // i: prepare amplitude spectrogram
            double[,] amplitudeSpectrogramData = dspOutput.AmplitudeSpectrogram; // get amplitude spectrogram.
            var image1 = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(amplitudeSpectrogramData));

            // ii: prepare decibel spectrogram prior to noise removal
            double[,] decibelSpectrogramdata = MFCCStuff.DecibelSpectra(dspOutput.AmplitudeSpectrogram, dspOutput.WindowPower, recording.SampleRate, epsilon);
            decibelSpectrogramdata = MatrixTools.NormaliseMatrixValues(decibelSpectrogramdata);
            var image2 = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(decibelSpectrogramdata));

            // iii: Calculate background noise spectrum in decibels
            // Calculate noise value for each freq bin.
            double sdCount = 0.0; // number of SDs above the mean for noise removal
            var decibelProfile = NoiseProfile.CalculateModalNoiseProfile(decibelSpectrogramdata, sdCount);

            // DataTools.writeBarGraph(dBProfile.NoiseMode);

            // iv: Prepare noise reduced spectrogram
            decibelSpectrogramdata = SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogramdata, decibelProfile.NoiseThresholds);

            //double dBThreshold = 1.0; // SPECTRAL dB THRESHOLD for smoothing background
            //decibelSpectrogramdata = SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogramdata, dBThreshold);
            var image3 = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(decibelSpectrogramdata));

            // prepare new sonogram config and draw second image going down different code pathway
            var config = new SonogramConfig
            {
                MinFreqBand = 0,
                MaxFreqBand = 10000,
                NoiseReductionType = SNR.KeyToNoiseReductionType("Standard"),
                NoiseReductionParameter = 1.0,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
            };

            //var mfccConfig = new MfccConfiguration(config);
            int bandCount = config.mfccConfig.FilterbankCount;
            bool doMelScale = config.mfccConfig.DoMelScale;
            int ccCount = config.mfccConfig.CcCount;
            int fftBins = config.FreqBinCount;  //number of Hz bands = 2^N +1 because includes the DC band
            int minHz = config.MinFreqBand ?? 0;
            int maxHz = config.MaxFreqBand ?? nyquistFreq;

            var standardSonogram = new SpectrogramStandard(config, recording.WavReader);
            var image4 = standardSonogram.GetImage();

            // TODO next line crashes - does not produce cepstral sonogram.
            //SpectrogramCepstral cepSng = new SpectrogramCepstral(config, recording.WavReader);
            //Image image5 = cepSng.GetImage();

            //var mti = SpectrogramTools.Sonogram2MultiTrackImage(sonogram, configDict);
            //var image = mti.GetImage();

            //Image image = SpectrogramTools.Matrix2SonogramImage(deciBelSpectrogram, config);
            //Image image = SpectrogramTools.Audio2SonogramImage(FileInfo fiAudio, Dictionary<string, string> configDict);

            //prepare sonogram images
            var protoImage6 = new Image_MultiTrack(standardSonogram.GetImage(doHighlightSubband: false, add1KHzLines: true, doMelScale: false));
            protoImage6.AddTrack(ImageTrack.GetTimeTrack(standardSonogram.Duration, standardSonogram.FramesPerSecond));
            protoImage6.AddTrack(ImageTrack.GetWavEnvelopeTrack(recording, protoImage6.SonogramImage.Width));
            protoImage6.AddTrack(ImageTrack.GetSegmentationTrack(standardSonogram));
            var image6 = protoImage6.GetImage();

            var list = new List<Image>();
            list.Add(image1); // amplitude spectrogram
            list.Add(image2); // decibel spectrogram before noise removal
            list.Add(image3); // decibel spectrogram after noise removal
            list.Add(image4); // second version of noise reduced spectrogram

            //list.Add(image5); // ceptral sonogram
            list.Add(image6); // multitrack image

            Image finalImage = ImageTools.CombineImagesVertically(list);
            finalImage.Save(fiImage.FullName, ImageFormat.Png);

            ////2: NOISE REMOVAL
            //double[,] originalSg = sonogram.Data;
            //double[,] mnr        = sonogram.Data;
            //mnr = ImageTools.WienerFilter(mnr, 3);

            //double backgroundThreshold = 4.0;   //SETS MIN DECIBEL BOUND
            //var output = SNR.NoiseReduce(mnr, NoiseReductionType.STANDARD, backgroundThreshold);

            //double ConfigRange = 70;        //sets the the max dB
            //mnr = SNR.SetConfigRange(output.Item1, 0.0, ConfigRange);

            ////3: Spectral tracks sonogram
            //byte[,] binary = MatrixTools.IdentifySpectralRidges(mnr);
            //binary = MatrixTools.ThresholdBinarySpectrum(binary, mnr, 10);
            //binary = MatrixTools.RemoveOrphanOnesInBinaryMatrix(binary);
            ////binary = MatrixTools.PickOutLines(binary); //syntactic approach

            //sonogram.SetBinarySpectrum(binary);
            ////sonogram.Data = SNR.SpectralRidges2Intensity(binary, originalSg);

            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, false));
            //image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            //image.AddTrack(ImageTrack.GetWavEnvelopeTrack(recording, image.sonogramImage.Width));
            //image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            //fn = outputFolder + wavFileName + "_tracks.png";
            //image.Save(fn);
            //LoggedConsole.WriteLine("Spectral tracks sonogram to file: " + fn);

            //3: prepare image of spectral peaks sonogram
            //sonogram.Data = SNR.NoiseReduce_Peaks(originalSg, dynamicRange);
            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(ImageTrack.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            //fn = outputFolder + wavFileName + "_peaks.png";
            //image.Save(fn);

            //LoggedConsole.WriteLine("Spectral peaks  sonogram to file: " + fn);

            //4: Sobel approach
            //sonogram.Data = SNR.NoiseReduce_Sobel(originalSg, dynamicRange);
            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(ImageTrack.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            //fn = outputFolder + wavFileName + "_sobel.png";
            //image.Save(fn);
            //LoggedConsole.WriteLine("Sobel sonogram to file: " + fn);

            // I1.txt contains the sonogram matrix produced by matlab
            //string matlabFile = @"C:\SensorNetworks\Software\AudioAnalysis\AED\Test\matlab\GParrots_JB2_20090607-173000.wav_minute_3\I1.txt";
            //double[,] matlabMatrix = Util.fileToMatrix(matlabFile, 256, 5166);

            //LoggedConsole.WriteLine(matrix[0, 2] + " vs " + matlabMatrix[254, 0]);
            //LoggedConsole.WriteLine(matrix[0, 3] + " vs " + matlabMatrix[253, 0]);

            // TODO put this back once sonogram issues resolved

            /*
            LoggedConsole.WriteLine("START: AED");
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(3.0, 100, matrix);
            LoggedConsole.WriteLine("END: AED");


            //set up static variables for init Acoustic events
            //AcousticEvent.   doMelScale = config.DoMelScale;
            AcousticEvent.FreqBinCount = config.FreqBinCount;
            AcousticEvent.FreqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount;
            //  int minF        = (int)config.MinFreqBand;
            //  int maxF        = (int)config.MaxFreqBand;
            AcousticEvent.FrameDuration = config.GetFrameOffset();


            var events = new List<EventPatternRecog.Rectangle>();
            foreach (Oblong o in oblongs)
            {
                var e = new AcousticEvent(o);
                events.Add(new EventPatternRecog.Rectangle(e.StartTime, (double) e.MaxFreq, e.StartTime + e.Duration, (double)e.MinFreq));
                //LoggedConsole.WriteLine(e.StartTime + "," + e.Duration + "," + e.MinFreq + "," + e.MaxFreq);
            }

            LoggedConsole.WriteLine("# AED events: " + events.Count);

            LoggedConsole.WriteLine("START: EPR");
            IEnumerable<EventPatternRecog.Rectangle> eprRects = EventPatternRecog.detectGroundParrots(events);
            LoggedConsole.WriteLine("END: EPR");

            var eprEvents = new List<AcousticEvent>();
            foreach (EventPatternRecog.Rectangle r in eprRects)
            {
                var ae = new AcousticEvent(r.Left, r.Right - r.Left, r.Bottom, r.Top, false);
                LoggedConsole.WriteLine(ae.WriteProperties());
                eprEvents.Add(ae);
            }

            string imagePath = Path.Combine(outputFolder, "RESULTS_" + Path.GetFileNameWithoutExtension(recording.BaseName) + ".png");

            bool doHighlightSubband = false; bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(ImageTrack.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            image.AddEvents(eprEvents);
            image.Save(outputFolder + wavFileName + ".png");
             */

            LoggedConsole.WriteLine("\nFINISHED!");
        }
    }
}
