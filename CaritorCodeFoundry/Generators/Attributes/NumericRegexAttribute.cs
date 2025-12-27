using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CodeFoundry.Generator.Attributes
{
    /// <summary>
    /// Validates that a property contains only numeric values,
    /// with optional decimal part (up to max decimal places).
    /// 
    /// Example patterns:
    ///   - max 2 decimals: "123", "123.4", "123.45"
    ///   - no commas, no currency symbols
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NumericRegexAttribute : ValidationAttribute
    {
        public int MaxDecimalPlaces { get; }
        private readonly Regex _regex;

        public NumericRegexAttribute(int maxDecimalPlaces = 2)
        {
            if (maxDecimalPlaces < 0 || maxDecimalPlaces > 10)
                throw new ArgumentOutOfRangeException(nameof(maxDecimalPlaces), "Decimal places must be between 0 and 10.");

            MaxDecimalPlaces = maxDecimalPlaces;

            // Regex:
            // ^\d+(\.\d{0,N})?$
            string pattern = @"^\d+(\.\d{0," + maxDecimalPlaces + "})?$";

            _regex = new Regex(pattern, RegexOptions.Compiled);
            ErrorMessage = $"Only numbers allowed; with maximum {maxDecimalPlaces} decimal places.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var str = value.ToString().Trim();

            if (string.IsNullOrEmpty(str))
                return ValidationResult.Success;

            if (_regex.IsMatch(str))
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage);
        }
    }
}
