// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileDateHelpersTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using MSTestExtensions;

namespace Acoustics.Test
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static DateTimeOffset;

    [TestClass]
    public class FileDateHelpersTests : BaseTest
    {
        [TestMethod]
        public void BasicTestCase()
        {
            string testCase = "example_20150813-120513Z.mp3";

            var test = testCase?.Equals(string.Empty);

            DateTimeOffset parsedDate;
            Assert.IsTrue(FileDateHelpers.FileNameContainsDateTime(testCase, out parsedDate));

            var expected = new DateTimeOffset(2015, 08, 13, 12, 05, 13, TimeSpan.Zero);

            Assert.AreEqual(expected, parsedDate);
        }

        // adapted from
        // https://github.com/QutBioacoustics/baw-workers/blob/master/spec/lib/baw-workers/file_info_spec.rb#L162
        private readonly string[] invalidFormats =
            {
                string.Empty, "blah", "blah.ext", ".ext.ext.ext", "hi.hi",
                "yyyymmdd_hhmmss.ext", "_yyyymmdd_hhmmss.ext", "blah_yyyymmdd_hhmmss.ext_blah",
                "blah_yyyymmdd_hhmmss.ext_blah", "blah_yyyymmdd_hhmmss.ext.blah", "yyyymmdd_hhmmss_yyyymmdd_hhmmss.ext.blah",
                "yyyymmdd_hhmmssyyyymmdd_hhmmss.ext.blah", "yyyymmdd_hhmmssyyyymmdd_hhmmss.ext",
                "p1_s1_u1_d20140301_t000000.ext", "1_s1_u1_d20140301_t000000.ext", "p1_s1_1_d20140301_t000000.ext",
                "p1_s1_u1_d0140301_t000000.ext", "p1_s1_u1_d20140301_t00000Z.ext", "my_audio_file.mp3",
                "sdncv*_-T&^%34jd_20140301_-085031_blah_T-suffix.ext",
                "sdncv*_-T&^%34jd_20140301_-085031+_blah_T-suffix.ext",
                "sdncv*_-T&^%34jd_20140301_-085031:_blah_T-suffix.ext",
                "sdncv*_-T&^%34jd_20140301_-085031-_blah_T-suffix.ext",
                // don't support colons in filenames
                "sdncv*_-T&^%34jd_20140301_085031+06:30blah_T-suffix.mp3",
                // do not support ambiguous dates
                "20150727133138.wav", "blah_T-suffix20140301085031.mp3", "SERF_20130314_000021_000.wav",
                "20150727T133138.wav",
                // do not allow invalid offsets
                "blah_T-suffix20140301-085031-7s:dncv*_-T&^%34jd.ext",
                // require at least a time seperator
                "20150727133138.mp3", "blah_T-suffix20140301085031:dncv*_-T&^%34jd.ext"
            };

        private readonly string[] invalidDates =
            {
                "a_99999999_999999_a.dnsb48364JSFDSD", "a_00000000_000000.a",
                "a_00000000_000000+00.a", "a_99999999_999999.dnsb48364JSFDSD", "a_00000000-000000+00.a",
                "a_99999999_999999+9999.dnsb48364JSFDSD"
            };

        [TestMethod]
        public void TestInvalidFormats()
        {
            foreach (var example in this.invalidFormats)
            {
                DateTimeOffset parsedDate;
                Debug.WriteLine($"Testing format: {example}");
                Assert.IsFalse(
                    FileDateHelpers.FileNameContainsDateTime(example, out parsedDate),
                    $"Testing format: {example}");
            }
        }

        [TestMethod]
        public void TestInvalidDateFormats()
        {
            foreach (var example in this.invalidDates)
            {
                DateTimeOffset parsedDate;
                Debug.WriteLine($"Testing format: {example}");
                Assert.IsFalse(
                    FileDateHelpers.FileNameContainsDateTime(example, out parsedDate),
                    $"Testing format: {example}");
            }
        }

        //private static DateTimeOffset Parse(string date) => DateTimeOffset.Parse(date);

        private Dictionary<string, DateTimeOffset> validFormats = new Dictionary<string, DateTimeOffset>
            {
                ["sdncv*_-T&^%34jd_20140301_085031+0630blah_T-suffix.mp3"] = Parse("2014-03-01T08:50:31.000+06:30"),
                ["sdncv*_-T&^%34jd_20140301_085031-0630blah_T-suffix.mp3"] = Parse("2014-03-01T08:50:31.000-06:30"),
                ["blah_T-suffix20140301-085031-07s:dncv*_-T&^%34jd.ext"] = Parse("2014-03-01T08:50:31.000-07:00"),
                ["20150727T133138Z.wav"] = Parse("2015-07-27T13:31:38.000+00:00"),
                ["blah_T-suffix20140301-085031Z:dncv*_-T&^%34jd.ext"] = Parse("2014-03-01T08:50:31.000+00:00"),
                ["SERF_20130314_000021Z_000.wav"] = Parse("2013-03-14T00:00:21.000+00:00"),
                ["20150727T133138Z.wav"] = Parse("2015-07-27T13:31:38.000+00:00"),
            };

        [TestMethod]
        public void TestValidDateFormats()
        {
            foreach (var example in this.validFormats)
            {
                DateTimeOffset parsedDate;
                Debug.WriteLine($"Testing format: {example.Key}");
                Assert.IsTrue(
                    FileDateHelpers.FileNameContainsDateTime(example.Key, out parsedDate),
                    $"Testing format: {example}");


                Assert.AreEqual(example.Value, parsedDate);
            }
        }

        private Dictionary<string, DateTimeOffset> validFormatsWithOffsetHint = new Dictionary<string, DateTimeOffset>
            {
                ["sdncv*_-T&^%34jd_20140301_085031blah_T-suffix.mp3"] = Parse("2014-03-01T08:50:31.000+06:30"),
                ["sdncv*_-T&^%34jd_20140301T085031blah_T-suffix.mp3"] = Parse("2014-03-01T08:50:31.000+06:30"),
                ["blah_T-suffix20140301-085031:dncv*_-T&^%34jd.ext"] = Parse("2014-03-01T08:50:31.000+06:30"),
                ["20150727T133138.wav"] = Parse("2015-07-27T13:31:38.000+06:30"),
                ["blah_T-suffix20140301-085031:dncv*_-T&^%34jd.ext"] = Parse("2014-03-01T08:50:31.000+06:30"),
                ["SERF_20130314_000021_000.wav"] = Parse("2013-03-14T00:00:21.000+06:30"),
                ["20150727T133138.wav"] = Parse("2015-07-27T13:31:38.000+06:30"),
                ["20150801-064555.wav"] = Parse("2015-08-01T06:45:55.000+06:30"),
            };

        [TestMethod]
        public void TestValidDateFormatsWithOffsetHint()
        {
            foreach (var example in this.validFormatsWithOffsetHint)
            {
                DateTimeOffset parsedDate;
                Debug.WriteLine($"Testing format: {example.Key}");
                Assert.IsTrue(
                    FileDateHelpers.FileNameContainsDateTime(
                        example.Key,
                        out parsedDate,
                        offsetHint: new TimeSpan(6, 30, 0)),
                    $"Testing format: {example}");


                Assert.AreEqual(example.Value, parsedDate);
            }
        }


        private readonly string[] unorderedFiles = new[]
            {
                @"Y:\2015Sept20\Woondum3\20150919_201742Z.wav", @"Y:\2015Sept20\Woondum3\20150920_000006Z.wav",
                @"Y:\2015Sept20\Woondum3\20150920_064555Z.wav", @"Y:\2015Sept20\Woondum3\20150920-133145Z.wav",
                @"Y:\2015Sept20\Woondum3\20150917-064553Z.wav", @"Y:\2015Sept20\Woondum3\20150917_133143+1000.wav",
                @"Y:\2015Sept20\Woondum3\20150917_201733+1000.wav", @"Y:\2015Aug2\GympieNP\20150801_000004+1000.wav",
                
            @"Y:\2015Aug2\GympieNP\20150801-064555.wav", @"Y:\2015Aug2\GympieNP\20150801_133148+1000.wav", @"Y:\2015Aug2\GympieNP\20150801-064555+1000.wav",
                @"Y:\2015Aug2\GympieNP\20150801-201742+1000.wav", @"Y:\2015Aug2\GympieNP\20150802-000006Z.wav",
                @"Y:\2015Aug2\GympieNP\20150802-064559+1000.wav", @"Y:\2015Sept20\Woondum3\20150919_000006+1000.wav",
                @"Y:\2015Sept20\Woondum3\20150919_064557+1000.wav", @"Y:\2015Sept20\Woondum3\20150919-133149+1000.wav",
                @"Y:\2015Sept20\Woondum3\20150918_000003Z.wav", @"Y:\2015Sept20\Woondum3\20150918_064554+1000.wav",
                @"Y:\2015Sept20\Woondum3\20150918-133146+1130.wav", @"Y:\2015Sept20\Woondum3\20150918_201738.wav",
            };


        private DateTimeOffset[] orderedDates = new[]
            {
                Parse("2014-03-01T08:50:31.000+06:30"), Parse("2014-03-01T08:50:31.000+06:30"),
                Parse("2014-03-01T08:50:31.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2014-03-01T08:50:31.000+06:30"), Parse("2013-03-14T00:00:21.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30"), Parse("2015-07-27T13:31:38.000+06:30"),
                Parse("2015-07-27T13:31:38.000+06:30")
            };

        [TestMethod]
        public void TestFileOrderingFunction()
        {
            var fileInfos = this.unorderedFiles.Select(s => s.ToFileInfo());

            var sortedFiles =
                FileDateHelpers.FilterFilesForDates(fileInfos, offsetHint: TimeSpan.FromHours(11)).ToArray();

            // all dates are valid
            Assert.AreEqual(this.orderedDates.Length, sortedFiles.Length);

            for (var i = 0; i < sortedFiles.Length; i++)
            {
                var datePair = sortedFiles[i];
                Debug.WriteLine($"Date parsed: {datePair.Key.ToString("O")}, {datePair.Key.UtcDateTime.ToString("O")}, for file: {datePair.Value}");

                //Assert.AreEqual(this.orderedDates[i], datePair.Key);
            }
        }
    }
}