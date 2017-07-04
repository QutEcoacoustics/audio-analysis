// <copyright file="UrlGenerator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;

    public static class UrlGenerator
    {
        public static Uri GetLoginUri(this IApi api)
        {
            return api.Base("security");
        }

        public static Uri GetSessionValidateUri(this IApi api)
        {
            return api.Base("security/new");
        }

        public static Uri GetListenUri(this IApi api, ISegment segment)
        {
            return GetListenUri(api, segment.AudioRecordingId, segment.StartOffsetSeconds, segment.EndOffsetSeconds);
        }

        public static Uri GetListenUri(this IApi api, int audioRecordingId, double startOffsetSeconds, double? endOffsetSeconds = null)
        {
            string end = endOffsetSeconds == null ? string.Empty : $"&end ={endOffsetSeconds}";
            return api.ViewBase($"listen/{audioRecordingId}?start={startOffsetSeconds}{end}");
        }

        public static Uri GetMediaInfoUri(this IApi api, ISegment segment)
        {
            return api.Base($"audio_recordings/{segment.AudioRecordingId}/media.json?start_offset={segment.StartOffsetSeconds}&end_offset={segment.EndOffsetSeconds}");
        }

        public static Uri GetMediaWaveUri(this IApi api, ISegment segment)
        {
            return api.Base($"audio_recordings/{segment.AudioRecordingId}/media.wav?start_offset={segment.StartOffsetSeconds}&end_offset={segment.EndOffsetSeconds}");
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
