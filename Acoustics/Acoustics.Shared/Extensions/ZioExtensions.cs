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
    using FileSystems;

    public static class ZioExtensions
    {
        private static readonly PhysicalFileSystem FileSystem = new PhysicalFileSystem();

        public static UPath ToUPath(this FileSystemInfo file)
        {
            return FileSystem.ConvertPathFromInternal(file.FullName);
        }

        public static DirectoryEntry ToDirectoryEntry(this DirectoryInfo directory)
        {
            return new DirectoryEntry(FileSystem, directory.ToUPath());
        }

        public static DirectoryEntry ToFileEntry(this FileInfo file)
        {
            return new DirectoryEntry(FileSystem, file.ToUPath());
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

        public static void Save(this System.Drawing.Bitmap bitmap, IFileSystem fileSystem, UPath path)
        {
            var extension = path.GetExtensionWithDot();

            ImageFormat format;
            switch (extension)
            {
                case ".png":
                    format = ImageFormat.Png;
                    break;
                default:
                    throw new NotSupportedException();
            }

            using (var fileStream = fileSystem.CreateFile(path))
            {
                bitmap.Save(fileStream, format);
            }
        }
    }
}
