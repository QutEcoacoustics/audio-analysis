// <copyright file="ZioExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Zio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FileSystems;

    public static class ZioExtensions
    {
        private static readonly PhysicalFileSystem FileSystem = new PhysicalFileSystem();

        public static UPath ToUPath(this FileSystemInfo file)
        {
            return FileSystem.ConvertPathFromInternal(file.FullName);
        }
    }
}
