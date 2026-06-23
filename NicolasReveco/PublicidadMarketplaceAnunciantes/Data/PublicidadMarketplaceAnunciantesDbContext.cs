using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Models;

namespace PublicidadMarketplaceAnunciantes.Data;

/// <summary>Contexto EF Core del bounded context PublicidadMarketplaceAnunciantes.</summary>
public class PublicidadMarketplaceAnunciantesDbContext : DbContext
{
    public PublicidadMarketplaceAnunciantesDbContext(
        DbContextOptions<PublicidadMarketplaceAnunciantesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Advertiser> Advertisers => Set<Advertiser>();
    public DbSet<AdCampaign> AdCampaigns => Set<AdCampaign>();
    public DbSet<Creative> Creatives => Set<Creative>();
    public DbSet<Targeting> Targetings => Set<Targeting>();
    public DbSet<AdSlot> AdSlots => Set<AdSlot>();
    public DbSet<AdEvent> AdEvents => Set<AdEvent>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Advertiser>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(256);
            entity.Property(a => a.Email).IsRequired().HasMaxLength(256);
            entity.Property(a => a.BillingName).HasMaxLength(256);
            entity.Property(a => a.BillingAddress).HasMaxLength(512);
            entity.Property(a => a.TaxId).HasMaxLength(64);
            entity.Property(a => a.PaymentMethodToken).HasMaxLength(256);
            entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<AdCampaign>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(256);
            entity.Property(c => c.Budget).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Spend).HasColumnType("decimal(18,2)");

            // Enum persistido como string en la BD.
            entity.Property(c => c.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);

            // Relación uno-a-muchos con borrado en cascada (RF-F3).
            entity.HasMany(c => c.Creatives)
                .WithOne()
                .HasForeignKey(cr => cr.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación uno-a-uno con borrado en cascada (RF-F4).
            entity.HasOne(c => c.Targeting)
                .WithOne()
                .HasForeignKey<Targeting>(t => t.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Creative>(entity =>
        {
            entity.HasKey(cr => cr.Id);
            entity.Property(cr => cr.Type)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(cr => cr.AssetUrl).HasMaxLength(1024);
            entity.Property(cr => cr.Title).HasMaxLength(256);
            entity.Property(cr => cr.PolicyReviewStatus)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(cr => cr.PolicyReviewNotes).HasMaxLength(1024);
        });

        modelBuilder.Entity<Targeting>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Countries).HasMaxLength(1024);
            entity.Property(t => t.Interests).HasMaxLength(1024);
            entity.Property(t => t.Keywords).HasMaxLength(1024);
            entity.Property(t => t.ContentCategories).HasMaxLength(1024);
            entity.Property(t => t.ExcludedCatalogItemIds).HasMaxLength(4096);
        });

        modelBuilder.Entity<AdSlot>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Placement)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(s => s.BrandSafetyLevel)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);
        });

        modelBuilder.Entity<AdEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.CostPerEvent).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.TotalSpend).HasColumnType("decimal(18,2)");
            entity.Property(i => i.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);
        });
    }
}
