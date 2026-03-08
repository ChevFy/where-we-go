namespace where_we_go.DTO
{
    public class ApplicantDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ProfileImageKey { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Approved, etc.
        public DateTime DateJoin { get; set; }

        public string DateJoinFormatted => DateJoin.ToString("dd MMM yyyy HH:mm");
    }
}