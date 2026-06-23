using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Models;

namespace MonetizacionEcosistemaCreador.Data;

/// <summary>Contexto EF Core del bounded context de Monetización del Ecosistema de Creadores.</summary>
public class MonetizacionEcosistemaCreadorDbContext : DbContext
{
    public MonetizacionEcosistemaCreadorDbContext(DbContextOptions<MonetizacionEcosistemaCreadorDbContext> options)
        : base(options)
    {
    }

    public DbSet<CreatorEarnings> CreatorEarnings => Set<CreatorEarnings>();
    public DbSet<MonetizationEligibility> Eligibilities => Set<MonetizationEligibility>();
    public DbSet<MonetizationApplication> MonetizationApplications => Set<MonetizationApplication>();
    public DbSet<MonetizationStatus> MonetizationStatuses => Set<MonetizationStatus>();
    public DbSet<MonetizationProduct> Products => Set<MonetizationProduct>();
    public DbSet<ProductPurchase> ProductPurchases => Set<ProductPurchase>();
    public DbSet<RevenueEntry> RevenueEntries => Set<RevenueEntry>();
    public DbSet<FiscalDocument> FiscalDocuments => Set<FiscalDocument>();
    public DbSet<Payout> Payouts => Set<Payout>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CreatorEarnings>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId).IsUnique();
            e.Property(x => x.Balance).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).HasMaxLength(8);
        });

        modelBuilder.Entity<MonetizationEligibility>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<MonetizationProduct>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<RevenueEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId);
            e.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PlatformShare).HasColumnType("decimal(18,2)");
            e.Property(x => x.CreatorShare).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Payout>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.MinimumPayoutAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<MonetizationApplication>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<MonetizationStatus>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ResourceId, x.ResourceType }).IsUnique();
            e.Property(x => x.ResourceType).HasConversion<string>().HasMaxLength(16);
        });

        modelBuilder.Entity<ProductPurchase>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).HasMaxLength(8);
        });

        modelBuilder.Entity<FiscalDocument>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ChannelId);
            e.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.DocumentNumber).HasMaxLength(64);
            e.Property(x => x.Country).HasMaxLength(2);
        });
    }
}
