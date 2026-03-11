using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.ViewModels;
using where_we_go.Database;
using where_we_go.Service;

namespace where_we_go.Controllers;

[Authorize]
public class GroupChatController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IFileService _fileService;

    public GroupChatController(
        AppDbContext context,
        UserManager<User> userManager,
        IFileService fileService)
    {
        _context = context;
        _userManager = userManager;
        _fileService = fileService;
    }

    public async Task<IActionResult> Chat(Guid group_chat_id)
    {
        var current_user = await _userManager.GetUserAsync(User);
        if (current_user == null)
            return Unauthorized();

        var groupchat = await _context.GroupChats
            .FirstOrDefaultAsync(g => g.GroupChatId == group_chat_id);

        if (groupchat == null)
            return NotFound();

        var isMember = await _chatService.IsUserMemberAsync(group_chat_id, current_user.Id);
        if (!isMember)
            return Forbid();

        var messages = new List<MessageDto>();
        foreach (var m in groupchat.ChatMessages.OrderBy(m => m.SentAt))
        {
            messages.Add(new MessageDto
            {
                message_id = m.MessageId,
                user_id = m.UserId,
                sender_name = m.User.Name,
                sender_avatar = await _fileService.GeneratePresignedProfileUrlAsync(m.User.ProfileImageKey),
                message = m.Message,
                sent_at = m.SentAt,
                is_me = m.UserId == current_user.Id
            });
        }

        var dto = new DTO.GroupChatViewDto
        {
            group_chat_id = groupchat.GroupChatId,
            name = groupchat.GroupChatName,
            messages = messages,
            current_user_id = current_user.Id
        };

        return View(dto);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage(Guid group_chat_id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return RedirectToAction("Chat", new { group_chat_id });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();


        if (!await _chatService.IsUserMemberAsync(group_chat_id, user.Id))
            return Forbid();

        var msg = await _chatService.CreateMessageAsync(group_chat_id, user.Id, message);

        var dto = new MessageDto
        {
            message_id = msg.MessageId,
            user_id = msg.UserId,
            sender_name = user.Name,
            message = msg.Message,
            sent_at = msg.SentAt,
            is_me = true
        };
        await _hubContext.Clients.Group(group_chat_id.ToString())
            .SendAsync("ReceiveMessage", dto);

        return RedirectToAction("Chat", new { group_chat_id });
    }
}