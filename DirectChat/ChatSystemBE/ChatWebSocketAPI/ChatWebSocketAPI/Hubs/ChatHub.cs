using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatWebSocketAPI.Services;
using ChatWebSocketAPI.DTOs;
using ChatWebSocketAPI.Models;
using static ChatWebSocketAPI.DTOs.ChatDto;

namespace ChatWebSocketAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IAuthService _authService;
        private static readonly Dictionary<string, Guid> _connectionUserMap = new();
        private static readonly Dictionary<Guid, HashSet<string>> _userConnections = new();

        public ChatHub(IChatService chatService, IAuthService authService)
        {
            _chatService = chatService;
            _authService = authService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                _connectionUserMap[Context.ConnectionId] = userId.Value;

                if (!_userConnections.ContainsKey(userId.Value))
                    _userConnections[userId.Value] = new HashSet<string>();

                _userConnections[userId.Value].Add(Context.ConnectionId);

                // Notify others about user online status
                await Clients.All.SendAsync("UserOnlineStatusChanged", userId.Value, true);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectionUserMap.TryGetValue(Context.ConnectionId, out var userId))
            {
                _connectionUserMap.Remove(Context.ConnectionId);

                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Remove(Context.ConnectionId);

                    // If user has no more connections, mark as offline
                    if (_userConnections[userId].Count == 0)
                    {
                        _userConnections.Remove(userId);
                        await Clients.All.SendAsync("UserOnlineStatusChanged", userId, false);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinSession(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid session ID");
                return;
            }

            var userId = GetUserId();
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var session = await _chatService.GetSessionEntityAsync(sessionGuid);
            if (session == null)
            {
                await Clients.Caller.SendAsync("Error", "Session not found");
                return;
            }

            // Check if user has access to this session
            if (session.CustomerId != userId.Value && session.AdminId != userId.Value)
            {
                await Clients.Caller.SendAsync("Error", "Access denied");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Session_{sessionId}");
            await _chatService.UpdateConnectionStateAsync(sessionGuid, ConnectionState.Connected);

            // Send message history to the joining user
            var messages = await _chatService.GetSessionMessagesAsync(sessionGuid);
            await Clients.Caller.SendAsync("MessageHistory", messages);
        }

        public async Task LeaveSession(string sessionId)
        {
            if (Guid.TryParse(sessionId, out var sessionGuid))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Session_{sessionId}");
                await _chatService.UpdateConnectionStateAsync(sessionGuid, ConnectionState.Disconnected);
            }
        }

        public async Task SendMessage(string sessionId, string content)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid session ID");
                return;
            }

            var userId = GetUserId();
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                await Clients.Caller.SendAsync("Error", "Message content cannot be empty");
                return;
            }

            try
            {
                var message = await _chatService.SendMessageAsync(sessionGuid, userId.Value, new SendMessageRequest
                {
                    Content = content.Trim(),
                    SourcePlatform = "Web"
                });

                // Send message to all users in the session
                await Clients.Group($"Session_{sessionId}").SendAsync("ReceiveMessage", message);

                // Update delivery status to delivered for other participants
                await _chatService.UpdateMessageDeliveryStatusAsync(message.Id, MessageDeliveryStatus.Delivered);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        public async Task AssignAndJoinSession(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid session ID");
                return;
            }

            var userId = GetUserId();
            if (!userId.HasValue)
                return;

            // Check if current user is admin
            var currentUser = await _authService.GetUserByIdAsync(userId.Value);
            if (currentUser?.RoleName != RoleNames.Admin)
            {
                await Clients.Caller.SendAsync("Error", "Only admins can assign sessions");
                return;
            }

            try
            {
                var updatedSession = await _chatService.AssignAndJoinSessionAsync(sessionGuid, userId.Value);
                if (updatedSession != null)
                {
                    // Notify all admins about the assignment
                    await Clients.All.SendAsync("SessionAssigned", updatedSession);

                    // Join the session group
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Session_{sessionId}");

                    // Send message history to the admin
                    var messages = await _chatService.GetSessionMessagesAsync(sessionGuid);
                    await Clients.Caller.SendAsync("MessageHistory", messages);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Session not available or already assigned");
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to assign session: {ex.Message}");
            }
        }

        public async Task MarkMessagesAsRead(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
                return;

            var userId = GetUserId();
            if (!userId.HasValue)
                return;

            // This would require additional implementation to mark specific messages as read
            // For now, we'll just acknowledge the request
            await Clients.Caller.SendAsync("MessagesMarkedAsRead", sessionId);
        }

        private Guid? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
