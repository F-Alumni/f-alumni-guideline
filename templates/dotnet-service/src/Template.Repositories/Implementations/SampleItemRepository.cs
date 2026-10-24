using System.Collections.Concurrent;
using Template.Repositories.Entities;
using Template.Repositories.Interfaces;

namespace Template.Repositories.Implementations;

/// <summary>
/// Thread-safe in-memory repository implementation for SampleItem.
/// Can be replaced with EF Core DbContext implementation in specific services.
/// </summary>
public class SampleItemRepository : ISampleItemRepository
{
    private readonly ConcurrentDictionary<Guid, SampleItem> _storage = new();

    public SampleItemRepository()
    {
        // Seed initial sample data
        var seedItem1 = new SampleItem
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Initial Sample Item",
            Description = "Seeded item to verify that the template works out of the box.",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _storage.TryAdd(seedItem1.Id, seedItem1);
    }

    public Task<IReadOnlyList<SampleItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = _storage.Values
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<SampleItem>>(items);
    }

    public Task<SampleItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _storage.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<SampleItem> CreateAsync(SampleItem item, CancellationToken cancellationToken = default)
    {
        _storage[item.Id] = item;
        return Task.FromResult(item);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = _storage.TryRemove(id, out _);
        return Task.FromResult(removed);
    }
}
