using System.IO.Compression;
using IcutechTestApi.Services;
using IcutechTestApi.Clients;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register services
builder.Services.AddScoped<IExampleService, ExampleService>();

// Register SOAP client
builder.Services.AddHttpClient<ISoapAuthClient, SoapAuthClient>();

// Response compression (Gzip/Brotli)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/javascript", "text/css", "application/json", "text/html" }
    );
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Add CORS to allow frontend on any host to call API
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS (must be before UseAuthorization and MapControllers)
app.UseCors();

// Use response compression
app.UseResponseCompression();

// Serve static files with caching headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLowerInvariant();
        if (path.EndsWith(".min.css") || path.EndsWith(".min.js"))
        {
            // Long-term caching for minified files
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
        else if (path.EndsWith(".html"))
        {
            // Short-term caching for HTML
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600, must-revalidate");
        }
        else if (path.EndsWith(".css") || path.EndsWith(".js"))
        {
            // Medium-term caching for regular files
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=86400");
        }
    }
});

app.UseAuthorization();
app.MapControllers();

app.Run();
