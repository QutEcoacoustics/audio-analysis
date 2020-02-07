namespace Acoustics.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    using Shared;
    using Audio;

    /// <summary>
    /// The audio file preparer.
    /// </summary>
    public static class AudioFilePreparer
    {
        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, modified based on the analysisSettings.
        /// </summary>
        /// <param name="outputDirectory">
        /// The analysis Base Directory.
        /// </param>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="outputMediaType">
        /// The output Media Type.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The prepared file.
        /// </returns>
        public static AudioUtilityModifiedInfo PrepareFile(
            DirectoryInfo outputDirectory,
            FileInfo source,
            string outputMediaType,
            AudioUtilityRequest request,
            DirectoryInfo temporaryFilesDirectory,
            bool oldFormat = true)
        {
            var outputFileName = GetFileName(
                source.Name,
                outputMediaType,
                request.OffsetStart,
                request.OffsetEnd,
                oldFormat: oldFormat);

            var outputFile = new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName));

            return PrepareFile(source, outputFile, request, temporaryFilesDirectory);
        }

        public static string GetFileName(
            string outputFileName,
            string outputMediaType,
            TimeSpan? requestOffsetStart,
            TimeSpan? requestOffsetEnd,
            bool oldFormat = false)
        {
            var start = oldFormat ? requestOffsetStart?.TotalMinutes : requestOffsetStart?.TotalSeconds;
            var end = oldFormat ? requestOffsetEnd?.TotalMinutes : requestOffsetEnd?.TotalSeconds;

            var format = oldFormat ? "0.######" : "0.###";

            outputFileName =
                string.Format(
                    "{0}_{1}{2}{4}.{3}",
                    Path.GetFileNameWithoutExtension(outputFileName),
                    (start ?? 0).ToString(format, CultureInfo.InvariantCulture),
                    end?.ToString("\\-" + format, CultureInfo.InvariantCulture) ?? string.Empty,
                    MediaTypes.GetExtension(outputMediaType),
                    oldFormat ? "min" : string.Empty);
            return outputFileName;
        }

        /// <summary>
        /// The prepare file.
        /// </summary>
        /// <param name="sourceF">
        ///   The source f.
        /// </param>
        /// <param name="outputF">
        ///   The output f.
        /// </param>
        /// <param name="request">
        ///   The request.
        /// </param>
        public static AudioUtilityModifiedInfo PrepareFile(FileInfo sourceFile, FileInfo outputFile, AudioUtilityRequest request, DirectoryInfo temporaryFilesDirectory)
        {
            var audioUtility = new MasterAudioUtility(temporaryFilesDirectory);
            var sourceMimeType = MediaTypes.GetMediaType(sourceFile.Extension);
            var outputMimeType = MediaTypes.GetMediaType(outputFile.Extension);
            string outputDirectory = Path.GetDirectoryName(outputFile.FullName);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            audioUtility.Modify(
                sourceFile,
                sourceMimeType,
                outputFile,
                outputMimeType,
                request);

            var result = new AudioUtilityModifiedInfo
                {
                    TargetInfo = audioUtility.Info(outputFile),
                    SourceInfo = audioUtility.Info(sourceFile),
                };
            return result;
        }

        /// <summary>
        /// Divide a value (numerator) by a segment value (denominator)
        /// to get segments of as equal size as possible.
        /// </summary>
        /// <param name="numerator">
        /// The numerator.
        /// </param>
        /// <param name="denominator">
        /// The denominator.
        /// </param>
        /// <returns>
        /// Segment start points.
        /// </returns>
        /// <remarks>
        /// from: http://stackoverflow.com/a/577451/31567
        /// This doesn't try to cope with negative numbers :).
        /// </remarks>
        [Obsolete]
        public static IEnumerable<long> DivideEvenly(long numerator, long denominator)
        {
            long div = Math.DivRem(numerator, denominator, out var rem);

            for (long i = 0; i < denominator; i++)
            {
                yield return i < rem ? div + 1 : div;
            }
        }

        /// <summary>
        /// Divide a value (numerator) by a segment value (denominator)
        /// to get segments of exactly denominator in size, and the leftovers at the end.
        /// </summary>
        /// <param name="numerator">
        /// The numerator.
        /// </param>
        /// <param name="denominator">
        /// The denominator.
        /// </param>
        /// <returns>
        /// Segment start points.
        /// </returns>
        public static IEnumerable<long> DivideExactLeaveLeftoversAtEnd(long numerator, long denominator)
        {
            var amountLeft = numerator;
            while (amountLeft > denominator)
            {
                yield return denominator;
                amountLeft -= denominator;
            }

            yield return amountLeft;
        }
    }
}
