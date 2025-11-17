using IcutechTestApi.DTOs;
using IcutechTestApi.Models;

namespace IcutechTestApi.Services;

public class ExampleService : IExampleService
{
    public async Task<ExampleResponseDto> GetExampleDataAsync()
    {
        // Simulate async operation
        await Task.Delay(100);
        
        var model = new ExampleModel
        {
            Id = Guid.NewGuid(),
            Name = "Example Data",
            CreatedAt = DateTime.UtcNow
        };

        return new ExampleResponseDto
        {
            Id = model.Id,
            Name = model.Name,
            CreatedAt = model.CreatedAt
        };
    }

    public async Task<ExampleResponseDto> CreateExampleDataAsync(ExampleRequestDto request)
    {
        // Simulate async operation
        await Task.Delay(100);

        var model = new ExampleModel
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        return new ExampleResponseDto
        {
            Id = model.Id,
            Name = model.Name,
            CreatedAt = model.CreatedAt
        };
    }
}

