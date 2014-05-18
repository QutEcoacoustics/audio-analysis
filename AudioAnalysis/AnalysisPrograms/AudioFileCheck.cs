using Acoustics.Tools.Audio;
using AnalysisPrograms.Production;
using log4net;
using PowerArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AnalysisPrograms
{
    public class AudioFileCheck
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments : IArgClassValidator
        {
            [ArgIgnore]
            public bool TaskIsProcessDirectory
            {
                get
                {
                    return this.InputDirectory != null;
                }
            }

            [ArgDescription("Csv file containing audio file information.")]
            [Production.ArgValidPathName]
            [ArgPosition(1)]
            [ArgRequired]
            public FileInfo OutputFile { get; set; }

            [ArgDescription("Directory containing audio files.")]
            [Production.ArgExistingDirectory]
            public DirectoryInfo InputDirectory { get; set; }

            [ArgDescription("true to recurse into subdirectories (if processing directories).")]
            [DefaultValue(false)]
            public bool Recurse { get; set; }

            [ArgDescription("A text file containing one path per line.")]
            [Production.ArgExistingFile]
            public FileInfo InputFile { get; set; }

            public void Validate()
            {
                // three args: 
                // first is output file path
                // second is whether to recurse or not
                // third is dir containing audio files
                // e.g. AnalysisPrograms.exe "<dir path>" recurse "<output file path>"
                // e.g. AnalysisPrograms.exe "<dir path>" norecurse "<output file path>"
                if (this.TaskIsProcessDirectory)
                {
                    if (this.InputFile != null)
                    {
                        throw new ValidationArgException("Cannot specify an input file if an input directory is given.");
                    }
                }

                // at least two args:
                // first is output file path
                // second is text file path containing audio file paths (one per line)
            }

            public static string Description()
            {
                return "Writes information about audio files to a csv file.";
            }

            public static string AdditionalNotes()
            {
                return "Note: Specify a directory to process or an input file containing file paths, but not both.";
            }
        }

        public static void Execute(Arguments args)
        {
            if (args == null)
            {
                throw new NoDeveloperMethodException();
            }

            IEnumerable<FileInfo> files = null;

            if (args.TaskIsProcessDirectory)
            {
                var shouldRecurse = args.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                files = args.InputDirectory.EnumerateFiles("*.*", shouldRecurse);
            }
            else
            {
                // skip the output file
                files = File.ReadLines(args.InputFile.FullName)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim(' ', '"'))
                    .Distinct()
                    .OrderBy(l => l)
                    .Select(l => new FileInfo(l));
            }

            var mau = new MasterAudioUtility();
            var stopwatch = new Stopwatch();

            var headers = "\"" + string.Join("\", \"",
                "SourceFile",
                "SampleRate (hertz)",
                "BitsPerSecond",
                "BitsPerSample",
                "ChannelCount",
                "Duration (sec)",
                "MediaType",
                "FileSize (bytes)",
                "SHA256 Hash",
                "Identifier") + "\"";

            using (var fs = File.Open(args.OutputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(fs))
            {
                sw.WriteLine(headers);

                foreach (var file in files)
                {
                    try
                    {
                        stopwatch.Restart();
                        var info = mau.Info(file);
                        stopwatch.Stop();

                        var infoTime = stopwatch.Elapsed;

                        stopwatch.Restart();
                        var hash = SHA256Hash(file);
                        stopwatch.Stop();

                        Console.WriteLine("info: {1} hash: {2} for {0}.", file.Name, infoTime, stopwatch.Elapsed);

                        var output = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}",
                            CsvSafeString(info.SourceFile != null ? info.SourceFile.ToString() : string.Empty),
                            CsvSafeString(info.SampleRate.HasValue ? info.SampleRate.Value.ToString() : string.Empty),
                            CsvSafeString(info.BitsPerSecond.HasValue ? info.BitsPerSecond.Value.ToString() : string.Empty),
                            CsvSafeString(info.BitsPerSample.HasValue ? info.BitsPerSample.Value.ToString() : string.Empty),
                            CsvSafeString(info.ChannelCount.HasValue ? info.ChannelCount.Value.ToString() : string.Empty),
                            CsvSafeString(info.Duration.HasValue ? info.Duration.Value.TotalSeconds.ToString() : string.Empty),
                            CsvSafeString(info.MediaType),
                            CsvSafeString(info.SourceFile.Length.ToString()),
                            CsvSafeString(hash),
                            GetIdentifierFromPath(info.SourceFile.FullName)
                            );

                        sw.WriteLine(output);

                        sw.Flush();
                        fs.Flush();
                    }
                    catch (Exception ex)
                    {
                        if (Log.IsWarnEnabled)
                        {
                            Log.Warn("Error processing " + file, ex);
                        }
                    }
                }
            }

        }

        private static string CsvSafeString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = string.Empty;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string SHA256Hash(FileInfo file)
        {
            Contract.Requires(file != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(file.FullName));

            var filePath = file.FullName;

            // see http://stackoverflow.com/questions/1177607/what-is-the-fastest-way-to-create-a-checksum-for-large-files-in-c-sharp/1177744#1177744
            // for info about the buffer size

            // buffer of 1.2gb
            var bufferSize = 1200000;

            //using (FileStream stream = File.OpenRead(filePath))
            using (var stream = new BufferedStream(File.OpenRead(filePath), bufferSize))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLowerInvariant();
            }
        }

        private static Guid GetIdentifierFromPath(string path)
        {
            Contract.Requires(Path.GetFileName(path) != null);

            var fileName = Path.GetFileName(path);
            if (fileName.IndexOf('_') > -1)
            {
                var guid = fileName.Substring(0, fileName.IndexOf('_'));

                Guid result;
                if (Guid.TryParse(guid, out result))
                {
                    return result;
                }
            }

            return Guid.Empty;
        }
    }
}
