using Microsoft.AspNetCore.SignalR;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Service;
using Microsoft.AspNetCore.Identity;

namespace where_we_go.Hubs;

public class ChatHub : Hub
{
    private readonly UserManager<User> _userManager;
    private readonly IChatService _chatService;

    public ChatHub(UserManager<User> userManager, IChatService chatService)
    {
        _userManager = userManager;
        _chatService = chatService;
    }

    // helpful debug logging so you can watch what the hub sees
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"[ChatHub] Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[ChatHub] Client disconnected: {Context.ConnectionId} (exc={exception})");
        return base.OnDisconnectedAsync(exception);
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

        var msg = await _chatService.CreateMessageAsync(groupChatId, user.Id, message);
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
