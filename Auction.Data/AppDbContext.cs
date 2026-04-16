using Auction.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auction.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<Account>(options)
{ 
    public DbSet<AuctionLot> AuctionLots { get; set; }
    public DbSet<AuctionHistory> AuctionHistories { get; set; }
    public DbSet<AuctionLotImage> AuctionLotImages { get; set; }
    public DbSet<AuctionCategory> AuctionCategories { get; set; }
    public DbSet<CategoryRequest> CategoryRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AuctionLot>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(256);

            entity.Property(e => e.StartPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .IsRequired();

            entity.HasOne(a => a.OwnerAccount)
                .WithMany(b => b.HostedLots)
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Category)
                .WithMany(c => c.Lots)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.WinnerAccount)
                .WithMany(a => a.WinningLots)
                .HasForeignKey(e => e.WinnerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Image)
                .WithOne(i => i.Lot)
                .HasForeignKey<AuctionLotImage>(i => i.LotId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.StartTime);
        });

        builder.Entity<AuctionLotImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContentType)
                .HasMaxLength(128)
                .IsRequired();
            entity.Property(e => e.FileName)
                .HasMaxLength(260)
                .IsRequired();
            entity.Property(e => e.Data)
                .IsRequired();
            entity.HasIndex(e => e.LotId)
                .IsUnique();
        });

        builder.Entity<AuctionCategory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(80)
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        builder.Entity<CategoryRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(80)
                .IsRequired();

            entity.HasOne(e => e.RequestedBy)
                .WithMany(a => a.RequestedCategoryRequests)
                .HasForeignKey(e => e.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewedBy)
                .WithMany(a => a.ReviewedCategoryRequests)
                .HasForeignKey(e => e.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        builder.Entity<AuctionHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.BidAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            entity.Property(e => e.BidTime)
                .HasColumnType("datetime")
                .IsRequired();

            entity.HasOne(a => a.AuctionLot)
                .WithMany(b => b.AuctionHistories)
                .HasForeignKey(a => a.LotId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
