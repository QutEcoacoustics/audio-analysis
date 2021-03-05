// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigFileException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ConfigFileException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;

    public class ConfigFileException : Exception
    {
        public const string Prelude = "Configuration exception";

        public ConfigFileException(string message)
            : base(message)
        {
        }

        public ConfigFileException(IEnumerable<ValidationResult> validations, string file)
            : base(FormatValidations(validations))
        {
            this.File = file;
        }

        public ConfigFileException(IEnumerable<ValidationResult> validations, FileInfo file)
            : base(FormatValidations(validations))
        {
            this.File = file.FullName;
        }

        public ConfigFileException(string message, string file)
            : base(message)
        {
            this.File = file;
        }

        public ConfigFileException(string message, Exception innerException, string file)
            : base(message, innerException)
        {
            this.File = file;
        }

        public ConfigFileException(string message, FileInfo file)
            : base(message, null)
        {
            this.File = file.FullName;
        }

        public ConfigFileException()
        {
        }

        public string File { get; set; }

        public string ProfileName { get; init; }

        public override string Message => Prelude + this.ProfileName?.Prepend(" in profile ") + ":" + base.Message + "\nin config file: " + (this.File ?? "<unknown>");

        private static string FormatValidations(IEnumerable<ValidationResult> validations)
        {
            var filtered = validations?.Where(v => v is not null);
            if (filtered.IsNullOrEmpty())
            {
                throw new ArgumentException("There are no errors in the validation list");
            }

            return filtered
                .Select(x => x.MemberNames.Join(",") + ": " + x.ErrorMessage)
                .FormatList();
        }
    }
}