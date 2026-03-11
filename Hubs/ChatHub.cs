using Microsoft.AspNetCore.SignalR;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Service;
using where_we_go.Database;
using Microsoft.AspNetCore.Identity;

namespace where_we_go.Hubs;

public class ChatHub : Hub
{
    private readonly UserManager<User> _userManager;
    private readonly IFileService _fileService;

    public ChatHub(AppDbContext db, UserManager<User> userManager, IFileService fileService)
    {
        _userManager = userManager;
        _fileService = fileService;
    }

    public async Task JoinGroup(Guid groupChatId)
    {
        Console.WriteLine($"[ChatHub] {Context.ConnectionId} joining group {groupChatId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, groupChatId.ToString());
    }
    

    public async Task LeaveGroup(Guid groupChatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupChatId.ToString());
    }

    public async Task SendMessage(Guid groupChatId, string message)
    {
        Console.WriteLine($"[ChatHub] SendMessage called by {Context.ConnectionId} for group {groupChatId}: '{message}'");
        if (string.IsNullOrWhiteSpace(message)) return;

        var user = await _userManager.GetUserAsync(Context.User as System.Security.Claims.ClaimsPrincipal);
        if (user == null) return;

        
        var isMember = await _chatService.IsUserMemberAsync(groupChatId, user.Id);
        if (!isMember) return;

        var msg = new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            GroupChatId = groupChatId,
            UserId = user.Id,
            Message = message,
            SentAt = DateTime.UtcNow
        };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        var avatarUrl = await _fileService.GeneratePresignedProfileUrlAsync(user.ProfileImageKey);

        var dto = new MessageDto
        {
            message_id = msg.MessageId,
            user_id = msg.UserId,
            sender_name = user.Name,
            sender_avatar = avatarUrl,
            message = msg.Message,
            sent_at = msg.SentAt,
            is_me = false
        };

        await Clients.Group(groupChatId.ToString()).SendAsync("ReceiveMessage", dto);
    }
}
