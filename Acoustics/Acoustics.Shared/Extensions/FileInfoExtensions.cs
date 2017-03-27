// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileInfoExtensions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the FileInfoExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared.Contracts;

    public static class FileInfoExtensions
    {
        public static void CreateParentDirectories(this FileInfo file)
        {
            Contract.Requires(file != null);

            if (file.Exists || file.Directory.Exists)
            {
                // It can not exist without all parent directories already existing
                return;
            }
            else
            {
                Directory.CreateDirectory(file.DirectoryName);
            }
        }

        public static DirectoryInfo Combine(this DirectoryInfo directoryInfo, params string[] str)
        {
            Contract.Requires(directoryInfo != null);
            Contract.Requires(str != null && str.Length > 0);

            string merged = Path.Combine(str.Prepend(directoryInfo.FullName).ToArray());

            return new DirectoryInfo(merged);
        }

        public static FileInfo CombineFile(this DirectoryInfo directoryInfo, params string[] str)
        {
            Contract.Requires(directoryInfo != null);
            Contract.Requires(str != null && str.Length > 0);

            string merged = Path.Combine(str.Prepend(directoryInfo.FullName).ToArray());

            return new FileInfo(merged);
        }

        public static FileInfo ToFileInfo(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return new FileInfo(str);
        }

        public static DirectoryInfo ToDirectoryInfo(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return new DirectoryInfo(str);
        }
    }

    public class FileInfoNameComparer : IComparer<FileInfo>, IEqualityComparer<FileInfo>
    {
        public int Compare(FileInfo x, FileInfo y)
        {
            return string.Compare(x.FullName, y.FullName, StringComparison.Ordinal);
        }

        public bool Equals(FileInfo x, FileInfo y)
        {
            return this.Compare(x, y) == 0;
        }

        public int GetHashCode(FileInfo obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}
