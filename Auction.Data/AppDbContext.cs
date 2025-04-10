using Auction.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auction.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<Account>(options)
{ 
    public DbSet<AuctionLot> AuctionLots { get; set; }
    public DbSet<AuctionHistory> AuctionHistories { get; set; }

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

            entity.HasOne(a => a.Account)
                .WithMany(b => b.HostedLots)
                .HasForeignKey(a => a.OwnerAccount)
                .IsRequired();

            entity.HasIndex(e => e.StartTime);
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
                .HasForeignKey(a => a.AuctionLot)
                .IsRequired();
        });
    }
}