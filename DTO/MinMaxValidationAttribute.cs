using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class MinMaxValidationAttribute : ValidationAttribute
    {
        private readonly string _minPropertyName;
        private readonly string _maxPropertyName;

        public MinMaxValidationAttribute(string minPropertyName, string maxPropertyName)
        {
            _minPropertyName = minPropertyName;
            _maxPropertyName = maxPropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var minProperty = validationContext.ObjectType.GetProperty(_minPropertyName);
            var maxProperty = validationContext.ObjectType.GetProperty(_maxPropertyName);

            if (minProperty == null || maxProperty == null)
            {
                return ValidationResult.Success;
            }

            var minValue = minProperty.GetValue(validationContext.ObjectInstance);
            var maxValue = maxProperty.GetValue(validationContext.ObjectInstance);

            if (minValue is int min && maxValue is int max)
            {
                if (min >= max)
                {
                    return new ValidationResult($"Minimum participants must be less than maximum participants.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
