using TestChatAPI.Payloads.ChatMessages;

namespace TestChatAPI.Payloads.ChatSessions
{
    public class GetChatSessionWithMessageResponse : GetChatSessionResponse
    {
        public List<GetChatMessageResponse> Messages { get; set; } = new List<GetChatMessageResponse>();
    }
}
