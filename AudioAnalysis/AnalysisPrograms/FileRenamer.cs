namespace AnalysisPrograms
{
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;
    using AnalysisBase;
    using Production;
    using log4net;
    using PowerArgs;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class FileRenamer
    {
        [CustomDescription]
        public class Arguments : IArgClassValidator
        {
            public static string Description()
            {
                return "Renames files based on modified and created date.";
            }

            [ArgDescription("The directory containing audio files.")]
            [Production.ArgExistingDirectory(createIfNotExists: false)]
            [ArgRequired]
            [ArgPosition(1)]
            public virtual DirectoryInfo InputDir { get; set; }

            [ArgDescription("Specify the timezone (e.g. '+1000', '-0700').")]
            [ArgRequired]
            [ArgPosition(2)]
            public string Timezone { get; set; }

            [ArgDescription("Whether to recurse into subfolders (defaults to true).")]
            [DefaultValue(true)]
            public bool Recursive { get; set; }

            [ArgDescription("Only print rename actions, don't actually rename files (defaults to true).")]
            [DefaultValue(true)]
            public bool DryRun { get; set; }

            public void Validate()
            {
                if (this.Timezone.Length != 5)
                {
                    throw new ArgumentException("Timezone must be exactly 5 characters long.");
                }

                var validPlusMinus = new[] { '+', '-' };
                if (!validPlusMinus.Contains(this.Timezone[0]))
                {
                    throw new ArgumentException("Timezone must start with '+' or '-'.");
                }

                var validChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                foreach (var item in this.Timezone.Substring(1))
                {
                    if (!validChars.Contains(item))
                    {
                        throw new ArgumentException("Timezone must contain only digits after initial '+' or '-'.");
                    }
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            var validExtensions = new[] { ".wav", ".mp3", ".wv", ".ogg", ".wma" };
            var searchOption = arguments.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var dir = arguments.InputDir;
            var searchpattern = "*.*";

            var files = dir
                .EnumerateFiles(searchpattern, searchOption)
                .Where(f => validExtensions.Contains(f.Extension.ToLowerInvariant()))
                .OrderBy(f => f.Name)
                .ToArray();

            var parsedTimeZone =
                DateTimeOffset.Parse(
                    new DateTime(DateTime.Now.Ticks, DateTimeKind.Unspecified).ToString("O") + arguments.Timezone)
                    .Offset;
            var newFileNames = StartParallel(files, arguments.DryRun, parsedTimeZone);

            // print out mapping of original file name to new file name
            // only include file names that have changed
            for (var i = 0; i < newFileNames.Length; i++)
            {
                var originalName = files[i].FullName;
                var newName = newFileNames[i];

                if (originalName != newName)
                {
                    Log.InfoFormat("{0}, {1}", originalName, newName);
                }
            }

            Log.Info("Finished.");
        }

        /// <summary>
        /// Determine new files names and rename if not a dry run.
        /// </summary>
        /// <param name="files">Array of files.</param>
        /// <param name="isDryRun">Dry run or not.</param>
        /// <param name="timezone">Timezone string to use.</param>
        /// <returns>Array of file names in same order.</returns>
        private static string[] StartParallel(FileInfo[] files, bool isDryRun, TimeSpan timezone)
        {
            var count = files.Count();
            var results = new string[count];

            Parallel.ForEach(
                files,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (item, state, index) =>
                {
                    var item1 = item;
                    var index1 = index;

                    var fileName = item1.Name;

                    var isDateTimeInFileName = FileDateHelpers.FileNameContainsDateTime(fileName);

                    if (isDateTimeInFileName)
                    {
                        results[index1] = item1.FullName;
                    }
                    else
                    {
                        results[index1] = Path.Combine(item1.DirectoryName, GetNewName(item1, timezone));

                        if (!isDryRun)
                        {
                            File.Move(item1.FullName, results[index1]);
                        }
                    }
                });

            return results;
        }

        private static string GetNewName(FileInfo file, TimeSpan timezone)
        {
            var fileName = file.Name;
            var fileLength = file.Length;
            var lastModified = file.LastWriteTime;
            var mediaType = MediaTypes.GetMediaType(file.Extension);

            var audioUtility = new MasterAudioUtility();
            var info = audioUtility.Info(file);
            var duration = info.Duration.HasValue ? info.Duration.Value : TimeSpan.Zero;

            var recordingStart = lastModified - duration;

            // some tweaking to get nice file names - round the minutes of last mod and duration
            // ticks are in 100-nanosecond intervals

            //var modifiedRecordingStart = lastModified.Round(TimeSpan.FromSeconds(15))
            //                             - duration.Round(TimeSpan.FromSeconds(15));

            //// DateTime rounded = new DateTime(((now.Ticks + 25000000) / 50000000) * 50000000);

            ////var roundedTotalSeconds = Math.Round(mediaFile.RecordingStart.TimeOfDay.TotalSeconds);
            ////var modifiedRecordingStart = mediaFile.RecordingStart.Date.AddSeconds(roundedTotalSeconds);

            var dateWithOffset = new DateTimeOffset(recordingStart, timezone);
            var dateTime = dateWithOffset.ToUniversalTime().ToString(AppConfigHelper.StandardDateFormatUtc);
            var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLowerInvariant();

            var prefix = fileName.Substring(0, fileName.LastIndexOf('.'));
            var result = string.Format("{0}_{1}.{2}", prefix, dateTime, ext);

            return result;
        }
    }
}
