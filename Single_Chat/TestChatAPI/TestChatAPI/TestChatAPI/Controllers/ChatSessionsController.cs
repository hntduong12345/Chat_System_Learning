using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestChatAPI.Models;
using TestChatAPI.Payloads.ChatSessions;
using TestChatAPI.Services;

namespace TestChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatSessionsController : ControllerBase
    {
        private readonly IChatSessionService _chatSessionService;

        public ChatSessionsController(IChatSessionService chatSessionService)
        {
            _chatSessionService = chatSessionService;
        }

        [HttpGet("{sessionId}")]
        public async Task<ActionResult<GetChatSessionWithMessageResponse>> GetSession(int sessionId)
        {
            var session = await _chatSessionService.GetSessionAsync(sessionId);

            if (session == null)
                return NotFound(new { message = "Session not found" });

            return Ok(session);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GetChatSessionResponse>>> GetUserSessions(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "UserId is required" });

            var sessions = await _chatSessionService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }

        [HttpPost]
        public async Task<ActionResult<GetChatSessionResponse>> CreateSession([FromBody] CreateSessionRequest request)
        {
            if (string.IsNullOrEmpty(request.SessionName) || string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { message = "SessionName and UserId are required" });

            var session = await _chatSessionService.CreateSessionAsync(request.SessionName, request.UserId);

            return CreatedAtAction(nameof(GetSession), new { sessionId = session.Id }, session);
        }

        [HttpPut("{sessionId}/close")]
        public async Task<ActionResult<GetChatSessionResponse>> CloseSession(int sessionId, [FromBody] SessionActionRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { message = "UserId is required" });

            var session = await _chatSessionService.CloseSessionAsync(sessionId, request.UserId);

            if (session == null)
                return NotFound(new { message = "Session not found or you don't have permission to close it" });

            return Ok(session);
        }

        [HttpPut("{sessionId}/reactivate")]
        public async Task<ActionResult<GetChatSessionResponse>> ReactivateSession(int sessionId, [FromBody] SessionActionRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { message = "UserId is required" });

            var session = await _chatSessionService.ReactivateSessionAsync(sessionId, request.UserId);

            if (session == null)
                return NotFound(new { message = "Session not found or you don't have permission to reactivate it" });

            return Ok(session);
        }

        [HttpGet("{sessionId}/messages")]
        public async Task<ActionResult<IEnumerable<GetChatSessionResponse>>> GetSessionMessages(
            int sessionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var messages = await _chatSessionService.GetSessionMessagesAsync(sessionId, page, pageSize);
            return Ok(messages);
        }
    }
}
