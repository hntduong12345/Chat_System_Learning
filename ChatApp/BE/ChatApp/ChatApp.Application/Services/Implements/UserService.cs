using AutoMapper;
using ChatApp.Application.DTOs.AuthDTOs;
using ChatApp.Application.DTOs.UserDTOs;
using ChatApp.Application.Services.Interfaces;
using ChatApp.Application.Utils;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<ChatAppDbContext> unitOfWork, ILogger<UserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        #region User Services
        public async Task<UserDTO> GetUserInfo(string id)
        {
            Guid userId = Guid.Parse(id);
            UserDTO user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: u => u.Id == userId,
                selector: u => new UserDTO
                {
                    AvatarUrl = u.AvatarUrl,
                    CreatedAt = u.CreatedAt,
                    Email = u.Email,
                    IsOnline = u.IsOnline,  
                    LastActive = u.LastActive,
                    PhoneNumber = u.PhoneNumber,
                    UserName = u.UserName,
                });

            if (user == null)
                throw new BadHttpRequestException("User is not found");

            return user;
        }

        public async Task UpdateUser()
        {

        }
        #endregion

        #region Auth Services
        public async Task<AuthDTO> SignIn(SignInDTO request)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: u => u.PhoneNumber == request.PhoneNumber);

            if(user == null)
                throw new BadHttpRequestException("This phone number does not registered in the system");

            if (!HashUtil.VerifyPassword(request.Password, user.Password, out var rehashedPassword))
                throw new BadHttpRequestException("Incorrect password!");

            if(rehashedPassword != null)
            {
                user.Password = rehashedPassword;
                _unitOfWork.GetRepository<User>().UpdateAsync(user);
                bool isSuccess = await _unitOfWork.CommitAsync() > 0;

                if (!isSuccess) throw new Exception("An error occured when executing sign-in");
            }

            var token = JwtUtil.GenerateJwtToken(user);
            return new AuthDTO
            {
                Token = token,
                AvatarUrl = user.AvatarUrl,
                UserId = user.Id.ToString(),
                Username = user.UserName,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<AuthDTO> SignUp(SignUpDTO request)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
                predicate: u => u.PhoneNumber == request.PhoneNumber);

            if (user != null)
                throw new BadHttpRequestException("Phone number has already been used");

            var newUser = _mapper.Map<User>(user);

            await _unitOfWork.GetRepository<User>().InsertAsync(newUser);
            bool isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess) throw new Exception("An error occured when executing sign-up");

            var token = JwtUtil.GenerateJwtToken(newUser);
            return new AuthDTO
            {
                Token = token,
                AvatarUrl = newUser.AvatarUrl,
                UserId = newUser.Id.ToString(),
                Username = newUser.UserName,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
        }
        #endregion
    }
}
