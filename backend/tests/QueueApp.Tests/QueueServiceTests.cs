using Microsoft.EntityFrameworkCore;
using QueueApp.Api.Data;
using QueueApp.Api.Models;
using QueueApp.Api.Services;
using Xunit;

namespace QueueApp.Tests;

public class QueueServiceTests
{
    private QueueDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<QueueDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new QueueDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GenerateQueue_FirstTime_ReturnsA0()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new QueueService(context);

        // Act
        var response = await service.GenerateNextQueueAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal("A0", response.QueueCode);
        Assert.Equal(0, response.CurrentIndex);

        var savedSetting = await context.QueueSettings.FirstOrDefaultAsync(s => s.Id == 1);
        Assert.NotNull(savedSetting);
        Assert.Equal(0, savedSetting.CurrentIndex);
    }

    [Fact]
    public async Task GenerateQueue_AtZ9_WrapsAroundToA0()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        // Seed queue at index 259 ("Z9")
        context.QueueSettings.Add(new QueueSetting
        {
            Id = 1,
            CurrentIndex = 259,
            LastActive = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new QueueService(context);

        // Act
        var response = await service.GenerateNextQueueAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal("A0", response.QueueCode);
        Assert.Equal(0, response.CurrentIndex);

        var savedSetting = await context.QueueSettings.FirstOrDefaultAsync(s => s.Id == 1);
        Assert.NotNull(savedSetting);
        Assert.Equal(0, savedSetting.CurrentIndex);
    }

    [Fact]
    public async Task ResetQueue_SetsIndexToMinusOne()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        // Seed queue at index 50 ("F0")
        context.QueueSettings.Add(new QueueSetting
        {
            Id = 1,
            CurrentIndex = 50,
            LastActive = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new QueueService(context);

        // Act
        var resetResponse = await service.ResetQueueAsync();

        // Assert
        Assert.NotNull(resetResponse);
        Assert.Equal(-1, resetResponse.CurrentIndex);

        var savedSetting = await context.QueueSettings.FirstOrDefaultAsync(s => s.Id == 1);
        Assert.NotNull(savedSetting);
        Assert.Equal(-1, savedSetting.CurrentIndex);
    }

    [Fact]
    public async Task GetCurrentQueue_WhenReset_Returns00()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new QueueService(context);

        // Act
        var currentResponse = await service.GetCurrentQueueAsync();

        // Assert
        Assert.NotNull(currentResponse);
        Assert.Equal("00", currentResponse.QueueCode);
        Assert.Equal(-1, currentResponse.CurrentIndex);
    }

    [Fact]
    public async Task GetCurrentQueue_WithActiveIndex_ReturnsFormattedCode()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.QueueSettings.Add(new QueueSetting
        {
            Id = 1,
            CurrentIndex = 5, // A5
            LastActive = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new QueueService(context);

        // Act
        var currentResponse = await service.GetCurrentQueueAsync();

        // Assert
        Assert.NotNull(currentResponse);
        Assert.Equal("A5", currentResponse.QueueCode);
        Assert.Equal(5, currentResponse.CurrentIndex);
    }
}

