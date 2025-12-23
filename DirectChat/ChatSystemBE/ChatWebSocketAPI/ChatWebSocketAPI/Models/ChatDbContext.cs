using Microsoft.EntityFrameworkCore;

namespace ChatWebSocketAPI.Models
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).HasMaxLength(11);
                entity.Property(e => e.CreateAt).HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");

                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Role)
                      .WithMany(e => e.Users)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ChatSession
            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreateAt).HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");
                entity.Property(e => e.LastActiveAt).HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");
                entity.Property(e => e.ConnectionState).HasMaxLength(30);
                entity.Property(e => e.ChannelType).HasMaxLength(20);

                entity.HasOne(e => e.Customer)
                      .WithMany(e => e.CustomerSessions)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Admin)
                      .WithMany(e => e.AdminSessions)
                      .HasForeignKey(e => e.AdminId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.AdminId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreateAt);
            });

            // Configure ChatMessage
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.SendAt).HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");
                entity.Property(e => e.DeliveryStatus).HasMaxLength(20);
                entity.Property(e => e.SourcePlatform).HasMaxLength(20);

                entity.HasOne(e => e.ChatSession)
                      .WithMany(e => e.Messages)
                      .HasForeignKey(e => e.ChatSessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sender)
                      .WithMany(e => e.Messages)
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ChatSessionId);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.SendAt);
            });

            // Seed data
            var customerRoleId = Guid.NewGuid();
            var adminRoleId = Guid.NewGuid();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = customerRoleId, Name = RoleNames.Customer },
                new Role { Id = adminRoleId, Name = RoleNames.Admin }
            );
        }
    }
}
