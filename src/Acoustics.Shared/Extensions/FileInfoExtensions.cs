// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileInfoExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.Contracts;
    using JetBrains.Annotations;
    using log4net;

    public static class FileInfoExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FileInfoExtensions));

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

        public static DirectoryInfo Combine(this DirectoryInfo directoryInfo, IEnumerable<string> str)
        {
            Contract.Requires(directoryInfo != null);
            Contract.Requires(str != null);

            string merged = Path.Combine(str.Prepend(directoryInfo.FullName).ToArray());

            return new DirectoryInfo(merged);
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

        public static string CombinePath(this DirectoryInfo directoryInfo, params string[] str)
        {
            Contract.Requires(directoryInfo != null);
            Contract.Requires(str != null && str.Length > 0);

            string merged = Path.Combine(str.Prepend(directoryInfo.FullName).ToArray());

            return merged;
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

        public static DirectoryInfo ToDirectoryInfo(this string str, params string[] subDirectories)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return new DirectoryInfo(Path.Combine(subDirectories.Prepend(str).ToArray()));
        }

        public static bool TryDelete(this FileSystemInfo file, string message = "")
        {
            return TryDelete(file, false, message);
        }

        public static bool TryDelete(this FileSystemInfo file, bool recursive, string message = "")
        {
            try
            {
                if (recursive && file is DirectoryInfo)
                {
                    ((DirectoryInfo)file).Delete(true);
                }
                else
                {
                    file.Delete();
                }

                Log.Debug($"Deleted file {file.FullName}. " + message);
            }
            catch (Exception ex)
            {
                // this error is not fatal, but it does mean we'll be leaving a file behind.
                Log.Warn(
                    $"Attempt to delete {file.FullName} failed." + message,
                    ex);

                return false;
            }

            return true;
        }

        public static bool TryCreate(this DirectoryInfo file)
        {
            if (file == null)
            {
                return false;
            }

            if (Directory.Exists(file.FullName))
            {
                return true;
            }

            try
            {
                file.Create();
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn($"Attempt to create directory {file} failed", ex);
            }

            return false;
        }

        public static FileInfo Touch(this FileInfo info)
        {
            Contract.RequiresNotNull(info);

            Directory.CreateDirectory(info.DirectoryName);

            using (File.OpenWrite(info.FullName))
            {
            }

            info.Refresh();

            return info;
        }

        public static T RefreshInfo<T>(this T info)
            where T : FileSystemInfo
        {
            info.Refresh();
            return info;
        }

        public static FileInfo CopyTo(this FileInfo source, DirectoryInfo dest)
        {
            var result = Path.Combine(dest.FullName, source.Name);
            source.CopyTo(result);
            return result.ToFileInfo();
        }

        public static string BaseName(this FileInfo file)
        {
            return Path.GetFileNameWithoutExtension(file.Name);
        }

        public static string FormatList(this IEnumerable<FileSystemInfo> infos)
        {
            return infos.Select(x => x.FullName).FormatList();
        }

        public static string[] ReadAllLines([NotNull] this FileInfo file) => File.ReadAllLines(file.FullName);
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