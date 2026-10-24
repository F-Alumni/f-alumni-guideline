using Microsoft.AspNetCore.Mvc;
using Template.Services.DTOs.Requests;
using Template.Services.DTOs.Responses;
using Template.Services.Interfaces;

namespace Template.Api.Controllers;

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
    /// Gets all sample items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SampleItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SampleItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET api/v1/sample-items requested.");
        var items = await _sampleItemService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Gets a sample item by ID.
    /// </summary>
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
    /// Creates a new sample item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SampleItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SampleItemResponse>> Create([FromBody] CreateSampleItemRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP POST api/v1/sample-items requested with name: {Name}", request.Name);
        var created = await _sampleItemService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Deletes a sample item by ID.
    /// </summary>
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
