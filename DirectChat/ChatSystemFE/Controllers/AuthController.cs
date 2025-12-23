using Microsoft.AspNetCore.Mvc;
using ChatWebSocketAPI.Services;
using ChatWebSocketAPI.DTOs;

namespace ChatWebSocketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized("Invalid email or password");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);
            if (result == null)
                return BadRequest("User with this email already exists or invalid role");

            return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("admins")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAdmins()
        {
            var admins = await _authService.GetAdminUsersAsync();
            return Ok(admins);
        }
    }
}
