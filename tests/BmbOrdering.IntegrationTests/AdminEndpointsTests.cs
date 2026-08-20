using System.Net;
using System.Net.Http.Json;
using BmbOrdering.Api.Contracts.Authentication;
using BmbOrdering.Api.Contracts.Customers;
using BmbOrdering.Api.Contracts.Orders;
using BmbOrdering.IntegrationTests.Infrastructure;

namespace BmbOrdering.IntegrationTests;

public sealed class AdminEndpointsTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminEndpointsTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateHttpsClient();
    }

    [Fact]
    public async Task CustomerDirectory_WithCustomerToken_ReturnsForbidden()
    {
        var login = await ApiTestClient.RegisterAndLoginAsync(
            _client,
            $"ordinary.{Guid.NewGuid():N}@example.com");
        ApiTestClient.Authorize(_client, login.AccessToken);

        var response = await _client.GetAsync(
            "/api/v1/admin/customers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CustomerDirectory_WithAdministratorToken_ReturnsCustomers()
    {
        var admin = await RegisterAdministratorAsync();
        ApiTestClient.Authorize(_client, admin.AccessToken);

        var response = await _client.GetAsync(
            "/api/v1/admin/customers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customers = await response.Content
            .ReadFromJsonAsync<CustomerSummaryResponse[]>();

        Assert.Contains(
            Assert.IsType<CustomerSummaryResponse[]>(customers),
            customer => customer.Email ==
                CustomWebApplicationFactory.AdministratorEmail);
    }

    [Fact]
    public async Task CustomerOrders_WithAdministratorToken_ReturnsSelectedCustomersOrders()
    {
        var customer = await ApiTestClient.RegisterAndLoginAsync(
            _client,
            $"managed.{Guid.NewGuid():N}@example.com");
        ApiTestClient.Authorize(_client, customer.AccessToken);
        var createdOrder = await ApiTestClient.CreateOrderAsync(_client);

        var admin = await RegisterAdministratorAsync();
        ApiTestClient.Authorize(_client, admin.AccessToken);

        var response = await _client.GetAsync(
            $"/api/v1/admin/customers/{customer.CustomerId}/orders");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<OrderResponse[]>();
        var order = Assert.Single(
            Assert.IsType<OrderResponse[]>(orders));

        Assert.Equal(createdOrder.Id, order.Id);
        Assert.Equal(customer.CustomerId, order.CustomerId);
    }

    private async Task<LoginCustomerResponse>
        RegisterAdministratorAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginCustomerRequest(
                CustomWebApplicationFactory.AdministratorEmail,
                "StrongPass1"));

        if (loginResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            return await ApiTestClient.RegisterAndLoginAsync(
                _client,
                CustomWebApplicationFactory.AdministratorEmail,
                "Integration Administrator");
        }

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content
            .ReadFromJsonAsync<LoginCustomerResponse>();

        return Assert.IsType<LoginCustomerResponse>(login);
    }
}
