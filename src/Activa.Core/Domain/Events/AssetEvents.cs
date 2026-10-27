using System;
using Activa.Core.Domain.Enums;
using Activa.Core.Domain.Events;

namespace Activa.Core.Domain.Events;

public sealed record AssetCreatedEvent(Guid AssetId, string Code, string CreatedBy) : DomainEvent;

public sealed record AssetStatusChangedEvent(Guid AssetId, AssetStatus PreviousStatus, AssetStatus NewStatus, string ChangedBy, string Reason) : DomainEvent;
