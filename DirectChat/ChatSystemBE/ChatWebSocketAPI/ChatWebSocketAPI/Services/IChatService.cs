using ChatWebSocketAPI.Models;
using static ChatWebSocketAPI.DTOs.ChatDto;

namespace ChatWebSocketAPI.Services
{
    public interface IChatService
    {
        // Session management
        Task<ChatSessionDto> CreateSessionAsync(Guid customerId, CreateSessionRequest request);
        Task<ChatSessionDto> CreateSessionWithAdminAsync(Guid customerId, CreateSessionWithAdminRequest request);
        Task<ChatSessionWithMessagesDto?> GetSessionAsync(Guid sessionId);
        Task<IEnumerable<ChatSessionDto>> GetCustomerSessionsAsync(Guid customerId);
        Task<IEnumerable<ChatSessionDto>> GetAdminSessionsAsync(Guid adminId);
        Task<IEnumerable<ChatSessionDto>> GetWaitingSessionsAsync();
        Task<ChatSessionDto?> AssignSessionAsync(Guid sessionId, Guid adminId);
        Task<ChatSessionDto?> AssignAndJoinSessionAsync(Guid sessionId, Guid adminId);
        Task<ChatSessionDto?> CloseSessionAsync(Guid sessionId);
        Task<bool> UpdateSessionActivityAsync(Guid sessionId);
        Task<bool> UpdateConnectionStateAsync(Guid sessionId, string connectionState);

        // Message management
        Task<ChatMessageDto> SendMessageAsync(Guid sessionId, Guid senderId, SendMessageRequest request);
        Task<IEnumerable<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, int page = 1, int pageSize = 50);
        Task<bool> UpdateMessageDeliveryStatusAsync(Guid messageId, string status);
        Task<int> GetUnreadMessageCountAsync(Guid sessionId, Guid userId);

        // Internal methods for SignalR
        Task<ChatSession?> GetSessionEntityAsync(Guid sessionId);
        Task<User?> GetUserEntityAsync(Guid userId);
    }
}
