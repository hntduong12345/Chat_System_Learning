using AutoMapper;
using ChatWebSocketAPI.Models;
using Microsoft.EntityFrameworkCore;
using static ChatWebSocketAPI.DTOs.ChatDto;

namespace ChatWebSocketAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatDbContext _context;
        private readonly IMapper _mapper;

        public ChatService(ChatDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ChatSessionDto> CreateSessionAsync(Guid customerId, CreateSessionRequest request)
        {
            var session = new ChatSession
            {
                CustomerId = customerId,
                Status = SessionStatus.Waiting,
                CreateAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                ChannelType = request.ChannelType,
                ConnectionState = ConnectionState.Connected
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            // Send initial message if provided
            if (!string.IsNullOrEmpty(request.InitialMessage))
            {
                await SendMessageAsync(session.Id, customerId, new SendMessageRequest
                {
                    Content = request.InitialMessage,
                    SourcePlatform = request.ChannelType
                });
            }

            // Reload with navigation properties
            session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Messages)
                .FirstAsync(s => s.Id == session.Id);

            return _mapper.Map<ChatSessionDto>(session);
        }

        public async Task<ChatSessionDto> CreateSessionWithAdminAsync(Guid customerId, CreateSessionWithAdminRequest request)
        {
            // Verify admin exists and is actually an admin
            var admin = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == request.AdminId && u.Role.Name == RoleNames.Admin);

            if (admin == null)
                throw new ArgumentException("Invalid admin specified");

            var session = new ChatSession
            {
                CustomerId = customerId,
                AdminId = request.AdminId, // Pre-assign to selected admin
                Status = SessionStatus.Active, // Start as active since admin is pre-selected
                CreateAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                ChannelType = request.ChannelType,
                ConnectionState = ConnectionState.Connected
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            // Send initial message if provided
            if (!string.IsNullOrEmpty(request.InitialMessage))
            {
                await SendMessageAsync(session.Id, customerId, new SendMessageRequest
                {
                    Content = request.InitialMessage,
                    SourcePlatform = request.ChannelType
                });
            }

            // Reload with navigation properties
            session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .Include(s => s.Messages)
                .FirstAsync(s => s.Id == session.Id);

            return _mapper.Map<ChatSessionDto>(session);
        }

        public async Task<ChatSessionWithMessagesDto?> GetSessionAsync(Guid sessionId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .Include(s => s.Messages.OrderBy(m => m.SendAt))
                    .ThenInclude(m => m.Sender).ThenInclude(s => s.Role)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            return session == null ? null : _mapper.Map<ChatSessionWithMessagesDto>(session);
        }

        public async Task<IEnumerable<ChatSessionDto>> GetCustomerSessionsAsync(Guid customerId)
        {
            var sessions = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .Include(s => s.Messages)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.LastActiveAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatSessionDto>>(sessions);
        }

        public async Task<IEnumerable<ChatSessionDto>> GetAdminSessionsAsync(Guid adminId)
        {
            var sessions = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .Include(s => s.Messages)
                .Where(s => s.AdminId == adminId && s.Status != SessionStatus.Waiting)
                .OrderByDescending(s => s.LastActiveAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatSessionDto>>(sessions);
        }

        public async Task<IEnumerable<ChatSessionDto>> GetWaitingSessionsAsync()
        {
            var sessions = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Messages)
                .Where(s => s.Status == SessionStatus.Waiting)
                .OrderBy(s => s.CreateAt) // First come, first served
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatSessionDto>>(sessions);
        }

        public async Task<ChatSessionDto?> AssignSessionAsync(Guid sessionId, Guid adminId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == SessionStatus.Waiting);

            if (session == null) return null;

            session.AdminId = adminId;
            session.Status = SessionStatus.Active;
            session.LastActiveAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload with admin
            session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .Include(s => s.Messages)
                .FirstAsync(s => s.Id == sessionId);

            return _mapper.Map<ChatSessionDto>(session);
        }

        public async Task<ChatSessionDto?> AssignAndJoinSessionAsync(Guid sessionId, Guid adminId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return null;

            // If it's a waiting session, assign it
            if (session.Status == SessionStatus.Waiting)
            {
                session.AdminId = adminId;
                session.Status = SessionStatus.Active;
            }
            // If it's already assigned to this admin, just update activity
            else if (session.AdminId == adminId)
            {
                // Already assigned to this admin, just update activity
            }
            // If assigned to different admin, don't allow
            else if (session.AdminId != null && session.AdminId != adminId)
            {
                return null; // Already assigned to different admin
            }

            session.LastActiveAt = DateTime.UtcNow;
            session.ConnectionState = ConnectionState.Connected;

            await _context.SaveChangesAsync();

            // Reload with admin
            session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .Include(s => s.Messages)
                .FirstAsync(s => s.Id == sessionId);

            return _mapper.Map<ChatSessionDto>(session);
        }

        public async Task<ChatSessionDto?> CloseSessionAsync(Guid sessionId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return null;

            session.Status = SessionStatus.Closed;
            session.CloseAt = DateTime.UtcNow;
            session.ConnectionState = ConnectionState.Disconnected;

            await _context.SaveChangesAsync();

            return _mapper.Map<ChatSessionDto>(session);
        }

        public async Task<bool> UpdateSessionActivityAsync(Guid sessionId)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session == null) return false;

            session.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateConnectionStateAsync(Guid sessionId, string connectionState)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session == null) return false;

            session.ConnectionState = connectionState;
            session.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid sessionId, Guid senderId, SendMessageRequest request)
        {
            var message = new ChatMessage
            {
                ChatSessionId = sessionId,
                SenderId = senderId,
                Content = request.Content,
                SendAt = DateTime.UtcNow,
                DeliveryStatus = MessageDeliveryStatus.Sent,
                SourcePlatform = request.SourcePlatform
            };

            _context.ChatMessages.Add(message);

            // Update session activity
            await UpdateSessionActivityAsync(sessionId);

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            message = await _context.ChatMessages
                .Include(m => m.Sender).ThenInclude(s => s.Role)
                .FirstAsync(m => m.Id == message.Id);

            return _mapper.Map<ChatMessageDto>(message);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, int page = 1, int pageSize = 50)
        {
            var messages = await _context.ChatMessages
                .Include(m => m.Sender).ThenInclude(s => s.Role)
                .Where(m => m.ChatSessionId == sessionId)
                .OrderByDescending(m => m.SendAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.SendAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatMessageDto>>(messages);
        }

        public async Task<bool> UpdateMessageDeliveryStatusAsync(Guid messageId, string status)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message == null) return false;

            message.DeliveryStatus = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadMessageCountAsync(Guid sessionId, Guid userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId &&
                           m.SenderId != userId &&
                           m.DeliveryStatus != MessageDeliveryStatus.Read)
                .CountAsync();
        }

        public async Task<ChatSession?> GetSessionEntityAsync(Guid sessionId)
        {
            return await _context.ChatSessions
                .Include(s => s.Customer).ThenInclude(c => c.Role)
                .Include(s => s.Admin).ThenInclude(a => a!.Role)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<User?> GetUserEntityAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
