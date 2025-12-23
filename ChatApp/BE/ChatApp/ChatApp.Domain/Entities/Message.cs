namespace ChatApp.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ChatRoomId { get; set; }
        public string? Content { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool IsRead { get; set; }
        public string MessageType { get; set; } = null!;
        public string? AttachmentUrl { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ChatRoom ChatRoom { get; set; } = null!;
    }
}
