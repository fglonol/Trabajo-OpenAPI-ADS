using DescubrimientoPersonalizacion.Models;
using Microsoft.EntityFrameworkCore;

namespace DescubrimientoPersonalizacion.Data;

/// <summary>
/// Contexto EF Core del bounded context Descubrimiento y Personalización. Persiste solo
/// referencias y puntajes (no la metadata editorial completa de Catálogo).
/// </summary>
public class DescubrimientoPersonalizacionDbContext : DbContext
{
    public DescubrimientoPersonalizacionDbContext(DbContextOptions<DescubrimientoPersonalizacionDbContext> options)
        : base(options)
    {
    }

    public DbSet<IndexedContent> IndexedContents => Set<IndexedContent>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<Signal> Signals => Set<Signal>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IndexedContent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(512);
            entity.HasIndex(e => e.ChannelId);
            entity.HasIndex(e => e.RankingScore);
            entity.HasIndex(e => e.TrendingScore);

            entity.HasMany(e => e.Tags)
                  .WithOne(t => t.IndexedContent)
                  .HasForeignKey(t => t.IndexedContentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContentTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(128);
            entity.HasIndex(e => e.Value);
        });

        modelBuilder.Entity<Signal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type)
                  .HasConversion<string>()
                  .HasMaxLength(32)
                  .IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ContentId);
        });

        modelBuilder.Entity<UserInterest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Interest).IsRequired().HasMaxLength(128);
            entity.HasIndex(e => new { e.UserId, e.Interest }).IsUnique();
        });
    }
}
