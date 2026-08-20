using System.Net;
using System.Net.Http.Json;
using BmbOrdering.Api.Contracts.Authentication;
using BmbOrdering.IntegrationTests.Infrastructure;

namespace BmbOrdering.IntegrationTests;

public sealed class AuthenticationEndpointsTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationEndpointsTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateHttpsClient();
    }

    [Fact]
    public async Task RegisterAndLogin_WithValidCredentials_ReturnsToken()
    {
        var email = $"customer.{Guid.NewGuid():N}@example.com";

        var login = await ApiTestClient.RegisterAndLoginAsync(
            _client,
            email,
            "Demiana Integration");

        Assert.Equal(email, login.Email);
        Assert.Equal("Demiana Integration", login.FullName);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.True(login.AccessTokenExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"duplicate.{Guid.NewGuid():N}@example.com";
        await ApiTestClient.RegisterAndLoginAsync(_client, email);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterCustomerRequest(
                "Duplicate Customer",
                email.ToUpperInvariant(),
                "StrongPass1",
                "StrongPass1"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var email = $"invalid.{Guid.NewGuid():N}@example.com";
        await ApiTestClient.RegisterAndLoginAsync(_client, email);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginCustomerRequest(email, "WrongPass1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Orders_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
