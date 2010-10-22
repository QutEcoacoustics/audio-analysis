namespace AnalysisPrograms.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AudioTools;
    using AudioTools.AudioUtlity;

    using QutSensors.Shared.Tools;

    /// <summary>
    /// Analyse local audio files.
    /// </summary>
    public class LocalProcessor
    {
        private readonly IAudioUtility audioUtility;

        private readonly FileInfo parameterFile;

        private readonly string analysisType;

        private readonly FileInfo resourceFile;

        private readonly ISegmenter segmenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalProcessor"/> class. 
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// Could not find parameter file.
        /// </exception>
        public LocalProcessor()
        {
            string assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string ffmpegExe = ConfigurationManager.AppSettings["AudioUtilityFfmpegExe"];
            string wvunpackExe = ConfigurationManager.AppSettings["AudioUtilityWvunpackExe"];
            string mp3SpltExe = ConfigurationManager.AppSettings["AudioUtilityMp3SpltExe"];


            this.audioUtility = new CombinedAudioUtility(
                Path.Combine(assemblyDir, wvunpackExe),
                Path.Combine(assemblyDir, ffmpegExe),
                Path.Combine(assemblyDir, mp3SpltExe));

            this.parameterFile = new FileInfo(ConfigurationManager.AppSettings["LocalParameterFile"]);
            if (!File.Exists(this.parameterFile.FullName))
            {
                throw new FileNotFoundException("Could not find parameter file.", this.parameterFile.FullName);
            }

            string resourceFileString = ConfigurationManager.AppSettings["LocalResourceFile"];
            if (!string.IsNullOrEmpty(resourceFileString))
            {
                this.resourceFile = new FileInfo(resourceFileString);
                if (!File.Exists(this.resourceFile.FullName))
                {
                    throw new FileNotFoundException("Could not find resource file.", this.resourceFile.FullName);
                }
            }

            this.analysisType = ConfigurationManager.AppSettings["LocalAnalysisType"];



            this.segmenter = new Segmenter();
        }

        /// <summary>
        /// Run analysis.
        /// </summary>
        public void Run()
        {
            string dir = ConfigurationManager.AppSettings["LocalAnalyseDir"];
            bool recurse = bool.Parse(ConfigurationManager.AppSettings["LocalRecurse"]);

            this.Run(new DirectoryInfo(dir), recurse);
        }

        /// <summary>
        /// Analyse a directory.
        /// </summary>
        /// <param name="dir">
        /// The directory.
        /// </param>
        /// <param name="recurse">
        /// Recurse to subfolders.
        /// </param>
        public void Run(DirectoryInfo dir, bool recurse)
        {
            var files = new List<string>();

            files.AddRange(recurse
                ? GetSubDirFiles(dir).Select(f => f.FullName)
                : dir.GetFiles().Select(f => f.FullName));

            foreach (var file in files)
            {
                AnalyseAudioFile(new FileInfo(file));
            }
        }

        private static IEnumerable<FileInfo> GetSubDirFiles(DirectoryInfo dir)
        {
            var files = new List<FileInfo>();

            foreach (var subDir in dir.GetDirectories())
            {
                files.AddRange(subDir.GetFiles());
                files.AddRange(GetSubDirFiles(subDir));
            }

            files.AddRange(dir.GetFiles());

            return files;
        }

        private void RunAudioFileSegment(FileInfo file, string fileMimeType, Range<TimeSpan> segment)
        {
            Console.WriteLine();
            Console.WriteLine("Analysing " + file.FullName);
            Console.WriteLine("Segment: " + segment.Minimum.ToReadableString() + " - " + segment.Maximum.ToReadableString());
            Console.WriteLine();

            // create run dir
            string fileDir = Path.GetDirectoryName(file.FullName);
            DirectoryInfo runDir = Directory.CreateDirectory(Path.Combine(fileDir, Guid.NewGuid().ToString()));

            // segment audio file to run dir
            string audioFileSegment = Path.Combine(runDir.FullName, ProcessingUtils.AudioFileName);
            audioUtility.Segment(file, fileMimeType, new FileInfo(audioFileSegment), MimeTypes.MimeTypeWav, segment.Minimum, segment.Maximum);

            // copy settings file to run dir
            string paramFile = Path.Combine(runDir.FullName, ProcessingUtils.SettingsFileName);
            File.Copy(this.parameterFile.FullName, paramFile);


            var results = ProcessingUtils.RunAnalysis(
                analysisType, runDir.FullName, this.resourceFile == null ? null : this.resourceFile.FullName);

            // save results as csv in same dir as original audio file
            var sb = new StringBuilder();
            sb.AppendLine("Start time, End time, Duration, Min freq, Max freq, Normalised Score, Extra Detail");

            foreach (var item in results)
            {
                sb.AppendLine(
                    string.Format(
                    "{0}, {1}, {2}, {3}, {4}, {5}, {6}",
                    item.StartTime,
                    item.EndTime,
                    item.EndTime - item.StartTime,
                    item.MinFrequency,
                    item.MaxFrequency,
                    item.NormalisedScore == null ? "no score" : item.NormalisedScore.ToString(),
                    item.ExtraDetail == null ? "no extra detail" : string.Join(" || ", item.ExtraDetail.Select(r => r.ToString()).ToArray())
                    ));
            }

            string resultsFileName = Path.GetFileName(file.Name) + DateTime.Now.ToString("_yyyyMMdd-HHmmss") + "_" +
                                     segment.Minimum.ToReadableString().Replace(' ', '_') + "--" +
                                     segment.Maximum.ToReadableString().Replace(' ', '_');

            File.WriteAllText(Path.Combine(fileDir, resultsFileName + ".csv"), sb.ToString());


            string spectrogramImage = Path.Combine(
                runDir.FullName, Path.GetFileNameWithoutExtension(ProcessingUtils.AudioFileName) + ".png");

            if (File.Exists(spectrogramImage))
            {
                // save results as image
                // uncomment 'SaveAe' method in 'ProcessingTypes'
                File.Copy(spectrogramImage, Path.Combine(fileDir, resultsFileName + ".png"));
            }

            // delete run dir
            if (Directory.Exists(runDir.FullName))
            {
                try
                {
                    Directory.Delete(runDir.FullName, true);
                }
                catch
                {
                }
            }
        }

        private void AnalyseAudioFile(FileInfo file)
        {
            string fileMimeType = MimeTypes.GetMimeTypeFromExtension(file.Extension);

            // 1. get duration - this also ensures the file is an audio file.
            TimeSpan duration;

            try
            {
                // get the file duration
                duration = audioUtility.Duration(file, fileMimeType);
            }
            catch
            {
                // if cannot get audio duration, return
                Console.WriteLine();
                Console.WriteLine("Could not get duration for file. Is it an audio file? " + file.FullName);
                Console.WriteLine();
                return;
            }

            // 2. split up long files
            var segments =
                segmenter.CreateSegments(
                    new Range<TimeSpan> { Minimum = TimeSpan.Zero, Maximum = duration },
                    duration,
                    TimeSpan.FromMinutes(2),
                    TimeSpan.FromSeconds(30),
                    false);

            // 3. analyse each segment
            foreach (var segment in segments)
            {
                RunAudioFileSegment(file, fileMimeType, segment);
            }
        }
    }
}
