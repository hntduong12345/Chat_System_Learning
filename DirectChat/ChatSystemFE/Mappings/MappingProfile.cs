using AutoMapper;
using ChatWebSocketAPI.Models;
using ChatWebSocketAPI.DTOs;

namespace ChatWebSocketAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

            // ChatSession mappings
            CreateMap<ChatSession, ChatSessionDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Admin, opt => opt.MapFrom(src => src.Admin))
                .ForMember(dest => dest.UnreadCount, opt => opt.MapFrom(src => 
                    src.Messages.Count(m => m.DeliveryStatus != MessageDeliveryStatus.Read)))
                .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => 
                    src.Messages.OrderByDescending(m => m.SendAt).FirstOrDefault()));

            CreateMap<ChatSession, ChatSessionWithMessagesDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Admin, opt => opt.MapFrom(src => src.Admin))
                .ForMember(dest => dest.UnreadCount, opt => opt.MapFrom(src => 
                    src.Messages.Count(m => m.DeliveryStatus != MessageDeliveryStatus.Read)))
                .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => 
                    src.Messages.OrderByDescending(m => m.SendAt).FirstOrDefault()))
                .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages.OrderBy(m => m.SendAt)));

            // ChatMessage mappings
            CreateMap<ChatMessage, ChatMessageDto>()
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender));
        }
    }
}
