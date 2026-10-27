using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Activa.Core.Application.DTOs;
using Activa.Core.Application.Interfaces;
using Activa.Core.Domain.Entities;
using Activa.Core.Domain.Enums;

namespace Activa.Infrastructure.Persistence;

public class InMemoryAssetRepository : IAssetRepository
{
    private readonly ConcurrentDictionary<Guid, Asset> _assets = new();

    public Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _assets.TryGetValue(id, out var asset);
        return Task.FromResult(asset);
    }

    public Task<Asset?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Task.FromResult<Asset?>(null);

        var normalizedCode = code.Trim().ToUpperInvariant();
        var asset = _assets.Values.FirstOrDefault(a => a.Code.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(asset);
    }

    public Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        _assets[asset.Id] = asset;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        _assets[asset.Id] = asset;
        return Task.CompletedTask;
    }

    public Task<PagedResult<Asset>> SearchAsync(
        string? searchTerm = null,
        string? category = null,
        AssetStatus? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _assets.Values.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(a => a.Code.ToLowerInvariant().Contains(term) || a.Name.ToLowerInvariant().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim().ToLowerInvariant();
            query = query.Where(a => a.Category.ToLowerInvariant() == cat);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<Asset>(items, totalCount, pageNumber, pageSize));
    }
}
