using System.ComponentModel.DataAnnotations;

namespace TestChatAPI.Models
{
    public class ChatSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SessionName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ConnectionId { get; set; }

        // Navigation property
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
