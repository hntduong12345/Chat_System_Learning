using System.ComponentModel.DataAnnotations;

namespace ChatWebSocketAPI.Models
{
    public class ChatSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CustomerId { get; set; }

        public Guid? AdminId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = SessionStatus.Waiting;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? CloseAt { get; set; }

        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

        [MaxLength(30)]
        public string? ConnectionState { get; set; }

        public int InactivityTimeout { get; set; } = 1800; // 30 minutes default

        [MaxLength(20)]
        public string ChannelType { get; set; } = "Web";

        // Navigation properties
        public virtual User Customer { get; set; } = null!;
        public virtual User? Admin { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public static class SessionStatus
    {
        public const string Waiting = "Waiting";
        public const string Active = "Active";
        public const string Closed = "Closed";
        public const string Transferred = "Transferred";
    }

    public static class ConnectionState
    {
        public const string Connected = "Connected";
        public const string Disconnected = "Disconnected";
        public const string Idle = "Idle";
    }
}
