﻿// <copyright file="UrlGenerator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;

    public static class UrlGenerator
    {
        public static Uri GetAudioEventUri(this IApi api, long audioRecordingId, long audioEventId)
        {
            return api.Base($"audio_recordings/{audioRecordingId}/audio_events/{audioEventId}");
        }

        public static Uri GetAudioEventFilterUri(this IApi api)
        {
            return api.Base("audio_events/filter");
        }

        public static Uri GetAudioRecordingUri(this IApi api, long audioRecordingId)
        {
            return api.Base($"audio_recordings/{audioRecordingId}");
        }

        public static Uri GetLoginUri(this IApi api)
        {
            return api.Base("security");
        }

        public static Uri GetSessionValidateUri(this IApi api)
        {
            return api.Base("security/new");
        }

        public static Uri GetListenUri(this IApi api, long audioRecordingId, double startOffsetSeconds, double? endOffsetSeconds = null)
        {
            string end = endOffsetSeconds == null ? string.Empty : $"&end ={endOffsetSeconds}";
            return api.ViewBase($"listen/{audioRecordingId}?start={startOffsetSeconds}{end}");
        }

        public static Uri GetMediaInfoUri(this IApi api, long audioRecordingId)
        {
            return api.Base($"audio_recordings/{audioRecordingId}/media.json");
        }

        public static Uri GetMediaInfoUri(this IApi api, long audioRecordingId, double startOffsetSeconds, double endOffsetSeconds)
        {
            return api.Base($"audio_recordings/{audioRecordingId}/media.json?start_offset={startOffsetSeconds}&end_offset={endOffsetSeconds}");
        }

        public static Uri GetMediaWaveUri(
            this IApi api,
            long audioRecordingId,
            double startOffsetSeconds,
            double endOffsetSeconds,
            int? sampleRate = null,
            byte? channel = 0)
        {
            return api.Base(
                $"audio_recordings/{audioRecordingId}/media.wav?"
                + $"start_offset={startOffsetSeconds}&end_offset={endOffsetSeconds}"
                + (sampleRate.HasValue ? $"&sample_rate={sampleRate.Value}" : string.Empty)
                + (channel.HasValue ? $"&channel={channel.Value}" : string.Empty));
        }

        public static Uri Base(this IApi api, string path = "")
        {
            string version = api.Version == "v1" ? string.Empty : "/" + api.Version;

            if (path.Length > 0)
            {
                path = "/" + path;
            }

            return new Uri($"{api.Protocol}://{api.Host}{version}{path}");
        }

        public static Uri ViewBase(this IApi api, string path = "")
        {
            if (path.Length > 0)
            {
                path = "/" + path;
            }

            return new Uri($"{api.Protocol}://{api.Host}{path}");
        }
    }
}
