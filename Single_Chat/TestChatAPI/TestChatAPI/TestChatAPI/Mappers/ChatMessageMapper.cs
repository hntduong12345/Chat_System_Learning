using AutoMapper;
using TestChatAPI.Models;
using TestChatAPI.Payloads.ChatMessages;

namespace TestChatAPI.Mappers
{
    public class ChatMessageMapper : Profile
    {
        public ChatMessageMapper()
        {
            CreateMap<ChatMessage, GetChatMessageResponse>();

            CreateMap<CreateMessageRequest, ChatMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ChatSession, opt => opt.Ignore());
        }
    }
}
