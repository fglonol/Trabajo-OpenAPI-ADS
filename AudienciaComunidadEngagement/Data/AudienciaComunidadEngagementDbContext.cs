using AudienciaComunidadEngagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Data;

/// <summary>
/// DbContext del bounded context AudienciaComunidadEngagement.
/// Persiste suscripciones, reacciones, comentarios, notificaciones,
/// historial/listas de visualización y publicaciones de comunidad.
/// </summary>
public class AudienciaComunidadEngagementDbContext : DbContext
{
    public AudienciaComunidadEngagementDbContext(DbContextOptions<AudienciaComunidadEngagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<WatchHistoryEntry> WatchHistoryEntries => Set<WatchHistoryEntry>();
    public DbSet<WatchLaterItem> WatchLaterItems => Set<WatchLaterItem>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RF-A1: una suscripción única por (UserId, ChannelId).
        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.ChannelId }).IsUnique();
            e.HasIndex(x => x.ChannelId);
        });

        // RF-A2: una reacción única por (UserId, CatalogItemId); enum como string.
        modelBuilder.Entity<Reaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
            e.HasIndex(x => new { x.UserId, x.CatalogItemId }).IsUnique();
            e.HasIndex(x => x.CatalogItemId);
        });

        // RF-A3: comentarios.
        modelBuilder.Entity<Comment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired();
            e.HasIndex(x => x.CatalogItemId);
            e.HasIndex(x => x.ParentCommentId);
        });

        // RF-A4: notificaciones.
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).IsRequired().HasMaxLength(64);
            e.HasIndex(x => x.UserId);
        });

        // RF-A5: historial de visualización.
        modelBuilder.Entity<WatchHistoryEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.CatalogItemId });
        });

        // RF-A5: lista "ver más tarde".
        modelBuilder.Entity<WatchLaterItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.CatalogItemId }).IsUnique();
        });

        // RF-A6: publicaciones de comunidad.
        modelBuilder.Entity<CommunityPost>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired();
            e.HasIndex(x => x.ChannelId);
        });
    }
}
