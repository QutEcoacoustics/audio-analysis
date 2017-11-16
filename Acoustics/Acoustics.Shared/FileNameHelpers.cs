// <copyright file="FileNameHelpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A set of helper methods used to create/read consistently encoded filenames
    /// </summary>
    public static class FilenameHelpers
    {
        public const string SegmentSeparator = "_";
        public const string BasenameSeparator = "__";
        public const string ExtensionSeparator = ".";
        public const string ExampleFilename = "orginalBasename" + BasenameSeparator + "AnalysisType.SubType" + SegmentSeparator + "someOtherValue" + ExtensionSeparator + "extension";
        public static readonly Regex AnalysisResultRegex = new Regex(@"^(.*)" + BasenameSeparator + @"(.*)\" + ExtensionSeparator + "(.+)$");

        /// <summary>
        /// Return an absolute path for a result file.
        /// </summary>
        public static string AnalysisResultPath(
            DirectoryInfo outputDirectory,
            FileInfo orignalFile,
            string analysisTag,
            string newExtension,
            params string[] otherSegments)
        {
            var baseName = Path.GetFileNameWithoutExtension(orignalFile.Name);

            return AnalysisResultPath(outputDirectory, baseName, analysisTag, newExtension, otherSegments);
        }

        /// <summary>
        /// Return an absolute path for a result file.
        /// </summary>
        public static string AnalysisResultPath(
            DirectoryInfo outputDirectory,
            string baseName,
            string analysisTag,
            string newExtension,
            params string[] otherSegments)
        {
            var newBaseName = AnalysisResultName(baseName, analysisTag, newExtension, otherSegments);

            return Path.Combine(outputDirectory.FullName, newBaseName);
        }

        /// <summary>
        /// Return a relative file name only (no directory) for a result file.
        /// </summary>
        public static string AnalysisResultName(string baseName, string analysisTag, string newExtension, params string[] otherSegments)
        {
            if (string.IsNullOrWhiteSpace(baseName))
            {
                throw new ArgumentException("Invalid file stem / base name supplied");
            }

            if (string.IsNullOrWhiteSpace(analysisTag))
            {
                throw new ArgumentException("analysisTag must have a value", nameof(analysisTag));
            }

            if (baseName.Contains(BasenameSeparator))
            {
                baseName = baseName.Replace(BasenameSeparator, SegmentSeparator);
            }

            var filename = baseName + BasenameSeparator + analysisTag;

            if (otherSegments.Length > 0)
            {
                filename += otherSegments.Aggregate(string.Empty, (aggregate, item) => aggregate + SegmentSeparator + item);
            }

            if (!string.IsNullOrWhiteSpace(newExtension))
            {
                filename += ExtensionSeparator + newExtension;
            }

            return filename;
        }

        public static void ParseAnalysisFileName(
            FileInfo file,
            out string originalBaseName,
            out string analysisTag,
            out string[] otherSegments)
        {
            ParseAnalysisFileName(file.Name, out originalBaseName, out analysisTag, out otherSegments);
        }

        public static void ParseAnalysisFileName(string fileName, out string originalBaseName, out string analysisTag, out string[] otherSegments)
        {
            if (!TryParseAnalysisFileName(fileName, out originalBaseName, out analysisTag, out otherSegments))
            {
                throw new FormatException(
                    "Could not parse analysis fileName. Expected fileName form like: " + ExampleFilename);
            }
        }

        public static bool TryParseAnalysisFileName(
            string filename,
            out string originalBasename,
            out string analysisTag,
            out string[] otherSegments)
        {
            originalBasename = string.Empty;
            analysisTag = string.Empty;
            otherSegments = null;

            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }

            var match = AnalysisResultRegex.Match(filename);

            if (match.Success)
            {
                originalBasename = match.Groups[1].Value;
                var suffix = match.Groups[2].Value;

                var segments = suffix.Split(new[] { SegmentSeparator }, StringSplitOptions.None);

                analysisTag = segments[0];

                otherSegments = segments.Length > 1 ? segments.Skip(1).ToArray() : new string[] { };

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Matches a very specific format:
        /// e.g. "4c77b524-1857-4550-afaa-c0ebe5e3960a_20101013_003000+1000_Towsey.Acoustic.ACI.csv"
        /// </summary>
        public static bool TryParseOldStyleCsvFileName(string filename, out string analysisTag)
        {
            analysisTag = string.Empty;
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }

            var matches = Regex.Match(filename, @"^(.*)_(.*)\.(.+)$");
            if (matches.Success)
            {
                analysisTag = matches.Groups[2].Value;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
