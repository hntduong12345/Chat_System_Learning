using ChatWebSocketAPI.Models;
using ChatWebSocketAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ChatWebSocketAPI.DTOs.ChatDto;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatWebSocketAPI.Hubs;

namespace ChatWebSocketAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IAuthService _authService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IAuthService authService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _authService = authService;
            _hubContext = hubContext;
        }

        [HttpPost("sessions")]
        public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionRequest request)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user?.RoleName != RoleNames.Customer)
                return Forbid("Only customers can create chat sessions");

            var session = await _chatService.CreateSessionAsync(userId.Value, request);

            // Notify the targeted user in realtime
            await _hubContext.Clients.User(user.Id.ToString())
                .SendAsync("SessionCreated", session);

            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        [HttpPost("sessions/with-admin")]
        public async Task<ActionResult<ChatSessionDto>> CreateSessionWithAdmin([FromBody] CreateSessionWithAdminRequest request)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user?.RoleName != RoleNames.Customer)
                return Forbid("Only customers can create chat sessions");

            // Verify the admin exists
            var admin = await _authService.GetUserByIdAsync(request.AdminId);
            if (admin?.RoleName != RoleNames.Admin)
                return BadRequest("Invalid admin selected");

            var session = await _chatService.CreateSessionWithAdminAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        [HttpGet("sessions/{id}")]
        public async Task<ActionResult<ChatSessionWithMessagesDto>> GetSession(Guid id)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var session = await _chatService.GetSessionAsync(id);
            if (session == null)
                return NotFound();

            // Check access permissions
            if (session.Customer.Id != userId && session.Admin?.Id != userId)
                return Forbid();

            return Ok(session);
        }

        [HttpGet("sessions/customer")]
        public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetCustomerSessions()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var sessions = await _chatService.GetCustomerSessionsAsync(userId.Value);
            return Ok(sessions);
        }

        [HttpGet("sessions/admin")]
        public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetAdminSessions()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user?.RoleName != RoleNames.Admin)
                return Forbid();

            var sessions = await _chatService.GetAdminSessionsAsync(userId.Value);
            return Ok(sessions);
        }

        [HttpGet("sessions/waiting")]
        public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetWaitingSessions()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user?.RoleName != RoleNames.Admin)
                return Forbid();

            var sessions = await _chatService.GetWaitingSessionsAsync();
            return Ok(sessions);
        }

        [HttpPost("sessions/{id}/assign-and-join")]
        public async Task<ActionResult<ChatSessionDto>> AssignAndJoinSession(Guid id)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user?.RoleName != RoleNames.Admin)
                return Forbid();

            var session = await _chatService.AssignAndJoinSessionAsync(id, userId.Value);
            if (session == null)
                return NotFound("Session not found or already assigned");

            return Ok(session);
        }

        [HttpPost("sessions/{id}/assign")]
        public async Task<ActionResult<ChatSessionDto>> AssignSession(Guid id, [FromBody] AssignSessionRequest request)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user?.RoleName != RoleNames.Admin)
                return Forbid();

            var session = await _chatService.AssignSessionAsync(id, request.AdminId);
            if (session == null)
                return NotFound();

            return Ok(session);
        }

        [HttpPost("sessions/{id}/close")]
        public async Task<ActionResult<ChatSessionDto>> CloseSession(Guid id)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var session = await _chatService.GetSessionAsync(id);
            if (session == null)
                return NotFound();

            // Check permissions - customer or assigned admin can close
            if (session.Customer.Id != userId && session.Admin?.Id != userId)
                return Forbid();

            var closedSession = await _chatService.CloseSessionAsync(id);
            return Ok(closedSession);
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var session = await _chatService.GetSessionAsync(sessionId);
            if (session == null)
                return NotFound();

            // Check permissions
            if (session.Customer.Id != userId && session.Admin?.Id != userId)
                return Forbid();

            var message = await _chatService.SendMessageAsync(sessionId, userId.Value, request);
            return CreatedAtAction(nameof(GetSessionMessages), new { sessionId }, message);
        }

        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetSessionMessages(Guid sessionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var session = await _chatService.GetSessionAsync(sessionId);
            if (session == null)
                return NotFound();

            // Check permissions
            if (session.Customer.Id != userId && session.Admin?.Id != userId)
                return Forbid();

            var messages = await _chatService.GetSessionMessagesAsync(sessionId, page, pageSize);
            return Ok(messages);
        }

        [HttpPut("messages/{messageId}/status")]
        public async Task<ActionResult> UpdateMessageStatus(Guid messageId, [FromBody] string status)
        {
            var success = await _chatService.UpdateMessageDeliveryStatusAsync(messageId, status);
            if (!success)
                return NotFound();

            return NoContent();
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
