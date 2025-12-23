namespace ChatApp.Domain.Entities
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsGroup { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new HashSet<Message>();
        public virtual ICollection<UserChatRoom> UserChatRooms { get; set; } = new HashSet<UserChatRoom>();
    }
}
