using ChatApp.Application.DTOs.AuthDTOs;
using ChatApp.Application.DTOs.UserDTOs;

namespace ChatApp.Application.Services.Interfaces
{
    public interface IUserService
    {
        #region User Services
        Task<UserDTO> GetUserInfo(string id);
        #endregion

        #region Auth Services
        Task<AuthDTO> SignIn(SignInDTO request);
        Task<AuthDTO> SignUp(SignUpDTO request);
        #endregion
    }
}
