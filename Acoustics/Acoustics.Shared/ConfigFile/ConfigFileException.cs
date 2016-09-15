// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigFileException.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the ConfigFileException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.ConfigFile
{
    using System;

    public class ConfigFileException : Exception
    {
        public const string Prelude = "Configuration exception: ";

        public ConfigFileException(string message)
            : base(message)
        {
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

        public ConfigFileException()
        {
        }

        public string File { get; set; }

        public override string Message => Prelude + base.Message;
    }
}