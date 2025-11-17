using Microsoft.AspNetCore.Mvc;
using IcutechTestApi.DTOs;
using IcutechTestApi.Services;

namespace IcutechTestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IExampleService _exampleService;
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(IExampleService exampleService, ILogger<ExampleController> logger)
    {
        _exampleService = exampleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ExampleResponseDto>> GetExample()
    {
        _logger.LogInformation("Getting example data");
        var result = await _exampleService.GetExampleDataAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExampleResponseDto>> CreateExample([FromBody] ExampleRequestDto request)
    {
        _logger.LogInformation("Creating example data");
        var result = await _exampleService.CreateExampleDataAsync(request);
        return CreatedAtAction(nameof(GetExample), new { id = result.Id }, result);
    }
}

