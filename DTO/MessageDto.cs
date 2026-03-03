namespace where_we_go.DTO;
public class MessageDto
{
    public string SenderName { get; set; }
    public string Content { get; set; }
    public bool IsMe { get; set; }

    public MessageDto(string senderName, string content, bool isMe)
    {
        SenderName = senderName;
        Content = content;
        IsMe = isMe;
    }
}