using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BmbOrdering.Api.Contracts.Authentication;
using BmbOrdering.Api.Contracts.Orders;

namespace BmbOrdering.IntegrationTests.Infrastructure;

internal static class ApiTestClient
{
    private const string Password = "StrongPass1";

    public static async Task<LoginCustomerResponse>
        RegisterAndLoginAsync(
            HttpClient client,
            string email,
            string fullName = "Integration Customer")
    {
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterCustomerRequest(
                fullName,
                email,
                Password,
                Password));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginCustomerRequest(email, Password));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content
            .ReadFromJsonAsync<LoginCustomerResponse>();

        return Assert.IsType<LoginCustomerResponse>(login);
    }

    public static void Authorize(
        HttpClient client,
        string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public static async Task<OrderResponse> CreateOrderAsync(
        HttpClient client,
        string productName = "Integration keyboard")
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest(
                new[]
                {
                    new CreateOrderItemRequest(
                        productName,
                        2,
                        125.50m)
                }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content
            .ReadFromJsonAsync<OrderResponse>();

        return Assert.IsType<OrderResponse>(order);
    }
}
