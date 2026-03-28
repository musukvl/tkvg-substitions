using Microsoft.EntityFrameworkCore;

namespace TkvgSubstitutionBot.Data;

public class AppDbContext : DbContext
{
    public DbSet<SubscriptionEntity> Subscriptions { get; set; } = null!;
    public DbSet<SubscriptionReceiveLogEntity> SubscriptionReceiveLogs { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionEntity>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.ClassName).HasColumnName("class_name").HasMaxLength(50);
            entity.Property(e => e.LastMessage).HasColumnName("last_message").HasDefaultValue("");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.ChatId);
            entity.HasIndex(e => e.ClassName);
            entity.HasIndex(e => new { e.ChatId, e.ClassName }).IsUnique();
        });

        modelBuilder.Entity<SubscriptionReceiveLogEntity>(entity =>
        {
            entity.ToTable("subscription_receive_log");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ClassName).HasColumnName("class_name").HasMaxLength(50);
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.MessageText).HasColumnName("message_text");
            entity.HasIndex(e => e.ReceivedAt);
            entity.HasIndex(e => e.ChatId);
            entity.HasIndex(e => e.ClassName);
        });
    }
}
