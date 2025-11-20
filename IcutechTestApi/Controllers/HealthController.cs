using Microsoft.AspNetCore.Mvc;
using IcutechTestApi.Clients;

namespace IcutechTestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ISoapAuthClient _soapAuthClient;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ISoapAuthClient soapAuthClient, ILogger<HealthController> logger)
    {
        _soapAuthClient = soapAuthClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "IcutechTestApi",
            version = "1.0.0",
            checks = new
            {
                api = new { status = "healthy", message = "API is running" },
                soapService = await CheckSoapServiceHealth()
            }
        };

        return Ok(health);
    }

    [HttpGet("ready")]
    public IActionResult GetReadiness()
    {
        return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }

    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    private async Task<object> CheckSoapServiceHealth()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await httpClient.GetAsync("http://isapi.mekashron.com/icu-tech/icutech-test.dll", cts.Token);
            
            return new 
            { 
                status = response.IsSuccessStatusCode ? "healthy" : "degraded",
                message = response.IsSuccessStatusCode 
                    ? "SOAP service is responding" 
                    : $"SOAP service responding with status {response.StatusCode}",
                statusCode = (int)response.StatusCode
            };
        }
        catch (TaskCanceledException)
        {
            return new 
            { 
                status = "unhealthy", 
                message = "SOAP service timeout - service may be down"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for SOAP service");
            return new 
            { 
                status = "unhealthy", 
                message = $"SOAP service unavailable: {ex.Message}"
            };
        }
    }
}

