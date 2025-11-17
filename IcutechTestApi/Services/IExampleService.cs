using IcutechTestApi.DTOs;

namespace IcutechTestApi.Services;

public interface IExampleService
{
    Task<ExampleResponseDto> GetExampleDataAsync();
    Task<ExampleResponseDto> CreateExampleDataAsync(ExampleRequestDto request);
}

