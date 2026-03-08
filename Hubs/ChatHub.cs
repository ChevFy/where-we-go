using Microsoft.AspNetCore.SignalR;
using where_we_go.DTO;
using where_we_go.Database;
using where_we_go.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace where_we_go.Hubs;

public class ChatHub : Hub
{
    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;

    public ChatHub(AppDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task JoinGroup(Guid groupChatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupChatId.ToString());
    }

    public async Task LeaveGroup(Guid groupChatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupChatId.ToString());
    }

    public async Task SendMessage(Guid groupChatId, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var user = await _userManager.GetUserAsync(Context.User as System.Security.Claims.ClaimsPrincipal);
        if (user == null) return;

        var chat = await _db.GroupChats.FindAsync(groupChatId);
        if (chat == null) return;

        
        var isMember = await _db.Participants.AnyAsync(p => p.PostId == chat.PostId
                                                             && p.UserId == user.Id
                                                             && p.Status == Models.Enums.ParticipantStatus.Approved);
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

        var dto = new MessageDto
        {
            message_id = msg.MessageId,
            user_id = msg.UserId,
            sender_name = user.Name,
            message = msg.Message,
            sent_at = msg.SentAt,
            is_me = true
        };

        
        await Clients.Group(groupChatId.ToString()).SendAsync("ReceiveMessage", dto);
    }
}
