// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Audio2InputForConvCNN.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
    using System.Text.RegularExpressions;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using PowerArgs;

    using TowseyLibrary;

    public class Audio2InputForConvCNN
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // use the following paths for the command line for the <audio2sonogram> task.
        // audio2InputForConvCNN "Path to CSV file"   @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Mangalam.Sonogram.yml"  "Output directory" true
        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments : SourceConfigOutputDirArguments
        {
            public string TargetEventBounds { get; set; }

            public static string Description()
            {
                return "Generates multiple spectrogram images and SNR info.";
            }

            public static string AdditionalNotes()
            {
                return "The Source file in this case is a csv file showing locations of short audio segments and the call bounds within each audio segment.";
            }
        }

        private static Arguments Dev()
        {
            DateTime time = DateTime.Now;
            string datestamp = string.Format("{0}{1:d2}{2:d2}", time.Year, time.Month, time.Day);
            return new Arguments
            {

                // prior to processing
                // Y:\Results\2014Aug29-000000 - ConvDNN Data Export\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.csv
                // audio_event_id	audio_recording_id	audio_recording_uuid	event_created_at_utc	projects	site_id	site_name	event_start_date_utc	event_start_seconds	event_end_seconds	event_duration_seconds	low_frequency_hertz	high_frequency_hertz	padding_start_time_seconds	padding_end_time_seconds	common_tags	species_tags	other_tags	listen_url	library_url
                // Y:\Results\2014Aug29-000000 - ConvDNN Data Export\Output\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv
                // audio_event_id	audio_recording_id	audio_recording_uuid	event_created_at_utc	projects	site_id	site_name	event_start_date_utc	event_start_seconds	event_end_seconds	event_duration_seconds	low_frequency_hertz	high_frequency_hertz	padding_start_time_seconds	padding_end_time_seconds	common_tags	species_tags	other_tags	listen_url	library_url	path	download_success	skipped

                // csv file containing recording info, call bounds etc
                Source = @"Y:\Results\2014Aug29-000000 - Mangalam Data Export\Output\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv".ToFileInfo(),
                ////Source = @"C:\SensorNetworks\Output\ConvDNN\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv".ToFileInfo(),
                ////Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\ConvDNN_annotation_export_commonNameOnly_withPadding_20140829.processed.csv".ToFileInfo(),

                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Mangalam.Sonogram.yml".ToFileInfo(),

                Output = (@"C:\SensorNetworks\Output\ConvDNN\" + datestamp).ToDirectoryInfo(),
            };

            throw new NoDeveloperMethodException();
        }

        /// <summary>
        /// This is the entrypoint for generating ConCNN spectrograms - one at a time
        /// </summary>
        /// <param name="arguments"></param>
        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            LoggedConsole.WriteLine("Generate ConvDNN images for single recording");
            LoggedConsole.WriteLine("# Input Audio file: " + arguments.Source.Name);
            LoggedConsole.WriteLine("# Configuration  file: " + arguments.Config.Name);
            LoggedConsole.WriteLine("# Output directry: " + arguments.Output.Name);

            // Verify target event information
            if (string.IsNullOrWhiteSpace(arguments.TargetEventBounds))
            {
                throw new ArgumentException("Ths TargetEventBounds paramter must be specified");
            }
            var matchResults = Regex.Match(arguments.TargetEventBounds, @"([0-9\.]+),([0-9\.]+),([0-9\.]+),([0-9\.]+)");
            TimeSpan localEventStart;
            TimeSpan localEventEnd;
            float minHz;
            float mazHz;
            if (matchResults.Success)
            {
                localEventStart = TimeSpan.FromSeconds(float.Parse(matchResults.Groups[1].Value));
                localEventEnd = TimeSpan.FromSeconds(float.Parse(matchResults.Groups[2].Value));
                minHz = float.Parse(matchResults.Groups[3].Value);
                mazHz = float.Parse(matchResults.Groups[4].Value);
            }
            else
            {
                throw new ArgumentException("The TargetEventBounds must be specified as four comma seperated numbers (localEventStart,localEventEnd,minHz,mazHz)");
            }

            // Grab configuration
            var configDict = GetConfigurationForConvCNN(arguments.Config);

            var result = AnalyseOneRecording(arguments.Source, configDict, localEventStart, localEventEnd, (int)minHz, (int)mazHz, arguments.Output);
            LoggedConsole.WriteLine("SpectrogramPath:" + result.SpectrogramFile);
            LoggedConsole.WriteLine("SnrStats:" + Json.SerialiseToString(result.SnrStatistics, prettyPrint: false));
        }



        /// <summary>
        /// This method written 18-09-2014 to process Mangalam's CNN recordings.
        /// Calculate the SNR statistics for each recording and then write info back to csv file
        /// </summary>
        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            if (!arguments.Output.Exists)
            {
                arguments.Output.Create();
            }

            const string Title = "# PRE-PROCESS SHORT AUDIO RECORDINGS FOR Convolutional DNN";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input .csv file: " + arguments.Source.Name);
            LoggedConsole.WriteLine("# Configure  file: " + arguments.Config.Name);
            LoggedConsole.WriteLine("# Output directry: " + arguments.Output.Name);

            // 1. set up the necessary files
            FileInfo csvFileInfo = arguments.Source;
            FileInfo configFile = arguments.Config;
            DirectoryInfo output = arguments.Output;

            // 2. get the config dictionary
            var configDict = GetConfigurationForConvCNN(configFile);

            bool doPreprocessing = true;


            // print out the parameters
            LoggedConsole.WriteLine("\nPARAMETERS");
            foreach (var kvp in configDict)
            {
                LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
            }



            // set up header of the output file
            string outputPath = Path.Combine(output.FullName, "SNRInfoForConvDnnDataset.csv");
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                string header = AudioToSonogramResult.GetCsvHeader();
                writer.WriteLine(header);
            }

            // following int's are counters to monitor file availability
            int lineNumber = 0;
            int fileExistsCount = 0;
            int fileLocationNotInCsv = 0;
            int fileInCsvDoesNotExist = 0;

            // keep track of species names and distribution of classes.
            // following dictionaries are to monitor species numbers
            var speciesCounts = new SpeciesCounts();


            // read through the csv file containing info about recording locations and call bounds
            try
            {
                var file = new FileStream(csvFileInfo.FullName, FileMode.Open);
                var sr = new StreamReader(file);

                // read the header and discard
                string strLine;
                lineNumber++;

                while ((strLine = sr.ReadLine()) != null)
                {
                    lineNumber++;
                    if (lineNumber % 5000 == 0)
                    {
                        Console.WriteLine(lineNumber);
                    }

                    // cannot use next line because reads the entire file
                    ////var data = Csv.ReadFromCsv<string[]>(csvFileInfo).ToList();

                    // read single record from csv file
                    var record = CsvDataRecord.ReadLine(strLine);

                    if (record.path == null)
                    {
                        fileLocationNotInCsv++;
                        ////string warning = String.Format("######### WARNING: line {0}  NULL PATH FIELD >>>null<<<", count);
                        ////LoggedConsole.WriteWarnLine(warning);
                        continue;
                    }

                    var sourceRecording = record.path;
                    var sourceDirectory = sourceRecording.Directory;
                    string parentDirectoryName = sourceDirectory.Parent.Name;
                    var imageOpDir = new DirectoryInfo(output.FullName + @"\" + parentDirectoryName);
                    ////DirectoryInfo imageOpDir = new DirectoryInfo(outDirectory.FullName + @"\" + parentDirectoryName + @"\" + directoryName);

                    /*#######################################
                      #######################################
                      my debug code for home to test on subset of data - comment these lines when at QUT!
                      Anthony will tell me I should use a conditional compilation flag.
                        -- Anthony will tell you that this is completely unnecessary!
                      ####################################### */
                    ////DirectoryInfo localSourceDir = new DirectoryInfo(@"C:\SensorNetworks\WavFiles\ConvDNNData");
                    ////sourceRecording = Path.Combine(localSourceDir.FullName + @"\" + parentDirectoryName + @"\" + directoryName, fileName).ToFileInfo();
                    ////record.path = sourceRecording;

                    /* ####################################### */

                    // TO TEST PORTION OF DATA
                    doPreprocessing = false;
                    if (parentDirectoryName.Equals("0"))
                    {
                        doPreprocessing = true;
                    }

                    /* #######################################
                       ####################################### */


                    if (!sourceRecording.Exists)
                    {
                        fileInCsvDoesNotExist++;
                        string warning = string.Format("FILE DOES NOT EXIST >>>," + sourceRecording.Name);
                        using (StreamWriter writer = new StreamWriter(outputPath, true))
                        {
                            writer.WriteLine(warning);
                        }
                        ////LoggedConsole.WriteWarnLine(warning);
                        continue;
                    }

                    // ####################################################################
                    if (doPreprocessing)
                    {
                        AudioToSonogramResult result = AnalyseOneRecording(record, configDict, output);
                        string line = result.WriteResultAsLineOfCSV();

                        // It is helpful to write to the output file as we go, so as to keep a record of where we are up to.
                        // This requires to open and close the output file at each iteration
                        using (StreamWriter writer = new StreamWriter(outputPath, true))
                        {
                            writer.WriteLine(line);
                        }
                    }

                    // everything should be OK - have jumped through all the hoops.
                    fileExistsCount++;
                    // keep track of species names and distribution of classes.
                    speciesCounts.AddSpeciesCount(record.common_tags);
                    speciesCounts.AddSpeciesID(record.common_tags, record.species_tags);
                    speciesCounts.AddSiteName(record.site_name);

                } // end while()

                string classDistributionOpPath = Path.Combine(output.FullName, "ClassDistributionsForConvDnnDataset.csv");
                speciesCounts.Save(classDistributionOpPath);
            }
            catch (IOException e)
            {
                LoggedConsole.WriteLine("Something went seriously bloody wrong!");
                LoggedConsole.WriteLine(e.ToString());
                return;
            }

            LoggedConsole.WriteLine("fileLocationNotInCsv =" + fileLocationNotInCsv);
            LoggedConsole.WriteLine("fileInCsvDoesNotExist=" + fileInCsvDoesNotExist);
            LoggedConsole.WriteLine("fileExistsCount      =" + fileExistsCount);

            LoggedConsole.WriteLine("\n##### FINISHED FILE ############################\n");
        }



        /// <summary>
        /// This method was written 9th March 2015 to process a dataset of some 1000 x 5 second recordings.
        /// The dataset was originally prepared by Meriem for use in her Master's thesis.
        /// The data is being processed to produce grayscale spectrogram images for use by Mangalam.
        /// She will classify them using a CNN.
        /// Note: NO SNR statistics are calculated. All reocrdings must be in single level directory.
        /// </summary>
        public static void ProcessMeriemsDataset()
        {
            //if (arguments == null)
            //{
            //    arguments = Dev();
            //}

            //if (!arguments.Output.Exists)
            //{
            //    arguments.Output.Create();
            //}

            const string Title = "# PRE-PROCESS SHORT AUDIO RECORDINGS FOR Convolutional DNN";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            //LoggedConsole.WriteLine("# Input .csv file: " + arguments.Source.Name);
            //LoggedConsole.WriteLine("# Configure  file: " + arguments.Config.Name);
            //LoggedConsole.WriteLine("# Output directry: " + arguments.Output.Name);


            //bool verbose = arguments.Verbose;
            bool verbose = true;

            // 1. set up the necessary files
            //FileInfo csvFileInfo = arguments.Source;
            //FileInfo configFile = arguments.Config;
            //DirectoryInfo output = arguments.Output;
            DirectoryInfo inputDirInfo  = new DirectoryInfo(@"C:\SensorNetworks\WavFiles\MeriemDataSet\1016 Distinct files");
            FileInfo         configFile = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Mangalam.Sonogram.yml");
            DirectoryInfo outputDirInfo = new DirectoryInfo(@"C:\SensorNetworks\Output\ConvDNN\MeriemDataset\");

            // 2. get the config dictionary
            var configDict = GetConfigurationForConvCNN(configFile);
            // print out the parameters
            if (verbose)
            {
                LoggedConsole.WriteLine("\nPARAMETERS");
                foreach (var kvp in configDict)
                {
                    LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                }
            }

            int fileCount = 0;
            FileInfo[] inputFiles = inputDirInfo.GetFiles();
            foreach (FileInfo file in inputFiles)
            {
                LoggedConsole.Write(".");
                if (!file.Exists) continue;
                fileCount++;

                // need to set up the config with file names
                configDict[ConfigKeys.Recording.Key_RecordingFileName] = file.FullName;
                configDict[ConfigKeys.Recording.Key_RecordingCallName] = Path.GetFileNameWithoutExtension(file.FullName);
                // reset the frame size
                configDict["FrameLength"] = "512";

                //AudioToSonogramResult result = GenerateFourSpectrogramImages(file, configDict, outputDirInfo);
                GenerateSpectrogramImages(file, configDict, outputDirInfo);
                fileCount++;
            } // end foreach()

            LoggedConsole.WriteLine("\nFile Count =" + fileCount);
            LoggedConsole.WriteLine("\n##### FINISHED ############################\n");
        }




        private static Dictionary<string, string> GetConfigurationForConvCNN(FileInfo configFile)
        {
            dynamic configuration = Yaml.Deserialise(configFile);

            var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);

            configDict[AnalysisKeys.AddAxes] = ((bool?)configuration[AnalysisKeys.AddAxes] ?? true).ToString();
            configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? true;

            ////bool makeSoxSonogram = (bool?)configuration[AnalysisKeys.MakeSoxSonogram] ?? false;
            configDict[AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes] ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = (string)configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";
            return configDict;
        }

        // end MAIN()




        public static AudioToSonogramResult AnalyseOneRecording(CsvDataRecord dataRecord, Dictionary<string, string> configDict, DirectoryInfo opDir)
        {
            // ############## IMPORTANT PARAMETER - SET EQUAL TO WHAT ANTHONY HAS EXTRACTED.
            double extractFixedTimeDuration = 4.0; // fixed length duration of all extracts from the original data - centred on the bounding box.

            //
            FileInfo sourceRecording = dataRecord.path;
            string fileName = sourceRecording.Name;
            // string message = String.Format("#########: line {0}  FILE EXISTS >>>" + sourceRecording.Name + "<<<", fileExistsCount);
            // LoggedConsole.WriteWarnLine(message);
            DirectoryInfo sourceDir = sourceRecording.Directory;
            int minHz = dataRecord.low_frequency_hertz;
            int maxHz = dataRecord.high_frequency_hertz;
            TimeSpan start = dataRecord.event_start_seconds;
            TimeSpan eventDuration = dataRecord.event_duration_seconds;
            TimeSpan eventCentre = TimeSpan.FromTicks(eventDuration.Ticks / 2);
            TimeSpan extractDuration = TimeSpan.FromSeconds(extractFixedTimeDuration);
            TimeSpan extractHalfDuration = TimeSpan.FromSeconds(extractFixedTimeDuration / 2);

            TimeSpan localEventStart = TimeSpan.Zero;
            TimeSpan localEventEnd = extractDuration;
            if ((eventDuration != TimeSpan.Zero) && (eventDuration < extractDuration))
            {
                localEventStart = extractHalfDuration - eventCentre;
                localEventEnd = extractHalfDuration + eventCentre;
            }

            var result = AnalyseOneRecording(sourceRecording, configDict, localEventStart, localEventEnd, minHz, maxHz, opDir);

            // add additional info to identify this recording
            result.AudioEventId = dataRecord.audio_event_id;
            result.SiteName = dataRecord.site_name;
            result.CommonTags = dataRecord.common_tags;
            return result;
        }

        public static AudioToSonogramResult AnalyseOneRecording(
            FileInfo sourceRecording,
            Dictionary<string, string> configDict,
            TimeSpan localEventStart,
            TimeSpan localEventEnd,
            int minHz,
            int maxHz,
            DirectoryInfo outDirectory)
        {
            // set a threshold for determining energy distribution in call
            // NOTE: value of this threshold depends on whether working with decibel, energy or amplitude values
            const double Threshold = 9.0;

            int resampleRate = AppConfigHelper.DefaultTargetSampleRate;
            if (configDict.ContainsKey(AnalysisKeys.ResampleRate))
            {
                resampleRate = int.Parse(configDict[AnalysisKeys.ResampleRate]);
            }

            configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

            // 1: GET RECORDING and make temporary copy
            // put temp audio FileSegment in same directory as the required output image.
            var tempAudioSegment = TempFileHelper.NewTempFile(outDirectory, "wav");

            // delete the temp audio file if it already exists.
            if (File.Exists(tempAudioSegment.FullName))
            {
                File.Delete(tempAudioSegment.FullName);
            }

            // This line creates a temporary version of the source file downsampled as per entry in the config file
            MasterAudioUtility.SegmentToWav(sourceRecording, tempAudioSegment, new AudioUtilityRequest() { TargetSampleRate = resampleRate });

            // 2: Generate sonogram image files
            AudioToSonogramResult result = new AudioToSonogramResult();
            result = GenerateSpectrogramImages(tempAudioSegment, configDict, outDirectory);

            // 3: GET the SNR statistics
            TimeSpan eventDuration = localEventEnd - localEventStart;
            result.SnrStatistics = SNR.Calculate_SNR_ShortRecording(tempAudioSegment, configDict, localEventStart, eventDuration, minHz, maxHz, Threshold);

            // 4: Delete the temp file
            File.Delete(tempAudioSegment.FullName);


            return result;
        }




        /// <summary>
        /// In line class used to store a single record read from a line of the csv file;
        /// </summary>
        public class CsvDataRecord
        {
            public int audio_event_id { get; set; }
            public int audio_recording_id { get; set; }
            //public int audio_recording_uuid { get; set; }
            //public string event_created_at_utc { get; set; }
            public string projects { get; set; }
            public int site_id { get; set; }
            public string site_name { get; set; }
            //event_start_date_utc { get; set; }
            public TimeSpan event_start_seconds { get; set; }
            //event_end_seconds { get; set; }
            public TimeSpan event_duration_seconds { get; set; }
            public int low_frequency_hertz { get; set; }
            public int high_frequency_hertz { get; set; }
            public TimeSpan padding_start_time_seconds { get; set; }
            public TimeSpan padding_end_time_seconds { get; set; }
            public string common_tags { get; set; }
            public string species_tags { get; set; }
            //other_tags { get; set; }
            //listen_url { get; set; }
            //library_url { get; set; }
            // path to audio recording
            public FileInfo path { get; set; }
            //download_success { get; set; }
            //skipped { get; set; }

            public static CsvDataRecord ReadLine(string record)
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
                csvDataRecord.audio_event_id = int.Parse(fields[0]);
                csvDataRecord.audio_recording_id = int.Parse(fields[1]);
                csvDataRecord.projects = fields[4];
                csvDataRecord.site_id = int.Parse(fields[5]);
                csvDataRecord.site_name = fields[6];

                csvDataRecord.event_start_seconds = TimeSpan.FromSeconds(double.Parse(fields[8]));
                csvDataRecord.event_duration_seconds = TimeSpan.FromSeconds(double.Parse(fields[10]));
                csvDataRecord.padding_start_time_seconds = TimeSpan.FromSeconds(double.Parse(fields[13]));
                csvDataRecord.padding_end_time_seconds   = TimeSpan.FromSeconds(double.Parse(fields[14]));

                csvDataRecord.low_frequency_hertz = (int)Math.Round(double.Parse(fields[11]));
                csvDataRecord.high_frequency_hertz = (int)Math.Round(double.Parse(fields[12]));
                csvDataRecord.common_tags = fields[15];
                csvDataRecord.species_tags = fields[16];
                csvDataRecord.path = fields[20].ToFileInfo();
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
                int value = int.Parse(parts1[0]);
                string commonName = parts1[1];

                string[] parts2 = latinInfo.Split(':');
                string latinName = "NOT AVAILABLE";
                if (parts2.Length > 1) latinName = parts2[1];

                string BothNames = commonName + "," + latinName;

                if (!this.speciesIDs.ContainsKey(BothNames))
                {
                    this.speciesIDs.Add(BothNames, value);
                }
                else
                if (!this.speciesIDs.ContainsValue(value))
                {
                    this.speciesIDs.Add(BothNames + "####", value);
                }
            }


            public void AddSpeciesCount(string speciesID)
            {
                string[] parts = speciesID.Split(':');
                if (this.speciesCounts.ContainsKey(parts[1]))
                {
                    this.speciesCounts[parts[1]]++;
                }
                else
                {
                    this.speciesCounts.Add(parts[1], 1);
                }

            }

            public void AddSiteName(string name)
            {
                if (this.siteNames.ContainsKey(name))
                {
                    this.siteNames[name]++;
                }
                else
                {
                    this.siteNames.Add(name, 1);
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
        /// In line class used to return results from the static method Audio2InputForConvCNN.GenerateFourSpectrogramImages();
        /// </summary>
        public class AudioToSonogramResult
        {
            /// <summary>
            /// ID of the event included in the recording segment
            /// </summary>
            public int AudioEventId { get; set; }

            /// <summary>
            /// name of site where recording was made
            /// </summary>
            public string SiteName { get; set; }

            /// <summary>
            /// class label for this recording
            /// </summary>
            public string CommonTags { get; set; }

            /// <summary>
            /// path to spectrogram images of this recording
            /// </summary>
            public FileInfo SpectrogramFile { get; set; }

            /// <summary>
            /// snr information for this recording
            /// </summary>
            public SNR.SnrStatistics SnrStatistics { get; set; }

            /// <summary>
            /// CONSTRUCT the header for csv file
            ///  audio_event_id,site_name,common_tags,Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTThirdSNR,path
            /// </summary>
            /// <returns></returns>
            public string WriteResultAsLineOfCSV()
            {
                string line = string.Format(
                    "{0},{1},{2},{3:f2},{4:f3},{5:f3},{6:f3},{7:f2},{8}",
                    this.AudioEventId,
                    this.SiteName,
                    this.CommonTags,
                    this.SnrStatistics.Threshold,
                    this.SnrStatistics.Snr,
                    this.SnrStatistics.FractionOfFramesExceedingThreshold,
                    this.SnrStatistics.FractionOfFramesExceedingOneThirdSnr,
                    this.SnrStatistics.ExtractDuration.TotalSeconds,
                    this.SpectrogramFile.FullName);
                return line;
            }

            /// <summary>
            /// CONSTRUCT the header for the above csv file
            /// Following line is headers from Anthony's returned csv file
            /// "audio_event_id,audio_recording_id,audio_recording_uuid,projects,site_name,event_start_date_utc,event_duration_seconds,common_tags,species_tags,other_tags,path,Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTHalfSNR";
            /// <para>
            /// Following line is header for these results.
            ///  audio_event_id,site_name,common_tags,Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTThirdSNR,path
            /// </para>
            /// </summary>
            /// <returns></returns>
            public static string GetCsvHeader()
            {
                const string Header = "audio_event_id,site_name,common_tags,Threshold,Snr,FractionOfFramesGTThreshold,FractionOfFramesGTThirdSNR,Duration(sec),path2Spectrograms";
                return Header;
            }
        }

        public static AudioToSonogramResult GenerateSpectrogramImages(FileInfo sourceRecording, Dictionary<string, string> configDict, DirectoryInfo outputDirectory)
        {
            // the source name was set up in the Analyse() method. But it could also be obtained directly from recording.
            string sourceName = configDict[ConfigKeys.Recording.Key_RecordingFileName];
            sourceName = Path.GetFileNameWithoutExtension(sourceName);

            var result = new AudioToSonogramResult();

            // init the image stack
            var list = new List<Image>();

            // 1) draw amplitude spectrogram
            AudioRecording recordingSegment = new AudioRecording(sourceRecording.FullName);

            // default values config
            SonogramConfig sonoConfig = new SonogramConfig(configDict);

            // disable noise removal for first two spectrograms
            sonoConfig.NoiseReductionType = NoiseReductionType.None;

            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);

            // remove the DC bin
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, sonogram.Configuration.FreqBinCount);

            // save spectrogram data at this point - prior to noise reduction
            double[,] spectrogramDataBeforeNoiseReduction = sonogram.Data;

            const int LowPercentile = 20;
            const double NeighbourhoodSeconds = 0.25;
            int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * NeighbourhoodSeconds);
            const double LcnContrastLevel = 0.25;

            ////LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
            ////LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_ShortRecordings_SubtractAndLCN(sonogram.Data, LowPercentile, neighbourhoodFrames, LcnContrastLevel);

            // draw amplitude spectrogram unannotated
            FileInfo outputImage1 = new FileInfo(Path.Combine(outputDirectory.FullName, sourceName + ".amplitd.bmp"));
            ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(sonogram.Data), outputImage1.FullName);

            // draw amplitude spectrogram annotated
            var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM + Bin LCN (Local Contrast Normalisation)");
            list.Add(image);
            ////string path2 = @"C:\SensorNetworks\Output\Sonograms\dataInput2.png";
            ////Histogram.DrawDistributionsAndSaveImage(sonogram.Data, path2);

            // 2) A FALSE-COLOUR VERSION OF AMPLITUDE SPECTROGRAM
            double ridgeThreshold = 0.20;
            double[,] matrix = ImageTools.WienerFilter(sonogram.Data, 3);
            byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);
            hits = RidgeDetection.JoinDisconnectedRidgesInMatrix(hits, matrix, ridgeThreshold);
            image = SpectrogramTools.CreateFalseColourAmplitudeSpectrogram(spectrogramDataBeforeNoiseReduction, null, hits);
            image = sonogram.GetImageAnnotatedWithLinearHerzScale(image, "AMPLITUDE SPECTROGRAM + LCN + ridge detection");
            list.Add(image);


            Image envelopeImage = Image_Track.DrawWaveEnvelopeTrack(recordingSegment, image.Width);
            list.Add(envelopeImage);


            // 3) now draw the standard decibel spectrogram
            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            // remove the DC bin
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, sonogram.Configuration.FreqBinCount);

            // draw decibel spectrogram unannotated
            FileInfo outputImage2 = new FileInfo(Path.Combine(outputDirectory.FullName, sourceName + ".deciBel.bmp"));
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
            ////sonoConfig.NoiseReductionType = NoiseReductionType.SHORT_RECORDING;
            ////sonoConfig.NoiseReductionParameter = 50;

            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);

            // draw decibel spectrogram unannotated
            FileInfo outputImage3 = new FileInfo(Path.Combine(outputDirectory.FullName, sourceName + ".noNoise_dB.bmp"));
            ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(sonogram.Data), outputImage3.FullName);
            image = sonogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM + Lamel noise subtraction");
            list.Add(image);

            // keep the sonogram data for later use
            double[,] nrSpectrogramData = sonogram.Data;

            // 5) A FALSE-COLOUR VERSION OF DECIBEL SPECTROGRAM
            ridgeThreshold = 2.5;
            matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);

            image = SpectrogramTools.CreateFalseColourDecibelSpectrogram(dbSpectrogramData, nrSpectrogramData, hits);
            image = sonogram.GetImageAnnotatedWithLinearHerzScale(image, "DECIBEL SPECTROGRAM - Colour annotated");
            list.Add(image);

            // 6) COMBINE THE SPECTROGRAM IMAGES
            Image compositeImage = ImageTools.CombineImagesVertically(list);
            FileInfo outputImage = new FileInfo(Path.Combine(outputDirectory.FullName, sourceName + ".5spectro.png"));
            compositeImage.Save(outputImage.FullName, ImageFormat.Png);
            result.SpectrogramFile = outputImage;

            // 7) Generate the FREQUENCY x OSCILLATIONS Graphs and csv data
            ////bool saveData = true;
            ////bool saveImage = true;
            ////double[] oscillationsSpectrum = Oscillations2014.GenerateOscillationDataAndImages(sourceRecording, configDict, saveData, saveImage);
            return result;
        }

    }



    /// <summary>
    /// This analyzer preprocesses short audio segments a few seconds to maximum 1 minute long for processing by a convolutional Deep NN.
    /// It does not accumulate data or other indices over a long recording.
    /// </summary>
    public class PreprocessorForConvDNN : IAnalyser2
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PreprocessorForConvDNN()
        {
            this.DisplayName = "ConvolutionalDNN";
            this.Identifier = "Towsey.PreprocessorForConvDNN";
            this.DefaultSettings = new AnalysisSettings()
            {
                AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
                AnalysisMinSegmentDuration = TimeSpan.FromSeconds(20),
                SegmentMediaType = MediaTypes.MediaTypeWav,
                SegmentOverlapDuration = TimeSpan.Zero,
            };
        }

        public string DisplayName { get; private set; }

        public string Identifier { get; private set; }
        public string Description => "This analyzer preprocesses short audio segments a few seconds to maximum 1 minute long for processing by a convolutional Deep NN. It does not accumulate data or other indices over a long recording.";

        public AnalysisSettings DefaultSettings { get; private set; }

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            var audioFile = analysisSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = analysisSettings.SegmentOutputDirectory;

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
            var soxImage = new FileInfo(Path.Combine(analysisSettings.SegmentOutputDirectory.FullName, audioFile.Name + ".SOX.png"));

            var spectrogramResult = Audio2Sonogram.GenerateFourSpectrogramImages(
                audioFile,
                soxImage,
                configurationDictionary,
                dataOnly: analysisSettings.SegmentImageFile == null,
                makeSoxSonogram: false);

            // this analysis produces no results!
            // but we still print images (that is the point)
            if (analysisSettings.AnalysisSaveBehavior.ShouldSave(analysisResult.Events.Length))
            {
                Debug.Assert(analysisSettings.SegmentImageFile.Exists);
            }

            if (saveCsv)
            {
                var basename = Path.GetFileNameWithoutExtension(analysisSettings.SegmentAudioFile.Name);
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


