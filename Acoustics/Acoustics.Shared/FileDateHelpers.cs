// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileDateHelpers.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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

        internal static readonly DateVariants[] PossibleFormats =
            {
                // valid: Prefix_YYYYMMDD_hhmmss.wav, Prefix_YYYYMMDD_hhmmssZ.wav
                // valid: prefix_20140101_235959.mp3, a_00000000_000000.a, a_99999999_999999.dnsb48364JSFDSD, prefix_20140101_235959Z.mp3
                // valid: SERF_20130314_000021_000.wav, a_20130314_000021_a.a, a_99999999_999999_a.dnsb48364JSFDSD
                new DateVariants(
                    @"^(.*)(?<date>(\d{4})(\d{2})(\d{2})(?<separator>T|-|_)(\d{2})(\d{2})(\d{2})(?![+-][\d:]{2,5}|Z)).*\.([a-zA-Z0-9]+)$",
                     AppConfigHelper.StandardDateFormatNoTimeZone, 
                    false),

                // valid: prefix_20140101-235959+10.mp3, a_00000000-000000+00.a, a_99999999-999999+9999.dnsb48364JSFDSD                                    
                // valid: prefix_20140101_235959+10.mp3, a_00000000_000000+00.a, a_99999999_999999+9999.dnsb48364JSFDSD                                    
                // ISO8601-ish (supports a file compatible variant of ISO8601)
                // valid: prefix_20140101T235959+10.mp3, a_00000000T000000+00.a, a_99999999T999999+9999.dnsb48364JSFDSD
                new DateVariants(
                    @"^(.*)(?<date>(\d{4})(\d{2})(\d{2})(?<separator>T|-|_)(\d{2})(\d{2})(\d{2})(?![+-][:]{2,5})(?<offset>([+-](?!\d{0,5}:)(\d{4}|\d{2}))|Z)).*\.([a-zA-Z0-9]+)",
                    AppConfigHelper.StandardDateFormat,
                    true),
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
                DateTimeOffset parsedDate;
                if (FileNameContainsDateTime(file.Name, out parsedDate, offsetHint))
                {
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
        /// <param name="directories">The files to filter.</param>
        /// <param name="offsetHint">If you know what timezone you should have, specify a hint to enable parsing of ambiguous dates.</param>
        /// <returns>A sorted dictionary FileInfo objects mapped to parsed dates.</returns>
        public static SortedDictionary<DateTimeOffset, DirectoryInfo> FilterDirectoriesForDates(IEnumerable<DirectoryInfo> directories, TimeSpan? offsetHint = null)
        {
            var datesAndDirs = new SortedDictionary<DateTimeOffset, DirectoryInfo>();
            foreach (var dir in directories)
            {
                DateTimeOffset parsedDate;
                if (FileNameContainsDateTime(dir.Name, out parsedDate, offsetHint))
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

            parsedDate = new DateTimeOffset();
            return false;
        }

        private static bool ParseFileDateTimeBase(
            string filename,
            DateVariants format,
            out DateTimeOffset fileDate,
            TimeSpan? offsetHint)
        {
            var match = Regex.Match(filename, format.Regex);
            fileDate = new DateTimeOffset();
            var successful = match.Success;

            if (!successful)
            {
                return false;
            }

            var stringDate = match.Groups["date"].Value;

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
                DateTime dateWithoutTimeZone;
                successful = DateTime.TryParseExact(
                    stringDate,
                    format.ParseFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dateWithoutTimeZone);

                if (successful)
                {
                    if (offsetHint == null)
                    {
                        Log.Warn(
                            "File date parse cannot parse date {0} - a timezone offset hint was not provided to the function for a date format with no timezone information"
                                .Format2(stringDate));
                        return false;
                    }
                    else
                    {
                        fileDate = new DateTimeOffset(dateWithoutTimeZone, offsetHint.Value);
                    }
                }
            }

            return successful;
        }

        internal class DateVariants
        {
            public DateVariants(string regex, string parseFormat, bool parseTimeZone)
            {
                this.Regex = regex;
                this.ParseFormat = parseFormat;
                this.ParseTimeZone = parseTimeZone;
            }

            public string Regex { get; set; }

            public string ParseFormat { get; set; }

            public bool ParseTimeZone{ get; set; }

        }
    }

    public class InvalidFileDateException : Exception
    {
        private readonly string options;

        public InvalidFileDateException(string message)
            : base(message)
        {
            this.options = "\n Valid formats include:  \n"
                           + FileDateHelpers.PossibleFormats.Select(x => x.ParseFormat)
                                 .Distinct()
                                 .Aggregate(string.Empty, (s, x) => s += "\t - " + x + "\n");
        }

        public override string Message
        {
            get
            {
                return base.Message + this.options;
            }
        }
    }
}
