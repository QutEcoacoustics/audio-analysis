using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;

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
            if (String.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return new FileInfo(str);
        }        

        public static DirectoryInfo ToDirectoryInfo(this string str)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return new DirectoryInfo(str);
        }
    }
}
