using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Activa.Core.Application.DTOs;
using Activa.Core.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Activa.Api.Tests;

public class AssetsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AssetsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthAndLivenessEndpoints_ShouldReturnHealthyStatus()
    {
        // Act
        var healthResponse = await _client.GetAsync("/healthz");
        var liveResponse = await _client.GetAsync("/healthz/live");
        var readyResponse = await _client.GetAsync("/healthz/ready");

        // Assert
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        liveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        readyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAsset_ValidPayload_ShouldReturn201CreatedAndLocationHeader()
    {
        // Arrange
        var request = new CreateAssetRequest(
            Code: "API-AST-101",
            Name: "API Testing Server",
            Category: "Hardware",
            Amount: 4999.99m,
            Currency: "USD",
            Facility: "DC-East",
            Detail: "Rack 1",
            Latitude: 40.7128,
            Longitude: -74.0060,
            CreatedBy: "api-test-runner"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/assets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var dto = await response.Content.ReadFromJsonAsync<AssetDto>();
        dto.Should().NotBeNull();
        dto!.Code.Should().Be("API-AST-101");
        dto.Name.Should().Be("API Testing Server");
    }

    [Fact]
    public async Task GetAssetById_NonExistentId_ShouldReturn404ProblemDetails()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/assets/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Resource Not Found");
        content.Should().Contain("type");
        content.Should().Contain("title");
    }

    [Fact]
    public async Task UpdateAssetStatus_ValidTransition_ShouldReturn200OK()
    {
        // Arrange: Create asset first
        var createRequest = new CreateAssetRequest("API-AST-202", "Robotic Controller", "Factory", 25000, "USD", "Plant-A", "", null, null, "tester");
        var createResponse = await _client.PostAsJsonAsync("/api/v1/assets", createRequest);
        var createdDto = await createResponse.Content.ReadFromJsonAsync<AssetDto>();

        var statusRequest = new UpdateAssetStatusRequest(AssetStatus.UnderMaintenance, "tech-lead", "Firmware update");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/assets/{createdDto!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedDto = await response.Content.ReadFromJsonAsync<AssetDto>();
        updatedDto!.Status.Should().Be(AssetStatus.UnderMaintenance);
    }
}
