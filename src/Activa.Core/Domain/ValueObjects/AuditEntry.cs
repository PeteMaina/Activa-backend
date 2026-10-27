using System;

namespace Activa.Core.Domain.ValueObjects;

public sealed record AuditEntry
{
    public Guid Id { get; }
    public string Action { get; }
    public string PerformedBy { get; }
    public string Description { get; }
    public DateTimeOffset TimestampUtc { get; }

    public AuditEntry(string action, string performedBy, string description = "")
    {
        Id = Guid.NewGuid();
        Action = action;
        PerformedBy = performedBy;
        Description = description;
        TimestampUtc = DateTimeOffset.UtcNow;
    }
}
