using TestChatAPI.Enums;

namespace TestChatAPI.Payloads.ChatMessages
{
    public class GetChatMessageResponse
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }
    }
}
