using System.ComponentModel.DataAnnotations;

namespace ChatWebSocketAPI.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RoleId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(11)]
        public string? PhoneNumber { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<ChatSession> CustomerSessions { get; set; } = new List<ChatSession>();
        public virtual ICollection<ChatSession> AdminSessions { get; set; } = new List<ChatSession>();
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
