// <copyright file="ConfigFileExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public static class ConfigFileExtensions
    {
        [ContractAnnotation("value:null => halt")]
        public static void ConfigNotNull(this object value, string name, FileInfo file, string message = "must be set in the config file")
        {
            if (value == null)
            {
                throw new ConfigFileException(name + " " + message, file);
            }
        }
    }
}
