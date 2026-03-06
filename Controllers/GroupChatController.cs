using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.ViewModels;
using where_we_go.Database;

namespace where_we_go.Controllers;

[Authorize]
public class GroupChatController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public GroupChatController(
        AppDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Chat(Guid group_chat_id)
    {
        var current_user = await _userManager.GetUserAsync(User);
        if (current_user == null)
            return Unauthorized();

        var groupchat = await _context.GroupChats
            .Include(g => g.ChatMessages)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.GroupChatId == group_chat_id);

        if (groupchat == null)
            return NotFound();

        var isMember = await _context.Participants
            .AnyAsync(p => p.PostId == groupchat.PostId
                           && p.UserId == current_user.Id
                           && p.Status == Models.Enums.ParticipantStatus.Approved);
        if (!isMember)
            return Forbid();

        var dto = new DTO.GroupChatViewDto
        {
            group_chat_id = groupchat.GroupChatId,
            name = groupchat.GroupChatName,
            messages = groupchat.ChatMessages
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    message_id = m.MessageId,
                    user_id = m.UserId,
                    sender_name = m.User.Name,
                    message = m.Message,
                    sent_at = m.SentAt,
                    is_me = m.UserId == current_user.Id
                })
                .ToList()
        };

        return View(dto);
    }

    [HttpPost]
    [Authorize]
    public Task<IActionResult> SendMessage(Guid group_chat_id, string message)
    {
        return Task.FromResult<IActionResult>(RedirectToAction("Chat", new { group_chat_id }));
    }
}