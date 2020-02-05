// <copyright file="ZioExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace Zio
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Acoustics.Shared.Contracts;

    using FileSystems;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Png;

    public static class ZioExtensions
    {
        private static readonly PhysicalFileSystem FileSystem = new PhysicalFileSystem();

        public static UPath ToUPath(this FileSystemInfo file)
        {
            return FileSystem.ConvertPathFromInternal(file.FullName);
        }

        public static string ToOsPath(this UPath path)
        {
            return FileSystem.ConvertPathToInternal(path);
        }

        public static DirectoryEntry ToDirectoryEntry(this DirectoryInfo directory)
        {
            return new DirectoryEntry(FileSystem, directory.ToUPath());
        }

        public static FileEntry ToFileEntry(this FileInfo file)
        {
            return new FileEntry(FileSystem, file.ToUPath());
        }

        public static DirectoryEntry ToDirectoryEntry(this string directory)
        {
            var path = Path.GetFullPath(directory);
            return new DirectoryEntry(FileSystem, FileSystem.ConvertPathFromInternal(path));
        }

        public static FileEntry ToFileEntry(this string file)
        {
            var path = Path.GetFullPath(file);

            return new FileEntry(FileSystem, FileSystem.ConvertPathFromInternal(path));
        }

        public static FileInfo ToFileInfo(this FileEntry file)
        {
            Contract.Requires(file != null);
            Contract.Requires(
                file.FileSystem is PhysicalFileSystem,
                $"To convert the path {file} back to a physical filesystem, it must be from a physical file system");

            return new FileInfo(file.Path.ToOsPath());
        }

        public static DirectoryInfo ToDirectoryInfo(this DirectoryEntry directory)
        {
            Contract.Requires(directory != null);
            Contract.Requires(
                directory.FileSystem is PhysicalFileSystem,
                $"To convert the path {directory} back to a physical filesystem, it must be from a physical file system");

            return new DirectoryInfo(directory.Path.ToOsPath());
        }

        public static DirectoryEntry Combine(this DirectoryEntry directoryInfo, params string[] str)
        {
            Contract.Requires(directoryInfo != null);
            Contract.Requires(str != null && str.Length > 0);

            UPath merged = directoryInfo.Path;
            foreach (var fragment in str)
            {
                merged = merged / fragment;
            }

            return new DirectoryEntry(directoryInfo.FileSystem, merged);
        }

        public static FileEntry CombineFile(this DirectoryEntry directoryInfo, params string[] str)
        {
            Contract.Requires(directoryInfo != null);
            Contract.Requires(str != null && str.Length > 0);

            UPath merged = directoryInfo.Path;
            foreach (var fragment in str)
            {
                merged = merged / fragment;
            }

            return new FileEntry(directoryInfo.FileSystem, merged);
        }

        /// <summary>
        /// Adapter method that converts a DirectoryInfo or FileInfo to a default Zio PhysicalFileSystem and equivalent
        /// UPath.
        /// </summary>
        public static (IFileSystem FileSystem, UPath Path) ToZio(this FileSystemInfo file)
        {
            return (FileSystem, FileSystem.ConvertPathFromInternal(file.FullName));
        }

        /// <summary>
        /// Return a default physical file system that can act as a crutch while we migrate
        /// </summary>
        /// <param name="path">Any UPath. This parameter has no effect.</param>
        /// <returns>A stati</returns>
        public static PhysicalFileSystem DefaultFileSystem(this UPath path)
        {
            return FileSystem;
        }

        public static void Save(this Image bitmap, IFileSystem fileSystem, UPath path)
        {
            var extension = path.GetExtensionWithDot();

            IImageEncoder format;
            switch (extension)
            {
                case ".png":
                    format = new PngEncoder();
                    break;
                default:
                    throw new NotSupportedException();
            }

            using var fileStream = fileSystem.CreateFile(path);
            bitmap.Save(fileStream, format);
        }

        public static StreamReader OpenText(this IFileSystem fileSystem, UPath path)
        {
            using (var stream = fileSystem.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return new StreamReader(stream, Encoding.UTF8, true);
            }
        }

        public static StreamReader OpenText(this FileEntry file)
        {
            var stream = file.FileSystem.OpenFile(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return new StreamReader(stream, Encoding.UTF8, true);
        }
    }
}
