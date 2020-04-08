// <copyright file="AudioRecordingFactory.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers.Factories
{
    using Acoustics.Shared;
    using Acoustics.Shared.Extensions;
    using global::AcousticWorkbench.Models;
    using Random = Acoustics.Test.TestHelpers.Random;

    public static class AudioRecordingFactory
    {
        public static AudioRecording Create(int? seed = null)
        {
            var random = Random.GetRandom(seed);

            return new AudioRecording()
            {
                Uuid = random.NextGuid().ToString(),
                Id = random.Next(),
                DurationSeconds = random.NextInSequence(3600, 86400, 3600),
                BitRateBps = random.Next(500, 7000),
                Channels = random.NextChoice(1, 2),
                CreatedAt = random.NextDate(),
                DataLengthBytes = random.NextInSequence(1 << 30, (long)(1 << 30) * 8, 1 << 30),
                MediaType = random.NextChoice(MediaTypes.MediaTypeWav, MediaTypes.MediaTypeMp3),
                RecordedDate = random.NextDate(),
                SampleRateHertz = random.NextChoice(8000, 16000, 22050, 24000, 44100, 48000),
                SiteId = random.Next(1000, 2000),
                Status = "Ready",
                UpdatedAt = random.NextDate(),
            };
        }
    }
}