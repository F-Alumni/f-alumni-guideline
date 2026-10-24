using Microsoft.Extensions.Logging.Abstractions;
using Template.Repositories.Implementations;
using Template.Repositories.Interfaces;
using Template.Services.DTOs.Requests;
using Template.Services.Exceptions;
using Template.Services.Implementations;
using Xunit;

namespace Template.Tests.Services;

public class SampleItemServiceTests
{
    private readonly ISampleItemRepository _repository;
    private readonly SampleItemService _service;

    public SampleItemServiceTests()
    {
        _repository = new SampleItemRepository();
        _service = new SampleItemService(_repository, NullLogger<SampleItemService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsInitialSeedItems()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, x => x.Name == "Initial Sample Item");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesAndReturnsSampleItem()
    {
        // Arrange
        var request = new CreateSampleItemRequest
        {
            Name = "New Feature Item",
            Description = "Unit test description"
        };

        // Act
        var created = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("New Feature Item", created.Name);
        Assert.Equal("Unit test description", created.Description);
        Assert.True(created.IsActive);

        // Verify it can be retrieved
        var retrieved = await _service.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsExpectedItem()
    {
        // Arrange
        var seedId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetByIdAsync(seedId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(seedId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(nonExistingId));
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesItem()
    {
        // Arrange
        var createRequest = new CreateSampleItemRequest
        {
            Name = "Item To Delete",
            Description = "Will be deleted"
        };
        var created = await _service.CreateAsync(createRequest);

        // Act
        await _service.DeleteAsync(created.Id);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(nonExistingId));
    }
}
