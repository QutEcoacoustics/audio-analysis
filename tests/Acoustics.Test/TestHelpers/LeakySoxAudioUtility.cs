// <copyright file="LeakySoxAudioUtility.cs" company="QutEcoacoustics">
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
    public class LeakySoxAudioUtility : SoxAudioUtility
    {
        public LeakySoxAudioUtility(FileInfo soxExe)
            : base(soxExe)
        {
        }

        public LeakySoxAudioUtility(FileInfo soxExe, DirectoryInfo temporaryFilesDirectory, bool enableShortNameHack = true)
            : base(soxExe, temporaryFilesDirectory, enableShortNameHack)
        {
        }

        public string GetConstructedModifyArguments(FileInfo source, FileInfo output, AudioUtilityRequest request)
        {
            return this.ConstructModifyArgs(source, output, request);
        }

        public string GetConstructedInfoArguments(FileInfo source)
        {
            return this.ConstructInfoArgs(source);
        }
    }
}