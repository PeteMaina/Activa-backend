using System;
using Activa.Core.Domain.Entities;
using Activa.Core.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Activa.Core.Tests.Domain;

public class AssetDepreciationAndTaggingTests
{
    [Fact]
    public void CalculateDepreciation_StraightLine_ShouldReturnCorrectCurrentValuation()
    {
        // Arrange
        var initialCost = Money.Create(10000m, "USD");
        var salvageValue = Money.Create(1000m, "USD");
        var usefulLifeYears = 5;
        var acquisitionDate = DateTimeOffset.UtcNow.AddYears(-2);

        var asset = Asset.Create("AST-DEP-1", "High-End Workstation", "Hardware", initialCost, Location.Create("Main Lab"), "admin");

        // Act
        var valuation = asset.CalculateDepreciatedValue(salvageValue, usefulLifeYears, acquisitionDate, DateTimeOffset.UtcNow);

        // Assert: Annual depreciation = (10000 - 1000) / 5 = 1800/yr. Over 2 years = 3600 depreciation. Remaining = 6400.
        valuation.Amount.Should().BeApproximately(6400m, 50m);
        valuation.Currency.Should().Be("USD");
    }

    [Fact]
    public void AddTag_ValidTag_ShouldAddUniqueTags()
    {
        // Arrange
        var asset = Asset.Create("AST-TAG-1", "Precision Router", "Networking", Money.Create(2500, "USD"), Location.Create("Server Room"), "admin");

        // Act
        asset.AddTag("Critical-Infra");
        asset.AddTag("Critical-Infra"); // Duplicate should be ignored
        asset.AddTag("Tier-1");

        // Assert
        asset.Tags.Should().HaveCount(2);
        asset.Tags.Should().Contain("CRITICAL-INFRA");
        asset.Tags.Should().Contain("TIER-1");
    }

    [Fact]
    public void RemoveTag_ExistingTag_ShouldRemoveTag()
    {
        // Arrange
        var asset = Asset.Create("AST-TAG-2", "Mobile Drone", "Robotics", Money.Create(8000, "USD"), Location.Create("Field"), "admin");
        asset.AddTag("Autonomous");
        asset.AddTag("Experimental");

        // Act
        asset.RemoveTag("Experimental");

        // Assert
        asset.Tags.Should().HaveCount(1);
        asset.Tags.Should().Contain("AUTONOMOUS");
    }
}
