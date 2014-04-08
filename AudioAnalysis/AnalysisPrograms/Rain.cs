using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using Acoustics.Shared;
using Acoustics.Tools;
using Acoustics.Tools.Audio;
using AnalysisBase;

using TowseyLib;
using AudioAnalysisTools;



namespace AnalysisPrograms
{
    using System.Diagnostics.Contracts;

    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    public class Rain : IAnalyser
    {
        public const string key_LOW_FREQ_BOUND = "LOW_FREQ_BOUND";
        public const string key_MID_FREQ_BOUND = "MID_FREQ_BOUND";

        private const int COL_NUMBER = 12;
        private static Type[] COL_TYPES = new Type[COL_NUMBER];
        private static string[] HEADERS = new string[COL_NUMBER];
        private static bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];

        public static string header_count = Keys.INDICES_COUNT;
        //public const string  = count;
        public const string header_startMin = "start-min";
        //public const string header_SecondsDuration = "SegTimeSpan";
        public const string header_avAmpdB  = "avAmp-dB";
        public const string header_snrdB    = "snr-dB";
        public const string header_bgdB     = "bg-dB";
        public const string header_activity = "activity";
        public const string header_spikes   = "spikes";
        public const string header_hfCover  = "hfCover";
        public const string header_mfCover  = "mfCover";
        public const string header_lfCover  = "lfCover";
        public const string header_HAmpl    = "H[t]";
        public const string header_HAvSpectrum  = "H[s]";
        public const string header_AcComplexity = "AcComplexity";


        public const string header_rain     = "rain";
        public const string header_cicada   = "cicada";
        public const string header_negative = "none";


        private const bool verbose = true;
        private const bool writeOutputFile = true;



        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices
        {
            public double snr, bgNoise, activity, spikes, avSig_dB, temporalEntropy; //amplitude indices
            public double lowFreqCover, midFreqCover, hiFreqCover, spectralEntropy;  //, entropyOfVarianceSpectrum; //spectral indices
            public double ACI;

            public Indices(double _snr, double _bgNoise, double _avSig_dB, double _activity, double _spikes,
                            double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                            double _entropyOfAvSpectrum, double _ACI )
            {
                snr = _snr;
                bgNoise = _bgNoise;
                activity = _activity;
                spikes   = _spikes;
                avSig_dB = _avSig_dB;
                temporalEntropy = _entropyAmp;
                hiFreqCover = _hiFreqCover;
                midFreqCover = _midFreqCover;
                lowFreqCover = _lowFreqCover;
                spectralEntropy = _entropyOfAvSpectrum;
                ACI = _ACI;
            }
        } //struct Indices




        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Rain";

        public string DisplayName
        {
            get { return "Rain Indices (DEV)"; }
        }

        private static string identifier = "Towsey." + ANALYSIS_NAME + ".DEV";
        public string Identifier
        {
            get { return identifier; }
        }

        public class Arguments : AnalyserArguments
        {
        }


        public static void Dev(Arguments arguments)
        {
            Log.Verbosity = 1;
            bool debug = MainEntry.InDEBUG;

            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("# FOR EXTRACTION OF RAIN Indices");
            LoggedConsole.WriteLine(date);

            var executeDev = arguments == null;
            if (executeDev)
            {

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min602.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min944.wav";   //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min1031.wav";  //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min1036.wav";  //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min1101.wav";  //rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\Jackaroo_20080715-103940.wav";   //koala
                //string recordingPath = @"C:\SensorNetworks\WavFiles\KoalaMale\SmallTestSet\HoneymoonBay_StBees_20080909-013000.wav";   //koala & mobile spikes
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Adelotus_brevis_TuskedFrog_BridgeCreek.wav";   //walking on dry leaves
                //string recordingPath = @"C:\SensorNetworks\Output\SunshineCoast\Acoustic\Site1\DM420036_min1081.wav";   //cicada
                string recordingPath = @"C:\SensorNetworks\Output\SunshineCoast\Acoustic\Site1\DM420036_min1076.wav";
                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Rain.cfg";
                string outputDir = @"C:\SensorNetworks\Output\Rain\";
                //string csvPath       = @"C:\SensorNetworks\Output\Rain\RainIndices.csv";
                var diOutputDir = new DirectoryInfo(outputDir);

                if (false)
                {
                    string path = @"C:\SensorNetworks\Output\Rain\TrainingExamplesForRain_3class.csv";
                    //read csv ifle
                    var dt = CsvTools.ReadCSVToTable(path, true);
                    //write as SEE5 files.
                    string fileStem = "3Class9features_RainCicadaNone";
                    WriteSee5DataFiles(dt, diOutputDir, fileStem);

                    // TODO: MIKE! check to make sure the "return" here is a suitable replacement for "Exit(0)"
                    throw new NotImplementedException();
                    ////System.Environment.Exit(0);
                    return;
                }

                int startMinute = 0;
                int durationSeconds = 0; //set zero to get entire recording
                var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
                var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
                var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, identifier);

                arguments = new Arguments
                            {
                                Source = recordingPath.ToFileInfo(),
                                Config = configPath.ToFileInfo(),
                                Output = outputDir.ToDirectoryInfo(),
                                TmpWav = segmentFName,
                                //Events = eventsFname,
                                Indices = indicesFname,
                                Sgram = sonogramFname,
                                Start = tsStart.TotalSeconds,
                                Duration = tsDuration.TotalSeconds
                            };
            }

            LoggedConsole.WriteLine("# Output folder:  " + arguments.Output);
            LoggedConsole.WriteLine("# Recording file: " + arguments.Source.Name);

            Execute(arguments);

            if (executeDev)
            {
                var csvIndicies = arguments.Output.CombineFile(arguments.Indices);
                if (!csvIndicies.Exists)
                {
                    Log.WriteLine(
                        "\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.",
                        arguments.Start.Value,
                        arguments.Source.FullName);
                }
                else
                {
                    LoggedConsole.WriteLine("\n");
                    DataTable dt = CsvTools.ReadCSVToTable(csvIndicies.FullName, true);
                    DataTableTools.WriteTable2Console(dt);
                }

                LoggedConsole.WriteLine("\n\n# Finished analysis for RAIN:- " + arguments.Source.Name);
            }
        }


        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            Contract.Requires(arguments != null);
            
            AnalysisSettings analysisSettings = arguments.ToAnalysisSettings();
            TimeSpan tsStart = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan tsDuration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = analysisSettings.AudioFile;
            if (tempF.Exists) tempF.Delete();
            if (tsDuration == TimeSpan.Zero)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(arguments.Source, tempF, new AudioUtilityRequest { TargetSampleRate = IndexCalculate.RESAMPLE_RATE }, analysisSettings.AnalysisBaseTempDirectoryChecked);
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(arguments.Source, tempF, new AudioUtilityRequest { TargetSampleRate = IndexCalculate.RESAMPLE_RATE, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) }, analysisSettings.AnalysisBaseTempDirectoryChecked);
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new Rain();  
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                int iter = 0; //dummy - iteration number would ordinarily be available at this point.
                int startMinute = (int)tsStart.TotalMinutes;
                foreach (DataRow row in dt.Rows)
                {
                    row[IndexProperties.header_count] = iter;
                    row[IndexProperties.header_startMin] = startMinute;
                    row[IndexProperties.header_SecondsDuration] = result.AudioDuration.TotalSeconds;
                }

                CsvTools.DataTable2CSV(dt, analysisSettings.IndicesFile.FullName);
                //DataTableTools.WriteTable2Console(dt);
            }

        }

        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var fiAudioF = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            //######################################################################
            var results = RainAnalyser(fiAudioF, analysisSettings);
            //######################################################################

            if (results == null)  return analysisResults; //nothing to process 

            analysisResults.Data = results.Item1;
            analysisResults.AudioDuration = results.Item2;
            //var sonogram = results.Item3;
            //var scores = results.Item4;

            //if ((sonogram != null) && (analysisSettings.ImageFile != null))
            //{
            //    string imagePath = Path.Combine(diOutputDir.FullName, analysisSettings.ImageFile.Name);
            //    var image = DrawSonogram(sonogram, scores);
            //    var fiImage = new FileInfo(imagePath);
            //    if (fiImage.Exists) fiImage.SafeDeleteFile();
            //    image.Save(imagePath, ImageFormat.Png);
            //    analysisResults.ImageFile = new FileInfo(imagePath);
            //}

            if ((analysisSettings.IndicesFile != null) && (analysisResults.Data != null))
            {
                CsvTools.DataTable2CSV(analysisResults.Data, analysisSettings.IndicesFile.FullName);
            }
            return analysisResults;
        }

        public static Tuple<DataTable, TimeSpan> RainAnalyser(FileInfo fiAudioFile, AnalysisSettings analysisSettings)
        {
            Dictionary<string, string> config = analysisSettings.ConfigDict;

            //get parameters for the analysis
            int frameSize = IndexCalculate.DEFAULT_WINDOW_SIZE;
            double windowOverlap = 0.0;
            int lowFreqBound = IndexCalculate.lowFreqBound;
            int midFreqBound = IndexCalculate.midFreqBound;

            if (config.ContainsKey(Keys.FRAME_LENGTH)) 
                frameSize = ConfigDictionary.GetInt(Keys.FRAME_LENGTH, config);
            if (config.ContainsKey(key_LOW_FREQ_BOUND)) 
                lowFreqBound = ConfigDictionary.GetInt(key_LOW_FREQ_BOUND, config);
            if (config.ContainsKey(key_MID_FREQ_BOUND)) 
                midFreqBound = ConfigDictionary.GetInt(key_MID_FREQ_BOUND, config);
            if (config.ContainsKey(Keys.FRAME_OVERLAP)) 
                windowOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, config);

            //get recording segment
            AudioRecording recording = new AudioRecording(fiAudioFile.FullName);

            //calculate duration/size of various quantities.
            int signalLength = recording.GetWavReader().Samples.Length;
            TimeSpan audioDuration = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double duration        = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(duration * TimeSpan.TicksPerSecond));

            int chunkDuration = 10; //seconds
            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            int chunkCount      = (int)Math.Round(audioDuration.TotalSeconds / (double)chunkDuration);
            int framesPerChunk  = (int)(chunkDuration * framesPerSecond);
            string[] classifications = new string[chunkCount];


            //i: EXTRACT ENVELOPE and FFTs
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            var signalextract = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, epsilon, frameSize, windowOverlap);
            double[]  envelope    = signalextract.Envelope;
            double[,] spectrogram = signalextract.amplitudeSpectrogram;  //amplitude spectrogram
            int colCount = spectrogram.GetLength(1);


            int nyquistFreq = recording.Nyquist;
            int nyquistBin = spectrogram.GetLength(1) - 1;
            double binWidth = nyquistFreq / (double)spectrogram.GetLength(1);

            // calculate the bin id of boundary between mid and low frequency spectrum
            int lowBinBound = (int)Math.Ceiling(lowFreqBound / binWidth);

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this iwll be less than 17640/2.
            int originalAudioNyquist = (int)analysisSettings.SampleRateOfOriginalAudioFile / 2; // original sample rate can be anything 11.0-44.1 kHz.
            if (recording.Nyquist > originalAudioNyquist)
            {
                nyquistFreq = originalAudioNyquist;
                nyquistBin = (int)Math.Floor(originalAudioNyquist / binWidth);
            }


            // vi: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            var subBandSpectrogram = MatrixTools.Submatrix(spectrogram, 0, lowBinBound, spectrogram.GetLength(0) - 1, nyquistBin);

            double[] aciArray = AcousticComplexityIndex.CalculateACI(subBandSpectrogram);
            double aci1 = aciArray.Average();


            // ii: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            var results3 = SNR.SubtractBackgroundNoiseFromWaveform_dB(SNR.Signal2Decibels(signalextract.Envelope), StandardDeviationCount);
            var dBarray = SNR.TruncateNegativeValues2Zero(results3.noiseReducedSignal);


            //// vii: remove background noise from the full spectrogram i.e. BIN 1 to Nyquist
            //spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, nyquistBin);
            //const double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            //double[] modalValues = SNR.CalculateModalValues(spectrogramData); // calculate modal value for each freq bin.
            //modalValues = DataTools.filterMovingAverage(modalValues, 7);      // smooth the modal profile
            //spectrogramData = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogramData, modalValues);
            //spectrogramData = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogramData, SpectralBgThreshold);

            //set up the output
            if (verbose)
                LoggedConsole.WriteLine("{0:d2}, {1},  {2},    {3},    {4},    {5},   {6},     {7},     {8},    {9},   {10},   {11}", "start", "end", "avDB", "BG", "SNR", "act", "spik", "lf", "mf", "hf", "H[t]", "H[s]", "index1", "index2");
            StringBuilder sb =  null;
            if (writeOutputFile)
            {
                string header = string.Format("{0:d2},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", "start", "end", "avDB", "BG", "SNR", "act", "spik", "lf", "mf", "hf", "H[t]", "H[s]", "index1", "index2");
                sb = new StringBuilder(header+"\n");
            }

            DataTable dt = GetIndices(envelope, audioDuration, frameDuration, spectrogram, lowFreqBound, midFreqBound, binWidth);
            return System.Tuple.Create(dt, audioDuration);
        } //Analysis()

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
            for (int i=0; i<chunkCount; i++)
            {
                int startSecond = i * chunkDuration;
                int start = (int)(startSecond * framesPerSecond);
                int end   = start + framesPerChunk;
                if (end >= signal.Length) end = signal.Length - 1;
                double[] chunkSignal = DataTools.Subarray(signal, start, framesPerChunk);
                if (chunkSignal.Length < 50) continue;  //an arbitrary minimum length
                double[,] chunkSpectro = DataTools.Submatrix(spectrogram, start, 1, end, nyquistBin-1);

                Indices rainIndices = Get10SecondIndices(chunkSignal, chunkSpectro, lowFreqBound, midFreqBound, binWidth);
                string classification = ConvertAcousticIndices2Classifcations(rainIndices);
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
        public static Indices Get10SecondIndices(double[] signal, double[,] spectrogram, int lowFreqBound, int midFreqBound, double binWidth)   
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
            double spikeIndex = Rain.CalculateSpikeIndex(signal, spikeThreshold);
            //Console.WriteLine("spikeIndex=" + spikeIndex);
            //DataTools.writeBarGraph(signal);

            Indices rainIndices; // struct in which to store all indices
            rainIndices.activity = activeFrameCount / (double)dBarray.Length;  //fraction of frames having acoustic activity 
            rainIndices.bgNoise  = results3.NoiseMode;                         //bg noise in dB
            rainIndices.snr      = results3.Snr;                               //snr
            rainIndices.avSig_dB = 20 * Math.Log10(signal.Average());        //10 times log of amplitude squared 
            rainIndices.temporalEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(signal)); //ENTROPY of ENERGY ENVELOPE
            rainIndices.spikes = spikeIndex;

            // ii: calculate the bin id of boundary between mid and low frequency spectrum
            int lowBinBound        = (int)Math.Ceiling(lowFreqBound / binWidth);
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
            rainIndices.hiFreqCover  = tuple3.Item3;
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
        }


        /// <summary>
        /// The values in this class were derived from See5 runs data extracted from 
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static string ConvertAcousticIndices2Classifcations(Indices indices)
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
                if (indices.bgNoise > -24)          return header_cicada;
            }
            return classification;
        }

        public static DataTable ConvertClassifcations2Datatable(string[] classifications)
        {
            string[] headers = { Keys.INDICES_COUNT, Keys.START_MIN, Keys.SEGMENT_TIMESPAN, header_rain,    header_cicada };
            Type[] types     = { typeof(int),        typeof(double),     typeof(double),    typeof(double), typeof(double) };

            int length = classifications.Length;
            int rainCount = 0;
            int cicadaCount = 0;
            for (int i = 0; i < length; i++)
            {
                if(classifications[i] == header_rain)   rainCount++;
                if(classifications[i] == header_cicada) cicadaCount++;
            }

            var dt = DataTableTools.CreateTable(headers, types);
            dt.Rows.Add(0, 0.0, 0.0,  //add dummy values to the first three columns. These will be entered later.
                        (rainCount/(double)length), (cicadaCount/(double)length)
                        );
            return dt;
        }

        //private static System.Tuple<string[], Type[], bool[]> InitOutputTableColumns()
        //{
        //    HEADERS[0] = header_count;    COL_TYPES[0] = typeof(int); DISPLAY_COLUMN[0] = false; COMBO_WEIGHTS[0] = 0.0;
        //    HEADERS[1] = header_startMin; COL_TYPES[1] = typeof(double); DISPLAY_COLUMN[1] = false; COMBO_WEIGHTS[1] = 0.0;
        //    HEADERS[2] = header_SecondsDuration; COL_TYPES[2] = typeof(double); DISPLAY_COLUMN[2] = false; COMBO_WEIGHTS[2] = 0.0;
        //    HEADERS[3] = header_avAmpdB; COL_TYPES[3] = typeof(double); DISPLAY_COLUMN[3] = true; COMBO_WEIGHTS[3] = 0.0;
        //    HEADERS[4] = header_snrdB; COL_TYPES[4] = typeof(double); DISPLAY_COLUMN[4] = true; COMBO_WEIGHTS[4] = 0.0;
        //    HEADERS[5] = header_bgdB; COL_TYPES[5] = typeof(double); DISPLAY_COLUMN[5] = true; COMBO_WEIGHTS[5] = 0.0;
        //    HEADERS[6] = header_activity; COL_TYPES[6] = typeof(double); DISPLAY_COLUMN[6] = true; COMBO_WEIGHTS[6] = 0.0;
        //    HEADERS[7] = header_hfCover; COL_TYPES[7] = typeof(double); DISPLAY_COLUMN[7] = true; COMBO_WEIGHTS[7] = 0.0;
        //    HEADERS[8] = header_mfCover; COL_TYPES[8] = typeof(double); DISPLAY_COLUMN[8] = true; COMBO_WEIGHTS[8] = 0.0;
        //    HEADERS[9] = header_lfCover; COL_TYPES[9] = typeof(double); DISPLAY_COLUMN[9] = true; COMBO_WEIGHTS[9] = 0.0;
        //    HEADERS[10] = header_HAmpl; COL_TYPES[10] = typeof(double); DISPLAY_COLUMN[10] = true; COMBO_WEIGHTS[10] = 0.0;
        //    HEADERS[11] = header_HAvSpectrum; COL_TYPES[11] = typeof(double); DISPLAY_COLUMN[11] = true; COMBO_WEIGHTS[11] = 0.4;
        //    //HEADERS[12] = header_HVarSpectrum; COL_TYPES[12] = typeof(double); DISPLAY_COLUMN[12] = false; COMBO_WEIGHTS[12] = 0.1;
        //    return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        //}

        static Image DrawSonogram(BaseSonogram sonogram, List<Plot> scores)
        {
            Dictionary<string, string> configDict = new Dictionary<string,string>();
            List<AcousticEvent> predictedEvents = null; 
            double eventThreshold = 0.0;
            Image image = SpectrogramTools.Sonogram2Image(sonogram, configDict, null, scores, predictedEvents, eventThreshold);
            return image;
        } //DrawSonogram()

        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
            if ((dt == null) || (dt.Rows.Count == 0)) return null;
            //get its column headers
            var dtHeaders = new List<string>();
            var dtTypes = new List<Type>();
            foreach (DataColumn col in dt.Columns)
            {
                dtHeaders.Add(col.ColumnName);
                dtTypes.Add(col.DataType);
            }

            List<string> displayHeaders = null;
            //check if config file contains list of display headers
            if (fiConfigFile != null)
            {
                var configuration = new ConfigDictionary(fiConfigFile.FullName);
                Dictionary<string, string> configDict = configuration.GetTable();
                if (configDict.ContainsKey(Keys.DISPLAY_COLUMNS))
                    displayHeaders = configDict[Keys.DISPLAY_COLUMNS].Split(',').ToList();
            }
            //if config file does not exist or does not contain display headers then use the original headers
            if (displayHeaders == null) displayHeaders = dtHeaders; //use existing headers if user supplies none.

            //now determine how to display tracks in display datatable
            Type[] displayTypes = new Type[displayHeaders.Count];
            bool[] canDisplay = new bool[displayHeaders.Count];
            for (int i = 0; i < displayTypes.Length; i++)
            {
                displayTypes[i] = typeof(double);
                canDisplay[i] = false;
                if (dtHeaders.Contains(displayHeaders[i])) canDisplay[i] = true;
            }

            DataTable table2Display = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes);
            foreach (DataRow row in dt.Rows)
            {
                DataRow newRow = table2Display.NewRow();
                for (int i = 0; i < canDisplay.Length; i++)
                {
                    if (canDisplay[i]) newRow[displayHeaders[i]] = row[displayHeaders[i]];
                    else newRow[displayHeaders[i]] = 0.0;
                }
                table2Display.Rows.Add(newRow);
            }

            //order the table if possible
            if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_START_ABS))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_START_ABS + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.EVENT_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.EVENT_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.INDICES_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.INDICES_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.Keys.START_MIN))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.Keys.START_MIN + " ASC");
            }

            table2Display = NormaliseColumnsOfDataTable(table2Display);

            //add in column of weighted indices
            bool addColumnOfweightedIndices = true;
            if (addColumnOfweightedIndices)
            {
                double[] comboWts = IndexCalculate.CalculateComboWeights();
                double[] weightedIndices = IndexCalculate.GetArrayOfWeightedAcousticIndices(dt, comboWts);
                string colName = "WeightedIndex";
                DataTableTools.AddColumnOfDoubles2Table(table2Display, colName, weightedIndices);
            }
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()

        /// <summary>
        /// takes a data table of indices and normalises column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable NormaliseColumnValuesOfDatatable(DataTable dt)
        {
            string[] headers = DataTableTools.GetColumnNames(dt);
            string[] newHeaders = new string[headers.Length];

            List<double[]> newColumns = new List<double[]>();

            for (int i = 0; i < headers.Length; i++)
            {
                double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
                if ((values == null) || (values.Length == 0)) continue;

                double min = 0;
                double max = 1;
                if (headers[i].Equals(Keys.AV_AMPLITUDE))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else //default is to normalise in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
                    newHeaders[i] = headers[i];
                }
            }

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Length];
            for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);

            return processedtable;
        }

        /// <summary>
        ///// takes a data table of indices and converts column values to values in [0,1].
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <returns></returns>
        //public static DataTable ProcessDataTableForDisplayOfColumnValues(DataTable dt, List<string> headers2Display)
        //{
        //    string[] headers = DataTableTools.GetColumnNames(dt);
        //    List<string> originalHeaderList = headers.ToList();
        //    List<string> newHeaders = new List<string>();
        //    List<double[]> newColumns = new List<double[]>();
        //    // double[] processedColumn = null;
        //    for (int i = 0; i < headers2Display.Count; i++)
        //    {
        //        string header = headers2Display[i];
        //        if (!originalHeaderList.Contains(header)) continue;
        //        double[] values = DataTableTools.Column2ArrayOfDouble(dt, header); //get list of values
        //        if ((values == null) || (values.Length == 0)) continue;
        //        double min = 0;
        //        double max = 1;
        //        if (header.Equals(AcousticFeatures.header_avAmpdB))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_avAmpdB + "  (-50..-5dB)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_snrdB))
        //        {
        //            min = 5;
        //            max = 50;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_snrdB + "  (5..50dB)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_avSegDur))
        //        {
        //            min = 0.0;
        //            max = 500.0; //av segment duration in milliseconds
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_avSegDur + "  (0..500ms)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_bgdB))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_bgdB + "  (-50..-5dB)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_avClustDur))
        //        {
        //            min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
        //            max = 200.0; //av segment duration in milliseconds
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_avClustDur + "  (50..200ms)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_lfCover))
        //        {
        //            min = 0.1; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_lfCover + "  (10..100%)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_mfCover))
        //        {
        //            min = 0.0; //
        //            max = 0.9; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_mfCover + "  (0..90%)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_hfCover))
        //        {
        //            min = 0.0; //
        //            max = 0.9; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_hfCover + "  (0..90%)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_HAmpl))
        //        {
        //            min = 0.5; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_HAmpl + "  (0.5..1.0)");
        //        }
        //        else if (header.Equals(AcousticFeatures.header_HAvSpectrum))
        //        {
        //            min = 0.2; //
        //            max = 1.0; //
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders.Add(AcousticFeatures.header_HAvSpectrum + "  (0.2..1.0)");
        //        }
        //        else //default is to normalise in [0,1]
        //        {
        //            newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
        //            newHeaders.Add(header);
        //        }
        //    }
        //    //convert type int to type double due to normalisation
        //    Type[] types = new Type[newHeaders.Count];
        //    for (int i = 0; i < newHeaders.Count; i++) types[i] = typeof(double);
        //    var processedtable = DataTableTools.CreateTable(newHeaders.ToArray(), types, newColumns);
        //    return processedtable;
        //}

        /// <summary>
        /// takes a data table of indices and converts column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable NormaliseColumnsOfDataTable(DataTable dt)
        {
            string[] headers = DataTableTools.GetColumnNames(dt);
            string[] newHeaders = new string[headers.Length];

            List<double[]> newColumns = new List<double[]>();

            for (int i = 0; i < headers.Length; i++)
            {
                double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
                if ((values == null) || (values.Length == 0)) continue;

                double min = 0;
                double max = 1;
                if (headers[i].Equals(IndexProperties.header_avAmpdB))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else if (headers[i].Equals(IndexProperties.header_snr))
                {
                    min = 5;
                    max = 50;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (5..50dB)";
                }
                else if (headers[i].Equals(IndexProperties.header_avSegDur))
                {
                    min = 0.0;
                    max = 500.0; //av segment duration in milliseconds
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..500ms)";
                }
                else if (headers[i].Equals(IndexProperties.header_bgdB))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else if (headers[i].Equals(IndexProperties.header_avClustDuration))
                {
                    min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
                    max = 200.0; //av segment duration in milliseconds
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (50..200ms)";
                }
                else if (headers[i].Equals(IndexProperties.header_lfCover))
                {
                    min = 0.1; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (10..100%)";
                }
                else if (headers[i].Equals(IndexProperties.header_mfCover))
                {
                    min = 0.0; //
                    max = 0.9; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..90%)";
                }
                else if (headers[i].Equals(IndexProperties.header_hfCover))
                {
                    min = 0.0; //
                    max = 0.9; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0..90%)";
                }
                else if (headers[i].Equals(IndexProperties.header_HAmpl))
                {
                    min = 0.5; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0.5..1.0)";
                }
                else if (headers[i].Equals(IndexProperties.header_HAvSpectrum))
                {
                    min = 0.2; //
                    max = 1.0; //
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (0.2..1.0)";
                }
                else //default is to normalise in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
                    newHeaders[i] = headers[i];
                }
            } //for loop

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Length];
            for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
            return processedtable;
        }


        public static void WriteSee5DataFiles(DataTable dt, DirectoryInfo diOutputDir, string fileStem)
        {
            string namesFilePath = Path.Combine(diOutputDir.FullName, fileStem + ".See5.names");
            string dataFilePath  = Path.Combine(diOutputDir.FullName, fileStem + ".See5.data");

            string class1Name = "none";
            string class2Name = "cicada";
            string class3Name = "rain";
            //string class4Name = "koala";
            //string class5Name = "mobile";

            var nameContent = new List<string>();
            nameContent.Add("|   THESE ARE THE CLASS NAMES FOR RAIN Classification.");
            nameContent.Add(string.Format("{0},  {1},  {2}", class1Name, class2Name, class3Name));
            //nameContent.Add(String.Format("{0},  {1},  {2},  {3},  {4}", class1Name, class2Name, class3Name, class4Name, class5Name));
            nameContent.Add("|   THESE ARE THE ATTRIBUTE NAMES FOR RAIN Classification.");
            //nameContent.Add(String.Format("{0}: ignore", "start"));
            //nameContent.Add(String.Format("{0}: ignore", "end"));
            nameContent.Add(string.Format("{0}: ignore", "avDB"));
            nameContent.Add(string.Format("{0}: continuous", "BG"));
            nameContent.Add(string.Format("{0}: continuous", "SNR"));
            nameContent.Add(string.Format("{0}: continuous", "activity"));
            nameContent.Add(string.Format("{0}: continuous", "spikes"));
            nameContent.Add(string.Format("{0}: continuous", "lf"));
            nameContent.Add(string.Format("{0}: continuous", "mf"));
            nameContent.Add(string.Format("{0}: continuous", "hf"));
            nameContent.Add(string.Format("{0}: continuous", "H[t]"));
            nameContent.Add(string.Format("{0}: continuous", "H[s]"));
            //nameContent.Add(String.Format("{0}: ignore",     "class"));
            FileTools.WriteTextFile(namesFilePath, nameContent);


            var dataContent = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                double avDB = (double)row["avDB"];
                double BG   = (double)row["BG"];
                double SNR  = (double)row["SNR"];
                double activity = (double)row["activity"];
                double spikes = (double)row["spikes"];
                double lf = (double)row["lf"];
                double mf = (double)row["mf"]; //average peak
                double hf = (double)row["hf"];
                double H_t = (double)row["H[t]"];
                double H_s = (double)row["H[s]"];
                string name = (string)row["class"];

                string line = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}", avDB, BG, SNR, activity, spikes, lf, mf, hf, H_t, H_s, name);
                dataContent.Add(line);
            }
            FileTools.WriteTextFile(dataFilePath, dataContent);
        }


        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            return null;
        }



        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }

        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
                };
            }
        }
    }
}
