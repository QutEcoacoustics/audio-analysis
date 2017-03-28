// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Audio2InputForConvCNN.cs" company="QutEcoacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Audio2InputForConvCNN type.
//   ACTIVITY CODE: audio2InputForConvCNN
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using PowerArgs;

    using TowseyLibrary;

    public class SURFAnalysis
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // use the following paths for the command line for the <audio2sonogram> task.
        // audio2InputForConvCNN "Path to CSV file"   @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Mangalam.Sonogram.yml"  "Output directory" true
        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments : SourceConfigOutputDirArguments
        {
            public FileInfo QueryWavFile { get; set; }
            public FileInfo QueryCsvFile { get; set; }

            public FileInfo TargtWavFile { get; set; }
            public FileInfo TargtCsvFile { get; set; }

            public static string Description()
            {
                return "Uses SURF points of interest to classify recording segments of bird calls.";
            }

            public static string AdditionalNotes()
            {
                return "Nothing to add.";
            }
        }



        private static Arguments Dev()
        {
            DateTime time = DateTime.Now;
            string datestamp = String.Format("{0}{1:d2}{2:d2}", time.Year, time.Month, time.Day);
            return new Arguments
            {
                QueryWavFile = @"Y:\XueyanDataset\Query\4. Eastern whipbird\Query1\NEJB_NE465_20101014-052000-0521000-estern whipbird.wav".ToFileInfo(),
                QueryCsvFile = @"Y:\XueyanDataset\Query\4. Eastern whipbird\Query1\NEJB_NE465_20101014-052000-0521000-estern whipbird.csv".ToFileInfo(),

                TargtWavFile = @"Y:\XueyanDataset\Training recordings\Eastern Whipbird-Training\NEJB_NE465_20101016-045500-045600-Eastern Whipbird.wav".ToFileInfo(),
                TargtCsvFile = @"Y:\XueyanDataset\Training recordings\Eastern Whipbird-Training\NEJB_NE465_20101016-045500-045600-Eastern Whipbird.csv".ToFileInfo(),

                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo(),

                Output = (@"C:\SensorNetworks\Output\XueyanDataset\" + datestamp).ToDirectoryInfo(),
                Source = null,
            };

            throw new NoDeveloperMethodException();
        }


        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            if (!arguments.Output.Exists) arguments.Output.Create();

            const string Title = "# PRE-PROCESS SHORT AUDIO RECORDINGS FOR Convolutional DNN";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input Query  file: " + arguments.QueryWavFile.Name);
            LoggedConsole.WriteLine("# Input target file: " + arguments.TargtWavFile.Name);
            LoggedConsole.WriteLine("# Configure    file: " + arguments.Config.Name);
            LoggedConsole.WriteLine("# Output  directory: " + arguments.Output.Name);

            // 1. set up the necessary files
            FileInfo queryWavfile = arguments.QueryWavFile;
            FileInfo queryCsvfile = arguments.QueryCsvFile;
            FileInfo targtWavfile = arguments.TargtWavFile;
            FileInfo targtCsvfile = arguments.TargtCsvFile;
            FileInfo configFile = arguments.Config;
            DirectoryInfo opDir = arguments.Output;

            // 2. get the config dictionary
            dynamic configuration = Yaml.Deserialise(configFile);

            // below four lines are examples of retrieving info from dynamic config
            //dynamic configuration = Yaml.Deserialise(configFile);
            // string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            // int resampleRate = (int?)configuration[AnalysisKeys.ResampleRate] ?? AppConfigHelper.DefaultTargetSampleRate;


            var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);


            configDict[AnalysisKeys.AddAxes] = ((bool?)configuration[AnalysisKeys.AddAxes] ?? true).ToString();
            configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? true;

            //bool makeSoxSonogram = (bool?)configuration[AnalysisKeys.MakeSoxSonogram] ?? false;
            configDict[AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes] ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = (string)configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";

            // print out the parameters
            LoggedConsole.WriteLine("\nPARAMETERS");
            foreach (KeyValuePair<string, string> kvp in configDict)
            {
                LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
            }


            //set up the output file
            //string header = "File Name, MinFreq(Hz), MaxFreq(Hz), StartTime(s), EndTime(s), Duration(s), Annotated by expert(Y-1/N-0),Correct Annotation(Y-1/N-0)";
            string header = "File Name,MinFreq(Hz),MaxFreq(Hz),StartTime(s),EndTime(s),Duration(s),Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTThirdSNR,path2Spectrograms";
            string opPath = Path.Combine(opDir.FullName, "OUTPUT.csv");
            using (StreamWriter writer = new StreamWriter(opPath))
            {
                writer.WriteLine(header);
            }

            // reads the entire file
            var data = FileTools.ReadTextFile(queryCsvfile.FullName);
            // read single record from csv file
            var record = CsvDataRecord.ReadDataRecord(data[1]);

            if (!queryWavfile.Exists)
            {
                string warning = String.Format("FILE DOES NOT EXIST >>>," + arguments.QueryWavFile.Name);
                LoggedConsole.WriteWarnLine(warning);
                return;
            }

            // ####################################################################
            var result = AnalyseOneRecording(queryWavfile, configDict, record.event_start_seconds, record.event_end_seconds,
                                             record.low_frequency_hertz, record.high_frequency_hertz, opDir);

            // CONSTRUCT the outputline for csv file
            //  fileName,Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTThirdSNR,path
            string line = String.Format("{0},{1},{2},{3:f2},{4:f2},{5:f2},{6:f1},{7:f3},{8:f3},{9:f3},{10}",
                                        record.wavFile_name, record.low_frequency_hertz, record.high_frequency_hertz,
                                        record.event_start_seconds.TotalSeconds, record.event_end_seconds.TotalSeconds,
                                        result.SnrStatistics.ExtractDuration.TotalSeconds,
                                        result.SnrStatistics.Threshold, result.SnrStatistics.Snr,
                                        result.SnrStatistics.FractionOfFramesExceedingThreshold, result.SnrStatistics.FractionOfFramesExceedingOneThirdSnr,
                                        result.SpectrogramFile.FullName);

            // It is helpful to write to the output file as we go, so as to keep a record of where we are up to.
            // This requires to open and close the output file at each iteration
            using (StreamWriter writer = new StreamWriter(opPath, true))
            {
                writer.WriteLine(line);
            }


            // ####################################################################
            result = AnalyseOneRecording(targtWavfile, configDict, record.event_start_seconds, record.event_end_seconds,
                                             record.low_frequency_hertz, record.high_frequency_hertz, opDir);

            // CONSTRUCT the outputline for csv file
            //  fileName,Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTThirdSNR,path
            //string line = String.Format("{0},{1},{2},{3:f2},{4:f2},{5:f2},{6:f1},{7:f3},{8:f3},{9:f3},{10}",
            //                            record.wavFile_name, record.low_frequency_hertz, record.high_frequency_hertz,
            //                            record.event_start_seconds.TotalSeconds, record.event_end_seconds.TotalSeconds,
            //                            result.SnrStatistics.ExtractDuration.TotalSeconds,
            //                            result.SnrStatistics.Threshold, result.SnrStatistics.Snr,
            //                            result.SnrStatistics.FractionOfFramesExceedingThreshold, result.SnrStatistics.FractionOfFramesExceedingOneThirdSNR,
            //                            result.SpectrogramFile.FullName);

            // It is helpful to write to the output file as we go, so as to keep a record of where we are up to.
            // This requires to open and close the output file at each iteration
            using (StreamWriter writer = new StreamWriter(opPath, true))
            {
                writer.WriteLine(line);
            }
        } // end MAIN()



        public static AudioToSonogramResult AnalyseOneRecording(FileInfo sourceRecording, Dictionary<string, string> configDict, TimeSpan localEventStart, TimeSpan localEventEnd,
                                                                int minHz, int maxHz, DirectoryInfo opDir)
        {
            // set a threshold for determining energy distribution in call
            // NOTE: value of this threshold depends on whether working with decibel, energy or amplitude values
            double threshold = 9.0;

            //int resampleRate = AppConfigHelper.DefaultTargetSampleRate;
            int resampleRate = 22050;
            if (configDict.ContainsKey(AnalysisKeys.ResampleRate))
            {
                resampleRate = Int32.Parse(configDict[AnalysisKeys.ResampleRate]);
            }
            configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

            // 1: GET RECORDING and make temporary copy
            // put temp audio FileSegment in same directory as the required output image.
            FileInfo tempAudioSegment = new FileInfo(Path.Combine(opDir.FullName, "tempWavFile.wav"));
            // delete the temp audio file if it already exists.
            if (File.Exists(tempAudioSegment.FullName))
            {
                File.Delete(tempAudioSegment.FullName);
            }
            // This line creates a temporary version of the source file downsampled as per entry in the config file
            MasterAudioUtility.SegmentToWav(sourceRecording, tempAudioSegment, new AudioUtilityRequest() { TargetSampleRate = resampleRate });

            // 2: Generate sonogram image files
            AudioToSonogramResult result = new AudioToSonogramResult();
            result = SURFAnalysis.GenerateSpectrogramImages(tempAudioSegment, configDict, opDir);

            // 3: GET the SNR statistics
            TimeSpan eventDuration = localEventEnd - localEventStart;
            result.SnrStatistics = SNR.Calculate_SNR_ShortRecording(tempAudioSegment, configDict, localEventStart, eventDuration, minHz, maxHz, threshold);
            return result;
        }




        /// <summary>
        /// In line class used to store a single record read from a line of the csv file;
        /// </summary>
        public class CsvDataRecord
        {
            // File Name, MinFreq(Hz), MaxFreq(Hz), StartTime(s), EndTime(s), Duration(s), Annotated by expert(Y-1/N-0),Correct Annotation(Y-1/N-0)
            public string wavFile_name { get; set; }
            public int low_frequency_hertz { get; set; }
            public int high_frequency_hertz { get; set; }
            public TimeSpan event_start_seconds { get; set; }
            public TimeSpan event_end_seconds   { get; set; }
            public TimeSpan event_duration_seconds { get; set; }

            public static CsvDataRecord ReadDataRecord(string record)
            {
                CsvDataRecord csvDataRecord = new CsvDataRecord();

                // split and parse elements of data line
                var fields = record.Split(',');
                for (int i= 0; i < fields.Length; i++)
                {
                    string word = fields[i];
                    while ((word.StartsWith("\"")) || word.StartsWith(" "))
                    {
                        word = word.Substring(1, word.Length - 1);
                    }
                    while ((word.EndsWith("\"")) || word.EndsWith(" "))
                    {
                        word = word.Substring(0, word.Length - 1);
                    }
                    fields[i] = word;
                }

                csvDataRecord.wavFile_name = fields[0];
                csvDataRecord.low_frequency_hertz = (int)Math.Round(Double.Parse(fields[1]));
                csvDataRecord.high_frequency_hertz = (int)Math.Round(Double.Parse(fields[2]));

                csvDataRecord.event_start_seconds    = TimeSpan.FromSeconds(Double.Parse(fields[3]));
                csvDataRecord.event_end_seconds      = TimeSpan.FromSeconds(Double.Parse(fields[4]));
                csvDataRecord.event_duration_seconds = TimeSpan.FromSeconds(Double.Parse(fields[5]));

                return csvDataRecord;
            }
        }
        // class CsvDataRecord






        public class SpeciesCounts
        {
            public Dictionary<string, int> speciesCounts = new Dictionary<string, int>();
            public Dictionary<string, int> speciesIDs = new Dictionary<string, int>();
            public Dictionary<string, int> siteNames = new Dictionary<string, int>();


            public void AddSpeciesID(string speciesID, string latinInfo)
            {
                string[] parts1 = speciesID.Split(':');
                int value = Int32.Parse(parts1[0]);
                string commonName = parts1[1];

                string[] parts2 = latinInfo.Split(':');
                string latinName = "NOT AVAILABLE";
                if (parts2.Length > 1) latinName = parts2[1];

                string BothNames = commonName + "," + latinName;

                if (!speciesIDs.ContainsKey(BothNames))
                {
                    speciesIDs.Add(BothNames, value);
                }
                else
                if (!speciesIDs.ContainsValue(value))
                {
                    speciesIDs.Add(BothNames + "####", value);
                }
            }


            public void AddSpeciesCount(string speciesID)
            {
                string[] parts = speciesID.Split(':');
                if (speciesCounts.ContainsKey(parts[1]))
                {
                    speciesCounts[parts[1]]++;
                }
                else
                {
                    speciesCounts.Add(parts[1], 1);
                }

            }

            public void AddSiteName(string name)
            {
                if (siteNames.ContainsKey(name))
                {
                    siteNames[name]++;
                }
                else
                {
                    siteNames.Add(name, 1);
                }

            }

            public void Save(string path)
            {
                Csv.WriteToCsv(new FileInfo(path + "Counts.csv"), this.speciesCounts);
                Csv.WriteToCsv(new FileInfo(path + "IDs.csv"), this.speciesIDs);
                Csv.WriteToCsv(new FileInfo(path + "Sites.csv"), this.siteNames);
            }
        }




        /// <summary>
        /// In line class used to return results from the static method GenerateSpectrogramImages();
        /// </summary>
        public class AudioToSonogramResult
        {
            //  path to spectrogram image
            public FileInfo SpectrogramFile { get; set; }
            public SNR.SnrStatistics SnrStatistics { get; set; }
        }

        public static AudioToSonogramResult GenerateSpectrogramImages(FileInfo sourceRecording, Dictionary<string, string> configDict, DirectoryInfo opDir)
        {
            string sourceName = configDict[ConfigKeys.Recording.Key_RecordingFileName];
            sourceName = Path.GetFileNameWithoutExtension(sourceName);

            var result = new AudioToSonogramResult();
            // init the image stack
            var list = new List<Image>();

            // 1) draw amplitude spectrogram
            AudioRecording recordingSegment = new AudioRecording(sourceRecording.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(configDict); // default values config

            // disable noise removal for first two spectrograms
            sonoConfig.NoiseReductionType = NoiseReductionType.None;

            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            // remove the DC bin
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, sonogram.Configuration.FreqBinCount);
            //save spectrogram data at this point - prior to noise reduction
            double[,] spectrogramDataBeforeNoiseReduction = sonogram.Data;

            int lowPercentile = 20;
            double neighbourhoodSeconds = 0.25;
            int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * neighbourhoodSeconds);
            double LcnContrastLevel = 0.25;
            //LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
            //LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_ShortRecordings_SubtractAndLCN(sonogram.Data, lowPercentile, neighbourhoodFrames, LcnContrastLevel);

            // draw amplitude spectrogram unannotated
            FileInfo outputImage1 = new FileInfo(Path.Combine(opDir.FullName, sourceName + ".amplitd.png"));
            ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(sonogram.Data), outputImage1.FullName);

            // draw amplitude spectrogram annotated
            var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM + Bin LCN (Local Contrast Normalisation)");
            list.Add(image);
            //string path2 = @"C:\SensorNetworks\Output\Sonograms\dataInput2.png";
            //Histogram.DrawDistributionsAndSaveImage(sonogram.Data, path2);

            // 2) A FALSE-COLOUR VERSION OF AMPLITUDE SPECTROGRAM
            double ridgeThreshold = 0.20;
            double[,] matrix = ImageTools.WienerFilter(sonogram.Data, 3);
            byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);
            hits = RidgeDetection.JoinDisconnectedRidgesInMatrix(hits, matrix, ridgeThreshold);
            image = sonogram.GetColourAmplitudeSpectrogramFullyAnnotated("AMPLITUDE SPECTROGRAM + LCN + ridge detection", spectrogramDataBeforeNoiseReduction, null, hits);
            list.Add(image);


            Image envelopeImage = Image_Track.DrawWaveEnvelopeTrack(recordingSegment, image.Width);
            list.Add(envelopeImage);


            // 3) now draw the standard decibel spectrogram
            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            // draw decibel spectrogram unannotated
            FileInfo outputImage2 = new FileInfo(Path.Combine(opDir.FullName, sourceName + ".deciBel.png"));
            ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(sonogram.Data), outputImage2.FullName);

            image = sonogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM");
            list.Add(image);

            Image segmentationImage = Image_Track.DrawSegmentationTrack(
                sonogram,
                EndpointDetectionConfiguration.K1Threshold,
                EndpointDetectionConfiguration.K2Threshold,
                image.Width);
            list.Add(segmentationImage);

            // keep the sonogram data (NOT noise reduced) for later use
            double[,] dbSpectrogramData = (double[,])sonogram.Data.Clone();

            // 4) now draw the noise reduced decibel spectrogram
            sonoConfig.NoiseReductionType = NoiseReductionType.Standard;
            sonoConfig.NoiseReductionParameter = 3;
            //sonoConfig.NoiseReductionType = NoiseReductionType.SHORT_RECORDING;
            //sonoConfig.NoiseReductionParameter = 50;

            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);

            // draw decibel spectrogram unannotated
            FileInfo outputImage3 = new FileInfo(Path.Combine(opDir.FullName, sourceName + ".noNoise_dB.png"));
            ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(sonogram.Data), outputImage3.FullName);
            image = sonogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM + Lamel noise subtraction");
            list.Add(image);

            // keep the sonogram data for later use
            double[,] nrSpectrogramData = sonogram.Data;

            // 5) A FALSE-COLOUR VERSION OF DECIBEL SPECTROGRAM
            ridgeThreshold = 2.5;
            matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);

            image = sonogram.GetColourDecibelSpectrogramFullyAnnotated("DECIBEL SPECTROGRAM - Colour annotated", dbSpectrogramData, nrSpectrogramData, hits);
            list.Add(image);

            // 6) COMBINE THE SPECTROGRAM IMAGES
            Image compositeImage = ImageTools.CombineImagesVertically(list);
            FileInfo outputImage = new FileInfo(Path.Combine(opDir.FullName, sourceName + ".5spectro.png"));
            compositeImage.Save(outputImage.FullName, ImageFormat.Png);
            result.SpectrogramFile = outputImage;

            // 7) Generate the FREQUENCY x OSCILLATIONS Graphs and csv data
            //bool saveData = true;
            //bool saveImage = true;
            //double[] oscillationsSpectrum = Oscillations2014.GenerateOscillationDataAndImages(sourceRecording, configDict, saveData, saveImage);
            return result;
        }

    }


    /// <summary>
    /// This analyzer preprocesses short audio segments a few seconds to maximum 1 minute long for processing by a convolutional Deep NN.
    /// It does not accumulate data or other indices over a long recording.
    /// </summary>
    public class PreprocessorForSURFAnalysis : IAnalyser2
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PreprocessorForSURFAnalysis()
        {
            this.DisplayName = "SURFAnalysis";
            this.Identifier = "Towsey.SURFAnalysis";
            this.DefaultSettings = new AnalysisSettings()
            {
                SegmentMaxDuration = TimeSpan.FromMinutes(1),
                SegmentMinDuration = TimeSpan.FromSeconds(20),
                SegmentMediaType = MediaTypes.MediaTypeWav,
                SegmentOverlapDuration = TimeSpan.Zero,
            };
        }

        public string DisplayName { get; private set; }

        public string Identifier { get; private set; }
        public string Description => "TODO";

        public AnalysisSettings DefaultSettings { get; private set; }

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            var audioFile = analysisSettings.AudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResult = new AnalysisResult2(analysisSettings, recording.Duration());
            dynamic configuration = Yaml.Deserialise(analysisSettings.ConfigFile);

            bool saveCsv = (bool?)configuration[AnalysisKeys.SaveIntermediateCsvFiles] ?? false;

            if ((bool?)configuration[AnalysisKeys.MakeSoxSonogram] == true)
            {
                Log.Warn("SoX spectrogram generation config variable found (and set to true) but is ignored when running as an IAnalyzer");
            }

            // generate spectrogram
            var configurationDictionary = new Dictionary<string, string>((Dictionary<string, string>)configuration);
            configurationDictionary[ConfigKeys.Recording.Key_RecordingCallName] = audioFile.FullName;
            configurationDictionary[ConfigKeys.Recording.Key_RecordingFileName] = audioFile.Name;
            var spectrogramResult = Audio2Sonogram.GenerateSpectrogramImages(
                audioFile,
                configurationDictionary,
                analysisSettings.AnalysisInstanceOutputDirectory,
                dataOnly: analysisSettings.ImageFile == null,
                makeSoxSonogram: false);

            // this analysis produces no results!
            // but we still print images (that is the point)
            if (analysisSettings.SegmentSaveBehavior.ShouldSave(analysisResult.Events.Length))
            {
                Debug.Assert(analysisSettings.ImageFile.Exists);
            }

            if (saveCsv)
            {
                var basename = Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name);
                var spectrogramCsvFile = outputDirectory.CombineFile(basename + ".Spectrogram.csv");
                Csv.WriteMatrixToCsv(spectrogramCsvFile, spectrogramResult.DecibelSpectrogram.Data, TwoDimensionalArray.RowMajor);
            }

            return analysisResult;
        }

        public void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold, bool absolute = false)
        {
            throw new NotImplementedException();
        }

        public void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // no-op
        }
    }
}


