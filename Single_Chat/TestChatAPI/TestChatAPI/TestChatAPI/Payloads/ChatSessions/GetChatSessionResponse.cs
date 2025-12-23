using System.ComponentModel.DataAnnotations;

namespace TestChatAPI.Payloads.ChatSessions
{
    public class GetChatSessionResponse
    {
        public int Id { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsActive { get; set; }
        public string? ConnectionId { get; set; }
        public int MessageCount { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
