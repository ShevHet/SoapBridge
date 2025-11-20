using System.Diagnostics;
using System.Text;

namespace IcutechTestApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsStaticFile(context.Request.Path) || IsHtmlRequest(context.Request))
        {
            await _next(context);
            return;
        }

        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        await LogRequest(context, requestId);

        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string requestId)
    {
        var request = context.Request;
        
        var logMessage = new StringBuilder();
        logMessage.AppendLine($"[{requestId}] Incoming Request:");
        logMessage.AppendLine($"  Method: {request.Method}");
        logMessage.AppendLine($"  Path: {request.Path}{request.QueryString}");
        logMessage.AppendLine($"  IP: {GetClientIp(context)}");
        logMessage.AppendLine($"  User-Agent: {request.Headers["User-Agent"].FirstOrDefault()}");

        if ((request.Method == "POST" || request.Method == "PUT") && 
            request.ContentType?.Contains("application/json") == true &&
            request.ContentLength > 0 && request.ContentLength < 10000)
        {
            request.EnableBuffering();
            var buffer = new byte[request.ContentLength.Value];
            await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
            var bodyText = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;

            bodyText = MaskSensitiveData(bodyText);
            logMessage.AppendLine($"  Body: {bodyText}");
        }

        _logger.LogInformation(logMessage.ToString().TrimEnd());
    }

    private async Task LogResponse(HttpContext context, string requestId, long elapsedMs)
    {
        var response = context.Response;
        
        var logMessage = new StringBuilder();
        logMessage.AppendLine($"[{requestId}] Response:");
        logMessage.AppendLine($"  Status: {response.StatusCode}");
        logMessage.AppendLine($"  Duration: {elapsedMs}ms");

        if (response.StatusCode >= 400 && response.Body.CanSeek)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            if (responseText.Length < 1000)
            {
                logMessage.AppendLine($"  Body: {responseText}");
            }
        }

        var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                      response.StatusCode >= 400 ? LogLevel.Warning :
                      LogLevel.Information;

        _logger.Log(logLevel, logMessage.ToString().TrimEnd());
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

    private static bool IsStaticFile(PathString path)
    {
        var staticExtensions = new[] { ".css", ".js", ".jpg", ".jpeg", ".png", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot" };
        return staticExtensions.Any(ext => path.Value?.EndsWith(ext, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static bool IsHtmlRequest(HttpRequest request)
    {
        return request.Path.Value == "/" || 
               request.Path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true ||
               request.Headers["Accept"].Any(a => a?.Contains("text/html") == true);
    }

    private static string MaskSensitiveData(string text)
    {
        var patterns = new[]
        {
            (@"""password""\s*:\s*""[^""]*""", @"""password"":""***"""),
            (@"""Password""\s*:\s*""[^""]*""", @"""Password"":""***"""),
            (@"""token""\s*:\s*""[^""]*""", @"""token"":""***"""),
            (@"""apiKey""\s*:\s*""[^""]*""", @"""apiKey"":""***""")
        };

        foreach (var (pattern, replacement) in patterns)
        {
            text = System.Text.RegularExpressions.Regex.Replace(text, pattern, replacement);
        }

        return text;
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}

