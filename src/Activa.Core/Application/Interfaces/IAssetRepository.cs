using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Activa.Core.Application.DTOs;
using Activa.Core.Domain.Entities;
using Activa.Core.Domain.Enums;

namespace Activa.Core.Application.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Asset?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
    Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default);
    Task<PagedResult<Asset>> SearchAsync(
        string? searchTerm = null,
        string? category = null,
        AssetStatus? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );
}
