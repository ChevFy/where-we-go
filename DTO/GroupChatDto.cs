namespace where_we_go.DTO;

public class GroupChatViewDto
{
    public Guid group_chat_id { get; set; }
    public string? name { get; set; }
    public List<MessageDto> messages { get; set; } = [];
}