using System.ComponentModel.DataAnnotations;

namespace ChatWebSocketAPI.Models
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }

    public static class RoleNames
    {
        public const string Customer = "Customer";
        public const string Admin = "Admin";
    }
}
