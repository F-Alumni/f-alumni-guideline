var builder = WebApplication.CreateBuilder(args);

// Register YARP Reverse Proxy from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Health probe endpoint
app.MapHealthChecks("/health");

// Root information endpoint
app.MapGet("/", () => Results.Ok(new
{
    gateway = "FPT Alumni Connect — YARP API Gateway",
    status = "Healthy",
    version = "1.0.0",
    routes = new[]
    {
        "/api/chat/{**remainder} -> chat-service",
        "/api/v1/chats/{**remainder} -> chat-service"
    }
}));

// Map reverse proxy pipeline
app.MapReverseProxy();

app.Run();

namespace ApiGateway
{
    public partial class Program { }
}
