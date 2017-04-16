namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using Acoustics.Shared;
    using Production;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using PowerArgs;
    using TowseyLibrary;

    /// <summary>
    /// Call this class by using the activity (first command line argument) "Create4Sonograms"
    /// </summary>
    public static class Create4Sonograms
    {


        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments : SourceAndConfigArguments
        {
            [ArgDescription("A file path to write output to")]
            [ArgNotExistingFile]
            [ArgRequired]
            public FileInfo Output { get; set; }

            //[ArgDescription("The start offset (in minutes) of the source audio file to operate on")]
            //[ArgRange(0, double.MaxValue)]
            //public double? StartOffset { get; set; }

            //[ArgDescription("The end offset (in minutes) of the source audio file to operate on")]
            //[ArgRange(0, double.MaxValue)]
            //public double? EndOffset { get; set; }


            public static string Description()
            {
                return "Does cool stuff";
            }

            public static string AdditionalNotes()
            {
                return "StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.";
            }
        }

        private static Arguments Dev()
        {

            return new Arguments
            {
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png".ToFileInfo(),
                Source = @"C:\SensorNetworks\WavFiles\TestRecordings\BAC2_20071008-085040.wav".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\Sonograms\BAC2Sonograms\BAC2_20071008-085040.png".ToFileInfo(),
                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo(),
            };

            throw new NoDeveloperMethodException();
        }

        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            arguments.Output.CreateParentDirectories();


            string title = "# CREATE FOUR (4) SONOGRAMS FROM AUDIO RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + arguments.Source.Name);
            LoggedConsole.WriteLine("# Output image file: " + arguments.Output);


            //1. set up the necessary files
            DirectoryInfo diSource = arguments.Source.Directory;
            FileInfo fiSourceRecording = arguments.Source;
            FileInfo fiConfig = arguments.Config;
            FileInfo fiImage = arguments.Output;

            //2. get the config dictionary
            dynamic configuration = Yaml.Deserialise(fiConfig);

            //below three lines are examples of retrieving info from dynamic config
            //string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            //bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            //scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

            //3 transfer conogram parameters to a dictionary to be passed around
            var configDict = new Dictionary<string, string>();
            configDict["FrameLength"] = configuration[AnalysisKeys.FrameLength] ?? 512;
            int frameSize             = configuration[AnalysisKeys.FrameLength] ?? 512;
            // #Frame Overlap as fraction: default=0.0
            configDict["FrameOverlap"] = configuration[AnalysisKeys.FrameOverlap] ?? 0.0;
            double windowOverlap       = configuration[AnalysisKeys.FrameOverlap] ?? 0.0;
            // #Resample rate must be 2 X the desired Nyquist. Default is that of recording.
            configDict["ResampleRate"] = configuration[AnalysisKeys.ResampleRate] ?? 17640;
            // #MinHz: 500
            // #MaxHz: 3500
            // #NOISE REDUCTION PARAMETERS
            configDict["DoNoiseReduction"] = configuration["DoNoiseReduction"] ?? true;
            configDict["BgNoiseThreshold"] = configuration["BgNoiseThreshold"] ?? 3.0;

            configDict["ADD_AXES"] = configuration["ADD_AXES"] ?? true;
            configDict["AddSegmentationTrack"] = configuration["AddSegmentationTrack"] ?? true;

            // 3: GET RECORDING
            TimeSpan startOffsetMins = TimeSpan.Zero;
            TimeSpan endOffsetMins = TimeSpan.Zero;

            FileInfo fiOutputSegment = fiSourceRecording;
            if (!((startOffsetMins == TimeSpan.Zero) && (endOffsetMins == TimeSpan.Zero)))
            {
                TimeSpan buffer = new TimeSpan(0, 0, 0);
                fiOutputSegment = new FileInfo(Path.Combine(arguments.Output.DirectoryName, "tempWavFile.wav"));
                //This method extracts segment and saves to disk at the location fiOutputSegment.
                var resampleRate = (int?)configuration[AnalysisKeys.ResampleRate] ?? AppConfigHelper.DefaultTargetSampleRate;
                AudioRecording.ExtractSegment(fiSourceRecording, startOffsetMins, endOffsetMins, buffer, resampleRate, fiOutputSegment);
            }

            var recording = new AudioRecording(fiOutputSegment.FullName);
            TimeSpan wavDuration = TimeSpan.FromSeconds(recording.WavReader.Time.TotalSeconds);

            // EXTRACT ENVELOPE and SPECTROGRAM
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFfts(recording, frameSize, windowOverlap);

            // average absolute value over the minute recording
            ////double[] avAbsolute = dspOutput.Average;

            // (A) ################################## EXTRACT INDICES FROM THE SIGNAL WAVEFORM ##################################
            double[] signalEnvelope = dspOutput.Envelope;
            double avSignalEnvelope = signalEnvelope.Average();
            // double[] frameEnergy = dspOutput.FrameEnergy;

            double totalSeconds = wavDuration.TotalSeconds;
            double highAmplIndex = dspOutput.MaxAmplitudeCount / totalSeconds;

            int nyquistFreq = dspOutput.NyquistFreq;
            ////double binWidth = dspOutput.BinWidth;
            int nyquistBin = dspOutput.NyquistBin;
            // dspOutput.WindowPower,
            // dspOutput.FreqBinWidth
            // recording.SampleRate

            double[,] amplitudeSpectrogram = dspOutput.AmplitudeSpectrogram; // get amplitude spectrogram.
            //DataTools.WriteMinMaxOfArray(MatrixTools.Matrix2Array(amplitudeSpectrogram));

            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput.AmplitudeSpectrogram, dspOutput.WindowPower, recording.SampleRate, epsilon);
            //DataTools.WriteMinMaxOfArray(MatrixTools.Matrix2Array(deciBelSpectrogram));
            //deciBelSpectrogram = MatrixTools.Normalise(deciBelSpectrogram, -80, -30);
            //DataTools.WriteMinMaxOfArray(MatrixTools.Matrix2Array(deciBelSpectrogram));

            // ii: Calculate background noise spectrum in decibels
            double sdCount = 0.0; // number of SDs above the mean for noise removal
            NoiseProfile dBProfile = NoiseProfile.CalculateModalNoiseProfile(deciBelSpectrogram, sdCount);       // calculate noise value for each freq bin.
            //DataTools.writeBarGraph(dBProfile.NoiseMode);


            //deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, dBProfile.NoiseThresholds);
            //double dBThreshold = 3.0; // SPECTRAL dB THRESHOLD for smoothing background
            //deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, dBThreshold);

            deciBelSpectrogram = MatrixTools.normalise(deciBelSpectrogram);
            //DataTools.WriteMinMaxOfArray(MatrixTools.Matrix2Array(deciBelSpectrogram));


            var list = new List<Image>();
            Image image1 = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(amplitudeSpectrogram));

            Image image2 = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(deciBelSpectrogram));


            //BaseSonogram sonogram = SpectrogramTools.Audio2Sonogram(fiAudio, configDict);
            //var mti = Sonogram2MultiTrackImage(sonogram, configDict);
            //var image = mti.GetImage();
            SonogramConfig config = new SonogramConfig();
            config.MinFreqBand = 0;
            config.MaxFreqBand = 8800;
            config.WindowSize = frameSize;
            config.WindowOverlap = windowOverlap;

            //var mfccConfig = new MfccConfiguration(config);
            int bandCount = config.mfccConfig.FilterbankCount;
            bool doMelScale = config.mfccConfig.DoMelScale;
            int ccCount = config.mfccConfig.CcCount;
            int FFTbins = config.FreqBinCount;  //number of Hz bands = 2^N +1. Subtract DC bin
            int minHz = config.MinFreqBand ?? 0;
            int maxHz = config.MaxFreqBand ?? nyquistFreq;


            AmplitudeSonogram amplitudeSonogram = new AmplitudeSonogram(config, amplitudeSpectrogram);
            amplitudeSonogram.SampleRate = recording.SampleRate;
            Image image3 = amplitudeSonogram.GetImage();




            SpectrogramCepstral cepSng = new SpectrogramCepstral(amplitudeSonogram);
            double[,] cepstralCoefficients = cepSng.Data;
            Image image4 = cepSng.GetImage();

            //BaseSonogram sonogram = new SpectrogramStandard(config, amplitudeSpectrogram);
            //sonogram.SampleRate = recording.SampleRate;
            ////Image image1 = sonogram.GetImage();

            //var mti = SpectrogramTools.Sonogram2MultiTrackImage(sonogram, configDict);
            //var image = mti.GetImage();


            //Image image = SpectrogramTools.Matrix2SonogramImage(deciBelSpectrogram, config);
            //Image image = SpectrogramTools.Audio2SonogramImage(FileInfo fiAudio, Dictionary<string, string> configDict);



            list.Add(image1);
            list.Add(image2);
            list.Add(image3);
            list.Add(image4);
            Image finalImage = ImageTools.CombineImagesVertically(list);
            finalImage.Save(fiImage.FullName, ImageFormat.Png);

            //prepare sonogram images
            bool doHighlightSubband = false;
            bool add1kHzLines = true;
            //Image_MultiTrack image = null;


            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.sonogramImage.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //image.Save(fn);
            //LoggedConsole.WriteLine("Ordinary sonogram to file: " + fn);

            ////2: NOISE REMOVAL
            //double[,] originalSg = sonogram.Data;
            //double[,] mnr        = sonogram.Data;
            //mnr = ImageTools.WienerFilter(mnr, 3);

            //double backgroundThreshold = 4.0;   //SETS MIN DECIBEL BOUND
            //var output = SNR.NoiseReduce(mnr, NoiseReductionType.STANDARD, backgroundThreshold);

            //double dynamicRange = 70;        //sets the the max dB
            //mnr = SNR.SetDynamicRange(output.Item1, 0.0, dynamicRange);

            ////3: Spectral tracks sonogram
            //byte[,] binary = MatrixTools.IdentifySpectralRidges(mnr);
            //binary = MatrixTools.ThresholdBinarySpectrum(binary, mnr, 10);
            //binary = MatrixTools.RemoveOrphanOnesInBinaryMatrix(binary);
            ////binary = MatrixTools.PickOutLines(binary); //syntactic approach

            //sonogram.SetBinarySpectrum(binary);
            ////sonogram.Data = SNR.SpectralRidges2Intensity(binary, originalSg);

            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, false));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.sonogramImage.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //fn = outputFolder + wavFileName + "_tracks.png";
            //image.Save(fn);
            //LoggedConsole.WriteLine("Spectral tracks sonogram to file: " + fn);


            //3: prepare image of spectral peaks sonogram
            //sonogram.Data = SNR.NoiseReduce_Peaks(originalSg, dynamicRange);
            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //fn = outputFolder + wavFileName + "_peaks.png";
            //image.Save(fn);

            //LoggedConsole.WriteLine("Spectral peaks  sonogram to file: " + fn);

            //4: Sobel approach
            //sonogram.Data = SNR.NoiseReduce_Sobel(originalSg, dynamicRange);
            //image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
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
            //image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(eprEvents);
            image.Save(outputFolder + wavFileName + ".png");
             */


            //LoggedConsole.WriteLine("\nFINISHED!");
        }


    }
}
