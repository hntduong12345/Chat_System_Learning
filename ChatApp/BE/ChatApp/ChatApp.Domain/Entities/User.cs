namespace ChatApp.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
        public bool IsOnline { get; set; }
        public DateTimeOffset LastActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new HashSet<Message>();
        public virtual ICollection<UserChatRoom> UserChatRooms { get; set; } = new HashSet<UserChatRoom>();
    }
}
