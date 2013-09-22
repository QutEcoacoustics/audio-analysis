namespace Acoustics.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

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
        public static FileInfo PrepareFile(DirectoryInfo outputDirectory, FileInfo fiSource, string outputMediaType, AudioUtilityRequest request, DirectoryInfo temporaryFilesDirectory)
        {
            var audioUtility = new MasterAudioUtility(temporaryFilesDirectory);
            var sourceMimeType = MediaTypes.GetMediaType(fiSource.Extension);
            var outputFileName = Path.GetFileNameWithoutExtension(fiSource.Name);

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

            var fiOutput = new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName));
            var outputMimeType = MediaTypes.GetMediaType(fiOutput.Extension);

            audioUtility.Modify(
                fiSource,
                sourceMimeType,
                fiOutput,
                outputMimeType,
                request);

            var result = new AudioUtilityModifiedInfo();
            result.SourceInfo = audioUtility.Info(fiSource);
            result.TargetInfo = audioUtility.Info(fiOutput);

            // Next line is a HACK!!!!!! ############### MARK WILL HAVE TO DO MORE ELEGANTLY ONE DAY
            request.OriginalSampleRate = result.SourceInfo.SampleRate;

            return fiOutput;
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
        public static AudioUtilityModifiedInfo PrepareFile(FileInfo fiSource, FileInfo fiOutput, AudioUtilityRequest request, DirectoryInfo temporaryFilesDirectory)
        {
            var audioUtility = new MasterAudioUtility(temporaryFilesDirectory);
            var sourceMimeType = MediaTypes.GetMediaType(fiSource.Extension);
            var outputMimeType = MediaTypes.GetMediaType(fiOutput.Extension);
            string outputDirectory = Path.GetDirectoryName(fiOutput.FullName);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            audioUtility.Modify(
                fiSource,
                sourceMimeType,
                fiOutput,
                outputMimeType,
                request);

            var result = new AudioUtilityModifiedInfo { SourceInfo = audioUtility.Info(fiSource) };
            result.TargetInfo = audioUtility.Info(fiOutput);

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
