using System;
using System.Collections.Generic;
using Activa.Core.Domain.Enums;
using Activa.Core.Domain.Events;
using Activa.Core.Domain.Exceptions;
using Activa.Core.Domain.ValueObjects;

namespace Activa.Core.Domain.Entities;

public class Asset
{
    private readonly List<AuditEntry> _auditEntries = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public AssetStatus Status { get; private set; }
    public Money Cost { get; private set; }
    public Location Location { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset LastModifiedAtUtc { get; private set; }

    public IReadOnlyCollection<AuditEntry> AuditEntries => _auditEntries.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Asset() { }

    public static Asset Create(
        string code,
        string name,
        string category,
        Money cost,
        Location location,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainValidationException("Asset code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Asset name is required.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainValidationException("Asset category is required.");
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainValidationException("CreatedBy identity is required.");

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Category = category.Trim(),
            Status = AssetStatus.Active,
            Cost = cost,
            Location = location,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = createdBy.Trim(),
            LastModifiedAtUtc = DateTimeOffset.UtcNow
        };

        asset.AddAuditEntry("Created", createdBy, $"Asset '{asset.Code}' created.");
        asset._domainEvents.Add(new AssetCreatedEvent(asset.Id, asset.Code, asset.CreatedBy));

        return asset;
    }

    public void TransitionStatus(AssetStatus newStatus, string performedBy, string reason = "")
    {
        if (string.IsNullOrWhiteSpace(performedBy))
            throw new DomainValidationException("PerformedBy identity is required for status transition.");

        if (Status == AssetStatus.Retired && newStatus != AssetStatus.Retired)
            throw new InvalidStatusTransitionException($"Cannot transition asset '{Code}' from Retired state.");

        if (Status == newStatus)
            return;

        var previousStatus = Status;
        Status = newStatus;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;

        AddAuditEntry($"StatusTransition:{previousStatus}->{newStatus}", performedBy, reason);
        _domainEvents.Add(new AssetStatusChangedEvent(Id, previousStatus, newStatus, performedBy, reason));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddAuditEntry(string action, string performedBy, string description)
    {
        _auditEntries.Add(new AuditEntry(action, performedBy, description));
    }
}
