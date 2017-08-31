// <copyright file="MediaService.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Models;

    public partial class MediaService : Service
    {
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

            var response = await this.Client.SendAsync(request);

            return await this.ProcessApiResult<Media>(response);
        }
    }
}
