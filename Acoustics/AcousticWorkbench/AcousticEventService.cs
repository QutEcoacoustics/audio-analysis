// <copyright file="AcousticEventService.cs" company="QutEcoacoustics">
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

    public class AcousticEventService : Service
    {
        public AcousticEventService(IAuthenticatedApi api)
            : base(api)
        {
        }

        public async Task<AudioEvent> GetAudioEvent(long audioEventId)
        {
            // currently no direct route is exposed by the API to just get an audio event by ID.
            // We have to use a filter instead.
            var uri = this.AuthenticatedApi.GetAudioEventFilterUri();

            var body = this.SerializeContent(
                new
                {
                    filter = new
                    {
                        id = new
                        {
                            eq = audioEventId,
                        },
                    },
                });

            var response = await this.Client.PostAsync(uri, body);

            return await this.ProcessApiResult<AudioEvent>(response);
        }
    }
}
