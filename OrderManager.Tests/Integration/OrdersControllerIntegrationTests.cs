using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderManager.Application.DTOs;

namespace OrderManager.Tests.Integration;

public class OrdersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldReturn201Created()
    {
        var request = new CreateOrderRequest(
            $"PED-{Guid.NewGuid()}",
            new List<OrderItemRequest>
            {
                new("Product A", 2, 50.00m),
                new("Product B", 1, 25.00m)
            });

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.PedidoId.Should().Be(request.PedidoId);
        order.TotalAmount.Should().Be(125.00m);
        order.TaxAmount.Should().Be(37.50m);
        order.Status.Should().Be("Processed");
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateOrder_WithDuplicatePedidoId_ShouldReturn409Conflict()
    {
        var pedidoId = $"PED-{Guid.NewGuid()}";
        var request = new CreateOrderRequest(
            pedidoId,
            new List<OrderItemRequest> { new("Product", 1, 10.00m) });

        await _client.PostAsJsonAsync("/api/orders", request);
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidRequest_ShouldReturn400BadRequest()
    {
        var request = new CreateOrderRequest("", new List<OrderItemRequest>());

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderById_WithExistingOrder_ShouldReturn200Ok()
    {
        var createRequest = new CreateOrderRequest(
            $"PED-{Guid.NewGuid()}",
            new List<OrderItemRequest> { new("Product", 1, 100.00m) });

        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Id.Should().Be(createdOrder.Id);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistingOrder_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderByPedidoId_WithExistingOrder_ShouldReturn200Ok()
    {
        var pedidoId = $"PED-{Guid.NewGuid()}";
        var createRequest = new CreateOrderRequest(
            pedidoId,
            new List<OrderItemRequest> { new("Product", 1, 50.00m) });

        await _client.PostAsJsonAsync("/api/orders", createRequest);

        var response = await _client.GetAsync($"/api/orders/pedido/{pedidoId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.PedidoId.Should().Be(pedidoId);
    }

    [Fact]
    public async Task GetOrderByPedidoId_WithNonExistingOrder_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync("/api/orders/pedido/NON-EXISTING");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturn200Ok()
    {
        var response = await _client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProcessedOrders_ShouldReturn200Ok()
    {
        var response = await _client.GetAsync("/api/orders/processed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
