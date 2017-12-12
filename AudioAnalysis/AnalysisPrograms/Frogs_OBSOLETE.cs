namespace AnalysisPrograms
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using AnalysisBase;
    using AnalysisBase.Extensions;
    using AnalysisBase.ResultBases;
    using Production;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    [Obsolete("This code most likely does not work. It should be ported to the new recognizer code base immediately.")]
    public class Frogs_OBSOLETE : AbstractStrongAnalyser
    {

        public class Arguments : AnalyserArguments
        {
        }

        public static string key_FROG_DATA = "FROG_DATA_FILE";

        //OTHER CONSTANTS
        public const string AnalysisName = "Frogs";
        public const int ResampleRate = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";

        public override string DisplayName
        {
            get { return AnalysisName; }
        }

        public override string Identifier
        {
            get { return "Towsey." + AnalysisName; }
        }

        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]
        public static void Dev(Arguments arguments)
        {
            Log.Verbosity = 1;
            bool debug = MainEntry.InDEBUG;

            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav";  //POSITIVE
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Adelotus_brevis_TuskedFrog_BridgeCreek.wav";   // NEGATIVE walking on dry leaves
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min646.wav";   //NEGATIVE  rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min599.wav";   //NEGATIVE  rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Rain\DM420036_min602.wav";   //NEGATIVE  rain
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Noise\BAC3_20070924-153657_noise.wav";               // NEGATIVE  noise
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\FrogPond_Samford_SE_555_20101023-000000.mp3";  // FROGs AT SAMFORD
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Crinia_signifera_july08.wav";                  // Crinia signifera
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Frogs_BridgeCreek_Night_Extract1-31-00.mp3";   // FROGs at Bridgecreek

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Compilation6_Mono.mp3";                          // FROG COMPILATION
                string recordingPath = @"C:\SensorNetworks\WavFiles\Frogs\Curramore\CurramoreSelection-mono16kHz.mp3";
                    // Curramore COMPILATION

                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Frogs.cfg";

                string outputDir = @"C:\SensorNetworks\Output\Frogs\";

                // example
                // "C:\SensorNetworks\WavFiles\Frogs\Rheobatrachus_silus_MONO.wav" C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.RheobatrachusSilus.cfg" "C:\SensorNetworks\Output\Frogs\"

                const int StartMinute = 0;
                //int startMinute = 1;
                const int DurationSeconds = 60; //set zero to get entire recording
                var tsStart = new TimeSpan(0, StartMinute, 0); //hours, minutes, seconds
                var tsDuration = new TimeSpan(0, 0, DurationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(arguments.Source.Name);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, StartMinute);
                var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, StartMinute);
                var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, StartMinute, "Towsey." + AnalysisName);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, StartMinute, "Towsey." + AnalysisName);

                arguments = new Arguments
                {
                    Source = recordingPath.ToFileInfo(),
                    Config = configPath.ToFileInfo(),
                    Output = outputDir.ToDirectoryInfo(),
                    Start = tsStart.TotalSeconds,
                    Duration = tsDuration.TotalSeconds,
                };
            }

            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("# FOR DETECTION OF 'FROG SPECIES' ");
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Output folder:  " + arguments.Output);
            LoggedConsole.WriteLine("# Recording file: " + arguments.Source.Name);
            var diOutputDir = arguments.Output;

            Execute(arguments);

        }

        /// <summary>
        /// A WRAPPER AROUND THE Analysis() METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        public static void Execute(Arguments arguments)
        {
            Contract.Requires(arguments != null);

            TimeSpan tsStart = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan tsDuration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            string outputDir = arguments.Output.FullName;

            // EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo sourceF = arguments.Source;
            FileInfo tempF = TempFileHelper.NewTempFile(arguments.Output);
            if (tempF.Exists) { tempF.Delete(); }

            // GET INFO ABOUT THE SOURCE and the TARGET files - esp need the sampling rate
            AudioUtilityModifiedInfo preparedFile;

            if (tsDuration == TimeSpan.Zero)  // Process entire file
            {
                preparedFile = AudioFilePreparer.PrepareFile(sourceF, tempF, new AudioUtilityRequest { TargetSampleRate = ResampleRate }, arguments.Output);
            }
            else
            {
                preparedFile = AudioFilePreparer.PrepareFile(sourceF, tempF, new AudioUtilityRequest { TargetSampleRate = ResampleRate, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) }, arguments.Output);
            }

            var (analysisSettings, segmentSettings) = arguments.ToAnalysisSettings(
                sourceSegment: preparedFile.SourceInfo.ToSegment(),
                preparedSegment: preparedFile.TargetInfo.ToSegment());

            //get the data file to identify frog calls. Check it exists and then store full path in dictionary.
            string frogParametersPath = analysisSettings.ConfigDict[key_FROG_DATA];
            FileInfo fi_FrogData = new FileInfo(Path.Combine(outputDir, frogParametersPath));

            if (!fi_FrogData.Exists)
            {
                LoggedConsole.WriteLine("INVALID PATH: " + fi_FrogData.FullName);
                LoggedConsole.WriteLine("The config file must contain the name of a valid .csv file (containing frog call parameters) located in same directory as the .cfg file.");
                LoggedConsole.WriteLine("For example, use Key/Value pair:  FROG_DATA_FILE=FrogDataAndCompilationFile.csv");
                throw new InvalidOperationException();
            }
            analysisSettings.ConfigDict[key_FROG_DATA] = fi_FrogData.FullName; // store full path in the dictionary.

            // DO THE ANALYSIS
            // #############################################################################################################################################
            IAnalyser2 analyser = new Frogs_OBSOLETE();
            AnalysisResult2 result = analyser.Analyze(analysisSettings, segmentSettings);

            throw new NotImplementedException("Intentionally broken");
            /*
            DataTable dt = result.Data;
            if (dt == null) { throw new InvalidOperationException("Data table of results is null"); }
            // #############################################################################################################################################

            // ADD IN ADDITIONAL INFO TO RESULTS TABLE
            AddContext2Table(dt, tsStart, result.AudioDuration);
            CsvTools.DataTable2CSV(dt, segmentSettings.SegmentEventsFile.FullName);
            // DataTableTools.WriteTable(augmentedTable);
            */
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var fiAudioF    = segmentSettings.SegmentAudioFile;
            var diOutputDir = segmentSettings.SegmentOutputDirectory;

            //######################################################################
            var results = Analysis(fiAudioF, analysisSettings, segmentSettings.Segment.SourceMetadata.SampleRate, segmentSettings.SegmentStartOffset);
            //######################################################################

            if (results == null) return null; //nothing to process (broken)
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;

            var result = new AnalysisResult2(analysisSettings, segmentSettings, recordingTimeSpan);
            result.AnalysisIdentifier = this.Identifier;
            result.MiscellaneousResults["dataTable"] = null;

            DataTable dataTable = null;

            if (predictedEvents != null)
            {
                string analysisName = analysisSettings.ConfigDict[AnalysisKeys.AnalysisName];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.FileName = fName;
                    //ev.Name = analysisName; //TEMPORARY DISABLE
                    ev.SegmentDurationSeconds = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = AnalysisKeys.EventStartAbs + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                CsvTools.DataTable2CSV(dataTable, segmentSettings.SegmentEventsFile.FullName);
            }
            else
                result.EventsFile = null;

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                double scoreThreshold = 0.01;
                if (analysisSettings.ConfigDict.ContainsKey(AnalysisKeys.IntensityThreshold))
                    scoreThreshold = ConfigDictionary.GetDouble(AnalysisKeys.IntensityThreshold, analysisSettings.ConfigDict);
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = this.ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, segmentSettings.SegmentSummaryIndicesFile.FullName);
            }
            else
                result.SummaryIndices = null;

            //save image of sonograms
            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(predictedEvents.Count))
            {
                string imagePath = segmentSettings.SegmentImageFile.FullName;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents);
                image.Save(imagePath, ImageFormat.Png);
            }

            result.MiscellaneousResults["dataTable"] = dataTable;
            result.ImageFile = segmentSettings.SegmentImageFile;

            //result.DisplayItems = { { 0, "example" }, { 1, "example 2" }, }
            //result.OutputFiles = { { "exmaple file key", new FileInfo("Where's that file?") } }
            return result;
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
            // noop
        }
//Analyze()

        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// Returns a DataTable
        /// </summary>
        /// <param name="fiSegmentOfSourceFile"></param>
        /// <param name="analysisSettings"></param>
        /// <param name="originalSampleRate"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="configDict"></param>
        /// <param name="diOutputDir"></param>
        public static Tuple<BaseSonogram, double[,], List<Plot>, List<AcousticEvent>, TimeSpan> Analysis(FileInfo fiSegmentOfSourceFile, AnalysisSettings analysisSettings, int originalSampleRate, TimeSpan segmentStartOffset)
        {
            Dictionary<string, string> configDict = analysisSettings.ConfigDict;
            int originalAudioNyquist = originalSampleRate / 2; // original sample rate can be anything 11.0-44.1 kHz.

            //set default values - ignore those set by user
            int frameSize = 32;
            double windowOverlap   = 0.3;
            int xCorrelationLength = 256;   //for Xcorrelation   - 256 frames @801 = 320ms, almost 1/3 second.
            //int xCorrelationLength = 128;   //for Xcorrelation   - 128 frames @801 = 160ms, almost 1/6 second.
            //int xCorrelationLength = 64;   //for Xcorrelation   - 64 frames @128 = 232ms, almost 1/4 second.
            //int xCorrelationLength = 16;   //for Xcorrelation   - 16 frames @128 = 232ms, almost 1/4 second.
            double dBThreshold = 12.0;

            // read frog data to datatable
            var dt = CsvTools.ReadCSVToTable(configDict[key_FROG_DATA], true); // read file contining parameters of frog calls to a table

            double intensityThreshold = double.Parse(configDict[AnalysisKeys.IntensityThreshold]); //in 0-1
            double minDuration = double.Parse(configDict[AnalysisKeys.MinDuration]);     // seconds
            double maxDuration = double.Parse(configDict[AnalysisKeys.MaxDuration]);     // seconds
            double minPeriod   = double.Parse(configDict[AnalysisKeys.MinPeriodicity]);  // seconds
            double maxPeriod   = double.Parse(configDict[AnalysisKeys.MaxPeriodicity]);  // seconds

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.BaseName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            sonoConfig.NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD");   //must do noise removal
            TimeSpan tsRecordingtDuration = recording.Duration;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double frameOffset = sonoConfig.GetFrameOffset(sr);
            double framesPerSecond = 1 / frameOffset;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            //iii: GET TRACKS
            int nhLimit = 3; //limit of neighbourhood around maximum
            var peaks = DataTools.GetPeakValues(sonogram.DecibelsPerFrame);
            var tuple = SpectralTrack.GetSpectralMaxima(sonogram.DecibelsPerFrame, sonogram.Data, dBThreshold, nhLimit);
            var maxFreqArray = tuple.Item1; //array (one element per frame) indicating which freq bin has max amplitude.
            var hitsMatrix   = tuple.Item2;
            int herzOffset = 0;
            int maxFreq = 6000;
            var tracks = SpectralTrack.GetSpectralTracks(maxFreqArray, framesPerSecond, freqBinWidth, herzOffset, SpectralTrack.MIN_TRACK_DURATION, SpectralTrack.MAX_INTRASYLLABLE_GAP, maxFreq);

            double severity = 0.5;
            double dynamicRange = 60; // deciBels above background noise. BG noise has already been removed from each bin.
            // convert sonogram to a list of frequency bin arrays
            var listOfFrequencyBins = SpectrogramTools.Sonogram2ListOfFreqBinArrays(sonogram, dynamicRange);
            int minFrameLength = SpectralTrack.FrameCountEquivalent(SpectralTrack.MIN_TRACK_DURATION, framesPerSecond);

            for (int i = tracks.Count-1; i >= 0; i--)
            {
                tracks[i].CropTrack(listOfFrequencyBins, severity);
                if (tracks[i].Length < minFrameLength) tracks.Remove(tracks[i]);
            } // foreach track

            foreach (SpectralTrack track in tracks) // find any periodicity in the track and calculate its score.
            {
                SpectralTrack.DetectTrackPeriodicity(track, xCorrelationLength, listOfFrequencyBins, sonogram.FramesPerSecond);
            } // foreach track

            int rowCount = sonogram.Data.GetLength(0);
            int MAX_FREQ_BOUND = 6000;
            int topBin = (int)Math.Round(MAX_FREQ_BOUND / freqBinWidth);
            var plots = CreateScorePlots(tracks, rowCount, topBin);

            //iv: CONVERT TRACKS TO ACOUSTIC EVENTS
            List<AcousticEvent> frogEvents = SpectralTrack.ConvertTracks2Events(tracks, segmentStartOffset);

            // v: GET FROG IDs
            //var frogEvents = new List<AcousticEvent>();
            foreach (AcousticEvent ae in frogEvents)
            {
                double oscRate = 1 / ae.Periodicity;
                // ae.DominantFreq
                // ae.Score
                // ae.Duration
                //ClassifyFrogEvent(ae);
                string[] names = ClassifyFrogEvent(ae.DominantFreq, oscRate, dt);
                ae.Name = names[0];
                ae.Name2 = names[1];
            }

            return Tuple.Create(sonogram, hitsMatrix, plots, frogEvents, tsRecordingtDuration);
        } //Analysis()

        /// <summary>
        /// Given the passed feature values (freq and oscRate) calculate p(Data|h[i]) for all hypotheses indexed by i.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="oscRate"></param>
        /// <param name="frogDataTable"></param>
        /// <returns></returns>
        public static string[] ClassifyFrogEvent(double freq, double oscRate, DataTable frogDataTable)
        {
            int rowCount = frogDataTable.Rows.Count;

            List<double> data = new List<double>();
            data.Add(freq);
            data.Add(oscRate);

            double[] probScore = new double[rowCount];

            for (int i = 0; i < rowCount; i++) // all rows in table = all frog hypotheses
            {
                DataRow row = frogDataTable.Rows[i];

                List<double> targets = new List<double>();
                targets.Add((double)((int)row["DominantFreq-Hz"]));
                targets.Add((double)((int)row["OscRate-cyclesPerSec"]));

                probScore[i] = GetNaiveBayesScore(targets.ToArray(), data.ToArray());
            }

            int id = DataTools.GetMaxIndex(probScore);
            DataRow row1 = frogDataTable.Rows[id];
            string[] names = new string[2];
            names[0] = (string)row1["LatinName"];
            names[1] = (string)row1["CommonName"];

            return names;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double GetNaiveBayesScore(double[] targetMeans, double[] data)
        {
            // ASSUME that one SD of the distribution feature values = 1/10th of the mean.
            double SD_FACTOR = 0.1;
            double dataProb = 1.0;
            for (int i = 0; i < data.Length; i++)
            {
                double targetSD = targetMeans[i] * SD_FACTOR;
                double featureZScore = (data[i] - targetMeans[i]) / targetSD;
                double featureProb = NormalDist.zScore2pValue(Math.Abs(featureZScore));
                dataProb *= featureProb;
            }
            return dataProb;
        }

        public static void ClassifyFrogEvent(AcousticEvent ae)
        {
            double oscRate = 1 / ae.Periodicity;

            if ((ae.DominantFreq > 3350) && (ae.DominantFreq < 3550) && (oscRate > 160) && (oscRate < 190) && (ae.Score > 0.20) && (ae.EventDurationSeconds > 0.05) && (ae.EventDurationSeconds < 0.3))
            {
                ae.Name = "Assa darlingtoni";
            }
            else
            if ((ae.DominantFreq > 4600) && (ae.DominantFreq < 4900) && (oscRate > 15) && (oscRate < 25) && (ae.Score > 0.20) && (ae.EventDurationSeconds > 0.05) && (ae.EventDurationSeconds < 0.5))
            {
                ae.Name = "Crinia deserticola";
            }
            else
            if ((ae.DominantFreq > 2600) && (ae.DominantFreq < 2800) && (oscRate > 20) && (oscRate < 30) && (ae.Score > 0.50) && (ae.EventDurationSeconds > 0.05) && (ae.EventDurationSeconds < 0.5))
            {
                ae.Name = "Crinia signifera";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2300) && (oscRate > 145) && (oscRate < 165) && (ae.Score > 0.50) && (ae.EventDurationSeconds > 0.5))
            {
                ae.Name = "Cyclorana brevipes";
            }
            else
            if ((ae.DominantFreq > 600) && (ae.DominantFreq < 700) && (oscRate > 10) && (oscRate < 14) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.3))
            {
                    ae.Name = "Heleioporus australiacus";
            }
            else
            if ((ae.DominantFreq > 1350) && (ae.DominantFreq < 1650) && (oscRate > 40) && (oscRate < 80) && (ae.Score > 0.3) && (ae.EventDurationSeconds > 0.3))
            {
                ae.Name = "GBH"; // the oscillation rate of the GBF increases from slow to fast, 30 - 80
            }
            else
            if ((ae.DominantFreq > 650) && (ae.DominantFreq < 750) && (oscRate > 13) && (oscRate < 15) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.3))
            {
                ae.Name = "Lechriodus fletcheri";
            }
            else
            if ((ae.DominantFreq > 1400) && (ae.DominantFreq < 1700) && (oscRate > 68) && (oscRate < 88) && (ae.Score > 0.15) && (ae.EventDurationSeconds > 0.1))
            {
                ae.Name = "Limnodynastes fletcheri";
            }
            else
            if ((ae.DominantFreq > 1600) && (ae.DominantFreq < 1900) && (oscRate > 14) && (oscRate < 20) && (ae.Score > 0.50) && (ae.EventDurationSeconds > 0.1) && (ae.EventDurationSeconds < 0.5))
            {
                ae.Name = "Limnodynastes tasmaniensis";
            }
            else
            if ((ae.DominantFreq > 1100) && (ae.DominantFreq < 1500) && (oscRate > 110) && (oscRate < 150) && (ae.Score > 0.3) && (ae.EventDurationSeconds > 0.2) && (ae.EventDurationSeconds < 1.0))
            {
                ae.Name = "Litoria aurea";
            }
            else
            if ((ae.DominantFreq > 2900) && (ae.DominantFreq < 3200) && (oscRate > 9) && (oscRate < 12) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.2) && (ae.EventDurationSeconds < 1.5))
            {
                ae.Name = "Litoria brevipalmata";
            }
            else
            if ((ae.DominantFreq > 1500) && (ae.DominantFreq < 1700) && (oscRate > 35) && (oscRate < 55) && (ae.Score > 0.50) && (ae.EventDurationSeconds > 0.2) && (ae.EventDurationSeconds < 1.5))
            {
                ae.Name = "Litoria citropa";
            }
            else
            if ((ae.DominantFreq > 2700) && (ae.DominantFreq < 3000) && (oscRate > 90) && (oscRate < 120) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.2) && (ae.EventDurationSeconds < 1.0))
            {
                ae.Name = "Litoria gracilenta";
            }
            else
            if ((ae.DominantFreq > 960) && (ae.DominantFreq < 1300) && (oscRate > 10) && (oscRate < 16) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.1))
            {
                ae.Name = "Litoria lesueuri";
            }
            else
            if ((ae.DominantFreq > 2050) && (ae.DominantFreq < 2200) && (oscRate > 35) && (oscRate < 45) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.1) && (ae.EventDurationSeconds < 0.5))
            {
                ae.Name = "Litoria littlejohni";
            }
            else
            if ((ae.DominantFreq > 2650) && (ae.DominantFreq < 2950) && (oscRate > 14) && (oscRate < 20) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.1) && (ae.EventDurationSeconds < 1.5))
            {
                ae.Name = "Litoria olongburensis";
            }
            else
            if ((ae.DominantFreq > 1750) && (ae.DominantFreq < 2050) && (oscRate > 15) && (oscRate < 25) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.5) && (ae.EventDurationSeconds < 2.0))
            {
                ae.Name = "Litoria peronii";
            }
            else
            if ((ae.DominantFreq > 800) && (ae.DominantFreq < 1100) && (oscRate > 62) && (oscRate < 82) && (ae.Score > 0.50) && (ae.EventDurationSeconds > 0.3))
            {
                ae.Name = "Mixophyes fleayi";
            }
            else
            if ((ae.DominantFreq > 900) && (ae.DominantFreq < 1200) && (oscRate > 125) && (oscRate < 145) && (ae.Score >= 0.20) )
            {
                ae.Name = "Mixophyes fasciolatus";
            }
            else
            if ((ae.DominantFreq > 700) && (ae.DominantFreq < 800) && (oscRate > 80) && (oscRate < 90) && (ae.Score > 0.20) && (ae.EventDurationSeconds > 0.2))
            {
                ae.Name = "Mixophyes iteratus";
            }
            else
            if ((ae.DominantFreq > 1350) && (ae.DominantFreq < 1550) && (oscRate > 19) && (oscRate < 23) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.1))
            {
                ae.Name = "Neobatrachus sudelli";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2400) && (oscRate > 90) && (oscRate < 120) && (ae.Score > 0.05) && (ae.EventDurationSeconds > 0.1))
            {
                ae.Name = "Paracrinia haswelli";
            }
            else
            if ((ae.DominantFreq > 450) && (ae.DominantFreq < 650) && (oscRate > 100) && (oscRate < 130) && (ae.Score > 0.5) && (ae.EventDurationSeconds > 0.01))
            {
                ae.Name = "Philoria kundagungan";
            }
            else
            if ((ae.DominantFreq > 400) && (ae.DominantFreq < 600) && (oscRate > 62) && (oscRate < 82) && (ae.Score > 0.5) && (ae.EventDurationSeconds > 0.1))
            {
                ae.Name = "Philoria loveridgei";
            }
            else
            if ((ae.DominantFreq > 960) && (ae.DominantFreq < 1250) && (oscRate > 20) && (oscRate < 45) && (ae.Score > 0.5) && (ae.EventDurationSeconds > 0.05))
            {
                ae.Name = "Philoria sphagnicolus";
            }
            else
            if ((ae.DominantFreq > 2650) && (ae.DominantFreq < 2950) && (oscRate > 67) && (oscRate < 85) && (ae.Score > 0.2) && (ae.EventDurationSeconds > 0.1))
            {
                ae.Name = "Pseudophryne australis";
            }
            else
            if ((ae.DominantFreq > 2300) && (ae.DominantFreq < 2600) && (oscRate > 45) && (oscRate < 55) && (ae.Score > 0.50) && (ae.EventDurationSeconds > 0.2))
            {
                ae.Name = "Pseudophryne coriacea";
            }
            else
            if ((ae.DominantFreq > 2400) && (ae.DominantFreq < 2700) && (oscRate > 40) && (oscRate < 50) && (ae.Score > 0.5) && (ae.EventDurationSeconds > 0.2))
            {
                ae.Name = "Pseudophryne raveni";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2400) && (oscRate > 35) && (oscRate < 48) && (ae.Score > 1.00) && (ae.EventDurationSeconds > 0.4))
            {
                ae.Name = "Uperoleia fusca";
            }
            else
            if ((ae.DominantFreq > 2300) && (ae.DominantFreq < 2500) && (oscRate > 135) && (oscRate < 155) && (ae.Score > 0.30) && (ae.EventDurationSeconds > 0.1) && (ae.EventDurationSeconds < 0.5))
            {
                ae.Name = "Uperoleia laevigata";
            }
            else
            if ((ae.DominantFreq > 2100) && (ae.DominantFreq < 2300) && (oscRate > 27) && (oscRate < 37) && (ae.Score > 0.30) && (ae.EventDurationSeconds < 0.2))
            {
                ae.Name = "Uperoleia rugosa";
            }
            else
            if ((ae.DominantFreq > 2700) && (ae.DominantFreq < 3000) && (oscRate > 80) && (oscRate < 100) && (ae.Score > 0.10) && (ae.EventDurationSeconds < 1.0))
            {
                ae.Name = "Uperoleia tyleri";
            }
        }

        public static List<Plot> CreateScorePlots(List<SpectralTrack> tracks, int rowCount, int topBin)
        {
            double herzPerBin = tracks[0].herzPerBin;
            //init score arrays
            var scores = new List<double[]>();
            for (int c = 1; c <= topBin; c++) //ignore DC bin
            {
                var array = new double[rowCount];
                scores.Add(array);
            }

            //add in track scores
            foreach (SpectralTrack track in tracks)
            {
                int sampleStart = track.StartFrame;
                int bin = (int)Math.Round(track.AverageBin) - 1; //get the tracks frequency i.e. bin number
                if(bin >= topBin) continue;
                int length = track.periodicity.Length;
                int periodicityStart = sampleStart + (track.Length/3);
                for (int r = 0; r < length; r++) // for each position in track
                {
                    scores[bin][periodicityStart + r] = track.periodicityScore[r];
                }
            }

            double intensityThreshold = 0.25;
            var plots = new List<Plot>();
            for (int c = 1; c < topBin; c++) //ignore DC bin
            {
                double[] filteredScores = DataTools.filterMovingAverage(scores[c-1], 3);
                filteredScores = DataTools.NormaliseInZeroOne(filteredScores, 0, 2.0); //## ABSOLUTE NORMALISATION 0-0.75 #####################################
                string title = ((int)Math.Round(c * herzPerBin)).ToString();
                plots.Add(new Plot(title + " Hz", filteredScores, intensityThreshold));
            }
            return plots;
        } // CreateScorePlots()

        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> plots, List<AcousticEvent> predictedEvents)
        {
            bool doHighlightSubband = false; bool add1kHzLines = false;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            if (plots != null)
                foreach (Plot plot in plots) image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title));
            //if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null) image.OverlayRedMatrix(hits, 1.0);
            if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            return image.GetImage();
        } //DrawSonogram()

        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AnalysisKeys.EventCount,        //1
                                 AnalysisKeys.EventStartMin,    //2
                                 AnalysisKeys.EventStartSec,    //3
                                 AnalysisKeys.EventStartAbs,    //4
                                 AnalysisKeys.KeySegmentDuration,   //5
                                 AnalysisKeys.EventDuration,     //6
                                 //AudioAnalysisTools.Keys.EVENT_INTENSITY,
                                 AnalysisKeys.EventName,         //7
                                 AnalysisKeys.DominantFrequency,
                                 AnalysisKeys.OscillationRate,
                                 AnalysisKeys.EventScore,
                                 AnalysisKeys.EventNormscore,

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(int), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), typeof(double),
                             typeof(double), typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AnalysisKeys.EventStartAbs] = (double)ev.TimeStart;  //Set now - will overwrite later
                row[AnalysisKeys.EventStartSec] = (double)ev.TimeStart;  //EvStartSec
                row[AnalysisKeys.EventDuration] = (double)ev.EventDurationSeconds;   //duration in seconds
                //row[AudioAnalysisTools.Keys.EVENT_INTENSITY] = (double)ev.kiwi_intensityScore;   //
                row[AnalysisKeys.EventName] = (string)ev.Name;   //
                row[AnalysisKeys.DominantFrequency] = (double)ev.DominantFreq;
                row[AnalysisKeys.OscillationRate] = 1 / (double)ev.Periodicity;
                row[AnalysisKeys.EventScore] = (double)ev.Score;      //Score
                row[AnalysisKeys.EventNormscore] = (double)ev.ScoreNormalised;
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        /// <summary>
        /// Converts a DataTable of events to a datatable where one row = one minute of indices
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan sourceDuration, double scoreThreshold)
        {
            double units = sourceDuration.TotalSeconds / unitTime.TotalSeconds;
            int unitCount = (int)(units / 1);   //get whole minutes
            if (units % 1 > 0.0) unitCount += 1; //add fractional minute
            int[] eventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] bigEvsPerUnitTime = new int[unitCount]; //to store counts of high scoring events

            foreach (DataRow ev in dt.Rows)
            {
                double eventStart = (double)ev[AnalysisKeys.EventStartSec];
                double eventScore = (double)ev[AnalysisKeys.EventNormscore];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);
                eventsPerUnitTime[timeUnit]++;
                if (eventScore > scoreThreshold) bigEvsPerUnitTime[timeUnit]++;
            }

            string[] headers = { AnalysisKeys.KeyStartMinute, AnalysisKeys.EventTotal, ("#Ev>" + scoreThreshold) };
            Type[] types = { typeof(int), typeof(int), typeof(int) };
            var newtable = DataTableTools.CreateTable(headers, types);

            for (int i = 0; i < eventsPerUnitTime.Length; i++)
            {
                int unitID = (int)(i * unitTime.TotalMinutes);
                newtable.Rows.Add(unitID, eventsPerUnitTime[i], bigEvsPerUnitTime[i]);
            }
            return newtable;
        }

        public static void AddContext2Table(DataTable dt, TimeSpan segmentStartMinute, TimeSpan recordingTimeSpan)
        {
            if (!dt.Columns.Contains(AnalysisKeys.KeySegmentDuration)) dt.Columns.Add(AnalysisKeys.KeySegmentDuration, typeof(double));
            if (!dt.Columns.Contains(AnalysisKeys.EventStartAbs)) dt.Columns.Add(AnalysisKeys.EventStartAbs, typeof(double));
            if (!dt.Columns.Contains(AnalysisKeys.EventStartMin)) dt.Columns.Add(AnalysisKeys.EventStartMin, typeof(double));
            double start = segmentStartMinute.TotalSeconds;
            foreach (DataRow row in dt.Rows)
            {
                row[AnalysisKeys.KeySegmentDuration] = recordingTimeSpan.TotalSeconds;
                row[AnalysisKeys.EventStartAbs] = start + (double)row[AnalysisKeys.EventStartSec];
                row[AnalysisKeys.EventStartMin] = start;
            }
        } //AddContext2Table()

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
                if (configDict.ContainsKey(AnalysisKeys.DisplayColumns))
                    displayHeaders = configDict[AnalysisKeys.DisplayColumns].Split(',').ToList();
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
            if (dt.Columns.Contains(AnalysisKeys.EventStartAbs))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.EventStartAbs + " ASC");
            }
            else if (dt.Columns.Contains(AnalysisKeys.EventCount))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.EventCount + " ASC");
            }
            else if (dt.Columns.Contains(AnalysisKeys.KeyRankOrder))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.KeyRankOrder + " ASC");
            }
            else if (dt.Columns.Contains(AnalysisKeys.KeyStartMinute))
            {
                dt = DataTableTools.SortTable(dt, AnalysisKeys.KeyStartMinute + " ASC");
            }

            table2Display = NormaliseColumnsOfDataTable(table2Display);
            return Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()

        /// <summary>
        /// takes a data table of indices and normalises column values to values in [0,1].
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
                if (headers[i].Equals(AnalysisKeys.KeyAvSignalAmplitude))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                    newHeaders[i] = headers[i] + "  (-50..-5dB)";
                }
                else //default is to NormaliseMatrixValues in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values)); //NormaliseMatrixValues all values in [0,1]
                    newHeaders[i] = headers[i];
                }
            } //for loop

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Length];
            for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
            return processedtable;
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
                    AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
                    AnalysisMinSegmentDuration = TimeSpan.FromSeconds(30),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    AnalysisTargetSampleRate = ResampleRate,
                };
            }
        }
    }
}
