using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IChatService
    {
        Task<bool> IsUserMemberAsync(Guid groupChatId, string userId);
        Task<List<ChatMessage>> GetMessagesAsync(Guid groupChatId);
        Task<ChatMessage> CreateMessageAsync(Guid groupChatId, string userId, string message);
        Task<Guid> EnsureGroupChatExistsForPostAsync(Guid postId, string title);
    }
}