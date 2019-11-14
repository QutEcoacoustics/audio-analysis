// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileDateHelpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the FileDateHelpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using log4net;

    public class FileDateHelpers
    {
        public static readonly DateTimeOffset UnixEpoch =
            new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private const string AudioMothKey = "AudioMoth";

        private static readonly string[] AcceptedFormatsNoTimeZone =
        {
                "yyyyMMdd[-|T|_]HHmmss (if timezone offset hint provided)",
                "yyyyMMdd[-|T|_]HHmmssZ",
        };

        private static readonly string[] AcceptedFormatsTimeZone =
        {
                "yyyyMMdd[-|T|_]HHmmss[+|-]HH",
                "yyyyMMdd[-|T|_]HHmmss[+|-]HHmm",
        };

        internal static readonly DateVariants[] PossibleFormats =
            {
                // valid: Prefix_YYYYMMDD_hhmmss.wav, Prefix_YYYYMMDD_hhmmssZ.wav
                // valid: prefix_20140101_235959.mp3, a_00000000_000000.a, a_99999999_999999.dnsb48364JSFDSD, prefix_20140101_235959Z.mp3
                // valid: SERF_20130314_000021_000.wav, a_20130314_000021_a.a, a_99999999_999999_a.dnsb48364JSFDSD
                new DateVariants(
                    @"^(.*)(?<date>(\d{4})(\d{2})(\d{2})(?<separator>T|-|_)(\d{2})(\d{2})(\d{2})(?![+-][\d:]{2,5}|Z)).*\.([a-zA-Z0-9]+)$",
                    AppConfigHelper.StandardDateFormatNoTimeZone,
                    parseTimeZone: false,
                    AcceptedFormatsNoTimeZone),

                // valid: prefix_20140101-235959+10.mp3, a_00000000-000000+00.a, a_99999999-999999+9999.dnsb48364JSFDSD
                // valid: prefix_20140101_235959+10.mp3, a_00000000_000000+00.a, a_99999999_999999+9999.dnsb48364JSFDSD
                // ISO8601-ish (supports a file compatible variant of ISO8601)
                // valid: prefix_20140101T235959+10.mp3, a_00000000T000000+00.a, a_99999999T999999+9999.dnsb48364JSFDSD
                new DateVariants(
                    @"^(.*)(?<date>(\d{4})(\d{2})(\d{2})(?<separator>T|-|_)(\d{2})(\d{2})(\d{2})(?![+-][:]{2,5})(?<offset>([+-](?!\d{0,5}:)(\d{4}|\d{2}))|Z)).*\.([a-zA-Z0-9]+)",
                    AppConfigHelper.StandardDateFormat,
                    parseTimeZone: true,
                    AcceptedFormatsTimeZone),

                // temporary support for an alternate date format. We will remove support for this format. We're also not documenting support for this date format.
                // valid: prefix_2359-01012015.mp3, a_2359-01012015.a, a_2359-01012015.dnsb48364JSFDSD
                new DateVariants(
                    @"^(.*)(?<date>(\d{2})(\d{2})(?<separator>-)(\d{2})(\d{2})(\d{4})).*\.([a-zA-Z0-9]+)",
                    "HHmm-ddMMyyyy",
                    parseTimeZone: false,
                    Array.Empty<string>()),

                // AudioMoth V1 format
                // 5BFA3A06.WAV
                // This pattern will recognize dates between 2012-07-13 and 2021-01-14.
                // We've restricted the date format to have a leading '5' character so that we can disambiguate between this pattern
                // and other patterns.
                new DateVariants(
                    "^(?<date>5[0-9A-F]{7}).*",
                    parseFormat: AudioMothKey,
                    parseTimeZone: true,
                    acceptedFormats: Array.Empty<string>()),
            };

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// sorts a list of files by the date assumed to be encoded in their file names
        /// and then returns the list as a sorted dictionary with file DateTime as the keys.
        /// </summary>
        /// <param name="files">The files to filter.</param>
        /// <param name="offsetHint">If you know what timezone you should have, specify a hint to enable parsing of ambiguous dates.</param>
        /// <returns>A sorted dictionary FileInfo objects mapped to parsed dates.</returns>
        public static SortedDictionary<DateTimeOffset, FileInfo> FilterFilesForDates(IEnumerable<FileInfo> files, TimeSpan? offsetHint = null)
        {
            var datesAndFiles = new SortedDictionary<DateTimeOffset, FileInfo>();
            foreach (var file in files)
            {
                if (FileNameContainsDateTime(file.Name, out var parsedDate, offsetHint))
                {
                    if (datesAndFiles.ContainsKey(parsedDate))
                    {
                        string message =
                            $"There was a duplicate date. File {file} with date {parsedDate:O} conflicts with existing file {datesAndFiles[parsedDate]}";
                        throw new InvalidDataSetException(message);
                    }

                    datesAndFiles.Add(parsedDate, file);
                }
            }

            // use following lines to get first and last date from returned dictionary
            //DateTimeOffset firstdate = datesAndFiles[datesAndFiles.Keys.First()];
            //DateTimeOffset lastdate  = datesAndFiles[datesAndFiles.Keys.Last()];
            return datesAndFiles;
        }

        /// <summary>
        /// sorts a list of files by the date assumed to be encoded in their file names
        /// and then returns the list as a sorted dictionary with file DateTime as the keys.
        /// </summary>
        /// <param name="files">The files to filter.</param>
        /// <param name="offsetHint">If you know what timezone you should have, specify a hint to enable parsing of ambiguous dates.</param>
        /// <returns>A sorted dictionary FileInfo objects mapped to parsed dates.</returns>
        public static SortedDictionary<DateTimeOffset, T> FilterObjectsForDates<T>(IEnumerable<T> objects, Func<T, FileSystemInfo> pathSelector, Func<T, DateTimeOffset?> overrideSelector, TimeSpan? offsetHint = null)
        {
            var datesAndFiles = new SortedDictionary<DateTimeOffset, T>();
            foreach (var @object in objects)
            {
                var date = overrideSelector?.Invoke(@object);

                FileSystemInfo file = null;
                if (date == null)
                {
                    file = pathSelector.Invoke(@object);
                    if (FileNameContainsDateTime(file.Name, out var parsedDate, offsetHint))
                    {
                        date = parsedDate;
                    }
                }

                if (date.NotNull())
                {
                    if (datesAndFiles.ContainsKey(date.Value))
                    {
                        if (file == null)
                        {
                            file = pathSelector.Invoke(@object);
                        }

                        string message =
                            $"There was a duplicate date. File {file} with date {date:O} conflicts with existing file {datesAndFiles[date.Value]}";
                        throw new InvalidDataSetException(message);
                    }

                    datesAndFiles.Add(date.Value, @object);
                }
            }

            // use following lines to get first and last date from returned dictionary
            //DateTimeOffset firstdate = datesAndFiles[datesAndFiles.Keys.First()];
            //DateTimeOffset lastdate  = datesAndFiles[datesAndFiles.Keys.Last()];
            return datesAndFiles;
        }

        /// <summary>
        /// sorts a list of files by the date assumed to be encoded in their file names
        /// and then returns the list as a sorted dictionary with file DateTime as the keys.
        /// </summary>
        /// <param name="directories">The files to filter.</param>
        /// <param name="offsetHint">If you know what timezone you should have, specify a hint to enable parsing of ambiguous dates.</param>
        /// <returns>A sorted dictionary FileInfo objects mapped to parsed dates.</returns>
        public static SortedDictionary<DateTimeOffset, DirectoryInfo> FilterDirectoriesForDates(IEnumerable<DirectoryInfo> directories, TimeSpan? offsetHint = null)
        {
            if (directories == null)
            {
                throw new ArgumentNullException(nameof(directories));
            }

            var datesAndDirs = new SortedDictionary<DateTimeOffset, DirectoryInfo>();
            foreach (var dir in directories)
            {
                if (FileNameContainsDateTime(dir.Name, out var parsedDate, offsetHint))
                {
                    datesAndDirs.Add(parsedDate, dir);
                }
            }

            return datesAndDirs;
        }

        public static bool FileNameContainsDateTime(string fileName)
        {
            return PossibleFormats.Any(format => Regex.IsMatch(fileName, format.Regex));
        }

        public static bool FileNameContainsDateTime(string fileName, out DateTimeOffset parsedDate, TimeSpan? offsetHint = null)
        {
            foreach (var format in PossibleFormats)
            {
                var success = ParseFileDateTimeBase(
                    fileName,
                    format,
                    out parsedDate,
                    offsetHint);

                if (success)
                {
                    return true;
                }
            }

            parsedDate = default;
            return false;
        }

        private static bool ParseFileDateTimeBase(
            string filename,
            DateVariants format,
            out DateTimeOffset fileDate,
            TimeSpan? offsetHint)
        {
            var match = Regex.Match(filename, format.Regex);
            fileDate = default;
            var successful = match.Success;

            if (!successful)
            {
                return false;
            }

            var stringDate = match.Groups["date"].Value;

            if (format.ParseFormat == AudioMothKey)
            {
                successful = ParseAudioMothV1Date(stringDate, out fileDate);
            }
            else
            {
                var separator = match.Groups["separator"].Value;

                // Normalize the separator
                stringDate = stringDate.Replace(separator, "-");

                if (format.ParseTimeZone)
                {
                    var offsetText = match.Groups["offset"].Value;

                    if (offsetText.Equals("Z", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var parseFormat = format.ParseFormat.Replace("zzz", "Z");

                        successful = DateTimeOffset.TryParseExact(
                            stringDate,
                            parseFormat,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal,
                            out fileDate);
                    }
                    else if (offsetText.Length == 5)
                    {
                        // e.g. +1000
                        successful = DateTimeOffset.TryParseExact(
                            stringDate,
                            format.ParseFormat,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out fileDate);
                    }
                    else
                    {
                        successful = DateTimeOffset.TryParseExact(
                            stringDate,
                            format.ParseFormat.Replace("zzz", "zz"),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out fileDate);
                    }
                }
                else
                {
                    successful = DateTime.TryParseExact(
                        stringDate,
                        format.ParseFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dateWithoutTimeZone);

                    if (successful)
                    {
                        if (offsetHint == null)
                        {
                            Log.Warn(
                                $"File date `{stringDate}` is ambiguous. The date is understood but no timezone offset could be found and a timezone offset hint was not provided.");
                            return false;
                        }
                        else
                        {
                            fileDate = new DateTimeOffset(dateWithoutTimeZone, offsetHint.Value);
                        }
                    }
                }
            }

            return successful;
        }

        private static bool ParseAudioMothV1Date(string text, out DateTimeOffset date)
        {
            var successful = long.TryParse(
                text,
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture,
                out var secondsSinceEpoch);

            if (successful)
            {
                date = UnixEpoch.AddSeconds(secondsSinceEpoch);
                return true;
            }

            date = default;
            return false;
        }

        internal class DateVariants
        {
            public DateVariants(string regex, string parseFormat, bool parseTimeZone, string[] acceptedFormats)
            {
                this.Regex = regex;
                this.ParseFormat = parseFormat;
                this.ParseTimeZone = parseTimeZone;
                this.AcceptedFormats = acceptedFormats;
            }

            public string Regex { get; }

            public string ParseFormat { get; }

            public bool ParseTimeZone { get; }

            public string[] AcceptedFormats { get; }
        }
    }

    public class InvalidFileDateException : Exception
    {
        private readonly string options;

        public InvalidFileDateException(string message)
            : base(message)
        {
            this.options = "\n Valid formats include:  \n"
                           + FileDateHelpers.PossibleFormats.SelectMany(x => x.AcceptedFormats)
                                 .Aggregate(string.Empty, (s, x) => s += "\t - " + x + "\n");
        }

        public override string Message => base.Message + this.options;
    }
}
