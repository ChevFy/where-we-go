using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class EventAfterDeadlineAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dateDeadlineProperty = validationContext.ObjectType.GetProperty("DateDeadline");
            var timeDeadlineProperty = validationContext.ObjectType.GetProperty("TimeDeadline");
            var eventDateProperty = validationContext.ObjectType.GetProperty("EventDate");
            var eventTimeProperty = validationContext.ObjectType.GetProperty("EventTime");

            if (dateDeadlineProperty == null || timeDeadlineProperty == null ||
                eventDateProperty == null || eventTimeProperty == null)
            {
                return ValidationResult.Success;
            }

            var dateDeadline = dateDeadlineProperty.GetValue(validationContext.ObjectInstance);
            var timeDeadline = timeDeadlineProperty.GetValue(validationContext.ObjectInstance);
            var eventDate = eventDateProperty.GetValue(validationContext.ObjectInstance);
            var eventTime = eventTimeProperty.GetValue(validationContext.ObjectInstance);

            if (dateDeadline is DateTime dd && timeDeadline is TimeOnly td &&
                eventDate is DateTime ed && eventTime is TimeOnly et)
            {
                var deadline = dd.Add(td.ToTimeSpan());
                var eventDateTime = ed.Add(et.ToTimeSpan());

                if (eventDateTime <= deadline)
                {
                    return new ValidationResult(
                        "Event date and time must be after the deadline.",
                        new[] { "EventDate", "EventTime" });
                }
            }

            return ValidationResult.Success;
        }
    }
}