using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Read ApiKey from configuration (appsettings.json or env var)
var apiKey = builder.Configuration["ApiKey"] ?? "NO-API-KEY";

var app = builder.Build();

app.MapGet("/", () => "Hello from SimpleCICD!");

app.MapGet("/secret", () => Results.Ok(new {
    Message = "This endpoint returns the injected ApiKey (for demo only).",
    ApiKey = apiKey
}));

app.Run();
