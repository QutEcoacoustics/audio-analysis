// <copyright file="OneOfThese.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Property)]
    public class OneOfThese : ValidationAttribute
    {
        public OneOfThese(params string[] validItems)
        {
            this.ValidItems = validItems;
        }

        public string[] ValidItems { get; set; }

        public string ExceptionMessage { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (this.ValidItems == null || this.ValidItems.Length == 0)
            {
                return ValidationResult.Success;
            }

            if (!(value is string))
            {
                return new ValidationResult($"The provided value {value} was not a string (for argument {validationContext.DisplayName}");
            }

            var str = (string)value;
            var match = this.ValidItems.Any(x => string.Equals(x, str, StringComparison.InvariantCultureIgnoreCase));

            if (!match)
            {
                var valids = "{" + this.ValidItems.Aggregate(string.Empty, (seed, current) => seed + ", " + current) + "}";
                return new ValidationResult(
                    this.ExceptionMessage
                    + $"Supplied value {str} for argument {validationContext.DisplayName} does not match any of the allowed values: {valids}");
            }

            return ValidationResult.Success;
        }
    }
}
