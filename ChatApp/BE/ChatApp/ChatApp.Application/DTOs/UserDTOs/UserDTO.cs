using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs.UserDTOs
{
    public class UserDTO
    {
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = null!;
        //public string Password { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
        public bool IsOnline { get; set; }
        public DateTimeOffset LastActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
