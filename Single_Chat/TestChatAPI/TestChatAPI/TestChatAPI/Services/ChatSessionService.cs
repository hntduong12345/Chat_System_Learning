using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestChatAPI.Enums;
using TestChatAPI.Models;
using TestChatAPI.Payloads.ChatMessages;
using TestChatAPI.Payloads.ChatSessions;

namespace TestChatAPI.Services
{
    public class ChatSessionService : IChatSessionService
    {
        private readonly TestChatSystemDbContext _context;
        private readonly IMapper _mapper;

        public ChatSessionService(TestChatSystemDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<GetChatSessionWithMessageResponse?> GetSessionAsync(int sessionId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Messages.OrderBy(m => m.Timestamp))
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            return session == null ? null : _mapper.Map<GetChatSessionWithMessageResponse>(session);
        }

        public async Task<GetChatSessionResponse> CreateSessionAsync(string sessionName, string userId)
        {
            var session = new ChatSession
            {
                SessionName = sessionName,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            return _mapper.Map<GetChatSessionResponse>(session);
        }

        public async Task<GetChatSessionResponse?> CloseSessionAsync(int sessionId, string userId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null) return null;

            session.IsActive = false;
            session.ClosedAt = DateTime.UtcNow;
            session.ConnectionId = null;

            await _context.SaveChangesAsync();

            return _mapper.Map<GetChatSessionResponse>(session);
        }

        public async Task<GetChatSessionResponse?> ReactivateSessionAsync(int sessionId, string userId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null) return null;

            session.IsActive = true;
            session.ClosedAt = null;

            await _context.SaveChangesAsync();

            return _mapper.Map<GetChatSessionResponse>(session);
        }

        public async Task<IEnumerable<GetChatSessionResponse>> GetUserSessionsAsync(string userId)
        {
            var sessions = await _context.ChatSessions
                .Include(s => s.Messages)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GetChatSessionResponse>>(sessions);
        }

        public async Task<IEnumerable<GetChatMessageResponse>> GetSessionMessagesAsync(int sessionId, int page = 1, int pageSize = 50)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GetChatMessageResponse>>(messages);
        }

        // Internal methods for SignalR Hub
        public async Task<ChatSession?> GetSessionEntityAsync(int sessionId)
        {
            return await _context.ChatSessions
                .Include(s => s.Messages.OrderBy(m => m.Timestamp))
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<ChatMessage> CreateMessageAsync(int sessionId, string userId, string userName, string content, MessageType type = MessageType.User)
        {
            var chatMessage = new ChatMessage
            {
                ChatSessionId = sessionId,
                UserId = userId,
                UserName = userName,
                Content = content,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return chatMessage;
        }
    }
}
