using ChatApp.Application.DTOs.AuthDTOs;
using ChatApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    public class AuthenticationController : BaseController<AuthenticationController>
    {
        private readonly IUserService _userService;
        public AuthenticationController(ILogger<AuthenticationController> logger, IUserService userService) : base(logger)
        {
            _userService = userService;
        }

        [HttpPost()]
        public async Task<IActionResult> SignUp([FromBody]SignUpDTO request)
        {
            var response = await _userService.SignUp(request);
            return Ok(response);
        }

        [HttpPost()]
        public async Task<IActionResult> SignIn([FromBody]SignInDTO request)
        {
            var response = await _userService.SignIn(request);
            return Ok(response);
        }
    }
}
