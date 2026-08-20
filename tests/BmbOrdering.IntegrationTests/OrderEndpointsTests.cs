using System.Net;
using System.Net.Http.Json;
using BmbOrdering.Api.Contracts.Orders;
using BmbOrdering.IntegrationTests.Infrastructure;

namespace BmbOrdering.IntegrationTests;

public sealed class OrderEndpointsTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FixedClock _clock;

    public OrderEndpointsTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateHttpsClient();
        _clock = factory.Clock;
    }

    [Fact]
    public async Task CreateAndRetrieveOrder_ForAuthenticatedCustomer_Succeeds()
    {
        var login = await RegisterUniqueCustomerAsync();
        ApiTestClient.Authorize(_client, login.AccessToken);

        var created = await ApiTestClient.CreateOrderAsync(_client);

        var getResponse = await _client.GetAsync(
            $"/api/v1/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var retrieved = await getResponse.Content
            .ReadFromJsonAsync<OrderResponse>();
        var order = Assert.IsType<OrderResponse>(retrieved);

        Assert.Equal(created.Id, order.Id);
        Assert.Equal(login.CustomerId, order.CustomerId);
        Assert.Equal(251.00m, order.TotalAmount);
        Assert.Single(order.Items);
    }

    [Fact]
    public async Task GetOrder_OwnedByAnotherCustomer_ReturnsNotFound()
    {
        var owner = await RegisterUniqueCustomerAsync();
        ApiTestClient.Authorize(_client, owner.AccessToken);
        var order = await ApiTestClient.CreateOrderAsync(_client);

        var otherCustomer = await RegisterUniqueCustomerAsync();
        ApiTestClient.Authorize(_client, otherCustomer.AccessToken);

        var response = await _client.GetAsync(
            $"/api/v1/orders/{order.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteThreeSameDayOrders_BansCustomerForSixHours()
    {
        var login = await RegisterUniqueCustomerAsync();
        ApiTestClient.Authorize(_client, login.AccessToken);

        var orders = new[]
        {
            await ApiTestClient.CreateOrderAsync(_client, "First item"),
            await ApiTestClient.CreateOrderAsync(_client, "Second item"),
            await ApiTestClient.CreateOrderAsync(_client, "Third item")
        };

        DeleteOrderResponse? thirdDeletion = null;

        for (var index = 0; index < orders.Length; index++)
        {
            var response = await _client.DeleteAsync(
                $"/api/v1/orders/{orders[index].Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var deletion = await response.Content
                .ReadFromJsonAsync<DeleteOrderResponse>();
            thirdDeletion = Assert.IsType<DeleteOrderResponse>(deletion);

            Assert.Equal(index + 1, thirdDeletion.QualifyingDeletionCount);
        }

        var finalDeletion =
            Assert.IsType<DeleteOrderResponse>(thirdDeletion);

        Assert.Equal(
            _clock.UtcNow.AddHours(6),
            finalDeletion.BannedUntilUtc);

        var blockedResponse = await _client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest(
                new[]
                {
                    new CreateOrderItemRequest(
                        "Blocked item",
                        1,
                        10m)
                }));

        Assert.Equal(HttpStatusCode.Forbidden, blockedResponse.StatusCode);
    }

    private Task<BmbOrdering.Api.Contracts.Authentication.LoginCustomerResponse>
        RegisterUniqueCustomerAsync()
    {
        return ApiTestClient.RegisterAndLoginAsync(
            _client,
            $"orders.{Guid.NewGuid():N}@example.com");
    }
}
