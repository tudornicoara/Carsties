using System.ComponentModel.DataAnnotations.Schema;
using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

[Table("Items")]
public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Auction> Auctions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}