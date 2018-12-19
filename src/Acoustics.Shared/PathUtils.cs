// <copyright file="PathUtils.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class PathUtils
    {
        private const int MaxPath = byte.MaxValue;

        private static readonly HashSet<char> UnsafeChars = new HashSet<char>(Path.GetInvalidPathChars());

        /// <summary>
        /// Detects whether the given path has unsafe or unicode characters in it.
        /// </summary>
        /// <remarks>
        /// Searches for control characters, characters above 0x7F,
        /// and any characters in <see cref="Path.GetInvalidPathChars"/>.
        /// </remarks>
        /// <param name="path">The path string to check.</param>
        /// <returns>Returns <value>True</value> if the given path contains unsafe or unicode characters.</returns>
        public static bool HasUnicodeOrUnsafeChars(string path)
        {
            foreach (char c in path)
            {
                if (char.IsControl(c) || c > '~' || UnsafeChars.Contains(c))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the short 8.3 filename for a file.
        /// </summary>
        /// <remarks>
        /// If the current platform is not Windows based this function will simply
        /// return the given input path without modification.
        /// If the supplied path is null or whitespace it will be again returned without
        /// modification.
        /// </remarks>
        /// <param name="path">The path to convert.</param>
        /// <exception cref="FileNotFoundException">If the requested file does not exist</exception>
        /// <returns>An 8.3 filename extracted from kernel32.dll.</returns>
        public static string GetShortFilename(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return path;
            }

            if (!AppConfigHelper.IsWindows)
            {
                return path;
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Can't get a short file name for a file that does not exist",
                    path);
            }

            var shortPath = new StringBuilder(MaxPath);

            path = @"\\?\" + path;

            // https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/Help.cs,208
            // If this is a local path, convert it to a short path name.  Pass 0 as the length the first time
            uint requiredStringSize = GetShortPathName(path, shortPath, 0);
            if (requiredStringSize > 0)
            {
                // It's able to make it a short path.  Happy day.
                shortPath.Capacity = (int)requiredStringSize;
                requiredStringSize = GetShortPathName(path, shortPath, requiredStringSize);
            }

            if (requiredStringSize == 0)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(
                    error,
                    $"The native call to `GetShortPathName` failed with error {error} for input path '{path}'");
            }

            return shortPath.ToString();
        }

        // https://stackoverflow.com/a/258382/224512
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            string path,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder shortPath,
            uint shortPathLength
        );
    }
}