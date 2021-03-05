// <copyright file="ConfigFileExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ConfigFile
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using JetBrains.Annotations;

    public static class ConfigFileExtensions
    {
        [ContractAnnotation("value:null => halt")]
        public static void NotNull(this object value, FileInfo file, [System.Runtime.CompilerServices.CallerMemberName]string name = null, string message = "must be set in the config file")
        {
            if (value == null)
            {
                throw new ConfigFileException(name + " " + message, file);
            }
        }

        public static ValidationResult ValidateNotNull(this object value, string name, string message = "must be set and be not null in the config file")
        {
            if (value == null)
            {
                return new ValidationResult(message, name.Wrap());
            }

            return ValidationResult.Success;
        }
    }
}