using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionBusTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly ITestHarness _testHarness;

    public AuctionBusTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _testHarness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        // Arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
        
        // Assert
        response.EnsureSuccessStatusCode();
        (await _testHarness.Published.Any<AuctionCreated>()).Should().Be(true);
    }
    
    public Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(db);
        return Task.CompletedTask;
    }
    
    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "testUrl",
            Color = "test",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }
}