using TestChatAPI.Enums;
using TestChatAPI.Models;
using TestChatAPI.Payloads.ChatMessages;
using TestChatAPI.Payloads.ChatSessions;

namespace TestChatAPI.Services
{
    public interface IChatSessionService
    {
        Task<GetChatSessionWithMessageResponse?> GetSessionAsync(int sessionId);
        Task<GetChatSessionResponse> CreateSessionAsync(string sessionName, string userId);
        Task<GetChatSessionResponse?> CloseSessionAsync(int sessionId, string userId);
        Task<GetChatSessionResponse?> ReactivateSessionAsync(int sessionId, string userId);
        Task<IEnumerable<GetChatSessionResponse>> GetUserSessionsAsync(string userId);
        Task<IEnumerable<GetChatMessageResponse>> GetSessionMessagesAsync(int sessionId, int page = 1, int pageSize = 50);

        // Internal methods for SignalR Hub (still return entities)
        Task<ChatSession?> GetSessionEntityAsync(int sessionId);
        Task<ChatMessage> CreateMessageAsync(int sessionId, string userId, string userName, string content, MessageType type = MessageType.User);
    }
}
