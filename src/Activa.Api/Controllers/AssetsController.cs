using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Activa.Core.Application.DTOs;
using Activa.Core.Application.Services;
using Activa.Core.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Activa.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AssetsController : ControllerBase
{
    private readonly AssetService _assetService;

    public AssetsController(AssetService assetService)
    {
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
    }

    /// <summary>
    /// Creates a new asset in the system.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await _assetService.CreateAssetAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAssetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves an asset by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssetById(Guid id, CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetAssetByIdAsync(id, cancellationToken);
        if (asset == null)
        {
            throw new KeyNotFoundException($"Asset with ID '{id}' was not found.");
        }
        return Ok(asset);
    }

    /// <summary>
    /// Searches and paginates assets based on query filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AssetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAssets(
        [FromQuery] string? searchTerm,
        [FromQuery] string? category,
        [FromQuery] AssetStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _assetService.SearchAssetsAsync(searchTerm, category, status, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Transitions asset status with state validation and audit logging.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAssetStatus(Guid id, [FromBody] UpdateAssetStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _assetService.UpdateAssetStatusAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
