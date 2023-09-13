using AuctionService.Entities;
using FluentAssertions;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    public void HasReservedPrice_ReservePriceGtZero_True()
    {
        // Arrange
        var auction = new Auction {Id = Guid.NewGuid(), ReservePrice = 10};
        
        // Act
        var result = auction.HasReservePrice();
        
        // Assert
        result.Should().Be(true);
    }
    
    [Fact]
    public void HasReservedPrice_ReservePriceIsZero_False()
    {
        // Arrange
        var auction = new Auction {Id = Guid.NewGuid(), ReservePrice = 0};
        
        // Act
        var result = auction.HasReservePrice();
        
        // Assert
        result.Should().Be(false);
    }
}