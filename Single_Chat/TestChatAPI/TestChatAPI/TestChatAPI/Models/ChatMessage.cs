using System.ComponentModel.DataAnnotations;
using TestChatAPI.Enums;

namespace TestChatAPI.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int ChatSessionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public MessageType Type { get; set; } = MessageType.User;

        // Navigation property
        public virtual ChatSession ChatSession { get; set; } = null!;
    }
}
