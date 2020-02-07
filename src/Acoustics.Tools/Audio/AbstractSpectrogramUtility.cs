namespace Acoustics.Tools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public abstract class AbstractSpectrogramUtility : AbstractUtility
    {
        /// <summary>
        /// Directory for temporary files.
        /// </summary>
        protected DirectoryInfo TemporaryFilesDirectory;
    }
}
