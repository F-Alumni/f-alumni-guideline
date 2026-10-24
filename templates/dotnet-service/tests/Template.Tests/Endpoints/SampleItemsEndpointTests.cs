using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Template.Services.DTOs.Requests;
using Template.Services.DTOs.Responses;
using Xunit;

namespace Template.Tests.Endpoints;

public class SampleItemsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SampleItemsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOkStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task GetAllEndpoint_ReturnsSuccessStatusCodeAndItems()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/sample-items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<SampleItemResponse>>();
        Assert.NotNull(items);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task PostEndpoint_ValidPayload_ReturnsCreatedAndResource()
    {
        // Arrange
        var request = new CreateSampleItemRequest
        {
            Name = "Integration Test Item",
            Description = "Created via WebApplicationFactory test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sample-items", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SampleItemResponse>();
        Assert.NotNull(created);
        Assert.Equal(request.Name, created.Name);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetByIdEndpoint_NonExistingId_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/sample-items/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
