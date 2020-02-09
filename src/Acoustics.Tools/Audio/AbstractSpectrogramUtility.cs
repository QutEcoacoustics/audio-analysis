namespace Acoustics.Tools.Audio
{
    using System.IO;

    public abstract class AbstractSpectrogramUtility : AbstractUtility
    {
        /// <summary>
        /// Directory for temporary files.
        /// </summary>
        protected DirectoryInfo TemporaryFilesDirectory;
    }
}
