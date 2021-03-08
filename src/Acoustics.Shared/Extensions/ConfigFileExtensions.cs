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

        public static ValidationResult ValidateNotEmpty<T>(this T[] value, string name, string message = "is an empty list - we need at least one value in the config file")
        {
            if (value?.Length == 0)
            {
                return new ValidationResult(message, name.Wrap());
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateLessThan<T>(this object _, T? a, string nameA, T? b, string nameB, string message = "{0} is not less than {1} - adjust the values in the config file")
            where T : struct, IComparable<T>
        {
            if (a.HasValue && b.HasValue && a.Value.CompareTo(b.Value) != -1)
            {
                return new ValidationResult(message.Format(nameA, nameB), new[] { nameA, nameB });
            }

            return ValidationResult.Success;
        }
    }

}