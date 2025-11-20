using System.Collections.Concurrent;

namespace IcutechTestApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
    
    private const int MaxRequestsPerMinute = 30;
    private const int MaxRequestsPerHour = 500;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastCleanup = DateTime.UtcNow;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIp(context);
        var now = DateTime.UtcNow;

        if ((now - _lastCleanup) > CleanupInterval)
        {
            CleanupOldEntries();
            _lastCleanup = now;
        }

        var clientInfo = _clients.GetOrAdd(clientIp, _ => new ClientRequestInfo());

        bool rateLimitExceeded = false;
        string? errorMessage = null;
        int retryAfter = 0;
        int requestsLastMinute = 0;

        await clientInfo.Semaphore.WaitAsync();
        try
        {
            clientInfo.Requests.RemoveAll(r => (now - r) > TimeSpan.FromHours(1));

            requestsLastMinute = clientInfo.Requests.Count(r => (now - r) < TimeSpan.FromMinutes(1));
            var requestsLastHour = clientInfo.Requests.Count;

            if (requestsLastMinute >= MaxRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for IP {IP}: {Count} requests in last minute", 
                    clientIp, requestsLastMinute);
                
                rateLimitExceeded = true;
                errorMessage = $"Превышен лимит запросов. Максимум {MaxRequestsPerMinute} запросов в минуту.";
                retryAfter = 60;
            }
            else if (requestsLastHour >= MaxRequestsPerHour)
            {
                _logger.LogWarning("Rate limit exceeded for IP {IP}: {Count} requests in last hour", 
                    clientIp, requestsLastHour);
                
                rateLimitExceeded = true;
                errorMessage = $"Превышен часовой лимит запросов. Максимум {MaxRequestsPerHour} запросов в час.";
                retryAfter = 3600;
            }
            else
            {
                clientInfo.Requests.Add(now);
            }
        }
        finally
        {
            clientInfo.Semaphore.Release();
        }

        if (rateLimitExceeded)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = retryAfter.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = errorMessage,
                retryAfter = retryAfter
            });
            return;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = MaxRequestsPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, MaxRequestsPerMinute - requestsLastMinute).ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow.AddHours(-2);
        var keysToRemove = _clients
            .Where(kvp => !kvp.Value.Requests.Any() || kvp.Value.Requests.Max() < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _clients.TryRemove(key, out _);
        }
    }

    private class ClientRequestInfo
    {
        public List<DateTime> Requests { get; } = new();
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}

