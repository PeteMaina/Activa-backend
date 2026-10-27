using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Activa.Core.Application.DTOs;
using Activa.Core.Application.Interfaces;
using Activa.Core.Domain.Entities;
using Activa.Core.Domain.Enums;
using Activa.Core.Domain.Exceptions;
using Activa.Core.Domain.ValueObjects;

namespace Activa.Core.Application.Services;

public class AssetService
{
    private readonly IAssetRepository _repository;

    public AssetService(IAssetRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing != null)
        {
            throw new DomainValidationException($"An asset with code '{request.Code}' already exists.");
        }

        var money = Money.Create(request.Amount, request.Currency);
        var location = Location.Create(request.Facility, request.Detail, request.Latitude, request.Longitude);

        var asset = Asset.Create(
            code: request.Code,
            name: request.Name,
            category: request.Category,
            cost: money,
            location: location,
            createdBy: request.CreatedBy
        );

        await _repository.AddAsync(asset, cancellationToken);

        return MapToDto(asset);
    }

    public async Task<AssetDto?> GetAssetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _repository.GetByIdAsync(id, cancellationToken);
        return asset == null ? null : MapToDto(asset);
    }

    public async Task<PagedResult<AssetDto>> SearchAssetsAsync(
        string? searchTerm = null,
        string? category = null,
        AssetStatus? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.SearchAsync(searchTerm, category, status, pageNumber, pageSize, cancellationToken);
        var dtos = result.Items.Select(MapToDto).ToList();

        return new PagedResult<AssetDto>(dtos, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<AssetDto> UpdateAssetStatusAsync(Guid id, UpdateAssetStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var asset = await _repository.GetByIdAsync(id, cancellationToken);
        if (asset == null)
        {
            throw new KeyNotFoundException($"Asset with ID '{id}' was not found.");
        }

        asset.TransitionStatus(request.NewStatus, request.PerformedBy, request.Reason);
        await _repository.UpdateAsync(asset, cancellationToken);

        return MapToDto(asset);
    }

    private static AssetDto MapToDto(Asset asset)
    {
        var auditDtos = asset.AuditEntries
            .Select(a => new AuditEntryDto(a.Id, a.Action, a.PerformedBy, a.Description, a.TimestampUtc))
            .ToList();

        return new AssetDto(
            Id: asset.Id,
            Code: asset.Code,
            Name: asset.Name,
            Category: asset.Category,
            Status: asset.Status,
            Amount: asset.Cost.Amount,
            Currency: asset.Cost.Currency,
            Facility: asset.Location.Facility,
            LocationDetail: asset.Location.Detail,
            Latitude: asset.Location.Latitude,
            Longitude: asset.Location.Longitude,
            CreatedAtUtc: asset.CreatedAtUtc,
            CreatedBy: asset.CreatedBy,
            LastModifiedAtUtc: asset.LastModifiedAtUtc,
            AuditEntries: auditDtos
        );
    }
}
