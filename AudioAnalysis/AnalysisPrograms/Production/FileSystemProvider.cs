// <copyright file="FileSystemProvider.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using log4net;
    using Zio;
    using Zio.FileSystems;
    using Zio.FileSystems.Community.SqliteFileSystem;

    /// <summary>
    /// Determine the output format for analysis results.
    /// </summary>
    public static class FileSystemProvider
    {
        public const string DestinationFileSystem =
                "A destination path to write output to. The path can be a directory or a file that has an \".sqlite3\" extension.";

        private const string SqlitePattern = ".sqlite3";

        private static readonly ILog Log = LogManager.GetLogger(nameof(FileSystemProvider));

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
        /// <param name="path"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public static IFileSystem DetermineFileSystem(string path, bool readOnly = false)
        {
            var emptyPath = string.IsNullOrWhiteSpace(path);

            Log.Debug($"Determining file system for {path}");

            IFileSystem fileSystem;

            if (emptyPath)
            {
                fileSystem = new PhysicalFileSystem();
            }
            else if (Directory.Exists(path))
            {
                var physicalFileSystem = new PhysicalFileSystem();
                var internalPath = physicalFileSystem.ConvertPathFromInternal(path);
                fileSystem = new SubFileSystem(physicalFileSystem, internalPath);
            }
            else
            {
                var extension = Path.GetExtension(path);

                switch (extension)
                {
                    case SqlitePattern:
                        fileSystem = new SqliteFileSystem(
                            path,
                            readOnly ? OpenMode.ReadOnly : OpenMode.ReadWriteCreate);
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Cannot determine file system for given extension {extension}");
                }
            }

            Log.Debug($"Filesystem for {path} is {fileSystem.GetType().Name}");

            return fileSystem;
        }

        public static AnalysisIo GetInputOutputFileSystems(string inputPath, string outputPath)
        {
            var input = DetermineFileSystem(inputPath);
            var output = DetermineFileSystem(outputPath);
            return new AnalysisIo(input, output, null);
        }
    }

}
