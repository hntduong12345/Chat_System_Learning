using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TestChatAPI.Enums;
using TestChatAPI.Models;
using TestChatAPI.Payloads.ChatMessages;
using TestChatAPI.Payloads.ChatSessions;
using TestChatAPI.Services;

namespace TestChatAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly TestChatSystemDbContext _context;
        private readonly IChatSessionService _chatSessionService;
        private readonly IMapper _mapper;

        public ChatHub(TestChatSystemDbContext context, IChatSessionService chatSessionService, IMapper mapper)
        {
            _context = context;
            _chatSessionService = chatSessionService;
            _mapper = mapper;
        }

        public async Task JoinSession(int sessionId, string userId, string userName)
        {
            var session = await _chatSessionService.GetSessionEntityAsync(sessionId);

            if (session == null || !session.IsActive)
            {
                await Clients.Caller.SendAsync("Error", "Session not found or inactive");
                return;
            }

            // Update connection ID
            session.ConnectionId = Context.ConnectionId;
            await _context.SaveChangesAsync();

            // Join SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Session_{sessionId}");

            // Notify others in the session
            await Clients.Group($"Session_{sessionId}")
                         .SendAsync("UserJoined", userName, DateTime.UtcNow);

            // Send recent messages to the user
            var recentMessages = await _chatSessionService.GetSessionMessagesAsync(sessionId, 1, 50);
            await Clients.Caller.SendAsync("MessageHistory", recentMessages);
        }

        public async Task SendMessage(int sessionId, string userId, string userName, string message)
        {
            try
            {
                var session = await _chatSessionService.GetSessionEntityAsync(sessionId);

                if (session == null || !session.IsActive)
                {
                    await Clients.Caller.SendAsync("Error", "Session not found or inactive");
                    return;
                }

                // Save message to database
                var chatMessage = await _chatSessionService.CreateMessageAsync(
                    sessionId, userId, userName, message, MessageType.User);

                // Convert to DTO for broadcasting
                var messageDto = _mapper.Map<GetChatMessageResponse>(chatMessage);

                // Broadcast message to all users in the session
                await Clients.Group($"Session_{sessionId}")
                             .SendAsync("ReceiveMessage", messageDto);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        public async Task LeaveSession(int sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Session_{sessionId}");

            // Update connection ID in database
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.ConnectionId == Context.ConnectionId);

            if (session != null)
            {
                session.ConnectionId = null;
                await _context.SaveChangesAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Clean up connection ID when user disconnects
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.ConnectionId == Context.ConnectionId);

            if (session != null)
            {
                session.ConnectionId = null;
                await _context.SaveChangesAsync();
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
