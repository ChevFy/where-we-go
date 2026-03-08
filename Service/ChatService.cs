using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using where_we_go.Database;
using where_we_go.Models;
using where_we_go.DTO;

namespace where_we_go.Service
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;

        public ChatService(AppDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<bool> IsUserMemberAsync(Guid groupChatId, string userId)
        {
            var chat = await _db.GroupChats.FindAsync(groupChatId);
            if (chat == null)
                return false;
            return await _db.Participants.AnyAsync(p => p.PostId == chat.PostId
                                                       && p.UserId == userId
                                                       && p.Status == Models.Enums.ParticipantStatus.Approved);
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(Guid groupChatId)
        {
            var msgs = await _db.ChatMessages
                .Where(m => m.GroupChatId == groupChatId)
                .Include(m => m.User)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
            return msgs;
        }

        public async Task<ChatMessage> CreateMessageAsync(Guid groupChatId, string userId, string message)
        {
            var msg = new ChatMessage
            {
                MessageId = Guid.NewGuid(),
                GroupChatId = groupChatId,
                UserId = userId,
                Message = message,
                SentAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();
            return msg;
        }

        public async Task<Guid> EnsureGroupChatExistsForPostAsync(Guid postId, string title)
        {
            var existing = await _db.GroupChats
                .Where(g => g.PostId == postId)
                .Select(g => g.GroupChatId)
                .FirstOrDefaultAsync();
            if (existing != Guid.Empty)
                return existing;

            var chat = new GroupChat
            {
                GroupChatId = Guid.NewGuid(),
                PostId = postId,
                GroupChatName = title
            };
            _db.GroupChats.Add(chat);
            await _db.SaveChangesAsync();
            return chat.GroupChatId;
        }
    }
}