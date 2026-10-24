using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Template.Services.DTOs.Requests;
using Template.Services.DTOs.Responses;
using Xunit;

namespace Template.Tests.Endpoints;

public class JwtAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string JwtSecret = "FAlumniConnect_SuperSecretSigningKey_MustBeAtLeast32BytesLong_PRN232_ASM_2026!";
    private const string JwtIssuer = "https://identity.falumni.local";
    private const string JwtAudience = "falumni-services";

    public JwtAuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static string GenerateTestJwt(string userId, string role, TimeSpan? lifetime = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(JwtSecret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role),
            new("role", role),
            new(JwtRegisteredClaimNames.Email, $"{userId}@fpt.edu.vn"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_MissingToken_Returns401Unauthorized()
    {
        // Act (No Authorization header)
        var response = await _client.GetAsync("/api/v1/sample-items/admin-only");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_InvalidToken_Returns401Unauthorized()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/sample-items/admin-only");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.jwt.token.value");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_UserRoleToken_Returns403Forbidden()
    {
        // Arrange: Valid token with role USER (insufficient role)
        var userToken = GenerateTestJwt(Guid.NewGuid().ToString(), "USER");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/sample-items/admin-only");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_AdminRoleToken_Returns200Ok()
    {
        // Arrange: Valid token with role ADMIN
        var adminToken = GenerateTestJwt(Guid.NewGuid().ToString(), "ADMIN");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/sample-items/admin-only");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateSampleItem_MissingToken_Returns401Unauthorized()
    {
        // Arrange
        var payload = new CreateSampleItemRequest
        {
            Name = "Unauthorized Item",
            Description = "Should fail without token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sample-items", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSampleItem_ValidUserToken_Returns201Created()
    {
        // Arrange
        var userToken = GenerateTestJwt(Guid.NewGuid().ToString(), "USER");
        var payload = new CreateSampleItemRequest
        {
            Name = "Authorized Item " + Guid.NewGuid().ToString()[..6],
            Description = "Created by authenticated user"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sample-items");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        request.Content = JsonContent.Create(payload);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SampleItemResponse>();
        Assert.NotNull(created);
        Assert.Equal(payload.Name, created.Name);
    }

    [Fact]
    public async Task DeleteSampleItem_UserRoleToken_Returns403Forbidden()
    {
        // Arrange: User has role USER, but DELETE requires ADMIN
        var userToken = GenerateTestJwt(Guid.NewGuid().ToString(), "USER");
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/sample-items/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserProfile_ValidToken_ReturnsExtractedClaims()
    {
        // Arrange
        var userId = "student-12345";
        var token = GenerateTestJwt(userId, "USER");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/sample-items/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(json);
        Assert.Equal(userId, json["userId"]);
        Assert.Equal("USER", json["role"]);
    }
}
