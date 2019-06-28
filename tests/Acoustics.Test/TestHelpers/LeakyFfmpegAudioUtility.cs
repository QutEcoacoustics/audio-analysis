// <copyright file="LeakyFfmpegAudioUtility.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System.IO;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    /// <summary>
    /// So named because we use it to break our abstractions, to make them leaky.
    /// </summary>
    public class LeakyFfmpegAudioUtility : FfmpegAudioUtility
    {
        public LeakyFfmpegAudioUtility(FileInfo ffmpegExe, FileInfo ffprobeExe)
            : base(ffmpegExe, ffprobeExe)
        {
        }

        public LeakyFfmpegAudioUtility(FileInfo ffmpegExe, FileInfo ffprobeExe, DirectoryInfo temporaryFilesDirectory)
            : base(ffmpegExe, ffprobeExe, temporaryFilesDirectory)
        {
        }

        public string GetConstructedModifyArguments(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            return this.ConstructModifyArgs(source, output, request);
        }

        public string GetConstructedInfoArguments(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            return this.ConstructInfoArgs(source);
        }
    }
}