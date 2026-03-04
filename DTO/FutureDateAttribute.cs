using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateTime)
            {
                // Allow today's date or future dates
                return dateTime.Date >= DateTime.Now.Date;
            }
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be today or a future date.";
        }
    }
}
