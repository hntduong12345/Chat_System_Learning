namespace ChatApp.Domain.Entities
{
    public class UserChatRoom
    {
        public Guid UserId { get; set; }
        public Guid ChatRoomId { get; set; }
        public DateTimeOffset JoinedAt { get; set; }
        public bool IsMuted { get; set; }
        public bool IsPinned { get; set; }
        public Guid LastReadMessageId { get; set; }
        public int UnreadCount { get; set; }
        public DateTimeOffset LastActivityAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ChatRoom ChatRoom { get; set; } = null!;
    }
}
