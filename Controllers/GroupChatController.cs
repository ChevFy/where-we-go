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
            .Include(g => g.ChatMessages)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.GroupChatId == group_chat_id);

        if (groupchat == null)
            return NotFound();

        // Owner of the post should always be able to see the chat,
        // even if they are not in Participants table.
        var post = await _context.Posts.FindAsync(groupchat.PostId);
        var isOwner = post != null && post.UserId == current_user.Id;

        if (!isOwner)
        {
            var isMember = await _context.Participants
                .AnyAsync(p => p.PostId == groupchat.PostId
                               && p.UserId == current_user.Id
                               && p.Status == Models.Enums.ParticipantStatus.Approved);
            if (!isMember)
                return Forbid();
        }

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
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> SendMessage(Guid group_chat_id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return RedirectToAction("Chat", new { group_chat_id });
        }

        var current_user = await _userManager.GetUserAsync(User);
        if (current_user == null)
        {
            return Unauthorized();
        }

        var groupchat = await _context.GroupChats.FindAsync(group_chat_id);
        if (groupchat == null)
        {
            return NotFound();
        }

        var post = await _context.Posts.FindAsync(groupchat.PostId);
        var isOwner = post != null && post.UserId == current_user.Id;

        if (!isOwner)
        {
            var isMember = await _context.Participants
                .AnyAsync(p => p.PostId == groupchat.PostId
                               && p.UserId == current_user.Id
                               && p.Status == Models.Enums.ParticipantStatus.Approved);
            if (!isMember)
            {
                return Forbid();
            }
        }

        var msg = new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            GroupChatId = group_chat_id,
            UserId = current_user.Id,
            Message = message,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(msg);
        await _context.SaveChangesAsync();

        return RedirectToAction("Chat", new { group_chat_id });
    }
}