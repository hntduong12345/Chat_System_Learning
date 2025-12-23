namespace ChatWebSocketAPI.DTOs
{
    public class ChatDto
    {
        public class ChatSessionDto
        {
            public Guid Id { get; set; }
            public UserDto Customer { get; set; } = null!;
            public UserDto? Admin { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime CreateAt { get; set; }
            public DateTime? CloseAt { get; set; }
            public DateTime LastActiveAt { get; set; }
            public string? ConnectionState { get; set; }
            public int InactivityTimeout { get; set; }
            public string ChannelType { get; set; } = string.Empty;
            public int UnreadCount { get; set; }
            public ChatMessageDto? LastMessage { get; set; }
        }

        public class ChatSessionWithMessagesDto : ChatSessionDto
        {
            public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
        }

        public class ChatMessageDto
        {
            public Guid Id { get; set; }
            public Guid ChatSessionId { get; set; }
            public UserDto Sender { get; set; } = null!;
            public string Content { get; set; } = string.Empty;
            public DateTime SendAt { get; set; }
            public string DeliveryStatus { get; set; } = string.Empty;
            public string SourcePlatform { get; set; } = string.Empty;
        }

        public class CreateSessionRequest
        {
            public string InitialMessage { get; set; } = string.Empty;
            public string ChannelType { get; set; } = "Web";
        }

        public class CreateSessionWithAdminRequest
        {
            public Guid AdminId { get; set; }
            public string InitialMessage { get; set; } = string.Empty;
            public string ChannelType { get; set; } = "Web";
        }

        public class SendMessageRequest
        {
            public string Content { get; set; } = string.Empty;
            public string SourcePlatform { get; set; } = "Web";
        }

        public class AssignSessionRequest
        {
            public Guid AdminId { get; set; }
        }
    }
}
