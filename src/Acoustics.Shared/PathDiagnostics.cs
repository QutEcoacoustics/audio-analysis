// <copyright file="PathDiagnostics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    // adapted from: https://github.com/atruskie/path-diagnostics/blob/master/Trace-Path.ps1
    // copyright Anthony Truskinger 2019

    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using static System.IO.Path;

    /// <summary>
    /// Shows where how many segments of a faulty path are valid.
    /// </summary>
    public static class PathDiagnostics
    {
        /// <summary>
        /// Checks if a path exists. If it does not it generates a report detailing
        /// what aspects of the path do exist and suggests possible alternatives.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="report">
        /// A report that details errors found in the path if the <paramref name="path"/> does not exist.
        /// </param>
        /// <param name="root">
        /// An optional root to apply to a relative path.
        /// Defaults to <see cref="Environment.CurrentDirectory"/> is <paramref name="root"/> is null.
        /// </param>
        /// <returns>True if the path exists.</returns>
        /// <exception cref="ArgumentException">
        /// If the supplied <paramref name="root"/> is not itself rooted.
        /// </exception>
        public static bool PathExistsOrDiff(string path, out PathDiffReport report, string root = null)
        {
            report = new PathDiffReport();

            if (string.IsNullOrEmpty(path))
            {
                report.Messages.AppendLine("Supplied path was null or empty");
                return false;
            }

            if (!IsPathRooted(path))
            {
                if (root != null)
                {
                    if (IsPathRooted(root))
                    {
                        path = Join(root, path);
                    }
                    else
                    {
                        throw new ArgumentException("Argument `root` must be fully rooted (i.e. absolute)", nameof(root));
                    }
                }
                else
                {
                    path = Join(Environment.CurrentDirectory, path);
                }
            }

            // optimal cases
            if (File.Exists(path))
            {
                report.Found = new FileInfo(path);
                return true;
            }

            if (Directory.Exists(path))
            {
                report.Found = new DirectoryInfo(path);
                return true;
            }

            report.Messages.AppendLine($"`{path}` does not exist");

            // the rest of this is just trying to determine how much of the path does exist
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var fragments = path.Split(DirectorySeparatorChar);

            var lastPart = fragments.Length == 0 ? path : fragments[^1];
            var spaceAtEnd = false;

            var builtUpPath = string.Empty;
            var slash = DirectorySeparatorChar.ToString();
            for (int f = 0; f < fragments.Length; f++)
            {
                var delimiter = f == 0 ? string.Empty : slash;
                var fragment = fragments[f];

                // skip checking if unix root exists
                if (!isWindows && f == 0 && fragment == string.Empty)
                {
                    continue;
                }

                var testPath = builtUpPath + delimiter + fragment;
                var endsWithSpaces = EndsWithSpaces(testPath);

                if (File.Exists(testPath) || Directory.Exists(testPath))
                {
                    // the .NET framework normalizes paths with trailing spaces by stripping
                    // the trailing spaces. It only does it in some cases though, which
                    // means if there is a directory with a trailing space, and another
                    // fragment after it, the path will not resolve, which is one use case
                    // for Trace-path
                    if (endsWithSpaces && isWindows)
                    {
                        lastPart = fragment;
                        spaceAtEnd = true;
                        break;
                    }

                    builtUpPath = testPath;
                }
                else if (EndsWithSpaces(testPath) && isWindows)
                {
                    // sometimes though, there are actually spaces in the path!
                    // we can't detect if this case is actually real though.

                    builtUpPath = testPath;
                }
                else if (EndsWithSpaces(testPath) && !isWindows)
                {
                    // does not exist, but has spaces in the folder name
                    spaceAtEnd = true;
                    lastPart = fragment;

                    break;
                }
                else
                {
                    lastPart = fragment;
                    break;
                }
            }

            var finalTestPath = builtUpPath;
            string lastFragment;
            if (spaceAtEnd)
            {
                lastFragment = lastPart.TrimEnd(' ');
                finalTestPath = builtUpPath + slash + lastFragment;
            }
            else
            {
                var searchPath = builtUpPath + slash;

                // reverse search here -i've found often the mistake is closer to the end of the file name
                // and if it is, there are way fewer matching files that could be returned as possible suggestions
                // (meaning a better auto suggest).
                int l;
                for (l = lastPart.Length; l > 0; l--)
                {
                    lastFragment = lastPart.Substring(0, l);

                    var hasResults = Directory.EnumerateFileSystemEntries(searchPath, $"{lastFragment}*")
                        .GetEnumerator()
                        .MoveNext();

                    if (hasResults)
                    {
                        finalTestPath = builtUpPath + slash + lastFragment;
                        break;
                    }

                    // continue
                }

                // nothing in fragment matched anything in the directory
                // but we want to suggest results inside the directory, so add the the directory delimiter
                if (l == 0)
                {
                    finalTestPath = builtUpPath + slash;
                }
            }

            // i.e. everything in original path that we could not match
            var rest = path.Substring(finalTestPath.Length).NormalizeDirectorySeparators();

            // index of last good character - and index of first bad character
            int goodIndex = finalTestPath.Length - 1;
            int errorIndex = finalTestPath.Length - 1 + 1;

            // where to put indicator arrows
            int goodColumn = goodIndex + 1;
            int errorColumn = errorIndex + 1;

            string message;
            string suffix;

            if (rest.Length == 0)
            {
                message = $"Input path exists wholly until its end (column {goodColumn}). Is the path complete?";
                suffix = "(too short)";
            }
            else if (spaceAtEnd)
            {
                message = $"Input path has one or more spaces in a parent folder, starting at column {errorColumn}:";
                suffix = "(remove trailing spaces)";
            }
            else
            {
                char nextChar = path[errorIndex];
                string nextCharSafe = nextChar switch
                {
                    ' ' => "' ' (<space>)",
                    _ when char.IsControl(nextChar) => $"'{nextChar}' (0x{(uint)nextChar:X})",
                    _ => $"'{nextChar}'",
                };

                message = $"Input path differs from real path with character {nextCharSafe}, at column {errorColumn}:";
                suffix = string.Empty;
            }

            // minus one to make room for '>' indicator
            var indicator = "\t " + new string(' ', goodIndex) + "><" + suffix;

            report.Messages.AppendLine(message);
            report.Messages.AppendLine(indicator);
            report.Messages.AppendLine("\t" + finalTestPath);
            report.Messages.AppendLine("\t" + new string(' ', finalTestPath.Length) + rest);
            report.Messages.AppendLine("Here are some alternatives:");

            var directoryName = GetDirectoryName(finalTestPath);
            if (EndsWithSpaces(directoryName))
            {
                directoryName += slash;
            }

            Directory
                .EnumerateFileSystemEntries(directoryName, $"{GetFileName(finalTestPath)}*", SearchOption.TopDirectoryOnly)
                .Take(10)
                .FormatList(report.Messages);

            return false;

            static bool EndsWithSpaces(string fragment)
            {
                return fragment.Length > 0 && (fragment[^1] == ' ' || fragment.TrimEnd(' ') != fragment);
            }
        }

        /// <summary>
        /// A report on a path.
        /// </summary>
        public class PathDiffReport
        {
            /// <summary>
            /// Gets the messages the comprise the diff report.
            /// </summary>
            public StringBuilder Messages { get; } = new StringBuilder(1000);

            /// <summary>
            /// Gets or sets a file system info of the path in question.
            /// </summary>
            public FileSystemInfo Found { get; set; }
        }
    }
}