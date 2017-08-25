namespace Acoustics.Tools
{
    using System;
    using System.Collections.Generic;
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
        ///
        /*public static FileInfo PrepareFile(DirectoryInfo outputDirectory, FileInfo source, string outputMediaType, AudioUtilityRequest request, DirectoryInfo temporaryFilesDirectory)
        {
            var audioUtility = new MasterAudioUtility(temporaryFilesDirectory);
            var sourceMimeType = MediaTypes.GetMediaType(source.Extension);
            var outputFileName = Path.GetFileNameWithoutExtension(source.Name);

            outputFileName = string.Format(
                "{0}_{1:f0}min.{3}",
                outputFileName,
                request.OffsetStart.Value.TotalMinutes,
                request.OffsetEnd.Value.TotalMilliseconds,
                MediaTypes.GetExtension(outputMediaType));

            if (!Directory.Exists(outputDirectory.FullName))
            {
                Directory.CreateDirectory(outputDirectory.FullName);
            }

            var outputFile = new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName));
            var outputMimeType = MediaTypes.GetMediaType(outputFile.Extension);

            audioUtility.Modify(
                source,
                sourceMimeType,
                outputFile,
                outputMimeType,
                request);

            // TODO: this is hyper inefficient, especially when .Info is the first thing called inside Modify
            //var result = new AudioUtilityModifiedInfo();
            //result.SourceInfo = audioUtility.Info(source);
            //result.TargetInfo = audioUtility.Info(outputFile);

            // Next line is a HACK!!!!!! ############### MARK WILL HAVE TO DO MORE ELEGANTLY ONE DAY
            //request.OriginalSampleRate = result.SourceInfo.SampleRate;

            return outputFile;
        }*/

        public static AudioUtilityModifiedInfo PrepareFile(
            DirectoryInfo outputDirectory,
            FileInfo source,
            string outputMediaType,
            AudioUtilityRequest request,
            DirectoryInfo temporaryFilesDirectory)
        {
            var outputFileName = GetFileName(source.Name, outputMediaType, request.OffsetStart, request.OffsetEnd);

            var outputFile = new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName));

            return PrepareFile(source, outputFile, request, temporaryFilesDirectory);
        }

        public static string GetFileName(string outputFileName, string outputMediaType, TimeSpan? requestOffsetStart, TimeSpan? requestOffsetEnd)
        {
            outputFileName = string.Format(
                "{0}_{1:0.######}{2}min.{3}",
                Path.GetFileNameWithoutExtension(outputFileName),
                requestOffsetStart?.TotalMinutes ?? 0,
                requestOffsetEnd?.TotalMinutes.ToString("\\-0.######") ?? string.Empty,
                MediaTypes.GetExtension(outputMediaType));
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
            long rem;
            long div = Math.DivRem(numerator, denominator, out rem);

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
