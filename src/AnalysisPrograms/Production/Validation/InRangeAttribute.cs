// <copyright file="InRangeAttribute.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    [AttributeUsage(AttributeTargets.Property)]
    public class InRangeAttribute : ValidationAttribute
    {
        private readonly double min;

        private readonly double max;

        public InRangeAttribute(double min = double.MinValue, double max = double.MaxValue)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Validates that the given directory exists.
        /// </summary>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            double number;
            if (value is string str)
            {
                if (!double.TryParse(str, out number))
                {
                    return new ValidationResult($"The number {number} for argument {validationContext.DisplayName} can not be parsed as a number.", validationContext.MemberName.AsArray());
                }
            }
            else if (value is double)
            {
                number = (double)value;
            }
            else
            {
                return new ValidationResult($"The value {value} for argument {validationContext.DisplayName} can not be parsed as a number.", validationContext.MemberName.AsArray());
            }

            if (double.IsNaN(number))
            {
                return new ValidationResult($"The number {number} for argument {validationContext.DisplayName} is not valid", validationContext.MemberName.AsArray());
            }

            if (number > this.max)
            {
                return new ValidationResult($"The number {number} for argument {validationContext.DisplayName} is greater than allowed limit {this.max}", validationContext.MemberName.AsArray());
            }

            if (number < this.min)
            {
                return new ValidationResult($"The number {number} for argument {validationContext.DisplayName} is less than allowed limit {this.min}", validationContext.MemberName.AsArray());
            }

            return ValidationResult.Success;
        }
    }
}