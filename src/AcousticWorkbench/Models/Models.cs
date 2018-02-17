// <copyright file="Models.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench.Models
{
    using System;
    using System.Collections.Generic;

    public class AudioRecording
    {
        public long Id { get; set; }

        public string Uuid { get; set; }

        public DateTimeOffset RecordedDate { get; set; }

        public long SiteId { get; set; }

        public double DurationSeconds { get; set; }

        public int SampleRateHertz { get; set; }

        public int Channels { get; set; }

        public int BitRateBps { get; set; }

        public string MediaType { get; set; }

        public long DataLengthBytes { get; set; }

        public string Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class Tagging
    {
        public long Id { get; set; }

        public long AudioEventId { get; set; }

        public long TagId { get; set; }

        public long CreatorId { get; set; }

        public long UpdaterId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class AudioEvent
    {
        public long Id { get; set; }

        public long AudioRecordingId { get; set; }

        public double StartTimeSeconds { get; set; }

        public double EndTimeSeconds { get; set; }

        public double LowFrequencyHertz { get; set; }

        public double HighFrequencyHertz { get; set; }

        public bool IsReference { get; set; }

        public long CreatorId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public IList<Tagging> Taggings { get; set; }
    }

    public class Recording
    {
        public long Id { get; set; }

        public string Uuid { get; set; }

        public DateTimeOffset RecordedDate { get; set; }

        public double DurationSeconds { get; set; }

        public int SampleRateHertz { get; set; }

        public int ChannelCount { get; set; }

        public string MediaType { get; set; }
    }

    public class CommonParameters
    {
        public double StartOffset { get; set; }

        public double EndOffset { get; set; }

        public long AudioEventId { get; set; }

        public int Channel { get; set; }

        public int SampleRate { get; set; }
    }

    public class FormatInfo
    {
        public string MediaType { get; set; }

        public string Extension { get; set; }

        public string Url { get; set; }
    }

    public class ImageFormatInfo : FormatInfo
    {
        public int WindowSize { get; set; }

        public string WindowFunction { get; set; }

        public string Colour { get; set; }

        public double Ppms { get; set; }
    }

    public class Available
    {
        public Dictionary<string, FormatInfo> Audio { get; set; }

        public Dictionary<string, ImageFormatInfo> Image { get; set; }

        public Dictionary<string, FormatInfo> Text { get; set; }
    }

    public class Media
    {
        public Recording Recording { get; set; }

        public CommonParameters CommonParameters { get; set; }

        public Available Available { get; set; }
    }
}