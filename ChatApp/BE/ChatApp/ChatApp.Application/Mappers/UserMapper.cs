using AutoMapper;
using ChatApp.Application.DTOs.AuthDTOs;
using ChatApp.Application.DTOs.UserDTOs;
using ChatApp.Application.Utils;
using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<SignUpDTO, User>()
                .ForMember(des => des.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(des => des.IsOnline, opt => opt.MapFrom(src => false))
                .ForMember(des => des.Password, opt => opt.MapFrom(src => HashUtil.PasswordHash(src.Password)))
                .ForMember(des => des.LastActive, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
                .ForMember(des => des.CreatedAt, opt => opt.MapFrom(src => DateTimeOffset.UtcNow));

            CreateMap<User, UserDTO>();
        }
    }
}
