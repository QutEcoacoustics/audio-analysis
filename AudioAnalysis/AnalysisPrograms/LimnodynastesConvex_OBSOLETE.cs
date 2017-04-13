// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LimnodynastesConvex_OBSOLETE.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using Production;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    //using Emgu.CV.UI;
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
    [Obsolete]
    public class LimnodynastesConvex_OBSOLETE : AbstractStrongAnalyser
    {
        #region Constants

        public const string AnalysisName = "LimnodynastesConvex_OBSOLETE";

        public static readonly int ResampleRate = AppConfigHelper.DefaultTargetSampleRate;

        #endregion

        #region Public Properties

        public override AnalysisSettings DefaultSettings => new AnalysisSettings
            {
                SegmentMaxDuration = TimeSpan.FromMinutes(1),
                SegmentMinDuration = TimeSpan.FromSeconds(30),
                SegmentMediaType = MediaTypes.MediaTypeWav,
                SegmentOverlapDuration = TimeSpan.Zero,
                SegmentTargetSampleRate = ResampleRate,
            };

        public override string DisplayName => "Limnodynastes convex";

        public static string abbreviatedName = "LimCon";

        public override string Identifier => "Towsey." + AnalysisName;

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
                const string OutputDir = @"C:\SensorNetworks\Output\Frogs\LimnodynastesConvex\";

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
                    throw new NotSupportedException("YOU CAN'T DO THIS!");
                    ////var process = new ProcessRunner(ImageViewer);
                    ////process.Run(image.FullName, arguments.Output.FullName);
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
                    new AudioUtilityRequest { TargetSampleRate = ResampleRate },
                    analysisSettings.AnalysisBaseTempDirectoryChecked);
            }
            else
            {
                AudioFilePreparer.PrepareFile(
                    arguments.Source,
                    tempF,
                    new AudioUtilityRequest
                        {
                            TargetSampleRate = ResampleRate,
                            OffsetStart = start,
                            OffsetEnd = start.Add(duration),
                        },
                    analysisSettings.AnalysisBaseTempDirectoryChecked);
            }

            // DO THE ANALYSIS
            /* ############################################################################################################################################# */
            IAnalyser2 analyser = new LimnodynastesConvex_OBSOLETE();
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
            LimnodynastesConvexResults results = Analysis(audioFile, configuration, analysisSettings);

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

            if (analysisSettings.SegmentSaveBehavior.ShouldSave(analysisResults.Events.Length))
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

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
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
        internal static LimnodynastesConvexResults Analysis(FileInfo segmentOfSourceFile, Dictionary<string, string> configDict, AnalysisSettings analysisSettings)
        {
            Dictionary<string, double[,]> dictionaryOfHiResSpectralIndices = null;
            var recording = new AudioRecording(segmentOfSourceFile.FullName);
            return Analysis(dictionaryOfHiResSpectralIndices, recording, configDict, analysisSettings);
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
        /// The <see cref="LimnodynastesConvexResults"/>.
        /// </returns>
        internal static LimnodynastesConvexResults Analysis(
            Dictionary<string, double[,]> dictionaryOfHiResSpectralIndices,
            AudioRecording recording,
            Dictionary<string, string> configDict,
            AnalysisSettings analysisSettings)
        {
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

            var outputDir = analysisSettings.AnalysisInstanceOutputDirectory;
            TimeSpan segmentStartOffset = analysisSettings.SegmentStartOffset ?? TimeSpan.Zero;


            //KeyValuePair<string, double[,]> kvp = dictionaryOfHiResSpectralIndices.First();
            var spg = dictionaryOfHiResSpectralIndices["RHZ"];
            int rhzRowCount = spg.GetLength(0);
            int rhzColCount = spg.GetLength(1);

            int sampleRate = recording.SampleRate;
            double herzPerBin = sampleRate / 2 / (double)rhzRowCount;
            double scoreThreshold = (double?)double.Parse(configDict["EventThreshold"]) ?? 3.0;
            int minimumFrequency  = (int?)int.Parse(configDict["MinHz"]) ?? 850;
            int dominantFrequency = (int?)int.Parse(configDict["DominantFrequency"]) ?? 1850;

            // # The Limnodynastes call has three major peaks. The dominant peak is at 1850 or as set above.
            // # The second and third peak are at equal gaps below. DominantFreq-gap and DominantFreq-(2*gap);
            // # Set the gap in the Config file. Should typically be in range 880 to 970
            int peakGapInHerz     = (int?)int.Parse(configDict["PeakGap"]) ?? 470;
            int F1AndF2Gap = (int)Math.Round(peakGapInHerz / herzPerBin);
            //int F1AndF2Gap = 10; // 10 = number of freq bins
            int F1AndF3Gap = 2 * F1AndF2Gap;
            //int F1AndF3Gap = 20;

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
                double maxAmplitude = -double.MaxValue;
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
                list.Add(new Point(c, freqBinID));
                // we now have a list of potential hits for LimCon. This needs to be filtered.

                // Console.WriteLine("Col {0}, Bin {1}  ", c, freqBinID);
            }

            // DEBUG ONLY // ################################ TEMPORARY ################################
            // superimpose point on RHZ HiRes spectrogram for debug purposes
            bool drawOnHiResSpectrogram = true;
            //string filePath = @"G:\SensorNetworks\Output\Frogs\TestOfHiResIndices-2016July\Test\Towsey.HiResIndices\SpectrogramImages\3mile_creek_dam_-_Herveys_Range_1076_248366_20130305_001700_30_0min.CombinedGreyScale.png";
            var fileName = Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name);
            string filePath = outputDir.FullName + @"\SpectrogramImages\" + fileName + ".CombinedGreyScale.png";
            var debugImage = new FileInfo(filePath);
            if (!debugImage.Exists) drawOnHiResSpectrogram = false;
            if (drawOnHiResSpectrogram)
            {
                // put red dot where max is
                Bitmap bmp = new Bitmap(filePath);
                foreach (Point point in list)
                {
                    bmp.SetPixel(point.X + 70, 1911 - point.Y, Color.Red);
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
                string opFilePath = outputDir.FullName + @"\SpectrogramImages\" + fileName + ".CombinedGreyScaleAnnotated.png";
                bmp.Save(opFilePath);
            }
            // END DEBUG ################################ TEMPORARY ################################


            // now construct the standard decibel spectrogram WITHOUT noise removal, and look for LimConvex
            // get frame parameters for the analysis
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            int frameSize = rhzRowCount * 2;
            int frameStep = frameSize; // this default = zero overlap
            double frameDurationInSeconds = frameSize / (double)sampleRate;
            double frameStepInSeconds     = frameStep / (double)sampleRate;
            double framesPerSec = 1 / frameStepInSeconds;
            //var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording, frameSize, frameStep);
            //// Generate deciBel spectrogram
            //double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(dspOutput.amplitudeSpectrogram, dspOutput.WindowPower, sampleRate, epsilon);

            // i: Init SONOGRAM config
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = 0.0,
                NoiseReductionType = NoiseReductionType.None,
            };
            // init sonogram
            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // remove the DC row of the spectrogram
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            //scores.Add(new Plot("Decibels", DataTools.normalise(dBArray), ActivityAndCover.DefaultActivityThresholdDb));
            //scores.Add(new Plot("Active Frames", DataTools.Bool2Binary(activity.activeFrames), 0.0));

            // convert spectral peaks to frequency
            //var tuple_DecibelPeaks = SpectrogramTools.HistogramOfSpectralPeaks(deciBelSpectrogram);
            //int[] peaksBins = tuple_DecibelPeaks.Item2;
            //double[] freqPeaks = new double[peaksBins.Length];
            //int binCount = sonogram.Data.GetLength(1);
            //for (int i = 1; i < peaksBins.Length; i++) freqPeaks[i] = (lowerBinBound + peaksBins[i]) / (double)nyquistBin;
            //scores.Add(new Plot("Max Frequency", freqPeaks, 0.0));  // location of peaks for spectral images

            // create new list of LimCon hits in the standard spectrogram.
            double timeSpanOfFrameInSeconds = frameSize / (double)sampleRate;
            var newList = new List<int[]>();
            int lastFrameID = sonogram.Data.GetLength(0) - 1;
            int lastBinID   = sonogram.Data.GetLength(1) - 1;

            foreach (Point point in list)
            {
                double secondsFromStartOfSegment = (point.X * 0.1) + 0.05; // convert point.Y to center of time-block.
                int framesFromStartOfSegment = (int)Math.Round(secondsFromStartOfSegment / timeSpanOfFrameInSeconds);

                // location of max point is uncertain, so search in neighbourhood.
                // NOTE: sonogram.data matrix is time*freqBin
                double maxValue = -double.MaxValue;
                int idOfTMax = framesFromStartOfSegment;
                int idOfFMax = point.Y;
                for (int deltaT = -4; deltaT <= 4; deltaT++)
                {
                    for (int deltaF = -1; deltaF <= 1; deltaF++)
                    {
                        int newT = framesFromStartOfSegment + deltaT;
                        if (newT < 0)
                        {
                            newT = 0;
                        }
                        else if (newT > lastFrameID)
                        { newT = lastFrameID; }

                        double value = sonogram.Data[newT, point.Y + deltaF];
                        if (value > maxValue)
                        {
                            maxValue = value;
                            idOfTMax = framesFromStartOfSegment + deltaT;
                            idOfFMax = point.Y   + deltaF;
                        }
                    }
                }

                // newList.Add(new Point(frameSpan, point.Y));
                int[] array = new int[2];
                array[0] = idOfTMax;
                array[1] = idOfFMax;
                newList.Add(array);
            }

            // Now obtain more of spectrogram to see if have peaks at two other places characteristic of Limnodynastes convex.
            // In the D.Stewart CD, there are peaks close to:
            //1. 1950 Hz
            //2. 1460 hz
            //3.  970 hz    These are 490 Hz apart.
            // For Limnodynastes convex, in the JCU recording, there are peaks close to:
            //1. 1780 Hz
            //2. 1330 hz
            //3.  880 hz    These are 450 Hz apart.

            // So strategy is to look for three peaks separated by same amount and in the vicinity of the above,
            //  starting with highest power (the top peak) and working down to lowest power (bottom peak).
            //We have found top/highest peak - now find the other two.
            int secondDominantFrequency = 1380;
            int secondDominantBin = (int)Math.Round(secondDominantFrequency / herzPerBin);
            int thirdDominantFrequency = 900;
            int thirdDominantBin       = (int)Math.Round(thirdDominantFrequency / herzPerBin);

            var acousticEvents = new List<AcousticEvent>();
            int Tbuffer = 2;
            // First extract a sub-matrix.
            foreach (int[] array in newList)
            {
                // NOTE: sonogram.data matrix is time*freqBin
                int Tframe = array[0];
                int F1bin = array[1];
                double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, Tframe - Tbuffer, 0, Tframe + Tbuffer, F1bin);
                double F1power = subMatrix[Tbuffer, F1bin];
                // convert to vector
                var spectrum = MatrixTools.GetColumnsAverages(subMatrix);

                // use the following code to get estimate of background noise
                double[,] powerMatrix = MatrixTools.Submatrix(sonogram.Data, Tframe - 3, 10, Tframe + 3, F1bin);
                double averagePower = (MatrixTools.GetRowAverages(powerMatrix)).Average();
                double score = F1power - averagePower;

                // debug - checking what the spectrum looks like.
                //for (int i = 0; i < 18; i++)
                //    spectrum[i] = -100.0;
                //DataTools.writeBarGraph(spectrum);

                // locate the peaks in lower frequency bands, F2 and F3
                bool[] peaks = DataTools.GetPeaks(spectrum);


                int F2bin = 0;
                double F2power = -200.0; // dB
                for (int i = -3; i <= 2; i++)
                {
                    int bin = F1bin - F1AndF2Gap + i;
                    if ((peaks[bin])&&(F2power < subMatrix[1, bin]))
                    {
                        F2bin = bin;
                        F2power = subMatrix[1, bin];
                    }
                }
                if (F2bin == 0) continue;
                if (F2power == -200.0) continue;
                score += (F2power - averagePower);

                int F3bin = 0;
                double F3power = -200.0;
                for (int i = -5; i <= 2; i++)
                {
                    int bin = F1bin - F1AndF3Gap + i;
                    if ((peaks[bin]) && (F3power < subMatrix[1, bin]))
                    {
                        F3bin = bin;
                        F3power = subMatrix[1, bin];
                    }
                }
                if (F3bin == 0) continue;
                if (F3power == -200.0) continue;

                score += (F3power - averagePower);
                score /= 3;

                // ignore events where SNR < decibel threshold
                if (score < scoreThreshold) continue;

                // ignore events with wrong power distribution. A good LimnoConvex call has strongest F1 power
                if ((F3power > F1power) || (F2power > F1power)) continue;

                //freq Bin ID must be converted back to Matrix row ID
                //  freqBin + rowID = binCount - 1;
                // therefore: rowID = binCount - freqBin - 1;
                minRowID = rhzRowCount - F1bin - 2;
                maxRowID = rhzRowCount - F3bin - 1;
                int F1RowID  = rhzRowCount - F1bin - 1;
                int F2RowID = rhzRowCount - F2bin - 1;
                int F3RowID = rhzRowCount - F3bin - 1;

                int maxfreq = dominantFrequency + hzBuffer;
                int topBin = (int)Math.Round(maxfreq / herzPerBin);
                int frameCount = 4;
                double duration = frameCount * frameStepInSeconds;
                double startTimeWrtSegment = (Tframe - 2) * frameStepInSeconds;

                // Got to here so start initialising an acoustic event
                var ae = new AcousticEvent(startTimeWrtSegment, duration, minimumFrequency, maxfreq);
                ae.SetTimeAndFreqScales(framesPerSec, herzPerBin);
                //var ae = new AcousticEvent(oblong, recording.Nyquist, binCount, frameDurationInSeconds, frameStepInSeconds, frameCount);
                //ae.StartOffset = TimeSpan.FromSeconds(Tframe * frameStepInSeconds);


                var pointF1 = new Point(2, topBin - F1bin);
                var pointF2 = new Point(2, topBin - F2bin);
                var pointF3 = new Point(2, topBin - F3bin);
                ae.Points = new List<Point>();
                ae.Points.Add(pointF1);
                ae.Points.Add(pointF2);
                ae.Points.Add(pointF3);
                //tried using HitElements but did not do what I wanted later on.
                //ae.HitElements = new HashSet<Point>();
                //ae.HitElements = new SortedSet<Point>();
                //ae.HitElements.Add(pointF1);
                //ae.HitElements.Add(pointF2);
                //ae.HitElements.Add(pointF3);
                ae.Score = score;
                //ae.MinFreq = Math.Round((topBin - F3bin - 5) * herzPerBin);
                //ae.MaxFreq = Math.Round(topBin * herzPerBin);
                acousticEvents.Add(ae);
            }

            // now add in extra common info to the acoustic events
            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = configDict[AnalysisKeys.SpeciesName];
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recording.Duration();
                ae.Name = abbreviatedName;
                ae.BorderColour = Color.Red;
                ae.FileName = recording.BaseName;
            });

            double[] scores = new double[rhzColCount]; // predefinition of score array
            double nomalisationConstant = scoreThreshold * 4; // four times the score threshold
            double compressionFactor = rhzColCount / (double)sonogram.Data.GetLength(0);
            foreach (AcousticEvent ae in acousticEvents)
            {
                ae.ScoreNormalised = ae.Score / nomalisationConstant;
                if (ae.ScoreNormalised > 1.0) ae.ScoreNormalised = 1.0;
                int frameID = (int)Math.Round(ae.EventStartSeconds / frameDurationInSeconds);
                int hiresFrameID = (int)Math.Floor(frameID * compressionFactor);
                scores[hiresFrameID] = ae.ScoreNormalised;
            }
            var plot = new Plot(AnalysisName, scores, scoreThreshold);


            // DEBUG ONLY ################################ TEMPORARY ################################
            // Draw a standard spectrogram and mark of hites etc.
            bool createStandardDebugSpectrogram = true;

            var imageDir = new DirectoryInfo(outputDir.FullName + @"\SpectrogramImages");
            if (!imageDir.Exists) imageDir.Create();
            if (createStandardDebugSpectrogram)
            {
                var fileName2 = Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name);
                string filePath2 = Path.Combine(imageDir.FullName, fileName + ".Spectrogram.png");
                Bitmap sonoBmp = (Bitmap)sonogram.GetImage();
                int height = sonoBmp.Height;
                foreach (AcousticEvent ae in acousticEvents)
                {
                    ae.DrawEvent(sonoBmp);
                    //g.DrawRectangle(pen, ob.ColumnLeft, ob.RowTop, ob.ColWidth-1, ob.RowWidth);
                    //ae.DrawPoint(sonoBmp, ae.HitElements.[0], Color.OrangeRed);
                    //ae.DrawPoint(sonoBmp, ae.HitElements[1], Color.Yellow);
                    //ae.DrawPoint(sonoBmp, ae.HitElements[2], Color.Green);
                    ae.DrawPoint(sonoBmp, ae.Points[0], Color.OrangeRed);
                    ae.DrawPoint(sonoBmp, ae.Points[1], Color.Yellow);
                    ae.DrawPoint(sonoBmp, ae.Points[2], Color.LimeGreen);
                }

                // draw the original hits on the standard sonogram
                foreach (int[] array in newList)
                {
                    sonoBmp.SetPixel(array[0], height - array[1], Color.Cyan);
                }

                // mark off every tenth frequency bin on the standard sonogram
                for (int r = 0; r < 20; r++)
                {
                    sonoBmp.SetPixel(0, height - (r * 10) - 1, Color.Blue);
                    sonoBmp.SetPixel(1, height - (r * 10) - 1, Color.Blue);
                }
                // mark off upper bound and lower frequency bound
                sonoBmp.SetPixel(0, height - dominantBinMin, Color.Lime);
                sonoBmp.SetPixel(0, height - dominantBinMax, Color.Lime);
                sonoBmp.Save(filePath2);
            }
            // END DEBUG ################################ TEMPORARY ################################


            return new LimnodynastesConvexResults
                       {
                           Sonogram = sonogram,
                           Hits = null,
                           Plot = plot,
                           Events = acousticEvents,
                           RecordingDuration = recording.Duration(),
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

        public class LimnodynastesConvexResults
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