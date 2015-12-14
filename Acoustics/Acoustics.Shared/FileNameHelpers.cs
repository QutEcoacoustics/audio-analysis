using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acoustics.Shared
{
    using System.IO;
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

        public static string AnalysisResultName(
            FileInfo orignalFile,
            string analysisTag,
            string newExtension,
            params string[] otherSegments)
        {
            var basename = Path.GetFileNameWithoutExtension(orignalFile.Name);

            return AnalysisResultName(orignalFile.Directory, basename, analysisTag, newExtension, otherSegments);
        }

        public static string AnalysisResultName(
            DirectoryInfo directory,
            string basename,
            string analysisTag,
            string newExtension,
            params string[] otherSegments)
        {
            var newBaseName = AnalysisResultName(basename, analysisTag, newExtension, otherSegments);

            return Path.Combine(directory.FullName, newBaseName);
        }

        public static string AnalysisResultName(string basename, string analysisTag, string newExtension, params string[] otherSegments)
        {
            if (string.IsNullOrWhiteSpace(basename))
            {
                throw new ArgumentException("Invalid file stem / basename supplied");
            }

            if (string.IsNullOrWhiteSpace(analysisTag))
            {
                throw new ArgumentException("analysisTag must have a value", "analysisTag");
            }

            if (basename.Contains(BasenameSeparator))
            {
                basename = basename.Replace(BasenameSeparator, SegmentSeparator);
            }

            var filename = basename + BasenameSeparator + analysisTag;

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

        public static string AnalysisResultName(DirectoryInfo outputDirectory, string v1, object spectralIndexDistributionsFilenameFragment, string v2)
        {
            throw new NotImplementedException();
        }

        public static void ParseAnalysisFileName(
            FileInfo file,
            out string originalBasename,
            out string analysisTag,
            out string[] otherSegments)
        {
            ParseAnalysisFileName(file.Name, out originalBasename, out analysisTag, out otherSegments);
        }

        public static void ParseAnalysisFileName(string filename, out string originalBasename, out string analysisTag, out string[] otherSegments)
        {
            if (!TryParseAnalysisFileName(filename, out originalBasename, out analysisTag, out otherSegments))
            {
                throw new FormatException(
                    "Could not parse analysis filename. Expected filename form like: " + ExampleFilename);
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
        /// <param name="filename"></param>
        /// <param name="analysisTag"></param>
        /// <returns></returns>
        public static bool TryParseOldStyleCsvFileName(string filename, out string analysisTag)
        {
            analysisTag = String.Empty;
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }

            var matches = Regex.Match(filename, @"^(.*)_(.*)\.(.+)$)");
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
