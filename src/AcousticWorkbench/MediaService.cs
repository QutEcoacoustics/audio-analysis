// <copyright file="MediaService.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Models;

    public partial class MediaService : Service
    {
        public const double MediaDownloadMinimumSeconds = 0.5;
        public const double MediaDownloadMaximumSeconds = 300;

        public MediaService(IAuthenticatedApi authenticatedApi)
            : base(authenticatedApi)
        {
        }

        public async Task<Media> GetMetaData(long audioRecordingId)
        {
            // url for getting metadata for whole file
            var uri = this.AuthenticatedApi.GetMediaInfoUri(audioRecordingId);

            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };

            var response = await this.HttpClient.SendAsync(request);

            return await this.ProcessApiResult<Media>(response);
        }

        public async Task<(Stream Stream, long? ContentLength)> DownloadMediaWave(
            long audioRecordingId,
            double startOffsetSeconds,
            double endOffsetSeconds,
            int? sampleRateHertz = null,
            byte? channel = 0)
        {
            var uri = this.AuthenticatedApi.GetMediaWaveUri(
                audioRecordingId,
                startOffsetSeconds,
                endOffsetSeconds,
                sampleRateHertz,
                channel);

            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };

            // this is just blocking until the response is ready to start streaming
            // and NOT blocking until the entire response has been read!
            var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadAsStreamAsync(), response.Content.Headers.ContentLength);
            }

            // only called for errors - this call should always throw
            await this.ProcessApiResult<object>(response);

            throw new InvalidOperationException("DownloadMedia reached an invalid state");
        }
    }
}
