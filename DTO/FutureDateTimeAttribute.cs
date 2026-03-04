using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class FutureDateTimeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dateProperty = validationContext.ObjectType.GetProperty("DateDeadline");
            var timeProperty = validationContext.ObjectType.GetProperty("TimeDeadline");

            if (dateProperty == null || timeProperty == null)
            {
                return ValidationResult.Success;
            }

            var dateValue = dateProperty.GetValue(validationContext.ObjectInstance);
            var timeValue = timeProperty.GetValue(validationContext.ObjectInstance);

            if (dateValue is DateTime date && timeValue is TimeOnly time)
            {
                var combinedDateTime = date.Add(time.ToTimeSpan());
                var now = DateTime.Now;

                // Allow same date but time must be in the future
                if (combinedDateTime <= now)
                {
                    return new ValidationResult("Event date and time must be in the future.", new[] { "DateDeadline", "TimeDeadline" });
                }
            }

            return ValidationResult.Success;
        }
    }
}
