namespace where_we_go.DTO;

public class GroupChatViewDto
{
    public Guid group_chat_id { get; set; }
    public string? name { get; set; }
    public string current_user_id { get; set; } = "";
    public List<MessageDto> messages { get; set; } = [];
}

public class GroupChatListItemDto
{
    public Guid group_chat_id { get; set; }
    public string? name { get; set; }

    public Guid post_id { get; set; }
    public string? post_title { get; set; }

    public bool is_owner { get; set; }
}