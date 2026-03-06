namespace where_we_go.DTO;

public class MessageDto
{
    public Guid message_id { get; set; }
    public string? user_id { get; set; }
    public string sender_name { get; set; } = "";
    public string message { get; set; } = "";
    public DateTime sent_at { get; set; }
    public bool is_me { get; set; }
}