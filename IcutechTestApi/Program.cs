using Microsoft.OpenApi.Models;
using IcutechTestApi.Clients;
using IcutechTestApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Icutech Test API", Version = "v1" });
});

var useMockClient = builder.Configuration.GetValue<bool>("SoapService:UseMockClient") ||
                    Environment.GetEnvironmentVariable("USE_MOCK_SOAP_CLIENT")?.ToLower() == "true";

if (useMockClient)
{
    builder.Services.AddScoped<ISoapAuthClient, MockSoapAuthClient>();
    Console.WriteLine("Using Mock SOAP Client for testing");
}
else
{
    builder.Services.AddHttpClient<ISoapAuthClient, SoapAuthClient>();
    Console.WriteLine("Using Real SOAP Client");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Icutech Test API v1");
    c.RoutePrefix = "swagger";
});

app.UseRequestLogging();
app.UseRateLimiting();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/example", () => new { message = "API is running", status = "ok" });
app.MapFallbackToFile("index.html");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5030";
app.Run($"http://+:{port}");

