using System;
using System.Collections.Generic;
using Activa.Core.Domain.Enums;

namespace Activa.Core.Application.DTOs;

public record CreateAssetRequest(
    string Code,
    string Name,
    string Category,
    decimal Amount,
    string Currency,
    string Facility,
    string Detail,
    double? Latitude,
    double? Longitude,
    string CreatedBy
);

public record UpdateAssetStatusRequest(
    AssetStatus NewStatus,
    string PerformedBy,
    string Reason
);

public record AuditEntryDto(
    Guid Id,
    string Action,
    string PerformedBy,
    string Description,
    DateTimeOffset TimestampUtc
);

public record AssetDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    AssetStatus Status,
    decimal Amount,
    string Currency,
    string Facility,
    string LocationDetail,
    double? Latitude,
    double? Longitude,
    DateTimeOffset CreatedAtUtc,
    string CreatedBy,
    DateTimeOffset LastModifiedAtUtc,
    IReadOnlyList<AuditEntryDto> AuditEntries
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
