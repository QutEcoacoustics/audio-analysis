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
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class FileDateHelpers
    {
        internal static readonly DateVariants[] PossibleFormats = new[]
                                                     {
                                                         // valid: Prefix_YYYYMMDD_hhmmss.wav, Prefix_YYYYMMDD_hhmmssZ.wav
                                                         new DateVariants(@".*_(\d{8}_\d{6}Z?)\..+", AppConfigHelper.StandardDateFormatSm2, false, 1), 

                                                         // valid: prefix_20140101_235959.mp3, a_00000000_000000.a, a_99999999_999999.dnsb48364JSFDSD, prefix_20140101_235959Z.mp3

                                                         // valid: prefix_20140101-235959.mp3, a_00000000-000000.a, a_99999999-999999Z.dnsb48364JSFDSD
                                                         new DateVariants(@"^(.*)((\d{4})(\d{2})(\d{2})-(\d{2})(\d{2})(\d{2})Z?)\.([a-zA-Z0-9]+)$", AppConfigHelper.StandardDateFormat, false, 2),                                                         new DateVariants(@"^(.*)((\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})Z?)\.([a-zA-Z0-9]+)$", AppConfigHelper.StandardDateFormatSm2, false,  2), 

                                                         // valid: prefix_20140101-235959+10.mp3, a_00000000-000000+00.a, a_99999999-999999+9999.dnsb48364JSFDSD
                                                         new DateVariants(@"^(.*)((\d{4})(\d{2})(\d{2})-(\d{2})(\d{2})(\d{2})([+-]\d{2,4}))\.([a-zA-Z0-9]+)$", AppConfigHelper.StandardDateFormat, true, 2),

                                                         // valid: SERF_20130314_000021_000.wav, a_20130314_000021_a.a, a_99999999_999999_a.dnsb48364JSFDSD
                                                         new DateVariants(@"(.*)((\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2}Z?))_(.*?)\.([a-zA-Z0-9]+)$", AppConfigHelper.StandardDateFormatSm2, false, 2), 
                                                     };
        public static bool FileNameContainsDateTime(string fileName)
        {
            return PossibleFormats.Any(format => FilenameHasDateTimeBase(fileName, format.Regex));
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

        private static bool ParseFileDateTimeBase(string filename, DateVariants format, out DateTimeOffset fileDate, TimeSpan? offsetHint)
        {
                var match = Regex.Match(filename, format.Regex);
                fileDate = new DateTimeOffset();

                if (match.Success)
                {
                    var stringDate = match.Groups[format.ParseGroup].Value;

                    if (stringDate.EndsWith("Z", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var parseFormat = format.ParseFormat.Replace("zzz", "Z");
                        DateTimeOffset.ParseExact(
                            stringDate,
                            parseFormat,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal);
                    }
                    else if (format.ParseTimeZone)
                    {
                        fileDate = DateTimeOffset.ParseExact(
                            stringDate,
                            format.ParseFormat,
                            CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var date = DateTime.ParseExact(
                            stringDate,
                            format.ParseFormat,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeLocal);

                        if (offsetHint == null)
                        {
                            throw new ArgumentException(
                                "Do not know how to parse date {0} - specify a timezone offset hint explicitly".Format2(
                                    stringDate));
                        }
                        else
                        {
                            fileDate = new DateTimeOffset(date, offsetHint.Value);
                        }
                    }
                }

                return match.Success;
        }

        private static bool FilenameHasDateTimeBase(string filename, string regex)
        {
            return Regex.IsMatch(filename, regex);
        }

        internal class DateVariants
        {
            public DateVariants(string regex, string parseFormat, bool parseTimeZone, int parseGroup)
            {
                this.Regex = regex;
                this.ParseFormat = parseFormat;
                this.ParseTimeZone = parseTimeZone;
                this.ParseGroup = parseGroup;
            }

            public string Regex { get; set; }

            public string ParseFormat { get; set; }

            public bool ParseTimeZone{ get; set; }

            public int ParseGroup { get; set; }
        }
    }

    public class InvalidFileDateException : Exception
    {
        private string options;

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
