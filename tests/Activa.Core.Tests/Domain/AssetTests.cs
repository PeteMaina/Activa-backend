using System;
using System.Linq;
using Activa.Core.Domain.Entities;
using Activa.Core.Domain.Enums;
using Activa.Core.Domain.Exceptions;
using Activa.Core.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Activa.Core.Tests.Domain;

public class AssetTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeAssetWithAuditAndDomainEvents()
    {
        // Arrange
        var code = "AST-1001";
        var name = "Enterprise Server Rack";
        var category = "Hardware";
        var cost = Money.Create(15000.50m, "USD");
        var location = Location.Create("DataCenter-A", "Rack 12", 37.7749, -122.4194);
        var createdBy = "admin@activa.io";

        // Act
        var asset = Asset.Create(code, name, category, cost, location, createdBy);

        // Assert
        asset.Id.Should().NotBe(Guid.Empty);
        asset.Code.Should().Be("AST-1001");
        asset.Name.Should().Be("Enterprise Server Rack");
        asset.Category.Should().Be("Hardware");
        asset.Status.Should().Be(AssetStatus.Active);
        asset.Cost.Should().Be(cost);
        asset.Location.Should().Be(location);
        asset.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        asset.CreatedBy.Should().Be(createdBy);

        asset.AuditEntries.Should().HaveCount(1);
        var audit = asset.AuditEntries.First();
        audit.Action.Should().Be("Created");
        audit.PerformedBy.Should().Be(createdBy);

        asset.DomainEvents.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("", "Name", "Category", "Invalid code")]
    [InlineData("AST-1", "", "Category", "Invalid name")]
    [InlineData("AST-1", "Name", "", "Invalid category")]
    public void Create_WithInvalidArguments_ShouldThrowDomainException(string code, string name, string category, string scenario)
    {
        // Arrange
        var cost = Money.Create(100, "USD");
        var location = Location.Create("HQ", "Room 1");

        // Act
        Action act = () => Asset.Create(code, name, category, cost, location, "admin");

        // Assert
        act.Should().Throw<DomainValidationException>(scenario);
    }

    [Fact]
    public void TransitionStatus_ValidLifecycle_ShouldUpdateStatusAndLogAudit()
    {
        // Arrange
        var asset = Asset.Create("AST-1002", "Robotic Arm", "Machinery", Money.Create(50000, "USD"), Location.Create("Factory-1"), "system");

        // Act 1: Move Active -> UnderMaintenance
        asset.TransitionStatus(AssetStatus.UnderMaintenance, "tech@activa.io", "Scheduled annual inspection");

        // Assert 1
        asset.Status.Should().Be(AssetStatus.UnderMaintenance);
        asset.AuditEntries.Should().HaveCount(2);
        asset.AuditEntries.Last().Action.Should().Be("StatusTransition:Active->UnderMaintenance");

        // Act 2: Move UnderMaintenance -> Active
        asset.TransitionStatus(AssetStatus.Active, "tech@activa.io", "Inspection completed successfully");

        // Assert 2
        asset.Status.Should().Be(AssetStatus.Active);

        // Act 3: Move Active -> Retired
        asset.TransitionStatus(AssetStatus.Retired, "manager@activa.io", "End of lifecycle");

        // Assert 3
        asset.Status.Should().Be(AssetStatus.Retired);
    }

    [Fact]
    public void TransitionStatus_InvalidLifecycleFromRetired_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("AST-1003", "Legacy Router", "Hardware", Money.Create(1000, "USD"), Location.Create("Warehouse"), "admin");
        asset.TransitionStatus(AssetStatus.Retired, "admin", "Decommissioned");

        // Act
        Action act = () => asset.TransitionStatus(AssetStatus.Active, "admin", "Attempt reactivate");

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Money_WithDifferentCurrencies_ShouldNotBeEqual()
    {
        var usd = Money.Create(100, "USD");
        var eur = Money.Create(100, "EUR");

        usd.Equals(eur).Should().BeFalse();
    }
}
