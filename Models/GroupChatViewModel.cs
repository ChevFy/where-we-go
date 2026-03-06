using where_we_go.DTO;

namespace where_we_go.ViewModels;

public class GroupChatViewModel
{
    public Guid GroupChatId { get; set; }
    public string name { get; set; } = "";

    public List<MessageDto> messages { get; set; } = new();
}