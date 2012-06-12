using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acoustics.Tools.Audio;
using System.IO;
using Acoustics.Shared;

namespace Acoustics.Tools
{
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
        /// <param name="startOffset">
        /// The start Offset from start of entire original file.
        /// </param>
        /// <param name="endOffset">
        /// The end Offset from start of entire original file.
        /// </param>
        /// <param name="targetSampleRateHz">
        /// The target Sample Rate Hz.
        /// </param>
        /// <returns>
        /// The prepared file.
        /// </returns>
        public static FileInfo PrepareFile(DirectoryInfo outputDirectory, FileInfo source, string outputMediaType, TimeSpan startOffset, TimeSpan endOffset, int targetSampleRateHz)
        {
            var audioUtility = GetNewAudioUtility(targetSampleRateHz);

            var sourceMimeType = MediaTypes.GetMediaType(source.Extension);

            var outputFileName = Path.GetFileNameWithoutExtension(source.Name);

            outputFileName = string.Format(
                "{0}_{1}_{2}.{3}",
                outputFileName,
                startOffset.TotalMilliseconds,
                endOffset.TotalMilliseconds,
                MediaTypes.GetExtension(outputMediaType));

            if (!Directory.Exists(outputDirectory.FullName))
            {
                Directory.CreateDirectory(outputDirectory.FullName);
            }

            var output = new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName));
            var outputMimeType = MediaTypes.GetMediaType(output.Extension);

            audioUtility.Segment(
                source,
                sourceMimeType,
                output,
                outputMimeType,
                startOffset,
                endOffset);

            return output;
        }

        public static FileInfo PrepareFile(DirectoryInfo outputDirectory, FileInfo source, string outputMediaType, int targetSampleRateHz)
        {
            var audioUtility = GetNewAudioUtility(targetSampleRateHz);

            var sourceMimeType = MediaTypes.GetMediaType(source.Extension);

            var outputFileName = Path.GetFileNameWithoutExtension(source.Name);

            outputFileName = string.Format(
                "{0}_converted.{1}",
                outputFileName,
                MediaTypes.GetExtension(outputMediaType));

            if (!Directory.Exists(outputDirectory.FullName))
            {
                Directory.CreateDirectory(outputDirectory.FullName);
            }

            var output = new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName));
            var outputMimeType = MediaTypes.GetMediaType(output.Extension);

            audioUtility.Convert(
                source,
                sourceMimeType,
                output,
                outputMimeType);

            return output;
        }

        public static void PrepareFile(FileInfo sourceF, FileInfo outputF, int targetSampleRateHz)
        {
            var audioUtility = GetNewAudioUtility(targetSampleRateHz);

            var sourceMimeType = MediaTypes.GetMediaType(sourceF.Extension);
            var outputMimeType = MediaTypes.GetMediaType(outputF.Extension);
            string outputDirectory = Path.GetDirectoryName(outputF.FullName);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            audioUtility.Convert(
                sourceF,
                sourceMimeType,
                outputF,
                outputMimeType);
        }

        public static void PrepareFile(FileInfo sourceF, FileInfo outputF, int targetSampleRateHz, TimeSpan startOffset, TimeSpan endOffset)
        {
            var audioUtility = GetNewAudioUtility(targetSampleRateHz);

            var sourceMimeType = MediaTypes.GetMediaType(sourceF.Extension);
            var outputMimeType = MediaTypes.GetMediaType(outputF.Extension);
            string outputDirectory = Path.GetDirectoryName(outputF.FullName);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            audioUtility.Segment(
                sourceF,
                sourceMimeType,
                outputF,
                outputMimeType,
                startOffset,
                endOffset);
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
        /// This doesn't try to cope with negative numbers :)
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

        private static IAudioUtility GetNewAudioUtility(int targetSampleRateHz)
        {
            var audioUtility = new MasterAudioUtility(targetSampleRateHz, SoxAudioUtility.SoxResampleQuality.VeryHigh);
            return audioUtility;
        }
    }
}
