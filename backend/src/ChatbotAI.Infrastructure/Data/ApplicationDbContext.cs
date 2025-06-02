using ChatbotAI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageRating> MessageRatings { get; set; }
        public DbSet<Character> Characters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Session configuration
            modelBuilder.Entity<Session>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Session>()
                .Property(s => s.CreatedAt)
                .IsRequired();

            modelBuilder.Entity<Session>()
                .Property(s => s.LastActivity)
                .IsRequired();
                
            modelBuilder.Entity<Session>()
                .Property(s => s.MetadataJson)
                .HasColumnName("Metadata");

            modelBuilder.Entity<Session>()
                .HasMany(s => s.Messages)
                .WithOne(m => m.Session)
                .HasForeignKey(m => m.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Message configuration
            modelBuilder.Entity<Message>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Message>()
                .Property(m => m.Content)
                .IsRequired();

            modelBuilder.Entity<Message>()
                .Property(m => m.CreatedAt)
                .IsRequired();

            // Add Character relationship to Message
            modelBuilder.Entity<Message>()
                .Property(m => m.CharacterId)
                .IsRequired();

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Character)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            // MessageRating configuration
            modelBuilder.Entity<MessageRating>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<MessageRating>()
                .HasOne(r => r.Message)
                .WithOne(m => m.Rating)
                .HasForeignKey<MessageRating>(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Character configuration and seeding
            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Key).IsRequired();
                entity.HasMany(c => c.Messages)
                      .WithOne(m => m.Character)
                      .HasForeignKey(m => m.CharacterId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasData(
                    new Character { Id = 1, Key = "rick" },
                    new Character { Id = 2, Key = "yoda" },
                    new Character { Id = 3, Key = "sherlock" },
                    new Character { Id = 4, Key = "socrates" }
                );
            });
        }
    }
} 