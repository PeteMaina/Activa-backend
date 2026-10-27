using System;
using Activa.Core.Domain.Entities;
using Activa.Core.Domain.Enums;
using Activa.Core.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Activa.Core.Tests.Domain;

public class AssetMaintenanceAndAuditTests
{
    [Fact]
    public void ScheduleMaintenance_FutureDate_ShouldRecordAuditEntryAndSetMaintenanceWindow()
    {
        // Arrange
        var asset = Asset.Create("AST-MNT-1", "Hydraulic Press", "Industrial", Money.Create(120000, "USD"), Location.Create("Factory-2"), "admin");
        var scheduledDate = DateTimeOffset.UtcNow.AddDays(14);

        // Act
        asset.ScheduleMaintenance(scheduledDate, "Annual calibration", "tech-lead");

        // Assert
        asset.NextScheduledMaintenanceUtc.Should().Be(scheduledDate);
        asset.AuditEntries.Should().Contain(a => a.Action == "MaintenanceScheduled" && a.PerformedBy == "tech-lead");
    }

    [Fact]
    public void CompleteMaintenance_WhenScheduled_ShouldUpdateLastMaintenanceDateAndTransitionStatusToActive()
    {
        // Arrange
        var asset = Asset.Create("AST-MNT-2", "Turbine Generator", "Energy", Money.Create(500000, "USD"), Location.Create("Station 4"), "admin");
        asset.TransitionStatus(AssetStatus.UnderMaintenance, "admin", "Starting maintenance");
        var completedDate = DateTimeOffset.UtcNow;

        // Act
        asset.CompleteMaintenance(completedDate, "Overhaul finished", "lead-engineer");

        // Assert
        asset.LastMaintenanceUtc.Should().Be(completedDate);
        asset.Status.Should().Be(AssetStatus.Active);
        asset.AuditEntries.Should().Contain(a => a.Action == "MaintenanceCompleted" && a.PerformedBy == "lead-engineer");
    }
}
