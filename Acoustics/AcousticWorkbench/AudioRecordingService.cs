// <copyright file="AudioRecordingService.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Models;

    public class AudioRecordingService : Service
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
    }
}
