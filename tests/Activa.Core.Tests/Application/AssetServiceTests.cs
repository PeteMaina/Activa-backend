using System;
using System.Threading.Tasks;
using Activa.Core.Application.DTOs;
using Activa.Core.Application.Services;
using Activa.Core.Domain.Enums;
using Activa.Core.Domain.Exceptions;
using Activa.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace Activa.Core.Tests.Application;

public class AssetServiceTests
{
    private readonly InMemoryAssetRepository _repository;
    private readonly AssetService _service;

    public AssetServiceTests()
    {
        _repository = new InMemoryAssetRepository();
        _service = new AssetService(_repository);
    }

    [Fact]
    public async Task CreateAssetAsync_ValidRequest_ShouldStoreAndReturnAssetDto()
    {
        // Arrange
        var request = new CreateAssetRequest(
            Code: "AST-2001",
            Name: "High-Performance GPU Server",
            Category: "Compute",
            Amount: 12000.00m,
            Currency: "USD",
            Facility: "DataCenter-1",
            Detail: "Rack 5",
            Latitude: 37.77,
            Longitude: -122.41,
            CreatedBy: "admin@activa.io"
        );

        // Act
        var result = await _service.CreateAssetAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Code.Should().Be("AST-2001");
        result.Name.Should().Be("High-Performance GPU Server");
        result.Status.Should().Be(AssetStatus.Active);
        result.AuditEntries.Should().HaveCount(1);

        var retrieved = await _service.GetAssetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Code.Should().Be("AST-2001");
    }

    [Fact]
    public async Task CreateAssetAsync_DuplicateCode_ShouldThrowDomainValidationException()
    {
        // Arrange
        var request = new CreateAssetRequest("AST-2002", "Server A", "Compute", 1000, "USD", "HQ", "", null, null, "admin");
        await _service.CreateAssetAsync(request);

        // Act
        Func<Task> act = async () => await _service.CreateAssetAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task SearchAssetsAsync_PaginationAndFiltering_ShouldReturnPagedResult()
    {
        // Arrange
        await _service.CreateAssetAsync(new CreateAssetRequest("AST-3001", "Alpha Server", "Hardware", 1000, "USD", "HQ", "", null, null, "admin"));
        await _service.CreateAssetAsync(new CreateAssetRequest("AST-3002", "Beta Server", "Hardware", 2000, "USD", "HQ", "", null, null, "admin"));
        await _service.CreateAssetAsync(new CreateAssetRequest("AST-3003", "Gamma Laptop", "Devices", 1500, "USD", "Office", "", null, null, "admin"));

        // Act
        var pagedHardware = await _service.SearchAssetsAsync(category: "Hardware", pageNumber: 1, pageSize: 2);

        // Assert
        pagedHardware.TotalCount.Should().Be(2);
        pagedHardware.Items.Should().HaveCount(2);
        pagedHardware.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAssetStatusAsync_ValidTransition_ShouldUpdateAndRecordAudit()
    {
        // Arrange
        var created = await _service.CreateAssetAsync(new CreateAssetRequest("AST-4001", "CNC Machine", "Factory", 75000, "USD", "Plant 1", "", null, null, "admin"));

        // Act
        var updated = await _service.UpdateAssetStatusAsync(created.Id, new UpdateAssetStatusRequest(AssetStatus.UnderMaintenance, "tech@activa.io", "Routine maintenance"));

        // Assert
        updated.Status.Should().Be(AssetStatus.UnderMaintenance);
        updated.AuditEntries.Should().HaveCount(2);
        updated.AuditEntries[1].Action.Should().Be("StatusTransition:Active->UnderMaintenance");
    }
}
