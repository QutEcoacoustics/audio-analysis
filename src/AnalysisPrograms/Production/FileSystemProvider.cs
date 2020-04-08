// <copyright file="FileSystemProvider.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using log4net;

    /// <summary>
    /// Determine the output format for analysis results.
    /// </summary>
    public static class FileSystemProvider
    {
        public const string DestinationPath =
            "A destination path to write output to. The path should be a directory";

        public const string DestinationFormat = "If empty, will write files. If \"sqlite3\" files will be written to a Sqlite3 database.";

        public const string SqlitePattern = "sqlite3";

        public static readonly string[] AllFormats = { SqlitePattern };

        private static readonly ILog Log = LogManager.GetLogger(typeof(FileSystemProvider));

        public enum Options
        {
            Physical,
            Sqlite,

            // Future formats:
            //Zip
            //HDF5
        }

        /// <summary>
        /// Determine what kind of filesystem to use.
        /// After this point we *should theoretically* not need to use System.IO.Path.
        /// </summary>
        public static (IFileSystem, IDirectoryInfo) DetermineFileSystem(string path, bool readOnly = false)
        {
            var emptyPath = string.IsNullOrWhiteSpace(path);
            var extension = Path.GetExtension(path);

            Log.Debug($"Determining file system for {path}");

            IFileSystem fileSystem;
            IDirectoryInfo baseEntry;

            if (emptyPath)
            {
                fileSystem = new FileSystem();
                baseEntry = fileSystem.DirectoryInfo.FromDirectoryName(Directory.GetCurrentDirectory());
            }
            else
            {
                // resolve path (relative to absolute)
                path = Path.GetFullPath(path);

                if (Directory.Exists(path) || extension == string.Empty)
                {
                    var physicalFileSystem = new FileSystem();
                    var internalPath = path;
                    physicalFileSystem.Directory.CreateDirectory(internalPath);
                    fileSystem = physicalFileSystem;
                    baseEntry = fileSystem.DirectoryInfo.FromDirectoryName(internalPath);
                }
                else
                {
                    // ensure parent directory exists on disk
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    switch (extension)
                    {
                        case "." + SqlitePattern:
                            throw new PlatformNotSupportedException("See https://github.com/QutEcoacoustics/audio-analysis/issues/289");
                            /*
                            fileSystem = new SqliteFileSystem(
                                path,
                                readOnly ? OpenMode.ReadOnly : OpenMode.ReadWriteCreate);
                                */
                            break;
                        default:
                            throw new NotSupportedException(
                                $"Cannot determine file system for given extension {extension}");
                    }

                    // broken!
                    baseEntry = null;
                }
            }

            Log.Debug($"Filesystem for {path} is {fileSystem.GetType().Name}");

            return (fileSystem, baseEntry);
        }

        public static AnalysisIo GetInputOutputFileSystems(string inputPath, string outputPath)
        {
            var input = DetermineFileSystem(inputPath);
            var output = DetermineFileSystem(outputPath);
            return new AnalysisIo(input, output, null);
        }

        public static string MakePath(string directory, string baseName, string format, string tag)
        {
            if (string.IsNullOrEmpty(format))
            {
                return directory;
            }

            Contract.Requires(AllFormats.Contains(format));

            return Path.Combine(directory, FilenameHelpers.AnalysisResultName(baseName, tag, format));
        }
    }
}