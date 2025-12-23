using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Entities
{
    public class ChatAppDbContext : DbContext
    {
        public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options) : base(options)
        {
        }

        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserChatRoom> UsersChatRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.ToTable("ChatRoom");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(x => x.Name).IsRequired();
                entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(x => x.Content).HasColumnType("text");
                entity.Property(x => x.Timestamp).IsRequired().HasColumnType("timestamptz");
                entity.Property(x => x.MessageType).IsRequired().HasMaxLength(10);
                entity.Property(x => x.AttachmentUrl).HasColumnType("text");

                entity.HasIndex(x => x.SenderId, "IX_ChatRoom_SenderId").IsUnique();
                entity.HasIndex(x => x.ChatRoomId, "IX_ChatRoom_ChatRoomId").IsUnique();

                entity.HasOne(x => x.User)
                      .WithMany(x => x.Messages)
                      .HasForeignKey(x => x.SenderId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .HasConstraintName("FK_Message_User");

                entity.HasOne(x => x.ChatRoom)
                      .WithMany(x => x.Messages)
                      .HasForeignKey(x => x.ChatRoomId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Message_ChatRoom");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(100);
                entity.Property(x => x.PhoneNumber).HasMaxLength(12).IsRequired();
                entity.Property(x => x.Password).IsRequired();
                entity.Property(x => x.AvatarUrl).IsRequired();
                entity.Property(x => x.LastActive).IsRequired().HasColumnType("timestamptz");
                entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz");

                entity.HasIndex(x => x.PhoneNumber, "IX_User_PhoneNumber").IsUnique();
            });

            modelBuilder.Entity<UserChatRoom>(entity =>
            {
                entity.ToTable("UserChatRoom");

                entity.HasKey(x => new { x.UserId, x.ChatRoomId});

                entity.Property(x => x.JoinedAt).IsRequired().HasColumnType("timestamptz");
                entity.Property(x => x.LastActivityAt).IsRequired().HasColumnType("timestamptz");

                entity.HasIndex(x => x.LastReadMessageId, "IX_UserChatRoom_LastReadMessageId").IsUnique();

                entity.HasOne(x => x.User)
                      .WithMany(x => x.UserChatRooms)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .HasConstraintName("FK_UserChatRoom_User");

                entity.HasOne(x => x.ChatRoom)
                      .WithMany(x => x.UserChatRooms)
                      .HasForeignKey(x => x.ChatRoomId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_UserChatRoom_ChatRoom");
            });
        }
    }
}
