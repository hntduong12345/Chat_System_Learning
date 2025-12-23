using ChatApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    public class UserController : BaseController<UserController>
    {
        private readonly IUserService _userService;
        public UserController(ILogger<UserController> logger, IUserService userService) : base(logger)
        {
            _userService = userService;
        }

        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetUserInfo([FromRoute]string id)
        {
            var response = await _userService.GetUserInfo(id);
            return Ok(response);
        }

        //[HttpPost()]
        //[Authorize]
        //public async Task<IActionResult> ResetPassword()
        //{
        //    return Ok();
        //}
    }
}
