// <copyright file="ExistingFileAttribute.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using Acoustics.Shared.Contracts;

    [AttributeUsage(AttributeTargets.Property)]
    public class ExistingFileAttribute : ValidationAttribute
    {
        private readonly bool createIfNotExists;

        private readonly bool shouldExist;

        public ExistingFileAttribute(bool createIfNotExists = false, bool shouldExist = true)
        {
            Contract.Requires(shouldExist || createIfNotExists == false);

            this.createIfNotExists = createIfNotExists;
            this.shouldExist = shouldExist;
        }

        public string Extension { get; set; }

        /// <summary>
        /// Validates that the given directory exists.
        /// </summary>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is string path) || path.Length == 0 || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return new ValidationResult(this.FormatErrorMessage(value as string));
            }

            if (Directory.Exists(path))
            {
                if (!this.shouldExist)
                {
                    return new ValidationResult($"The specified file ({path}) for argument {validationContext.DisplayName} exists and should not");
                }
            }
            else
            {
                if (this.createIfNotExists)
                {
                    File.Create(path).Close();
                }
                else
                {
                    string expected = this.shouldExist ? " was expected" : string.Empty;
                    return new ValidationResult($"The specified file ({path}) for argument {validationContext.DisplayName} {expected}, but was not found.");
                }
            }

            path = Path.GetFullPath(path);

            if (this.Extension != null)
            {
                var extension = Path.GetExtension(path);
                if (this.Extension[0] != '.')
                {
                    this.Extension = "." + this.Extension;
                }

                if (!string.Equals(extension, this.Extension, StringComparison.CurrentCultureIgnoreCase))
                {
                    return new ValidationResult(
                        $"Expected an input file with an extensions of {this.Extension}. Instead got {extension} for argument {validationContext.DisplayName}");
                }
            }

            return ValidationResult.Success;
        }
    }
}