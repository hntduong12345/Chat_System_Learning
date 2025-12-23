using System.ComponentModel.DataAnnotations;

namespace ChatWebSocketAPI.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ChatSessionId { get; set; }

        public Guid SenderId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SendAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string DeliveryStatus { get; set; } = MessageDeliveryStatus.Sent;

        [MaxLength(20)]
        public string SourcePlatform { get; set; } = "Web";

        // Navigation properties
        public virtual ChatSession ChatSession { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
    }

    public static class MessageDeliveryStatus
    {
        public const string Sent = "Sent";
        public const string Delivered = "Delivered";
        public const string Read = "Read";
        public const string Failed = "Failed";
    }
}
