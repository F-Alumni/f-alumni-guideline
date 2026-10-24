using Template.Api.Middlewares;
using Template.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Logging & Configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 2. Add Framework Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// 3. Add Application Layer (Services & Repositories)
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// 4. Configure HTTP Request Pipeline
// Centralized Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();

// Health Check Endpoint (Required by Pass Criteria & Monitoring)
app.MapHealthChecks("/health");

// Map API Controllers
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
