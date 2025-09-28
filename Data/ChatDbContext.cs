using Microsoft.EntityFrameworkCore;
using ChatApi.Models;

namespace ChatApi.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<ChatConversation> Conversations { get; set; }
    public DbSet<ChatMessage> Messages { get; set; }
    public DbSet<KeyPoint> KeyPoints { get; set; }
    public DbSet<ConversationSummary> Summaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<KeyPoint>()
            .HasOne(kp => kp.Conversation)
            .WithMany(c => c.KeyPoints)
            .HasForeignKey(kp => kp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationSummary>()
            .HasOne(s => s.Conversation)
            .WithOne()
            .HasForeignKey<ConversationSummary>(s => s.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for better query performance
        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.ConversationId);
        
        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.Timestamp);

        modelBuilder.Entity<KeyPoint>()
            .HasIndex(kp => kp.ConversationId);
        
        modelBuilder.Entity<KeyPoint>()
            .HasIndex(kp => kp.Priority);

        modelBuilder.Entity<ChatConversation>()
            .HasIndex(c => c.CreatedAt);
    }
}