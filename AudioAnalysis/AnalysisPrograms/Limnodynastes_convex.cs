// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Limnodynastes_convex.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
//  The ACTION code for this analysis is: "Limnodynastes_convex"
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using TowseyLibrary;

    using ProcessRunner = TowseyLibrary.ProcessRunner;



    /// <summary>
    /// NOTE: This recogniser is for the frog Limnodynastes convexiusculus.
    /// It was built using two recordings:
    /// 1. One from the david Steart CD with high SNR and usual cleaned up recording
    /// 2. One from JCU, Lin and Kiyomi.
    /// Recording 2. also contains canetoad and Litoria fallax.
    /// So I have combined the three recognisers into one analysis.
    /// 
    /// </summary>
    public class Limnodynastes_convex : AbstractStrongAnalyser
    {
        #region Constants

        public const string AnalysisName = "Limnodynastes_convex";

        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";
        //public const int RESAMPLE_RATE = 17640;
        public const int RESAMPLE_RATE = 22050;

        #endregion

        #region Public Properties

        public override AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                           {
                               SegmentMaxDuration = TimeSpan.FromMinutes(1), 
                               SegmentMinDuration = TimeSpan.FromSeconds(30), 
                               SegmentMediaType = MediaTypes.MediaTypeWav, 
                               SegmentOverlapDuration = TimeSpan.Zero,
                               SegmentTargetSampleRate = RESAMPLE_RATE
                           };
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Limnodynastes convex";
            }
        }

        public override string Identifier
        {
            get
            {
                return "Towsey." + AnalysisName;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void Dev(Arguments arguments)
        {
            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                const string RecordingPath =
                    @"F:\SensorNetworks\WavFiles\CaneToad\UndetectedCalls-2014\KiyomiUndetected210214-1.mp3";

                const string ConfigPath =
                            @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Limnodynastes_convexiusculus.yml";
                const string OutputDir = @"C:\SensorNetworks\Output\Frogs\Limnodynastes_convex\";

                string title = "# FOR DETECTION OF LIM_CON";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + OutputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(RecordingPath));

                Log.Verbosity = 1;
                const int StartMinute = 0;
                const int DurationSeconds = 0; // set zero to get entire recording
                TimeSpan start = TimeSpan.FromMinutes(StartMinute); // hours, minutes, seconds
                TimeSpan duration = TimeSpan.FromSeconds(DurationSeconds); // hours, minutes, seconds
                string segmentFileStem = Path.GetFileNameWithoutExtension(RecordingPath);
                string segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, StartMinute);
                string sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, StartMinute);
                string eventsFname = string.Format(
                    "{0}_{1}min.{2}.Events.csv", 
                    segmentFileStem, 
                    StartMinute, 
                    "Towsey." + AnalysisName);
                string indicesFname = string.Format(
                    "{0}_{1}min.{2}.Indices.csv", 
                    segmentFileStem, 
                    StartMinute, 
                    "Towsey." + AnalysisName);

                if (true)
                {
                    arguments.Source = RecordingPath.ToFileInfo();
                    arguments.Config = ConfigPath.ToFileInfo();
                    arguments.Output = OutputDir.ToDirectoryInfo();
                    arguments.TmpWav = segmentFName;
                    arguments.Events = eventsFname;
                    arguments.Indices = indicesFname;
                    arguments.Sgram = sonogramFname;
                    arguments.Start = start.TotalSeconds;
                    arguments.Duration = duration.TotalSeconds;
                }

                if (false)
                {
                    // loads a csv file for visualisation
                    ////string indicesImagePath = "some path or another";
                    ////var fiCsvFile    = new FileInfo(restOfArgs[0]);
                    ////var fiConfigFile = new FileInfo(restOfArgs[1]);
                    ////var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                    ////IAnalysis analyser = new AnalysisTemplate();
                    ////var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                    ////returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
                }
            }

            Execute(arguments);

            if (executeDev)
            {
                FileInfo csvEvents = arguments.Output.CombineFile(arguments.Events);
                if (!csvEvents.Exists)
                {
                    Log.WriteLine(
                        "\n\n\n############\n WARNING! Events CSV file not returned from analysis of minute {0} of file <{0}>.", 
                        arguments.Start.Value, 
                        arguments.Source.FullName);
                }
                else
                {
                    LoggedConsole.WriteLine("\n");
                    DataTable dt = CsvTools.ReadCSVToTable(csvEvents.FullName, true);
                    DataTableTools.WriteTable2Console(dt);
                }

                FileInfo csvIndicies = arguments.Output.CombineFile(arguments.Indices);
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

                FileInfo image = arguments.Output.CombineFile(arguments.Sgram);
                if (image.Exists)
                {
                    var process = new ProcessRunner(ImageViewer);
                    process.Run(image.FullName, arguments.Output.FullName);
                }

                LoggedConsole.WriteLine("\n\n# Finished analysis:- " + arguments.Source.FullName);
            }
        }

        /// <summary>
        /// A WRAPPER AROUND THE analyzer.Analyze(analysisSettings) METHOD
        ///     To be called as an executable with command line arguments.
        /// </summary>
        /// <param name="arguments">
        /// The command line arguments.
        /// </param>
        public static void Execute(Arguments arguments)
        {
            Contract.Requires(arguments != null);

            AnalysisSettings analysisSettings = arguments.ToAnalysisSettings();
            TimeSpan start = TimeSpan.FromSeconds(arguments.Start ?? 0);
            TimeSpan duration = TimeSpan.FromSeconds(arguments.Duration ?? 0);

            // EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = analysisSettings.AudioFile;
            if (duration == TimeSpan.Zero)
            {
                // Process entire file
                AudioFilePreparer.PrepareFile(
                    arguments.Source, 
                    tempF,
                    new AudioUtilityRequest { TargetSampleRate = RESAMPLE_RATE }, 
                    analysisSettings.AnalysisBaseTempDirectoryChecked);
            }
            else
            {
                AudioFilePreparer.PrepareFile(
                    arguments.Source, 
                    tempF, 
                    new AudioUtilityRequest
                        {
                            TargetSampleRate = RESAMPLE_RATE, 
                            OffsetStart = start, 
                            OffsetEnd = start.Add(duration)
                        }, 
                    analysisSettings.AnalysisBaseTempDirectoryChecked);
            }

            // DO THE ANALYSIS
            /* ############################################################################################################################################# */
            IAnalyser2 analyser = new Limnodynastes_convex();
            analyser.BeforeAnalyze(analysisSettings);
            AnalysisResult2 result = analyser.Analyze(analysisSettings);
            /* ############################################################################################################################################# */

            if (result.Events.Length > 0)
            {
                LoggedConsole.WriteLine("{0} events found", result.Events.Length);
            }
            else
            {
                LoggedConsole.WriteLine("No events found");
            }
        }

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // NOTHING TO DO
            return;
        }

        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            FileInfo audioFile = analysisSettings.AudioFile;

            // execute actual analysis
            Dictionary<string, string> configuration = analysisSettings.Configuration;
            LimConResults results = Analysis(audioFile, configuration, analysisSettings.SegmentStartOffset ?? TimeSpan.Zero);
            
            var analysisResults = new AnalysisResult2(analysisSettings, results.RecordingDuration);

            BaseSonogram sonogram = results.Sonogram;
            double[,] hits = results.Hits;
            Plot scores = results.Plot;
            List<AcousticEvent> predictedEvents = results.Events;

            analysisResults.Events = predictedEvents.ToArray();

            if (analysisSettings.EventsFile != null)
            {
                this.WriteEventsFile(analysisSettings.EventsFile, analysisResults.Events);
                analysisResults.EventsFile = analysisSettings.EventsFile;
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(analysisResults.Events, unitTime, analysisResults.SegmentAudioDuration, 0);

                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
            }

            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                const double EventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, EventThreshold);
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;
            }

            return analysisResults;
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

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        public override void WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        #endregion






        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentOfSourceFile"></param>
        /// <param name="configDict"></param>
        /// <param name="segmentStartOffset"></param>
        /// <returns></returns>
        internal static LimConResults Analysis(FileInfo segmentOfSourceFile, Dictionary<string, string> configDict, TimeSpan segmentStartOffset)
        {
            Dictionary<string, double[,]> dictionaryOfHiResSpectralIndices = null;
            var recording = new AudioRecording(segmentOfSourceFile.FullName);
            return Analysis(dictionaryOfHiResSpectralIndices, recording, configDict, segmentStartOffset);
        }


        /// <summary>
        /// THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="recording">
        ///     The segment Of Source File.
        /// </param>
        /// <param name="configDict">
        ///     The config Dict.
        /// </param>
        /// <param name="value"></param>
        /// <returns>
        /// The <see cref="LimConResults"/>.
        /// </returns>
        internal static LimConResults Analysis(
            Dictionary<string, double[,]> dictionaryOfHiResSpectralIndices,
            AudioRecording recording,
            Dictionary<string, string> configDict,
            TimeSpan segmentStartOffset)
        {
            // WARNING: TODO TODO TODO = this end of this method simply duplicates the CANETOAD analyser!!!!!!!!!!!!!!!!!!!!! ###################

            // for Limnodynastes convex, in the D.Stewart CD, there are peaks close to:
            //1. 1950 Hz
            //2. 1460 hz
            //3.  970 hz    These are 490 Hz apart.
            // for Limnodynastes convex, in the JCU recording, there are peaks close to:
            //1. 1780 Hz
            //2. 1330 hz
            //3.  880 hz    These are 450 Hz apart.

            // So strategy is to look for three peaks separated by same amount and in the vicinity of the above,
            //  starting with highest power (the top peak) and working down to lowest power (bottom peak).

            //KeyValuePair<string, double[,]> kvp = dictionaryOfHiResSpectralIndices.First();
            var spg = dictionaryOfHiResSpectralIndices["RHZ"];
            int rhzRowCount = spg.GetLength(0);
            int rhzColCount = spg.GetLength(1);

            int sampleRate = recording.SampleRate;
            double herzPerBin = sampleRate / 2 / (double)rhzRowCount;
            int dominantFrequency = 1850;
            int hzBuffer  = 250;
            int bottomBin = 5;
            int dominantBin = (int)Math.Round(dominantFrequency / herzPerBin);
            int binBuffer = (int)Math.Round(hzBuffer / herzPerBin); ;
            int dominantBinMin = dominantBin - binBuffer;
            int dominantBinMax = dominantBin + binBuffer;

            //  freqBin + rowID = binCount - 1;
            // therefore: rowID = binCount - freqBin - 1;
            int minRowID  = rhzRowCount - dominantBinMax - 1;
            int maxRowID  = rhzRowCount - dominantBinMin - 1;
            int bottomRow = rhzRowCount - bottomBin - 1;

            var list = new List<Point>();

            // loop through all spectra/columns of the hi-res spectrogram.
            for (int c = 1; c < rhzColCount-1; c++)
            {
                double maxAmplitude = -Double.MaxValue;
                int idOfRowWithMaxAmplitude = 0;

                for (int r = minRowID; r <= bottomRow; r++)
                {
                    if (spg[r, c] > maxAmplitude)
                    {
                        maxAmplitude = spg[r, c];
                        idOfRowWithMaxAmplitude = r;
                    }
                }

                if (idOfRowWithMaxAmplitude < minRowID) continue;
                if (idOfRowWithMaxAmplitude > maxRowID) continue;

                // want a spectral peak.
                if (spg[idOfRowWithMaxAmplitude, c] < spg[idOfRowWithMaxAmplitude, c - 1]) continue;
                if (spg[idOfRowWithMaxAmplitude, c] < spg[idOfRowWithMaxAmplitude, c + 1]) continue;
                // peak should exceed thresold amplitude
                if (spg[idOfRowWithMaxAmplitude, c] < 3.0) continue;

                // convert row ID to freq bin ID
                int freqBinID = rhzRowCount - idOfRowWithMaxAmplitude - 1;
                list.Add(new Point(freqBinID, c));
                // we now have a list of potential hits for LimCon. This needs to be filtered.

                Console.WriteLine("Bin {0}, Col {1}  ", freqBinID, c);
            }

            // draw a debug image.
            if (true)
            {
                // put red dot where max is
                string filePath = @"G:\SensorNetworks\Output\Frogs\TestOfHiResIndices-2016July\Test\Towsey.HiResIndices\SpectrogramImages\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30_0min.CombinedGreyScale.png";
                Bitmap bmp = new Bitmap(filePath);
                foreach (Point point in list)
                {
                    bmp.SetPixel(point.Y + 70, 1911 - point.X, Color.Red);
                }
                // mark off every tenth frequency bin
                for (int r = 0; r < 26; r++)
                {
                    bmp.SetPixel(68, 1911 - (r * 10), Color.Blue);
                    bmp.SetPixel(69, 1911 - (r * 10), Color.Blue);
                }
                // mark off upper bound and lower frequency bound
                bmp.SetPixel(69, 1911 - dominantBinMin, Color.Lime);
                bmp.SetPixel(69, 1911 - dominantBinMax, Color.Lime);
                //bmp.SetPixel(69, 1911 - maxRowID, Color.Lime);
                bmp.Save(filePath + ".png");
            }


            // now construct the standard decibel spectrogram WITHOUT noise removal, and look for LimConvex
            // get frame parameters for the analysis
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            int frameSize = rhzRowCount;
            int frameStep = frameSize; // this default = zero overlap
            //var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording, frameSize, frameStep);
            //// Generate deciBel spectrogram
            //double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput.amplitudeSpectrogram, dspOutput.WindowPower, sampleRate, epsilon);

            // i: Init SONOGRAM config
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.FileName,
                WindowSize = frameSize * 2,
                WindowOverlap = 0.0,
                NoiseReductionType = NoiseReductionType.NONE
            };
            // init sonogram
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // remove the DC row of the spectrogram
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            //scores.Add(new Plot("Decibels", DataTools.normalise(dBArray), ActivityAndCover.DEFAULT_ActivityThreshold_dB));
            //scores.Add(new Plot("Active Frames", DataTools.Bool2Binary(activity.activeFrames), 0.0));

            // convert spectral peaks to frequency
            //var tuple_DecibelPeaks = SpectrogramTools.HistogramOfSpectralPeaks(deciBelSpectrogram);
            //int[] peaksBins = tuple_DecibelPeaks.Item2;
            //double[] freqPeaks = new double[peaksBins.Length];
            //int binCount = sonogram.Data.GetLength(1);
            //for (int i = 1; i < peaksBins.Length; i++) freqPeaks[i] = (lowerBinBound + peaksBins[i]) / (double)nyquistBin;
            //scores.Add(new Plot("Max Frequency", freqPeaks, 0.0));  // location of peaks for spectral images

            bool returnSonogramInfo = true; // TEMPORARY ################################
            double factor = sonogram.FrameCount / (double)rhzColCount; 
            if (returnSonogramInfo)
            {
                string file2Path = @"G:\SensorNetworks\Output\Frogs\TestOfHiResIndices-2016July\Test\Towsey.HiResIndices\SpectrogramImages\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30_0min.Spectrogram.png";
                Bitmap sonoBmp = (Bitmap)sonogram.GetImage();
                int height = sonoBmp.Height;
                foreach (Point point in list)
                {
                    sonoBmp.SetPixel((int)Math.Round(point.Y * factor), height - point.X - 1, Color.Red);
                }
                // mark off every tenth frequency bin
                for (int r = 0; r < 20; r++)
                {
                    sonoBmp.SetPixel(0, height - (r * 10) - 1, Color.Blue);
                    sonoBmp.SetPixel(1, height - (r * 10) - 1, Color.Blue);
                }
                // mark off upper bound and lower frequency bound
                sonoBmp.SetPixel(0, height - dominantBinMin, Color.Lime);
                sonoBmp.SetPixel(0, height - dominantBinMax, Color.Lime);
                sonoBmp.Save(file2Path);  
            }


            double[] scores = new double[rhzColCount]; // predefinition of score array
            foreach (Point point in list)
            {
                scores[point.Y] = point.X / (double)60.0;
            }
            double eventThreshold = 0.4;


            //events.ForEach(ae =>
            //        {
            //            ae.SpeciesName = configDict[AnalysisKeys.SpeciesName];
            //            ae.SegmentStartOffset = segmentStartOffset;
            //            ae.SegmentDuration = recordingDuration;
            //            ae.Name = "AdvertCall";
            //        });

            var plot = new Plot(AnalysisName, scores, eventThreshold);
            return new LimConResults
                       {
                           Sonogram = sonogram, 
                           Hits = null, 
                           Plot = plot, 
                           Events = null, 
                           RecordingDuration = recording.Duration()
                       };
        } // Analysis()

        private static Image DrawSonogram(
            BaseSonogram sonogram, 
            double[,] hits, 
            Plot scores, 
            List<AcousticEvent> predictedEvents, 
            double eventThreshold)
        {
            const bool DoHighlightSubband = false;
            const bool Add1KHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(DoHighlightSubband, Add1KHzLines));

            ////System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            ////img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            ////Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null)
            {
                image.AddTrack(Image_Track.GetNamedScoreTrack(scores.data, 0.0, 1.0, scores.threshold, scores.title));
            }

            if (hits != null)
            {
                image.OverlayRedTransparency(hits);
            }

            if ((predictedEvents != null) && (predictedEvents.Count > 0))
            {
                image.AddEvents(
                    predictedEvents, 
                    sonogram.NyquistFrequency, 
                    sonogram.Configuration.FreqBinCount, 
                    sonogram.FramesPerSecond);
            }

            return image.GetImage();
        }

        #endregion

        public class Arguments : AnalyserArguments
        {
        }

        public class LimConResults
        {
            #region Public Properties

            public List<AcousticEvent> Events { get; set; }

            public double[,] Hits { get; set; }

            public Plot Plot { get; set; }

            public TimeSpan RecordingDuration { get; set; }

            public BaseSonogram Sonogram { get; set; }

            #endregion
        }
    }
}