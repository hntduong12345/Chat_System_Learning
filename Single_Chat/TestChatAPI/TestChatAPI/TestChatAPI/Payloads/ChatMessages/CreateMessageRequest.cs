using TestChatAPI.Enums;

namespace TestChatAPI.Payloads.ChatMessages
{
    public class CreateMessageRequest
    {
        public int ChatSessionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.User;
    }
}
