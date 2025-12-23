using AutoMapper;
using TestChatAPI.Models;
using TestChatAPI.Payloads.ChatSessions;

namespace TestChatAPI.Mappers
{
    public class ChatSessionMapper : Profile
    {
        public ChatSessionMapper()
        {
            CreateMap<ChatSession, GetChatSessionResponse>()
                .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Messages.Count))
                .ForMember(dest => dest.LastMessageAt, opt => opt.MapFrom(src =>
                    src.Messages.Any() ? src.Messages.Max(m => m.Timestamp) : (DateTime?)null));

            CreateMap<ChatSession, GetChatSessionWithMessageResponse>()
                .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Messages.Count))
                .ForMember(dest => dest.LastMessageAt, opt => opt.MapFrom(src =>
                    src.Messages.Any() ? src.Messages.Max(m => m.Timestamp) : (DateTime?)null))
                .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages.OrderBy(m => m.Timestamp)));

            CreateMap<CreateSessionRequest, ChatSession>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Messages, opt => opt.Ignore())
                .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionId, opt => opt.Ignore());
        }
    }
}
