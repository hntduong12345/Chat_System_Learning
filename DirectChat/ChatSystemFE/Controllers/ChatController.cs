using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatWebSocketAPI.Services;
using ChatWebSocketAPI.DTOs;
using ChatWebSocketAPI.Models;

namespace ChatWebSocketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IAuthService _authService;

        public ChatController(IChatService chatService, IAuthService authService)
        {
            _chatService = chatService;
            _authService = authService;
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
            if (session.Customer.Id != userId.Value && session.Admin?.Id != userId.Value)
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
            if (session.Customer.Id != userId.Value && session.Admin?.Id != userId.Value)
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
            if (session.Customer.Id != userId.Value && session.Admin?.Id != userId.Value)
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
            if (session.Customer.Id != userId.Value && session.Admin?.Id != userId.Value)
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
