using ChatApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class ChatController : BaseController<ChatController>
    {
        private readonly IChatService _chatService;
        public ChatController(ILogger<ChatController> logger, IChatService chatService) : base(logger)
        {
            _chatService = chatService;
        }

        #region Chat APIs
        [HttpGet()]
        public async Task<IActionResult> GetSession()
        {
            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> GetUserSessions()
        {
            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> GetWaitingSessions()
        {
            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> CreateSession()
        {
            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> AssignAndJoinSession()
        {
            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> AssignSession()
        {
            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> CloseSession()
        {
            return Ok();
        }
        #endregion

        #region Message APIs
        [HttpGet()]
        public async Task<IActionResult> GetSessionMessages()
        {
            return Ok();
        }

        [HttpPost()]
        public async Task<IActionResult> SendMessage()
        {
            return Ok();
        }

        [HttpPatch()]
        public async Task<IActionResult> UpdateMessageStatus()
        {
            return Ok();
        }
        #endregion
    }
}
