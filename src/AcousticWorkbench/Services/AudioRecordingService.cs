// <copyright file="AudioRecordingService.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AcousticWorkbench.Models;

    public partial class AudioRecordingService : Service
    {
        public AudioRecordingService(IAuthenticatedApi authenticatedApi)
            : base(authenticatedApi)
        {
        }

        public async Task<AudioRecording> GetAudioRecording(long audioRecordingId)
        {
            var uri = this.AuthenticatedApi.GetAudioRecordingUri(audioRecordingId);

            var response = await this.HttpClient.GetAsync(uri);

            return await this.ProcessApiResult<AudioRecording>(response);
        }

        public async Task<IReadOnlyCollection<AudioRecording>> FilterRecordingsForDownload(
            ulong[] ids = null,
            Interval<DateTime>? range = null,
            ulong[] projectIds = null,
            ulong[] regionIds = null,
            ulong[] siteIds = null,
            int page = 1)
        {
            var uri = this.AuthenticatedApi.GetAudioRecordingFilterUri(page);

            var filter = QueryFilter.Empty
                .FilterByIds("id", ids)
                .FilterByIds("projects.id", projectIds)
                .FilterByIds("regions.id", regionIds)
                .FilterByIds("sites.id", siteIds)
                .FilterByRange("recorded_date", range)
                .WithProjectionInclude("id", "recorded_date", "sites.name", "site_id", "canonical_file_name")
                .OrderBy("recorded_date");

            var body = this.SerializeContent(
                filter,
                out var stringBody);

            var response = await this.HttpClient.PostAsync(uri, body);

            return await this.ProcessApiResults<AudioRecording>(response, stringBody);
        }

        public async Task<DownloadStats> DownloadOriginalAudioRecording(
            long id,
            string destination,
            Action<long> totalCallback = null,
            Action<int> progressCallback = null)
        {
            var totalTime = Stopwatch.StartNew();
            var headers = Stopwatch.StartNew();
            var url = this.AuthenticatedApi.GetOriginalAudioUri(id);

            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = url,
            };

            // unset the default accept type
            message.Headers.Accept.Clear();

            // get just the headers
            using HttpResponseMessage response = await this.HttpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead);

            headers.Stop();

            if (!response.IsSuccessStatusCode)
            {
                // only called for errors - this call should always throw
                await this.ProcessApiResult<object>(response);
            }

            // Set the max value  of bytes
            totalCallback(response.Content.Headers.ContentLength ?? 0);

            var body = Stopwatch.StartNew();

            ulong total = 0;
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
            {
                var buffer = new byte[8192];
                while (true)
                {
                    var read = await contentStream.ReadAsync(buffer);
                    if (read == 0)
                    {
                        break;
                    }

                    // Increment the number of read bytes for the progress task
                    progressCallback(read);

                    // Write the read bytes to the output stream
                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                    total += (ulong)read;
                }
            }

            body.Stop();
            totalTime.Stop();

            return new DownloadStats(
                destination,
                totalTime.Elapsed,
                headers.Elapsed,
                body.Elapsed,
                total);
        }
    }
}