// <copyright file="DirectoryExistsOrCreateAttribute.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using Acoustics.Shared.Contracts;

    /// <inheritdoc />
    /// <summary>
    /// Validates that if the user specifies a value for a property that the value represents a directory that exists
    /// as determined by System.IO.Directory.Exists(directory).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DirectoryExistsOrCreateAttribute : ValidationAttribute
    {
        private readonly bool createIfNotExists;

        private readonly bool shouldExist;

        public DirectoryExistsOrCreateAttribute(bool createIfNotExists = false, bool shouldExist = true)
        {
            Contract.Requires(shouldExist || createIfNotExists == false);

            this.createIfNotExists = createIfNotExists;
            this.shouldExist = shouldExist;
        }

        /// <inheritdoc />
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
                    return new ValidationResult($"The specified directory ({path}) for argument {validationContext.DisplayName} exists and should not");
                }
            }
            else
            {
                if (this.createIfNotExists)
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    string expected = this.shouldExist ? " was expected" : string.Empty;
                    return new ValidationResult($"The specified directory ({path}) for argument {validationContext.DisplayName} {expected}, but was not found.");
                }
            }

            return ValidationResult.Success;
        }
    }
}