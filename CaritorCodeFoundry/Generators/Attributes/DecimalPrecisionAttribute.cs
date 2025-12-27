using System;
using System.ComponentModel.DataAnnotations;

namespace CodeFoundry.Generator.Attributes
{
    /// <summary>
    /// Specifies decimal precision (scale only).
    /// Used by ViewModelGenerator for numeric fields.
    /// This attribute does NOT enforce validation by itself.
    /// Validation is typically combined with RegularExpression or server-side rules.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DecimalPrecisionAttribute : ValidationAttribute
    {
        public int Scale { get; }

        public DecimalPrecisionAttribute(int scale)
        {
            if (scale < 0 || scale > 10)
                throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be between 0 and 10.");

            Scale = scale;
            ErrorMessage = $"Value cannot have more than {scale} digits after decimal point.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is decimal dec)
            {
                // Count decimal places
                int actualScale = BitConverter.GetBytes(decimal.GetBits(dec)[3])[2];

                if (actualScale > Scale)
                    return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
