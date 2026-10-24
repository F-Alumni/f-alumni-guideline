using Microsoft.AspNetCore.Mvc;
using Template.Services.DTOs.Common;
using Template.Services.DTOs.Requests;
using Template.Services.DTOs.Responses;
using Template.Services.Interfaces;

namespace Template.Api.Controllers;

/// <summary>
/// Sample REST API endpoints demonstrating standard routing, status codes, OpenAPI metadata, and pagination.
/// </summary>
[ApiController]
[Route("api/v1/sample-items")]
[Produces("application/json")]
public class SampleItemsController : ControllerBase
{
    private readonly ISampleItemService _sampleItemService;
    private readonly ILogger<SampleItemsController> _logger;

    public SampleItemsController(
        ISampleItemService sampleItemService,
        ILogger<SampleItemsController> logger)
    {
        _sampleItemService = sampleItemService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of all sample items.
    /// </summary>
    /// <response code="200">Returns the list of sample items.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SampleItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SampleItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET api/v1/sample-items requested.");
        var items = await _sampleItemService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of sample items.
    /// </summary>
    /// <param name="search">Optional search keyword matching item name or description.</param>
    /// <param name="pageNumber">1-based page number (default 1).</param>
    /// <param name="pageSize">Number of items per page (default 10, max 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the paginated list envelope with metadata.</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<SampleItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<SampleItemResponse>>> GetPaged(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("HTTP GET api/v1/sample-items/paged requested. Search: {Search}, Page: {Page}, Size: {Size}", search, pageNumber, pageSize);
        var pagedResult = await _sampleItemService.GetPagedAsync(search, pageNumber, pageSize, cancellationToken);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Retrieves a specific sample item by unique identifier.
    /// </summary>
    /// <param name="id">Unique GUID identifier of the sample item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the requested sample item.</response>
    /// <response code="404">Item with specified ID was not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SampleItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SampleItemResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET api/v1/sample-items/{Id} requested.", id);
        var item = await _sampleItemService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Creates a new sample item resource.
    /// </summary>
    /// <param name="request">Creation payload with name and optional description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Item created successfully, returns the created item and Location header.</response>
    /// <response code="400">Request validation failed or payload invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SampleItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SampleItemResponse>> Create(
        [FromBody] CreateSampleItemRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP POST api/v1/sample-items requested with name: {Name}", request.Name);
        var created = await _sampleItemService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Deletes a sample item resource by ID.
    /// </summary>
    /// <param name="id">Unique GUID identifier of the sample item to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Item deleted successfully.</response>
    /// <response code="404">Item with specified ID was not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP DELETE api/v1/sample-items/{Id} requested.", id);
        await _sampleItemService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
